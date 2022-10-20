using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;

using HS.SupportComponents;
using HS.SupportComponents.Base;
using SystemManage.Factory.IBLL;
using Common.Attribute;
using EYB.ServiceEntity.SystemEntity;
using PersonnelManage.Factory.IBLL;
using EYB.ServiceEntity.PersonnelEntity;
using BaseinfoManage.Factory.IBLL;
using EYB.ServiceEntity.WarehouseEntity;
using WarehouseManage.Factory.IBLL;

namespace EYB.Web.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public class AccountController : Controller
    {
        #region 依赖注入
        private ISystemManageBLL systemBLL;
        private IPersonnelManageBLL personnelManageBLL;
        private IBaseinfoBLL baseinfoBLL;

        private IWarehouseManageBLL iwareBLL; //仓库管理    
        public AccountController()
        {
            iwareBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IWarehouseManageBLL>();
            systemBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();
            personnelManageBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            baseinfoBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IBaseinfoBLL>();
        }
        #endregion 依赖注入

        #region==Get请求==
        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        public ActionResult LoginOut()
        {
            Session.Abandon();
            CookieHelper.ClearCookie("eyblogin");

            var sysSetting = baseinfoBLL.GetSysSettingEntity();

            return View("LoginPage", sysSetting);
        }
        //退出
        public ActionResult LoginOff()
        {
            return View("LoginPage");
        }
        public ActionResult LoginPage()
        {
            var sysSetting = baseinfoBLL.GetSysSettingEntity();
            return View(sysSetting);
        }
        public ActionResult VersionPage()
        {
            return View();
        }
        public ActionResult DownloadPage()
        {
            return View();
        }

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <returns></returns>
        [WriteOperateLog("用户登录", "用户登录Login", "登入模块")]
        public ActionResult Login(string account, string pwd, string checkcode, string rememberme)
        {

            //string checkCode = Session["CheckCode"] + "";
            //if (!checkCode.ToLower().Equals(checkcode.ToLower()))//校验码验证
            //{
            //    Session.Remove("CheckCode");    //移除验证码
            //    Response.Write("验证码错误");
            //    Response.End();
            //    return Json(-9);
            //}
            //else
            //{
            try
            {
                HospitalUserEntity entity = personnelManageBLL.UserLogin(account, pwd);
                if (entity.HospitalID == 0)
                {
                    //账户异常
                    return Json(-8);

                }
                //if(entity.HospitalID>0)
                //{
                //    Session["userInfoquanxian"] = entity.VersionName;
                //    if (entity.VersionName == "管理员")
                //    {
                //        var usersmodel = systemBLL.GetUserSettingsModelMemo("管理员",entity.HospitalID);
                //        if (usersmodel.ID==0)
                //        {
                //            UserSettingsEntity user = new UserSettingsEntity();
                //            user.Name = "guanliquanxian";
                //            user.AttributeValue = "2";
                //            user.Memo = "管理员";
                //            user.Type = "radio";
                //            user.HospitalID = entity.HospitalID;
                //            systemBLL.AddUserSettings(user);
                //            user = new UserSettingsEntity();
                //            user.Name = "dianzhangquanxian";
                //            user.AttributeValue = "2";
                //            user.Memo = "店长";
                //            user.Type = "radio";
                //            user.HospitalID = entity.HospitalID;
                //            systemBLL.AddUserSettings(user);
                //            user = new UserSettingsEntity();
                //            user.Name = "qiantaiquanxian";
                //            user.AttributeValue = "2";
                //            user.Memo = "前台";
                //            user.Type = "radio";
                //            user.HospitalID = entity.HospitalID;
                //            systemBLL.AddUserSettings(user);
                //            user = new UserSettingsEntity();
                //            user.Name = "guwenquanxian";
                //            user.AttributeValue = "2";
                //            user.Memo = "顾问";
                //            user.Type = "radio";
                //            user.HospitalID = entity.HospitalID;
                //            systemBLL.AddUserSettings(user);
                //            user = new UserSettingsEntity();
                //            user.Name = "meirongshiquanxian";
                //            user.AttributeValue = "2";
                //            user.Memo = "美容师";
                //            user.Type = "radio";
                //            user.HospitalID = entity.HospitalID;
                //            systemBLL.AddUserSettings(user);

                //        }
                //    }
                //}
                //手机用户不能登录PC
                if (entity.IsAPP == 1)
                {
                    return Json(-7);
                }
                if (entity.OpenActive == 0)
                {
                    return Json(-8);
                }
                //修改最后登录时间
                personnelManageBLL.UpdateLoginTime(entity.UserID);
                //日志记录用
                ViewBag.UserName = entity.UserName;
                ViewBag.UserID = entity.UserID;
                ViewBag.HospitalID = entity.HospitalID;

                /*

                var kucunlist = new List<ProductStockEntity>(); //库存列表
                ProductStockEntity pse = new ProductStockEntity();
                            ProductStockEntity entity1 = new ProductStockEntity();
                            entity1.HospitalID = entity.HospitalID;
                            entity1.ProductID = 68537;         ////
                            entity1.HouseID = 1071; //一个门店一个仓库,所以直接拿来用  ////
                            var list1 = iwareBLL.GetProductStockListToseleect(entity1);
                            if (list1 != null)
                            {
                                foreach (var info in list1)
                                {
                                    kucunlist.Add(info);
                                }
                            }

                if (kucunlist.Count > 0)
                {
                    #region ==新建入库单据==

                    StockOrderEntity sentity = new StockOrderEntity();
                    sentity.BaseID = 357;  ////
                    sentity.HospitalID = entity.HospitalID;
                    sentity.IsVerify = 2;
                    sentity.Memo = "项目产品订单取消入库单据";
                    sentity.OrderNo = "R2020021810699424";
                    sentity.OrderTime = DateTime.Now;
                    sentity.Type = 2;
                    sentity.UserID = entity.UserID;
                    sentity.VerifyTime = DateTime.Now;
                    sentity.VerifyUserID = entity.UserID;
                    sentity.Class = 6;
                    sentity.VerifyMemo = "202002272073928168"; ////
                    sentity.AllMoney = 0;
                    sentity.AllQuatity = (1);
                    sentity.AllChengBen = 0;
                    //添加入库单据
                    var rt = iwareBLL.AddStockOrder(sentity);

                    #endregion

                    #region ==添加单据详情==

                    //添加单据详情
                    StockOrderInfo soinfo = new StockOrderInfo();
                    soinfo.StockOrderNo = sentity.OrderNo;
                    soinfo.Number = (1);
                    soinfo.ProductID = 68537; ////
                    soinfo.HouseID = 1071;    ////
                    iwareBLL.AddStockOrderInfo(soinfo);

                    #endregion

                    #region ==入库操作==

                    if (rt > 0)
                    {
                        StockChangeLogEntity scl = new StockChangeLogEntity();
                        scl.UserID = entity.UserID;
                        scl.LogClass = 8;
                        scl.LogState = 1;
                        scl.OwnedWarehouse = soinfo.HouseID;
                        scl.IntoWarehouse = pse.HouseID;
                        scl.ProductID = 68537;////
                        scl.LogType = "入库";
                        scl.LogTitle = "订单产品订单取消.取消订单编号:" + "202002272073928168" + "; 产品ID:" + "68537" + ";返回数量:" + "1" + ";";////
                        scl.NewQuatity = kucunlist[0].Quatity + (1);
                        scl.OldQuatity = kucunlist[0].Quatity;
                        scl.Quantity = (1);
                        scl.ProductStockID = kucunlist[0].ID;
                        scl.OrderNo = sentity.OrderNo;
                        var sclID = iwareBLL.AddStockChangeLog(scl);
                        //这里需要入库操作
                        pse.ID = kucunlist[0].ID;
                        pse.Quatity = 1;
                        iwareBLL.UpdateProductStock(pse);
                    }
                }

                #endregion
                */
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
                    RechargeEntity charge = personnelManageBLL.GetRechargeModelByHospitalID(entity.ParentHospitalID);//获取到期时间
                    entity.ParentHospitalName = charge.Name;
                    entity.Price = charge.Price;
                    Session["ExpireTime"] = charge.ExpireTime.Subtract(DateTime.Now).Days;
                    //判断是否要写入cookie
                    if (rememberme == "1")
                    {
                        string resultval = account;
                        CookieHelper.SetCookie("eyblogin", resultval);
                    }

                    Session["userInfo"] = entity;
                    List<UIControlEntity> menulist = new List<UIControlEntity>();

                    if (entity.IsAdmin != 1)
                    {
                        menulist = systemBLL.GetUserMenuByUserID(entity.UserID).Where(c => c.ParentUIControlID == new Guid("BEF5CCF2-18C0-46a6-9B91-E984BEB9BFEF")).ToList();
                    }
                    else
                    {
                        menulist = systemBLL.GetUserMenuByUserID(entity.UserID).Where(c => c.ParentUIControlID == new Guid("BEF5CCF2-18C0-46a6-9B91-E984BEB9BFEF")).ToList();

                        //  menulist = systemBLL.GetMenuList(1, 1).Where(c => c.ParentUIControlID == new Guid("BEF5CCF2-18C0-46a6-9B91-E984BEB9BFEF")).ToList();
                    }
                    Session["listMenu"] = menulist;
                    return Json(true);
                }
            }
            catch (Exception e)
            {
                LogWriter.WriteError(e, "登录错误");
                if (e.Message == "账户过期")
                {
                    return Json(-9);
                }
                return Json(false);
            }

            //}
            return Json(false);

        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePassword()
        {
            return View();
        }
        #endregion


        #region==生成验证码==
        public ActionResult CheckCode()
        {
            string checkCode = GenerateCheckCode();
            Session["CheckCode"] = checkCode;
            CreateCheckCodeImage(checkCode);
            return View();
        }

        /// <summary>
        /// 将文字验证码转化图形验证码。
        /// </summary>
        public void CreateCheckCodeImage(string checkCode)
        {
            if (checkCode == null || checkCode.Trim() == String.Empty)
                return;

            Bitmap image = new Bitmap((int)Math.Ceiling((checkCode.Length * 13.5)), 25);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成随机生成器
                Random random = new Random();
                //清空图片背景色
                g.Clear(Color.White);
                //画图片的背景噪音线
                //for (int i = 0; i < 25; i++)
                //{
                //    int x1 = random.Next(image.Width);
                //    int x2 = random.Next(image.Width);
                //    int y1 = random.Next(image.Height);
                //    int y2 = random.Next(image.Height);
                //    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                //}
                Font font = new Font("Arial", 12, (FontStyle.Bold | FontStyle.Italic));
                var brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(checkCode, font, brush, 2, 2);
                //画图片的前景噪音点
                //for (int i = 0; i < 100; i++)
                //{
                //    int x = random.Next(image.Width);
                //    int y = random.Next(image.Height);
                //    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                //}

                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Green), 1, 0, image.Width - 2, image.Height - 1);
                var ms = new System.IO.MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                Response.ClearContent();
                Response.ContentType = "image/Gif";
                Response.BinaryWrite(ms.ToArray());
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 根据用户IP生成验证码
        /// </summary>
        /// <returns>验证码</returns>
        public string GenerateCheckCode()
        {
            string code = "";
            // 随机数对象
            Random random = new Random();

            for (int i = 0; i < 4; i++)
            {
                // 26: a - z
                int n = random.Next(26);
                // 将数字转换成大写字母
                code += (char)(n + 65);
            }
            return code;
        }
        #endregion



    }
}
