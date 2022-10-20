using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PersonnelManage.Factory.IBLL;
using EYB.ServiceEntity.PersonnelEntity;
using SystemManage.Factory.IBLL;
using Webdiyer.WebControls.Mvc;
using HS.SupportComponents;
using EYB.ServiceEntity.SystemEntity;
using EYB.ServiceEntity.BaseInfo;
namespace EYB.Web.Controllers
{
    /// <summary>
    /// 员工管理
    /// </summary>
    public class PersonnelManageController : BaseController
    {
        #region 依赖注入
        private IPersonnelManageBLL personnelBLL;
        private ISystemManageBLL SystemManageBLL;
        public PersonnelManageController()
        {
            personnelBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
            SystemManageBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();
        }
        #endregion 依赖注入

        #region==Get请求==
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

            var list = personnelBLL.GetAllUser(new HospitalUserEntity { HospitalID = entity.HospitalID, IsActive = -1 });
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

            //非系统用户、非老板角色
            list = list.Where(i => i.IsSys != 1 && i.IsAdmin != 1).ToList();

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
        /// 员工弹出看页面
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonList(HospitalUserEntity entity)
        {
            if (entity.HospitalID == 0)
            {
                entity.HospitalID = LoginUserEntity.HospitalID;
            }
            var list = personnelBLL.GetAllUser(new HospitalUserEntity { HospitalID = entity.HospitalID, IsActive = -1 });
            if (!String.IsNullOrEmpty(entity.UserName))
            {
                list = list.Where(c => (c.UserName != null) && c.UserName.Contains(entity.UserName)).ToList();
            }
            if (!String.IsNullOrEmpty(entity.DutyName))
            {
                list = list.Where(c => (c.DutyName != null) && c.DutyName.Contains(entity.DutyName)).ToList();
            }
            list = list.Where(i => i.IsSys != 1 && i.IsActive == 1).ToList();

            list = SortHelper.DataSorting<HospitalUserEntity>(list, entity.orderField, entity.orderDirection).ToList();
            var result = list.AsQueryable().ToPagedList(entity.pageNum, 50);
            result.TotalItemCount = list.Count();
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            ViewBag.HospitalID = entity.HospitalID;
            if (Request.IsAjaxRequest())
                return PartialView("_PersonnelManagePage", result);
            return View(result);
        }
        /// <summary>
        /// 员工管理页面 ---选择控件
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonnelSelectManagePage(HospitalUserEntity entity)
        {
            //entity.pageNum = 5000;
            var list = personnelBLL.GetAllUser(new HospitalUserEntity { HospitalID = LoginUserEntity.HospitalID });
            if (!String.IsNullOrEmpty(entity.UserName))
            {
                list = list.Where(c => c.UserName.Contains(entity.UserName)).ToList();
            }
            list = list.Where(i => i.IsSys != 1).ToList();

            list = SortHelper.DataSorting<HospitalUserEntity>(list, entity.orderField, entity.orderDirection).ToList();
            var result = list.AsQueryable().ToPagedList(entity.pageNum, 10);
            result.TotalItemCount = list.Count();
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            if (Request.IsAjaxRequest())
                return PartialView("_PersonnelSelectManagePage", result);
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
        /// 员工控件
        /// </summary>
        /// <returns></returns>
        public ActionResult _BaseInfoControl()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }

        public ActionResult _PersonnelNoticeContro()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }
        public ActionResult _PersonnelSetControl()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return PartialView();
        }

        public ActionResult _PersonnelManagePage()
        {
            return View();
        }

        /// <summary>
        /// 员工编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonnelEdit()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;//获取门店ID,根据当前登录者信息获取
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            int row;
            int pagecount;
            HospitalEntity entity = new HospitalEntity();
            entity.ID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 5000;
            var list = personnelBLL.GetOtherHospitallist(entity, out row, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, 5000);
            return View(result);
        }
        /// <summary>
        /// 新建员工
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonnelNew()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;//获取门店ID,根据当前登录者信息获取
            ViewBag.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            int row;
            int pagecount;
            HospitalEntity entity = new HospitalEntity();
            entity.ID = LoginUserEntity.ParentHospitalID;
            entity.numPerPage = 1000;
            var list = personnelBLL.GetOtherHospitallist(entity, out row, out pagecount);
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            return View(result);
        }

        /// <summary>
        /// 新建通知
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonnelNoticeNew()
        {
            return View();
        }
        public ActionResult PersonnelNoticeDetail()
        {
            return View();
        }

        /// <summary>
        /// 我接收的通知列表-----已读
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonnelNoticeReciev(NoticeEntity entity)
        {
            int rows = 0;
            entity.numPerPage = 10;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.UserID = LoginUserEntity.UserID;
            entity.IsRead = 1;
            var list = personnelBLL.GetNotcieReciveList(entity, out rows);
            //list = SortHelper.DataSorting<NoticeEntity>(list, entity.orderField, entity.orderDirection).ToList();
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            return View(result);
        }
        /// <summary>
        /// 我接收的通知列表-----未读
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonnelNoticeNoReciev(NoticeEntity entity)
        {
            int rows = 0;
            entity.numPerPage = 10;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.UserID = LoginUserEntity.UserID;
            entity.IsRead = 0;
            var list = personnelBLL.GetNotcieReciveList(entity, out rows);
            // list = SortHelper.DataSorting<NoticeEntity>(list, entity.orderField, entity.orderDirection).ToList();
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            return View(result);
        }
        /// <summary>
        /// 搜索全部未读通知
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchNotice(NoticeEntity entity)
        {
            if (entity.Title == "搜索") entity.Title = "";
            int rows = 0;
            entity.numPerPage = 10;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.UserID = LoginUserEntity.UserID;
            entity.IsRead = 999;
            var list = personnelBLL.GetNotcieReciveList(entity, out rows);
            // list = SortHelper.DataSorting<NoticeEntity>(list, entity.orderField, entity.orderDirection).ToList();
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            return View(result);
        }
        /// <summary>
        /// 我发送通知列表
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonnelNoticeSend(NoticeEntity entity)
        {
            if (entity.Title == "搜索") entity.Title = "";
            int rows = 0;
            entity.numPerPage = 10;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.UserID = LoginUserEntity.UserID;
            entity.orderField = "ID";
            entity.orderDirection = "desc";
            var list = personnelBLL.GetNotcieList(entity, out rows);
            //  list = SortHelper.DataSorting<NoticeEntity>(list, entity.orderField, entity.orderDirection).ToList();
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            return View(result);

        }
        /// <summary>
        /// 我接收的通知列表-- 弹出框
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonNotice(NoticeEntity entity)
        {
            int rows = 0;
            entity.numPerPage = 5;
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.UserID = LoginUserEntity.UserID;
            entity.IsRead = 0;
            var list = personnelBLL.GetNotcieReciveList(entity, out rows);
            // list = SortHelper.DataSorting<NoticeEntity>(list, entity.orderField, entity.orderDirection).ToList();
            var result = list.AsQueryable().ToPagedList(1, entity.numPerPage);
            result.TotalItemCount = rows;
            result.CurrentPageIndex = entity.pageNum;
            ViewBag.orderField = entity.orderField;
            ViewBag.orderDirection = entity.orderDirection;
            return View(result);
        }

        /// <summary>
        /// 下一篇
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNextMsgInfo(string Type, int index)
        {
            NoticeEntity info = new NoticeEntity();
            int rows = 0;
            List<NoticeEntity> list = new List<NoticeEntity>();
            //已发送通知
            if (Type == "1")
            {
                info.numPerPage = 10;
                info.HospitalID = ViewBag.HospitalID;
                info.UserID = ViewBag.UserID;
                info.IsRead = 1;
                list = personnelBLL.GetNotcieReciveList(info, out rows);
            }
            else if (Type == "2")
            {//未读通知
                info.numPerPage = 10;
                info.HospitalID = ViewBag.HospitalID;
                info.UserID = ViewBag.UserID;
                info.IsRead = 0;
                list = personnelBLL.GetNotcieReciveList(info, out rows);
            }
            else if (Type == "4")
            {
                info.numPerPage = 10;
                info.HospitalID = ViewBag.HospitalID;
                info.UserID = ViewBag.UserID;
                info.IsRead = 999;
                list = personnelBLL.GetNotcieReciveList(info, out rows);
            }
            else
            {//已读通知
                info.numPerPage = 10;
                info.HospitalID = ViewBag.HospitalID;
                info.UserID = ViewBag.UserID;
                list = personnelBLL.GetNotcieList(info, out rows);
            }

            if (list.Count > 0 && (index <= list.Count))
                info = list[index - 1];
            return Json(info);
        }
        #endregion

        #region==Ajax请求==
        public ArrayList GetDate()
        {
            var list = personnelBLL.GetDate(1, "2014-05");
            return list;
        }
        /// <summary>
        /// 添加员工
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddPersonnel(HospitalUserEntity entity)
        {
            if (entity.Calendar == "1")
            {
                entity.Calendar = "农历";
            }
            else
            {
                entity.Calendar = "公历";
            }
            var result = personnelBLL.GetUserEntityByAccount(entity.LoginAccount);
            if (result != null && result.UserID != 0)
            {
                return Json(-1);
                //return this.SuccessDirect("操作失败,该账号无效！", "/PersonnelManage/PersonnelNew");
            }
            if (entity.HospitalID == 0) { return Json(-1); }

            //调用图片上传 ，返回图片地址   上传头像
            string[] albumArr1 = Request.Form.GetValues("hid_photo_name1");
            if (albumArr1 != null && albumArr1.Length > 0)
            {
                for (int i = 0; i < albumArr1.Length; i++)
                {
                    string[] imgArr = albumArr1[i].Split('|');
                    if (imgArr.Length == 3)
                    {
                        entity.Photo = imgArr[1];
                    }
                }
            }
            //  entity.Photo = BufferToDiskByDate();
            var menulist = Request.Form.GetValues("UIControlID");
            List<Guid> guid = new List<Guid>();
            //获取选中菜单
            if (menulist != null)
            {
                foreach (var info in menulist)
                {
                    if (!String.IsNullOrEmpty(info))
                        guid.Add(new Guid(info));
                }
            }
            //  entity.HospitalID = loginUserEntity.HospitalID;//获取门店ID,根据当前登录者信息获取
            entity.ParentHospitalID = LoginUserEntity.ParentHospitalID;
            entity.IsActive = 1;
            entity.IsAdmin = 0;

            entity.IsSys = string.IsNullOrEmpty(entity.LoginAccount) ? 0 : 2;//默认是系统用户
            Random rd = new Random();
            entity.LoginAccount = (entity.LoginAccount == null ? entity.UserName + rd.Next(10, 999) : entity.LoginAccount);
            entity.Password = "123456";
            //entity.HospitalID = loginUserEntity.HospitalID;

            int userId = personnelBLL.AddUser(entity);




            //保存用户菜单关系
            // SystemManageBLL.SaveUserMenuRight(guid, "1", userID.ToString());


            if (userId > 0)
            {
                return Json(1);
            }
            else
            {
                return Json(-1);
            }
        }
        /// <summary>
        /// 修改员工
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult UpdatePersonnel(HospitalUserEntity entity)
        {
            if (entity.Calendar == "1")
            {
                entity.Calendar = "农历";
            }
            else
            {
                entity.Calendar = "公历";
            }
            //调用图片上传 ，返回图片地址   上传头像
            string[] albumArr1 = Request.Form.GetValues("hid_photo_name1");
            if (albumArr1 != null && albumArr1.Length > 0)
            {
                for (int i = 0; i < albumArr1.Length; i++)
                {
                    string[] imgArr = albumArr1[i].Split('|');
                    if (imgArr.Length == 3)
                    {
                        entity.Photo = imgArr[1];
                    }
                }
            }
            var menulist = Request.Form.GetValues("UIControlID");
            List<Guid> guid = new List<Guid>();
            //获取选中菜单
            if (menulist != null)
            {
                foreach (var info in menulist)
                {
                    if (!String.IsNullOrEmpty(info))
                        guid.Add(new Guid(info));
                }
            }
            //entity.HospitalID = loginUserEntity.HospitalID;//获取门店ID,根据当前登录者信息获取

            int userID = personnelBLL.UpdateUser(entity);
            //保存用户菜单关系
            //SystemManageBLL.SaveUserMenuRight(guid, "1", entity.UserID.ToString());
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (userID > 0)
            {
                return Json(1);
            }
            else
            {
                return Json(-1);
            }

        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteDepartment(int UserID)
        {
            var result = personnelBLL.BatchActive(new List<int> { UserID }, 0);
            return Json(result);
        }
        /// <summary>
        /// 还原用户
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult RestoreDepartment(int UserID)
        {
            int result = personnelBLL.BatchActive(new List<int> { UserID }, 1);
            return Json(result);
        }

        /// <summary>
        /// 离职操作
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>

        public ActionResult lizhiDepartment(int UserID)
        {
            var result = 0;
            if (UserID > 0)
                result = personnelBLL.UpdateIsSeparation(UserID, 1);
            return Json(result);
        }

        /// <summary>
        /// 入职操作
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>

        public ActionResult ruzhizhiDepartment(int UserID)
        {
            var result = 0;
            if (UserID > 0)
                result = personnelBLL.UpdateIsSeparation(UserID, 0);
            return Json(result);
        }



        /// <summary>
        /// 检查账号是否唯一
        /// </summary>
        /// <param name="loginAccount"></param>
        /// <returns></returns>
        public ActionResult GetUserEntityByAccount(string loginAccount)
        {
            var entity = personnelBLL.GetUserEntityByAccount(loginAccount);
            if (entity != null && entity.UserID != 0)
            {
                return Json(1);
            }
            return Json(-1);
        }

        /// <summary>
        /// 检查账号是否唯一
        /// </summary>
        /// <param name="loginAccount"></param>
        /// <returns></returns>
        public ActionResult GetUserEntityByAccountHavePrefix(string loginAccount)
        {
            //var user_shouzimu = EYB.Web.Code.PageFunction.GetSpellCode(loginUserEntity.Name).ToLower();
            var Account = SystemManageBLL.GetParentAccount(LoginUserEntity.ParentHospitalID).ToLower();

            var entity = personnelBLL.GetUserEntityByAccount(Account + loginAccount);
            if (entity != null && entity.UserID != 0)
            {
                return Json(1);
            }
            return Json(-1);
        }


        /// <summary>
        /// 重置密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ResetPwd(int UserID)
        {
            int result = personnelBLL.ResetPassword(UserID, Encrypt.MD5("123456"));
            return Json(result);
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdatePwd(int UserID, string OldPwd, string Password)
        {
            if (LoginUserEntity.Password != Encrypt.MD5(OldPwd))
            {
                return Json(-2);
            }
            int result = personnelBLL.UpdatePassowrd(UserID, Password);
            if (result > 0)
            {
                return Json(1);
            }
            else
            {
                return Json(-1);

            }
        }

        /// <summary>
        /// 修改个人资料
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult UpdatePersonnelDetail(HospitalUserEntity entity)
        {
            //调用图片上传 ，返回图片地址   上传头像
            string[] albumArr1 = Request.Form.GetValues("hid_photo_name1");
            if (albumArr1 != null && albumArr1.Length > 0)
            {
                for (int i = 0; i < albumArr1.Length; i++)
                {
                    string[] imgArr = albumArr1[i].Split('|');
                    if (imgArr.Length == 3)
                    {
                        entity.Photo = imgArr[1];
                    }
                }
            }

            entity.HospitalID = LoginUserEntity.HospitalID;//获取门店ID,根据当前登录者信息获取
            int userID = personnelBLL.UpdateUser(entity);
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            if (userID > 0)
            {
                return Json(1);
            }
            else
            {
                return Json(-1);
            }

        }

        /// <summary>
        /// 添加通知
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string AddNotice(NoticeEntity entity)
        {
            entity.HospitalID = LoginUserEntity.HospitalID;
            entity.UserID = LoginUserEntity.UserID;
            entity.UserName = LoginUserEntity.UserName;
            int result = personnelBLL.AddNotcie(entity);
            if (result > 0)
            {
                return "操作成功";// this.SuccessDirect("操作成功", "/PersonnelManage/PersonnelNoticeNew");

            }
            else
            {
                return "操作失败"; //this.SuccessDirect("操作失败", "/PersonnelManage/PersonnelNoticeNew");
            }
        }
        /// <summary>
        /// 设为已读
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult SetRead(List<int> ids)
        {
            int result = personnelBLL.BatchNotcie(ids, 2, LoginUserEntity.UserID);
            return Json(result);
        }
        /// <summary>
        /// 删除--接收者
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult DeleteNotice(List<int> ids)
        {
            int result = personnelBLL.BatchNotcie(ids, 1, LoginUserEntity.UserID);
            return Json(result);
        }
        /// <summary>
        /// 删除--发送者
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult DeleteNoticeSend(List<int> ids)
        {
            int result = personnelBLL.BatchNotcieSend(ids, LoginUserEntity.UserID);
            return Json(result);
        }

        public ActionResult HospitalUserWeixinbindRemove()
        {
            var result = 0;
            var UserID = HS.SupportComponents.ConvertValueHelper.ConvertIntValue(Request["UserID"]);
            if (UserID > 0)
            {
                result = personnelBLL.HospitalUserWeixinbindRemove(UserID);
            }
            return Json(result);
        }
        #endregion


    }
}
