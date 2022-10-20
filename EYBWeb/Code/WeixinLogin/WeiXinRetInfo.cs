using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EYB.Web.Code
{
    public class WeiXinRetInfo
    {
        public base_resp base_resp { get; set; }

        public string ret { get; set; }
        public string err_msg { get; set; }

        public string redirect_url { get; set; }
    }
}