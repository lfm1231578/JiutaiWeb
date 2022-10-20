using System;
using System.Web.Mvc;
using HS.SupportComponents;
using System.Globalization;
using System.IO;
using BaseinfoManage.BLL;
using EYB.ServiceEntity.PersonnelEntity;
using PersonnelManage.BLL;
using PersonnelManage.Factory.IBLL;
using BaseinfoManage.Factory.IBLL;

namespace EYB.Web.Controllers
{
    /// <summary>
    /// 所有的Controllers都继承于该类
    /// </summary>

    public class BaseController : Controller
    {
        protected HospitalUserEntity LoginUserEntity;

        private IPersonnelManageBLL PersonnelManageBll;
        private IBaseinfoBLL BaseinfoBll;
        public BaseController()
        {
            LoginUserEntity = new HospitalUserEntity();
            PersonnelManageBll = new PersonnelManageBLL();
            BaseinfoBll = new BaseinfoBLL();
        }
        /// <summary>
        /// Action请求执行前
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            CheckUser();
        }
        /// <summary>
        /// 系统异常日志
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnException(ExceptionContext filterContext)
        {
            LogWriter.WriteError(filterContext.Exception, "系统出现异常");
            //Tracker.ErrorTrack(filterContext.Exception, "系统异常");
        }


        /// <summary>
        /// 检查用户数据
        /// </summary>
        /// <returns></returns>
        public void CheckUser()
        {
            try
            {
                HospitalUserEntity loginUserInfo = Session["userInfo"] as HospitalUserEntity;// LoginUserManager.GetLoginInfo();
                if (loginUserInfo == null || loginUserInfo.UserID == 0)//如果session丢失则重新登陆
                {
                    throw new Exception();
                }

                var entity = PersonnelManageBll.GetUserByUserID(loginUserInfo.UserID);
                if (entity == null || entity.UserID <= 0 || entity.IsActive == 0 || entity.Password != loginUserInfo.Password)
                {
                    throw new Exception();
                }

                Session["userInfo"] = LoginUserEntity = entity;
                Session.Timeout = 180;//设置过期时间

                Session["UserID"] = ViewBag.UserID = entity.UserID;
                Session["HospitalID"] = ViewBag.HospitalID = entity.HospitalID;
                ViewBag.UserName = entity.UserName;
                ViewData["UserID"] = entity.UserID;
                Session["HospitalName"] = ViewBag.OwenHospitalName = entity.Name;//门店名称

                ViewBag.Photo = entity.Photo;//照片
                ViewBag.IsAdmin = entity.IsAdmin;
                if (string.IsNullOrEmpty(loginUserInfo.SoftName))
                {
                    var sysSetting = BaseinfoBll.GetSysSettingEntity();
                    loginUserInfo.SoftName = sysSetting.SoftName;
                    ViewBag.SoftName = loginUserInfo.SoftName;
                    loginUserInfo.Domain = sysSetting.Domain;
                    ViewBag.Domain = loginUserInfo.Domain;
                    loginUserInfo.PCIndexImgUrl = sysSetting.PCIndexImgUrl;
                    ViewBag.PCIndexImgUrl = loginUserInfo.PCIndexImgUrl;
                    ViewData["SoftName"] = loginUserInfo.SoftName;
                }
                else
                {
                    ViewBag.SoftName = loginUserInfo.SoftName;
                    ViewBag.Domain = loginUserInfo.Domain;
                    ViewBag.PCIndexImgUrl = loginUserInfo.PCIndexImgUrl;

                }

                if (string.IsNullOrEmpty(loginUserInfo.Photo))
                {
                    loginUserInfo.Photo= PersonnelManageBll.GetWxBindByHospItalID(loginUserInfo.ParentHospitalID).QrCode;
                    ViewBag.Photo = loginUserInfo.Photo;
                }
                else
                {
                    ViewBag.Photo = loginUserInfo.Photo;
                }


            }
            catch
            {
                Response.Write(string.Format("<script  type=\"text/javascript\">top.location=\"{0}\"</script>", Url.Action("LoginPage", "Account")));
                Response.End();
            }
        }
        //弹出框
        public void ShowMessage(string message)
        {
            Response.Write(string.Format("<script>alert('{0}')</script>", message));
        }
        //成功页面重写
        public ActionResult SuccessDirect(string message, string url_forward = "", int ms = 2, string dialog = "a", int statu = 1)
        {
            if (url_forward != "")
            {
                ViewBag.jumpUrl = url_forward;
            }
            else
            {
                ViewBag.jumpUrl = Request.UrlReferrer;
            }
            ViewBag.waitSecond = ms;
            ViewBag.dialog = dialog;
            ViewBag.Message = message;
            //if (statu == 1)
            //{
            //    ViewBag.waitSecond = 1;
            //}
            //else
            //{
            //    ViewBag.waitSecond = 3;
            //}
            return View("Success");
        }



        /// <summary>
        /// 文件上传--按日期建立文件夹
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public string BufferToDiskByDate()
        {

            var path = Server.MapPath("~/Uploads");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            //每月创建1个文件夹
            var ymd = DateTime.Now.ToString("yyyyMM", DateTimeFormatInfo.InvariantInfo);
            path += "/" + ymd + "/";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (string file in Request.Files)
            {
                var fileBase = Request.Files[file];
                try
                {
                    if (fileBase != null && fileBase.ContentLength > 0)
                    {
                        var fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + fileBase.FileName.Substring(fileBase.FileName.LastIndexOf("."));
                        //保存文件
                        fileBase.SaveAs(path + "/" + fileName);

                        return "/Uploads/" + ymd + "/" + fileName;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return "";
        }


    }
}
