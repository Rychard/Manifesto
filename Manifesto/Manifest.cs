using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Manifesto
{
    /// <summary>
    /// Represents a collection of files.
    /// </summary>
    public class Manifest
    {
        /// <summary>
        /// Gets or sets the identifier of the instance.
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Gets or sets a description of this instance.
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Gets or sets the hash algorithm that was used to generate the hashes for the files in the instance.
        /// </summary>
        public String HashAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the <seealso cref="DateTime" /> in UTC, that the instance was created or modified.
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Gets or sets the contents of the instance.
        /// </summary>
        [XmlArray]
        [XmlArrayItem(ElementName="Entry", Type = typeof(ManifestEntry))] 
        public List<ManifestEntry> Contents { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Manifest"/> class.
        /// </summary>
        public Manifest()
        {
            this.Contents = new List<ManifestEntry>();
            this.TimestampUtc = DateTime.UtcNow;
            this.ID = Guid.NewGuid();
        }

        /// <summary>
        /// Adds the collection of files to the contents of the current instance.
        /// </summary>
        /// <param name="files">A collection of absolute filepaths to be added.</param>
        public void AddFiles(IEnumerable<String> files)
        {
            List<ManifestEntry> entries = new List<ManifestEntry>();
            foreach (var file in files)
            {
                var entry = ManifestHelper.CreateManifestEntry(null, file, null, this.HashAlgorithm);
                if (entry != null)
                {
                    entries.Add(entry);    
                }
            }
            Contents.AddRange(entries);
        }

        /// <summary>
        /// Calculates the hashes for all manifest entries in this instance.
        /// </summary>
        /// <param name="rootDirectory">The directory that filepaths are relative to.</param>
        /// <param name="skipFilesLargerThan">Skips files larger than this size, in bytes.</param>
        public void CalculateHashes(String rootDirectory, long skipFilesLargerThan = -1)
        {
            var result = Parallel.ForEach(this.Contents, entry =>
            {
                String absolutePath = Path.Combine(rootDirectory, entry.File);
                if (skipFilesLargerThan > 0)
                {
                    FileInfo fi = new FileInfo(absolutePath);
                    if (fi.Length > skipFilesLargerThan) { return; }
                }
                var task = ManifestHelper.GetHashAsync(this.HashAlgorithm, absolutePath);
                task.Wait();
                entry.Hash = task.Result;
            });
            while (!result.IsCompleted)
            {
                System.Threading.Thread.Sleep(250);
            }
        }

        /// <summary>
        /// If the specified array contains only entries with absolute paths, the entries will be searched to find a common ancestor.  If found, the paths of the entries are modified to be relative to the common path.
        /// </summary>
        /// <param name="entries">The entries.</param>
        /// <param name="commonAncestor">The common ancestor.</param>
        /// <returns>
        /// An array of manifest entries with the common ancestor removed.
        /// </returns>
        public static ManifestEntry[] FindCommonAncestor(IEnumerable<ManifestEntry> entries, out String commonAncestor)
        {
            
            var tempEntries = entries.ToList();
            commonAncestor = "";
            if (tempEntries.Count > 0)
            {
                var files = tempEntries.Select(obj => obj.File).ToList();
                var matchingChars =
                        from len in Enumerable.Range(0, files.Min(s => s.Length)).Reverse()
                        let possibleMatch = files.First().Substring(0, len)
                        where files.All(f => f.StartsWith(possibleMatch))
                        select possibleMatch;
                commonAncestor = Path.GetDirectoryName(matchingChars.First()) + Path.DirectorySeparatorChar;                
            }
            return SetRelativePath(tempEntries, commonAncestor).ToArray();
        }

        /// <summary>
        /// Sets the path of all entries relative to the specified root directory.
        /// </summary>
        /// <param name="entries">A collection of entries to modify.</param>
        /// <param name="rootDirectory">The directory path that all filepaths will be stored relative to.</param>
        /// <returns>
        /// An array of manifest entries with paths modified to be relative to the specified root directory.
        /// </returns>
        /// <exception cref="System.ApplicationException">Not all entries are children of the specified root directory!</exception>
        public static ManifestEntry[] SetRelativePath(IEnumerable<ManifestEntry> entries, String rootDirectory)
        {
            if (String.IsNullOrWhiteSpace(rootDirectory)) { return entries.ToArray(); }
            var tempEntries = entries.ToList();
            var commonAncestor = Path.GetDirectoryName(rootDirectory) + Path.DirectorySeparatorChar;
            foreach (var manifestEntry in tempEntries)
            {
                if (manifestEntry.File.Contains(commonAncestor))
                {
                    manifestEntry.File = manifestEntry.File.Replace(commonAncestor, "");
                }
                else
                {
                    throw new ApplicationException("Not all entries are children of the specified root directory!");
                }
            }
            return tempEntries.ToArray();
        }
    }
}
