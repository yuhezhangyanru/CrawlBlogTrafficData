using BaiduCang;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using System.Configuration;

/// <summary>
/// function：采集我的博客访问量，DownloadData()中为我的博客地址
/// date：2018-7-18 15:45:49
/// </summary>
public class Program
{
    private const String BLOG_URL = "https://blog.csdn.net/Stephanie_1";
    private static DateTime last = DateTime.Now;
    private static int COLLECT_INERVAL_MILISECOND = 43200;//0;//1000 * 120;//yanruTODO 测试每隔2min采集一次 43200;//每隔0.5天采集一次，60;//每隔5s采集一次数据

    public static void Main(string[] args)
    {
        Console.WriteLine("博客访问量统计开始统计，等待:"+COLLECT_INERVAL_MILISECOND+"毫秒之后"); 
        int tempCount = 0;
        Thread th = new Thread(ThreadChild);//ThreadChild);
        th.Start(20);
        th.IsBackground = true;
        //   th.s.Start(); 
        //    Console.WriteLine("另一个目录=" + AppDomain.CurrentDomain.BaseDirectory);
        Console.ReadLine(); //让控制台暂停,否则一闪而过了  
    }

    public static void ThreadChild(object obj)
    {
        while (true)
        {
            Thread.Sleep(COLLECT_INERVAL_MILISECOND);
            // Console.WriteLine("Child Thread Start!");
            //   DateTime now = DateTime.Now;
            UpdateGetData();
            //    Console.WriteLine("刷新 now=" + now + ",sub=" + (now - last).TotalMilliseconds);
        }
    }

    private static void UpdateGetData()
    {
        Console.WriteLine("开始请求我的数据");
        WebClient MyWebClient = new WebClient();
        MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
        Byte[] pageData = MyWebClient.DownloadData(BLOG_URL);

        string pageHtml = Encoding.UTF8.GetString(pageData);  //如果获取网站页面采用的是GB2312，则使用这句
        String finalContent = "记录时间," + DateTime.Now;
        //访问：
        int index = 0;
        Boolean hasData = false;
        String visitCountKey = "";
        String readStr = "";
        int visitIndex = 0;
        visitCountKey = "<dt>访问：</dt>";
        visitIndex = pageHtml.IndexOf(visitCountKey) + visitCountKey.Length;
        if (pageHtml.IndexOf(visitCountKey) >= 0)
        {
            for (index = visitIndex; index < pageHtml.Length; index++)
            {
                var ch = pageHtml[index];
                readStr += ch;
                if (ch == '>')
                {
                    readStr = readStr.Replace("\n", "");
                    readStr = readStr.Replace(" ", "");
                    readStr = readStr.Substring(readStr.IndexOf("\"") + 1, readStr.LastIndexOf("\"") - readStr.IndexOf("\"") - 1);
                    //       Console.WriteLine("当前值readStr=" + readStr);
                    finalContent += ",总访问量," + readStr;
                    hasData = true;
                    break;
                }
            }
        }
        if (!hasData)
        {
            finalContent += ",总访问量,-1";
        }

        //我的排名
        visitCountKey = "<dt>排名：</dt>";
        hasData = false;
        readStr = "";
        visitIndex = pageHtml.IndexOf(visitCountKey) - 1;// +visitCountKey.Length;
        if (pageHtml.IndexOf(visitCountKey) >= 0)
        {
            for (index = visitIndex; index > 0; index--)
            {
                var ch = pageHtml[index];
                readStr = readStr.Insert(0, ch + "");//+=ch;
                if (ch == '<')
                {
                    readStr = readStr.Replace("\n", "");
                    readStr = readStr.Replace(" ", "");
                    //   readStr.Reverse();
                    readStr = readStr.Substring(readStr.IndexOf("\"") + 1, readStr.LastIndexOf("\"") - readStr.IndexOf("\"") - 1);
                    //     Console.WriteLine("当前值readStr=" + readStr);
                    finalContent += ",排名," + readStr;
                    hasData = true;
                    break;
                }
            }
        }
        if (!hasData)
        {
            finalContent += ",排名,-1";
        }

        //Unity学习总结</a></p> 专栏访问量和文章数
        visitCountKey = "Unity学习总结</a></p>";
        hasData = false;
        readStr = "";
        visitIndex = pageHtml.IndexOf(visitCountKey) + +visitCountKey.Length;
        try
        {
            if (pageHtml.IndexOf(visitCountKey) >= 0)
            {
                for (index = visitIndex; index < pageHtml.Length; index++)
                {
                    var ch = pageHtml[index];
                    readStr += ch;
                    if (readStr.EndsWith("</span></div>"))
                    {
                        readStr = readStr.Replace("\n", "");
                        readStr = readStr.Replace(" ", "");
                        //     readStr = readStr.Substring(readStr.IndexOf("\"") + 1, readStr.LastIndexOf("\"") - readStr.IndexOf("\"") - 1);
                        //    Console.WriteLine("当前值readStr=" + readStr);
                        String[] array = readStr.Split("<".ToCharArray());
                        //  for (int tempIndex = 0; tempIndex < array.Length; tempIndex++)
                        //  {
                        //array[0]=
                        //array[1]=divclass="data">阅读量：
                        //array[2]=span>46465
                        //array[3]=/span>
                        //array[4]=spanclass="count">18篇
                        //array[5]=/span>
                        //array[6]=/div>
                        //   Console.WriteLine("array[" + tempIndex + "]=" + array[tempIndex]);
                        // }
                        finalContent += ",Unity学习专栏访问量," + array[2].Substring(array[2].IndexOf(">") + 1);
                        String fileCount = array[4].Substring(array[4].IndexOf(">") + 1);
                        fileCount = fileCount.Replace("篇", "");
                        finalContent += ",Unity学习专栏文章," + fileCount;
                        hasData = true;
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
        }
        if (!hasData)
        {
            finalContent += ",Unity学习专栏访问量,-1";
            finalContent += ",Unity学习专栏文章,-1";
        }



        finalContent += "\n";
        String pathRoot = Environment.CurrentDirectory;
        Console.WriteLine(DateTime.Now + "爬取完毕，当前程序pathRoot=\n" + pathRoot);
        using (StreamWriter sw = new StreamWriter(pathRoot + @"\yanruCSDNVisitLog_1.txt", true, Encoding.Default))
        {
            sw.Write(finalContent);
            //    sw.Write(pageHtml);
        } 
    }

    //public static  string GetConfigValue(string appKey)
    //{
    //    XmlDocument xDoc = new XmlDocument();
    //    try
    //    {
    //        xDoc.Load(System.Windows.Forms.Application.ExecutablePath + ".config");
    //        XmlNode xNode;
    //        XmlElement xElem;
    //        xNode = xDoc.SelectSingleNode("//appSettings");
    //        xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + appKey + "']");
    //        if (xElem != null)
    //            return xElem.GetAttribute("value");
    //        else
    //            return "";
    //    }
    //    catch (Exception)
    //    {
    //        return "";
    //    }
    //}
}