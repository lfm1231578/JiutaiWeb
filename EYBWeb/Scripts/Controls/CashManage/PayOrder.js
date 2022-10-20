//只能输入浮点或整数
function CheckInputIntFloat(oInput) {
    if ('' != oInput.value.replace(/\d{1,}\.{0,1}\d{0,}/, '')) {
        oInput.value = oInput.value.match(/\d{1,}\.{0,1}\d{0,}/) == null ? '' : oInput.value.match(/\d{1,}\.{0,1}\d{0,}/);
    }
}
//选择储值卡事件
$("#tbodychekbox :checkbox").click(function () {
    //alert("测试123");
    if ($(this).prop("checked") == true) {
        var curentprice = parseFloat($(this).val());
        //获取尚欠部分
        var shengyuprice = parseFloat($("#labshengyuprice").text());
        if (shengyuprice < 0) {
            shengyuprice = -shengyuprice;
        }
        if (shengyuprice < curentprice) {
            curentprice = shengyuprice;
        }
        if ($(this).attr("Name") == '积分消费') {
            if ($("#ssff").is(":checked")) {
                var count = ($("#ssff1").text())
                if (count == 1) {
                    var truthBeTold = window.confirm("个人积分不够，是否继续抵扣?");
                    if (truthBeTold) {
                        //确定
                        var str = "<tr name=" + $(this).attr("Name") + "><td  class='ddd'>" + $(this).attr("Name") + "<input type='hidden' name='IsGive' value=" + $(this).attr("IsGive") + " /><input type='hidden' name='CardID' value=" + $(this).attr("code") + " /></td><td class='ccc'><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' class='CardMoney' name='txtChuzhika' code=" + curentprice + " value=" + curentprice + " /></td></tr>"
                    } else {
                        //取消
                        $("#ssff").removeAttr("checked");
                    }
                }
                else if (count == 2) {
                    $("#ssff").removeAttr("checked");
                    alert("个人积分不够抵扣,请分开开单");
                    return false;
                }
                else {
                    var str = "<tr name=" + $(this).attr("Name") + "><td  class='ddd'>" + $(this).attr("Name") + "<input type='hidden' name='IsGive' value=" + $(this).attr("IsGive") + " /><input type='hidden' name='CardID' value=" + $(this).attr("code") + " /></td><td class='ccc'><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' class='CardMoney' name='txtChuzhika' code=" + curentprice + " value=" + curentprice + " /></td></tr>"
                }

            }
        } else {
            var str = "<tr name=" + $(this).attr("Name") + "><td  class='ddd'>" + $(this).attr("Name") + "<input type='hidden' name='IsGive' value=" + $(this).attr("IsGive") + " /><input type='hidden' name='CardID' value=" + $(this).attr("code") + " /></td><td class='ccc'><input onkeyup='javascript:CheckInputIntFloat(this);' type='text' class='CardMoney' name='txtChuzhika' code=" + curentprice + " value=" + curentprice + " /></td></tr>"
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

    } else {
        $('[id=CouponPay]').val('0');
        $('[id=CouponPay]').keyup();
        $('[name=CouponNo]').val('');
    }
    CalcMoney();
}

//计算现金
function CalcMoney() {
    var allprice = 0;
    var ca = parseFloat(cashamout);
    $(".CardMoney").each(function (i) {
        var nowval = $(this).val();
        if (nowval == "") { nowval = 0 }
        if (parseFloat(nowval) > ca) {
            // alert("支付金额不能大于实际收款金额！");
            $(this).val("0");
        }
        nowval = $(this).val();
        if (nowval == "") { nowval = 0 }
        allprice += parseFloat(nowval);

        if (ca < 0) {
            ca = -ca;
        }
        if (parseFloat(allprice) > ca) {
            // alert("支付金额不能大于实际收款金额！");
            $(this).val("");
        }
    });
    if (parseFloat(allprice) > ca) {
        //alert("支付金额不能大于实际收款金额！"); $(this).val("");
        var allprice1 = 0;
        $(".CardMoney").each(function (i) {
            var nowval = $(this).val();
            if (nowval == "") { nowval = 0 }
            allprice1 += parseFloat(nowval);
        });
        $("#labshengyuprice").text((ca - parseFloat(allprice1)).toFixed(2));
    } else {
        $("#labshengyuprice").text((ca - parseFloat(allprice)).toFixed(2));
    }
}

//支付按钮
function PayMoney() {
    // $("#savebtn").attr("disabled", "disabled");
   // alert("请全额支付!");
   // debugger
    var labshengyuprice = $("#labshengyuprice").text();
    var allprice1 = 0;
    $(".CardMoney").each(function (i) {
        var nowval = $(this).val();
        if (nowval == "") { nowval = 0 }
        allprice1 += parseFloat(nowval);
    });
    var jifen="";
    var fen=0;
    $("#xftable tr").each(function (i) {
        
        if ($("#xftable tr").eq(i).attr("name") == "积分消费") {
            jifen = $("#xftable tr").eq(i).attr("name");
            fen = $("#xftable tr").eq(i).children("td").children(".CardMoney").val()
           // console.log($("#xftable tr").eq(i).children("td").children(".CardMoney").val())
        }
    })
    //$(".ddd").each(function (i) {
    //    if ($(".ddd").eq(i).text() == "积分消费") {
    //        jifen = $(".ddd").eq(i).text();
    //       // alert($("table tr").attr("name"))
    //       // alert($(".CardMoney").val())
    //        var fff = $(".ccc").eq(i).val();
    //        //$("ccc").each(function (i) {
    //        //    var nowval2fff = $(this).val();
    //        console.log("fsfs", fff)
    //        console.log("fsfs", $("table tr").attr("CardMoney"))
    //        //   // if (nowval2 == "") { nowval2 = 0 }
    //        //   // if (nowval2 == "") { nowval2 = 0 }
    //        //    //txtChuzhikaValue += parseFloat(nowval2);
    //        //});
    //    }
       
    //   // if (nowval == "") { nowval = 0 }
    //    //allprice1 += parseFloat(nowval);
    //});


    if (parseFloat(allprice1) > parseFloat(cashamout)) {
        // alert("支付金额不能大于实际收款金额！");
        $(this).val("");
    }
    //获取支付方式金额
    // var txtTuangou = $("#txtTuangou").val();
    //var txtQuan = $("#txtQuan").val();
    //股本作为现金  支付
    var depositPayment = 0;

    if ($("#DepositPayment").val() != undefined) {
        depositPayment = $("#DepositPayment").val();
    }
    var txtCash = parseFloat($("#txtCash").val()) + parseFloat(depositPayment);
    var txtyingliancard = $("#txtyingliancard").val();
    //股本支付
    //var depositValue = $.trim($("input[name='DepositPaymentValue']").val());


    //优惠券
    var couponNo = $.trim($("input[name='CouponNo']").val());
    var couponPay = 0; //优惠券金额
    if ($("#CouponPay").val() != undefined) {
        couponPay = $("#CouponPay").val();
    }
    var txtChuzhikaValue = 0;
    $("input[name='txtChuzhika']").each(function (i) {
        var nowval2 = $(this).val();
        if (nowval2 == "") { nowval2 = 0 }
        txtChuzhikaValue += parseFloat(nowval2);
    });
    var caridInfo = 0;
    //储值卡ID集合
    var cardidarr = new Array();
    $("input[name='CardID']").each(function (i) {
        cardidarr[i] = $(this).val();
        caridInfo = cardidarr[i];
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

    //退项目 、退产品
    if (parseFloat(allcashamout) < 0) {
        if (labshengyuprice != 0) {
            alert("请全额支付!");
            return false;
        }
    }
    if (labshengyuprice > 0) {
        if (!confirm("当前存在欠款,确认支付?")) {
            return false;
        }
    } else if (labshengyuprice < 0) {
        alert("请全额支付!");
        return false;
    }
    var jjff = $("#jjff").val();
    hospitalID = $("#hospitalID").val();
    //if (hospitalID == 1017) {
    //    if (jjff == 1) {
    //        var gnl = confirm("个人积分不够，是否继续抵扣?");
    //        if (gnl == true) {
    //            art.dialog.open("/CashManage/UserVerify?UserID=" + UserID, {
    //                title: "请输入支付密码", lock: true, opacity: 0.05, width: 400, height: 200, lock: true, close: function () {
    //                    var bValue = art.dialog.data('paystatus'); // 读取B页面的数据
    //                    var hiddenToken = art.dialog.data('hiddenToken'); // 读取B页面的数据

    //                    if (bValue > 0) {
    //                        PayOrderInfo(OrderNo, cashamout, labshengyuprice, UserID, txtCash, txtyingliancard, txtChuzhikaValue, cardamout, CardIDstr, ordertimesstr, cardidarr, txtChuzhikaarr, OtherPayment, IsCashValue, IsGiveValue, OtherPaymentValue, couponNo, couponPay, depositPayment, preOrderNOList, hiddenToken, chkcreatetime, txtCreateTime);
    //                        art.dialog.data('paystatus', "-1");
    //                    }
    //                }
    //            });
    //        }
    //    }
    //    else if (jjff==2) {
    //        alert("个人积分不够抵扣,请分开开单");
    //        return false;
    //    }
    //    else {
    //        art.dialog.open("/CashManage/UserVerify?UserID=" + UserID, {
    //            title: "请输入支付密码", lock: true, opacity: 0.05, width: 400, height: 200, lock: true, close: function () {
    //                var bValue = art.dialog.data('paystatus'); // 读取B页面的数据
    //                var hiddenToken = art.dialog.data('hiddenToken'); // 读取B页面的数据

    //                if (bValue > 0) {
    //                    PayOrderInfo(OrderNo, cashamout, labshengyuprice, UserID, txtCash, txtyingliancard, txtChuzhikaValue, cardamout, CardIDstr, ordertimesstr, cardidarr, txtChuzhikaarr, OtherPayment, IsCashValue, IsGiveValue, OtherPaymentValue, couponNo, couponPay, depositPayment, preOrderNOList, hiddenToken, chkcreatetime, txtCreateTime);
    //                    art.dialog.data('paystatus', "-1");
    //                }
    //            }
    //        });

    //    }
   // }
   // else {
        art.dialog.open("/CashManage/UserVerify?UserID=" + UserID, {
            title: "请输入支付密码", lock: true, opacity: 0.05, width: 400, height: 200, lock: true, close: function () {
                var bValue = art.dialog.data('paystatus'); // 读取B页面的数据
                var hiddenToken = art.dialog.data('hiddenToken'); // 读取B页面的数据

                if (bValue > 0) {
                    PayOrderInfo(OrderNo, cashamout, labshengyuprice, UserID, txtCash, txtyingliancard, txtChuzhikaValue, cardamout, CardIDstr, ordertimesstr, cardidarr, txtChuzhikaarr, OtherPayment, IsCashValue, IsGiveValue, OtherPaymentValue, couponNo, couponPay, depositPayment, preOrderNOList, hiddenToken, chkcreatetime, txtCreateTime);
                    art.dialog.data('paystatus', "-1");
                }
            }
        });
   // }



}
//支付
var OrderNocache = "";
function PayOrderInfo(OrderNo, cashamout, labshengyuprice, UserID, txtCash, txtyingliancard, txtChuzhikaValue, cardamout, CardIDstr, ordertimesstr, cardidarr, txtChuzhikaarr, OtherPayment, IsCashValue, IsGiveValue, OtherPaymentValue, couponNo, CouponPay, depositValue, preOrderNOList, hiddenToken, chkcreatetime, txtCreateTime) {
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
        url: "/CashManage/UpdateOrder",
        data: {
            "OrderNo": OrderNo, "Statu": 1, "Price": cashamout, "QianPrice": labshengyuprice, "UserID": UserID, "Xianjin": txtCash, "Yinlianka": txtyingliancard, "Chuzhika": txtChuzhikaValue, "Huakouka": cardamout, "CardIDstr": CardIDstr, "ordertimesstr": ordertimesstr, "cardidarr": cardidarr.toString(), "txtChuzhikaarr": txtChuzhikaarr.toString(), "OtherPayment": OtherPayment.toString(), "IsCashValue": IsCashValue.toString(), "IsGiveValue": IsGiveValue.toString(), "OtherPaymentValue": OtherPaymentValue.toString(), "preOrderNOList": preOrderNOList, "hiddenToken": hiddenToken, "chkcreatetime": chkcreatetime, "txtCreateTime": txtCreateTime,
            "couponNo": couponNo, "CouponPay": CouponPay, "depositValue": depositValue
        },
        async: false,
        success: function (data) {
            if (data == -99) {
                alert("网络延迟，请勿重复提交！"); return;
            }
            if (data == -2) {
                alert("没有仓库,请先设置仓库"); return;
                //Preview1();
            }
            if (data == -10) {
                alert("该优惠券已用完,请选择其它优惠券"); return;
            }
            if (data == -11) {
                alert("该优惠券已过期"); return;
            }
            if (data == -12) {
                alert("该优惠券无效"); return;
            }
            if (data == -20) {
                alert("非股东无法使用股东股本支付"); return;
            }
            if (data == -21) {
                alert("股东已停用,无法使用股东股本支付"); return;
            }
            if (data == -22) {
                alert("股东股本支付金额超出股东股本金额"); return;
            }

            if (data > 0) {
                // closeed();
                if (isopenprint == "1") {
                    Preview1();
                }


                alert("支付成功！");
               
                var win = art.dialog.open.origin; //来源页面
                // 如果父页面重载或者关闭其子对话框全部会关闭
                win.InFoDate(UserID + "|0");
                closeedNorefresh();
                //关闭打开的窗口
                //closeedNorefresh();

                // $("#btncancel").click();
              
                // art.dialog({ id: "payorderpage" }).hide();
                //$(".aui_close").click();
                //return false;
                //  location.reload(false);
            } else {
                alert("支付失败！");
            }
        },
        error: function () {
            alert("支付失败！");
        }
    });
}
//现金改变事件
$(".mainContentRight_bottomnewdepart").delegate('.CardMoney', 'keyup', function () {
    //如果输入金额大于储值卡余额，清空文本框

    if (parseFloat($("#labshishou").text()) > 0) {
        if (parseFloat($(this).val()) > parseFloat($(this).attr("code"))) {
            $(this).val("");
        }
    }
    if (parseFloat(cashamout) >= 0) { //购买操作
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
            $("#labshengyuprice").text((parseFloat(cashamout) - parseFloat(allprice1)).toFixed(2));

        } else {
            $("#labshengyuprice").text((parseFloat(cashamout) - parseFloat(allprice)).toFixed(2));

        }
    } else {  //退换操作
        var allprice = 0;
        $(".CardMoney").each(function (i) {
            var nowval = $(this).val();
            if (nowval == "") { nowval = 0 }
            allprice += parseFloat(nowval);
        });
        if (parseFloat(allprice) > -parseFloat(cashamout)) {
            //alert("支付金额不能大于实际收款金额！");
            $(this).val("");
            var allprice1 = 0;
            $(".CardMoney").each(function (i) {
                var nowval = $(this).val();
                if (nowval == "") { nowval = 0 }
                allprice1 += parseFloat(nowval);
            });
            $("#labshengyuprice").text((parseFloat(allprice1) + parseFloat(cashamout)).toFixed(2));
        } else {
            $("#labshengyuprice").text((parseFloat(allprice) + parseFloat(cashamout)).toFixed(2));
        }
    }


});


//支付按钮 ---房间入口
function PayMoneyOnBed() {
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
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "json", //返回json格式的数据
        url: "/CashManage/UpdateOrderOnBed",
        data: { "OrderNo": OrderNo, "Statu": 1, "Price": cashamout, "QianPrice": labshengyuprice, "UserID": UserID, "Xianjin": txtCash, "Yinlianka": txtyingliancard, "Chuzhika": txtChuzhikaValue, "Huakouka": cardamout, "CardIDstr": CardIDstr, "ordertimesstr": ordertimesstr, "cardidarr": cardidarr.toString(), "txtChuzhikaarr": txtChuzhikaarr.toString(), "OtherPayment": OtherPayment.toString(), "OtherPaymentValue": OtherPaymentValue.toString(), "OldOrderNo": OldOrderNo },
        async: false,
        success: function (data) {
            if (data != null) {
                alert("支付成功！");
                closeed();
            }
        },
        error: function () {
            alert("支付失败！");
        }
    });

}

