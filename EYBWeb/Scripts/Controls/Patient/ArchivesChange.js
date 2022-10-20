var isshowdoc = false;
//选择顾客后事件
function InFoDate(obj) {
    var userid = obj.split("|")[0];
    $("#labuserid").val(userid);
    $("#labuserid1").val(userid);
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
                $("#labUserName").attr("onclick", "OpenPatientDetail(" + data.UserID + ",1)");
                // $("#labUserName").attr("href", "/PatientManage/PatientDetail?UserID=" + data.UserID);

                //汪  选择顾客后在选一遍刷新欠费变更（档案变更里面）
                if ($("#qiankuan").attr("class") == "thistab") {
                    var index = $("#tabs  .btab3").parent().find("li").index("#tabs  .btab3");
                    $("#tab_conbox").find(".tab_con").eq(index).show();
                    var iframe1 = $("#tab_conbox").find(".tab_con").eq(index).find("iframe");
                    iframe1.attr("src", iframe1.attr("href") + userid.toString());
                }
                //汪
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

}

//汪
//$("#hidif").delegate()
//汪

//获取已购买疗程卡信息
function GetprojectCardlist(userid) {

    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "text", //返回json格式的数据
        url: getControllerUrl("PatientManage", "GetprojectCardlist"),
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



//获取可以充值的储值卡列表
function GetStoredCardList(id) {
    if (id > 0) {
        $.ajax({
            type: "post", //使用get方法访问后台
            dataType: "text", //返回json格式的数据
            url: getControllerUrl("PatientManage", "ToGetStoredCardList"),
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

//提交操作
$("#btnsave").click(function () {
    var isok = true;
    var checedbox = $("#purchasetbody").find("tr");
    var OrderNoList = new Array(); //订单集合
    var HuankuanList = new Array(); //还款集合
    var UserIDList = new Array(); //参与员工集合
    var canUserIDList = new Array(); //参与员工集合
    checedbox.each(function (i) {
        var newuseridlist = $(this).find("input[name='UserID']:checked");
        newuseridlist.each(function (j) {
            canUserIDList[j] = $(this).val();
        });
        UserIDList[i] = canUserIDList.toString() + "$";
    });
    if (canUserIDList.toString() == null || canUserIDList.toString() == "") {
        isok = false;
    }

    if (!isok) {
        artdialogfailTwo("请选择卡项！", "", "250", "100");
        return false;
    }

    $.ajax({
        type: "POST",
        url: getControllerUrl("CashManage", "AddCard?uslist=" + UserIDList.toString()),
        // data: { "uslist": UserIDList.toString() },
        //url: "@Url.Content("~/CashManage/AddCard?userlist=")"+UserIDList.toString(),
        async: false,
        data: $('#myform').serialize(), // 你的formid
        error: function (request) {
            artdialogfailTwo("操作失败！", "", "250", "100");
        },
        success: function (data) {
            if (data != null) {
                var res = eval("(" + data + ")")
                var param = "?cashamout=" + res.cashamout + "&cardamout=" + res.cardamout + "&allamout=" + res.allamout + "&orderNo=" + res.orderNo + "&UserID=" + res.UserID + "&CardIDstr=" + res.CardIDstr + "&ordertimesstr=" + res.ordertimesstr + "";
                art.dialog.open("/CashManage/PayOrderPagePuchanseCard" + param, { title: "结账付款", width: 750, height: 430 })

            } else {
                artdialogfailTwo("操作失败,！", "", "250", "100");
            }
        }
    });
});

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
    $(".addtreament").click(function() {
        if (IsselectUser()) {
            art.dialog.open("/BaseInfo/CardList?Type=2", { title: "疗程卡列表", width: 660, height: 470 });
        } else {
            artdialogfailTwo("请选择客户！", "", "250", "100");
            return false;
        }
    });


    //添加储值卡项
    $("#addsto").click(function() {
        if (IsselectUser()) {
            art.dialog.open("/BaseInfo/CardList?Type=1", { title: "储值卡列表", width: 680, height: 470 })
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
        allprice += parseInt($(this).val());
    })
    $("#labamout").text(allprice);
}

//点击编辑时打开选择页面
$("#purchasetbody").delegate(".editproject", "click", function () {

    var thistd = $(this);
    var id = $(this).parent().parent().find("input[name=ID]").val();
    var hidstr = $(this).parent().parent().find(".hidproject").val();
    art.dialog.open("/PatientManage/ProjectEdit?ID=" + id + "&hidstr=" + hidstr, {
        title: "项目列表", width: 660, height: 470, cancel: false, close: function () {
            var bValue = art.dialog.data('OValue'); // 读取B页面的数据
            if (bValue !== undefined) {
                //这里添加一个绑定方法
                projbind(bValue, thistd);
            }
        }
    })
})

function projbind(obj, thethis) {
    thethis.parent().parent().find(".Price").val(obj.split('&')[0]);
    thethis.parent().parent().find(".hidproject").val(obj.split('&')[1])
    priceupdate();
}

//选择卡项确认事件
function GetTemCardinfo(obj) {
    // var s = new Array();
    //s = obj.split("|");
    //alert(obj);
    var arry = obj.split("$");
    var str = "";
    var result = 0;

    var appendUser = $("#appendUser").html(); //获取隐藏的参与员工列表
    //for (var i = 0; i < arry.length - 1; i++) {
    //    $("#purchasetbody").append('<tr><td> <input class="ID" type="hidden" name="ID" value="' + arry[i].split("|")[0] + '" /> <input type="hidden" class="hidproject" value="0" /> <input type="hidden" name="vallist" class="vallist" value="' + arry[i].split("|")[0] + '|' + arry[i].split("|")[2] + '" />' + arry[i].split("|")[1] + ' </td> <td><input type="text" name="Price"  readonly="readonly" class="Price wid50" value="' + arry[i].split("|")[2] + '" /></td> <td style=" display:none;">' + appendUser + '  </td> <td><a href="#" class="editproject">编辑</a> </td>   <td><a class="delthis" href="javascript:;" >删除</a></td>  </tr>');
    //    result += parseFloat(arry[i].split("|")[2]);
    //}
    //根据卡编号 查询项目  AJAX

    for (var i = 0; i < arry.length - 1; i++) {
        //alert(res);
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
                    var newstr = res;
                    $("#purchasetbody").append('<tr><td><input type="hidden" class="ID" name="ID" value="' + arry[i].split("|")[0] + '" /> <input type="hidden" class="hidproject" value="' + newstr + '" /> <input type="hidden" name="vallist" class="vallist" value="' + arry[i].split("|")[0] + '|' + arry[i].split("|")[2] + '" />' + arry[i].split("|")[1] + ' </td> <td><input type="text" name="Price"  readonly="readonly" class="Price wid50" value="' + arry[i].split("|")[2] + '" /></td> <td style=" display:none;">' + appendUser + '  </td> <td><a href="#" class="editproject">编辑</a> </td>   <td><a class="delthis" href="javascript:;" >删除</a></td>  </tr>');
                    result += parseFloat(arry[i].split("|")[2]);
                }
                else {
                    artdialogfailTwo("操作失败！", "", "250", "100");
                }
            }
        });
    }

    var oldprice = $("#labamout").text();
    $("#labamout").text(parseFloat(oldprice) + result);

    $(".accountdiv2").find("input[name='UserID']").each(function() {
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
    var userid = $("#labuserid").val();
    if (userid == "0") {
        artdialogfailTwo("请选择客户！", "", "250", "100");
        return false;
    }

    var checedbox = $("#purchasetbody").find("tr");
    if (checedbox.length == 0) {
        artdialogfailTwo("请选择卡项！", "", "250", "100");
        return;
    }
    var projectList = new Object(); //项目集合
    var plisarray = $("#purchasetbody").find(".hidproject");  //获取隐藏的项目信息
    plisarray.each(function(i) {
        projectList[i] = $(this).val();
    });
    var PriceList = $("#purchasetbody").find(".Price"); //金额
    var pricesList = new Object(); //金额集合
    PriceList.each(function(i) {
        pricesList[i] = $(this).val();
    });
    var idList = $("#purchasetbody").find(".ID"); //卡ID
    var cardidList = new Object(); //卡ID集合
    idList.each(function(i) {
        cardidList[i] = $(this).val();
    });
    var Memo = $("#txtmemo1").val();
    $.ajax({
        type: "POST",
        url: getControllerUrl("PatientManage", "AddTrentCard"),
        async: false,
        data: { "projectList": projectList, "pricesList": pricesList, "userid": userid, "cardidList": cardidList, "Memo": Memo },
        error: function (request) {
            artdialogfailTwo("操作失败！", "", "250", "100");
        },
        success: function (data) {
            if (data > 0) {
                alert("操作成功！");
                InFoDate(userid + "|0");
                $("#purchasetbody").empty();
                updateProjectprice();
            } else {
                artdialogfailTwo("操作失败！", "", "250", "100");
            }
        }
    });
});

$("#myform").delegate('.btneditproject', 'click', function () {
    var objid = $(this).attr("code");
    art.dialog.open("/PatientManage/UpdateProjectBalancePage?ID=" + objid, { title: "变更卡项", width: 400, height: 340 });

});
/****************疗程卡end****************************/




/*******************储值卡*************************/
//选择储值卡确认事件
function GetstoCardinfo(obj) {
    //var s = new Array();
    //  s = obj.split("|");
    var appendUser = $("#appendUser3").html(); //获取隐藏的参与员工列表
    var arry = obj.split("$");
    var str = "";
    var result = 0;
    for (var i = 0; i < arry.length - 1; i++) {
        if (arry[i].split("|")[5] == "2") {//为2时是正常的储值卡,1是赠卡
            str += '<tr id="' + arry[i].split("|")[0] + '"><td> <input type="hidden" name="isgive" value="' + arry[i].split("|")[5] + '" /> <input type="hidden" name="Type" class="Type" value="2" /> <input type="hidden" name="ID" class="ID" value="' + arry[i].split("|")[0] + '" />' + arry[i].split("|")[1] + ' </td>  <td><input type="text" onkeyup="CheckInputIntFloat(this)" name="Price" class="Price wid50" value="' + arry[i].split("|")[2] + '" />   </td> <td><input type="text" onkeyup="CheckInputIntFloat0and1(this)" class="Discount wid50"  name="Discount" value="1" />   </td> <td style=" display:none;">' + appendUser + '</td><td><input type="text" onkeyup="CheckInputIntFloat(this)" name="TPrice" class="TPrice wid50" value="' + arry[i].split("|")[2] + '" />   </td> <td><a href="javascript:;" class="delme" >删除</a> </td> </tr>';
        }
        else {
            str += '<tr id="' + arry[i].split("|")[0] + '"><td><input type="hidden" name="isgive" value="' + arry[i].split("|")[5] + '" /> <input type="hidden" name="Type" class="Type" value="2" /> <input type="hidden" name="ID" class="ID" value="' + arry[i].split("|")[0] + '" />' + arry[i].split("|")[1] + ' </td>  <td><input type="text"  onkeyup="CheckInputIntFloat(this)" name="Price" class="Price wid50" value="' + arry[i].split("|")[2] + '" />   </td> <td><input type="text"  onkeyup="CheckInputIntFloat0and1(this)" class="Discount wid50"  name="Discount" value="1" />   </td> <td><input type="text" onkeyup="CheckInputIntFloat(this)" name="TPrice" class="TPrice wid50" value="' + arry[i].split("|")[2] + '" />   </td>  <td style=" display:none;">' + appendUser + '</td><td><a href="javascript:;" class="delme" >删除</a> </td> </tr>';
        }
        result += parseFloat(arry[i].split("|")[2]);
    }
    $("#purchasetbody3").append(str);
    // var oldprice = $("#labamout0").text();
    // $("#labamout0").text(parseInt(oldprice) + result);
    updatestoprice();
    $(".accountdiv1").find("input[name='UserID']").each(function () {
        $(this).attr("checked", false);
    })


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
    function() {
        var appendUser = $("#appendUser3").html(); //获取隐藏的参与员工列表
        var thisval = $(this); //获取当前对象
        var ischecked = thisval.attr('checked');
        if (ischecked == "checked") {
            var hidval = $(this).parent().find(".val").val(); //获取隐藏值
            var s = new Array();
            s = hidval.split("|");
            //purchasetable3
            $("#purchasetable3").append('<tr id="' +
                s[0] +
                '"><td><input type="hidden" name="isgive" value="2" /> <input type="hidden" name="Type" class="Type" value="1" /> <input type="hidden" name="ID" class="ID" value="' +
                s[0] +
                '" />' +
                s[3] +
                ' </td>  <td><input type="text" name="Price" class="Price wid50" value="0" onkeyup="CheckInputIntFloat(this)" />   </td><td><input type="text" class="wid50 Discount" name="Discount" value="1"   onkeyup="CheckInputIntFloat0and1(this)" /> </td>  <td style=" display:none;">' +
                appendUser +
                '</td><td><a href="javascript:;" class="delme" >删除</a> </td> </tr>');
        } else {
            $("#purchasetable3").find("tr[id='" + thisval.val() + "']").remove();
        }

    });

//修改金额时自动更新价格purchasetbody
$("#purchasetbody3").delegate("input[name='TPrice']", "keyup", function () {
    if ($(this).val() == '') {
        $(this).val(0);
    }
    var allprice = $(this).parent().parent().find("input[name='Price']").val();
    if (parseFloat(allprice) < parseFloat($(this).val())) {
        alert("余额不能比总金额多!");
        $(this).val(parseFloat(allprice));
    }
    updatestoprice();
})

$("#purchasetbody3").delegate(".Discount", "keyup", function () {
    if ($(this).val() == '') {
        $(this).val(0);
    }
})


//更新储值卡价格(实际价格)
function updatestoprice() {
    var list = $("#purchasetbody3").find(".TPrice");
    var allprice = 0;
    list.each(function (i) {
        allprice += parseInt($(this).val());
    })
    $("#labamout0").text(allprice);

    //-------------上面更新的是实际价格,下面更新的是总价格------------

    var alllist = $("#purchasetbody3").find("input[name='TPrice']");
    var sumprice = 0;
    alllist.each(function (i) {
        sumprice += parseInt($(this).val());
    })
    $("#labcash0").text(sumprice);
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
})

//提交操作--添加储值卡
$("#btnsavesto").click(function () {
    var istrue = true;
    var userid = $("#labuserid").val();

    if (userid == "0") {
        artdialogfailTwo("请选择客户！", "", "250", "100");
        return false;
    }


    var checedbox = $("#purchasetbody3").find("tr");
    if (checedbox.length == 0) {
        artdialogfailTwo("请选择卡项！", "", "250", "100");
        return;
    }
    var discountlist = $(".Discount"); //获取划扣折率列表
    var disList = new Object(); //划扣折率集合
    discountlist.each(function (i) {
        disList[i] = $(this).val();
    });
    var PriceList = $("#purchasetbody3").find(".Price"); //金额
    var pricesList = new Object(); //金额集合
    PriceList.each(function (i) {
        pricesList[i] = $(this).val();
    });
    var idList = $("#purchasetbody3").find(".ID"); //卡ID
    var cardidList = new Object(); //卡ID集合
    idList.each(function (i) {
        cardidList[i] = $(this).val();
    });

    var tprice = $("#purchasetbody3").find(".TPrice"); //余额
    var tpricelist = new Object();
    tprice.each(function (i) {
        tpricelist[i] = $(this).val();
        if (parseFloat(pricesList[i]) < parseFloat($(this).val())) {
            alert("储值卡总额必须必余额多!");
            istrue = false;
        }
    });
    var Memo = $("#txtmemo").val();
    if (!istrue) {
        return false;
    }
    if (confirm("确认添加？")) {
        $.ajax({
            type: "POST",
            url: getControllerUrl("PatientManage", "AddCard1"),
            async: false,
            data: { "discountlist": disList, "pricesList": pricesList, "userid": userid, "cardidList": cardidList, "tpricelist": tpricelist, "Memo": Memo }, // 你的formid
            error: function (request) {
                artdialogfailTwo("操作失败！", "", "250", "100");
            },
            success: function (r) {
                if (!r.IsError) {
                    alert("添加成功！");
                    InFoDate(userid + "|0");
                    $("#purchasetbody3").empty();
                    updatestoprice();
                }
                else {
                    artdialogfailTwo(r.Message + "！", "", "250", "100");
                }
            }
        });
    }
});
//编辑储值卡
$("#myform2").delegate('.btnedit', 'click', function () {
    var objid = $(this).attr("code");
    art.dialog.open("/PatientManage/UpdateMainCardBalancePage?ID=" + objid, { title: "变更卡项", width: 400, height: 340 });

});
/*******************储值卡end*************************/
function IsselectUser() {
    return parseInt($("#labuserid").val()) > 0
}
/*----------------欠款---------------------------------------*/
