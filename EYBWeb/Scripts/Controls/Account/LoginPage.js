/* 文 件 名：LoginPage.js
* 功    能：登录页面控制脚本
* 备    注：
* 创建时间：
*/

var LoginURL = getControllerUrl("Account", "Login");
var IndexURL = getControllerUrl("BaseInfo", "TheProject1");
//var IndexURL = getControllerUrl("Home", "Index");
var CheckURL = getControllerUrl("Account", "CheckCode");
//var usbKeyUrl = getControllerUrl("Account", "ValidateUSBKeyPage");

$(function () {
    // SetWindowMax();
    //禁止从index页推到LoginPage页
    window.history.go(1);
    //用户登录业务
    $("#btnLogin").click(function () {
        login();
    });
    $("#txtAccount").focus();
    //登录回车事件
    $(document).keydown(function (event) {
        if (event.keyCode == 13 || event.keyCode == 32) {
            login(); //登录
            event.stopPropagation();
        }
        event.returnValue = false; // 取消此事件的默认操作 
    });

    //刷新验证码点击事件
    $("#imgCheckCode").click(function () {
        var rdm = Math.random();
        //alert(CheckURL);
        $(this).attr("src", CheckURL + '?' + rdm);
    });

});

//登录前检查
function checkLogin() {
    if ($('#txtAccount').val().length == 0) {
        $('#txtAccount').focus();
        return false;
    }
    else if ($('#txtPassword').val().length == 0) {
        $('#txtPassword').focus();
        return false;
    }
    else
        return true;
}

//登录
function login() {
    if (!checkLogin()) {
        alert("请输入完整的登录信息!");
        return;
    }
    var txtAccount = $('#txtAccount').val();
    var txtPassword = $('#txtPassword').val();
    var txtCheckCode = $('#txtCheckCode').val();
    var cookie = 0;
    if($("#texrememberme").prop("checked") == true)
    {
     cookie=1;
    }
    $.ajax({
        type: "POST",
        dataType: "json",
        url: LoginURL,
        data: { "account": txtAccount, "pwd": txtPassword, "checkcode": txtCheckCode, "rememberme": cookie },
        success: function (data) {
            //window.location = IndexURL;
            //return;
       
            if (data == -9) {
                alert("账户到期，请续费，请联系客服充值！ ");
                return;
            }
            if (data == -8) {
                alert("系统故障，数据处理中，敬请谅解！");
                return;
            }
            if (data == -7) {
                alert("账号异常，请不要使用手机端账号登录！");
                return;
            }
            if (data) {
                window.location = IndexURL;
            }
            else {
                alert("账号或密码错误");
                // var rdm = Math.random();
                // $("#imgCheckCode").attr("src", CheckURL + '?' + rdm);
            }
        },
        error: function (xhr) {
            alert(xhr.responseText);
            var rdm = Math.random();
            $("#imgCheckCode").attr("src", CheckURL + '?' + rdm);
            alert("系统正忙，请稍后登录！");
        }
    });
}


function LoadUsb() {
    //判断是否已经安装IA100插件，没安装则显示安装链接
    if (check()) {
        $("#hfIA100").val("Setup");
        $("#hfIA100").hide();
    }
    else {
        $("#hfIA100").val("notSetup");
        $("#hfIA100").show();
    }
}

function resize() {
    //resetHeightAndWidthToBody("tblFullSize");
}