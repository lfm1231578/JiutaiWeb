using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EYB.ServiceEntity.CashEntity;
using System.Text;
using EYB.ServiceEntity.BaseInfo;
using System.Collections;
using Common.Helper;
using HS.SupportComponents;
using EYB.ServiceEntity.PatientEntity;
using System.Threading.Tasks;
using Common.Manager;
using Common.Define;
using System.Data.SqlClient;

namespace EYB.Web.Controllers
{
    /// <summary>
    /// 办卡
    /// </summary>
    public partial class CashManageController : BaseController
    {
        #region ==办卡==
        /// <summary>
        /// 办卡页面
        /// </summary>
        /// <returns></returns>
        public ActionResult PurchaseCard()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        /// <summary>
        /// 获取卡列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCardList()
        {
            int rows;
            int pagecount;
            StringBuilder str = new StringBuilder();
            int type = Convert.ToInt32(Request["type"]);
            if (type > 0)
            {
                MainCardEntity entity = new MainCardEntity();
                entity.Type = type;
                entity.HospitalID = LoginUserEntity.ParentHospitalID;
                entity.numPerPage = 10;
                entity.IsSale = 1;
                IList<MainCardEntity> entityList = new List<MainCardEntity>();
                entityList = baseinfoBLL.GetAllPrePayCard(entity, out rows, out pagecount);

                foreach (MainCardEntity i in entityList)
                {
                    str.Append("<tr><td><input type='checkbox' class='txtcheckbox' value=" + i.ID + " /></td><td><input type='hidden' name='id' class='id' value='" + i.ID + "|" + i.Name + "|" + i.Price + "|" + i.AllTimes + "|" + i.Memo + "'> " + i.Name + "</td><td>" + i.Price + "</td></tr>");
                }

            }
            return Content(str.ToString());

        }

        #region==暂时没用到==
        /// <summary>
        /// 办卡  没用到---暂时没用到
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCard()
        {
            int userid = Convert.ToInt32(Request["txtUserID"]);//获取用户ID
            string[] strval = Request.Form.GetValues("hidval");//获取列表数据 (ID|价格|数量)
            string[] strinfo = Request.Form.GetValues("hidinfo");//获取列表详细(ID~b~c~d)
            string userlist = Request["uslist"];//获取用户列表
            StringBuilder strjson = new StringBuilder();
            decimal allPrice = 0;//一共需要多少钱

            #region ==获取当前订单的总价格=
            foreach (string s in strval)
            {
                string[] info = s.Split('|');
                allPrice = allPrice + Convert.ToDecimal(info[1]);//累加金额
            }
            #endregion


            #region //添加订单主表
            OrderEntity entity = new OrderEntity();
            entity.OrderType = (int)OrderType.购买卡项;
            entity.CreateTime = DateTime.Now;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
            entity.Statu = (int)OrderStatu.待结算;
            entity.OrderType = (int)OrderType.购买卡项;
            entity.Price = allPrice;
            entity.UserID = userid;
            var mendian = personnelBLL.HospitalEntityByID(LoginUserEntity.HospitalID);
            string time = DateTime.Now.ToString("MMdd");
            var count = cashBll.GetTodayOrderCount(LoginUserEntity.HospitalID, DateTime.Now);//获取总数
            entity.DocumentNumber = mendian.Prefix + time + (count + 1).ToString("d3");

            var result = cashBll.AddOrder(entity);
            #endregion

            if (result != null)
            {
                MainCardEntity card = new MainCardEntity();
                OrderinfoEntity orderinfo = new OrderinfoEntity();
                orderinfo.OrderNo = entity.OrderNo;
                entity.HospitalID = LoginUserEntity.UserID;
                orderinfo.Type = 3;
                int k = 0;
                foreach (string s in strval)
                {
                    string[] bindlist = new string[] { };//新定义一个数组
                    string[] info = s.Split('|');//对每条数据分割(第一组数据)

                    int id;//这个是ID
                    string StoredCard;//这个是储值卡的id列表
                    string project;//这个是项目 第一个\ ID|次数 \第三个
                    string product;//这个是产品 第一个\ ID|次数 \第三个

                    #region //循环数据并且解析数据
                    foreach (string f in strinfo)//看看是否能绑定到对应的数据(第二族数据)
                    {
                        string[] list = f.Split('~');//这个是拆分每个组(第二组数据第N列数据分解)
                        if (info[0] == list[0])//如果2组数据第一个数据能相等,说明他们是一组数据
                        { bindlist = list; }
                    }
                    #endregion

                    #region //把详细数据获取出来
                    IList<ProjectProductEntity> pplist = new List<ProjectProductEntity>();
                    if (bindlist.Length == 0)//如果没有获取到数据,就要去后台请求出默认数据
                    {
                        pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = int.Parse(info[0]) });//获取所有的产品对应关系

                    }
                    else//如果有匹配数据,则解析匹配数据
                    {
                        //bindlist {45,"","","" }获取到的是这样的数组
                        id = int.Parse(bindlist[0]);//这个是ID
                        StoredCard = bindlist[1];//这个是储值卡的id列表
                        project = bindlist[2];//这个是项目 第一个/ ID|次数 /第三个
                        product = bindlist[3];//这个是产品 第一个/ ID|次数 /第三个

                        ProjectProductEntity pent = new ProjectProductEntity();
                        //储值卡数据处理
                        string[] scarray = StoredCard.Split('/');    //StoredCard-->96/97/98
                        foreach (string str in scarray)
                        {
                            if (!string.IsNullOrEmpty(str))
                            {
                                pent = cashBll.GetProjectProductModel(new ProjectProductEntity { ID = int.Parse(str), Type = 1 });
                                pplist.Add(pent);
                            }
                        }

                        //项目数据处理
                        string[] projarray = project.Split('/'); //第一组数据的分割
                        foreach (string pro in projarray)    //分割成id|数量
                        {
                            if (!string.IsNullOrEmpty(pro))
                            {
                                string[] proinfo = pro.Split('|');  //在此分割字符串获取ID和数量
                                pent = cashBll.GetProjectProductModel(new ProjectProductEntity { ID = int.Parse(proinfo[0]), Type = 2 });
                                pent.Qualtity = int.Parse(proinfo[1]);
                                pplist.Add(pent);
                            }
                        }

                        //产品数据处理
                        string[] prodarray = product.Split('/');  //分割成每组数据
                        foreach (string prod in prodarray)
                        {
                            if (!string.IsNullOrEmpty(prod))
                            {
                                string[] prodinfo = prod.Split('|');   //再次分割
                                pent = cashBll.GetProjectProductModel(new ProjectProductEntity { ID = int.Parse(prodinfo[0]), Type = 3 });
                                pent.Qualtity = int.Parse(prodinfo[1]);
                                pplist.Add(pent);
                            }
                        }
                    }
                    #endregion

                    #region //添加订单详情(保存到卡项,不用保存到更详细的级别)
                    card = baseinfoBLL.GetPrePayCardModel(Convert.ToInt32(info[0]));//获取卡类基本信息
                    orderinfo.AllPrice = Convert.ToDecimal(info[1]);
                    orderinfo.OldPrice = card.Price;
                    orderinfo.Price = Convert.ToDecimal(info[1]);
                    //orderinfo.CardType=1; 这个不要传
                    orderinfo.CardID = Convert.ToInt32(info[0]);
                    orderinfo.Quantity = 1;//卡数量默认为1
                    orderinfo.ProdcuitID = Convert.ToInt32(info[0]);
                    int orderinfonumber = cashBll.AddOrderinfo(orderinfo);  //获取订单详细ID
                    #endregion

                    #region //逐条添加购买关系
                    UserCardEntity usercard = new UserCardEntity();
                    usercard.CardID = Convert.ToInt32(info[0]);
                    usercard.CreateTime = DateTime.Now;
                    usercard.IsActive = 1;
                    usercard.UserID = userid;
                    // usercard.Price = GetAllPrice(Convert.ToInt32(info[0]), card.Price);//第一个参数是卡的ID,第二个是卡的原有价格
                    usercard.Price = card.Price;
                    long usercardid = cashBll.AddUserCard(usercard);
                    #endregion

                    #region //添加用户余额记录
                    MainCardBalanceEntity cardbalance = new MainCardBalanceEntity();
                    cardbalance.CardID = Convert.ToInt32(info[0]);
                    cardbalance.UserCardID = Convert.ToInt32(usercardid);
                    cardbalance.UserID = userid;
                    cardbalance.Type = 1;
                    IList<ProjectProductEntity> pp = new List<ProjectProductEntity>();

                    //下面添加的是赠卡
                    foreach (ProjectProductEntity ppentity in pplist)
                    {
                        cardbalance.AllPrice = ppentity.Price;//初始化的时候就是这张卡的默认金额
                        cardbalance.AllTiems = ppentity.Qualtity;
                        cardbalance.KouTimes = 0;
                        cardbalance.Price = ppentity.Price;
                        cardbalance.ProjectID = ppentity.ProjectID;
                        cardbalance.Tiems = ppentity.Qualtity;
                        cardbalance.Type = ppentity.Type;
                        cardbalance.UserID = userid;
                        cardbalance.ExpireTime = GetExpireTime(orderinfo.CardID);
                        cashBll.AddMainCardBalance(cardbalance);

                        if (cardbalance.Type == 1)
                        {
                            //用户储值卡余额明细日志
                            cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                            {
                                CardBalanceId = cardbalance.ID,
                                OrderNo = "",
                                Type = (int)MainCardBalanceDetailType.充值,
                                Amount = entity.Price,
                                Balance = cardbalance.Price + entity.Price,
                                OldAmount = cardbalance.Price,
                                CreateTime = DateTime.Now
                            });
                        }
                    }
                    //储值卡时需要把它本身也加入进去
                    MainCardEntity maincard = baseinfoBLL.GetPrePayCardModel(Convert.ToInt32(info[0]));
                    if (maincard.Type == 1)
                    {
                        cardbalance.AllPrice = maincard.Price;
                        cardbalance.AllTiems = 1;
                        cardbalance.CardID = maincard.ID;
                        cardbalance.CostPrice = maincard.Price;
                        cardbalance.KouTimes = 0;
                        cardbalance.Name = "储值卡主卡";
                        cardbalance.Price = maincard.Price;
                        cardbalance.Tiems = 1;
                        cardbalance.Type = maincard.Type;
                        cardbalance.UserID = userid;
                        cardbalance.ExpireTime = GetExpireTime(orderinfo.CardID);
                        cashBll.AddMainCardBalance(cardbalance);

                        //用户储值卡余额明细日志
                        cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                        {
                            CardBalanceId = cardbalance.ID,
                            OrderNo = "",
                            Type = (int)MainCardBalanceDetailType.充值,
                            Amount = entity.Price,
                            Balance = cardbalance.Price + entity.Price,
                            OldAmount = cardbalance.Price,
                            CreateTime = DateTime.Now
                        });
                    }

                    #endregion

                    #region  //添加购买记录
                    BuyRecordEntity buyentity = new BuyRecordEntity();
                    buyentity.BuyID = Convert.ToInt32(usercardid);
                    buyentity.CreateTime = DateTime.Now;
                    buyentity.Price = Convert.ToDecimal(info[1]);
                    buyentity.Quantity = Convert.ToInt32(info[2]);
                    buyentity.Type = 1;
                    buyentity.UserID = userid;
                    cashBll.AddBuyRecord(buyentity);
                    #endregion

                    #region //添加员工业绩
                    string[] joinuser = userlist.Split('$'); //员工业绩列表分割
                    string joinuserlist = joinuser[k];      //第几个就找到第几个员工业绩

                    string[] userinfo = joinuserlist.Split(',');//分割当前业绩员工列表

                    #region 剔除空字符串(用于剔除空字符串)
                    ArrayList al = new ArrayList();
                    for (int i = 0; i < userinfo.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(userinfo[i]))
                        {
                            al.Add(userinfo[i]);
                        }
                    }
                    string[] userinfo1 = new string[al.Count];
                    for (int i = 0; i < userinfo1.Length; i++)
                    {
                        userinfo1[i] = (string)al[i];
                    }

                    #endregion

                    decimal oneprice = orderinfo.Price / userinfo1.Length;
                    foreach (string uinfo in userinfo1)
                    {
                        JoinUserEntity juentity = new JoinUserEntity();
                        juentity.IsZhiding = 0;
                        juentity.OrderInfoID = orderinfonumber;
                        juentity.OrderNo = entity.OrderNo;
                        juentity.OtherTiCheng = 0;
                        juentity.ProjectID = Convert.ToInt32(info[0]);
                        juentity.ServiceTime = 0;
                        juentity.Ticheng = 0;
                        juentity.Type = 3;//为储值卡,所以为3
                        juentity.UserID = Convert.ToInt32(uinfo);
                        juentity.XiaoHao = 0;
                        juentity.Yeji = oneprice;
                        cashBll.AddJoinUser(juentity);
                    }
                    #endregion
                    k++;
                }
                strjson.Append("{cashamout:" + allPrice + ",cardamout:0,allamout:" + allPrice + ",orderNo:" + entity.OrderNo + " }");
            }
            return Json(strjson.ToString());
        }


        /// <summary>
        /// 办卡 (备份)
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCard2()
        {
            int maika = 0;//是否买卡字段 1是购卡 0不是
            string userid = Request["UserID"];//获取办理用户
            string[] IDlist = Request.Form.GetValues("ID");  //获取ID列表
            string[] PriceList = Request.Form.GetValues("Price");//获取价格列表
            string uslist = Request["uslist"];  //获取参与人员集合
            string[] typelist = Request.Form.GetValues("Type");
            string[] Isgive = Request.Form.GetValues("isgive");//是否赠卡列表

            StringBuilder strjson = new StringBuilder();
            decimal allPrice = 0;//一共需要多少钱
            for (int p = 0; p < PriceList.Length; p++)//赠卡的赠送金额不统计进总金额里面
            {
                if (Isgive == null)
                {

                    allPrice += Convert.ToDecimal(PriceList[p]);
                }
                else
                {
                    if (Isgive[p] == "2")
                    {
                        allPrice += Convert.ToDecimal(PriceList[p]);
                    }
                }
            }

            #region //添加订单主表
            OrderEntity entity = new OrderEntity();
            entity.OrderType = (int)OrderType.购买卡项;
            entity.CreateTime = DateTime.Now;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
            entity.Statu = (int)OrderStatu.待结算;
            entity.Price = allPrice;
            entity.UserID = int.Parse(userid);
            var mendian = personnelBLL.HospitalEntityByID(LoginUserEntity.HospitalID);
            string time = DateTime.Now.ToString("MMdd");
            var count = cashBll.GetTodayOrderCount(LoginUserEntity.HospitalID, DateTime.Now);//获取总数
            entity.DocumentNumber = mendian.Prefix + time + (count + 1).ToString("d3");
            var result = cashBll.AddOrder(entity);
            #endregion

            if (result != null)
            {
                MainCardEntity card = new MainCardEntity();

                OrderinfoEntity orderinfo = new OrderinfoEntity();
                orderinfo.OrderNo = entity.OrderNo;
                entity.HospitalID = LoginUserEntity.UserID;
                orderinfo.Type = 3;

                for (int k = 0; k < IDlist.Length; k++)
                {
                    if (typelist != null)  //如果有这个分类则表示是储值卡这块的 1是充值 2是买卡
                    {
                        if (typelist[k] == "1") //如果是充值操作
                        {
                            #region 充值
                            //  cashBll.RechargeMoney(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(IDlist[k]), Price = ConvertValueHelper.ConvertDecimalValue(PriceList[k]) });
                            #endregion
                            #region 添加订单详情
                            OrderinfoEntity orderinfo1 = new OrderinfoEntity();
                            orderinfo1.OrderNo = entity.OrderNo;
                            orderinfo.Type = 3;
                            orderinfo.ProdcuitID = ConvertValueHelper.ConvertIntValue(IDlist[k]);
                            orderinfo.OldPrice = 0;
                            orderinfo.Price = ConvertValueHelper.ConvertDecimalValue(PriceList[k]);
                            orderinfo.AllPrice = ConvertValueHelper.ConvertDecimalValue(PriceList[k]);
                            orderinfo.Quantity = 1;
                            orderinfo.CardID = ConvertValueHelper.ConvertIntValue(IDlist[k]);
                            int orderinfonumber = cashBll.AddOrderinfo(orderinfo);
                            #endregion
                            #region //添加员工业绩
                            string[] joinuser = uslist.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries); //员工业绩列表分割
                            string joinuserlist = joinuser[k];      //第几个就找到第几个员工业绩

                            string[] userinfo1 = joinuserlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//分割当前业绩员工列表

                            decimal oneprice = orderinfo.Price / userinfo1.Length;

                            foreach (string uinfo in userinfo1)
                            {
                                JoinUserEntity juentity = new JoinUserEntity();
                                juentity.IsZhiding = 0;
                                juentity.OrderInfoID = orderinfonumber;
                                juentity.OrderNo = entity.OrderNo;
                                juentity.OtherTiCheng = 0;
                                juentity.ProjectID = Convert.ToInt32(IDlist[k]);
                                juentity.ServiceTime = 0;
                                juentity.Ticheng = 0;
                                juentity.Type = 3;//为储值卡,所以为3
                                juentity.UserID = Convert.ToInt32(uinfo);
                                juentity.XiaoHao = 0;
                                juentity.Yeji = oneprice;
                                cashBll.AddJoinUser(juentity);
                            }

                            #endregion
                        }
                        else if (typelist[k] == "2")//购卡操作
                        {
                            maika = 1;
                            if (Isgive[k] == "1") //赠卡的话用另外一种算法
                            {
                                #region //添加订单详情(保存到卡项,不用保存到更详细的级别)
                                card = baseinfoBLL.GetPrePayCardModel(Convert.ToInt32(IDlist[k]));//获取卡类基本信息
                                orderinfo.AllPrice = Convert.ToDecimal(PriceList[k]);
                                orderinfo.OldPrice = card.Price;
                                orderinfo.Price = 0; //赠卡的实际售价为0,所以这个售价是0
                                orderinfo.CardID = Convert.ToInt32(card.ID);
                                orderinfo.Quantity = 1;//卡数量默认为1
                                orderinfo.ProdcuitID = Convert.ToInt32(card.ID);
                                int orderinfonumber = cashBll.AddOrderinfo(orderinfo);  //获取订单详细ID
                                orderinfo.ID = orderinfonumber;
                                #endregion

                            }
                            else   //这个是普通卡的算法
                            {

                                #region //添加订单详情(保存到卡项,不用保存到更详细的级别)
                                card = baseinfoBLL.GetPrePayCardModel(Convert.ToInt32(IDlist[k]));//获取卡类基本信息
                                orderinfo.AllPrice = Convert.ToDecimal(PriceList[k]);
                                orderinfo.OldPrice = card.Price;

                                // orderinfo.Price = 0;//因为现在的卡没有价值,所以直接为0
                                orderinfo.Price = Convert.ToDecimal(PriceList[k]);
                                //orderinfo.CardType=1; 这个不要传
                                orderinfo.CardID = Convert.ToInt32(card.ID);
                                orderinfo.Quantity = 1;//卡数量默认为1
                                orderinfo.ProdcuitID = Convert.ToInt32(card.ID);
                                int orderinfonumber = cashBll.AddOrderinfo(orderinfo);  //获取订单详细ID
                                orderinfo.ID = orderinfonumber;
                                #endregion

                                #region //添加员工业绩
                                string[] joinuser = uslist.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries); //员工业绩列表分割
                                string joinuserlist = joinuser[k];      //第几个就找到第几个员工业绩

                                string[] userinfo1 = joinuserlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//分割当前业绩员工列表

                                decimal oneprice = orderinfo.Price / userinfo1.Length;

                                foreach (string uinfo in userinfo1)
                                {
                                    JoinUserEntity juentity = new JoinUserEntity();
                                    juentity.IsZhiding = 0;
                                    juentity.OrderInfoID = orderinfonumber;
                                    juentity.OrderNo = entity.OrderNo;
                                    juentity.OtherTiCheng = 0;
                                    juentity.ProjectID = Convert.ToInt32(IDlist[k]);
                                    juentity.ServiceTime = 0;
                                    juentity.Ticheng = 0;
                                    juentity.Type = 3;//为储值卡,所以为3
                                    juentity.UserID = Convert.ToInt32(uinfo);
                                    juentity.XiaoHao = 0;
                                    juentity.Yeji = oneprice;
                                    cashBll.AddJoinUser(juentity);
                                }





                                #endregion
                            }
                        }
                    }
                    else
                    {
                        #region //添加订单详情(保存到卡项,不用保存到更详细的级别)
                        card = baseinfoBLL.GetPrePayCardModel(Convert.ToInt32(IDlist[k]));//获取卡类基本信息
                        orderinfo.AllPrice = Convert.ToDecimal(PriceList[k]);
                        orderinfo.OldPrice = card.Price;
                        orderinfo.Price = Convert.ToDecimal(PriceList[k]);
                        //orderinfo.CardType=1; 这个不要传
                        orderinfo.CardID = Convert.ToInt32(card.ID);
                        orderinfo.Quantity = 1;//卡数量默认为1
                        orderinfo.ProdcuitID = Convert.ToInt32(card.ID);
                        int orderinfonumber = cashBll.AddOrderinfo(orderinfo);  //获取订单详细ID
                        orderinfo.ID = orderinfonumber;
                        #endregion

                        #region //添加员工业绩
                        string[] joinuser = uslist.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries); //员工业绩列表分割
                        string joinuserlist = joinuser[k];      //第几个就找到第几个员工业绩

                        string[] userinfo1 = joinuserlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//分割当前业绩员工列表

                        decimal oneprice = orderinfo.Price / userinfo1.Length;
                        foreach (string uinfo in userinfo1)
                        {
                            JoinUserEntity juentity = new JoinUserEntity();
                            juentity.IsZhiding = 0;
                            juentity.OrderInfoID = orderinfonumber;
                            juentity.OrderNo = entity.OrderNo;
                            juentity.OtherTiCheng = 0;
                            juentity.ProjectID = Convert.ToInt32(IDlist[k]);
                            juentity.ServiceTime = 0;
                            juentity.Ticheng = 0;
                            juentity.Type = 3;//为储值卡,所以为3
                            juentity.UserID = Convert.ToInt32(uinfo);
                            juentity.XiaoHao = 0;
                            juentity.Yeji = oneprice;
                            cashBll.AddJoinUser(juentity);
                        }
                        #endregion
                    }
                }
                strjson.Append("{cashamout:" + allPrice + ",cardamout:0,allamout:" + allPrice + ",orderNo:" + entity.OrderNo + ",UserID:" + userid + ",maika:" + maika + "}");

            }

            return Json(strjson.ToString());

        }


        /// <summary>
        ///    购卡结算 (备份)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="CardIDstr"></param>
        /// <param name="ordertimesstr"></param>
        /// <param name="cardidarr"></param>
        /// <param name="txtChuzhikaarr"></param>
        /// <param name="OtherPayment"></param>
        /// <param name="OtherPaymentValue"></param>
        /// <returns></returns>
        public ActionResult UpdateOrderPuchanseCardbeifen(OrderEntity entity, string CardIDstr, string ordertimesstr, string cardidarr, string txtChuzhikaarr, string OtherPayment, string OtherPaymentValue, string ismaika, string discountlist, string projectlisr)
        {
            entity.Statu = (int)OrderStatu.已结算;
            entity.ActurePrice = entity.Price + entity.QianPrice;
            entity.AllQianprice = entity.QianPrice;
            //修改订单为已支付
            int result = cashBll.UpdateOrder(entity);
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            //修改员工业绩的有效性
            #region ==结算==
            if (result > 0)
            {
                ////添加订单支付方式
                //其他支付方式
                if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                {
                    string[] strpay = OtherPayment.Split(',');
                    string[] OtherPaymentValuelist = OtherPaymentValue.Split(',');
                    for (int i = 0; i < strpay.Length; i++)
                    {
                        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = ConvertValueHelper.ConvertIntValue(strpay[i]), PayMoney = ConvertValueHelper.ConvertIntValue(OtherPaymentValuelist[i]) });
                    }
                }
                if (entity.Xianjin > 0)//现金
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.现金, PayMoney = entity.Xianjin });
                }
                if (entity.Yinlianka > 0)//银联卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.银联卡, PayMoney = entity.Yinlianka });
                }
                if (entity.Chuzhika > 0)//储值卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.储值卡, PayMoney = entity.Chuzhika });
                    //扣除卡类金额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        string[] ordertimesstrlist = txtChuzhikaarr.Split(',');
                        string[] caridlist = cardidarr.Split(',');
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            dic.Add(ConvertValueHelper.ConvertIntValue(caridlist[f]), ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]));
                            cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Price = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) });

                        }
                    }
                }
                if (entity.Huakouka > 0)//划扣次卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.划扣次数卡, PayMoney = entity.Huakouka });
                    //减去相应的卡类次数
                    if (!String.IsNullOrEmpty(CardIDstr))
                    {
                        string[] caridlist = CardIDstr.Split(',');
                        string[] ordertimesstrlist = ordertimesstr.Split(',');
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            //修改用户余额记录,通常一个客户只能买一个卡类型，多个可充值充值,所以根据CardID修改，没有根据ID
                            cashBll.UpdateMainCardBalanceTime(new MainCardBalanceEntity { CardID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Tiems = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]) });
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
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                //新的产品或者项目
                PayCash = 0;
                PayCarded = 0;
                ChuzhikaPayed = 0;
                PaymentOrderProductEntity conditon = new PaymentOrderProductEntity();
                conditon.AllPrice = info.AllPrice;//订单产品总价
                conditon.OrderInfoID = info.ID;//支付订单产品

                #region==支付==

                #region 现金
                if (ShengyuCash > 0)//现金
                {
                    conditon.PayType = (int)PaymentEnum.现金;
                    if (ShengyuCash >= info.AllPrice)//如果现金部分超出实际产品价格
                    {
                        conditon.PayMoney = info.AllPrice;
                        ShengyuCash = ShengyuCash - conditon.PayMoney;
                        PayCash = conditon.PayMoney;
                        //添加产品支付方式
                        cashBll.AddPaymentOrderProduct(conditon);
                        continue;//跳出循环
                    }
                    else
                    { //现金部分不够
                        conditon.PayMoney = ShengyuCash;
                        PayCash = ShengyuCash;
                        ShengyuCash = 0;
                        //添加产品支付方式
                        cashBll.AddPaymentOrderProduct(conditon);
                    }
                }//现金支付

                #endregion

                #region  银联卡
                if (YinLianCard > 0)//银联卡
                {
                    conditon.PayType = (int)PaymentEnum.银联卡;
                    if (YinLianCard >= info.AllPrice)//如果银联卡部分超出实际产品价格
                    {
                        if (PayCash > 0)//支付了部分现金
                        {
                            conditon.PayMoney = info.AllPrice - PayCash;
                            YinLianCard = YinLianCard - conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                        else
                        {
                            conditon.PayMoney = info.AllPrice;
                            YinLianCard = YinLianCard - conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                    }
                    else
                    { //银联卡部分不够
                        if (PayCash > 0)//支付了部分现金
                        {
                            if (PayCash + YinLianCard >= info.AllPrice)
                            {
                                conditon.PayMoney = info.AllPrice - PayCash;
                                YinLianCard = YinLianCard - conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue;//跳出循环
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

                }//银联卡支付

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
                        var manCardid = valu;//用户余额记录ID
                        var manPrice = dic[valu];//支付金额
                        if (manPrice > 0)
                        {
                            conditon.PayType = (int)PaymentEnum.储值卡;
                            if (manPrice >= info.AllPrice)//如果储值卡部分超出实际产品价格
                            {
                                if (PayCash + PayCarded > 0)//支付了部分现金和银联
                                {
                                    conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                    dic.Remove(manCardid);
                                    dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额

                                    //添加产品支付方式
                                    conditon.CardID = manCardid;
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//终止循环
                                }
                                else
                                {
                                    conditon.PayMoney = info.AllPrice;
                                    dic.Remove(manCardid);
                                    dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额
                                    conditon.CardID = manCardid;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//跳出循环
                                }
                            }
                            else
                            { //储值卡部分不够
                                if (PayCash + PayCarded > 0)//支付了部分现金和银联
                                {
                                    if (PayCash + PayCarded + manPrice > info.AllPrice)
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                        // Chuzhika = Chuzhika - conditon.PayMoney;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额
                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break;//跳出循环
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

                #region 其他支付方式
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
                            if (nowprice > info.AllPrice)//如果银联卡部分超出实际产品价格
                            {
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联和储值卡
                                {
                                    conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                    nowprice = nowprice - conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue;//跳出循环
                                }
                                else
                                {
                                    conditon.PayMoney = info.AllPrice;
                                    nowprice = nowprice - conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue;//跳出循环
                                }
                            }
                            else
                            { //银联卡部分不够
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed + nowprice > info.AllPrice)
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                        nowprice = nowprice - conditon.PayMoney;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;//跳出循环
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
                    #endregion

                }
                #endregion
            }
            #endregion

            #region ==添加储值记录和储值关系==
            entity = cashBll.GetOrderModel(entity);//获取订单详细
            OrderinfoEntity oi = new OrderinfoEntity();
            oi.OrderNo = entity.OrderNo;
            var orderinfolist = cashBll.GetOrderinfoList(oi);
            int o = 0;
            foreach (var orderinfo in orderinfolist)
            {
                #region ==获取尾款==
                decimal weikuan = 0;
                //如果存在欠款，修改用户欠款情况
                if (entity.QianPrice != 0)
                {
                    //如果是买卡欠钱,先获取当前订单详情的支付信息,再用总价减去所有的支付金额,剩下的就是欠款.
                    //判断是否是买卡,直接判断oldprice的价格是否为0,不为0则为买卡
                    if (ismaika == "1" && orderinfo.OldPrice != 0)
                    {
                        var zhifulist = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = orderinfo.ID });//获取支付详细列表
                        decimal zhifuprice = 0;
                        foreach (var zhifu in zhifulist)     //循环获得一共支付金额
                        {
                            zhifuprice += zhifu.PayMoney;
                        }
                        weikuan = orderinfo.AllPrice - zhifuprice;//订单详情的总价减去支付总价就是尾款价格
                    }
                }
                #endregion

                #region //逐条添加购买关系
                UserCardEntity usercard = new UserCardEntity();
                usercard.CardID = orderinfo.CardID;
                usercard.CreateTime = DateTime.Now;
                usercard.IsActive = 1;
                usercard.UserID = entity.UserID;
                //  usercard.Price = GetAllPrice(orderinfo.CardID, orderinfo.Price);//第一个参数是卡的ID,第二个是卡的原有价格
                usercard.Price = orderinfo.AllPrice;//第一个参数是卡的ID,第二个是卡的原有价格
                long usercardid = cashBll.AddUserCard(usercard);
                #endregion


                #region 添加用户余额记录
                //如果详情里面的IsChongzhi字段为1,则是充值单,则直接加
                if (orderinfo.IsChongzhi == 1)
                {
                    MainCardBalanceEntity mcb = new MainCardBalanceEntity();
                    mcb.ID = orderinfo.ProdcuitID;
                    mcb.Price = orderinfo.Price;
                    cashBll.RechargeMoney(mcb);

                    //用户储值卡余额明细日志
                    var mainCardBalance = cashBll.GetMainCardBalanceModel(mcb);
                }

                IList<ProjectProductEntity> pplist = new List<ProjectProductEntity>();

                // string[] projectarray = projectlisr.Split(new Char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
                string[] projectarray = projectlisr.Split('^');
                if (projectarray.Length >= o && !string.IsNullOrEmpty(projectlisr))   //如果当前数据集合大于现在循环的次数,则表示产品关系是前台传入
                {     //数据格式2|13|110.76923076923077|1440|180.00,1|5|218.00|1090|218.00
                    string thelist = projectarray[o];
                    if (!string.IsNullOrEmpty(thelist))
                    {
                        if (thelist == "0")//如果是默认的0,则表示读取默认数据
                        {
                            pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = orderinfo.CardID });//获取所有的产品对应关系
                        }
                        else
                        {
                            string[] infoarray = thelist.Split(',');
                            foreach (var info in infoarray)
                            {
                                string[] ia = info.Split('|');
                                ProjectProductEntity ppe = new ProjectProductEntity();
                                ppe.AllPrice = Convert.ToDecimal(ia[3]);
                                ppe.ConsumptionPrice = Convert.ToDecimal(ia[2]);
                                ppe.ProjectID = Convert.ToInt32(ia[0]);
                                ppe.Price = Convert.ToDecimal(ia[4]);
                                ppe.ProgrammeID = orderinfo.ProdcuitID;//父卡ID是当前订单详情的ID
                                ppe.Qualtity = Convert.ToInt32(ia[1]);
                                ppe.Type = 2;    //这里默认是项目
                                ppe.RealityPrice = Convert.ToDecimal(ia[3]) / Convert.ToInt32(ia[1]);
                                pplist.Add(ppe);
                            }
                        }
                    }
                }
                else
                {
                    pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = orderinfo.CardID });//获取所有的产品对应关系
                }
                MainCardBalanceEntity cardbalance = new MainCardBalanceEntity();
                cardbalance.CardID = orderinfo.CardID;
                cardbalance.UserCardID = Convert.ToInt32(usercardid);
                cardbalance.UserID = entity.UserID;
                cardbalance.Type = 1;
                IList<ProjectProductEntity> pp = new List<ProjectProductEntity>();
                long cbalance = 0;
                ////下面添加的是赠卡
                foreach (ProjectProductEntity ppentity in pplist)
                {
                    cardbalance.AllPrice = ppentity.AllPrice;//初始化的时候就是这张卡的默认金额
                    cardbalance.AllTiems = ppentity.Qualtity;
                    cardbalance.KouTimes = 0;
                    cardbalance.Price = ppentity.RealityPrice == 0 ? ppentity.Price : ppentity.RealityPrice;//这里如果是从数据库读取出来的,实际价格就是单价,如果是写入进来的,则不是.
                    cardbalance.ProjectID = ppentity.ProjectID;
                    cardbalance.Tiems = ppentity.Qualtity;
                    cardbalance.Type = ppentity.Type;
                    cardbalance.UserID = entity.UserID;
                    cardbalance.ConsumePrice = ppentity.ConsumptionPrice;
                    cardbalance.ExpireTime = GetExpireTime(orderinfo.CardID);
                    cbalance = cashBll.AddMainCardBalance(cardbalance);
                    var newinfo = cashBll.GetOrderinfoModel(orderinfo.ID);
                    newinfo.CardBalanceID = (newinfo.CardBalanceID == "0" ? cbalance.ToString() : newinfo.CardBalanceID + "," + cbalance.ToString());
                    cashBll.UpdateInfoCardID(newinfo);//把用户约记录的ID绑定到当前订单详情
                }
                //储值卡时需要把它本身也加入进去
                MainCardEntity maincard = baseinfoBLL.GetPrePayCardModel(orderinfo.CardID);
                maincard.Price = orderinfo.AllPrice;
                if (maincard.Type == 1)//
                {
                    cardbalance.AllPrice = maincard.Price;
                    cardbalance.AllTiems = 0;
                    cardbalance.ProjectID = maincard.ID;//储值卡,项目,产品ID存放在这个字段
                    cardbalance.CardID = Convert.ToInt32(usercardid);  //这个字段接受上面返回的关系ID
                    cardbalance.CostPrice = maincard.Price;
                    cardbalance.KouTimes = 0;
                    //cardbalance.UserCardID = Convert.ToInt32(usercardid);
                    cardbalance.Name = (maincard.Type == 1 ? "储值卡主卡" : (maincard.Type == 2 ? "疗程卡项目" : "方案卡详情"));
                    cardbalance.Price = maincard.Price;
                    cardbalance.Tiems = 0;
                    cardbalance.Weikuan = weikuan;//把尾款放入到用户记录
                    cardbalance.Type = maincard.Type;
                    cardbalance.UserID = entity.UserID;
                    cardbalance.ExpireTime = GetExpireTime(orderinfo.CardID);
                    if (maincard.Type == 1)
                    {
                        cardbalance.HuaKouZheLv = Convert.ToDecimal(discountlist.Split(',')[o]);
                    }

                    cbalance = cashBll.AddMainCardBalance(cardbalance);

                    //因为现在对应ID是文字串,需要重新编辑,所以每次加载都要重新载入最新的
                    var newinfo1 = cashBll.GetOrderinfoModel(orderinfo.ID);
                    newinfo1.CardBalanceID = (newinfo1.CardBalanceID == "0" ? cbalance.ToString() : newinfo1.CardBalanceID + "," + cbalance.ToString());
                    cashBll.UpdateInfoCardID(newinfo1);//把用户约记录的ID绑定到当前订单详情

                }




                #endregion

                #region ==添加员工业绩==
                // var yeji = new JoinUserEntity();
                // yeji.ProjectID = orderinfo.ProdcuitID;
                // yeji.Type = 3;//卡类操作
                // yeji.IsZhiding = 0;//默认不指定服务
                // yeji.OrderInfoID = orderinfo.ID;
                // yeji.OrderNo = entity.OrderNo;
                // yeji.XiaoHao = 0;//买卡没有消耗
                //// yeji.Yeji = orderinfo.AllPrice;  //业绩是按照订单收到的价格
                // cashBll.AddJoinUser(yeji);
                #endregion

                #region ==添加积分==
                var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate");  //获取积分实体
                decimal jifen = 0;

                if (usersetting.AttributeValue != null && usersetting.AttributeValue != "")
                {
                    jifen = Convert.ToDecimal(entity.Price) / (usersetting.AttributeValue == null
                                ? 1000000
                                : Convert.ToDecimal(usersetting.AttributeValue)); //判断能获取多少积分
                }


                IntegralRecordEntity ir = new IntegralRecordEntity();
                ir.UserID = entity.UserID;
                ir.OrderNO = entity.DocumentNumber;
                ir.Integral = jifen;
                ir.CreateTime = DateTime.Now;
                IpatBLL.AddIntegralRecord(ir);  //添加用户积分记录

                var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                intrgral.AllIntegral = intrgral.AllIntegral + jifen;
                intrgral.Integral = intrgral.Integral + jifen;

                IpatBLL.UpdateIntegral(intrgral);
                #endregion
                o++;

            }
            #endregion

            #region==重新计算员工业绩==
            //重新计算员工业绩
            foreach (var info in orderInfoList)
            {
                int row, pagecount;
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
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
                    cond.KakouYeji = ConvertValueHelper.ConvertDecimalValue((paymoney / joinList.Count).ToString("f2"));//卡扣业绩
                    cond.Yeji = ConvertValueHelper.ConvertDecimalValue((paymoney1 / joinList.Count).ToString("f2"));//现金业绩
                    cashBll.UpdateJoinUserYejiForPay(cond);
                }
            }
            #endregion

            return Json(result);
        }


        #endregion

        #region ==办卡以及支付==
        /// <summary>
        /// 办卡
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCard1()
        {
            string isbuyliaocheng = Request["isbuyliaocheng"];//是否购买疗程卡,是的话这个字段有值
            int maika = 0;//是否买卡字段 1是购卡 0不是
            var isRecharge = 0;//是否充值办卡 1是 0不是
            string userid = Request["UserID"];//获取办理用户
            int chkcreatetime = ConvertValueHelper.ConvertIntValue(Request["chkcreatetime"]);//是否选中
            string txtCreateTime = Request["txtCreateTime"];//订单时间
            string Name = HttpUtility.UrlDecode(Request["Name"], Encoding.UTF8); ;//备注
            if (Name.Contains(",")) {
                Name = Name.Split(',')[0];
            }
            string[] IDlist = Request.Form.GetValues("ID");  //获取ID列表
            string[] PriceList = Request.Form.GetValues("Price");//获取价格列表
            string uslist = Request["uslist"];  //获取参与人员集合
            string[] typelist = Request.Form.GetValues("Type");
            string[] isGive = Request.Form.GetValues("isgive");//是否赠卡列表
            //if (txtCreateTime=="")
            //{
            //    txtCreateTime =Convert.ToDateTime(DateTime.Now).ToString();
            //}
            StringBuilder strjson = new StringBuilder();
            decimal allPrice = GetAllprice(PriceList, isGive);//一共需要多少钱,获取总价

            var isbuychuzhi = 0;
            if (isbuyliaocheng == null)//因为现在只有疗程卡和储值卡,所以不是购买疗程卡就是购买储值卡
            { isbuychuzhi = 1; }
            string orderNo = AddOrder(allPrice, Convert.ToInt32(userid), isbuychuzhi, Name, chkcreatetime, txtCreateTime);

            MainCardEntity card = new MainCardEntity();

            OrderinfoEntity orderinfo = new OrderinfoEntity();
            orderinfo.OrderNo = orderNo;
            //entity.HospitalID = loginUserEntity.UserID;
            orderinfo.Type = 3;

            for (int k = 0; k < IDlist.Length; k++)
            {
                if (typelist != null)  //如果有这个分类则表示是储值卡这块的 1是充值 2是买卡
                {
                    if (typelist[k] == "1") //如果是充值操作
                    {
                        #region 充值
                        isRecharge = 1;
                        //  cashBll.RechargeMoney(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(IDlist[k]), Price = ConvertValueHelper.ConvertDecimalValue(PriceList[k]) });
                        #endregion
                        #region 添加订单详情
                        //card = baseinfoBLL.GetPrePayCardModel(Convert.ToInt32(IDlist[k]));//获取卡类基本信息
                        var maincardinfo = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = Convert.ToInt32(IDlist[k]) });
                        int orderinfonumber = AddOrderinfo(orderNo, maincardinfo.ProjectID, 0, ConvertValueHelper.ConvertDecimalValue(PriceList[k]), ConvertValueHelper.ConvertDecimalValue(PriceList[k]), maincardinfo.ProjectID, 1, isbuyliaocheng, IDlist[k]);
                        #endregion
                        #region //添加员工业绩
                        string[] joinuser = uslist.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries); //员工业绩列表分割
                        string joinuserlist = joinuser[0];      //第几个就找到第几个员工业绩
                        string[] userinfo1 = joinuserlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//分割当前业绩员工列表
                        decimal oneprice = orderinfo.Price / userinfo1.Length;
                        foreach (string uinfo in userinfo1)
                        {
                            AddJoinUser(orderinfonumber, orderNo, Convert.ToInt32(IDlist[k]), Convert.ToInt32(uinfo), oneprice, ConvertValueHelper.ConvertIntValue(userid));
                        }
                        #endregion
                    }
                    else if (typelist[k] == "2")//购卡操作
                    {
                        maika = 1;
                        isRecharge = 1;
                        if (isGive[k] == "1") //赠卡的话用另外一种算法
                        {
                            #region 添加订单详情(保存到卡项,不用保存到更详细的级别)
                            card = baseinfoBLL.GetPrePayCardModel(Convert.ToInt32(IDlist[k]));//获取卡类基本信息
                            //参数5表示他是买卡操作.1表示是充值
                            int orderinfonumber = AddOrderinfo(orderNo, Convert.ToInt32(card.ID), card.Price, 0, Convert.ToDecimal(PriceList[k]), card.ID, 5, isbuyliaocheng, "");
                            orderinfo.ID = orderinfonumber;
                            #endregion
                            #region 添加员工业绩
                            string[] joinuser = uslist.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries); //员工业绩列表分割
                            string joinuserlist = joinuser[0];      //第几个就找到第几个员工业绩
                            string[] userinfo1 = joinuserlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//分割当前业绩员工列表
                            decimal oneprice = orderinfo.Price / userinfo1.Length;
                            foreach (string uinfo in userinfo1)
                            {
                                AddJoinUser(orderinfonumber, orderNo, Convert.ToInt32(IDlist[k]), Convert.ToInt32(uinfo), 0, ConvertValueHelper.ConvertIntValue(userid));
                            }
                            #endregion
                        }
                        else   //这个是普通卡的算法
                        {
                            #region //添加订单详情(保存到卡项,不用保存到更详细的级别)
                            card = baseinfoBLL.GetPrePayCardModel(Convert.ToInt32(IDlist[k]));//获取卡类基本信息
                            int orderinfonumber = AddOrderinfo(orderNo, card.ID, card.Price, Convert.ToDecimal(PriceList[k]), Convert.ToDecimal(PriceList[k]), card.ID, 5, isbuyliaocheng, "");
                            orderinfo.ID = orderinfonumber;
                            #endregion
                            #region //添加员工业绩
                            string[] joinuser = uslist.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries); //员工业绩列表分割
                            string joinuserlist = joinuser[0];      //第几个就找到第几个员工业绩
                            string[] userinfo1 = joinuserlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//分割当前业绩员工列表
                            decimal oneprice = orderinfo.Price / userinfo1.Length;
                            foreach (string uinfo in userinfo1)
                            {
                                AddJoinUser(orderinfonumber, orderNo, Convert.ToInt32(IDlist[k]), Convert.ToInt32(uinfo), oneprice, ConvertValueHelper.ConvertIntValue(userid));
                            }
                            #endregion
                        }
                    }
                }
                else
                {
                    #region //添加订单详情(保存到卡项,不用保存到更详细的级别)
                    //isRecharge = 1;
                    card = baseinfoBLL.GetPrePayCardModel(Convert.ToInt32(IDlist[k]));//获取卡类基本信息
                    int orderinfonumber = AddOrderinfo(orderNo, card.ID, card.Price, Convert.ToDecimal(PriceList[k]), Convert.ToDecimal(PriceList[k]), card.ID, 5, isbuyliaocheng, "");
                    orderinfo.ID = orderinfonumber;
                    #endregion
                    #region //添加员工业绩
                    string[] joinuser = uslist.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries); //员工业绩列表分割
                    string joinuserlist = joinuser[0];      //第几个就找到第几个员工业绩

                    string[] userinfo1 = joinuserlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//分割当前业绩员工列表

                    decimal oneprice = orderinfo.Price / userinfo1.Length;
                    foreach (string uinfo in userinfo1)
                    {
                        AddJoinUser(orderinfonumber, orderNo, Convert.ToInt32(IDlist[k]), Convert.ToInt32(uinfo), oneprice, ConvertValueHelper.ConvertIntValue(userid));
                    }
                    #endregion
                }
            }
            // strjson.AppendFormat("{cashamout:" + allPrice + ",cardamout:0,allamout:" + allPrice + ",orderNo:"+ "\'"+OrderNo+"\'" + ",UserID:" + userid + ",maika:" + maika + "}");
            OrderInfoOver info = new OrderInfoOver();
            info.cashamout = allPrice;
            info.cardamout = 0;
            info.allamout = allPrice;
            info.orderNo = orderNo;
            info.UserID = userid;
            info.maika = maika;
            info.IsRecharge = isRecharge;
            TempData["MemoContent"] = Name;
            return Json(info);
        }

        public ActionResult UpdateOrderPuchanseCard(OrderEntity entity, string CardIDstr, string ordertimesstr, string cardidarr, string txtChuzhikaarr, string OtherPayment, string IsCashValue, string IsGiveValue, string OtherPaymentValue, string ismaika, string discountlist, string projectlisr, string hiddenToken, string idlist, string couponNo, decimal CouponPay, decimal depositValue, string txtCreateTime)
        {

            // LogWriter.WriteInfo("后台传入的项目：",  projectlisr );
            var OrderModel = cashBll.GetOrderModel(entity);
            if (OrderModel.Statu == 1)
            {
                return Json(-1);
            }
            //防止重复提交
            if (Session["hiddenToken1"] == null)
            {
                Session["hiddenToken1"] = hiddenToken;
            }
            else
            {
                if (Session["hiddenToken1"] == hiddenToken)
                {
                    return Json(-99);
                }
            }
            if (txtCreateTime == null || txtCreateTime == "")
            {
                //yyyy-MM-dd
                txtCreateTime = DateTime.Now.ToString("yyyy-MM-dd");
            }
            int count11111 = cashBll.Patient_UpdateLastTime1(entity.UserID, Convert.ToDateTime(txtCreateTime));
            entity.ActurePrice = entity.Price + entity.QianPrice;
            entity.AllQianprice = entity.QianPrice;
            var acturePrice = entity.ActurePrice;
            var allQianprice = entity.QianPrice;
            var qianPrice = entity.QianPrice;
            decimal otherCash = 0;//其他方式支付的 现金部分
            decimal Otherquan = 0;//其他劵类支付  部分
            decimal jffff = 0;
            //修改订单为已支付*(这个要放到最后才执行)
            int result = 1;
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            Dictionary<int, int> isgiveDic = new Dictionary<int, int>();
            Dictionary<int, decimal> otherpay = new Dictionary<int, decimal>();//其他支付方式金额
            Dictionary<int, int> otherpaytype = new Dictionary<int, int>();//其他支付方式类型

            decimal allzengchuzhikaPay = 0;//获取赠送储值卡支付金额
            //修改员工业绩的有效性
            /*
             */
            #region ==订单支付方式==
            if (result > 0)
            {
                //添加订单支付方式
                PaymentOrderEntity poe = new PaymentOrderEntity();


                //其他支付方式
                if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                {
                    string[] strpay = OtherPayment.Split(',');
                    string[] otherPaymentValuelist = OtherPaymentValue.Split(',');
                    string[] isCashValueStr = IsCashValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//此字段判断是否是 现金还是券类 1 现金 2 券类
                    for (int i = 0; i < strpay.Length; i++)
                    {
                        poe.OrderNo = entity.OrderNo;
                        poe.PayType = ConvertValueHelper.ConvertIntValue(strpay[i]);
                        poe.PayMoney = ConvertValueHelper.ConvertDecimalValue(otherPaymentValuelist[i]);
                        otherpay.Add(poe.PayType, poe.PayMoney);
                        poe.BalanceMoney = 0;
                        poe.BalanceTiems = 0;
                        poe.CardID = 0;
                        if (ConvertValueHelper.ConvertIntValue(isCashValueStr[i]) == 1)
                        {
                            otherCash = otherCash + poe.PayMoney;
                            poe.ParentPayType = (int)ParentPayType.现金;
                        }
                        else
                        {
                            Otherquan = Otherquan + poe.PayMoney;
                            poe.ParentPayType = (int)ParentPayType.券类;
                        }
                        otherpaytype.Add(poe.PayType, poe.ParentPayType);//其他支付方式
                        poe.PayName = sysbll.GetBaseinfoEntity(ConvertValueHelper.ConvertIntValue(strpay[i])).InfoName;
                        cashBll.AddPaymentOrder(poe);
                    }
                }
                if (entity.Xianjin > 0)//现金
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.现金;
                    poe.PayMoney = entity.Xianjin;
                    poe.PayName = "现金";
                    poe.BalanceMoney = 0;
                    poe.BalanceTiems = 0;
                    poe.CardID = 0;
                    poe.ParentPayType = (int)ParentPayType.现金;
                    cashBll.AddPaymentOrder(poe);
                }
                if (entity.Yinlianka > 0)//银联卡
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.银联卡;
                    poe.PayName = "银联卡";
                    poe.PayMoney = entity.Yinlianka;
                    poe.BalanceMoney = 0;
                    poe.BalanceTiems = 0;
                    poe.CardID = 0;
                    poe.ParentPayType = (int)ParentPayType.现金;
                    cashBll.AddPaymentOrder(poe);
                }

                if (entity.Chuzhika != 0)//储值卡
                {
                    poe.OrderNo = entity.OrderNo;

                    poe.PayType = (int)PaymentEnum.储值卡;

                    poe.BalanceTiems = 0;
                    //扣除卡类金额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        string[] ordertimesstrlist = txtChuzhikaarr.Split(',');
                        string[] caridlist = cardidarr.Split(',');
                        string[] isGiveValueList = IsGiveValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            if (isGiveValueList[f] == "1")
                            {
                                allzengchuzhikaPay = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) +
                                                     allzengchuzhikaPay;
                            }


                            dic.Add(ConvertValueHelper.ConvertIntValue(caridlist[f]), ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]));
                            isgiveDic.Add(ConvertValueHelper.ConvertIntValue(caridlist[f]), ConvertValueHelper.ConvertIntValue(isGiveValueList[f]));
                            result = cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Price = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) });

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

                            //获取这张支付卡的详细信息
                            var cardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]) });
                            //判断是否是消费积分
                            if (cardBalance.ProjectID == 40112 || cardBalance.ProjectID == 42221)
                            {
                                poe.PayType = 8;
                                poe.ParentPayType = 8;
                                var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                                jffff = intrgral.Integral;
                                intrgral.AllIntegral = mainCardBalanceDetail.Price;
                                intrgral.Integral = mainCardBalanceDetail.Price;
                                int count = IpatBLL.UpdateIntegral(intrgral);

                                IntegralOperationEntity entity1 = new IntegralOperationEntity();
                                entity1.Memo = "购买疗程卡";
                                entity1.Name = "一";
                                decimal PayMoney1 = 0;

                                if (mainCardBalanceDetail.Price >= 1)
                                {
                                    PayMoney1 = jffff - mainCardBalanceDetail.Price;
                                }
                                else
                                {
                                    PayMoney1 = jffff;
                                }
                                entity1.Number = PayMoney1;
                                entity1.UserID = entity.UserID;
                                entity1.OperationType = 2;
                                var id = IpatBLL.AddIntegralOperation(entity1);
                                IntegralRecordEntity ir1 = new IntegralRecordEntity();
                                ir1.UserID = entity.UserID;
                                ir1.Integral = PayMoney1;
                                ir1.CreateTime = DateTime.Now;
                                ir1.OrderNO = "购买疗程卡";
                                ir1.DocumentNumber = "购买疗程卡";
                                ir1.IntegralOperationID = id;
                                IpatBLL.AddIntegralRecord(ir1);//增加积分记录

                            }
                            else
                            {
                                poe.ParentPayType = (int)ParentPayType.储值卡;
                                poe.PayType = (int)ParentPayType.储值卡;
                            }
                            var chuzhika = baseinfoBLL.GetPrePayCardModel(cardBalance.ProjectID);
                            poe.PayName = chuzhika.Name;
                            poe.PayMoney = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]);
                            poe.CardBalanceID = ConvertValueHelper.ConvertIntValue(caridlist[f]);
                            poe.IsGive = ConvertValueHelper.ConvertIntValue(isGiveValueList[f]);
                            poe.CardID = cardBalance.CardID;
                            poe.BalanceMoney = cardBalance.Price;
                            poe.ids = poe.CardBalanceID.ToString();
                            cashBll.AddPaymentOrder(poe);
                        }
                    }



                }
                if (entity.Huakouka != 0)//划扣次卡
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.划扣次数卡;
                    poe.ParentPayType = (int)ParentPayType.划卡;
                    poe.BalanceMoney = 0;
                    //减去相应的卡类次数
                    if (!String.IsNullOrEmpty(CardIDstr))
                    {
                        string[] caridlist = CardIDstr.Split(',');
                        string[] ordertimesstrlist = ordertimesstr.Split(',');
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            //修改用户余额记录,根据ID                      CardID是用户约记录的ID
                            cashBll.UpdateMainCardBalanceTime(new MainCardBalanceEntity { CardID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Tiems = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]) });
                            //获取这张支付卡的详细信息
                            var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]) });
                            var xiangmu = baseinfoBLL.GetProjectModel(CardBalance.ProjectID);
                            poe.PayName = xiangmu.Name;
                            poe.PayMoney = xiangmu.RetailPrice * ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]);
                            poe.CardBalanceID = ConvertValueHelper.ConvertIntValue(caridlist[f]);
                            poe.CardID = CardBalance.CardID;
                            poe.PayTime = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]);
                            poe.BalanceTiems = CardBalance.Tiems;
                            poe.ids = poe.CardBalanceID.ToString();
                            cashBll.AddPaymentOrder(poe);
                        }
                    }
                }
            }
            #endregion
            //if (result > 0)
            //{
            //    ////添加订单支付方式
            //    //其他支付方式
            //    if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
            //    {
            //        string[] strpay = OtherPayment.Split(',');
            //        string[] OtherPaymentValuelist = OtherPaymentValue.Split(',');
            //        for (int i = 0; i < strpay.Length; i++)
            //        {
            //            cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = ConvertValueHelper.ConvertIntValue(strpay[i]), PayMoney = ConvertValueHelper.ConvertIntValue(OtherPaymentValuelist[i]) });
            //        }
            //    }
            //    if (entity.Xianjin > 0)//现金
            //    {
            //        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.现金, PayMoney = entity.Xianjin });
            //    }
            //    if (entity.Yinlianka > 0)//银联卡
            //    {
            //        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.银联卡, PayMoney = entity.Yinlianka });
            //    }
            //    if (entity.Chuzhika > 0)//储值卡
            //    {
            //        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.储值卡, PayMoney = entity.Chuzhika });
            //        //扣除卡类金额
            //        if (!String.IsNullOrEmpty(cardidarr))
            //        {
            //            string[] ordertimesstrlist = txtChuzhikaarr.Split(',');
            //            string[] caridlist = cardidarr.Split(',');
            //            for (int f = 0; f < caridlist.Length; f++)
            //            {
            //                dic.Add(ConvertValueHelper.ConvertIntValue(caridlist[f]), ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]));
            //                cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Price = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) });
            //            }
            //        }
            //    }
            //    if (entity.Huakouka > 0)//划扣次卡
            //    {
            //        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.划扣次数卡, PayMoney = entity.Huakouka });
            //        //减去相应的卡类次数
            //        if (!String.IsNullOrEmpty(CardIDstr))
            //        {
            //            string[] caridlist = CardIDstr.Split(',');
            //            string[] ordertimesstrlist = ordertimesstr.Split(',');
            //            for (int f = 0; f < caridlist.Length; f++)
            //            {
            //                //修改用户余额记录,通常一个客户只能买一个卡类型，多个可充值充值,所以根据CardID修改，没有根据ID
            //                cashBll.UpdateMainCardBalanceTime(new MainCardBalanceEntity { CardID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Tiems = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]) });
            //            }
            //        }
            //    }
            //}
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
            decimal ShengyuCash1 = entity.Xianjin;
            decimal ShengyuCashMoney = entity.Xianjin;//积分写入
            //已支付现金金额
            decimal PayCash = 0;
            //剩余银联卡部分
            decimal YinLianCard = entity.Yinlianka;
            decimal YinLianCard1 = entity.Yinlianka;
            // decimal YinLianCard1 = 0;
            //已支付银联卡部分
            decimal PayCarded = 0;
            //剩余储值卡
            decimal Chuzhika = entity.Chuzhika;
            //已支付银联卡部分
            decimal ChuzhikaPayed = 0;
            decimal otherpayprice = 0;//其他已经付款的
            //储值卡集合
            List<int> cardlist = new List<int>();
            cardlist.AddRange(dic.Keys);
            //其他支付方式
            List<int> otherpaylist = new List<int>();
            otherpaylist.AddRange(otherpay.Keys);

            foreach (var info in orderInfoList)
            {
                var ModelCard1 = cashBll.GetMainCardModel(info.ProdcuitID);
                var cardmodel = baseinfoBLL.GetPrePayCardModel(info.ProdcuitID);
                if (cardmodel.IsGive == 1) { continue; }
                int row, pagecount;
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                //新的产品或者项目
                PayCash = 0;
                PayCarded = 0;
                ChuzhikaPayed = 0;
                otherpayprice = 0;
                PaymentOrderProductEntity conditon = new PaymentOrderProductEntity();
                conditon.AllPrice = info.AllPrice;//订单产品总价
                conditon.OrderInfoID = info.ID;//支付订单产品
                LogWriter.WriteInfo("现金：" + ShengyuCash + "，银联卡：" + YinLianCard, "产品支付方式：" + entity.OrderNo);
                #region==支付==

                #region 现金
                if (ShengyuCash > 0)//现金
                {
                    conditon.PayType = (int)PaymentEnum.现金;
                    if (ShengyuCash >= info.AllPrice)//如果现金部分超出实际产品价格
                    {


                        if (LoginUserEntity.HospitalID == 1017)
                        {
                            var paymentOrderList = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = info.OrderNo });
                            paymentOrderList = paymentOrderList.Where(t => t.PayName == "积分消费").ToList();
                            if (paymentOrderList.Count == 1)
                            {
                                conditon.PayMoney = ModelCard1.Price - ModelCard1.Proportion;
                            }
                            else
                            {
                                conditon.PayMoney = info.AllPrice;
                            }

                        }
                        else
                        {
                            conditon.PayMoney = info.AllPrice;
                        }

                        ShengyuCash = ShengyuCash - conditon.PayMoney;
                        PayCash = conditon.PayMoney;
                        //添加产品支付方式
                        conditon.ParentPayType = (int)ParentPayType.现金;
                        cashBll.AddPaymentOrderProduct(conditon);
                        continue;//跳出循环
                    }
                    else
                    { //现金部分不够
                        conditon.PayMoney = ShengyuCash;
                        PayCash = ShengyuCash;
                        ShengyuCash = 0;
                        //添加产品支付方式
                        conditon.ParentPayType = (int)ParentPayType.现金;
                        cashBll.AddPaymentOrderProduct(conditon);
                    }
                }//现金支付

                #endregion

                #region  银联卡
                if (YinLianCard > 0)//银联卡
                {
                    //pppp
                    //  YinLianCard1 = YinLianCard;
                    conditon.PayType = (int)PaymentEnum.银联卡;
                    if (YinLianCard >= info.AllPrice)//如果银联卡部分超出实际产品价格
                    {
                        if (PayCash > 0)//支付了部分现金
                        {
                            conditon.PayMoney = info.AllPrice - PayCash;
                            YinLianCard = YinLianCard - conditon.PayMoney;
                            //添加产品支付方式
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                        else
                        {
                            if (LoginUserEntity.HospitalID == 1017)
                            {
                                var paymentOrderList = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = info.OrderNo });
                                paymentOrderList = paymentOrderList.Where(t => t.PayName == "积分消费").ToList();
                                if (paymentOrderList.Count == 1)
                                {
                                    if (ModelCard1.ID > 0)
                                    {
                                        conditon.PayMoney = ModelCard1.Price - ModelCard1.Proportion;
                                    }
                                }
                                else
                                {
                                    conditon.PayMoney = info.AllPrice;
                                }

                            }
                            else
                            {
                                conditon.PayMoney = info.AllPrice;
                            }

                            YinLianCard = YinLianCard - conditon.PayMoney;
                            //添加产品支付方式
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                    }
                    else
                    { //银联卡部分不够
                        if (PayCash > 0)//支付了部分现金
                        {
                            if (PayCash + YinLianCard >= info.AllPrice)
                            {
                                conditon.PayMoney = info.AllPrice - PayCash;
                                YinLianCard = YinLianCard - conditon.PayMoney;
                                //添加产品支付方式
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue;//跳出循环
                            }
                            else
                            {
                                // conditon.PayMoney = YinLianCard + PayCash;
                                conditon.PayMoney = YinLianCard;
                                YinLianCard = 0;
                                PayCarded = conditon.PayMoney;
                                //添加产品支付方式
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                cashBll.AddPaymentOrderProduct(conditon);
                            }

                        }
                        else
                        {
                            conditon.PayMoney = YinLianCard;
                            YinLianCard = 0;
                            PayCarded = conditon.PayMoney;
                            //添加产品支付方式
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            cashBll.AddPaymentOrderProduct(conditon);

                        }
                    }

                }//银联卡支付

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
                        var manCardid = valu;//用户余额记录ID
                        var manPrice = dic[valu];//支付金额
                        if (manPrice > 0)
                        {
                            conditon.PayType = (int)PaymentEnum.储值卡;
                            if (manPrice >= info.AllPrice)//如果储值卡部分超出实际产品价格
                            {
                                if (PayCash + PayCarded > 0)//支付了部分现金和银联
                                {
                                    conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                    dic.Remove(manCardid);
                                    dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额
                                    conditon.ParentPayType = (int)ParentPayType.储值卡;
                                    //添加产品支付方式 g
                                    conditon.CardID = manCardid;
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//终止循环
                                }
                                else
                                {
                                    conditon.PayMoney = info.AllPrice;
                                    dic.Remove(manCardid);
                                    dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额
                                    conditon.CardID = manCardid;
                                    //添加产品支付方式
                                    conditon.ParentPayType = (int)ParentPayType.储值卡;
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//跳出循环
                                }
                            }
                            else
                            { //储值卡部分不够
                                if (PayCash + PayCarded > 0)//支付了部分现金和银联
                                {
                                    if (PayCash + PayCarded + manPrice > info.AllPrice)
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                        // Chuzhika = Chuzhika - conditon.PayMoney;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额
                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break;//跳出循环
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
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
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
                                    conditon.ParentPayType = (int)ParentPayType.储值卡;
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 其他支付方式
                //其他支付方式
                if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                {
                    foreach (var PayType in otherpaylist)
                    {
                        var manCardid = PayType;//用户余额记录ID
                        var nowprice = otherpay[PayType];//支付金额
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
                            if (nowprice > Math.Abs(info.AllPrice))//如果大于实际产品价格
                            {
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联和储值卡
                                {
                                    if (LoginUserEntity.HospitalID == 1017)
                                    {
                                        var paymentOrderList = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = info.OrderNo });
                                        paymentOrderList = paymentOrderList.Where(t => t.PayName == "积分消费").ToList();
                                        if (paymentOrderList.Count == 1)
                                        {
                                            //var ModelCard = cashBll.GetMainCardModel(info.ProdcuitID);
                                            conditon.PayMoney = ModelCard1.Price - ModelCard1.Proportion;
                                        }
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice < 0 ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed) : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
                                    }
                                    nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                    otherpay.Remove(PayType);
                                    otherpay.Add(PayType, nowprice);
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//跳出循环
                                }
                                else
                                {
                                    conditon.PayMoney = Math.Abs(info.AllPrice) - Math.Abs(otherpayprice);
                                    nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                    otherpay.Remove(PayType);
                                    otherpay.Add(PayType, nowprice);
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//跳出循环
                                }
                            }
                            else
                            { //银联卡部分不够111
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed + nowprice > Math.Abs(info.AllPrice))
                                    {
                                        conditon.PayMoney = info.AllPrice < 0 ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed - otherpayprice) : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed - otherpayprice);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);

                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break;//跳出循环
                                    }
                                    else
                                    {
                                        otherpayprice = conditon.PayMoney = info.AllPrice < 0 ? -nowprice : nowprice;
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


            #region ==添加储值记录和储值关系==
            entity = cashBll.GetOrderModel(entity);//获取订单详细
            OrderinfoEntity oi = new OrderinfoEntity();
            oi.OrderNo = entity.OrderNo;
            var orderinfolist = cashBll.GetOrderinfoList(oi);
            int o = 0;
            //疗程卡ID集合
            string[] projectidlist = idlist.Split('$');
            //项目ID集合
            string[] projectarray = projectlisr.Split('^');
            foreach (var orderinfo in orderinfolist)
            {
                #region ==获取尾款==
                decimal weikuan = 0;
                //如果存在欠款，修改用户欠款情况
                if (entity.QianPrice != 0)
                {
                    //如果是买卡欠钱,先获取当前订单详情的支付信息,再用总价减去所有的支付金额,剩下的就是欠款.
                    //判断是否是买卡,直接判断oldprice的价格是否为0,不为0则为买卡
                    if (ismaika == "1" && orderinfo.OldPrice != 0)
                    {
                        var zhifulist = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = orderinfo.ID });//获取支付详细列表
                        decimal zhifuprice = 0;
                        foreach (var zhifu in zhifulist)     //循环获得一共支付金额
                        {
                            zhifuprice += zhifu.PayMoney;
                        }
                        weikuan = orderinfo.AllPrice - zhifuprice;//订单详情的总价减去支付总价就是尾款价格
                    }
                }
                #endregion

                #region //逐条添加购买关系
                UserCardEntity usercard = new UserCardEntity();
                usercard.CardID = orderinfo.CardID;
                usercard.CreateTime = DateTime.Now;
                usercard.IsActive = 1;
                usercard.UserID = entity.UserID;
                //  usercard.Price = GetAllPrice(orderinfo.CardID, orderinfo.Price);//第一个参数是卡的ID,第二个是卡的原有价格
                usercard.Price = orderinfo.AllPrice;//第一个参数是卡的ID,第二个是卡的原有价格
                long usercardid = cashBll.AddUserCard(usercard);
                #endregion


                #region //添加用户余额记录
                //如果详情里面的IsChongzhi字段为1,则是充值单,则直接加
                if (orderinfo.IsChongzhi == 1)
                {
                    var mcb = new MainCardBalanceEntity();
                    //mcb.ID = orderinfo.ProdcuitID;  //充值操作修改  edit by kuang  2019-06-01
                    mcb.ID = Convert.ToInt32(orderinfo.CardBalanceID);
                    mcb.Price = orderinfo.Price;
                    cashBll.RechargeMoney(mcb);

                    //用户储值卡余额明细日志
                    var mainCardBalance = cashBll.GetMainCardBalanceModel(mcb);
                    cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                    {
                        CardBalanceId = mcb.ID,
                        OrderNo = orderinfo.OrderNo,
                        Type = (int)MainCardBalanceDetailType.充值,
                        Amount = mcb.Price,
                        Balance = mainCardBalance.Price,
                        OldAmount = mainCardBalance.Price,
                        CreateTime = DateTime.Now
                    });
                }

                IList<ProjectProductEntity> pplist = new List<ProjectProductEntity>();
                //string projsssss = "";
                // string[] projectarray = projectlisr.Split(new Char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);

                Dictionary<string, string> dct = new Dictionary<string, string>();
                for (int i = 0; i < projectidlist.Length; i++)
                {
                    if (!String.IsNullOrEmpty(projectidlist[i]))
                    {
                        dct.Add(projectidlist[i], projectarray[i]);
                    }
                }
                if (projectarray.Length >= o && !string.IsNullOrEmpty(projectlisr))   //如果当前数据集合大于现在循环的次数,则表示产品关系是前台传入
                {     //数据格式2|13|110.76|1440|180.00,1|5|218.00|1090|218.00
                    string thelist = dct[orderinfo.CardID.ToString()];// projectarray[o];
                    if (!string.IsNullOrEmpty(thelist))
                    {
                        if (thelist == "0")//如果是默认的0,则表示读取默认数据
                        {
                            pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = orderinfo.CardID });//获取所有的产品对应关系
                        }
                        else
                        {
                            //string[] infoarray =thelist.Split(',');
                            string[] infoarray = String.IsNullOrEmpty(dct[orderinfo.CardID.ToString()]) ? thelist.Split(',') : dct[orderinfo.CardID.ToString()].Split(',');
                            foreach (var info in infoarray)
                            {
                                string[] ia = info.Split('|');
                                ProjectProductEntity ppe = new ProjectProductEntity();
                                ppe.RealityPrice = Convert.ToDecimal(ia[3]) / (Convert.ToInt32(ia[1]) == 0 ? 1 : Convert.ToInt32(ia[1]));
                                ppe.AllPrice = Convert.ToDecimal(ia[3]);
                                ppe.ConsumptionPrice = Convert.ToDecimal(ia[2]);
                                ppe.ProjectID = Convert.ToInt32(ia[0]);
                                ppe.Price = ppe.RealityPrice; //修改原因:因为赠送卡总金额为0时,这个价格需要改为0.故在此处修改.原来代码为:Convert.ToDecimal(ia[4]);
                                ppe.ProgrammeID = orderinfo.ProdcuitID;//父卡ID是当前订单详情的ID
                                ppe.Qualtity = Convert.ToInt32(ia[1]);
                                ppe.Type = 2;    //这里默认是项目

                                pplist.Add(ppe);
                                //projsssss = projsssss + ppe.ProjectID + ",";
                            }
                        }
                    }
                }
                else
                {
                    pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = orderinfo.CardID });//获取所有的产品对应关系
                }
                // LogWriter.WriteInfo("疗程卡ID:" + orderinfo.CardID, "项目："+projsssss);
                MainCardBalanceEntity cardbalance = new MainCardBalanceEntity();
                cardbalance.CardID = orderinfo.CardID;
                cardbalance.UserCardID = Convert.ToInt32(usercardid);
                cardbalance.UserID = entity.UserID;
                cardbalance.Type = 1;
                cardbalance.OrderNo = entity.OrderNo;
                IList<ProjectProductEntity> pp = new List<ProjectProductEntity>();
                long cbalance = 0;
                // 添加卡对应的项目 --添加账户余额
                foreach (ProjectProductEntity ppentity in pplist)
                {
                    if (ppentity.Type != 3)
                    {
                        cardbalance.AllPrice = ppentity.AllPrice;//初始化的时候就是这张卡的默认金额
                        cardbalance.AllTiems = ppentity.Qualtity;
                        cardbalance.KouTimes = 0;
                        //修复日期:20160428
                        //修复代码:L0428001
                        //修复范围:修复购买疗程卡时默认下的价格的取值错误.
                        //代码范围:本行以下5行
                        decimal consumptionPrice = 0;
                        if (ppentity.Type == 2)
                        {
                            //decimal dd = ShengyuCash1;

                            //if (LoginUserEntity.HospitalID==1017)
                            // {
                            cardbalance.Price = ppentity.AllPrice / (ppentity.Qualtity == 0 ? 1 : ppentity.Qualtity);//如果是默认的疗程卡,则用总金额除以总次数来获取价格
                            if (LoginUserEntity.HospitalID == 1017)
                            {
                                var paymentOrderList = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = orderinfo.OrderNo });
                                paymentOrderList = paymentOrderList.Where(t => t.PayName == "积分消费").ToList();
                                if (paymentOrderList.Count == 1)
                                {
                                    var ModelCard = cashBll.GetMainCardModel(orderinfo.ProdcuitID);
                                    if (ModelCard.ID > 0)
                                    {
                                        if (ModelCard.Price >= ModelCard.Proportion)
                                        {
                                            ShengyuCash1 = ModelCard.Price - ModelCard.Proportion;
                                        }
                                        else
                                        {
                                            ShengyuCash1 = ModelCard.Price - paymentOrderList[0].PayMoney;
                                            // ShengyuCash1 = ModelCard.Proportion- ModelCard.Price;
                                        }


                                        consumptionPrice = ShengyuCash1 / (ppentity.Qualtity == 0 ? 1 : ppentity.Qualtity);//如果是默认的疗程卡,则用总金额除以总次数来获取价格
                                    }
                                }
                                else
                                {
                                    consumptionPrice = ppentity.AllPrice / (ppentity.Qualtity == 0 ? 1 : ppentity.Qualtity);
                                }
                                // var iiii = orderinfo.ProdcuitID;
                            }
                            //}
                            //else
                            // {
                            cardbalance.Price = ppentity.AllPrice / (ppentity.Qualtity == 0 ? 1 : ppentity.Qualtity);//如果是默认的疗程卡,则用总金额除以总次数来获取价格
                                                                                                                     // }
                        }
                        else
                        {
                            cardbalance.Price = ppentity.RealityPrice == 0 ? ppentity.Price : ppentity.RealityPrice;//这里如果是从数据库读取出来的,实际价格就是单价,如果是写入进来的,则不是.
                        }
                        cardbalance.ProjectID = ppentity.ProjectID;
                        LogWriter.WriteInfo("购买卡项操作：", "疗程卡ID：" + orderinfo.CardID + "项目ID：" + ppentity.ProjectID);
                        cardbalance.Tiems = ppentity.Qualtity;
                        if (ppentity.Type == 1)
                        {
                            cardbalance.AllTiems = 0;
                            cardbalance.Tiems = 0;
                        }
                        cardbalance.Type = ppentity.Type;
                        cardbalance.UserID = entity.UserID;
                        if (LoginUserEntity.HospitalID == 1017)
                        {
                            cardbalance.ConsumePrice = consumptionPrice;
                        }
                        else
                        {
                            cardbalance.ConsumePrice = ppentity.ConsumptionPrice;
                        }
                        cardbalance.ExpireTime = GetExpireTime(orderinfo.CardID);
                        cardbalance.MemoContent = TempData["MemoContent"] + "";
                        cbalance = cashBll.AddMainCardBalance(cardbalance);
                        if (cardbalance.Type == 1)
                        {
                            //用户储值卡余额明细日志
                            cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                            {
                                CardBalanceId = Convert.ToInt32(cbalance),
                                OrderNo = orderinfo.OrderNo,
                                Type = (int)MainCardBalanceDetailType.充值,
                                Amount = cardbalance.Price,
                                Balance = cardbalance.Price,
                                OldAmount = 0,
                                CreateTime = DateTime.Now
                            });
                        }
                        var newinfo = cashBll.GetOrderinfoModel(orderinfo.ID);
                        //添加订单详细CardBalanceID 关联表
                        cashBll.AddOrderinfoProject(new OrderinfoProjectEntity { OrderInfoID = orderinfo.ID, CardBalanceID = (int)cbalance });
                        newinfo.CardBalanceID = (newinfo.CardBalanceID == "0" ? cbalance.ToString() : newinfo.CardBalanceID + "," + cbalance.ToString());
                        cashBll.UpdateInfoCardID(newinfo);//把用户约记录的ID绑定到当前订单详情
                    }
                }
                //储值卡时需要把它本身也加入进去
                MainCardEntity maincard = baseinfoBLL.GetPrePayCardModel(orderinfo.CardID);
                maincard.Price = orderinfo.AllPrice;
                if (orderinfo.IsChongzhi != 1) //edit by kuang 2019-06-01
                {
                    if (maincard.Type == 1) //
                    {
                        cardbalance.AllPrice = maincard.Price;
                        cardbalance.AllTiems = 0;
                        cardbalance.ProjectID = maincard.ID; //储值卡,项目,产品ID存放在这个字段
                        cardbalance.CardID = Convert.ToInt32(usercardid); //这个字段接受上面返回的关系ID
                        cardbalance.CostPrice = maincard.Price;
                        cardbalance.KouTimes = 0;
                        //cardbalance.UserCardID = Convert.ToInt32(usercardid);
                        cardbalance.Name = (maincard.Type == 1 ? "储值卡主卡" : (maincard.Type == 2 ? "疗程卡项目" : "方案卡详情"));
                        cardbalance.Price = maincard.Price;
                        cardbalance.Tiems = 0;
                        cardbalance.Weikuan = weikuan; //把尾款放入到用户记录
                        cardbalance.Type = maincard.Type;
                        cardbalance.UserID = entity.UserID;
                        cardbalance.ExpireTime = GetExpireTime(orderinfo.CardID);
                        if (maincard.Type == 1)
                        {
                            cardbalance.HuaKouZheLv = Convert.ToDecimal(discountlist.Split(',')[o]);
                        }
                        cbalance = cashBll.AddMainCardBalance(cardbalance);
                        if (cardbalance.Type == 1)
                        {
                            //用户储值卡余额明细日志
                            cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                            {
                                CardBalanceId = Convert.ToInt32(cbalance),
                                OrderNo = orderinfo.OrderNo,
                                Type = (int)MainCardBalanceDetailType.充值,
                                Amount = cardbalance.Price,
                                Balance = cardbalance.Price,
                                OldAmount = 0,
                                CreateTime = DateTime.Now
                            });
                        }
                        //添加订单详细CardBalanceID 关联表
                        cashBll.AddOrderinfoProject(new OrderinfoProjectEntity { OrderInfoID = orderinfo.ID, CardBalanceID = (int)cbalance });
                        //因为现在对应ID是文字串,需要重新编辑,所以每次加载都要重新载入最新的
                        var newinfo1 = cashBll.GetOrderinfoModel(orderinfo.ID);
                        newinfo1.CardBalanceID = (newinfo1.CardBalanceID == "0" ? cbalance.ToString() : newinfo1.CardBalanceID + "," + cbalance.ToString());
                        cashBll.UpdateInfoCardID(newinfo1); //把用户约记录的ID绑定到当前订单详情
                    }
                }




                #endregion

                #region ==添加员工业绩==
                // var yeji = new JoinUserEntity();
                // yeji.ProjectID = orderinfo.ProdcuitID;
                // yeji.Type = 3;//卡类操作
                // yeji.IsZhiding = 0;//默认不指定服务
                // yeji.OrderInfoID = orderinfo.ID;
                // yeji.OrderNo = entity.OrderNo;
                // yeji.XiaoHao = 0;//买卡没有消耗
                //// yeji.Yeji = orderinfo.AllPrice;  //业绩是按照订单收到的价格
                // cashBll.AddJoinUser(yeji);
                #endregion

                #region ==添加积分==
                if (ShengyuCashMoney != 0 || YinLianCard1 != 0 || otherpayprice != 0)
                {
                    var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate");  //获取积分实体
                    decimal jifen = Convert.ToDecimal(ShengyuCashMoney + YinLianCard1 + otherpayprice) / (string.IsNullOrEmpty(usersetting.AttributeValue) ? 100000 : Convert.ToDecimal(usersetting.AttributeValue));//判断能获取多少积分
                    if (LoginUserEntity.HospitalID == 1017)
                    { }
                    else
                    {

                        IntegralRecordEntity ir = new IntegralRecordEntity();
                        ir.UserID = entity.UserID;
                        ir.DocumentNumber = entity.OrderNo;
                        ir.Integral = jifen;
                        ir.CreateTime = DateTime.Now;
                        IpatBLL.AddIntegralRecord(ir);  //添加用户积分记录

                        var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                        intrgral.AllIntegral = intrgral.AllIntegral + jifen;
                        intrgral.Integral = intrgral.Integral + jifen;

                        IpatBLL.UpdateIntegral(intrgral);
                    }
                }
                #endregion
                o++;

            }
            #endregion

            #region==重新计算员工业绩==
            //重新计算员工业绩
            foreach (var info in orderInfoList)
            {
                int row, pagecount;
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                foreach (var cond in joinList)
                {
                    // var list = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = entity.OrderNo });
                    //  decimal Proportion = 0;
                    // for (int i = 0; i < list.Count; i++)
                    // {
                    //  var mar = cashBll.GetMainCardModel(Convert.ToInt32(list[i].CardID));
                    // Proportion += mar.Proportion;
                    //  }
                    //var list = cashBll.GetOrderinfoList2(new OrderinfoEntity { OrderNo = entity.OrderNo });
                    //卡扣业绩


                    var payproduct = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 3 });
                    decimal paymoney = 0;
                    if (LoginUserEntity.HospitalID == 1017)
                    {
                        var minCarModel = cashBll.GetMainCardModel(info.CardID);
                        var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                        if (minCarModel.ID > 0)
                        {
                            foreach (var m in payproduct)
                            {
                                decimal money = m.AllPrice - minCarModel.Proportion;
                                paymoney = money * m.HuaKouZheLv;
                                cond.Proportion = minCarModel.Proportion;
                            }
                            if (jffff >= minCarModel.Proportion)
                            {
                                cond.Proportion = minCarModel.Proportion;
                            }
                            else
                            {
                                cond.Proportion = jffff;
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
                    foreach (var m in payproduct1)
                    {

                        paymoney1 += m.PayMoney * 1;
                    }

                    //修改业绩
                    cond.KakouYeji = ConvertValueHelper.ConvertDecimalValue((paymoney / joinList.Count).ToString("f2"));//卡扣业绩

                    cond.Yeji = ConvertValueHelper.ConvertDecimalValue((paymoney1 / joinList.Count).ToString("f2"));//现金业绩
                    if (info.Type == 1)
                    {
                        cond.KakouYeji_Xiangmu = cond.KakouYeji;
                        cond.Yeji_Xiangmu = cond.Yeji;
                    }
                    else if (info.Type == 2)
                    {
                        cond.Yeji_Chanpin = cond.Yeji;
                        cond.KakouYeji_Chanpin = cond.KakouYeji;
                    }
                    else if (info.Type == 3)
                    {
                        if (info.IsChongzhi == 1)//如果是充值单据,直接记为储值卡业绩
                        {
                            cond.Yeji_Chuzhika = cond.Yeji;
                        }
                        else
                        {
                            var model = baseinfoBLL.GetPrePayCardModel(info.ProdcuitID);
                            if (model.Type == 1) { cond.Yeji_Chuzhika = cond.Yeji; }
                            if (model.Type == 2) {
                               
                                try
                                {
                                    string conStr = "server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft";//连接字符串  
                                    SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                                    conText.Open();//打开数据库  
                                    string sqls = @"SELECT top 1  BaseType,* from EYB_tb_MainCard  WHERE HospitalID = " + LoginUserEntity.ParentHospitalID + @" AND isnull( IsActive, 0 ) = 0 and ID = " + model.ID + @"  ";
                                    SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
                                    SqlDataReader dr;//创建DataReader对象  
                                    dr = comText.ExecuteReader();//执行查询  
                                    while (dr.Read())//判断数据表中是否含有数据  
                                    {
                                        var date = dr;
                                        ViewBag.BaseType = date["BaseType"].ToString();
                                    }
                                    dr.Close();//关闭DataReader对象  
                                }
                                catch
                                {

                                }
                                if (ViewBag.BaseType == "2") {
                                    cond.Yeji_Daxiangmu = cond.Yeji;
                                    cond.KakouYeji_Daxiangmu = cond.KakouYeji;

                                } else if (ViewBag.BaseType == "1") {
                                    cond.Yeji_Liaochengka = cond.Yeji;
                                    cond.KakouYeji_Liaocheng = cond.KakouYeji;
                                }
                                 //cond.KakouYeji_Liaocheng = cond.KakouYeji;
                            }
                        }
                        if (info.InfoBuyType == 1 || info.InfoBuyType == 2)
                        {
                            cond.KakouYeji = 0;
                            cond.KakouYeji_Chanpin = 0;
                            cond.KakouYeji_Huankuan = 0;
                            cond.KakouYeji_Liaocheng = 0;
                            cond.KakouYeji_Xiangmu = 0;
                        }
                    }
                    //购买疗程卡没有消耗
                    cond.XiaoHao = 0;
                    cond.XiaoHao_kakou = 0;
                    cond.XiaoHao_Liaocheng = 0;
                    if (LoginUserEntity.HospitalID == 1017)
                    {
                        if (YinLianCard1 > 0 || ShengyuCash1 > 0)
                        {
                            cond.KakouYeji = 0;
                        }
                    }
                    cashBll.UpdateJoinUserYejiForPay(cond);
                }
            }
            #endregion
            #region ==充值办卡模块独立写入日志数据==

            //这里需要更新订单详情,所以要重新请求一次
            var neworderInfoList = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = entity.OrderNo });

            foreach (var info in neworderInfoList)
            {
                ArchivesChangeLogEntity ac = new ArchivesChangeLogEntity();
                ac.CreateTime = DateTime.Now;
                ac.OrderNo = entity.OrderNo;
                ac.ChangeType = "Add";
                ac.ConsumptionUnitPrice = info.AllPrice;
                if (info.InfoBuyType == 6 || info.InfoBuyType == 2)//如果是储值卡或者疗程卡,则分割字结余字符串,然后再写入每条记录
                {
                    String[] tempstrlist = info.CardBalanceID.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in tempstrlist)
                    {
                        var _usermodel = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(s) });
                        ac.ConsumptionUnitPrice = _usermodel.AllTiems;
                        ac.CourseCardIDList = s;
                        ac.LogType = info.InfoBuyType == 6 ? 2 : 1;
                        if (ac.LogType == 1)
                        {
                            ac.ConsumptionUnitPrice = info.AllPrice;
                        }
                        IpatBLL.AddArchivesChangeLog(ac);
                    }
                }
                else if (info.InfoBuyType == 1)//充值单据直接进这里
                {
                    ac.CourseCardIDList = info.CardID.ToString();
                    ac.LogType = 1;
                    IpatBLL.AddArchivesChangeLog(ac);
                }
            }

            #endregion

            //由于微信端内容需要,修改订单状态需要放在最后处理.
            entity.Statu = (int)OrderStatu.已结算;
            entity.Statu = 1;
            entity.ActurePrice = acturePrice;
            entity.AllQianprice = allQianprice;
            entity.QianPrice = qianPrice;
            result = cashBll.UpdateOrder(entity);
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
                    // var S = new WeiXinMessage().SendBuyCardMessage(entity.OrderNo);
                    new WeiXinMessage().SendMessage(entity.OrderNo);
                });
            }
            catch (Exception e)
            {
                LogWriter.WriteInfo(e.Message, "发送买卡消息失败");
            }
            return Json(result);
        }
        private IList<IntegralRecordEntity> GetBaseTypeList(string sqlstr, int filter = 0)
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
        public ActionResult UpdateOrderPuchanseCard2(OrderEntity entity, string CardIDstr, string ordertimesstr, string cardidarr, string txtChuzhikaarr, string OtherPayment, string IsCashValue, string IsGiveValue, string OtherPaymentValue, string ismaika, string discountlist, string projectlisr, string hiddenToken, string idlist, string couponNo, decimal CouponPay, decimal depositValue,string txtCreateTime)
        {

            // LogWriter.WriteInfo("后台传入的项目：",  projectlisr );
            var OrderModel = cashBll.GetOrderModel(entity);
            if (OrderModel.Statu == 1)
            {
                return Json(-1);
            }
            //防止重复提交
            if (Session["hiddenToken1"] == null)
            {
                Session["hiddenToken1"] = hiddenToken;
            }
            else
            {
                if (Session["hiddenToken1"] == hiddenToken)
                {
                    return Json(-99);
                }
            }
            if (txtCreateTime==null||txtCreateTime=="")
            {
                //yyyy-MM-dd
                txtCreateTime = DateTime.Now.ToString("yyyy-MM-dd");
            }
            int count11111 = cashBll.Patient_UpdateLastTime1(entity.UserID, Convert.ToDateTime(txtCreateTime));
            entity.ActurePrice = entity.Price + entity.QianPrice;
            entity.AllQianprice = entity.QianPrice;
            var acturePrice = entity.ActurePrice;
            var allQianprice = entity.QianPrice;
            var qianPrice = entity.QianPrice;
            decimal otherCash = 0;//其他方式支付的 现金部分
            decimal Otherquan = 0;//其他劵类支付  部分
            decimal jffff = 0;
            //修改订单为已支付*(这个要放到最后才执行)
            int result = 1;
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            Dictionary<int, int> isgiveDic = new Dictionary<int, int>();
            Dictionary<int, decimal> otherpay = new Dictionary<int, decimal>();//其他支付方式金额
            Dictionary<int, int> otherpaytype = new Dictionary<int, int>();//其他支付方式类型

            decimal allzengchuzhikaPay = 0;//获取赠送储值卡支付金额
            //修改员工业绩的有效性

            #region ==订单支付方式==
            if (result > 0)
            {
                //添加订单支付方式
                PaymentOrderEntity poe = new PaymentOrderEntity();

               
                //其他支付方式
                if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                {
                    string[] strpay = OtherPayment.Split(',');
                    string[] otherPaymentValuelist = OtherPaymentValue.Split(',');
                    string[] isCashValueStr = IsCashValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//此字段判断是否是 现金还是券类 1 现金 2 券类
                    for (int i = 0; i < strpay.Length; i++)
                    {
                        poe.OrderNo = entity.OrderNo;
                        poe.PayType = ConvertValueHelper.ConvertIntValue(strpay[i]);
                        poe.PayMoney = ConvertValueHelper.ConvertDecimalValue(otherPaymentValuelist[i]);
                        otherpay.Add(poe.PayType, poe.PayMoney);
                        poe.BalanceMoney = 0;
                        poe.BalanceTiems = 0;
                        poe.CardID = 0;
                        if (ConvertValueHelper.ConvertIntValue(isCashValueStr[i]) == 1)
                        {
                            otherCash = otherCash + poe.PayMoney;
                            poe.ParentPayType = (int)ParentPayType.现金;
                        }
                        else
                        {
                            Otherquan = Otherquan + poe.PayMoney;
                            poe.ParentPayType = (int)ParentPayType.券类;
                        }
                        otherpaytype.Add(poe.PayType, poe.ParentPayType);//其他支付方式
                        poe.PayName = sysbll.GetBaseinfoEntity(ConvertValueHelper.ConvertIntValue(strpay[i])).InfoName;
                        cashBll.AddPaymentOrder(poe);
                    }
                }
                if (entity.Xianjin > 0)//现金
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.现金;
                    poe.PayMoney = entity.Xianjin;
                    poe.PayName = "现金";
                    poe.BalanceMoney = 0;
                    poe.BalanceTiems = 0;
                    poe.CardID = 0;
                    poe.ParentPayType = (int)ParentPayType.现金;
                    cashBll.AddPaymentOrder(poe);
                }
                if (entity.Yinlianka > 0)//银联卡
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.银联卡;
                    poe.PayName = "银联卡";
                    poe.PayMoney = entity.Yinlianka;
                    poe.BalanceMoney = 0;
                    poe.BalanceTiems = 0;
                    poe.CardID = 0;
                    poe.ParentPayType = (int)ParentPayType.现金;
                    cashBll.AddPaymentOrder(poe);
                }

                if (entity.Chuzhika != 0)//储值卡
                {
                    poe.OrderNo = entity.OrderNo;
        
                    poe.PayType = (int)PaymentEnum.储值卡;
                  
                    poe.BalanceTiems = 0;
                    //扣除卡类金额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        string[] ordertimesstrlist = txtChuzhikaarr.Split(',');
                        string[] caridlist = cardidarr.Split(',');
                        string[] isGiveValueList = IsGiveValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            if (isGiveValueList[f] == "1")
                            {
                                allzengchuzhikaPay = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) +
                                                     allzengchuzhikaPay;
                            }


                            dic.Add(ConvertValueHelper.ConvertIntValue(caridlist[f]), ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]));
                            isgiveDic.Add(ConvertValueHelper.ConvertIntValue(caridlist[f]), ConvertValueHelper.ConvertIntValue(isGiveValueList[f]));
                            result = cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Price = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) });

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
                            
                            //获取这张支付卡的详细信息
                            var cardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]) });
                            //判断是否是消费积分
                            if (cardBalance.ProjectID == 40112 || cardBalance.ProjectID == 42221)
                            {
                                poe.PayType = 8;
                                poe.ParentPayType = 8;
                                var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                                jffff = intrgral.Integral;
                                intrgral.AllIntegral = mainCardBalanceDetail.Price;
                                intrgral.Integral = mainCardBalanceDetail.Price;
                                int count = IpatBLL.UpdateIntegral(intrgral);

                                IntegralOperationEntity entity1 = new IntegralOperationEntity();
                                entity1.Memo = "购买疗程卡";
                                entity1.Name = "一";
                                decimal PayMoney1 = 0;

                                if (mainCardBalanceDetail.Price >= 1)
                                {
                                    PayMoney1 = jffff - mainCardBalanceDetail.Price;
                                }
                                else
                                {
                                    PayMoney1 = jffff;
                                }
                                entity1.Number = PayMoney1;
                                entity1.UserID = entity.UserID;
                                entity1.OperationType = 2;
                                var id = IpatBLL.AddIntegralOperation(entity1);
                                IntegralRecordEntity ir1 = new IntegralRecordEntity();
                                ir1.UserID = entity.UserID;
                                ir1.Integral = PayMoney1;
                                ir1.CreateTime = DateTime.Now;
                                ir1.OrderNO = "购买疗程卡";
                                ir1.DocumentNumber = "购买疗程卡";
                                ir1.IntegralOperationID = id;
                                IpatBLL.AddIntegralRecord(ir1);//增加积分记录

                            }
                            else
                            {
                                poe.ParentPayType = (int)ParentPayType.储值卡;
                                poe.PayType = (int)ParentPayType.储值卡;
                            }
                            var chuzhika = baseinfoBLL.GetPrePayCardModel(cardBalance.ProjectID);
                            poe.PayName = chuzhika.Name;
                            poe.PayMoney = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]);
                            poe.CardBalanceID = ConvertValueHelper.ConvertIntValue(caridlist[f]);
                            poe.IsGive = ConvertValueHelper.ConvertIntValue(isGiveValueList[f]);
                            poe.CardID = cardBalance.CardID;
                            poe.BalanceMoney = cardBalance.Price;
                            poe.ids = poe.CardBalanceID.ToString();
                            cashBll.AddPaymentOrder(poe);
                        }
                    }



                }
                if (entity.Huakouka != 0)//划扣次卡
                {
                    poe.OrderNo = entity.OrderNo;
                    poe.PayType = (int)PaymentEnum.划扣次数卡;
                    poe.ParentPayType = (int)ParentPayType.划卡;
                    poe.BalanceMoney = 0;
                    //减去相应的卡类次数
                    if (!String.IsNullOrEmpty(CardIDstr))
                    {
                        string[] caridlist = CardIDstr.Split(',');
                        string[] ordertimesstrlist = ordertimesstr.Split(',');
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            //修改用户余额记录,根据ID                      CardID是用户约记录的ID
                            cashBll.UpdateMainCardBalanceTime(new MainCardBalanceEntity { CardID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Tiems = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]) });
                            //获取这张支付卡的详细信息
                            var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]) });
                            var xiangmu = baseinfoBLL.GetProjectModel(CardBalance.ProjectID);
                            poe.PayName = xiangmu.Name;
                            poe.PayMoney = xiangmu.RetailPrice * ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]);
                            poe.CardBalanceID = ConvertValueHelper.ConvertIntValue(caridlist[f]);
                            poe.CardID = CardBalance.CardID;
                            poe.PayTime = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]);
                            poe.BalanceTiems = CardBalance.Tiems;
                            poe.ids = poe.CardBalanceID.ToString();
                            cashBll.AddPaymentOrder(poe);
                        }
                    }
                }
            }
            #endregion
            //if (result > 0)
            //{
            //    ////添加订单支付方式
            //    //其他支付方式
            //    if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
            //    {
            //        string[] strpay = OtherPayment.Split(',');
            //        string[] OtherPaymentValuelist = OtherPaymentValue.Split(',');
            //        for (int i = 0; i < strpay.Length; i++)
            //        {
            //            cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = ConvertValueHelper.ConvertIntValue(strpay[i]), PayMoney = ConvertValueHelper.ConvertIntValue(OtherPaymentValuelist[i]) });
            //        }
            //    }
            //    if (entity.Xianjin > 0)//现金
            //    {
            //        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.现金, PayMoney = entity.Xianjin });
            //    }
            //    if (entity.Yinlianka > 0)//银联卡
            //    {
            //        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.银联卡, PayMoney = entity.Yinlianka });
            //    }
            //    if (entity.Chuzhika > 0)//储值卡
            //    {
            //        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.储值卡, PayMoney = entity.Chuzhika });
            //        //扣除卡类金额
            //        if (!String.IsNullOrEmpty(cardidarr))
            //        {
            //            string[] ordertimesstrlist = txtChuzhikaarr.Split(',');
            //            string[] caridlist = cardidarr.Split(',');
            //            for (int f = 0; f < caridlist.Length; f++)
            //            {
            //                dic.Add(ConvertValueHelper.ConvertIntValue(caridlist[f]), ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]));
            //                cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Price = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) });
            //            }
            //        }
            //    }
            //    if (entity.Huakouka > 0)//划扣次卡
            //    {
            //        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.划扣次数卡, PayMoney = entity.Huakouka });
            //        //减去相应的卡类次数
            //        if (!String.IsNullOrEmpty(CardIDstr))
            //        {
            //            string[] caridlist = CardIDstr.Split(',');
            //            string[] ordertimesstrlist = ordertimesstr.Split(',');
            //            for (int f = 0; f < caridlist.Length; f++)
            //            {
            //                //修改用户余额记录,通常一个客户只能买一个卡类型，多个可充值充值,所以根据CardID修改，没有根据ID
            //                cashBll.UpdateMainCardBalanceTime(new MainCardBalanceEntity { CardID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Tiems = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]) });
            //            }
            //        }
            //    }
            //}
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
            decimal ShengyuCash1 = entity.Xianjin;
            decimal ShengyuCashMoney = entity.Xianjin;//积分写入
            //已支付现金金额
            decimal PayCash = 0;
            //剩余银联卡部分
            decimal YinLianCard = entity.Yinlianka;
            decimal YinLianCard1 = entity.Yinlianka;
           // decimal YinLianCard1 = 0;
            //已支付银联卡部分
            decimal PayCarded = 0;
            //剩余储值卡
            decimal Chuzhika = entity.Chuzhika;
            //已支付银联卡部分
            decimal ChuzhikaPayed = 0;
            decimal otherpayprice = 0;//其他已经付款的
            //储值卡集合
            List<int> cardlist = new List<int>();
            cardlist.AddRange(dic.Keys);
            //其他支付方式
            List<int> otherpaylist = new List<int>();
            otherpaylist.AddRange(otherpay.Keys);

            foreach (var info in orderInfoList)
            {
                var ModelCard1 = cashBll.GetMainCardModel(info.ProdcuitID);
                var cardmodel = baseinfoBLL.GetPrePayCardModel(info.ProdcuitID);
                if (cardmodel.IsGive == 1) { continue; }
                int row, pagecount;
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                //新的产品或者项目
                PayCash = 0;
                PayCarded = 0;
                ChuzhikaPayed = 0;
                otherpayprice = 0;
                PaymentOrderProductEntity conditon = new PaymentOrderProductEntity();
                conditon.AllPrice = info.AllPrice;//订单产品总价
                conditon.OrderInfoID = info.ID;//支付订单产品
                LogWriter.WriteInfo("现金：" + ShengyuCash + "，银联卡：" + YinLianCard, "产品支付方式：" + entity.OrderNo);
                #region==支付==

                #region 现金
                if (ShengyuCash > 0)//现金
                {
                    conditon.PayType = (int)PaymentEnum.现金;
                    if (ShengyuCash >= info.AllPrice)//如果现金部分超出实际产品价格
                    {


                        if (LoginUserEntity.HospitalID==1017)
                        {
                            var paymentOrderList = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = info.OrderNo });
                            paymentOrderList = paymentOrderList.Where(t => t.PayName == "积分消费").ToList();
                            if (paymentOrderList.Count == 1)
                            {
                                conditon.PayMoney = ModelCard1.Price - ModelCard1.Proportion;
                            }
                            else
                            {
                                conditon.PayMoney = info.AllPrice;
                            }
                           
                        }
                        else
                        {
                            conditon.PayMoney = info.AllPrice;
                        }
                        
                        ShengyuCash = ShengyuCash - conditon.PayMoney;
                        PayCash = conditon.PayMoney;
                        //添加产品支付方式
                        conditon.ParentPayType = (int)ParentPayType.现金;
                        cashBll.AddPaymentOrderProduct(conditon);
                        continue;//跳出循环
                    }
                    else
                    { //现金部分不够
                        conditon.PayMoney = ShengyuCash;
                        PayCash = ShengyuCash;
                        ShengyuCash = 0;
                        //添加产品支付方式
                        conditon.ParentPayType = (int)ParentPayType.现金;
                        cashBll.AddPaymentOrderProduct(conditon);
                    }
                }//现金支付

                #endregion

                #region  银联卡
                if (YinLianCard > 0)//银联卡
                {
                    //pppp
                  //  YinLianCard1 = YinLianCard;
                    conditon.PayType = (int)PaymentEnum.银联卡;
                    if (YinLianCard >= info.AllPrice)//如果银联卡部分超出实际产品价格
                    {
                        if (PayCash > 0)//支付了部分现金
                        {
                            conditon.PayMoney = info.AllPrice - PayCash;
                            YinLianCard = YinLianCard - conditon.PayMoney;
                            //添加产品支付方式
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                        else
                        {
                            if (LoginUserEntity.HospitalID==1017)
                            {
                                var paymentOrderList = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = info.OrderNo });
                                paymentOrderList = paymentOrderList.Where(t=>t.PayName=="积分消费").ToList();
                                if (paymentOrderList.Count==1)
                                {
                                    if (ModelCard1.ID > 0)
                                    {
                                        conditon.PayMoney = ModelCard1.Price - ModelCard1.Proportion;
                                    }
                                }
                                else
                                {
                                    conditon.PayMoney = info.AllPrice;
                                }

                            }
                            else
                            {
                                conditon.PayMoney = info.AllPrice;
                            }
                          
                            YinLianCard = YinLianCard - conditon.PayMoney;
                            //添加产品支付方式
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                    }
                    else
                    { //银联卡部分不够
                        if (PayCash > 0)//支付了部分现金
                        {
                            if (PayCash + YinLianCard >= info.AllPrice)
                            {
                                conditon.PayMoney = info.AllPrice - PayCash;
                                YinLianCard = YinLianCard - conditon.PayMoney;
                                //添加产品支付方式
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue;//跳出循环
                            }
                            else
                            {
                                // conditon.PayMoney = YinLianCard + PayCash;
                                conditon.PayMoney = YinLianCard;
                                YinLianCard = 0;
                                PayCarded = conditon.PayMoney;
                                //添加产品支付方式
                                conditon.ParentPayType = (int)ParentPayType.现金;
                                cashBll.AddPaymentOrderProduct(conditon);
                            }

                        }
                        else
                        {
                            conditon.PayMoney = YinLianCard;
                            YinLianCard = 0;
                            PayCarded = conditon.PayMoney;
                            //添加产品支付方式
                            conditon.ParentPayType = (int)ParentPayType.现金;
                            cashBll.AddPaymentOrderProduct(conditon);

                        }
                    }

                }//银联卡支付

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
                        var manCardid = valu;//用户余额记录ID
                        var manPrice = dic[valu];//支付金额
                        if (manPrice > 0)
                        {
                            conditon.PayType = (int)PaymentEnum.储值卡;
                            if (manPrice >= info.AllPrice)//如果储值卡部分超出实际产品价格
                            {
                                if (PayCash + PayCarded > 0)//支付了部分现金和银联
                                {
                                    conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                    dic.Remove(manCardid);
                                    dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额
                                    conditon.ParentPayType = (int)ParentPayType.储值卡;
                                    //添加产品支付方式 g
                                    conditon.CardID = manCardid;
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//终止循环
                                }
                                else
                                {
                                    conditon.PayMoney = info.AllPrice;
                                    dic.Remove(manCardid);
                                    dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额
                                    conditon.CardID = manCardid;
                                    //添加产品支付方式
                                    conditon.ParentPayType = (int)ParentPayType.储值卡;
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//跳出循环
                                }
                            }
                            else
                            { //储值卡部分不够
                                if (PayCash + PayCarded > 0)//支付了部分现金和银联
                                {
                                    if (PayCash + PayCarded + manPrice > info.AllPrice)
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                        // Chuzhika = Chuzhika - conditon.PayMoney;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额
                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break;//跳出循环
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
                                        conditon.ParentPayType = (int)ParentPayType.储值卡;
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
                                    conditon.ParentPayType = (int)ParentPayType.储值卡;
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 其他支付方式
                //其他支付方式
                if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                {
                    foreach (var PayType in otherpaylist)
                    {
                        var manCardid = PayType;//用户余额记录ID
                        var nowprice = otherpay[PayType];//支付金额
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
                            if (nowprice > Math.Abs(info.AllPrice))//如果大于实际产品价格
                            {
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联和储值卡
                                {
                                    if (LoginUserEntity.HospitalID == 1017)
                                    {
                                        var paymentOrderList = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = info.OrderNo });
                                        paymentOrderList = paymentOrderList.Where(t => t.PayName == "积分消费").ToList();
                                        if (paymentOrderList.Count==1)
                                        {
                                            //var ModelCard = cashBll.GetMainCardModel(info.ProdcuitID);
                                            conditon.PayMoney = ModelCard1.Price - ModelCard1.Proportion;
                                        }
                                    }
                                    else
                                    {
                                        conditon.PayMoney = info.AllPrice < 0 ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed) : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
                                    }
                                    nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                    otherpay.Remove(PayType);
                                    otherpay.Add(PayType, nowprice);
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//跳出循环
                                }
                                else
                                {
                                    conditon.PayMoney = Math.Abs(info.AllPrice) - Math.Abs(otherpayprice);
                                    nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                    otherpay.Remove(PayType);
                                    otherpay.Add(PayType, nowprice);
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//跳出循环
                                }
                            }
                            else
                            { //银联卡部分不够111
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed + nowprice > Math.Abs(info.AllPrice))
                                    {
                                        conditon.PayMoney = info.AllPrice < 0 ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed - otherpayprice) : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed - otherpayprice);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                        otherpay.Remove(PayType);
                                        otherpay.Add(PayType, nowprice);

                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break;//跳出循环
                                    }
                                    else
                                    {
                                        otherpayprice = conditon.PayMoney = info.AllPrice < 0 ? -nowprice : nowprice;
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


            #region ==添加储值记录和储值关系==
            entity = cashBll.GetOrderModel(entity);//获取订单详细
            OrderinfoEntity oi = new OrderinfoEntity();
            oi.OrderNo = entity.OrderNo;
            var orderinfolist = cashBll.GetOrderinfoList(oi);
            int o = 0;
            //疗程卡ID集合
            string[] projectidlist = idlist.Split('$');
            //项目ID集合
            string[] projectarray = projectlisr.Split('^');
            foreach (var orderinfo in orderinfolist)
            {
                #region ==获取尾款==
                decimal weikuan = 0;
                //如果存在欠款，修改用户欠款情况
                if (entity.QianPrice != 0)
                {
                    //如果是买卡欠钱,先获取当前订单详情的支付信息,再用总价减去所有的支付金额,剩下的就是欠款.
                    //判断是否是买卡,直接判断oldprice的价格是否为0,不为0则为买卡
                    if (ismaika == "1" && orderinfo.OldPrice != 0)
                    {
                        var zhifulist = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = orderinfo.ID });//获取支付详细列表
                        decimal zhifuprice = 0;
                        foreach (var zhifu in zhifulist)     //循环获得一共支付金额
                        {
                            zhifuprice += zhifu.PayMoney;
                        }
                        weikuan = orderinfo.AllPrice - zhifuprice;//订单详情的总价减去支付总价就是尾款价格
                    }
                }
                #endregion

                #region //逐条添加购买关系
                UserCardEntity usercard = new UserCardEntity();
                usercard.CardID = orderinfo.CardID;
                usercard.CreateTime = DateTime.Now;
                usercard.IsActive = 1;
                usercard.UserID = entity.UserID;
                //  usercard.Price = GetAllPrice(orderinfo.CardID, orderinfo.Price);//第一个参数是卡的ID,第二个是卡的原有价格
                usercard.Price = orderinfo.AllPrice;//第一个参数是卡的ID,第二个是卡的原有价格
                long usercardid = cashBll.AddUserCard(usercard);
                #endregion


                #region //添加用户余额记录
                //如果详情里面的IsChongzhi字段为1,则是充值单,则直接加
                if (orderinfo.IsChongzhi == 1)
                {
                    var mcb = new MainCardBalanceEntity();
                    //mcb.ID = orderinfo.ProdcuitID;  //充值操作修改  edit by kuang  2019-06-01
                    mcb.ID = Convert.ToInt32(orderinfo.CardBalanceID);
                    mcb.Price = orderinfo.Price;
                    cashBll.RechargeMoney(mcb);

                    //用户储值卡余额明细日志
                    var mainCardBalance = cashBll.GetMainCardBalanceModel(mcb);
                    cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                    {
                        CardBalanceId = mcb.ID,
                        OrderNo = orderinfo.OrderNo,
                        Type = (int)MainCardBalanceDetailType.充值,
                        Amount = mcb.Price,
                        Balance = mainCardBalance.Price,
                        OldAmount = mainCardBalance.Price,
                        CreateTime = DateTime.Now
                    });
                }

                IList<ProjectProductEntity> pplist = new List<ProjectProductEntity>();
                //string projsssss = "";
                // string[] projectarray = projectlisr.Split(new Char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);

                Dictionary<string, string> dct = new Dictionary<string, string>();
                for (int i = 0; i < projectidlist.Length; i++)
                {
                    if (!String.IsNullOrEmpty(projectidlist[i]))
                    {
                        dct.Add(projectidlist[i], projectarray[i]);
                    }
                }
                if (projectarray.Length >= o && !string.IsNullOrEmpty(projectlisr))   //如果当前数据集合大于现在循环的次数,则表示产品关系是前台传入
                {     //数据格式2|13|110.76|1440|180.00,1|5|218.00|1090|218.00
                    string thelist = dct[orderinfo.CardID.ToString()];// projectarray[o];
                    if (!string.IsNullOrEmpty(thelist))
                    {
                        if (thelist == "0")//如果是默认的0,则表示读取默认数据
                        {
                            pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = orderinfo.CardID });//获取所有的产品对应关系
                        }
                        else
                        {
                            //string[] infoarray =thelist.Split(',');
                            string[] infoarray = String.IsNullOrEmpty(dct[orderinfo.CardID.ToString()]) ? thelist.Split(',') : dct[orderinfo.CardID.ToString()].Split(',');
                            foreach (var info in infoarray)
                            {
                                string[] ia = info.Split('|');
                                ProjectProductEntity ppe = new ProjectProductEntity();
                                ppe.RealityPrice = Convert.ToDecimal(ia[3]) / (Convert.ToInt32(ia[1]) == 0 ? 1 : Convert.ToInt32(ia[1]));
                                ppe.AllPrice = Convert.ToDecimal(ia[3]);
                                ppe.ConsumptionPrice = Convert.ToDecimal(ia[2]);
                                ppe.ProjectID = Convert.ToInt32(ia[0]);
                                ppe.Price = ppe.RealityPrice; //修改原因:因为赠送卡总金额为0时,这个价格需要改为0.故在此处修改.原来代码为:Convert.ToDecimal(ia[4]);
                                ppe.ProgrammeID = orderinfo.ProdcuitID;//父卡ID是当前订单详情的ID
                                ppe.Qualtity = Convert.ToInt32(ia[1]);
                                ppe.Type = 2;    //这里默认是项目

                                pplist.Add(ppe);
                                //projsssss = projsssss + ppe.ProjectID + ",";
                            }
                        }
                    }
                }
                else
                {
                    pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = orderinfo.CardID });//获取所有的产品对应关系
                }
                // LogWriter.WriteInfo("疗程卡ID:" + orderinfo.CardID, "项目："+projsssss);
                MainCardBalanceEntity cardbalance = new MainCardBalanceEntity();
                cardbalance.CardID = orderinfo.CardID;
                cardbalance.UserCardID = Convert.ToInt32(usercardid);
                cardbalance.UserID = entity.UserID;
                cardbalance.Type = 1;
                cardbalance.OrderNo = entity.OrderNo;
                IList<ProjectProductEntity> pp = new List<ProjectProductEntity>();
                long cbalance = 0;
                // 添加卡对应的项目 --添加账户余额
                foreach (ProjectProductEntity ppentity in pplist)
                {
                    if (ppentity.Type != 3)
                    {
                        cardbalance.AllPrice = ppentity.AllPrice;//初始化的时候就是这张卡的默认金额
                        cardbalance.AllTiems = ppentity.Qualtity;
                        cardbalance.KouTimes = 0;
                        //修复日期:20160428
                        //修复代码:L0428001
                        //修复范围:修复购买疗程卡时默认下的价格的取值错误.
                        //代码范围:本行以下5行
                        decimal consumptionPrice = 0;
                        if (ppentity.Type == 2)
                        {
                            //decimal dd = ShengyuCash1;
                           
                            //if (LoginUserEntity.HospitalID==1017)
                           // {
                                cardbalance.Price = ppentity.AllPrice / (ppentity.Qualtity == 0 ? 1 : ppentity.Qualtity);//如果是默认的疗程卡,则用总金额除以总次数来获取价格
                            if (LoginUserEntity.HospitalID==1017)
                            {
                                var paymentOrderList = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = orderinfo.OrderNo });
                                paymentOrderList = paymentOrderList.Where(t => t.PayName == "积分消费").ToList();
                                if (paymentOrderList.Count==1)
                                {
                                    var ModelCard = cashBll.GetMainCardModel(orderinfo.ProdcuitID);
                                    if (ModelCard.ID > 0)
                                    {
                                        if (ModelCard.Price>= ModelCard.Proportion)
                                        {
                                            ShengyuCash1 = ModelCard.Price - ModelCard.Proportion;
                                        }
                                        else
                                        {
                                            ShengyuCash1 = ModelCard.Price - paymentOrderList[0].PayMoney;
                                            // ShengyuCash1 = ModelCard.Proportion- ModelCard.Price;
                                        }
                                            
                                        
                                        consumptionPrice = ShengyuCash1 / (ppentity.Qualtity == 0 ? 1 : ppentity.Qualtity);//如果是默认的疗程卡,则用总金额除以总次数来获取价格
                                    }
                                }
                                else
                                {
                                    consumptionPrice = ppentity.AllPrice / (ppentity.Qualtity == 0 ? 1 : ppentity.Qualtity);
                                }
                                // var iiii = orderinfo.ProdcuitID;
                            }
                            //}
                            //else
                           // {
                                cardbalance.Price = ppentity.AllPrice / (ppentity.Qualtity == 0 ? 1 : ppentity.Qualtity);//如果是默认的疗程卡,则用总金额除以总次数来获取价格
                           // }
                        }
                        else
                        {
                            cardbalance.Price = ppentity.RealityPrice == 0 ? ppentity.Price : ppentity.RealityPrice;//这里如果是从数据库读取出来的,实际价格就是单价,如果是写入进来的,则不是.
                        }
                        cardbalance.ProjectID = ppentity.ProjectID;
                        LogWriter.WriteInfo("购买卡项操作：", "疗程卡ID：" + orderinfo.CardID + "项目ID：" + ppentity.ProjectID);
                        cardbalance.Tiems = ppentity.Qualtity;
                        if (ppentity.Type == 1)
                        {
                            cardbalance.AllTiems = 0;
                            cardbalance.Tiems = 0;
                        }
                        cardbalance.Type = ppentity.Type;
                        cardbalance.UserID = entity.UserID;
                        if (LoginUserEntity.HospitalID==1017)
                        {
                            cardbalance.ConsumePrice = consumptionPrice;
                        }
                        else
                        {
                            cardbalance.ConsumePrice = ppentity.ConsumptionPrice;
                        }
                        cardbalance.ExpireTime = GetExpireTime(orderinfo.CardID);
                        cardbalance.MemoContent = TempData["MemoContent"] + "";
                        cbalance = cashBll.AddMainCardBalance(cardbalance);
                        if (cardbalance.Type == 1)
                        {
                            //用户储值卡余额明细日志
                            cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                            {
                                CardBalanceId = Convert.ToInt32(cbalance),
                                OrderNo = orderinfo.OrderNo,
                                Type = (int)MainCardBalanceDetailType.充值,
                                Amount = cardbalance.Price,
                                Balance = cardbalance.Price,
                                OldAmount = 0,
                                CreateTime = DateTime.Now
                            });
                        }
                        var newinfo = cashBll.GetOrderinfoModel(orderinfo.ID);
                        //添加订单详细CardBalanceID 关联表
                        cashBll.AddOrderinfoProject(new OrderinfoProjectEntity { OrderInfoID = orderinfo.ID, CardBalanceID = (int)cbalance });
                        newinfo.CardBalanceID = (newinfo.CardBalanceID == "0" ? cbalance.ToString() : newinfo.CardBalanceID + "," + cbalance.ToString());
                        cashBll.UpdateInfoCardID(newinfo);//把用户约记录的ID绑定到当前订单详情
                    }
                }
                //储值卡时需要把它本身也加入进去
                MainCardEntity maincard = baseinfoBLL.GetPrePayCardModel(orderinfo.CardID);
                maincard.Price = orderinfo.AllPrice;
                if (orderinfo.IsChongzhi != 1) //edit by kuang 2019-06-01
                {
                    if (maincard.Type == 1) //
                    {
                        cardbalance.AllPrice = maincard.Price;
                        cardbalance.AllTiems = 0;
                        cardbalance.ProjectID = maincard.ID; //储值卡,项目,产品ID存放在这个字段
                        cardbalance.CardID = Convert.ToInt32(usercardid); //这个字段接受上面返回的关系ID
                        cardbalance.CostPrice = maincard.Price;
                        cardbalance.KouTimes = 0;
                        //cardbalance.UserCardID = Convert.ToInt32(usercardid);
                        cardbalance.Name = (maincard.Type == 1 ? "储值卡主卡" : (maincard.Type == 2 ? "疗程卡项目" : "方案卡详情"));
                        cardbalance.Price = maincard.Price;
                        cardbalance.Tiems = 0;
                        cardbalance.Weikuan = weikuan; //把尾款放入到用户记录
                        cardbalance.Type = maincard.Type;
                        cardbalance.UserID = entity.UserID;
                        cardbalance.ExpireTime = GetExpireTime(orderinfo.CardID);
                        if (maincard.Type == 1)
                        {
                            cardbalance.HuaKouZheLv = Convert.ToDecimal(discountlist.Split(',')[o]);
                        }
                        cbalance = cashBll.AddMainCardBalance(cardbalance);
                        if (cardbalance.Type == 1)
                        {
                            //用户储值卡余额明细日志
                            cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                            {
                                CardBalanceId = Convert.ToInt32(cbalance),
                                OrderNo = orderinfo.OrderNo,
                                Type = (int)MainCardBalanceDetailType.充值,
                                Amount = cardbalance.Price,
                                Balance = cardbalance.Price,
                                OldAmount = 0,
                                CreateTime = DateTime.Now
                            });
                        }
                        //添加订单详细CardBalanceID 关联表
                        cashBll.AddOrderinfoProject(new OrderinfoProjectEntity { OrderInfoID = orderinfo.ID, CardBalanceID = (int)cbalance });
                        //因为现在对应ID是文字串,需要重新编辑,所以每次加载都要重新载入最新的
                        var newinfo1 = cashBll.GetOrderinfoModel(orderinfo.ID);
                        newinfo1.CardBalanceID = (newinfo1.CardBalanceID == "0" ? cbalance.ToString() : newinfo1.CardBalanceID + "," + cbalance.ToString());
                        cashBll.UpdateInfoCardID(newinfo1); //把用户约记录的ID绑定到当前订单详情
                    }
                }




                #endregion

                #region ==添加员工业绩==
                // var yeji = new JoinUserEntity();
                // yeji.ProjectID = orderinfo.ProdcuitID;
                // yeji.Type = 3;//卡类操作
                // yeji.IsZhiding = 0;//默认不指定服务
                // yeji.OrderInfoID = orderinfo.ID;
                // yeji.OrderNo = entity.OrderNo;
                // yeji.XiaoHao = 0;//买卡没有消耗
                //// yeji.Yeji = orderinfo.AllPrice;  //业绩是按照订单收到的价格
                // cashBll.AddJoinUser(yeji);
                #endregion

                #region ==添加积分==
                if (ShengyuCashMoney != 0 || YinLianCard1 != 0 || otherpayprice != 0)
                {
                    var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate");  //获取积分实体
                    decimal jifen = Convert.ToDecimal(ShengyuCashMoney + YinLianCard1 + otherpayprice) / (string.IsNullOrEmpty(usersetting.AttributeValue) ? 100000 : Convert.ToDecimal(usersetting.AttributeValue));//判断能获取多少积分
                    if (LoginUserEntity.HospitalID == 1017)
                    { }
                    else
                    {

                        IntegralRecordEntity ir = new IntegralRecordEntity();
                        ir.UserID = entity.UserID;
                        ir.DocumentNumber = entity.OrderNo;
                        ir.Integral = jifen;
                        ir.CreateTime = DateTime.Now;
                        IpatBLL.AddIntegralRecord(ir);  //添加用户积分记录

                        var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                        intrgral.AllIntegral = intrgral.AllIntegral + jifen;
                        intrgral.Integral = intrgral.Integral + jifen;

                        IpatBLL.UpdateIntegral(intrgral);
                    }
                }
                #endregion
                o++;

            }
            #endregion

            #region==重新计算员工业绩==
            //重新计算员工业绩

            string[] projectarray1 = projectlisr.Split('^');
            int item = 0;
            foreach (var info in orderInfoList)
            {

                var prid = projectarray1[item].Split('|')[0];
                int pid = Convert.ToInt32(prid);
                var productentity = baseinfoBLL.GetProjectModel(pid);

                item += 1;

                int row, pagecount;
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                foreach (var cond in joinList)
                {
                    // var list = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = entity.OrderNo });
                    //  decimal Proportion = 0;
                    // for (int i = 0; i < list.Count; i++)
                    // {
                    //  var mar = cashBll.GetMainCardModel(Convert.ToInt32(list[i].CardID));
                    // Proportion += mar.Proportion;
                    //  }
                    //var list = cashBll.GetOrderinfoList2(new OrderinfoEntity { OrderNo = entity.OrderNo });
                    //卡扣业绩


                    var payproduct = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 3 });
                    decimal paymoney = 0;
                    if (LoginUserEntity.HospitalID == 1017)
                    {
                        var minCarModel = cashBll.GetMainCardModel(info.CardID);
                        var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                        if (minCarModel.ID>0)
                        {
                            foreach (var m in payproduct)
                            {
                                decimal money = m.AllPrice - minCarModel.Proportion;
                                paymoney = money * m.HuaKouZheLv;
                                cond.Proportion = minCarModel.Proportion;
                            }
                            if (jffff >= minCarModel.Proportion)
                            {
                                cond.Proportion = minCarModel.Proportion;
                            }
                            else
                            {
                                cond.Proportion = jffff;
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
                    foreach (var m in payproduct1)
                    {
                       
                        paymoney1 += m.PayMoney * 1;
                    }

                    //修改业绩
                    cond.KakouYeji = ConvertValueHelper.ConvertDecimalValue((paymoney / joinList.Count).ToString("f2"));//卡扣业绩

                    cond.Yeji = ConvertValueHelper.ConvertDecimalValue((paymoney1 / joinList.Count).ToString("f2"));//现金业绩
                    if (info.Type == 1)
                    {
                        cond.KakouYeji_Xiangmu = cond.KakouYeji;
                        cond.Yeji_Xiangmu = cond.Yeji;
                    }
                    else if (info.Type == 2)
                    {
                        cond.Yeji_Chanpin = cond.Yeji;
                        cond.KakouYeji_Chanpin = cond.KakouYeji;
                    }
                    else if (info.Type == 3)
                    {
                        if (info.IsChongzhi == 1)//如果是充值单据,直接记为储值卡业绩
                        {
                            cond.Yeji_Chuzhika = cond.Yeji;
                        }
                        else
                        {
                            var model = baseinfoBLL.GetPrePayCardModel(info.ProdcuitID);
                            if (model.Type == 1) { cond.Yeji_Chuzhika = cond.Yeji; }
                            if (model.Type == 2) {
                                //判断产品是否是属于大项目或者特殊项目
                                if (productentity.BaseType == (int)ProductBaseType.特殊项目)
                                {
                                    cond.KakouYeji_Teshuxiangmu = cond.KakouYeji;
                                    cond.Yeji_Teshuxiangmu = cond.Yeji;
                                }
                                else if (productentity.BaseType == (int)ProductBaseType.大项目)
                                {
                                    
                                    cond.Yeji_Daxiangmu = cond.Yeji; cond.KakouYeji_Daxiangmu = cond.KakouYeji;// cond.KakouYeji_Liaocheng = cond.KakouYeji;
                                }
                                else
                                {
                                    cond.Yeji_Liaochengka = cond.Yeji; cond.KakouYeji_Liaocheng = cond.KakouYeji;
                                }
                                //cond.Yeji_Liaochengka = cond.Yeji; cond.KakouYeji_Liaocheng = cond.KakouYeji;
                            }
                        }
                        if (info.InfoBuyType == 1 || info.InfoBuyType == 2)
                        {
                            cond.KakouYeji = 0;
                            cond.KakouYeji_Chanpin = 0;
                            cond.KakouYeji_Huankuan = 0;
                            cond.KakouYeji_Liaocheng = 0;
                            cond.KakouYeji_Xiangmu = 0;
                        }
                    }
                    //购买疗程卡没有消耗
                    cond.XiaoHao = 0;
                    cond.XiaoHao_kakou = 0;
                    cond.XiaoHao_Liaocheng = 0;
                    if (LoginUserEntity.HospitalID==1017)
                    {
                        if (YinLianCard1 > 0|| ShengyuCash1>0)
                        {
                            cond.KakouYeji = 0;
                        }
                    }
                    cashBll.UpdateJoinUserYejiForPay(cond);
                }

            }
            #endregion
            #region ==充值办卡模块独立写入日志数据==

            //这里需要更新订单详情,所以要重新请求一次
            var neworderInfoList = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = entity.OrderNo });

            foreach (var info in neworderInfoList)
            {
                ArchivesChangeLogEntity ac = new ArchivesChangeLogEntity();
                ac.CreateTime = DateTime.Now;
                ac.OrderNo = entity.OrderNo;
                ac.ChangeType = "Add";
                ac.ConsumptionUnitPrice = info.AllPrice;
                if (info.InfoBuyType == 6 || info.InfoBuyType == 2)//如果是储值卡或者疗程卡,则分割字结余字符串,然后再写入每条记录
                {
                    String[] tempstrlist = info.CardBalanceID.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in tempstrlist)
                    {
                        var _usermodel = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(s) });
                        ac.ConsumptionUnitPrice = _usermodel.AllTiems;
                        ac.CourseCardIDList = s;
                        ac.LogType = info.InfoBuyType == 6 ? 2 : 1;
                        if (ac.LogType == 1)
                        {
                            ac.ConsumptionUnitPrice = info.AllPrice;
                        }
                        IpatBLL.AddArchivesChangeLog(ac);
                    }
                }
                else if (info.InfoBuyType == 1)//充值单据直接进这里
                {
                    ac.CourseCardIDList = info.CardID.ToString();
                    ac.LogType = 1;
                    IpatBLL.AddArchivesChangeLog(ac);
                }
            }

            #endregion

            //由于微信端内容需要,修改订单状态需要放在最后处理.
            entity.Statu = (int)OrderStatu.已结算;
            entity.Statu = 1;
            entity.ActurePrice = acturePrice;
            entity.AllQianprice = allQianprice;
            entity.QianPrice = qianPrice;
            result = cashBll.UpdateOrder(entity);
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
                    // var S = new WeiXinMessage().SendBuyCardMessage(entity.OrderNo);
                    new WeiXinMessage().SendMessage(entity.OrderNo);
                });
            }
            catch (Exception e)
            {
                LogWriter.WriteInfo(e.Message, "发送买卡消息失败");
            }
            return Json(result);
        }
        public class OrderInfoOver
        {
            public decimal cashamout { get; set; }
            public int cardamout { get; set; }
            public decimal allamout { get; set; }
            public string orderNo { get; set; }
            public string UserID { get; set; }
            public int maika { get; set; }

            public int IsRecharge { get; set; }
        }

        /// <summary>
        /// 获取金额总价
        /// 因为储值卡是不计算金额统计的,只算储值卡和充值
        /// </summary>
        /// <param name="PriceList"></param>
        /// <param name="Isgive"></param>
        /// <returns></returns>
        private decimal GetAllprice(string[] PriceList, string[] Isgive)
        {
            decimal result = 0;
            for (int p = 0; p < PriceList.Length; p++)
            {
                if (Isgive == null)
                {
                    result += Convert.ToDecimal(PriceList[p]);
                }
                else
                {
                    if (Isgive[p] != "1") //赠卡的赠送金额不统计进总金额里面
                    {
                        result += Convert.ToDecimal(PriceList[p]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 充值办卡添加订单方法
        /// </summary>
        /// <param name="allprice">总价</param>
        /// <param name="userid">用户ID</param>
        /// <returns></returns>
        private string AddOrder(decimal allprice, int userid, int ischizhuka, string Name, int chkcreatetime, string txtCreateTime)
        {
            OrderEntity entity = new OrderEntity();
            entity.OrderType = (int)OrderType.购买卡项;
            //汪  7-24 补单及到店时间的修改
            PatientEntity list = new PatientEntity();
            list = IpatBLL.GetPatienttEntityByID(userid);

            //罗发明 补单时间时分秒换成实际的
            //var hms = DateTime.Now.ToString("hh:mm:ss").ToString();
            var hms1 = DateTime.Now.ToLongTimeString().ToString();
            

            //var timess = "yyyy-MM-dd " + hms;

            if (chkcreatetime == 0)
            {
                //修改顾客最后到店时间
                if (txtCreateTime==""|| txtCreateTime == "undefined")
                {
                    entity.CreateTime = DateTime.Now;
                    entity.OrderTime = entity.CreateTime;
                }
                else
                {
                entity.CreateTime = Convert.ToDateTime(txtCreateTime + " " + hms1);
                //entity.CreateTime = Convert.ToDateTime(txtCreateTime + " 10:10:10");
                    entity.OrderTime = entity.CreateTime;
                }

            }
            else
            {

                //读取顾客最后到店时间，如果最后到店时间 小于补单时间，则修改最后到店时间，否则不修改
                //if (list.LastTime > entity.CreateTime)
                //{
                //    entity.CreateTime = list.LastTime;
                //}
                //else
                //{
                //    entity.CreateTime = Convert.ToDateTime(txtCreateTime + " 10:10:10");
                //}

                //entity.CreateTime = Convert.ToDateTime(txtCreateTime + " 10:10:10");
                entity.CreateTime = Convert.ToDateTime(txtCreateTime + " " + hms1);
                //entity.CreateTime = Convert.ToDateTime(txtCreateTime + " 10:10:10");
                entity.OrderTime = entity.CreateTime;
            }
            //汪 end

            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
            entity.Statu = (int)OrderStatu.处理中;
            entity.Price = allprice;
            entity.UserID = userid;
            entity.BuyType = ischizhuka;//现在添加购买类型
            entity.DocumentNumber = GetDocumentNumber(entity.CreateTime);
            entity.Name = Name;
            var result = cashBll.AddOrder(entity);
            return entity.OrderNo;//返回单据号
        }
        /// <summary>
        /// 获取新的单据编号
        /// </summary>
        /// <returns></returns>
        private string GetDocumentNumber(DateTime datetime)
        {
            var mendian = personnelBLL.HospitalEntityByID(LoginUserEntity.HospitalID);  //获取门店信息,用来获取门店前缀
            string time = datetime.ToString("MMdd");
            var count = cashBll.GetTodayOrderCount(LoginUserEntity.HospitalID, datetime);//获取总数
            var result = mendian.Prefix + time + (count + 1).ToString("d3");
            return result;
        }
        /// <summary>
        /// 添加订单详情
        /// </summary>
        /// <param name="OrderNo">订单编号</param>
        /// <param name="ProdcuitID">产品ID</param>
        /// <param name="OldPrice">原价</param>
        /// <param name="Price"></param>
        /// <param name="AllPrice"></param>
        /// <param name="CardID"></param>
        /// <returns></returns>
        private int AddOrderinfo(string OrderNo, int ProdcuitID, decimal OldPrice, decimal Price, decimal AllPrice, int CardID, int IsChongzhi, string isbuyliaocheng, string maincardid)
        {
            OrderinfoEntity orderinfo = new OrderinfoEntity();
            orderinfo.OrderNo = OrderNo;
            orderinfo.Type = 3;
            orderinfo.ProdcuitID = ProdcuitID;
            orderinfo.OldPrice = 0;
            orderinfo.Price = Price;
            orderinfo.AllPrice = AllPrice;
            orderinfo.Quantity = 1;
            orderinfo.CardID = CardID;
            orderinfo.IsChongzhi = IsChongzhi;
            orderinfo.CardBalanceID = string.IsNullOrEmpty(maincardid) ? "0" : maincardid;
            orderinfo.CardType = 2;
            var model = baseinfoBLL.GetPrePayCardModel(ProdcuitID);
            orderinfo.ProdcuitName = model.Name;
            if (orderinfo.IsChongzhi == 1)
            {
                orderinfo.InfoBuyType = 1;
            }
            else
            {
                orderinfo.InfoBuyType = (model.Type == 1 ? 2 : (model.Type == 2 ? 6 : 7));
            }
            if (!string.IsNullOrEmpty(isbuyliaocheng))
            {
                orderinfo.IsChongzhi = 99;//如果是疗程卡,把这个充值单据号改为99.前台用来识别
            }
            //var cardmodel = baseinfoBLL.GetPrePayCardModel(ProdcuitID);
            if (orderinfo.Price == 0 && model.IsGive == 1)//如果实收价格为0
            {
                orderinfo.SendAmount = orderinfo.AllPrice;
            }
            //折让金额=理论总额-实际总额    理论总额=原本单价*数量
            if (model.Price > orderinfo.AllPrice)
                orderinfo.DiscountAmount = model.Price - orderinfo.AllPrice;

            int result = cashBll.AddOrderinfo(orderinfo);
            return result;
        }
        /// <summary>
        /// 添加员工业绩
        /// </summary>
        /// <param name="OrderInfoID">订单详情ID</param>
        /// <param name="OrderNo">订单编号</param>
        /// <param name="ProjectID">产品ID</param>
        /// <param name="UserID">用户ID</param>
        /// <param name="yeji">业绩</param>
        /// <returns></returns>
        private int AddJoinUser(int OrderInfoID, string OrderNo, int ProjectID, int UserID, decimal yeji, int PatientUserID)
        {
            JoinUserEntity juentity = new JoinUserEntity();
            //juentity.IsZhiding = 0;
            var patientEntity = IpatBLL.GetPatienttEntityByID(PatientUserID);
            juentity.IsZhiding = (patientEntity.FollowUpMrUserID == UserID || patientEntity.FollowUpUserID == UserID) ? 1 : 0;// ConvertValueHelper.ConvertIntValue(IsZhidingList[i]);
            juentity.OrderInfoID = OrderInfoID;
            juentity.OrderNo = OrderNo;
            juentity.OtherTiCheng = 0;
            juentity.ProjectID = ProjectID;
            juentity.ServiceTime = 0;
            juentity.Ticheng = 0;
            juentity.Type = 3;//为储值卡,所以为3
            juentity.UserID = UserID;
            juentity.XiaoHao = 0;
            juentity.Yeji = yeji;
            var result = cashBll.AddJoinUser(juentity);
            return Convert.ToInt32(result);
        }
        #endregion



        /// <summary>
        /// 根据ID和卡售出价格获取当前卡的总价格
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cardprice"></param>
        /// <returns></returns>
        public decimal GetAllPrice(int id, decimal cardprice)
        {
            IList<ProjectProductEntity> pp = new List<ProjectProductEntity>();
            pp = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = id });

            decimal price = 0;
            price += cardprice;
            foreach (ProjectProductEntity entity in pp)
            {
                price += entity.Price;
            }

            return price;

        }

        /// <summary>
        /// 卡项详细信息页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Cardinfo()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        /// <summary>
        /// 卡项订单结算页面
        /// </summary>
        /// <returns></returns>
        public ActionResult PayOrderPagePuchanseCard()
        {
            string ddd = Request["projectlisr"];
            string dff = Request["cardidarr"];
            string OrderNo = Request["orderNo"];
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.Username = LoginUserEntity.UserName;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            var entity = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "IsOpenAutoPrint");
            var list = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = OrderNo });
            var jjff =3;
            decimal Proportion = 0;
            ViewBag.Proportion = 0;
            if (LoginUserEntity.HospitalID==1017)
            {
                if (list.Count > 1)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var mar = cashBll.GetMainCardModel(Convert.ToInt32(list[i].CardID));
                        Proportion += mar.Proportion;
                    }
                    var intrgral = IpatBLL.GetIntegralModel(Convert.ToInt32(Request.QueryString["UserID"])); //获取原来的积分列表
                    ViewBag.intrgral = intrgral.Integral;
                    if (intrgral.Integral >= Proportion)
                    {
                        ViewBag.Proportion = Proportion;
                        jjff = 3;
                    }
                    else
                    {
                        ViewBag.Proportion = intrgral.Integral;
                        jjff =2;
                    }
                }
                else
                {

                    for (int i = 0; i < list.Count; i++)
                    {
                        var mar = cashBll.GetMainCardModel(Convert.ToInt32(list[i].CardID));
                        Proportion += mar.Proportion;
                    }
                    var intrgral = IpatBLL.GetIntegralModel(Convert.ToInt32(Request.QueryString["UserID"])); //获取原来的积分列表
                    ViewBag.intrgral = intrgral.Integral;
                    if (intrgral.Integral >= Proportion)
                    {
                        ViewBag.Proportion = Proportion;
                        jjff = 3;
                    }
                    else
                    {
                        ViewBag.Proportion = intrgral.Integral;
                        jjff = 1;
                    }
                }
            }

            ViewBag.jjff = jjff;
            

            // ViewBag.Proportion = list.Sum(t => t.Proportion);
            ViewBag.IsOpenAutoPrint = entity.AttributeValue;
            return View();
        }



        #region ==结算以及结算构造==
        /// <summary>
        ///    购卡结算
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="CardIDstr"></param>
        /// <param name="ordertimesstr"></param>
        /// <param name="cardidarr"></param>
        /// <param name="txtChuzhikaarr"></param>
        /// <param name="OtherPayment"></param>
        /// <param name="OtherPaymentValue"></param>
        /// <returns></returns>
        public ActionResult UpdateOrderPuchanseCard1(OrderEntity entity, string CardIDstr, string ordertimesstr, string cardidarr, string txtChuzhikaarr, string OtherPayment, string IsCashValue, string OtherPaymentValue, string ismaika, string discountlist, string projectlisr)
        {
            var orderInfoList = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = entity.OrderNo });
            int result = UpdateOrderStatuAndQianprice(entity.OrderNo, (int)OrderStatu.已结算, entity.Price + entity.QianPrice, entity.QianPrice, true);

            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            //修改员工业绩的有效性

            //订单结算
            OrderSettlement(entity, OtherPayment, IsCashValue, OtherPaymentValue, cardidarr, txtChuzhikaarr, CardIDstr, ordertimesstr, out dic);
            AddBlance(entity, discountlist, ismaika, projectlisr);
            return Json(result);
        }

        /// <summary>
        /// 修改订单状态及欠款金额
        /// 先根据订单获取单据信息.在修改单据信息
        /// </summary>
        /// <param name="_orderno">订单编号</param>
        /// <param name="_statu">订单状态</param>
        /// <param name="_actureprice">实际支付金额</param>
        /// <param name="_allqiankuan">总欠款</param>
        /// <param name="_isupdateprice">是否更新价格</param>
        /// <returns></returns>
        public int UpdateOrderStatuAndQianprice(string _orderno, int _statu, decimal _actureprice, decimal _allqiankuan, bool _isupdateprice)
        {
            OrderEntity entity = new OrderEntity();
            entity.OrderNo = _orderno;
            entity = cashBll.GetOrderModel(entity);
            if (_isupdateprice)
            {
                entity.AllQianprice = _allqiankuan;
                entity.ActurePrice = _actureprice;
            }
            entity.Statu = _statu;
            int result = cashBll.UpdateOrder(entity);
            return result;
        }

        /// <summary>
        /// 订单结算构造
        /// </summary>
        /// <param name="entity">订单实体类</param>
        /// <param name="OtherPayment">其他结算方式</param>
        /// <param name="OtherPaymentValue">其它结算金额</param>
        /// <param name="cardidarr"></param>
        /// <param name="txtChuzhikaarr"></param>
        /// <param name="CardIDstr"></param>
        /// <param name="ordertimesstr"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public int OrderSettlement(OrderEntity entity, string OtherPayment, string IsCashValue, string OtherPaymentValue, string cardidarr, string txtChuzhikaarr, string CardIDstr, string ordertimesstr, out Dictionary<int, decimal> dic)
        {
            //以下是订单支付方式结算
            dic = new Dictionary<int, decimal>();

            #region ==其他支付方式结算==
            //其他支付方式
            if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
            {
                string[] strpay = OtherPayment.Split(',');
                string[] OtherPaymentValuelist = OtherPaymentValue.Split(',');
                for (int i = 0; i < strpay.Length; i++)
                {
                    AddPaymentOrder(entity.OrderNo, ConvertValueHelper.ConvertIntValue(strpay[i]), ConvertValueHelper.ConvertIntValue(OtherPaymentValuelist[i]), 0, 0, 0, sysbll.GetBaseinfoEntity(ConvertValueHelper.ConvertIntValue(strpay[i])).InfoName);
                }
            }
            #endregion


            #region ==银联卡结算==
            if (entity.Yinlianka > 0)//银联卡
            {
                AddPaymentOrder(entity.OrderNo, (int)PaymentEnum.银联卡, entity.Yinlianka, 0, 0, 0, "银联卡");
            }
            #endregion

            #region ==储值卡结算==
            if (entity.Chuzhika != 0)//储值卡
            {
                //扣除卡类金额
                if (!String.IsNullOrEmpty(cardidarr))
                {
                    string[] ordertimesstrlist = txtChuzhikaarr.Split(',');
                    string[] caridlist = cardidarr.Split(',');
                    for (int f = 0; f < caridlist.Length; f++)
                    {
                        dic.Add(ConvertValueHelper.ConvertIntValue(caridlist[f]), ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]));
                        var result = cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Price = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) });

                        //用户储值卡余额明细日志
                        var mainCardBalanceDetail = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity() { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]) });
                        cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                        {
                            CardBalanceId = mainCardBalanceDetail.ID,
                            OrderNo = entity.OrderNo,
                            Type = (int)MainCardBalanceDetailType.消费,
                            Amount = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]),
                            Balance = mainCardBalanceDetail.Price,
                            OldAmount = mainCardBalanceDetail.Price + ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]),
                            CreateTime = DateTime.Now
                        });

                        //获取这张支付卡的详细信息
                        var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]) });
                        var chuzhika = baseinfoBLL.GetPrePayCardModel(CardBalance.ProjectID);
                        AddPaymentOrder(entity.OrderNo, (int)PaymentEnum.储值卡, ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]), CardBalance.Price, 1, CardBalance.CardID, chuzhika.Name);
                    }
                }
            }
            #endregion

            #region ==划扣次卡结算===
            if (entity.Huakouka != 0)//划扣次卡
            {
                if (!String.IsNullOrEmpty(CardIDstr))
                {
                    string[] caridlist = CardIDstr.Split(',');
                    string[] ordertimesstrlist = ordertimesstr.Split(',');
                    for (int f = 0; f < caridlist.Length; f++)
                    {
                        //修改用户余额记录,根据ID                      CardID是用户约记录的ID
                        cashBll.UpdateMainCardBalanceTime(new MainCardBalanceEntity { CardID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Tiems = ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]) });
                        //获取这张支付卡的详细信息
                        var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]) });
                        var xiangmu = baseinfoBLL.GetProjectModel(CardBalance.ProjectID);
                        AddPaymentOrder(entity.OrderNo, (int)PaymentEnum.划扣次数卡, xiangmu.RetailPrice * ConvertValueHelper.ConvertIntValue(ordertimesstrlist[f]), ConvertValueHelper.ConvertIntValue(caridlist[f]), CardBalance.Tiems, CardBalance.CardID, xiangmu.Name);

                    }
                }
            }
            #endregion

            #region ==如果存在欠款,修改欠款==
            if (entity.QianPrice != 0)
            {
                cashBll.UpdateArrearsMoney(entity.UserID, entity.QianPrice);
            }
            #endregion


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
                var cardmodel = baseinfoBLL.GetPrePayCardModel(info.ProdcuitID);
                if (cardmodel.IsGive == 1) { continue; }
                int row, pagecount;
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                //新的产品或者项目
                PayCash = 0;
                PayCarded = 0;
                ChuzhikaPayed = 0;
                PaymentOrderProductEntity conditon = new PaymentOrderProductEntity();
                conditon.AllPrice = info.AllPrice;//订单产品总价
                conditon.OrderInfoID = info.ID;//支付订单产品
                #region==支付==

                #region 现金
                if (ShengyuCash > 0)//现金
                {
                    if (ShengyuCash >= info.AllPrice)//如果现金部分超出实际产品价格
                    {
                        ShengyuCash = ShengyuCash - info.AllPrice;
                        PayCash = info.AllPrice;
                        AddPaymentOrderProduct(info.AllPrice, info.ID, (int)PaymentEnum.现金, info.PayMoney);
                        continue;//跳出循环
                    }
                    else
                    {
                        //现金部分不够
                        PayCash = ShengyuCash;
                        ShengyuCash = 0;
                        AddPaymentOrderProduct(info.AllPrice, info.ID, (int)PaymentEnum.现金, ShengyuCash);
                    }
                }//现金支付

                #endregion

                #region  银联卡
                if (YinLianCard > 0)//银联卡
                {
                    //conditon.PayType = (int)PaymentEnum.银联卡;
                    if (YinLianCard >= info.AllPrice)//如果银联卡部分超出实际产品价格
                    {
                        if (PayCash > 0)//支付了部分现金
                        {
                            //  conditon.PayMoney = info.AllPrice - PayCash;
                            YinLianCard = YinLianCard - info.AllPrice - PayCash;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            AddPaymentOrderProduct(info.AllPrice, info.ID, (int)PaymentEnum.银联卡, info.AllPrice - PayCash);
                            continue;//跳出循环
                        }
                        else
                        {
                            conditon.PayMoney = info.AllPrice;
                            YinLianCard = YinLianCard - conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                    }
                    else
                    { //银联卡部分不够
                        if (PayCash > 0)//支付了部分现金
                        {
                            if (PayCash + YinLianCard >= info.AllPrice)
                            {
                                conditon.PayMoney = info.AllPrice - PayCash;
                                YinLianCard = YinLianCard - conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue;//跳出循环
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

                }//银联卡支付

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
                        var manCardid = valu;//用户余额记录ID
                        var manPrice = dic[valu];//支付金额
                        if (manPrice > 0)
                        {
                            conditon.PayType = (int)PaymentEnum.储值卡;
                            if (manPrice >= info.AllPrice)//如果储值卡部分超出实际产品价格
                            {
                                if (PayCash + PayCarded > 0)//支付了部分现金和银联
                                {
                                    conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                    dic.Remove(manCardid);
                                    dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额

                                    //添加产品支付方式 g
                                    conditon.CardID = manCardid;
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//终止循环
                                }
                                else
                                {
                                    conditon.PayMoney = info.AllPrice;
                                    dic.Remove(manCardid);
                                    dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额
                                    conditon.CardID = manCardid;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    break;//跳出循环
                                }
                            }
                            else
                            { //储值卡部分不够
                                if (PayCash + PayCarded > 0)//支付了部分现金和银联
                                {
                                    if (PayCash + PayCarded + manPrice > info.AllPrice)
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                        // Chuzhika = Chuzhika - conditon.PayMoney;
                                        dic.Remove(manCardid);
                                        dic.Add(manCardid, manPrice - conditon.PayMoney);//储值卡剩余金额
                                        //添加产品支付方式
                                        conditon.CardID = manCardid;
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        break;//跳出循环
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

                #region 其他支付方式
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
                            if (nowprice > info.AllPrice)//如果银联卡部分超出实际产品价格
                            {
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联和储值卡
                                {
                                    conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                    nowprice = nowprice - conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue;//跳出循环
                                }
                                else
                                {
                                    conditon.PayMoney = info.AllPrice;
                                    nowprice = nowprice - conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue;//跳出循环
                                }
                            }
                            else
                            { //银联卡部分不够
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed + nowprice > info.AllPrice)
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                        nowprice = nowprice - conditon.PayMoney;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;//跳出循环
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
                    #endregion
                }
                #endregion
            }

            return 1;
        }

        /// <summary>
        /// 添加订单支付方式
        /// </summary>
        /// <param name="_orderNo">订单编号</param>
        /// <param name="_PayType">支付方式</param>
        /// <param name="PayMoney">支付金额</param>
        /// <param name="BalanceMoney">余额</param>
        /// <param name="BalanceTiems">余次</param>
        /// <param name="CardID">卡ID</param>
        /// <param name="PayName">支付名称</param>
        /// <returns></returns>
        public int AddPaymentOrder(string _orderNo, int _PayType, decimal _PayMoney, decimal _BalanceMoney, int _BalanceTiems, int _CardID, string _PayName)
        {
            PaymentOrderEntity poe = new PaymentOrderEntity();
            poe.OrderNo = _orderNo;
            poe.PayType = _PayType;
            poe.PayMoney = _PayMoney;
            poe.BalanceMoney = _BalanceMoney;
            poe.BalanceTiems = _BalanceTiems;
            poe.CardID = _CardID;
            poe.PayName = _PayName;
            var result = cashBll.AddPaymentOrder(poe);
            return (int)result;
        }


        /// <summary>
        /// 添加订单详情支付方式
        /// </summary>
        /// <param name="_AllPrice">总共金额</param>
        /// <param name="_OrderInfoID">订单详情编号</param>
        /// <param name="_PayType">支付方式</param>
        /// <param name="_PayMoney">支付金额</param>
        /// <returns></returns>
        public int AddPaymentOrderProduct(decimal _AllPrice, int _OrderInfoID, int _PayType, decimal _PayMoney)
        {
            PaymentOrderProductEntity entity = new PaymentOrderProductEntity();
            entity.AllPrice = _AllPrice;
            entity.OrderInfoID = _OrderInfoID;
            entity.PayType = _PayType;
            entity.PayMoney = _PayMoney;
            var result = cashBll.AddPaymentOrderProduct(entity);
            return (int)result;
        }

        /// <summary>
        /// 添加储值卡记录
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="discountlist"></param>
        /// <param name="ismaika"></param>
        /// <param name="projectlisr"></param>
        /// <returns></returns>
        public int AddBlance(OrderEntity entity, string discountlist, string ismaika, string projectlisr)
        {
            #region ==添加储值记录和储值关系==
            entity = cashBll.GetOrderModel(entity);//获取订单详细
            OrderinfoEntity oi = new OrderinfoEntity();
            oi.OrderNo = entity.OrderNo;
            var orderinfolist = cashBll.GetOrderinfoList(oi);
            int o = 0;
            foreach (var orderinfo in orderinfolist)
            {
                #region ==获取尾款==
                decimal weikuan = 0;
                //如果存在欠款，修改用户欠款情况
                if (entity.QianPrice != 0)
                {
                    //如果是买卡欠钱,先获取当前订单详情的支付信息,再用总价减去所有的支付金额,剩下的就是欠款.
                    //判断是否是买卡,直接判断oldprice的价格是否为0,不为0则为买卡
                    if (ismaika == "1" && orderinfo.OldPrice != 0)
                    {
                        var zhifulist = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = orderinfo.ID });//获取支付详细列表
                        decimal zhifuprice = 0;
                        foreach (var zhifu in zhifulist)     //循环获得一共支付金额
                        {
                            zhifuprice += zhifu.PayMoney;
                        }
                        weikuan = orderinfo.AllPrice - zhifuprice;//订单详情的总价减去支付总价就是尾款价格
                    }
                }
                #endregion

                #region //逐条添加购买关系
                UserCardEntity usercard = new UserCardEntity();
                usercard.CardID = orderinfo.CardID;
                usercard.CreateTime = DateTime.Now;
                usercard.IsActive = 1;
                usercard.UserID = entity.UserID;
                //  usercard.Price = GetAllPrice(orderinfo.CardID, orderinfo.Price);//第一个参数是卡的ID,第二个是卡的原有价格
                usercard.Price = orderinfo.AllPrice;//第一个参数是卡的ID,第二个是卡的原有价格
                long usercardid = cashBll.AddUserCard(usercard);
                #endregion


                #region //添加用户余额记录
                //如果详情里面的IsChongzhi字段为1,则是充值单,则直接加
                if (orderinfo.IsChongzhi == 1)
                {
                    var mcb = new MainCardBalanceEntity { ID = orderinfo.ProdcuitID, Price = orderinfo.Price };
                    cashBll.RechargeMoney(mcb);

                    //用户储值卡余额明细日志
                    var mainCardBalance = cashBll.GetMainCardBalanceModel(mcb);
                    cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                    {
                        CardBalanceId = mcb.ID,
                        OrderNo = orderinfo.OrderNo,
                        Type = (int)MainCardBalanceDetailType.充值,
                        Amount = mcb.Price,
                        Balance = mainCardBalance.Price + mcb.Price,
                        OldAmount = mainCardBalance.Price,
                        CreateTime = DateTime.Now
                    });
                }

                IList<ProjectProductEntity> pplist = new List<ProjectProductEntity>();

                // string[] projectarray = projectlisr.Split(new Char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
                string[] projectarray = projectlisr.Split('^');
                if (projectarray.Length >= o && !string.IsNullOrEmpty(projectlisr))   //如果当前数据集合大于现在循环的次数,则表示产品关系是前台传入
                {     //数据格式2|13|110.76|1440|180.00,1|5|218.00|1090|218.00
                    string thelist = projectarray[o];
                    if (!string.IsNullOrEmpty(thelist))
                    {
                        if (thelist == "0")//如果是默认的0,则表示读取默认数据
                        {
                            pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = orderinfo.CardID });//获取所有的产品对应关系
                        }
                        else
                        {
                            string[] infoarray = thelist.Split(',');
                            foreach (var info in infoarray)
                            {
                                string[] ia = info.Split('|');
                                ProjectProductEntity ppe = new ProjectProductEntity();
                                ppe.AllPrice = Convert.ToDecimal(ia[3]);
                                ppe.ConsumptionPrice = Convert.ToDecimal(ia[2]);
                                ppe.ProjectID = Convert.ToInt32(ia[0]);
                                ppe.Price = Convert.ToDecimal(ia[4]);
                                ppe.ProgrammeID = orderinfo.ProdcuitID;//父卡ID是当前订单详情的ID
                                ppe.Qualtity = Convert.ToInt32(ia[1]);
                                ppe.Type = 2;    //这里默认是项目
                                ppe.RealityPrice = Convert.ToDecimal(ia[3]) / Convert.ToInt32(ia[1]);
                                pplist.Add(ppe);
                            }
                        }
                    }
                }
                else
                {
                    pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = orderinfo.CardID });//获取所有的产品对应关系
                }
                MainCardBalanceEntity cardbalance = new MainCardBalanceEntity();
                cardbalance.CardID = orderinfo.CardID;
                cardbalance.UserCardID = Convert.ToInt32(usercardid);
                cardbalance.UserID = entity.UserID;
                cardbalance.Type = 1;
                IList<ProjectProductEntity> pp = new List<ProjectProductEntity>();
                long cbalance = 0;
                ////下面添加的是赠卡
                foreach (ProjectProductEntity ppentity in pplist)
                {
                    if (ppentity.Type != 3)
                    {
                        cardbalance.AllPrice = ppentity.AllPrice;//初始化的时候就是这张卡的默认金额

                        cardbalance.AllTiems = ppentity.Qualtity;
                        cardbalance.KouTimes = 0;
                        cardbalance.Price = ppentity.RealityPrice == 0 ? ppentity.Price : ppentity.RealityPrice;//这里如果是从数据库读取出来的,实际价格就是单价,如果是写入进来的,则不是.
                        if (cardbalance.AllPrice == 0) { cardbalance.Price = 0; }//如果总金额为0,则,单价也为0;//用于处理赠送项目的处理
                        cardbalance.ProjectID = ppentity.ProjectID;
                        cardbalance.Tiems = ppentity.Qualtity;
                        if (ppentity.Type == 1)
                        {
                            cardbalance.AllTiems = 0;
                            cardbalance.Tiems = 0;
                        }
                        cardbalance.Type = ppentity.Type;
                        cardbalance.UserID = entity.UserID;
                        cardbalance.ConsumePrice = ppentity.ConsumptionPrice;
                        cardbalance.ExpireTime = GetExpireTime(orderinfo.CardID);
                        cbalance = cashBll.AddMainCardBalance(cardbalance);
                        var newinfo = cashBll.GetOrderinfoModel(orderinfo.ID);
                        newinfo.CardBalanceID = (newinfo.CardBalanceID == "0" ? cbalance.ToString() : newinfo.CardBalanceID + "," + cbalance.ToString());
                        cashBll.UpdateInfoCardID(newinfo);//把用户约记录的ID绑定到当前订单详情
                    }
                }
                //储值卡时需要把它本身也加入进去
                MainCardEntity maincard = baseinfoBLL.GetPrePayCardModel(orderinfo.CardID);
                maincard.Price = orderinfo.AllPrice;
                if (maincard.Type == 1)//
                {
                    cardbalance.AllPrice = maincard.Price;
                    cardbalance.AllTiems = 0;
                    cardbalance.ProjectID = maincard.ID;//储值卡,项目,产品ID存放在这个字段
                    cardbalance.CardID = Convert.ToInt32(usercardid);  //这个字段接受上面返回的关系ID
                    cardbalance.CostPrice = maincard.Price;
                    cardbalance.KouTimes = 0;
                    //cardbalance.UserCardID = Convert.ToInt32(usercardid);
                    cardbalance.Name = (maincard.Type == 1 ? "储值卡主卡" : (maincard.Type == 2 ? "疗程卡项目" : "方案卡详情"));
                    cardbalance.Price = maincard.Price;
                    cardbalance.Tiems = 0;
                    cardbalance.Weikuan = weikuan;//把尾款放入到用户记录
                    cardbalance.Type = maincard.Type;
                    cardbalance.UserID = entity.UserID;
                    cardbalance.ExpireTime = GetExpireTime(orderinfo.CardID);
                    if (maincard.Type == 1)
                    {
                        cardbalance.HuaKouZheLv = Convert.ToDecimal(discountlist.Split(',')[o]);
                    }

                    cbalance = cashBll.AddMainCardBalance(cardbalance);
                    //因为现在对应ID是文字串,需要重新编辑,所以每次加载都要重新载入最新的
                    var newinfo1 = cashBll.GetOrderinfoModel(orderinfo.ID);
                    newinfo1.CardBalanceID = (newinfo1.CardBalanceID == "0" ? cbalance.ToString() : newinfo1.CardBalanceID + "," + cbalance.ToString());
                    cashBll.UpdateInfoCardID(newinfo1);//把用户约记录的ID绑定到当前订单详情

                }


                #endregion

                #region ==添加员工业绩==
                // var yeji = new JoinUserEntity();
                // yeji.ProjectID = orderinfo.ProdcuitID;
                // yeji.Type = 3;//卡类操作
                // yeji.IsZhiding = 0;//默认不指定服务
                // yeji.OrderInfoID = orderinfo.ID;
                // yeji.OrderNo = entity.OrderNo;
                // yeji.XiaoHao = 0;//买卡没有消耗
                //// yeji.Yeji = orderinfo.AllPrice;  //业绩是按照订单收到的价格
                // cashBll.AddJoinUser(yeji);
                #endregion

                #region ==添加积分==
                var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate");  //获取积分实体
                decimal jifen = 0;
                if (usersetting.AttributeValue != null && usersetting.AttributeValue != "")
                {
                    jifen = Convert.ToDecimal(entity.Price) / (usersetting.AttributeValue == null ? 1000000 : Convert.ToDecimal(usersetting.AttributeValue));//判断能获取多少积分
                }

                IntegralRecordEntity ir = new IntegralRecordEntity();
                ir.UserID = entity.UserID;
                ir.OrderNO = entity.DocumentNumber;
                ir.Integral = jifen;
                ir.CreateTime = DateTime.Now;
                IpatBLL.AddIntegralRecord(ir);  //添加用户积分记录

                var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                intrgral.AllIntegral = intrgral.AllIntegral + jifen;
                intrgral.Integral = intrgral.Integral + jifen;

                IpatBLL.UpdateIntegral(intrgral);
                #endregion
                o++;
            }
            #endregion
            return 1;
        }

        public int AddJoinUser(IList<OrderinfoEntity> orderInfoList)
        {
            #region==重新计算员工业绩==
            //重新计算员工业绩
            foreach (var info in orderInfoList)
            {
                int row, pagecount;
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
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

                    //修改业绩
                    cond.KakouYeji = ConvertValueHelper.ConvertDecimalValue((paymoney / joinList.Count).ToString("f2"));//卡扣业绩

                    cond.Yeji = ConvertValueHelper.ConvertDecimalValue((paymoney1 / joinList.Count).ToString("f2"));//现金业绩
                    if (info.Type == 1)
                    {
                        cond.KakouYeji_Xiangmu = cond.KakouYeji;
                        cond.Yeji_Xiangmu = cond.Yeji;
                    }
                    else if (info.Type == 2)
                    {
                        cond.Yeji_Chanpin = cond.Yeji;
                        cond.KakouYeji_Chanpin = cond.KakouYeji;
                    }
                    else if (info.Type == 3)
                    {
                        if (info.IsChongzhi == 1)//如果是充值单据,直接记为储值卡业绩
                        {
                            cond.Yeji_Chuzhika = cond.Yeji;
                        }
                        else
                        {
                            var model = baseinfoBLL.GetPrePayCardModel(info.ProdcuitID);
                            if (model.Type == 1) { cond.Yeji_Chuzhika = cond.Yeji; }
                            if (model.Type == 2) { cond.Yeji_Liaochengka = cond.Yeji; cond.KakouYeji_Liaocheng = cond.KakouYeji; }
                        }
                        if (info.InfoBuyType == 1 || info.InfoBuyType == 2)
                        {
                            cond.KakouYeji = 0;
                            cond.KakouYeji_Chanpin = 0;
                            cond.KakouYeji_Huankuan = 0;
                            cond.KakouYeji_Liaocheng = 0;
                            cond.KakouYeji_Xiangmu = 0;
                        }
                    }


                    //购买疗程卡没有消耗
                    cond.XiaoHao = 0;
                    cond.XiaoHao_kakou = 0;
                    cond.XiaoHao_Liaocheng = 0;

                    cashBll.UpdateJoinUserYejiForPay(cond);
                }
            }
            #endregion
            return 1;
        }



        #endregion


        /// <summary>
        /// 通过卡类ID获取卡类信息
        /// 然后通过卡类信息获取次卡类的有效期
        /// 然后加入有效期时间,返回,就是这张卡的到期时间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DateTime GetExpireTime(int id)
        {
            var chuzhi = baseinfoBLL.GetPrePayCardModel(id);  //更具储值卡ID获取储值卡信息.
            DateTime et = DateTime.Now;
            if (chuzhi.VaildityDay > 3600) chuzhi.VaildityDay = 3600;
             et = et.AddDays(chuzhi.VaildityDay);
            return et;
        }


        /// <summary>
        /// 获取顾客购买疗程卡
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetprojectCardlist(int UserID)
        {
            int rows = 0, pagecount = 0;
            // var list = cashBll.GetAllUserCard(new UserCardEntity { UserID = UserID, Type = 2, IsActive = 1 }, out rows, out pagecount);
            var list = cashBll.GetBlanceinproject(UserID, LoginUserEntity.ParentHospitalID).Where(x => x.ExpireTime > DateTime.Now); //赠送的储值卡为零或过期 不显示
            list = list.Where(t=>t.IsDel!=3);
            StringBuilder str = new StringBuilder();

            foreach (var info in list)
            {
                //int alltimes = cashBll.GetTiems(info.ID);
                str.Append("<tr class='cashpricetr'>");
                //str.AppendFormat("<td><input type='checkbox' name='checkboxorder2' value='{0}' code='{1}' /></td>", info.ID, info.CardID);
                str.AppendFormat("<td>{0}</td>", info.ProjectName);
                str.AppendFormat("<td>{0}</td>", info.Tiems);
                str.Append("</tr>");
            }
            return Content(str.ToString());
        }

        /// <summary>
        /// 获取顾客购买方案卡
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetFanganCardlist(int UserID)
        {
            int rows = 0, pagecount = 0;
            var list = cashBll.GetAllUserCard(new UserCardEntity { UserID = UserID, Type = 3, IsActive = 1 }, out rows, out pagecount);
            IList<ProjectProductEntity> newlist = new List<ProjectProductEntity>();
            foreach (var info in list)
            {
                var pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = info.CardID });
                foreach (var ppinfo in pplist)
                {
                    newlist.Add(ppinfo);
                }
            }

            StringBuilder str = new StringBuilder();

            foreach (var info in newlist)
            {
                str.Append("<tr class='cashpricetr'>");
                //str.AppendFormat("<td><input type='checkbox' name='checkboxorder2' value='{0}' code='{1}' /></td>", info.ID, info.CardID);
                str.AppendFormat("<td>{0}</td>", info.ProjectName);
                str.AppendFormat("<td>{0}</td>", info.Price);
                str.Append("</tr>");
            }
            return Content(str.ToString());
        }


        /// <summary>
        /// 获取顾客购买方案卡
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public ActionResult getProjectProductNopage(int cardId)
        {
            var pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = cardId });
            var str = new StringBuilder();
            var newstr = "";
            foreach (var projprod in pplist)
            {
                var projectEntity = baseinfoBLL.GetProjectModel(projprod.ProjectID);

                newstr += projprod.ProjectID + "|" + projprod.Qualtity + "|" + projprod.ConsumptionPrice + "|" + projprod.AllPrice + "|" + projprod.Price + "|" + projectEntity.ISKit + "|" + projprod.AutQualtity + ",";
                //newstr += projprod.ProjectID + "|" + projprod.Qualtity + "|" + projprod.ConsumptionPrice + "|" + projprod.AllPrice + "|" + projprod.Price + "|" + projectEntity.ISKit +  ",";
            }

            if (!string.IsNullOrWhiteSpace(newstr))
                newstr = newstr.Substring(0, newstr.Length - 1);

            return Content(newstr.ToString());
        }
        public ActionResult getProjectProductNopage9(int cardId)
        {
            var pplist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = cardId });
            var str = new StringBuilder();
            var newstr = "";
            foreach (var projprod in pplist)
            {
                var projectEntity = baseinfoBLL.GetProjectModel(projprod.ProjectID);

                newstr += projprod.ProjectID + "|" + projprod.Qualtity + "|" + projprod.ConsumptionPrice + "|" + projprod.AllPrice + "|" + projprod.Price + "|" + projectEntity.ISKit + "|" + projprod.AutQualtity + ",";
            }

            if (!string.IsNullOrWhiteSpace(newstr))
                newstr = newstr.Substring(0, newstr.Length - 1);

            return Content(newstr.ToString());
        }
        #endregion

        #region ==充值储值卡==

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
            list = list.Where(t => t.Price > 0).ToList();

            StringBuilder str = new StringBuilder();

            foreach (var info in list)
            {
                if (info.Name != "积分消费")
                {
                    if (info.IsGive == 1 && info.Price == 0)//如果是赠卡切没有钱了就不显示
                    {
                        continue;
                    }

                    str.Append("<tr class='cashpricetr'>");
                    if (info.IsGive == 1)//如果是赠卡,不给充值功能  20161104突然要开启这个以前被关闭的功能,替换这一行完成可以读取数据.
                    {
                        str.AppendFormat("<td><input type='checkbox' name='checkboxorder2' class='thebox' isgive='1' value='{0}' code='{1}' />", info.ID, info.CardID);
                        //str.AppendFormat("<td>");
                    }
                    else
                    {
                        str.AppendFormat("<td><input type='checkbox' name='checkboxorder2' class='thebox' isgive='0' value='{0}' code='{1}' />", info.ID, info.CardID);
                    }
                    // str.AppendFormat("<td><input type='checkbox' name='checkboxorder2' class='thebox' value='{0}' code='{1}' isgive='{2}' />", info.ID, info.CardID,info.IsGive);

                    str.AppendFormat("<input type='hidden' class='val' value='{0}' /> </td>", info.ID + "|" + info.Price + "|" + info.ProjectID + "|" + info.Name);
                    str.AppendFormat("<td>{0}</td>", info.Name);
                    str.AppendFormat("<td>{0}</td>", info.Price);

                    str.Append("</tr>");
                }
            }
            return Content(str.ToString());
        }


        public ActionResult ToGetStoredCardListSUM(int id)
        {
            MainCardBalanceEntity entity = new MainCardBalanceEntity();
            entity.UserID = Convert.ToInt32(id);
            entity.Type = 1;
            IList<MainCardBalanceEntity> list = new List<MainCardBalanceEntity>();
            list = cashBll.GetMainCardBalanceList(entity);
            list = list.Where(t => t.IsDel != 3&&t.Name!= "积分消费").ToList();
            
            var sum = list.Sum(i => i.Price);
            return Json(sum);
        }
        public ActionResult GetIntegralSUM(int id)
        {
            var sum = IpatBLL.GetIntegralModel(id);
            //MainCardBalanceEntity entity = new MainCardBalanceEntity();
            //entity.UserID = Convert.ToInt32(id);
            //entity.Type = 1;
            //IList<MainCardBalanceEntity> list = new List<MainCardBalanceEntity>();
            //list = cashBll.GetMainCardBalanceList(entity);
            //var sum = list.Sum(i => i.Price);
            return Json(sum.Integral);
        }
        //充值支付页面
        public ActionResult PayOrderPageRecharge()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <returns></returns>
        public ActionResult PayRechargeOrder(OrderEntity entity, string UserIDList, string Memo, string shouxufei, string TimesList, string txtChuzhikaarr, string IDList, string HuankuanList, string OtherPayment, string IsCashValue, string OtherPaymentValue, string CardIDList)
        {
            entity.CreateTime = DateTime.Now;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
            entity.OrderType = (int)OrderType.充值;
            entity.Statu = (int)OrderStatu.已结算;
            entity.ActurePrice = entity.Price;
            entity.Name = Memo;
            var mendian = personnelBLL.HospitalEntityByID(LoginUserEntity.HospitalID);
            string time = DateTime.Now.ToString("MMdd");
            var count = cashBll.GetTodayOrderCount(LoginUserEntity.HospitalID, DateTime.Now);//获取总数
            entity.DocumentNumber = mendian.Prefix + time + (count + 1).ToString("d3");

            //添加订单
            string result = cashBll.AddOrder(entity);

            string[] HuankuanList1 = HuankuanList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] IDList1 = IDList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] TimesList1 = TimesList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] CardIDList1 = CardIDList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] canyuUserID = UserIDList.Split(new Char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            if (result != null)
            {
                OrderinfoEntity orderinfo = new OrderinfoEntity();
                orderinfo.OrderNo = entity.OrderNo;

                for (int i = 0; i < IDList1.Length; i++)
                {
                    int results = 0;
                    //充值
                    var mcb = new MainCardBalanceEntity
                    {
                        ID = ConvertValueHelper.ConvertIntValue(IDList1[i]),
                        Price = ConvertValueHelper.ConvertDecimalValue(HuankuanList1[i])
                    };
                    cashBll.RechargeMoney(mcb);

                    //用户储值卡余额明细日志
                    var mainCardBalance = cashBll.GetMainCardBalanceModel(mcb);
                    cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                    {
                        CardBalanceId = mcb.ID,
                        OrderNo = orderinfo.OrderNo,
                        Type = (int)MainCardBalanceDetailType.充值,
                        Amount = mcb.Price,
                        Balance = mainCardBalance.Price + mcb.Price,
                        OldAmount = mainCardBalance.Price,
                        CreateTime = DateTime.Now
                    });

                    //添加详情信息表
                    orderinfo.Type = 3;
                    orderinfo.ProdcuitID = ConvertValueHelper.ConvertIntValue(CardIDList1[i]);
                    orderinfo.OldPrice = ConvertValueHelper.ConvertDecimalValue(HuankuanList1[i]);
                    orderinfo.Price = ConvertValueHelper.ConvertDecimalValue(HuankuanList1[i]);
                    orderinfo.AllPrice = ConvertValueHelper.ConvertDecimalValue(HuankuanList1[i]);
                    orderinfo.Quantity = 1;
                    results = cashBll.AddOrderinfo(orderinfo);

                    //获取参与员工
                    if (results > 0)
                    {
                        string[] nowUserID = canyuUserID[i].Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        JoinUserEntity model = new JoinUserEntity();
                        for (int j = 0; j < nowUserID.Length; j++)
                        {
                            model.UserID = ConvertValueHelper.ConvertIntValue(nowUserID[j]);
                            model.OrderNo = entity.OrderNo;
                            model.OrderInfoID = results;
                            //业绩,平均分配
                            model.Yeji = ConvertValueHelper.ConvertDecimalValue((orderinfo.Price / nowUserID.Length).ToString("f2"));
                            cashBll.AddJoinUser(model);
                        }
                    }
                }
                //如果存在手续费，则添加一条记录
                if (!string.IsNullOrEmpty(shouxufei))
                {
                    int results = 0;
                    orderinfo.Type = 2;
                    orderinfo.Quantity = 1;
                    orderinfo.ProdcuitName = "手续服务费";
                    orderinfo.OldPrice = Convert.ToDecimal(shouxufei);
                    orderinfo.Price = Convert.ToDecimal(shouxufei);
                    orderinfo.AllPrice = Convert.ToDecimal(shouxufei);
                    results = cashBll.AddOrderinfo(orderinfo);

                }
                //其他支付方式
                if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                {
                    string[] strpay = OtherPayment.Split(',');
                    string[] OtherPaymentValuelist = OtherPaymentValue.Split(',');
                    for (int i = 0; i < strpay.Length; i++)
                    {
                        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = ConvertValueHelper.ConvertIntValue(strpay[i]), PayMoney = ConvertValueHelper.ConvertIntValue(OtherPaymentValuelist[i]) });
                    }
                }
                if (entity.Xianjin > 0)//现金
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.现金, PayMoney = entity.Xianjin });
                }
                if (entity.Yinlianka > 0)//银联卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.银联卡, PayMoney = entity.Yinlianka });
                }
                if (entity.Chuzhika > 0)//储值卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.储值卡, PayMoney = entity.Chuzhika });

                }

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
            foreach (var info in orderInfoList)
            {
                if (info.ProdcuitName == "手续服务费")
                {
                    continue;
                }
                //新的产品或者项目
                PayCash = 0;
                PayCarded = 0;
                ChuzhikaPayed = 0;
                PaymentOrderProductEntity conditon = new PaymentOrderProductEntity();
                conditon.AllPrice = info.AllPrice;//订单产品总价
                conditon.OrderInfoID = info.ID;//支付订单产品
                #region==产品支付==

                if (ShengyuCash > 0)//现金
                {
                    conditon.PayType = (int)PaymentEnum.现金;
                    if (ShengyuCash >= info.AllPrice)//如果现金部分超出实际产品价格
                    {
                        conditon.PayMoney = info.AllPrice;
                        ShengyuCash = ShengyuCash - conditon.PayMoney;
                        PayCash = conditon.PayMoney;
                        //添加产品支付方式
                        cashBll.AddPaymentOrderProduct(conditon);
                        continue;//跳出循环
                    }
                    else
                    { //现金部分不够
                        conditon.PayMoney = ShengyuCash;
                        PayCash = ShengyuCash;
                        ShengyuCash = 0;
                        //添加产品支付方式
                        cashBll.AddPaymentOrderProduct(conditon);
                    }
                }//现金支付
                if (YinLianCard > 0)//银联卡
                {
                    conditon.PayType = (int)PaymentEnum.银联卡;
                    if (YinLianCard >= info.AllPrice)//如果银联卡部分超出实际产品价格
                    {
                        if (PayCash > 0)//支付了部分现金
                        {
                            conditon.PayMoney = info.AllPrice - PayCash;
                            YinLianCard = YinLianCard - conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                        else
                        {
                            conditon.PayMoney = info.AllPrice;
                            YinLianCard = YinLianCard - conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                    }
                    else
                    { //银联卡部分不够
                        if (PayCash > 0)//支付了部分现金
                        {
                            if (PayCash + YinLianCard >= info.AllPrice)
                            {
                                conditon.PayMoney = info.AllPrice - PayCash;
                                YinLianCard = YinLianCard - conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue;//跳出循环
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

                }//银联卡支付

                //储值卡支付
                if (Chuzhika > 0)
                {
                    conditon.PayType = (int)PaymentEnum.储值卡;

                    if (Chuzhika >= info.AllPrice)//如果银联卡部分超出实际产品价格
                    {
                        if (PayCash + PayCarded > 0)//支付了部分现金和银联
                        {
                            conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                            Chuzhika = Chuzhika - conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                        else
                        {
                            conditon.PayMoney = info.AllPrice;
                            Chuzhika = Chuzhika - conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                    }
                    else
                    { //银联卡部分不够
                        if (PayCash + PayCarded > 0)//支付了部分现金和银联
                        {
                            if (PayCash + PayCarded + Chuzhika >= info.AllPrice)
                            {
                                conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                Chuzhika = Chuzhika - conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue;//跳出循环
                            }
                            else
                            {
                                conditon.PayMoney = Chuzhika + PayCash + PayCarded;
                                Chuzhika = 0;
                                ChuzhikaPayed = conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                            }

                        }
                        else
                        {
                            conditon.PayMoney = YinLianCard;
                            Chuzhika = 0;
                            ChuzhikaPayed = conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);

                        }
                    }
                }
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
                            if (nowprice > info.AllPrice)//如果银联卡部分超出实际产品价格
                            {
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联和储值卡
                                {
                                    conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                    nowprice = nowprice - conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue;//跳出循环
                                }
                                else
                                {
                                    conditon.PayMoney = info.AllPrice;
                                    nowprice = nowprice - conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue;//跳出循环
                                }
                            }
                            else
                            { //银联卡部分不够
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed + nowprice > info.AllPrice)
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                        nowprice = nowprice - conditon.PayMoney;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;//跳出循环
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

            }


            return Json(result);
        }

        #endregion

        #region ==项目充值==
        /// <summary>
        /// 获取项目列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ToGetProjectList(int id)
        {
            var list = cashBll.GetProjectTimes(id, LoginUserEntity.ParentHospitalID);
            var persoList = personnelBLL.GetAllUserByHospitalID(LoginUserEntity.HospitalID, 0);
            StringBuilder str = new StringBuilder();
            //参与员工
            StringBuilder persoListstr = new StringBuilder();
            persoListstr.AppendFormat("<ul style='position:relative;'>");
            persoListstr.AppendFormat("<li class='liMenu' ><a href='javascript:;'> 点击选择</a></li>");
            persoListstr.AppendFormat("<ul class='liHide ulClass'>");

            foreach (var info in persoList)
            {
                persoListstr.AppendFormat("<li class='liClass' ><input type='checkbox' id='{0}' value='{1}' name='UserID' /><label for='{0}'>{2}</label> </li>", "chk1(" + info.UserID + ")", info.UserID, info.UserName);
            }
            persoListstr.AppendFormat("</ul>");
            persoListstr.AppendFormat("</ul>");
            foreach (var info in list)
            {
                str.Append("<tr class='cashpricetr'>");
                str.AppendFormat("<td><input type='checkbox' name='checkboxorder' value='{0}' /><input type='hidden' name='CardIDList' value='{1}' </td>", info.ID, info.ProjectID);
                str.AppendFormat("<td>{0}</td>", info.Name);
                str.AppendFormat("<td>{0}</td>", info.ProjectName);
                str.AppendFormat("<td>{0}</td>", info.CostPrice);
                str.AppendFormat("<td class='youhuiprice'>{0}</td>", info.RetailPrice);
                str.AppendFormat("<td>{0}</td>", info.AllTiems);
                str.AppendFormat("<td class='shengyuTimes'>{0}</td>", info.Tiems);
                str.AppendFormat("<td><input style='width:60px;' name='txttuitiems' onkeyup='javascript:CheckInputIntFloat(this);' class='tuitiems' type='text' /></td>");
                str.AppendFormat("<td><input style='width:60px;' name='txthuankuanp' onkeyup='javascript:CheckInputIntFloat(this);' class='cashprice' type='text' /></td>");
                str.AppendFormat("<td>{0}</td>", persoListstr.ToString());
                str.Append("</tr>");
            }
            return Content(str.ToString());
        }

        /// <summary>
        /// 项目充值页面
        /// </summary>
        /// <returns></returns>
        public ActionResult PayOrderRechargeproject()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        public ActionResult PayRechargeproject(OrderEntity entity, string UserIDList, string Memo, string shouxufei, string TimesList, string txtChuzhikaarr, string IDList, string HuankuanList, string OtherPayment, string IsCashValue, string OtherPaymentValue)
        {
            int results = 0;
            entity.CreateTime = DateTime.Now;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
            entity.OrderType = (int)OrderType.充值;
            entity.Statu = (int)OrderStatu.已结算;
            entity.ActurePrice = entity.Price;
            entity.Name = Memo;
            var mendian = personnelBLL.HospitalEntityByID(LoginUserEntity.HospitalID);
            string time = DateTime.Now.ToString("MMdd");
            var count = cashBll.GetTodayOrderCount(LoginUserEntity.HospitalID, DateTime.Now);//获取总数
            entity.DocumentNumber = mendian.Prefix + time + (count + 1).ToString("d3");

            //添加订单
            string result = cashBll.AddOrder(entity);

            string[] HuankuanList1 = HuankuanList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] IDList1 = IDList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] TimesList1 = TimesList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            string[] canyuUserID = UserIDList.Split(new Char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            if (result != null)
            {
                OrderinfoEntity orderinfo = new OrderinfoEntity();
                orderinfo.OrderNo = entity.OrderNo;

                for (int i = 0; i < IDList1.Length; i++)
                {

                    //充值
                    cashBll.Rechargeproject(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(IDList1[i]), Tiems = ConvertValueHelper.ConvertIntValue(TimesList1[i]) });
                    //添加详情信息表
                    orderinfo.Type = 2;
                    orderinfo.OldPrice = ConvertValueHelper.ConvertDecimalValue(HuankuanList1[i]);
                    orderinfo.Price = ConvertValueHelper.ConvertDecimalValue(HuankuanList1[i]);
                    orderinfo.AllPrice = ConvertValueHelper.ConvertDecimalValue(HuankuanList1[i]);
                    orderinfo.Quantity = 1;
                    results = cashBll.AddOrderinfo(orderinfo);

                    //获取参与员工
                    if (results > 0)
                    {
                        string[] nowUserID = canyuUserID[i].Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        JoinUserEntity model = new JoinUserEntity();
                        for (int j = 0; j < nowUserID.Length; j++)
                        {
                            model.UserID = ConvertValueHelper.ConvertIntValue(nowUserID[j]);
                            model.OrderNo = entity.OrderNo;
                            model.OrderInfoID = results;
                            //业绩,平均分配
                            model.Yeji = ConvertValueHelper.ConvertDecimalValue((orderinfo.Price / nowUserID.Length).ToString("f2"));
                            cashBll.AddJoinUser(model);
                        }
                    }
                }
                //如果存在手续费，则添加一条记录
                if (!string.IsNullOrEmpty(shouxufei))
                {
                    orderinfo.Type = 2;
                    orderinfo.Quantity = 1;
                    orderinfo.ProdcuitName = "手续服务费";
                    orderinfo.OldPrice = Convert.ToDecimal(shouxufei);
                    orderinfo.Price = Convert.ToDecimal(shouxufei);
                    orderinfo.AllPrice = Convert.ToDecimal(shouxufei);
                    results = cashBll.AddOrderinfo(orderinfo);

                }
                //其他支付方式
                if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                {
                    string[] strpay = OtherPayment.Split(',');
                    string[] otherPaymentValuelist = OtherPaymentValue.Split(',');
                    for (int i = 0; i < strpay.Length; i++)
                    {
                        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = ConvertValueHelper.ConvertIntValue(strpay[i]), PayMoney = ConvertValueHelper.ConvertIntValue(otherPaymentValuelist[i]) });
                    }
                }
                if (entity.Xianjin > 0)//现金
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.现金, PayMoney = entity.Xianjin });
                }
                if (entity.Yinlianka > 0)//银联卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.银联卡, PayMoney = entity.Yinlianka });
                }
                if (entity.Chuzhika > 0)//储值卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.储值卡, PayMoney = entity.Chuzhika });

                }

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
            foreach (var info in orderInfoList)
            {
                if (info.ProdcuitName == "手续服务费")
                {
                    continue;
                }
                //新的产品或者项目
                PayCash = 0;
                PayCarded = 0;
                ChuzhikaPayed = 0;
                PaymentOrderProductEntity conditon = new PaymentOrderProductEntity();
                conditon.AllPrice = info.AllPrice;//订单产品总价
                conditon.OrderInfoID = info.ID;//支付订单产品
                #region==产品支付==

                if (ShengyuCash > 0)//现金
                {
                    conditon.PayType = (int)PaymentEnum.现金;
                    if (ShengyuCash >= info.AllPrice)//如果现金部分超出实际产品价格
                    {
                        conditon.PayMoney = info.AllPrice;
                        ShengyuCash = ShengyuCash - conditon.PayMoney;
                        PayCash = conditon.PayMoney;
                        //添加产品支付方式
                        cashBll.AddPaymentOrderProduct(conditon);
                        continue;//跳出循环
                    }
                    else
                    { //现金部分不够
                        conditon.PayMoney = ShengyuCash;
                        PayCash = ShengyuCash;
                        ShengyuCash = 0;
                        //添加产品支付方式
                        cashBll.AddPaymentOrderProduct(conditon);
                    }
                }//现金支付
                if (YinLianCard > 0)//银联卡
                {
                    conditon.PayType = (int)PaymentEnum.银联卡;
                    if (YinLianCard >= info.AllPrice)//如果银联卡部分超出实际产品价格
                    {
                        if (PayCash > 0)//支付了部分现金
                        {
                            conditon.PayMoney = info.AllPrice - PayCash;
                            YinLianCard = YinLianCard - conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                        else
                        {
                            conditon.PayMoney = info.AllPrice;
                            YinLianCard = YinLianCard - conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                    }
                    else
                    { //银联卡部分不够
                        if (PayCash > 0)//支付了部分现金
                        {
                            if (PayCash + YinLianCard >= info.AllPrice)
                            {
                                conditon.PayMoney = info.AllPrice - PayCash;
                                YinLianCard = YinLianCard - conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue;//跳出循环
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

                }//银联卡支付

                //储值卡支付
                if (Chuzhika > 0)
                {
                    conditon.PayType = (int)PaymentEnum.储值卡;

                    if (Chuzhika >= info.AllPrice)//如果银联卡部分超出实际产品价格
                    {
                        if (PayCash + PayCarded > 0)//支付了部分现金和银联
                        {
                            conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                            Chuzhika = Chuzhika - conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                        else
                        {
                            conditon.PayMoney = info.AllPrice;
                            Chuzhika = Chuzhika - conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);
                            continue;//跳出循环
                        }
                    }
                    else
                    { //银联卡部分不够
                        if (PayCash + PayCarded > 0)//支付了部分现金和银联
                        {
                            if (PayCash + PayCarded + Chuzhika >= info.AllPrice)
                            {
                                conditon.PayMoney = info.AllPrice - PayCash - PayCarded;
                                Chuzhika = Chuzhika - conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                                continue;//跳出循环
                            }
                            else
                            {
                                conditon.PayMoney = Chuzhika + PayCash + PayCarded;
                                Chuzhika = 0;
                                ChuzhikaPayed = conditon.PayMoney;
                                //添加产品支付方式
                                cashBll.AddPaymentOrderProduct(conditon);
                            }

                        }
                        else
                        {
                            conditon.PayMoney = YinLianCard;
                            Chuzhika = 0;
                            ChuzhikaPayed = conditon.PayMoney;
                            //添加产品支付方式
                            cashBll.AddPaymentOrderProduct(conditon);

                        }
                    }
                }
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
                            if (nowprice > info.AllPrice)//如果银联卡部分超出实际产品价格
                            {
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联和储值卡
                                {
                                    conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                    nowprice = nowprice - conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue;//跳出循环
                                }
                                else
                                {
                                    conditon.PayMoney = info.AllPrice;
                                    nowprice = nowprice - conditon.PayMoney;
                                    //添加产品支付方式
                                    cashBll.AddPaymentOrderProduct(conditon);
                                    continue;//跳出循环
                                }
                            }
                            else
                            { //银联卡部分不够
                                if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed + nowprice > info.AllPrice)
                                    {
                                        conditon.PayMoney = info.AllPrice - PayCash - PayCarded - ChuzhikaPayed;
                                        nowprice = nowprice - conditon.PayMoney;
                                        //添加产品支付方式
                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;//跳出循环
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

            }
            return Json(result);
        }

        #endregion

    }
}
