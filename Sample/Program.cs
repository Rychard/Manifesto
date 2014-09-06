using System;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using Manifesto;

namespace Sample
{
    class Program
    {
        private static Double lastProgressReportPercentage = 0D;

        static void Main(string[] args)
        {
            // TODO: Read arguments in to generate manifests without requiring user-input.

            var pathDefault = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            Console.WriteLine("Please enter a directory path to generate a manifest. (Press enter for default)");
            Console.WriteLine("Default Value: \"{0}\"", pathDefault);
            Console.WriteLine();
            Console.Write("Path: ");
            var path = Console.ReadLine();
            if (String.IsNullOrWhiteSpace(path))
            {
                path = pathDefault;
            }
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
            {
                path += Path.DirectorySeparatorChar;
            }

            if (!Directory.Exists(path))
            {
                Console.WriteLine("The entered directory was invalid or does not exist.  Exiting...");
                return;
            }
            
            Console.WriteLine();
            Console.WriteLine("Enter the name of a Hashing Algorithm  (Press enter for default.)");
            Console.WriteLine("Default Value: \"\" (Skip Hashing)");
            Console.WriteLine("Examples: MD5, SHA1, SHA256, SHA384, SHA512");
            Console.WriteLine("A full list of valid values can be found here:");
            Console.WriteLine(" http://msdn.microsoft.com/en-us/library/wet69s13(v=vs.110).aspx ");
            Console.WriteLine();
            Console.Write("Hashing Algorithm: ");
            var hashingAlgorithm = Console.ReadLine();
            Console.WriteLine();

	        var m = ManifestHelper.CreateManifest(path, hashingAlgorithm);
            m.Wait();

            var manifest = m.Result;

            String desktop = Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            String outputPath = String.Format(@"{0}\Manifest_{1}.xml", desktop, DateTime.UtcNow.ToFileTimeUtc());
            Boolean success = SerializationHelper.TryXmlSerialize(manifest, outputPath);
            Console.WriteLine("Manifest Serialized {0}", success ? "Successfully" : "Unsuccessfully");

            Console.WriteLine("Press any key to load the manifest file for verification...");
            Console.ReadKey();
            Console.WriteLine();

            Boolean deserializeSuccess = SerializationHelper.TryXmlDeserialize(outputPath, out manifest);
            Console.WriteLine("Manifest Deserialized {0}", deserializeSuccess ? "Successfully" : "Unsuccessfully");

            if (deserializeSuccess)
            {
                var mv = new ManifestValidator(manifest, path);
                mv.ValidationStarted += delegate { Console.WriteLine("Validation Started @ {0:P2}.", mv.PercentComplete); };
                mv.ValidationStopped += delegate { Console.WriteLine("Validation Stopped @ {0:P2}.", mv.PercentComplete); };
                mv.ValidationProgressChanged += OnValidationProgressChanged;
                mv.ValidationCompleted += OnValidationCompleted;
                mv.Start();
                Console.WriteLine("Press any key to pause/resume the validation process.");
                while (!mv.IsCompleted)
                {
                    Console.ReadKey(true);
                    if (!mv.IsCompleted)
                    {
                        if (mv.IsRunning)
                        {
                            mv.Stop();
                        }
                        else
                        {
                            mv.Start();
                        }
                    }
                }

                // TODO: Output validation results.
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void OnValidationProgressChanged(object sender, ManifestValidatorProgressChangedEventArgs e)
        {
            if (e.PercentageComplete - 0.1D > lastProgressReportPercentage)
            {
                Console.WriteLine("{0:P2}", e.PercentageComplete);
                lastProgressReportPercentage = e.PercentageComplete;
            }
        }

        private static void OnValidationCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("Validation is finished.");

            if (sender is ManifestValidator)
            {
                var validator = sender as ManifestValidator;
                var results = validator.ValidationResults;
                foreach (var result in results)
                {
                    if (!result.IsValidHash)
                    {
                        Console.WriteLine("{0} - Hash Mismatch!", result.ManifestEntry.File);
                    }
                    if (String.IsNullOrWhiteSpace(result.FileLocation))
                    {
                        Console.WriteLine("{0} - File Not Found!", result.ManifestEntry.File);
                    }
                }
            }
        }
    }
}
