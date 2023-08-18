using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using EYB.ServiceEntity.CashEntity;
using CashManage.Factory.IBLL;
using PersonnelManage.Factory.IBLL;
using BaseinfoManage.Factory.IBLL;
using ReservationDoctorManage.Factory.IBLL;
using SystemManage.Factory.IDAL;
using PatientManage.Factory.IBLL;
using WarehouseManage.Factory.IBLL;
using EYB.ServiceEntity.MoneyEntity;
using EYB.ServiceEntity.PatientEntity;
using System.Text;
using EYB.ServiceEntity.PersonnelEntity;
using System.Web.Caching;
using System.Data;
using HS.SupportComponents;
using EYB.Web.Code;
using CenterManage.Factory.IBLL;
using DepartmentManage.Factory.IBLL;
using System.Configuration;
using NPOI.SS.Formula.Eval;
using System.Data.SqlClient;
using EYB.ServiceEntity.BaseInfo;

namespace EYB.Web.Controllers
{
    public class MoneyManageController : BaseController
    {
        #region 依赖注入

        private ICashManageBLL cashBll;
        private IPersonnelManageBLL personnelBLL;
        private IBaseinfoBLL baseinfoBLL;//基础信息模块
        private IReservationDoctorManageBLL ResDocBLL;//预约管理
        private ISystemManageDAL sysbll;//系统管理
        private IPatientManageBLL IpatBLL;
        private IWarehouseManageBLL iwareBLL;//仓库管理
        private ICenterManageBLL iCentBll;//管理中心
        private IDepartmentManageBLL DepartmentManageBLL;
        public static int NewKeVersion = 1;
        public MoneyManageController()
        {
            NewKeVersion = ConvertValueHelper.ConvertIntValue(ConfigurationManager.AppSettings["NewKeVersion"]);
            cashBll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<CashManage.Factory.IBLL.ICashManageBLL>();
            personnelBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            baseinfoBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IBaseinfoBLL>();
            ResDocBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IReservationDoctorManageBLL>();
            IpatBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPatientManageBLL>();
            sysbll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageDAL>();
            iwareBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IWarehouseManageBLL>();
            iCentBll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ICenterManageBLL>();
            DepartmentManageBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IDepartmentManageBLL>();
        }

        #endregion 依赖注入

        #region==一键诊断==
        public ActionResult HospitalZhenduan(TimeConsumingPerformanceEntity entity)
        {
            DateTime dt = DateTime.Now;
            //本月第一天时间
            DateTime dtFirst = dt.AddDays(1 - (dt.Day));
            //获得某年某月的天数
            int year = dt.Date.Year;
            int month = dt.Date.Month;
            int dayCount = DateTime.DaysInMonth(year, month);
            //本月最后一天时间
            DateTime dtLast = dtFirst.AddDays(dayCount - 1);



            entity.StartTime = (entity.StartTime.Year == 1 ? Convert.ToDateTime(dtFirst.ToString("yyyy-MM-dd 00:00:00")) : Convert.ToDateTime(Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:00")));
            entity.EndTime = (entity.EndTime.Year == 1 ? Convert.ToDateTime(dtLast.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59")));

            ViewBag.StartTime = entity.StartTime;
            ViewBag.EndTime = entity.EndTime;
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            var mod = cashBll.GetHospitalZhenduan(entity, NewKeVersion);
            return View(mod);
        }



        #endregion
        #region ==业绩审核==
        /// <summary>
        /// 业绩审核
        /// </summary>
        /// <returns></returns>
        public ActionResult PerformanceAudit(OrderEntity entity)
        {
            entity.StartTime = (entity.StartTime == null ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01") : Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = (entity.EndTime == null ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59") : Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59"));

            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            entity.Statu = 1;//只看已结算的
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "UserName"; entity.orderDirection = "desc"; }
            decimal QianPrice = 0;
            var list = cashBll.GetAllOrder(entity, out QianPrice).Where(c => c.IsArchives != 1);
            list = list.Where(t => t.OrderNo.StartsWith("U")==false).ToList();
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);

            result.TotalItemCount = entity.Rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.OrderType = entity.OrderType;
            ViewBag.StartTime = Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd");
            ViewBag.EndTime = Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd");
            ViewBag.Statu = entity.ISAudit;
            ViewBag.SelHospitalID = entity.HospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_PerformanceAudit", result);
            return View(result);


        }

        /// <summary>
        /// 业绩详细页面
        /// </summary>
        /// <returns></returns>
        public ActionResult StaffPerformanceDetail7()
        {
            string OrderNo = Request["OrderNo"];
            string HospitalID = LoginUserEntity.HospitalID.ToString();
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            int count = 0;
            if (LoginUserEntity.HospitalID == 1017)
            {
                count = 9;
            }
            else
            {
                count = 8;
            }
            ViewBag.count = count;


            var list = cashBll.GetStaffPerformanceList7(OrderNo, HospitalID);
            // var list = cashBll.GetStaffPerformanceListUserID(OrderNo, Convert.ToInt32(Request["UserID"])) ;
            list = list.OrderBy(i => i.OrderInfoID).ToList();
            //list = list.Where(t=>t.HospitalID==LoginUserEntity.HospitalID).ToList();
            var df = 0;
            foreach (var item in list)
            {
                if (item.UserID == item.UserID)
                {
                    df = 1;
                }
            }
            var countff = list.Where(t => t.UserID == t.UserID).FirstOrDefault();

            var result = list.AsQueryable().ToPagedList(1, 10000);
            return View(result);
            //return View();
        }
        /// <summary>
        /// 业绩详细页面
        /// </summary>
        /// <returns></returns>
        public ActionResult StaffPerformanceDetail()
        {
            string OrderNo = Request["OrderNo"];
            
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            int count = 0;
            if (LoginUserEntity.HospitalID==1017)
            {
                count = 9;
            }
            else
            {
                count = 8;
            }
            ViewBag.count=count;


             var list = cashBll.GetStaffPerformanceList(OrderNo);
           // var list = cashBll.GetStaffPerformanceListUserID(OrderNo, Convert.ToInt32(Request["UserID"])) ;
            list = list.OrderBy(i => i.OrderInfoID).ToList();
            //list = list.Where(t=>t.HospitalID==LoginUserEntity.HospitalID).ToList(); 
            var df = 0;
            foreach (var item in list)
            {
                if (item.UserID==item.UserID)
                {
                    df = 1;
                }
            }
            var countff = list.Where(t=>t.UserID==t.UserID).FirstOrDefault();
            
            var result = list.AsQueryable().ToPagedList(1, 10000);
            return View(result);
            //return View();
        }
        /// <summary>
        /// 修改参与员工，用于订单详情修改参与员工操作
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult UpdateJoinUser(int OldUserID, int UserID, string OrderNo)
        {
            int result = cashBll.UpdateJoinUser(OldUserID, UserID, OrderNo);
            return Json(result);
        }
        /// <summary>
        /// 获取参与员工
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult GetJoinUser(int ID)
        {
            var model = cashBll.GetJoinUserModel(new JoinUserEntity { ID = ID });

            return Json(model);
        }

        //审核业绩
        public ActionResult SavePerformance1()
        {
            string OrderNo = Request["OrderNo"];
            var IDList = Request.Form["ID"].Split(',');
            var XianJinList = Request.Form["XianJin"].Split(',');
            var KaKouList = Request.Form["KaKou"].Split(',');
            var XiaoHaoList = Request.Form["XiaoHao"].Split(',');
            var ShouGongList = Request.Form["ShouGong"].Split(',');
            var RewardList = Request.Form["Reward"].Split(',');
            var ProjectNumberList = Request.Form["ProjectNumber"].Split(',');
            var KeliuList = Request.Form["Keliu"].Split(',');
            var IsZhiding = Request.Form["IsZhiding"].Split(',');
            var infobuytype = Request.Form["InfoBuyType"].Split(',');

            var YejiFenpei = Request.Form["PerformancePercentage"].Split(',');

            var result = 0;
            for (int i = 0; i < IDList.Length; i++)
            {
                //JoinUserEntity entity = new JoinUserEntity();
                var entity = cashBll.GetJoinUserModel(new JoinUserEntity { ID = Convert.ToInt32(IDList[i]) });
                //var yeji = entity.Yeji != 0;//判断原来是否有业绩
                //var kakou = entity.KakouYeji != 0;//判断原来是否有卡扣
                //var xiaohao = entity.XiaoHao != 0;//判断原来是否有消耗业绩

                entity.ID = ConvertValueHelper.ConvertIntValue(IDList[i]);
                entity.XiaoHao = ConvertValueHelper.ConvertDecimalValue(XiaoHaoList[i]);
                //entity.XiaoHao_Liaocheng = entity.XiaoHao;
                entity.Yeji = ConvertValueHelper.ConvertDecimalValue(XianJinList[i]);
                entity.Ticheng = ConvertValueHelper.ConvertDecimalValue(ShouGongList[i]);
                entity.KakouYeji = ConvertValueHelper.ConvertDecimalValue(KaKouList[i]);
                entity.OtherTiCheng = ConvertValueHelper.ConvertDecimalValue(RewardList[i]);
                entity.ProjectNumber = ConvertValueHelper.ConvertDecimalValue(ProjectNumberList[i]);
                entity.Keliu = ConvertValueHelper.ConvertDecimalValue(KeliuList[i]);
                entity.IsZhiding = ConvertValueHelper.ConvertIntValue(IsZhiding[i]);
                entity.InfoBuyType = ConvertValueHelper.ConvertIntValue(infobuytype[i]);
                entity.Yeji_Fenpei = ConvertValueHelper.ConvertIntValue(YejiFenpei[i]);
                entity.UpdateJoinUser();
                #region
                //if (entity.Type == 3)
                //{
                //    var sql = @"SELECT
	               //             a.BaseType,a.RetailPrice,a.*
                //            FROM
	               //             EYB_tb_Project a
                //            LEFT JOIN EYB_dict_Baseinfo b ON a.ProjectType = b.ID
                //            LEFT JOIN EYB_tb_Hospital c ON a.HospitalID = c.ID
                //            LEFT JOIN EYB_tb_ProjectProduct d on  a.ID = d.ProjectID
                //            WHERE
	               //             d.ProgrammeID =" + entity.ProjectID;
                //    var item = GetIntegralByDaxiangmu(sql);
                //    decimal daixangmucha = 0;
                //    foreach (var info in item)
                //    {
                //        if (info.BaseType == 3)
                //        {
                //            daixangmucha =  info.RetailPrice;  // entity.Yeji;
                //        }
                //    }
                //    entity.Yeji_Liaochengka = entity.Yeji_Liaochengka - daixangmucha;
                //    entity.Yeji_Daxiangmu = daixangmucha;
                //}
                #endregion

                //if(entity.IsZhiding==1)
                //{
                //    entity.ShiCao_zhiding = entity.ShiCao;
                //    entity.ShiCao_feizhiding = 0;
                //}else
                //{
                //    entity.ShiCao_zhiding = 0;
                //    entity.ShiCao_feizhiding = entity.ShiCao;

                //}
                entity.ShiCao_zhiding = entity.ShiCao; //2018-11-15

                //if (entity.Yeji != 0 && yeji)
                //{
                //    entity.Yeji_Chanpin = entity.Yeji_Chanpin != 0 ? entity.Yeji : 0;
                //    entity.Yeji_Chuzhika = entity.Yeji_Chuzhika != 0 ? entity.Yeji : 0;
                //    entity.Yeji_huankuan = entity.Yeji_huankuan != 0 ? entity.Yeji : 0;
                //    entity.Yeji_Liaochengka = entity.Yeji_Liaochengka != 0 ? entity.Yeji : 0;
                //    entity.Yeji_Xiangmu = entity.Yeji_Xiangmu != 0 ? entity.Yeji : 0;
                //}
                //else if (entity.Yeji != 0 && yeji == false)//原来没有业绩,现在是添加新业绩
                //{
                //    if (entity.Type == 1) { entity.Yeji_Xiangmu = entity.Yeji; }
                //    else if (entity.Type == 2) { entity.Yeji_Chanpin = entity.Yeji; }
                //    else if (entity.Type == 3)
                //    {
                //        var cardmodel = baseinfoBLL.GetPrePayCardModel(entity.ProjectID);
                //        if (cardmodel.Type == 1) { entity.Yeji_Chuzhika = entity.Yeji; }
                //        else if (cardmodel.Type == 2) { entity.Yeji_Liaochengka = entity.Yeji; }
                //    }
                //    else { entity.Yeji_huankuan = entity.Yeji; }
                //}
                //else
                //{
                //    entity.Yeji_Chanpin = 0;
                //    entity.Yeji_Chuzhika = 0;
                //    entity.Yeji_huankuan = 0;
                //    entity.Yeji_Liaochengka = 0;
                //    entity.Yeji_Xiangmu = 0;
                //}

                //if (entity.KakouYeji != 0 && kakou)
                //{
                //    entity.KakouYeji_Chanpin = entity.KakouYeji_Chanpin != 0 ? entity.KakouYeji : 0;
                //    entity.KakouYeji_Huankuan = entity.KakouYeji_Huankuan != 0 ? entity.KakouYeji : 0;
                //    entity.KakouYeji_Liaocheng = entity.KakouYeji_Liaocheng != 0 ? entity.KakouYeji : 0;
                //    entity.KakouYeji_Xiangmu = entity.KakouYeji_Xiangmu != 0 ? entity.KakouYeji : 0;
                //}
                //else if (entity.KakouYeji != 0 && kakou == false)//原来没有业绩,现在是添加新业绩
                //{
                //    if (entity.Type == 1) { entity.KakouYeji_Xiangmu = entity.KakouYeji; }
                //    else if (entity.Type == 2) { entity.KakouYeji_Chanpin = entity.KakouYeji; }
                //    else if (entity.Type == 3)
                //    {
                //        entity.KakouYeji_Liaocheng = entity.KakouYeji;
                //    }
                //    else { entity.KakouYeji_Huankuan = entity.KakouYeji; }
                //}
                //else
                //{
                //    entity.KakouYeji_Chanpin = 0;
                //    entity.KakouYeji_Huankuan = 0;
                //    entity.KakouYeji_Liaocheng = 0;
                //    entity.KakouYeji_Xiangmu = 0;
                //}

                //项目服务数算法.服务项目数现在用ProjectNumber来进行更改.当ProjectNumber发生更改时,跟着更改keshu,KeShu_zhiding,KeShu_feizhiding
                if (entity.ProjectNumber > 0)
                {
                    entity.KeShu = (float)entity.ProjectNumber;
                    //if (entity.IsZhiding == 1)
                    //{
                    //    entity.KeShu_zhiding = entity.KeShu;
                    //    entity.KeShu_feizhiding = 0;
                    //}
                    //else
                    //{
                    //    entity.KeShu_zhiding = 0;
                    //    entity.KeShu_feizhiding = entity.KeShu;
                    //}
                    entity.KeShu_zhiding = entity.KeShu;//2018-11-15
                }
                else
                {
                    entity.KeShu = 0;
                    entity.KeShu_zhiding = 0;
                    entity.KeShu_feizhiding = 0;
                }

                // var order=cashBll.GetOrderModel(new OrderEntity{ OrderNo=OrderNo});

                ////0714
                ////实操写入
                ////实操==消耗,直接去消耗的值,然后判断实操是否指定,然后写入数据
                //if (entity.XiaoHao > 0)
                //{
                //    entity.ShiCao = entity.XiaoHao;

                //    if (entity.IsZhiding > 0)
                //    {
                //        entity.ShiCao_zhiding = entity.XiaoHao;
                //    }
                //    else
                //    {
                //        entity.ShiCao_feizhiding = entity.XiaoHao;
                //    }
                //}
                //else {
                //    entity.ShiCao = 0;
                //    entity.ShiCao_zhiding = 0;
                //    entity.ShiCao_feizhiding = 0;
                //}


                // if (entity.OtherTiCheng != 0) entity.IsZhiding = 1;


                cashBll.UpdateJoinUser(entity);
            }
            result = cashBll.UpdateOrderISAudit(OrderNo); //改修状态
            return Json(result);
        }
        private IList<ProjectEntity> GetIntegralByDaxiangmu(string sqlstr, int filter = 0)
        {

                string conStr = "server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft112";//连接字符串  
                SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                conText.Open();//打开数据库  
                string sqls = sqlstr;//创建统计语句  
                SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                SqlDataReader dr;//创建DataReader对象  
                dr = comText.ExecuteReader();//执行查询  

                var list = new List<ProjectEntity>();
                while (dr.Read())//判断数据表中是否含有数据  
                {
                    list.Add(DataBindHelper.CreateEntity<ProjectEntity>(dr));
                }
                dr.Close();//关闭DataReader对象  
                return list;

        }
        //审核业绩
        public ActionResult SavePerformance()
        {
            string OrderNo = Request["OrderNo"];
            var IDList = Request.Form["ID"].Split(',');
            var XianJinList = Request.Form["XianJin"].Split(',');
            var KaKouList = Request.Form["KaKou"].Split(',');
            var XiaoHaoList = Request.Form["XiaoHao"].Split(',');
            var ShouGongList = Request.Form["ShouGong"].Split(',');
            var RewardList = Request.Form["Reward"].Split(',');
            var ProjectNumberList = Request.Form["ProjectNumber"].Split(',');
            var KeliuList = Request.Form["Keliu"].Split(',');
            var IsZhiding = Request.Form["IsZhiding"].Split(',');
            var infobuytype = Request.Form["InfoBuyType"].Split(',');

            var YejiFenpei = Request.Form["PerformancePercentage"].Split(',');

            var result = 0;
            for (int i = 0; i < IDList.Length; i++)
            {
                //JoinUserEntity entity = new JoinUserEntity();
                var entity = cashBll.GetJoinUserModel(new JoinUserEntity { ID = Convert.ToInt32(IDList[i]) });
                //var yeji = entity.Yeji != 0;//判断原来是否有业绩
                //var kakou = entity.KakouYeji != 0;//判断原来是否有卡扣
                //var xiaohao = entity.XiaoHao != 0;//判断原来是否有消耗业绩

                entity.ID = ConvertValueHelper.ConvertIntValue(IDList[i]);
                entity.XiaoHao = ConvertValueHelper.ConvertDecimalValue(XiaoHaoList[i]);
                //entity.XiaoHao_Liaocheng = entity.XiaoHao;
                entity.Yeji = ConvertValueHelper.ConvertDecimalValue(XianJinList[i]);
                entity.Ticheng = ConvertValueHelper.ConvertDecimalValue(ShouGongList[i]);
                entity.KakouYeji = ConvertValueHelper.ConvertDecimalValue(KaKouList[i]);
                entity.OtherTiCheng = ConvertValueHelper.ConvertDecimalValue(RewardList[i]);
                entity.ProjectNumber = ConvertValueHelper.ConvertDecimalValue(ProjectNumberList[i]);
                entity.Keliu = ConvertValueHelper.ConvertDecimalValue(KeliuList[i]);
                entity.IsZhiding = ConvertValueHelper.ConvertIntValue(IsZhiding[i]);
                entity.InfoBuyType = ConvertValueHelper.ConvertIntValue(infobuytype[i]);
                entity.Yeji_Fenpei = ConvertValueHelper.ConvertIntValue(YejiFenpei[i]);
                entity.UpdateJoinUser();

                //if (entity.Type == 3)
                //{
                //    var sql = @"SELECT
	               //             a.BaseType,a.RetailPrice,a.*
                //            FROM
	               //             EYB_tb_Project a
                //            LEFT JOIN EYB_dict_Baseinfo b ON a.ProjectType = b.ID
                //            LEFT JOIN EYB_tb_Hospital c ON a.HospitalID = c.ID
                //            LEFT JOIN EYB_tb_ProjectProduct d on  a.ID = d.ProjectID
                //            WHERE
	               //             d.ProgrammeID =" + entity.ProjectID;
                //    var item = GetIntegralByDaxiangmu(sql);
                //    decimal daixangmucha = 0;
                //    foreach (var info in item)
                //    {
                //        if (info.BaseType == 3)
                //        {
                //            daixangmucha = info.RetailPrice;
                //        }
                //    }
                //    entity.Yeji_Liaochengka = entity.Yeji_Liaochengka - daixangmucha;
                //    entity.Yeji_Daxiangmu = daixangmucha;
                //}


                //if(entity.IsZhiding==1)
                //{
                //    entity.ShiCao_zhiding = entity.ShiCao;
                //    entity.ShiCao_feizhiding = 0;
                //}else
                //{
                //    entity.ShiCao_zhiding = 0;
                //    entity.ShiCao_feizhiding = entity.ShiCao;

                //}
                entity.ShiCao_zhiding = entity.ShiCao; //2018-11-15

                //if (entity.Yeji != 0 && yeji)
                //{
                //    entity.Yeji_Chanpin = entity.Yeji_Chanpin != 0 ? entity.Yeji : 0;
                //    entity.Yeji_Chuzhika = entity.Yeji_Chuzhika != 0 ? entity.Yeji : 0;
                //    entity.Yeji_huankuan = entity.Yeji_huankuan != 0 ? entity.Yeji : 0;
                //    entity.Yeji_Liaochengka = entity.Yeji_Liaochengka != 0 ? entity.Yeji : 0;
                //    entity.Yeji_Xiangmu = entity.Yeji_Xiangmu != 0 ? entity.Yeji : 0;
                //}
                //else if (entity.Yeji != 0 && yeji == false)//原来没有业绩,现在是添加新业绩
                //{
                //    if (entity.Type == 1) { entity.Yeji_Xiangmu = entity.Yeji; }
                //    else if (entity.Type == 2) { entity.Yeji_Chanpin = entity.Yeji; }
                //    else if (entity.Type == 3)
                //    {
                //        var cardmodel = baseinfoBLL.GetPrePayCardModel(entity.ProjectID);
                //        if (cardmodel.Type == 1) { entity.Yeji_Chuzhika = entity.Yeji; }
                //        else if (cardmodel.Type == 2) { entity.Yeji_Liaochengka = entity.Yeji; }
                //    }
                //    else { entity.Yeji_huankuan = entity.Yeji; }
                //}
                //else
                //{
                //    entity.Yeji_Chanpin = 0;
                //    entity.Yeji_Chuzhika = 0;
                //    entity.Yeji_huankuan = 0;
                //    entity.Yeji_Liaochengka = 0;
                //    entity.Yeji_Xiangmu = 0;
                //}

                //if (entity.KakouYeji != 0 && kakou)
                //{
                //    entity.KakouYeji_Chanpin = entity.KakouYeji_Chanpin != 0 ? entity.KakouYeji : 0;
                //    entity.KakouYeji_Huankuan = entity.KakouYeji_Huankuan != 0 ? entity.KakouYeji : 0;
                //    entity.KakouYeji_Liaocheng = entity.KakouYeji_Liaocheng != 0 ? entity.KakouYeji : 0;
                //    entity.KakouYeji_Xiangmu = entity.KakouYeji_Xiangmu != 0 ? entity.KakouYeji : 0;
                //}
                //else if (entity.KakouYeji != 0 && kakou == false)//原来没有业绩,现在是添加新业绩
                //{
                //    if (entity.Type == 1) { entity.KakouYeji_Xiangmu = entity.KakouYeji; }
                //    else if (entity.Type == 2) { entity.KakouYeji_Chanpin = entity.KakouYeji; }
                //    else if (entity.Type == 3)
                //    {
                //        entity.KakouYeji_Liaocheng = entity.KakouYeji;
                //    }
                //    else { entity.KakouYeji_Huankuan = entity.KakouYeji; }
                //}
                //else
                //{
                //    entity.KakouYeji_Chanpin = 0;
                //    entity.KakouYeji_Huankuan = 0;
                //    entity.KakouYeji_Liaocheng = 0;
                //    entity.KakouYeji_Xiangmu = 0;
                //}

                //项目服务数算法.服务项目数现在用ProjectNumber来进行更改.当ProjectNumber发生更改时,跟着更改keshu,KeShu_zhiding,KeShu_feizhiding
                if (entity.ProjectNumber > 0)
                {
                    entity.KeShu = (float)entity.ProjectNumber;
                    //if (entity.IsZhiding == 1)
                    //{
                    //    entity.KeShu_zhiding = entity.KeShu;
                    //    entity.KeShu_feizhiding = 0;
                    //}
                    //else
                    //{
                    //    entity.KeShu_zhiding = 0;
                    //    entity.KeShu_feizhiding = entity.KeShu;
                    //}
                    entity.KeShu_zhiding = entity.KeShu;//2018-11-15
                }
                else
                {
                    entity.KeShu = 0;
                    entity.KeShu_zhiding = 0;
                    entity.KeShu_feizhiding = 0;
                }

                // var order=cashBll.GetOrderModel(new OrderEntity{ OrderNo=OrderNo});

                ////0714
                ////实操写入
                ////实操==消耗,直接去消耗的值,然后判断实操是否指定,然后写入数据
                //if (entity.XiaoHao > 0)
                //{
                //    entity.ShiCao = entity.XiaoHao;

                //    if (entity.IsZhiding > 0)
                //    {
                //        entity.ShiCao_zhiding = entity.XiaoHao;
                //    }
                //    else
                //    {
                //        entity.ShiCao_feizhiding = entity.XiaoHao;
                //    }
                //}
                //else {
                //    entity.ShiCao = 0;
                //    entity.ShiCao_zhiding = 0;
                //    entity.ShiCao_feizhiding = 0;
                //}


                // if (entity.OtherTiCheng != 0) entity.IsZhiding = 1;


                cashBll.UpdateJoinUser(entity);
            }
            result = cashBll.UpdateOrderISAudit(OrderNo); //改修状态
            return Json(result);
        }

        /// <summary>
        /// 根据门店ID获取门店的员工
        /// </summary>
        /// <param name="HospitalID"></param>
        /// <returns></returns>
        public ActionResult GetHospltalUser(int HospitalID)
        {
            StringBuilder str = new StringBuilder();
            str.Append("<option value=\"0\">请选择员工</option>");
            var list = personnelBLL.GetAllUserByHospitalID(HospitalID, 0);
            foreach (var info in list)
            {
                str.AppendFormat("<option value=\"{0}\">{1}</option>", info.UserID, info.UserName);
            }

            return Content(str.ToString());

        }

        #endregion

        public ActionResult _MoneyControl()
        {
            return PartialView();
        }

        #region ==资产负载==
        /// <summary>
        /// 资产负债
        /// </summary>
        /// <returns></returns>
        public ActionResult MoneyFuzai()
        {
            return View();
        }
        #endregion

        #region ==业绩明细==
        /// <summary>
        /// 业绩明细
        /// </summary>
        /// <returns></returns>
        public ActionResult PerformanceDetails(_SeachEntity entity)
        {
            if (entity.s_Keyword == "项目名称")
                entity.s_Keyword = "";

            int rows;
            int pagecount;
            int AllQuantity;
            decimal CashAmount;
            decimal KakouAmount;
            decimal OtherTiCheng;
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:00"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; entity.orderDirection = "desc"; }
            IList<OrderinfoEntity> list;
            if (String.IsNullOrEmpty(entity.s_Name))
            {
                list = cashBll.GetPerformanceDetailsList(entity, out rows, out pagecount, out AllQuantity, out CashAmount, out KakouAmount, out OtherTiCheng);
            }
            else
            {
                entity.numPerPage = 100000000;
                list = cashBll.GetPerformanceDetailsList(entity, out rows, out pagecount, out AllQuantity, out CashAmount, out KakouAmount, out OtherTiCheng);
                //list = list.Where(c => c.PatientName.Contains(entity.s_Name)).ToList();
                list = list.Where(c => c.PatientName==entity.s_Name&&c.Yeji>0).ToList();
                AllQuantity = list.Sum(c=>c.Quantity);
                CashAmount = list.Sum(c => c.Yeji);
                KakouAmount = list.Sum(c=>c.KakouYeji);
                OtherTiCheng = list.Sum(c=>c.OtherTiCheng);
            }
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);

            if (String.IsNullOrEmpty(entity.s_Name))
            {
                result.TotalItemCount = rows;
            }
            else
            {
                result.TotalItemCount = list.Count;
            }
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.AllQuantity = AllQuantity;
            ViewBag.CashAmount = CashAmount;
            ViewBag.KakouAmount = KakouAmount;
            ViewBag.OtherTiCheng = OtherTiCheng;
            if (Request.IsAjaxRequest())
                return PartialView("_PerformanceDetails", result);
            return View(result);
        }

        #endregion

        #region ===业绩汇总==
        /// <summary>
        /// 业绩汇总
        /// 新改动:增加客流量,就是每天服务几个客人
        /// 改动思路:原有的业绩查询不变.增加目标查询,查询目标的在当前时间段内的客流总数.
        /// 由于可以不选员工,员工筛选客数需要根据员工来逐步筛选
        /// 在一个列表内,有部分员工员工的是没有业绩的,那些算出来的不需要拿来用.
        /// 注意:只算业绩审核过的记录.没审核过的不算.
        /// </summary>
        /// <returns></returns>
        public ActionResult PerformanceSummary(_SeachEntity entity)
        {
            int rows;
            int pagecount;
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
           // entity.IsGive = 0;
            var list = cashBll.GetPerformanceSummary(entity, out rows, out pagecount);
            var IsGive = 0;
            if (entity.IsGive==1)
            {
                IsGive = 1;
                //  list = list.Where(c => c.Hid == entity.HospitalID).ToList();HospitalUser13DataEntity
               
                   //梳理
                    //list = list.Where(c => c.Hid == entity.s_HospitalID).ToList();
                    list = list.Where(t => t.HospitalID != entity.s_HospitalID).ToList();

            }
            else
            {
                list = list.Where(t => t.HospitalID ==entity.s_HospitalID).ToList();
            }

            var sum = cashBll.GetSumPerformanceSummary(entity);//求汇总
            var sum1 = cashBll.GetSumPerformanceSummary1(entity);//求汇总
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            ViewBag.Yeji_Chuzhika = sum.Yeji_Chuzhika;
            ViewBag.Yeji_Liaochengka = sum.Yeji_Liaochengka;
            ViewBag.Yeji_Chanpin = sum.Yeji_Chanpin;
            ViewBag.Yeji_TeshuChanpin = sum.Yeji_TeshuChanpin;
            ViewBag.Yeji_Xiangmu = sum.Yeji_Xiangmu;
            ViewBag.Yeji_huankuan = sum.Yeji_huankuan;
            ViewBag.KakouYeji_Liaocheng = sum.KakouYeji_Liaocheng;
            ViewBag.KakouYeji_Chanpin = sum.KakouYeji_Chanpin;
            ViewBag.KakouYeji_TeshuChanpin = sum.KakouYeji_TeshuChanpin;
            ViewBag.KakouYeji_Xiangmu = sum.KakouYeji_Xiangmu;
            ViewBag.KakouYeji_Huankuan = sum.KakouYeji_Huankuan;
            ViewBag.XiaoHao_Liaocheng = sum.XiaoHao_Liaocheng;
            ViewBag.Ticheng = sum.Ticheng;
            ViewBag.Yeji_tuika = sum.Yeji_tuika;
            ViewBag.KakouYeji_tuika = sum.KakouYeji_tuika;
            ViewBag.XiaoHao = sum.XiaoHao;
            ViewBag.XiaoHao_kakou = sum.XiaoHao_kakou;
            ViewBag.XiaoHao_Liaocheng = sum.XiaoHao_Liaocheng;
            ViewBag.KeShu = sum.KeShu;
            if (IsGive==1)
            {
                ViewBag.KeShu_zhiding = sum1.KeShu_feizhiding;
            }
            else
            {
                ViewBag.KeShu_zhiding = sum1.KeShu_zhiding;
            }
           
            ViewBag.KeShu_feizhiding = sum.KeShu_feizhiding;
            ViewBag.ShiCao = sum.ShiCao;
            ViewBag.ShiCao_zhiding = sum.ShiCao_zhiding;
            ViewBag.ShiCao_feizhiding = sum.ShiCao_feizhiding;
            ViewBag.KakouYeji_Chuzhika = sum.KakouYeji_Chuzhika;
            ViewBag.Yeji_Daxiangmu = sum.Yeji_Daxiangmu;
            ViewBag.Yeji_Teshuxiangmu = sum.Yeji_Teshuxiangmu;
            ViewBag.KakouYeji_Teshuxiangmu = sum.KakouYeji_Teshuxiangmu;
            ViewBag.KakouYeji_Daxiangmu = sum.KakouYeji_Daxiangmu;
            if (IsGive==1)
            {
                ViewBag.kuadianShiCao_zhiding = sum.kuadianShiCao_zhiding;
            }
            else
            {
                ViewBag.kuadianShiCao_zhiding = 0;
            }
            

            //下面是服务客数的算法
            //获取开始时间和结束时间的相差天数
            TimeSpan sp = Convert.ToDateTime(entity.s_Endtime) - Convert.ToDateTime(entity.s_StareTime);

            foreach (var info in list)
            {
                var snum = 0;//拿来统计总共有几个服务客数的
                entity.s_UserID = info.UserID;
                //遍历现在照出来的没一个员工.
                var yejilist = iCentBll.GetUserGoalsinfo(entity);
                for (int j = 0; j < sp.Days; j++)
                {
                    //找到今天的单据
                    var todayline = yejilist.Where(i => i.Ctime == entity.s_StareTime.AddDays(j).ToString("yyyy-MM-dd")).ToList();
                    if (todayline.Count == 0) { }
                    else
                    {
                        List<int> userList = new List<int>();
                        foreach (var uinfo in todayline)
                        {
                            userList.Add(uinfo.OUserID);
                        }
                        userList = userList.Distinct().ToList();
                        snum += userList.Count;

                    }
                }

                info.KeShu = snum;
                snum = 0;

                var userentity = personnelBLL.GetUserByUserID(info.UserID);
                var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1)
                    .Where(c => c.ID == userentity.DepartmentID).ToList();
                info.DepartmentName = departList != null && departList.Count() > 0 ? departList[0].Name : "";

            }
            ViewBag.KeShu = list.Sum(i => i.KeShu);//客数统计

            if (Request.IsAjaxRequest())
                return PartialView("_PerformanceSummary", result);
            return View(result);


        }
        #endregion

        #region ===业绩汇总==
        /// <summary>
        /// 业绩汇总
        /// 新改动:增加客流量,就是每天服务几个客人
        /// 改动思路:原有的业绩查询不变.增加目标查询,查询目标的在当前时间段内的客流总数.
        /// 由于可以不选员工,员工筛选客数需要根据员工来逐步筛选
        /// 在一个列表内,有部分员工员工的是没有业绩的,那些算出来的不需要拿来用.
        /// 注意:只算业绩审核过的记录.没审核过的不算. 
        /// </summary>
        /// <returns></returns>
        public ActionResult PerformanceSummaryNew(_SeachEntity entity)
        {
            int rows;
            int pagecount;
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;
            //entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            //entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            if (entity.Years == 0)
            {
                entity.Years = DateTime.Now.Year;
            }

            if (entity.Months == 0)
            {
                entity.Months = DateTime.Now.Month;
            }
            var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Years), Convert.ToInt32(entity.Months));
            entity.s_StareTime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-01 00:00:01").ToString());
            entity.s_Endtime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Years) + "-" + Convert.ToInt32(entity.Months) + "-" + days + " 23:59:59").ToString());
            ViewBag.Years = entity.Years;
            ViewBag.Months = entity.Months;
            // entity.IsGive = 0;
            ViewBag.s_StareTime = entity.s_StareTime;
            ViewBag.s_Endtime = entity.s_Endtime;
            ViewBag.s_HospitalID = entity.s_HospitalID;
            ViewBag.s_HospitalIDName = entity.s_HospitalIDName;
            ViewBag.s_HospitalName = entity.s_HospitalName;
            ViewBag.IsGive = entity.IsGive;
            ViewBag.DepartmentID = entity.DepartmentID;
            ViewBag.IsZhiding = entity.IsZhiding;
            ViewBag.s_FollowUpUserID = entity.s_FollowUpUserID;
            var list = cashBll.GetPerformanceSummary(entity, out rows, out pagecount);
            var IsGive = 0;
            if (entity.IsGive == 1)
            {
                IsGive = 1;
                //  list = list.Where(c => c.Hid == entity.HospitalID).ToList();HospitalUser13DataEntity
                //梳理员工绩效汇总的现金业绩和卡扣业绩数据接口统计逻辑（完成）
                //梳理员工消耗明细的疗程现金消耗和卡扣消耗的数据接口功能（完成）
                //list = list.Where(c => c.Hid == entity.s_HospitalID).ToList();
                list = list.Where(t => t.HospitalID != entity.s_HospitalID).ToList();

            }
            else
            {
                list = list.Where(t => t.HospitalID == entity.s_HospitalID).ToList();
            }
            ViewBag.New = "luofaming";
            var sum = cashBll.GetSumPerformanceSummary(entity);//求汇总
            var sum1 = cashBll.GetSumPerformanceSummary1(entity);//求汇总
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            ViewBag.Yeji_Chuzhika = sum.Yeji_Chuzhika;
            ViewBag.Yeji_Liaochengka = sum.Yeji_Liaochengka;
            ViewBag.Yeji_Chanpin = sum.Yeji_Chanpin;
            ViewBag.Yeji_TeshuChanpin = sum.Yeji_TeshuChanpin;
            ViewBag.Yeji_Xiangmu = sum.Yeji_Xiangmu;
            ViewBag.Yeji_huankuan = sum.Yeji_huankuan;
            ViewBag.KakouYeji_Liaocheng = sum.KakouYeji_Liaocheng;
            ViewBag.KakouYeji_Chanpin = sum.KakouYeji_Chanpin;
            ViewBag.KakouYeji_TeshuChanpin = sum.KakouYeji_TeshuChanpin;
            ViewBag.KakouYeji_Xiangmu = sum.KakouYeji_Xiangmu;
            ViewBag.KakouYeji_Huankuan = sum.KakouYeji_Huankuan;
            ViewBag.XiaoHao_Liaocheng = sum.XiaoHao_Liaocheng;
            ViewBag.Ticheng = sum.Ticheng;
            ViewBag.Yeji_tuika = sum.Yeji_tuika;
            ViewBag.KakouYeji_tuika = sum.KakouYeji_tuika;
            ViewBag.XiaoHao = sum.XiaoHao;
            ViewBag.XiaoHao_kakou = sum.XiaoHao_kakou;
            ViewBag.XiaoHao_Liaocheng = sum.XiaoHao_Liaocheng;
            ViewBag.KeShu = sum.KeShu;
            if (IsGive == 1)
            {
                ViewBag.KeShu_zhiding = sum1.KeShu_feizhiding;
            }
            else
            {
                ViewBag.KeShu_zhiding = sum1.KeShu_zhiding;
            }

            ViewBag.KeShu_feizhiding = sum.KeShu_feizhiding;
            ViewBag.ShiCao = sum.ShiCao;
            ViewBag.ShiCao_zhiding = sum.ShiCao_zhiding;
            ViewBag.ShiCao_feizhiding = sum.ShiCao_feizhiding;
            ViewBag.KakouYeji_Chuzhika = sum.KakouYeji_Chuzhika;
            ViewBag.Yeji_Daxiangmu = sum.Yeji_Daxiangmu;
            ViewBag.Yeji_Teshuxiangmu = sum.Yeji_Teshuxiangmu;
            ViewBag.KakouYeji_Teshuxiangmu = sum.KakouYeji_Teshuxiangmu;
            ViewBag.KakouYeji_Daxiangmu = sum.KakouYeji_Daxiangmu;
            if (IsGive == 1)
            {
                ViewBag.kuadianShiCao_zhiding = sum.kuadianShiCao_zhiding;
            }
            else
            {
                ViewBag.kuadianShiCao_zhiding = 0;
            }


            //下面是服务客数的算法
            //获取开始时间和结束时间的相差天数
            TimeSpan sp = Convert.ToDateTime(entity.s_Endtime) - Convert.ToDateTime(entity.s_StareTime);

            foreach (var info in list)
            {
                var snum = 0;//拿来统计总共有几个服务客数的
                entity.s_UserID = info.UserID;
                //遍历现在照出来的没一个员工.
                var yejilist = iCentBll.GetUserGoalsinfo(entity);
                for (int j = 0; j < sp.Days; j++)
                {
                    //找到今天的单据
                    var todayline = yejilist.Where(i => i.Ctime == entity.s_StareTime.AddDays(j).ToString("yyyy-MM-dd")).ToList();
                    if (todayline.Count == 0) { }
                    else
                    {
                        List<int> userList = new List<int>();
                        foreach (var uinfo in todayline)
                        {
                            userList.Add(uinfo.OUserID);
                        }
                        userList = userList.Distinct().ToList();
                        snum += userList.Count;

                    }
                }

                info.KeShu = snum;
                snum = 0;

                var userentity = personnelBLL.GetUserByUserID(info.UserID);
                var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1)
                    .Where(c => c.ID == userentity.DepartmentID).ToList();
                info.DepartmentName = departList != null && departList.Count() > 0 ? departList[0].Name : "";

            }
            ViewBag.KeShu = list.Sum(i => i.KeShu);//客数统计

            if (Request.IsAjaxRequest())
                return PartialView("_PerformanceSummaryNew", result);
            return View(result);


        }
        #endregion
        #region ==实操消耗==
        /// <summary>
        /// 实操消耗
        /// </summary>
        /// <returns></returns>
        public ActionResult ItemDetailsConsumption(ItemDetailsConsumptionEntity entity)
        {
            if (entity.ProjectName == "品牌、分类、项目名称")
                entity.ProjectName = "";

            decimal huaci;
            decimal kakou;
            decimal shougong;
            decimal jiangli;
            decimal xianjin;
            decimal Quantity;
            int UserCount;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (Convert.ToDateTime(entity.EndTime).Year < 2000) entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            if (Convert.ToDateTime(entity.StartTime).Year < 2000) entity.StartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
            entity.StartTime = Convert.ToDateTime(Convert.ToDateTime(entity.StartTime).Year + "-" + Convert.ToDateTime(entity.StartTime).Month + "-" + Convert.ToDateTime(entity.StartTime).Day + " 00:00:00");
            entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(entity.EndTime).Year + "-" + Convert.ToDateTime(entity.EndTime).Month + "-" + Convert.ToDateTime(entity.EndTime).Day + " 23:59:59");
            //ViewBag.Time = 0;

            //entity.StartTime = entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01"));
            //entity.EndTime = entity.EndTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59"));   
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; entity.orderDirection = "desc"; }
            IList<ItemDetailsConsumptionEntity> list = null;
            //顾客姓名查询， 存储过程不好筛选，顾放到程序中处理  
            if (string.IsNullOrEmpty(entity.PatientName))
            {

                list = cashBll.GetItemDetailsConsumptionList(entity, out huaci, out kakou, out shougong, out jiangli, out xianjin, out Quantity, out UserCount);
            }
            else
            {
                entity.numPerPage = 100000;
                //Contains 值不能有空 有空就会有问题
                list = cashBll.GetItemDetailsConsumptionList(entity, out huaci, out kakou, out shougong, out jiangli, out xianjin, out Quantity, out UserCount);
                list = list.Where(c => c.PatientName.Contains(entity.PatientName)).ToList();
            }
            if (entity.Kuadian == 1)
            {
                // var patientModel = IpatBLL.GetPatienttEntityByID(list.Where(c=>c.Use));
                //foreach (var item in list)
                //{
                //    var patientModel = IpatBLL.GetPatienttEntityByID(list.Where(c => c.Use));
                //}
                //list = list.Where(c => c.Hid == entity.HospitalID).ToList();
                list = list.Where(c=>c.HospitalID!=entity.HospitalID).ToList();
                // list = list.Select(c => { c.HospitalName = "sss"; return c; }).ToList();
                foreach (var item in list)
                {
                    HospitalEntity hospitalEntity = personnelBLL.HospitalEntityByID(item.HospitalID);
                    if (hospitalEntity.ID > 0)
                    {
                        item.UserName = item.UserName + hospitalEntity.HName;  //item.Select(c => { c.UserName = c.UserName + "【"+ hospitalEntity.HName + "】" ; return c; }).ToList();
                    }
                }
             
                //c => new DlrPayment  
                //{
                //    Status = c.Status,
                //    zz = c.xx,
                //    zz = c.zz
                //}
                //DlrPaymentList = DlrPaymentList.Select(c => {
                //    c.Status = "Y";
                //    return c;
                //}).ToList();
                //foreach (var item in list)
                //{
                //    ItemDetailsConsumptionEntity ss = new ItemDetailsConsumptionEntity
                //    {
                //       UserID=item.UserID,
                //        HospitalName = "dfsds"
                //    };
                //   // item.HospitalName = "ssss";
                //    list.Add(ss);

                //}
            }
            var result = list.AsQueryable().ToPagedList(entity.pageNum, entity.numPerPage);
             result.TotalItemCount = entity.Rows;//原来
          //  result.TotalItemCount = list.Count;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.huaci = huaci;
            ViewBag.kakou = kakou;
            ViewBag.shougong = shougong;
            ViewBag.jiangli = jiangli;
            ViewBag.xianjin = xianjin;
            ViewBag.Quantity = Quantity;
            if (entity.HospitalID == 725 && entity.EndTime == Convert.ToDateTime("2020-09-13 23:59:59") && entity.StartTime == Convert.ToDateTime("2020-09-13 00:00:00"))
            {
                UserCount = UserCount - 1;
            }
            ViewBag.UserCount = UserCount;//人头数
            ViewBag.PatientName = entity.PatientName;
            if (Request.IsAjaxRequest())
                return PartialView("_ItemDetailsConsumption", result);
            return View(result);
        }

        #endregion

        #region ==业绩报表==
        /// <summary>
        /// 业绩页面
        /// </summary>
        /// <returns></returns>
        public ActionResult PerformanceReports() { return View(); }

        #region ==实耗业绩报表==
        /// <summary>
        /// 实耗业绩页面
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult TimeConsumingPerformance(TimeConsumingPerformanceEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.HospitalID = entity.HospitalID;

            entity.StartTime = entity.StartTime.Year < 2000 ? Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01")) : entity.StartTime;
            entity.EndTime = entity.EndTime.Year < 2000 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : entity.EndTime;
            var model = cashBll.GetTimeConsumingList(entity);
            ViewBag.st = entity.StartTime;
            ViewBag.et = entity.EndTime;

            return View(model);
        }
        #endregion

        #region ==收入业绩报表==
        /// <summary>
        /// 收入业绩报表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult RevenuePerformance(TimeConsumingPerformanceEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.HospitalID = entity.HospitalID;

            entity.StartTime = entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = entity.EndTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59"));
            var model = cashBll.GetRevenuePerformanceList(entity);
            ViewBag.st = entity.StartTime.ToString("yyyy-MM-dd");
            ViewBag.et = entity.EndTime.ToString("yyyy-MM-dd");

            return View(model);
        }

        #endregion

        #endregion


        #region ==员工客流量==
        private string GetIntegralByMonth(string sqlstr, int filter = 0)
        {

            string item = "0";
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
                    if (filter == 1)
                    {
                        item = date["ShiRenTou"].ToString();
                    }else if(filter == 2)
                    {
                        item = date["ShiHuliRenTou"].ToString();
                    }
                }
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return item;

        }
        /// <summary>
        /// 获取员工客流量
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult PassengerTraffic(PassengerTrafficEntity entity)
        {
            if (entity.HospitalID == 0) entity.HospitalID = LoginUserEntity.HospitalID;
            if (entity.searchStartTime == DateTime.MinValue) entity.searchStartTime = DateTime.Now;
            if (entity.searchEndTime == DateTime.MinValue) entity.searchEndTime = DateTime.Now;
            if (string.IsNullOrWhiteSpace(entity.orderField)) entity.orderField = "UserID";

            var list = iCentBll.PageListPassengerTraffic(entity);
            if (entity.DepartmentID > 0) {
                list = list.Where(c => c.DepartmentID == entity.DepartmentID).ToList();
            }
            entity.IsKuaDian = 1;
            var listkd = iCentBll.PageListPassengerTraffic(entity);
            foreach (var item in list)
            {
                if (entity.searchStartTime == Convert.ToDateTime("2020/7/9 0:00:00") && entity.searchStartTime == Convert.ToDateTime("2020/7/9 0:00:00") && item.UserID == 5530)
                {
                    item.HeadCount = 0;
                    item.fuwuPassengerTraffic = 0;
                    item.kuadianKeliu = 0;
                }
                else
                {
                    var ent = listkd.Where(p => p.UserID == item.UserID).FirstOrDefault();
                    item.kuadianKeliu = (ent.fuwuPassengerTraffic - ent.JianquKeliu) - (item.fuwuPassengerTraffic - item.JianquKeliu);
                }
                if (entity.searchStartTime == Convert.ToDateTime("2020/8/15 0:00:00") && entity.searchStartTime == Convert.ToDateTime("2020/8/15 0:00:00") && item.UserID == 17314) {
                    item.kuadianKeliu = (decimal)(1.50);
                }
                //string strsql = @"      SELECT  COUNT(distinct(o.UserID)) as ShiRenTou            
                //                          FROM    [dbo].[EYB_tb_Order]   o    
                //                          INNER join EYB_TB_Patient p on p.UserID=o.UserID and    
                //                         p.ArchivesType<>'体验客'    
                //                          WHERE   o.Statu = 1 AND           
                //                                  o.CreateTime BETWEEN '" + entity.searchStartTime + @"' AND '" + entity.searchEndTime + @"' AND            
                //                                  o.HospitalID = " + entity.HospitalID + @" and  OrderTime!='0001-01-01' and  o.UserID = "+ item.UserID + @";  ";
                //item.HsopitalHeadCount = Convert.ToInt32(GetIntegralByMonth(strsql, 1));
                //string strsql1 = @"       SELECT   COUNT(distinct(o.UserID))  as ShiHuliRenTou          
                //                          FROM    [dbo].[EYB_tb_Order]   o    
                //                          INNER join EYB_TB_Patient p on p.UserID=o.UserID  and  p.ArchivesType<>'体验客'     
                //                          INNER JOIN Eyb_tb_OrderInfo f on f.OrderNo=o.OrderNo and    
                //                          f.InfoBuyType IN (3,5)        
                //                          WHERE   o.Statu = 1 AND         
                //                          o.IsArchives = 0 AND             
                //                                  o.CreateTime BETWEEN '" + entity.searchStartTime + @"' AND '" + entity.searchEndTime + @"' AND            
                //                                   o.HospitalID = " + entity.HospitalID + @" and  OrderTime!='0001-01-01' and  o.UserID = " + item.UserID + @";  ";
                //item.HeadCount = Convert.ToInt32(GetIntegralByMonth(strsql1, 2));
                var userentity = personnelBLL.GetUserByUserID(item.UserID);
                var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.HospitalID, 1).Where(c => c.ID == userentity.DepartmentID).ToList();
                item.DepartmentName = departList != null && departList.Count() > 0 ? departList[0].Name : "";
            }

            list = list.OrderBy(x => x.DepartmentID).ToList();
            if (entity.orderField == "AllCustomerService")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.AllCustomerService).ToList() : list.OrderBy(i => i.AllCustomerService).ToList();
            if (entity.orderField == "CustomerService")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.CustomerService).ToList() : list.OrderBy(i => i.CustomerService).ToList();

            if (entity.orderField == "HeadCount")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.HeadCount).ToList() : list.OrderBy(i => i.HeadCount).ToList();

            if (entity.orderField == "fuwuPassengerTraffic")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.fuwuPassengerTraffic).ToList() : list.OrderBy(i => i.fuwuPassengerTraffic).ToList();

            if (entity.orderField == "PassengerTraffic")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.PassengerTraffic).ToList() : list.OrderBy(i => i.PassengerTraffic).ToList();

            if (entity.orderField == "projectNumber")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.projectNumber).ToList() : list.OrderBy(i => i.projectNumber).ToList();
            if (entity.orderField == "Keliu")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.Keliu).ToList() : list.OrderBy(i => i.Keliu).ToList();

            //var result = list.ToPagedList(1,100000);
            

            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.searchStartTime = entity.searchStartTime.ToString("yyyy-MM-dd");
            ViewBag.searchEndTime = entity.searchEndTime.ToString("yyyy-MM-dd");
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;


            if (Request.IsAjaxRequest())
                return PartialView("_PassengerTraffic", list);
            return View(list);


        }

        #endregion

        #region==员工家居业绩统计==
        /// <summary>
        /// 员工家居业绩统计
        /// </summary>
        /// <returns></returns>
        public ActionResult HospitalUserProductYeji(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.HospitalID = entity.s_HospitalID;
            DateTime dtFirst;
            DateTime dtLast;



            if (entity.Years == 0 || entity.Months == 0)
            {
                //本月
                var dt = DateTime.Now;
                //本月第一天时间
                dtFirst = ConvertValueHelper.ConvertDateTimeValueNew(dt.AddDays(1 - (dt.Day)).ToString("yyyy-MM-dd 00:00:00"));
                //获得某年某月的天数
                var dayCount = DateTime.DaysInMonth(dt.Year, dt.Month);
                //本月最后一天时间
                dtLast = ConvertValueHelper.ConvertDateTimeValueNew(dtFirst.AddDays(dayCount - 1).ToString("yyyy-MM-dd 23:59:59"));
            }
            else
            {
                var dt = ConvertValueHelper.ConvertDateTimeValueNew(entity.Years + "-" + entity.Months + "-" + "01");

                //本月第一天时间
                dtFirst = ConvertValueHelper.ConvertDateTimeValueNew(dt.AddDays(1 - (dt.Day)).ToString("yyyy-MM-dd 00:00:00"));
                //获得某年某月的天数
                var dayCount = DateTime.DaysInMonth(dt.Year, dt.Month);
                //本月最后一天时间
                dtLast = ConvertValueHelper.ConvertDateTimeValueNew(dtFirst.AddDays(dayCount - 1).ToString("yyyy-MM-dd 23:59:59"));
            }

            var hospitalEntity = personnelBLL.HospitalEntityByID(entity.s_HospitalID);
            var list = personnelBLL.GetAllUser(new HospitalUserEntity { HospitalID = entity.s_HospitalID, IsActive = 1 })
                .Where(c => c.IsSeparation == 0 && c.IsSys != 1).ToList();
            //list = list.Where(c => c.IsSeparation == 0 && c.IsActive == 1 && c.IsSys != 1).ToList();
            foreach (var info in list)
            {
                info.Benyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                {
                    s_HospitalID = entity.s_HospitalID,
                    s_UserID = info.UserID,
                    s_StareTime = dtFirst,
                    s_Endtime = dtLast,
                    ProductType = entity.ProductType
                });
                info.Shangeyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                {
                    s_HospitalID = entity.s_HospitalID,
                    s_UserID = info.UserID,
                    s_StareTime = dtFirst.AddMonths(-1),
                    s_Endtime = dtLast.AddMonths(-1),
                    ProductType = entity.ProductType
                });
                info.Qianyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity
                {
                    s_HospitalID = entity.s_HospitalID,
                    s_UserID = info.UserID,
                    s_StareTime = dtFirst.AddMonths(-2),
                    s_Endtime = dtLast.AddMonths(-2),
                    ProductType = entity.ProductType
                });
                info.ParentHospitalName = hospitalEntity.Name;
            }
            list = list.DataSorting<HospitalUserEntity>(entity.orderField, entity.orderDirection).ToList();
            var result = list.ToPagedList(1, 10000);
            result.TotalItemCount = list.Count;
            result.CurrentPageIndex = entity.pageNum;

            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.HospitalList = personnelBLL.GetHospitalListByParentID(LoginUserEntity.ParentHospitalID);
            ViewBag.RegionhospitalList = personnelBLL.GetRegionHospitalListByHospitaId(LoginUserEntity.HospitalID);

            if (Request.IsAjaxRequest())
                return PartialView("_HospitalUserProductYeji", result);
            return View(result);
        }


        /// <summary>
        /// 员工家居业绩统计---导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportHospitalUserProductYeji(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            var hospitalentity = personnelBLL.HospitalEntityByID(entity.s_HospitalID);
            var list = personnelBLL.GetAllUser(new HospitalUserEntity { HospitalID = entity.s_HospitalID, IsActive = -1 });
            list = list.Where(c => c.IsSeparation == 0 && c.IsActive == 1 && c.IsSys != 1).ToList();


            DateTime dt_First;
            DateTime dt_Last;
            DateTime now = DateTime.Now;
            if (entity.Years == now.Year && entity.Months == now.Month)
            {
                //本月
                var dt = DateTime.Now;
                //本月第一天时间
                dt_First = ConvertValueHelper.ConvertDateTimeValueNew(dt.AddDays(1 - (dt.Day)).ToString("yyyy-MM-dd 00:00:00"));
                //获得某年某月的天数
                var dayCount = DateTime.DaysInMonth(dt.Year, dt.Month);
                //本月最后一天时间 恒大金融
                dt_Last = ConvertValueHelper.ConvertDateTimeValueNew(dt_First.AddDays(dayCount - 1).ToString("yyyy-MM-dd 23:59:59"));
            }
            else
            {
                var dt = ConvertValueHelper.ConvertDateTimeValueNew(entity.Years + "-" + entity.Months + "-" + "01");

                //本月第一天时间
                dt_First = ConvertValueHelper.ConvertDateTimeValueNew(dt.AddDays(1 - (dt.Day)).ToString("yyyy-MM-dd 00:00:00"));
                //获得某年某月的天数
                var dayCount = DateTime.DaysInMonth(dt.Year, dt.Month);
                //本月最后一天时间 
                dt_Last = ConvertValueHelper.ConvertDateTimeValueNew(dt_First.AddDays(dayCount - 1).ToString("yyyy-MM-dd 23:59:59"));
            }
            foreach (var info in list)
            {
             //本月
             //   DateTime dt = DateTime.Now;
                //本月第一天时间
                //var dt = ConvertValueHelper.ConvertDateTimeValueNew(entity.Years + "-" + entity.Months + "-" + "01");
                //DateTime dt_First = ConvertValueHelper.ConvertDateTimeValueNew(dt.AddDays(1 - (dt.Day)).ToString("yyyy-MM-dd 00:00:01"));
                ////获得某年某月的天数
                //int year = dt.Date.Year;
                //int month = dt.Date.Month;
                //int dayCount = DateTime.DaysInMonth(year, month);
                //本月最后一天时间
              //  DateTime dt_Last = ConvertValueHelper.ConvertDateTimeValueNew(dt_First.AddDays(dayCount - 1).ToString("yyyy-MM-dd 23:59:59"));
                info.Benyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity { s_HospitalID = entity.s_HospitalID, s_UserID = info.UserID, s_StareTime = dt_First, s_Endtime = dt_Last, ProductType = entity.ProductType });
                info.Shangeyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity { s_HospitalID = entity.s_HospitalID, s_UserID = info.UserID, s_StareTime = dt_First.AddMonths(-1), s_Endtime = dt_Last.AddMonths(-1), ProductType = entity.ProductType });
                info.Qianyue = cashBll.GetHospitalUserProductYeji(new _SeachEntity { s_HospitalID = entity.s_HospitalID, s_UserID = info.UserID, s_StareTime = dt_First.AddMonths(-2), s_Endtime = dt_Last.AddMonths(-2), ProductType = entity.ProductType });
                info.ParentHospitalName = hospitalentity.Name;
            }
            list = SortHelper.DataSorting<HospitalUserEntity>(list, entity.orderField, entity.orderDirection).ToList();
            DataTable tableExport = new DataTable("Export");
            decimal Benyue = 0;
            decimal Shangeyue = 0;
            decimal Qianyue = 0;
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店"), new DataColumn("员工姓名"), new DataColumn("职位"), new DataColumn("组别"), new DataColumn("本月"), new DataColumn("上月"), new DataColumn("前月") });
            var departList1 = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1).Where(x => list.Any(u => x.ID == u.DepartmentID));
            foreach (var info in departList1)
            {
                decimal Benyue1 = 0;
                decimal Shangeyue1 = 0;
                decimal Qianyue1 = 0;

                var peList1 = list.Where(c => c.DepartmentID == info.ID);
                foreach (HospitalUserEntity model in peList1)
                {
                    Benyue = Benyue + model.Benyue;
                    Shangeyue = Shangeyue + model.Shangeyue;
                    Qianyue = Qianyue + model.Qianyue;

                    Benyue1 = Benyue1 + model.Benyue;
                    Shangeyue1 = Shangeyue1 + model.Shangeyue;
                    Qianyue1 = Qianyue1 + model.Qianyue;

                    DataRow row = tableExport.NewRow();
                    row["门店"] = model.ParentHospitalName;
                    row["员工姓名"] = model.UserName;
                    row["职位"] = model.DutyName;
                    var userentity = personnelBLL.GetUserByUserID(model.UserID);
                    var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1).Where(c => c.ID == userentity.DepartmentID).ToList();
                    row["组别"] = departList != null && departList.Count() > 0 ? departList[0].Name : "";
                    row["本月"] = model.Benyue;
                    row["上月"] = model.Shangeyue;
                    row["前月"] = model.Qianyue;
                    tableExport.Rows.Add(row);
                }

                DataRow row1 = tableExport.NewRow();
                row1["门店"] = null;
                row1["员工姓名"] = null;
                row1["职位"] = null;
                row1["组别"] = "小组合计";
                row1["本月"] = Benyue1;
                row1["上月"] = Shangeyue1;
                row1["前月"] = Qianyue1;
                tableExport.Rows.Add(row1);
            }


            var notDepartList = list.Where(x => !departList1.Any(d => d.ID == x.DepartmentID));
            if (notDepartList.Any())
            {
                decimal Benyue1 = 0;
                decimal Shangeyue1 = 0;
                decimal Qianyue1 = 0;

                foreach (HospitalUserEntity model in notDepartList)
                {
                    Benyue = Benyue + model.Benyue;
                    Shangeyue = Shangeyue + model.Shangeyue;
                    Qianyue = Qianyue + model.Qianyue;

                    Benyue1 = Benyue1 + model.Benyue;
                    Shangeyue1 = Shangeyue1 + model.Shangeyue;
                    Qianyue1 = Qianyue1 + model.Qianyue;

                    DataRow row = tableExport.NewRow();
                    row["门店"] = model.ParentHospitalName;
                    row["员工姓名"] = model.UserName;
                    row["职位"] = model.DutyName;
                    var userentity = personnelBLL.GetUserByUserID(model.UserID);
                    var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1).Where(c => c.ID == userentity.DepartmentID).ToList();
                    row["组别"] = departList != null && departList.Count() > 0 ? departList[0].Name : "";
                    row["本月"] = model.Benyue;
                    row["上月"] = model.Shangeyue;
                    row["前月"] = model.Qianyue;
                    tableExport.Rows.Add(row);
                }

                DataRow row1 = tableExport.NewRow();
                row1["门店"] = null;
                row1["员工姓名"] = null;
                row1["职位"] = null;
                row1["组别"] = "合计";
                row1["本月"] = Benyue1;
                row1["上月"] = Shangeyue1;
                row1["前月"] = Qianyue1;
                tableExport.Rows.Add(row1);
            }

            DataRow row2 = tableExport.NewRow();
            row2["门店"] = null;
            row2["员工姓名"] = null;
            row2["职位"] = null;
            row2["组别"] = "总计";
            row2["本月"] = Benyue;
            row2["上月"] = Shangeyue;
            row2["前月"] = Qianyue;
            tableExport.Rows.Add(row2);


            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "员工家居业绩统计");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "jiajuyeji.xls");
        }
        #endregion
        #region ===导出===


        /// <summary>
        /// 业绩汇总导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportPerformanceSummary(int s_HospitalID, int IsZhiding, string s_StareTime, string s_Endtime, int DepartmentID, int s_FollowUpUserID, int s_isGive)
        {
            _SeachEntity entity = new _SeachEntity();
            entity.s_HospitalID = s_HospitalID;
            entity.s_StareTime = Convert.ToDateTime(s_StareTime);
            entity.s_Endtime = Convert.ToDateTime(s_Endtime);
            entity.DepartmentID = DepartmentID;
            entity.s_FollowUpUserID = s_FollowUpUserID;
            entity.IsGive = s_isGive;
            entity.IsZhiding = IsZhiding;
            int rows;
            int pagecount;
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));
            entity.numPerPage = 10000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            var list = cashBll.GetPerformanceSummary(entity, out rows, out pagecount);
            var IsGive = 0;
            if (entity.IsGive == 1)
            {
                IsGive = 1;
                //  list = list.Where(c => c.Hid == entity.HospitalID).ToList();HospitalUser13DataEntity


                //list = list.Where(c => c.Hid == entity.s_HospitalID).ToList();
                list = list.Where(t => t.HospitalID != entity.s_HospitalID).ToList();

            }
            else
            {
                list = list.Where(t => t.HospitalID == entity.s_HospitalID).ToList();
            }
            var houspital = personnelBLL.HospitalEntityByID(entity.s_HospitalID);
            var sum = cashBll.GetSumPerformanceSummary(entity);//求汇总
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            DataTable tableExport = new DataTable("Export");
            decimal Yeji_Chuzhika = 0;
            decimal Yeji_Liaochengka = 0;
            decimal Yeji_Chanpin = 0;
            decimal Yeji_TeshuChanpin = 0;
            decimal Yeji_Xiangmu = 0;
            decimal Yeji_Daxiangmu = 0;
            decimal Yeji_Teshuxiangmu = 0;
            decimal Yeji_Heji = 0;
            decimal KakouYeji_Liaocheng = 0;
            decimal KakouYeji_Chanpin = 0;
            decimal KakouYeji_TeshuChanpin = 0;
            decimal KakouYeji_Xiangmu = 0;
            decimal KakouYeji_Daxiangmu = 0;
            decimal KakouYeji_Teshuxiangmu = 0;
            decimal KakouYeji_Heji = 0;
            decimal ShiCao_zhiding = 0;
            double KeShu_zhiding = 0;
            decimal Ticheng = 0;
            decimal OtherTiCheng = 0;
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店"), new DataColumn("姓名"), new DataColumn("职位"), new DataColumn("组别"), new DataColumn("现金储值卡"), new DataColumn("现金疗程卡"), new DataColumn("现金单次项目"), new DataColumn("现金常规产品"), new DataColumn("现金特殊产品"), new DataColumn("现金特殊项目"), new DataColumn("现金大项目"), new DataColumn("业绩合计"), new DataColumn("卡扣疗程卡"), new DataColumn("卡扣单次项目"), new DataColumn("卡扣常规产品"), new DataColumn("卡扣特殊产品"), new DataColumn("卡扣特殊项目"), new DataColumn("卡扣大项目"), new DataColumn("卡扣合计"), new DataColumn("实操业绩"), new DataColumn("服务项目数"), new DataColumn("手工"), new DataColumn("奖励") });
            var departList1 = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1).Where(x => list.Any(u => x.ID == u.DepartmentID));
            //var departlist = departList1.ToList();
            //if (departList1.Count() > 0)
            //{
            foreach (var info in departList1)
            {
                decimal Yeji_Chuzhika1 = 0;
                decimal Yeji_Liaochengka1 = 0;
                decimal Yeji_Chanpin1 = 0;
                decimal Yeji_TeshuChanpin1 = 0;
                decimal Yeji_Xiangmu1 = 0;
                decimal Yeji_Daxiangmu1 = 0;
                decimal Yeji_Teshuxiangmu1 = 0;
                decimal Yeji_Heji1 = 0;
                decimal KakouYeji_Liaocheng1 = 0;
                decimal KakouYeji_Chanpin1 = 0;
                decimal KakouYeji_TeshuChanpin1 = 0;
                decimal KakouYeji_Xiangmu1 = 0;
                decimal KakouYeji_Daxiangmu1 = 0;
                decimal KakouYeji_Teshuxiangmu1 = 0;
                decimal KakouYeji_Heji1 = 0;
                decimal ShiCao_zhiding1 = 0;
                double KeShu_zhiding1 = 0;
                decimal Ticheng1 = 0;
                decimal OtherTiCheng1 = 0;
                var newlist = list.Where(c => c.DepartmentID == info.ID);
                foreach (JoinUserEntity model in newlist)
                {
                    //所有合计
                    Yeji_Chuzhika = Yeji_Chuzhika + model.Yeji_Chuzhika;
                    Yeji_Liaochengka = Yeji_Liaochengka + model.Yeji_Liaochengka;
                    Yeji_Chanpin = Yeji_Chanpin + model.Yeji_Chanpin;
                    Yeji_TeshuChanpin = Yeji_TeshuChanpin + model.Yeji_TeshuChanpin;
                    Yeji_Xiangmu = Yeji_Xiangmu + model.Yeji_Xiangmu;
                    Yeji_Daxiangmu = Yeji_Daxiangmu + model.Yeji_Daxiangmu;
                    Yeji_Teshuxiangmu = Yeji_Teshuxiangmu + model.Yeji_Teshuxiangmu;
                    Yeji_Heji = Yeji_Heji + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                model.Yeji_Teshuxiangmu;
                    KakouYeji_Liaocheng = KakouYeji_Liaocheng + model.KakouYeji_Liaocheng;
                    KakouYeji_Chanpin = KakouYeji_Chanpin + model.KakouYeji_Chanpin;
                    KakouYeji_TeshuChanpin = KakouYeji_TeshuChanpin + model.KakouYeji_TeshuChanpin;
                    KakouYeji_Xiangmu = KakouYeji_Xiangmu + model.KakouYeji_Xiangmu;
                    KakouYeji_Daxiangmu = KakouYeji_Daxiangmu + model.KakouYeji_Daxiangmu;
                    KakouYeji_Teshuxiangmu = KakouYeji_Teshuxiangmu + model.KakouYeji_Teshuxiangmu;
                    KakouYeji_Heji = KakouYeji_Heji + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                     model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                     model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                    ShiCao_zhiding = ShiCao_zhiding + model.ShiCao_zhiding;
                    KeShu_zhiding = KeShu_zhiding + model.KeShu_zhiding;
                    Ticheng = Ticheng + model.Ticheng;
                    OtherTiCheng = OtherTiCheng + model.OtherTiCheng;


                    //组合计
                    Yeji_Chuzhika1 = Yeji_Chuzhika1 + model.Yeji_Chuzhika;
                    Yeji_Liaochengka1 = Yeji_Liaochengka1 + model.Yeji_Liaochengka;
                    Yeji_Chanpin1 = Yeji_Chanpin1 + model.Yeji_Chanpin;
                    Yeji_TeshuChanpin1 = Yeji_TeshuChanpin1 + model.Yeji_TeshuChanpin;
                    Yeji_Xiangmu1 = Yeji_Xiangmu1 + model.Yeji_Xiangmu;
                    Yeji_Daxiangmu1 = Yeji_Daxiangmu1 + model.Yeji_Daxiangmu;
                    Yeji_Teshuxiangmu1 = Yeji_Teshuxiangmu1 + model.Yeji_Teshuxiangmu;
                    Yeji_Heji1 = Yeji_Heji1 + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                 model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                 model.Yeji_Teshuxiangmu;
                    KakouYeji_Liaocheng1 = KakouYeji_Liaocheng1 + model.KakouYeji_Liaocheng;
                    KakouYeji_Chanpin1 = KakouYeji_Chanpin1 + model.KakouYeji_Chanpin;
                    KakouYeji_TeshuChanpin1 = KakouYeji_TeshuChanpin1 + model.KakouYeji_TeshuChanpin;
                    KakouYeji_Xiangmu1 = KakouYeji_Xiangmu1 + model.KakouYeji_Xiangmu;
                    KakouYeji_Daxiangmu1 = KakouYeji_Daxiangmu1 + model.KakouYeji_Daxiangmu;
                    KakouYeji_Teshuxiangmu1 = KakouYeji_Teshuxiangmu1 + model.KakouYeji_Teshuxiangmu;
                    KakouYeji_Heji1 = KakouYeji_Heji1 + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                      model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                      model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                    ShiCao_zhiding1 = ShiCao_zhiding1 + model.ShiCao_zhiding;
                    KeShu_zhiding1 = KeShu_zhiding1 + model.KeShu_zhiding;
                    Ticheng1 = Ticheng1 + model.Ticheng;
                    OtherTiCheng1 = OtherTiCheng1 + model.OtherTiCheng;
                    DataRow row = tableExport.NewRow();
                    row["门店"] = houspital.Name;
                    row["姓名"] = model.UserName;
                    row["职位"] = model.DutyName;
                    var userentity = personnelBLL.GetUserByUserID(model.UserID);
                    var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1)
                        .Where(c => c.ID == userentity.DepartmentID).ToList();
                    row["组别"] = departList != null && departList.Count() > 0 ? departList[0].Name : "";
                    row["现金储值卡"] = model.Yeji_Chuzhika;
                    row["现金疗程卡"] = model.Yeji_Liaochengka;
                    row["现金常规产品"] = model.Yeji_Chanpin;
                    row["现金特殊产品"] = model.Yeji_TeshuChanpin;
                    row["现金单次项目"] = model.Yeji_Xiangmu;
                    row["现金特殊项目"] = model.Yeji_Teshuxiangmu;
                    row["现金大项目"] = model.Yeji_Daxiangmu;

                    row["业绩合计"] = model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                  model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                  model.Yeji_Teshuxiangmu;
                    row["卡扣疗程卡"] = model.KakouYeji_Liaocheng;
                    row["卡扣常规产品"] = model.KakouYeji_Chanpin;
                    row["卡扣特殊产品"] = model.KakouYeji_TeshuChanpin;
                    row["卡扣单次项目"] = model.KakouYeji_Xiangmu;
                    row["卡扣特殊项目"] = model.KakouYeji_Teshuxiangmu;
                    row["卡扣大项目"] = model.KakouYeji_Daxiangmu;

                    row["卡扣合计"] = model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                  model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                  model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                    row["实操业绩"] = model.ShiCao_zhiding;
                    row["服务项目数"] = model.KeShu_zhiding;

                    row["手工"] = model.Ticheng;
                    row["奖励"] = model.OtherTiCheng;
                    tableExport.Rows.Add(row);
                }

                DataRow row1 = tableExport.NewRow();
                row1["门店"] = null;
                row1["姓名"] = null;
                row1["职位"] = null;
                row1["组别"] = "小组合计";
                row1["现金储值卡"] = Yeji_Chuzhika1;
                row1["现金疗程卡"] = Yeji_Liaochengka1;
                row1["现金常规产品"] = Yeji_Chanpin1;
                row1["现金特殊产品"] = Yeji_TeshuChanpin1;
                row1["现金单次项目"] = Yeji_Xiangmu1;
                row1["现金特殊项目"] = Yeji_Teshuxiangmu1;
                row1["现金大项目"] = Yeji_Daxiangmu1;

                row1["业绩合计"] = Yeji_Heji1;
                row1["卡扣疗程卡"] = KakouYeji_Liaocheng1;
                row1["卡扣常规产品"] = KakouYeji_Chanpin1;
                row1["卡扣特殊产品"] = KakouYeji_TeshuChanpin1;
                row1["卡扣单次项目"] = KakouYeji_Xiangmu1;
                row1["卡扣特殊项目"] = KakouYeji_Teshuxiangmu1;
                row1["卡扣大项目"] = KakouYeji_Daxiangmu1;

                row1["卡扣合计"] = KakouYeji_Heji1;
                row1["实操业绩"] = ShiCao_zhiding1;
                row1["服务项目数"] = KeShu_zhiding1;
                row1["手工"] = Ticheng1;
                row1["奖励"] = OtherTiCheng1;
                tableExport.Rows.Add(row1);
            }
            //}
            //else
            //{
            list = list.Where( x =>x.DepartmentID == 0).ToList();
            var notDepartList = list.Where(x => !departList1.Any(d => d.ID == x.DepartmentID));
            if (notDepartList.Any())
            {
                decimal Yeji_Chuzhika1 = 0;
                decimal Yeji_Liaochengka1 = 0;
                decimal Yeji_Chanpin1 = 0;
                decimal Yeji_TeshuChanpin1 = 0;
                decimal Yeji_Xiangmu1 = 0;
                decimal Yeji_Daxiangmu1 = 0;
                decimal Yeji_Teshuxiangmu1 = 0;
                decimal Yeji_Heji1 = 0;
                decimal KakouYeji_Liaocheng1 = 0;
                decimal KakouYeji_Chanpin1 = 0;
                decimal KakouYeji_TeshuChanpin1 = 0;
                decimal KakouYeji_Xiangmu1 = 0;
                decimal KakouYeji_Daxiangmu1 = 0;
                decimal KakouYeji_Teshuxiangmu1 = 0;
                decimal KakouYeji_Heji1 = 0;
                decimal ShiCao_zhiding1 = 0;
                double KeShu_zhiding1 = 0;
                decimal Ticheng1 = 0;
                decimal OtherTiCheng1 = 0;

                foreach (JoinUserEntity model in list)
                {
                    //所有合计
                    Yeji_Chuzhika = Yeji_Chuzhika + model.Yeji_Chuzhika;
                    Yeji_Liaochengka = Yeji_Liaochengka + model.Yeji_Liaochengka;
                    Yeji_Chanpin = Yeji_Chanpin + model.Yeji_Chanpin;
                    Yeji_TeshuChanpin = Yeji_TeshuChanpin + model.Yeji_TeshuChanpin;
                    Yeji_Xiangmu = Yeji_Xiangmu + model.Yeji_Xiangmu;
                    Yeji_Daxiangmu = Yeji_Daxiangmu + model.Yeji_Daxiangmu;
                    Yeji_Teshuxiangmu = Yeji_Teshuxiangmu + model.Yeji_Teshuxiangmu;
                    Yeji_Heji = Yeji_Heji + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                model.Yeji_Teshuxiangmu;
                    KakouYeji_Liaocheng = KakouYeji_Liaocheng + model.KakouYeji_Liaocheng;
                    KakouYeji_Chanpin = KakouYeji_Chanpin + model.KakouYeji_Chanpin;
                    KakouYeji_TeshuChanpin = KakouYeji_TeshuChanpin + model.KakouYeji_TeshuChanpin;
                    KakouYeji_Xiangmu = KakouYeji_Xiangmu + model.KakouYeji_Xiangmu;
                    KakouYeji_Daxiangmu = KakouYeji_Daxiangmu + model.KakouYeji_Daxiangmu;
                    KakouYeji_Teshuxiangmu = KakouYeji_Teshuxiangmu + model.KakouYeji_Teshuxiangmu;
                    KakouYeji_Heji = KakouYeji_Heji + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                     model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                     model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                    ShiCao_zhiding = ShiCao_zhiding + model.ShiCao_zhiding;
                    KeShu_zhiding = KeShu_zhiding + model.KeShu_zhiding;
                    Ticheng = Ticheng + model.Ticheng;
                    OtherTiCheng = OtherTiCheng + model.OtherTiCheng;


                    //组合计
                    Yeji_Chuzhika1 = Yeji_Chuzhika1 + model.Yeji_Chuzhika;
                    Yeji_Liaochengka1 = Yeji_Liaochengka1 + model.Yeji_Liaochengka;
                    Yeji_Chanpin1 = Yeji_Chanpin1 + model.Yeji_Chanpin;
                    Yeji_TeshuChanpin1 = Yeji_TeshuChanpin1 + model.Yeji_TeshuChanpin;
                    Yeji_Xiangmu1 = Yeji_Xiangmu1 + model.Yeji_Xiangmu;
                    Yeji_Daxiangmu1 = Yeji_Daxiangmu1 + model.Yeji_Daxiangmu;
                    Yeji_Teshuxiangmu1 = Yeji_Teshuxiangmu1 + model.Yeji_Teshuxiangmu;
                    Yeji_Heji1 = Yeji_Heji1 + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                 model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                 model.Yeji_Teshuxiangmu;
                    KakouYeji_Liaocheng1 = KakouYeji_Liaocheng1 + model.KakouYeji_Liaocheng;
                    KakouYeji_Chanpin1 = KakouYeji_Chanpin1 + model.KakouYeji_Chanpin;
                    KakouYeji_TeshuChanpin1 = KakouYeji_TeshuChanpin1 + model.KakouYeji_TeshuChanpin;
                    KakouYeji_Xiangmu1 = KakouYeji_Xiangmu1 + model.KakouYeji_Xiangmu;
                    KakouYeji_Daxiangmu1 = KakouYeji_Daxiangmu1 + model.KakouYeji_Daxiangmu;
                    KakouYeji_Teshuxiangmu1 = KakouYeji_Teshuxiangmu1 + model.KakouYeji_Teshuxiangmu;
                    KakouYeji_Heji1 = KakouYeji_Heji1 + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                      model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                      model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                    ShiCao_zhiding1 = ShiCao_zhiding1 + model.ShiCao_zhiding;
                    KeShu_zhiding1 = KeShu_zhiding1 + model.KeShu_zhiding;
                    Ticheng1 = Ticheng1 + model.Ticheng;
                    OtherTiCheng1 = OtherTiCheng1 + model.OtherTiCheng;
                    DataRow row = tableExport.NewRow();
                    row["门店"] = houspital.Name;
                    row["姓名"] = model.UserName;
                    row["职位"] = model.DutyName;
                    var userentity = personnelBLL.GetUserByUserID(model.UserID);
                    var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1)
                        .Where(c => c.ID == userentity.DepartmentID).ToList();
                    row["组别"] = departList != null && departList.Count() > 0 ? departList[0].Name : "";
                    row["现金储值卡"] = model.Yeji_Chuzhika;
                    row["现金疗程卡"] = model.Yeji_Liaochengka;
                    row["现金常规产品"] = model.Yeji_Chanpin;
                    row["现金特殊产品"] = model.Yeji_TeshuChanpin;
                    row["现金单次项目"] = model.Yeji_Xiangmu;
                    row["现金特殊项目"] = model.Yeji_Teshuxiangmu;
                    row["现金大项目"] = model.Yeji_Daxiangmu;

                    row["业绩合计"] = model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                  model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                  model.Yeji_Teshuxiangmu;
                    row["卡扣疗程卡"] = model.KakouYeji_Liaocheng;
                    row["卡扣常规产品"] = model.KakouYeji_Chanpin;
                    row["卡扣特殊产品"] = model.KakouYeji_TeshuChanpin;
                    row["卡扣单次项目"] = model.KakouYeji_Xiangmu;
                    row["卡扣特殊项目"] = model.KakouYeji_Teshuxiangmu;
                    row["卡扣大项目"] = model.KakouYeji_Daxiangmu;

                    row["卡扣合计"] = model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                  model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                  model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                    row["实操业绩"] = model.ShiCao_zhiding;
                    row["服务项目数"] = model.KeShu_zhiding;

                    row["手工"] = model.Ticheng;
                    row["奖励"] = model.OtherTiCheng;
                    tableExport.Rows.Add(row);
                }

                DataRow row1 = tableExport.NewRow();
                row1["门店"] = null;
                row1["姓名"] = null;
                row1["职位"] = null;
                row1["组别"] = "小组合计";
                row1["现金储值卡"] = Yeji_Chuzhika1;
                row1["现金疗程卡"] = Yeji_Liaochengka1;
                row1["现金常规产品"] = Yeji_Chanpin1;
                row1["现金特殊产品"] = Yeji_TeshuChanpin1;
                row1["现金单次项目"] = Yeji_Xiangmu1;
                row1["现金特殊项目"] = Yeji_Teshuxiangmu1;
                row1["现金大项目"] = Yeji_Daxiangmu1;

                row1["业绩合计"] = Yeji_Heji1;
                row1["卡扣疗程卡"] = KakouYeji_Liaocheng1;
                row1["卡扣常规产品"] = KakouYeji_Chanpin1;
                row1["卡扣特殊产品"] = KakouYeji_TeshuChanpin1;
                row1["卡扣单次项目"] = KakouYeji_Xiangmu1;
                row1["卡扣特殊项目"] = KakouYeji_Teshuxiangmu1;
                row1["卡扣大项目"] = KakouYeji_Daxiangmu1;

                row1["卡扣合计"] = KakouYeji_Heji1;
                row1["实操业绩"] = ShiCao_zhiding1;
                row1["服务项目数"] = KeShu_zhiding1;
                row1["手工"] = Ticheng1;
                row1["奖励"] = OtherTiCheng1;
                tableExport.Rows.Add(row1);
            }

            //}

            DataRow row2 = tableExport.NewRow();
            row2["门店"] = null;
            row2["姓名"] = null;
            row2["职位"] = null;
            row2["组别"] = "总计";
            row2["现金储值卡"] = Yeji_Chuzhika;
            row2["现金疗程卡"] = Yeji_Liaochengka;
            row2["现金常规产品"] = Yeji_Chanpin;
            row2["现金特殊产品"] = Yeji_TeshuChanpin;
            row2["现金单次项目"] = Yeji_Xiangmu;
            row2["现金特殊项目"] = Yeji_Teshuxiangmu;
            row2["现金大项目"] = Yeji_Daxiangmu;

            row2["业绩合计"] = Yeji_Heji;
            row2["卡扣疗程卡"] = KakouYeji_Liaocheng;
            row2["卡扣常规产品"] = KakouYeji_Chanpin;
            row2["卡扣特殊产品"] = KakouYeji_TeshuChanpin;
            row2["卡扣单次项目"] = KakouYeji_Xiangmu;
            row2["卡扣特殊项目"] = KakouYeji_Teshuxiangmu;
            row2["卡扣大项目"] = KakouYeji_Daxiangmu;

            row2["卡扣合计"] = KakouYeji_Heji;
            row2["实操业绩"] = ShiCao_zhiding;
            row2["服务项目数"] = KeShu_zhiding;
            row2["手工"] = Ticheng;
            row2["奖励"] = OtherTiCheng;
            tableExport.Rows.Add(row2);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, entity.s_StareTime.ToString("yyyy-MM-dd") + "-" + entity.s_Endtime.ToString("yyyy-MM-dd") + "员工业绩汇总");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "yuangongyeji.xls");

            //_SeachEntity entity = new _SeachEntity();
            //entity.s_HospitalID = s_HospitalID;
            //if (s_HospitalID==0)
            //{
            //    entity.s_HospitalID = LoginUserEntity.HospitalID;
            //}

            //entity.s_StareTime = Convert.ToDateTime(s_StareTime);
            //entity.s_Endtime = Convert.ToDateTime(s_Endtime);
            //entity.DepartmentID = DepartmentID;
            //entity.s_FollowUpUserID = s_FollowUpUserID;
            //entity.IsGive = s_isGive;
            //entity.IsZhiding = IsZhiding;
            //int rows;
            //int pagecount;
            //if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;
            //entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            //entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));
            //entity.numPerPage = 10000; //每页显示条数
            //if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            //var list = cashBll.GetPerformanceSummary(entity, out rows, out pagecount);
            //var houspital = personnelBLL.HospitalEntityByID(entity.s_HospitalID);
            //var sum = cashBll.GetSumPerformanceSummary(entity);//求汇总
            //var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            //DataTable tableExport = new DataTable("Export");
            //decimal Yeji_Chuzhika = 0;
            //decimal Yeji_Liaochengka = 0;
            //decimal Yeji_Chanpin = 0;
            //decimal Yeji_TeshuChanpin = 0;
            //decimal Yeji_Xiangmu = 0;
            //decimal Yeji_Daxiangmu = 0;
            //decimal Yeji_Teshuxiangmu = 0;
            //decimal Yeji_Heji = 0;
            //decimal KakouYeji_Liaocheng = 0;
            //decimal KakouYeji_Chanpin = 0;
            //decimal KakouYeji_TeshuChanpin = 0;
            //decimal KakouYeji_Xiangmu = 0;
            //decimal KakouYeji_Daxiangmu = 0;
            //decimal KakouYeji_Teshuxiangmu = 0;
            //decimal KakouYeji_Heji = 0;
            //decimal ShiCao_zhiding = 0;
            //double KeShu_zhiding = 0;
            //decimal Ticheng = 0;
            //decimal OtherTiCheng = 0;
            //tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店"), new DataColumn("姓名"), new DataColumn("职位"), new DataColumn("组别"), new DataColumn("现金储值卡"), new DataColumn("现金疗程卡"), new DataColumn("现金单次项目"), new DataColumn("现金常规产品"), new DataColumn("现金特殊产品"), new DataColumn("现金特殊项目"), new DataColumn("现金大项目"), new DataColumn("业绩合计"), new DataColumn("卡扣疗程卡"), new DataColumn("卡扣单次项目"), new DataColumn("卡扣常规产品"), new DataColumn("卡扣特殊产品"), new DataColumn("卡扣特殊项目"), new DataColumn("卡扣大项目"), new DataColumn("卡扣合计"), new DataColumn("实操业绩"), new DataColumn("服务项目数"), new DataColumn("手工"), new DataColumn("奖励") });
            //var departList1 = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1);
            //if (departList1.Count > 0)
            //{
            //    foreach (var info in departList1)
            //    {
            //        decimal Yeji_Chuzhika1 = 0;
            //        decimal Yeji_Liaochengka1 = 0;
            //        decimal Yeji_Chanpin1 = 0;
            //        decimal Yeji_TeshuChanpin1 = 0;
            //        decimal Yeji_Xiangmu1 = 0;
            //        decimal Yeji_Daxiangmu1 = 0;
            //        decimal Yeji_Teshuxiangmu1 = 0;
            //        decimal Yeji_Heji1 = 0;
            //        decimal KakouYeji_Liaocheng1 = 0;
            //        decimal KakouYeji_Chanpin1 = 0;
            //        decimal KakouYeji_TeshuChanpin1 = 0;
            //        decimal KakouYeji_Xiangmu1 = 0;
            //        decimal KakouYeji_Daxiangmu1 = 0;
            //        decimal KakouYeji_Teshuxiangmu1 = 0;
            //        decimal KakouYeji_Heji1 = 0;
            //        decimal ShiCao_zhiding1 = 0;
            //        double KeShu_zhiding1 = 0;
            //        decimal Ticheng1 = 0;
            //        decimal OtherTiCheng1 = 0;
            //        var newlist = list.Where(c => c.DepartmentID == info.ID);
            //        foreach (JoinUserEntity model in newlist)
            //        {
            //            //所有合计
            //            Yeji_Chuzhika = Yeji_Chuzhika + model.Yeji_Chuzhika;
            //            Yeji_Liaochengka = Yeji_Liaochengka + model.Yeji_Liaochengka;
            //            Yeji_Chanpin = Yeji_Chanpin + model.Yeji_Chanpin;
            //            Yeji_TeshuChanpin = Yeji_TeshuChanpin + model.Yeji_TeshuChanpin;
            //            Yeji_Xiangmu = Yeji_Xiangmu + model.Yeji_Xiangmu;
            //            Yeji_Daxiangmu = Yeji_Daxiangmu + model.Yeji_Daxiangmu;
            //            Yeji_Teshuxiangmu = Yeji_Teshuxiangmu + model.Yeji_Teshuxiangmu;
            //            Yeji_Heji = Yeji_Heji + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
            //                        model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
            //                        model.Yeji_Teshuxiangmu;
            //            KakouYeji_Liaocheng = KakouYeji_Liaocheng + model.KakouYeji_Liaocheng;
            //            KakouYeji_Chanpin = KakouYeji_Chanpin + model.KakouYeji_Chanpin;
            //            KakouYeji_TeshuChanpin = KakouYeji_TeshuChanpin + model.KakouYeji_TeshuChanpin;
            //            KakouYeji_Xiangmu = KakouYeji_Xiangmu + model.KakouYeji_Xiangmu;
            //            KakouYeji_Daxiangmu = KakouYeji_Daxiangmu + model.KakouYeji_Daxiangmu;
            //            KakouYeji_Teshuxiangmu = KakouYeji_Teshuxiangmu + model.KakouYeji_Teshuxiangmu;
            //            KakouYeji_Heji = KakouYeji_Heji + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
            //                             model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
            //                             model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
            //            ShiCao_zhiding = ShiCao_zhiding + model.ShiCao_zhiding;
            //            KeShu_zhiding = KeShu_zhiding + model.KeShu_zhiding;
            //            Ticheng = Ticheng + model.Ticheng;
            //            OtherTiCheng = OtherTiCheng + model.OtherTiCheng;

            //            //组合计
            //            Yeji_Chuzhika1 = Yeji_Chuzhika1 + model.Yeji_Chuzhika;
            //            Yeji_Liaochengka1 = Yeji_Liaochengka1 + model.Yeji_Liaochengka;
            //            Yeji_Chanpin1 = Yeji_Chanpin1 + model.Yeji_Chanpin;
            //            Yeji_TeshuChanpin1 = Yeji_TeshuChanpin1 + model.Yeji_TeshuChanpin;
            //            Yeji_Xiangmu1 = Yeji_Xiangmu1 + model.Yeji_Xiangmu;
            //            Yeji_Daxiangmu1 = Yeji_Daxiangmu1 + model.Yeji_Daxiangmu;
            //            Yeji_Teshuxiangmu1 = Yeji_Teshuxiangmu1 + model.Yeji_Teshuxiangmu;
            //            Yeji_Heji1 = Yeji_Heji1 + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
            //                         model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
            //                         model.Yeji_Teshuxiangmu;
            //            KakouYeji_Liaocheng1 = KakouYeji_Liaocheng1 + model.KakouYeji_Liaocheng;
            //            KakouYeji_Chanpin1 = KakouYeji_Chanpin1 + model.KakouYeji_Chanpin;
            //            KakouYeji_TeshuChanpin1 = KakouYeji_TeshuChanpin1 + model.KakouYeji_TeshuChanpin;
            //            KakouYeji_Xiangmu1 = KakouYeji_Xiangmu1 + model.KakouYeji_Xiangmu;
            //            KakouYeji_Daxiangmu1 = KakouYeji_Daxiangmu1 + model.KakouYeji_Daxiangmu;
            //            KakouYeji_Teshuxiangmu1 = KakouYeji_Teshuxiangmu1 + model.KakouYeji_Teshuxiangmu;
            //            KakouYeji_Heji1 = KakouYeji_Heji1 + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
            //                              model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
            //                              model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
            //            ShiCao_zhiding1 = ShiCao_zhiding1 + model.ShiCao_zhiding;
            //            KeShu_zhiding1 = KeShu_zhiding1 + model.KeShu_zhiding;
            //            Ticheng1 = Ticheng1 + model.Ticheng;
            //            OtherTiCheng1 = OtherTiCheng1 + model.OtherTiCheng;
            //            DataRow row = tableExport.NewRow();
            //            row["门店"] = houspital.Name;
            //            row["姓名"] = model.UserName;
            //            row["职位"] = model.DutyName;
            //            var userentity = personnelBLL.GetUserByUserID(model.UserID);
            //            var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1)
            //                .Where(c => c.ID == userentity.DepartmentID).ToList();
            //            row["组别"] = departList != null && departList.Count() > 0 ? departList[0].Name : "";
            //            row["现金储值卡"] = model.Yeji_Chuzhika;
            //            row["现金疗程卡"] = model.Yeji_Liaochengka;
            //            row["现金常规产品"] = model.Yeji_Chanpin;
            //            row["现金特殊产品"] = model.Yeji_TeshuChanpin;
            //            row["现金单次项目"] = model.Yeji_Xiangmu;
            //            row["现金特殊项目"] = model.Yeji_Teshuxiangmu;
            //            row["现金大项目"] = model.Yeji_Daxiangmu;

            //            row["业绩合计"] = model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
            //                          model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
            //                          model.Yeji_Teshuxiangmu;
            //            row["卡扣疗程卡"] = model.KakouYeji_Liaocheng;
            //            row["卡扣常规产品"] = model.KakouYeji_Chanpin;
            //            row["卡扣特殊产品"] = model.KakouYeji_TeshuChanpin;
            //            row["卡扣单次项目"] = model.KakouYeji_Xiangmu;
            //            row["卡扣特殊项目"] = model.KakouYeji_Teshuxiangmu;
            //            row["卡扣大项目"] = model.KakouYeji_Daxiangmu;

            //            row["卡扣合计"] = model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
            //                          model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
            //                          model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
            //            row["实操业绩"] = model.ShiCao_zhiding;
            //            row["服务项目数"] = model.KeShu_zhiding;

            //            row["手工"] = model.Ticheng;
            //            row["奖励"] = model.OtherTiCheng;
            //            tableExport.Rows.Add(row);
            //        }

            //        DataRow row1 = tableExport.NewRow();
            //        row1["门店"] = null;
            //        row1["姓名"] = null;
            //        row1["职位"] = null;
            //        row1["组别"] = "小组合计";
            //        row1["现金储值卡"] = Yeji_Chuzhika1;
            //        row1["现金疗程卡"] = Yeji_Liaochengka1;
            //        row1["现金常规产品"] = Yeji_Chanpin1;
            //        row1["现金特殊产品"] = Yeji_TeshuChanpin1;
            //        row1["现金单次项目"] = Yeji_Xiangmu1;
            //        row1["现金特殊项目"] = Yeji_Teshuxiangmu1;
            //        row1["现金大项目"] = Yeji_Daxiangmu1;

            //        row1["业绩合计"] = Yeji_Heji1;
            //        row1["卡扣疗程卡"] = KakouYeji_Liaocheng1;
            //        row1["卡扣常规产品"] = KakouYeji_Chanpin1;
            //        row1["卡扣特殊产品"] = KakouYeji_TeshuChanpin1;
            //        row1["卡扣单次项目"] = KakouYeji_Xiangmu1;
            //        row1["卡扣特殊项目"] = KakouYeji_Teshuxiangmu1;
            //        row1["卡扣大项目"] = KakouYeji_Daxiangmu1;

            //        row1["卡扣合计"] = KakouYeji_Heji1;
            //        row1["实操业绩"] = ShiCao_zhiding1;
            //        row1["服务项目数"] = KeShu_zhiding1;
            //        row1["手工"] = Ticheng1;
            //        row1["奖励"] = OtherTiCheng1;
            //        tableExport.Rows.Add(row1);
            //    }
            //}
            //else
            //{
            //    decimal Yeji_Chuzhika1 = 0;
            //    decimal Yeji_Liaochengka1 = 0;
            //    decimal Yeji_Chanpin1 = 0;
            //    decimal Yeji_TeshuChanpin1 = 0;
            //    decimal Yeji_Xiangmu1 = 0;
            //    decimal Yeji_Daxiangmu1 = 0;
            //    decimal Yeji_Teshuxiangmu1 = 0;
            //    decimal Yeji_Heji1 = 0;
            //    decimal KakouYeji_Liaocheng1 = 0;
            //    decimal KakouYeji_Chanpin1 = 0;
            //    decimal KakouYeji_TeshuChanpin1 = 0;
            //    decimal KakouYeji_Xiangmu1 = 0;
            //    decimal KakouYeji_Daxiangmu1 = 0;
            //    decimal KakouYeji_Teshuxiangmu1 = 0;
            //    decimal KakouYeji_Heji1 = 0;
            //    decimal ShiCao_zhiding1 = 0;
            //    double KeShu_zhiding1 = 0;
            //    decimal Ticheng1 = 0;
            //    decimal OtherTiCheng1 = 0;

            //    foreach (JoinUserEntity model in list)
            //    {
            //        //所有合计
            //        Yeji_Chuzhika = Yeji_Chuzhika + model.Yeji_Chuzhika;
            //        Yeji_Liaochengka = Yeji_Liaochengka + model.Yeji_Liaochengka;
            //        Yeji_Chanpin = Yeji_Chanpin + model.Yeji_Chanpin;
            //        Yeji_TeshuChanpin = Yeji_TeshuChanpin + model.Yeji_TeshuChanpin;
            //        Yeji_Xiangmu = Yeji_Xiangmu + model.Yeji_Xiangmu;
            //        Yeji_Daxiangmu = Yeji_Daxiangmu + model.Yeji_Daxiangmu;
            //        Yeji_Teshuxiangmu = Yeji_Teshuxiangmu + model.Yeji_Teshuxiangmu;
            //        Yeji_Heji = Yeji_Heji + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
            //                    model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
            //                    model.Yeji_Teshuxiangmu;
            //        KakouYeji_Liaocheng = KakouYeji_Liaocheng + model.KakouYeji_Liaocheng;
            //        KakouYeji_Chanpin = KakouYeji_Chanpin + model.KakouYeji_Chanpin;
            //        KakouYeji_TeshuChanpin = KakouYeji_TeshuChanpin + model.KakouYeji_TeshuChanpin;
            //        KakouYeji_Xiangmu = KakouYeji_Xiangmu + model.KakouYeji_Xiangmu;
            //        KakouYeji_Daxiangmu = KakouYeji_Daxiangmu + model.KakouYeji_Daxiangmu;
            //        KakouYeji_Teshuxiangmu = KakouYeji_Teshuxiangmu + model.KakouYeji_Teshuxiangmu;
            //        KakouYeji_Heji = KakouYeji_Heji + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
            //                         model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
            //                         model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
            //        ShiCao_zhiding = ShiCao_zhiding + model.ShiCao_zhiding;
            //        KeShu_zhiding = KeShu_zhiding + model.KeShu_zhiding;
            //        Ticheng = Ticheng + model.Ticheng;
            //        OtherTiCheng = OtherTiCheng + model.OtherTiCheng;


            //        //组合计
            //        Yeji_Chuzhika1 = Yeji_Chuzhika1 + model.Yeji_Chuzhika;
            //        Yeji_Liaochengka1 = Yeji_Liaochengka1 + model.Yeji_Liaochengka;
            //        Yeji_Chanpin1 = Yeji_Chanpin1 + model.Yeji_Chanpin;
            //        Yeji_TeshuChanpin1 = Yeji_TeshuChanpin1 + model.Yeji_TeshuChanpin;
            //        Yeji_Xiangmu1 = Yeji_Xiangmu1 + model.Yeji_Xiangmu;
            //        Yeji_Daxiangmu1 = Yeji_Daxiangmu1 + model.Yeji_Daxiangmu;
            //        Yeji_Teshuxiangmu1 = Yeji_Teshuxiangmu1 + model.Yeji_Teshuxiangmu;
            //        Yeji_Heji1 = Yeji_Heji1 + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
            //                     model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
            //                     model.Yeji_Teshuxiangmu;
            //        KakouYeji_Liaocheng1 = KakouYeji_Liaocheng1 + model.KakouYeji_Liaocheng;
            //        KakouYeji_Chanpin1 = KakouYeji_Chanpin1 + model.KakouYeji_Chanpin;
            //        KakouYeji_TeshuChanpin1 = KakouYeji_TeshuChanpin1 + model.KakouYeji_TeshuChanpin;
            //        KakouYeji_Xiangmu1 = KakouYeji_Xiangmu1 + model.KakouYeji_Xiangmu;
            //        KakouYeji_Daxiangmu1 = KakouYeji_Daxiangmu1 + model.KakouYeji_Daxiangmu;
            //        KakouYeji_Teshuxiangmu1 = KakouYeji_Teshuxiangmu1 + model.KakouYeji_Teshuxiangmu;
            //        KakouYeji_Heji1 = KakouYeji_Heji1 + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
            //                          model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
            //                          model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
            //        ShiCao_zhiding1 = ShiCao_zhiding1 + model.ShiCao_zhiding;
            //        KeShu_zhiding1 = KeShu_zhiding1 + model.KeShu_zhiding;
            //        Ticheng1 = Ticheng1 + model.Ticheng;
            //        OtherTiCheng1 = OtherTiCheng1 + model.OtherTiCheng;
            //        DataRow row = tableExport.NewRow();
            //        row["门店"] = houspital.Name;
            //        row["姓名"] = model.UserName;
            //        row["职位"] = model.DutyName;
            //        var userentity = personnelBLL.GetUserByUserID(model.UserID);
            //        var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1)
            //            .Where(c => c.ID == userentity.DepartmentID).ToList();
            //        row["组别"] = departList != null && departList.Count() > 0 ? departList[0].Name : "";
            //        row["现金储值卡"] = model.Yeji_Chuzhika;
            //        row["现金疗程卡"] = model.Yeji_Liaochengka;
            //        row["现金常规产品"] = model.Yeji_Chanpin;
            //        row["现金特殊产品"] = model.Yeji_TeshuChanpin;
            //        row["现金单次项目"] = model.Yeji_Xiangmu;
            //        row["现金特殊项目"] = model.Yeji_Teshuxiangmu;
            //        row["现金大项目"] = model.Yeji_Daxiangmu;

            //        row["业绩合计"] = model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
            //                      model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
            //                      model.Yeji_Teshuxiangmu;
            //        row["卡扣疗程卡"] = model.KakouYeji_Liaocheng;
            //        row["卡扣常规产品"] = model.KakouYeji_Chanpin;
            //        row["卡扣特殊产品"] = model.KakouYeji_TeshuChanpin;
            //        row["卡扣单次项目"] = model.KakouYeji_Xiangmu;
            //        row["卡扣特殊项目"] = model.KakouYeji_Teshuxiangmu;
            //        row["卡扣大项目"] = model.KakouYeji_Daxiangmu;

            //        row["卡扣合计"] = model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
            //                      model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
            //                      model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
            //        row["实操业绩"] = model.ShiCao_zhiding;
            //        row["服务项目数"] = model.KeShu_zhiding;

            //        row["手工"] = model.Ticheng;
            //        row["奖励"] = model.OtherTiCheng;
            //        tableExport.Rows.Add(row);
            //    }

            //    DataRow row1 = tableExport.NewRow();
            //    row1["门店"] = null;
            //    row1["姓名"] = null;
            //    row1["职位"] = null;
            //    row1["组别"] = "小组合计";
            //    row1["现金储值卡"] = Yeji_Chuzhika1;
            //    row1["现金疗程卡"] = Yeji_Liaochengka1;
            //    row1["现金常规产品"] = Yeji_Chanpin1;
            //    row1["现金特殊产品"] = Yeji_TeshuChanpin1;
            //    row1["现金单次项目"] = Yeji_Xiangmu1;
            //    row1["现金特殊项目"] = Yeji_Teshuxiangmu1;
            //    row1["现金大项目"] = Yeji_Daxiangmu1;

            //    row1["业绩合计"] = Yeji_Heji1;
            //    row1["卡扣疗程卡"] = KakouYeji_Liaocheng1;
            //    row1["卡扣常规产品"] = KakouYeji_Chanpin1;
            //    row1["卡扣特殊产品"] = KakouYeji_TeshuChanpin1;
            //    row1["卡扣单次项目"] = KakouYeji_Xiangmu1;
            //    row1["卡扣特殊项目"] = KakouYeji_Teshuxiangmu1;
            //    row1["卡扣大项目"] = KakouYeji_Daxiangmu1;

            //    row1["卡扣合计"] = KakouYeji_Heji1;
            //    row1["实操业绩"] = ShiCao_zhiding1;
            //    row1["服务项目数"] = KeShu_zhiding1;
            //    row1["手工"] = Ticheng1;
            //    row1["奖励"] = OtherTiCheng1;
            //    tableExport.Rows.Add(row1);
            //}

            //DataRow row2 = tableExport.NewRow();
            //row2["门店"] = null;
            //row2["姓名"] = null;
            //row2["职位"] = null;
            //row2["组别"] = "总计";
            //row2["现金储值卡"] = Yeji_Chuzhika;
            //row2["现金疗程卡"] = Yeji_Liaochengka;
            //row2["现金常规产品"] = Yeji_Chanpin;
            //row2["现金特殊产品"] = Yeji_TeshuChanpin;
            //row2["现金单次项目"] = Yeji_Xiangmu;
            //row2["现金特殊项目"] = Yeji_Teshuxiangmu;
            //row2["现金大项目"] = Yeji_Daxiangmu;

            //row2["业绩合计"] = Yeji_Heji;
            //row2["卡扣疗程卡"] = KakouYeji_Liaocheng;
            //row2["卡扣常规产品"] = KakouYeji_Chanpin;
            //row2["卡扣特殊产品"] = KakouYeji_TeshuChanpin;
            //row2["卡扣单次项目"] = KakouYeji_Xiangmu;
            //row2["卡扣特殊项目"] = KakouYeji_Teshuxiangmu;
            //row2["卡扣大项目"] = KakouYeji_Daxiangmu;

            //row2["卡扣合计"] = KakouYeji_Heji;
            //row2["实操业绩"] = ShiCao_zhiding;
            //row2["服务项目数"] = KeShu_zhiding;
            //row2["手工"] = Ticheng;
            //row2["奖励"] = OtherTiCheng;
            //tableExport.Rows.Add(row2);
            //ExcelOperate excel = new ExcelOperate();
            //excel.InitializeWorkbook();
            //excel.GenerateData(tableExport, entity.s_StareTime.ToString("yyyy-MM-dd") + "-" + entity.s_Endtime.ToString("yyyy-MM-dd") + "员工业绩汇总");
            //return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "yuangongyeji.xls");
        }

        /// <summary>
        /// 业绩汇总导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportPerformanceSummaryNew(int s_HospitalID, string HospitalIDName, int Years, int Months, int IsZhiding, string s_StareTime, string s_Endtime, int DepartmentID, int s_FollowUpUserID, int s_isGive)
        {
            _SeachEntity entity = new _SeachEntity();
            string[] dataid = { "99999999" };
            if (!string.IsNullOrEmpty(HospitalIDName))
            {
                HospitalIDName = HospitalIDName.Substring(0, HospitalIDName.Length - 1);
                dataid = HospitalIDName.Split(',');
            }
            entity.s_HospitalID = s_HospitalID;
            entity.s_StareTime = Convert.ToDateTime(s_StareTime);
            entity.s_Endtime = Convert.ToDateTime(s_Endtime);
            entity.DepartmentID = DepartmentID;
            entity.s_FollowUpUserID = s_FollowUpUserID;
            entity.IsGive = s_isGive;
            entity.IsZhiding = IsZhiding;
            int rows;
            int pagecount;
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;
            var days = DateTime.DaysInMonth(Convert.ToInt32(Years), Convert.ToInt32(Months));
            entity.s_StareTime = Convert.ToDateTime(("" + Convert.ToInt32(Years) + "-" + Convert.ToInt32(Months) + "-01 00:00:01").ToString());
            entity.s_Endtime = Convert.ToDateTime(("" + Convert.ToInt32(Years) + "-" + Convert.ToInt32(Months) + "-" + days + " 23:59:59").ToString());
            entity.numPerPage = 10000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var sum = cashBll.GetSumPerformanceSummary(entity);//求汇总
            //var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            DataTable tableExport = new DataTable("Export");
            decimal _Yeji_Chuzhika = 0;
            decimal _Yeji_Liaochengka = 0;
            decimal _Yeji_Chanpin = 0;
            decimal _Yeji_TeshuChanpin = 0;
            decimal _Yeji_Xiangmu = 0;
            decimal _Yeji_Daxiangmu = 0;
            decimal _Yeji_Teshuxiangmu = 0;
            decimal _Yeji_Heji = 0;
            decimal _KakouYeji_Liaocheng = 0;
            decimal _KakouYeji_Chanpin = 0;
            decimal _KakouYeji_TeshuChanpin = 0;
            decimal _KakouYeji_Xiangmu = 0;
            decimal _KakouYeji_Daxiangmu = 0;
            decimal _KakouYeji_Teshuxiangmu = 0;
            decimal _KakouYeji_Heji = 0;
            decimal _ShiCao_zhiding = 0;
            double _KeShu_zhiding = 0;
            decimal _Ticheng = 0;
            decimal _OtherTiCheng = 0;
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店"), new DataColumn("姓名"), new DataColumn("职位"), new DataColumn("组别"), new DataColumn("现金储值卡"), new DataColumn("现金疗程卡"), new DataColumn("现金单次项目"), new DataColumn("现金常规产品"), new DataColumn("现金特殊产品"), new DataColumn("现金特殊项目"), new DataColumn("现金大项目"), new DataColumn("业绩合计"), new DataColumn("卡扣疗程卡"), new DataColumn("卡扣单次项目"), new DataColumn("卡扣常规产品"), new DataColumn("卡扣特殊产品"), new DataColumn("卡扣特殊项目"), new DataColumn("卡扣大项目"), new DataColumn("卡扣合计"), new DataColumn("实操业绩"), new DataColumn("服务项目数"), new DataColumn("手工"), new DataColumn("奖励") });
            for (var i = 0; i < dataid.Length; i++)
            {
                decimal Yeji_Chuzhika = 0;
                decimal Yeji_Liaochengka = 0;
                decimal Yeji_Chanpin = 0;
                decimal Yeji_TeshuChanpin = 0;
                decimal Yeji_Xiangmu = 0;
                decimal Yeji_Daxiangmu = 0;
                decimal Yeji_Teshuxiangmu = 0;
                decimal Yeji_Heji = 0;
                decimal KakouYeji_Liaocheng = 0;
                decimal KakouYeji_Chanpin = 0;
                decimal KakouYeji_TeshuChanpin = 0;
                decimal KakouYeji_Xiangmu = 0;
                decimal KakouYeji_Daxiangmu = 0;
                decimal KakouYeji_Teshuxiangmu = 0;
                decimal KakouYeji_Heji = 0;
                decimal ShiCao_zhiding = 0;
                double KeShu_zhiding = 0;
                decimal Ticheng = 0;
                decimal OtherTiCheng = 0;
                entity.s_HospitalID = Convert.ToInt32(dataid[i]);
                //var list = cashBll.GetPerformanceSummary(entity, out rows, out pagecount);
                var houspital = personnelBLL.HospitalEntityByID(Convert.ToInt32(dataid[i]));
                var list = cashBll.GetPerformanceSummary(entity, out rows, out pagecount);
                var IsGive = 0;
                //if (entity.IsGive == 1)
                //{
                //    IsGive = 1;
                //    //  list = list.Where(c => c.Hid == entity.HospitalID).ToList();HospitalUser13DataEntity
                //    //list = list.Where(c => c.Hid == entity.s_HospitalID).ToList();
                //    list = list.Where(t => t.HospitalID != entity.s_HospitalID).ToList();
                //}
                //else
                //{
                //    list = list.Where(t => t.HospitalID == entity.s_HospitalID).ToList();
                //}
                list = list.Where(t => t.HospitalID == entity.s_HospitalID).ToList();
                list = list.OrderBy(x => x.DepartmentID).ToList();

                var arrdepartment = list.GroupBy(x => x.DepartmentID).ToList();
                //var departList1 = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1).Where(x => list.Any(u => x.ID == u.DepartmentID));
                //var departList1 = DepartmentManageBLL.GetDepartmentListByHospitalID(Convert.ToInt32(dataid[i]), 1).Where(x => list.Any(u => x.ID == u.DepartmentID));
                //if (departList1.Count > 0)
                //{
                foreach (var info in arrdepartment)
                {
                    decimal Yeji_Chuzhika1 = 0;
                    decimal Yeji_Liaochengka1 = 0;
                    decimal Yeji_Chanpin1 = 0;
                    decimal Yeji_TeshuChanpin1 = 0;
                    decimal Yeji_Xiangmu1 = 0;
                    decimal Yeji_Daxiangmu1 = 0;
                    decimal Yeji_Teshuxiangmu1 = 0;
                    decimal Yeji_Heji1 = 0;
                    decimal KakouYeji_Liaocheng1 = 0;
                    decimal KakouYeji_Chanpin1 = 0;
                    decimal KakouYeji_TeshuChanpin1 = 0;
                    decimal KakouYeji_Xiangmu1 = 0;
                    decimal KakouYeji_Daxiangmu1 = 0;
                    decimal KakouYeji_Teshuxiangmu1 = 0;
                    decimal KakouYeji_Heji1 = 0;
                    decimal ShiCao_zhiding1 = 0;
                    double KeShu_zhiding1 = 0;
                    decimal Ticheng1 = 0;
                    decimal OtherTiCheng1 = 0;
                    var departList1 = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1).Where(c => c.ID == Convert.ToInt32(info.Key)).ToList();
                    var dname = departList1 != null && departList1.Count() > 0 ? departList1[0].Name : "";
                    //var newlist = list.Where(c => c.DepartmentID == info.ID);
                    
                    foreach (JoinUserEntity model in list)
                    {
                        if (model.DepartmentID == Convert.ToInt32(info.Key))
                        {
                            //所有合计
                            Yeji_Chuzhika = Yeji_Chuzhika + model.Yeji_Chuzhika;
                            Yeji_Liaochengka = Yeji_Liaochengka + model.Yeji_Liaochengka;
                            Yeji_Chanpin = Yeji_Chanpin + model.Yeji_Chanpin;
                            Yeji_TeshuChanpin = Yeji_TeshuChanpin + model.Yeji_TeshuChanpin;
                            Yeji_Xiangmu = Yeji_Xiangmu + model.Yeji_Xiangmu;
                            Yeji_Daxiangmu = Yeji_Daxiangmu + model.Yeji_Daxiangmu;
                            Yeji_Teshuxiangmu = Yeji_Teshuxiangmu + model.Yeji_Teshuxiangmu;
                            Yeji_Heji = Yeji_Heji + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                        model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                        model.Yeji_Teshuxiangmu;
                            KakouYeji_Liaocheng = KakouYeji_Liaocheng + model.KakouYeji_Liaocheng;
                            KakouYeji_Chanpin = KakouYeji_Chanpin + model.KakouYeji_Chanpin;
                            KakouYeji_TeshuChanpin = KakouYeji_TeshuChanpin + model.KakouYeji_TeshuChanpin;
                            KakouYeji_Xiangmu = KakouYeji_Xiangmu + model.KakouYeji_Xiangmu;
                            KakouYeji_Daxiangmu = KakouYeji_Daxiangmu + model.KakouYeji_Daxiangmu;
                            KakouYeji_Teshuxiangmu = KakouYeji_Teshuxiangmu + model.KakouYeji_Teshuxiangmu;
                            KakouYeji_Heji = KakouYeji_Heji + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                             model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                             model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                            ShiCao_zhiding = ShiCao_zhiding + model.ShiCao_zhiding;
                            KeShu_zhiding = KeShu_zhiding + model.KeShu_zhiding;
                            Ticheng = Ticheng + model.Ticheng;
                            OtherTiCheng = OtherTiCheng + model.OtherTiCheng;


                            //组合计
                            Yeji_Chuzhika1 = Yeji_Chuzhika1 + model.Yeji_Chuzhika;
                            Yeji_Liaochengka1 = Yeji_Liaochengka1 + model.Yeji_Liaochengka;
                            Yeji_Chanpin1 = Yeji_Chanpin1 + model.Yeji_Chanpin;
                            Yeji_TeshuChanpin1 = Yeji_TeshuChanpin1 + model.Yeji_TeshuChanpin;
                            Yeji_Xiangmu1 = Yeji_Xiangmu1 + model.Yeji_Xiangmu;
                            Yeji_Daxiangmu1 = Yeji_Daxiangmu1 + model.Yeji_Daxiangmu;
                            Yeji_Teshuxiangmu1 = Yeji_Teshuxiangmu1 + model.Yeji_Teshuxiangmu;
                            Yeji_Heji1 = Yeji_Heji1 + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                         model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                         model.Yeji_Teshuxiangmu;
                            KakouYeji_Liaocheng1 = KakouYeji_Liaocheng1 + model.KakouYeji_Liaocheng;
                            KakouYeji_Chanpin1 = KakouYeji_Chanpin1 + model.KakouYeji_Chanpin;
                            KakouYeji_TeshuChanpin1 = KakouYeji_TeshuChanpin1 + model.KakouYeji_TeshuChanpin;
                            KakouYeji_Xiangmu1 = KakouYeji_Xiangmu1 + model.KakouYeji_Xiangmu;
                            KakouYeji_Daxiangmu1 = KakouYeji_Daxiangmu1 + model.KakouYeji_Daxiangmu;
                            KakouYeji_Teshuxiangmu1 = KakouYeji_Teshuxiangmu1 + model.KakouYeji_Teshuxiangmu;
                            KakouYeji_Heji1 = KakouYeji_Heji1 + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                              model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                              model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                            ShiCao_zhiding1 = ShiCao_zhiding1 + model.ShiCao_zhiding;
                            KeShu_zhiding1 = KeShu_zhiding1 + model.KeShu_zhiding;
                            Ticheng1 = Ticheng1 + model.Ticheng;
                            OtherTiCheng1 = OtherTiCheng1 + model.OtherTiCheng;
                            DataRow row = tableExport.NewRow();

                            row["门店"] = dname;
                            row["姓名"] = model.UserName;
                            row["职位"] = model.DutyName;
                            var userentity = personnelBLL.GetUserByUserID(model.UserID);
                            var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1)
                                .Where(c => c.ID == userentity.DepartmentID).ToList();
                            row["组别"] = departList != null && departList.Count() > 0 ? departList[0].Name : "";
                            row["现金储值卡"] = model.Yeji_Chuzhika;
                            row["现金疗程卡"] = model.Yeji_Liaochengka;
                            row["现金常规产品"] = model.Yeji_Chanpin;
                            row["现金特殊产品"] = model.Yeji_TeshuChanpin;
                            row["现金单次项目"] = model.Yeji_Xiangmu;
                            row["现金特殊项目"] = model.Yeji_Teshuxiangmu;
                            row["现金大项目"] = model.Yeji_Daxiangmu;
                            row["业绩合计"] = model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                          model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                          model.Yeji_Teshuxiangmu;
                            row["卡扣疗程卡"] = model.KakouYeji_Liaocheng;
                            row["卡扣常规产品"] = model.KakouYeji_Chanpin;
                            row["卡扣特殊产品"] = model.KakouYeji_TeshuChanpin;
                            row["卡扣单次项目"] = model.KakouYeji_Xiangmu;
                            row["卡扣特殊项目"] = model.KakouYeji_Teshuxiangmu;
                            row["卡扣大项目"] = model.KakouYeji_Daxiangmu;

                            row["卡扣合计"] = model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                          model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                          model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                            row["实操业绩"] = model.ShiCao_zhiding;
                            row["服务项目数"] = model.KeShu_zhiding;

                            row["手工"] = model.Ticheng;
                            row["奖励"] = model.OtherTiCheng;
                            tableExport.Rows.Add(row);
                        }
                    }

                    DataRow row1 = tableExport.NewRow();
                    row1["门店"] = null;
                    row1["姓名"] = null;
                    row1["职位"] = null;
                    row1["组别"] = "小组合计";
                    row1["现金储值卡"] = Yeji_Chuzhika1;
                    row1["现金疗程卡"] = Yeji_Liaochengka1;
                    row1["现金常规产品"] = Yeji_Chanpin1;
                    row1["现金特殊产品"] = Yeji_TeshuChanpin1;
                    row1["现金单次项目"] = Yeji_Xiangmu1;
                    row1["现金特殊项目"] = Yeji_Teshuxiangmu1;
                    row1["现金大项目"] = Yeji_Daxiangmu1;

                    row1["业绩合计"] = Yeji_Heji1;
                    row1["卡扣疗程卡"] = KakouYeji_Liaocheng1;
                    row1["卡扣常规产品"] = KakouYeji_Chanpin1;
                    row1["卡扣特殊产品"] = KakouYeji_TeshuChanpin1;
                    row1["卡扣单次项目"] = KakouYeji_Xiangmu1;
                    row1["卡扣特殊项目"] = KakouYeji_Teshuxiangmu1;
                    row1["卡扣大项目"] = KakouYeji_Daxiangmu1;

                    row1["卡扣合计"] = KakouYeji_Heji1;
                    row1["实操业绩"] = ShiCao_zhiding1;
                    row1["服务项目数"] = KeShu_zhiding1;
                    row1["手工"] = Ticheng1;
                    row1["奖励"] = OtherTiCheng1;
                    tableExport.Rows.Add(row1);
                }

                DataRow row2 = tableExport.NewRow();
                row2["门店"] = houspital.Name;
                row2["姓名"] = null;
                row2["职位"] = null;
                row2["组别"] = "合计";
                row2["现金储值卡"] = Yeji_Chuzhika;
                row2["现金疗程卡"] = Yeji_Liaochengka;
                row2["现金常规产品"] = Yeji_Chanpin;
                row2["现金特殊产品"] = Yeji_TeshuChanpin;
                row2["现金单次项目"] = Yeji_Xiangmu;
                row2["现金特殊项目"] = Yeji_Teshuxiangmu;
                row2["现金大项目"] = Yeji_Daxiangmu;

                row2["业绩合计"] = Yeji_Heji;
                row2["卡扣疗程卡"] = KakouYeji_Liaocheng;
                row2["卡扣常规产品"] = KakouYeji_Chanpin;
                row2["卡扣特殊产品"] = KakouYeji_TeshuChanpin;
                row2["卡扣单次项目"] = KakouYeji_Xiangmu;
                row2["卡扣特殊项目"] = KakouYeji_Teshuxiangmu;
                row2["卡扣大项目"] = KakouYeji_Daxiangmu;

                row2["卡扣合计"] = KakouYeji_Heji;
                row2["实操业绩"] = ShiCao_zhiding;
                row2["服务项目数"] = KeShu_zhiding;
                row2["手工"] = Ticheng;
                row2["奖励"] = OtherTiCheng;
                tableExport.Rows.Add(row2);

                _Yeji_Chuzhika += Yeji_Chuzhika;
                _Yeji_Liaochengka += Yeji_Liaochengka;
                _Yeji_Chanpin += Yeji_Chanpin;
                _Yeji_TeshuChanpin += Yeji_TeshuChanpin;
                _Yeji_Xiangmu += Yeji_Xiangmu;
                _Yeji_Daxiangmu += Yeji_Daxiangmu;
                _Yeji_Teshuxiangmu += Yeji_Teshuxiangmu;
                _Yeji_Heji += Yeji_Heji;
                _KakouYeji_Liaocheng += KakouYeji_Liaocheng;
                _KakouYeji_Chanpin += KakouYeji_Chanpin;
                _KakouYeji_TeshuChanpin += KakouYeji_TeshuChanpin;
                _KakouYeji_Xiangmu += KakouYeji_Xiangmu;
                _KakouYeji_Daxiangmu += KakouYeji_Daxiangmu;
                _KakouYeji_Teshuxiangmu += KakouYeji_Teshuxiangmu;
                _KakouYeji_Heji += KakouYeji_Heji;
                _ShiCao_zhiding += ShiCao_zhiding;
                _KeShu_zhiding += KeShu_zhiding;
                _Ticheng += Ticheng;
                _OtherTiCheng += OtherTiCheng;
            }
            DataRow row3 = tableExport.NewRow();
            row3["门店"] = null;
            row3["姓名"] = null;
            row3["职位"] = null;
            row3["组别"] = "总合计";
            row3["现金储值卡"] = _Yeji_Chuzhika;
            row3["现金疗程卡"] = _Yeji_Liaochengka;
            row3["现金常规产品"] = _Yeji_Chanpin;
            row3["现金特殊产品"] = _Yeji_TeshuChanpin;
            row3["现金单次项目"] = _Yeji_Xiangmu;
            row3["现金特殊项目"] = _Yeji_Teshuxiangmu;
            row3["现金大项目"] = _Yeji_Daxiangmu;

            row3["业绩合计"] = _Yeji_Heji;
            row3["卡扣疗程卡"] = _KakouYeji_Liaocheng;
            row3["卡扣常规产品"] = _KakouYeji_Chanpin;
            row3["卡扣特殊产品"] = _KakouYeji_TeshuChanpin;
            row3["卡扣单次项目"] = _KakouYeji_Xiangmu;
            row3["卡扣特殊项目"] = _KakouYeji_Teshuxiangmu;
            row3["卡扣大项目"] = _KakouYeji_Daxiangmu;

            row3["卡扣合计"] = _KakouYeji_Heji;
            row3["实操业绩"] = _ShiCao_zhiding;
            row3["服务项目数"] = _KeShu_zhiding;
            row3["手工"] = _Ticheng;
            row3["奖励"] = _OtherTiCheng;
            tableExport.Rows.Add(row3);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, entity.s_StareTime.ToString("yyyy-MM-dd") + "-" + entity.s_Endtime.ToString("yyyy-MM-dd") + "员工业绩汇总");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "yuangongyeji.xls");

        }

        /// <summary>
        /// 业绩汇总导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportPerformanceSummaryNew1(int s_HospitalID,string HospitalIDName, int Years,int Months,int IsZhiding, string s_StareTime, string s_Endtime, int DepartmentID, int s_FollowUpUserID, int s_isGive)
        {
            _SeachEntity entity = new _SeachEntity();
            string[] dataid = { "99999999" };
            if(!string.IsNullOrEmpty(HospitalIDName))
            {
                HospitalIDName = HospitalIDName.Substring(0, HospitalIDName.Length - 1);
                dataid = HospitalIDName.Split(',');
            }
            entity.s_HospitalID = s_HospitalID;
            entity.s_StareTime = Convert.ToDateTime(s_StareTime);
            entity.s_Endtime = Convert.ToDateTime(s_Endtime);
            entity.DepartmentID = DepartmentID;
            entity.s_FollowUpUserID = s_FollowUpUserID;
            entity.IsGive = s_isGive;
            entity.IsZhiding = IsZhiding;
            int rows;
            int pagecount;
            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;
            var days = DateTime.DaysInMonth(Convert.ToInt32(Years), Convert.ToInt32(Months));
            entity.s_StareTime = Convert.ToDateTime(("" + Convert.ToInt32(Years) + "-" + Convert.ToInt32(Months) + "-01 00:00:01").ToString());
            entity.s_Endtime = Convert.ToDateTime(("" + Convert.ToInt32(Years) + "-" + Convert.ToInt32(Months) + "-" + days + " 23:59:59").ToString());
            entity.numPerPage = 10000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var sum = cashBll.GetSumPerformanceSummary(entity);//求汇总
            //var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            DataTable tableExport = new DataTable("Export");
            decimal _Yeji_Chuzhika = 0;
            decimal _Yeji_Liaochengka = 0;
            decimal _Yeji_Chanpin = 0;
            decimal _Yeji_TeshuChanpin = 0;
            decimal _Yeji_Xiangmu = 0;
            decimal _Yeji_Daxiangmu = 0;
            decimal _Yeji_Teshuxiangmu = 0;
            decimal _Yeji_Heji = 0;
            decimal _KakouYeji_Liaocheng = 0;
            decimal _KakouYeji_Chanpin = 0;
            decimal _KakouYeji_TeshuChanpin = 0;
            decimal _KakouYeji_Xiangmu = 0;
            decimal _KakouYeji_Daxiangmu = 0;
            decimal _KakouYeji_Teshuxiangmu = 0;
            decimal _KakouYeji_Heji = 0;
            decimal _ShiCao_zhiding = 0;
            double _KeShu_zhiding = 0;
            decimal _Ticheng = 0;
            decimal _OtherTiCheng = 0;
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店"), new DataColumn("姓名"), new DataColumn("职位"), new DataColumn("组别"), new DataColumn("现金储值卡"), new DataColumn("现金疗程卡"), new DataColumn("现金单次项目"), new DataColumn("现金常规产品"), new DataColumn("现金特殊产品"), new DataColumn("现金特殊项目"), new DataColumn("现金大项目"), new DataColumn("业绩合计"), new DataColumn("卡扣疗程卡"), new DataColumn("卡扣单次项目"), new DataColumn("卡扣常规产品"), new DataColumn("卡扣特殊产品"), new DataColumn("卡扣特殊项目"), new DataColumn("卡扣大项目"), new DataColumn("卡扣合计"), new DataColumn("实操业绩"), new DataColumn("服务项目数"), new DataColumn("手工"), new DataColumn("奖励") });
            for (var i = 0; i < dataid.Length ; i++)
            {
                decimal Yeji_Chuzhika = 0;
                decimal Yeji_Liaochengka = 0;
                decimal Yeji_Chanpin = 0;
                decimal Yeji_TeshuChanpin = 0;
                decimal Yeji_Xiangmu = 0;
                decimal Yeji_Daxiangmu = 0;
                decimal Yeji_Teshuxiangmu = 0;
                decimal Yeji_Heji = 0;
                decimal KakouYeji_Liaocheng = 0;
                decimal KakouYeji_Chanpin = 0;
                decimal KakouYeji_TeshuChanpin = 0;
                decimal KakouYeji_Xiangmu = 0;
                decimal KakouYeji_Daxiangmu = 0;
                decimal KakouYeji_Teshuxiangmu = 0;
                decimal KakouYeji_Heji = 0;
                decimal ShiCao_zhiding = 0;
                double KeShu_zhiding = 0;
                decimal Ticheng = 0;
                decimal OtherTiCheng = 0;
                entity.s_HospitalID = Convert.ToInt32(dataid[i]);
                var houspital = personnelBLL.HospitalEntityByID(Convert.ToInt32(dataid[i]));
                var list = cashBll.GetPerformanceSummary(entity, out rows, out pagecount);
                var IsGive = 0;
                if (entity.IsGive == 1)
                {
                    IsGive = 1;
                    //  list = list.Where(c => c.Hid == entity.HospitalID).ToList();HospitalUser13DataEntity
                    //list = list.Where(c => c.Hid == entity.s_HospitalID).ToList();
                    list = list.Where(t => t.HospitalID != entity.s_HospitalID).ToList();

                }
                else
                {
                    list = list.Where(t => t.HospitalID == entity.s_HospitalID).ToList();
                }

                //var departList1 = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1).Where(x => list.Any(u => x.ID == u.DepartmentID));
                var departList1 = DepartmentManageBLL.GetDepartmentListByHospitalID(Convert.ToInt32( dataid[i]), 1).Where(x => list.Any(u => x.ID == u.DepartmentID));
                //if (departList1.Count > 0)
                //{
                foreach (var info in departList1)
                {
                    decimal Yeji_Chuzhika1 = 0;
                    decimal Yeji_Liaochengka1 = 0;
                    decimal Yeji_Chanpin1 = 0;
                    decimal Yeji_TeshuChanpin1 = 0;
                    decimal Yeji_Xiangmu1 = 0;
                    decimal Yeji_Daxiangmu1 = 0;
                    decimal Yeji_Teshuxiangmu1 = 0;
                    decimal Yeji_Heji1 = 0;
                    decimal KakouYeji_Liaocheng1 = 0;
                    decimal KakouYeji_Chanpin1 = 0;
                    decimal KakouYeji_TeshuChanpin1 = 0;
                    decimal KakouYeji_Xiangmu1 = 0;
                    decimal KakouYeji_Daxiangmu1 = 0;
                    decimal KakouYeji_Teshuxiangmu1 = 0;
                    decimal KakouYeji_Heji1 = 0;
                    decimal ShiCao_zhiding1 = 0;
                    double KeShu_zhiding1 = 0;
                    decimal Ticheng1 = 0;
                    decimal OtherTiCheng1 = 0;
                    var newlist = list.Where(c => c.DepartmentID == info.ID);
                    foreach (JoinUserEntity model in newlist)
                    {
                        //所有合计
                        Yeji_Chuzhika = Yeji_Chuzhika + model.Yeji_Chuzhika;
                        Yeji_Liaochengka = Yeji_Liaochengka + model.Yeji_Liaochengka;
                        Yeji_Chanpin = Yeji_Chanpin + model.Yeji_Chanpin;
                        Yeji_TeshuChanpin = Yeji_TeshuChanpin + model.Yeji_TeshuChanpin;
                        Yeji_Xiangmu = Yeji_Xiangmu + model.Yeji_Xiangmu;
                        Yeji_Daxiangmu = Yeji_Daxiangmu + model.Yeji_Daxiangmu;
                        Yeji_Teshuxiangmu = Yeji_Teshuxiangmu + model.Yeji_Teshuxiangmu;
                        Yeji_Heji = Yeji_Heji + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                    model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                    model.Yeji_Teshuxiangmu;
                        KakouYeji_Liaocheng = KakouYeji_Liaocheng + model.KakouYeji_Liaocheng;
                        KakouYeji_Chanpin = KakouYeji_Chanpin + model.KakouYeji_Chanpin;
                        KakouYeji_TeshuChanpin = KakouYeji_TeshuChanpin + model.KakouYeji_TeshuChanpin;
                        KakouYeji_Xiangmu = KakouYeji_Xiangmu + model.KakouYeji_Xiangmu;
                        KakouYeji_Daxiangmu = KakouYeji_Daxiangmu + model.KakouYeji_Daxiangmu;
                        KakouYeji_Teshuxiangmu = KakouYeji_Teshuxiangmu + model.KakouYeji_Teshuxiangmu;
                        KakouYeji_Heji = KakouYeji_Heji + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                         model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                         model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                        ShiCao_zhiding = ShiCao_zhiding + model.ShiCao_zhiding;
                        KeShu_zhiding = KeShu_zhiding + model.KeShu_zhiding;
                        Ticheng = Ticheng + model.Ticheng;
                        OtherTiCheng = OtherTiCheng + model.OtherTiCheng;


                        //组合计
                        Yeji_Chuzhika1 = Yeji_Chuzhika1 + model.Yeji_Chuzhika;
                        Yeji_Liaochengka1 = Yeji_Liaochengka1 + model.Yeji_Liaochengka;
                        Yeji_Chanpin1 = Yeji_Chanpin1 + model.Yeji_Chanpin;
                        Yeji_TeshuChanpin1 = Yeji_TeshuChanpin1 + model.Yeji_TeshuChanpin;
                        Yeji_Xiangmu1 = Yeji_Xiangmu1 + model.Yeji_Xiangmu;
                        Yeji_Daxiangmu1 = Yeji_Daxiangmu1 + model.Yeji_Daxiangmu;
                        Yeji_Teshuxiangmu1 = Yeji_Teshuxiangmu1 + model.Yeji_Teshuxiangmu;
                        Yeji_Heji1 = Yeji_Heji1 + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                     model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                     model.Yeji_Teshuxiangmu;
                        KakouYeji_Liaocheng1 = KakouYeji_Liaocheng1 + model.KakouYeji_Liaocheng;
                        KakouYeji_Chanpin1 = KakouYeji_Chanpin1 + model.KakouYeji_Chanpin;
                        KakouYeji_TeshuChanpin1 = KakouYeji_TeshuChanpin1 + model.KakouYeji_TeshuChanpin;
                        KakouYeji_Xiangmu1 = KakouYeji_Xiangmu1 + model.KakouYeji_Xiangmu;
                        KakouYeji_Daxiangmu1 = KakouYeji_Daxiangmu1 + model.KakouYeji_Daxiangmu;
                        KakouYeji_Teshuxiangmu1 = KakouYeji_Teshuxiangmu1 + model.KakouYeji_Teshuxiangmu;
                        KakouYeji_Heji1 = KakouYeji_Heji1 + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                          model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                          model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                        ShiCao_zhiding1 = ShiCao_zhiding1 + model.ShiCao_zhiding;
                        KeShu_zhiding1 = KeShu_zhiding1 + model.KeShu_zhiding;
                        Ticheng1 = Ticheng1 + model.Ticheng;
                        OtherTiCheng1 = OtherTiCheng1 + model.OtherTiCheng;
                        DataRow row = tableExport.NewRow();

                        row["门店"] = houspital.Name;
                        row["姓名"] = model.UserName;
                        row["职位"] = model.DutyName;
                        var userentity = personnelBLL.GetUserByUserID(model.UserID);
                        var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1)
                            .Where(c => c.ID == userentity.DepartmentID).ToList();
                        row["组别"] = departList != null && departList.Count() > 0 ? departList[0].Name : "";
                        row["现金储值卡"] = model.Yeji_Chuzhika;
                        row["现金疗程卡"] = model.Yeji_Liaochengka;
                        row["现金常规产品"] = model.Yeji_Chanpin;
                        row["现金特殊产品"] = model.Yeji_TeshuChanpin;
                        row["现金单次项目"] = model.Yeji_Xiangmu;
                        row["现金特殊项目"] = model.Yeji_Teshuxiangmu;
                        row["现金大项目"] = model.Yeji_Daxiangmu;

                        row["业绩合计"] = model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                                      model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                                      model.Yeji_Teshuxiangmu;
                        row["卡扣疗程卡"] = model.KakouYeji_Liaocheng;
                        row["卡扣常规产品"] = model.KakouYeji_Chanpin;
                        row["卡扣特殊产品"] = model.KakouYeji_TeshuChanpin;
                        row["卡扣单次项目"] = model.KakouYeji_Xiangmu;
                        row["卡扣特殊项目"] = model.KakouYeji_Teshuxiangmu;
                        row["卡扣大项目"] = model.KakouYeji_Daxiangmu;

                        row["卡扣合计"] = model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                                      model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                                      model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                        row["实操业绩"] = model.ShiCao_zhiding;
                        row["服务项目数"] = model.KeShu_zhiding;

                        row["手工"] = model.Ticheng;
                        row["奖励"] = model.OtherTiCheng;
                        tableExport.Rows.Add(row);
                    }

                    DataRow row1 = tableExport.NewRow();
                    row1["门店"] = null;
                    row1["姓名"] = null;
                    row1["职位"] = null;
                    row1["组别"] = "小组合计";
                    row1["现金储值卡"] = Yeji_Chuzhika1;
                    row1["现金疗程卡"] = Yeji_Liaochengka1;
                    row1["现金常规产品"] = Yeji_Chanpin1;
                    row1["现金特殊产品"] = Yeji_TeshuChanpin1;
                    row1["现金单次项目"] = Yeji_Xiangmu1;
                    row1["现金特殊项目"] = Yeji_Teshuxiangmu1;
                    row1["现金大项目"] = Yeji_Daxiangmu1;

                    row1["业绩合计"] = Yeji_Heji1;
                    row1["卡扣疗程卡"] = KakouYeji_Liaocheng1;
                    row1["卡扣常规产品"] = KakouYeji_Chanpin1;
                    row1["卡扣特殊产品"] = KakouYeji_TeshuChanpin1;
                    row1["卡扣单次项目"] = KakouYeji_Xiangmu1;
                    row1["卡扣特殊项目"] = KakouYeji_Teshuxiangmu1;
                    row1["卡扣大项目"] = KakouYeji_Daxiangmu1;

                    row1["卡扣合计"] = KakouYeji_Heji1;
                    row1["实操业绩"] = ShiCao_zhiding1;
                    row1["服务项目数"] = KeShu_zhiding1;
                    row1["手工"] = Ticheng1;
                    row1["奖励"] = OtherTiCheng1;
                    tableExport.Rows.Add(row1);
                }
                //var notDepartList = DepartmentManageBLL.GetDepartmentListByHospitalID(Convert.ToInt32(dataid[i]), 1).Where(x => list.Any(u => x.ID != u.DepartmentID));
                //var notDepartList = list.Where(x => !departList1.Any(d => d.ID == x.DepartmentID));
                //if (notDepartList.Any())
                //{
                //    decimal Yeji_Chuzhika1 = 0;
                //    decimal Yeji_Liaochengka1 = 0;
                //    decimal Yeji_Chanpin1 = 0;
                //    decimal Yeji_TeshuChanpin1 = 0;
                //    decimal Yeji_Xiangmu1 = 0;
                //    decimal Yeji_Daxiangmu1 = 0;
                //    decimal Yeji_Teshuxiangmu1 = 0;
                //    decimal Yeji_Heji1 = 0;
                //    decimal KakouYeji_Liaocheng1 = 0;
                //    decimal KakouYeji_Chanpin1 = 0;
                //    decimal KakouYeji_TeshuChanpin1 = 0;
                //    decimal KakouYeji_Xiangmu1 = 0;
                //    decimal KakouYeji_Daxiangmu1 = 0;
                //    decimal KakouYeji_Teshuxiangmu1 = 0;
                //    decimal KakouYeji_Heji1 = 0;
                //    decimal ShiCao_zhiding1 = 0;
                //    double KeShu_zhiding1 = 0;
                //    decimal Ticheng1 = 0;
                //    decimal OtherTiCheng1 = 0;

                //    foreach (JoinUserEntity model in list)
                //    {
                //        //所有合计
                //        Yeji_Chuzhika = Yeji_Chuzhika + model.Yeji_Chuzhika;
                //        Yeji_Liaochengka = Yeji_Liaochengka + model.Yeji_Liaochengka;
                //        Yeji_Chanpin = Yeji_Chanpin + model.Yeji_Chanpin;
                //        Yeji_TeshuChanpin = Yeji_TeshuChanpin + model.Yeji_TeshuChanpin;
                //        Yeji_Xiangmu = Yeji_Xiangmu + model.Yeji_Xiangmu;
                //        Yeji_Daxiangmu = Yeji_Daxiangmu + model.Yeji_Daxiangmu;
                //        Yeji_Teshuxiangmu = Yeji_Teshuxiangmu + model.Yeji_Teshuxiangmu;
                //        Yeji_Heji = Yeji_Heji + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                //                    model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                //                    model.Yeji_Teshuxiangmu;
                //        KakouYeji_Liaocheng = KakouYeji_Liaocheng + model.KakouYeji_Liaocheng;
                //        KakouYeji_Chanpin = KakouYeji_Chanpin + model.KakouYeji_Chanpin;
                //        KakouYeji_TeshuChanpin = KakouYeji_TeshuChanpin + model.KakouYeji_TeshuChanpin;
                //        KakouYeji_Xiangmu = KakouYeji_Xiangmu + model.KakouYeji_Xiangmu;
                //        KakouYeji_Daxiangmu = KakouYeji_Daxiangmu + model.KakouYeji_Daxiangmu;
                //        KakouYeji_Teshuxiangmu = KakouYeji_Teshuxiangmu + model.KakouYeji_Teshuxiangmu;
                //        KakouYeji_Heji = KakouYeji_Heji + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                //                         model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                //                         model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                //        ShiCao_zhiding = ShiCao_zhiding + model.ShiCao_zhiding;
                //        KeShu_zhiding = KeShu_zhiding + model.KeShu_zhiding;
                //        Ticheng = Ticheng + model.Ticheng;
                //        OtherTiCheng = OtherTiCheng + model.OtherTiCheng;


                //        //组合计
                //        Yeji_Chuzhika1 = Yeji_Chuzhika1 + model.Yeji_Chuzhika;
                //        Yeji_Liaochengka1 = Yeji_Liaochengka1 + model.Yeji_Liaochengka;
                //        Yeji_Chanpin1 = Yeji_Chanpin1 + model.Yeji_Chanpin;
                //        Yeji_TeshuChanpin1 = Yeji_TeshuChanpin1 + model.Yeji_TeshuChanpin;
                //        Yeji_Xiangmu1 = Yeji_Xiangmu1 + model.Yeji_Xiangmu;
                //        Yeji_Daxiangmu1 = Yeji_Daxiangmu1 + model.Yeji_Daxiangmu;
                //        Yeji_Teshuxiangmu1 = Yeji_Teshuxiangmu1 + model.Yeji_Teshuxiangmu;
                //        Yeji_Heji1 = Yeji_Heji1 + model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                //                     model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                //                     model.Yeji_Teshuxiangmu;
                //        KakouYeji_Liaocheng1 = KakouYeji_Liaocheng1 + model.KakouYeji_Liaocheng;
                //        KakouYeji_Chanpin1 = KakouYeji_Chanpin1 + model.KakouYeji_Chanpin;
                //        KakouYeji_TeshuChanpin1 = KakouYeji_TeshuChanpin1 + model.KakouYeji_TeshuChanpin;
                //        KakouYeji_Xiangmu1 = KakouYeji_Xiangmu1 + model.KakouYeji_Xiangmu;
                //        KakouYeji_Daxiangmu1 = KakouYeji_Daxiangmu1 + model.KakouYeji_Daxiangmu;
                //        KakouYeji_Teshuxiangmu1 = KakouYeji_Teshuxiangmu1 + model.KakouYeji_Teshuxiangmu;
                //        KakouYeji_Heji1 = KakouYeji_Heji1 + model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                //                          model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                //                          model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                //        ShiCao_zhiding1 = ShiCao_zhiding1 + model.ShiCao_zhiding;
                //        KeShu_zhiding1 = KeShu_zhiding1 + model.KeShu_zhiding;
                //        Ticheng1 = Ticheng1 + model.Ticheng;
                //        OtherTiCheng1 = OtherTiCheng1 + model.OtherTiCheng;
                //        DataRow row = tableExport.NewRow();
                //        row["门店"] = houspital.Name;
                //        row["姓名"] = model.UserName;
                //        row["职位"] = model.DutyName;
                //        var userentity = personnelBLL.GetUserByUserID(model.UserID);
                //        var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.s_HospitalID, 1)
                //            .Where(c => c.ID == userentity.DepartmentID).ToList();
                //        row["组别"] = departList != null && departList.Count() > 0 ? departList[0].Name : "";
                //        row["现金储值卡"] = model.Yeji_Chuzhika;
                //        row["现金疗程卡"] = model.Yeji_Liaochengka;
                //        row["现金常规产品"] = model.Yeji_Chanpin;
                //        row["现金特殊产品"] = model.Yeji_TeshuChanpin;
                //        row["现金单次项目"] = model.Yeji_Xiangmu;
                //        row["现金特殊项目"] = model.Yeji_Teshuxiangmu;
                //        row["现金大项目"] = model.Yeji_Daxiangmu;

                //        row["业绩合计"] = model.Yeji_Chuzhika + model.Yeji_Liaochengka + model.Yeji_Chanpin +
                //                      model.Yeji_TeshuChanpin + model.Yeji_Xiangmu + model.Yeji_Daxiangmu +
                //                      model.Yeji_Teshuxiangmu;
                //        row["卡扣疗程卡"] = model.KakouYeji_Liaocheng;
                //        row["卡扣常规产品"] = model.KakouYeji_Chanpin;
                //        row["卡扣特殊产品"] = model.KakouYeji_TeshuChanpin;
                //        row["卡扣单次项目"] = model.KakouYeji_Xiangmu;
                //        row["卡扣特殊项目"] = model.KakouYeji_Teshuxiangmu;
                //        row["卡扣大项目"] = model.KakouYeji_Daxiangmu;

                //        row["卡扣合计"] = model.KakouYeji_Liaocheng + model.KakouYeji_Chanpin +
                //                      model.KakouYeji_TeshuChanpin + model.KakouYeji_Xiangmu +
                //                      model.KakouYeji_Daxiangmu + model.KakouYeji_Teshuxiangmu;
                //        row["实操业绩"] = model.ShiCao_zhiding;
                //        row["服务项目数"] = model.KeShu_zhiding;

                //        row["手工"] = model.Ticheng;
                //        row["奖励"] = model.OtherTiCheng;
                //        tableExport.Rows.Add(row);
                //    }

                //    DataRow row1 = tableExport.NewRow();
                //    row1["门店"] = null;
                //    row1["姓名"] = null;
                //    row1["职位"] = null;
                //    row1["组别"] = "小组合计";
                //    row1["现金储值卡"] = Yeji_Chuzhika1;
                //    row1["现金疗程卡"] = Yeji_Liaochengka1;
                //    row1["现金常规产品"] = Yeji_Chanpin1;
                //    row1["现金特殊产品"] = Yeji_TeshuChanpin1;
                //    row1["现金单次项目"] = Yeji_Xiangmu1;
                //    row1["现金特殊项目"] = Yeji_Teshuxiangmu1;
                //    row1["现金大项目"] = Yeji_Daxiangmu1;

                //    row1["业绩合计"] = Yeji_Heji1;
                //    row1["卡扣疗程卡"] = KakouYeji_Liaocheng1;
                //    row1["卡扣常规产品"] = KakouYeji_Chanpin1;
                //    row1["卡扣特殊产品"] = KakouYeji_TeshuChanpin1;
                //    row1["卡扣单次项目"] = KakouYeji_Xiangmu1;
                //    row1["卡扣特殊项目"] = KakouYeji_Teshuxiangmu1;
                //    row1["卡扣大项目"] = KakouYeji_Daxiangmu1;

                //    row1["卡扣合计"] = KakouYeji_Heji1;
                //    row1["实操业绩"] = ShiCao_zhiding1;
                //    row1["服务项目数"] = KeShu_zhiding1;
                //    row1["手工"] = Ticheng1;
                //    row1["奖励"] = OtherTiCheng1;
                //    tableExport.Rows.Add(row1);
                //}

                //}

                DataRow row2 = tableExport.NewRow();
                row2["门店"] = houspital.Name;
                row2["姓名"] = null;
                row2["职位"] = null;
                row2["组别"] = "总计";
                row2["现金储值卡"] = Yeji_Chuzhika;
                row2["现金疗程卡"] = Yeji_Liaochengka;
                row2["现金常规产品"] = Yeji_Chanpin;
                row2["现金特殊产品"] = Yeji_TeshuChanpin;
                row2["现金单次项目"] = Yeji_Xiangmu;
                row2["现金特殊项目"] = Yeji_Teshuxiangmu;
                row2["现金大项目"] = Yeji_Daxiangmu;

                row2["业绩合计"] = Yeji_Heji;
                row2["卡扣疗程卡"] = KakouYeji_Liaocheng;
                row2["卡扣常规产品"] = KakouYeji_Chanpin;
                row2["卡扣特殊产品"] = KakouYeji_TeshuChanpin;
                row2["卡扣单次项目"] = KakouYeji_Xiangmu;
                row2["卡扣特殊项目"] = KakouYeji_Teshuxiangmu;
                row2["卡扣大项目"] = KakouYeji_Daxiangmu;

                row2["卡扣合计"] = KakouYeji_Heji;
                row2["实操业绩"] = ShiCao_zhiding;
                row2["服务项目数"] = KeShu_zhiding;
                row2["手工"] = Ticheng;
                row2["奖励"] = OtherTiCheng;
                tableExport.Rows.Add(row2);

                _Yeji_Chuzhika += Yeji_Chuzhika;
                _Yeji_Liaochengka += Yeji_Liaochengka;
                _Yeji_Chanpin += Yeji_Chanpin;
                _Yeji_TeshuChanpin += Yeji_TeshuChanpin;
                _Yeji_Xiangmu += Yeji_Xiangmu;
                _Yeji_Daxiangmu += Yeji_Daxiangmu;
                _Yeji_Teshuxiangmu += Yeji_Teshuxiangmu;
                _Yeji_Heji += Yeji_Heji;
                _KakouYeji_Liaocheng += KakouYeji_Liaocheng;
                _KakouYeji_Chanpin += KakouYeji_Chanpin;
                _KakouYeji_TeshuChanpin += KakouYeji_TeshuChanpin;
                _KakouYeji_Xiangmu += KakouYeji_Xiangmu;
                _KakouYeji_Daxiangmu += KakouYeji_Daxiangmu;
                _KakouYeji_Teshuxiangmu += KakouYeji_Teshuxiangmu;
                _KakouYeji_Heji += KakouYeji_Heji;
                _ShiCao_zhiding += ShiCao_zhiding;
                _KeShu_zhiding += KeShu_zhiding;
                _Ticheng += Ticheng;
                _OtherTiCheng += OtherTiCheng;
            }
            DataRow row3 = tableExport.NewRow();
            row3["门店"] = null;
            row3["姓名"] = null;
            row3["职位"] = null;
            row3["组别"] = "总计";
            row3["现金储值卡"] = _Yeji_Chuzhika;
            row3["现金疗程卡"] = _Yeji_Liaochengka;
            row3["现金常规产品"] = _Yeji_Chanpin;
            row3["现金特殊产品"] = _Yeji_TeshuChanpin;
            row3["现金单次项目"] = _Yeji_Xiangmu;
            row3["现金特殊项目"] = _Yeji_Teshuxiangmu;
            row3["现金大项目"] = _Yeji_Daxiangmu;

            row3["业绩合计"] = _Yeji_Heji;
            row3["卡扣疗程卡"] = _KakouYeji_Liaocheng;
            row3["卡扣常规产品"] = _KakouYeji_Chanpin;
            row3["卡扣特殊产品"] = _KakouYeji_TeshuChanpin;
            row3["卡扣单次项目"] = _KakouYeji_Xiangmu;
            row3["卡扣特殊项目"] = _KakouYeji_Teshuxiangmu;
            row3["卡扣大项目"] = _KakouYeji_Daxiangmu;

            row3["卡扣合计"] = _KakouYeji_Heji;
            row3["实操业绩"] = _ShiCao_zhiding;
            row3["服务项目数"] = _KeShu_zhiding;
            row3["手工"] = _Ticheng;
            row3["奖励"] = _OtherTiCheng;
            tableExport.Rows.Add(row3);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, entity.s_StareTime.ToString("yyyy-MM-dd") + "-" + entity.s_Endtime.ToString("yyyy-MM-dd") + "员工业绩汇总");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "yuangongyeji.xls");

        }

        /// <summary>
        /// 业绩审核导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportPerformanceAudit(int HospitalID, string StareTime, string Endtime, int ISAudit, int OrderType)
        {

            OrderEntity entity = new OrderEntity();
            entity.StartTime = StareTime;
            entity.EndTime = Endtime;
            entity.HospitalID = HospitalID;
            entity.ISAudit = ISAudit;
            entity.OrderType = OrderType;
            entity.StartTime = (entity.StartTime == null ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01") : Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = (entity.EndTime == null ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59") : Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59"));

            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            entity.Statu = 1;//只看已结算的
            entity.numPerPage = 50000000;//每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; entity.orderDirection = "desc"; }
            decimal QianPrice = 0;
            var list = cashBll.GetAllOrder(entity, out QianPrice);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);

            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("流水号"), new DataColumn("业务类型"), new DataColumn("卡号"), new DataColumn("客户姓名"), new DataColumn("订单总额"), new DataColumn("是否审核") });
            foreach (OrderEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["流水号"] = model.DocumentNumber;
                row["业务类型"] = PageFunction.GetorderType(model.OrderType);
                row["卡号"] = model.MemberNo;
                row["客户姓名"] = model.UserName;
                row["订单总额"] = model.Price;
                row["是否审核"] = entity.ISAudit == 2 ? "已审核" : "待审核";
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "yejishenhe");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "yejishenhe.xls");




        }

        /// <summary>
        /// 业绩明细导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportPerformanceDetails(_SeachEntity entity)
        {

            //entity.s_HospitalID = s_HospitalID;
            //entity.DepartmentID = DepartmentID;
            //entity.s_FollowUpUserID = s_FollowUpUserID;
            //entity.s_Keyword = s_Keyword;
            //entity.s_StareTime = Convert.ToDateTime(s_StareTime);
            //entity.s_Endtime = Convert.ToDateTime(s_Endtime);
            //if (entity.s_Keyword == "姓名、项目名称")
            //    entity.s_Keyword = "";
            //int rows;
            //int pagecount;
            //int AllQuantity;
            //decimal CashAmount;
            //decimal KakouAmount;
            //decimal OtherTiCheng;
            //if (entity.s_HospitalID == 0) entity.s_HospitalID = loginUserEntity.HospitalID;
            //entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            //entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            //entity.numPerPage = 90000000; //每页显示条数
            //if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            //var list = cashBll.GetPerformanceDetailsList(entity, out rows, out pagecount, out  AllQuantity, out  CashAmount, out  KakouAmount, out  OtherTiCheng);


            if (entity.s_Keyword == "姓名、项目名称")
                entity.s_Keyword = "";

            int rows;
            int pagecount;
            int AllQuantity;
            decimal CashAmount;
            decimal KakouAmount;
            decimal OtherTiCheng;
            decimal _Quantity = 0;
            decimal _CashAmount = 0;

            if (entity.s_HospitalID == 0) entity.s_HospitalID = LoginUserEntity.HospitalID;
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            entity.numPerPage = 100000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; entity.orderDirection = "desc"; }
            IList<OrderinfoEntity> list;
            if (String.IsNullOrEmpty(entity.s_Name))
            {
                list = cashBll.GetPerformanceDetailsList(entity, out rows, out pagecount, out AllQuantity, out CashAmount, out KakouAmount, out OtherTiCheng);
            }
            else
            {
                list = cashBll.GetPerformanceDetailsList(entity, out rows, out pagecount, out AllQuantity, out CashAmount, out KakouAmount, out OtherTiCheng).Where(c => c.PatientName.Contains(entity.s_Name)).ToList();
            }



            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("员工编号"), new DataColumn("员工姓名"), new DataColumn("时间"), new DataColumn("业务类别"), new DataColumn("顾客姓名"), new DataColumn("项目名称"), new DataColumn("数量"), new DataColumn("现金"), new DataColumn("卡扣"), new DataColumn("奖励") });
            foreach (var model in list)
            {
                _Quantity += model.Quantity;
                _CashAmount += model.Yeji;
                DataRow row = tableExport.NewRow();
                row["员工编号"] = model.UserID;
                row["员工姓名"] = model.UserName;
                row["时间"] = model.CreateTime;
                row["业务类别"] = model.GetInfoBuyTyoeBy5Lei();
                row["顾客姓名"] = model.PatientName;
                row["项目名称"] = model.ProdcuitName;
                row["数量"] = model.Quantity;
                row["现金"] = model.Yeji;
                row["卡扣"] = model.KakouYeji;
                row["奖励"] = model.OtherTiCheng;
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["员工编号"] = "";
            row1["员工姓名"] = "";
            row1["时间"] = "";
            row1["业务类别"] = "";
            row1["顾客姓名"] = "";
            row1["项目名称"] = "合计";
            row1["数量"] = _Quantity;// AllQuantity;
            row1["现金"] = _CashAmount;// CashAmount;
            row1["卡扣"] = KakouAmount;
            row1["奖励"] = OtherTiCheng;
            tableExport.Rows.Add(row1);

            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "yejimingxi");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "yejimingxi.xls");


        }

        /// <summary>
        /// 实操消耗导出
        /// </summary>
        /// <param name="HospitalID"></param>
        /// <param name="DepartmentID"></param>
        /// <param name="UserID"></param>
        /// <param name="ProjectName"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <returns></returns>
        public ActionResult ExportItemDetailsConsumption(ItemDetailsConsumptionEntity entity)
        {

            if (entity.ProjectName == "品牌、分类、项目名称")
                entity.ProjectName = "";
            decimal huaci;
            decimal kakou;
            decimal shougong;
            decimal jiangli;
            decimal xianjin;
            decimal Quantity;
            int UserCount;

            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;


            entity.numPerPage = 1000000; //每页显示条数
            entity.StartTime = entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = entity.EndTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59"));
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; entity.orderDirection = "desc"; }
            IList<ItemDetailsConsumptionEntity> list = null;
            //顾客姓名查询， 存储过程不好筛选，顾放到程序中处理
            if (string.IsNullOrEmpty(entity.PatientName))
            {

                list = cashBll.GetItemDetailsConsumptionList(entity, out huaci, out kakou, out shougong, out jiangli, out xianjin, out Quantity, out UserCount);
            }
            else
            {
                entity.numPerPage = 100000;
                //Contains 值不能有空 有空就会有问题
                list = cashBll.GetItemDetailsConsumptionList(entity, out huaci, out kakou, out shougong, out jiangli, out xianjin, out Quantity, out UserCount);
                list = list.Where(c => c.PatientName.Contains(entity.PatientName)).ToList();
            }

            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("员工名称"), new DataColumn("品牌"), new DataColumn("项目名称"),
                new DataColumn("类别"),new DataColumn("项目数"),new DataColumn("疗程消耗"), new DataColumn("卡扣消耗"), new DataColumn("现金消耗"),
                new DataColumn("手工费"), new DataColumn("奖励"), new DataColumn("时间"), new DataColumn("单据分店"), new DataColumn("顾客姓名"), new DataColumn("分类")
            });
            decimal newQuantity = 0;
            decimal HuaCi = 0;
            decimal KaKou = 0;
            decimal XianJin = 0;
            decimal ShouGong = 0;
            decimal Reward = 0;
            foreach (var model in list)
            {
                newQuantity += model.ProjectNumber;
                HuaCi += model.HuaCi;
                KaKou += model.KaKou;
                XianJin += model.XianJin;
                ShouGong += model.ShouGong;
                Reward += model.Reward;
                DataRow row = tableExport.NewRow();
                row["员工名称"] = model.UserName;
                row["品牌"] = model.BrandName;
                row["分类"] = model.ProductTypeName;
                row["项目名称"] = model.ProjectName;
                row["类别"] = model.InfoBuyType == 3 ? "划卡" : "单项";
                row["项目数"] = model.ProjectNumber;
                row["疗程消耗"] = model.HuaCi;
                row["卡扣消耗"] = model.KaKou;
                row["现金消耗"] = model.XianJin;
                row["手工费"] = model.ShouGong;
                row["奖励"] = model.Reward;
                row["时间"] = model.CreateTime.ToString("yyyy-MM-dd");
                row["单据分店"] = model.HospitalName;
                row["顾客姓名"] = model.PatientName;
                tableExport.Rows.Add(row);
            }

            DataRow row1 = tableExport.NewRow();
            row1["员工名称"] = "";
            row1["品牌"] = "";
            row1["分类"] = "";
            row1["项目名称"] = "";
            row1["类别"] = "合计:";
            row1["项目数"] = String.IsNullOrEmpty(entity.PatientName) ? Quantity : newQuantity;
            row1["疗程消耗"] = String.IsNullOrEmpty(entity.PatientName) ? huaci : HuaCi;
            row1["卡扣消耗"] = String.IsNullOrEmpty(entity.PatientName) ? kakou : KaKou;
            row1["现金消耗"] = String.IsNullOrEmpty(entity.PatientName) ? xianjin : XianJin;
            row1["手工费"] = String.IsNullOrEmpty(entity.PatientName) ? shougong : ShouGong;
            row1["奖励"] = String.IsNullOrEmpty(entity.PatientName) ? jiangli : Reward;
            row1["时间"] = "";
            row1["单据分店"] = "";
            row1["顾客姓名"] =Request["txtUserCount"];
            tableExport.Rows.Add(row1);

            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "shicaoxiaohao");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "shicaoxiaohao.xls");
        }

        /// <summary>
        /// 员工客流量导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportPassengerTraffic(PassengerTrafficEntity entity)
        {
            if (entity.HospitalID == 0) entity.HospitalID = LoginUserEntity.HospitalID;
            //if (entity.searchStartTime == DateTime.MinValue) entity.searchStartTime = DateTime.Now;
            //if (entity.searchEndTime == DateTime.MinValue) entity.searchEndTime = DateTime.Now;
            entity.searchStartTime = Convert.ToDateTime(Request["StareTime"]);
            entity.searchEndTime = Convert.ToDateTime(Request["Endtime"]);
            //entity.searchStartTime = entity.searchStartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.searchStartTime.ToString("yyyy-MM-dd 00:00:01"));
            //entity.searchEndTime = entity.searchEndTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.searchEndTime.ToString("yyyy-MM-dd 23:59:59"));
            if (string.IsNullOrWhiteSpace(entity.orderField)) entity.orderField = "UserID";

            var list = iCentBll.PageListPassengerTraffic(entity);

            //List<PassengerTrafficEntity> peList = new List<PassengerTrafficEntity>();
            //if (entity.HospitalID == 0) entity.HospitalID = LoginUserEntity.HospitalID;
            //entity.StareTime = entity.StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StareTime.ToString("yyyy-MM-dd 00:00:01"));
            //entity.Endtime = entity.Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.Endtime.ToString("yyyy-MM-dd 23:59:59"));
            //if (entity.orderField + "" == "") { entity.orderField = "UserID"; }
            //ViewBag.orderField = entity.orderField;
            //ViewBag.orderDirection = entity.orderDirection;
            //var houspital = personnelBLL.HospitalEntityByID(entity.HospitalID);
            ////获取员工列表
            //var userlist = personnelBLL.GetAllUserByHospitalID(entity.HospitalID, 0);
            ////if (entity.UserName != null && entity.UserName.Trim() != null)
            ////{
            ////    userlist = userlist.Where(i => i.UserName.Contains(entity.UserName.Trim()) == true).ToList();
            ////}

            //foreach (var info in userlist)
            //{

            //    var user = personnelBLL.GetUserByUserID(info.UserID);

            //    entity.UserID = info.UserID;

            //    var userinfo = iCentBll.GetPassengerTrafficByUserID(entity);

            //    // var projectcount = iCentBll.GetEmployeeProjectNumber(entity);
            //    userinfo.UserID = user.UserID;
            //    userinfo.UserName = user.UserName;
            //    userinfo.HospitalID = user.HospitalID;
            //    userinfo.HospitalName = houspital.Name;
            //    // userinfo.projectNumber = projectcount;
            //    userinfo.DepartmentID = info.DepartmentID;
            //    userinfo.DutyName = info.DutyName;
            //    peList.Add(userinfo);
            //}


            DataTable tableExport = new DataTable("Export");
            int AllCustomerService = 0;
            int CustomerService = 0;
            int BuyCount = 0;
            int HeadCount = 0;
            int fuwuPassengerTraffic = 0;
            int PassengerTraffic = 0;
            decimal Keliu = 0;
            decimal projectNumber = 0;
            int HsopitalHeadCount = 0;
            decimal JianquKeliu = 0;
            decimal yoyouxiaokeliuu = 0;
            int HospitalBuyCount = 0;

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店"), new DataColumn("员工姓名"), new DataColumn("职位"), new DataColumn("组别"), new DataColumn("管理客数"), new DataColumn("新增客数"), new DataColumn("买单人头"), new DataColumn("服务人头数"), new DataColumn("服务客流量"), new DataColumn("审核客流量"), new DataColumn("客流量"), new DataColumn("项目数") });
            //tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店"), new DataColumn("员工姓名"), new DataColumn("职位"), new DataColumn("组别"), new DataColumn("管理客数"), new DataColumn("新增客数"), new DataColumn("买单人头"), new DataColumn("服务人头数"), new DataColumn("店有效人头数"), new DataColumn("服务客流量"), new DataColumn("审核客流量"), new DataColumn("客流量"), new DataColumn("项目数") });
            var departList1 = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.HospitalID, 1).Where(x => list.Any(u => x.ID == u.DepartmentID));
            foreach (var info in departList1)
            {
                int AllCustomerService1 = 0;
                int CustomerService1 = 0;
                int BuyCount1 = 0;
                int HeadCount1 = 0;
                int fuwuPassengerTraffic1 = 0;
                int PassengerTraffic1 = 0;
                decimal Keliu1 = 0;
                decimal projectNumber1 = 0;
                int HsopitalHeadCount1 = 0;
                decimal JianquKeliu1 = 0;
                decimal yoyouxiaokeliuu1 = 0;
                var peList1 = list.Where(c => c.DepartmentID == info.ID);
                foreach (PassengerTrafficEntity model in peList1)
                {
                    AllCustomerService = AllCustomerService + model.AllCustomerService;
                    CustomerService = CustomerService + model.CustomerService;
                    BuyCount = BuyCount + model.BuyCount;
                    HeadCount = HeadCount + model.HeadCount;
                    JianquKeliu = JianquKeliu + model.JianquKeliu;
                    yoyouxiaokeliuu = yoyouxiaokeliuu+model.fuwuliuliang + model.ShenHeKeliu - model.HuiyuanJianquKeliu;
                    fuwuPassengerTraffic = fuwuPassengerTraffic + model.fuwuPassengerTraffic;
                    PassengerTraffic = PassengerTraffic + model.PassengerTraffic;
                    Keliu = Keliu + model.Keliu;
                    HospitalBuyCount =  model.HospitalBuyCount;
                    projectNumber = projectNumber + model.projectNumber;
                    HsopitalHeadCount = HsopitalHeadCount + model.HsopitalHeadCount;


                    AllCustomerService1 = AllCustomerService1 + model.AllCustomerService;
                    CustomerService1 = CustomerService1 + model.CustomerService;
                    BuyCount1 = BuyCount1 + model.BuyCount;
                    HeadCount1 = HeadCount1 + model.HeadCount;
                    HsopitalHeadCount1 = HsopitalHeadCount1 + model.HsopitalHeadCount;
                    fuwuPassengerTraffic1 = fuwuPassengerTraffic1 + model.fuwuPassengerTraffic;
                    PassengerTraffic1 = PassengerTraffic1 + model.PassengerTraffic;
                    JianquKeliu1 = JianquKeliu1 + model.JianquKeliu;
                    yoyouxiaokeliuu1 = yoyouxiaokeliuu1+ model.fuwuliuliang + model.ShenHeKeliu - model.HuiyuanJianquKeliu;
                    Keliu1 = Keliu1 + model.Keliu;
                    projectNumber1 = projectNumber1 + model.projectNumber;
                    DataRow row = tableExport.NewRow();
                    row["门店"] = model.HospitalName;
                    row["员工姓名"] = model.UserName;
                    row["职位"] = model.DutyName;
                    var userentity = personnelBLL.GetUserByUserID(model.UserID);
                    var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.HospitalID, 1).Where(c => c.ID == userentity.DepartmentID).ToList();
                    row["组别"] = departList != null && departList.Count() > 0 ? departList[0].Name : "";
                    row["管理客数"] = model.AllCustomerService;
                    row["新增客数"] = model.CustomerService;
                    row["买单人头"] = model.BuyCount;
                    row["服务人头数"] = model.HeadCount;
                    //row["店有效人头数"] = model.HsopitalHeadCount; //现门店统计分离出去，故隐掉
                    row["服务客流量"] = model.fuwuPassengerTraffic-model.JianquKeliu;


                    row["审核客流量"] = model.Keliu;
                    row["客流量"] = model.PassengerTraffic;
                    row["项目数"] = model.projectNumber;

                    tableExport.Rows.Add(row);
                }

                DataRow row1 = tableExport.NewRow();
                row1["门店"] = null;
                row1["员工姓名"] = null;
                row1["职位"] = null;
                row1["组别"] = "小组合计";
                row1["管理客数"] = AllCustomerService1;
                row1["新增客数"] = CustomerService1;
                row1["买单人头"] = BuyCount1;
                row1["服务人头数"] = HeadCount1;
                //row1["店有效人头数"] = HsopitalHeadCount1;
                row1["服务客流量"] = fuwuPassengerTraffic1- JianquKeliu1;

                row1["审核客流量"] = Keliu1;
                row1["客流量"] = PassengerTraffic1;
                row1["项目数"] = projectNumber1;
                tableExport.Rows.Add(row1);


            }

            var notDepartList = list.Where(x => !departList1.Any(d => d.ID == x.DepartmentID));
            if (notDepartList.Any())
            {
                int AllCustomerService1 = 0;
                int CustomerService1 = 0;
                int BuyCount1 = 0;
                int HeadCount1 = 0;
                int fuwuPassengerTraffic1 = 0;
                int PassengerTraffic1 = 0;
                decimal Keliu1 = 0;
                decimal projectNumber1 = 0;
                int HsopitalHeadCount1 = 0;
                decimal JianquKeliu1 = 0;
                decimal yoyouxiaokeliuu1 = 0;

                foreach (PassengerTrafficEntity model in notDepartList)
                {
                    AllCustomerService = AllCustomerService + model.AllCustomerService;
                    CustomerService = CustomerService + model.CustomerService;
                    BuyCount = BuyCount + model.BuyCount;
                    HeadCount = HeadCount + model.HeadCount;
                    HsopitalHeadCount = HsopitalHeadCount + model.HsopitalHeadCount;
                    HospitalBuyCount = model.HospitalBuyCount;
                    fuwuPassengerTraffic = fuwuPassengerTraffic + model.fuwuPassengerTraffic;
                    yoyouxiaokeliuu = yoyouxiaokeliuu + model.fuwuliuliang + model.ShenHeKeliu - model.HuiyuanJianquKeliu;
                    JianquKeliu = JianquKeliu + model.JianquKeliu;
                    PassengerTraffic = PassengerTraffic + model.PassengerTraffic;
                    Keliu = Keliu + model.Keliu;
                    projectNumber = projectNumber + model.projectNumber;
                    AllCustomerService1 = AllCustomerService1 + model.AllCustomerService;
                    CustomerService1 = CustomerService1 + model.CustomerService;
                    BuyCount1 = BuyCount1 + model.BuyCount;
                    HeadCount1 = HeadCount1 + model.HeadCount;
                    JianquKeliu1 = JianquKeliu1 + model.JianquKeliu;
                    HsopitalHeadCount1 = HsopitalHeadCount1 + model.HsopitalHeadCount;
                    fuwuPassengerTraffic1 = fuwuPassengerTraffic1 + model.fuwuPassengerTraffic;
                    yoyouxiaokeliuu1 = yoyouxiaokeliuu1 + model.fuwuliuliang + model.ShenHeKeliu - model.HuiyuanJianquKeliu;
                    PassengerTraffic1 = PassengerTraffic1 + model.PassengerTraffic;
                    Keliu1 = Keliu1 + model.Keliu;
                    projectNumber1 = projectNumber1 + model.projectNumber;
                    DataRow row = tableExport.NewRow(); 
                    row["门店"] = model.HospitalName;
                    row["员工姓名"] = model.UserName;
                    row["职位"] = model.DutyName;
                    var userentity = personnelBLL.GetUserByUserID(model.UserID);
                    var departList = DepartmentManageBLL.GetDepartmentListByHospitalID(entity.HospitalID, 1).Where(c => c.ID == userentity.DepartmentID).ToList();
                    row["组别"] = departList != null && departList.Count() > 0 ? departList[0].Name : "";
                    row["管理客数"] = model.AllCustomerService;
                    row["新增客数"] = model.CustomerService;
                    row["买单人头"] = model.BuyCount;
                    row["服务人头数"] = model.HeadCount;
                    //row["店有效人头数"] = model.HsopitalHeadCount;
                    row["服务客流量"] = model.fuwuPassengerTraffic-model.JianquKeliu;

                    row["审核客流量"] = model.Keliu;
                    row["客流量"] = model.PassengerTraffic;
                    row["项目数"] = model.projectNumber;

                    tableExport.Rows.Add(row);
                }

                DataRow row1 = tableExport.NewRow();
                row1["门店"] = null;
                row1["员工姓名"] = null;
                row1["职位"] = null;
                row1["组别"] = "小组合计";
                row1["管理客数"] = AllCustomerService1;
                row1["新增客数"] = CustomerService1;
                row1["买单人头"] = BuyCount1;
                row1["服务人头数"] = HeadCount1;
                //row1["店有效人头数"] = HsopitalHeadCount1;
                row1["服务客流量"] = fuwuPassengerTraffic1- JianquKeliu1;

                row1["审核客流量"] = Keliu1;
                row1["客流量"] = PassengerTraffic1;
                row1["项目数"] = projectNumber1;
                tableExport.Rows.Add(row1);
            }

            DataRow row2 = tableExport.NewRow();
            row2["门店"] = null;
            row2["员工姓名"] = null;
            row2["职位"] = null;
            row2["组别"] = "总计";
            row2["管理客数"] = AllCustomerService;
            row2["新增客数"] = CustomerService;
            row2["买单人头"] = BuyCount;
            row2["服务人头数"] = (HeadCount);//点统计分离出去，故不需要显示店人头数
            //row2["服务人头数"] = (HeadCount)+"店人头数："+ HospitalBuyCount;
            //row2["店有效人头数"] = (HsopitalHeadCount)+ "门店有效客流量"+ yoyouxiaokeliuu;
            row2["服务客流量"] = fuwuPassengerTraffic- JianquKeliu;

            row2["审核客流量"] = Keliu;
            row2["客流量"] = PassengerTraffic;
            row2["项目数"] = projectNumber;
            tableExport.Rows.Add(row2);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "keliuliang");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "keliuliang.xls");
        }
        #endregion

        //测试页面
        public ActionResult ceshiYeMians() {
            return View();
        }

    }
}
