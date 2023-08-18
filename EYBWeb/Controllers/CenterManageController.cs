using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EYB.ServiceEntity.CashEntity;
using System.Text;
using HS.SupportComponents;
using EYB.ServiceEntity.PatientEntity;
using CashManage.Factory.IBLL;
using PersonnelManage.Factory.IBLL;
using BaseinfoManage.Factory.IBLL;
using ReservationDoctorManage.Factory.IBLL;
using SystemManage.Factory.IDAL;
using PatientManage.Factory.IBLL;
using Webdiyer.WebControls.Mvc;
using WarehouseManage.Factory.IBLL;
using EYB.ServiceEntity.WarehouseEntity;
using EYB.ServiceEntity.PersonnelEntity;
using Common.Attribute;
using System.Data;
using EYB.ServiceEntity.CenterEntity;
using CenterManage.Factory.IBLL;
using System.Web.Script.Serialization;
using YcHighChartsMvc.Model.Chart.SeriesStlye;
using EYB.Web.Code;
using System.Data.SqlClient;

namespace EYB.Web.Controllers
{
    public partial class CenterManageController : BaseController
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
        public CenterManageController()
        {
            cashBll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<CashManage.Factory.IBLL.ICashManageBLL>();
            personnelBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            baseinfoBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IBaseinfoBLL>();
            ResDocBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IReservationDoctorManageBLL>();
            IpatBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPatientManageBLL>();
            sysbll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageDAL>();
            iwareBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IWarehouseManageBLL>();
            iCentBll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ICenterManageBLL>();
        }

        #endregion 依赖注入


        #region 前台管理
        public ActionResult GoalsListInfo()
        {
            return View();
        }
        public ActionResult ReceptionManager(OrderinfoEntity entity)
        {

            if (entity.HospitalID == 0) entity.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            entity.STime = entity.STime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.STime.ToString("yyyy-MM-dd 00:00:01"));
            entity.ETime = entity.ETime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.ETime.ToString("yyyy-MM-dd 23:59:59"));
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderinfoEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            int rows;
            int pagecount;
            entity.numPerPage = 50; //每页显示条数
            string newcardname = entity.CardName;
            if (entity.OrderType == 6)
            {
                entity.CardName = "";
                entity.numPerPage = 1000;
            }
            decimal NeedAllPrice, Price, XianJin, ShuaKa, DiscountAmount, SendAmount, Huachika = 0;
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            var list = cashBll.GetReceptionManager(entity, out rows, out pagecount, out NeedAllPrice, out Price, out XianJin, out ShuaKa, out DiscountAmount, out SendAmount, out Huachika);
            // OrderinfoEntity entity1 = entity;
            //  entity1.numPerPage = 50000;
            // 梳理系统设置的权限组管理的PC端权限列表的角色权限下权限勾选接口数据及增加统一选择功能（完成）
            // 梳理手机端的护理日志关联电脑端护理日志接口数据功能（完成）
            // var list1 = cashBll.GetReceptionManager123(entity, out rows, out pagecount, out NeedAllPrice, out Price, out XianJin, out ShuaKa, out DiscountAmount, out SendAmount, out Huachika);
            var AddList = new List<OrderinfoEntity>();
            var DelList = new List<OrderinfoEntity>();

            foreach (var newinfo in list)
            {
                var info = newinfo;

                if (info.InfoBuyType == 10)
                {
                    var cardmodel = cashBll.GetUserCardModel(info.ProdcuitID);
                    if (newinfo.Price != 0)
                    {
                        newinfo.ProdcuitName = cardmodel.CardName;
                        newinfo.ProdcuitName1 = cardmodel.CardName;
                    }
                    else
                    {
                        newinfo.ProdcuitName = "手续服务费";
                        newinfo.ProdcuitName1 = "手续服务费";
                    }

                }

                if (info.Type == 3 && info.IsChongzhi != 1)
                {
                    ProjectProductEntity pp = new ProjectProductEntity();
                    pp.ProgrammeID = newinfo.ProdcuitID;

                    string[] idlist = info.CardBalanceID == null ? null : info.CardBalanceID.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (idlist != null)
                    {
                        foreach (var id in idlist)
                        {
                            var cardbalanc = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = Convert.ToInt32(id) });

                            var Add = new OrderinfoEntity();
                            Add.CardType = newinfo.CardType;
                            Add.CreateTime = newinfo.CreateTime;
                            Add.JoinUserName = newinfo.JoinUserName;
                            Add.OrderType = newinfo.OrderType;
                            Add.Quantity = 1;    //储值卡在数据库用户余额里面里面是没有次数的,但是现在要默认显示为1,现在默认为1cardbalanc.AllTiems;
                            Add.Type = newinfo.Type;
                            Add.AllPrice = cardbalanc.AllPrice;
                            Add.ProdcuitName1 = cardbalanc.ProdcuitName;
                            Add.ProdcuitName = cardbalanc.ProdcuitName;
                            Add.DocumentNumber = info.DocumentNumber;
                            Add.Price = cardbalanc.AllPrice;
                            Add.RetailPrice = cardbalanc.RetailPrice;
                            Add.OrderNo = info.OrderNo;
                            Add.UserName = info.UserName;
                            Add.IsGive = cardbalanc.IsGive;
                            Add.InfoBuyType = newinfo.InfoBuyType;
                            Add.OldPrice = newinfo.OldPrice;
                            Add.HospitalName = newinfo.HospitalName;
                            if (cardbalanc.Type == 2)
                            {
                                Add.Quantity = cardbalanc.AllTiems;
                                Add.Price = cardbalanc.Price;
                                Add.AllPrice = cardbalanc.Price * cardbalanc.AllTiems;
                            }
                            if (cardbalanc.IsGive == 1)
                            {
                                Add.AllPrice = 0;
                                Add.Price = 0;
                            }
                            AddList.Add(Add);
                        }
                        DelList.Add(newinfo);
                    }
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
                            newname = newname + dyinfo.ProdcuitName1 + ",";
                        }
                    }

                    info.ProdcuitName = newname;

                }
                //退卡操作
                if (newinfo.OrderType == 5)
                {
                    newinfo.Quantity = -newinfo.Quantity;
                    newinfo.AllPrice = -newinfo.AllPrice;
                }

                if (newinfo.OrderType == 1 || newinfo.OrderType == 2)
                {
                    if (newinfo.Price < 0)
                    {
                        newinfo.Price = System.Math.Abs(newinfo.Price);
                    }
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



            list = list.OrderBy(i => i.DocumentNumber).ToList();
            //疗程卡
            if (entity.OrderType == 6 && !string.IsNullOrEmpty(newcardname))
            {
                list = list.Where(c => c.ProdcuitName1 == newcardname || c.ProdcuitName == newcardname).ToList();
            }
            string oldorderno = "";
            string oldshijian = "";
            string olddanju = "";


            decimal shuliang = 0;
            decimal zonge = 0;
            decimal xianjinzr = 0;
            decimal yinliankazr = 0;
            decimal chuzhikazr = 0;
            decimal zengkazr = 0;
            decimal qiankuan = 0;
            decimal huakazr = 0;
            decimal jf = 0;
            List<int> tempList = new List<int>();

            //组装SQL
            foreach (var info in list)
            {
                if (!tempList.Contains(info.UserID))
                    tempList.Add(info.UserID);

                StringBuilder str = new StringBuilder();
                var paymentlist = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = info.OrderNo });
                var Xianjin = paymentlist.Where(t => t.ParentPayType == 1).Count() > 0 ? paymentlist.Where(t => t.ParentPayType == 1).Sum(c => c.PayMoney) : 0;
                var Shuaka = paymentlist.Where(t => t.ParentPayType == 2).Count() > 0 ? paymentlist.Where(t => t.ParentPayType == 2).Sum(c => c.PayMoney) : 0;
                var HuaKa = paymentlist.Where(t => t.PayType == 4).Count() > 0 ? paymentlist.Where(t => t.PayType == 4).ToList().Sum(i => i.PayMoney) : 0;
                var Jifen = paymentlist.Where(t => t.PayType == 8).Count() > 0 ? paymentlist.Where(t => t.PayType == 8).ToList().Sum(i => i.PayMoney) : 0;

                var shijian = list.Where(t => t.CreateTime.ToShortDateString() == info.CreateTime.ToShortDateString()).Count() > 0 ? list.Where(t => t.CreateTime.ToShortDateString() == info.CreateTime.ToShortDateString()).Count() : 0;
                var danju = list.Where(t => t.OrderNo == info.OrderNo).Count() > 0 ? list.Where(t => t.OrderNo == info.OrderNo).Count() : 0;
                var count = list.Where(t => t.OrderNo == info.OrderNo).Count();
                var zengka = paymentlist.Where(t => t.PayType == 3 && t.IsGive == 1).ToList().Sum(i => i.PayMoney);
                var chuzhika = paymentlist.Where(t => t.PayType == 3).ToList().Where(t => t.IsGive != 1).Sum(i => i.PayMoney);

                if (oldshijian != info.CreateTime.ToShortDateString())
                {
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", shijian, info.CreateTime.ToString("yyyy-MM-dd"));
                    oldshijian = info.CreateTime.ToShortDateString();
                }
                str.AppendFormat("<td>{0}</td>", info.UserName);
                str.AppendFormat("<td>{0}</td>", info.HospitalName);
                if (olddanju != info.OrderNo)
                {
                    str.AppendFormat("<td rowspan='{0}'><a href='javascript:OpenOrderDetail(\"{2}\")' > {1}</a></td>", danju, info.DocumentNumber, info.OrderNo);
                    olddanju = info.OrderNo;
                }
                str.AppendFormat("<td>{0}</td>", info.GetInfoBuyTyoeBy5Lei());
                str.AppendFormat("<td>{0}</td>", info.BrandName);
                str.AppendFormat("<td>{0}</td>", info.ProductTypeName);
                str.AppendFormat("<td>{0}</td>", info.ProdcuitName == null ? info.ProdcuitName1 : info.ProdcuitName);


                str.AppendFormat("<td>{0}</td>", Math.Round(info.Price, 2).ToString());
                str.AppendFormat("<td>{0}</td>", (info.AllPrice / (info.Quantity == 0 ? 1 : info.Quantity)).ToString("#0.00"));
                str.AppendFormat("<td>{0}</td>", info.Quantity);
                shuliang = shuliang + info.Quantity;
                str.AppendFormat("<td>{0}</td>", info.AllPrice.ToString("#0.00"));
                zonge = zonge + info.AllPrice;
                if (oldorderno != info.OrderNo)
                {
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, Xianjin);
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, Shuaka);
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, HuaKa);

                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, chuzhika);
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, zengka);
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, info.OldPrice);
                    if (LoginUserEntity.HospitalID == 1017 || LoginUserEntity.HospitalID == 2)
                    {
                        if(paymentlist.Count>0)
                        {
                            var dd = paymentlist.Where(t => t.PayType == 8).ToList();
                            if (dd.Count>0)
                            {
                                str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, Jifen);
                            }
                            else
                            {
                                str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, "0.00");
                            }
                        }

                        else
                        {
                            // str.AppendFormat("<td>0</td>");
                            str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, "0.00");
                        }
                    }
                    oldorderno = info.OrderNo;
                    xianjinzr = xianjinzr + Xianjin;
                    yinliankazr = yinliankazr + Shuaka;
                    chuzhikazr += chuzhika;
                    zengkazr += zengka;
                    qiankuan += info.OldPrice;
                    huakazr += HuaKa;
                    jf += Jifen;

                }
                info.ids = str.ToString();
            }
            //  ViewState["dd"]= shuliang;


          var result = list.AsQueryable().ToPagedList(entity.pageNum, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.AllCount = rows;
            ViewBag.sl = shuliang;
            ViewBag.ze = zonge;
            ViewBag.xj = xianjinzr;
            ViewBag.ylk = yinliankazr;
            ViewBag.czk = chuzhikazr;
            ViewBag.zk = zengkazr;
            ViewBag.ct = tempList.Count();
            ViewBag.kq = qiankuan;
            ViewBag.hk = huakazr;
            ViewBag.Jifen = jf;
            if (Request.IsAjaxRequest())
                return PartialView("_ReceptionList", result);
            return View(result);




            //if (entity.HospitalID == 0) entity.HospitalID = loginUserEntity.HospitalID;
            //ViewBag.ParentHospitalID = loginUserEntity.ParentHospitalID;
            //ViewBag.HospitalID = loginUserEntity.HospitalID;
            //entity.STime = entity.STime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.STime.ToString("yyyy-MM-dd 00:00:01"));
            //entity.ETime = entity.ETime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.ETime.ToString("yyyy-MM-dd 23:59:59"));
            //if (!Request.IsAjaxRequest())
            //{
            //    return View(new List<OrderinfoEntity>().AsQueryable().ToPagedList(1, 10));
            //}
            //int rows;
            //int pagecount;
            //entity.numPerPage = 50; //每页显示条数
            //string newcardname = entity.CardName;
            //if (entity.OrderType == 6)
            //{
            //    entity.CardName = "";
            //    entity.numPerPage = 1000;
            //}
            //if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            //decimal NeedAllPrice, Price, XianJin, ShuaKa, DiscountAmount, SendAmount, Huachika = 0;
            //if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }
            //var list = cashBll.GetReceptionManager(entity, out rows, out pagecount, out  NeedAllPrice, out  Price, out  XianJin, out  ShuaKa, out  DiscountAmount, out  SendAmount, out  Huachika);
            //var AddList = new List<OrderinfoEntity>();
            //var DelList = new List<OrderinfoEntity>();

            //foreach (var newinfo in list)
            //{
            //    var info = newinfo;

            //    if (info.InfoBuyType == 10)
            //    {
            //        var cardmodel = cashBll.GetUserCardModel(info.ProdcuitID);
            //        if (newinfo.Price != 0)
            //        {
            //            newinfo.ProdcuitName = cardmodel.CardName;
            //            newinfo.ProdcuitName1 = cardmodel.CardName;
            //        }
            //        else
            //        {
            //            newinfo.ProdcuitName = "手续服务费";
            //            newinfo.ProdcuitName1 = "手续服务费";
            //        }

            //    }

            //    if (info.Type == 3 && info.IsChongzhi != 1)
            //    {
            //        ProjectProductEntity pp = new ProjectProductEntity();
            //        pp.ProgrammeID = newinfo.ProdcuitID;

            //        string[] idlist = info.CardBalanceID == null ? null : info.CardBalanceID.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //        if (idlist != null)
            //        {
            //            foreach (var id in idlist)
            //            {
            //                var cardbalanc = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = Convert.ToInt32(id) });

            //                var Add = new OrderinfoEntity();
            //                Add.CardType = newinfo.CardType;
            //                Add.CreateTime = newinfo.CreateTime;
            //                Add.JoinUserName = newinfo.JoinUserName;
            //                Add.OrderType = newinfo.OrderType;
            //                Add.Quantity = 1;    //储值卡在数据库用户余额里面里面是没有次数的,但是现在要默认显示为1,现在默认为1cardbalanc.AllTiems;
            //                Add.Type = newinfo.Type;
            //                Add.AllPrice = cardbalanc.AllPrice;
            //                Add.ProdcuitName1 = cardbalanc.ProdcuitName;
            //                Add.ProdcuitName = cardbalanc.ProdcuitName;
            //                Add.DocumentNumber = info.DocumentNumber;
            //                Add.Price = cardbalanc.AllPrice;
            //                Add.RetailPrice = cardbalanc.RetailPrice;
            //                Add.OrderNo = info.OrderNo;
            //                Add.UserName = info.UserName;
            //                Add.IsGive = cardbalanc.IsGive;
            //                Add.InfoBuyType = newinfo.InfoBuyType;
            //                Add.OldPrice = newinfo.OldPrice;
            //                Add.HospitalName = newinfo.HospitalName;
            //                if (cardbalanc.Type == 2)
            //                {
            //                    Add.Quantity = cardbalanc.AllTiems;
            //                    Add.Price = cardbalanc.Price;
            //                    Add.AllPrice = cardbalanc.Price * cardbalanc.AllTiems;
            //                }
            //                if (cardbalanc.IsGive == 1)
            //                {
            //                    Add.AllPrice = 0;
            //                    Add.Price = 0;
            //                }
            //                AddList.Add(Add);
            //            }
            //            DelList.Add(newinfo);
            //        }
            //    }

            //    if (newinfo.OrderType == 4) //如果是还款操作,给ProdcuitName字段重新赋值
            //    {
            //        string orderno = null;
            //        string newname = null;
            //        var cxlist = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = newinfo.OrderNo });     //如果是还款操作,用自身的订单编号获取订单详情,从而获取订单详情下面的对应的订单号
            //        foreach (var cxinfo in cxlist)
            //        {
            //            if (cxinfo.ProdcuitName != null)
            //            {
            //                orderno = cxinfo.ProdcuitName;
            //            }
            //        }
            //        if (orderno != null)
            //        {
            //            var dylist = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = orderno });
            //            foreach (var dyinfo in dylist)
            //            {
            //                newname = newname + dyinfo.ProdcuitName1 + ",";
            //            }
            //        }

            //        info.ProdcuitName = newname;

            //    }
            //    //退卡操作
            //    if (newinfo.OrderType == 5)
            //    {
            //        newinfo.Quantity = -newinfo.Quantity;
            //        newinfo.AllPrice = -newinfo.AllPrice;
            //    }

            //    if (newinfo.OrderType == 1 || newinfo.OrderType == 2)
            //    {
            //        if (newinfo.Price < 0)
            //        {
            //            newinfo.Price = System.Math.Abs(newinfo.Price);
            //        }
            //    }

            //}

            //foreach (var Ainfo in AddList)
            //{
            //    list.Add(Ainfo);
            //}

            //foreach (var Dinfo in DelList)
            //{
            //    list.Remove(Dinfo);
            //}



            //list = list.OrderBy(i => i.DocumentNumber).ToList();
            ////疗程卡
            //if (entity.OrderType == 6 && !string.IsNullOrEmpty(newcardname))
            //{
            //    list = list.Where(c => c.ProdcuitName1 == newcardname || c.ProdcuitName == newcardname).ToList();
            //}
            //string oldorderno = "";
            //string oldshijian = "";
            //string olddanju = "";


            //decimal shuliang = 0;
            //decimal zonge = 0;
            //decimal xianjinzr = 0;
            //decimal yinliankazr = 0;
            //decimal chuzhikazr = 0;
            //decimal zengkazr = 0;
            //decimal qiankuan = 0;
            //decimal huakazr = 0;

            //List<int> tempList = new List<int>();

            ////组装SQL
            //foreach (var info in list)
            //{
            //    if (!tempList.Contains(info.UserID))
            //        tempList.Add(info.UserID);

            //    StringBuilder str = new StringBuilder();
            //    var paymentlist = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = info.OrderNo });
            //    var Xianjin = paymentlist.Where(t => t.PayType == 1).Count() > 0 ? paymentlist.Where(t => t.PayType == 1).First().PayMoney : 0;
            //    var Shuaka = paymentlist.Where(t => t.PayType == 2).Count() > 0 ? paymentlist.Where(t => t.PayType == 2).First().PayMoney : 0;
            //    var HuaKa = paymentlist.Where(t => t.PayType == 4).Count() > 0 ? paymentlist.Where(t => t.PayType == 4).ToList().Sum(i => i.PayMoney) : 0;


            //    var shijian = list.Where(t => t.CreateTime.ToShortDateString() == info.CreateTime.ToShortDateString()).Count() > 0 ? list.Where(t => t.CreateTime.ToShortDateString() == info.CreateTime.ToShortDateString()).Count() : 0;
            //    var danju = list.Where(t => t.OrderNo == info.OrderNo).Count() > 0 ? list.Where(t => t.OrderNo == info.OrderNo).Count() : 0;
            //    var count = list.Where(t => t.OrderNo == info.OrderNo).Count();
            //    var zengka = paymentlist.Where(t => t.PayType == 3 && t.IsGive == 1).ToList().Sum(i => i.PayMoney);
            //    var chuzhika = paymentlist.Where(t => t.PayType == 3).ToList().Where(t => t.IsGive != 1).Sum(i => i.PayMoney);

            //    if (oldshijian != info.CreateTime.ToShortDateString())
            //    {
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", shijian, info.CreateTime.ToString("yyyy-MM-dd"));
            //        oldshijian = info.CreateTime.ToShortDateString();
            //    }
            //    str.AppendFormat("<td>{0}</td>", info.UserName);
            //    str.AppendFormat("<td>{0}</td>", info.HospitalName);
            //    if (olddanju != info.OrderNo)
            //    {
            //        str.AppendFormat("<td rowspan='{0}'><a onclick='OpenOrderDetail(\"{2}\")' > {1}</a></td>", danju, info.DocumentNumber, info.OrderNo);
            //        olddanju = info.OrderNo;
            //    }
            //    str.AppendFormat("<td>{0}</td>", info.GetInfoBuyTyoeBy5Lei());
            //    str.AppendFormat("<td>{0}</td>", info.BrandName);
            //    str.AppendFormat("<td>{0}</td>", info.ProductTypeName);
            //    str.AppendFormat("<td>{0}</td>", info.ProdcuitName == null ? info.ProdcuitName1 : info.ProdcuitName);


            //    str.AppendFormat("<td>{0}</td>", Math.Round(info.Price, 2).ToString());
            //    str.AppendFormat("<td>{0}</td>", (info.AllPrice / (info.Quantity == 0 ? 1 : info.Quantity)).ToString("#0.00"));
            //    str.AppendFormat("<td>{0}</td>", info.Quantity);
            //    shuliang = shuliang + info.Quantity;
            //    str.AppendFormat("<td>{0}</td>", info.AllPrice.ToString("#0.00"));
            //    zonge = zonge + info.AllPrice;
            //    if (oldorderno != info.OrderNo)
            //    {
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, Xianjin);
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, Shuaka);
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, HuaKa);

            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, chuzhika);
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, zengka);
            //        str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, info.OldPrice);
            //        oldorderno = info.OrderNo;
            //        xianjinzr = xianjinzr + Xianjin;
            //        yinliankazr = yinliankazr + Shuaka;
            //        chuzhikazr += chuzhika;
            //        zengkazr += zengka;
            //        qiankuan += info.OldPrice;
            //        huakazr += HuaKa;

            //    }
            //    info.ids = str.ToString();
            //}



            //var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            //result.TotalItemCount = rows;
            //result.CurrentPageIndex = entity.pageNum;
            //ViewBag.orderField = entity.orderField;
            //ViewBag.orderDirection = entity.orderDirection;
            //ViewBag.sl = shuliang;
            //ViewBag.ze = zonge;
            //ViewBag.xj = xianjinzr;
            //ViewBag.ylk = yinliankazr;
            //ViewBag.czk = chuzhikazr;
            //ViewBag.zk = zengkazr;
            //ViewBag.ct = tempList.Count();
            //ViewBag.kq = qiankuan;
            //ViewBag.hk = huakazr;
            //ViewBag.NeedAllPrice = NeedAllPrice;
            //ViewBag.Price = Price;
            //ViewBag.XianJin = XianJin;
            //ViewBag.ShuaKa = ShuaKa;
            //ViewBag.DiscountAmount = DiscountAmount;
            //ViewBag.SendAmount = SendAmount;
            //ViewBag.Huachika = Huachika;
            //if (Request.IsAjaxRequest())
            //    return PartialView("_ReceptionList", result);
            //return View(result);


        }
        //获取项目或者产品的分类
        public ActionResult GetProductTypeNoPage(int Type)
        {
            var list = iwareBLL.GetProductTypeNoPage(LoginUserEntity.ParentHospitalID, Type);
            StringBuilder str = new StringBuilder();
            str.AppendFormat("<option value='0'>=选择分类=</option>");
            foreach (var info in list)
            {
                str.AppendFormat("<option value='{0}'>{1}</option>", info.ID, info.Name);
            }
            return Content(str.ToString());
        }
        public ActionResult _ReceptionList()
        {
            return View();
        }

        #endregion

        #region ==用户余额==
        public ActionResult CardBalanceManager(MainCardBalanceEntity entity)
        {
            int rows;
            int pagecount;
            decimal sumcard;
            decimal sumconsumer;
            decimal Zhexian;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProdcuitName,CardName"; }
            if (entity.HospitalID == 0) entity.HospitalID = LoginUserEntity.HospitalID;
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;

            var list = IpatBLL.GetCardBalanceByHospitalId(entity, out rows, out pagecount, out sumcard, out sumconsumer, out Zhexian);
            list = list.Where(t=>t.CardName!= "积分消费").ToList();
            list = list.Where(t=>t.IsDel!=3).ToList();
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount =  rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.AllCount = rows;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.sumcard = sumcard;
            ViewBag.sumconsumer = sumconsumer;
            ViewBag.Zhexian = Zhexian;
            ViewBag.Type = entity.Type;
            if (Request.IsAjaxRequest())
                return PartialView("_CardBalanceManager", result);
            return View(result);
        }

        public ActionResult _CardBalanceManager()
        {
            return View();
        }

        #endregion

        #region==数据初始化==
        /// <summary>
        /// 数据清空
        /// </summary>
        /// <returns></returns>
        public ActionResult DataClear()
        {
            return View();
        }

        #region ==仓库初始化==
        /// <summary>
        /// 仓库清理实现方法
        /// </summary>
        /// <returns></returns>
        [WriteOperateLogAttribute("清理仓库数据，包括盘点单，采购单，入库单等数据", "清理仓库数据", "数据初始化")]
        public ActionResult ClearWarnhouse()
        {
            var result = "";
            //清除盘点单
            result = result + ClearCheckOrderInfoAndCheckOrd();
            //清除采购单
            result = result + ClearBuyOrderAndInfo();
            //清除出入库数据
            result = result + ClearStockOrderAndInfo();
            //清除仓库和缓存
            result = result + ClearHouseInfo();
            return Json(result);
        }
        /// <summary>
        /// 清除盘点单和盘点详情
        /// </summary>
        /// <returns></returns>
        public string ClearCheckOrderInfoAndCheckOrd()
        {
            int infocount = 0;
            int count = 0;
            CheckOrderEntity entity = new CheckOrderEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.StartTime = DateTime.Now.AddYears(-20);
            entity.EndTime = DateTime.Now.AddYears(1);
            entity.numPerPage = 10000000;
            int row = 0;
            int pagecount = 0;
            ///先查出所有的盘点单据列表
            var pandianlist = iwareBLL.GetCheckOrderList(entity, out row, out pagecount);
            foreach (var info in pandianlist)
            {
                var c = iwareBLL.DelStockOrderInfoByOrderNo(info.OrderNo);//更具盘点id删除 盘点详情
                infocount = infocount + c;
                var co = iwareBLL.DelCheckOrder(info.ID);
                count = count + co;
            }

            return "删除盘点单:" + count + "条,删除盘点详细:" + infocount + "条;";
        }

        /// <summary>
        /// 清除采购单和详情
        /// </summary>
        /// <returns></returns>
        public string ClearBuyOrderAndInfo()
        {
            int count = 0;
            int infocount = 0;
            BuyOrderEntity entity = new BuyOrderEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.StartTime = DateTime.Now.AddYears(-20);
            entity.EndTime = DateTime.Now.AddDays(1);
            entity.numPerPage = 10000000;
            int row = 0;
            int pagecount = 0;
            //根据 仓库获取所有的采购单据
            var caigoulist = iwareBLL.GetBuyOrderList(entity, out row, out pagecount);
            foreach (var info in caigoulist)
            {
                var ic = iwareBLL.DelBuyOrderInfo(info.OrderNo);
                infocount = infocount + ic;
                var c = iwareBLL.DelBuyOrder(info.ID);
                count = count + c;
            }

            return "删除出入数据:" + count + "条,删除出入详情:" + infocount + "条;";

        }

        /// <summary>
        /// 删除出入库单据和详情
        /// </summary>
        /// <returns></returns>
        public string ClearStockOrderAndInfo()
        {
            int count = 0;
            int infocount = 0;
            StockOrderEntity entity = new StockOrderEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.StartTime = DateTime.Now.AddYears(-20);
            entity.EndTime = DateTime.Now.AddDays(1);
            entity.numPerPage = 10000000;
            int row = 0;
            int pagecount = 0;
            //根据 仓库获取所有的采购单据
            var caigoulist = iwareBLL.GetStockOrderList(entity, out row, out pagecount);
            foreach (var info in caigoulist)
            {
                var ic = iwareBLL.DelStockOrderInfoByOrderNo(info.OrderNo);
                infocount = infocount + ic;
                var c = iwareBLL.DelStockOrder(info.ID);
                count = count + c;
            }

            return "删除出入数据:" + count + "条,删除出入详情:" + infocount + "条;";


        }
        /// <summary>
        /// 默认仓库和清除库存
        /// </summary>
        /// <returns></returns>
        public string ClearHouseInfo()
        {
            int csh = 0;
            //删除库存
            int delcount = iwareBLL.DelProductStockByHospitalID(LoginUserEntity.HospitalID);
            var houselist = iwareBLL.GetWarehouseList(new WarehouseEntity { HospitalID = LoginUserEntity.HospitalID });
            foreach (var info in houselist)
            {
                //初始化仓库
                int chushihua = iwareBLL.InitializeWarehosue(info.ID, LoginUserEntity.Name + "仓库");
                if (chushihua < 1)
                {
                    csh = 1;
                }
            }
            if (csh == 1)
            {
                return "部分仓库初始化失败,库存清除:" + delcount + "条";
            }
            else
            {
                return "仓库初始化成功,库存清除:" + delcount + "条";
            }
        }
        #endregion

        #region ==会员初始化==
        [WriteOperateLogAttribute("清理会员数据，包括积分、用户余额、卡项、顾客等数据", "清理会员数据", "数据初始化")]
        public ActionResult ClearPatient()
        {
            var result = "";
            PatientEntity entity = new PatientEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 100000000;
            int row = 0;
            int pagecount = 0;
            entity.s_StareTime = Convert.ToDateTime("1970-01-01");
            entity.s_Endtime = Convert.ToDateTime("1970-01-01");
            //获取用户列表
            var list = IpatBLL.GetPatientList(entity, out row, out pagecount);
            //清空积分记录
            result = result + ClearIntegralRecord(list);
            // 清空积分
            result = result + CleaarIntegral();
            //清空余额记录
            result = result + ClearMainCardBalance(list);
            //清空卡关系
            result = result + ClearUserCard(list);
            //删除顾客
            result = result + ClearUser(list);
            return Json(result);
        }

        /// <summary>
        /// 清空积分记录
        /// </summary>
        /// <returns></returns>
        public string ClearIntegralRecord(IList<PatientEntity> list)
        {
            int count = 0;
            foreach (var info in list)
            {
                int c = IpatBLL.DelIntegralRecord(info.UserID);
                count = count + c;
            }
            return "清空积分记录:" + count + "条;";
        }

        /// <summary>
        /// 清空客户积分
        /// </summary>
        /// <returns></returns>
        public string CleaarIntegral()
        {
            int c = IpatBLL.DelIntegral(new IntegralEntity { HospitalID = LoginUserEntity.HospitalID });
            return "清空客户积分:" + c + "条;";
        }

        /// <summary>
        /// 清空会员余额记录
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public string ClearMainCardBalance(IList<PatientEntity> list)
        {
            int count = 0;
            foreach (var info in list)
            {
                //删除用户余额记录
                int c = cashBll.DelMainCardBalanceByUserID(info.UserID);
                count = count + c;
            }
            return "清空记录:" + count + "条";
        }

        /// <summary>
        /// 清除储值卡关系
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public string ClearUserCard(IList<PatientEntity> list)
        {
            int count = 0;
            foreach (var info in list)
            {
                UserCardEntity entity = new UserCardEntity();
                entity.UserID = info.UserID;
                entity.numPerPage = 10000000;
                int row = 0;
                int pagecount = 0;
                //获取当前用户的储值卡关系
                var uclist = cashBll.GetAllUserCard(entity, out row, out pagecount);
                foreach (var ucinfo in uclist)
                {
                    int c = cashBll.DelProduct(ucinfo.ID);
                    count = count + c;
                }
            }
            return "清除购买关系:" + count + "条;";
        }

        /// <summary>
        /// 删除会员
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public string ClearUser(IList<PatientEntity> list)
        {
            int count = 0;
            foreach (var info in list)
            {
                var c = IpatBLL.DelPatient(info.UserID);
                count = count + c;
            }
            return "删除会员数据:" + count + "条;";
        }

        #endregion

        #region ==员工数据初始化==
        [WriteOperateLogAttribute("清理员工数据，包括员工相关数据", "清理员工数据", "数据初始化")]
        public ActionResult ClearMember()
        {
            var result = "";
            HospitalUserEntity entity = new HospitalUserEntity();
            entity.DepartmentID = 0;
            entity.HospitalID = LoginUserEntity.HospitalID;
            //获取员工列表
            IList<HospitalUserEntity> list = personnelBLL.GetAllUser(entity);
            result = result + ClearHospitalUser();
            result = result + ClearIntegralOperation();
            result = result + ClearScheduling();
            result = result + ClearHospitalTarget();
            result = result + ClearUserbase(list);
            return Json(result);
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public string ClearHospitalUser()
        {
            var c = personnelBLL.DelHospitalUsre(LoginUserEntity.HospitalID);
            return "删除用户数据:" + c + "条;";
        }

        /// <summary>
        /// 删除员工积分
        /// </summary>
        /// <returns></returns>
        public string ClearIntegralOperation()
        {
            var c = IpatBLL.DelIntegral(new IntegralEntity { HospitalID = LoginUserEntity.HospitalID });
            return "删除员工积分:" + c + "条";
        }

        /// <summary>
        /// 删除员工排班表
        /// </summary>
        /// <returns></returns>
        public string ClearScheduling()
        {
            var c = personnelBLL.DelScheduling(LoginUserEntity.HospitalID);
            return "删除员工排班记录:" + c + "条";
        }
        /// <summary>
        ///   删除员工目标计划
        /// </summary>
        /// <returns></returns>
        public string ClearHospitalTarget()
        {
            int count = 0;
            int row = 0;
            int pagecount = 0;
            HospitalTargetEntity entity = new HospitalTargetEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 100000000;
            //获取员工计划
            var list = personnelBLL.GetHospitalTargetList(entity, out row, out pagecount);
            foreach (var info in list)
            {
                int c = personnelBLL.DelHospitalTarget(info.ID);
                count = count + c;
            }
            return "删除员工目标计划:" + count + "条;";
        }

        /// <summary>
        /// 删除用户基本信息(userbase登录信息)
        /// </summary>
        /// <returns></returns>
        public string ClearUserbase(IList<HospitalUserEntity> list)
        {
            int count = 0;
            foreach (var info in list)
            {
                var c = personnelBLL.DelUserBase(info.UserID);
                count = count + c;
            }
            return "删除基础信息:" + count + "条;";
        }

        #endregion

        #region ===业务单据初始化==
        /// <summary>
        /// 需要先删除员工业绩和订单支付方式才能删除订单详情
        ///
        /// </summary>
        /// <returns></returns>
        [WriteOperateLogAttribute("清理业务数据，包括订单支付方式、参与员工、订单详情，订单数据、预约相关数据", "清理业务数据", "数据初始化")]
        public ActionResult ClearBusiness()
        {
            var result = "";
            decimal qianPrice = 0;
            OrderEntity entity = new OrderEntity();
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.numPerPage = 10000000;
            var list = cashBll.GetAllOrder(entity, out qianPrice);
            result = result + ClearPaymentOrderProduct(list);
            result = result + ClearJoinUser(list);
            result = result + ClearOrderInfo(list);
            result = result + ClearPaymentOrder(list);
            result = result + ClearOrder(list);
            result = result + ClearReservation();
            return Json(result);
        }

        /// <summary>
        /// 删除订单产品支付方式
        /// </summary>
        /// <returns></returns>
        public string ClearPaymentOrderProduct(IList<OrderEntity> list)
        {
            int count = 0;
            foreach (var info in list)
            {
                //获取订单详情列表
                var orderinfolist = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = info.OrderNo });
                foreach (var iinfo in orderinfolist)
                {
                    //获取当前订单详情下的订单支付方式
                    var jslist = cashBll.GetAllPaymentOrderProduct(new PaymentOrderProductEntity { OrderInfoID = iinfo.ID });
                    foreach (var jsinfo in jslist)
                    {
                        //删除当前支付方式列表数据
                        int c = cashBll.DelPaymentOrderProduct(jsinfo.ID);
                        count = count + c;
                    }

                }

            }

            return "删除订单支付方式:" + count + "条;";
        }

        /// <summary>
        /// 删除员工业绩
        /// </summary>
        /// <returns></returns>
        public string ClearJoinUser(IList<OrderEntity> list)
        {
            int count = 0;
            foreach (var info in list)
            {
                int c = cashBll.DelJoinUserByOrderNO(info.OrderNo);
                count = count + c;
            }
            return "删除员工业绩:" + count + "条;";
        }


        /// <summary>
        /// 清除订单详情
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public string ClearOrderInfo(IList<OrderEntity> list)
        {
            int count = 0;
            foreach (var info in list)
            {
                int c = cashBll.DeleteOrderInfo(info.OrderNo);
                count = count + c;
            }
            return "删除单据详情:" + count + "条;";
        }

        /// <summary>
        /// 清除订单结算表数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public string ClearPaymentOrder(IList<OrderEntity> list)
        {
            int count = 0;
            foreach (var info in list)
            {
                int c = cashBll.DelPaymentOrderByOrderNo(info.OrderNo);
                count = count + c;
            }
            return "删除订单结算数据:" + count + "条;";

        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public string ClearOrder(IList<OrderEntity> list)
        {
            int count = 0;
            foreach (var info in list)
            {
                int c = cashBll.DeleteOrder(info.OrderNo);
            }
            return "删除订单:" + count + "条;";
        }

        /// <summary>
        /// 删除预约信息
        /// </summary>
        /// <returns></returns>
        public string ClearReservation()
        {
            int count = ResDocBLL.DelReservationByHospitalID(LoginUserEntity.HospitalID);
            return "删除预约信息:" + count + "条;";
        }


        #endregion

        //#region ==基础信息初始化==
        //public ActionResult ClearBaseInfo()
        //{

        //}

        /////// <summary>
        /////// 删除卡类
        /////// </summary>
        /////// <returns></returns>
        ////public string ClearCard()
        ////{
        ////    int count = baseinfoBLL.DelCardByHospitalID(loginUserEntity.HospitalID);
        ////    return "删除卡类:" + count + "张;";
        ////}

        /////// <summary>
        /////// 清空产品关系
        /////// </summary>
        /////// <returns></returns>
        ////public string ClearProjectProduct()
        ////{
        ////    int count = 0;
        ////    int row=0;
        ////    int pagecount=0;
        ////    MainCardEntity entity=new MainCardEntity();
        ////    entity.HospitalID=loginUserEntity.HospitalID;
        ////    entity.numPerPage=10000000;
        ////    //获取卡项列表
        ////    var list = baseinfoBLL.GetAllPrePayCard(entity, out row, out pagecount);
        ////    foreach (var info in list)
        ////    {
        ////        //获取关系列表
        ////        var gxlist = cashBll.getProjectProductNopage(new ProjectProductEntity { ProgrammeID = info.ID });
        ////        foreach (var gxinfo in gxlist)
        ////        {
        ////            //删除关系
        ////            int c = cashBll.DelProjectProduct(gxinfo.ID);
        ////            count = count + c;
        ////        }
        ////    }
        ////    return "删除产品关系:" + count + "条;";
        ////}
        /////// <summary>
        /////// 清空产品
        /////// </summary>
        /////// <returns></returns>
        ////public string ClearProduct()
        ////{

        ////}






        //#endregion

        #endregion

        #region==业务销售明细==
        /// <summary>
        /// 销售明细
        /// </summary>
        /// <returns></returns>
        public ActionResult SalesDetail()
        {
            return View();
        }

        /// <summary>
        /// 储值卡销售明细
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalesDetailPage_ChuzhiKa(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderinfoEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }

            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            int Quatity;
            decimal SumPrice;
            decimal ZengSong;
            decimal ZheRang;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            entity.numPerPage = 1000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var list = cashBll.GetSalesChuzhika(entity, out rows, out pagecount, out Quatity, out SumPrice, out ZengSong, out ZheRang);



            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.Quatity = Quatity;
            ViewBag.SumPrice = SumPrice;
            ViewBag.ZengSong = ZengSong;
            ViewBag.ZheRang = ZheRang;


            if (Request.IsAjaxRequest())
                return PartialView("_Sale_ChuzhiKa", result);
            return View(result);

        }

        /// <summary>
        /// 获取疗程卡和项目销售明细
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalesDetailPage_Liaocheng(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderinfoEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            int Quatity;
            decimal SumPrice;
            decimal ZengSong;
            decimal ZheRang;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            entity.numPerPage = 99999999; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "AddTime"; }

            var list = cashBll.GetSalesLiaocheng(entity, out rows, out pagecount, out Quatity, out SumPrice, out ZengSong, out ZheRang);
            var result = list.AsQueryable().ToPagedList(1, 10000000);

            //var ShowList = new  List<OrderinfoEntity>();

            //foreach (var info in list)
            //{
            //    GetNewList(info, ref ShowList);
            //}

            //var result = ShowList.AsQueryable().ToPagedList(1, 10000000);
            //result.TotalItemCount = rows;
            //result.CurrentPageIndex = entity.pageNum;

            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.Quatity = Quatity;
            ViewBag.SumPrice = SumPrice;
            ViewBag.ZengSong = ZengSong;
            ViewBag.ZheRang = ZheRang;
            if (Request.IsAjaxRequest())
                return PartialView("_Sale_Liaocheng", result);
            return View(result);
        }

        /// <summary>
        /// 业务销售明细---原来疗程转新疗程
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="nl"></param>
        private void GetNewList(OrderinfoEntity entity, ref List<OrderinfoEntity> nl)
        {
            if (!string.IsNullOrEmpty(entity.CardBalanceID))
            {
                var list = entity.CardBalanceID.Split(',');
                if (list.Count() > 0)
                {
                    foreach (var info in list)
                    {
                        if (!string.IsNullOrEmpty(info) && HS.SupportComponents.ConvertValueHelper.ConvertIntValue(info) > 0)
                        {
                            var CardBalance = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(info) });
                            var project = baseinfoBLL.GetProjectModel(CardBalance.ProjectID);//获取产品详情
                            entity.Quantity = CardBalance.AllTiems;
                            entity.Price = CardBalance.Price;
                            entity.OldPrice = project.RetailPrice;
                            entity.AllPrice = entity.Price * entity.Quantity;
                            if (entity.Price == 0)
                            {
                                entity.SendAmount = entity.OldPrice;
                            }
                            entity.DiscountAmount = entity.OldPrice - entity.Price;
                            nl.Add(entity);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 产品销售明细
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalesDetailPage_ChanPin(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderinfoEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            int Quatity;
            decimal SumPrice;
            decimal ZengSong;
            decimal ZheRang;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var list = cashBll.GetSalesChanPin(entity, out rows, out pagecount, out Quatity, out SumPrice, out ZengSong, out ZheRang);

            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.Quatity = Quatity;
            ViewBag.SumPrice = SumPrice;
            ViewBag.ZengSong = ZengSong;
            ViewBag.ZheRang = ZheRang;


            if (Request.IsAjaxRequest())
                return PartialView("_Sale_ChanPin", result);
            return View(result);
        }

        /// <summary>
        /// 获取划扣销售列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalesDetailPage_HuaKou(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderinfoEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            int quatity;
            decimal sumPrice;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var list = cashBll.GetSalesHuaKou(entity, out rows, out pagecount, out quatity, out sumPrice);
            // var list = cashBll.GetSalesChanPin(entity, out rows, out pagecount);


            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.Quatity = quatity;
            ViewBag.SumPrice = sumPrice;

            if (Request.IsAjaxRequest())
                return PartialView("_Sale_HuaKou", result);
            return View(result);
        }

        /// <summary>
        /// 获取单项销售列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalesDetailPage_DanXiang(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderinfoEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            int rows;
            int pagecount;
            int Quatity;
            decimal SumPrice;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var list = cashBll.GetSalesDanXiang(entity, out rows, out pagecount, out Quatity, out SumPrice);

            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.Quatity = Quatity;
            ViewBag.SumPrice = SumPrice;

            if (Request.IsAjaxRequest())
                return PartialView("_Sale_DanXiang", result);
            return View(result);
        }

        /// <summary>
        /// 还款明细
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalesDetailPage_HuanKuan(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderinfoEntity>().AsQueryable().ToPagedList(1, 10));
                }
            }
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));


            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            decimal SumPrice;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var list = cashBll.GetSalesHuanKuan(entity, out rows, out pagecount, out SumPrice);

            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.SumPrice = SumPrice;

            if (Request.IsAjaxRequest())
                return PartialView("_Sale_HuanKuan", result);
            return View(result);
        }

        #endregion


        #region==类别占有率==

        /// <summary>
        /// 类别占有率  统计项目的+产品的基础类型和特殊项目
        /// </summary>
        /// <returns></returns>
        public ActionResult TypeZhanyoulv(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var caretypelist = iwareBLL.GetProductTypeNoPage(LoginUserEntity.ParentHospitalID, 1);//获取项目分类列表
            var allprice = cashBll.GetAllProjectTypePrice(entity);
            List<OrderEntity> list = new List<OrderEntity>();
            //项目分类占比
            foreach (var info in caretypelist)
            {
                OrderEntity model = new OrderEntity();
                model.Name = info.Name;
                //获取项目的营业额
                entity.ProductType = info.ID;
                model.Price = cashBll.GetProjectTypePrice(entity);
                model.ActurePrice = allprice != 0 ? model.Price / allprice : 0;
                list.Add(model);
            }
            //产品占比：常规产品+特殊产品
            OrderEntity model1 = new OrderEntity();
            _SeachEntity entity1 = entity;
            entity1.ProductType = 1;
            model1.Price = cashBll.GetProducctTypePrice(entity1);
            model1.ActurePrice = allprice != 0 ? model1.Price / allprice : 0;
            model1.Name = "常规产品";
            list.Add(model1);

            OrderEntity model2 = new OrderEntity();
            _SeachEntity entity2 = entity;
            entity2.ProductType = 2;
            model2.Price = cashBll.GetProducctTypePrice(entity2);
            model2.ActurePrice = allprice != 0 ? model2.Price / allprice : 0;
            model2.Name = "特殊产品";
            list.Add(model2);

            list = SortHelper.DataSorting<OrderEntity>(list, "Price", "desc").ToList();

            list = list.Where(x => x.Price > 0).ToList();
            var result = list.AsQueryable().ToPagedList(1, 1000);

            ViewBag.ordersum = list.Sum(x =>x.Price);
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            if (Request.IsAjaxRequest())
                return PartialView("_TypeZhanyoulv", result);
            return View(result);
        }


        /// <summary>
        /// 品牌占有率
        /// </summary>
        /// <returns></returns>
        public ActionResult BrandZhanyoulv(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var caretypelist = iwareBLL.GetBrandListNoPage(LoginUserEntity.ParentHospitalID);
            var allprice = cashBll.GetBrandTypePrice(entity);
            List<OrderEntity> list = new List<OrderEntity>();
            //品牌占比
            foreach (var info in caretypelist)
            {
                OrderEntity model = new OrderEntity();
                model.Name = info.Name;
                //获取项目的营业额
                entity.BrandID = info.ID;
                model.Price = cashBll.GetBrandTypePrice(entity);
                model.ActurePrice = allprice != 0 ? model.Price / allprice : 0;
                list.Add(model);
            }


            list = SortHelper.DataSorting<OrderEntity>(list, "Price", "desc").ToList();
            list = list.Where(x => x.Price > 0).ToList();
            var result = list.AsQueryable().ToPagedList(1, 1000);
            ViewBag.ordersum = list.Sum(x =>x.Price);
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            if (Request.IsAjaxRequest())
                return PartialView("_BrandZhanyoulv", result);
            return View(result);
        }
        #endregion

        #region==普及率==

        /// <summary>
        /// 普及率
        /// </summary>
        /// <returns></returns>
        public ActionResult PopularityRate(_SeachEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;

            entity.numPerPage = 50; //每页显示条数
            if (string.IsNullOrWhiteSpace(entity.orderField)) { entity.orderField = "ID"; }
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.s_ID == 0)
            {
                entity.s_ID = 1;
            }
            if (entity.s_StareTime == DateTime.MinValue) entity.s_StareTime = DateTime.Now;
            if (entity.s_Endtime == DateTime.MinValue) entity.s_Endtime = DateTime.Now;

            //汪
            entity.s_Parent = LoginUserEntity.ParentHospitalID;
            //汪
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.Sumyouxiaoke = cashBll.GetYouXiaoKeAll(entity.s_HospitalID);
            ViewBag.s_ID = entity.s_ID;

            ViewBag.s_StareTime = entity.s_StareTime.ToString("yyyy-MM-dd");
            ViewBag.s_Endtime = entity.s_Endtime.ToString("yyyy-MM-dd");

            if (entity.pageNum <= 1)
            {
                if (!Request.IsAjaxRequest())
                {
                    return View(new List<OrderEntity>().ToPagedList(1, 10));
                }
            }
            var list = cashBll.GetPopularityRateList(entity);

            //  result.TotalItemCount = rows;
            //  result.CurrentPageIndex = entity.pageNum;
            if (entity.orderField == "ProductName")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.ProductName).ToList() : list.OrderBy(i => i.ProductName).ToList();
            if (entity.orderField == "Quantity")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.Quantity).ToList() : list.OrderBy(i => i.Quantity).ToList();

            if (entity.orderField == "UserID")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.UserID).ToList() : list.OrderBy(i => i.UserID).ToList();

            var result = list.ToPagedList(1, 100000000);
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ordersum = list.Sum(x =>x.Quantity);
            if (Request.IsAjaxRequest())
                return PartialView("_PopularityRate", result);
            return View(result);
        }
        /// <summary>
        /// 普及率详情
        /// </summary>
        /// <returns></returns>
        public ActionResult PopularityRateDetail(_SeachEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            int rows;
            int pagecount;
            entity.numPerPage = 50000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.s_ID == 0)
            {
                entity.s_ID = 1;
            }
            var list = cashBll.GetPopularityRateDetail(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            //if (Request.IsAjaxRequest())
            //    return PartialView("_PopularityRate", list);
            return View(result);
        }
        #endregion

        #region==业务销售汇总==
        /// <summary>
        /// 销售汇总
        /// </summary>
        /// <returns></returns>
        public ActionResult Salestatistics()
        {
            return View();
        }
        /// <summary>
        /// 产品销售汇总
        /// </summary>
        /// <returns></returns>
        public ActionResult SalestatisticsProduct(_SeachEntity entity)
        {
            ViewBag.al = false;//是否传入单个门店
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;

            if (!Request.IsAjaxRequest())
            {
                return View(new List<OrderinfoEntity>());
            }

            int rows;
            int pagecount;
            decimal XiaoShouAmount;
            int Quatity;
            decimal ZengSong;
            decimal ZheRang;
            decimal ChengBen;
            decimal Summaoli;
            entity.numPerPage = 99999999;//每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProdcuitID"; }
            if (entity.s_HospitalID == 0)
            {
                entity.s_TableType = LoginUserEntity.ParentHospitalID;
                //entity.s_HospitalID = loginUserEntity.HospitalID;
                ViewBag.al = true;
            }
            else
            {
                var hospitalmodel = personnelBLL.HospitalEntityByID(entity.s_HospitalID);
                ViewBag.hname = hospitalmodel.Name;
            }

            if (entity.s_HospitalID > 0)
            {
                var hosp = personnelBLL.HospitalEntityByID(entity.s_HospitalID);
                ViewBag.HospitalName = hosp.Name;
            }
            else
            {
                ViewBag.HospitalName = "全店";
            }

            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            var list = cashBll.GetSalestatisticsProductList(entity, out rows, out pagecount, out XiaoShouAmount, out Quatity, out ZengSong, out ZheRang, out ChengBen, out Summaoli);
            list = list.OrderBy(i => i.HospitalID).ToList();

            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            list =  list.OrderByDescending(i => i.Quantity).ToList();
            if (entity.orderField == "Quantity")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.Quantity).ToList() : list.OrderBy(i => i.Quantity).ToList();
            if (entity.orderField == "AllPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.AllPrice).ToList() : list.OrderBy(i => i.AllPrice).ToList();
            if (entity.orderField == "SendAmount")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.SendAmount).ToList() : list.OrderBy(i => i.SendAmount).ToList();
            if (entity.orderField == "DiscountAmount")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.DiscountAmount).ToList() : list.OrderBy(i => i.DiscountAmount).ToList();
            if (entity.orderField == "CostPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.CostPrice).ToList() : list.OrderBy(i => i.CostPrice).ToList();
            if (entity.orderField == "RetailPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.RetailPrice).ToList() : list.OrderBy(i => i.RetailPrice).ToList();
            //if (entity.orderField == "XiaoShouAmount")
            //    list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i..XiaoShouAmount).ToList() : list.OrderBy(i => i.XiaoShouAmount).ToList();

            

            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.XiaoShouAmount = XiaoShouAmount;//销售总额
            ViewBag.Quatity = Quatity;
            ViewBag.ZengSong = ZengSong;
            ViewBag.ZheRang = ZheRang;
            ViewBag.ChengBen = ChengBen;
            ViewBag.Summaoli = Summaoli;
            if (Request.IsAjaxRequest())
                return PartialView("_SalestatisticsProduct", list);
            return View(list);
        }

        /// <summary>
        /// 划扣销售汇总
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalestatisticsHuaKou(_SeachEntity entity)
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
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0 && !Request.IsAjaxRequest())
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.s_HospitalID == 0)
            {
                ViewBag.al = true;//是否传入单个门店
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
            int AllUser = 0;
            decimal AllPrice = 0;
            decimal AllChengben = 0;
            decimal AllMaoLi = 0;
            //entity.orderDirection = "desc";
            entity.numPerPage = 99999999; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProdcuitID"; }

            var list = cashBll.GetSalestatisticsHuaKouList(entity, out rows, out pagecount, out AllQuantity, out AllPrice, out AllChengben, out AllMaoLi, out AllUser);
            // var list = cashBll.GetSalesChanPin(entity, out rows, out pagecount);
            list = list.OrderBy(i => i.HospitalID).ToList();
            if (entity.orderField == "Quantity")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.Quantity).ToList() : list.OrderBy(i => i.Quantity).ToList();
            if (entity.orderField == "AllPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.AllPrice).ToList() : list.OrderBy(i => i.AllPrice).ToList();
            if (entity.orderField == "SendAmount")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.SendAmount).ToList() : list.OrderBy(i => i.SendAmount).ToList();
            if (entity.orderField == "DiscountAmount")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.DiscountAmount).ToList() : list.OrderBy(i => i.DiscountAmount).ToList();
            if (entity.orderField == "CostPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.CostPrice).ToList() : list.OrderBy(i => i.CostPrice).ToList();
            if (entity.orderField == "RetailPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.RetailPrice).ToList() : list.OrderBy(i => i.RetailPrice).ToList();
            if (entity.orderField == "Maoli")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.Maoli).ToList() : list.OrderBy(i => i.Maoli).ToList();

            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.AllQuantity = AllQuantity;
            ViewBag.AllUser = AllUser;
            ViewBag.AllPrice = AllPrice;
            ViewBag.AllChengben = AllChengben;
            ViewBag.AllMaoLi = AllMaoLi;

            if (Request.IsAjaxRequest())
                return PartialView("_SalestatisticsHuaKou", result);
            return View(result);
        }

        /// <summary>
        /// 单项销售汇总
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalestatisticsDanXiang(_SeachEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.al = false;
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
            int AllUser = 0;
            decimal AllPrice = 0;
            decimal AllChengben = 0;
            decimal AllMaoLi = 0;
            decimal AllZengsong = 0;
            decimal AllZheRang = 0;
            //entity.orderDirection = "desc";
            entity.numPerPage = 99999999; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProdcuitID"; }

            var list = cashBll.GetSalestatisticsDanXiangList(entity, out rows, out pagecount, out AllQuantity, out AllPrice, out AllChengben, out AllMaoLi, out AllUser, out AllZengsong, out AllZheRang);
            // var list = cashBll.GetSalesChanPin(entity, out rows, out pagecount);
            list = list.OrderBy(i => i.HospitalID).ToList();
            if (entity.orderField == "Quantity")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.Quantity).ToList() : list.OrderBy(i => i.Quantity).ToList();
            if (entity.orderField == "AllPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.AllPrice).ToList() : list.OrderBy(i => i.AllPrice).ToList();
            if (entity.orderField == "SendAmount")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.SendAmount).ToList() : list.OrderBy(i => i.SendAmount).ToList();
            if (entity.orderField == "DiscountAmount")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.DiscountAmount).ToList() : list.OrderBy(i => i.DiscountAmount).ToList();
            if (entity.orderField == "CostPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.CostPrice).ToList() : list.OrderBy(i => i.CostPrice).ToList();
            if (entity.orderField == "RetailPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.RetailPrice).ToList() : list.OrderBy(i => i.RetailPrice).ToList();
            if (entity.orderField == "Maoli")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.Maoli).ToList() : list.OrderBy(i => i.Maoli).ToList();
            //if (entity.orderField == "AllMaoLi")
            //    list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.AllMaoLi).ToList() : list.OrderBy(i => i.AllMaoLi).ToList();
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);

            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.AllQuantity = AllQuantity;
            ViewBag.AllUser = AllUser;
            ViewBag.AllPrice = AllPrice;
            ViewBag.AllChengben = AllChengben;
            ViewBag.AllMaoLi = AllMaoLi;
            ViewBag.AllZengsong = AllZengsong;
            ViewBag.AllZheRang = AllZheRang;


            if (Request.IsAjaxRequest())
                return PartialView("_SalestatisticsDanXiang", result);
            return View(result);
        }

        /// <summary>
        /// 储值卡销售汇总
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalestatisticsChuZhiKaList(_SeachEntity entity)
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
            int AllUser = 0;
            decimal AllPrice = 0;
            decimal AllChengben = 0;
            decimal AllMaoLi = 0;
            decimal AllZengsong = 0;
            decimal AllZheRang = 0;
            //entity.orderDirection = "desc";
            entity.numPerPage = 99999999; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProdcuitID"; }
            var list = cashBll.GetSalestatisticsChuZhikaList(entity, out rows, out pagecount, out AllQuantity, out AllPrice, out AllZengsong);
            if (entity.orderField == "Quantity")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.Quantity).ToList() : list.OrderBy(i => i.Quantity).ToList();
            if (entity.orderField == "AllPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.AllPrice).ToList() : list.OrderBy(i => i.AllPrice).ToList();
            if (entity.orderField == "SendAmount")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.SendAmount).ToList() : list.OrderBy(i => i.SendAmount).ToList();
            if (entity.orderField == "DiscountAmount")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.DiscountAmount).ToList() : list.OrderBy(i => i.DiscountAmount).ToList();
            if (entity.orderField == "CostPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.CostPrice).ToList() : list.OrderBy(i => i.CostPrice).ToList();
            if (entity.orderField == "RetailPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.RetailPrice).ToList() : list.OrderBy(i => i.RetailPrice).ToList();
            if (entity.orderField == "Maoli")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.Maoli).ToList() : list.OrderBy(i => i.Maoli).ToList();
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.AllQuantity = AllQuantity;
            ViewBag.AllUser = AllUser;
            ViewBag.AllPrice = AllPrice;
            ViewBag.AllChengben = AllChengben;
            ViewBag.AllMaoLi = AllMaoLi;
            ViewBag.AllZengsong = AllZengsong;
            ViewBag.AllZheRang = AllZheRang;


            if (Request.IsAjaxRequest())
                return PartialView("_SalestatisticsChuZhiKa", result);
            return View(result);
        }

        /// <summary>
        /// 疗程项目销售
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult SalestatisticsLiaochengXiangmuList(_SeachEntity entity)
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

            //entity.orderDirection = "desc";
            entity.numPerPage = 99999999; //每页显示条数
            //if (entity.orderField + "" == "") { entity.orderField = "Quantity"; }
            var list = cashBll.GetSalestatisticsLiaochengkaList(entity, out rows, out pagecount, out AllQuantity, out AllPrice);

            if (entity.orderField == "Quantity")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.Quantity).ToList() : list.OrderBy(i => i.Quantity).ToList();
            if (entity.orderField == "AllPrice")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.AllPrice).ToList() : list.OrderBy(i => i.AllPrice).ToList();
            if (entity.orderField == "ProdcuitName")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.ProdcuitName).ToList() : list.OrderBy(i => i.ProdcuitName).ToList();
            if (entity.orderField == "CardName")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.CardName).ToList() : list.OrderBy(i => i.CardName).ToList();
            if (entity.orderField == "ProductTypeName")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.ProductTypeName).ToList() : list.OrderBy(i => i.ProductTypeName).ToList();
            if (entity.orderField == "BrandName")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.BrandName).ToList() : list.OrderBy(i => i.BrandName).ToList();
            if (entity.orderField == "HospitalName")
                list = entity.orderDirection.ToLower() == "desc" ? list.OrderByDescending(i => i.HospitalName).ToList() : list.OrderBy(i => i.HospitalName).ToList();
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.AllQuantity = AllQuantity;
            ViewBag.AllPrice = AllPrice;


            if (Request.IsAjaxRequest())
                return PartialView("_SalestatisticsLiaochengXiangmuList", result);
            return View(result);

        }



        #endregion

        #region==导出==
        /// <summary>
        /// 前台消费管理导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportReceptionManager(OrderinfoEntity entity)
        {

            if (entity.HospitalID == 0) entity.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            entity.STime = entity.STime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.STime.ToString("yyyy-MM-dd 00:00:01"));
            entity.ETime = entity.ETime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.ETime.ToString("yyyy-MM-dd 23:59:59"));

            int rows;
            int pagecount;
            // entity.numPerPage = 50; //每页显示条数

            string newcardname = entity.CardName;
            if (entity.OrderType == 6)
            {
                entity.CardName = "";
                entity.numPerPage = 10000;
            }
            decimal NeedAllPrice, Price, XianJin, ShuaKa, DiscountAmount, SendAmount, Huachika = 0;
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var list = cashBll.GetReceptionManager(entity, out rows, out pagecount, out NeedAllPrice, out Price, out XianJin, out ShuaKa, out DiscountAmount, out SendAmount, out Huachika);
            var AddList = new List<OrderinfoEntity>();
            var DelList = new List<OrderinfoEntity>();

            foreach (var newinfo in list)
            {
                var info = newinfo;
                if (info.Type == 3 && info.IsChongzhi != 1)
                {
                    ProjectProductEntity pp = new ProjectProductEntity();
                    pp.ProgrammeID = newinfo.ProdcuitID;

                    string[] idlist = info.CardBalanceID == null ? null : info.CardBalanceID.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (idlist != null)
                    {
                        foreach (var id in idlist)
                        {
                            var cardbalanc = cashBll.GetMainCardBalanceModel(new MainCardBalanceEntity { ID = Convert.ToInt32(id) });

                            var Add = new OrderinfoEntity();
                            Add.CardType = newinfo.CardType;
                            Add.CreateTime = newinfo.CreateTime;
                            Add.JoinUserName = newinfo.JoinUserName;
                            Add.OrderType = newinfo.OrderType;
                            Add.Quantity = 1;    //储值卡在数据库用户余额里面里面是没有次数的,但是现在要默认显示为1,现在默认为1cardbalanc.AllTiems;
                            Add.Type = newinfo.Type;
                            Add.AllPrice = cardbalanc.AllPrice;
                            Add.ProdcuitName1 = cardbalanc.ProdcuitName;
                            Add.ProdcuitName = cardbalanc.ProdcuitName;
                            Add.DocumentNumber = info.DocumentNumber;
                            Add.Price = cardbalanc.AllPrice;
                            Add.InfoBuyType = newinfo.InfoBuyType;
                            Add.RetailPrice = cardbalanc.RetailPrice;
                            Add.OrderNo = info.OrderNo;
                            Add.UserName = info.UserName;
                            Add.IsGive = cardbalanc.IsGive;
                            Add.HospitalName = info.HospitalName;
                            if (cardbalanc.Type == 2)
                            {
                                Add.Quantity = cardbalanc.AllTiems;
                                Add.Price = cardbalanc.Price;
                                Add.AllPrice = cardbalanc.Price * cardbalanc.AllTiems;
                            }
                            AddList.Add(Add);
                        }
                        DelList.Add(newinfo);
                    }
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
                            newname = newname + dyinfo.ProdcuitName1 + ",";
                        }
                    }

                    info.ProdcuitName = newname;

                }
                //退卡操作
                if (newinfo.OrderType == 5)
                {
                    newinfo.Quantity = -newinfo.Quantity;
                    newinfo.AllPrice = -newinfo.AllPrice;
                }

                if (newinfo.OrderType == 1 || newinfo.OrderType == 2)
                {
                    if (newinfo.Price < 0)
                    {
                        newinfo.Price = System.Math.Abs(newinfo.Price);
                    }
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


            list = list.OrderBy(i => i.DocumentNumber).ToList();
            //疗程卡
            if (entity.OrderType == 6 && !string.IsNullOrEmpty(newcardname))
            {
                list = list.Where(c => c.ProdcuitName1 == newcardname || c.ProdcuitName == newcardname).ToList();
            }
            string oldorderno = "";
            string oldshijian = "";
            string olddanju = "";


            decimal shuliang = 0;
            decimal zonge = 0;
            decimal xianjinzr = 0;
            decimal yinliankazr = 0;
            decimal chuzhikazr = 0;
            decimal zengkazr = 0;
            decimal qiankuan = 0;
            decimal huakazr = 0;
            StringBuilder str = new StringBuilder();

            str.Append("<table class=\"gridTable\" style=\"border-collapse: collapse;\"><thead><tr><th>日期</th><th>顾客</th><th>门店</th><th>单据编号</th><th>订单类型</th><th>品牌</th><th>分类</th><th>项目名称</th><th>销售单价</th><th>优惠单价</th><th>数量</th><th>总额</th><th>现金</th><th>券类</th><th>划卡</th><th>储值卡划扣</th><th>赠卡划扣</th><th>欠款</th></tr></thead><tbody>");

            //组装SQL 
            foreach (var info in list)
            {
                str.Append("<tr>");
                var paymentlist = cashBll.GetAllPaymentOrder(new PaymentOrderEntity { OrderNo = info.OrderNo });
                var Xianjin = paymentlist.Where(t => t.ParentPayType == 1).Count() > 0 ? paymentlist.Where(t => t.ParentPayType == 1).Sum(c => c.PayMoney) : 0;
                var Shuaka = paymentlist.Where(t => t.ParentPayType == 2).Count() > 0 ? paymentlist.Where(t => t.ParentPayType == 2).Sum(c => c.PayMoney) : 0;
                var HuaKa = paymentlist.Where(t => t.PayType == 4).Count() > 0 ? paymentlist.Where(t => t.PayType == 4).ToList().Sum(i => i.PayMoney) : 0;


                var shijian = list.Where(t => t.CreateTime.ToShortDateString() == info.CreateTime.ToShortDateString()).Count() > 0 ? list.Where(t => t.CreateTime.ToShortDateString() == info.CreateTime.ToShortDateString()).Count() : 0;
                var danju = list.Where(t => t.OrderNo == info.OrderNo).Count() > 0 ? list.Where(t => t.OrderNo == info.OrderNo).Count() : 0;
                var count = list.Where(t => t.OrderNo == info.OrderNo).Count();
                var zengka = paymentlist.Where(t => t.PayType == 3 && t.IsGive == 1).ToList().Sum(i => i.PayMoney);
                var chuzhika = paymentlist.Where(t => t.PayType == 3).ToList().Where(t => t.IsGive != 1).Sum(i => i.PayMoney);

                if (oldshijian != info.CreateTime.ToShortDateString())
                {
                    str.AppendFormat("<td  style='border:1px solid #ccc;' rowspan='{0}'>{1}</td>", shijian, info.CreateTime.ToString("yyyy-MM-dd"));
                    oldshijian = info.CreateTime.ToShortDateString();
                }
                str.AppendFormat("<td style='border:1px solid #ccc;'>{0}</td>", info.UserName);
                str.AppendFormat("<td style='border:1px solid #ccc;'>{0}</td>", info.HospitalName);
                if (olddanju != info.OrderNo)
                {
                    str.AppendFormat("<td style='border:1px solid #ccc;' rowspan='{0}'><a> {1}</a></td>", danju, info.DocumentNumber);
                    olddanju = info.OrderNo;
                }
                str.AppendFormat("<td style='border:1px solid #ccc;'>{0}</td>", info.GetInfoBuyTyoeBy5Lei());
                str.AppendFormat("<td>{0}</td>", info.BrandName);
                str.AppendFormat("<td>{0}</td>", info.ProductTypeName);
                str.AppendFormat("<td style='border:1px solid #ccc;'>{0}</td>", info.ProdcuitName == null ? info.ProdcuitName1 : info.ProdcuitName);
                str.AppendFormat("<td style='border:1px solid #ccc;'>{0}</td>", info.Price);
                str.AppendFormat("<td style='border:1px solid #ccc;'>{0}</td>", info.AllPrice / (info.Quantity == 0 ? 1 : info.Quantity));

                str.AppendFormat("<td style='border:1px solid #ccc;'>{0}</td>", info.Quantity);
                shuliang = shuliang + info.Quantity;
                str.AppendFormat("<td style='border:1px solid #ccc;'>{0}</td>", info.AllPrice);
                zonge = zonge + info.AllPrice;
                if (oldorderno != info.OrderNo)
                {
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, Xianjin);
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, Shuaka);
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, HuaKa);
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, chuzhika);
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, zengka);
                    str.AppendFormat("<td rowspan='{0}'>{1}</td>", count, info.OldPrice);
                    oldorderno = info.OrderNo;
                    xianjinzr = xianjinzr + Xianjin;
                    yinliankazr = yinliankazr + Shuaka;
                    chuzhikazr += chuzhika;
                    zengkazr += zengka;
                    qiankuan += info.OldPrice;
                    huakazr += HuaKa;
                }

                str.Append("</tr>");
                //info.ids = str.ToString();
            }
            str.Append("</tbody>");
            str.AppendFormat("<tfoot><tr><td style='border:1px solid #ccc;' colspan=\"7\"></td><td style='border:1px solid #ccc;'>当页合计:</td><td></td><td></td><td style='border:1px solid #ccc;'>{0}</td><td style='border:1px solid #ccc;'>{1}</td><td style='border:1px solid #ccc;'>{2}</td><td style='border:1px solid #ccc;'>{3}</td><td style='border:1px solid #ccc;'>{4}</td><td style='border:1px solid #ccc;'>{5}</td><td style='border:1px solid #ccc;'>{6}</td><td style='border:1px solid #ccc;'>{7}</td></tr></tfoot>", shuliang, zonge, xianjinzr, yinliankazr, huakazr, chuzhikazr, zengkazr, qiankuan);
            str.Append("</table>");
            JsonModelEntity model = new JsonModelEntity();
            model.OutputHtml = str.ToString();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "utf-8";
            Response.ContentType = "application/vnd.ms-excel";
            Response.AppendHeader("Content-Disposition", "attachment;filename= Huizong.xls");
            Response.ContentEncoding = Encoding.GetEncoding("utf-8");
            Response.Write(str.ToString());

            Response.Flush();
            Response.End();
            return null;
        }
        /// <summary>
        /// 用户余额导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportCardBalanceManager(MainCardBalanceEntity entity)
        {
            int rows;
            int pagecount;
            decimal sumcard;
            decimal sumconsumer;
            decimal Zhexian;
            entity.numPerPage = 100000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProdcuitName,CardName"; }
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            var list = IpatBLL.GetCardBalanceByHospitalId(entity, out rows, out pagecount, out sumcard, out sumconsumer, out Zhexian);

            list = list.Where(t => t.CardName != "积分消费").ToList();
            list = list.Where(t => t.IsDel != 3).ToList();
            list = list.AsQueryable().ToPagedList(1, entity.numPerPage);

            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("名店名称"), new DataColumn("卡号"), new DataColumn("用户名称"), new DataColumn("品牌"), new DataColumn("卡项名称（项目名称）"), new DataColumn("余次"), new DataColumn("余额"), new DataColumn("折现") });

            foreach (MainCardBalanceEntity model in list)
            {

                DataRow row = tableExport.NewRow();
                row["名店名称"] = model.HospitalName;
                row["卡号"] = model.MemberNo;
                row["用户名称"] = model.UserName;
                row["品牌"] = model.BrandName;
                row["卡项名称(项目名称)"] = model.Type == 2 ? model.ProdcuitName : model.CardName;
                row["余次"] = model.Type == 2 ? model.Tiems : 0;
                row["余额"] = model.Type == 2 ? 0 : model.Price;
                row["折现"] = model.Type == 2 ? model.Tiems * model.Price : (model.IsGive == 1 ? 0 : model.Price);
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["卡项名称(项目名称)"] = "合计";
            row1["余次"] = Request["txtcardCount"];
            row1["余额"] = Request["txtsumconsumer"];
            row1["折现"] = Request["txtZhexian"];
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "JieYu");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "JieYu.xls");
        }


        /// <summary>
        /// 产品销售汇总--导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportSalestatisticsProduct(_SeachEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            int rows;
            int pagecount;
            decimal XiaoShouAmount;
            int Quatity;
            decimal ZengSong;
            decimal ZheRang;
            decimal ChengBen;
            decimal Summaoli;
            // entity.numPerPage = 1000000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProdcuitID"; }
            //if (entity.s_HospitalID == 0)
            //{
            //    entity.s_HospitalID = loginUserEntity.HospitalID;
            //}

            var list = cashBll.GetSalestatisticsProductList(entity, out rows, out pagecount, out XiaoShouAmount, out Quatity, out ZengSong, out ZheRang, out ChengBen, out Summaoli);

            var hospitalList = personnelBLL.GetHospitalListByParentID(LoginUserEntity.ParentHospitalID);
            if (entity.s_HospitalID > 0)
            {
                hospitalList = hospitalList.Where(i => i.ID == entity.s_HospitalID).ToList();
            }

            DataTable tableExport = new DataTable("Export");
            bool HasPermission = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_项目管理成本价);
            if (HasPermission)
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("产品名称"), new DataColumn("零售单价"), new DataColumn("购买价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("赠送"), new DataColumn("折让"), new DataColumn("成本"), new DataColumn("毛利"), new DataColumn("销售占比"), new DataColumn("毛利占比") });
            }
            else
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("产品名称"), new DataColumn("零售单价"), new DataColumn("购买价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("赠送"), new DataColumn("折让"), new DataColumn("毛利"), new DataColumn("销售占比"), new DataColumn("毛利占比") });
            }

            foreach (var hosp in hospitalList)
            {
                var dlist = list.Where(i => i.HospitalID == hosp.ID).ToList();
                DataRow row1 = tableExport.NewRow();
                row1["门店名称"] = hosp.Name;
                row1["产品名称"] = "";
                row1["零售单价"] = "";
                row1["购买价"] = "";
                row1["数量"] = "";
                row1["合计"] = "";
                row1["赠送"] = "";
                row1["折让"] = "";
                if (HasPermission)
                {
                    row1["成本"] = "";
                }
                row1["毛利"] = "";
                row1["销售占比"] = "";
                row1["毛利占比"] = "";
                tableExport.Rows.Add(row1);


                foreach (OrderinfoEntity model in dlist)
                {

                    DataRow row = tableExport.NewRow();
                    row["门店名称"] = "";//model.HospitalName;
                    row["产品名称"] = model.ProdcuitName;
                    row["零售单价"] = model.RetailPrice;
                    row["购买价"] = model.Price;
                    row["数量"] = model.Quantity;
                    row["合计"] = model.AllPrice;
                    row["赠送"] = model.SendAmount;
                    row["折让"] = model.DiscountAmount;
                    if (HasPermission)
                    {
                        row["成本"] = model.CostPrice;
                    }
                    row["毛利"] = model.Maoli;
                    row["销售占比"] = XiaoShouAmount != 0 ? ((model.AllPrice / XiaoShouAmount) * 100).ToString("0.00") + "%" : "0%";
                    row["毛利占比"] = (model.MaoliZhanbi * 100).ToString("0.00") + "%";
                    tableExport.Rows.Add(row);
                }

            }
            DataRow row2 = tableExport.NewRow();
            row2["购买价"] = "合计";
            row2["数量"] = Request["txtshuliang"];
            row2["合计"] = Request["txtheji"];
            row2["赠送"] = Request["txtzengsong"];
            row2["折让"] = Request["txtzherang"];
            row2["成本"] = Request["txtchengben"];
            row2["毛利"] = Request["txtmaoli"];
            tableExport.Rows.Add(row2);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "ChanPInYewu");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "ChanPInYewu.xls");
        }

        /// <summary>
        /// 单项销售汇总--导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportSalestatisticsDanXiang(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_TableType = LoginUserEntity.ParentHospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));


            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            int AllQuantity = 0;
            int AllUser = 0;
            decimal AllPrice = 0;
            decimal AllChengben = 0;
            decimal AllMaoLi = 0;
            decimal AllZengsong = 0;
            decimal AllZheRang = 0;
            entity.orderDirection = "desc";
            // entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProdcuitID"; }

            var list = cashBll.GetSalestatisticsDanXiangList(entity, out rows, out pagecount, out AllQuantity, out AllPrice, out AllChengben, out AllMaoLi, out AllUser, out AllZengsong, out AllZheRang);
            // var list = cashBll.GetSalesChanPin(entity, out rows, out pagecount);
            DataTable tableExport = new DataTable("Export");
            bool HasPermission = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_项目管理成本价);
            if (HasPermission)
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("名称"), new DataColumn("单价"), new DataColumn("优惠价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("赠送"), new DataColumn("折让"), new DataColumn("成本"), new DataColumn("毛利"), new DataColumn("销售占比"), new DataColumn("毛利占比") });
            }
            else
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("名称"), new DataColumn("单价"), new DataColumn("优惠价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("赠送"), new DataColumn("折让"), new DataColumn("毛利"), new DataColumn("销售占比"), new DataColumn("毛利占比") });
            }
            var hospitalList = personnelBLL.GetHospitalListByParentID(LoginUserEntity.ParentHospitalID);
            if (entity.s_HospitalID > 0)
            {
                hospitalList = hospitalList.Where(i => i.ID == entity.s_HospitalID).ToList();
            }
            foreach (var hosp in hospitalList)
            {
                var dlist = list.Where(i => i.HospitalID == hosp.ID).ToList();
                DataRow row1 = tableExport.NewRow();
                row1["门店名称"] = hosp.Name;
                row1["名称"] = "";
                row1["单价"] = "";
                row1["优惠价"] = "";
                row1["数量"] = "";
                row1["合计"] = "";
                row1["赠送"] = "";
                row1["折让"] = "";
                if (HasPermission)
                {
                    row1["成本"] = "";
                }
                row1["毛利"] = "";
                row1["销售占比"] = "";
                row1["毛利占比"] = "";
                tableExport.Rows.Add(row1);

                foreach (OrderinfoEntity model in dlist)
                {

                    DataRow row = tableExport.NewRow();
                    row["门店名称"] = ""; //model.HospitalName;
                    row["名称"] = model.ProdcuitName;
                    row["单价"] = model.RetailPrice;
                    row["优惠价"] = model.Price;
                    row["数量"] = model.Quantity;
                    row["合计"] = model.AllPrice;
                    row["赠送"] = model.SendAmount;
                    row["折让"] = model.DiscountAmount;
                    if (HasPermission)
                    {
                        row["成本"] = model.CostPrice;
                    }
                    row["毛利"] = model.Maoli;
                    row["销售占比"] = ((model.AllPrice / Convert.ToDecimal(AllPrice) * 100).ToString("#0.00") + "%");
                    row["毛利占比"] = ((model.Maoli / Convert.ToDecimal(AllMaoLi) * 100).ToString("#0.00") + "%");

                    tableExport.Rows.Add(row);
                }
            }
            DataRow row2 = tableExport.NewRow();
            row2["优惠价"] = "合计";
            row2["数量"]=Request["txtAllQuantity"];
            row2["合计"] = Request["txtAllPrice"];
            row2["赠送"] = Request["txtAllZengsong"];
            row2["折让"] = Request["txtAllZheRang"];
            row2["成本"] = Request["txtAllChengben"];
            row2["毛利"] = Request["txtAllMaoLi"];
            tableExport.Rows.Add(row2);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "DanXiangXiaoShou");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "DanXiangXiaoShou.xls");
        }


        /// <summary>
        /// 划扣销售汇总---导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportSalestatisticsHuaKou(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_TableType = LoginUserEntity.ParentHospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));


            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            int AllQuantity = 0;
            int AllUser = 0;
            decimal AllPrice = 0;
            decimal AllChengben = 0;
            decimal AllMaoLi = 0;
            decimal AllZengsong = 0;
            decimal AllZheRang = 0;
            entity.orderDirection = "desc";
            // entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProdcuitID"; }

            var list = cashBll.GetSalestatisticsHuaKouList(entity, out rows, out pagecount, out AllQuantity, out AllPrice, out AllChengben, out AllMaoLi, out AllUser);


            // var list = cashBll.GetSalesChanPin(entity, out rows, out pagecount);
            DataTable tableExport = new DataTable("Export");


            bool hasPermission = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_项目管理成本价);
            if (hasPermission)
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("名称"), new DataColumn("单价"), new DataColumn("优惠价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("赠送"), new DataColumn("折让"), new DataColumn("成本"), new DataColumn("毛利"), new DataColumn("销售占比"), new DataColumn("毛利占比") });
            }
            else
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("名称"), new DataColumn("单价"), new DataColumn("优惠价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("赠送"), new DataColumn("折让"), new DataColumn("毛利"), new DataColumn("销售占比"), new DataColumn("毛利占比") });
            }
            var hospitalList = personnelBLL.GetHospitalListByParentID(LoginUserEntity.ParentHospitalID);
            if (entity.s_HospitalID > 0)
            {
                hospitalList = hospitalList.Where(i => i.ID == entity.s_HospitalID).ToList();
            }
            foreach (var hosp in hospitalList)
            {
                var dlist = list.Where(i => i.HospitalID == hosp.ID).ToList();
                DataRow row1 = tableExport.NewRow();
                row1["门店名称"] = hosp.Name;
                row1["名称"] = "";
                row1["单价"] = "";
                row1["优惠价"] = "";
                row1["数量"] = "";
                row1["合计"] = "";
                row1["赠送"] = "";
                row1["折让"] = "";
                if (hasPermission)
                {
                    row1["成本"] = "";
                }
                row1["毛利"] = "";
                row1["销售占比"] = "";
                row1["毛利占比"] = "";
                tableExport.Rows.Add(row1);

                foreach (var model in dlist)
                {

                    DataRow row = tableExport.NewRow();
                    row["门店名称"] = ""; //model.HospitalName;
                    row["名称"] = model.ProdcuitName;
                    row["单价"] = model.RetailPrice;
                    row["优惠价"] = model.Price;
                    row["数量"] = model.Quantity;
                    row["合计"] = model.AllPrice;
                    row["赠送"] = model.SendAmount;
                    row["折让"] = model.DiscountAmount;
                    if (hasPermission)
                    {
                        row["成本"] = model.CostPrice;
                    }
                    row["毛利"] = model.Maoli;
                    row["销售占比"] = ((model.AllPrice / Convert.ToDecimal(AllPrice) * 100).ToString("#0.00") + "%");
                    row["毛利占比"] = ((model.Maoli / Convert.ToDecimal(AllMaoLi) * 100).ToString("#0.00") + "%");

                    tableExport.Rows.Add(row);
                }
            }
            DataRow row2 = tableExport.NewRow();
            row2["优惠价"] = "合计";
            row2["数量"] = Request["txtshuliang"];
            row2["合计"] = Request["txtheji"];
            row2["成本"] = Request["txtchengben"];
            row2["毛利"] = Request["txtmaoli"];
            tableExport.Rows.Add(row2);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "HuaKouXiaoshou");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "HuaKouXiaoshou.xls");

        }
        public ActionResult ExportSalestatisticsHuaKou1(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_TableType = LoginUserEntity.ParentHospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));


            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            int AllQuantity = 0;
            int AllUser = 0;
            decimal AllPrice = 0;
            decimal AllChengben = 0;
            decimal AllMaoLi = 0;
            decimal AllZengsong = 0;
            decimal AllZheRang = 0;
            entity.orderDirection = "desc";
            // entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProdcuitID"; }

            var list = cashBll.GetSalestatisticsHuaKouList(entity, out rows, out pagecount, out AllQuantity, out AllPrice, out AllChengben, out AllMaoLi, out AllUser);


            // var list = cashBll.GetSalesChanPin(entity, out rows, out pagecount);
            DataTable tableExport = new DataTable("Export");


            bool hasPermission = Common.Manager.LoginHospitalUserManager.HasPermission(Common.Define.PermissionDefine.HospitalManager_Manage_项目管理成本价);
            if (hasPermission)
            {
                // tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("名称"), new DataColumn("单价"), new DataColumn("优惠价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("赠送"), new DataColumn("折让"), new DataColumn("成本"), new DataColumn("毛利"), new DataColumn("销售占比"), new DataColumn("毛利占比") });
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("名称"), new DataColumn("单价"), new DataColumn("优惠价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("成本"), new DataColumn("毛利"), new DataColumn("销售占比") });
            }
            else
            {
                tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("名称"), new DataColumn("单价"), new DataColumn("优惠价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("赠送"), new DataColumn("折让"), new DataColumn("毛利"), new DataColumn("销售占比"), new DataColumn("毛利占比") });
            }
            var hospitalList = personnelBLL.GetHospitalListByParentID(LoginUserEntity.ParentHospitalID);
            if (entity.s_HospitalID > 0)
            {
                hospitalList = hospitalList.Where(i => i.ID == entity.s_HospitalID).ToList();
            }
            foreach (var hosp in hospitalList)
            {
                var dlist = list.Where(i => i.HospitalID == hosp.ID).ToList();
                DataRow row1 = tableExport.NewRow();
                row1["门店名称"] = hosp.Name;
                row1["名称"] = "";
                row1["单价"] = "";
                row1["优惠价"] = "";
                row1["数量"] = "";
                row1["合计"] = "";
                //row1["赠送"] = "";//2020-1-8
                // row1["折让"] = "";//2020-1-8
                if (hasPermission)
                {
                    row1["成本"] = "";
                }
                row1["毛利"] = "";
                row1["销售占比"] = "";
                //row1["毛利占比"] = "";//2020-1-8
                tableExport.Rows.Add(row1);

                foreach (var model in dlist)
                {

                    DataRow row = tableExport.NewRow();
                    row["门店名称"] = ""; //model.HospitalName;
                    row["名称"] = model.ProdcuitName;
                    row["单价"] = model.RetailPrice;
                    row["优惠价"] = model.Price;
                    row["数量"] = model.Quantity;
                    row["合计"] = model.AllPrice;
                    //  row["赠送"] = model.SendAmount;//2020-1-8
                    // row["折让"] = model.DiscountAmount;//2020-1-8
                    if (hasPermission)
                    {
                        row["成本"] = model.CostPrice;
                    }
                    row["毛利"] = model.Maoli;
                    row["销售占比"] = ((model.AllPrice / Convert.ToDecimal(AllPrice) * 100).ToString("#0.00") + "%");
                    // row["毛利占比"] = ((model.Maoli / Convert.ToDecimal(AllMaoLi) * 100).ToString("#0.00") + "%");//2020-1-8

                    tableExport.Rows.Add(row);
                }
            }
            
            DataRow row2 = tableExport.NewRow();
            row2["优惠价"] = "合计";
            row2["数量"] = Request["txtshuliang"];
            row2["合计"] = Request["txtheji"];
            row2["成本"] = Request["txtchengben"];
            row2["毛利"] = Request["txtmaoli"];
            tableExport.Rows.Add(row2);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "HuaKouXiaoshou");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "HuaKouXiaoshou.xls");

        }

        /// <summary>
        /// 储值卡销售总额导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportSalestatisticsChiZhiKa(_SeachEntity entity)
        {
            if (entity.s_HospitalID == 0)
            {
                entity.s_TableType = LoginUserEntity.ParentHospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));


            int rows;
            int pagecount;
            int AllQuantity = 0;
            decimal AllPrice = 0;
            decimal AllZengsong = 0;
            entity.orderDirection = "desc";
            //  entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ProdcuitID"; }

            var list = cashBll.GetSalestatisticsChuZhikaList(entity, out rows, out pagecount, out AllQuantity, out AllPrice, out AllZengsong);
            DataTable tableExport = new DataTable("Export");



            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("名称"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("赠送"), new DataColumn("销售占比") });

            var hospitalList = personnelBLL.GetHospitalListByParentID(LoginUserEntity.ParentHospitalID);
            if (entity.s_HospitalID > 0)
            {
                hospitalList = hospitalList.Where(i => i.ID == entity.s_HospitalID).ToList();
            }
            foreach (var hosp in hospitalList)
            {
                var dlist = list.Where(i => i.HospitalID == hosp.ID).ToList();
                DataRow row1 = tableExport.NewRow();
                row1["门店名称"] = hosp.Name;
                row1["名称"] = "";
                row1["数量"] = "";
                row1["合计"] = "";
                row1["赠送"] = "";
                row1["销售占比"] = "";
                tableExport.Rows.Add(row1);

                foreach (OrderinfoEntity model in dlist)
                {

                    DataRow row = tableExport.NewRow();
                    row["门店名称"] = "";// model.HospitalName;
                    row["名称"] = model.ProdcuitName;
                    row["数量"] = model.Quantity;
                    row["合计"] = model.AllPrice;
                    row["赠送"] = model.SendAmount;
                    if (AllPrice == 0)
                    {
                        row["销售占比"] = 0;
                    }
                    else
                    {
                        row["销售占比"] = ((model.AllPrice / Convert.ToDecimal(AllPrice) * 100).ToString("#0.00") + "%");
                    }

                    tableExport.Rows.Add(row);
                }
            }
            DataRow row2 = tableExport.NewRow();
            row2["名称"] = "合计";
            row2["数量"] = Request["txtshuliang"];
            row2["合计"] = Request["txtheji"];
            row2["赠送"] = Request["txtzengsong"];
            tableExport.Rows.Add(row2);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "ChuzhikaXiaoShou");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "HuaKouXiaoshou.xls");

        }

        /// <summary>
        /// 疗程卡销售汇总导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportSalestatisticsLiaochengXiangmuList(_SeachEntity entity)
        {

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
            if (entity.orderField + "" == "") { entity.orderField = "ProjectID"; }
            var list = cashBll.GetSalestatisticsLiaochengkaList(entity, out rows, out pagecount, out AllQuantity, out AllPrice);
            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("品牌"), new DataColumn("分类"), new DataColumn("卡名称"), new DataColumn("项目名称"), new DataColumn("数量"), new DataColumn("总额") });

            var hospitalList = personnelBLL.GetHospitalListByParentID(LoginUserEntity.ParentHospitalID);
            if (entity.s_HospitalID > 0)
            {
                hospitalList = hospitalList.Where(i => i.ID == entity.s_HospitalID).ToList();
            }
            foreach (var hosp in hospitalList)
            {
                var dlist = list.Where(i => i.HospitalID == hosp.ID).ToList();
                DataRow row1 = tableExport.NewRow();
                row1["门店名称"] = hosp.Name;
                row1["品牌"] = "";
                row1["分类"] = "";
                row1["卡名称"] = "";
                row1["项目名称"] = "";
                row1["数量"] = "";
                row1["总额"] = "";

                tableExport.Rows.Add(row1);

                foreach (OrderinfoEntity model in dlist)
                {

                    DataRow row = tableExport.NewRow();
                    row["门店名称"] = model.HospitalName;
                    row["品牌"] = model.BrandName;
                    row["分类"] = model.ProductTypeName;
                    row["卡名称"] = model.CardName;
                    row["项目名称"] = model.ProdcuitName;
                    row["数量"] = model.Quantity;
                    row["总额"] = model.AllPrice;
                    tableExport.Rows.Add(row);
                }
            }
            DataRow row2 = tableExport.NewRow();
            row2["项目名称"] = "合计";
            row2["数量"] = Request["txtshuliang"];
            row2["总额"] = Request["txtheji"];
            tableExport.Rows.Add(row2);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "LiaochengXiangmuList");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "LiaochengXiangmuList.xls");

        }


        /// <summary>
        /// 普及率--导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportPopularityRate(_SeachEntity entity)
        {
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            int rows;
            int pagecount;
            // entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            if (entity.s_ID == 0)
            {
                entity.s_ID = 1;
            }
            var list = cashBll.GetPopularityRateList(entity);
            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("项目名称"), new DataColumn("销售数量"), new DataColumn("购买客数"), new DataColumn("普及率") });

            foreach (OrderEntity model in list)
            {

                DataRow row = tableExport.NewRow();
                row["项目名称"] = model.ProductName;
                row["销售数量"] = model.Quantity;
                row["购买客数"] = model.UserID;
               // row["有效客数"]=model
                row["普及率"] = (Convert.ToDouble(model.UserID) * 100 / Convert.ToDouble(model.DocumentNumber)).ToString("0.00") + "%";

                tableExport.Rows.Add(row);
            }
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "PuJilv");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "PuJilv.xls");
        }
        /// <summary>
        /// 导出划扣销售列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportSalesDetailPage_HuaKou(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            int Quatity;
            decimal SumPrice;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            // entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var list = cashBll.GetSalesHuaKou(entity, out rows, out pagecount, out Quatity, out SumPrice);
            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("店名"), new DataColumn("时间"), new DataColumn("品牌"), new DataColumn("分类"), new DataColumn("名称"), new DataColumn("单价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("单据号"), new DataColumn("会员卡号"), new DataColumn("顾客"), new DataColumn("说明") });

            foreach (OrderinfoEntity model in list)
            {

                DataRow row = tableExport.NewRow();
                row["店名"] = model.HospitalName;
                row["时间"] = model.CreateTime.ToString("yyyy-MM-dd");
                row["品牌"] = model.BrandName;
                row["分类"] = model.ProductTypeName;
                row["名称"] = model.ProdcuitName;
                row["单价"] = (model.AllPrice / model.Quantity).ToString("f2");
                row["数量"] = model.Quantity;
                row["合计"] = model.AllPrice;
                row["单据号"] = model.DocumentNumber;
                row["会员卡号"] = model.MemberNo;
                row["顾客"] = model.UserName;
                row["说明"] = model.Memo;

                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["单价"] = "合计";
            row1["数量"] = Request["txtQuatity"];
            row1["合计"]=Request["txtSumPrice"];
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "PuJilv");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "SalesDetailPage_HuaKou.xls");
        }

        /// <summary>
        /// 导出储值卡销售明细
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportSalesDetailPage_ChuzhiKa(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;


            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));


            int rows;
            int pagecount;
            int Quatity;
            decimal SumPrice;
            decimal ZengSong;
            decimal ZheRang;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;

            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var list = cashBll.GetSalesChuzhika(entity, out rows, out pagecount, out Quatity, out SumPrice, out ZengSong, out ZheRang);

            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("店名"), new DataColumn("时间"), new DataColumn("储值卡名称"), new DataColumn("单价"), new DataColumn("优惠价"), new DataColumn("数量"), new DataColumn("合计金额"), new DataColumn("赠送"), new DataColumn("折让"), new DataColumn("单据号"), new DataColumn("会员卡号"), new DataColumn("顾客"), new DataColumn("说明") });

            foreach (OrderinfoEntity model in list)
            {

                DataRow row = tableExport.NewRow();
                row["店名"] = model.HospitalName;
                row["时间"] = model.CreateTime.ToString("yyyy-MM-dd");
                row["储值卡名称"] = model.ProdcuitName;
                row["单价"] = model.OldPrice;
                row["优惠价"] = model.Price;
                row["数量"] = model.Quantity;
                row["合计金额"] = model.AllPrice;
                row["赠送"] = model.SendAmount;
                row["折让"] = model.DiscountAmount;
                row["单据号"] = model.DocumentNumber;
                row["会员卡号"] = model.MemberNo;
                row["顾客"] = model.UserName;
                row["说明"] = model.Memo;
                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["优惠价"] = "合计";
            row1["数量"]=Request["txtQuatity"];
            row1["合计金额"] = Request["txtSumPrice"];
            row1["赠送"] = Request["txtZengSong"];
            row1["折让"] = Request["txtZheRang"];
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "PuJilv");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "SalesDetailPage_ChuzhiKa.xls");

        }

        /// <summary>
        /// 导出获取疗程卡和项目销售明细
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportSalesDetailPage_Liaocheng(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;


            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            int Quatity;
            decimal SumPrice;
            decimal ZengSong;
            decimal ZheRang;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            //entity.numPerPage = 99999999; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "AddTime"; }
            var list = cashBll.GetSalesLiaocheng(entity, out rows, out pagecount, out Quatity, out SumPrice, out ZengSong, out ZheRang);

            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("店名"), new DataColumn("时间"), new DataColumn("品牌"), new DataColumn("分类"), new DataColumn("卡名称"), new DataColumn("项目"), new DataColumn("单价"), new DataColumn("优惠价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("赠送"), new DataColumn("折让"), new DataColumn("折率"), new DataColumn("会员卡号"), new DataColumn("顾客"), new DataColumn("说明") });

            foreach (OrderinfoEntity model in list)
            {

                DataRow row = tableExport.NewRow();
                row["店名"] = model.HospitalName;
                row["时间"] = model.AddTime.ToString("yyyy-MM-dd");
                row["品牌"] = model.BrandName;
                row["分类"] = model.ProductTypeName;
                row["卡名称"] = model.CardName;
                row["项目"] = model.ProjectName;
                row["单价"] = model.OldPrice;
                row["优惠价"] = model.Price;
                row["数量"] = model.Quantity;
                row["合计"] = model.AllPrice;
                row["赠送"] = model.SendAmount;
                row["折让"] = model.DiscountAmount;
                row["折率"] = String.Format("{0:F}", model.OldPrice * model.Quantity != 0 ? model.AllPrice / (model.OldPrice * model.Quantity) * 100 : 0) + "%";
                // row["单据号"] = model.DocumentNumber;
                row["会员卡号"] = model.MemberNo;
                row["顾客"] = model.UserName;
                row["说明"] = model.Memo;

                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["优惠价"] = "合计";
            row1["数量"] = Request["txtQuatity"];
            row1["合计"] = Request["txtZengSong"];
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "PuJilv");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "ExportSalesDetailPage_Liaocheng.xls");

        }
        /// <summary>
        /// 导出产品销售明细
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportSalesDetailPage_ChanPin(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            int Quatity;
            decimal SumPrice;
            decimal ZengSong;
            decimal ZheRang;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            //  entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var list = cashBll.GetSalesChanPin(entity, out rows, out pagecount, out Quatity, out SumPrice, out ZengSong, out ZheRang);


            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("店名"), new DataColumn("时间"), new DataColumn("品牌"), new DataColumn("分类"), new DataColumn("名称"), new DataColumn("单价"), new DataColumn("优惠价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("赠送"), new DataColumn("折让"), new DataColumn("单据号"), new DataColumn("会员卡号"), new DataColumn("顾客"), new DataColumn("说明") });

            foreach (OrderinfoEntity model in list)
            {

                DataRow row = tableExport.NewRow();
                row["店名"] = model.HospitalName;
                row["时间"] = model.CreateTime.ToString("yyyy-MM-dd");
                row["品牌"] = model.BrandName;
                row["分类"] = model.ProductTypeName;
                row["名称"] = model.ProdcuitName;

                row["单价"] = model.OldPrice;
                row["优惠价"] = model.Price;
                row["数量"] = model.Quantity;
                row["合计"] = model.AllPrice;
                row["赠送"] = model.SendAmount;
                row["折让"] = model.DiscountAmount;

                row["单据号"] = model.DocumentNumber;
                row["会员卡号"] = model.MemberNo;
                row["顾客"] = model.UserName;
                row["说明"] = model.Memo;

                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["优惠价"]="合计";
            row1["数量"] = Request["txtQuatity"];
            row1["合计"] = Request["txtSumPrice"];
            row1["赠送"]=Request["txtZengSong"];
            row1["折让"]=Request["txtZheRang"];
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "PuJilv");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "ExportSalesDetailPage_ChanPin.xls");

        }
        /// <summary>
        /// 导出获取单项销售列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportSalesDetailPage_DanXiang(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));

            int rows;
            int pagecount;
            int Quatity;
            decimal SumPrice;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            // entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var list = cashBll.GetSalesDanXiang(entity, out rows, out pagecount, out Quatity, out SumPrice);


            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("店名"), new DataColumn("时间"), new DataColumn("品牌"), new DataColumn("分类"), new DataColumn("名称"), new DataColumn("单价"), new DataColumn("数量"), new DataColumn("合计"), new DataColumn("单据号"), new DataColumn("会员卡号"), new DataColumn("顾客"), new DataColumn("说明") });

            foreach (OrderinfoEntity model in list)
            {

                DataRow row = tableExport.NewRow();
                row["店名"] = model.HospitalName;
                row["时间"] = model.CreateTime.ToString("yyyy-MM-dd");
                row["品牌"] = model.BrandName;
                row["分类"] = model.ProductTypeName;
                row["名称"] = model.ProdcuitName;

                row["单价"] = (model.AllPrice / model.Quantity).ToString("f2");

                row["数量"] = model.Quantity;
                row["合计"] = model.AllPrice;

                row["单据号"] = model.DocumentNumber;
                row["会员卡号"] = model.MemberNo;
                row["顾客"] = model.UserName;
                row["说明"] = model.Memo;

                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["单价"] = "合计";
            row1["数量"] = Request["txtQuatity"];
            row1["合计"] = Request["txtSumPrice"];
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "PuJilv");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "ExportSalesDetailPage_DanXiang.xls");
        }

        /// <summary>
        /// 导出还款明细
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult ExportSalesDetailPage_HuanKuan(_SeachEntity entity)
        {
            //如果是第一次进来,这个ID则显示是0,那么就把它赋值给他本身的门店ID
            if (entity.s_HospitalID == 0)
            {
                entity.s_HospitalID = LoginUserEntity.HospitalID;
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:01")) : Convert.ToDateTime(entity.s_StareTime.ToString("yyyy-MM-dd 00:00:01"));
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")) : Convert.ToDateTime(entity.s_Endtime.ToString("yyyy-MM-dd 23:59:59"));


            // entity.s_TableType = 1;
            int rows;
            int pagecount;
            decimal SumPrice;
            entity.orderDirection = entity.orderDirection == "" ? "desc" : entity.orderDirection;
            //entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "CreateTime"; }

            var list = cashBll.GetSalesHuanKuan(entity, out rows, out pagecount, out SumPrice);


            DataTable tableExport = new DataTable("Export");
            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("店名"), new DataColumn("时间"), new DataColumn("还款单号"), new DataColumn("金额"), new DataColumn("单据号"), new DataColumn("会员卡号"), new DataColumn("顾客"), new DataColumn("说明") });

            foreach (OrderinfoEntity model in list)
            {

                DataRow row = tableExport.NewRow();
                row["店名"] = model.HospitalName;
                row["时间"] = model.CreateTime.ToString("yyyy-MM-dd");

                row["还款单号"] = model.ProdcuitName;
                row["金额"] = model.Price;

                row["单据号"] = model.DocumentNumber;
                row["会员卡号"] = model.MemberNo;
                row["顾客"] = model.UserName;
                row["说明"] = model.Memo;

                tableExport.Rows.Add(row);
            }
            DataRow row1 = tableExport.NewRow();
            row1["还款单号"] = "合计";
            row1["金额"] = Request["txtSumPrice"];
            tableExport.Rows.Add(row1);
            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "PuJilv");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "ExportSalesDetailPage_HuanKuan.xls");
        }

        #endregion

        #region ==目标管理==
        /// <summary>
        /// 目标列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult GoalsList(PerformanceGoalsEntity entity)
        {
            int rows;
            int pagecount;
            entity.numPerPage = 50; //每页显示条数
            entity.Years = entity.Years > 1900 ? entity.Years : DateTime.Now.Year;
            if (!Request.IsAjaxRequest())
                entity.Months = DateTime.Now.Month.ToString().PadLeft(2, '0');
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            //entity.GoalsType = 1;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;

            var list = iCentBll.GetPerformanceGoalsList(entity, out rows, out pagecount);
            foreach (var info in list)
            {
                if (info.GoalsType == 2)
                {
                    info.UserName = info.HospitalName + "门店";
                }
                if (info.GoalsType == 3)
                {
                    info.UserName = info.Name;
                }
            } 
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;

            if (Request.IsAjaxRequest())
                return PartialView("_GoalsList", result);
            return View(result);
        }
        //公司目标
        public ActionResult GoalsListcompany(PerformanceGoalsEntity entity)
        {
            int rows;
            int pagecount;
            entity.numPerPage = 50; //每页显示条数
            entity.GoalsType = 2;
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            var list = iCentBll.GetPerformanceGoalsList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_GoalsListcompany", result);
            return View(result);
        }
        public ActionResult GoalEdit()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            return View();
        }
        public ActionResult EditGoals(PerformanceGoalsEntity entity)
        {
            //entity.HospitalID = entity.HospitalID == 0 ? loginUserEntity.HospitalID : entity.HospitalID;
            //判断是否有相同数据.查询本年本月是否有过目标单据
            if (!IsOnlyOneGoals(entity))
            {
                return Json(-1);
            }
            ////判断日期是否可用
            //var todayyear = DateTime.Now.Year;
            //var todaymonth = DateTime.Now.Month;
            //if (entity.Years < todayyear)
            //{
            //    return Json(-2);
            //}
            //if (Convert.ToInt32(entity.Months) < todaymonth && entity.Years == todayyear)
            //{
            //    return Json(-2);
            //}


            var result = 0;
            var Usermodel = personnelBLL.GetUserByUserID(entity.UserID);
            var HospitalModel = personnelBLL.HospitalEntityByID(entity.HospitalID);
            string sqlstr = "select Name from EYB_tb_Department where ID =" + entity.DepartmentID;
            var departName = GetNameByID(sqlstr);
            string txtname;
            if (entity.DepartmentID > 0) {
                txtname =  departName ;
            } else { 
            txtname = entity.GoalsType == 1 ? entity.Years.ToString() + "年" + entity.Months + "月员工" + Usermodel.UserName + "目标" : entity.Years.ToString() + "年" + entity.Months + "月" + HospitalModel.Name + "门店目标";
            }
            //本月一共有几天
            int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(entity.Years, Convert.ToInt32(entity.Months));
            entity.AverageProduct = Math.Round(entity.ProductConsumption / monthDays, 2);
            entity.AverageProjects = Math.Round(entity.ProjectsConsumption / monthDays, 2);
            entity.AverageShiHao = Math.Round(entity.ShiHao / monthDays, 2);
            entity.AverageTraffic = Math.Round(Convert.ToDecimal(entity.Traffic) / monthDays, 2);
            entity.AverageXianjin = Math.Round(entity.Xianjin / monthDays, 2);
            entity.AverageNewCustomer = Math.Round(Convert.ToDecimal(entity.NewCustomerNumber) / monthDays, 2);
            entity.Name = entity.Name + "" == "" ? txtname : entity.UserName;
            entity.LastUpdateTime = DateTime.Now;
            if (entity.ID == 0)
            {
                entity.CreateTime = DateTime.Now;
                result = iCentBll.AddPerformanceGoals(entity);
            }
            else
            {
                result = iCentBll.UpdatePerformanceGoals(entity);
            }
            return Json(result);
        }
        private string GetNameByID(string sqlstr, string filter = "")
        {

            string month = "0";
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
                    month = date["Name"].ToString();
                }
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return month;

        }
        /// <summary>
        /// 目标唯一性判断
        /// 唯一性判断需要传入的是
        /// 门店ID
        /// 年
        /// 月
        /// 用户ID
        /// 类别
        /// 注意,名称不传
        /// </summary>
        /// <param name="enttity"></param>
        /// <returns></returns>
        private bool IsOnlyOneGoals(PerformanceGoalsEntity entity)
        {
            int rows;
            int pagecount;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.Name = null;//避免名字判断弄乱查询结果
            var list = iCentBll.GetPerformanceGoalsList(entity, out rows, out pagecount);
            list = list.Where(i => i.ID != entity.ID).ToList();
            if (entity.ID != 0)
            {
                list = list.Where(i => i.ID != entity.ID).ToList();
            }
            if (list.Count() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 获取用户下拉列表
        /// ajax请求
        /// 需要验证账户实体类是否为空,为空则不给数据
        /// </summary>
        /// <param name="HospitalID"></param>
        /// <returns></returns>
        public ActionResult GetUserList(int HospitalID)
        {
            if (HospitalID == 0 || LoginUserEntity == null)
            {
                return Json(null);
            }
            var list = personnelBLL.GetAllUserByHospitalID(HospitalID, 0);
            StringBuilder stb = new StringBuilder();
            stb.AppendFormat("<option value='0'>选择员工</option>");
            foreach (var info in list)
            {
                stb.AppendFormat("<option value='{0}'>{1}</option>", info.UserID, info.UserName);
            }
            return Json(stb.ToString());
        }
        public ActionResult PerformanceGoals()
        {
            return View();
        }
        /// <summary>
        /// 目标完成度
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult GoalsProgress(int ID)
        {
            StringBuilder sb = new StringBuilder();
            var GoalModel = iCentBll.GetPerformanceGoalsModel(ID);
            _SeachEntity entity = new _SeachEntity();
            entity.s_StareTime = Convert.ToDateTime(GoalModel.Years + "-" + GoalModel.Months + "-01 00:00:01");
            int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(GoalModel.Years, Convert.ToInt32(GoalModel.Months));
            entity.s_Endtime = Convert.ToDateTime(GoalModel.Years + "-" + GoalModel.Months + "-" + monthDays + " 23:59:59");//获取当月最后一天
            entity.s_UserID = GoalModel.UserID;
            var list = iCentBll.GetUserGoalsinfo(entity);
            decimal xianjin = list.Sum(i => i.Yeji);
            //                       --这个是项目划卡(消耗)           --单项项目                      ---产品销售                ---卡扣产品                         ---卡扣项目
            decimal shihao = list.Sum(i => i.XiaoHao_Liaocheng) + list.Sum(i => i.Yeji_Xiangmu) + list.Sum(i => i.Yeji_Chanpin) + list.Sum(i => i.KakouYeji_Chanpin) + list.Sum(i => i.KakouYeji_Xiangmu);



            //var newlist = list.GroupBy(i => i.CreateTime).ToList();
            decimal SumXianJin = 0;
            decimal SumShiHao = 0;
            int SumTraffic = 0;
            int num = 1;
            for (int j = 1; j <= monthDays; j++)
            {
                //foreach (var info in list)
                //{
                //找到今天的单据
                var todayline = list.Where(i => i.Ctime == GoalModel.Years + "-" + GoalModel.Months + "-" + num.ToString().PadLeft(2, '0')).ToList();
                if (todayline.Count == 0)
                { sb.AppendFormat("<tr style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;'><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;'>{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;'>{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}<td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td></tr>", num, 0, 0, 0, 0, 0, 0, 0, 0, 0); }
                else
                {
                    var xj = todayline.Sum(i => i.Yeji);
                    //var sh = todayline.Sum(i => i.XiaoHao_Liaocheng) + todayline.Sum(i => i.Yeji_Xiangmu) + todayline.Sum(i => i.Yeji_Chanpin) + todayline.Sum(i => i.KakouYeji_Chanpin) + todayline.Sum(i => i.XiaoHao_Liaocheng) + list.Sum(i => i.KakouYeji_Xiangmu);
                    var sh = todayline.Sum(i => i.ShiCao);
                    List<int> userList = new List<int>();
                    foreach (var uinfo in todayline)
                    {
                        userList.Add(uinfo.OUserID);
                    }
                    userList = userList.Distinct().ToList();
                    var tf = userList.Count;
                    SumXianJin += xj;
                    SumShiHao += sh;
                    SumTraffic += tf;
                    //sb.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;'>{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{10}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{11}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{12}</td></tr>", num, xj, SumXianJin, (SumXianJin / GoalModel.Xianjin).ToString("P"), sh, SumShiHao, (SumShiHao / GoalModel.ShiHao).ToString("P"), tf, SumTraffic, (GoalModel.Traffic == 0 ? 0 : Convert.ToDecimal(SumTraffic) / GoalModel.Traffic).ToString("P"), GoalModel.Xianjin - SumXianJin, GoalModel.ShiHao - SumShiHao, GoalModel.Traffic - SumTraffic);
                    sb.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;'>{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{10}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{11}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{12}</td></tr>",
                        num, xj, SumXianJin, (GoalModel.Xianjin==0 ? 0 : (SumXianJin / GoalModel.Xianjin)).ToString("P"), sh, SumShiHao, (GoalModel.ShiHao==0 ? 0:(SumShiHao / GoalModel.ShiHao)).ToString("P"), tf, SumTraffic, (GoalModel.Traffic == 0 ? 0 : Convert.ToDecimal(SumTraffic) / GoalModel.Traffic).ToString("P"), GoalModel.Xianjin - SumXianJin, GoalModel.ShiHao - SumShiHao, GoalModel.Traffic - SumTraffic);
                    //先要找到算出今日现金,今日实耗,今日客流量.
                    //需要一个字段保存累计现金,累积实耗,累积流量.
                    //再通过总量计算出完成量,然后添加到文字排序内
                    //添加完成后赋值给一个实体.然后前台调取.
                }
                if (j == monthDays)
                {
                    break;
                }
                num++;
                //}
            }
            int traffic = SumTraffic;
            GoalModel.StrHtml = sb.ToString();
            GoalModel.AccumulativeShiHao = SumShiHao;
            GoalModel.AccumulativeXianJin = SumXianJin;
            GoalModel.AccumulativeTraffic = SumTraffic;
            //获取当日客人数量
            return View(GoalModel);
        }
        private string GetIntegralByKeliu(string sqlstr, string filter = "")
        {

            string month = "0";
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
                    month = date["keliu"].ToString();
                }
                dr.Close();//关闭DataReader对象  
            }
            catch
            {

            }
            return month;

        }
        /// <summary>
        /// 目标完成度
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult GoalsProgress3(int ID)
        {
            StringBuilder sb = new StringBuilder();
            var GoalModel = iCentBll.GetPerformanceGoalsModel(ID);
            _SeachEntity entity = new _SeachEntity();
            entity.s_StareTime = Convert.ToDateTime(GoalModel.Years + "-" + GoalModel.Months + "-01 00:00:01");
            int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(GoalModel.Years, Convert.ToInt32(GoalModel.Months));
            entity.s_Endtime = Convert.ToDateTime(GoalModel.Years + "-" + GoalModel.Months + "-" + monthDays + " 23:59:59");//获取当月最后一天
            entity.s_UserID = GoalModel.UserID;
            var list = iCentBll.GetUserGoalsinfo(entity);
            decimal xianjin = list.Sum(i => i.Yeji);
            //                       --这个是项目划卡(消耗)           --单项项目                      ---产品销售                ---卡扣产品                         ---卡扣项目
            decimal shihao = list.Sum(i => i.XiaoHao_Liaocheng) + list.Sum(i => i.Yeji_Xiangmu) + list.Sum(i => i.Yeji_Chanpin) + list.Sum(i => i.KakouYeji_Chanpin) + list.Sum(i => i.KakouYeji_Xiangmu);



            //var newlist = list.GroupBy(i => i.CreateTime).ToList();
            decimal SumXianJin = 0;
            decimal SumShiHao = 0;
            //int SumTraffic = 0;
            decimal SumTraffic = 0;
            int num = 1;
            for (int j = 1; j <= monthDays; j++)
            {
                //foreach (var info in list)
                //{
                //找到今天的单据
                var todayline = list.Where(i => i.Ctime == GoalModel.Years + "-" + GoalModel.Months + "-" + num.ToString().PadLeft(2, '0')).ToList();
                var _today = GoalModel.Years + "-" + GoalModel.Months + "-" + num.ToString().PadLeft(2, '0');
                var _todaystar = GoalModel.Years + "-" + GoalModel.Months + "-" + num.ToString().PadLeft(2, '0') + " 00:00:01";
                var _todayend = GoalModel.Years + "-" + GoalModel.Months + "-" + num.ToString().PadLeft(2, '0') + " 23:59:59";
                if (todayline.Count == 0)
                { sb.AppendFormat("<tr style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;'><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;'>{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;'>{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}<td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td></tr>", num, 0, 0, 0, 0, 0, 0, 0, 0, 0); }
                else
                {
                    var xj = todayline.Sum(i => i.Yeji);
                    //var sh = todayline.Sum(i => i.XiaoHao_Liaocheng) + todayline.Sum(i => i.Yeji_Xiangmu) + todayline.Sum(i => i.Yeji_Chanpin) + todayline.Sum(i => i.KakouYeji_Chanpin) + todayline.Sum(i => i.XiaoHao_Liaocheng) + list.Sum(i => i.KakouYeji_Xiangmu);
                    var sh = todayline.Sum(i => i.ShiCao);
                    //List<int> userList = new List<int>();
                    //foreach (var uinfo in todayline)
                    //{
                    //    userList.Add(uinfo.OUserID);
                    //}
                    //userList = userList.Distinct().ToList();
                    var sql = @"SELECT
	                            COUNT (0) as keliu
                            FROM
	                            (
		                            SELECT
			                            SUBSTRING (A.OrderNo, 0, 9) AS tm,
			                            A.UserID,
			                            b.UserID AS Bid
		                            FROM
			                            Eyb_tb_Joinuser A
		                            INNER JOIN EYB_tb_order b ON A.Orderno = b.orderNo
		                            AND b.HospitalID = "+ GoalModel.HospitalID + @"
		                            AND b.Statu = 1
		                            AND b.IsArchives = 0
		                            AND b.CreateTime BETWEEN '"+ _todaystar + @"' AND '" + _todayend + @"'
		                            INNER JOIN dbo.EYB_tb_Patient c ON b.UserID = c.UserID
		                            INNER JOIN Eyb_tb_OrderInfo d ON A.Orderno = d.Orderno
		                            AND (
			                            d.infobuytype = 3
			                            OR d.infobuytype = 5
		                            )
		                            WHERE
			                            A.UserID = "+ GoalModel.UserID + @"
		                            AND A.ID <> 2845338
		                            AND A.ID <> 2845341 
		                            GROUP BY
			                            SUBSTRING (A.OrderNo, 0, 9),
			                            A.UserID,
			                            b.UserID
	                            ) AS t";
                    var fuwukeiliu = string.IsNullOrEmpty(GetIntegralByKeliu(sql))?(decimal)0:Convert.ToDecimal(GetIntegralByKeliu(sql));
                    var sqljiangqu = @"SELECT SUM(CASE WHEN ISNULL(Keliu,0.00) = 0.00 THEN 0.00        
							                                     WHEN ISNULL(Keliu,0.00) < 1 THEN 1.00        
							                                     WHEN ISNULL(Keliu,0.00) > 1 THEN 1.00        
					                                    END) AS keliu        
                                    FROM   dbo.EYB_tb_Order b        
                                    INNER JOIN dbo.EYB_tb_JoinUser j        
                                    ON     b.OrderNo = j.OrderNo AND        
			                                    j.UserID = " + GoalModel.UserID + @"       
                                    WHERE  b.Statu = 1 AND        
			                                    b.HospitalID = " + GoalModel.HospitalID + @" AND 
			                                    b.CreateTime BETWEEN '" + _todaystar + @"' AND '" + _todayend + @"'";
                    var jiangqukeliu = string.IsNullOrEmpty(GetIntegralByKeliu(sqljiangqu)) ? (decimal)0 : Convert.ToDecimal(GetIntegralByKeliu(sqljiangqu));
                    var hulikeliu = fuwukeiliu - jiangqukeliu < 0 ? 0 : fuwukeiliu - jiangqukeliu;
                    var duorenkeliusql = @"SELECT SUM(ISNULL(j.Keliu,0)) AS keliu         
                                           FROM   dbo.EYB_tb_Order b        
                                           INNER JOIN dbo.EYB_tb_JoinUser j        
                                           ON     b.OrderNo = j.OrderNo AND        
                                                  j.UserID =  " + GoalModel.UserID + @"           
                                           WHERE  
                                                  b.Statu = 1 AND        
                                                  b.IsArchives = 0 AND        
                                                  b.CreateTime BETWEEN '" + _todaystar + @"' AND '" + _todayend + @"'";

                    var duorenliu = string.IsNullOrEmpty(GetIntegralByKeliu(duorenkeliusql)) ? (decimal)0 : Convert.ToDecimal(GetIntegralByKeliu(duorenkeliusql));
                    var tf = hulikeliu + duorenliu;//userList.Count; 护理客流 + 多人客流
                    SumXianJin += xj;
                    SumShiHao += sh;
                    SumTraffic += tf;
                    //sb.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;'>{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{10}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{11}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{12}</td></tr>", num, xj, SumXianJin, (SumXianJin / GoalModel.Xianjin).ToString("P"), sh, SumShiHao, (SumShiHao / GoalModel.ShiHao).ToString("P"), tf, SumTraffic, (GoalModel.Traffic == 0 ? 0 : Convert.ToDecimal(SumTraffic) / GoalModel.Traffic).ToString("P"), GoalModel.Xianjin - SumXianJin, GoalModel.ShiHao - SumShiHao, GoalModel.Traffic - SumTraffic);
                    sb.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;'>{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{10}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{11}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{12}</td></tr>",
                        num, xj, SumXianJin, (GoalModel.Xianjin == 0 ? 0 : (SumXianJin / GoalModel.Xianjin)).ToString("P"), sh, SumShiHao, (GoalModel.ShiHao == 0 ? 0 : (SumShiHao / GoalModel.ShiHao)).ToString("P"), tf, SumTraffic, (GoalModel.Traffic == 0 ? 0 : Convert.ToDecimal(SumTraffic) / GoalModel.Traffic).ToString("P"), GoalModel.Xianjin - SumXianJin, GoalModel.ShiHao - SumShiHao, GoalModel.Traffic - SumTraffic);
                    //先要找到算出今日现金,今日实耗,今日客流量.
                    //需要一个字段保存累计现金,累积实耗,累积流量.
                    //再通过总量计算出完成量,然后添加到文字排序内
                    //添加完成后赋值给一个实体.然后前台调取.
                }
                if (j == monthDays)
                {
                    break;
                }
                num++;
                //}
            }
            //int traffic = SumTraffic;
            decimal traffic = SumTraffic;
            GoalModel.StrHtml = sb.ToString();
            GoalModel.AccumulativeShiHao = SumShiHao;
            GoalModel.AccumulativeXianJin = SumXianJin;
            GoalModel.AccumulativeTraffic = SumTraffic;
            //获取当日客人数量
            return View(GoalModel);
        }

        /// <summary>
        /// 目标完成度(公司)
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult GoalsProgressCompany(int ID)
        {
            StringBuilder sb = new StringBuilder();
            var GoalModel = iCentBll.GetPerformanceGoalsModel(ID);
            _SeachEntity entity = new _SeachEntity();
            entity.numPerPage = 10000000;
            entity.s_StareTime = Convert.ToDateTime(GoalModel.Years + "-" + GoalModel.Months + "-01 00:00:01");
            int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(GoalModel.Years, Convert.ToInt32(GoalModel.Months));
            entity.s_Endtime = Convert.ToDateTime(GoalModel.Years + "-" + GoalModel.Months + "-" + monthDays + " 23:59:59");//获取当月最后一天
            entity.s_HospitalID = GoalModel.HospitalID;
            var list = iCentBll.GetUserGoalsinfo(entity);
            decimal xianjin = list.Sum(i => i.Yeji);
            //                       --这个是项目划卡(消耗)           --单项项目                      ---产品销售                ---卡扣产品
            decimal shihao = list.Sum(i => i.XiaoHao_Liaocheng) + list.Sum(i => i.Yeji_Xiangmu) + list.Sum(i => i.Yeji_Chanpin) + list.Sum(i => i.KakouYeji_Chanpin) + list.Sum(i => i.KakouYeji_Xiangmu);

            decimal xiangmu = list.Sum(i => i.Yeji_Xiangmu) + list.Sum(i => i.KakouYeji_Xiangmu) + list.Sum(i => i.XiaoHao_Liaocheng);
            decimal chanpin = list.Sum(i => i.Yeji_Chanpin) + list.Sum(i => i.KakouYeji_Chanpin);
            decimal xinke = list.Count(i => i.Ctime == i.Qtime);
            //var newlist = list.GroupBy(i => i.CreateTime).ToList();
            decimal SumXianJin = 0;
            decimal SumShiHao = 0;
            decimal SumXiangmu = 0;
            decimal SumChanPin = 0;
            int SumNewCustomerNumber = 0;
            int SumTraffic = 0;

            int num = 1;
            for (int j = 1; j <= monthDays; j++)
            {
                //foreach (var info in list)
                //{
                //找到今天的单据
                var todayline = list.Where(i => i.Ctime == GoalModel.Years + "-" + GoalModel.Months + "-" + num.ToString().PadLeft(2, '0')).ToList();
                if (todayline.Count == 0)
                {

                    //    sb.AppendFormat("<tr style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{3}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{4}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{5}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{6}</td>", num, 0, 0, 0, 0, 0, 0);
                    //    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{3}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{4}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{5}</td>", 0, 0, 0, 0, 0, 0);
                    //    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{3}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{4}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{5}</td></tr>", 0, 0, 0, 0, 0, 0);


                    sb.AppendFormat("<tr><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{3}</td>", num, 0, SumXianJin, (SumXianJin * 100 / (GoalModel.Xianjin == 0 ? 1 : GoalModel.Xianjin)).ToString("0.#") + "%");

                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", 0, SumShiHao, (SumShiHao * 100 / (GoalModel.ShiHao == 0 ? 1 : GoalModel.ShiHao)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", 0, SumXiangmu, (SumXiangmu * 100 / (GoalModel.ProjectsConsumption == 0 ? 1 : GoalModel.ProjectsConsumption)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", 0, SumChanPin, (SumChanPin * 100 / (GoalModel.ProductConsumption == 0 ? 1 : GoalModel.ProductConsumption)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", 0, SumTraffic, (Convert.ToDecimal(SumTraffic) * 100 / (GoalModel.Traffic == 0 ? 1 : GoalModel.Traffic)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td></tr>", 0, SumNewCustomerNumber, (Convert.ToDecimal(SumNewCustomerNumber) * 100 / (GoalModel.NewCustomerNumber == 0 ? 1 : GoalModel.NewCustomerNumber)).ToString("0.#") + "%");
                }

                else
                {
                    var xj = todayline.Sum(i => i.Yeji);
                    var sh = todayline.Sum(i => i.XiaoHao_Liaocheng) + todayline.Sum(i => i.Yeji_Xiangmu) + todayline.Sum(i => i.Yeji_Chanpin) + todayline.Sum(i => i.KakouYeji_Chanpin) + list.Sum(i => i.KakouYeji_Xiangmu);
                    var xm = todayline.Sum(i => i.Yeji_Xiangmu) + todayline.Sum(i => i.KakouYeji_Xiangmu) + todayline.Sum(i => i.XiaoHao_Liaocheng);
                    var cp = todayline.Sum(i => i.Yeji_Chanpin) + todayline.Sum(i => i.KakouYeji_Chanpin);
                    var xk = todayline.Count(i => i.Ctime == i.Qtime);
                    List<int> userList = new List<int>();
                    foreach (var uinfo in todayline)
                    {
                        userList.Add(uinfo.OUserID);
                    }
                    userList = userList.Distinct().ToList();
                    var tf = userList.Count;
                    SumXianJin += xj;
                    SumShiHao += sh;
                    SumTraffic += tf;
                    SumXiangmu += xm;
                    SumChanPin += cp;
                    SumNewCustomerNumber += xk;

                    sb.AppendFormat("<tr><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{3}</td>", num, xj, SumXianJin, (SumXianJin * 100 / (GoalModel.Xianjin == 0 ? 1 : GoalModel.Xianjin)).ToString("0.#") + "%");

                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", sh, SumShiHao, (SumShiHao * 100 / (GoalModel.ShiHao == 0 ? 1 : GoalModel.ShiHao)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", xm, SumXiangmu, (SumXiangmu * 100 / (GoalModel.ProjectsConsumption == 0 ? 1 : GoalModel.ProjectsConsumption)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", cp, SumChanPin, (SumChanPin * 100 / (GoalModel.ProductConsumption == 0 ? 1 : GoalModel.ProductConsumption)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", tf, SumTraffic, (Convert.ToDecimal(SumTraffic * 100 / (GoalModel.Traffic == 0 ? 1 : GoalModel.Traffic))).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td></tr>", xk, SumNewCustomerNumber, (Convert.ToDecimal(SumNewCustomerNumber) * 100 / (GoalModel.NewCustomerNumber == 0 ? 1 : GoalModel.NewCustomerNumber)).ToString("0.#") + "%");
                    //先要找到算出今日现金,今日实耗,今日客流量.
                    //需要一个字段保存累计现金,累积实耗,累积流量.
                    //再通过总量计算出完成量,然后添加到文字排序内
                    //添加完成后赋值给一个实体.然后前台调取.
                }
                if (j == monthDays)
                {
                    break;
                }
                num++;
            }
            int traffic = SumTraffic;
            GoalModel.StrHtml = sb.ToString();
            GoalModel.AccumulativeShiHao = SumShiHao;
            GoalModel.AccumulativeXianJin = SumXianJin;
            GoalModel.AccumulativeTraffic = SumTraffic;
            GoalModel.AccumulativeNewCustomerNumber = SumNewCustomerNumber;
            GoalModel.AccumulativeProductConsumption = SumChanPin;
            GoalModel.AccumulativeProjectsConsumption = SumXiangmu;
            //获取当日客人数量
            return View(GoalModel);
        }
        /// <summary>
        /// 目标完成度(公司)
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult GoalsProgressCompany1(int ID)
        {
            StringBuilder sb = new StringBuilder();
            var GoalModel = iCentBll.GetPerformanceGoalsModel(ID);
            _SeachEntity entity = new _SeachEntity();
            entity.numPerPage = 10000000;
            entity.s_StareTime = Convert.ToDateTime(GoalModel.Years + "-" + GoalModel.Months + "-01 00:00:01");
            int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(GoalModel.Years, Convert.ToInt32(GoalModel.Months));
            entity.s_Endtime = Convert.ToDateTime(GoalModel.Years + "-" + GoalModel.Months + "-" + monthDays + " 23:59:59");//获取当月最后一天
            entity.s_HospitalID = GoalModel.HospitalID;
            var list = iCentBll.GetUserGoalsinfo(entity);
            //IList<JoinUserEntity> lists = new List<JoinUserEntity>();
            //foreach (var item in list) {
            //    string strsql = @"SELECT d.NAME AS Name from EYB_tb_HospitalUser h
            //                LEFT JOIN EYB_tb_Department d ON d.ID = h.DepartmentID
            //                WHERE h.UserId = " + item.UserID;
            //    var departName = GetNameByID(strsql);
            //    if (GoalModel.Name == departName) {
            //        lists.Add(item);
            //    }
            //}
            //list = lists;
            decimal xianjin = list.Sum(i => i.Yeji);
            //                       --这个是项目划卡(消耗)           --单项项目                      ---产品销售                ---卡扣产品
            decimal shihao = list.Sum(i => i.XiaoHao_Liaocheng) + list.Sum(i => i.Yeji_Xiangmu) + list.Sum(i => i.Yeji_Chanpin) + list.Sum(i => i.KakouYeji_Chanpin) + list.Sum(i => i.KakouYeji_Xiangmu);

            decimal xiangmu = list.Sum(i => i.Yeji_Xiangmu) + list.Sum(i => i.KakouYeji_Xiangmu) + list.Sum(i => i.XiaoHao_Liaocheng);
            decimal chanpin = list.Sum(i => i.Yeji_Chanpin) + list.Sum(i => i.KakouYeji_Chanpin);
            decimal xinke = list.Count(i => i.Ctime == i.Qtime);
            //var newlist = list.GroupBy(i => i.CreateTime).ToList();
            decimal SumXianJin = 0;
            decimal SumShiHao = 0;
            decimal SumXiangmu = 0;
            decimal SumChanPin = 0;
            int SumNewCustomerNumber = 0;
            int SumTraffic = 0;

            int num = 1;
            for (int j = 1; j <= monthDays; j++)
            {
                //foreach (var info in list)
                //{
                //找到今天的单据
                var todayline = list.Where(i => i.Ctime == GoalModel.Years + "-" + GoalModel.Months + "-" + num.ToString().PadLeft(2, '0')).ToList();
                if (todayline.Count == 0)
                {

                    //    sb.AppendFormat("<tr style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{3}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{4}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{5}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{6}</td>", num, 0, 0, 0, 0, 0, 0);
                    //    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{3}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{4}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{5}</td>", 0, 0, 0, 0, 0, 0);
                    //    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{3}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{4}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{5}</td></tr>", 0, 0, 0, 0, 0, 0);


                    sb.AppendFormat("<tr><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{3}</td>", num, 0, SumXianJin, (SumXianJin * 100 / (GoalModel.Xianjin == 0 ? 1 : GoalModel.Xianjin)).ToString("0.#") + "%");

                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", 0, SumShiHao, (SumShiHao * 100 / (GoalModel.ShiHao == 0 ? 1 : GoalModel.ShiHao)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", 0, SumXiangmu, (SumXiangmu * 100 / (GoalModel.ProjectsConsumption == 0 ? 1 : GoalModel.ProjectsConsumption)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", 0, SumChanPin, (SumChanPin * 100 / (GoalModel.ProductConsumption == 0 ? 1 : GoalModel.ProductConsumption)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", 0, SumTraffic, (Convert.ToDecimal(SumTraffic) * 100 / (GoalModel.Traffic == 0 ? 1 : GoalModel.Traffic)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td></tr>", 0, SumNewCustomerNumber, (Convert.ToDecimal(SumNewCustomerNumber) * 100 / (GoalModel.NewCustomerNumber == 0 ? 1 : GoalModel.NewCustomerNumber)).ToString("0.#") + "%");
                }

                else
                {
                    var xj = todayline.Sum(i => i.Yeji);
                    var sh = todayline.Sum(i => i.XiaoHao_Liaocheng) + todayline.Sum(i => i.Yeji_Xiangmu) + todayline.Sum(i => i.Yeji_Chanpin) + todayline.Sum(i => i.KakouYeji_Chanpin) + list.Sum(i => i.KakouYeji_Xiangmu);
                    var xm = todayline.Sum(i => i.Yeji_Xiangmu) + todayline.Sum(i => i.KakouYeji_Xiangmu) + todayline.Sum(i => i.XiaoHao_Liaocheng);
                    var cp = todayline.Sum(i => i.Yeji_Chanpin) + todayline.Sum(i => i.KakouYeji_Chanpin);
                    var xk = todayline.Count(i => i.Ctime == i.Qtime);
                    List<int> userList = new List<int>();
                    foreach (var uinfo in todayline)
                    {
                        userList.Add(uinfo.OUserID);
                    }
                    userList = userList.Distinct().ToList();
                    var tf = userList.Count;
                    SumXianJin += xj;
                    SumShiHao += sh;
                    SumTraffic += tf;
                    SumXiangmu += xm;
                    SumChanPin += cp;
                    SumNewCustomerNumber += xk;

                    sb.AppendFormat("<tr><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{3}</td>", num, xj, SumXianJin, (SumXianJin * 100 / (GoalModel.Xianjin == 0 ? 1 : GoalModel.Xianjin)).ToString("0.#") + "%");

                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", sh, SumShiHao, (SumShiHao * 100 / (GoalModel.ShiHao == 0 ? 1 : GoalModel.ShiHao)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", xm, SumXiangmu, (SumXiangmu * 100 / (GoalModel.ProjectsConsumption == 0 ? 1 : GoalModel.ProjectsConsumption)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", cp, SumChanPin, (SumChanPin * 100 / (GoalModel.ProductConsumption == 0 ? 1 : GoalModel.ProductConsumption)).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td>", tf, SumTraffic, (Convert.ToDecimal(SumTraffic * 100 / (GoalModel.Traffic == 0 ? 1 : GoalModel.Traffic))).ToString("0.#") + "%");
                    sb.AppendFormat("<td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{0}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;'>{1}</td><td style=' line-height:24px;  text-align:center; border:1px #ccc solid; font-size:13px;' >{2}</td></tr>", xk, SumNewCustomerNumber, (Convert.ToDecimal(SumNewCustomerNumber) * 100 / (GoalModel.NewCustomerNumber == 0 ? 1 : GoalModel.NewCustomerNumber)).ToString("0.#") + "%");
                    //先要找到算出今日现金,今日实耗,今日客流量.
                    //需要一个字段保存累计现金,累积实耗,累积流量.
                    //再通过总量计算出完成量,然后添加到文字排序内
                    //添加完成后赋值给一个实体.然后前台调取.
                }
                if (j == monthDays)
                {
                    break;
                }
                num++;
            }
            int traffic = SumTraffic;
            GoalModel.StrHtml = sb.ToString();
            GoalModel.AccumulativeShiHao = SumShiHao;
            GoalModel.AccumulativeXianJin = SumXianJin;
            GoalModel.AccumulativeTraffic = SumTraffic;
            GoalModel.AccumulativeNewCustomerNumber = SumNewCustomerNumber;
            GoalModel.AccumulativeProductConsumption = SumChanPin;
            GoalModel.AccumulativeProjectsConsumption = SumXiangmu;
            //获取当日客人数量
            return View(GoalModel);
        }
        #region ==目标状态原来的代码(暂时先保留)==

        /// <summary>
        /// 目标状态
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult GoalsState1(PerformanceGoalsEntity entity)
        {
            GoalsStateEntity ge;
            entity.Years = entity.Years < 1970 ? DateTime.Now.Year : entity.Years;
            entity.Months = entity.Months == null ? DateTime.Now.Month.ToString().PadLeft(2, '0') : entity.Months;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.GoalsType = entity.GoalsType == 0 ? 1 : entity.GoalsType;
            entity.BusinessClass = entity.BusinessClass == 0 ? 1 : entity.BusinessClass;
            //用户用户列表


            int rows;
            int pagecount;
            entity.numPerPage = 50; //每页显示条数
            //获取目标状态
            var GoalsList = iCentBll.GetPerformanceGoalsList(entity, out rows, out pagecount);
            //员工目标管理
            if (entity.GoalsType == 1)
            {
                var UserList = personnelBLL.GetAllUserByHospitalID(entity.HospitalID, 0);

                ge = new GoalsStateEntity(UserList.Count, UserList.Count, UserList.Count);

                ge.GoalsType = entity.GoalsType;
                ge.TName = "目标状态分析";

                var BusinessClassName = entity.BusinessClass == 1 ? "现金" : (entity.BusinessClass == 2 ? "实耗" : "客流量");
                ge.TTitle = entity.Years + "年" + entity.Months + "月" + personnelBLL.HospitalEntityByID(entity.ID).Name + BusinessClassName + "分析";

                //遍历用户
                int n = 0;
                foreach (var userinfo in UserList)
                {
                    var MyGoals = GoalsList.Where(i => i.UserID == userinfo.UserID).ToList();

                    ge.TxValue[n] = userinfo.UserName;
                    if (MyGoals.Count > 0)
                    {
                        ge.TyValues0[n] = entity.BusinessClass == 1 ? MyGoals[0].Xianjin.ToString() : (entity.BusinessClass == 2 ? MyGoals[0].ShiHao.ToString() : (entity.BusinessClass == 3 ? MyGoals[0].Traffic.ToString() : "0"));
                    }
                    _SeachEntity _s = new _SeachEntity();
                    _s.s_StareTime = Convert.ToDateTime(entity.Years + "-" + entity.Months + "-01 00:00:01");
                    int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(entity.Years, Convert.ToInt32(entity.Months));
                    _s.s_Endtime = Convert.ToDateTime(entity.Years + "-" + entity.Months + "-" + monthDays + " 23:59:59");//获取当月最后一天
                    _s.s_UserID = userinfo.UserID;
                    var UserGoalslist = iCentBll.GetUserGoalsinfo(_s);
                    if (entity.BusinessClass == 1)
                    {
                        ge.TyValues1[n] = UserGoalslist.Sum(i => i.Yeji).ToString();
                    }
                    else if (entity.BusinessClass == 2)
                    {
                        ge.TyValues1[n] = (UserGoalslist.Sum(i => i.XiaoHao_Liaocheng) + UserGoalslist.Sum(i => i.Yeji_Xiangmu) + UserGoalslist.Sum(i => i.Yeji_Chanpin) + UserGoalslist.Sum(i => i.KakouYeji_Chanpin) + UserGoalslist.Sum(i => i.KakouYeji_Xiangmu)).ToString();

                    }
                    else if (entity.BusinessClass == 3)
                    {
                        int alltf = 0;
                        for (int j = 1; j <= monthDays; j++)
                        {
                            //找到今天的单据
                            var todayline = UserGoalslist.Where(i => i.Ctime == entity.Years + "-" + entity.Months + "-" + j.ToString().PadLeft(2, '0')).ToList();
                            if (todayline.Count > 0)
                            {
                                List<int> userList = new List<int>();
                                foreach (var uinfo in todayline)
                                {
                                    userList.Add(uinfo.OUserID);
                                }
                                userList = userList.Distinct().ToList();
                                var tf = userList.Count;
                                alltf += tf;
                            }
                        }
                        ge.TyValues1[n] = alltf.ToString();
                    }
                    n++;
                }
            }
            else//下面是公司
            {
                ge = new GoalsStateEntity(6, 6, 0);
                ge.GoalsType = entity.GoalsType;
                ge.TName = "目标状态分析";
                ge.TTitle = entity.Years + "年" + entity.Months + "月" + personnelBLL.HospitalEntityByID(entity.ID).Name + "分析";


                ge.TxValue[0] = "现金";
                ge.TxValue[1] = "实耗";
                ge.TxValue[2] = "客流量";
                ge.TxValue[3] = "项目";
                ge.TxValue[4] = "产品";
                ge.TxValue[5] = "新客";
                if (GoalsList.Count == 0)
                {
                    ge.TyValues0[0] = "0";
                    ge.TyValues0[1] = "0";
                    ge.TyValues0[2] = "0";
                    ge.TyValues0[3] = "0";
                    ge.TyValues0[4] = "0";
                    ge.TyValues0[5] = "0";
                }
                else
                {
                    ge.TyValues0[0] = GoalsList[0].Xianjin.ToString();
                    ge.TyValues0[1] = GoalsList[0].ShiHao.ToString();
                    ge.TyValues0[2] = GoalsList[0].Traffic.ToString();
                    ge.TyValues0[3] = GoalsList[0].ProjectsConsumption.ToString();
                    ge.TyValues0[4] = GoalsList[0].ProductConsumption.ToString();
                    ge.TyValues0[5] = GoalsList[0].NewCustomerNumber.ToString();
                }
                _SeachEntity _s = new _SeachEntity();
                _s.s_StareTime = Convert.ToDateTime(entity.Years + "-" + entity.Months + "-01 00:00:01");
                int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(entity.Years, Convert.ToInt32(entity.Months));
                _s.s_Endtime = Convert.ToDateTime(entity.Years + "-" + entity.Months + "-" + monthDays + " 23:59:59");//获取当月最后一天
                _s.s_HospitalID = entity.HospitalID;
                var UserGoalslist = iCentBll.GetUserGoalsinfo(_s);
                decimal xianjin = UserGoalslist.Sum(i => i.Yeji);
                //                       --这个是项目划卡(消耗)           --单项项目                      ---产品销售                ---卡扣产品
                decimal shihao = UserGoalslist.Sum(i => i.XiaoHao_Liaocheng) + UserGoalslist.Sum(i => i.Yeji_Xiangmu) + UserGoalslist.Sum(i => i.Yeji_Chanpin) + UserGoalslist.Sum(i => i.KakouYeji_Chanpin) + UserGoalslist.Sum(i => i.KakouYeji_Xiangmu);

                decimal xiangmu = UserGoalslist.Sum(i => i.Yeji_Xiangmu) + UserGoalslist.Sum(i => i.KakouYeji_Xiangmu) + UserGoalslist.Sum(i => i.XiaoHao_Liaocheng);
                decimal chanpin = UserGoalslist.Sum(i => i.Yeji_Chanpin) + UserGoalslist.Sum(i => i.KakouYeji_Chanpin);
                decimal xinke = UserGoalslist.Count(i => i.Ctime == i.Qtime);
                int alltf = 0;
                for (int j = 1; j <= monthDays; j++)
                {

                    //找到今天的单据
                    var todayline = UserGoalslist.Where(i => i.Ctime == entity.Years + "-" + entity.Months + "-" + j.ToString().PadLeft(2, '0')).ToList();
                    if (todayline.Count == 0)
                    { }
                    else
                    {
                        List<int> userList = new List<int>();
                        foreach (var uinfo in todayline)
                        {
                            userList.Add(uinfo.OUserID);
                        }
                        userList = userList.Distinct().ToList();
                        var tf = userList.Count;
                        alltf += tf;
                    }
                }

                ge.TyValues1[0] = xianjin.ToString();
                ge.TyValues1[1] = shihao.ToString();
                ge.TyValues1[2] = alltf.ToString();
                ge.TyValues1[3] = xiangmu.ToString();
                ge.TyValues1[4] = chanpin.ToString();
                ge.TyValues1[5] = xinke.ToString();
            }




            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;

            if (Request.IsAjaxRequest())
                return PartialView("_GoalsState", ge);
            return View(ge);
        }
        #endregion
        /// <summary>
        /// 目标状态
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult GoalsState(PerformanceGoalsEntity entity)
        {
            //这里是处理第一次进来时的程序处理
            entity.Years = entity.Years < 1970 ? DateTime.Now.Year : entity.Years;
            entity.Months = entity.Months == null ? DateTime.Now.Month.ToString().PadLeft(2, '0') : entity.Months;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.GoalsType = entity.GoalsType == 0 ? 1 : entity.GoalsType;
            entity.BusinessClass = entity.BusinessClass == 0 ? 1 : entity.BusinessClass;
            entity.numPerPage = 50; //每页显示条数
            //标题
            string title = null;
            //副标题
            string subTitle = null;
            //底栏数据(员工的话是业绩)(公司是类别)
            List<string> _series_serialize = new List<string>();
            //获取用户集合
            var UserList = personnelBLL.GetAllUserByHospitalID(entity.HospitalID, 0);
            //这个是数据组装几个
            List<seriesList> _series_list = new List<seriesList>();
            if (entity.GoalsType == 1)
            {
                GetUserGoalsSeries(entity, ref _series_list, ref title, ref subTitle, ref _series_serialize);
            }
            else
            {
                GetCompanyGoalsSeries(entity, ref _series_list, ref title, ref subTitle, ref _series_serialize);
            }
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            //json转换
            JavaScriptSerializer _ser = new JavaScriptSerializer();
            //配置列表
            ChartConfigEntity chartConfig_list = new ChartConfigEntity()
            {
                Type = "column",
                Title = title,
                Subtitle = subTitle,
                XAxis = new xAxis() { Title = "月份", CategoriesJson = _ser.Serialize(_series_serialize) },
                YAxis = new yAxis() { Title = "( 元 )" },
                SeriesJson = _ser.Serialize(_series_list)
            };
            if (Request.IsAjaxRequest())
                return PartialView("_GoalsState", chartConfig_list);
            return View(chartConfig_list);
        }
        /// <summary>
        /// 获取用户数据和统计图标题
        /// </summary>
        /// <param name="_entity">条件数据</param>
        /// <param name="_series_list">返回的用户数据</param>
        /// <param name="_title">返回的标题</param>
        /// <param name="_series_serialize">返回的用户列表</param>
        public void GetUserGoalsSeries(PerformanceGoalsEntity _entity, ref List<seriesList> _series_list, ref string _title, ref string _subtitle, ref List<string> _series_serialize)
        {
            List<Double> yuji = new List<double>();
            List<Double> dangqian = new List<double>();
            //获取用户集合
            var UserList = personnelBLL.GetAllUserByHospitalID(_entity.HospitalID, 0);
            _SeachEntity _s = new _SeachEntity();
            _s.s_StareTime = Convert.ToDateTime(_entity.Years + "-" + _entity.Months + "-01 00:00:01");
            int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(_entity.Years, Convert.ToInt32(_entity.Months));
            _s.s_Endtime = Convert.ToDateTime(_entity.Years + "-" + _entity.Months + "-" + monthDays + " 23:59:59");//获取当月最后一天
            string ClassName = null;
            int rows;
            int pagecount;
            var GoalsList = iCentBll.GetPerformanceGoalsList(_entity, out rows, out pagecount);
            //遍历用户
            foreach (var userinfo in UserList)
            {
                _series_serialize.Add(userinfo.UserName);//添加用户列表
                var MyGoals = GoalsList.Where(i => i.UserID == userinfo.UserID).ToList();
                if (MyGoals.Count > 0)
                {
                    yuji.Add(RetnunvalByClass(MyGoals[0], _entity.BusinessClass, ref ClassName));
                }
                else
                {
                    yuji.Add(0);
                }
                _s.s_UserID = userinfo.UserID;
                var UserGoalslist = iCentBll.GetUserGoalsinfo(_s);
                if (_entity.BusinessClass == 1)
                {
                    dangqian.Add(Convert.ToDouble(UserGoalslist.Sum(i => i.Yeji)));
                }
                else if (_entity.BusinessClass == 2)
                {
                    dangqian.Add(Convert.ToDouble(UserGoalslist.Sum(i => i.XiaoHao_Liaocheng) + UserGoalslist.Sum(i => i.Yeji_Xiangmu) + UserGoalslist.Sum(i => i.Yeji_Chanpin) + UserGoalslist.Sum(i => i.KakouYeji_Chanpin) + UserGoalslist.Sum(i => i.KakouYeji_Xiangmu)));
                }
                else if (_entity.BusinessClass == 3)
                {
                    int alltf = 0;
                    for (int j = 1; j <= monthDays; j++)
                    {
                        //找到今天的单据
                        var todayline = UserGoalslist.Where(i => i.Ctime == _entity.Years + "-" + _entity.Months + "-" + j.ToString().PadLeft(2, '0')).ToList();
                        if (todayline.Count > 0)
                        {
                            List<int> userList = new List<int>();
                            foreach (var uinfo in todayline)
                            {
                                userList.Add(uinfo.OUserID);
                            }
                            userList = userList.Distinct().ToList();
                            var tf = userList.Count;
                            alltf += tf;
                        }
                    }
                    dangqian.Add(alltf);
                }
            }
            seriesList _series = new seriesList()
            {
                name = "设定目标",
                data = yuji
            };
            _series_list.Add(_series);
            seriesList _series1 = new seriesList()
            {
                name = "完成进度",
                data = dangqian
            };
            _series_list.Add(_series1);
            _title = _entity.Years + "年" + _entity.Months + "月" + personnelBLL.HospitalEntityByID(_entity.ID).Name + ClassName + "分析";
            _subtitle = "员工" + ClassName + "分析";
        }
        /// <summary>
        /// 获取公司目标数据
        /// </summary>
        /// <param name="_entity"></param>
        /// <param name="_series_list"></param>
        /// <param name="_title"></param>
        /// <param name="_subtitle"></param>
        /// <param name="_series_serialize"></param>
        public void GetCompanyGoalsSeries(PerformanceGoalsEntity _entity, ref List<seriesList> _series_list, ref string _title, ref string _subtitle, ref List<string> _series_serialize)
        {
            List<Double> yuji = new List<double>();
            List<Double> dangqian = new List<double>();
            int rows;
            int pagecount;
            var GoalsList = iCentBll.GetPerformanceGoalsList(_entity, out rows, out pagecount);
            //算出地下列表
            _series_serialize.Add("现金");
            _series_serialize.Add("实耗");
            _series_serialize.Add("客流量");
            _series_serialize.Add("项目");
            _series_serialize.Add("产品");
            _series_serialize.Add("新客");
            //写入预计数据
            if (GoalsList.Count > 0)
            {
                yuji.Add(Convert.ToDouble(GoalsList[0].Xianjin));
                yuji.Add(Convert.ToDouble(GoalsList[0].ShiHao));
                yuji.Add(GoalsList[0].Traffic);
                yuji.Add(Convert.ToDouble(GoalsList[0].ProjectsConsumption));
                yuji.Add(Convert.ToDouble(GoalsList[0].ProductConsumption));
                yuji.Add(GoalsList[0].NewCustomerNumber);
            }
            else
            {
                yuji.Add(0);
                yuji.Add(0);
                yuji.Add(0);
                yuji.Add(0);
                yuji.Add(0);
                yuji.Add(0);
            }
            _SeachEntity _s = new _SeachEntity();
            _s.s_StareTime = Convert.ToDateTime(_entity.Years + "-" + _entity.Months + "-01 00:00:01");
            int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(_entity.Years, Convert.ToInt32(_entity.Months));
            _s.s_Endtime = Convert.ToDateTime(_entity.Years + "-" + _entity.Months + "-" + monthDays + " 23:59:59");//获取当月最后一天
            _s.s_HospitalID = _entity.HospitalID;
            var UserGoalslist = iCentBll.GetUserGoalsinfo(_s);
            decimal xianjin = UserGoalslist.Sum(i => i.Yeji);
            //                       --这个是项目划卡(消耗)           --单项项目                      ---产品销售                ---卡扣产品
            decimal shihao = UserGoalslist.Sum(i => i.XiaoHao_Liaocheng) + UserGoalslist.Sum(i => i.Yeji_Xiangmu) + UserGoalslist.Sum(i => i.Yeji_Chanpin) + UserGoalslist.Sum(i => i.KakouYeji_Chanpin) + UserGoalslist.Sum(i => i.KakouYeji_Xiangmu);

            decimal xiangmu = UserGoalslist.Sum(i => i.Yeji_Xiangmu) + UserGoalslist.Sum(i => i.KakouYeji_Xiangmu) + UserGoalslist.Sum(i => i.XiaoHao_Liaocheng);
            decimal chanpin = UserGoalslist.Sum(i => i.Yeji_Chanpin) + UserGoalslist.Sum(i => i.KakouYeji_Chanpin);
            decimal xinke = UserGoalslist.Count(i => i.Ctime == i.Qtime);
            int alltf = 0;
            for (int j = 1; j <= monthDays; j++)
            {
                //找到今天的单据
                var todayline = UserGoalslist.Where(i => i.Ctime == _entity.Years + "-" + _entity.Months + "-" + j.ToString().PadLeft(2, '0')).ToList();
                if (todayline.Count == 0)
                { }
                else
                {
                    List<int> userList = new List<int>();
                    foreach (var uinfo in todayline)
                    {
                        userList.Add(uinfo.OUserID);
                    }
                    userList = userList.Distinct().ToList();
                    var tf = userList.Count;
                    alltf += tf;
                }
            }
            dangqian.Add(Convert.ToDouble(xianjin));
            dangqian.Add(Convert.ToDouble(shihao));
            dangqian.Add(alltf);
            dangqian.Add(Convert.ToDouble(xiangmu));
            dangqian.Add(Convert.ToDouble(chanpin));
            dangqian.Add(Convert.ToDouble(xinke));
            seriesList _series = new seriesList()
            {
                name = "设定目标",
                data = yuji
            };
            _series_list.Add(_series);
            seriesList _series1 = new seriesList()
            {
                name = "完成进度",
                data = dangqian
            };
            _series_list.Add(_series1);
            _title = _entity.Years + "年" + _entity.Months + "月" + personnelBLL.HospitalEntityByID(_entity.ID).Name + "分析";
            _subtitle = "门店分析";
        }
        /// <summary>
        /// 返回某个类型的值
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="_businessClass"></param>
        /// <returns></returns>
        public Double RetnunvalByClass(PerformanceGoalsEntity entity, int _businessClass, ref string ClassName)
        {
            switch (_businessClass)
            {
                case 1: ClassName = "现金"; return Convert.ToDouble(entity.Xianjin);
                case 2: ClassName = "实耗"; return Convert.ToDouble(entity.ShiHao);
                case 3: ClassName = "客流量"; return entity.Traffic;
                default: ClassName = "其它"; return 0;
            }
        }
        #endregion
        #region ==目标汇总==
        private PerformanceGoalsEntity GetGoalsEntity(string sqlstr, string filter = "")
        {

            var info = new PerformanceGoalsEntity();
            string conStr = "server=47.99.170.3;user=gaole;pwd=heyi2020!@#$%^;database=Ymsoft112";//连接字符串  
            SqlConnection conText = new SqlConnection(conStr);//创建Connection对象 
            conText.Open();//打开数据库  
            string sqls = sqlstr;//创建统计语句  
            SqlCommand comText = new SqlCommand(sqls, conText);//创建Command对象  
            SqlDataReader dr;//创建DataReader对象  
            dr = comText.ExecuteReader();//执行查询  
            while (dr.Read())//判断数据表中是否含有数据  
            {
                info = DataBindHelper.CreateEntity<PerformanceGoalsEntity>(dr);
            }
            dr.Close();//关闭DataReader对象  
            return info;
        }
        /// <summary>
        /// 门店目标查询
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult GoalsStatistics(_SeachEntity entity)
        {
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? DateTime.Now.AddMonths(-1) : entity.s_StareTime;
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? DateTime.Now : entity.s_Endtime;
            entity.s_HospitalID = entity.s_HospitalID == 0 ? LoginUserEntity.HospitalID : entity.s_HospitalID;
            var list = iCentBll.GetUserGoalsinfo(entity);

            decimal SumXianJin = 0, SumChangping = 0;
            decimal SumShiHao = 0;
            int SumTraffic = 0;
            decimal SumXianJin1 = 0, SumChangping1 = 0;
            decimal SumShiHao1 = 0;
            int SumTraffic1 = 0;
            StringBuilder TxtHtm = new StringBuilder();

            //获取查找这个门店的员工的列表
            var UserList = personnelBLL.GetAllUserByHospitalID(entity.s_HospitalID, 0);
            //找出查询时间所包含的月份


            var y = entity.s_Endtime.Year - entity.s_StareTime.Year;//相隔几年
            var m = entity.s_Endtime.Month - entity.s_StareTime.Month;//相差月份
            if (y > 0)
            {
                m = m + (12 * y);
            }
            //这个字段代表在原来时间的基础上加几年
            int addy = 0;
            //遍历用户
            foreach (var info in UserList)
            {
                addy = 0;
                //遍历月份
                for (int j = 0; j <= m; j++)
                {
                    //遍历的当月
                    var thismonth = entity.s_StareTime.Month + j;
                    if (thismonth > 12)
                    {
                        addy = (thismonth - 1) / 12;
                        thismonth = thismonth - (12 * addy);
                    }
                    var thisyear = entity.s_StareTime.Year + addy;
                    string strsql = @"SELECT top 1
	                                a.ProductConsumption,a.*,
	                                b.NAME AS HospitalName,
	                                d.UserName 
                                FROM
	                                EYB_tb_PerformanceGoals a
	                                INNER JOIN dbo.EYB_tb_Hospital b ON a.HospitalID = b.ID
	                                LEFT JOIN dbo.EYB_tb_HospitalUser d ON a.UserID = d.UserID 
                                WHERE
	                                1 = 1 
	                                AND a.UserID = "+ info.UserID  + @" 
	                                AND a.HospitalID = "+ entity.s_HospitalID + @" 
	                                AND a.Months = "+ thismonth + @" 
	                                AND a.Years = "+ thisyear + @"";
                    var golsEntity = GetGoalsEntity(strsql);
                    //找到所有当前用户的数据
                    var userjoinuser = list.Where(u => u.UserID == info.UserID).ToList();
                    //找到当月的用户数据 
                    userjoinuser = userjoinuser.Where(u => u.Ctime.Substring(0, 7) == thisyear + "-" + thismonth.ToString().PadLeft(2, '0')).ToList();

                    var XianJin = userjoinuser.Sum(i => i.Yeji);
                    SumXianJin += XianJin;

                    var Changping  = userjoinuser.Sum(i => i.Yeji_Chanpin) + userjoinuser.Sum(i => i.Yeji_TeshuChanpin) + userjoinuser.Sum(i => i.KakouYeji_Chanpin) + userjoinuser.Sum(i => i.KakouYeji_TeshuChanpin);
                    SumChangping += Changping;
                    //var ShiHao = userjoinuser.Sum(i => i.XiaoHao_Liaocheng) + userjoinuser.Sum(i => i.Yeji_Xiangmu) + userjoinuser.Sum(i => i.Yeji_Chanpin) + userjoinuser.Sum(i => i.KakouYeji_Chanpin) + userjoinuser.Sum(i => i.XiaoHao_Liaocheng) + userjoinuser.Sum(i => i.KakouYeji_Xiangmu);
                    var ShiHao = userjoinuser.Sum(i => i.XiaoHao);
                    SumShiHao += ShiHao;

                    var Traffic = 0;
                    //本月一共有几天
                    int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(thisyear, Convert.ToInt32(thismonth));
                    for (int ml = 0; ml <= monthDays; ml++)
                    {  
                        List<int> userList = new List<int>();
                        var userjoinuser1 = userjoinuser.Where(i => i.Ctime == thisyear + "-" + thismonth.ToString().PadLeft(2, '0') + "-" + ml.ToString().PadLeft(2, '0')).ToList();
                        foreach (var uinfo in userjoinuser1)
                        {
                            userList.Add(uinfo.OUserID);
                        }
                        userList = userList.Distinct().ToList();
                        Traffic += userList.Count;
                    }   

                    SumTraffic += Traffic;

                    if (golsEntity != null && golsEntity.Xianjin != 0) {
                        SumXianJin1 += golsEntity.Xianjin;
                        SumChangping1 += golsEntity.ProductConsumption;
                        SumShiHao1 += golsEntity.ShiHao;
                        SumTraffic1 += golsEntity.Traffic;
                        decimal _ProductConsumption = 0;
                        try
                        {
                            _ProductConsumption = Math.Round((Changping / golsEntity.ProductConsumption) * 100, 2);
                        }
                        catch {
                            _ProductConsumption = 0;
                        }
                        if (j == 0)
                        {
                            TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td></tr>", info.UserName, thisyear + "-" + thismonth, XianJin , ShiHao , Traffic , Changping, Math.Round((XianJin / golsEntity.Xianjin) * 100, 2), _ProductConsumption, Math.Round((ShiHao / golsEntity.ShiHao) * 100, 2), Math.Round((Convert.ToDecimal((decimal)Traffic / (decimal)golsEntity.Traffic)) * 100, 2));
                        }
                        else if (j == m)
                        {
                            TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td></tr>", "", thisyear + "-" + thismonth, XianJin, ShiHao, Traffic, Changping, Math.Round((XianJin / golsEntity.Xianjin) * 100, 2), _ProductConsumption, Math.Round((ShiHao / golsEntity.ShiHao) * 100, 2), Math.Round((Convert.ToDecimal((decimal)Traffic / (decimal)golsEntity.Traffic)) * 100, 2));
                            TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td></tr>", "合计:", "", SumXianJin, SumShiHao, SumTraffic, SumChangping, "", "", "", "");
                            SumTraffic = 0; SumXianJin = 0; SumShiHao = 0; SumChangping = 0;
                        }
                        else
                        {
                            TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td></tr>", "", thisyear + "-" + thismonth, XianJin, ShiHao, Traffic, Changping, Math.Round((XianJin / golsEntity.Xianjin) * 100, 2), _ProductConsumption, Math.Round((ShiHao / golsEntity.ShiHao) * 100, 2), Math.Round((Convert.ToDecimal((decimal)Traffic / (decimal)golsEntity.Traffic)) * 100, 2));
                        }
                    }
                    else
                    {
                        if (j == 0)
                        {
                            TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td></tr>", info.UserName, thisyear + "-" + thismonth, XianJin, ShiHao, Traffic, Changping, "", "", "", "");
                        }
                        else if (j == m)
                        {
                            TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td></tr>", "", thisyear + "-" + thismonth, XianJin, ShiHao, Traffic, Changping, "", "", "", "");
                            TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td></tr>", "合计:", "", SumXianJin, SumShiHao, SumTraffic, SumChangping, "", "", "", "");
                            SumTraffic = 0; SumXianJin = 0; SumShiHao = 0; SumChangping = 0;
                        }
                        else
                        {
                            TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{6}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{5}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{7}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{8}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{9}</td></tr>", "", thisyear + "-" + thismonth, XianJin, ShiHao, Traffic, Changping, "", "", "", "");
                        }
                    }
                }

            }
            PerformanceGoalsEntity model = new PerformanceGoalsEntity();
            model.StrHtml = TxtHtm.ToString();
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.St = entity.s_StareTime;
            ViewBag.et = entity.s_Endtime;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_GoalsStatistics", model);
            return View(model);

        }
        public ActionResult GoalsStatistics1(_SeachEntity entity)
        {
            entity.s_StareTime = entity.s_StareTime.Year == 1 ? DateTime.Now.AddMonths(-1) : entity.s_StareTime;
            entity.s_Endtime = entity.s_Endtime.Year == 1 ? DateTime.Now : entity.s_Endtime;
            entity.s_HospitalID = entity.s_HospitalID == 0 ? LoginUserEntity.HospitalID : entity.s_HospitalID;
            var list = iCentBll.GetUserGoalsinfo(entity);

            decimal SumXianJin = 0;
            decimal SumShiHao = 0;
            int SumTraffic = 0;
            StringBuilder TxtHtm = new StringBuilder();

            //获取查找这个门店的员工的列表
            var UserList = personnelBLL.GetAllUserByHospitalID(entity.s_HospitalID, 0);
            //找出查询时间所包含的月份


            var y = entity.s_Endtime.Year - entity.s_StareTime.Year;//相隔几年
            var m = entity.s_Endtime.Month - entity.s_StareTime.Month;//相差月份
            if (y > 0)
            {
                m = m + (12 * y);
            }
            //这个字段代表在原来时间的基础上加几年
            int addy = 0;
            //遍历用户
            foreach (var info in UserList)
            {
                addy = 0;
                //遍历月份
                for (int j = 0; j <= m; j++)
                {
                    //遍历的当月
                    var thismonth = entity.s_StareTime.Month + j;
                    if (thismonth > 12)
                    {
                        addy = (thismonth - 1) / 12;
                        thismonth = thismonth - (12 * addy);
                    }
                    var thisyear = entity.s_StareTime.Year + addy;

                    //找到所有当前用户的数据
                    var userjoinuser = list.Where(u => u.UserID == info.UserID).ToList();
                    //找到当月的用户数据
                    userjoinuser = userjoinuser.Where(u => u.Ctime.Substring(0, 7) == thisyear + "-" + thismonth.ToString().PadLeft(2, '0')).ToList();

                    var XianJin = userjoinuser.Sum(i => i.Yeji);
                    SumXianJin += XianJin;

                    //var ShiHao = userjoinuser.Sum(i => i.XiaoHao_Liaocheng) + userjoinuser.Sum(i => i.Yeji_Xiangmu) + userjoinuser.Sum(i => i.Yeji_Chanpin) + userjoinuser.Sum(i => i.KakouYeji_Chanpin) + userjoinuser.Sum(i => i.XiaoHao_Liaocheng) + userjoinuser.Sum(i => i.KakouYeji_Xiangmu);
                    var ShiHao = userjoinuser.Sum(i => i.XiaoHao);
                    SumShiHao += ShiHao;

                    var Traffic = 0;
                    //本月一共有几天
                    int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(thisyear, Convert.ToInt32(thismonth));
                    for (int ml = 0; ml <= monthDays; ml++)
                    {
                        List<int> userList = new List<int>();
                        var userjoinuser1 = userjoinuser.Where(i => i.Ctime == thisyear + "-" + thismonth.ToString().PadLeft(2, '0') + "-" + ml.ToString().PadLeft(2, '0')).ToList();
                        foreach (var uinfo in userjoinuser1)
                        {
                            userList.Add(uinfo.OUserID);
                        }
                        userList = userList.Distinct().ToList();
                        Traffic += userList.Count;
                    }

                    SumTraffic += Traffic;

                    if (j == 0)
                    {
                        TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td></tr>", info.UserName, thisyear + "-" + thismonth, XianJin, ShiHao, Traffic);
                    }
                    else if (j == m)
                    {
                        TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td></tr>", "", thisyear + "-" + thismonth, XianJin, ShiHao, Traffic);
                        TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td></tr>", "合计:", "", SumXianJin, SumShiHao, SumTraffic);
                        SumTraffic = 0; SumXianJin = 0; SumShiHao = 0;
                    }
                    else
                    {
                        TxtHtm.AppendFormat("<tr><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{0}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{1}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{2}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{3}</td><td style=' line-height:26px;  text-align:center; border:1px #ccc solid; font-size:14px;' >{4}</td></tr>", "", thisyear + "-" + thismonth, XianJin, ShiHao, Traffic);
                    }
                }

            }
            PerformanceGoalsEntity model = new PerformanceGoalsEntity();
            model.StrHtml = TxtHtm.ToString();
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            ViewBag.St = entity.s_StareTime;
            ViewBag.et = entity.s_Endtime;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_GoalsStatistics", model);
            return View(model);

        }
        #endregion

        #region ==盈亏平衡管理==

        /// <summary>
        /// 盈亏平衡列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult BreakEvenList(BreakEvenEntity entity)
        {
            int rows;
            int pagecount;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "ID"; }
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            var list = iCentBll.GetBreakEvenList(entity, out rows, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_BreakEvenList", result);
            return View(result);
        }

        /// <summary>
        /// 平衡编辑页面
        /// </summary>
        /// <returns></returns>
        public ActionResult BreakEvenEdit()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult EditBreakEven(BreakEvenEntity entity)
        {
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            var result = 0;


            if (entity.ID > 0)
            {
                result = iCentBll.UpdateBreakEven(entity);
            }
            else
            {
                result = iCentBll.AddBreakEven(entity);
            }
            return Json(result);
        }

        public ActionResult BreakEvenProgress()
        {
            var ID = Request["ID"];
            var model = iCentBll.GetBreakEvenModel(Convert.ToInt32(ID));
            int pagecount = 0;
            int row = 0;
            _SeachEntity entity = new _SeachEntity();
            entity.numPerPage = 10000000;
            entity.s_StareTime = Convert.ToDateTime(model.Years + "-" + model.Months.ToString().PadLeft(2, '0') + "-01 00:00:01");
            int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(model.Years, Convert.ToInt32(model.Months));
            entity.s_Endtime = Convert.ToDateTime(model.Years + "-" + model.Months.ToString().PadLeft(2, '0') + "-" + monthDays + " 23:59:59");//获取当月最后一天
            entity.s_HospitalID = model.HospitalID;
            entity.s_ProductType = 1;
            var list = iCentBll.GetUserGoalsinfo(entity);
            var pdList = cashBll.GetPaymentOrderByTime(entity);//获取所有现金支付的记录
            entity.s_ProductType = 2;
            var pdList1 = cashBll.GetPaymentOrderByTime(entity);//获取银联卡支付记录

            StockOrderEntity soe = new StockOrderEntity();
            soe.HospitalID = model.HospitalID;
            soe.StartTime = entity.s_StareTime;
            soe.EndTime = entity.s_Endtime;
            soe.IsVerify = 2;//已审核
            soe.Type = 1;//出库
            soe.Class = 7;//院用
            var stList = iwareBLL.GetStockOrderList(soe, out row, out pagecount);
            var yuanyong = stList.Sum(i => i.AllChengBen);


            //实耗
            var shihao = list.Sum(i => i.XiaoHao_Liaocheng) + list.Sum(i => i.Yeji_Xiangmu) + list.Sum(i => i.Yeji_Chanpin) + list.Sum(i => i.KakouYeji_Chanpin) + list.Sum(i => i.KakouYeji_Xiangmu);
            var xianjin = pdList.Sum(i => i.PayMoney) + pdList1.Sum(i => i.PayMoney);//现金的算法是求出当月时间段的现金和银联卡支付方式的所有支付记录,然后加起来
            OrderEntity ord = new OrderEntity();
            ord.Statu = 1;
            ord.HospitalID = model.HospitalID;
            ord.StartTime = Convert.ToDateTime(model.Years + "-" + model.Months.ToString().PadLeft(2, '0') + "-01 00:00:01").ToString();
            ord.EndTime = Convert.ToDateTime(model.Years + "-" + model.Months.ToString().PadLeft(2, '0') + "-" + monthDays + " 23:59:59").ToString();
            ord.numPerPage = 100000000;
            ord.OrderType = 1;
            decimal QianPrice = 0;
            //获取那个月的项目产品订单单据
            var orderList = cashBll.GetAllOrder(ord, out QianPrice);
            decimal AllChengben = 0;
            foreach (var order in orderList)
            {
                var infolist = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = order.OrderNo });
                foreach (var iil in infolist)
                {
                    if (iil.InfoBuyType == 4)
                    {
                        var productModel = baseinfoBLL.GetModel(iil.ProdcuitID);
                        AllChengben += productModel.CostPrice;
                    }
                    else if (iil.InfoBuyType == 3 || iil.InfoBuyType == 5)
                    {
                        var ProjectModel = baseinfoBLL.GetProjectModel(iil.ProdcuitID);
                        AllChengben += ProjectModel.CostPrice;
                    }
                }
            }
            ViewBag.Shihao = shihao;
            ViewBag.Xianjin = xianjin;
            ViewBag.Chengben = AllChengben;//产品成本
            ViewBag.yuanyong = yuanyong;


            var nm = model;
            nm.Months -= 1;
            var lastList = iCentBll.GetBreakEvenList(nm, out row, out pagecount);
            if (lastList.Count == 0)
            {
                ViewBag.Xianjin1 = 0;
                ViewBag.Shihao1 = 0;
                ViewBag.Chengben1 = 0;
                ViewBag.Zujin = 0;
                ViewBag.ShuiDian = 0;
                ViewBag.ShenghuoKaizhi = 0;
                ViewBag.Yunying = 0;
                ViewBag.Zhuangxiu = 0;
                ViewBag.Dixin = 0;
                ViewBag.yuanyong = 0;
            }
            else
            {
                entity.s_ProductType = 1;
                entity.s_StareTime = entity.s_StareTime.AddMonths(-1);
                entity.s_Endtime = entity.s_Endtime.AddMonths(-1);//开始结束时间都减一个月
                var list1 = iCentBll.GetUserGoalsinfo(entity);
                var pdList2 = cashBll.GetPaymentOrderByTime(entity);//获取所有现金支付的记录
                entity.s_ProductType = 2;
                var pdList3 = cashBll.GetPaymentOrderByTime(entity);//获取银联卡支付记录
                var shihao1 = list.Sum(i => i.XiaoHao_Liaocheng) + list.Sum(i => i.Yeji_Xiangmu) + list.Sum(i => i.Yeji_Chanpin) + list.Sum(i => i.KakouYeji_Chanpin) + list.Sum(i => i.KakouYeji_Xiangmu);
                var xianjin1 = pdList.Sum(i => i.PayMoney) + pdList1.Sum(i => i.PayMoney);//现金的算法是求出当月时间段的现金和银联卡支付方式的所有支付记录,然后加起来
                ord.StartTime = Convert.ToDateTime(ord.StartTime).AddMonths(-1).ToString();
                ord.EndTime = Convert.ToDateTime(ord.EndTime).AddMonths(-1).ToString();
                decimal QianPrice1 = 0;
                var orderList1 = cashBll.GetAllOrder(ord, out QianPrice1);
                decimal AllChengben1 = 0;
                foreach (var order in orderList)
                {
                    var infolist1 = cashBll.GetOrderinfoList(new OrderinfoEntity { OrderNo = order.OrderNo });
                    foreach (var iil in infolist1)
                    {
                        if (iil.InfoBuyType == 4)
                        {
                            var productModel = baseinfoBLL.GetModel(iil.ProdcuitID);
                            AllChengben1 += productModel.CostPrice;
                        }
                        else if (iil.InfoBuyType == 3 || iil.InfoBuyType == 5)
                        {
                            var ProjectModel = baseinfoBLL.GetProjectModel(iil.ProdcuitID);
                            AllChengben1 += ProjectModel.CostPrice;
                        }
                    }
                }
                soe.StartTime = entity.s_StareTime.AddMonths(-1);
                soe.EndTime = entity.s_Endtime.AddMonths(-1);

                var stList1 = iwareBLL.GetStockOrderList(soe, out row, out pagecount);
                var yuanyong1 = stList1.Sum(i => i.AllChengBen);

                ViewBag.Xianjin1 = xianjin1;
                ViewBag.Shihao1 = shihao1;
                ViewBag.Chengben1 = AllChengben1;
                ViewBag.Zujin = lastList[0].Zujin;
                ViewBag.ShuiDian = lastList[0].ShuiDian;
                ViewBag.ShenghuoKaizhi = lastList[0].ShenghuoKaizhi;
                ViewBag.Yunying = lastList[0].Yunying;
                ViewBag.Zhuangxiu = lastList[0].Zhuangxiu;
                ViewBag.Dixin = lastList[0].Dixin;
                ViewBag.yuanyong1 = yuanyong1;
            }
            return View(model);

        }

        public ActionResult BreakManage()
        {
            return View();
        }

        /// <summary>
        /// 盘亏平衡统计分析
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult BreakState(BreakEvenEntity entity)
        {

            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.Years = entity.Years < 1970 ? DateTime.Now.Year : entity.Years;
            entity.Months = entity.Months == 0 ? DateTime.Now.Month : entity.Months;
            entity.HospitalID = entity.HospitalID == 0 ? LoginUserEntity.HospitalID : entity.HospitalID;
            entity.numPerPage = 5000;
            List<Double> zc = new List<double>();
            List<Double> sr = new List<double>();
            List<string> tname = new List<string>();
            //这个是数据组装几个
            List<seriesList> _series_list = new List<seriesList>();
            //获取这个月的记录
            decimal shouru = 0;
            decimal zhichu = 0;
            GetBreakMonthsState(entity, ref zhichu, ref shouru);
            zc.Add(Convert.ToDouble(zhichu));
            sr.Add(Convert.ToDouble(shouru));
            shouru = 0;
            zhichu = 0;
            tname.Add(entity.Months + "月");
            DateTime dt = Convert.ToDateTime(entity.Years + "-" + entity.Months.ToString().PadLeft(2, '0') + "-01 00:00:01");
            dt = dt.AddMonths(-1);
            entity.Years = dt.Year;
            entity.Months = dt.Month;
            tname.Add(entity.Months + "月");
            shouru = 0;
            zhichu = 0;
            GetBreakMonthsState(entity, ref zhichu, ref shouru);
            zc.Add(Convert.ToDouble(zhichu));
            sr.Add(Convert.ToDouble(shouru));
            dt = dt.AddMonths(-1);
            entity.Years = dt.Year;
            entity.Months = dt.Month;
            tname.Add(entity.Months + "月");
            shouru = 0;
            zhichu = 0;
            GetBreakMonthsState(entity, ref zhichu, ref shouru);
            zc.Add(Convert.ToDouble(zhichu));
            sr.Add(Convert.ToDouble(shouru));

            seriesList _series = new seriesList()
            {
                name = "支出",
                data = zc
            };
            _series_list.Add(_series);

            seriesList _series1 = new seriesList()
            {
                name = "收入",
                data = sr
            };
            _series_list.Add(_series1);


            //json转换
            JavaScriptSerializer _ser = new JavaScriptSerializer();

            //配置列表
            ChartConfigEntity chartConfig_list = new ChartConfigEntity()
            {
                Type = "column",
                Title = "",
                Subtitle = "",
                XAxis = new xAxis() { Title = "月份", CategoriesJson = _ser.Serialize(tname) },
                YAxis = new yAxis() { Title = "( 元 )" },
                SeriesJson = _ser.Serialize(_series_list)
            };
            if (Request.IsAjaxRequest())
                return PartialView("_BreakState", chartConfig_list);
            return View(chartConfig_list);
        }

        /// <summary>
        /// 获得每个月的收入支出结果
        /// </summary>
        /// <param name="_entity"></param>
        /// <param name="Zhichu"></param>
        /// <param name="Shouru"></param>
        public void GetBreakMonthsState(BreakEvenEntity _entity, ref decimal Zhichu, ref decimal Shouru)
        {

            int rows;
            int pagecount;
            //获取当月的分析数据
            var list = iCentBll.GetBreakEvenList(_entity, out rows, out pagecount);

            _SeachEntity _se = new _SeachEntity();
            _se.numPerPage = 10000000;
            _se.s_StareTime = Convert.ToDateTime(_entity.Years + "-" + _entity.Months.ToString().PadLeft(2, '0') + "-01 00:00:01");
            int monthDays = HS.SupportComponents.DateTimeHelper.GetAllDayByMonth(_entity.Years, Convert.ToInt32(_entity.Months));
            _se.s_Endtime = Convert.ToDateTime(_entity.Years + "-" + _entity.Months.ToString().PadLeft(2, '0') + "-" + monthDays + " 23:59:59");//获取当月最后一天
            _se.s_HospitalID = _entity.HospitalID;
            _se.ProductType = 1;
            // var UserGoalslist = iCentBll.GetUserGoalsinfo(_se);
            var pdList = cashBll.GetPaymentOrderByTime(_se);//获取所有现金支付的记录
            //_se.ProductType = 2;
            //var pdList1 = cashBll.GetPaymentOrderByTime(_se);//获取银联卡支付记录

            //用户用户订单详情
            var userinfolist = cashBll.GetOrderinfoInClassByTime(_se);
            Shouru = pdList.Sum(i => i.PayMoney);
            //支出=盘亏设置条件+(收入*现金比例)+(项目产品里面所有的单据*消耗比例)
            if (list.Count > 0)
            {
                //先获取原来的支出项
                Zhichu = list[0].OtherCost + list[0].ShenghuoKaizhi + list[0].ShuiDian + list[0].Yunying + list[0].Zhuangxiu + list[0].Zujin;
                //加上现金比例
                Zhichu += list[0].Dixin + (pdList.Sum(i => i.PayMoney) * list[0].XianjinBili);
                //加上实耗比例
                //Zhichu += userinfolist.Sum(i => i.Price);
            }





        }

        #endregion

        #region ==顾客转介绍==

        public ActionResult CustomerRecommendation()
        {
            return View();
        }


        public ActionResult CustomerRecommendationList(_SeachEntity entity)
        {
            if (entity.s_StareTime == DateTime.MinValue)
            {
                entity.s_StareTime = DateTime.Today.AddDays(-3);
            }
            if (entity.s_Endtime == DateTime.MinValue)
            {
                entity.s_Endtime = DateTime.Now;
            }


            entity.s_HospitalID = entity.s_HospitalID == 0 ? LoginUserEntity.HospitalID : entity.s_HospitalID;
            entity.numPerPage = 100; //每页显示条数
            if (string.IsNullOrWhiteSpace(entity.orderField)) { entity.orderField = "UserID"; }
            var list = iCentBll.GetCustomerRecommendationOrderList(entity);    
            foreach (var info in list)
            {
                var s = "";
                int rows;
                int pageCount;
                var joinUser = cashBll.GetAllJoinUser(new JoinUserEntity { OrderNo = info.OrderNo }, out rows, out pageCount);
                var lists = joinUser.GroupBy(c => c.UserName).Select(c => c.First());
                foreach (var j in lists)
                {
                    s += j.UserName+ ",";
                }
                if (s.Length > 1)
                {
                    s = s.Substring(0, s.Length - 1);
                }  
                info.OpenID = s;
            }

            var result = list.ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = entity.Rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_CustomerRecommendation", result);
            return View(result);
        }

        public ActionResult CustomerRecommendationCount(_SeachEntity entity)
        {

            if (entity.s_StareTime == DateTime.MinValue)
            {
                entity.s_StareTime = DateTime.Today.AddDays(-3);
            }
            if (entity.s_Endtime == DateTime.MinValue)
            {
                entity.s_Endtime = DateTime.Now;
            }

            entity.s_HospitalID = entity.s_HospitalID == 0 ? LoginUserEntity.HospitalID : entity.s_HospitalID;
            entity.numPerPage = 500000; //每页显示条数
            if (string.IsNullOrWhiteSpace(entity.orderField)) { entity.orderField = "JieshaoUserName"; }
            var list = iCentBll.CustomerRecommendationCount(entity);

            foreach (var info in list)
            {
                var s = new _SeachEntity
                {
                    s_StareTime = entity.s_StareTime,
                    s_Endtime = entity.s_Endtime,
                    s_Keyword = info.JieshaoUserName,
                    s_HospitalID = entity.s_HospitalID,
                    numPerPage = 500000
                };
                //每页显示条数
                if (string.IsNullOrWhiteSpace(entity.orderField)) { s.orderField = "UserID"; }

                int rows;
                int pageCount;
                var userList = iCentBll.CustomerRecommendationList(s, out rows, out pageCount);
                foreach (var uinfo in userList)
                {
                    info.ArrearsMoney += uinfo.ArrearsMoney;
                }

            }



            var result = list.ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = entity.Rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_CustomerRecommendationCount", result);
            return View(result);
        }

        #endregion

        #region==顾客负债排名==
        /// <summary>
        /// 顾客负债排名
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientFuzhaiOrder(MainCardBalanceEntity entity)
        {
            int rows;
            int pagecount;
            decimal sumcard;
            decimal sumconsumer;
            decimal Zhexian;
            entity.numPerPage = 50; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "Price"; entity.orderDirection = "desc"; }
            if (entity.HospitalID == 0) entity.HospitalID = LoginUserEntity.HospitalID;


            var list = IpatBLL.GetCustomerCardBalanceOrder(entity, out rows, out pagecount, out sumcard, out sumconsumer, out Zhexian);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.sumcard = sumcard;
            ViewBag.sumconsumer = sumconsumer;
            ViewBag.Zhexian = Zhexian;

            if (Request.IsAjaxRequest())
                return PartialView("_PatientFuzhaiOrder", result);
            return View(result);
        }

        /// <summary>
        /// 顾客负债排名
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportPatientFuzhaiOrder(MainCardBalanceEntity entity)
        {
            int rows;
            int pagecount;
            decimal sumcard;
            decimal sumconsumer;
            decimal Zhexian;
            entity.numPerPage = 100000; //每页显示条数
            if (entity.orderField + "" == "") { entity.orderField = "Price"; entity.orderDirection = "desc"; }
            if (entity.HospitalID == 0) entity.HospitalID = LoginUserEntity.HospitalID;


            var list = IpatBLL.GetCustomerCardBalanceOrder(entity, out rows, out pagecount, out sumcard, out sumconsumer, out Zhexian);
            DataTable tableExport = new DataTable("Export");

            tableExport.Columns.AddRange(new DataColumn[] { new DataColumn("门店名称"), new DataColumn("顾客卡号"), new DataColumn("顾客名称"), new DataColumn("余次"), new DataColumn("余额") });
            foreach (MainCardBalanceEntity model in list)
            {
                DataRow row = tableExport.NewRow();
                row["门店名称"] = model.HospitalName;
                row["顾客卡号"] = model.MemberNo;
                row["顾客名称"] = model.UserName;
                row["余次"] = model.Tiems;
                row["余额"] = model.Price;
                tableExport.Rows.Add(row);
            }

            ExcelOperate excel = new ExcelOperate();
            excel.InitializeWorkbook();
            excel.GenerateData(tableExport, "顾客负债排名");
            return File(excel.WriteToStream().GetBuffer(), "application/vnd.ms-excel", "顾客负债排名.xls");
        }
        #endregion

        #region== 门店卡项结余月结
        /// <summary>
        /// 门店卡项结余月结
        /// </summary>
        /// <returns></returns>
        public ActionResult CardBalanceYueJie(MaincardBalanceYuejieEntity entity)
        {
            if (entity.orderField + "" == "") { entity.orderField = "Price"; entity.orderDirection = "desc"; }
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
                entity.Month = DateTime.Now.Month;
                entity.Year = DateTime.Now.Year;
            }
            var list = IpatBLL.SelectMaincardBalanceYuejie(entity);
            var result = list.ToPagedList(1, list.Count);
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            ViewBag.Count = list.Count;
            if (ViewBag.orderDirection == "desc") {
                result.Sort((a, b) => {
                    var o = a.Times - b.Times;
                    return o;
                });
            }
            else {
                result.Sort((a, b) => {
                    var o =b.Times - a.Times;
                    return o;
                });
            }
            if (Request.IsAjaxRequest())
                return PartialView("_CardBalanceYueJie", result);
            return View(result);
        }
        #endregion
    }
}
