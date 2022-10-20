using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SystemManage.Factory.IBLL;
using EYB.ServiceEntity.SystemEntity;
using SystemManage.BLL;
using EYB.Web.Code;
using EYB.ServiceEntity.PersonnelEntity;
using PersonnelManage.Factory.IBLL;
using BaseinfoManage.Factory.IBLL;

namespace EYB.Web.Controllers
{
    public class HomeController : BaseController
    {
        #region 依赖注入

        private ISystemManageBLL systemBLL;
        private IPersonnelManageBLL personnelManageBLL;
        private IBaseinfoBLL baseinfoBLL;
        public HomeController()
        {
            systemBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();
            personnelManageBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            baseinfoBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IBaseinfoBLL>();
        }

        #endregion 依赖注入

        public ActionResult Index()
        {
            ViewBag.UserID = LoginUserEntity.UserID;
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return View();
        }
        public ActionResult Error500()
        {

            return View();
        }
        public ActionResult Error400()
        {

            return View();
        }
        public ActionResult Help()
        {
            var sysSetting = baseinfoBLL.GetSysSettingEntity();
            return View(sysSetting);
        }
        public ActionResult Charge()
        {
            ViewBag.ParentHospitalName = LoginUserEntity.ParentHospitalName;
            ViewBag.Price = LoginUserEntity.Price;

            RechargeEntity charge = personnelManageBLL.GetRechargeModelByHospitalID(LoginUserEntity.ParentHospitalID);//获取到期时间
           ViewBag.ExpireTime = charge.ExpireTime;
          ViewBag.ParentHospitalID=  LoginUserEntity.ParentHospitalID;
            return View();
        }

        /// <summary>
        /// 获取门店二维码
        /// </summary>
        /// <returns></returns>
        public JsonResult GetHospitalWxQrCde()
        {
            var result = personnelManageBLL.GetWxBindByHospItalID(LoginUserEntity.ParentHospitalID).QrCode;
            return Json(result);
        }
    }
}
