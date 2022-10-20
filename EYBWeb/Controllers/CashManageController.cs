using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CashManage.Factory.IBLL;
using EYB.ServiceEntity.CashEntity;
using System.Text;
using Common.Helper;
using Webdiyer.WebControls.Mvc;
using HS.SupportComponents;
using PersonnelManage.Factory.IBLL;
using EYB.ServiceEntity.BaseInfo;
using BaseinfoManage.Factory.IBLL;
using System.Configuration;
using EYB.ServiceEntity.PatientEntity;
using ReservationDoctorManage.Factory.IBLL;
using SystemManage.Factory.IDAL;
using PatientManage.Factory.IBLL;
using EYB.ServiceEntity.SystemEntity;
using WarehouseManage.Factory.IBLL;
using EYB.ServiceEntity.WarehouseEntity;
using Common.Attribute;
using System.Data;
using CenterManage.Factory.IBLL;
using System.Threading.Tasks;
using Common.Define;
using Common.Manager;
using EYB.ServiceEntity.MoneyEntity;
using System.Data.SqlClient;

namespace EYB.Web.Controllers
{
    /// <summary>
    /// 前台收银
    /// </summary>
    public partial class CashManageController : BaseController
    {
        #region 依赖注入

        private ICashManageBLL cashBll;
        private IPersonnelManageBLL personnelBLL;
        private IBaseinfoBLL baseinfoBLL; //基础信息模块
        private IReservationDoctorManageBLL ResDocBLL; //预约管理
        private ISystemManageDAL sysbll; //系统管理
        private IPatientManageBLL IpatBLL;
        private IWarehouseManageBLL iwareBLL; //仓库管理
        private ICenterManageBLL iCentBLL; //管理中心

        public static int NewKeVersion = 1;

        public CashManageController()
        {
            NewKeVersion = ConvertValueHelper.ConvertIntValue(ConfigurationManager.AppSettings["NewKeVersion"]);
            cashBll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer()
                .Resolve<CashManage.Factory.IBLL.ICashManageBLL>();
            personnelBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer()
                .Resolve<IPersonnelManageBLL>();
            baseinfoBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer()
                .Resolve<IBaseinfoBLL>();
            ResDocBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer()
                .Resolve<IReservationDoctorManageBLL>();
            IpatBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer()
                .Resolve<IPatientManageBLL>();
            sysbll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer()
                .Resolve<ISystemManageDAL>();
            iwareBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer()
                .Resolve<IWarehouseManageBLL>();
            iCentBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer()
                .Resolve<ICenterManageBLL>();

        }

        #endregion 依赖注入

        #region==项目产品==


        /// <summary>
        /// 项目产品——消费界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Consumer()
        {

            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        /// <summary>
        /// 项目产品——消费界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Consumerliaochen()
        {

            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        /// <summary>
        /// 项目产品——消费界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Consumerchangping()
        {

            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        /// <summary>
        /// 项目产品——消费界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Consumerdanci()
        {

            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
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
        /// 选择客户
        /// </summary>
        /// <returns></returns>
        public ActionResult _PatientSelect()
        {
            return PartialView();
        }

        /// <summary>
        /// 前台收银控件
        /// </summary>
        /// <returns></returns>
        public ActionResult _CashControl()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return View();
        }
        /// <summary>
        /// 根据顾客ID获取账户信息
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetAllBalance(int UserID)
        {
            var entity = cashBll.GetAllBalance(new MainCardBalanceEntity { UserID = UserID });
            if (LoginUserEntity.HospitalID == 1017)
            {
                MainCardBalanceEntity mcb = new MainCardBalanceEntity();
                mcb.UserID = UserID;
                mcb.HospitalID = LoginUserEntity.HospitalID;
                var mcbModel = cashBll.GetAllBalanceUserID(mcb);
                if (mcbModel.ID == 0)
                {

                    Random ran = new Random();
                    string orderNumber = "U" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ran.Next(1000, 9999).ToString();
                    int caridID = 0;
                    if (LoginUserEntity.HospitalID == 2)
                    {
                        caridID = 40112;
                    }
                    if (LoginUserEntity.HospitalID == 1017)
                    {
                        caridID = 42221;
                    }
                    UserCardEntity usermodel = new UserCardEntity();
                    usermodel.CardID = caridID;
                    usermodel.UserID = UserID;
                    usermodel.Price = 0;
                    usermodel.CreateTime = DateTime.Now;
                    usermodel.IsActive = 1;
                    int cardid = cashBll.AddUserCard(usermodel);
                    MainCardBalanceEntity mcar = new MainCardBalanceEntity();
                    mcar.CardID = cardid;
                    mcar.ProjectID = caridID;
                    mcar.UserID = UserID;
                    mcar.Price = 0;
                    mcar.Tiems = 0;
                    mcar.KouTimes = 0;
                    mcar.AllTiems = 0;
                    mcar.Type = 1;
                    mcar.UserCardID = cardid;
                    mcar.Weikuan = 0;
                    mcar.AllPrice = 0;
                    mcar.HuaKouZheLv = 1;
                    mcar.ConsumePrice = 0;
                    mcar.OrderNo = orderNumber;
                    var chuzhi = baseinfoBLL.GetPrePayCardModel(caridID);  //更具储值卡ID获取储值卡信息.
                    DateTime et = DateTime.Now;
                    et = et.AddDays(chuzhi.VaildityDay);
                    mcar.ExpireTime = et;
                    mcar.AddTime = DateTime.Now;
                    long itndd = cashBll.AddMainCardBalance(mcar);
                    OrderEntity orderModel = new OrderEntity();
                    orderModel.OrderNo = orderNumber;
                    orderModel.CreateTime = DateTime.Now;
                    orderModel.OrderTime = DateTime.Now;
                    orderModel.HospitalID = LoginUserEntity.HospitalID;
                    orderModel.Statu = 1;
                    orderModel.ReservationTime = DateTime.Now;
                    orderModel.OrderType = 2;
                    orderModel.ReservationType = 1;
                    orderModel.DocumentNumber = "1128003";
                    orderModel.BuyType = 1;
                    orderModel.ISAudit = 1;
                    string pp = cashBll.AddOrder(orderModel);
                }
            }
            var intrgralMode = IpatBLL.GetIntegralModel(entity.UserID); //获取原来的积分列表
            if (intrgralMode.ID > 0)
            {
                var MainCardBalanceModel = cashBll.GetAllBalanceUserID(new MainCardBalanceEntity { UserID = UserID, HospitalID = LoginUserEntity.HospitalID });
                if (MainCardBalanceModel.ID > 0)
                {
                    MainCardBalanceModel.Price = intrgralMode.Integral;
                    MainCardBalanceModel.AllPrice = intrgralMode.Integral;
                    int count = cashBll.UpdateMainCardBalancePrice(MainCardBalanceModel);
                }
            }
            return Json(entity);
            //  var entity = cashBll.GetAllBalance(new MainCardBalanceEntity { UserID = UserID });
            // return Json(entity);
        }

        /// <summary>
        /// 根据客户ID获取剩余项目次数
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetProjectBalance(int UserID, int ID)
        {
            var list = cashBll.GetProjectBalance(UserID, ID, LoginUserEntity.ParentHospitalID);
            return Json(list);
        }

        /// <summary>
        /// 根据客户ID获取剩余项目次数---组装下拉框用途
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetProjectBalanceText(int UserID, int ID)
        {
            // var list = cashBll.GetProjectBalance(UserID, ID, LoginUserEntity.ParentHospitalID);原来
            var list = cashBll.GetProjectBalanceIsDel(UserID, ID, LoginUserEntity.ParentHospitalID);
            StringBuilder str = new StringBuilder();
            //str.Append("<option value='-999'>请选择项目</option>");
            //foreach (var info in list)
            //{
            //    str.AppendFormat("<option value={0}>{1}</option>", info.ID, info.Name + "(剩余 " + info.Tiems + " 次）");
            //}
            str.AppendFormat(
                "<li class='liMenu' style='width:100%;height:27px; line-height:27px; text-align:center; border:1px solid #ccc;' > 疗程划次</li>");
            if (list.Count != 0)
            {
                str.AppendFormat("<ul class='liHide ulClass' style='width:150%; left:0px;'>");
                foreach (var info in list)
                {
                    str.AppendFormat(
                        "<li class='liClass' style='width:100%;height:20px; line-height:20px; overflow:hidden;color:{8}'  {4} ><input  {5} code='{2}' type='checkbox' id='{0}' value='{1}' alltimes={3} name='ProjectID' /><label for='{0}'>{2}(总:{7}次,余{3}次，消耗:{6}) </label><label style='float:right;'>{9}</label></li>",
                        "chk1(" + info.ID + ")", info.ID, info.Name, info.Tiems,
                        ((info.IsActive == 1 || DateTime.Compare(DateTime.Now, info.ExpireTime) > 0)
                            ? "disabled=disabled"
                            : ""),
                        ((info.IsActive == 1 || DateTime.Compare(DateTime.Now, info.ExpireTime) > 0)
                            ? "disabled=disabled"
                            //: ""), info.Price, info.AllTiems,
                            : ""), info.ConsumePrice, info.AllTiems,
                        ((info.IsActive == 1 || DateTime.Compare(DateTime.Now, info.ExpireTime) > 0)
                            ? "#FF4040"
                            : "#888"),
                        (String.IsNullOrEmpty(info.MemoContent) ? "" :
                            info.MemoContent.Length >25 ? info.MemoContent.Substring(0, 25) : info.MemoContent));
                }

                str.AppendFormat("</ul>");
            }

            return Content(str.ToString());
        }

        /// <summary>
        /// 根据客户ID获取剩余储值卡结余金额
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetMainCardBalanceText(MainCardBalanceEntity entity)
        {
            decimal chongzhi, xiaofei = 0;
            decimal Shengyu = 0;
            int row, pagecount = 0;
            entity.Type = 1;
            entity.numPerPage = 50;
            entity.HospitalID = LoginUserEntity.ParentHospitalID;
            var list = cashBll.GetMainCardBalanceListInUser(entity, out row, out pagecount, out chongzhi, out xiaofei,
                out Shengyu);
            list = list.Where(t => t.IsDel != 3).ToList();
            StringBuilder str = new StringBuilder();
            if (list.Count != 0)
            {
                str.AppendFormat("<ul   style='width:500px; left:0px;'>");
                foreach (var info in list)
                {
                    str.AppendFormat(
                        "<li class='liClass' style='width:100%;height:25px; line-height:25px; overflow:hidden;padding:3px;'><a style='width:100%;height:25px; line-height:25px; overflow:hidden;padding:3px;' href='javascript:;'><label>{0}(余额:{1}) </label>套盒折扣：<label class='taohezhekou'>{2}</label >院用折扣：<label  class='yuanyongzhekou'>{3}</label>家居折扣：<label  class='jiajuzhekou'>{4}</label></a></li>",
                        info.Name, info.Price, info.Taohezhekou, info.Yuanyongzhekou, info.Jiajuzhekou);
                }

                str.AppendFormat("</ul>");
            }

            return Content(str.ToString());
        }

        public ActionResult UpdateMemo(OrderEntity entity)
        {
            return View();
        }

        /// <summary>
        /// 修改备注
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult UpdateMemoInfo(OrderEntity entity)
        {
            return Json(cashBll.UpdateMemo(entity));
        }

        #endregion

        #region==提交订单 订单支付 操作 ==

        /// <summary>
        /// 保存订单---结账操作
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult SaveOrder(OrderEntity entity, string Memo)
        {
            //汪  7-24 补单及到店时间的修改
            PatientEntity list = new PatientEntity();
            list = IpatBLL.GetPatienttEntityByID(entity.UserID);
            var results = 0;

            //罗发明 补单时间时分秒换成实际的
            //var hms = DateTime.Now.ToString("hh:mm:ss").ToString();
            var hms = DateTime.Now.ToLongTimeString().ToString();
            var timess = "yyyy-MM-dd " + hms;

            if (entity.CheckCreateTime == 0)
            {
                //修改顾客最后到店时间
                if (entity.CreateTime == Convert.ToDateTime("0001/1/1 0:00:00"))
                {
                    entity.CreateTime = DateTime.Now;
                    entity.OrderTime = entity.CreateTime;
                }
                else
                {

                    entity.CreateTime = Convert.ToDateTime(entity.CreateTime.ToString(timess));
                    //entity.CreateTime = Convert.ToDateTime(entity.CreateTime.ToString("yyyy-MM-dd 10:10:10"));
                    entity.OrderTime = entity.CreateTime;
                    // entity.CreateTime = DateTime.Now;
                    //entity.OrderTime = entity.CreateTime;
                }
            }
            else
            {

                entity.CreateTime = Convert.ToDateTime(entity.CreateTime.ToString(timess));
                //entity.CreateTime = Convert.ToDateTime(entity.CreateTime.ToString("yyyy-MM-dd 10:10:10"));
                entity.OrderTime = entity.CreateTime;
            }
            //汪 end

            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
            entity.Statu = (int)OrderStatu.处理中;
            entity.OrderType = (int)OrderType.项目产品;
            entity.Name = Memo;
            string[] orderprice = Request.Form.GetValues("orderprice"); //总价
            string[] ordertimes = Request.Form.GetValues("ordertimes"); //总数
            string[] GiveIDList = Request.Form.GetValues("GiveIDList"); //主键ID
            string[] TypeList = Request.Form.GetValues("TypeList"); //1项目 2产品
            string[] CardType = Request.Form.GetValues("CardType"); //1、扣卡类 2现金类
            string[] PriceList = Request.Form.GetValues("PriceList"); //1、单价
            string[] IsZhidingList = Request.Form.GetValues("IsZhiding"); //1、是否指定
            string[] XiaoHao = Request.Form.GetValues("XiaoHao"); //1、消耗
            string[] Ticheng = Request.Form.GetValues("Ticheng"); //1、手工
            string[] CardID = Request.Form.GetValues("CardID"); //1、卡ID
            var patientEntity = IpatBLL.GetPatienttEntityByID(entity.UserID);
            var mendian = personnelBLL.HospitalEntityByID(LoginUserEntity.HospitalID);
            string time = entity.CreateTime.ToString("MMdd");
            int count = cashBll.GetTodayOrderCount(LoginUserEntity.HospitalID, entity.CreateTime); //获取总数
            entity.DocumentNumber = mendian.Prefix + time + (count + 1).ToString("d3");
            count = 0;
            entity.BuyType = 4;

            //添加订单主表
            var result = cashBll.AddOrder(entity);
            if (result != null)
            {
                OrderinfoEntity orderinfo = new OrderinfoEntity();
                orderinfo.OrderNo = entity.OrderNo;
                for (int i = 0; i < orderprice.Length; i++)
                {
                    orderinfo.Quantity = ConvertValueHelper.ConvertIntValue(ordertimes[i]); //这个是数量
                    //如果数量为0跳出本次添加订单项目信息
                    if (orderinfo.Quantity == 0)
                        continue;
                    orderinfo.Type = ConvertValueHelper.ConvertIntValue(TypeList[i]); //类型
                    orderinfo.AllPrice = Convert.ToDecimal(orderprice[i]); //这个是实际总价
                    orderinfo.ProdcuitID = ConvertValueHelper.ConvertIntValue(GiveIDList[i]); //产品ID
                    orderinfo.CardType = ConvertValueHelper.ConvertIntValue(CardType[i]);
                    orderinfo.Price = orderinfo.Quantity < 0
                        ? -Convert.ToDecimal(PriceList[i])
                        : Convert.ToDecimal(PriceList[i]); //这个价格是产品的原本单价
                    orderinfo.CardID = ConvertValueHelper.ConvertIntValue(CardID[i]);
                    if (orderinfo.Type == 1) //判断是否是项目，如果是则看下他是否是对半业务属性
                    {
                        var projectEntity = baseinfoBLL.GetProjectModel(orderinfo.ProdcuitID);
                        orderinfo.ProdcuitName = projectEntity.Name;
                        orderinfo.ProductType = projectEntity.ProjectType;
                        orderinfo.BrandID = projectEntity.BrandID;
                        orderinfo.BrandName = iwareBLL.GetBrandModel(new BrandEntity { ID = projectEntity.BrandID }).Name;
                        orderinfo.ProductTypeName = iwareBLL
                            .GetProductTypeModel(new ProductTypeEntity { ID = projectEntity.ProjectType }).Name;
                        if (projectEntity.IsHalf == 1) //如果是，则更新订单表的IsHalf 属性
                        {
                            cashBll.UpdateOrderIsHalf(new OrderEntity { OrderNo = entity.OrderNo, IsHalf = projectEntity.IsHalf });
                        }
                    }
                    else
                    {
                        var projectEntity = baseinfoBLL.GetModel(orderinfo.ProdcuitID);
                        orderinfo.ProdcuitName = projectEntity.Name;
                        orderinfo.ProductType = projectEntity.ProductType;
                        orderinfo.BrandID = projectEntity.BrandID;
                        orderinfo.BrandName = iwareBLL.GetBrandModel(new BrandEntity { ID = projectEntity.BrandID }).Name;
                        orderinfo.ProductTypeName = iwareBLL
                            .GetProductTypeModel(new ProductTypeEntity { ID = projectEntity.ProductType }).Name;
                    }

                    if (orderinfo.AllPrice == 0)
                    {
                        //如果总价为0(即优惠价为0),则代表是赠送产品,添加赠送金额
                        orderinfo.SendAmount = orderinfo.Price * orderinfo.Quantity;
                    }

                    //折让金额=理论总额-实际总额    理论总额=原本单价*数量
                    orderinfo.DiscountAmount = orderinfo.Price * orderinfo.Quantity - orderinfo.AllPrice < 0
                        ? Math.Abs(orderinfo.AllPrice)
                        : orderinfo.AllPrice;

                    if (orderinfo.CardType == 1) //如果是卡扣类的操作则算出一个即时数量
                    {
                        orderinfo.InfoBuyType = 3;
                        var kakouinfo = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = orderinfo.CardID });
                        orderinfo.InstantInventory = kakouinfo.Tiems - orderinfo.Quantity;
                    }
                    else
                    {
                        if (orderinfo.Type == 1)
                        {
                            orderinfo.InfoBuyType = 5;
                        }
                        else
                        {
                            orderinfo.InfoBuyType = 4;
                        }

                        orderinfo.InstantInventory = 0;
                    }

                    //添加详情信息表
                    results = cashBll.AddOrderinfo(orderinfo);
                    //
                    //获取参与员工
                    string[] UserIDList =
                        Request.Form.GetValues("UserID-" + orderinfo.Type + "-" + orderinfo.CardID + "-" +
                                               orderinfo.ProdcuitID); //1、手工

                    if (results > 0)
                    {
                        JoinUserEntity model = new JoinUserEntity();
                        for (int j = 0; j < UserIDList.Length; j++)
                        {
                            model.UserID = ConvertValueHelper.ConvertIntValue(UserIDList[j]);
                            model.IsZhiding = ConvertValueHelper.ConvertIntValue(IsZhidingList[i]) > 0 ? ConvertValueHelper.ConvertIntValue(IsZhidingList[i]) : ((patientEntity.FollowUpMrUserID == ConvertValueHelper.ConvertIntValue(UserIDList[j]) || patientEntity.FollowUpUserID == ConvertValueHelper.ConvertIntValue(UserIDList[j])) ? 1 : 0); // ConvertValueHelper.ConvertIntValue(IsZhidingList[i]);
                            model.OrderNo = entity.OrderNo;
                            model.ProjectID = orderinfo.ProdcuitID;
                            model.Type = orderinfo.Type;
                            model.OrderInfoID = results;
                            //消耗：划扣次数才会产生消耗、储值卡支付项目 产生消耗
                            //if (!string.IsNullOrEmpty(XiaoHao[i]))//如果消耗不为空
                            //{
                            //    model.XiaoHao = ConvertValueHelper.ConvertIntValue(XiaoHao[i]);//消耗
                            //    model.XiaoHao_Liaocheng = model.XiaoHao;
                            //}

                            //手工 只有项目才有手工费用
                            if (!string.IsNullOrEmpty(Ticheng[i]) && model.Type == 1)//如果提成不为空
                            {
                                model.Ticheng = ConvertValueHelper.ConvertDecimalValue(Ticheng[i]);
                            }

                            //if (model.Type == 1)//如果是项目则存手工,产品是没有手工的,产品是不可能手工的
                            //{
                            //    //获取项目的提成
                            //    var royaltyList = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = orderinfo.ProdcuitID });
                            //    decimal shougong = 0;
                            //    decimal othershougong = 0;
                            //    CaculateRoyalty(orderinfo.Price * orderinfo.Quantity, orderinfo.AllPrice, royaltyList, out shougong, out othershougong);
                            //    if (model.Ticheng == 0)
                            //        model.Ticheng = shougong * orderinfo.Quantity;//手工(算出来的单个手工乘以数量)
                            //    if (model.IsZhiding == 0)//如果非指定
                            //    {
                            //       model.OtherTiCheng =0;
                            //    }else
                            //    {
                            //       model.OtherTiCheng =othershougong * orderinfo.Quantity;//额外手工
                            //    }
                            //}

                            //业绩
                            //  model.Yeji = ConvertValueHelper.ConvertDecimalValue((orderinfo.Price / UserIDList.Length).ToString("f2"));//正常员工业绩就是项目或产品的价格
                            //为项目或者划卡时,
                            if (orderinfo.InfoBuyType == 5 || orderinfo.InfoBuyType == 3)
                            {
                                model.ProjectNumber = Convert.ToDecimal(1 / UserIDList.Count());
                                model.KeShu = (float)model.ProjectNumber;
                                if (model.IsZhiding == 1)
                                {
                                    model.KeShu_zhiding = (float)model.ProjectNumber;
                                }
                                else
                                {
                                    model.KeShu_feizhiding = (float)model.ProjectNumber;
                                }

                                model.ProjectNumber = Convert.ToDecimal(orderinfo.Quantity) /
                                                      Convert.ToDecimal(UserIDList.Count());
                            }

                            cashBll.AddJoinUser(model);
                        }
                    }
                }

            }

            //返回订单编号
            return Json(entity.OrderNo);
        }
        private string GetIntegralByMonth(string sqlstr, int filter = 0)
        {

            string month = "0";
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
                    month = date["Quatitys"].ToString();
                }
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return month;

        }
        /// <summary>
        /// 保存订单---结账操作
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult SaveOrder1(OrderEntity entity, string Memo)
        {
            //汪  7-24 补单及到店时间的修改
            PatientEntity list = new PatientEntity();
            list = IpatBLL.GetPatienttEntityByID(entity.UserID);
            var results = 0;

            //罗发明 补单时间时分秒换成实际的
            //var hms = DateTime.Now.ToString("hh:mm:ss").ToString();
            var hms = DateTime.Now.ToLongTimeString().ToString();
            var timess = "yyyy-MM-dd " + hms;

            if (entity.CheckCreateTime == 0)
            {
                //修改顾客最后到店时间
                if (entity.CreateTime == Convert.ToDateTime("0001/1/1 0:00:00"))
                {
                    entity.CreateTime = DateTime.Now;
                    entity.OrderTime = entity.CreateTime;
                }
                else
                {

                    entity.CreateTime = Convert.ToDateTime(entity.CreateTime.ToString(timess));
                    //entity.CreateTime = Convert.ToDateTime(entity.CreateTime.ToString("yyyy-MM-dd 10:10:10"));
                    entity.OrderTime = entity.CreateTime;
                    // entity.CreateTime = DateTime.Now;
                    //entity.OrderTime = entity.CreateTime;
                }
            }
            else
            {

                entity.CreateTime = Convert.ToDateTime(entity.CreateTime.ToString(timess));
                //entity.CreateTime = Convert.ToDateTime(entity.CreateTime.ToString("yyyy-MM-dd 10:10:10"));
                entity.OrderTime = entity.CreateTime;
            }
            //汪 end

            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
            entity.Statu = (int)OrderStatu.处理中;
            entity.OrderType = (int)OrderType.项目产品;
            entity.Name = Memo;
            string[] orderprice = Request.Form.GetValues("orderprice"); //总价
            string[] ordertimes = Request.Form.GetValues("ordertimes"); //总数
            string[] GiveIDList = Request.Form.GetValues("GiveIDList"); //主键ID
            string[] TypeList = Request.Form.GetValues("TypeList"); //1项目 2产品
            string[] CardType = Request.Form.GetValues("CardType"); //1、扣卡类 2现金类
            string[] PriceList = Request.Form.GetValues("PriceList"); //1、单价
            string[] IsZhidingList = Request.Form.GetValues("IsZhiding"); //1、是否指定
            string[] XiaoHao = Request.Form.GetValues("XiaoHao"); //1、消耗
            string[] Ticheng = Request.Form.GetValues("Ticheng"); //1、手工
            string[] CardID = Request.Form.GetValues("CardID"); //1、卡ID
            var patientEntity = IpatBLL.GetPatienttEntityByID(entity.UserID);
            var mendian = personnelBLL.HospitalEntityByID(LoginUserEntity.HospitalID);
            string time = entity.CreateTime.ToString("MMdd");
            int count = cashBll.GetTodayOrderCount(LoginUserEntity.HospitalID, entity.CreateTime); //获取总数
            entity.DocumentNumber = mendian.Prefix + time + (count + 1).ToString("d3");
            count = 0;
            entity.BuyType = 4;
            var filter = "";
            //添加订单主表
            var result = cashBll.AddOrder(entity);
            if (result != null)
            {
                OrderinfoEntity orderinfo = new OrderinfoEntity();
                orderinfo.OrderNo = entity.OrderNo;
                for (int i = 0; i < orderprice.Length; i++)
                {
                    orderinfo.Quantity = ConvertValueHelper.ConvertIntValue(ordertimes[i]); //这个是数量

                    orderinfo.Type = ConvertValueHelper.ConvertIntValue(TypeList[i]); //类型
                    if (orderinfo.Type == 1) //判断是否是项目，如果是则看下他是否是对半业务属性
                    { }
                    else
                    {
                        string strsql = @"	SELECT SUM (Quatity) as  Quatitys FROM  EYB_tb_ProductStock g
				                    WHERE g.ProductID = " + ConvertValueHelper.ConvertIntValue(GiveIDList[i]) + @"  AND HospitalID = " + LoginUserEntity.HospitalID + @" GROUP BY g.ProductID";
                        var quatity = GetIntegralByMonth(strsql, 1) == "" ? "0" : GetIntegralByMonth(strsql, 1);
                        var stokequatity = Convert.ToInt32(quatity);
                        //var prodcuitName = baseinfoBLL.GetModel(ConvertValueHelper.ConvertIntValue(GiveIDList[i])).Name;
                        var updatequantity = orderinfo.Quantity;

                        //var projectEntity = baseinfoBLL.GetModel(ConvertValueHelper.ConvertIntValue(GiveIDList[i]));
                        //orderinfo.ProdcuitName = projectEntity.Name;
                        //var cha = updatequantity - stokequatity;
                        if (stokequatity < updatequantity)
                        {
                            //filter += orderinfo.ProdcuitName + ":差"+ cha + ";";
                            filter = "库存不足";
                        }
                    }
                    //如果数量为0跳出本次添加订单项目信息
                    if (orderinfo.Quantity == 0)
                    continue;
                    orderinfo.AllPrice = Convert.ToDecimal(orderprice[i]); //这个是实际总价
                    orderinfo.ProdcuitID = ConvertValueHelper.ConvertIntValue(GiveIDList[i]); //产品ID
                    orderinfo.CardType = ConvertValueHelper.ConvertIntValue(CardType[i]);
                    orderinfo.Price = orderinfo.Quantity < 0
                        ? -Convert.ToDecimal(PriceList[i])
                        : Convert.ToDecimal(PriceList[i]); //这个价格是产品的原本单价
                    orderinfo.CardID = ConvertValueHelper.ConvertIntValue(CardID[i]);
                    if (orderinfo.Type == 1) //判断是否是项目，如果是则看下他是否是对半业务属性
                    {
                        var projectEntity = baseinfoBLL.GetProjectModel(orderinfo.ProdcuitID);
                        orderinfo.ProdcuitName = projectEntity.Name;
                        orderinfo.ProductType = projectEntity.ProjectType;
                        orderinfo.BrandID = projectEntity.BrandID;
                        orderinfo.BrandName = iwareBLL.GetBrandModel(new BrandEntity { ID = projectEntity.BrandID }).Name;
                        orderinfo.ProductTypeName = iwareBLL.GetProductTypeModel(new ProductTypeEntity { ID = projectEntity.ProjectType }).Name;
                        if (projectEntity.IsHalf == 1) //如果是，则更新订单表的IsHalf 属性
                        {
                            cashBll.UpdateOrderIsHalf(new OrderEntity { OrderNo = entity.OrderNo, IsHalf = projectEntity.IsHalf });
                        }
                    }
                    else
                    {
                        var projectEntity = baseinfoBLL.GetModel(orderinfo.ProdcuitID);
                        orderinfo.ProdcuitName = projectEntity.Name;
                        orderinfo.ProductType = projectEntity.ProductType;
                        orderinfo.BrandID = projectEntity.BrandID;
                        orderinfo.BrandName = iwareBLL.GetBrandModel(new BrandEntity { ID = projectEntity.BrandID }).Name;
                        orderinfo.ProductTypeName = iwareBLL
                            .GetProductTypeModel(new ProductTypeEntity { ID = projectEntity.ProductType }).Name;
                    }

                    if (orderinfo.AllPrice == 0)
                    {
                        //如果总价为0(即优惠价为0),则代表是赠送产品,添加赠送金额
                        orderinfo.SendAmount = orderinfo.Price * orderinfo.Quantity;
                    }

                    //折让金额=理论总额-实际总额    理论总额=原本单价*数量
                    orderinfo.DiscountAmount = orderinfo.Price * orderinfo.Quantity - orderinfo.AllPrice < 0
                        ? Math.Abs(orderinfo.AllPrice)
                        : orderinfo.AllPrice;

                    if (orderinfo.CardType == 1) //如果是卡扣类的操作则算出一个即时数量
                    {
                        orderinfo.InfoBuyType = 3;
                        var kakouinfo = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = orderinfo.CardID });
                        orderinfo.InstantInventory = kakouinfo.Tiems - orderinfo.Quantity;
                    }
                    else
                    {
                        if (orderinfo.Type == 1)
                        {
                            orderinfo.InfoBuyType = 5;
                        }
                        else
                        {
                            orderinfo.InfoBuyType = 4;
                        }

                        orderinfo.InstantInventory = 0;
                    } 

                    //添加详情信息表
                    results = cashBll.AddOrderinfo(orderinfo);
                    //
                    //获取参与员工
                    string[] UserIDList =
                        Request.Form.GetValues("UserID-" + orderinfo.Type + "-" + orderinfo.CardID + "-" +
                                               orderinfo.ProdcuitID); //1、手工

                    if (results > 0)
                    {
                        JoinUserEntity model = new JoinUserEntity();
                        for (int j = 0; j < UserIDList.Length; j++)
                        {
                            model.UserID = ConvertValueHelper.ConvertIntValue(UserIDList[j]);
                            model.IsZhiding = ConvertValueHelper.ConvertIntValue(IsZhidingList[i]) > 0 ? ConvertValueHelper.ConvertIntValue(IsZhidingList[i]) : ((patientEntity.FollowUpMrUserID == ConvertValueHelper.ConvertIntValue(UserIDList[j]) || patientEntity.FollowUpUserID == ConvertValueHelper.ConvertIntValue(UserIDList[j])) ? 1 : 0); // ConvertValueHelper.ConvertIntValue(IsZhidingList[i]);
                            model.OrderNo = entity.OrderNo;
                            model.ProjectID = orderinfo.ProdcuitID;
                            model.Type = orderinfo.Type;
                            model.OrderInfoID = results;
                            //消耗：划扣次数才会产生消耗、储值卡支付项目 产生消耗
                            //if (!string.IsNullOrEmpty(XiaoHao[i]))//如果消耗不为空
                            //{
                            //    model.XiaoHao = ConvertValueHelper.ConvertIntValue(XiaoHao[i]);//消耗
                            //    model.XiaoHao_Liaocheng = model.XiaoHao;
                            //}

                            //手工 只有项目才有手工费用
                            if (!string.IsNullOrEmpty(Ticheng[i]) && model.Type == 1)//如果提成不为空
                            {
                                model.Ticheng = ConvertValueHelper.ConvertDecimalValue(Ticheng[i]);
                            }

                            //if (model.Type == 1)//如果是项目则存手工,产品是没有手工的,产品是不可能手工的
                            //{
                            //    //获取项目的提成
                            //    var royaltyList = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = orderinfo.ProdcuitID });
                            //    decimal shougong = 0;
                            //    decimal othershougong = 0;
                            //    CaculateRoyalty(orderinfo.Price * orderinfo.Quantity, orderinfo.AllPrice, royaltyList, out shougong, out othershougong);
                            //    if (model.Ticheng == 0)
                            //        model.Ticheng = shougong * orderinfo.Quantity;//手工(算出来的单个手工乘以数量)
                            //    if (model.IsZhiding == 0)//如果非指定
                            //    {
                            //       model.OtherTiCheng =0;
                            //    }else
                            //    {
                            //       model.OtherTiCheng =othershougong * orderinfo.Quantity;//额外手工
                            //    }
                            //}

                            //业绩
                            //  model.Yeji = ConvertValueHelper.ConvertDecimalValue((orderinfo.Price / UserIDList.Length).ToString("f2"));//正常员工业绩就是项目或产品的价格
                            //为项目或者划卡时,
                            if (orderinfo.InfoBuyType == 5 || orderinfo.InfoBuyType == 3)
                            {
                                model.ProjectNumber = Convert.ToDecimal(1 / UserIDList.Count());
                                model.KeShu = (float)model.ProjectNumber;
                                if (model.IsZhiding == 1)
                                {
                                    model.KeShu_zhiding = (float)model.ProjectNumber;
                                }
                                else
                                {
                                    model.KeShu_feizhiding = (float)model.ProjectNumber;
                                }

                                model.ProjectNumber = Convert.ToDecimal(orderinfo.Quantity) /
                                                      Convert.ToDecimal(UserIDList.Count());
                            }

                            cashBll.AddJoinUser(model);
                        }
                    }
                }

            }
            var ft = "|" + filter;
            if (string.IsNullOrEmpty(filter))
            {
                return Json(entity.OrderNo);
            }
            else
            {
                return Json(entity.OrderNo + ft);
            } 
        }
        /// <summary>
        ///  修改订单状态、支付金额---支付操作
        /// </summary>
        /// <param name="entity">订单实体</param>
        /// <param name="CardIDstr">用户卡扣余额ID集合</param>
        /// <param name="ordertimesstr">卡扣次数集合</param>
        /// <param name="cardidarr">用户余额记录储值卡ID集合</param>
        /// <param name="txtChuzhikaarr">储值金额集合</param>
        /// <param name="OtherPayment">其他支付方式ID集合</param>
        /// <param name="OtherPaymentValue">其他支付方式支付金额</param>
        /// <param name="couponNo">优惠券号</param>
        /// <param name="depositValue">股本支付金额</param>
        /// <returns></returns>
        [WriteOperateLogAttribute("项目产品点击支付操作，支付订单编号：", "项目产品点击支付操作", "项目产品")]
        public ActionResult UpdateOrderNew(OrderEntity entity, string CardIDstr, string ordertimesstr, string cardidarr,
                string txtChuzhikaarr, string OtherPayment, string IsCashValue, string IsGiveValue,
                string OtherPaymentValue, string preOrderNOList, string hiddenToken, int chkcreatetime,
                string txtCreateTime, string couponNo, decimal CouponPay, decimal depositValue)
        {

            //防止重复提交
            if (Session["hiddenToken"] == null)
            {
                Session["hiddenToken"] = hiddenToken;
            }
            else
            {
                if (Session["hiddenToken"].ToString() == hiddenToken)
                {
                    LogWriter.WriteInfo("重复支付记录" + entity.OrderNo, "重复支付记录");
                    return Json(-99);
                }
            }

            //操作日志参数
            ViewBag.CacheData = entity.OrderNo;
            //try
            //{
            if (!haveCangku(LoginUserEntity.HospitalID))
            {
                return Json(-2); //如果没有仓库直接返回-2
            }

            if (entity.QianPrice > 0)
            {
                entity.QianPrice = -entity.QianPrice;
            }

            entity.Statu = (int)OrderStatu.已结算;
            entity.ActurePrice = entity.Price + entity.QianPrice;
            entity.AllQianprice = entity.QianPrice;
            //修改订单为已支付
            int result = cashBll.UpdateOrder(entity);
            //根据用户ID更新最后到店时间
            int count = cashBll.Patient_UpdateLastTime1(entity.UserID,Convert.ToDateTime(txtCreateTime));

            decimal OtherCash = 0; //其他方式支付的 现金部分
            decimal Otherquan = 0;//其他劵类支付  部分
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            Dictionary<int, int> isgiveDic = new Dictionary<int, int>();
            Dictionary<int, decimal> otherpay = new Dictionary<int, decimal>(); //其他支付方式金额
            Dictionary<int, int> otherpaytype = new Dictionary<int, int>(); //其他支付方式类型
            decimal allchuzhikaPay = 0;//获取非赠卡支付金额
            decimal allzengchuzhikaPay = 0;//获取赠送储值卡支付金额
            #region==订单支付方式==

            //订单结算方式记录
            if (result > 0)
            {
                //删除已挂单操作
                if (!string.IsNullOrEmpty(preOrderNOList))
                {
                    string[] preOrderNOList1 =
                        preOrderNOList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var orderno in preOrderNOList1)
                    {
                        cashBll.DeleteOrder(orderno);
                    }
                }


                //添加订单支付方式
                PaymentOrderEntity poe = new PaymentOrderEntity();



                //其他支付方式
                if (!string.IsNullOrEmpty(OtherPayment) && !string.IsNullOrEmpty(OtherPaymentValue))
                {
                    string[] strpay = OtherPayment.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] OtherPaymentValuelist =
                        OtherPaymentValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] IsCashValueStr =
                        IsCashValue.Split(new char[] { ',' },
                            StringSplitOptions.RemoveEmptyEntries); //此字段判断是否是 现金还是券类 1 现金 2 券类


                    for (int i = 0; i < strpay.Length; i++)
                    {

                        poe.OrderNo = entity.OrderNo;
                        poe.PayType = ConvertValueHelper.ConvertIntValue(strpay[i]);
                        poe.PayMoney = entity.Price < 0
                            ? -ConvertValueHelper.ConvertDecimalValue(OtherPaymentValuelist[i])
                            : ConvertValueHelper.ConvertDecimalValue(OtherPaymentValuelist[i]);
                        otherpay.Add(poe.PayType, poe.PayMoney); //其他支付方式
                        // poe.PayMoney = ;
                        poe.BalanceMoney = 0;
                        poe.BalanceTiems = 0;
                        poe.CardID = 0;
                        if (ConvertValueHelper.ConvertIntValue(IsCashValueStr[i]) == 1)
                        {
                            poe.ParentPayType = (int)ParentPayType.现金;
                            OtherCash = OtherCash + poe.PayMoney;

                        }
                        else
                        {
                            Otherquan = Otherquan + poe.PayMoney;
                            poe.ParentPayType = (int)ParentPayType.券类;
                        }

                        otherpaytype.Add(poe.PayType, poe.ParentPayType); //其他支付方式
                        poe.PayName = sysbll.GetBaseinfoEntity(ConvertValueHelper.ConvertIntValue(strpay[i])).InfoName;
                        cashBll.AddPaymentOrder(poe);
                    }
                }

                if (entity.Xianjin > 0) //现金
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.现金;
                    //退产品
                    poe.PayMoney = entity.Price < 0 ? -entity.Xianjin : entity.Xianjin;
                    poe.PayName = "现金";
                    poe.BalanceMoney = 0;
                    poe.BalanceTiems = 0;
                    poe.CardID = 0;
                    poe.ParentPayType = (int)ParentPayType.现金;

                    cashBll.AddPaymentOrder(poe);
                }

                if (entity.Yinlianka > 0) //银联卡
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.银联卡;
                    poe.PayName = "银联卡";
                    poe.PayMoney = entity.Price < 0 ? -entity.Yinlianka : entity.Yinlianka;
                    poe.BalanceMoney = 0;
                    poe.BalanceTiems = 0;
                    poe.ParentPayType = (int)ParentPayType.现金;
                    poe.CardID = 0;
                    cashBll.AddPaymentOrder(poe);
                }


                if (entity.Chuzhika != 0) //储值卡
                {
                    poe.OrderNo = entity.OrderNo;


                    poe.BalanceTiems = 0;
                    //扣除卡类金额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        string[] ordertimesstrlist =
                            txtChuzhikaarr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] IsGiveValueList =
                            IsGiveValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        string[] caridlist = cardidarr.Split(',');
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            if (IsGiveValueList[f] != "1")
                            {
                                allchuzhikaPay = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) +
                                                 allchuzhikaPay;
                            }
                            else
                            {
                                allzengchuzhikaPay = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) +
                                                     allzengchuzhikaPay;
                            }
                            poe.PayType = (int)PaymentEnum.储值卡;
                            var zhifuprice = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]);
                            var nowid = ConvertValueHelper.ConvertIntValue(caridlist[f]);
                            dic.Add(nowid, zhifuprice);
                            isgiveDic.Add(nowid, ConvertValueHelper.ConvertIntValue(IsGiveValueList[f]));
                            //获取这张支付卡的详细信息
                            var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = nowid });
                            var chuzhika = baseinfoBLL.GetPrePayCardModel(CardBalance.ProjectID);
                            if (CardBalance.ProjectID == 40112 || CardBalance.ProjectID == 42221)
                            {
                                poe.PayType = 8;
                            }
                            poe.PayName = chuzhika.Name;
                            poe.PayMoney = entity.Price < 0 ? -zhifuprice : zhifuprice;
                            poe.CardBalanceID = nowid;
                            poe.IsGive = ConvertValueHelper.ConvertIntValue(IsGiveValueList[f]);

                            poe.ids = nowid.ToString();
                            poe.CardID = CardBalance.CardID;
                            poe.BalanceMoney = CardBalance.Price - poe.PayMoney;
                            poe.ParentPayType = (int)ParentPayType.储值卡;
                            if (CardBalance.ProjectID == 40112 || CardBalance.ProjectID == 42221)
                            {
                                poe.ParentPayType = 8;
                            }
                            cashBll.AddPaymentOrder(poe);

                            //减去相应金额
                            result = cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity { ID = nowid, Price = poe.PayMoney });

                            //用户储值卡余额明细日志
                            var mainCardBalanceDetail = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity() { ID = nowid });
                            cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                            {
                                CardBalanceId = nowid,
                                OrderNo = poe.OrderNo,
                                Type = (int)MainCardBalanceDetailType.消费,
                                Amount = -poe.PayMoney,
                                Balance = mainCardBalanceDetail.Price,
                                OldAmount = mainCardBalanceDetail.Price + poe.PayMoney,
                                CreateTime = DateTime.Now
                            });
                            if (CardBalance.ProjectID == 40112 || CardBalance.ProjectID == 42221)//积分消费
                            {
                                var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                                decimal jffff = intrgral.Integral;
                                intrgral.AllIntegral = mainCardBalanceDetail.Price;
                                intrgral.Integral = mainCardBalanceDetail.Price;
                                int number = IpatBLL.UpdateIntegral(intrgral);
                                //IntegralOperationEntity entity1 = new IntegralOperationEntity();
                                //entity1.Memo = "消费";
                                //entity1.Name = "一";
                                //decimal PayMoney1 = 0;

                                //if (mainCardBalanceDetail.Price >= 1)
                                //{
                                //    PayMoney1 = jffff - mainCardBalanceDetail.Price;
                                //}
                                //else
                                //{
                                //    PayMoney1 = jffff;
                                //}
                                //entity1.Number = PayMoney1;
                                //entity1.UserID = entity.UserID;
                                //entity1.OperationType = 2;
                                //var id = IpatBLL.AddIntegralOperation(entity1);
                                //IntegralRecordEntity ir1 = new IntegralRecordEntity();
                                //ir1.UserID = entity.UserID;
                                //ir1.Integral = PayMoney1;
                                //ir1.CreateTime = DateTime.Now;
                                //ir1.OrderNO = "消费";
                                //ir1.DocumentNumber = "消费";
                                //ir1.IntegralOperationID = id;
                                //IpatBLL.AddIntegralRecord(ir1);//增加积分记录
                            }
                        }
                    }



                }

                if (!String.IsNullOrEmpty(CardIDstr)) //划扣次卡
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.划扣次数卡;

                    poe.BalanceMoney = 0;
                    //减去相应的卡类次数
                    if (!String.IsNullOrEmpty(CardIDstr))
                    {
                        string[] caridlist = CardIDstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] ordertimesstrlist =
                            ordertimesstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        //检测是否重复写入
                        List<string> jianche = new List<string>();
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            if (!jianche.Contains(caridlist[f]))
                            {
                                var CardBalanceID = ConvertValueHelper.ConvertIntValue(caridlist[f]);
                                jianche.Add(caridlist[f]);
                                int paycount = cashBll.GetPaymentOrderModelByOrder(new PaymentOrderEntity
                                {
                                    PayType = 4,
                                    CardBalanceID = CardBalanceID,
                                    OrderNo = entity.OrderNo,
                                    BalanceTiems = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f])
                                });
                                //如果存在划卡支付记录
                                if (paycount > 0)
                                {
                                    continue;
                                }

                                //修改用户余额记录,根据ID                      CardID是用户约记录的ID
                                cashBll.UpdateMainCardBalanceTime(new MainCardBalanceEntity
                                {
                                    CardID = CardBalanceID,
                                    Tiems = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f])
                                });
                                // LogWriter.WriteInfo("当前账户结余ID：" + caridlist[f] + "总划扣账户ID:" + CardIDstr, "订单支付方式，单据号：" + entity.OrderNo);

                                //获取这张支付卡的详细信息
                                var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = CardBalanceID });
                                var xiangmu = baseinfoBLL.GetProjectModel(CardBalance.ProjectID);
                                //如果是划次,直接用购买卡的时候的价格来算.
                                //修改代码:L0428002
                                //修复记录:在订单结算时的划卡的金额.现在用的是买卡时的金额.
                                //修复代码:以下7行
                                //------------------------160625修改
                                //现在购买金额用实耗单价算.
                                //因为疗程卡划扣按照实耗算,不是按照购买单价算了.
                                //消耗单价

                                if (CardBalance.Type == 2)
                                {
                                    poe.PayMoney = CardBalance.ConsumePrice *
                                                   ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]);
                                }
                                else
                                {
                                    poe.PayMoney = CardBalance.ConsumePrice == 0
                                        ? xiangmu.CostPrice * ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f])
                                        : CardBalance.ConsumePrice *
                                          ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]);
                                }

                                poe.PayName = xiangmu.Name;
                                poe.CardBalanceID = ConvertValueHelper.ConvertIntValue(caridlist[f]);
                                poe.CardID = CardBalance.CardID;
                                poe.PayTime = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]);
                                poe.BalanceTiems = CardBalance.Tiems;
                                poe.ids = caridlist[f].ToString();
                                poe.ParentPayType = (int)ParentPayType.划卡;
                                cashBll.AddPaymentOrder(poe);
                            }
                            else
                            {
                                LogWriter.WriteInfo("该单据总项目数：" + caridlist.Length + ",同一账户余额只扣除一次：" + caridlist[f],
                                    "重复支付，单据号：" + entity.OrderNo);
                                //跳出循环
                                continue;
                            }

                        }
                    }
                }


            }

            #endregion

            //如果存在欠款，修改用户欠款情况
            if (entity.QianPrice != 0)
            {
                cashBll.UpdateArrearsMoney(entity.UserID, entity.QianPrice);
            }

            var patientEntity = IpatBLL.GetPatienttEntityByID(entity.UserID);
            //订单产品支付方式表
            //现金类：现金支付部分优先产品，产品价格由大到小进行支付结算
            //卡扣类：优先产品
            //根据订单编号查询订单产品表
            var orderInfoList = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = entity.OrderNo });
            //剩余现金部分
            decimal ShengyuCash = entity.Xianjin;
            //已支付现金金额
            decimal PayCash = 0;
            //剩余银联卡部分
            decimal YinLianCard = entity.Yinlianka;
            //已支付银联卡部分
            decimal PayCarded = 0;
            //剩余储值卡
            decimal Chuzhika = entity.Chuzhika;
            //已支付储值卡部分
            decimal ChuzhikaPayed = 0;
            decimal otherpayprice = 0; //其他已经付款的
            //储值卡集合
            List<int> cardlist = new List<int>();
            cardlist.AddRange(dic.Keys);
            //其他支付方式
            List<int> otherpaylist = new List<int>();
            otherpaylist.AddRange(otherpay.Keys);

            //订单详情结算方式记录
            //检测重复支付记录问题
            List<int> jianchepay = new List<int>();
            int IsProject = 0;
            //判断是否包含产品和项目
            List<int> prolist = new List<int>();
            //跟进人id
            int followuserid = 0;

            int row, pagecount;

            #region 跟进人

            var joins = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = orderInfoList.Select(x => x.ID).FirstOrDefault() }, out row, out pagecount);

            //多对一：判断是否存在指定美容师
            if (joins.Count > 1 && joins.Any(x => x.UserID == patientEntity.FollowUpMrUserID))
            {
                followuserid = patientEntity.FollowUpMrUserID;
            }
            //否则取第一个
            else
            {
                followuserid = joins.Select(x => x.UserID).FirstOrDefault();
            }
            //2019-11-23
            var HospitalUserModel = personnelBLL.GetUserByUserID(followuserid);
            int guid = cashBll.UpdaetOrderInfoGetByID(entity.OrderNo, HospitalUserModel.HospitalID);
            int guid1 = cashBll.UpdaetOrderGetByID(entity.OrderNo, HospitalUserModel.HospitalID);
            #endregion


            foreach (var info in orderInfoList)
            {
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                //新的产品或者项目
                PayCash = 0;
                PayCarded = 0;
                ChuzhikaPayed = 0;
                otherpayprice = 0;
                PaymentOrderProductEntity conditon = new PaymentOrderProductEntity();
                conditon.AllPrice = info.AllPrice; //订单产品总价
                conditon.OrderInfoID = info.ID; //支付订单产品

                if (info.Type == 2) //产品
                {
                    prolist.Add(2);

                    #region==产品支付==

                    #region==现金支付==

                    if (ShengyuCash > 0) //现金
                    {
                        conditon.PayType = (int)PaymentEnum.现金;
                        if (ShengyuCash >= Math.Abs(info.AllPrice)) //如果现金部分超出实际产品价格
                        {
                            conditon.PayMoney = info.AllPrice;
                            ShengyuCash = ShengyuCash - Math.Abs(conditon.PayMoney);
                            PayCash = Math.Abs(conditon.PayMoney);
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue; //跳出循环
                        }
                        else
                        {
                            //现金部分不够
                            conditon.PayMoney = info.AllPrice < 0 ? -ShengyuCash : ShengyuCash;
                            PayCash = ShengyuCash;
                            ShengyuCash = 0;
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                        }
                    } //现金支付

                    #endregion

                    #region==银联卡支付==

                    if (YinLianCard > 0) //银联卡
                    {
                        conditon.PayType = (int)PaymentEnum.银联卡;
                        if (YinLianCard >= Math.Abs(info.AllPrice)) //如果银联卡部分超出实际产品价格
                        {
                            if (PayCash > 0) //支付了部分现金
                            {
                                conditon.PayMoney = info.AllPrice < 0
                                    ? -(Math.Abs(info.AllPrice) - PayCash)
                                    : (Math.Abs(info.AllPrice) - PayCash);
                                //YinLianCard = YinLianCard - conditon.PayMoney;
                                YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                            else
                            {
                                conditon.PayMoney = info.AllPrice;
                                YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                        }
                        else
                        {
                            //银联卡部分不够
                            if (PayCash > 0) //支付了部分现金
                            {
                                if (PayCash + YinLianCard >= Math.Abs(info.AllPrice))
                                {
                                    //conditon.PayMoney = Math.Abs(info.AllPrice) - PayCash;
                                    conditon.PayMoney = info.AllPrice < 0
                                        ? -(Math.Abs(info.AllPrice) - PayCash)
                                        : (Math.Abs(info.AllPrice) - PayCash);
                                    YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                    conditon.ParentPayType = (int)ParentPayType.现金;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue; //跳出循环
                                }
                                else
                                {
                                    // conditon.PayMoney = YinLianCard + PayCash;
                                    //conditon.PayMoney = YinLianCard;
                                    conditon.PayMoney = info.AllPrice < 0 ? -YinLianCard : YinLianCard;
                                    YinLianCard = 0;
                                    PayCarded = Math.Abs(conditon.PayMoney);
                                    conditon.ParentPayType = (int)ParentPayType.现金;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                }

                            }
                            else
                            {
                                // conditon.PayMoney = YinLianCard;
                                conditon.PayMoney = info.AllPrice < 0 ? -YinLianCard : YinLianCard;
                                YinLianCard = 0;
                                PayCarded = Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);

                            }
                        }

                    } //银联卡支付

                    #endregion

                    #region==储值卡支付==

                    //储值卡支付  Chuzhika=储值卡总额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        foreach (var valu in cardlist)
                        {
                            conditon.IsGive = isgiveDic[valu];
                            var manCardid = valu; //用户余额记录ID
                            var manPrice = dic[valu]; //支付金额
                            if (manPrice > 0)
                            {
                                conditon.PayType = (int)PaymentEnum.储值卡;
                                if (manPrice >= Math.Abs(info.AllPrice)) //如果储值卡部分超出实际产品价格
                                {
                                    if (ChuzhikaPayed + PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {

                                        conditon.PayMoney = info.AllPrice < 0
                                            ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                            : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);

                                        dic.Remove(manCardid);

                                        dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额

                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //终止循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        conditon.CardID = manCardid;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                }
                                else
                                {
                                    //储值卡部分不够
                                    if (ChuzhikaPayed + PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {
                                        if (ChuzhikaPayed + PayCash + PayCarded + manPrice > Math.Abs(info.AllPrice))
                                        {
                                            conditon.PayMoney = info.AllPrice < 0
                                                ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                                : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
                                            // Chuzhika = Chuzhika - conditon.PayMoney;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            conditon.ParentPayType = (int)ParentPayType.储值卡;
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            break; //跳出循环
                                        }
                                        else
                                        {
                                            conditon.PayMoney = info.AllPrice < 0 ? -manPrice : manPrice;
                                            //  dic.Remove(manCardid);
                                            ChuzhikaPayed = conditon.PayMoney;
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, (decimal)0.0);
                                            conditon.ParentPayType = (int)ParentPayType.储值卡;
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice < 0 ? -manPrice : manPrice;
                                        //Chuzhika = 0;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, (decimal)0.0);
                                        ChuzhikaPayed = conditon.PayMoney;
                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;
                                    }
                                }
                            }
                            else //如果manprice是负数,给当前储值卡加上这笔钱
                            {
                                if (manPrice == 0)
                                {
                                    continue;
                                }

                                conditon.PayType = (int)PaymentEnum.储值卡;
                                conditon.PayMoney = info.AllPrice < 0
                                    ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded)
                                    : (Math.Abs(info.AllPrice) - PayCash - PayCarded);
                                dic.Remove(manCardid);
                                dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额

                                //添加产品支付方式
                                conditon.CardID = manCardid;
                                conditon.ParentPayType = (int)ParentPayType.储值卡;
                                cashBll.AddPaymentOrderProduct(conditon);
                                break; //终止循环

                            }
                        }
                    }

                    #endregion

                    #region==其他支付===

                    //其他支付方式
                    if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                    {
                        foreach (var PayType in otherpaylist)
                        {
                            var manCardid = PayType; //用户余额记录ID
                            var nowprice = otherpay[PayType]; //支付金额
                            if (otherpaytype[PayType] == 1)
                            {
                                conditon.ParentPayType = (int)ParentPayType.现金;
                            }
                            else
                            {
                                conditon.ParentPayType = (int)ParentPayType.券类;
                            }

                            if (nowprice > 0)
                            {
                                conditon.PayType = PayType;
                                if (nowprice > Math.Abs(info.AllPrice)) //如果大于实际产品价格
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联和储值卡
                                    {
                                        conditon.PayMoney = info.AllPrice < 0
                                            ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                            : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);

                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = Math.Abs(info.AllPrice) - Math.Abs(otherpayprice);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);

                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                }
                                else
                                {
                                    //银联卡部分不够
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联
                                    {
                                        if (PayCash + PayCarded + ChuzhikaPayed + nowprice > Math.Abs(info.AllPrice))
                                        {
                                            conditon.PayMoney = info.AllPrice < 0
                                                ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed -
                                                    otherpayprice)
                                                : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed -
                                                   otherpayprice);
                                            nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                            otherpay.Remove(PayType);
                                            otherpay.Add(PayType, nowprice);

                                            cashBll.AddPaymentOrderProduct(conditon);
                                            break; //跳出循环
                                        }
                                        else
                                        {
                                            otherpayprice = conditon.PayMoney =
                                                info.AllPrice < 0 ? -nowprice : nowprice;
                                            nowprice = 0;
                                            nowprice = Math.Abs(conditon.PayMoney);
                                            otherpay.Remove(PayType);
                                            otherpay.Add(PayType, 0);

                                            cashBll.AddPaymentOrderProduct(conditon);
                                        }

                                    }
                                    else
                                    {
                                        //conditon.PayMoney = nowprice;
                                        otherpayprice = conditon.PayMoney = info.AllPrice < 0 ? -nowprice : nowprice;
                                        nowprice = 0;
                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);
                                        otherpayprice = nowprice = Math.Abs(conditon.PayMoney);

                                        cashBll.AddPaymentOrderProduct(conditon);

                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #endregion
                }
                else
                {
                    //支付项目
                    prolist.Add(1);
                    //获取项目和产品关系，写入院用消耗，虚拟仓库
                    IList<ProjectProductRelationEntity> relationList =
                        baseinfoBLL.GetProjectProductRelation(new ProjectProductRelationEntity { ProjectID = info.ProdcuitID });
                    foreach (var mod in relationList)
                    {
                        iwareBLL.AddProductUseUnit(new ProductUseUnitEntity
                        {
                            HospitalID = LoginUserEntity.HospitalID,
                            ProductID = mod.ProductID,
                            ProjectID = info.ProdcuitID,
                            UseUnit = mod.DanCiUseUnit * info.Quantity,
                            Quatity = info.Quantity
                        });
                        //mod.ProductID
                        // iwareBLL.UpdateProductXuniStock(new ProductXuniStockEntity { HospitalID=loginUserEntity.HospitalID, ProductID = mod.ProductID, UseUnit = mod.DanCiUseUnit * info.Quantity });
                    }

                    #region==项目支付==

                    IsProject = IsProject + 1;

                    #region==划扣次卡==

                    if (info.CardID != 0) //划扣次卡
                    {
                        if (!jianchepay.Contains(info.CardID))
                        {
                            jianchepay.Add(info.CardID);

                            //获取这张支付卡的详细信息
                            var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = info.CardID });
                            conditon.PayType = (int)PaymentEnum.划扣次数卡;
                            conditon.PayMoney = info.AllPrice;
                            conditon.CardID = info.CardID;
                            conditon.ConsumePrice = CardBalance.ConsumePrice; //吧余额下面的员工业绩赋值给详情支付方式表
                            //获取划卡项目支付方式
                            var payordercount = cashBll.GetPaymentOrderModelByOrderInfo(new PaymentOrderProductEntity
                            {
                                CardID = info.CardID,
                                PayType = (int)PaymentEnum.划扣次数卡,
                                OrderInfoID = conditon.OrderInfoID
                            });
                            if (payordercount > 0)
                            {
                                continue;
                            }

                            ;
                            //添加产品支付方式
                            conditon.ParentPayType = (int)ParentPayType.划卡;
                            cashBll.AddPaymentOrderProduct(conditon);
                            LogWriter.WriteInfo("当前账户余额ID：" + info.CardID, "产品支付方式，单据号：" + entity.OrderNo);

                            #region ===划扣次数计算消耗===

                            //获取订单参与人数数量
                            foreach (var cond in joinList)
                            {
                                if (cond.XiaoHao == (decimal)0.00)
                                {
                                    cond.XiaoHao = CardBalance.ConsumePrice * info.Quantity; //这里原来是info.allprice.应为
                                }

                                //修改消耗
                                cashBll.UpdateJoinUserForPay(cond);
                            }

                            #endregion

                        }
                        else
                        {
                            LogWriter.WriteInfo("卡号：" + info.CardID, "重复支付，单据号：" + entity.OrderNo);
                            //跳出循环
                            continue;
                        }

                        continue;
                    } //划扣次卡

                    #endregion

                    #region==现金支付==

                    if (ShengyuCash > 0) //现金
                    {
                        conditon.PayType = (int)PaymentEnum.现金;
                        if (ShengyuCash >= Math.Abs(info.AllPrice)) //如果现金部分超出实际产品价格
                        {
                            conditon.PayMoney = info.AllPrice;
                            ShengyuCash = ShengyuCash - Math.Abs(conditon.PayMoney);
                            PayCash = Math.Abs(conditon.PayMoney);
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue; //跳出循环
                        }
                        else
                        {
                            //现金部分不够
                            conditon.PayMoney = info.AllPrice < 0 ? -ShengyuCash : ShengyuCash;
                            PayCash = ShengyuCash;
                            ShengyuCash = 0;
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                        }
                    } //现金支付

                    #endregion

                    #region==银联卡支付==

                    if (YinLianCard > 0) //银联卡
                    {
                        conditon.PayType = (int)PaymentEnum.银联卡;
                        if (YinLianCard >= Math.Abs(info.AllPrice)) //如果银联卡部分超出实际产品价格
                        {
                            if (PayCash > 0) //支付了部分现金
                            {
                                conditon.PayMoney = info.AllPrice < 0
                                    ? -(Math.Abs(info.AllPrice) - PayCash)
                                    : (Math.Abs(info.AllPrice) - PayCash);
                                //YinLianCard = YinLianCard - conditon.PayMoney;
                                YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                            else
                            {
                                conditon.PayMoney = info.AllPrice;
                                YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                        }
                        else
                        {
                            //银联卡部分不够
                            if (PayCash > 0) //支付了部分现金
                            {
                                if (PayCash + YinLianCard >= Math.Abs(info.AllPrice))
                                {
                                    //conditon.PayMoney = Math.Abs(info.AllPrice) - PayCash;
                                    conditon.PayMoney = info.AllPrice < 0
                                        ? -(Math.Abs(info.AllPrice) - PayCash)
                                        : (Math.Abs(info.AllPrice) - PayCash);
                                    YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                    conditon.ParentPayType = (int)ParentPayType.现金;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue; //跳出循环
                                }
                                else
                                {
                                    // conditon.PayMoney = YinLianCard + PayCash;
                                    //conditon.PayMoney = YinLianCard;
                                    conditon.PayMoney = info.AllPrice < 0 ? -YinLianCard : YinLianCard;
                                    YinLianCard = 0;
                                    PayCarded = Math.Abs(conditon.PayMoney);
                                    conditon.ParentPayType = (int)ParentPayType.现金;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                }

                            }
                            else
                            {
                                // conditon.PayMoney = YinLianCard;
                                conditon.PayMoney = info.AllPrice < 0 ? -YinLianCard : YinLianCard;
                                YinLianCard = 0;
                                PayCarded = Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);

                            }
                        }

                    } //银联卡支付

                    #endregion

                    #region==储值卡支付==

                    //储值卡支付  Chuzhika=储值卡总额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        #region ===划扣次数计算消耗===

                        //获取订单参与人数数量

                        foreach (var cond in joinList)
                        {
                            if (cond.XiaoHao == (decimal)0.00)
                            {
                                cond.XiaoHao = info.AllPrice;
                            }

                            //修改消耗
                            cashBll.UpdateJoinUserForPay(cond);
                        }

                        #endregion

                        foreach (var valu in cardlist)
                        {
                            conditon.IsGive = isgiveDic[valu];
                            var manCardid = valu; //用户余额记录ID
                            var manPrice = dic[valu]; //支付金额
                            if (manPrice > 0)
                            {
                                conditon.PayType = (int)PaymentEnum.储值卡;
                                if (manPrice >= Math.Abs(info.AllPrice)) //如果储值卡部分超出实际产品价格
                                {
                                    if (ChuzhikaPayed + PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {

                                        conditon.PayMoney = info.AllPrice < 0
                                            ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                            : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);

                                        dic.Remove(manCardid);

                                        dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额

                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //终止循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        conditon.CardID = manCardid;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                }
                                else
                                {
                                    //储值卡部分不够
                                    if (ChuzhikaPayed + PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {
                                        if (ChuzhikaPayed + PayCash + PayCarded + manPrice > Math.Abs(info.AllPrice))
                                        {
                                            conditon.PayMoney = info.AllPrice < 0
                                                ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                                : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
                                            // Chuzhika = Chuzhika - conditon.PayMoney;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            conditon.ParentPayType = (int)ParentPayType.储值卡;
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            break; //跳出循环
                                        }
                                        else
                                        {
                                            conditon.PayMoney = info.AllPrice < 0 ? -manPrice : manPrice;
                                            //  dic.Remove(manCardid);
                                            ChuzhikaPayed = conditon.PayMoney;
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, (decimal)0.0);
                                            conditon.ParentPayType = (int)ParentPayType.储值卡;
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice < 0 ? -manPrice : manPrice;
                                        //Chuzhika = 0;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, (decimal)0.0);
                                        ChuzhikaPayed = conditon.PayMoney;
                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;
                                    }
                                }
                            }
                            else //如果manprice是负数,给当前储值卡加上这笔钱
                            {
                                if (manPrice == 0)
                                {
                                    continue;
                                }

                                conditon.PayType = (int)PaymentEnum.储值卡;
                                conditon.PayMoney = info.AllPrice < 0
                                    ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded)
                                    : (Math.Abs(info.AllPrice) - PayCash - PayCarded);
                                dic.Remove(manCardid);
                                dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额

                                //添加产品支付方式
                                conditon.CardID = manCardid;
                                conditon.ParentPayType = (int)ParentPayType.储值卡;
                                cashBll.AddPaymentOrderProduct(conditon);
                                break; //终止循环

                            }
                        }
                    }

                    #endregion

                    #region==其他支付===

                    //其他支付方式
                    if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                    {
                        foreach (var PayType in otherpaylist)
                        {
                            var manCardid = PayType; //用户余额记录ID
                            var nowprice = otherpay[PayType]; //支付金额
                            if (otherpaytype[PayType] == 1)
                            {
                                conditon.ParentPayType = (int)ParentPayType.现金;
                            }
                            else
                            {
                                conditon.ParentPayType = (int)ParentPayType.券类;
                            }

                            if (nowprice > 0)
                            {
                                conditon.PayType = PayType;
                                if (nowprice > Math.Abs(info.AllPrice)) //如果大于实际产品价格
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联和储值卡
                                    {
                                        conditon.PayMoney = info.AllPrice < 0
                                            ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                            : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);

                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = Math.Abs(info.AllPrice) - Math.Abs(otherpayprice);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);

                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                }
                                else
                                {
                                    //银联卡部分不够
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联
                                    {
                                        if (PayCash + PayCarded + ChuzhikaPayed + nowprice > Math.Abs(info.AllPrice))
                                        {
                                            conditon.PayMoney = info.AllPrice < 0
                                                ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed -
                                                    otherpayprice)
                                                : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed -
                                                   otherpayprice);
                                            nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                            otherpay.Remove(PayType);
                                            otherpay.Add(PayType, nowprice);

                                            cashBll.AddPaymentOrderProduct(conditon);
                                            break; //跳出循环
                                        }
                                        else
                                        {
                                            otherpayprice = conditon.PayMoney =
                                                info.AllPrice < 0 ? -nowprice : nowprice;
                                            nowprice = 0;
                                            nowprice = Math.Abs(conditon.PayMoney);
                                            otherpay.Remove(PayType);
                                            otherpay.Add(PayType, 0);

                                            cashBll.AddPaymentOrderProduct(conditon);
                                        }

                                    }
                                    else
                                    {
                                        //conditon.PayMoney = nowprice;
                                        otherpayprice = conditon.PayMoney = info.AllPrice < 0 ? -nowprice : nowprice;
                                        nowprice = 0;
                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);
                                        otherpayprice = nowprice = Math.Abs(conditon.PayMoney);

                                        cashBll.AddPaymentOrderProduct(conditon);

                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #endregion
                }
            }

            string projectname = "";
            string productname = "";
            string host = Request.Url.Host;
            if (NewKeVersion == 2) //美益通和其他算法不一样
            {
                #region==计算员工业绩==

                //重新计算员工业绩
                foreach (var info in orderInfoList)
                {
                    //int row, pagecount;
                    var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row,
                        out pagecount);
                    foreach (var cond in joinList)
                    {
                        //获取第一个跟进人员
                        //if (followuserid == 0)
                        //{
                        //    followuserid = cond.UserID;
                        //}

                        //卡扣业绩
                        var payproduct = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 3 });
                        decimal paymoney = 0;
                        foreach (var m in payproduct)
                        {
                            paymoney += m.PayMoney * m.HuaKouZheLv;

                        }

                        //现金业绩
                        var payproduct1 = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 1 });
                        decimal paymoney1 = 0;
                        foreach (var m in payproduct1)
                        {
                            paymoney1 += m.PayMoney * 1;
                        }


                        //消耗业绩(疗程卡卡扣次数)
                        var payproduct2 = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 4 });
                        decimal paymoney2 = 0;
                        foreach (var m in payproduct2)
                        {
                            if (m.ConsumePrice != 0)
                            {
                                paymoney2 += m.ConsumePrice * info.Quantity;
                            }
                            else
                            {
                                paymoney2 += m.PayMoney * 1;
                            }
                        }

                        //修改业绩
                        cond.KakouYeji =
                            ConvertValueHelper.ConvertDecimalValue((paymoney / (joinList.Count)).ToString("f2")); //卡扣业绩
                        cond.Yeji = ConvertValueHelper.ConvertDecimalValue(
                            (paymoney1 / (joinList.Count)).ToString("f2")); //现金业绩

                        //decimal Yuangongyeji_KakouYeji = cond.KakouYeji;
                        //decimal Yuangongyeji_Yeji = cond.Yeji;
                        //如果数量为负数,则吧业绩算出负数
                        //if (info.Quantity < 0)
                        //{
                        //    cond.KakouYeji = -Yuangongyeji_KakouYeji;
                        //    cond.Yeji = -Yuangongyeji_Yeji;
                        //}

                        if (info.Type == 1) //如果订单详情是项目
                        {
                            var productentity = baseinfoBLL.GetProjectModel(info.ProdcuitID);
                            var tichenglist = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = cond.ProjectID });

                            if (!projectname.Contains(productentity.Name))
                            {
                                projectname += productentity.Name;
                            }

                            //判断产品是否是属于大项目或者特殊项目
                            if (productentity.BaseType == (int)ProductBaseType.特殊项目)
                            {
                                cond.KakouYeji_Teshuxiangmu = cond.KakouYeji;
                                cond.Yeji_Teshuxiangmu = cond.Yeji;
                            }
                            else if (productentity.BaseType == (int)ProductBaseType.大项目)
                            {
                                cond.KakouYeji_Daxiangmu = cond.KakouYeji;
                                cond.Yeji_Daxiangmu = cond.Yeji;
                            }
                            else
                            {
                                cond.KakouYeji_Xiangmu = cond.KakouYeji;
                                cond.Yeji_Xiangmu = cond.Yeji;
                            }

                            //cond.ProjectNumber = info.Quantity;
                            //1、单次项目是大项目只计销售业绩（现金和卡扣）不计消耗，是基础项目和特殊项目计销售也计消耗。
                            //当单次消耗价格≤提成方案里设定的上限数值时，计销售不计消耗计手工或奖励。只能设置一个，如果设置多个提成方案 取第一个，如果记录了消耗就不能记录手工和奖励
                            //3 一个项目只有一个提成方案、提成方案上线=下线 既有实操，又有手工，如果设置了额外奖励，就需要记录额外奖励
                       
                            decimal xiaohaoyeji =
                                ConvertValueHelper.ConvertDecimalValue(
                                    (paymoney2 / (joinList.Count)).ToString("f2")); //消耗业绩

                            //如果设置了提成方案，并且消耗业绩大于提成方案结束金额
                            if (tichenglist.Count() > 0)
                            {
                                var tcinfo = tichenglist[0];
                                if (tcinfo.SpecifiedPrice > 0)
                                {
                                    cond.OtherTiCheng = tcinfo.SpecifiedPrice; //额外奖励，只要设置了就有
                                }

                                if (info.AllPrice / info.Quantity == 0) //金额为0，只有手工没有消耗
                                {
                                    cond.XiaoHao_Liaocheng = cond.XiaoHao = 0;
                                    if (tcinfo.NonSpecifiedType == 1)
                                    {
                                        cond.Ticheng = ((info.AllPrice) * tcinfo.NonSpecifiedPrice) / joinList.Count;
                                    }
                                    else
                                    {
                                        cond.Ticheng = (tcinfo.NonSpecifiedPrice * info.Quantity) / joinList.Count;
                                    }
                                }
                                else if (info.AllPrice / info.Quantity >= tcinfo.StartPrice &&
                                         (info.AllPrice / info.Quantity) <= tcinfo.EndPrice)
                                {
                                    cond.XiaoHao_Liaocheng = cond.XiaoHao = 0;
                                    if (tcinfo.NonSpecifiedType == 1)
                                    {
                                        cond.Ticheng = ((info.AllPrice) * tcinfo.NonSpecifiedPrice) / joinList.Count;
                                    }
                                    else
                                    {
                                        cond.Ticheng = (tcinfo.NonSpecifiedPrice * info.Quantity) / joinList.Count;
                                    }

                                }
                                else if (tcinfo.StartPrice == tcinfo.EndPrice)
                                {
                                    if (xiaohaoyeji > 0) //划卡
                                    {
                                        cond.XiaoHao_Liaocheng = cond.XiaoHao = xiaohaoyeji;
                                    }
                                    else
                                    {
                                        //单项
                                        cond.XiaoHao_Liaocheng = cond.XiaoHao =
                                            cond.KakouYeji > 0 ? cond.KakouYeji : cond.Yeji;
                                    }

                                    if (tcinfo.NonSpecifiedType == 1)
                                    {
                                        cond.Ticheng = ((info.AllPrice) * tcinfo.NonSpecifiedPrice) / joinList.Count;
                                    }
                                    else
                                    {
                                        cond.Ticheng = (tcinfo.NonSpecifiedPrice * info.Quantity) / joinList.Count;
                                    }
                                }
                                else if (info.AllPrice / info.Quantity > tcinfo.EndPrice)
                                {
                                    if (productentity.BaseType != (int)ProductBaseType.大项目)
                                    {
                                        if (xiaohaoyeji > 0) //划卡
                                        {
                                            cond.XiaoHao_Liaocheng = cond.XiaoHao = xiaohaoyeji;
                                        }
                                        else
                                        {
                                            //单项
                                            cond.XiaoHao_Liaocheng = cond.XiaoHao =
                                                cond.KakouYeji > 0 ? cond.KakouYeji : cond.Yeji;
                                        }
                                    }
                                }


                            }
                            else
                            {
                                //没有设置提成方案
                                if (productentity.BaseType != (int)ProductBaseType.大项目)
                                {
                                    if (xiaohaoyeji > 0) //划卡
                                    {
                                        cond.XiaoHao_Liaocheng = cond.XiaoHao = xiaohaoyeji;
                                    }
                                    else
                                    {
                                        //单项
                                        cond.XiaoHao_Liaocheng = cond.XiaoHao;
                                        cond.XiaoHao = cond.KakouYeji > 0 ? cond.KakouYeji : cond.Yeji;
                                    }
                                }
                            }

                            if (productentity.BaseType == (int)ProductBaseType.大项目)
                            {
                                cond.XiaoHao_Liaocheng = cond.XiaoHao = 0;
                            }
                        }
                        else if (info.Type == 2) //产品
                        {
                            var productentity = baseinfoBLL.GetModel(info.ProdcuitID);
                            if (!productname.Contains(productentity.Name))
                            {
                                productname += productentity.Name;
                            }

                            //判断产品是否是属于大项目或者特殊项目
                            if (productentity.BaseType == (int)ProductBaseType.基础项目)
                            {
                                cond.KakouYeji_Chanpin = cond.KakouYeji;
                                cond.Yeji_Chanpin = cond.Yeji;
                            }
                            else if (productentity.BaseType == (int)ProductBaseType.特殊项目)
                            {
                                cond.KakouYeji_TeshuChanpin = cond.KakouYeji;
                                cond.Yeji_TeshuChanpin = cond.Yeji;
                            }
                            //else {
                            //    cond.KakouYeji_Chanpin = cond.KakouYeji;
                            //    cond.Yeji_Chanpin = cond.Yeji;
                            //}


                        }
                        else if (info.Type == 3)
                        {
                            cond.KakouYeji_Liaocheng = cond.KakouYeji;
                            var model = baseinfoBLL.GetPrePayCardModel(info.ProdcuitID);
                            if (model.Type == 1)
                            {
                                cond.Yeji_Chuzhika = cond.Yeji;
                            }

                            if (model.Type == 2)
                            {
                                cond.Yeji_Liaochengka = cond.Yeji;
                            }
                        }
                        //if (cond.XiaoHao > 0)//消耗不为0时,子项是疗程卡消耗
                        //{
                        //    cond.XiaoHao_Liaocheng = cond.XiaoHao;
                        //}


                        //if (cond.IsZhiding == 1)
                        //{
                        //    //实操=疗程卡扣+单项卡扣+单项业绩
                        //    cond.ShiCao_zhiding = cond.KakouYeji_Liaocheng + cond.KakouYeji_Xiangmu + cond.Yeji_Xiangmu;
                        //    cond.ShiCao = cond.ShiCao_zhiding;
                        //    cond.KeShu = 1;
                        //    cond.KeShu_zhiding = 1;
                        //}
                        //else
                        //{
                        //    //实操=疗程卡扣+单项卡扣+单项业绩
                        //    cond.ShiCao_feizhiding = cond.KakouYeji_Liaocheng + cond.KakouYeji_Xiangmu + cond.Yeji_Xiangmu;
                        //    cond.ShiCao = cond.ShiCao_feizhiding;
                        //    cond.KeShu = 1;
                        //    cond.KeShu_feizhiding = 1;
                        //}
                        cashBll.UpdateJoinUserYejiForPay(cond);
                    }


                }

                #endregion
            }
            else
            {
                #region==计算员工业绩==

                //重新计算员工业绩
                foreach (var info in orderInfoList)
                {
                    //int row, pagecount;
                    var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row,
                        out pagecount);
                    foreach (var cond in joinList)
                    {
                        //卡扣业绩
                        var payproduct = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 3 });
                        decimal paymoney = 0;
                        foreach (var m in payproduct)
                        {
                            paymoney += m.PayMoney * m.HuaKouZheLv;

                        }

                        //现金业绩
                        var payproduct1 = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 1 });
                        decimal paymoney1 = 0;
                        foreach (var m in payproduct1)
                        {
                            paymoney1 += m.PayMoney * 1;
                        }


                        //消耗业绩(疗程卡卡扣次数)
                        var payproduct2 = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 4 });
                        decimal paymoney2 = 0;
                        foreach (var m in payproduct2)
                        {
                            if (m.ConsumePrice != 0)
                            {
                                paymoney2 += m.ConsumePrice * info.Quantity;
                            }
                            else
                            {
                                paymoney2 += m.PayMoney * 1;
                            }
                        }

                        //修改业绩



                        cond.KakouYeji =
                            ConvertValueHelper.ConvertDecimalValue((paymoney / (joinList.Count)).ToString("f2")); //卡扣业绩
                        cond.Yeji = ConvertValueHelper.ConvertDecimalValue(
                            (paymoney1 / (joinList.Count)).ToString("f2")); //现金业绩
                        cond.XiaoHao =
                            ConvertValueHelper.ConvertDecimalValue(
                                (paymoney2 / (joinList.Count)).ToString("f2")); //消耗业绩
                        //decimal Yuangongyeji_KakouYeji = cond.KakouYeji;
                        //decimal Yuangongyeji_Yeji = cond.Yeji;
                        //如果数量为负数,则吧业绩算出负数
                        //if (info.Quantity < 0)
                        //{
                        //    cond.KakouYeji = -Yuangongyeji_KakouYeji;
                        //    cond.Yeji = -Yuangongyeji_Yeji;
                        //}

                        if (info.Type == 1) //如果订单详情是项目
                        {
                            //cond.ProjectNumber = info.Quantity;
                            cond.KakouYeji_Xiangmu = cond.KakouYeji;
                            cond.Yeji_Xiangmu = cond.Yeji;
                            var tichenglist = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = cond.ProjectID });

                            foreach (var tcinfo in tichenglist)
                            {
                                if (info.Price >= tcinfo.StartPrice &&
                                    (info.AllPrice / info.Quantity) <= tcinfo.EndPrice)
                                {
                                    if (cond.Ticheng == 0)
                                    {
                                        if (tcinfo.NonSpecifiedType == 1)
                                        {
                                            cond.Ticheng =
                                                ((info.AllPrice / info.Quantity) * tcinfo.NonSpecifiedPrice) /
                                                joinList.Count;
                                        }
                                        else
                                        {
                                            cond.Ticheng = (tcinfo.NonSpecifiedPrice * info.Quantity) / joinList.Count;
                                        }
                                    }

                                    if (tcinfo.SpecifiedType == 1 && cond.IsZhiding == 1)
                                    {
                                        cond.OtherTiCheng =
                                            cond.OtherTiCheng +
                                            (tcinfo.SpecifiedPrice * (info.AllPrice / info.Quantity) / joinList.Count);
                                    }
                                    else if (cond.IsZhiding == 1)
                                    {
                                        cond.OtherTiCheng = tcinfo.SpecifiedPrice / joinList.Count;
                                    }

                                }
                            }
                        }
                        else if (info.Type == 2) //产品
                        {
                            cond.KakouYeji_Chanpin = cond.KakouYeji;
                            cond.Yeji_Chanpin = cond.Yeji;

                        }
                        else if (info.Type == 3)
                        {
                            cond.KakouYeji_Liaocheng = cond.KakouYeji;
                            var model = baseinfoBLL.GetPrePayCardModel(info.ProdcuitID);
                            if (model.Type == 1)
                            {
                                cond.Yeji_Chuzhika = cond.Yeji;
                            }

                            if (model.Type == 2)
                            {
                                cond.Yeji_Liaochengka = cond.Yeji;
                            }
                        }

                        if (cond.XiaoHao > 0) //消耗不为0时,子项是疗程卡消耗
                        {
                            cond.XiaoHao_Liaocheng = cond.XiaoHao;
                        }


                        //if (cond.IsZhiding == 1)
                        //{
                        //    //实操=疗程卡扣+单项卡扣+单项业绩
                        //    cond.ShiCao_zhiding = cond.KakouYeji_Liaocheng + cond.KakouYeji_Xiangmu + cond.Yeji_Xiangmu;
                        //    cond.ShiCao = cond.ShiCao_zhiding;
                        //    cond.KeShu = 1;
                        //    cond.KeShu_zhiding = 1;
                        //}
                        //else
                        //{
                        //    //实操=疗程卡扣+单项卡扣+单项业绩
                        //    cond.ShiCao_feizhiding = cond.KakouYeji_Liaocheng + cond.KakouYeji_Xiangmu + cond.Yeji_Xiangmu;
                        //    cond.ShiCao = cond.ShiCao_feizhiding;
                        //    cond.KeShu = 1;
                        //    cond.KeShu_feizhiding = 1;
                        //}
                        cashBll.UpdateJoinUserYejiForPay(cond);
                    }


                }

                #endregion
            }

            //现金包含：银联卡、现金、其他方式支付的现金
            if (entity.Xianjin != 0 || OtherCash != 0 || entity.Yinlianka != 0)
            {
                #region ==添加积分==

                //只有现金支付才算积分
                var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate"); //获取积分实体
                decimal jifen = 0;
                if (usersetting.AttributeValue != null && usersetting.AttributeValue != "")
                {
                    if (LoginUserEntity.UserID == 1017)
                    {
                        //int typeid = Convert.ToInt32(orderInfoList[0].Type);
                        //int proid = Convert.ToInt32(orderInfoList[0].ProdcuitID);
                        //// var productModel=null;
                        //if (typeid == 2)
                        //{
                        //    var productModel1 = baseinfoBLL.GetModel(proid);
                        //    if (productModel1.ID > 0)
                        //    {
                        //        if (productModel1.Proportion > 0)
                        //        {
                        //            // decimal tion = productModel.Proportion /100;
                        //            jifen =
                        //                Convert.ToDecimal(entity.Price < 0
                        //                    ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                        //                    : (entity.Xianjin + OtherCash + entity.Yinlianka)) / Convert.ToDecimal(productModel1.Proportion);
                        //            jifen = CutDecimalWithN(jifen, 0);
                        //            //(usersetting.AttributeValue == null
                        //            //    ? 1000000
                        //            //    : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                        //        }
                        //        else
                        //        {
                        //            if (ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue) != 0)
                        //            {

                        //                jifen =
                        //                    Convert.ToDecimal(entity.Price < 0
                        //                        ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                        //                        : (entity.Xianjin + OtherCash + entity.Yinlianka)) /
                        //                    (usersetting.AttributeValue == null
                        //                        ? 1000000
                        //                        : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                        //            }

                        //        }
                        //    }

                        //}
                        //else
                        //{
                        //    var productModel = cashBll.GetProjectProductModelNew(proid.ToString());
                        //    if (productModel.ID > 0)
                        //    {
                        //        if (productModel.Proportion > 0)
                        //        {
                        //            // decimal tion = productModel.Proportion /100;
                        //            jifen =
                        //                Convert.ToDecimal(entity.Price < 0
                        //                    ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                        //                    : (entity.Xianjin + OtherCash + entity.Yinlianka)) / Convert.ToDecimal(productModel.Proportion);
                        //            jifen = CutDecimalWithN(jifen, 0);
                        //            //(usersetting.AttributeValue == null
                        //            //    ? 1000000
                        //            //    : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                        //        }
                        //        else
                        //        {
                        //            if (ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue) != 0)
                        //            {

                        //                jifen =
                        //                    Convert.ToDecimal(entity.Price < 0
                        //                        ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                        //                        : (entity.Xianjin + OtherCash + entity.Yinlianka)) /
                        //                    (usersetting.AttributeValue == null
                        //                        ? 1000000
                        //                        : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                        //            }
                        //        }
                        //    }
                        //}

                    }
                    else
                    {
                        if (ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue) != 0)
                        {

                            jifen =
                                Convert.ToDecimal(entity.Price < 0
                                    ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                                    : (entity.Xianjin + OtherCash + entity.Yinlianka)) /
                                (usersetting.AttributeValue == null
                                    ? 1000000
                                    : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                        }
                    }
                    //if (ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue) != 0)
                    //{

                    //    jifen =
                    //        Convert.ToDecimal(entity.Price < 0
                    //            ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                    //            : (entity.Xianjin + OtherCash + entity.Yinlianka)) /
                    //        (usersetting.AttributeValue == null
                    //            ? 1000000
                    //            : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                    //}
                }

                IntegralRecordEntity ir = new IntegralRecordEntity();
                ir.UserID = entity.UserID;
                ir.DocumentNumber = entity.OrderNo;
                ir.Integral = jifen;
                ir.CreateTime = DateTime.Now;
                IpatBLL.AddIntegralRecord(ir); //添加用户积分记录

                var intrgral = IpatBLL.GetIntegralModel(entity.UserID); //获取原来的积分列表
                intrgral.AllIntegral = intrgral.AllIntegral + jifen;
                intrgral.Integral = intrgral.Integral + jifen;
                IpatBLL.UpdateIntegral(intrgral);

                #endregion
            }

            #region ==仓库出库OR入库==

            foreach (var info in orderInfoList)
            {
                //仓库去库存
                if (info.Type == 2)
                {
                    UpdateKucun(LoginUserEntity.HospitalID, info.ProdcuitID, info.Price, info.Quantity, entity.OrderNo,
                        entity.UserID);
                }
            }

            #endregion


            try
            {
                //获取顾客订单数量，更新顾客第一次消费时间
                int xiaofeicount = cashBll.GetXiaofeiListCount(entity.UserID);
                if (xiaofeicount == 1)
                {
                    IpatBLL.UpdateFirstTiyanTime(entity.UserID);
                }

                Task.Factory.StartNew(() =>
                {
                    //  var S = new WeiXinMessage().SendConsumerMessage(entity.OrderNo);
                    //推送消费提醒
                    new WeiXinMessage().SendMessage(entity.OrderNo);
                    if (IsProject > 0) //如果存在做护理的，则发送推送消息
                    {
                        //发送点评
                        new WeiXinMessage().SendDianPingMessage(entity.OrderNo);
                    }



                });

                //如果有项目，添加项目跟进
                if (prolist.Contains(1))
                {
                    IpatBLL.AddCustomerService(new CustomerServiceEntity
                    {
                        UserID = entity.UserID,
                        Memo = projectname,
                        FollowUserID = followuserid,
                        OrderNo = entity.OrderNo,
                        ProductType = 1,
                        Statu = 1
                    });
                }

                if (prolist.Contains(2))
                {
                    //产品
                    IpatBLL.AddCustomerService(new CustomerServiceEntity
                    {
                        UserID = entity.UserID,
                        Memo = productname,
                        FollowUserID = followuserid,
                        OrderNo = entity.OrderNo,
                        ProductType = 2,
                        Statu = 1
                    });
                }


                #region 今日待跟进记录（顾客护理日志）1、划卡 2、项目

                if (result > 0 && entity.Statu == 1 && orderInfoList.Any(x => new[] { 3, 5 }.Contains(x.InfoBuyType)))
                {
                    //var modelNowFollowUp = ResDocBLL.GetNowFollowUp(entity.HospitalID, entity.UserID, DateTime.Now);
                    //if (modelNowFollowUp.Id == 0)
                    //{
                    //    ResDocBLL.AddNowFollowUp(new NowFollowUpEntity() {
                    //        HospitalId = entity.HospitalID,
                    //        PatientId = entity.UserID,
                    //        OrderDate = DateTime.Now,
                    //        CreateTime = DateTime.Now});
                    //}

                    var modelNowFollowUp = IpatBLL.GetCustomertracks(LoginUserEntity.HospitalID, entity.UserID, DateTime.Now);
                    if (modelNowFollowUp.ID == 0)
                    {
                        //int row, pagecount;
                        //var joinList = cashBll.GetAllJoinUser(
                        //    new JoinUserEntity { OrderInfoID = orderInfoList.Select(x => x.ID).FirstOrDefault() },
                        //    out row, out pagecount);

                        modelNowFollowUp.ReturnTheme = 1;
                        modelNowFollowUp.Type = 1;
                        modelNowFollowUp.CustomerUserID = entity.UserID;

                        modelNowFollowUp.UserID = followuserid;//joinList.Select(x => x.UserID).FirstOrDefault();
                        modelNowFollowUp.HospitalID = LoginUserEntity.HospitalID;
                        modelNowFollowUp.ReturnTime = DateTime.Now;
                        modelNowFollowUp.LastTime = DateTime.Now;
                        modelNowFollowUp.CreateTime = DateTime.Now;
                        modelNowFollowUp.Projects = string.Join(",", orderInfoList.Where(x => new[] { 3, 5 }.Contains(x.InfoBuyType)).Select(x => x.ProdcuitName));
                        IpatBLL.AddCustomertracks(modelNowFollowUp);
                    }
                    else
                    {

                        //if (!string.IsNullOrEmpty(modelNowFollowUp.Projects))
                        //{
                        //    var projects = new List<string>();
                        //    foreach (var product in orderInfoList.Where(x => new[] { 3, 5 }.Contains(x.InfoBuyType)).Select(x => x.ProdcuitName))
                        //    {
                        //        if (modelNowFollowUp.Projects.IndexOf(product) == -1)
                        //        {
                        //            projects.Add(product);
                        //        }
                        //    }

                        //    modelNowFollowUp.Projects = "," + string.Join(",", projects);
                        //}
                        //else
                        //{
                        //    modelNowFollowUp.Projects = string.Join(",", orderInfoList.Where(x => new[] { 3, 5 }.Contains(x.InfoBuyType)).Select(x => x.ProdcuitName));
                        //}
                        //IpatBLL.UpCustomertracks(modelNowFollowUp);
                    }

                }


                #endregion
            }
            catch (Exception e)
            {

                LogWriter.WriteInfo(e.Message, "发送项目产品消费消息失败");
            }


            return Json(result);
            //}
            //catch (Exception e)
            //{
            //    LogWriter.WriteError(e, "项目产品支付错误");
            //    entity.Statu = (int)OrderStatu.待结算;
            //    //修改订单为待结算
            //    int result = cashBll.UpdateOrder(entity);
            //    return Json(-1);
            //}
        }
        /// <summary>
        ///  修改订单状态、支付金额---支付操作
        /// </summary>
        /// <param name="entity">订单实体</param>
        /// <param name="CardIDstr">用户卡扣余额ID集合</param>
        /// <param name="ordertimesstr">卡扣次数集合</param>
        /// <param name="cardidarr">用户余额记录储值卡ID集合</param>
        /// <param name="txtChuzhikaarr">储值金额集合</param>
        /// <param name="OtherPayment">其他支付方式ID集合</param>
        /// <param name="OtherPaymentValue">其他支付方式支付金额</param>
        /// <param name="couponNo">优惠券号</param>
        /// <param name="depositValue">股本支付金额</param>
        /// <returns></returns>
        [WriteOperateLogAttribute("项目产品点击支付操作，支付订单编号：", "项目产品点击支付操作", "项目产品")]
        public ActionResult UpdateOrder(OrderEntity entity, string CardIDstr, string ordertimesstr, string cardidarr,
                string txtChuzhikaarr, string OtherPayment, string IsCashValue, string IsGiveValue,
                string OtherPaymentValue, string preOrderNOList, string hiddenToken, int chkcreatetime,
                string txtCreateTime, string couponNo, decimal CouponPay, decimal depositValue)
        {

            //防止重复提交
            if (Session["hiddenToken"] == null)
            {
                Session["hiddenToken"] = hiddenToken;
            }
            else
            {
                if (Session["hiddenToken"].ToString() == hiddenToken)
                {
                    LogWriter.WriteInfo("重复支付记录" + entity.OrderNo, "重复支付记录");
                    return Json(-99);
                }
            }

            //操作日志参数
            ViewBag.CacheData = entity.OrderNo;
            //try
            //{
            if (!haveCangku(LoginUserEntity.HospitalID))
            {
                return Json(-2); //如果没有仓库直接返回-2
            }

            if (entity.QianPrice > 0)
            {
                entity.QianPrice = -entity.QianPrice;
            }

            entity.Statu = (int)OrderStatu.已结算;
            entity.ActurePrice = entity.Price + entity.QianPrice;
            entity.AllQianprice = entity.QianPrice;
            //修改订单为已支付
            int result = cashBll.UpdateOrder(entity);
            //根据用户ID更新最后到店时间
            int count = cashBll.Patient_UpdateLastTime1(entity.UserID,Convert.ToDateTime(txtCreateTime));

            decimal OtherCash = 0; //其他方式支付的 现金部分
            decimal Otherquan = 0;//其他劵类支付  部分
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            Dictionary<int, int> isgiveDic = new Dictionary<int, int>();
            Dictionary<int, decimal> otherpay = new Dictionary<int, decimal>(); //其他支付方式金额
            Dictionary<int, int> otherpaytype = new Dictionary<int, int>(); //其他支付方式类型
            decimal allchuzhikaPay = 0;//获取非赠卡支付金额
            decimal jifen11 = 0;//积分
            decimal allzengchuzhikaPay = 0;//获取赠送储值卡支付金额
            #region==订单支付方式==

            //订单结算方式记录
            if (result > 0)
            {
                //删除已挂单操作
                if (!string.IsNullOrEmpty(preOrderNOList))
                {
                    string[] preOrderNOList1 =
                        preOrderNOList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var orderno in preOrderNOList1)
                    {
                        cashBll.DeleteOrder(orderno);
                    }
                }


                //添加订单支付方式
                PaymentOrderEntity poe = new PaymentOrderEntity();



                //其他支付方式
                if (!string.IsNullOrEmpty(OtherPayment) && !string.IsNullOrEmpty(OtherPaymentValue))
                {
                    string[] strpay = OtherPayment.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] OtherPaymentValuelist =
                        OtherPaymentValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] IsCashValueStr =
                        IsCashValue.Split(new char[] { ',' },
                            StringSplitOptions.RemoveEmptyEntries); //此字段判断是否是 现金还是券类 1 现金 2 券类


                    for (int i = 0; i < strpay.Length; i++)
                    {

                        poe.OrderNo = entity.OrderNo;
                        poe.PayType = ConvertValueHelper.ConvertIntValue(strpay[i]);
                        poe.PayMoney = entity.Price < 0
                            ? -ConvertValueHelper.ConvertDecimalValue(OtherPaymentValuelist[i])
                            : ConvertValueHelper.ConvertDecimalValue(OtherPaymentValuelist[i]);
                        otherpay.Add(poe.PayType, poe.PayMoney); //其他支付方式
                        // poe.PayMoney = ;
                        poe.BalanceMoney = 0;
                        poe.BalanceTiems = 0;
                        poe.CardID = 0;
                        if (ConvertValueHelper.ConvertIntValue(IsCashValueStr[i]) == 1)
                        {
                            poe.ParentPayType = (int)ParentPayType.现金;
                            OtherCash = OtherCash + poe.PayMoney;

                        }
                        else
                        {
                            Otherquan = Otherquan + poe.PayMoney;
                            poe.ParentPayType = (int)ParentPayType.券类;
                        }

                        otherpaytype.Add(poe.PayType, poe.ParentPayType); //其他支付方式
                        poe.PayName = sysbll.GetBaseinfoEntity(ConvertValueHelper.ConvertIntValue(strpay[i])).InfoName;
                        cashBll.AddPaymentOrder(poe);
                    }
                }

                if (entity.Xianjin > 0) //现金
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.现金;
                    //退产品
                    poe.PayMoney = entity.Price < 0 ? -entity.Xianjin : entity.Xianjin;
                    poe.PayName = "现金";
                    poe.BalanceMoney = 0;
                    poe.BalanceTiems = 0;
                    poe.CardID = 0;
                    poe.ParentPayType = (int)ParentPayType.现金;

                    cashBll.AddPaymentOrder(poe);
                }

                if (entity.Yinlianka > 0) //银联卡
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.银联卡;
                    poe.PayName = "银联卡";
                    poe.PayMoney = entity.Price < 0 ? -entity.Yinlianka : entity.Yinlianka;
                    poe.BalanceMoney = 0;
                    poe.BalanceTiems = 0;
                    poe.ParentPayType = (int)ParentPayType.现金;
                    poe.CardID = 0;
                    cashBll.AddPaymentOrder(poe);
                }


                if (entity.Chuzhika != 0) //储值卡
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.BalanceTiems = 0;
                    //扣除卡类金额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        string[] ordertimesstrlist =
                            txtChuzhikaarr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] IsGiveValueList =
                            IsGiveValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        string[] caridlist = cardidarr.Split(',');
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            if (IsGiveValueList[f] != "1")
                            {
                                allchuzhikaPay = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) +
                                                 allchuzhikaPay;
                            }
                            else
                            {
                                allzengchuzhikaPay = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) +
                                                     allzengchuzhikaPay;
                            }
                            poe.PayType = 0;
                            poe.PayType = (int)PaymentEnum.储值卡;
                            var zhifuprice = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]);
                            var nowid = ConvertValueHelper.ConvertIntValue(caridlist[f]);
                            dic.Add(nowid, zhifuprice);
                            isgiveDic.Add(nowid, ConvertValueHelper.ConvertIntValue(IsGiveValueList[f]));
                            //获取这张支付卡的详细信息
                            var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = nowid });
                            var chuzhika = baseinfoBLL.GetPrePayCardModel(CardBalance.ProjectID);
                            if (CardBalance.ProjectID == 40112 || CardBalance.ProjectID == 42221)
                            {
                                poe.PayType = 8;
                            }
                            poe.PayName = chuzhika.Name;
                            poe.PayMoney = entity.Price < 0 ? -zhifuprice : zhifuprice;
                            poe.CardBalanceID = nowid;
                            poe.IsGive = ConvertValueHelper.ConvertIntValue(IsGiveValueList[f]);

                            poe.ids = nowid.ToString();
                            poe.CardID = CardBalance.CardID;
                            poe.BalanceMoney = CardBalance.Price - poe.PayMoney;
                            poe.ParentPayType = (int)ParentPayType.储值卡;
                            if (CardBalance.ProjectID == 40112 || CardBalance.ProjectID == 42221)
                            {
                                poe.ParentPayType = 8;
                            }
                            cashBll.AddPaymentOrder(poe);
                            //减去相应金额
                            result = cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity { ID = nowid, Price = poe.PayMoney });

                            //用户储值卡余额明细日志
                            var mainCardBalanceDetail = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity() { ID = nowid });
                            cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                            {
                                CardBalanceId = nowid,
                                OrderNo = poe.OrderNo,
                                Type = (int)MainCardBalanceDetailType.消费,
                                Amount = -poe.PayMoney,
                                Balance = mainCardBalanceDetail.Price,
                                OldAmount = mainCardBalanceDetail.Price + poe.PayMoney,
                                CreateTime = DateTime.Now
                            });
                            if (CardBalance.ProjectID == 40112 || CardBalance.ProjectID == 42221)//积分消费
                            {
                                var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                                jifen11 = intrgral.Integral;
                                intrgral.AllIntegral = mainCardBalanceDetail.Price;
                                intrgral.Integral = mainCardBalanceDetail.Price;
                                int number = IpatBLL.UpdateIntegral(intrgral);
                                IntegralOperationEntity entity1 = new IntegralOperationEntity();
                                entity1.Memo = "消费";
                                entity1.Name = "一";
                                decimal PayMoney1 = 0;

                                if (mainCardBalanceDetail.Price >= 1)
                                {
                                    PayMoney1 = jifen11 - mainCardBalanceDetail.Price;
                                }
                                else
                                {
                                    PayMoney1 = jifen11;
                                }
                                entity1.Number = PayMoney1;
                                entity1.UserID = entity.UserID;
                                entity1.OperationType = 2;
                                var id = IpatBLL.AddIntegralOperation(entity1);
                                IntegralRecordEntity ir1 = new IntegralRecordEntity();
                                ir1.UserID = entity1.UserID;
                                ir1.Integral = PayMoney1;
                                ir1.CreateTime = DateTime.Now;
                                ir1.OrderNO = entity.OrderNo;
                                ir1.DocumentNumber = "消费";
                                ir1.IntegralOperationID = 0;
                                // ir1.IntegralOperationID = id;
                                IpatBLL.AddIntegralRecord(ir1);//增加积分记录
                            }
                        }
                    }



                }

                if (!String.IsNullOrEmpty(CardIDstr)) //划扣次卡
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.划扣次数卡;

                    poe.BalanceMoney = 0;
                    //减去相应的卡类次数
                    if (!String.IsNullOrEmpty(CardIDstr))
                    {
                        string[] caridlist = CardIDstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] ordertimesstrlist =
                            ordertimesstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        //检测是否重复写入
                        List<string> jianche = new List<string>();
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            if (!jianche.Contains(caridlist[f]))
                            {
                                var CardBalanceID = ConvertValueHelper.ConvertIntValue(caridlist[f]);
                                jianche.Add(caridlist[f]);
                                int paycount = cashBll.GetPaymentOrderModelByOrder(new PaymentOrderEntity
                                {
                                    PayType = 4,
                                    CardBalanceID = CardBalanceID,
                                    OrderNo = entity.OrderNo,
                                    BalanceTiems = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f])
                                });
                                //如果存在划卡支付记录
                                if (paycount > 0)
                                {
                                    continue;
                                }

                                //修改用户余额记录,根据ID                      CardID是用户约记录的ID
                                cashBll.UpdateMainCardBalanceTime(new MainCardBalanceEntity
                                {
                                    CardID = CardBalanceID,
                                    Tiems = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f])
                                });
                                // LogWriter.WriteInfo("当前账户结余ID：" + caridlist[f] + "总划扣账户ID:" + CardIDstr, "订单支付方式，单据号：" + entity.OrderNo);

                                //获取这张支付卡的详细信息
                                var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = CardBalanceID });
                                var xiangmu = baseinfoBLL.GetProjectModel(CardBalance.ProjectID);
                                //如果是划次,直接用购买卡的时候的价格来算.
                                //修改代码:L0428002
                                //修复记录:在订单结算时的划卡的金额.现在用的是买卡时的金额.
                                //修复代码:以下7行
                                //------------------------160625修改
                                //现在购买金额用实耗单价算.
                                //因为疗程卡划扣按照实耗算,不是按照购买单价算了.
                                //消耗单价

                                if (CardBalance.Type == 2)
                                {
                                    poe.PayMoney = CardBalance.ConsumePrice *
                                                   ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]);
                                }
                                else
                                {
                                    poe.PayMoney = CardBalance.ConsumePrice == 0
                                        ? xiangmu.CostPrice * ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f])
                                        : CardBalance.ConsumePrice *
                                          ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]);
                                }

                                poe.PayName = xiangmu.Name;
                                poe.CardBalanceID = ConvertValueHelper.ConvertIntValue(caridlist[f]);
                                poe.CardID = CardBalance.CardID;
                                poe.PayTime = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]);
                                poe.BalanceTiems = CardBalance.Tiems;
                                poe.ids = caridlist[f].ToString();
                                poe.ParentPayType = (int)ParentPayType.划卡;
                                cashBll.AddPaymentOrder(poe);
                            }
                            else
                            {
                                LogWriter.WriteInfo("该单据总项目数：" + caridlist.Length + ",同一账户余额只扣除一次：" + caridlist[f],
                                    "重复支付，单据号：" + entity.OrderNo);
                                //跳出循环
                                continue;
                            }

                        }
                    }
                }


            }

            #endregion

            //如果存在欠款，修改用户欠款情况
            if (entity.QianPrice != 0)
            {
                cashBll.UpdateArrearsMoney(entity.UserID, entity.QianPrice);
            }

            var patientEntity = IpatBLL.GetPatienttEntityByID(entity.UserID);
            //订单产品支付方式表
            //现金类：现金支付部分优先产品，产品价格由大到小进行支付结算
            //卡扣类：优先产品
            //根据订单编号查询订单产品表
            var orderInfoList = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = entity.OrderNo });
            //剩余现金部分
            decimal ShengyuCash = entity.Xianjin;
            //已支付现金金额
            decimal PayCash = 0;
            //剩余银联卡部分
            decimal YinLianCard = entity.Yinlianka;
            //已支付银联卡部分
            decimal PayCarded = 0;
            //剩余储值卡
            decimal Chuzhika = entity.Chuzhika;
            //已支付储值卡部分
            decimal ChuzhikaPayed = 0;
            decimal otherpayprice = 0; //其他已经付款的
            //储值卡集合
            List<int> cardlist = new List<int>();
            cardlist.AddRange(dic.Keys);
            //其他支付方式
            List<int> otherpaylist = new List<int>();
            otherpaylist.AddRange(otherpay.Keys);

            //订单详情结算方式记录
            //检测重复支付记录问题
            List<int> jianchepay = new List<int>();
            int IsProject = 0;
            //判断是否包含产品和项目
            List<int> prolist = new List<int>();
            //跟进人id
            int followuserid = 0;
            int producntType = 0;
            int row, pagecount;

            #region 跟进人

            var joins = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = orderInfoList.Select(x => x.ID).FirstOrDefault() }, out row, out pagecount);

            //多对一：判断是否存在指定美容师
            if (joins.Count > 1 && joins.Any(x => x.UserID == patientEntity.FollowUpMrUserID))
            {
                followuserid = patientEntity.FollowUpMrUserID;
            }
            //否则取第一个
            else
            {
                followuserid = joins.Select(x => x.UserID).FirstOrDefault();
            }
            //2019-11-23
            var HospitalUserModel = personnelBLL.GetUserByUserID(followuserid);
            int guid = cashBll.UpdaetOrderInfoGetByID(entity.OrderNo, HospitalUserModel.HospitalID);
            int guid1 = cashBll.UpdaetOrderGetByID(entity.OrderNo, HospitalUserModel.HospitalID);
            #endregion


            foreach (var info in orderInfoList)
            {
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                //新的产品或者项目
                PayCash = 0;
                PayCarded = 0;
                ChuzhikaPayed = 0;
                otherpayprice = 0;
                PaymentOrderProductEntity conditon = new PaymentOrderProductEntity();
                conditon.AllPrice = info.AllPrice; //订单产品总价
                conditon.OrderInfoID = info.ID; //支付订单产品

                if (info.Type == 2) //产品
                {
                    producntType = info.Type;
                    prolist.Add(2);

                    #region==产品支付==

                    #region==现金支付==

                    if (ShengyuCash > 0) //现金
                    {
                        conditon.PayType = (int)PaymentEnum.现金;
                        if (ShengyuCash >= Math.Abs(info.AllPrice)) //如果现金部分超出实际产品价格
                        {
                            conditon.PayMoney = info.AllPrice;
                            ShengyuCash = ShengyuCash - Math.Abs(conditon.PayMoney);
                            PayCash = Math.Abs(conditon.PayMoney);
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue; //跳出循环
                        }
                        else
                        {
                            //现金部分不够
                            conditon.PayMoney = info.AllPrice < 0 ? -ShengyuCash : ShengyuCash;
                            PayCash = ShengyuCash;
                            ShengyuCash = 0;
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                        }
                    } //现金支付

                    #endregion

                    #region==银联卡支付==

                    if (YinLianCard > 0) //银联卡
                    {
                        conditon.PayType = (int)PaymentEnum.银联卡;
                        if (YinLianCard >= Math.Abs(info.AllPrice)) //如果银联卡部分超出实际产品价格
                        {
                            if (PayCash > 0) //支付了部分现金
                            {
                                conditon.PayMoney = info.AllPrice < 0
                                    ? -(Math.Abs(info.AllPrice) - PayCash)
                                    : (Math.Abs(info.AllPrice) - PayCash);
                                //YinLianCard = YinLianCard - conditon.PayMoney;
                                YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                            else
                            {
                                conditon.PayMoney = info.AllPrice;
                                YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                        }
                        else
                        {
                            //银联卡部分不够
                            if (PayCash > 0) //支付了部分现金
                            {
                                if (PayCash + YinLianCard >= Math.Abs(info.AllPrice))
                                {
                                    //conditon.PayMoney = Math.Abs(info.AllPrice) - PayCash;
                                    conditon.PayMoney = info.AllPrice < 0
                                        ? -(Math.Abs(info.AllPrice) - PayCash)
                                        : (Math.Abs(info.AllPrice) - PayCash);
                                    YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                    conditon.ParentPayType = (int)ParentPayType.现金;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue; //跳出循环
                                }
                                else
                                {
                                    // conditon.PayMoney = YinLianCard + PayCash;
                                    //conditon.PayMoney = YinLianCard;
                                    conditon.PayMoney = info.AllPrice < 0 ? -YinLianCard : YinLianCard;
                                    YinLianCard = 0;
                                    PayCarded = Math.Abs(conditon.PayMoney);
                                    conditon.ParentPayType = (int)ParentPayType.现金;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                }

                            }
                            else
                            {
                                // conditon.PayMoney = YinLianCard;
                                conditon.PayMoney = info.AllPrice < 0 ? -YinLianCard : YinLianCard;
                                YinLianCard = 0;
                                PayCarded = Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);

                            }
                        }

                    } //银联卡支付

                    #endregion

                    #region==储值卡支付==

                    //储值卡支付  Chuzhika=储值卡总额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        foreach (var valu in cardlist)
                        {
                            conditon.IsGive = isgiveDic[valu];
                            var manCardid = valu; //用户余额记录ID
                            var manPrice = dic[valu]; //支付金额
                            if (manPrice > 0)
                            {
                                conditon.PayType = (int)PaymentEnum.储值卡;
                                if (manPrice >= Math.Abs(info.AllPrice)) //如果储值卡部分超出实际产品价格
                                {
                                    if (ChuzhikaPayed + PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {

                                        conditon.PayMoney = info.AllPrice < 0
                                            ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                            : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);

                                        dic.Remove(manCardid);

                                        dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额

                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //终止循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        conditon.CardID = manCardid;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                }
                                else
                                {
                                    //储值卡部分不够
                                    if (ChuzhikaPayed + PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {
                                        if (ChuzhikaPayed + PayCash + PayCarded + manPrice > Math.Abs(info.AllPrice))
                                        {
                                            conditon.PayMoney = info.AllPrice < 0
                                                ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                                : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
                                            // Chuzhika = Chuzhika - conditon.PayMoney;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            conditon.ParentPayType = (int)ParentPayType.储值卡;
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            break; //跳出循环
                                        }
                                        else
                                        {
                                            conditon.PayMoney = info.AllPrice < 0 ? -manPrice : manPrice;
                                            //  dic.Remove(manCardid);
                                            ChuzhikaPayed = conditon.PayMoney;
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, (decimal)0.0);
                                            conditon.ParentPayType = (int)ParentPayType.储值卡;
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice < 0 ? -manPrice : manPrice;
                                        //Chuzhika = 0;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, (decimal)0.0);
                                        ChuzhikaPayed = conditon.PayMoney;
                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;
                                    }
                                }
                            }
                            else //如果manprice是负数,给当前储值卡加上这笔钱
                            {
                                if (manPrice == 0)
                                {
                                    continue;
                                }

                                conditon.PayType = (int)PaymentEnum.储值卡;
                                conditon.PayMoney = info.AllPrice < 0
                                    ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded)
                                    : (Math.Abs(info.AllPrice) - PayCash - PayCarded);
                                dic.Remove(manCardid);
                                dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额

                                //添加产品支付方式
                                conditon.CardID = manCardid;
                                conditon.ParentPayType = (int)ParentPayType.储值卡;
                                cashBll.AddPaymentOrderProduct(conditon);
                                break; //终止循环

                            }
                        }
                    }

                    #endregion

                    #region==其他支付===

                    //其他支付方式
                    if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                    {
                        foreach (var PayType in otherpaylist)
                        {
                            var manCardid = PayType; //用户余额记录ID
                            var nowprice = otherpay[PayType]; //支付金额
                            if (otherpaytype[PayType] == 1)
                            {
                                conditon.ParentPayType = (int)ParentPayType.现金;
                            }
                            else
                            {
                                conditon.ParentPayType = (int)ParentPayType.券类;
                            }

                            if (nowprice > 0)
                            {
                                conditon.PayType = PayType;
                                if (nowprice > Math.Abs(info.AllPrice)) //如果大于实际产品价格
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联和储值卡
                                    {
                                        conditon.PayMoney = info.AllPrice < 0
                                            ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                            : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);

                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = Math.Abs(info.AllPrice) - Math.Abs(otherpayprice);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);

                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                }
                                else
                                {
                                    //银联卡部分不够
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联
                                    {
                                        if (PayCash + PayCarded + ChuzhikaPayed + nowprice > Math.Abs(info.AllPrice))
                                        {
                                            conditon.PayMoney = info.AllPrice < 0
                                                ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed -
                                                    otherpayprice)
                                                : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed -
                                                   otherpayprice);
                                            nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                            otherpay.Remove(PayType);
                                            otherpay.Add(PayType, nowprice);

                                            cashBll.AddPaymentOrderProduct(conditon);
                                            break; //跳出循环
                                        }
                                        else
                                        {
                                            otherpayprice = conditon.PayMoney =
                                                info.AllPrice < 0 ? -nowprice : nowprice;
                                            nowprice = 0;
                                            nowprice = Math.Abs(conditon.PayMoney);
                                            otherpay.Remove(PayType);
                                            otherpay.Add(PayType, 0);

                                            cashBll.AddPaymentOrderProduct(conditon);
                                        }

                                    }
                                    else
                                    {
                                        //conditon.PayMoney = nowprice;
                                        otherpayprice = conditon.PayMoney = info.AllPrice < 0 ? -nowprice : nowprice;
                                        nowprice = 0;
                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);
                                        otherpayprice = nowprice = Math.Abs(conditon.PayMoney);

                                        cashBll.AddPaymentOrderProduct(conditon);

                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #endregion
                }
                else
                {
                    //支付项目
                    prolist.Add(1);
                    //获取项目和产品关系，写入院用消耗，虚拟仓库
                    IList<ProjectProductRelationEntity> relationList =
                        baseinfoBLL.GetProjectProductRelation(new ProjectProductRelationEntity { ProjectID = info.ProdcuitID });
                    foreach (var mod in relationList)
                    {
                        iwareBLL.AddProductUseUnit(new ProductUseUnitEntity
                        {
                            HospitalID = LoginUserEntity.HospitalID,
                            ProductID = mod.ProductID,
                            ProjectID = info.ProdcuitID,
                            UseUnit = mod.DanCiUseUnit * info.Quantity,
                            Quatity = info.Quantity
                        });
                        //mod.ProductID
                        // iwareBLL.UpdateProductXuniStock(new ProductXuniStockEntity { HospitalID=loginUserEntity.HospitalID, ProductID = mod.ProductID, UseUnit = mod.DanCiUseUnit * info.Quantity });
                    }

                    #region==项目支付==

                    IsProject = IsProject + 1;

                    #region==划扣次卡==

                    if (info.CardID != 0) //划扣次卡
                    {
                        if (!jianchepay.Contains(info.CardID))
                        {
                            jianchepay.Add(info.CardID);

                            //获取这张支付卡的详细信息
                            var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = info.CardID });
                            conditon.PayType = (int)PaymentEnum.划扣次数卡;
                            conditon.PayMoney = info.AllPrice;
                            conditon.CardID = info.CardID;
                            conditon.ConsumePrice = CardBalance.ConsumePrice; //吧余额下面的员工业绩赋值给详情支付方式表
                            //获取划卡项目支付方式
                            var payordercount = cashBll.GetPaymentOrderModelByOrderInfo(new PaymentOrderProductEntity
                            {
                                CardID = info.CardID,
                                PayType = (int)PaymentEnum.划扣次数卡,
                                OrderInfoID = conditon.OrderInfoID
                            });
                            if (payordercount > 0)
                            {
                                continue;
                            }

                            ;
                            //添加产品支付方式
                            conditon.ParentPayType = (int)ParentPayType.划卡;
                            cashBll.AddPaymentOrderProduct(conditon);
                            LogWriter.WriteInfo("当前账户余额ID：" + info.CardID, "产品支付方式，单据号：" + entity.OrderNo);

                            #region ===划扣次数计算消耗===

                            //获取订单参与人数数量
                            foreach (var cond in joinList)
                            {
                                if (cond.XiaoHao == (decimal)0.00)
                                {
                                    cond.XiaoHao = CardBalance.ConsumePrice * info.Quantity; //这里原来是info.allprice.应为
                                }

                                //修改消耗
                                cashBll.UpdateJoinUserForPay(cond);
                            }

                            #endregion

                        }
                        else
                        {
                            LogWriter.WriteInfo("卡号：" + info.CardID, "重复支付，单据号：" + entity.OrderNo);
                            //跳出循环
                            continue;
                        }

                        continue;
                    } //划扣次卡

                    #endregion

                    #region==现金支付==

                    if (ShengyuCash > 0) //现金
                    {
                        conditon.PayType = (int)PaymentEnum.现金;
                        if (ShengyuCash >= Math.Abs(info.AllPrice)) //如果现金部分超出实际产品价格
                        {
                            conditon.PayMoney = info.AllPrice;
                            ShengyuCash = ShengyuCash - Math.Abs(conditon.PayMoney);
                            PayCash = Math.Abs(conditon.PayMoney);
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue; //跳出循环
                        }
                        else
                        {
                            //现金部分不够
                            conditon.PayMoney = info.AllPrice < 0 ? -ShengyuCash : ShengyuCash;
                            PayCash = ShengyuCash;
                            ShengyuCash = 0;
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                        }
                    } //现金支付

                    #endregion

                    #region==银联卡支付==

                    if (YinLianCard > 0) //银联卡
                    {
                        conditon.PayType = (int)PaymentEnum.银联卡;
                        if (YinLianCard >= Math.Abs(info.AllPrice)) //如果银联卡部分超出实际产品价格
                        {
                            if (PayCash > 0) //支付了部分现金
                            {
                                conditon.PayMoney = info.AllPrice < 0
                                    ? -(Math.Abs(info.AllPrice) - PayCash)
                                    : (Math.Abs(info.AllPrice) - PayCash);
                                //YinLianCard = YinLianCard - conditon.PayMoney;
                                YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                            else
                            {
                                conditon.PayMoney = info.AllPrice;
                                YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                        }
                        else
                        {
                            //银联卡部分不够
                            if (PayCash > 0) //支付了部分现金
                            {
                                if (PayCash + YinLianCard >= Math.Abs(info.AllPrice))
                                {
                                    //conditon.PayMoney = Math.Abs(info.AllPrice) - PayCash;
                                    conditon.PayMoney = info.AllPrice < 0
                                        ? -(Math.Abs(info.AllPrice) - PayCash)
                                        : (Math.Abs(info.AllPrice) - PayCash);
                                    YinLianCard = YinLianCard - Math.Abs(conditon.PayMoney);
                                    conditon.ParentPayType = (int)ParentPayType.现金;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue; //跳出循环
                                }
                                else
                                {
                                    // conditon.PayMoney = YinLianCard + PayCash;
                                    //conditon.PayMoney = YinLianCard;
                                    conditon.PayMoney = info.AllPrice < 0 ? -YinLianCard : YinLianCard;
                                    YinLianCard = 0;
                                    PayCarded = Math.Abs(conditon.PayMoney);
                                    conditon.ParentPayType = (int)ParentPayType.现金;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                }

                            }
                            else
                            {
                                // conditon.PayMoney = YinLianCard;
                                conditon.PayMoney = info.AllPrice < 0 ? -YinLianCard : YinLianCard;
                                YinLianCard = 0;
                                PayCarded = Math.Abs(conditon.PayMoney);
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);

                            }
                        }

                    } //银联卡支付

                    #endregion

                    #region==储值卡支付==

                    //储值卡支付  Chuzhika=储值卡总额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        #region ===划扣次数计算消耗===

                        //获取订单参与人数数量

                        foreach (var cond in joinList)
                        {
                            if (cond.XiaoHao == (decimal)0.00)
                            {
                                cond.XiaoHao = info.AllPrice;
                            }

                            //修改消耗
                            cashBll.UpdateJoinUserForPay(cond);
                        }

                        #endregion

                        foreach (var valu in cardlist)
                        {
                            conditon.IsGive = isgiveDic[valu];
                            var manCardid = valu; //用户余额记录ID
                            var manPrice = dic[valu]; //支付金额
                            if (manPrice > 0)
                            {
                                conditon.PayType = (int)PaymentEnum.储值卡;
                                if (manPrice >= Math.Abs(info.AllPrice)) //如果储值卡部分超出实际产品价格
                                {
                                    if (ChuzhikaPayed + PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {

                                        conditon.PayMoney = info.AllPrice < 0
                                            ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                            : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);

                                        dic.Remove(manCardid);

                                        dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额

                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //终止循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        conditon.CardID = manCardid;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                }
                                else
                                {
                                    //储值卡部分不够
                                    if (ChuzhikaPayed + PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {
                                        if (ChuzhikaPayed + PayCash + PayCarded + manPrice > Math.Abs(info.AllPrice))
                                        {
                                            conditon.PayMoney = info.AllPrice < 0
                                                ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                                : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
                                            // Chuzhika = Chuzhika - conditon.PayMoney;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            conditon.ParentPayType = (int)ParentPayType.储值卡;
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            break; //跳出循环
                                        }
                                        else
                                        {
                                            conditon.PayMoney = info.AllPrice < 0 ? -manPrice : manPrice;
                                            //  dic.Remove(manCardid);
                                            ChuzhikaPayed = conditon.PayMoney;
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, (decimal)0.0);
                                            conditon.ParentPayType = (int)ParentPayType.储值卡;
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice < 0 ? -manPrice : manPrice;
                                        //Chuzhika = 0;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, (decimal)0.0);
                                        ChuzhikaPayed = conditon.PayMoney;
                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;
                                    }
                                }
                            }
                            else //如果manprice是负数,给当前储值卡加上这笔钱
                            {
                                if (manPrice == 0)
                                {
                                    continue;
                                }

                                conditon.PayType = (int)PaymentEnum.储值卡;
                                conditon.PayMoney = info.AllPrice < 0
                                    ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded)
                                    : (Math.Abs(info.AllPrice) - PayCash - PayCarded);
                                dic.Remove(manCardid);
                                dic.Add(manCardid, manPrice - Math.Abs(conditon.PayMoney)); //储值卡剩余金额

                                //添加产品支付方式
                                conditon.CardID = manCardid;
                                conditon.ParentPayType = (int)ParentPayType.储值卡;
                                cashBll.AddPaymentOrderProduct(conditon);
                                break; //终止循环

                            }
                        }
                    }

                    #endregion

                    #region==其他支付===

                    //其他支付方式
                    if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                    {
                        foreach (var PayType in otherpaylist)
                        {
                            var manCardid = PayType; //用户余额记录ID
                            var nowprice = otherpay[PayType]; //支付金额
                            if (otherpaytype[PayType] == 1)
                            {
                                conditon.ParentPayType = (int)ParentPayType.现金;
                            }
                            else
                            {
                                conditon.ParentPayType = (int)ParentPayType.券类;
                            }

                            if (nowprice > 0)
                            {
                                conditon.PayType = PayType;
                                if (nowprice > Math.Abs(info.AllPrice)) //如果大于实际产品价格
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联和储值卡
                                    {
                                        conditon.PayMoney = info.AllPrice < 0
                                            ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed)
                                            : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);

                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = Math.Abs(info.AllPrice) - Math.Abs(otherpayprice);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);

                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                }
                                else
                                {
                                    //银联卡部分不够
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联
                                    {
                                        if (PayCash + PayCarded + ChuzhikaPayed + nowprice > Math.Abs(info.AllPrice))
                                        {
                                            conditon.PayMoney = info.AllPrice < 0
                                                ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed -
                                                    otherpayprice)
                                                : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed -
                                                   otherpayprice);
                                            nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                            otherpay.Remove(PayType);
                                            otherpay.Add(PayType, nowprice);

                                            cashBll.AddPaymentOrderProduct(conditon);
                                            break; //跳出循环
                                        }
                                        else
                                        {
                                            otherpayprice = conditon.PayMoney =
                                                info.AllPrice < 0 ? -nowprice : nowprice;
                                            nowprice = 0;
                                            nowprice = Math.Abs(conditon.PayMoney);
                                            otherpay.Remove(PayType);
                                            otherpay.Add(PayType, 0);

                                            cashBll.AddPaymentOrderProduct(conditon);
                                        }

                                    }
                                    else
                                    {
                                        //conditon.PayMoney = nowprice;
                                        otherpayprice = conditon.PayMoney = info.AllPrice < 0 ? -nowprice : nowprice;
                                        nowprice = 0;
                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);
                                        otherpayprice = nowprice = Math.Abs(conditon.PayMoney);

                                        cashBll.AddPaymentOrderProduct(conditon);

                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #endregion
                }
            }

            string projectname = "";
            string productname = "";
            string host = Request.Url.Host;
            if (LoginUserEntity.HospitalID == 1017)
            {
                NewKeVersion = 0;
            }
            if (NewKeVersion == 2) //美益通和其他算法不一样
            {
                #region==计算员工业绩==

                //重新计算员工业绩
                foreach (var info in orderInfoList)
                {
                    //int row, pagecount;
                    var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row,
                        out pagecount);
                    foreach (var cond in joinList)
                    {
                        //获取第一个跟进人员
                        //if (followuserid == 0)
                        //{
                        //    followuserid = cond.UserID;
                        //}

                        //卡扣业绩
                        var payproduct = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 3 });
                        decimal paymoney = 0;
                        foreach (var m in payproduct)
                        {
                            paymoney += m.PayMoney * m.HuaKouZheLv;

                        }

                        //现金业绩
                        var payproduct1 = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 1 });
                        decimal paymoney1 = 0;
                        foreach (var m in payproduct1)
                        {
                            paymoney1 += m.PayMoney * 1;
                        }
                        #region
                        if (payproduct1.Count == 0) {
                            if (info.AllPrice < 0 && info.Quantity < 0)
                            {
                                paymoney1 = info.AllPrice;
                            }
                        }
                        #endregion


                        //消耗业绩(疗程卡卡扣次数) 
                        var payproduct2 = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 4 });
                        decimal paymoney2 = 0;
                        foreach (var m in payproduct2)
                        {
                            if (m.ConsumePrice != 0)
                            {
                                paymoney2 += m.ConsumePrice * info.Quantity;
                            }
                            else
                            {
                                paymoney2 += m.PayMoney * 1;
                            }
                        }

                        //修改业绩 
                        cond.KakouYeji =
                            ConvertValueHelper.ConvertDecimalValue((paymoney / (joinList.Count)).ToString("f2")); //卡扣业绩
                        cond.Yeji = ConvertValueHelper.ConvertDecimalValue(
                            (paymoney1 / (joinList.Count)).ToString("f2")); //现金业绩


                        //decimal Yuangongyeji_KakouYeji = cond.KakouYeji;
                        //decimal Yuangongyeji_Yeji = cond.Yeji;
                        //如果数量为负数,则吧业绩算出负数  
                        //if (info.Quantity < 0)
                        //{
                        //    cond.KakouYeji = -Yuangongyeji_KakouYeji;
                        //    cond.Yeji = -Yuangongyeji_Yeji;
                        //}

                        if (info.Type == 1) //如果订单详情是项目
                        {
                            var productentity = baseinfoBLL.GetProjectModel(info.ProdcuitID);
                            var tichenglist = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = cond.ProjectID });

                            if (!projectname.Contains(productentity.Name))
                            {
                                projectname += productentity.Name;
                            }

                            //判断产品是否是属于大项目或者特殊项目
                            if (productentity.BaseType == (int)ProductBaseType.特殊项目)
                            {
                                cond.KakouYeji_Teshuxiangmu = cond.KakouYeji;
                                cond.Yeji_Teshuxiangmu = cond.Yeji;
                            }
                            else if (productentity.BaseType == (int)ProductBaseType.大项目)
                            {
                                cond.KakouYeji_Daxiangmu = cond.KakouYeji;
                                cond.Yeji_Daxiangmu = cond.Yeji;
                            }
                            else
                            {
                                cond.KakouYeji_Xiangmu = cond.KakouYeji;
                                cond.Yeji_Xiangmu = cond.Yeji;
                            }

                            //cond.ProjectNumber = info.Quantity;
                            //1、单次项目是大项目只计销售业绩（现金和卡扣）不计消耗，是基础项目和特殊项目计销售也计消耗。
                            //当单次消耗价格≤提成方案里设定的上限数值时，计销售不计消耗计手工或奖励。只能设置一个，如果设置多个提成方案 取第一个，如果记录了消耗就不能记录手工和奖励
                            //3 一个项目只有一个提成方案、提成方案上线=下线 既有实操，又有手工，如果设置了额外奖励，就需要记录额外奖励
                            decimal xiaohaoyeji =
                                ConvertValueHelper.ConvertDecimalValue(
                                    (paymoney2 / (joinList.Count)).ToString("f2")); //消耗业绩

                            //如果设置了提成方案，并且消耗业绩大于提成方案结束金额
                            if (tichenglist.Count() > 0)
                            {
                                var tcinfo = tichenglist[0];
                                if (tcinfo.SpecifiedPrice > 0)
                                {
                                    cond.OtherTiCheng = tcinfo.SpecifiedPrice; //额外奖励，只要设置了就有
                                }

                                if (info.AllPrice / info.Quantity == 0) //金额为0，只有手工没有消耗
                                {
                                    cond.XiaoHao_Liaocheng = cond.XiaoHao = 0;
                                    if (tcinfo.NonSpecifiedType == 1)
                                    {
                                        cond.Ticheng = ((info.AllPrice) * tcinfo.NonSpecifiedPrice) / joinList.Count;
                                    }
                                    else
                                    {
                                        cond.Ticheng = (tcinfo.NonSpecifiedPrice * info.Quantity) / joinList.Count;
                                    }
                                }
                                else if (info.AllPrice / info.Quantity >= tcinfo.StartPrice &&
                                         (info.AllPrice / info.Quantity) <= tcinfo.EndPrice)
                                {
                                    cond.XiaoHao_Liaocheng = cond.XiaoHao = 0;
                                    if (tcinfo.NonSpecifiedType == 1)
                                    {
                                        cond.Ticheng = ((info.AllPrice) * tcinfo.NonSpecifiedPrice) / joinList.Count;
                                    }
                                    else
                                    {
                                        cond.Ticheng = (tcinfo.NonSpecifiedPrice * info.Quantity) / joinList.Count;
                                    }

                                }
                                else if (tcinfo.StartPrice == tcinfo.EndPrice)
                                {
                                    if (xiaohaoyeji > 0) //划卡
                                    {
                                        cond.XiaoHao_Liaocheng = cond.XiaoHao = xiaohaoyeji;
                                    }
                                    else
                                    {
                                        //单项
                                        cond.XiaoHao_Liaocheng = cond.XiaoHao =
                                            cond.KakouYeji > 0 ? cond.KakouYeji : cond.Yeji;
                                    }

                                    if (tcinfo.NonSpecifiedType == 1)
                                    {
                                        cond.Ticheng = ((info.AllPrice) * tcinfo.NonSpecifiedPrice) / joinList.Count;
                                    }
                                    else
                                    {
                                        cond.Ticheng = (tcinfo.NonSpecifiedPrice * info.Quantity) / joinList.Count;
                                    }
                                }
                                else if (info.AllPrice / info.Quantity > tcinfo.EndPrice)
                                {
                                    if (productentity.BaseType != (int)ProductBaseType.大项目)
                                    {
                                        if (xiaohaoyeji > 0) //划卡
                                        {
                                            cond.XiaoHao_Liaocheng = cond.XiaoHao = xiaohaoyeji;
                                        }
                                        else
                                        {
                                            //单项
                                            cond.XiaoHao_Liaocheng = cond.XiaoHao =
                                                cond.KakouYeji > 0 ? cond.KakouYeji : cond.Yeji;
                                        }
                                    }
                                }


                            }
                            else
                            {
                                //没有设置提成方案
                                if (productentity.BaseType != (int)ProductBaseType.大项目)
                                {
                                    if (xiaohaoyeji > 0) //划卡
                                    {
                                        cond.XiaoHao_Liaocheng = cond.XiaoHao = xiaohaoyeji;
                                    }
                                    else
                                    {
                                        //单项
                                        cond.XiaoHao_Liaocheng = cond.XiaoHao;
                                        cond.XiaoHao = cond.KakouYeji > 0 ? cond.KakouYeji : cond.Yeji;
                                    }
                                }
                            }

                            if (productentity.BaseType == (int)ProductBaseType.大项目)
                            {
                                cond.XiaoHao_Liaocheng = cond.XiaoHao = 0;
                            }
                        }
                        else if (info.Type == 2) //产品
                        {
                            var productentity = baseinfoBLL.GetModel(info.ProdcuitID);
                            if (!productname.Contains(productentity.Name))
                            {
                                productname += productentity.Name;
                            }

                            //判断产品是否是属于大项目或者特殊项目
                            if (productentity.BaseType == (int)ProductBaseType.基础项目)
                            {
                                cond.KakouYeji_Chanpin = cond.KakouYeji;
                                cond.Yeji_Chanpin = cond.Yeji;
                            }
                            else if (productentity.BaseType == (int)ProductBaseType.特殊项目)
                            {
                                cond.KakouYeji_TeshuChanpin = cond.KakouYeji;
                                cond.Yeji_TeshuChanpin = cond.Yeji;
                            }
                            //else {
                            //    cond.KakouYeji_Chanpin = cond.KakouYeji;
                            //    cond.Yeji_Chanpin = cond.Yeji;
                            //}


                        }
                        else if (info.Type == 3)
                        {
                            cond.KakouYeji_Liaocheng = cond.KakouYeji;
                            var model = baseinfoBLL.GetPrePayCardModel(info.ProdcuitID);
                            if (model.Type == 1)
                            {
                                cond.Yeji_Chuzhika = cond.Yeji;
                            }

                            if (model.Type == 2)
                            {
                                cond.Yeji_Liaochengka = cond.Yeji;
                            }
                        }
                        //if (cond.XiaoHao > 0)//消耗不为0时,子项是疗程卡消耗
                        //{
                        //    cond.XiaoHao_Liaocheng = cond.XiaoHao;
                        //}


                        //if (cond.IsZhiding == 1)
                        //{
                        //    //实操=疗程卡扣+单项卡扣+单项业绩
                        //    cond.ShiCao_zhiding = cond.KakouYeji_Liaocheng + cond.KakouYeji_Xiangmu + cond.Yeji_Xiangmu;
                        //    cond.ShiCao = cond.ShiCao_zhiding;
                        //    cond.KeShu = 1;
                        //    cond.KeShu_zhiding = 1;
                        //}
                        //else
                        //{
                        //    //实操=疗程卡扣+单项卡扣+单项业绩
                        //    cond.ShiCao_feizhiding = cond.KakouYeji_Liaocheng + cond.KakouYeji_Xiangmu + cond.Yeji_Xiangmu;
                        //    cond.ShiCao = cond.ShiCao_feizhiding;
                        //    cond.KeShu = 1;
                        //    cond.KeShu_feizhiding = 1;
                        //}
                        cashBll.UpdateJoinUserYejiForPay(cond);
                    }


                }

                #endregion
            }
            else
            {
                #region==计算员工业绩==

                //重新计算员工业绩
                foreach (var info in orderInfoList)
                {
                    //int row, pagecount;
                    var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row,
                        out pagecount);
                    var paymentOrderList = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = info.OrderNo });
                    paymentOrderList = paymentOrderList.Where(t => t.PayName == "积分消费").ToList();
                    //  var intrgral = IpatBLL.GetIntegralModel(entity.UserID);
                    foreach (var cond in joinList)
                    {
                        //卡扣业绩
                        var payproduct = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 3 });
                        decimal paymoney = 0;
                        if (LoginUserEntity.HospitalID == 1017)
                        {
                            if (info.Type == 2)
                            {
                                var minCarModel = cashBll.GetProductModelNew(info.ProdcuitID.ToString());
                                if (minCarModel.ID > 0)
                                {
                                    foreach (var m in payproduct)
                                    {
                                        decimal money = 0;
                                        if (minCarModel.Proportion >= jifen11)
                                        {
                                            money = m.AllPrice - jifen11;
                                            paymoney = money * m.HuaKouZheLv;
                                            cond.Proportion = jifen11;
                                        }
                                        else
                                        {
                                            money = m.AllPrice - minCarModel.Proportion;
                                            paymoney = money * m.HuaKouZheLv;
                                            cond.Proportion = minCarModel.Proportion;
                                        }


                                    }
                                }
                            }
                            else
                            {
                                var minCarModel = cashBll.GetProjectProductModelNew(info.ProdcuitID.ToString());

                                if (minCarModel.ID > 0)
                                {
                                    foreach (var m in payproduct)
                                    {
                                        decimal money = 0;


                                        if (minCarModel.Proportion >= jifen11)
                                        {
                                            money = m.AllPrice - jifen11;
                                            paymoney = money * m.HuaKouZheLv;
                                            cond.Proportion = jifen11;
                                        }
                                        else
                                        {
                                            money = m.AllPrice - minCarModel.Proportion;
                                            paymoney = money * m.HuaKouZheLv;
                                            cond.Proportion = minCarModel.Proportion;
                                        }

                                    }
                                }
                            }

                        }
                        else
                        {
                            foreach (var m in payproduct)
                            {
                                paymoney += m.PayMoney * m.HuaKouZheLv;

                            }
                        }

                        //现金业绩
                        var payproduct1 = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 1 });

                        decimal paymoney1 = 0;
                        if (LoginUserEntity.HospitalID == 1017|| LoginUserEntity.HospitalID ==2 && entity.Chuzhika > 0)
                        {

                            if (info.Type == 2)
                            {
                                var minCarModel = cashBll.GetProductModelNew(info.ProdcuitID.ToString());
                                if (minCarModel.ID > 0)
                                {
                                    foreach (var m in payproduct1)
                                    {
                                        decimal money = 0;
                                        if (minCarModel.Proportion >= jifen11)
                                        {
                                            money = m.AllPrice - jifen11;
                                            paymoney1 = money;
                                            cond.Proportion = jifen11;
                                        }
                                        else
                                        {
                                            money = m.AllPrice - minCarModel.Proportion;
                                            paymoney1 = money;
                                            cond.Proportion = minCarModel.Proportion;
                                        }

                                    }
                                }
                            }
                            else
                            {
                                var minCarModel = cashBll.GetProjectProductModelNew(info.ProdcuitID.ToString());
                                if (minCarModel.ID > 0)
                                {
                                    foreach (var m in payproduct1)
                                    {
                                        decimal money = 0;
                                        if (minCarModel.Proportion >= jifen11)
                                        {
                                            money = m.AllPrice - jifen11;
                                            paymoney1 = money;
                                            cond.Proportion = jifen11;
                                        }
                                        else
                                        {
                                            money = m.AllPrice - minCarModel.Proportion;
                                            paymoney1 = money;
                                            cond.Proportion = minCarModel.Proportion;
                                        }

                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var m in payproduct1)
                            {
                                paymoney1 += m.PayMoney * 1;
                            }
                        }


                        //消耗业绩(疗程卡卡扣次数)
                        var payproduct2 = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 4 });
                        decimal paymoney2 = 0;
                        foreach (var m in payproduct2)
                        {
                            if (m.ConsumePrice != 0)
                            {
                                paymoney2 += m.ConsumePrice * info.Quantity;
                            }
                            else
                            {
                                paymoney2 += m.PayMoney * 1;
                            }
                        }

                        //修改业绩



                        cond.KakouYeji =
                            ConvertValueHelper.ConvertDecimalValue((paymoney / (joinList.Count)).ToString("f2")); //卡扣业绩
                        cond.Yeji = ConvertValueHelper.ConvertDecimalValue(
                            (paymoney1 / (joinList.Count)).ToString("f2")); //现金业绩
                        if (LoginUserEntity.HospitalID == 1017|| LoginUserEntity.HospitalID ==2)
                        {
                            if (entity.Xianjin > 0 || entity.Yinlianka > 0)
                            {
                                cond.KakouYeji = 0;
                            }
                        }
                        cond.XiaoHao =
                            ConvertValueHelper.ConvertDecimalValue(
                                (paymoney2 / (joinList.Count)).ToString("f2")); //消耗业绩
                        //decimal Yuangongyeji_KakouYeji = cond.KakouYeji;
                        //decimal Yuangongyeji_Yeji = cond.Yeji;
                        //如果数量为负数,则吧业绩算出负数
                        //if (info.Quantity < 0)
                        //{
                        //    cond.KakouYeji = -Yuangongyeji_KakouYeji;
                        //    cond.Yeji = -Yuangongyeji_Yeji;
                        //}

                        if (info.Type == 1) //如果订单详情是项目
                        {
                            //cond.ProjectNumber = info.Quantity;
                            cond.KakouYeji_Xiangmu = cond.KakouYeji;
                            cond.Yeji_Xiangmu = cond.Yeji;
                            var tichenglist = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = cond.ProjectID });

                            foreach (var tcinfo in tichenglist)
                            {
                                if (info.Price >= tcinfo.StartPrice &&
                                    (info.AllPrice / info.Quantity) <= tcinfo.EndPrice)
                                {
                                    if (cond.Ticheng == 0)
                                    {
                                        if (tcinfo.NonSpecifiedType == 1)
                                        {
                                            cond.Ticheng =
                                                ((info.AllPrice / info.Quantity) * tcinfo.NonSpecifiedPrice) /
                                                joinList.Count;
                                        }
                                        else
                                        {
                                            cond.Ticheng = (tcinfo.NonSpecifiedPrice * info.Quantity) / joinList.Count;
                                        }
                                    }

                                    if (tcinfo.SpecifiedType == 1 && cond.IsZhiding == 1)
                                    {
                                        cond.OtherTiCheng =
                                            cond.OtherTiCheng +
                                            (tcinfo.SpecifiedPrice * (info.AllPrice / info.Quantity) / joinList.Count);
                                    }
                                    else if (cond.IsZhiding == 1)
                                    {
                                        cond.OtherTiCheng = tcinfo.SpecifiedPrice / joinList.Count;
                                    }

                                }
                            }
                        }
                        else if (info.Type == 2) //产品
                        {
                            cond.KakouYeji_Chanpin = cond.KakouYeji;
                            cond.Yeji_Chanpin = cond.Yeji;

                        }
                        else if (info.Type == 3)
                        {
                            cond.KakouYeji_Liaocheng = cond.KakouYeji;
                            var model = baseinfoBLL.GetPrePayCardModel(info.ProdcuitID);
                            if (model.Type == 1)
                            {
                                cond.Yeji_Chuzhika = cond.Yeji;
                            }

                            if (model.Type == 2)
                            {
                                cond.Yeji_Liaochengka = cond.Yeji;
                            }
                        }

                        if (cond.XiaoHao > 0) //消耗不为0时,子项是疗程卡消耗
                        {
                            cond.XiaoHao_Liaocheng = cond.XiaoHao;
                        }


                        //if (cond.IsZhiding == 1)
                        //{
                        //    //实操=疗程卡扣+单项卡扣+单项业绩
                        //    cond.ShiCao_zhiding = cond.KakouYeji_Liaocheng + cond.KakouYeji_Xiangmu + cond.Yeji_Xiangmu;
                        //    cond.ShiCao = cond.ShiCao_zhiding;
                        //    cond.KeShu = 1;
                        //    cond.KeShu_zhiding = 1;
                        //}
                        //else
                        //{
                        //    //实操=疗程卡扣+单项卡扣+单项业绩
                        //    cond.ShiCao_feizhiding = cond.KakouYeji_Liaocheng + cond.KakouYeji_Xiangmu + cond.Yeji_Xiangmu;
                        //    cond.ShiCao = cond.ShiCao_feizhiding;
                        //    cond.KeShu = 1;
                        //    cond.KeShu_feizhiding = 1;
                        //}
                        cashBll.UpdateJoinUserYejiForPay(cond);
                    }


                }

                #endregion
            }

            //现金包含：银联卡、现金、其他方式支付的现金
            if (entity.Xianjin != 0 || OtherCash != 0 || entity.Yinlianka != 0)
            {
                #region ==添加积分==

                //只有现金支付才算积分
                var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate"); //获取积分实体
                decimal jifen = 0;
                if (usersetting.AttributeValue != null && usersetting.AttributeValue != "")
                {
                    if (LoginUserEntity.UserID == 1017)
                    {
                        int typeid = Convert.ToInt32(orderInfoList[0].Type);
                        int proid = Convert.ToInt32(orderInfoList[0].ProdcuitID);
                        // var productModel=null;
                        if (typeid == 2)
                        {
                            var productModel1 = baseinfoBLL.GetModel(proid);
                            if (productModel1.ID > 0)
                            {
                                if (productModel1.Proportion > 0)
                                {
                                    // decimal tion = productModel.Proportion /100;
                                    jifen =
                                        Convert.ToDecimal(entity.Price < 0
                                            ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                                            : (entity.Xianjin + OtherCash + entity.Yinlianka)) / Convert.ToDecimal(productModel1.Proportion);
                                    jifen = CutDecimalWithN(jifen, 0);
                                    //(usersetting.AttributeValue == null
                                    //    ? 1000000
                                    //    : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                                }
                                else
                                {
                                    if (ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue) != 0)
                                    {

                                        jifen =
                                            Convert.ToDecimal(entity.Price < 0
                                                ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                                                : (entity.Xianjin + OtherCash + entity.Yinlianka)) /
                                            (usersetting.AttributeValue == null
                                                ? 1000000
                                                : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                                    }

                                }
                            }

                        }
                        else
                        {
                            var productModel = cashBll.GetProjectProductModelNew(proid.ToString());
                            if (productModel.ID > 0)
                            {
                                if (productModel.Proportion > 0)
                                {
                                    // decimal tion = productModel.Proportion /100;
                                    jifen =
                                        Convert.ToDecimal(entity.Price < 0
                                            ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                                            : (entity.Xianjin + OtherCash + entity.Yinlianka)) / Convert.ToDecimal(productModel.Proportion);
                                    jifen = CutDecimalWithN(jifen, 0);
                                    //(usersetting.AttributeValue == null
                                    //    ? 1000000
                                    //    : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                                }
                                else
                                {
                                    if (ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue) != 0)
                                    {

                                        jifen =
                                            Convert.ToDecimal(entity.Price < 0
                                                ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                                                : (entity.Xianjin + OtherCash + entity.Yinlianka)) /
                                            (usersetting.AttributeValue == null
                                                ? 1000000
                                                : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        if (ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue) != 0)
                        {

                            jifen =
                                Convert.ToDecimal(entity.Price < 0
                                    ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                                    : (entity.Xianjin + OtherCash + entity.Yinlianka)) /
                                (usersetting.AttributeValue == null
                                    ? 1000000
                                    : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                        }
                    }
                    //if (ConvertValueHelper.ConvertIntValue(usersetting.AttributeValue) != 0)
                    //{

                    //    jifen =
                    //        Convert.ToDecimal(entity.Price < 0
                    //            ? -(entity.Xianjin + OtherCash + entity.Yinlianka)
                    //            : (entity.Xianjin + OtherCash + entity.Yinlianka)) /
                    //        (usersetting.AttributeValue == null
                    //            ? 1000000
                    //            : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                    //}
                }
                if (LoginUserEntity.HospitalID == 1017)
                {

                }
                else
                {
                    IntegralRecordEntity ir = new IntegralRecordEntity();
                    ir.UserID = entity.UserID;
                    ir.DocumentNumber = entity.OrderNo;
                    ir.Integral = jifen;
                    ir.CreateTime = DateTime.Now;
                    IpatBLL.AddIntegralRecord(ir); //添加用户积分记录

                    var intrgral = IpatBLL.GetIntegralModel(entity.UserID); //获取原来的积分列表
                    intrgral.AllIntegral = intrgral.AllIntegral + jifen;
                    intrgral.Integral = intrgral.Integral + jifen;
                    IpatBLL.UpdateIntegral(intrgral);
                }

                #endregion
            }

            #region ==仓库出库OR入库==

            foreach (var info in orderInfoList)
            {
                //仓库去库存
                if (info.Type == 2)
                {
                    UpdateKucun(LoginUserEntity.HospitalID, info.ProdcuitID, info.Price, info.Quantity, entity.OrderNo,
                        entity.UserID);
                }
            }

            #endregion


            try
            {
                //获取顾客订单数量，更新顾客第一次消费时间
                int xiaofeicount = cashBll.GetXiaofeiListCount(entity.UserID);
                if (xiaofeicount == 1)
                {
                    IpatBLL.UpdateFirstTiyanTime(entity.UserID);
                }

                Task.Factory.StartNew(() =>
                {
                    //  var S = new WeiXinMessage().SendConsumerMessage(entity.OrderNo);
                    //推送消费提醒
                    new WeiXinMessage().SendMessage(entity.OrderNo);
                    if (IsProject > 0) //如果存在做护理的，则发送推送消息
                    {
                        //发送点评
                        new WeiXinMessage().SendDianPingMessage(entity.OrderNo);
                    }



                });

                //如果有项目，添加项目跟进
                if (prolist.Contains(1))
                {
                    IpatBLL.AddCustomerService(new CustomerServiceEntity
                    {
                        UserID = entity.UserID,
                        Memo = projectname,
                        FollowUserID = followuserid,
                        OrderNo = entity.OrderNo,
                        ProductType = 1,
                        Statu = 1
                    });
                }

                if (prolist.Contains(2))
                {
                    //产品
                    IpatBLL.AddCustomerService(new CustomerServiceEntity
                    {
                        UserID = entity.UserID,
                        Memo = productname,
                        FollowUserID = followuserid,
                        OrderNo = entity.OrderNo,
                        ProductType = 2,
                        Statu = 1
                    });
                }


                #region 今日待跟进记录（顾客护理日志）1、划卡 2、项目

                if (result > 0 && entity.Statu == 1 && orderInfoList.Any(x => new[] { 3, 5 }.Contains(x.InfoBuyType)))
                {
                    //var modelNowFollowUp = ResDocBLL.GetNowFollowUp(entity.HospitalID, entity.UserID, DateTime.Now);
                    //if (modelNowFollowUp.Id == 0)
                    //{
                    //    ResDocBLL.AddNowFollowUp(new NowFollowUpEntity() {
                    //        HospitalId = entity.HospitalID,
                    //        PatientId = entity.UserID,
                    //        OrderDate = DateTime.Now,
                    //        CreateTime = DateTime.Now});
                    //}

                    var modelNowFollowUp = IpatBLL.GetCustomertracks(LoginUserEntity.HospitalID, entity.UserID, DateTime.Now);
                    if (modelNowFollowUp.ID == 0)
                    {
                        //int row, pagecount;
                        //var joinList = cashBll.GetAllJoinUser(
                        //    new JoinUserEntity { OrderInfoID = orderInfoList.Select(x => x.ID).FirstOrDefault() },
                        //    out row, out pagecount);

                        modelNowFollowUp.ReturnTheme = 1;
                        modelNowFollowUp.Type = 1;
                        modelNowFollowUp.CustomerUserID = entity.UserID;

                        modelNowFollowUp.UserID = followuserid;//joinList.Select(x => x.UserID).FirstOrDefault();
                        modelNowFollowUp.HospitalID = LoginUserEntity.HospitalID;
                        modelNowFollowUp.ReturnTime = DateTime.Now;
                        modelNowFollowUp.LastTime = DateTime.Now;
                        modelNowFollowUp.CreateTime = DateTime.Now;
                        modelNowFollowUp.Projects = string.Join(",", orderInfoList.Where(x => new[] { 3, 5 }.Contains(x.InfoBuyType)).Select(x => x.ProdcuitName));
                        IpatBLL.AddCustomertracks(modelNowFollowUp);
                    }
                    else
                    {

                        //if (!string.IsNullOrEmpty(modelNowFollowUp.Projects))
                        //{
                        //    var projects = new List<string>();
                        //    foreach (var product in orderInfoList.Where(x => new[] { 3, 5 }.Contains(x.InfoBuyType)).Select(x => x.ProdcuitName))
                        //    {
                        //        if (modelNowFollowUp.Projects.IndexOf(product) == -1)
                        //        {
                        //            projects.Add(product);
                        //        }
                        //    }

                        //    modelNowFollowUp.Projects = "," + string.Join(",", projects);
                        //}
                        //else
                        //{
                        //    modelNowFollowUp.Projects = string.Join(",", orderInfoList.Where(x => new[] { 3, 5 }.Contains(x.InfoBuyType)).Select(x => x.ProdcuitName));
                        //}
                        //IpatBLL.UpCustomertracks(modelNowFollowUp);
                    }

                }


                #endregion
            }
            catch (Exception e)
            {

                LogWriter.WriteInfo(e.Message, "发送项目产品消费消息失败");
            }


            return Json(result);
            //}
            //catch (Exception e)
            //{
            //    LogWriter.WriteError(e, "项目产品支付错误");
            //    entity.Statu = (int)OrderStatu.待结算;
            //    //修改订单为待结算
            //    int result = cashBll.UpdateOrder(entity);
            //    return Json(-1);
            //}
        }


        /// <summary>
        ///修改库存
        /// </summary>
        /// <param name="HospitalID"></param>
        /// <param name="ProductID"></param>
        /// <param name="price"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public int UpdateKucun(int HospitalID, int ProductID, decimal price, int number, string orderno, int UserID)
        {
            var ProductModel = baseinfoBLL.GetModel(ProductID);
            var result = 0;
            var thetype = sysbll.GetBaseInfoTreeByType("WarehouseIn", 1, LoginUserEntity.ParentHospitalID)
                .Where(c => c.InfoName == "销售出库").ToList(); //获取库存原因
            List<ProductStockEntity> kucunlist = new List<ProductStockEntity>(); //库存列表
            var cangkulist = iwareBLL.GetWarehouseList(new ServiceEntity.WarehouseEntity.WarehouseEntity { HospitalID = HospitalID });
            //如果有仓库则可以保存
            if (cangkulist.Count > 0)
            {
                //这里先遍历出这个用户有几个仓库有着产品库存.

                #region ==先把每个仓库的库存添加kucunlist==

                foreach (var cangku in cangkulist)
                {
                    ProductStockEntity entity = new ProductStockEntity();
                    entity.HospitalID = HospitalID;
                    entity.ProductID = ProductID;
                    entity.HouseID = cangku.ID;
                    var list = iwareBLL.GetProductStockListToseleect(entity);
                    if (list != null)
                    {
                        foreach (var info in list)
                        {
                            kucunlist.Add(info);
                        }
                    }


                }

                #endregion



                ProductStockEntity pse = new ProductStockEntity();
                //如果有产品库存,则减去库存,生成出库单
                if (kucunlist.Count > 0)
                {
                    var projectmodel = baseinfoBLL.GetModel(ProductID);
                    if (kucunlist[0].Quatity >= number && number > 0) //如果库存比数量多,直接扣库存
                    {
                        #region ==新建出库单据==

                        StockOrderEntity entity = new StockOrderEntity();
                        entity.BaseID = thetype.Count > 0 ? thetype[0].ID : 0;
                        entity.HospitalID = HospitalID;
                        entity.IsVerify = 2;
                        entity.Memo = "项目产品订单出库单据";
                        entity.OrderNo = RandomHelper.GetRandomStock("C");
                        entity.OrderTime = DateTime.Now;
                        entity.Type = 1;
                        entity.UserID = LoginUserEntity.UserID;
                        entity.VerifyTime = DateTime.Now;
                        entity.VerifyUserID = LoginUserEntity.UserID;
                        entity.Class = 2;
                        entity.VerifyMemo = orderno;
                        entity.AllMoney = price * number;
                        entity.AllQuatity = number;
                        entity.VerifyMemo = orderno;
                        entity.AllChengBen = projectmodel.CostPrice * number;
                        entity.DocumentNumber =
                            new WarehouseManageController().GetDocumentNumber(LoginUserEntity.HospitalID, 1);
                        //添加出库单据

                        var rt = iwareBLL.AddStockOrder(entity);

                        #endregion

                        #region ==添加单据详情==

                        //添加单据详情
                        StockOrderInfo soinfo = new StockOrderInfo();
                        soinfo.StockOrderNo = entity.OrderNo;
                        soinfo.Number = number;
                        soinfo.ProductID = ProductID;
                        soinfo.HouseID = cangkulist[0].ID;
                        iwareBLL.AddStockOrderInfo(soinfo);

                        #endregion

                        #region ==出库操作==

                        if (rt > 0)
                        {
                            StockChangeLogEntity scl = new StockChangeLogEntity();
                            scl.UserID = LoginUserEntity.UserID;
                            scl.LogClass = 7;
                            scl.LogType = "出库";
                            scl.OwnedWarehouse = soinfo.HouseID;
                            scl.IntoWarehouse = 0;
                            scl.ProductID = ProductModel.ID;
                            scl.LogTitle = "顾客购买产品出库.产品名称:" + ProductModel.Name + "; 产品ID:" + ProductModel.ID +
                                           ";购买数量:" + number + ";";
                            scl.NewQuatity = kucunlist[0].Quatity - number;
                            scl.OldQuatity = kucunlist[0].Quatity;
                            scl.Quantity = number;
                            scl.ProductStockID = kucunlist[0].ID;
                            scl.OrderNo = orderno;

                            var sclID = iwareBLL.AddStockChangeLog(scl);


                            //这里需要出库操作
                            pse.ID = kucunlist[0].ID;
                            pse.Quatity = kucunlist[0].Quatity - number;
                            result = iwareBLL.UpdateProductStock(pse);
                            if (result > 0) //如果实际出库成功,则更改当前这条日志的状态
                            {

                                int len = 2;
                                var usersetting =
                                    sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "StockNumber");
                                if (usersetting != null)
                                {
                                    try
                                    {
                                        len = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(usersetting
                                            .AttributeValue);
                                    }
                                    catch (Exception)
                                    {
                                        len = 2;
                                    }
                                }

                                if (pse.Quatity <= len)
                                {
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

                        #endregion
                    }
                    else if (number > 0 && kucunlist[0].Quatity > 0 && kucunlist[0].Quatity < number
                    ) //只有销售数量和库存都有货，才能出库
                    {
                        #region ==新建出库单据==

                        StockOrderEntity entity = new StockOrderEntity();
                        entity.BaseID = thetype.Count > 0 ? thetype[0].ID : 0;
                        ;
                        entity.HospitalID = HospitalID;
                        entity.IsVerify = 2;
                        entity.Memo = "项目产品订单出库单据";
                        entity.OrderNo = RandomHelper.GetRandomStock("C");
                        entity.OrderTime = DateTime.Now;
                        entity.Type = 1;
                        entity.UserID = LoginUserEntity.UserID;
                        entity.VerifyTime = DateTime.Now;
                        entity.VerifyUserID = LoginUserEntity.UserID;
                        entity.Class = 2;
                        entity.VerifyMemo = orderno;
                        entity.AllMoney = price * kucunlist[0].Quatity;
                        entity.AllQuatity = kucunlist[0].Quatity;
                        entity.DocumentNumber =
                            new WarehouseManageController().GetDocumentNumber(LoginUserEntity.HospitalID, 1);
                        //添加出库单据
                        var rt = iwareBLL.AddStockOrder(entity);

                        #endregion

                        #region ==添加单据详情==

                        //添加单据详情
                        StockOrderInfo soinfo = new StockOrderInfo();
                        soinfo.StockOrderNo = entity.OrderNo;
                        soinfo.Number = kucunlist[0].Quatity;
                        soinfo.ProductID = ProductID;
                        soinfo.HouseID = cangkulist[0].ID;

                        iwareBLL.AddStockOrderInfo(soinfo);

                        #endregion

                        #region ==出库操作==

                        if (rt > 0)
                        {
                            StockChangeLogEntity scl = new StockChangeLogEntity();
                            scl.UserID = LoginUserEntity.UserID;
                            scl.LogClass = 7;
                            scl.LogType = "出库";
                            scl.OwnedWarehouse = cangkulist[0].ID;
                            scl.IntoWarehouse = 0;
                            scl.ProductID = ProductModel.ID;

                            scl.LogTitle = "顾客购买产品出库.产品名称:" + ProductModel.Name + "; 产品ID:" + ProductModel.ID +
                                           ";购买数量:" + number + ";";
                            scl.NewQuatity = 0; //由于这个是数量不够,到0自动加到预约单上,所以新的数量是0
                            scl.OldQuatity = kucunlist[0].Quatity; //老的数量是它原来的库存
                            scl.Quantity = kucunlist[0].Quatity; //调出多少即掉了所有的库存
                            scl.ProductStockID = kucunlist[0].ID;
                            scl.OrderNo = orderno;

                            var sclID = iwareBLL.AddStockChangeLog(scl);

                            //这里需要出库操作
                            pse.ID = kucunlist[0].ID;
                            pse.Quatity = 0;
                            result = iwareBLL.UpdateProductStock(pse);
                            if (result > 0) //如果实际出库成功,则更改当前这条日志的状态
                            {
                                int len = 2;
                                var usersetting =
                                    sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "StockNumber");
                                if (usersetting != null)
                                {
                                    try
                                    {
                                        len = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(usersetting
                                            .AttributeValue);
                                    }
                                    catch (Exception)
                                    {
                                        len = 2;
                                    }
                                }

                                if (pse.Quatity <= len)
                                {
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

                        #endregion


                        #region==欠货管理==

                        //欠货处理
                        JiChunEntity jichunmodel = new JiChunEntity();
                        jichunmodel.HospitalID = LoginUserEntity.HospitalID;
                        jichunmodel.CreateTime = DateTime.Now;
                        jichunmodel.JichunTime = DateTime.Now;
                        jichunmodel.OrderNo = RandomHelper.GetRandomStock("Q");
                        jichunmodel.Type = 2; //欠货
                        jichunmodel.UserID = UserID;
                        jichunmodel.Memo = orderno;
                        jichunmodel.IsActive = 1;
                        //添加寄存主表
                        var jichunID = iwareBLL.AddJiChun(jichunmodel);
                        JichunDetailEntity info = new JichunDetailEntity();

                        info.JiChunID = jichunID;
                        info.JichunCount = number - kucunlist[0].Quatity;
                        info.LingquCount = 0;
                        info.ProductID = ProductID;
                        info.ShengYuCount = info.JichunCount;
                        //添加寄存详情表
                        iwareBLL.AddJiChunProduct(info);

                        #endregion
                    }
                    else if (number > 0 && kucunlist[0].Quatity <= 0)
                    {
                        #region==欠货管理==

                        //欠货处理
                        JiChunEntity jichunmodel = new JiChunEntity();
                        jichunmodel.HospitalID = LoginUserEntity.HospitalID;
                        jichunmodel.CreateTime = DateTime.Now;
                        jichunmodel.JichunTime = DateTime.Now;
                        jichunmodel.OrderNo = RandomHelper.GetRandomStock("Q");
                        jichunmodel.Type = 2; //欠货
                        jichunmodel.UserID = UserID;
                        jichunmodel.Memo = orderno;
                        jichunmodel.IsActive = 1;
                        //添加寄存主表
                        var jichunID = iwareBLL.AddJiChun(jichunmodel);
                        JichunDetailEntity info = new JichunDetailEntity();

                        info.JiChunID = jichunID;
                        info.JichunCount = number;
                        info.LingquCount = 0;
                        info.ProductID = ProductID;
                        info.ShengYuCount = info.JichunCount;
                        //添加寄存详情表
                        iwareBLL.AddJiChunProduct(info);

                        #endregion
                    }
                    else if (number < 0)
                    {
                        #region ==新建入库单据==

                        StockOrderEntity entity = new StockOrderEntity();
                        entity.BaseID = thetype.Count > 0 ? thetype[0].ID : 0;
                        ;
                        entity.HospitalID = HospitalID;
                        entity.IsVerify = 2;
                        entity.Memo = "项目产品订单退产品单据";
                        entity.OrderNo = RandomHelper.GetRandomStock("R");
                        entity.OrderTime = DateTime.Now;
                        entity.Type = 2;
                        entity.UserID = LoginUserEntity.UserID;
                        entity.VerifyTime = DateTime.Now;
                        entity.VerifyUserID = LoginUserEntity.UserID;
                        entity.Class = 5;
                        entity.AllMoney = price * number;
                        entity.VerifyMemo = orderno;
                        entity.AllQuatity = -number;
                        entity.AllChengBen = -number * projectmodel.CostPrice;
                        entity.DocumentNumber =
                            new WarehouseManageController().GetDocumentNumber(LoginUserEntity.HospitalID, 2);
                        //添加出库单据
                        var rt = iwareBLL.AddStockOrder(entity);

                        #endregion

                        #region ==添加单据详情==

                        //添加单据详情
                        StockOrderInfo soinfo = new StockOrderInfo();
                        soinfo.StockOrderNo = entity.OrderNo;
                        soinfo.Number = -number;
                        soinfo.ProductID = ProductID;
                        soinfo.HouseID = cangkulist[0].ID;
                        iwareBLL.AddStockOrderInfo(soinfo);

                        #endregion

                        #region ==入库操作==

                        if (rt > 0)
                        {
                            StockChangeLogEntity scl = new StockChangeLogEntity();
                            scl.UserID = LoginUserEntity.UserID;
                            scl.LogClass = 7;
                            scl.LogType = "入库";
                            scl.OwnedWarehouse = 0;
                            scl.IntoWarehouse = soinfo.HouseID;
                            scl.ProductID = ProductModel.ID;
                            scl.LogTitle = "顾客退返购买产品出库.产品名称:" + ProductModel.Name + "; 产品ID:" + ProductModel.ID +
                                           ";购买数量:" + number + ";";
                            scl.NewQuatity = kucunlist[0].Quatity - number;
                            scl.OldQuatity = kucunlist[0].Quatity;
                            scl.Quantity = -number;
                            scl.ProductStockID = kucunlist[0].ID;
                            scl.OrderNo = orderno;

                            var sclID = iwareBLL.AddStockChangeLog(scl);
                            //这里需要出库操作
                            pse.ID = kucunlist[0].ID;
                            pse.Quatity = kucunlist[0].Quatity - number; //减负就是加上
                            result = iwareBLL.UpdateProductStock(pse);
                            if (result > 0) //如果实际出库成功,则更改当前这条日志的状态
                            {
                                int len = 2;
                                var usersetting =
                                    sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "StockNumber");
                                if (usersetting != null)
                                {
                                    try
                                    {
                                        len = ConvertValueHelper.ConvertIntValue(usersetting
                                            .AttributeValue);
                                    }
                                    catch (Exception)
                                    {
                                        len = 2;
                                    }
                                }

                                if (pse.Quatity <= len)
                                {
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

                        #endregion
                    }


                }
                else //如果没有产品库存,直接添加一个仓库
                {
                    #region ==直接添加0库存==

                    pse.HouseID = cangkulist[0].ID;
                    pse.HospitalID = HospitalID;
                    pse.ProductID = ProductID;
                    pse.Quatity = 0;
                    result = Convert.ToInt32(iwareBLL.AddProductStock(pse));

                    #endregion

                    //#region ==新建出库单据==
                    //StockOrderEntity entity = new StockOrderEntity();
                    //entity.BaseID = thetype[0].ID;
                    //entity.HospitalID = HospitalID;
                    //entity.IsVerify = 2;
                    //entity.Memo = "项目产品订单出库单据";
                    //entity.OrderNo = RandomHelper.GetRandomStock("C");
                    //entity.OrderTime = DateTime.Now;
                    //entity.Type = 1;
                    //entity.UserID = loginUserEntity.UserID;
                    //entity.VerifyTime = DateTime.Now;
                    //entity.VerifyUserID = loginUserEntity.UserID;
                    //entity.Class = 2;
                    //entity.AllMoney = price * number;
                    //entity.AllQuatity = number;
                    ////添加出库单据
                    //var rt = iwareBLL.AddStockOrder(entity);
                    //#endregion

                    //#region ==添加单据详情==
                    ////添加单据详情
                    //StockOrderInfo soinfo = new StockOrderInfo();
                    //soinfo.StockOrderNo = entity.OrderNo;
                    //soinfo.Number = number;
                    //soinfo.ProductID = ProductID;
                    //soinfo.HouseID = cangkulist[0].ID;

                    //iwareBLL.AddStockOrderInfo(soinfo);
                    //#endregion

                    // #region ==新建预付单据  这部分可以删除掉的==
                    // StockOrderEntity Yufu1 = new StockOrderEntity();
                    // var usermodel = IpatBLL.GetPatienttEntityByID(UserID);//获取顾客姓名
                    // Yufu1.BaseID = thetype[0].ID;
                    // Yufu1.HospitalID = HospitalID;
                    // Yufu1.IsVerify = 1;
                    // Yufu1.Memo = "项目产品订单预付单据";
                    // Yufu1.OrderNo = RandomHelper.GetRandomStock("Y");
                    // Yufu1.OrderTime = DateTime.Now;
                    // Yufu1.Type = 5;
                    // Yufu1.UserID = loginUserEntity.UserID;
                    // Yufu1.VerifyTime = DateTime.Now;
                    // Yufu1.VerifyUserID = loginUserEntity.UserID;
                    // Yufu1.Class = 1;
                    // Yufu1.VerifyMemo = orderno;
                    // Yufu1.AllMoney = price * number;
                    // Yufu1.AllQuatity = number;
                    // Yufu1.CustomerName = usermodel.UserName;
                    // Yufu1.ProductList = baseinfoBLL.GetModel(ProductID).Name;
                    // //添加预付单据
                    //// var yf = iwareBLL.AddStockOrder(Yufu1);




                    // //添加预付详情
                    // //添加单据详情
                    // StockOrderInfo yfinfo = new StockOrderInfo();
                    // yfinfo.StockOrderNo = Yufu1.OrderNo;
                    // yfinfo.Number = number;
                    // yfinfo.ProductID = ProductID;
                    // yfinfo.HouseID = pse.HouseID;
                    // iwareBLL.AddStockOrderInfo(yfinfo);
                    // #endregion


                    //如果是退产品，则不添加欠货
                    if (number > 0)
                    {

                        #region==欠货管理==

                        //欠货处理
                        JiChunEntity jichunmodel = new JiChunEntity();
                        jichunmodel.HospitalID = LoginUserEntity.HospitalID;
                        jichunmodel.CreateTime = DateTime.Now;
                        jichunmodel.JichunTime = DateTime.Now;
                        jichunmodel.OrderNo = RandomHelper.GetRandomStock("Q");
                        jichunmodel.Type = 2; //欠货
                        jichunmodel.UserID = UserID;
                        jichunmodel.Memo = orderno;
                        jichunmodel.IsActive = 1;
                        //添加寄存主表
                        var jichunID = iwareBLL.AddJiChun(jichunmodel);
                        JichunDetailEntity info = new JichunDetailEntity();

                        info.JiChunID = jichunID;
                        info.JichunCount = number;
                        info.LingquCount = 0;
                        info.ProductID = ProductID;
                        info.ShengYuCount = info.JichunCount;
                        //添加寄存详情表
                        iwareBLL.AddJiChunProduct(info);

                        #endregion
                    }

                }

            }
            else
            {
                result = 0;
            }

            return result;

        }

        /// <summary>
        /// 验证是否有仓库,有仓库才操作,没有返回false
        /// </summary>
        /// <param name="HospitalID"></param>
        /// <returns></returns>
        public bool haveCangku(int HospitalID)
        {
            var cangkulist = iwareBLL.GetWarehouseList(new ServiceEntity.WarehouseEntity.WarehouseEntity { HospitalID = HospitalID });
            return cangkulist.Count > 0;
        }

        #endregion

        #region==挂单 提单  床位入口操作

        /// <summary>
        /// 保存订单---挂单操作
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult SaveOrderandupdatebed(OrderEntity entity, string Memo)
        {
            int results = 0;
            entity.CreateTime = DateTime.Now;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
            entity.Statu = (int)OrderStatu.待结算;
            entity.OrderType = (int)OrderType.项目产品;
            entity.Name = Memo;

            //汪-----验证是否存在挂单
            var num = cashBll.GetUserIdOrder(entity);
            if (num > 0)
            {
                return Json(null);
            }
            //汪

            var mendian = personnelBLL.HospitalEntityByID(LoginUserEntity.HospitalID);
            string time = DateTime.Now.ToString("MMdd");
            var count = cashBll.GetTodayOrderCount(LoginUserEntity.HospitalID, DateTime.Now); //获取总数
            entity.DocumentNumber = mendian.Prefix + time + (count + 1).ToString("d3");

            string[] orderprice = Request.Form.GetValues("orderprice"); //总价
            string[] ordertimes = Request.Form.GetValues("ordertimes"); //总数
            string[] GiveIDList = Request.Form.GetValues("GiveIDList"); //主键ID
            string[] TypeList = Request.Form.GetValues("TypeList"); //1项目 2产品
            string[] CardType = Request.Form.GetValues("CardType"); //1、扣卡类 2现金类
            string[] PriceList = Request.Form.GetValues("PriceList"); //1、单价
            string[] IsZhidingList = Request.Form.GetValues("IsZhiding"); //1、是否指定
            string[] XiaoHao = Request.Form.GetValues("XiaoHao"); //1、消耗
            string[] Ticheng = Request.Form.GetValues("Ticheng"); //1、手工
            string[] CardID = Request.Form.GetValues("CardID"); //1、卡ID

            string bed = Request["bedid"]; //床位ID
            string orderno = Request["OrderNo"]; //订单编号
            int resultinfoid = 0;

            if (!string.IsNullOrEmpty(orderno)) //订单编号不为空的时候则代表预约的继续挂单.
            {
                #region ==不为空的情况下==

                OrderEntity oe = new OrderEntity();
                oe.OrderNo = orderno;
                var result = cashBll.GetOrderModel(oe);
                OrderinfoEntity orderinfo = new OrderinfoEntity();
                orderinfo.OrderNo = orderno;

                #region ==删除详情==

                cashBll.DeleteOrderInfo(orderno);

                #endregion

                #region ==添加订单详情==

                for (int i = 0; i < orderprice.Length; i++)
                {
                    orderinfo.Type = ConvertValueHelper.ConvertIntValue(TypeList[i]); //类型
                    orderinfo.AllPrice = Convert.ToDecimal(orderprice[i]);
                    orderinfo.Quantity = ConvertValueHelper.ConvertIntValue(ordertimes[i]);
                    orderinfo.ProdcuitID = ConvertValueHelper.ConvertIntValue(GiveIDList[i]); //产品ID
                    orderinfo.CardType = ConvertValueHelper.ConvertIntValue(CardType[i]);
                    orderinfo.Price = Convert.ToDecimal(PriceList[i]);
                    orderinfo.CardID = ConvertValueHelper.ConvertIntValue(CardID[i]);
                    //添加详情信息表
                    resultinfoid = cashBll.AddOrderinfo(orderinfo);

                    //获取参与员工
                    string[] UserIDList =
                        Request.Form.GetValues("UserID-" + orderinfo.Type + "-" + orderinfo.ProdcuitID); //1、手工

                    if (resultinfoid > 0)
                    {
                        JoinUserEntity model = new JoinUserEntity();
                        for (int j = 0; j < UserIDList.Length; j++)
                        {
                            model.UserID = ConvertValueHelper.ConvertIntValue(UserIDList[j]);
                            model.IsZhiding = ConvertValueHelper.ConvertIntValue(IsZhidingList[i]);
                            model.OrderNo = entity.OrderNo;
                            model.ProjectID = orderinfo.ProdcuitID;
                            model.Type = orderinfo.Type;
                            model.OrderInfoID = resultinfoid;
                            //消耗
                            if (!string.IsNullOrEmpty(XiaoHao[i])) //如果消耗为空
                            {
                                model.XiaoHao = ConvertValueHelper.ConvertIntValue(XiaoHao[i]); //消耗
                            }
                            else
                            {
                                if (model.Type == 1) //如果是项目则存在消耗，产品不存在消耗
                                {
                                    //获取项目的消耗, 这里应该获取实际消耗
                                    model.XiaoHao = orderinfo.Price;
                                }
                            }

                            //手工 只有项目才有手工费用
                            if (!string.IsNullOrEmpty(Ticheng[i])) //如果提成为空
                            {
                                model.Ticheng = ConvertValueHelper.ConvertIntValue(Ticheng[i]);
                            }
                            else
                            {
                                if (model.Type == 1) //如果是项目则存手工
                                {
                                    //获取项目的提成
                                    var royaltyList = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = orderinfo.ProdcuitID });
                                    decimal shougong = 0;
                                    decimal othershougong = 0;
                                    CaculateRoyalty(orderinfo.Price * orderinfo.Quantity, orderinfo.AllPrice,
                                        royaltyList, out shougong, out othershougong);
                                    model.Ticheng = shougong; //手工
                                    if (model.IsZhiding == 1) //如果指定
                                    {
                                        model.OtherTiCheng = othershougong; //额外手工
                                    }
                                }
                            }


                            //业绩 ，疗程卡卡扣没有业绩
                            //if (orderinfo.CardType != 1)
                            //{
                            model.Yeji =
                                ConvertValueHelper.ConvertDecimalValue(
                                    (orderinfo.Price / UserIDList.Length).ToString("f2")); //正常员工业绩就是项目或产品的价格
                            //}
                            cashBll.AddJoinUser(model);
                        }
                    }
                }

                #endregion

                #region ==修改房间状态和床位使用情况==

                int bedid = Convert.ToInt32(bed); //床位ID
                var shiyongbiao = cashBll.getAppointmentModel(orderno); //获取这个订单的床位使用情况
                shiyongbiao.Type = 2;
                cashBll.UpdateAppointmenttype(shiyongbiao); //修改床位使用表


                ResDocBLL.UpReservationStatu(2, shiyongbiao.ReservationID);

                #endregion

                #endregion
            }
            else //没有订单编号时表示是新的人员进入
            {
                //添加订单主表
                var result = cashBll.AddOrder(entity);
                if (result != null)
                {
                    OrderinfoEntity orderinfo = new OrderinfoEntity();
                    orderinfo.OrderNo = entity.OrderNo;
                    for (int i = 0; i < orderprice.Length; i++)
                    {
                        orderinfo.Type = ConvertValueHelper.ConvertIntValue(TypeList[i]); //类型
                        orderinfo.AllPrice = Convert.ToDecimal(orderprice[i]);
                        orderinfo.Quantity = ConvertValueHelper.ConvertIntValue(ordertimes[i]);
                        orderinfo.ProdcuitID = ConvertValueHelper.ConvertIntValue(GiveIDList[i]); //产品ID
                        orderinfo.CardType = ConvertValueHelper.ConvertIntValue(CardType[i]);
                        orderinfo.Price = orderinfo.Quantity < 0
                            ? -Convert.ToDecimal(PriceList[i])
                            : Convert.ToDecimal(PriceList[i]);
                        ;
                        orderinfo.CardID = ConvertValueHelper.ConvertIntValue(CardID[i]);
                        //orderinfo.IsZhiding= Convert.ToInt32(IsZhidingList[i]);
                        //orderinfo.Shougong = Convert.ToDecimal(Ticheng[i]);
                        //添加详情信息表
                        results = cashBll.AddOrderinfo(orderinfo);
                        //
                        //获取参与员工
                        //string[] UserIDList = Request.Form.GetValues("UserID-" + orderinfo.Type + "-" + orderinfo.ProdcuitID);//1、手工
                        string[] UserIDList =
                            Request.Form.GetValues("UserID-" + orderinfo.Type + "-" + orderinfo.CardID + "-" +
                                                   orderinfo.ProdcuitID); //1、手工
                        if (results > 0)
                        {
                            JoinUserEntity model = new JoinUserEntity();
                            for (int j = 0; j < UserIDList.Length; j++)
                            {
                                model.UserID = ConvertValueHelper.ConvertIntValue(UserIDList[j]);
                                model.IsZhiding = ConvertValueHelper.ConvertIntValue(IsZhidingList[i]);
                                model.OrderNo = entity.OrderNo;
                                model.ProjectID = orderinfo.ProdcuitID;
                                model.Type = orderinfo.Type;
                                model.OrderInfoID = results;
                                //消耗：划扣次数才会产生消耗、储值卡支付项目 产生消耗
                                //if (!string.IsNullOrEmpty(XiaoHao[i]))//如果消耗不为空
                                //{
                                //    model.XiaoHao = ConvertValueHelper.ConvertIntValue(XiaoHao[i]);//消耗
                                //    model.XiaoHao_Liaocheng = model.XiaoHao;
                                //}
                                //手工 只有项目才有手工费用
                                if (!string.IsNullOrEmpty(Ticheng[i])) //如果提成不为空
                                {
                                    model.Ticheng = ConvertValueHelper.ConvertDecimalValue(Ticheng[i]);
                                }
                                else
                                {
                                    if (model.Type == 1) //如果是项目则存手工
                                    {
                                        //获取项目的提成
                                        var royaltyList = baseinfoBLL.GetAllRoyaltyScheme(new RoyaltySchemeEntity { ProjectID = orderinfo.ProdcuitID });
                                        decimal shougong = 0;
                                        decimal othershougong = 0;
                                        CaculateRoyalty(orderinfo.Price * orderinfo.Quantity, orderinfo.AllPrice,
                                            royaltyList, out shougong, out othershougong);
                                        model.Ticheng = shougong * orderinfo.Quantity; //手工(算出来的单个手工乘以数量)
                                        if (model.IsZhiding == 1) //如果指定
                                        {
                                            model.OtherTiCheng = othershougong * orderinfo.Quantity; //额外手工
                                        }
                                    }
                                }

                                //业绩
                                //  model.Yeji = ConvertValueHelper.ConvertDecimalValue((orderinfo.Price / UserIDList.Length).ToString("f2"));//正常员工业绩就是项目或产品的价格
                                cashBll.AddJoinUser(model);
                            }
                        }
                    }
                }

                #region ==修改房间状态和床位使用情况==

                BedAppointmentEntity baentity = new BedAppointmentEntity();
                baentity.BedID = Convert.ToInt32(bed);
                baentity.OrderNo = entity.OrderNo;
                baentity.ReservationID = 0;
                baentity.Type = 2;
                baentity.UserID = entity.UserID;
                cashBll.AddAppointment(baentity);

                #endregion
            }

            //返回订单编号
            return Json(entity.OrderNo);
        }

        /// <summary>
        /// 订单支付页面---床位管理入口
        /// </summary>
        /// <returns></returns>
        public ActionResult PayOrderPageOnBed()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        /// <summary>
        /// 计算手工费  price实际总单价 allprice成交价
        /// </summary>
        /// <returns></returns>
        public void CaculateRoyalty(decimal price, decimal allprice, IList<RoyaltySchemeEntity> list,
            out decimal shougong, out decimal othershougong)
        {
            shougong = 0;
            othershougong = 0;
            try
            {
                if (list != null && list.Count > 0)
                {
                    foreach (var info in list)
                    {
                        if (info.AllPriceType == 1) //折扣率
                        {
                            if (info.StartPrice * price < allprice && allprice < info.EndPrice * price)
                            {
                                if (info.NonSpecifiedType == 1) //非指定折扣率
                                {
                                    shougong = allprice * info.NonSpecifiedPrice;
                                }
                                else
                                {
                                    shougong = info.NonSpecifiedPrice;
                                }

                                if (info.SpecifiedType == 1) //指定折扣率
                                {
                                    othershougong = allprice * info.SpecifiedPrice;
                                }
                                else
                                {
                                    othershougong = info.SpecifiedPrice;
                                }
                            }
                        }
                        else
                        {
                            //固定价格
                            if (info.StartPrice < allprice && allprice < info.EndPrice)
                            {
                                if (info.NonSpecifiedType == 1) //非指定折扣率
                                {
                                    shougong = allprice * info.NonSpecifiedPrice;
                                }
                                else
                                {
                                    shougong = info.NonSpecifiedPrice;
                                }

                                if (info.SpecifiedType == 1) //指定折扣率
                                {
                                    othershougong = allprice * info.SpecifiedPrice;
                                }
                                else
                                {
                                    othershougong = info.SpecifiedPrice;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 提单界面
        /// </summary>
        /// <returns></returns>
        public ActionResult PreOrderList(OrderEntity entity)
        {
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.Statu = (int)OrderStatu.待结算;
            entity.numPerPage = 9; //每页显示条数
            entity.RuHuiStartTime = DateTime.Now.AddYears(-10).ToString("yyyy-MM-dd hh:MM:ss");
            entity.RuHuiEndTime = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd hh:MM:ss");
            ;
            entity.OrderType = (int)OrderType.项目产品;
            if (entity.orderField + "" == "")
            {
                entity.orderField = "CreateTime";
                entity.orderDirection = "desc";
            }

            var mendian = personnelBLL.HospitalEntityByID(LoginUserEntity.HospitalID);
            string time = DateTime.Now.ToString("MMdd");
            var count = cashBll.GetTodayOrderCount(LoginUserEntity.HospitalID, DateTime.Now); //获取总数
            entity.DocumentNumber = mendian.Prefix + time + (count + 1).ToString("d3");
            decimal QianPrice = 0;
            var list = cashBll.GetAllOrder(entity, out QianPrice);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage); //添加Webdiyer.WebControls.Mvc应用
            result.TotalItemCount = entity.Rows;
            result.CurrentPageIndex = entity.pageNum;
            if (Request.IsAjaxRequest())
                return PartialView("_PreOrderList", result);
            return View(result);
        }


        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderinfoList(string OrderNo)
        {

            var list = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = OrderNo })
                .OrderByDescending(i => i.CardType).ToList();


            StringBuilder str = new StringBuilder();
            //***************************************订单详情*************/
            str.Append(
                "<table style='height: auto;width:100%; border-collapse: collapse;'  cellpadding='0' cellspacing='0'>");
            str.Append("<tr style='height:16px;'>");
            str.AppendFormat("<td style='width:30%;font-size:12px; text-align:left; '>名称</td>");
            str.AppendFormat("<td style='width:40%;font-size:12px;text-align:center;'>数量</td>");
            str.AppendFormat("<td style='width:30%;font-size:12px;text-align:center;'>金额</td>");
            str.Append("</tr>");
            foreach (var info in list)
            {
                if (info.ProdcuitName != "手续服务费")
                {
                    if (info.InfoBuyType == 6)
                    {
                        str.AppendFormat("<tr style='height:16px;'>");
                        str.AppendFormat("<td style='overflow:hidden;font-size:12px;text-align:left;font-family:宋体;' colspan='3'>{0}</td>", string.IsNullOrEmpty(info.ProdcuitName) ? info.ProdcuitName1 : info.ProdcuitName);
                        str.AppendFormat("</tr>");

                        if (info.CardBalanceID + "" != "")
                        {
                            var balancelist = info.CardBalanceID.Split(new char[] { ',' },
                                StringSplitOptions.RemoveEmptyEntries);
                            foreach (var binfo in balancelist)
                            {
                                var newid = ConvertValueHelper.ConvertIntValue(binfo);
                                if (newid == 0)
                                    continue;

                                var balanccemodel = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = newid });

                                str.AppendFormat("<tr style='height:16px;'>");
                                str.AppendFormat("<td style='overflow:hidden;font-size:12px;text-align:left;  font-family:宋体;' colspan='3'>{0}</td>", balanccemodel.ProdcuitName);
                                str.AppendFormat("</tr>");
                                str.AppendFormat("<tr style='height:16px;'>");
                                str.AppendFormat("<td style='font-size:12px;text-align:center; font-family:宋体;'>{0}</td>", "↑");
                                str.AppendFormat("<td style='font-size:12px;text-align:center; font-family:宋体;'>{0}</td>", balanccemodel.AllTiems);
                                str.AppendFormat("<td style='font-size:12px;text-align:center; font-family:宋体;'>{0}</td>", balanccemodel.AllPrice);
                                str.AppendFormat("</tr>");
                                //str.AppendFormat("<tr style='height:16px;'>");
                                //str.AppendFormat(
                                //    "<td style='overflow:hidden;font-size:12px;  font-family:宋体;'>{0}</td>",
                                //    balanccemodel.Price);
                                //    str.AppendFormat(
                                //    "<td style='font-size:12px;text-align:center; font-family:宋体;'>{0}</td>",
                                //    balanccemodel.AllTiems);
                                //    str.AppendFormat("<td style='font-size:12px;text-align:left; font-family:宋体;'>{0}</td>",
                                //    balanccemodel.AllPrice);
                                //    str.AppendFormat("</tr>");
                            }
                        }

                    }
                    else
                    {
                        if (info.CardType == 1)
                        {
                            str.AppendFormat("<tr style='height:16px;'>");
                            str.AppendFormat("<td style='overflow:hidden;font-size:12px;text-align:left;font-family:宋体;' colspan='3'>{0}</td>", string.IsNullOrEmpty(info.ProdcuitName) ? info.ProdcuitName1 : info.ProdcuitName);
                            str.AppendFormat("</tr>");

                            str.AppendFormat("<tr style='height:16px;'>");
                            str.AppendFormat("<td style='overflow:hidden;font-size:12px;text-align:left;font-family:宋体;'>划卡</td>");
                            str.AppendFormat("<td style='font-size:12px;text-align:center; font-family:宋体;' colspan='2'>{0}</td>", info.Quantity + "次/余" + info.InstantInventory + "次");
                            str.AppendFormat("</tr>");

                        }
                        else
                        {
                            str.AppendFormat("<tr style='height:16px;'>");
                            str.AppendFormat("<td style='overflow:hidden;font-size:12px;text-align:left;font-family:宋体;' colspan='3'>{0}</td>", string.IsNullOrEmpty(info.ProdcuitName) ? info.ProdcuitName1 : info.ProdcuitName);
                            str.AppendFormat("</tr>");

                            var type = "";
                            switch (info.Type)
                            {
                                case 1:
                                    type = "项目";
                                    break;
                                case 2:
                                    type = "产品";
                                    break;
                                case 3:
                                    type = "卡类";
                                    break;
                            }
                            str.AppendFormat("<tr style='height:16px;'>");
                            str.AppendFormat("<td style='overflow:hidden;font-size:12px;text-align:left;font-family:宋体;'>{0}</td>", type);
                            str.AppendFormat("<td style='font-size:12px;text-align:center;font-family:宋体;'>{0}</td>", info.Quantity);
                            str.AppendFormat("<td style='font-size:12px;text-align:center;font-family:宋体;'>{0}</td>", info.AllPrice);
                            str.AppendFormat("</tr>");

                        }
                    }
                }
            }

            str.Append("</table>");
            //////////////////////////////////////////支付方式//////////////////////////
            //获取订单支付方式
            var payList = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = OrderNo });
            StringBuilder str2 = new StringBuilder();
            str2.Append(
                "<table style='height: auto;width:100%; border-collapse: collapse;'  cellpadding='0' cellspacing='0'>");

            foreach (var info in payList)
            {
                if (info.PayType != 4)
                {
                    str2.AppendFormat("<tr style='height:16px;'>");
                    str2.AppendFormat(
                        "<td style='overflow:hidden; font-size:12px;width:60px; font-family:宋体;'>{0}</td>",
                        info.PayName);
                    str2.AppendFormat("<td style='font-size:12px;text-align:left; font-family:宋体;'>{0}",
                        info.PayMoney.ToString() + "元");
                    if (info.PayType == 3)
                        str2.AppendFormat("(余" + info.BalanceMoney.ToString() + "元)");
                    str2.Append("</td>");
                    str2.AppendFormat("</tr>");
                }
            }

            str2.Append("</table>");

            //获取参与员工
            int row = 0;
            int pagecount = 0;
            //////////////////////////////参与用户/////////////////////////////////////////
            var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderNo = OrderNo }, out row, out pagecount);
            string username = "";
            var newlist = joinList.Select(i => i.UserName).Distinct().ToList();
            foreach (var info in newlist)
            {
                username += info + ",";
            }

            if (!string.IsNullOrEmpty(username))
            {
                username = username.Substring(0, username.Length - 1);
            }

            decimal needmoney = 0; //实际支付金额
            decimal removemoney = 0; //需要移除的价钱总和(因为单据总价格的划扣价格要扣除)
            needmoney = list.Where(i => i.CardType > 1).ToList().Sum(i => i.Price * i.Quantity);
            removemoney = list.Where(i => i.CardType == 1).ToList().Sum(i => i.Price * i.Quantity);
            //获取订单详情
            var entity = cashBll.GetOrderModel(new OrderEntity { OrderNo = OrderNo });

            JsonModelEntity jon = new JsonModelEntity();
            jon.OutputHtml = str.ToString();
            jon.message = username;
            jon.OutputString = str2.ToString();
            if (entity.OrderType == 5)
            {
                entity.Price = -entity.Price;
            }

            //jon.ZhePay = needmoney - (entity.Price - removemoney);
            ///汪 修改打印折让 begin 2019-/-14
            //jon.ZhePay = list.Where(x => x.CardType > 1 && x.Quantity < 0).Sum(x => x.Price);
            jon.ZhePay = needmoney - (entity.Price - removemoney);
            //汪 end
            //合计=折让+应付
            jon.AllPrice = needmoney;
            //jon.ZhePay + list.Where(x => x.CardType > 1 && x.Quantity > 0).Sum(x => x.Price * x.Quantity);
            jon.YinPay = entity.Price - removemoney;
            jon.QianPrice = entity.QianPrice;


            return Json(jon);
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderinfoList76(string OrderNo)
        {

            var list = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = OrderNo })
                .OrderByDescending(i => i.CardType).ToList();


            StringBuilder str = new StringBuilder();
            //***************************************订单详情*************/
            str.Append(
                "<table style='height: auto;width:100%; border-collapse: collapse;'  cellpadding='0' cellspacing='0'>");
            str.Append("<tr style='height:18px;'>");
            str.AppendFormat("<td style='width:30%;font-size:16px; text-align:center; '>名称</td>");
            str.AppendFormat("<td style='width:40%;font-size:16px;text-align:center;'>数量</td>");
            str.AppendFormat("<td style='width:30%;font-size:16px;text-align:center;'>金额</td>");
            str.Append("</tr>");
            foreach (var info in list)
            {
                if (info.ProdcuitName != "手续服务费")
                {
                    if (info.InfoBuyType == 6)
                    {
                        str.AppendFormat("<tr style='height:18px;'>");
                        str.AppendFormat(
                            "<td style='overflow:hidden;font-size:18px;  font-family:宋体;'  colspan='3'>{0}</td>",
                            String.IsNullOrEmpty(info.ProdcuitName) ? info.ProdcuitName1 : info.ProdcuitName);
                        str.AppendFormat("</tr>");

                        if (info.CardBalanceID + "" != "")
                        {
                            var balancelist = info.CardBalanceID.Split(new char[] { ',' },
                                StringSplitOptions.RemoveEmptyEntries);
                            foreach (var binfo in balancelist)
                            {
                                var newid = ConvertValueHelper.ConvertIntValue(binfo);
                                if (newid == 0)
                                    continue;

                                var balanccemodel = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = newid });

                                str.AppendFormat("<tr style='height:16px;'>");
                                str.AppendFormat("<td style='overflow:hidden;font-size:12px;text-align:left;  font-family:宋体;' colspan='3'>{0}</td>", balanccemodel.ProdcuitName);
                                str.AppendFormat("</tr>");
                                str.AppendFormat("<tr style='height:16px;'>");
                                str.AppendFormat("<td style='font-size:12px;text-align:center; font-family:宋体;'>{0}</td>", "↑");
                                str.AppendFormat("<td style='font-size:12px;text-align:center; font-family:宋体;'>{0}</td>", balanccemodel.AllTiems);
                                str.AppendFormat("<td style='font-size:12px;text-align:center; font-family:宋体;'>{0}</td>", balanccemodel.AllPrice);
                                str.AppendFormat("</tr>");

                                //str.AppendFormat("<tr style='height:18px;'>");
                                //str.AppendFormat(
                                //    "<td style='overflow:hidden;font-size:16px;  font-family:宋体;'>{0}</td>",
                                //    balanccemodel.Price);
                                //str.AppendFormat(
                                //    "<td style='font-size:16px;text-align:center; font-family:宋体;'>{0}</td>",
                                //    balanccemodel.AllTiems);
                                //str.AppendFormat("<td style='font-size:16px;text-align:left; font-family:宋体;'>{0}</td>",
                                //    balanccemodel.AllPrice);
                                //str.AppendFormat("</tr>");
                            }
                        }

                    }
                    else
                    {
                        if (info.CardType == 1)
                        {
                            str.AppendFormat("<tr style='height:18px;'>");
                            str.AppendFormat("<td style='overflow:hidden;font-size:16px;text-align:left;font-family:宋体;' colspan='3'>{0}</td>", string.IsNullOrEmpty(info.ProdcuitName) ? info.ProdcuitName1 : info.ProdcuitName);
                            str.AppendFormat("</tr>");

                            str.AppendFormat("<tr style='height:18px;'>");
                            str.AppendFormat("<td style='overflow:hidden;font-size:16px; text-align:left; font-family:宋体;'>划卡</td>");
                            str.AppendFormat("<td style='font-size:16px;text-align:center; font-family:宋体;' colspan='2'>{0}</td>", info.Quantity + "次/余" + info.InstantInventory + "次");
                            str.AppendFormat("</tr>");

                        }
                        else
                        {
                            str.AppendFormat("<tr style='height:18px;'>");
                            str.AppendFormat("<td style='overflow:hidden;font-size:16px;text-align:left;font-family:宋体;' colspan='3'>{0}</td>", string.IsNullOrEmpty(info.ProdcuitName) ? info.ProdcuitName1 : info.ProdcuitName);
                            str.AppendFormat("</tr>");

                            var type = "";
                            switch (info.Type)
                            {
                                case 1:
                                    type = "项目";
                                    break;
                                case 2:
                                    type = "产品";
                                    break;
                                case 3:
                                    type = "卡类";
                                    break;
                            }
                            str.AppendFormat("<tr style='height:18px;'>");
                            str.AppendFormat("<td style='overflow:hidden;font-size:16px;text-align:center; font-family:宋体;'>{0}</td>", type);
                            str.AppendFormat("<td style='font-size:16px;text-align:center; font-family:宋体;'>{0}</td>", info.Quantity);
                            str.AppendFormat("<td style='font-size:16px;text-align:center; font-family:宋体;'>{0}</td>", info.AllPrice);
                            str.AppendFormat("</tr>");

                        }
                    }



                }
            }

            str.Append("</table>");
            //////////////////////////////////////////支付方式//////////////////////////
            //获取订单支付方式
            var payList = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = OrderNo });
            StringBuilder str2 = new StringBuilder();
            str2.Append(
                "<table style='height: auto;width:100%; border-collapse: collapse;'  cellpadding='0' cellspacing='0'>");

            foreach (var info in payList)
            {
                if (info.PayType != 4)
                {
                    str2.AppendFormat("<tr style='height:20px;'>");
                    str2.AppendFormat(
                        "<td style='overflow:hidden; font-size:16px;width:70px; font-family:宋体;'>{0}</td>",
                        GetlengthStr(info.PayName, 6));
                    str2.AppendFormat("<td style='font-size:16px;text-align:left; font-family:宋体;'>{0}",
                        info.PayMoney.ToString() + "元");
                    if (info.PayType == 3)
                        str2.AppendFormat("(余" + info.BalanceMoney.ToString() + "元)");
                    str2.Append("</td>");
                    str2.AppendFormat("</tr>");
                }
            }

            str2.Append("</table>");

            //获取参与员工
            int row = 0;
            int pagecount = 0;
            //////////////////////////////参与用户/////////////////////////////////////////
            var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderNo = OrderNo }, out row, out pagecount);
            string username = "";
            var newlist = joinList.Select(i => i.UserName).Distinct().ToList();
            foreach (var info in newlist)
            {
                username += info + ",";
            }

            if (!string.IsNullOrEmpty(username))
            {
                username = username.Substring(0, username.Length - 1);
            }

            decimal needmoney = 0; //实际支付金额
            decimal removemoney = 0; //需要移除的价钱总和(因为单据总价格的划扣价格要扣除)
            needmoney = list.Where(i => i.CardType > 1).ToList().Sum(i => i.Price * i.Quantity);
            removemoney = list.Where(i => i.CardType == 1).ToList().Sum(i => i.Price * i.Quantity);
            //获取订单详情
            var entity = cashBll.GetOrderModel(new OrderEntity { OrderNo = OrderNo });

            JsonModelEntity jon = new JsonModelEntity();
            jon.OutputHtml = str.ToString();
            jon.message = username;
            jon.OutputString = str2.ToString();
            jon.AllPrice = needmoney;
            if (entity.OrderType == 5)
            {
                entity.Price = -entity.Price;
            }

            //jon.ZhePay = needmoney - (entity.Price - removemoney);
            jon.ZhePay = list.Where(x => x.CardType > 1 && x.Quantity < 0).Sum(x => x.Price);
            jon.YinPay = entity.Price - removemoney;
            jon.QianPrice = entity.QianPrice;


            return Json(jon);
        }

        private string GetlengthStr(string str, int length)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            var len = str.Trim().Length;
            if (len > length)
            {
                return str.Trim().Substring(0, length);
            }
            else
            {
                return str.Trim();
            }

        }

        public ActionResult GetOrderName(string OrderNo)
        {
            return Content(cashBll.GetOrderModel(new OrderEntity { OrderNo = OrderNo }).Name.ToString());
        }


        public ActionResult GetAllUserByHospitalID(int HospitalID)
        {
            var persoList = personnelBLL.GetAllUserByHospitalID(HospitalID, 0);
            StringBuilder persoListstr1 = new StringBuilder();

            foreach (var info in persoList)
            {
                persoListstr1.AppendFormat(
                    "<li class='liClass' ><input code='{2}' type='checkbox' id='{0}' value='{1}' name='UserID' /><label for='{0}'>{2}</label> </li>",
                    "chk1(" + info.UserID + ")", info.UserID, info.UserName);
            }

            return Content(persoListstr1.ToString());
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderDetail(string OrderNo)
        {
            var list = cashBll.GetGuaDanOrderinfoList(new OrderinfoEntity { OrderNo = OrderNo });
            int row = 0;
            int pagecount = 0;
            foreach (var info in list)
            {
                JoinUserEntity joe = new JoinUserEntity();
                joe.OrderNo = OrderNo;
                joe.OrderInfoID = info.ID;
                var joinList = cashBll.GetAllJoinUser(joe, out row, out pagecount);
                if (joinList.Count() > 0)
                {
                    info.IsZhiding = joinList[0].IsZhiding;
                    info.Shougong = joinList.Sum(i => i.Ticheng) / joinList.Count();
                }

            }


            var persoList = personnelBLL.GetAllUserByHospitalID(LoginUserEntity.HospitalID, 0);
            //美容师
            //StringBuilder persoListstr = new StringBuilder();
            //persoListstr.AppendFormat("<ul style='position:relative;' class='selectUL'>");
            //persoListstr.AppendFormat("<li class='liMenu' ><a href='javascript:;'> 点击选择</a></li>");
            //persoListstr.AppendFormat("<ul class='liHide ulClass'>");
            //foreach (var info in persoList)
            //{
            //    persoListstr.AppendFormat("<li class='liClass' ><input  code='{2}' type='checkbox' {3}   id='{0}' value='{1}' name='UserID' /><label for='{0}'>{2}</label> </li>", "chk1(" + info.UserID + ")", info.UserID, info.UserName, joinuserlist.Where(i => i.UserID == info.UserID).Count() > 0 ? "checked='checked'" : "");
            //}
            //persoListstr.AppendFormat("</ul>");
            //persoListstr.AppendFormat("</ul>");

            //顾问
            StringBuilder persoListstr1 = new StringBuilder();
            persoListstr1.AppendFormat("<ul style='position:relative;' class='selectUL1'>");
            persoListstr1.AppendFormat("<li class='liMenu1' ><a href='javascript:;'> 点击选择</a></li>");
            persoListstr1.AppendFormat("<ul class='liHide1 ulClass'>");

            foreach (var info in persoList)
            {
                persoListstr1.AppendFormat(
                    "<li class='liClass' ><input code='{2}' type='checkbox' id='{0}' value='{1}' name='UserID' /><label for='{0}'>{2}</label> </li>",
                    "chk1(" + info.UserID + ")", info.UserID, info.UserName);
            }

            persoListstr1.AppendFormat("</ul>");
            persoListstr1.AppendFormat("</ul>");
            StringBuilder str = new StringBuilder();
            foreach (var info in list)
            {
                JoinUserEntity joe = new JoinUserEntity();
                joe.OrderNo = OrderNo;
                joe.OrderInfoID = info.ID;
                var joinList = cashBll.GetAllJoinUser(joe, out row, out pagecount);

                str.AppendFormat(
                    "<tr orderNo='{3}' class='" + (info.CardType == 1 ? "cardtype" : "cashtype") +
                    "'><td>提单</td><td><input type='hidden' name='CardID' value=" + info.CardID +
                    " /><input type='hidden' name='CardType' value=" + info.CardType +
                    " /><input type='hidden' name='TypeList' value='" + info.Type +
                    "' /><input type='hidden' name='GiveIDList' value={0} /><input type='hidden' name='GiveIDString' value={2} />{1}</td>",
                    info.ProdcuitID, String.IsNullOrEmpty(info.ProdcuitName1) ? info.ProdcuitName : info.ProdcuitName1,
                    info.Type + "-" + info.ProdcuitID, info.OrderNo);
                str.AppendFormat(
                    "<td><input type='hidden' name='PriceList' value={1} ><label class='productmoney'>{0}</label></td>",
                    info.Price, info.Price);
                str.AppendFormat(
                    "<td><input type='text' class='smallinput zhekou'    onkeyup='CheckInputIntFloat0and1(this)' name='zhekou' value='1'></td>");
                //"<td><input type='text' class='smallinput zhekou'    onkeyup='CheckInputIntFloat0and1(this)' name='zhekou' value={0}></td>",
                //Convert.ToDecimal(info.AllPrice/(info.Price * info.Quantity)));
                str.AppendFormat(
                    "<td><input type='text' onkeyup='javascript:CheckInputIntFloat(this);' class='smallinput times' name='ordertimes' value={0}></td>",
                    info.Quantity);
                str.AppendFormat(
                    "<td><input type='text'  onkeyup='javascript:CheckInputIntFloat(this);' class='smallinput alltimes' name='orderprice'  value={0}></td>",
                    info.AllPrice);
                str.AppendFormat("<td style='width:80px;'>{0}</td>", GetPersonList(joinList)); //美容师下拉框
                //str.AppendFormat("<td style='width:80px; display:none;' class='isshowth'   >{0}</td>", persoListstr1.ToString());//顾问下拉框
                str.AppendFormat(
                    "<td class='isshowth' style='display:none;' ><select autocomplete='off' name='IsZhiding'><option {0} value='0'>否</option><option {1} value='1'>是</option></select></td>",
                    info.IsZhiding == 0 ? "selected='selected'" : " ",
                    info.IsZhiding == 1 ? "selected='selected'" : " ");
                //str.AppendFormat("<td class='isshowth'  style='display:none;'><input type='text'  name='XiaoHao' class='smallinput'></td>");
                str.AppendFormat(
                    "<td class='isshowth'  style='display:none;'><input type='text'  name='Ticheng' value='{0}' class='smallinput'></td>",
                    info.Shougong);
                str.AppendFormat("<td><a href='javascript:;' class='deletebtn'>删除</a></td></tr>");

            }

            return Content(str.ToString());
        }


        /// <summary>
        /// 获取参与员工--员工业绩审核页面，新增修改参与员工
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <returns></returns>
        public ActionResult UpdatejoinUserYeji(string OrderNo)
        {
            var list = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = OrderNo });
            int row = 0;
            int pagecount = 0;

            var persoList = personnelBLL.GetAllUserByHospitalID(LoginUserEntity.HospitalID, 0);
            StringBuilder str = new StringBuilder();
            foreach (var info in list)
            {
                JoinUserEntity joe = new JoinUserEntity();
                joe.OrderNo = OrderNo;
                joe.OrderInfoID = info.ID;
                var joinList = cashBll.GetAllJoinUser(joe, out row, out pagecount);
                str.AppendFormat("<tr ><td><input type='hidden' name='ProjectID' value={0} />{1}</td>", info.ProdcuitID,
                    String.IsNullOrEmpty(info.ProdcuitName1) ? info.ProdcuitName : info.ProdcuitName1);
                str.AppendFormat("<td><input type='hidden' name='Type' value={1} />{0}</td>", info.Price, info.Type);
                str.AppendFormat("<td><input type='hidden' name='OrderInfoID' value={1} />{0}</td>", info.Quantity,
                    info.ID);
                str.AppendFormat(
                    "<td><input type='hidden' name='UserID'  /><input type='hidden' name='CardID' value={1} />{0}</td>",
                    info.AllPrice, info.CardID);
                str.AppendFormat("<td style='width:80px;'>{0}</td>", GetPersonList(joinList)); //美容师下拉框
                str.AppendFormat("</tr>");

            }

            return View(str);
        }

        /// <summary>
        /// 添加修改业绩
        /// </summary>
        /// <returns></returns>
        public ActionResult AddUpdateYeji(string OrderNo)
        {
            int resut = 0;
            try
            {
                string[] ProjectIDList = Request.Form.GetValues("ProjectID");
                string[] TypeList = Request.Form.GetValues("Type");
                string[] OrderInfoIDList = Request.Form.GetValues("OrderInfoID");
                string[] CardIDList = Request.Form.GetValues("CardID");
                //获取之前的参与员工，如果一样就跳过，否则判断是修改用户还是增加用户，1：添加新的，2修改为其他的，3删除再加新的
                var list = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = OrderNo });
                int row = 0;
                int pagecount = 0;


                foreach (var info in list)
                {
                    JoinUserEntity joe = new JoinUserEntity();
                    joe.OrderNo = OrderNo;
                    joe.OrderInfoID = info.ID;
                    var joinList = cashBll.GetAllJoinUser(joe, out row, out pagecount);
                    var persoList = personnelBLL.GetAllUserByHospitalID(LoginUserEntity.HospitalID, 0);

                    //存储当前已有UserID
                    List<int> useridlist = new List<int>();
                    foreach (var model in joinList)
                    {

                        useridlist.Add(model.UserID);

                    }

                    //和现有UserID进行比较  如果一样就跳过，否则判断是修改用户还是增加用户，1：添加新的，2修改为其他的，3删除再加新的
                    for (int i = 0; i < OrderInfoIDList.Length; i++)
                    {
                        List<int> newuseridlist = new List<int>();
                        if (info.ID == ConvertValueHelper.ConvertIntValue(OrderInfoIDList[i]))
                        {
                            string[] UserIDList =
                                Request.Form.GetValues("UserID-" + TypeList[i] + "-" + CardIDList[i] + "-" +
                                                       ProjectIDList[i]);
                            foreach (var useridstr in UserIDList)
                            {
                                if (!string.IsNullOrEmpty(useridstr))
                                {
                                    newuseridlist.Add(ConvertValueHelper.ConvertIntValue(useridstr));
                                }
                            }

                            //比较useridlist和newuseridlist
                            if (useridlist.Count == newuseridlist.Count &&
                                useridlist.Except(newuseridlist).Count() == 0 &&
                                newuseridlist.Except(useridlist).Count() == 0)
                            {
                                //集合相等
                                break;
                            }
                            else
                            {

                                var resutlist = useridlist.Except(newuseridlist).ToList(); //差集
                                //删除差集
                                foreach (var duouserid in resutlist)
                                {
                                    cashBll.DelJoinUserbyOrderInfo(OrderNo,
                                        ConvertValueHelper.ConvertIntValue(OrderInfoIDList[i]), duouserid);
                                }

                                var resutlist1 = newuseridlist.Except(useridlist).ToList(); //差集
                                //添加差集
                                foreach (var duouserid in resutlist1)
                                {
                                    resut = cashBll.AddJoinUserbyOrderInfo(new JoinUserEntity
                                    {
                                        ProjectID = ConvertValueHelper.ConvertIntValue(ProjectIDList[i]),
                                        Type = ConvertValueHelper.ConvertIntValue(TypeList[i]),
                                        IsZhiding = 0,
                                        OrderNo = OrderNo,
                                        OrderInfoID = ConvertValueHelper.ConvertIntValue(OrderInfoIDList[i]),
                                        UserID = duouserid
                                    });
                                }
                            }
                        }

                    }

                }
            }
            catch (Exception e)
            {
                LogWriter.WriteInfo(e.Message, "修改员工业绩错误");
                resut = -1;
            }

            return Json(resut);
        }

        /// <summary>
        /// 审核还款单业绩
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <returns></returns>
        public ActionResult UpdatejoinUserHuankuanYeji()
        {
            string OrderNo = Request["OrderNo"];
            var list = cashBll.GetStaffPerformanceList(OrderNo);
            list = list.OrderBy(i => i.UserID).ToList();
            var result = list.AsQueryable().ToPagedList(1, 10000);
            return View(result);
        }

        /// <summary>
        /// 审核还款单业绩
        /// </summary>
        /// <returns></returns>
        public ActionResult ShenheYeji(string OrderNo)
        {
            int resut = 0;
            string[] IDList = Request.Form.GetValues("ID");
            string[] Yeji_Chuzhika = Request.Form.GetValues("Yeji_Chuzhika");
            string[] XianJin = Request.Form.GetValues("XianJin");
            string[] KaKou = Request.Form.GetValues("KaKou");
            string[] Yeji_Liaochengka = Request.Form.GetValues("Yeji_Liaochengka");
            string[] Yeji_Chanpin = Request.Form.GetValues("Yeji_Chanpin");
            string[] Yeji_TeshuChanpin = Request.Form.GetValues("Yeji_TeshuChanpin");
            string[] Yeji_Xiangmu = Request.Form.GetValues("Yeji_Xiangmu");
            string[] Yeji_Daxiangmu = Request.Form.GetValues("Yeji_Daxiangmu");
            string[] Yeji_Teshuxiangmu = Request.Form.GetValues("Yeji_Teshuxiangmu");
            string[] KakouYeji_Liaocheng = Request.Form.GetValues("KakouYeji_Liaocheng");
            string[] KakouYeji_Chanpin = Request.Form.GetValues("KakouYeji_Chanpin");
            string[] KakouYeji_TeshuChanpin = Request.Form.GetValues("KakouYeji_TeshuChanpin");
            string[] KakouYeji_Xiangmu = Request.Form.GetValues("KakouYeji_Xiangmu");
            string[] KakouYeji_Daxiangmu = Request.Form.GetValues("KakouYeji_Daxiangmu");
            string[] KakouYeji_Teshuxiangmu = Request.Form.GetValues("KakouYeji_Teshuxiangmu");
            string[] Reward = Request.Form.GetValues("Reward");
            for (int i = 0; i < IDList.Length; i++)
            {
                var entity = cashBll.GetJoinUserModel(new JoinUserEntity { ID = Convert.ToInt32(IDList[i]) });
                entity.ID = ConvertValueHelper.ConvertIntValue(IDList[i]);
                entity.Yeji = ConvertValueHelper.ConvertDecimalValue(XianJin[i]);
                entity.KakouYeji = ConvertValueHelper.ConvertDecimalValue(KaKou[i]);
                entity.Yeji_Chuzhika = ConvertValueHelper.ConvertDecimalValue(Yeji_Chuzhika[i]);
                entity.Yeji_Liaochengka = ConvertValueHelper.ConvertDecimalValue(Yeji_Liaochengka[i]);
                entity.Yeji_Chanpin = ConvertValueHelper.ConvertDecimalValue(Yeji_Chanpin[i]);
                entity.Yeji_TeshuChanpin = ConvertValueHelper.ConvertDecimalValue(Yeji_TeshuChanpin[i]);
                entity.Yeji_Xiangmu = ConvertValueHelper.ConvertDecimalValue(Yeji_Xiangmu[i]);
                entity.Yeji_Daxiangmu = ConvertValueHelper.ConvertDecimalValue(Yeji_Daxiangmu[i]);
                entity.Yeji_Teshuxiangmu = ConvertValueHelper.ConvertDecimalValue(Yeji_Teshuxiangmu[i]);
                entity.KakouYeji_Liaocheng = ConvertValueHelper.ConvertDecimalValue(KakouYeji_Liaocheng[i]);
                entity.KakouYeji_Chanpin = ConvertValueHelper.ConvertDecimalValue(KakouYeji_Chanpin[i]);
                entity.KakouYeji_TeshuChanpin = ConvertValueHelper.ConvertDecimalValue(KakouYeji_TeshuChanpin[i]);
                entity.KakouYeji_Xiangmu = ConvertValueHelper.ConvertDecimalValue(KakouYeji_Xiangmu[i]);
                entity.KakouYeji_Daxiangmu = ConvertValueHelper.ConvertDecimalValue(KakouYeji_Daxiangmu[i]);
                entity.KakouYeji_Teshuxiangmu = ConvertValueHelper.ConvertDecimalValue(KakouYeji_Teshuxiangmu[i]);
                entity.OtherTiCheng = ConvertValueHelper.ConvertDecimalValue(Reward[i]);
                resut = cashBll.UpdateJoinUser(entity);
            }

            cashBll.UpdateOrderISAudit(OrderNo); //改修状态
            return Json(resut);
        }

        /// <summary>
        /// 付款操作
        /// </summary>
        /// <returns></returns>
        public ActionResult PayOrderPage()
        {
            string OrderNo = Request["orderNo"];
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.Username = LoginUserEntity.UserName;
            decimal Proportion = 0;
            var entity = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "IsOpenAutoPrint");
            int count = 0;
            int quantity = 0;
            //if (LoginUserEntity.HospitalID == 1017)
            if (LoginUserEntity.HospitalID == 1017 || LoginUserEntity.HospitalID == 2)
                {

                var list1 = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = OrderNo });
                var list3 = list1.Where(t => t.CardType == 2).ToList();
                count = list3.Count;
                if (list1.Count > 0)
                {
                    foreach (var item in list1)
                    {
                        if (item.CardType == 1)
                        {
                        }
                        else
                        {
                            var list = cashBll.GetOrderinfoList3(new OrderinfoEntity { OrderNo = OrderNo });
                            Proportion = list.Sum(t => t.Proportion * t.Quantity);
                        }
                    }

                }
            }
            // var list = cashBll.GetOrderinfoList2(new OrderinfoEntity { OrderNo = OrderNo});
            //  ViewBag.Proportion = list.Sum(t=>t.Proportion);

            var intrgral = IpatBLL.GetIntegralModel(Convert.ToInt32(Request.QueryString["UserID"])); //获取原来的积分列表
            //if (LoginUserEntity.HospitalID == 1017)
            if (LoginUserEntity.HospitalID == 1017 || LoginUserEntity.HospitalID == 2)
            {
                if (intrgral.Integral >= Proportion)
                {
                    ViewBag.Proportion = Proportion;
                    ViewBag.count = 3;
                }
                else
                {
                    ViewBag.Proportion = intrgral.Integral;
                    ViewBag.count = count;
                    if (count > 1)
                    {
                        ViewBag.count = 2;
                    }
                }
            }

            ViewBag.intrgral = intrgral.Integral;

            ViewBag.IsOpenAutoPrint = entity.AttributeValue;

            return View();
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteOrder(int Statu, string OrderNo)
        {
            var Model = cashBll.GetOrderModel(new OrderEntity { OrderNo = OrderNo });
            if (Model.Statu == 2)
            {
                int result = cashBll.DeleteOrder(OrderNo);
                return Json(result);
            }
            else
            {
                return Json(0);
            }
        }

        ///// <summary>
        ///// 添加或者修改员工进度
        ///// 现金业绩根据支付现金和银联卡的金额去统计
        ///// 实耗业绩直接抓取产品项目的订单详情
        ///// 1.先获取员工的当前目标(已有),再获取员工今日的进度信息.然后修改信息
        ///// </summary>
        ///// <param name="OrderNo"></param>
        ///// <returns></returns>
        //public int AddOrUpdateGoalSchedule(string OrderNo,PerformanceGoalsEntity Goals)
        //{

        //    PaymentOrderEntity entity = new PaymentOrderEntity();
        //    entity.OrderNo=OrderNo;
        //    var list = cashBll.GetAllPaymentOrder(entity);
        //    foreach (var info in list)
        //    {
        //        //如果支付类别时现金和银联卡的,直接算现金
        //        if (info.PayType == 1 || info.PayType == 2)
        //        {
        //            AddGoalSchedule(Goals.ID, 1, Goals.AccumulativeXianJin, info.PayMoney, Goals.Xianjin);
        //        }
        //    }

        //    OrderinfoEntity oi = new OrderinfoEntity();
        //    oi.OrderNo = OrderNo;
        //    var oinfolist=cashBll.GetOrderinfoList(oi);
        //    foreach (var oinfo in oinfolist)
        //    {
        //        if (oinfo.Type == 1 || oinfo.Type == 2)
        //        {
        //            AddGoalSchedule(Goals.ID, 2, Goals.AccumulativeShiHao, oinfo.Price, Goals.ShiHao);
        //        }
        //    }

        //    return 1;

        //}

        ///// <summary>
        ///// 添加(修改)目标进度
        ///// </summary>
        ///// <param name="_GoalID">目标ID</param>
        ///// <param name="_CategoryType">目标类别</param>
        ///// <param name="_CategoryTypeNumber">当前类别累计完成</param>
        ///// <param name="Number">添加数量</param>
        ///// <param name="AllGoalNumber">当前类别总目标</param>
        ///// <returns>如果添加的话返回的是状态表的ID,修改返回的是状态</returns>
        //public int AddGoalSchedule(int _GoalID, int _CategoryType, decimal _CategoryTypeNumber, decimal Number, decimal AllGoalNumber)
        //{
        //    var result = 0;
        //    int row;
        //    int pagecount;
        //    GoalScheduleEntity Gse = new GoalScheduleEntity();
        //    Gse.GoalID = _GoalID;
        //    Gse.Months=DateTime.Now.Month.ToString().PadLeft(2,'0');
        //    Gse.Days = DateTime.Now.Day.ToString().PadLeft(2, '0');
        //    Gse.CategoryType = _CategoryType;
        //    var list = iCentBLL.GetGoalScheduleList(Gse, out row, out pagecount);//通过目标ID,月天,类别,查找符合条件的要求
        //    if (list.Count == 0)//如果没有,则添加一项.
        //    {
        //        Gse.CompletedNumber =_CategoryTypeNumber+Number;
        //        Gse.TodayNumber = Number;
        //        Gse.LevelOfCompletion = String.Format("{0:F}", (Number / AllGoalNumber) * 100) + "%";//求出比例
        //        result = iCentBLL.AddGoalSchedule(Gse); //添加数据
        //    }
        //    else
        //    {
        //        list[0].CompletedNumber += Number;
        //        list[0].TodayNumber += Number;
        //        result=iCentBLL.UpdateGoalSchedule(list[0]);
        //    }
        //    return result;
        //}


        ///// <summary>
        ///// 通过用户ID找到次用户的目标信息,如果找不到,返回的是null
        ///// </summary>
        ///// <param name="UserID">用户ID</param>
        ///// <param name="GoalsType">1.员工 2.公司</param>
        ///// <returns></returns>
        //public PerformanceGoalsEntity FindGoals(int UserID,int GoalsType)
        //{
        //    //传入当前用户ID,当前时间的年月和门店id
        //    PerformanceGoalsEntity entity = new PerformanceGoalsEntity();
        //    entity.UserID = UserID;
        //    entity.Years = DateTime.Now.Year;
        //    entity.Months = DateTime.Now.Month.ToString().PadLeft(2,'0');
        //    entity.HospitalID = loginUserEntity.HospitalID;
        //    entity.GoalsType = GoalsType;
        //    entity.pageNum = 1; entity.numPerPage = 500;
        //    int row = 0; int pagecount = 0;
        //    var list = iCentBLL.GetPerformanceGoalsList(entity, out row, out pagecount);
        //    if (list.Count > 0)
        //    {
        //        return list[0];
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}





        /// <summary>
        /// 修改订单状态、支付金额---支付操作(床位入口)
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateOrderOnBed(OrderEntity entity, string CardIDstr, string ordertimesstr,
            string cardidarr, string txtChuzhikaarr, string OtherPayment, string OtherPaymentValue, string OldOrderNo)
        {
            entity.Statu = (int)OrderStatu.已结算;
            entity.ActurePrice = entity.Price + entity.QianPrice;
            //修改订单为已支付
            int result = cashBll.UpdateOrder(entity);
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            if (result > 0)
            {
                //添加订单支付方式

                //其他支付方式
                if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                {
                    string[] strpay = OtherPayment.Split(',');
                    string[] OtherPaymentValuelist = OtherPaymentValue.Split(',');
                    for (int i = 0; i < strpay.Length; i++)
                    {
                        cashBll.AddPaymentOrder(new PaymentOrderEntity
                        {
                            OrderNo = entity.OrderNo,
                            PayType = ConvertValueHelper.ConvertIntValue(strpay[i]),
                            PayMoney = ConvertValueHelper.ConvertIntValue(OtherPaymentValuelist[i])
                        });
                    }
                }

                if (entity.Xianjin > 0) //现金
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.现金, PayMoney = entity.Xianjin });
                }

                if (entity.Yinlianka > 0) //银联卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.银联卡, PayMoney = entity.Yinlianka });
                }

                if (entity.Chuzhika > 0) //储值卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.储值卡, PayMoney = entity.Chuzhika });
                    //扣除卡类金额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        string[] ordertimesstrlist = txtChuzhikaarr.Split(',');
                        string[] caridlist = cardidarr.Split(',');
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            dic.Add(ConvertValueHelper.ConvertIntValue(caridlist[f]),
                                ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]));
                            result = cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity
                            {
                                ID = ConvertValueHelper.ConvertIntValue(caridlist[f]),
                                Price = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f])
                            });

                            //用户储值卡余额明细日志
                            var mainCardBalanceDetail = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity() { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]) });
                            cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                            {
                                CardBalanceId = mainCardBalanceDetail.ID,
                                OrderNo = entity.OrderNo,
                                Type = (int)MainCardBalanceDetailType.消费,
                                Amount = -ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]),
                                Balance = mainCardBalanceDetail.Price,
                                OldAmount = mainCardBalanceDetail.Price + ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]),
                                CreateTime = DateTime.Now
                            });
                        }
                    }
                }

                if (entity.Huakouka > 0) //划扣次卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity
                    {
                        OrderNo = entity.OrderNo,
                        PayType = (int)PaymentEnum.划扣次数卡,
                        PayMoney = entity.Huakouka,
                        ids = CardIDstr
                    });
                    //减去相应的卡类次数
                    if (!String.IsNullOrEmpty(CardIDstr))
                    {
                        string[] caridlist = CardIDstr.Split(',');
                        string[] ordertimesstrlist = ordertimesstr.Split(',');
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            //修改用户余额记录,通常一个客户只能买一个卡类型，多个可充值充值,所以根据CardID修改，没有根据ID
                            cashBll.UpdateMainCardBalanceTime(new MainCardBalanceEntity
                            {
                                CardID = ConvertValueHelper.ConvertIntValue(caridlist[f]),
                                Tiems = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f])
                            });
                        }
                    }
                }
            }

            //如果存在欠款，修改用户欠款情况
            if (entity.QianPrice != 0)
            {
                cashBll.UpdateArrearsMoney(entity.UserID, entity.QianPrice);
            }

            //订单产品支付方式表
            //现金类：现金支付部分优先产品，产品价格由大到小进行支付结算
            //卡扣类：优先产品
            //根据订单编号查询订单产品表
            var orderInfoList = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = entity.OrderNo });
            //剩余现金部分
            decimal ShengyuCash = entity.Xianjin;
            //已支付现金金额
            decimal PayCash = 0;
            //剩余银联卡部分
            decimal YinLianCard = entity.Yinlianka;
            //已支付银联卡部分
            decimal PayCarded = 0;
            //剩余储值卡
            decimal Chuzhika = entity.Chuzhika;
            //已支付银联卡部分
            decimal ChuzhikaPayed = 0;
            //储值卡集合
            List<int> cardlist = new List<int>();
            cardlist.AddRange(dic.Keys);
            foreach (var info in orderInfoList)
            {
                int row, pagecount;
                var joinList =
                    cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                //新的产品或者项目
                PayCash = 0;
                PayCarded = 0;
                ChuzhikaPayed = 0;
                PaymentOrderProductEntity conditon = new PaymentOrderProductEntity();
                conditon.AllPrice = info.AllPrice; //订单产品总价
                conditon.OrderInfoID = info.ID; //支付订单产品
                if (info.Type == 2) //产品
                {
                    #region==产品支付==

                    #region==现金支付==

                    if (ShengyuCash > 0) //现金
                    {
                        conditon.PayType = (int)PaymentEnum.现金;
                        if (ShengyuCash >= info.AllPrice) //如果现金部分超出实际产品价格
                        {
                            conditon.PayMoney = info.AllPrice;
                            ShengyuCash = ShengyuCash - conditon.PayMoney;
                            PayCash = conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue; //跳出循环
                        }
                        else
                        {
                            //现金部分不够
                            conditon.PayMoney = ShengyuCash;
                            PayCash = ShengyuCash;
                            ShengyuCash = 0;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                        }
                    } //现金支付

                    #endregion

                    #region==银联卡支付==

                    if (YinLianCard > 0) //银联卡
                    {
                        conditon.PayType = (int)PaymentEnum.银联卡;
                        if (YinLianCard >= info.AllPrice) //如果银联卡部分超出实际产品价格
                        {
                            if (PayCash > 0) //支付了部分现金
                            {
                                conditon.PayMoney = info.AllPrice - PayCash;
                                YinLianCard = YinLianCard - conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                            else
                            {
                                conditon.PayMoney = info.AllPrice;
                                YinLianCard = YinLianCard - conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                        }
                        else
                        {
                            //银联卡部分不够
                            if (PayCash > 0) //支付了部分现金
                            {
                                if (PayCash + YinLianCard >= info.AllPrice)
                                {
                                    conditon.PayMoney = info.AllPrice - PayCash;
                                    YinLianCard = YinLianCard - conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue; //跳出循环
                                }
                                else
                                {
                                    // conditon.PayMoney = YinLianCard + PayCash;
                                    conditon.PayMoney = YinLianCard;
                                    YinLianCard = 0;
                                    PayCarded = conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                }

                            }
                            else
                            {
                                conditon.PayMoney = YinLianCard;
                                YinLianCard = 0;
                                PayCarded = conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);

                            }
                        }

                    } //银联卡支付

                    #endregion

                    #region==储值卡支付==

                    //储值卡支付  Chuzhika=储值卡总额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {

                        foreach (var valu in cardlist)
                        {
                            var manCardid = valu; //用户余额记录ID
                            var manPrice = dic[valu]; //支付金额
                            if (manPrice > 0)
                            {
                                conditon.PayType = (int)PaymentEnum.储值卡;
                                if (manPrice >= info.AllPrice) //如果储值卡部分超出实际产品价格
                                {
                                    if (PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - conditon.PayMoney); //储值卡剩余金额

                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //终止循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - conditon.PayMoney); //储值卡剩余金额
                                        conditon.CardID = manCardid;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                }
                                else
                                {
                                    //储值卡部分不够
                                    if (PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {
                                        if (PayCash + PayCarded + manPrice >= info.AllPrice)
                                        {
                                            conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                            // Chuzhika = Chuzhika - conditon.PayMoney;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, manPrice - conditon.PayMoney); //储值卡剩余金额
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            break; //跳出循环
                                        }
                                        else
                                        {
                                            conditon.PayMoney = manPrice;
                                            //  dic.Remove(manCardid);
                                            ChuzhikaPayed = conditon.PayMoney;
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, (decimal)0.0);
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        conditon.PayMoney = manPrice;
                                        //Chuzhika = 0;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, (decimal)0.0);
                                        ChuzhikaPayed = conditon.PayMoney;
                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region==其他支付===

                    //其他支付方式
                    if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                    {
                        string[] strpay = OtherPayment.Split(',');
                        string[] OtherPaymentValuelist = OtherPaymentValue.Split(',');
                        for (int i = 0; i < strpay.Length; i++)
                        {
                            var nowprice = ConvertValueHelper.ConvertDecimalValue(OtherPaymentValuelist[i]);
                            if (nowprice > 0)
                            {
                                conditon.PayType = ConvertValueHelper.ConvertIntValue(strpay[i]);
                                if (nowprice > info.AllPrice) //如果银联卡部分超出实际产品价格
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联和储值卡
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                        nowprice = nowprice - conditon.PayMoney;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue; //跳出循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice;
                                        nowprice = nowprice - conditon.PayMoney;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue; //跳出循环
                                    }
                                }
                                else
                                {
                                    //银联卡部分不够
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联
                                    {
                                        if (PayCash + PayCarded + ChuzhikaPayed + nowprice > info.AllPrice)
                                        {
                                            conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                            nowprice = nowprice - conditon.PayMoney;
                                            //添加产品支付方式
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            continue; //跳出循环
                                        }
                                        else
                                        {
                                            conditon.PayMoney = nowprice;
                                            nowprice = 0;
                                            nowprice = conditon.PayMoney;
                                            //添加产品支付方式
                                            cashBll.AddPaymentOrderProduct(conditon);
                                        }

                                    }
                                    else
                                    {
                                        conditon.PayMoney = nowprice;
                                        nowprice = 0;
                                        nowprice = conditon.PayMoney;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);

                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #endregion
                }
                else
                {
                    //支付项目

                    #region==项目支付==

                    #region==划扣次卡==

                    if (info.CardID != 0) //划扣次卡
                    {
                        conditon.PayType = (int)PaymentEnum.划扣次数卡;
                        conditon.PayMoney = info.AllPrice;
                        conditon.CardID = info.CardID;
                        //添加产品支付方式
                        cashBll.AddPaymentOrderProduct(conditon);

                        #region ===划扣次数计算消耗===

                        //获取订单参与人数数量

                        foreach (var cond in joinList)
                        {
                            if (cond.XiaoHao == (decimal)0.00)
                            {
                                cond.XiaoHao = info.AllPrice;
                            }

                            //修改消耗
                            cashBll.UpdateJoinUserForPay(cond);
                        }

                        #endregion

                        continue;
                    } //划扣次卡

                    #endregion

                    #region==现金支付

                    if (ShengyuCash > 0) //现金
                    {
                        conditon.PayType = (int)PaymentEnum.现金;
                        if (ShengyuCash >= info.AllPrice) //如果现金部分超出实际产品价格
                        {
                            conditon.PayMoney = info.AllPrice;
                            ShengyuCash = ShengyuCash - conditon.PayMoney;
                            PayCash = conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue; //跳出循环
                        }
                        else
                        {
                            //现金部分不够
                            conditon.PayMoney = ShengyuCash;
                            PayCash = ShengyuCash;
                            ShengyuCash = 0;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                        }
                    } //现金支付

                    #endregion

                    #region==银联卡支付==

                    if (YinLianCard > 0) //银联卡
                    {
                        conditon.PayType = (int)PaymentEnum.银联卡;
                        if (YinLianCard >= info.AllPrice) //如果银联卡部分超出实际产品价格
                        {
                            if (PayCash > 0) //支付了部分现金
                            {
                                conditon.PayMoney = info.AllPrice - PayCash;
                                YinLianCard = YinLianCard - conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                            else
                            {
                                conditon.PayMoney = info.AllPrice;
                                YinLianCard = YinLianCard - conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue; //跳出循环
                            }
                        }
                        else
                        {
                            //银联卡部分不够
                            if (PayCash > 0) //支付了部分现金
                            {
                                if (PayCash + YinLianCard >= info.AllPrice)
                                {
                                    conditon.PayMoney = info.AllPrice - PayCash;
                                    YinLianCard = YinLianCard - conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue; //跳出循环
                                }
                                else
                                {
                                    conditon.PayMoney = YinLianCard + PayCash;
                                    YinLianCard = 0;
                                    PayCarded = conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                }

                            }
                            else
                            {
                                conditon.PayMoney = YinLianCard;
                                YinLianCard = 0;
                                PayCarded = conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);

                            }
                        }

                    } //银联卡支付

                    #endregion

                    #region==储值卡支付==

                    //储值卡支付  Chuzhika=储值卡总额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        #region ===划扣次数计算消耗===

                        //获取订单参与人数数量

                        foreach (var cond in joinList)
                        {
                            if (cond.XiaoHao == (decimal)0.00)
                            {
                                cond.XiaoHao = info.AllPrice;
                            }

                            //修改消耗
                            cashBll.UpdateJoinUserForPay(cond);
                        }

                        #endregion

                        foreach (var valu in cardlist)
                        {
                            var manCardid = valu; //用户余额记录ID
                            var manPrice = dic[valu]; //支付金额
                            if (manPrice > 0)
                            {
                                conditon.PayType = (int)PaymentEnum.储值卡;
                                if (manPrice >= info.AllPrice) //如果储值卡部分超出实际产品价格
                                {
                                    if (PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - conditon.PayMoney); //储值卡剩余金额

                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //终止循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - conditon.PayMoney); //储值卡剩余金额
                                        conditon.CardID = manCardid;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break; //跳出循环
                                    }
                                }
                                else
                                {
                                    //储值卡部分不够
                                    if (PayCash + PayCarded > 0) //支付了部分现金和银联
                                    {
                                        if (PayCash + PayCarded + manPrice >= info.AllPrice)
                                        {
                                            conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                            // Chuzhika = Chuzhika - conditon.PayMoney;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, manPrice - conditon.PayMoney); //储值卡剩余金额
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            break; //跳出循环
                                        }
                                        else
                                        {
                                            conditon.PayMoney = manPrice;
                                            //  dic.Remove(manCardid);
                                            ChuzhikaPayed = conditon.PayMoney;
                                            //添加产品支付方式
                                            conditon.CardID = manCardid;
                                            dic.Remove(manCardid);
                                            dic.Add(manCardid, (decimal)0.0);
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        conditon.PayMoney = manPrice;
                                        //Chuzhika = 0;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, (decimal)0.0);
                                        ChuzhikaPayed = conditon.PayMoney;
                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region==其他支付方式==

                    //其他支付方式
                    if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                    {
                        string[] strpay = OtherPayment.Split(',');
                        string[] OtherPaymentValuelist = OtherPaymentValue.Split(',');
                        for (int i = 0; i < strpay.Length; i++)
                        {
                            var nowprice = ConvertValueHelper.ConvertDecimalValue(OtherPaymentValuelist[i]);
                            if (nowprice > 0)
                            {
                                conditon.PayType = ConvertValueHelper.ConvertIntValue(strpay[i]);
                                if (nowprice >= info.AllPrice) //如果银联卡部分超出实际产品价格
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联和储值卡
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                        nowprice = nowprice - conditon.PayMoney;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue; //跳出循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = nowprice;
                                        nowprice = nowprice - conditon.PayMoney;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue; //跳出循环
                                    }
                                }
                                else
                                {
                                    //银联卡部分不够
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0) //支付了部分现金和银联
                                    {
                                        if (PayCash + PayCarded + ChuzhikaPayed + nowprice > info.AllPrice)
                                        {
                                            conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                            nowprice = nowprice - conditon.PayMoney;
                                            //添加产品支付方式
                                            cashBll.AddPaymentOrderProduct(conditon);
                                            continue; //跳出循环
                                        }
                                        else
                                        {
                                            conditon.PayMoney = nowprice;
                                            //  conditon.PayMoney = nowprice + PayCash + PayCarded + ChuzhikaPayed;
                                            nowprice = 0;
                                            nowprice = conditon.PayMoney;
                                            //添加产品支付方式
                                            cashBll.AddPaymentOrderProduct(conditon);
                                        }

                                    }
                                    else
                                    {
                                        conditon.PayMoney = nowprice;
                                        nowprice = 0;
                                        nowprice = conditon.PayMoney;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);

                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #endregion
                }
            }

            #region==计算员工业绩==

            //重新计算员工业绩
            foreach (var info in orderInfoList)
            {
                int row, pagecount;
                var joinList =
                    cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                foreach (var cond in joinList)
                {
                    //卡扣业绩
                    var payproduct = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 3 });
                    decimal paymoney = 0;
                    foreach (var m in payproduct)
                    {
                        paymoney += m.PayMoney * m.HuaKouZheLv;
                    }

                    //现金业绩
                    var payproduct1 = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 1 });
                    decimal paymoney1 = 0;
                    foreach (var m in payproduct1)
                    {
                        paymoney1 += m.PayMoney * m.HuaKouZheLv;
                    }

                    //修改业绩
                    cond.KakouYeji =
                        ConvertValueHelper.ConvertDecimalValue((paymoney / joinList.Count).ToString("f2")); //卡扣业绩
                    cond.Yeji = ConvertValueHelper.ConvertDecimalValue(
                        (paymoney1 / joinList.Count).ToString("f2")); //现金业绩
                    cashBll.UpdateJoinUserYejiForPay(cond);
                }
            }

            #endregion

            #region ==修改表状态==

            var shiyongbiao = cashBll.getAppointmentModel(OldOrderNo); //获取这个订单的床位使用情况
            shiyongbiao.Type = 3;
            cashBll.UpdateAppointmenttype(shiyongbiao); //修改床位使用表
            ResDocBLL.UpReservationStatu(3, shiyongbiao.ReservationID);

            #endregion

            #region ==添加积分==

            var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate"); //获取积分实体
            decimal jifen = 0;
            if (usersetting.AttributeValue != null && usersetting.AttributeValue != "")
            {
                jifen = Convert.ToDecimal(entity.Price) / (usersetting.AttributeValue == null
                            ? 1000000
                            : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
            }

            IntegralRecordEntity ir = new IntegralRecordEntity();
            ir.UserID = entity.UserID;
            ir.OrderNO = entity.OrderNo;
            ir.Integral = jifen;
            ir.CreateTime = DateTime.Now;
            IpatBLL.AddIntegralRecord(ir); //添加用户积分记录

            var intrgral = IpatBLL.GetIntegralModel(entity.UserID); //获取原来的积分列表
            intrgral.AllIntegral = intrgral.AllIntegral + jifen;
            intrgral.Integral = intrgral.Integral + jifen;

            IpatBLL.UpdateIntegral(intrgral);

            #endregion

            return Json(result);
        }

        public ActionResult UserVerify()
        {
            return View();
        }

        /// <summary>
        /// 用户验证
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public ActionResult VerifyUser(string UserId, string Password)
        {
            var result = 0;
            var Model = IpatBLL.GetPatienttEntityByID(Convert.ToInt32(UserId)); //用户当前用户信息
            if (Model != null)
            {
                if (Model.Password == Encrypt.MD5(Password))
                {
                    result = 1;
                }
            }

            return Json(result);
        }

        #endregion

        #region==单据列表==

        public ActionResult OrderList(OrderEntity entity)
        {

            if (entity.UserName == "姓名、订单号、会员卡号") entity.UserName = "";
            entity.StartTime = (entity.StartTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01")
                : Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = (entity.EndTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59")
                : Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59"));

            entity.RuHuiStartTime = (entity.RuHuiStartTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01")
                : Convert.ToDateTime(entity.RuHuiStartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.RuHuiEndTime = (entity.RuHuiEndTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59")
                : Convert.ToDateTime(entity.RuHuiEndTime).ToString("yyyy-MM-dd 23:59:59"));
            int rows;
            int pagecount;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            //  ViewBag.HospitalID = entity.HospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "")
            {
                entity.orderField = "DocumentNumber";
                entity.orderDirection = "desc";
            }

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

            bool HasPermission1 =
                Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine
                    .TransferofworkManager_模拟账号); //模拟账号控制查看权限
            if (HasPermission1)
            {
                entity.IsShow = 2;
            }

            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            var list = cashBll.GetAllOrderList(entity, out rows, out pagecount);
            //  list = list.Where(c => c.Price > 0).ToList();
            list = list.Where(t => t.OrderNo.StartsWith("U") == false).ToList();
            //list = list.Where(t=>t.Statu!=4).ToList();
            var result = list.ToPagedList(entity.pageNum, entity.numPerPage);

            //var sumList = cashBll.GetSumOrderForEveryPrice(entity);//获取
            ViewBag.Price = list.Sum(c => c.Price);

            ViewBag.SumMoney = list.Sum(c => c.SumMoney);
            ViewBag.Xianjin = list.Sum(c => c.Xianjin);
            ViewBag.Yinlianka = list.Sum(c => c.Yinlianka);
            ViewBag.Zhifubao = list.Sum(c => c.Zhifubao);
            ViewBag.Weixin = list.Sum(c => c.Weixin);
            ViewBag.Chuzhika = list.Sum(c => c.Chuzhika);
            ViewBag.ZengChuzhika = list.Sum(c => c.ZengChuzhika);
            ViewBag.Huakouka = list.Sum(c => c.Huakouka);
            ViewBag.OtherPay = list.Sum(c => c.OtherPay);
            ViewBag.QianPrice = list.Where(i => i.QianPrice < 0).Sum(i => i.QianPrice);
            ViewBag.Jifen = list.Sum(c => c.Jifen);
            result.TotalItemCount = list.Count();
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.OrderType = entity.OrderType;
            ViewBag.StartTime = Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd");
            ViewBag.EndTime = Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd");

            if (Request.IsAjaxRequest())
                return PartialView("_OrderList", result);
            return View(result);


            //if (entity.UserName == "姓名、订单号、会员卡号") entity.UserName = "";
            //entity.StartTime = (entity.StartTime == null ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01") : Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01"));
            //entity.EndTime = (entity.EndTime == null ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59") : Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59"));
            //int rows;
            //int pagecount;
            //entity.HospitalID = entity.HospitalID == 0 ? loginUserEntity.HospitalID : entity.HospitalID;
            //ViewBag.ParentHospitalID = loginUserEntity.ParentHospitalID;
            //ViewBag.HospitalID = entity.HospitalID;
            //entity.numPerPage = 50; //每页显示条数
            //if (entity.orderField + "" == "") { entity.orderField = "DocumentNumber"; entity.orderDirection = "desc"; }

            //if (entity.Statu == 0) //首次加载页面查询 已完成的
            //{
            //    ViewBag.Statu = 1;
            //    entity.Statu = 1;
            //}
            //else if (entity.Statu == 999) //查询全部
            //{
            //    entity.Statu = 0;
            //    ViewBag.Statu = entity.Statu;
            //}
            //else
            //{
            //    ViewBag.Statu = entity.Statu;
            //}
            //var list = cashBll.GetAllOrder(entity, out rows, out pagecount);

            //var newlist = new List<OrderEntity>();
            //foreach (var info in list)
            //{
            //    var infolist = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = info.OrderNo });
            //    decimal otherpay = 0;
            //    foreach (var io in infolist)
            //    {
            //        if (io.PayType == 1)
            //        {
            //            info.Xianjin = io.PayMoney;
            //        }
            //        else if (io.PayType == 2)
            //        {
            //            info.Yinlianka = io.PayMoney;
            //        }
            //        else if (io.PayType == 3)
            //        {
            //            info.Chuzhika = info.Chuzhika + io.PayMoney;
            //        }
            //        else if (io.PayType == 4)
            //        {
            //            info.Huakouka = info.Huakouka + io.PayMoney;
            //        }
            //        else
            //        {
            //            otherpay += io.PayMoney;
            //        }
            //    }
            //    info.OtherPay = otherpay;

            //    newlist.Add(info);
            //}
            //var result = newlist.AsQueryable().ToPagedList(1, entity.numPerPage);

            //var sumList = cashBll.GetSumOrderForEveryPrice(entity);//获取
            //ViewBag.xianjin = sumList.Where(i => i.PayType == 1).Count() > 0 ? sumList.Where(i => i.PayType == 1).ToList().Sum(z => z.SumMoney) : 0;
            //ViewBag.shuaka = sumList.Where(i => i.PayType == 2).Count() > 0 ? sumList.Where(i => i.PayType == 2).ToList().Sum(z => z.SumMoney) : 0;
            //ViewBag.zongshouru = ViewBag.xianjin + ViewBag.shuaka;
            //ViewBag.chuzhi = sumList.Where(i => i.PayType == 3).Count() > 0 ? sumList.Where(i => i.PayType == 3).ToList().Sum(z => z.SumMoney) : 0;
            //ViewBag.liaocheng = sumList.Where(i => i.PayType == 4).Count() > 0 ? sumList.Where(i => i.PayType == 4).ToList().Sum(z => z.SumMoney) : 0;
            //ViewBag.qita = sumList.Where(i => i.PayType > 4).Count() > 0 ? sumList.Where(i => i.PayType > 4).ToList().Sum(z => z.SumMoney) : 0;
            //ViewBag.qiankuan = list.Where(i => i.QianPrice < 0).Sum(i => i.QianPrice);

            //result.TotalItemCount = rows;
            //result.CurrentPageIndex = entity.pageNum;
            //ViewBag.orderField = entity.orderField;
            //ViewBag.orderDirection = entity.orderDirection;
            //ViewBag.OrderType = entity.OrderType;
            //ViewBag.StartTime = Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd");
            //ViewBag.EndTime = Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd");

            //if (Request.IsAjaxRequest())
            //    return PartialView("_OrderList", result);
            //return View(result);
        }
        public ActionResult SignView()
        {
            ViewBag.Autograph = "";
            string OrderNo = Request["OrderNofilter"];
            if (OrderNo != "")
            {
                try
                {
                    string conStr = "server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft";//连接字符串  
                    SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                    conText.Open();//打开数据库  
                    string sqls = @"SELECT top 1 Autograph, * from EYB_tb_Order WHERE OrderNo = '" + OrderNo + "'";
                    SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                    SqlDataReader dr;//创建DataReader对象  
                    dr = comText.ExecuteReader();//执行查询  
                    while (dr.Read())//判断数据表中是否含有数据  
                    {
                        var date = dr;
                        ViewBag.Autograph = date["Autograph"].ToString();
                    }
                    dr.Close();//关闭DataReader对象  
                }
                catch
                {

                }
            }
            return View();

        }
            /// <summary>
            /// 订单详情
            /// </summary>
            /// <returns></returns>
         public ActionResult OrderDetail()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.Username = LoginUserEntity.UserName;
            var entity = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "IsOpenAutoPrint");
            ViewBag.IsOpenAutoPrint = entity.AttributeValue;
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

            return View();
        }

        /// <summary>
        /// 获取参与记录
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllJoinUser(int orderinfoid, string projectname, int Quantity)
        {
            int row = 0;
            int rowpage = 0;
            var list = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = orderinfoid }, out row, out rowpage);
            StringBuilder str = new StringBuilder();
            foreach (var info in list)
            {
                str.AppendFormat("<tr code={0}>", info.ID);
                str.AppendFormat("<td>{0}</td>", info.UserName);
                str.AppendFormat("<td>{0}</td>", info.DutyName);
                str.AppendFormat("<td style='display:none;'>{0}</td>", info.IsZhiding == 1 ? "是" : "否");
                str.AppendFormat("<td>{0}</td>", info.ProjectName);
                // str.AppendFormat("<td><a href='javascript:;' onclick='UpdateJoinUser({0},\"{1}\",{2})'>编辑</a>&nbsp;&nbsp;&nbsp;<a href='javascript:;' onclick='DeleteJoinUser({0})'>删除</a></td>", info.ID, projectname, Quantity);

                str.Append("</tr>");
            }

            return Content(str.ToString());
        }

        /// <summary>
        /// 删除参与员工 ----修改状态
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult DeleteJoinUser(int ID)
        {
            int result = cashBll.DelJoinUser(ID);
            return Json(result);
        }

        /// <summary>
        /// 编辑员工业绩页面
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult UpdateYeji()
        {
            return View();
        }

        /// <summary>
        /// 编辑员工业绩
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult SaveJonUser(JoinUserEntity entity)
        {
            int result = cashBll.UpdateJoinUser(entity);
            return Json(result);

        }

        /// <summary>
        /// 订单取消功能
        /// 1.匹配是否是符合条件的单据,如果不是,不处理.
        /// 2.先找到当前订单的订单详情,查看是项目,产品,还是划扣
        /// 3.如果是项目,则不用处理.如果是产品,则修改仓库库存.如果是划扣,则添加回原来的卡项中.
        /// 4.查询当前订单的支付方式,现金,银联不用返回.储值卡的要退回储值卡,其它暂时不处理.
        /// 5.当这一切处理完成后,修改订单状态
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <returns></returns>
        public ActionResult ClearOrder(string OrderNo)
        {
            var order = cashBll.GetOrderModel(new OrderEntity { OrderNo = OrderNo });
            if (order == null) //如果找不到当前订单,则提示处理失败!
            {
                return Json(-2);
            }

            if (order.OrderType != 1 || order.Statu != 1) //如果不是项目产品,则提示处理失败!
            {
                return Json(-2);
            }

            var infoList = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = OrderNo }); //获取订单详情
            var payModel = cashBll.GetPaymentOrderNoModel(new PaymentOrderEntity { OrderNo = OrderNo });
            //获取仓库列表
            var cangkulist = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = order.HospitalID });
            var thetype = sysbll.GetBaseInfoTreeByType("WarehouseIn", 1, LoginUserEntity.ParentHospitalID); //获取库存原因

            //遍历处理
            foreach (var iinfo in infoList)
            {
                var kucunlist = new List<ProductStockEntity>(); //库存列表
                //划扣处理方式
                if (iinfo.InfoBuyType == 3)
                {
                    var kaxiang = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = iinfo.CardID });
                    kaxiang.Tiems += iinfo.Quantity;
                    kaxiang.KouTimes -= iinfo.Quantity;
                    cashBll.UpdateMainCardBalance(kaxiang);
                }
                //产品处理方式
                else if (iinfo.InfoBuyType == 4)
                {
                    //如果存在欠货单 则取消欠货单，欠货单不出库，取消也不入库，平掉
                    //梳理系统设置下数据导入的产品导入关联产品管理的接口数据列表（完成）
                    //梳理顾客管理的顾客未消耗统计的列表关联顾客详情储值卡列表接口（完成） 
                    //var warehouseinfo = iwareBLL.GetJiChunListByOrderNo(new JiChunEntity { Memo = OrderNo });
                    //if (warehouseinfo.ID == 0) //如果不存在欠货单
                    //{
                    ProductStockEntity pse = new ProductStockEntity();
                        ProductStockEntity entity = new ProductStockEntity();
                        entity.HospitalID = order.HospitalID;
                        entity.ProductID = iinfo.ProdcuitID;
                        entity.HouseID = cangkulist[0].ID; //一个门店一个仓库,所以直接拿来用
                        var list = iwareBLL.GetProductStockListToseleect(entity);
                        if (list != null)
                        {
                            foreach (var info in list)
                            {
                                kucunlist.Add(info);
                            }
                        }

                        if (kucunlist.Count > 0)
                        {
                            #region ==新建入库单据==

                            StockOrderEntity sentity = new StockOrderEntity();
                            sentity.BaseID = thetype[0].ID;
                            sentity.HospitalID = order.HospitalID;
                            sentity.IsVerify = 2;
                            sentity.Memo = "项目产品订单取消入库单据";
                            sentity.OrderNo = RandomHelper.GetRandomStock("R");
                            sentity.OrderTime = DateTime.Now;
                            sentity.Type = 2;
                            sentity.UserID = LoginUserEntity.UserID;
                            sentity.VerifyTime = DateTime.Now;
                            sentity.VerifyUserID = LoginUserEntity.UserID;
                            sentity.Class = 6;
                            sentity.VerifyMemo = order.OrderNo;
                            sentity.AllMoney = iinfo.AllPrice;
                            sentity.AllQuatity = iinfo.Quantity;
                            sentity.AllChengBen = iinfo.CostPrice * iinfo.Quantity;
                            //添加入库单据
                            var rt = iwareBLL.AddStockOrder(sentity);

                            #endregion

                            #region ==添加单据详情==

                            //添加单据详情
                            StockOrderInfo soinfo = new StockOrderInfo();
                            soinfo.StockOrderNo = sentity.OrderNo;
                            soinfo.Number = iinfo.Quantity;
                            soinfo.ProductID = iinfo.ProdcuitID;
                            soinfo.HouseID = cangkulist[0].ID;

                            iwareBLL.AddStockOrderInfo(soinfo);

                            #endregion

                            #region ==入库操作==

                            if (rt > 0)
                            {
                                StockChangeLogEntity scl = new StockChangeLogEntity();
                                scl.UserID = LoginUserEntity.UserID;
                                scl.LogClass = 8;
                                scl.LogState = 1;
                                scl.OwnedWarehouse = soinfo.HouseID;
                                scl.IntoWarehouse = pse.HouseID;
                                scl.ProductID = iinfo.ProdcuitID;
                                scl.LogType = "入库";
                                scl.LogTitle = "订单产品订单取消.取消订单编号:" + iinfo.OrderNo + "; 产品ID:" + iinfo.ProdcuitID + ";返回数量:" + iinfo.Quantity + ";";
                                scl.NewQuatity = kucunlist[0].Quatity + iinfo.Quantity;
                                scl.OldQuatity = kucunlist[0].Quatity;
                                scl.Quantity = iinfo.Quantity;
                                scl.ProductStockID = kucunlist[0].ID;
                                scl.OrderNo = sentity.OrderNo;
                                var sclID = iwareBLL.AddStockChangeLog(scl);
                                //这里需要入库操作
                                pse.ID = kucunlist[0].ID;
                                pse.Quatity = kucunlist[0].Quatity + iinfo.Quantity;
                                iwareBLL.UpdateProductStock(pse);
                            }

                            #endregion
                        }

                    //}
                    //else
                    //{
                    //    //取消欠货单据
                    //    iwareBLL.DelJiChunByOrderNo(OrderNo);
                    //    //if (order.HospitalID == 1080) {
                    //        //比较欠货单和单据产品数量，如果单据产品数量大于欠货单数量，则进行入库处理
                    //        //if (iinfo.Quantity > warehouseinfo.JichunCount)
                    //        //{
                    //            ProductStockEntity pse = new ProductStockEntity();
                    //            ProductStockEntity entity = new ProductStockEntity();
                    //            entity.HospitalID = order.HospitalID;
                    //            entity.ProductID = iinfo.ProdcuitID;
                    //            entity.HouseID = cangkulist[0].ID; //一个门店一个仓库,所以直接拿来用
                    //            var list = iwareBLL.GetProductStockListToseleect(entity);
                    //            if (list != null)
                    //            {
                    //                foreach (var info in list)
                    //                {
                    //                    kucunlist.Add(info);
                    //                }
                    //            }

                    //            if (kucunlist.Count > 0)
                    //            {
                    //                #region ==新建入库单据==

                    //                StockOrderEntity sentity = new StockOrderEntity();
                    //                sentity.BaseID = thetype[0].ID;
                    //                sentity.HospitalID = order.HospitalID;
                    //                sentity.IsVerify = 2;
                    //                sentity.Memo = "项目产品订单取消入库单据";
                    //                sentity.OrderNo = RandomHelper.GetRandomStock("R");
                    //                sentity.OrderTime = DateTime.Now;
                    //                sentity.Type = 2;
                    //                sentity.UserID = LoginUserEntity.UserID;
                    //                sentity.VerifyTime = DateTime.Now;
                    //                sentity.VerifyUserID = LoginUserEntity.UserID;
                    //                sentity.Class = 6;
                    //                sentity.VerifyMemo = order.OrderNo;
                    //                sentity.AllMoney = iinfo.AllPrice;
                    //                sentity.AllQuatity = (iinfo.Quantity - warehouseinfo.JichunCount);
                    //                sentity.AllChengBen = iinfo.CostPrice * (iinfo.Quantity - warehouseinfo.JichunCount);
                    //                //添加入库单据
                    //                var rt = iwareBLL.AddStockOrder(sentity);

                    //                #endregion

                    //                #region ==添加单据详情==

                    //                //添加单据详情
                    //                StockOrderInfo soinfo = new StockOrderInfo();
                    //                soinfo.StockOrderNo = sentity.OrderNo;
                    //                soinfo.Number = (iinfo.Quantity - warehouseinfo.JichunCount);
                    //                soinfo.ProductID = iinfo.ProdcuitID;
                    //                soinfo.HouseID = cangkulist[0].ID;

                    //                iwareBLL.AddStockOrderInfo(soinfo);

                    //                #endregion

                    //                #region ==入库操作==

                    //                if (rt > 0)
                    //                {
                    //                    StockChangeLogEntity scl = new StockChangeLogEntity();
                    //                    scl.UserID = LoginUserEntity.UserID;
                    //                    scl.LogClass = 8;
                    //                    scl.LogState = 1;
                    //                    scl.OwnedWarehouse = soinfo.HouseID;
                    //                    scl.IntoWarehouse = pse.HouseID;
                    //                    scl.ProductID = iinfo.ProdcuitID;
                    //                    scl.LogType = "入库";
                    //                    scl.LogTitle = "订单产品订单取消.取消订单编号:" + iinfo.OrderNo + "; 产品ID:" + iinfo.ProdcuitID + ";返回数量:" + iinfo.Quantity + ";";
                    //                    scl.NewQuatity = kucunlist[0].Quatity + (iinfo.Quantity - warehouseinfo.JichunCount);
                    //                    scl.OldQuatity = kucunlist[0].Quatity;
                    //                    scl.Quantity = (iinfo.Quantity - warehouseinfo.JichunCount);
                    //                    scl.ProductStockID = kucunlist[0].ID;
                    //                    scl.OrderNo = sentity.OrderNo;
                    //                    var sclID = iwareBLL.AddStockChangeLog(scl);
                    //                    //这里需要入库操作
                    //                    pse.ID = kucunlist[0].ID;
                    //                    pse.Quatity = kucunlist[0].Quatity + (iinfo.Quantity - warehouseinfo.JichunCount);
                    //                    iwareBLL.UpdateProductStock(pse);
                    //                }

                    //                #endregion
                    //            }
                    //        //}
                        
                    //     //}
                    //    //else
                    //    //{
                    //    //    //比较欠货单和单据产品数量，如果单据产品数量大于欠货单数量，则进行入库处理
                    //    //    if (iinfo.Quantity > warehouseinfo.JichunCount)
                    //    //    {
                    //    //        ProductStockEntity pse = new ProductStockEntity();
                    //    //        ProductStockEntity entity = new ProductStockEntity();
                    //    //        entity.HospitalID = order.HospitalID;
                    //    //        entity.ProductID = iinfo.ProdcuitID;
                    //    //        entity.HouseID = cangkulist[0].ID; //一个门店一个仓库,所以直接拿来用
                    //    //        var list = iwareBLL.GetProductStockListToseleect(entity);
                    //    //        if (list != null)
                    //    //        {
                    //    //            foreach (var info in list)
                    //    //            {
                    //    //                kucunlist.Add(info);
                    //    //            }
                    //    //        }

                    //    //        if (kucunlist.Count > 0)
                    //    //        {
                    //    //            #region ==新建入库单据==

                    //    //            StockOrderEntity sentity = new StockOrderEntity();
                    //    //            sentity.BaseID = thetype[0].ID;
                    //    //            sentity.HospitalID = order.HospitalID;
                    //    //            sentity.IsVerify = 2;
                    //    //            sentity.Memo = "项目产品订单取消入库单据";
                    //    //            sentity.OrderNo = RandomHelper.GetRandomStock("R");
                    //    //            sentity.OrderTime = DateTime.Now;
                    //    //            sentity.Type = 2;
                    //    //            sentity.UserID = LoginUserEntity.UserID;
                    //    //            sentity.VerifyTime = DateTime.Now;
                    //    //            sentity.VerifyUserID = LoginUserEntity.UserID;
                    //    //            sentity.Class = 6;
                    //    //            sentity.VerifyMemo = order.OrderNo;
                    //    //            sentity.AllMoney = iinfo.AllPrice;
                    //    //            sentity.AllQuatity = (iinfo.Quantity - warehouseinfo.JichunCount);
                    //    //            sentity.AllChengBen = iinfo.CostPrice * (iinfo.Quantity - warehouseinfo.JichunCount);
                    //    //            //添加入库单据
                    //    //            var rt = iwareBLL.AddStockOrder(sentity);

                    //    //            #endregion

                    //    //            #region ==添加单据详情==

                    //    //            //添加单据详情
                    //    //            StockOrderInfo soinfo = new StockOrderInfo();
                    //    //            soinfo.StockOrderNo = sentity.OrderNo;
                    //    //            soinfo.Number = (iinfo.Quantity - warehouseinfo.JichunCount);
                    //    //            soinfo.ProductID = iinfo.ProdcuitID;
                    //    //            soinfo.HouseID = cangkulist[0].ID;

                    //    //            iwareBLL.AddStockOrderInfo(soinfo);

                    //    //            #endregion

                    //    //            #region ==入库操作==

                    //    //            if (rt > 0)
                    //    //            {
                    //    //                StockChangeLogEntity scl = new StockChangeLogEntity();
                    //    //                scl.UserID = LoginUserEntity.UserID;
                    //    //                scl.LogClass = 8;
                    //    //                scl.LogState = 1;
                    //    //                scl.OwnedWarehouse = soinfo.HouseID;
                    //    //                scl.IntoWarehouse = pse.HouseID;
                    //    //                scl.ProductID = iinfo.ProdcuitID;
                    //    //                scl.LogType = "入库";
                    //    //                scl.LogTitle = "订单产品订单取消.取消订单编号:" + iinfo.OrderNo + "; 产品ID:" + iinfo.ProdcuitID + ";返回数量:" + iinfo.Quantity + ";";
                    //    //                scl.NewQuatity = kucunlist[0].Quatity + (iinfo.Quantity - warehouseinfo.JichunCount);
                    //    //                scl.OldQuatity = kucunlist[0].Quatity;
                    //    //                scl.Quantity = (iinfo.Quantity - warehouseinfo.JichunCount);
                    //    //                scl.ProductStockID = kucunlist[0].ID;
                    //    //                scl.OrderNo = sentity.OrderNo;
                    //    //                var sclID = iwareBLL.AddStockChangeLog(scl);
                    //    //                //这里需要入库操作
                    //    //                pse.ID = kucunlist[0].ID;
                    //    //                pse.Quatity = kucunlist[0].Quatity + (iinfo.Quantity - warehouseinfo.JichunCount);
                    //    //                iwareBLL.UpdateProductStock(pse);
                    //    //            }

                    //    //            #endregion
                    //    //        }
                    //    //    }
                    //    //}
                    //}
                }

                //如果取消单据，如果是项目 则生成虚拟仓库退回
                if (iinfo.InfoBuyType == 3 || iinfo.InfoBuyType == 5)
                {
                    //获取项目和产品关系，写入院用消耗，虚拟仓库
                    IList<ProjectProductRelationEntity> relationList = baseinfoBLL.GetProjectProductRelation(new ProjectProductRelationEntity { ProjectID = iinfo.ProdcuitID });
                    foreach (var mod in relationList)
                    {
                        iwareBLL.AddProductUseUnit(new ProductUseUnitEntity
                        {
                            HospitalID = LoginUserEntity.HospitalID,
                            ProductID = mod.ProductID,
                            ProjectID = iinfo.ProdcuitID,
                            UseUnit = -mod.DanCiUseUnit * iinfo.Quantity,
                            Quatity = -iinfo.Quantity
                        });
                        //mod.ProductID
                        // iwareBLL.UpdateProductXuniStock(new ProductXuniStockEntity { HospitalID=loginUserEntity.HospitalID, ProductID = mod.ProductID, UseUnit = mod.DanCiUseUnit * info.Quantity });
                    }
                }
            }

            //取消预付单
            var yufulist = iwareBLL.GetStockOrderByOrderNo(OrderNo);
            var intrgral1 = IpatBLL.GetIntegralModel(order.UserID);//获取原来的积分列表
            var pagyList1 = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = OrderNo });
            //var mainCardBalanceModel = cashBll.GetMainCardBalanceUser_Model(new MainCardBalanceEntity { UserID = mainCardBalance.UserID, UserCardID = Convert.ToInt32(pagyList[i].CardID) });
            //var model = cashBll.GetPaymentOrderNoModelID(new PaymentOrderEntity { CardID = Convert.ToInt32(pagyList[i].CardID), OrderNo = OrderNo });
            //if (mainCardBalanceModel.ID > 0 && model.ParentPayType == 3)
            //{
            //    mainCardBalanceModel.AllPrice += payModel.PayMoney;
            //    mainCardBalanceModel.Price += payModel.PayMoney;
            //    int number = cashBll.EYB_tb_MainCardBalance_UpdateAllPrice(mainCardBalanceModel);
            //}
            if (pagyList1.Count > 0)
            {
                for (int i = 0; i < pagyList1.Count; i++)
                {
                    var mainCardBalanceModel = cashBll.GetMainCardBalanceUser_Model(new MainCardBalanceEntity { UserID = order.UserID, UserCardID = Convert.ToInt32(pagyList1[i].CardID) });
                    var model = cashBll.GetPaymentOrderNoModelID(new PaymentOrderEntity { CardID = Convert.ToInt32(pagyList1[i].CardID), OrderNo = OrderNo });
                    if (model.ParentPayType == 8)
                    {
                        intrgral1.AllIntegral += model.PayMoney;
                        intrgral1.Integral += model.PayMoney;
                        IpatBLL.UpdateIntegral(intrgral1);
                        IntegralOperationEntity entity1 = new IntegralOperationEntity();
                        entity1.Memo = "取消消费";
                        entity1.Name = "+";
                        entity1.Number = model.PayMoney;
                        entity1.UserID = order.UserID;
                        entity1.OperationType = 2;
                        var id = IpatBLL.AddIntegralOperation(entity1);
                        IntegralRecordEntity ir1 = new IntegralRecordEntity();
                        ir1.UserID = order.UserID;
                        ir1.Integral = model.PayMoney;
                        ir1.CreateTime = DateTime.Now;
                        ir1.OrderNO = order.OrderNo;
                        ir1.DocumentNumber = "取消消费";
                        ir1.IntegralOperationID = 0;
                        //ir1.IntegralOperationID = id;
                        IpatBLL.AddIntegralRecord(ir1);//增加积分记录
                    }
                    //if (mainCardBalanceModel.ID > 0 && model.ParentPayType == 3)
                    //{
                    //    mainCardBalanceModel.AllPrice += model.PayMoney;
                    //    mainCardBalanceModel.Price += model.PayMoney;
                    //    int number = cashBll.EYB_tb_MainCardBalance_UpdateAllPrice(mainCardBalanceModel);
                    //}

                }


            }

            foreach (var yufuinfo in yufulist)
            {
                iwareBLL.DelStockOrder(yufuinfo.ID);
            }
            //获取订单结算方式
            var PayList = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = order.OrderNo });
            foreach (var payinfo in PayList)
            {
                if (payinfo.PayType == 3) //储值卡支付
                {
                    var mcb = new MainCardBalanceEntity { ID = payinfo.CardBalanceID, Price = payinfo.PayMoney };
                    cashBll.RechargeMoney(mcb);

                    //用户储值卡余额明细日志
                    var mainCardBalance = cashBll.GetMainCardBalanceModel(mcb);
                    cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                    {
                        CardBalanceId = mcb.ID,
                        OrderNo = payinfo.OrderNo,
                        Type = (int)MainCardBalanceDetailType.退单,
                        Amount = mcb.Price,
                        Balance = mainCardBalance.Price + mcb.Price,
                        OldAmount = mainCardBalance.Price,
                        CreateTime = DateTime.Now
                    });
                }
                else if (payinfo.PayType == 4) //划卡
                {
                    cashBll.UpdateMainCardBalanceTimeByOrderClear(new MainCardBalanceEntity { ID = payinfo.CardBalanceID, Tiems = payinfo.PayTime });
                } 
            }
            if (PayList.Count() > 0 && PayList != null)
            {
                #region ==添加积分==
                int payinfos = PayList.ToList()[0].PayType;
                // if (payinfos == 1 || payinfos == 2 || payinfos == 10519 || payinfos == 14687)原来的
                string zfname = PayList.ToList()[0].PayName;
                if (zfname == "支付宝" || zfname == "微信" || zfname == "银联" || zfname == "现金"|| zfname == "银联卡" )
                {
                    //新取消积分
                    if (order.ActurePrice > 0)
                    {
                        decimal jifen = 0;
                        var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate"); //获取积分实体

                        if (LoginUserEntity.HospitalID == 1017)
                        {
                            var orderInfoList = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = OrderNo });
                            int proid = Convert.ToInt32(orderInfoList[0].ProdcuitID);
                            var productModel = cashBll.GetProjectProductModelNew(proid.ToString());
                            int typeid = Convert.ToInt32(orderInfoList[0].Type);
                            if (typeid == 2)
                            {
                                var productModel1 = baseinfoBLL.GetModel(proid);
                                if (productModel1.ID > 0)
                                {
                                    if (productModel1.Proportion > 0)
                                    {
                                        // jifen = Convert.ToDecimal(order.ActurePrice) / Convert.ToDecimal(productModel1.Proportion);
                                        // jifen = CutDecimalWithN(jifen, 0);
                                    }
                                    else
                                    {
                                       // jifen = Convert.ToDecimal(order.ActurePrice) / (usersetting.AttributeValue == null
                                                  //  ? 1000000
                                                   // : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                                    }
                                }
                            }
                            if (productModel.ID > 0)
                            {
                                if (productModel.Proportion > 0)
                                {
                                    // decimal tion = productModel.Proportion / 100;
                                    //jifen = Convert.ToDecimal(order.ActurePrice) / Convert.ToDecimal(productModel.Proportion);
                                    // jifen = CutDecimalWithN(jifen, 0);
                                    //(usersetting.AttributeValue == null
                                    //    ? 1000000
                                    //    : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                                }
                                else
                                {
                                   // jifen = Convert.ToDecimal(order.ActurePrice) / (usersetting.AttributeValue == null
                                             //   ? 1000000
                                              //  : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                                }

                            }
                        }
                        else
                        {

                            if (usersetting.AttributeValue != null && usersetting.AttributeValue != "")
                            {
                                jifen = Convert.ToDecimal(order.ActurePrice) / (usersetting.AttributeValue == null
                                            ? 1000000
                                            : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                            }
                        }


                        var intrgral = IpatBLL.GetIntegralModel(order.UserID); //获取原来的积分列表
                        intrgral.AllIntegral = intrgral.AllIntegral + (-jifen);
                        intrgral.Integral = intrgral.Integral + (-jifen);
                        if (LoginUserEntity.HospitalID == 1017)
                        {

                        }
                        else
                        {
                            IpatBLL.UpdateIntegral(intrgral);
                        }
                    }
                }
                //if (payinfos == 1 || payinfos == 2)
                //{
                //    if (order.ActurePrice > 0)
                //    {
                //        var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate"); //获取积分实体
                //        decimal jifen = 0;
                //        if (usersetting.AttributeValue != null && usersetting.AttributeValue != "")
                //        {
                //            jifen = Convert.ToDecimal(order.ActurePrice) / (usersetting.AttributeValue == null
                //                        ? 1000000
                //                        : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                //        }

                //        var intrgral = IpatBLL.GetIntegralModel(order.UserID); //获取原来的积分列表
                //        intrgral.AllIntegral = intrgral.AllIntegral + (-jifen);
                //        intrgral.Integral = intrgral.Integral + (-jifen);

                //        IpatBLL.UpdateIntegral(intrgral);
                //    }
                //}
                #endregion
            }



            order.Statu = 3;
            int result = cashBll.UpdateOrder(order);

            return Json(result);

        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="Statu"></param>
        /// <returns></returns>
        //public ActionResult UpdateOrderSattu(string OrderNo, int Statu)
        //{
        //    var order = cashBll.GetOrderModel(new OrderEntity { OrderNo = OrderNo });
        //    if (String.IsNullOrEmpty(order.OrderNo)) //如果找不到当前订单,则提示处理失败!
        //    {
        //        return Json(-2);
        //    }
        //    order.Statu = Statu; //处理异常单据或者作废单据
        //    int result = cashBll.UpdateOrderSattu(order);
        //    return Json(result);
        //}
        public static decimal CutDecimalWithN(decimal d, int n)
        {
            string strDecimal = d.ToString();
            int index = strDecimal.IndexOf(".");
            if (index == -1 || strDecimal.Length < index + n + 1)
            {
                strDecimal = string.Format("{0:F" + n + "}", d);
            }
            else
            {
                int length = index;
                if (n != 0)
                {
                    length = index + n + 1;
                }
                strDecimal = strDecimal.Substring(0, length);
            }
            return Decimal.Parse(strDecimal);
        }
        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="Statu"></param>
        /// <returns></returns>
        public ActionResult UpdateOrderSattu(string OrderNo, int Statu)
        {
            var order = cashBll.GetOrderModel(new OrderEntity { OrderNo = OrderNo });
            if (String.IsNullOrEmpty(order.OrderNo)) //如果找不到当前订单,则提示处理失败!
            {
                return Json(-2);
            }
            order.Statu = Statu; //处理异常单据或者作废单据
            int result = cashBll.UpdateOrderSattu(order);
            var mainCardBalance = cashBll.GetMainCardBalanceOrderNoModel(new MainCardBalanceEntity { OrderNo = OrderNo });
            var payModel = cashBll.GetPaymentOrderNoModel(new PaymentOrderEntity { OrderNo = OrderNo });
            var pagyList = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = OrderNo });
            if (!string.IsNullOrEmpty(mainCardBalance.OrderNo))
            {
                if (mainCardBalance.IsDel != 3)
                {
                    for (int i = 0; i < pagyList.Count; i++)
                    {
                        var mainCardBalanceModel = cashBll.GetMainCardBalanceUser_Model(new MainCardBalanceEntity { UserID = mainCardBalance.UserID, UserCardID = Convert.ToInt32(pagyList[i].CardID) });
                        var model = cashBll.GetPaymentOrderNoModelID(new PaymentOrderEntity { CardID = Convert.ToInt32(pagyList[i].CardID), OrderNo = OrderNo });
                        if (mainCardBalanceModel.ID > 0 && model.ParentPayType == 3)
                        {
                            mainCardBalanceModel.AllPrice += payModel.PayMoney;
                            mainCardBalanceModel.Price += payModel.PayMoney;
                            int number = cashBll.EYB_tb_MainCardBalance_UpdateAllPrice(mainCardBalanceModel);
                        }
                        var intrgral = IpatBLL.GetIntegralModel(mainCardBalance.UserID);//获取原来的积分列表
                        if (model.ParentPayType == 8)
                        {
                            if (LoginUserEntity.HospitalID == 1017)
                            {
                                //9999
                                IntegralOperationEntity entity = new IntegralOperationEntity();
                                entity.Memo = "疗程卡撤销";
                                entity.Name = "+";
                                entity.Number = model.PayMoney;
                                entity.UserID = mainCardBalance.UserID;
                                entity.OperationType = 2;
                                var id = IpatBLL.AddIntegralOperation(entity);
                                IntegralRecordEntity ir1 = new IntegralRecordEntity();
                                ir1.UserID = mainCardBalance.UserID;
                                ir1.Integral = model.PayMoney;
                                ir1.CreateTime = DateTime.Now;
                                ir1.OrderNO = mainCardBalance.OrderNo;
                                ir1.DocumentNumber = "疗程卡撤销";
                                ir1.IntegralOperationID = id;
                                IpatBLL.AddIntegralRecord(ir1);//增加积分记录
                            }
                            intrgral.AllIntegral += model.PayMoney;
                            intrgral.Integral += model.PayMoney;
                            IpatBLL.UpdateIntegral(intrgral);


                        }
                        if (model.ParentPayType == 1)
                        {
                            if (LoginUserEntity.HospitalID == 1017)
                            {

                            }
                            else
                            {
                                var intrgralRecordList = IpatBLL.GetIntegralRecordListInfo(OrderNo);
                                if (intrgralRecordList.Count > 0)
                                {
                                    foreach (var item in intrgralRecordList)
                                    {
                                        if (intrgral.Integral>0)
                                        {
                                            intrgral.AllIntegral = intrgral.AllIntegral - item.Integral;
                                            intrgral.Integral = intrgral.Integral - item.Integral;
                                            IpatBLL.UpdateIntegral(intrgral);
                                        }

                                    }
                                }
                            }

                        }
                    }

                    mainCardBalance.IsDel = Statu;
                    int count = cashBll.UpdateMainCardBalanceIsDel(mainCardBalance);

                }

                // return Json(5);
            }
            //  payOrder.IsDel = Statu;
            // int result1 = cashBll.UpdatePaymentOrderIsDel(payOrder);
            return Json(result);
        }
        #endregion

        #region==营业汇总==

        /// <summary>
        /// 营业汇总
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderSummary(PatientEntity entity)
        {
            if (entity.UserName == "搜索") entity.UserName = "";
            int rows;
            int pagecount;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 10; //每页显示条数
            entity.s_StareTime = Convert.ToDateTime("1970-01-01");
            entity.s_Endtime = Convert.ToDateTime("1970-01-01");
            if (entity.orderField + "" == "")
            {
                entity.orderField = "CreateTime";
            }

            if (entity.IsActive == 0)
            {
                entity.IsActive = 1;
            }

            var list = IpatBLL.GetPatientList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            return View(result);
        }

        #endregion

        #region ==当日营业汇总==

        /// <summary>
        /// 废弃接口
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult TodaySummaryOld(_SeachEntity model)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (model.s_HospitalID == 0)
            {
                model.s_HospitalID = LoginUserEntity.HospitalID;
            }

            ViewBag.HospitalID = model.s_HospitalID;
            var stime = model.s_StareTime == DateTime.MinValue
                ? DateTime.Now.ToString("yyyy-MM-dd 00:00:00")
                : model.s_StareTime.ToString("yyyy-MM-dd 00:00:00");
            var etime = model.s_Endtime == DateTime.MinValue
                ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59")
                : model.s_Endtime.ToString("yyyy-MM-dd 23:59:59");
            ViewBag.s_StareTime = stime;
            ViewBag.s_Endtime = etime;

            int userid = model.s_HospitalID;
            if (userid == 0) userid = LoginUserEntity.HospitalID;
            TodaySummaryEntity entity = new TodaySummaryEntity();
            var info = cashBll.GetCash(stime, etime, userid);
            entity.cash = info.SumPayMoney; //现金
            entity.AllXianJin = info.AllXianJin;
            entity.AllYinLian = info.AllYinLian;
            entity.Arrears = cashBll.GetArrears(stime, etime, userid); //欠缺
            entity.Free = cashBll.GetFree(stime, etime, userid);
            int xinke = 0;
            int oldke = 0;
            cashBll.GetGuestNumber(stime, etime, userid, out xinke, out oldke);
            entity.GuestNumber = xinke; //新客
            entity.OldGuestNumber = cashBll.GetCountoldPatient(stime, etime, userid); //老客
            entity.IntegratedSale = cashBll.GetIntegratedSale(stime, etime, userid);
            entity.NursingBuckleConsume = cashBll.GetNursingBuckleConsume(stime, etime, userid); //划卡消耗
            entity.DanxiangConsume = cashBll.GetDanxiangConsume(stime, etime, userid); //单项护理消耗
            entity.NursingSell = cashBll.GetNursingSell(stime, etime, userid);
            entity.ProductMoney = cashBll.GetProductMoney(stime, etime, userid); //产品
            entity.ProductsBuckleConsume = cashBll.GetProductsBuckleConsume(stime, etime, userid);
            entity.Recharge = cashBll.GetRecharge(stime, etime, userid);
            entity.StoredCard = cashBll.GetStoredCard(stime, etime, userid); //卡扣业绩
            entity.StoredSale = cashBll.GetStoredSale(stime, etime, userid);
            entity.TreatmentSale = cashBll.GetTreatmentSale(stime, etime, userid);
            entity.UnionPayCard = cashBll.GetUnionPayCard(stime, etime, userid); //券类
            //今日营业开支
            entity.DayPay = cashBll.GetDayPayByDate(new DayPayEntity { HospitalID = userid, StartTime = Convert.ToDateTime(stime), EndTime = Convert.ToDateTime(etime) });
            entity.allprice = cashBll.Getallprice(Convert.ToDateTime(stime), Convert.ToDateTime(etime), userid);
            //实收现金
            entity.allprice = entity.allprice + entity.DayPay;
            return View(entity);
        }

        /// <summary>
        ///当天营业汇总-新接口
        /// </summary>
        /// <returns></returns>
        public ActionResult TodaySummary(TimeConsumingPerformanceEntity entity)
        {
            entity.StartTime = (entity.StartTime == DateTime.MinValue
                ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"))
                : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:00")));
            entity.EndTime = (entity.EndTime == DateTime.MinValue
                ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"))
                : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59")));
            ViewBag.StartTime = entity.StartTime;
            ViewBag.EndTime = entity.EndTime;
            if (entity.HospitalID == 0)
                entity.HospitalID = LoginUserEntity.HospitalID;

            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Huankuan = 0;
            var mod = cashBll.GetTodaySummary(entity, NewKeVersion);
            try
            {
                string strsql = @"select top 100  max(Price) as Price, MAX(OrderNo) AS OrderNo        
                                  from (select max(a.Price) as Price, max(a.OrderNo) as OrderNo                     
                                  from EYB_tb_Order a WITH (NOLOCK) left join EYB_tb_Patient b WITH (NOLOCK) on a.UserID=b.UserID inner join                                  
                                 EYB_tb_Hospital h WITH (NOLOCK) on a.HospitalID=h.ID left join                           
                                 EYB_tb_PaymentOrder p on p.OrderNo=a.OrderNo                               
                                left join EYB_dict_Baseinfo f on p.PayType=f.ID                                    
                                left join EYB_tb_UserCard c on p.CardID=c.ID                                 
                                left join dbo.EYB_tb_MainCard d on c.CardID=d.ID 
                                where  OrderType= 4  and a.HospitalID= "+ entity.HospitalID + @" and 
                                 a.OrderTime between '"+ entity.StartTime  + @"' and '" + entity.EndTime + @"') t group by OrderNo ";
                ViewBag.Huankuan = GetIntegralByHuankuan(strsql, 1);
                string strdaxiangmu = @"select isnull(SUM(Price),0)  as  Price from(  select 
                                    isnull(Sum(Yeji_Daxiangmu),0) as  Price
                                    from dbo.EYB_tb_JoinUser j                                              
                                    inner join dbo.EYB_tb_HospitalUser h on j.UserID=h.UserID left join EYB_tb_Order o on o.OrderNo=j.OrderNo 
                                    where 1=1 and h.IsActive=1 and o.Statu=1   
                                    and h.HospitalID= " + entity.HospitalID + @"
                                    and o.CreateTime between '"+ entity.StartTime  + @"' and '" + entity.EndTime + @"'
                                    group by j.UserID,h.UserName , h.hospitalID,o.hospitalID )aa";
                ViewBag.Yeji_Daxiangmu = GetIntegralByHuankuan(strdaxiangmu, 1);
            }
            catch { }
            
            return View(mod);
        }
        private string GetIntegralByHuankuan(string sqlstr, int filter = 0)
        {

            string month = "0";
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
                    month = date["Price"].ToString();
                }
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return month;

        }

        /// <summary>
        ///店务数据汇总
        /// </summary>
        /// <returns></returns>
        public ActionResult StoreDataSummary(StoreDataSummaryEntity entity)
        {
            var dt = DateTime.Now;
            //本月第一天时间
            var dtFirst = ConvertValueHelper.ConvertDateTimeValueNew(dt.AddDays(1 - (dt.Day)).ToString("yyyy-MM-dd 00:00:00"));
            var o = DateTime.MinValue;
            entity.StartTime = (entity.StartTime == DateTime.MinValue
                ? Convert.ToDateTime(dtFirst)
                : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:00")));
            entity.EndTime = (entity.EndTime == DateTime.MinValue
                ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"))
                : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59")));
            ViewBag.StartTime = entity.StartTime;
            ViewBag.EndTime = entity.EndTime;
            if (entity.HospitalID == 0)
                entity.HospitalID = LoginUserEntity.HospitalID;
          
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            var mod = cashBll.GetStoreDataSummary(entity);
            ViewBag.zskl = mod.YouXiaoKeLiu + mod.XuKeLiu;
            return View(mod);
        }

        #endregion

        #region ==床位管理==

        /// <summary>
        /// 床位管理
        /// </summary>
        /// <returns></returns>
        public ActionResult BedManagement()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            BedManageEntity entity = new BedManageEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            List<BedManageEntity> list = cashBll.GetAllBedList(entity);
            var alllist = new List<BedAppointmentEntity>();
            foreach (var info in list)
            {

                var YYList = cashBll.GetYuYueList(info.ID);
                foreach (var yy in YYList)
                {
                    alllist.Add(yy);
                    info.BList = new List<BedAppointmentEntity>();
                    info.BList.Add(yy);
                }

                info.Count = YYList.Count; //获取当日预约人数
            }

            var result = list.AsQueryable().ToPagedList(1, 10000);
            return View(result);
        }

        /// <summary>
        /// 字符串长度判断和截取
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [HttpPost]
        public static string outstr(string str)
        {
            string s = string.Empty;
            if (!string.IsNullOrEmpty(str))
            {
                s = str.Length > 9 ? str.Substring(0, 8) + "..." : str;
            }

            return s;
        }

        #endregion

        #region ==项目产品单页==

        public ActionResult _Consumer()
        {
            string bedid = Request["bedid"]; //床位ID
            string orderno = Request["orderno"]; //订单编号
            int userid = Convert.ToInt32(Request["userid"]); //用户ID
            ViewBag.BedID = bedid; //吧床位ID传到前台保存

            #region ==用户ID不为空时绑定用户==

            var entity = cashBll.GetAllBalance(new MainCardBalanceEntity { UserID = userid });
            ViewBag.Price = entity.Price;
            ViewBag.Tiems = entity.Tiems;
            ViewBag.UserName = entity.UserName;
            ViewBag.AllPrice = entity.AllPrice;
            ViewBag.UserID = entity.UserID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;

            #endregion


            #region ==订单编号不为空时绑定订单数据到前台==

            //if (!string.IsNullOrEmpty(orderno))  //如果传进来不为空,则表示是预约状态和在处理状态,非空白状态
            //{
            //   // return GetOrderDetail(orderno);
            //}
            //else
            //{

            //}
            return View();

            #endregion
        }

        #endregion

        #region ==床位预约列表==

        public ActionResult AppointmentList()
        {
            int bedid = Convert.ToInt32(Request["bedid"]);
            var list = cashBll.GetYuYueList(bedid);
            var result = list.AsQueryable().ToPagedList(1, 10000); //添加Webdiyer.WebControls.Mvc应用
            return View(result);
        }

        #endregion

        #region==日常费用收支

        /// <summary>
        /// 日常费用收支
        /// </summary>
        /// <returns></returns>
        public ActionResult DayPayList(DayPayEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            int rows;
            int pagecount;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.numPerPage = 50; //每页显示条数
            entity.StartTime = entity.StartTime.Year == 1
                ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"))
                : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:00"));
            entity.EndTime = entity.EndTime.Year == 1
                ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"))
                : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59"));
            if (entity.orderField + "" == "")
            {
                entity.orderField = "CreateTime";
            }

            decimal Shouru;
            decimal Zhichu;
            var list = cashBll.GetAllDayPay(entity, out rows, out pagecount, out Shouru, out Zhichu);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.AllCount = rows;
            ViewBag.Shouru = Shouru;
            ViewBag.Zhichu = Zhichu;
            if (Request.IsAjaxRequest())
                return PartialView("_DayPayList", result);
            return View(result);
        }




        /// <summary>
        /// 日常费用收支---导出
        /// </summary>
        /// <returns></returns>
        public ActionResult DayPayExport(DayPayEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            int rows;
            int pagecount;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.StartTime = entity.StartTime.Year == 1
                ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01"))
                : Convert.ToDateTime(entity.StartTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = entity.EndTime.Year == 1
                ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"))
                : Convert.ToDateTime(entity.EndTime.ToString("yyyy-MM-dd 23:59:59"));
            if (entity.orderField + "" == "")
            {
                entity.orderField = "CreateTime";
            }
            decimal Shouru;
            decimal Zhichu;
            var list = cashBll.GetAllDayPay(entity, out rows, out pagecount, out Shouru, out Zhichu);
            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("费用项目名称"), new DataColumn("收入金额"), new DataColumn("支出金额"), new DataColumn("来往单位名称"),
                new DataColumn("责任人"), new DataColumn("票据号码"), new DataColumn("业务日期"), new DataColumn("备注")
            });
            foreach (DayPayEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["费用项目名称"] = model.Name;
                row["收入金额"] = model.InMoney;
                row["支出金额"] = model.OutMoney;
                row["来往单位名称"] = model.CompanyName;
                row["责任人"] = model.UserName;
                row["票据号码"] = model.PiaoNo;
                row["业务日期"] = model.CreateTime;
                row["备注"] = model.Memo;
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["费用项目名称"] = "合计";
            row1["收入金额"]=Request["txtShouru"];
            row1["支出金额"] = Request["txtZhichu"];
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "JieYu");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "richangfeiyong.xls");
        }


        /// <summary>
        /// 日常费用收支
        /// </summary>
        /// <returns></returns>
        public ActionResult DayPay()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public ActionResult DelDayPay(int ID)
        {
            var result = cashBll.DelDayPay(ID);
            return Json(result);
        }

        /// <summary>
        /// 添加日常费用收支
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddDayPay(DayPayEntity entity)
        {
            entity.OutMoney = -entity.OutMoney;
            entity.HospitalID = LoginUserEntity.HospitalID;
            var result = cashBll.AddDayPay(entity);
            return Json(result);
        }

        #endregion

        #region==月营业报表==

        /// <summary>
        /// 月营业报表
        /// </summary>
        /// <returns></returns>
        public ActionResult MonthSummary()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }

        /// <summary>
        /// 月营业报表查询
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchMonthSummary(DateTime StartTime1, DateTime EndTime1, int HospitalID = 0)
        {
            StringBuilder str = new StringBuilder();
            if (HospitalID == 0)
            {
                HospitalID = LoginUserEntity.HospitalID;
            }

            ViewBag.HospitalID = HospitalID;

            int ordercount = 0;
            decimal AllPrice = 0;
            decimal XianJin = 0;
            decimal Yinlian = 0;
            decimal OtherPrice = 0;
            decimal ProductPrice = 0;
            decimal Xiaohao = 0;
            decimal Kakou = 0;
            decimal Zengka = 0;
            decimal QianPrice = 0;
            int NewKe = 0;
            decimal NewKePrice = 0;
            int OldKe = 0;
            decimal OldKePrice = 0;
            decimal DayPay = 0;
            var list = cashBll.GetNewMonthSummary(HospitalID, StartTime1.ToString("yyyy-MM-dd"),
                EndTime1.ToString("yyyy-MM-dd"), NewKeVersion);
            str.Append("<tr style='height:40px;'>");
            str.Append("</tr>");
            foreach (var info in list)
            {

                str.Append("<tr>");
                str.AppendFormat("<td><div >{0}</div></td>", info.nowdatetime);

                str.AppendFormat("<td><div >{0}</div></td>", info.ordercount);
                str.AppendFormat("<td><div >{0}</div></td>", info.AllPrice);
                str.AppendFormat("<td><div >{0}</div></td>", info.XianJin);
                str.AppendFormat("<td><div >{0}</div></td>", info.Yinlian);
                str.AppendFormat("<td><div >{0}</div></td>", info.AllPrice - info.XianJin - info.Yinlian);
                str.AppendFormat("<td><div >{0}</div></td>", info.ProductPrice);
                str.AppendFormat("<td><div >{0}</div></td>", info.Xiaohao+info.Danxiangxiaohao);
                str.AppendFormat("<td><div >{0}</div></td>", info.Kakou);
                str.AppendFormat("<td><div >{0}</div></td>", info.Zengka);

                str.AppendFormat("<td><div>{0}</div></td>", info.QianPrice);
                str.AppendFormat("<td><div >{0}</div></td>", info.NewKe);
                str.AppendFormat("<td><div >{0}</div></td>", info.NewKePrice);
                str.AppendFormat("<td><div >{0}</div></td>", info.OldKe);

                str.AppendFormat("<td><div >{0}</div></td>", info.OldKePrice);
                str.AppendFormat("<td><div >{0}</div></td>", info.DayPay);
                str.Append("</tr>");
                ordercount += info.ordercount;
                AllPrice += info.AllPrice;
                XianJin += info.XianJin;
                Yinlian += info.Yinlian;
                OtherPrice += AllPrice - XianJin - Yinlian;
                //于 2016-03-17 edit by kuangsg
                ProductPrice += info.ProductPrice; // info.XianJin + info.Kakou;
                Xiaohao += info.Xiaohao+info.Danxiangxiaohao;
                Kakou += info.Kakou;
                Zengka += info.Zengka;
                QianPrice += info.QianPrice;
                NewKe += info.NewKe;
                NewKePrice += info.NewKePrice;
                OldKe += info.OldKe;
                OldKePrice += info.OldKePrice;
                DayPay += info.DayPay;

            }

            str.Append("<tr>");
            str.AppendFormat("<td><div ></div></td>");
            //合计
            str.AppendFormat("<td><div >合计</div></td>");
            str.AppendFormat("<td><div >{0}</div></td>", AllPrice);
            str.AppendFormat("<td><div >{0}</div></td>", XianJin);
            str.AppendFormat("<td><div >{0}</div></td>", Yinlian);
            str.AppendFormat("<td><div >{0}</div></td>", AllPrice - XianJin - Yinlian);

            str.AppendFormat("<td><div >{0}</div></td>", ProductPrice);
            str.AppendFormat("<td><div >{0}</div></td>", Xiaohao);
            str.AppendFormat("<td><div >{0}</div></td>", Kakou);
            str.AppendFormat("<td><div >{0}</div></td>", Zengka);

            str.AppendFormat("<td><div>{0}</div></td>", QianPrice);
            str.AppendFormat("<td><div >{0}</div></td>", NewKe);
            str.AppendFormat("<td><div >{0}</div></td>", NewKePrice);
            str.AppendFormat("<td><div >{0}</div></td>", OldKe);

            str.AppendFormat("<td><div >{0}</div></td>", OldKePrice);
            str.AppendFormat("<td><div >{0}</div></td>", DayPay);
            str.Append("</tr>");

            return Content(str.ToString());
        }

        #endregion

        #region ==导出==

        /// <summary>
        /// 导出订单列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public FileContentResult ExportOrderList(OrderEntity entity)
        {

            if (entity.UserName == "姓名、订单号、会员卡号") entity.UserName = "";
            entity.StartTime = (entity.StartTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01")
                : Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = (entity.EndTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59")
                : Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59"));

            entity.RuHuiStartTime = (entity.RuHuiStartTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01")
                : Convert.ToDateTime(entity.RuHuiStartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.RuHuiEndTime = (entity.RuHuiEndTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59")
                : Convert.ToDateTime(entity.RuHuiEndTime).ToString("yyyy-MM-dd 23:59:59"));
            int rows;
            int pagecount;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "")
            {
                entity.orderField = "DocumentNumber";
                entity.orderDirection = "desc";
            }

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

            bool HasPermission1 =
                Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine
                    .TransferofworkManager_模拟账号); //模拟账号控制查看权限
            if (HasPermission1)
            {
                entity.IsShow = 2;
            }

            var list = cashBll.GetAllOrderList(entity, out rows, out pagecount);

            var newlist = new List<OrderEntity>();
            decimal Price = 0;
            decimal Xianjin = 0;
            decimal Yinlianka = 0;
            decimal Chuzhika = 0;
            decimal Zhifubao = 0;
            decimal Weixin = 0;
            decimal Huakouka = 0;
            decimal OtherPay = 0;
            decimal QianPrice = 0;
            decimal OtherXianjin = 0;
            decimal ZengChuzhika = 0;
            decimal zongxianjin = 0;
            //var sumList = cashBll.GetSumOrderForEveryPrice(entity);//获取

            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("流水号"), new DataColumn("业务类型"), new DataColumn("卡号"), new DataColumn("姓名"),
                new DataColumn("订单金额"), new DataColumn("现金"), new DataColumn("刷卡"), new DataColumn("支付宝"),
                new DataColumn("微信"), new DataColumn("其他现金"),new DataColumn("总现金"), new DataColumn("储值卡划扣"),
                new DataColumn("赠卡划扣"),new DataColumn("疗程消耗"), new DataColumn("券类"), new DataColumn("欠款")
            });
            foreach (OrderEntity model in list)
            {
                Price = Price + model.Price;
                Xianjin = Xianjin + model.Xianjin;
                Yinlianka = Yinlianka + model.Yinlianka;
                Chuzhika = Chuzhika + model.Chuzhika;
                Weixin = Weixin + model.Weixin;
                Zhifubao = Zhifubao + model.Zhifubao;

                Huakouka = Huakouka + model.Huakouka;
                OtherPay = OtherPay + model.OtherPay;
                QianPrice = QianPrice + model.QianPrice;
                ZengChuzhika = ZengChuzhika + model.ZengChuzhika;
                OtherXianjin = OtherXianjin +
                               (model.SumMoney - model.Xianjin - model.Yinlianka - model.Zhifubao - model.Weixin);
                zongxianjin = zongxianjin + model.SumMoney;
                DataRow row = tableExport.NewRow();
                row["流水号"] = model.DocumentNumber;
                row["业务类型"] = EYB.Web.Code.PageFunction.GetorderType(model.OrderType);
                row["卡号"] = model.MemberNo;
                row["姓名"] = model.UserName;
                row["订单金额"] = model.Price;
                row["现金"] = model.Xianjin;
                row["刷卡"] = model.Yinlianka;
                row["支付宝"] = model.Zhifubao;
                row["微信"] = model.Weixin;
                row["其他现金"] = model.SumMoney - model.Xianjin - model.Yinlianka - model.Zhifubao - model.Weixin;
                row["总现金"] = model.SumMoney;
                row["储值卡划扣"] = model.Chuzhika;
                row["赠卡划扣"] = model.ZengChuzhika;
                row["疗程消耗"] = model.Huakouka;
                row["券类"] = model.OtherPay;
                row["欠款"] = model.QianPrice;
                tableExport.Rows.Add(row);
            }

            DataRow row1 = tableExport.NewRow();
            row1["流水号"] = null;
            row1["业务类型"] = null;
            row1["卡号"] = null;
            row1["姓名"] = "合计:";
            //row1["现金"] = sumList.Where(i => i.PayType == 1).Count() > 0 ? sumList.Where(i => i.PayType == 1).ToList()[0].SumMoney : 0;
            //row1["刷卡"] = sumList.Where(i => i.PayType == 2).Count() > 0 ? sumList.Where(i => i.PayType == 2).ToList()[0].SumMoney : 0;
            //row1["储值卡划扣"] = sumList.Where(i => i.PayType == 3).Count() > 0 ? sumList.Where(i => i.PayType == 3).ToList()[0].SumMoney : 0;
            //row1["疗程卡划扣"] = sumList.Where(i => i.PayType == 4).Count() > 0 ? sumList.Where(i => i.PayType == 4).ToList()[0].SumMoney : 0;
            //row1["其它支付"] = sumList.Where(i => i.PayType > 4).Count() > 0 ? sumList.Where(i => i.PayType > 4).ToList().Sum(z => z.SumMoney) : 0;
            //row1["欠款"] = list.Where(i => i.QianPrice < 0).Sum(i => i.QianPrice);
            row1["订单金额"] = Price;
            row1["现金"] = Xianjin;
            row1["刷卡"] = Yinlianka;
            row1["支付宝"] = Zhifubao;
            row1["微信"] = Weixin;
            row1["其他现金"] = OtherXianjin;
            row1["总现金"] = zongxianjin;
            row1["储值卡划扣"] = Chuzhika;
            row1["赠卡划扣"] = ZengChuzhika;
            row1["疗程消耗"] = Huakouka;
            row1["券类"] = OtherPay;
            row1["欠款"] = QianPrice;

            tableExport.Rows.Add(row1);


            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "OrderList");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "OrderList.xls");
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="StartTime1"></param>
        /// <param name="EndTime1"></param>
        /// <param name="HospitalID"></param>
        /// <returns></returns>
        public ActionResult ExportMonthSummaryList(DateTime StartTime1, DateTime EndTime1, int HospitalID = 0)
        {
            StringBuilder str = new StringBuilder();
            str.Append("<table class='gridTable'  style='width:100%;border-collapse:collapse;'>");
            str.Append(
                "<thead><tr><th style='border:1px solid #ccc;' >日期</th><th style='border:1px solid #ccc;'>总人次</th><th style='border:1px solid #ccc;'>总业绩</th><th style='border:1px solid #ccc;'>现金</th> <th style='border:1px solid #ccc;'>银联</th><th style='border:1px solid #ccc;'>其他现金</th><th style='border:1px solid #ccc;'>产品</th><th style='border:1px solid #ccc;'>项目</th><th style='border:1px solid #ccc;'> 卡扣</th><th style='border:1px solid #ccc;'>赠卡业绩</th><th style='border:1px solid #ccc;'>当日欠款</th><th style='border:1px solid #ccc;'>新客人次</th><th  style='border:1px solid #ccc;'>新客业绩</th><th style='border:1px solid #ccc;'>老客人次</th><th style='border:1px solid #ccc;'>老客业绩</th><th style='border:1px solid #ccc;'>费用支出</th></tr></thead>");
            str.Append("<tbody>");
            if (HospitalID == 0)
            {
                HospitalID = LoginUserEntity.HospitalID;
            }

            int ordercount = 0;
            decimal AllPrice = 0;
            decimal XianJin = 0;
            decimal Yinlian = 0;
            decimal OtherPrice = 0;
            decimal ProductPrice = 0;
            decimal Xiaohao = 0;
            decimal Kakou = 0;
            decimal Zengka = 0;
            decimal QianPrice = 0;
            int NewKe = 0;
            decimal NewKePrice = 0;
            int OldKe = 0;
            decimal OldKePrice = 0;
            decimal DayPay = 0;
            var list = cashBll.GetNewMonthSummary(HospitalID, StartTime1.ToString("yyyy-MM-dd"),
                EndTime1.ToString("yyyy-MM-dd"), NewKeVersion);
            foreach (var info in list)
            {

                str.Append("<tr>");
                str.AppendFormat("<td><div style='width:40px;'>{0}</div></td>", info.nowdatetime);

                str.AppendFormat("<td><div style='width:40px;'>{0}</div></td>", info.ordercount);
                str.AppendFormat("<td><div >{0}</div></td>", info.AllPrice);
                str.AppendFormat("<td><div >{0}</div></td>", info.XianJin);
                str.AppendFormat("<td><div >{0}</div></td>", info.Yinlian);
                str.AppendFormat("<td><div >{0}</div></td>", info.AllPrice - info.XianJin - info.Yinlian);
                str.AppendFormat("<td><div >{0}</div></td>", info.ProductPrice);
                str.AppendFormat("<td><div >{0}</div></td>", info.Xiaohao);
                str.AppendFormat("<td><div >{0}</div></td>", info.Kakou);
                str.AppendFormat("<td><div >{0}</div></td>", info.Zengka);

                str.AppendFormat("<td><div>{0}</div></td>", info.QianPrice);
                str.AppendFormat("<td><div >{0}</div></td>", info.NewKe);
                str.AppendFormat("<td><div >{0}</div></td>", info.NewKePrice);
                str.AppendFormat("<td><div >{0}</div></td>", info.OldKe);

                str.AppendFormat("<td><div >{0}</div></td>", info.OldKePrice);
                str.AppendFormat("<td><div >{0}</div></td>", info.DayPay);
                str.Append("</tr>");
                ordercount += info.ordercount;
                AllPrice += info.AllPrice;
                XianJin += info.XianJin;
                Yinlian += info.Yinlian;
                OtherPrice += AllPrice - XianJin - Yinlian;
                //于 2016-03-17 edit by kuangsg
                ProductPrice += info.ProductPrice; // info.XianJin + info.Kakou;
                Xiaohao += info.Xiaohao;
                Xiaohao += info.Danxiangxiaohao;
                Kakou += info.Kakou;
                Zengka += info.Zengka;
                QianPrice += info.QianPrice;
                NewKe += info.NewKe;
                NewKePrice += info.NewKePrice;
                OldKe += info.OldKe;
                OldKePrice += info.OldKePrice;
                DayPay += info.DayPay;

            }


            str.Append("<tr>");
            str.AppendFormat("<td style='border:1px solid #ccc;'><div style='width:80px;'></div></td>");
            //查询当天数据

            str.AppendFormat("<td style='border:1px solid #ccc;'><div style='width:80px;'>{0}</div></td>", "合计:");
            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", AllPrice);

            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", XianJin);
            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", Yinlian);
            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", AllPrice - XianJin - Yinlian);

            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", ProductPrice);
            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", Xiaohao);
            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", Kakou);
            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", Zengka);

            str.AppendFormat("<td style='border:1px solid #ccc;'><div>{0}</div></td>", QianPrice);
            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", NewKe);
            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", NewKePrice);
            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", OldKe);

            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", OldKePrice);
            str.AppendFormat("<td style='border:1px solid #ccc;'><div >{0}</div></td>", DayPay);
            str.Append("</tr>");


            str.Append("</tbody>");
            str.Append("</table>");
            JsonModelEntity model = new JsonModelEntity();
            model.OutputHtml = str.ToString();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "utf-8";
            Response.ContentType = "application/vnd.ms-excel";
            Response.AppendHeader("Content-Disposition",
                "attachment;filename= MonthSummary" + StartTime1.ToString("yyyy-MM-dd") + "to" +
                EndTime1.ToString("yyyy-MM-dd") + ".xls");
            Response.ContentEncoding = Encoding.GetEncoding("utf-8");
            Response.Write(str.ToString());

            Response.Flush();
            Response.End();
            return null;

        }

        #endregion

        #region==寄存==

        /// <summary>
        /// 寄存管理
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductJichun()
        {


            return View();
        }

        /// <summary>
        /// 寄存
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductJichunIn(JiChunEntity entity)
        {
            //默认查询启用状态的
            if (entity.IsActive == 0)
            {
                entity.IsActive = 1;
                //标识未领取
                entity.ShengYuCount = 1;
            }

            if (entity.IsActive == 999)
            {
                entity.IsActive = 0;
            }

            int rows;
            int pagecount;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.Type = 1;
            entity.JichunStartTime = entity.JichunStartTime.Year == 1
                ? Convert.ToDateTime(DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd 00:00:01"))
                : Convert.ToDateTime(entity.JichunStartTime.ToString("yyyy-MM-dd 00:00:01"));
            ViewBag.JichunStartTime = entity.JichunStartTime.ToString("yyyy-MM-dd");
            entity.JichunEndTime = entity.JichunEndTime.Year == 1
                ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"))
                : Convert.ToDateTime(entity.JichunEndTime.ToString("yyyy-MM-dd 23:59:59"));
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "")
            {
                entity.orderField = "CreateTime";
            }

            int JichunCount = 0;
            int LingquCount = 0;
            int ShengYuCount = 0;
            var list = iwareBLL.GetJiChunList(entity, out rows, out pagecount, out JichunCount, out LingquCount,
                out ShengYuCount);
            var result = list.ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.IsActive = entity.IsActive;
            ViewBag.JichunCount = JichunCount;
            ViewBag.LingquCount = LingquCount;
            ViewBag.ShengYuCount = ShengYuCount;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_ProductJichunIn", result);
            }
            else
            {

                return View(result);
            }
        }
        ///<summary>
        ///寄存导出
        ///</summary>
        ///<returns></returns>
        public ActionResult ExportProductJichunIn(JiChunEntity entity)
        {
            entity.JichunStartTime = Convert.ToDateTime(Request["JichunStartTime"]);
            entity.JichunEndTime = Convert.ToDateTime(Convert.ToDateTime(Request["JichunEndTime"]).ToString("yyyy-MM-dd 23:59:59"));
            entity.HospitalID = Convert.ToInt32(Request["HospitalID"]);
            entity.IsActive = Convert.ToInt32(Request["IsActive"]);
            entity.UserName = Request["UserName"];
            entity.ProductName = Request["ProductName"];
            entity.numPerPage = Convert.ToInt32(Request["numPerPage"]); //每页显示条数
            entity.Type = 1;//寄存
            int rows;
            int pagecount;
            int JichunCount = 0;
            int LingquCount = 0;
            int ShengYuCount = 0;
            var list = iwareBLL.GetJiChunList(entity, out rows, out pagecount, out JichunCount, out LingquCount, out ShengYuCount);

            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("单据号"), new DataColumn("姓名"), new DataColumn("卡号"), new DataColumn("寄存时间"),
                new DataColumn("产品名称"), new DataColumn("寄存类型"), new DataColumn("寄存数量"), new DataColumn("已领取数量"),
                new DataColumn("剩余数量"), new DataColumn("备注")
            });
            foreach (JiChunEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["单据号"] = model.OrderNo;
                row["姓名"] = model.UserName;
                row["卡号"] = model.MemberNo;
                row["寄存时间"] = model.JichunTime;
                row["产品名称"] = model.ProductName;
                row["寄存类型"] = model.JiChunType;
                row["寄存数量"] = model.JichunCount;
                row["已领取数量"] = model.LingquCount;
                row["剩余数量"] = model.ShengYuCount;
                row["备注"] = model.Memo;
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["单据号"] = null;
            row1["姓名"] = null;
            row1["卡号"] = null;
            row1["寄存时间"] = null;
            row1["产品名称"] = null;
            row1["寄存类型"] = null;
            row1["寄存数量"] = JichunCount;
            row1["已领取数量"] = LingquCount;
            row1["剩余数量"] = ShengYuCount;
            row1["备注"] = null;
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "ProductJichunIn");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "ProductJichunIn.xls");
        }

        /// <summary>
        /// 欠货
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductJichunOut(JiChunEntity entity)
        {
            //默认查询启用状态的
            if (entity.IsActive == 0)
            {
                entity.IsActive = 1;
                //标识未领取
                entity.ShengYuCount = 1;
            }

            if (entity.IsActive == 999)
            {
                entity.IsActive = 0;
            }

            int rows;
            int pagecount;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.Type = 2;
            entity.JichunStartTime = entity.JichunStartTime.Year == 1
                ? Convert.ToDateTime(DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd 00:00:01"))
                : Convert.ToDateTime(entity.JichunStartTime.ToString("yyyy-MM-dd 00:00:01"));
            ViewBag.JichunStartTime = entity.JichunStartTime.ToString("yyyy-MM-dd");
            entity.JichunEndTime = entity.JichunEndTime.Year == 1
                ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"))
                : Convert.ToDateTime(entity.JichunEndTime.ToString("yyyy-MM-dd 23:59:59"));
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "")
            {
                entity.orderField = "CreateTime";
            }

            int JichunCount = 0;
            int LingquCount = 0;
            int ShengYuCount = 0;
            var list = iwareBLL.GetJiChunList(entity, out rows, out pagecount, out JichunCount, out LingquCount,
                out ShengYuCount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.IsActive = entity.IsActive;
            ViewBag.JichunCount = JichunCount;
            ViewBag.LingquCount = LingquCount;
            ViewBag.ShengYuCount = ShengYuCount;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_ProductJichunOut", result);
            }
            else
            {

                return View(result);
            }
        }

        /// <summary>
        /// 添加寄存页面
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductJichunInAdd()
        {
            return View();
        }

        /// <summary>
        /// 领取页面
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductJichunLingqu()
        {
            return View();
        }

        /// <summary>
        /// 领取
        /// </summary>
        /// <returns></returns>
        public JsonResult AddProductJichunLingqu(JichunDetailEntity entity, string Orderno, string Type, string hiddenToken)
        {
            int result = 0;
            //防止重复提交
            if (Session["hiddenToken"] == null)
            {
                Session["hiddenToken"] = hiddenToken;
            }
            else
            {
                if (Session["hiddenToken"].ToString() == hiddenToken)
                {
                    // LogWriter.WriteInfo("重复支付记录" + entity.OrderNo, "重复支付记录");
                    return Json(AjaxResult.Error("网络延迟，请勿重复提交！"));
                }
            }

            if (entity.LingquCount <= 0)
                return Json(AjaxResult.Error("请输入领取数量！"));

            //欠货领取
            if (Type == "2")
            {
                //1、查看库存是否足够
                var warelist = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = LoginUserEntity.HospitalID }).Where(c => c.IsActive == 1).ToList();
                ProductStockEntity pse = new ProductStockEntity();
                if (pse.HospitalID == 0)
                {
                    pse.HospitalID = LoginUserEntity.HospitalID;
                }

                pse.HouseID = warelist[0].ID;
                pse.ProductID = entity.ProductID;
                pse = iwareBLL.GetProductStockListToseleect(pse)[0];
                //获取产品寄存数量
                int procount = iwareBLL.GetProductJiChunCount(new JiChunEntity { ProductID = pse.ProductID, HospitalID = pse.HospitalID });
                //判断领取数量是否大于欠货数量
                var jichunrecordcount = iwareBLL.GetJichunRecordList(new JichunRecordEntity { JichunID = entity.JiChunID, ProductID = entity.ProductID }).Sum(c => c.LingquCount);
                if (jichunrecordcount >= entity.JichunCount)
                {
                    return Json(AjaxResult.Error("已领取，请勿重复领取！"));
                }

                //如果库存数量小于要领取的数量
                if (entity.LingquCount > (procount + pse.Quatity))
                {
                    return Json(AjaxResult.Error("库存不足！"));
                }
                else
                {
                    //新建出库单
                    StockOrderEntity soe = new StockOrderEntity();
                    var thetype = sysbll.GetBaseInfoTreeByType("WarehouseIn", 1, LoginUserEntity.ParentHospitalID)
                        .Where(c => c.InfoName == "销售出库").ToList(); //获取库存原因
                    soe.BaseID = thetype.Count > 0 ? thetype[0].ID : 0;
                    soe.OrderNo = RandomHelper.GetRandomStock("C");
                    soe.Class = 6;
                    soe.HospitalID = LoginUserEntity.HospitalID;
                    soe.IsVerify = 2;
                    soe.Memo = "领取欠货出库";
                    soe.OrderTime = DateTime.Now;
                    soe.UserID = LoginUserEntity.UserID;

                    soe.VerifyTime = DateTime.Now;
                    soe.VerifyUserID = LoginUserEntity.UserID;
                    soe.DocumentNumber = GetDocumentNumber(soe.HospitalID, 1);
                    soe.VerifyMemo = Orderno;
                    soe.Type = 1;
                    soe.AllQuatity = entity.LingquCount;
                    var pmodel = baseinfoBLL.GetModel(pse.ProductID);

                    soe.AllMoney = pmodel.RetailPrice * entity.LingquCount;

                    soe.AllChengBen = pmodel.CostPrice * entity.LingquCount;
                    iwareBLL.AddStockOrder(soe); //添加出库单
                    //新建出库单详情
                    StockOrderInfo stoinfo = new StockOrderInfo();
                    stoinfo.StockOrderNo = soe.OrderNo;
                    stoinfo.ProductID = pse.ProductID;
                    stoinfo.HouseID = pse.HouseID;
                    stoinfo.Number = entity.LingquCount;
                    iwareBLL.AddStockOrderInfo(stoinfo); //添加出库详情

                    //新建出库日志
                    StockChangeLogEntity scl = new StockChangeLogEntity();
                    scl.UserID = LoginUserEntity.UserID;
                    scl.LogClass = 5;
                    scl.OwnedWarehouse = pse.HouseID;
                    scl.IntoWarehouse = 0;
                    scl.ProductID = entity.ProductID;
                    scl.LogType = "出库";
                    scl.LogTitle = "领取欠货.仓库ID:" + pse.HouseID + "; 产品ID:" + entity.ProductID + ";出库数量:" + entity.LingquCount + ";";
                    scl.NewQuatity = pse.Quatity - entity.LingquCount;
                    scl.OldQuatity = pse.Quatity;
                    scl.Quantity = entity.LingquCount;
                    scl.ProductStockID = pse.ID;
                    scl.OrderNo = soe.OrderNo;
                    var sclID = iwareBLL.AddStockChangeLog(scl);

                    pse.Quatity = pse.Quatity - entity.LingquCount;
                    //更新产品库存
                    iwareBLL.UpdateProductStock(pse);
                    //添加领取记录
                    entity.ChukuOrderNo = soe.OrderNo;

                    result = iwareBLL.AddJiChunProductLingqu(entity);
                }
            }
            else
            {
                var modelJichun = iwareBLL.GetJiChunDetail(entity.JiChunID, entity.ProductID);
                if (modelJichun.ID == 0)
                    return Json(AjaxResult.Error("寄存信息参数错误！"));

                if (entity.LingquCount > modelJichun.ShengYuCount)
                    return Json(AjaxResult.Error("领取数量不能大于剩余数量！"));

                if (modelJichun.JiChunType == 0)
                {
                    var jichunRecods = iwareBLL.GetJichunRecordList(new JichunRecordEntity() { JichunID = modelJichun.ID, ProductID = modelJichun.ProductID });
                    if (jichunRecods.Any(x => x.AuditState == 1))
                    {
                        return Json(AjaxResult.Error("当前寄存有领取审核，暂时无法领取"));
                    }
                }

                //寄存领取
                result = iwareBLL.AddJiChunProductLingqu(entity);

                Json(AjaxResult.Success("操作成功，请等待审核。"));
            }

            if (result <= 0)
            {
                return Json(AjaxResult.Success("领取失败"));
            }

            return Json(AjaxResult.Success("领取成功"));
        }

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
                case 1:
                    result = "C";
                    break;
                case 2:
                    result = "R";
                    break;
                case 3:
                    result = "D";
                    break;
                case 4:
                    result = "P";
                    break;
                case 5:
                    result = "Y";
                    break;
                default: break;
            }

            result += DateTime.Today.ToString("yyyyMMdd");
            result += (number + 1).ToString().PadLeft(3, '0');
            return result;
        }

        /// <summary>
        /// 删除寄存单据
        /// </summary>
        /// <returns></returns>
        public ActionResult DelJiChun(int ID, int IsActive)
        {
            var result = iwareBLL.DelJiChun(ID, IsActive);
            return Json(result);
        }

        /// <summary>
        /// 领取记录
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductJichunLingquRecord(JichunRecordEntity entity)
        {
            //var JiChunID = Request["JiChunID"];
            //var ProductID = Request["ProductID"];
            var list = iwareBLL.GetJichunRecordList(entity);

            var modelJicun = iwareBLL.GetJiChun(entity.JichunID);
            ViewBag.Audit = modelJicun.ID > 0 && modelJicun.JiChunType == 0 ? 1 : 0;
            ViewBag.JiCunType = modelJicun.Type;
            return View(list);
        }

        /// <summary>
        /// 添加寄存
        /// </summary>
        /// <returns></returns>
        public ActionResult AddProductJichunIn(JiChunEntity entity)
        {
            string[] productid = Request.Form.GetValues("GiveIDList"); //寄存产品ID
            string[] txtinput = Request.Form.GetValues("txtinput"); //寄存数量
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.CreateTime = DateTime.Now;
            entity.JichunTime = DateTime.Now;
            entity.OrderNo = RandomHelper.GetRandomStock("J");
            entity.Type = 1; //寄存
            entity.IsActive = 1;
            //添加寄存主表
            var jichunID = iwareBLL.AddJiChun(entity);
            JichunDetailEntity info = new JichunDetailEntity();
            int result = 0;
            info.JiChunID = jichunID;
            if (productid != null && txtinput != null)
            {
                for (int i = 0; i < productid.Length; i++)
                {
                    info.ProductID = ConvertValueHelper.ConvertIntValue(productid[i]);
                    info.JichunCount = ConvertValueHelper.ConvertIntValue(txtinput[i]);
                    info.LingquCount = 0;
                    info.ShengYuCount = info.JichunCount;
                    //添加寄存详情表
                    result = iwareBLL.AddJiChunProduct(info);
                }
            }
            else
            {
                result = -1;
            }

            return Json(result);
        }

        /// <summary>
        /// 寄存领取记录审核
        /// </summary>
        /// <param name="rId">领取记录id</param>
        /// <returns></returns>
        public ActionResult ProductJichunLingquRecordAudit(int rId)
        {
            var model = iwareBLL.GetJichunRecordById(rId);
            return View(model);
        }

        /// <summary>
        /// 审核领取记录
        /// </summary>
        /// <param name="rId">领取记录id</param>
        /// <param name="audit">审核状态 1通过 0拒绝</param>
        /// <param name="reason">审核原因</param>
        /// <returns></returns>
        public JsonResult _AuditProductJichunLingquRecord(int rId, string reason)
        {

            bool isAuthorize = LoginHospitalUserManager.HasPermission(PermissionDefine.CashManage_Jicunshenhe);
            if (!isAuthorize)
            {
                return Json(AjaxResult.Error("操作失败！没有权限"));
            }


            //拒绝操作需要拒绝理由
            //if (audit == 0 && string.IsNullOrEmpty(reason))
            //    return Json(AjaxResult.Error("缺少拒绝理由"));

            try
            {
                var record = iwareBLL.GetJichunRecordById(rId);
                if (record.Id == 0)
                    return Json(AjaxResult.Error("领取记录不存在"));
                if (record.AuditState != 1)
                    return Json(AjaxResult.Error("领取记录已审核.请勿重复操作"));

                record.AuditState = 2;
                record.AuditReason = reason;
                record.UpdateTime = DateTime.Now;
                iwareBLL.UpdateJichunRecord(record);
            }
            catch (Exception e)
            {
                return Json(AjaxResult.Error("出现异常：" + e.Message));
            }

            return Json(AjaxResult.Success("审核成功"));
        }

        public ActionResult TestAddQianhuo()
        {
            return View();
        }

        /// <summary>
        /// 手动添加欠货
        /// </summary>
        /// <returns></returns>
        public ActionResult Addqianhuo()
        {
            #region==欠货管理==

            //欠货处理
            JiChunEntity jichunmodel = new JiChunEntity();
            jichunmodel.HospitalID = 79; //要改
            jichunmodel.CreateTime = DateTime.Now;
            jichunmodel.JichunTime = DateTime.Now;
            jichunmodel.OrderNo = RandomHelper.GetRandomStock("Q");
            jichunmodel.Type = 2; //欠货
            jichunmodel.UserID = 20790; //要改
            jichunmodel.Memo = "201705272661312947"; //要改
            jichunmodel.IsActive = 1;
            //添加寄存主表
            var jichunID = iwareBLL.AddJiChun(jichunmodel);
            JichunDetailEntity info = new JichunDetailEntity();

            info.JiChunID = jichunID;
            info.JichunCount = 1; //要改
            info.LingquCount = 0;
            info.ProductID = 3825; //要改
            info.ShengYuCount = info.JichunCount;
            //添加寄存详情表
            int result = iwareBLL.AddJiChunProduct(info);

            #endregion

            return Json(result);
        }

        #endregion

        #region==收银对账表==

        /// <summary>
        /// 收银对账表
        /// </summary>
        /// <returns></returns>
        public ActionResult CashCheckList(OrderEntity entity)
        {
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;

            if (entity.UserName == "姓名、订单号、会员卡号") entity.UserName = "";
            entity.StartTime = (entity.StartTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01")
                : Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = (entity.EndTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59")
                : Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59"));

            entity.numPerPage = 50; //每页显示条数
            if (string.IsNullOrEmpty(entity.orderField))
            {
                entity.orderField = "DocumentNumber";
                entity.orderDirection = "desc";
            }

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

            bool HasPermission1 = LoginHospitalUserManager.HasPermission(PermissionDefine.TransferofworkManager_模拟账号); //模拟账号控制查看权限
            if (HasPermission1)
            {
                entity.IsShow = 2;
            }

            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderEntity>().ToPagedList(1, 10));
                }
            }

            decimal QianPrice = 0;
            var list = cashBll.GetAllOrder(entity, out QianPrice);

            var newlist = new List<OrderEntity>();
            foreach (var info in list)
            {
                var infolist = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = info.OrderNo });
                var OrderinfoList = cashBll.GetOrderInfoListXH(info.OrderNo);

                decimal otherpay = 0;
                foreach (var io in infolist)
                {
                    if (OrderinfoList.Count>0)
                    {
                        info.DanXiangXiaoHao = OrderinfoList.Sum(t=>t.Price);
                    }
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
                    else if (io.PayType > 4 && !string.IsNullOrEmpty(io.PayName) && io.PayName.Contains("支付宝"))
                    {
                        info.Zhifubao = io.PayMoney;
                    }
                    else if (io.PayType == 8 && !string.IsNullOrEmpty(io.PayName) && io.PayName.Contains("积分消费"))
                    {
                        info.Jifen = io.PayMoney;
                    }
                    else if (io.PayType > 4 && !string.IsNullOrEmpty(io.PayName) && io.PayName.Contains("微信"))
                    {
                        info.Weixin = io.PayMoney;
                    }
                    else if (io.PayType > 4 && io.ParentPayType == 1 && !string.IsNullOrEmpty(io.PayName) && !io.PayName.Contains("微信") &&
                             !io.PayName.Contains("支付宝"))
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

            var result = newlist.ToPagedList(1, entity.numPerPage);

            var sumList = cashBll.GetSumOrderForEveryPrice(entity); //获取
            ViewBag.xianjin = sumList.Where(i => i.PayType == 1).Any()
                ? sumList.Where(i => i.PayType == 1).Sum(z => z.SumMoney)
                : 0;
            ViewBag.shuaka = sumList.Where(i => i.PayType == 2).Any()
                ? sumList.Where(i => i.PayType == 2).Sum(z => z.SumMoney)
                : 0;
            ViewBag.zhifubao = sumList.Where(i => i.PayType > 4 && i.PayName != null && i.PayName.Contains("支付宝")).Any()
                ? sumList.Where(i => i.PayType > 4 && !string.IsNullOrEmpty(i.PayName) && i.PayName.Contains("支付宝")).Sum(z => z.SumMoney)
                : 0;
            ViewBag.Jifen = sumList.Where(i => i.PayType == 8 && i.PayName != null && i.PayName.Contains("积分消费")).Any()
    ? sumList.Where(i => i.PayType == 8 && !string.IsNullOrEmpty(i.PayName) && i.PayName.Contains("积分消费")).Sum(z => z.SumMoney)
    : 0;
            ViewBag.weixin = sumList.Where(i => i.PayType > 4 && i.PayName != null && i.PayName.Contains("微信")).Any()
                ? sumList.Where(i => i.PayType > 4 && i.PayName != null && !string.IsNullOrEmpty(i.PayName) && i.PayName.Contains("微信")).Sum(z => z.SumMoney)
                : 0;
            ViewBag.miandan = sumList.Where(i => i.PayType > 4 && i.ParentPayType == 1 && (i.PayName == null || (!i.PayName.Contains("微信") && !i.PayName.Contains("支付宝")))).Any()
                    ? sumList.Where(i => i.PayType > 4 && i.ParentPayType == 1 && (i.PayName == null || (!i.PayName.Contains("微信") && !i.PayName.Contains("支付宝")))).Sum(z => z.SumMoney) : 0;
            ViewBag.otherpay = sumList.Where(i => i.ParentPayType == 2).Any()
                ? sumList.Where(i => i.ParentPayType == 2).Sum(z => z.SumMoney)
                : 0;
            // ViewBag.shuaka = sumList.Where(i => i.PayType == 2).Count() > 0 ? sumList.Where(i => i.PayType == 2).ToList()[0].SumMoney : 0;
            ViewBag.zongshouru = ViewBag.xianjin + ViewBag.shuaka + ViewBag.zhifubao + ViewBag.weixin + ViewBag.miandan;
            ViewBag.chuzhi = sumList.Where(i => i.PayType == 3 && i.IsGive == 2).Any()
                ? sumList.Where(i => i.PayType == 3 && i.IsGive == 2).Sum(z => z.SumMoney)
                : 0;
            ViewBag.zengchuzhi = sumList.Where(i => i.PayType == 3 && i.IsGive == 1).Any()
                ? sumList.Where(i => i.PayType == 3 && i.IsGive == 1).Sum(z => z.SumMoney)
                : 0;
            ViewBag.liaocheng = sumList.Where(i => i.PayType == 4).Any()
                ? sumList.Where(i => i.PayType == 4).Sum(z => z.SumMoney)
                : 0;
            //ViewBag.qita = sumList.Where(i => i.PayType > 4).Count() > 0 ? sumList.Where(i => i.PayType > 4).ToList().Sum(z => z.SumMoney) : 0;
            ViewBag.qiankuan = QianPrice; // sumList.Where(i => i.QianPrice < 0).Sum(i => i.QianPrice);
             var XHSumList = cashBll.GetEYB_tb_Orderinfo_GetOrderinfoListXHSum(entity.StartTime,entity.EndTime,entity.HospitalID);
            ViewBag.XHSum = XHSumList.Sum(t=>t.Price);
            result.TotalItemCount = entity.Rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.OrderType = entity.OrderType;
            ViewBag.StartTime = Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd");
            ViewBag.EndTime = Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd");

            if (Request.IsAjaxRequest())
                return PartialView("_CashCheckList", result);
            return View(result);
        }


        /// <summary>
        /// 导出收银对账表
        /// </summary>
        /// <returns></returns>
        public FileContentResult ExportCashCheckList(OrderEntity entity)
        {
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;

            if (entity.UserName == "姓名、订单号、会员卡号") entity.UserName = "";
            entity.StartTime = (entity.StartTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 00:00:01")
                : Convert.ToDateTime(entity.StartTime).ToString("yyyy-MM-dd 00:00:01"));
            entity.EndTime = (entity.EndTime == null
                ? DateTime.Now.ToString("yyyy-MM-dd 23:59:59")
                : Convert.ToDateTime(entity.EndTime).ToString("yyyy-MM-dd 23:59:59"));
            int rows;
            int pagecount;

            entity.numPerPage = 50000; //每页显示条数
            if (entity.orderField + "" == "")
            {
                entity.orderField = "DocumentNumber";
                entity.orderDirection = "desc";
            }

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

            bool HasPermission1 =
                Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine
                    .TransferofworkManager_模拟账号); //模拟账号控制查看权限
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
                var OrderinfoList = cashBll.GetOrderInfoListXH(info.OrderNo);

                decimal otherpay = 0;
                foreach (var io in infolist)
                {
                    if (io.PayName == null) io.PayName = "";
                    if (OrderinfoList.Count > 0)
                    {
                        info.DanXiangXiaoHao = OrderinfoList.Sum(t => t.Price);
                    }
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
                    else if (io.PayType > 4 && io.ParentPayType == 1 && !io.PayName.Contains("微信") &&
                             !io.PayName.Contains("支付宝"))
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

            decimal Price = 0;
            decimal Xianjin = 0;
            decimal Yinlianka = 0;
            decimal Chuzhika = 0;
            decimal Huakouka = 0;
            decimal OtherPay = 0;

            decimal OtherXianjin = 0;
            decimal ZengChuzhika = 0;
            decimal Zhifubao = 0;
            decimal Weixin = 0;
            decimal Allpirce = 0;
            decimal Qiankuan1 = 0;
            decimal DanXiangXiaoHao = 0;
            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("流水号"), new DataColumn("业务类型"), new DataColumn("卡号"), new DataColumn("姓名"),
                new DataColumn("现金"), new DataColumn("刷卡"), new DataColumn("支付宝"), new DataColumn("微信"),
                new DataColumn("其他现金"), new DataColumn("总收入"), new DataColumn("储值卡划扣"), new DataColumn("赠卡划扣"),
                new DataColumn("疗程消耗"), new DataColumn("券类"), new DataColumn("单次消费"), new DataColumn("欠款")
            });
            foreach (OrderEntity model in newlist)
            {
                Price = Price + model.Price;
                Xianjin = Xianjin + model.Xianjin;
                Zhifubao = Zhifubao + model.Zhifubao;
                Weixin = Weixin + model.Weixin;
                Allpirce = Allpirce + model.Xianjin + model.Yinlianka + model.Zhifubao + model.Weixin + model.MianDan;
                Yinlianka = Yinlianka + model.Yinlianka;
                Chuzhika = Chuzhika + model.Chuzhika;
                Huakouka = Huakouka + model.Huakouka;
                OtherPay = OtherPay + model.OtherPay;
                Qiankuan1 = Qiankuan1 + model.QianPrice;
                ZengChuzhika = ZengChuzhika + model.ZengChuzhika;
                OtherXianjin = OtherXianjin + model.MianDan;
                DanXiangXiaoHao= DanXiangXiaoHao+model.DanXiangXiaoHao;
                DataRow row = tableExport.NewRow();
                row["流水号"] = model.DocumentNumber;
                row["业务类型"] = EYB.Web.Code.PageFunction.GetorderType(model.OrderType);
                row["卡号"] = model.MemberNo;
                row["姓名"] = model.UserName;

                row["现金"] = model.Xianjin;
                row["刷卡"] = model.Yinlianka;
                row["支付宝"] = model.Zhifubao;
                row["微信"] = model.Weixin;
                row["其他现金"] = model.MianDan;
                row["总收入"] = model.Xianjin + model.Yinlianka + model.Zhifubao + model.Weixin + model.MianDan;
                row["储值卡划扣"] = model.Chuzhika;
                row["赠卡划扣"] = model.ZengChuzhika;
                row["疗程消耗"] = model.Huakouka;
                row["券类"] = model.OtherPay;
                row["单次消费"] = model.DanXiangXiaoHao;
                row["欠款"] = model.QianPrice;
               // row["单次消费"] = model.DanXiangXiaoHao;
                tableExport.Rows.Add(row);
            }

            DataRow row1 = tableExport.NewRow();
            row1["流水号"] = null;
            row1["业务类型"] = null;
            row1["卡号"] = null;
            row1["姓名"] = "合计:";


            row1["现金"] = Xianjin;
            row1["刷卡"] = Yinlianka;
            row1["支付宝"] = Zhifubao;
            row1["微信"] = Weixin;
            row1["其他现金"] = OtherXianjin;
            row1["总收入"] = Allpirce;
            row1["储值卡划扣"] = Chuzhika;
            row1["赠卡划扣"] = ZengChuzhika;
            row1["疗程消耗"] = Huakouka;
            row1["券类"] = OtherPay;
            row1["单次消费"] = DanXiangXiaoHao;
            row1["欠款"] = Qiankuan1;

            tableExport.Rows.Add(row1);


            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "OrderList");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "OrderList.xls");


        }

        #endregion


    }
}

