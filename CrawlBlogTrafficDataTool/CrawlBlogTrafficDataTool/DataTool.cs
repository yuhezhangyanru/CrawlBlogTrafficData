using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CrawlBlogTrafficDataTool
{
    public class DataTool
    {

        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static DataSet GetDatasetByString(string Value)
        {
            DataSet ds = new DataSet();
            string CC = GZipDecompressString(Value);
            System.IO.StringReader Sr = new StringReader(CC);
            ds.ReadXml(Sr);
            return ds;
        }

        /// <summary>
        /// 根据DATASET压缩字符串
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static string GetStringByDataset(string ds)
        {
            return GZipCompressString(ds);
        }

        /// <summary>
        /// 将传入字符串以GZip算法压缩后，返回Base64编码字符
        /// </summary>
        /// <param name="rawString">需要压缩的字符串</param>
        /// <returns>压缩后的Base64编码的字符串</returns>
        public static string GZipCompressString(string rawString)
        {
            if (string.IsNullOrEmpty(rawString) || rawString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] rawData = System.Text.Encoding.UTF8.GetBytes(rawString.ToString());
                byte[] zippedData = Compress(rawData);
                return (string)(Convert.ToBase64String(zippedData));
            }

        }

        /// <summary>
        /// GZip压缩
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] rawData)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }

        /// <summary>
        /// 将传入的二进制字符串资料以GZip算法解压缩
        /// </summary>
        /// <param name="zippedString">经GZip压缩后的二进制字符串</param>
        /// <returns>原始未压缩字符串</returns>
        public static string GZipDecompressString(string zippedString)
        {
            if (string.IsNullOrEmpty(zippedString) || zippedString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] zippedData = Convert.FromBase64String(zippedString.ToString());
                return (string)(System.Text.Encoding.UTF8.GetString(Decompress(zippedData)));
            }
        }

        /// <summary>
        /// ZIP解压
        /// </summary>
        /// <param name="zippedData"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] zippedData)
        {
            MemoryStream ms = new MemoryStream(zippedData);
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Decompress);
            MemoryStream outBuffer = new MemoryStream();
            byte[] block = new byte[1024];
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            return outBuffer.ToArray();
        }

        public static String getHtml1(string url)
        {
            StringBuilder s = new StringBuilder(102400); 
            WebClient wr = new WebClient();
            wr.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
            byte[] buffer = wr.DownloadData(url);
            GZipStream g = new GZipStream((Stream)(new MemoryStream(buffer)), CompressionMode.Decompress); byte[] d = new byte[20480]; int l = g.Read(d, 0, 20480); while (l > 0)
            {
                s.Append(Encoding.Default.GetString(d, 0, l)); l = g.Read(d, 0, 20480);
            }
         //   Console.Write(s.ToString() + "/n/n/n" + s.Length);
            return s.ToString();
        }


        public static String getHtml2(string url)
        {
            StringBuilder s = new StringBuilder(102400);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url); 
            wr.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate"; 
            HttpWebResponse response = (HttpWebResponse)wr.GetResponse(); head(response);
            GZipStream g = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress); byte[] d = new byte[20480]; int l = g.Read(d, 0, 20480); while (l > 0)
            {
                s.Append(Encoding.Default.GetString(d, 0, l)); l = g.Read(d, 0, 20480);


            }
            Console.Write(s.ToString() + "/n/n/n" + s.Length);
            return s.ToString();
        }
        public static void head(HttpWebResponse r)
        {
            string[] keys = r.Headers.AllKeys; for (int i = 0; i < keys.Length; ++i)
            {
                Console.WriteLine(keys[i] + "   " + r.Headers[keys[i]]);
            }
        } 
    }
}
