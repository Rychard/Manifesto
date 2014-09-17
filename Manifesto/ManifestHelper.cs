using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Manifesto
{
    /// <summary>
    /// Contains various methods used in the creation of <seealso cref="Manifest" /> objects.
    /// </summary>
    public static class ManifestHelper
    {
        /// <summary>
        /// Recursively enumerates the files present in the specified directory.
        /// </summary>
        /// <param name="path">The absolute path to get the directory listing of.</param>
        public static IEnumerable<String> GetFiles(String path)
        {
            var files = Directory.EnumerateFiles(path);
            foreach (var file in files)
            {
                yield return file;
            }

            var directories = Directory.EnumerateDirectories(path);
            foreach (var directory in directories)
            {
                var directoryFiles = GetFiles(directory);
                foreach (var file in directoryFiles)
                {
                    yield return file;
                }
            }
        }

        /// <summary>
        /// Gets the hash of the file using the specified hashing algorithm.
        /// </summary>
        /// <param name="hashAlgorithm">A valid parameter for the <seealso cref="System.Security.Cryptography.HashAlgorithm"/>.<seealso cref="System.Security.Cryptography.HashAlgorithm.Create(string)"/> method.</param>
        /// <param name="filePath">The absolute path to the file.</param>
        /// <param name="bufferSize">Overrides the size of the buffer used when calculating hashes.  The default buffer size is 128KB.</param>
        public static async Task<String> GetHashAsync(String hashAlgorithm, String filePath, long bufferSize = 131072)
        {
            var hashAlgorithmInstance = HashAlgorithm.Create(hashAlgorithm);
            if (hashAlgorithmInstance == null) { return null; }
            if (!File.Exists(filePath)) { return null; }

            long offset = 0;
            FileStream fs = null;

            try
            {
                fs = new FileStream(filePath, FileMode.Open);
            }
            catch (Exception)
            {
                // If an exception is thrown, it's likely that the file is in use.
                return null;
            }

            long filesize = fs.Length;
            //double onePercent = filesize / 100D;
            byte[] buffer = new byte[bufferSize];
            //long percentage = 0;
            var work = await Task.Factory.StartNew(() =>
            {
                while (fs.Read(buffer, 0, buffer.Length) > 0)
                {
                    long length = filesize - offset;
                    int currentLength = buffer.Length;
                    if (buffer.Length > length)
                    {
                        currentLength = (int)length;
                    }
                    offset += hashAlgorithmInstance.TransformBlock(buffer, 0, currentLength, buffer, 0);
                    //long percentComplete = (long)Math.Round((fs.Position / onePercent), 0);
                    //percentage = percentComplete;
                }

                fs.Close();
                hashAlgorithmInstance.TransformFinalBlock(new Byte[0], 0, 0);
                byte[] bHash = hashAlgorithmInstance.Hash;

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bHash.Length; i++)
                {
                    sb.Append(bHash[i].ToString("x2")); // Return as Hex.
                }
                String sHash = sb.ToString();
                return sHash;
            });
            return work;
        }

        /// <summary>
        /// Creates a new manifest entry.
        /// </summary>
        /// <param name="rootDirectory">The directory that the file's path will be stored relative to.</param>
        /// <param name="relativeFilepath">The path to the file, relative to the <paramref name="rootDirectory" /></param>
        /// <param name="remoteLocation">A remote location where the file can be obtained from.</param>
        /// <param name="hashAlgorithm">A valid parameter for the <seealso cref="System.Security.Cryptography.HashAlgorithm"/>.<seealso cref="System.Security.Cryptography.HashAlgorithm.Create(string)"/> method.</param>
        public static ManifestEntry CreateManifestEntry(String rootDirectory, String relativeFilepath, String remoteLocation = null, String hashAlgorithm = null)
        {
            String absolutePath;
            if (String.IsNullOrWhiteSpace(rootDirectory))
            {
                absolutePath = relativeFilepath;
            }
            else
            {
                absolutePath = Path.Combine(rootDirectory, relativeFilepath);
            }
            
            if (!File.Exists(absolutePath)) { return null; }

            FileInfo fi = new FileInfo(absolutePath);
            var size = fi.Length;
            String hash = String.Empty;

            if (hashAlgorithm != null)
            {
                var hashTask = ManifestHelper.GetHashAsync(hashAlgorithm, absolutePath);
                hashTask.Wait();
                hash = hashTask.Result;
            }

            String webLocation = remoteLocation;
            var entry = new ManifestEntry(hash, size, relativeFilepath, webLocation);
            return entry;
        }

        /// <summary>
        /// Creates a new manifest using the specified parameters.
        /// </summary>
        /// <param name="directory">The directory containing the files to include in the manifest.</param>
        /// <param name="hashAlgorithm">A valid parameter for the <seealso cref="System.Security.Cryptography.HashAlgorithm" />.<seealso cref="System.Security.Cryptography.HashAlgorithm.Create(string)" /> method.</param>
        /// <param name="description">A description to be included in the manifest.</param>
        /// <returns></returns>
        public static async Task<Manifest> CreateManifest(String directory, String hashAlgorithm = null, String description = null)
        {
            var work = await Task.Factory.StartNew(() =>
            {
                List<ManifestEntry> entries = new List<ManifestEntry>();
                var files = ManifestHelper.GetFiles(directory);
                foreach (var file in files)
                {
                    var entry = CreateManifestEntry(directory, file.Replace(directory, ""), null, hashAlgorithm);
                    if (entry == null) { continue; }
                    entries.Add(entry);
                }
                Manifest manifest = new Manifest
                {
                    ID = Guid.NewGuid(),
                    Description = description,
                    HashAlgorithm = hashAlgorithm,
                    TimestampUtc = DateTime.UtcNow,
                    Contents = entries,
                };
                return manifest;
            });

            return work;
        }
    }
}
