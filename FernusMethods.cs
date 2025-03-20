/*
 * Author: Zemi @ GitHub/Ucaninek
 * Date: 2024
 */


using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System.Text;

namespace FernusSWFExporter
{
    public class FernusMethods
    {
        public static string Fd1(string _data, string _key, bool _s = true)
        {
            byte[] key;
            string data;
            if (_s) data = Fd2(_data, _key);
            else data = _data;
            byte[] fileBytes = Convert.FromBase64String(data);
            key = Encoding.UTF8.GetBytes(_key);//StringToHexByteArray(_key);
            return Decrypt(fileBytes, key);
        }

        public static string Decrypt(byte[] data, byte[] key)
        {
            PaddedBufferedBlockCipher cipher = new(new BlowfishEngine(), new Pkcs7Padding());
            KeyParameter keyParam = new(key);
            cipher.Init(false, keyParam);

            byte[] decryptedBytes = new byte[cipher.GetOutputSize(data.Length)];
            int decryptedLength = cipher.ProcessBytes(data, 0, data.Length, decryptedBytes, 0);
            decryptedLength += cipher.DoFinal(decryptedBytes, decryptedLength);

            return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedLength);
        }

        public static byte[] XOR(byte[] data, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return result;
        }

        public static string Fd2(string input, string key)
        {
            byte[] inputBuffer = Convert.FromBase64String(new string(input.ToCharArray().Reverse().ToArray()));
            byte[] outBytes = XOR(inputBuffer, key);

            return Encoding.UTF8.GetString(outBytes);
        }


        public static byte[] Decrypte(byte[] fBytes, dynamic fCode)
        {
            byte[] bytes = fBytes;
            int n1 = fCode.f1;
            int n2 = fCode.f2;
            int n3 = fCode.f3;

            SeparateBytes(bytes, 10000, 11000, n1, n2);
            SeparateBytes(bytes, 5000, 5500, n3, n1);
            SeparateBytes(bytes, 850, 1500, n2, n3);
            SeparateBytes(bytes, 0, 300, n1, n2);

            bytes[fCode.f3] -= (byte)n3;
            bytes[fCode.f2] -= (byte)n2;
            bytes[fCode.f1] -= (byte)n1;
            bytes[2] -= (byte)n3;
            bytes[1] -= (byte)n2;
            bytes[0] -= (byte)n1;

            return bytes;
        }

        public static void SeparateBytes(byte[] b, int sIndex, int eIndex, int n1, int n2)
        {
            List<byte> tempArray = new();
            for (int i = sIndex; i < eIndex + n1 * 3; i++)
            {
                tempArray.Add((byte)(b[i] - n2));
            }
            tempArray.Reverse();
            int k = 0;
            for (int i = sIndex; i < eIndex + n1 * 3; i++)
            {
                b[i] = (byte)(tempArray[k] - n2);
                k++;
            }
        }


        public static string Decode(string input, string key)
        {
            byte[] inputBuffer = Convert.FromBase64String(input);
            byte[] outBytes = XOR(inputBuffer, key);
            return Encoding.UTF8.GetString(outBytes);
        }
    }
}
