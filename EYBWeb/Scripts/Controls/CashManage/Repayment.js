
    $("#labuserid").val("0");
    $("#laballPrice").val("0");
    var isshowdoc = false; //美容师下拉框显示控制
    $("#gridtbody").delegate(".liMenu", "click", function () {

        var curent = $(this).parent().parent().parent().find(".liHide");
        $(".liHide1").slideUp("slow");
        $(".liHide").slideUp("slow");
        if (isshowdoc == false) {
            curent.slideDown("fast");
            //curent.css("display", "block");
            isshowdoc = true;
        } else {
            curent.slideUp("slow");
            isshowdoc = false;
        }
        // setTimeout(curent.mouseout(function () { curent.slideUp("slow"); }), 10000)

    });
    /////////////////////////////////////美容师 选择 //////////////////////////////////////
     //其他门店员工选择
$("#gridtbody").delegate(".selHospitalID", "change", function () {
    var hospitalid = $(this).val();
    $(this).parent().nextAll().remove();
    var parent = $(this).parent().parent();
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "text", //返回json格式的数据
        url: "/CashManage/GetAllUserByHospitalID",
        data: { "HospitalID": hospitalid },
        success: function (data) {
            //赋值下拉框
            parent.append(data);

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
    $("#gridtbody").delegate(".liMenu", "mouseover", function () {
        var curent = $(this).parent().parent().parent().find(".liHide");
        curent.css("display", "block");
    });
    $("#gridtbody").delegate(".liHide", "mouseleave", function () {
        $(this).fadeOut("slow");
    });
    $("#gridtbody").delegate(".labcode", "mouseover", function () {
        var curent = $(this).parent().find(".liHide");
        curent.css("display", "block");
    });
    //选择顾客后事件
    function InFoDate(obj) {
        var userid = obj.split("|")[0];
        $("#labuserid").val(userid);
        $.ajax({
            type: "post", //使用get方法访问后台
            dataType: "json", //返回json格式的数据
            url: getControllerUrl("CashManage", "GetAllBalance"),
            data: { "UserID": userid },
            async: false,
            success: function (data) {
                //赋值文本
                if (data != null) {
                    $("#labMemberNo").text(data.MemberNo);
                    $("#labPrice").text(data.Price);
                    $("#labTiems").text(data.Tiems);
                    $("#labUserName").text(data.UserName);
                    $("#labQinakuan").text(data.AllPrice);
                    //$("#labUserName").attr("onclick", "OpenPatientDetail(" + data.UserID + ",1)");
                    $("#labUserName").attr("href", "javascript:OpenPatientDetail(" + data.UserID + ",1);");
                    //$("#labUserName").attr("href", "/PatientManage/PatientDetail?type=1&UserID=" + data.UserID);
                    $("#labamout").text("0");
                }

                $("#gridtbody").empty();
                $("#txtmemo").val("");

                //获取欠款记录
                GetRepayment(userid);

                //加载储值卡总额
                GetStoredCardListSUM(userid);

            },
            error: function () {
                alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
            }
        });

    }
    //获取欠款记录
    function GetRepayment(userid) {
        $.ajax({
            type: "post", //使用get方法访问后台
            dataType: "text", //返回json格式的数据
            url: getControllerUrl("CashManage", "GetPaymentRecordList"),
            data: { "UserID": userid },
            success: function (data) {
                //获取欠款记录
                if (data != null) {
                    $("#gridtbody").append(data);
                }
            },
            error: function () {
                //alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
            }
        });
    }
    //合计金额改变事件
    $("#gridtbody").delegate('.cashprice',
        'keyup',
        function() {
            Caculatecash();
        });
    //计算总额变化
    function Caculatecash() {

        //还款总额变化
        var cashpricetr = $("#gridtbody").find(".cashpricetr");
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
        //获取欠款总额
        var qianprice = $("#gridtbody").find(".qianprice");
        var qiamoney = 0;
        qianprice.each(function (i) {

            qiamoney += parseFloat($(this).text());
        });
        $("#labamout").text(cashmoney);
        $("#labcardamout").text((qiamoney + cashmoney).toFixed(2));

    }

    //结账
    function CheckOrder() {
        var labuserid = $("#labuserid").val(); // 用户ID
        console.log("11");
        if (labuserid == "0") {
            artdialogfail("请选择客户！"); return false;
        }
        var value = $("input[name='checkboxorder']:checked").val();
        if (value == null) {
            artdialogfail("请选择要还款的订单！");
            return false;
        }
        if ($("#labamout").text() == "0") {
            artdialogfail("还款金额必须大于0！"); return;
        }
        var allamout = $("#labamout").text(); //还款总额
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
        var OrderNoList = new Array();//订单集合
        var HuankuanList = new Array(); //还款集合
        var UserIDList = new Array(); //参与员工集合
        var DocumentNumList = new Array(); //流水号集合
        checedbox.each(function (i) {
            OrderNoList[i] = $(this).val();
            DocumentNumList[i] = $(this).attr("code");
            HuankuanList[i] = $(this).parent().parent().find("input[name='txthuankuanp']").val();
            var newuseridlist = $(this).parent().parent().find("input[name='UserID']:checked");
            var canUserIDList = new Array(); //参与员工集合
            newuseridlist.each(function (j) {
                canUserIDList[j] = $(this).val();
            });
            UserIDList[i] = canUserIDList.toString() + "$";
        });
        var txtmemo = $("#txtmemo").val();
        var param = "?allamout=" + allamout + "&DocumentNumList=" + DocumentNumList.toString() + "&txtmemo=" + txtmemo + "&orderNo=" + OrderNoList.toString() + "&UserID=" + labuserid + "&HuankuanList=" + HuankuanList.toString() + "&UserIDList=" + UserIDList.toString() + "&chkcreatetime=" + chkcreatetime + "&txtCreateTime=" + txtCreateTime;
        art.dialog.open("/CashManage/PayOrderPageRepayment" + param, { title: "结账付款", lock: true, opacity: 0.05, width: 750, height: 450 });
    }

