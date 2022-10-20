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
        var str = "<tr name=" + $(this).attr("Name") + "><td>" + $(this).attr("Name") + "<input type='hidden' name='IsGive' value=" + $(this).attr("IsGive") + " /><input type='hidden' name='CardID' value=" + $(this).attr("code") + " /></td><td><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' class='CardMoney' name='txtChuzhika' code=" + curentprice + " value='" + curentprice + "' /></td></tr>"
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

        var str = "<tr name=" + $(this).attr("Name") + "><td>" + $(this).attr("code") + "<input type='hidden' name='OtherPayment' value=" + $(this).val() + " /><input type='hidden' name='IsCash' value=" + $(this).attr("iscash") + " /></td><td><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' class='CardMoney' name='OtherPaymentValue'  value='0' /></td></tr>"
        $("#mainContentRight_bottomnewdepart").append(str);
        CalcMoney();

    } else {
        $("tr[name='" + $(this).attr("Name") + "']").remove();
        CalcMoney();
    }

});

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
    var labshengyuprice = $("#labshengyuprice").text();

    var allprice1 = 0;
    $(".CardMoney").each(function (i) {
        var nowval = $(this).val();
        if (nowval == "") { nowval = 0 }
        allprice1 += parseFloat(nowval);
    });
    if (parseFloat(allprice1) != parseFloat(cashamout)) {
        $("#savebtn").attr("disabled", false);
        alert("请全额支付！"); return;
    }
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
    art.dialog.open("/CashManage/UserVerify?UserID=" + UserID, { title: "请输入支付密码", width: 400, height: 200, close: function () {
        var bValue = art.dialog.data('paystatus'); // 读取B页面的数据
        var hiddenToken = art.dialog.data('hiddenToken'); // 读取B页面的数据
        $("#savebtn").attr("disabled", false);
        if (bValue > 0) {
            PayOrderInfo(OrderNo, DocumentNumList, cashamout, HuankuanList, UserIDList, txtmemo, labshengyuprice, UserID, txtCash, txtyingliancard, txtChuzhikaValue, txtChuzhikaarr, OtherPayment, IsGiveValue, IsCashValue, OtherPaymentValue, cardidarr, chkcreatetime, txtCreateTime, hiddenToken);
            art.dialog.data('paystatus', -1);
        }
    }
    });


}
//支付
var OrderNocache = "";
function PayOrderInfo(OrderNo, DocumentNumList, cashamout, HuankuanList, UserIDList, txtmemo, labshengyuprice, UserID, txtCash, txtyingliancard, txtChuzhikaValue, txtChuzhikaarr, OtherPayment, IsGiveValue, IsCashValue, OtherPaymentValue, cardidarr, chkcreatetime, txtCreateTime, hiddenToken) {
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

    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "json", //返回json格式的数据
        url: "/CashManage/RepaymentOrder",
        data: { "OrderNoList": OrderNo, "DocumentNumList": DocumentNumList, "Memo": txtmemo, "hiddenToken": hiddenToken, "Price": cashamout, "HuankuanList": HuankuanList, "UserIDList": UserIDList, "QianPrice": labshengyuprice, "UserID": UserID, "Xianjin": txtCash, "Yinlianka": txtyingliancard, "Chuzhika": txtChuzhikaValue, "txtChuzhikaarr": txtChuzhikaarr.toString(), "OtherPayment": OtherPayment.toString(), "IsGiveValue": IsGiveValue.toString(), "IsCashValue": IsCashValue.toString(), "OtherPaymentValue": OtherPaymentValue.toString(), "cardidarr": cardidarr.toString(), "chkcreatetime": chkcreatetime, "txtCreateTime": txtCreateTime },
        async: false,
        success: function (data) {
            if (data != null) {

                //打印
                if (isopenprint == "1") {
                    Preview1(data + "");
                }
                var win = art.dialog.open.origin; //来源页面
                // 如果父页面重载或者关闭其子对话框全部会关闭
                win.InFoDate(UserID + "|0");
                alert("支付成功！");
               closeedNorefresh();
            }
        },
        error: function (res) {
            alert("支付失败！" + res.responseText);
        }
    });


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
        $("#labshengyuprice").text(parseFloat(allprice1) - parseFloat(cashamout));
    } else {
        $("#labshengyuprice").text(parseFloat(allprice) - parseFloat(cashamout));
    }


});