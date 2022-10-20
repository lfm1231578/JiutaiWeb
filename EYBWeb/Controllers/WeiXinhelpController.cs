using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common.Helper;
using HS.SupportComponents;
using PatientManage.Factory.IBLL;
using ThoughtWorks.QRCode.Codec;
using PersonnelManage.Factory.IBLL;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using EYB.ServiceEntity.PatientEntity;
using System.Configuration;

namespace EYB.Web.Controllers
{

    public class WeiXinhelpController : BaseController
    {
        #region ==依赖注入==

        private IPatientManageBLL patientBLL;
        private IPersonnelManageBLL personnelBLL;

        public WeiXinhelpController()
        {
            patientBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPatientManageBLL>();//顾客
            personnelBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
        }

        #endregion

        //
        // GET: /WeiXinhelp/

        private static string Access_token;

        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 顾客--微信绑定
        /// </summary>
        /// <returns></returns>
        public ActionResult WeiXinBind()
        {
            var _user = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["UserID"]);
            var UserModel = patientBLL.GetPatienttEntityByID(_user);

            var Ipkey="";
            try
            {
                Ipkey = ConfigurationManager.AppSettings["IP"].ToString();
            }
            catch (Exception)
            {
                ViewBag.url = "/Uploads/QRCode/NoBind.png";
                //没有绑定数据
                UserModel.SessionID = "0";
                //return Json(ex,JsonRequestBehavior.AllowGet);
               return View(UserModel);
                
            }
             
            var wx = personnelBLL.GetWxBindByHospItalID(LoginUserEntity.ParentHospitalID);
            if(wx.ID==0)
            {
                ViewBag.url = "/Uploads/QRCode/NoBind.png";
                //没有绑定数据
                UserModel.SessionID = "0";
              
                return View(UserModel);
            }            
            else
            {
            
            
            var userkey = (UserModel.UserID + 2016) * 5;
            string statrstr =userkey.ToString();
            statrstr += "a" + Common.Helper.WeiXinMessage.UpStr(UserModel.UserKey.ToString().Substring(0, 6)) + "a" + wx.ID + "a4799663";


            var url = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_userinfo&state={2}#wechat_redirect", wx.AppID, wx.BindUrl + "/WeiXin/ParientBindingWeixin", statrstr);

            LogWriter.WriteInfo("地址：" + url, "绑定地址,用户编号："+UserModel.UserID);                                                
            QRCodeHandler qr = new QRCodeHandler();
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Uploads\QRCode\";    //文件目录                      //二维码字符串  
            string logoFilePath = path + "myLogo.jpg";                                    //Logo路径50*50  
            string filePath = path +_user+ "myCode.jpg";                                        //二维码文件名  
            qr.CreateQRCode(url, "Byte", 8, 0, "H", filePath, false, null);   //生成 
            ViewBag.ClickUrl = url;
            ViewBag.url = "/Uploads/QRCode/"+_user+"myCode.jpg?v=" + RandomHelper.CreateVersion();
            UserModel.SessionID = "1";
            return View(UserModel);
            }
                                                        
            
        }

        /// <summary>
        /// 微信测试页面
        /// </summary>
        /// <returns></returns>
        public ActionResult WeiXintest()
        {
            // new WeiXinMessage().SendConsumerMessage("201609275988495503");
            //var appid = System.Configuration.ConfigurationManager.AppSettings["AppID"];
            //var secret = System.Configuration.ConfigurationManager.AppSettings["AppSecret"];
            ////var token = System.Configuration.ConfigurationManager.AppSettings["Access_token"];
            //var TempID=System.Configuration.ConfigurationManager.AppSettings["TempID"];
            //if(string.IsNullOrEmpty(Access_token))
            //{
            //    //var token = TemplateMessageAPI.GetToken(appid, secret);
            //    //Access_token = token;
            //    Access_token = "ltL2tLmpbTIq0PRhPcNSb7oDluT1Dt0iZ-HKSGc--oivPi-FKwTwaQDyzWQtB-WGTmf8lwU_IqNIV-zNTFzxn2lGD40CvHkwKPyOnSzTQCuUHLvBNgTO4TlC78Zm4LDgLNGfAAAZBU";
            //}
            //var UserOpenID = "o1PN5uDqHwJ_lmEUxeI9DIFq4-Z4";//用户OPENID
            //var TemplateID = "xGNWR4oEwqNEi3ws6NkRU9uSeNu1ldnr7hxzN2WBpGM";//模板ID
            //var Url = "http://www.yimee.net";
            //var Title = "你在时代店的消费成功!";
            //var productType = "消费内容";
            //var name = "氨基酸活性洁面膏,清肠排油疗法,活力绽放疗法,铂蒂补水霜";
            //var Price = "1980元";
            //var time = "2016年9月26号";
            //var remark = "感谢你的消费,欢迎下次光临";

            //string strPostData = "{\"touser\":\"" + UserOpenID + "\",\"template_id\":\"" + TemplateID + "\",\"url\":\"" + Url + "\",\"data\":{\"first\":{\"value\":\"" + Title + "\",\"color\":\"#173177\"},\"productType\":{\"value\":\"" + productType + "\",\"color\":\"#173177\"},\"name\":{\"value\":\"" + name + "\",\"color\":\"#173177\"},\"accountType\":{\"value\":\"金额\",\"color\":\"#173177\"},\"account\":{\"value\":\"" + Price + "\",\"color\":\"#173177\"},\"time\":{\"value\":\"" + time + "\",\"color\":\"#173177\"},\"remark\":{\"value\":\"" + remark + "\",\"color\":\"#173177\"}}}";

            //var result = TemplateMessageAPI.SendTemplateMessage(Access_token, strPostData);

            //梳理顾客管理的档案变更的选择卡项的疗程卡列表相关接口数据逻辑（完成）
            //梳理提醒管理及提醒合计及列表相关数据及错误数据筛选条件下接口（完成）

            return View();
        }


        /// <summary>
        /// 员工--微信绑定
        /// </summary>
        /// <returns></returns>
        public ActionResult WeiXinBindForYuangong()
        {
            var _user = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["UserID"]);
            var UserModel = personnelBLL.GetUserByUserID(_user);
            var Ipkey = "";
            try
            {
                Ipkey = ConfigurationManager.AppSettings["IP"].ToString();
            }
            catch (Exception)
            {
                ViewBag.url = "/Uploads/QRCode/NoBind.png";
                //没有绑定数据
                UserModel.SessionID = "0";
                return View(UserModel);

            }

            var wx = personnelBLL.GetWxBindByHospItalID(LoginUserEntity.ParentHospitalID);
            if (wx.ID == 0)
            {
                ViewBag.url = "/Uploads/QRCode/NoBind.png";
                //没有绑定数据
                UserModel.SessionID = "0";
                return View(UserModel);
            }
            else
            {
                var userkey = (UserModel.UserID + 2016) * 5;
                string statrstr = userkey.ToString();
                statrstr += "a" + wx.ID + "a" + Ipkey;
                var url = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_userinfo&state={2}#wechat_redirect", wx.AppID, wx.BindUrl + "/WeiXin/HospitalUserBindingWeixin", statrstr);

                QRCodeHandler qr = new QRCodeHandler();
                string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Uploads\QRCodeHospitalUser\";    //文件目录                      //二维码字符串  
                string logoFilePath = path + "myLogo.jpg";                                    //Logo路径50*50  
                string filePath = path + _user + "myCode.jpg";                                        //二维码文件名  
                qr.CreateQRCode(url, "Byte", 8, 0, "H", filePath, false, null);   //生成 

                ViewBag.url = "/Uploads/QRCodeHospitalUser/" + _user + "myCode.jpg?v=" + RandomHelper.CreateVersion();
                UserModel.SessionID = "1";
                return View(UserModel);
            }


        }


    }
}