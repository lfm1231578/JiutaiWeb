var isshow = false; //更多选项显示
var isshowdoc = false; //美容师下拉框显示控制
$(function () {
    //  $("#labuserid").val("0");
    // $("#laballPrice").val("0");
    // $("#labuserid").val("0");
    $("#laballPrice").val("0");
    $("#btnmore").toggle(function () {
        $(".isshowth").css("display", "");
        isshow = true;
    },
        function () {
            $(".isshowth").css("display", "none");
            isshow = false;
        });

    $("#gridtbody").delegate(".liMenu", "click", function () {

        var curent = $(this).parent().parent().parent().find(".liHide");
        $(".liHide1").slideUp("fast");
        $(".liHide").slideUp("fast");
        if (isshowdoc == false) {
            curent.slideDown("fast");
            //curent.css("display", "block");
            isshowdoc = true;
        } else {
            curent.slideUp("fast");
            isshowdoc = false;
        }
        // setTimeout(curent.mouseout(function () { curent.slideUp("slow"); }), 10000)
    });
    //是否指定选择第一行，下面的同步更新
    $("#gridtbody").delegate('tr:first .IsZhiding',
        'change',
        function () {
            var cureentval = $(this).val();
            $(".IsZhiding option[value='" + cureentval + "']").attr("selected", "selected");
        });
    /////////////////////////////////////美容师 选择 //////////////////////////////////////
    $("#gridtbody").delegate(".liMenu1",
        "click",
        function () {
            var curent = $(this).parent().parent().parent().find(".liHide1");
            $(".liHide1").slideUp("slow");
            $(".liHide").slideUp("slow");
            if (isshowdoc == false) {
                curent.slideDown("fast");
                isshowdoc = true;
            } else {
                curent.slideUp("slow");
                isshowdoc = false;
            }
        });
    var countj = 0;
    //第一次选中第一个，下面的都选中，第二次选中第一个，下面的无操作
    $("#gridtbody").delegate('tr:first .liMenu',
        'click',
        function () {
            countj += 1;
        });
    //第一次选中第一个，下面的都选中，第二次选中第一个，下面的无操作
    $("#gridtbody").delegate('tr:first .liHide :checkbox',
        'click',
        function () {
            $("tr").find(".liMenu").css("display", "none");

            if ($(this).prop("checked") == true) {
                $("#gridtbody  .liHide  input:checkbox[value='" + $(this).val() + "']").attr("checked", "checked");
                var $tr = $("#gridtbody tr:gt(0)").find(".selectUL");
                var codestr = "<span class='labcode' userid='" + $(this).val() + "'>" + $(this).attr("code") + "&nbsp;&nbsp;</span>";
                //$tr.after(codestr);
                $tr.before(codestr)
       
            } else {
                $("#gridtbody  .liHide  input:checkbox[value='" + $(this).val() + "']").attr("checked", false);

                var parentcode = $("#gridtbody tr:gt(0)");
                var dsd = parentcode.find(".labcode[userid=" + $(this).val() + "]");
                dsd.remove();
                if (parentcode.find(".labcode").length == 0) {
                    parentcode.find(".liMenu").css("display", "block");
                }
            }
        });
    //edit by kuangsg  2015-12-28
    $("#gridtbody").delegate('tr .liHide :checkbox',
        'click',
        function () {
            var parentcode = $(this).parent().parent().parent();

            parentcode.find(".liMenu").css("display", "none");
            if ($(this).prop("checked") == true) {
             
                var codestr = "<span class='labcode' userid='" + $(this).val() + "'>" + $(this).attr("code") + "&nbsp;&nbsp;</span>";
                 //parentcode.after(codestr)
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
    $("#gridtbody").delegate(".liMenu",
        "click",
        function () {
            var curent = $(this).parent().parent().parent().find(".liHide");
            // curent.css("display", "block");
            curent.fadeIn("slow");
        });
    $("#gridtbody").delegate(".liHide",
        "mouseleave",
        function (event) {
            if (event.toElement == null)
                return;
            $(this).delay(1000).fadeOut("slow");
        });
    //edit by kuangsg  2015-12-28
    $("#gridtbody").delegate(".labcode",
        "click",
        function () {
            //var curent = $(this).parent().parent().parent().find(".liHide");
            var curent = $(this).parent().find(".liHide");
            //curent.css("display", "block");
            curent.fadeIn("slow");
        });
    /////////////////////////////////////顾问选择 选择 //////////////////////////////////////
    //第一次选中第一个，下面的都选中，第二次选中第一个，下面的无操作
    $("#gridtbody").delegate('tr:first .liHide1 :checkbox',
        'click',
        function () {
            //        if (countj <= 1) {
            //            if ($(this).prop("checked") == true) {
            //                $("#gridtbody  .liHide1  input:checkbox[value='" + $(this).val() + "']").attr("checked", "checked");
            //            } else {
            //                $("#gridtbody  .liHide1  input:checkbox[value='" + $(this).val() + "']").attr("checked", false);
            //            }
            //        }
            $("tr").find(".liMenu1").css("display", "none");
            
            if ($(this).prop("checked") == true) {
                $("#gridtbody  .liHide1  input:checkbox[value='" + $(this).val() + "']").attr("checked", "checked");
                var $tr = $("#gridtbody tr:gt(0)").find(".selectUL1");
                var codestr = "<span class='labcode1' userid='" +
                    $(this).val() +
                    "'>" +
                    $(this).attr("code") +
                    "&nbsp;&nbsp;</span>";

                $tr.append(codestr);
            } else {
                $("#gridtbody  .liHide1  input:checkbox[value='" + $(this).val() + "']").attr("checked", false);

                var parentcode = $("#gridtbody tr:gt(0)").find(".selectUL1");
                var dsd = parentcode.find(".labcode1[userid=" + $(this).val() + "]");
                dsd.remove();
                if (parentcode.find(".labcode1").length == 0) {
                    parentcode.find(".liMenu1").css("display", "block");
                }
            }
        });
    $("#gridtbody").delegate('tr .liHide1 :checkbox',
        'click',
        function () {
            var parentcode = $(this).parent().parent().parent();
            parentcode.find(".liMenu1").css("display", "none");
            if ($(this).prop("checked") == true) {
                var codestr = "<span class='labcode1' userid='" +
                    $(this).val() +
                    "'>" +
                    $(this).attr("code") +
                    "&nbsp;&nbsp;</span>";
                parentcode.append(codestr);
            } else {
                var dsd = parentcode.find(".labcode1[userid=" + $(this).val() + "]");
                dsd.remove();
                if (parentcode.find(".labcode1").length == 0) {
                    parentcode.find(".liMenu1").css("display", "block");
                }
            }
        });
    $("#gridtbody").delegate(".liMenu1",
        "mouseover",
        function () {
            var curent = $(this).parent().parent().parent().find(".liHide1");
            curent.css("display", "block");
        });
    $("#gridtbody").delegate(".liHide1",
        "mouseleave",
        function () {
            $(this).fadeOut("slow");
        });
    $("#gridtbody").delegate(".labcode1",
        "mouseover",
        function () {
            var curent = $(this).parent().parent().parent().find(".liHide1");
            // var curent = $(this).parent().find(".liHide1");
            curent.css("display", "block");
        });
    $(".accountdiv").delegate("#labChuzhi",
        "mouseover",
        function () {
            $("#txtchuzhishow").css("display", "block");

        });
    $(".accountdiv").delegate("#txtchuzhishow",
        "mouseleave",
        function () {
            $("#txtchuzhishow").fadeOut("slow");
        });
    $("#txtchuzhishow").delegate("li",
        "click",
        function () {

            //获取项目和产品类型 改变折扣
            var taohezhekou = $(this).find("label[class='taohezhekou']").text();

            if (taohezhekou == "0") {
                taohezhekou = "1";
            }
            var yuanyongzhekou = $(this).find("label[class='yuanyongzhekou']").text();
            if (yuanyongzhekou == "0") {
                yuanyongzhekou = "1";
            }
            var jiajuzhekou = $(this).find("label[class='jiajuzhekou']").text();
            if (jiajuzhekou == "0") {
                jiajuzhekou = "1";
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
                } else if (typename == "产品") {
                    nowzhekou = jiajuzhekou;
                    $(this).find("input[name='zhekou']").val(jiajuzhekou);

                }

                var productmoney = $(this).find(".productmoney").text();
                var times = $(this).find(".times").val();
                $(this).find(".alltimes").val((times * productmoney * nowzhekou).toFixed(2));
                Caculatecash();
            });
            $("#txtchuzhishow").fadeOut("slow");
        });

    /**************************************************卡项选择*******************************************/
    $("#selectOtherProject").delegate('.liHide :checkbox',
        'click',
        function () {
            var parentcode = $(this).parent().parent().parent();
            //parentcode.find(".liMenu").css("display", "none");
            var cardid = $(this).val();
            var alltimes = $(this).attr("alltimes"); //剩余次数
            if ($(this).prop("checked") == true) {
                //            var codestr = "<span class='labcode' userid='" + $(this).val() + "'>" + $(this).attr("code") + "&nbsp;&nbsp;</span>";
                //            parentcode.append(codestr);
                SelectProjectFun(cardid, alltimes);
            } else {
                //            var dsd = parentcode.find(".labcode[userid=" + $(this).val() + "]");
                //            dsd.remove();
                //            if (parentcode.find(".labcode").length == 0) {
                //                parentcode.find(".liMenu").css("display", "block");
                //            }
                $(".deletebtn[code='" + cardid + "']").click();
            }
        });
    //$("#selectOtherProject").delegate('.liClass', 'click', function () {
    //    var cardid = $(this).find("input[name='ProjectID']").val();
    //    var alltimes = $(this).find("input[name='ProjectID']").attr("alltimes"); //剩余次数
    //    $(this).find("input[name='ProjectID']").prop("checked")=true;
    //    if ($(this).find("input[name='ProjectID']").prop("checked") == true) {
    //        SelectProjectFun(cardid, alltimes);
    //    } else {
    //        $(".deletebtn[code='" + cardid + "']").click();
    //    }
    //});
    $("#selectOtherProject").delegate(".liMenu",
        "mouseover",
        function () {
            var curent = $(this).parent().find(".liHide");
            curent.css("display", "block");
        });
    $("#selectOtherProject").delegate(".liHide",
        "mouseleave",
        function () {
            $(this).fadeOut("slow");
        });
    $("#selectOtherProject").delegate(".labcode",
        "mouseover",
        function () {
            var curent = $(this).parent().parent().parent().find(".liHide");
            curent.css("display", "block");
        });
});
//计算总额变化

function Caculatecash() {
    //卡扣总额变化
    var currentclass = $("#gridtbody").find(".cardtype");
    var cardmoney = 0;
    currentclass.each(function (i) {
        cardmoney += parseFloat($(this).find(".alltimes").val());
    });
    $("#labcardamout").text(cardmoney);
    //现金总额变化
    var currentcash = $("#gridtbody").find(".cashtype");
    var cashmoney = 0;
    currentcash.each(function (i) {
        cashmoney += parseFloat($(this).find(".alltimes").val());
    });
    $("#labcash").text(cashmoney.toFixed(2));

    $("#labamout").text((cashmoney + cardmoney).toFixed(2));
    $("#laballPrice").val((cashmoney + cardmoney).toFixed(2));


}
$(function () {
    //删除功能
    $("#gridtbody").delegate('.deletebtn',
        'click',
        function () {
            $(this).parent().parent().remove();
            if ($(this).attr("code") != undefined) {
                $("#selectUL").find(":checkbox[value='" + $(this).attr("code") + "']").attr("checked", false);
            }
            Caculatecash();
        });
    //次数改变事件
    $("#gridtbody").delegate('.times', 'keyup', function () {

        var times = $(this).val();
        times = times.dbc2sbc();
        $(this).val(times);

        if (times == "-") {
            times = 0;
        }

        var maxnumber = $(this).attr("code");
        if (parseFloat(times) > parseFloat(maxnumber)) {
            alert("划扣次数不能比剩余总数多!");
            times = maxnumber;
            $(this).val(maxnumber);
        }

        var nowparent = $(this).parent().parent();
        var zhekou = nowparent.find(".zhekou").val();
        var productmoney = nowparent.find(".productmoney").text();
        nowparent.find(".alltimes").val((times * productmoney * zhekou).toFixed(2));
        Caculatecash();
    });
    //折扣事件改变
    $("#gridtbody").delegate('.zhekou', 'keyup', function () {
        var zhekou = $(this).val();
        var nowparent = $(this).parent().parent();
        var productmoney = nowparent.find(".productmoney").text();
        var times = nowparent.find(".times").val();
        nowparent.find(".alltimes").val((times * productmoney * zhekou).toFixed(2));
        Caculatecash();
    });
    $("#gridtbody").delegate('.zhekou', 'change', function () {
        var zhekou = $(this).val();
        var nowparent = $(this).parent().parent();
        var productmoney = nowparent.find(".productmoney").text();
        var times = nowparent.find(".times").val();
        nowparent.find(".alltimes").val((times * productmoney * zhekou).toFixed(2));
        Caculatecash();
    });
    //合计金额改变事件
    $("#gridtbody").delegate('.alltimes', 'keyup', function () {
        Caculatecash();
    });
});

//选择项目
function ProjetList() {
    var userid = $("#labuserid").val();
    if (userid == "0") {
        artdialogfail("请选择客户！"); return "";

    }
    art.dialog.open("/BaseInfo/ProjectList",
        {
            title: "项目列表",
            width: 660,
            height: 470,
            close: function () {
                var bValue = art.dialog.data('bValue'); // 读取B页面的数据
                if (bValue !== undefined) {
                    Selectsection(bValue);
                }
            }
        });
}
//选择产品
function ProductList() {
    var userid = $("#labuserid").val();
    if (userid == "0") {
        artdialogfail("请选择客户！"); return "";
        return;
    }
    art.dialog.open("/BaseInfo/ProductList?isHome=1", {
        title: "产品列表", width: 660, height: 470, close: function () {
            var bValue = art.dialog.data('bValueProduct'); // 读取B页面的数据
            if (bValue !== undefined) {
                Selectproduct(bValue);
            }
        }
    });
}

//提单
function GetOperateOrder() {
    var userid = $("#labuserid").val();
    art.dialog.open("/CashManage/PreOrderList?UserID=" + userid, { title: "待结订单列表", width: 660, height: 470 })
}
//结账
function CheckOrder() {
    var userid = $("#labuserid").val();
    console.log("开单结账1");
    if (userid == "0") {
        artdialogfail("请选择客户！");
        return;
    }

    var cashamout = $("#labcash").text(); //现金总额
    var cardamout = $("#labcardamout").text(); //卡扣总额
    var allamout = $("#labamout").text(); //所有总额
    var labuserid = $("#labuserid").val(); //所有总额
    var txtmemo = $("#txtmemo").val(); //备注

    var chkcreatetime = 0;//判断复选框是否选中

    var txtCreateTime = $("#txtCreateTime").val();
    // console.log("补单时间<<", txtCreateTime);

    if (txtCreateTime == undefined) {
       // alert("123");
        var date = new Date();
        txtCreateTime = date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate() + ' ' + date.getHours() + ':' + date.getMinutes() + ':' + date.getSeconds();

    } 
    if (txtCreateTime != "") {
        //alert("456");
        if ($("#chkcreatetime").prop("checked") == false) {
            artdialogfail("补单日期不为空，请在前面打勾！");
            return;
        }
    }
    if ($("#chkcreatetime").prop("checked") == true) {
        if (txtCreateTime == "") {
            artdialogfail("前面已打勾，请填写补单日期！");
            return;
        }
    }

   // console.log("补单时间8888<<", txtCreateTime);
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
    //var results = SaveOrderCheck();
    //var orderNo = "";
    //var reg = RegExp(/|/);
    //if (str.match(reg)) {

    //    orderNo = results.split("|")[0];
    //    var filter = results.split("|")[1];
    //    if (filter != "") {
    //        filter = filter + "库存不足,请确认是否继续结账？"
    //        if (!confirm(filter)) {
    //            return false;
    //        }
    //    }
    //} else {
    //    orderNo = results;
    //}
    var results = SaveOrderCheck();
    var orderNo = results.split("|")[0];
    var filter = results.split("|")[1];
    if (filter == "库存不足") {
        if (!confirm("库存不足,请确认是否继续结账？")) {
            return false;
        }
    }

    if (orderNo == "") { return; }
    var CardID = $("#gridtbody .cardtype").find("input[name='CardID']");
    var CardIDstr = new Array();
    CardID.each(function (i) {
        CardIDstr[i] = $(this).val();
    });
    var ordertimes = $("#gridtbody .cardtype").find("input[name='ordertimes']");
    var ordertimesstr = new Array();
    ordertimes.each(function (i) {
        ordertimesstr[i] = $(this).val();
    });
    var preOrderList = $("#gridtbody tr[orderno!='']");

    var preOrderNOList = new Array();
    preOrderList.each(function (i) {
        preOrderNOList[i] = $(this).attr("orderno");
    });
    //cardamout = cardamout.toFixed(2);
    var param = "?cashamout=" + cashamout + "&cardamout=" + cardamout + "&chkcreatetime=" + chkcreatetime + "&txtCreateTime=" + txtCreateTime + "&txtmemo=" + txtmemo + "&allamout=" + allamout + "&orderNo=" + orderNo + "&UserID=" + labuserid + "&CardIDstr=" + CardIDstr + "&ordertimesstr=" + ordertimesstr + "&preOrderNOList=" + preOrderNOList.toString();
    //$("#btn").attr("disabled", false);
    PayOrderPage = art.dialog.open("/CashManage/PayOrderPage" + param,
        { id: "payorderpage", cancel: false, lock: true, opacity: 0.05, title: "结账付款", width: 750, height: 440 });
}
//保存订单--结账保存
function SaveOrderCheck() {
    var userid = $("#labuserid").val();
    var laballPrice = $("#laballPrice").val();
    var Fiter = "";
    var counti = -1;
    $("#gridtbody tr").each(function (i) {
        counti = i;
    });
    if (counti == -1) {
        artdialogfail("请选择消费项目！"); return "";
        return;
    }
    var amout = $("#labamout").text();
    var ischeckedbox = true;
    var ordertime = true;
    //获取每行选中美容师顾问，组装数据
    $("#gridtbody tr").each(function (i) {
        var trval = $(this).find("input[name='GiveIDList']").val();
        var TypeList = $(this).find("input[name='TypeList']").val();
        var CardID = $(this).find("input[name='CardID']").val();
        var ordertimes = $(this).find("input[name='ordertimes']").val();
        var useridlist = $(this).find("input:checked");
        if (useridlist.length == 0) { ischeckedbox = false; return false } else { ischeckedbox = true; }
        if (ordertimes == 0) { ordertime = false; return false } else { ordertime = true; }
        if ((ordertimes.toString()).indexOf(".") != -1) { ordertime = false; return false } else { ordertime = true; }
        $(this).find("input[name='UserID']").attr("Name", "UserID" + "-" + TypeList + "-" + CardID + "-" + trval); //UserID+类型+ID组成
    });
    if (ordertime==false) {
        //artdialogfail("数量不能为0！"); return false;
        artdialogfail("数量不能为0且为整数！"); return false;
    }
    if (ischeckedbox == false) {
        artdialogfail("请选择美容师、顾问！"); return false;
    }
    //$("#btn").attr("disabled", "disabled");
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "json", //返回json格式的数据
        url: getControllerUrl("CashManage", "SaveOrder1"),
        data: $('#myform').serialize(),
        async: false,
        success: function (data) {
            if (data != null) {
                OrderNo = data;
            }
        },
        error: function (e) {
            //alert(e.responseText);
            artdialogfail("操作失败！");
        }
    });
    return OrderNo;

}
//只能输入浮点或整数
function CheckInputIntFloat0and1(oInput) {
    if ('' != oInput.value.replace(/\d{1,}\.{0,1}\d{0,}/, '')) {
        oInput.value = oInput.value.match(/\d{1,}\.{0,1}\d{0,}/) == null ? '' : oInput.value.match(/\d{1,}\.{0,1}\d{0,}/);
    }
    if (oInput.value > 1 || oInput.value < 0) {
        oInput.value = 1;
    }
}
function CheckInputIntFloat0and9(oInput) {
    if ('' != oInput.value.replace(/\d{1,}\.{0,1}\d{0,}/, '')) {
        oInput.value = oInput.value.match(/\d{1,}\.{0,1}\d{0,}/) == null ? '' : oInput.value.match(/\d{1,}\.{0,1}\d{0,}/);
    }
}
function CheckInputIntFloat(oInput) {
    if ('' != oInput.value.replace(/^[-]?\d{1,}\.{0,1}\d{0,}/, '')) {
        oInput.value = oInput.value.match(/^[-]?\d{1,}\.{0,1}\d{0,}/) == null ? '' : oInput.value.match(/^[-]?\d{1,}\.{0,1}\d{0,}/);
    }
}
function CheckInputIntFloatCompare(oInput) {
    if ('' != oInput.value.replace(/^[-]?\d{1,}\.{0,1}\d{0,}/, '')) {
        oInput.value = oInput.value.match(/^[-]?\d{1,}\.{0,1}\d{0,}/) == null ? '' : oInput.value.match(/^[-]?\d{1,}\.{0,1}\d{0,}/);
    }
    if (parseInt($(oInput).val()) > parseInt($(oInput).attr("code"))) {
        oInput.value = "1";
    }
}
//选择项目后的事件
function Selectsection(obj) {
    //只能加载一次项目，产品，划扣次卡，不能重复加载
    var isshownext = true;
    $("input[name='GiveIDString']").each(function (i) {
        if ("1-" + obj.split("|")[0] == $(this).val()) {
            isshownext = false;
            return false;
        } else {
            isshownext = true;
        }
    });


    if (!isshownext) return;

    var trLength = $("#gridtbody tr").length;
    var appendUser = $("#appendUser").html().replace(new RegExp(/#chk/g), 'chk' + trLength);
    var appendGuUser = $("#appendGuUser").html();
    var arry = obj.split("$");
    var str = "";
    for (var i = 0; i < arry.length - 1; i++) {
        str += '<tr class="cashtype"><td><label name="typename" >项目</label></td><td><input type="hidden" name="ISKit" value=' + arry[i].split("|")[6] + ' /><input type="hidden" name="CardID" value="0" /><input type="hidden" name="CardType" value="2" /><input type="hidden" name="TypeList" value="1" /><input type="hidden" name="GiveIDList" value="' + arry[i].split("|")[0] + '" /><input type="hidden" name="GiveIDString" value="' + '1-' + arry[i].split("|")[0] + '" />' + arry[i].split("|")[1] + '</td>'
       + '<td><input type="hidden" name="PriceList" value=' + arry[i].split("|")[2] + ' ><label class="productmoney">' + arry[i].split("|")[2] + '</label></td>'
        + '<td><input type="text" class="smallinput zhekou" onkeyup="CheckInputIntFloat0and1(this)" name="zhekou" value="1"></td>'
       + '<td><input type="text" class="smallinput times"  name="ordertimes" value="1"></td>'
      + '<td><input type="text" class="smallinput alltimes"  onkeyup="CheckInputIntFloat(this)" name="orderprice"  value=' + arry[i].split("|")[2] + '></td>'
      + '<td  style="width:80px;">' + appendUser + '</td>'   //美容师下拉框
     // + '<td  class="isshowth" style=' + (isshow == true ? "display:''" : "display:none;") + '>' + appendGuUser + '</td>'   //顾问下拉框

     + '<td class="isshowth" style=' + (isshow == true ? "display:''" : "display:none;") + '><select class="IsZhiding" name="IsZhiding"><option value="0">否</option><option value="1">是</option></select></td>'   //
    //  + '<td class="isshowth" style=' + (isshow == true ? "display:''" : "display:none;") + '><input  name="XiaoHao" type="text" name="" class="smallinput"></td>'
      + '<td class="isshowth" style=' + (isshow == true ? "display:''" : "display:none;") + '><input name="Ticheng" type="text" class="smallinput"></td>'
       + '<td><a href="javascript:;" class=\'deletebtn\'>删除</a></td></tr>'
    }
    $("#gridtbody").append(str);
    Caculatecash();
    //上下左右按键
    var baseIndex = 100;
    $("#gridtbody").find("tr").each(function (r) {
        $(this).find("td").each(function (c) {
            $(this).find("input").attr("tabindex", r * 100 + c + baseIndex);
        });
    });
}
//选择产品后的事件
function Selectproduct(obj) {

    //只能加载一次项目，产品，划扣次卡，不能重复加载
    var isshownext = true;
    $("input[name='GiveIDString']").each(function (i) {
        if ("2-" + obj.split("|")[0] == $(this).val()) {
            isshownext = false;
            return false;
        } else {
            isshownext = true;
        }
    });
    if (!isshownext) return;

    var trLength = $("#gridtbody tr").length;
    var appendUser = $("#appendUser").html().replace(new RegExp(/#chk/g), 'chk' + trLength);
    var appendGuUser = $("#appendGuUser").html();
    var arry = obj.split("$");
    var str = "";
    for (var i = 0; i < arry.length - 1; i++) {
        str += '<tr  class="cashtype"><td><label name="typename" >产品</label></td><td><input type="hidden" name="CardID" value="0" /><input type="hidden" name="CardType" value="2" /><input type="hidden" name="TypeList" value="2" /><input type="hidden" name="GiveIDList" value="' + arry[i].split("|")[0] + '" /><input type="hidden" name="GiveIDString" value="' + '2-' + arry[i].split("|")[0] + '" />' + arry[i].split("|")[1] + '</td>'
       + '<td><input type="hidden" name="PriceList" value=' + arry[i].split("|")[2] + ' ><label class="productmoney">' + arry[i].split("|")[2] + '</label></td>'
       + '<td><input type="text" class="smallinput zhekou"  onkeyup="CheckInputIntFloat0and1(this)" name="zhekou" value="1"></td>'
       + '<td><input type="text" class="smallinput times"  name="ordertimes" value="1"></td>'

      + '<td><input type="text" class="smallinput alltimes"  onkeyup="CheckInputIntFloat(this)"  name="orderprice"  value=' + arry[i].split("|")[2] + '></td>'
        + '<td style="width:80px;">' + appendUser + '</td>'   //美容师下拉框
    //  + '<td  class="isshowth" style=' + (isshow == true ? "display:''" : "display:none;") + '>' + appendGuUser + '</td>'    //顾问下拉框

+ '<td class="isshowth"  style=' + (isshow == true ? "display:''" : "display:none;") + '><select  class="IsZhiding" name="IsZhiding"><option value="0">否</option><option value="1">是</option></select></td>'   //
    //  + '<td class="isshowth"  style=' + (isshow == true ? "display:''" : "display:none;") + '><input  name="XiaoHao" type="text" class="smallinput"></td>'
      + '<td class="isshowth"  style=' + (isshow == true ? "display:''" : "display:none;") + '><input  name="Ticheng" type="text" class="smallinput"></td>'
       + '<td><a href="javascript:;" class=\'deletebtn\'>删除</a></td></tr>'
    }
    $("#gridtbody").append(str);
    Caculatecash();
    //上下左右按键
    var baseIndex = 100;
    $("#gridtbody").find("tr").each(function (r) {
        $(this).find("td").each(function (c) {
            $(this).find("input").attr("tabindex", r * 100 + c + baseIndex);
        });
    })
}
//选择顾客后事件
function InFoDate(obj) {
    var userid = obj.split("|")[0];
    $("#btn").attr("disabled", false);
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

                // $("#labUserName").attr("onclick", "OpenPatientDetail(" + data.UserID + ",1)");
                $("#labUserName").attr("href", "javascript:OpenPatientDetail(" + data.UserID + ",1);");
                //$("#labUserName").attr("href", "/PatientManage/PatientDetail?type=1&UserID=" + data.UserID);
                $("#labQinakuan").attr("href", "/PatientManage/PatientDetail?type=6&UserID=" + data.UserID);
                $("#txtmemo").val("");

            }
            $("#selectProject").empty();
            $("#gridtbody").empty();

            //加载下拉框值
            LoadSelection(userid);
            //加载储值下拉
            LoadChuzhiSelection(userid);
            //加载储值卡总额
            GetStoredCardListSUM(userid);
            //积分
            GetIntegralSUM(userid);


        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}


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

    // var selHospitalID = $(this).val();

    // $("tr:not(:first)").find("option[value=" + selHospitalID + "]").attr("selected", true);
    //    if ($(this).prop("checked") == true) {
    //        $("#gridtbody  .liHide  input:checkbox[value='" + $(this).val() + "']").attr("checked", "checked");
    //        var $tr = $("#gridtbody tr:gt(0)").find(".selectUL");
    //        var codestr = "<span class='labcode' userid='" + $(this).val() + "'>" + $(this).attr("code") + "&nbsp;&nbsp;</span>";
    //        $tr.after(codestr);

    //    } else {
    //        $("#gridtbody  .liHide  input:checkbox[value='" + $(this).val() + "']").attr("checked", false);

    //        var parentcode = $("#gridtbody tr:gt(0)").find(".selectUL");
    //        var dsd = parentcode.find(".labcode[userid=" + $(this).val() + "]");
    //        dsd.remove();
    //        if (parentcode.find(".labcode").length == 0) {
    //            parentcode.find(".liMenu").css("display", "block");
    //        }
    //    }

});


//加载疗程卡下拉框值
function LoadSelection(userid) {
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetProjectBalanceText"),
        data: { "UserID": userid, "ID": 0 },
        success: function (data) {
            $("#selectUL").html("");
            //赋值下拉框
            $("#selectUL").append(data);
            Caculatecash();
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });

}
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
//加载储值卡总额
function GetStoredCardListSUM(id) {
    if (id > 0) {
        $.ajax({
            type: "post", //使用get方法访问后台
            dataType: "text", //返回json格式的数据
            url: getControllerUrl("CashManage", "ToGetStoredCardListSUM"),
            data: { "id": id },
            success: function (data) {
                if (data != null) {
                    $("#labChuzhi").text(data);
                }
            },
            error: function () {
                alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
            }
        });
    }
}
//加载积分
function GetIntegralSUM(id) {
    console.log("11111")
    if (id != "") {
        $.ajax({
            type: "post", //使用get方法访问后台
            dataType: "text", //返回json格式的数据
            url: getControllerUrl("CashManage", "GetIntegralSUM"),
            data: { "id": id },
            success: function (data) {
                if (data != null) {
                    $("#labjifen").text(data);
                }
            },
            error: function () {
                alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
            }
        });
    }
}

////卡扣下拉框改变事件
//$("#selectProject").change(function () {
//    var selectval = $(this).val();
//    var userid = $("#labuserid").val();
//  SelectProjectFun(selectval,userid);
//});
function SelectProjectFun(selectval, alltimes) {
    var userid = $("#labuserid").val();
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "json", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetProjectBalance"),
        data: { "UserID": userid, "ID": selectval },
        success: function (data) {
            if (data != null) {
                //只能加载一次项目，产品，划扣次卡，不能重复加载
                var nowcard = data[0].ID; //此ID为用户余额主键ID
                var isshownext = true;
                var cardlist = $("input[name='CardID']");
                cardlist.each(function (i) {
                    if (nowcard.toString() == $(this).val()) {
                        isshownext = false;
                        return false;
                    } else {
                        isshownext = true;
                    }
                });
                if (!isshownext) return;
                var trLength = $("#gridtbody tr").length;
                var appendUser = $("#appendUser").html().replace(new RegExp(/#chk/g), 'chk' + trLength);
                var appendGuUser = $("#appendGuUser").html();
//                var str = '<tr class="cardtype"><td>划卡</td><td><input type="hidden" name="CardID" value="' + data[0].ID + '" /><input type="hidden" name="CardType" value="1" /><input type="hidden" name="TypeList" value="1" /><input type="hidden" name="GiveIDList" value="' + data[0].ProjectID + '" />' + data[0].Name + '</td>'
//       + '<td><input type="hidden" name="PriceList" value=' + data[0].Price + ' ><label class="productmoney">' + data[0].Price + '</label></td>'
//       + '<td><input type="text" class="smallinput zhekou"  readonly="readonly"  onkeyup="CheckInputIntFloat0and1(this)" name="zhekou" value="1"></td>'
//       + '<td><input type="text" class="smallinput times"   name="ordertimes" value="1" code=' + alltimes + '></td>'

//      + '<td><input type="text" class="smallinput alltimes" readonly="readonly"  onkeyup="CheckInputIntFloat(this)" name="orderprice" value=' + data[0].Price + '></td>'
//         + '<td style="width:80px;">' + appendUser + '</td>'   //美容师下拉框
//     // + '<td class="isshowth"  style=' + (isshow == true ? "display:''" : "display:none;") + '>' + appendGuUser + '</td>'  //顾问下拉框

//+ '<td class="isshowth"  style=' + (isshow == true ? "display:''" : "display:none;") + '><select  class="IsZhiding" name="IsZhiding"><option value="0">否</option><option value="1">是</option></select></td>'   //
//    //  + '<td class="isshowth"  style=' + (isshow == true ? "display:''" : "display:none;") + '><input name="XiaoHao" type="text" class="smallinput"></td>'
//      + '<td class="isshowth"  style=' + (isshow == true ? "display:''" : "display:none;") + '><input name="Ticheng" type="text" class="smallinput"></td>'
//                    + '<td><a href="javascript:;" class=\'deletebtn\' code=' + selectval + '>删除</a></td></tr>'
                
       var str = '<tr class="cardtype"><td>划卡</td><td><input type="hidden" name="CardID" value="' + data[0].ID + '" /><input type="hidden" name="CardType" value="1" /><input type="hidden" name="TypeList" value="1" /><input type="hidden" name="GiveIDList" value="' + data[0].ProjectID + '" />' + data[0].Name + '</td>'
       + '<td><input type="hidden" name="PriceList" value=' + data[0].ConsumePrice + ' ><label class="productmoney">' + data[0].ConsumePrice + '</label></td>'
       + '<td><input type="text" class="smallinput zhekou"  readonly="readonly"  onkeyup="CheckInputIntFloat0and1(this)" name="zhekou" value="1"></td>'
       + '<td><input type="text" class="smallinput times"   name="ordertimes" value="1" code=' + alltimes + '></td>'

      + '<td><input type="text" class="smallinput alltimes" readonly="readonly"  onkeyup="CheckInputIntFloat(this)" name="orderprice" value=' + data[0].ConsumePrice + '></td>'
         + '<td style="width:80px;">' + appendUser + '</td>'   //美容师下拉框
     // + '<td class="isshowth"  style=' + (isshow == true ? "display:''" : "display:none;") + '>' + appendGuUser + '</td>'  //顾问下拉框

+ '<td class="isshowth"  style=' + (isshow == true ? "display:''" : "display:none;") + '><select  class="IsZhiding" name="IsZhiding"><option value="0">否</option><option value="1">是</option></select></td>'   //
    //  + '<td class="isshowth"  style=' + (isshow == true ? "display:''" : "display:none;") + '><input name="XiaoHao" type="text" class="smallinput"></td>'
      + '<td class="isshowth"  style=' + (isshow == true ? "display:''" : "display:none;") + '><input name="Ticheng" type="text" class="smallinput"></td>'
       + '<td><a href="javascript:;" class=\'deletebtn\' code=' + selectval + '>删除</a></td></tr>'
                 
                $("#gridtbody").append(str);
                Caculatecash();
                //上下左右按键
                var baseIndex = 100;
                $("#gridtbody").find("tr").each(function(r) {
                    $(this).find("td").each(function(c) {
                        $(this).find("input").attr("tabindex", r * 100 + c + baseIndex);
                    });
                });
            }
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}
//挂单操作
function OperateOrder() {
    var userid = $("#labuserid").val();
    if (userid == "0") {
        artdialogfail("请选择客户！");
        return;
    }
    if (confirm("确认挂单操作吗？")) {
        SaveOrder();
    }
}
//保存订单--挂单保存
function SaveOrder() {
    var userid = $("#labuserid").val();
    var laballPrice = $("#laballPrice").val();
    var OrderNo = "";
    var counti = -1;
    $("#gridtbody tr").each(function (i) {
        counti = i;
    });
    if (counti == -1) {
        artdialogfail("请选择消费项目！"); return "";
        return;
    }
    var amout = $("#labamout").text();
    var ischeckedbox = true;
    //获取每行选中美容师顾问，组装数据
    $("#gridtbody tr").each(function (i) {
        //        var trval = $(this).find("input[name='GiveIDList']").val();
        //        var TypeList = $(this).find("input[name='TypeList']").val();
        //        var useridlist = $(this).find("input:checked");
        //        if (useridlist.length == 0) { ischeckedbox = false; return false } else { ischeckedbox = true; }
        //        $(this).find("input[name='UserID']").attr("Name", "UserID" + "-" + TypeList + "-" + trval); //UserID+类型+ID组成


        var trval = $(this).find("input[name='GiveIDList']").val();
        var TypeList = $(this).find("input[name='TypeList']").val();
        var CardID = $(this).find("input[name='CardID']").val();
        var useridlist = $(this).find("input:checked");
        if (useridlist.length == 0) { ischeckedbox = false; return false } else { ischeckedbox = true; }
        $(this).find("input[name='UserID']").attr("Name", "UserID" + "-" + TypeList + "-" + CardID + "-" + trval); //UserID+类型+ID组成
    });
    if (ischeckedbox == false) {
        artdialogfail("请选择美容师、顾问！"); return false;
    }
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "json", //返回json格式的数据
        url: getControllerUrl("CashManage", "SaveOrderandupdatebed"),
        data: $('#myform').serialize(),
        async: false,
        success: function (data) {
            if (data != null) {
                OrderNo = data;
                artdialog("操作成功！", "");
                location.reload();
                try { art.dialog.open.api.close(); } catch (e) { }
            }
                //汪
            else {
                artdialogfail("该顾客已有挂单，请处理完在进行挂单操作！");
            }
            //汪
        },
        error: function () {
            artdialogfail("操作失败！");
        }
    });
    return OrderNo;

}

//提单操作
function SelectOrder(obj) {

    var orderNo = obj.split("|")[0];
    $("#labuserid").val(obj.split("|")[1]);
    var str = obj.split("|")[1] + "|000";
    InFoDate(str);
    //$("#OldOrderNo").val(orderNo);//保存之前的订单编号，进行删除操作

    //跟进订单编号提取订单详情列表
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetOrderDetail"),
        data: { "OrderNo": orderNo },
        success: function (data) {
            $("#gridtbody").empty();
            if ($(".isshowth").is(":visible")) {
                $(".isshowth").css("display", "none");
            }
            $("#gridtbody").append(data);

            Caculatecash();
        },
        error: function () {
            artdialogfail("操作失败！");
        }
    });

    //获取订单备注
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetOrderName"),
        data: { "OrderNo": orderNo },
        success: function (data) {
            $("#txtmemo").val(data);
            //Caculatecash();
        },
        error: function () {
            artdialogfail("操作失败！");
        }
    });

}


///////////////////////////////////床位入口操作////////////////////////////////////////////////////////
//挂单操作
function OperateOrderonBed() {
    var userid = $("#labuserid").val();
    if (userid == "0") {
        artdialogfail("请选择客户！");
        return;
    }
    if (confirm("确认挂单操作吗？")) {
        SaveOrderonBed();
    }
}
//保存订单--床位入口--挂单保存
function SaveOrderonBed() {
    var userid = $("#labuserid").val();
    var laballPrice = $("#laballPrice").val();
    var bedid = $("#bedid").val(); //获取床位ID
    var OrderNo = $("#orderno").val(); //获取订单编号

    if (userid == "0" || laballPrice == "0") {
        artdialogfail("请选择客户和消费项目！"); return "";
        return;
    }
    var amout = $("#labamout").text();
    var ischeckedbox = true;
    //获取每行选中美容师顾问，组装数据
    $("#gridtbody tr").each(function (i) {
        var trval = $(this).find("input[name='GiveIDList']").val();
        var TypeList = $(this).find("input[name='TypeList']").val();
        var useridlist = $(this).find("input:checked");
        if (useridlist.length == 0) { ischeckedbox = false; return false } else { ischeckedbox = true; }
        $(this).find("input[name='UserID']").attr("Name", "UserID" + "-" + TypeList + "-" + trval); //UserID+类型+ID组成
    });
    if (ischeckedbox == false) {
        artdialogfail("请选择美容师、顾问！"); return false;
    }

    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "json", //返回json格式的数据
        url: getControllerUrl("CashManage", "SaveOrderandupdatebed?bedid=" + bedid + "&OrderNo=" + OrderNo),
        data: $('#myform').serialize(),
        async: false,
        success: function (data) {

            if (data != null) {
                OrderNo = data;
                artdialog("操作成功！", "")
                location.reload();
            }
        },
        error: function () {
            artdialogfail("操作失败！");
        }
    });
    return OrderNo;
}

//结账 ---床位入口
function CheckOrderOnBed() {
    var userid = $("#labuserid").val();
    if (userid == "0") {
        artdialogfail("请选择客户！");
        return;
    }
    var cashamout = $("#labcash").text(); //现金总额
    var cardamout = $("#labcardamout").text(); //卡扣总额
    var allamout = $("#labamout").text(); //所有总额
    var labuserid = $("#labuserid").val(); //所有总额
    var OldOrderNo = $("#orderno").val(); //旧的订单编号
    var orderNo = SaveOrderCheck();  //新的订单编号

    if (orderNo == "") { return; }
    var CardID = $("#gridtbody .cardtype").find("input[name='CardID']");
    var CardIDstr = new Array();
    CardID.each(function (i) {
        CardIDstr[i] = $(this).val();
    });
    var ordertimes = $("#gridtbody .cardtype").find("input[name='ordertimes']");
    var ordertimesstr = new Array();
    ordertimes.each(function (i) {
        ordertimesstr[i] = $(this).val();
    });
    //cardamout = cardamout.toFixed(2);
    var param = "?cashamout=" + cashamout + "&cardamout=" + cardamout + "&allamout=" + allamout + "&orderNo=" + orderNo + "&UserID=" + labuserid + "&CardIDstr=" + CardIDstr + "&ordertimesstr=" + ordertimesstr + "&OldOrderNo=" + OldOrderNo;
    art.dialog.open("/CashManage/PayOrderPageOnBed" + param, { title: "结账付款", width: 750, height: 440 })
}


