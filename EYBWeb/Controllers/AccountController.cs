using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using HS.SupportComponents;
using HS.SupportComponents.Base;
using SystemManage.Factory.IBLL;
using Common.Attribute;
using EYB.ServiceEntity.SystemEntity;
using PersonnelManage.Factory.IBLL;
using EYB.ServiceEntity.PersonnelEntity;
using BaseinfoManage.Factory.IBLL;
using EYB.ServiceEntity.WarehouseEntity;
using WarehouseManage.Factory.IBLL;
using System.Security.Policy;
using System.Text;
using EYB.ServiceEntity.BaseInfo;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using NPOI.Util.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web.UI.WebControls;
using NPOI.SS.Formula.Functions;
using static NPOI.HSSF.Util.HSSFColor;
using System.ComponentModel.DataAnnotations;
using org.pdfbox.pdmodel;
using org.pdfbox.util;
using org.pdfbox.pdmodel.graphics.predictor;
using static javax.print.attribute.standard.MediaSize;
using gnu.javax.crypto.mode;

using Microsoft.Office.Interop.Word;
using Font = System.Drawing.Font;
using Rectangle = System.Drawing.Rectangle;
using java.lang.reflect;
using Type = System.Type;
using Aspose.Words;
using Document = Microsoft.Office.Interop.Word.Document;
using java.text;
using System.ServiceModel.Channels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Spire.Pdf;
using gnu.javax.crypto.sasl.srp;
using org.w3c.dom.ls;
using PdfDocument = Spire.Pdf.PdfDocument;
using Common.Helper;
using EYB.ServiceEntity.BaseInfo.ImportExcel;

namespace EYB.Web.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public class AccountController : Controller
    {
        #region 依赖注入
        private ISystemManageBLL systemBLL;
        private IPersonnelManageBLL personnelManageBLL;
        private IBaseinfoBLL baseinfoBLL;

        private IWarehouseManageBLL iwareBLL; //仓库管理    
        public AccountController()
        {
            iwareBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IWarehouseManageBLL>();
            systemBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();
            personnelManageBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            baseinfoBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IBaseinfoBLL>();
        }
        #endregion 依赖注入

        #region==Get请求==
        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        public ActionResult LoginOut()
        {
            Session.Abandon();
            CookieHelper.ClearCookie("eyblogin");

            var sysSetting = baseinfoBLL.GetSysSettingEntity();

            return View("LoginPage", sysSetting);
        }
        //退出
        public ActionResult LoginOff()
        {
            return View("LoginPage");
        }
        public ActionResult LoginPage()
        {


            var sysSetting = baseinfoBLL.GetSysSettingEntity();
            return View(sysSetting);
        }
        public ActionResult LoginHome()
        {


            var sysSetting = baseinfoBLL.GetSysSettingEntity();
            return View(sysSetting);
        }
        public ActionResult VersionPage()
        {
            return View();
        }
        public ActionResult DownloadPage()
        {
            return View();
        }
        public string  pdf2txt(FileInfo pdffile, FileInfo txtfile)
        {

            PDDocument doc = PDDocument.load(pdffile.FullName);
            PDFTextStripper pdfStripper = new PDFTextStripper();
            string text = "数据报错，赋予空值";
            try {
                text = pdfStripper.getText(doc);
            } catch(Exception e) { 

            }
            return text;
        }

        private string GetIntegralByMonth341(string sqlstr, int filter = 0)
        {

            string month = "失败";
            try
            {
                string conStr = "server=192.168.10.12;user=sa;pwd=123456;database=Ymsoft";//连接字符串   
                SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                conText.Open();//打开数据库  
                string sqls = sqlstr;//创建统计语句  
                SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                SqlDataReader dr;//创建DataReader对象  
                dr = comText.ExecuteReader();//执行查询  
                //while (dr.Read())//判断数据表中是否含有数据  
                //{
                //    var date = dr;
                //    if (filter == 0)
                //    {
                //        month = date["IntegralByMonth"].ToString();//到店次数
                //    } 
                //    else
                //    {
                //        month = date["AttributeValue"].ToString();//其他       
                //    }
                //}
                month = "成功";
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return month;

        }
        private string GetIntegralByMonth1(string sqlstr, int filter = 0)
        {

            string month = "失败";
            try
            {
                string conStr = "server=192.168.10.12;user=sa;pwd=123456;database=Blog";//连接字符串   
                SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                conText.Open();//打开数据库  
                string sqls = sqlstr;//创建统计语句  
                SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                SqlDataReader dr;//创建DataReader对象  
                dr = comText.ExecuteReader();//执行查询  
                //while (dr.Read())//判断数据表中是否含有数据  
                //{
                //    var date = dr;
                //    if (filter == 0)
                //    {
                //        month = date["IntegralByMonth"].ToString();//到店次数
                //    } 
                //    else
                //    {
                //        month = date["AttributeValue"].ToString();//其他       
                //    }
                //}
                month = "成功";
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return month;

        }
        public static bool WordToPDF(string inputPath, string outputPath, int startPage = 0, int endPage = 0)
        {
            bool b = true;

            #region 初始化
            //初始化一个application
            Application wordApplication = new Application();
            //初始化一个document
            Document wordDocument = null;
            #endregion

            #region 参数设置（所谓的参数都是根据这个方法来的:ExportAsFixedFormat）
            //word路径
            object wordPath = Path.GetFullPath(inputPath);

            //输出路径
            string pdfPath = Path.GetFullPath(outputPath);

            //导出格式为PDF
            WdExportFormat wdExportFormat = WdExportFormat.wdExportFormatPDF;

            //导出大文件
            WdExportOptimizeFor wdExportOptimizeFor = WdExportOptimizeFor.wdExportOptimizeForPrint;

            //导出整个文档
            WdExportRange wdExportRange = WdExportRange.wdExportAllDocument;

            //开始页码
            int startIndex = startPage;

            //结束页码
            int endIndex = endPage;

            //导出不带标记的文档（这个可以改）
            WdExportItem wdExportItem = WdExportItem.wdExportDocumentContent;

            //包含word属性
            bool includeDocProps = true;

            //导出书签
            WdExportCreateBookmarks paramCreateBookmarks = WdExportCreateBookmarks.wdExportCreateWordBookmarks;

            //默认值
            object paramMissing = Type.Missing;

            #endregion

            #region 转换
            try
            {
              
                //打开word
                wordDocument = wordApplication.Documents.Open(ref wordPath, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing);
                //转换成指定格式
                if (wordDocument != null)
                {
                    wordDocument.ExportAsFixedFormat(pdfPath, wdExportFormat, false, wdExportOptimizeFor, wdExportRange, startIndex, endIndex, wdExportItem, includeDocProps, true, paramCreateBookmarks, true, true, false, ref paramMissing);
                }
            }
            catch (Exception ex)
            {
                b = false;
            }
            finally
            {
                //关闭
                if (wordDocument != null)
                {
                    wordDocument.Close(ref paramMissing, ref paramMissing, ref paramMissing);
                    wordDocument = null;
                }

                //退出
                if (wordApplication != null)
                {
                    wordApplication.Quit(ref paramMissing, ref paramMissing, ref paramMissing);
                    wordApplication = null;
                }
            }

            return b;
            #endregion
        }
        public static int WordsToPDFs(string[] inputPaths, string outputPath)
        {
            int count = 0;

            #region 初始化
            //初始化一个application
            Application wordApplication = new Application();
            //初始化一个document
            Document wordDocument = null;
            #endregion

            //默认值
            object paramMissing = Type.Missing;

            for (int i = 0; i < inputPaths.Length; i++)
            {
                #region 参数设置（所谓的参数都是根据这个方法来的:ExportAsFixedFormat）
                //word路径
                object wordPath = Path.GetFullPath(inputPaths[i]);

                //获取文件名
                string outputName = Path.GetFileNameWithoutExtension(inputPaths[i]);

                //输出路径
                string pdfPath = Path.GetFullPath(outputPath + @"\" + outputName + ".pdf");

                //导出格式为PDF
                WdExportFormat wdExportFormat = WdExportFormat.wdExportFormatPDF;

                //导出大文件
                WdExportOptimizeFor wdExportOptimizeFor = WdExportOptimizeFor.wdExportOptimizeForPrint;

                //导出整个文档
                WdExportRange wdExportRange = WdExportRange.wdExportAllDocument;

                //开始页码
                int startIndex = 0;

                //结束页码
                int endIndex = 0;

                //导出不带标记的文档（这个可以改）
                WdExportItem wdExportItem = WdExportItem.wdExportDocumentContent;

                //包含word属性
                bool includeDocProps = true;

                //导出书签
                WdExportCreateBookmarks paramCreateBookmarks = WdExportCreateBookmarks.wdExportCreateWordBookmarks;

                #endregion

                #region 转换
                try
                {
                    //打开word
                    wordDocument = wordApplication.Documents.Open(ref wordPath, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing);
                    //转换成指定格式
                    if (wordDocument != null)
                    {
                        wordDocument.ExportAsFixedFormat(pdfPath, wdExportFormat, false, wdExportOptimizeFor, wdExportRange, startIndex, endIndex, wdExportItem, includeDocProps, true, paramCreateBookmarks, true, true, false, ref paramMissing);
                    }
                    count++;
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    //关闭
                    if (wordDocument != null)
                    {
                        wordDocument.Close(ref paramMissing, ref paramMissing, ref paramMissing);
                        wordDocument = null;
                    }
                }
            }

            //退出
            if (wordApplication != null)
            {
                wordApplication.Quit(ref paramMissing, ref paramMissing, ref paramMissing);
                wordApplication = null;
            }
            return count;
            #endregion
        }
        public string  Main11(string str)
        {
            //return pdf2txt(new FileInfo(@"D:/文献/pdf3/" + str + @".pdf"), new FileInfo(@"C:/Users/dell/Desktop/222.txt"));
            return pdf2txt(new FileInfo(@"D:/文献/pdf3/" + str ), new FileInfo(@"C:/Users/dell/Desktop/222.txt"));
        }
        static string PDF;
        static string TEXT2;
        public void parsePdf(String src, String dest)
        {
            PdfReader reader = new PdfReader(src);
            StreamWriter output = new StreamWriter(new FileStream(dest, FileMode.Create));
            int pageCount = reader.NumberOfPages;
            for (int pg = 1; pg <= pageCount; pg++)
            {
                byte[] streamBytes = reader.GetPageContent(pg);
                PRTokeniser tokenizer = new PRTokeniser(streamBytes);
                while (tokenizer.NextToken())
                {
                    if (tokenizer.TokenType == PRTokeniser.TokType.STRING)
                    {
                        output.WriteLine(tokenizer.StringValue);
                    }
                }
            }
            output.Flush();
            output.Close();
        }
        [WriteOperateLog("用户登录", "用户登录Login", "登入模块")]
        public ActionResult JiutaiChatGPT11459412(string key)
        {
            var resluts = "";
            //DataTable excelData = null;
            //var fileName = "";
            //try
            //{

            //    excelData = (DataTable)ExcelOperate.ExcelToTableForXLSX(@"");
            //}
            //catch
            //{
            //    excelData = (DataTable)ExcelOperate.ExcelToTableForXLS(@"");
            //}
            //List<ImportProductEntity> list = ModelConvertHelper<ImportProductEntity>.ConvertToModel(excelData).ToList();

            //foreach (var info in list)
            //{
            //}
            return Json(new { resluts });

        }
        [WriteOperateLog("用户登录", "用户登录Login", "登入模块")]
        public ActionResult JiutaiChatGPT11459411(string key)
        {
            
            string name1 = "";
            Console.WriteLine(1);

            for(var i = 0;i<100; i++)
            {
                Console.WriteLine("下载 ----->" + i);
            }



            var resluts = "imag";
            string name = "";

            string path = @"D:\文献\pdf3";
            DirectoryInfo root = new DirectoryInfo(path);
            foreach (FileInfo f in root.GetFiles())
            {
                //name = f.Name.Replace(".pdf", "") ;
                name = f.Name;//.Replace(".pdf", "");
                              //string fullName = f.FullName;
                try
                {

                    PdfReader pdfReader = new PdfReader(@"D:\\文献\\pdf3\\" + name);

                    for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                    {

                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                        string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                        name1 += currentText;

                    }
                    pdfReader.Close();

                    string cc = name1.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "").Replace("'", "").Trim();
                    var dd = cc.Length;
                    string str1 = "", str2 = "", str3 = "", str4 = "", str5 = "", str6 = "", str7 = "", str8 = "", str9 = "", str10 = "", str11 = "", str12 = "", str13 = "", str14 = "";
                    if (dd > 0)
                    {
                        name = f.Name.Replace(".pdf", "");//.Replace("_", "/");
                        //name = f.Name.Replace(".pdf", "").Replace("_", "/");
                        //string insql = "INSERT INTO PaperPDF (技术审评报告名称) VALUES ( '" + name + "') ";
                        //string inrr = GetIntegralByMonth341(insql);
                        Console.WriteLine(name);
                        if (dd > 4000)
                        {
                            str1 = cc.Substring(0, 4000);
                            string sql = "UPDATE PaperPDF SET  详细内容1 ='" + str1 + "' WHERE other = '"+ name + "'";
                            string rr = GetIntegralByMonth341(sql);

                            if (dd > 8000)
                            {
                                str2 = cc.Remove(0, 4000).Substring(0, 4000);
                                sql = "UPDATE PaperPDF SET  详细内容2 ='" + str2 + "' WHERE other = '"+ name + "'";
                                rr = GetIntegralByMonth341(sql);

                                if (dd > 12000)
                                {
                                    str3 = cc.Remove(0, 8000).Substring(0, 4000);
                                    sql = "UPDATE PaperPDF SET  详细内容3 ='" + str3 + "' WHERE other = '"+ name + "'";
                                    rr = GetIntegralByMonth341(sql);

                                    if (dd > 16000)
                                    {
                                        str4 = cc.Remove(0, 12000).Substring(0, 4000);
                                        sql = "UPDATE PaperPDF SET  详细内容4 ='" + str4 + "' WHERE other = '"+ name + "'";
                                        rr = GetIntegralByMonth341(sql);

                                        if (dd > 20000)
                                        {
                                            str5 = cc.Remove(0, 16000).Substring(0, 4000);
                                            sql = "UPDATE PaperPDF SET  详细内容5 ='" + str5 + "' WHERE other = '"+ name + "'";
                                            rr = GetIntegralByMonth341(sql);

                                            if (dd > 24000)
                                            {
                                                str6 = cc.Remove(0, 20000).Substring(0, 4000);
                                                sql = "UPDATE PaperPDF SET  详细内容6 ='" + str6 + "' WHERE other = '"+ name + "'";
                                                rr = GetIntegralByMonth341(sql);

                                                if (dd > 28000)
                                                {
                                                    str7 = cc.Remove(0, 24000).Substring(0, 4000);
                                                    sql = "UPDATE PaperPDF SET  详细内容7 ='" + str7 + "' WHERE other = '"+ name + "'";
                                                    rr = GetIntegralByMonth341(sql);

                                                    if (dd > 32000)
                                                    {
                                                        str8 = cc.Remove(0, 28000).Substring(0, 4000);
                                                        sql = "UPDATE PaperPDF SET  详细内容8 ='" + str8 + "' WHERE other = '"+ name + "'";
                                                        rr = GetIntegralByMonth341(sql);

                                                        if (dd > 36000)
                                                        {
                                                            str9 = cc.Remove(0, 32000).Substring(0, 4000);
                                                            sql = "UPDATE PaperPDF SET  详细内容9 ='" + str9 + "' WHERE other = '"+ name + "'";
                                                            rr = GetIntegralByMonth341(sql);

                                                            if (dd > 40000)
                                                            {

                                                                str10 = cc.Remove(0, 36000).Substring(0, 4000);
                                                                sql = "UPDATE PaperPDF SET  详细内容10 ='" + str10 + "' WHERE other = '"+ name + "'";
                                                                rr = GetIntegralByMonth341(sql);

                                                                if (dd > 44000) {

                                                                    str11 = cc.Remove(0, 40000).Substring(0, 4000);
                                                                    sql = "UPDATE PaperPDF SET  详细内容11 ='" + str11 + "' WHERE other = '" + name + "'";
                                                                    rr = GetIntegralByMonth341(sql);

                                                                    if (dd > 48000){

                                                                        str12 = cc.Remove(0, 44000).Substring(0, 4000);
                                                                        sql = "UPDATE PaperPDF SET  详细内容12 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                        rr = GetIntegralByMonth341(sql);

                                                                        if (dd > 52000) {

                                                                            str12 = cc.Remove(0, 48000).Substring(0, 4000);
                                                                            sql = "UPDATE PaperPDF SET  详细内容13 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                            rr = GetIntegralByMonth341(sql);

                                                                            if (dd > 56000) {

                                                                                str12 = cc.Remove(0, 52000).Substring(0, 4000);
                                                                                sql = "UPDATE PaperPDF SET  详细内容14 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                rr = GetIntegralByMonth341(sql);

                                                                                if (dd > 60000)
                                                                                {
                                                                                    str12 = cc.Remove(0, 56000).Substring(0, 4000);
                                                                                    sql = "UPDATE PaperPDF SET  详细内容15 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                    rr = GetIntegralByMonth341(sql);

                                                                                    if (dd > 64000)
                                                                                    {
                                                                                        str12 = cc.Remove(0, 60000).Substring(0, 4000);
                                                                                        sql = "UPDATE PaperPDF SET  详细内容16 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                        rr = GetIntegralByMonth341(sql);
                                                                                    
                                                                                        if (dd > 68000) {

                                                                                            str12 = cc.Remove(0, 64000).Substring(0, 4000);
                                                                                            sql = "UPDATE PaperPDF SET  详细内容17 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                            rr = GetIntegralByMonth341(sql);

                                                                                            if (dd > 72000) {

                                                                                                str12 = cc.Remove(0, 68000).Substring(0, 4000);
                                                                                                sql = "UPDATE PaperPDF SET  详细内容18 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                                rr = GetIntegralByMonth341(sql);

                                                                                                if (dd > 78000)
                                                                                                {
                                                                                                    str12 = cc.Remove(0, 72000).Substring(0, 4000);
                                                                                                    sql = "UPDATE PaperPDF SET  详细内容19 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                                    rr = GetIntegralByMonth341(sql);

                                                                                                    if (dd > 82000) {

                                                                                                        str12 = cc.Remove(0, 78000).Substring(0, 4000);
                                                                                                        sql = "UPDATE PaperPDF SET  详细内容20 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                                        rr = GetIntegralByMonth341(sql);

                                                                                                    } else {

                                                                                                        str12 = cc.Remove(0, 78000);
                                                                                                        sql = "UPDATE PaperPDF SET  详细内容20 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                                        rr = GetIntegralByMonth341(sql);
                                                                                                    }

                                                                                                } else {
                                                                                                    str12 = cc.Remove(0, 72000);
                                                                                                    sql = "UPDATE PaperPDF SET  详细内容19 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                                    rr = GetIntegralByMonth341(sql);

                                                                                                }

                                                                                            } else {

                                                                                                str12 = cc.Remove(0, 68000);
                                                                                                sql = "UPDATE PaperPDF SET  详细内容18 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                                rr = GetIntegralByMonth341(sql);

                                                                                            }

                                                                                        } else {

                                                                                            str12 = cc.Remove(0, 64000);
                                                                                            sql = "UPDATE PaperPDF SET  详细内容17 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                            rr = GetIntegralByMonth341(sql);
                                                                                        }

                                                                                    } else {

                                                                                        str12 = cc.Remove(0, 60000);
                                                                                        sql = "UPDATE PaperPDF SET  详细内容16 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                        rr = GetIntegralByMonth341(sql);

                                                                                    }


                                                                                } else {

                                                                                    str12 = cc.Remove(0, 56000);
                                                                                    sql = "UPDATE PaperPDF SET  详细内容15 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                    rr = GetIntegralByMonth341(sql);
                                                                                }

                                                                            } else {

                                                                                str12 = cc.Remove(0, 52000);
                                                                                sql = "UPDATE PaperPDF SET  详细内容14 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                                rr = GetIntegralByMonth341(sql);
                                                                            }
                                                                        } else {

                                                                            str12 = cc.Remove(0, 48000);
                                                                            sql = "UPDATE PaperPDF SET  详细内容13 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                            rr = GetIntegralByMonth341(sql);
                                                                        }
                                                                    } else {

                                                                        str12 = cc.Remove(0, 44000);
                                                                        sql = "UPDATE PaperPDF SET  详细内容12 ='" + str12 + "' WHERE other = '" + name + "'";
                                                                        rr = GetIntegralByMonth341(sql);
                                                                    }
                                                                } else {

                                                                    str11 = cc.Remove(0, 38000);
                                                                    sql = "UPDATE PaperPDF SET  详细内容11 ='" + str11 + "' WHERE other = '" + name + "'";
                                                                    rr = GetIntegralByMonth341(sql);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                str10 = cc.Remove(0, 34000);
                                                                sql = "UPDATE PaperPDF SET  详细内容10 ='" + str10 + "' WHERE other = '"+ name + "'";
                                                                rr = GetIntegralByMonth341(sql);

                                                            }
                                                        }
                                                        else
                                                        {
                                                            str9 = cc.Remove(0, 32000);
                                                            sql = "UPDATE PaperPDF SET  详细内容9 ='" + str9 + "' WHERE other = '"+ name + "'";
                                                            rr = GetIntegralByMonth341(sql);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        str8 = cc.Remove(0, 28000);
                                                        sql = "UPDATE PaperPDF SET  详细内容8 ='" + str8 + "' WHERE other = '"+ name + "'";
                                                        rr = GetIntegralByMonth341(sql);

                                                    }
                                                }
                                                else
                                                {
                                                    str7 = cc.Remove(0, 24000);
                                                    sql = "UPDATE PaperPDF SET  详细内容7 ='" + str7 + "' WHERE other = '"+ name + "'";
                                                    rr = GetIntegralByMonth341(sql);

                                                }
                                            }
                                            else
                                            {

                                                str6 = cc.Remove(0, 20000);
                                                sql = "UPDATE PaperPDF SET  详细内容6 ='" + str6 + "' WHERE other = '"+ name + "'";
                                                rr = GetIntegralByMonth341(sql);
                                            }
                                        }
                                        else
                                        {
                                            str5 = cc.Remove(0, 16000);
                                            sql = "UPDATE PaperPDF SET  详细内容5 ='" + str5 + "' WHERE other = '"+ name + "'";
                                            rr = GetIntegralByMonth341(sql);

                                        }
                                    }
                                    else
                                    {

                                        str4 = cc.Remove(0, 12000);
                                        sql = "UPDATE PaperPDF SET  详细内容4 ='" + str4 + "' WHERE other = '"+ name + "'";
                                        rr = GetIntegralByMonth341(sql);
                                    }
                                }
                                else
                                {
                                    str3 = cc.Remove(0, 8000);
                                    sql = "UPDATE PaperPDF SET  详细内容3 ='" + str3 + "' WHERE other = '"+ name + "'";
                                    rr = GetIntegralByMonth341(sql);

                                }
                            }
                            else
                            {
                                str2 = cc.Remove(0, 4000);
                                sql = "UPDATE PaperPDF SET  详细内容2 ='" + str2 + "' WHERE other = '"+ name + "'";
                                rr = GetIntegralByMonth341(sql);
                            }
                        }
                        else
                        {
                            string sql = "UPDATE PaperPDF SET  详细内容1 ='" + cc + "' WHERE other = '"+ name + "'";
                            string rr = GetIntegralByMonth341(sql);
                        }

                    }
                }
                catch (Exception ex)
                {
                }
            } 

       





            return Json(new { resluts });
        }
        [WriteOperateLog("用户登录", "用户登录Login", "登入模块")]
        public ActionResult JiutaiChatGPT(string key)
        {
            var resluts = "imag";

            //string cc = Main11().Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("'","").Trim();
            //var  dd = Encoding.Default.GetByteCount(cc);
            //string str1 = cc.Substring(0, 4000);
            //string str2 = cc.Remove(0, 7000);




            //string sql = "UPDATE PaperPDF SET  详细内容='"+ str1 + "' WHERE other = '"+ name + "'";
            //string rr  = GetIntegralByMonth1(sql);
            string _url = string.Format("https://gpttalk.live/api/chat-process");

            //System.Threading.Thread.Sleep(1 * 2 * 1000); //休眠30秒
            //json参数
            //string jsonParam = Newtonsoft.Json.JsonConvert.SerializeObject(new
            //{
            //    chatId = "15187477",
            //    parentChatId = "15187475",
            //    sentenceId =0,
            //    stop = 0,
            //    timestamp = 1679368929167, 
            //    deviceType = "pc",
            //    sign = "1679288415966_1679368029164_13iqOlPG28VuKkzRh9alRF0wVQ8WjvSsu2iuGokzD/ujqvrj/wmAD6A2a0hENZFH7YCZlX+DeNZQGooNjQW2zNPoR6TeAN9MD/GVfzUy8OJ081yg0f5dk8yFT7IPiBGAGLWozEdQQCEyXN20sR3gtYHrgniwxx5YFr69v+8WLBE77iFx7/dsJDjFi3tv78MFVWxgO5wywWJdv1kqWVQJqzwCqcwJ1wW2N5VngvNKyIy9r2953aEzK60VCc1TExjLY/bm+Pyu+v8uqMjGZZMlK4O93B3hTw+26raJkXxazDabx8b6s7adC9ymYANKqaWKwjx66+GGso2qaq9OHN2acg=="

            //});
            var valuestr = key;
            string jsonParam = "{\"prompt\":\"中国\",\"options\":{\"parentMessageId\":\"chatcmpl-72AuDhmT57dbpvS9gmDOvuryWgsVU\"},\"systemMessage\":\"You are ChatGPT, a large language model trained by OpenAI. Follow the user's instructions carefully. Respond using markdown.\"}";
            var request = (HttpWebRequest)WebRequest.Create(_url);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";//ContentType
            request.Headers.Add("Cookie", "_ga=GA1.1.1405402686.1680744018; _clck=1hpgfw2|1|faj|0; _ga_DR0DKGM5XH=GS1.1.1680758052.3.1.1680758054.0.0.0; csrfToken=WqSRImavOvSr_Gb1wCEozjws; _clsk=k7wi4|1680758248512|3|1|r.clarity.ms/collect");
            request.Headers.Add("x-csrf-token", "WqSRImavOvSr_Gb1wCEozjws");
            //request.Headers.Add("Cookie", "BIDUPSID=7891B647FEF86D67510769E8D37EA33E; BAIDUID=F0147F17F26EC14E47BCF2AEFFE12921:FG=1; PSTM=1658661266; BDUSS=JXYVktblhaVTlGZkcxMFlPNGg2aG01MVZ0RDgyY1dXSEYxWTI4R35RUzBCalprSUFBQUFBJCQAAAAAAAAAAAEAAAB-4P9EbHVvZmFtaW5nNAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALR5DmS0eQ5kbG; BDUSS_BFESS=JXYVktblhaVTlGZkcxMFlPNGg2aG01MVZ0RDgyY1dXSEYxWTI4R35RUzBCalprSUFBQUFBJCQAAAAAAAAAAAEAAAB-4P9EbHVvZmFtaW5nNAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALR5DmS0eQ5kbG; MCITY=-257%3A; Hm_lvt_01e907653ac089993ee83ed00ef9c2f3=1679276132; BDSFRCVID=MSkOJeC62GHnemvfyjuGhfuCPvJiZXjTH6aoT49yHx85QsgAUu5oEG0P0U8g0KuM5NlwogKK3gOTH4DF_2uxOjjg8UtVJeC6EG0Ptf8g0f5; H_BDCLCKID_SF=JnI8oCD2JIK3Hnnp5-J25KCQbMDDJT8XbD5jVM7Dbl7keq8CD6jIW4KuhxjIKTjJ2Ict5MJ8WCQsKqc2y5jHhUL4yfTv2-T--27phlvRfnjpsIJMhxFWbT8UQ4cT04r0aKviaKOjBMb1MhbDBT5h2M4qMxtOLR3pWDTm_q5TtUJMeCnTD-Dhe6jWDH8HJ6-jfKQtW4thHDKaJbTp24QEq4tHeNt8-URZ5m7LaUoOtbnEHRQwjjQbM-K0Ln3MBMPj52OnanRsaqo6ED5c5RjbLpFm346-35543bRTLnLy5KJvfJoD3h3ChP-UyN3MWh37Je3lMKoaMp78jR093JO4y4Ldj4oxJpOJ5JbMopCafD8bhDtGjTt-en-W5gTBa4T8bC_X3buQtII28pcNLTDKjx4eD-6BKJcvWJn30t5nLPL-MxoKKlO1j4_eQh642-Lq2Kvr0nT7atTvhl5jDh0K25ksD-4JW45AWHby0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qhja0tJT0ttJ-sL-35HJj2jtJvKn8_-P6MXGjCWMT-0bFH5xJgWx7ke4Te55osbRLd0h3eQpRxJan7_JjOQI-BJqn9e5oGKP-b54tOaUQxtN4j2CnjtpvP8D58hbJobUPUDUJ9LUkJ3gcdot5yBbc8eIna5hjkbfJBQttjQn3hfIkj0DKLJIthhK-GD6A35n-WqxQjaROy-5PX3buQBxOO8pcNLTDKQUkHbfcqKJcBbmv30t5nLPQkjfcyMlO1j4_e3a88hxvOfnTIQb3pQ4KhDp5jDh023jksD-RtW6QwaTvy0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qh-p52f6KetJFf3e; BDORZ=B490B5EBF6F3CD402E515D22BCDA1598; BA_HECTOR=a10h8l25a120ag8ka08080c81i1i5k51n; delPer=0; PSINO=6; ZFY=xeG8XGnodAhNOH37maKvpHgvVS4yWCQShEruX4R6o2w:C; BAIDUID_BFESS=F0147F17F26EC14E47BCF2AEFFE12921:FG=1; BDSFRCVID_BFESS=MSkOJeC62GHnemvfyjuGhfuCPvJiZXjTH6aoT49yHx85QsgAUu5oEG0P0U8g0KuM5NlwogKK3gOTH4DF_2uxOjjg8UtVJeC6EG0Ptf8g0f5; H_BDCLCKID_SF_BFESS=JnI8oCD2JIK3Hnnp5-J25KCQbMDDJT8XbD5jVM7Dbl7keq8CD6jIW4KuhxjIKTjJ2Ict5MJ8WCQsKqc2y5jHhUL4yfTv2-T--27phlvRfnjpsIJMhxFWbT8UQ4cT04r0aKviaKOjBMb1MhbDBT5h2M4qMxtOLR3pWDTm_q5TtUJMeCnTD-Dhe6jWDH8HJ6-jfKQtW4thHDKaJbTp24QEq4tHeNt8-URZ5m7LaUoOtbnEHRQwjjQbM-K0Ln3MBMPj52OnanRsaqo6ED5c5RjbLpFm346-35543bRTLnLy5KJvfJoD3h3ChP-UyN3MWh37Je3lMKoaMp78jR093JO4y4Ldj4oxJpOJ5JbMopCafD8bhDtGjTt-en-W5gTBa4T8bC_X3buQtII28pcNLTDKjx4eD-6BKJcvWJn30t5nLPL-MxoKKlO1j4_eQh642-Lq2Kvr0nT7atTvhl5jDh0K25ksD-4JW45AWHby0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qhja0tJT0ttJ-sL-35HJj2jtJvKn8_-P6MXGjCWMT-0bFH5xJgWx7ke4Te55osbRLd0h3eQpRxJan7_JjOQI-BJqn9e5oGKP-b54tOaUQxtN4j2CnjtpvP8D58hbJobUPUDUJ9LUkJ3gcdot5yBbc8eIna5hjkbfJBQttjQn3hfIkj0DKLJIthhK-GD6A35n-WqxQjaROy-5PX3buQBxOO8pcNLTDKQUkHbfcqKJcBbmv30t5nLPQkjfcyMlO1j4_e3a88hxvOfnTIQb3pQ4KhDp5jDh023jksD-RtW6QwaTvy0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qh-p52f6KetJFf3e; H_PS_PSSID=38185_36558_38410_38349_38307_37862_38174_38290_37922_38425_38314_38383_38285_26350_38420_38283_37881; __bid_n=186fca84a5b9216b5752e1; XFI=bf261800-c796-11ed-9e7c-993a1e2bd1b9; Hm_lpvt_01e907653ac089993ee83ed00ef9c2f3=1679368579; ab_sr=1.0.1_N2YyOTA1NTAyMjI2NmViYzQ3NjMxZjg3OTE4ZTQ3ODg1NGJiNDgxOWZhZjAyYjU5YTk4NDIyMmZlZmI4NGFiYTA5MDg1YzZiOTdiMGNkNzc5NjZlOGI5ZTBiYTY5ZjBkZmZjNTE1NjhkMzExMmRjZGU1MzEyYmNjOTVjYTBiMDhhNjgwMjA0YmQxMjM5M2VjMjRhMGE4ZWZiNmNhZTVkYzViYjMxNGFhYzE4ZDJkMzNkZDBiMmIyMmZlYjI0MmQz; XFCS=EED03EFAACD1A34C9A95B676A5742E986961084A1D66E5C82F7FDBF66FFF6525; XFT=YIBAwYbCQVTX/hBmbFSk3pokc8vmvt9MmWTTIO60MVk=");
            //request.Headers.Add("Acs-Token", "1679288415966_1679369409945_H2S6+gSGuHdouzSnuYXZt9m6M5kauNTIZXpHl6c5/p+ky0VMWplii+uwH2E+JsD1c0Z2Wu58VGKzVUJYrqlyfr/b7NEqKCVQUbNp47RIRZtwfSNLueUDK/pidv5MvU8cJe8LFzjfAuJ1t+2ujjkn403zZnT+V+6QsuO1P4Mpk3YM+z/ulNVN6hCOdG17KnB9alyB7XcceKCBmghYIbFrICf40Lw10FUqxtGo2QHUgCYOMC/Ev3baG6pNIMrjYdaiGFFjug9Kx7sAS7k8dfjADk7H3W80kuq4d6/AyXELYQ+hylZLQVgzyqa9lDBsd1C26fl102rgY47dLfkjwOpfIw==");
            //CookieContainer cc = new CookieContainer();
            //cc.Add(new System.Uri("https://yiyan.baidu.com"), new Cookie("Cookie", "BIDUPSID=7891B647FEF86D67510769E8D37EA33E; BAIDUID=F0147F17F26EC14E47BCF2AEFFE12921:FG=1; PSTM=1658661266; BDUSS=JXYVktblhaVTlGZkcxMFlPNGg2aG01MVZ0RDgyY1dXSEYxWTI4R35RUzBCalprSUFBQUFBJCQAAAAAAAAAAAEAAAB-4P9EbHVvZmFtaW5nNAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALR5DmS0eQ5kbG; BDUSS_BFESS=JXYVktblhaVTlGZkcxMFlPNGg2aG01MVZ0RDgyY1dXSEYxWTI4R35RUzBCalprSUFBQUFBJCQAAAAAAAAAAAEAAAB-4P9EbHVvZmFtaW5nNAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALR5DmS0eQ5kbG; MCITY=-257%3A; Hm_lvt_01e907653ac089993ee83ed00ef9c2f3=1679276132; BDORZ=B490B5EBF6F3CD402E515D22BCDA1598; BA_HECTOR=8h8k002h050g01048k8g840s1i1i3g31n; BDRCVFR[feWj1Vr5u3D]=I67x6TjHwwYf0; delPer=0; PSINO=6; BAIDUID_BFESS=F0147F17F26EC14E47BCF2AEFFE12921:FG=1; ZFY=xeG8XGnodAhNOH37maKvpHgvVS4yWCQShEruX4R6o2w:C; BCLID=4201984001025888909; BCLID_BFESS=4201984001025888909; BDSFRCVID=MSkOJeC62GHnemvfyjuGhfuCPvJiZXjTH6aoT49yHx85QsgAUu5oEG0P0U8g0KuM5NlwogKK3gOTH4DF_2uxOjjg8UtVJeC6EG0Ptf8g0f5; BDSFRCVID_BFESS=MSkOJeC62GHnemvfyjuGhfuCPvJiZXjTH6aoT49yHx85QsgAUu5oEG0P0U8g0KuM5NlwogKK3gOTH4DF_2uxOjjg8UtVJeC6EG0Ptf8g0f5; H_BDCLCKID_SF=JnI8oCD2JIK3Hnnp5-J25KCQbMDDJT8XbD5jVM7Dbl7keq8CD6jIW4KuhxjIKTjJ2Ict5MJ8WCQsKqc2y5jHhUL4yfTv2-T--27phlvRfnjpsIJMhxFWbT8UQ4cT04r0aKviaKOjBMb1MhbDBT5h2M4qMxtOLR3pWDTm_q5TtUJMeCnTD-Dhe6jWDH8HJ6-jfKQtW4thHDKaJbTp24QEq4tHeNt8-URZ5m7LaUoOtbnEHRQwjjQbM-K0Ln3MBMPj52OnanRsaqo6ED5c5RjbLpFm346-35543bRTLnLy5KJvfJoD3h3ChP-UyN3MWh37Je3lMKoaMp78jR093JO4y4Ldj4oxJpOJ5JbMopCafD8bhDtGjTt-en-W5gTBa4T8bC_X3buQtII28pcNLTDKjx4eD-6BKJcvWJn30t5nLPL-MxoKKlO1j4_eQh642-Lq2Kvr0nT7atTvhl5jDh0K25ksD-4JW45AWHby0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qhja0tJT0ttJ-sL-35HJj2jtJvKn8_-P6MXGjCWMT-0bFH5xJgWx7ke4Te55osbRLd0h3eQpRxJan7_JjOQI-BJqn9e5oGKP-b54tOaUQxtN4j2CnjtpvP8D58hbJobUPUDUJ9LUkJ3gcdot5yBbc8eIna5hjkbfJBQttjQn3hfIkj0DKLJIthhK-GD6A35n-WqxQjaROy-5PX3buQBxOO8pcNLTDKQUkHbfcqKJcBbmv30t5nLPQkjfcyMlO1j4_e3a88hxvOfnTIQb3pQ4KhDp5jDh023jksD-RtW6QwaTvy0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qh-p52f6KetJFf3e; H_BDCLCKID_SF_BFESS=JnI8oCD2JIK3Hnnp5-J25KCQbMDDJT8XbD5jVM7Dbl7keq8CD6jIW4KuhxjIKTjJ2Ict5MJ8WCQsKqc2y5jHhUL4yfTv2-T--27phlvRfnjpsIJMhxFWbT8UQ4cT04r0aKviaKOjBMb1MhbDBT5h2M4qMxtOLR3pWDTm_q5TtUJMeCnTD-Dhe6jWDH8HJ6-jfKQtW4thHDKaJbTp24QEq4tHeNt8-URZ5m7LaUoOtbnEHRQwjjQbM-K0Ln3MBMPj52OnanRsaqo6ED5c5RjbLpFm346-35543bRTLnLy5KJvfJoD3h3ChP-UyN3MWh37Je3lMKoaMp78jR093JO4y4Ldj4oxJpOJ5JbMopCafD8bhDtGjTt-en-W5gTBa4T8bC_X3buQtII28pcNLTDKjx4eD-6BKJcvWJn30t5nLPL-MxoKKlO1j4_eQh642-Lq2Kvr0nT7atTvhl5jDh0K25ksD-4JW45AWHby0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qhja0tJT0ttJ-sL-35HJj2jtJvKn8_-P6MXGjCWMT-0bFH5xJgWx7ke4Te55osbRLd0h3eQpRxJan7_JjOQI-BJqn9e5oGKP-b54tOaUQxtN4j2CnjtpvP8D58hbJobUPUDUJ9LUkJ3gcdot5yBbc8eIna5hjkbfJBQttjQn3hfIkj0DKLJIthhK-GD6A35n-WqxQjaROy-5PX3buQBxOO8pcNLTDKQUkHbfcqKJcBbmv30t5nLPQkjfcyMlO1j4_e3a88hxvOfnTIQb3pQ4KhDp5jDh023jksD-RtW6QwaTvy0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qh-p52f6KetJFf3e; XFI=d8cdfac0-c78c-11ed-9363-11fb77acfb22; Hm_lpvt_01e907653ac089993ee83ed00ef9c2f3=1679364327; __bid_n=186fca84a5b9216b5752e1; ab_sr=1.0.1_MDhlMzhhZjkyZDJiYWNlOGRhMjdkNjQ1YzczYjkzNDA0YzAyODQ1ZDhhNjNmMDAzZTZkMDM2ZmVhNGEzMzUwZTcwOTc3ODlmMWU0NjgzYzY1OWMyOWE2YTYyMjcxMDA3N2IwNTBkZTEzZGJiMTc4MjM4YjkzOTEwMmFmYjlhNzI4NzA5ZjY4ZjQxMmFiYWJjNDRiN2NjM2ZjNTBlNjQyNGQ0NTFiZGNlYmVkZjc0MWRmMDg5MWJiZTI5ODlhNzlm; XFCS=D4DA282258F7335484898A2AF69DC41EA15E3806075C8AAFEA6DDBE278479D6E; XFT=s25AHfKy5Z9kcvgUrcryF3aCf5jg21c06GlfhkXUHWw=; BDRCVFR[VXHUG3ZuJnT]=mk3SLVN4HKm; H_PS_PSSID=26350"));
            //cc.Add(new System.Uri("https://yiyan.baidu.com"), new Cookie("token", "1679288415966_1679364661713_iUtUdtoyITmXeBfte7MCnnK8oI/JVoX8qSHD7dEk/qtqgG5DD1JKOgEqQGD+msq4GuE05P//gtrrZjQxk0dwRuFPCSdlrfU9duyrDWkp0PeyFljeboqFVso55q4it+VrbQPUW34pdj80wLEIty8YrBGL7NvlqnmDTly+w/my5pvhAFPr4YD7RzY/xMP//Agyxi/wMj8bJP8rcozzEmWbaLOxslIZPgO5PFN/KBVN/tj9ZYJJDt5xvYk8ZwHHFgA4039fco4g20z32qhEs5JEM4Jtwuyyd7Jas0esf6GC36Af+FGzKL/hMbd3kFS3giu0DNLBz4CEY01UoqqVCSkBgA=="));
            //request.CookieContainer = cc;
            byte[] byteData = Encoding.UTF8.GetBytes(jsonParam);
            int length = byteData.Length;
            request.ContentLength = length;
            Stream writer = request.GetRequestStream();
            writer.Write(byteData, 0, length);
            writer.Close();
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")).ReadToEnd();
            return Json(new { responseString });
        }
        [WriteOperateLog("用户登录", "用户登录Login", "登入模块")]
        public ActionResult AddInfo(string name, string email, string phone, string city, string message, string other)
        {
            other = DateTime.Now.ToString();
            var rusult = string.Empty;
            if (string.IsNullOrEmpty(phone))
            {

                rusult = "手机号不能为空哦！";
                return Json(new { rusult, name, email, phone, city, message, other }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                //if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^1[3456789]\d{9}$"))
                //{
                //    rusult = "请您输入正确的手机号！";
                //    return Json(new { rusult, name, email, phone, city, message, other });
                //}
                //else
                //{
                if (string.IsNullOrEmpty(email))
                {

                    rusult = "邮箱不能为空哦！";
                    return Json(new { rusult, name, email, phone, city, message, other }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    string emailStr = @"([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,5})+";

                    Regex emailReg = new Regex(emailStr);
                    if (!emailReg.IsMatch(email))
                    {
                        rusult = "请您输入正确的邮箱！";
                        return Json(new { rusult, name, email, phone, city, message, other }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var strsql = "INSERT INTO userinfoTable(name, email, phone, city, message, other) VALUES('" +
                                       name + @"', '" + email + @"', '" + phone + @"', '" + city + @"', '" + message + @"', '" + other + @"')";
                        rusult = GetIntegralByMonth(strsql);
                        return Json(new { rusult, name, email, phone, city, message, other }, JsonRequestBehavior.AllowGet);
                    }
                }
                //}
            }
            //var strsql = "INSERT INTO userinfoTable(name, email, phone, city, message, other) VALUES('" +
            //               name + @"', '" + email + @"', '" + phone + @"', '" + city + @"', '" + message + @"', '" + other + @"')"; 
            //var rusult = GetIntegralByMonth(strsql);
            //return Json(new { rusult, name, email, phone, city, message, other });
        }

        [WriteOperateLog("用户登录", "用户登录Login", "登入模块")]
        public ActionResult AddInfoFinance(string name, string password, string phone, string filter1, string filter2, string other)
        {
            //other = DateTime.Now.ToString();
            var rusult = string.Empty;
            if (string.IsNullOrEmpty(name))
            {

                rusult = "账号不能为空哦！";
                return Json(new { rusult, name, password, phone, filter1, filter2, other }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                //if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^1[3456789]\d{9}$"))
                //{
                //    rusult = "请您输入正确的手机号！";
                //    return Json(new { rusult, name, email, phone, city, message, other });
                //}
                //else
                //{
                if (string.IsNullOrEmpty(password))
                {

                    rusult = "密码不能为空哦！";
                    return Json(new { rusult, name, password, phone, filter1, filter2, other }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    string emailStr = @"([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,5})+";

                    Regex emailReg = new Regex(emailStr);

                        var strsql = "INSERT INTO caiwuTable(name, password, phone, filter1, filter2, other) VALUES('" +
                                       name + @"', '" + password + @"', '" + phone + @"', '" + filter1 + @"', '" + filter2 + @"', '" + other + @"')";
                        rusult = GetIntegralByMonth(strsql);
                        return Json(new { rusult, name, password, phone, filter1, filter2, other }, JsonRequestBehavior.AllowGet);
                    
                }
                //}
            }
            //var strsql = "INSERT INTO userinfoTable(name, email, phone, city, message, other) VALUES('" +
            //               name + @"', '" + email + @"', '" + phone + @"', '" + city + @"', '" + message + @"', '" + other + @"')"; 
            //var rusult = GetIntegralByMonth(strsql);
            //return Json(new { rusult, name, email, phone, city, message, other });
        }

        public ActionResult UpdateFinance(string id ,string name, string password, string phone, string filter1, string filter2, string other) {
            var rusult = string.Empty;

            int ids = 0;
            if (id != null && id != "")
            {
                ids = Convert.ToInt32(id);
            }
            if (string.IsNullOrEmpty(name))
            {

                rusult = "账号不能为空哦！";
                return Json(new { rusult, name, password, phone, filter1, filter2, other }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                //if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^1[3456789]\d{9}$"))
                //{
                //    rusult = "请您输入正确的手机号！";
                //    return Json(new { rusult, name, email, phone, city, message, other });
                //}
                //else
                //{
                if (string.IsNullOrEmpty(password))
                {

                    rusult = "密码不能为空哦！";
                    return Json(new { rusult, name, password, phone, filter1, filter2, other }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    //string emailStr = @"([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,5})+";

                    //Regex emailReg = new Regex(emailStr);
                    var strsql = "UPDATE caiwuTable SET name='" + name + @"', Password='" + password + @"', email='" + phone + @"', phone='" + phone + @"', city='" + filter1 + @"', filter1='" + filter1 + @"', filter2='" + filter2 + @"', filter3='" + filter2 + @"', filter4='" + filter2 + @"', filter5='" + filter2 + @"', other='" + DateTime.Now.ToString() + @"' WHERE id=" + ids ;
                    //var strsql = "INSERT INTO caiwuTable(name, password, phone, filter1, filter2, other) VALUES('" +
                    //               name + @"', '" + password + @"', '" + phone + @"', '" + filter1 + @"', '" + filter2 + @"', '" + other + @"')";
                    rusult = GetIntegralByMonth(strsql);
                    return Json(new { rusult, name, password, phone, filter1, filter2, other }, JsonRequestBehavior.AllowGet);

                }
                //}
            }
            
        }

        public ActionResult SelectFinanceList(string key, string papes, string cid) {
            var en = "0";
            int p = 1;
            if (papes != null && papes != "")
            {
                p = Convert.ToInt32(papes.ToString());
            }
            //var _papes = "当前页papes:" + papes;
            var where = "   where 1 = 1";
            if (key != null && key != "")
            {
                    where = where + "and  name like '%" + key + @"%' ";
            }
            if (cid != null && cid != "")
            {
                where = where + " and phone like '%" + cid + @"%' ";
            }
            var sql = "SELECT id,name, Password, email, phone, city, filter1, filter2, filter3, filter4, filter5, other FROM caiwuTable" + where;
            var list = GetItemEntityList(sql);
            var count = list.Count.ToString();
            list = list.Skip((p - 1) * 5).Take(5).ToList();

            sql = "";
            return Json(new { sql, key, papes, cid, count, list }, JsonRequestBehavior.AllowGet);


        }
        public ActionResult ModuleFinance(string id, string papes, string cid, string isen)
        {
            //string _url = string.Format("https://modelscope.cn/api/v1/models/damo/cv_diffusion_text-to-image-synthesis/widgets/submit");
            //string[] arr = new string[1];
            //arr[0] = "2只小熊和狗";
            ////json参数
            //string jsonParam = Newtonsoft.Json.JsonConvert.SerializeObject(new
            //{
            //    task = "text-to-image-synthesis",
            //    inputs = arr
            //});
            //var request = (HttpWebRequest)WebRequest.Create(_url);
            
            //request.Method = "POST";
            //request.ContentType = "application/json;charset=UTF-8";

            //byte[] byteData = Encoding.UTF8.GetBytes(jsonParam);
            //int length = byteData.Length;
            //request.ContentLength = length;

            //request.Headers.Add("cookie", "uuid_tt_dd=11_27076939590-1679470980362-017728; dc_session_id=11_1679470980362.638130; c_segment=0; c_page_id=default; cna=ffxkG9VIjgkCAXBeH6SxVDo+; xlly_s=1; dp_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZCI6MTkwNzAwNywiZXhwIjoxNjgwMDc1Nzg5LCJpYXQiOjE2Nzk0NzA5ODksInVzZXJuYW1lIjoid2VpeGluXzM5MjY5NzQwIn0.RV9_FW84VdXJM0roKBuzUwglc5DrpFdRA687nRC1gRA; UserName=weixin_39269740; UN=weixin_39269740; c_ref=https%3A//community.modelscope.cn/; c_first_ref=community.modelscope.cn; c_pref=https%3A//community.modelscope.cn/; c_first_page=https%3A//community.modelscope.cn/%3F; c_dsid=11_1679470991475.861447; dc_tos=rrww3z; log_Id_pv=3; _samesite_flag_=true; cookie2=1d1928d7774c4ae1fcdc9391baf7953c; t=cb3738081446cb99b5a4dbf3415aa368; _tb_token_=75ea6a93eb370; csg=ed524eae; m_session_id=d12d527f-624f-40ae-8cf6-bcb76e634ef1; h_uid=2215618637015; log_Id_view=7; log_Id_click=3; isg=BNHR29IOUfrlB73ukpEUmXac4N1rPkWw2OHdS7NIDB9jWqKsT4l7gIf8_C680t3o");

            //Stream writer = request.GetRequestStream();
            //writer.Write(byteData, 0, length);
            //writer.Close();
            //var response = (HttpWebResponse)request.GetResponse();
            //var responseString = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")).ReadToEnd();



            var en = "0";
            if (isen != null && isen != "")
            {
                en = isen;
            }
            int ids = 0;
            if (id != null && id != "")
            {
                ids = Convert.ToInt32(id);
            }
            var uptitle = "";
            var downtitle = "";
            var upid = 0;
            var downid = 0;
            var _papes = "当前页papes:" + papes;
            var strsql = " SELECT id,name, Password, email, phone, city, filter1, filter2, filter3, filter4, filter5, other FROM caiwuTable where id =" + ids;
            var list = GetItemEntityMouthbyid(strsql, en);
            return Json(new { id, list }, JsonRequestBehavior.AllowGet);
        }
        private CoreCmsMouths GetItemEntityMouthbyid(string sqlstr, string filter = "")
        {
            var i = new CoreCmsMouths();
            string conStr = "server=120.26.74.87;user=sa;pwd=Jiutai!@#2022;database=Blog";//连接字符串  
            SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
            conText.Open();//打开数据库  
            string sqls = sqlstr;//创建统计语句  
            SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
            SqlDataReader dr;//创建DataReader对象  
            dr = comText.ExecuteReader();//执行查询  
            while (dr.Read())//判断数据表中是否含有数据  
            {
                var date = dr;
                i.id = Convert.ToInt32(date["id"].ToString());
                i.name = date["name"].ToString();
                i.Password = date["Password"].ToString();
                i.email = date["email"].ToString();
                i.phone = date["phone"].ToString();
                i.city = date["city"].ToString();
                i.filter1 = date["filter1"].ToString();
                i.filter2 = date["filter2"].ToString();
                i.filter3 = date["filter3"].ToString();
                i.other = date["other"].ToString();
            }
            dr.Close();//关闭DataReader对象  
            return i;
        }
        private IList<CoreCmsMouths> GetItemEntityList(string sqlstr, string filter = "")
        {
            var list1 = new List<CoreCmsMouths>();
            string conStr = "server=120.26.74.87;user=sa;pwd=Jiutai!@#2022;database=Blog";//连接字符串  
            SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
            conText.Open();//打开数据库  
            string sqls = sqlstr;//创建统计语句  
            SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
            SqlDataReader dr;//创建DataReader对象  
            dr = comText.ExecuteReader();//执行查询  
            while (dr.Read())//判断数据表中是否含有数据  
            {
                var i = new CoreCmsMouths();
                var date = dr;
                i.id = Convert.ToInt32(date["id"].ToString());
                i.name = date["name"].ToString();
                i.Password = date["Password"].ToString();
                i.email = date["email"].ToString();
                i.phone = date["phone"].ToString();
                i.city = date["city"].ToString();
                i.filter1 = date["filter1"].ToString();
                i.filter2 = date["filter2"].ToString();
                i.filter3 = date["filter3"].ToString();
                i.other = date["other"].ToString();
                list1.Add(i);
            }
            dr.Close();//关闭DataReader对象  
            return list1;
        }
        private string GetIntegralByMonth(string sqlstr, int filter = 0)
        {

            string month = "失败";
            try
            {
                string conStr = "server=120.26.74.87;user=sa;pwd=Jiutai!@#2022;database=Blog";//连接字符串 
                SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                conText.Open();//打开数据库  
                string sqls = sqlstr;//创建统计语句  
                SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                SqlDataReader dr;//创建DataReader对象  
                dr = comText.ExecuteReader();//执行查询  
                //while (dr.Read())//判断数据表中是否含有数据  
                //{
                //    var date = dr;
                //    if (filter == 0)
                //    {
                //        month = date["IntegralByMonth"].ToString();//到店次数
                //    } 
                //    else
                //    {
                //        month = date["AttributeValue"].ToString();//其他       
                //    }
                //}
                month = "成功";
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return month;

        }

        [WriteOperateLog("用户登录", "用户登录Login", "登入模块")]
        public ActionResult ModuleNew(string id, string papes, string cid, string isen)
        {
            var en = "0";
            if (isen != null && isen != "")
            {
                en = isen;
            }
            int ids = 2287;
            if (id != null && id != "")
            {
                ids = Convert.ToInt32(id);
            }
            var uptitle = "";
            var downtitle = "";
            var upid = 0;
            var downid = 0;
            var _papes = "当前页papes:" + papes;
            var strsql = " SELECT id, title, brief, coverImage, contentBody, typeId, sort, isPub, isDel, pv, createTime, updateTime, modular, entitle, enbrief, encontentBody, encreateTime FROM CoreCmsNews where id =" + ids;
            var list = GetItemEntityCorebyid(strsql, en);
            if (ids > 2276)
            {
                upid = ids - 1;
                var uplist = "SELECT id, title, brief, coverImage, contentBody, typeId, sort, isPub, isDel, pv, createTime, updateTime, modular, entitle, enbrief, encontentBody, encreateTime FROM CoreCmsNews where id =" + upid;
                var updata = GetItemEntityCorebyid(uplist, en);
                uptitle = updata.title;
            }
            if (ids < 2312)
            {
                downid = ids + 1;
                var downlist = "SELECT id, title, brief, coverImage, contentBody, typeId, sort, isPub, isDel, pv, createTime, updateTime, modular, entitle, enbrief, encontentBody, encreateTime FROM CoreCmsNews where id =" + downid;
                var _id = "id:" + id;
                var downdata = GetItemEntityCorebyid(downlist, en);

                downtitle = downdata.title;

            }
            if (en == "0")
            { }
            else
            {
                if (list.createTime != null)
                    list.createTime = Convert.ToDateTime(list.createTime).ToString("dd-MMM-yyyy", new System.Globalization.CultureInfo("en-US"));
            }
            return Json(new { uptitle, upid, downtitle, downid, id, list }, JsonRequestBehavior.AllowGet);
        }

        private CoreCmsNews GetItemEntityCorebyid(string sqlstr, string filter = "")
        {
            var i = new CoreCmsNews();
            string conStr = "server=192.168.10.12;user=sa;pwd=123456;database=Blog";//连接字符串
            SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
            conText.Open();//打开数据库  
            string sqls = sqlstr;//创建统计语句  
            SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
            SqlDataReader dr;//创建DataReader对象  
            dr = comText.ExecuteReader();//执行查询  
            while (dr.Read())//判断数据表中是否含有数据  
            {
                var date = dr;
                i.id = Convert.ToInt32(date["id"]);
                i.createTime = Convert.ToDateTime(date["createTime"].ToString() == null ? DateTime.Now.ToString() : date["createTime"].ToString()).ToLongDateString();
                if (filter == "0")
                {
                    i.title = date["title"].ToString();
                    i.brief = date["brief"].ToString();
                    i.contentBody = date["contentBody"].ToString();
                    i.createTime = Convert.ToDateTime(date["createTime"].ToString() == null ? DateTime.Now.ToString() : date["createTime"].ToString()).ToLongDateString();
                }
                else if (filter == "1")
                {
                    i.title = date["entitle"].ToString();
                    i.brief = date["enbrief"].ToString();
                    i.contentBody = date["encontentBody"].ToString();
                    //i.createTime = Convert.ToDateTime(date["encreateTime"].ToString() == null ? DateTime.Now.ToString() : date["encreateTime"].ToString()).ToLongDateString();
                }

                i.coverImage = date["coverImage"].ToString();
                i.typeId = Convert.ToInt32(date["typeId"]);
                i.sort = Convert.ToInt32(date["sort"]);
            }
            dr.Close();//关闭DataReader对象  
            return i;
        }

        [WriteOperateLog("用户登录", "用户登录Login", "登入模块")]
        public ActionResult ModuleNewlist1(string key, string papes, string cid, string isen)
        {
            var en = "0";
            if (isen != null && isen != "")
            {
                en = isen;
            }
            int p = 1;
            if (papes != null && papes != "")
            {
                p = Convert.ToInt32(papes.ToString());
            }
            //var _papes = "当前页papes:" + papes;
            var where = "   where 1 = 1";
            if (key != null && key != "")
            {
                if (en == "0")
                {
                    where = where + "and  ( title like '%" + key + @"%' or  contentBody like '%" + key + @"%')";
                }
                else
                {
                    where = where + "and  ( entitle like '%" + key + @"%' or  encontentBody like '%" + key + @"%')";
                }
            }
            if (cid != null && cid != "")
            {
                where = where + " and typeId = " + Convert.ToInt32(cid.ToString());
            }
            //var _cid = "1 = 法规 ；2=新闻；3：警戒: cid的值为：" + cid;
            var strsql = "SELECT id, title, brief, coverImage, contentBody, typeId, sort, isPub, isDel, pv, createTime, updateTime, modular, entitle, enbrief, encontentBody, encreateTime FROM CoreCmsNews" + where + " order by sort desc";
            var list = GetItemEntityCore11(strsql, en);
            var count = list.Count.ToString();
            //var _key = "查询条件key:" + key;
            list = list.Skip((p - 1) * 5).Take(5).ToList();
            foreach (var item in list)
            {
                if (en == "0")
                { }
                else
                {
                    if (item.createTime != null)
                        item.createTime = Convert.ToDateTime(item.createTime).ToString("dd-MMM-yyyy", new System.Globalization.CultureInfo("en-US"));
                }

            }
            //var hot = await _articleService.Hot(6);
            //var category = await _categoryService.GetRootCategories();
            strsql = "";
            return Json(new { strsql, key, papes, cid, count, list }, JsonRequestBehavior.AllowGet);

        }

        private IList<CoreCmsNews> GetItemEntityCore11(string sqlstr, string filter = "")
        {
            var list1 = new List<CoreCmsNews>();
            string conStr = "server=.;user=sa;pwd=Jiutai!@#2022;database=Blog";//连接字符串  
            SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
            conText.Open();//打开数据库  
            string sqls = sqlstr;//创建统计语句  
            SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
            SqlDataReader dr;//创建DataReader对象  
            dr = comText.ExecuteReader();//执行查询  
            while (dr.Read())//判断数据表中是否含有数据  
            {
                var i = new CoreCmsNews();
                var date = dr;
                i.id = Convert.ToInt32(date["id"]);

                i.createTime = Convert.ToDateTime(date["createTime"].ToString() == null ? DateTime.Now.ToString() : date["createTime"].ToString()).ToLongDateString();
                if (filter == "0")
                {
                    i.title = date["title"].ToString();
                    i.brief = date["brief"].ToString();
                    i.contentBody = date["contentBody"].ToString();
                    i.createTime = Convert.ToDateTime(date["createTime"].ToString() == null ? DateTime.Now.ToString() : date["createTime"].ToString()).ToLongDateString();
                }
                else if (filter == "1")
                {
                    i.title = date["entitle"].ToString();
                    i.brief = date["enbrief"].ToString();
                    i.contentBody = date["encontentBody"].ToString();

                }
                i.coverImage = date["coverImage"].ToString();
                i.typeId = Convert.ToInt32(date["typeId"]);
                i.sort = Convert.ToInt32(date["sort"]);


                list1.Add(i);
            }
            dr.Close();//关闭DataReader对象  
            return list1;
        }
        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <returns></returns>
        [WriteOperateLog("用户登录", "用户登录Login", "登入模块")]
        public ActionResult Login(string account, string pwd, string checkcode, string rememberme)
        {
            var resluts = "imag";


            //string sourcePath = "";
            //Aspose.Words.Document doc = new Aspose.Words.Document(@"D:/Work/test/word/77.docx");
            //doc.Save(@"D:/Work/test/pdf/", SaveFormat.Pdf);


            //WordToPDF(@"D:/Work/test/word/77.docx", @"D:/Work/test/pdf/");
            //string _url = string.Format("https://modelscope.cn/api/v1/models/damo/cv_diffusion_text-to-image-synthesis/widgets/submit");

            //System.Threading.Thread.Sleep(1 * 2 * 1000); //休眠30秒
            //                                             //json参数

            //string key = "狗和人";
            //var valuestr = key;
            //string jsonParam = Newtonsoft.Json.JsonConvert.SerializeObject(new
            //{
            //    task = "text-to-image-synthesis",
            //    inputs = new[]{ valuestr},
            //    parameters = new { tokenizer = "xglm" , batch_size  = "2"},
            //    urlPaths = new { inUrls = new[] { new { value = valuestr, type = "text", displayProps = new { size = "small" }, displayType = "TextArea", name = "text", title = "输入prompt", validator = new { max_words = "75" } } } ,
            //                     outUrls = new[] { new{ fileType = "png-list", outputKey  = "output_imgs" } }
            //    }
            //});
            ////string jsonParam = "\r\n{\"task\":\"text-to-image-synthesis\",\"inputs\":[\"" + valuestr + "\"],\"parameters\":{\"tokenizer\":\"xglm\",\"batch_size\":2},\"urlPaths\":
            ////{\"inUrls\":[{\"value\":\"" + valuestr + "\",\"type\":\"text\",\"displayProps\":{\"size\":\"small\"},\"displayType\":\"TextArea\",\"name\":\"text\",\"title\":\"输入prompt\",\"validator\":
            ////{\"max_words\":\"75\"}}],\"outUrls\":[{\"fileType\":\"png-list\",\"outputKey\":\"output_imgs\"}]}}";
            //var request = (HttpWebRequest)WebRequest.Create(_url);
            //request.Method = "POST";
            //request.ContentType = "application/json;charset=UTF-8";//ContentType
            //request.Headers.Add("cookie", "uuid_tt_dd=11_27076939590-1679470980362-017728; dc_session_id=11_1679470980362.638130; c_segment=0; c_page_id=default; cna=ffxkG9VIjgkCAXBeH6SxVDo+; xlly_s=1; dp_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZCI6MTkwNzAwNywiZXhwIjoxNjgwMDc1Nzg5LCJpYXQiOjE2Nzk0NzA5ODksInVzZXJuYW1lIjoid2VpeGluXzM5MjY5NzQwIn0.RV9_FW84VdXJM0roKBuzUwglc5DrpFdRA687nRC1gRA; UserName=weixin_39269740; UN=weixin_39269740; c_ref=https%3A//community.modelscope.cn/; c_first_ref=community.modelscope.cn; c_pref=https%3A//community.modelscope.cn/; c_first_page=https%3A//community.modelscope.cn/%3F; c_dsid=11_1679470991475.861447; dc_tos=rrww3z; log_Id_pv=3; _samesite_flag_=true; cookie2=1d1928d7774c4ae1fcdc9391baf7953c; t=cb3738081446cb99b5a4dbf3415aa368; _tb_token_=75ea6a93eb370; csg=ed524eae; m_session_id=d12d527f-624f-40ae-8cf6-bcb76e634ef1; h_uid=2215618637015; log_Id_view=7; log_Id_click=3; isg=BNHR29IOUfrlB73ukpEUmXac4N1rPkWw2OHdS7NIDB9jWqKsT4l7gIf8_C680t3o");
            ////request.Headers.Add("Cookie", "BIDUPSID=7891B647FEF86D67510769E8D37EA33E; BAIDUID=F0147F17F26EC14E47BCF2AEFFE12921:FG=1; PSTM=1658661266; BDUSS=JXYVktblhaVTlGZkcxMFlPNGg2aG01MVZ0RDgyY1dXSEYxWTI4R35RUzBCalprSUFBQUFBJCQAAAAAAAAAAAEAAAB-4P9EbHVvZmFtaW5nNAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALR5DmS0eQ5kbG; BDUSS_BFESS=JXYVktblhaVTlGZkcxMFlPNGg2aG01MVZ0RDgyY1dXSEYxWTI4R35RUzBCalprSUFBQUFBJCQAAAAAAAAAAAEAAAB-4P9EbHVvZmFtaW5nNAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALR5DmS0eQ5kbG; MCITY=-257%3A; Hm_lvt_01e907653ac089993ee83ed00ef9c2f3=1679276132; BDSFRCVID=MSkOJeC62GHnemvfyjuGhfuCPvJiZXjTH6aoT49yHx85QsgAUu5oEG0P0U8g0KuM5NlwogKK3gOTH4DF_2uxOjjg8UtVJeC6EG0Ptf8g0f5; H_BDCLCKID_SF=JnI8oCD2JIK3Hnnp5-J25KCQbMDDJT8XbD5jVM7Dbl7keq8CD6jIW4KuhxjIKTjJ2Ict5MJ8WCQsKqc2y5jHhUL4yfTv2-T--27phlvRfnjpsIJMhxFWbT8UQ4cT04r0aKviaKOjBMb1MhbDBT5h2M4qMxtOLR3pWDTm_q5TtUJMeCnTD-Dhe6jWDH8HJ6-jfKQtW4thHDKaJbTp24QEq4tHeNt8-URZ5m7LaUoOtbnEHRQwjjQbM-K0Ln3MBMPj52OnanRsaqo6ED5c5RjbLpFm346-35543bRTLnLy5KJvfJoD3h3ChP-UyN3MWh37Je3lMKoaMp78jR093JO4y4Ldj4oxJpOJ5JbMopCafD8bhDtGjTt-en-W5gTBa4T8bC_X3buQtII28pcNLTDKjx4eD-6BKJcvWJn30t5nLPL-MxoKKlO1j4_eQh642-Lq2Kvr0nT7atTvhl5jDh0K25ksD-4JW45AWHby0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qhja0tJT0ttJ-sL-35HJj2jtJvKn8_-P6MXGjCWMT-0bFH5xJgWx7ke4Te55osbRLd0h3eQpRxJan7_JjOQI-BJqn9e5oGKP-b54tOaUQxtN4j2CnjtpvP8D58hbJobUPUDUJ9LUkJ3gcdot5yBbc8eIna5hjkbfJBQttjQn3hfIkj0DKLJIthhK-GD6A35n-WqxQjaROy-5PX3buQBxOO8pcNLTDKQUkHbfcqKJcBbmv30t5nLPQkjfcyMlO1j4_e3a88hxvOfnTIQb3pQ4KhDp5jDh023jksD-RtW6QwaTvy0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qh-p52f6KetJFf3e; BDORZ=B490B5EBF6F3CD402E515D22BCDA1598; BA_HECTOR=a10h8l25a120ag8ka08080c81i1i5k51n; delPer=0; PSINO=6; ZFY=xeG8XGnodAhNOH37maKvpHgvVS4yWCQShEruX4R6o2w:C; BAIDUID_BFESS=F0147F17F26EC14E47BCF2AEFFE12921:FG=1; BDSFRCVID_BFESS=MSkOJeC62GHnemvfyjuGhfuCPvJiZXjTH6aoT49yHx85QsgAUu5oEG0P0U8g0KuM5NlwogKK3gOTH4DF_2uxOjjg8UtVJeC6EG0Ptf8g0f5; H_BDCLCKID_SF_BFESS=JnI8oCD2JIK3Hnnp5-J25KCQbMDDJT8XbD5jVM7Dbl7keq8CD6jIW4KuhxjIKTjJ2Ict5MJ8WCQsKqc2y5jHhUL4yfTv2-T--27phlvRfnjpsIJMhxFWbT8UQ4cT04r0aKviaKOjBMb1MhbDBT5h2M4qMxtOLR3pWDTm_q5TtUJMeCnTD-Dhe6jWDH8HJ6-jfKQtW4thHDKaJbTp24QEq4tHeNt8-URZ5m7LaUoOtbnEHRQwjjQbM-K0Ln3MBMPj52OnanRsaqo6ED5c5RjbLpFm346-35543bRTLnLy5KJvfJoD3h3ChP-UyN3MWh37Je3lMKoaMp78jR093JO4y4Ldj4oxJpOJ5JbMopCafD8bhDtGjTt-en-W5gTBa4T8bC_X3buQtII28pcNLTDKjx4eD-6BKJcvWJn30t5nLPL-MxoKKlO1j4_eQh642-Lq2Kvr0nT7atTvhl5jDh0K25ksD-4JW45AWHby0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qhja0tJT0ttJ-sL-35HJj2jtJvKn8_-P6MXGjCWMT-0bFH5xJgWx7ke4Te55osbRLd0h3eQpRxJan7_JjOQI-BJqn9e5oGKP-b54tOaUQxtN4j2CnjtpvP8D58hbJobUPUDUJ9LUkJ3gcdot5yBbc8eIna5hjkbfJBQttjQn3hfIkj0DKLJIthhK-GD6A35n-WqxQjaROy-5PX3buQBxOO8pcNLTDKQUkHbfcqKJcBbmv30t5nLPQkjfcyMlO1j4_e3a88hxvOfnTIQb3pQ4KhDp5jDh023jksD-RtW6QwaTvy0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qh-p52f6KetJFf3e; H_PS_PSSID=38185_36558_38410_38349_38307_37862_38174_38290_37922_38425_38314_38383_38285_26350_38420_38283_37881; __bid_n=186fca84a5b9216b5752e1; XFI=bf261800-c796-11ed-9e7c-993a1e2bd1b9; Hm_lpvt_01e907653ac089993ee83ed00ef9c2f3=1679368579; ab_sr=1.0.1_N2YyOTA1NTAyMjI2NmViYzQ3NjMxZjg3OTE4ZTQ3ODg1NGJiNDgxOWZhZjAyYjU5YTk4NDIyMmZlZmI4NGFiYTA5MDg1YzZiOTdiMGNkNzc5NjZlOGI5ZTBiYTY5ZjBkZmZjNTE1NjhkMzExMmRjZGU1MzEyYmNjOTVjYTBiMDhhNjgwMjA0YmQxMjM5M2VjMjRhMGE4ZWZiNmNhZTVkYzViYjMxNGFhYzE4ZDJkMzNkZDBiMmIyMmZlYjI0MmQz; XFCS=EED03EFAACD1A34C9A95B676A5742E986961084A1D66E5C82F7FDBF66FFF6525; XFT=YIBAwYbCQVTX/hBmbFSk3pokc8vmvt9MmWTTIO60MVk=");
            ////request.Headers.Add("Acs-Token", "1679288415966_1679369409945_H2S6+gSGuHdouzSnuYXZt9m6M5kauNTIZXpHl6c5/p+ky0VMWplii+uwH2E+JsD1c0Z2Wu58VGKzVUJYrqlyfr/b7NEqKCVQUbNp47RIRZtwfSNLueUDK/pidv5MvU8cJe8LFzjfAuJ1t+2ujjkn403zZnT+V+6QsuO1P4Mpk3YM+z/ulNVN6hCOdG17KnB9alyB7XcceKCBmghYIbFrICf40Lw10FUqxtGo2QHUgCYOMC/Ev3baG6pNIMrjYdaiGFFjug9Kx7sAS7k8dfjADk7H3W80kuq4d6/AyXELYQ+hylZLQVgzyqa9lDBsd1C26fl102rgY47dLfkjwOpfIw==");
            ////CookieContainer cc = new CookieContainer();
            ////cc.Add(new System.Uri("https://yiyan.baidu.com"), new Cookie("Cookie", "BIDUPSID=7891B647FEF86D67510769E8D37EA33E; BAIDUID=F0147F17F26EC14E47BCF2AEFFE12921:FG=1; PSTM=1658661266; BDUSS=JXYVktblhaVTlGZkcxMFlPNGg2aG01MVZ0RDgyY1dXSEYxWTI4R35RUzBCalprSUFBQUFBJCQAAAAAAAAAAAEAAAB-4P9EbHVvZmFtaW5nNAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALR5DmS0eQ5kbG; BDUSS_BFESS=JXYVktblhaVTlGZkcxMFlPNGg2aG01MVZ0RDgyY1dXSEYxWTI4R35RUzBCalprSUFBQUFBJCQAAAAAAAAAAAEAAAB-4P9EbHVvZmFtaW5nNAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALR5DmS0eQ5kbG; MCITY=-257%3A; Hm_lvt_01e907653ac089993ee83ed00ef9c2f3=1679276132; BDORZ=B490B5EBF6F3CD402E515D22BCDA1598; BA_HECTOR=8h8k002h050g01048k8g840s1i1i3g31n; BDRCVFR[feWj1Vr5u3D]=I67x6TjHwwYf0; delPer=0; PSINO=6; BAIDUID_BFESS=F0147F17F26EC14E47BCF2AEFFE12921:FG=1; ZFY=xeG8XGnodAhNOH37maKvpHgvVS4yWCQShEruX4R6o2w:C; BCLID=4201984001025888909; BCLID_BFESS=4201984001025888909; BDSFRCVID=MSkOJeC62GHnemvfyjuGhfuCPvJiZXjTH6aoT49yHx85QsgAUu5oEG0P0U8g0KuM5NlwogKK3gOTH4DF_2uxOjjg8UtVJeC6EG0Ptf8g0f5; BDSFRCVID_BFESS=MSkOJeC62GHnemvfyjuGhfuCPvJiZXjTH6aoT49yHx85QsgAUu5oEG0P0U8g0KuM5NlwogKK3gOTH4DF_2uxOjjg8UtVJeC6EG0Ptf8g0f5; H_BDCLCKID_SF=JnI8oCD2JIK3Hnnp5-J25KCQbMDDJT8XbD5jVM7Dbl7keq8CD6jIW4KuhxjIKTjJ2Ict5MJ8WCQsKqc2y5jHhUL4yfTv2-T--27phlvRfnjpsIJMhxFWbT8UQ4cT04r0aKviaKOjBMb1MhbDBT5h2M4qMxtOLR3pWDTm_q5TtUJMeCnTD-Dhe6jWDH8HJ6-jfKQtW4thHDKaJbTp24QEq4tHeNt8-URZ5m7LaUoOtbnEHRQwjjQbM-K0Ln3MBMPj52OnanRsaqo6ED5c5RjbLpFm346-35543bRTLnLy5KJvfJoD3h3ChP-UyN3MWh37Je3lMKoaMp78jR093JO4y4Ldj4oxJpOJ5JbMopCafD8bhDtGjTt-en-W5gTBa4T8bC_X3buQtII28pcNLTDKjx4eD-6BKJcvWJn30t5nLPL-MxoKKlO1j4_eQh642-Lq2Kvr0nT7atTvhl5jDh0K25ksD-4JW45AWHby0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qhja0tJT0ttJ-sL-35HJj2jtJvKn8_-P6MXGjCWMT-0bFH5xJgWx7ke4Te55osbRLd0h3eQpRxJan7_JjOQI-BJqn9e5oGKP-b54tOaUQxtN4j2CnjtpvP8D58hbJobUPUDUJ9LUkJ3gcdot5yBbc8eIna5hjkbfJBQttjQn3hfIkj0DKLJIthhK-GD6A35n-WqxQjaROy-5PX3buQBxOO8pcNLTDKQUkHbfcqKJcBbmv30t5nLPQkjfcyMlO1j4_e3a88hxvOfnTIQb3pQ4KhDp5jDh023jksD-RtW6QwaTvy0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qh-p52f6KetJFf3e; H_BDCLCKID_SF_BFESS=JnI8oCD2JIK3Hnnp5-J25KCQbMDDJT8XbD5jVM7Dbl7keq8CD6jIW4KuhxjIKTjJ2Ict5MJ8WCQsKqc2y5jHhUL4yfTv2-T--27phlvRfnjpsIJMhxFWbT8UQ4cT04r0aKviaKOjBMb1MhbDBT5h2M4qMxtOLR3pWDTm_q5TtUJMeCnTD-Dhe6jWDH8HJ6-jfKQtW4thHDKaJbTp24QEq4tHeNt8-URZ5m7LaUoOtbnEHRQwjjQbM-K0Ln3MBMPj52OnanRsaqo6ED5c5RjbLpFm346-35543bRTLnLy5KJvfJoD3h3ChP-UyN3MWh37Je3lMKoaMp78jR093JO4y4Ldj4oxJpOJ5JbMopCafD8bhDtGjTt-en-W5gTBa4T8bC_X3buQtII28pcNLTDKjx4eD-6BKJcvWJn30t5nLPL-MxoKKlO1j4_eQh642-Lq2Kvr0nT7atTvhl5jDh0K25ksD-4JW45AWHby0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qhja0tJT0ttJ-sL-35HJj2jtJvKn8_-P6MXGjCWMT-0bFH5xJgWx7ke4Te55osbRLd0h3eQpRxJan7_JjOQI-BJqn9e5oGKP-b54tOaUQxtN4j2CnjtpvP8D58hbJobUPUDUJ9LUkJ3gcdot5yBbc8eIna5hjkbfJBQttjQn3hfIkj0DKLJIthhK-GD6A35n-WqxQjaROy-5PX3buQBxOO8pcNLTDKQUkHbfcqKJcBbmv30t5nLPQkjfcyMlO1j4_e3a88hxvOfnTIQb3pQ4KhDp5jDh023jksD-RtW6QwaTvy0hvctb3cShPmQMjrDRLbXU6BK5vPbNcZ0l8K3l02V-bIe-t2b6Qh-p52f6KetJFf3e; XFI=d8cdfac0-c78c-11ed-9363-11fb77acfb22; Hm_lpvt_01e907653ac089993ee83ed00ef9c2f3=1679364327; __bid_n=186fca84a5b9216b5752e1; ab_sr=1.0.1_MDhlMzhhZjkyZDJiYWNlOGRhMjdkNjQ1YzczYjkzNDA0YzAyODQ1ZDhhNjNmMDAzZTZkMDM2ZmVhNGEzMzUwZTcwOTc3ODlmMWU0NjgzYzY1OWMyOWE2YTYyMjcxMDA3N2IwNTBkZTEzZGJiMTc4MjM4YjkzOTEwMmFmYjlhNzI4NzA5ZjY4ZjQxMmFiYWJjNDRiN2NjM2ZjNTBlNjQyNGQ0NTFiZGNlYmVkZjc0MWRmMDg5MWJiZTI5ODlhNzlm; XFCS=D4DA282258F7335484898A2AF69DC41EA15E3806075C8AAFEA6DDBE278479D6E; XFT=s25AHfKy5Z9kcvgUrcryF3aCf5jg21c06GlfhkXUHWw=; BDRCVFR[VXHUG3ZuJnT]=mk3SLVN4HKm; H_PS_PSSID=26350"));
            ////cc.Add(new System.Uri("https://yiyan.baidu.com"), new Cookie("token", "1679288415966_1679364661713_iUtUdtoyITmXeBfte7MCnnK8oI/JVoX8qSHD7dEk/qtqgG5DD1JKOgEqQGD+msq4GuE05P//gtrrZjQxk0dwRuFPCSdlrfU9duyrDWkp0PeyFljeboqFVso55q4it+VrbQPUW34pdj80wLEIty8YrBGL7NvlqnmDTly+w/my5pvhAFPr4YD7RzY/xMP//Agyxi/wMj8bJP8rcozzEmWbaLOxslIZPgO5PFN/KBVN/tj9ZYJJDt5xvYk8ZwHHFgA4039fco4g20z32qhEs5JEM4Jtwuyyd7Jas0esf6GC36Af+FGzKL/hMbd3kFS3giu0DNLBz4CEY01UoqqVCSkBgA=="));
            ////request.CookieContainer = cc;
            //byte[] byteData = Encoding.UTF8.GetBytes(jsonParam);
            //int length = byteData.Length;
            //request.ContentLength = length;
            //Stream writer = request.GetRequestStream();
            //writer.Write(byteData, 0, length);
            //writer.Close();
            //var response = (HttpWebResponse)request.GetResponse();
            //var responseString = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")).ReadToEnd();

            //JObject jo = JObject.Parse(responseString);
            //string checkCode = Session["CheckCode"] + "";
            //if (!checkCode.ToLower().Equals(checkcode.ToLower()))//校验码验证
            //{
            //    Session.Remove("CheckCode");    //移除验证码
            //    Response.Write("验证码错误");
            //    Response.End();
            //    return Json(-9);
            //}
            //else
            //{
            try
            {
                HospitalUserEntity entity = personnelManageBLL.UserLogin(account, pwd);
                if (entity.HospitalID == 0)
                {
                    //账户异常
                    return Json(-8);

                }
                //if(entity.HospitalID>0)
                //{
                //    Session["userInfoquanxian"] = entity.VersionName;
                //    if (entity.VersionName == "管理员")
                //    {
                //        var usersmodel = systemBLL.GetUserSettingsModelMemo("管理员",entity.HospitalID);
                //        if (usersmodel.ID==0)
                //        {
                //            UserSettingsEntity user = new UserSettingsEntity();
                //            user.Name = "guanliquanxian";
                //            user.AttributeValue = "2";
                //            user.Memo = "管理员";
                //            user.Type = "radio";
                //            user.HospitalID = entity.HospitalID;
                //            systemBLL.AddUserSettings(user);
                //            user = new UserSettingsEntity();
                //            user.Name = "dianzhangquanxian";
                //            user.AttributeValue = "2";
                //            user.Memo = "店长";
                //            user.Type = "radio";
                //            user.HospitalID = entity.HospitalID;
                //            systemBLL.AddUserSettings(user);
                //            user = new UserSettingsEntity();
                //            user.Name = "qiantaiquanxian";
                //            user.AttributeValue = "2";
                //            user.Memo = "前台";
                //            user.Type = "radio";
                //            user.HospitalID = entity.HospitalID;
                //            systemBLL.AddUserSettings(user);
                //            user = new UserSettingsEntity();
                //            user.Name = "guwenquanxian";
                //            user.AttributeValue = "2";
                //            user.Memo = "顾问";
                //            user.Type = "radio";
                //            user.HospitalID = entity.HospitalID;
                //            systemBLL.AddUserSettings(user);
                //            user = new UserSettingsEntity();
                //            user.Name = "meirongshiquanxian";
                //            user.AttributeValue = "2";
                //            user.Memo = "美容师";
                //            user.Type = "radio";
                //            user.HospitalID = entity.HospitalID;
                //            systemBLL.AddUserSettings(user);

                //        }
                //    }
                //}
                //手机用户不能登录PC
                if (entity.IsAPP == 1)
                {
                    return Json(-7);
                }
                if (entity.OpenActive == 0)
                {
                    return Json(-8);
                }
                //修改最后登录时间
                personnelManageBLL.UpdateLoginTime(entity.UserID);
                //日志记录用
                ViewBag.UserName = entity.UserName;
                ViewBag.UserID = entity.UserID;
                ViewBag.HospitalID = entity.HospitalID;

                /*

                var kucunlist = new List<ProductStockEntity>(); //库存列表
                ProductStockEntity pse = new ProductStockEntity();
                            ProductStockEntity entity1 = new ProductStockEntity();
                            entity1.HospitalID = entity.HospitalID;
                            entity1.ProductID = 68537;         ////
                            entity1.HouseID = 1071; //一个门店一个仓库,所以直接拿来用  ////
                            var list1 = iwareBLL.GetProductStockListToseleect(entity1);
                            if (list1 != null)
                            {
                                foreach (var info in list1)
                                {
                                    kucunlist.Add(info);
                                }
                            }

                if (kucunlist.Count > 0)
                {
                    #region ==新建入库单据==

                    StockOrderEntity sentity = new StockOrderEntity();
                    sentity.BaseID = 357;  ////
                    sentity.HospitalID = entity.HospitalID;
                    sentity.IsVerify = 2;
                    sentity.Memo = "项目产品订单取消入库单据";
                    sentity.OrderNo = "R2020021810699424";
                    sentity.OrderTime = DateTime.Now;
                    sentity.Type = 2;
                    sentity.UserID = entity.UserID;
                    sentity.VerifyTime = DateTime.Now;
                    sentity.VerifyUserID = entity.UserID;
                    sentity.Class = 6;
                    sentity.VerifyMemo = "202002272073928168"; ////
                    sentity.AllMoney = 0;
                    sentity.AllQuatity = (1);
                    sentity.AllChengBen = 0;
                    //添加入库单据
                    var rt = iwareBLL.AddStockOrder(sentity);

                    #endregion

                    #region ==添加单据详情==

                    //添加单据详情
                    StockOrderInfo soinfo = new StockOrderInfo();
                    soinfo.StockOrderNo = sentity.OrderNo;
                    soinfo.Number = (1);
                    soinfo.ProductID = 68537; ////
                    soinfo.HouseID = 1071;    ////
                    iwareBLL.AddStockOrderInfo(soinfo);

                    #endregion

                    #region ==入库操作==

                    if (rt > 0)
                    {
                        StockChangeLogEntity scl = new StockChangeLogEntity();
                        scl.UserID = entity.UserID;
                        scl.LogClass = 8;
                        scl.LogState = 1;
                        scl.OwnedWarehouse = soinfo.HouseID;
                        scl.IntoWarehouse = pse.HouseID;
                        scl.ProductID = 68537;////
                        scl.LogType = "入库";
                        scl.LogTitle = "订单产品订单取消.取消订单编号:" + "202002272073928168" + "; 产品ID:" + "68537" + ";返回数量:" + "1" + ";";////
                        scl.NewQuatity = kucunlist[0].Quatity + (1);
                        scl.OldQuatity = kucunlist[0].Quatity;
                        scl.Quantity = (1);
                        scl.ProductStockID = kucunlist[0].ID;
                        scl.OrderNo = sentity.OrderNo;
                        var sclID = iwareBLL.AddStockChangeLog(scl);
                        //这里需要入库操作
                        pse.ID = kucunlist[0].ID;
                        pse.Quatity = 1;
                        iwareBLL.UpdateProductStock(pse);
                    }
                }

                #endregion
                */
                //获取子门店ID,如果不存在则为本身
                string SonHospitalID = "";
                //
                List<HospitalUserEntity> list = personnelManageBLL.GetSonHospitalID(entity.HospitalID);
                if (list != null && list.Count > 0)
                {
                    foreach (var info in list)
                    {
                        SonHospitalID += info.HospitalID + ",";
                    }
                    entity.SonHospitalID = SonHospitalID.Substring(0, SonHospitalID.Length - 1);
                }
                else
                {
                    entity.SonHospitalID = entity.HospitalID.ToString();
                }

                if (entity != null)
                {
                    RechargeEntity charge = personnelManageBLL.GetRechargeModelByHospitalID(entity.ParentHospitalID);//获取到期时间
                    entity.ParentHospitalName = charge.Name;
                    entity.Price = charge.Price;
                    Session["ExpireTime"] = charge.ExpireTime.Subtract(DateTime.Now).Days;
                    //判断是否要写入cookie
                    if (rememberme == "1")
                    {
                        string resultval = account;
                        CookieHelper.SetCookie("eyblogin", resultval);
                    }

                    Session["userInfo"] = entity;
                    List<UIControlEntity> menulist = new List<UIControlEntity>();

                    if (entity.IsAdmin != 1)
                    {
                        menulist = systemBLL.GetUserMenuByUserID(entity.UserID).Where(c => c.ParentUIControlID == new Guid("BEF5CCF2-18C0-46a6-9B91-E984BEB9BFEF")).ToList();
                    }
                    else
                    {
                        menulist = systemBLL.GetUserMenuByUserID(entity.UserID).Where(c => c.ParentUIControlID == new Guid("BEF5CCF2-18C0-46a6-9B91-E984BEB9BFEF")).ToList();

                        //  menulist = systemBLL.GetMenuList(1, 1).Where(c => c.ParentUIControlID == new Guid("BEF5CCF2-18C0-46a6-9B91-E984BEB9BFEF")).ToList();
                    }
                    Session["listMenu"] = menulist;
                    return Json(true);
                }
            }
            catch (Exception e)
            {
                LogWriter.WriteError(e, "登录错误");
                if (e.Message == "账户过期")
                {
                    return Json(-9);
                }
                return Json(false);
            }

            //}
            return Json(false);

        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePassword()
        {
            return View();
        }
        #endregion


        #region==生成验证码==
        public ActionResult CheckCode()
        {
            string checkCode = GenerateCheckCode();
            Session["CheckCode"] = checkCode;
            CreateCheckCodeImage(checkCode);
            return View();
        }

        /// <summary>
        /// 将文字验证码转化图形验证码。
        /// </summary>
        public void CreateCheckCodeImage(string checkCode)
        {
            if (checkCode == null || checkCode.Trim() == String.Empty)
                return;

            Bitmap image = new Bitmap((int)Math.Ceiling((checkCode.Length * 13.5)), 25);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成随机生成器
                Random random = new Random();
                //清空图片背景色
                g.Clear(Color.White);
                //画图片的背景噪音线
                //for (int i = 0; i < 25; i++)
                //{
                //    int x1 = random.Next(image.Width);
                //    int x2 = random.Next(image.Width);
                //    int y1 = random.Next(image.Height);
                //    int y2 = random.Next(image.Height);
                //    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                //}
                Font font = new Font("Arial", 12, (FontStyle.Bold | FontStyle.Italic));
                var brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(checkCode, font, brush, 2, 2);
                //画图片的前景噪音点
                //for (int i = 0; i < 100; i++)
                //{
                //    int x = random.Next(image.Width);
                //    int y = random.Next(image.Height);
                //    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                //}

                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Green), 1, 0, image.Width - 2, image.Height - 1);
                var ms = new System.IO.MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                Response.ClearContent();
                Response.ContentType = "image/Gif";
                Response.BinaryWrite(ms.ToArray());
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 根据用户IP生成验证码
        /// </summary>
        /// <returns>验证码</returns>
        public string GenerateCheckCode()
        {
            string code = "";
            // 随机数对象
            Random random = new Random();

            for (int i = 0; i < 4; i++)
            {
                // 26: a - z
                int n = random.Next(26);
                // 将数字转换成大写字母
                code += (char)(n + 65);
            }
            return code;
        }
        #endregion



    }
}
