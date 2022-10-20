var userid;

$(function () {

    $('#btnlinkSetUserRight').click(function () {
        var selectUnit = $(':checked', '#tableUserManage');
        if (selectUnit.size() == 0) {
            alert('请选择要设置的用户！');
            return;
        } else if (selectUnit.size() > 1) {
            alert('只能选择一个用户！');
            return;
        }
        if (selectUnit.size() == 1) {
            userid = jQuery("#tableUserManage").jqGrid('getGridParam', 'selrow');
            initTree(userid);
        }
        $("#dialogUserright").dialog('open');
    });


    $("#dialogUserright").dialog({
        modal: true,
        autoOpen: false,
        height: 400,
        buttons: {
            "确定": function () {
                var menuid = $("#areatreeUserright").getCkOperat();
                $.ajax({
                    type: "post",
                    dataType: "json",
                    url: getControllerUrl("SystemManagement", "SaveUserRight"),
                    data: { "menuId": menuid.toString(), "userid": userid },
                    success: function (d) {
                        if (d == null || d.IsSuccess == false)
                            alert("您的当前操作未成功，请重新操作！");
                        else
                            alert("操作成功！");
                    },
                    error: function () {
                        alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
                    }
                });
                $(this).dialog("close");
            },
            "取消": function () {
                $(this).dialog("close");
            }
        }
    });
});
function SetUserRight(userid) {
    initTree(userid);
    $("#dialogUserright").dialog('open');
}
function initTree(userid) {
    var userid = userid;
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "json", //返回json格式的数据
        data: { userid: userid },
        url: getControllerUrl("SystemManagement", "GetUserMenuTree"),
        success: function (data) {
            var o = { showcheck: false };
            o.data = data;
            $("#areatreeUserright").treeview(o);
            $(".bbit-tree-elbow-minus").click();
            $("#areatreeUserright_43").find(".bbit-tree-elbow-end-minus").click();
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}

