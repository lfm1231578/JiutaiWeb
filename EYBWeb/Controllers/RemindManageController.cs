using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PatientManage.Factory.IBLL;
using BaseinfoManage.Factory.IBLL;
using PersonnelManage.Factory.IBLL;
using HS.SupportComponents;
using EYB.ServiceEntity.PersonnelEntity;
using Webdiyer.WebControls.Mvc;
using EYB.ServiceEntity.CashEntity;
using EYB.ServiceEntity.WarehouseEntity;
using WarehouseManage.Factory.IBLL;
using EYB.ServiceEntity.PatientEntity;
using SystemManage.Factory.IBLL;
using EYB.ServiceEntity.SystemEntity;
namespace EYB.Web.Controllers
{
    public class RemindManageController : BaseController
    {
        #region 依赖注入
        private IPersonnelManageBLL personnelBLL;
        private IBaseinfoBLL baseinfoBLL;//基础信息模块
        private IPatientManageBLL IpatBLL;
        private ISystemManageBLL systemBLL;
        private IWarehouseManageBLL iwareBLL;//仓库管理



        public RemindManageController()
        {
            personnelBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            baseinfoBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IBaseinfoBLL>();
            IpatBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPatientManageBLL>();
            systemBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();
            iwareBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IWarehouseManageBLL>();

        }

        #endregion 依赖注入

        /// <summary>
        /// 公历转为农历的函数
        /// </summary>
        /// <param name="solarDateTime">公历日期</param>
        /// <returns>农历的日期</returns>
        static string SolarToChineseLunisolarDate(DateTime solarDateTime)
        {
            System.Globalization.ChineseLunisolarCalendar cal = new System.Globalization.ChineseLunisolarCalendar();

            int year = cal.GetYear(solarDateTime);
            int month = cal.GetMonth(solarDateTime);
            int day = cal.GetDayOfMonth(solarDateTime);
            int leapMonth = cal.GetLeapMonth(year);
            return string.Format("农历{0}{1}（{2}）年{3}{4}月{5}{6}"
                                , "甲乙丙丁戊己庚辛壬癸"[(year - 4) % 10]
                                , "子丑寅卯辰巳午未申酉戌亥"[(year - 4) % 12]
                                , "鼠牛虎兔龙蛇马羊猴鸡狗猪"[(year - 4) % 12]
                                , month == leapMonth ? "闰" : ""
                                , "无正二三四五六七八九十冬腊"[leapMonth > 0 && leapMonth <= month ? month - 1 : month]
                                , "初十廿三"[day / 10]
                                , "日一二三四五六七八九"[day % 10]
                                );
        }

        #region ==顾客生日提醒==  （提醒管理 - 顾客生日提醒）
        public ActionResult CustomerBirthdaysList()
        {
            int patientBirthdayTimeLong = 3;
            var settinglist = systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
            foreach (var info in settinglist)
            {

                //顾客生日参数
                if (info.Name == "PatientBirthdayTimeLong")
                {
                    patientBirthdayTimeLong = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                }
            }
            try
            {
                var memberno = Request["m"];
                var username = Request["u"];

                var stime = Convert.ToDateTime(Convert.ToDateTime(Request["s"]).ToString("yyyy-MM-dd 00:00:00"));
                var etime = Convert.ToDateTime(Convert.ToDateTime(Request["e"]).ToString("yyyy-MM-dd 23:59:59"));
                if (stime.Year < 1999)
                {
                    stime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
                    //stime = Convert.ToDateTime(DateTime.Now.AddDays(-patientBirthdayTimeLong).ToString("yyyy-MM-dd 00:00:00")); //910
                }

                if (etime.Year < 1999)
                {
                    etime = Convert.ToDateTime(DateTime.Now.AddDays(patientBirthdayTimeLong).ToString("yyyy-MM-dd 23:59:59"));
                    //etime = Convert.ToDateTime(DateTime.Now.AddDays(patientBirthdayTimeLong - 1).ToString("yyyy-MM-dd 23:59:59"));
                    //etime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));  //910
                }
                //var kk = Convert.ToDateTime(SolarToChineseLunisolarDate(stime));
                //var tt = Season.GetLunarCalendar(stime);
                var nle = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(etime).ToString("yyyy-MM-dd 23:59:59")).AddYears(1);
                var nls = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(stime).ToString("yyyy-MM-dd 00:00:00")).AddYears(1);
                if (Convert.ToDateTime(stime.ToString("yyyy-MM-dd 23:59:59")) > Convert.ToDateTime("2021-01-13 00:00:00") && Convert.ToDateTime(stime.ToString("yyyy-MM-dd 23:59:59")) < Convert.ToDateTime("2021-02-10 23:59:59"))
                {

                    nls = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(stime).ToString("yyyy-MM-dd 00:00:00")).AddMonths(1).AddYears(1);
                }
                else
                {
                    nls = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(stime).ToString("yyyy-MM-dd 00:00:00")).AddYears(1);
                }
                if (Convert.ToDateTime(etime.ToString("yyyy-MM-dd 23:59:59")) > Convert.ToDateTime("2021-01-13 00:00:00") && Convert.ToDateTime(etime.ToString("yyyy-MM-dd 23:59:59")) < Convert.ToDateTime("2021-02-10 23:59:59"))
                {

                     nle = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(etime).ToString("yyyy-MM-dd 23:59:59")).AddMonths(1).AddYears(1);
                }
                else {
                    nle = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(etime).ToString("yyyy-MM-dd 23:59:59")).AddYears(1);
                }
                var list = IpatBLL.GetCustomerBirthdaysList(nls, nle, stime, etime, memberno, username, LoginUserEntity.HospitalID);
 
                var result = list.AsQueryable().ToPagedList(1, 5000);
                result.CurrentPageIndex = 1;
                result.TotalItemCount = list.Count();
                if (Request.IsAjaxRequest())
                    return PartialView("_CustomerBirthdaysList", result);
                return View(result);
            }
            catch (Exception)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_CustomerBirthdaysList", new List<PatientEntity>().AsQueryable().ToPagedList(1, 5000));
                }
                else
                {
                    return View(new List<PatientEntity>().AsQueryable().ToPagedList(1, 5000));
                }
            }

        }
        #endregion

        #region ==员工生日提醒==
        public ActionResult PersonnelBirthdaysList()
        {
            int StaffBirthdayTimeLong = 3;
            var settinglist = systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
            foreach (var info in settinglist)
            {
                //员工生日参数
                if (info.Name == "StaffBirthdayTimeLong")
                {
                    StaffBirthdayTimeLong = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                }

            }
            try
            {
                var username = Request["u"];

                var stime = Convert.ToDateTime(Convert.ToDateTime(Request["s"]).ToString("yyyy-MM-dd 00:00:00"));
                var etime = Convert.ToDateTime(Convert.ToDateTime(Request["e"]).ToString("yyyy-MM-dd 23:59:59"));
                if (stime.Year < 1999)
                {
                    //stime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                    stime = Convert.ToDateTime(DateTime.Now.AddDays(-StaffBirthdayTimeLong).ToString("yyyy-MM-dd 00:00:00"));
                }

                if (etime.Year < 1999)
                {
                    //etime = Convert.ToDateTime(DateTime.Now.AddDays(StaffBirthdayTimeLong-1).ToString("yyyy-MM-dd 23:59:59"));
                    etime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
                }
                var nls = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(stime).ToString("yyyy-MM-dd 00:00:00"));
                var nle = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(etime).ToString("yyyy-MM-dd 23:59:59"));
                var list = personnelBLL.SelectHospitalUserList(nls, nle, stime, etime, username, LoginUserEntity.HospitalID);


                var result = list.AsQueryable().ToPagedList(1, 50);
                result.TotalItemCount = list.Count();
                result.CurrentPageIndex = 50;
                if (Request.IsAjaxRequest())
                    return PartialView("_PersonnelBirthdaysList", result);
                return View(result);
            }
            catch (Exception)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_PersonnelBirthdaysList", new List<HospitalUserEntity>().AsQueryable().ToPagedList(1, 5000));
                }
                else
                {
                    return View(new List<HospitalUserEntity>().AsQueryable().ToPagedList(1, 5000));
                }
            }
        }
        #endregion

        #region ==BalanceManage==

        public ActionResult BalanceManage(MainCardBalanceEntity entity)
        {
            entity.numPerPage = 50; //每页显示条数
            entity.HospitalID = LoginUserEntity.HospitalID;

            if (entity.Price == 0) entity.Price = 0.1m;
            if (entity.AllPrice == 0)
            {
                var settings = systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
                foreach (var setting in settings)
                {
                    //储值卡提醒参数
                    if (setting.Name == "ChuzhiCardQuatity")
                    {
                        entity.AllPrice = ConvertValueHelper.ConvertIntValue(setting.AttributeValue);
                        break;
                    }
                }
                //entity.AllPrice = 1000m;
            }
            if (string.IsNullOrWhiteSpace(entity.orderField))
                entity.orderField = "Price";

            int rows = 0;
            int pagecount = 0;
            var list = IpatBLL.GetBalanceManage(entity, out rows, out pagecount);
            var result = list.ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;

            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;

            if (Request.IsAjaxRequest())
                return PartialView("_BalanceList", result);
            return View(result);
        }

        #endregion

        #region ==BalanceTimesManage==
        /// <summary>
        /// 余次不足
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult BalanceTimesManage(MainCardBalanceEntity entity)
        {
            int chuzhiCardQuatity = 3;

            var settinglist = systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
            foreach (var info in settinglist)
            {

                //疗程卡提醒参数
                if (info.Name == "CourseCardQuatity")
                {
                    chuzhiCardQuatity = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                    break;
                }

            }

            if (entity.Tiems == 0)
                entity.Tiems = chuzhiCardQuatity;
            int rows = 0;
            int pagecount = 0;
            entity.numPerPage = 50; //每页显示条数
            entity.HospitalID = LoginUserEntity.HospitalID;
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            entity.Type = 1;
            entity.ExpireTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
            entity.ExpireEndTime = Convert.ToDateTime(DateTime.Now.AddDays(chuzhiCardQuatity).ToString("yyyy-MM-dd 23:59:59"));
            var list = IpatBLL.GetBalanceTimesManage(entity, out rows, out pagecount);
            var result = list.ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_BalanceTimesList", result);
            return View(result);
        }
        /// <summary>
        /// 过期卡项提醒
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult BalanceExpireTimesManage(MainCardBalanceEntity entity)
        {
            int courseCardQuatity = 3;
            //var settinglist = systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
            //foreach (var info in settinglist)
            //{

            //    //疗程卡提醒参数
            //    if (info.Name == "CourseCardQuatity")
            //    {
            //        courseCardQuatity = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
            //        break;
            //    }

            //}
            //汪
            var settinglist = systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
            foreach (var info in settinglist)
            {

                //疗程卡提醒参数
                if (info.Name == "CourseCardQuatity")
                {
                    courseCardQuatity = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                    break;
                }

            }
            //汪
            int rows = 0;
            int pagecount = 0;
            entity.numPerPage = 50; //每页显示条数
            entity.HospitalID = LoginUserEntity.HospitalID;
            if (entity.orderField + "" == "") { entity.orderField = "ExpireTime"; }
            entity.Type = 2;
            entity.ExpireTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
            entity.ExpireEndTime = Convert.ToDateTime(DateTime.Now.AddDays(courseCardQuatity-1).ToString("yyyy-MM-dd 23:59:59"));
            var list = IpatBLL.GetBalanceTimesManage(entity, out rows, out pagecount);

            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = list.Count;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_BalanceExpireTimesList", result);
            return View(result);
        }
        #endregion


        #region ==KucunRemind==

        public ActionResult KucunRemind(ProductStockEntity entity)
        {
            var thishous = 0;
            var hlist = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = LoginUserEntity.HospitalID });
            if (hlist.Count > 0)
            {
                thishous = hlist[0].ID;
            }
            int StockNumber = 3;
            var settinglist = systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
            foreach (var info in settinglist)
            {
                //库存预警参数
                if (info.Name == "StockNumber")
                {
                    StockNumber = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                    break;
                }

            }

            entity.HouseID = entity.HouseID == 0 ? thishous : entity.HouseID;
            entity.Quatity = entity.Quatity == 0 ? StockNumber : entity.Quatity;

            int rows = 0;
            int pagecount = 0;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = IpatBLL.GetKucunRemind(entity, out rows, out pagecount);

            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HouseID = entity.HouseID;
            if (Request.IsAjaxRequest())
                return PartialView("_KucunRemind", result);
            return View(result);
        }

        #endregion

        #region==顾客跟进
        public ActionResult CustomerRemind(CustomertracksEntity entity)
        {
            int PatientFollowDays = 3;
            var settinglist = systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
            foreach (var info in settinglist)
            {

                //顾客生日参数
                if (info.Name == "PatientFollowDays")
                {
                    PatientFollowDays = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                }


            }
            entity.StartTime = entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:00"));
            entity.EndTime = entity.EndTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.AddDays(PatientFollowDays-1).ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.EndTime.AddDays(PatientFollowDays-1).ToString("yyyy-MM-dd 23:59:59"));//天数减去一才是整
            int rows;
            int pagecount;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            //entity.Type = 1;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            if (entity.IsVisit == 0) { entity.IsVisit = -1; }
            if (entity.IsVisit == 2) { entity.IsVisit = 0; }
            var list = IpatBLL.GetCustomertracksList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.StartTime = entity.StartTime.ToString("yyyy-MM-dd hh:MM:ss");
            ViewBag.EndTime = entity.EndTime.ToString("yyyy-MM-dd hh:MM:ss");
            if (Request.IsAjaxRequest())
                return PartialView("_CustomerRemind", result);
            return View(result);
        }


        #endregion

        /// <summary>
        /// 获取提醒个数（铃铛统计）
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRemindCount()
        {
            var list = systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
            int staffBirthdayTimeLong = 3;
            int patientBirthdayTimeLong = 3;
            int stockNumber = 3;
            int patientFollowDays = 3;
            decimal chuzhiCardQuatity = 300;
            int courseCardQuatity = 3;
            foreach (var info in list)
            {
                //员工生日参数
                if (info.Name == "StaffBirthdayTimeLong")
                {
                    staffBirthdayTimeLong = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                }
                //顾客生日参数
                if (info.Name == "PatientBirthdayTimeLong")
                {
                    patientBirthdayTimeLong = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                }
                //回访提醒参数
                if (info.Name == "PatientFollowDays")
                {
                    patientFollowDays = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                }
                //库存预警参数
                if (info.Name == "StockNumber")
                {
                    stockNumber = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                }

                //储值卡提醒参数
                if (info.Name == "ChuzhiCardQuatity")
                {
                    chuzhiCardQuatity = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                }
                //疗程卡提醒参数
                if (info.Name == "CourseCardQuatity")
                {
                    courseCardQuatity = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                }
            }
            //员工生日参数
            var staffBirthdayTimeLongStartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
            var staffBirthdayTimeLongEndTime = Convert.ToDateTime(DateTime.Now.AddDays(staffBirthdayTimeLong-1).ToString("yyyy-MM-dd 23:59:59"));
            var staffBirthdayTimeLongNongliStartTime = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(staffBirthdayTimeLongStartTime).ToString("yyyy-MM-dd 00:00:00"));
            var staffBirthdayTimeLongNongliEndTime = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(staffBirthdayTimeLongEndTime).ToString("yyyy-MM-dd 23:59:59"));

            //顾客生日参数
            //var patientBirthdayTimeLongStartTime = Convert.ToDateTime(DateTime.Now.AddDays(-patientBirthdayTimeLong).ToString("yyyy-MM-dd 00:00:00"));
            var patientBirthdayTimeLongStartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00")); //910
            //var patientBirthdayTimeLongEndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            var patientBirthdayTimeLongEndTime = Convert.ToDateTime(DateTime.Now.AddDays(patientBirthdayTimeLong ).ToString("yyyy-MM-dd 23:59:59"));//910
            var patientBirthdayTimeLongNongliStartTime = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(patientBirthdayTimeLongStartTime).ToString("yyyy-MM-dd 00:00:00"));
            var patientBirthdayTimeLongNongliEndTime = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(patientBirthdayTimeLongEndTime).ToString("yyyy-MM-dd 23:59:59"));

            //if (Convert.ToDateTime(Season.GetLunarCalendarToDatetime(patientBirthdayTimeLongStartTime).ToString("yyyy-MM-dd 00:00:00")).Month > 10 && Convert.ToDateTime(Season.GetLunarCalendarToDatetime(patientBirthdayTimeLongStartTime).ToString("yyyy-MM-dd 00:00:00")).Day > 30)
            if (Convert.ToDateTime(patientBirthdayTimeLongStartTime.ToString("yyyy-MM-dd 23:59:59")) > Convert.ToDateTime("2021-01-13 00:00:00") && Convert.ToDateTime(patientBirthdayTimeLongStartTime.ToString("yyyy-MM-dd 23:59:59")) < Convert.ToDateTime("2021-02-10 23:59:59"))
            {

                patientBirthdayTimeLongNongliStartTime = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(patientBirthdayTimeLongStartTime).ToString("yyyy-MM-dd 00:00:00")).AddMonths(1).AddYears(1);
            }
            else
            {
                patientBirthdayTimeLongNongliStartTime = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(patientBirthdayTimeLongStartTime).ToString("yyyy-MM-dd 00:00:00")).AddYears(1);
            }
            //if (Convert.ToDateTime(Season.GetLunarCalendarToDatetime(patientBirthdayTimeLongEndTime).ToString("yyyy-MM-dd 23:59:59")).Month > 10)
            if (Convert.ToDateTime(patientBirthdayTimeLongEndTime.ToString("yyyy-MM-dd 23:59:59")) > Convert.ToDateTime("2021-01-13 00:00:00") && Convert.ToDateTime(patientBirthdayTimeLongEndTime.ToString("yyyy-MM-dd 23:59:59")) < Convert.ToDateTime("2021-02-10 23:59:59"))
            {

                patientBirthdayTimeLongNongliEndTime = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(patientBirthdayTimeLongEndTime).ToString("yyyy-MM-dd 23:59:59")).AddMonths(1).AddYears(1);
            }
            else
            {
                patientBirthdayTimeLongNongliEndTime = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(patientBirthdayTimeLongEndTime).ToString("yyyy-MM-dd 23:59:59")).AddYears(1);
            }

            //顾客跟进 参数
            var courseCardQuatityStartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
            var courseCardQuatityEndTime = Convert.ToDateTime(DateTime.Now.AddDays(patientFollowDays-1).ToString("yyyy-MM-dd 23:59:59"));

            //疗程卡过期 参数
            var liaochengkaStartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
            var liaochengkaEndTime= Convert.ToDateTime(DateTime.Now.AddDays(courseCardQuatity-1).ToString("yyyy-MM-dd 23:59:59"));

            var houseId = 0;
            var hlist = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = LoginUserEntity.HospitalID });
            if (hlist.Count > 0)
            {
                houseId = hlist[0].ID;
            }
            ViewBag.StaffBirthdayTimeLong = staffBirthdayTimeLong;

            var entity = IpatBLL.GetRemindCount(LoginUserEntity.HospitalID, patientBirthdayTimeLongStartTime, patientBirthdayTimeLongEndTime, patientBirthdayTimeLongNongliStartTime, patientBirthdayTimeLongNongliEndTime, staffBirthdayTimeLongStartTime, staffBirthdayTimeLongEndTime, staffBirthdayTimeLongNongliStartTime, staffBirthdayTimeLongNongliEndTime, courseCardQuatityStartTime,  courseCardQuatityEndTime, liaochengkaStartTime, liaochengkaEndTime, chuzhiCardQuatity, courseCardQuatity, houseId, stockNumber);
            return Json(entity);

        }



        #region 
        // 顾客生日（右下角弹屏）
        public ActionResult CustomerBirthdaysListJson()
        {

            #region 
           
            #endregion

            int patientBirthdayTimeLong = 3;
            var settinglist = systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
            foreach (var info in settinglist)
            {

                //顾客生日参数
                if (info.Name == "PatientBirthdayTimeLong")
                {
                    patientBirthdayTimeLong = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                }
            }
            try
            {
                var memberno = Request["m"];
                var username = Request["u"];

                var stime = Convert.ToDateTime(Convert.ToDateTime(Request["s"]).ToString("yyyy-MM-dd 00:00:00"));
                var etime = Convert.ToDateTime(Convert.ToDateTime(Request["e"]).ToString("yyyy-MM-dd 23:59:59"));
                if (stime.Year < 1999)
                {

                    stime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
                    //stime = Convert.ToDateTime(DateTime.Now.AddDays(-patientBirthdayTimeLong).ToString("yyyy-MM-dd 00:00:00"));   
                }

                if (etime.Year < 1999)
                {
                    //etime = Convert.ToDateTime(DateTime.Now.AddDays(patientBirthdayTimeLong).ToString("yyyy-MM-dd 23:59:59"));
                    //etime = Convert.ToDateTime(DateTime.Now.AddDays(patientBirthdayTimeLong - 1).ToString("yyyy-MM-dd 23:59:59"));
                    etime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
                }

                var nls = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(stime).ToString("yyyy-MM-dd 00:00:00"));
                var nle = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(etime).ToString("yyyy-MM-dd 23:59:59"));
                if (Convert.ToDateTime(stime.ToString("yyyy-MM-dd 23:59:59")) > Convert.ToDateTime("2021-01-13 00:00:00") && Convert.ToDateTime(stime.ToString("yyyy-MM-dd 23:59:59")) < Convert.ToDateTime("2021-02-10 23:59:59"))
                {

                    nls = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(stime).ToString("yyyy-MM-dd 00:00:00")).AddMonths(1).AddYears(1);
                }
                else
                {
                    nls = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(stime).ToString("yyyy-MM-dd 00:00:00")).AddYears(1);
                }
                if (Convert.ToDateTime(etime.ToString("yyyy-MM-dd 23:59:59")) > Convert.ToDateTime("2021-01-13 00:00:00") && Convert.ToDateTime(etime.ToString("yyyy-MM-dd 23:59:59")) < Convert.ToDateTime("2021-02-10 23:59:59"))
                {

                    nle = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(etime).ToString("yyyy-MM-dd 23:59:59")).AddMonths(1).AddYears(1);
                }
                else
                {
                    nle = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(etime).ToString("yyyy-MM-dd 23:59:59")).AddYears(1);
                }
                //stime.ToString().Trim()
                var list = IpatBLL.GetCustomerBirthdaysList(nls, nle, stime, etime, memberno, username, LoginUserEntity.HospitalID);
                list = list.OrderBy(i => i.Birthday).ToList();
                var result = list.AsQueryable().ToPagedList(1, 5000);
                result.CurrentPageIndex = 1;
                result.TotalItemCount = list.Count();
              
                var json1 = Json(result, JsonRequestBehavior.AllowGet);
                return json1;
               

            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_CustomerBirthdaysList", new List<PatientEntity>().AsQueryable().ToPagedList(1, 5000));
                } 
                else
                {
                    return View(new List<PatientEntity>().AsQueryable().ToPagedList(1, 5000));
                } 
            }

        }

        #endregion

        #region 员工
        public ActionResult PersonnelBirthdaysListJson()
        {
         
            int StaffBirthdayTimeLong = 3;
            var settinglist = systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
            foreach (var info in settinglist)
            {
                //员工生日参数
                if (info.Name == "StaffBirthdayTimeLong")
                {
                    StaffBirthdayTimeLong = ConvertValueHelper.ConvertIntValue(info.AttributeValue);
                }

            }
            try
            {
                var username = Request["u"];

                var stime = Convert.ToDateTime(Convert.ToDateTime(Request["s"]).ToString("yyyy-MM-dd 00:00:00"));
                var etime = Convert.ToDateTime(Convert.ToDateTime(Request["e"]).ToString("yyyy-MM-dd 23:59:59"));
                if (stime.Year < 1999)
                {
                    stime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
                }

                if (etime.Year < 1999)
                {
                    etime = Convert.ToDateTime(DateTime.Now.AddDays(StaffBirthdayTimeLong - 1).ToString("yyyy-MM-dd 23:59:59"));
                }
                var nls = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(stime).ToString("yyyy-MM-dd 00:00:00"));
                var nle = Convert.ToDateTime(Season.GetLunarCalendarToDatetime(etime).ToString("yyyy-MM-dd 23:59:59"));
                var list = personnelBLL.SelectHospitalUserList(nls, nle, stime, etime, username, LoginUserEntity.HospitalID);


                var result = list.AsQueryable().ToPagedList(1, 5000);
                result.TotalItemCount = list.Count();
                result.CurrentPageIndex =1;
                var json1 = Json(result, JsonRequestBehavior.AllowGet);
                return json1;
                
            }
            catch (Exception)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_PersonnelBirthdaysList", new List<HospitalUserEntity>().AsQueryable().ToPagedList(1, 5000));
                }
                else
                {
                    return View(new List<HospitalUserEntity>().AsQueryable().ToPagedList(1, 5000));
                }
            }

        }


        #endregion

        #region MyRegion
        public ActionResult espiredJson()
        {
            try {
                int rows, pagecount;
                var entitys = personnelBLL.GetRechargeModelByHospitalID(LoginUserEntity.ParentHospitalID);
               // var entitys = new EYB.ServiceEntity.PersonnelEntity.RechargeEntity();
                entitys.numPerPage = 10000; //每页显示条数   
                if (entitys.orderField + "" == "") { entitys.orderField = "ExpireTime"; entitys.orderDirection = "asc"; }
                var list1 = personnelBLL.GetUserRechargeByHospital(entitys, LoginUserEntity.ParentHospitalID, out rows, out pagecount);
                //var list1 = personnelBLL.GetUserRechargeByHospital(entitys, 1, out rows, out pagecount);
                return Json(list1);
            }
            catch
            {
                return View();
            }


            
        }

        public ActionResult espiredJson1()
        {
            try
            {
                int rows, pagecount;
                var entitys = personnelBLL.GetRechargeModelByHospitalID(LoginUserEntity.ParentHospitalID);
                // var entitys = new EYB.ServiceEntity.PersonnelEntity.RechargeEntity();
                entitys.numPerPage = 10000; //每页显示条数   
                if (entitys.orderField + "" == "") { entitys.orderField = "ExpireTime"; entitys.orderDirection = "asc"; }
                var list1 = personnelBLL.GetRechargeModelExpiration(LoginUserEntity.ParentHospitalID);
                //var list1 = personnelBLL.GetUserRechargeByHospital(entitys, 1, out rows, out pagecount);
                return Json(list1);
            }
            catch
            {
                return View();
            }



        }

        #endregion

    }
}
