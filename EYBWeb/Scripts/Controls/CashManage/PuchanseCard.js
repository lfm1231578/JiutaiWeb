var isshowdoc = false;
//选择顾客后事件
function InFoDate(obj) {

    var userid = obj.split("|")[0];
    $("#labuserid").val(userid);
    $("#labuserid1").val(userid);
    $("#tab_conbox").find("label.showusername").remove();
    $("#tab_conbox").find("input:checkbox").attr("checked", false);
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
                //$("#labUserName").attr("onclick", "OpenPatientDetail(" + data.UserID + ",1)");
                $("#labUserName").attr("href", "javascript:OpenPatientDetail(" + data.UserID + ",1);");
                //  $("#labUserName").attr("href", "/PatientManage/PatientDetail?type=1&UserID=" + data.UserID);
                $("#purchasetbody3").empty();
                $("#purchasetbody").empty();
                $("#purchasetbody2").empty();
                $("#labamout").text("0");
                $("#labamout2").text("0");
                $("#labcash0").text("0");
                $("#labamout0").text("0");
                $("#txtmemo").val("");
                //加载储值下拉
                LoadChuzhiSelection(userid);
            }
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
    //获取存储卡列表
    GetStoredCardList(userid);

    //获取已购买疗程卡信息
    GetprojectCardlist(userid);

    //获取已购买方案卡信息
    GetFanganCardlist(userid);

    //加载储值卡总额
    GetStoredCardListSUM(userid);
    GetIntegralSUM(userid);

}
//获取已购买疗程卡信息
function GetprojectCardlist(userid) {

    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetprojectCardlist"),
        data: { "UserID": userid },
        success: function (data) {
            if (data != null) {
                $("#cardtbody1").empty();
                $("#cardtbody1").append(data);
            }
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}

/////////////////////////////////////美容师 选择 //////////////////////////////////////

var countj = 0;

//第一次选中第一个，下面的都选中，第二次选中第一个，下面的无操作
$(".accountdiv1").delegate('.liHide :checkbox', 'click', function () {
    if ($(this).prop("checked") == true) {
        var codestr = "<label  class='showusername' userid='" + $(this).val() + "'>" + $(this).attr("code") + "&nbsp;&nbsp;</label>";
        $(".accountdiv1 .liMenu").append(codestr);
        $("#purchasetbody3  .liHide  input:checkbox[value='" + $(this).val() + "']").attr("checked", "checked");
    } else {
        var dsd = $(".accountdiv1 .liMenu").find(".showusername[userid=" + $(this).val() + "]");
        dsd.remove();
        $("#purchasetbody3  .liHide  input:checkbox[value='" + $(this).val() + "']").attr("checked", false);
    }
});
$(".accountdiv2").delegate('.liHide :checkbox', 'click', function () {
    if ($(this).prop("checked") == true) {
        var codestr = "<label  class='showusername' userid='" + $(this).val() + "'>" + $(this).attr("code") + "&nbsp;&nbsp;</label>";
        $(".accountdiv2 .liMenu").append(codestr);
        $("#purchasetbody  .liHide  input:checkbox[value='" + $(this).val() + "']").attr("checked", "checked");
    } else {
        var dsd = $(".accountdiv2 .liMenu").find(".showusername[userid=" + $(this).val() + "]");
        dsd.remove();
        $("#purchasetbody  .liHide  input:checkbox[value='" + $(this).val() + "']").attr("checked", false);
    }
});
$(".accountdiv3").delegate('.liHide :checkbox', 'click', function () {
    if ($(this).prop("checked") == true) {
        $("#purchasetbody2  .liHide  input:checkbox[value='" + $(this).val() + "']").attr("checked", "checked");
    } else {
        $("#purchasetbody2  .liHide  input:checkbox[value='" + $(this).val() + "']").attr("checked", false);
    }
});
$(".accountdiv1,.accountdiv3,.accountdiv2").delegate('tr .liHide :checkbox', 'click', function () {
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
$(".accountdiv1,.accountdiv2,.accountdiv3").delegate(".liMenu", "mouseover", function () {
    var curent = $(this).parent().parent().parent().find(".liHide");
    curent.css("display", "block");
});
$(".accountdiv1,.accountdiv2,.accountdiv3").delegate(".liHide", "mouseleave", function () {
    $(this).fadeOut("slow");
});
$(".accountdiv1,.accountdiv2,.accountdiv3").delegate(".labcode", "mouseover", function () {
    var curent = $(this).parent().find(".liHide");
    curent.css("display", "block");
});


//其他门店员工选择--疗程卡
$("#selaccountdiv2").delegate(".selHospitalID", "change", function () {
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

    var typelist = $("#purchasetbody tr");
    var nowzhekou;
    var XiaofeiAllPrice = 0;//消费总额
    var shoujia = 0;//卡项售价
    typelist.each(function (i) {
        var idandvalue = $(this).find("input[class='hidproject']").val();
        var arry = idandvalue.split(",");
        var ISKit = "";
        var hidproject = "";
        for (var i = 0; i < arry.length; i++) {

            var nowarry= arry[i].split("|");
            ISKit = nowarry[5];//是否套盒
            var XiangMuAllPrice = nowarry[3];//项目总价
            if (ISKit == "2") { //是套盒
                nowzhekou = taohezhekou;
            } else { //不是套盒
                nowzhekou = yuanyongzhekou;
            }
            XiangMuAllPrice =(XiangMuAllPrice * nowzhekou).toFixed(2);
            nowarry[3] = XiangMuAllPrice;//改变项目总价的值
            shoujia = parseFloat(shoujia) + parseFloat(nowarry[3]);
            XiaofeiAllPrice = parseFloat(XiaofeiAllPrice) + parseFloat(nowarry[3]);//消费总额=所有项目总价相加
            hidproject = hidproject + nowarry[0] + "|" + nowarry[1] + "|" + nowarry[2] + "|" + nowarry[3] + "|" + nowarry[4] + "|" +  nowarry[5] + ",";
        }
        $(this).find("input[class='hidproject']").val(hidproject.substring(0,hidproject.length-1));
        $(this).find("input[name='Price']").val(shoujia);//改变卡项售价文本
        shoujia = 0;//让售价归零，不然会叠加数据
        //Caculatecash();
    });
    $("#labamout").text(XiaofeiAllPrice);
    $("#txtchuzhishow").fadeOut("slow");
});
////折扣事件改变
//$("#purchasetbody").delegate('.zhekou', 'keyup', function () {
//    var zhekou = $(this).val();
//    var nowparent = $(this).parent().parent();
//    console.log(nowparent)
//    var productmoney = nowparent.find(".ConsumptionPrice").val();
//    console.log(productmoney)
//    var times = nowparent.find(".ProjectProductQualtity").val();
// //   console.log("长生》》》", productmoney,times);
//    nowparent.find(".Price").val((times * productmoney * zhekou).toFixed(2));
//    Caculatecash();
//});
//次数改变
//$("#purchasetbody").delegate('.ProjectProductQualtity', 'keyup', function () {
//    var times = $(this).val();
//    var nowparent = $(this).parent().parent();
//    var zhekou = nowparent.find(".zhekou").val();
//    console.log(nowparent)
//    var productmoney = nowparent.find(".ConsumptionPrice").val();
//    console.log(productmoney)
//   // var times = nowparent.find(".ProjectProductQualtity").val();
//    console.log("次数改变", zhekou);
//    nowparent.find(".Price").val((times * productmoney * zhekou).toFixed(2));
//     Caculatecash();
//});
function Caculatecash() {
    //卡扣总额变化
    var nowparent = $(this).parent().parent();
    var currentclass = $("#purchasetbody").find(".cardtype");
    var cardmoney = 0;
    currentclass.each(function (i) {
        cardmoney += parseFloat($(this).find(".Price").val());
    });
 //   $("#labcardamout").text(cardmoney);
    //现金总额变化
    var currentcash = $("#purchasetbody").find(".cashtype");
    var cashmoney = 0;
    currentcash.each(function (i) {
        cashmoney += parseFloat($(this).find(".Price").val());
    });
   // $("#labcash").text(cashmoney.toFixed(2));

    $("#labamout").text((cashmoney + cardmoney).toFixed(2));
  //  $("#laballPrice").val((cashmoney + cardmoney).toFixed(2));


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
//////////////////////////////////////////显示储值卡余额  结束///////////////////////////////////////////////////////////////////

//其他门店员工选择---储值卡
$("#selaccountdiv1").delegate(".selHospitalID", "change", function () {
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
////////////////////////////////////////////////////////END/////////////////////////////////////
//获取已购买方案卡信息
function GetFanganCardlist(userid) {
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("CashManage", "GetFanganCardlist"),
        data: { "UserID": userid },
        success: function (data) {
            if (data != null) {
                $("#cardtbody2").empty();
                $("#cardtbody2").append(data);
            }
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}
//获取可以充值的储值卡列表
function GetStoredCardList(id) {
    if (id > 0) {
        $.ajax({
            type: "post", //使用get方法访问后台
            dataType: "text", //返回json格式的数据
            url: getControllerUrl("CashManage", "ToGetStoredCardList"),
            data: { "id": id },
            success: function (data) {
                if (data != null) {
                    $("#cardtbod3").empty();
                    $("#cardtbod3").append(data);
                }
            },
            error: function () {
                alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
            }
        });
    }
}

$(function () {

    ////Tab切换
    $("#tabs  li").click(function () {
        $("#tabs li").removeClass("thistab");
        $(this).addClass("thistab");
        $(".tab_con").hide();
        var index = $(this).parent().find("li").index(this);
        $("#tab_conbox").find(".tab_con").eq(index).show();
    });
    $("#tabs li:first").addClass("thistab").show();

    //添加疗程卡卡项
    $(".addtreament").click(function () {
        if (IsselectUser()) {
            art.dialog.open("/BaseInfo/CardList?Type=2", { title: "疗程卡列表", width: 700, height: 470 });
        } else {
            artdialogfailTwo("请选择客户！", "", "250", "100");
            return false;
        }
    });

    //添加方案卡卡项
    $("#addpro").click(function () {
        if (IsselectUser()) {
            art.dialog.open("/BaseInfo/CardList?Type=3", { title: "方案卡列表", width: 680, height: 470 });
        } else {
            artdialogfailTwo("请选择客户！", "", "250", "100");
            return false;
        }
    });

    //添加方案卡卡项
    $("#addsto").click(function () {
        if (IsselectUser()) {
            art.dialog.open("/BaseInfo/CardList?Type=1", { title: "储值卡列表", width: 680, height: 470 });
        } else {
            artdialogfailTwo("请选择客户！", "", "250", "100");
            return false;
        }
    });
});
/****************疗程卡****************************/

//修改金额时自动更新价格purchasetbody
$("#purchasetbody").delegate(".Price", "keyup", function () {
    priceupdate();
});

function priceupdate() {
    var list = $("#purchasetbody").find(".Price");
    var allprice = 0;
    list.each(function (i) {
        allprice += parseFloat($(this).val());
    });
    $("#labamout").text(allprice);
}

//点击编辑时打开选择页面
$("#purchasetbody").delegate(".editproject", "click", function () {
    var thistd = $(this);
    var id = $(this).parent().parent().find("input[name=ID]").val();
    var project = $(this).parent().parent().find('.hidproject').val();
    art.dialog.open("/BaseInfo/ProjectEdit?ID=" + id + '&project=' + project,
        {
            title: "项目列表",
            width: 760,
            height: 470,
            cancel: false,
            close: function () {
                var bValue = art.dialog.data('OValue'); // 读取B页面的数据
                if (bValue !== undefined) {
                    //这里添加一个绑定方法
                    projbind(bValue, thistd);
                    console.log("从B页面传回来的值", bValue);
                }
            }
        });
});

function projbind(obj, thethis) {
    thethis.parent().parent().find(".Price").val(obj.split('&')[0]);
    thethis.parent().parent().find(".hidproject").val(obj.split('&')[1]);
    priceupdate();
}

//选择卡项确认事件
function GetTemCardinfo(obj) {
    var newstr = 0;
    var arry = obj.split("$");
    var result = 0;
    //根据卡编号 查询项目  AJAX
    for (var i = 0; i < arry.length - 1; i++) {
        $.ajax({
            type: "POST",
            url: "/CashManage/getProjectProductNopage?CardID=" + arry[i].split("|")[0],
            async: false,
            dataType: "text",
            error: function (request) {
                artdialogfailTwo("操作失败！", "", "250", "100");
            },
            success: function (res) {
                if (res == '') {
                    artdialogfailTwo("疗程卡项目缺失", "", "250", "100");
                }
                else if (res != null) {
                   
                    newstr = res;
                    $("#purchasetbody").append('<tr><td><input type="hidden" name="ID" value="' + arry[i].split("|")[0] + '" /> <input type="hidden" class="hidproject" value="' + newstr + '" /> <input type="hidden" name="vallist" class="vallist" value="' + arry[i].split("|")[0] + '|' + arry[i].split("|")[2] + '" />' + arry[i].split("|")[1] + ' </td> <td><input type="text" name="Price"  readonly="readonly" class="Price wid50" value="' + arry[i].split("|")[2] + '" /></td> <td style=" display:none;">' + appendUser + '  </td> <td><a href="#" class="editproject">编辑</a> </td>   <td><a class="delthis" href="javascript:;" >删除</a></td>  </tr>');
                   // $("#purchasetbody").append('<tr class="cashtype"><td><input type="hidden" class="cardID" name="ID" value="' + arry[i].split("|")[0] + '" /> <input type="hidden" class="hidproject" value="' + newstr + '" /> <input type="hidden" name="vallist" class="vallist" value="' + arry[i].split("|")[0] + '|' + arry[i].split("|")[2] + '" />' + arry[i].split("|")[1] + ' </td> <td><input type="text" name="Price"  readonly="readonly" class="Price wid50" value="' + arry[i].split("|")[2] + '" /></td><td><input type="text" class="smallinput zhekou" onkeyup="CheckInputIntFloat0and1(this)" name="zhekou" value="1"></td><td><input type="text" class="smallinput ProjectProductQualtity" onkeyup="CheckInputIntFloat(this)" name="ProjectProductQualtity" value="' + arry[i].split("|")[7] + '"></td><td><input type="text" class="smallinput danjiaPrice" onkeyup="CheckInputIntFloat(this)" name="danjiaPrice" disabled="disabled" value="' + arry[i].split("|")[8] + '"></td><td><input type="text" class="smallinput ConsumptionPrice" onkeyup="CheckInputIntFloat(this)" name="ConsumptionPrice" value="' + arry[i].split("|")[9] + '"></td> <td style=" display:none;">' + appendUser + '  </td>   <td><a class="delthis" href="javascript:;" >删除</a></td>  </tr>');
                     result += parseFloat(arry[i].split("|")[2]);


                }
                else {
                    artdialogfailTwo("操作失败！", "", "250", "100");
                }
            }
        });
    }
    //$.ajax({
    //    type: "POST",
    //    url: "/CashManage/getProjectProductNopage?CardID=" + arry[0].split("|")[0],
    //    async: false,
    //    dataType: "text",
    //    error: function (request) {
    //        artdialogfailTwo("操作失败！", "", "250", "100");
    //    },
    //    success: function (res) {
    //        if (res != null) {
    //            newstr = res;
    //            alert(res);
    //        } else {
    //            artdialogfailTwo("操作失败,！", "", "250", "100");
    //        }
    //    }
    //});
    var str = "";
    //var result = 0;
    var appendUser = $("#appendUser").html(); //获取隐藏的参与员工列表
    //for (var i = 0; i < arry.length - 1; i++) {
    //    $("#purchasetbody").append('<tr><td><input type="hidden" name="ID" value="' + arry[i].split("|")[0] + '" /> <input type="hidden" class="hidproject" value="' + newstr + '" /> <input type="hidden" name="vallist" class="vallist" value="' + arry[i].split("|")[0] + '|' + arry[i].split("|")[2] + '" />' + arry[i].split("|")[1] + ' </td> <td><input type="text" name="Price"  readonly="readonly" class="Price wid50" value="' + arry[i].split("|")[2] + '" /></td> <td style=" display:none;">' + appendUser + '  </td> <td><a href="#" class="editproject">编辑</a> </td>   <td><a class="delthis" href="javascript:;" >删除</a></td>  </tr>');
    //    result += parseFloat(arry[i].split("|")[2]);
    //}
    var oldprice = $("#labamout").text();
    $("#labamout").text(parseFloat(oldprice) + result);

    $(".accountdiv2").find("input[name='UserID']").each(function () {
        $(this).attr("checked", false);
    });
}

var isshowdoc2 = false; //美容师下拉框显示控制
$("#purchasetbody").delegate(".liMenu", "click", function () {
    var curent = $(this).parent().parent().parent().find(".liHide");
    $(".liHide").slideUp("slow");
    if (isshowdoc2 == false) {
        curent.slideDown("fast");
        //curent.css("display", "block");
        isshowdoc2 = true;
    } else {
        curent.slideUp("slow");
        isshowdoc2 = false;
    }

});


//删除当前疗程卡
$("#purchasetbody").delegate(".delthis", "click", function () {
    $(this).parent().parent().remove();
    updateProjectprice();
})
function updateProjectprice() {
    var list = $("#purchasetbody").find(".Price");
    var allprice = 0;
    list.each(function (i) {
        allprice += parseFloat($(this).val());
    })
    $("#labamout").text(allprice);
}
//售价改变时更改(直接在最后调取Price,可以不用写方法)

//结账操作
$("#btnsavetem").click(function () {
    if ($("#labuserid").val() == "0") {
        artdialogfailTwo("请选择客户！", "", "250", "100");
        return false;
    }
    var isok = true;
    var userid = $("#labuserid").val(); //顾客ID
    var txtmemo = $("#txtmemo").val();

    var chkcreatetime = 0; //判断补单复选框是否选中
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

    var checedbox = $("#purchasetbody").find("tr");
    if (checedbox.length == 0) {
        artdialogfailTwo("请选择卡项！", "", "250", "100");
        return;
    }
    var UserIDList = ""; //参与员工集合
    var canUserIDList = new Array(); //参与员工集合
    var projectlisr = ""; //项目列表
    var idlist = ""; //疗程卡集合
    var newCarIDList = new Array();

    var newuseridlist = $("#selaccountdiv2").find("input[name='UserID']:checked");

    if (newuseridlist.length == 0) { isok = false; artdialogfailTwo("请选择参与员工！", "", "250", "100"); return false; } else { isok = true; }
    newuseridlist.each(function (j) {
        canUserIDList[j] = $(this).val();
    });
    UserIDList = canUserIDList.toString() + "$";


    if (canUserIDList.toString() == null || canUserIDList.toString() == "") {
        isok = false;
    }
    var plisarray = $("#purchasetbody").find(".hidproject");  //获取影藏的项目信息

    plisarray.each(function () {

      //  console.log($(this),"---------------->$this")
        projectlisr = projectlisr + $(this).val() + "^";

    });
    //var plisarray = "";

    //$(".ProjectProductQualtity").each(function () {
    //    var nowparent = $(this).parent().parent();
    //    var times = nowparent.find(".ProjectProductQualtity").val();
    //    var id = nowparent.find(".cardID").val();
    //    var productmoney = nowparent.find(".ConsumptionPrice").val();
    //    var shoujia = nowparent.find(".Price").val();
    //    var danjia = nowparent.find(".danjiaPrice").val();
    //    var zhekou = nowparent.find(".zhekou").val();
    //    var str = `${id}|${times}|${productmoney}|${shoujia}|${danjia}|${zhekou}^`
    //    projectlisr = projectlisr + str;
    //})
  
  
    var idlistarray = $("#purchasetbody").find("input[name=ID]");  //获取隐藏的疗程卡ID
    idlistarray.each(function (i) {
        idlist = idlist + $(this).val() + "$";
        newCarIDList[i] = $(this).val();
    });

    if (!isok) {
        artdialogfailTwo("请选择参与员工！", "", "250", "100");
        return false;
    }

    var nary = newCarIDList.sort();
    for (var i = 0; i < newCarIDList.length; i++) {

        if (nary[i] == nary[i + 1]) {
            artdialogfailTwo("卡项名称相同请分开购买！", "", "250", "100");
            return false;
        }
    }


    console.log($('#myform').serialize());

    $.ajax({
        type: "POST",
        url: encodeURI(getControllerUrl("CashManage", "AddCard1?uslist=" + UserIDList.toString() + "&userid=" + userid.toString() + "&isbuyliaocheng=1" + "&Name=" + txtmemo + "&Name=" + txtmemo + "&chkcreatetime=" + chkcreatetime + "&txtCreateTime=" + txtCreateTime)),
        async: false,
        dataType: "json",
        data: $('#myform').serialize(), // 你的formid
        error: function (request) {
            artdialogfailTwo("操作失败！", "", "250", "100");
        },
        success: function (res) {
            if (res != null) {

                var param = "?BuyType=1&cashamout=" + res.cashamout + "&cardamout=" + res.cardamout + "&allamout=" + res.allamout + "&orderNo=" + res.orderNo + "&UserID=" + res.UserID + "&CardIDstr=" + res.CardIDstr + "&ordertimesstr=" + res.ordertimesstr + "&projectlisr=" + projectlisr  + "&idlist=" + idlist + "&chkcreatetime=" + chkcreatetime + "&txtCreateTime=" + txtCreateTime + "&recharge=" + res.IsRecharge;
                art.dialog.open("/CashManage/PayOrderPagePuchanseCard" + param,
                    { title: "结账付款", cancel: false, lock: true, opacity: 0.05, width: 750, height: 430 });

            } else {
                artdialogfailTwo("操作失败,！", "", "250", "100");
            }
        }
    });
});
/****************疗程卡end****************************/



/*******************方案卡**********************************/
//选择方案卡确认事件
function GetProCardinfo(obj) {
    // var s = new Array();
    // s = obj.split("|");
    // var appendUser = $("#appendUser1").html(); //获取隐藏的参与员工列表
    var arry = obj.split("$");
    var str = "";
    var result = 0;
    var appendUser = $("#appendUser").html(); //获取隐藏的参与员工列表
    for (var i = 0; i < arry.length - 1; i++) {
        str += '<tr><td><input type="hidden" name="ID" value="' + arry[i].split("|")[0] + '" /> <input type="hidden" name="vallist" class="vallist" value="' + arry[i].split("|")[0] + '|' + arry[i].split("|")[2] + '" />' + arry[i].split("|")[1] + ' </td> <td><input type="text" name="Price"  onkeyup="CheckInputIntFloat(this)" class="Price wid50" value="' + arry[i].split("|")[2] + '" /></td> <td style=" display:none;">' + appendUser + '  </td>   <td><a class="delthis" href="javascript:;" >删除</a></td>  </tr>';
        result += parseFloat(arry[i].split("|")[2]);
    }
    $("#purchasetbody2").append(str);
    var oldprice = $("#labamout2").text();
    $("#labamout2").text(parseInt(oldprice) + result);
    $(".accountdiv3").find("input[name='UserID']").each(function () {
        $(this).attr("checked", false);
    });
}

var isshowdoc1 = false; //美容师下拉框显示控制
$("#purchasetbody2").delegate(".liMenu", "click", function () {
    var curent = $(this).parent().parent().parent().find(".liHide");
    $(".liHide").slideUp("slow");
    if (isshowdoc1 == false) {
        curent.slideDown("fast");
        //curent.css("display", "block");
        isshowdoc1 = true;
    } else {
        curent.slideUp("slow");
        isshowdoc1 = false;
    }

});


//删除当前方案卡
$("#purchasetbody2").delegate(".delthis",
    "click",
    function () {
        $(this).parent().parent().remove();
        updateProgramprice();
    });

$("#purchasetbody2").delegate(".Price",
    "keyup",
    function () {
        if ($(this).val() == '') {
            $(this).val(0);
        }
        updateProgramprice();
    });

function updateProgramprice() {
    var list = $("#purchasetbody2").find(".Price");
    var allprice = 0;
    list.each(function (i) {
        allprice += parseInt($(this).val());
    });
    $("#labamout2").text(allprice);
}
//提交操作
$("#btnsavepro").click(function () {
    if ($("#labuserid").val() == "0") {
        artdialogfailTwo("请选择客户！", "", "250", "100");
        return false;
    }

    var isok = true;
    var userid = $("#labuserid").val();
    var checedbox = $("#purchasetbody2").find("tr");
    if (checedbox.length == 0) {
        artdialogfailTwo("请选择卡项！", "", "250", "100");
        return;
    }
    var UserIDList = new Array(); //参与员工集合
    var canUserIDList = new Array(); //参与员工集合
    checedbox.each(function (i) {
        var newuseridlist = $(this).find("input[name='UserID']:checked");
        newuseridlist.each(function (j) {
            canUserIDList[j] = $(this).val();
        });
        if (canUserIDList.toString() == null || canUserIDList.toString() == "") {
            isok = false;
        }
        UserIDList[i] = canUserIDList.toString() + "$";
    });

    if (canUserIDList.toString() == null || canUserIDList.toString() == "") {
        isok = false;
    }

    if (!isok) {
        artdialogfailTwo("请选择参与员工！", "", "250", "100");
        return false;
    }

    $.ajax({
        type: "POST",
        url: getControllerUrl("CashManage", "AddCard1?uslist=" + UserIDList.toString() + "&userid=" + userid.toString()),
        async: false,
        dataType: "json",
        data: $('#myform1').serialize(), // 你的formid
        error: function (request) {
            artdialogfailTwo("操作失败！", "", "250", "100");
        },
        success: function (res) {

            if (res != null) {

                var param = "?cashamout=" +
                    res.cashamout +
                    "&cardamout=" +
                    res.cardamout +
                    "&allamout=" +
                    res.allamout +
                    "&orderNo=" +
                    res.orderNo +
                    "&UserID=" +
                    res.UserID +
                    "&CardIDstr=" +
                    res.CardIDstr +
                    "&ordertimesstr=" +
                    res.ordertimesstr +
                    "";
                art.dialog.open("/CashManage/PayOrderPagePuchanseCard" + param,
                    { title: "结账付款", lock: true, opacity: 0.05, width: 750, height: 440 });

            } else {
                artdialogfailTwo("操作失败,！", "", "250", "100");
            }
        }
    });
});
/*******************方案卡end**********************************/


/*******************储值卡*************************/
//选择储值卡确认事件
function GetstoCardinfo(obj) {
    //var s = new Array();
    //  s = obj.split("|");

    var isExist = false;
    $('#cardtbod3 tr').each(function () {
        var val = $(this).find('input:hidden.val').val();
        if (val.split('|')[2] == obj.split('|')[0]) {
            isExist = true;
            return false;
        }
    });
    if (isExist) {
        artdialogfailTwo("该顾客已有此卡，请充值。", "", "250", "100");
        return;
    }

    var appendUser = $("#appendUser3").html(); //获取隐藏的参与员工列表
    var arry = obj.split("$");
    var str = "";
    var result = 0;
    for (var i = 0; i < arry.length - 1; i++) {
        if (arry[i].split("|")[5] == "2") {//为2时是正常的储值卡,1是赠卡
            str += '<tr id="' + arry[i].split("|")[0] + '"><td> <input type="hidden" name="isgive" value="' + arry[i].split("|")[5] + '" /> <input type="hidden" name="Type" class="Type" value="2" /> <input type="hidden" name="ID" class="ID" value="' + arry[i].split("|")[0] + '" />' + arry[i].split("|")[1] + ' </td>  <td><input type="text" onkeyup="CheckInputIntFloat(this)" name="Price" class="Price wid50" value="' + arry[i].split("|")[2] + '" />   </td> <td><input type="text" onkeyup="CheckInputIntFloat0and1(this)" onchange="updatestoprice()" class="Discount wid50"  name="Discount" value="1" />   </td> <td style=" display:none;">' + appendUser + '</td><td><a href="javascript:;" class="delme" >删除</a> </td> </tr>';
        }
        else {
            str += '<tr id="' + arry[i].split("|")[0] + '"><td><input type="hidden" name="isgive" value="' + arry[i].split("|")[5] + '" /> <input type="hidden" name="Type" class="Type" value="2" /> <input type="hidden" name="ID" class="ID" value="' + arry[i].split("|")[0] + '" />' + arry[i].split("|")[1] + ' </td>  <td><input type="text"  onkeyup="CheckInputIntFloat(this)" name="Price" class="Price Pricezeng wid50" value="' + arry[i].split("|")[2] + '" />   </td> <td><input type="text"  onkeyup="CheckInputIntFloat0and1(this)" onchange="updatestoprice()" class="Discount wid50"  name="Discount" value="1" />   </td> <td style=" display:none;">' + appendUser + '</td><td><a href="javascript:;" class="delme" >删除</a> </td> </tr>';
        }
        result += parseFloat(arry[i].split("|")[2]);
    }
    $("#purchasetbody3").append(str);
    // var oldprice = $("#labamout0").text();
    // $("#labamout0").text(parseInt(oldprice) + result);
    updatestoprice();
    $(".accountdiv1").find("input[name='UserID']").each(function () {
        $(this).attr("checked", false);
    });


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
function CheckInputIntFloat(oInput) {
    if ('' != oInput.value.replace(/\d{1,}\.{0,1}\d{0,}/, '')) {
        oInput.value = oInput.value.match(/\d{1,}\.{0,1}\d{0,}/) == null ? '' : oInput.value.match(/\d{1,}\.{0,1}\d{0,}/);
    }

}
$("#cardtbod3").delegate(".thebox",
    "click",
    function () {
        var appendUser = $("#appendUser3").html(); //获取隐藏的参与员工列表
        var thisval = $(this); //获取当前对象
        var ischecked = thisval.attr('checked');
        if (ischecked == "checked") {
            var hidval = $(this).parent().find(".val").val(); //获取隐藏值
            var s = new Array();
            s = hidval.split("|");
            //purchasetable3
            var isgive = $(this).attr("isgive");
            if (isgive == "1") { //赠卡
                $("#purchasetable3").append('<tr id="' +
                    s[0] +
                    '"><td><input type="hidden" name="isgive" value="1" /> <input type="hidden" name="Type" class="Type" value="1" /> <input type="hidden" name="ID" class="ID" value="' +
                    s[0] +
                    '" />' +
                    s[3] +
                    ' </td>  <td><input type="text" name="Price" class="Pricezeng wid50" value="0" onkeyup="" />   </td><td><input type="text" class="wid50 Discount" name="Discount" value="1"   onkeyup="CheckInputIntFloat0and1(this)" /> </td>  <td style=" display:none;">' +
                    appendUser +
                    '</td><td><a href="javascript:;" class="delme" >删除</a> </td> </tr>')
            } else { //非赠卡
                $("#purchasetable3").append('<tr id="' +
                    s[0] +
                    '"><td><input type="hidden" name="isgive" value="2" /> <input type="hidden" name="Type" class="Type" value="1" /> <input type="hidden" name="ID" class="ID" value="' +
                    s[0] +
                    '" />' +
                    s[3] +
                    ' </td>  <td><input type="text" name="Price" class="Price wid50" value="0" onkeyup="CheckInputIntFloat(this)" />   </td><td><input type="text" class="wid50 Discount" name="Discount" value="1"   onkeyup="CheckInputIntFloat0and1(this)" /> </td>  <td style=" display:none;">' +
                    appendUser +
                    '</td><td><a href="javascript:;" class="delme" >删除</a> </td> </tr>')
            }
        } else {
            $("#purchasetable3").find("tr[id='" + thisval.val() + "']").remove();
        }

    });






//修改金额时自动更新价格purchasetbody
$("#purchasetbody3").delegate("input[name='Price']",
    "keyup",
    function () {
        if ($(this).val() == '') {
            $(this).val(0);
        }
        updatestoprice();
    });

$("#purchasetbody3").delegate(".Discount",
    "keyup",
    function () {
        if ($(this).val() == '') {
            $(this).val(0);
        }
    });


//更新储值卡价格(实际价格)
function updatestoprice() {
    var list = $("#purchasetbody3").find(".Price");
    var list1 = $("#purchasetbody3").find(".Discount") 
    var allprice = 0;
    let arr1 = [];
    let arr2 = [];

    list.each(function (i) {
        arr1.push(parseFloat($(this).val()))
       // allprice += parseInt($(this).val());
        //console.log(list1.eq(i).val())
        //allprice += (parseFloat($(this).val()) * (+list1.eq(i).val()));
    });
    list1.each(function (){
        arr2.push($(this).val())

    })

    console.log(arr1, arr2)

    for (let i = 0; i < list1.length; i++) {
        allprice += (arr1[i]*arr2[i])
    }
    allprice = allprice.toFixed(2)
    $("#labamout0").text(allprice);

    //-------------上面更新的是实际价格,下面更新的是总价格------------

    //var alllist = $("#purchasetbody3").find("input[name='Price']");
    //var sumprice = 0;
    //alllist.each(function (i) {
    //    sumprice += parseInt($(this).val());
    //});
    //$("#labcash0").text(sumprice);
    $("#labcash0").text(allprice);
}





var isshowdoc4 = false; //美容师下拉框显示控制
$("#purchasetable3").delegate(".liMenu", "click", function () {
    var curent = $(this).parent().parent().parent().find(".liHide");
    $(".liHide").slideUp("slow");
    if (isshowdoc4 == false) {
        curent.slideDown("fast");
        //curent.css("display", "block");
        isshowdoc4 = true;
    } else {
        curent.slideUp("slow");
        isshowdoc4 = false;
    }

});


//删除当前卡项
$("#purchasetable3").delegate(".delme", "click", function () {
    $(this).parent().parent().remove();
    updatestoprice();
});

//提交操作
$("#btnsavesto").click(function () {
    if ($("#labuserid").val() == "0") {
        artdialogfailTwo("请选择客户！", "", "250", "100");
        return false;
    }
    var isok = true;
    var userid = $("#labuserid").val();
    var txtmemo = $("#txtmemo1").val();
    var checedbox = $("#purchasetbody3").find("tr");
    if (checedbox.length == 0) {
        artdialogfailTwo("请选择卡项！", "", "250", "100");
        return;
    }
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
    var discountlist = $(".Discount"); //获取划扣折率列表
    var UserIDList = ""; //参与员工集合
    var disList = new Array(); //划扣折率集合
    var canUserIDList = new Array(); //参与员工集合

    var newuseridlist = $("#selaccountdiv1").find("input[name='UserID']:checked");

    if (newuseridlist.length == 0) { isok = false; artdialogfailTwo("请选择参与员工！", "", "250", "100"); return false; } else { isok = true; }
    newuseridlist.each(function (j) {
        canUserIDList[j] = $(this).val();
    });
    UserIDList = canUserIDList.toString() + "$";

    if (canUserIDList.toString() == null || canUserIDList.toString() == "") {
        isok = false;
    }
    discountlist.each(function (i) {
        disList[i] = $(this).val();
    });

    if (!isok) {
        artdialogfailTwo("请选择参与员工！", "", "250", "100");
        return false;
    }

    $.ajax({
        type: "POST",
        url: encodeURI(getControllerUrl("CashManage", "AddCard1?uslist=" + UserIDList.toString() + "&userid=" + userid.toString() + "&Name=" + txtmemo + "&chkcreatetime=" + chkcreatetime + "&txtCreateTime=" + txtCreateTime)),
        async: false,
        dataType: "json",
        data: $('#myform2').serialize(), // 你的formid
        error: function (request) {
            artdialogfailTwo("操作失败！", "", "250", "100");
        },
        success: function (res) {
            if (res != null) {
                var param = "?BuyType=2&cashamout=" + res.cashamout + "&cardamout=" + res.cardamout + "&allamout=" + res.allamout + "&orderNo=" + res.orderNo + "&UserID=" + res.UserID + "&CardIDstr=" + res.CardIDstr + "&ordertimesstr=" + res.ordertimesstr + "&maika=" + res.maika + "&recharge=" + res.IsRecharge + "&discountlist=" + disList.toString() + "&txtCreateTime=" + txtCreateTime;

                art.dialog.open("/CashManage/PayOrderPagePuchanseCard" + param,
                    { title: "结账付款", lock: true, opacity: 0.05, width: 750, height: 450 });

            } else {
                artdialogfailTwo("操作失败,！", "", "250", "100");
            }
        }
    });
})

/*******************储值卡end*************************/
function IsselectUser() {
    return parseInt($("#labuserid").val()) > 0;
}