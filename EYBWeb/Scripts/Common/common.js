//获取网站的根目录
function getRootPath() {
    var pathName = window.parent.location.pathname.substring(1);
    var webName = pathName.indexOf('/') > 0 ? pathName.substring(0, pathName.indexOf('/')) : pathName;
    if (webName == "") {
        return window.location.protocol + '//' + window.location.host + '/';
    }
    // return window.location.protocol + '//' + window.location.host + '/' + webName + '/';
    return window.location.protocol + '//' + window.location.host + '/';
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
//只能输入浮点或整数
function CheckInputIntFloat(oInput) {
    if ('' != oInput.value.replace(/\d{1,}\.{0,1}\d{0,}/, '')) {
        oInput.value = oInput.value.match(/\d{1,}\.{0,1}\d{0,}/) == null ? '' : oInput.value.match(/\d{1,}\.{0,1}\d{0,}/);
    }
}
//只能输入浮点数,如果不是返回0
function CheckInputIntFloatReturn0(oInput) {
    if ('' != oInput.value.replace(/\d{1,}\.{0,1}\d{0,}/, '')) {
        oInput.value = oInput.value.match(/\d{1,}\.{0,1}\d{0,}/) == null ? '0' : oInput.value.match(/\d{1,}\.{0,1}\d{0,}/);
    }
}

//获取完整路径：controller, methodName一定要输入
function getControllerUrl(controller, methodName) {
    if (typeof (controller) == "undefined") {
        alert("请输入控制器名称");
        return "";
    }

    if (typeof (methodName) == "undefined") {
        alert("请输入调用的目标");
        return "";
    }
    return getRootPath() + controller + "/" + methodName;
}
//跳转页面
function JumpUrl(url) {
    location.href = url;
}

//窗体自适应大小
function autoResize(options) {
    var size = getGridWidthAndHeigh();
    if ($.isFunction(options.callback)) {
        options.callback(size);
    }
    // 窗口大小改变的时候
    window.onresize = function () {
        var size = getGridWidthAndHeigh(true);
        $(options.dataGrid).jqGrid('setGridHeight', size.height).jqGrid('setGridWidth', size.width);
    };
}
//获取jqgrid宽度和高度
function getGridWidthAndHeigh(resize) {
    return GetJqGridHeight(resize);
}

//获取文档可见区域的宽度
function getDocumentWidth() {
    return document.documentElement.clientWidth;
}

//获取文档可见区域的高度
function getDocumentHeight() {
    return document.documentElement.clientHeight;
}
//获取表格的高度
function GetJqGridHeight(height) {
    var docheight = getDocumentHeight();
    var headheight = 0;
    if ($(".pageHeader").length != 0) {//存在pageHeader
        headheight = $(".pageHeader").height();
    }
    var panelBarheight = 0;
    if ($(".panelBar").length != 0) {//存在pageHeader
        panelBarheight = $(".panelBar").height();
    }
    return (docheight - headheight - panelBarheight - 60);
    //    if (isNaN(height))
    //        return (docheight - headheight - 20);
    //    else
    //        return (docheight - headheight - height);
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
    console.log("11111000000")
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


//清除HTML标记
function ClearHtml(str) {
    return str.replace(/<[^>]+>/g, "");
}

//转换时间格式
function formatDateTime(value) {
    var d = new Date(+/\d+/g.exec(value));
    return d.toLocaleString();
}

//转换时间格式
function formatDate(value) {
    var d = new Date(+/\d+/g.exec(value));
    return d.toLocaleString();
}

//转换时间格式
function formatShortDate(value) {
    var d = new Date(value);
    return d.toLocaleString();
}
//打开新tab
function openTab(url, title) {
    navTab.openTab("tab" + title, url, { title: title, fresh: true, data: {} });
}

//打开新Iframetab
function openIframeTab(url, title) {
    navTab.openTab("tab" + title, url, { title: title, fresh: true, data: {}, external: true });
}
//验证数据
function CheckData() {
    var flag = true;
    var $requireds = $(".required");
    var html;
    for (var i = 0; i < $requireds.length; i++) {
        if ($requireds.eq(i).val() == "" && $requireds.eq(i).offset().top > 0) {
            flag = false;
            if ($requireds.eq(i).next(".error").length != 0)
                continue;
            html = '<span title="必填字段" class="error">必填字段</span>';
            $requireds.eq(i).after(html);
        } else {
            $requireds.eq(i).next(".error").remove();
        }
    }

    var $mail = $(".email");
    for (var i = 0; i < $mail.length; i++) {
        if ($requireds.eq(i).offset().top > 0 && $mail.eq(i).val() != "" && $mail.eq(i).val().match(/^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/) == null) {
            flag = false;
            if ($requireds.eq(i).next(".error").length != 0)
                continue;
            html = '<span title="请输入合法的Email" class="error">请输入合法的Email</span>';
            $mail.eq(i).after(html);
        } else {
            $mail.eq(i).next(".error").remove();
        }
    }

    var $phone = $(".phone");
    for (var i = 0; i < $phone.length; i++) {
        if ($requireds.eq(i).offset().top > 0 && $phone.eq(i).val() != "" && $phone.eq(i).val().match(/\d{3}-\d{8}|\d{4}-\d{7}/) == null) {
            flag = false;
            if ($requireds.eq(i).next(".error").length != 0)
                continue;
            html = '<span title="请输入合法的电话" class="error">请输入合法的电话</span>';
            $phone.eq(i).after(html);
        } else {
            $phone.eq(i).next(".error").remove();
        }
    }

    var $number = $(".number");
    for (var i = 0; i < $number.length; i++) {
        if ($requireds.eq(i).offset().top > 0 && isNaN($number.eq(i).val())) {
            flag = false;
            if ($requireds.eq(i).next(".error").length != 0)
                continue;
            html = '<span title="请输入合法的数字" class="error">请输入合法的数字</span>';
            $number.eq(i).after(html);

        } else {
            $number.eq(i).next(".error").remove();
        }
    }

    var $url = $(".url");
    for (var i = 0; i < $url.length; i++) {
        if ($requireds.eq(i).offset().top > 0 && $url.eq(i).val() != "" && $url.eq(i).val().match('[a-zA-z]+://[^\s]*') == null) {
            flag = false;
            if ($requireds.eq(i).next(".error").length != 0)
                continue;
            html = '<span title="请输入合法的Url" class="error">请输入合法的Url</span>';
            $url.eq(i).after(html);

        } else {
            $url.eq(i).next(".error").remove();
        }
    }

    var $lettersonly = $(".lettersonly");
    for (var i = 0; i < $lettersonly.length; i++) {
        if ($requireds.eq(i).offset().top > 0 && $lettersonly.eq(i).val() != "" && $lettersonly.eq(i).val().match(/^[a-zA-Z]+$/) == null) {
            flag = false;
            if ($requireds.eq(i).next(".error").length != 0)
                continue;
            html = '<span title="请输入合法的字母" class="error">请输入合法的字母</span>';
            $lettersonly.eq(i).after(html);

        } else {
            $lettersonly.eq(i).next(".error").remove();
        }
    }

    var $creditcard = $(".lettersonly");
    for (var i = 0; i < $creditcard.length; i++) {
        if ($requireds.eq(i).offset().top > 0 && $creditcard.eq(i).val() != "" || isNaN($creditcard.eq(i).val()) || $creditcard.eq(i).val().length != 19) {
            flag = false;
            if ($requireds.eq(i).next(".error").length != 0)
                continue;
            html = '<span title="请输入合法的银行帐号" class="error">请输入合法的银行帐号</span>';
            $creditcard.eq(i).after(html);

        } else {
            $creditcard.eq(i).next(".error").remove();
        }
    }
    if (!flag)
        alertMsg.warn("请您输入合法的数据！");
    return flag;
}




/**
* 全选checkbox,注意：标识checkbox id固定为为check_box
* @param string name 列表check名称,如 uid[]
*/
function selectall(name) {
    if ($("#check_box").prop("checked") == false) {

        $("input[name='" + name + "']").each(function () {
            this.checked = false;
        });
    } else {

        $("input[name='" + name + "']").each(function () {
            this.checked = true;
        });
    }
}

//检测邮箱格式
function is_email(str) {
    var reg = /^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/;
    return reg.test(str);
}
//设为首页
function SetHomePage(obj, url) {
    try {
        obj.style.behavior = 'url(#default#homepage)'; obj.setHomePage(url);
    } catch (e) {
        if (window.netscape) {
            try {
                netscape.security.PrivilegeManager.enablePrivilege("UniversalXPConnect");
            } catch (e) {
                return false;
            }
            var prefs = Components.classes['@mozilla.org/preferences-service;1'].getService(Components.interfaces.nsIPrefBranch);
            prefs.setCharPref('browser.startup.homepage', url);
        }
    }
}
//加入收藏
function AddFavorite(url, title) {
    try {
        window.external.addFavorite(url, title);
    } catch (e) {
        try {
            window.sidebar.addPanel(title, url, "");
        } catch (e) {
            return false;
        }
    }
}
/* 火狐下取本地全路径 */
function getFullPath(obj) {
    if (obj) {
        //ie
        if (window.navigator.userAgent.indexOf("MSIE") >= 1) {
            obj.select();
            return document.selection.createRange().text;
        }
        //firefox
        else if (window.navigator.userAgent.indexOf("Firefox") >= 1) {
            if (obj.files) {
                return obj.files.item(0).getAsDataURL();
            }
            return obj.value;
        }
        return obj.value;
    }
}

function redirect(url) {
    location.href = url;
}

function formatDateTime(value) {
    if (value == "/Date(-62135596800000)/") {
        return "";
    }
    var d = new Date(+/\d+/g.exec(value));
    return d.format("yyyy-MM-dd hh:mm:ss");
}
Date.prototype.format = function (format) {
    var o =
    {
        "M+": this.getMonth() + 1, //month
        "d+": this.getDate(),    //day
        "h+": this.getHours(),   //hour
        "m+": this.getMinutes(), //minute
        "s+": this.getSeconds(), //cond
        "q+": Math.floor((this.getMonth() + 3) / 3),  //quarter
        "S": this.getMilliseconds() //millisecond
    }
    if (/(y+)/.test(format))
        format = format.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(format))
            format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
    return format;
}

function show(tag, obj) {
    var light = document.getElementById(tag);
    var fade = document.getElementById('fade');
    light.style.display = 'block';
    fade.style.display = 'block';

    var posX;
    var posY;
    document.getElementById(obj).onmousedown = function (e) {
        if (!e) e = window.event;  //IE
        posX = e.clientX - parseInt(light.style.left);
        posY = e.clientY - parseInt(light.style.top);
        document.onmousemove = mousemove;
    }
    document.onmouseup = function () {
        document.onmousemove = null;
    }
    function mousemove(ev) {
        if (ev == null) {
            ev = window.event; //IE
        }
        light.style.left = (ev.clientX - posX) + "px";
        light.style.top = (ev.clientY - posY) + "px";
    }

}
function hide(tag) {
    var light = document.getElementById(tag);
    var fade = document.getElementById('fade');
    light.style.display = 'none';
    fade.style.display = 'none';
}



//禁止选择一级分类
function check_cate(obj) {
    var level = parseInt($("option:selected", $(obj)).attr('level'));
    var pid = parseInt($("option:selected", $(obj)).attr('pid'));
    if (pid == 0 || level == 0 || level == 1) {
        alert("一级、二级分类禁止选择!");
        $('option[value="0"]', $(obj)).attr('selected', 'selected');
    }
}
// DIV切换
$(function () {
    $(".tabhead").click(function () {
        $(this).parent().find('.currtabhead').removeClass("currtabhead");
        $(".currtabcontent").removeClass("currtabcontent");
        $(this).addClass("currtabhead");
        $(".tabcontent").eq($(this).index()).addClass("currtabcontent");
    });
    $('.tabhead').eq(0).addClass("currtabhead");
    $('.tabcontent').eq(0).addClass("currtabcontent");
});
//Tab控制函数
function tabsChange(tabObj) {
    var tabNum = $(tabObj).parent().index()
    //设置点击后的切换样式
    $(tabObj).parent().parent().find("li a").removeClass("selected");
    $(tabObj).addClass("selected");
    //根据参数决定显示内容
    $(".tab-content").hide();
    $(".tab-content").eq(tabNum).show();
}


function GoTop() {
    if (document.all) {
        history.go(-1);
    } else {
        history.go(-1);
    }
}


//加载其他页面共享元件(用户输入账户信息时,有大量时间停留在页面上,利用此段时间可以后台加载好其他页面所用js,样式,图片等元件)
function LoadSharePageItems() {
    setTimeout(function () {
        _first_load_page_tag = false;
        var frame = document.createElement("iframe");
        frame.src = "sharepageitems.html";
        document.getElementById("spanShareEntity").appendChild(frame);
    }, 1000);
}

//弹出成功框
function artdialog(mess, url, width, height) {
    if (width == "" || width == undefined) {
        width = "250";
    }
    if (height == "" || height == undefined) {
        height = "50";
    }
    if (url == "" || url == undefined) {

        art.dialog({ lock: true, background: '#000',opacity: 0.07, content: "<img style='float:left;margin-top:20px;' src='../../Content/themes/images/success.png' width='75' height='65' />" + "<div style='float:left;margin-top:30px; text-align:left;width:120px; min-width:100px; height:50px;line-height:20px;'>" + mess + "</div>", ok: function () { return true; }, cancelVal: '取 消', width: width + 'px', height: height + 'px', cancel: true
        });
    } else {
        art.dialog( {lock: true,background: '#000',opacity: 0.07, content: "<img style='float:left;margin-top:20px;' src='../../Content/themes/images/success.png' width='75' height='65' />" + "<div style='float:left;margin-top:30px; text-align:left;width:120px; min-width:100px; height:50px;line-height:20px;'>" + mess + "</div>", ok: function () { return true; }, cancelVal: '取 消', width: width + 'px', height: height + 'px' }, function () { location.href = url; });
    }
}

//弹出失败框
function artdialogfail(mess, url, width, height) {
    if (width == "" || width == undefined) {
        width = "250";
    }
    if (height == "" || height == undefined) {
        height = "50";
    }
    if (url == "" || url == undefined) {

        art.dialog({ content: "<img style='float:left;margin-top:20px;' src='../../Content/themes/images/fail.png' width='75' height='65' />" + "<div style='float:left;margin-top:30px; text-align:left;width:120px; min-width:100px; height:50px;line-height:20px;'>" + mess + "</div>", ok: function () { return true; }, cancelVal: '取 消', width: width + 'px', height: height + 'px', cancel: true });
    } else {
        art.dialog({ content: "<img style='float:left;margin-top:20px;' src='../../Content/themes/images/fail.png' width='75' height='65' />" + "<div style='float:left;margin-top:30px; text-align:left;width:120px; min-width:100px; height:50px;line-height:20px;'>" + mess + "</div>", ok: function () { return true; }, cancelVal: '取 消', width: width + 'px', height: height + 'px' }, function () { location.href = url; });
    }
}

//弹出失败框
function artdialogfailTwo(mess, url, width, height) {
    if (width == "" || width == undefined) {
        width = "250";
    }
    if (height == "" || height == undefined) {
        height = "50";
    }
    if (url == "" || url == undefined) {

        art.dialog({ content: "<img style='float:left;margin-top:20px;' src='../../Content/themes/images/fail.png' width='75' height='65' />" + "<div style='float:left;margin-top:30px; text-align:left;width:150px; min-width:130px; height:50px;line-height:20px;'>" + mess + "</div>", ok: function () { return true; }, cancelVal: '取 消', width: width + 'px', height: height + 'px', cancel: true });
    } else {
        art.dialog({ content: "<img style='float:left;margin-top:20px;' src='../../Content/themes/images/fail.png' width='75' height='65' />" + "<div style='float:left;margin-top:30px; text-align:left;width:150px; min-width:130px; height:50px;line-height:20px;'>" + mess + "</div>", ok: function () { return true; }, cancelVal: '取 消', width: width + 'px', height: height + 'px' }, function () { location.href = url; });
    }
}

//打开顾客列表
function PatientList(obj) {
    art.dialog.open("/ReservationDoctorManage/ReservationPatientList?type=" + obj, { title: "顾客列表", width: 660, height: 470 });
}
//关联员工
//function PatientList(obj) {
//    art.dialog.open("/ReservationDoctorManage/ReservationPatientList?type=" + obj, { title: "员工列表", width: 660, height: 470 });
//}
//打开订单详情
function OpenOrderDetail(orderNo) {

    art.dialog.open("/CashManage/OrderDetail?orderNo=" + orderNo, { title: "订单详情", width: 860, height: 500 });
}
//打开订单详情
function OpenOrderDetail1(userid,orderNo) {
    art.dialog.open("/CashManage/OrderDetail?UserID=" + userid + "&orderNo=" + orderNo, { title: "订单详情", width: 860, height: 500 });
}
//打开顾客详情1
function OpenPatientDetail(obj, type) {
    art.dialog.open("/PatientManage/PatientDetailControl?type=" + type + "&UserID=" + obj, { title: "顾客详情", width: 960, height: 640 });
}
////打开收银管理的订单详情
//function OpenCashOrderDetail(orderNo) {

//    art.dialog.open("/CashManage/OrderDetail?orderNo=" + orderNo, { title: "订单详情", width: 860, height: 500 });
//}
//进入全屏
function requestFullScreen() {
    var de = document.documentElement;
    if (de.requestFullscreen) {
        de.requestFullscreen();
    } else if (de.mozRequestFullScreen) {
        de.mozRequestFullScreen();
    } else if (de.webkitRequestFullScreen) {
        de.webkitRequestFullScreen();
    }
}

//退出全屏
function exitFullscreen() {
    var de = document;
    if (de.exitFullscreen) {
        de.exitFullscreen();
    } else if (de.mozCancelFullScreen) {
        de.mozCancelFullScreen();
    } else if (de.webkitCancelFullScreen) {
        de.webkitCancelFullScreen();
    }
}

////检测是否有操作权限
//function CheckRight(obj, curentright) {
//    var url = $(obj).attr("redirect");
//    if (isadmin != 1) {
//        if (curentright != "False") {
//          $(obj).find("a").attr("href",url);
//        } else {
//            artdialogfail("无此操作权限！");
//        }
//    } else {
//        $(obj).find("a").attr("href", url);
//    }
//}
//指定日期加天数
function DateAdd(interval, number, date) {
    switch (interval) {
    case "y":
    {
        date.setFullYear(date.getFullYear() + number);
        return date;
        break;
    }
    case "q":
    {
        date.setMonth(date.getMonth() + number * 3);
        return date;
        break;
    }
    case "m":
    {
        date.setMonth(date.getMonth() + number);
        return date;
        break;
    }
    case "w":
        {

        date.setDate(date.getDate() + number * 7);
        return date;
        break;
    }
    case "d":
    {
        date.setDate(date.getDate() + number);
        return date;
        break;
    }
    case "h":
    {
        date.setHours(date.getHours() + number);
        return date;
        break;
    }
    case "m":
    {
        date.setMinutes(date.getMinutes() + number);
        return date;
        break;
    }
    case "s":
    {
        date.setSeconds(date.getSeconds() + number);
        return date;
        break;
    }
    default:
    {
        date.setDate(d.getDate() + number);
        return date;
        break;
    }
    }
}
//时间戳转换方法    date:时间戳数字
function formatDate(date) {
    var date = new Date(date);
    var YY = date.getFullYear() + '-';
    var MM = (date.getMonth() + 1 < 10 ? '0' + (date.getMonth() + 1) : date.getMonth() + 1) + '-';
    var DD = (date.getDate() < 10 ? '0' + (date.getDate()) : date.getDate());
    return YY + MM + DD;
}
function formatDateime(date) {
    var date = new Date(date);
    var YY = date.getFullYear() + '-';
    var MM = (date.getMonth() + 1 < 10 ? '0' + (date.getMonth() + 1) : date.getMonth() + 1) + '-';
    var DD = (date.getDate() < 10 ? '0' + (date.getDate()) : date.getDate());
    var hh = (date.getHours() < 10 ? '0' + date.getHours() : date.getHours()) + ':';
    var mm = (date.getMinutes() < 10 ? '0' + date.getMinutes() : date.getMinutes()) + ':';
    var ss = (date.getSeconds() < 10 ? '0' + date.getSeconds() : date.getSeconds());
    return YY + MM + DD + " " + hh + mm + ss;
}
//关闭
function closeed() {
    art.dialog.open.api.close();
    var win = art.dialog.open.origin; //来源页面
    // 如果父页面重载或者关闭其子对话框全部会关闭
    win.location.reload();
}
function closeedNorefresh() {
    art.dialog.open.api.close();
}

//关闭页面并且删除单据
function closeedNorefreshAndClearOrder(obj) {
    $.ajax({
        type: "post", //使用get方法访问后台
        dataType: "text", //返回json格式的数据
       // url: getControllerUrl("CashManage", "DeleteOrder"),
        url:"/CashManage/DeleteOrder",
        data: { "OrderNo": obj, "Statu": 3 },
        async: false,
        //url: getControllerUrl("CashManage", "DeleteOrder?Statu=0&OrderNo=" + obj),
        success: function (data) {
            if (data > 0) {
                art.dialog.open.api.close();
            }
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });

    art.dialog.open.api.close();
}

//全角转为半角
String.prototype.dbc2sbc = function () {
    return this.replace(/[\uff01-\uff5e]/g, function (a) { return String.fromCharCode(a.charCodeAt(0) - 65248); }).replace(/\u3000/g, " ");
}

//全角转半角
function SBC(text) {
    return text.replace(/[\x20-\x7e]/g, function ($0) {
        return $0 == "" ? "\u3000" : String.fromCharCode($0.charCodeAt(0) + 0xfee0);
    });
}

//禁止F12键
window.onload = function () {
    //document.onkeydown = function (event) {
    //    var ev = event || window.event;
    //    if (ev.keyCode == 123) {
    //        return false; //123 代表F12键位
    //    }
    //}
}