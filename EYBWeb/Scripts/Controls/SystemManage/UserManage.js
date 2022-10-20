var BindUserID;
var getUserListURL = getControllerUrl("SystemManagement", "GetUserList");
var addUserURL = getControllerUrl("SystemManagement", "AddUser");
var deleteUserURL = getControllerUrl("SystemManagement", "DeleteUser");
var resetPwdUrl = getControllerUrl("SystemManagement", "ResetPassword");
var ActiveUrl = getControllerUrl("SystemManagement", "ActivityUser"); //启用禁用

//---------------------------初始化Ajax请求，查询用户列表-------------------------------------------------

//修改用户
function edit(id) {
    window.top.art.dialog({ id: 'edit' }).close();
    window.top.art.dialog({ title: "编辑", id: 'edit', iframe: getControllerUrl("SystemManagement", "UserManageUpdate") + '?id=' + id, width: '460', height: '420' ,show:true}, function () { var d = window.top.art.dialog({ id: 'edit' }).data.iframe; d.document.getElementById('dosubmit').click(); return false; }, function () { window.top.art.dialog({ id: 'edit' }).close() });
}


//启用禁用用户
function DisableAndActive(id, status) {
    if (!confirm("确认操作吗？")) {
        return;
    }
    if (status == 0) {
        status = 1;
    } else {
        status = 0;
    }
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "json", //返回json格式的数据
        url: ActiveUrl,
        data: { status: status, userids: id },
        success: function (data) {
            if (data) {
                alert("操作成功！");
            }
            location.reload();
        },
        error: function (responseText) {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}
function ResetPassword(id) {
    //重置密码

    if (!confirm("确认操作吗？")) {
        return;
    }

    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "json", //返回json格式的数据
        url: resetPwdUrl,
        data: { userid: parseInt(id) },
        success: function (data) {
            if (data) {
                alert("操作成功！");
            }
            location.reload();

        },
        error: function (responseText) {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}

//删除用户
function DeleteUser(id) {
    if (!confirm("确认操作吗？")) {
        return;
    }
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "json", //返回json格式的数据
        url: deleteUserURL,
        data: { userids: id },
        success: function (data) {
            if (data) {
                alert("操作成功！");
            }
            location.reload();

        },
        error: function (responseText) {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}
