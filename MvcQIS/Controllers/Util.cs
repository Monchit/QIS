using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MvcQIS.Models
{
    public class Util
    {
        public string CalculateMD5Hash(string input)
        {
            if (input != null)
            {
                // step 1, calculate MD5 hash from input
                MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hash = md5.ComputeHash(inputBytes);

                // step 2, convert byte array to hex string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    //sb.Append(hash[i].ToString("X2")); This line Return upper-case
                    sb.Append(hash[i].ToString("x2"));  // This line Return lower-case
                }
                return sb.ToString();
            }
            return string.Empty;
        }
    }
}