using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using EYB.ServiceEntity.CashEntity;
using Common.Helper;
using HS.SupportComponents;
using EYB.ServiceEntity.BaseInfo;
using EYB.ServiceEntity.PatientEntity;
using System.Threading.Tasks;
using EYB.ServiceEntity.WarehouseEntity;
using System.Data.SqlClient;

namespace EYB.Web.Controllers
{
    /// <summary>
    /// 退换类
    /// </summary>
    public partial class CashManageController : BaseController
    {
        #region==退换操作==
        /// <summary>
        /// 退换页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Exchange()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }

        #region==退换项目==
        /// <summary>
        /// 结算页面
        /// </summary>
        /// <returns></returns>
        public ActionResult PayOrderExchange()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.Username = LoginUserEntity.UserName;
            var entity = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "IsOpenAutoPrint");
            ViewBag.IsOpenAutoPrint = entity.AttributeValue;
            return View();
        }

        /// <summary>
        /// 获取顾客项目次数及其明细
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetProjectTimes(int UserID)
        {
            var list = cashBll.GetProjectTimes(UserID, LoginUserEntity.ParentHospitalID);
            var persoList = personnelBLL.GetAllUserByHospitalID(LoginUserEntity.HospitalID, 0);
            StringBuilder str = new StringBuilder();
            //参与员工
            StringBuilder persoListstr = new StringBuilder();
            persoListstr.AppendFormat("<ul style='position:relative;'>");
            persoListstr.AppendFormat("<li class='liMenu' ><a href='javascript:;'> 点击选择</a></li>");
            persoListstr.AppendFormat("<ul class='liHide ulClass'>");

            foreach (var info in persoList)
            {
                persoListstr.AppendFormat("<li class='liClass' ><input code='{2}' type='checkbox' id='{0}' value='{1}' name='UserID' /><label for='{0}'>{2}</label> </li>", "[#chk1](" + info.UserID + ")", info.UserID, info.UserName);
            }

            var HospitalList = personnelBLL.GetHospitalListByParentID(LoginUserEntity.ParentHospitalID);//获取父门店
            persoListstr.Append("<li class='liClass' style='margin-left:-10px;' >");
            persoListstr.Append("<select name=\"HospitalID\" class=\"select selHospitalID\" style=\"width:100px;color:Blue;font-size:12px;\">");
            persoListstr.Append("<option value=\"0\">选择跨店员工</option>");
            foreach (var info in HospitalList)
            {
                persoListstr.AppendFormat("<option value=\"{0}\">{1}</option>", info.ID, info.Name);
            }

            persoListstr.Append("</select>");
            persoListstr.Append("</li>");

            persoListstr.AppendFormat("</ul>");
            persoListstr.AppendFormat("</ul>");






            foreach (var info in list)
            {
                str.Append("<tr class='cashpricetr'>");
                str.AppendFormat("<td><input type='checkbox' name='checkboxorder' value='{0}' code={1} /></td>", info.ID, info.ProjectID);
                str.AppendFormat("<td>{0}</td>", info.Name);
                str.AppendFormat("<td>{0}</td>", info.ProjectName);
                // str.AppendFormat("<td>{0}</td>", info.CostPrice);
                str.AppendFormat("<td class='youhuiprice'>{0}</td>", info.ConsumePrice);
                str.AppendFormat("<td>{0}</td>", info.AllTiems);
                str.AppendFormat("<td class='shengyuTimes'>{0}</td>", info.Tiems);
                str.AppendFormat("<td><input style='width:60px;' name='txttuitiems' onkeyup='javascript:CheckInputIntFloat(this);' class='tuitiems' type='text' /></td>");
                //str.AppendFormat("<td><lable   class='Tuikuanneedmoney'>0</lable></td>");
                str.AppendFormat("<td><input class='Tuikuanneedmoney' style='width:60px;'  disabled='disabled' type='text' /></td>");
                str.AppendFormat("<td><input style='width:60px;' name='txthuankuanp' onkeyup='javascript:CheckInputIntFloat(this);' class='cashprice' type='text' /></td>");
                str.AppendFormat("<td>{0}</td>", persoListstr.ToString().Replace("[#chk1]", "chk" + info.ID));
                str.Append("</tr>");
            }
            return Content(str.ToString());
        }

        /// <summary>
        /// 退换操作---------项目
        /// 1、添加订单 2、添加订单详情   2更改结算表支付金额 3生成订单 4支付结算
        /// </summary>
        /// <returns></returns>
        public ActionResult PayExchangeOrder(OrderEntity entity, string UserIDList, string Memo, int TuiType, string shouxufei, string TimesList, string txtChuzhikaarr, string IDList, string HuankuanList, string HuankuanList11, string OtherPayment, string IsCashValue, string IsGiveValue, string OtherPaymentValue, string CardIDList, string cardidarr, int chkcreatetime, string txtCreateTime, string hiddenToken)
        {
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
            var results = 0;

            //罗发明 补单时间时分秒换成实际的
            //var hms = DateTime.Now.ToString("hh:mm:ss").ToString();
            var hms = DateTime.Now.ToLongTimeString().ToString();
            var timess = "yyyy-MM-dd " + hms;

            //汪  7-26 补单及到店时间的修改
            PatientEntity list = new PatientEntity();
            list = IpatBLL.GetPatienttEntityByID(entity.UserID);
            if (chkcreatetime == 0)
            {
                //修改顾客最后到店时间
                entity.CreateTime = DateTime.Now;
                entity.OrderTime = entity.CreateTime;
            }
            else
            {
                entity.CreateTime = Convert.ToDateTime(Convert.ToDateTime(txtCreateTime).ToString(timess));
                //entity.CreateTime = Convert.ToDateTime(Convert.ToDateTime(txtCreateTime).ToString("yyyy-MM-dd 10:10:10"));
                entity.OrderTime = entity.CreateTime;
            }
            //汪 end

            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
            entity.OrderType = (int)OrderType.退换;
            entity.Statu = (int)OrderStatu.已结算;
            entity.ActurePrice = entity.Price;
            entity.Name = Memo;
            entity.DocumentNumber = GetDocumentNumber(entity.CreateTime);
            entity.TuiType = TuiType;
            //添加订单
            string result = cashBll.AddOrder(entity);
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            Dictionary<int, int> isgiveDic = new Dictionary<int, int>();
            Dictionary<int, decimal> otherpay = new Dictionary<int, decimal>();//其他支付方式金额
            Dictionary<int, int> otherpaytype = new Dictionary<int, int>();//其他支付方式类型
            string[] HuankuanList1 = HuankuanList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] HuankuanList12 = HuankuanList11.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var patientEntity = IpatBLL.GetPatienttEntityByID(entity.UserID);
            string[] IDList1 = IDList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] TimesList1 = TimesList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] CardIDList1 = CardIDList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] cardidarrList1 = cardidarr.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//余额记录ID

            string[] canyuUserID = UserIDList.Split(new Char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);

            if (result != null)
            {
                OrderinfoEntity orderinfo = new OrderinfoEntity();
                orderinfo.OrderNo = entity.OrderNo;
                orderinfo.InfoBuyType = 9;

                for (int i = 0; i < IDList1.Length; i++)
                {
                    if (LoginUserEntity.HospitalID == 1017)
                    {
                        // var dd = IDList1[i];

                        var model = cashBll.MainCardBalance_GetByID(Convert.ToInt32(IDList1[i]));
                        var paymentOrderList = cashBll.GetAllPaymentOrderList(new PaymentOrderEntity { OrderNo = model.OrderNo });
                        paymentOrderList = paymentOrderList.Where(t => t.PayName == "积分消费").ToList();
                        var intrgral = IpatBLL.GetIntegralModel(model.UserID); //获取原来的积分列表
                        intrgral.AllIntegral = intrgral.AllIntegral + paymentOrderList[0].PayMoney;
                        intrgral.Integral = intrgral.Integral + paymentOrderList[0].PayMoney;
                        IpatBLL.UpdateIntegral(intrgral);
                    }
                    //修改用户余额记录
                    cashBll.UpdateMainCardBalanceTimeByID(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(IDList1[i]), Tiems = ConvertValueHelper.ConvertIntValue(TimesList1[i]), ExpireTime = DateTime.Now });
                    //添加详情信息表
                    orderinfo.Type = 1;
                    orderinfo.ProdcuitID = ConvertValueHelper.ConvertIntValue(CardIDList1[i]);
                    orderinfo.OldPrice = ConvertValueHelper.ConvertDecimalValue(HuankuanList1[i]);
                    orderinfo.Price = ConvertValueHelper.ConvertDecimalValue(HuankuanList1[i]) / ConvertValueHelper.ConvertIntValue(TimesList1[i]);
                    orderinfo.AllPrice = ConvertValueHelper.ConvertDecimalValue(HuankuanList1[i]);
                    orderinfo.Quantity = ConvertValueHelper.ConvertIntValue(TimesList1[i]);
                    orderinfo.CardType = 2;
                    orderinfo.CardBalanceID = IDList1[i].ToString();
                    var projectEntity = baseinfoBLL.GetProjectModel(orderinfo.ProdcuitID);
                    orderinfo.ProdcuitName = projectEntity.Name;
                    orderinfo.ProductType = projectEntity.ProjectType;
                    orderinfo.BrandID = projectEntity.BrandID;
                    orderinfo.BrandName = iwareBLL.GetBrandModel(new BrandEntity { ID = projectEntity.BrandID }).Name;
                    orderinfo.ProductTypeName = iwareBLL.GetProductTypeModel(new ProductTypeEntity { ID = projectEntity.ProjectType }).Name;
                    results = cashBll.AddOrderinfo(orderinfo);
                    //添加订单详细CardBalanceID 关联表
                    cashBll.AddOrderinfoProject(new OrderinfoProjectEntity { OrderInfoID = results, CardBalanceID = ConvertValueHelper.ConvertIntValue(orderinfo.CardBalanceID) });
                    //获取参与员工
                    if (results > 0)
                    {
                        string[] nowUserID = canyuUserID[i].Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        JoinUserEntity model = new JoinUserEntity();
                        for (int j = 0; j < nowUserID.Length; j++)
                        {
                            model.UserID = ConvertValueHelper.ConvertIntValue(nowUserID[j]);
                            if (patientEntity.FollowUpMrUserID == model.UserID || patientEntity.FollowUpUserID == model.UserID)
                            {
                                model.IsZhiding = 1;
                            }
                            else
                            {
                                model.IsZhiding = 0;
                            }
                            model.OrderNo = entity.OrderNo;
                            model.OrderInfoID = results;
                            orderinfo.ProdcuitID = ConvertValueHelper.ConvertIntValue(CardIDList1[i]);
                            orderinfo.Type = 1;
                            //业绩,平均分配
                            //model.Yeji = ConvertValueHelper.ConvertDecimalValue((-orderinfo.Price / nowUserID.Length).ToString("f2"));
                            cashBll.AddJoinUser(model);
                        }
                    }
                }

                //如果存在手续费，则添加一条记录
                if (!string.IsNullOrEmpty(shouxufei) && ConvertValueHelper.ConvertDecimalValue(shouxufei) > 0)
                {
                    orderinfo.Type = 1;
                    orderinfo.Quantity = -1;
                    orderinfo.ProdcuitName = "手续服务费";
                    orderinfo.OldPrice = Convert.ToDecimal(shouxufei);
                    orderinfo.Price = Convert.ToDecimal(shouxufei);
                    orderinfo.AllPrice = -Convert.ToDecimal(shouxufei);
                    results = cashBll.AddOrderinfo(orderinfo);

                }
                ////其他支付方式
                //if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                //{
                //    string[] strpay = OtherPayment.Split(',');
                //    string[] OtherPaymentValuelist = OtherPaymentValue.Split(',');
                //    for (int i = 0; i < strpay.Length; i++)
                //    {
                //        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = ConvertValueHelper.ConvertIntValue(strpay[i]), PayMoney = ConvertValueHelper.ConvertIntValue(OtherPaymentValuelist[i]) });
                //    }
                //}
                //if (entity.Xianjin > 0)//现金
                //{
                //    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.现金, PayMoney = entity.Xianjin });
                //}
                //if (entity.Yinlianka > 0)//银联卡
                //{
                //    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.银联卡, PayMoney = entity.Yinlianka });
                //}
                //if (entity.Chuzhika > 0)//储值卡
                //{
                //    //修改储值卡金额,把钱退到储值卡上面去
                //    foreach (var info in cardidarrList1)
                //    {
                //        var mod = new MainCardBalanceEntity();
                //        mod.Price = entity.Price;
                //        mod.ID = ConvertValueHelper.ConvertIntValue(info);
                //        dic.Add(mod.ID, mod.Price);
                //        cashBll.UpdateMainCardBalanceMoneyNoExpireTime(mod);
                //        break;
                //    }
                //    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.储值卡, PayMoney = entity.Chuzhika });

                //}

                //添加订单支付方式
                PaymentOrderEntity poe = new PaymentOrderEntity();
                //其他支付方式
                if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                {
                    string[] strpay = OtherPayment.Split(',');
                    string[] OtherPaymentValuelist = OtherPaymentValue.Split(',');
                    string[] IsCashValueStr = IsCashValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//此字段判断是否是 现金还是券类 1 现金 2 券类
                    for (int i = 0; i < strpay.Length; i++)
                    {
                        poe.OrderNo = entity.OrderNo;
                        poe.PayType = ConvertValueHelper.ConvertIntValue(strpay[i]);
                        poe.PayMoney = ConvertValueHelper.ConvertDecimalValue(OtherPaymentValuelist[i]);
                        otherpay.Add(poe.PayType, poe.PayMoney);//其他支付方式
                        poe.BalanceMoney = 0;
                        poe.BalanceTiems = 0;
                        poe.CardID = 0;
                        if (ConvertValueHelper.ConvertIntValue(IsCashValueStr[i]) == 1)
                        {
                            poe.ParentPayType = (int)ParentPayType.现金;
                        }
                        else
                        {
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
                    poe.ParentPayType = (int)ParentPayType.储值卡;
                    poe.BalanceTiems = 0;
                    //扣除卡类金额
                    if (!String.IsNullOrEmpty(cardidarr))
                    {
                        string[] ordertimesstrlist = txtChuzhikaarr.Split(',');
                        string[] caridlist = cardidarr.Split(',');
                        string[] IsGiveValueList = IsGiveValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int f = 0; f < caridlist.Length; f++)
                        {
                            dic.Add(ConvertValueHelper.ConvertIntValue(caridlist[f]), ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]));
                            isgiveDic.Add(ConvertValueHelper.ConvertIntValue(caridlist[f]), ConvertValueHelper.ConvertIntValue(IsGiveValueList[f]));
                            cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Price = -ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) });

                            //用户储值卡余额明细日志
                            var mainCardBalanceDetail = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity() { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]) });
                            cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                            {
                                CardBalanceId = mainCardBalanceDetail.ID,
                                OrderNo = entity.OrderNo,
                                Type = (int)MainCardBalanceDetailType.退卡,
                                Amount = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]),
                                Balance = mainCardBalanceDetail.Price,
                                OldAmount = mainCardBalanceDetail.Price + ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]),
                                CreateTime = DateTime.Now
                            });

                            //获取这张支付卡的详细信息
                            var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]) });
                            var chuzhika = baseinfoBLL.GetPrePayCardModel(CardBalance.ProjectID);
                            poe.PayName = chuzhika.Name;
                            poe.PayMoney = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]);
                            poe.CardBalanceID = ConvertValueHelper.ConvertIntValue(caridlist[f]);
                            poe.IsGive = ConvertValueHelper.ConvertIntValue(IsGiveValueList[f]);
                            poe.CardID = CardBalance.CardID;
                            poe.BalanceMoney = CardBalance.Price;
                            poe.ids = poe.CardBalanceID.ToString();
                            cashBll.AddPaymentOrder(poe);
                        }
                    }
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
            decimal otherpayprice = 0;//其他已经付款的
            //储值卡集合
            List<int> cardlist = new List<int>();
            cardlist.AddRange(dic.Keys);
            //其他支付方式
            List<int> otherpaylist = new List<int>();
            otherpaylist.AddRange(otherpay.Keys);
            foreach (var info in orderInfoList)
            {
                int row, pagecount;
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                if (info.ProdcuitName == "手续服务费")
                {
                    continue;
                }
                //新的产品或者项目
                PayCash = 0;
                PayCarded = 0;
                ChuzhikaPayed = 0;
                otherpayprice = 0;
                PaymentOrderProductEntity conditon = new PaymentOrderProductEntity();
                conditon.AllPrice = info.AllPrice;//订单产品总价
                conditon.OrderInfoID = info.ID;//支付订单产品
                #region==支付==

                #region==现金支付
                if (ShengyuCash > 0)//现金
                {
                    conditon.PayType = (int)PaymentEnum.现金;
                    conditon.ParentPayType = (int)ParentPayType.现金;
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

                #region==银联卡支付==
                if (YinLianCard > 0)//银联卡
                {
                    conditon.PayType = (int)PaymentEnum.银联卡;
                    conditon.ParentPayType = (int)ParentPayType.现金;
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
                            cond.XiaoHao = -info.AllPrice;
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
                            conditon.ParentPayType = (int)ParentPayType.储值卡;
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
                                    if (PayCash + PayCarded + manPrice >= info.AllPrice)
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

                #region==其他支付方式==
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
                                    conditon.PayMoney = info.AllPrice < 0 ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed) : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
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
                            { //银联卡部分不够
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

            #region==计算员工业绩==
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
                    try
                    {
                        string conStr = "server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft112";//连接字符串  
                        SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
                        conText.Open();//打开数据库  
                        string sqls = @"SELECT top 1 BaseType,*  from EYB_tb_Project WHERE ID = " + info.ProdcuitID + @"  ";
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
                    if (ViewBag.BaseType == "3")
                    {
                        //修改业绩
                        cond.Yeji_Daxiangmu = -ConvertValueHelper.ConvertDecimalValue((paymoney1 / joinList.Count).ToString("f2"));//现金业绩
                                                                                                                                     //cond.Yeji_Liaochengka = -cond.Yeji_tuika;//因为退换是负数
                        cond.Yeji = cond.Yeji_Daxiangmu;

                        cond.KakouYeji_Daxiangmu = ConvertValueHelper.ConvertDecimalValue((paymoney / joinList.Count).ToString("f2"));//卡扣业绩
                        cond.KakouYeji_Daxiangmu = -cond.KakouYeji_Daxiangmu;
                        cond.KakouYeji = cond.KakouYeji_Daxiangmu;//获取卡扣业绩
                    }
                    else
                    {
                        //修改业绩
                        cond.Yeji_Liaochengka = -ConvertValueHelper.ConvertDecimalValue((paymoney1 / joinList.Count).ToString("f2"));//现金业绩
                                                                                                                                     //cond.Yeji_Liaochengka = -cond.Yeji_tuika;//因为退换是负数
                        cond.Yeji = cond.Yeji_Liaochengka;

                        cond.KakouYeji_Liaocheng = ConvertValueHelper.ConvertDecimalValue((paymoney / joinList.Count).ToString("f2"));//卡扣业绩
                        cond.KakouYeji_Liaocheng = -cond.KakouYeji_Liaocheng;
                        cond.KakouYeji = cond.KakouYeji_Liaocheng;//获取卡扣业绩
                    }
                    //修改业绩
                    //cond.Yeji_Liaochengka = -ConvertValueHelper.ConvertDecimalValue((paymoney1 / joinList.Count).ToString("f2"));//现金业绩
                    //cond.Yeji_Liaochengka = -cond.Yeji_tuika;//因为退换是负数
                    //cond.Yeji = cond.Yeji_Liaochengka;
                    //cond.KakouYeji_Liaocheng = ConvertValueHelper.ConvertDecimalValue((paymoney / joinList.Count).ToString("f2"));//卡扣业绩
                    //cond.KakouYeji_Liaocheng = -cond.KakouYeji_Liaocheng;
                    //cond.KakouYeji = cond.KakouYeji_Liaocheng;//获取卡扣业绩
                    cond.XiaoHao = 0;//退卡没有消耗
                    cond.XiaoHao_kakou = 0;
                    cond.XiaoHao_Liaocheng = 0;

                    cashBll.UpdateJoinUserYejiForPay(cond);
                }
            }
            #endregion
            #region ==修改支付金额==
            UpdatePayMoney(entity.OrderNo);
            #endregion


            #region ==记录次数更改的记录==
            for (int i = 0; i < IDList1.Length; i++)
            {
                ArchivesChangeLogEntity ac = new ArchivesChangeLogEntity();
                ac.CreateTime = DateTime.Now;
                ac.OrderNo = entity.OrderNo;
                ac.ChangeType = "Add";
                ac.LogType = 2;
                ac.ConsumptionUnitPrice = Convert.ToInt32(TimesList1[i]);
                ac.CourseCardIDList = ConvertValueHelper.ConvertIntValue(IDList1[i]).ToString();
                IpatBLL.AddArchivesChangeLog(ac);
            }
            #endregion
            //积分
            if (entity.Xianjin >= 1|| entity.Yinlianka>=1|| otherpayprice>0)
            {
                #region ==添加积分==
                var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate");  //获取积分实体
                decimal jifen = -Convert.ToDecimal(entity.Xianjin+entity.Yinlianka+ otherpayprice) / (string.IsNullOrEmpty(usersetting.AttributeValue) ? 100000 : Convert.ToDecimal(usersetting.AttributeValue));//判断能获取多少积分


                IntegralRecordEntity ir = new IntegralRecordEntity();
                ir.UserID = entity.UserID;
                ir.DocumentNumber = entity.OrderNo;
                ir.Integral = jifen;
                ir.CreateTime = DateTime.Now;
                IpatBLL.AddIntegralRecord(ir);  //添加用户积分记录

                var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                intrgral.AllIntegral = intrgral.AllIntegral + jifen;
                intrgral.Integral = intrgral.Integral + jifen;
                if (LoginUserEntity.HospitalID == 1017)
                {

                }
                else
                {
                    IpatBLL.UpdateIntegral(intrgral);
                }
                #endregion
            }

            try
            {

                Task.Factory.StartNew(() =>
                {
                    //var S = new WeiXinMessage().SendTuikaMessage(entity.OrderNo);
                    new WeiXinMessage().SendMessage(entity.OrderNo);
                });
            }
            catch (Exception e)
            {

                LogWriter.WriteInfo(e.Message, "发送退卡消息失败");
            }

            return Json(entity.OrderNo);
        }

        /// <summary>
        /// 修改退卡支付的金额
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <returns></returns>
        public int UpdatePayMoney(string OrderNo)
        {
            //先获取单据的支付方式
            var orderpaylist = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = OrderNo });
            foreach (var info in orderpaylist)
            {
                info.PayMoney = -info.PayMoney;
                //把支付金额改为负数再保存
                cashBll.UpdatePaymentOrder(info);
            }
            var orderinfolist = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = OrderNo });
            foreach (var oiinfo in orderinfolist)
            {
                var prolist = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = oiinfo.ID });
                foreach (var prinfo in prolist)
                {
                    prinfo.AllPrice = -prinfo.AllPrice;
                    prinfo.PayMoney = -prinfo.PayMoney;
                    cashBll.UpdatePaymentOrderProduct(prinfo);
                }
            }

            return 1;

        }

        #endregion

        #region==退换产品==

        /// <summary>
        /// 获取顾客购买产品明细
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetProductListByUserID(int UserID)
        {
            var list = cashBll.GetProductListByUserID(UserID);
            var persoList = personnelBLL.GetAllUserByHospitalID(LoginUserEntity.HospitalID, 0);
            StringBuilder str = new StringBuilder();
            //参与员工
            StringBuilder persoListstr = new StringBuilder();
            persoListstr.AppendFormat("<ul style='position:relative;'>");
            persoListstr.AppendFormat("<li class='liMenu1' ><a href='javascript:;'> 点击选择</a></li>");
            persoListstr.AppendFormat("<ul class='liHide1 ulClass'>");

            foreach (var info in persoList)
            {
                persoListstr.AppendFormat("<li class='liClass' ><input type='checkbox' code='{2}' id='{0}' value='{1}' name='UserID1' /><label for='{0}'>{2}</label> </li>", "chk1(" + info.UserID + ")", info.UserID, info.UserName);
            }
            persoListstr.AppendFormat("</ul>");
            persoListstr.AppendFormat("</ul>");
            foreach (var info in list)
            {
                str.Append("<tr class='cashpricetr'>");
                str.AppendFormat("<td><input type='checkbox' name='checkboxorder1' value='{0}' code={1} /></td>", info.ID, info.ProdcuitID);
                str.AppendFormat("<td>{0}</td>", info.ProdcuitName);
                // str.AppendFormat("<td>{0}</td>", info.OldPrice);
                str.AppendFormat("<td class='youhuiprice'>{0}</td>", info.Price);
                str.AppendFormat("<td class='shengyuTimes'>{0}</td>", info.Quantity);
                str.AppendFormat("<td>{0}</td>", info.AllPrice);
                str.AppendFormat("<td><input style='width:60px;' name='txttuitiems' onkeyup='javascript:CheckInputIntFloat(this);' class='tuitiems' type='text' /></td>");
                str.AppendFormat("<td><input style='width:60px;' name='txthuankuanp' onkeyup='javascript:CheckInputIntFloat(this);' class='cashprice' type='text' /></td>");
                str.AppendFormat("<td>{0}</td>", persoListstr.ToString());
                str.Append("</tr>");
            }
            return Content(str.ToString());
        }

        /// <summary>
        /// 退换操作-----产品
        /// 1、添加订单 2、添加订单详情   2更改结算表支付金额 3生成订单 4支付结算
        /// </summary>
        /// <returns></returns>
        public ActionResult PayExchangeOrderProduct(OrderEntity entity, string UserIDList, string Memo, string shouxufei, string TimesList, string txtChuzhikaarr, string IDList, string HuankuanList, string OtherPayment, string IsCashValue, string OtherPaymentValue, string CardIDList)
        {

            int results = 0;
            entity.CreateTime = DateTime.Now;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
            entity.OrderType = (int)OrderType.退换;
            entity.Statu = (int)OrderStatu.已结算;
            entity.ActurePrice = entity.Price;
            entity.DocumentNumber = GetDocumentNumber(entity.CreateTime);
            entity.Name = Memo;
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
                    //添加详情信息表
                    orderinfo.Type = 2;
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
                            orderinfo.ProdcuitID = ConvertValueHelper.ConvertIntValue(CardIDList1[i]);
                            orderinfo.Type = 2;
                            model.OrderInfoID = results;
                            //业绩,平均分配
                            model.Yeji = ConvertValueHelper.ConvertDecimalValue((-orderinfo.Price / nowUserID.Length).ToString("f2"));
                            model.KakouYeji_Chanpin = model.Yeji;
                            cashBll.AddJoinUser(model);
                        }
                    }
                }
                //如果存在手续费，则添加一条记录
                if (!string.IsNullOrEmpty(shouxufei) && ConvertValueHelper.ConvertDecimalValue(shouxufei) > 0)
                {
                    orderinfo.Type = 2;
                    orderinfo.Quantity = -1;
                    orderinfo.ProdcuitName = "手续服务费";
                    orderinfo.OldPrice = Convert.ToDecimal(shouxufei);
                    orderinfo.Price = Convert.ToDecimal(shouxufei);
                    orderinfo.AllPrice = -Convert.ToDecimal(shouxufei);
                    results = cashBll.AddOrderinfo(orderinfo);

                }
                //其他支付方式
                if (!String.IsNullOrEmpty(OtherPayment) && !String.IsNullOrEmpty(OtherPaymentValue))
                {
                    string[] strpay = OtherPayment.Split(',');
                    string[] OtherPaymentValuelist = OtherPaymentValue.Split(',');
                    string[] IsCashValueStr = IsCashValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//此字段判断是否是 现金还是券类 1 现金 2 券类
                    for (int i = 0; i < strpay.Length; i++)
                    {
                        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = ConvertValueHelper.ConvertIntValue(strpay[i]), ParentPayType = ConvertValueHelper.ConvertIntValue(IsCashValueStr[i]) == 1 ? (int)ParentPayType.现金 : (int)ParentPayType.券类, PayMoney = ConvertValueHelper.ConvertIntValue(OtherPaymentValuelist[i]) });
                    }
                }
                if (entity.Xianjin > 0)//现金
                {

                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.现金, ParentPayType = (int)ParentPayType.现金, PayMoney = entity.Xianjin });
                }
                if (entity.Yinlianka > 0)//银联卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.银联卡, ParentPayType = (int)ParentPayType.现金, PayMoney = entity.Yinlianka });
                }
                if (entity.Chuzhika > 0)//储值卡
                {
                    cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.储值卡, ParentPayType = (int)ParentPayType.储值卡, PayMoney = entity.Chuzhika, ids = IDList1.ToString() });

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
                    conditon.ParentPayType = (int)ParentPayType.现金;
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
                    conditon.ParentPayType = (int)ParentPayType.现金;
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
                    conditon.ParentPayType = (int)ParentPayType.储值卡;
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
                    string[] IsCashValueStr = IsCashValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//此字段判断是否是 现金还是券类 1 现金 2 券类
                    for (int i = 0; i < strpay.Length; i++)
                    {
                        if (ConvertValueHelper.ConvertIntValue(IsCashValueStr[i]) == 1)
                        {
                            conditon.ParentPayType = (int)ParentPayType.现金;
                        }
                        else
                        {
                            conditon.ParentPayType = (int)ParentPayType.券类;
                        }
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
            #region ==修改支付金额==
            UpdatePayMoney(entity.OrderNo);
            #endregion

            return Json(entity.OrderNo);
        }

        #endregion


        #region==退换储值卡==

        /// <summary>
        /// 获取顾客储值卡明细
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetCardListByUserID(int UserID)
        {
            var list = cashBll.GetMainCardBalanceList(new MainCardBalanceEntity { UserID = UserID, Type = 1 }).Where(c => c.Price > 0);
            list = list.Where(t=>t.IsDel!=3&&t.Name!= "积分消费");
            var persoList = personnelBLL.GetAllUserByHospitalID(LoginUserEntity.HospitalID, 0);
            StringBuilder str = new StringBuilder();
            //参与员工
            StringBuilder persoListstr = new StringBuilder();
            persoListstr.AppendFormat("<ul style='position:relative;'>");
            persoListstr.AppendFormat("<li class='liMenu2' ><a href='javascript:;'> 点击选择</a></li>");
            persoListstr.AppendFormat("<ul class='liHide2 ulClass'>");

            foreach (var info in persoList)
            {
                persoListstr.AppendFormat("<li class='liClass' ><input type='checkbox' code='{2}' id='{0}' value='{1}' name='UserID' /><label for='{0}'>{2}</label> </li>", "[#chk1](" + info.UserID + ")", info.UserID, info.UserName);
            }

            var HospitalList = personnelBLL.GetHospitalListByParentID(LoginUserEntity.ParentHospitalID);//获取父门店
            persoListstr.Append("<li class='liClass' style='margin-left:-10px;' >");
            persoListstr.Append("<select name=\"HospitalID\" class=\"select selHospitalID\" style=\"width:100px;color:Blue;font-size:12px;\">");
            persoListstr.Append("<option value=\"0\">选择跨店员工</option>");
            foreach (var info in HospitalList)
            {
                persoListstr.AppendFormat("<option value=\"{0}\">{1}</option>", info.ID, info.Name);
            }

            persoListstr.Append("</select>");
            persoListstr.Append("</li>");


            persoListstr.AppendFormat("</ul>");
            persoListstr.AppendFormat("</ul>");
            foreach (var info in list)
            {
                str.Append("<tr class='cashpricetr'>");
                // str.AppendFormat("<td><input type='checkbox' name='checkboxorder2' value='{0}' code='{1}' /></td>", info.ID, info.ProjectID);
                str.AppendFormat("<td><input type='checkbox' name='checkboxorder2' value='{0}' code='{1}' projectName='{2}' /></td>", info.ID, info.CardID, info.Name); //edit by kuangsg 2016/06/03
                str.AppendFormat("<td>{0}</td>", info.Name);
                str.AppendFormat("<td>{0}<input type='hidden' class='isgive' value='{1}' /></td>", info.IsGive == 1 ? "是" : "否", info.IsGive);
                str.AppendFormat("<td class='youhuiprice'>{0}</td>", info.Price);
                str.AppendFormat("<td><input style='width:60px;' name='txthuankuanp1' onkeyup='javascript:CheckInputIntFloat(this);' class='cashprice1' type='text' /></td>");
                str.AppendFormat("<td><input style='width:60px;' name='txthuankuanp' onkeyup='javascript:CheckInputIntFloat(this);' class='cashprice' type='text'  {0} /></td>", info.IsGive == 1 ? "disabled=disabled" : "");
                str.AppendFormat("<td >{0}</td>", persoListstr.ToString().Replace("[#chk1]", "chk" + info.ID));
                str.Append("</tr>");
            }
            return Content(str.ToString());
        }

        /// <summary>
        /// 退换操作-----储值卡
        /// 1、添加订单 2、添加订单详情   2更改结算表支付金额 3生成订单 4支付结算
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult PayExchangeOrderCard(OrderEntity entity, string UserIDList, string Memo, int TuiType, string shouxufei, string TimesList, string txtChuzhikaarr, string IDList, string HuankuanList, string HuankuanList11, string OtherPayment, string IsCashValue, string OtherPaymentValue, string CardIDList, string projectNameList, int chkcreatetime, string txtCreateTime)
        {
            try
            {

                int results = 0;

                //罗发明 补单时间时分秒换成实际的
                //var hms = DateTime.Now.ToString("hh:mm:ss").ToString();
                var hms = DateTime.Now.ToLongTimeString().ToString();
                var timess = "yyyy-MM-dd " + hms;

                //汪  7-26 补单及到店时间的修改
                PatientEntity list = new PatientEntity();
                list = IpatBLL.GetPatienttEntityByID(entity.UserID);
                if (chkcreatetime == 0)
                {
                    //修改顾客最后到店时间
                    entity.CreateTime = DateTime.Now;
                    entity.OrderTime = entity.CreateTime;
                }
                else
                {
                    entity.CreateTime = Convert.ToDateTime(Convert.ToDateTime(txtCreateTime).ToString(timess));
                    //entity.CreateTime = Convert.ToDateTime(Convert.ToDateTime(txtCreateTime).ToString("yyyy-MM-dd 10:10:10"));
                    entity.OrderTime = entity.CreateTime;
                }
                //汪 end

                entity.HospitalID = LoginUserEntity.HospitalID;
                entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
                entity.OrderType = (int)OrderType.退换;
                entity.Statu = (int)OrderStatu.已结算;
                entity.ActurePrice = entity.Price;
                entity.Name = Memo;
                entity.TuiType = TuiType;
                entity.DocumentNumber = GetDocumentNumber(entity.CreateTime);
                //添加订单
                string result = cashBll.AddOrder(entity);
                //应退金额
                string[] HuankuanList1 = HuankuanList11.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //实退金额
                string[] HuankuanList12 = HuankuanList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                string[] IDList1 = IDList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] TimesList1 = TimesList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] CardIDList1 = CardIDList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] projectNameList1 = projectNameList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                string[] canyuUserID = UserIDList.Split(new Char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                if (result != null)
                {
                    OrderinfoEntity orderinfo = new OrderinfoEntity();
                    orderinfo.OrderNo = entity.OrderNo;
                    orderinfo.InfoBuyType = 10;

                    for (int i = 0; i < IDList1.Length; i++)
                    {
                        //判断当前卡是否为赠送卡
                        var maincardList = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(IDList1[i]) });
                        var price = ConvertValueHelper.ConvertDecimalValue(HuankuanList1[i]);
                        //if (maincardList.IsGive == 1)//如果是赠卡
                        //{
                        //    price = maincardList.AllPrice;
                        //    price =ConvertValueHelper.ConvertDecimalValue(shouxufei);

                        //}
                        //更改用户余额记录
                        cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(IDList1[i]), Price = price });

                        //用户储值卡余额明细日志
                        var mainCardBalanceDetail = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity() { ID = ConvertValueHelper.ConvertIntValue(IDList1[i]) });
                        cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                        {
                            CardBalanceId = mainCardBalanceDetail.ID,
                            OrderNo = entity.OrderNo,
                            Type = (int)MainCardBalanceDetailType.退卡,
                            Amount = price,
                            Balance = mainCardBalanceDetail.Price,
                            OldAmount = mainCardBalanceDetail.Price + price,
                            CreateTime = DateTime.Now
                        });

                        //添加详情信息表
                        orderinfo.Type = 3;
                        orderinfo.ProdcuitName = projectNameList1[i];
                        orderinfo.ProdcuitID = ConvertValueHelper.ConvertIntValue(CardIDList1[i]);
                        orderinfo.OldPrice = ConvertValueHelper.ConvertDecimalValue(HuankuanList12[i]);
                        orderinfo.Price = ConvertValueHelper.ConvertDecimalValue(HuankuanList12[i]);
                        orderinfo.AllPrice = -ConvertValueHelper.ConvertDecimalValue(HuankuanList12[i]);
                        orderinfo.Quantity = -1;
                        orderinfo.CardType = 2;
                        //var model = baseinfoBLL.GetPrePayCardModel(orderinfo.ProdcuitID);
                        //orderinfo.ProdcuitName = model.Name;

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
                                orderinfo.ProdcuitID = ConvertValueHelper.ConvertIntValue(CardIDList1[i]);
                                orderinfo.Type = 3;
                                model.OrderInfoID = results;
                                //业绩,平均分配
                                //model.Yeji = ConvertValueHelper.ConvertDecimalValue((-orderinfo.Price / nowUserID.Length).ToString("f2"));
                                cashBll.AddJoinUser(model);
                            }
                        }
                    }
                    //如果存在手续费，则添加一条记录
                    if (!string.IsNullOrEmpty(shouxufei) && ConvertValueHelper.ConvertDecimalValue(shouxufei) > 0)
                    {
                        orderinfo.Type = 3;
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
                        string[] IsCashValueStr = IsCashValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//此字段判断是否是 现金还是券类 1 现金 2 券类
                        for (int i = 0; i < strpay.Length; i++)
                        {
                            cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = ConvertValueHelper.ConvertIntValue(strpay[i]), ParentPayType = ConvertValueHelper.ConvertIntValue(IsCashValueStr[i]) == 1 ? (int)ParentPayType.现金 : (int)ParentPayType.券类, PayMoney = -ConvertValueHelper.ConvertDecimalValue(OtherPaymentValuelist[i]) });
                        }
                    }
                    if (entity.Xianjin > 0)//现金
                    {
                        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.现金, ParentPayType = (int)ParentPayType.现金, PayName = "现金", PayMoney = -entity.Xianjin });
                    }
                    if (entity.Yinlianka > 0)//银联卡
                    {
                        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.银联卡, ParentPayType = (int)ParentPayType.现金, PayName = "银联卡", PayMoney = -entity.Yinlianka, ids = IDList1.ToString() });
                    }
                    if (entity.Chuzhika > 0)//储值卡
                    {
                        cashBll.AddPaymentOrder(new PaymentOrderEntity { OrderNo = entity.OrderNo, PayType = (int)PaymentEnum.储值卡, ParentPayType = (int)ParentPayType.储值卡, PayMoney = -entity.Chuzhika, ids = IDList1.ToString() });

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
                decimal otherpayprice = 0;//其他已经付款的
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
                    otherpayprice = 0;
                    PaymentOrderProductEntity conditon = new PaymentOrderProductEntity();
                    conditon.AllPrice = info.AllPrice;//订单产品总价
                    conditon.OrderInfoID = info.ID;//支付订单产品
                    #region==产品支付==

                    if (ShengyuCash > 0)//现金
                    {
                        conditon.ParentPayType = (int)ParentPayType.现金;
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
                        conditon.ParentPayType = (int)ParentPayType.现金;
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
                        conditon.ParentPayType = (int)ParentPayType.储值卡;
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
                        string[] IsCashValueStr = IsCashValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//此字段判断是否是 现金还是券类 1 现金 2 券类
                        for (int i = 0; i < strpay.Length; i++)
                        {
                            if (ConvertValueHelper.ConvertIntValue(IsCashValueStr[i]) == 1)
                            {
                                conditon.ParentPayType = (int)ParentPayType.现金;
                            }
                            else
                            {
                                conditon.ParentPayType = (int)ParentPayType.券类;
                            }
                            var nowprice = ConvertValueHelper.ConvertDecimalValue(OtherPaymentValuelist[i]);
                            if (nowprice > 0)
                            {
                                conditon.PayType = ConvertValueHelper.ConvertIntValue(strpay[i]);
                                if (nowprice > Math.Abs(info.AllPrice))//如果银联卡部分超出实际产品价格
                                {
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联和储值卡
                                    {
                                        conditon.PayMoney = info.AllPrice < 0 ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed) : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);

                                        OtherPaymentValuelist[i] = nowprice + "";

                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;//跳出循环
                                    }
                                    else
                                    {
                                        conditon.PayMoney = Math.Abs(info.AllPrice) - Math.Abs(otherpayprice);
                                        nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                        OtherPaymentValuelist[i] = nowprice + "";

                                        cashBll.AddPaymentOrderProduct(conditon);
                                        continue;//跳出循环
                                    }
                                }
                                else
                                { //银联卡部分不够
                                    if (PayCash + PayCarded + ChuzhikaPayed > 0)//支付了部分现金和银联
                                    {
                                        if (PayCash + PayCarded + ChuzhikaPayed + nowprice > Math.Abs(info.AllPrice))
                                        {
                                            conditon.PayMoney = info.AllPrice < 0 ? -(Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed - otherpayprice) : (Math.Abs(info.AllPrice) - PayCash - PayCarded - ChuzhikaPayed - otherpayprice);
                                            nowprice = nowprice - Math.Abs(conditon.PayMoney);
                                            OtherPaymentValuelist[i] = nowprice + "";

                                            cashBll.AddPaymentOrderProduct(conditon);
                                            continue;//跳出循环
                                        }
                                        else
                                        {
                                            otherpayprice = conditon.PayMoney = info.AllPrice < 0 ? -nowprice : nowprice;
                                            nowprice = 0;
                                            nowprice = Math.Abs(conditon.PayMoney);
                                            OtherPaymentValuelist[i] = "0";
                                            //添加产品支付方式

                                            cashBll.AddPaymentOrderProduct(conditon);
                                        }

                                    }
                                    else
                                    {
                                        //conditon.PayMoney = nowprice;
                                        otherpayprice = conditon.PayMoney = info.AllPrice < 0 ? -nowprice : nowprice;
                                        nowprice = 0;
                                        OtherPaymentValuelist[i] = "0";
                                        otherpayprice = nowprice = Math.Abs(conditon.PayMoney);

                                        cashBll.AddPaymentOrderProduct(conditon);

                                    }
                                }
                            }
                        }
                    }
                    #endregion

                }

                #region ==修改支付金额==
                //  UpdatePayMoney(entity.OrderNo);
                #endregion

                #region==计算员工业绩==
                //重新计算员工业绩
                foreach (var info in orderInfoList)
                {
                    int row, pagecount;
                    var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderInfoID = info.ID }, out row, out pagecount);
                    var infopaylist = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = info.ID, PayType = 1 });
                    foreach (var cond in joinList)
                    {
                        foreach (var fp in infopaylist)
                        {
                            if (fp.PayType == 1)//现金
                            {
                                cond.Yeji_Chuzhika += fp.PayMoney / joinList.Count;
                            }
                            else if (fp.PayType == 2)//银联卡
                            {
                                cond.Yeji_Chuzhika += fp.PayMoney / joinList.Count;
                            }
                            //else if (fp.PayType == 3)
                            //{
                            //    cond.KakouYeji_tuika += fp.PayMoney / joinList.Count;
                            //}
                        }
                        cond.Yeji = cond.Yeji_Chuzhika;
                        //cond.KakouYeji = cond.KakouYeji_tuika;

                        cashBll.UpdateJoinUserYejiForPay(cond);
                    }
                }
                #endregion


                #region ==写入结余日志数据==

                ArchivesChangeLogEntity ac = new ArchivesChangeLogEntity();
                ac.CreateTime = DateTime.Now;
                ac.OrderNo = entity.OrderNo;
                ac.ChangeType = "Add";
                for (int i = 0; i < IDList1.Length; i++)
                {
                    ac.ChangeType = "Add";
                    ac.LogType = 1;
                    ac.ConsumptionUnitPrice = Convert.ToDecimal(HuankuanList1[i]);
                    ac.CourseCardIDList = IDList1[i];
                    IpatBLL.AddArchivesChangeLog(ac);
                }

                //var neworderInfoList = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = entity.OrderNo });
                //foreach (var info in neworderInfoList)
                //{
                //    ac.ChangeType = "Delete";
                //    ac.LogType = 1;
                //    ac.ConsumptionUnitPrice = info.AllPrice;
                //    ac.CourseCardIDList = info.ProdcuitID.ToString();
                //    IpatBLL.AddArchivesChangeLog(ac);
                //}

                #endregion

                //积分
                if (entity.Xianjin >= 1||entity.Yinlianka>=1|| otherpayprice>0)
                {
                    #region ==添加积分==
                    var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate");  //获取积分实体
                    decimal jifen = -Convert.ToDecimal(entity.Xianjin+ otherpayprice+ entity.Yinlianka) / (usersetting.AttributeValue == null ? 100000 : Convert.ToDecimal(usersetting.AttributeValue));//判断能获取多少积分

                    var intrgral = IpatBLL.GetIntegralModel(entity.UserID);//获取原来的积分列表
                    if (intrgral.Integral>0)
                    {
                        IntegralRecordEntity ir = new IntegralRecordEntity();
                        ir.UserID = entity.UserID;
                        ir.DocumentNumber = entity.OrderNo;
                        ir.Integral = jifen;
                        ir.CreateTime = DateTime.Now;
                        IpatBLL.AddIntegralRecord(ir);  //添加用户积分记录
                        intrgral.AllIntegral = intrgral.AllIntegral + jifen;
                        intrgral.Integral = intrgral.Integral + jifen;
                        if (LoginUserEntity.HospitalID == 1017)
                        {

                        }
                        else
                        {
                            IpatBLL.UpdateIntegral(intrgral);
                        } 
                    }

                       
                    #endregion
                }
                try
                {

                    Task.Factory.StartNew(() =>
                    {
                        // var S = new WeiXinMessage().SendTuikaMessage(entity.OrderNo);
                        new WeiXinMessage().SendMessage(entity.OrderNo);

                    });
                }
                catch (Exception e)
                {

                    LogWriter.WriteInfo(e.Message, "发送退卡消息失败");
                }
                return Json(entity.OrderNo);
            }
            catch
            {
                return Json(-1);
            }
        }

        #endregion

        #endregion
    }
}
