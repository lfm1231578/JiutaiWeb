using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;

namespace EYB.Web.Code
{
    public class MsgSend
    {
        /// <summary>
        /// 短信发送
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool SendMsg(string mobile, string code)
        {

            try
            {
              
                WebRequest myHttpWebRequest = WebRequest.Create("http://www.10690300.com/http/SendSms");
                myHttpWebRequest.Method = "POST";
                string PostData = GetPostStringFromEntity(mobile, code);
                string testData = PostData;
                //xml字符串必须是GBK编码
                byte[] byte1 = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(PostData);
                myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                myHttpWebRequest.ContentLength = byte1.Length;
                Stream newStream = myHttpWebRequest.GetRequestStream();
                newStream.Write(byte1, 0, byte1.Length);
                newStream.Close();
                //请求时间  以毫秒为单位  上行时间
                HttpWebResponse myResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                string content = reader.ReadToEnd();//得到结果
                reader.Close();
                //返回的是XML各式的数据，解析XML
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                XmlNode node = doc.SelectSingleNode("descendant::response");
                var result = node.InnerXml;
                if (int.Parse(result) > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 组装请求的XML
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static string GetPostStringFromEntity(string mobile,string code)
        {
            //密码MD5 加密  Wx2014!@#$%^   6c77dfdd5ed43b0a7c8119d2715bbf7c
            StringBuilder sbPost = new StringBuilder();
            string Content = "您的验证码为：" + code + ",感谢您的参与！";
            sbPost.Append(string.Format("Account={0}&Password={1}&Phone={2}&Content={3}&SubCode={4}&SendTime={5}", "dh21387",
                "6c77dfdd5ed43b0a7c8119d2715bbf7c", mobile, Content, "", ""));
            return sbPost.ToString();
        }

       
    }
}