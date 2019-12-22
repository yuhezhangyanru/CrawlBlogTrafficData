using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CrawlBlogTrafficDataTool
{
    public class HttpGetUrl
    {
        public static string HttpGet(string url, string post_parament)
        {
            string html;
            HttpWebRequest Web_Request = (HttpWebRequest)WebRequest.Create(url);
            Web_Request.Timeout = 30000;
            Web_Request.Method = "GET";
            Web_Request.UserAgent = "Mozilla/4.0";
            Web_Request.Headers.Add("Accept-Encoding", "gzip, deflate");
            //Web_Request.Credentials = CredentialCache.DefaultCredentials;

            //设置代理属性WebProxy-------------------------------------------------
            //WebProxy proxy = new WebProxy("111.13.7.120", 80);
            //在发起HTTP请求前将proxy赋值给HttpWebRequest的Proxy属性
            //Web_Request.Proxy = proxy;

            HttpWebResponse Web_Response = (HttpWebResponse)Web_Request.GetResponse();

            if (Web_Response.ContentEncoding.ToLower() == "gzip")  // 如果使用了GZip则先解压
            {
                using (Stream Stream_Receive = Web_Response.GetResponseStream())
                {
                    using (var Zip_Stream = new GZipStream(Stream_Receive, CompressionMode.Decompress))
                    {
                        using (StreamReader Stream_Reader = new StreamReader(Zip_Stream, Encoding.UTF8))
                        {
                            html = Stream_Reader.ReadToEnd();
                        }
                    }
                }
            }
            else
            {
                using (Stream Stream_Receive = Web_Response.GetResponseStream())
                {
                    using (StreamReader Stream_Reader = new StreamReader(Stream_Receive, Encoding.UTF8))
                    {
                        html = Stream_Reader.ReadToEnd();
                    }
                }
            }

            return html;
        }


        public static string GetStringFromAsciiHex(String input)
        {
            if (input.Length % 2 != 0)
                throw new ArgumentException("input");

            byte[] bytes = new byte[input.Length / 2];

            for (int i = 0; i < input.Length; i += 2)
            {
                // Split the string into two-bytes strings which represent a hexadecimal value, and convert each value to a byte
                String hex = input.Substring(i, 2);
                bytes[i / 2] = Convert.ToByte(hex, 16);
            }

            return System.Text.ASCIIEncoding.ASCII.GetString(bytes);
        }

        public static string ConvertHexToString(string HexValue, string separator = null)
        {
            HexValue = string.IsNullOrEmpty(separator) ? HexValue : HexValue.Replace(string.Empty, separator);
            StringBuilder sbStrValue = new StringBuilder();
            while (HexValue.Length > 0)
            {
                sbStrValue.Append(Convert.ToChar(Convert.ToUInt32(HexValue.Substring(0, 2), 16)).ToString());
                HexValue = HexValue.Substring(2);
            }
            return sbStrValue.ToString();
        } 
    }

}
