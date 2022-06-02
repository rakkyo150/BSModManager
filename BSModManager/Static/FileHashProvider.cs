using System;
using System.IO;
using System.Security.Cryptography;

namespace BSModManager.Static
{
    internal static class FileHashProvider
    {
        static readonly HashAlgorithm hashProvider = new SHA256CryptoServiceProvider();

        // https://mseeeen.msen.jp/compute-hash-string-of-files/
        /// <summary>
        /// Returns the hash string for the file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static string ComputeFileHash(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var bs = hashProvider.ComputeHash(fs);
                return BitConverter.ToString(bs).ToLower().Replace("-", "");
            }
        }
    }
}
