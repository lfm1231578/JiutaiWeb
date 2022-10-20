using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DepartmentManage.Factory.IBLL;
using EYB.ServiceEntity.DepartmentEntity;
using HS.SupportComponents;
using Webdiyer.WebControls.Mvc;
namespace EYB.Web.Controllers
{
    public class DepartmentManageController : BaseController
    {
        #region 依赖注入
        private IDepartmentManageBLL systemBLL;
        public DepartmentManageController()
        {
            systemBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IDepartmentManageBLL>();
        }
        #endregion 依赖注入

        #region==Get请求==
        /// <summary>
        /// 科室管理页面
        /// </summary>
        /// <returns></returns>
        public ActionResult DepartmentPage(string Name,int pageindex=1)
        {
            if (Name == "搜索") Name = "";
            var list = systemBLL.GetListByHospitalID(LoginUserEntity.ParentHospitalID, 0);
            if (!String.IsNullOrEmpty(Name))
            {
                list = list.Where(c => c.Name.Contains(Name)).ToList();
            }
            var result = list.AsQueryable().ToPagedList(pageindex, 50);
            result.TotalItemCount = list.Count;
            result.CurrentPageIndex = pageindex;
            return View(result);
        }
        /// <summary>
        /// 科室控件
        /// </summary>
        /// <returns></returns>
        public ActionResult _DepartmentControl()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return View();
        }
        /// <summary>
        /// 科室控件
        /// </summary>
        /// <returns></returns>
        public ActionResult _BaseInfoControl()
        {
            ViewBag.IsAdmin = LoginUserEntity.IsAdmin;
            return View();
        }
        /// <summary>
        /// 新建
        /// </summary>
        /// <returns></returns>
        public ActionResult DepartmentNew()
        {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;//所属门店ID
            return View();
        }
        /// <summary>
        /// 科室详情
        /// </summary>
        /// <returns></returns>
        public ActionResult DepartmentDetail()
        {
            return View();
        }
        /// <summary>
        /// 验证
        /// </summary>
        /// <returns></returns>
        public ActionResult Validatetest()
        {
            return View();
        }
        public ActionResult SetDepartment() {
            ViewBag.HospitalID = LoginUserEntity.HospitalID;
            return View();
        }
        #endregion

        #region==Ajax请求==

    

        /// <summary>
        /// 添加修改科室
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult AddDepartment(DepartmentEntity entity)
        {
            entity.HospitalNo = LoginUserEntity.HospitalNo;
            entity.HospitalID = LoginUserEntity.HospitalID;
            var departList = Request.Form.GetValues("DepartUserID");
            if (departList != null)
            {
                foreach (var info in departList)
                {
                    if (!String.IsNullOrEmpty(info))
                        entity.DepartUserIDList.Add(ConvertValueHelper.ConvertIntValue(info));
                }
            }
            var hsuserList = Request.Form.GetValues("HSUserID");
            if (hsuserList != null)
            {
                foreach (var info in hsuserList)
                {
                    if (!String.IsNullOrEmpty(info))
                        entity.HSUserIDList.Add(ConvertValueHelper.ConvertIntValue(info));
                }
            }
            int result = 0;
            if (entity.Type == "add")//添加
            {
                 result = systemBLL.AddDepartment(entity);
            }
            else { //修改
                result = systemBLL.UpDepartment(entity);
            }
            if (result > 0)
            {
                return Json(1);
            }
            else {
                return Json(-1);
            }
        }
        /// <summary>
        /// 删除科室
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult DeleteDepartment(int ID)
        {
            var result =systemBLL.DelDepartment(ID+"");
            if (result > 0)
            {
                return Json(1);
            }
            else {
                return Json(-1);
            }
        }
        #endregion
    }
}
