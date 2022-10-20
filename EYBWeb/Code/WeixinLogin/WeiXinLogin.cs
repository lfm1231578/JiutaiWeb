using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace EYB.Web.Code
{
    public class WeiXinLogin
    {

        /// <summary>
        /// MD5　32位加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
       public static string GetMd5Str32(string str)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            // Convert the input string to a byte array and compute the hash.  
            char[] temp = str.ToCharArray();
            byte[] buf = new byte[temp.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                buf[i] = (byte)temp[i];
            }
            byte[] data = md5Hasher.ComputeHash(buf);
            // Create a new Stringbuilder to collect the bytes  
            // and create a string.  
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data   
            // and format each one as a hexadecimal string.  
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.  
            return sBuilder.ToString();
        }

        public static bool ExecLogin(string name, string pass)
        {
            bool result = false;
            string password = GetMd5Str32(pass).ToUpper();
            string padata = "username=" + name + "&pwd=" + password + "&imgcode=&f=json";
            string url = "https://mp.weixin.qq.com/cgi-bin/login?lang=zh_CN ";//请求登录的URL
            try
            {
                CookieContainer cc = new CookieContainer();//接收缓存
                byte[] byteArray = Encoding.UTF8.GetBytes(padata); // 转化
                HttpWebRequest webRequest2 = (HttpWebRequest)WebRequest.Create(url);  //新建一个WebRequest对象用来请求或者响应url
                ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();

                webRequest2.CookieContainer = cc;                                      //保存cookie  
                webRequest2.Method = "POST";                                          //请求方式是POST
                webRequest2.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36";
                webRequest2.Referer = "https://mp.weixin.qq.com/";
                webRequest2.ContentType = "application/x-www-form-urlencoded";       //请求的内容格式为application/x-www-form-urlencoded
                webRequest2.ContentLength = byteArray.Length;
                Stream newStream = webRequest2.GetRequestStream();           //返回用于将数据写入 Internet 资源的 Stream。
                // Send the data.
                newStream.Write(byteArray, 0, byteArray.Length);    //写入参数
                newStream.Close();
                HttpWebResponse response2 = (HttpWebResponse)webRequest2.GetResponse();
                StreamReader sr2 = new StreamReader(response2.GetResponseStream(), Encoding.Default);
                string text2 = sr2.ReadToEnd();

                //此处用到了newtonsoft来序列化
                WeiXinRetInfo retinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WeiXinRetInfo>(text2);
                string token = string.Empty;
                if (retinfo.err_msg.Length > 0)
                {
                    token = retinfo.err_msg.Split(new char[] { '&' })[2].Split(new char[] { '=' })[1].ToString();//取得令牌
                    LoginInfo.LoginCookie = cc;
                    LoginInfo.CreateDate = DateTime.Now;
                    LoginInfo.Token = token;
                    result = true;
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.StackTrace);
            }
            return result;
        }

        public static class LoginInfo
        {
            /// <summary>
            /// 登录后得到的令牌
            /// </summary>        
            public static string Token { get; set; }
            /// <summary>
            /// 登录后得到的cookie
            /// </summary>
            public static CookieContainer LoginCookie { get; set; }
            /// <summary>
            /// 创建时间
            /// </summary>
            public static DateTime CreateDate { get; set; }

        }

        internal class AcceptAllCertificatePolicy : ICertificatePolicy
        {
            public AcceptAllCertificatePolicy()
            {
            }

            public bool CheckValidationResult(ServicePoint sPoint,
               X509Certificate cert, WebRequest wRequest, int certProb)
            {
                // Always accept  
                return true;
            }
        }
        //获取fakeid
        public static ArrayList SubscribeMP()
        {

            try
            {
                CookieContainer cookie = null;
                string token = null;


                cookie = WeiXinLogin.LoginInfo.LoginCookie;//取得cookie
                token = WeiXinLogin.LoginInfo.Token;//取得token

                /*获取用户信息的url，这里有几个参数给大家讲一下，1.token此参数为上面的token 2.pagesize此参数为每一页显示的记录条数
 
                3.pageid为当前的页数，4.groupid为微信公众平台的用户分组的组id，当然这也是我的猜想不一定正确*/
                string Url = "https://mp.weixin.qq.com/cgi-bin/contactmanage?t=user/index&pagesize=10&pageidx=0&type=0&groupid=0&token=" + token + "&lang=zh_CN";
                HttpWebRequest webRequest2 = (HttpWebRequest)WebRequest.Create(Url);
                webRequest2.CookieContainer = cookie;
                webRequest2.ContentType = "text/html; charset=UTF-8";
                webRequest2.Method = "GET";
                webRequest2.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";
                webRequest2.ContentType = "application/x-www-form-urlencoded";
                HttpWebResponse response2 = (HttpWebResponse)webRequest2.GetResponse();
                StreamReader sr2 = new StreamReader(response2.GetResponseStream(), Encoding.Default);
                string text2 = sr2.ReadToEnd();


                MatchCollection mc;

                //由于此方法获取过来的信息是一个html网页所以此处使用了正则表达式，注意：（此正则表达式只是获取了fakeid的信息如果想获得一些其他的信息修改此处的正则表达式就可以了。）
                Regex r = new Regex("\"id\"\\:\\d+,\"nick_name\""); //定义一个Regex对象实例
                mc = r.Matches(text2);
                Int32 friendSum = mc.Count;          //好友总数

                string fackID = "";

                ArrayList fackID1 = new ArrayList();

                for (int i = 0; i < friendSum; i++)
                {
                    //"id":208989515,"nick_name"
                    fackID = mc[i].Value.Replace(",\"nick_name\"", "").Split(new char[] { ':' })[1];
                    fackID = fackID.Replace("\"", "").Trim();
                    fackID1.Add(fackID);
                }

                return fackID1;



            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        //根据fakeid获取openid  
        public static string GetOpenidByFakeid(string fakeid)
        {
            try
            {
                CookieContainer cookie = null;
                string token = null;


                cookie = WeiXinLogin.LoginInfo.LoginCookie;//取得cookie
                token = WeiXinLogin.LoginInfo.Token;//取得token
                string Url = "https://mp.weixin.qq.com/cgi-bin/singlemsgpage?msgid=&source=&count=20&t=wxm-singlechat&fromfakeid=" + fakeid + "&token=" + token + "&lang=zh_CN";
                HttpWebRequest webRequest2 = (HttpWebRequest)WebRequest.Create(Url);
                webRequest2.CookieContainer = cookie;
                webRequest2.ContentType = "text/html; charset=UTF-8";
                webRequest2.Method = "GET";
                webRequest2.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";
                webRequest2.ContentType = "application/x-www-form-urlencoded";
                HttpWebResponse response2 = (HttpWebResponse)webRequest2.GetResponse();
                StreamReader sr2 = new StreamReader(response2.GetResponseStream(), Encoding.UTF8);
                string text2 = sr2.ReadToEnd();
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(text2);
                var str = doc.GetElementbyId("json-msgList").InnerHtml;
                JArray messages = JArray.Parse(str);
                foreach (var message in messages)
                {
                    string strContent = HttpUtility.HtmlDecode(message["content"].ToString());
                    Regex reg = new Regex(@"(?is)<a[^>]*?href=(['""\s]?)(?<href>[^'""\s]*)\1[^>]*?>");
                    MatchCollection match = reg.Matches(strContent);
                    var href = "";
                    foreach (Match m in match)
                    {
                        href = m.Groups["href"].Value;
                    }

                    var openid = GetUrlParamValue(href, "openid");
                    if (!string.IsNullOrEmpty(openid))
                        return openid;

                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }

        }
        //分隔参数
        public static string GetUrlParamValue(string href, string key)
        {
            
            var url = href.Split('?');
            if (url.Length <= 1)
            {
                return "";
            }

            var paramsList = url[1].Split('&');

            for (var i = 0; i < paramsList.Length; i++)
            {
                var param = paramsList[i].Split('=');
                if (key == param[0])
                {
                    return param[1];
                }
            }

            return "";
        }

        public static bool SendMessage(string Message, string fakeid)
        {
            bool result = false;
            CookieContainer cookie = null;
            string token = null;
            cookie = WeiXinLogin.LoginInfo.LoginCookie;//取得cookie
            token = WeiXinLogin.LoginInfo.Token;//取得token

            string strMsg = System.Web.HttpUtility.UrlEncode(Message);  //对传递过来的信息进行url编码
            string padate = "type=1&content=" + strMsg + "&error=false&tofakeid=" + fakeid + "&token=" + token + "&ajax=1";
            string url = "https://mp.weixin.qq.com/cgi-bin/singlesend?t=ajax-response&lang=zh_CN";

            byte[] byteArray = Encoding.UTF8.GetBytes(padate); // 转化

            HttpWebRequest webRequest2 = (HttpWebRequest)WebRequest.Create(url);

            webRequest2.CookieContainer = cookie; //登录时得到的缓存

            webRequest2.Referer = "https://mp.weixin.qq.com/cgi-bin/singlemsgpage?token=" + token + "&fromfakeid=" + fakeid + "&msgid=&source=&count=20&t=wxm-singlechat&lang=zh_CN";

            webRequest2.Method = "POST";

            webRequest2.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";

            webRequest2.ContentType = "application/x-www-form-urlencoded";

            webRequest2.ContentLength = byteArray.Length;

            Stream newStream = webRequest2.GetRequestStream();

            // Send the data.            
            newStream.Write(byteArray, 0, byteArray.Length);    //写入参数    

            newStream.Close();

            HttpWebResponse response2 = (HttpWebResponse)webRequest2.GetResponse();

            StreamReader sr2 = new StreamReader(response2.GetResponseStream(), Encoding.Default);

            string text2 = sr2.ReadToEnd();
            if (text2.Contains("ok"))
            {
                result = true;
            }
            return result;
        }
    }
}