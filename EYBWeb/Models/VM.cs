using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace EYB.Web.Models
{
    /// <summary>
    /// 为实体提供扩展属性的渲染模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class VM<T> : DynamicObject
    {
        private readonly Func<Dictionary<string, dynamic>> _viewDataThunk;

        /// <summary>
        /// 数据实体
        /// </summary>
        public T Entity { get; set; }

        /// <summary>
        /// 扩展的动态数据
        /// </summary>
        public dynamic ViewBag
        {
            get
            {
                return this;
            }

        }
        public VM(T entity)
        {
            this.Entity = entity;
            this._viewDataThunk = () => new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
        }
        public VM(T entity, Func<Dictionary<string, dynamic>> viewDataThunk)
        {
            this.Entity = entity;
            this._viewDataThunk = viewDataThunk;
        }
        private Dictionary<string, dynamic> _datas = null;
        /// <summary>
        /// 数据体
        /// </summary>
        private Dictionary<string, dynamic> ViewData
        {
            get
            {
                if (_datas == null) _datas = _viewDataThunk();
                return _datas;
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return ViewData.Values.Select(x => (string)x);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.ViewData.ContainsKey(binder.Name))
            {
                result = ViewData[binder.Name];
            }
            else
            {
                result = null;
            }

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (this.ViewData.ContainsKey(binder.Name))
            {
                ViewData[binder.Name] = value;
            }
            else
            {
                ViewData.Add(binder.Name, value);
            }
            return true;
        }
    }
}