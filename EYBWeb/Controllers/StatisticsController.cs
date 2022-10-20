using CenterManage.Factory.IBLL;
using EYB.ServiceEntity.PatientEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Newtonsoft.Json;
using EYB.ServiceEntity.SystemEntity;
using SystemManage.Factory.IBLL;
using PatientManage.Factory.IBLL;
using ReservationDoctorManage.Factory.IBLL;
using PersonnelManage.Factory.IBLL;
using CashManage.Factory.IBLL;
using BaseinfoManage.Factory.IBLL;
using Webdiyer.WebControls.Mvc;
using System.Data;
using HS.SupportComponents;
using EYB.ServiceEntity.CashEntity;
using EYB.ServiceEntity.MoneyEntity;

namespace EYB.Web.Controllers
{
    public class StatisticsController : BaseController
    {

        #region 依赖注入
        private ISystemManageBLL systemBLL;
        private IPatientManageBLL patientBLL;
        private IReservationDoctorManageBLL reservationBLL;
        private IPersonnelManageBLL personBLL;
        private ICashManageBLL cashBll;
        private IBaseinfoBLL Ibll;
        private ICenterManageBLL iCentBLL;//管理中心
        private IPersonnelManageBLL personnelBLL;
        public StatisticsController()
        {

            systemBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();//基础数据
            patientBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPatientManageBLL>();//顾客
            reservationBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IReservationDoctorManageBLL>();//病历
            personBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            cashBll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<CashManage.Factory.IBLL.ICashManageBLL>();
            Ibll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IBaseinfoBLL>();
            iCentBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ICenterManageBLL>();
            personnelBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
        }
        #endregion 依赖注入




        /// <summary>
        /// 员工统计
        /// </summary>
        /// <returns></returns>
        public ActionResult StatisticsHospitalUser()
        {
            return View();
        }
        /// <summary>
        /// 财务统计
        /// </summary>
        /// <returns></returns>
        public ActionResult StatisticsMoney()
        {
            return View();
        }
        /// <summary>
        /// 顾客统计
        /// </summary>
        /// <returns></returns>
        public ActionResult StatisticsPatient()
        {
            return View();
        }
        /// <summary>
        ///营业统计图表
        /// </summary>
        /// <returns></returns>
        public ActionResult StatisticsBusinesChart()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        ///营业统计图表--获取数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStatisticsBusinesChart(TimeConsumingPerformanceEntity entity)
        {
            entity.StartTime = (entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01")));
            entity.EndTime = (entity.EndTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59")));
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            var mod = cashBll.GetStatisticsBusinesChart(entity);
            return Json(mod);
        }
        public ActionResult StatisticsBusinesChartPage(TimeConsumingPerformanceEntity entity)
        {
            entity.StartTime = (entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01")));
            entity.EndTime = (entity.EndTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59")));
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }

            var mod =cashBll.GetStatisticsBusinesChart(entity);
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View(mod);
        }
        /// <summary>
        /// 营业统计报表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult StatisticsBusiness(OrderEntity entity)
        {
            entity.StartTime = (entity.StartTime == null ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01") : Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = (entity.EndTime == null ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59") : Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59"));
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            if (String.IsNullOrEmpty(entity.orderField))
            {
                entity.orderField = "SumMoney";
                entity.orderDirection = "desc";
            }
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
           var list = cashBll.GetStatisticsBusiness(entity);
            var result = list.AsQueryable().ToPagedList(1, 100);
            if (Request.IsAjaxRequest())
                return PartialView("_StatisticsBusiness", result);
            return View(result);
        }
           /// <summary>
        /// 财务分析表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult StatisticsCaiwu(OrderEntity entity)
        {
            entity.StartTime = (entity.StartTime == null ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01") : Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = (entity.EndTime == null ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59") : Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59"));
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            if (String.IsNullOrEmpty(entity.orderField))
            {
                entity.orderField = "SumMoney";
                entity.orderDirection = "desc";
            }
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            var list = cashBll.GetStatisticsCaiwu(entity);
            var result = list.AsQueryable().ToPagedList(1, 100);
            if (Request.IsAjaxRequest())
                return PartialView("_StatisticsCaiwu", result);
            return View(result);
        }



        /// <summary>
        /// 会员排行
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult StatisticsPatientOrder(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            entity.StartTime = entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.Endtime = entity.Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));
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
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "SumPayMoney"; }
            var list = patientBLL.GetUserPayMent(entity, out rows, out pagecount, out xiaofei, out shihao); //求出现金的总额
            decimal allmoney = patientBLL.GetUserPayMentSum(entity);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.st = entity.s_StareTime.ToString("yyyy-MM-dd");
            ViewBag.et = entity.s_Endtime.ToString("yyyy-MM-dd");
            ViewBag.xiaofei = xiaofei;
            ViewBag.shihao = shihao;

            if (Request.IsAjaxRequest())
                return PartialView("_StatisticsPatientOrder", result);
            return View(result);
        }
        /// <summary>
        /// 品项统计
        /// </summary>
        /// <returns></returns>
        public ActionResult StatisticsProduct()
        {
            return View();
        }
        /// <summary>
        /// 品项分析表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult StatisticsProductAlayise(_SeachEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.al = false;//是否传入单个门店
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderinfoEntity>().AsQueryable().ToPagedList(1, entity.numPerPage));
                }
            }

            if (entity.s_HospitalID == 0)
            {
                entity.s_TableType = LoginUserEntity.ParentHospitalID;
                ViewBag.al = true;
            }
            else
            {
                var hospitalmodel = personnelBLL.HospitalEntityByID(entity.s_HospitalID);
                ViewBag.hname = hospitalmodel.Name;
            }

            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));


            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            int AllQuantity = 0;
            decimal AllPrice = 0;

            entity.orderDirection = "desc";
            entity.numPerPage = 99999999; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "Quantity"; }
            var list = cashBll.GetStatisticsProductAlayiseList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.AllQuantity = AllQuantity;
            ViewBag.AllPrice = AllPrice;


            if (Request.IsAjaxRequest())
                return PartialView("_StatisticsProductAlayise", result);
            return View(result);
        }


        public ActionResult StatisticsCheckOrderList(OrderEntity entity)
        {

            if (entity.UserName == "姓名、订单号、会员卡号") entity.UserName = "";
            entity.StartTime = (entity.StartTime == null ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01") : Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = (entity.EndTime == null ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59") : Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59"));

            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "DocumentNumber"; entity.orderDirection = "desc"; }

            if (entity.Statu == 0) //首次加载页面查询 已完成的
            {
                ViewBag.Statu = 1;
                entity.Statu = 1;
            }
            else if (entity.Statu == 999) //查询全部
            {
                entity.Statu = 0;
                ViewBag.Statu = entity.Statu;
            }
            else
            {
                ViewBag.Statu = entity.Statu;
            }
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            if (HasPermission1)
            {
                entity.IsShow = 2;
            }
            decimal QianPrice = 0;
            var list = cashBll.GetAllOrder(entity, out QianPrice);

            var newlist = new List<OrderEntity>();
            foreach (var info in list)
            {
                var infolist = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = info.OrderNo });
                decimal otherpay = 0;
                foreach (var io in infolist)
                {
                    if (io.PayType == 1)
                    {
                        info.Xianjin = io.PayMoney;
                    }
                    else if (io.PayType == 2)
                    {
                        info.Yinlianka = io.PayMoney;
                    }
                    else if (io.PayType == 3 && io.IsGive == 1) //赠送
                    {
                        info.ZengChuzhika = info.ZengChuzhika + io.PayMoney;
                    }
                    else if (io.PayType == 3 && io.IsGive == 2) //非赠送
                    {
                        info.Chuzhika = info.Chuzhika + io.PayMoney;
                    }
                    else if (io.PayType == 4)
                    {
                        info.Huakouka = info.Huakouka + io.PayMoney;
                    }
                    else if (io.PayType > 4 && io.PayName.Contains("支付宝"))
                    {
                        info.Zhifubao = io.PayMoney;
                    }
                    else if (io.PayType > 4 && io.PayName.Contains("微信"))
                    {
                        info.Weixin = io.PayMoney;
                    }
                    else if (io.PayType > 4 && io.ParentPayType == 1 && !io.PayName.Contains("微信") && !io.PayName.Contains("支付宝"))
                    {
                        info.MianDan = io.PayMoney;
                    }
                    else if (io.ParentPayType == 2)
                    {
                        otherpay = io.PayMoney;
                    }
                }
                info.OtherPay = otherpay;

                newlist.Add(info);
            }
            var result = newlist.AsQueryable().ToPagedList(1, entity.numPerPage);

            var sumList = cashBll.GetSumOrderForEveryPrice(entity);//获取
            ViewBag.xianjin = sumList.Any(i => i.PayType == 1) ? sumList.Where(i => i.PayType == 1).Sum(z => z.SumMoney) : 0;
            ViewBag.shuaka = sumList.Any(i => i.PayType == 2) ? sumList.Where(i => i.PayType == 2).Sum(z => z.SumMoney) : 0;
            ViewBag.zhifubao = sumList.Any(i => i.PayType > 4 && i.PayName.Contains("支付宝")) ? sumList.Where(i => i.PayType > 4 && i.PayName.Contains("支付宝")).ToList().Sum(z => z.SumMoney) : 0;
            ViewBag.weixin = sumList.Any(i => i.PayType > 4 && i.PayName.Contains("微信")) ? sumList.Where(i => i.PayType > 4 && i.PayName.Contains("微信")).ToList().Sum(z => z.SumMoney) : 0;
            ViewBag.miandan = sumList.Any(i => i.PayType > 4 && i.ParentPayType == 1 && !i.PayName.Contains("微信") && !i.PayName.Contains("支付宝")) ? sumList.Where(i => i.PayType > 4 && i.ParentPayType == 1 && !i.PayName.Contains("微信") && !i.PayName.Contains("支付宝")).ToList().Sum(z => z.SumMoney) : 0;
            ViewBag.otherpay = sumList.Any(i => i.ParentPayType == 2) ? sumList.Where(i => i.ParentPayType == 2).ToList().Sum(z => z.SumMoney) : 0;
            // ViewBag.shuaka = sumList.Where(i => i.PayType == 2).Count() > 0 ? sumList.Where(i => i.PayType == 2).ToList()[0].SumMoney : 0;
            ViewBag.zongshouru = ViewBag.xianjin + ViewBag.shuaka + ViewBag.zhifubao + ViewBag.weixin + ViewBag.miandan;
            ViewBag.chuzhi = sumList.Any(i => i.PayType == 3 && i.IsGive == 2) ? sumList.Where(i => i.PayType == 3 && i.IsGive == 2).ToList().Sum(z => z.SumMoney) : 0;
            ViewBag.zengchuzhi = sumList.Any(i => i.PayType == 3 && i.IsGive == 1) ? sumList.Where(i => i.PayType == 3 && i.IsGive == 1).ToList().Sum(z => z.SumMoney) : 0;
            ViewBag.liaocheng = sumList.Any(i => i.PayType == 4) ? sumList.Where(i => i.PayType == 4).Sum(z => z.SumMoney) : 0;
            //ViewBag.qita = sumList.Where(i => i.PayType > 4).Count() > 0 ? sumList.Where(i => i.PayType > 4).ToList().Sum(z => z.SumMoney) : 0;
            ViewBag.qiankuan = QianPrice;// sumList.Where(i => i.QianPrice < 0).Sum(i => i.QianPrice);

            result.TotalItemCount = entity.Rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.OrderType = entity.OrderType;
            ViewBag.StartTime = Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd");
            ViewBag.EndTime = Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd");

            if (Request.IsAjaxRequest())
                return PartialView("_StatisticsCheckOrderList", result);
            return View(result);
        }
        #region==顾客分析报表==
        /// <summary>
        /// 会员结构分析
        /// </summary>
        /// <returns></returns>
        public ActionResult StatisticsPatient_CustomerStructure(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.HospitalID = entity.s_HospitalID;
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            int rows;
            int pagecount;
            decimal xiaofei = 0; decimal shihao = 0;

            entity.numPerPage = 2000; //每页显示条数
            entity.orderField = "PayMoney"; entity.orderDirection = "desc";
           var list = patientBLL.GetUserStructure(entity, out rows, out pagecount, out xiaofei, out shihao); //求出现金的总额
            ViewBag.s_ProductType = entity.s_ProductType;
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.st = entity.s_StareTime.ToString("yyyy-MM-dd");
            ViewBag.et = entity.s_Endtime.ToString("yyyy-MM-dd");
            ViewBag.xiaofei = xiaofei;
            ViewBag.shihao = shihao;

            //if (Request.IsAjaxRequest())  
            //    return PartialView("_StatisticsPatient_CustomerStructure", result);
            return View(result);
        }
        /// <summary>
        /// 设置顾客分类
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult SetCustomerType(string UserID, int CustomerTypeID)
        {
            List<int> list = new List<int>();
            string[] UserIDList = UserID.Split(',');
            foreach (var info in UserIDList)
            {
                list.Add(HS.SupportComponents.ConvertValueHelper.ConvertIntValue(info));
            }
            int result = patientBLL.SetCustomerType(list, CustomerTypeID);
            return Json(result);
        }
        /// <summary>
        /// 顾客分类图表查询
        /// </summary>
        /// <returns></returns>
        public ActionResult StatisticsPatient_CustomerStructureSelect(int HospitalID = 0)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (HospitalID == 0) { HospitalID = LoginUserEntity.HospitalID; }
            var list = patientBLL.SelectCustomerType(new BaseinfoEntity { HospitalID = HospitalID, parentID = LoginUserEntity.ParentHospitalID });
            StringBuilder str = new StringBuilder();
            string results = "";
            str.Append("[");
            foreach (var info in list)
            {
                str.Append("{ name:\'" + info.InfoName + "\' ,y :" + info.SortID + "},");
            }
            results = str.ToString().Substring(0, str.Length - 1) + "]";
            ViewBag.result = JsonConvert.SerializeObject(results); ;
            return View();
        }
        /// <summary>
        /// 流失客户分析
        /// </summary>
        /// <returns></returns>
        public ActionResult StatisticsPatient_CustomerLiushi(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            //三个月内到店一次
            //梳理员工管理的目标管理下增加组目标管理添加小组数据接口（完成）
            //梳理财务管理的月营业报表的卡扣业绩相关数据接口逻辑功能（完成）
            if (entity.s_ProductType == 1)
            {
                entity.s_StareTime = DateTime.Now.AddMonths(-3);
                entity.s_Endtime = DateTime.Now;
            }
            else if (entity.s_ProductType == 2)//3-6个月到店一次
            {
                entity.s_StareTime = DateTime.Now.AddMonths(-6);
                entity.s_Endtime = DateTime.Now.AddMonths(-3);
            }
            else if (entity.s_ProductType == 3)//6个月以上未到店
            {
                entity.s_StareTime = Convert.ToDateTime("1990-01-01");
                entity.s_Endtime = DateTime.Now.AddMonths(-6);
            }
            else
            {
                entity.s_StareTime = DateTime.Now;
                entity.s_Endtime = DateTime.Now;
            }

            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<PatientEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            var list = patientBLL.GetUserLiushi(entity); //求出现金的总额
            ViewBag.Count = list.Count;
            var result = list.AsQueryable().ToPagedList(1, 10000);
            ViewBag.HospitalID = entity.s_HospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_StatisticsPatient_CustomerLiushi", result);
            return View(result);
        }
        /// <summary>
        /// 顾客项目结构分布图
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult StatisticsPatient_ProjectJiegou(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            entity.s_Parent = LoginUserEntity.ParentHospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.s_HospitalID =  entity.s_HospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<PatientEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            var list = patientBLL.GetPatientProjectJiegou(entity); //求出现金的总额
            ViewBag.Count = list.Count;
            var result = list.AsQueryable().ToPagedList(1, 10000);

            if (Request.IsAjaxRequest())
                return PartialView("_StatisticsPatient_ProjectJiegou", result);
            return View(result);
        }
        #endregion




        public ActionResult PerformanceAnalysis(_SeachEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (!Request.IsAjaxRequest())
            {
                entity.s_StareTime = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
                entity.s_HospitalID = LoginUserEntity.HospitalID;
                return View(new List<PerformanceAnalysisEntity>().AsQueryable().ToPagedList(1, 10));
            }

            ViewBag.stime = entity.s_StareTime.ToString("yyyy-MM");

            var list = iCentBLL.GetPerformanceAnalysisList(entity);
            var result = list.AsQueryable().ToPagedList(1, 1000);

            if (Request.IsAjaxRequest())
                return PartialView("_PerformanceAnalysis", result);
            return View(result);
        }


        public ActionResult PerformanceRanking(_SeachEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (!Request.IsAjaxRequest())
            {
                entity.s_StareTime = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
                entity.s_HospitalID = LoginUserEntity.HospitalID;
                return View(new List<PerformanceRankingEntity>().AsQueryable().ToPagedList(1, 10));
            }


            ViewBag.stime = entity.s_StareTime.ToString("yyyy-MM");

            var list = iCentBLL.GetPerformanceRankingList(entity);
            int num = 1;

            list = list.OrderByDescending(i => i.KaXiang).ToList();
            num = 1;
            foreach (var info in list)
            {
                info.YejiRank = num;
                num++;
            }
            list = list.OrderByDescending(i => i.ShiHao).ToList();
            num = 1;
            foreach (var info in list)
            {
                info.ShihaoRank = num;
                num++;
            }
            list = list.OrderByDescending(i => i.XinKe).ToList();
            num = 1;
            foreach (var info in list)
            {
                info.XinKeRank = num;
                num++;
            }
            list = list.OrderByDescending(i => i.DaoDian).ToList();
            num = 1;
            foreach (var info in list)
            {
                info.DaoDianRank = num;
                num++;
            }

            list = list.OrderByDescending(i => i.Yeji).ToList();
            num = 1;
            foreach (var info in list)
            {
                info.YejiRank = num;
                num++;
            }


            var result = list.AsQueryable().ToPagedList(1, 1000);

            if (Request.IsAjaxRequest())
                return PartialView("_PerformanceRanking", result);
            return View(result);
        }


        /// <summary>
        /// 畅销品项分析
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SellingProductAndProject(_SeachEntity entity)
        {

            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (!Request.IsAjaxRequest())
            {
                entity.s_StareTime = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
                entity.s_Endtime = DateTime.Now;
                entity.s_HospitalID = LoginUserEntity.HospitalID;
                ViewBag.stime = entity.s_StareTime.ToString("yyyy-MM");
                ViewBag.etime = entity.s_Endtime.ToString("yyyy-MM");
                return View(new List<SellingProductAndProjectEntity>().AsQueryable().ToPagedList(1, 10));
            }

            entity.s_Endtime = Convert.ToDateTime(entity.s_Endtime.AddDays(-entity.s_Endtime.Day + 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd 23:59:59"));
            entity.s_ProductType=entity.s_ProductType == 0 ? 1 : entity.s_ProductType;
            int row = 0;
            int pagecount = 0;

            entity.numPerPage = 2000; //每页显示条数
            entity.orderField = "SaleNumber"; entity.orderDirection = "desc";
            var list = iCentBLL.GetSellingProductAndProjectList(entity, out row, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = row;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;

            ViewBag.stime = entity.s_StareTime.ToString("yyyy-MM");
            ViewBag.etime = entity.s_Endtime.ToString("yyyy-MM");

            if (Request.IsAjaxRequest())
                return PartialView("_SellingProductAndProject", result);
            return View(result);
        }


        public ActionResult BrandProfitList(_SeachEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (!Request.IsAjaxRequest())
            {
                entity.s_StareTime = DateTime.Today;
                entity.s_HospitalID = LoginUserEntity.HospitalID;
                entity.s_ProductType = 1;
                return View(new List<BrandProfitEntity>().AsQueryable().ToPagedList(1, 10));
            }

            int row = 0;
            int pagecount = 0;

            entity.numPerPage = 50; //每页显示条数
            entity.orderField = "AllPrice"; entity.orderDirection = "desc";
            var list = iCentBLL.GetBrandProfitList(entity, out row, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = row;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;


            if (Request.IsAjaxRequest())
                return PartialView("_BrandProfitList", result);
            return View(result);

        }

        /// <summary>
        /// 流失客户分析---导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportStatisticsPatient_CustomerLiushi(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            //三个月内到店一次
            if (entity.s_ProductType == 1)
            {
                entity.s_StareTime = DateTime.Now.AddMonths(-3);
                entity.s_Endtime = DateTime.Now;
            }
            else if (entity.s_ProductType == 2)//3-6个月到店一次
            {
                entity.s_StareTime = DateTime.Now.AddMonths(-6);
                entity.s_Endtime = DateTime.Now.AddMonths(-3);
            }
            else if (entity.s_ProductType == 3)//6个月以上未到店
            {
                entity.s_StareTime = Convert.ToDateTime("1990-01-01");
                entity.s_Endtime = DateTime.Now.AddMonths(-6);
            }
            else
            {
                entity.s_StareTime = DateTime.Now;
                entity.s_Endtime = DateTime.Now;
            }

            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;

            var list = patientBLL.GetUserLiushi(entity); //求出现金的总额
            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("会员卡号"), new DataColumn("姓名"), new DataColumn("会员级别"), new DataColumn("尊贵级别") });
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["会员卡号"] = model.MemberNo;
                row["姓名"] = model.UserName;
                row["会员级别"] = model.ArchivesType;
                row["尊贵级别"] = model.Category;
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "Liushike");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "Liushike.xls");
        }
        /// <summary>
        /// 获取当月营业额
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult GetStatisticsBusinesChartPage(OrderEntity entity)
        {
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }

            DateTime dt = DateTime.Now;
            //本月第一天时间
            DateTime dt_First = dt.AddDays(1 - (dt.Day));
            //获得某年某月的天数
            int year = dt.Date.Year;
            int month = dt.Date.Month;
            int dayCount = DateTime.DaysInMonth(year, month);
            //本月最后一天时间
            DateTime dt_Last = dt_First.AddDays(dayCount - 1);



            string result1 = "";
            decimal amout1 = 0;
            decimal allpirce = 0;
            for (int i=1;i<= dayCount;i++)
            {
                entity.StartTime = DateTime.Now.ToString("yyyy-MM") + "-" + i+" 00:00:01";
                entity.EndTime = DateTime.Now.ToString("yyyy-MM") + "-" + i + " 23:59:59";
                amout1 = cashBll.GetHospitalYeji(entity);
                result1 += amout1 + ",";
                allpirce = allpirce + amout1;
            }
            JsonModelEntity res = new JsonModelEntity();
            res.OutputHtml = "[" + result1.Substring(0, result1.Length - 1) + "]";

            ViewBag.HospitalID = entity.HospitalID;
            res.AllPrice = allpirce;

            return Json(res);
        }
    }
}
