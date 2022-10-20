using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SystemManage.Factory.IBLL;
using PersonnelManage.Factory.IBLL;
using System.Collections;
using Newtonsoft.Json;
using EYB.ServiceEntity.PersonnelEntity;
using EYB.ServiceEntity.SystemEntity;
using CashManage.Factory.IBLL;
using EYB.ServiceEntity.MoneyEntity;
using HS.SupportComponents;
using EYB.ServiceEntity.CashEntity;
using CenterManage.Factory.IBLL;
using Common.Helper;
using EYB.ServiceEntity.PatientEntity;
using PatientManage.Factory.IBLL;
using Newtonsoft.Json.Converters;

namespace EYB.Web.Controllers
{
    public class MobilePhoneController : Controller
    {
        #region 依赖注入
        private ISystemManageBLL systemBLL;
        private IPersonnelManageBLL personnelManageBLL;
        private ICashManageBLL cashBll;
        private ICenterManageBLL iCentBLL;//管理中心
        private IPatientManageBLL IpatBLL;
        public MobilePhoneController()
        {
            systemBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();
            personnelManageBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            cashBll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<CashManage.Factory.IBLL.ICashManageBLL>();
            iCentBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ICenterManageBLL>();
            IpatBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPatientManageBLL>();
        }
        public static string ReturnFalse(int code, string msg)
        {

            Hashtable hb = new Hashtable();
            hb.Add("responseCode", code + "");
            hb.Add("responseComment", msg + "");
            return JsonConvert.SerializeObject(hb);
        }
        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public string CallFunction(string method)
        {
            switch (method)
            {
                case "Login"://登录
                    return this.Login();
                case "HospitalDataIndex"://首页数据
                    return this.HospitalDataIndex();
                case "GetHospitalUser13Data"://美容师13大数据
                    return this.GetHospitalUser13Data();
                case "GetHospital13Data"://门店13大数据
                    return this.GetHospital13Data();
                case "GetGukuqundongchalist"://顾客群洞察
                    return this.GetGukuqundongchalist();
                case "GetPatientProjectlist"://卡内剩余项目
                    return this.GetPatientProjectlist();

                case "GetCompanyNewslist"://门店动态
                    return this.GetCompanyNewslist();
                case "GetHospitalList"://获取门店
                    return this.GetHospitalList();

                default:
                    return "";

            }
        }
        #endregion 依赖注入
        /// <summary>
        /// 登录接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string Login()
        {
            Hashtable hb = new Hashtable();
            try
            {
                var account = Request["account"];
                var pwd = Request["pwd"];
                HospitalUserEntity entity = personnelManageBLL.UserLogin(account, pwd);
                if (entity.HospitalID == 0)
                {
                    //账户异常
                    hb.Add("responseCode", "-8");
                    hb.Add("responseComment", "账户异常，请联系客服！");
                    return JsonConvert.SerializeObject(hb);
                }
                if (entity.OpenActive == 0)
                {
                    hb.Add("responseCode", "-9");
                    hb.Add("responseComment", "账户到期，请续费");
                    return JsonConvert.SerializeObject(hb);
                }
                //修改最后登录时间
                personnelManageBLL.UpdateLoginTime(entity.UserID);
                //日志记录用
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
                    var resultentity = new { UserID = (entity.ContactUserID != 0 ? entity.ContactUserID : entity.UserID), Name = entity.Name, UserName = entity.UserName, LoginAccount = entity.LoginAccount, IsAdmin = entity.IsAdmin, Password = entity.Password, HospitalID = entity.HospitalID, ParentHospitalID = entity.ParentHospitalID };
                    hb.Add("responseCode", "1");
                    hb.Add("responseComment", "登录成功");
                    hb.Add("Model", resultentity);
                    return JsonConvert.SerializeObject(hb);
                }
            }
            catch (Exception e)
            {

                if (e.Message == "账户过期")
                {
                    hb.Add("responseCode", "-9");
                    hb.Add("responseComment", "账户到期，请续费");
                    return JsonConvert.SerializeObject(hb);
                }
                hb.Add("responseCode", "-1");
                hb.Add("responseComment", "登录失败");
                return JsonConvert.SerializeObject(hb);
            }

            hb.Add("responseCode", "-1");
            hb.Add("responseComment", "登录失败");
            return JsonConvert.SerializeObject(hb);
        }

        /// <summary>
        /// 首页接口数据获取
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string HospitalDataIndex()
        {
            Hashtable hb = new Hashtable();
            var HospitalID = ConvertValueHelper.ConvertIntValue(Request["HospitalID"]);
            //1、获取美容师13大数据
            var HospitalUser13Data = cashBll.GetHospitalUser13Data(new TimeConsumingPerformanceEntity { HospitalID = HospitalID, StartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")), EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) });
            //2、店内动态
            int rows, pagecount = 0;
            CompanyNewsEntity entity = new CompanyNewsEntity();
            entity.HospitalID = HospitalID;
            entity.numPerPage = 5;
            entity.orderField = "ID";
            entity.orderDirection = "desc";
            entity.pageNum = 1;
            entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            var CompanyNewslist = iCentBLL.GetCompanyNewsEntityList(entity, out rows, out pagecount);
            var CompanyNewslist1 = iCentBLL.GetCompanyNewsEntityList(entity, out rows, out pagecount);
            foreach (var info in CompanyNewslist)
            {
                info.ProjectName = CompanyNewsHelp.GetTypeName(info.Type);
                info.HuiFangUserName = CompanyNewsHelp.getDateStr(info.CreateTime);
            }
            foreach (var info in CompanyNewslist1)
            {
                if (info.Type == 1 || info.Type == 2)
                {
                    info.Name = "顾客" + info.UserName + " " + CompanyNewsHelp.GetTypeNameFor(info.Type) + info.ProjectName;
                    info.HuiFangUserName = CompanyNewsHelp.getDateStr(info.CreateTime);
                }
            }
            //3、门店13大数据
            var Hospital13Data = cashBll.GetHospital13DataList(new Hospital13DataEntity { HospitalID = HospitalID, Years = DateTime.Now.Year, Months = DateTime.Now.Month });
            //4、顾客群洞察
            PatientEntity pentity = new PatientEntity();
            pentity.StartTime = pentity.StartTime == null ? DateTime.Now.AddMonths(-3).ToString() : pentity.StartTime;
            pentity.EndTime = pentity.EndTime == null ? DateTime.Now.ToString() : pentity.EndTime;

            if (pentity.IsActive == 0)
            {
                pentity.IsActive = 1;
            }

            if (pentity.IsSelectTime == 0)
            {
                pentity.StartTime = null;
                pentity.EndTime = null;
            }
            //三个月内到店一次
            pentity.s_StareTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01"));
            pentity.s_Endtime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            pentity.numPerPage = 5; //每页显示条数
            if (pentity.orderField + "" == "") { pentity.orderField = "CreateTime"; }
            IList<PatientEntity> list = IpatBLL.GetPatientList(pentity, out rows, out pagecount);


            //获取美容师13大数据
            hb.Add("HospitalUser13Data", new { TiyanKeliu = HospitalUser13Data.RenTou, ZengxiaoNewKe = HospitalUser13Data.NewRenTou, HuliKeliu = HospitalUser13Data.Keliu, HuliShihao = HospitalUser13Data.Liaocheng, ShihaoProjectCount = HospitalUser13Data.Shihaoxiangmu, SendProjectCount = HospitalUser13Data.zengsongxiangmu, ChongzhiPrice = HospitalUser13Data.Chuzhika, XianjinProductPrice = HospitalUser13Data.SumMoney, KahuaProductPrice = HospitalUser13Data.Xianjin, XianjinliaochengPrice = HospitalUser13Data.Yinlianka, KahualiaochengPrice = HospitalUser13Data.Danxiangxiaohao });
            //店内动态
            hb.Add("CompanyNewslist", CompanyNewslist.Select(c => new { c.Name, c.ProjectName, c.HuiFangUserName }));
            //门店13大数据
            hb.Add("Hospital13Data", Hospital13Data);
            //顾客群洞察
            hb.Add("GukuDongchalist", CompanyNewslist1.Select(c => new { c.Name, c.HuiFangUserName }));
            IsoDateTimeConverter dtConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd" };
            return JsonConvert.SerializeObject(hb, dtConverter);
        }

        /// <summary>
        /// 首页测试用
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            Hashtable hb = new Hashtable();
            var HospitalID = ConvertValueHelper.ConvertIntValue(Request["HospitalID"]);
            //1、获取美容师13大数据 
            var HospitalUser13Data = cashBll.GetHospitalUser13Data(new TimeConsumingPerformanceEntity { HospitalID = HospitalID, StartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")), EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) });

            //3、门店13大数据
            var Hospital13Data = cashBll.GetHospital13DataList(new Hospital13DataEntity { HospitalID = HospitalID, Years = DateTime.Now.Year, Months = DateTime.Now.Month });

            ViewBag.HospitalUser13Data = HospitalUser13Data;
            ViewBag.Hospital13Data = Hospital13Data.Count > 0 ? Hospital13Data[0] : new Hospital13DataEntity();
            return View();

        }
        /// <summary>
        /// 美容师13大数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string GetHospitalUser13Data()
        {
            Hashtable hb = new Hashtable();
            var HospitalID = ConvertValueHelper.ConvertIntValue(Request["HospitalID"]);
            var UserID = ConvertValueHelper.ConvertIntValue(Request["UserID"]);
            var Years = ConvertValueHelper.ConvertIntValue(Request["Years"]);
            var Months = ConvertValueHelper.ConvertIntValue(Request["Months"]);
            //1、获取美容师13大数据
            var HospitalUser13Data = cashBll.GetHospitalUser13DataList(new HospitalUser13DataEntity { HospitalID = HospitalID, UserID = UserID, Years = Years, Months = Months });
            hb.Add("HospitalUser13Data", HospitalUser13Data);
            return JsonConvert.SerializeObject(hb);
        }
        /// <summary>
        /// 门店13大数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string GetHospital13Data()
        {
            Hashtable hb = new Hashtable();
            var HospitalID = ConvertValueHelper.ConvertIntValue(Request["HospitalID"]);
            var Type = ConvertValueHelper.ConvertIntValue(Request["Type"]);
            var Years = ConvertValueHelper.ConvertIntValue(Request["Years"]);
            var StartTime = ConvertValueHelper.ConvertDateTimeValueNew(Request["StartTime"]);
            var EndTime = ConvertValueHelper.ConvertDateTimeValueNew(Request["EndTime"]);

            var Hospital13Data = cashBll.SelectHospital13DataList(new Hospital13DataEntity { HospitalID = HospitalID, Years = Years, Type = Type, StartTime = StartTime, EndTime = EndTime });
            Hospital13DataEntity info = new Hospital13DataEntity();
            info.FirstExperienceCount = Hospital13Data.Sum(c => c.FirstExperienceCount);
            info.ExperienceCount = Hospital13Data.Sum(c => c.ExperienceCount);
            info.ZengxiaoKeCount = Hospital13Data.Sum(c => c.ZengxiaoKeCount);
            info.HuliKeliu = Hospital13Data.Sum(c => c.HuliKeliu);
            info.DabiaoKeliu = Hospital13Data.Sum(c => c.DabiaoKeliu);
            info.HuliShihao = Hospital13Data.Sum(c => c.HuliShihao);
            info.HuliShihaoOne = Hospital13Data.Sum(c => c.HuliShihaoOne);
            info.ShihaoProjectCount = Hospital13Data.Sum(c => c.ShihaoProjectCount);
            info.SendProjectCount = Hospital13Data.Sum(c => c.SendProjectCount);
            info.JichuPrice = Hospital13Data.Sum(c => c.JichuPrice);
            info.HezuoPrice = Hospital13Data.Sum(c => c.HezuoPrice);
            info.DaodianCount = Hospital13Data.Sum(c => c.DaodianCount);
            Hospital13Data.Add(info);
            hb.Add("Hospital13Data", Hospital13Data);
            return JsonConvert.SerializeObject(hb);


        }
        /// <summary>
        /// 顾客群洞察
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string GetGukuqundongchalist()
        {
            int rows, pagecount = 0;
            Hashtable hb = new Hashtable();
            var HospitalID = ConvertValueHelper.ConvertIntValue(Request["HospitalID"]);
            var UserID = ConvertValueHelper.ConvertIntValue(Request["UserID"]);
            var LiushiKe = ConvertValueHelper.ConvertIntValue(Request["LiushiKe"]);
            var StartTime = ConvertValueHelper.ConvertDateTimeValueNew(Request["StartTime"]);
            var EndTime = ConvertValueHelper.ConvertDateTimeValueNew(Request["EndTime"]);


            PatientEntity pentity = new PatientEntity();
            pentity.StartTime = pentity.StartTime == null ? DateTime.Now.AddMonths(-3).ToString() : pentity.StartTime;
            pentity.EndTime = pentity.EndTime == null ? DateTime.Now.ToString() : pentity.EndTime;
            pentity.LiushiKe = LiushiKe;
            if (pentity.IsActive == 0)
            {
                pentity.IsActive = 1;
            }
            pentity.StartTime = null;// DateTime.Now.ToString("yyyy-MM-dd 00:00:01");
            pentity.EndTime = null;// DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                                   //三个月内到店一次
            if (pentity.LiushiKe == 1) //今天
            {
                //pentity.IsSelectTime = 1;


                pentity.s_StareTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01"));
                pentity.s_Endtime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            }
            else if (pentity.LiushiKe == 2)//最近一周
            {
                pentity.s_StareTime = Convert.ToDateTime(DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd 00:00:01"));
                pentity.s_Endtime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            }
            else if (pentity.LiushiKe == 3)//最近一月
            {
                pentity.s_StareTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01"));
                pentity.s_Endtime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            }
            else if (pentity.LiushiKe == 4)
            {
                pentity.s_StareTime = ConvertValueHelper.ConvertDateTimeValueNew(StartTime.ToString("yyyy-MM-dd 00:00:01"));
                pentity.s_Endtime = ConvertValueHelper.ConvertDateTimeValueNew(EndTime.ToString("yyyy-MM-dd 23:59:59"));
            }
            pentity.HospitalID = HospitalID;
            pentity.numPerPage = String.IsNullOrEmpty(Request["numPerPage"]) ? 10 : ConvertValueHelper.ConvertIntValue(Request["numPerPage"]);//传递参数
            pentity.pageNum = String.IsNullOrEmpty(Request["pageNum"]) ? 1 : ConvertValueHelper.ConvertIntValue(Request["pageNum"]);
            if (pentity.orderField + "" == "") { pentity.orderField = "CreateTime"; }
            IList<PatientEntity> list = IpatBLL.GetPatientList(pentity, out rows, out pagecount);
            string ss = Request.Url.Host;
            foreach (var info in list)
            {
                info.Sexs = info.Sex == 1 ? "女" : "男";
                DateTime dt = DateTime.Now;
                ConvertValueHelper.ConvertDateTimeValue(info.Birthday, out dt);
                info.Age = DateTime.Now.Year - dt.Year;
                info.Integral = DateTime.Now.Day - info.LastTime.Day;
                info.Photo = "http://" + ss + info.Photo;
            }
            if (rows <= pentity.numPerPage && pentity.pageNum >= 2)
            {
                list = new List<PatientEntity>();
            }
            hb.Add("AllCount", rows);

            hb.Add("GukuDongchalist", list.Select(c => new { c.UserID, c.UserName, c.Age, c.Sexs, c.FirstTiyanTime, c.Integral, c.Photo, c.LastTime, c.ChuzhijieyuPirce }));
            IsoDateTimeConverter dtConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd" };
            return JsonConvert.SerializeObject(hb, dtConverter);
        }
        /// <summary>
        /// 查看用户剩余项目
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string GetPatientProjectlist()
        {
            Hashtable hb = new Hashtable();
            MainCardBalanceEntity entity = new MainCardBalanceEntity();
            var UserID = ConvertValueHelper.ConvertIntValue(Request["UserID"]);
            entity.UserID = UserID;
            entity.numPerPage = 500; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "AddTime"; }
            int row, pagecount = 0;
            int Xiaofei, Shengyu = 0;
            var list = cashBll.GetProjectTimesInUser(entity, out row, out pagecount, out Xiaofei, out Shengyu);
            hb.Add("Projectlist", list.Select(c => new { c.ProjectName, c.Tiems, c.ExpireTime }));
            IsoDateTimeConverter dtConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd" };
            return JsonConvert.SerializeObject(hb, dtConverter);
        }
        /// <summary>
        /// 门店动态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string GetCompanyNewslist()
        {
            Hashtable hb = new Hashtable();
            var HospitalID = ConvertValueHelper.ConvertIntValue(Request["HospitalID"]);

            int rows, pagecount = 0;
            CompanyNewsEntity entity = new CompanyNewsEntity();
            entity.HospitalID = HospitalID;
            entity.numPerPage = String.IsNullOrEmpty(Request["numPerPage"]) ? 10 : ConvertValueHelper.ConvertIntValue(Request["numPerPage"]);//传递参数
            entity.pageNum = String.IsNullOrEmpty(Request["pageNum"]) ? 1 : ConvertValueHelper.ConvertIntValue(Request["pageNum"]);
            entity.orderField = "ID";
            entity.orderDirection = "desc";
            entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            var CompanyNewslist = iCentBLL.GetCompanyNewsEntityList(entity, out rows, out pagecount);
            foreach (var info in CompanyNewslist)
            {
                info.ProjectName = CompanyNewsHelp.GetTypeName(info.Type);
                info.HuiFangUserName = CompanyNewsHelp.getDateStr(info.CreateTime);
            }
            if (rows <= entity.numPerPage && entity.pageNum >= 2)
            {
                CompanyNewslist = new List<CompanyNewsEntity>();
            }
            hb.Add("CompanyNewslist", CompanyNewslist.Select(c => new { c.Name, c.ProjectName, c.HuiFangUserName }));
            return JsonConvert.SerializeObject(hb);
        }

        /// <summary>
        /// 获取门店
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string GetHospitalList()
        {
            Hashtable hb = new Hashtable();
            var HospitalID = ConvertValueHelper.ConvertIntValue(Request["ParentHospitalID"]);
            var hospitalList = personnelManageBLL.GetHospitalListByParentID(HospitalID);
            hb.Add("HospitalList", hospitalList.Select(c => new { c.ID, c.Name }));
            return JsonConvert.SerializeObject(hb);
        }
    }
}
