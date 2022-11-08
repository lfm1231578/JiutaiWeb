
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EYB.ServiceEntity.BaseInfo;
using EYB.ServiceEntity.PersonnelEntity;
using BaseinfoManage.Factory.IBLL;
using HS.SupportComponents;
using Common.Helper;
using Webdiyer.WebControls.Mvc;
using EYB.ServiceEntity.CashEntity;
using CashManage.Factory.IBLL;
using EYB.ServiceEntity.SystemEntity;
using SystemManage.Factory.IBLL;
using System.Text;
using PersonnelManage.Factory.IBLL;
using WarehouseManage.Factory.IBLL;
using EYB.ServiceEntity.WarehouseEntity;
using PatientManage.Factory.IBLL;
using System.Data;
using EYB.ServiceEntity.BaseInfo.ImportExcel;
using EYB.ServiceEntity.PatientEntity;
using HS.SupportComponents.Base;
using System.IO;
using System.Data.SqlClient;
using NPOI.SS.Formula.Functions;

namespace EYB.Web.Controllers
{
    public class BaseInfoController : BaseController
    {

        #region 依赖注入

        private IBaseinfoBLL baseinfoBLL;//基础信息模块
        private ICashManageBLL icashManageBLL;//收银管理
        private ISystemManageBLL IsystemBLL;//系统模块
        private IPersonnelManageBLL PersonnelManageBLL;
        private IWarehouseManageBLL warehouseBLL;
        private IPatientManageBLL patientbll;

        public BaseInfoController()
        {
            baseinfoBLL = XmlRegistration.Current.GetResovleContainer().Resolve<IBaseinfoBLL>();
            icashManageBLL = XmlRegistration.Current.GetResovleContainer().Resolve<ICashManageBLL>();
            IsystemBLL = XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();
            PersonnelManageBLL = XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            warehouseBLL = XmlRegistration.Current.GetResovleContainer().Resolve<IWarehouseManageBLL>();
            patientbll = XmlRegistration.Current.GetResovleContainer().Resolve<IPatientManageBLL>();

        }
        #endregion 依赖注入

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EditStoredCard()
        {
            return View();
        }

        public ActionResult _StordCardList()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }

        public ActionResult _ProjectList()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }

        public ActionResult _ProductList()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }

        public ActionResult _CardList()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }

        public ActionResult EditTreatmentCard()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        public ActionResult EditProgramCard()
        {
            return View();
        }

        public ActionResult EditProduct()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        public ActionResult Editproject()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }
        public ActionResult Editproject1()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        public ActionResult CardBusiness()
        {
            return View();
        }
        /// <summary>
        /// 九泰报价
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientRegister1()
        {
            int rows;
            int pagecount;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.ProvinceCode = LoginUserEntity.ProvinceCode;
  
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<ProjectEntity> list = new List<ProjectEntity>();
            var entity = new ProjectEntity();
            if (HasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
                list = list.Where(c => c.IsMoni != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            }
            var result = list.AsQueryable().ToPagedList(1, 5000);
            ViewBag.Yixue = result.Where(c => c.IsHalf == 1).ToList();
            ViewBag.Shujutongji = result.Where(c => c.IsHalf == 2).ToList();
            ViewBag.CRA = result.Where(c => c.IsHalf == 3).ToList();
            ViewBag.Qita = result.Where(c => c.IsHalf == 4).ToList();
            return View();
        }
        public ActionResult ProjectEdit()
        {
            var ID = Request["ID"];//这是疗程卡ID
            var strlist = Request["project"];  //这是疗程卡编辑隐藏值
            if (!string.IsNullOrEmpty(strlist) && strlist != "0" && ID != null)
            {
                List<ProjectProductEntity> list = new List<ProjectProductEntity>();
                string[] count = strlist.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                decimal allmoney = 0;
                foreach (var info in count)
                {
                    string[] sinfo = info.Split(new Char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    allmoney = allmoney + (Convert.ToDecimal(sinfo[2]) * Convert.ToInt32(sinfo[1]));
                }


                foreach (var info in count)
                {
                    string[] sinfo = info.Split(new Char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    //ID|次数|消耗单价|总价|单价
                    ProjectProductEntity model = new ProjectProductEntity();
                    model.AllPrice = Convert.ToDecimal(sinfo[3]);
                    model.ID = Convert.ToInt32(ID);
                    model.ProjectID = Convert.ToInt32(sinfo[0]);
                    model.Qualtity = Convert.ToInt32(sinfo[1]);
                    //model.ConsumptionPrice = decimal.Round(Convert.ToDecimal(Convert.ToDecimal(sinfo[3]) / Convert.ToInt32(sinfo[1])), 2);// //Convert.ToDecimal(sinfo[2]);
                    model.ConsumptionPrice = Convert.ToDecimal(Convert.ToDecimal(sinfo[3]) / Convert.ToInt32(sinfo[1]));//2020.07.10 修改
                    //model.ConsumptionPrice = Convert.ToDecimal(sinfo[2]);//修改前的内容
                    model.AllPrice = Convert.ToDecimal(sinfo[3]);
                    model.Price = Convert.ToDecimal(sinfo[4]);
                    model.ProjectName = baseinfoBLL.GetProjectModel(Convert.ToInt32(sinfo[0])).Name;
                    model.AllMoney = allmoney;
                    model.ISKit = Convert.ToInt32(sinfo[5]);
                    list.Add(model);
                }
                var result = list.AsQueryable().ToPagedList(1, 500000);
                return View(result);
            }
            else if (!string.IsNullOrEmpty(ID) && strlist == "0")
            {
                var result = icashManageBLL.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = Convert.ToInt32(ID) });//获取对应关系
                return View(result);
            }
            ViewBag.Count = strlist;
            return View();
        }



        /// <summary>
        /// 门店信息
        /// </summary>
        /// <returns></returns>
        public ActionResult HospitalDetail()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return View();
        }
        /// <summary>
        /// 医院信息修改
        /// </summary>
        /// <returns></returns>
        public ActionResult HospitalUpdate()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 员工管理页面
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonnelManagePage(HospitalUserEntity entity)
        {

            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }

            var list = PersonnelManageBLL.GetAllUser(new HospitalUserEntity { HospitalID = entity.HospitalID, IsActive = -1 });
            if (!String.IsNullOrEmpty(entity.UserName))
            {
                list = list.Where(c => (c.UserName != null) && c.UserName.Contains(entity.UserName)).ToList();
            }
            if (!String.IsNullOrEmpty(entity.DutyName))
            {
                list = list.Where(c => (c.DutyName != null) && c.DutyName.Contains(entity.DutyName)).ToList();
            }
            if (entity.IsSeparation != -1)
            {
                list = list.Where(c => c.IsSeparation == entity.IsSeparation).ToList();
            }
            if (entity.IsActive == 0)
            {
                list = list.Where(c => c.IsActive == 1).ToList();
            }
            if (entity.IsActive == 1)
            {
                list = list.Where(c => c.IsActive == 0).ToList();
            }

            list = list.Where(i => i.IsSys != 1).ToList();

            list = SortHelper.DataSorting<HospitalUserEntity>(list, entity.orderField, entity.orderDirection).ToList();
            var result = list.AsQueryable().ToPagedList(entity.pageNum, 50);
            result.TotalItemCount = list.Count();
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_PersonnelManagePage", result);
            return View(result);
        }
        /// <summary>
        /// 员工控件
        /// </summary>
        /// <returns></returns>
        public ActionResult _PersonnelControl()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }
        /// <summary>
        /// 个人资料
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonnelDetail()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.UserID = LoginUserEntity.UserID;
            ViewBag.HospitalName = "顾客生日哈";
            return View();
        }
        /// <summary>
        /// 账号安全
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonnelAccount(SysOperateLogEntity entity)
        {
            int rows = 0;
            entity.numPerPage = 50;
            //entity.UserID = ViewBag.UserID = loginUserEntity.UserID;
            /*
             */
            // ViewBag.LoginAccount = loginUserEntity.LoginAccount;
            entity.orderField = "OccurTime";
            entity.orderDirection = "desc";
            entity.StartTime = DateTime.Now.AddDays(-7).ToString();
            entity.EndTime = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
            entity.HospitalID = LoginUserEntity.HospitalID;
            var list = IsystemBLL.GetSystemLog(entity, out rows);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            return View(result);
        }
        /// <summary>
        /// 产品搜索
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult TheProduct(ProductEntity entity)
        {

            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            entity.HospitalID = LoginUserEntity.HospitalID;//基础信息类全部用总店ID
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<ProductEntity> list = new List<ProductEntity>();
            if (HasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = baseinfoBLL.GetAllProduct(entity, out rows, out pagecount);
                list = list.Where(c => c.IsMoni != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = baseinfoBLL.GetAllProduct(entity, out rows, out pagecount);
            }
            // var list = baseinfoBLL.GetAllProduct(entity, out rows, out pagecount);

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
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.orderName = entity.Name;
            ViewBag.BrandID = entity.BrandID;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            if (Request.IsAjaxRequest())
                return PartialView("_TheProduct", result);
            return View(result);
        }

        /// <summary>
        /// 项目搜索
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult TheProject1(ProjectEntity entity)
        {
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            entity.HospitalID = LoginUserEntity.ParentHospitalID;//基础数据通用总店ID
            entity.numPerPage = 50; //每页显示条数

            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<ProjectEntity> list = new List<ProjectEntity>();
            if (HasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
                list = list.Where(c => c.IsMoni != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            }
            list = list.Where(c => c.IsHalf == 1).ToList();
            //var list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            //foreach (var info in result) {
            //    var enresult = baseinfoBLL.GetProjectModel(ConvertValueHelper.ConvertIntValue(info.ID.ToString()));
            //    var royaltyScheme = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = entity.ID });
            //    if(royaltyScheme.Count() > 0) {

            //        info.IsServiceFee = "是";
            //    } else
            //    {
            //        info.IsServiceFee = "否";
            //    }
            //}
            if (HasPermission1)//模拟账号
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
            ViewBag.orderName = entity.Name;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_TheProject1", result);
            return View(result);
        }
        /// <summary>
        /// 项目搜索
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult TheProject2(ProjectEntity entity)
        {
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            entity.HospitalID = LoginUserEntity.ParentHospitalID;//基础数据通用总店ID
            entity.numPerPage = 50; //每页显示条数

            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<ProjectEntity> list = new List<ProjectEntity>();
            if (HasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
                list = list.Where(c => c.IsMoni != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            }

            list = list.Where(c => c.IsHalf == 2).ToList();
            //var list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            //foreach (var info in result) {
            //    var enresult = baseinfoBLL.GetProjectModel(ConvertValueHelper.ConvertIntValue(info.ID.ToString()));
            //    var royaltyScheme = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = entity.ID });
            //    if(royaltyScheme.Count() > 0) {

            //        info.IsServiceFee = "是";
            //    } else
            //    {
            //        info.IsServiceFee = "否";
            //    }
            //}
            if (HasPermission1)//模拟账号
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
            ViewBag.orderName = entity.Name;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_TheProject2", result);
            return View(result);
        }

        /// <summary>
        /// 项目搜索
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult TheProject3(ProjectEntity entity)
        {
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            entity.HospitalID = LoginUserEntity.ParentHospitalID;//基础数据通用总店ID
            entity.numPerPage = 50; //每页显示条数

            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<ProjectEntity> list = new List<ProjectEntity>();
            if (HasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
                list = list.Where(c => c.IsMoni != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            }

            list = list.Where(c => c.IsHalf == 3).ToList();
            //var list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            //foreach (var info in result) {
            //    var enresult = baseinfoBLL.GetProjectModel(ConvertValueHelper.ConvertIntValue(info.ID.ToString()));
            //    var royaltyScheme = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = entity.ID });
            //    if(royaltyScheme.Count() > 0) {

            //        info.IsServiceFee = "是";
            //    } else
            //    {
            //        info.IsServiceFee = "否";
            //    }
            //}
            if (HasPermission1)//模拟账号
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
            ViewBag.orderName = entity.Name;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_TheProject3", result);
            return View(result);
        }
        public ActionResult TheProject4(ProjectEntity entity)
        {
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            entity.HospitalID = LoginUserEntity.ParentHospitalID;//基础数据通用总店ID
            entity.numPerPage = 50; //每页显示条数

            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<ProjectEntity> list = new List<ProjectEntity>();
            if (HasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
                list = list.Where(c => c.IsMoni != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            }

            list = list.Where(c => c.IsHalf == 4).ToList();
            //var list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            //foreach (var info in result) {
            //    var enresult = baseinfoBLL.GetProjectModel(ConvertValueHelper.ConvertIntValue(info.ID.ToString()));
            //    var royaltyScheme = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = entity.ID });
            //    if(royaltyScheme.Count() > 0) {

            //        info.IsServiceFee = "是";
            //    } else
            //    {
            //        info.IsServiceFee = "否";
            //    }
            //}
            if (HasPermission1)//模拟账号
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
            ViewBag.orderName = entity.Name;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_TheProject4", result);
            return View(result);
        }
        public ActionResult TheProject5(ProjectEntity entity)
        {
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            entity.HospitalID = LoginUserEntity.ParentHospitalID;//基础数据通用总店ID
            entity.numPerPage = 50; //每页显示条数

            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<ProjectEntity> list = new List<ProjectEntity>();
            if (HasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
                list = list.Where(c => c.IsMoni != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            }

            list = list.Where(c => c.IsHalf == 5).ToList();
            //var list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            //foreach (var info in result) {
            //    var enresult = baseinfoBLL.GetProjectModel(ConvertValueHelper.ConvertIntValue(info.ID.ToString()));
            //    var royaltyScheme = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = entity.ID });
            //    if(royaltyScheme.Count() > 0) {

            //        info.IsServiceFee = "是";
            //    } else
            //    {
            //        info.IsServiceFee = "否";
            //    }
            //}
            if (HasPermission1)//模拟账号
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
            ViewBag.orderName = entity.Name;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_TheProject5", result);
            return View(result);
        }
        /// <summary>
        /// 项目搜索
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult TheProject(ProjectEntity entity)
        {
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            entity.HospitalID = LoginUserEntity.ParentHospitalID;//基础数据通用总店ID
            entity.numPerPage = 50; //每页显示条数

            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<ProjectEntity> list = new List<ProjectEntity>();
            if (HasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
                list = list.Where(c => c.IsMoni != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            }

            //var list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            //foreach (var info in result) {
            //    var enresult = baseinfoBLL.GetProjectModel(ConvertValueHelper.ConvertIntValue(info.ID.ToString()));
            //    var royaltyScheme = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = entity.ID });
            //    if(royaltyScheme.Count() > 0) {

            //        info.IsServiceFee = "是";
            //    } else
            //    {
            //        info.IsServiceFee = "否";
            //    }
            //}
            if (HasPermission1)//模拟账号
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
            ViewBag.orderName = entity.Name;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_TheProject", result);
            return View(result);
        }

        public ActionResult EditRoyaltyScheme()
        {
            string type = Request["Type"].ToUpper();//类型
            int ID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["ID"]);//ID
            RoyaltySchemeEntity entity = new RoyaltySchemeEntity();
            if (type == "EDIT" && ID > 0)
            {
                entity = baseinfoBLL.GetRoyaltySchemeModel(new RoyaltySchemeEntity { ID = ID });
            }
            return View(entity);
        }

        public ActionResult StordCardPageList(MainCardEntity entity)
        {

            int rows;
            int pagecount;
            entity.Type = 1;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllPrePayCard(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.orderName = entity.Name;
            if (Request.IsAjaxRequest())
                return PartialView("_StordCardPageList", result);

            return View(result);
        }

        public ActionResult ChuZhika(MainCardEntity entity)
        {

            int rows;
            int pagecount;
            entity.Type = 1;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllPrePayCard(entity, out rows, out pagecount);
            list = list.Where(c => c.Name != "积分消费").ToList();
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = list.Count;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.orderName = entity.Name;
            if (Request.IsAjaxRequest())
                return PartialView("_StordCardPageList", result);

            return View(result);
        }


        public ActionResult TreamentCardPageList(MainCardEntity entity)
        {

            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            entity.Type = 2;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllTreatment(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.orderName = entity.Name;
            ViewBag.Type = entity.CardType;
            if (Request.IsAjaxRequest())
                return PartialView("_TreamentCardPageList", result);

            return View(result);
        }


        public ActionResult LiaoChengKa(MainCardEntity entity)
        {

            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            entity.Type = 2;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllTreatment(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.orderName = entity.Name;
            ViewBag.Type = entity.CardType;
            if (Request.IsAjaxRequest())
                return PartialView("_TreamentCardPageList", result);

            return View(result);
        }

        public ActionResult ProgramCardPageList(MainCardEntity entity)
        {

            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            entity.Type = 3;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllProgramme(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.orderName = entity.Name;
            ViewBag.Type = entity.CardType;
            if (Request.IsAjaxRequest())
                return PartialView("_ProgramCardPageList", result);

            return View(result);
        }

        public ActionResult BasicInformation()
        {
            return View();
        }

        public ActionResult EditBaseInfo()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        public ActionResult progCardInfo()
        {
            return View();
        }
        public ActionResult SysUserpage()
        {
            return View();
        }
        public ActionResult SysUserManage()
        {
            return View();
        }
        /// <summary>
        /// 系统用户----门店
        /// </summary>
        /// <returns></returns>
        public ActionResult UserManagePage(HospitalUserEntity entity)
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            //var entity = new HospitalUserEntity();

            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            else
            {
                if (LoginUserEntity.ParentHospitalID == LoginUserEntity.HospitalID)
                {
                    entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                }

            }
            if (!Request.IsAjaxRequest())
                entity.IsActive = 1;


            var list = PersonnelManageBLL.GetAllUser(entity);
            //系统用户
            list = list.Where(i => i.IsSys == 1 && i.IsAPP != 1).ToList();
            var result = list.AsQueryable().ToPagedList(1, 100000);
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_UserManagePage", result);
            return View(result);
        }

        /// <summary>
        /// 系统用户 ----APP
        /// </summary>
        /// <returns></returns>
        public ActionResult AppUserPage(HospitalUserEntity entity)
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            //var entity = new HospitalUserEntity();
            //if (loginUserEntity.ParentHospitalID == loginUserEntity.HospitalID)
            //{
            //    entity.HospitalID = 0;
            //    entity.ParentHospitalID = loginUserEntity.ParentHospitalID;
            //}
            //else
            //{
            //    entity.HospitalID = loginUserEntity.HospitalID;
            //    entity.ParentHospitalID = 0;
            //}

            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            else
            {
                if (LoginUserEntity.ParentHospitalID == LoginUserEntity.HospitalID)
                {
                    entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                }

            }
            if (!Request.IsAjaxRequest())
                entity.IsActive = 1;
            var list = PersonnelManageBLL.GetAllUser(entity);
            //系统手机客户端用户
            list = list.Where(i => i.IsSys == 1 && i.IsAPP == 1).ToList();
            var result = list.AsQueryable().ToPagedList(1, 100000);
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_AppUserPage", result);
            return View(result);
        }

        public ActionResult UserManageEdit()
        {
            var userId = Request["UserID"];

            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var info = PersonnelManageBLL.GetUserByUserID(HS.SupportComponents.ConvertValueHelper.ConvertIntValue(userId));
            if (info.HospitalID == 0)
            {
                info.HospitalID = LoginUserEntity.HospitalID;
            }

            var personnelInfo = PersonnelManageBLL.GetUserByUserID(info.ContactUserID);
            ViewBag.ContactUserName = personnelInfo.UserName;

            return View(info);
        }

        public ActionResult UpdateUserManage(HospitalUserEntity entity)
        {
            //var user_shouzimu = EYB.Web.Code.PageFunction.GetSpellCode(loginUserEntity.Name).ToLower();
            //var userShouzimu = Utils.ConvertSpeel(loginUserEntity.Name, true);

            var account = IsystemBLL.GetParentAccount(LoginUserEntity.ParentHospitalID);
            if (string.IsNullOrEmpty(account))
            {
                return Json(-2);
            }

            var result = 0;
            if (entity.UserID == 0)
            {
                HospitalMemberEntity he = new HospitalMemberEntity();
                //这里复制当前entity的信息给he实体类
                he.HospitalID = entity.HospitalID;
                he.IsActive = entity.IsActive;
                he.IsAdmin = entity.IsAdmin;
                he.LoginAccount = account + entity.LoginAccount;
                he.Password = entity.Password;
                he.Sex = entity.Sex;
                he.Type = entity.Type;
                he.UserID = entity.UserID;
                he.UserName = entity.UserName;
                he.Version = entity.Version;

                he.Password = Encrypt.MD5(entity.Password);
                he.Type = 1;
                he.IsActive = 1;
                he.IsAPP = entity.IsAPP;
                he.ContactUserID = entity.ContactUserID;
                // he.IsAdmin = 0;
                //entity.HospitalID = loginUserEntity.HospitalID;
                he.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                result = PersonnelManageBLL.AddHospitalMemberBYmenber(he);
                var quanXianList = PersonnelManageBLL.SelectVersionRightList(he.Version);
                List<Guid> list = new List<Guid>();
                foreach (var info in quanXianList)
                {
                    Guid id = new Guid(info.UIControlID);
                    list.Add(id);
                }
                IsystemBLL.SaveUserMenuRight(list, LoginUserEntity.UserID.ToString(), result.ToString());

                // PersonnelManageBLL.SaveHospitalMenuRight(list, he.UserID.ToString());



            }
            else
            {
                var info = PersonnelManageBLL.GetUserByUserID(entity.UserID);
                //info.HospitalID = entity.HospitalID;
                info.Version = entity.Version;
                if (info.Password != entity.Password)
                {
                    entity.Password = Encrypt.MD5(entity.Password);
                }
                else if (info.Password == entity.Password)
                {
                    entity.Password = "";
                }
                else
                {
                    entity.Password = null;
                }
                //修改固定门店，不进行修改，但是要重新赋值，因为控件是不可用状态，获取不到值
                //entity.HospitalID = info.HospitalID;
                result = PersonnelManageBLL.UpdateUser(entity);
                var quanXianList = PersonnelManageBLL.SelectVersionRightList(entity.Version);
                List<Guid> list = new List<Guid>();
                foreach (var info1 in quanXianList)
                {
                    Guid id = new Guid(info1.UIControlID);
                    list.Add(id);
                }
                IsystemBLL.SaveUserMenuRight(list, LoginUserEntity.UserID.ToString(), entity.UserID + "");

            }


            return Json(result);
        }



        #region ==get请求==

        /// <summary>
        /// 编辑提成方案
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult RoyaltySchemeAddorEdit(RoyaltySchemeEntity entity)
        {
            int result = 0;
            if (Request["Type"].ToUpper() == "ADD")//添加功能
            {
                result = baseinfoBLL.AddRoyaltyScheme(entity);
            }
            else if (Request["Type"].ToUpper() == "EDIT")
            {
                result = baseinfoBLL.UpdateRoyaltyScheme(entity);
            }

            return Json(result);
        }

        /// <summary>
        /// 获取存储卡列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult StoredCardList(MainCardEntity entity)
        {
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 6; //每页显示条数
            entity.Type = 1;//1是存储卡的分类
            entity.IsGive = 0;//这里是要获取可赠送的储值卡
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllPrePayCard(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_StordCardList", result);
            return View(result);

        }

        /// <summary>
        /// 添加或者编辑存储卡
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult StoredCardAddOrEdit(MainCardEntity entity)
        {
            string[] str = Request.Form.GetValues("GiveIDList");
            long result = 0;
            if (Request["Type"].ToUpper() == "ADD")//新增
            {
                entity.Name = entity.Name.Replace(" ", "");
                var list = baseinfoBLL.GetAllName(LoginUserEntity.ParentHospitalID);//获取所有的名称
                if (list.Where(i => i.Name == entity.Name).Count() > 0)
                {
                    return Json(-1);
                }
                entity.CreateTime = DateTime.Now;
                entity.Type = 1;//存储卡默认类别
                entity.HospitalID = LoginUserEntity.ParentHospitalID;//默认是总店ID
                result = baseinfoBLL.AddPrePayCard(entity);//添加储值卡

                //---------因为添加储值卡没有赠卡的概念了,所以这个不需要了
                //  AddProjectProduct(entity, HS.SupportComponents.ConvertValueHelper.ConvertIntValue(result), str, 1);


            }
            else if (Request["Type"].ToUpper() == "EDIT")//编辑功能
            {
                var list = baseinfoBLL.GetAllName(LoginUserEntity.ParentHospitalID).Where(i => i.Name == entity.Name);//获取所有的名称
                foreach (var info in list)
                {
                    if (info.ID != entity.ID)
                    {
                        return Json(-1);
                    }
                }
                baseinfoBLL.UpdatePrePayCard(entity);
                //修改前删除全部的赠卡
                icashManageBLL.DelByProgrammeID(entity.ID);

                result = entity.ID;

                //---------因为添加储值卡没有赠卡的概念了,所以这个不需要了
                //AddProjectProduct(entity, HS.SupportComponents.ConvertValueHelper.ConvertIntValue(result), str, 1);
                result = 1;

            }
            else//其它的功能
            {

            }
            return Json(result);


        }


        /// <summary>
        /// 编辑和添加疗程卡
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult TreatmentCardAddOrEdit(MainCardEntity entity)
        {
            var Liaochengliexing = entity.IsAdmin;
            if (Liaochengliexing == "0") entity.BaseType = 1;
            else if (Liaochengliexing == "1") entity.BaseType = 2;

            string[] str = Request.Form.GetValues("GiveIDList");  //ID|数量|消耗单价|总价|销售单价
            long result = 0;
            entity.Name = entity.Name.Replace(" ", "");
            if (Request["thetype"].ToUpper() == "ADD")//添加
            {
                var list = baseinfoBLL.GetAllName(LoginUserEntity.ParentHospitalID);//获取所有的名称
                if (list.Where(i => i.Name == entity.Name).Count() > 0)
                {
                    return Json(-1);
                }

                entity.HospitalID = LoginUserEntity.ParentHospitalID;
                entity.Type = 2;
                entity.CreateTime = DateTime.Now;
                result = baseinfoBLL.AddTreatment(entity);
                if (result > 0)
                {
                    AddProjectProduct(entity, Convert.ToInt32(result), str, 2);
                }
            }
            else if (Request["thetype"].ToUpper() == "EDIT")//修改
            {
                var list = baseinfoBLL.GetAllName(LoginUserEntity.ParentHospitalID).Where(i => i.Name == entity.Name);//获取所有的名称
                foreach (var info in list)
                {
                    if (info.ID != entity.ID)
                    {
                        return Json(-1);
                    }
                }
                baseinfoBLL.UpdateTreatment(entity);
                //修改前删除全部的赠卡
                icashManageBLL.DelByProgrammeID(entity.ID);
                result = entity.ID;

                AddProjectProduct(entity, Convert.ToInt32(result), str, 2);//添加项目详细
                result = 1;

            }
            else
            {

            }
            String sql = @"UPDATE EYB_tb_MainCard set BaseType = " + entity.BaseType + @"   WHERE 	HospitalID = " + LoginUserEntity.ParentHospitalID + @" AND isnull( IsActive, 0 ) = 0 and Name = '" + entity.Name + @"'  ";
            changeSqlData(sql);
            return Json(result);

        }

        public int changeSqlData(String sql)
        {  //执行
            try
            {
                using (SqlConnection con = new SqlConnection("server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft"))
                {
                    con.Open();//操作数据库的工具SqlCommand
                    SqlCommand cmd = new SqlCommand(sql, con);//(操作语句和链接工具)
                    int i = cmd.ExecuteNonQuery();//执行操作返回影响行数（）
                    con.Close();
                    return i;
                }
            }
            catch { return 99; }

        }
        /// <summary>
        /// 方案卡编辑和添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ProgramCardAddOrEdit(MainCardEntity entity)
        {
            string[] strProject = Request.Form.GetValues("GiveIDList");//项目
            string[] strProduct = Request.Form.GetValues("prod");//产品
            string[] strStored = Request.Form.GetValues("stordcardlist");//储值卡
            long result = 0;


            if (Request["Type"].ToUpper() == "ADD")//添加功能
            {
                var list = baseinfoBLL.GetAllName(LoginUserEntity.ParentHospitalID).Where(i => i.Name == entity.Name);//获取所有的名称
                foreach (var info in list)
                {
                    if (info.ID != entity.ID)
                    {
                        return Json(-1);
                    }
                }
                entity.CreateTime = DateTime.Now;
                entity.HospitalID = LoginUserEntity.ParentHospitalID;
                entity.Type = 3;
                result = baseinfoBLL.AddProgramme(entity);

                if (result > 0)
                {
                    AddProjectProduct(entity, Convert.ToInt32(result), strProject, 2);//添加项目详细
                    AddProjectProduct(entity, Convert.ToInt32(result), strProduct, 3);//添加产品详细
                    AddProjectProduct(entity, Convert.ToInt32(result), strStored, 1);//添加储值卡详细
                }


            }
            else if (Request["Type"].ToUpper() == "EDIT")//编辑
            {
                baseinfoBLL.UpdateProgramme(entity);

                //修改前删除全部的赠卡
                var c = icashManageBLL.DelByProgrammeID(entity.ID);
                result = entity.ID;

                AddProjectProduct(entity, Convert.ToInt32(result), strProject, 2);//添加项目详细
                AddProjectProduct(entity, Convert.ToInt32(result), strProduct, 3);//添加产品详细
                AddProjectProduct(entity, Convert.ToInt32(result), strStored, 1);//添加储值卡详细

                result = 1;

            }

            return Json(result);
        }


        /// <summary>
        /// 查询项目列表
        /// </summary>
        /// <returns></returns>
        public ActionResult ProjectList(ProjectEntity entity)
        {

            var Tepy = Request["Tepy"];
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 200; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<ProjectEntity> list = new List<ProjectEntity>();
            if (HasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
                list = list.Where(c => c.IsMoni != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            }
            if (Tepy != null)
            {
                if (Tepy == "1")
                {
                    list = list.Where(x => x.BaseType == 3).ToList();
                    //list = list.Where(x => x.TypeName == "大项目").ToList();
                }
                else
                {
                    list = list.Where(x => x.BaseType != 3).ToList();
                }
            }
            //var list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
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
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            if (Request.IsAjaxRequest())
                return PartialView("_ProjectList", result);
            return View(result);
        }

        /// <summary>
        /// 获取卡列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult CardList(MainCardEntity entity)
        {
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 9;
            if (entity.orderField + "" == "") { entity.orderField = "ID"; entity.orderDirection = "desc"; }
            IList<MainCardEntity> list = new List<MainCardEntity>();
            // var list = baseinfoBLL.GetAllPrePayCard1(entity, out rows, out pagecount);
            //if (entity.Type==2)
            /// {
            // list = baseinfoBLL.GetAllPrePayCard1(entity, out rows, out pagecount);
            //   list = baseinfoBLL.GetAllPrePayCard1(entity, out rows, out pagecount).GroupBy(t => t.ID).Select(t => t.First()).ToList();
            // list = list.GroupBy(t => t.ID).Select(t => t.First()).ToList();
            // list=(t=>t.id)
            // list = list.Where(t=>t.ID).;

            // }
            //  else
            // {
            list = baseinfoBLL.GetAllPrePayCard(entity, out rows, out pagecount);
            // }
            //  var list1 = baseinfoBLL.GetAllPrePayCard(entity, out rows, out pagecount);

            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_CardList", result);
            return View(result);
        }

        /// <summary>
        /// 获取产品
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductList(ProductEntity entity, int isHome = 0)
        {
            if (!string.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }

            //获取产品分类“家居类”id
            if (isHome == 1)
            {
                var productTypes = warehouseBLL.GetProductTypeNoPage(LoginUserEntity.ParentHospitalID, 2) as List<ProductTypeEntity>;
                if (productTypes != null && productTypes.Any())
                {
                    entity.ProductType = productTypes.Where(x => x.Name == "家居类").Select(x => x.ID).FirstOrDefault();
                }
            }
            ViewBag.IsHome = isHome;
            ViewBag.ProductType = entity.ProductType;

            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 200; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            bool hasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<ProductEntity> list = new List<ProductEntity>();
            if (hasPermission1)//模拟账号
            {
                entity.numPerPage = 1000; //每页显示条数
                list = baseinfoBLL.GetAllProduct(entity, out rows, out pagecount);
                list = list.Where(c => c.IsMoni != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = baseinfoBLL.GetAllProduct(entity, out rows, out pagecount);
            }
            //var list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
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
            if (Request.IsAjaxRequest())
                return PartialView("_ProductList", result);
            return View(result);
        }



        #endregion

        #region ==AJAX请求==
        /// <summary>
        /// 添加详细记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="result">返回的结果值</param>
        /// <param name="str">数组</param>
        /// <param name="type">类型:1.添加储值卡的,2.添加项目的,3添加产品的</param>
        private void AddProjectProduct(MainCardEntity entity, int result, string[] str, int type)
        {
            ProjectProductEntity pmodel = new ProjectProductEntity();

            pmodel.Type = type;
            pmodel.ProgrammeID = Convert.ToInt32(result);

            if (str != null)
            {
                foreach (string s in str)
                {

                    if (type == 2)   //ID|数量|消耗单价|总价|销售单价
                    {
                        string[] strlist = s.Split('|');
                        pmodel.ProjectID = int.Parse(strlist[0]);
                        pmodel.Qualtity = int.Parse(strlist[1]);
                        pmodel.ConsumptionPrice = decimal.Parse(strlist[2]);
                        ProjectEntity penty = baseinfoBLL.GetProjectModel(pmodel.ProjectID);//获取这条数据的单价.时常等信息
                        pmodel.ServiceTime = penty.ServiceTime;
                        pmodel.Price = penty.RetailPrice;
                        pmodel.AllPrice = Decimal.Parse(strlist[3]);
                        icashManageBLL.AddProjectProduct(pmodel);
                    }
                    else if (type == 3)//ID|数量|总价|单价
                    {
                        string[] strlist = s.Split('|');
                        pmodel.ProjectID = int.Parse(strlist[0]);
                        pmodel.Qualtity = int.Parse(strlist[1]);
                        //pmodel.ConsumptionPrice = decimal.Parse(strlist[2]);//产品没有消耗单价
                        ProductEntity penty = baseinfoBLL.GetModel(pmodel.ProjectID);//获取这条数据的单价.时常等信息
                        pmodel.Price = penty.RetailPrice;
                        pmodel.AllPrice = decimal.Parse(strlist[2]);
                        icashManageBLL.AddProjectProduct(pmodel);
                    }
                    else if (type == 1) //ID
                    {
                        pmodel.isGive = 0;
                        pmodel.Qualtity = 1;
                        pmodel.ProgrammeID = Convert.ToInt32(result);
                        pmodel.ProjectID = int.Parse(s);

                        MainCardEntity penty = baseinfoBLL.GetPrePayCardModel(pmodel.ProjectID);
                        pmodel.Price = penty.Price;
                        pmodel.AllPrice = penty.Price;
                        icashManageBLL.AddProjectProduct(pmodel);
                    }
                }
            }
        }


        /// <summary>
        /// 添加产品或者编辑产品
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ProductAddOrEdit(ProductEntity entity)
        {
            entity.Namepingyin = Utils.GetAllPYLetters(entity.Name);
            long result = 0;
            if (Request["Type"].ToUpper() == "ADD")//添加功能
            {
                entity.Name = entity.Name.Replace(" ", "");
                var list = baseinfoBLL.GetAllName(LoginUserEntity.ParentHospitalID);//获取所有的名称
                if (list.Where(i => i.Name == entity.Name).Count() > 0)
                {
                    return Json(-1);
                }
                entity.HospitalID = LoginUserEntity.ParentHospitalID;//基础信息里面存放总店ID
                if (entity.SellEndTime.Year == 1) entity.SellEndTime = DateTime.Now;
                result = baseinfoBLL.AddProduct(entity);
            }
            else if (Request["Type"].ToUpper() == "EDIT")//编辑功能
            {
                var list = baseinfoBLL.GetAllName(LoginUserEntity.ParentHospitalID).Where(i => i.Name == entity.Name);//获取所有的名称
                foreach (var info in list)
                {
                    if (info.ID != entity.ID)
                    {
                        return Json(-1);
                    }
                }

                if (entity.SellEndTime.Year == 1) entity.SellEndTime = DateTime.Now;
                result = baseinfoBLL.UpdateProduct(entity);
            }

            return Json(result);
        }


        /// <summary>
        /// 项目编辑/新建
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ProjectAddOrEdit1(ProjectEntity entity)
        {
            //var k = Request["Tag"];
            entity.Namepingyin = Utils.GetAllPYLetters(entity.Name);
            string[] strlist = Request.Form.GetValues("vallist");//提成方案数据列表 2|1|200|1|0.7|1|0.4

            long result = 0;
            long projectId = entity.ID;
            string[] productIdList = Request.Form.GetValues("ProductID");
            string[] danCiUseUnitList = Request.Form.GetValues("DanCiUseUnit");

            if (Request["Type"].ToUpper() == "ADD")//添加功能
            {
                var list = baseinfoBLL.GetAllName(LoginUserEntity.ParentHospitalID);//获取所有的名称
                entity.Name = entity.Name.Replace(" ", "");
                if (list.Any(i => i.Name == entity.Name))
                {
                    return Json(-1);
                }


                //添加项目
                entity.HospitalID = LoginUserEntity.ParentHospitalID;//基础信息全部改为父节点

                projectId = result = baseinfoBLL.AddProject(entity);



            }
            else if (Request["Type"].ToUpper() == "EDIT")
            {
                var list = baseinfoBLL.GetAllName(LoginUserEntity.ParentHospitalID).Where(i => i.Name == entity.Name);//获取所有的名称
                foreach (var info in list)
                {
                    if (info.ID != entity.ID)
                    {
                        return Json(-1);
                    }
                }
                result = baseinfoBLL.UpdateProject(entity);

            }
            return Json(result);
        }

        /// <summary>
        /// 项目编辑/新建
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ProjectAddOrEdit(ProjectEntity entity)
        {
            entity.Namepingyin = Utils.GetAllPYLetters(entity.Name);
            string[] strlist = Request.Form.GetValues("vallist");//提成方案数据列表 2|1|200|1|0.7|1|0.4

            long result = 0;
            long projectId = entity.ID;
            string[] productIdList = Request.Form.GetValues("ProductID");
            string[] danCiUseUnitList = Request.Form.GetValues("DanCiUseUnit");


            if (Request["Type"].ToUpper() == "ADD")//添加功能
            {
                var list = baseinfoBLL.GetAllName(LoginUserEntity.ParentHospitalID);//获取所有的名称
                entity.Name = entity.Name.Replace(" ", "");
                if (list.Any(i => i.Name == entity.Name))
                {
                    return Json(-1);
                }


                //添加项目
                entity.HospitalID = LoginUserEntity.ParentHospitalID;//基础信息全部改为父节点

                projectId = result = baseinfoBLL.AddProject(entity);

                //添加产品提成方案
                if (result > 0 && strlist != null)
                {
                    RoyaltySchemeEntity royEntity = new RoyaltySchemeEntity();
                    royEntity.ProjectID = (int)result;
                    foreach (string str in strlist)
                    {

                        string[] royaltyInfo = str.Split('|');
                        royEntity.AllPriceType = int.Parse(royaltyInfo[0]);
                        royEntity.StartPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(royaltyInfo[1]);
                        royEntity.EndPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(royaltyInfo[2]);
                        royEntity.NonSpecifiedType = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(royaltyInfo[3]);
                        royEntity.NonSpecifiedPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(royaltyInfo[4]);
                        royEntity.SpecifiedType = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(royaltyInfo[5]);
                        royEntity.SpecifiedPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(royaltyInfo[6]);

                        baseinfoBLL.AddRoyaltyScheme(royEntity);
                    }
                }


            }
            else if (Request["Type"].ToUpper() == "EDIT")
            {
                var list = baseinfoBLL.GetAllName(LoginUserEntity.ParentHospitalID).Where(i => i.Name == entity.Name);//获取所有的名称
                foreach (var info in list)
                {
                    if (info.ID != entity.ID)
                    {
                        return Json(-1);
                    }
                }
                result = baseinfoBLL.UpdateProject(entity);
                //删除原来的规则
                baseinfoBLL.DelRoyaltySchemeByprojectid(entity.ID);
                if (result > 0 && strlist != null)
                {
                    RoyaltySchemeEntity royEntity = new RoyaltySchemeEntity();
                    royEntity.ProjectID = entity.ID;
                    foreach (string str in strlist)
                    {

                        string[] royaltyInfo = str.Split('|');
                        royEntity.AllPriceType = int.Parse(royaltyInfo[0]);
                        royEntity.StartPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(royaltyInfo[1]);
                        royEntity.EndPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(royaltyInfo[2]);
                        royEntity.NonSpecifiedType = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(royaltyInfo[3]);
                        royEntity.NonSpecifiedPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(royaltyInfo[4]);
                        royEntity.SpecifiedType = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(royaltyInfo[5]);
                        royEntity.SpecifiedPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(royaltyInfo[6]);

                        baseinfoBLL.AddRoyaltyScheme(royEntity);
                    }
                }

            }
            if (productIdList != null && danCiUseUnitList != null)
            {
                result = baseinfoBLL.DeleteProjectProductRelation(new ProjectProductRelationEntity { ProjectID = (int)projectId });
                for (int i = 0; i < productIdList.Length; i++)
                {

                    result = baseinfoBLL.AddProjectProductRelation(new ProjectProductRelationEntity
                    {
                        ProductID = ConvertValueHelper.ConvertIntValue(productIdList[i]),
                        ProjectID = (int)projectId,
                        DanCiUseUnit = Convert.ToSingle(danCiUseUnitList[i])
                    });
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 基础信息获取table列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTablelist()
        {
            string strType = Request["type"];
            string id = Request["id"];
            StringBuilder str = new StringBuilder();
            IList<BaseinfoEntity> EntityList = new List<BaseinfoEntity>();

            if (!string.IsNullOrEmpty(strType))
            {
                if (strType != "Payment")
                {
                    EntityList = IsystemBLL.GetBaseInfoTreeByType(strType, 1, LoginUserEntity.ParentHospitalID);

                    foreach (BaseinfoEntity entity in EntityList)
                    {
                        //1不可以删除 0 可以删除
                        if (entity.IsDelete == 1)
                        {
                            str.Append("<tr><td>" + entity.InfoName + "</td><td style='display:none;' >" + entity.InfoCode + "</td><td><input type='hidden' class='hidid' value=" + entity.ID + " /> 默认类型（不可删除）</td></tr>");
                        }
                        else
                        {
                            //str.Append("<tr><td>" + entity.InfoName + "</td><td style='display:none;' >" + entity.InfoCode + "</td><td><input type='hidden' class='hidid' value=" + entity.ID + " />  <a href='/BaseInfo/EditBaseInfo?Type=Edit&id=" + entity.ID + "'>编辑</a>&nbsp;<a class='delbaseinfo' style='display:none;' >删除</a></td></tr>");
                            str.Append("<tr><td>" + entity.InfoName + "</td><td style='display:none;' >" + entity.InfoCode + "</td><td><input type='hidden' class='hidid' value=" + entity.ID + " />  <a href='/BaseInfo/EditBaseInfo?Type=Edit&id=" + entity.ID + "'>编辑</a>&nbsp;<a class='delbaseinfo'  >删除</a></td></tr>");
                        }

                    }

                }
                else
                {
                    EntityList = IsystemBLL.GetBaseInfoTreeByParentID(ConvertValueHelper.ConvertIntValue(id), strType, 1, LoginUserEntity.ParentHospitalID);
                    foreach (BaseinfoEntity entity in EntityList)
                    {
                        //str.Append("<tr><td>" + entity.InfoName + "</td><td style='display:none;' >" + entity.InfoCode + "</td><td><input type='hidden' class='hidid' value=" + entity.ID + " /> </td></tr>");
                        str.Append("<tr><td>" + entity.InfoName + "</td><td style='display:none;' >" + entity.InfoCode + "</td><td><input type='hidden' class='hidid' value=" + entity.ID + " />  <a href='/BaseInfo/EditBaseInfo?Type=Edit&id=" + entity.ID + "'>编辑</a>&nbsp;<a class='delbaseinfo' style='display:none;' >删除</a></td></tr>");
                    }
                }
            }
            return Content(str.ToString());
        }


        /// <summary>
        /// 删除产品
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductDel(int isshow)
        {
            int ID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["id"]);
            int result = 0;
            if (ID > 0)
            {
                result = baseinfoBLL.DelProduct(ID, isshow);
            }
            return Json(result);

        }

        /// <summary>
        /// 删除项目
        /// </summary>
        /// <returns></returns>
        public ActionResult ProjectDel(int isshow)
        {
            int ID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["id"]);
            int result = 0;
            if (ID > 0)
            {
                result = baseinfoBLL.DelProject(ID, isshow);
            }
            return Json(result);

        }
        /// <summary>
        /// 批量设置项目隐藏
        /// </summary>
        /// <returns></returns>
        public ActionResult SetProjectIsMoni(List<int> checkbox)
        {
            int result = baseinfoBLL.SetProjectIsMoni(checkbox, 2);
            return Json(result);
        }
        /// <summary>
        /// 批量设置产品隐藏
        /// </summary>
        /// <returns></returns>
        public ActionResult SetProductIsMoni(List<int> checkbox)
        {
            int result = baseinfoBLL.SetProductIsMoni(checkbox, 2);
            return Json(result);
        }
        /// <summary>
        /// 删除存储卡
        /// </summary>
        /// <returns></returns>
        public ActionResult StordCardDel(int IsActive)
        {
            int ID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["id"]);
            int result = 0;
            if (ID > 0)
            {
                result = baseinfoBLL.DelPrePayCard(ID, IsActive);
            }
            return Json(result);
        }

        /// <summary>
        /// 删除疗程卡
        /// </summary>
        /// <returns></returns>
        public ActionResult TreatmentCardDel(int IsActive)
        {
            int ID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["id"]);
            int result = 0;
            if (ID > 0)
            {
                result = baseinfoBLL.DelPrePayCard(ID, IsActive);
            }
            return Json(result);
        }

        /// <summary>
        /// 删除方案卡
        /// </summary>
        /// <returns></returns>
        public ActionResult ProgramCardDel()
        {
            int ID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["id"]);
            int result = 0;
            if (ID > 0)
            {
                result = baseinfoBLL.DelProgramme(ID);
            }
            return Json(result);
        }

        /// <summary>
        /// 删除基础信息
        /// </summary>
        /// <returns></returns>
        public ActionResult Delbaseinfo()
        {
            int ID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["id"]);
            int result = 0;
            if (ID > 0)
            {
                result = IsystemBLL.DeleteBaseInfoEntity(ID.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 编辑基础信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult BaseinfoEdit(BaseinfoEntity entity)
        {
            long result = 0;
            if (Request["Type"].ToUpper() == "ADD")//添加功能
            {
                //entity.parentID = 0;//父节点默认为0
                entity.HospitalID = LoginUserEntity.ParentHospitalID;
                //entity.IsShow=1;
                result = IsystemBLL.AddBaseInfoEntity(entity);
            }
            else if (Request["Type"].ToUpper() == "EDIT")//编辑功能
            {
                result = IsystemBLL.UpdateBaseInfoEntity(entity);
            }

            return Json(result);
        }
        #endregion

        #region==门店信息修改==
        public ActionResult UpdateHospital(HospitalEntity entity, string busiesstime1, string busiesstime2)
        {
            entity.BusinessHours = busiesstime1 + "-" + busiesstime2;
            //调用图片上传 ，返回图片地址
            // entity.Img = BufferToDiskByDate();
            string[] albumArr1 = Request.Form.GetValues("hid_photo_name1");
            if (albumArr1 != null && albumArr1.Length > 0)
            {
                for (int i = 0; i < albumArr1.Length; i++)
                {
                    string[] imgArr = albumArr1[i].Split('|');
                    if (imgArr.Length == 3)
                    {
                        entity.Img = imgArr[1];
                    }
                }
            }
            entity.ID = LoginUserEntity.HospitalID;
            int result = PersonnelManageBLL.UpHospital(entity);
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (result > 0)
            {
                return Json(1);
            }
            else
            {
                return Json(-1);
            }
        }

        #endregion

        #region ==用户设置===
        public ActionResult UserSettings1()
        {
            int hospitalID = LoginUserEntity.HospitalID;
            var entity = new UserSettingsEntity();
            entity.HospitalID = hospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            var list = IsystemBLL.GetUserSettingsList(entity);
            if (hospitalID == 1017 || hospitalID == 2)
            {
                list.Remove(list[0]);
            }
            var result = list.AsQueryable().ToPagedList(1, 10000);
            return View(result);
        }
        public ActionResult UserSettings()
        {
            int hospitalID = LoginUserEntity.HospitalID;
            var entity = new UserSettingsEntity();
            entity.HospitalID = hospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (ViewBag.HospitalID == 616 || ViewBag.HospitalID == 1108 || ViewBag.HospitalID == 1144 || ViewBag.HospitalID == 615)
            {
                try
                {
                    string conStr = "server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft";//连接字符串  
                    SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                    conText.Open();//打开数据库  
                    string sqls = "select AttributeValue from Eyb_tb_UserSettings where    HospitalID= " + ViewBag.HospitalID + " and Name = 'IsShowPrintPrice'";//创建统计语句  
                    SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                    SqlDataReader dr;//创建DataReader对象  
                    dr = comText.ExecuteReader();//执行查询  
                    while (dr.Read())//判断数据表中是否含有数据  
                    {
                        var date = dr;
                        ViewBag.IsPrintPrice = date["AttributeValue"].ToString();
                    }
                    dr.Close();//关闭DataReader对象  
                }
                catch
                {

                }
            }
            //ViewBag.IsPrintPrice = 1;
            var list = IsystemBLL.GetUserSettingsList(entity);
            if (hospitalID == 1017 || hospitalID == 2)
            {
                list.Remove(list[0]);
            }
            var result = list.AsQueryable().ToPagedList(1, 10000);
            return View(result);
        }

        /// <summary>
        /// 保存用户设置
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveSettings()
        {
            var list = IsystemBLL.GetUserSettingsList(new UserSettingsEntity() { HospitalID = LoginUserEntity.HospitalID });
            foreach (var info in list)
            {
                var infomodel = IsystemBLL.GetUserSettingsModel(info.ID);

                info.AttributeValue = Request.Form.GetValues(info.Name) == null ? "" : Request.Form.GetValues(info.Name)[0].ToString();
                if (LoginUserEntity.HospitalID == 1017)
                {

                }
                else
                {
                    if (info.Name == "Intergate")
                    {
                        if (!string.IsNullOrEmpty(Request.Form.GetValues(info.Name).ToString()))
                        {
                            if (Request.Form.GetValues(info.Name)[0].ToString() == "0")
                            {
                                return Json(-1);
                            }
                        }
                    }
                }
                IsystemBLL.UpdateUserSettings(info);//修改
            }


            return Json(1);

        }

        #endregion

        #region==品牌==
        /// <summary>
        /// 添加品牌页面
        /// </summary>
        /// <returns></returns>
        public ActionResult AddBrank()
        {
            BrandEntity be = new BrandEntity();
            be.pageNum = 1;
            be.numPerPage = 100000;
            string name = Request["Name"];
            ViewBag.Name = name;
            if (!string.IsNullOrEmpty(name))
            {
                be.Name = name;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            int rows, pagenum = 0;
            var list = warehouseBLL.GetBrandList(be, out rows, out pagenum);
            var result = list.AsQueryable().ToPagedList(1, 100000);//添加Webdiyer.WebControls.Mvc应用
            if (Request.IsAjaxRequest())
                return PartialView("_AddBrank", result);
            return View(result);
        }

        public ActionResult _AddBrank()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }


        /// <summary>
        /// 保存品牌
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveBrank(BrandEntity entity)
        {
            string[] brank = Request["Brank"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int rows, pagenum = 0;
            long result = 0;
            var list = warehouseBLL.GetBrandList(new BrandEntity { pageNum = 1, numPerPage = 1000, ParentHospitalID = LoginUserEntity.ParentHospitalID }, out rows, out pagenum);
            foreach (var info in list)
            {
                if (!brank.Contains(info.ParentID + ""))
                {
                    //删除数据
                    warehouseBLL.DelBrand(info.ID);
                }
            }
            var newlist = warehouseBLL.GetBrandList(new BrandEntity { pageNum = 1, numPerPage = 1000, ParentHospitalID = LoginUserEntity.ParentHospitalID }, out rows, out pagenum).Select(c => c.ParentID).ToList();
            foreach (var info in brank)
            {
                var id = ConvertValueHelper.ConvertIntValue(info);
                if (!newlist.Contains(id))
                {
                    var model = warehouseBLL.GetBrandModel(new BrandEntity { ID = id });
                    //添加数据
                    result = warehouseBLL.AddBrand(new BrandEntity { ParentHospitalID = LoginUserEntity.ParentHospitalID, CreatTime = DateTime.Now, ParentID = ConvertValueHelper.ConvertIntValue(info), Name = model.Name });
                }
            }
            return Json(result);
        }
        #endregion

        #region==类别==
        /// <summary>
        /// 添加类别页面
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCategory(int type)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            int rows, pagenum = 0;
            var list = warehouseBLL.GetProductTypeList(new ProductTypeEntity { pageNum = 1, numPerPage = 1000, Type = type }, out rows, out pagenum);
            return View(list);
        }
        /// <summary>
        /// 保存分类
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveProductType(ProductTypeEntity entity)
        {
            string[] brank = Request["Brank"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int rows, pagenum = 0;
            long result = 0;
            var list = warehouseBLL.GetProductTypeList(new ProductTypeEntity { pageNum = 1, numPerPage = 1000, ParentHospitalID = LoginUserEntity.ParentHospitalID, Type = entity.Type }, out rows, out pagenum);
            foreach (var info in list)
            {
                if (!brank.Contains(info.parentID + ""))
                {
                    //删除数据
                    warehouseBLL.DelProductType(info.ID);
                }
            }
            var newlist = warehouseBLL.GetProductTypeList(new ProductTypeEntity { pageNum = 1, numPerPage = 1000, ParentHospitalID = LoginUserEntity.ParentHospitalID, Type = entity.Type }, out rows, out pagenum).Select(c => c.parentID).ToList();
            foreach (var info in brank)
            {
                var id = ConvertValueHelper.ConvertIntValue(info);
                if (!newlist.Contains(id))
                {
                    var model = warehouseBLL.GetProductTypeModel(new ProductTypeEntity { ID = id });
                    //添加数据
                    result = warehouseBLL.AddProductType(new ProductTypeEntity { ParentHospitalID = LoginUserEntity.ParentHospitalID, CreatTime = DateTime.Now, parentID = ConvertValueHelper.ConvertIntValue(info), Name = model.Name, Type = entity.Type });
                }
            }
            return Json(result);
        }
        #endregion

        #region ==用户权限==
        public ActionResult CompetenceManage()
        {
            VersionEntity ve = new VersionEntity();
            ve.HospitalID = LoginUserEntity.ParentHospitalID;
            var list = PersonnelManageBLL.SelectVersionList(ve);
            var result = list.AsQueryable().ToPagedList(1, 10000);
            return View(result);
        }

        public ActionResult CompetenceManageP()
        {
            VersionEntity ve = new VersionEntity();
            ve.HospitalID = LoginUserEntity.ParentHospitalID;
            var list = PersonnelManageBLL.SelectVersionList(ve);
            var result = list.AsQueryable().ToPagedList(1, 10000);
            return View(result);
        }
        /// <summary>
        /// 添加权限
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCompetence()
        {
            return View();
        }

        /// <summary>
        /// 添加权限
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCompetenceP()
        {
            return View();
        }
        /// <summary>
        /// 添加权限
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult CompetenceAdd(VersionEntity entity)
        {
            entity.HospitalID = LoginUserEntity.ParentHospitalID;  //这个默认为父店ID
            var result = PersonnelManageBLL.AddVersion(entity);
            return Json(result);
        }

        public ActionResult CompetenceEdit()
        {
            ViewBag.UserID = LoginUserEntity.UserID;
            var hosp = PersonnelManageBLL.HospitalEntityByID(LoginUserEntity.ParentHospitalID);
            ViewBag.VersionID = hosp.VersionID;
            return View();
        }

        public ActionResult CompetenceEditP()
        {
            ViewBag.UserID = LoginUserEntity.UserID;
            var hosp = PersonnelManageBLL.HospitalEntityByID(LoginUserEntity.ParentHospitalID);
            ViewBag.VersionID = hosp.VersionID;
            return View();
        }
        public ActionResult EditCompetence()
        {
            var menuList = Request.Form.GetValues("UIControlID");
            var id = Request["ID"];

            var controlIdList = new List<Guid>();
            //获取选中菜单
            if (menuList != null)
            {
                controlIdList.AddRange(from menuId in menuList where !string.IsNullOrEmpty(id) select new Guid(menuId));
            }
            var result = PersonnelManageBLL.SaveHospitalMenuRight(controlIdList, id);
            //如果权限组保存成功,则把当前门店的当前权限组用户 的权限再写一遍.
            if (result > 0)
            {
                //获取当前门店下面的所有管理用户
                //由于现在只要分配足够的权限,就能更改权限组.
                //故修改权限组可以修改所有分店所有相同权限组的用户的权限.
                //var entity = new HospitalUserEntity { ParentHospitalID = LoginUserEntity.ParentHospitalID };

                ////获取所有用户
                //var list = PersonnelManageBLL.GetAllUser(entity);

                //////获取门店管理员账号
                ////entity.HospitalID = loginUserEntity.ParentHospitalID;
                ////entity.ParentHospitalID = loginUserEntity.HospitalID;
                ////var adminuser = PersonnelManageBLL.GetAllUser(entity);
                ////foreach (var info in adminuser)
                ////{
                ////    list.Add(info);
                ////}

                ////获取系统用户
                //list = list.Where(i => i.IsSys == 1 && i.IsAPP != 1).ToList();
                ////获取当前权限的用户
                //list = list.Where(i => i.Version == ConvertValueHelper.ConvertIntValue(id)).ToList();

                ////便利修改每个用户
                //foreach (var info in list)
                //{
                //    IsystemBLL.SaveUserMenuRight(controlIdList, LoginUserEntity.UserID.ToString(), info.UserID.ToString());
                //}

                IsystemBLL.BatchUpdateUserMenuRight(ConvertValueHelper.ConvertIntValue(id), LoginUserEntity.UserID);
            }


            return Json(result);
        }



        #endregion

        #region ==基础导出==
        /// <summary>
        /// 项目导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public FileContentResult ExportTheProjectList(ProjectEntity entity)
        {
            if (entity.Name == "搜索") entity.Name = "";
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            entity.HospitalID = LoginUserEntity.ParentHospitalID;//基础数据通用总店ID
            entity.numPerPage = 10000000; //每页显示条数
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.TransferofworkManager_模拟账号);//模拟账号控制查看权限
            IList<ProjectEntity> list = new List<ProjectEntity>();
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            if (HasPermission1)//模拟账号
            {
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
                list = list.Where(c => c.IsMoni != 2).ToList(); //2就是要隐藏
            }
            else
            {
                list = baseinfoBLL.GetAllProject(entity, out rows, out pagecount);
            }
            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("服务项目"), new DataColumn("负责人员"), new DataColumn("工时单价"), new DataColumn("工时"),
                new DataColumn("成本"), new DataColumn("备注说明") });
            foreach (ProjectEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["名称"] = model.Name;
                row["所属类别"] = model.TypeName;
                if (model.BaseType == 1)
                    row["基础类别"] = "基础项目";
                else
                    row["基础类别"] = "合作项目";
                if (model.ISKit == 1)
                    row["是否套盒"] = "否";
                else
                    row["是否套盒"] = "是";
                row["间隔天数"] = model.HuiDay;
                row["服务时长"] = model.ServiceTime;
                row["零售价"] = model.RetailPrice;
                row["成本价"] = "";
                row["品牌"] = model.BrandName;
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "XiangMu");
            //application/ms-excel  application/vnd.ms-excel
            return File(excel.WriteToStream().GetBuffer(), "application/octet-stream", "XiangMu.xls");
        }


        public FileContentResult ExportTheProductList(ProductEntity entity)
        {
            if (entity.Name == "搜索") entity.Name = "";
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            entity.HospitalID = LoginUserEntity.HospitalID;//基础信息类全部用总店ID
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 10000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllProduct(entity, out rows, out pagecount);
            DataTable tableExport = new DataTable("Export");
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_产品管理);
            //if (HasPermission1)
            //{
            //    tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("产品名称"), new DataColumn("零售价"), new DataColumn("成本价"), new DataColumn("所属类别"), new DataColumn("品牌"), new DataColumn("容量"), new DataColumn("规格"), new DataColumn("有效期(天)") });
            //}
            //else
            //{
            //    tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("产品名称"), new DataColumn("零售价"), new DataColumn("所属类别"), new DataColumn("品牌"), new DataColumn("容量"), new DataColumn("规格"), new DataColumn("有效期(天)") });
            //}
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("名称"), new DataColumn("所属类别"), new DataColumn("基础类别"), new DataColumn("容量"), new DataColumn("规格"), new DataColumn("可用天数"), new DataColumn("零售价"), new DataColumn("成本价"), new DataColumn("品牌"), new DataColumn("产品编码") });



            foreach (ProductEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["名称"] = model.Name;
                row["所属类别"] = model.ProductTypeName;

                if (model.BaseType == 1)
                {
                    row["基础类别"] = "常规产品";
                }
                else
                {
                    row["基础类别"] = "特殊产品";
                }
                //row["基础类别"] = model.BaseType;
                row["容量"] = model.UseUnit;
                row["规格"] = model.StandardUnitName;
                row["可用天数"] = model.ValidDay;
                row["零售价"] = model.RetailPrice;
                if (HasPermission1)
                {
                    row["成本价"] = model.CostPrice;
                }
                row["品牌"] = model.BrandName;
                row["产品编码"] = model.ID;
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "ChanPin");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "ChanPin.xlsx");

        }


        public FileContentResult ExportTheProductList1(ProductEntity entity)
        {
            if (entity.Name == "搜索") entity.Name = "";
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            entity.HospitalID = LoginUserEntity.HospitalID;//基础信息类全部用总店ID
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 10000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllProduct(entity, out rows, out pagecount);
            DataTable tableExport = new DataTable("Export");
            bool HasPermission1 = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_产品管理);
            //if (HasPermission1)
            //{
            //    tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("产品名称"), new DataColumn("零售价"), new DataColumn("成本价"), new DataColumn("所属类别"), new DataColumn("品牌"), new DataColumn("容量"), new DataColumn("规格"), new DataColumn("有效期(天)") });
            //}
            //else
            //{
            //    tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("产品名称"), new DataColumn("零售价"), new DataColumn("所属类别"), new DataColumn("品牌"), new DataColumn("容量"), new DataColumn("规格"), new DataColumn("有效期(天)") });
            //}
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("名称"), new DataColumn("所属类别"), new DataColumn("基础类别"), new DataColumn("容量"), new DataColumn("规格"), new DataColumn("可用天数"), new DataColumn("零售价"), new DataColumn("成本价"), new DataColumn("品牌"), new DataColumn("产品编码") });



            foreach (ProductEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["名称"] = model.Name;
                row["所属类别"] = model.ProductTypeName;

                if (model.BaseType == 1)
                {
                    row["基础类别"] = "常规产品";
                }
                else
                {
                    row["基础类别"] = "特殊产品";
                }
                //row["基础类别"] = model.BaseType;
                row["容量"] = model.UseUnit;
                row["规格"] = model.StandardUnitName;
                row["可用天数"] = model.ValidDay;
                row["零售价"] = model.RetailPrice;
                if (HasPermission1)
                {
                    row["成本价"] = model.CostPrice;
                }
                row["品牌"] = model.BrandName;
                row["产品编码"] = model.ID;
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "ChanPin");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "ChanPin.xls");

        }
        public FileContentResult ExportTreanebrCardList(MainCardEntity entity)
        {
            if (entity.Name == "搜索") entity.Name = "";
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            entity.Type = 2;
            entity.IsActive = Convert.ToInt32(Request["IsActive"]);
            // entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllTreatment(entity, out rows, out pagecount);
            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("卡项名称"), new DataColumn("金额"), new DataColumn("有效期(天)"), new DataColumn("次数") });
            foreach (MainCardEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["卡项名称"] = model.Name;
                row["金额"] = model.Price;
                row["有效期(天)"] = model.VaildityDay;
                row["次数"] = model.AllTimes;
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "Liaochengka");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "Liaochengka.xls");
        }

        public FileContentResult ExportStordCardList(MainCardEntity entity)
        {
            if (entity.Name == "搜索") entity.Name = "";
            int rows;
            int pagecount;
            entity.Type = 1;
            string IsActive = Request["IsActive"];
            entity.IsActive = Convert.ToInt32(IsActive);
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            //  entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllPrePayCard(entity, out rows, out pagecount);
            list = list.Where(c => c.Name != "积分消费").ToList();
            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("卡项名称"), new DataColumn("金额"), new DataColumn("有效期(天)") });
            foreach (MainCardEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["卡项名称"] = model.Name;
                row["金额"] = model.Price;
                row["有效期(天)"] = model.VaildityDay;
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "ChuzhiKa");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "ChuzhiKa.xls");
        }

        #endregion

        #region ==数据导入==

        /// <summary>
        /// 数据导入
        /// </summary>
        /// <returns></returns>
        public ActionResult DataImport()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }

        #region ==产品导入==

        /// <summary>
        /// 产品数据导入
        /// </summary>
        /// <param name="file"></param>
        /// <param name="IsJoinBrands"></param>
        /// <returns></returns>
        public ActionResult UploadProduct(string file, int IsJoinBrands)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile("产品");

            var mod = ImportData(filename, IsJoinBrands);
            if (mod.IsOK)
            {

                // Content("<script >alert('操作成功!');</script >", "text/html");
                return this.SuccessDirect("操作成功");
                // return "操作成功";
                //ShowMessage("操作成功!");
                //return RedirectToAction("DataImport", "BaseInfo");
            }
            else
            {
                return this.SuccessDirect("操作失败," + mod.message, "/BaseInfo/DataImport");
            }
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <returns></returns>
        public string SaveFile1(string typename)
        {

            var path = Server.MapPath("~/Uploads"); //网站根目录
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);


            //path += "/ImportFile/" + typename + "/";
            path = "D:\\Eye/Jiutai";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (string file in Request.Files)
            {
                var fileBase = Request.Files[file];
                try
                {
                    if (fileBase.ContentLength > 0)
                    {
                        var fileName = fileBase.FileName;
                        //保存文件
                        fileBase.SaveAs(path + "/" + fileName);

                        return fileName;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return "";
        }
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <returns></returns>
        public string SaveFile(string typename)
        {

            var path = Server.MapPath("~/Uploads"); //网站根目录
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);


            path += "/ImportFile/" + typename + "/";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (string file in Request.Files)
            {
                var fileBase = Request.Files[file];
                try
                {
                    if (fileBase.ContentLength > 0)
                    {
                        var fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + fileBase.FileName.Substring(fileBase.FileName.LastIndexOf("."));
                        //保存文件
                        fileBase.SaveAs(path + "/" + fileName);

                        return "/Uploads/ImportFile/" + typename + "/" + fileName;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return "";
        }


        /// <summary>
        /// 导入数据到数据库
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="IsJoinBrands"></param>
        /// <returns></returns>
        public JsonModelEntity ImportData(string fileName, int isjoinbrands)
        {
            JsonModelEntity mod = new JsonModelEntity();
            var BindList = new Dictionary<int, string>();
            List<Int32> AddIDList = new List<int>();
            int row = 0;
            int pagecount = 0;
            int allrow = 1;
            //查询卡项名称
            try
            {
                var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
                //将Excel中的数据写入table。格式要XLS
                DataTable excelData = null;
                try
                {

                    excelData = ExcelOperate.ExcelToTableForXLSX(Server.MapPath("~/") + fileName);
                }
                catch
                {

                    excelData = ExcelOperate.ExcelToTableForXLS(Server.MapPath("~/") + fileName);
                }
                List<ImportProductEntity> list = ModelConvertHelper<ImportProductEntity>.ConvertToModel(excelData).ToList();
                if (isjoinbrands == 1)//如果绑定品牌,则需要加入这些品牌
                {
                    var BrandsList = list.Select(i => i.品牌.Trim()).ToList();
                    BrandsList = BrandsList.Distinct().ToList();
                    BindList = AddBrandsAndBingBrand(BrandsList, isjoinbrands);
                }
                //获取所有品牌列表
                var allBrand = warehouseBLL.GetBrandList(new BrandEntity() { numPerPage = 50000000, pageNum = 1, ParentHospitalID = hospid }, out row, out pagecount);
                //遍历每个产品

                foreach (var info in list)
                {
                    allrow = allrow + 1;
                    var selbeand = allBrand.Where(i => i.Name == info.品牌.Trim()).ToList();
                    ProductEntity pe = new ProductEntity();
                    pe.Name = info.名称.Trim();
                    pe.Namepingyin = Utils.GetAllPYLetters(info.名称.Trim());
                    pe.ProductCode = info.产品编码;
                    pe.RetailPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.零售价);
                    pe.CostPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.成本价);
                    pe.PiFaPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.批发价);
                    pe.ProductType = GetProductType(info.所属类别, hospid, 2);

                    pe.UseUnit = float.Parse(info.容量);
                    pe.StandardUnitName = info.规格;
                    pe.AbleUseDay = Convert.ToInt32(info.可用天数);

                    if (info.基础类别 == "常规产品")
                    {
                        pe.BaseType = 1;
                    }
                    else
                    {
                        pe.BaseType = 2;
                    }

                    pe.Memo = null;
                    pe.IsShow = 2;
                    pe.SellEndTime = DateTime.FromOADate(HS.SupportComponents.ConvertValueHelper.ConvertIntValue(info.销售截止期));
                    pe.HospitalID = hospid;

                    //  var listpsf = IsystemBLL.GetBaseInfoTreeByType("ProductSpecification", 1, hospid);
                    var lists = IsystemBLL.GetBaseInfoTreeByTypeName("ProductSpecification", 1, hospid, info.规格);
                    var standardunit = lists.ID;
                    //var standardunit = 0;
                    //if (listpsf.Count > 0)
                    //{
                    //    standardunit = listpsf[0].ID;
                    //}
                    pe.StandardUnit = standardunit;//获取规格
                    pe.UnitRelationship = 0;
                    pe.ValidDay = 1000;//默认有效期
                    if (info.品牌 != null && info.品牌 != "" && selbeand.Count() > 0)
                    {
                        if (BindList.Count > 0)
                        {
                            var keys = BindList.Where(q => q.Value == info.品牌.Trim()).Select(q => q.Key).ToList();
                            pe.BrandID = keys[0];
                        }
                        else
                        {
                            pe.BrandID = selbeand[0].ID;

                        }
                    }
                    //   pe.AbleUseDay = 0;
                    var count = baseinfoBLL.SelectProductisNull(pe);
                    if (count == 0)
                    {
                        var id = baseinfoBLL.AddProduct(pe);
                        AddIDList.Add(Convert.ToInt32(id));
                    }
                }
                mod.IsOK = true;

                return mod;
            }
            catch (Exception e)
            {
                //删除添加的产品(品牌暂时不删除)
                foreach (var id in AddIDList)
                {
                    baseinfoBLL.DelProduct(id, 1);
                }
                mod.IsOK = false;
                mod.message = "报错行数：" + allrow + "," + e.Message;
                return mod;
            }

        }

        /// <summary>
        /// 通过名称找到可用的ID
        /// </summary>
        /// <param name="str"></param>
        /// <param name="_type">类别:1是项目 2是产品</param>
        /// <returns></returns>
        public int GetProductType(string str, int hospid, int _type)
        {
            int rows = 0;
            int pagecount = 0;
            var list = warehouseBLL.GetProductTypeNoPage(hospid, _type);

            var sellist = list.Where(i => i.Name == str).ToList();
            if (sellist.Count > 0)
            {
                return sellist[0].ID;
            }
            else if (list.Count > 0)
            {
                return list[0].ID;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 添加和绑定品牌
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public Dictionary<int, string> AddBrandsAndBingBrand(List<string> list, int isadd)
        {
            var Bind = new Dictionary<int, string>();

            int row = 0;
            int pagecount = 0;
            //获取所有品牌列表
            var allBrand = warehouseBLL.GetBrandList(new BrandEntity() { ParentHospitalID = 0, numPerPage = 50000000, pageNum = 1 }, out row, out pagecount);

            list.Where(m => m == "").ToList().ForEach(x => { list.Remove(x); });


            foreach (var info in list)
            {
                if (Bind.ContainsValue(info))
                    continue;

                //查找是否有当前名称的品牌
                var sel = allBrand.Where(i => i.Name == info).ToList();
                var select = sel.Count();
                if (select == 0)
                {
                    BrandEntity be = new BrandEntity();
                    be.ParentHospitalID = 0;
                    be.Name = info;
                    be.Namepingyin = Utils.GetAllPYLetters(info);
                    be.CreatTime = DateTime.Now;
                    var id = warehouseBLL.AddBrand(be);//添加系统品牌
                    be.ParentHospitalID = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
                    be.ParentID = (int)id;
                    var tid = warehouseBLL.AddBrand(be);//添加当前用户的绑定
                    Bind.Add((int)tid, info);
                }
                else
                {
                    sel[0].ParentHospitalID = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
                    int count = warehouseBLL.BrandIsNone(sel[0]);//这个是检查品牌是否被绑定,如果已经绑定的,则不要继续绑定
                    if (count <= 0)
                    {
                        var tid = warehouseBLL.AddBrand(sel[0]);//添加当前用户的绑定
                        Bind.Add((int)tid, sel[0].Name);
                    }

                }
            }

            return Bind;


        }

        #endregion





        #region ==项目导入==

        public ActionResult UploadProject(string file, int IsJoinBrands)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile("项目");

            var mod = ImportDataProject(filename, IsJoinBrands);

            if (mod.IsOK)
            {
                return this.SuccessDirect("操作成功");
            }
            else
            {
                return this.SuccessDirect("操作失败," + mod.message, "/BaseInfo/DataImport");
            }


        }

        public ActionResult UploadProject1(string file, int IsJoinBrands)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile("项目");

            var mod = ImportDataProject(filename, IsJoinBrands);

            if (mod.IsOK)
            {
                return this.SuccessDirect("操作成功");
            }
            else
            {
                return this.SuccessDirect("操作失败," + mod.message, "/BaseInfo/TheProject1");
            }


        }




        public JsonModelEntity ImportDataProject(string fileName, int isjoinbrands)
        {
            JsonModelEntity mod = new JsonModelEntity();
            List<Int32> AddIDList = new List<int>();
            int row = 0;
            int pagecount = 0;
            int allrow = 1;
            //查询卡项名称
            try
            {
                var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
                //写入table
                DataTable excelData = null;
                try
                {

                    excelData = ExcelOperate.ExcelToTableForXLSX(Server.MapPath("~/") + fileName);
                }
                catch
                {

                    excelData = ExcelOperate.ExcelToTableForXLS(Server.MapPath("~/") + fileName);
                }
                List<ImportProjectEntity> list = ModelConvertHelper<ImportProjectEntity>.ConvertToModel(excelData).ToList();
                var BrandsList = list.Select(i => i.品牌.Trim()).ToList();
                BrandsList = BrandsList.Distinct().ToList();
                var Bind = AddBrandsAndBingBrand(BrandsList, isjoinbrands);

                //获取所有品牌列表
                var allBrand = warehouseBLL.GetBrandList(new BrandEntity() { numPerPage = 50000000, pageNum = 1, ParentHospitalID = hospid }, out row, out pagecount);
                //便利每个产品
                foreach (var info in list)
                {
                    allrow = allrow + 1;
                    var selbeand = allBrand.Where(i => i.Name == info.品牌.Trim()).ToList();
                    ProjectEntity pe = new ProjectEntity();
                    pe.Name = info.名称.Trim();
                    pe.Namepingyin = Utils.GetAllPYLetters(info.名称.Trim());
                    pe.RetailPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.零售价);
                    pe.CostPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.成本价);
                    pe.ServiceTime = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(info.服务时长);
                    //    pe.ProjectType = GetProductType("其它", hospid, 1);

                    pe.ProjectType = GetProductType(info.所属类别, hospid, 1);
                    pe.HuiDay = Convert.ToInt32(info.间隔天数);
                    if (info.基础类别 == "基础项目")
                    {
                        pe.BaseType = 1;
                    }
                    else if (info.基础类别 == "特殊项目")
                    {
                        pe.BaseType = 2;
                    }
                    pe.Memo = info.编号;
                    pe.IsShow = 2;
                    pe.SellEndTime = DateTime.FromOADate(HS.SupportComponents.ConvertValueHelper.ConvertIntValue(info.销售截止期));
                    pe.HospitalID = hospid;
                    if (info.是否套盒 == "是")
                    {
                        pe.ISKit = 2;
                    }
                    else
                    {
                        pe.ISKit = 1;
                    }
                    //      pe.ISKit = info.是否套合 == "0" ? 2 : 1;
                    if (info.品牌 != null && info.品牌 != "" && selbeand.Count() > 0)
                    {
                        if (Bind.Count > 0)
                        {
                            var keys = Bind.Where(q => q.Value == info.品牌.Trim()).Select(q => q.Key).ToList();
                            pe.BrandID = keys[0];
                        }
                        else
                        {
                            pe.BrandID = selbeand[0].ID;

                        }

                    }
                    var count = baseinfoBLL.SelectProjectNull(pe);
                    if (count == 0)
                    {
                        var id = baseinfoBLL.AddProject(pe);
                        AddIDList.Add((int)id);
                    }

                }
                mod.IsOK = true;

                return mod;
            }
            catch (Exception e)
            {
                //删除添加的项目
                foreach (var id in AddIDList)
                {
                    baseinfoBLL.DelProject(id, 1);
                }
                mod.IsOK = false;
                mod.message = "报错行数：" + allrow + "," + e.Message;
                return mod;
            }

        }

        #endregion

        #region ==卡项导入==
        //卡项导入
        public ActionResult UploadCard(string file)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile("卡项");
            var mod = ImportDataCard(filename);

            if (mod.IsOK)
            {
                return this.SuccessDirect("操作成功");
            }
            else
            {
                return this.SuccessDirect("操作失败," + mod.message, "/BaseInfo/DataImport");
            }



        }
        public ActionResult UploadCard1(string file)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile("卡项");
            var mod = ImportDataCard1(filename);

            if (mod.IsOK)
            {
                return this.SuccessDirect("操作成功");
            }
            else
            {
                return this.SuccessDirect("操作失败," + mod.message, "/BaseInfo/DataImport");
            }



        }
        public JsonModelEntity ImportDataCard1(string fileName)
        {
            JsonModelEntity mod = new JsonModelEntity();
            List<Int32> AddIDList = new List<int>();
            int allrow = 0;
            var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
            try
            {

                DataTable excelData = null;
                try
                {

                    excelData = ExcelOperate.ExcelToTableForXLSX(Server.MapPath("~/") + fileName);
                }
                catch
                {

                    excelData = ExcelOperate.ExcelToTableForXLS(Server.MapPath("~/") + fileName);
                }
                List<ImportCardEntity> list = ModelConvertHelper<ImportCardEntity>.ConvertToModel(excelData).ToList();

                //var chuzhika = list.Where(o => o.类别名称 == "储值卡").ToList();
                //var liaochengka = list.Where(o => o.类别名称 != "储值卡").ToList();

                //便利储值卡集合
                foreach (var info in list)
                {
                    allrow = allrow + 1;
                    MainCardEntity entity = new MainCardEntity();
                    entity.Name = info.卡项名称.Trim();
                    entity.Price = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.购卡金额);
                    entity.CreateTime = DateTime.Now;
                    entity.VaildityDay = 3600;

                    entity.VaildityDay = string.IsNullOrEmpty(info.有效期) ? 3650 : Convert.ToInt32(info.有效期);
                    entity.Discount = 1;
                    entity.HospitalID = hospid;
                    entity.ISBuyCard = 1;
                    //if (info.类别名称 == "储值卡")
                    //{
                    entity.Type = 1;
                    //}
                    //else
                    //{
                    //entity.Type = 2;
                    //}

                    entity.IsSale = 1;
                    entity.IsBuyProduct = 1;
                    entity.AllTimes = string.IsNullOrEmpty(info.次数) ? 3650 : Convert.ToInt32(info.次数); //info.次数;
                    //entity.IsGive = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(info.是否赠卡);
                    entity.CardType = 0;
                    if (baseinfoBLL.SelectHaveCard(entity) == 0)
                    {
                        //if (string.IsNullOrEmpty(info.使用说明))
                        //{
                        //    var str = info.使用说明.Split(':');
                        //    if (str.Count() > 0)
                        //    {
                        //        if (str[0] == "专项套算卡")
                        //        {
                        //            continue;
                        //        }
                        //    }
                        //}

                        var id = baseinfoBLL.AddPrePayCard(entity);
                        AddIDList.Add(id);
                    }
                }

                mod.IsOK = true;

                return mod;
            }
            catch (Exception e)
            {
                //删除添加的卡项()
                foreach (var id in AddIDList)
                {
                    baseinfoBLL.DelPrePayCard(id, 1);
                }
                mod.IsOK = false;
                mod.message = "报错行数：" + allrow + "," + e.Message;
                return mod;
            }
        }
        public JsonModelEntity ImportDataCard(string fileName)
        {
            JsonModelEntity mod = new JsonModelEntity();
            List<Int32> AddIDList = new List<int>();
            int allrow = 0;
            var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
            try
            {

                DataTable excelData = null;
                try
                {

                    excelData = ExcelOperate.ExcelToTableForXLSX(Server.MapPath("~/") + fileName);
                }
                catch
                {

                    excelData = ExcelOperate.ExcelToTableForXLS(Server.MapPath("~/") + fileName);
                }
                List<ImportCardEntity> list = ModelConvertHelper<ImportCardEntity>.ConvertToModel(excelData).ToList();

                //var chuzhika = list.Where(o => o.类别名称 == "储值卡").ToList();
                //var liaochengka = list.Where(o => o.类别名称 != "储值卡").ToList();

                //便利储值卡集合
                foreach (var info in list)
                {
                    allrow = allrow + 1;
                    MainCardEntity entity = new MainCardEntity();
                    entity.Name = info.卡项名称.Trim();
                    entity.Price = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.购卡金额);
                    entity.CreateTime = DateTime.Now;
                    entity.VaildityDay = 3600;

                    entity.VaildityDay = string.IsNullOrEmpty(info.有效期) ? 3650 : Convert.ToInt32(info.有效期);
                    entity.Discount = 1;
                    entity.HospitalID = hospid;
                    entity.ISBuyCard = 1;
                    //if (info.类别名称 == "储值卡")
                    //{
                    entity.Type = 1;
                    //}
                    //else
                    //{
                    //    entity.Type = 2;
                    //}

                    entity.IsSale = 1;
                    entity.IsBuyProduct = 1;
                    entity.IsGive = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(info.是否赠卡);
                    entity.CardType = 0;
                    if (baseinfoBLL.SelectHaveCard(entity) == 0)
                    {
                        //if (string.IsNullOrEmpty(info.使用说明))
                        //{
                        //    var str = info.使用说明.Split(':');
                        //    if (str.Count() > 0)
                        //    {
                        //        if (str[0] == "专项套算卡")
                        //        {
                        //            continue;
                        //        }
                        //    }
                        //}

                        var id = baseinfoBLL.AddPrePayCard(entity);
                        AddIDList.Add(id);
                    }
                }

                mod.IsOK = true;

                return mod;
            }
            catch (Exception e)
            {
                //删除添加的卡项()
                foreach (var id in AddIDList)
                {
                    baseinfoBLL.DelPrePayCard(id, 1);
                }
                mod.IsOK = false;
                mod.message = "报错行数：" + allrow + "," + e.Message;
                return mod;
            }
        }


        #endregion


        #region ==员工导入==
        //员工导入
        public ActionResult UploadStaff(string file)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile("员工");
            var mod = ImportDataStaff(filename);

            if (mod.IsOK)
            {
                return this.SuccessDirect("操作成功");

            }
            else
            {
                return this.SuccessDirect("操作失败," + mod.message, "/BaseInfo/DataImport");
            }
        }

        /// <summary>
        /// 员工数据导入
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public JsonModelEntity ImportDataStaff(string fileName)
        {
            JsonModelEntity mod = new JsonModelEntity();
            List<Int32> AddIDList = new List<int>();
            int allrow = 0;
            var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
            try
            {

                DataTable excelData = ExcelOperate.ExcelToTableForXLSX(Server.MapPath("~/") + fileName);
                List<ImportStaffEntity> list = ModelConvertHelper<ImportStaffEntity>.ConvertToModel(excelData).ToList();
                //获取职务(职务现在暂时用自己的,在稍后加入所有职称便可)
                // var baseinfolist =  IsystemBLL.GetBaseInfoTreeByType("Job", 1, loginUserEntity.ParentHospitalID);

                var joblist = list.Where(i => i.职称 != null).ToList();
                joblist = joblist.Distinct().ToList();

                AddJob(joblist, hospid);

                //便利员工集合
                foreach (var info in list)
                {
                    allrow = allrow + 1;
                    var entity = new HospitalUserEntity();
                    entity.UserName = info.员工名称;
                    entity.Phone = info.手机号码 + "_" + info.工作电话 + "_" + info.其它电话;
                    entity.Address = info.快递地址;
                    entity.DutyName = info.职称;
                    entity.Grade = 0;
                    entity.HospitalID = GetHospitalIDByName(info.分店.Trim());
                    entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                    entity.IsActive = 1;
                    entity.IsAdmin = 0;
                    entity.Memo = info.员工编号;
                    entity.Sex = info.性别;
                    entity.Birthday = GetBirthday(info.年份, info.生日);
                    entity.Email = info.Email邮箱;
                    entity.CreateTime = DateTime.Now;
                    entity.Amount = 0;
                    entity.Version = 0;
                    entity.IdentityNo = info.身份证;
                    entity.IsSys = 0;
                    entity.Calendar = info.日历;
                    Random rd = new Random();
                    entity.LoginAccount = entity.UserName + rd.Next(10, 999);
                    entity.Password = "123456";
                    if (entity.HospitalID != 0)//如果找不到门店则设不添加此用户
                    {
                        var Id = PersonnelManageBLL.AddUser(entity);
                        AddIDList.Add(Id);
                    }
                }

                mod.IsOK = true;

                return mod;
            }
            catch (Exception e)
            {

                //删除添加的员工()
                PersonnelManageBLL.BatchActive(AddIDList, 0);
                mod.IsOK = false;
                mod.message = "报错行数：" + allrow + "," + e.Message;
                return mod;
            }
        }

        /// <summary>
        /// 获取生日
        /// </summary>
        /// <param name="_y"></param>
        /// <param name="_m"></param>
        /// <returns></returns>
        public string GetBirthday(string _y, string _m)
        {
            var _str = "";
            try
            {
                if (!string.IsNullOrEmpty(_y))
                {
                    int y = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(_y);
                    if (y == 0)
                    {
                        _str = "1990";//如果是0给个默认值
                    }
                    else
                    {
                        _str = y.ToString();
                    }
                }
                else
                {
                    _str = "1990";//如果是0给个默认值
                }

                if (!string.IsNullOrEmpty(_m) && _m.Contains('.'))
                {
                    var time = _m.Split('.');
                    if (time[0] == "00" || time[0] == "0")
                    {
                        time[0] = "01";
                    }
                    if (time[1] == "00" || time[1] == "0")
                    {
                        time[1] = "01";
                    }
                    _str = _str + "-" + time[0].PadLeft(2, '0') + "-" + time[1].PadLeft(2, '0');
                }
                else
                {
                    _str = _str + "-01-01";
                }
            }
            catch (Exception)
            {

                _str = "1990-01-01";
            }
            return _str;
        }


        /// <summary>
        /// 通过门店名称获取门店ID
        /// 如果没有这个门店就添加一个门店
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public int GetHospitalIDByName(string _Name)
        {
            var parentid = 0;
            if (LoginUserEntity.ParentHospitalID == 0)
            {
                parentid = LoginUserEntity.HospitalID;
            }
            else
            {
                parentid = LoginUserEntity.ParentHospitalID;
            }
            var entity = PersonnelManageBLL.HospitalEntityByName(_Name, parentid);
            if (entity.ID > 0)
            {
                return entity.ID;
            }
            else
            {
                ////如果找不到就添加一个门店
                //HospitalEntity he = new HospitalEntity();
                //he.Name = _Name;
                //he.ParentID = loginUserEntity.ParentHospitalID;
                //he.IsActive = 1;
                //he.CreateTime = DateTime.Now;
                //he.UpdateTime = DateTime.Now;
                //var id = PersonnelManageBLL.AddHospital(he);
                return 0;
            }
        }

        public void AddJob(List<ImportStaffEntity> _nameList, int hospID)
        {
            //获取列表
            var list = IsystemBLL.GetBaseInfoTreeByType("Job", 1, hospID);
            foreach (var info in _nameList)
            {
                if (list.Where(i => i.InfoName == info.职称).Count() == 0)
                {
                    BaseinfoEntity entity = new BaseinfoEntity();
                    entity.parentID = 0;//父节点默认为0
                    entity.InfoName = info.职称;
                    entity.HospitalID = LoginUserEntity.ParentHospitalID;
                    entity.InfoCode = "Job";
                    entity.InfoType = "职务";
                    entity.IsShow = 1;
                    IsystemBLL.AddBaseInfoEntity(entity);
                    list.Add(new BaseinfoEntity { InfoName = info.职称 });
                }
            }
        }


        #endregion

        #region  ==报价系统导入==
        public ActionResult UploadCustomerBaojia1(string file, string type)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            if (type == "上传文件")
            {
                string filename = SaveFile1(type);
                var mod = ImportDataCustomerbaojiao1(filename, type);

                if (mod.IsOK)
                {

                    return this.SuccessDirect("操作成功");

                }
                else
                {
                    return this.SuccessDirect("操作失败，" + mod.message, "/BaseInfo/TheProject5");
                }

            } else {

                string filename = SaveFile(type);
                var mod = ImportDataCustomerbaojiao(filename, type);

                if (mod.IsOK)
                {

                    return this.SuccessDirect("操作成功");

                }
                else
                {
                    return this.SuccessDirect("操作失败，" + mod.message, "/BaseInfo/TheProject5");
                }

            }
            //string type = "";

        }
        #endregion

        #region ==顾客导入==

        public ActionResult UploadCustomer(string file)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile("顾客");
            var mod = ImportDataCustomer(filename);

            if (mod.IsOK)
            {

                return this.SuccessDirect("操作成功");

            }
            else
            {
                return this.SuccessDirect("操作失败，" + mod.message, "/BaseInfo/DataImport");
            }
        }

        public ActionResult UploadCustomer1(string file)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile1("顾客");
            //var mod = ImportDataCustomer(filename);

            if (filename != "")
            {

                return this.SuccessDirect("操作成功");

            }
            else
            {
                return this.SuccessDirect("操作失败");
            }
        }

        public JsonModelEntity ImportDataCustomerbaojiao1(string fileName, string type)
        {
            JsonModelEntity mod = new JsonModelEntity();
            List<Int32> AddIDList = new List<int>();
            List<Int32> IRIDList = new List<int>();

            try
            {

                var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
                var tag = type;
                var entity = new ProjectEntity();
                if (fileName != null)
                {
                    entity.Name = fileName.Trim();
                }

                entity.BarCode = "9999";
                entity.RetailPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue("9999");

                entity.ServiceTime = ConvertValueHelper.ConvertIntValue("9999");
                entity.HuiDay = ConvertValueHelper.ConvertIntValue("9999");
                entity.Memo = tag.Trim();
                entity.ids = LoginUserEntity.UserName.Trim();
                entity.SellEndTime = DateTime.Now;
                entity.HospitalID = hospid;
                entity.IsHalf = 5;
                long id = baseinfoBLL.AddProject(entity);
                AddIDList.Add(Convert.ToInt32(id));

                mod.IsOK = true;
                return mod;
            }
            catch {
                mod.IsOK = false;
                return mod;
            }
        }
        public JsonModelEntity ImportDataCustomerbaojiao(string fileName, string type)
        {
            JsonModelEntity mod = new JsonModelEntity();
            List<Int32> AddIDList = new List<int>();
            List<Int32> IRIDList = new List<int>();
            var tag = type;
            var isHf = 0;
            if (tag == "医学")
            {
                isHf = 1;
            }
            if (tag == "数据统计")
            {
                isHf = 2;
            }
            if (tag == "CRA")
            {
                isHf = 3;
            }
            if (tag == "其他")
            {
                isHf = 4;
            }
            if (tag == "九泰报价")
            {
                isHf = 5;
            }
            var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
            int allrow = 1;
            try
            {
                DataTable excelData = null;
                try
                {
                    excelData = ExcelOperate.ExcelToTableForXLSX(Server.MapPath("~/") + fileName);
                }
                catch
                {
                    excelData = ExcelOperate.ExcelToTableForXLS(Server.MapPath("~/") + fileName);
                }
                List<ImportBaojiaEntity> list = ModelConvertHelper<ImportBaojiaEntity>.ConvertToModel(excelData).ToList();

                foreach (var info in list)
                {
                    allrow = allrow + 1;
                    var entity = new ProjectEntity();
                    if (info.服务项目 != null)
                    {
                        entity.Name = info.服务项目.Trim();
                    }
                    if (info.责任人员 != null)
                    {
                        entity.BarCode = info.责任人员.Trim();
                    }
                    if (info.工时单价 != null)
                    {
                        entity.RetailPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.工时单价);
                    }
                    if (info.工时 != null)
                    {
                        entity.ServiceTime = ConvertValueHelper.ConvertIntValue(info.工时);
                    }
                    if (info.成本 != null)
                    {
                        entity.HuiDay = ConvertValueHelper.ConvertIntValue(info.成本);
                    }
                    if (info.说明 != null)
                    {
                        entity.Memo = info.说明.Trim();
                    }
                    entity.IsHalf = isHf;
                    //添加项目
                    entity.HospitalID = LoginUserEntity.ParentHospitalID;//基础信息全部改为父节点
                    long id = baseinfoBLL.AddProject(entity);
                    AddIDList.Add(Convert.ToInt32(id));

                }
                mod.IsOK = true;

                return mod;
            }
            catch (Exception e)
            {
                foreach (var id in AddIDList)
                {
                    baseinfoBLL.DelProject(id, 0);//.DelPatient(id);
                }
                mod.IsOK = false;
                mod.message = "报错行数：" + allrow + "," + e.Message;
                return mod;
            }
        }
        public JsonModelEntity ImportDataCustomer(string fileName)
        {
            JsonModelEntity mod = new JsonModelEntity();
            List<Int32> AddIDList = new List<int>();
            List<Int32> IRIDList = new List<int>();

            var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
            int allrow = 1;
            try
            {
                AddCategorys();
                DataTable excelData = null;
                try
                {

                    excelData = ExcelOperate.ExcelToTableForXLSX(Server.MapPath("~/") + fileName);
                }
                catch
                {

                    excelData = ExcelOperate.ExcelToTableForXLS(Server.MapPath("~/") + fileName);
                }
                List<ImportCustomerEntity> list = ModelConvertHelper<ImportCustomerEntity>.ConvertToModel(excelData).ToList();

                foreach (var info in list)
                {
                    allrow = allrow + 1;
                    var entity = new PatientEntity();
                    if (info.客户名称 != null)
                    {
                        entity.UserName = info.客户名称.Trim();
                        entity.Namepingyin = Utils.GetAllPYLetters(info.客户名称.Trim());
                    }
                    entity.Sex = info.性别 == "女" ? 1 : 2;
                    var day = "28";
                    var bir = string.IsNullOrEmpty(info.生日) ? "2000-01-01" : info.生日;
                    try
                    {
                        var a = Convert.ToInt32(info.生日);
                        bir = "2000-01-01";
                    }
                    catch
                    {

                    }
                    if (!string.IsNullOrEmpty(info.生日))
                    {
                        if (bir.Contains("-"))
                        {
                            if (bir.Split('-')[1] == "2" || bir.Split('-')[1] == "02")
                            {

                                if (bir.Split('-')[2] == "29")
                                {
                                    bir = bir.Split('-')[0] + bir.Split('-')[1] + day;
                                }
                            }
                        }
                    }
                    entity.Birthday = bir;// info.生日;
                    entity.Mobile = info.手机号码;
                    entity.Password = "123456";
                    entity.IsActive = 1;
                    entity.CreateTime = DateTime.Now;
                    entity.Channel = "0";
                    entity.ArchivesType = "";
                    entity.Calendar = string.IsNullOrEmpty(info.日历) ? "2000-01-01" : info.日历;
                    entity.CustomerTypeID = string.IsNullOrEmpty(info.顾客分类) ? 26 : info.顾客分类 == "A客" ? 26 : info.顾客分类 == "B客" ? 27 : info.顾客分类 == "C客" ? 28 : 29;
                    entity.DaodianCount = 0;// string.IsNullOrEmpty(info.到店次数) ? 0 : Convert.ToInt32(info.到店次数);

                    try
                    {
                        var a = Convert.ToInt32(info.上次到店);
                        info.上次到店 = "2000-01-01";
                    }
                    catch
                    {

                    }
                    entity.LastTime = string.IsNullOrEmpty(info.上次到店) ? Convert.ToDateTime("2000-01-01") : Convert.ToDateTime(info.上次到店).Year == 1 ? Convert.ToDateTime("2000-01-01") : Convert.ToDateTime(info.上次到店); ;
                    entity.HospitalID = GetHospitalIDByName(info.分店.Trim());
                    entity.FollowUpUserID = GetFollowUpUserID(entity.HospitalID, info.咨询顾问);
                    entity.Integral = 0;
                    entity.MemberNo = info.客户卡号.Trim();
                    entity.ArrearsMoney = 0;
                    entity.Category = info.会员级别;
                    entity.FollowUpMrUserID = GetFollowUpUserID(entity.HospitalID, info.美容师);
                    entity.JieshaoUserID = 0;

                    try
                    {
                        var a = Convert.ToInt32(info.入会日期);
                        info.入会日期 = "2000-01-01";
                    }
                    catch
                    {

                    }
                    DateTime dtime = new DateTime(ConvertValueHelper.ConvertLongValue(info.入会日期));
                    entity.RuHuiTime = info.入会日期;

                    entity.ArchivesType = info.顾客类型;

                    if (entity.HospitalID > 0)
                    {
                        int id = patientbll.AddPatient1(entity);
                        AddIDList.Add(id);

                        IntegralEntity ir = new IntegralEntity();
                        ir.UserID = (int)id;
                        ir.Integral = 0;
                        ir.HospitalID = entity.HospitalID;
                        ir.CreateTime = DateTime.Now;

                        int irid = patientbll.AddIntegralUser(ir);
                        IRIDList.Add(irid);
                    }
                }
                mod.IsOK = true;

                return mod;
            }
            catch (Exception e)
            {
                foreach (var id in AddIDList)
                {
                    patientbll.DelPatient(id);
                }
                mod.IsOK = false;
                mod.message = "报错行数：" + allrow + "," + e.Message;
                return mod;
            }
        }


        /// <summary>
        /// 添加顾客类别
        /// 待定()
        /// </summary>
        public void AddCategorys()
        {
            //获取当前门店的类别
            var BaseInfolist = IsystemBLL.GetBaseInfoTreeByType("Category", 1, LoginUserEntity.ParentHospitalID);

            BaseinfoEntity be = new BaseinfoEntity();
            be.InfoCode = "Category";
            be.InfoType = "尊贵级别";
            be.HospitalID = LoginUserEntity.ParentHospitalID;
            be.IsShow = 1;

            for (int i = 1; i < 6; i++)
            {
                if (BaseInfolist.Where(o => o.InfoName == i + "星").Count() == 0)
                {
                    be.InfoName = i + "星";
                    IsystemBLL.AddBaseInfoEntity(be);
                }
            }




        }

        /// <summary>
        /// 通过名称获取顾客类别
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        public int GetCategoryIDByName(string _name)
        {
            var list = IsystemBLL.GetBaseInfoTreeByType("Category", 1, LoginUserEntity.ParentHospitalID);
            if (string.IsNullOrEmpty(_name))
            {
                return 0;
            }
            else
            {
                var len = _name.Length;
                var find = list.Where(i => i.InfoName == len + "星").ToList();
                if (find.Count > 0)
                {
                    return len;
                }
                else
                {
                    return len;
                }
            }

        }

        /// <summary>
        /// 获取跟进人员
        /// </summary>
        /// <param name="hospitalID">门店ID</param>
        /// <param name="_name">跟进人名称</param>
        /// <returns></returns>
        public int GetFollowUpUserID(int hospitalID, string _name)
        {
            var list = PersonnelManageBLL.GetAllUserByHospitalID(hospitalID, 0);
            list = list.Where(i => i.UserName == _name).ToList();
            if (list.Count > 0)
            {
                return list[0].UserID;
            }
            else
            {
                return 0;
            }
        }

        #endregion


        #region ==顾客更新==


        public ActionResult CustomerUpdate(string file)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile("顾客");
            var mod = UpdateDataCustomer(filename);

            if (mod.IsOK)
            {

                return this.SuccessDirect("操作成功");

            }
            else
            {
                return this.SuccessDirect("操作失败，" + mod.message, "/BaseInfo/DataImport");
            }
        }


        public JsonModelEntity UpdateDataCustomer(string fileName)
        {
            JsonModelEntity mod = new JsonModelEntity();
            List<Int32> AddIDList = new List<int>();
            List<Int32> IRIDList = new List<int>();
            int allrow = 0;
            var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;

            try
            {
                DataTable excelData = ExcelOperate.ExcelToTableForXLSX(Server.MapPath("~/") + fileName);
                List<ImportCustomerEntity> list = ModelConvertHelper<ImportCustomerEntity>.ConvertToModel(excelData).ToList();

                foreach (var info in list)
                {
                    allrow = allrow + 1;
                    var entity = new PatientEntity();
                    entity.UserName = info.客户名称.Trim();
                    entity.Mobile = info.手机号码.Trim();
                    entity.HospitalID = hospid;
                    var model = patientbll.GetPatienttEntityByMobile(entity.Mobile, entity.UserName, entity.HospitalID);
                    if (model == null)
                    {
                        entity.Namepingyin = Utils.GetAllPYLetters(info.客户名称.Trim());
                        entity.Sex = info.性别 == "女" ? 1 : 2;
                        entity.Birthday = GetBirthday(info.年份, info.生日);
                        entity.Password = "123456";
                        entity.IsActive = 1;
                        entity.CreateTime = DateTime.Now;
                        entity.Channel = "0";
                        entity.ArchivesType = "";
                        entity.Calendar = info.日历;
                        entity.HospitalID = GetHospitalIDByName(info.分店.Trim());
                        entity.FollowUpUserID = GetFollowUpUserID(entity.HospitalID, info.咨询顾问);
                        entity.Integral = 0;
                        entity.MemberNo = info.客户卡号.Trim();
                        entity.ArrearsMoney = 0;
                        entity.Category = GetCategoryIDByName(info.会员级别).ToString() + "星";
                        entity.FollowUpMrUserID = 0;//待修改
                        entity.JieshaoUserID = 0;
                        entity.RuHuiTime = info.入会日期;
                        if (entity.HospitalID > 0)
                        {
                            int id = patientbll.AddPatient(entity);
                            AddIDList.Add(id);

                            IntegralEntity ir = new IntegralEntity();
                            ir.UserID = (int)id;
                            ir.Integral = 0;
                            ir.HospitalID = entity.HospitalID;
                            ir.CreateTime = DateTime.Now;

                            int irid = patientbll.AddIntegralUser(ir);
                            IRIDList.Add(irid);
                        }
                    }
                    else
                    {
                        model.HospitalID = GetHospitalIDByName(info.分店);
                        patientbll.UpPatient(model);
                    }
                }
                mod.IsOK = true;

                return mod;
            }
            catch (Exception e)
            {
                mod.IsOK = false;
                mod.message = "报错行数：" + allrow + "," + e.Message;
                return mod;
            }
        }



        #endregion


        #region ==顾客疗程卡结余导入==

        public ActionResult UploadCustomerProject(string file)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile("顾客疗程卡");
            var mod = ImportDataCustomerProject(filename);

            if (mod.IsOK)
            {

                return this.SuccessDirect("操作成功");

            }
            else
            {
                return this.SuccessDirect("操作失败," + mod.message, "/BaseInfo/DataImport");
            }
        }


        public JsonModelEntity ImportDataCustomerProject(string _filename)
        {
            JsonModelEntity mod = new JsonModelEntity();
            List<Int32> AddIDList = new List<int>();
            List<Int32> AddUCIDList = new List<int>();
            int allrow = 0;
            var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
            try
            {
                DataTable excelData = ExcelOperate.ExcelToTableForXLSX(Server.MapPath("~/") + _filename);
                List<ImportCustomerProjectEntity> list = ModelConvertHelper<ImportCustomerProjectEntity>.ConvertToModel(excelData).ToList();
                foreach (var info in list)
                {
                    allrow = allrow + 1;
                    UserCardEntity uc = new UserCardEntity();
                    MainCardBalanceEntity mb = new MainCardBalanceEntity();
                    int _cardid = GetTreatmentCardID(info.卡项套餐名称.Trim());
                    var _projectid = GetProjectID(info.项目名称.Trim(), info.项目编号);
                    if (String.IsNullOrEmpty(info.客户名称) || String.IsNullOrEmpty(info.分店))
                    {
                        continue;
                    }
                    var userid = GetUserID(info.客户名称.Trim(), info.客户卡号, info.分店.Trim());
                    uc.CardID = _cardid;
                    uc.UserID = userid;
                    uc.CreateTime = GetDateTime(info.购买日期);
                    uc.Price = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.购买价);
                    uc.IsActive = 1;
                    var ucid = icashManageBLL.AddUserCard(uc);
                    AddUCIDList.Add(ucid);
                    mb.CardID = _cardid;
                    mb.ProjectID = _projectid;
                    mb.UserID = userid;
                    mb.Price = Math.Round(HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.购买价), 2);

                    mb.Tiems = ConvertValueHelper.ConvertIntValue(info.结余次数);
                    mb.AllTiems = ConvertValueHelper.ConvertIntValue(info.购买次数);
                    mb.KouTimes = ConvertValueHelper.ConvertIntValue(info.购买次数) - ConvertValueHelper.ConvertIntValue(info.结余次数);
                    mb.Type = 2;
                    mb.UserCardID = ucid;
                    mb.AllPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.购买价) * HS.SupportComponents.ConvertValueHelper.ConvertIntValue(info.购买次数);
                    mb.HuaKouZheLv = 0;
                    mb.ConsumePrice = mb.Price;
                    mb.ExpireTime = DateTime.Now.AddYears(10);
                    mb.AddTime = GetDateTime(info.购买日期);
                    var mbID = (int)icashManageBLL.AddMainCardBalance(mb);
                    AddIDList.Add(mbID);
                }
                mod.IsOK = true;

                return mod;
            }
            catch (Exception e)
            {
                foreach (var id in AddIDList)
                {
                    icashManageBLL.DelMainCardBalance(id);
                }

                foreach (var uid in AddUCIDList)
                {
                    icashManageBLL.DelProduct(uid);
                }
                mod.IsOK = false;
                mod.message = "报错行数：" + allrow + "," + e.Message;
                return mod;
            }
        }

        /// <summary>
        /// 时间处理
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public DateTime GetDateTime(string str)
        {
            DateTime dt = new DateTime();
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    dt = DateTime.FromOADate(HS.SupportComponents.ConvertValueHelper.ConvertIntValue(str));
                }
                catch (Exception)
                {
                    try
                    {
                        dt = Convert.ToDateTime(str);
                    }
                    catch (Exception)
                    {

                        dt = DateTime.Now;
                    }

                }
            }
            else
            {
                dt = DateTime.Now;
            }

            return dt;
        }

        /// <summary>
        /// 获取疗程卡项ID
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        private int GetTreatmentCardID(string _name)
        {
            int row, pagecount = 0;
            if (!string.IsNullOrEmpty(_name))
            {

                MainCardEntity me = new MainCardEntity();
                me.HospitalID = LoginUserEntity.ParentHospitalID;
                me.Name = _name;
                me.Type = 2;
                me.numPerPage = 500;
                var list = baseinfoBLL.GetAllTreatment(me, out row, out pagecount);
                if (list.Count > 0)
                {
                    return list[0].ID;
                }
                else//如果找不到当前疗程卡名称,则添加一个
                {
                    MainCardEntity entity = new MainCardEntity();
                    entity.Name = _name.Trim();
                    entity.Price = 0;
                    entity.CreateTime = DateTime.Now;
                    entity.Discount = 1;
                    entity.HospitalID = LoginUserEntity.ParentHospitalID;
                    entity.ISBuyCard = 0;
                    entity.Type = 2;
                    entity.IsSale = 1;
                    entity.IsBuyProduct = 0;
                    entity.IsGive = 0;
                    entity.CardType = 0;
                    var id = baseinfoBLL.AddPrePayCard(entity);
                    return id;
                }
            }
            else
            {
                _name = "疗程卡";
                MainCardEntity me = new MainCardEntity();
                me.HospitalID = LoginUserEntity.ParentHospitalID;
                me.Name = _name;
                me.Type = 2;
                me.numPerPage = 500;
                var list = baseinfoBLL.GetAllTreatment(me, out row, out pagecount);
                list = list.Where(i => i.Name == _name).ToList();
                if (list.Count > 0)
                {
                    return list[0].ID;
                }
                else//如果找不到当前疗程卡名称,则添加一个
                {
                    MainCardEntity entity = new MainCardEntity();
                    entity.Name = _name.Trim();
                    entity.Price = 0;
                    entity.CreateTime = DateTime.Now;
                    entity.Discount = 1;
                    entity.HospitalID = LoginUserEntity.ParentHospitalID;
                    entity.ISBuyCard = 0;
                    entity.Type = 2;
                    entity.IsSale = 1;
                    entity.IsBuyProduct = 0;
                    entity.IsGive = 0;
                    entity.CardType = 0;
                    var id = baseinfoBLL.AddPrePayCard(entity);
                    return id;
                }
            }
        }

        /// <summary>
        /// 获得项目产品名称
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        private int GetProjectID(string _name, string _id)
        {
            if (!string.IsNullOrEmpty(_name))
            {
                int row, pagecount = 0;
                ProjectEntity pe = new ProjectEntity();
                pe.HospitalID = LoginUserEntity.ParentHospitalID;
                pe.numPerPage = 50;
                pe.Name = _name.Trim();
                var list = baseinfoBLL.GetAllProject(pe, out row, out pagecount);
                if (list.Count > 0)
                {
                    var slist = list.Where(i => i.Memo == _id).ToList();
                    if (slist.Count > 0)
                    {
                        return slist[0].ID;
                    }
                    else
                    {
                        return list[0].ID;
                    }

                }
                else//添加项目
                {
                    var allBrand = warehouseBLL.GetBrandList(new BrandEntity() { numPerPage = 50000000, pageNum = 1, ParentHospitalID = LoginUserEntity.ParentHospitalID }, out row, out pagecount);
                    ProjectEntity pj = new ProjectEntity();
                    pj.Name = _name.Trim();
                    pj.Namepingyin = Utils.GetAllPYLetters(_name.Trim());
                    pj.RetailPrice = 0;
                    pj.CostPrice = 0;
                    pj.ServiceTime = 0;
                    pj.ProjectType = GetProductType("其它", LoginUserEntity.ParentHospitalID, 1);
                    pj.Memo = null;
                    pj.SellEndTime = DateTime.Now.AddYears(3);
                    pj.HospitalID = LoginUserEntity.ParentHospitalID;
                    pj.ISKit = 1;
                    pj.IsHalf = 0;
                    pj.BrandID = allBrand.Count > 0 ? allBrand[0].ID : 0;
                    var id = baseinfoBLL.AddProject(pj);
                    return (int)id;
                }

            }
            else
            {
                return 0;
            }


        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <param name="_name">顾客姓名</param>
        /// <param name="cardNo">卡号</param>
        /// <returns></returns>
        private int GetUserID(string _name, string cardNo, string Hospital)
        {
            var HospitalID = GetHospitalIDByName(Hospital);
            if (!string.IsNullOrEmpty(_name))
            {
                int row, pagecount = 0;
                PatientEntity pe = new PatientEntity();
                pe.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                //pe.UserName = _name.Trim();
                pe.Fax = _name.Trim();//直接查询,不用模糊查询
                pe.numPerPage = 50;
                pe.s_StareTime = Convert.ToDateTime("1970-01-01");
                pe.s_Endtime = Convert.ToDateTime("1970-01-01");
                if (pe.IsActive == 0)
                {
                    pe.IsActive = 1;
                }
                var list = patientbll.GetPatientList(pe, out row, out pagecount);
                if (cardNo != null)
                {
                    list = list.Where(i => i.MemberNo == cardNo).ToList();
                }
                if (HospitalID > 0)
                {
                    list = list.Where(i => i.HospitalID == HospitalID).ToList();
                }
                if (list.Count > 0)
                {
                    return list[0].UserID;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }

        }




        #endregion



        #region ==顾客储值卡结余导入==

        public ActionResult UploadCustomerChuZhika(string file)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile("顾客储值卡");
            var mod = ImportDataCustomerCard(filename);

            if (mod.IsOK)
            {

                return this.SuccessDirect("操作成功");

            }
            else
            {
                return this.SuccessDirect("操作失败，" + mod.message, "/BaseInfo/DataImport");
            }
        }


        public JsonModelEntity ImportDataCustomerCard(string _filename)
        {
            JsonModelEntity mod = new JsonModelEntity();
            List<Int32> AddIDList = new List<int>();
            List<Int32> AddUCIDList = new List<int>();
            int allrow = 0;
            var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
            try
            {
                DataTable excelData = ExcelOperate.ExcelToTableForXLSX(Server.MapPath("~/") + _filename);
                List<ImportCustomerCardEntity> list = ModelConvertHelper<ImportCustomerCardEntity>.ConvertToModel(excelData).ToList();
                list = list.Where(i => i.卡项名称 != null).ToList();
                foreach (var info in list)
                {
                    allrow = allrow + 1;
                    UserCardEntity uc = new UserCardEntity();
                    MainCardBalanceEntity mb = new MainCardBalanceEntity();
                    var _projectid = GetStoredCardID(info.卡项名称.Trim());
                    var userid = GetUserID(info.客户名称, info.客户卡号, info.分店);
                    uc.CardID = _projectid;
                    uc.UserID = userid;
                    uc.CreateTime = DateTime.Now;
                    uc.Price = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.总金额);
                    uc.IsActive = 1;
                    var ucid = icashManageBLL.AddUserCard(uc);
                    AddUCIDList.Add(ucid);
                    mb.CardID = ucid;
                    mb.ProjectID = _projectid;
                    mb.UserID = userid;
                    mb.Price = Math.Round(Convert.ToDecimal(info.余额), 2);
                    mb.Tiems = 0;
                    mb.AllTiems = 0;
                    mb.KouTimes = 0;
                    mb.Type = 1;
                    mb.UserCardID = ucid;
                    mb.AllPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.总金额) == 0 ? Math.Round(Convert.ToDecimal(info.余额), 2) : Math.Round(Convert.ToDecimal(info.总金额), 2);
                    mb.HuaKouZheLv = ConvertValueHelper.ConvertDecimalValue(info.划扣折率);
                    mb.ConsumePrice = 0;
                    mb.ExpireTime = GetDateTime(info.有效期);
                    mb.AddTime = DateTime.Now;
                    if (mb.Price > 0 && mb.UserID > 0)
                    {
                        var mbID = (int)icashManageBLL.AddMainCardBalance(mb);
                        AddIDList.Add(mbID);
                    }
                }
                mod.IsOK = true;

                return mod;
            }
            catch (Exception e)
            {
                foreach (var id in AddIDList)
                {
                    icashManageBLL.DelMainCardBalance(id);
                }

                foreach (var uid in AddUCIDList)
                {
                    icashManageBLL.DelProduct(uid);
                }
                mod.IsOK = false;
                mod.message = "报错行数：" + allrow + "," + e.Message;
                return mod;
            }
        }

        /// <summary>
        /// 获取储值卡项ID
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        private int GetStoredCardID(string _name)
        {
            if (!string.IsNullOrEmpty(_name))
            {
                int row, pagecount = 0;
                MainCardEntity me = new MainCardEntity();
                me.HospitalID = LoginUserEntity.ParentHospitalID;
                me.Name = _name;
                me.Type = 1;
                me.numPerPage = 500;
                var list = baseinfoBLL.GetAllTreatment(me, out row, out pagecount);
                list = list.Where(i => i.Name == _name).ToList();//防止相似的冲突
                if (list.Count > 0)
                {
                    return list[0].ID;
                }
                else//如果找不到当前储值卡名称,则添加一个
                {
                    MainCardEntity entity = new MainCardEntity();
                    entity.Name = _name.Trim();
                    entity.Price = 0;
                    entity.CreateTime = DateTime.Now;
                    entity.Discount = 1;
                    entity.HospitalID = LoginUserEntity.ParentHospitalID;
                    entity.ISBuyCard = 1;
                    entity.Type = 1;
                    entity.IsSale = 1;
                    entity.IsBuyProduct = 1;
                    entity.IsGive = 0;
                    entity.CardType = 0;
                    var id = baseinfoBLL.AddPrePayCard(entity);
                    return id;
                }
            }
            else
            {
                return 0;
            }
        }

        #endregion


        #region ==顾客欠款导入==
        public ActionResult UploadCustomerQianKuan(string file)
        {
            if (file == null)
            {
                return Content("没有文件！", "text/plain");
            }
            string filename = SaveFile("顾客欠款");
            var mod = ImportDataCustomerArrears(filename);

            if (mod.IsOK)
            {

                return this.SuccessDirect("操作成功");

            }
            else
            {
                return this.SuccessDirect("操作失败，" + mod.message, "/BaseInfo/DataImport");
            }
        }

        public JsonModelEntity ImportDataCustomerArrears(string fileName)
        {
            JsonModelEntity mod = new JsonModelEntity();
            List<string> AddOrderNoList = new List<string>();
            //List<Int32> IDList = new List<int>();
            int allrow = 0;
            var hospid = LoginUserEntity.ParentHospitalID == 0 ? LoginUserEntity.HospitalID : LoginUserEntity.ParentHospitalID;
            try
            {
                DataTable excelData = ExcelOperate.ExcelToTableForXLSX(Server.MapPath("~/") + fileName);
                List<ImportCustomerArrearsEntity> list = ModelConvertHelper<ImportCustomerArrearsEntity>.ConvertToModel(excelData).ToList();

                foreach (var info in list)
                {
                    allrow = allrow + 1;
                    OrderEntity entity = new OrderEntity();
                    entity.CreateTime = DateTime.Now;
                    entity.OrderNo = RandomHelper.GetRandomOrder(DateTime.Now);
                    entity.Price = -Convert.ToDecimal(info.欠款);
                    entity.UserID = GetUserID(info.客户名称, info.客户卡号, info.分店);
                    entity.HospitalID = GetHospitalIDByName(info.分店);
                    entity.Statu = 1;
                    entity.ReservationTime = DateTime.Now;
                    entity.ActurePrice = 0;
                    entity.QianPrice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.欠款);
                    entity.OrderType = 2;
                    entity.ReservationType = 1;
                    entity.AllQianprice = HS.SupportComponents.ConvertValueHelper.ConvertDecimalValue(info.欠款);
                    entity.DocumentNumber = "0000000";
                    entity.ISAudit = 2;
                    icashManageBLL.AddOrder(entity);
                }
                mod.IsOK = true;

                return mod;

            }
            catch (Exception e)
            {
                foreach (var ord in AddOrderNoList)
                {
                    icashManageBLL.DeleteOrder(ord);
                }
                mod.IsOK = false;
                mod.message = "报错行数：" + allrow + "," + e.Message;
                return mod;
            }

        }

        #endregion


        #endregion

        /// <summary>
        /// 批量查询成本
        /// </summary>
        /// <returns></returns>
        public ActionResult BatchSetCost(ProjectEntity entity)
        {
            if (!string.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            int rows;
            int pageCount;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 2000; //每页显示条数
            if (string.IsNullOrWhiteSpace(entity.orderField)) { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllProject(entity, out rows, out pageCount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            if (Request.IsAjaxRequest())
                return PartialView("_BatchSetCost", result);
            return View(result);
        }
        /// <summary>
        /// 批量查询成本
        /// </summary>
        /// <returns></returns>
        public ActionResult BatchSetCostProduct(ProductEntity entity)
        {
            int rows;
            int pagecount;
            if (!string.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            entity.HospitalID = LoginUserEntity.HospitalID;//基础信息类全部用总店ID
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 2000; //每页显示条数
            if (string.IsNullOrWhiteSpace(entity.orderField)) { entity.orderField = "ID"; }
            var list = baseinfoBLL.GetAllProduct(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.orderName = entity.Name;
            ViewBag.BrandID = entity.BrandID;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            if (Request.IsAjaxRequest())
                return PartialView("_BatchSetCostProduct", result);

            return View(result);
        }
        /// <summary>
        /// 批量查询成本
        /// </summary>
        /// <returns></returns>
        public ActionResult SetBatchSetCost()
        {
            var operateType = Request["operateType"];
            string[] prodcutlist = Request.Form.GetValues("ProductID");
            string[] CostPricelist = Request.Form.GetValues("CostPrice");
            int result = 0;
            for (int i = 0; i < prodcutlist.Length; i++)
            {
                result = baseinfoBLL.BatchSetCost(ConvertValueHelper.ConvertIntValue(operateType), ConvertValueHelper.ConvertIntValue(prodcutlist[i]), ConvertValueHelper.ConvertDecimalValue(CostPricelist[i]));
            }
            return Json(result);
        }
        #region MyRegion
        public ActionResult SetSalary(string salar, string types)
        {
            int hospitalID = LoginUserEntity.HospitalID;
            var entitys = new SystemUserSettingsEntity();
            entitys.HospitalID = hospitalID;
            var sals = salar;
            entitys.AttributeValue = sals;
            entitys.AttributeID = hospitalID;
            if (types == "add")
            {
                var results = IsystemBLL.AddSystemUserSettingsByPay(entitys);
                return Json('1');
            }
            else
            {
                var resultss = IsystemBLL.UpdateUserSettingsByPay(entitys);
                return Json('2');
            }
        }
        #endregion
        #region MyRegion
        public ActionResult InitSalary(string salar, string types)
        {
            var entitys = new SystemUserSettingsEntity();
            int hospitalID = LoginUserEntity.HospitalID;
            entitys.HospitalID = hospitalID;
            var test = IsystemBLL.GetSystemUserSettingsModelByPay(entitys);
            return Json(test.AttributeValue);
        }
        #endregion

        #region MyRegion
        public ActionResult salaryStructure()
        {
            return View();
        }

        #endregion



    }
}
