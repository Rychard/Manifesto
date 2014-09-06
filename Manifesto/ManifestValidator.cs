using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Manifesto
{
    /// <summary>
    /// Used to validate the contents of a <seealso cref="Manifest" /> instance.
    /// </summary>
    public class ManifestValidator
    {
        #region Events

        /// <summary>
        /// Occurs when validation is started or resumed.
        /// </summary>
        public event EventHandler ValidationStarted;

        protected virtual void OnValidationStarted()
        {
            EventHandler handler = this.ValidationStarted;
            if (handler != null) { handler(this, EventArgs.Empty); }
        }

        /// <summary>
        /// Occurs when validation is stopped before it has completed.
        /// </summary>
        public event EventHandler ValidationStopped;

        protected virtual void OnValidationStopped()
        {
            EventHandler handler = this.ValidationStopped;
            if (handler != null) { handler(this, EventArgs.Empty); }
        }

        /// <summary>
        /// Occurs when validation completes.
        /// </summary>
        public event EventHandler ValidationCompleted;

        protected virtual void OnValidationCompleted()
        {
            EventHandler handler = this.ValidationCompleted;
            if (handler != null) { handler(this, EventArgs.Empty); }
        }

        /// <summary>
        /// Occurs when the progress of the validation task changes.
        /// </summary>
        public event EventHandler<ManifestValidatorProgressChangedEventArgs> ValidationProgressChanged;

        protected virtual void OnValidationProgressChanged()
        {
            EventHandler<ManifestValidatorProgressChangedEventArgs> handler = this.ValidationProgressChanged;
            if (handler != null)
            {
                lock (_locker)
                {
                    var processed = _processed.Count;
                    var remaining = _remaining.Count;
                    var passed = _results.Count(obj => obj.IsValidHash);
                    var failed = _results.Count(obj => !obj.IsValidHash);
                    var e = new ManifestValidatorProgressChangedEventArgs(processed, remaining, passed, failed);
                    handler(this, e);
                }
            }
        }

        #endregion

        private readonly Object _locker = new Object();
        private readonly Manifest _manifest;
        private readonly String _localDirectory;
        private readonly List<ManifestEntry> _processed;
        private readonly Queue<ManifestEntry> _remaining;
        private readonly List<ManifestValidatorResult> _results;
        private Boolean _running;
        private Boolean _complete;

        /// <summary>
        /// Gets the path of the directory that will be used to locate files with relative paths.
        /// </summary>
        public String LocalDirectory
        {
            get { return _localDirectory; }
        }

        /// <summary>
        /// Gets a value indicating whether validation is currently underway.
        /// </summary>
        /// <value>
        /// <c>true</c> if validation is underway; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsRunning
        {
            get { return _running; }
        }

        /// <summary>
        /// Gets a value indicating whether validation has completed.
        /// </summary>
        /// <value>
        /// <c>true</c> if validation has completed; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsCompleted
        {
            get { return _complete; }
        }

        /// <summary>
        /// Gets the progress of the validation process as a value between 0 and 1.
        /// </summary>
        public Double PercentComplete
        {
            get
            {
                lock (_locker)
                {
                    var total = _processed.Count + _remaining.Count;
                    if (total == 0) { return 0D;}
                    var percentageComplete = (_processed.Count / (double)total);
                    return percentageComplete;    
                }
            }
        }

        /// <summary>
        /// Gets the results of the validation.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Validation results are unavailable until the process has completed.</exception>
        public ManifestValidatorResult[] ValidationResults
        {
            get
            {
                if (!_complete)
                {
                    throw new InvalidOperationException("Validation results are unavailable until the process has completed.");
                }
                return _results.ToArray();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestValidator"/> class.
        /// </summary>
        /// <param name="manifest">The manifest instance to be validated.</param>
        /// <param name="localDirectory">The directory that will be used to locate files with relative paths.</param>
        public ManifestValidator(Manifest manifest, String localDirectory)
        {
            _manifest = manifest;
            _localDirectory = localDirectory;
            _running = false;
            _complete = false;

            var contents = manifest.Contents;

            _results = new List<ManifestValidatorResult>();
            _processed = new List<ManifestEntry>(contents.Count);
            _remaining = new Queue<ManifestEntry>(contents.Count);
        }

        /// <summary>
        /// Starts or resumes the validation process.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The validation process is currently running.</exception>
        public void Start()
        {
            if (_running)
            {
                throw new InvalidOperationException("The validation process is currently running.");
            }

            // If either of the collections have been populated, this is a resume, and not a fresh start.
            // In that case, we don't need to populate our collections.
            if (_remaining.Count > 0 || _processed.Count > 0)
            {
                _running = true;
                _complete = false;
            }
            else
            {
                var contents = _manifest.Contents;
                foreach (var entry in contents)
                {
                    _remaining.Enqueue(entry);
                }    
            }

            Task.Factory.StartNew(() =>
            {
                _running = true;
                _complete = false;
                OnValidationStarted();
                while (_remaining.Count > 0)
                {
                    // If the process has been cancelled, do not move to the next item.
                    if (!_running)
                    {
                        OnValidationStopped();
                        return;
                    }
                    lock (_locker)
                    {
                        this.ProcessNext();    
                    }
                }
                
                _complete = true;
                _running = false; 
                OnValidationCompleted();
            });
        }

        /// <summary>
        /// Stops a currently underway validation process.  The process can be resumed by calling the <see cref="Start()"/> method.
        /// </summary>
        public void Stop()
        {
            if (!_running) { return; }
            _running = false;
        }

        /// <summary>
        /// Processes the next <see cref="ManifestEntry"/> in the queue.
        /// </summary>
        private void ProcessNext()
        {
            var entry = _remaining.Peek();
            Boolean validHash = false;
            String path = null;

            var localPath = Path.Combine(_localDirectory, entry.File);
            if (File.Exists(localPath))
            {
                path = localPath;
                var hashTask = ManifestHelper.GetHashAsync(_manifest.HashAlgorithm, localPath);
                hashTask.Wait();
                var hash = hashTask.Result;
                validHash = (entry.Hash == hash);
            }

            // Add the results to our collection.
            _results.Add(new ManifestValidatorResult(entry, validHash, path));

            // Remove the entry from the queue, and add it to the list of processed entries.
            _processed.Add(_remaining.Dequeue());

            // Raise the progress changed event.
            OnValidationProgressChanged();
        }
    }
}
