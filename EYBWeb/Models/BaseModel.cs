using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EYB.Web.Models
{
    public class BaseModel
    {
        public BaseModel()
        {
            
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string OrderField { get; set; }

        /// <summary>
        /// 排序方向
        /// </summary>
        public string OrderDirection { get; set; }
    }
}