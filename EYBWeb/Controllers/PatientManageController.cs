using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PatientManage.Factory.IBLL;
using SystemManage.Factory.IBLL;
using ReservationDoctorManage.Factory.IBLL;
using EYB.ServiceEntity.PatientEntity;
using EYB.ServiceEntity.SystemEntity;
using Webdiyer.WebControls.Mvc;
using Common.Helper;
using HS.SupportComponents;
using PersonnelManage.Factory.IBLL;
using EYB.ServiceEntity.CashEntity;
using CashManage.Factory.IBLL;
using System.Text;
using BaseinfoManage.Factory.IBLL;
using System.Data;
using Common.Attribute;
using System.Configuration;
using System.Globalization;
using System.Data.SqlClient;

namespace EYB.Web.Controllers
{
    public class PatientManageController : BaseController
    {
        #region 依赖注入
        private ISystemManageBLL systemBLL;
        private IPatientManageBLL patientBLL;
        private IReservationDoctorManageBLL reservationBLL;
        private IPersonnelManageBLL personBLL;
        private ICashManageBLL cashBll;
        private IBaseinfoBLL Ibll;
        public PatientManageController()
        {
            systemBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();//基础数据
            patientBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPatientManageBLL>();//顾客
            reservationBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IReservationDoctorManageBLL>();//病历
            personBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            cashBll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<CashManage.Factory.IBLL.ICashManageBLL>();
            Ibll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IBaseinfoBLL>();
        }
        #endregion 依赖注入

        #region==Get请求==
        public ActionResult ProjectEdit()
        {

            var ID = Request["ID"];//这是疗程卡ID
            var strlist = Request["hidstr"];  //这是疗程卡编辑隐藏值
            if (strlist == "0" && ID != null)
            {
                var list = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = Convert.ToInt32(ID) });
                var result = list.AsQueryable().ToPagedList(1, 500000);
                return View(result);
            }
            else if (strlist != "0")
            {
                List<ProjectProductEntity> list = new List<ProjectProductEntity>();
                string[] count = strlist.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                decimal allmoney = 0;
                foreach (var info in count)
                {
                    string[] sinfo = info.Split(new Char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    //汪 begin 2019-10-16 档案变更疗程卡，默认剩余次数等于总次数
                    //allmoney = allmoney + (Convert.ToDecimal(sinfo[2]) * Convert.ToInt32(sinfo[5]));
                    //总价=消耗单价*总次数
                    allmoney = allmoney + (Convert.ToDecimal(sinfo[2]) * Convert.ToInt32(sinfo[1]));
                    //汪 2019-10-16 end
                }
                foreach (var info in count)
                {
                    string[] sinfo = info.Split(new Char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    //27|100|599.00|59900.00|799.00|100
                    //ID| 总次数|消耗单价|剩余总价|单价|剩余次数
                    ProjectProductEntity model = new ProjectProductEntity();
                    model.AllPrice = Convert.ToDecimal(sinfo[3]);
                    model.ID = Convert.ToInt32(ID);
                    model.Price = Convert.ToDecimal(sinfo[4]);
                    model.Qualtity = Convert.ToInt32(sinfo[1]);
                    //汪 begin 2019-10-16 剩余次数等于总次数
                    //model.AutQualtity = Convert.ToInt32(sinfo[5]);
                    //model.AutQualtity = Convert.ToInt32(sinfo[1]);  //原先的
                    #region //罗发明
                    //model.AutQualtity = Convert.ToInt32(sinfo[1]);  //原先的
                    //model.AutQualtity = Convert.ToInt32(sinfo[6]);
                    if (sinfo.Count() > 6)
                    {
                        model.AutQualtity = Convert.ToInt32(sinfo[6]);
                    }
                    else {
                        model.AutQualtity = Convert.ToInt32(sinfo[5]);
                    }
                    #endregion
                    //汪 2019-10-16 end
                    //  model.RealityPrice = Convert.ToInt32(sinfo[4]);
                    model.ConsumptionPrice = Convert.ToDecimal(sinfo[2]);
                    model.ProjectID = Convert.ToInt32(sinfo[0]);
                    model.ProjectName = Ibll.GetProjectModel(Convert.ToInt32(sinfo[0])).Name;
                    model.AllMoney = allmoney;
                    list.Add(model);
                }
                var result = list.AsQueryable().ToPagedList(1, 500000);
                return View(result);
            }
            else
            {
                return View();
            }
        }
        /// <summary>
        /// 顾客信息登记
        /// </summary>
        /// <returns></returns>
        public ActionResult ReservationDetail()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 当月预约
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientMonthReserve()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 当周预约
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientWeeksReserve()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 顾客管理 左侧部分视图 
        /// </summary>
        /// <returns></returns>
        public ActionResult _PartialPatientControl()
        {
            return PartialView();
        }
        public ActionResult _DoctorControl()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }
        public ActionResult ReservationMonth()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        public ActionResult SearchResult(PatientEntity entity)
        {
            if (entity.UserName == "搜索") entity.UserName = "";
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 20; //每页显示条数
            entity.s_StareTime = Convert.ToDateTime("1970-01-01");
            entity.s_Endtime = Convert.ToDateTime("1970-01-01");
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            if (entity.IsActive == 0)
            {
                entity.IsActive = 1;
            }
            var list = patientBLL.GetPatientList(entity, out rows, out pagecount);
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
        public ActionResult _SearchResultList()
        {
            return View();
        }
        /// <summary>
        /// 顾客登记
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientRegister()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.ProvinceCode = LoginUserEntity.ProvinceCode;
            return View();
        }
        /// <summary>
        /// 顾客登记 弹出框
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientRegisterDialog()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }
        /// <summary>
        /// 顾客信息管理
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientInfoManage(PatientEntity entity)
        {
            // ServiceEntity.PersonnelEntity.HospitalUserEntity loginUserInfo =(ServiceEntity.PersonnelEntity.HospitalUserEntity)Session["userInfo"];
            //ServiceEntity.PersonnelEntity.HospitalUserEntity loginUserInfo = Session["userInfo"] as ServiceEntity.PersonnelEntity.HospitalUserEntity;
            // var nameInfo = Session["userInfoquanxian"];
            int one = 0;
            //if (nameInfo != null)
            //{
            //    var usersmodelName = systemBLL.GetUserSettingsModelMemo(nameInfo.ToString(), LoginUserEntity.HospitalID);
            //    if (usersmodelName.ID > 0)
            //    {
            //        if (usersmodelName.AttributeValue == "1")
            //        {
            //            one = 1;

            //        }

            //    }
            //}
            // ServiceEntity.PersonnelEntity.HospitalUserEntity user = (ServiceEntity.PersonnelEntity.HospitalUserEntity)Session.get("userInfo");
            var Url = Request.Url.AbsoluteUri;
            if (entity.UserName == "姓名、电话、会员卡号") entity.UserName = "";
            if (!String.IsNullOrEmpty(entity.UserName))
            {
                entity.UserName = entity.UserName.ToUpper();
            }

            int rows;
            int pagecount;
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.IsActive == 0)
            {
                entity.IsActive = 1;
            }
            if (entity.HospitalID == LoginUserEntity.ParentHospitalID)
            {
                entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            }
            if (Request.IsAjaxRequest())
            {

                if (entity.HospitalID == 0)
                {
                    entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                }
                //else
                //{
                //    entity.ParentHospitalID = 0;
                //}
            }
            if (entity.IsSelectTime == 0)
            {
                entity.StartTime = null;
                entity.EndTime = null;
            }
            //三个月内到店一次
            if (entity.LiushiKe == 1)
            {
                entity.s_StareTime = DateTime.Now.AddMonths(-3);
                entity.s_Endtime = DateTime.Now;
            }
            else if (entity.LiushiKe == 2)//3-6个月到店一次
            {
                entity.s_StareTime = DateTime.Now.AddMonths(-6);
                entity.s_Endtime = DateTime.Now.AddMonths(-3);
            }
            else if (entity.LiushiKe == 3)//6个月以上未到店
            {
                entity.s_StareTime = Convert.ToDateTime("1990-01-01");
                entity.s_Endtime = DateTime.Now.AddMonths(-6);
            }
            else
            {
                entity.s_StareTime = Convert.ToDateTime("1970-01-01");
                entity.s_Endtime = Convert.ToDateTime("1970-01-01");
            }
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            bool hasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<PatientEntity> list = new List<PatientEntity>();
            if (hasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = patientBLL.GetPatientList(entity, out rows, out pagecount);
                list = list.Where(c => c.IsShow != 2).ToList(); //2就是要隐藏
                
            }
            else
            {
                list = patientBLL.GetPatientList(entity, out rows, out pagecount);
                list = list.Where(t => t.IsDel!=1).ToList();//1就是隐藏
                if (entity.LiushiKe != 0)
                {
                    list = list.Where(t => t.ArchivesType != "体验客").ToList();
                }

            }
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            if (hasPermission1)//模拟账号
            {
                result.TotalItemCount = list.Count;
            }
            else
            {
                result.TotalItemCount = rows;
            }
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.UerEntity = entity;
            ViewBag.TotalItemCount = list.Count;
            ViewBag.one = one;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_Patientlist", result);
            }
            else
            {

                return View(result);
            }
        }

        /// <summary>
        /// 顾客未消耗
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientCardNoxiaohao(PatientEntity entity)
        {
          
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.IsActive == 0)
            {
                entity.IsActive = 1;
            }
            if (entity.HospitalID == LoginUserEntity.ParentHospitalID)
            {
                entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            }

            if (Request.IsAjaxRequest())
            {
                if (entity.HospitalID == 0)
                {
                    entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                }
            }
            /*entity.numPerPage = 50; *///每页显示条数
            entity.numPerPage = 700;
            if (entity.pageNum == 0)
                entity.pageNum = 1;

            var UserName = Request["UserName"];
            var UserID = Request["UserID"];
            var orderField = entity.orderField;
            if (orderField == null)
            {
                orderField = "Sum_Price,UserId";
            }
            if (orderField.IndexOf("UserId") < 0)
            {
                orderField += ",UserId";
            }
            var orderDirection = entity.orderDirection;
            if (orderDirection == null)
            {
                orderDirection = "desc";
            }
            int rows;
            int pagecount;
            int ZongSum_AllTimes;
            decimal ZongRetailPrice;
            decimal ZongSum_Price;
            decimal ZongSum_AllPrice;
            int ZongNumber;
            int ZongSum_Tiems;
            var list = patientBLL.StatisticsPatientNotExpenseNew(entity.HospitalID, UserName, orderField, orderDirection, entity.numPerPage, entity.pageNum, out rows, out pagecount, out ZongSum_AllTimes, out ZongRetailPrice, out ZongSum_Price, out ZongSum_AllPrice, out ZongNumber, out ZongSum_Tiems,entity.FollowUpMrUserID);
            // var list1 = patientBLL.StatisticsPatientNotExpense(entity.HospitalID, UserName, orderField, orderDirection, entity.numPerPage, entity.pageNum, out rows, out pagecount, out ZongSum_AllTimes, out ZongRetailPrice, out ZongSum_Price, out ZongSum_AllPrice, out ZongNumber, out ZongSum_Tiems);
            //汪 2019-10-23 begin 屏蔽一些顾客未消耗统计 
            // var result= list;
            // var result = list.Where(c => c.AllTiems > 0 || c.Sum_Price > 0 || c.Sum_AllPrice > 0).ToPagedList(1, entity.numPerPage);  
            //result.TotalItemCount = rows;
            //list = list.Where(x =>x.BuyCardTime >);
            if (entity.StartTime != null && entity.EndTime != null) {
                
                list = list.Where(x => Convert.ToDateTime(x.RuhuiTime) > Convert.ToDateTime(entity.StartTime) && Convert.ToDateTime(x.RuhuiTime) < Convert.ToDateTime(entity.EndTime).AddDays(1)).ToList();
            }
            var result = list.ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = result.Count();
            //汪 end
            ViewBag.ZongSum_AllTimes = ZongSum_AllTimes;
            ViewBag.ZongRetailPrice = ZongRetailPrice;
            ViewBag.ZongSum_Price = ZongSum_Price;
            ViewBag.ZongSum_AllPrice = ZongSum_AllPrice;
            ViewBag.ZongNumber = ZongNumber;
            ViewBag.ZongSum_Tiems = ZongSum_Tiems;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PatientCardNoxiaohao", result);
            }
            else
            {
                return View(result);
            }
        }
        /// <summary>
        /// --顾客详情
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientDetail()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// --顾客详情  弹出框
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientDetailControl()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 指定美容师
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonnelList()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 办卡页面
        /// </summary>
        /// <returns></returns>
        public ActionResult MakeCardNo()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 办卡页面
        /// </summary>
        /// <returns></returns>
        public ActionResult MakeCardNoSet()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 变更日志
        /// </summary>
        /// <returns></returns>
        public ActionResult ArchivesChangeLog(ArchivesChangeLogEntity entity)
        {
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.StareTime = entity.StareTime.Year < 1990 ? DateTime.Now.AddMonths(-1) : entity.StareTime;
            entity.Endtime = entity.Endtime.Year < 1990 ? DateTime.Now : entity.Endtime;
            entity.StareTime = Convert.ToDateTime(entity.StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.Endtime = Convert.ToDateTime(entity.Endtime.ToString("yyyy-MM-dd 23:59:59"));
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            int rows;
            int pagecount;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            var list = patientBLL.GetArchivesChangeLogList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_ArchivesChangeLog", result);
            }
            else
            {

                return View(result);
            }
        }

        /// <summary>
        /// 变更日志
        /// </summary>
        /// <returns></returns>
        public ActionResult ArchivesChangeLog1(ArchivesChangeLogEntity entity)
        {
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.StareTime = entity.StareTime.Year < 1990 ? DateTime.Now.AddMonths(-1) : entity.StareTime;
            entity.Endtime = entity.Endtime.Year < 1990 ? DateTime.Now : entity.Endtime;
            entity.StareTime = Convert.ToDateTime(entity.StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.Endtime = Convert.ToDateTime(entity.Endtime.ToString("yyyy-MM-dd 23:59:59"));
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            int rows;
            int pagecount;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            var list = patientBLL.GetArchivesChangeLogList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_ArchivesChangeLog1", result);
            }
            else
            {

                return View(result);
            }
        }
        /// <summary>
        /// 用户余额改变日志
        /// </summary>
        /// <param name="ae"></param>
        /// <returns></returns>
        public ActionResult BlanceLog(ArchivesChangeLogEntity ae)
        {

            var LogType = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["LogType"]);
            ae.LogType = 0;
            ae.UserID = 0;
            ae.StareTime = DateTime.Now.AddYears(-20);
            ae.Endtime = DateTime.Now.AddDays(1);
            int row, pagecount = 0;
            //var UserID = Request["UserID"];
            //var ID = Request["CourseCardIDList"];
            //var _type = Request["LogType"];  

            ae.numPerPage = 50; //每页显示条数  
            if (ae.orderField + "" == "") { ae.orderField = "CreateTime"; ae.orderDirection = "asc"; }
            var list = patientBLL.GetArchivesChangeLogList(ae, out row, out pagecount);
            var list1 = list.Where(i => i.ChangeType == "Add").ToList();
            var list2 = list.Where(i => i.ChangeType == "Delete").ToList();
            list = new List<ArchivesChangeLogEntity>();
            foreach (var info1 in list1)
            {
                if (info1.OrderType != 2)
                {
                    info1.ConsumptionUnitPrice = -info1.ConsumptionUnitPrice;
                }
                else {
                    info1.ConsumptionUnitPrice = info1.ConsumptionUnitPrice;
                }
                list.Add(info1);
            }
            foreach (var info2 in list2)
            {
                //info2.ConsumptionUnitPrice = -info2.ConsumptionUnitPrice;
                //疗程卡应为次数相减，而不是之前的减去消耗单价 
                info2.ConsumptionUnitPrice = -1 * info2.Quantity;
                list.Add(info2);
            }

            if (LogType == 1)
            {
                ViewBag.SumNum = list.Sum(i => i.ConsumptionUnitPrice);
            }
            else
            {
                ViewBag.SumNum = (int)list.Sum(i => i.ConsumptionUnitPrice);
            }
            var result = list.OrderBy(c => c.CreateTime).AsQueryable().ToPagedList(1, ae.numPerPage);
            return View(result);
        }
        public ActionResult CardBalanceRechargeDetail(MainCardBalanceDetailEntity entity)
        {
            if (string.IsNullOrWhiteSpace(entity.orderField))
            {
                entity.orderField = "CreateTime";
                entity.orderDirection = "desc";
            }
            entity.numPerPage = 15;
            entity.Type = (int)MainCardBalanceDetailType.充值;
            var list = cashBll.PageListMainCardBalanceDetail(entity);

            var result = list.ToPagedList(1, entity.numPerPage);
            return View(result);
        }
        public ActionResult CardBalanceDetail(MainCardBalanceDetailEntity entity)
        {
            if (string.IsNullOrWhiteSpace(entity.orderField))
            {
                entity.orderField = "CreateTime";
                entity.orderDirection = "desc";
            }

            entity.numPerPage = 15;
            var list = cashBll.PageListMainCardBalanceDetail(entity);

            var result = list.ToPagedList(1, entity.numPerPage);
            return View(result);
        }
        #endregion
        #region==Ajax请求==

        #region==顾客管理
        /// <summary>
        /// 设置顾客隐藏(应付模拟账号检查  设置模拟账号隐藏的顾客)
        /// </summary>
        /// <returns></returns>
        public ActionResult SetCustomerList(List<int> checkbox)
        {
            int result = patientBLL.BatchSetIsShow(checkbox, 2);
            return Json(result);
        }

        /// <summary>
        /// 设置顾客美容师&顾问
        /// </summary>
        /// <returns></returns>
        public ActionResult SetCustomerMRSList(List<int> checkbox,int BGFollowUpMrUserID)
        {
            int result =  patientBLL.BatchUpdateFollowUpMrUser(checkbox, BGFollowUpMrUserID);
            return Json(result);
        }

        /// <summary>
        /// 设置顾客美容师&顾问
        /// </summary>
        /// <returns></returns>
        public ActionResult SetCustomerGWList(List<int> checkbox, int BGFollowUpUserID)
        {
            int result = patientBLL.BatchUpdateFollowUpUser(checkbox, BGFollowUpUserID);
            return Json(result);
        }
        
        /// <summary>
        /// 伪删除顾客
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="IsActive"></param>
        /// <returns></returns>
        public ActionResult BatchUpdatePatientIsActive(int UserID, int IsActive)
        {
            int result = patientBLL.BatchUpdatePatientIsActive(UserID, IsActive);
            return Json(result);
        }

        /// <summary>
        /// 添加/修改顾客信息
        /// </summary>
        /// <param name="PE"></param>
        /// <returns></returns>      
        public ActionResult AddUpPatient(PatientEntity PE)
        {
            if (string.IsNullOrEmpty(PE.Calendar))
            {
                PE.Calendar = "公历";
            }
            else
            {
                PE.Calendar = "农历";
            }

    DateTime dtBirthday;
    ConvertValueHelper.ConvertDateTimeValue(PE.Birthday, out dtBirthday);
            PE.Birthday = dtBirthday.ToString("yyyy-MM-dd");

            //if (PE.Birthday == "0" || string.IsNullOrEmpty(PE.Birthday))
            //{
            //    PE.Birthday = "1991-01-01";
            //}
            long result = 0;
    PE.Namepingyin = Utils.GetAllPYLetters(PE.UserName);
            if (PE.UserID == 0)//添加
            {
                PE.HospitalID = LoginUserEntity.HospitalID;
                //PE.Birthday = Request.Form["Y"] + "-" + Request.Form["M"] + "-" + Request.Form["D"];
                PE.Address = Request.Form["S"] + "-" + Request.Form["Q"] + "-" + Request.Form["X"] + "-" + Request.Form["AD"];
                PE.CreateTime = DateTime.Now;
                PE.IsActive = 1;
                PE.ArchivesType = String.IsNullOrEmpty(PE.ArchivesType)? "非正式会员" : PE.ArchivesType;//默认为非正式会员
                PE.ReservationTime = DateTime.Now;//时间为当天
                PE.Password = "123456";
                PE.Points = PE.Points;

                if (LoginUserEntity.HospitalID == 2||LoginUserEntity.HospitalID==1017)
                {
                    if (PE.JieshaoUserID>0&& PE.JieshaoUserID!=null)
                    {
                        if (PE.Channel=="转介绍")
                        {
                            var usersetting = systemBLL.GetUserSettingsValue(LoginUserEntity.HospitalID, "ZhuanjieshaosongjiFen"); //获取积分实体
    var intrgral = patientBLL.GetIntegralModel(PE.JieshaoUserID);//获取原来的积分列表
    intrgral.AllIntegral += Convert.ToInt32(usersetting.AttributeValue);
                            intrgral.Integral += Convert.ToInt32(usersetting.AttributeValue);
                            int count = patientBLL.UpdateIntegral(intrgral);
    IntegralOperationEntity entity = new IntegralOperationEntity();
    entity.Memo = "转介绍：" + PE.JieshaoUserName + "" + " 客户名字: " + PE.UserName + "";
                            entity.Number = Convert.ToDecimal(usersetting.AttributeValue);
                            entity.UserID = PE.JieshaoUserID;
                            entity.OperationType = 2;
                            var id = patientBLL.AddIntegralOperation(entity);
    IntegralRecordEntity ir1 = new IntegralRecordEntity();
    ir1.UserID = PE.JieshaoUserID;
                            // ir1.DocumentNumber = ID.ToString();
                            ir1.Integral = Convert.ToDecimal(usersetting.AttributeValue);
                            ir1.CreateTime = DateTime.Now;
                        //    ir1.IntegralOperationID = LoginUserEntity.HospitalID;
                            ir1.OrderNO = "转介绍";
                            ir1.DocumentNumber = "转介绍";
                            ir1.IntegralOperationID = id;
                            patientBLL.AddIntegralRecord(ir1);//增加积分记录
                        }
                    }
                }
                result = patientBLL.AddPatient(PE);
                IntegralEntity ir = new IntegralEntity();
ir.UserID = (int) result;
ir.Integral = 0;
                ir.HospitalID = LoginUserEntity.HospitalID;
                ir.CreateTime = DateTime.Now;

                patientBLL.AddIntegralUser(ir);
                //顾客日志
                if (!String.IsNullOrEmpty(PE.ArchivesType))
                {
                    patientBLL.AddPatientArchivesType(new PatientArchivesTypeEntity { UserID = ir.UserID, ArchivesType = PE.ArchivesType, CreateTime = DateTime.Now, CreateUserName = LoginUserEntity.UserName });
                }
            }
            else // 修改
            {
                //PE.Birthday = Request.Form["Y"] + "-" + Request.Form["M"] + "-" + Request.Form["D"];
                PE.Address = Request.Form["S"] + "-" + Request.Form["Q"] + "-" + Request.Form["X"] + "-" + Request.Form["AD"];
                patientBLL.UpPatient(PE);
                result = PE.UserID;
            }
            //编辑详情

            patientBLL.AddPatientDetail(new PatientDetailEntity { UserID = (int) result, DoProject = PE.DoProject, DoProduct = PE.DoProduct, XiaofeiSum = PE.XiaofeiSum, Payment = Request.Form["Payment"], ZhiYe = PE.ZhiYe, ZhiWu = PE.ZhiWu, PersonShouru = PE.PersonShouru, HunyinZhuangkuang = PE.HunyinZhuangkuang, PeiouZhiYe = PE.PeiouZhiYe, JiatingShouru = PE.JiatingShouru, ShijiaChe = PE.ShijiaChe, Zhufang = PE.Zhufang, OtherShouru = PE.OtherShouru, Xiaofeiyishi = Request.Form["Xiaofeiyishi"], XiaofeiWaiyin = Request.Form["XiaofeiWaiyin"], Xiaofeiqingxiang = Request.Form["Xiaofeiqingxiang"], XiaofeiLeixing = Request.Form["XiaofeiLeixing"], Xuexing = PE.Xuexing, Xingzuo = PE.Xingzuo, Shuxiang = PE.Shuxiang, Xingge = Request.Form["Xingge"], XiGuanDaoDianTime = PE.XiGuanDaoDianTime, XiGuanDaoDianDay = PE.XiGuanDaoDianDay, YueDaoDianTimes = PE.YueDaoDianTimes, Hulixiguan = Request.Form["Hulixiguan"], Shengliqi = PE.Shengliqi, Shuimianxiguan = PE.Shuimianxiguan, Yundongxiguan = PE.Yundongxiguan, Yinshixiguan = Request.Form["Yinshixiguan"], Othershenghuo = Request.Form["Othershenghuo"] });
            return Json(result);
        }

        //天干 
        private static string[] TianGan = { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸" };

        //地支 
        private static string[] DiZhi = { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };

        //十二生肖 
        private static string[] ShengXiao = { "鼠", "牛", "虎", "兔", "龙", "蛇", "马", "羊", "猴", "鸡", "狗", "猪" };

        //农历日期 
        private static string[] DayName = {"*","初一","初二","初三","初四","初五",
                                                 "初六","初七","初八","初九","初十",
                                                 "十一","十二","十三","十四","十五",
                                                 "十六","十七","十八","十九","二十",
                                                 "廿一","廿二","廿三","廿四","廿五",
                                                 "廿六","廿七","廿八","廿九","三十"};

        //农历月份 
        private static string[] MonthName = { "*", "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "腊" };

        //公历月计数天 
        private static int[] MonthAdd = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
        //农历数据 
        private static int[] LunarData = {2635,333387,1701,1748,267701,694,2391,133423,1175,396438
                                             ,3402,3749,331177,1453,694,201326,2350,465197,3221,3402
                                             ,400202,2901,1386,267611,605,2349,137515,2709,464533,1738
                                             ,2901,330421,1242,2651,199255,1323,529706,3733,1706,398762
                                             ,2741,1206,267438,2647,1318,204070,3477,461653,1386,2413
                                             ,330077,1197,2637,268877,3365,531109,2900,2922,398042,2395
                                             ,1179,267415,2635,661067,1701,1748,398772,2742,2391,330031
                                             ,1175,1611,200010,3749,527717,1452,2742,332397,2350,3222
                                             ,268949,3402,3493,133973,1386,464219,605,2349,334123,2709
                                             ,2890,267946,2773,592565,1210,2651,395863,1323,2707,265877};
        /// <summary> 
        /// 获取对应日期的农历 
        /// </summary> 
        /// <param name="dtDay">公历日期</param> 
        /// <returns></returns> 
        public string GetLunarCalendar(DateTime dtDay)
        {
            string sYear = dtDay.Year.ToString();
            string sMonth = dtDay.Month.ToString();
            string sDay = dtDay.Day.ToString();
            int year;
            int month;
            int day;
            try
            {
                year = int.Parse(sYear);
                month = int.Parse(sMonth);
                day = int.Parse(sDay);
            }
            catch
            {
                year = DateTime.Now.Year;
                month = DateTime.Now.Month;
                day = DateTime.Now.Day;
            }

            int nTheDate;
            int nIsEnd;
            int k, m, n, nBit, i;
            string calendar = string.Empty;
            //计算到初始时间1921年2月8日的天数：1921-2-8(正月初一) 
            nTheDate = (year - 1921) * 365 + (year - 1921) / 4 + day + MonthAdd[month - 1] - 38;
            if ((year % 4 == 0) && (month > 2))
                nTheDate += 1;
            //计算天干，地支，月，日 
            nIsEnd = 0;
            m = 0;
            k = 0;
            n = 0;
            while (nIsEnd != 1)
            {
                if (LunarData[m] < 4095)
                    k = 11;
                else
                    k = 12;
                n = k;
                while (n >= 0)
                {
                    //获取LunarData[m]的第n个二进制位的值 
                    nBit = LunarData[m];
                    for (i = 1; i < n + 1; i++)
                        nBit = nBit / 2;
                    nBit = nBit % 2;
                    if (nTheDate <= (29 + nBit))
                    {
                        nIsEnd = 1;
                        break;
                    }
                    nTheDate = nTheDate - 29 - nBit;
                    n = n - 1;
                }
                if (nIsEnd == 1)
                    break;
                m = m + 1;
            }
            year = 1921 + m;
            month = k - n + 1;
            day = nTheDate;
            //return year + "-" + month + "-" + day;

            if (k == 12)
            {
                if (month == LunarData[m] / 65536 + 1)
                    month = 1 - month;
                else if (month > LunarData[m] / 65536 + 1)
                    month = month - 1;
            }
            //年
            calendar = year + "年";
            //生肖 
            calendar += ShengXiao[(year - 4) % 60 % 12].ToString() + "年 ";
            // //天干 
            calendar += TianGan[(year - 4) % 60 % 10].ToString();
            // //地支 
            calendar += DiZhi[(year - 4) % 60 % 12].ToString() + " ";

            //农历月 
            if (month < 1)
                calendar += "闰" + MonthName[-1 * month].ToString() + "月";
            else
                calendar += MonthName[month].ToString() + "月";

            //农历日 
            calendar += DayName[day].ToString() + "日";

            return calendar;

        }
        public ActionResult AddUpPatient2(PatientEntity PE)
        {
            if (string.IsNullOrEmpty(PE.Calendar))
            {
                PE.Calendar = "公历";
            }
            else {
                
                var Nonli = GetLunarCalendar(Convert.ToDateTime(PE.Birthday));
                ChineseLunisolarCalendar cls = new ChineseLunisolarCalendar();
                if (string.IsNullOrEmpty(PE.Birthday)) PE.Birthday = "1991-01-01 00:00:00";
                DateTime dt = cls.ToDateTime(Convert.ToDateTime(PE.Birthday).Year, Convert.ToDateTime(PE.Birthday).Month, Convert.ToDateTime(PE.Birthday).Day, 0, 0, 0, 0);
                DateTime dtBirthday = dt;
                //ConvertValueHelper.ConvertDateTimeValue(PE.Birthday, out dtBirthday);
                PE.Birthday = dtBirthday.ToString("yyyy-MM-dd");
            }
            if ( PE.Birthday == "0"|| string.IsNullOrEmpty(PE.Birthday)) {
                PE.Birthday = "1991-01-01";
            }
            long result = 0;
            PE.Namepingyin = Utils.GetAllPYLetters(PE.UserName);
            if (PE.UserID == 0)//添加
            {
                PE.HospitalID = LoginUserEntity.HospitalID;
                //PE.Birthday = Request.Form["Y"] + "-" + Request.Form["M"] + "-" + Request.Form["D"];
                PE.Address = Request.Form["S"] + "-" + Request.Form["Q"] + "-" + Request.Form["X"] + "-" + Request.Form["AD"];
                PE.CreateTime = DateTime.Now;
                PE.IsActive = 1;
                PE.ArchivesType = String.IsNullOrEmpty(PE.ArchivesType) ? "非正式会员" : PE.ArchivesType;//默认为非正式会员
                PE.ReservationTime = DateTime.Now;//时间为当天
                PE.Password = "123456";
                PE.Points = PE.Points;

                if (LoginUserEntity.HospitalID == 2||LoginUserEntity.HospitalID==1017)
                {
                    if (PE.JieshaoUserID>0&& PE.JieshaoUserID!=null)
                    {
                        if (PE.Channel=="转介绍")
                        {
                            var usersetting = systemBLL.GetUserSettingsValue(LoginUserEntity.HospitalID, "ZhuanjieshaosongjiFen"); //获取积分实体
                            var intrgral = patientBLL.GetIntegralModel(PE.JieshaoUserID);//获取原来的积分列表
                            intrgral.AllIntegral += Convert.ToInt32(usersetting.AttributeValue);
                            intrgral.Integral += Convert.ToInt32(usersetting.AttributeValue);
                            int count = patientBLL.UpdateIntegral(intrgral);
                            IntegralOperationEntity entity = new IntegralOperationEntity();
                            entity.Memo = "转介绍：" + PE.JieshaoUserName + "" + " 客户名字: " + PE.UserName + "";
                            entity.Number = Convert.ToDecimal(usersetting.AttributeValue);
                            entity.UserID = PE.JieshaoUserID;
                            entity.OperationType = 2;
                            var id = patientBLL.AddIntegralOperation(entity);
                            IntegralRecordEntity ir1 = new IntegralRecordEntity();
                            ir1.UserID = PE.JieshaoUserID;
                            // ir1.DocumentNumber = ID.ToString();
                            ir1.Integral = Convert.ToDecimal(usersetting.AttributeValue);
                            ir1.CreateTime = DateTime.Now;
                        //    ir1.IntegralOperationID = LoginUserEntity.HospitalID;
                            ir1.OrderNO = "转介绍";
                            ir1.DocumentNumber = "转介绍";
                            ir1.IntegralOperationID = id;
                            patientBLL.AddIntegralRecord(ir1);//增加积分记录
                        }
                    }
                }
                result = patientBLL.AddPatient(PE);
                IntegralEntity ir = new IntegralEntity();
                ir.UserID = (int)result;
                ir.Integral = 0;
                ir.HospitalID = LoginUserEntity.HospitalID;
                ir.CreateTime = DateTime.Now;

                patientBLL.AddIntegralUser(ir);
                //顾客日志
                if (!String.IsNullOrEmpty(PE.ArchivesType))
                {
                    patientBLL.AddPatientArchivesType(new PatientArchivesTypeEntity { UserID = ir.UserID, ArchivesType = PE.ArchivesType, CreateTime = DateTime.Now, CreateUserName = LoginUserEntity.UserName });
                }
            }
            else // 修改
            {
                //PE.Birthday = Request.Form["Y"] + "-" + Request.Form["M"] + "-" + Request.Form["D"];
                PE.Address = Request.Form["S"] + "-" + Request.Form["Q"] + "-" + Request.Form["X"] + "-" + Request.Form["AD"];
                patientBLL.UpPatient(PE);
                result = PE.UserID;
            }
            //编辑详情

            patientBLL.AddPatientDetail(new PatientDetailEntity { UserID = (int)result, DoProject = PE.DoProject, DoProduct = PE.DoProduct, XiaofeiSum = PE.XiaofeiSum, Payment = Request.Form["Payment"], ZhiYe = PE.ZhiYe, ZhiWu = PE.ZhiWu, PersonShouru = PE.PersonShouru, HunyinZhuangkuang = PE.HunyinZhuangkuang, PeiouZhiYe = PE.PeiouZhiYe, JiatingShouru = PE.JiatingShouru, ShijiaChe = PE.ShijiaChe, Zhufang = PE.Zhufang, OtherShouru = PE.OtherShouru, Xiaofeiyishi = Request.Form["Xiaofeiyishi"], XiaofeiWaiyin = Request.Form["XiaofeiWaiyin"], Xiaofeiqingxiang = Request.Form["Xiaofeiqingxiang"], XiaofeiLeixing = Request.Form["XiaofeiLeixing"], Xuexing = PE.Xuexing, Xingzuo = PE.Xingzuo, Shuxiang = PE.Shuxiang, Xingge = Request.Form["Xingge"], XiGuanDaoDianTime = PE.XiGuanDaoDianTime, XiGuanDaoDianDay = PE.XiGuanDaoDianDay, YueDaoDianTimes = PE.YueDaoDianTimes, Hulixiguan = Request.Form["Hulixiguan"], Shengliqi = PE.Shengliqi, Shuimianxiguan = PE.Shuimianxiguan, Yundongxiguan = PE.Yundongxiguan, Yinshixiguan = Request.Form["Yinshixiguan"], Othershenghuo = Request.Form["Othershenghuo"] });
            return Json(result);
        }
        /// <summary>
        /// 顾客图片上传
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadPatientImg(int UserID)
        {
            int result = 0;
            result = patientBLL.DelPatientImg(UserID);
            string[] albumArr = Request.Form.GetValues("hid_photo_name");
            string[] hid_photo_remark = Request.Form.GetValues("hid_photo_remark");
            if (albumArr != null && albumArr.Length > 0)
            {
                for (int i = 0; i < albumArr.Length; i++)
                {
                    string[] imgArr = albumArr[i].Split('|');
                    if (imgArr.Length == 3)
                    {
                        result = patientBLL.AddPatientImg(new PatientImgEntity { ImgUrl = imgArr[1], UserID = UserID, Memo = hid_photo_remark[i], CreateTime = DateTime.Now });
                    }
                }
            }
            return Json(result);
        }
        //获取二级城市
        [HttpPost]
        public ActionResult GetAreaByParentID(int id)
        {
            var list = systemBLL.GetAllAreaList("").Where(x => x.ParentAreaCode == id).ToList();
            return Json(list);
        }
        /// <summary>
        /// 会员级别变更
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientArchivesType(int UserID)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var list = patientBLL.GetPatientArchivesTypeList(new PatientArchivesTypeEntity { UserID = UserID });
            return View(list);
        }
        /// <summary>
        /// 设置会员级别
        /// </summary>
        /// <returns></returns>
        public ActionResult SetPatientArchivesType(int UserID, string ArchivesType)
        {
            //判断如果是设置为正式会员
            bool isset = false;
            if (ArchivesType == "正式会员")
            {
                //如果前面没有设置过正式会员，则修改用户转正时间
                var list = patientBLL.GetPatientArchivesTypeList(new PatientArchivesTypeEntity { UserID = UserID });

                foreach (var info in list)
                {
                    if (info.ArchivesType == "非正式会员")
                    {
                        isset = false;
                    }
                    else if (info.ArchivesType == "正式会员")
                    {
                        isset = true;
                        break;
                    }
                }
                //
                if (isset == false)
                {
                    patientBLL.UpdateFirstZhuanzhengTime(UserID);
                }
            }
            int result = patientBLL.AddPatientArchivesType(new PatientArchivesTypeEntity { UserID = UserID, HospitalID = LoginUserEntity.HospitalID, ArchivesType = ArchivesType, CreateTime = DateTime.Now, CreateUserName = LoginUserEntity.UserName });
            return Json(result);
        }
        /// 删除会员信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult DelPatient(int ID)
        {
            int result = patientBLL.DelPatient(ID);
            return Json(result);
        }
        /// <summary>
        /// 根据ID更改会员卡
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult UpdateMemberNo(PatientEntity entity)
        {
            var result = 0;
            if (String.IsNullOrEmpty(entity.MemberNo))
            {
                //生成临时会员
                entity.MemberNo = RandomHelper.GetRandom("L");
            }

            if (String.IsNullOrEmpty(entity.Password))
            {

                entity.Password = "123456";
                result = patientBLL.UpdatePwd(entity);
            }
            else
            {
                result = patientBLL.UpdatePwd(entity);
            }
            //获取卡号是否是唯一的
            int countre = patientBLL.GetUserCardNo(entity.MemberNo.ToLower(), LoginUserEntity.ParentHospitalID);
            if (countre > 0)
            {
                return Json("-9");
            }
            result = patientBLL.UpdateUserCardNo(entity);
            var Patient = patientBLL.GetPatienttEntityByID(Convert.ToInt64(entity.UserID));
            if (Patient.RuHuiTime == null)
            {
                Patient.RuHuiTime = DateTime.Now.ToString("yyyy-MM-dd");
                patientBLL.UpPatient(Patient);
            }
            return Json(result);
        }
        /// <summary>
        /// 更改会员卡密码页面
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateCardPwd()
        {
            return View();
        }
        /// <summary>
        /// 更改卡密码
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult UpdatePwd(PatientEntity entity, string oldPassword)
        {
            //获取用户原始密码
            var info = patientBLL.GetPatienttEntityByID(entity.UserID);
            if (info.Password != Encrypt.MD5(oldPassword))
            {
                return Json(-9);
            }
            var result = patientBLL.UpdatePwd(entity);
            return Json(result);
        }
        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ResetUpdatePwd(PatientEntity entity, string oldPassword)
        {
            //获取用户原始密码
            entity.Password = "123456";
            var result = patientBLL.UpdatePwd(entity);
            return Json(result);
        }
        /// <summary>
        /// 账户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult AccountPage()
        {
            var userid = 0;
            if (!string.IsNullOrEmpty(Request["UserID"]))
                userid = Convert.ToInt32(Request["UserID"].Split(',')[0]);
            //获取用户储值卡余额列表
            //var CardBalancelist = cashBll.GetMainCardBalanceList(new MainCardBalanceEntity { Type = 1, UserID = Convert.ToInt32(Request["UserID"]) });
            var CardBalancelist = cashBll.GetMainCardBalanceList(new MainCardBalanceEntity { Type = 1, UserID = Convert.ToInt32(userid) });
            //获取疗程卡列表
            //var Prijectlist = cashBll.GetProjectTimesSel(new MainCardBalanceEntity { UserID = Convert.ToInt32(Request["UserID"]), ParentHospitalID = LoginUserEntity.ParentHospitalID });
            var Prijectlist = cashBll.GetProjectTimesSel(new MainCardBalanceEntity { UserID = Convert.ToInt32(userid), ParentHospitalID = LoginUserEntity.ParentHospitalID });
            var list = new List<MainCardBalanceEntity>();

            //余额要取有剩余的
            CardBalancelist = CardBalancelist.Where(i => i.Price > 0).ToList();
            Prijectlist = Prijectlist.Where(i => i.Tiems > 0).ToList();


            foreach (var cinfo in CardBalancelist)
            {
                MainCardBalanceEntity entity = new MainCardBalanceEntity();
                entity.Price = cinfo.Price;
                entity.Name = cinfo.Name;
                entity.Number = 0;
                entity.AllTiems = 0;
                list.Add(entity);
            }
            foreach (var pinfo in Prijectlist)
            {
                MainCardBalanceEntity entity = new MainCardBalanceEntity();
                entity.Price = 0;
                entity.Number = pinfo.Tiems;
                entity.AllTiems = pinfo.AllTiems;
                entity.Name = pinfo.ProjectName;
                list.Add(entity);
            }
            var result = list.AsQueryable().ToPagedList(1, 1000000000);

            return View(result);

        }
        /// <summary>
        /// 客户档案详情
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientDetailPage()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }
        /// <summary>
        /// 消费记录
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientOrderList(OrderinfoEntity entity)
        {
            int rows;
            int pagecount;
            entity.numPerPage = 30; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; entity.orderDirection = "desc"; }
            var list = cashBll.GetXiaofeiList(entity, out rows, out pagecount);
            var AddList = new List<OrderinfoEntity>();
            var DelList = new List<OrderinfoEntity>();

            foreach (var newinfo in list)
            {
                var info = newinfo;
                if (info.Type == 3 && info.IsChongzhi == 99)
                {
                    ProjectProductEntity pp = new ProjectProductEntity();
                    pp.ProgrammeID = newinfo.ProdcuitID;
                    if (info.CardBalanceID != null)
                    {
                        string[] idlist = info.CardBalanceID.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var id in idlist)
                        {
                            if (id == "0")
                            {
                                continue;
                            }
                            var cardbalanc = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = Convert.ToInt32(id) });

                            var Add = new OrderinfoEntity();
                            Add.CardType = newinfo.CardType;
                            Add.CreateTime = newinfo.CreateTime;
                            Add.JoinUserName = newinfo.JoinUserName;
                            Add.OrderType = newinfo.OrderType;
                            Add.Quantity = cardbalanc.AllTiems;
                            Add.Type = newinfo.Type;
                            Add.AllPrice = cardbalanc.AllPrice;
                            Add.ProdcuitName1 = cardbalanc.ProjectName;
                            Add.ProdcuitName = info.ProdcuitName;
                            Add.HospitalName = newinfo.HospitalName;
                            Add.ID = newinfo.ID;
                            Add.OrderNo = newinfo.OrderNo;
                            Add.IsZhiding = newinfo.IsZhiding;
                            Add.Memo = newinfo.Memo;
                            AddList.Add(Add);
                        }
                        DelList.Add(newinfo);
                    }
                    //var pplist = cashBll.getProjectProductNopage(pp);
                    //foreach (var ppinfo in pplist)
                    //{
                    //    var Add = new OrderinfoEntity();
                    //    Add.CardType = newinfo.CardType;
                    //    Add.CreateTime = newinfo.CreateTime;
                    //    Add.JoinUserName = newinfo.JoinUserName;
                    //    Add.OrderType = newinfo.OrderType;
                    //    Add.Quantity = newinfo.Quantity;
                    //    Add.Type = newinfo.Type;
                    //    Add.ProdcuitName1 = ppinfo.ProjectName;
                    //    AddList.Add(Add);
                    //}
                    //DelList.Add(newinfo);
                }
                else if (info.IsChongzhi == 1)
                {
                    var cardbalanc = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = info.ProdcuitID });
                    var Add = new OrderinfoEntity();
                    Add.CardType = newinfo.CardType;
                    Add.CreateTime = newinfo.CreateTime;
                    Add.JoinUserName = newinfo.JoinUserName;
                    Add.OrderType = newinfo.OrderType;
                    Add.Quantity = info.Quantity;
                    Add.Type = newinfo.Type;
                    Add.AllPrice = info.AllPrice;
                    //  Add.ProdcuitName1 = cardbalanc.Name;
                    Add.ProdcuitName = info.ProdcuitName;
                    Add.HospitalName = newinfo.HospitalName;
                    Add.ID = newinfo.ID;
                    Add.OrderNo = newinfo.OrderNo;
                    Add.IsZhiding = newinfo.IsZhiding;
                    Add.Memo = newinfo.Memo;
                    AddList.Add(Add);
                    DelList.Add(newinfo);
                }

                if (newinfo.OrderType == 4) //如果是还款操作,给ProdcuitName字段重新赋值
                {
                    string orderno = null;
                    string newname = null;
                    var cxlist = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = newinfo.OrderNo });     //如果是还款操作,用自身的订单编号获取订单详情,从而获取订单详情下面的对应的订单号
                    foreach (var cxinfo in cxlist)
                    {
                        if (cxinfo.ProdcuitName != null)
                        {
                            orderno = cxinfo.ProdcuitName;
                        }
                    }
                    if (orderno != null)
                    {
                        var dylist = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = orderno });
                        foreach (var dyinfo in dylist)
                        {
                            newname = newname + dyinfo.ProdcuitName ;
                        }
                    }

                    newinfo.ProdcuitName = newname;

                }
                else if (newinfo.OrderType == 5) //退换
                {
                    newinfo.AllPrice = -newinfo.AllPrice;
                    newinfo.Quantity = -newinfo.Quantity;
                }
            }
            foreach (var Ainfo in AddList)
            {
                list.Add(Ainfo);
            }
            foreach (var Dinfo in DelList)
            {
                list.Remove(Dinfo);
            }

            list = list.OrderByDescending(c => c.CreateTime).ToList();
            #region ==加入员工.(无法一次性读取出所有数据,数据是组装出来的,故而员工直接取订单关联员工.)==
            int ro = 0; int pagec = 0;
            foreach (var info in list)
            {
                var user = cashBll.GetAllJoinUser(new JoinUserEntity { OrderNo = info.OrderNo, OrderInfoID = info.ID, numPerPage = 50000, orderField = "UserID" }, out ro, out pagec);
                StringBuilder sb = new StringBuilder();
                StringBuilder sname = new StringBuilder();
                foreach (var u in user)
                {
                    if (sb.Length == 0)
                    {
                        sb.Append(u.UserName);
                    }
                    else
                    {
                        sb.Append("," + u.UserName);
                    }
                    if (sname.Length == 0)
                    {
                        sname.Append(u.IsZhiding == 1 ? "指定" : "非指定");
                    }
                    else
                    {
                        sname.Append("," + (u.IsZhiding == 1 ? "指定" : "非指定"));
                    }

                }
                info.JoinUserName = sb.ToString();
                info.ZhidingMemo = sname.ToString();
            }

            #endregion



            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.Quatity = list.Sum(i => i.Quantity);
            ViewBag.AllPrice = list.Sum(i => i.AllPrice);
            return View(result);
        }

        public ActionResult BuyOrderList(OrderinfoEntity entity)
        {
            int rows;
            int pagecount;
            entity.numPerPage = 10000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; entity.orderDirection = "desc"; }
            var list = cashBll.GetGouMaiList(entity, out rows, out pagecount);
            var AddList = new List<OrderinfoEntity>();
            var DelList = new List<OrderinfoEntity>();
            //  list = list.OrderBy(i => i.DocumentNumber).ToList();


            int Quantity = 0;
            decimal AllPrice = 0;

            string oldorderno = "";
            string oldshijian = "";
            string olddanju = "";



            ////组装SQL
            //foreach (var info in list)
            //{
            //    StringBuilder str = new StringBuilder();
            //    var paymentlist = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = info.OrderNo });
            //    //var id = paymentlist.Where(i => i.PayType <= 2).Count();
            //    if (paymentlist.Where(i => i.PayType <= 2).Count() == 0) { entity.ids = null; continue; }
            //    var Xianjin = paymentlist.Where(t => t.PayType == 1).Count() > 0 ? paymentlist.Where(t => t.PayType == 1).First().PayMoney : 0;
            //    var Shuaka = paymentlist.Where(t => t.PayType == 2).Count() > 0 ? paymentlist.Where(t => t.PayType == 2).First().PayMoney : 0;
            //    var count = list.Where(t => t.OrderNo == info.OrderNo).Count();
            //    var shijian = patientBLL.GetTodayUserMoneyPayCount(entity.UserID, info.CreateTime); //list.Where(t => t.CreateTime.ToShortDateString() == info.CreateTime.ToShortDateString()).Count() > 0 ? list.Where(t => t.CreateTime.ToShortDateString() == info.CreateTime.ToShortDateString()).Count() : 0;

            //    var danju = list.Where(t => t.DocumentNumber == info.DocumentNumber).Count() > 0 ? list.Where(t => t.DocumentNumber == info.DocumentNumber).Count() : 0;
            //    var qita = paymentlist.Where(t => t.PayType > 4).Count() > 0 ? paymentlist.Where(t => t.PayType > 4).First().PayMoney : 0;
            //    var chuzhikahuakou = paymentlist.Where(t => t.PayType == 3).Count() > 0 ? paymentlist.Where(t => t.PayType == 3).First().PayMoney : 0;
            //    var liaochenghuakou = paymentlist.Where(t => t.PayType == 4).Count() > 0 ? paymentlist.Where(t => t.PayType == 4).First().PayMoney : 0;

            //    if (oldshijian != info.CreateTime.ToShortDateString())
            //    {
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", shijian, info.CreateTime.ToString("yyyy-MM-dd"));
            //        oldshijian = info.CreateTime.ToShortDateString();
            //    }
            //    if (olddanju != info.DocumentNumber)
            //    {
            //        str.AppendFormat("<td rowspan='{0}'><a href='#' onclick=\"OpenOrderDetail('{2}')\" > {1}</a></td>", danju, info.DocumentNumber, info.OrderNo);
            //        olddanju = info.DocumentNumber;
            //    }

            //    str.AppendFormat("<td>{0}</td>", info.HospitalName);
            //    str.AppendFormat("<td>{0}</td>", EYB.Web.Code.PageFunction.GetorderType(info.OrderType));
            //    str.AppendFormat("<td>{0}</td>", info.ProdcuitName);
            //    str.AppendFormat("<td>{0}</td>", info.Price);
            //    str.AppendFormat("<td>{0}</td>", info.OrderType < 4 ? (info.OrderType == 1 ? 1 : info.Quantity) : -System.Math.Abs(info.Quantity));
            //    str.AppendFormat("<td>{0}</td>", info.OrderType < 4 ? info.AllPrice : -System.Math.Abs(info.AllPrice));
            //    if (oldorderno != info.OrderNo)
            //    {
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, Xianjin);
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, Shuaka);
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, qita);
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, chuzhikahuakou);
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, liaochenghuakou);
            //        oldorderno = info.OrderNo;
            //    }

            //    info.ids = str.ToString();
            //    AllPrice = AllPrice + info.AllPrice;
            //    if (info.OrderType == 1)
            //        Quantity = Quantity + info.Quantity;
            //}
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            //var sum= cashBll.GetSumXiaofeiList(entity);
            //ViewBag.Quantity = Quantity;
            //ViewBag.AllPrice = AllPrice;

            return View(result);
        }



        /// <summary>
        /// 欠款
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult PatientQianPrice(OrderEntity entity)
        {
            var list = cashBll.GetPaymentRecordList(entity.UserID);
            var summodel = cashBll.GetSumRecordList(entity.UserID);
            var result = list.AsQueryable().ToPagedList(1, 1000);
            ViewBag.QianPrice = summodel.QianPrice;
            ViewBag.HuanKuan = summodel.HuanPrice;
            ViewBag.Price = summodel.Price;
            //result.TotalItemCount = list.Count;
            //result.CurrentPageIndex = entity.pageNum;
            //ViewBag.orderField = entity.orderField;
            //ViewBag.orderDirection = entity.orderDirection;
            return View(result);
        }
        /// <summary>
        /// 顾客消费记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult PatientPayOrderList(OrderEntity entity)
        {
            //默认查询已结算订单
            if (entity.Statu == 0)
            {
                entity.Statu = 1;
            }
            //查询所有订单
            if (entity.Statu == 999)
            {
                entity.Statu = 0;
            }

            decimal qianPrice = 0;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;

            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.UserName == "姓名、订单号、会员卡号") entity.UserName = "";
            entity.StartTime = (entity.StartTime == null ? DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd 00:00:01") : Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = (entity.EndTime == null ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59") : Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59"));

            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; entity.orderDirection = "desc"; }
            bool hasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            if (hasPermission1)
            {
                entity.IsShow = 2;
            }
            var list = cashBll.GetAllOrder(entity, out qianPrice);
            var sumprice = cashBll.GetSumOrder(entity);  //获取总订单金额
            decimal pageSum = 0;
            var newlist = new List<OrderEntity>();
            foreach (var info in list)
            {
                pageSum = pageSum + info.Price;
                var infoList = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = info.OrderNo });

                foreach (var io in infoList)
                {
                    switch (io.PayType)
                    {
                        case 1:
                            info.Xianjin = io.PayMoney;
                            break;
                        case 2:
                            info.Yinlianka = io.PayMoney;
                            break;
                        case 5:
                            info.Chuzhika = io.PayMoney;
                            break;
                    }
                }
                newlist.Add(info);
            }

            ViewBag.SumPrice = sumprice;
            ViewBag.PageSum = pageSum;
            ViewBag.sumXianjin = list.Sum(i => i.Xianjin);
            ViewBag.yinlian = list.Sum(i => i.Yinlianka);
            ViewBag.qiankuan = list.Sum(i => i.QianPrice);

            var result = newlist.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = entity.Rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;

            if (Request.IsAjaxRequest())
                return PartialView("_PatientPayOrderList", result);
            return View(result);
        }
        /// <summary>
        /// 顾客跟踪
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientFollow(CustomertracksEntity entity)
        {
            if (Convert.ToDateTime(entity.EndTime).Year < 2000) entity.EndTime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
            if (Convert.ToDateTime(entity.StartTime).Year < 2000) entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss"));
            entity.StartTime = Convert.ToDateTime(Convert.ToDateTime(entity.StartTime).Year + "-" + Convert.ToDateTime(entity.StartTime).Month + "-" + Convert.ToDateTime(entity.StartTime).Day + " 00:00:00");
            entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(entity.EndTime).Year + "-" + Convert.ToDateTime(entity.EndTime).Month + "-" + Convert.ToDateTime(entity.EndTime).Day + " 23:59:59");
            int rows;
            int pagecount;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            //entity.Type = 1;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            if (entity.orderDirection + "" == "") { entity.orderDirection = "desc"; }
            if (entity.IsVisit == 0) { entity.IsVisit = -1; }
            if (entity.IsVisit == 2) { entity.IsVisit = 0; }
            var list = patientBLL.GetCustomertracksList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.StartTime = entity.StartTime.ToString("yyyy-MM-dd hh:MM:ss");
            ViewBag.EndTime = entity.EndTime.ToString("yyyy-MM-dd hh:MM:ss");
            if (Request.IsAjaxRequest())
                return PartialView("_PatientFollow", result);
            return View(result);
        }
        /// <summary>
        /// 添加回访记录
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPatientFollow()
        {
            var id = Request["ID"];
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }
        /// <summary>
        /// 添加回访记录
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPatientFollowInfo(CustomertracksEntity entity)
        {
            string[] albumArr = Request.Form.GetValues("hid_photo_name");
            string[] hid_photo_remark = Request.Form.GetValues("hid_photo_remark");
            entity.CreateTime = DateTime.Now;
            entity.Type = 1;
            entity.HospitalID = LoginUserEntity.HospitalID;
            var result = patientBLL.AddCustomertracks(entity);
            FollowImgEntity img = new FollowImgEntity();
            img.FollowID = result;

            if (albumArr != null && albumArr.Length > 0)
            {
                for (int i = 0; i < albumArr.Length; i++)
                {
                    string[] imgArr = albumArr[i].Split('|');
                    if (imgArr.Length == 3)
                    {
                        // curbll.AddMenuContentImg(new MenuContentImgEntity { Url = imgArr[1], MenuContentID = result, Content = hid_photo_remark[i] });
                        img.Url = imgArr[1];
                        img.Remark = hid_photo_remark[i];
                        patientBLL.AddFollowImg(img);
                    }
                }
            }
            return Json(result);
        }
        /// <summary>
        /// 修改回访记录
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult UpdatePatientFollow(int ID)
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var Model = patientBLL.GetCustomertracksByID(ID);
            var imglist = patientBLL.GetFollowImgByFollowID(Model.ID);
            ViewData["img"] = imglist.ToList();
            return View(Model);
        }
        /// <summary>
        /// 修改保存方法
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult UpdatePatientFollowInfo(CustomertracksEntity entity)
        {
            string[] albumArr = Request.Form.GetValues("hid_photo_name");
            string[] hid_photo_remark = Request.Form.GetValues("hid_photo_remark");
            var result = patientBLL.UpCustomertracks(entity);
            var imglist = patientBLL.GetFollowImgByFollowID(entity.ID);
            List<FollowImgEntity> saveID = new List<FollowImgEntity>();
            FollowImgEntity img = new FollowImgEntity();
            img.FollowID = entity.ID;
            if (albumArr != null && albumArr.Length > 0)
            {
                for (int i = 0; i < albumArr.Length; i++)
                {
                    string[] imgArr = albumArr[i].Split('|');
                    if (imgArr.Length == 3)
                    {
                        //如果ID为0则表示是新加的
                        if (imgArr[0] == "0")
                        {
                            img.Url = imgArr[1];
                            img.Remark = hid_photo_remark[i];
                            patientBLL.AddFollowImg(img);
                        }
                        else//如果不为0,则去匹配原来的列哪些需要继续保存
                        {
                            //如果有图片地址,说明是需要保存的内容.
                            var selinfo = imglist.Where(y => y.Url == imgArr[1]).ToList();
                            if (selinfo.Count > 0)
                                saveID.Add(selinfo[0]);

                            //修改操作
                            if (selinfo[0].Remark != hid_photo_remark[i])
                            {
                                selinfo[0].Remark = hid_photo_remark[i];
                                patientBLL.UpdateFollowImg(selinfo[0]);

                            }
                        }
                    }
                }
            }
            var delID = imglist.Except(saveID).ToList();
            foreach (var delinfo in delID)
            {
                patientBLL.DelFollowImgByID(delinfo.ID);
            }
            return Json(result);
        }
        /// <summary>
        /// 删除回访记录
        /// </summary>
        /// <returns></returns>
        public ActionResult DelCustomertracks(string ID)
        {
            var result = patientBLL.DelCustomertracks(ID);
            return Json(result);
        }
        /// <summary>
        /// 顾客跟踪回访 内页
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientFollowPage(CustomertracksEntity entity)
        {
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            // entity.Type = 1;
            entity.IsVisit = -1;//查询全部
            //查询的是顾客的回访，从顾客进去的，所以把UserID重置，该查询方法通用，列表页也是调用此查询方法
            entity.CustomerUserID = entity.UserID;
            entity.UserID = 0;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            entity.StartTime = DateTime.Now.AddYears(-20);
            entity.EndTime = DateTime.Now.AddYears(1);

            var list = patientBLL.GetCustomertracksList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            return View(result);
        }
        /// <summary>
        /// 获取积分
        /// </summary>
        /// <returns></returns>
        public ActionResult IntegralManage(IntegralEntity entity)
        {
            int rows;
            int pagecount;
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;//基础数据通用总店ID
            }
            //entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField == "" || entity.orderField == null)
            {
                entity.orderField = "UserName";
                entity.orderDirection = "desc";
            }
            var list = patientBLL.GetIntegralList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_IntegralManage", result);
            }
            return View(result);
        }
        /// <summary>
        /// 客户详情--储值卡
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientChuzhiCard(MainCardBalanceEntity entity)
        {
            decimal chongzhi, xiaofei = 0;
            decimal Shengyu = 0;
            int row, pagecount = 0;
            entity.Type = 1;
            entity.numPerPage = 50;
            ViewBag.UserID = entity.UserID;
            ViewBag.Price = entity.Price;
            //var list = cashBll.GetMainCardBalanceList(entity);
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            var newList = cashBll.GetMainCardBalanceListInUser(entity, out row, out pagecount, out chongzhi, out xiaofei, out Shengyu);
            ViewBag.cz = chongzhi;
            ViewBag.ye = Shengyu;
            ViewBag.xf = xiaofei;
            newList = newList.Where(t => t.IsDel != 3&&t.Name!= "积分消费").ToList();
            var result = newList.ToPagedList(1, 10000);
            if (Request.IsAjaxRequest())
                return PartialView("_PatientChuzhiCard", result);
            return View(result);
        }
        /// <summary>
        /// 客户详情--疗程卡
        /// </summary>
        /// <returns></returns>
        //public ActionResult PatientProjectCard(MainCardBalanceEntity entity)
        //{
        //    entity.numPerPage = 1000; //每页显示条数
        //    if (entity.orderField + "" == "") { entity.orderField = "AddTime"; }
        //    int row, pagecount = 0;
        //    int Xiaofei, Shengyu = 0;
        //    entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
        //    var list = cashBll.GetProjectTimesInUser(entity, out row, out pagecount, out Xiaofei, out Shengyu);
        //    ViewBag.allcishu = Shengyu;
        //    ViewBag.allxiaohao = Xiaofei;
        //    var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
        //    if (Request.IsAjaxRequest())
        //        return PartialView("_PatientProjectCard", result);
        //    return View(result);
        //}
        //  
        /// <summary>
        /// 客户详情--疗程卡
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientProjectCard(MainCardBalanceEntity entity)
        {
            entity.numPerPage = 1000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "AddTime"; }
            int row, pagecount = 0;
            int Xiaofei, Shengyu = 0;
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            // var list = cashBll.GetProjectTimesInUser(entity, out row, out pagecount, out Xiaofei, out Shengyu);
            var list = cashBll.GetProjectTimesInUserNewOne(entity, out row, out pagecount, out Xiaofei, out Shengyu);
            ViewBag.allcishu = Shengyu;
            ViewBag.allxiaohao = Xiaofei;
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            if (Request.IsAjaxRequest())
                return PartialView("_PatientProjectCard", result);
            return View(result);
        }
        /// <summary>
        /// 客户详情--储值卡
        /// </summary>
        /// <returns></returns>
        public ActionResult Cardinfo_In()
        {
            string type = Request["Type"];//类别
            int UserID = Convert.ToInt32(Request["UserID"]);
            int ID = Convert.ToInt32(Request["ID"]);
            var result = new List<MainCardBalanceEntity>();
            PaymentOrderProductEntity pop = new PaymentOrderProductEntity();
            pop.UserID = UserID;
            pop.CardID = ID;
            var xiaohaolist = cashBll.GetPaymentOrderProductForID(pop);
            if (type.ToUpper() == "IN")
            {
                MainCardBalanceEntity mcb = new MainCardBalanceEntity();
                mcb.ProjectID = ID;
                var chongzhilist = cashBll.GetChongZhiList(mcb).Where(c => c.UserID == UserID);
                var maikapirice = cashBll.GetBuyCardForID(mcb);
                result.Add(maikapirice);
                var xiaohaolist1 = xiaohaolist.Where(i => i.OrderType == 5).ToList();
                xiaohaolist = xiaohaolist.Where(i => i.PayMoney < 0).ToList();
                foreach (var info in xiaohaolist)
                {
                    MainCardBalanceEntity mentity = new MainCardBalanceEntity();
                    mentity.Price = -info.PayMoney;
                    mentity.BuyCardTime = info.BuyCardTime;
                    mentity.UserName = info.UserName;
                    mentity.OrderNo = info.OrderNo;
                    result.Add(mentity);
                }
                foreach (var info in xiaohaolist1)
                {
                    MainCardBalanceEntity mentity = new MainCardBalanceEntity();
                    mentity.Price = info.PayMoney;
                    mentity.BuyCardTime = info.BuyCardTime;
                    mentity.UserName = info.UserName;
                    mentity.OrderNo = info.OrderNo;
                    result.Add(mentity);
                }
                foreach (var info in chongzhilist)
                {
                    if (info.OrderType == 5)
                    {
                        info.Price = -info.Price;
                    }
                    result.Add(info);
                }
            }
            else
            {
                //PaymentOrderProductEntity pop = new PaymentOrderProductEntity();
                //pop.UserID = UserID;
                //pop.CardID = ID;
                //var xiaohaolist = cashBll.GetPaymentOrderProductForID(pop);
                //  xiaohaolist = xiaohaolist.Where(i => i.PayMoney > 0).ToList();
                xiaohaolist = xiaohaolist.Where(i => i.OrderType != 5).ToList();
                foreach (var info in xiaohaolist)
                {
                    MainCardBalanceEntity mentity = new MainCardBalanceEntity();
                    mentity.Price = info.PayMoney;
                    mentity.BuyCardTime = info.BuyCardTime;
                    mentity.UserName = info.UserName;
                    mentity.OrderNo = info.OrderNo;
                    result.Add(mentity);
                }
            }
            ViewBag.SumNum = result.Sum(i => i.Price);
            var returnlist = result.AsQueryable().ToPagedList(1, 100000);
            return View(returnlist);
        }
        public ActionResult TreatmentCard_Out()
        {
            // int UserID = Convert.ToInt32(Request["UserID"]);
            int ID = Convert.ToInt32(Request["ID"]);//接受的是此卡项的用户余额记录ID
            MainCardBalanceEntity entity = new MainCardBalanceEntity();
            entity.ID = ID;
            var list = cashBll.GetliaochengXiaohao(entity);      //获取正常的划扣列表
            ViewBag.SumNum = list.Sum(i => i.Number);
            //entity.ID = ID;
            //var list1 = cashBll.GetliaochengXiaohaoByTuieka(entity);   //获取退卡的列表
            //foreach (var info in list1)
            //{
            //    list.Add(info);
            //}
            var result = list.AsQueryable().ToPagedList(1, 100000);
            return View(result);
        }
        public ActionResult QianKuan_List()
        {
            // int ID = Convert.ToInt32(Request["ID"]);
            HuanKuanEntity hk = new HuanKuanEntity();
            hk.OrderNo = Request["OrderNo"];
            var list = cashBll.GetQianKuanlist(hk);
            var result = list.AsQueryable().ToPagedList(1, 100000);
            return View(result);
        }
        /// <summary>
        /// 护理日志表格页面
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientFollowTable()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        public ActionResult GetPatientFollowByTable(CustomertracksEntity entity)
        {
            if (Convert.ToDateTime(entity.EndTime).Year < 2000) entity.EndTime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
            if (Convert.ToDateTime(entity.StartTime).Year < 2000) entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss"));
            entity.StartTime = Convert.ToDateTime(Convert.ToDateTime(entity.StartTime).Year + "-" + Convert.ToDateTime(entity.StartTime).Month + "-" + Convert.ToDateTime(entity.StartTime).Day + " 00:01:01");
            entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(entity.EndTime).Year + "-" + Convert.ToDateTime(entity.EndTime).Month + "-" + Convert.ToDateTime(entity.EndTime).Day + " 23:59:59");
            int rows;
            int pagecount;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            //entity.Type = 1;
            entity.numPerPage = 999999999; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            if (entity.IsVisit == 0) { entity.IsVisit = -1; }
            if (entity.IsVisit == 2) { entity.IsVisit = 0; }
            //获取所有顾客日志
            var list = patientBLL.GetCustomertracksList(entity, out rows, out pagecount);
            //获取员工列表
            var UserList = personBLL.GetAllUserByHospitalID(entity.HospitalID, 0);
            TimeSpan span = entity.EndTime.Subtract(entity.StartTime);
            StringBuilder sb = new StringBuilder();
            sb.Append("<table id=\"weekTable\" class=\"todayrevertion\" style=\"overflow:auto;\" cellpadding=\"0\" cellpadding=\"0\">");
            sb.Append("<thead><tr>");
            sb.Append("<th>员工</th>");
            for (int len = 0; len <= span.Days; len++)
            {
                sb.AppendFormat("<th >{0}</th>", entity.StartTime.AddDays(len).ToString("MM-dd"));
            }
            sb.Append("</tr></thead>");
            sb.Append("<tbody>");
            foreach (var userinfo in UserList)
            {
                int num = 1;
                sb.Append("<tr>");
                sb.AppendFormat("<td >{0}</td>", userinfo.UserName);
                for (int len = 0; len <= span.Days; len++)
                {
                    sb.AppendFormat("<td style=\"{0}\"> <div  style=\"position:relative;width:90px; min-height:90px;\">", num % 2 == 1 ? "background-color:#d8eff8" : "");
                    var todaylist = list.Where(i => i.CreateTime.Date == entity.StartTime.AddDays(len).Date && i.UserID == userinfo.UserID).ToList();
                    foreach (var todayinfo in todaylist)
                    {
                        sb.AppendFormat("<a style=\"font-size:12px;\" onclick=\"UpdateCustomertrack('{0}')\" class=\"weekreservation\" href=\"#\">{1}({2})</a><br />", todayinfo.ID, todayinfo.CustomerUsername, todayinfo.IsVisit == 1 ? "已确认" : "待确认");
                    }
                    sb.Append("</div></td>");
                    num++;
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");
            sb.Append("</table>");
            return Json(sb.ToString());
        }
        /// <summary>
        /// 满意度点评
        /// </summary>
        /// <returns></returns>
        public ActionResult manyidu(RemarkEntity entity)
        {
            int rows;
            int pagecount;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            if (entity.Years == 0) { entity.Years = DateTime.Now.Year; }
            if (entity.Months == 0) { entity.Months = DateTime.Now.Month; }
            var list = cashBll.GetRemarkList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Years = entity.Years;
            ViewBag.Months = entity.Months;
            if (Request.IsAjaxRequest())
                return PartialView("_Manyidu", result);
            return View(result);
        }
        #endregion

        #region ==积分管理==
        /// <summary>
        /// 积分详细列表页
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult IntegralRecordList(IntegralRecordEntity entity)
        {
            int userid = Convert.ToInt32(Request["userid"]);
            int rows;
            int pagecount;
            entity.UserID = userid;
            entity.numPerPage = 10; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }

            //var list = patientBLL.GetIntegralRecordList(entity, out rows, out pagecount);
            //string sql = @"select top 100  * from EYB_tb_IntegralRecord
            //            where 1 = 1
            //            and UserID = " + entity.UserID + @"
                        
            //            ";
            string sql = @"select top 100  * from EYB_tb_IntegralRecord
                        where 1=1
                        and UserID= " + entity.UserID + @" 
                        AND ID NOT IN (
	                        SELECT
		                        MIN (ID)
	                        FROM
		                        EYB_tb_IntegralRecord
	                        WHERE
		                        UserID = " + entity.UserID + @"
	                        GROUP BY
		                        UserID,
		                        OrderNO,
		                        Integral,
		                        CONVERT (VARCHAR(19), CreateTime, 120)
	                        HAVING
		                        COUNT (*) > 1
                        )
                        ORDER BY CreateTime desc";
            var list = GetIntegralRecordList(sql);
            foreach (var info in list)
            {
                if (info.IntegralOperationID != 0)
                {
                    var model = patientBLL.GetOperationByID(info.IntegralOperationID);
                    info.Name = model.Name;
                    info.Memo = model.Memo;
                }
            }
            var result = list.AsQueryable().ToPagedList(entity.pageNum, entity.numPerPage);
            result.TotalItemCount = list.Count();// rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            return View(result);
        }
        private IList<IntegralRecordEntity> GetIntegralRecordList(string sqlstr, int filter = 0)
        {

            var list = new List<IntegralRecordEntity>();
            try
            {
                string conStr = "server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft";//连接字符串  
                SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                conText.Open();//打开数据库  
                string sqls = sqlstr;//创建统计语句  
                SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                SqlDataReader dr;//创建DataReader对象  
                dr = comText.ExecuteReader();//执行查询  
                //var list = new List<IntegralRecordEntity>();
                while (dr.Read())//判断数据表中是否含有数据  
                {
                    list.Add(DataBindHelper.CreateEntity<IntegralRecordEntity>(dr));
                }
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return list;

        }
        public ActionResult IntegralEdit()
        {
            string userid = Request["userid"];
            IntegralEntity entity = new IntegralEntity();
            entity = patientBLL.GetIntegralModel(Convert.ToInt32(userid));
            ViewBag.UserID = userid;
            return View(entity);
        }
        public ActionResult EditIntegral()
        {
            int OperationType = int.Parse(Request["OperationType"]);  //操作
            string ExchangeType = Request["ExchangeType"];  //兑换方式
            string Name = Request["Name"];    //兑换物品名称
            string cardstr = Request["CardID"];  //接受储值卡ID
            string kouNumber = Request["kouNumber"];  //扣除数量
            string ZengNumber = Request["ZengNumber"];//赠送数量
            string money = Request["money"];//兑换金额
            string Memo = Request["Memo"];   //备注
            int userID = int.Parse(Request["UserID"]);//用户ID
            IntegralOperationEntity entity = new IntegralOperationEntity();
            entity.UserID = userID;
            entity.Memo = Memo;
            entity.Name = Name;
            #region ==数据采集处理==
            if (OperationType == 1) //积分操作
            {
                entity.OperationType = 1;
                entity.Action = "Reduction";
                if (ExchangeType == "Divwuping")//兑换物品
                {
                    entity.ExchangeType = 1;
                    entity.Name = Name;
                    entity.Number = int.Parse(kouNumber);

                }
                else if (ExchangeType == "Divchuzhika")//兑换储值卡
                {
                    if (!string.IsNullOrEmpty(cardstr))
                    {
                        entity.ExchangeType = 2;
                        entity.CardID = Convert.ToInt32(cardstr);
                        entity.Number = int.Parse(kouNumber);
                        AddCardmoney(entity.CardID, userID, Convert.ToDecimal(money));
                    }
                    else
                    {
                        return Json(-1);
                    }
                }
            }
            else
            {
                entity.OperationType = 2;
                entity.Action = "Add";
                entity.Number = int.Parse(ZengNumber);
            }
            #endregion
            #region ==加入操作表==
            var id = patientBLL.AddIntegralOperation(entity);
            #endregion
            #region ==修改积分==
            IntegralEntity Ientity = new IntegralEntity();
            Ientity = patientBLL.GetIntegralModel(userID);  //获取当前用户的积分表

            if (entity.OperationType == 1)  //如果是积分操作,则减去这些值
            {
                //总积分不变
                Ientity.Integral = Ientity.Integral - entity.Number;//现有积分减少
                Ientity.UseIntegral = Ientity.UseIntegral + entity.Number;//消耗积分增加

            }
            else //如果是添加积分
            {
                Ientity.AllIntegral = Ientity.AllIntegral + entity.Number;//添加总积分
                Ientity.Integral = Ientity.Integral + entity.Number;//现有积分添加
                //消耗积分不变
            }
            patientBLL.UpdateIntegral(Ientity);//修改积分
            #endregion
            #region ==添加到积分记录表==
            //如果是添加积分操作,则加入到积分记录表
            if (entity.OperationType == 2)
            {
                IntegralRecordEntity IR = new IntegralRecordEntity();
                IR.CreateTime = DateTime.Now;
                IR.Integral = entity.Number;
                IR.DocumentNumber = "人员操作";
                IR.UserID = entity.UserID;
                IR.IntegralOperationID = id;
                patientBLL.AddIntegralRecord(IR);
            }
            else if (entity.OperationType == 1)
            {
                IntegralRecordEntity IR = new IntegralRecordEntity();
                IR.CreateTime = DateTime.Now;
                IR.Integral = -entity.Number;
                IR.DocumentNumber = "人员操作(兑换)";
                IR.UserID = entity.UserID;
                IR.Name = Name;
                IR.IntegralOperationID = id;
                patientBLL.AddIntegralRecord(IR);
            }

            #endregion
            return Json(1);
        }
        /// <summary>
        /// 卡充值
        /// </summary>
        /// <param name="cardid">卡ID</param>
        /// <param name="userID">用户ID</param>
        /// <param name="Money">充值金额</param>
        /// <returns></returns>
        public int AddCardmoney(int cardid, int userID, decimal Money)
        {
            var mcb = new MainCardBalanceEntity { ID = cardid, Price = Money };
            var result = cashBll.RechargeMoney(mcb);

            //用户储值卡余额明细日志
            var mainCardBalance = cashBll.GetMainCardBalanceModel(mcb);
            cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
            {
                CardBalanceId = mcb.ID,
                OrderNo = "",
                Type = (int)MainCardBalanceDetailType.充值,
                Amount = mcb.Price,
                Balance = mainCardBalance.Price + mcb.Price,
                OldAmount = mainCardBalance.Price,
                CreateTime = DateTime.Now
            });
            return result;
        }
        /// <summary>
        /// 清空积分
        /// </summary>
        /// <returns></returns>
        [WriteOperateLogAttribute("清除积分操作", "积分清除", "积分管理")]
        public ActionResult DelIntegNumber1()
        {
            string idList = Request["idlist"]; //ID列表
            string[] ID = idList.Split(new Char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);//分割ID列表
            foreach (string s in ID)
            {
                if (Convert.ToInt32(s) > 0)
                {
                    // patientBLL.ClearIntegralNumber(Convert.ToInt32(s));
                }
                IntegralEntity entity = new IntegralEntity();
                entity = patientBLL.GetIntegralModelByID(Convert.ToInt32(s));

                IntegralOperationEntity ir = new IntegralOperationEntity();
                ir.OperationType = 3;
                ir.ExchangeType = 3;
                ir.Name = "清空积分";
                ir.Number = -entity.Integral;
                ir.Memo = "首页清空积分";
                ir.UserID = entity.UserID;
                #region ==加入操作表==
                patientBLL.AddIntegralOperation(ir);
                #endregion
                #region ==修改积分==
                IntegralEntity Ientity = new IntegralEntity();
                Ientity = patientBLL.GetIntegralModel(entity.UserID);  //获取当前用户的积分表
                //Ientity.UseIntegral = Ientity.UseIntegral + Ientity.Integral;//消耗积分增加2019-12-25
                Ientity.Integral = 0;//现有积分减少
                Ientity.AllIntegral = 0;//
                Ientity.UseIntegral = 0;
                patientBLL.UpdateIntegral(Ientity);//修改积分
                #endregion
                #region ==添加到积分记录表==
                //如果是添加积分操作,则加入到积分记录表
                IntegralRecordEntity IR = new IntegralRecordEntity();
                IR.CreateTime = DateTime.Now;
                IR.Integral = -entity.Integral;
                IR.OrderNO = "人员操作(清空)";
                IR.UserID = entity.UserID;
                patientBLL.AddIntegralRecord(IR);
                #endregion
            }
            return Json(1);
        }
        /// <summary>
        /// 清空积分
        /// </summary>
        /// <returns></returns>
        [WriteOperateLogAttribute("清除积分操作", "积分清除", "积分管理")]
        public ActionResult DelIntegNumber()
        {
            string idList = Request["idlist"]; //ID列表
            string[] ID = idList.Split(new Char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);//分割ID列表
            foreach (string s in ID)
            {
                if (Convert.ToInt32(s) > 0)
                {
                    // patientBLL.ClearIntegralNumber(Convert.ToInt32(s));
                }
                IntegralEntity entity = new IntegralEntity();
                entity = patientBLL.GetIntegralModelByID(Convert.ToInt32(s));

                IntegralOperationEntity ir = new IntegralOperationEntity();
                ir.OperationType = 3;
                ir.ExchangeType = 3;
                ir.Name = "清空积分";
                ir.Number = -entity.Integral;
                ir.Memo = "首页清空积分";
                ir.UserID = entity.UserID;
                #region ==加入操作表==
                patientBLL.AddIntegralOperation(ir);
                #endregion
                #region ==修改积分==
                IntegralEntity Ientity = new IntegralEntity();
                Ientity = patientBLL.GetIntegralModel(entity.UserID);  //获取当前用户的积分表
                //Ientity.UseIntegral = Ientity.UseIntegral + Ientity.Integral;//消耗积分增加2019-12-25
                Ientity.Integral = 0;//现有积分减少
                patientBLL.UpdateIntegral(Ientity);//修改积分
                #endregion
                #region ==添加到积分记录表==
                //如果是添加积分操作,则加入到积分记录表
                IntegralRecordEntity IR = new IntegralRecordEntity();
                IR.CreateTime = DateTime.Now;
                IR.Integral = -entity.Integral;
                IR.OrderNO = "人员操作(清空)";
                IR.UserID = entity.UserID;
                patientBLL.AddIntegralRecord(IR);
                #endregion
            }
            return Json(1);
        }
        #endregion

        #region==顾客分析==

        /// <summary>
        /// 顾客分析
        /// </summary>
        /// <returns></returns>
        public ActionResult Patientanalyse()
        {
            return View();
        }
        /// <summary>
        /// 顾客分析
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientanalyseBig()
        {
            return View();
        }

        /// <summary>
        /// 顾客到店率统计
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult PatientstatisticsDaodian()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }
        /// <summary>
        /// 顾客到店率统计
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="entity"></param>
        /// <param name="operareType"></param>
        /// <returns></returns>
        public ActionResult PatientstatisticsDaodianOperate(PatientEntity entity, int operareType = 0)
        {

            DateTime StartTime1 = DateTime.Now; DateTime EndTime1 = DateTime.Now; int Zhouqi = 3;
            int zhengshicount = 1;
            if (entity.UserName == "姓名、会员卡号") entity.UserName = "";
            int daodiancount = 0;
            int zhengshidaodiancount = 0;
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            //判断统计方式，有月份就按月份，否则按年统计
            if (entity.months > 0)
            {
                //Zhouqi = 3;
                StartTime1 = Convert.ToDateTime(entity.years + "-" + entity.months + "-01 00:00:01");
                EndTime1 = Convert.ToDateTime(entity.years + "-" + entity.months + "-" + DateTime.DaysInMonth(entity.years, entity.months) + " 23:59:59");
            }
            else
            {
                StartTime1 = Convert.ToDateTime(entity.years + "-01-01 00:00:01");
                EndTime1 = Convert.ToDateTime(entity.years + "-12-31 23:59:59");
                Zhouqi = 5;//按年统计
            }
            if (entity.IsActive == 0)
            {
                entity.IsActive = 1;
            }

            StringBuilder str = new StringBuilder();
            str.Append("<table class='gridTable' id='gridTable'  style='width:100%;border-collapse:collapse;'><thead><tr><th><div  >客户卡号</div></th><th><div>客户姓名</div></th>");
            //str.Append("<th><div>所属门店</div></th>");
            str.Append("<th><div>最后消费时间</div></th>");
            //str.Append("<th><div>消费门店</div></th>");
            if (entity.IsChecked == 1)
            {
                str.Append("<th><div>几天没来店</div></th>");
            }
            //实际这段时间到店客户，只需要查这段时间到过店的
            List<int> acturePatientList = patientBLL.GetPatientListByDateTime(new PatientEntity { HospitalID = entity.HospitalID, StartTime = StartTime1.ToString("yyyy-MM-dd 00:00:01"), EndTime = EndTime1.ToString("yyyy-MM-dd 23:59:59") }).Select(c => c.UserID).ToList();
            int row = 0;
            int pagecount = 0;
            int alldaodianrenci = 0;//到店人次
            int zhengshialldaodianrenci = 0;//正式到店人次
            //  str.Append("<tr style='height:40px;'><td colspan='5'></td></tr>");

            if (Zhouqi == 3)//按月统计
            {
                int interval = EndTime1.Month - StartTime1.Month;
                for (int i = 0; i <= interval; i++)
                {
                    str.AppendFormat("<th>{0}</th>", StartTime1.ToString("yyyy") + "年" + (StartTime1.Month + i) + "月");
                }
                str.Append("<th>合计</th></tr></thead>");
                if (entity.IsKuaDian == 1)
                {
                    entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                }
                entity.numPerPage = 10000;
                //循环客户，筛选客户
                entity.s_StareTime = Convert.ToDateTime("1970-01-01");
                entity.s_Endtime = Convert.ToDateTime("1970-01-01");

                var list = patientBLL.GetPatientList(entity, out row, out pagecount);
                row = list.Where(c => c.HospitalID == entity.HospitalID).Count();//跨店时本店总人数不需要变  汪
                zhengshicount = list.Where(c => c.ArchivesType != "体验客" & c.HospitalID == entity.HospitalID).Count();//本店的正式会员总人数
                var q = from u in list
                        where acturePatientList.Contains(u.UserID)
                        select u;

                list = q.ToList();
                foreach (var info in list)
                {
                    int conditioncount = 0;
                    //过滤合计次数
                    for (int i = 0; i <= interval; i++)
                    {
                        int count = cashBll.GetAllOrderPinlv(new OrderEntity { Type = 3, HospitalID = entity.HospitalID, UserID = info.UserID, StartTime = StartTime1.ToString("yyyy") + '-' + (StartTime1.Month + i) + '-' + StartTime1.Day });
                        conditioncount += count;

                    }
                    //目前退卡和还款算人头不算人次，故人头为一人次可能为0，所以需隐掉一下判断  汪 begin 2019-8-20
                    //if (conditioncount == 0)
                    //{
                    //    continue;
                    //}  汪  end
                    if (entity.StartCount != 0 && entity.EndCount != 0)
                    {
                        if (conditioncount < entity.StartCount || conditioncount >= entity.EndCount)
                        {
                            continue;
                        }
                    }
                    DateTime Lastdatetime;
                    //去最后消费时间
                    if (entity.IsChecked == 1)
                    {
                        int count1 = cashBll.GetAllOrderByDaoDian(new OrderEntity { UserID = info.UserID, StartTime = StartTime1.ToString("yyyy-MM-dd HH:mm:ss"), EndTime = EndTime1.ToString("yyyy-MM-dd HH:mm:ss") });
                        //查询开始时间到结束时间内的最后一笔订单 时间
                        OrderEntity modelinfo = cashBll.GetLastOrderTime(new OrderEntity { UserID = info.UserID, StartTime = StartTime1.ToString(), EndTime = EndTime1.ToString() });
                        if (info.LastTime.Year != 1)
                        {
                            str.Append("<tr>");
                            str.AppendFormat("<td><div>{0}</div></td>", info.MemberNo);
                            str.AppendFormat("<td><div><a href=\"###\" onclick=\"OpenPatientDetail(\'{0}\', 1)\">{1}</a></div></td>", info.UserID, info.UserName);
                            Lastdatetime = modelinfo.CreateTime;
                            str.AppendFormat("<td><div>{0}</div></td>", Lastdatetime);
                            TimeSpan nd = DateTime.Now - Lastdatetime;
                            str.AppendFormat("<td><div>{0}</div></td>", Lastdatetime.Year == 1 ? 365 : Convert.ToInt32(nd.TotalDays));
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        str.Append("<tr>");
                        str.AppendFormat("<td><div>{0}</div></td>", info.MemberNo);
                        str.AppendFormat("<td><div><a href=\"###\" onclick=\"OpenPatientDetail(\'{0}\', 1)\">{1}</a></div></td>", info.UserID, info.UserName);
                        //str.AppendFormat("<td><div>{0}</div></td>", info.HospitalName);
                        Lastdatetime = info.LastTime;
                        str.AppendFormat("<td><div>{0}</div></td>", info.LastTime.ToString("yyyy-MM-dd"));
                        //str.AppendFormat("<td><div>{0}</div></td>", info.HospitalName);
                    }

                    int allcount = 0;
                    for (int i = 0; i <= interval; i++)
                    {
                        //TYPE=3 按月统计
                        int count = cashBll.GetAllOrderPinlv(new OrderEntity { Type = 3, HospitalID = entity.HospitalID, UserID = info.UserID, StartTime = StartTime1.ToString("yyyy") + '-' + (StartTime1.Month + i) + '-' + StartTime1.Day });
                        allcount += count;
                        str.AppendFormat("<td><div>{0}</div></td>", count);
                    }
                    str.AppendFormat("<td><div >{0}</div></td>", allcount);
                    alldaodianrenci += allcount;
                    if (info.ArchivesType != "体验客")
                    {
                        zhengshidaodiancount++;
                        zhengshialldaodianrenci += allcount;
                    }
                    str.Append("</tr>");
                    daodiancount++;
                }
            }
            else if (Zhouqi == 5)
            {
                int interval = EndTime1.Month - StartTime1.Month;
                for (int i = 0; i <= interval; i++)
                {
                    str.AppendFormat("<th>{0}</th>", StartTime1.ToString("yyyy") + "年" + (StartTime1.Month + i) + "月");
                }
                str.Append("<th>合计</th></tr></thead>");
                if (Convert.ToInt32(Request["IsKuaDian"]) == 1)
                {
                    entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                }
                entity.numPerPage = 10000;
                //循环客户，筛选客户
                entity.s_StareTime = Convert.ToDateTime("1970-01-01");
                entity.s_Endtime = Convert.ToDateTime("1970-01-01");
                var list = patientBLL.GetPatientList(entity, out row, out pagecount);
                row = list.Where(c => c.HospitalID == entity.HospitalID).Count();//跨店时本店总人数不需要变  汪
                zhengshicount = list.Where(c => c.ArchivesType != "体验客" & c.HospitalID == entity.HospitalID).Count();//本店正式会员总数
                var q = from u in list
                        where acturePatientList.Contains(u.UserID)
                        select u;
                list = q.ToList();
                foreach (var info in list)
                {
                    int conditioncount = 0;
                    //过滤合计次数
                    for (int i = 0; i <= interval; i++)
                    {
                        int count = cashBll.GetAllOrderPinlv(new OrderEntity { Type = 3, HospitalID = entity.HospitalID, UserID = info.UserID, StartTime = StartTime1.ToString("yyyy") + '-' + ((StartTime1.Month + i) >= 10 ? (StartTime1.Month + i + "") : ("0" + (StartTime1.Month + i))) + '-' + StartTime1.Day });
                        conditioncount += count;
                    }
                    if (conditioncount == 0)
                    {
                        continue;
                    }
                    if (entity.StartCount != 0 && entity.EndCount != 0)
                    {
                        if (conditioncount < entity.StartCount || conditioncount >= entity.EndCount)
                        {
                            continue;
                        }
                    }
                    DateTime Lastdatetime;
                    //去最后消费时间
                    if (entity.IsChecked == 1)
                    {
                        int count1 = cashBll.GetAllOrderByDaoDian(new OrderEntity { UserID = info.UserID, StartTime = StartTime1.ToString("yyyy-MM-dd HH:mm:ss"), EndTime = EndTime1.ToString("yyyy-MM-dd HH:mm:ss") });
                        //查询开始时间到结束时间内的最后一笔订单 时间
                        OrderEntity modelinfo = cashBll.GetLastOrderTime(new OrderEntity { UserID = info.UserID, StartTime = StartTime1.ToString(), EndTime = EndTime1.ToString() });
                        if (info.LastTime.Year != 1)
                        {
                            str.Append("<tr>");
                            str.AppendFormat("<td><div>{0}</div></td>", info.MemberNo);
                            str.AppendFormat("<td><div>{0}</div></td>", info.UserName);
                            Lastdatetime = modelinfo.CreateTime;
                            str.AppendFormat("<td><div>{0}</div></td>", Lastdatetime);
                            TimeSpan nd = DateTime.Now - Lastdatetime;
                            str.AppendFormat("<td><div>{0}</div></td>", Convert.ToInt32(nd.TotalDays));
                        }
                        else
                        {

                            continue;
                        }
                    }
                    else
                    {
                        str.Append("<tr>");
                        str.AppendFormat("<td><div>{0}</div></td>", info.MemberNo);
                        str.AppendFormat("<td><div>{0}</div></td>", info.UserName);
                        Lastdatetime = info.LastTime;
                        str.AppendFormat("<td><div>{0}</div></td>", info.LastTime.ToString("yyyy-MM-dd"));
                    }

                    int allcount = 0;
                    for (int i = 0; i <= interval; i++)
                    {
                        //TYPE=3 按月统计
                        int count = cashBll.GetAllOrderPinlv(new OrderEntity { Type = 3, HospitalID = entity.HospitalID, UserID = info.UserID, StartTime = StartTime1.ToString("yyyy") + '-' + ((StartTime1.Month + i) >= 10 ? (StartTime1.Month + i + "") : ("0" + (StartTime1.Month + i))) + '-' + StartTime1.Day });
                        allcount += count;
                        str.AppendFormat("<td><div>{0}</div></td>", count);
                    }
                    str.AppendFormat("<td><div >{0}</div></td>", allcount);
                    alldaodianrenci += allcount;
                    if (info.ArchivesType != "体验客")
                    {
                        zhengshidaodiancount++;
                        zhengshialldaodianrenci += allcount;
                    }
                    str.Append("</tr>");
                    daodiancount++;
                }
            }
            str.Append("</table>");
            JsonModelEntity model = new JsonModelEntity();
            model.OutputHtml = str.ToString();
            //model.OutputString = " 总客人数 ：" + row;//总人数
            //model.callbackType = " 到店人数 ：" + daodiancount;//到店人数
            //model.forwardUrl = " 到店人次 ：" + alldaodianrenci;//到店人次
            model.rel = " 总客人数（所有档案） ：" + row + "   到店人数 ：" + daodiancount + " 到店人次 ：" + alldaodianrenci + " 到店率 ：" + Math.Round((double)((float)alldaodianrenci / (float)daodiancount), 2);//到店率
            model.forwardUrl = " 总客人数（会员） ：" + zhengshicount + " 到店人数 ：" + zhengshidaodiancount + " 到店人次 ：" + zhengshialldaodianrenci + " 到店率 ：" + Math.Round((double)((float)zhengshialldaodianrenci / (float)zhengshidaodiancount), 2);//到店率
            if (operareType == 1) //导出操作
            {
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "utf-8";
                Response.ContentType = "application/vnd.ms-excel";
                Response.AppendHeader("Content-Disposition", "attachment;filename=DaoDianLv" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls");
                Response.ContentEncoding = Encoding.GetEncoding("utf-8");
                Response.Write(str.ToString());
                Response.Flush();
                Response.End();
                return null;
            }
            else
            {
                //查询操作
                return Json(model);
            }
        }
        ///// <summary>
        ///// 现金消费统计
        ///// </summary>
        ///// <param name="UserID"></param>
        ///// <returns></returns>
        //public ActionResult PatientstatisticsCashxiaofei()
        //{
        //    return View();
        //}

        /// <summary>
        /// 消费汇总页面
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult PatientstatisticsAllxiaofei()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        #endregion

        #region ==档案变更---欠款变更==
        /// <summary>
        /// 顾客欠款列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ArchivesChangeQiankuan(OrderEntity entity)
        {
            entity.IsArchives = 1;
            entity.Statu = 1;
            int rows;
            int pagecount;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            var list = cashBll.GetAllQuanOrder(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;

            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            //if (Request.IsAjaxRequest())
            //    return PartialView("_ArchivesChangeQiankuan", result);
            return View(result);
        }

        /// <summary>
        /// 编辑欠款页面
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public ActionResult EditArchivesQianOrder(string OrderNo, string Type)
        {
            if (Type.ToUpper() == "ADD")
            {
                ViewBag.UserID = 0;
                return View(new OrderEntity());
            }
            else
            {
                var model = cashBll.GetOrderModel(new OrderEntity { OrderNo = OrderNo });
                ViewBag.UserID = model == null ? 0 : model.UserID;
                return View(model);
            }
        }

        #endregion


        #region ==顾客分析--->卡项结余==
        /// <summary>
        /// 门店卡项结余月结(
        /// </summary>
        /// <returns></returns>
        public ActionResult AddMaincardBalanceYuejieByHospital(MaincardBalanceYuejieEntity entity)
        {

            var result = patientBLL.AddMaincardBalanceYuejieByHospital(entity);
            return Json(result);
        }
        /// <summary>
        /// 得到顾客张卡下面的详细卡信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBalanceByUserCardID()
        {
            int UserCardID = Convert.ToInt32(Request["UserCardID"]); //获取用户卡ID

            var list = cashBll.GetAllPaymentOrderProductByCardID(new PaymentOrderProductEntity { CardID = UserCardID });
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<table id='newtable'>");
            foreach (var info in list)
            {

                sb.AppendFormat("<tr>");
                sb.AppendFormat("<td style='width:10%;'>详细:</td>");
                sb.AppendFormat("<td  style='width:15%;'>单据类型:{0}</td>", EYB.Web.Code.PageFunction.GetorderType(info.OrderType));
                sb.AppendFormat("<td style=\" width:20%;\">单据号:{0}</td>", info.DocumentNumber);
                sb.AppendFormat("<td style=\" width:15%;\">订单总额:{0}</td>", info.AllPrice);
                sb.AppendFormat("<td style=\" width:15%;\">消耗次数:{0}</td>", info.Quantity);
                sb.AppendFormat("<td style=\" width:15%;\">支付金额:{0}</td>", info.PayMoney);
                sb.AppendFormat("<td style=\" width:10%;\"></td><td style=\" width:10%;\"></td><td ></td>");
                sb.AppendFormat("</tr>");
            }
            sb.AppendFormat("</table>");

            return Content(sb.ToString());

        }
        /// <summary>
        /// 修改用户余额记录状态
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult UpdateMainCardBalanceStatu(MainCardBalanceEntity entity)
        {
            int result = patientBLL.UpdateMainCardBalanceStatu(entity);
            return Json(result);
        }
        /// <summary>
        /// 消费汇总操作
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult PatientstatisticsAllxiaofeiOperate(OrderEntity entity)
        {

            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }

            //获取顾客分析总数
            var allsum = cashBll.GetAllCustomerQualtitySum(entity);
            //顾客年度消费汇总
            var allxiaofeisum = cashBll.GetAllCustomerQualtityYeji(entity);
            //基础字典查询
            var BaseInfolist = systemBLL.GetBaseInfoTreeByType("CustomerType", 1, 1);
            string result = "";
            int amout = 0;
            foreach (var info in BaseInfolist)
            {
                amout = cashBll.GetAllCustomerQualtity(entity, info.startamout, info.endamout);
                result += amout + ",";
            }
            string result1 = "";
            decimal amout1 = 0;
            foreach (var info in BaseInfolist)
            {
                amout1 = cashBll.GetAllCustomerQualtityYejiSum(entity, info.startamout, info.endamout);
                result1 += amout1 + ",";
            }
            JsonModelEntity res = new JsonModelEntity();
            res.OutputHtml = "[" + result.Substring(0, result.Length - 1) + "]";
            res.OutputString = "[" + result1.Substring(0, result1.Length - 1) + "]";
            ViewBag.HospitalID = entity.HospitalID;
            res.AllPrice = allsum;
            res.ZhePay = allxiaofeisum;
            return Json(res);
        }
        /// <summary>
        /// 卡项结余统计
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult PatientstatisticsCardShengyu(_SeachEntity entity)
        {
            if (entity.s_Name == "姓名、名称、会员卡号") entity.s_Name = "";
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (entity.s_HospitalName == "搜索") entity.s_HospitalName = "";
            //if (entity.s_Endtime.Year > 2000) entity.s_Endtime = entity.s_Endtime.AddDays(1);//默认加一天
            //if (entity.s_Endtime.Year < 2000) entity.s_Endtime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
            //if (entity.s_StareTime.Year < 2000) entity.s_StareTime = Convert.ToDateTime(DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss"));

            int rows;
            int pagecount;
            entity.numPerPage = 50; //每页显示条数
            entity.s_Parent = LoginUserEntity.ParentHospitalID;
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            entity.s_Parent = LoginUserEntity.ParentHospitalID;
            var list = patientBLL.GetCardBalanceByUserId(entity, true, out rows, out pagecount)
                .Where(c => c.Sum_AllTimes > 0 || c.Sum_AllPrice > 0);

            if (entity.s_Endtime > Convert.ToDateTime("1991-01-01 00:00:00") && entity.s_StareTime > Convert.ToDateTime("1991-01-01 00:00:00"))
            {
                list = list.Where( x => x.ExpireTime > entity.s_StareTime && x.ExpireTime < entity.s_Endtime.AddDays(1)).ToList();
            }
            //var sum = patientBLL.GetCardBalanceByUserIdSum(entity);
            //list = list.Where(i=>((i.Type==2&&i.Sum_Tiems>0) ||  (i.Type==1&&i.Sum_Price>0)||(i.Type==3&&i.Sum_Tiems>0) )).ToList();
            //list = list.Where(i => i.Price > 0).ToList();
            var result = list.ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            //*汪*//
            entity.numPerPage = 10000;//为求总数据，将行加大才可查
            var list1 = patientBLL.GetCardBalanceByUserId(entity, true, out rows, out pagecount);
            decimal sumAllPrice = 0;
            decimal sumPrice = 0;
            decimal consumePrice = 0;
            int sumAllTimes = 0;
            int sumTiems = 0;
            foreach (MainCardBalanceEntity model in list1)
            {
                sumAllPrice = sumAllPrice + model.Sum_AllPrice;
                sumPrice = sumPrice + model.ConsumePrice;// Sum_Price;
                sumAllTimes = sumAllTimes + model.Sum_AllTimes;
                consumePrice = consumePrice + (model.Sum_AllTimes == 0 ? model.Sum_Price : model.ConsumePrice * model.Sum_Tiems);
                sumTiems = sumTiems + model.Sum_Tiems;
            }
            ViewBag.Sum_AllPrice = sumAllPrice;
            ViewBag.Sum_Price = sumPrice;
            ViewBag.ConsumePrice = consumePrice;
            ViewBag.Sum_AllTimes = sumAllTimes;
            ViewBag.Sum_Tiems = sumTiems;
            //ViewBag.Sum_AllPrice = sum.Sum_AllPrice;
            //ViewBag.Sum_Price = sum.Sum_Price;
            //ViewBag.ConsumePrice = sum.ConsumePrice;//消耗金额
            //ViewBag.Sum_AllTimes = sum.Sum_AllTimes;
            //ViewBag.Sum_Tiems = sum.Sum_Tiems;
            //*汪*//
            //entity.numPerPage = 9999999;
            //var sumlist = patientBLL.GetCardBalanceByUserID(entity, out rows, out pagecount);
            //decimal sumprice = 0;
            //foreach (var info in sumlist)
            //{
            //    if (info.Sum_AllTimes == 0)
            //        sumprice += info.Sum_Price;
            //    else
            //        sumprice += info.Sum_AllPrice / info.Sum_AllTimes * info.Sum_Tiems;
            //}
            if (Request.IsAjaxRequest())
                return PartialView("_ShengyuList", result);
            Response.Buffer = true;
            Response.ExpiresAbsolute = System.DateTime.Now.AddSeconds(-1);
            Response.Expires = 0;
            Response.CacheControl = "no-cache";
            return View(result);
        }
        public ActionResult _ShengyuList()
        {
            return View();
        }
        /// <summary>
        /// 顾客流失率
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult PatientstatisticsLost(_SeachEntity entity)
        {
            if (entity.s_Keyword == "姓名、手机、顾问姓名")
                entity.s_Keyword = "";
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            if (entity.s_HospitalID == 0 && !Request.IsAjaxRequest())
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }

            if (entity.s_HospitalID == 0)
            {
                entity.s_Parent = LoginUserEntity.ParentHospitalID;
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            //if (entity.pageNum <= 1)
            //{
            //    if (!Request.IsAjaxRequest())
            //    {
            //        return View(new List<PatientstatisticsLostEnrity>().AsQueryable().ToPagedList(1, 10));
            //    }
            //}
            //这2个时间做预备,防止时间越界
            if (entity.s_StareTime == DateTime.MinValue)
            {
                entity.s_StareTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
            }
            if (entity.s_Endtime == DateTime.MinValue)
            {
                entity.s_Endtime = DateTime.Now;
            }
            int rows = 0; int pagecount = 0;
            decimal sumyue = 0.00M;
            if (entity.s_TableType > 0)
            {
                switch (entity.s_TableType)
                {
                    case 1: entity.s_StareTime = DateTime.Now.AddMonths(-1); entity.s_Endtime = DateTime.Now; break;
                    case 2: entity.s_StareTime = DateTime.Now.AddMonths(-3); entity.s_Endtime = DateTime.Now.AddMonths(-1); break;
                    case 3: entity.s_StareTime = DateTime.Now.AddMonths(-6); entity.s_Endtime = DateTime.Now.AddMonths(-3); break;
                    case 4: entity.s_StareTime = DateTime.Now.AddMonths(-12); entity.s_Endtime = DateTime.Now.AddMonths(-6); break;
                    case 5: entity.s_StareTime = DateTime.Now.AddYears(-20); entity.s_Endtime = DateTime.Now.AddYears(-1); break;
                    default: break;
                }
            }
            entity.orderDirection = entity.orderDirection == "" ? "asc" : entity.orderDirection;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "LeaveDays"; entity.orderDirection = "desc"; }
            var list = patientBLL.GetLostList(entity, out rows, out pagecount, out sumyue);
            //  var newlist = list.OrderBy(i => i.LeaveDays).ToList();
            var result = list.AsQueryable().ToPagedList(entity.pageNum, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.s_StareTime = entity.s_StareTime.ToString("yyyy-MM-dd");
            ViewBag.s_Endtime = entity.s_Endtime.ToString("yyyy-MM-dd");
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PatientstatisticsLost", result);
            }
            else
            {
                return View(result);
            }
        }
        #endregion

        #region ==顾客分析--->现金统计===

        /// <summary>
        /// 现金统计
        /// 现金统计现在不需要分类统计了,故不用传TYPE类型.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult PatientstatisticsCashxiaofei(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));


            entity.StartTime = entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.Endtime = entity.Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.Endtime.ToString("yyyy-MM-dd 23:59:59"));
            ViewBag.st = entity.s_StareTime.ToString("yyyy-MM-dd");
            ViewBag.et = entity.s_Endtime.ToString("yyyy-MM-dd");
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<PaymentOrderEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            int rows;
            int pagecount;
            decimal xiaofei = 0; decimal shihao = 0;
            entity.orderDirection = entity.orderDirection + "" == "" ? "desc" : entity.orderDirection;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "SumPayMoney"; }
            var list = patientBLL.GetUserPayMent(entity, out rows, out pagecount, out xiaofei, out shihao); //求出现金的总额
            //decimal allmoney = patientBLL.GetUserPayMentSum(entity);
            var result = list.ToPagedList(1, entity.numPerPage);

            //**汪**/
            entity.numPerPage = 10000;
            var list1 = patientBLL.GetUserPayMent(entity, out rows, out pagecount, out xiaofei, out shihao); //求出现金的总额
            decimal sumPayMoney = 0;
            decimal payMoney = 0;
            decimal allXianJin = 0;
            foreach (PaymentOrderEntity model in list1)
            {
                sumPayMoney += model.SumPayMoney;
                payMoney += model.PayMoney;
                allXianJin += model.AllXianJin;
            }
            ViewBag.SumPayMoney = sumPayMoney;
            ViewBag.PayMoney = payMoney;
            ViewBag.AllXianJin = allXianJin;
            //**汪**/

            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;

            ViewBag.xiaofei = xiaofei;
            ViewBag.shihao = shihao;

            if (Request.IsAjaxRequest())
                return PartialView("_XiaoFeiList", result);
            return View(result);
        }

        public ActionResult _XiaoFeiList()
        {
            return View();
        }


        /// <summary>
        /// 得到顾客得到现金消费详细信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetuserPaymentinfo()
        {
            int UserID = Convert.ToInt32(Request["UserID"]); //获取用户卡ID
            var st = Request["st"];//开始时间
            var et = Request["et"];//结束时间
            DateTime Stime = (st == null ? DateTime.Now : Convert.ToDateTime(st));
            DateTime Etime = (et == null ? DateTime.Now.AddMonths(-3) : Convert.ToDateTime(et));
            Stime = Convert.ToDateTime(Stime.Year + "-" + Stime.Month + "-" + Stime.Day + " 01:01:01");
            Etime = Convert.ToDateTime(Etime.Year + "-" + Etime.Month + "-" + Etime.Day + " 23:59:59");
            int s_HospitalID = ConvertValueHelper.ConvertIntValue(Request["s_HospitalID"]);
            var list = patientBLL.GetuserPaymentinfo(UserID).Where(c => c.HospitalID == s_HospitalID);
            list = list.Where(i => i.CreateTime > Stime).ToList();
            list = list.Where(i => i.CreateTime < Etime).ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<table id='newtable'>");
            foreach (var info in list)
            {
                if (info.PayType == 1 || info.PayType == 2)
                {
                    sb.AppendFormat("<tr>");
                    sb.AppendFormat("<td style=\"width:15%;\" ><a href='###' onclick=\"OpenOrderDetail('{0}')\">{1}</a></td>", info.OrderNo, info.DocumentNumber);
                    sb.AppendFormat("<td style=\"width:20%;\">业务类型:{0}</td>", EYB.Web.Code.PageFunction.GetorderType(info.OrderType));
                    sb.AppendFormat("<td style=\"width:20%;\">时间:{0}</td>", info.CreateTime);
                    sb.AppendFormat("<td style=\"width:20%;\" >现金:{0}</td>", info.PayType == 1 ? info.PayMoney : 0);
                    sb.AppendFormat("<td style=\"width:20%;\" >银联卡:{0}</td>", info.PayType == 2 ? info.PayMoney : 0);
                    sb.AppendFormat("<td></td>");
                    sb.AppendFormat("</tr>");
                }
            }
            sb.AppendFormat("</table>");

            return Content(sb.ToString());

        }

        #endregion
        #region ==储值卡销售统计==
        /// <summary>
        ///    获取统计列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult PatientstatisticsCardxiaoshou(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;//如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.s_HospitalID == -1) entity.s_HospitalID = LoginUserEntity.HospitalID;//前台接收的如果是-1,则代表是没有选的,那就把门店ID更换为自己的
            if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID == LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = 0;//这里为-999时默认为0,就是全部的意思
                var users = personBLL.GetHospitalListByParentID(LoginUserEntity.HospitalID);
                string u = "";
                foreach (var user in users)
                {
                    u = u + "c.HospitalID = '" + user.ID + "' or ";
                }
                entity.s_Keyword = " and (" + u + " 1=1 )";
            }
            else if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID != LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.s_HospitalName == "搜索") entity.s_HospitalName = "";
            if (entity.s_Endtime.Year > 2000) entity.s_Endtime = entity.s_Endtime.AddDays(1);//默认加一天
            if (entity.s_Endtime.Year < 2000) entity.s_Endtime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
            if (entity.s_StareTime.Year < 2000) entity.s_StareTime = Convert.ToDateTime(DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss"));
            int rows;
            int pagecount;
            entity.numPerPage = 10; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = patientBLL.GetCardSellList(entity, out rows, out pagecount); //求出现金的总额
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            var sum = patientBLL.GetCardSellSum(entity);
            ViewBag.sq = sum.SumQuantity;
            ViewBag.sp = sum.SumPrice;
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.st = entity.s_StareTime.ToString("yyyy-MM-dd");
            ViewBag.et = entity.s_Endtime.ToString("yyyy-MM-dd");
            if (Request.IsAjaxRequest())
                return PartialView("_CardXiaoShouList", result);
            return View(result);
        }
        /// <summary>
        /// 子页面
        /// </summary>
        /// <returns></returns>
        public ActionResult _CardXiaoShouList()
        {
            return View();
        }
        /// <summary>
        /// 得到销售详细信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPayinfoList()
        {
            int UserID = Convert.ToInt32(Request["UserID"]);
            int HospitalID = Convert.ToInt32(Request["HospitalID"]);
            DateTime Endtime = Convert.ToDateTime(Request["Endtime"]);
            DateTime StareTime = Convert.ToDateTime(Request["StareTime"]);
            string MemberNo = Request["MemberNo"];
            int ProjectID = Convert.ToInt32(Request["ProjectID"]);
            int Type = Convert.ToInt32(Request["Type"]);
            decimal Price = Convert.ToDecimal(Request["Price"]);
            _SeachEntity entity = new _SeachEntity();
            entity.s_HospitalID = HospitalID;
            entity.s_Endtime = Endtime.AddDays(1);
            entity.s_StareTime = StareTime;
            entity.s_UserID = UserID;
            entity.s_MemberNo = MemberNo;
            entity.s_ID = ProjectID;
            entity.s_TableType = Type;
            entity.s_Price = Price;
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;//如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == -1) entity.s_HospitalID = 0;//前台接收的如果是-1,则代表是全部的,那就把门店ID更换为0
            var list = patientBLL.GetBuyCardList(entity);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<table id='newtable'>");
            foreach (var info in list)
            {
                sb.AppendFormat("<tr>");
                sb.AppendFormat("<td style=\"width:10%;\">详细:<td>");
                sb.AppendFormat("<td style=\" width:20%;\"><a href='###' onclick=\"OpenOrderDetail('{0}')\">{1}</a></td>", info.OrderNo, info.DocumentNumber);
                sb.AppendFormat("<td style=\"width:20%;\">单据类型:{0}<td>", EYB.Web.Code.PageFunction.GetorderType(info.OrderType));
                sb.AppendFormat("<td  style='width:15%;'>数量:{0}</td>", info.Quantity);
                sb.AppendFormat("<td style=\" width:10%;\">单价:{0}</td>", info.Price);
                sb.AppendFormat("<td style=\" width:20%;\">购买时间:{0}</td>", info.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendFormat("<td></td></tr>");
            }
            sb.AppendFormat("</table>");
            return Content(sb.ToString());
        }
        /// <summary>
        /// 得到项目销售信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPayinfoListForproject()
        {
            int UserID = Convert.ToInt32(Request["UserID"]);
            int HospitalID = Convert.ToInt32(Request["HospitalID"]);
            DateTime Endtime = Convert.ToDateTime(Request["Endtime"]);
            DateTime StareTime = Convert.ToDateTime(Request["StareTime"]);
            string MemberNo = Request["MemberNo"];
            int ProjectID = Convert.ToInt32(Request["ProjectID"]);
            int Type = Convert.ToInt32(Request["Type"]);
            decimal AveragePrice = Convert.ToDecimal(Request["AveragePrice"]);//金额
            _SeachEntity entity = new _SeachEntity();
            entity.s_HospitalID = HospitalID;
            entity.s_Endtime = Endtime.AddDays(1);
            entity.s_StareTime = StareTime;
            entity.s_UserID = UserID;
            entity.s_MemberNo = MemberNo;
            entity.s_ID = ProjectID;
            entity.s_TableType = Type;
            entity.numPerPage = 10000;
            entity.s_AveragePrice = AveragePrice;
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;//如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == -1) entity.s_HospitalID = 0;//前台接收的如果是-1,则代表是全部的,那就把门店ID更换为0
            var list = patientBLL.GetProjectList(entity);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<table id='newtable'>");
            foreach (var info in list)
            {
                sb.AppendFormat("<tr>");
                sb.AppendFormat("<td style=\"width:10%;\">详细:<td>");
                sb.AppendFormat("<td style=\" width:20%;\"><a href='###' onclick=\"OpenOrderDetail('{0}')\">{1}</a></td>", info.OrderNo, info.DocumentNumber);
                sb.AppendFormat("<td style=\"width:20%;\">单据类型:{0}<td>", EYB.Web.Code.PageFunction.GetorderType(info.OrderType));
                sb.AppendFormat("<td  style='width:15%;'>数量:{0}</td>", info.Quantity);
                sb.AppendFormat("<td style=\" width:10%;\">单价:{0}</td>", info.Price);
                sb.AppendFormat("<td style=\" width:20%;\">购买时间:{0}</td>", info.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendFormat("<td></td></tr>");
            }
            sb.AppendFormat("</table>");
            return Content(sb.ToString());
        }
        #endregion
        #region ==项目销售统计==
        /// <summary>
        ///    获取统计列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult PatientstatisticsProjectxiaoshou(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;//如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.s_HospitalID == -1) entity.s_HospitalID = LoginUserEntity.HospitalID;//前台接收的如果是-1,则代表是没有选的,那就把门店ID更换为自己的
            if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID == LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = 0;//这里为-999时默认为0,就是全部的意思
                var users = personBLL.GetHospitalListByParentID(LoginUserEntity.HospitalID);
                string u = "";
                foreach (var user in users)
                {
                    u = u + "d.HospitalID = '" + user.ID + "' or ";
                }
                entity.s_Keyword = " and (" + u + " 1=1 )";
            }
            else if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID != LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.s_Endtime.Year > 2000) entity.s_Endtime = entity.s_Endtime.AddDays(1);//默认加一天
            if (entity.s_Endtime.Year < 2000) entity.s_Endtime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
            if (entity.s_StareTime.Year < 2000) entity.s_StareTime = Convert.ToDateTime(DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss"));
            int rows;
            int pagecount;
            entity.numPerPage = 10; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = patientBLL.GetProjectSellList(entity, out rows, out pagecount); //求出现金的总额
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            var sum = patientBLL.GetProjectSellListSum(entity);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.sumallprice = sum.AllPrice;
            ViewBag.sumqu = sum.Quantity;
            if (Request.IsAjaxRequest())
                return PartialView("_ProjectXiaoShou", result);
            return View(result);
        }
        ///// <summary>
        ///// 获取项目销售总和
        ///// </summary>
        ///// <param name="entity"></param>
        ///// <returns></returns>
        //public ActionResult GetProjectxiaoshouSum(_SeachEntity entity)
        //{
        //    if (entity.s_HospitalID == 0) entity.s_HospitalID = loginUserEntity.HospitalID;//如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
        //    ViewBag.ParentHospitalID = loginUserEntity.ParentHospitalID;
        //    if (entity.s_HospitalID == -1) entity.s_HospitalID = loginUserEntity.HospitalID;//前台接收的如果是-1,则代表是没有选的,那就把门店ID更换为自己的
        //    if (entity.s_HospitalID == -999 && loginUserEntity.HospitalID == loginUserEntity.ParentHospitalID)
        //    {
        //        entity.s_HospitalID = 0;//这里为-999时默认为0,就是全部的意思
        //        var users = personBLL.GetHospitalListByParentID(loginUserEntity.HospitalID);
        //        string u = "";
        //        foreach (var user in users)
        //        {
        //            u = u + "d.HospitalID = '" + user.ID + "' or ";
        //        }
        //        entity.s_Keyword = " and (" + u + " 1=1 )";
        //    }
        //    else if (entity.s_HospitalID == -999 && loginUserEntity.HospitalID != loginUserEntity.ParentHospitalID)
        //    {
        //        entity.s_HospitalID = loginUserEntity.HospitalID;
        //    }

        //    if (entity.s_Endtime.Year > 2000) entity.s_Endtime = entity.s_Endtime.AddDays(1);//默认加一天
        //    if (entity.s_Endtime.Year < 2000) entity.s_Endtime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
        //    if (entity.s_StareTime.Year < 2000) entity.s_StareTime = Convert.ToDateTime(DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss"));
        //    int rows;
        //    int pagecount;

        //    entity.numPerPage = 10; //每页显示条数
        //    if (entity.orderField + "" == "") { entity.orderField = "ID"; }
        //    var list = patientBLL.GetProjectSellListSum(entity); //求出现金的总额
        //    var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
        //    //result.TotalItemCount = rows;
        //    //result.CurrentPageIndex = entity.pageNum;
        //    //ViewBag.orderField = entity.orderField;
        //    //ViewBag.orderDirection = entity.orderDirection;
        //    //if (Request.IsAjaxRequest())
        //    //    return PartialView("_ProjectXiaoShou", result);
        //    //return View(result);
        //}

        #endregion

        #region==单据导入查询==
        public ActionResult OrderImport(OrderEntity entity)
        {
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            entity.Statu = 1;
            entity.PingzhengNo = "123456";//只要这个值存在，那么就查询存的凭证号单据
            decimal QianPrice = 0;
            var list = cashBll.GetAllOrder(entity, out QianPrice);
            var sumprice = cashBll.GetSumOrder(entity);  //获取总订单金额
            decimal pageSum = 0;
            ViewBag.SumPrice = sumprice;
            ViewBag.PageSum = list.Select(c => c.Price).Sum();

            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = entity.Rows;
            result.CurrentPageIndex = entity.pageNum;

            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            if (Request.IsAjaxRequest())
                return PartialView("_OrderImport", result);
            return View(result);
        }
        /// <summary>
        /// 导入单据号
        /// </summary>
        /// <returns></returns>
        public ActionResult ImportOrderInfo()
        {
            return View();
        }

        #endregion

        #region==顾客档案变更==
        /// <summary>
        /// 顾客档案变更
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientArchivesChange()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }
        //变更储值卡页面
        public ActionResult UpdateMainCardBalancePage()
        {
            return View();
        }
        public ActionResult UpdateProjectBalancePage()
        {
            return View();
        }
        /// <summary>
        /// 选择客户
        /// </summary>
        /// <returns></returns>
        public ActionResult _PatientSelect()
        {
            return PartialView();
        }
        /// <summary>
        /// 欠款变更方法
        /// </summary>
        /// <returns></returns>
        public ActionResult EditQiankuan(int UserID, decimal Price, string memo)
        {
            if (UserID > 0 && Price > 0)
            {
                OrderEntity order = new OrderEntity();
                order.OrderNo = RandomHelper.GetRandomOrder(DateTime.Now);
                order.OrderType = 2;
                order.ActurePrice = 0;
                order.QianPrice = -Price;
                order.AllQianprice = -Price;
                order.BuyType = 1;
                order.CreateTime = DateTime.Now;
                order.HospitalID = LoginUserEntity.HospitalID;
                order.HuanPrice = 0;
                order.ISAudit = 2;
                order.IsHalf = 0;
                order.Name = memo;
                order.OperateType = 2;
                order.Price = Price;
                order.ReservationType = 1;
                order.Statu = 1;
                order.IsArchives = 1;
                order.UserID = UserID;
                order.DocumentNumber = GetDocumentNumber();
                var result = cashBll.AddOrder(order);
                if (result != null)
                {
                    ArchivesChangeLogEntity acl = new ArchivesChangeLogEntity();
                    acl.UserID = UserID;
                    acl.UpdateStaffID = LoginUserEntity.UserID;
                    acl.ChangeClass = 3;
                    acl.ChangeType = ArchivesChangeLogEntity.ChangeTypeEnum.Add.ToString();
                    acl.OrderNo = order.OrderNo;
                    acl.newResidualFrequency = order.QianPrice;
                    patientBLL.AddArchivesChangeLog(acl);
                }
                return Json(result);
            }
            else
            {
                return Json(-1);
            }
        }
        /// <summary>
        /// 作废单据
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <returns></returns>
        public ActionResult DelQiankuan(string OrderNo)
        {
            if (OrderNo != null)
            {
                var model = cashBll.GetOrderModel(new OrderEntity { OrderNo = OrderNo });
                model.Statu = 3;
                var result = cashBll.UpdateOrder(model);
                if (result > 0)
                {
                    ArchivesChangeLogEntity acl = new ArchivesChangeLogEntity();
                    acl.UserID = model.UserID;
                    acl.UpdateStaffID = LoginUserEntity.UserID;
                    acl.ChangeClass = 3;
                    acl.ChangeType = ArchivesChangeLogEntity.ChangeTypeEnum.Delete.ToString();
                    acl.OrderNo = OrderNo;
                    acl.ResidualFrequency = model.Price;
                    patientBLL.AddArchivesChangeLog(acl);
                }
                return Json(result);
            }
            else
            {
                return Json(-1);
            }
        }
        private string GetDocumentNumber()
        {
            var mendian = personBLL.HospitalEntityByID(LoginUserEntity.HospitalID);  //获取门店信息,用来获取门店前缀
            string time = DateTime.Now.ToString("MMdd");
            var count = cashBll.GetTodayOrderCount(LoginUserEntity.HospitalID, DateTime.Now);//获取总数
            var result = mendian.Prefix + time + (count + 1).ToString("d3");
            return result;
        }
        /// <summary>
        /// 获取用户的储值卡列表
        /// </summary>
        /// <returns></returns>
        public ActionResult ToGetStoredCardList(int id)
        {
            // var id = Request["id"];//获取用户ID

            MainCardBalanceEntity entity = new MainCardBalanceEntity();
            entity.UserID = Convert.ToInt32(id);
            entity.Type = 1;
            IList<MainCardBalanceEntity> list = new List<MainCardBalanceEntity>();
            list = cashBll.GetMainCardBalanceList(entity);
            list = list.Where(t => t.IsDel != 3).ToList();
            list = list.Where(t => t.Price>0).ToList();
            StringBuilder str = new StringBuilder();
            foreach (var info in list)
            {
                str.Append("<tr class='cashpricetr'>");
                //str.AppendFormat("<td><input type='checkbox' name='checkboxorder2' class='thebox' value='{0}' code='{1}' />", info.ID, info.CardID);
                str.AppendFormat("<input type='hidden' class='val' value='{0}' /> </td>", info.ID + "|" + info.Price + "|" + info.ProjectID + "|" + info.Name);
                str.AppendFormat("<td>{0}</td>", info.Name);
                str.AppendFormat("<td>{0}</td>", info.Price);
                str.AppendFormat("<td><a href='###' class='btnedit' code='{0}'>变 更</a></td>", info.ID);
                str.Append("</tr>");
            }
            return Content(str.ToString());
        }
        /// <summary>
        /// 办卡--储值卡
        /// </summary>
        /// <param name="discountlist">划扣折率集合</param>
        /// <param name="pricesList">储值卡原价集合</param>
        /// <param name="userid">用户ID</param>
        /// <param name="cardIdList">储值卡ID集合</param>
        /// <param name="tpricelist">卡余额集合</param>
        /// <returns></returns>
        [WriteOperateLogAttribute("办理储值卡，用户编号：", "办理储值卡", "档案变更")]
        public JsonResult AddCard1(List<decimal> discountlist, List<decimal> pricesList, int userid, List<int> cardIdList, List<decimal> tpricelist, string Memo)
        {
            #region 验证储值卡是否已存在
            var list = cashBll.GetMainCardBalanceList(new MainCardBalanceEntity()
            {
                UserID = userid,
                Type = 1
            }).Select(x => new { x.ProjectID, x.Name,x.IsDel,x.Price }).ToList();
           // list 
            foreach (var cardId in cardIdList)
            {
                list = list.Where(t=>t.Price>0&&t.IsDel!=3).ToList();
                var card = list.FirstOrDefault(x => x.ProjectID == cardId);
                if (card != null) return Json(AjaxResult.Error("储值卡已存在"));
            }
            #endregion

            var acl = new ArchivesChangeLogEntity
            {
                UserID = userid,
                UpdateStaffID = LoginUserEntity.UserID,
                ChangeClass = 2,
                ChangeType = ArchivesChangeLogEntity.ChangeTypeEnum.Add.ToString(),
                Memo = Memo
            };
            var cardlist = "";
            var maincardblancelist = "";
            var cardname = "";
            //操作日志参数
            ViewBag.CacheData = userid;
            long result = 0;
            if (cardIdList.Any())
            {
                for (int i = 0; i < cardIdList.Count; i++)
                {
                    cardlist += cardIdList[i] + ",";
                    var mod = Ibll.GetPrePayCardModel(cardIdList[i]);
                    cardname += mod.Name + ",";
                    UserCardEntity userCard = new UserCardEntity
                    {
                        CardID = cardIdList[i],
                        CreateTime = DateTime.Now,
                        IsActive = 1,
                        UserID = userid,
                        Price = pricesList[i]
                    };
                    var userCardId = cashBll.AddUserCard(userCard);//添加购卡关系
                    if (userCardId > 0)//添加余额记录
                    {
                        MainCardBalanceEntity cardBalance = new MainCardBalanceEntity
                        {
                            CardID = userCardId,
                            UserCardID = userCardId,
                            UserID = userid,
                            AllPrice = pricesList[i],
                            Price = tpricelist[i],
                            ProjectID = cardIdList[i],
                            Type = 1,
                            HuaKouZheLv = discountlist[i],
                            AddTime = DateTime.Now
                        };
                        //获取过期时间
                        cardBalance.ExpireTime = cardBalance.AddTime.AddDays(mod.VaildityDay);
                        result = cashBll.AddMainCardBalance(cardBalance);
                        maincardblancelist += result + ",";
                    }
                }
            }
            acl.CourseCardIDList = cardlist;
            acl.MainCardBalanceList = maincardblancelist;
            acl.Name = cardname;
            patientBLL.AddArchivesChangeLog(acl);
            return Json(result > 0 ? AjaxResult.Success() : AjaxResult.Error());
        }
        /// <summary>
        /// 修改用户余额记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [WriteOperateLogAttribute("变更用户余额记录，用户编号：", "变更用户储值卡记录", "档案变更")]
        public ActionResult UpdateMainCardBalanceMoney(MainCardBalanceEntity entity)
        {
            //操作日志参数
            ViewBag.CacheData = entity.UserID + " ;变更金额：" + entity.Price;
            entity.Type = 1;
            // entity.ExpireTime = entity.AddTime.AddDays(entity.VaildityDay);
            var Oldmodel = cashBll.GetMainCardBalanceModel(entity);//获取原来的数据
            int result = cashBll.UpdateMainCardBalanceMoney(entity);
            if (result > 0)
            {
                var model = cashBll.GetMainCardBalanceModel(entity);
                //用户储值卡余额明细日志
                cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                {
                    CardBalanceId = model.ID,
                    OrderNo = "",
                    Type = (int)MainCardBalanceDetailType.变更,
                    Amount = entity.Price,
                    Balance = model.Price,
                    OldAmount = model.Price + entity.Price,
                    CreateTime = DateTime.Now
                });
                #region ==添加变更日志==
                ArchivesChangeLogEntity acl = new ArchivesChangeLogEntity();
                acl.CourseCardIDList = entity.ID.ToString();
                acl.UserID = model.UserID;
                acl.UpdateStaffID = LoginUserEntity.UserID;//修改员工是当前账号
                acl.ChangeClass = 2;
                acl.ChangeType = ArchivesChangeLogEntity.ChangeTypeEnum.Update.ToString();
                acl.Name = model.Name;
                acl.ValidityPeriod = Oldmodel.ExpireTime.ToShortDateString();
                acl.newValidityPeriod = model.ExpireTime.ToShortDateString();
                acl.ConsumptionUnitPrice = Oldmodel.Price;
                acl.newConsumptionUnitPrice = model.Price;
                acl.ResidualFrequency = Oldmodel.HuaKouZheLv;
                acl.newResidualFrequency = model.HuaKouZheLv;
                patientBLL.AddArchivesChangeLog(acl);
                #endregion

                return Json(model.UserID);
            }
            else
            {
                return Json(null);
            }
        }
        /// <summary>
        /// 获取顾客购买疗程卡
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetprojectCardlist(int UserID)
        {
            // var list = cashBll.GetBlanceinproject(UserID, LoginUserEntity.ParentHospitalID)//
            //var list = cashBll.GetBlanceinproject(UserID, LoginUserEntity.ParentHospitalID)//原来
            var list = cashBll.GetBlanceinprojectIsDel(UserID, LoginUserEntity.ParentHospitalID)
           .Where(x => x.Tiems > 0);//赠送的储值卡为零 不显示
            var str = new StringBuilder();
            foreach (var info in list)
            {
                //int alltimes = cashBll.GetTiems(info.ID);
                str.Append("<tr class='cashpricetr'>");
                //str.AppendFormat("<td><input type='checkbox' name='checkboxorder2' value='{0}' code='{1}' /></td>", info.ID, info.CardID);
                str.AppendFormat("<td {1}>{0}</td>", info.ProjectName, (info.ExpireTime <= DateTime.Now ? "style=\"color:red;\"" : ""));
                str.AppendFormat("<td>{0}</td>", info.Tiems);
                str.AppendFormat("<td><a href='###' class='btneditproject' code='{0}'>变 更</a></td>", info.ID);
                str.Append("</tr>");
            }
            return Content(str.ToString());
        }
        /// <summary>
        /// 修改用户余额记录---修改卡次数
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [WriteOperateLogAttribute("变更用户余额记录，用户编号：", "变更用户疗程卡记录", "档案变更")]
        public ActionResult UpdateMainCardBalanceTimeByID(MainCardBalanceEntity entity)
        {
            //操作日志参数
            ViewBag.CacheData = entity.UserID + " ;变更次数：" + entity.Tiems;
            entity.Type = 1;
            var Oldmodel = cashBll.GetMainCardBalanceModel(entity);//获取原来的数据
            // entity.ExpireTime = entity.AddTime.AddDays(entity.VaildityDay);
            int result = cashBll.UpdateMainCardBalanceTimeByID(entity);
            if (result > 0)
            {
                var model = cashBll.GetMainCardBalanceModel(entity);
                #region ==添加变更日志==
                ArchivesChangeLogEntity acl = new ArchivesChangeLogEntity();
                acl.CourseCardIDList = entity.ID.ToString();
                acl.UserID = model.UserID;
                acl.UpdateStaffID = LoginUserEntity.UserID;//修改员工是当前账号
                acl.ChangeClass = 1;
                acl.ChangeType = ArchivesChangeLogEntity.ChangeTypeEnum.Update.ToString();
                acl.Name = model.ProjectName;
                acl.ValidityPeriod = Oldmodel.ExpireTime.ToShortDateString();
                acl.newValidityPeriod = model.ExpireTime.ToShortDateString();
                acl.ConsumptionUnitPrice = Oldmodel.ConsumePrice;
                acl.newConsumptionUnitPrice = model.ConsumePrice;
                acl.ResidualFrequency = Oldmodel.Tiems;
                acl.newResidualFrequency = model.Tiems;
                acl.Memo = entity.MemoContent;
                patientBLL.AddArchivesChangeLog(acl);

                #endregion
                return Json(model.UserID);
            }
            else
            {
                return Json(null);
            }
        }
        /// <summary>
        /// 办卡--疗程卡
        /// </summary>
        /// <returns></returns>
        [WriteOperateLogAttribute("办理储值卡，用户编号：", "办理疗程卡", "档案变更")]
        public ActionResult AddTrentCard(List<string> projectList, List<decimal> pricesList, int userid, List<int> cardidList, string Memo)
        {
            ArchivesChangeLogEntity acl = new ArchivesChangeLogEntity();
            acl.UserID = userid;
            acl.UpdateStaffID = LoginUserEntity.UserID;
            acl.ChangeClass = 1;
            acl.ChangeType = ArchivesChangeLogEntity.ChangeTypeEnum.Add.ToString();
            acl.Memo = Memo;
            var cardlist = "";
            var maincardblancelist = "";
            var cardname = "";
            //操作日志参数
            ViewBag.CacheData = userid;
            long result = 0;
            if (cardidList != null && cardidList.Count > 0)
            {
                for (int i = 0; i < cardidList.Count; i++)
                {
                    //var cardmodel = Ibll.GetPrePayTreatment(cardidList[i]);
                    //cardname += cardmodel.Name + ",";
                    UserCardEntity usercard = new UserCardEntity();
                    usercard.CardID = cardidList[i];
                    usercard.CreateTime = DateTime.Now;
                    usercard.IsActive = 1;
                    usercard.UserID = userid;
                    usercard.Price = pricesList[i];
                    var usercardid = cashBll.AddUserCard(usercard);//添加购卡关系
                    if (usercardid > 0)//添加余额记录
                    {
                        //cardlist += usercard.CardID + ",";
                        cardlist = Convert.ToString(usercard.CardID);
                        string projectstr = projectList[i];
                        if (projectstr != "0") // 查询后台传递过来的数据
                        {
                            //获取疗程卡 项目集合
                            string[] projectarr = projectstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var info in projectarr)
                            {
                                string[] entityarr = info.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                MainCardBalanceEntity cardbalance = new MainCardBalanceEntity();
                                cardbalance.CardID = cardidList[i];
                                cardbalance.UserCardID = usercardid;
                                cardbalance.UserID = userid;

                                //if (entityarr[1] == entityarr[5] || entityarr[5] == "1")
                                //{
                                //    cardbalance.Tiems = ConvertValueHelper.ConvertIntValue(entityarr[1]);
                                //}
                                //else {
                                //    cardbalance.Tiems = ConvertValueHelper.ConvertIntValue(entityarr[5]);
                                //}
                                //20200903
                                if (entityarr.Count() > 6)
                                {
                                    if (entityarr[6] == "0")
                                    {
                                        cardbalance.Tiems = ConvertValueHelper.ConvertIntValue(entityarr[1]);
                                    }
                                    else
                                    {
                                        cardbalance.Tiems = ConvertValueHelper.ConvertIntValue(entityarr[5]);
                                    }
                                }
                                else {
                                    cardbalance.Tiems = ConvertValueHelper.ConvertIntValue(entityarr[5]);
                                }
                                cardbalance.AllTiems = ConvertValueHelper.ConvertIntValue(entityarr[1]);
                                cardbalance.Price = ConvertValueHelper.ConvertDecimalValue(entityarr[3]) / ConvertValueHelper.ConvertDecimalValue(entityarr[1]);
                                cardbalance.ProjectID = ConvertValueHelper.ConvertIntValue(entityarr[0]);
                                cardbalance.Type = 2;
                                cardbalance.AddTime = DateTime.Now;
                                cardbalance.ConsumePrice = ConvertValueHelper.ConvertDecimalValue(entityarr[2]);
                                cardbalance.AllPrice = ConvertValueHelper.ConvertDecimalValue(entityarr[3]);
                                //获取过期时间
                                var mod = Ibll.GetPrePayCardModel(cardidList[i]);
                                cardbalance.ExpireTime = cardbalance.AddTime.AddDays(mod.VaildityDay);
                                result = cashBll.AddMainCardBalance(cardbalance);
                                //maincardblancelist += result + ",";
                                maincardblancelist = Convert.ToString(result);
                                var list = cashBll.GetProjectBalance(userid, Convert.ToInt32(result), LoginUserEntity.ParentHospitalID);
                                foreach (var ofin in list) {
                                    cardname = ofin.Name;
                                }
                                acl.newResidualFrequency= ConvertValueHelper.ConvertIntValue(entityarr[5]);
                                acl.CourseCardIDList = cardlist;
                                acl.MainCardBalanceList = maincardblancelist;
                                acl.Name = cardname;
                                patientBLL.AddArchivesChangeLog(acl);
                            }
                        }
                        else
                        {  //查询疗程卡默认项目
                            var pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = cardidList[i] });
                            foreach (var info in pplist)
                            {
                                MainCardBalanceEntity cardbalance = new MainCardBalanceEntity();
                                cardbalance.CardID = cardidList[i];
                                cardbalance.UserCardID = usercardid;
                                cardbalance.UserID = userid;
                                cardbalance.AllPrice = pricesList[i];
                                cardbalance.Tiems = info.Qualtity;
                                cardbalance.AllTiems = info.Qualtity;
                                cardbalance.Price = info.RealityPrice == 0 ? info.Price : info.RealityPrice;
                                cardbalance.ProjectID = info.ProjectID;
                                cardbalance.Type = 2;
                                cardbalance.ConsumePrice = info.ConsumptionPrice;
                                //获取过期时间
                                var mod = Ibll.GetPrePayCardModel(cardidList[i]);
                                cardbalance.AddTime = DateTime.Now;
                                cardbalance.ExpireTime = cardbalance.AddTime.AddDays(mod.VaildityDay);
                                result = cashBll.AddMainCardBalance(cardbalance);
                                maincardblancelist += result + ",";
                            }
                        }
                    }
                }
            }
            //acl.CourseCardIDList = cardlist;
            //acl.MainCardBalanceList = maincardblancelist;
            //acl.Name = cardname;
            //patientBLL.AddArchivesChangeLog(acl);
            return Json(result);
        }
        #endregion
        #region ==欠款汇总==
        /// <summary>
        /// 欠款汇总页面
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult PatientstatisticsQianKuan(_SeachEntity entity)
        {
            if (entity.s_MemberNo == "姓名、会员卡号") entity.s_MemberNo = "";
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;//如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.s_HospitalID == -1) entity.s_HospitalID = LoginUserEntity.HospitalID;//前台接收的如果是-1,则代表是没有选的,那就把门店ID更换为自己的
            if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID == LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = 0;//这里为-999时默认为0,就是全部的意思
                var users = personBLL.GetHospitalListByParentID(LoginUserEntity.HospitalID);
                string u = "";
                foreach (var user in users)
                {
                    u = u + "d.HospitalID = '" + user.ID + "' or ";
                }
                entity.s_Keyword = " and (" + u + " 1=1 )";
            }
            else if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID != LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.s_Endtime.Year > 2000) entity.s_Endtime = Convert.ToDateTime(entity.s_Endtime.Year + "-" + entity.s_Endtime.Month + "-" + entity.s_Endtime.Day + " 23:59:59");//默认加一天
            if (entity.s_Endtime.Year < 2000) entity.s_Endtime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            if (entity.s_StareTime.Year < 2000) entity.s_StareTime = Convert.ToDateTime(DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss"));
            entity.s_TableType = 1;
            int rows;
            int pagecount;
            decimal sumcard;
            decimal sumconsumer;
            entity.numPerPage = 50000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "UserID"; }
            var list = patientBLL.GetUserArrearsList(entity, out rows, out pagecount, out sumcard, out sumconsumer); //求出现金的总额
            var result = list.AsQueryable().ToPagedList(1, 50000000);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.Type = entity.s_TableType;
            ViewBag.StareTime = entity.s_StareTime.ToString("yyyy-MM-dd hh:MM:ss");
            ViewBag.EndTime = entity.s_Endtime.ToString("yyyy-MM-dd hh:MM:ss");
            ViewBag.sc = sumcard;
            ViewBag.ss = sumconsumer;
            if (Request.IsAjaxRequest())
                return PartialView("_QianKuanList", result);
            return View(result);
        }
        /// <summary>
        /// 欠款详情列表
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetQiankuanList(int UserID, string St, string Et, int Type)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<table id='newtable'>");
            if (Type > 0)
            {
                DateTime stime = Convert.ToDateTime(Convert.ToDateTime(St).Year + "-" + Convert.ToDateTime(St).Month + "-" + Convert.ToDateTime(St).Day + " 00:00:01");
                DateTime etime = Convert.ToDateTime(Convert.ToDateTime(Et).Year + "-" + Convert.ToDateTime(Et).Month + "-" + Convert.ToDateTime(Et).Day + " 23:59:59");
                var list = patientBLL.GetQiankuanOrder(UserID, stime, etime);
                foreach (var info in list)
                {
                    sb.AppendFormat("<tr>");
                    sb.AppendFormat("<td style='width:10%;'>详细:</td>");
                    sb.AppendFormat("<td  style='width:15%;'>时间:{0}", info.CreateTime.ToString("yyyy-MM-dd hh:MM:ss"));
                    sb.AppendFormat("<td style=\" width:25%;\">{0}</td>", EYB.Web.Code.PageFunction.GetorderType(info.OrderType));
                    sb.AppendFormat("<td>欠款总额:{0}</td>", info.QianPrice);
                    sb.AppendFormat("<td></td>");
                    sb.AppendFormat("</tr>");
                }
            }
            else
            {
                var list = patientBLL.GetHuankuanList(UserID);
                foreach (var info in list)
                {
                    sb.AppendFormat("<tr>");
                    sb.AppendFormat("<td style='width:10%;'>详细:</td>");
                    sb.AppendFormat("<td  style='width:15%;'>时间:{0}", info.CreateTime.ToString("yyyy-MM-dd HH:MM:ss"));
                    sb.AppendFormat("<td style=\" width:25%;\">欠款总额:{0}</td>", info.AllQianKuan);
                    sb.AppendFormat("<td>还款金额:{0}</td>", info.RepaymentMoney);
                    sb.AppendFormat("<td>剩余欠款:{0}</td>", info.SurplusQianKuan);
                    sb.AppendFormat("</tr>");
                }
            }
            sb.AppendFormat("</table>");
            return Json(sb.ToString());
        }
        #endregion
        #region===导出==
        /// <summary>
        /// 现金统计
        /// 现金统计现在不需要分类统计了,故不用传TYPE类型.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportPatientstatisticsCashxiaofei(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;//如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.s_HospitalID == -1) entity.s_HospitalID = LoginUserEntity.HospitalID;//前台接收的如果是-1,则代表是没有选的,那就把门店ID更换为自己的
            if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID == LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = 0;//这里为-999时默认为0,就是全部的意思
                var users = personBLL.GetHospitalListByParentID(LoginUserEntity.HospitalID);
                string u = "";
                foreach (var user in users)
                {
                    u = u + "b.HospitalID = '" + user.ID + "' or ";
                }
                entity.s_Keyword = " and (" + u + " 1=1 )";
            }
            else if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID == LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.s_HospitalName == "搜索") entity.s_HospitalName = "";
            if (entity.s_Endtime.Year > 2000) entity.s_Endtime = entity.s_Endtime;
            if (entity.s_Endtime.Year < 2000) entity.s_Endtime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
            if (entity.s_StareTime.Year < 2000) entity.s_StareTime = Convert.ToDateTime(DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss"));
            entity.s_StareTime = Convert.ToDateTime(entity.s_StareTime.Year + "-" + entity.s_StareTime.Month + "-" + entity.s_StareTime.Day + " 00:01:01");
            entity.s_Endtime = Convert.ToDateTime(entity.s_Endtime.Year + "-" + entity.s_Endtime.Month + "-" + entity.s_Endtime.Day + " 23:59:59");
            entity.StartTime = entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.Endtime = entity.Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.Endtime.ToString("yyyy-MM-dd 23:59:59"));
            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            decimal xiaofei = 0; decimal shihao = 0;
            entity.orderDirection = "desc";
            entity.numPerPage = 100000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "SumPayMoney"; }
            var list = patientBLL.GetUserPayMent(entity, out rows, out pagecount, out xiaofei, out shihao); //求出现金的总额
            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("会员卡号"), new DataColumn("姓名"), new DataColumn("消费金额"), new DataColumn("实耗业绩"), new DataColumn("实操") });
            decimal SumPayMoney = 0;
            decimal PayMoney = 0;
            decimal AllXianJin = 0;
            foreach (PaymentOrderEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["会员卡号"] = model.MemberNo;
                row["姓名"] = model.UserName;
                row["消费金额"] = model.SumPayMoney;
                row["实耗业绩"] = model.PayMoney;
                row["实操"] = model.AllXianJin;
                SumPayMoney = SumPayMoney + model.SumPayMoney;
                PayMoney = PayMoney + model.PayMoney;
                AllXianJin = AllXianJin + model.AllXianJin;
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["会员卡号"] = "";
            row1["姓名"] = "合计";
            row1["消费金额"] = SumPayMoney;
            row1["实耗业绩"] = PayMoney;
            row1["实操"] = AllXianJin;
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "xiaofei");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "xiaofei.xls");
        }
        /// <summary>
        /// 卡项结余-导出
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult ExportPatientstatisticsCardShengyu(_SeachEntity entity)
        {
            //if (entity.s_HospitalID == 0) entity.s_HospitalID = loginUserEntity.HospitalID;//如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            //ViewBag.ParentHospitalID = loginUserEntity.ParentHospitalID;
            //if (entity.s_HospitalID == -1) entity.s_HospitalID = 0;//前台接收的如果是-1,则代表是全部的,那就把门店ID更换为0
            if (entity.s_Name == "姓名、名称、会员卡号") entity.s_Name = "";
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;//如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.s_HospitalID == -1) entity.s_HospitalID = LoginUserEntity.HospitalID;//前台接收的如果是-1,则代表是没有选的,那就把门店ID更换为自己的
            if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID == LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = 0;//这里为-999时默认为0,就是全部的意思
                var users = personBLL.GetHospitalListByParentID(LoginUserEntity.HospitalID);
                string u = "";
                foreach (var user in users)
                {
                    u = u + "d.HospitalID = '" + user.ID + "' or ";
                }
                entity.s_Keyword = " and (" + u + " 1=1 )";
            }
            else if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID != LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.s_HospitalName == "搜索") entity.s_HospitalName = "";
            if (entity.s_Endtime.Year > 2000) entity.s_Endtime = entity.s_Endtime.AddDays(1);//默认加一天
            // if (entity.s_Endtime.Year < 2000) entity.s_Endtime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
            //if (entity.s_StareTime.Year < 2000) entity.s_StareTime = Convert.ToDateTime(DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss"));
            int rows;
            int pagecount;
            entity.numPerPage = 10000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            var list = patientBLL.GetCardBalanceByUserId(entity, true, out rows, out pagecount);
            var sum = patientBLL.GetCardBalanceByUserIdSum(entity);

            DataTable tableExport = new DataTable("Export");
            decimal Sum_AllPrice = 0;
            decimal Sum_Price = 0;
            decimal SumShengyu_Price = 0;
            int Sum_AllTimes = 0;
            int Sum_Tiems = 0;
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("会员卡号"), new DataColumn("姓名"), new DataColumn("卡名称"), new DataColumn("过期时间"), new DataColumn("总共金额"), new DataColumn("剩余金额"), new DataColumn("剩余消耗金额"), new DataColumn("总共次数"), new DataColumn("剩余次数") });
            foreach (MainCardBalanceEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["会员卡号"] = model.MemberNo;
                row["姓名"] = model.UserName;
                row["卡名称"] = model.Type == 1 ? model.Name : (model.ProjectName + "(" + model.Name + ")");
                row["过期时间"] = model.ExpireTime;
                row["总共金额"] = model.Sum_AllPrice.ToString("#0.00");
                //row["剩余金额"] = (model.Sum_AllTimes == 0 ? model.Sum_Price : model.Sum_AllPrice / model.Sum_AllTimes * model.Sum_Tiems).ToString("#0.00");
                row["剩余金额"] = model.Sum_Price.ToString("#0.00");
                row["剩余消耗金额"] = (model.Sum_AllTimes == 0 ? model.Sum_Price : model.ConsumePrice * model.Sum_Tiems).ToString("#0.00");
                row["总共次数"] = model.Sum_AllTimes;
                row["剩余次数"] = model.Sum_Tiems;
                Sum_AllPrice = Sum_AllPrice + model.Sum_AllPrice;
                //Sum_Price = Sum_Price + (model.Sum_AllTimes == 0 ? model.Sum_Price : model.Sum_AllPrice / model.Sum_AllTimes * model.Sum_Tiems);
                Sum_Price = Sum_Price + model.Sum_Price;
                Sum_AllTimes = Sum_AllTimes + model.Sum_AllTimes;
                SumShengyu_Price = SumShengyu_Price + (model.Sum_AllTimes == 0 ? model.Sum_Price : model.ConsumePrice * model.Sum_Tiems);
                Sum_Tiems = Sum_Tiems + model.Sum_Tiems;
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["会员卡号"] = "";
            row1["姓名"] = "";
            row1["卡名称"] = "";
            row1["过期时间"] = "合计";
            // row1["卡名"] = "合计";
            row1["总共金额"] = Sum_AllPrice;
            row1["剩余金额"] = Sum_Price;
            row1["剩余消耗金额"] = SumShengyu_Price;
            row1["总共次数"] = Sum_AllTimes;
            row1["剩余次数"] = Sum_Tiems;
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "JieYu");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "JieYu.xls");
        }
        /// <summary>
        /// 欠款汇总页面---导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportPatientstatisticsQianKuan(_SeachEntity entity)
        {
            if (entity.s_MemberNo == "姓名、会员卡号") entity.s_MemberNo = "";
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;//如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.s_HospitalID == -1) entity.s_HospitalID = LoginUserEntity.HospitalID;//前台接收的如果是-1,则代表是没有选的,那就把门店ID更换为自己的
            if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID == LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = 0;//这里为-999时默认为0,就是全部的意思
                var users = personBLL.GetHospitalListByParentID(LoginUserEntity.HospitalID);
                string u = "";
                foreach (var user in users)
                {
                    u = u + "d.HospitalID = '" + user.ID + "' or ";
                }
                entity.s_Keyword = " and (" + u + " 1=1 )";
            }
            else if (entity.s_HospitalID == -999 && LoginUserEntity.HospitalID != LoginUserEntity.ParentHospitalID)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.s_Endtime.Year > 2000) entity.s_Endtime = Convert.ToDateTime(entity.s_Endtime.Year + "-" + entity.s_Endtime.Month + "-" + entity.s_Endtime.Day + " 23:59:59");//默认加一天
            if (entity.s_Endtime.Year < 2000) entity.s_Endtime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            if (entity.s_StareTime.Year < 2000) entity.s_StareTime = Convert.ToDateTime(DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss"));
            int rows;
            int pagecount;
            decimal sumcard;
            decimal sumconsumer;
            entity.numPerPage = 50000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "UserID"; }
            var list = patientBLL.GetUserArrearsList(entity, out rows, out pagecount, out sumcard, out sumconsumer); //求出现金的总额
            var qiankuan = 0m;
            var xiaofeiqiankuan = 0m;
            foreach (var info in list)
            {
                qiankuan += info.CardArrears;
                xiaofeiqiankuan += info.ConsumerArrears;
            }
            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("会员卡号"), new DataColumn("姓名"), new DataColumn("储值卡欠款"), new DataColumn("消费欠款") });
            foreach (UserArrearsEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["会员卡号"] = model.MemberNo;
                row["姓名"] = model.UserName;
                row["储值卡欠款"] = model.CardArrears;
                row["消费欠款"] = model.ConsumerArrears;
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["会员卡号"] = "";
            row1["姓名"] = "合计";
            row1["储值卡欠款"] = list.Sum(i => i.CardArrears);
            row1["消费欠款"] = xiaofeiqiankuan;
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "QianKuan");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "QianKuan.xls");
        }
        /// <summary>
        /// 顾客详情---用户储值卡导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportPatientChuzhiCard(MainCardBalanceEntity entity)
        {
            entity.Type = 1;
            var list = cashBll.GetMainCardBalanceList(entity);
            //var result = list.AsQueryable().ToPagedList(1, 10000);
            var usermodel = patientBLL.GetPatienttEntityByID(entity.UserID);
            decimal Sumchongzhi = 0;
            decimal Sumxiaofei = 0;
            decimal Sumyu_e = 0;
            var name = "";
            if (usermodel != null)
            {
                name = usermodel.UserName;
            }

            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("顾客姓名"), new DataColumn("卡项名称"), new DataColumn("充值金额"), new DataColumn("消费金额"), new DataColumn("余额"), new DataColumn("是否赠卡"), new DataColumn("购卡时间"), new DataColumn("备注") });
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["顾客姓名"] = name;
                row["卡项名称"] = model.Name;
                row["充值金额"] = model.chongzhiPrice;
                row["消费金额"] = model.xiaofeiPrice;
                row["余额"] = model.Price;
                row["是否赠卡"] = model.IsGive == 1 ? "是" : "否";
                row["购卡时间"] = model.BuyCardTime.ToString("yyyy-MM-dd");
                row["备注"] = model.Memo;
                tableExport.Rows.Add(row);
                Sumchongzhi += model.chongzhiPrice;
                Sumxiaofei += model.xiaofeiPrice;
                Sumyu_e += model.Price;
            }
            DataRow row1 = tableExport.NewRow();
            row1["卡项名称"] = "合计";
            row1["充值金额"] = Sumchongzhi;
            row1["消费金额"] = Sumxiaofei;
            row1["余额"] = Sumyu_e;
            tableExport.Rows.Add(row1);

            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "Chizhika");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "Chizhika.xls");

        }
        /// <summary>
        /// 疗程卡导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportProjectCard(MainCardBalanceEntity entity)
        {
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var list = cashBll.GetProjectTimesSel(entity);
            var usermodel = patientBLL.GetPatienttEntityByID(entity.UserID);

            var name = "";
            if (usermodel != null)
            {
                name = usermodel.UserName;
            }

            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("顾客姓名"), new DataColumn("卡项名称"), new DataColumn("项目名称"), new DataColumn("购买单价"), new DataColumn("消耗单价"), new DataColumn("总次数"), new DataColumn("消费次数"), new DataColumn("结余"), new DataColumn("购卡时间"), new DataColumn("备注") });
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["顾客姓名"] = name;
                row["卡项名称"] = model.Name;
                row["项目名称"] = model.ProjectName;
                row["购买单价"] = model.Price;
                row["消耗单价"] = model.ConsumePrice;
                row["总次数"] = model.AllTiems;
                row["消费次数"] = model.AllTiems - model.Tiems;
                row["结余"] = model.Tiems;
                row["购卡时间"] = model.BuyCardTime.ToString("yyyy-MM-dd");
                row["备注"] = model.Memo;
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "LiaoChengKa");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "LiaoChengKa.xls");
        }
        /// <summary>
        /// 导出未消耗
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportPatientInfoNoxiaohao(PatientEntity entity)
        {
           
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.IsActive == 0)
            {
                entity.IsActive = 1;
            }
            if (entity.HospitalID == LoginUserEntity.ParentHospitalID)
            {
                entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            }
            if (Request.IsAjaxRequest())
            {
                if (entity.HospitalID == 0)
                {
                    entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                }
            }
            /*entity.numPerPage = 50; *///每页显示条数
            entity.numPerPage = 700;
            if (entity.pageNum == 0)
                entity.pageNum = 1;

            var UserName = Request["UserName"];
            int userID = Convert.ToInt32(Request["UserID"]);
            var orderField = entity.orderField;
            if (orderField == null)
            {
                orderField = "Sum_Price,UserId";
            }
            if (orderField.IndexOf("UserId") < 0)
            {
                orderField += ",UserId";
            }
            var orderDirection = entity.orderDirection;
            if (orderDirection == null)
            {
                orderDirection = "desc";
            }
            int rows;
            int pagecount;
            int ZongSum_AllTimes;
            decimal ZongRetailPrice;
            decimal ZongSum_Price;
            decimal ZongSum_AllPrice;
            int ZongNumber;
            int ZongSum_Tiems;

            int Sum_AllTimes=0;
            decimal RetailPrice=0;
            decimal Sum_Price = 0;
            decimal Sum_AllPrice = 0;
            int Number0 = 0;
            int Sum_Tiems = 0;
            var list = patientBLL.StatisticsPatientNotExpenseNew(entity.HospitalID, UserName, orderField, orderDirection, entity.numPerPage, entity.pageNum, out rows, out pagecount, out ZongSum_AllTimes, out ZongRetailPrice, out ZongSum_Price, out ZongSum_AllPrice, out ZongNumber, out ZongSum_Tiems, userID);
            //var list = patientBLL.StatisticsPatientNotExpense(entity.HospitalID, UserName, orderField, orderDirection, entity.numPerPage, entity.pageNum, out rows, out pagecount, out ZongSum_AllTimes, out ZongRetailPrice, out ZongSum_Price, out ZongSum_AllPrice, out ZongNumber, out ZongSum_Tiems);
            //汪 2019-10-23 begin 屏蔽一些顾客未消耗统计
            // var result= list;
            // var result = list.Where(c => c.AllTiems > 0 || c.Sum_Price > 0 || c.Sum_AllPrice > 0).ToPagedList(1, entity.numPerPage);
            //result.TotalItemCount = rows;
            var result = list.ToPagedList(1, entity.numPerPage);
            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] {
                 new DataColumn("客户名称"),new DataColumn("客户卡号"), new DataColumn("疗程卡余次"), new DataColumn("疗程卡余额"),
                new DataColumn("储值卡"), new DataColumn("赠卡"), new DataColumn("未到天数"), new DataColumn("未销售天数")
                });
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["客户名称"] = model.UserName;
                row["客户卡号"] = model.MemberNo;
                row["疗程卡余次"] = model.Sum_AllTimes;
                row["疗程卡余额"] = model.RetailPrice;
                row["储值卡"] = model.Sum_Price;
                row["赠卡"] = model.Sum_AllPrice;
                row["未到天数"] = model.Number;
                row["未销售天数"] = model.Sum_Tiems;
                tableExport.Rows.Add(row);
                Sum_AllTimes += model.Sum_AllTimes;
                RetailPrice += model.RetailPrice;
                Sum_Price += model.Sum_Price;
                Sum_AllPrice += model.Sum_AllPrice;
                Number0 += model.Number;
                Sum_Tiems += model.Sum_Tiems;

            }

            DataRow row1 = tableExport.NewRow();
            row1["客户卡号"] = "合计";
            row1["疗程卡余次"] = Sum_AllTimes;
            row1["疗程卡余额"] = RetailPrice;
            row1["储值卡"] = Sum_Price;
            row1["赠卡"] = Sum_AllPrice;
            row1["未到天数"] = Number0;
            row1["未销售天数"] = Sum_Tiems;
           // row1["储值卡"] = Sum_Price;
            //row1["余额"] = Sumyu_e;
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "Weixiaohao");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "Weixiaohao.xls");
        }
        /// <summary>
        /// 导出顾客
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportPatientInfo(PatientEntity entity)
        {
            entity.StartTime = entity.StartTime == null ? DateTime.Now.AddMonths(-3).ToString() : entity.StartTime;
            entity.EndTime = entity.EndTime == null ? DateTime.Now.ToString() : entity.EndTime;
            entity.StartTime = Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01");
            entity.EndTime = Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59");
            if (entity.UserName == "姓名、电话、会员卡号") entity.UserName = "";
            if (!String.IsNullOrEmpty(entity.UserName))
            {
                entity.UserName = entity.UserName.ToUpper();
            }
            int rows;
            int pagecount;
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.IsActive == 0)
            {
                entity.IsActive = 1;
            }
            if (entity.HospitalID == LoginUserEntity.ParentHospitalID)
            {
                entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            }
            if (Request.IsAjaxRequest())
            {
                if (entity.HospitalID == 0)
                {
                    entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                }
                //else
                //{
                //    entity.ParentHospitalID = 0;
                //}
            }
            if (entity.IsSelectTime == 0)
            {
                entity.StartTime = null;
                entity.EndTime = null;
            }
            //三个月内到店一次
            if (entity.LiushiKe == 1)
            {
                entity.s_StareTime = DateTime.Now.AddMonths(-3);
                entity.s_Endtime = DateTime.Now;
            }
            else if (entity.LiushiKe == 2)//3-6个月到店一次
            {
                entity.s_StareTime = DateTime.Now.AddMonths(-6);
                entity.s_Endtime = DateTime.Now.AddMonths(-3);
            }
            else if (entity.LiushiKe == 3)//6个月以上未到店
            {
                entity.s_StareTime = Convert.ToDateTime("1990-01-01");
                entity.s_Endtime = DateTime.Now.AddMonths(-6);
            }
            else
            {
                entity.s_StareTime = Convert.ToDateTime("1970-01-01");
                entity.s_Endtime = Convert.ToDateTime("1970-01-01");
            }
            entity.numPerPage = 1000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            var list = patientBLL.GetPatientList(entity, out rows, out pagecount);
            list = list.Where(t=>t.IsDel!=1).ToList();
            DataTable tableExport = new DataTable("Export");
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_查看手机号);
            var customerTypeList = systemBLL.GetBaseInfoTreeByType("CustomerType", 1, LoginUserEntity.ParentHospitalID);
            //if (HasPermission1)
            //{
            //    tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("会员卡号"), new DataColumn("姓名"), new DataColumn("顾问"), new DataColumn("美容师"), new DataColumn("开卡日期"), new DataColumn("手机"), new DataColumn("性别"), new DataColumn("生日"), new DataColumn("会员级别"), new DataColumn("顾客类型"), new DataColumn("顾客分类"), new DataColumn("到店次数"), new DataColumn("上次到店") });
            //}
            //else
            //{
            //    tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("会员卡号"), new DataColumn("姓名"), new DataColumn("顾问"), new DataColumn("美容师"), new DataColumn("开卡日期"), new DataColumn("性别"), new DataColumn("生日"), new DataColumn("会员级别"), new DataColumn("顾客类型"), new DataColumn("顾客分类"), new DataColumn("到店次数"), new DataColumn("上次到店") });
            //}
            //tableExport.Columns.AddRange(new DataColumn[] {
            //    new DataColumn("客户卡号"), new DataColumn("客户名称"), new DataColumn("性别"), new DataColumn("日历"),
            //    new DataColumn("咨询顾问"), new DataColumn("美容师"), new DataColumn("顾客类型"), new DataColumn("会员级别"),
            //     new DataColumn("生日"),new DataColumn("手机号码"),  new DataColumn("分店"),
            //    new DataColumn("入会日期"),  new DataColumn("到店次数"),
            //    new DataColumn("上次到店"),  new DataColumn("顾客分类")
            //    });
            tableExport.Columns.AddRange(new DataColumn[] {
                new DataColumn("客户卡号"), new DataColumn("客户名称"), new DataColumn("性别"), new DataColumn("日历"),
                new DataColumn("咨询顾问"), new DataColumn("美容师"), new DataColumn("顾客类型"), new DataColumn("会员级别"),
                 new DataColumn("生日"),new DataColumn("手机号码"),  new DataColumn("分店"),
                new DataColumn("入会日期"),  
                new DataColumn("上次到店"),  new DataColumn("顾客分类")
                });
            //new DataColumn("顾客分类"), new DataColumn("到店次数"), new DataColumn("上次到店") 
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["客户卡号"] = model.MemberNo;
                row["客户名称"] = model.UserName;
                row["性别"] = model.Sex == 0 ? "男" : "女";
                row["日历"] = model.Calendar;
                row["咨询顾问"] = model.FollowUpUserName;
                row["美容师"] = model.FollowUpMrUserName;
                row["顾客类型"] = model.ArchivesType;
                row["会员级别"] = model.Category;
                //row["年份"] = model.years;
                row["生日"] = model.Birthday != null ? model.Birthday.Substring(0, model.Birthday.Length - 0) : "";
                row["手机号码"] = model.Mobile;
                row["分店"] = model.HospitalName;
                row["入会日期"] = model.RuHuiTime;
                //row["到店次数"] = model.DaodianCount;
                string lasttime = model.LastTime.ToString("yyyy-MM-dd");
                row["上次到店"] = lasttime;
                foreach (BaseinfoEntity type in customerTypeList)
                {
                    if (type.ID == model.CustomerTypeID)
                    {
                        row["顾客分类"] = type.InfoName;
                        break;
                    }
                }
                //if (HasPermission1)
                //{
                //    row["手机"] = model.Mobile;
                //}
                //row["到店次数"] = model.DaodianCount;
                //row["上次到店"] = model.LastTime.ToString("yyyy-MM-dd");

                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "Guke");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "Guke.xls");
        }
        public ActionResult ExportPatientstatisticsLost(_SeachEntity entity)
        {
            if (entity.s_Keyword == "姓名、手机、顾问姓名")
                entity.s_Keyword = "";
            entity.pageNum = 1;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (entity.s_HospitalID == 0 && entity.s_ID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.s_HospitalID == 0)
            {
                entity.s_Parent = LoginUserEntity.ParentHospitalID;
            }
            ////这2个时间做预备,防止时间越界
            //if (entity.s_StareTime == DateTime.MinValue)
            //{
            //    entity.s_StareTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
            //}
            //if (entity.s_Endtime == DateTime.MinValue)
            //{
            //    entity.s_Endtime = DateTime.Now;
            //}
            if (entity.s_HospitalName == "搜索") entity.s_HospitalName = "";
            if (entity.s_Endtime.Year > 2000) entity.s_Endtime = entity.s_Endtime;
            if (entity.s_Endtime.Year < 2000) entity.s_Endtime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
            if (entity.s_StareTime.Year < 2000) entity.s_StareTime = Convert.ToDateTime(DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss"));
            entity.s_StareTime = Convert.ToDateTime(entity.s_StareTime.Year + "-" + entity.s_StareTime.Month + "-" + entity.s_StareTime.Day + " 00:00:01");
            entity.s_Endtime = Convert.ToDateTime(entity.s_Endtime.Year + "-" + entity.s_Endtime.Month + "-" + entity.s_Endtime.Day + " 23:59:59");
            //  entity.s_StareTime = DateTime.Now; entity.s_Endtime = DateTime.Now;//这2个时间做预备,防止时间越界
            int rows = 0; int pagecount = 0;
            decimal sumyue = 0.00M;
            if (entity.s_TableType > 0)
            {
                switch (entity.s_TableType)
                {
                    case 1: entity.s_StareTime = DateTime.Now.AddMonths(-1); entity.s_Endtime = DateTime.Now; break;
                    case 2: entity.s_StareTime = DateTime.Now.AddMonths(-3); entity.s_Endtime = DateTime.Now.AddMonths(-1); break;
                    case 3: entity.s_StareTime = DateTime.Now.AddMonths(-6); entity.s_Endtime = DateTime.Now.AddMonths(-3); break;
                    case 4: entity.s_StareTime = DateTime.Now.AddMonths(-12); entity.s_Endtime = DateTime.Now.AddMonths(-6); break;
                    case 5: entity.s_StareTime = DateTime.Now.AddYears(-20); entity.s_Endtime = DateTime.Now.AddYears(-1); break;
                    default: break;
                }
            }
            entity.orderDirection = "asc";
            entity.numPerPage = 999999999; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "LeaveDays"; entity.orderDirection = "desc"; }
            var list = patientBLL.GetLostList(entity, out rows, out pagecount, out sumyue);
            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店"), new DataColumn("姓名"), new DataColumn("手机"), new DataColumn("顾问"), new DataColumn("美容师"), new DataColumn("等级"), new DataColumn("最后到店时间"), new DataColumn("几天未到店") });
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["门店"] = model.HospitalName;
                row["姓名"] = model.UserName;
                row["手机"] = model.Phone;
                row["顾问"] = model.FollowUpUserName;
                row["美容师"] = model.FollowUserName;
                row["等级"] = model.Category;
                row["最后到店时间"] = model.LastTime.ToString("yyyy-MM-dd");
                row["几天未到店"] = model.LeaveDays;
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "liushike");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "liushike.xls");
        }
        public ActionResult WeixinbindRemove()
        {
            var result = 0;
            var UserID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["UserID"]);
            if (UserID > 0)
            {
                result = patientBLL.WeixinbindRemove(UserID);
            }
            return Json(result);
        }
        /// <summary>
        /// 会员结构分析导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportStatisticsPatient_CustomerStructure(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            int rows;
            int pagecount;
            decimal xiaofei = 0; decimal shihao = 0;
            entity.numPerPage = 10000; //每页显示条数
            entity.orderField = "PayMoney"; entity.orderDirection = "desc";
            var list = patientBLL.GetUserStructure(entity, out rows, out pagecount, out xiaofei, out shihao); //求出现金的总额
            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("会员卡号"), new DataColumn("姓名"), new DataColumn("顾客类型"), new DataColumn("会员级别"), entity.s_ProductType == 0 ? new DataColumn("实耗") : new DataColumn("现金") });
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["会员卡号"] = model.MemberNo;
                row["姓名"] = model.UserName;
                row["顾客类型"] = model.ArchivesType;
                row["会员级别"] = model.Category;
                if (entity.s_ProductType == 0) { row["实耗"] = model.PayMoney; }
                else { row["现金"] = model.PayMoney; }
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "Huiyuanjiegou");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "Huiyuanjiegou.xls");
        }
        #endregion

        #region== 流失客月结

        /// <summary>
        /// 流失客月结
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientYuejie(PatientYuejieEntity entity)
        {
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
                entity.Year = DateTime.Now.Year + "";
            }
            var list = patientBLL.SelectPatientMonthjieList(entity);
            var result = list.AsQueryable().ToPagedList(1, list.Count);
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Count = list.Count;
            if (Request.IsAjaxRequest())
                return PartialView("_PatientYuejie", result);
            return View(result);
        }
        #endregion
        /// <summary>
        /// 删除客户资料
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public string DeleteById(string UserID)
        {
            int id = patientBLL.DeleteUserId(UserID);
            return id.ToString();
        }
        #endregion
    }
}
