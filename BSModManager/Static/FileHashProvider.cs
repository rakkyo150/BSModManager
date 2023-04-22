using System;
using System.IO;
using System.Security.Cryptography;

namespace BSModManager.Static
{
    internal static class FileHashProvider
    {
        private static readonly HashAlgorithm hashProvider = new SHA256CryptoServiceProvider();

        // https://mseeeen.msen.jp/compute-hash-string-of-files/
        /// <summary>
        /// Returns the hash string for the file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static string ComputeFileHash(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] bs = hashProvider.ComputeHash(fs);
                return BitConverter.ToString(bs).ToLower().Replace("-", "");
            }
        }
    }
}
