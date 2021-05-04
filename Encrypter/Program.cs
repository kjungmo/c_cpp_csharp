using System;
using System.Collections.Generic;
using System.Linq;
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
        {
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
        }
    }
}
