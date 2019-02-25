using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Linq;

namespace gView.Framework.system
{
    internal class FileReadWrite
    {
        // Key for TripleDES encryption
        public static byte[] key = { 21, 10, 64, 10, 100, 40, 200, 4,
                    21, 54, 65, 246, 5, 62, 1, 54,
                    54, 6, 8, 9, 65, 4, 65, 9};
                    
        private static byte[] iv = { 0, 0, 0, 0, 0, 0, 0, 0 };

        public static string ReadFile(string FilePath)
        {
            FileInfo fi = new FileInfo(FilePath);
            if (fi.Exists == false)
                return string.Empty;

            FileStream fin = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            TripleDES tdes = new TripleDESCryptoServiceProvider();
            CryptoStream cs = new CryptoStream(fin, tdes.CreateDecryptor(key, iv), CryptoStreamMode.Read);

            StringBuilder SB = new StringBuilder();
            int ch;
            for (int i = 0; i < fin.Length; i++)
            {
                ch = cs.ReadByte();
                if (ch == 0)
                    break;
                SB.Append(Convert.ToChar(ch));
            }

            cs.Close();
            fin.Close();
            return SB.ToString();
        }

        public static void WriteFile(string FilePath, string Data)
        {
            FileStream fout = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write);
            TripleDES tdes = new TripleDESCryptoServiceProvider();
            CryptoStream cs = new CryptoStream(fout, tdes.CreateEncryptor(key, iv), CryptoStreamMode.Write);
            
            byte[] d = Encoding.ASCII.GetBytes(Data);
            cs.Write(d, 0, d.Length);
            cs.WriteByte(0);

            cs.Close();
            fout.Close();
        }
    }

    public class Crypto
    {
        public enum ResultType
        {
            Base64,
            Hex
        }

        //// Key for TripleDES encryption
        //public static byte[] _key = { 21, 10, 64, 10, 100, 40, 200, 4,
        //            21, 54, 65, 246, 5, 62, 1, 54,
        //            54, 6, 8, 9, 65, 4, 65, 9};

        //private static byte[] _iv = { 0, 0, 0, 0, 0, 0, 0, 0 };

        //static public string Write(string original)
        //{
        //    return Write(original, _key, _iv);
        //}
        //static public string Write(string str, byte []rgbKey,byte [] rgbIV)
        //{
        //    //TripleDES tdes = new TripleDESCryptoServiceProvider();
        //    //Rijndael RijndaelAlg = Rijndael.Create();

        //    //MemoryStream ms = new MemoryStream();
        //    //CryptoStream cs = new CryptoStream(ms, RijndaelAlg.CreateEncryptor(RijndaelAlg.Key, RijndaelAlg.IV), CryptoStreamMode.Write);

        //    //byte[] d = Encoding.ASCII.GetBytes(str);
        //    //cs.Write(d, 0, d.Length);
        //    //cs.WriteByte(0);
        //    //cs.Flush();
            
        //    //byte[] buffer = new byte[ms.Length];
        //    //ms.Position = 0;
        //    //ms.Read(buffer, 0, buffer.Length);

        //    ////cs.Close();
        //    ////ms.Close();
        //    //ms.Position = 0;
        //    //cs = new CryptoStream(ms, RijndaelAlg.CreateDecryptor(RijndaelAlg.Key, RijndaelAlg.IV), CryptoStreamMode.Read);
        //    //cs.ReadByte();

        //    //return Convert.ToBase64String(buffer);

        //    return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        //}

        //static public string Read(string str)
        //{
        //    return Read(str, _key, _iv);
        //}
        //static public string Read(string str, byte[] rgbKey, byte[] rgbIV)
        //{
            //MemoryStream ms = new MemoryStream();
            //byte[] data = Convert.FromBase64String(str);
            //ms.Write(data, 0, data.Length);
            //ms.Position = 0;

            //TripleDES tdes = new TripleDESCryptoServiceProvider();
            //CryptoStream cs = new CryptoStream(ms, tdes.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Read);

            //StringBuilder SB = new StringBuilder();
            //int ch;
            //for (int i = 0; i < ms.Length; i++)
            //{
            //    ch = cs.ReadByte();
            //    if (ch == 0)
            //        break;
            //    SB.Append(Convert.ToChar(ch));
            //}

            //cs.Close();
            //ms.Close();

            //return ms.ToString();

            //return Encoding.ASCII.GetString(Convert.FromBase64String(str));
        //}

        // Encrypt a byte array into a byte array using a key and an IV 
        public static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {

            // Create a MemoryStream that is going to accept the encrypted bytes 

            MemoryStream ms = new MemoryStream();



            // Create a symmetric algorithm. 

            // We are going to use Rijndael because it is strong and available on all platforms. 

            // You can use other algorithms, to do so substitute the next line with something like 

            //                      TripleDES alg = TripleDES.Create(); 

            Rijndael alg = Rijndael.Create();



            // Now set the key and the IV. 

            // We need the IV (Initialization Vector) because the algorithm is operating in its default 

            // mode called CBC (Cipher Block Chaining). The IV is XORed with the first block (8 byte) 

            // of the data before it is encrypted, and then each encrypted block is XORed with the 

            // following block of plaintext. This is done to make encryption more secure. 

            // There is also a mode called ECB which does not need an IV, but it is much less secure. 

            alg.Key = Key;

            alg.IV = IV;



            // Create a CryptoStream through which we are going to be pumping our data. 

            // CryptoStreamMode.Write means that we are going to be writing data to the stream 

            // and the output will be written in the MemoryStream we have provided. 

            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);



            // Write the data and make it do the encryption 

            cs.Write(clearData, 0, clearData.Length);



            // Close the crypto stream (or do FlushFinalBlock). 

            // This will tell it that we have done our encryption and there is no more data coming in, 

            // and it is now a good time to apply the padding and finalize the encryption process. 

            cs.Close();



            // Now get the encrypted data from the MemoryStream. 

            // Some people make a mistake of using GetBuffer() here, which is not the right way. 

            byte[] encryptedData = ms.ToArray();



            return encryptedData;

        }

        // Encrypt a string into a string using a password 

        //    Uses Encrypt(byte[], byte[], byte[]) 
        public static string Encrypt(string clearText, string Password, ResultType resultType = ResultType.Base64)
        {
            if (String.IsNullOrEmpty(clearText))
                return String.Empty;


            byte[] clearBytes = global::System.Text.Encoding.Unicode.GetBytes(clearText);

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4e, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

            byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            switch (resultType)
            {
                case ResultType.Hex:
                    return "0x" + string.Concat(encryptedData.Select(b => b.ToString("X2")));
                default: // base64
                    return Convert.ToBase64String(encryptedData);
            }
        }

        // Encrypt bytes into bytes using a password 

        //    Uses Encrypt(byte[], byte[], byte[]) 
        public static byte[] Encrypt(byte[] clearData, string Password)
        {

            // We need to turn the password into Key and IV. 

            // We are using salt to make it harder to guess our key using a dictionary attack - 

            // trying to guess a password by enumerating all possible words. 

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4e, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });



            // Now get the key/IV and do the encryption using the function that accepts byte arrays. 

            // Using PasswordDeriveBytes object we are first getting 32 bytes for the Key 

            // (the default Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV. 

            // IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael. 

            // If you are using DES/TripleDES/RC2 the block size is 8 bytes and so should be the IV size. 

            // You can also read KeySize/BlockSize properties off the algorithm to find out the sizes. 

            return Encrypt(clearData, pdb.GetBytes(32), pdb.GetBytes(16));



        }

        // Encrypt a file into another file using a password 
        public static void Encrypt(string fileIn, string fileOut, string Password)
        {

            // First we are going to open the file streams 

            FileStream fsIn = new FileStream(fileIn, FileMode.Open, FileAccess.Read);

            FileStream fsOut = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);



            // Then we are going to derive a Key and an IV from the Password and create an algorithm 

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4e, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });



            Rijndael alg = Rijndael.Create();



            alg.Key = pdb.GetBytes(32);

            alg.IV = pdb.GetBytes(16);



            // Now create a crypto stream through which we are going to be pumping data. 

            // Our fileOut is going to be receiving the encrypted bytes. 

            CryptoStream cs = new CryptoStream(fsOut, alg.CreateEncryptor(), CryptoStreamMode.Write);



            // Now will will initialize a buffer and will be processing the input file in chunks. 

            // This is done to avoid reading the whole file (which can be huge) into memory. 

            int bufferLen = 4096;

            byte[] buffer = new byte[bufferLen];

            int bytesRead;



            do
            {

                // read a chunk of data from the input file 

                bytesRead = fsIn.Read(buffer, 0, bufferLen);



                // encrypt it 

                cs.Write(buffer, 0, bytesRead);



            } while (bytesRead != 0);



            // close everything 

            cs.Close(); // this will also close the unrelying fsOut stream 

            fsIn.Close();

        }

        // Decrypt a byte array into a byte array using a key and an IV 
        public static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {

            // Create a MemoryStream that is going to accept the decrypted bytes 

            MemoryStream ms = new MemoryStream();



            // Create a symmetric algorithm. 

            // We are going to use Rijndael because it is strong and available on all platforms. 

            // You can use other algorithms, to do so substitute the next line with something like 

            //                      TripleDES alg = TripleDES.Create(); 

            Rijndael alg = Rijndael.Create();



            // Now set the key and the IV. 

            // We need the IV (Initialization Vector) because the algorithm is operating in its default 

            // mode called CBC (Cipher Block Chaining). The IV is XORed with the first block (8 byte) 

            // of the data after it is decrypted, and then each decrypted block is XORed with the previous 

            // cipher block. This is done to make encryption more secure. 

            // There is also a mode called ECB which does not need an IV, but it is much less secure. 

            alg.Key = Key;

            alg.IV = IV;



            // Create a CryptoStream through which we are going to be pumping our data. 

            // CryptoStreamMode.Write means that we are going to be writing data to the stream 

            // and the output will be written in the MemoryStream we have provided. 

            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);



            // Write the data and make it do the decryption 

            cs.Write(cipherData, 0, cipherData.Length);



            // Close the crypto stream (or do FlushFinalBlock). 

            // This will tell it that we have done our decryption and there is no more data coming in, 

            // and it is now a good time to remove the padding and finalize the decryption process. 

            cs.Close();



            // Now get the decrypted data from the MemoryStream. 

            // Some people make a mistake of using GetBuffer() here, which is not the right way. 

            byte[] decryptedData = ms.ToArray();



            return decryptedData;

        }

        // Decrypt a string into a string using a password 

        //    Uses Decrypt(byte[], byte[], byte[]) 
        public static string Decrypt(string cipherText, string Password)
        {
            if (String.IsNullOrEmpty(cipherText))
                return String.Empty;

            // First we need to turn the input string into a byte array. 

            // We presume that Base64 encoding was used 

            byte[] cipherBytes = null;
            if (IsHexString(cipherText))
            {
                cipherBytes = StringToByteArray(cipherText);
            }
            else
            {
                cipherBytes = Convert.FromBase64String(cipherText);
            }


            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4e, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });


            byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            return global::System.Text.Encoding.Unicode.GetString(decryptedData);
        }

        // Decrypt bytes into bytes using a password 

        //    Uses Decrypt(byte[], byte[], byte[]) 
        public static byte[] Decrypt(byte[] cipherData, string Password)
        {

            // We need to turn the password into Key and IV. 

            // We are using salt to make it harder to guess our key using a dictionary attack - 

            // trying to guess a password by enumerating all possible words. 

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4e, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });



            // Now get the key/IV and do the Decryption using the function that accepts byte arrays. 

            // Using PasswordDeriveBytes object we are first getting 32 bytes for the Key 

            // (the default Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV. 

            // IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael. 

            // If you are using DES/TripleDES/RC2 the block size is 8 bytes and so should be the IV size. 

            // You can also read KeySize/BlockSize properties off the algorithm to find out the sizes. 

            return Decrypt(cipherData, pdb.GetBytes(32), pdb.GetBytes(16));



        }

        // Decrypt a file into another file using a password 
        public static void Decrypt(string fileIn, string fileOut, string Password)
        {

            // First we are going to open the file streams 

            FileStream fsIn = new FileStream(fileIn, FileMode.Open, FileAccess.Read);

            FileStream fsOut = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);



            // Then we are going to derive a Key and an IV from the Password and create an algorithm 

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4e, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });



            Rijndael alg = Rijndael.Create();



            alg.Key = pdb.GetBytes(32);

            alg.IV = pdb.GetBytes(16);



            // Now create a crypto stream through which we are going to be pumping data. 

            // Our fileOut is going to be receiving the Decrypted bytes. 

            CryptoStream cs = new CryptoStream(fsOut, alg.CreateDecryptor(), CryptoStreamMode.Write);



            // Now will will initialize a buffer and will be processing the input file in chunks. 

            // This is done to avoid reading the whole file (which can be huge) into memory. 

            int bufferLen = 4096;

            byte[] buffer = new byte[bufferLen];

            int bytesRead;



            do
            {

                // read a chunk of data from the input file 

                bytesRead = fsIn.Read(buffer, 0, bufferLen);



                // Decrypt it 

                cs.Write(buffer, 0, bytesRead);



            } while (bytesRead != 0);



            // close everything 

            cs.Close(); // this will also close the unrelying fsOut stream 

            fsIn.Close();

        }


        public static string Hash64(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            return Convert.ToBase64String(passwordBytes);
        }
        // 

        // Testing function 

        //    I am sure you will be able to figure out what it does! 

        // 

        //public static void Main(string[] args)
        //{

        //    if (args.Length == 0)
        //    {

        //        string plainText = "This is some plain text";

        //        string Password = "Password";

        //        Console.WriteLine("Plain text: \"" + plainText + "\", Password: \"" + Password + "\"");

        //        string cipherText = Encrypt(plainText, Password);

        //        Console.WriteLine("Encrypted text: " + cipherText);

        //        string decryptedText = Decrypt(cipherText, Password);

        //        Console.WriteLine("Decrypted: " + decryptedText);

        //    }
        //    else
        //    {

        //        Encrypt(args[0], args[0] + ".encrypted", "Password");

        //        Decrypt(args[0] + ".encrypted", args[0] + ".decrypted", "Password");

        //    }

        //    Console.WriteLine("Done.");

        //} 

        #region Helper

        static private byte[] StringToByteArray(String hex)
        {
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2, hex.Length - 2);

            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        static private bool IsHexString(string hex)
        {
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2, hex.Length - 2);

            bool isHex;
            foreach (var c in hex)
            {
                isHex = ((c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F'));

                if (!isHex)
                    return false;
            }
            return true;
        }

        #endregion
    }
}
