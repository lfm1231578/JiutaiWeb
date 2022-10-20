using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SystemManage.Factory.IBLL;
using Common.Attribute;
using HS.SupportComponents;
using EYB.ServiceEntity.SystemEntity;
//using EYB.Web.SerialNumber;

namespace EYB.Web.Controllers
{
    public class SystemManagementController : BaseController
    {
        #region 依赖注入
        private ISystemManageBLL systemBLL;
        public SystemManagementController()
        {
            systemBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<ISystemManageBLL>();
        }
        #endregion 依赖注入

        #region==get请求==


        #region==用户管理==



        /// <summary>
        /// 添加用户
        /// </summary>
        /// <returns></returns>
        public ActionResult AddUserView(UserEntity entity)
        {
            ViewBag.UserID = entity.UserID;
            return View();
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <returns></returns>
        public ActionResult UserManageAdd()
        {
            return View();
        }
        /// <summary>
        /// 添加账号
        /// </summary>
        /// <returns></returns>
        public ActionResult AccountManageAdd()
        {
            return View();
        }
        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <returns></returns>
        public ActionResult UserManageUpdate()
        {
            return View();
        }
        /// <summary>
        /// 成功页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Success()
        {
            return View();
        }
        /// <summary>
        /// 用户详情
        /// </summary>
        /// <returns></returns>
        public ActionResult UserDetail()
        {
            return View();
        }
        #endregion

        #region==角色管理==

        /// <summary>
        /// 角色管理页面
        /// </summary>
        /// <returns></returns>
        public ActionResult RoleManage()
        {
            return View();
        }

        /// <summary>
        /// 添加角色
        /// </summary>
        /// <returns></returns>
        public ActionResult RoleManageAdd()
        {
            return View();
        }

        /// <summary>
        /// 修改角色
        /// </summary>
        /// <returns></returns>
        public ActionResult RoleManageUpdate()
        {
            return View();
        }
        /// <summary>
        /// 设置菜单权限
        /// </summary>
        /// <returns></returns>
        public ActionResult SetMenuRight()
        {
            return View();
        }
        /// <summary>
        /// 设置数据权限
        /// </summary>
        /// <returns></returns>
        public ActionResult RoleManageDataRight()
        {
            return View();
        }

        #endregion

        #region==日志管理==
        /// <summary>
        /// 操作日志
        /// </summary>
        /// <returns></returns>
        public ActionResult OperateLog()
        {
            return View();
        }
        #endregion


        #region==基础信息设置==
        public ActionResult BaseinfoSet()
        {
            return View();
        }
        #endregion

        #region==部门管理==
        public ActionResult DepartmentManage()
        {
            return View();
        }
        #endregion

        #region==地区管理==
        /// <summary>
        /// 修改地区页面
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateArea()
        {
            return View();
        }
        /// <summary>
        /// 添加地区页面
        /// </summary>
        /// <returns></returns>
        public ActionResult AddArea()
        {
            return View();
        }


        #endregion
        #endregion

        #region==Ajax请求==
        #region 用户管理

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetUserList(UserEntity param)
        {
            return null;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddUser(UserEntity entity)
        {
            entity.Password = Encrypt.MD5(entity.Password);
            if (string.IsNullOrEmpty(entity.LoginAccount))
            {
                entity.LoginAccount = entity.UserName;
            }
            var result = systemBLL.AddUser(entity);
            return this.SuccessDirect("添加用户成功", "/SystemManagement/UserManage", 3, "add");
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult UpdateUser(UserEntity entity)
        {

            var result = systemBLL.UpdateUser(entity);

            return this.SuccessDirect("修改用户成功", "", 3, "edit");
        }

        /// <summary>
        /// 启用/禁用用户
        /// </summary>
        /// <param name="userids"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ActivityUser(string userids, int status)
        {
            if (!string.IsNullOrEmpty(userids))
            {
                List<int> useridlist = new List<int>();
                foreach (var info in userids.Split(','))
                {
                    useridlist.Add(ConvertValueHelper.ConvertIntValue(info));
                }
                systemBLL.BatchActive(useridlist, status);
            }
            return Json(1);
        }



        /// <summary>
        /// 获取单个用户，进行相应的编辑操作
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetUserById(string userid)
        {
            int result = 0;
            int.TryParse(userid, out result);
            return Json(systemBLL.GetUserByUserID(result));
        }

        /// <summary>
        /// 查询用户是否存在
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserEntityByAccount(string user_name)
        {
            var entity = systemBLL.GetUserEntityByAccount(user_name);
            if (entity != null && entity.UserID != 0)
            {
                return Content("0");
            }
            return Content("1");

        }

        /// <summary>
        /// 重置密码（一般是重置“123456”）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ResetPassword(int userid)
        {
            var pwd = Encrypt.MD5("123456");
            return Json(systemBLL.ResetPassword(userid, pwd));
        }


        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public void UpdatePassowrd(string oldpwd, string newpwd, string QRPassword)
        {

        }


        #endregion

        #region==角色管理==
        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetRoleList(string sidx, string sord, int page, int rows)
        {

            var list = systemBLL.GetAllRole(LoginUserEntity.HospitalID);
            list = list.DataSorting(sidx, sord).ToList();
            //排序
            //list = SortHelper.DataSorting(list, systemBLL.sidx, systemBLL.sord).ToList();
            var pageCount = list.Count == rows ? rows + 1 : rows;
            var pageindex = page;
            var jsondata = new
            {
                total = pageCount,
                page = pageindex,
                rows = from entity in list
                       select new
                       {
                           cell = new string[]{
                                            entity.RoleID.ToString(),
                                            entity.RoleName,
                                            entity.Memo,
                                            //entity.IsActive.ToString()=="1"? "是":"否",
                                            string.Format("<a href='javascript:;' onclick='edit(\"" + entity.RoleID + "\",\"" +entity.RoleName + "\")' >编辑</a>")
                                                }
                       }
            };
            return Json(jsondata);
        }
        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddRole(RoleEntity role)
        {
            role.IsActive = 1;
            systemBLL.AddRole(role);
            return this.SuccessDirect("添加角色成功", "/Admin/SystemManagement/RoleManage", 3, "add");
        }
        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditRole(RoleEntity role)
        {

            systemBLL.UpdateRole(role);
            return this.SuccessDirect("修改角色成功", "/Admin/SystemManagement/RoleManage", 3, "edit");
        }
        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="rolelist"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteRole(string rolelist)
        {

            if (!string.IsNullOrEmpty(rolelist))
            {
                List<int> role = new List<int>();
                foreach (var info in rolelist.Split(','))
                {
                    role.Add(ConvertValueHelper.ConvertIntValue(info));
                }
                var result = systemBLL.DeleteRoleByRoleID(role);
            }
            return View("RoleManage");
        }
        /// <summary>
        /// 保存用户角色关系
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="userids"></param>
        /// <returns></returns>
        public ActionResult SaveRoleUserRelation(string RoleID, string userids)
        {
            int result = -1;
            var array = userids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in array)
            {
                result = systemBLL.SaveRoleUserRelation(s, RoleID);
            }
            return Json(result);
        }


        #endregion

        #region 获取角色菜单

        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetMenuTree(int roleid)
        {
            List<UIControlEntity> uiControlEntities = systemBLL.GetMenuList(1,1);
            List<int> roleList = new List<int>();
            roleList.Add(roleid);
            List<string> rightEntity = systemBLL.GetMenu(roleList);
            JsonTreeNodeEntity rootnode = new JsonTreeNodeEntity();
            rootnode.id = "";
            rootnode.text = "全部";
            rootnode.value = "";
            rootnode.showcheck = true;
            rootnode.isexpand = true;
            rootnode.ChildNodes = IterationChilds(uiControlEntities, rightEntity, "BEF5CCF2-18C0-46a6-9B91-E984BEB9BFEF".ToLower());
            rootnode.hasChildren = rootnode.ChildNodes != null && rootnode.ChildNodes.Count > 0;
            List<JsonTreeNodeEntity> nodelist = new List<JsonTreeNodeEntity>();
            nodelist.Add(rootnode);
            return Json(nodelist);
        }

        /// <summary>
        /// 迭代树菜单
        /// </summary>
        /// <param name="table"></param>
        /// <param name="parentCode"></param>
        /// <returns></returns>
        private List<JsonTreeNodeEntity> IterationChilds(List<UIControlEntity> uiControlEntities, List<string> rightEntity, string parentCode)
        {
            var JsonTreeNodeEntitys = new List<JsonTreeNodeEntity>();
            List<UIControlEntity> controlEntities = uiControlEntities.Where(c => c.ParentUIControlID.ToString() == parentCode).ToList();
            if (controlEntities != null && controlEntities.Count > 0)
            {
                foreach (UIControlEntity row in controlEntities)
                {
                    var node = new JsonTreeNodeEntity();
                    node.id = row.UIControlID.ToString();
                    node.text = row.UIControlName;
                    node.value = GetMenuRgihtValue(rightEntity, row.UIControlID.ToString());
                    node.showcheck = true;
                    node.checkstate = 0;
                    node.ChildNodes = IterationChilds(uiControlEntities, rightEntity, row.UIControlID.ToString());
                    node.hasChildren = node.ChildNodes != null && node.ChildNodes.Count > 0;
                    node.isexpand = true;
                    JsonTreeNodeEntitys.Add(node);
                }
            }
            return JsonTreeNodeEntitys;
        }

        private string GetMenuRgihtValue(List<string> rightEntity, string uIControlID)
        {
            if (rightEntity != null && rightEntity.Count > 0)
            {
                foreach (string temp in rightEntity)
                {
                    if (temp == uIControlID)
                    {
                        return "1";
                    }
                }
            }
            return "2";
        }

        #endregion

        #region==保存角色权限配置==

        /// <summary>
        /// 保存功能授权
        /// </summary>
        /// <param name="menuId">权限编号</param>
        /// <param name="roleId">角色</param>
        /// <returns></returns>
        public JsonResult SaveRight(string menuId, int roleId)
        {
            var arry = menuId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int result = 0;
            List<Guid> list = new List<Guid>();
            //排序
            // list = SortHelper.DataSorting(list, param.Sidx, param.Sord).ToList();
            foreach (var s in arry)
            {
                var temp = s.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                if (temp.Length == 2 && temp[1] == "1")
                {
                    Guid id = new Guid(temp[0]);
                    list.Add(id);
                }
            }
            result = systemBLL.SaveMenuRight(list, "1", roleId + "");
            return Json(result);
        }

        #endregion

        #region==保存用户权限配置==

        /// <summary>
        /// 保存功能授权
        /// </summary>
        /// <param name="menuId">权限编号</param>
        /// <param name="roleId">用户</param>
        /// <returns></returns>
        public JsonResult SaveUserRight(string menuId, int userid)
        {
            var arry = menuId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int result = 0;
            List<Guid> list = new List<Guid>();
            //排序
            //list = SortHelper.DataSorting(list, param.Sidx, param.Sord).ToList();
            foreach (var s in arry)
            {
                var temp = s.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                if (temp.Length == 2 && temp[1] == "1")
                {
                    Guid id = new Guid(temp[0]);
                    list.Add(id);
                }
            }
            result = systemBLL.SaveUserMenuRight(list, LoginUserEntity.UserID.ToString(), userid + "");
            return Json(result);
        }

        #endregion

        #region 获取用户菜单

        /// <summary>
        /// 获取用户菜单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetUserMenuTree(int userid)
        {
            List<UIControlEntity> uiControlEntities = systemBLL.GetMenuList(1,1);
            List<int> useridList = new List<int>();
            useridList.Add(userid);
            List<string> rightEntity = systemBLL.GetUserMenu(useridList);
            JsonTreeNodeEntity rootnode = new JsonTreeNodeEntity();
            rootnode.id = "";
            rootnode.text = "全部";
            rootnode.value = "";
            rootnode.showcheck = true;
            rootnode.isexpand = true;
            rootnode.ChildNodes = IterationUserrightChilds(uiControlEntities, rightEntity, "BEF5CCF2-18C0-46a6-9B91-E984BEB9BFEF".ToLower());
            rootnode.hasChildren = rootnode.ChildNodes != null && rootnode.ChildNodes.Count > 0;
            List<JsonTreeNodeEntity> nodelist = new List<JsonTreeNodeEntity>();
            nodelist.Add(rootnode);
            return Json(nodelist);
        }

        /// <summary>
        /// 迭代树菜单
        /// </summary>
        /// <param name="table"></param>
        /// <param name="parentCode"></param>
        /// <returns></returns>
        private List<JsonTreeNodeEntity> IterationUserrightChilds(List<UIControlEntity> uiControlEntities, List<string> rightEntity, string parentCode)
        {
            var JsonTreeNodeEntitys = new List<JsonTreeNodeEntity>();
            List<UIControlEntity> controlEntities = uiControlEntities.Where(c => c.ParentUIControlID.ToString() == parentCode).ToList();
            if (controlEntities != null && controlEntities.Count > 0)
            {
                foreach (UIControlEntity row in controlEntities)
                {
                    var node = new JsonTreeNodeEntity();
                    node.id = row.UIControlID.ToString();
                    node.text = row.UIControlName;
                    node.value = GetUserMenuRgihtValue(rightEntity, row.UIControlID.ToString());
                    node.showcheck = true;
                    node.checkstate = 0;
                    node.ChildNodes = IterationChilds(uiControlEntities, rightEntity, row.UIControlID.ToString());
                    node.hasChildren = node.ChildNodes != null && node.ChildNodes.Count > 0;
                    node.isexpand = true;
                    JsonTreeNodeEntitys.Add(node);
                }
            }
            return JsonTreeNodeEntitys;
        }

        private string GetUserMenuRgihtValue(List<string> rightEntity, string uIControlID)
        {
            if (rightEntity != null && rightEntity.Count > 0)
            {
                foreach (string temp in rightEntity)
                {
                    if (temp == uIControlID)
                    {
                        return "1";
                    }
                }
            }
            return "2";
        }

        #endregion

        #region==日志管理==
        /// <summary>
        /// 日志查询
        /// </summary>
        /// <param name="sidx"></param>
        /// <param name="sord"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="account"></param>
        /// <param name="keyword"></param>
        /// <param name="module"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public ActionResult GetLogList(string sidx, string sord, int page, int rows, string account, string keyword, string module, string startTime, string endTime)
        {
            var list = new List<SysOperateLogEntity>();// systemBLL.GetSystemLog(account, keyword, module, startTime, endTime);
            var pageCount = list.Count % rows == 0 ? list.Count / rows : list.Count / rows + 1;
            var pageindex = page;
            list = list.DataSorting(sidx, sord).ToList();

            var jsondata = new
            {
                rows = from entity in list
                       select new
                       {
                           cell = new string[]{
                                            entity.OccurTime.ToString(),
                                            entity.LoginAccount,
                                            entity.OperationType,
                                            entity.ClientIP,
                                            entity.ModuleName,
                                            entity.Action
                                                }
                       }
            };
            return Json(jsondata);
        }
        #endregion

        #region==基础信息管理==

        /// <summary>
        /// 获取所有的基础分类
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBaseInfoTree()
        {
            List<BaseinfoEntity> baseinfolist = systemBLL.GetBaseInfoList();
            JsonTreeNodeEntity rootnode = new JsonTreeNodeEntity();
            rootnode.id = "";
            rootnode.text = "全部类型";
            rootnode.value = "";
            rootnode.showcheck = false;
            rootnode.isexpand = true;
            rootnode.ChildNodes = IterationChilds(baseinfolist);

            rootnode.hasChildren = rootnode.ChildNodes != null && rootnode.ChildNodes.Count > 0;
            List<JsonTreeNodeEntity> nodelist = new List<JsonTreeNodeEntity>();
            nodelist.Add(rootnode);
            return Json(nodelist);
        }

        /// <summary>
        /// 获取基础信息分类
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBaseInfoType()
        {
            List<BaseinfoEntity> baseinfolistAll = systemBLL.GetBaseInfoType();
            List<BaseinfoEntity> baseinfolist = new List<BaseinfoEntity>();
            foreach (var i in baseinfolistAll)
            {
                if ((i.InfoCode == "CustomerType" && i.InfoType == "会员级别")||(i.InfoCode == "MemberType" && i.InfoType == "费用项目名称") || (i.InfoCode == "ProductSpecification" && i.InfoType == "费用项目名称"))
                { } else {
                    if (i.InfoCode != null )
                    baseinfolist.Add(i);
                     }
            }
            JsonTreeNodeEntity rootnode = new JsonTreeNodeEntity();
            rootnode.id = "";
            rootnode.text = "全部类型";
            rootnode.value = "";
            rootnode.showcheck = false;
            rootnode.isexpand = true;
            rootnode.ChildNodes = IterationChilds(baseinfolist);

            rootnode.hasChildren = rootnode.ChildNodes != null && rootnode.ChildNodes.Count > 0;
            List<JsonTreeNodeEntity> nodelist = new List<JsonTreeNodeEntity>();
            nodelist.Add(rootnode);
            return Json(nodelist);
        }

        /// <summary>
        /// 迭代树菜单
        /// </summary>
        /// <param name="table"></param>
        /// <param name="parentCode"></param>
        /// <returns></returns>
        private List<JsonTreeNodeEntity> IterationChilds(List<BaseinfoEntity> baseinfolist)
        {
            var JsonTreeNodeEntitys = new List<JsonTreeNodeEntity>();
            if (baseinfolist != null && baseinfolist.Count > 0)
            {
                foreach (BaseinfoEntity row in baseinfolist)
                {
                    var node = new JsonTreeNodeEntity();
                    node.id = row.ID.ToString();
                    node.text = row.InfoType;
                    node.value = row.InfoCode;
                    node.showcheck = false;
                    node.checkstate = 0;
                    if (row.InfoCode == "Payment")
                    {
                        var list = systemBLL.GetBaseInfoTreeByParentID(0,"Payment", 1, LoginUserEntity.ParentHospitalID);
                        var JsonTreeNodeEntitys1 = new List<JsonTreeNodeEntity>();
                        foreach (BaseinfoEntity row1 in list)
                        {
                            var node1 = new JsonTreeNodeEntity();
                            node1.id = row1.ID.ToString();
                            node1.text = row1.InfoName;
                            node1.value = row1.InfoCode;
                            node1.showcheck = false;
                            node1.checkstate = 0;
                            node1.ChildNodes = null;
                            node1.hasChildren = false;
                            node1.isexpand = true;
                            JsonTreeNodeEntitys1.Add(node1);
                        }
                        node.ChildNodes = JsonTreeNodeEntitys1;
                        node.hasChildren = true;
                    }
                    else {
                        node.ChildNodes = null;
                        node.hasChildren = false;
                    }

                    node.isexpand = true;
                    JsonTreeNodeEntitys.Add(node);
                }
            }
            return JsonTreeNodeEntitys;
        }
        /// <summary>
        /// 根据类型获取信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBaseInfoTreeByType(string sidx, string sord, int page, int rows, string InfoType)
        {
            var list = systemBLL.GetBaseInfoTreeByType(InfoType,1,LoginUserEntity.ParentHospitalID);
            list = list.DataSorting(sidx, sord).ToList();
            var pageCount = list.Count == rows ? rows + 1 : rows;
            var pageindex = page;
            var jsondata = new
            {
                total = pageCount,
                page = pageindex,
                rows = from entity in list
                       select new
                       {
                           cell = new string[]{
                                            entity.ID.ToString(),
                                            entity.InfoType,
                                              entity.InfoCode,
                                            entity.InfoName,
                                            entity.parentID.ToString(),
                                            entity.InfoDesc,

                                            entity.SortID.ToString()
                                                }
                       }
            };
            return Json(jsondata);
        }
        /// <summary>
        /// 添加分类
        /// </summary>
        /// <param name="InfoType"></param>
        /// <returns></returns>
        public ActionResult AddBaseInfo(string InfoType)
        {
            var result = systemBLL.AddBaseInfo(InfoType);
            return Json(result);
        }

        /// <summary>
        /// 修改分类
        /// </summary>
        /// <param name="InfoType"></param>
        /// <returns></returns>
        public ActionResult UpdateBaseInfo(string InfoType, string oldInfotype)
        {
            var result = systemBLL.UpdateBaseInfo(InfoType, oldInfotype);
            return Json(result);
        }


        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="InfoType"></param>
        /// <returns></returns>
        public ActionResult DeleteBaseInfo(string InfoType)
        {
            var result = systemBLL.DeleteBaseInfo(InfoType);
            return Json(result);
        }

        /// <summary>
        /// 添加分类信息
        /// </summary>
        /// <param name="InfoType"></param>
        /// <returns></returns>
        public ActionResult AddBaseInfoEntity(BaseinfoEntity entity)
        {
            var result = systemBLL.AddBaseInfoEntity(entity);
            return Json(result);
        }

        /// <summary>
        /// 修改分类信息
        /// </summary>
        /// <param name="InfoType"></param>
        /// <returns></returns>
        public ActionResult UpdateBaseInfoEntity(BaseinfoEntity entity)
        {
            var result = systemBLL.UpdateBaseInfoEntity(entity);
            return Json(result);
        }
        /// <summary>
        /// 删除分类信息
        /// </summary>
        /// <param name="InfoType"></param>
        /// <returns></returns>
        public ActionResult DeleteBaseInfoEntity(string ids)
        {
            var result = -1;
            var array = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var id in array)
            {
                result = systemBLL.DeleteBaseInfoEntity(id);
            }
            return Json(result);
        }
        #endregion==


        #region==地区管理==

        /// <summary>
        /// 获取地区
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllAreaList(string areacode)
        {
            List<AreaEntity> list = systemBLL.GetAllAreaList(areacode);
            string str = "";
            foreach (var info in list)
            {
                str += string.Format("<a href='javascript:;' type='city' code='{0}'>{1}</a>", info.AreaCode, info.AreaName);
            }
            return Content(str);
        }

        /// <summary>
        /// 获取地区
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllAreaListNo(string areacode)
        {
            List<AreaEntity> list = systemBLL.GetAllAreaList(areacode);
            return Json(list);
        }


        /// <summary>
        /// 地区管理
        /// </summary>
        /// <returns></returns>
        public ActionResult AreaManage()
        {
            List<AreaEntity> list = systemBLL.GetAllAreaList("");
            return View(IterationChildsFor(list, "-1"));
        }

        /// <summary>
        /// 修改地区
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateArea(AreaEntity entity)
        {
            int result = 0;
            try
            {
                result = systemBLL.UpdateArea(entity);
                return this.SuccessDirect("操作成功", "/BaseInfoManagement/AreaManage", 2, "edit");
            }
            catch (Exception e)
            {
                result = -1;
                return this.SuccessDirect("操作失败", "/BaseInfoManagement/AreaManage", 2, "edit");
            }

        }
        /// <summary>
        /// 添加地区
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddArea(AreaEntity entity)
        {
            int result = 0;
            try
            {
                result = systemBLL.AddArea(entity);
                return this.SuccessDirect("操作成功", "/BaseInfoManagement/AreaManage", 2, "add");
            }
            catch (Exception e)
            {
                result = -1;
                return this.SuccessDirect("操作失败", "/BaseInfoManagement/AreaManage", 2, "add");
            }
        }
        /// <summary>
        /// 迭代地区
        /// </summary>
        /// <param name="table"></param>
        /// <param name="parentCode"></param>
        /// <returns></returns>
        private List<AreaEntity> IterationChilds(List<AreaEntity> areaEntities, string parentCode)
        {
            var AreaEntitys = new List<AreaEntity>();
            List<AreaEntity> controlEntities = areaEntities.Where(c => c.ParentAreaCode.ToString() == parentCode).ToList();
            if (controlEntities != null && controlEntities.Count > 0)
            {
                foreach (AreaEntity row in controlEntities)
                {

                    AreaEntitys.Add(row);
                    var result = IterationChilds(areaEntities, row.AreaCode.ToString());
                    AreaEntitys.AddRange(result);

                }
            }
            return AreaEntitys;
        }
        /// <summary>
        /// 迭代地区————笨方法
        /// </summary>
        /// <param name="table"></param>
        /// <param name="parentCode"></param>
        /// <returns></returns>
        private List<AreaEntity> IterationChildsFor(List<AreaEntity> areaEntities, string parentCode)
        {
            List<AreaEntity> result = new List<AreaEntity>();
            List<AreaEntity> controlEntities = areaEntities.Where(c => c.ParentAreaCode.ToString() == parentCode).ToList();
            foreach (var area1 in controlEntities)
            {
                AreaEntity entity1 = new AreaEntity();
                entity1 = area1;
                result.Add(entity1);
                var childsubList1 = areaEntities.Where(c => c.ParentAreaCode == area1.AreaCode).ToList();
                foreach (var area2 in childsubList1)
                {
                    AreaEntity entity2 = new AreaEntity();
                    entity2 = area2;
                    entity2.ClassName = "sub_" + area1.AreaCode;
                    result.Add(entity2);
                    var childsubList2 = areaEntities.Where(c => c.ParentAreaCode == area2.AreaCode).ToList();
                    foreach (var area3 in childsubList2)
                    {
                        AreaEntity entity3 = new AreaEntity();
                        entity3 = area3;
                        entity3.ClassName = "sub_" + area2.AreaCode + " sub_" + area1.AreaCode;
                        result.Add(entity3);
                        var childsubList3 = areaEntities.Where(c => c.ParentAreaCode == area3.AreaCode).ToList();
                        foreach (var area4 in childsubList3)
                        {
                            AreaEntity entity4 = new AreaEntity();
                            entity4 = area4;
                            entity4.ClassName = "sub_" + area3.AreaCode + " sub_" + area2.AreaCode + " sub_" + area1.AreaCode;
                            result.Add(entity4);
                            var childsubList4 = areaEntities.Where(c => c.ParentAreaCode == area4.AreaCode).ToList();
                            foreach (var area5 in childsubList4)
                            {
                                AreaEntity entity5 = new AreaEntity();
                                entity5 = area5;
                                entity5.ClassName = "sub_" + area4.AreaCode + " sub_" + area3.AreaCode + " sub_" + area2.AreaCode + " sub_" + area1.AreaCode;
                                result.Add(entity5);

                            }
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region==会员活动==

        #endregion

        #region==APP下载统计==

        public ActionResult AddAppStatistics(AppDownloadEntity entity)
        {
            try
            {
                var nowip = HttpContext.Request.UserHostAddress;
                string IP = nowip == "::1" ? "127.0.0.1" : nowip;
                var ipdetail = IPHelper.GetIPAdd(IP);
                entity.IP = IP;
                entity.IPAddress = ipdetail[0];
                entity.CreateTime = DateTime.Now;
               // SerialNumberSoapClient client = new SerialNumberSoapClient();
                //int result =   client.AddAppStatistics(IP, ipdetail[0], DateTime.Now);
                int result = systemBLL.AddAppStatistics(entity);
                return Json(result);
            }
            catch(Exception e) {
                LogWriter.WriteError(e, "APP下载统计出错");
                return Json(0);
            }
        }

        #endregion

        #endregion

        #region ==用户设置==


        /// <summary>
        /// 获得分类列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserSettingsType(){
            List<UserSettingsEntity> baseinfolist= systemBLL.GetUserSettingsList(new UserSettingsEntity { HospitalID = LoginUserEntity.HospitalID });
            JsonTreeNodeEntity rootnode = new JsonTreeNodeEntity();
            rootnode.id = "";
            rootnode.text = "全部类型";
            rootnode.value = "";
            rootnode.showcheck = false;
            rootnode.isexpand = true;
            rootnode.ChildNodes = IterationChilds(baseinfolist);

            rootnode.hasChildren = rootnode.ChildNodes != null && rootnode.ChildNodes.Count > 0;
            List<JsonTreeNodeEntity> nodelist = new List<JsonTreeNodeEntity>();
            nodelist.Add(rootnode);
            return Json(nodelist);
        }


        /// <summary>
        /// 迭代树菜单
        /// </summary>
        /// <param name="table"></param>
        /// <param name="parentCode"></param>
        /// <returns></returns>
        private List<JsonTreeNodeEntity> IterationChilds(List<UserSettingsEntity> list)
        {
            var JsonTreeNodeEntitys = new List<JsonTreeNodeEntity>();
            if (list != null && list.Count > 0)
            {
                foreach (UserSettingsEntity row in list)
                {
                    var node = new JsonTreeNodeEntity();
                    node.id = row.ID.ToString();
                    node.text = row.Name;
                    node.value = row.AttributeValue;
                    node.memo=row.Memo;
                    node.showcheck = false;
                    node.checkstate = 0;
                    node.ChildNodes = null;
                    node.hasChildren = false;
                    node.isexpand = true;
                    JsonTreeNodeEntitys.Add(node);
                }
            }
            return JsonTreeNodeEntitys;
        }

        #endregion

    }
}
