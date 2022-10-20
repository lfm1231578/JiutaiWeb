using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EYB.Web.Code.Pay.WeixinPay;
using ThoughtWorks.QRCode.Codec;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Text;
using HS.SupportComponents;
using PersonnelManage.Factory.IBLL;
using EYB.ServiceEntity.PersonnelEntity;
namespace EYB.Web.Controllers
{
    /// <summary>
    /// 支付 及回调 （微信和支付宝）
    /// </summary>
    public class PayController : Controller
    {
        private IPersonnelManageBLL personnelManageBLL;
        public PayController()
        {

            personnelManageBLL = HS.SupportComponents.Base.XmlRegistration.Current.GetResovleContainer().Resolve<IPersonnelManageBLL>();
        }

        public ActionResult WeixinPay()
        {
            return View();

        }
        /// <summary>
        /// 微信支付界面
        /// </summary>
        /// <returns></returns>
        public ActionResult WeixinPayImg()
        {
            var Productid = Request["Productid"];

            var ParentHospitalID = Request["ParentHospitalID"];
            var addmonth = Request["addmonth"];
            var Price = Request["Price"];
            string result = GetPayUrl(Productid, ParentHospitalID, Price, addmonth);

            string data = result;
            if (!string.IsNullOrEmpty(data))
            {

                //初始化二维码生成工具
                QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
                qrCodeEncoder.QRCodeVersion = 0;
                qrCodeEncoder.QRCodeScale = 4;

                //将字符串生成二维码图片
                Bitmap image = qrCodeEncoder.Encode(data, Encoding.Default);

                //保存为PNG到内存流  
                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                //Response.ContentType = "image/Png";
                //Response.OutputStream.Write(ms.GetBuffer(), 0, (int)ms.Length);
                //Response.End();
                //输出二维码图片

                Response.BinaryWrite(ms.GetBuffer());
                Response.End();
            }
            return View();
        }
        /**
      * 生成直接支付url，支付url有效期为2小时,模式二
      * @param productId 商品ID
      * @return 模式二URL
      */
        public string GetPayUrl(string productId, string ParentHospitalID, string Price, string addmonth)
        {
            //var Price = Request["Price"];
            Log.Info(this.GetType().ToString(), "Native pay mode 2 url is producing...");

            WxPayData data = new WxPayData();
            data.SetValue("body", "服务器年费");//商品描述
            data.SetValue("attach", ParentHospitalID + "-" + Price + "-" + addmonth);//附加数据
            data.SetValue("out_trade_no", WxPayApi.GenerateOutTradeNo());//随机字符串
           data.SetValue("total_fee", ConvertValueHelper.ConvertIntValue(Price) * 100);//总金额 订单总金额，单位为分
           // data.SetValue("total_fee", 1);//总金额 订单总金额，单位为分
            // data.SetValue("sub_mch_id", 1413962102);//微信支付分配的子商户号
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));//交易起始时间
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));//交易结束时间
            data.SetValue("goods_tag", "fwf");//商品标记
            data.SetValue("trade_type", "NATIVE");//交易类型
            data.SetValue("product_id", productId);//商品ID

            WxPayData result = WxPayApi.UnifiedOrder(data);//调用统一下单接口
            string url = result.GetValue("code_url").ToString();//获得统一下单接口返回的二维码链接 

            Log.Info(this.GetType().ToString(), "Get native pay mode 2 url : " + url);
            return url;
        }
        /**
        * 生成扫描支付模式一URL
        * @param productId 商品ID
        * @return 模式一URL
        */
        public string GetPrePayUrl(string productId)
        {
            Log.Info(this.GetType().ToString(), "Native pay mode 1 url is producing...");

            WxPayData data = new WxPayData();
            data.SetValue("appid", WxPayConfig.APPID);//公众帐号id
            data.SetValue("mch_id", WxPayConfig.MCHID);//商户号
            data.SetValue("time_stamp", WxPayApi.GenerateTimeStamp());//时间戳
            data.SetValue("nonce_str", WxPayApi.GenerateNonceStr());//随机字符串
            data.SetValue("product_id", productId);//商品ID
            data.SetValue("sign", data.MakeSign());//签名
            string str = ToUrlParams(data.GetValues());//转换为URL串
            string url = "weixin://wxpay/bizpayurl?" + str;

            Log.Info(this.GetType().ToString(), "Get native pay mode 1 url : " + url);
            return url;
        }



        /**
        * 参数数组转换为url格式
        * @param map 参数名与参数值的映射表
        * @return URL字符串
        */
        private string ToUrlParams(SortedDictionary<string, object> map)
        {
            string buff = "";
            foreach (KeyValuePair<string, object> pair in map)
            {
                buff += pair.Key + "=" + pair.Value + "&";
            }
            buff = buff.Trim('&');
            return buff;
        }
         
        public ActionResult ProcessNotify()
        {

            WxPayData notifyData = Notify.GetNotifyData();

             //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                Log.Error(this.GetType().ToString(), "The Pay result is error : " + res.ToXml());
                ViewBag.Statu = -1;
                return Content(res.ToXml());
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();

            //查询订单，判断订单真实性
            if (!QueryOrder(transaction_id))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                Log.Error(this.GetType().ToString(), "Order query failure : " + res.ToXml());
                ViewBag.Statu = -1;
               // return View();
                return Content(res.ToXml());
            }
            //查询订单成功
            else
            {
               
               //Log.Info(this.GetType().ToString(), "UnifiedOrder success , send data to WeChat : " + notifyData.ToXml());
                //回调文本res.ToXml()  弹出提示   //更改门店续费时长
                string attach = notifyData.GetValue("attach").ToString();
         
                    int result = 0;
                if (!String.IsNullOrEmpty(attach))
                {

                    string[] res = attach.Split('-');
                    var ParentHospitalID = ConvertValueHelper.ConvertIntValue(res[0]);
                    var Price = ConvertValueHelper.ConvertDecimalValue(res[1]);
                    var addmonth = ConvertValueHelper.ConvertIntValue(res[2]) == 0 ? 12 : ConvertValueHelper.ConvertIntValue(res[2]);//默认续费一年
                    RechargeEntity charge = personnelManageBLL.GetRechargeModelByHospitalID(ParentHospitalID);//获取到期时间
                    var ExpireTime = charge.ExpireTime.AddMonths(addmonth);

                    LogWriter.WriteInfo(charge.ExpireTime + "测试前后" + ExpireTime + "", "回调时间测试");
                    result = personnelManageBLL.AddUserRechargeOperate(new RechargeEntity { ParentHospitalID = ParentHospitalID, AllPrice = Price, ExpireTime = ExpireTime });
                }
                WxPayData res11 = new WxPayData();
                res11.SetValue("return_code", "SUCCESS");
                res11.SetValue("return_msg", "OK");
                //调用成功一定要做出反馈，否则微信会重复调用
                return Content(res11.ToXml());
             
            }
           // return View();
        }
        //查询订单
        private bool QueryOrder(string transaction_id)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WxPayData res = WxPayApi.OrderQuery(req);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private WxPayData UnifiedOrder(string openId, string productId)
        {
            //统一下单
            WxPayData req = new WxPayData();
            req.SetValue("body", "test");
            req.SetValue("attach", "test");
            req.SetValue("out_trade_no", WxPayApi.GenerateOutTradeNo());
            req.SetValue("total_fee", 1);
            req.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            req.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
            req.SetValue("goods_tag", "test");
            req.SetValue("trade_type", "NATIVE");
            req.SetValue("openid", openId);
            req.SetValue("product_id", productId);
            WxPayData result = WxPayApi.UnifiedOrder(req);
            return result;
        }
    }
}
