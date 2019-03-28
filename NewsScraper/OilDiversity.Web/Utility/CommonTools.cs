using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace OilDiversity.Web.Utility
{
    public class CommonTools
    {
        public static string Encrypt(string clearText)
        {
            const string encryptionKey = "MAKV2SPBNI99212";
            var clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey,
                    new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});
                if (encryptor == null) return clearText;
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText)
        {
            const string encryptionKey = "MAKV2SPBNI99212";
            cipherText = cipherText.Replace(" ", "+");
            var cipherBytes = Convert.FromBase64String(cipherText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey,
                    new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});
                if (encryptor == null) return cipherText;
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static string TrimByWord(string text, int count)
        {
            var array = text.Split(' ');
            var total = array.Length;
            var newString = string.Join(" ", array.Take(count));
            return total > count ? newString + "..." : newString;
        }

        public static string RemoveUselessTags(
            string stringHtml)
        {
            var regex =
                new Regex(
                    "(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)|(\\<figure(.+?)\\</figure\\>)|(\\<aside(.+?)\\</aside\\>)",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return regex.Replace(stringHtml, "");
        }

        public static string RemoveH2Tags(
            string stringHtml)
        {
            var regex = new Regex("(\\<h2(.+?)\\</h2\\>)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regex.Replace(stringHtml, "");
        }

        public static string RemoveUl(
            string stringHtml)
        {
            var regex = new Regex("(\\<ul(.+?)\\</ul\\>)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regex.Replace(stringHtml, "");
        }

        public static string GetCurrentContextUrl()
        {
            return
                $"{HttpContext.Current.Request.Url.Scheme}{Uri.SchemeDelimiter}{HttpContext.Current.Request.Url.Host}{(HttpContext.Current.Request.Url.IsDefaultPort ? string.Empty : $":{HttpContext.Current.Request.Url.Port}")}";
        }

        // remove all html tags from string
        public static string StripHtml(string input)
        {
            input = string.IsNullOrWhiteSpace(input) ? "" : input;
            return Regex.Replace(input, "<.*?>", "");
        }
    }
}