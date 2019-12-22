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
using System.Text.RegularExpressions;
using System.Security.AccessControl;

/// <summary>
/// function：采集我的博客访问量，DownloadData()中为我的博客地址
/// date：2018-7-18 15:45:49
/// </summary>
public class Program
{
    /// <summary>
    /// 统计的博客地址
    /// </summary>
    private static String BLOG_URL = "https://blog.csdn.net/Stephanie_1";
    private static DateTime last = DateTime.Now;
    private static int COLLECT_INERVAL_MILISECOND = 43200;//0;//1000 * 120;//测试每隔2min采集一次 43200;//每隔0.5天采集一次，60;//每隔5s采集一次数据
    private static string userNameKey = "";//作为统计文档的关键字

    public static void Main(string[] args)
    {
        Console.WriteLine("请输入你的CSDN博客地址(如:https://blog.csdn.net/Stephanie_1)：");
        var readLine = Console.ReadLine();
        Console.WriteLine("输入的博客地址:" + readLine + "@");
        if (!readLine.StartsWith("https://blog.csdn.net/") && readLine != "")
        {
            Console.WriteLine("输入的站点非CSDN博客地址！请重启程序重新输入！");
            Console.ReadKey();
            return;
        }
        if (readLine != "")
        {
            BLOG_URL = readLine;
        }
        userNameKey = BLOG_URL.Substring(BLOG_URL.LastIndexOf("/") + 1);
        Console.WriteLine("用户统计关键字:" + userNameKey + ",博客访问量统计开始统计，等待:" + COLLECT_INERVAL_MILISECOND + "毫秒之后");
        int tempCount = 0;
        Thread th = new Thread(ThreadChild);//ThreadChild);
        th.Start(20);
        th.IsBackground = true;
        //   th.s.Start(); 
        //    Console.WriteLine("另一个目录=" + AppDomain.CurrentDomain.BaseDirectory); 
        UpdateGetData();//刚刚启动先采集一次
        Console.ReadLine(); //让控制台暂停,否则一闪而过了  
    }

    public static void ThreadChild(object obj)
    {
        while (true)
        {
            Thread.Sleep(COLLECT_INERVAL_MILISECOND); 
            UpdateGetData(); 
        }
    }

    private static void UpdateGetData()
    {
        Console.WriteLine("开始请求我的数据");
        WebClient MyWebClient = new WebClient();
        MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
        Byte[] pageData = MyWebClient.DownloadData(BLOG_URL);

        string pageHtml = Encoding.UTF8.GetString(pageData);  //如果获取网站页面采用的是GB2312，则使用这句
        String finalContent = "";
        List<KeyValueInfo> keyList = new List<KeyValueInfo>();
         
        string anyStr = "(.*?)";
        string sign="\"";
        string regexNumber = @"^(-?\d+)(\.\d+)?$";//匹配一个数字
         
        Match m;
        //统计：专栏数据格式
        /**
         <p class="title"><a href="https://blog.csdn.net/column/details/25100.html" target="_blank">ASP.NET网站开发</a></p>
                        <div class="data">阅读量：<span>5599</span><span class="count">5 篇</span></div>
         * **/ 
        string patternURI = @"<p class="+sign+"title"+sign+"><a href=" + sign + "https://blog.csdn.net" + anyStr + ".html" + sign;// +" target=";// +sign + "_blank" + sign + ">";// +anyStr + "</a></p>";
        m = Regex.Match(pageHtml, patternURI, RegexOptions.Multiline);
        Console.WriteLine("匹配字符串patternURI=" + patternURI);
        while (m.Success)
        {
            var match = m;
            int tempIndex = 0; 
            //Console.WriteLine("匹配到的=" + match.ToString());//+"@index="+match.Index);
            foreach (var item in m.Groups)
            {
                string value = item.ToString().Replace("<dt>", "");
                value = value.Replace("</dt>", "");
                // Console.WriteLine("参数[" + tempIndex + "],value=" + item.ToString()+",laterValue="+value);
                string addStr = "";
                for (int readIndex = match.Index + match.ToString().Length+1; readIndex < pageHtml.Length; readIndex++)
                {
                    addStr += pageHtml[readIndex];
                    if (addStr.EndsWith("</div>"))
                    {
                        var categoryName = addStr.Substring(0, addStr.IndexOf("<")); //专栏标题
                        var tempContent = addStr.Substring(addStr.IndexOf("<span>")+"<span>".Length);
                        var readCount = tempContent.Substring(0, tempContent.IndexOf("</span>")); //专栏阅读量
                        var fileCountKey = "<span class="+sign+"count"+sign+">"; 
                        var fileCount = tempContent.Substring(tempContent.IndexOf(fileCountKey) + fileCountKey.Length); 
                        fileCount = fileCount.Substring(0, fileCount.IndexOf(" "));//</span></div>"));//专栏文章数 
                        //File.AppendAllText(@"D:\yanruDelete.txt", DateTime.Now + ":读到的关键字符串=" + addStr
                        //    + "\n专栏标题title=" + categoryName + "\ntempContent=" + tempContent + "\n阅读数=" + readCount + "\nfileCount="+fileCount+"\n");
                        keyList.Add(new KeyValueInfo(categoryName + "阅读量", readCount));
                        keyList.Add(new KeyValueInfo(categoryName + "文章数", fileCount));
                        break;
                    }
                }
            }
            m = m.NextMatch();
        }

        string matchChinese = @"[\u4e00-\u9fa5]*[\u4e00-\u9fa5]";//匹配一个中文字符串：即收尾要求都是中文暂时 "[\u4e00-\u9fa5]";
        string matchNumber = @"[\d\.]+ [\d\.]*"; 
        //统计：访问量等关键字 
        /**
         <dl>
            <dt>访问：</dt>
            <dd title="88988">
                8万+            </dd>
        </dl>
         * str
         * **/
        string matchCount = "<dt>" + matchChinese + "：</dt>";//\n            ";//<dd title=" + anyStr + ">";// +sign;// +matchNumber;// +anyStr + "<dd title=";
        m = Regex.Match(pageHtml, matchCount, RegexOptions.Multiline);
        Console.WriteLine("匹配字符串matchCount=" + matchCount);
        while (m.Success)
        {
            var match = m;
            int tempIndex = 0;

        //    Console.WriteLine("匹配到的=" + match.ToString()+"@index="+match.Index);
            foreach (var item in m.Groups)
            {
                string value = item.ToString().Replace("<dt>", "");
                value = value.Replace("</dt>", "");
            //    Console.WriteLine("参数[" + tempIndex + "],value=" + item.ToString() + ",laterValue=" + value);
                string addStr = "";
                for (int readIndex = match.Index +match.ToString().Length+1; readIndex < pageHtml.Length; readIndex++)
                {
                    addStr += pageHtml[readIndex];
                    if (addStr.EndsWith(">"))
                    {
                       // Console.WriteLine("该参数累加的addStr=" + addStr);//[" + tempIndex + "],value=" + item.ToString() + ",laterValue=" + value);
                        var tempValues = addStr.Split('"');
                        foreach (string tempStr in tempValues)
                        {
                            int tempNumberAsCount = -1;
                            int.TryParse(tempStr, out tempNumberAsCount);
                        //    Console.WriteLine("临时值=" + tempStr + ",tempNumberAsCount=" + tempNumberAsCount + ",是数字?" + (tempNumberAsCount>0) + "@");
                            if (tempNumberAsCount > 0)
                            {
                                keyList.Add(new KeyValueInfo(value,tempStr));
                            }
                        }
                        break;
                    }
                }
            }
            m = m.NextMatch();
        }

        //统计：积分、排名等关键字。匹配格式
        /**  <dt>积分：</dt>
            <dd title=1627>
                1627            </dd>
         * **/
      //  string matchContent = pageHtml;
        string matchStr1 = "<dt>" + matchChinese + "</dt>";//匹配到所有的中文关键字，看下一个词段内是否包含数字 +anyStr + "<dd title=" + sign + matchNumber + sign + ">";// "<dt>" + matchChinese + "：</dt>";
        // +anyStr + "</dd>";//"^\d*$";//"^[1-9]\d*$";// regexNumber;// "<dd title=" + sign + regexNumber + anyStr + "</dd>";
        m = Regex.Match(pageHtml, matchStr1, RegexOptions.Multiline);
        Console.WriteLine("匹配字符串matchStr1=" + matchStr1);
        while (m.Success)
        {
            var match = m;
            int tempIndex = 0;
              
            //Console.WriteLine("匹配到的=" + match.ToString());//+"@index="+match.Index);
            foreach (var item in m.Groups)
            {
                string value = item.ToString().Replace("<dt>", "");
                value = value.Replace("</dt>", "");
                //Console.WriteLine("参数[" + tempIndex + "],value=" + item.ToString()+",laterValue="+value);
                string addStr = "";
                for (int readIndex = match.Index+ match.ToString().Length; readIndex < pageHtml.Length; readIndex++)
                {
                    addStr += pageHtml[readIndex];
                    if (addStr.EndsWith("/dd>"))
                    {
                        //提取数字，提取成功的，添加一次key value,此外， 
                        //yanruTODO 最后对键值对列表 key过滤多余符号，并且去重复
                        addStr = addStr.Replace("\n", ""); 
                        Match tempMatch = Regex.Match(addStr, @"[\d]*[\d]", RegexOptions.Multiline);//匹配两位及以上数字 
                        if (tempMatch.Groups.Count > 0)
                        {
                            string tempValue = tempMatch.Groups[0].ToString(); 
                            if (tempValue != "")
                            {
                                keyList.Add(new KeyValueInfo(value, tempValue));
                            }
                        }
                        break;
                    }
                }

                tempIndex++;
            }
            m = m.NextMatch();
        }


        string content = pageHtml;
        string regexStr = "<dt>" + anyStr + "</dt>" + "([\\s\\S]*?)<dd><span class=" + sign + "count" + sign + ">" + anyStr + "</span></dd>";
        
        //step0:匹配格式：
        //<dt>评论</dt>
        //    <dd><span class="count">72</span></dd>
        m = Regex.Match(content, regexStr, RegexOptions.Multiline);
        Console.WriteLine("匹配的字符串="+regexStr);
        while (m.Success)
        {
            var match = m;
            int tempIndex = 0;
        //    Console.WriteLine("匹配到的=" + match.ToString());
            string key = "";
            foreach(var item in m.Groups)
            { 
                if (tempIndex == 1)//中文关键字
                    key = item.ToString();
                if (tempIndex == 3 && key!="")
                    keyList.Add(new KeyValueInfo(key,item.ToString()));
                tempIndex++;
            }            
            m = m.NextMatch();
        }
        List<string> tempStrList = new List<string>();
        for (int tempIndex = 0; tempIndex < keyList.Count; tempIndex++)
        {
            keyList[tempIndex].key = keyList[tempIndex].key.Replace("：", "");
            if (tempStrList.Contains(keyList[tempIndex].key))
            {
            //    keyList[tempIndex].key += tempIndex + "";
                keyList.RemoveAt(tempIndex);
                continue;
            }
            tempStrList.Add(keyList[tempIndex].key);
        }

        var keyContent = ""; 
        keyList.Insert(0, new KeyValueInfo("记录时间",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        tempStrList.Clear();
        List<string> newValueList = new List<string>();
        foreach (KeyValueInfo item in keyList)
        {
            if (tempStrList.Contains(item.key))
                continue;
            finalContent += item.value + ",";
            keyContent += item.key + ",";
            tempStrList.Add(item.key);
            newValueList.Add(item.value);
        }
        
        //保存解析后的值
        finalContent += "\n";
        string accountFileKey = userNameKey +"的";
        String pathRoot = Environment.CurrentDirectory;
        string valuePath = pathRoot + @"\" + accountFileKey + "统计关键字的值.txt";
        string keyPath = pathRoot + @"\" + accountFileKey + "统计关键字.txt";
        var oldLines = new string[1];
        bool allValueSame = true;
        Console.WriteLine(DateTime.Now + "爬取完毕，当前程序pathRoot=\n" + pathRoot + ",保存统计结果的路径=" + valuePath
            + ",路径存在?" + ((File.Exists(valuePath))));// + ",keyList=" + keyList.Count);
        if (File.Exists(valuePath))
        {
            oldLines = File.ReadAllLines(valuePath, Encoding.Default);
          //  Console.WriteLine("读取旧的文件文件="+valuePath+",文件的行数="+oldLines.Length);

            var oldLastLine = "";
            if (oldLines.Length >= 1)
            {
                oldLastLine = oldLines[oldLines.Length - 1];
            }
            if (oldLastLine == "" || oldLastLine == "\n")
            {
                allValueSame = false;
            }
            else
            {
                var oldValueLine = oldLastLine.Split(',');
              //  Console.WriteLine("读到的行内容=" + oldLines[index] + ",个数=" + oldValueLine.Length);
                if (oldValueLine.Length < newValueList.Count)
                {
                //    Console.WriteLine("读取的新的值个数newValueList.Count=" + newValueList.Count + ",oldValueLine.Length=" + oldValueLine.Length);
                    allValueSame = false;
                }
                else
                {
                    for (int tempIndex = 0; tempIndex < oldValueLine.Length; tempIndex++)
                    {
                        var oldValue = oldValueLine[tempIndex];
                        if (oldValue != "" && tempIndex < newValueList.Count)
                        {
                            var newValue = newValueList[tempIndex];
                            if (oldValue != newValue && tempIndex > 0)//忽略0的
                            {
                             //   Console.WriteLine("新的值=" + oldValue + ",newValue=" + newValue + ",值不同?" + (oldValue != newValue) + "@");
                                allValueSame = false;
                                break;
                            }
                        }
                    }
                } 
            } 
        }
        else
        { 
            allValueSame = false; 
        }
        Console.WriteLine("统计完毕，有变化插入新记录？" + (!allValueSame));
        //有值发生变化，才进行统计
        if (!allValueSame)
        {
            File.AppendAllText(valuePath, finalContent, Encoding.Default);
        }
        using (StreamWriter sw = new StreamWriter(keyPath, false, Encoding.Default))
        {
            sw.Write(keyContent); 
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