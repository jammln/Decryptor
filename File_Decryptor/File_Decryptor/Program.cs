using System;
using System.IO;
using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        // Decryption directory path
        string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // Get all encrypted files in the directory
        string[] encryptedFilePaths = Directory.GetFiles(directoryPath, "*.!Locked", SearchOption.AllDirectories);

        // Read the encryption key and initialization vector from files
        string keyFilePath = Path.Combine(directoryPath, "key.bin");
        string ivFilePath = Path.Combine(directoryPath, "iv.bin");
        byte[] key = File.ReadAllBytes(keyFilePath);
        byte[] iv = File.ReadAllBytes(ivFilePath);

        // Create an instance of the AES decryption algorithm
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            // Decrypt all encrypted files in the directory
            foreach (string encryptedFilePath in encryptedFilePaths)
            {
                // Separate file name and extension
                string fileName = Path.GetFileNameWithoutExtension(encryptedFilePath).Replace(".!Locked", "");
                string fileExtension = Path.GetExtension(fileName);

                // Create decrypted file path
                string decryptedFilePath = Path.Combine(Path.GetDirectoryName(encryptedFilePath), fileName);

                // Create file streams
                using (FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (FileStream decryptedFileStream = new FileStream(decryptedFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        // Create decryption stream
                        using (ICryptoTransform decryptor = aes.CreateDecryptor())
                        {
                            // Decrypt the file contents and write to the decrypted file stream
                            using (CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, decryptor, CryptoStreamMode.Read))
                            {
                                cryptoStream.CopyTo(decryptedFileStream);
                            }
                        }
                    }
                }

                // Delete the encrypted file
                File.Delete(encryptedFilePath);
            }
        }

        // Delete the key and iv files
        File.Delete(keyFilePath);
        File.Delete(ivFilePath);
    }
}
