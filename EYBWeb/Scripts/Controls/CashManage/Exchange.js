//Tab切换
$(function () {
    $("#tabs  li").click(function () {
        $("#tabs li").removeClass("thistab");
        $(this).addClass("thistab");
        $(".tab_con").hide();
        var index = $(this).parent().find("li").index(this);
        $("#tab_conbox").find(".tab_con").eq(index).show();
    });
    $("#tabs li:first").addClass("thistab").show();
});

//////////////////////////////////////////显示储值卡余额  开始///////////////////////////////////////////////////////////////////
$(".accountdiv").delegate("#labChuzhi", "mouseover", function () {
    $("#txtchuzhishow").css("display", "block");

});
$(".accountdiv").delegate("#txtchuzhishow", "mouseleave", function () {
    $("#txtchuzhishow").fadeOut("slow");
});
$("#txtchuzhishow").delegate("li", "click", function () {

    //获取项目和产品类型 改变折扣  -获取选中的套盒折扣
    var taohezhekou = $(this).find("label[class='taohezhekou']").text();

    if (taohezhekou == "0") {
        taohezhekou = "1";
    }
    //-获取选中的院用折扣
    var yuanyongzhekou = $(this).find("label[class='yuanyongzhekou']").text();
    if (yuanyongzhekou == "0") {
        yuanyongzhekou = "1";
    }

    var typelist = $("#gridtbody tr.cashtype");
    var nowzhekou;
    typelist.each(function (i) {
        var typename = $(this).find("label[name='typename']").text();

        if (typename == "项目") {
            var ISKit = $(this).find("input[name='ISKit']").val();

            if (ISKit == "2") { //是套盒
                nowzhekou = taohezhekou;
                $(this).find("input[name='zhekou']").val(taohezhekou);
            } else { //不是套盒
                nowzhekou = yuanyongzhekou;
                $(this).find("input[name='zhekou']").val(yuanyongzhekou);
            }
        }

        var productmoney = $(this).find(".productmoney").text();
        var times = $(this).find(".times").val();
        $(this).find(".alltimes").val((times * productmoney * nowzhekou).toFixed(2));
        Caculatecash();
    });
    $("#txtchuzhishow").fadeOut("slow");
});


//加载储值卡下拉
function LoadChuzhiSelection(userid) {
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetMainCardBalanceText"),
        data: { "UserID": userid },
        success: function (data) {
            $("#txtchuzhishow").html("");
            //赋值下拉框
            $("#txtchuzhishow").append(data);

        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });

}
//////////////////////////////////////////显示储值卡余额  结束///////////////////////////////////////////////////////////////////


/////////////////////////////////////美容师 选择  退换疗程卡 //////////////////////////////////////

//其他门店员工选择
$("#gridtbody").delegate(".selHospitalID", "change", function () {
    var hospitalid = $(this).val();
    $(this).parent().nextAll().remove();
    var parent = $(this).parent().parent();
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetAllUserByHospitalID"),
        data: { "HospitalID": hospitalid },
        success: function (data) {
            //赋值下拉框
            parent.append(data);
            // alert($("tr:gt(0)").find(".selHospitalID").length);

        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
});
// 其他店员工选择，第一行选择 其他跟着改变
$("#gridtbody").delegate('tr:first .selHospitalID', 'change', function () {

    var hospitalid = $(this).val();
    $(this).parent().nextAll().remove();
    var parent = $(this).parent().parent();
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetAllUserByHospitalID"),
        data: { "HospitalID": hospitalid },
        success: function (data) {
            $("#gridtbody tr:gt(0)").find(".selHospitalID").parent().nextAll().remove();
            $("#gridtbody tr:gt(0)").find(".selHospitalID").parent().parent().append(data);

        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
});



//其他门店员工选择
$("#gridtbody2").delegate(".selHospitalID", "change", function () {
    var hospitalid = $(this).val();
    $(this).parent().nextAll().remove();
    var parent = $(this).parent().parent();
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetAllUserByHospitalID"),
        data: { "HospitalID": hospitalid },
        success: function (data) {
            //赋值下拉框
            parent.append(data);
            // alert($("tr:gt(0)").find(".selHospitalID").length);

        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
});
// 其他店员工选择，第一行选择 其他跟着改变
$("#gridtbody2").delegate('tr:first .selHospitalID', 'change', function () {

    var hospitalid = $(this).val();
    $(this).parent().nextAll().remove();
    var parent = $(this).parent().parent();
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetAllUserByHospitalID"),
        data: { "HospitalID": hospitalid },
        success: function (data) {
            $("#gridtbody2 tr:gt(0)").find(".selHospitalID").parent().nextAll().remove();
            $("#gridtbody2 tr:gt(0)").find(".selHospitalID").parent().parent().append(data);

        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
});




$("#gridtbody").delegate('tr .liHide :checkbox', 'click', function () {
    var parentcode = $(this).parent().parent().parent();

    parentcode.find(".liMenu").css("display", "none");
    if ($(this).prop("checked") == true) {
        var codestr = "<span class='labcode' userid='" + $(this).val() + "'>" + $(this).attr("code") + "&nbsp;&nbsp;</span>";

        parentcode.append(codestr);
    } else {
        var nowparent = $(this).parent().parent().parent().parent();

        var dsd = nowparent.find(".labcode[userid=" + $(this).val() + "]");

        dsd.remove();
        if (nowparent.find(".labcode").length == 0) {
            parentcode.find(".liMenu").css("display", "block");
        }
    }
});

//$("#gridtbody").delegate(".liMenu", "click", function () {
//    var curent = $(this).parent().parent().parent().find(".liHide");
//    $(".liHide1").slideUp("fast");
//    $(".liHide").slideUp("fast");
//    if (isshowdoc == false) {
//        curent.slideDown("fast");
//        //curent.css("display", "block"); 
//        isshowdoc = true;
//    } else {
//        curent.slideUp("fast");
//        isshowdoc = false;
//    }
//});
//$("#gridtbody").delegate(".liHide", "mouseleave", function () {
//    $(this).fadeOut("slow");
//});


$("#gridtbody").delegate(".liHide", "mouseleave", function (event) {
    if (event.toElement == null)
        return;
    $(this).delay(1000).fadeOut("slow");
});

$("#gridtbody").delegate(".labcode", "mouseover", function () {
    var curent = $(this).parent().find(".liHide");
    curent.css("display", "block");
});
////////////////////////////////////////////////END///////////////////////////////////////////////////////


/////////////////////////////////////美容师 选择  退换储值卡 //////////////////////////////////////


$("#gridtbody2").delegate('tr .liHide2 :checkbox', 'click', function () {
    var parentcode = $(this).parent().parent().parent();
    parentcode.find(".liMenu2").css("display", "none");
    if ($(this).prop("checked") == true) {
        var codestr = "<span class='labcode2' userid='" + $(this).val() + "'>" + $(this).attr("code") + "&nbsp;&nbsp;</span>";

        parentcode.append(codestr);
    } else {
        var dsd = parentcode.find(".labcode2[userid=" + $(this).val() + "]");
        dsd.remove();
        if (parentcode.find(".labcode2").length == 0) {
            parentcode.find(".liMenu2").css("display", "block");
        }
    }
});
$("#gridtbody2").delegate(".liMenu2", "mouseover", function () {
    var curent = $(this).parent().parent().parent().find(".liHide2");
    curent.css("display", "block");
});
$("#gridtbody2").delegate(".liHide2", "mouseleave", function () {
    $(this).fadeOut("slow");
});
$("#gridtbody2").delegate(".labcode2", "mouseover", function () {
    var curent = $(this).parent().parent().parent().find(".liHide2");
    curent.css("display", "block");
});
////////////////////////////////////////////////END///////////////////////////////////////////////////////
//选择顾客后事件
function InFoDate(obj) {
    var userid = obj.split("|")[0];
    $("#labuserid").val(userid);
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "json", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetAllBalance"),
        data: { "UserID": userid },
        success: function (data) {
            //赋值文本
            if (data != null) {
                $("#labMemberNo").text(data.MemberNo);
                $("#labPrice").text(data.Price);
                $("#labTiems").text(data.Tiems);
                $("#labUserName").text(data.UserName);
                $("#labQinakuan").text(data.AllPrice);
                //  $("#labUserName").attr("onclick", "OpenPatientDetail(" + data.UserID + ",1)");
                $("#labUserName").attr("href", "javascript:OpenPatientDetail(" + data.UserID + ",1);");
                // $("#labUserName").attr("href", "/PatientManage/PatientDetail?type=1&UserID=" + data.UserID);
            }
            $("#txtmemo").val("");
            $("#gridtbody").empty();
            $("#gridtbody1").empty();
            $("#gridtbody2").empty();
            //获取疗程卡次数
            GetProjectTimes(userid);
            //获取购买的产品
            // GetProductListByUserID(userid);
            //获取储值卡余额记录
            GetCardListByUserID(userid);

            //加载储值卡总额
            GetStoredCardListSUM(userid);
            //加载积分
            GetIntegralSUM(userid);
            $("#labtuitimes").text("0")
            $("#labamout1").text("0")
            $(".labamout1").text("0")
            $("#txtshouxufei1").val("")
            $("#txtmemo1").val("")

            //加载储值下拉
            LoadChuzhiSelection(userid);
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}
$("#labuserid").val("0");
/***********************************************退换项目********************************************/

var isshowdoc = false; //美容师下拉框显示控制
$("#gridtbody").delegate(".liMenu", "click", function () {

    var curent = $(this).parent().parent().parent().find(".liHide");
    $(".liHide").slideUp("slow");
    if (isshowdoc == false) {
        curent.slideDown("fast");
        //curent.css("display", "block");
        isshowdoc = true;
    } else {
        curent.slideUp("slow");
        isshowdoc = false;
    }

});

//获取疗程卡次数
function GetProjectTimes(userid) {
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetProjectTimes"),
        data: { "UserID": userid },
        success: function (data) {
            //获取欠款记录
            if (data != null) {
                $("#gridtbody").append(data);
            }
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}
$("#gridtbody").delegate("input[name='checkboxorder']", "change", function () {
    CaculateTimes(false);
})

//合计次数改变事件
$("#gridtbody").delegate('.tuitiems', 'keyup', function () {
    CaculateTimes(true);
});
//实退金额编号
$("#gridtbody").delegate('.cashprice', 'keyup', function () {
    CaculateTimes();
});

//制保留2位小数，如：2，会在2后面补上00.即2.00    
//function toDecimal2(x) {
//    var f = parseFloat(x);
//    if (isNaN(f)) {
//        return false;
//    }
//    var f = Math.round(x * 100) / 100;
//    var s = f.toString();
//    var rs = s.indexOf('.');
//    if (rs < 0) {
//        rs = s.length;
//        s += '.';
//    }
//    while (s.length <= rs + 2) {
//        s += '0';
//    }
//    return s;
//}


//计算总次数
function CaculateTimes(obj) {
    //还款总额变化
    var cashpricetr = $("#gridtbody").find(".cashpricetr");
    var alltuitimes = 0;
    cashpricetr.each(function (i) {
        var tuitiems = $(this).find(".tuitiems").val();
        var cashprice = $(this).find(".cashprice").val();

        if (tuitiems == "") { tuitiems = 0; }
        if (cashprice == "") { cashprice = 0; }
        //var amount = Math.floor(tuitiems * parseFloat($(this).find(".youhuiprice").text()));//Math.round(tuitiems * parseFloat($(this).find(".youhuiprice").text())); // Math.floor() === 向下取整 现不需取整数
        var amount = tuitiems * parseFloat($(this).find(".youhuiprice").text());
        if (obj) {
            //$(this).find(".cashprice").val(parseFloat(tuitiems * parseFloat($(this).find(".youhuiprice").text())));
            $(this).find(".cashprice").val(amount);
        }
        $(this).find(".Tuikuanneedmoney").val(amount);
         
        if (parseFloat(tuitiems) > parseFloat($(this).find(".shengyuTimes").text()) || parseFloat(cashprice) > (parseFloat($(this).find(".Tuikuanneedmoney").val()) + 1)) {
          
            $(this).find(".tuitiems").val("0");
            $(this).find(".cashprice").val("0");
            $(this).find(".Tuikuanneedmoney").val("0");
        }
        var newtuitiems = $(this).find(".tuitiems").val();
        if (newtuitiems == "") { newtuitiems = 0; }
        if ($(this).find("input[name='checkboxorder']").attr("checked")) {
            alltuitimes += parseFloat(newtuitiems);
        }
    });

    $("#labtuitimes").text(alltuitimes);
    Caculatecash();
}
//合计金额改变事件
$("#gridtbody").delegate('.cashprice', 'keyup', function () {
    Caculatecash();
})
//计算总额变化
function Caculatecash() {
    //还款总额变化
    var cashpricetr = $("#gridtbody").find(".cashpricetr");
    var cashmoney = 0;
    var allyingfu = 0;
    cashpricetr.each(function (i) {
        var cash = $(this).find(".cashprice").val();
        if (cash == "") { cash = 0; }
        var nowtimes = $(this).find(".tuitiems").val(); //获取当前次数
        if (nowtimes == "" || nowtimes == "0") {
            cash = 0; //如果次数为空或者0,则不能添加实付金额
            $(this).find(".cashprice").val(0);
        }
        if (parseFloat(cash) > -parseFloat($(this).find(".qianprice").text())) {
            $(this).find(".cashprice").val("0");
        }

        var yf = $(this).find(".Tuikuanneedmoney").val();
        if (cash == "") { cash = 0; }
        if ($(this).find("input[name='checkboxorder']").attr("checked")) {
            cashmoney += parseFloat(cash);
            allyingfu += parseFloat(yf);
        }
    });
    $("#labamout").text(-cashmoney.toFixed(2));
    $("#yingtui").text(-allyingfu.toFixed(2));
    $("#txtshouxufei").val((allyingfu - cashmoney).toFixed(2));

}

//结账
function CheckOrder() {
    var labuserid = $("#labuserid").val(); // 用户ID
    if (labuserid == "0") {
        artdialogfail("请选择客户！"); return false;
    }
    var value = $("input[name='checkboxorder']:checked").val();
    if (value == null) {
        artdialogfail("请选择要退换的项目！");
        return false;
    }
    var allamout = $("#yingtui").text(); //退款总额
    var allshitui = $("#labamout").text();


    var ischeckedbox = true;
    //获取每行选中美容师顾问，组装数据
    $("#gridtbody input[name='checkboxorder']:checked").each(function (i) {
        var useridlist = $(this).parent().parent().find("input[name='UserID']:checked");
        if (useridlist.length == 0) { ischeckedbox = false; return false } else { ischeckedbox = true; }
    });
    if (ischeckedbox == false) {
        artdialogfail("请选择参与员工！"); return false;
    }
    // var orderNo = SaveOrderCheck();
    //if (orderNo == "") { return; }
    var checedbox = $("#gridtbody").find("input[name='checkboxorder']:checked");
    var IDList = new Array(); //用户余额记录ID集合
    var TimesList = new Array(); //退换次数集合
    var HuankuanList = new Array(); //实退款集合
    var HuankuanList1 = new Array(); //应退款集合
    var UserIDList = new Array(); //参与员工集合
    var CardIDList = new Array(); //项目ID集合
    var isnext = true;
    checedbox.each(function (i) {
        IDList[i] = $(this).val();
        CardIDList[i] = $(this).attr("code");
        HuankuanList[i] = $(this).parent().parent().find("input[name='txthuankuanp']").val();
        HuankuanList1[i] = $(this).parent().parent().find("input[class='Tuikuanneedmoney']").val();
        //        if (HuankuanList[i] == "" || HuankuanList[i] == "0") {
        //            isnext = false;
        //            return;
        //        } 

        if (HuankuanList[i] == "") {
            HuankuanList[i] = "0";
        }
        TimesList[i] = $(this).parent().parent().find("input[name='txttuitiems']").val();
        if (TimesList[i] == "" || TimesList[i] == "0") {
            isnext = false;
            return;
        }
        var newuseridlist = $(this).parent().parent().find("input[name='UserID']:checked");
        var canUserIDList = new Array(); //参与员工集合
        newuseridlist.each(function (j) {
            canUserIDList[j] = $(this).val();
        });
        UserIDList[i] = canUserIDList.toString() + "$";
    });
    if (!isnext) {
        artdialogfail("选中项退卡次数必须大于0！"); return;
    }
    var shouxufei = $("#txtshouxufei").val() || 0;
    var actureamout = parseFloat(allshitui);//  + parseFloat(shouxufei);
    var txtmemo = $("#txtmemo").val();
    var tuichuzhiType = $("#tuiliaochengType").val();
    var chkcreatetime = 0; //判断复选框是否选中
    var txtCreateTime = $("#txtCreateTime").val();
    if ($("#chkcreatetime").prop("checked")) {
        //汪-----------补单日期不能大于今天日期
        var timenow = new Date();
        var y = timenow.getFullYear() + '-';
        var m = (timenow.getMonth() + 1 < 10 ? '0' + (timenow.getMonth() + 1) : timenow.getMonth() + 1) + '-';
        var d = (timenow.getDate() < 10 ? '0' + timenow.getDate() : timenow.getDate());
        var time = y + m + d;
        var n = txtCreateTime.split("-");
        var q = time.split("-");
        if (n > q) {
            artdialogfail("补单日期不能在今天之后，请重填！");
            return;
        }
        //汪
        chkcreatetime = 1;
    }
    if (tuichuzhiType == "0") {
        artdialogfail("请选择退卡原因！");
        return false;
    }
    var param = "?ProductyType=1&allamout=" + allshitui + "&shouxufei=" + shouxufei + "&txtmemo=" + txtmemo + "&tuichuzhiType=" + tuichuzhiType + "&IDList=" + IDList.toString() + "&TimesList=" + TimesList.toString() + "&UserID=" + labuserid + "&HuankuanList=" + HuankuanList.toString() + "&HuankuanList1=" + HuankuanList1.toString() + "&UserIDList=" + UserIDList.toString() + "&CardIDList=" + CardIDList.toString() + "&chkcreatetime=" + chkcreatetime + "&txtCreateTime=" + txtCreateTime;
    art.dialog.open("/CashManage/PayOrderExchange" + param,
        { title: "结账付款", width: 750, height: 460, lock: true, opacity: 0.05 });
}

/***********************************************退换产品********************************************/

var isshowdoc1 = false; //美容师下拉框显示控制
$("#gridtbody1").delegate(".liMenu1", "click", function () {

    var curent = $(this).parent().parent().parent().find(".liHide1");
    $(".liHide1").slideUp("slow");
    if (isshowdoc1 == false) {
        curent.slideDown("fast");
        //curent.css("display", "block");
        isshowdoc1 = true;
    } else {
        curent.slideUp("slow");
        isshowdoc1 = false;
    }

});
//获取购买的产品
function GetProductListByUserID(userid) {
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetProductListByUserID"),
        data: { "UserID": userid },
        success: function (data) {
            //获取购买的产品
            if (data != null) {
                $("#gridtbody1").append(data);
            }
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}


//合计次数改变事件
$("#gridtbody1").delegate('.tuitiems', 'keyup', function () {
    CaculateTimes1();
});
//计算总次数
function CaculateTimes1() {

    //还款总额变化
    var cashpricetr = $("#gridtbody1").find(".cashpricetr");
    var alltuitimes = 0;
    cashpricetr.each(function (i) {
        var tuitiems = $(this).find(".tuitiems").val();
        if (tuitiems == "") { tuitiems = 0; }
        $(this).find(".cashprice").val(tuitiems * parseFloat($(this).find(".youhuiprice").text()));
        if (parseFloat(tuitiems) > parseFloat($(this).find(".shengyuTimes").text())) {
            $(this).find(".tuitiems").val("0");
            $(this).find(".cashprice").val("0");
        }
        var newtuitiems = $(this).find(".tuitiems").val();
        if (newtuitiems == "") { newtuitiems = 0; }
        alltuitimes += parseFloat(newtuitiems);
    });

    $("#labtuitimes1").text(alltuitimes);
    Caculatecash1();
}
//合计金额改变事件
$("#gridtbody1").delegate('.cashprice', 'keyup', function () {
    Caculatecash1();
})
//计算总额变化
function Caculatecash1() {

    //还款总额变化
    var cashpricetr = $("#gridtbody1").find(".cashpricetr");
    var cashmoney = 0;
    cashpricetr.each(function (i) {
        var nowprice = $(this).find(".cashprice").val();
        if (nowprice == "") { nowprice = 0; }
        if (parseFloat(nowprice) > -parseFloat($(this).find(".qianprice").text())) {
            $(this).find(".cashprice").val("0");
        }
        var cash = $(this).find(".cashprice").val();
        if (cash == "") { cash = 0; }
        cashmoney += parseFloat(cash);
    });

    $("#labamout1").text(-cashmoney);

}


//结账 -------产品
function PayOrderProduct() {
    var labuserid = $("#labuserid").val(); // 用户ID
    if (labuserid == "0") {
        artdialogfail("请选择客户！"); return false;
    }
    var value = $("input[name='checkboxorder1']:checked").val();
    if (value == null) {
        artdialogfail("请选择要退换的产品！");
        return false;
    }
    var allamout = $("#labamout1").text(); //退款总额

    var ischeckedbox = true;
    //获取每行选中美容师顾问，组装数据
    $("#gridtbody1 input[name='checkboxorder1']:checked").each(function (i) {
        var useridlist = $(this).parent().parent().find("input[name='UserID1']:checked");
        if (useridlist.length == 0) { ischeckedbox = false; return false } else { ischeckedbox = true; }
    });
    if (ischeckedbox == false) {
        artdialogfail("请选择参与员工！"); return false;
    }
    // var orderNo = SaveOrderCheck();
    //if (orderNo == "") { return; }
    var checedbox = $("#gridtbody1").find("input[name='checkboxorder1']:checked");
    var IDList = new Array(); //用户余额记录ID集合
    var TimesList = new Array(); //退换次数集合
    var HuankuanList = new Array(); //退款集合
    var UserIDList = new Array(); //参与员工集合
    var CardIDList = new Array(); //产品ID集合
    checedbox.each(function (i) {
        IDList[i] = $(this).val();
        CardIDList[i] = $(this).attr("code");
        HuankuanList[i] = $(this).parent().parent().find("input[name='txthuankuanp']").val();
        TimesList[i] = $(this).parent().parent().find("input[name='txttuitiems']").val();
        var newuseridlist = $(this).parent().parent().find("input[name='UserID1']:checked");
        var canUserIDList = new Array(); //参与员工集合
        newuseridlist.each(function (j) {
            canUserIDList[j] = $(this).val();
        });
        UserIDList[i] = canUserIDList.toString() + "$";
    });
    var shouxufei = $("#txtshouxufei1").val() || 0;
    var actureamout = parseFloat(allamout) + parseFloat(shouxufei);
    var txtmemo = $("#txtmemo1").val()
    var param = "?ProductyType=2&allamout=" + actureamout + "&shouxufei=" + shouxufei + "&txtmemo=" + txtmemo + "&IDList=" + IDList.toString() + "&TimesList=" + TimesList.toString() + "&UserID=" + labuserid + "&HuankuanList=" + HuankuanList.toString() + "&UserIDList=" + UserIDList.toString();
    art.dialog.open("/CashManage/PayOrderExchange" + param, { title: "结账付款", width: 750, height: 430, lock: true, opacity: 0.05 })
}


/***********************************************退换储值卡********************************************/

var isshowdoc2 = false; //美容师下拉框显示控制
$("#gridtbody2").delegate(".liMenu2", "click", function () {

    var curent = $(this).parent().parent().parent().find(".liHide2");
    $(".liHide2").slideUp("slow");
    if (isshowdoc2 == false) {
        curent.slideDown("fast");
        //curent.css("display", "block");
        isshowdoc2 = true;
    } else {
        curent.slideUp("slow");
        isshowdoc2 = false;
    }

});
//获取购买的储值卡
function GetCardListByUserID(userid) {
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetCardListByUserID"),
        data: { "UserID": userid },
        success: function (data) {
            //获取购买的产品
            if (data != null) {
                $("#gridtbody2").append(data);
            }
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}


//应退金额改变事件
$("#gridtbody2").delegate('.cashprice1', 'keyup', function () {
    //还款总额变化
    var nowprice = $(this).val();
    //    var cashpricetr = $("#gridtbody2").find(".cashpricetr");
    //    var cashmoney = 0;
    //    cashpricetr.each(function (i) {
    //        var nowprice = $(this).find(".cashprice1").val();
    //        if (nowprice == "") { nowprice = 0; }
    //        if (parseFloat(nowprice) > parseFloat($(this).find(".youhuiprice").text())) {
    //            $(this).find(".cashprice1").val("0");
    //        }
    //        $(this).parent().parent().find(".cashprice").val(nowprice);

    //    });
    var isgive = $(this).parent().parent().find(".isgive").val();
    if (isgive == "2") {
        $(this).parent().parent().find(".cashprice").val(nowprice);
    } else {
        $(this).parent().parent().find(".cashprice").val(0);
    }
    Caculatecash2();
})
//实退金额编号
$("#gridtbody2").delegate('.cashprice', 'keyup', function () {
    Caculatecash2();
})

$("#gridtbody2").delegate("input[name = 'checkboxorder2']", 'change', function () {
    Caculatecash2();
})

//计算总额变化
function Caculatecash2() {

    //还款总额变化
    var cashpricetr = $("#gridtbody2").find(".cashpricetr");
    var cashmoney = 0;
    var allyingfu = 0;
    cashpricetr.each(function (i) {

        //实退金额
        var cash = $(this).find(".cashprice").val();
        //应退金额
        var cashprice1 = $(this).find(".cashprice1").val();
        var isgive = $(this).find(".isgive").val();
        if (cash == "") { cash = 0; }
        if (cashprice1 == "") { cashprice1 = 0; }
        if (parseFloat(cash) > parseFloat($(this).find(".youhuiprice").text()) || parseFloat(cashprice1) > parseFloat($(this).find(".youhuiprice").text()) || parseFloat(cash) > parseFloat(cashprice1)) {
            $(this).find(".cashprice").val("0");
            $(this).find(".cashprice1").val("0");
            cash = 0;
            cashprice1 = 0;
        }

        if ($(this).find("input[name='checkboxorder2']").attr("checked")) {
            cashmoney += parseFloat(cash);
            allyingfu += parseFloat(cashprice1);
        }
    });


    $("#labamout2").text(-cashmoney.toFixed(2));
    $("#labyingtui").text(-allyingfu.toFixed(2));
    $("#txtshouxufei2").val((allyingfu - cashmoney).toFixed(2));



}


//结账 -------储值卡
function PayOrderCard() {
    var labuserid = $("#labuserid").val(); // 用户ID
    if (labuserid == "0") {
        artdialogfail("请选择客户！"); return false;
    }
    var value = $("input[name='checkboxorder2']:checked").val();
    if (value == null) {
        artdialogfail("请选择要退换的储值卡！");
        return false;
    }
    var allamout = $("#labamout2").text(); //退款总额

    //    if (allamout == "0") {
    //        artdialogfail("退款总额应大于0！");
    //        return false;
    //    }
    var ischeckedbox = true;
    //获取每行选中美容师顾问，组装数据
    $("#gridtbody2 input[name='checkboxorder2']:checked").each(function (i) {
        var useridlist = $(this).parent().parent().find("input[name='UserID']:checked");
        if (useridlist.length == 0) { ischeckedbox = false; return false } else { ischeckedbox = true; }
    });
    if (ischeckedbox == false) {
        artdialogfail("请选择参与员工！"); return false;
    }
    // var orderNo = SaveOrderCheck();
    //if (orderNo == "") { return; }
    var checedbox = $("#gridtbody2").find("input[name='checkboxorder2']:checked");
    var IDList = new Array(); //用户余额记录ID集合
    var TimesList = new Array(); //退换次数集合
    var HuankuanList1 = new Array(); //应退金额
    var HuankuanList = new Array(); //实退金额
    var UserIDList = new Array(); //参与员工集合
    var CardIDList = new Array(); //储值卡ID集合
    var ProjectIDList = new Array(); //项目名称（卡名称）ID集合
    var projectNameList = new Array(); //储值卡名称集合
    var istrue = true;
    checedbox.each(function (i) {
        var yingfu = $(this).parent().parent().find("input[name='txthuankuanp1']").val();
        if (yingfu == "0" || yingfu == "") {
            alert("应付金额应该大于0!");
            istrue = false;
        }
        IDList[i] = $(this).val();
        CardIDList[i] = $(this).attr("code");
        projectNameList[i] = $(this).attr("projectName");

        HuankuanList[i] = $(this).parent().parent().find("input[name='txthuankuanp']").val();
        HuankuanList1[i] = $(this).parent().parent().find("input[name='txthuankuanp1']").val();
        TimesList[i] = $(this).parent().parent().find("input[name='txttuitiems']").val();
        var newuseridlist = $(this).parent().parent().find("input[name='UserID']:checked");
        var canUserIDList = new Array(); //参与员工集合
        newuseridlist.each(function (j) {
            if ($(this))
                canUserIDList[j] = $(this).val();
        });
        UserIDList[i] = canUserIDList.toString() + "$";
    });
    if (!istrue) {
        return false;
    }

    var shouxufei = $("#txtshouxufei2").val() || 0;
    var actureamout = parseFloat(allamout) - parseFloat(shouxufei);
    var txtmemo = $("#txtmemo2").val();
    var tuichuzhiType = $("#tuichuzhiType").val(); //退卡原因
    var chkcreatetime = 0; //判断复选框是否选中
    var txtCreateTime = $("#txtCreateTime1").val();
    if ($("#chkcreatetime1").prop("checked")) {
        //汪-----------补单日期不能大于今天日期
        var timenow = new Date();
        var y = timenow.getFullYear() + '-';
        var m = (timenow.getMonth() + 1 < 10 ? '0' + (timenow.getMonth() + 1) : timenow.getMonth() + 1) + '-';
        var d = (timenow.getDate() < 10 ? '0' + timenow.getDate() : timenow.getDate());
        var time = y + m + d;
        var n = txtCreateTime.split("-");
        var q = time.split("-");
        if (n > q) {
            artdialogfail("补单日期不能在今天之后，请重填！");
            return;
        }
        //汪
        chkcreatetime = 1;
    }
    if (tuichuzhiType == "0") {
        artdialogfail("请选择退卡原因！");
        return false;
    }
    var param = "?ProductyType=3&allamout=" + parseFloat(allamout) + "&shouxufei=" + shouxufei + "&txtmemo=" + txtmemo + "&tuichuzhiType=" + tuichuzhiType + "&IDList=" + IDList.toString() + "&TimesList=" + TimesList.toString() + "&UserID=" + labuserid + "&HuankuanList=" + HuankuanList.toString() + "&HuankuanList1=" + HuankuanList1.toString() + "&UserIDList=" + UserIDList.toString() + "&CardIDList=" + CardIDList.toString() + "&projectNameList=" + projectNameList.toString() + "&chkcreatetime=" + chkcreatetime + "&txtCreateTime=" + txtCreateTime;
    art.dialog.open("/CashManage/PayOrderExchange" + param, { title: "结账付款", width: 750, height: 430, lock: true, opacity: 0.05 })
}



