using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using EYB.ServiceEntity.SystemEntity;
using HS.SupportComponents.Base;

namespace EYB.Web.Code
{
    public class PageFunction
    {
        /// <summary>
        /// 初始化左侧子菜单
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public static IList<UIControlEntity> GetSubMenu(string menuId)
        {
            if (HttpContext.Current.Session["userMenu"] == null)
            {
                HttpContext.Current.Response.Write(string.Format("<script  type=\"text/javascript\">top.location=\"{0}\"</script>", "Account/LoginPage"));
                return null;
            }
            else
            {
                var mainMenu = HttpContext.Current.Session["userMenu"] as IList<UIControlEntity>;
                if (mainMenu != null)
                {
                    return mainMenu.Where(c => c.ParentUIControlID == new Guid(menuId)).ToList();
                }
                return null;
            }
        }

        public static string GetChildMenus(List<MenuEntity> list, string parentId)
        {
            StringBuilder sb = new StringBuilder();
            var childList = list.Where(info => info.ID == new Guid(parentId)).ToList();
            foreach (var info in childList[0].Children)
            {
                sb.Append("<ul id='s_menu'>");
                sb.AppendFormat(" <li class='li' id='_MP{0}'><a  style='outline-style:none; outline-color:invert;outline-width:medium;' href='{2}' target='navTab'  rel='" + info.ID + "'  external='true' onclick='_MP(\"" + info.ID + "\",\"" + info.URL + "\")'>{1}</a></li>  <li class='backstage_Main_Menu_line'></li>", info.ID, info.Name, info.URL);
                sb.Append("</ul>");
            }
            return sb.ToString();
        }


        /// <summary>
        /// 获取对应大图标
        /// </summary>
        /// <param name="UIControlName"></param>
        /// <returns></returns>
        public static int GetNumStatu(string UIControlName)
        {
            switch (UIControlName)
            {
                case "顾客管理":
                    return 1;
                case "收银管理":
                    return 2;
                case "仓库管理":
                    return 3;
                case "项目管理":
                    return 4;
                case "财务管理":
                    return 5;
                case "员工管理":
                    return 6;
                case "系统设置":
                    return 7;
                case "员工分红":
                    return 8;
                case "共享店铺":
                    return 9;
                default:
                    return 0;
            }
        }


        /// <summary>
        /// 转换图片格式
        /// </summary>
        /// <returns></returns>
        public static string ConvertByteToImg(byte[] bytes, string id, bool isThumb)
        {
            if (bytes == null)
                return "";

            string path = HttpContext.Current.Server.MapPath("~/Img");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var thumbName = String.Format("{0}thumb.jpg", id);
            var fileName = String.Format("{0}", id);
            var imgPath = path + "/" + fileName;
            var thumbPath = path + "/" + thumbName;
            if (isThumb)
            {
                if (File.Exists(thumbPath))
                    return thumbName;
            }
            else
            {
                if (File.Exists(imgPath))
                    return fileName;
            }

            try
            {
                MemoryStream ms = new MemoryStream(bytes);
                ms.Position = 0;
                Image img = Image.FromStream(ms);
                if (isThumb)
                {
                    var myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
                    Image myThumbnail = img.GetThumbnailImage(120, 120, myCallback, IntPtr.Zero);
                    myThumbnail.Save(thumbPath);
                    ms.Close();
                    img.Dispose();
                    return thumbName;
                }
                else
                {
                    img.Save(imgPath, ImageFormat.Jpeg);
                    ms.Close();
                    img.Dispose();
                    return fileName;
                }
            }
            catch (Exception ce)
            {
                //Tracker.ErrorTrack(ce, "图片生成异常");
                return "";
            }
        }

        public static bool ThumbnailCallback()
        {
            return false;
        }
        /// <summary>
        /// 获取订单状态
        /// </summary>
        /// <param name="statu"></param>
        /// <returns></returns>
        public static string GetOrderStatu(int statu)
        {
            switch (statu)
            {
                case 1:
                    return "已结算";
                case 2:
                    return "待结算";
                case 3:
                    return "已取消";
                default:
                    return "未知";
            }
        }

        /// <summary>
        /// 根据订单编号获取订单类型名称
        /// </summary>
        /// <returns></returns>
        public static string GetorderType(int ordertype)
        {
            switch (ordertype)
            {
                case 1:
                    return "项目产品";
                case 2:
                    return "购买卡项";
                case 3:
                    return "充值";
                case 4:
                    return "还款";
                default:
                    return "退换";
            }
        }

        /// <summary>
        /// 获取采购进货单的业务类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetBuyOrderType(int type)
        {
            switch (type)
            {
                case 1:
                    return "进货单";
                case 2:
                    return "退货单";
                case 3:
                    return "销售单";
                case 4:
                    return "销售退货单";
                default:
                    return "其他";
            }
        }

        /// <summary>
        /// 采购结算方式
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetCaigou(int type)
        {
            switch (type)
            {
                case 1:
                    return "进货单";
                case 2:
                    return "退货单";
                case 3:
                    return "销售单";
                case 4:
                    return "销售退货单";
                default:
                    return "其他";
            }
        }

        public static string BuyOrderStatu(int type)
        {
            switch (type)
            {
                case 1:
                    return "未入库";
                case 2:
                    return "已入库";
                default:
                    return "其他";
            }
        }


        public static string BuyOrderType(int type)
        {
            switch (type)
            {
                case 1:
                    return "未完成";
                case 2:
                    return "已完成";
                default:
                    return "其他";
            }
        }

        /// <summary>
        /// 获取参与员工的姓名
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetHospitalName(int id)
        {
            PersonnelManage.Factory.IBLL.IPersonnelManageBLL bll = XmlRegistration.Current.GetResovleContainer().Resolve<PersonnelManage.Factory.IBLL.IPersonnelManageBLL>();
            var model = bll.GetUserByUserID(id);
            if (model != null)
            {
                return model.UserName;
            }
            else
            {
                return "暂未查出此用户";
            }


        }

        public static string OrderType(int type)
        {
            switch (type)
            {
                case 1:
                    return "出库";
                case 2:
                    return "入库";
                case 3:
                    return "调拨";
                case 5:
                    return "预付";
                default:
                    return "其它";
            }
        }

        /// <summary>
        /// 获取权限名称
        /// </summary>
        /// <param name="ve"></param>
        /// <returns></returns>
        public static string GetQuanXian(int ve, int hospid)
        {
            PersonnelManage.Factory.IBLL.IPersonnelManageBLL PersonnelManageBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<PersonnelManage.Factory.IBLL.IPersonnelManageBLL>();
            EYB.ServiceEntity.PersonnelEntity.VersionEntity velist = new EYB.ServiceEntity.PersonnelEntity.VersionEntity();
            velist.HospitalID = hospid;
            var list = PersonnelManageBLL.SelectVersionList(velist);
            var str = "其它";
            foreach (var info in list)
            {
                if (info.ID == ve)
                {
                    str = info.VersionName;
                }
            }

            return str;
        }

        /// <summary>
        /// 获取操作原因
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="ParentHospitalID"></param>
        /// <returns></returns>
        public static string GetBaseName(int ID, int ParentHospitalID)
        {
            SystemManage.Factory.IBLL.ISystemManageBLL sysBll = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<SystemManage.Factory.IBLL.ISystemManageBLL>();
            var thetype = sysBll.GetBaseInfoTreeByType("WarehouseIn", 1, ParentHospitalID);//类型分类
            var result = "其它";
            foreach (var info in thetype)
            {
                if (info.ID == ID)
                {
                    result = info.InfoName;
                    break;
                }
            }
            return result;
        }

        public static string GetClassName(int ID, int type)
        {     /// 分类:入库时:1.进货单入库 2.入库单入库 3.调拨入库 4.盘点入库
              /// 分类:出库时:1.正常出库 2.销售出库 3.退货出库 4.调拨出库 5.盘点出库
            var result = "";
            if (type == 1)
            {
                switch (ID)
                {
                    case 1: result = "正常出库"; break;
                    case 2: result = "销售出库"; break;
                    case 3: result = "退货出库"; break;
                    case 4: result = "调拨出库"; break;
                    case 5: result = "盘点出库"; break;
                    case 6: result = "预付出库"; break;
                    case 7: result = "院用出库"; break;

                    default: result = "其它"; break;
                }
            }
            else if (type == 2)
            {
                switch (ID)
                {
                    case 1: result = "进货单入库"; break;
                    case 2: result = "入库单入库"; break;
                    case 3: result = "调拨入库"; break;
                    case 4: result = "盘点入库"; break;
                    case 5: result = "退产品入库"; break;
                    case 6: result = "订单取消入库"; break;
                    case 7: result = "调拨确认后的退货入库"; break;
                    default: result = "其它"; break;
                }
            }
            else if (type == 5)
            {
                switch (ID)
                {
                    case 1: result = "前台预付单"; break;
                    default: result = "其它"; break;
                }

            }
            else
            {
                result = "其它";
            }

            return result;

        }

        /// <summary>
        /// 获取仓库单据类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTypeInHouse(int type)
        {
            var result = "";
            switch (type)
            {
                case 1: result = "入库单"; break;
                case 2: result = "出库单"; break;
                case 3: result = "调拨单"; break;
                case 4: result = "调拨出库单"; break;
                case 5: result = "预付单"; break;
                default: result = "其它"; break;
            }
            return result;
        }




        /// <summary>
        /// 在指定的字符串列表CnStr中检索符合拼音索引字符串
        /// </summary>
        /// <param name="CnStr">汉字字符串</param>
        /// <returns>相对应的汉语拼音首字母串</returns>
        public static string GetSpellCode(string CnStr)
        {
            string strTemp = "";
            if (CnStr == null)
            {
                return strTemp;
            }
            else
            {
                int iLen = CnStr.Length;
                int i = 0;

                for (i = 0; i <= iLen - 1; i++)
                {
                    strTemp += GetCharSpellCode(CnStr.Substring(i, 1));
                }
                return strTemp;
            }


        }


        /// <summary>
        /// 得到一个汉字的拼音第一个字母，如果是一个英文字母则直接返回大写字母
        /// </summary>
        /// <param name="CnChar">单个汉字</param>
        /// <returns>单个大写字母</returns>
        private static string GetCharSpellCode(string CnChar)
        {
            long iCnChar;

            byte[] ZW = System.Text.Encoding.Default.GetBytes(CnChar);

            //如果是字母，则直接返回
            if (ZW.Length == 1)
            {
                return CnChar.ToUpper();
            }
            else
            {
                // get the array of byte from the single char
                int i1 = (short)(ZW[0]);
                int i2 = (short)(ZW[1]);
                iCnChar = i1 * 256 + i2;
            }

            //expresstion
            //table of the constant list
            // 'A'; //45217..45252
            // 'B'; //45253..45760
            // 'C'; //45761..46317
            // 'D'; //46318..46825
            // 'E'; //46826..47009
            // 'F'; //47010..47296
            // 'G'; //47297..47613

            // 'H'; //47614..48118
            // 'J'; //48119..49061
            // 'K'; //49062..49323
            // 'L'; //49324..49895
            // 'M'; //49896..50370
            // 'N'; //50371..50613
            // 'O'; //50614..50621
            // 'P'; //50622..50905
            // 'Q'; //50906..51386

            // 'R'; //51387..51445
            // 'S'; //51446..52217
            // 'T'; //52218..52697
            //没有U,V
            // 'W'; //52698..52979
            // 'X'; //52980..53640
            // 'Y'; //53689..54480
            // 'Z'; //54481..55289

            // iCnChar match the constant
            if ((iCnChar >= 45217) && (iCnChar <= 45252))
            {
                return "A";
            }
            else if ((iCnChar >= 45253) && (iCnChar <= 45760))
            {
                return "B";
            }
            else if ((iCnChar >= 45761) && (iCnChar <= 46317))
            {
                return "C";
            }
            else if ((iCnChar >= 46318) && (iCnChar <= 46825))
            {
                return "D";
            }
            else if ((iCnChar >= 46826) && (iCnChar <= 47009))
            {
                return "E";
            }
            else if ((iCnChar >= 47010) && (iCnChar <= 47296))
            {
                return "F";
            }
            else if ((iCnChar >= 47297) && (iCnChar <= 47613))
            {
                return "G";
            }
            else if ((iCnChar >= 47614) && (iCnChar <= 48118))
            {
                return "H";
            }
            else if ((iCnChar >= 48119) && (iCnChar <= 49061))
            {
                return "J";
            }
            else if ((iCnChar >= 49062) && (iCnChar <= 49323))
            {
                return "K";
            }
            else if ((iCnChar >= 49324) && (iCnChar <= 49895))
            {
                return "L";
            }
            else if ((iCnChar >= 49896) && (iCnChar <= 50370))
            {
                return "M";
            }
            else if ((iCnChar >= 50371) && (iCnChar <= 50613))
            {
                return "N";
            }
            else if ((iCnChar >= 50614) && (iCnChar <= 50621))
            {
                return "O";
            }
            else if ((iCnChar >= 50622) && (iCnChar <= 50905))
            {
                return "P";
            }
            else if ((iCnChar >= 50906) && (iCnChar <= 51386))
            {
                return "Q";
            }
            else if ((iCnChar >= 51387) && (iCnChar <= 51445))
            {
                return "R";
            }
            else if ((iCnChar >= 51446) && (iCnChar <= 52217))
            {
                return "S";
            }
            else if ((iCnChar >= 52218) && (iCnChar <= 52697))
            {
                return "T";
            }
            else if ((iCnChar >= 52698) && (iCnChar <= 52979))
            {
                return "W";
            }
            else if ((iCnChar >= 52980) && (iCnChar <= 53640))
            {
                return "X";
            }
            else if ((iCnChar >= 53689) && (iCnChar <= 54480))
            {
                return "Y";
            }
            else if ((iCnChar >= 54481) && (iCnChar <= 55289))
            {
                return "Z";
            }
            else return ("?");
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="str">str字符串</param>
        /// <param name="Type">1是开始时间,2是结束时间</param>
        /// <returns></returns>
        private static DateTime StringToDatetime(string str, int Type)
        {
            DateTime dt = new DateTime();
            try
            {
                dt = Convert.ToDateTime(str);
                if (Type == 1)
                {
                    dt = Convert.ToDateTime(dt.Year + "-" + dt.Month + "-" + dt.Day + " 00:00:01");
                }
                else if (Type == 2)
                {
                    dt = Convert.ToDateTime(dt.Year + "-" + dt.Month + "-" + dt.Day + " 23:59:59");
                }
            }
            catch (Exception)
            {

                dt = Convert.ToDateTime("1999-01-01 00:00:01");
            }

            return dt;
        }



        /// <summary>
        /// 按字符串长度切分成数组
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <param name="separatorCharNum">切分长度</param>
        /// <returns>字符串数组</returns>
        public static string[] SplitByLen(string str, int separatorCharNum)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= separatorCharNum)
            {
                return new string[] { str };
            }
            string tempStr = str;
            List<string> strList = new List<string>();
            int iMax = Convert.ToInt32(Math.Ceiling(str.Length / (separatorCharNum * 1.0)));//获取循环次数
            for (int i = 1; i <= iMax; i++)
            {
                string currMsg = tempStr.Substring(0, tempStr.Length > separatorCharNum ? separatorCharNum : tempStr.Length);
                strList.Add(currMsg);
                if (tempStr.Length > separatorCharNum)
                {
                    tempStr = tempStr.Substring(separatorCharNum, tempStr.Length - separatorCharNum);
                }
            }
            return strList.ToArray();
        }

        public static string GetOrderStr(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (str.Contains("("))
                {
                    var slist = str.Split('(');
                    return slist[0];
                }
                else
                {
                    return str;
                }
            }
            else
            {
                return "default";
            }
        }






    }
}