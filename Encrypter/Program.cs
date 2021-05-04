using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace Encrypter
{
    class Program
    {
        static void Main(string[] args)
        public static void Main()
        {
<<<<<<< HEAD
            string OriStr = "7085C231FBC4";
            string EncStr = Encrypt(OriStr);
            string DecStr = Decrypt(EncStr);

            Console.WriteLine("원본 데이터==========================");
            Console.WriteLine(OriStr);

            Console.WriteLine("암호화 데이터=========================");
            Console.WriteLine(EncStr);

            Console.WriteLine("복호화 데이터=========================");
            Console.WriteLine(DecStr);

            Console.ReadKey();
        }

        //암호화 키.  8글자로 이루어짐. 
        static byte[] Skey = ASCIIEncoding.ASCII.GetBytes("CogAplex");

        static string Encrypt(string p_data)
        {
            // 암호화 알고리즘중 RC2 암호화를 하려면 RC를 
            // DES알고리즘을 사용하려면 DESCryptoServiceProvider 객체를 선언한다. 
            //RC2 rc2 = new RC2CryptoServiceProvider(); 
            DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider();

            rc2.Mode = CipherMode.ECB;
            rc2.Padding = PaddingMode.PKCS7;

            // 대칭키 배치 
            rc2.Key = Skey;
            rc2.IV = Skey;

            // 암호화는 스트림(바이트 배열)을 
            // 대칭키에 의존하여 암호화 하기때문에 먼저 메모리 스트림을 생성한다. 
            MemoryStream ms = new MemoryStream();

            //만들어진 메모리 스트림을 이용해서 암호화 스트림 생성 
            CryptoStream cryStream =
                              new CryptoStream(ms, rc2.CreateEncryptor(), CryptoStreamMode.Write);

            // 데이터를 바이트 배열로 변경 
            byte[] data = Encoding.UTF8.GetBytes(p_data.ToCharArray());

            // 암호화 스트림에 데이터 씀 
            cryStream.Write(data, 0, data.Length);
            cryStream.FlushFinalBlock();

            // 암호화 완료 (string으로 컨버팅해서 반환) 
            return Convert.ToBase64String(ms.ToArray());
        }

        static string Decrypt(string p_data)
        {

            // 암호화 알고리즘중 RC2 암호화를 하려면 RC를 
            // DES알고리즘을 사용하려면 DESCryptoServiceProvider 객체를 선언한다. 
            //RC2 rc2 = new RC2CryptoServiceProvider(); 
            DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider();

            rc2.Mode = CipherMode.ECB;
            rc2.Padding = PaddingMode.PKCS7;

            // 대칭키 배치 
            rc2.Key = Skey;
            rc2.IV = Skey;

            // 암호화는 스트림(바이트 배열)을 
            // 대칭키에 의존하여 암호화 하기때문에 먼저 메모리 스트림을 생성한다. 
            MemoryStream ms = new MemoryStream();

            //만들어진 메모리 스트림을 이용해서 암호화 스트림 생성 
            CryptoStream cryStream =
                              new CryptoStream(ms, rc2.CreateDecryptor(), CryptoStreamMode.Write);

            //데이터를 바이트배열로 변경한다. 
            byte[] data = Convert.FromBase64String(p_data);

            //변경된 바이트배열을 암호화 한다. 
            cryStream.Write(data, 0, data.Length);
            cryStream.FlushFinalBlock();

            //암호화 한 데이터를 스트링으로 변환해서 리턴 
            return Encoding.UTF8.GetString(ms.GetBuffer());
=======
            try
            {

                string original = "Here is some data to encrypt!";

                // Create a new instance of the Rijndael
                // class.  This generates a new key and initialization
                // vector (IV).
                using (Rijndael myRijndael = Rijndael.Create())
                {
                    // Encrypt the string to an array of bytes.
                    byte[] encrypted = EncryptStringToBytes(original, myRijndael.Key, myRijndael.IV);

                    // Decrypt the bytes to a string.
                    string roundtrip = DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);

                    //Display the original data and the decrypted data.
                    Console.WriteLine("Original:   {0}", original);
                    Console.WriteLine("Encrypted string : ");
                    for (int i = 0; i < encrypted.Length; i++)
                        Console.Write(encrypted[i]);
                    Console.WriteLine("\nRound Trip: {0}", roundtrip);
                    Console.ReadKey();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }
        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an Rijndael object
            // with the specified key and IV.
            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0) 
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Rijndael object
            // with the specified key and IV.
            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
>>>>>>> 9ecd2e19b8dbc4f532c0da801906b17bd0bea7ea
        }
    }
}
