using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using BaseinfoManage.Factory.IBLL;
using CashManage.Factory.IBLL;
using EYB.ServiceEntity.WarehouseEntity;
using WarehouseManage.Factory.IBLL;
using EYB.ServiceEntity.BaseInfo;
using Common.Helper;
using System.Text;
using System.Data;
using HS.SupportComponents;
using SystemManage.Factory.IBLL;
using EYB.ServiceEntity.PatientEntity;
using EYB.ServiceEntity.CashEntity;
using PersonnelManage.Factory.IBLL;
using CenterManage.Factory.IBLL;
using System.Data.SqlClient;

namespace EYB.Web.Controllers
{
    public class WarehouseManageController : BaseController
    {
        #region 依赖注入

        private IBaseinfoBLL baseinfoBLL;//基础信息模块
        private ICashManageBLL icashManageBLL;//收银管理
        private IWarehouseManageBLL iwareBLL;//仓库管理
        private ISystemManageBLL iSysBLL;//系统管理
        private IPersonnelManageBLL PersBLL;
        private ICenterManageBLL iCentBLL;//管理中心
        public WarehouseManageController()
        {
            baseinfoBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IBaseinfoBLL>();
            icashManageBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ICashManageBLL>();
            iwareBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IWarehouseManageBLL>();
            iSysBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();
            PersBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            iCentBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ICenterManageBLL>();
        }
        #endregion 依赖注入


        #region ==Get方法==

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 品牌列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult BrandList(BrandEntity entity)
        {
            if (entity.Name == "搜索") entity.Name = "";
            int rows = 0;
            int pagecount;
            entity.numPerPage = 10; //每页显示条数

            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = iwareBLL.GetBrandList(entity, out rows, out pagecount);
            var allnumber = 0;//总共数量
            var allpeice = 0.00;//总共金额

            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.orderName = entity.Name;
            return View(result);
        }

        /// <summary>
        /// 编辑品牌页面
        /// </summary>
        /// <returns></returns>
        public ActionResult EditBrand()
        {
            return View();
        }

        /// <summary>
        /// 编辑品牌
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult BrandEdit(BrandEntity entity)
        {
            int result = 0;
            if (Request["Type"].ToUpper() == "ADD" && entity.Name != null)//添加
            {
                entity.CreatTime = DateTime.Now;
                result = Convert.ToInt32(iwareBLL.AddBrand(entity));

            }
            else if (Request["Type"].ToUpper() == "EDIT" && entity.Name != null)
            {
                result = iwareBLL.UpdateBrand(entity);
            }
            return Json(result);
        }
        /// <summary>
        /// 删除操作
        /// </summary>
        /// <returns></returns>
        public ActionResult DelBrand()
        {
            int result = 0;
            int id = Convert.ToInt32(Request["ID"]);
            if (id > 0)
            {
                result = iwareBLL.DelBrand(id);
            }
            return Json(result);

        }

        #endregion

        #region ==仓库管理==
        /// <summary>
        /// 仓库管理
        /// </summary>
        /// <returns></returns>
        public ActionResult WarehouseManage()
        {

            return View();
        }

        #region ==入库==
        /// <summary>
        /// 入库页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Ruku()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        /// <summary>
        /// 产品页面
        /// </summary>
        /// <returns></returns>
        public ActionResult _PdList()
        {
            return View();
        }

        /// <summary>
        /// 产品页面
        /// </summary>
        /// <returns></returns>
        public ActionResult _PdListNew()
        {
            return View();
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
        }

        /// <summary>
        /// 产品页面
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult PdList(ProductEntity entity)
        {
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 200; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "Name"; entity.orderDirection = "asc"; }
            var list = baseinfoBLL.GetAllProduct(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_PdList", result);
            return View(result);
        }


        /// <summary>
        /// 产品页面
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult PdListNew(ProductEntity entity)
        {
            ViewBag.Name = "";
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
                ViewBag.Name = entity.Name;
            }
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 200; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "Name"; entity.orderDirection = "asc"; }
            var list = baseinfoBLL.GetAllProduct(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_PdList", result);
            return View(result);
        }

        /// <summary>
        /// 产品入库
        /// </summary>
        /// <returns></returns>
        public ActionResult AddRuku()
        {
            int Allquatity = 0;
            string[] IDlist = Request.Form.GetValues("ID");
            string[] NumberList = Request.Form.GetValues("Number");
            string HouseID = Request["HouseID"];//仓库
            string UserID = Request["UserID"];//经手人
            string BaseID = Request["BaseID"];//操作原因
            string Memo = Request["Memo"];//备注
            #region ==入库==
            StockOrderEntity entity = new StockOrderEntity();
            entity.BaseID = int.Parse(BaseID);
            entity.HospitalID = LoginUserEntity.HospitalID;
            // entity.HouseID = int.Parse(HouseID);
            entity.IsVerify = 1;
            entity.Memo = Memo;
            entity.OrderNo = RandomHelper.GetRandomStock("R");
            entity.OrderTime = DateTime.Now;
            entity.Type = 2;
            entity.Class = 2;
            entity.UserID = int.Parse(UserID);
            entity.VerifyTime = DateTime.Now;//审核时间(现在没有什么用)
            entity.AllMoney = getPrice(NumberList, IDlist, out Allquatity);
            entity.AllQuatity = Allquatity;
            entity.AllChengBen = GetChengBen(NumberList, IDlist);


            var result = iwareBLL.AddStockOrder(entity);
            #endregion

            if (result > 0)
            {
                #region ==添加入库详情==
                StockOrderInfo info = new StockOrderInfo();
                info.StockOrderNo = entity.OrderNo;
                for (int i = 0; i < IDlist.Length; i++)
                {


                    info.ProductID = Convert.ToInt32(IDlist[i]);
                    var productEntity = baseinfoBLL.GetModel(info.ProductID);
                    if (productEntity.HospitalID != LoginUserEntity.ParentHospitalID)
                    {
                        return Json(-1);
                    }
                    info.Number = Convert.ToInt32(NumberList[i]);
                    info.HouseID = Convert.ToInt32(HouseID);
                    iwareBLL.AddStockOrderInfo(info);
                    //#region ==保存到库存==
                    ////先去库存里面找这个是否存在
                    //ProductStockEntity pse = new ProductStockEntity();
                    //pse.HospitalID = loginUserEntity.HospitalID;
                    //pse.ProductID = info.ProductID;
                    //pse.HouseID = Convert.ToInt32(HouseID);
                    //var stocklist = iwareBLL.GetProductStockListToseleect(pse);
                    //if (stocklist.Count == 0) //如果没有找到数据,表示原来没有库存,需要新增进去
                    //{
                    //    pse.Quatity = info.Number;
                    //    iwareBLL.AddProductStock(pse);//添加库存
                    //}
                    //else//修改库存
                    //{
                    //    pse.ID = stocklist[0].ID;
                    //    pse.Quatity = info.Number + stocklist[0].Quatity;
                    //    iwareBLL.UpdateProductStock(pse);
                    //}
                    //#endregion
                }

                #endregion
            }
            return Json(result);
        }

        public ActionResult RukuList(string StartTime = "", string EndTime = "", int TClass = 0, int IsVerify = 999, int pageNum = 1, int HospitalID = 0, int HouseID = 0)
        {
            StockOrderEntity entity = new StockOrderEntity();
            //entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01"));
            //entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            //默认查询待审核单据
            if (IsVerify == 999)
            {
                IsVerify = 1;
            }
            entity.IsVerify = IsVerify;
            entity.pageNum = pageNum;
            entity.Class = TClass;
            entity.StartTime = StartTime == "" ? Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(Convert.ToDateTime(StartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = EndTime == "" ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(Convert.ToDateTime(EndTime).ToString("yyyy-MM-dd 23:59:59"));

            if (!String.IsNullOrEmpty(entity.ProductName))
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            int rows;
            int pagecount;
            //传入仓库ID时通过仓库ID找到对应门店ID方法.现在暂时不改
            //HospitalID = GetHouserIDByHospitalID(HouseID);
            //if (HospitalID == -1)
            //{
            //    return Json(-3);
            //}
            entity.HospitalID = HospitalID == 0 ? LoginUserEntity.HospitalID : HospitalID;

            entity.Type = 2;
            //entity.Class = 2;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            entity.orderDirection = "desc";
            var list = iwareBLL.GetStockOrderList(entity, out rows, out pagecount);


            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            var sum = iwareBLL.getSumStock(entity);
            ViewBag.AllPrice = sum.AllMoney;
            ViewBag.AllNumber = sum.AllQuatity;
            ViewBag.allchengben = sum.AllChengBen;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            ViewBag.UserID = LoginUserEntity.UserID;

            ViewBag.Version = LoginUserEntity.Version; 
            ViewBag.LoginAccount = LoginUserEntity.LoginAccount;

            ViewBag.IsVerify = IsVerify;
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_RukuList", result);
            return View(result);
            //  return View();
        }

        /// <summary>
        /// 获取总成本
        /// </summary>
        /// <param name="NumList"></param>
        /// <param name="IDList"></param>
        /// <returns></returns>
        public decimal GetChengBen(string[] NumList, string[] IDList)
        {
            decimal allprice = 0;
            for (int i = 0; i < IDList.Length; i++)
            {
                var pmodel = baseinfoBLL.GetModel(Convert.ToInt32(IDList[i]));
                if (pmodel != null)
                {
                    allprice = allprice + pmodel.CostPrice * Convert.ToInt32(NumList[i]);
                }
            }
            return allprice;
        }

        /// <summary>
        /// 获取出库成本总额
        /// </summary>
        /// <param name="NumList"></param>
        /// <param name="IDList">库存ID</param>
        /// <returns></returns>
        public decimal GetChukuChengBen(string[] NumList, string[] IDList)
        {
            decimal allprice = 0;
            for (int i = 0; i < IDList.Length; i++)
            {
                var kucunmodel = iwareBLL.GetProductStockModel(new ProductStockEntity { ID = Convert.ToInt32(IDList[i]) });
                var pmodel = baseinfoBLL.GetModel(kucunmodel.ProductID);
                if (pmodel != null)
                {
                    allprice = allprice + pmodel.CostPrice * Convert.ToInt32(NumList[i]);
                }
            }
            return allprice;
        }

        /// <summary>
        /// 通过仓库ID获取门店ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private int GetHouserIDByHospitalID(int ID)
        {
            var warehouse = iwareBLL.GetWarehouseModel(ID);
            if (warehouse != null)
            {
                return warehouse.HospitalID;
            }
            else
            {
                return -1;
            }

        }

        public ActionResult _RukuList()
        {
            return View();
        }

        /// <summary>
        /// 入库审核
        /// </summary>
        /// <returns></returns>
        public ActionResult RukuXamine()
        {
            var orderno = Request["OrderNo"];
            var infolist = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = orderno });
            var model = iwareBLL.GetStockOrderModel(new StockOrderEntity { OrderNo = orderno });
            if (model.IsVerify != 1)
            {
                return Json(-1);
            }
            ProductStockEntity pse = new ProductStockEntity();
            pse.HospitalID = LoginUserEntity.HospitalID;
            var result = 0;
            foreach (var info in infolist)
            {
                pse.HouseID = info.HouseID;
                pse.ProductID = info.ProductID;

                if (iwareBLL.GetProductStockListToseleect(pse).Count == 0)
                {
                    StockChangeLogEntity scl = new StockChangeLogEntity();
                    scl.UserID = LoginUserEntity.UserID;
                    scl.LogClass = 5;
                    scl.LogType = "入库";
                    scl.OwnedWarehouse = 0;//原来所在仓库为0
                    scl.IntoWarehouse = pse.HouseID;//调往仓库为自己当前仓库
                    scl.ProductID = info.ProductID;
                    scl.LogTitle = "正常入库单审核(原来仓库无此产品记录.).仓库ID" + pse.HouseID + ";产品ID:" + info.ProductID + ";入库数量:" + info.Number + ";";
                    scl.NewQuatity = info.Number;//当原来仓库没此产品记录时,自动添加一个记录
                    scl.OldQuatity = 0;
                    scl.Quantity = info.Number;
                    scl.ProductStockID = 0;
                    scl.OrderNo = orderno;
                    var sclID = iwareBLL.AddStockChangeLog(scl);

                    pse.Quatity = info.Number;
                    result = Convert.ToInt32(iwareBLL.AddProductStock(pse));

                }
                else
                {

                    pse = iwareBLL.GetProductStockListToseleect(pse)[0];
                    StockChangeLogEntity scl = new StockChangeLogEntity();
                    scl.UserID = LoginUserEntity.UserID;
                    scl.LogClass = 5;
                    scl.LogType = "入库";
                    scl.OwnedWarehouse = 0;//原来所在仓库为0
                    scl.IntoWarehouse = pse.HouseID;//调往仓库为自己当前仓库
                    scl.ProductID = info.ProductID;
                    scl.LogTitle = "正常入库单审核(仓库有库存).仓库ID" + pse.HouseID + "; 产品ID:" + info.ProductID + ";入库数量:" + info.Number + ";";
                    scl.NewQuatity = info.Number + pse.Quatity;
                    scl.OldQuatity = pse.Quatity;
                    scl.Quantity = info.Number;
                    scl.ProductStockID = pse.ID;
                    scl.OrderNo = orderno;
                    var sclID = iwareBLL.AddStockChangeLog(scl);

                    pse.Quatity = pse.Quatity + info.Number;
                    result = iwareBLL.UpdateProductStock(pse);
                    if (result > 0)
                    {
                        iwareBLL.UpdateStatu(sclID);
                    }
                }

                //pse = iwareBLL.GetProductStockListToseleect(pse)[0];
                //pse.Quatity = pse.Quatity + info.Number;
                //iwareBLL.UpdateProductStock(pse);
                result = 1;
            }
            //修改状态
            if (result > 0)
            {
                StockOrderEntity soe = new StockOrderEntity();
                soe.OrderNo = orderno;
                soe.IsVerify = 2;
                soe.DocumentNumber = GetDocumentNumber(model.HospitalID, 2);
                iwareBLL.UpdateStockOrderVerify(soe);
                #region ==添加库存动态==
                CompanyNewsEntity cust = new CompanyNewsEntity();
                cust.HospitalID = pse.HospitalID;
                cust.CreateTime = DateTime.Now;
                cust.HuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.NextHuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.Type = 4;
                var hospuser = PersBLL.GetUserByUserID(model.UserID);
                cust.UserName = hospuser.UserName;
                cust.Name = cust.UserName + "进行产品入库";
                cust.ProjectName = orderno;
                iCentBLL.AddCompanyNewsEntity(cust);
                #endregion

            }
            return Json(result);
        }

        public ActionResult OrderInfo()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            var ID = Request["ID"];
            var model = iwareBLL.GetStockOrderModel(new StockOrderEntity { ID = Convert.ToInt32(ID) });
            var list = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = model.OrderNo });
            var result = list.ToPagedList(1, 100000);//添加Webdiyer.WebControls.Mvc应用
            return View(result);
        }

        public ActionResult YufuOrderInfo()
        {
            var ID = Request["ID"];
            var model = iwareBLL.GetStockOrderModel(new StockOrderEntity { ID = Convert.ToInt32(ID) });
            var list = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = model.OrderNo });
            var result = list.AsQueryable().ToPagedList(1, 100000);//添加Webdiyer.WebControls.Mvc应用
            return View(result);
        }

        public ActionResult RukuEdit()
        {
            var ID = Convert.ToInt32(Request["ID"]);
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var model = iwareBLL.GetStockOrderModel(new StockOrderEntity { ID = ID });
            var list = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = model.OrderNo });
            var result = list.AsQueryable().ToPagedList(1, 100000);//添加Webdiyer.WebControls.Mvc应用
            return View(result);
        }

        public ActionResult RditRuku()
        {
            int Allquatity = 0;
            string[] IDlist = Request.Form.GetValues("ID");
            string[] NumberList = Request.Form.GetValues("Number");
            string HouseID = Request["HouseID"];//仓库
            string UserID = Request["UserID"];//仓库
            string BaseID = Request["BaseID"];//操作原因
            string Memo = Request["Memo"];//备注
            string Orderno = Request["OrderNo"];//单据编号
            #region ==入库==
            StockOrderEntity entity = new StockOrderEntity();
            entity.BaseID = int.Parse(BaseID);
            entity.Memo = Memo;
            entity.OrderNo = Orderno;
            entity.UserID = int.Parse(UserID);
            entity.AllMoney = getPrice(NumberList, IDlist, out Allquatity);
            entity.AllQuatity = Allquatity;

            var result = iwareBLL.UpdateStockOrder(entity);
            #endregion
            //删除原来的单据详情,再添加


            if (result > 0)
            {
                //删除原有的单据详情
                iwareBLL.DelStockOrderInfoByOrderNo(Orderno);

                #region ==添加入库详情==
                StockOrderInfo info = new StockOrderInfo();
                info.StockOrderNo = entity.OrderNo;
                for (int i = 0; i < IDlist.Length; i++)
                {
                    info.ProductID = Convert.ToInt32(IDlist[i]);
                    info.Number = Convert.ToInt32(NumberList[i]);
                    info.HouseID = Convert.ToInt32(HouseID);
                    iwareBLL.AddStockOrderInfo(info);
                }

                #endregion
            }
            return Json(result);
        }

        public ActionResult ChexiaoOrder()
        {
            var id = Convert.ToInt32(Request["ID"]);
            return Json(iwareBLL.DelStockOrder(id));
        }


        #endregion

        #region ==出库==
        /// <summary>
        /// 出库页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Chuku()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        ///  库存列表
        /// </summary>
        /// <returns></returns>
        public ActionResult _KucunList()
        {
            return View();
        }

        /// <summary>
        /// 库存列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult KucunList(ProductStockEntity entity)
        {
            if (entity.StartTime.Year < 1980) { entity.StartTime = DateTime.Now.AddYears(-10); }
            if (entity.EndTime.Year < 1980) { entity.EndTime = DateTime.Now; }
            if (!String.IsNullOrEmpty(entity.ProductName))
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            int rows;
            int pagecount;
            if (entity.HouseID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }

            entity.orderDirection = "desc";
            entity.numPerPage = 200; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProductName"; }
            var list = iwareBLL.GetProductStockList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_KucunList", result);
            return View(result);
        }

        /// <summary>
        /// 出库操作
        /// </summary>
        /// <returns></returns>
        public ActionResult AddChuku()
        {
            string[] IDlist = Request.Form.GetValues("ID");  //产品库存表的ID
            string[] NumberList = Request.Form.GetValues("Number"); //数量
            string UserID = Request["UserID"];//用户
            string BaseID = Request["BaseID"];//操作原因
            string Memo = Request["Memo"];//备注

            int Allquatity = 0;
            int _allquatity = 0;

            #region ==生成出库单==
            StockOrderEntity entity = new StockOrderEntity();
            entity.BaseID = int.Parse(BaseID);
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.IsVerify = 1;
            entity.Memo = Memo;
            entity.OrderNo = RandomHelper.GetRandomStock("C");
            entity.OrderTime = DateTime.Now;
            entity.Type = 1;
            entity.Class = 1;
            entity.UserID = int.Parse(UserID);
            entity.VerifyTime = DateTime.Now;//审核时间(现在没有什么用)
            entity.AllMoney = GetAllPrice(NumberList, IDlist, out Allquatity);
            entity.AllQuatity = Allquatity;
            entity.AllChengBen = GetChukuChengBen(NumberList, IDlist);
            var result = iwareBLL.AddStockOrder(entity);
            #endregion
            if (result > 0)
            {
                #region ==添加出库详情==
                StockOrderInfo info = new StockOrderInfo();
                info.StockOrderNo = entity.OrderNo;
                for (int i = 0; i < IDlist.Length; i++)
                {
                    #region 根据ID获取这条库存的详细信息
                    ProductStockEntity p = new ProductStockEntity();
                    p.ID = Convert.ToInt32(IDlist[i]);
                    p = iwareBLL.GetProductStockModel(p);
                    #endregion

                    info.ProductID = p.ProductID;
                    var productEntity = baseinfoBLL.GetModel(info.ProductID);
                    if (productEntity.HospitalID != LoginUserEntity.ParentHospitalID)
                    {
                        return Json(-1);
                    }
                    info.Number = Convert.ToInt32(NumberList[i]);
                    info.HouseID = p.HouseID; //仓库ID需要从后台关联出来
                    var res = iwareBLL.AddStockOrderInfo(info);
                    if (res > 0)
                    {
                        _allquatity += info.Number;
                    }

                    //}
                }
                #endregion
            }
            return Json(result);
        }

        /// <summary>
        /// 出库审核
        /// </summary>
        /// <returns></returns>
        public ActionResult ChukuXamine()
        {
            var orderno = Request["OrderNo"];
            var OperateType = Request["OperateType"];//院用出库或者院用消耗，生成虚拟仓库库存
            var infolist = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = orderno });
            var model = iwareBLL.GetStockOrderModel(new StockOrderEntity { OrderNo = orderno });
            if (model.IsVerify != 1)
            {
                return Json(-1);
            }
            //审核操作只能本店的人员审核
            ProductStockEntity pse = new ProductStockEntity();
            pse.HospitalID = LoginUserEntity.HospitalID;
            var result = 0;
            if (QuatityEnough(infolist))
            {
                foreach (var info in infolist)
                {
                    pse.HouseID = info.HouseID;
                    pse.ProductID = info.ProductID;
                    pse = iwareBLL.GetProductStockListToseleect(pse)[0];

                    StockChangeLogEntity scl = new StockChangeLogEntity();
                    scl.UserID = LoginUserEntity.UserID;
                    scl.LogClass = 6;
                    scl.OwnedWarehouse = pse.HouseID;
                    scl.IntoWarehouse = 0;
                    scl.ProductID = info.ProductID;
                    scl.LogType = "出库";
                    scl.LogTitle = "正常出库单审核.仓库ID:" + info.HouseID + "; 产品ID:" + info.ProductID + ";出库数量:" + info.Number + ";";
                    scl.NewQuatity = pse.Quatity - info.Number;
                    scl.OldQuatity = pse.Quatity;
                    scl.Quantity = info.Number;
                    scl.ProductStockID = pse.ID;
                    scl.OrderNo = orderno;
                    var sclID = iwareBLL.AddStockChangeLog(scl);

                    pse.Quatity = pse.Quatity - info.Number;
                    result = iwareBLL.UpdateProductStock(pse);
                    //增加虚拟库存入库，院用出库
                    if (!String.IsNullOrEmpty(OperateType) && OperateType.Contains("院用"))
                    {
                        var AllUnit = (info.Number * info.UseUnit);
                        iwareBLL.AddProductXuniStock(new ProductXuniStockEntity { HospitalID = LoginUserEntity.HospitalID, ProductID = info.ProductID, AllUnit = AllUnit });
                    }
                    if (result > 0)
                    {
                        int len = 2;
                        var usersetting = iSysBLL.GetUserSettingsValue(LoginUserEntity.HospitalID, "StockNumber");
                        if (usersetting != null)
                        {
                            try
                            {
                                if (usersetting.AttributeValue != null)
                                {
                                    len = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue);
                                }
                                else
                                {
                                    len = 2;
                                }
                            }
                            catch (Exception)
                            {
                                len = 2;
                            }
                        }

                        if (pse.Quatity <= len)
                        {
                            var ProductModel = baseinfoBLL.GetModel(info.ProductID);

                            TaskEntity te = new TaskEntity();
                            te.HospitalID = LoginUserEntity.HospitalID;
                            te.CreateTime = DateTime.Now;
                            te.Type = 4;
                            te.Statu = 1;
                            te.Name = LoginUserEntity.Name + "库存不足,请入库";
                            te.Content = "产品:" + ProductModel.Name + "库存为:" + pse.Quatity;
                            iCentBLL.AddTaskEntity(te);
                        }
                        iwareBLL.UpdateStatu(sclID);
                    }

                }
            }
            else
            {
                result = -1;
            }


            //修改状态
            if (result > 0)
            {
                StockOrderEntity soe = new StockOrderEntity();
                soe.OrderNo = orderno;
                soe.IsVerify = 2;
                soe.DocumentNumber = GetDocumentNumber(LoginUserEntity.HospitalID, 1);
                iwareBLL.UpdateStockOrderVerify(soe);
                #region ==添加库存动态==
                CompanyNewsEntity cust = new CompanyNewsEntity();
                cust.HospitalID = pse.HospitalID;
                cust.CreateTime = DateTime.Now;
                cust.HuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.NextHuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.Type = 4;
                //var hospuser = PersBLL.GetUserByUserID(model.UserID);
                cust.UserName = model.UserName;
                cust.Name = cust.UserName + "进行产品出库";
                cust.ProjectName = orderno;
                iCentBLL.AddCompanyNewsEntity(cust);
                #endregion

            }
            return Json(result);
        }

        /// <summary>
        /// 判断仓库库存是否足够
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool QuatityEnough(IList<StockOrderInfo> list)
        {
            ProductStockEntity pse = new ProductStockEntity();
            pse.HospitalID = LoginUserEntity.HospitalID;
            foreach (var info in list)
            {
                pse.HouseID = info.HouseID;
                pse.ProductID = info.ProductID;
                pse = iwareBLL.GetProductStockListToseleect(pse)[0];
                if (pse.Quatity < info.Number)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// 获取总价 (IDlist 传入的是库存ID)
        /// </summary>
        /// <param name="NumberList"></param>
        /// <param name="IDlist"></param>
        /// <returns></returns>
        public Decimal GetAllPrice(string[] NumberList, string[] IDlist, out int Allquatity)
        {
            decimal allprice = 0;
            int quat = 0;
            for (int i = 0; i < IDlist.Length; i++)
            {
                ProductStockEntity p = new ProductStockEntity();
                p.ID = Convert.ToInt32(IDlist[i]);
                p = iwareBLL.GetProductStockModel(p);

                //通过库存id获取产品ID
                var kucun = iwareBLL.GetProductStockModel(new ProductStockEntity { ID = Convert.ToInt32(IDlist[i]) });
                //通过库存的ID获取产品ID
                var pmodel = baseinfoBLL.GetModel(kucun.ProductID);
                if (pmodel != null)
                {
                    allprice = allprice + pmodel.RetailPrice * Convert.ToInt32(NumberList[i]);
                    quat = quat + Convert.ToInt32(NumberList[i]);
                }

            }
            Allquatity = quat;
            return allprice;
        }

        /// <summary>
        /// 获取总价(这个方法IDlist传入的是产品名称)
        /// </summary>
        /// <param name="NumberList"></param>
        /// <param name="IDlist"></param>
        /// <param name="Allquatity"></param>
        /// <returns></returns>
        public decimal getPrice(string[] NumberList, string[] IDlist, out int Allquatity)
        {
            decimal allprice = 0;
            int quat = 0;
            for (int i = 0; i < IDlist.Length; i++)
            {
                var pmodel = baseinfoBLL.GetModel(Convert.ToInt32(IDlist[i]));
                if (pmodel != null)
                {
                    allprice = allprice + pmodel.RetailPrice * Convert.ToInt32(NumberList[i]);
                }
                quat = quat + Convert.ToInt32(NumberList[i]);
            }
            Allquatity = quat;
            return allprice;
        }


        public ActionResult ChukuList(int TClass = 0, int IsVerify = 999, string StartTime = "", string EndTime = "", int pageNum = 1, int HospitalID = 0)
        {
            StockOrderEntity entity = new StockOrderEntity();
            entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            //默认查询待审核单据
            if (IsVerify == 999)
            {
                IsVerify = 1;
            }
            entity.IsVerify = IsVerify;
            entity.pageNum = pageNum;
            entity.Class = TClass;
            if (StartTime != "")
            {
                entity.StartTime = Convert.ToDateTime(StartTime);
            }
            if (EndTime != "")
            {
                entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndTime).Year + "-" + Convert.ToDateTime(EndTime).Month + "-" + Convert.ToDateTime(EndTime).Day + " 23:59:59");
            }
            if (!String.IsNullOrEmpty(entity.ProductName))
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            int rows;
            int pagecount;
            entity.HospitalID = HospitalID == 0 ? LoginUserEntity.HospitalID : HospitalID;

            entity.Type = 1;
            //  entity.Class = 1;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            entity.orderDirection = "desc";
            var list = iwareBLL.GetStockOrderList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            var sum = iwareBLL.getSumStock(entity);
            ViewBag.AllPrice = sum.AllMoney;
            ViewBag.AllNumber = sum.AllQuatity;
            ViewBag.allchengben = sum.AllChengBen;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.IsVerify = IsVerify;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;

            ViewBag.UserID = LoginUserEntity.UserID;
            ViewBag.LoginAccount = LoginUserEntity.LoginAccount;

            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_ChukuList", result);
            return View(result);
        }


        public ActionResult _ChukuList()
        {
            return View();
        }

        public ActionResult ChukuEdit()
        {
            var ID = Convert.ToInt32(Request["ID"]);

            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var model = iwareBLL.GetStockOrderModel(new StockOrderEntity { ID = ID });
            var list = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = model.OrderNo });
            foreach (var info in list)
            {
                ProductStockEntity pse = new ProductStockEntity();
                pse.HouseID = info.HouseID;
                pse.ProductID = info.ProductID;
                pse.HospitalID = LoginUserEntity.HospitalID;
                var kucunlist = iwareBLL.GetProductStockListToseleect(pse);
                if (kucunlist.Count > 0)     //如果能找到库存,则找到这个库存
                {
                    info.KucunID = kucunlist[0].ID;
                    info.Kucun = kucunlist[0].Quatity;
                }
            }

            var result = list.AsQueryable().ToPagedList(1, 100000);//添加Webdiyer.WebControls.Mvc应用
            return View(result);

        }

        public ActionResult EditChuku()
        {
            string[] IDlist = Request.Form.GetValues("ID");  //产品库存表的ID
            string[] NumberList = Request.Form.GetValues("Number"); //数量
            string UserID = Request["UserID"];//用户
            string BaseID = Request["BaseID"];//操作原因
            string Memo = Request["Memo"];//备注
            string OrderNo = Request["OrderNo"];

            int Allquatity = 0;

            #region ==生成出库单==
            StockOrderEntity entity = new StockOrderEntity();
            entity.BaseID = int.Parse(BaseID);
            entity.Memo = Memo;
            entity.OrderNo = OrderNo;
            entity.OrderTime = DateTime.Now;
            entity.UserID = int.Parse(UserID);
            entity.AllMoney = GetAllPrice(NumberList, IDlist, out Allquatity);
            entity.AllQuatity = Allquatity;
            var result = iwareBLL.UpdateStockOrder(entity);
            #endregion
            if (result > 0)
            {
                //删除原有的单据详情
                iwareBLL.DelStockOrderInfoByOrderNo(OrderNo);
                #region ==添加出库详情==
                StockOrderInfo info = new StockOrderInfo();
                info.StockOrderNo = entity.OrderNo;
                for (int i = 0; i < IDlist.Length; i++)
                {
                    #region 根据ID获取这条库存的详细信息
                    ProductStockEntity p = new ProductStockEntity();
                    p.ID = Convert.ToInt32(IDlist[i]);
                    p = iwareBLL.GetProductStockModel(p);
                    #endregion
                    //if (p.Quatity >= info.Number)
                    //{
                    info.ProductID = p.ProductID;
                    info.Number = Convert.ToInt32(NumberList[i]);
                    info.HouseID = p.HouseID; //仓库ID需要从后台关联出来
                    iwareBLL.AddStockOrderInfo(info);
                    //}
                }
                #endregion
            }
            return Json(result);
        }


        #endregion

        #region ==调拨==
        /// <summary>
        /// 调拨页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Diaobo()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        /// <summary>
        /// 调拨操作
        /// </summary>
        /// <returns></returns>
        public ActionResult AddDiaobo()
        {
            //调拨思路:先添加一个调拨详情.然后把这个调拨的数据调取出来处理,第一次处理其实就是出库.
            //出库完成后再把它入库到对应的仓库
            string[] IDlist = Request.Form.GetValues("ID");  //产品库存表的ID
            string[] NumberList = Request.Form.GetValues("Number"); //数量
            string Cangku = Request["HouseID"]; //仓库
            string UserID = Request["UserID"];//用户
            string BaseID = Request["BaseID"];//操作原因
            string Memo = Request["Memo"];//备注

            //通过调拨到的仓库找到调拨门店
            var model = iwareBLL.GetWarehouseModel(Convert.ToInt32(Cangku));
            if (model == null)
            {
                return Json(0);
            }
            else if (model.HospitalID == 0)
            {
                return Json(0);
            }


            #region ==生成调拨单==
            int allquatity = 0;
            StockOrderEntity entity = new StockOrderEntity();
            entity.BaseID = int.Parse(BaseID);
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.IsVerify = 1;//现在默认是通过的
            entity.Memo = Memo;
            entity.OrderNo = RandomHelper.GetRandomStock("D");
            entity.OrderTime = DateTime.Now;
            entity.Type = 3;
            entity.UserID = int.Parse(UserID);
            entity.VerifyTime = DateTime.Now;//审核时间(现在没有什么用)
            entity.AllMoney = GetAllPrice(NumberList, IDlist, out allquatity);
            entity.ShowDocumentNumber = DateTime.Today.ToString("MMdd") + (iwareBLL.GetTodayStockOrderNumber(entity.HospitalID, entity.Type, 0) + 1).ToString().PadLeft(3, '0'); ;
            entity.AllQuatity = allquatity;
            entity.DiaoHospitalID = model.HospitalID;
            LogWriter.WriteInfo("当前门店ID：" + LoginUserEntity.HospitalID + "，调出仓库ID：" + Cangku + " 调出门店ID:" + model.HospitalID, "调出仓库ID");
            var result = iwareBLL.AddStockOrder(entity);
            if (result > 0)
            {
                #region ==添加调拨(调出)详情==

                StockOrderInfo info = new StockOrderInfo();
                info.StockOrderNo = entity.OrderNo;
                for (int i = 0; i < IDlist.Length; i++)
                {
                    #region 根据ID获取这条库存的详细信息
                    ProductStockEntity p = new ProductStockEntity();
                    p.ID = Convert.ToInt32(IDlist[i]);
                    p = iwareBLL.GetProductStockModel(p);
                    #endregion

                    info.ProductID = p.ProductID;
                    var productEntity = baseinfoBLL.GetModel(info.ProductID);
                    if (productEntity.HospitalID != LoginUserEntity.ParentHospitalID)
                    {
                        return Json(-1);
                    }
                    info.Number = Convert.ToInt32(NumberList[i]);
                    info.HouseID = p.HouseID; //仓库ID需要从后台关联出来
                    info.TransferHouseID = Convert.ToInt32(Cangku);
                    iwareBLL.AddStockOrderInfo(info);
                }
                #endregion
            }

            #endregion

            return Json(result);
        }

        public ActionResult DiaoBoList(int IsVerify = 999, string StartTime = "", string EndTime = "", int pageNum = 1, int HospitalID = 0)
        {
            StockOrderEntity entity = new StockOrderEntity();
            entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));

            //默认查询未审核单据
            if (IsVerify == 999)
            {
                IsVerify = 1;
            }
            entity.IsVerify = IsVerify;
            entity.pageNum = pageNum;
            if (StartTime != "")
            {
                entity.StartTime = Convert.ToDateTime(StartTime);
            }
            if (EndTime != "")
            {
                entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndTime).Year + "-" + Convert.ToDateTime(EndTime).Month + "-" + Convert.ToDateTime(EndTime).Day + " 23:59:59");
            }
            int rows;
            int pagecount;
            entity.HospitalID = HospitalID == 0 ? LoginUserEntity.HospitalID : HospitalID;

            entity.Type = 3;
            entity.numPerPage = 50; //每页显示条数
            entity.orderDirection = "desc";
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = iwareBLL.GetStockOrderList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            var sum = iwareBLL.getSumStock(entity);
            ViewBag.AllPrice = sum.AllMoney;
            ViewBag.AllNumber = sum.AllQuatity;
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.Name = entity.ProductName;
            ViewBag.HouseID = entity.HouseID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            ViewBag.IsVerify = IsVerify;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (Request.IsAjaxRequest())
            {
                ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                ViewBag.HospitalID = LoginUserEntity.HospitalID;
                return PartialView("_DiaoboList", result);
            }
            return View(result);
        }

        public ActionResult _DiaoboList()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }

        /// <summary>
        /// 调拨审核
        /// </summary>
        /// <returns></returns>
        public ActionResult DiaoboXamine()
        {
            var orderno = Request["OrderNo"];
            //var model = iwareBLL.GetStockOrderModel(new StockOrderEntity { OrderNo = orderno });

            var infolist = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = orderno });

            var order = iwareBLL.GetStockOrderModel(new StockOrderEntity { OrderNo = orderno });

            if (order.IsVerify != 4)
            {
                return Json(-2);
            }
            List<int> ProductIDlist = new List<int>();
            List<int> Quatity = new List<int>();
            foreach (var il in infolist)
            {
                ProductIDlist.Add(il.ProductID);
                Quatity.Add(il.Number);
            }

            StockOrderEntity soe = new StockOrderEntity();

            //if (QuatityEnoughDiaoBo(infolist, order.HospitalID))
            if (true)
            {



                #region 添加出库单和出库单详情和修改库存
                ////出库单是调拨单发起人出库,故用订单的门店ID
                //soe.BaseID = 0;
                //soe.OrderNo = RandomHelper.GetRandomStock("C");
                //soe.Class = 4;
                //soe.HospitalID = order.HospitalID;
                //soe.IsVerify = 2;
                //soe.Memo = "调拨出库";
                //soe.OrderTime = DateTime.Now;
                //soe.UserID = loginUserEntity.UserID;
                //soe.VerifyMemo = orderno;
                //soe.VerifyTime = DateTime.Now;
                //soe.VerifyUserID = loginUserEntity.UserID;
                //soe.Type = 1;
                //soe.AllQuatity = infolist.Sum(i => i.Number);
                //soe.AllMoney = GetAllPriceByList(infolist.ToList());
                //iwareBLL.AddStockOrder(soe);    //添加出库单
                StockOrderInfo stoinfo = new StockOrderInfo();

                //stoinfo.StockOrderNo = soe.OrderNo;
                //foreach (var info in infolist)
                //{
                //    var results = 0;
                //    stoinfo.ProductID = info.ProductID;
                //    stoinfo.HouseID = info.HouseID;
                //    stoinfo.Number = info.Number;
                //    iwareBLL.AddStockOrderInfo(stoinfo);//添加出库详情

                //    //产品出库(线通过门店ID,仓库产品获取库存,然后修改库存后保存)
                //    ProductStockEntity pse = new ProductStockEntity();
                //    pse.HospitalID = order.HospitalID;// loginUserEntity.HospitalID; //order.DiaoHospitalID;
                //    pse.HouseID = info.HouseID;
                //    pse.ProductID = info.ProductID;
                //    pse = iwareBLL.GetProductStockListToseleect(pse)[0];

                //    StockChangeLogEntity scl = new StockChangeLogEntity();
                //    scl.UserID = loginUserEntity.UserID;
                //    scl.LogClass = 1;
                //    scl.LogType = "出库";
                //    scl.LogTitle = "调拨出库单审核.仓库ID:" + info.HouseID + "; 产品ID:" + info.ProductID + ";出库数量:" + info.Number + ";";
                //    scl.NewQuatity = pse.Quatity - info.Number;
                //    scl.OldQuatity = pse.Quatity;
                //    scl.Quantity = info.Number;
                //    scl.ProductStockID = pse.ID;
                //    scl.OrderNo = soe.OrderNo + "(调拨单:" + orderno + ")";
                //    var sclID = iwareBLL.AddStockChangeLog(scl);

                //    pse.Quatity = pse.Quatity - info.Number;
                //    results= iwareBLL.UpdateProductStock(pse);
                //    if (results > 0)
                //    {
                //        iwareBLL.UpdateStatu(sclID);
                //    }
                //}

                #endregion

                #region ==再添加入库单和入库详情以及修改库存==
                soe.OrderNo = RandomHelper.GetRandomStock("R");
                soe.Class = 3;
                soe.Memo = order.Memo;
                soe.Type = 2;
                soe.VerifyMemo = orderno;
                soe.HospitalID = order.DiaoHospitalID;//入库单是另外一个门店的入库单,所以应该用别的门店的ID
                soe.AllQuatity = infolist.Sum(i => i.Number);
                soe.AllMoney = GetAllPriceByList(infolist.ToList());
                soe.BaseID = 0;
                soe.IsVerify = 2;
                soe.OrderTime = DateTime.Now;
                soe.UserID = LoginUserEntity.UserID;
                soe.VerifyTime = DateTime.Now;
                soe.VerifyUserID = LoginUserEntity.UserID;
                int _quatity = 0;
                decimal _allprice = 0;
                soe.AllChengBen = GetAllPriceInPurchasing(ProductIDlist, Quatity, ref _quatity, ref _allprice);
                soe.DocumentNumber = GetDocumentNumber(order.DiaoHospitalID, soe.Type);


                iwareBLL.AddStockOrder(soe);    //添加入库单
                stoinfo.StockOrderNo = soe.OrderNo;
                foreach (var info in infolist)
                {
                    stoinfo.ProductID = info.ProductID;
                    stoinfo.HouseID = info.TransferHouseID;
                    stoinfo.Number = info.Number;
                    iwareBLL.AddStockOrderInfo(stoinfo);//添加入库详情

                    //产品入库(线通过门店ID,仓库产品获取库存,然后修改库存后保存)
                    ProductStockEntity pse = new ProductStockEntity();
                    pse.HospitalID = order.DiaoHospitalID;
                    pse.HouseID = info.TransferHouseID;
                    pse.ProductID = info.ProductID;
                    var results = 0;
                    if (iwareBLL.GetProductStockListToseleect(pse).Count == 0)
                    {
                        pse.Quatity = info.Number;
                        StockChangeLogEntity scl = new StockChangeLogEntity();
                        scl.UserID = LoginUserEntity.UserID;
                        scl.LogClass = 1;
                        scl.OwnedWarehouse = 0;
                        scl.IntoWarehouse = pse.HouseID;
                        scl.ProductID = info.ProductID;
                        scl.LogType = "入库";
                        scl.LogTitle = "调拨单审核入库(原本仓库无此产品纪录).仓库ID:" + info.TransferHouseID + "; 产品ID:" + info.ProductID + ";入库数量:" + info.Number + ";";
                        scl.NewQuatity = info.Number;
                        scl.OldQuatity = 0;
                        scl.Quantity = info.Number;
                        scl.ProductStockID = pse.ID;
                        scl.OrderNo = soe.OrderNo + "(调拨单:" + orderno + ")";
                        var sclID = iwareBLL.AddStockChangeLog(scl);

                        results = Convert.ToInt32(iwareBLL.AddProductStock(pse));
                        if (results > 0)
                        {
                            iwareBLL.UpdateStatuAndProductStockID(sclID, results);
                        }
                    }
                    else
                    {
                        pse = iwareBLL.GetProductStockListToseleect(pse)[0];
                        StockChangeLogEntity scl = new StockChangeLogEntity();
                        scl.UserID = LoginUserEntity.UserID;
                        scl.LogClass = 1;
                        scl.OwnedWarehouse = 0;
                        scl.IntoWarehouse = pse.HouseID;
                        scl.ProductID = info.ProductID;
                        scl.LogType = "入库";
                        scl.LogTitle = "调拨单审核入库.仓库ID:" + info.TransferHouseID + "; 产品ID:" + info.ProductID + ";出库数量:" + info.Number + ";";
                        scl.NewQuatity = pse.Quatity + info.Number;
                        scl.OldQuatity = pse.Quatity;
                        scl.Quantity = info.Number;
                        scl.ProductStockID = pse.ID;
                        scl.OrderNo = soe.OrderNo + "(调拨单:" + orderno + ")";
                        var sclID = iwareBLL.AddStockChangeLog(scl);

                        pse.Quatity = pse.Quatity + info.Number;
                        results = iwareBLL.UpdateProductStock(pse);
                        if (results > 0)
                        {
                            iwareBLL.UpdateStatu(sclID);
                        }
                    }

                }


                #endregion


                soe.OrderNo = orderno;
                soe.IsVerify = 2;
                soe.DocumentNumber = GetDocumentNumber(order.HospitalID, 3);
                var result = iwareBLL.UpdateStockOrderVerify(soe);

                #region ==添加库存动态==
                CompanyNewsEntity cust = new CompanyNewsEntity();
                cust.HospitalID = order.DiaoHospitalID;
                cust.CreateTime = DateTime.Now;
                cust.HuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.NextHuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.Type = 4;
                //var hospuser = PersBLL.GetUserByUserID(model.UserID);
                cust.UserName = LoginUserEntity.UserName;
                cust.Name = cust.UserName + "进行产品调拨确认";
                cust.ProjectName = orderno;
                iCentBLL.AddCompanyNewsEntity(cust);
                #endregion

                return Json(result);

            }
            return Json(-1);
        }

        /// <summary>
        /// 调拨确认
        /// </summary>
        /// <returns></returns>
        public ActionResult DiaoboConfirm()
        {
            var orderno = Request["OrderNo"];
            //var model = iwareBLL.GetCheckOrderModelByOrderNo(new CheckOrderEntity { OrderNo = orderno });

            var infolist = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = orderno });
            List<int> ProductIDlist = new List<int>();
            List<int> Quatity = new List<int>();
            foreach (var il in infolist)
            {
                ProductIDlist.Add(il.ProductID);
                Quatity.Add(il.Number);
            }
            //检查是否入库
            foreach (var model in infolist)
            {
                ProductStockEntity pse = new ProductStockEntity();
                pse.HospitalID = LoginUserEntity.HospitalID;
                pse.HouseID = model.HouseID;
                pse.ProductID = model.ProductID;
                var pselist = iwareBLL.GetProductStockListToseleect(pse);
                if (pselist.Count == 0)
                {
                    ViewBag.ProductName = model.ProductName;
                    return Json(-1);
                }
            }
            var order = iwareBLL.GetStockOrderModel(new StockOrderEntity { OrderNo = orderno });

            if (order.IsVerify != 1)
            {
                return Json(-2);
            }
            StockOrderEntity soe = new StockOrderEntity();

            if (QuatityEnoughDiaoBo(infolist, order.HospitalID))
            {
                //出库单是调拨单发起人出库,故用订单的门店ID
                soe.BaseID = 0;
                soe.OrderNo = RandomHelper.GetRandomStock("C");
                soe.Class = 4;
                soe.HospitalID = order.HospitalID;
                soe.IsVerify = 2;
                soe.Memo = order.Memo;
                soe.OrderTime = DateTime.Now;
                soe.UserID = LoginUserEntity.UserID;
                soe.VerifyMemo = orderno;
                soe.VerifyTime = DateTime.Now;
                soe.VerifyUserID = LoginUserEntity.UserID;
                soe.Type = 1;
                soe.AllQuatity = infolist.Sum(i => i.Number);
                soe.AllMoney = GetAllPriceByList(infolist.ToList());
                int _quatity = 0;
                decimal _allprice = 0;
                soe.AllChengBen = GetAllPriceInPurchasing(ProductIDlist, Quatity, ref _quatity, ref _allprice);
                soe.DocumentNumber = GetDocumentNumber(order.HospitalID, soe.Type);
                iwareBLL.AddStockOrder(soe);    //添加出库单
                StockOrderInfo stoinfo = new StockOrderInfo();

                stoinfo.StockOrderNo = soe.OrderNo;
                foreach (var info in infolist)
                {
                    var results = 0;
                    stoinfo.ProductID = info.ProductID;
                    stoinfo.HouseID = info.HouseID;
                    stoinfo.Number = info.Number;
                    iwareBLL.AddStockOrderInfo(stoinfo);//添加出库详情

                    //产品出库(线通过门店ID,仓库产品获取库存,然后修改库存后保存)
                    ProductStockEntity pse = new ProductStockEntity();
                    pse.HospitalID = order.HospitalID;// loginUserEntity.HospitalID; //order.DiaoHospitalID;
                    pse.HouseID = info.HouseID;
                    pse.ProductID = info.ProductID;
                    pse = iwareBLL.GetProductStockListToseleect(pse)[0];

                    StockChangeLogEntity scl = new StockChangeLogEntity();
                    scl.UserID = LoginUserEntity.UserID;
                    scl.LogClass = 1;
                    scl.OwnedWarehouse = pse.HouseID;
                    scl.IntoWarehouse = 0;
                    scl.ProductID = info.ProductID;
                    scl.LogType = "出库";
                    scl.LogTitle = "调拨出库单审核.仓库ID:" + info.HouseID + "; 产品ID:" + info.ProductID + ";出库数量:" + info.Number + ";";
                    scl.NewQuatity = pse.Quatity - info.Number;
                    scl.OldQuatity = pse.Quatity;
                    scl.Quantity = info.Number;
                    scl.ProductStockID = pse.ID;
                    scl.OrderNo = soe.OrderNo + "(调拨单:" + orderno + ")";
                    var sclID = iwareBLL.AddStockChangeLog(scl);

                    pse.Quatity = pse.Quatity - info.Number;
                    results = iwareBLL.UpdateProductStock(pse);
                    if (results > 0)
                    {
                        int len = 2;
                        var usersetting = iSysBLL.GetUserSettingsValue(LoginUserEntity.HospitalID, "StockNumber");
                        if (usersetting != null)
                        {
                            try
                            {
                                if (usersetting.AttributeValue != null)
                                {
                                    len = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue);
                                }
                                else
                                {
                                    len = 2;
                                }
                            }
                            catch (Exception)
                            {
                                len = 2;
                            }
                        }

                        if (pse.Quatity <= len)
                        {
                            var ProductModel = baseinfoBLL.GetModel(info.ProductID);
                            TaskEntity te = new TaskEntity();
                            te.HospitalID = LoginUserEntity.HospitalID;
                            te.CreateTime = DateTime.Now;
                            te.Type = 4;
                            te.Statu = 1;
                            te.Name = LoginUserEntity.Name + "库存不足,请入库";
                            te.Content = "产品:" + ProductModel.Name + "库存为:" + pse.Quatity;
                            iCentBLL.AddTaskEntity(te);
                        }

                        iwareBLL.UpdateStatu(sclID);
                    }
                }


                soe.OrderNo = orderno;
                soe.IsVerify = 4;
                soe.DocumentNumber = null;
                var result = iwareBLL.UpdateStockOrderVerify(soe);
                #region ==添加库存动态==
                CompanyNewsEntity cust = new CompanyNewsEntity();
                cust.HospitalID = order.HospitalID;
                cust.CreateTime = DateTime.Now;
                cust.HuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.NextHuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.Type = 4;
                //var hospuser = PersBLL.GetUserByUserID(model.UserID);
                cust.UserName = order.UserName;
                cust.Name = cust.UserName + "进行产品调拨";
                cust.ProjectName = orderno;
                iCentBLL.AddCompanyNewsEntity(cust);
                #endregion
            }
            else
            {
                return Json(-1);
            }

            return Json(1);
        }

        public bool QuatityEnoughDiaoBo(IList<StockOrderInfo> list, int HospitalID)
        {
            ProductStockEntity pse = new ProductStockEntity();
            pse.HospitalID = HospitalID;
            foreach (var info in list)
            {
                pse.HouseID = info.HouseID;
                pse.ProductID = info.ProductID;
                pse = iwareBLL.GetProductStockListToseleect(pse)[0];
                if (pse.Quatity < info.Number)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 通过列表获取总价 之List集合
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public Decimal GetAllPriceByList(List<StockOrderInfo> list)
        {
            decimal price = 0;
            foreach (var info in list)
            {
                //通过库存id获取产品ID
                // var kucun = iwareBLL.GetProductStockModel(new ProductStockEntity { ID = Convert.ToInt32(info.ID) });
                var pmodel = baseinfoBLL.GetModel(info.ProductID);
                price = price + pmodel.RetailPrice * info.Number;
            }

            return price;
        }


        public ActionResult DiaoboEdit()
        {
            var ID = Convert.ToInt32(Request["ID"]);

            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var model = iwareBLL.GetStockOrderModel(new StockOrderEntity { ID = ID });
            var list = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = model.OrderNo });
            foreach (var info in list)
            {
                ProductStockEntity pse = new ProductStockEntity();
                pse.HouseID = info.HouseID;
                pse.ProductID = info.ProductID;
                pse.HospitalID = LoginUserEntity.HospitalID;
                var kucunlist = iwareBLL.GetProductStockListToseleect(pse);
                if (kucunlist.Count > 0)     //如果能找到库存,则找到这个库存
                {
                    info.KucunID = kucunlist[0].ID;
                    info.Kucun = kucunlist[0].Quatity;
                }
            }

            var result = list.AsQueryable().ToPagedList(1, 100000);//添加Webdiyer.WebControls.Mvc应用
            return View(result);
        }

        public ActionResult EditDiaobo()
        {
            //调拨思路:先添加一个调拨详情.然后把这个调拨的数据调取出来处理,第一次处理其实就是出库.
            //出库完成后再把它入库到对应的仓库
            string[] IDlist = Request.Form.GetValues("ID");  //产品库存表的ID
            string[] NumberList = Request.Form.GetValues("Number"); //数量
            string[] CangkuList = Request.Form.GetValues("HouseID"); //仓库列表
            string UserID = Request["UserID"];//用户
            string BaseID = Request["BaseID"];//操作原因
            string Memo = Request["Memo"];//备注
            string OrderNo = Request["OrderNo"];
            #region ==生成调拨单==
            int allquatity = 0;
            StockOrderEntity entity = new StockOrderEntity();
            entity.BaseID = int.Parse(BaseID);
            entity.Memo = Memo;
            entity.OrderNo = OrderNo;
            entity.UserID = int.Parse(UserID);
            entity.AllMoney = GetAllPrice(NumberList, IDlist, out allquatity);
            entity.AllQuatity = allquatity;
            var result = iwareBLL.UpdateStockOrder(entity);
            if (result > 0)
            {
                //删除原有的单据详情
                iwareBLL.DelStockOrderInfoByOrderNo(OrderNo);
                #region ==添加调拨(调出)详情==
                StockOrderInfo info = new StockOrderInfo();
                info.StockOrderNo = entity.OrderNo;
                for (int i = 0; i < IDlist.Length; i++)
                {
                    #region 根据ID获取这条库存的详细信息
                    ProductStockEntity p = new ProductStockEntity();
                    p.ID = Convert.ToInt32(IDlist[i]);
                    p = iwareBLL.GetProductStockModel(p);
                    #endregion

                    info.ProductID = p.ProductID;
                    info.Number = Convert.ToInt32(NumberList[i]);
                    info.HouseID = p.HouseID; //仓库ID需要从后台关联出来
                    info.TransferHouseID = Convert.ToInt32(CangkuList[i]);
                    iwareBLL.AddStockOrderInfo(info);
                }
                #endregion
            }

            #endregion


            return Json(result);
        }

        /// <summary>
        /// 确认的单据取消调拨(一般这个时候需要把自己这边的数量补回去)
        /// 还没做完
        /// </summary>
        /// <returns></returns>
        public ActionResult DiaoBocancel()
        {
            var id = Convert.ToInt32(Request["ID"]);
            var model = iwareBLL.GetStockOrderModel(new StockOrderEntity { ID = id });

            var result = -1;
            if (model != null)
            {
                var list = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = model.OrderNo });
                List<int> ProductIDlist = new List<int>();
                List<int> Quatity = new List<int>();
                foreach (var il in list)
                {
                    ProductIDlist.Add(il.ProductID);
                    Quatity.Add(il.Number);
                }
                #region ==新建入库单==
                StockOrderEntity rk = new StockOrderEntity();
                rk.BaseID = 5;

                rk.HospitalID = LoginUserEntity.HospitalID;
                rk.HouseID = model.HouseID;
                rk.IsVerify = 2;
                rk.Memo = "调拨单确认后的取消入库";
                rk.OrderTime = DateTime.Now;
                rk.Type = 2;
                rk.Class = 7;
                rk.OrderNo = RandomHelper.GetRandomStock("R");
                rk.UserID = LoginUserEntity.UserID;
                rk.VerifyTime = DateTime.Now;
                rk.VerifyUserID = LoginUserEntity.UserID;
                rk.DocumentNumber = GetDocumentNumber(rk.HospitalID, rk.Type);
                int _quatity = 0;
                decimal _allprice = 0;

                rk.AllChengBen = GetAllPriceInPurchasing(ProductIDlist, Quatity, ref _quatity, ref _allprice);
                rk.AllMoney = _allprice;
                rk.AllQuatity = _quatity;
                rk.VerifyMemo = "";
                var orderid = iwareBLL.AddStockOrder(rk);
                #endregion

                #region ==添加入库详情==
                foreach (var io in list)
                {
                    io.StockOrderNo = rk.OrderNo;
                    iwareBLL.AddStockOrderInfo(io);
                }
                #endregion

                #region ==入库操作和写入日志==

                ProductStockEntity pse = new ProductStockEntity();
                pse.HospitalID = LoginUserEntity.HospitalID;
                foreach (var io in list)
                {
                    pse.HouseID = io.HouseID;
                    pse.ProductID = io.ProductID;

                    pse = iwareBLL.GetProductStockListToseleect(pse)[0];
                    StockChangeLogEntity scl = new StockChangeLogEntity();
                    scl.UserID = LoginUserEntity.UserID;
                    scl.LogClass = 9;
                    scl.LogType = "入库";
                    scl.OwnedWarehouse = 0;//原来所在仓库为0
                    scl.IntoWarehouse = pse.HouseID;//调往仓库为自己当前仓库
                    scl.ProductID = io.ProductID;
                    scl.LogTitle = "调拨单确认后的取消入库.仓库ID" + pse.HouseID + "; 产品ID:" + io.ProductID + ";入库数量:" + io.Number + ";";
                    scl.NewQuatity = io.Number + pse.Quatity;
                    scl.OldQuatity = pse.Quatity;
                    scl.Quantity = io.Number;
                    scl.ProductStockID = pse.ID;
                    scl.OrderNo = rk.OrderNo;
                    var sclID = iwareBLL.AddStockChangeLog(scl);

                    pse.Quatity = pse.Quatity + io.Number;
                    result = iwareBLL.UpdateProductStock(pse);
                    if (result > 0)
                    {
                        result = iwareBLL.UpdateStatu(sclID);
                    }
                }



                #endregion

                #region ==取消单据==
                if (result > 0)
                {
                    result = iwareBLL.DelStockOrder(id);
                }
                #endregion
            }
            return Json(result);
        }

        #endregion


        #region ==盘点单==
        /// <summary>
        /// 盘点单页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Pandian()
        {
            return View();
        }

        /// <summary>
        /// 通过ID列表获取所有的信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllpandianList()
        {
            string IDlist = Request["idlist"];
            string kucunIDlist = Request["kucunIDlist"];
            var theID = IDlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var thekucun = kucunIDlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder str = new StringBuilder();

            int k = 0;
            foreach (var id in theID)
            {
                var list = GetInventory(Convert.ToInt32(id), Convert.ToInt32(thekucun[k]));
                str.Append("<tr>");
                str.AppendFormat("<td><input type='hidden' name='ID' value='{0}' />{1} <input type='hidden' name='HouseID' value='{2}' /> </td>", list.ID, list.Name, list.HouseID);
                str.AppendFormat("<td>{0}</td>", list.BrandName);
                str.AppendFormat("<td class='price'>{0}</td>", list.RetailPrice);//单价
                str.AppendFormat("<td class='BookNumber'>{0}</td>", list.BookNumber);//账面数量
                str.AppendFormat("<td><input type='text' class='wid50 InventoryNumber' onkeyup='CheckInputIntFloat(this)' name='InventoryNumber' value='{0}' /></td>", list.BookNumber);//实盘数量
                str.AppendFormat("<td class='MoreNumber'>0</td>");//赢盘数量
                str.AppendFormat("<td class='MorePrice'>0</td>");//赢盘金额
                str.AppendFormat("<td class='LessNumber'>0</td>");//盘亏数量
                str.AppendFormat("<td class='LessPrice'>0</td>");//盘亏金额
                str.AppendFormat("<td><a class='delthis' href='#'>删除</a> </td>");
                str.Append("</tr>");
                k++;
            }
            return Content(str.ToString());

        }

        /// <summary>
        /// 获取相信产品实体
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ProductInventoryEntity GetInventory(int ID, int kucun)
        {
            ProductStockEntity ps = new ProductStockEntity();
            ps.ID = kucun;
            ps = iwareBLL.GetProductStockModel(ps);

            ProductInventoryEntity entity = new ProductInventoryEntity();
            entity.ID = ID;
            entity.HospitalID = LoginUserEntity.HospitalID;
            //var  sent = iwareBLL.GetInventoryByID(entity);   //先获取账面数量
            ProductEntity pe = new ProductEntity();
            pe = baseinfoBLL.GetModel(ID);//获取产品详情
            entity.BookNumber = ps.Quatity;
            entity.BrandID = pe.BrandID;
            entity.BrandName = pe.BrandName;
            entity.CostPrice = pe.CostPrice;
            entity.Name = pe.Name;
            entity.PiFaPrice = pe.PiFaPrice;
            entity.ProductType = pe.ProductType;
            entity.ProductTypeName = pe.ProductTypeName;
            entity.RetailPrice = pe.RetailPrice;
            entity.TypeName = pe.TypeName;
            entity.HouseID = ps.HouseID;
            entity.ID = ps.ID;  //这里传入的是库存ID.之后获取这个库存ID即可
            return entity;
        }



        /// <summary>
        /// 读取库存产品
        /// </summary>
        /// <returns></returns>
        public ActionResult _kucunChanpin()
        {
            return View();
        }

        /// <summary>
        /// 读取库存产品
        /// </summary>
        /// <returns></returns>
        public ActionResult kucunChanpin(ProductStockEntity entity)
        {
            // int house = Request["houseid"]==""?0:Convert.ToInt32(Request["houseid"]);
            int house = 0;
            var houserList = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = LoginUserEntity.HospitalID });
            if (houserList != null)
            {
                house = houserList[0].ID;
            }

            if (entity.StartTime.Year < 1980) { entity.StartTime = DateTime.Now.AddYears(-10); }
            if (entity.EndTime.Year < 1980) { entity.EndTime = DateTime.Now; }

            if (!String.IsNullOrEmpty(entity.ProductName))
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            if (house > 0)
            {
                entity.HouseID = house;
            }


            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 150; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = iwareBLL.GetProductStockList(entity, out rows, out pagecount); //baseinfoBLL.GetAllProduct(entity, out rows, out pagecount);  //
              
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.HouseID = entity.HouseID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.BrandID = entity.BrandID;
            if (Request.IsAjaxRequest())
                return PartialView("_kucunChanpin", result);
            return View(result);
        }
        /// <summary>
        /// 添加盘点
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPandian()
        {
            try
            {
                string[] IDList = Request.Form.GetValues("ID");//获取库存id列表
                string[] InventoryList = Request.Form.GetValues("InventoryNumber");//实盘数量列表
                int UserID = Convert.ToInt32(Request["UserID"]);//盘点人
                //  DateTime time = Convert.ToDateTime(Request["Time"]);//盘点时间
                string memo = Request["VerifyMemo"];//备注
                var result = 0;
                CheckOrderEntity entity = new CheckOrderEntity();
                entity.OrderNo = RandomHelper.GetRandomStock("Pan");
                entity.HospitalID = LoginUserEntity.HospitalID;
                entity.CheckTime = DateTime.Now;
                entity.UserID = UserID;
                entity.VerifyMemo = memo;
                entity.VerifyTime = DateTime.Now;
                entity.VerifyUserID = 0;
                entity.IsVerify = 1;
                entity.HouseID = 0; //ps.HouseID;         //仓库现在暂时不用
                entity.ProductID = 0;// ps.ProductID;    //产品现在暂时没有
                entity.ZhangMianQuatity = 0;//要算账面数量先要清0
                entity.ShijiQuatity = 0;//先清零
                for (int i = 0; i < IDList.Length; i++)
                {
                    if (i == 121)
                    {
                        var k = 1;
                    }
                    var kucunID = Convert.ToInt32(IDList[i]);

                    if (kucunID == 0)
                    {
                        LogWriter.WriteInfo(IDList[i] + IDList.ToString(), "盘点数据有误");
                        continue;
                    }
                    ProductStockEntity ps = new ProductStockEntity();    //通过库存ID查询出库存
                    ps.ID = kucunID;
                    ps = iwareBLL.GetProductStockModel(ps);
                    var pe = baseinfoBLL.GetModel(ps.ProductID);//获取产品详情 ,需要获取它的售价

                    #region ==添加详情==
                    CheckOrderInfoEntity coe = new CheckOrderInfoEntity();
                    coe.OrderNo = entity.OrderNo;
                    coe.kucunID = ps.ID;
                    coe.HouseID = ps.HouseID;
                    coe.ProductID = ps.ProductID;
                    var productEntity = baseinfoBLL.GetModel(coe.ProductID);
                    if (productEntity.HospitalID != LoginUserEntity.ParentHospitalID)
                    {
                    }
                    else
                    {
                        coe.ActualQuatity = Convert.ToInt32(InventoryList[i]);//实际数量
                    //coe.BookQuatity = ps.Quatity;    //账面数量
                    //coe.LessQuatity = coe.BookQuatity - coe.ActualQuatity;// 盘亏数量=账面数量-实盘数量
                    //coe.LessPrice = coe.LessQuatity * pe.RetailPrice;
                    //coe.Price = pe.RetailPrice;
                    //如果是盘盈,则修改盘盈
                    //if (coe.LessQuatity < 0)
                    //{
                    //    coe.MoreQuatity = -coe.LessQuatity;
                    //    coe.MorePrice = -coe.LessPrice;
                    //    coe.LessPrice = 0;
                    //    coe.LessQuatity = 0;
                    //}
                       iwareBLL.AddCheckOrderInfo(coe);//添加详情

                    }
                    #endregion

                    #region ==订单主要数据收集==
                    entity.ZhangMianQuatity += ps.Quatity;//累加账面数量
                    entity.ShijiQuatity += coe.ActualQuatity;  //累加实盘数量
                    //entity.PankuiQuatity = entity.PankuiQuatity + coe.LessQuatity;   //获取盘亏数量
                    //entity.PankuiMoeny = entity.PankuiMoeny + coe.LessPrice;    //获取盘亏金额
                    //entity.PanYinQuatity = entity.PanYinQuatity + coe.MoreQuatity;
                    //entity.PanyinMoney = entity.PanyinMoney + coe.MorePrice;
                    #endregion
                }
                result = Convert.ToInt32(iwareBLL.AddCheckOrder(entity));
                return Json(result);
            }
            catch
            {
                var a = 1;
                return Json(null);
            }
        }

        /// <summary>
        /// 添加盘点
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPandian1()
        {
            try { 
                string[] IDList = Request.Form.GetValues("ID");//获取库存id列表
                string[] InventoryList = Request.Form.GetValues("InventoryNumber");//实盘数量列表
                int UserID = Convert.ToInt32(Request["UserID"]);//盘点人
                //  DateTime time = Convert.ToDateTime(Request["Time"]);//盘点时间
                string memo = Request["VerifyMemo"];//备注
                var result = 0;
                CheckOrderEntity entity = new CheckOrderEntity();
                entity.OrderNo = RandomHelper.GetRandomStock("Pan");
                entity.HospitalID = LoginUserEntity.HospitalID;
                entity.CheckTime = DateTime.Now;
                entity.UserID = UserID;
                entity.VerifyMemo = memo;
                entity.VerifyTime = DateTime.Now;
                entity.VerifyUserID = 0;
                entity.IsVerify = 1;
                entity.HouseID = 0; //ps.HouseID;         //仓库现在暂时不用
                entity.ProductID = 0;// ps.ProductID;    //产品现在暂时没有
                entity.ZhangMianQuatity = 0;//要算账面数量先要清0
                entity.ShijiQuatity = 0;//先清零
                for (int i = 0; i < IDList.Length; i++)
                {
                    if (i == 121) {
                        var k = 1;
                    }
                    var kucunID = Convert.ToInt32(IDList[i]);

                    if (kucunID == 0)
                    {
                        LogWriter.WriteInfo(IDList[i] + IDList.ToString(), "盘点数据有误");
                        continue;
                    }
                    ProductStockEntity ps = new ProductStockEntity();    //通过库存ID查询出库存
                    ps.ID = kucunID;
                    ps = iwareBLL.GetProductStockModel(ps);
                    var pe = baseinfoBLL.GetModel(ps.ProductID);//获取产品详情 ,需要获取它的售价

                    #region ==添加详情==
                    CheckOrderInfoEntity coe = new CheckOrderInfoEntity();
                    coe.OrderNo = entity.OrderNo;
                    coe.kucunID = ps.ID;
                    coe.HouseID = ps.HouseID;
                    coe.ProductID = ps.ProductID;
                    var productEntity = baseinfoBLL.GetModel(coe.ProductID);
                    if (productEntity.HospitalID != LoginUserEntity.ParentHospitalID)
                    {
                        return Json(-1);
                    }
                    coe.ActualQuatity = Convert.ToInt32(InventoryList[i]);//实际数量
                    //coe.BookQuatity = ps.Quatity;    //账面数量
                    //coe.LessQuatity = coe.BookQuatity - coe.ActualQuatity;// 盘亏数量=账面数量-实盘数量
                    //coe.LessPrice = coe.LessQuatity * pe.RetailPrice;
                    //coe.Price = pe.RetailPrice;
                    //如果是盘盈,则修改盘盈
                    //if (coe.LessQuatity < 0)
                    //{
                    //    coe.MoreQuatity = -coe.LessQuatity;
                    //    coe.MorePrice = -coe.LessPrice;
                    //    coe.LessPrice = 0;
                    //    coe.LessQuatity = 0;
                    //}
                    iwareBLL.AddCheckOrderInfo(coe);//添加详情
                    #endregion

                    #region ==订单主要数据收集==
                    entity.ZhangMianQuatity += ps.Quatity;//累加账面数量
                    entity.ShijiQuatity += coe.ActualQuatity;  //累加实盘数量
                    //entity.PankuiQuatity = entity.PankuiQuatity + coe.LessQuatity;   //获取盘亏数量
                    //entity.PankuiMoeny = entity.PankuiMoeny + coe.LessPrice;    //获取盘亏金额
                    //entity.PanYinQuatity = entity.PanYinQuatity + coe.MoreQuatity;
                    //entity.PanyinMoney = entity.PanyinMoney + coe.MorePrice;
                    #endregion
                }
                result = Convert.ToInt32(iwareBLL.AddCheckOrder(entity));
                return Json(result);
            }
            catch {
                var a = 1;
                return Json(null);
            }
        }

        public ActionResult _PandianList()
        {
            return View();
        }

        public ActionResult PandianList(int IsVerify = 999, string StartTime = "", string EndTime = "", int pageNum = 1, int HospitalID = 0)
        {
            CheckOrderEntity entity = new CheckOrderEntity();
            entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));

            if (IsVerify == 999)
            {
                IsVerify = 1;
            }
            entity.IsVerify = IsVerify;
            entity.pageNum = pageNum;
            if (StartTime != "")
            {
                entity.StartTime = Convert.ToDateTime(StartTime);
            }
            if (EndTime != "")
            {
                entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndTime).Year + "-" + Convert.ToDateTime(EndTime).Month + "-" + Convert.ToDateTime(EndTime).Day + " 23:59:59");
            }
            int rows;
            int pagecount;

            entity.HospitalID = HospitalID == 0 ? LoginUserEntity.HospitalID : HospitalID;
            entity.numPerPage = 10; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            entity.orderDirection = "desc";
            var list = iwareBLL.GetCheckOrderList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            var sum = iwareBLL.GetSUmOrderList(entity);
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.IsVerify = IsVerify;
            ViewBag.AllPrice = sum.PankuiMoeny;
            ViewBag.AllNumber = sum.PankuiQuatity;
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.Name = entity.ProductName;
            ViewBag.HouseID = entity.HouseID;

            if (Request.IsAjaxRequest())
                return PartialView("_PandianList", result);
            return View(result);
        }

        public ActionResult _Pandianinfo()
        {
            return View();
        }

        public ActionResult Pandianinfo()
        {
            var id = Request["ID"];
            var checkorder = iwareBLL.GetCheckorderInfoModel(new CheckOrderInfoEntity { ID = Convert.ToInt32(id) });
            //var checkorderinfolist = iwareBLL.GetCheckOrderInfoList(new CheckOrderInfoEntity { OrderNo = checkorder.OrderNo, HospitalID=loginUserEntity.ParentHospitalID });
            // var result = checkorder.AsQueryable().ToPagedList(1, 10000);
            return View(checkorder);
        }

        /// <summary>
        /// 添加盘点出入库操作
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPandianRukuAndChuku()
        {
            var ID = Convert.ToInt32(Request["ID"]);
            var result = 0;
            if (ID > 0)
            {
                CheckOrderEntity entity = new CheckOrderEntity();
                entity.ID = ID;
                entity = iwareBLL.GetCheckOrderModel(entity);//获取盘点单
                if (entity.IsVerify != 1) { return Json(-1); }
                var checkinfolist = iwareBLL.GetCheckOrderInfoList(new CheckOrderInfoEntity { OrderNo = entity.OrderNo, HospitalID = LoginUserEntity.ParentHospitalID });
                int zhangmian = 0, shipan = 0, panying = 0, pankui = 0;
                decimal pyjinger = 0, pkjinger = 0, shipanprice = 0;
                //盘点检查是否入库
                string productname = "";
                foreach (var model in checkinfolist)
                {
                    ProductStockEntity pse = new ProductStockEntity();
                    pse.HospitalID = LoginUserEntity.HospitalID;
                    pse.HouseID = model.HouseID;
                    pse.ProductID = model.ProductID;
                    var pselist = iwareBLL.GetProductStockListToseleect(pse);
                    if (pselist.Count == 0)
                    {
                        var productinfo = baseinfoBLL.GetModel(model.ProductID);
                        productname = productname + "," + productinfo.Name;
                    }
                }
                if (!String.IsNullOrEmpty(productname))
                {
                    //ViewData["productname"] = productname;
                    return Json(productname);
                }
                foreach (var model in checkinfolist)
                {
                    #region ==修改盘点单==
                    ProductStockEntity ps = new ProductStockEntity();
                    ps.ID = model.kucunID;//获取这个库存ID
                    ps = iwareBLL.GetProductStockModel(ps);
                    var Prodect = baseinfoBLL.GetModel(ps.ProductID);//获取产品详情 ,需要获取它的售价
                    shipanprice = model.ActualQuatity * Prodect.RetailPrice;
                    model.BookQuatity = ps.Quatity;//获取即时的库存数量,也就是账面数量
                    model.RetailPrice = Prodect.RetailPrice;
                    model.Price = Prodect.RetailPrice;
                    zhangmian += model.BookQuatity;
                    shipan += model.ActualQuatity;
                    if (model.ActualQuatity > model.BookQuatity)
                    {
                        model.MoreQuatity = model.ActualQuatity - model.BookQuatity;
                        model.MorePrice = model.MoreQuatity * Prodect.RetailPrice;
                        //盘盈盘亏累加
                        pyjinger += model.MorePrice;
                        panying += model.MoreQuatity;
                    }
                    else
                    {
                        model.LessQuatity = model.BookQuatity - model.ActualQuatity;
                        model.LessPrice = model.LessQuatity * Prodect.RetailPrice;
                        //盘盈盘亏累加
                        pankui += model.LessQuatity;
                        pkjinger += model.LessPrice;
                    }

                    result = iwareBLL.UpdateCheckOrderInfo(model);



                    #endregion

                    #region ==出入库==
                    StockOrderEntity rk = new StockOrderEntity();
                    rk.BaseID = 5;

                    rk.HospitalID = LoginUserEntity.HospitalID;
                    rk.HouseID = model.HouseID;
                    rk.IsVerify = 2;
                    rk.Memo = "自动绑定处理盘点出入库";

                    rk.OrderTime = DateTime.Now;
                    // rk.ProductID = model.ProductID;

                    rk.Type = (model.LessQuatity > 0 ? 1 : 2);
                    rk.Class = (model.LessQuatity > 0 ? 5 : 4);
                    rk.OrderNo = rk.Type == 1 ? RandomHelper.GetRandomStock("C") : RandomHelper.GetRandomStock("R");
                    rk.UserID = LoginUserEntity.UserID;
                    rk.VerifyTime = DateTime.Now;
                    rk.AllQuatity = (model.LessQuatity > 0 ? model.LessQuatity : model.MoreQuatity);
                    rk.AllMoney = model.LessPrice > 0 ? model.LessPrice : model.MorePrice;
                    rk.AllChengBen = Prodect.CostPrice * rk.AllQuatity;

                    rk.VerifyUserID = LoginUserEntity.UserID;
                    rk.DocumentNumber = GetDocumentNumber(rk.HospitalID, rk.Type);
                    rk.VerifyMemo = entity.OrderNo;
                    iwareBLL.AddStockOrder(rk);

                    //出入库详情
                    StockOrderInfo info = new StockOrderInfo();
                    info.HouseID = model.HouseID;
                    info.Number = (model.LessQuatity > 0 ? model.LessQuatity : model.MoreQuatity);
                    info.ProductID = model.ProductID;
                    info.StockOrderNo = rk.OrderNo;

                    result = iwareBLL.AddStockOrderInfo(info);
                    //库存出入修改
                    ProductStockEntity pse = new ProductStockEntity();
                    pse.HospitalID = LoginUserEntity.HospitalID;
                    pse.HouseID = info.HouseID;
                    pse.ProductID = info.ProductID;
                    pse = iwareBLL.GetProductStockListToseleect(pse)[0];

                    var sclID = 0;
                    if (rk.Type == 1)
                    {
                        StockChangeLogEntity scl = new StockChangeLogEntity();
                        scl.UserID = LoginUserEntity.UserID;
                        scl.LogClass = 2;
                        scl.LogType = "出库";
                        scl.OwnedWarehouse = pse.HouseID;
                        scl.IntoWarehouse = 0;
                        scl.ProductID = info.ProductID;
                        scl.LogTitle = "盘点单审核出库.仓库ID:" + info.HouseID + "; 产品ID:" + info.ProductID + ";出库数量:" + info.Number + ";";
                        scl.NewQuatity = pse.Quatity - info.Number;
                        scl.OldQuatity = pse.Quatity;
                        scl.Quantity = info.Number;
                        scl.ProductStockID = pse.ID;
                        scl.OrderNo = rk.OrderNo + "(盘点单:" + entity.OrderNo + ")";
                        sclID = iwareBLL.AddStockChangeLog(scl);
                        pse.Quatity = pse.Quatity - info.Number;
                    }
                    else
                    {
                        StockChangeLogEntity scl = new StockChangeLogEntity();
                        scl.UserID = LoginUserEntity.UserID;
                        scl.LogClass = 2;
                        scl.OwnedWarehouse = 0;
                        scl.IntoWarehouse = pse.HouseID;
                        scl.ProductID = info.ProductID;
                        scl.LogType = "入库";
                        scl.LogTitle = "盘点单审核入库.仓库ID:" + info.HouseID + "; 产品ID:" + info.ProductID + ";入库数量:" + info.Number + ";";
                        scl.NewQuatity = pse.Quatity + info.Number;
                        scl.OldQuatity = pse.Quatity;
                        scl.Quantity = info.Number;
                        scl.ProductStockID = pse.ID;
                        scl.OrderNo = rk.OrderNo + "(盘点单:" + entity.OrderNo + ")";
                        sclID = iwareBLL.AddStockChangeLog(scl);
                        pse.Quatity = pse.Quatity + info.Number;
                    }

                    var returns = iwareBLL.UpdateProductStock(pse);
                    if (returns > 0)
                    {
                        int len = 2;
                        var usersetting = iSysBLL.GetUserSettingsValue(LoginUserEntity.HospitalID, "StockNumber");

                        try
                        {
                            if (usersetting.AttributeValue != null)
                            {
                                len = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue);
                            }
                            else
                            {
                                len = 2;
                            }
                        }
                        catch (Exception)
                        {
                            len = 2;
                        }


                        if (pse.Quatity <= len)
                        {
                            var ProductModel = baseinfoBLL.GetModel(info.ProductID);
                            TaskEntity te = new TaskEntity();
                            te.HospitalID = LoginUserEntity.HospitalID;
                            te.CreateTime = DateTime.Now;
                            te.Type = 4;
                            te.Statu = 1;
                            te.Name = LoginUserEntity.Name + "库存不足,请入库";
                            te.Content = "产品:" + ProductModel.Name + "库存为:" + pse.Quatity;
                            iCentBLL.AddTaskEntity(te);
                        }

                        iwareBLL.UpdateStatu(sclID);
                    }

                    if (result > 0)
                    {
                        iwareBLL.UpdateinfoStatus(ID);
                        iwareBLL.UpdateOrderVerify(ID, 2);
                    }

                    #endregion
                }

                entity.PankuiMoeny = pkjinger;
                entity.PankuiQuatity = pankui;
                entity.PanyinMoney = pyjinger;
                entity.PanYinQuatity = panying;
                entity.ShijiPrice = shipanprice;
                entity.ShijiQuatity = shipan;
                entity.ZhangMianQuatity = zhangmian;
                entity.VerifyTime = DateTime.Now;
                entity.DocumentNumber = "P" + DateTime.Today.ToString("yyyyMMdd") + iwareBLL.GetTodayCheckOrderNumber(entity.HospitalID, 2).ToString().PadLeft(3, '0');
                result = iwareBLL.UpdateCheckOrder(entity);
                #region ==添加库存动态==
                CompanyNewsEntity cust = new CompanyNewsEntity();
                cust.HospitalID = entity.HospitalID;
                cust.CreateTime = DateTime.Now;
                cust.HuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.NextHuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.Type = 4;
                //var hospuser = PersBLL.GetUserByUserID(model.UserID);
                cust.UserName = entity.UserName;
                cust.Name = cust.UserName + "进行库存盘点";
                // cust.ProjectName = rk.OrderNo;
                iCentBLL.AddCompanyNewsEntity(cust);
                #endregion
            }

            return Json(result);

        }

        public ActionResult PandianEdit()
        {
            var ID = Convert.ToInt32(Request["ID"]);
            var order = iwareBLL.GetCheckOrderModel(new CheckOrderEntity { ID = ID });
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var ProductName = Request["ProductName"];
            IList<CheckOrderInfoEntity> list = null;
            if (String.IsNullOrEmpty(ProductName))
            {
                list = iwareBLL.GetCheckOrderInfoList(new CheckOrderInfoEntity { OrderNo = order.OrderNo, HospitalID = LoginUserEntity.ParentHospitalID });
            }
            else
            {
                list = iwareBLL.GetCheckOrderInfoList(new CheckOrderInfoEntity { OrderNo = order.OrderNo, HospitalID = LoginUserEntity.ParentHospitalID }).Where(c => c.ProductName == ProductName).ToList();
            }
            foreach (var info in list)
            {
                ProductStockEntity pse = new ProductStockEntity();
                pse.HouseID = info.HouseID;
                pse.ProductID = info.ProductID;
                pse.HospitalID = LoginUserEntity.HospitalID;
                if (order.IsVerify == 1)
                {
                    var kucunlist = iwareBLL.GetProductStockListToseleect(pse);
                    if (kucunlist.Count > 0)     //如果能找到库存,则找到这个库存
                    {
                        info.kucunID = kucunlist[0].ID;
                        info.BookQuatity = kucunlist[0].Quatity;
                    }
                }

            }
            var result = list.AsQueryable().ToPagedList(1, 100000);//添加Webdiyer.WebControls.Mvc应用
            return View(result);
        }

        public ActionResult EditPandian()
        {
            string[] IDList = Request.Form.GetValues("kucunID");//获取库存id列表
            string[] InventoryList = Request.Form.GetValues("InventoryNumber");//实盘数量列表
            int UserID = Convert.ToInt32(Request["UserID"]);//盘点人
            //DateTime time = Convert.ToDateTime(Request["Time"]);//盘点时间
            string memo = Request["VerifyMemo"];//备注
            string OrderNo = Request["OrderNo"];
            var result = 0;
            CheckOrderEntity entity = new CheckOrderEntity();
            entity.OrderNo = OrderNo;
            entity.UserID = UserID;
            // entity.CheckTime = DateTime.Now;
            entity.VerifyMemo = memo;
            iwareBLL.DelCheckOrderInfoByOrderNo(OrderNo);//删除原有的详情
            for (int i = 0; i < IDList.Length; i++)
            {
                ProductStockEntity ps = new ProductStockEntity();    //通过库存ID查询出库存
                ps.ID = Convert.ToInt32(IDList[i]);
                ps = iwareBLL.GetProductStockModel(ps);
                var pe = baseinfoBLL.GetModel(ps.ProductID);//获取产品详情 ,需要获取它的售价

                #region ==添加详情==
                CheckOrderInfoEntity coe = new CheckOrderInfoEntity();
                coe.kucunID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(IDList[i]);
                coe.OrderNo = entity.OrderNo;
                coe.HouseID = ps.HouseID;
                coe.ProductID = ps.ProductID;
                coe.BookQuatity = ps.Quatity;    //账面数量
                coe.ActualQuatity = Convert.ToInt32(InventoryList[i]);//实际数量
                coe.LessQuatity = coe.BookQuatity - coe.ActualQuatity;// 盘亏数量=账面数量-实盘数量
                coe.LessPrice = coe.LessQuatity * pe.RetailPrice;
                coe.Price = pe.RetailPrice;
                //如果是盘盈,则修改盘盈
                if (coe.LessQuatity < 0)
                {
                    coe.MoreQuatity = -coe.LessQuatity;
                    coe.MorePrice = -coe.LessPrice;
                    coe.LessPrice = 0;
                    coe.LessQuatity = 0;
                }

                iwareBLL.AddCheckOrderInfo(coe);//添加详情
                #endregion

                #region ==订单主要数据收集==
                entity.ZhangMianQuatity = entity.ZhangMianQuatity + coe.BookQuatity;//累加账面数量
                entity.ShijiQuatity = entity.ShijiQuatity + coe.ActualQuatity;  //累加实盘数量
                entity.PankuiQuatity = entity.PankuiQuatity + coe.LessQuatity;   //获取盘亏数量
                entity.PankuiMoeny = entity.PankuiMoeny + coe.LessPrice;    //获取盘亏金额
                entity.PanYinQuatity = entity.PanYinQuatity + coe.MoreQuatity;
                entity.PanyinMoney = entity.PanyinMoney + coe.MorePrice;
                #endregion
            }
            result = Convert.ToInt32(iwareBLL.UpdateCheckOrder(entity));
            return Json(result);
        }

        public ActionResult DelPanOrder()
        {
            var ID = Convert.ToInt32(Request["ID"]);
            return Json(iwareBLL.DelCheckOrder(ID));
        }

        #endregion

        #region ==预付单 ==
        public ActionResult YufuList(int IsVerify = 999, string StartTime = "", string EndTime = "", int pageNum = 1, int HospitalID = 0)
        {
            StockOrderEntity entity = new StockOrderEntity();
            entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));


            //页面默认进去查询 待审核单据
            if (IsVerify == 999)
            {
                IsVerify = 1;
            }
            entity.IsVerify = IsVerify;
            entity.pageNum = pageNum;
            if (StartTime != "")
            {
                entity.StartTime = Convert.ToDateTime(StartTime);
            }
            if (EndTime != "")
            {
                entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndTime).Year + "-" + Convert.ToDateTime(EndTime).Month + "-" + Convert.ToDateTime(EndTime).Day + " 23:59:59");
            }
            if (!String.IsNullOrEmpty(entity.ProductName))
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            int rows;
            int pagecount;
            entity.HospitalID = HospitalID == 0 ? LoginUserEntity.HospitalID : HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.IsVerify = IsVerify;
            entity.Type = 5;
            //  entity.Class = 1;
            entity.numPerPage = 9; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            entity.orderDirection = "desc";
            var list = iwareBLL.GetStockOrderList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            var sum = iwareBLL.getSumStock(entity);
            ViewBag.AllPrice = sum.AllMoney;
            ViewBag.AllNumber = sum.AllQuatity;
            ViewBag.allchengben = sum.AllChengBen;
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_YufuList", result);
            return View(result);
        }

        /// <summary>
        /// 预付单审核
        /// </summary>
        /// <returns></returns>
        public ActionResult YuFuXamine()
        {
            var orderno = Request["OrderNo"];
            // var model = iwareBLL.GetCheckOrderModelByOrderNo(new CheckOrderEntity { OrderNo = orderno });
            var model = iwareBLL.GetStockOrderModel(new StockOrderEntity { OrderNo = orderno });
            //if (model.IsVerify != 1)
            //{
            //    return Json(-1);
            //}
            var infolist = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = orderno });
            StockOrderEntity soe = new StockOrderEntity();
            var order = iwareBLL.GetStockOrderModel(new StockOrderEntity { OrderNo = orderno });

            List<int> ProductIDlist = new List<int>();
            List<int> Quatity = new List<int>();
            foreach (var il in infolist)
            {
                ProductIDlist.Add(il.ProductID);
                Quatity.Add(il.Number);
            }

            if (QuatityEnough(infolist))
            {
                #region 添加出库单和出库单详情和修改库存

                soe.BaseID = 0;
                soe.OrderNo = RandomHelper.GetRandomStock("C");
                soe.Class = 6;
                soe.HospitalID = order.HospitalID;
                soe.IsVerify = 2;
                soe.Memo = "预付出库";
                soe.OrderTime = DateTime.Now;
                soe.UserID = LoginUserEntity.UserID;
                soe.VerifyMemo = order.VerifyMemo;
                soe.VerifyTime = DateTime.Now;
                soe.VerifyUserID = LoginUserEntity.UserID;
                soe.DocumentNumber = GetDocumentNumber(soe.HospitalID, 1);
                soe.VerifyMemo = orderno;
                soe.Type = 1;
                soe.AllQuatity = infolist.Sum(i => i.Number);
                soe.AllMoney = GetAllPriceByList(infolist.ToList());
                int _quatity = 0;
                decimal _allprice = 0;
                soe.AllChengBen = GetAllPriceInPurchasing(ProductIDlist, Quatity, ref _quatity, ref _allprice);
                iwareBLL.AddStockOrder(soe);    //添加出库单
                StockOrderInfo stoinfo = new StockOrderInfo();
                var result = 0;
                stoinfo.StockOrderNo = soe.OrderNo;
                foreach (var info in infolist)
                {
                    stoinfo.ProductID = info.ProductID;
                    stoinfo.HouseID = info.HouseID;
                    stoinfo.Number = info.Number;
                    iwareBLL.AddStockOrderInfo(stoinfo);//添加出库详情

                    //产品出库(线通过门店ID,仓库产品获取库存,然后修改库存后保存)
                    ProductStockEntity pse = new ProductStockEntity();
                    pse.HospitalID = order.HospitalID;// loginUserEntity.HospitalID; //order.DiaoHospitalID;
                    pse.HouseID = info.HouseID;
                    pse.ProductID = info.ProductID;
                    pse = iwareBLL.GetProductStockListToseleect(pse)[0];

                    StockChangeLogEntity scl = new StockChangeLogEntity();
                    scl.UserID = LoginUserEntity.UserID;
                    scl.LogClass = 3;
                    scl.OwnedWarehouse = pse.HouseID;
                    scl.IntoWarehouse = 0;
                    scl.ProductID = info.ProductID;
                    scl.LogType = "出库";
                    scl.LogTitle = "预付单审核.仓库ID:" + info.HouseID + "; 产品ID:" + info.ProductID + ";出库数量:" + info.Number + ";";
                    scl.NewQuatity = pse.Quatity - info.Number;
                    scl.OldQuatity = pse.Quatity;
                    scl.Quantity = info.Number;
                    scl.ProductStockID = pse.ID;
                    scl.OrderNo = soe.OrderNo + "(预付单:" + orderno + ")";
                    var sclID = iwareBLL.AddStockChangeLog(scl);

                    pse.Quatity = pse.Quatity - info.Number;
                    result = iwareBLL.UpdateProductStock(pse);

                    if (result > 0)
                    {
                        int len = 2;
                        var usersetting = iSysBLL.GetUserSettingsValue(LoginUserEntity.HospitalID, "StockNumber");
                        try
                        {
                            if (usersetting.AttributeValue != null)
                            {
                                len = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue);
                            }
                            else
                            {
                                len = 2;
                            }
                        }
                        catch (Exception)
                        {
                            len = 2;
                        }

                        if (pse.Quatity <= len)
                        {
                            var ProductModel = baseinfoBLL.GetModel(info.ProductID);
                            TaskEntity te = new TaskEntity();
                            te.HospitalID = LoginUserEntity.HospitalID;
                            te.CreateTime = DateTime.Now;
                            te.Type = 4;
                            te.Statu = 1;
                            te.Name = LoginUserEntity.Name + "库存不足,请入库";
                            te.Content = "产品:" + ProductModel.Name + "库存为:" + pse.Quatity;
                            iCentBLL.AddTaskEntity(te);
                        }
                    }

                }

                #endregion



                soe.OrderNo = orderno;
                soe.IsVerify = 2;
                soe.DocumentNumber = GetDocumentNumber(model.HospitalID, model.Type);
                result = iwareBLL.UpdateStockOrderVerify(soe);


                #region ==添加库存动态==
                CompanyNewsEntity cust = new CompanyNewsEntity();
                cust.HospitalID = LoginUserEntity.HospitalID;
                cust.CreateTime = DateTime.Now;
                cust.HuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.NextHuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
                cust.Type = 4;
                //var hospuser = PersBLL.GetUserByUserID(model.UserID);
                cust.UserName = LoginUserEntity.UserName;
                cust.Name = cust.UserName + "进行预付单处理";
                cust.ProjectName = orderno;
                iCentBLL.AddCompanyNewsEntity(cust);
                #endregion

                return Json(result);
            }
            else
            {
                return Json(-1);
            }

        }




        #endregion


        #endregion

        #region ==采购管理==
        /// <summary>
        /// 采购管理
        /// </summary>
        /// <returns></returns>
        public ActionResult PurchasingManagement()
        {
            return View();
        }


        #region ==进货==
        /// <summary>
        /// 进货单页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Jinhuo()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }

        public ActionResult _SupplierList()
        {
            return View();
        }

        public ActionResult SupplierList(SupplierEntity entity)
        {
            if (!String.IsNullOrEmpty(entity.Name))
            {
                entity.Name = entity.Name.ToUpper();
            }
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 9; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = iwareBLL.GetSupplierList1(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_SupplierList", result);
            return View(result);
        }

        /// <summary>
        /// 添加进货单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddJinhuo(BuyOrderEntity entity)
        {
            var result = 0;
            string[] ProductList = Request.Form.GetValues("ID"); //产品ID
            string[] pricelist = Request.Form.GetValues("Price");//价格
            string[] NumberList = Request.Form.GetValues("Number"); //数量集合
            int allnumber = 0;
            for (int i = 0; i < NumberList.Count(); i++)
            {
                allnumber = allnumber + Convert.ToInt32(NumberList[i]);
            }

            entity.AllQutity = allnumber;

            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.CreateTime = DateTime.Now;
            entity.Type = 1;
            entity.Statu = 1;
            entity.OrderNo = RandomHelper.GetRandomStock("IN");
            var res = iwareBLL.AddBuyOrder(entity);
            BuyOrderInfoEntity bo = new BuyOrderInfoEntity();
            bo.OrderNO = entity.OrderNo;
            for (int i = 0; i < ProductList.Length; i++)
            {
                var product = baseinfoBLL.GetModel(Convert.ToInt32(ProductList[i]));  //获取产品详情
                bo.Quatity = Convert.ToInt32(NumberList[i]);
                bo.Price = Convert.ToDecimal(pricelist[i]);//product.CostPrice; k
                bo.ProductID = Convert.ToInt32(ProductList[i]);
                bo.OldPrice = product.CostPrice;//现在只要零售价,不需要成本价
                bo.NeedMoney = bo.OldPrice * bo.Quatity;//预计所需价格
                bo.Allprice = bo.Price * bo.Quatity;//所有价格
                bo.DifferentMoney = bo.NeedMoney - bo.Allprice;   //这个是相差价格
                result = iwareBLL.AddBuyOrderInfo(bo);



            }
            return Json(result);
        }

        /// <summary>
        /// 进货列表
        /// </summary>
        /// <returns></returns>
        public ActionResult JinhuoList(BuyOrderEntity entity)
        {
            ViewBag.Statu = entity.Statu;
            entity.StartTime = (entity.StartTime.Year < 1970 ? DateTime.Now.AddDays(-30) : entity.StartTime);
            entity.EndTime = (entity.EndTime.Year < 1970 ? DateTime.Now : entity.EndTime);

            entity.StartTime = Convert.ToDateTime(entity.StartTime.Year + "-" + entity.StartTime.Month + "-" + entity.StartTime.Day + " 01:01:01");
            entity.EndTime = Convert.ToDateTime(entity.EndTime.Year + "-" + entity.EndTime.Month + "-" + entity.EndTime.Day + " 23:59:59");
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.Type = 1;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            entity.orderDirection = "desc";
            var list = iwareBLL.GetBuyOrderList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(entity.pageNum, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            var sumall = iwareBLL.getSumBuyOrder(entity);
            ViewBag.AllActualMoney = sumall.ActualMoney;      //总共实付总额
            ViewBag.AllNeedMoney = sumall.NeedMoney;         //应付总额
            entity.Statu = 2;//把状态改为2,表示会已审核的记录
            var TSum = iwareBLL.getSumBuyOrder(entity);//获取已审核的记录
            ViewBag.TActualMoney = TSum.ActualMoney;      //已审核的实付总额
            ViewBag.TNeedMoney = TSum.NeedMoney;         //已审核的应付总额
            entity.Statu = 1;//把状态改为1,表示未审核的记录
            var FSum = iwareBLL.getSumBuyOrder(entity);
            ViewBag.FActualMoney = FSum.ActualMoney;      //已审核的实付总额
            ViewBag.FNeedMoney = FSum.NeedMoney;         //已审核的应付总额

            ViewBag.StartTime = entity.StartTime.ToString("yyyy-MM-dd");
            ViewBag.EndTime = entity.EndTime.ToString("yyyy-MM-dd");
            if (Request.IsAjaxRequest())
                return PartialView("_JinhuoList", result);

            return View(result);
        }

        /// <summary>
        /// 添加仓库页面
        /// </summary>
        /// <returns></returns>
        public ActionResult addWarehouse()
        {
            return View();
        }

        /// <summary>
        /// 添加仓库,返回ID
        /// </summary>
        /// <returns></returns>
        public ActionResult Addhouse()
        {
            string name = Request["name"];
            WarehouseEntity entity = new WarehouseEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.IsActive = 1;
            entity.IsDefault = 2;
            entity.Name = name;
            var result = Convert.ToInt32(iwareBLL.AddWarehouse(entity));
            return Json(result);
        }

        /// <summary>
        /// 进货详情
        /// </summary>
        /// <returns></returns>
        public ActionResult JinhuoInfo()
        {
            string id = Request["ID"];
            var info = iwareBLL.GetBuyOrderModel(new BuyOrderEntity { ID = Convert.ToInt32(id) });
            BuyOrderInfoEntity be = new BuyOrderInfoEntity();
            var list = iwareBLL.GetBuyOrderInfoList(new BuyOrderInfoEntity { OrderNO = info.OrderNo });
            var result = list.AsQueryable().ToPagedList(1, 10000);//添加Webdiyer.WebControls.Mvc应用
            return View(result);
        }

        /// <summary>
        /// 进货单入库
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderRuku()
        {
            int id = Convert.ToInt32(Request["ID"]);
            var info = iwareBLL.GetBuyOrderModel(new BuyOrderEntity { ID = Convert.ToInt32(id) });
            if (info.Statu != 1)
            {
                return Json(-1);
            }
            BuyOrderInfoEntity be = new BuyOrderInfoEntity();
            be.OrderNO = info.OrderNo;
            be.OrderNo = info.OrderNo;
            var list = iwareBLL.GetBuyOrderInfoList(be);
            List<int> ProductIDlist = new List<int>();
            List<int> Quatity = new List<int>();
            foreach (var infolist in list)
            {
                ProductIDlist.Add(infolist.ProductID);
                Quatity.Add(infolist.Quatity);
            }

            #region ==入库==
            StockOrderEntity entity = new StockOrderEntity();
            var thetype = iSysBLL.GetBaseInfoTreeByType("WarehouseIn", 1, LoginUserEntity.ParentHospitalID).Where(c => c.InfoName == "采购进货").ToList();//获取库存原因
            entity.BaseID = thetype.Count > 0 ? thetype[0].ID : 0;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.IsVerify = 2;//现在默认是通过的
            entity.Memo = "采购单入库";
            entity.OrderNo = RandomHelper.GetRandomStock("R");
            entity.OrderTime = DateTime.Now;
            entity.Type = 2;
            entity.Class = 1;

            entity.AllQuatity = info.AllQutity;
            int _quatity = 0;
            decimal _allprice = 0;
            entity.AllChengBen = GetAllPriceInPurchasing(ProductIDlist, Quatity, ref _quatity, ref _allprice);
            entity.AllMoney = _allprice;
            entity.DocumentNumber = GetDocumentNumber(entity.HospitalID, entity.Type);

            entity.UserID = LoginUserEntity.UserID;//经手人设置为自己
            entity.VerifyTime = DateTime.Now;//审核时间(现在没有什么用)
            var result = 1;// 0;
            //result = Convert.ToInt32(iwareBLL.AddStockOrder(entity));
            #endregion
            //if (result > 0)
            //{
            //    #region ==添加入库详情==
            //    StockOrderInfo sinfo = new StockOrderInfo();
            //    sinfo.StockOrderNo = entity.OrderNo;
            //    foreach (var so in list)
            //    {
            //        sinfo.ProductID = so.ProductID;
            //        sinfo.Number = so.Quatity;
            //        sinfo.HouseID = info.HouseID;
            //        iwareBLL.AddStockOrderInfo(sinfo);
            //        #region ==保存到库存==
            //        //先去库存里面找这个是否存在
            //        ProductStockEntity pse = new ProductStockEntity();
            //        pse.HospitalID = LoginUserEntity.HospitalID;
            //        pse.ProductID = sinfo.ProductID;
            //        pse.HouseID = info.HouseID;
            //        var stocklist = iwareBLL.GetProductStockListToseleect(pse);
            //        if (stocklist.Count == 0) //如果没有找到数据,表示原来没有库存,需要新增进去
            //        {
            //            StockChangeLogEntity scl = new StockChangeLogEntity();
            //            scl.UserID = LoginUserEntity.UserID;
            //            scl.LogClass = 4;
            //            scl.LogType = "入库";
            //            scl.LogTitle = "采购单单审核(原来仓库无此产品记录.).仓库ID" + pse.HouseID + ";产品ID:" + pse.ProductID + ";入库数量:" + sinfo.Number + ";";
            //            scl.NewQuatity = sinfo.Number;//当原来仓库没此产品记录时,自动添加一个记录
            //            scl.OldQuatity = 0;
            //            scl.Quantity = sinfo.Number;
            //            scl.ProductStockID = 0;
            //            scl.OwnedWarehouse = pse.HouseID;
            //            scl.ProductID = pse.ProductID;
            //            scl.OrderNo = entity.OrderNo + "(采购单:" + info.OrderNo + ")";

            //            var sclID = iwareBLL.AddStockChangeLog(scl);

            //            pse.Quatity = sinfo.Number;
            //            result = Convert.ToInt32(iwareBLL.AddProductStock(pse));//添加库存
            //            if (result > 0)
            //            {
            //                iwareBLL.UpdateStatuAndProductStockID(sclID, result);
            //            }
            //        }
            //        else//修改库存
            //        {
            //            StockChangeLogEntity scl = new StockChangeLogEntity();
            //            scl.UserID = LoginUserEntity.UserID;
            //            scl.LogClass = 4;
            //            scl.LogType = "入库";
            //            scl.LogTitle = "采购单单审核.仓库ID" + pse.HouseID + ";产品ID:" + pse.ProductID + ";入库数量:" + sinfo.Number + ";";
            //            scl.NewQuatity = sinfo.Number + stocklist[0].Quatity;//当原来仓库没此产品记录时,自动添加一个记录
            //            scl.OldQuatity = stocklist[0].Quatity;
            //            scl.OwnedWarehouse = pse.HouseID;
            //            scl.Quantity = sinfo.Number;
            //            scl.ProductStockID = stocklist[0].ID;
            //            scl.OrderNo = entity.OrderNo + "(采购单:" + info.OrderNo + ")";
            //            scl.ProductID = pse.ProductID;
            //            var sclID = iwareBLL.AddStockChangeLog(scl);

            //            pse.ID = stocklist[0].ID;
            //            pse.Quatity = sinfo.Number + stocklist[0].Quatity;
            //            result = iwareBLL.UpdateProductStock(pse);
            //            if (result > 0)
            //            {
            //                iwareBLL.UpdateStatu(sclID);
            //            }
            //        }
            //        #endregion
            //    }

            //    #endregion

            //    #region ==修改订单状态==

            //    iwareBLL.UpdateBuyOrderStatu(id, GetBuyOrderNumber(info.HospitalID, info.Type));
            //    #endregion

            //    #region ==添加库存动态==
            //    CompanyNewsEntity cust = new CompanyNewsEntity();
            //    cust.HospitalID = LoginUserEntity.HospitalID;
            //    cust.CreateTime = DateTime.Now;

            //    cust.HuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
            //    cust.NextHuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
            //    cust.Type = 4;
            //    //var hospuser = PersBLL.GetUserByUserID(entity.UserID);
            //    cust.UserName = info.HouseUserName;
            //    cust.ProjectName = entity.OrderNo;
            //    cust.Name = cust.UserName + "进行产品进货";
            //    iCentBLL.AddCompanyNewsEntity(cust);
            //    #endregion
            //}
            return Json(result);
        }

        /// <summary>
        /// 获取采购单今日第几比单的编号
        /// </summary>
        /// <param name="hospitalID"></param>
        /// <param name="ordertype"></param>
        /// <returns></returns>
        private string GetBuyOrderNumber(int hospitalID, int ordertype)
        {
            var result = "";
            var CountNumber = iwareBLL.GetTodayBuyOrderCount(hospitalID, ordertype);
            CountNumber++;
            switch (ordertype)
            {
                case 1: result = "IN"; break;
                case 2: result = "OUT"; break;
                default: result = "Q"; break;
            }
            result += DateTime.Today.ToString("yyyyMMdd");
            result += CountNumber.ToString().PadLeft(3, '0');
            return result;
        }

        /// <summary>
        /// 获取采购单内所有价格次数总和
        /// </summary>
        /// <param name="ProductIDList"></param>
        /// <returns></returns>
        public decimal GetAllPriceInPurchasing(List<int> ProductIDList, List<int> Quatity, ref int AllQuatity, ref decimal AllPrice)
        {
            decimal CostPrice = 0;
            int i = 0;
            foreach (var info in ProductIDList)
            {
                var product = baseinfoBLL.GetModel(info);
                if (product != null)
                {
                    CostPrice += Quatity[i] * product.CostPrice;//成本价
                    AllQuatity += Quatity[i];
                    AllPrice += Quatity[i] * product.RetailPrice;//零售价
                }
                i++;
            }
            return CostPrice;
        }

        public ActionResult Jinhuo1()
        {
            ProductStockEntity entity = new ProductStockEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 1000000;
            if (entity.StartTime.Year < 1980) { entity.StartTime = DateTime.Now.AddYears(-10); }
            if (entity.EndTime.Year < 1980) { entity.EndTime = DateTime.Now; }

            int rows;
            int pagecount;
            var list = iwareBLL.GetProductStockList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View(result);
        }

        public ActionResult GetHouseList()
        {
            bool HasPermission = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_采购进货成本价);
            ProductStockEntity entity = new ProductStockEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 1000000;
            if (entity.StartTime.Year < 1980) { entity.StartTime = DateTime.Now.AddYears(-10); }
            if (entity.EndTime.Year < 1980) { entity.EndTime = DateTime.Now; }

            entity.HouseID = Convert.ToInt32(Request["HouseID"]);
            entity.BrandID = Convert.ToInt32(Request["BrandID"]);
            int rows;
            int pagecount;
            var list = iwareBLL.GetProductStockList(entity, out rows, out pagecount);

            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var allkucun = iwareBLL.GetSumProduct(entity);


            StringBuilder str = new StringBuilder();
            foreach (var info in list)
            {
                str.Append("<tr>");
                str.AppendFormat("<td> <input type='hidden' name='ID' class='ID' value='{0}' /><input type='hidden' name='HouseID' class='HouseID' value='{1}' />{2}</td>", info.ProductID, info.HouseID, info.ProductName);
                str.AppendFormat(" <td>{0}</td>", info.HouseName);
                str.AppendFormat(" <td>{0}</td>", info.BrandName);

                str.AppendFormat("<td style='{1}' class='CostPrice'>{0}</td>", info.CostPrice, HasPermission ? "" : "display:none;");

                str.AppendFormat("<td>{0}</td>", info.Quatity);
                var kucun = allkucun.Where(i => i.ProductID == info.ProductID).Count() == 0 ? 0 : allkucun.Where(i => i.ProductID == info.ProductID).ToList()[0].Quatity;
                str.AppendFormat("<td>{0}</td>", kucun);
                str.AppendFormat("<td><input type='text' onkeyup='CheckInputIntFloat(this)' class='wid50 number' value='0' /></td>");

                str.AppendFormat("<td style='{0}'  class='Allprice'>0</td>", HasPermission ? "" : "display:none;");

                str.Append("</tr>");
            }
            return Content(str.ToString());
        }


        public ActionResult AddJinhuo1(BuyOrderEntity entity)
        {
            var IDlist = Request["IDlist"];
            var HouseIDList = Request["HouseIDList"];
            var numberlist = Request["numberlist"];

            string[] IDArray = IDlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] HouseIDArray = HouseIDList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] NumberArray = numberlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            int allnumber = 0;
            for (int i = 0; i < NumberArray.Count(); i++)
            {
                allnumber = allnumber + Convert.ToInt32(NumberArray[i]);
            }

            entity.AllQutity = allnumber;
            entity.CreateTime = DateTime.Now;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomStock("IN");
            entity.Statu = 1;
            entity.Type = 1;
            var result = iwareBLL.AddBuyOrder(entity);

            if (result > 0)
            {
                decimal bili = 0;
                if (entity.NeedMoney != 0)  //这个是比例判断
                {
                    bili = entity.ActualMoney / entity.NeedMoney;
                }
                else
                {
                    bili = 1;
                }
                BuyOrderInfoEntity info = new BuyOrderInfoEntity();
                info.HouseUserID = LoginUserEntity.HospitalID;
                info.OrderNO = entity.OrderNo;
                for (int i = 0; i < IDArray.Length; i++)
                {
                    info.HouseID = entity.HouseID;
                    info.ProductID = Convert.ToInt32(IDArray[i]);
                    info.Quatity = Convert.ToInt32(NumberArray[i]);
                    var product = baseinfoBLL.GetModel(info.ProductID);  //获取产品详情
                    info.OldPrice = product.CostPrice;
                    info.Price = info.OldPrice * bili;
                    info.DifferentMoney = (info.OldPrice - info.Price) * info.Quatity;
                    info.Allprice = info.OldPrice * info.Quatity;
                    info.ActualMoney = info.Price * info.Quatity;
                    info.NeedMoney = info.ActualMoney;
                    if (info.Quatity > 0)
                    {
                        iwareBLL.AddBuyOrderInfo(info);
                    }

                }
            }



            return Json(result);
        }

        public ActionResult JinhuoEdit()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            var ID = Convert.ToInt32(Request["ID"]);
            var order = iwareBLL.GetBuyOrderModel(new BuyOrderEntity { ID = ID });
            var list = iwareBLL.GetBuyOrderInfoList(new BuyOrderInfoEntity { OrderNO = order.OrderNo });
            var result = list.AsQueryable().ToPagedList(1, 10000);//添加Webdiyer.WebControls.Mvc应用

            return View(result);
        }

        public ActionResult EditJinhuo(BuyOrderEntity entity)
        {
            var result = 0;
            string[] ProductList = Request.Form.GetValues("ID"); //产品ID
            string[] pricelist = Request.Form.GetValues("Price");//价格
            string[] NumberList = Request.Form.GetValues("Number"); //数量集合
            var orderno = Request["OrderNo"];

            int allnumber = 0;
            for (int i = 0; i < NumberList.Count(); i++)
            {
                allnumber = allnumber + Convert.ToInt32(NumberList[i]);
            }

            entity.AllQutity = allnumber;

            entity.OrderNo = orderno;
            iwareBLL.UpdateBuyOrder(entity);
            BuyOrderInfoEntity bo = new BuyOrderInfoEntity();
            bo.OrderNO = entity.OrderNo;
            iwareBLL.DelBuyOrderInfo(entity.OrderNo);
            for (int i = 0; i < ProductList.Length; i++)
            {
                var product = baseinfoBLL.GetModel(Convert.ToInt32(ProductList[i]));  //获取产品详情
                bo.Quatity = Convert.ToInt32(NumberList[i]);
                bo.Price = product.CostPrice; //Convert.ToDecimal(pricelist[i]);
                bo.ProductID = Convert.ToInt32(ProductList[i]);
                bo.OldPrice = product.CostPrice;//现在只要零售价,不需要成本价
                bo.NeedMoney = bo.OldPrice * bo.Quatity;//预计所需价格
                bo.Allprice = bo.Price * bo.Quatity;//所有价格
                bo.DifferentMoney = bo.NeedMoney - bo.Allprice;   //这个是相差价格
                result = iwareBLL.AddBuyOrderInfo(bo);
            }
            return Json(result);
        }

        /// <summary>
        /// 撤销单据
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult OrderChexiao(int ID)
        {
            var result = 0;
            if (ID > 0)
            {
                result = iwareBLL.DelBuyOrder(ID);
            }
            return Json(result);

        }

        #endregion

        #region ==退货==
        public ActionResult Tuihuo()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }

        /// <summary>
        /// 退货单据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddTuihuo(BuyOrderEntity entity)
        {
            //由于退货现在不只是单据,需要关系库存,所以选择仓库了的时候要先判断仓库 的库存是否够
            // 如果库存够则成功添加退货单,如果不够则提示不够

            var result = 0;
            string[] ProductList = Request.Form.GetValues("ID"); //产品ID
            string[] pricelist = Request.Form.GetValues("Price");//价格
            string[] NumberList = Request.Form.GetValues("Number"); //数量集合

            int allnumber = 0;
            for (int i = 0; i < NumberList.Count(); i++)
            {
                allnumber = allnumber + Convert.ToInt32(NumberList[i]);
            }

            entity.AllQutity = allnumber;

            #region ==获取仓库的库存,然后判断是否足够==
            int Ispass = 0;

            ProductStockEntity pse = new ProductStockEntity();
            pse.HospitalID = LoginUserEntity.HospitalID;
            pse.HouseID = entity.HouseID;
            for (int i = 0; i < ProductList.Length; i++)
            {
                pse.ProductID = Convert.ToInt32(ProductList[i]);
                var list = iwareBLL.GetProductStockListToseleect(pse);
                if (list.Count > 0)
                {
                    if (list[0].Quatity >= Convert.ToInt32(NumberList[i]))
                    {
                        //如果把自己的门店ID,仓库和产品传入得到数据,
                        //并且出来的数量大于现有的,则通过加一
                        Ispass++;
                    }
                }
            }

            if (Ispass != ProductList.Length)
            {
                return Json(9);//如果返回9则表示库存不够
            }

            #endregion

            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.CreateTime = DateTime.Now;
            entity.Type = 2;
            entity.Statu = 1;
            entity.OrderNo = RandomHelper.GetRandomStock("Out");
            iwareBLL.AddBuyOrder(entity);
            BuyOrderInfoEntity bo = new BuyOrderInfoEntity();
            bo.OrderNO = entity.OrderNo;
            for (int i = 0; i < ProductList.Length; i++)
            {
                var product = baseinfoBLL.GetModel(Convert.ToInt32(ProductList[i]));  //获取产品详情
                bo.Quatity = Convert.ToInt32(NumberList[i]);
                bo.Price = product.CostPrice;// Convert.ToDecimal(pricelist[i]);
                bo.ProductID = Convert.ToInt32(ProductList[i]);
                bo.OldPrice = product.RetailPrice;//现在只要零售价,不需要成本价
                bo.NeedMoney = bo.OldPrice * bo.Quatity;//预计所需价格
                bo.Allprice = bo.Price * bo.Quatity;//所有价格
                bo.DifferentMoney = bo.NeedMoney - bo.Allprice;   //这个是相差价格
                result = iwareBLL.AddBuyOrderInfo(bo);

            }
            return Json(result);
        }

        /// <summary>
        /// 退货列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult TuihuoList(BuyOrderEntity entity)
        {
            ViewBag.Statu = entity.Statu;
            entity.StartTime = (entity.StartTime.Year < 1970 ? DateTime.Now.AddDays(-30) : entity.StartTime);
            entity.EndTime = (entity.EndTime.Year < 1970 ? DateTime.Now : entity.EndTime);

            entity.StartTime = Convert.ToDateTime(entity.StartTime.Year + "-" + entity.StartTime.Month + "-" + entity.StartTime.Day + " 01:01:01");
            entity.EndTime = Convert.ToDateTime(entity.EndTime.Year + "-" + entity.EndTime.Month + "-" + entity.EndTime.Day + " 23:59:59");

            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.Type = 2;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            entity.orderDirection = "desc";
            var list = iwareBLL.GetBuyOrderList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(entity.pageNum, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;

            var sumall = iwareBLL.getSumBuyOrder(entity);
            ViewBag.AllActualMoney = sumall.ActualMoney;      //总共实付总额
            ViewBag.AllNeedMoney = sumall.NeedMoney;         //应付总额
            entity.Statu = 2;//吧状态改为2,表示会已审核的记录
            var TSum = iwareBLL.getSumBuyOrder(entity);//获取已审核的记录
            ViewBag.TActualMoney = TSum.ActualMoney;      //已审核的实付总额
            ViewBag.TNeedMoney = TSum.NeedMoney;         //已审核的应付总额
            entity.Statu = 1;//吧状态改为1,表示未审核的记录
            var FSum = iwareBLL.getSumBuyOrder(entity);
            ViewBag.FActualMoney = FSum.ActualMoney;      //已审核的实付总额
            ViewBag.FNeedMoney = FSum.NeedMoney;         //已审核的应付总额
            ViewBag.StartTime = entity.StartTime.ToString("yyyy-MM-dd");
            ViewBag.EndTime = entity.EndTime.ToString("yyyy-MM-dd");
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_Tuihuolist", result);
            return View(result);

        }

        /// <summary>
        /// 退货详情
        /// </summary>
        /// <returns></returns>
        public ActionResult TuihuoInfo()
        {
            string id = Request["ID"];
            var info = iwareBLL.GetBuyOrderModel(new BuyOrderEntity { ID = Convert.ToInt32(id) });
            BuyOrderInfoEntity be = new BuyOrderInfoEntity();
            var list = iwareBLL.GetBuyOrderInfoList(new BuyOrderInfoEntity { OrderNO = info.OrderNo });
            var result = list.AsQueryable().ToPagedList(1, 10000);//添加Webdiyer.WebControls.Mvc应用
            return View(result);
        }

        /// <summary>
        /// 退货审核
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderCHuku()
        {
            int id = Convert.ToInt32(Request["ID"]);
            var info = iwareBLL.GetBuyOrderModel(new BuyOrderEntity { ID = Convert.ToInt32(id) });
            if (info.Statu != 1)
            {
                return Json(-2);
            }
            BuyOrderInfoEntity be = new BuyOrderInfoEntity();
            be.OrderNO = info.OrderNo;
            be.OrderNo = info.OrderNo;
            var list = iwareBLL.GetBuyOrderInfoList(be);
            List<int> ProductIDlist = new List<int>();
            List<int> Quatity = new List<int>();
            foreach (var infolist in list)
            {
                ProductIDlist.Add(infolist.ProductID);
                Quatity.Add(infolist.Quatity);
            }

            //if (TuiHuoYanZheng(list, id))
            //{

            //    #region ==出库==
            //    StockOrderEntity entity = new StockOrderEntity();
            //    var thetype = iSysBLL.GetBaseInfoTreeByType("WarehouseIn", 1, LoginUserEntity.ParentHospitalID).Where(c => c.InfoName == "采购退货").ToList();//获取库存原因
            //    entity.BaseID = thetype.Count > 0 ? thetype[0].ID : 0;
            //    entity.HospitalID = LoginUserEntity.HospitalID;
            //    entity.IsVerify = 2;//现在默认是通过的
            //    entity.Memo = "采购退货单出库";
            //    entity.OrderNo = RandomHelper.GetRandomStock("R");
            //    entity.OrderTime = DateTime.Now;
            //    entity.Type = 1;
            //    entity.Class = 3;
            //    entity.UserID = LoginUserEntity.UserID;//经手人设置为自己
            //    entity.VerifyTime = DateTime.Now;//审核时间(现在没有什么用)

            //    entity.AllQuatity = info.AllQutity;
            //    int _quatity = 0;
            //    decimal _allprice = 0;
            //    entity.AllChengBen = GetAllPriceInPurchasing(ProductIDlist, Quatity, ref _quatity, ref _allprice);
            //    entity.AllMoney = _allprice;
            //    entity.DocumentNumber = GetDocumentNumber(entity.HospitalID, entity.Type);
            //    var result = iwareBLL.AddStockOrder(entity);
            //    #endregion
            //    if (result > 0)
            //    {
            //        #region ==添加出库详情==
            //        StockOrderInfo sinfo = new StockOrderInfo();
            //        sinfo.StockOrderNo = entity.OrderNo;
            //        foreach (var so in list)
            //        {
            //            sinfo.ProductID = so.ProductID;
            //            sinfo.Number = so.Quatity;
            //            sinfo.HouseID = info.HouseID;
            //            iwareBLL.AddStockOrderInfo(sinfo);
            //            #region ==保存到库存==
            //            //先去库存里面找这个是否存在
            //            ProductStockEntity pse = new ProductStockEntity();
            //            pse.HospitalID = LoginUserEntity.HospitalID;
            //            pse.ProductID = sinfo.ProductID;
            //            pse.HouseID = info.HouseID;
            //            var stocklist = iwareBLL.GetProductStockListToseleect(pse);

            //            if (stocklist.Count == 0) //如果没有找到数据,表示原来没有库存,需要新增进去
            //            {
            //                //如果退换在仓库没有次产品,直接返回-1
            //                return Json(-1);

            //                //pse.Quatity = sinfo.Number;
            //                //iwareBLL.AddProductStock(pse);//添加库存
            //            }
            //            else if (stocklist[0].Quatity - sinfo.Number < 0)
            //            {
            //                return Json(-3);
            //            }
            //            else//修改库存
            //            {
            //                StockChangeLogEntity scl = new StockChangeLogEntity();
            //                scl.UserID = LoginUserEntity.UserID;
            //                scl.LogClass = 4;
            //                scl.LogType = "出库";
            //                scl.LogTitle = "采购单审核.仓库ID" + pse.HouseID + ";产品ID:" + pse.ProductID + ";出库数量:" + sinfo.Number + ";";
            //                scl.NewQuatity = stocklist[0].Quatity - sinfo.Number;
            //                scl.OldQuatity = stocklist[0].Quatity;
            //                scl.Quantity = sinfo.Number;
            //                scl.ProductStockID = stocklist[0].ID;
            //                scl.OwnedWarehouse = pse.HouseID;
            //                scl.ProductID = pse.ProductID;
            //                scl.OrderNo = entity.OrderNo + "(采购单:" + info.OrderNo + ")";
            //                var sclID = iwareBLL.AddStockChangeLog(scl);

            //                pse.ID = stocklist[0].ID;
            //                pse.Quatity = stocklist[0].Quatity - sinfo.Number;
            //                result = iwareBLL.UpdateProductStock(pse);
            //                if (result > 0)
            //                {
            //                    iwareBLL.UpdateStatu(sclID);

            //                    int len = 2;
            //                    var usersetting = iSysBLL.GetUserSettingsValue(LoginUserEntity.HospitalID, "StockNumber");
            //                    if (usersetting != null)
            //                    {
            //                        try
            //                        {
            //                            if (usersetting.AttributeValue != null)
            //                            {
            //                                len = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue);
            //                            }
            //                            else
            //                            {
            //                                len = 2;
            //                            }
            //                        }
            //                        catch (Exception)
            //                        {
            //                            len = 2;
            //                        }
            //                    }

            //                    if (pse.Quatity <= len)
            //                    {
            //                        var ProductModel = baseinfoBLL.GetModel(pse.ProductID);

            //                        TaskEntity te = new TaskEntity();
            //                        te.HospitalID = LoginUserEntity.HospitalID;
            //                        te.CreateTime = DateTime.Now;
            //                        te.Type = 4;
            //                        te.Statu = 1;
            //                        te.Name = LoginUserEntity.Name + "库存不足,请入库";
            //                        te.Content = "产品:" + ProductModel.Name + "库存为:" + pse.Quatity;
            //                        iCentBLL.AddTaskEntity(te);
            //                    }


            //                }
            //            }
            //            #endregion
            //        }

            //        #endregion

            //        #region ==修改订单状态==
            //        iwareBLL.UpdateBuyOrderStatu(id, GetBuyOrderNumber(info.HospitalID, info.Type));
            //        #endregion


            //        #region ==添加库存动态==
            //        CompanyNewsEntity cust = new CompanyNewsEntity();
            //        cust.HospitalID = LoginUserEntity.HospitalID;
            //        cust.CreateTime = DateTime.Now;
            //        cust.HuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
            //        cust.NextHuiFangTime = Convert.ToDateTime("1999-01-01 00:00:01");
            //        cust.Type = 4;
            //        //var hospuser = PersBLL.GetUserByUserID(entity.UserID);
            //        cust.UserName = info.HouseUserName;
            //        cust.ProjectName = entity.OrderNo;
            //        cust.Name = cust.UserName + "进行产品退货";
            //        iCentBLL.AddCompanyNewsEntity(cust);
            //        #endregion



            //    }
            //    return Json(result);
            //}
            //else
            //{
            //    return Json(-1);
            //}
            return Json(-1);
        }
        /// <summary>
        /// 采购退货验证
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool TuiHuoYanZheng(IList<BuyOrderInfoEntity> list, int id)
        {
            var info = iwareBLL.GetBuyOrderModel(new BuyOrderEntity { ID = id });
            foreach (var sinfo in list)
            {
                ProductStockEntity pse = new ProductStockEntity();
                pse.HospitalID = LoginUserEntity.HospitalID;
                pse.ProductID = sinfo.ProductID;
                pse.HouseID = info.HouseID;
                var stocklist = iwareBLL.GetProductStockListToseleect(pse);
                if (stocklist.Count == 0) //如果没有找到数据,表示原来没有库存,需要新增进去
                { return false; }
                else
                {
                    if (stocklist[0].Quatity < sinfo.Quatity)
                    { return false; }
                }

            }
            return true;
        }

        public ActionResult EditTuihuo(BuyOrderEntity entity)
        {
            var result = 0;
            string[] ProductList = Request.Form.GetValues("ID"); //产品ID
            string[] pricelist = Request.Form.GetValues("Price");//价格
            string[] NumberList = Request.Form.GetValues("Number"); //数量集合
            string OrderNo = Request["OrderNo"];

            int allnumber = 0;
            for (int i = 0; i < NumberList.Count(); i++)
            {
                allnumber = allnumber + Convert.ToInt32(NumberList[i]);
            }

            entity.AllQutity = allnumber;

            #region ==获取仓库的库存,然后判断是否足够==
            int Ispass = 0;

            ProductStockEntity pse = new ProductStockEntity();
            pse.HospitalID = LoginUserEntity.HospitalID;
            pse.HouseID = entity.HouseID;
            for (int i = 0; i < ProductList.Length; i++)
            {
                pse.ProductID = Convert.ToInt32(ProductList[i]);
                var list = iwareBLL.GetProductStockListToseleect(pse);
                if (list.Count > 0)
                {
                    if (list[0].Quatity >= Convert.ToInt32(NumberList[i]))
                    {
                        //如果把自己的门店ID,仓库和产品传入得到数据,
                        //并且出来的数量大于现有的,则通过加一
                        Ispass++;
                    }
                }
            }

            if (Ispass != ProductList.Length)
            {
                return Json(9);//如果返回9则表示库存不够
            }

            #endregion

            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.CreateTime = DateTime.Now;
            entity.OrderNo = OrderNo;
            iwareBLL.UpdateBuyOrder(entity);
            BuyOrderInfoEntity bo = new BuyOrderInfoEntity();
            bo.OrderNO = entity.OrderNo;
            iwareBLL.DelBuyOrderInfo(entity.OrderNo);
            for (int i = 0; i < ProductList.Length; i++)
            {
                var product = baseinfoBLL.GetModel(Convert.ToInt32(ProductList[i]));  //获取产品详情
                bo.Quatity = Convert.ToInt32(NumberList[i]);
                bo.Price = product.CostPrice;// Convert.ToDecimal(pricelist[i]);
                bo.ProductID = Convert.ToInt32(ProductList[i]);
                bo.OldPrice = product.RetailPrice;//现在只要零售价,不需要成本价
                bo.NeedMoney = bo.OldPrice * bo.Quatity;//预计所需价格
                bo.Allprice = bo.Price * bo.Quatity;//所有价格
                bo.DifferentMoney = bo.NeedMoney - bo.Allprice;   //这个是相差价格
                result = iwareBLL.AddBuyOrderInfo(bo);
            }
            return Json(result);
        }


        #endregion

        #endregion

        #region ==库存查询==
        public ActionResult WarehouseList(ProductStockEntity entity)
        {
            if (entity.StartTime.Year < 1980) { entity.StartTime = DateTime.Now.AddYears(-10); }
            if (entity.EndTime.Year < 1980) { entity.EndTime = DateTime.Now; }
            if (entity.ProductName == "搜索" && entity.ProductName == null) entity.ProductName = "";
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.ProductName))
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            entity.EndTime = Convert.ToDateTime(entity.EndTime.Year + "-" + entity.EndTime.Month + "-" + entity.EndTime.Day + " 23:59:59");
            if (entity.HouseID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;

            }
            else
            {
                var houseentity = iwareBLL.GetWarehouseModel(entity.HouseID);
                entity.HospitalID = houseentity.HospitalID;
            }

            var warelist = iwareBLL.GetWarehouseList(new WarehouseEntity {HospitalID = entity.HospitalID});
            entity.HouseID = warelist.Count > 0 ? warelist[0].ID : 0;

            ViewBag.HouseID = LoginUserEntity.HospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProductID"; }

            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
           // entity.ParentHospitalID = -1;
            var list = iwareBLL.GetProductStockList(entity, out rows, out pagecount);
            list = list.Where(t=>t.HospitalID==entity.HospitalID).ToList();
            //先默认查询出当前的余额
            //获取当前
            //  StockOrderInfo soi = new StockOrderInfo();
            var sum = iwareBLL.GetSumtStockList(entity);
            decimal lingshou = sum.RetailPrice;
            decimal chengben = sum.CostPrice;
            int allnumber = sum.Quatity;

            foreach (var info in list)
            {
                //soi.HouseID = info.HouseID;
                //soi.ProductID = info.ProductID;
                //soi.StartTime = Convert.ToDateTime("1970-01-01 01:01:01");
                //soi.EndTime = entity.EndTime;
                //var infolist = iwareBLL.GetStockOrderInfoByTime(soi); 这个是控制原来即时库存的,现在是当前库存
                //var rukusum = infolist.Where(t => t.Type == 3).Sum(t => t.Number);
                //var chukusum = infolist.Where(t => t.Type == 1).Sum(t => t.Number);
                //info.Quatity = info.Quatity - rukusum + chukusum;
                var model = baseinfoBLL.GetModel(info.ProductID);
                info.LingshouPrice = model.RetailPrice * info.Quatity;
                info.Chengbenprice = model.CostPrice * info.Quatity;
                if (info.HospitalID == 1167 && info.ProductID == 95719) {
                    info.Quatity = info.Quatity + 1;
                }

                //算零售价,成本价总额
                //lingshou =lingshou+info.LingshouPrice;
                //chengben=chengben+info.Chengbenprice ;
                //allnumber = allnumber + info.Quatity;
             
            }

            var result = list.ToPagedList(1, entity.numPerPage);
            ViewBag.LingshouPrice = lingshou;
            ViewBag.Chengbenprice = chengben;
            ViewBag.allnumber = allnumber;
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.Name = entity.ProductName;
            //ViewBag.HouseID = entity.HouseID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;

            if (Request.IsAjaxRequest())
            {
                ViewBag.LingshouPrice = lingshou;
                ViewBag.Chengbenprice = chengben;
                ViewBag.allnumber = allnumber;
                return PartialView("_WarehouseList", result);
            }
            return View(result);
        }
        private string GetIntegralByMonth(string sqlstr, int filter = 0)
        {

            string month = "";
            try
            {
                string conStr = "server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft";//连接字符串  
                SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                conText.Open();//打开数据库  
                string sqls = sqlstr;//创建统计语句  
                SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                SqlDataReader dr;//创建DataReader对象  
                dr = comText.ExecuteReader();//执行查询  
                while (dr.Read())//判断数据表中是否含有数据  
                {
                    var date = dr;
                    month =  date["Quatity"].ToString();
                }
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return month;

        }
        private IList<ProductStockYuejieEntity> GetIntegralByMonthjieEntity(string sqlstr, int filter = 0)
        {

            var list = new List<ProductStockYuejieEntity>();
            try
            {
                string conStr = "server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft";//连接字符串  
                SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                conText.Open();//打开数据库  
                string sqls = sqlstr;//创建统计语句  
                SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                SqlDataReader dr;//创建DataReader对象  
                dr = comText.ExecuteReader();//执行查询  


                while (dr.Read())//判断数据表中是否含有数据  
                {
                    list.Add(DataBindHelper.CreateEntity<ProductStockYuejieEntity>(dr));
                }
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return list;

        }
        /// <summary>
        /// 库存月结
        /// </summary>
        /// <returns></returns>
        public ActionResult KucunYuejie(ProductStockYuejieEntity entity)
        {

            if (entity.ProductName == "搜索" && entity.ProductName == null) entity.ProductName = "";
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.ProductName))
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            if (entity.HouseID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;

            }
            else
            {
                var houseentity = iwareBLL.GetWarehouseModel(entity.HouseID);
                entity.HospitalID = houseentity.HospitalID;
            }

            var warelist = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = entity.HospitalID });
            entity.HouseID = warelist.Count > 0 ? warelist[0].ID : 0;

            ViewBag.HouseID = LoginUserEntity.HospitalID;
            entity.numPerPage = 50; //每页显示条数
            string wherestr = "";
            if (entity.orderField + "" == "") { entity.orderField = "ProductID"; }

            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            // entity.ParentHospitalID = -1;
            var list = new List<ProductStockYuejieEntity>();  //iwareBLL.GetProductStockList(entity, out rows, out pagecount);
            //list = list.Where(t => t.HospitalID == entity.HospitalID).ToList();
            if (entity.Year > 0 && entity.Month > 0)
            {
                if (!string.IsNullOrEmpty(entity.ProductName))
                {
                    wherestr += " and t.NAME like '%" + entity.ProductName + "%'";
                }
                var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Year), Convert.ToInt32(entity.Month));
                var starttime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-01 00:00:01").ToString()).AddMonths(-1);
                var starttime1 = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-01 00:00:01").ToString());

                var endtime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-" + days + " 23:59:59").ToString()).AddMonths(-1);
                var endtime1 = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-" + days + " 23:59:59").ToString());
                if (entity.orderField + "" == "") { entity.orderField = "ProductName"; }
                string strsql = @"
SELECT *,
isnull((select top 1  a.NewQuatity as Quatity from  
                                                  dbo.EYB_TB_StockChangeLog a  
																              left join dbo.EYB_tb_Product b on a.ProductID=b.ID          
                                              left join  dbo.EYB_tb_Warehouse c on a.OwnedWarehouse=c.ID          
                                              left join  dbo.EYB_tb_Warehouse d on a.IntoWarehouse=D.ID           
                                              left join dbo.EYB_tb_HospitalUser e on a.UserID=e.UserID
                                              left join EYB_TB_StockOrder S on a.OrderNo=s.OrderNo
	                                            LEFT JOIN EYB_tb_Order o on o.OrderNo = a.OrderNo
	                                            LEFT JOIN  EYB_tb_Patient p on p.UserID = o.UserID									
                                             WHERE a.ProductID =  ht.ID  and a.CreateTime >'"+ starttime + @"' and a.CreateTime <'" + endtime + @"'
																					   and ( IntoWarehouse="+ entity.HouseID+ @" or OwnedWarehouse=" + entity.HouseID + @")	
																					   ORDER BY a.CreateTime DESC 
                                               ),0) as QichuKucun,
isnull((select  SUM(isnull(Quantity,0)) as Quatity  from  
                                            dbo.EYB_TB_StockChangeLog a  
													                    left join dbo.EYB_tb_Product b on a.ProductID=b.ID          
                                              left join  dbo.EYB_tb_Warehouse c on a.OwnedWarehouse=c.ID          
                                              left join  dbo.EYB_tb_Warehouse d on a.IntoWarehouse=D.ID           
                                              left join dbo.EYB_tb_HospitalUser e on a.UserID=e.UserID
                                              left join EYB_TB_StockOrder S on a.OrderNo=s.OrderNo
	                                            LEFT JOIN EYB_tb_Order o on o.OrderNo = a.OrderNo
	                                            LEFT JOIN  EYB_tb_Patient p on p.UserID = o.UserID									
                                            where 1=1 and a.ProductID =  ht.ID   and a.CreateTime >'" + starttime1 + @"' and a.CreateTime <'" + endtime1 + @"'  and a.LogType = '入库'  
																						and ( IntoWarehouse=" + entity.HouseID + @" or OwnedWarehouse=" + entity.HouseID + @")  ),0) as Jinku ,
isnull((select  SUM(Quantity) as Quatity  from  
                                            dbo.EYB_TB_StockChangeLog a  
													                    left join dbo.EYB_tb_Product b on a.ProductID=b.ID          
                                              left join  dbo.EYB_tb_Warehouse c on a.OwnedWarehouse=c.ID          
                                              left join  dbo.EYB_tb_Warehouse d on a.IntoWarehouse=D.ID           
                                              left join dbo.EYB_tb_HospitalUser e on a.UserID=e.UserID
                                              left join EYB_TB_StockOrder S on a.OrderNo=s.OrderNo
	                                            LEFT JOIN EYB_tb_Order o on o.OrderNo = a.OrderNo
	                                            LEFT JOIN  EYB_tb_Patient p on p.UserID = o.UserID									
                                            where 1=1 and a.ProductID =  ht.ID   and a.CreateTime >'" + starttime1 + @"' and a.CreateTime <'" + endtime1 + @"'  and a.LogType = '出库'  
																						and ( IntoWarehouse=" + entity.HouseID + @" or OwnedWarehouse=" + entity.HouseID + @")   ),0) as Chuku 
																						from (
SELECT top 50
	t.ID,p.Month,p.Year,p.HouseID,p.HospitalID,
	t.NAME AS ProductName,
	h.NAME AS HospitalName,
	h.ParentID 
FROM
	EYB_tb_ProductStockYuejie p
	INNER JOIN EYB_tb_Product t ON p.ProductID = t.ID
	INNER JOIN EYB_tb_Hospital h ON p.HospitalID = h.ID
	INNER JOIN EYB_tb_Warehouse w ON w.ID = p.HouseID
	WHERE HouseID = " + entity.HouseID + @"  and ParentID= " + entity.ParentHospitalID + @"  and Month= " + entity.Month + @"  and Year=  " + entity.Year + wherestr + @"   and p.HospitalID = " + entity.HospitalID + @"  
	) ht  ORDER BY ID ";

                list = GetIntegralByMonthjieEntity(strsql).ToList();
                list = list.Skip(entity.numPerPage * (entity.pageNum - 1)).Take(entity.numPerPage).ToList();
            }
            var result = list.ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = list.Count();
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.Name = entity.ProductName;
            //ViewBag.HouseID = entity.HouseID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_KucunYuejie", result);
            }
            return View(result);
        }
        /// <summary>
        /// 库存月结
        /// </summary>
        /// <returns></returns>
        public ActionResult KucunYuejie23(ProductStockYuejieEntity entity)
        {
            if (LoginUserEntity.HospitalID == 571 || LoginUserEntity.HospitalID == 572 )
            {
                if (entity.ProductName == "搜索" && entity.ProductName == null) entity.ProductName = "";
                int rows;
                int pagecount;
                if (!String.IsNullOrEmpty(entity.ProductName))
                {
                    entity.ProductName = entity.ProductName.ToUpper();
                }
                if (entity.HouseID == 0)
                {
                    entity.HospitalID = LoginUserEntity.HospitalID;

                }
                else
                {
                    var houseentity = iwareBLL.GetWarehouseModel(entity.HouseID);
                    entity.HospitalID = houseentity.HospitalID;
                }

                var warelist = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = entity.HospitalID });
                entity.HouseID = warelist.Count > 0 ? warelist[0].ID : 0;

                ViewBag.HouseID = LoginUserEntity.HospitalID;
                entity.numPerPage = 10; //每页显示条数
                if (entity.orderField + "" == "") { entity.orderField = "ProductID"; }

                entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                // entity.ParentHospitalID = -1;
                var list = iwareBLL.GetProductStockList(entity, out rows, out pagecount);
                list = list.Where(t => t.HospitalID == entity.HospitalID).ToList();
                if (entity.Year > 0 && entity.Month > 0)
                {
                    var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Year), Convert.ToInt32(entity.Month));
                    var starttime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-01 00:00:01").ToString()).AddMonths(-1);
                    var starttime1 = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-01 00:00:01").ToString());

                    var endtime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-" + days + " 23:59:59").ToString()).AddMonths(-1);
                    var endtime1 = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-" + days + " 23:59:59").ToString());
                    if (entity.orderField + "" == "") { entity.orderField = "ProductName"; }
                    foreach (var item in list)
                    {
                        try
                        {
                            //string strsql = @"SELECT
                            //                    a.CreateTime,
                            //                    a.*,
                            //                      a.Quatity
                            //                    FROM
                            //                     EYB_tb_ProductStock a
                            //                    INNER JOIN EYB_tb_Product b ON a.ProductID = b.ID
                            //                    AND b.IsShow = 2
                            //                    INNER JOIN EYB_tb_Warehouse c ON a.HouseID = c.ID
                            //                    LEFT JOIN EYB_tb_ProductType d ON b.ProductType = d.ID
                            //                    LEFT JOIN EYB_tb_Supplier e ON b.SupplierID = e.ID
                            //                    LEFT JOIN EYB_tb_Brand f ON b.BrandID = f.ID
                            //                    LEFT JOIN EYB_dict_Baseinfo g ON b.StandardUnit = g.ID
                            //                    LEFT JOIN EYB_tb_Hospital h ON a.HospitalID = h.ID 
                            //                    where a.CreateTime > '" + starttime + @"' AND a.CreateTime < '" + endtime + @"' AND a.HospitalID = " + entity.HospitalID + @" and a.ProductID = " + item.ProductID;
                            string strsql = @"
                                             select top 1  a.NewQuatity as Quatity from  
                                                  dbo.EYB_TB_StockChangeLog a           
                                              left join dbo.EYB_tb_Product b on a.ProductID=b.ID          
                                              left join  dbo.EYB_tb_Warehouse c on a.OwnedWarehouse=c.ID          
                                              left join  dbo.EYB_tb_Warehouse d on a.IntoWarehouse=D.ID           
                                              left join dbo.EYB_tb_HospitalUser e on a.UserID=e.UserID
                                              left join EYB_TB_StockOrder S on a.OrderNo=s.OrderNo
	                                            LEFT JOIN EYB_tb_Order o on o.OrderNo = a.OrderNo
	                                            LEFT JOIN  EYB_tb_Patient p on p.UserID = o.UserID
                                               where 1=1 and a.ProductID= " + item.ProductID + @"

                                                and a.CreateTime > '" + starttime + @"' AND a.CreateTime < '" + endtime + @"'
	                                            and ( IntoWarehouse='" + entity.HouseID + "' or OwnedWarehouse='" + entity.HouseID + "') ORDER BY a.CreateTime DESC";
                            var ss = GetIntegralByMonth(strsql, 1);
                            var kk = ss == "" ? 0 : Convert.ToInt32(GetIntegralByMonth(strsql, 1));
                            item.QichuKucun = kk;
                            string strruku = @"select  SUM(Quantity) as Quatity  from  
                                            dbo.EYB_TB_StockChangeLog a           
                                            left join dbo.EYB_tb_Product b on a.ProductID=b.ID          
                                            left join  dbo.EYB_tb_Warehouse c on a.OwnedWarehouse=c.ID          
                                            left join  dbo.EYB_tb_Warehouse d on a.IntoWarehouse=D.ID           
                                            left join dbo.EYB_tb_HospitalUser e on a.UserID=e.UserID
                                            left join EYB_TB_StockOrder S on a.OrderNo=s.OrderNo
                                            LEFT JOIN EYB_tb_Order o on o.OrderNo = a.OrderNo
                                            LEFT JOIN  EYB_tb_Patient p on p.UserID = o.UserID
                                            where 1=1 and a.ProductID= " + item.ProductID + @"
                                            and a.CreateTime  > '" + starttime1 + @"' AND a.CreateTime < '" + endtime1 + @"'
                                            and ( IntoWarehouse='" + entity.HouseID + "' or OwnedWarehouse='" + entity.HouseID + "') and a.LogType = '入库' ";
                            ss = GetIntegralByMonth(strruku, 1);
                            kk = ss == "" ? 0 : Convert.ToInt32(GetIntegralByMonth(strruku, 1));
                            item.Jinku = kk;

                            strruku = @"select  SUM(Quantity) as Quatity  from  
                                            dbo.EYB_TB_StockChangeLog a           
                                            left join dbo.EYB_tb_Product b on a.ProductID=b.ID          
                                            left join  dbo.EYB_tb_Warehouse c on a.OwnedWarehouse=c.ID          
                                            left join  dbo.EYB_tb_Warehouse d on a.IntoWarehouse=D.ID           
                                            left join dbo.EYB_tb_HospitalUser e on a.UserID=e.UserID
                                            left join EYB_TB_StockOrder S on a.OrderNo=s.OrderNo
                                            LEFT JOIN EYB_tb_Order o on o.OrderNo = a.OrderNo
                                            LEFT JOIN  EYB_tb_Patient p on p.UserID = o.UserID
                                            where 1=1 and a.ProductID= " + item.ProductID + @"
                                            and a.CreateTime  > '" + starttime1 + @"' AND a.CreateTime < '" + endtime1 + @"'
                                            and ( IntoWarehouse='" + entity.HouseID + "' or OwnedWarehouse='" + entity.HouseID + "') and a.LogType = '出库' ";
                            ss = GetIntegralByMonth(strruku, 1);
                            kk = ss == "" ? 0 : Convert.ToInt32(GetIntegralByMonth(strruku, 1));
                            item.Chuku = kk;


                        }
                        catch
                        {
                        }
                    }
                }
                var result = list.ToPagedList(1, entity.numPerPage);
                result.TotalItemCount = rows;
                result.CurrentPageIndex = entity.pageNum;
                ViewBag.orderField = entity.orderField;
                ViewBag.orderDirection = entity.orderDirection;
                ViewBag.Name = entity.ProductName;
                //ViewBag.HouseID = entity.HouseID;
                ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_KucunYuejie", result);
                }
                return View(result);
            }
            else
            {
                int rows;
                int pagecount;
                ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
                entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                entity.numPerPage = 10; //每页显示条数
                entity.pageNum = 2;
                ViewBag.HouseID = LoginUserEntity.HospitalID;
                var list = iwareBLL.GetProductStockList(entity, out rows, out pagecount);
                var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
                #region
                //if (entity.Year > 0 && entity.Month > 0)
                //{
                //    var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Year), Convert.ToInt32(entity.Month));
                //    var starttime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-01 00:00:01").ToString()).AddMonths(-1);
                //    var endtime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-" + days + " 23:59:59").ToString()).AddMonths(-1);
                //    if (entity.orderField + "" == "") { entity.orderField = "ProductName"; }
                //    foreach (var item in list)
                //    {
                //        try
                //        {
                //            string strsql = @"SELECT
                //                                a.CreateTime,
                //                                a.*,
                //                                  a.Quatity
                //                                FROM
                //                                 EYB_tb_ProductStock a
                //                                INNER JOIN EYB_tb_Product b ON a.ProductID = b.ID
                //                                AND b.IsShow = 2
                //                                INNER JOIN EYB_tb_Warehouse c ON a.HouseID = c.ID
                //                                LEFT JOIN EYB_tb_ProductType d ON b.ProductType = d.ID
                //                                LEFT JOIN EYB_tb_Supplier e ON b.SupplierID = e.ID
                //                                LEFT JOIN EYB_tb_Brand f ON b.BrandID = f.ID
                //                                LEFT JOIN EYB_dict_Baseinfo g ON b.StandardUnit = g.ID
                //                                LEFT JOIN EYB_tb_Hospital h ON a.HospitalID = h.ID 
                //                                where a.CreateTime > '" + starttime + @"' AND a.CreateTime < '" + endtime + @"' AND a.HospitalID = " + entity.HospitalID + @" and a.ProductID = " + item.ProductID;
                //            var kk = Convert.ToInt32(GetIntegralByMonth(strsql, 1));
                //            item.QichuKucun = kk;
                //        }
                //        catch
                //        {
                //        }
                //    }
                //}
                #endregion
                result.TotalItemCount = rows;
                //result.CurrentPageIndex = entity.pageNum;
                ViewBag.orderField = entity.orderField;
                ViewBag.orderDirection = entity.orderDirection;
                if (Request.IsAjaxRequest())
                {

                    return PartialView("_KucunYuejie", result);
                }
                return View(result);

            }
        }
        /// <summary>
        /// 库存月结
        /// </summary>
        /// <returns></returns>
        public ActionResult KucunYuejie1(ProductStockYuejieEntity entity)
        {

            int rows;
            int pagecount;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 10; //每页显示条数
            entity.pageNum = 2;
            ViewBag.HouseID = LoginUserEntity.HospitalID;
            var list = iwareBLL.GetProductStockList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            #region
            //if (entity.Year > 0 && entity.Month > 0)
            //{
            //    var days = DateTime.DaysInMonth(Convert.ToInt32(entity.Year), Convert.ToInt32(entity.Month));
            //    var starttime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-01 00:00:01").ToString()).AddMonths(-1);
            //    var endtime = Convert.ToDateTime(("" + Convert.ToInt32(entity.Year) + "-" + Convert.ToInt32(entity.Month) + "-" + days + " 23:59:59").ToString()).AddMonths(-1);
            //    if (entity.orderField + "" == "") { entity.orderField = "ProductName"; }
            //    foreach (var item in list)
            //    {
            //        try
            //        {
            //            string strsql = @"SELECT
            //                                a.CreateTime,
            //                                a.*,
            //                                  a.Quatity
            //                                FROM
            //                                 EYB_tb_ProductStock a
            //                                INNER JOIN EYB_tb_Product b ON a.ProductID = b.ID
            //                                AND b.IsShow = 2
            //                                INNER JOIN EYB_tb_Warehouse c ON a.HouseID = c.ID
            //                                LEFT JOIN EYB_tb_ProductType d ON b.ProductType = d.ID
            //                                LEFT JOIN EYB_tb_Supplier e ON b.SupplierID = e.ID
            //                                LEFT JOIN EYB_tb_Brand f ON b.BrandID = f.ID
            //                                LEFT JOIN EYB_dict_Baseinfo g ON b.StandardUnit = g.ID
            //                                LEFT JOIN EYB_tb_Hospital h ON a.HospitalID = h.ID 
            //                                where a.CreateTime > '" + starttime + @"' AND a.CreateTime < '" + endtime + @"' AND a.HospitalID = " + entity.HospitalID + @" and a.ProductID = " + item.ProductID;
            //            var kk = Convert.ToInt32(GetIntegralByMonth(strsql, 1));
            //            item.QichuKucun = kk;
            //        }
            //        catch
            //        {
            //        }
            //    }
            //}
            #endregion
            result.TotalItemCount = rows;
            //result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            if (Request.IsAjaxRequest())
            {

                return PartialView("_KucunYuejie", result);
            }
            return View(result);
        }

        /// <summary>
        /// 库存月结采集数据
        /// </summary>
        /// <returns></returns>
        public ActionResult AddProductStockYuejiebyHospital(ProductStockYuejieEntity entity)
        {
            var result = iwareBLL.AddProductStockYuejiebyHospital(entity);
            return Json(result);
        }
        public ActionResult _WarehouseList()
        {

            return View();
        }

        /// <summary>
        /// 库存详情
        /// </summary>
        /// <returns></returns>
        public ActionResult WarehouseInfo()
        {
            var id = Request["ID"];
            var st = Request["St"];
            var et = Request["Et"];
            var type = Convert.ToInt32(Request["TheType"]);
            var info = iwareBLL.GetProductStockModel(new ProductStockEntity { ID = Convert.ToInt32(id) });
            StockOrderInfo so = new StockOrderInfo();
            so.StartTime = DateTime.Now.AddDays(-30);
            so.EndTime = DateTime.Now;
            if (st != null && st != "")
            {
                so.StartTime = Convert.ToDateTime(st);
            }
            if (et != null && et != "")
            {
                so.EndTime = Convert.ToDateTime(Convert.ToDateTime(et).Year + "-" + Convert.ToDateTime(et).Month + "-" + Convert.ToDateTime(et).Day + " 23:59:59");
            }
            if (type > 0)
            {
                so.Type = type;
            }

            so.ProductID = info.ProductID;
            so.HouseID = info.HouseID;
            var list = iwareBLL.GetStockOrderInfoByTime(so);
            var chuku = list.Where(i => i.Type == 1).Sum(i => i.Number);
            var ruku = list.Where(i => i.Type == 2).Sum(i => i.Number);
            ViewBag.Ruku = ruku;
            ViewBag.Chuku = chuku;
            var result = list.AsQueryable().ToPagedList(1, 100000);
            ViewBag.St = so.StartTime.ToString("yyyy-MM-dd");
            ViewBag.Et = so.EndTime.ToString("yyyy-MM-dd");
            ViewBag.ID = id;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_WarehouseInfo", result);

            }
            return View(result);

        }

        public ActionResult _WarehouseInfo()
        {
            return View();
        }


        #endregion

        #region ==批发销售==
        public ActionResult WholesaleSalesManagement()
        {
            return View();
        }

        #region ==销售单==
        public ActionResult Xiaoshou()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }

        /// <summary>
        /// 添加销售单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddXiaoshou(BuyOrderEntity entity)
        {
            var result = 0;
            string[] ProductList = Request.Form.GetValues("ID"); //产品ID
            string[] pricelist = Request.Form.GetValues("Price");//价格
            string[] NumberList = Request.Form.GetValues("Number"); //数量集合
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.CreateTime = DateTime.Now;
            entity.Type = 3;
            entity.OrderNo = RandomHelper.GetRandomStock("Out");
            iwareBLL.AddBuyOrder(entity);
            BuyOrderInfoEntity bo = new BuyOrderInfoEntity();
            bo.OrderNO = entity.OrderNo;
            for (int i = 0; i < ProductList.Length; i++)
            {
                var product = baseinfoBLL.GetModel(Convert.ToInt32(ProductList[i]));  //获取产品详情
                bo.Quatity = Convert.ToInt32(NumberList[i]);
                bo.Price = Convert.ToDecimal(pricelist[i]);
                bo.ProductID = Convert.ToInt32(ProductList[i]);
                bo.OldPrice = product.RetailPrice;//现在只要零售价,不需要成本价
                bo.NeedMoney = bo.OldPrice * bo.Quatity;//预计所需价格
                bo.Allprice = bo.Price * bo.Quatity;//所有价格
                bo.DifferentMoney = bo.NeedMoney - bo.Allprice;   //这个是相差价格
                result = iwareBLL.AddBuyOrderInfo(bo);
            }
            return Json(result);
        }

        public ActionResult XiaoshouList(BuyOrderEntity entity)
        {
            int rows;
            int pagecount;

            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = iwareBLL.GetBuyOrderList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_XiaoshouList", result);

            }
            return View(result);


        }

        public ActionResult _XiaoshouList()
        {
            return View();
        }

        #endregion

        #region ==销售退货单==
        public ActionResult TuiXiaoshou()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }

        /// <summary>
        /// 添加销售退货单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddTuiXiaoshou(BuyOrderEntity entity)
        {
            var result = 0;
            string[] ProductList = Request.Form.GetValues("ID"); //产品ID
            string[] pricelist = Request.Form.GetValues("Price");//价格
            string[] NumberList = Request.Form.GetValues("Number"); //数量集合
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.CreateTime = DateTime.Now;
            entity.Type = 4;
            entity.OrderNo = RandomHelper.GetRandomStock("Out");
            iwareBLL.AddBuyOrder(entity);
            BuyOrderInfoEntity bo = new BuyOrderInfoEntity();
            bo.OrderNO = entity.OrderNo;
            for (int i = 0; i < ProductList.Length; i++)
            {
                var product = baseinfoBLL.GetModel(Convert.ToInt32(ProductList[i]));  //获取产品详情
                bo.Quatity = Convert.ToInt32(NumberList[i]);
                bo.Price = product.CostPrice; //Convert.ToDecimal(pricelist[i]);
                bo.ProductID = Convert.ToInt32(ProductList[i]);
                bo.OldPrice = product.RetailPrice;//现在只要零售价,不需要成本价
                bo.NeedMoney = bo.OldPrice * bo.Quatity;//预计所需价格
                bo.Allprice = bo.Price * bo.Quatity;//所有价格
                bo.DifferentMoney = bo.NeedMoney - bo.Allprice;   //这个是相差价格
                result = iwareBLL.AddBuyOrderInfo(bo);
            }
            return Json(result);
        }
        #endregion

        #endregion

        #region ==供应商==
        public ActionResult _TheSupplierList()
        {
            return View();
        }

        public ActionResult TheSupplierList(SupplierEntity entity)
        {
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 100; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = iwareBLL.GetSupplierList1(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            if (Request.IsAjaxRequest())
                return PartialView("_TheSupplierList", result);
            return View(result);
        }

        public ActionResult AddSupplier()
        {
            return View();
        }

        /// <summary>
        /// 添加供应商
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SupplierADD(SupplierEntity entity)
        {
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.Name != "")
            {
                return Json(iwareBLL.AddSupplier(entity));

            }
            else
            {
                return Json(0);
            }
        }

        /// <summary>
        /// 供应商管理
        /// </summary>
        /// <returns></returns>
        public ActionResult SupplierListEdit()
        {
            var ID = Convert.ToInt32(Request["ID"]);
            var info = iwareBLL.GetSupplierModel(new SupplierEntity { ID = ID });
            return View(info);
        }

        /// <summary>
        /// 供应商修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SupplierEdit(SupplierEntity entity)
        {
            var result = 0;
            if (entity.ID > 0)
            {
                result = iwareBLL.UpdateSupplier(entity);
            }

            return Json(result);
        }


        #endregion

        #region ==进出仓汇总==

        public ActionResult WarehouseSummary(ProductStockEntity entity)
        {
            var list = new List<ProductStockEntity>();
            var result = list.AsQueryable().ToPagedList(1, 0);
            result.TotalItemCount = 0;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = 0;
            ViewBag.orderDirection = 0;

            return View();
        }

        public ActionResult WarehouseSummaryPage(ProductStockEntity entity)
        {
            if (entity.StartTime.Year < 1980) { entity.StartTime = DateTime.Now.AddDays(-30); }
            if (entity.EndTime.Year < 1980) { entity.EndTime = DateTime.Now; }
            entity.EndTime = Convert.ToDateTime(entity.EndTime.Year + "-" + entity.EndTime.Month + "-" + entity.EndTime.Day + " 23:59:59");
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 50;
            entity.orderDirection = "desc";
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = iwareBLL.GetProductStockList(entity, out rows, out pagecount);  //先获取库存列表
            var newList = new List<WarehouseSummaryEntity>();



            StockOrderInfo soi = new StockOrderInfo();
            var AllInQuantity = 0;    //入库总数量
            var AllOutQuantity = 0;  //出库总数量
            decimal AllInMoney = 0;//总入库金额(零售价计算)
            decimal AllOutMoney = 0;//总出库金额(零售价计算)

            foreach (var info in list)
            {
                soi.HouseID = info.HouseID;
                soi.ProductID = info.ProductID;
                soi.StartTime = entity.StartTime;
                soi.EndTime = entity.EndTime;
                var infolist = iwareBLL.GetStockOrderInfoByTime(soi);
                var rukusum = infolist.Where(t => t.Type == 2).Sum(t => t.Number);
                var chukusum = infolist.Where(t => t.Type == 1).Sum(t => t.Number);
                //  info.Quatity = info.Quatity - rukusum + chukusum;
                info.Quatity = rukusum - chukusum;

                info.INQuatity = rukusum;
                info.OutQuatity = chukusum;
                foreach (var xiangqing in infolist)
                {
                    if (xiangqing.Type == 1)
                    {
                        AllOutQuantity = AllOutQuantity + xiangqing.Number;
                        AllOutMoney = AllOutMoney + xiangqing.Number * xiangqing.RetailPrice;
                    }
                    else
                    {
                        AllInQuantity = AllInQuantity + xiangqing.Number;
                        AllInMoney = AllInMoney + xiangqing.Number * xiangqing.RetailPrice;
                    }
                }
            }
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.StartTime = entity.StartTime.ToString("yyyy-MM-dd");
            ViewBag.EndTime = entity.EndTime.ToString("yyyy-MM-dd");
            ViewBag.AllInQuantity = AllInQuantity;    //入库总数量
            ViewBag.AllOutQuantity = AllOutQuantity;  //出库总数量
            ViewBag.AllInMoney = AllInMoney;//总入库金额(零售价计算)
            ViewBag.AllOutMoney = AllOutMoney;//总出库金额(零售价计算)
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_WarehouseSummary", result);
            return View(result);
            //首先获取库存

        }

        #endregion

        #region ==产品明细汇总==
        public ActionResult ProductSummary(StockOrderInfo entity)
        {
            if (entity.StartTime.Year < 1980) { entity.StartTime = DateTime.Now.AddDays(-1); }
            if (entity.EndTime.Year < 1980) { entity.EndTime = DateTime.Now; }
            entity.EndTime = Convert.ToDateTime(entity.EndTime.Year + "-" + entity.EndTime.Month + "-" + entity.EndTime.Day + " 23:59:59");
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            if (entity.ProductName != null)
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.HouseID == 0)
            {
                entity.HouseID = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = LoginUserEntity.HospitalID })[0].ID;
            }
            if (!string.IsNullOrEmpty(entity.TransferHouseName))
            {
                if (entity.TransferHouseName.Length > 2)
                {
                    var arr = entity.TransferHouseName.Split(',');
                    entity.Class = ConvertValueHelper.ConvertIntValue(arr[1]);
                    entity.Type = ConvertValueHelper.ConvertIntValue(arr[0]);
                }
            }

            entity.numPerPage = 50;
            int rows = 0;
            int pagecount = 0;

            var list = iwareBLL.GetStockOrderInfoListinSelect(entity, out rows, out pagecount);
            var sum = iwareBLL.GetStockOrderInfoSum(entity)[0];
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.EndTime = entity.EndTime.ToString("yyyy-MM-dd");
            ViewBag.StartTime = entity.StartTime.ToString("yyyy-MM-dd");
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.Name = entity.ProductName;
            ViewBag.AllNumber = sum.Number;
            ViewBag.AllPrice = sum.RetailPrice;
            ViewBag.cb = sum.CostPrice;
            ViewBag.HouseID = entity.HouseID;
            if (Request.IsAjaxRequest())
                return PartialView("_ProductSummary", result);
            return View(result);

        }

        public ActionResult _ProductSummary()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();

        }


        #endregion

        #region ==仓库编辑==
        public ActionResult EditWareHouse(WarehouseEntity entity)
        {
            var result = 0;
            if (entity.ID != 0)
            {
                var info = iwareBLL.GetWarehouseModel(entity.ID);
                info.Name = entity.Name;
                result = iwareBLL.UpdateWarehouse(info);
            }
            else
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
                entity.IsActive = 1;
                entity.IsDefault = 1;
                entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                result = Convert.ToInt32(iwareBLL.AddWarehouse(entity));
            }

            return Json(result);
        }

        public ActionResult WarehouseNameEdit()
        {
            var list = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = LoginUserEntity.HospitalID });
            if (list != null && list.Count > 0)
            {
                return View(list[0]);
            }
            else
            {
                return View(new WarehouseEntity { Name = "没有找到仓库" });
            }
        }


        #endregion

        #region ==出入库汇总==

        public ActionResult StorageSummary()
        {
            StockOrderEntity entity = new StockOrderEntity();
            entity.StartTime = Convert.ToDateTime(Request["StartTime"]);
            entity.EndTime = Convert.ToDateTime(Request["EndTime"]);
            entity.BrandID = Convert.ToInt32(Request["BrandID"]);
            entity.HouseID = Convert.ToInt32(Request["HouseID"]);
            entity.Type = Convert.ToInt32(Request["Type"]);
            entity.ProductName = Request["ProductName"];
            entity.pageNum = Convert.ToInt32(Request["pageNum"]) == 0 ? 1 : Convert.ToInt32(Request["pageNum"]);
            entity.EndTime = entity.EndTime.Year > 1980 ? Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            entity.StartTime = entity.StartTime.Year > 1980 ? Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01"));
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HouseID = LoginUserEntity.HospitalID;
            int rows;
            int pagecount;
            decimal lingshou;
            decimal chengben;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.Type = entity.Type == 0 ? entity.Type = 1 : entity.Type;
            entity.numPerPage = 50;
            entity.orderDirection = "desc";
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<StockOrderEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }

            var list = iwareBLL.GetStorageSummaryList(entity, out rows, out pagecount);  //先获取库存列表
            iwareBLL.GetStorageSummarySum(entity, out lingshou, out chengben);
            foreach (var info in list)
            {
                info.AllChengBen = info.AllQuatity * info.CostPrice;
                chengben += info.AllChengBen;
                lingshou += info.AllMoney;
                info.AllMoney = info.AllQuatity * info.RetailPrice;
            }
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;

            ViewBag.ls = lingshou;
            ViewBag.cb = chengben;
            if (Request.IsAjaxRequest())
                return PartialView("_StorageSummary", result);
            return View(result);
            //首先获取库存

        }
        #endregion


        #region ==导入导出==

        /// <summary>
        /// 导出入库单
        /// </summary>
        /// <param name="IsVerify"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public FileContentResult ExportRukuList(int TClass = 0, int IsVerify = 0, string StartTime = "", string EndTime = "", string HospitalID = "", int pageNum = 1)
        {
            StockOrderEntity entity = new StockOrderEntity();
            entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));

            entity.IsVerify = IsVerify;
            entity.pageNum = pageNum;
            entity.Class = TClass;
            if (StartTime != "")
            {
                entity.StartTime = Convert.ToDateTime(StartTime);
            }
            if (EndTime != "")
            {
                entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndTime).Year + "-" + Convert.ToDateTime(EndTime).Month + "-" + Convert.ToDateTime(EndTime).Day + " 23:59:59");
            }
            if (!String.IsNullOrEmpty(entity.ProductName))
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            if (HospitalID != "" && HospitalID != "0" && HospitalID != "undefined")
            {
                entity.HospitalID = Convert.ToInt32(HospitalID);
            }
            else//如果没传门店值进来,默认查询当前门店
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            int rows;
            int pagecount;

            entity.Type = 2;
            entity.numPerPage = 1000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            entity.orderDirection = "desc";
            var list = iwareBLL.GetStockOrderList(entity, out rows, out pagecount);
            DataTable tableExport = new DataTable("Export");
            string hasPermission = Request["txtHasPermission"];
            if (hasPermission=="1")
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("订单编号"), new DataColumn("入库时间"), new DataColumn("入库类别"), new DataColumn("入库数量"),new DataColumn("总计成本"), new DataColumn("总计金额"), new DataColumn("操作人"), new DataColumn("订单状态") });
            }
            else
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("订单编号"), new DataColumn("入库时间"), new DataColumn("入库类别"), new DataColumn("入库数量"), new DataColumn("总计金额"), new DataColumn("操作人"), new DataColumn("订单状态") });
            }

           
            foreach (StockOrderEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["订单编号"] = model.OrderNo;
                row["入库时间"] = model.OrderTime.ToString("yyyy-MM-dd");
                row["入库类别"] = EYB.Web.Code.PageFunction.GetClassName(entity.Class, entity.Type);
                row["入库数量"] = model.AllQuatity;
                if (hasPermission=="1")
                {
                    row["总计成本"] = model.AllChengBen;
                }
                row["总计金额"] = model.AllMoney;
                row["操作人"] = model.UserName;

                row["订单状态"] = model.IsVerify == 1 ? "待审核" : (model.IsVerify == 2 ? "已通过" : "审核不通过");
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["入库类别"] = "合计";// Request["txtallnumber"];
            row1["入库数量"] = Request["txtallnumber"];
            if (hasPermission=="1")
            {
                row1["总计成本"] = Request["txtallchengben"];
            }
            row1["总计金额"] = Request["txtallprice"];
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "RuKuDan");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "RuKuDan.xls");
        }


        /// <summary>
        /// 导出-库存查询
        /// </summary>
        /// <param name="IsVerify"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public FileContentResult ExportWarehouseList(ProductStockEntity entity)
        {
            if (entity.StartTime.Year < 1980) { entity.StartTime = DateTime.Now.AddYears(-10); }
            if (entity.EndTime.Year < 1980) { entity.EndTime = DateTime.Now; }
            if (entity.ProductName == "搜索" && entity.ProductName == null) entity.ProductName = "";
            int rows;
            int pagecount;
            if (!String.IsNullOrEmpty(entity.ProductName))
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            entity.EndTime = Convert.ToDateTime(entity.EndTime.Year + "-" + entity.EndTime.Month + "-" + entity.EndTime.Day + " 23:59:59");

            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 500000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProductName"; }
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var list = iwareBLL.GetProductStockList(entity, out rows, out pagecount);       //先默认查询出当前的余额
            //获取当前
            //  StockOrderInfo soi = new StockOrderInfo();
            var sum = iwareBLL.GetSumtStockList(entity);
            decimal lingshou = sum.RetailPrice;
            decimal chengben = sum.CostPrice;
            int allnumber = sum.Quatity;
            foreach (var info in list)
            {
                var model = baseinfoBLL.GetModel(info.ProductID);
                info.LingshouPrice = model.RetailPrice * info.Quatity;
                info.Chengbenprice = model.CostPrice * info.Quatity;

            }
            DataTable tableExport = new DataTable("Export");
            bool HasPermission = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_库存管理成本价);
            if (HasPermission)
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("产品名称"), new DataColumn("产品编码"), new DataColumn("零售价"), new DataColumn("实际库存"), new DataColumn("寄存数量"), new DataColumn("欠货"), new DataColumn("理论库存"), new DataColumn("零售价总额"), new DataColumn("成本价总额"), new DataColumn("所属品牌"), new DataColumn("类别"), new DataColumn("容量"), new DataColumn("规格"), new DataColumn("所在仓库") });
            }
            else
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("产品名称"), new DataColumn("产品编码"), new DataColumn("零售价"), new DataColumn("实际库存"), new DataColumn("寄存数量"), new DataColumn("欠货"), new DataColumn("理论库存"), new DataColumn("零售价总额"), new DataColumn("所属品牌"), new DataColumn("类别"), new DataColumn("容量"), new DataColumn("规格"), new DataColumn("所在仓库") });
            }
            foreach (ProductStockEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["产品名称"] = model.ProductName;
                if (model.ProductCode!=""&&model.ProductCode!=null)
                {
                    row["产品编码"] = model.ProductCode.ToString().Trim();
                }
                else
                {
                    row["产品编码"] = model.ProductCode;
                }
              
                row["零售价"] = model.RetailPrice;
                row["实际库存"] = model.Quatity;
                row["寄存数量"] = model.JichunCount;
                row["欠货"] = model.QianhuoCount;
                row["理论库存"] = model.Quatity + model.JichunCount;
                row["零售价总额"] = model.LingshouPrice;
                if (HasPermission)
                {
                    row["成本价总额"] = model.Chengbenprice;
                }
                row["所属品牌"] = model.BrandName;
                row["类别"] = model.TypeName;
                row["容量"] = model.UseUnit;
                row["规格"] = model.StandardUnitName;
                row["所在仓库"] = model.HouseName;
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["零售价"] = "合计";
            row1["实际库存"] =sum.Quatity;
            row1["零售价总额"] = sum.RetailPrice;
            if (HasPermission)
            {
                row1["成本价总额"] = sum.CostPrice;
            }
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData1(tableExport, "KuCun");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "KuCun.xls");
        }

        /// <summary>
        /// 导出-采购进货
        /// </summary>
        /// <param name="IsVerify"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public FileContentResult ExportJinhuoList(BuyOrderEntity entity)
        {
            ViewBag.Statu = entity.Statu;
            entity.StartTime = (entity.StartTime.Year < 1970 ? DateTime.Now.AddDays(-30) : entity.StartTime);
            entity.EndTime = (entity.EndTime.Year < 1970 ? DateTime.Now : entity.EndTime);

            entity.StartTime = Convert.ToDateTime(entity.StartTime.Year + "-" + entity.StartTime.Month + "-" + entity.StartTime.Day + " 01:01:01");
            entity.EndTime = Convert.ToDateTime(entity.EndTime.Year + "-" + entity.EndTime.Month + "-" + entity.EndTime.Day + " 23:59:59");
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.Type = 1;
            entity.numPerPage = 500000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            entity.orderDirection = "desc";
            var list = iwareBLL.GetBuyOrderList(entity, out rows, out pagecount);
            var FSum = iwareBLL.getSumBuyOrder(entity);
            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("业务单号"), new DataColumn("业务类型"), new DataColumn("供应商"), new DataColumn("业务日期"), new DataColumn("应付金额"), new DataColumn("实付金额") });
            foreach (BuyOrderEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["业务单号"] = model.OrderNo;
                row["业务类型"] = EYB.Web.Code.PageFunction.GetBuyOrderType(model.Type);
                row["供应商"] = model.SupplierName;
                row["业务日期"] = model.CreateTime.ToString("yyyy-MM-dd");
                row["应付金额"] = model.NeedMoney;
                row["实付金额"] = model.ActualMoney;

                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["业务日期"] = "合计";
            row1["应付金额"] = FSum.NeedMoney;
            row1["实付金额"] = FSum.ActualMoney;
            tableExport.Rows.Add(row1);

          //  ViewBag.AllActualMoney = sumall.ActualMoney;      //总共实付总额
           // ViewBag.AllNeedMoney = sumall.NeedMoney;         //应付总额
                                                             // row1["应付金额"] = FSum.;

            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "CaiGou");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "CaiGou.xls");
        }


        /// <summary>
        /// 导出-采购退货
        /// </summary>
        /// <param name="IsVerify"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public FileContentResult ExportTuiHuoList(BuyOrderEntity entity)
        {
            ViewBag.Statu = entity.Statu;
            entity.StartTime = (entity.StartTime.Year < 1970 ? DateTime.Now.AddDays(-30) : entity.StartTime);
            entity.EndTime = (entity.EndTime.Year < 1970 ? DateTime.Now : entity.EndTime);

            entity.StartTime = Convert.ToDateTime(entity.StartTime.Year + "-" + entity.StartTime.Month + "-" + entity.StartTime.Day + " 01:01:01");
            entity.EndTime = Convert.ToDateTime(entity.EndTime.Year + "-" + entity.EndTime.Month + "-" + entity.EndTime.Day + " 23:59:59");

            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.Type = 2;
            entity.numPerPage = 5000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            entity.orderDirection = "desc";
            var list = iwareBLL.GetBuyOrderList(entity, out rows, out pagecount);

            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("业务单号"), new DataColumn("业务类型"), new DataColumn("供应商"), new DataColumn("业务日期"), new DataColumn("实付金额") });
            foreach (BuyOrderEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["业务单号"] = model.OrderNo;
                row["业务类型"] = EYB.Web.Code.PageFunction.GetBuyOrderType(model.Type);
                row["供应商"] = model.SupplierName;
                row["业务日期"] = model.CreateTime.ToString("yyyy-MM-dd");

                row["实付金额"] = model.ActualMoney;

                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "TuiHuo");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "TuiHuo.xls");
        }


        /// <summary>
        /// 出库单导出
        /// </summary>
        /// <param name="IsVerify"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public FileContentResult ExportChuKuList(int TClass = 0, int IsVerify = 0, string StartTime = "", string HospitalID = "", string EndTime = "", int pageNum = 1)
        {
            StockOrderEntity entity = new StockOrderEntity();
            entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));

            entity.IsVerify = IsVerify;
            entity.pageNum = pageNum;
            entity.Class = TClass;
            if (StartTime != "")
            {
                entity.StartTime = Convert.ToDateTime(StartTime);
            }
            if (EndTime != "")
            {
                entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndTime).Year + "-" + Convert.ToDateTime(EndTime).Month + "-" + Convert.ToDateTime(EndTime).Day + " 23:59:59");
            }
            if (!String.IsNullOrEmpty(entity.ProductName))
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            int rows;
            int pagecount;
            if (HospitalID != "" && HospitalID != "0" && HospitalID != "undefined")
            {
                entity.HospitalID = Convert.ToInt32(HospitalID);
            }
            else//如果没传门店值进来,默认查询当前门店
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            entity.Type = 1;
            entity.numPerPage = 1000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            entity.orderDirection = "desc";
            var list = iwareBLL.GetStockOrderList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用

            DataTable tableExport = new DataTable("Export");
            string txtHasPermission = Request["txtHasPermission"];
            if (txtHasPermission == "1")
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("订单编号"), new DataColumn("出库时间"), new DataColumn("出库类别"), new DataColumn("出库数量"), new DataColumn("总计成本"), new DataColumn("总计金额"), new DataColumn("操作人"), new DataColumn("订单状态") });
            }
            else
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("订单编号"), new DataColumn("出库时间"), new DataColumn("出库类别"), new DataColumn("出库数量"), new DataColumn("总计金额"), new DataColumn("操作人"), new DataColumn("订单状态") });
            }

           
            foreach (StockOrderEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["订单编号"] = model.OrderNo;
                row["出库时间"] = model.OrderTime.ToString("yyyy-MM-dd");
                row["出库类别"] = EYB.Web.Code.PageFunction.GetClassName(model.Class, entity.Type);
                row["出库数量"] = model.AllQuatity;
                if (txtHasPermission=="1")
                {
                    row["总计成本"] = model.AllChengBen;
                }
                row["总计金额"] = model.AllMoney;
                row["操作人"] = model.UserName;
                row["订单状态"] = model.IsVerify == 1 ? "待审核" : (model.IsVerify == 2 ? "已通过" : "审核不通过");
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["出库类别"]="合计";
            row1["出库数量"] = Request["txtallnumber"];
            if (txtHasPermission=="1")
            {
                row1["总计成本"] = Request["txtallchengben"];
            }
            row1["总计金额"] = Request["txtallprice"];
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "ChuKu");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "ChuKu.xls");
        }

        /// <summary>
        /// 调拨导出
        /// </summary>
        /// <param name="IsVerify"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public FileContentResult ExportDiaoBoList(int IsVerify = 0, string StartTime = "", string HospitalID = "", string EndTime = "", int pageNum = 1)
        {
            StockOrderEntity entity = new StockOrderEntity();
            entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));

            entity.IsVerify = IsVerify;
            entity.pageNum = pageNum;
            if (StartTime != "")
            {
                entity.StartTime = Convert.ToDateTime(StartTime);
            }
            if (EndTime != "")
            {
                entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndTime).Year + "-" + Convert.ToDateTime(EndTime).Month + "-" + Convert.ToDateTime(EndTime).Day + " 23:59:59");
            }
            int rows;
            int pagecount;
            if (HospitalID != "" && HospitalID != "0")
            {
                entity.HospitalID = Convert.ToInt32(HospitalID);
            }
            else//如果没传门店值进来,默认查询当前门店
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            entity.Type = 3;
            entity.numPerPage = 10000000; //每页显示条数
            entity.orderDirection = "desc";
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = iwareBLL.GetStockOrderList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("订单编号"), new DataColumn("出库时间"), new DataColumn("调拨类别"), new DataColumn("出库数量"), new DataColumn("总计金额"), new DataColumn("订单状态"), new DataColumn("操作人") });
            foreach (StockOrderEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["订单编号"] = model.OrderNo;
                row["出库时间"] = model.OrderTime.ToString("yyyy-MM-dd");
                row["调拨类别"] = entity.HospitalID == LoginUserEntity.HospitalID ? "调出" : "调入";
                row["出库数量"] = model.AllQuatity;
                row["总计金额"] = model.AllMoney;
                row["订单状态"] = model.IsVerify == 1 ? "待审核" : (model.IsVerify == 2 ? "已通过" : "审核不通过");
                row["操作人"] = model.UserName;
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["调拨类别"] = "合计";
            row1["出库数量"] = Request["txtallnumber"];
            row1["总计金额"] = Request["txtallprice"];
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "DiaoBo");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "DiaoBo.xls");

        }

        /// <summary>
        /// 盘点导出
        /// </summary>
        /// <param name="IsVerify"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public FileContentResult ExportPanDianList(int IsVerify = 0, string StartTime = "", string HospitalID = "", string EndTime = "", int pageNum = 1)
        {
            CheckOrderEntity entity = new CheckOrderEntity();
            entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));


            entity.IsVerify = IsVerify;
            entity.pageNum = pageNum;
            if (StartTime != "")
            {
                entity.StartTime = Convert.ToDateTime(StartTime);
            }
            if (EndTime != "")
            {
                entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndTime).Year + "-" + Convert.ToDateTime(EndTime).Month + "-" + Convert.ToDateTime(EndTime).Day + " 23:59:59");
            }
            int rows;
            int pagecount;
            if (HospitalID != "" && HospitalID != "0")
            {
                entity.HospitalID = Convert.ToInt32(HospitalID);
            }
            else//如果没传门店值进来,默认查询当前门店
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            entity.numPerPage = 1000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = iwareBLL.GetCheckOrderList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);

            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("盘点编号"), new DataColumn("账面数量"), new DataColumn("实盘数量"), new DataColumn("盘盈数量"), new DataColumn("盘盈金额"), new DataColumn("盘亏数量"), new DataColumn("盘亏金额"), new DataColumn("盘点时间"), new DataColumn("操作人"), new DataColumn("订单状态") });
            foreach (CheckOrderEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["盘点编号"] = model.OrderNo;
                row["账面数量"] = model.ZhangMianQuatity;
                row["实盘数量"] = model.ShijiQuatity;
                row["盘盈数量"] = model.PanYinQuatity;
                row["盘盈金额"] = model.PanyinMoney;
                row["盘亏数量"] = model.PankuiQuatity;
                row["盘亏金额"] = model.PankuiMoeny;
                row["盘点时间"] = model.CheckTime.ToString("yyyy-MM-dd");
                row["操作人"] = model.UserName;
                row["订单状态"] = model.IsVerify == 1 ? "待审核" : (model.IsVerify == 2 ? "已通过" : "审核不通过");
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "PanDian");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "PanDian.xls");


        }


        /// <summary>
        /// 仓库产品明细导出
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="ProductName"></param>
        /// <param name="ordetype"></param>
        /// <param name="BaseID"></param>
        /// <param name="BrandID"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <returns></returns>
        public FileContentResult ExportMingXiList(int HouseID = 0, string ProductName = "", int ordetype = 0, int BaseID = 0, int BrandID = 0, string StartTime = "", string EndTime = "")
        {
            StockOrderInfo entity = new StockOrderInfo();
            entity.HouseID = HouseID;
            entity.ProductName = ProductName;
            entity.Type = ordetype;
            entity.BaseID = BaseID;
            entity.BrandID = BrandID;
            entity.StartTime = Convert.ToDateTime(StartTime);
            entity.EndTime = Convert.ToDateTime(EndTime);

            if (entity.StartTime.Year < 1980) { entity.StartTime = DateTime.Now.AddMonths(-1); }
            if (entity.EndTime.Year < 1980) { entity.EndTime = DateTime.Now; }
            entity.EndTime = Convert.ToDateTime(entity.EndTime.Year + "-" + entity.EndTime.Month + "-" + entity.EndTime.Day + " 23:59:59");
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            if (entity.ProductName != null)
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (entity.HouseID == 0)
            {
                entity.HouseID = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = LoginUserEntity.HospitalID })[0].ID;
            }

            entity.numPerPage = 50000000;
            int rows = 0;
            int pagecount = 0;

            var list = iwareBLL.GetStockOrderInfoListinSelect(entity, out rows, out pagecount);
            var sum = iwareBLL.GetStockOrderInfoSum(entity)[0];

            DataTable tableExport = new DataTable("Export");
            bool HasPermission = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_项目管理成本价);
            if (HasPermission)
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("仓库"), new DataColumn("单据列表"), new DataColumn("单据类型"), new DataColumn("单据分类"), new DataColumn("原因"), new DataColumn("产品名称"), new DataColumn("品牌"), new DataColumn("零售单价"), new DataColumn("成本价"), new DataColumn("数量"), new DataColumn("合计金额"), new DataColumn("合计成本"), new DataColumn("业务日期") });
            }
            else
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("仓库"), new DataColumn("单据列表"), new DataColumn("单据类型"), new DataColumn("单据分类"), new DataColumn("原因"), new DataColumn("产品名称"), new DataColumn("品牌"), new DataColumn("零售单价"), new DataColumn("数量"), new DataColumn("合计金额"), new DataColumn("业务日期") });
            }
            foreach (StockOrderInfo model in list)
            {
                DataRow row = tableExport.NewRow();
                row["仓库"] = model.Housename;
                row["单据列表"] = model.StockOrderNo;
                row["单据类型"] = EYB.Web.Code.PageFunction.OrderType(model.OrderType);
                row["单据分类"] = EYB.Web.Code.PageFunction.GetClassName(model.Class, model.OrderType);
                row["原因"] = EYB.Web.Code.PageFunction.GetBaseName(model.BaseID, LoginUserEntity.ParentHospitalID);
                row["产品名称"] = model.ProductName;
                row["品牌"] = model.BrandName;
                row["零售单价"] = model.RetailPrice;
                if (HasPermission)
                {
                    row["成本价"] = model.CostPrice;
                }
                row["数量"] = model.Number;
                row["合计金额"] = model.Number * model.RetailPrice;
                if (HasPermission)
                {
                    row["合计成本"] = model.Number * model.CostPrice;
                }
                row["业务日期"] = model.OrderTime.ToString("yyyy-MM-dd hh:MM:ss");
                tableExport.Rows.Add(row);
            }

            DataRow row1 = tableExport.NewRow();
            row1["仓库"] = "";
            row1["单据列表"] = "";
            row1["单据类型"] = "";
            row1["单据分类"] = "";
            row1["原因"] = "";
            row1["产品名称"] = "";
            row1["品牌"] = "";
            row1["零售单价"] = "";
            if (HasPermission)
            {
                row1["成本价"] = "合计";
            }
            row1["数量"] = sum.Number;
            row1["合计金额"] = sum.RetailPrice;
            if (HasPermission)
            {
                row1["合计成本"] = sum.CostPrice;
            }
            row1["业务日期"] = "";
            tableExport.Rows.Add(row1);

            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "ChanPinMingxi");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "ChanPinMingxi.xls");

        }




        /// <summary>
        /// 进出仓汇总
        /// </summary>
        /// <param name="IsVerify"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public FileContentResult ExportWarehouseSummary(int BrandID = 0, string StartTime = "", string EndTime = "", int HouseID = 0, string ProductName = "")
        {
            ProductStockEntity entity = new ProductStockEntity();
            entity.BrandID = BrandID;
            entity.StartTime = StartTime == "" ? DateTime.Now.AddDays(-1) : Convert.ToDateTime(StartTime);
            entity.EndTime = EndTime == "" ? DateTime.Now : Convert.ToDateTime(EndTime);
            entity.HouseID = HouseID;
            entity.ProductName = ProductName;
            if (entity.StartTime.Year < 1980) { entity.StartTime = DateTime.Now.AddDays(-30); }
            if (entity.EndTime.Year < 1980) { entity.EndTime = DateTime.Now; }
            entity.EndTime = Convert.ToDateTime(entity.EndTime.Year + "-" + entity.EndTime.Month + "-" + entity.EndTime.Day + " 23:59:59");
            int rows;
            int pagecount;
            //entity.HospitalID = loginUserEntity.HospitalID;
            entity.numPerPage = 10000000;
            entity.orderDirection = "desc";
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = iwareBLL.GetProductStockList(entity, out rows, out pagecount);  //先获取库存列表

            DataTable tableExport = new DataTable("Export");
            bool HasPermission = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_项目管理成本价);
            if (HasPermission)
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("仓库名称"), new DataColumn("产品名称"), new DataColumn("出库数量"), new DataColumn("入库数量"), new DataColumn("零售价"), new DataColumn("结算库存"), new DataColumn("库存金额（零售价）"), new DataColumn("库存金额（成本价）") });
            }
            else
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("仓库名称"), new DataColumn("产品名称"), new DataColumn("出库数量"), new DataColumn("入库数量"), new DataColumn("零售价"), new DataColumn("结算库存"), new DataColumn("库存金额（零售价）") });
            }
            foreach (ProductStockEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["仓库名称"] = model.HouseName;
                row["产品名称"] = model.ProductName;
                row["出库数量"] = model.OutQuatity;
                row["入库数量"] = model.INQuatity;

                row["零售价"] = model.RetailPrice;
                row["结算库存"] = model.Quatity;
                row["库存金额(零售价)"] = model.RetailPrice * model.Quatity;
                if (HasPermission)
                {
                    row["库存金额(成本价)"] = model.CostPrice * model.Quatity;
                }
                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "Jinchucang");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "Jinchucang.xls");
        }

        ///<summary>
        /// 导出-预付查询
        /// </summary>
        /// <param name="IsVerify"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public FileContentResult ExportYufuList(int IsVerify, string StartTime, string HospitalID = "", string EndTime = "")
        {
            StockOrderEntity entity = new StockOrderEntity();
            int rows;
            int pagecount;
            if (StartTime != "")
            {
                entity.StartTime = Convert.ToDateTime(Convert.ToDateTime(StartTime).ToString("yyyy-MM-dd 00:00:01"));
            }
            else
            {
                entity.StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:01"));

            }
            if (EndTime != "")
            {
                entity.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndTime).ToString("yyyy-MM-dd 00:00:01"));
            }
            else
            {
                entity.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            }

            //entity.IsVerify = IsVerify > 0 ? IsVerify : 2;
            entity.pageNum = 1;

            if (!String.IsNullOrEmpty(entity.ProductName))
            {
                entity.ProductName = entity.ProductName.ToUpper();
            }
            if (HospitalID != "" && HospitalID != "0")
            {
                entity.HospitalID = Convert.ToInt32(HospitalID);
            }
            else//如果没传门店值进来,默认查询当前门店
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            entity.Type = 5;
            entity.numPerPage = 100000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            entity.orderDirection = "desc";
            var list = iwareBLL.GetStockOrderList(entity, out rows, out pagecount);
            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("订单编号"), new DataColumn("出库时间"), new DataColumn("顾客姓名"), new DataColumn("产品名称"), new DataColumn("出库数量"), new DataColumn("总计金额"), new DataColumn("操作人") });
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["订单编号"] = model.OrderNo;
                row["出库时间"] = model.OrderTime;
                row["顾客姓名"] = model.CustomerName;
                row["产品名称"] = model.ProductName;
                row["出库数量"] = model.AllQuatity;
                row["总计金额"] = model.AllMoney;
                row["操作人"] = model.UserName;

                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "Yufu");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "Yufu.xls");
        }

        /// <summary>
        /// 导出出入库汇总
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportStorageSummary()
        {
            StockOrderEntity entity = new StockOrderEntity();
            entity.StartTime = Request["StartTime"] + "" == "" ? DateTime.Now.AddDays(-1) : Convert.ToDateTime(Request["StartTime"]);
            entity.EndTime = Request["EndTime"] + "" == "" ? DateTime.Now : Convert.ToDateTime(Request["EndTime"]);
            entity.BrandID = Convert.ToInt32(Request["BrandID"]);
            entity.HouseID = Convert.ToInt32(Request["HouseID"]);
            entity.Type = Convert.ToInt32(Request["Type"]);
            entity.ProductName = Request["ProductName"];
            entity.EndTime = entity.EndTime.Year > 1980 ? Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            entity.StartTime = entity.StartTime.Year > 1980 ? Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01"));

            decimal lingshou = 0;
            decimal chengben = 0;
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.Type = entity.Type == 0 ? entity.Type = 1 : entity.Type;
            entity.numPerPage = 50000000;
            entity.orderDirection = "desc";
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var list = iwareBLL.GetStorageSummaryList(entity, out rows, out pagecount);  //先获取库存列表
            //iwareBLL.GetStorageSummarySum(entity, out lingshou, out chengben);
            foreach (var info in list)
            {
                info.AllChengBen = info.AllQuatity * info.CostPrice;
                info.AllMoney = info.AllQuatity * info.RetailPrice;
            }
            DataTable tableExport = new DataTable("Export");
            bool HasPermission = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_项目管理成本价);
            if (HasPermission)
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("仓库名称"), new DataColumn("类别"), new DataColumn("品牌"), new DataColumn("产品名称"), new DataColumn("数量"), new DataColumn("零售价"), new DataColumn("零售总价"), new DataColumn("成本总额") });
            }
            else
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("仓库名称"), new DataColumn("类别"), new DataColumn("品牌"), new DataColumn("产品名称"), new DataColumn("数量"), new DataColumn("零售价"), new DataColumn("零售总价") });
            }
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["仓库名称"] = model.HouseName;
                row["类别"] = EYB.Web.Code.PageFunction.OrderType(entity.Type);
                row["品牌"] = model.BrandName;
                row["产品名称"] = model.ProductName;
                row["数量"] = model.AllQuatity;
                row["零售价"] = model.RetailPrice;
                row["零售总价"] = model.AllMoney;
                lingshou += model.AllMoney;
                if (HasPermission)
                {
                    row["成本总额"] = model.AllChengBen;
                    chengben += model.AllChengBen;
                }
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["零售价"] = "合计";
            row1["零售总价"] = lingshou;
            if (HasPermission)
            {
                row1["成本总额"] = chengben;
            }
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "Churuku");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "Churuku.xls");


        }


        /// <summary>
        /// 调拨报表导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public FileContentResult ExportAllocateReport(StockOrderInfo entity)
        {
            if (entity.StartTime.Year < 1980)
            {
                entity.StartTime = DateTime.Today.AddDays(-1);
            }
            if (entity.EndTime.Year < 1980)
            {
                entity.EndTime = DateTime.Today;
            }
            int SumNumber = 0;
            decimal SumRetailPrice, SumCostPrice = 0;
            int rows, pagecount = 0;
            //entity.numPerPage = 50;
            entity.orderDirection = "desc";
            entity.orderField = "OrderTime";
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var list = iwareBLL.GetStockOrderInfoListByDiaoBo(entity, out rows, out pagecount, out SumNumber, out SumRetailPrice, out SumCostPrice);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            DataTable tableExport = new DataTable("Export");
            bool HasPermission = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_项目管理成本价);
            if (HasPermission)
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("调出仓库"), new DataColumn("调入仓库"), new DataColumn("订单编号"), new DataColumn("单据时间"), new DataColumn("产品名称"), new DataColumn("品牌"), new DataColumn("数量"), new DataColumn("成本价"), new DataColumn("成本总价"), new DataColumn("零售价"), new DataColumn("零售总价"), new DataColumn("订单状态") });
            }
            else
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("调出仓库"), new DataColumn("调入仓库"), new DataColumn("订单编号"), new DataColumn("单据时间"), new DataColumn("产品名称"), new DataColumn("品牌"), new DataColumn("数量"), new DataColumn("零售价"), new DataColumn("零售总价"), new DataColumn("订单状态") });
            }
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["调出仓库"] = model.Housename;
                row["调入仓库"] = model.TransferHouseName;
                row["订单编号"] = model.StockOrderNo;
                row["单据时间"] = model.OrderTime.ToString("yyyy-MM-dd");
                row["产品名称"] = model.ProductName;
                row["品牌"] = model.BrandName;
                row["数量"] = model.Number;
                if (HasPermission)
                {
                    row["成本价"] = model.CostPrice;
                    row["成本总价"] = model.AllCostPrice;
                }
                row["零售价"] = model.RetailPrice;
                row["零售总价"] = model.AllRetailPrice;
                row["订单状态"] = model.GetIsVerifyName();

                tableExport.Rows.Add(row);
            }
            //DataRow row1 = tableExport.NewRow();
            //row1["调出仓库"] = "";
            //row1["调入仓库"] = "";
            //row1["订单编号"] = "";
            //row1["单据时间"] = "";
            //row1["产品名称"] = "";
            //row1["品牌"] = "合计";
            //row1["数量"] = SumNumber;
            //if (HasPermission)
            //{
            //    row1["成本价"] = "";
            //    row1["成本总价"] = SumCostPrice;
            //}
            //row1["零售价"] = "";
            //row1["零售总价"] = SumRetailPrice;
            //row1["订单状态"] = "";
            //tableExport.Rows.Add(row1);

            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "DiaoboBaobiao");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "DiaoboBaobiao.xls");

        }
        /// <summary>
        /// 导出盘点单
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportPandianEdit()
        {
            var ID = Convert.ToInt32(Request["ID"]);
            var order = iwareBLL.GetCheckOrderModel(new CheckOrderEntity { ID = ID });
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var list = iwareBLL.GetCheckOrderInfoList(new CheckOrderInfoEntity { OrderNo = order.OrderNo, HospitalID = LoginUserEntity.ParentHospitalID });
            foreach (var info in list)
            {
                ProductStockEntity pse = new ProductStockEntity();
                pse.HouseID = info.HouseID;
                pse.ProductID = info.ProductID;
                pse.HospitalID = LoginUserEntity.HospitalID;
                if (order.IsVerify == 1)
                {
                    var kucunlist = iwareBLL.GetProductStockListToseleect(pse);
                    if (kucunlist.Count > 0)     //如果能找到库存,则找到这个库存
                    {
                        info.kucunID = kucunlist[0].ID;
                        info.BookQuatity = kucunlist[0].Quatity;
                    }
                }
            }
            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("产品名称"), new DataColumn("品牌"), new DataColumn("单价"), new DataColumn("账面数量"), new DataColumn("实盘数量"), new DataColumn("盘盈数量"), new DataColumn("盘盈金额"), new DataColumn("盘亏数量"), new DataColumn("盘亏金额") });
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["产品名称"] = model.ProductName;
                row["品牌"] = model.BrandName;
                row["单价"] = model.RetailPrice;
                row["账面数量"] = model.BookQuatity;
                row["实盘数量"] = order.IsVerify == 1 ? model.ActualQuatity : model.ActualQuatity;

                row["盘盈数量"] = model.MoreQuatity;
                row["盘盈金额"] = model.MorePrice;
                row["盘亏数量"] = model.LessQuatity;
                row["盘亏金额"] = model.LessPrice;

                tableExport.Rows.Add(row);
            }


            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "Pandian");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "Pandian.xls");
        }
        /// <summary>
        /// 全店仓库分析
        /// </summary>
        /// <returns></returns>
        public FileContentResult ExportWarehouseAnalysis(ProductEntity entity)
        {
            string txtAllQuatity = Request["txtAllQuatity"].ToString();
            string txtAllMoney = Request["txtAllMoney"].ToString();
            string txtAllChengben = Request["txtAllChengben"].ToString();
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            List<WarehouseAnalysisEntity> list = new List<WarehouseAnalysisEntity>();
            int row;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;//基础信息类全部用总店ID
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 99999999; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            var productlist = baseinfoBLL.GetAllProduct(entity, out row, out pagecount);
            var houselist = iwareBLL.GetAllWarehouseListByParentHospitalID(new WarehouseEntity { ParentHospitalID = LoginUserEntity.ParentHospitalID });
            int quatity = 0;//数量统计

            foreach (var info in productlist)//遍历产品
            {
                quatity = 0;
                WarehouseAnalysisEntity wa = new WarehouseAnalysisEntity();
                wa.ProdcuitID = info.ID;
                wa.ProdcuitName = info.Name;
                wa.Price = info.RetailPrice;
                wa.BrandName = info.BrandName;
                wa.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                foreach (var house in houselist)//遍历每个仓库
                {
                    //通过仓库ID,产品ID,门店ID获取是否有库存.
                    var pstlist = iwareBLL.GetProductStockListToseleect(new ProductStockEntity { HouseID = house.ID, ProductID = info.ID, HospitalID = house.HospitalID });
                    var val = pstlist.Count > 0 ? pstlist[0].Quatity : 0;
                    wa.Quatity.Add(house.ID, val);//添加字典数据
                    quatity = pstlist.Count == 0 ? quatity : quatity + pstlist[0].Quatity;
                }
                wa.AllQuatity = quatity;
                wa.AllMoney = wa.AllQuatity * wa.Price;
                wa.AllChengben = wa.AllQuatity * info.CostPrice;
                list.Add(wa);
            }
            DataTable tableExport = new DataTable("Export");
            DataColumn[] dc = new DataColumn[houselist.Count + 6];

            //DataColumn column = new DataColumn();
            //column.ColumnName = "品牌";
            dc[0] = new DataColumn("品牌");
            dc[1] = new DataColumn("名称");
            dc[2] = new DataColumn("价格");
            int t = 3;
            foreach (var info in houselist)
            {
                dc[t] = new DataColumn(info.Name);
                t++;
            }
            dc[houselist.Count + 3] = new DataColumn("合计数量");
            dc[houselist.Count + 4] = new DataColumn("合计金额");
            dc[houselist.Count + 5] = new DataColumn("合计成本");

            tableExport.Columns.AddRange(dc);
            foreach (var model in list)
            {
                DataRow drow = tableExport.NewRow();
                drow["品牌"] = model.BrandName;
                drow["名称"] = model.ProdcuitName;
                drow["价格"] = model.Price;
                int t1 = 3;
                foreach (var info in model.Quatity.Values)
                {
                    drow[t1] = info + "";
                    t1++;
                }


                drow["合计数量"] = model.AllQuatity;
                drow["合计金额"] = model.AllMoney;
                drow["合计成本"] = model.AllChengben;
                tableExport.Rows.Add(drow);
            }
            DataRow row2 = tableExport.NewRow();
            row2["客户体验2仓库"] = "合计";
            row2["合计数量"] = txtAllQuatity;
            row2["合计金额"] = txtAllMoney;
            row2["合计成本"] = txtAllChengben;
            tableExport.Rows.Add(row2);
          //  list.Sum()


            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "cangkufenxi");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "cangkufenxi.xls");


        }

        #endregion

        #region ==全店仓库分析==

        //public ActionResult WarehouseAnalysis(ProductEntity entity)
        //{
        //    var list = new List<WarehouseAnalysisEntity>();
        //    var result = list.AsQueryable().ToPagedList(1, 0);

        //    result.TotalItemCount = 0;
        //    result.CurrentPageIndex = 0;
        //    ViewBag.ParentHospitalID = loginUserEntity.ParentHospitalID;
        //    return View(result);
        //}

        /// <summary>
        /// 全店仓库分析
        /// </summary>
        /// <returns></returns>
        public ActionResult WarehouseAnalysis(ProductEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            List<WarehouseAnalysisEntity> list = new List<WarehouseAnalysisEntity>();
            int row;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;//基础信息类全部用总店ID
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<WarehouseAnalysisEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }

            var productlist = baseinfoBLL.GetAllProduct(entity, out row, out pagecount);
            var houselist = iwareBLL.GetAllWarehouseListByParentHospitalID(new WarehouseEntity { ParentHospitalID = LoginUserEntity.ParentHospitalID });
            int quatity = 0;//数量统计

            foreach (var info in productlist)//遍历产品
            {
                quatity = 0;
                WarehouseAnalysisEntity wa = new WarehouseAnalysisEntity();
                wa.ProdcuitID = info.ID;
                wa.ProdcuitName = info.Name;
                wa.Price = info.RetailPrice;
                wa.BrandName = info.BrandName;
                wa.ParentHospitalID = LoginUserEntity.ParentHospitalID;
                foreach (var house in houselist)//遍历每个仓库
                {
                    //通过仓库ID,产品ID,门店ID获取是否有库存.
                    var pstlist = iwareBLL.GetProductStockListToseleect(new ProductStockEntity { HouseID = house.ID, ProductID = info.ID, HospitalID = house.HospitalID });
                    var val = pstlist.Count > 0 ? pstlist[0].Quatity : 0;
                    wa.Quatity.Add(house.ID, val);//添加字典数据
                    quatity = pstlist.Count == 0 ? quatity : quatity + pstlist[0].Quatity;
                }
                wa.AllQuatity = quatity;
                wa.AllMoney = wa.AllQuatity * wa.Price;
                wa.AllChengben = wa.AllQuatity * info.CostPrice;
                list.Add(wa);
            }

            var result = list.AsQueryable().ToPagedList(1, 50);

            result.TotalItemCount = row;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;

            if (Request.IsAjaxRequest())
                return PartialView("_WarehouseAnalysis", result);
            return View(result);

        }




        #endregion

        #region ==仓库日志==

        public ActionResult WarehouseLog(StockChangeLogEntity entity)
        {
            if (entity.IntoWarehouse == 0)
            {
                var houselist = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = LoginUserEntity.HospitalID });

                entity.IntoWarehouse = houselist[0].ID;

            }
            entity.StartTime = entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = entity.EndTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59"));
            ViewBag.starttime = entity.StartTime.ToString("yyyy-MM-dd");
            ViewBag.endtime = entity.EndTime.ToString("yyyy-MM-dd");
            int row, pagecount = 0;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; entity.orderDirection = "desc"; }
            entity.orderDirection = "desc";
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HouseID = LoginUserEntity.HospitalID;
            var list = iwareBLL.GetStockChangeLogList(entity, out row, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = row;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_WarehouseLog", result);
            return View(result);
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportWarehouseLog(StockChangeLogEntity entity)
        {
            if (entity.IntoWarehouse == 0)
            {
                var houselist = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = LoginUserEntity.HospitalID });

                entity.IntoWarehouse = houselist[0].ID;

            }
            entity.StartTime = entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = entity.EndTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59"));

            int rows, pagecount = 0;
            entity.numPerPage = 100000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; entity.orderDirection = "desc"; }
            entity.orderDirection = "desc";
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var list = iwareBLL.GetStockChangeLogList(entity, out rows, out pagecount);
            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("类别"), new DataColumn("操作模块"), new DataColumn("更改仓库"), new DataColumn("产品名称"), new DataColumn("原有数量"), new DataColumn("出库"), new DataColumn("进库"), new DataColumn("更改后数量"), new DataColumn("操作者"), new DataColumn("订单编号"), new DataColumn("顾客名称"),  new DataColumn("备注"), new DataColumn("操作时间") });
            foreach (var model in list)
            {
                DataRow row = tableExport.NewRow();
                row["类别"] = model.LogType;
                row["操作模块"] = model.GetClassName();
                row["更改仓库"] = model.IntoWarehouse == 0 ? model.OwnedWarehouseName : model.IntoWarehouseName;
                row["产品名称"] = model.ProductName;
                row["原有数量"] = model.OldQuatity;

                row["出库"] = model.OutQuatity;
                row["进库"] = model.JoinQuantity;
                row["更改后数量"] = model.NewQuatity;
                row["操作者"] = model.UserName;
                row["订单编号"] = model.OrderNo;
                row["顾客名称"] = model.PatientName;
                row["备注"] = model.Memo;
                row["操作时间"] = model.CreateTime;
                tableExport.Rows.Add(row);
            }


            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData1(tableExport, "Cangkurizhi");
            //excel.GenerateData(tableExport, "Cangkurizhi");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "Cangkurizhi.xls");
        }
        #endregion

        #region ==调拨查询报表==

        public ActionResult AllocateReport(StockOrderInfo entity)
        {
            //if (!Request.IsAjaxRequest())
            //{
            //    entity.StartTime = DateTime.Today.AddDays(-1);
            //    entity.EndTime = DateTime.Today;
            //}
            //梳理仓库管理的仓库日志的原有数量和变更数量关联仓库日志接口数据（完成）
            //梳理员工管理的员工销售明细关联员工绩效每月每天时间接口统计逻辑（完成）
            if (entity.StartTime.Year < 1980)
            {
                entity.StartTime = DateTime.Today.AddDays(-1);
            }
            if (entity.EndTime.Year < 1980)
            {
                entity.EndTime = DateTime.Today;
            }
            int SumNumber = 0;
            decimal SumRetailPrice, SumCostPrice = 0;
            int row, pagecount = 0;
            entity.numPerPage = 50;
            entity.orderDirection = "desc";
            entity.orderField = "OrderTime";
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<StockOrderInfo>().AsQueryable().ToPagedList(1, 10));
                }
            }
          //  int row, pagecount = 0;
          //  entity.numPerPage = 50; //每页显示条数
            var list = iwareBLL.GetStockOrderInfoListByDiaoBo(entity, out row, out pagecount, out SumNumber, out SumRetailPrice, out SumCostPrice);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);//添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = row;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;

            ViewBag.SumNumber = SumNumber;
            ViewBag.SumRetailPrice = SumRetailPrice;
            ViewBag.SumCostPrice = SumCostPrice;

            if (Request.IsAjaxRequest())
                return PartialView("_AllocateReport", result);
            return View(result);
        }

        public ActionResult OrderInfoInOtherPage()
        {
            var OrderNO = Request["OrderNO"];
            var model = iwareBLL.GetStockOrderModel(new StockOrderEntity { OrderNo = OrderNO });
            var list = iwareBLL.GetStockOrderInfoList(new StockOrderInfo { StockOrderNo = model.OrderNo });
            var result = list.AsQueryable().ToPagedList(1, 100000);//添加Webdiyer.WebControls.Mvc应用
            return View(result);
        }


        #endregion


        #region ==私有方法==
        /// <summary>
        /// 通过门店和订单类型获取门店编号
        /// </summary>
        /// <param name="HospitalID"></param>
        /// <param name="OrderType"></param>
        /// <returns></returns>
        public string GetDocumentNumber(int HospitalID, int OrderType)
        {
            var number = iwareBLL.GetTodayStockOrderNumber(HospitalID, OrderType, 2);
            var result = "";
            switch (OrderType)
            {
                case 1: result = "C"; break;
                case 2: result = "R"; break;
                case 3: result = "D"; break;
                case 4: result = "P"; break;
                case 5: result = "Y"; break;
                default: break;
            }
            result += DateTime.Today.ToString("yyyyMMdd");
            result += (number + 1).ToString().PadLeft(3, '0');
            return result;
        }

        /// <summary>
        /// 盘点单获取
        /// </summary>
        /// <param name="HospitalID"></param>
        /// <param name="OrderType"></param>
        /// <returns></returns>
        private string GetCheckOrderDocumentNumber(int HospitalID, int OrderType)
        {
            return "s";
        }

        #endregion


        #region==院用产品明细==

        public ActionResult YuanyongProduct(ProductUseUnitEntity entity)
        {
            int rows, pagecount = 0;
            entity.numPerPage = 5000; //每页显示条数
            entity.StartTime = (entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01")));
            entity.EndTime = (entity.EndTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59")));
            if (entity.orderField + "" == "") { entity.orderField = "ProductName"; }
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            //if (entity.pageNum <= 1)
            //{
            //    if (!Request.IsAjaxRequest())
            //    {
            //        return View(new List<ProductUseUnitEntity>().AsQueryable().ToPagedList(1, 10));
            //    }
            //}
            var list = iwareBLL.GetProductUseUnitList(entity, out rows, out pagecount);       //先默认查询出当前的余额

            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);

            result.TotalItemCount = list.Count;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;


            ViewBag.StartTime = entity.StartTime;
            ViewBag.EndTime = entity.EndTime;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_YuanyongProduct", result);
            }
            return View(result);

        }

        public ActionResult YuanyongProductDetail(ProductUseUnitEntity entity)
        {
            entity.StartTime = (entity.StartTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01")));
            entity.EndTime = (entity.EndTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59")));

            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            var list = iwareBLL.GetProductUseUnitDetail(entity);
            return View(list);
        }
        #endregion

    }
}