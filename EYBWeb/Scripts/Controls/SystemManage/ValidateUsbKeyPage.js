/* 文 件 名：ValidateUSBKeyPage.js
* 功    能：USBKEY
* 作    者：匡世钢
* 备    注：
* 创建时间：2011-06-07
*/
//****************************************************定义请求路径开始**********************************************//
var LoginPageURL = getControllerUrl("Account", "LoginPage");
var IndexURL = getControllerUrl("Home","Index");
//****************************************************定义请求路径结束**********************************************//
$(function () {
    CheckUSB();//检测USB，注意是放在onload中还是放在jquery加载完中，待定......
    $('#btnBind').click(function () {
        $.ajax({
            url: getFullURL("Account", "BindUSBKEY"),
            type: 'POST',
            dataType: 'json',
            data: 'hfUSBKeyID=' + $("#hfUSBKeyID").val(),
            error: function (e) {
                alert('操作出错！');
            },
            success: function (data) {
                if (data == undefined || data == null || data.IsSuccess == false) {
                    window.location = LoginPageURL;
                    alert(data.Decription); 
                    return;
                }
                window.location = IndexURL; //通过USBKey验证
            }
        });
    });
});

function CheckUSB() {
    if (check()) {
        var Time = document.getElementById("Time");
        Time.innerHTML = Time.innerHTML - 1;
        var result = checkData();
        if (result == "18") {
            if (Time.innerHTML != 0) {
                setTimeout(CheckUSB, 1000);
            }
            else {
                window.location = LoginPageURL;
                alert("未找到USBKey！");
            }
        }
        else if (result.length == 32) {
            var hfID = document.getElementById("hfUSBKeyID");
            hfID.value = result;
            document.getElementById("btnBind").click();
        }
        else {
            window.location = LoginPageURL;
            alert("无法获取USBKey的ID");
        }
    }
}