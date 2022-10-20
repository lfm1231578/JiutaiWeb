using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using EYB.ServiceEntity.BaseInfo;
using EYB.ServiceEntity.CashEntity;
using HS.SupportComponents;
using Common.Helper;
using EYB.ServiceEntity.PatientEntity;
using System.Threading.Tasks;

namespace EYB.Web.Controllers
{
    /// <summary>
    /// 还款类操作
    /// </summary>
    public partial class CashManageController : BaseController
    {
        #region==充值还款==
        /// <summary>
        /// 还款
        /// </summary>
        /// <returns></returns>
        public ActionResult Repayment()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }
        /// <summary>
        /// 获取顾客欠款记录
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult GetPaymentRecordList(int UserID)
        {
            var list = cashBll.GetPaymentRecordList(UserID);
            StringBuilder str = new StringBuilder();

            int rows = 0;
            int pagecount = 0;
            foreach (var info in list)
            {
                //获取订单参与人员
                var joinList = cashBll.GetAllJoinUser(new JoinUserEntity { OrderNo = info.OrderNo }, out rows, out pagecount);
                str.Append("<tr class='cashpricetr'>");
                str.AppendFormat("<td><input type='checkbox' name='checkboxorder' value='{0}' code='{1}' /></td>", info.OrderNo, info.DocumentNumber);
                str.AppendFormat("<td><a href='#' onclick='OpenOrderDetail(\"{1}\")'>{0}</a></td>", info.OrderNo, info.OrderNo);
                str.AppendFormat("<td>{0}</td>", info.CreateTime.ToString("yyyy-MM-dd"));
                str.AppendFormat("<td>{0}</td>", EYB.Web.Code.PageFunction.GetorderType(info.OrderType));
                str.AppendFormat("<td>{0}</td>", info.QianPrice);
                str.AppendFormat("<td  >{0}</td>", info.HuanPrice);
                str.AppendFormat("<td  class='qianprice' >{0}</td>", (info.QianPrice + info.HuanPrice));
                str.AppendFormat("<td><input style='width:80px;' name='txthuankuanp' onkeyup='javascript:CheckInputIntFloat(this);' class='cashprice' type='text' /></td>");
                str.AppendFormat("<td>{0}</td>", GetPersonList(joinList));
                str.Append("</tr>");
            }
            return Content(str.ToString());
        }


        public string GetPersonList(IList<JoinUserEntity> list)
        {
            var name = "";
            foreach (var joininfo in list)
            {
                if (string.IsNullOrEmpty(name))
                {
                    name += joininfo.UserName;
                }
                else
                {
                    string[] res = name.Split(',');
                    if (!res.Contains(joininfo.UserName))
                    {
                        name += "," + joininfo.UserName;
                    }
                }

            }
            var persoList = personnelBLL.GetAllUserByHospitalID(LoginUserEntity.HospitalID, 0);
            //美容师
            StringBuilder persoListstr = new StringBuilder();
            persoListstr.AppendFormat("<ul style='position:relative;'>");
            persoListstr.AppendFormat("<li class='liMenu' style='z-index:0; display:block;' ><a href='javascript:;'  style='z-index:0; '> {0}</a></li>", string.IsNullOrEmpty(name) ? "点击选择" : name);
            persoListstr.AppendFormat("<ul  class='liHide ulClass'>");

            foreach (var info in persoList)
            {
                var ischecked = list.Select(c => c.UserID).ToList().Contains(info.UserID);
                persoListstr.AppendFormat("<li class='liClass' ><input code='{2}' type='checkbox' id='{0}' value='{1}' name='UserID' {3} /><label for='{0}'>{2}</label> </li>", "chk1(" + info.UserID + ")", info.UserID, info.UserName, ischecked ? "checked=checked" : "");
            }
            var hospitalList = personnelBLL.GetHospitalListByParentID(LoginUserEntity.ParentHospitalID);
            persoListstr.AppendFormat("<li class='liClass' style='margin-left:-10px;'>");
            persoListstr.AppendFormat("<select name='HospitalID' class='select selHospitalID' style='width:100px;color:Blue;font-size:12px;'>");
            persoListstr.AppendFormat("<option value='0'>选择跨店员工</option>");
            foreach (var info in hospitalList)
            {
                if (info.ID != ViewBag.HospitalID)
                {
                    persoListstr.AppendFormat("<option  value='{1}' >{0}</option>", info.Name, info.ID);

                }
            }
            persoListstr.AppendFormat("  </select></li>");

            persoListstr.AppendFormat("</ul>");
            persoListstr.AppendFormat("</ul>");
            return persoListstr.ToString();
        }





        /// <summary>
        /// 还款操作
        /// 1、添加订单 2、添加订单   2更改结算表支付金额 3生成订单 4支付结算
        /// </summary>
        /// <returns></returns>
        public ActionResult RepaymentOrder(OrderEntity entity, string UserIDList, string txtChuzhikaarr, string DocumentNumList, string Memo, string OrderNoList, string HuankuanList, string OtherPayment, string IsCashValue, string IsGiveValue, string OtherPaymentValue, string cardidarr, int chkcreatetime, string txtCreateTime, string hiddenToken)
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
            int results = 0;

            //罗发明 补单时间时分秒换成实际的

            //var hms = DateTime.Now.ToString("hh:mm:ss").ToString();
            var hms = DateTime.Now.ToLongTimeString().ToString();
            //var timess = "yyyy-MM-dd " + hms;

            //汪  7-26 补单及到店时间的修改
            PatientEntity list = new PatientEntity();
            list = IpatBLL.GetPatienttEntityByID(entity.UserID);
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
                    entity.CreateTime = Convert.ToDateTime(txtCreateTime + " " + hms);
                    //entity.CreateTime = Convert.ToDateTime(txtCreateTime + " 10:10:10");
                    entity.OrderTime = entity.CreateTime;
                }

            }
            else
            {
                entity.CreateTime = Convert.ToDateTime(txtCreateTime + " " + hms);
                //entity.CreateTime = Convert.ToDateTime(txtCreateTime + " 10:10:10");
                entity.OrderTime = entity.CreateTime;
            }
            //汪 end

            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.OrderNo = RandomHelper.GetRandomOrder(entity.CreateTime);
            entity.OrderType = (int)OrderType.还款;
            entity.Statu = (int)OrderStatu.已结算;
            entity.ActurePrice = entity.Price + entity.QianPrice;
            entity.DocumentNumber = GetDocumentNumber(entity.CreateTime);
            entity.Name = Memo;
            //添加订单
            string result = cashBll.AddOrder(entity);
            var patientEntity = IpatBLL.GetPatienttEntityByID(entity.UserID);
            decimal OtherCash = 0;//其他方式支付的 现金部分
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            Dictionary<int, int> isgiveDic = new Dictionary<int, int>();
            Dictionary<int, decimal> otherpay = new Dictionary<int, decimal>();//其他支付方式金额
            Dictionary<int, int> otherpaytype = new Dictionary<int, int>();//其他支付方式类型
            // string[] OrderNoList = Request.Form.GetValues("OrderNoList");//订单编号
            //string[] HuankuanList = HuankuanList;//本次还款金额
            string[] HuankuanList1 = HuankuanList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] OrderNoList1 = OrderNoList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] DocumentNumList1 = DocumentNumList.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] canyuUserID = UserIDList.Split(new Char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            string[] cardidarrList1 = cardidarr.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//余额记录ID
            if (result != null)
            {
                OrderinfoEntity orderinfo = new OrderinfoEntity();
                orderinfo.OrderNo = entity.OrderNo;
                orderinfo.InfoBuyType = 8;//还款操作
                for (int i = 0; i < OrderNoList1.Length; i++)
                {
                    orderinfo.Quantity = 1;
                    orderinfo.Price = Convert.ToDecimal(HuankuanList1[i]);
                    orderinfo.AllPrice = Convert.ToDecimal(HuankuanList1[i]);
                    orderinfo.ProdcuitName = OrderNoList1[i].ToString();
                    orderinfo.CardType = 2;
                    //修改订单欠款情况，只修改欠款字段
                    cashBll.UpdateOrderQiankuan(new OrderEntity { QianPrice = orderinfo.Price, OrderNo = OrderNoList1[i] });
                    //添加订单修改记录
                    AddRepaymentRecords(OrderNoList1[i], orderinfo.Price);

                    //添加详情信息表
                    results = cashBll.AddOrderinfo(orderinfo);

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
                            //业绩,平均分配
                            //  model.Yeji = orderinfo.Price / nowUserID.Length;//正常员工业绩就是项目或产品的价格
                            model.Yeji = ConvertValueHelper.ConvertDecimalValue((orderinfo.Price / nowUserID.Length).ToString("f2"));
                            model.Yeji_huankuan = model.Yeji / 2;
                            model.KakouYeji_Huankuan = model.Yeji / 2;
                            cashBll.AddJoinUser(model);
                        }
                    }
                }
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
                            OtherCash = OtherCash + poe.PayMoney;
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
                    poe.ParentPayType = (int)ParentPayType.现金;
                    poe.BalanceMoney = 0;
                    poe.BalanceTiems = 0;
                    poe.CardID = 0;

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
                            cashBll.UpdateMainCardBalanceMoneyNoExpireTime(new MainCardBalanceEntity { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]), Price = ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]) });

                            //用户储值卡余额明细日志
                            var mainCardBalanceDetail = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity() { ID = ConvertValueHelper.ConvertIntValue(caridlist[f]) });
                            cashBll.AddMainCardBalanceDetail(new MainCardBalanceDetailEntity()
                            {
                                CardBalanceId = mainCardBalanceDetail.ID,
                                OrderNo = entity.OrderNo,
                                Type = (int)MainCardBalanceDetailType.还款,
                                Amount = -ConvertValueHelper.ConvertDecimalValue(ordertimesstrlist[f]),
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



            //修改顾客欠款
            cashBll.UpdateArrearsMoney(entity.UserID, -entity.Price);

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
                #endregion

                #region==银联卡支付==
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

                    //修改业绩
                    cond.KakouYeji = ConvertValueHelper.ConvertDecimalValue((paymoney / joinList.Count).ToString("f2"));//卡扣业绩
                    cond.KakouYeji_Huankuan = cond.KakouYeji;

                    cond.Yeji = ConvertValueHelper.ConvertDecimalValue((paymoney1 / joinList.Count).ToString("f2"));//现金业绩
                    cond.Yeji_huankuan = cond.Yeji;
                    //还款没有消耗
                    cond.XiaoHao = 0;
                    cond.XiaoHao_kakou = 0;
                    cond.XiaoHao_Liaocheng = 0;



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
            if (entity.Xianjin != 0 || OtherCash != 0 || entity.Yinlianka != 0)
            {
                #region ==添加积分==
                var usersetting = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "Intergate");  //获取积分实体
                decimal jifen = Convert.ToDecimal(entity.Xianjin + OtherCash + entity.Yinlianka) / (string.IsNullOrEmpty(usersetting.AttributeValue) ? 100000 : Convert.ToDecimal(usersetting.AttributeValue));//判断能获取多少积分


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
                #endregion
            }

            try
            {

                Task.Factory.StartNew(() =>
                {
                    // var S = new WeiXinMessage().SendHuanKuanMessage(entity.OrderNo);
                    new WeiXinMessage().SendMessage(entity.OrderNo);

                });
            }
            catch (Exception e)
            {

                LogWriter.WriteInfo(e.Message, "发送还款消息失败");
            }
            return Json(entity.OrderNo);
        }


        /// <summary>
        /// 添加还款记录
        /// --先通过订单编号获取订单的信息
        /// --然后编写还款记录
        /// --然后保存
        /// </summary>
        /// <param name="OrderNo"></param>
        public void AddRepaymentRecords(string _OrderNo, decimal price)
        {
            var order = cashBll.GetOrderModel(new OrderEntity { OrderNo = _OrderNo });

            RepaymentRecordsEntity entity = new RepaymentRecordsEntity();
            entity.UserID = order.UserID;
            entity.OrderNo = order.OrderNo;
            entity.AllQianKuan = order.QianPrice;
            entity.RepaymentMoney = price;
            entity.ReturnQianKuan = order.QianPrice - order.AllQianprice;
            entity.SurplusQianKuan = order.AllQianprice;
            entity.CreateTime = DateTime.Now;
            cashBll.AddRepaymentRecords(entity);
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderModel(string OrderNo)
        {
            var result = cashBll.GetOrderModel(new OrderEntity { OrderNo = OrderNo });
            return Json(result);
        }
        /// <summary>
        /// 付款操作---还款操作
        /// </summary>
        /// <returns></returns>
        public ActionResult PayOrderPageRepayment()
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.Username = LoginUserEntity.UserName;
            var entity = sysbll.GetUserSettingsValue(LoginUserEntity.HospitalID, "IsOpenAutoPrint");
            ViewBag.IsOpenAutoPrint = entity.AttributeValue;
            return View();
        }


        #endregion
    }
}
