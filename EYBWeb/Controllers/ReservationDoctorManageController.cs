using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReservationDoctorManage.Factory.IBLL;
using PatientManage.Factory.IBLL;
using EYB.ServiceEntity.ReservationEntity;
using EYB.ServiceEntity.PatientEntity;
using Webdiyer.WebControls.Mvc;
using HS.SupportComponents;
using Common.Helper;
using PersonnelManage.Factory.IBLL;
using EYB.ServiceEntity.PersonnelEntity;
using EYB.ServiceEntity.SystemEntity;
using System.Text;
using CashManage.Factory.IBLL;
using EYB.ServiceEntity.MoneyEntity;
using SystemManage.Factory.IBLL;
using CenterManage.Factory.IBLL;
using EYB.ServiceEntity.CashEntity;
using Newtonsoft.Json;
using SystemManage.Factory.IDAL;
using System.Data.SqlClient;
using DepartmentManage.Factory.IBLL;

namespace EYB.Web.Controllers
{
    public class ReservationDoctorManageController : BaseController
    {
        #region 依赖注入
        private IReservationDoctorManageBLL reserBLL;
        private IPatientManageBLL patientBLL;
        private IPersonnelManageBLL personBLL;
        private ICashManageBLL cashBll;
        private ISystemManageBLL IsystemBLL;//系统模块
        private ICenterManageBLL iCentBll;//管理中心
        private ISystemManageDAL sysbll; //系统管理
        private IDepartmentManageBLL DepartmentManageBLL;
        public ReservationDoctorManageController()
        {
            reserBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IReservationDoctorManageBLL>();
            patientBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPatientManageBLL>();//顾客
            personBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            cashBll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<CashManage.Factory.IBLL.ICashManageBLL>();
            IsystemBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();
            iCentBll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ICenterManageBLL>();
            sysbll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer()
    .Resolve<ISystemManageDAL>();
            DepartmentManageBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IDepartmentManageBLL>();
        }
        #endregion 依赖注入

        #region==Get请求==
        /// <summary>
        /// 预约就诊管理页面
        /// </summary>
        /// <returns></returns>
        public ActionResult ReservationDoctorPage()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        public ActionResult SalaryStatisticsFilterShopManagerInfo()
        {
            return View();
        }
        /// <summary>
        /// 预约管理控件
        /// </summary>
        /// <returns></returns>
        public ActionResult _ReservationControl()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }
        public ActionResult _ReservationPatientList()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }
        public ActionResult _SearchResultList()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }
        /// <summary>
        /// 今日预约
        /// </summary>
        /// <returns></returns>
        public ActionResult ReservationNew()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 就诊详情
        /// </summary>
        /// <returns></returns>
        public ActionResult ReservationDetail()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 本周预约
        /// </summary>
        /// <returns></returns>
        public ActionResult ReservationWeek()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 本月预约
        /// </summary>
        /// <returns></returns>
        public ActionResult ReservationMonth()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }
        /// <summary>
        /// 获取月度预约
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult GetReservationMonth(PatientPerformanceGoalsEntity entity)
        {
            var persoList = reserBLL.GetPatientPerformanceGoalsListMonth(entity);
            StringBuilder persoListstr1 = new StringBuilder();

            persoListstr1.AppendFormat("<tr><th style='width: 60px;'>员工</th>");
            persoListstr1.AppendFormat("<th style='width: 60px;'>顾客</th>");
            persoListstr1.AppendFormat("<th style='width: 60px;'>目标</th>");
            persoListstr1.AppendFormat("<th style='width: 60px;'>达成</th>");

            int greenCount = 0;
            int blueCount = 0;
            int redCount = 0;

            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));


            //获取当前月份的天数
            for (var i = 1; i <= days; i++)
            {
                if (i == 7 || i == 10 || i == 15 || i == 21)
                {
                    persoListstr1.AppendFormat("<th style='padding: 0 5px;color:peru;'>{0}<br/>检查</th>", i);
                }
                else
                {
                    persoListstr1.AppendFormat("<th style='padding: 0 5px;'>{0}</th>", i);
                }



            }
            persoListstr1.AppendFormat("</tr>");
            int DaodianCount = 0;
            int daodianlistcount = 0;
            foreach (var info in persoList)
            {
                var Dates = entity.Years + "-" + entity.Months + "-01" + " 00:00:01";
                var DatesEnd = DateTimeHelper.LastDayOfMonth(ConvertValueHelper.ConvertDateTimeValueNew(Dates)).ToString("yyyy-MM-dd 23:59:59");
                var daodianlist = cashBll.GetOnedayOrderList(info.UserID, Dates, DatesEnd);
                DaodianCount = DaodianCount + info.DaodianCount;
                daodianlistcount = daodianlistcount + daodianlist;
                persoListstr1.AppendFormat("<tr><td style='text-align: center;border-right:1px solid #ccc;'>{0}</td>", !string.IsNullOrEmpty(info.HospitalUserName) ? info.HospitalUserName : info.GenjinUserName);
                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;'>{0}</td>", info.UserName);
                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;'>{0}</td>", info.DaodianCount);
                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;'>{0}</td>", daodianlist);
                for (var i = 1; i <= days; i++)
                {
                    //日期组装
                    var Dates1 = entity.Years + "-" + entity.Months + "-" + i + " 00:00:01";
                    var DatesEnd1 = entity.Years + "-" + entity.Months + "-" + i + " 23:59:59";
                    var Dates11 = entity.Years + "-" + entity.Months + "-" + i;
                    var daodianlist1 = cashBll.GetOnedayOrderList(info.UserID, Dates1, DatesEnd1);
                    var list = reserBLL.SelectReservationList(entity.HospitalID, Dates11, info.UserID);
                    if (daodianlist1 > 0 && list.Count > 0)
                    {
                        greenCount++;
                        persoListstr1.AppendFormat("<td style='background :green;border-right:1px solid #ccc;'><div style='position: relative; width: 100%; height: 70px;background :green;'></div></td>");
                    }
                    else if (list.Count > 0 && daodianlist1 == 0)
                    {
                        blueCount++;
                        persoListstr1.AppendFormat("<td style='background :blue;border-right:1px solid #ccc;'><div style='position: relative; width: 100%; height: 70px;background :blue;'></div></td>");

                    }
                    else if (daodianlist1 > 0 && list.Count == 0)

                    {
                        redCount++;
                        persoListstr1.AppendFormat("<td style='border-right:1px solid #ccc;'><div style='position: relative; width: 100%; height: 70px;background :red; '></div></td>");

                    }
                    else if (i == 7 || i == 10 || i == 15 || i == 21)
                    {
                        persoListstr1.AppendFormat("<td style='border-right:1px solid #ccc;'><div style='position: relative; width: 100%; height: 70px; '></div></td>");
                    }
                    else
                    {
                        persoListstr1.AppendFormat("<td style='border-right:1px solid #ccc;'><div style='position: relative; width: 100%; height: 70px;'></div></td>");
                    }
                }
                persoListstr1.AppendFormat("</tr>");
            }
            persoListstr1.AppendFormat("<tr><td></td><td style='text-align:center;line-height:20px;'>合计</td><td  style='text-align:center;'>{0}</td><td  style='text-align:center;'>{1}</td></tr>", DaodianCount, daodianlistcount);
            persoListstr1.AppendFormat("<tr style='height: 35px; line-height:35px; '><td colspan='14' style='line-height:20px;' ><span style='color:green;'>绿色块 :{0}</span>   <span style='color:blue;'> 蓝色块 : {1}</span>   <span style='color:red;'> 红色块: {2}</span></td></tr>", greenCount, blueCount, redCount);
            return Content(persoListstr1.ToString());
        }
        /// <summary>
        /// 顾客目标查询界面
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientPerformanceGoals(PatientPerformanceGoalsEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            int rows;
            int pagecount;
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.Years == 0)
            {
                entity.Years = DateTime.Now.Year;
            }
            if (entity.Months == 0)
            {
                entity.Months = DateTime.Now.Month;
            }
            entity.numPerPage = 10000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "UserID"; }
            var list = reserBLL.GetPatientPerformanceGoalsListResult(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_PatientPerformanceGoals", result);
            return View(result);

        }
        /// <summary>
        /// 顾客目标设置界面
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientPerformanceGoalsSet(PatientPerformanceGoalsEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            int rows;
            int pagecount;
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.Years == 0)
            {
                entity.Years = DateTime.Now.Year;
            }
            if (entity.Months == 0)
            {
                entity.Months = DateTime.Now.Month;
            }

            entity.numPerPage = 10000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "UserID"; }
            var list = reserBLL.GetPatientPerformanceGoalsList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_PatientPerformanceGoalsSet", result);
            return View(result);

        }
        /// <summary>
        /// 保存目标设置结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SavePatientPerformanceGoals(PatientPerformanceGoalsEntity entity)
        {
            string[] UserIDList = Request.Form.GetValues("UserID");//1、用户编号
            string[] DaodianCountList = Request.Form.GetValues("DaodianCount");//1、到店次数
            int result = 0;
            if (UserIDList != null)
            {
                for (int j = 0; j < UserIDList.Length; j++)
                {
                    if (DaodianCountList != null && DaodianCountList[j] != "" && DaodianCountList[j] != "0")
                    {
                        entity.UserID = Convert.ToInt32(UserIDList[j]);
                        entity.DaodianCount = ConvertValueHelper.ConvertIntValue(DaodianCountList[j]);
                        result = reserBLL.AddPatientPerformanceGoals(entity);
                    }
                }
            }
            return Json(result);
        }
        public ActionResult ReservationPatientList(PatientEntity entity)
        {
            int rows;
            int pagecount;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.numPerPage = 9; //每页显示条数
            entity.s_StareTime = Convert.ToDateTime("1970-01-01");
            entity.s_Endtime = Convert.ToDateTime("1970-01-01");


            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<PatientEntity> list = new List<PatientEntity>();
            if (entity.IsActive == 0)
            {
                entity.IsActive = 1;
            }
            if (HasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = patientBLL.GetPatientList(entity, out rows, out pagecount);
                list = list.Where(c => c.IsShow != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = patientBLL.GetPatientList(entity, out rows, out pagecount);
            }
            list = list.Where(t => t.IsDel != 1).ToList();
            // var list = patientBLL.GetPatientList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);

            if (HasPermission1)//模拟账号
            {
                result.TotalItemCount = list.Count;
            }
            else
            {
                result.TotalItemCount = rows;
            }
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_ReservationPatientList", result);
            return View(result);

        }
        public ActionResult ReservationReVisit(ReservationEntity entity)
        {

            int rows;
            int pagecount;
            entity.UserID = ConvertValueHelper.ConvertIntValue(Request["UserID"]);
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }

            entity.numPerPage = 30; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            var list = reserBLL.GetReservationListByHPid(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_ReservationReVisit", result);
            return View(result);
        }

        /// <summary>
        /// 预约列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ReservationList(ReservationEntity entity)
        {
            int rows;
            int pagecount;
            if (entity.orderField + "" == "") { entity.orderField = "ApplyTime"; }
            entity.numPerPage = 50;
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            var now = DateTime.Now.ToString("yyyy-MM");
            int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            if (entity.StartTime == null)
            {
                //entity.StartTime = string.Format(now,"-1");
                entity.StartTime = now + "-1";
            }
            if (entity.EndTime == null)
            {
                //entity.EndTime = string.Format(now,"-",days);
                entity.EndTime = now + "-" + days;
            }
            var list = reserBLL.GetReservationListByCondition(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            //var aList = result.Where(item => Convert.ToDateTime(item.ApplyTime).ToString("yyyy-mm") == DateTime.Now.ToString("yyyy-mm")).ToList();

            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_ReservationList", result);
            return View(result);
        }
        #endregion

        #region==Ajax请求==
        /// <summary>
        /// 修改预约状态
        /// </summary>
        /// <param name="Statu"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult UpReservationStatu(int Statu, int ID)
        {
            return Json(reserBLL.UpReservationStatu(Statu, ID));
        }
        /// <summary>
        /// 护理完成
        /// </summary>
        /// <param name="Statu"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult UpReservationStatu1(int Statu, int ID, int UserID,int IsInitiative)
        {
            if (LoginUserEntity.HospitalID == 2 || LoginUserEntity.HospitalID == 1017)
            {
                if (IsInitiative==1)
                {
                    var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "YuyuejiFen"); //获取积分实体
                    ViewBag.YuyuejiFen = usersetting.AttributeValue;
                    var intrgral1 = patientBLL.GetIntegralModel(UserID);//获取原来的积分列表
                    if (Statu == 3)
                    {
                        intrgral1.AllIntegral += Convert.ToInt32(usersetting.AttributeValue);
                        intrgral1.Integral += Convert.ToInt32(usersetting.AttributeValue);
                    }
                    patientBLL.UpdateIntegral(intrgral1);
                    IntegralRecordEntity ir = new IntegralRecordEntity();
                    ir.UserID = UserID;
                    ir.DocumentNumber = ID.ToString();
                    ir.Integral = Convert.ToDecimal(usersetting.AttributeValue);
                    ir.CreateTime = DateTime.Now;
                    ir.IntegralOperationID = LoginUserEntity.HospitalID;
                    patientBLL.AddIntegralRecord(ir);//增加积分记录
                }

            }
            return Json(reserBLL.UpReservationStatu(Statu, ID));
            // return View();
        }
        /// <summary>
        /// 取消预约
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public ActionResult DelReservation(int ID, long UserID)
        {
            return Json(reserBLL.DelReservation(ID, UserID, LoginUserEntity.HospitalID));
        }

        /// <summary>
        /// 修改预约时间和预约科室
        /// 需要先判断当天是否有此顾客的非当前ID的预约记录
        /// </summary>
        /// <returns></returns>
        public ActionResult UpReservationDate(DateTime Date, int ID, int departID, string StartTime, string EndTime)
        {
            var model = reserBLL.ReservationEntityByID(ID);
            var list = reserBLL.SelectReservationList(LoginUserEntity.HospitalID, Convert.ToDateTime(Date).ToString("yyyy-MM-dd"), Convert.ToInt32(model.UserID));
            var count = list.Where(i => i.ID != ID).Count();
            if (count == 0)
            {
                return Json(reserBLL.UpReservationDate(Date, ID, departID, StartTime, EndTime));
            }
            else
            {
                return Json(-1);
            }

        }
        /// <summary>
        /// 添加预约
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddReservation(ReservationEntity entity)
        {
            // var list = personBLL.GetDate(entity.ReservationUserID, entity.ApplyTime.Value.ToString("yyyy-MM"));
            //if (list.Contains(entity.ApplyTime.Value.ToString("yyyy-MM-dd")))//如果是假期，则不能预约
            //{
            //    return Json(-3);
            //}
            //if (entity.UserID == 0)//如果顾客不存在，则添加顾客信息
            //{
            //    return Json(-4);

            //}

            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.CreateTime = DateTime.Now;
            entity.Statu = 1;
            entity.IsActive = 1;

            //entity.IsInitiative,
            long result = 0;
            //修改预约
            if (entity.ID > 0)
            {
                result = reserBLL.UpdateReservation(entity);
            }
            else
            {
                //添加预约
                if (IsConflict(entity.StartTime, entity.EndTime, entity.ReservationUserID, Convert.ToDateTime(entity.ApplyTime)))
                {
                    result = reserBLL.AddReservation(entity);
                }
            }
            return Json(result);
        }

        public bool IsConflict(string stime, string etime, int userid, DateTime appdate)
        {
            DateTime st = Convert.ToDateTime(stime);
            DateTime et = Convert.ToDateTime(etime);
            ReservationEntity entity = new ReservationEntity();
            int rows;
            int pagecount;
            if (entity.orderField + "" == "") { entity.orderField = "ApplyTime"; }
            entity.numPerPage = 50000;
            var list = reserBLL.GetReservationListByCondition(entity, out rows, out pagecount);
            list = list.Where(i => i.ReservationUserID == userid).ToList();
            list = list.Where(i => i.ApplyTime == appdate).ToList();
            foreach (var info in list)
            {
                if (Convert.ToDateTime(info.StartTime) > et)
                {
                    return false;
                }
                if (Convert.ToDateTime(info.EndTime) < st)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 添加预约
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddReservationAgain(ReservationEntity entity)
        {
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.CreateTime = DateTime.Now;
            entity.Statu = 1;
            entity.IsActive = 1;
            var list = personBLL.GetDate(entity.ReservationUserID, entity.ApplyTime.Value.ToString("yyyy-MM"));
            if (list.Contains(entity.ApplyTime.Value.ToString("yyyy-MM-dd")))//如果是假期，则不能预约
            {
                return Json(-3);
            }

            long result = reserBLL.AddReservation(entity);

            return Json(result);
        }


        /// <summary>
        /// 搜索结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SearchResult(ReservationEntity entity)
        {

            if (entity.UserName == "搜索") entity.UserName = "";
            int rows;
            int pagecount;
            entity.numPerPage = 10; //每页显示条数
            entity.HospitalID = LoginUserEntity.HospitalID;
            if (String.IsNullOrEmpty(entity.orderField)) { entity.orderField = "CreateTime"; }
            var list = reserBLL.GetReservationList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.PatientName = entity.UserName;
            if (Request.IsAjaxRequest())
                return PartialView("_SearchResultList", result);
            return View(result);
        }
        /// <summary>
        /// 初诊预约
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ReservationFirst(ReservationEntity entity)
        {

            int rows;
            int pagecount;
            entity.numPerPage = 10; //每页显示条数
            entity.HospitalID = LoginUserEntity.HospitalID;
            if (String.IsNullOrEmpty(entity.orderField)) { entity.orderField = "CreateTime"; }
            var list = reserBLL.GetReservationFristList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.name = entity.UserName;
            if (Request.IsAjaxRequest())
                return PartialView("_ReservationFirstList", result);
            return View(result);

        }
        public ActionResult _ReservationFirstList()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return View();
        }

        public ActionResult ReservationWorkDay()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }

        #region==回访记录

        public ActionResult AddReservationRevisit(string Memo, int ReservationID, int UserID)
        {
            return Json(reserBLL.AddReservationRevisit(new ReservationRevisitEntity { Memo = Memo, CreateUserID = LoginUserEntity.UserID, UserID = UserID, ReservationID = ReservationID }));
        }
        public ActionResult ReservationReVisitDetail()
        {
            return View();
        }

        #endregion
        #endregion

        #region==排班设置==

        /// <summary>
        /// 排班表
        /// </summary>
        /// <returns></returns>
        public ActionResult SchedulingPage()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 保存假期
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SaveScheduling(SchedulingEntity entity)
        {
            //第一步 添加排班 如果存在则先删除后添加
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.Type = 1;
            var RestDay = Request.Form.GetValues("RestDay");
            string indicationstr = "";
            if (RestDay != null && RestDay.Length > 0)
            {
                foreach (var info in RestDay)
                {
                    if (!String.IsNullOrEmpty(info))
                        indicationstr += info + ",";
                }
                entity.RestDay = indicationstr.Substring(0, indicationstr.Length - 1);
            }

            var result = personBLL.SaveScheduling(entity);
            //第二步 添加请假记录
            if (Request.Form["addqjHoliday"] == "1")
            {
                DateTime dt1 = new DateTime();
                DateTime dt2 = new DateTime();
                ConvertValueHelper.ConvertDateTimeValue(Request["StartTime"], out dt1);
                ConvertValueHelper.ConvertDateTimeValue(Request["StartTime"], out dt2);
                entity.StartTime = dt1;
                entity.StartTime = dt2;
                entity.Memo = Request["Memo"];
                entity.Type = 2;
                personBLL.SaveScheduling(entity);
            }
            //第三步 添加调休记录
            if (Request.Form["addtxHoliday"] == "1")
            {
                DateTime dt1 = new DateTime();
                DateTime dt2 = new DateTime();
                ConvertValueHelper.ConvertDateTimeValue(Request["StartTime1"], out dt1);
                ConvertValueHelper.ConvertDateTimeValue(Request["StartTime1"], out dt2);
                entity.StartTime = dt1;
                entity.StartTime = dt2;
                entity.Memo = Request["Memo1"];
                entity.Type = 3;
                personBLL.SaveScheduling(entity);
            }
            if (result > 0)
            {
                return Json(1);
            }
            else
            {
                return Json(-1);
            }

        }
        /// <summary>
        /// 假期记录
        /// </summary>
        /// <returns></returns>
        public ActionResult SchedulingHoliday(int Type, int UserID)
        {
            int rows = 0;

            SchedulingEntity entity = new SchedulingEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.Type = Type;
            entity.UserID = UserID;
            entity.orderField = "CreateTime";
            entity.orderDirection = "desc";
            entity.numPerPage = 10;
            var list = personBLL.SelectScheduList(entity, out rows);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            return View(result);
        }

        /// <summary>
        /// 获取排班记录
        /// </summary>
        /// <returns></returns>
        public ActionResult SelectSchedulingHoliday(int Type, int UserID)
        {
            int rows = 0;

            SchedulingEntity entity = new SchedulingEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.Type = Type;
            entity.UserID = UserID;
            entity.orderField = "CreateTime";
            entity.orderDirection = "desc";
            entity.numPerPage = 10;
            var list = personBLL.SelectScheduList(entity, out rows);
            JsonModelEntity info = new JsonModelEntity();
            if (list != null && list.Count > 0)
            {
                info.OutputHtml = GetWeekDay(list[0].RestDay);
                info.OutputString = GetSelectDay(list[0].RestDay);
                return Json(info);
            }
            else
            {
                return Json(info);
            }
        }
        /// <summary>
        /// 根据数字获取日期
        /// </summary>
        /// <returns></returns>
        public string GetWeekDay(string day)
        {
            if (!String.IsNullOrEmpty(day))
            {
                return day.Replace("1", "星期一").Replace("2", "星期二").Replace("3", "星期三").Replace("4", "星期四").Replace("5", "星期五").Replace("6", "星期六").Replace("0", "星期日");
            }
            return "";
        }

        /// <summary>
        /// 获取下拉框的值
        /// </summary>
        /// <returns></returns>
        public string GetSelectDay(string day)
        {
            string str = "";
            if (!String.IsNullOrEmpty(day))
            {
                foreach (var en in day.Split(','))
                {
                    StringBuilder strbuilder = new StringBuilder();
                    strbuilder.Append("<div class='selectdiv' style='min-width: 120px; float: left;'>");
                    strbuilder.Append("<select name='RestDay' class='RestDay select'>");
                    strbuilder.AppendFormat("<option {0} value='1'>星期一</option>", en == "1" ? "selected=selected" : "");
                    strbuilder.AppendFormat("<option {0} value='2'>星期二</option>", en == "2" ? "selected=selected" : "");
                    strbuilder.AppendFormat("<option {0} value='3'>星期三</option>", en == "3" ? "selected=selected" : "");
                    strbuilder.AppendFormat("<option {0} value='4'>星期四</option>", en == "4" ? "selected=selected" : "");
                    strbuilder.AppendFormat("<option {0} value='5'>星期五</option>", en == "5" ? "selected=selected" : "");
                    strbuilder.AppendFormat("<option {0} value='6'>星期六</option>", en == "6" ? "selected=selected" : "");
                    strbuilder.AppendFormat("<option {0} value='0'>星期日</option>", en == "0" ? "selected=selected" : "");
                    strbuilder.Append("</select><a href='javascript:;' class='deletedepart' style='color: Red;'></a></div>");
                    str += strbuilder.ToString();
                }
            }
            return str;
        }
        #endregion

        #region==排班管理==

        public ActionResult PaibanManage()
        {
            return View();
        }
        /// <summary>
        /// 排班设置
        /// </summary>
        /// <returns></returns>
        public ActionResult PaibanSet()
        {
            var list = reserBLL.GetPaiBanSetList(new PaiBanSetEntity() { HospitalID = LoginUserEntity.HospitalID });
            return View(list);
        }
        /// <summary>
        /// 排班列表
        /// </summary>
        /// <returns></returns>
        public ActionResult PaibanList()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            //获取排班
            //获取所有的员工
            var persoList = personBLL.GetAllUserByHospitalID(LoginUserEntity.HospitalID, 0);
            StringBuilder str = new StringBuilder();
            str.Append("<thead style='position: absolute' ><tr><th>员工编号</th><th>员工姓名</th>");
            //获取本月天数
            DateTime dtNow = DateTime.Now;
            int days = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            for (int i = 1; i <= days; i++)
            {
                str.AppendFormat("<th>{0}</th>", i);
            }
            str.Append("</tr></thead>");

            str.Append("<tbody>");
            str.Append("<tr style='height:80px;'></tr>");
            foreach (var info in persoList)
            {
                str.AppendFormat(" <tr><td>{0}</td><td>{1}</td>", info.UserID, info.UserName);

                var list = reserBLL.GetPaiBanListByUserID(new PaiBanEntity { UserID = info.UserID, Month = DateTime.Now.ToString("yyyy-MM") });
                list = list.Where(x => x.HospitalID == LoginUserEntity.HospitalID).ToList();
                foreach (var mod in list)
                {
                    str.AppendFormat("<td>{0}</td>", mod.Name);
                }
                str.Append("</tr>  ");

            }
            str.Append("</tbody>");
            ViewBag.Html = str.ToString();
            return View();
        }


        /// <summary>
        /// 查询排班
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPaibanInfoList(PaiBanEntity entity)
        {

            //获取排班
            //获取所有的员工
            var persoList = personBLL.GetAllUserByHospitalID(entity.HospitalID, 0);
            StringBuilder str = new StringBuilder();
            str.Append("<thead><tr><th>员工编号</th><th>员工姓名</th>");
            //获取本月天数
            DateTime dtNow = Convert.ToDateTime(entity.Month);
            int days = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            for (int i = 1; i <= days; i++)
            {
                str.AppendFormat("<th>{0}</th>", i);
            }
            str.Append("</tr></thead>");

            str.Append("<tbody>");
            foreach (var info in persoList)
            {
                str.AppendFormat(" <tr><td>{0}</td><td>{1}</td>", info.UserID, info.UserName);

                var list = reserBLL.GetPaiBanListByUserID(new PaiBanEntity { UserID = info.UserID, Month = entity.Month });
                foreach (var mod in list)
                {
                    str.AppendFormat("<td>{0}</td>", String.IsNullOrEmpty(mod.Name) ? "<label style='color:green;'>休息</label>" : mod.Name);
                }
                str.Append("</tr>  ");

            }
            str.Append("</tbody>");
            return Content(str.ToString());
        }
        /// <summary>
        /// 添加排班页面
        /// </summary>
        /// <returns></returns>
        public ActionResult PaibanSetAdd()
        {
            var ID = Request["ID"];
            var entity = reserBLL.GetPaiBanSetModel(ConvertValueHelper.ConvertIntValue(ID));
            return View(entity);
        }

        /// <summary>
        /// 添加排班
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPaibanSet(PaiBanSetEntity entity)
        {
            int result = 0;
            if (entity.ID > 0) //编辑
            {
                result = reserBLL.UpdatePaiBanSet(entity);
            }
            else
            {
                //添加
                entity.HospitalID = LoginUserEntity.HospitalID;
                result = reserBLL.AddPaiBanSet(entity);
            }
            return Json(result);
        }
        /// <summary>
        /// 添加排班表
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPaibanPage()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 保存排班记录
        /// </summary>
        /// <returns></returns>
        public ActionResult SavePaiban(string userarr, string paibanarr, string txtMonth)
        {
            if (!String.IsNullOrEmpty(userarr))
            {
                string[] useridlist = userarr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] paibanlist = paibanarr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var info in useridlist)
                {
                    foreach (var mod in paibanlist)
                    {
                        string[] res = mod.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        reserBLL.DeletePaiBan(new PaiBanEntity { UserID = ConvertValueHelper.ConvertIntValue(info), Month = txtMonth, MonthDay = ConvertValueHelper.ConvertIntValue(res[0]), HospitalID = LoginUserEntity.HospitalID });
                        reserBLL.AddPaiBan(new PaiBanEntity { UserID = ConvertValueHelper.ConvertIntValue(info), Month = txtMonth, MonthDay = ConvertValueHelper.ConvertIntValue(res[0]), PaiBanSetID = ConvertValueHelper.ConvertIntValue(res[1]), HospitalID = LoginUserEntity.HospitalID });
                    }
                }
                return Json(1);
            }
            else
            {
                return Json(0);
            }
        }
        /// <summary>
        /// 获取员工的排班情况
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public ActionResult GetPaibanList(PaiBanEntity entity)
        {
            var list = reserBLL.GetPaiBanListByUserID(entity);

            StringBuilder str = new StringBuilder();
            list = list.Where(x => x.HospitalID == LoginUserEntity.HospitalID).ToList();
            //获取班次
            var Paibanlist = reserBLL.GetPaiBanSetList(new EYB.ServiceEntity.ReservationEntity.PaiBanSetEntity() { HospitalID = LoginUserEntity.HospitalID }).Where(c => c.Statu == 1).ToList();

            string[] colorstr = { "#f5940b", "#15d415", "#0fbaec", "#0d83e8", "#ef0b07", "#730aec" };
            if (list.Count != 0)
            {
                foreach (var info in list)
                {
                    int j = -1;
                    str.AppendFormat("<tr code='{0}' codevalue='{1}'>", info.MonthDay, info.PaiBanSetID);
                    str.AppendFormat("<td style='border: 1px solid #ccc;'  disabled='disabled'>{0}</td>", info.MonthDay);
                    foreach (var mode in Paibanlist)
                    {
                        j = j + 1;
                        if (j > 5)
                        {
                            j = 5;
                        }
                        var color = j < 6 ? colorstr[j] : colorstr[j - (j * (int)j % 6) - 1];
                        str.AppendFormat(" <td style='border: 1px solid #ccc; background-color:{2}' code='{0}'  color='{1}'></td>", mode.ID, color, info.PaiBanSetID == mode.ID ? color : "");

                    }
                    str.Append("</tr>");
                }
            }
            else
            {
                DateTime dtNow = Convert.ToDateTime(entity.Month);
                int days = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);
                for (int i = 1; i <= days; i++)
                {
                    int j = -1;
                    str.AppendFormat("<tr code='{0}' codevalue='{1}'>", i, 0);
                    str.AppendFormat("<td style='border: 1px solid #ccc;'  disabled='disabled'>{0}</td>", i);
                    foreach (var mode in Paibanlist)
                    {
                        j = j + 1;
                        if (j > 5) {
                            j = 5;
                        }
                        str.AppendFormat(" <td style='border: 1px solid #ccc;' code='{0}'  color='{1}'></td>", mode.ID, colorstr[j]);

                    }
                    str.Append("</tr>");
                }
            }
            return Content(str.ToString());
        }


        /// <summary>
        /// 获取月份天数
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public ActionResult GetPaibanListbyMonth(PaiBanEntity entity)
        {
            var list = reserBLL.GetPaiBanListByUserID(entity);
            //获取本月天数
            DateTime dtNow = Convert.ToDateTime(entity.Month);
            int days = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);


            StringBuilder str = new StringBuilder();
            //获取班次
            var Paibanlist = reserBLL.GetPaiBanSetList(new EYB.ServiceEntity.ReservationEntity.PaiBanSetEntity() { HospitalID = LoginUserEntity.HospitalID }).Where(c => c.Statu == 1).ToList();
            string[] colorstr = { "#f5940b", "#15d415", "#0fbaec", "#0d83e8", "#ef0b07", "#730aec" };
            for (int i = 1; i <= days; i++)
            {
                int j = -1;
                str.AppendFormat("<tr code='{0}' codevalue='{1}'>", i, 0);
                str.AppendFormat("<td style='border: 1px solid #ccc;'  disabled='disabled'>{0}</td>", i);
                foreach (var mode in Paibanlist)
                {
                    j = j + 1;
                    str.AppendFormat(" <td style='border: 1px solid #ccc;' code='{0}'  color='{1}'></td>", mode.ID, colorstr[j]);

                }
                str.Append("</tr>");
            }
            return Content(str.ToString());
        }
        /// <summary>
        /// 添加预约   
        /// </summary>
        /// <returns></returns>
        public ActionResult ReservationAdd()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        #endregion


        #region==员工打卡记录


        /// <summary>
        /// 打卡记录
        /// </summary>
        /// <returns></returns>
        public ActionResult DakaRecord()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            //ViewBag.Years = Request["Years"];
            //ViewBag.Years = Request["Years"];
            return View();
        }
        /// <summary>
        /// 添加打卡记录
        /// </summary>
        /// <returns></returns>
        public ActionResult DakaAdd()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        public ActionResult DakaAdd1()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }

        /// <summary>
        /// 添加薪资行政扣罚和分数记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddDaka1(DakaRecordEntity entity)
        {
            int UserId = Convert.ToInt32(Request["UserId"]);

            DateTime statetime = Convert.ToDateTime(entity.StartTime);
            DateTime endtime = Convert.ToDateTime(entity.EndTime);
            var years = statetime.Year;
            var months = statetime.Month;
            var day = statetime.Day;
            if (LoginUserEntity.ParentHospitalID == 0)
            {
                return Json(-99);
            }
            var daka = reserBLL.SelectDaka(LoginUserEntity.HospitalID, years, months, day, UserId);
            int result = 0;
            if (daka.Id > 0)
            {
                result = reserBLL.UpdateDakaRecord(new DakaRecordEntity()
                {
                    Id = daka.Id,
                    StartTime = entity.StartTime,
                    EndTime = entity.EndTime,
                    StartStatuName = "正常打卡",
                    EndStatuName = "正常打卡",
                    Months = months,
                    Years = years,
                    Day = day,
                    UserID = entity.UserID
                });

            }
            else
            {
                result = reserBLL.AddDakaRecord(new DakaRecordEntity()
                {
                    HospitalID = LoginUserEntity.HospitalID,
                    Months = months,
                    Years = years,
                    Day = day,
                    StartTime = entity.StartTime,
                    EndTime = entity.EndTime,
                    StartStatuName = "正常打卡",
                    EndStatuName = "正常打卡",
                    UserID = entity.UserID
                });
            }
            return Json(result);
        }
        /// <summary>
        /// 添加打卡记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddDaka(DakaRecordEntity entity)
        {
            int UserId = Convert.ToInt32(Request["UserId"]);

            DateTime statetime = Convert.ToDateTime(entity.StartTime);
            DateTime endtime = Convert.ToDateTime(entity.EndTime);
            var years = statetime.Year;
            var months = statetime.Month;
            var day = statetime.Day;
            if (LoginUserEntity.ParentHospitalID == 0)
            {
                return Json(-99);
            }
            var daka = reserBLL.SelectDaka(LoginUserEntity.HospitalID, years, months, day, UserId);
            int result = 0;
            if (daka.Id > 0)
            {
                result = reserBLL.UpdateDakaRecord(new DakaRecordEntity()
                {
                    Id = daka.Id,
                    StartTime = entity.StartTime,
                    EndTime = entity.EndTime,
                    StartStatuName = "正常打卡",
                    EndStatuName = "正常打卡",
                    Months = months,
                    Years = years,
                    Day = day,
                    UserID = entity.UserID
                });

            }
            else
            {
                result = reserBLL.AddDakaRecord(new DakaRecordEntity()
                {
                    HospitalID = LoginUserEntity.HospitalID,
                    Months = months,
                    Years = years,
                    Day = day,
                    StartTime = entity.StartTime,
                    EndTime = entity.EndTime,
                    StartStatuName = "正常打卡",
                    EndStatuName = "正常打卡",
                    UserID = entity.UserID
                });
            }
            return Json(result);
        }

        public ActionResult DakaRecordStatistics(DakaRecordEntity entity)
        {

            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;

            }

            if (entity.Years == 0)
            {
                entity.Years = DateTime.Now.Year;
            }

            if (entity.Months == 0)
            {
                entity.Months = DateTime.Now.Month;
            }
            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Years = entity.Years;
            ViewBag.Months = entity.Months;
            var list = personBLL.GetAllUser(new HospitalUserEntity { IsActive = 1, HospitalID = entity.HospitalID });
            list = list.Where(i => i.IsSys != 1 && i.IsAPP != 1).ToList();
            var _yingxianggongxiu = "0";
            try
            {
                if (!string.IsNullOrEmpty(entity.HospitalID.ToString())) { 
                    int Inqdays =  Convert.ToInt32(entity.HospitalID.ToString());
                    string strsql = @"select top 10 YingXiangGongXiu from EYB_tb_Hospital WHERE ID =  " + Convert.ToInt32(Inqdays) + "";
                    _yingxianggongxiu = GetIntegralByMonth(strsql, 1);
                }
            }
            catch
            {
            }
            if(string.IsNullOrEmpty(_yingxianggongxiu)) _yingxianggongxiu = "0";
            List<DakaRecordEntity> newlist = new List<DakaRecordEntity>();
            //var chaoxiu = days - Convert.ToInt32(_yingxianggongxiu);
            //chaoxiu = chaoxiu > 0 ? chaoxiu : 0;
            foreach (var info in list)
            {
                var mod = reserBLL.SelectKaoqingtongji(new DakaRecordEntity()
                { UserID = info.UserID, Years = entity.Years, Months = entity.Months });
                mod.Chaoxiuday = (mod.XiuxiDay - Convert.ToInt32( _yingxianggongxiu))> 0? (mod.XiuxiDay - Convert.ToInt32(_yingxianggongxiu)):0;
                mod.Yingxianggongxiu = Convert.ToInt32(_yingxianggongxiu);
                mod.UserName = info.UserName;
                newlist.Add(mod);
            }

            ViewBag.days = days;
            return View(newlist);
        }
        private string GetIntegralByMonth(string sqlstr, int filter = 0)
        {

            string month = "0";
            try
            {
                string conStr = "server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft112";//连接字符串  
                SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                conText.Open();//打开数据库  
                string sqls = sqlstr;//创建统计语句  
                SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                SqlDataReader dr;//创建DataReader对象  
                dr = comText.ExecuteReader();//执行查询  
                while (dr.Read())//判断数据表中是否含有数据  
                {
                    var date = dr;
                    month =  date["YingXiangGongXiu"].ToString();
                }
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return month;

        }
        /// <summary>
        /// 员工打卡记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult DakaMonth(DakaRecordEntity entity, int operareType = 0)
        {

            var mengdianyuangong = personBLL.GetAllUserByHospitalID(entity.HospitalID, 0);
            // var persoList = reserBLL.GetUserByHospitalIDListMonth(entity).GroupBy(c => c.UserID).Select(g => g.First()).ToList();
            StringBuilder persoListstr1 = new StringBuilder();
            persoListstr1.AppendFormat("<table border='0'>");
            persoListstr1.AppendFormat("<tr  class='ymxkaoqingTr'><th style='height:22px;'>{0}月考勤</th>", entity.Months);
            //persoListstr1.AppendFormat("<tr style='background-color:#3bafda;height:40px;color:white;'><th style='width:60px'>{0}月考勤</th>", entity.Months);

            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            //获取当前月份的天数
            for (var i = 1; i <= days; i++)
            {
                persoListstr1.AppendFormat("<th > {0}</th>", i);
                //persoListstr1.AppendFormat("<th style='width: 35px ;color: #fff;'> {0}</th>", i);
            }
            persoListstr1.AppendFormat("</tr>");

            foreach (var info in mengdianyuangong)
            {
                //      str.AppendFormat(" <tr><td>{0}</td><td>{1}</td>", info.UserID, info.UserName);
                persoListstr1.AppendFormat("<tr><td style='text-align:center;border-right:1px solid #ccc;width:25px;border-bottom:1px dashed #ccc;height:45px;'>{0}</td>", info.UserName);

                for (var i = 1; i <= days; i++)
                {
                    //   var Dates = entity.Years + "-" + entity.Months + "-" + i;
                    var years = entity.Years;
                    var months = entity.Months;
                    var day = i;
                    var daka = reserBLL.SelectDaka(entity.HospitalID, years, months, day, info.UserID);
                    var qingjia = reserBLL.SelectQingjiaList(entity.HospitalID, years, months, day, info.UserID);
                    int count = 0;
                    if (qingjia.Count > 0)
                    {
                        foreach (var item in qingjia)
                        {
                            count += 1;
                            if (item.leixing == "请假")
                            {
                                if (qingjia.Count != 1)
                                {
                                    if (count == 1)
                                    {
                                        persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>", item.category, item.category);
                                    }
                                }
                                else
                                {
                                    if (item.AfternoonTime == "am")
                                    {
                                        DateTime dt = ConvertValueHelper.ConvertDateTimeValueNew(daka.EndTime);
                                        string hm2 = dt.ToString("HH:mm");
                                        persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px'>{1}</div></td>", item.category, hm2);
                                    }
                                    else if (item.AfternoonTime == "pm")
                                    {
                                        DateTime dd = ConvertValueHelper.ConvertDateTimeValueNew(daka.StartTime);
                                        string hm = dd.ToString("HH:mm");
                                        persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>", hm, item.category);
                                    }
                                }
                            }
                            else if (item.leixing == "休息")
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:green;'>{0}</div><div style='padding-top:5px;color:green;'>{1}</div></td>", "休息", "休息");
                            }
                        }
                    }
                    else
                    {
                        if (daka.Id > 0)
                        {
                            DateTime dd = ConvertValueHelper.ConvertDateTimeValueNew(daka.StartTime);
                            string hm = dd.ToString("HH:mm");
                            DateTime dt = ConvertValueHelper.ConvertDateTimeValueNew(daka.EndTime);
                            string hm2 = dt.ToString("HH:mm");
                            if (daka.StartTime == null)
                            {
                                hm = "00:00";
                            }
                            if (daka.EndTime==null)
                            {
                                hm2 = "00:00";
                            }
                            if (hm == "00:00")
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px'>{1}</div></td>", "缺卡", hm2);
                            }
                            else if (hm2 == "00:00")
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>", hm, "缺卡");
                            }
                            else
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px'>{1}</div></td>", hm, hm2);
                            }
                        }
                        else
                        {
                            if (Convert.ToDateTime(entity.Years + "-" + entity.Months + "-" + i) > DateTime.Now)
                            {
                                persoListstr1.AppendFormat(
                                    "<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px'>{1}</div></td>",
                                    "", "");
                            }
                            else
                            {
                                persoListstr1.AppendFormat(
                                    "<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>",
                                    "缺卡", "缺卡");
                            }
                        }
                    }

                }
                persoListstr1.AppendFormat("</tr>");
            }
            persoListstr1.AppendFormat("</table>");
            if (operareType == 1) //导出操作
            {
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "utf-8";
                Response.ContentType = "application/vnd.ms-excel";
                Response.AppendHeader("content-disposition", "attachment;filename=dakajilu" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls");
                Response.ContentEncoding = Encoding.GetEncoding("utf-8");
                Response.Write(persoListstr1.ToString());
                Response.Flush();
                Response.End();
                return null;
            }
            else
            {
                return Content(persoListstr1.ToString());
            }

        }
        /// <summary>
        /// 员工打卡记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult DakaMonth1(DakaRecordEntity entity, int operareType = 0)
        {

            var mengdianyuangong = personBLL.GetAllUserByHospitalID(entity.HospitalID, 0);
            // var persoList = reserBLL.GetUserByHospitalIDListMonth(entity).GroupBy(c => c.UserID).Select(g => g.First()).ToList();
            StringBuilder persoListstr1 = new StringBuilder();
            persoListstr1.AppendFormat("<table border='0'>");
            persoListstr1.AppendFormat("<tr style='background-color:#ff8680;height:40px;color:white;'><th style='width:60px'>{0}月考勤</th>", entity.Months);
            //persoListstr1.AppendFormat("<tr style='background-color:#3bafda;height:40px;color:white;'><th style='width:60px'>{0}月考勤</th>", entity.Months);

            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            //获取当前月份的天数
            for (var i = 1; i <= days; i++)
            {
                persoListstr1.AppendFormat("<th style='width: 35px ;color: #fff;'> {0}日</th>", i);
            }
            persoListstr1.AppendFormat("</tr>");

            foreach (var info in mengdianyuangong)
            {
                //      str.AppendFormat(" <tr><td>{0}</td><td>{1}</td>", info.UserID, info.UserName);
                persoListstr1.AppendFormat("<tr><td style='text-align:center;border-right:1px solid #ccc;width:25px;border-bottom:1px dashed #ccc;height:45px;'>{0}</td>", info.UserName);

                for (var i = 1; i <= days; i++)
                {
                    //   var Dates = entity.Years + "-" + entity.Months + "-" + i;
                    var years = entity.Years;
                    var months = entity.Months;
                    var day = i;
                    var daka = reserBLL.SelectDaka(entity.HospitalID, years, months, day, info.UserID);
                    var qingjia = reserBLL.SelectQingjiaList(entity.HospitalID, years, months, day, info.UserID);
                    int count = 0;
                    if (qingjia.Count > 0)
                    {
                        foreach (var item in qingjia)
                        {
                            count += 1;
                            if (item.leixing == "请假")
                            {
                                if (qingjia.Count != 1)
                                {
                                    if (count == 1)
                                    {
                                        persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>", item.category, item.category);
                                    }
                                }
                                else
                                {
                                    if (item.AfternoonTime == "am")
                                    {
                                        DateTime dt = ConvertValueHelper.ConvertDateTimeValueNew(daka.EndTime);
                                        string hm2 = dt.ToString("HH:mm");
                                        persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px'>{1}</div></td>", item.category, hm2);
                                    }
                                    else if (item.AfternoonTime == "pm")
                                    {
                                        DateTime dd = ConvertValueHelper.ConvertDateTimeValueNew(daka.StartTime);
                                        string hm = dd.ToString("HH:mm");
                                        persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>", hm, item.category);
                                    }
                                }
                            }
                            else if (item.leixing == "休息")
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:green;'>{0}</div><div style='padding-top:5px;color:green;'>{1}</div></td>", "休息", "休息");
                            }
                        }
                    }
                    else
                    {
                        if (daka.Id > 0)
                        {
                            DateTime dd = ConvertValueHelper.ConvertDateTimeValueNew(daka.StartTime);
                            string hm = dd.ToString("HH:mm");
                            DateTime dt = ConvertValueHelper.ConvertDateTimeValueNew(daka.EndTime);
                            string hm2 = dt.ToString("HH:mm");
                            if (daka.EndTime == null)
                            {
                                hm2 = "00:00";
                            }
                            if (hm == "00:00")
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px'>{1}</div></td>", "缺卡", hm2);
                            }
                            else if (hm2 == "00:00")
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>", hm, "缺卡");
                            }
                            else
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px'>{1}</div></td>", hm, hm2);
                            }
                        }
                        else
                        {
                            if (Convert.ToDateTime(entity.Years + "-" + entity.Months + "-" + i) > DateTime.Now)
                            {
                                persoListstr1.AppendFormat(
                                    "<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px'>{1}</div></td>",
                                    "", "");
                            }
                            else
                            {
                                persoListstr1.AppendFormat(
                                    "<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>",
                                    "缺卡", "缺卡");
                            }
                        }
                    }

                }
                persoListstr1.AppendFormat("</tr>");
            }
            persoListstr1.AppendFormat("</table>");
            if (operareType == 1) //导出操作
            {
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "utf-8";
                Response.ContentType = "application/vnd.ms-excel";
                Response.AppendHeader("content-disposition", "attachment;filename=dakajilu" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls");
                Response.ContentEncoding = Encoding.GetEncoding("utf-8");
                Response.Write(persoListstr1.ToString());
                Response.Flush();
                Response.End();
                return null;
            }
            else
            {
                return Content(persoListstr1.ToString());
            }

        }
        #endregion

        #region ==请假记录
        public ActionResult Leavepage(LeaveRecordsEntity entity)
        {
            if (entity.HospitalID == 0) entity.HospitalID = LoginUserEntity.HospitalID;
            if (string.IsNullOrEmpty(entity.orderField))
            {
                entity.orderField = "ID";
                entity.orderDirection = "desc";
            }
            entity.numPerPage = 50; //每页显示条数
            if (string.IsNullOrEmpty(entity.StartTime))
            {
                entity.StartTime = DateTime.Now.ToString("yyyy-MM-dd 00:00:01");
                entity.EndTime = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
            }

            var list = reserBLL.GetLeaveRecordsList(entity);
            list = list.Where(t => t.category != "d91b4hykqlfm").ToList();
            var result = list.ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = list.Count;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_LeavePage", result);
            return View(result);
        }
        public ActionResult Leavepage1(LeaveRecordsEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;

            }

            if (entity.Years == 0)
            {
                entity.Years = DateTime.Now.Year;
            }

            if (entity.Months == 0)
            {
                entity.Months = DateTime.Now.Month;
            }
            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Years = entity.Years;
            ViewBag.Months = entity.Months;
            if (entity.HospitalID == 0) entity.HospitalID = LoginUserEntity.HospitalID;
            if (string.IsNullOrEmpty(entity.orderField))
            {
                entity.orderField = "ID";
                entity.orderDirection = "desc";
            }
            entity.numPerPage = 50; //每页显示条数
            //if (string.IsNullOrEmpty(entity.StartTime))
            //{
            //    entity.StartTime = DateTime.Now.ToString("yyyy-MM-dd 00:00:01");
            //    entity.EndTime = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
            //}

            days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            entity.StartTime = ("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString();
            entity.EndTime = ("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString();
            var list = reserBLL.GetLeaveRecordsList1(entity);
            list = list.Where(t => t.category == "d91b4hykqlfm").ToList();
            var result = list.ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = list.Count;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_LeavePage1", result);
            return View(result);
        }
        //编辑请假
        public ActionResult Leavepage2(LeaveRecordsEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;

            }

            if (entity.Years == 0)
            {
                entity.Years = DateTime.Now.Year;
            }

            if (entity.Months == 0)
            {
                entity.Months = DateTime.Now.Month;
            }
            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Years = entity.Years;
            ViewBag.Months = entity.Months;
            if (entity.HospitalID == 0) entity.HospitalID = LoginUserEntity.HospitalID;
            if (string.IsNullOrEmpty(entity.orderField))
            {
                entity.orderField = "ID";
                entity.orderDirection = "desc";
            }
            entity.numPerPage = 50; //每页显示条数
            //if (string.IsNullOrEmpty(entity.StartTime))
            //{
            //    entity.StartTime = DateTime.Now.ToString("yyyy-MM-dd 00:00:01");
            //    entity.EndTime = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
            //}

            days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            entity.StartTime = ("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString();
            entity.EndTime = ("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString();
            var list = reserBLL.GetLeaveRecordsList(entity);
            list = list.Where(t => t.category == "d91b4hykqlfm").ToList();
            var result = list.ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = list.Count;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_LeavePage1", result);
            return View(result);
        }
        public ActionResult AddLeavePage()
        {
            int ID = Convert.ToInt32(Request["ID"]);
            var info = reserBLL.GetLeaveRecords(ID);
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View(info);
        }
        //编辑请假
        public ActionResult AddLeavePage1(DakaRecordEntity dakaRecordEntity)
        {
            var entity = dakaRecordEntity;
            if (entity.HospitalID == 0)
                entity.HospitalID = LoginUserEntity.HospitalID;
            if (entity.Years == 0)
                entity.Years = DateTime.Now.Year;
            if (entity.Months == 0)
                entity.Months = DateTime.Now.Month;
            //考勤
            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Years = entity.Years;
            ViewBag.Months = entity.Months;
            int ID = Convert.ToInt32(Request["ID"]);
            var info = reserBLL.GetLeaveRecords(ID);
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View(info);
        }

        public ActionResult AddLeavePage2(DakaRecordEntity dakaRecordEntity)
        {
            var entity = dakaRecordEntity;
            if (entity.HospitalID == 0)
                entity.HospitalID = LoginUserEntity.HospitalID;
            if (entity.Years == 0)
                entity.Years = DateTime.Now.Year;
            if (entity.Months == 0)
                entity.Months = DateTime.Now.Month;
            //考勤
            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Years = entity.Years;
            ViewBag.Months = entity.Months;
            int ID = Convert.ToInt32(Request["ID"]);
            var info = reserBLL.GetLeaveRecords(ID);
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View(info);
        }
        //添加请假记录
        public ActionResult BianLeave(LeaveRecordsEntity entity)
        {
            int id = Convert.ToInt32(Request["ID"]);

            entity.Dateleave = DateTime.Now;
            DateTime statetime = Convert.ToDateTime(entity.StartTime);
            DateTime endtime = Convert.ToDateTime(entity.EndTime);
            if (statetime.Month != endtime.Month)
            {
                return Json(-99);
            }
            entity.StartYear = statetime.Year;
            entity.StartMoths = statetime.Month;
            entity.StartDays = statetime.Day;

            entity.EndYear = endtime.Year;
            entity.EndMonths = endtime.Month;
            entity.EndDays = endtime.Day;



            var shi = endtime.Hour;
            TimeSpan dayd = endtime - statetime;
            int day = Convert.ToInt32(dayd.TotalDays);
            int tian = 0;
            if (id > 0)
            {
                entity.ID = id;
                int pa = reserBLL.UpdateBonusEmployeed(entity);
                if (pa > 0)
                {
                    entity.LeaveId = id;
                    var ss = reserBLL.DeleteLeaveRecordMINGXIList(entity);
                    tian = id;
                }
                else
                {
                    tian = -99;
                }


            }
            else
            {
                tian = reserBLL.AddLeaveRecords(entity);
            }

            int jishu = 0;
            if (tian > 0)
            {
                if (day > 0)
                {
                    for (int i = 0; i < day + 1; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            entity.LeaveId = tian;
                            entity.LeaveTime = statetime.AddDays(i);
                            DateTime leavetime = Convert.ToDateTime(entity.LeaveTime);
                            entity.LeaveYear = leavetime.Year;
                            entity.LeaveMonths = leavetime.Month;
                            entity.LeaveDay = leavetime.Day;
                            jishu++;
                            if (jishu % 2 == 0)
                            {

                                entity.AfternoonTime = "pm";
                            }
                            else
                            {
                                entity.AfternoonTime = "am";
                            }
                            reserBLL.AddLeaveRecordsMINGXI(entity);
                        }

                    }
                }

                else
                {
                    entity.LeaveId = tian;
                    entity.LeaveTime = statetime;
                    DateTime leavetime = Convert.ToDateTime(entity.LeaveTime);
                    entity.LeaveYear = leavetime.Year;
                    entity.LeaveMonths = leavetime.Month;
                    entity.LeaveDay = leavetime.Day;
                    for (int k = 0; k < 2; k++)
                    {
                        jishu++;
                        if (jishu % 2 == 0)
                        {

                            entity.AfternoonTime = "pm";
                        }
                        else
                        {
                            entity.AfternoonTime = "am";
                        }
                        reserBLL.AddLeaveRecordsMINGXI(entity);
                    }
                    //if (shi > 12 || shi == 12)
                    //{
                    //    entity.AfternoonTime = "am";
                    //}
                    //else
                    //{
                    //    entity.AfternoonTime = "pm";
                    //}

                }
            }

            return Json(tian);
        }

        //添加请假记录
        public ActionResult BianLeave1(LeaveRecordsEntity entity)
        {
            int id = Convert.ToInt32(Request["ID"]);

            if (entity.HospitalID == 0)
                entity.HospitalID = LoginUserEntity.HospitalID;
            entity.Dateleave = DateTime.Now;
            entity.StartTime = DateTime.Now.ToString();
            entity.EndTime = DateTime.Now.ToString();
            //DateTime statetime = Convert.ToDateTime(entity.StartTime);
            //DateTime endtime = Convert.ToDateTime(entity.EndTime);
            //if (statetime.Month != endtime.Month)
            //{
            //    return Json(-99);
            //}
            //db.AddInParameter(comm, "StartTime", DbType.String, entity.StartTime); //开始时间
            ////db.AddInParameter(comm, "StartLeave", DbType.String, entity.StartLeave);
            //db.AddInParameter(comm, "EndTime", DbType.String, entity.EndTime); //结束时间
            entity.StartYear = entity.Year;
            entity.StartMoths = entity.Month;
            entity.StartDays = 1;

            // 店业绩特别奖励
            //entity.EndYear = 888;
            //沉睡客激活
            //entity.EndMonths = 999;

            entity.EndDays = 1;

            var entityl = new LeaveRecordsEntity();
            entityl.HospitalID = LoginUserEntity.HospitalID;
            entityl.numPerPage = 50; //每页显示条数

            //var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Year), Convert.ToInt32(entity.Month));
            //entityl.StartTime = ("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-01 00:00:01").ToString();
            //entityl.EndTime = ("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-" + days + " 23:59:59").ToString();
            string strsql = @" select top 1000  L.ID from [dbo].[EYB_tb_LeaveRecords] L
                              inner join EYB_tb_HospitalUser H
                              on L.UserId=h.UserID
                            where 1=1  and category = 'd91b4hykqlfm'
                            and HospitalID=" + LoginUserEntity.HospitalID + @"
                            and StartYear =" + entity.Year + @" AND StartMoths = "+ entity.Month + @"and L.Userid = " + entity.UserId  + @"";
            var str = GetIntegralByMonthLID(strsql);
            //var list1 = reserBLL.GetLeaveRecordsList(entityl);
            //list1 = list1.Where(t => t.category == "d91b4hykqlfm" && t.StartYear == entity.Year && t.StartMoths == entity.Month ).ToList();
            //if (list1.Count() > 0)
            //{
            //    return Json(-9999);
            //}
            entity.OtherStart = 1;
            //var shi = endtime.Hour;
            //TimeSpan dayd = endtime - statetime;
            //int day = Convert.ToInt32(dayd.TotalDays);
            entity.category = "d91b4hykqlfm"; 
            int tian = 0;
            if (str.ID > 0)
            {
                entity.ID = str.ID;
                tian = reserBLL.UpdateBonusEmployeed(entity);


            }
            else
            {
                tian = reserBLL.AddLeaveRecords(entity);
            }

            return Json(tian);
        }

        private LeaveRecordsEntity GetIntegralByMonthLID(string sqlstr, int filter = 0)
        {

            var LR = new LeaveRecordsEntity();
            try
            {
                string conStr = "server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft112";//连接字符串  
                SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                conText.Open();//打开数据库  
                string sqls = sqlstr;//创建统计语句  
                SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                SqlDataReader dr;//创建DataReader对象  
                dr = comText.ExecuteReader();//执行查询  
                while (dr.Read())//判断数据表中是否含有数据  
                {
                    var date = dr;
                    var aa = date["ID"].ToString() == ""?"0": date["ID"].ToString();
                    LR.ID = Convert.ToInt32(aa);


                }
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return LR;

        }
        public int changeSqlData(String sql)
        {  //执行
            try
            {
                using (SqlConnection con = new SqlConnection("server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft112"))
                {
                    con.Open();//操作数据库的工具SqlCommand
                    SqlCommand cmd = new SqlCommand(sql, con);//(操作语句和链接工具)
                    int i = cmd.ExecuteNonQuery();//执行操作返回影响行数（）
                    con.Close();
                    return i;
                }
            }
            catch { return 11; }

        }
        //添加请假记录
        public ActionResult BianLeave2(LeaveRecordsEntity entity)
        {
            int id = Convert.ToInt32(Request["ID"]);

            if (entity.HospitalID == 0)
                entity.HospitalID = LoginUserEntity.HospitalID;

            var YingXiangGongXiu = entity.NumDay;

            int tian = 0;
            if (!string.IsNullOrEmpty(YingXiangGongXiu)) 
            {
                string Inqdays =  YingXiangGongXiu;
                string strsql = "";
                strsql = @"UPDATE EYB_tb_Hospital SET YingXiangGongXiu = '" + Inqdays + "' from EYB_tb_Hospital WHERE ID = " + entity.HospitalID + "";

                changeSqlData(strsql);
                tian = 1;
            }
        

            return Json(tian);
        }

        public ActionResult _DeleteLeavePage(LeaveRecordsEntity entity)
        {
            entity.ID = Convert.ToInt32(Request["ID"]);
            int i = reserBLL.DeleteBonusEmployeed(entity);
            if (i > 0)
            {
                entity.LeaveId = entity.ID;
                reserBLL.DeleteLeaveRecordMINGXIList(entity);
            }
            return Json(i);
        }

        #endregion

        #region ==薪资模块==
        /// <summary>
        /// 全勤客流项目奖
        /// </summary>
        /// <param name="beautReward"></param>
        /// <param name="pingjukeliu"></param>
        /// <param name="xiangmu"></param>
        /// <returns></returns>
        private decimal Getqqklxmj(string beautReward, decimal pingjukeliu, decimal xiangmu)
        {
            decimal qqklxmj = 0;
            var MrsKX = beautReward.Replace("}", "").Split(',');
            var _pingjukeliu = Convert.ToDecimal(MrsKX[0].Split(':')[1] == "" ? "0" : MrsKX[0].Split(':')[1]);
            var _xiangmu = Convert.ToDecimal(MrsKX[1].Split(':')[1] == "" ? "0" : MrsKX[1].Split(':')[1]);
            var _qqklxmj = Convert.ToDecimal(MrsKX[2].Split(':')[1] == "" ? "0" : MrsKX[2].Split(':')[1]);
            if (pingjukeliu > _pingjukeliu && xiangmu > _xiangmu) qqklxmj = _qqklxmj;
            return qqklxmj;
        }
        /// <summary>
        /// 人头业绩
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="BenyueRentou"></param>
        /// <param name="ShangyueRentou"></param>
        /// <param name="mod"></param>
        /// <param name="_deductMoney"></param>
        private void GetBijiaoRentou(DakaRecordEntity entity, int BenyueRentou, int ShangyueRentou, DakaRecordEntity mod,
            decimal _deductMoney)
        {
            //decimal ShangyueRentou = 0;
            //那个月过年
            int year1 = entity.Years;
            DateTime myDT = new DateTime(year1, 1, 1, new System.Globalization.ChineseLunisolarCalendar());
            System.Globalization.Calendar myCal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            int month = myCal.GetMonth(myDT);
            if (entity.Months != month)
            {
                mod.BijiaoRentou = BenyueRentou - ShangyueRentou;
                mod.BijiaoRentouKoukuang = mod.BijiaoRentou * (decimal)_deductMoney;
            }
        }
        /// <summary>
        /// 获取薪资数据
        /// </summary>
        /// <param name="dakaRecordEntity"></param>
        private List<DakaRecordEntity> GetSalarySatisticsF(DakaRecordEntity dakaRecordEntity, int jobid = 0)
        {
            #region
            var entity = dakaRecordEntity;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.HospitalID == 0)
                entity.HospitalID = LoginUserEntity.HospitalID;
            if (entity.Years == 0)
                entity.Years = DateTime.Now.Year;
            if (entity.Months == 0)
                entity.Months = DateTime.Now.Month;
            //考勤
            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Years = entity.Years;
            ViewBag.Months = entity.Months;
            var list = personBLL.GetAllUser(new HospitalUserEntity { IsActive = 1, HospitalID = entity.HospitalID });
            list = list.Where(i => i.IsSys != 1 && i.IsAPP != 1).OrderBy(x => x.DutyName).ToList();
            List<DakaRecordEntity> newlist = new List<DakaRecordEntity>();
            //业绩
            int rows;
            int pagecount;
            var seachentity = new _SeachEntity();
            seachentity.s_HospitalID = entity.HospitalID;
            seachentity.s_StareTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
            seachentity.s_Endtime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());
            seachentity.numPerPage = 50; //每页显示条数
            if (seachentity.orderField + "" == "") { seachentity.orderField = "CreateTime"; }
            var result = cashBll.GetPerformanceSummary(seachentity, out rows, out pagecount);//默认非指定
            seachentity.IsZhiding = 1;//指定
            var resultzhiding = cashBll.GetPerformanceSummary(seachentity, out rows, out pagecount);//指定
            seachentity.IsZhiding = 1;//指定
            var sum_zhid = cashBll.GetSumPerformanceSummary(seachentity);//求汇总 指定
            seachentity.IsZhiding = -1;//非指定
            var sum_feizhid = cashBll.GetSumPerformanceSummary(seachentity);//求汇总 非指定
            var HospitalYeji = sum_feizhid.Yeji_Chuzhika + sum_feizhid.Yeji_Liaochengka + sum_feizhid.Yeji_Chanpin + sum_feizhid.Yeji_TeshuChanpin + sum_feizhid.Yeji_Xiangmu + sum_feizhid.Yeji_Teshuxiangmu;
            //var HospitalYeji = sum_zhid.Yeji_Chanpin + sum_feizhid.Yeji_Chanpin;//店业绩
            //返单
            var entityFangd = new _SeachEntity();
            if (entityFangd.s_HospitalID == 0)
            {
                entityFangd.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.HospitalID = entityFangd.s_HospitalID;
            DateTime dtFirst;
            DateTime dtLast;


            var dt = ConvertValueHelper.ConvertDateTimeValueNew(entity.Years + "-" + entity.Months + "-" + "01");

            //本月第一天时间
            dtFirst = ConvertValueHelper.ConvertDateTimeValueNew(dt.AddDays(1 - (dt.Day)).ToString("yyyy-MM-dd 00:00:00"));
            //获得某年某月的天数
            var dayCount = DateTime.DaysInMonth(dt.Year, dt.Month);
            //本月最后一天时间
            dtLast = ConvertValueHelper.ConvertDateTimeValueNew(dtFirst.AddDays(dayCount - 1).ToString("yyyy-MM-dd 23:59:59"));

            var listPers = personBLL.GetAllUser(new HospitalUserEntity { HospitalID = entityFangd.s_HospitalID, IsActive = 1 })
                .Where(c => c.IsSeparation == 0 && c.IsSys != 1).ToList();
            //客流
            var seachPatient = new PassengerTrafficEntity();
            seachPatient.HospitalID = entity.HospitalID;
            seachPatient.searchStartTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
            seachPatient.searchEndTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());
            if (string.IsNullOrWhiteSpace(entity.orderField)) seachPatient.orderField = "UserID";

            var listkekiuGuwenRentou = new List<PassengerTrafficEntity>();
            var listkekiuGuwenKwliu = new List<PassengerTrafficEntity>();
            var listkekiuGuwenPass = new List<PassengerTrafficEntity>();
            var listkekiuGuwenJiangqu = new List<PassengerTrafficEntity>();
            var listkekiuGuwenRentoushangyue = new List<PassengerTrafficEntity>();
            var listkekiuGuwenRentouCustomerService = new List<PassengerTrafficEntity>();
            if (jobid == 3 || jobid == 0)
            {
                listkekiuGuwenRentou = iCentBll.PageListPassengerTrafficGuWenRentou(seachPatient);
                listkekiuGuwenKwliu = iCentBll.PageListPassengerTrafficGuWenkeliu(seachPatient);
                listkekiuGuwenPass = iCentBll.PageListPassengerTrafficGuWenfuwuPassengerTraffic(seachPatient);
                listkekiuGuwenJiangqu = iCentBll.PageListPassengerTrafficGuWenJianquKeliu(seachPatient);
                listkekiuGuwenRentouCustomerService = iCentBll.PageListPassengerTrafficGuWenCustomerService(seachPatient);

                var seachPatientshangyue = new PassengerTrafficEntity();
                seachPatientshangyue.HospitalID = entity.HospitalID;
                seachPatientshangyue.searchStartTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
                seachPatientshangyue.searchEndTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());
                if (string.IsNullOrWhiteSpace(entity.orderField)) seachPatientshangyue.orderField = "UserID";
                seachPatientshangyue.searchStartTime = seachPatientshangyue.searchStartTime.AddMonths(-1);
                seachPatientshangyue.searchEndTime = seachPatientshangyue.searchEndTime.AddMonths(-1);
                listkekiuGuwenRentoushangyue = iCentBll.PageListPassengerTrafficGuWenRentou(seachPatientshangyue);

            }
            var listkekiu = iCentBll.PageListPassengerTrafficF(seachPatient);

            decimal Keliu = 0, fuwuPassengerTraffic = 0, kuadianKeliu = 0, kuadianKeliuC = 0;
            if (jobid == 2 || jobid == 4 || jobid == 0)
            {

                seachPatient.IsKuaDian = 1;
                var fuwuPassengerTrafficsum = 0;
                if (jobid != 0)
                    fuwuPassengerTrafficsum = iCentBll.PageGetfuwuPassengerTraffic(seachPatient).Where(x => x.DepartmentID == 1).FirstOrDefault().fuwuPassengerTraffic;//慢
                foreach (var en in listkekiu)
                {

                    //var ent = listkd.Where(p => p.UserID == en.UserID).FirstOrDefault();
                    kuadianKeliuC += en.fuwuPassengerTraffic;
                    Keliu += en.Keliu;
                    fuwuPassengerTraffic += (en.fuwuPassengerTraffic - en.JianquKeliu);
                }
                if (jobid != 0)
                    kuadianKeliu = fuwuPassengerTrafficsum - kuadianKeliuC;
            }


            var entityxm = new ItemDetailsConsumptionEntity();
            decimal huaci;
            decimal kakou;
            decimal shougong;
            decimal jiangli;
            decimal xianjin;
            decimal Quantity;
            int UserCount;
            entityxm.HospitalID = entityxm.HospitalID == 0 ? LoginUserEntity.HospitalID : entityxm.HospitalID;
            entityxm.numPerPage = 50; //每页显示条数
            entityxm.StartTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
            entityxm.EndTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());
            if (entityxm.orderField + "" == "") { entityxm.orderField = "CreateTime"; entityxm.orderDirection = "desc"; }
            var xiangmulist = cashBll.GetItemDetailsConsumptionList(entityxm, out huaci, out kakou, out shougong, out jiangli, out xianjin, out Quantity, out UserCount);
            #endregion
            //考勤参数
            var entitys = new SystemUserSettingsEntity();
            entitys.HospitalID = LoginUserEntity.HospitalID;
            var sysSalary = IsystemBLL.GetSystemUserSettingsModelByPay(entitys);
            //逻辑
            try
            {
                var syssalary = sysSalary.AttributeValue.Replace("|||", "★").Replace("\"", "").Replace("[", "").Replace("]", "");
                var arrsys = syssalary.Split('★');
                if (arrsys.Count() > 0)
                {
                    #region 参数赋值
                    Dictionary<string, string> arr = new Dictionary<string, string>();
                    for (int i = 0; i < arrsys.Length; i++)
                    {
                        arr.Add(arrsys[i], arrsys[i + 1]);
                        i++;
                    }
                    var BeauticianSal = arr["BeauticianSal"];
                    var msr_xktc = arr["newGuests"];
                    var mrs_scyj_arr = arr["storeRevenue"];
                    var mrs_djcxj_arr = arr["personalRoyalty"];
                    var shopownerSal = arr["shopownerSal"];
                    var shopownerAche = arr["shopownerAche"];
                    var adviserSal = arr["adviserSal"];
                    var asverBeauDuty = arr["asverBeauDuty"];
                    var receptionSal = arr["receptionSal"];
                    var receptionAche = arr["receptionAche"];
                    var operationPerformance = arr["operationPerformance"];
                    var isShopownerCheck = arr["isShopownerCheck"];
                    var deductMoney = arr["deductMoney"];
                    var beautReward = arr["beautReward"];
                    var beautReward1 = arr["beautReward1"];
                    var BeautYSJY = arr["BeautYSJY"];
                    var BeautDJCXJ = arr["BeautDJCXJ"];
                    var shopownerDJCXJ = arr["shopownerDJCXJ"];
                    var adviserDJCXJL = arr["adviserDJCXJL"];

                    var isAccum = arr["isAccum"];
                    var accum = arr["accum"];
                    var isAchieMRS = arr["isAchieMRS"];
                    var isAchieQT = arr["isAchieQT"];
                    var isAchieDZ = arr["isAchieDZ"];
                    var isAchieGW = arr["isAchieGW"];
                    var cardBuckle = arr["cardBuckle"];
                    var _isAchieMRS = Convert.ToInt32(string.IsNullOrEmpty(isAchieMRS) ? "0" : isAchieMRS);
                    var _isAchieQT = Convert.ToInt32(string.IsNullOrEmpty(isAchieQT) ? "0" : isAchieQT);
                    var _isAchieDZ = Convert.ToInt32(string.IsNullOrEmpty(isAchieDZ) ? "0" : isAchieDZ);
                    var _isAchieGW = Convert.ToInt32(string.IsNullOrEmpty(isAchieGW) ? "0" : isAchieGW);
                    var HomeProTiChen = arr["HomeProTiChen"];
                    var isSpecial = arr["isSpecial"];
                    var isHomePro = arr["isHomePro"];
                    var isFullAttendMRS = arr["isFullAttendMRS"];
                    var isFullAttendGW = arr["isFullAttendGW"];
                    var isFullAttendDZ = arr["isFullAttendDZ"];
                    var isFullAttendQT = arr["isFullAttendQT"];
                    var deductMoney1 = arr["deductMoney1"];
                    var receptionDJCXJ = arr["receptionDJCXJ"];
                    var _isFullAttendMRS = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendMRS) ? "0" : isFullAttendMRS);
                    var _isFullAttendGW = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendGW) ? "0" : isFullAttendGW);
                    var _isFullAttendDZ = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendDZ) ? "0" : isFullAttendDZ);
                    var _isFullAttendQT = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendQT) ? "0" : isFullAttendQT);
                    var _deductMoney1 = Convert.ToDecimal(string.IsNullOrEmpty(deductMoney1) ? "0" : deductMoney1);
                    var _deductMoney = Convert.ToDecimal(string.IsNullOrEmpty(deductMoney) ? "0" : deductMoney);
                    var dt1 = ConvertValueHelper.ConvertDateTimeValueNew(entity.Years + "-" + entity.Months + "-" + "01");
                    var dayCount1 = DateTime.DaysInMonth(dt1.Year, dt1.Month);
                    //var isAccum = arr["isAccum"];
                    #endregion
                    #region

                    int DePartMentID = -1;
                    foreach (var info in list)
                    {

                        var mod = reserBLL.SelectKaoqingtongji(new DakaRecordEntity()
                        { UserID = info.UserID, Years = entity.Years, Months = entity.Months });
                        if (jobid == 1 || jobid == 0)
                        {
                            #region 获取单条统计
                            //考勤统计
                            //var mod = reserBLL.SelectKaoqingtongji(new DakaRecordEntity()
                            //{ UserID = info.UserID, Years = entity.Years, Months = entity.Months });
                            mod.UserName = info.UserName;
                            mod.UserPost = info.DutyName;

                            #endregion
                            if (mod.UserPost == "美容师")
                            {
                                var ShouGongFeiModel = result.Where(t => t.DutyName == mod.UserPost && t.UserName == mod.UserName).ToList();
                                if (ShouGongFeiModel.Count() > 0) 
                                mod.ShouGongFei = ShouGongFeiModel[0].Ticheng;
                                //考勤全勤客流项目奖
                                decimal qqklxmj = 0;
                                decimal quangqingjiang = 0;
                                //全勤
                                var keliu = listkekiu.Where(x => x.UserID == info.UserID).ToList();
                                var getxiangmu = xiangmulist.Where(x => x.UserID == info.UserID).ToList();
                                decimal pingjukeliu = 0;
                                decimal xiangmu = 0;

                                var res = result.Where(x => x.UserID == info.UserID).ToList();
                                var regzhiding = resultzhiding.Where(x => x.UserID == info.UserID).ToList();

                                //店综合业绩
                                var sumzh = sum_zhid.Yeji_Chuzhika + sum_zhid.Yeji_Liaochengka + sum_zhid.Yeji_Xiangmu +
                                        sum_zhid.Yeji_Chanpin + sum_zhid.Yeji_TeshuChanpin + sum_zhid.Yeji_Daxiangmu +
                                        sum_zhid.KakouLiaoCheng + sum_zhid.Yeji_Xiangmu + sum_zhid.KakouYeji_Chanpin +
                                        sum_zhid.KakouYeji_TeshuChanpin + sum_zhid.KakouYeji_Teshuxiangmu + sum_zhid.KakouYeji_Daxiangmu +
                                        sum_zhid.ShiCao;
                                mod.HospitalRewardALL = sumzh / 2;
                                var BeauticianSalarr = ("," + BeauticianSal).Split('}');
                                var item = BeauticianSalarr;
                                #region 底薪1
                                var Dixin1 = 0;

                                if (res.Count() > 0)
                                {
                                    var infoo = res[0];
                                    var sumshichao = infoo.ShiCao_zhiding ;
                                    DixinOne(res[0], mod, item, sumshichao);//底薪1
                                }
                                mod.Mrsdanxiangmu = mod.Mrsdanxiangmu > 0 ? mod.Mrsdanxiangmu : 0;
                                if (mod.MrsDixin1 < 500) mod.MrsDixin1 = 500;
                                #endregion
                                #region 底薪2

                                var Dixin2 = 0;
                                var kel = listkekiu.Where(x => x.UserID == info.UserID).ToList();
                                if (kel.Count() > 0)
                                {
                                    var infoo = kel[0];
                                    seachPatient.IsKuaDian = 1;
                                    seachPatient.UserID = info.UserID;
                                    var kuandiankeliu = iCentBll.PageGetfuwuPassengerTrafficBy(seachPatient).Where(x => x.UserID == info.UserID).FirstOrDefault().fuwuPassengerTraffic - infoo.fuwuPassengerTraffic;//慢
                                    //var sumkeliu = infoo.fuwuPassengerTraffic - infoo.JianquKeliu + infoo.Keliu + kuandiankeliu;
                                    //var sumkeliu = infoo.fuwuPassengerTraffic - infoo.JianquKeliu + infoo.Keliu + kuandiankeliu;
                                    var sumkeliu = infoo.fuwuPassengerTraffic - infoo.JianquKeliu + infoo.Keliu;
                                    DixinTwo(mod, item, sumkeliu);//底薪2
                                    mod.MrsDixin2 = mod.MrsDixin2 < 500 ? 500 : mod.MrsDixin2;
                                    mod.MrsDixin2keliu = (decimal)sumkeliu;
                                    /* 新客*/
                                    var Customerarr = msr_xktc.Split(',');
                                    if (infoo.CustomerService == 0) { mod.NewCusReward = Convert.ToInt32(Customerarr[0].Split(':')[1]); }
                                    else if (infoo.CustomerService == 1) { mod.NewCusReward = Convert.ToInt32(Customerarr[1].Split(':')[1]); }
                                    else if (infoo.CustomerService == 2) { mod.NewCusReward = Convert.ToInt32(Customerarr[2].Split(':')[1]); }
                                    else if (infoo.CustomerService == 3) { mod.NewCusReward = Convert.ToInt32(Customerarr[3].Split(':')[1]); }
                                    else if (infoo.CustomerService == 4) { mod.NewCusReward = Convert.ToInt32(Customerarr[4].Split(':')[1]); }
                                    else { mod.NewCusReward = Convert.ToInt32(Customerarr[5].Split(':')[1].Replace("}", "")); }
                                    mod.CustomerService = infoo.CustomerService;
                                }
                                //mod.DixinSalary = (Dixin1 + Dixin2) < 1000 ? 1000 : (Dixin1 + Dixin2);//总底薪
                                //mod.DixinSalary = (Dixin1 + Dixin2) < 1000 ? 1000 : (Dixin1 + Dixin2);//总底薪
                                mod.DixinSalary = Convert.ToInt32(mod.MrsDixin1) + Convert.ToInt32(mod.MrsDixin2) < 1000 ? 1000 : Convert.ToInt32(mod.MrsDixin1) + Convert.ToInt32(mod.MrsDixin2);//总底薪
                                if (_isAchieMRS == 1)
                                    mod.DixinSalary = mod.DixinSalary * mod.ChuqingDay / dayCount;
                                mod.JixiaoReward = Convert.ToInt32(quangqingjiang + qqklxmj + mod.NewCusReward);//绩效奖金
                                mod.QuanqingReward = (int)quangqingjiang;
                                #endregion
                                #region 业绩提成
                                var _operationPerformance = ("," + operationPerformance).Split('}');
                                var _BeautYSJY = BeautYSJY.Replace("}", "").Split(',');
                                var ag = 0; var tichengzhidingke = 0;//指定客提成
                                var fg = 0; var feitichengzhidingke = 0;//非指定客提
                                if (res.Count() > 0)
                                {

                                    var info1 = res[0];
                                    var _MouthShiCaoTicheng_zhiding = info1.ShiCao_zhiding + info1.kuadianShiCao_zhiding;
                                    var _MouthShiCaoTicheng_feizhiding = info1.ShiCao_feizhiding + info1.kuadianShiCao_feizhiding;
                                    var _MouthShiCao = _MouthShiCaoTicheng_zhiding + _MouthShiCaoTicheng_feizhiding;
                                    mod.MrsMouthShicao = _MouthShiCao;
                                    if (_MouthShiCao > Convert.ToDecimal(string.IsNullOrEmpty(_BeautYSJY[4].Split(':')[1]) ? "0" : _BeautYSJY[4].Split(':')[1]))
                                    {
                                        ag = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[0].Split(',')[4].Split(':')[1]) * 100);//指定客提成 * 100
                                        fg = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[1].Split(',')[4].Split(':')[1]) * 100);//非指定客提成 * 100

                                    }
                                    else if (_MouthShiCao > Convert.ToDecimal(string.IsNullOrEmpty(_BeautYSJY[2].Split(':')[1]) ? "0" : _BeautYSJY[2].Split(':')[1]))
                                    {
                                        ag = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[0].Split(',')[3].Split(':')[1]) * 100);//指定客提成 * 100
                                        fg = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[1].Split(',')[3].Split(':')[1]) * 100);//非指定客提成 * 100
                                    }
                                    else
                                    {
                                        ag = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[0].Split(',')[2].Split(':')[1]) * 100);//指定客提成 * 100
                                        fg = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[1].Split(',')[2].Split(':')[1]) * 100);//非指定客提成 * 100

                                    }
                                    mod.Mrsag = ag/100;
                                    mod.Mrsfg = fg / 100;
                                    tichengzhidingke = Convert.ToInt32((decimal)_MouthShiCaoTicheng_zhiding * (decimal)ag / (decimal)10000);
                                    feitichengzhidingke = Convert.ToInt32((decimal)_MouthShiCaoTicheng_feizhiding * (decimal)fg / (decimal)10000);
                                    mod.Mrstichengzhidingke = tichengzhidingke + feitichengzhidingke;
                                    //mod.Mrsfeitichengzhidingke = feitichengzhidingke;
                                }
                                mod.Mrstichengzhidingke = mod.Mrstichengzhidingke > 0 ? mod.Mrstichengzhidingke : 0;
                                var _mrs_scyj_arr = ("," + mrs_scyj_arr).Split('}');//店基础现金总收入  计算参数

                                var _mrs_djcxj_arr = ("," + mrs_djcxj_arr).Split('}');//个人特殊  计算参数

                                decimal yejitc = 0;
                                JoinUserEntity infoo1;
                                decimal infoo1KakouYeji_Chanpin = 0; decimal infoo1KakouYeji_Liaocheng = 0;
                                decimal infoo1Yeji_Chanpin = 0; decimal infoo1Yeji_Liaochengka = 0; decimal infoo1Yeji_TeshuChanpin = 0;
                                if (regzhiding.Count() > 0)
                                {
                                    infoo1 = regzhiding[0];
                                    infoo1KakouYeji_Chanpin = infoo1.KakouYeji_Chanpin;
                                    infoo1KakouYeji_Liaocheng = infoo1.KakouYeji_Liaocheng;
                                    infoo1Yeji_Chanpin = infoo1.Yeji_Chanpin;
                                    infoo1Yeji_Liaochengka = infoo1.Yeji_Liaochengka;
                                    infoo1Yeji_TeshuChanpin = infoo1.Yeji_TeshuChanpin;
                                }
                                if (res.Count() > 0)
                                {
                                    Dictionary<string, string> shopownerDJCXJ_item = JsonConvert.DeserializeObject<Dictionary<string, string>>(shopownerDJCXJ);
                                    var infoo = res[0];
                                    var keys = shopownerDJCXJ_item.Keys;
                                    #region  店基础现金总收入
                                    #endregion
                                    #region
                                    var iii = 7;
                                    var _isSpecial = Convert.ToInt32(string.IsNullOrEmpty(isSpecial) ? "0" : isSpecial);
                                    var _isHomePro = Convert.ToInt32(string.IsNullOrEmpty(isHomePro) ? "0" : isHomePro);
                                    var _cardBuckle = Convert.ToDecimal(string.IsNullOrEmpty(cardBuckle) ? "0" : cardBuckle);
                                    var _HomeProTiChen = Convert.ToDecimal(string.IsNullOrEmpty(HomeProTiChen) ? "0" : HomeProTiChen);
                                    if (_isHomePro == 1)
                                    {
                                        var _listPers = listPers.Where(x => x.UserID == info.UserID).ToList();
                                        if (_listPers.Count() > 0)
                                        {
                                            var Benyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                                            {
                                                s_HospitalID = entityFangd.s_HospitalID,
                                                s_UserID = _listPers[0].UserID,
                                                s_StareTime = dtFirst,
                                                s_Endtime = dtLast,
                                                ProductType = entityFangd.ProductType
                                            });
                                            if (Benyue > (decimal)0)
                                            {
                                                var Shangeyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                                                {
                                                    s_HospitalID = entityFangd.s_HospitalID,
                                                    s_UserID = _listPers[0].UserID,
                                                    s_StareTime = dtFirst.AddMonths(-1),
                                                    s_Endtime = dtLast.AddMonths(-1),
                                                    ProductType = entityFangd.ProductType
                                                });
                                                var Qianyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                                                {
                                                    s_HospitalID = entityFangd.s_HospitalID,
                                                    s_UserID = _listPers[0].UserID,
                                                    s_StareTime = dtFirst.AddMonths(-2),
                                                    s_Endtime = dtLast.AddMonths(-2),
                                                    ProductType = entityFangd.ProductType
                                                });
                                                if (Shangeyue > (decimal)0 || Qianyue > (decimal)0)
                                                {
                                                    mod.MrsFangdang = Convert.ToInt32(Benyue * (decimal)_HomeProTiChen / 100);
                                                    mod.MrsFangdangBenyue = Benyue;
                                                }
                                            }
                                        }
                                    }
                                    for (int i = shopownerDJCXJ_item.Count - 1; i >= 0; i--)
                                    {
                                        var yejize = Convert.ToDecimal(shopownerDJCXJ_item["scope" + i]);
                                        i--;
                                        iii--;
                                        if (HospitalYeji > yejize || HospitalYeji == (decimal)yejize)
                                        {
                                            mod.MrsHospitalYeji = HospitalYeji;
                                            mod.Mrssdyeji = yejize;
                                            yejitc = infoo.KakouYeji_Chuzhika * Convert.ToDecimal(_mrs_scyj_arr[0].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人会员卡指定客提成
                                            yejitc += (infoo.Yeji_Liaochengka) * Convert.ToDecimal(_mrs_scyj_arr[2].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居指定客提成
                                            yejitc += (infoo.Yeji_Chanpin) * Convert.ToDecimal(_mrs_scyj_arr[4].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居指定客提成
                                            yejitc += (infoo.Yeji_TeshuChanpin) * Convert.ToDecimal(_mrs_djcxj_arr[0].Split(',')[1].Split(':')[1]) / 100;//实操提成_个人特殊现金业绩个人提成指定客提成
                                            yejitc += (infoo.KakouYeji_Chanpin + infoo.KakouYeji_Liaocheng + infoo1KakouYeji_Chanpin + infoo1KakouYeji_Chanpin) * _cardBuckle / 100;//实操提成_卡扣家居卡扣疗程  一律1%提成

                                            yejitc += infoo1KakouYeji_Chanpin * Convert.ToDecimal(_mrs_scyj_arr[1].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人会员卡非指定客提成
                                            yejitc += (infoo1Yeji_Liaochengka) * Convert.ToDecimal(_mrs_scyj_arr[3].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居非指定客提成
                                            yejitc += (infoo1Yeji_Chanpin) * Convert.ToDecimal(_mrs_scyj_arr[5].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居非指定客提成
                                            if (_isSpecial == 1)//是特殊提成
                                            {
                                                yejitc += (infoo1Yeji_TeshuChanpin) * Convert.ToDecimal(_mrs_djcxj_arr[0].Split(',')[2].Split(':')[1]) / 100;//实操提成_个人特殊现金业绩个人提成非指定客
                                            }
                                            #region
                                            //mod.MrsshicaoStr = sum_zhid.KakouYeji_Chuzhika + "*" + Convert.ToDecimal(_mrs_scyj_arr[0].Split(',')[iii].Split(':')[1]) + "%  +"
                                            //                  + sum_feizhid.KakouYeji_Chuzhika + "*" + Convert.ToDecimal(_mrs_scyj_arr[1].Split(',')[iii].Split(':')[1]) + "%  +"
                                            //                  + (sum_zhid.Yeji_Liaochengka + sum_zhid.Yeji_Chanpin) + "*" + Convert.ToDecimal(_mrs_scyj_arr[2].Split(',')[iii].Split(':')[1]) + "%  +"
                                            //                  + (sum_feizhid.Yeji_Liaochengka + sum_feizhid.Yeji_Chanpin) + "*" + Convert.ToDecimal(_mrs_scyj_arr[3].Split(',')[iii].Split(':')[1]) + "%  +"
                                            //                  + (sum_zhid.Yeji_TeshuChanpin) + "*" + Convert.ToDecimal(_mrs_djcxj_arr[0].Split(',')[1].Split(':')[1]) + "%  +"
                                            //                  + (sum_feizhid.Yeji_TeshuChanpin) + "*" + Convert.ToDecimal(_mrs_djcxj_arr[0].Split(',')[2].Split(':')[1]) + "% ";
                                            #endregion
                                            break;
                                        }

                                    }
                                    #endregion
                                    mod.MrsshicaoStr = yejitc.ToString();
                                    #endregion

                                }

                                mod.MrsFangdang = mod.MrsFangdang > 0 ? mod.MrsFangdang : 0;
                                mod.MrsFangdangBenyue = mod.MrsFangdangBenyue > 0 ? mod.MrsFangdangBenyue : 0;
                                mod.MrsHospitalYeji = HospitalYeji;
                                mod.MrsshicaoStr = string.IsNullOrEmpty(mod.MrsshicaoStr) ? "0" : yejitc.ToString();
                                var jiaoxi1 = Convert.ToInt32(mod.Mrsdanxiangmu + mod.Mrstichengzhidingke + mod.MrsFangdang + Convert.ToInt32(Convert.ToDecimal(mod.MrsshicaoStr)));
                                mod.JixiaoTicheng = jiaoxi1;
                                #endregion
                                #region 绩效奖金

                                if (res.Count() > 0)
                                {
                                    var infoo = res[0];
                                    //单项目奖励
                                    var _isAccum = Convert.ToInt32(string.IsNullOrEmpty(isAccum) ? "0" : isAccum);
                                    var _accum = Convert.ToInt32(string.IsNullOrEmpty(accum) ? "0" : accum);
                                    if (_isAccum == 1)
                                        mod.Mrsdanxiangmu = Convert.ToInt32(infoo.Yeji_Daxiangmu) > _accum ? _accum : Convert.ToInt32(infoo.Yeji_Daxiangmu);
                                    else
                                        mod.Mrsdanxiangmu = Convert.ToInt32(infoo.Yeji_Daxiangmu);
                                }
                                if (mod.ChuqingDay + mod.XiuxiDay == dayCount1)
                                {
                                    if (keliu.Count() > 0 && mod.ShijiChuqingDay > 0)
                                    {
                                        pingjukeliu = Convert.ToDecimal((keliu[0].fuwuliuliang + keliu[0].fuwuPassengerTraffic + keliu[0].Keliu) / mod.ShijiChuqingDay); //平均客流

                                    }
                                    if (getxiangmu.Count() > 0)
                                    {
                                        xiangmu = Convert.ToDecimal(getxiangmu[0].ProjectNumber);//项目数
                                    }
                                    if (Getqqklxmj(beautReward1, pingjukeliu, xiangmu) == (decimal)0)
                                    {
                                        qqklxmj = Getqqklxmj(beautReward, pingjukeliu, xiangmu);
                                    }
                                    else
                                    {
                                        qqklxmj = Getqqklxmj(beautReward1, pingjukeliu, xiangmu);
                                    }
                                    mod.Mrspingjukeliu = pingjukeliu;
                                    mod.Mrsxiangmu = xiangmu;
                                    mod.Mrsqqklxmj = qqklxmj;
                                    if (_isFullAttendMRS == 1)
                                    {
                                        quangqingjiang = (decimal)50;
                                        mod.Mrsquangqingjiang = quangqingjiang;
                                    }
                                }

                                mod.Mrsquangqingjiang = mod.Mrsquangqingjiang > 0 ? mod.Mrsquangqingjiang : 0;
                                mod.Mrspingjukeliu = mod.Mrspingjukeliu > 0 ? mod.Mrspingjukeliu : 0;
                                mod.Mrsxiangmu = mod.Mrsxiangmu > 0 ? mod.Mrsxiangmu : 0;
                                mod.Mrsqqklxmj = mod.Mrsqqklxmj > 0 ? mod.Mrsqqklxmj : 0;
                                if (jobid == 1)
                                {
                                    #endregion


                                    newlist.Add(mod);
                                }

                            }
                        }
                        if (jobid == 2 || jobid == 0)
                        {
                            mod.UserPost = info.DutyName;
                            if (mod.UserPost == "店长")
                            {
                                #region 获取单条统计
                                //考勤统计
                                //var mod = reserBLL.SelectKaoqingtongji(new DakaRecordEntity()
                                //{ UserID = info.UserID, Years = entity.Years, Months = entity.Months });
                                //考勤全勤客流项目奖

                                mod.UserName = info.UserName;
                                decimal qqklxmj = 0;
                                decimal quangqingjiang = 0;
                                //全勤
                                mod.ShouGongFei = result.Sum(t=>t.Ticheng);
                                var keliu = listkekiu.Where(x => x.UserID == info.UserID).ToList();
                                var getxiangmu = xiangmulist.Where(x => x.UserID == info.UserID).ToList();
                                decimal pingjukeliu = 0;
                                decimal xiangmu = 0;

                                var res = result.Where(x => x.UserID == info.UserID).ToList();
                                var regzhiding = resultzhiding.Where(x => x.UserID == info.UserID).ToList();

                                //店综合业绩
                                var sumzh = sum_feizhid.Yeji_Chuzhika + sum_feizhid.Yeji_Liaochengka + sum_feizhid.Yeji_Xiangmu +
                                        sum_feizhid.Yeji_Chanpin + sum_feizhid.Yeji_TeshuChanpin +
                                        sum_feizhid.KakouLiaoCheng + sum_feizhid.Yeji_Xiangmu + sum_feizhid.KakouYeji_Chanpin +
                                        sum_feizhid.KakouYeji_TeshuChanpin + sum_feizhid.KakouYeji_Teshuxiangmu +
                                        sum_zhid.ShiCao;
                                mod.HospitalRewardALL = sumzh / 2;
                                mod.XiangmuYeji = sum_feizhid.Yeji_Daxiangmu;
                                #endregion

                                var _shopownerSal = ("," + shopownerSal).Split('}');
                                var item = _shopownerSal;
                                #region 底薪1
                                var Dixin1 = 0;
                                if (res.Count() > 0)
                                {
                                    var infoo = res[0];
                                    seachPatient.IsKuaDian = -1;
                                    seachPatient.searchStartTime = seachPatient.searchStartTime.AddMonths(-1);
                                    seachPatient.searchEndTime = seachPatient.searchEndTime.AddMonths(-1);
                                    var listkekiushang = iCentBll.PageListPassengerTraffic(seachPatient);
                                    DixinOne(res[0], mod, item, (decimal)HospitalYeji);//底薪1
                                    var BenyueRentou = listkekiu.Sum(x => x.BuyCount);
                                    var ShangyueRentou = listkekiushang.Sum(x => x.BuyCount);
                                    GetBijiaoRentou(entity, BenyueRentou, ShangyueRentou, mod, _deductMoney);
                                }

                                mod.BijiaoRentou = mod.BijiaoRentou != 0 ? mod.BijiaoRentou : 0;
                                mod.BijiaoRentouKoukuang = mod.BijiaoRentouKoukuang != 0 ? mod.BijiaoRentouKoukuang : 0;
                                #endregion
                                #region 底薪2
                                var Dixin2 = 0;
                                var sumkeliu = Convert.ToInt32(Keliu + fuwuPassengerTraffic + kuadianKeliu);// listkekiu.Sum(x => x.PassengerTraffic) + listkekiu.Sum(x => x.fuwuPassengerTraffic); //总客流
                                DixinTwo(mod, item, sumkeliu);//底薪2

                                mod.MrsDixin2keliu = (decimal)sumkeliu;
                                var newkeliu = listkekiu.Sum(x => x.CustomerService); //新客流
                                mod.MrsDixin1 = mod.MrsDixin1 < 1200 ? 1200 : mod.MrsDixin1;
                                mod.MrsDixin2 = mod.MrsDixin2 < 1200 ? 1200 : mod.MrsDixin2;//总底薪
                                mod.DixinSalary = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2)) < 2400 ? 2400 : (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪

                                if (_isAchieDZ == 1)
                                    mod.DixinSalary = mod.DixinSalary * mod.ChuqingDay / dayCount;
                                if (_isFullAttendDZ == 1)
                                {
                                    quangqingjiang = (decimal)50;
                                    mod.Mrsquangqingjiang = quangqingjiang;
                                }
                                mod.Mrsquangqingjiang = mod.Mrsquangqingjiang > 0 ? mod.Mrsquangqingjiang : 0;
                                #region 业绩提成
                                var yejitc = GetYejiTi(mod, HospitalYeji, mod.HospitalRewardALL, shopownerAche, shopownerDJCXJ);
                                var yejitcdaxiangmu = GetYejiTiDaxianmu(mod, HospitalYeji, mod.XiangmuYeji, shopownerAche, shopownerDJCXJ);
                                mod.MrsshicaoStr = (yejitc + yejitcdaxiangmu).ToString();
                                mod.JixiaoTicheng = yejitc + yejitcdaxiangmu;
                                #endregion
                                #region 店长顾问责任指标
                                int GZZZ = GetGZZhize(HospitalYeji, sumkeliu, newkeliu, asverBeauDuty);
                                mod.sumkeliu = sumkeliu;
                                mod.newkeliu = newkeliu;
                                mod.JiangliDuty = GZZZ;
                                //mod.JiangliDuty = Convert.ToInt32(GZZZ.Split(',')[0] == "" ? "" : GZZZ.Split(',')[0]);//业绩奖金; 
                                //mod.KoukuanDuty = Convert.ToInt32(GZZZ.Split(',')[1] == "" ? "" : GZZZ.Split(',')[1]);//职责罚扣; 
                                mod.JixiaoReward = Convert.ToInt32(mod.BijiaoRentouKoukuang) + mod.JiangliDuty;//业绩资金; 
                                #endregion
                                if (jobid == 2)
                                {
                                    newlist.Add(mod);
                                }
                            }
                        }
                        if (jobid == 3 || jobid == 0)
                        {
                            #region 获取单条统计
                            //考勤统计
                            //var mod = reserBLL.SelectKaoqingtongji(new DakaRecordEntity()
                            //{ UserID = info.UserID, Years = entity.Years, Months = entity.Months });
                            mod.UserName = info.UserName;
                            mod.UserPost = info.DutyName;

                            #endregion
                            if (mod.UserPost == "顾问")
                            {

                                //考勤全勤客流项目奖
                                //decimal qqklxmj = 0;
                                decimal quangqingjiang = 0;
                                //全勤
                                var keliu = listkekiu.Where(x => x.UserID == info.UserID).ToList();
                                var getxiangmu = xiangmulist.Where(x => x.UserID == info.UserID).ToList();
                                //decimal pingjukeliu = 0;
                                //decimal xiangmu = 0;
                             
                                var res = result.Where(x => x.UserID == info.UserID).ToList();
                                //var regzhiding = resultzhiding.Where(x => x.UserID == info.UserID).ToList();
                                //店综合业绩
                                //var sumzh = info..Yeji_Chuzhika + info.Yeji_Liaochengka + info.Yeji_Xiangmu +
                                //            info.Yeji_Chanpin + info.Yeji_TeshuChanpin + info.Yeji_Daxiangmu +
                                //            info.KakouLiaoCheng + info.Yeji_Xiangmu + info.KakouYeji_Chanpin +
                                //            info.KakouYeji_TeshuChanpin + info.KakouYeji_Teshuxiangmu + info.KakouYeji_Daxiangmu +
                                //            info.ShiCao;
                                mod.HospitalRewardALL = 0;// = sumzh / 2;
                                decimal sumzhs = 0;
                                decimal XiangmuYeji = 0;


                                var _adviserSal = ("," + adviserSal).Split('}');
                                var item = _adviserSal;
                                #region 底薪1
                                var Dixin1 = 0;
                                decimal HospitalYejiDEe = 0;//
                                if (res.Count() > 0)
                                {
                                    var infoo = res[0];
                                    var listDepartMent = result.Where(x => x.DepartmentID == DePartMentID);
                                    //if (DePartMentID != infoo.DepartmentID)
                                    //{
                                    DePartMentID = infoo.DepartmentID;
                                   var reslist = result.Where(t=>t.DepartmentID==DePartMentID).ToList();
                                    mod.ShouGongFei = reslist.Sum(t=>t.Ticheng);
                                    foreach (var infooo in listDepartMent)
                                    {
                                        sumzhs += infooo.Yeji_Chuzhika + infooo.Yeji_Liaochengka + infooo.Yeji_Xiangmu +
                                                infooo.Yeji_Chanpin + infooo.Yeji_TeshuChanpin +
                                                infooo.KakouLiaoCheng + infooo.Yeji_Xiangmu + infooo.KakouYeji_Chanpin +
                                                infooo.KakouYeji_TeshuChanpin + infooo.KakouYeji_Teshuxiangmu +
                                                infooo.ShiCao;
                                        XiangmuYeji += infooo.Yeji_Daxiangmu;
                                    }
                                    //}
                                    mod.HospitalRewardALL = sumzhs / 2;
                                    //seachentity.IsZhiding = 1;//指定
                                    seachentity.DepartmentID = DePartMentID;
                                    //var sum_zhids = cashBll.GetSumPerformanceSummary(seachentity);//求汇总 指定
                                    seachentity.IsZhiding = -1;//非指定
                                    var sum_feizhids = cashBll.GetSumPerformanceSummary(seachentity);//求汇总 非指定
                                    var sumzhDe = sum_feizhids.Yeji_Chuzhika + sum_feizhids.Yeji_Liaochengka + sum_feizhids.Yeji_Chanpin + sum_feizhids.Yeji_TeshuChanpin + sum_feizhids.Yeji_Xiangmu + sum_feizhids.Yeji_Teshuxiangmu;
                                    HospitalYejiDEe = sumzhDe;// sum_zhids.Yeji_Chanpin + sum_feizhids.Yeji_Chanpin;//店业绩
                                    DixinOne(res[0], mod, item, (decimal)HospitalYejiDEe);//底薪1
                                }
                                #endregion
                                #region 底薪2
                                var Dixin2 = 0;
                                var kel = keliu;
                                decimal sumkeliu = 0;
                                decimal ShangyueRentou = 0;
                                decimal istkekiuCustomerService = 0;


                                if (kel.Count() > 0)
                                {
                                    var infoo = kel[0];
                                    DePartMentID = infoo.DepartmentID;
                                    seachPatient.DepartmentID = DePartMentID;
                                    decimal Keliu1 = 0, fuwuPassengerTraffic1 = 0, kuadianKeliu1 = 0;
                                    var keliug = listkekiuGuwenKwliu.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().Keliu;
                                    var fuwuPassengerTrafficg = listkekiuGuwenPass.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().fuwuPassengerTraffic;
                                    var JianquKeliug = listkekiuGuwenJiangqu.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().JianquKeliu;
                                    kuadianKeliu1 = (fuwuPassengerTrafficg - JianquKeliug) - (fuwuPassengerTrafficg - JianquKeliug);
                                    Keliu1 = keliug;
                                    fuwuPassengerTraffic1 = (fuwuPassengerTrafficg - JianquKeliug);
                                    sumkeliu = Keliu1 + fuwuPassengerTraffic1 + kuadianKeliu1;// listPage.Sum(x => x.PassengerTraffic) + listPage.Sum(x => x.fuwuPassengerTraffic);
                                    DixinTwo(mod, item, (decimal)sumkeliu);//底薪2

                                    mod.MrsDixin2keliu = (decimal)sumkeliu;
                                    mod.MrsDixin1 = mod.MrsDixin1 < 1000 ? 1000 : mod.MrsDixin1;
                                    mod.MrsDixin2 = mod.MrsDixin2 < 1000 ? 1000 : mod.MrsDixin2;
                                    mod.DixinSalary = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2)) < 2000 ? 2000 : (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪

                                    if (_isAchieGW == 1)
                                        mod.DixinSalary = mod.DixinSalary * mod.ChuqingDay / dayCount;
                                    //seachPatientshangyue.DepartmentID = DePartMentID;
                                    istkekiuCustomerService = 0;
                                    var BenyueRentou = 2;
                                    //istkekiuCustomerService = listkekiuGuwenRentouCustomerService.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().CustomerService;
                                    //var BenyueRentou = listkekiuGuwenRentou.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().BuyCount;
                                    //ShangyueRentou = listkekiuGuwenRentoushangyue.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().BuyCount;// iCentBll.PageListPassengerTraffic(seachPatientshangyue).Sum(x => x.BuyCount);
                                    GetBijiaoRentou(entity, BenyueRentou, Convert.ToInt32(ShangyueRentou), mod, _deductMoney1);
                                }
                                mod.BijiaoRentou = mod.BijiaoRentou != 0 ? mod.BijiaoRentou : 0;
                                mod.BijiaoRentouKoukuang = mod.BijiaoRentouKoukuang != 0 ? mod.BijiaoRentouKoukuang : 0;
                                #endregion
                                //var d_sumkeliu = 0;//店总流量
                                //var d_newkeliu = 0; //店新客流量
                                //if (kel.Count() > 0)
                                //{
                                //    var infoo = kel[0];
                                //    d_sumkeliu = infoo.PassengerTraffic + infoo.fuwuPassengerTraffic; //个人总客流
                                //    d_newkeliu = infoo.CustomerService;

                                //}
                                seachPatient.DepartmentID = DePartMentID; //小组
                                var d_sumkeliu = Convert.ToInt32(sumkeliu);
                                var d_newkeliu = Convert.ToInt32(istkekiuCustomerService);//listkekiuDe.Sum(x => x.CustomerService); //店新客流量
                                #region 业绩提成
                                seachentity.DepartmentID = DePartMentID;

                                //sum_zhid = cashBll.GetSumPerformanceSummary(seachentity);//求汇总 指定
                                //var sumzhDepartMent = sum_zhid.Yeji_Chuzhika + sum_zhid.Yeji_Liaochengka + sum_zhid.Yeji_Xiangmu +
                                //sum_zhid.Yeji_Chanpin + sum_zhid.Yeji_TeshuChanpin + sum_zhid.Yeji_Daxiangmu +
                                //sum_zhid.KakouLiaoCheng + sum_zhid.Yeji_Xiangmu + sum_zhid.KakouYeji_Chanpin +
                                //sum_zhid.KakouYeji_TeshuChanpin + sum_zhid.KakouYeji_Teshuxiangmu + sum_zhid.KakouYeji_Daxiangmu +
                                //sum_zhid.ShiCao;
                                seachentity.IsZhiding = -1;//非指定
                                                           //var sum_feizhidsd = cashBll.GetSumPerformanceSummary(seachentity);//求汇总 非指定
                                                           //var sumzhDepartMent2 = sum_feizhidsd.Yeji_Chuzhika + sum_feizhidsd.Yeji_Liaochengka + sum_feizhidsd.Yeji_Xiangmu +
                                                           //sum_feizhidsd.Yeji_Chanpin + sum_feizhidsd.Yeji_TeshuChanpin + sum_feizhidsd.Yeji_Daxiangmu +
                                                           //sum_feizhidsd.KakouLiaoCheng + sum_feizhidsd.Yeji_Xiangmu + sum_feizhidsd.KakouYeji_Chanpin +
                                                           //sum_feizhidsd.KakouYeji_TeshuChanpin + sum_feizhidsd.KakouYeji_Teshuxiangmu + sum_feizhidsd.KakouYeji_Daxiangmu +
                                                           //sum_feizhidsd.ShiCao;
                                                           //var sumzhDepartMent =  sumzhDepartMent2;
                                                           //mod.HospitalRewardALL = sumzhDepartMent / 2;
                                var yejitc = GetYejiTi(mod, HospitalYeji, mod.HospitalRewardALL, shopownerAche, shopownerDJCXJ);
                                mod.XiangmuYeji = XiangmuYeji;
                                var yejitcdaxiangmu = GetYejiTiDaxianmu(mod, HospitalYeji, XiangmuYeji, shopownerAche, shopownerDJCXJ);
                                mod.MrsshicaoStr = (yejitc + yejitcdaxiangmu).ToString();
                                //mod.JixiaoTicheng = yejitc;//绩效提成
                                #endregion
                                if (_isFullAttendGW == 1)
                                {
                                    quangqingjiang = (decimal)50;
                                    mod.Mrsquangqingjiang = quangqingjiang;
                                }
                                mod.Mrsquangqingjiang = mod.Mrsquangqingjiang > 0 ? mod.Mrsquangqingjiang : 0;
                                //mod.JixiaoReward = Convert.ToInt32(GZZZ.Split(',')[0] == ""?"": GZZZ.Split(',')[0]);//业绩奖金; 
                                //mod.KoukuanDuty = Convert.ToInt32(GZZZ.Split(',')[1] == "" ? "" : GZZZ.Split(',')[1]);//职责罚扣; 
                                mod.ChaoxiuQingjiaDay = 0;
                                if (mod.QingjiaDay > 6)
                                {
                                    mod.ChaoxiuQingjiaDay = mod.QingjiaDay - 6;
                                    var ChaoxiuKoukuang = (mod.QingjiaDay - 6) * 1.5 * (mod.JixiaoTicheng / mod.ChuqingDay);
                                    mod.ChaoxiuKoukuang = (decimal)ChaoxiuKoukuang;
                                }
                                #region 业绩奖金
                                int GZZZ = GetGZZhize(HospitalYejiDEe, d_sumkeliu, d_newkeliu, asverBeauDuty);
                                mod.JiangliDuty = GZZZ;
                                mod.sumkeliu = d_sumkeliu;
                                mod.newkeliu = d_newkeliu;
                                mod.JixiaoTicheng = yejitc + yejitcdaxiangmu;//绩效提成
                                mod.JixiaoReward = Convert.ToInt32(mod.BijiaoRentouKoukuang + mod.JiangliDuty);
                                #endregion
                                if (jobid == 3)
                                {
                                    newlist.Add(mod);
                                }

                            }
                        }
                        if (jobid == 4 || jobid == 0)
                        {
                            #region 获取单条统计
                            //var mod = reserBLL.SelectKaoqingtongji(new DakaRecordEntity()
                            //{ UserID = info.UserID, Years = entity.Years, Months = entity.Months });
                            //考勤统计
                            mod.UserName = info.UserName;
                            mod.UserPost = info.DutyName;

                            //考勤全勤客流项目奖
                            decimal qqklxmj = 0;
                            decimal quangqingjiang = 0;
                            //全勤
                            var keliu = listkekiu.Where(x => x.UserID == info.UserID).ToList();
                            var getxiangmu = xiangmulist.Where(x => x.UserID == info.UserID).ToList();
                            decimal pingjukeliu = 0;
                            decimal xiangmu = 0;

                            var res = result.Where(x => x.UserID == info.UserID).ToList();
                            var regzhiding = resultzhiding.Where(x => x.UserID == info.UserID).ToList();

                            //店综合业绩
                            var sumzh = sum_zhid.Yeji_Chuzhika + sum_zhid.Yeji_Liaochengka + sum_zhid.Yeji_Xiangmu +
                                    sum_zhid.Yeji_Chanpin + sum_zhid.Yeji_TeshuChanpin + sum_zhid.Yeji_Daxiangmu +
                                    sum_zhid.KakouLiaoCheng + sum_zhid.Yeji_Xiangmu + sum_zhid.KakouYeji_Chanpin +
                                    sum_zhid.KakouYeji_TeshuChanpin + sum_zhid.KakouYeji_Teshuxiangmu + sum_zhid.KakouYeji_Daxiangmu +
                                    sum_zhid.ShiCao;
                            mod.HospitalRewardALL = sumzh / 2;
                            #endregion
                            if (mod.UserPost == "前台")
                            {
                                #region 底薪2
                                var _receptionSal = ("," + receptionSal).Split('}');
                                var item = _receptionSal;
                                var sumkeliu = Convert.ToInt32(Keliu + fuwuPassengerTraffic + kuadianKeliu); ;
                                int Isqiantai = 1;//1是 0非
                                DixinTwo(mod, item, (decimal)sumkeliu, Isqiantai);
                                var qresult = result.Where(t=>t.DutyName=="前台").ToList();
                                if(qresult.Count() > 0)
                                mod.ShouGongFei = qresult[0].Ticheng;
                                mod.MrsDixin2keliu = (decimal)sumkeliu;
                                mod.DixinSalary = mod.DixinSalary < 1900 ? 1900 : mod.DixinSalary;
                                if (mod.DixinSalary < 1900) mod.DixinSalary = 1900;
                                if (mod.DixinSalary > 2300) mod.DixinSalary = 2300;

                                if (_isAchieQT == 1)
                                    mod.DixinSalary = mod.DixinSalary * mod.ChuqingDay / dayCount;
                                #endregion
                                #region 业绩提成
                                var yejitc = GetYejiTi(mod, HospitalYeji, HospitalYeji, receptionAche, receptionDJCXJ);
                                mod.MrsshicaoStr = yejitc.ToString();//绩效提成
                                mod.JixiaoTicheng = yejitc;//业绩资金

                                if (_isFullAttendQT == 1)
                                {
                                    quangqingjiang = (decimal)50;
                                    mod.Mrsquangqingjiang = quangqingjiang;
                                }
                                #endregion

                                if (jobid == 4)
                                {
                                    newlist.Add(mod);
                                }
                            }
                        }
                        if (jobid == 0)
                        {

                            mod.GonglingSubsidy = 0;
                            mod.JiabanSalary = 0;
                            mod.Shibao = 0;
                            mod.Gongjijin = 0;
                            mod.Other = 0;
                            mod.YingfaSalary = mod.DixinSalary + mod.JixiaoReward + mod.JixiaoTicheng
                                                + mod.QuanqingReward + mod.GonglingSubsidy
                                                + mod.JiabanSalary
                                                +Convert.ToInt32(mod.ShouGongFei)
                                                + mod.Gongjijin - mod.Shibao;
                            mod.ShifaSalary = mod.YingfaSalary - mod.Other - mod.KoukuanDuty;
                            newlist.Add(mod);
                        }

                    }
                }
            }
            catch { }
            ViewBag.days = days;
            return newlist;
        }

        /// <summary>
        /// 获取薪资数据
        /// </summary>
        /// <param name="dakaRecordEntity"></param>
        private List<DakaRecordEntity> GetSalarySatisticsF1(DakaRecordEntity dakaRecordEntity, int jobid = 0)
        {
            #region
            #region 变量
            var entity = dakaRecordEntity;
            var rr = entity.PapeID;
            int numberpage = 1;
            if (rr == "第一页") {
                numberpage = 1;
            }
            if (rr == "第二页")
            {
                numberpage = 2;
            }
            if (rr == "第三页")
            {
                numberpage = 3;
            }
            if (rr == "第四页")
            {
                numberpage = 4;
            }
            if (rr == "第五页")
            {
                numberpage = 5;
            }
            if (rr == "第六页")
            {
                numberpage = 6;
            }
            if (rr == "第七页")
            {
                numberpage = 7;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.HospitalID == 0)
                entity.HospitalID = LoginUserEntity.HospitalID;
            if (entity.Years == 0)
                entity.Years = DateTime.Now.Year;
            if (entity.Months == 0)
                entity.Months = DateTime.Now.Month;
            //考勤
            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Years = entity.Years;
            ViewBag.Months = entity.Months;

            ViewBag.days = days;
            List<DakaRecordEntity> newlist = new List<DakaRecordEntity>();
            if (dakaRecordEntity.HospitalID == 0) {
                return null;
            }
            else
            {
                //业绩
                int rows;
                int pagecount;
                var seachentity = new _SeachEntity();
                if (seachentity.s_HospitalID == 0) seachentity.s_HospitalID = entity.HospitalID;
                seachentity.s_StareTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
                seachentity.s_Endtime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());
                seachentity.numPerPage = 50; //每页显示条数
                if (seachentity.orderField + "" == "") { seachentity.orderField = "CreateTime"; }
                IList<JoinUserEntity> result = null;
                IList<JoinUserEntity> resultzhidingmrs = null;
                IList<JoinUserEntity> resultzhidingduadian = null;
                IList<JoinUserEntity> resultzhifeidingduadian = null;
                IList<JoinUserEntity> resultfeizhidingmrs = null;
                IList<JoinUserEntity> resultfeizhidingduadian = null;
                IList<JoinUserEntity> resultfeizhifeidingduadian = null;
                JoinUserEntity dianzhang = null;
                JoinUserEntity sum_zhid = null;
                JoinUserEntity sum_feizhid = null;
                var entityFangd = new _SeachEntity();
                if (entityFangd.s_HospitalID == 0)
                {
                    entityFangd.s_HospitalID = entity.HospitalID;
                }
                ViewBag.HospitalID = entityFangd.s_HospitalID;
                DateTime dtFirst;
                DateTime dtLast;


                var dt = ConvertValueHelper.ConvertDateTimeValueNew(entity.Years + "-" + entity.Months + "-" + "01");

                //本月第一天时间
                dtFirst = ConvertValueHelper.ConvertDateTimeValueNew(dt.AddDays(1 - (dt.Day)).ToString("yyyy-MM-dd 00:00:00"));
                //获得某年某月的天数
                var dayCount = DateTime.DaysInMonth(dt.Year, dt.Month);
                //本月最后一天时间
                dtLast = ConvertValueHelper.ConvertDateTimeValueNew(dtFirst.AddDays(dayCount - 1).ToString("yyyy-MM-dd 23:59:59"));

                //客流
                var seachPatient = new PassengerTrafficEntity();
                seachPatient.HospitalID = entity.HospitalID;
                seachPatient.searchStartTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
                seachPatient.searchEndTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());

                var daysgw = 0;
                if (entity.Months == 1) {
                    daysgw = DateTime.DaysInMonth(Convert.ToInt32(entity.Years)-1, Convert.ToInt32(12));
                }
                else
                {
                    daysgw = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months) - 1);
                }
                var shangyueendtime = DateTime.Now;
                if (entity.Months == 1)
                {
                    daysgw = DateTime.DaysInMonth(Convert.ToInt32(entity.Years) - 1, Convert.ToInt32(12));

                    shangyueendtime = Convert.ToDateTime(("" + Convert.ToInt32(2020) + "-" + Convert.ToInt32(12) + "-" + daysgw + " 23:59:59").ToString());
                }
                else
                {
                    daysgw = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months) - 1);

                    shangyueendtime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months - 1) + "-" + daysgw + " 23:59:59").ToString());
                }
                if (string.IsNullOrWhiteSpace(entity.orderField)) seachPatient.orderField = "UserID";
                var listkekiuGuwenRentou = new List<PassengerTrafficEntity>();
                var listkekiuGuwenKwliu = new List<PassengerTrafficEntity>();
                var listkekiuGuwenPass = new List<PassengerTrafficEntity>();
                var listkekiuGuwenJiangqu = new List<PassengerTrafficEntity>();
                var listkekiuGuwenRentoushangyue = new List<PassengerTrafficEntity>();
                var listkekiuGuwenRentouCustomerService = new List<PassengerTrafficEntity>();
                var entityxm = new ItemDetailsConsumptionEntity();
                decimal huaci;
                decimal kakou;
                decimal shougong;
                decimal jiangli;
                decimal xianjin;
                decimal Quantity;
                int UserCount;
                entityxm.HospitalID = entityxm.HospitalID == 0 ? entity.HospitalID : entityxm.HospitalID;
                entityxm.numPerPage = 50; //每页显示条数
                entityxm.StartTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
                entityxm.EndTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());
                if (entityxm.orderField + "" == "") { entityxm.orderField = "CreateTime"; entityxm.orderDirection = "desc"; }
                var entitys = new SystemUserSettingsEntity();
                entitys.HospitalID = entity.HospitalID;


                StoreDataSummaryEntity entityss = new StoreDataSummaryEntity();
                var o = DateTime.MinValue;
                entityss.StartTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
                entityss.EndTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());
                if (entityss.HospitalID == 0) entityss.HospitalID = entity.HospitalID;
                var mod1 = cashBll.GetStoreDataSummary(entityss);
                var listkekiushang = new StoreDataSummaryEntity();
                if (jobid == 2)
                {

                    StoreDataSummaryEntity seachPatients = new StoreDataSummaryEntity();
                    if (seachPatients.HospitalID == 0) seachPatients.HospitalID = entity.HospitalID;
                    seachPatients.StartTime = entityss.StartTime.AddMonths(-1);
                    seachPatients.EndTime = shangyueendtime;// entityss.EndTime.AddMonths(-1); 
                    listkekiushang = cashBll.GetStoreDataSummary(seachPatients);
                }
                var seachPatientshangyue = new PassengerTrafficEntity();
                seachPatientshangyue.HospitalID = entity.HospitalID;
                seachPatientshangyue.searchStartTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
                seachPatientshangyue.searchEndTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());

                if (string.IsNullOrWhiteSpace(entity.orderField)) seachPatientshangyue.orderField = "UserID";
                seachPatientshangyue.searchStartTime = seachPatientshangyue.searchStartTime.AddMonths(-1);
                seachPatientshangyue.searchEndTime = shangyueendtime;// seachPatientshangyue.searchEndTime.AddMonths(-1);
                decimal Keliu = 0, fuwuPassengerTraffic = 0, kuadianKeliu = 0, kuadianKeliuC = 0;
                #endregion

                var entityl = new LeaveRecordsEntity();
                entityl.HospitalID = entity.HospitalID;
                entityl.numPerPage = 50; //每页显示条数
                if (string.IsNullOrEmpty(entity.StartTime))
                {

                    entityl.StartTime = ("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString();
                    entityl.EndTime = ("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString();
                }

                var list1 = reserBLL.GetLeaveRecordsList1(entityl);

                list1 = list1.Where(t => t.category == "d91b4hykqlfm" && t.StartYear == entity.Years && t.StartMoths == entity.Months).OrderByDescending(x =>x.Dateleave).ToList();
                //if (list1.Count() > 0) {
                //    list1 = list1.fi;
                //}



                var list = personBLL.GetAllUser(new HospitalUserEntity { IsActive = 1, HospitalID = entity.HospitalID });

                list = list.Where(i => i.IsSys != 1 && i.IsAPP != 1).ToList();
                if (jobid == 1)
                {
                    list = list.Where(x => x.DutyName == "美容师").ToList();
                    ViewBag.MrsCount = list.Count();
                    list = list.AsQueryable().ToPagedList(numberpage, 12).ToList();
                }
                if (jobid == 2)
                {
                    list = list.Where(x => x.DutyName == "店长").ToList();
                }
                if (jobid == 3)
                {
                    list = list.Where(x => x.DutyName == "顾问").ToList();
                }
                if (jobid == 4)
                {
                    list = list.Where(x => x.DutyName == "前台").ToList();
                }
                //list = list.OrderByDescending(x => x.DutyName).ToList(); 

                result = cashBll.GetPerformanceSummary(seachentity, out rows, out pagecount);//默认全部
                seachentity.IsZhiding = 1;//指定
                var resultzhiding = cashBll.GetPerformanceSummary(seachentity, out rows, out pagecount);//指定
                if (jobid == 1 || jobid == 0)
                {
                    seachentity.IsZhiding = 0;//非指定  
                    resultfeizhidingmrs = cashBll.GetPerformanceSummary(seachentity, out rows, out pagecount);//非指定
                    seachentity.IsZhiding = 1;//指定
                    resultzhidingmrs = cashBll.GetPerformanceSummary(seachentity, out rows, out pagecount);//指定
                }
                seachentity.IsZhiding = -1;//全部
                sum_feizhid = cashBll.GetSumPerformanceSummary(seachentity);//求汇总 默认全部
                var HospitalYeji = sum_feizhid.Yeji_Chuzhika + sum_feizhid.Yeji_Liaochengka + sum_feizhid.Yeji_Chanpin + sum_feizhid.Yeji_TeshuChanpin + sum_feizhid.Yeji_Xiangmu + sum_feizhid.Yeji_Teshuxiangmu;
                var HospitalYejiJx = HospitalYeji;
                var dzhospitalyeji = HospitalYeji;
                decimal dzhospitalglyj = 0;
                decimal dzhospitalallglyj = 0;
                decimal HospitalZhongheYeji = 0;
                decimal HospitalDaxiangmuYeji = 0;
                if (jobid == 2 || jobid == 4)
                {
                    //综合业绩=【基础项目现金业绩+实操业绩+卡扣家居 + 现金家居】÷2  
                    HospitalZhongheYeji = (HospitalYeji + sum_feizhid.ShiCao + sum_feizhid.KakouYeji_Chanpin + sum_feizhid.KakouYeji_TeshuChanpin + +sum_feizhid.Yeji_Chanpin + sum_feizhid.Yeji_TeshuChanpin) / 2;
                    //HospitalZhongheYeji = (HospitalYeji + sum_feizhid.ShiCao + sum_feizhid.KakouYeji_Chanpin + sum_feizhid.KakouYeji_TeshuChanpin) / 2;
                    HospitalDaxiangmuYeji = sum_feizhid.Yeji_Daxiangmu;
                } 
                var listPers = personBLL.GetAllUser(new HospitalUserEntity { HospitalID = entityFangd.s_HospitalID, IsActive = 1 })
                    .Where(c => c.IsSeparation == 0 && c.IsSys != 1).ToList();
                if (jobid == 3 || jobid == 0)
                {
                    listkekiuGuwenRentou = iCentBll.PageListPassengerTrafficGuWenRentou(seachPatient);
                    listkekiuGuwenKwliu = iCentBll.PageListPassengerTrafficGuWenkeliu(seachPatient);
                    listkekiuGuwenPass = iCentBll.PageListPassengerTrafficGuWenfuwuPassengerTraffic(seachPatient);
                    listkekiuGuwenJiangqu = iCentBll.PageListPassengerTrafficGuWenJianquKeliu(seachPatient);
                    listkekiuGuwenRentouCustomerService = iCentBll.PageListPassengerTrafficGuWenCustomerService(seachPatient);
                    listkekiuGuwenRentoushangyue = iCentBll.PageListPassengerTrafficGuWenRentou(seachPatientshangyue);

                }
                var listkekiu = iCentBll.PageListPassengerTraffic(seachPatient);
                var listkekiushanggw = new List<PassengerTrafficEntity>();
                if (jobid == 3)
                {

                    PassengerTrafficEntity seachPatientsgw = new PassengerTrafficEntity();
                    if (seachPatientsgw.HospitalID == 0) seachPatientsgw.HospitalID = entity.HospitalID;
                    seachPatientsgw.searchStartTime = seachPatient.searchStartTime.AddMonths(-1);
                    seachPatientsgw.searchEndTime = shangyueendtime;// seachPatient.searchEndTime.AddMonths(-1);
                    listkekiushanggw = iCentBll.PageListPassengerTraffic(seachPatientsgw);
                }
                if (jobid == 2 || jobid == 4 || jobid == 0)
                {

                    seachPatient.IsKuaDian = 1;
                    var fuwuPassengerTrafficsum = 0;
                    if (jobid != 0)
                        fuwuPassengerTrafficsum = iCentBll.PageGetfuwuPassengerTraffic(seachPatient).Where(x => x.DepartmentID == 1).FirstOrDefault().fuwuPassengerTraffic;//慢
                    foreach (var en in listkekiu)
                    {

                        kuadianKeliuC += en.fuwuPassengerTraffic;
                        Keliu += en.Keliu;
                        fuwuPassengerTraffic += (en.fuwuPassengerTraffic - en.JianquKeliu);
                    }
                    if (jobid != 0)
                        kuadianKeliu = fuwuPassengerTrafficsum - kuadianKeliuC;
                }
                var xiangmulist = cashBll.GetItemDetailsConsumptionList(entityxm, out huaci, out kakou, out shougong, out jiangli, out xianjin, out Quantity, out UserCount);
                //考勤参数
                var sysSalary = IsystemBLL.GetSystemUserSettingsModelByPay(entitys);
                #endregion
                //逻辑
                //try
                //{
                if (!string.IsNullOrEmpty(sysSalary.AttributeValue))
                {
                    var syssalary = sysSalary.AttributeValue.Replace("|||", "★").Replace("\"", "").Replace("[", "").Replace("]", "");
                    var arrsys = syssalary.Split('★');
                    if (arrsys.Count() > 0)
                    {
                        #region 参数赋值
                        Dictionary<string, string> arr = new Dictionary<string, string>();
                        for (int i = 0; i < arrsys.Length; i++)
                        {
                            arr.Add(arrsys[i], arrsys[i + 1]);
                            i++;
                        }
                        var asverBeauDuty1 = "";
                        var isExceedRestAdver = arr["isExceedRestAdver"];
                        var isExceedRestshopowner = arr["isExceedRestshopowner"];
                        var specialProduct = arr["specialProduct"];
                        var specialProject = arr["specialProject"];
                        var BeauticianSal = arr["BeauticianSal"];
                        var msr_xktc = arr["newGuests"];
                        var mrs_scyj_arr = arr["storeRevenue"];
                        var mrs_djcxj_arr = arr["personalRoyalty"];
                        var shopownerSal = arr["shopownerSal"];
                        var shopownerAche = arr["shopownerAche"];
                        var advererAche = arr["advererAche"];
                        var adviserSal = arr["adviserSal"];
                        var asverBeauDuty = arr["asverBeauDuty"];
                        try
                        {
                            asverBeauDuty1 = arr["asverBeauDuty1"];
                        }
                        catch { }
                        var receptionSal = arr["receptionSal"];
                        var receptionAche = arr["receptionAche"];
                        var operationPerformance = arr["operationPerformance"];
                        var isShopownerCheck = arr["isShopownerCheck"];
                        var deductMoney = arr["deductMoney"];
                        var beautReward = arr["beautReward"];
                        var beautReward1 = arr["beautReward1"];
                        var BeautYSJY = arr["BeautYSJY"];
                        var BeautDJCXJ = arr["BeautDJCXJ"];
                        var shopownerDJCXJ = arr["shopownerDJCXJ"];
                        var adviserDJCXJL = arr["adviserDJCXJL"];

                        var isAccum = arr["isAccum"];
                        var accum = arr["accum"];
                        var isAchieMRS = arr["isAchieMRS"];
                        var isAchieQT = arr["isAchieQT"];
                        var isAchieDZ = arr["isAchieDZ"];
                        var isAchieGW = arr["isAchieGW"];
                        var cardBuckle = arr["cardBuckle"];
                        var isBeauticianCheckQT = arr["isBeauticianCheckQT"];
                        var _isBeauticianCheckQT = Convert.ToInt32(string.IsNullOrEmpty(isBeauticianCheckQT) ? "0" : isBeauticianCheckQT);
                        var _isAchieMRS = Convert.ToInt32(string.IsNullOrEmpty(isAchieMRS) ? "0" : isAchieMRS);
                        var _isAchieQT = Convert.ToInt32(string.IsNullOrEmpty(isAchieQT) ? "0" : isAchieQT);
                        var _isAchieDZ = Convert.ToInt32(string.IsNullOrEmpty(isAchieDZ) ? "0" : isAchieDZ);
                        var _isAchieGW = Convert.ToInt32(string.IsNullOrEmpty(isAchieGW) ? "0" : isAchieGW);
                        var HomeProTiChen = arr["HomeProTiChen"];
                        var isSpecial = arr["isSpecial"];
                        var isHomePro = arr["isHomePro"];
                        var isFullAttendMRS = arr["isFullAttendMRS"];
                        var FullAttendMRS = arr["FullAttendRewMRS"];
                        var isFullAttendGW = arr["isFullAttendGW"];
                        var FullAttendGW = arr["FullAttendRewGW"];
                        var isFullAttendDZ = arr["isFullAttendDZ"];
                        var FullAttendDZ = arr["FullAttendRewDZ"];
                        var isFullAttendQT = arr["isFullAttendQT"];
                        var FullAttendQT = arr["FullAttendRewQT"];
                        var deductMoney1 = arr["deductMoney1"];
                        var receptionDJCXJ = arr["receptionDJCXJ"];
                        var _isFullAttendMRS = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendMRS) ? "0" : isFullAttendMRS);
                        var _isFullAttendGW = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendGW) ? "0" : isFullAttendGW);
                        var _isFullAttendDZ = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendDZ) ? "0" : isFullAttendDZ);
                        var _isFullAttendQT = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendQT) ? "0" : isFullAttendQT);
                        var _deductMoney1 = Convert.ToDecimal(string.IsNullOrEmpty(deductMoney1) ? "0" : deductMoney1);
                        var _deductMoney = Convert.ToDecimal(string.IsNullOrEmpty(deductMoney) ? "0" : deductMoney);
                        var dt1 = ConvertValueHelper.ConvertDateTimeValueNew(entity.Years + "-" + entity.Months + "-" + "01");
                        var dayCount1 = DateTime.DaysInMonth(dt1.Year, dt1.Month);
                        #endregion

                        int DePartMentID = -1;

                        int test = 0;
                        var _yingxianggongxiu = "0";
                        try
                        {
                            if (!string.IsNullOrEmpty(entity.HospitalID.ToString()))
                            {
                                int Inqdays = Convert.ToInt32(entity.HospitalID.ToString());
                                string strsql = @"select top 10 YingXiangGongXiu from EYB_tb_Hospital WHERE ID =  " + Convert.ToInt32(Inqdays) + "";
                                _yingxianggongxiu = GetIntegralByMonth(strsql, 1);
                            }
                        }
                        catch
                        {
                        }
                        foreach (var info in list)
                        {


                            var mod = reserBLL.SelectKaoqingtongji(new DakaRecordEntity()
                            { UserID = info.UserID, Years = entity.Years, Months = entity.Months });

                            #region 超修

                            if (string.IsNullOrEmpty(_yingxianggongxiu)) { _yingxianggongxiu = "0"; }
                            else
                            {
                                mod.Yingxianggongxiu = Convert.ToInt32(_yingxianggongxiu);
                            };
                            int SalaryDay = 0;
                            if (mod.ChuqingDay < (dayCount - mod.Yingxianggongxiu))
                            {
                                if (_isAchieDZ == 1)
                                {
                                    //考勤天数 =出勤天数 + 出勤天数/7
                                    SalaryDay = mod.ChuqingDay + Convert.ToInt32(Math.Floor(Convert.ToDecimal(mod.ChuqingDay / 7)));
                                }
                                else
                                {
                                    //考勤天数 =出勤天数 + 出勤天数/8
                                    SalaryDay = mod.ChuqingDay + Convert.ToInt32(Math.Floor(Convert.ToDecimal(mod.ChuqingDay / 8)));// mod.ChuqingDay / 8;

                                }
                            }
                            else
                            {
                                SalaryDay = dayCount;
                            }
                            mod.Chaoxiuday = mod.XiuxiDay - Convert.ToInt32(_yingxianggongxiu);
                            mod.Chaoxiuday = mod.Chaoxiuday > 0 ? mod.Chaoxiuday : 0;
                            #endregion
                            if (jobid == 1 || jobid == 0)
                            {
                                try
                                {
                                    #region 获取单条统计
                                    //考勤统计
                                    mod.UserName = info.UserName;
                                    mod.UserPost = info.DutyName;

                                    #endregion
                                    if (mod.UserPost == "美容师")
                                    {

                                        //test++;
                                        //if (test > 9) break;

                                        #region 变量
                                        //考勤全勤客流项目奖
                                        decimal qqklxmj = 0;
                                        decimal quangqingjiang = 0;
                                        decimal pingjukeliu = 0;
                                        decimal xiangmu = 0;
                                        var Dixin2 = 0;
                                        var _mrs_scyj_arr = ("," + mrs_scyj_arr).Split('}');//店基础现金总收入  计算参数

                                        var _mrs_djcxj_arr = ("," + mrs_djcxj_arr).Split('}');//个人特殊  计算参数

                                        decimal yejitc = 0;
                                        JoinUserEntity infoo1;
                                        decimal infoo1KakouYeji_Chanpin = 0; decimal infoo1KakouYeji_Liaocheng = 0;
                                        decimal infoo1Yeji_Chanpin = 0; decimal infoo1Yeji_Liaochengka = 0; decimal infoo1Yeji_TeshuChanpin = 0;

                                        var Dixin1 = 0;
                                        JoinUserEntity reszhidingkuandian1 = null;
                                        JoinUserEntity reszhidingfeikuandian1 = null;

                                        var sumzh = 1;//
                                        mod.HospitalRewardALL = sumzh / 2;
                                        var BeauticianSalarr = ("," + BeauticianSal).Split('}');
                                        var item = BeauticianSalarr;
                                        #endregion

                                        #region

                                        #endregion
                                        var res = result.Where(x => x.UserID == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                        #region 底薪1
                                        if (res.Count() > 0)
                                        {
                                            var infoo = res[0];
                                            var sumshichao = infoo.ShiCao_zhiding;
                                            DixinOne(res[0], mod, item, sumshichao);//底薪1
                                        }
                                        #endregion
                                        #region 底薪2

                                        var kel = listkekiu.Where(x => x.UserID == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                        if (kel.Count() > 0)
                                        {
                                            var infoo = kel[0];
                                            seachPatient.IsKuaDian = 1;
                                            seachPatient.UserID = info.UserID;
                                            var ent = iCentBll.PageListPassengerTraffic(seachPatient).Where(x => x.UserID == info.UserID && x.HospitalID == entity.HospitalID).FirstOrDefault();
                                            var kuandiankeliu = (ent.fuwuPassengerTraffic - ent.JianquKeliu) - (infoo.fuwuPassengerTraffic - infoo.JianquKeliu);
                                            var sumkeliu = (infoo.fuwuPassengerTraffic - infoo.JianquKeliu) + infoo.Keliu + kuandiankeliu;
                                            DixinTwo(mod, item, sumkeliu);//底薪2
                                            mod.MrsDixin2 = mod.MrsDixin2;// < 500 ? 500 : mod.MrsDixin2;
                                            mod.MrsDixin2keliu = (decimal)sumkeliu;
                                            /* 新客*/
                                        }

                                        mod.MrsDixin1 = mod.MrsDixin1 > 0 ? mod.MrsDixin1 : Convert.ToInt32(item[5].Split(',')[2].Split(':')[1]);
                                        mod.MrsDixin2 = mod.MrsDixin2 > 0 ? mod.MrsDixin2 : Convert.ToInt32(item[5].Split(',')[7].Split(':')[1]);
                                        mod.MrsDixin = Convert.ToInt32(mod.MrsDixin1) + Convert.ToInt32(mod.MrsDixin2);
                                        mod.DixinSalary = Convert.ToInt32(mod.MrsDixin1) + Convert.ToInt32(mod.MrsDixin2);//总底薪

                                        mod.JixiaoReward = Convert.ToInt32(quangqingjiang + qqklxmj + mod.NewCusReward);//绩效奖金
                                        mod.QuanqingReward = (int)quangqingjiang;
                                        #endregion
                                        #region 指定和非指定客和手工费可实操
                                        var _resultzhidingmrs = resultzhidingmrs.Where(x => x.UserID == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                        if (_resultzhidingmrs.Count() > 0)
                                        {
                                            var infoomrs = _resultzhidingmrs[0];
                                            mod.ShiCao_zhiding = infoomrs.ShiCao_zhiding;
                                        }

                                        var _resultfeizhidingmrs = resultfeizhidingmrs.Where(x => x.UserID == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                        if (_resultfeizhidingmrs.Count() > 0)
                                        {
                                            var infoomrsfei = _resultfeizhidingmrs[0];
                                            mod.ShiCao_feizhiding = infoomrsfei.ShiCao_zhiding;
                                        }
                                        var _operationPerformance = ("," + operationPerformance).Split('}');
                                        var _BeautYSJY = BeautYSJY.Replace("}", "").Split(',');
                                        var ag = 0; var tichengzhidingke = 0;//指定客提成
                                        var fg = 0; var feitichengzhidingke = 0;//非指定客提

                                        if (res.Count() > 0)
                                        {

                                            var info1 = res[0];
                                            var _MouthShiCaoTicheng_zhiding = mod.ShiCao_zhiding;// info1.ShiCao_zhiding + info1.kuadianShiCao_zhiding;
                                            var _MouthShiCaoTicheng_feizhiding = mod.ShiCao_feizhiding;// info1.ShiCao_feizhiding + info1.kuadianShiCao_feizhiding;
                                            var _MouthShiCao = _MouthShiCaoTicheng_zhiding + _MouthShiCaoTicheng_feizhiding;
                                            mod.MrsMouthShicao = _MouthShiCao;
                                            if (_MouthShiCao > Convert.ToDecimal(string.IsNullOrEmpty(_BeautYSJY[4].Split(':')[1]) ? "0" : _BeautYSJY[4].Split(':')[1]))
                                            {
                                                ag = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[0].Split(',')[4].Split(':')[1]) * 100);//指定客提成 * 100
                                                fg = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[1].Split(',')[4].Split(':')[1]) * 100);//非指定客提成 * 100

                                            }
                                            else if (_MouthShiCao > Convert.ToDecimal(string.IsNullOrEmpty(_BeautYSJY[2].Split(':')[1]) ? "0" : _BeautYSJY[2].Split(':')[1]))
                                            {
                                                ag = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[0].Split(',')[3].Split(':')[1]) * 100);//指定客提成 * 100
                                                fg = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[1].Split(',')[3].Split(':')[1]) * 100);//非指定客提成 * 100
                                            }
                                            else
                                            {
                                                ag = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[0].Split(',')[2].Split(':')[1]) * 100);//指定客提成 * 100
                                                fg = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[1].Split(',')[2].Split(':')[1]) * 100);//非指定客提成 * 100

                                            }
                                            mod.Mrsag = ag / 100;
                                            mod.Mrsfg = fg / 100;
                                            tichengzhidingke = Convert.ToInt32((decimal)_MouthShiCaoTicheng_zhiding * (decimal)ag / (decimal)10000);
                                            feitichengzhidingke = Convert.ToInt32((decimal)_MouthShiCaoTicheng_feizhiding * (decimal)fg / (decimal)10000);
                                            mod.Mrstichengzhidingke = tichengzhidingke + feitichengzhidingke;
                                        }
                                        var ShouGongFeiModel = result.Where(t => t.DutyName == mod.UserPost && t.UserName == mod.UserName && t.HospitalID == entity.HospitalID).ToList();
                                        if (ShouGongFeiModel.Count() > 0)
                                        {
                                            mod.ShouGongFei = ShouGongFeiModel[0].Ticheng;
                                            mod.Jiangli = ShouGongFeiModel[0].OtherTiCheng;
                                        }
                                        mod.Sumshicaoshougong = tichengzhidingke + feitichengzhidingke + mod.ShouGongFei + mod.Jiangli;
                                        #endregion

                                        #endregion

                                        mod.Yeji_tushuchangpingzhidingf = "";
                                        mod.Yeji_tushuxiangmuzhidingf = "";
                                        mod.MrsFangdangBenyuef = "";
                                        #region 现金业绩提成
                                        if (res.Count() > 0)
                                        {
                                            Dictionary<string, string> shopownerDJCXJ_item = JsonConvert.DeserializeObject<Dictionary<string, string>>(shopownerDJCXJ);
                                            var infoo = res[0];
                                            var keys = shopownerDJCXJ_item.Keys;
                                            #region  店基础现金总收入
                                            #endregion
                                            #region
                                            var iii = 7;
                                            var _isSpecial = Convert.ToInt32(string.IsNullOrEmpty(isSpecial) ? "0" : isSpecial);
                                            var _cardBuckle = Convert.ToDecimal(string.IsNullOrEmpty(cardBuckle) ? "0" : cardBuckle);
                                            if (_resultzhidingmrs.Count() > 0)
                                            {
                                                var infoomrs = _resultzhidingmrs[0];
                                                mod.Yeji_Chuzhikazhiding = infoomrs.Yeji_Chuzhika;
                                                mod.Yeji_liaochengdancizhiding = infoomrs.Yeji_Liaochengka + infoomrs.Yeji_Xiangmu;
                                                mod.Yeji_jiajuzhiding = infoomrs.Yeji_Chanpin;
                                                mod.Yeji_daxiangmuzhiding = infoomrs.Yeji_Daxiangmu;
                                            }
                                            if (_resultfeizhidingmrs.Count() > 0)
                                            {
                                                var infoomrsfei = _resultfeizhidingmrs[0];
                                                mod.Yeji_Chuzhikafeizhiding = infoomrsfei.Yeji_Chuzhika;
                                                mod.Yeji_liaochengdancifeizhiding = infoomrsfei.Yeji_Liaochengka + infoomrsfei.Yeji_Xiangmu;
                                                mod.Yeji_jiajufeizhiding = infoomrsfei.Yeji_Chanpin;
                                                mod.Yeji_daxiangmufeizhiding = infoomrsfei.Yeji_Daxiangmu;
                                            }
                                            for (int i = shopownerDJCXJ_item.Count - 1; i >= 0; i--)
                                            {
                                                var yejize = Convert.ToDecimal(shopownerDJCXJ_item["scope" + i]);
                                                i--;
                                                iii--;
                                                if (HospitalYeji > yejize || HospitalYeji == (decimal)yejize)
                                                {
                                                    mod.MrsHospitalYeji = HospitalYeji;
                                                    mod.Mrssdyeji = yejize;
                                                    yejitc = mod.Yeji_Chuzhikazhiding * Convert.ToDecimal(_mrs_scyj_arr[0].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人会员卡指定客提成
                                                    mod.Yeji_Chuzhikazhidingf = _mrs_scyj_arr[0].Split(',')[iii].Split(':')[1];
                                                    yejitc += (mod.Yeji_liaochengdancizhiding) * Convert.ToDecimal(_mrs_scyj_arr[2].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居指定客提成
                                                    mod.Yeji_liaochengdancizhidingf = _mrs_scyj_arr[2].Split(',')[iii].Split(':')[1];
                                                    yejitc += (mod.Yeji_jiajuzhiding) * Convert.ToDecimal(_mrs_scyj_arr[4].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居指定客提成
                                                    mod.Yeji_jiajuzhidingf = _mrs_scyj_arr[4].Split(',')[iii].Split(':')[1];


                                                    yejitc += mod.Yeji_Chuzhikafeizhiding * Convert.ToDecimal(_mrs_scyj_arr[1].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人会员卡非指定客提成
                                                    mod.Yeji_Chuzhikafeizhidingf = _mrs_scyj_arr[1].Split(',')[iii].Split(':')[1];
                                                    yejitc += (mod.Yeji_liaochengdancifeizhiding) * Convert.ToDecimal(_mrs_scyj_arr[3].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居非指定客提成
                                                    mod.Yeji_liaochengdancifeizhidingf = _mrs_scyj_arr[3].Split(',')[iii].Split(':')[1];
                                                    yejitc += (mod.Yeji_jiajufeizhiding) * Convert.ToDecimal(_mrs_scyj_arr[5].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居非指定客提成
                                                    mod.Yeji_jiajufeizhidingf = _mrs_scyj_arr[5].Split(',')[iii].Split(':')[1];
                                                    mod.Yeji_tushuchangpingzhidingf = "未开启";
                                                    mod.Yeji_tushuxiangmuzhidingf = "未开启";
                                                    if (_isSpecial == 1)//是特殊提成
                                                    {
                                                        yejitc += infoo.Yeji_TeshuChanpin * Convert.ToDecimal(specialProduct) / 100;
                                                        mod.Yeji_tushuchangpingzhiding = infoo.Yeji_TeshuChanpin;
                                                        mod.Yeji_tushuchangpingzhidingf = specialProduct.ToString() + "%";
                                                        yejitc += infoo.Yeji_Teshuxiangmu * Convert.ToDecimal(specialProject) / 100;
                                                        mod.Yeji_tushuxiangmuzhiding = infoo.Yeji_Teshuxiangmu;
                                                        mod.Yeji_tushuxiangmuzhidingf = specialProject.ToString() + "%";
                                                    }
                                                    yejitc += (mod.Yeji_daxiangmuzhiding) * Convert.ToDecimal(_mrs_djcxj_arr[0].Split(',')[1].Split(':')[1]) / 100;//实操提成大项目提成指定客提成
                                                    mod.Yeji_daxiangmuzhidingf = _mrs_djcxj_arr[0].Split(',')[1].Split(':')[1];
                                                    yejitc += (mod.Yeji_daxiangmufeizhiding) * Convert.ToDecimal(_mrs_djcxj_arr[0].Split(',')[2].Split(':')[1]) / 100;//实操提成_大项目个人提成非指定客
                                                    mod.Yeji_daxiangmufeizhidingf = _mrs_djcxj_arr[0].Split(',')[2].Split(':')[1];
                                                    #region
                                                    #endregion
                                                    break;
                                                }

                                            }

                                            var _isHomePro = Convert.ToInt32(string.IsNullOrEmpty(isHomePro) ? "0" : isHomePro);
                                            mod.MrsFangdangBenyuef = "未开启";
                                            if (_isHomePro == 1)
                                            {

                                                var _HomeProTiChen = Convert.ToDecimal(string.IsNullOrEmpty(HomeProTiChen) ? "0" : HomeProTiChen);
                                                var _listPers = listPers.Where(x => x.UserID == info.UserID).ToList();
                                                if (_listPers.Count() > 0)
                                                {
                                                    var Benyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                                                    {
                                                        s_HospitalID = entityFangd.s_HospitalID,
                                                        s_UserID = _listPers[0].UserID,
                                                        s_StareTime = dtFirst,
                                                        s_Endtime = dtLast,
                                                        ProductType = entityFangd.ProductType
                                                    });
                                                    if (Benyue > (decimal)0)
                                                    {
                                                        var Shangeyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                                                        {
                                                            s_HospitalID = entityFangd.s_HospitalID,
                                                            s_UserID = _listPers[0].UserID,
                                                            s_StareTime = dtFirst.AddMonths(-1),
                                                            s_Endtime = dtLast.AddMonths(-1),
                                                            ProductType = entityFangd.ProductType
                                                        });
                                                        var Qianyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                                                        {
                                                            s_HospitalID = entityFangd.s_HospitalID,
                                                            s_UserID = _listPers[0].UserID,
                                                            s_StareTime = dtFirst.AddMonths(-2),
                                                            s_Endtime = dtLast.AddMonths(-2),
                                                            ProductType = entityFangd.ProductType
                                                        });
                                                        if (Shangeyue > (decimal)0 || Qianyue > (decimal)0)
                                                        {
                                                            mod.MrsFangdang = Convert.ToInt32(Benyue * (decimal)_HomeProTiChen / 100);
                                                            yejitc += mod.MrsFangdang;
                                                            mod.MrsFangdangBenyue = Benyue;
                                                        }
                                                    }
                                                }
                                                mod.MrsFangdangBenyuef = _HomeProTiChen.ToString() + "%";
                                            }
                                            yejitc = Math.Round(yejitc, 2);
                                            mod.MrsFangdang = mod.MrsFangdang > 0 ? mod.MrsFangdang : 0;
                                            mod.MrsFangdangBenyue = mod.MrsFangdangBenyue > 0 ? mod.MrsFangdangBenyue : 0;
                                            #endregion
                                            mod.MrsshicaoStr = yejitc.ToString();

                                        }

                                        #endregion

                                        #region 卡扣业绩
                                        if (res.Count() > 0)
                                        {

                                            var _cardBuckle = Convert.ToDecimal(string.IsNullOrEmpty(cardBuckle) ? "0" : cardBuckle);
                                            var infoo = res[0];
                                            mod.Mrskaikouyeji = infoo.KakouYeji_Liaocheng + infoo.KakouYeji_Chanpin + infoo.KakouYeji_TeshuChanpin + infoo.KakouYeji_Xiangmu + infoo.KakouYeji_Daxiangmu + infoo.KakouYeji_Teshuxiangmu;
                                            mod.MrsSumkaikouyeji = mod.Mrskaikouyeji * _cardBuckle / 100;
                                            mod.Mrskaikouyejif = _cardBuckle.ToString();

                                        }
                                        #endregion

                                        mod.Mrstichengzhidingke = mod.Mrstichengzhidingke > 0 ? mod.Mrstichengzhidingke : 0;

                                        #region 奖励&扣罚

                                        if (kel.Count() > 0)
                                        {
                                            var infoo = kel[0];
                                            /* 新客*/
                                            var Customerarr = msr_xktc.Split(',');
                                            if (infoo.CustomerService == 0) { mod.NewCusReward = Convert.ToInt32(Customerarr[0].Split(':')[1]); }
                                            else if (infoo.CustomerService == 1) { mod.NewCusReward = Convert.ToInt32(Customerarr[1].Split(':')[1]); }
                                            else if (infoo.CustomerService == 2) { mod.NewCusReward = Convert.ToInt32(Customerarr[2].Split(':')[1]); }
                                            else if (infoo.CustomerService == 3) { mod.NewCusReward = Convert.ToInt32(Customerarr[3].Split(':')[1]); }
                                            else if (infoo.CustomerService == 4) { mod.NewCusReward = Convert.ToInt32(Customerarr[4].Split(':')[1]); }
                                            else { mod.NewCusReward = Convert.ToInt32(Customerarr[5].Split(':')[1].Replace("}", "")); }
                                            mod.CustomerService = infoo.CustomerService;
                                        }

                                        if (mod.ChuqingDay + mod.XiuxiDay == dayCount1)
                                        {
                                            var keliu = listkekiu.Where(x => x.UserID == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                            if (keliu.Count() > 0 && mod.ShijiChuqingDay > 0)
                                            {
                                                pingjukeliu = Convert.ToDecimal(mod.MrsDixin2keliu / mod.ShijiChuqingDay); //平均客流

                                            }
                                            var getxiangmu = xiangmulist.Where(x => x.UserID == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                            if (getxiangmu.Count() > 0)
                                            {
                                                xiangmu = Convert.ToDecimal(getxiangmu[0].ProjectNumber);//项目数
                                            }
                                            if (Getqqklxmj(beautReward1, pingjukeliu, xiangmu) == (decimal)0)
                                            {
                                                qqklxmj = Getqqklxmj(beautReward, pingjukeliu, xiangmu);
                                            }
                                            else
                                            {
                                                qqklxmj = Getqqklxmj(beautReward1, pingjukeliu, xiangmu);
                                            }
                                            mod.Mrspingjukeliu = pingjukeliu;
                                            mod.Mrsxiangmu = xiangmu;
                                            mod.Mrsqqklxmj = qqklxmj;
                                            if (_isFullAttendMRS == 1)
                                            {
                                                quangqingjiang = (decimal)50;
                                                mod.Mrsquangqingjiang = quangqingjiang;
                                            }
                                        }
                                        mod.Mrsqqklxmj = mod.Mrsqqklxmj > 0 ? mod.Mrsqqklxmj : 0;
                                        mod.Mrsdanxiangmuf = "";
                                        if (res.Count() > 0)
                                        {
                                            //var infoo = res[0];
                                            //var _isAccum = Convert.ToInt32(string.IsNullOrEmpty(isAccum) ? "0" : isAccum);
                                            //var _accum = Convert.ToInt32(string.IsNullOrEmpty(accum) ? "0" : accum);
                                            //if (_isAccum == 1)
                                            //{
                                            //    mod.Mrsdanxiangmu = Convert.ToInt32(infoo.Yeji_Xiangmu) > _accum ? _accum : Convert.ToInt32(infoo.Yeji_Xiangmu);
                                            //    mod.Mrsdanxiangmuf = mod.Mrsdanxiangmu.ToString();
                                            //}
                                            //else
                                            //{
                                            //    mod.Mrsdanxiangmu = Convert.ToInt32(infoo.Yeji_Xiangmu);
                                            //    mod.Mrsdanxiangmuf = "未开启".ToString();
                                            //}

                                            mod.Mrsdanxiangmu = 0;// mod.Mrsdanxiangmu > 0 ? mod.Mrsdanxiangmu : 0;
                                        }
                                        mod.IsQuanqing = "否";
                                        mod.QuanqingRewardf = "未开启";

                                        if (_isFullAttendMRS == 1)
                                        {
                                            mod.QuanqingRewardf = "0";
                                            if (mod.ChuqingDay + mod.XiuxiDay == dayCount1)
                                            {
                                                mod.IsQuanqing = "是";
                                                mod.QuanqingReward = Convert.ToInt32(FullAttendMRS);
                                                mod.QuanqingRewardf = FullAttendMRS.ToString();
                                            }
                                        }

                                        mod.MrsHospitalYeji = HospitalYeji;
                                        mod.MrsshicaoStr = string.IsNullOrEmpty(mod.MrsshicaoStr) ? "0" : yejitc.ToString();
                                        var jiaoxi1 = Convert.ToInt32(mod.Mrsdanxiangmu + mod.Mrstichengzhidingke + mod.MrsFangdang + Convert.ToInt32(Convert.ToDecimal(mod.MrsshicaoStr)));
                                        mod.JixiaoTicheng = jiaoxi1;
                                        mod.JiangliKoufang = mod.NewCusReward + Convert.ToInt32(mod.Mrsqqklxmj) + mod.Mrsdanxiangmu + mod.QuanqingReward;


                                        #endregion
                                        mod.IsDixinSalary = "否";
                                        decimal dixin = mod.MrsDixin;
                                        if (_isAchieQT == 1)
                                        {
                                            mod.IsDixinSalary = "是";
                                            dixin = mod.MrsDixin * SalaryDay / dayCount;
                                        }
                                        var zhineng = Convert.ToInt32(mod.Sumshicaoshougong) + Convert.ToInt32(yejitc) + Convert.ToInt32(mod.MrsSumkaikouyeji) + Convert.ToInt32(dixin);
                                        mod.Iszhinengkaoping = "否";
                                        mod.Xingzhen = 0;
                                        if (list1.Count() > 0)
                                        {
                                            var items1 = list1.Where(x => x.UserId == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                            if (items1.Count() > 0)
                                            {
                                                var items = items1.Where(x => x.UserId == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                                if (items.Count() > 0)
                                                    mod.Xingzhen = Convert.ToInt32(string.IsNullOrEmpty(items[0].Memo) ? "0" : items[0].Memo);
                                            }
                                        }
                                        if (_isBeauticianCheckQT == 1)
                                        {
                                            mod.Iszhinengkaoping = "是";
                                            if (list1.Count() > 0)
                                            {
                                                var items1 = list1.Where(x => x.UserId == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                                if (items1.Count() > 0)
                                                {
                                                    var items = items1.Where(x => x.UserId == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                                    if (items.Count() > 0)
                                                        mod.Fenshu = Convert.ToInt32(string.IsNullOrEmpty(items[0].NumDay) ? "80" : items[0].NumDay);
                                                    else
                                                        mod.Fenshu = 80;
                                                    if (mod.Fenshu < 80)
                                                        zhineng = Convert.ToInt32((1 - (decimal)(80 - mod.Fenshu) / 100) * zhineng);
                                                }
                                                else
                                                {
                                                    mod.Fenshu = 80;
                                                }
                                            }
                                            else
                                            {
                                                mod.Fenshu = 80;
                                            }
                                        }
                                        else
                                        {
                                            mod.Fenshu = 80;
                                        }
                                        mod.Mrsquangqingjiang = mod.Mrsquangqingjiang > 0 ? mod.Mrsquangqingjiang : 0;
                                        mod.Mrspingjukeliu = mod.Mrspingjukeliu > 0 ? mod.Mrspingjukeliu : 0;
                                        mod.Mrsxiangmu = mod.Mrsxiangmu > 0 ? mod.Mrsxiangmu : 0;
                                        mod.ShifaSalary = zhineng + mod.JiangliKoufang - mod.Xingzhen;
                                        if (jobid == 1)
                                        {
                                            newlist.Add(mod);
                                        }

                                    }
                                }
                                catch { }
                            }
                            if (jobid == 2 || jobid == 0)
                            {
                                mod.UserPost = info.DutyName;
                                if (mod.UserPost == "店长")
                                {
                                    #region 获取单条统计
                                    //考勤统计
                                    //考勤全勤客流项目奖

                                    mod.UserName = info.UserName;
                                    mod.UserName = info.UserName;

                                    #region 超休 6天
                                    mod.DzChaoxiuStr = ("否").ToString();
                                    if (jobid == 2 || jobid == 4 || jobid == 0)
                                    {
                                        if (isExceedRestshopowner == "1")
                                        { //如果超休

                                            mod.DzChaoxiuStr = ("是").ToString();
                                            if (mod.Chaoxiuday > 0)
                                            {
                                                if (mod.Chaoxiuday > 6)
                                                {
                                                    dzhospitalglyj = (HospitalYeji * Convert.ToDecimal((1 - ((mod.Chaoxiuday - 6) * (decimal)1.5 / days) - ((6) * (decimal)1 / days))));
                                                    dzhospitalglyj = HospitalYeji > 0 ? HospitalYeji : 0;
                                                    HospitalYejiJx = dzhospitalglyj;
                                                    //超休只扣现金业绩部分
                                                    dzhospitalallglyj = dzhospitalglyj + sum_feizhid.ShiCao + sum_feizhid.KakouYeji_Chanpin + sum_feizhid.KakouYeji_TeshuChanpin;
                                                    dzhospitalallglyj = dzhospitalallglyj / 2;
                                                    HospitalZhongheYeji = dzhospitalallglyj;
                                                }
                                                else
                                                {
                                                    dzhospitalglyj = (HospitalYeji * (1 - ((mod.Chaoxiuday) * (decimal)1 / days)));
                                                    dzhospitalglyj = HospitalYeji > 0 ? HospitalYeji : 0;
                                                    HospitalYejiJx = dzhospitalglyj;
                                                    //超休只扣现金业绩部分
                                                    dzhospitalallglyj = dzhospitalglyj + sum_feizhid.ShiCao + sum_feizhid.KakouYeji_Chanpin + sum_feizhid.KakouYeji_TeshuChanpin;
                                                    dzhospitalallglyj = dzhospitalallglyj / 2;
                                                    HospitalZhongheYeji = dzhospitalallglyj;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    decimal qqklxmj = 0;
                                    decimal quangqingjiang = 0;
                                    //全勤
                                    mod.ShouGongFei = result.Sum(t => t.Ticheng);
                                    decimal pingjukeliu = 0;
                                    decimal xiangmu = 0;
                                    var res = result.Where(x => x.UserID == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                    #endregion

                                    var _shopownerSal = ("," + shopownerSal).Split('}');
                                    var item = _shopownerSal;
                                    #region 底薪1
                                    var Dixin1 = 0;

                                    mod.HospitalXianjinYeji = (decimal)HospitalYeji;


                                    mod.HospitalXianjinYeji = (decimal)HospitalYeji;
                                    DixinOne(dianzhang, mod, item, (decimal)HospitalYeji);//底薪1
                                    #endregion
                                    #region 底薪2

                                    var Dixin2 = 0;
                                    var sumkeliu = mod1.YouXiaoKeLiu;
                                    mod.MrsDixin2Rentou = mod1.YouXiaoRenTou;
                                    DixinTwo(mod, item, mod.MrsDixin2Rentou);//底薪2

                                    #endregion


                                    #region 业绩提成
                                    if (HospitalZhongheYeji > 0) mod.HospitalRewardALL = HospitalZhongheYeji;
                                    var yejitc = GetYejiTi1(mod, HospitalYejiJx, mod.HospitalRewardALL, shopownerAche, shopownerDJCXJ);
                                    mod.XiangmuYeji = HospitalDaxiangmuYeji;
                                    var yejitcdaxiangmu = GetYejiTiDaxianmu(mod, HospitalYejiJx, HospitalDaxiangmuYeji, shopownerAche, shopownerDJCXJ);
                                    mod.MrsshicaoStr = (yejitc + yejitcdaxiangmu).ToString();

                                    var ShouGongFeiModel = result.Where(t => t.DutyName == mod.UserPost && t.UserName == mod.UserName && t.HospitalID == entity.HospitalID).ToList();
                                    if (ShouGongFeiModel.Count() > 0)
                                    {
                                        mod.ShouGongFei = ShouGongFeiModel[0].Ticheng;
                                        mod.Jiangli = ShouGongFeiModel[0].OtherTiCheng;
                                    }
                                    mod.DzYejiSum = Convert.ToInt32(((mod.ShouGongFei + mod.Jiangli + (decimal)mod.HospitalRewardALL * Convert.ToDecimal(mod.DzHospitalYejiFilter) + (decimal)mod.XiangmuYeji * Convert.ToDecimal(mod.DzDaxiangmuYejiFilter)) / 100)).ToString();
                                    mod.JixiaoTicheng = yejitc + yejitcdaxiangmu;
                                    #endregion
                                    #region 奖励&扣罚

                                    var BenyueRentou = mod1.YouXiaoRenTou;
                                    mod.DzBenyueRentou = BenyueRentou;
                                    var ShangyueRentou = listkekiushang.YouXiaoRenTou;
                                    mod.DzShangyueRentou = ShangyueRentou;
                                    GetBijiaoRentou(entity, BenyueRentou, ShangyueRentou, mod, _deductMoney);

                                    mod.BijiaoRentou = mod.BijiaoRentou != 0 ? mod.BijiaoRentou : 0;
                                    mod.BijiaoRentouKoukuang = mod.BijiaoRentouKoukuang != 0 ? mod.BijiaoRentouKoukuang : 0;


                                    mod.IsQuanqing = "否";
                                    mod.QuanqingRewardf = "未开启";

                                    if (_isFullAttendDZ == 1)
                                    {
                                        mod.QuanqingRewardf = "0";
                                        if (mod.ChuqingDay + mod.XiuxiDay == dayCount1)
                                        {
                                            mod.IsQuanqing = "是";
                                            mod.QuanqingReward = Convert.ToInt32(FullAttendDZ);
                                            mod.QuanqingRewardf = FullAttendDZ.ToString();
                                        }
                                    }

                                    if (_isFullAttendDZ == 1)
                                    {
                                        quangqingjiang = (decimal)50;
                                        mod.Mrsquangqingjiang = quangqingjiang;
                                    }
                                    mod.Mrsquangqingjiang = mod.Mrsquangqingjiang > 0 ? mod.Mrsquangqingjiang : 0;
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                            mod.DzJiangjin = items1[0].EndYear;
                                    }
                                    mod.DzKoufangCoutz = mod.QuanqingReward + mod.BijiaoRentouKoukuang + Convert.ToDecimal(mod.DzJiangjin);
                                    mod.DzKoufangCout = (mod.QuanqingReward + mod.BijiaoRentouKoukuang + Convert.ToDecimal(mod.DzJiangjin)).ToString();

                                    #endregion
                                    #region 店长责任指标

                                    var newkeliu = listkekiu.Sum(x => x.CustomerService); //新客流
                                    int GZZZ = GetGZZhize1(mod, HospitalYeji, sumkeliu, newkeliu, asverBeauDuty);
                                    var chengshuike = 0;
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                            chengshuike = items1[0].EndMonths;
                                    }
                                    mod.XianJinYeJi = HospitalYeji.ToString();
                                    mod.JiangliDuty = GZZZ + chengshuike;
                                    mod.ChengSuiKeJiangfang = chengshuike.ToString();
                                    mod.sumkeliu = sumkeliu;
                                    mod.newkeliu = newkeliu;
                                    mod.JixiaoReward = Convert.ToInt32(mod.BijiaoRentouKoukuang) + mod.JiangliDuty;//业绩资金; 
                                    #endregion

                                    mod.MrsDixin2keliu = (decimal)mod.MrsDixin2Rentou;
                                    mod.MrsDixin1 = mod.MrsDixin1;
                                    mod.MrsDixin2 = mod.MrsDixin2;//总底薪
                                    mod.DixinSalary = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪                                   

                                    var zhineng = mod.JixiaoTicheng + mod.DixinSalary;
                                    mod.IsDixinSalary = "否";
                                    if (_isAchieQT == 1)
                                    {
                                        mod.IsDixinSalary = "是";
                                        zhineng = zhineng * SalaryDay / dayCount;
                                    }
                                    mod.Iszhinengkaoping = "否";
                                    mod.Xingzhen = 0;
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                        {
                                            var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items.Count() > 0)
                                                mod.Xingzhen = Convert.ToInt32(string.IsNullOrEmpty(items[0].Memo) ? "0" : items[0].Memo);
                                        }
                                    }
                                    if (_isBeauticianCheckQT == 1)
                                    {
                                        mod.Iszhinengkaoping = "是";
                                        if (list1.Count() > 0)
                                        {
                                            var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items1.Count() > 0)
                                            {
                                                var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                                if (items.Count() > 0)
                                                {
                                                    mod.Fenshu = Convert.ToInt32(string.IsNullOrEmpty(items[0].NumDay) ? "80" : items[0].NumDay);
                                                }
                                                else
                                                {
                                                    mod.Fenshu = 80;
                                                }
                                                if (mod.Fenshu < 80)
                                                    zhineng = Convert.ToInt32((1 - (decimal)(80 - mod.Fenshu) / 100) * zhineng);
                                            }
                                            else
                                            {
                                                mod.Fenshu = 80;
                                            }
                                        }
                                        else
                                        {
                                            mod.Fenshu = 80;
                                        }
                                    }
                                    else
                                    {
                                        mod.Fenshu = 80;
                                    }
                                    mod.ShifaSalary = zhineng + Convert.ToInt32(mod.JiangliDuty) + Convert.ToInt32(mod.DzKoufangCoutz) - Convert.ToInt32(mod.Xingzhen);
                                    if (jobid == 2)
                                    {
                                        newlist.Add(mod);
                                    }
                                }
                            }
                            if (jobid == 3 || jobid == 0)
                            {
                                #region 获取单条统计
                                //考勤统计
                                mod.UserName = info.UserName;
                                mod.UserPost = info.DutyName;

                                #endregion
                                if (mod.UserPost == "顾问")
                                {

                                    //考勤全勤客流项目奖
                                    decimal quangqingjiang = 0;
                                    //全勤
                                    var keliu = listkekiu.Where(x => x.UserID == info.UserID).ToList();
                                    var getxiangmu = xiangmulist.Where(x => x.UserID == info.UserID).ToList();
                                    var res = result.Where(x => x.UserID == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                    mod.HospitalRewardALL = 0;// = sumzh / 2;
                                    decimal sumzhs = 0;
                                    decimal sumzhsGw = 0;
                                    decimal sumzhsall = 0;
                                    decimal XiangmuYeji = 0;


                                    var _adviserSal = ("," + adviserSal).Split('}');
                                    var item = _adviserSal;
                                    #region 组别
                                    if (keliu.Count() > 0)
                                    {
                                        DePartMentID = keliu[0].DepartmentID;
                                    }
                                    var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.HospitalID, 1).Where(c => c.ID == DePartMentID).ToList();
                                    mod.DePartMentStr = departList != null && departList.Count() > 0 ? departList[0].Name : "";
                                    #endregion
                                    #region 底薪1
                                    if (res.Count() > 0)
                                    {
                                        var infoo = res[0];
                                        DePartMentID = infoo.DepartmentID;
                                        var listDepartMent = result.Where(x => x.DepartmentID == DePartMentID);
                                        mod.ShouGongFei = listDepartMent.Sum(t => t.Ticheng);
                                        foreach (var infooo in listDepartMent)
                                        {
                                            sumzhs += infooo.Yeji_Chuzhika + infooo.Yeji_Liaochengka + infooo.Yeji_Chanpin + infooo.Yeji_TeshuChanpin + infooo.Yeji_Xiangmu + infooo.Yeji_Teshuxiangmu;
                                            sumzhsGw += (infooo.ShiCao + infooo.KakouYeji_Chanpin + infooo.KakouYeji_TeshuChanpin);
                                            sumzhsall += (infooo.Yeji_Chuzhika + infooo.Yeji_Liaochengka + infooo.Yeji_Chanpin + infooo.Yeji_TeshuChanpin + infooo.Yeji_Xiangmu + infooo.Yeji_Teshuxiangmu + infooo.ShiCao + infooo.KakouYeji_Chanpin + infooo.KakouYeji_TeshuChanpin);
                                            XiangmuYeji += infooo.Yeji_Daxiangmu;
                                        }

                                        mod.HospitalRewardALL = Math.Round(sumzhsall / 2, 2);


                                        DixinOne(res[0], mod, item, sumzhs);//底薪1

                                    }
                                    #endregion
                                    #region 底薪2

                                    var Dixin2 = 0;
                                    var kel = keliu;

                                    var rentou = 0;
                                    decimal sumkeliu = 0;
                                    decimal istkekiuCustomerService = 0;


                                    if (kel.Count() > 0)
                                    {
                                        var kk = listkekiu.Where(x => x.DepartmentID == DePartMentID).Sum(x => x.HeadCount);
                                        sumkeliu = (decimal)kk;
                                        DixinTwo(mod, item, (decimal)sumkeliu);//底薪2

                                        mod.MrsDixin2keliu = (decimal)sumkeliu;
                                        mod.MrsDixin1 = mod.MrsDixin1;
                                        mod.MrsDixin2 = mod.MrsDixin2;
                                        //mod.DixinSalary = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪
                                        //mod.DixinSalaryf = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪


                                        var infoo = kel[0];
                                        DePartMentID = infoo.DepartmentID;
                                        seachPatient.DepartmentID = DePartMentID;
                                        decimal Keliu1 = 0, fuwuPassengerTraffic1 = 0, kuadianKeliu1 = 0;
                                        var keliug = listkekiuGuwenKwliu.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().Keliu;
                                        var fuwuPassengerTrafficg = listkekiuGuwenPass.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().fuwuPassengerTraffic;
                                        var JianquKeliug = listkekiuGuwenJiangqu.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().JianquKeliu;
                                        kuadianKeliu1 = (fuwuPassengerTrafficg - JianquKeliug) - (fuwuPassengerTrafficg - JianquKeliug);
                                        Keliu1 = keliug;
                                        fuwuPassengerTraffic1 = (fuwuPassengerTrafficg - JianquKeliug);
                                        sumkeliu = Keliu1 + fuwuPassengerTraffic1 + kuadianKeliu1;
                                        istkekiuCustomerService = listkekiu.Where(x => x.DepartmentID == DePartMentID).Sum(x => x.CustomerService);// listkekiuGuwenRentouCustomerService.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().CustomerService;


                                    }

                                    mod.MrsDixin1 = mod.MrsDixin1 > 0 ? mod.MrsDixin1 : Convert.ToInt32(item[4].Split(',')[2].Split(':')[1]);
                                    mod.MrsDixin2 = mod.MrsDixin2 > 0 ? mod.MrsDixin2 : Convert.ToInt32(item[4].Split(',')[7].Split(':')[1]);
                                    mod.DixinSalary = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪
                                    mod.DixinSalaryf = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪
                                    mod.BijiaoRentou = mod.BijiaoRentou != 0 ? mod.BijiaoRentou : 0;
                                    mod.BijiaoRentouKoukuang = mod.BijiaoRentouKoukuang != 0 ? mod.BijiaoRentouKoukuang : 0;
                                    #endregion
                                    seachPatient.DepartmentID = DePartMentID; //小组
                                    var d_sumkeliu = Convert.ToInt32(listkekiu.Where(x => x.DepartmentID == DePartMentID).Sum(x => x.PassengerTraffic));
                                    var d_newkeliu = Convert.ToInt32(istkekiuCustomerService); //店新客流量

                                    #region 超休 6天
                                    mod.GwChaoxiuStr = ("否").ToString();
                                    if (jobid == 3 || jobid == 4 || jobid == 0)
                                    {
                                        if (isExceedRestAdver == "1")
                                        { //如果超休

                                            mod.GwChaoxiuStr = ("是").ToString();
                                            if (mod.Chaoxiuday > 0)
                                            {
                                                if (mod.Chaoxiuday > 6)
                                                {
                                                    dzhospitalglyj = (sumzhs * Convert.ToDecimal((1 - ((mod.Chaoxiuday - 6) * (decimal)1.5 / days) - ((6) * (decimal)1 / days))));
                                                    dzhospitalglyj = dzhospitalglyj > 0 ? dzhospitalglyj : 0;
                                                    //超休只扣现金业绩部分
                                                    dzhospitalallglyj = dzhospitalglyj + sumzhsGw;
                                                    dzhospitalallglyj = dzhospitalallglyj / 2;
                                                }
                                                else
                                                {
                                                    dzhospitalglyj = (sumzhs * (1 - ((mod.Chaoxiuday) * (decimal)1 / days)));
                                                    dzhospitalglyj = dzhospitalglyj > 0 ? dzhospitalglyj : 0;
                                                    dzhospitalallglyj = dzhospitalglyj + sumzhsGw;
                                                    dzhospitalallglyj = dzhospitalallglyj / 2;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region 业绩提成
                                    seachentity.DepartmentID = DePartMentID;
                                    if (dzhospitalallglyj > 0) mod.HospitalRewardALL = Math.Round(dzhospitalallglyj, 2); //Convert.ToDecimal(dzhospitalallglyj);
                                    var yejitc = GetYejiTi1(mod, dzhospitalglyj, dzhospitalallglyj, advererAche, shopownerDJCXJ);

                                    mod.XiangmuYeji = XiangmuYeji;
                                    var yejitcdaxiangmu = GetYejiTiDaxianmu1(mod, dzhospitalglyj, XiangmuYeji, advererAche, shopownerDJCXJ);
                                    mod.MrsshicaoStr = (yejitc + yejitcdaxiangmu).ToString();
                                    var ShouGongFeiModel = result.Where(t => t.DutyName == mod.UserPost && t.UserName == mod.UserName && t.HospitalID == entity.HospitalID).ToList();
                                    if (ShouGongFeiModel.Count() > 0)
                                    {
                                        mod.ShouGongFei = ShouGongFeiModel[0].Ticheng;
                                        mod.Jiangli = ShouGongFeiModel[0].OtherTiCheng;
                                    }
                                    mod.GwYejiSum = Convert.ToInt32((mod.ShouGongFei + mod.Jiangli + ((decimal)mod.HospitalRewardALL * Convert.ToDecimal(mod.DzHospitalYejiFilter) + (decimal)mod.XiangmuYeji * Convert.ToDecimal(mod.DzDaxiangmuYejiFilter)) / 100)).ToString();

                                    #endregion
                                    #region 奖励&扣罚
                                    if (kel.Count() > 0)
                                    {
                                        var benyuerentou = listkekiu.Where(x => x.DepartmentID == DePartMentID).Sum(x => x.HeadCount);
                                        mod.DzBenyueRentou = benyuerentou;
                                    }
                                    var kelgw = listkekiushanggw;
                                    if (kelgw.Count() > 0)
                                    {
                                        var shangyuerentou = listkekiushanggw.Where(x => x.DepartmentID == DePartMentID).Sum(x => x.HeadCount);
                                        mod.DzShangyueRentou = shangyuerentou;
                                    }
                                    GetBijiaoRentou(entity, mod.DzBenyueRentou, mod.DzShangyueRentou, mod, _deductMoney);

                                    mod.BijiaoRentou = mod.BijiaoRentou != 0 ? mod.BijiaoRentou : 0;
                                    mod.BijiaoRentouKoukuang = mod.BijiaoRentouKoukuang != 0 ? mod.BijiaoRentouKoukuang : 0;


                                    mod.IsQuanqing = "否";
                                    mod.QuanqingRewardf = "未开启";

                                    if (_isFullAttendGW == 1)
                                    {
                                        mod.QuanqingRewardf = "0";
                                        if (mod.ChuqingDay + mod.XiuxiDay == dayCount1)
                                        {
                                            mod.IsQuanqing = "是";
                                            mod.QuanqingReward = Convert.ToInt32(FullAttendGW);
                                            mod.QuanqingRewardf = FullAttendGW.ToString();
                                        }
                                    }


                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                            mod.DzJiangjin = items1[0].EndYear;
                                    }
                                    mod.DzKoufangCoutz = mod.QuanqingReward + mod.BijiaoRentouKoukuang + Convert.ToDecimal(mod.DzJiangjin);
                                    mod.DzKoufangCout = (mod.QuanqingReward + mod.BijiaoRentouKoukuang + Convert.ToDecimal(mod.DzJiangjin)).ToString();

                                    #endregion
                                    #region 业绩奖金
                                    int GZZZ = GetGZZhize1(mod, sumzhs, d_sumkeliu, d_newkeliu, asverBeauDuty1);
                                    var chengshuike = 0;
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                            chengshuike = items1[0].EndMonths;
                                    }
                                    mod.XianJinYeJi = sumzhs.ToString();
                                    mod.JiangliDuty = GZZZ + chengshuike;
                                    mod.ChengSuiKeJiangfang = chengshuike.ToString();
                                    mod.sumkeliu = d_sumkeliu;
                                    mod.newkeliu = d_newkeliu;
                                    mod.JixiaoTicheng = yejitc + yejitcdaxiangmu;//绩效提成

                                    var zhineng = Convert.ToDecimal(mod.GwYejiSum) + Convert.ToDecimal(mod.DixinSalary); // Convert.ToDecimal(mod.MrsshicaoStr) + Convert.ToDecimal(mod.DixinSalary);
                                    mod.IsDixinSalary = "否";
                                    if (_isAchieQT == 1)
                                    {
                                        mod.IsDixinSalary = "是";

                                        zhineng = zhineng * SalaryDay / dayCount;
                                    }
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                        {
                                            var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items.Count() > 0)
                                                mod.Xingzhen = Convert.ToInt32(string.IsNullOrEmpty(items[0].Memo) ? "0" : items[0].Memo);
                                        }
                                    }

                                    mod.Iszhinengkaoping = "否";
                                    if (_isBeauticianCheckQT == 1)
                                    {
                                        mod.Iszhinengkaoping = "是";
                                        if (list1.Count() > 0)
                                        {
                                            var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items1.Count() > 0)
                                            {
                                                var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                                if (items.Count() > 0)
                                                    mod.Fenshu = Convert.ToInt32(string.IsNullOrEmpty(items[0].NumDay) ? "80" : items[0].NumDay);
                                                else
                                                    mod.Fenshu = 80;
                                                if (mod.Fenshu < 80)
                                                    zhineng = Convert.ToInt32((1 - (decimal)(80 - mod.Fenshu) / 100) * zhineng);
                                            }
                                            else
                                            {
                                                mod.Fenshu = 80;
                                            }
                                        }
                                        else
                                        {
                                            mod.Fenshu = 80;
                                        }
                                    }
                                    else
                                    {
                                        mod.Fenshu = 80;
                                    }

                                    mod.ShifaSalary = Convert.ToInt32(zhineng) + Convert.ToInt32(mod.DzKoufangCoutz) + Convert.ToInt32(mod.JiangliDuty) - Convert.ToInt32(mod.Xingzhen);
                                    #endregion
                                    if (jobid == 3)
                                    {
                                        newlist.Add(mod);
                                    }

                                }
                            }
                            if (jobid == 4 || jobid == 0)
                            {
                                #region 获取单条统计
                                //考勤统计
                                mod.UserName = info.UserName;
                                mod.UserPost = info.DutyName;

                                //考勤全勤客流项目奖
                                decimal qqklxmj = 0;
                                decimal quangqingjiang = 0;
                                //全勤
                                var getxiangmu = xiangmulist.Where(x => x.UserID == info.UserID).ToList();
                                var res = result.Where(x => x.UserID == info.UserID && x.HospitalID == entity.HospitalID).ToList();
                                var regzhiding = resultzhiding.Where(x => x.UserID == info.UserID).ToList();
                                mod.HospitalRewardALL = HospitalZhongheYeji;// sumzh / 2;
                                #endregion
                                if (mod.UserPost == "前台")
                                {
                                    #region 底薪2
                                    var _receptionSal = ("," + receptionSal).Split('}');
                                    var item = _receptionSal;
                                    var sumkeliu = mod1.YouXiaoHuLiKeLiu;
                                    int Isqiantai = 1;//1是 0非
                                    DixinTwo(mod, item, (decimal)sumkeliu, Isqiantai);
                                    var qresult = result.Where(t => t.DutyName == "前台").ToList();
                                    if (qresult.Count() > 0)
                                        mod.ShouGongFei = qresult[0].Ticheng;
                                    mod.MrsDixin2keliu = (decimal)sumkeliu;
                                    mod.DixinSalary = (int)mod.MrsDixin2;

                                    #endregion
                                    #region 业绩提成
                                    var yejitc = GetYejiTi2(mod, HospitalYeji, HospitalDaxiangmuYeji, receptionAche, receptionDJCXJ);
                                    mod.HospitalXianjinYeji = (decimal)HospitalYeji;
                                    mod.Yeji_daxiangmuzhiding = (decimal)HospitalDaxiangmuYeji;

                                    var ShouGongFeiModel = result.Where(t => t.DutyName == mod.UserPost && t.UserName == mod.UserName && t.HospitalID == entity.HospitalID).ToList();
                                    if (ShouGongFeiModel.Count() > 0)
                                    {
                                        mod.ShouGongFei = ShouGongFeiModel[0].Ticheng;
                                        mod.Jiangli = ShouGongFeiModel[0].OtherTiCheng;
                                    }
                                    mod.MrsshicaoStr = (yejitc + mod.ShouGongFei + mod.Jiangli).ToString();//绩效提成
                                    mod.JixiaoTicheng = yejitc;//业绩资金

                                    mod.IsQuanqing = "否";
                                    mod.QuanqingRewardf = "未开启";

                                    if (_isFullAttendQT == 1)
                                    {
                                        mod.QuanqingRewardf = "0";
                                        if (mod.ChuqingDay + mod.XiuxiDay == dayCount1)
                                        {
                                            mod.IsQuanqing = "是";
                                            mod.QuanqingReward = Convert.ToInt32(FullAttendQT);
                                            mod.QuanqingRewardf = FullAttendQT.ToString();
                                        }
                                    }

                                    if (_isFullAttendQT == 1)
                                    {
                                        quangqingjiang = (decimal)50;
                                        mod.Mrsquangqingjiang = quangqingjiang;
                                    }
                                    if (_isFullAttendQT == 1)
                                    {
                                        quangqingjiang = (decimal)50;
                                        mod.Mrsquangqingjiang = quangqingjiang;
                                    }

                                    var zhineng = Convert.ToInt32(Convert.ToDecimal(mod.MrsshicaoStr)) + Convert.ToInt32(mod.DixinSalary);
                                    mod.IsDixinSalary = "否";
                                    if (_isAchieQT == 1)
                                    {
                                        mod.IsDixinSalary = "是";
                                        zhineng = zhineng * SalaryDay / dayCount;
                                    }
                                    mod.Iszhinengkaoping = "否";
                                    mod.Xingzhen = 0;
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                        {
                                            var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items.Count() > 0)
                                                mod.Xingzhen = Convert.ToInt32(string.IsNullOrEmpty(items[0].Memo) ? "0" : items[0].Memo);
                                        }
                                    }
                                    if (_isBeauticianCheckQT == 1)
                                    {
                                        mod.Iszhinengkaoping = "是";
                                        if (list1.Count() > 0)
                                        {
                                            var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items1.Count() > 0)
                                            {
                                                var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                                if (items.Count() > 0)
                                                {
                                                    mod.Fenshu = Convert.ToInt32(string.IsNullOrEmpty(items[0].NumDay) ? "80" : items[0].NumDay);
                                                }
                                                else
                                                {
                                                    mod.Fenshu = 80;
                                                }
                                                if (mod.Fenshu < 80)
                                                    zhineng = Convert.ToInt32((1 - (decimal)(80 - (int)mod.Fenshu) / 100) * Convert.ToInt32(zhineng));
                                            }
                                            else
                                            {
                                                mod.Fenshu = 80;
                                            }
                                        }
                                        else
                                        {
                                            mod.Fenshu = 80;
                                        }
                                    }
                                    else
                                    {
                                        mod.Fenshu = 80;
                                    }

                                    mod.ShifaSalary = Convert.ToInt32(zhineng) + mod.QuanqingReward - mod.Xingzhen;
                                    #endregion

                                    if (jobid == 4)
                                    {
                                        newlist.Add(mod);
                                    }
                                }
                            }
                            if (jobid == 0)
                            {

                                mod.GonglingSubsidy = 0;
                                mod.JiabanSalary = 0;
                                mod.Shibao = 0;
                                mod.Gongjijin = 0;
                                mod.Other = 0;
                                mod.YingfaSalary = mod.DixinSalary + mod.JixiaoReward + mod.JixiaoTicheng
                                                    + mod.QuanqingReward + mod.GonglingSubsidy
                                                    + mod.JiabanSalary
                                                    + Convert.ToInt32(mod.ShouGongFei)
                                                    + mod.Gongjijin - mod.Shibao;
                                mod.ShifaSalary = mod.YingfaSalary - mod.Other - mod.KoukuanDuty;
                                newlist.Add(mod);
                            }

                        }
                    }
                }
                //}
                //catch { }

                return newlist;
            }
        }

        private List<DakaRecordEntity> GetSalarySatisticsF2(DakaRecordEntity dakaRecordEntity, int jobid = 0)
        {
            #region
            #region 变量
            var entity = dakaRecordEntity;
            var rr = entity.PapeID;
            int numberpage = 1;
            if (rr == "第一页")
            {
                numberpage = 1;
            }
            if (rr == "第二页")
            {
                numberpage = 2;
            }
            if (rr == "第三页")
            {
                numberpage = 3;
            }
            if (rr == "第四页")
            {
                numberpage = 4;
            }
            if (rr == "第五页")
            {
                numberpage = 5;
            }
            if (rr == "第六页")
            {
                numberpage = 6;
            }
            if (rr == "第七页")
            {
                numberpage = 7;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.HospitalID == 0)
                entity.HospitalID = LoginUserEntity.HospitalID;
            if (entity.Years == 0)
                entity.Years = DateTime.Now.Year;
            if (entity.Months == 0)
                entity.Months = DateTime.Now.Month;
            //考勤
            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Years = entity.Years;
            ViewBag.Months = entity.Months;

            ViewBag.days = days;
            List<DakaRecordEntity> newlist = new List<DakaRecordEntity>();
            if (dakaRecordEntity.HospitalID == 0)
            {
                return null;
            }
            else
            {
                //业绩
                int rows;
                int pagecount;
                var seachentity = new _SeachEntity();
                if (seachentity.s_HospitalID == 0) seachentity.s_HospitalID = LoginUserEntity.HospitalID;
                seachentity.s_StareTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
                seachentity.s_Endtime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());
                seachentity.numPerPage = 50; //每页显示条数
                if (seachentity.orderField + "" == "") { seachentity.orderField = "CreateTime"; }
                IList<JoinUserEntity> result = null;
                IList<JoinUserEntity> resultzhidingmrs = null;
                IList<JoinUserEntity> resultzhidingduadian = null;
                IList<JoinUserEntity> resultzhifeidingduadian = null;
                IList<JoinUserEntity> resultfeizhidingmrs = null;
                IList<JoinUserEntity> resultfeizhidingduadian = null;
                IList<JoinUserEntity> resultfeizhifeidingduadian = null;
                JoinUserEntity dianzhang = null;
                JoinUserEntity sum_zhid = null;
                JoinUserEntity sum_feizhid = null;
                var entityFangd = new _SeachEntity();
                if (entityFangd.s_HospitalID == 0)
                {
                    entityFangd.s_HospitalID = LoginUserEntity.HospitalID;
                }
                ViewBag.HospitalID = entityFangd.s_HospitalID;
                DateTime dtFirst;
                DateTime dtLast;


                var dt = ConvertValueHelper.ConvertDateTimeValueNew(entity.Years + "-" + entity.Months + "-" + "01");

                //本月第一天时间
                dtFirst = ConvertValueHelper.ConvertDateTimeValueNew(dt.AddDays(1 - (dt.Day)).ToString("yyyy-MM-dd 00:00:00"));
                //获得某年某月的天数
                var dayCount = DateTime.DaysInMonth(dt.Year, dt.Month);
                //本月最后一天时间
                dtLast = ConvertValueHelper.ConvertDateTimeValueNew(dtFirst.AddDays(dayCount - 1).ToString("yyyy-MM-dd 23:59:59"));

                //客流
                var seachPatient = new PassengerTrafficEntity();
                seachPatient.HospitalID = entity.HospitalID;
                seachPatient.searchStartTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
                seachPatient.searchEndTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());

                var daysgw = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months) - 1);
                var shangyueendtime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months - 1) + "-" + daysgw + " 23:59:59").ToString());
                if (string.IsNullOrWhiteSpace(entity.orderField)) seachPatient.orderField = "UserID";
                var listkekiuGuwenRentou = new List<PassengerTrafficEntity>();
                var listkekiuGuwenKwliu = new List<PassengerTrafficEntity>();
                var listkekiuGuwenPass = new List<PassengerTrafficEntity>();
                var listkekiuGuwenJiangqu = new List<PassengerTrafficEntity>();
                var listkekiuGuwenRentoushangyue = new List<PassengerTrafficEntity>();
                var listkekiuGuwenRentouCustomerService = new List<PassengerTrafficEntity>();
                var entityxm = new ItemDetailsConsumptionEntity();
                decimal huaci;
                decimal kakou;
                decimal shougong;
                decimal jiangli;
                decimal xianjin;
                decimal Quantity;
                int UserCount;
                entityxm.HospitalID = entityxm.HospitalID == 0 ? LoginUserEntity.HospitalID : entityxm.HospitalID;
                entityxm.numPerPage = 50; //每页显示条数
                entityxm.StartTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
                entityxm.EndTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());
                if (entityxm.orderField + "" == "") { entityxm.orderField = "CreateTime"; entityxm.orderDirection = "desc"; }
                var entitys = new SystemUserSettingsEntity();
                entitys.HospitalID = LoginUserEntity.HospitalID;


                StoreDataSummaryEntity entityss = new StoreDataSummaryEntity();
                var o = DateTime.MinValue;
                entityss.StartTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
                entityss.EndTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());
                if (entityss.HospitalID == 0) entityss.HospitalID = LoginUserEntity.HospitalID;
                var mod1 = cashBll.GetStoreDataSummary(entityss);
                var listkekiushang = new StoreDataSummaryEntity();
                if (jobid == 2)
                {

                    StoreDataSummaryEntity seachPatients = new StoreDataSummaryEntity();
                    if (seachPatients.HospitalID == 0) seachPatients.HospitalID = LoginUserEntity.HospitalID;
                    seachPatients.StartTime = entityss.StartTime.AddMonths(-1);
                    seachPatients.EndTime = shangyueendtime;// entityss.EndTime.AddMonths(-1);
                    listkekiushang = cashBll.GetStoreDataSummary(seachPatients);
                }
                var seachPatientshangyue = new PassengerTrafficEntity();
                seachPatientshangyue.HospitalID = entity.HospitalID;
                seachPatientshangyue.searchStartTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
                seachPatientshangyue.searchEndTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());

                if (string.IsNullOrWhiteSpace(entity.orderField)) seachPatientshangyue.orderField = "UserID";
                seachPatientshangyue.searchStartTime = seachPatientshangyue.searchStartTime.AddMonths(-1);
                seachPatientshangyue.searchEndTime = shangyueendtime;// seachPatientshangyue.searchEndTime.AddMonths(-1);
                decimal Keliu = 0, fuwuPassengerTraffic = 0, kuadianKeliu = 0, kuadianKeliuC = 0;
                #endregion

                var entityl = new LeaveRecordsEntity();
                entityl.HospitalID = LoginUserEntity.HospitalID;
                entityl.numPerPage = 50; //每页显示条数
                if (string.IsNullOrEmpty(entity.StartTime))
                {

                    entityl.StartTime = ("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString();
                    entityl.EndTime = ("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString();
                }

                var list1 = reserBLL.GetLeaveRecordsList1(entityl);

                list1 = list1.Where(t => t.category == "d91b4hykqlfm" && t.StartYear == entity.Years && t.StartMoths == entity.Months).OrderByDescending(x => x.Dateleave).ToList();
                //if (list1.Count() > 0) {
                //    list1 = list1.fi;
                //}



                var list = personBLL.GetAllUser(new HospitalUserEntity { IsActive = 1, HospitalID = entity.HospitalID });

                list = list.Where(i => i.IsSys != 1 && i.IsAPP != 1).ToList();
                if (jobid == 1)
                {
                    list = list.Where(x => x.DutyName == "美容师").ToList();
                    ViewBag.MrsCount = list.Count();
                    list = list.AsQueryable().ToPagedList(numberpage, 12).ToList();
                }
                if (jobid == 2)
                {
                    list = list.Where(x => x.DutyName == "店长").ToList();
                }
                if (jobid == 3)
                {
                    list = list.Where(x => x.DutyName == "顾问").ToList();
                }
                if (jobid == 4)
                {
                    list = list.Where(x => x.DutyName == "前台").ToList();
                }
                //list = list.OrderByDescending(x => x.DutyName).ToList();

                result = cashBll.GetPerformanceSummary(seachentity, out rows, out pagecount);//默认全部
                seachentity.IsZhiding = 1;//指定
                var resultzhiding = cashBll.GetPerformanceSummary(seachentity, out rows, out pagecount);//指定
                if (jobid == 1 || jobid == 0)
                {
                    seachentity.IsZhiding = 0;//非指定
                    resultfeizhidingmrs = cashBll.GetPerformanceSummary(seachentity, out rows, out pagecount);//非指定
                    seachentity.IsZhiding = 1;//指定
                    resultzhidingmrs = cashBll.GetPerformanceSummary(seachentity, out rows, out pagecount);//指定
                }
                seachentity.IsZhiding = -1;//全部
                sum_feizhid = cashBll.GetSumPerformanceSummary(seachentity);//求汇总 默认全部
                var HospitalYeji = sum_feizhid.Yeji_Chuzhika + sum_feizhid.Yeji_Liaochengka + sum_feizhid.Yeji_Chanpin + sum_feizhid.Yeji_TeshuChanpin + sum_feizhid.Yeji_Xiangmu + sum_feizhid.Yeji_Teshuxiangmu;
                var HospitalYejiJx = HospitalYeji;
                var dzhospitalyeji = HospitalYeji;
                decimal dzhospitalglyj = 0;
                decimal dzhospitalallglyj = 0;
                decimal HospitalZhongheYeji = 0;
                decimal HospitalDaxiangmuYeji = 0;
                if (jobid == 2 || jobid == 4)
                {
                    //综合业绩=【基础项目现金业绩+实操业绩+卡扣家居】÷2
                    HospitalZhongheYeji = (HospitalYeji + sum_feizhid.ShiCao + sum_feizhid.KakouYeji_Chanpin + sum_feizhid.KakouYeji_TeshuChanpin) / 2;
                    HospitalDaxiangmuYeji = sum_feizhid.Yeji_Daxiangmu;
                }
                var listPers = personBLL.GetAllUser(new HospitalUserEntity { HospitalID = entityFangd.s_HospitalID, IsActive = 1 })
                    .Where(c => c.IsSeparation == 0 && c.IsSys != 1).ToList();
                if (jobid == 3 || jobid == 0)
                {
                    listkekiuGuwenRentou = iCentBll.PageListPassengerTrafficGuWenRentou(seachPatient);
                    listkekiuGuwenKwliu = iCentBll.PageListPassengerTrafficGuWenkeliu(seachPatient);
                    listkekiuGuwenPass = iCentBll.PageListPassengerTrafficGuWenfuwuPassengerTraffic(seachPatient);
                    listkekiuGuwenJiangqu = iCentBll.PageListPassengerTrafficGuWenJianquKeliu(seachPatient);
                    listkekiuGuwenRentouCustomerService = iCentBll.PageListPassengerTrafficGuWenCustomerService(seachPatient);
                    listkekiuGuwenRentoushangyue = iCentBll.PageListPassengerTrafficGuWenRentou(seachPatientshangyue);

                }
                var listkekiu = iCentBll.PageListPassengerTraffic(seachPatient);
                var listkekiushanggw = new List<PassengerTrafficEntity>();
                if (jobid == 3)
                {

                    PassengerTrafficEntity seachPatientsgw = new PassengerTrafficEntity();
                    if (seachPatientsgw.HospitalID == 0) seachPatientsgw.HospitalID = LoginUserEntity.HospitalID;
                    seachPatientsgw.searchStartTime = seachPatient.searchStartTime.AddMonths(-1);
                    seachPatientsgw.searchEndTime = shangyueendtime;// seachPatient.searchEndTime.AddMonths(-1);
                    listkekiushanggw = iCentBll.PageListPassengerTraffic(seachPatientsgw);
                }
                if (jobid == 2 || jobid == 4 || jobid == 0)
                {

                    seachPatient.IsKuaDian = 1;
                    var fuwuPassengerTrafficsum = 0;
                    if (jobid != 0)
                        fuwuPassengerTrafficsum = iCentBll.PageGetfuwuPassengerTraffic(seachPatient).Where(x => x.DepartmentID == 1).FirstOrDefault().fuwuPassengerTraffic;//慢
                    foreach (var en in listkekiu)
                    {

                        kuadianKeliuC += en.fuwuPassengerTraffic;
                        Keliu += en.Keliu;
                        fuwuPassengerTraffic += (en.fuwuPassengerTraffic - en.JianquKeliu);
                    }
                    if (jobid != 0)
                        kuadianKeliu = fuwuPassengerTrafficsum - kuadianKeliuC;
                }
                var xiangmulist = cashBll.GetItemDetailsConsumptionList(entityxm, out huaci, out kakou, out shougong, out jiangli, out xianjin, out Quantity, out UserCount);
                //考勤参数
                var sysSalary = IsystemBLL.GetSystemUserSettingsModelByPay(entitys);
                #endregion
                //逻辑
                //try
                //{
                if (!string.IsNullOrEmpty(sysSalary.AttributeValue))
                {
                    var syssalary = sysSalary.AttributeValue.Replace("|||", "★").Replace("\"", "").Replace("[", "").Replace("]", "");
                    var arrsys = syssalary.Split('★');
                    if (arrsys.Count() > 0)
                    {
                        #region 参数赋值
                        Dictionary<string, string> arr = new Dictionary<string, string>();
                        for (int i = 0; i < arrsys.Length; i++)
                        {
                            arr.Add(arrsys[i], arrsys[i + 1]);
                            i++;
                        }
                        var asverBeauDuty1 = "";
                        var isExceedRestAdver = arr["isExceedRestAdver"];
                        var isExceedRestshopowner = arr["isExceedRestshopowner"];
                        var specialProduct = arr["specialProduct"];
                        var specialProject = arr["specialProject"];
                        var BeauticianSal = arr["BeauticianSal"];
                        var msr_xktc = arr["newGuests"];
                        var mrs_scyj_arr = arr["storeRevenue"];
                        var mrs_djcxj_arr = arr["personalRoyalty"];
                        var shopownerSal = arr["shopownerSal"];
                        var shopownerAche = arr["shopownerAche"];
                        var advererAche = arr["advererAche"];
                        var adviserSal = arr["adviserSal"];
                        var asverBeauDuty = arr["asverBeauDuty"];
                        try
                        {
                            asverBeauDuty1 = arr["asverBeauDuty1"];
                        }
                        catch { }
                        var receptionSal = arr["receptionSal"];
                        var receptionAche = arr["receptionAche"];
                        var operationPerformance = arr["operationPerformance"];
                        var isShopownerCheck = arr["isShopownerCheck"];
                        var deductMoney = arr["deductMoney"];
                        var beautReward = arr["beautReward"];
                        var beautReward1 = arr["beautReward1"];
                        var BeautYSJY = arr["BeautYSJY"];
                        var BeautDJCXJ = arr["BeautDJCXJ"];
                        var shopownerDJCXJ = arr["shopownerDJCXJ"];
                        var adviserDJCXJL = arr["adviserDJCXJL"];

                        var isAccum = arr["isAccum"];
                        var accum = arr["accum"];
                        var isAchieMRS = arr["isAchieMRS"];
                        var isAchieQT = arr["isAchieQT"];
                        var isAchieDZ = arr["isAchieDZ"];
                        var isAchieGW = arr["isAchieGW"];
                        var cardBuckle = arr["cardBuckle"];
                        var isBeauticianCheckQT = arr["isBeauticianCheckQT"];
                        var _isBeauticianCheckQT = Convert.ToInt32(string.IsNullOrEmpty(isBeauticianCheckQT) ? "0" : isBeauticianCheckQT);
                        var _isAchieMRS = Convert.ToInt32(string.IsNullOrEmpty(isAchieMRS) ? "0" : isAchieMRS);
                        var _isAchieQT = Convert.ToInt32(string.IsNullOrEmpty(isAchieQT) ? "0" : isAchieQT);
                        var _isAchieDZ = Convert.ToInt32(string.IsNullOrEmpty(isAchieDZ) ? "0" : isAchieDZ);
                        var _isAchieGW = Convert.ToInt32(string.IsNullOrEmpty(isAchieGW) ? "0" : isAchieGW);
                        var HomeProTiChen = arr["HomeProTiChen"];
                        var isSpecial = arr["isSpecial"];
                        var isHomePro = arr["isHomePro"];
                        var isFullAttendMRS = arr["isFullAttendMRS"];
                        var FullAttendMRS = arr["FullAttendRewMRS"];
                        var isFullAttendGW = arr["isFullAttendGW"];
                        var FullAttendGW = arr["FullAttendRewGW"];
                        var isFullAttendDZ = arr["isFullAttendDZ"];
                        var FullAttendDZ = arr["FullAttendRewDZ"];
                        var isFullAttendQT = arr["isFullAttendQT"];
                        var FullAttendQT = arr["FullAttendRewQT"];
                        var deductMoney1 = arr["deductMoney1"];
                        var receptionDJCXJ = arr["receptionDJCXJ"];
                        var _isFullAttendMRS = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendMRS) ? "0" : isFullAttendMRS);
                        var _isFullAttendGW = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendGW) ? "0" : isFullAttendGW);
                        var _isFullAttendDZ = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendDZ) ? "0" : isFullAttendDZ);
                        var _isFullAttendQT = Convert.ToInt32(string.IsNullOrEmpty(isFullAttendQT) ? "0" : isFullAttendQT);
                        var _deductMoney1 = Convert.ToDecimal(string.IsNullOrEmpty(deductMoney1) ? "0" : deductMoney1);
                        var _deductMoney = Convert.ToDecimal(string.IsNullOrEmpty(deductMoney) ? "0" : deductMoney);
                        var dt1 = ConvertValueHelper.ConvertDateTimeValueNew(entity.Years + "-" + entity.Months + "-" + "01");
                        var dayCount1 = DateTime.DaysInMonth(dt1.Year, dt1.Month);
                        #endregion

                        int DePartMentID = -1;

                        int test = 0;
                        var _yingxianggongxiu = "0";
                        try
                        {
                            if (!string.IsNullOrEmpty(entity.HospitalID.ToString()))
                            {
                                int Inqdays = Convert.ToInt32(entity.HospitalID.ToString());
                                string strsql = @"select top 10 YingXiangGongXiu from EYB_tb_Hospital WHERE ID =  " + Convert.ToInt32(Inqdays) + "";
                                _yingxianggongxiu = GetIntegralByMonth(strsql, 1);
                            }
                        }
                        catch
                        {
                        }
                        foreach (var info in list)
                        {


                            var mod = reserBLL.SelectKaoqingtongji(new DakaRecordEntity()
                            { UserID = info.UserID, Years = entity.Years, Months = entity.Months });

                            #region 超修

                            if (string.IsNullOrEmpty(_yingxianggongxiu)) { _yingxianggongxiu = "0"; }
                            else
                            {
                                mod.Yingxianggongxiu = Convert.ToInt32(_yingxianggongxiu);
                            };
                            int SalaryDay = 0;
                            if (mod.ChuqingDay < (dayCount - mod.Yingxianggongxiu))
                            {
                                if (_isAchieDZ == 1)
                                {
                                    //考勤天数 =出勤天数 + 出勤天数/7
                                    SalaryDay = mod.ChuqingDay + Convert.ToInt32(Math.Floor(Convert.ToDecimal(mod.ChuqingDay / 7)));
                                }
                                else
                                {
                                    //考勤天数 =出勤天数 + 出勤天数/8
                                    SalaryDay = mod.ChuqingDay + Convert.ToInt32(Math.Floor(Convert.ToDecimal(mod.ChuqingDay / 8)));// mod.ChuqingDay / 8;

                                }
                            }
                            else
                            {
                                SalaryDay = dayCount;
                            }
                            mod.Chaoxiuday = mod.XiuxiDay - Convert.ToInt32(_yingxianggongxiu);
                            mod.Chaoxiuday = mod.Chaoxiuday > 0 ? mod.Chaoxiuday : 0;
                            #endregion
                            if (jobid == 1 || jobid == 0)
                            {
                                try
                                {
                                    #region 获取单条统计
                                    //考勤统计
                                    mod.UserName = info.UserName;
                                    mod.UserPost = info.DutyName;

                                    #endregion
                                    if (mod.UserPost == "美容师")
                                    {

                                        //test++;
                                        //if (test > 9) break;

                                        #region 变量
                                        //考勤全勤客流项目奖
                                        decimal qqklxmj = 0;
                                        decimal quangqingjiang = 0;
                                        decimal pingjukeliu = 0;
                                        decimal xiangmu = 0;
                                        var Dixin2 = 0;
                                        var _mrs_scyj_arr = ("," + mrs_scyj_arr).Split('}');//店基础现金总收入  计算参数

                                        var _mrs_djcxj_arr = ("," + mrs_djcxj_arr).Split('}');//个人特殊  计算参数

                                        decimal yejitc = 0;
                                        JoinUserEntity infoo1;
                                        decimal infoo1KakouYeji_Chanpin = 0; decimal infoo1KakouYeji_Liaocheng = 0;
                                        decimal infoo1Yeji_Chanpin = 0; decimal infoo1Yeji_Liaochengka = 0; decimal infoo1Yeji_TeshuChanpin = 0;

                                        var Dixin1 = 0;
                                        JoinUserEntity reszhidingkuandian1 = null;
                                        JoinUserEntity reszhidingfeikuandian1 = null;

                                        var sumzh = 1;//
                                        mod.HospitalRewardALL = sumzh / 2;
                                        var BeauticianSalarr = ("," + BeauticianSal).Split('}');
                                        var item = BeauticianSalarr;
                                        #endregion

                                        #region

                                        #endregion
                                        var res = result.Where(x => x.UserID == info.UserID).ToList();
                                        #region 底薪1
                                        if (res.Count() > 0)
                                        {
                                            var infoo = res[0];
                                            var sumshichao = infoo.ShiCao_zhiding;
                                            DixinOne(res[0], mod, item, sumshichao);//底薪1
                                        }
                                        #endregion
                                        #region 底薪2

                                        var kel = listkekiu.Where(x => x.UserID == info.UserID).ToList();
                                        if (kel.Count() > 0)
                                        {
                                            var infoo = kel[0];
                                            seachPatient.IsKuaDian = 1;
                                            seachPatient.UserID = info.UserID;
                                            var ent = iCentBll.PageListPassengerTraffic(seachPatient).Where(x => x.UserID == info.UserID).FirstOrDefault();
                                            var kuandiankeliu = (ent.fuwuPassengerTraffic - ent.JianquKeliu) - (infoo.fuwuPassengerTraffic - infoo.JianquKeliu);
                                            var sumkeliu = (infoo.fuwuPassengerTraffic - infoo.JianquKeliu) + infoo.Keliu + kuandiankeliu;
                                            DixinTwo(mod, item, sumkeliu);//底薪2
                                            mod.MrsDixin2 = mod.MrsDixin2;// < 500 ? 500 : mod.MrsDixin2;
                                            mod.MrsDixin2keliu = (decimal)sumkeliu;
                                            /* 新客*/
                                        }

                                        mod.MrsDixin1 = mod.MrsDixin1 > 0 ? mod.MrsDixin1 : Convert.ToInt32(item[5].Split(',')[2].Split(':')[1]);
                                        mod.MrsDixin2 = mod.MrsDixin2 > 0 ? mod.MrsDixin2 : Convert.ToInt32(item[5].Split(',')[7].Split(':')[1]);
                                        mod.MrsDixin = Convert.ToInt32(mod.MrsDixin1) + Convert.ToInt32(mod.MrsDixin2);
                                        mod.DixinSalary = Convert.ToInt32(mod.MrsDixin1) + Convert.ToInt32(mod.MrsDixin2);//总底薪

                                        mod.JixiaoReward = Convert.ToInt32(quangqingjiang + qqklxmj + mod.NewCusReward);//绩效奖金
                                        mod.QuanqingReward = (int)quangqingjiang;
                                        #endregion
                                        #region 指定和非指定客和手工费可实操
                                        var _resultzhidingmrs = resultzhidingmrs.Where(x => x.UserID == info.UserID).ToList();
                                        if (_resultzhidingmrs.Count() > 0)
                                        {
                                            var infoomrs = _resultzhidingmrs[0];
                                            mod.ShiCao_zhiding = infoomrs.ShiCao_zhiding;
                                        }

                                        var _resultfeizhidingmrs = resultfeizhidingmrs.Where(x => x.UserID == info.UserID).ToList();
                                        if (_resultfeizhidingmrs.Count() > 0)
                                        {
                                            var infoomrsfei = _resultfeizhidingmrs[0];
                                            mod.ShiCao_feizhiding = infoomrsfei.ShiCao_zhiding;
                                        }
                                        var _operationPerformance = ("," + operationPerformance).Split('}');
                                        var _BeautYSJY = BeautYSJY.Replace("}", "").Split(',');
                                        var ag = 0; var tichengzhidingke = 0;//指定客提成
                                        var fg = 0; var feitichengzhidingke = 0;//非指定客提

                                        if (res.Count() > 0)
                                        {

                                            var info1 = res[0];
                                            var _MouthShiCaoTicheng_zhiding = mod.ShiCao_zhiding;// info1.ShiCao_zhiding + info1.kuadianShiCao_zhiding;
                                            var _MouthShiCaoTicheng_feizhiding = mod.ShiCao_feizhiding;// info1.ShiCao_feizhiding + info1.kuadianShiCao_feizhiding;
                                            var _MouthShiCao = _MouthShiCaoTicheng_zhiding + _MouthShiCaoTicheng_feizhiding;
                                            mod.MrsMouthShicao = _MouthShiCao;
                                            if (_MouthShiCao > Convert.ToDecimal(string.IsNullOrEmpty(_BeautYSJY[4].Split(':')[1]) ? "0" : _BeautYSJY[4].Split(':')[1]))
                                            {
                                                ag = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[0].Split(',')[4].Split(':')[1]) * 100);//指定客提成 * 100
                                                fg = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[1].Split(',')[4].Split(':')[1]) * 100);//非指定客提成 * 100

                                            }
                                            else if (_MouthShiCao > Convert.ToDecimal(string.IsNullOrEmpty(_BeautYSJY[2].Split(':')[1]) ? "0" : _BeautYSJY[2].Split(':')[1]))
                                            {
                                                ag = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[0].Split(',')[3].Split(':')[1]) * 100);//指定客提成 * 100
                                                fg = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[1].Split(',')[3].Split(':')[1]) * 100);//非指定客提成 * 100
                                            }
                                            else
                                            {
                                                ag = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[0].Split(',')[2].Split(':')[1]) * 100);//指定客提成 * 100
                                                fg = Convert.ToInt32(Convert.ToDecimal(_operationPerformance[1].Split(',')[2].Split(':')[1]) * 100);//非指定客提成 * 100

                                            }
                                            mod.Mrsag = ag / 100;
                                            mod.Mrsfg = fg / 100;
                                            tichengzhidingke = Convert.ToInt32((decimal)_MouthShiCaoTicheng_zhiding * (decimal)ag / (decimal)10000);
                                            feitichengzhidingke = Convert.ToInt32((decimal)_MouthShiCaoTicheng_feizhiding * (decimal)fg / (decimal)10000);
                                            mod.Mrstichengzhidingke = tichengzhidingke + feitichengzhidingke;
                                        }
                                        var ShouGongFeiModel = result.Where(t => t.DutyName == mod.UserPost && t.UserName == mod.UserName).ToList();
                                        if (ShouGongFeiModel.Count() > 0)
                                        {
                                            mod.ShouGongFei = ShouGongFeiModel[0].Ticheng;
                                            mod.Jiangli = ShouGongFeiModel[0].OtherTiCheng;
                                        }
                                        mod.Sumshicaoshougong = tichengzhidingke + feitichengzhidingke + mod.ShouGongFei + mod.Jiangli;
                                        #endregion

                                        #endregion

                                        mod.Yeji_tushuchangpingzhidingf = "";
                                        mod.Yeji_tushuxiangmuzhidingf = "";
                                        mod.MrsFangdangBenyuef = "";
                                        #region 现金业绩提成
                                        if (res.Count() > 0)
                                        {
                                            Dictionary<string, string> shopownerDJCXJ_item = JsonConvert.DeserializeObject<Dictionary<string, string>>(shopownerDJCXJ);
                                            var infoo = res[0];
                                            var keys = shopownerDJCXJ_item.Keys;
                                            #region  店基础现金总收入
                                            #endregion
                                            #region
                                            var iii = 7;
                                            var _isSpecial = Convert.ToInt32(string.IsNullOrEmpty(isSpecial) ? "0" : isSpecial);
                                            var _cardBuckle = Convert.ToDecimal(string.IsNullOrEmpty(cardBuckle) ? "0" : cardBuckle);
                                            if (_resultzhidingmrs.Count() > 0)
                                            {
                                                var infoomrs = _resultzhidingmrs[0];
                                                mod.Yeji_Chuzhikazhiding = infoomrs.Yeji_Chuzhika;
                                                mod.Yeji_liaochengdancizhiding = infoomrs.Yeji_Liaochengka + infoomrs.Yeji_Xiangmu;
                                                mod.Yeji_jiajuzhiding = infoomrs.Yeji_Chanpin;
                                                mod.Yeji_daxiangmuzhiding = infoomrs.Yeji_Daxiangmu;
                                            }
                                            if (_resultfeizhidingmrs.Count() > 0)
                                            {
                                                var infoomrsfei = _resultfeizhidingmrs[0];
                                                mod.Yeji_Chuzhikafeizhiding = infoomrsfei.Yeji_Chuzhika;
                                                mod.Yeji_liaochengdancifeizhiding = infoomrsfei.Yeji_Liaochengka + infoomrsfei.Yeji_Xiangmu;
                                                mod.Yeji_jiajufeizhiding = infoomrsfei.Yeji_Chanpin;
                                                mod.Yeji_daxiangmufeizhiding = infoomrsfei.Yeji_Daxiangmu;
                                            }
                                            for (int i = shopownerDJCXJ_item.Count - 1; i >= 0; i--)
                                            {
                                                var yejize = Convert.ToDecimal(shopownerDJCXJ_item["scope" + i]);
                                                i--;
                                                iii--;
                                                if (HospitalYeji > yejize || HospitalYeji == (decimal)yejize)
                                                {
                                                    mod.MrsHospitalYeji = HospitalYeji;
                                                    mod.Mrssdyeji = yejize;
                                                    yejitc = mod.Yeji_Chuzhikazhiding * Convert.ToDecimal(_mrs_scyj_arr[0].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人会员卡指定客提成
                                                    mod.Yeji_Chuzhikazhidingf = _mrs_scyj_arr[0].Split(',')[iii].Split(':')[1];
                                                    yejitc += (mod.Yeji_liaochengdancizhiding) * Convert.ToDecimal(_mrs_scyj_arr[2].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居指定客提成
                                                    mod.Yeji_liaochengdancizhidingf = _mrs_scyj_arr[2].Split(',')[iii].Split(':')[1];
                                                    yejitc += (mod.Yeji_jiajuzhiding) * Convert.ToDecimal(_mrs_scyj_arr[4].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居指定客提成
                                                    mod.Yeji_jiajuzhidingf = _mrs_scyj_arr[4].Split(',')[iii].Split(':')[1];


                                                    yejitc += mod.Yeji_Chuzhikafeizhiding * Convert.ToDecimal(_mrs_scyj_arr[1].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人会员卡非指定客提成
                                                    mod.Yeji_Chuzhikafeizhidingf = _mrs_scyj_arr[1].Split(',')[iii].Split(':')[1];
                                                    yejitc += (mod.Yeji_liaochengdancifeizhiding) * Convert.ToDecimal(_mrs_scyj_arr[3].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居非指定客提成
                                                    mod.Yeji_liaochengdancifeizhidingf = _mrs_scyj_arr[3].Split(',')[iii].Split(':')[1];
                                                    yejitc += (mod.Yeji_jiajufeizhiding) * Convert.ToDecimal(_mrs_scyj_arr[5].Split(',')[iii].Split(':')[1]) / 100;//实操提成_个人现金疗程家居非指定客提成
                                                    mod.Yeji_jiajufeizhidingf = _mrs_scyj_arr[5].Split(',')[iii].Split(':')[1];
                                                    mod.Yeji_tushuchangpingzhidingf = "未开启";
                                                    mod.Yeji_tushuxiangmuzhidingf = "未开启";
                                                    if (_isSpecial == 1)//是特殊提成
                                                    {
                                                        yejitc += infoo.Yeji_TeshuChanpin * Convert.ToDecimal(specialProduct) / 100;
                                                        mod.Yeji_tushuchangpingzhiding = infoo.Yeji_TeshuChanpin;
                                                        mod.Yeji_tushuchangpingzhidingf = specialProduct.ToString() + "%";
                                                        yejitc += infoo.Yeji_Teshuxiangmu * Convert.ToDecimal(specialProject) / 100;
                                                        mod.Yeji_tushuxiangmuzhiding = infoo.Yeji_Teshuxiangmu;
                                                        mod.Yeji_tushuxiangmuzhidingf = specialProject.ToString() + "%";
                                                    }
                                                    yejitc += (mod.Yeji_daxiangmuzhiding) * Convert.ToDecimal(_mrs_djcxj_arr[0].Split(',')[1].Split(':')[1]) / 100;//实操提成大项目提成指定客提成
                                                    mod.Yeji_daxiangmuzhidingf = _mrs_djcxj_arr[0].Split(',')[1].Split(':')[1];
                                                    yejitc += (mod.Yeji_daxiangmufeizhiding) * Convert.ToDecimal(_mrs_djcxj_arr[0].Split(',')[2].Split(':')[1]) / 100;//实操提成_大项目个人提成非指定客
                                                    mod.Yeji_daxiangmufeizhidingf = _mrs_djcxj_arr[0].Split(',')[2].Split(':')[1];
                                                    #region
                                                    #endregion
                                                    break;
                                                }

                                            }

                                            var _isHomePro = Convert.ToInt32(string.IsNullOrEmpty(isHomePro) ? "0" : isHomePro);
                                            mod.MrsFangdangBenyuef = "未开启";
                                            if (_isHomePro == 1)
                                            {

                                                var _HomeProTiChen = Convert.ToDecimal(string.IsNullOrEmpty(HomeProTiChen) ? "0" : HomeProTiChen);
                                                var _listPers = listPers.Where(x => x.UserID == info.UserID).ToList();
                                                if (_listPers.Count() > 0)
                                                {
                                                    var Benyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                                                    {
                                                        s_HospitalID = entityFangd.s_HospitalID,
                                                        s_UserID = _listPers[0].UserID,
                                                        s_StareTime = dtFirst,
                                                        s_Endtime = dtLast,
                                                        ProductType = entityFangd.ProductType
                                                    });
                                                    if (Benyue > (decimal)0)
                                                    {
                                                        var Shangeyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                                                        {
                                                            s_HospitalID = entityFangd.s_HospitalID,
                                                            s_UserID = _listPers[0].UserID,
                                                            s_StareTime = dtFirst.AddMonths(-1),
                                                            s_Endtime = dtLast.AddMonths(-1),
                                                            ProductType = entityFangd.ProductType
                                                        });
                                                        var Qianyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                                                        {
                                                            s_HospitalID = entityFangd.s_HospitalID,
                                                            s_UserID = _listPers[0].UserID,
                                                            s_StareTime = dtFirst.AddMonths(-2),
                                                            s_Endtime = dtLast.AddMonths(-2),
                                                            ProductType = entityFangd.ProductType
                                                        });
                                                        if (Shangeyue > (decimal)0 || Qianyue > (decimal)0)
                                                        {
                                                            mod.MrsFangdang = Convert.ToInt32(Benyue * (decimal)_HomeProTiChen / 100);
                                                            yejitc += mod.MrsFangdang;
                                                            mod.MrsFangdangBenyue = Benyue;
                                                        }
                                                    }
                                                }
                                                mod.MrsFangdangBenyuef = _HomeProTiChen.ToString() + "%";
                                            }
                                            yejitc = Math.Round(yejitc, 2);
                                            mod.MrsFangdang = mod.MrsFangdang > 0 ? mod.MrsFangdang : 0;
                                            mod.MrsFangdangBenyue = mod.MrsFangdangBenyue > 0 ? mod.MrsFangdangBenyue : 0;
                                            #endregion
                                            mod.MrsshicaoStr = yejitc.ToString();

                                        }

                                        #endregion

                                        #region 卡扣业绩
                                        if (res.Count() > 0)
                                        {

                                            var _cardBuckle = Convert.ToDecimal(string.IsNullOrEmpty(cardBuckle) ? "0" : cardBuckle);
                                            var infoo = res[0];
                                            mod.Mrskaikouyeji = infoo.KakouYeji_Liaocheng + infoo.KakouYeji_Chanpin + infoo.KakouYeji_TeshuChanpin + infoo.KakouYeji_Xiangmu + infoo.KakouYeji_Daxiangmu + infoo.KakouYeji_Teshuxiangmu;
                                            mod.MrsSumkaikouyeji = mod.Mrskaikouyeji * _cardBuckle / 100;
                                            mod.Mrskaikouyejif = _cardBuckle.ToString();

                                        }
                                        #endregion

                                        mod.Mrstichengzhidingke = mod.Mrstichengzhidingke > 0 ? mod.Mrstichengzhidingke : 0;

                                        #region 奖励&扣罚

                                        if (kel.Count() > 0)
                                        {
                                            var infoo = kel[0];
                                            /* 新客*/
                                            var Customerarr = msr_xktc.Split(',');
                                            if (infoo.CustomerService == 0) { mod.NewCusReward = Convert.ToInt32(Customerarr[0].Split(':')[1]); }
                                            else if (infoo.CustomerService == 1) { mod.NewCusReward = Convert.ToInt32(Customerarr[1].Split(':')[1]); }
                                            else if (infoo.CustomerService == 2) { mod.NewCusReward = Convert.ToInt32(Customerarr[2].Split(':')[1]); }
                                            else if (infoo.CustomerService == 3) { mod.NewCusReward = Convert.ToInt32(Customerarr[3].Split(':')[1]); }
                                            else if (infoo.CustomerService == 4) { mod.NewCusReward = Convert.ToInt32(Customerarr[4].Split(':')[1]); }
                                            else { mod.NewCusReward = Convert.ToInt32(Customerarr[5].Split(':')[1].Replace("}", "")); }
                                            mod.CustomerService = infoo.CustomerService;
                                        }

                                        if (mod.ChuqingDay + mod.XiuxiDay == dayCount1)
                                        {
                                            var keliu = listkekiu.Where(x => x.UserID == info.UserID).ToList();
                                            if (keliu.Count() > 0 && mod.ShijiChuqingDay > 0)
                                            {
                                                pingjukeliu = Convert.ToDecimal(mod.MrsDixin2keliu / mod.ShijiChuqingDay); //平均客流

                                            }
                                            var getxiangmu = xiangmulist.Where(x => x.UserID == info.UserID).ToList();
                                            if (getxiangmu.Count() > 0)
                                            {
                                                xiangmu = Convert.ToDecimal(getxiangmu[0].ProjectNumber);//项目数
                                            }
                                            if (Getqqklxmj(beautReward1, pingjukeliu, xiangmu) == (decimal)0)
                                            {
                                                qqklxmj = Getqqklxmj(beautReward, pingjukeliu, xiangmu);
                                            }
                                            else
                                            {
                                                qqklxmj = Getqqklxmj(beautReward1, pingjukeliu, xiangmu);
                                            }
                                            mod.Mrspingjukeliu = pingjukeliu;
                                            mod.Mrsxiangmu = xiangmu;
                                            mod.Mrsqqklxmj = qqklxmj;
                                            if (_isFullAttendMRS == 1)
                                            {
                                                quangqingjiang = (decimal)50;
                                                mod.Mrsquangqingjiang = quangqingjiang;
                                            }
                                        }
                                        mod.Mrsqqklxmj = mod.Mrsqqklxmj > 0 ? mod.Mrsqqklxmj : 0;
                                        mod.Mrsdanxiangmuf = "";
                                        if (res.Count() > 0)
                                        {
                                            //var infoo = res[0];
                                            //var _isAccum = Convert.ToInt32(string.IsNullOrEmpty(isAccum) ? "0" : isAccum);
                                            //var _accum = Convert.ToInt32(string.IsNullOrEmpty(accum) ? "0" : accum);
                                            //if (_isAccum == 1)
                                            //{
                                            //    mod.Mrsdanxiangmu = Convert.ToInt32(infoo.Yeji_Xiangmu) > _accum ? _accum : Convert.ToInt32(infoo.Yeji_Xiangmu);
                                            //    mod.Mrsdanxiangmuf = mod.Mrsdanxiangmu.ToString();
                                            //}
                                            //else
                                            //{
                                            //    mod.Mrsdanxiangmu = Convert.ToInt32(infoo.Yeji_Xiangmu);
                                            //    mod.Mrsdanxiangmuf = "未开启".ToString();
                                            //}

                                            mod.Mrsdanxiangmu = 0;// mod.Mrsdanxiangmu > 0 ? mod.Mrsdanxiangmu : 0;
                                        }
                                        mod.IsQuanqing = "否";
                                        mod.QuanqingRewardf = "未开启";

                                        if (_isFullAttendMRS == 1)
                                        {
                                            mod.QuanqingRewardf = "0";
                                            if (mod.ChuqingDay + mod.XiuxiDay == dayCount1)
                                            {
                                                mod.IsQuanqing = "是";
                                                mod.QuanqingReward = Convert.ToInt32(FullAttendMRS);
                                                mod.QuanqingRewardf = FullAttendMRS.ToString();
                                            }
                                        }

                                        mod.MrsHospitalYeji = HospitalYeji;
                                        mod.MrsshicaoStr = string.IsNullOrEmpty(mod.MrsshicaoStr) ? "0" : yejitc.ToString();
                                        var jiaoxi1 = Convert.ToInt32(mod.Mrsdanxiangmu + mod.Mrstichengzhidingke + mod.MrsFangdang + Convert.ToInt32(Convert.ToDecimal(mod.MrsshicaoStr)));
                                        mod.JixiaoTicheng = jiaoxi1;
                                        mod.JiangliKoufang = mod.NewCusReward + Convert.ToInt32(mod.Mrsqqklxmj) + mod.Mrsdanxiangmu + mod.QuanqingReward;


                                        #endregion
                                        mod.IsDixinSalary = "否";
                                        decimal dixin = mod.MrsDixin;
                                        if (_isAchieQT == 1)
                                        {
                                            mod.IsDixinSalary = "是";
                                            dixin = mod.MrsDixin * SalaryDay / dayCount;
                                        }
                                        var zhineng = Convert.ToInt32(mod.Sumshicaoshougong) + Convert.ToInt32(yejitc) + Convert.ToInt32(mod.MrsSumkaikouyeji) + Convert.ToInt32(dixin);
                                        mod.Iszhinengkaoping = "否";
                                        mod.Xingzhen = 0;
                                        if (list1.Count() > 0)
                                        {
                                            var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items1.Count() > 0)
                                            {
                                                var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                                if (items.Count() > 0)
                                                    mod.Xingzhen = Convert.ToInt32(string.IsNullOrEmpty(items[0].Memo) ? "0" : items[0].Memo);
                                            }
                                        }
                                        if (_isBeauticianCheckQT == 1)
                                        {
                                            mod.Iszhinengkaoping = "是";
                                            if (list1.Count() > 0)
                                            {
                                                var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                                if (items1.Count() > 0)
                                                {
                                                    var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                                    if (items.Count() > 0)
                                                        mod.Fenshu = Convert.ToInt32(string.IsNullOrEmpty(items[0].NumDay) ? "80" : items[0].NumDay);
                                                    else
                                                        mod.Fenshu = 80;
                                                    if (mod.Fenshu < 80)
                                                        zhineng = Convert.ToInt32((1 - (decimal)(80 - mod.Fenshu) / 100) * zhineng);
                                                }
                                                else
                                                {
                                                    mod.Fenshu = 80;
                                                }
                                            }
                                            else
                                            {
                                                mod.Fenshu = 80;
                                            }
                                        }
                                        else
                                        {
                                            mod.Fenshu = 80;
                                        }
                                        mod.Mrsquangqingjiang = mod.Mrsquangqingjiang > 0 ? mod.Mrsquangqingjiang : 0;
                                        mod.Mrspingjukeliu = mod.Mrspingjukeliu > 0 ? mod.Mrspingjukeliu : 0;
                                        mod.Mrsxiangmu = mod.Mrsxiangmu > 0 ? mod.Mrsxiangmu : 0;
                                        mod.ShifaSalary = zhineng + mod.JiangliKoufang - mod.Xingzhen;
                                        if (jobid == 1)
                                        {
                                            newlist.Add(mod);
                                        }

                                    }
                                }
                                catch { }
                            }
                            if (jobid == 2 || jobid == 0)
                            {
                                mod.UserPost = info.DutyName;
                                if (mod.UserPost == "店长")
                                {
                                    #region 获取单条统计
                                    //考勤统计
                                    //考勤全勤客流项目奖

                                    mod.UserName = info.UserName;
                                    mod.UserName = info.UserName;

                                    #region 超休 6天
                                    mod.DzChaoxiuStr = ("否").ToString();
                                    if (jobid == 2 || jobid == 4 || jobid == 0)
                                    {
                                        if (isExceedRestshopowner == "1")
                                        { //如果超休

                                            mod.DzChaoxiuStr = ("是").ToString();
                                            if (mod.Chaoxiuday > 0)
                                            {
                                                if (mod.Chaoxiuday > 6)
                                                {
                                                    dzhospitalglyj = (HospitalYeji * Convert.ToDecimal((1 - ((mod.Chaoxiuday - 6) * (decimal)1.5 / days) - ((6) * (decimal)1 / days))));
                                                    dzhospitalglyj = HospitalYeji > 0 ? HospitalYeji : 0;
                                                    HospitalYejiJx = dzhospitalglyj;
                                                    //超休只扣现金业绩部分
                                                    dzhospitalallglyj = dzhospitalglyj + sum_feizhid.ShiCao + sum_feizhid.KakouYeji_Chanpin + sum_feizhid.KakouYeji_TeshuChanpin;
                                                    dzhospitalallglyj = dzhospitalallglyj / 2;
                                                    HospitalZhongheYeji = dzhospitalallglyj;
                                                }
                                                else
                                                {
                                                    dzhospitalglyj = (HospitalYeji * (1 - ((mod.Chaoxiuday) * (decimal)1 / days)));
                                                    dzhospitalglyj = HospitalYeji > 0 ? HospitalYeji : 0;
                                                    HospitalYejiJx = dzhospitalglyj;
                                                    //超休只扣现金业绩部分
                                                    dzhospitalallglyj = dzhospitalglyj + sum_feizhid.ShiCao + sum_feizhid.KakouYeji_Chanpin + sum_feizhid.KakouYeji_TeshuChanpin;
                                                    dzhospitalallglyj = dzhospitalallglyj / 2;
                                                    HospitalZhongheYeji = dzhospitalallglyj;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    decimal qqklxmj = 0;
                                    decimal quangqingjiang = 0;
                                    //全勤
                                    mod.ShouGongFei = result.Sum(t => t.Ticheng);
                                    decimal pingjukeliu = 0;
                                    decimal xiangmu = 0;
                                    var res = result.Where(x => x.UserID == info.UserID).ToList();
                                    #endregion

                                    var _shopownerSal = ("," + shopownerSal).Split('}');
                                    var item = _shopownerSal;
                                    #region 底薪1
                                    var Dixin1 = 0;

                                    mod.HospitalXianjinYeji = (decimal)HospitalYeji;


                                    mod.HospitalXianjinYeji = (decimal)HospitalYeji;
                                    DixinOne(dianzhang, mod, item, (decimal)HospitalYeji);//底薪1
                                    #endregion
                                    #region 底薪2

                                    var Dixin2 = 0;
                                    var sumkeliu = mod1.YouXiaoKeLiu;
                                    mod.MrsDixin2Rentou = mod1.YouXiaoRenTou;
                                    DixinTwo(mod, item, mod.MrsDixin2Rentou);//底薪2

                                    #endregion


                                    #region 业绩提成
                                    if (HospitalZhongheYeji > 0) mod.HospitalRewardALL = HospitalZhongheYeji;
                                    var yejitc = GetYejiTi1(mod, HospitalYejiJx, mod.HospitalRewardALL, shopownerAche, shopownerDJCXJ);
                                    mod.XiangmuYeji = HospitalDaxiangmuYeji;
                                    var yejitcdaxiangmu = GetYejiTiDaxianmu(mod, HospitalYejiJx, HospitalDaxiangmuYeji, shopownerAche, shopownerDJCXJ);
                                    mod.MrsshicaoStr = (yejitc + yejitcdaxiangmu).ToString();

                                    var ShouGongFeiModel = result.Where(t => t.DutyName == mod.UserPost && t.UserName == mod.UserName).ToList();
                                    if (ShouGongFeiModel.Count() > 0)
                                    {
                                        mod.ShouGongFei = ShouGongFeiModel[0].Ticheng;
                                        mod.Jiangli = ShouGongFeiModel[0].OtherTiCheng;
                                    }
                                    mod.DzYejiSum = Convert.ToInt32(((mod.ShouGongFei + mod.Jiangli + (decimal)mod.HospitalRewardALL * Convert.ToDecimal(mod.DzHospitalYejiFilter) + (decimal)mod.XiangmuYeji * Convert.ToDecimal(mod.DzDaxiangmuYejiFilter)) / 100)).ToString();
                                    mod.JixiaoTicheng = yejitc + yejitcdaxiangmu;
                                    #endregion
                                    #region 奖励&扣罚

                                    var BenyueRentou = mod1.YouXiaoRenTou;
                                    mod.DzBenyueRentou = BenyueRentou;
                                    var ShangyueRentou = listkekiushang.YouXiaoRenTou;
                                    mod.DzShangyueRentou = ShangyueRentou;
                                    GetBijiaoRentou(entity, BenyueRentou, ShangyueRentou, mod, _deductMoney);

                                    mod.BijiaoRentou = mod.BijiaoRentou != 0 ? mod.BijiaoRentou : 0;
                                    mod.BijiaoRentouKoukuang = mod.BijiaoRentouKoukuang != 0 ? mod.BijiaoRentouKoukuang : 0;


                                    mod.IsQuanqing = "否";
                                    mod.QuanqingRewardf = "未开启";

                                    if (_isFullAttendDZ == 1)
                                    {
                                        mod.QuanqingRewardf = "0";
                                        if (mod.ChuqingDay + mod.XiuxiDay == dayCount1)
                                        {
                                            mod.IsQuanqing = "是";
                                            mod.QuanqingReward = Convert.ToInt32(FullAttendDZ);
                                            mod.QuanqingRewardf = FullAttendDZ.ToString();
                                        }
                                    }

                                    if (_isFullAttendDZ == 1)
                                    {
                                        quangqingjiang = (decimal)50;
                                        mod.Mrsquangqingjiang = quangqingjiang;
                                    }
                                    mod.Mrsquangqingjiang = mod.Mrsquangqingjiang > 0 ? mod.Mrsquangqingjiang : 0;
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                            mod.DzJiangjin = items1[0].EndYear;
                                    }
                                    mod.DzKoufangCoutz = mod.QuanqingReward + mod.BijiaoRentouKoukuang + Convert.ToDecimal(mod.DzJiangjin);
                                    mod.DzKoufangCout = (mod.QuanqingReward + mod.BijiaoRentouKoukuang + Convert.ToDecimal(mod.DzJiangjin)).ToString();

                                    #endregion
                                    #region 店长责任指标

                                    var newkeliu = listkekiu.Sum(x => x.CustomerService); //新客流
                                    int GZZZ = GetGZZhize1(mod, HospitalYeji, sumkeliu, newkeliu, asverBeauDuty);
                                    var chengshuike = 0;
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                            chengshuike = items1[0].EndMonths;
                                    }
                                    mod.XianJinYeJi = HospitalYeji.ToString();
                                    mod.JiangliDuty = GZZZ + chengshuike;
                                    mod.ChengSuiKeJiangfang = chengshuike.ToString();
                                    mod.sumkeliu = sumkeliu;
                                    mod.newkeliu = newkeliu;
                                    mod.JixiaoReward = Convert.ToInt32(mod.BijiaoRentouKoukuang) + mod.JiangliDuty;//业绩资金; 
                                    #endregion

                                    mod.MrsDixin2keliu = (decimal)mod.MrsDixin2Rentou;
                                    mod.MrsDixin1 = mod.MrsDixin1;
                                    mod.MrsDixin2 = mod.MrsDixin2;//总底薪
                                    mod.DixinSalary = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪                                   

                                    var zhineng = mod.JixiaoTicheng + mod.DixinSalary;
                                    mod.IsDixinSalary = "否";
                                    if (_isAchieQT == 1)
                                    {
                                        mod.IsDixinSalary = "是";
                                        zhineng = zhineng * SalaryDay / dayCount;
                                    }
                                    mod.Iszhinengkaoping = "否";
                                    mod.Xingzhen = 0;
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                        {
                                            var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items.Count() > 0)
                                                mod.Xingzhen = Convert.ToInt32(string.IsNullOrEmpty(items[0].Memo) ? "0" : items[0].Memo);
                                        }
                                    }
                                    if (_isBeauticianCheckQT == 1)
                                    {
                                        mod.Iszhinengkaoping = "是";
                                        if (list1.Count() > 0)
                                        {
                                            var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items1.Count() > 0)
                                            {
                                                var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                                if (items.Count() > 0)
                                                {
                                                    mod.Fenshu = Convert.ToInt32(string.IsNullOrEmpty(items[0].NumDay) ? "80" : items[0].NumDay);
                                                }
                                                else
                                                {
                                                    mod.Fenshu = 80;
                                                }
                                                if (mod.Fenshu < 80)
                                                    zhineng = Convert.ToInt32((1 - (decimal)(80 - mod.Fenshu) / 100) * zhineng);
                                            }
                                            else
                                            {
                                                mod.Fenshu = 80;
                                            }
                                        }
                                        else
                                        {
                                            mod.Fenshu = 80;
                                        }
                                    }
                                    else
                                    {
                                        mod.Fenshu = 80;
                                    }
                                    mod.ShifaSalary = zhineng + Convert.ToInt32(mod.JiangliDuty) + Convert.ToInt32(mod.DzKoufangCoutz) - Convert.ToInt32(mod.Xingzhen);
                                    if (jobid == 2)
                                    {
                                        newlist.Add(mod);
                                    }
                                }
                            }
                            if (jobid == 3 || jobid == 0)
                            {
                                #region 获取单条统计
                                //考勤统计
                                mod.UserName = info.UserName;
                                mod.UserPost = info.DutyName;

                                #endregion
                                if (mod.UserPost == "顾问")
                                {

                                    //考勤全勤客流项目奖
                                    decimal quangqingjiang = 0;
                                    //全勤
                                    var keliu = listkekiu.Where(x => x.UserID == info.UserID).ToList();
                                    var getxiangmu = xiangmulist.Where(x => x.UserID == info.UserID).ToList();
                                    var res = result.Where(x => x.UserID == info.UserID).ToList();
                                    mod.HospitalRewardALL = 0;// = sumzh / 2;
                                    decimal sumzhs = 0;
                                    decimal sumzhsGw = 0;
                                    decimal sumzhsall = 0;
                                    decimal XiangmuYeji = 0;


                                    var _adviserSal = ("," + adviserSal).Split('}');
                                    var item = _adviserSal;
                                    #region 组别
                                    if (keliu.Count() > 0)
                                    {
                                        DePartMentID = keliu[0].DepartmentID;
                                    }
                                    var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.HospitalID, 1).Where(c => c.ID == DePartMentID).ToList();
                                    mod.DePartMentStr = departList != null && departList.Count() > 0 ? departList[0].Name : "";
                                    #endregion
                                    #region 底薪1
                                    if (res.Count() > 0)
                                    {
                                        var infoo = res[0];
                                        DePartMentID = infoo.DepartmentID;
                                        var listDepartMent = result.Where(x => x.DepartmentID == DePartMentID);
                                        mod.ShouGongFei = listDepartMent.Sum(t => t.Ticheng);
                                        foreach (var infooo in listDepartMent)
                                        {
                                            sumzhs += infooo.Yeji_Chuzhika + infooo.Yeji_Liaochengka + infooo.Yeji_Chanpin + infooo.Yeji_TeshuChanpin + infooo.Yeji_Xiangmu + infooo.Yeji_Teshuxiangmu;
                                            sumzhsGw += (infooo.ShiCao + infooo.KakouYeji_Chanpin + infooo.KakouYeji_TeshuChanpin);
                                            sumzhsall += (infooo.Yeji_Chuzhika + infooo.Yeji_Liaochengka + infooo.Yeji_Chanpin + infooo.Yeji_TeshuChanpin + infooo.Yeji_Xiangmu + infooo.Yeji_Teshuxiangmu + infooo.ShiCao + infooo.KakouYeji_Chanpin + infooo.KakouYeji_TeshuChanpin);
                                            XiangmuYeji += infooo.Yeji_Daxiangmu;
                                        }

                                        mod.HospitalRewardALL = Math.Round(sumzhsall / 2, 2);


                                        DixinOne(res[0], mod, item, sumzhs);//底薪1

                                    }
                                    #endregion
                                    #region 底薪2

                                    var Dixin2 = 0;
                                    var kel = keliu;

                                    var rentou = 0;
                                    decimal sumkeliu = 0;
                                    decimal istkekiuCustomerService = 0;


                                    if (kel.Count() > 0)
                                    {
                                        var kk = listkekiu.Where(x => x.DepartmentID == DePartMentID).Sum(x => x.HeadCount);
                                        sumkeliu = (decimal)kk;
                                        DixinTwo(mod, item, (decimal)sumkeliu);//底薪2

                                        mod.MrsDixin2keliu = (decimal)sumkeliu;
                                        mod.MrsDixin1 = mod.MrsDixin1;
                                        mod.MrsDixin2 = mod.MrsDixin2;
                                        //mod.DixinSalary = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪
                                        //mod.DixinSalaryf = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪


                                        var infoo = kel[0];
                                        DePartMentID = infoo.DepartmentID;
                                        seachPatient.DepartmentID = DePartMentID;
                                        decimal Keliu1 = 0, fuwuPassengerTraffic1 = 0, kuadianKeliu1 = 0;
                                        var keliug = listkekiuGuwenKwliu.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().Keliu;
                                        var fuwuPassengerTrafficg = listkekiuGuwenPass.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().fuwuPassengerTraffic;
                                        var JianquKeliug = listkekiuGuwenJiangqu.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().JianquKeliu;
                                        kuadianKeliu1 = (fuwuPassengerTrafficg - JianquKeliug) - (fuwuPassengerTrafficg - JianquKeliug);
                                        Keliu1 = keliug;
                                        fuwuPassengerTraffic1 = (fuwuPassengerTrafficg - JianquKeliug);
                                        sumkeliu = Keliu1 + fuwuPassengerTraffic1 + kuadianKeliu1;
                                        istkekiuCustomerService = listkekiu.Where(x => x.DepartmentID == DePartMentID).Sum(x => x.CustomerService);// listkekiuGuwenRentouCustomerService.Where(x => x.DepartmentID == DePartMentID).FirstOrDefault().CustomerService;


                                    }

                                    mod.MrsDixin1 = mod.MrsDixin1 > 0 ? mod.MrsDixin1 : Convert.ToInt32(item[4].Split(',')[2].Split(':')[1]);
                                    mod.MrsDixin2 = mod.MrsDixin2 > 0 ? mod.MrsDixin2 : Convert.ToInt32(item[4].Split(',')[7].Split(':')[1]);
                                    mod.DixinSalary = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪
                                    mod.DixinSalaryf = (mod.MrsDixin1 + Convert.ToInt32(mod.MrsDixin2));//总底薪
                                    mod.BijiaoRentou = mod.BijiaoRentou != 0 ? mod.BijiaoRentou : 0;
                                    mod.BijiaoRentouKoukuang = mod.BijiaoRentouKoukuang != 0 ? mod.BijiaoRentouKoukuang : 0;
                                    #endregion
                                    seachPatient.DepartmentID = DePartMentID; //小组
                                    var d_sumkeliu = Convert.ToInt32(listkekiu.Where(x => x.DepartmentID == DePartMentID).Sum(x => x.PassengerTraffic));
                                    var d_newkeliu = Convert.ToInt32(istkekiuCustomerService); //店新客流量

                                    #region 超休 6天
                                    mod.GwChaoxiuStr = ("否").ToString();
                                    if (jobid == 3 || jobid == 4 || jobid == 0)
                                    {
                                        if (isExceedRestAdver == "1")
                                        { //如果超休

                                            mod.GwChaoxiuStr = ("是").ToString();
                                            if (mod.Chaoxiuday > 0)
                                            {
                                                if (mod.Chaoxiuday > 6)
                                                {
                                                    dzhospitalglyj = (sumzhs * Convert.ToDecimal((1 - ((mod.Chaoxiuday - 6) * (decimal)1.5 / days) - ((6) * (decimal)1 / days))));
                                                    dzhospitalglyj = dzhospitalglyj > 0 ? dzhospitalglyj : 0;
                                                    //超休只扣现金业绩部分
                                                    dzhospitalallglyj = dzhospitalglyj + sumzhsGw;
                                                    dzhospitalallglyj = dzhospitalallglyj / 2;
                                                }
                                                else
                                                {
                                                    dzhospitalglyj = (sumzhs * (1 - ((mod.Chaoxiuday) * (decimal)1 / days)));
                                                    dzhospitalglyj = dzhospitalglyj > 0 ? dzhospitalglyj : 0;
                                                    dzhospitalallglyj = dzhospitalglyj + sumzhsGw;
                                                    dzhospitalallglyj = dzhospitalallglyj / 2;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region 业绩提成
                                    seachentity.DepartmentID = DePartMentID;
                                    if (dzhospitalallglyj > 0) mod.HospitalRewardALL = Math.Round(dzhospitalallglyj, 2); //Convert.ToDecimal(dzhospitalallglyj);
                                    var yejitc = GetYejiTi1(mod, dzhospitalglyj, dzhospitalallglyj, advererAche, shopownerDJCXJ);

                                    mod.XiangmuYeji = XiangmuYeji;
                                    var yejitcdaxiangmu = GetYejiTiDaxianmu1(mod, dzhospitalglyj, XiangmuYeji, advererAche, shopownerDJCXJ);
                                    mod.MrsshicaoStr = (yejitc + yejitcdaxiangmu).ToString();
                                    var ShouGongFeiModel = result.Where(t => t.DutyName == mod.UserPost && t.UserName == mod.UserName).ToList();
                                    if (ShouGongFeiModel.Count() > 0)
                                    {
                                        mod.ShouGongFei = ShouGongFeiModel[0].Ticheng;
                                        mod.Jiangli = ShouGongFeiModel[0].OtherTiCheng;
                                    }
                                    mod.GwYejiSum = Convert.ToInt32((mod.ShouGongFei + mod.Jiangli + ((decimal)mod.HospitalRewardALL * Convert.ToDecimal(mod.DzHospitalYejiFilter) + (decimal)mod.XiangmuYeji * Convert.ToDecimal(mod.DzDaxiangmuYejiFilter)) / 100)).ToString();

                                    #endregion
                                    #region 奖励&扣罚
                                    if (kel.Count() > 0)
                                    {
                                        var benyuerentou = listkekiu.Where(x => x.DepartmentID == DePartMentID).Sum(x => x.HeadCount);
                                        mod.DzBenyueRentou = benyuerentou;
                                    }
                                    var kelgw = listkekiushanggw;
                                    if (kelgw.Count() > 0)
                                    {
                                        var shangyuerentou = listkekiushanggw.Where(x => x.DepartmentID == DePartMentID).Sum(x => x.HeadCount);
                                        mod.DzShangyueRentou = shangyuerentou;
                                    }
                                    GetBijiaoRentou(entity, mod.DzBenyueRentou, mod.DzShangyueRentou, mod, _deductMoney);

                                    mod.BijiaoRentou = mod.BijiaoRentou != 0 ? mod.BijiaoRentou : 0;
                                    mod.BijiaoRentouKoukuang = mod.BijiaoRentouKoukuang != 0 ? mod.BijiaoRentouKoukuang : 0;


                                    mod.IsQuanqing = "否";
                                    mod.QuanqingRewardf = "未开启";

                                    if (_isFullAttendGW == 1)
                                    {
                                        mod.QuanqingRewardf = "0";
                                        if (mod.ChuqingDay + mod.XiuxiDay == dayCount1)
                                        {
                                            mod.IsQuanqing = "是";
                                            mod.QuanqingReward = Convert.ToInt32(FullAttendGW);
                                            mod.QuanqingRewardf = FullAttendGW.ToString();
                                        }
                                    }


                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                            mod.DzJiangjin = items1[0].EndYear;
                                    }
                                    mod.DzKoufangCoutz = mod.QuanqingReward + mod.BijiaoRentouKoukuang + Convert.ToDecimal(mod.DzJiangjin);
                                    mod.DzKoufangCout = (mod.QuanqingReward + mod.BijiaoRentouKoukuang + Convert.ToDecimal(mod.DzJiangjin)).ToString();

                                    #endregion
                                    #region 业绩奖金
                                    int GZZZ = GetGZZhize1(mod, sumzhs, d_sumkeliu, d_newkeliu, asverBeauDuty1);
                                    var chengshuike = 0;
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                            chengshuike = items1[0].EndMonths;
                                    }
                                    mod.XianJinYeJi = sumzhs.ToString();
                                    mod.JiangliDuty = GZZZ + chengshuike;
                                    mod.ChengSuiKeJiangfang = chengshuike.ToString();
                                    mod.sumkeliu = d_sumkeliu;
                                    mod.newkeliu = d_newkeliu;
                                    mod.JixiaoTicheng = yejitc + yejitcdaxiangmu;//绩效提成

                                    var zhineng = Convert.ToDecimal(mod.GwYejiSum) + Convert.ToDecimal(mod.DixinSalary); // Convert.ToDecimal(mod.MrsshicaoStr) + Convert.ToDecimal(mod.DixinSalary);
                                    mod.IsDixinSalary = "否";
                                    if (_isAchieQT == 1)
                                    {
                                        mod.IsDixinSalary = "是";

                                        zhineng = zhineng * SalaryDay / dayCount;
                                    }
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                        {
                                            var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items.Count() > 0)
                                                mod.Xingzhen = Convert.ToInt32(string.IsNullOrEmpty(items[0].Memo) ? "0" : items[0].Memo);
                                        }
                                    }

                                    mod.Iszhinengkaoping = "否";
                                    if (_isBeauticianCheckQT == 1)
                                    {
                                        mod.Iszhinengkaoping = "是";
                                        if (list1.Count() > 0)
                                        {
                                            var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items1.Count() > 0)
                                            {
                                                var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                                if (items.Count() > 0)
                                                    mod.Fenshu = Convert.ToInt32(string.IsNullOrEmpty(items[0].NumDay) ? "80" : items[0].NumDay);
                                                else
                                                    mod.Fenshu = 80;
                                                if (mod.Fenshu < 80)
                                                    zhineng = Convert.ToInt32((1 - (decimal)(80 - mod.Fenshu) / 100) * zhineng);
                                            }
                                            else
                                            {
                                                mod.Fenshu = 80;
                                            }
                                        }
                                        else
                                        {
                                            mod.Fenshu = 80;
                                        }
                                    }
                                    else
                                    {
                                        mod.Fenshu = 80;
                                    }

                                    mod.ShifaSalary = Convert.ToInt32(zhineng) + Convert.ToInt32(mod.DzKoufangCoutz) + Convert.ToInt32(mod.JiangliDuty) - Convert.ToInt32(mod.Xingzhen);
                                    #endregion
                                    if (jobid == 3)
                                    {
                                        newlist.Add(mod);
                                    }

                                }
                            }
                            if (jobid == 4 || jobid == 0)
                            {
                                #region 获取单条统计
                                //考勤统计
                                mod.UserName = info.UserName;
                                mod.UserPost = info.DutyName;

                                //考勤全勤客流项目奖
                                decimal qqklxmj = 0;
                                decimal quangqingjiang = 0;
                                //全勤
                                var getxiangmu = xiangmulist.Where(x => x.UserID == info.UserID).ToList();
                                var res = result.Where(x => x.UserID == info.UserID).ToList();
                                var regzhiding = resultzhiding.Where(x => x.UserID == info.UserID).ToList();
                                mod.HospitalRewardALL = HospitalZhongheYeji;// sumzh / 2;
                                #endregion
                                if (mod.UserPost == "前台")
                                {
                                    #region 底薪2
                                    var _receptionSal = ("," + receptionSal).Split('}');
                                    var item = _receptionSal;
                                    var sumkeliu = mod1.YouXiaoHuLiKeLiu;
                                    int Isqiantai = 1;//1是 0非
                                    DixinTwo(mod, item, (decimal)sumkeliu, Isqiantai);
                                    var qresult = result.Where(t => t.DutyName == "前台").ToList();
                                    if (qresult.Count() > 0)
                                        mod.ShouGongFei = qresult[0].Ticheng;
                                    mod.MrsDixin2keliu = (decimal)sumkeliu;
                                    mod.DixinSalary = (int)mod.MrsDixin2;

                                    #endregion
                                    #region 业绩提成
                                    var yejitc = GetYejiTi2(mod, HospitalYeji, HospitalDaxiangmuYeji, receptionAche, receptionDJCXJ);
                                    mod.HospitalXianjinYeji = (decimal)HospitalYeji;
                                    mod.Yeji_daxiangmuzhiding = (decimal)HospitalDaxiangmuYeji;

                                    var ShouGongFeiModel = result.Where(t => t.DutyName == mod.UserPost && t.UserName == mod.UserName).ToList();
                                    if (ShouGongFeiModel.Count() > 0)
                                    {
                                        mod.ShouGongFei = ShouGongFeiModel[0].Ticheng;
                                        mod.Jiangli = ShouGongFeiModel[0].OtherTiCheng;
                                    }
                                    mod.MrsshicaoStr = (yejitc + mod.ShouGongFei + mod.Jiangli).ToString();//绩效提成
                                    mod.JixiaoTicheng = yejitc;//业绩资金

                                    mod.IsQuanqing = "否";
                                    mod.QuanqingRewardf = "未开启";

                                    if (_isFullAttendQT == 1)
                                    {
                                        mod.QuanqingRewardf = "0";
                                        if (mod.ChuqingDay + mod.XiuxiDay == dayCount1)
                                        {
                                            mod.IsQuanqing = "是";
                                            mod.QuanqingReward = Convert.ToInt32(FullAttendQT);
                                            mod.QuanqingRewardf = FullAttendQT.ToString();
                                        }
                                    }

                                    if (_isFullAttendQT == 1)
                                    {
                                        quangqingjiang = (decimal)50;
                                        mod.Mrsquangqingjiang = quangqingjiang;
                                    }
                                    if (_isFullAttendQT == 1)
                                    {
                                        quangqingjiang = (decimal)50;
                                        mod.Mrsquangqingjiang = quangqingjiang;
                                    }

                                    var zhineng = Convert.ToInt32(Convert.ToDecimal(mod.MrsshicaoStr)) + Convert.ToInt32(mod.DixinSalary);
                                    mod.IsDixinSalary = "否";
                                    if (_isAchieQT == 1)
                                    {
                                        mod.IsDixinSalary = "是";
                                        zhineng = zhineng * SalaryDay / dayCount;
                                    }
                                    mod.Iszhinengkaoping = "否";
                                    mod.Xingzhen = 0;
                                    if (list1.Count() > 0)
                                    {
                                        var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                        if (items1.Count() > 0)
                                        {
                                            var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items.Count() > 0)
                                                mod.Xingzhen = Convert.ToInt32(string.IsNullOrEmpty(items[0].Memo) ? "0" : items[0].Memo);
                                        }
                                    }
                                    if (_isBeauticianCheckQT == 1)
                                    {
                                        mod.Iszhinengkaoping = "是";
                                        if (list1.Count() > 0)
                                        {
                                            var items1 = list1.Where(x => x.UserId == info.UserID).ToList();
                                            if (items1.Count() > 0)
                                            {
                                                var items = items1.Where(x => x.UserId == info.UserID).ToList();
                                                if (items.Count() > 0)
                                                {
                                                    mod.Fenshu = Convert.ToInt32(string.IsNullOrEmpty(items[0].NumDay) ? "80" : items[0].NumDay);
                                                }
                                                else
                                                {
                                                    mod.Fenshu = 80;
                                                }
                                                if (mod.Fenshu < 80)
                                                    zhineng = Convert.ToInt32((1 - (decimal)(80 - (int)mod.Fenshu) / 100) * Convert.ToInt32(zhineng));
                                            }
                                            else
                                            {
                                                mod.Fenshu = 80;
                                            }
                                        }
                                        else
                                        {
                                            mod.Fenshu = 80;
                                        }
                                    }
                                    else
                                    {
                                        mod.Fenshu = 80;
                                    }

                                    mod.ShifaSalary = Convert.ToInt32(zhineng) + mod.QuanqingReward - mod.Xingzhen;
                                    #endregion

                                    if (jobid == 4)
                                    {
                                        newlist.Add(mod);
                                    }
                                }
                            }
                            if (jobid == 0)
                            {

                                mod.GonglingSubsidy = 0;
                                mod.JiabanSalary = 0;
                                mod.Shibao = 0;
                                mod.Gongjijin = 0;
                                mod.Other = 0;
                                mod.YingfaSalary = mod.DixinSalary + mod.JixiaoReward + mod.JixiaoTicheng
                                                    + mod.QuanqingReward + mod.GonglingSubsidy
                                                    + mod.JiabanSalary
                                                    + Convert.ToInt32(mod.ShouGongFei)
                                                    + mod.Gongjijin - mod.Shibao;
                                mod.ShifaSalary = mod.YingfaSalary - mod.Other - mod.KoukuanDuty;
                                newlist.Add(mod);
                            }

                        }
                    }
                }
                //}
                //catch { }

                return newlist;
            }
        }
        /// <summary>
        /// 美容师薪资参数
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalaryStatisticsFilter(DakaRecordEntity entity)
        {
            List<DakaRecordEntity> newlist = GetSalarySatisticsF(entity, 1);
            return View(newlist);
        }
        /// <summary>
        /// 美容师薪资参数1
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalaryStatisticsFilter1(DakaRecordEntity entity)
        {
            if (entity.HospitalID == 0)
            {
                ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                entity.HospitalID = LoginUserEntity.HospitalID;
                entity.Years = DateTime.Now.Year;
                entity.Months = DateTime.Now.Month;
                //考勤
                ViewBag.HospitalID = entity.HospitalID;
                ViewBag.Years = entity.Years;
                ViewBag.Months = entity.Months;
                List<DakaRecordEntity> newlist = null;
                return View(newlist);
            }
            else
            {

                List<DakaRecordEntity> newlist = GetSalarySatisticsF1(entity, 1);
                return View(newlist);
            }

        }
        /// <summary>
        /// 店长薪资参数记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalaryStatisticsFilterShopManager(DakaRecordEntity entity)
        {
            List<DakaRecordEntity> newlist = GetSalarySatisticsF(entity, 2);
            return View(newlist);
        }
        public ActionResult SalaryStatisticsFilterShopManager1(DakaRecordEntity entity)
        {


            if (entity.HospitalID == 0)
            {
                ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                entity.HospitalID = LoginUserEntity.HospitalID;
                entity.Years = DateTime.Now.Year;
                entity.Months = DateTime.Now.Month;
                //考勤
                ViewBag.HospitalID = entity.HospitalID;
                ViewBag.Years = entity.Years;
                ViewBag.Months = entity.Months;
                List<DakaRecordEntity> newlist = null;
                return View(newlist);
            }
            else
            {

                List<DakaRecordEntity> newlist = GetSalarySatisticsF1(entity, 2);
                return View(newlist);
            }
        }
        /// <summary>
        /// 顾问薪资参数记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalaryStatisticsFilterCounselor(DakaRecordEntity entity)
        {
            List<DakaRecordEntity> newlist = GetSalarySatisticsF(entity, 3);
            return View(newlist);
        }
        public ActionResult SalaryStatisticsFilterCounselor1(DakaRecordEntity entity)
        {
            if (entity.HospitalID == 0)
            {
                ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                entity.HospitalID = LoginUserEntity.HospitalID;
                entity.Years = DateTime.Now.Year;
                entity.Months = DateTime.Now.Month;
                //考勤
                ViewBag.HospitalID = entity.HospitalID;
                ViewBag.Years = entity.Years;
                ViewBag.Months = entity.Months;
                List<DakaRecordEntity> newlist = null;
                return View(newlist);
            }
            else
            {

                List<DakaRecordEntity> newlist = GetSalarySatisticsF1(entity, 3);
                return View(newlist);
            }

        }
        /// <summary>
        /// 前台薪资参数记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalaryStatisticsFilterReceptionist(DakaRecordEntity entity)
        {
            List<DakaRecordEntity> newlist = GetSalarySatisticsF(entity, 4);
            return View(newlist);
        }
        public ActionResult SalaryStatisticsFilterReceptionist1(DakaRecordEntity entity)
        {
            if (entity.HospitalID == 0)
            {
                ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                entity.HospitalID = LoginUserEntity.HospitalID;
                entity.Years = DateTime.Now.Year;
                entity.Months = DateTime.Now.Month;
                //考勤
                ViewBag.HospitalID = entity.HospitalID;
                ViewBag.Years = entity.Years;
                ViewBag.Months = entity.Months;
                List<DakaRecordEntity> newlist = null;
                return View(newlist);
            }
            else
            {

                List<DakaRecordEntity> newlist = GetSalarySatisticsF1(entity, 4);
                return View(newlist);
            }
        }
        /// <summary>
        /// 底薪2
        /// </summary>
        /// <param name="infoo"></param>
        /// <param name="mod"></param>
        /// <param name="item"></param>
        /// <param name="Sumshichao"></param>
        /// <param name="isqiantai"></param>
        private void DixinTwo(DakaRecordEntity mod, string[] item, decimal Sumkeliu, int isqiantai = 0)
        {

            decimal sumkeliu = Sumkeliu;
            decimal Dixin2_customerNum1 = 0;
            decimal BasicsalaryTwo = 0;
            for (int i = 0; i < item.Count() - 1; i++)
            {
                if (isqiantai == 0)
                {
                    Dixin2_customerNum1 = Convert.ToDecimal(item[i].Split(',')[5].Split(':')[1]);//底薪2客流
                    BasicsalaryTwo = Convert.ToDecimal(item[i].Split(',')[7].Split(':')[1]);//底薪2单价
                }
                else
                {
                    Dixin2_customerNum1 = Convert.ToDecimal(item[i].Split(',')[2].Split(':')[1]);//底薪2客流
                    BasicsalaryTwo = Convert.ToDecimal(item[i].Split(',')[4].Split(':')[1]);//底薪2单价
                }
                if ((decimal)sumkeliu > (decimal)Dixin2_customerNum1 || (decimal)sumkeliu == (decimal)Dixin2_customerNum1 || (decimal)sumkeliu == (decimal)0)
                {
                    mod.MrsDixin2_customerNum1 = Dixin2_customerNum1;
                    mod.MrsBasicsalaryTwo = BasicsalaryTwo;
                    if (isqiantai == 0)
                    {

                        if (BasicsalaryTwo > (decimal)99 || item[i].Split(',')[1].Split(':')[1] == "F级")
                        {
                            mod.MrsDixin2 = (decimal)BasicsalaryTwo;
                            break;

                        }
                        else
                        {
                            mod.MrsDixin2 = Convert.ToInt32((decimal)sumkeliu * (decimal)BasicsalaryTwo);
                            break;
                        }
                    }
                    else
                    {
                        mod.MrsDixin2 = Convert.ToInt32(BasicsalaryTwo);
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// 底薪1
        /// </summary>
        /// <param name="infoo"></param>
        /// <param name="mod"></param>
        /// <param name="item"></param>
        /// <param name="Sumshichao"></param>
        private void DixinOne(JoinUserEntity infoo, DakaRecordEntity mod, string[] item, decimal Sumshichao)
        {

            if (Sumshichao > 0)
            {
                var sumshichao = Sumshichao;
                mod.Mrssumshichao = sumshichao;
                for (int i = 0; i < item.Count() - 1; i++)
                {
                    var Dixin1_Shicao1 = Convert.ToInt32(item[i].Split(',')[3].Split(':')[1] == "" ? "0" : item[i].Split(',')[3].Split(':')[1]);//底薪1实操
                    if (sumshichao > (decimal)Dixin1_Shicao1 || sumshichao == (decimal)Dixin1_Shicao1 || sumshichao == (decimal)0)
                    {
                        mod.MrsDixin1_Shicao1 = Dixin1_Shicao1;
                        mod.MrsDixin1 = Convert.ToInt32(item[i].Split(',')[2].Split(':')[1] == ""?"0": item[i].Split(',')[2].Split(':')[1]);//底薪1;
                        //if (mod.UserPost == "店长" ) {
                        //    mod.DzJiangjin = Convert.ToInt32(item[i].Split(',')[8].Split(':')[1] == "" ? "0" : item[i].Split(',')[8].Split(':')[1]);
                        //}
                        break;
                    }
                }
            }
        }

        private int GetGZZhize(decimal HospitalYeji, int d_sumkeliu, int d_newkeliu, string asverBeauDuty)
        {
            var _asverBeauDuty = (asverBeauDuty).Split('}');
            var asveritem = _asverBeauDuty;
            var asveritem_item = asveritem[0].Split(',');
            var asveritem_item_jj = asveritem[1].Split(',');
            var asveritem_item_fk = asveritem[2].Split(',');
            Dictionary<string, string> dcmb = new Dictionary<string, string>();
            dcmb.Add("scope1", HospitalYeji.ToString());//月基础现金
            dcmb.Add("scope2", d_sumkeliu.ToString());//店客流量
            dcmb.Add("scope3", d_newkeliu.ToString());//店新客数量
            //dcmb.Add("scope4", "0");//沉睡客激活
            var yjjj = 0;//业绩资金
            var jzfk = 0;//职责罚扣
            for (int i2 = 0; i2 < dcmb.Count; i2++)
            {
                var ipp = i2 + 1;
                var dcz = Convert.ToDecimal(dcmb["scope" + ipp]); //达成值

                var gmb = Convert.ToDecimal(asveritem_item[i2 * 2 + 1].Split(':')[1]);//低目标

                var dmb = Convert.ToDecimal(asveritem_item[i2 * 2 + 2].Split(':')[1]);//高目标
                //if (i2 == 0)
                //{
                //    gmb = gmb;
                //    dmb = dmb;
                //}
                if (dcz > (decimal)gmb || dcz == (decimal)gmb)
                {
                    var jjz = Convert.ToInt32(asveritem_item_fk[i2 + 2].Split(':')[1]);//资金值
                    yjjj += jjz;
                }
                if (dcz < (decimal)dmb || dcz == (decimal)dmb)
                {
                    var fkz = Convert.ToInt32(asveritem_item_jj[i2 + 2].Split(':')[1]);//罚款值
                    jzfk += fkz;
                }

            }
            return yjjj + jzfk;
        }
        private int GetGZZhize1(DakaRecordEntity mod,decimal HospitalYeji, int d_sumkeliu, int d_newkeliu, string asverBeauDuty)
        {
            var _asverBeauDuty = (asverBeauDuty).Split('}');
            var asveritem = _asverBeauDuty;
            var asveritem_item = asveritem[0].Split(',');
            var asveritem_item_jj = asveritem[1].Split(',');
            var asveritem_item_fk = asveritem[2].Split(',');
            Dictionary<string, string> dcmb = new Dictionary<string, string>();
            dcmb.Add("scope1", HospitalYeji.ToString());//月基础现金
            dcmb.Add("scope2", d_sumkeliu.ToString());//店客流量
            dcmb.Add("scope3", d_newkeliu.ToString());//店新客数量
            //dcmb.Add("scope4", "0");//沉睡客激活
            var yjjj = 0;//业绩资金
            var jzfk = 0;//职责罚扣                
            mod.XianJinYejiJianFang = "0";
            mod.KeliuJiangfang = "0";
            mod.NewKeJiangfang = "0";
            for (int i2 = 0; i2 < dcmb.Count; i2++)
            {
                var ipp = i2 + 1;
                var dcz = Convert.ToDecimal(dcmb["scope" + ipp]); //达成值

                var gmb = Convert.ToDecimal(asveritem_item[i2 * 2 + 1].Split(':')[1]);//低目标

                var dmb = Convert.ToDecimal(asveritem_item[i2 * 2 + 2].Split(':')[1]);//高目标

                //if (dcz > (decimal)gmb || dcz == (decimal)gmb)
                if (dcz > (decimal)dmb || dcz == (decimal)dmb)
                    {
                    var jjz = Convert.ToInt32(asveritem_item_fk[i2 + 2].Split(':')[1]);//资金值
                    if (ipp == 1) {
                        mod.XianJinYejiJianFang = jjz.ToString();
                    }
                    else if(ipp == 2){
                        mod.KeliuJiangfang = jjz.ToString();

                    }
                    else if (ipp == 3){
                        mod.NewKeJiangfang = jjz.ToString();
                    }
                    yjjj += jjz;
                }
                //if (dcz < (decimal)dmb || dcz == (decimal)dmb)
                if (dcz < (decimal)gmb || dcz == (decimal)gmb)
                    {
                    var fkz = Convert.ToInt32(asveritem_item_jj[i2 + 2].Split(':')[1]);//罚款值
                    if (ipp == 1)
                    {
                        mod.XianJinYejiJianFang = fkz.ToString();
                    }
                    else if (ipp == 2)
                    {
                        mod.KeliuJiangfang = fkz.ToString();

                    }
                    else if (ipp == 3)
                    {
                        mod.NewKeJiangfang = fkz.ToString();
                    }
                    jzfk += fkz;
                }

            }
            return yjjj + jzfk;
        }
        /// <summary>
        /// 获取业绩
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="hospitalYeji"></param>
        /// <param name="sumzh"></param>
        /// <param name="shopownerAche"></param>
        /// <param name="_fiterStr"></param>
        /// <returns></returns>
        private int GetYejiTiDaxianmu(DakaRecordEntity mod, decimal hospitalYeji, decimal sumdxm, string shopownerAche, string _fiterStr)
        {

            var HospitalYeji = hospitalYeji;
            var yejitc = 0;
            var _shopownerAche = ("," + shopownerAche).Split('}');
            //var shopowneritem = _shopownerAche;
            var shopowneritem = _shopownerAche[1].Substring(1) + "}";
            shopowneritem = shopowneritem.Replace("name:大项目,合作项目,", "");
            //var shopowneritem = _shopownerAche[0].Split(',')[2];
            Dictionary<string, string> shopownerDJCXJ_item1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(shopowneritem);
            Dictionary<string, string> shopownerDJCXJ_item = JsonConvert.DeserializeObject<Dictionary<string, string>>(_fiterStr);
            for (int i = shopownerDJCXJ_item.Count - 1; i >= 0; i--)
            {
                var yejize = Convert.ToDecimal(shopownerDJCXJ_item["scope" + i]);// * 10000;//业绩总额
                i--;
                if (HospitalYeji > (decimal)yejize || HospitalYeji == (decimal)yejize)
                {
                    mod.MrsHospitalYeji = HospitalYeji;
                    mod.Mrssdyeji = yejize;// / 10000;
                    var zhyj = sumdxm;//店综合业绩
                    var j = i / 2 + 1;
                    mod.DzDaxiangmuYejiFilter = shopownerDJCXJ_item1["scope" + j];
                    yejitc = Convert.ToInt32(zhyj * Convert.ToDecimal(mod.DzDaxiangmuYejiFilter) / 100);//店综合业绩提成; 
                    break;
                }
            }
            return yejitc;
        }
        private int GetYejiTiDaxianmu1(DakaRecordEntity mod, decimal hospitalYeji, decimal sumdxm, string shopownerAche, string _fiterStr)
        {

            var HospitalYeji = hospitalYeji;
            var yejitc = 0;
            var _shopownerAche = ("," + shopownerAche).Split('}');
            //var shopowneritem = _shopownerAche;
            var shopowneritem = _shopownerAche[1].Substring(1) + "}";
            shopowneritem = shopowneritem.Replace("name:大项目,合作项目,", "");
            //var shopowneritem = _shopownerAche[0].Split(',')[2];
            Dictionary<string, string> shopownerDJCXJ_item1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(shopowneritem);
            Dictionary<string, string> shopownerDJCXJ_item = JsonConvert.DeserializeObject<Dictionary<string, string>>(_fiterStr);
            for (int i = shopownerDJCXJ_item.Count - 1; i >= 0; i--)
            {
                var yejize = Convert.ToDecimal(shopownerDJCXJ_item["scope" + i]);// * 10000;//业绩总额
                i--;
                if (HospitalYeji > (decimal)yejize || HospitalYeji == (decimal)yejize)
                {
                    mod.MrsHospitalYeji = HospitalYeji;
                    mod.Mrssdyeji = yejize;// / 10000;
                    var zhyj = sumdxm;//店综合业绩
                    var j = i / 2 + 1;
                    //if (j == 1) 
                    mod.DzDaxiangmuYejiFilter = j == 1 ? shopownerDJCXJ_item1["scope"] : shopownerDJCXJ_item1["scope" + j];
                    yejitc = Convert.ToInt32(zhyj * Convert.ToDecimal(mod.DzDaxiangmuYejiFilter) / 100);//店综合业绩提成; 
                    break;
                }
            }
            return yejitc;
        }
        private int GetYejiTi(DakaRecordEntity mod, decimal hospitalYeji, decimal sumzh, string shopownerAche, string _fiterStr)
        {
            var HospitalYeji = hospitalYeji;
            var yejitc = 0;
            var _shopownerAche = ("," + shopownerAche).Split('}');
            var shopowneritem = _shopownerAche;
            Dictionary<string, string> shopownerDJCXJ_item = JsonConvert.DeserializeObject<Dictionary<string, string>>(_fiterStr);
            for (int i = shopownerDJCXJ_item.Count - 1; i >= 0; i--)
            {
                var yejize = Convert.ToDecimal(shopownerDJCXJ_item["scope" + i]);// * 10000;//业绩总额
                i--;
                if (HospitalYeji > (decimal)yejize || HospitalYeji == (decimal)yejize)
                {
                    mod.MrsHospitalYeji = HospitalYeji;
                    mod.Mrssdyeji = yejize;//  10000;
                    var zhyj = sumzh;//店综合业绩
                    yejitc = Convert.ToInt32(zhyj * Convert.ToDecimal(shopowneritem[0].Split(',')[2].Split(':')[1]) / 100);//店综合业绩提成; 
                    break;
                }
            }
            return yejitc;
        }

        private int GetYejiTi2(DakaRecordEntity mod, decimal hospitalYeji, decimal sumzh, string shopownerAche, string _fiterStr)
        {
            var HospitalYeji = hospitalYeji;
            var yejitc = 0;
            var _shopownerAche = ("," + shopownerAche).Split('}');
            var shopowneritem = _shopownerAche;
            var qtxiangjingf = shopowneritem[0].Substring(1) + "}";
            qtxiangjingf = qtxiangjingf.Replace("name:提成,", "");
            var qtdaxiangmuf = shopowneritem[1].Substring(1) + "}";
            qtdaxiangmuf = qtdaxiangmuf.Replace("name:大项目合作项目业绩提成,", "");
            Dictionary<string, string> shopownerDJCXJ_item = JsonConvert.DeserializeObject<Dictionary<string, string>>(_fiterStr);
            Dictionary<string, string> shopownerDJCXJ_item1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(qtxiangjingf);
            Dictionary<string, string> shopownerDJCXJ_item2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(qtdaxiangmuf);
            for (int i = shopownerDJCXJ_item.Count - 1; i >= 0; i--)
            {
                var yejize = Convert.ToDecimal(shopownerDJCXJ_item["scope" + i]);// * 10000;//业绩总额
                i--;
                if (HospitalYeji > (decimal)yejize || HospitalYeji == (decimal)yejize)
                {
                    mod.MrsHospitalYeji = HospitalYeji;
                    mod.Mrssdyeji = yejize;//  10000;
                    var zhyj = sumzh;//店综合业绩

                    var j = i / 2 + 1; 
                    mod.XianJinYejiJianFang = j == 1 ? shopownerDJCXJ_item1["scope"] :  shopownerDJCXJ_item1["scope" + j];
                    mod.Yeji_daxiangmuzhidingf = j == 1 ? shopownerDJCXJ_item2["scope"]: shopownerDJCXJ_item2["scope" + j];
                    yejitc = Convert.ToInt32(HospitalYeji * Convert.ToDecimal(mod.XianJinYejiJianFang) / 100);//店综合业绩提成; 

                    yejitc += Convert.ToInt32(zhyj * Convert.ToDecimal(mod.Yeji_daxiangmuzhidingf) / 100);//店综合业绩提成; 
                    break;
                }
            }
            return yejitc;
        }
        /// <summary>
        /// 获取业绩
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="hospitalYeji"></param>
        /// <param name="sumzh"></param>
        /// <param name="shopownerAche"></param>
        /// <param name="_fiterStr"></param>
        /// <returns></returns>
        private int GetYejiTi1(DakaRecordEntity mod, decimal hospitalYeji, decimal sumzh, string shopownerAche, string _fiterStr)
        {
            var HospitalYeji = hospitalYeji;
            var yejitc = 0;
            var _shopownerAche = ("," + shopownerAche).Split('}');
            //var shopowneritem = _shopownerAche;
            var shopowneritem = _shopownerAche[0].Substring(1) + "}";
            shopowneritem = shopowneritem.Replace("name:店综合业绩提成,", "");
            //var shopowneritem = _shopownerAche[0].Split(',')[2];
            Dictionary<string, string> shopownerDJCXJ_item1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(shopowneritem);
            Dictionary<string, string> shopownerDJCXJ_item = JsonConvert.DeserializeObject<Dictionary<string, string>>(_fiterStr);
            for (int i = shopownerDJCXJ_item.Count - 1; i >= 0; i--)
            {
                var yejize = Convert.ToDecimal(shopownerDJCXJ_item["scope" + i]);// * 10000;//业绩总额
                i--;
                if (HospitalYeji > (decimal)yejize || HospitalYeji == (decimal)yejize)
                {
                    mod.MrsHospitalYeji = HospitalYeji;
                    mod.Mrssdyeji = yejize;// / 10000;
                    var zhyj = sumzh;//店综合业绩
                    var j = i / 2 + 1;
                    mod.DzHospitalYejiFilter = j == 1 ? shopownerDJCXJ_item1["scope"] : shopownerDJCXJ_item1["scope" + j];
                    yejitc = Convert.ToInt32(zhyj * Convert.ToDecimal(mod.DzHospitalYejiFilter) / 100);//店综合业绩提成; 
                    break;
                }
            }
            return yejitc;
        }

        /// <summary>
        /// 薪资统计
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalaryStatistics(DakaRecordEntity entity)
        {

            List<DakaRecordEntity> newlist = GetSalarySatisticsF(entity);
            return View(newlist);
        }


        /// <summary>
        /// 员工薪资记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalaryDaka(DakaRecordEntity entity, int operareType = 0)
        {

            var mengdianyuangong = personBLL.GetAllUserByHospitalID(entity.HospitalID, 0);
            //     var persoList = reserBLL.GetUserByHospitalIDListMonth(entity).GroupBy(c => c.UserID).Select(g => g.First()).ToList();
            StringBuilder persoListstr1 = new StringBuilder();
            persoListstr1.AppendFormat("<table border='0'>");
            persoListstr1.AppendFormat("<tr style='background-color:#3bafda;height:40px;color:white;'><th style='width:60px'>{0}月考勤</th>", entity.Months);

            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            //获取当前月份的天数
            for (var i = 1; i <= days; i++)
            {
                persoListstr1.AppendFormat("<th style='width: 35px'> {0}日</th>", i);
            }
            persoListstr1.AppendFormat("</tr>");

            foreach (var info in mengdianyuangong)
            {
                //      str.AppendFormat(" <tr><td>{0}</td><td>{1}</td>", info.UserID, info.UserName);
                persoListstr1.AppendFormat("<tr><td style='text-align:center;border-right:1px solid #ccc;width:25px;border-bottom:1px dashed #ccc;height:45px;'>{0}</td>", info.UserName);

                for (var i = 1; i <= days; i++)
                {
                    //   var Dates = entity.Years + "-" + entity.Months + "-" + i;
                    var years = entity.Years;
                    var months = entity.Months;
                    var day = i;
                    var daka = reserBLL.SelectDaka(entity.HospitalID, years, months, day, info.UserID);
                    var qingjia = reserBLL.SelectQingjiaList(entity.HospitalID, years, months, day, info.UserID);
                    int count = 0;
                    if (qingjia.Count > 0)
                    {
                        foreach (var item in qingjia)
                        {
                            count += 1;
                            if (item.leixing == "请假")
                            {
                                if (qingjia.Count != 1)
                                {
                                    if (count == 1)
                                    {
                                        persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>", item.category, item.category);
                                    }
                                }
                                else
                                {
                                    if (item.AfternoonTime == "am")
                                    {
                                        DateTime dt = ConvertValueHelper.ConvertDateTimeValueNew(daka.EndTime);
                                        string hm2 = dt.ToString("HH:mm");
                                        persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px'>{1}</div></td>", item.category, hm2);
                                    }
                                    else if (item.AfternoonTime == "pm")
                                    {
                                        DateTime dd = ConvertValueHelper.ConvertDateTimeValueNew(daka.StartTime);
                                        string hm = dd.ToString("HH:mm");
                                        persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>", hm, item.category);
                                    }
                                }
                            }
                            else if (item.leixing == "休息")
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:green;'>{0}</div><div style='padding-top:5px;color:green;'>{1}</div></td>", "休息", "休息");
                            }
                        }
                    }
                    else
                    {
                        if (daka.Id > 0)
                        {
                            DateTime dd = ConvertValueHelper.ConvertDateTimeValueNew(daka.StartTime);
                            string hm = dd.ToString("HH:mm");
                            DateTime dt = ConvertValueHelper.ConvertDateTimeValueNew(daka.EndTime);
                            string hm2 = dt.ToString("HH:mm");
                            if (hm == "00:00")
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px'>{1}</div></td>", "缺卡", hm2);
                            }
                            else if (hm2 == "00:00")
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>", hm, "缺卡");
                            }
                            else
                            {
                                persoListstr1.AppendFormat("<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px'>{1}</div></td>", hm, hm2);
                            }
                        }
                        else
                        {
                            if (Convert.ToDateTime(entity.Years + "-" + entity.Months + "-" + i) > DateTime.Now)
                            {
                                persoListstr1.AppendFormat(
                                    "<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px'>{0}</div><div style='padding-top:5px'>{1}</div></td>",
                                    "", "");
                            }
                            else
                            {
                                persoListstr1.AppendFormat(
                                    "<td style='text-align: center;border-right:1px solid #ccc;border-bottom:1px dashed #ccc;height:45px;font-size:10px;width:35px;'><div style='border-bottom:1px dashed #ccc; padding-bottom:5px;color:red;'>{0}</div><div style='padding-top:5px;color:red;'>{1}</div></td>",
                                    "缺卡", "缺卡");
                            }
                        }
                    }

                }
                persoListstr1.AppendFormat("</tr>");
            }
            persoListstr1.AppendFormat("</table>");
            if (operareType == 1) //导出操作
            {
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "utf-8";
                Response.ContentType = "application/vnd.ms-excel";
                Response.AppendHeader("content-disposition", "attachment;filename=dakajilu" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls");
                Response.ContentEncoding = Encoding.GetEncoding("utf-8");
                Response.Write(persoListstr1.ToString());
                Response.Flush();
                Response.End();
                return null;
            }
            else
            {
                return Content(persoListstr1.ToString());
            }

        }

    }
}
