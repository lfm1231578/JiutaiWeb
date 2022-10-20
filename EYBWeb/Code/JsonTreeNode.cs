using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EYB.Web.Code
{
    /// <summary>
    /// 树结构实体
    /// </summary>
    public class JsonTreeNode
    {
        public string id { get; set; }
        public string text { get; set; }
        public string value { get; set; }
        public bool showcheck { get; set; }
        public bool isexpand { get; set; }
        public byte checkstate { get; set; }
        public bool hasChildren { get; set; }
        public List<JsonTreeNode> ChildNodes { get; set; }
        public bool complete { get; set; }
    }
}