using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Ugoria.URBD.RemoteService
{
    static class ServiceUtil
    {
        private static MD5 md5Hash = MD5.Create();

        public static string GetMd5Hash(string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            return GetStringFromMD5Byte(data);
        }

        public static string GetFileMd5Hash(string filepath)
        {
            using (FileStream fileStream = new FileStream(filepath, FileMode.Open))
            {
                byte[] data = md5Hash.ComputeHash(fileStream);
                return GetStringFromMD5Byte(data);
            }
        }

        public static string GetStringFromMD5Byte(byte[] data)
        {
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();
        }
    }
}
