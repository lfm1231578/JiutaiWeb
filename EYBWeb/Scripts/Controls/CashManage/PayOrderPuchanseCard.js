//只能输入浮点或整数
function CheckInputIntFloat(oInput) {
    if ('' != oInput.value.replace(/\d{1,}\.{0,1}\d{0,}/, '')) {
        oInput.value = oInput.value.match(/\d{1,}\.{0,1}\d{0,}/) == null ? '' : oInput.value.match(/\d{1,}\.{0,1}\d{0,}/);
    }
}
//选择储值卡事件
$("#tbodychekbox :checkbox").click(function () {
    if ($(this).prop("checked") == true) {
        var curentprice = $(this).val();
        //获取尚欠部分
        var shengyuprice = -parseFloat($("#labshengyuprice").text());
        if (shengyuprice < curentprice) {
            curentprice = shengyuprice;
        }
        if ($(this).attr("Name") == '积分消费') {
            if ($("#ssff").is(":checked")) {
                var count = ($("#ssff1").text())
                if (count == 1) {
                    var truthBeTold = window.confirm("个人积分不够，是否继续抵扣?");
                    if (truthBeTold) {
                        var str = "<tr name=" + $(this).attr("Name") + "><td>" + $(this).attr("Name") + "<input type='hidden' name='IsGive' value=" + $(this).attr("IsGive") + " /><input type='hidden' name='CardID' value=" + $(this).attr("code") + " /></td><td><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' class='CardMoney' name='txtChuzhika' code=" + curentprice + " value='" + curentprice + "' /></td></tr>"
                    }
                    else {
                        $("#ssff").removeAttr("checked");
                    }
                }
                else if (count == 2) {
                    $("#ssff").removeAttr("checked");
                    alert("个人积分不够抵扣,请分开开单");
                    return false;
                }
                else {
                    var str = "<tr name=" + $(this).attr("Name") + "><td>" + $(this).attr("Name") + "<input type='hidden' name='IsGive' value=" + $(this).attr("IsGive") + " /><input type='hidden' name='CardID' value=" + $(this).attr("code") + " /></td><td><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' class='CardMoney' name='txtChuzhika' code=" + curentprice + " value='" + curentprice + "' /></td></tr>"
                }
            }
        }
        else {
            var str = "<tr name=" + $(this).attr("Name") + "><td>" + $(this).attr("Name") + "<input type='hidden' name='IsGive' value=" + $(this).attr("IsGive") + " /><input type='hidden' name='CardID' value=" + $(this).attr("code") + " /></td><td><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' class='CardMoney' name='txtChuzhika' code=" + curentprice + " value='" + curentprice + "' /></td></tr>"
        }
        $("#mainContentRight_bottomnewdepart").append(str);
        CalcMoney();
    } else {
        $("tr[name='" + $(this).attr("Name") + "']").remove();
        CalcMoney();
    }
});
//其他支付方式
$("#otherPayment :checkbox").click(function () {
    if ($(this).prop("checked") == true) {

        var str = "";
        if ($(this).attr('id') == 'txtDeposit') {//股本消费
            var curentprice = parseFloat($(this).val());
            if (curentprice > 0) {
                //获取尚欠部分
                var shengyuprice = parseFloat($("#labshengyuprice").text());
                if (shengyuprice < 0) {
                    shengyuprice = -shengyuprice;
                }
                if (shengyuprice < curentprice) {
                    curentprice = shengyuprice;
                }
            }
            str = "<tr name=" + $(this).attr("Name") + "><td>" + $(this).attr("code") +
                "</td><td><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' class='CardMoney text-input' id='DepositPayment' name='CashPrice' code=" + $(this).val() + " value='" + curentprice + "' /></td></tr>";
        }
        else if ($(this).attr('code') == '优惠券') {//优惠券消费
            str = "<tr name=" + $(this).attr("Name") + "><td>" + $(this).attr("code")
                + "<input type='hidden' name='OtherPayment' value=" + $(this).val()
                + " /><input type='hidden' name='CouponNo' value='' /><input type='hidden' name='IsCash' value=" + $(this).attr("iscash")
                + " /></td><td><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' id='CouponPay' class='CardMoney text-input' name='OtherPaymentValue' value='0' /></td></tr>";

        } else {
            str = "<tr name=" + $(this).attr("Name") + "><td>" + $(this).attr("code") +
                "<input type='hidden' name='OtherPayment' value=" + $(this).val() + " /><input type='hidden' name='IsCash' value=" + $(this).attr("iscash") +
                " /></td><td><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' class='CardMoney text-input' name='OtherPaymentValue' value='0' /></td></tr>";
        }
        $("#mainContentRight_bottomnewdepart").append(str);
        CalcMoney();

    } else {
        $("tr[name='" + $(this).attr("Name") + "']").remove();
        CalcMoney();
    }

});
//计算优惠券
function CalcCoupon(obj) {
    var couponNo = obj.split('|')[1];
    var couponAmount = obj.split('|')[3];
    var couponLimit = obj.split('|')[4];
    if (parseFloat(cashamout) >= parseFloat(couponLimit)) {
        $('[id=CouponPay]').val(couponAmount);
        $('[id=CouponPay]').keyup();
        $('[name=CouponNo]').val(couponNo);
    }
    else {
        $('[id=CouponPay]').val('0');
        $('[id=CouponPay]').keyup();
        $('[name=CouponNo]').val('');
    }

    CalcMoney();
}

//计算现金
function CalcMoney() {
    var allprice = 0;
    $(".CardMoney").each(function (i) {
        var nowval = $(this).val();
        if (nowval == "") { nowval = 0 }
        if (parseFloat(nowval) > parseFloat(cashamout)) {
            // alert("支付金额不能大于实际收款金额！");
            $(this).val("0");
        }
        nowval = $(this).val();
        if (nowval == "") { nowval = 0 }
        allprice += parseFloat(nowval);
        if (parseFloat(allprice) > parseFloat(cashamout)) {
            // alert("支付金额不能大于实际收款金额！");
            $(this).val("");
        }
    });

    if (parseFloat(allprice) > parseFloat(cashamout)) {
        //alert("支付金额不能大于实际收款金额！"); $(this).val("");
        var allprice1 = 0;
        $(".CardMoney").each(function (i) {
            var nowval = $(this).val();
            if (nowval == "") { nowval = 0 }
            allprice1 += parseFloat(nowval);
        });
        $("#labshengyuprice").text(parseFloat(allprice1) - parseFloat(cashamout));
    } else {
        $("#labshengyuprice").text(parseFloat(allprice) - parseFloat(cashamout));
    }
}
//支付按钮
function PayMoney() {
    $("#savebtn").attr("disabled", "disabled");
    var userid = UserID;
    var jjff = $("#jjff").val();
    hospitalID = $("#hospitalID").val();
    //if (hospitalID == 1017) {
    //    if (jjff == 1) {
    //        var gnl = confirm("个人积分不够，是否继续抵扣?");
    //        if (gnl == true) {
    //            art.dialog.open("/CashManage/UserVerify?UserID=" + UserID, {
    //                title: "请输入支付密码", width: 400, height: 200, close: function () {
    //                    var bValue = art.dialog.data('paystatus'); // 读取B页面的数据 个人积分不够抵扣,请分开开单
    //                    var hiddenToken = art.dialog.data('hiddenToken'); // 读取B页面的数据
    //                    $("#savebtn").attr("disabled", false);
    //                    if (bValue > 0) {
    //                        PayMoneyVerify(hiddenToken);
    //                        art.dialog.data('paystatus', -1);
    //                    }
    //                }
    //            });
    //        }
    //    }
    //    else if (jjff==2)
    //    {
    //        alert("个人积分不够抵扣,请分开开单");
    //        return false;
    //    }
    //    else {
           
    //        art.dialog.open("/CashManage/UserVerify?UserID=" + UserID, {
    //            title: "请输入支付密码", width: 400, height: 200, close: function () {
    //                var bValue = art.dialog.data('paystatus'); // 读取B页面的数据
    //                var hiddenToken = art.dialog.data('hiddenToken'); // 读取B页面的数据
    //                $("#savebtn").attr("disabled", false);
    //                if (bValue > 0) {
    //                    PayMoneyVerify(hiddenToken);
    //                    art.dialog.data('paystatus', -1);
    //                }
    //            }
    //        });
    //    }
    //}
    //else {

        art.dialog.open("/CashManage/UserVerify?UserID=" + UserID, {
            title: "请输入支付密码", width: 400, height: 200, close: function () {
                var bValue = art.dialog.data('paystatus'); // 读取B页面的数据
                var hiddenToken = art.dialog.data('hiddenToken'); // 读取B页面的数据

             //   console.log(bValue, hiddenToken)

                $("#savebtn").attr("disabled", false);
                if (bValue > 0) {
                    PayMoneyVerify(hiddenToken);
                    art.dialog.data('paystatus', -1);
                }
            }
        });
   // }

}
var OrderNocache = "";
function PayMoneyVerify(hiddenToken) {
    if ((navigator.userAgent.indexOf('MSIE') >= 0) && (navigator.userAgent.indexOf('Opera') < 0)) {
        //IE内核不用做判断
    } else {
        //防重复提交
        if (OrderNocache == "") {
            OrderNocache = OrderNo;
        } else {
            if (OrderNocache == OrderNo) {
                return;
            } else {
                OrderNocache = "";
            }
        }
    }

    var labshengyuprice = $("#labshengyuprice").text();
    var allprice1 = 0;
    $(".CardMoney").each(function (i) {
        var nowval = $(this).val();
        if (nowval == "") { nowval = 0 }
        allprice1 += parseFloat(nowval);
    });
    if (parseFloat(allprice1) > parseFloat(cashamout)) {
        // alert("支付金额不能大于实际收款金额！");
        $(this).val("");
    }
    //获取支付方式金额
    // var txtTuangou = $("#txtTuangou").val();
    //var txtQuan = $("#txtQuan").val();
    var txtCash = $("#txtCash").val();
    var txtyingliancard = $("#txtyingliancard").val();
    var txtChuzhikaValue = 0;
    $("input[name='txtChuzhika']").each(function (i) {
        var nowval2 = $(this).val();
        if (nowval2 == "") { nowval2 = 0 }
        txtChuzhikaValue += parseFloat(nowval2);
    });
    //储值卡ID集合
    var cardidarr = new Array();
    $("input[name='CardID']").each(function (i) {
        cardidarr[i] = $(this).val();
    });
    //储值卡金额集合
    var txtChuzhikaarr = new Array();
    $("input[name='txtChuzhika']").each(function (i) {
        txtChuzhikaarr[i] = $(this).val();
    });
    //其他支付方式ID
    var OtherPayment = new Array();
    $("input[name='OtherPayment']").each(function (i) {
        OtherPayment[i] = $(this).val();
    });
    //其他支付方式ID
    var OtherPaymentValue = new Array();
    $("input[name='OtherPaymentValue']").each(function (i) {
        OtherPaymentValue[i] = $(this).val();
    });
    var IsCashValue = new Array();
    $("input[name='IsCash']").each(function (i) {
        IsCashValue[i] = $(this).val();
    });
    var IsGiveValue = new Array();
    $("input[name='IsGive']").each(function (i) {
        IsGiveValue[i] = $(this).val();
    });
    if (labshengyuprice < 0) {
        if (!confirm("当前存在欠款,确认支付?")) {
            return false;
        }
    }
    //优惠券
    var couponNo = $.trim($("input[name='CouponNo']").val());

    var CouponPay = 0; //优惠券金额
    if ($("#CouponPay").val() != undefined) {
        CouponPay = $("#CouponPay").val();
    }

    //股本作为现金  支付
    var DepositPayment = 0;

    if ($("#DepositPayment").val() != undefined) {
        DepositPayment = $("#DepositPayment").val();
    }
    let obj = { "OrderNo": OrderNo, "Statu": 1, "Price": cashamout, "QianPrice": labshengyuprice, "UserID": UserID, "Xianjin": txtCash, "Yinlianka": txtyingliancard, "Chuzhika": txtChuzhikaValue, "Huakouka": cardamout, "CardIDstr": CardIDstr, "ordertimesstr": ordertimesstr, "cardidarr": cardidarr.toString(), "txtChuzhikaarr": txtChuzhikaarr.toString(), "OtherPayment": OtherPayment.toString(), "couponNo": couponNo, "CouponPay": CouponPay, "depositValue": DepositPayment, "IsCashValue": IsCashValue.toString(), "IsGiveValue": IsGiveValue.toString(), "OtherPaymentValue": OtherPaymentValue.toString(), "ismaika": ismaika, "discountlist": discountlist, "projectlisr": projectlisr, "idlist": idlist, "hiddenToken": hiddenToken }
    //console.log(projectlisr)
    //console.log(discountlist)
    //console.table(obj, "------------------------->")
   
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "json", //返回json格式的数据
        url: "/CashManage/UpdateOrderPuchanseCard",
        data: { "OrderNo": OrderNo, "Statu": 1, "Price": cashamout, "QianPrice": labshengyuprice, "UserID": UserID, "Xianjin": txtCash, "Yinlianka": txtyingliancard, "Chuzhika": txtChuzhikaValue, "Huakouka": cardamout, "CardIDstr": CardIDstr, "ordertimesstr": ordertimesstr, "cardidarr": cardidarr.toString(), "txtChuzhikaarr": txtChuzhikaarr.toString(), "OtherPayment": OtherPayment.toString(), "couponNo": couponNo, "CouponPay": CouponPay, "depositValue": DepositPayment, "IsCashValue": IsCashValue.toString(), "IsGiveValue": IsGiveValue.toString(), "OtherPaymentValue": OtherPaymentValue.toString(), "ismaika": ismaika, "discountlist": discountlist, "projectlisr": projectlisr, "idlist": idlist, "hiddenToken": hiddenToken, "txtCreateTime": txtCreateTime },
        async: false,
        success: function (data) {
            if (data != null) {
                if (isopenprint == "1") {
                    Preview1();
                }
                var win = art.dialog.open.origin; //来源页面
                // 如果父页面重载或者关闭其子对话框全部会关闭
                win.InFoDate(UserID + "|0");
                alert("支付成功！");
                closeedNorefresh();
            }
        },
        error: function () {
            alert("支付失败！");
        }
    });
    // closeed();
    $("#savebtn").attr("disabled", "");

}





//现金改变事件
$(".mainContentRight_bottomnewdepart").delegate('.CardMoney', 'keyup', function () {
    //如果输入金额大于储值卡余额，清空文本框
    if (parseFloat($(this).val()) > parseFloat($(this).attr("code"))) {
        $(this).val("");
    }
    var allprice = 0;
    $(".CardMoney").each(function (i) {
        var nowval = $(this).val();
        if (nowval == "") { nowval = 0 }
        allprice += parseFloat(nowval);
    });
    if (parseFloat(allprice) > parseFloat(cashamout)) {
        //alert("支付金额不能大于实际收款金额！");
        $(this).val("");
        var allprice1 = 0;
        $(".CardMoney").each(function (i) {
            var nowval = $(this).val();
            if (nowval == "") { nowval = 0 }
            allprice1 += parseFloat(nowval);
        });
        $("#labshengyuprice").text((parseFloat(allprice1) - parseFloat(cashamout)).toFixed(2));
    } else {
        $("#labshengyuprice").text((parseFloat(allprice) - parseFloat(cashamout)).toFixed(2));
    }


});