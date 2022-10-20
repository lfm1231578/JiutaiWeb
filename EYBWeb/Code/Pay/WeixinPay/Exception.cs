using System;
using System.Collections.Generic;
using System.Web;

namespace EYB.Web.Code.Pay.WeixinPay
{
    public class WxPayException : Exception 
    {
        public WxPayException(string msg) : base(msg) 
        {

        }
     }
}