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
        var str = "<tr name=" + $(this).attr("Name") + "><td>" + $(this).attr("Name") + "<input type='hidden' name='IsGive' value=" + $(this).attr("IsGive") + " /><input type='hidden' name='CardID' value=" + $(this).attr("code") + " /></td><td><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' class='CardMoney' name='txtChuzhika' value='0' /></td></tr>"
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
        var nowval = $(this).val(); //现金输入框的值
        if (nowval == "") { nowval = 0 }

        if (parseFloat(nowval) < parseFloat(cashamout)) {
            // alert("支付金额不能大于实际收款金额！");
            $(this).val("0");
        }
        nowval = $(this).val();
        if (nowval == "") { nowval = 0 }
        allprice += parseFloat(nowval);
        if (parseFloat(allprice) < parseFloat(cashamout)) {
            // alert("支付金额不能大于实际收款金额！");
            $(this).val("");
        }
    });

    if (parseFloat(allprice) > -parseFloat(cashamout)) {
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

    var allprice1 = 0;
    $(".CardMoney").each(function (i) {
        var nowval = $(this).val();
        if (nowval == "") { nowval = 0 }
        allprice1 += parseFloat(nowval);
    });
    if (parseFloat(allprice1) != -parseFloat(cashamout)) {
        alert("请全额支付！");return;
    }
    //获取支付方式金额

    var txtCash = $("#txtCash").val();
    var CardIDList=$("#CardIDList").val();
    alert(CardIDList);
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
    var RedirectUrl = "/CashManage/PayRechargeOrder"
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "json", //返回json格式的数据
        url: RedirectUrl,
        data: { "IDList": IDList, "Price": cashamout, "shouxufei": shouxufei, "Memo": txtmemo, "TimesList": TimesList, "HuankuanList": HuankuanList, "UserIDList": UserIDList, "UserID": UserID, "Xianjin": txtCash, "Yinlianka": txtyingliancard, "Chuzhika": txtChuzhikaValue, "txtChuzhikaarr": txtChuzhikaarr.toString(), "OtherPayment": OtherPayment.toString(), "IsCashValue": IsCashValue.toString(), "IsGiveValue": IsGiveValue.toString(), "OtherPaymentValue": OtherPaymentValue.toString(), "CardIDList": CardIDList.toString() },
        async: false,
        success: function (data) {
            if (data != null) {
                alert("支付成功！");
                closeed();
            }
        },
        error: function (res) {
            alert("支付失败！" + res.responseText);
        }
    });
    closeed();
    
}
//现金改变事件
$(".mainContentRight_bottomnewdepart").delegate('.CardMoney', 'keyup', function () {
    
    var allprice = 0;
    $(".CardMoney").each(function (i) {
        var nowval = $(this).val();
        if (nowval == "") { nowval = 0 }
        allprice += parseFloat(nowval);
    });
   
    if (parseFloat(allprice) > parseFloat(cashamout)) {    //cashamout现金总额(订单总额)
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