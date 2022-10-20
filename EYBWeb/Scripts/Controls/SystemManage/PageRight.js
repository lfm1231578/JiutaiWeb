var roleIDs;
$(function () {

    $('#btnlinkRoleMemuPermit').click(function () {
        var selectUnit = $(':checked', '#tableRoleManage');
        if (selectUnit.size() == 0) {
            confirm('请选择要设置的角色！');
            return;
        } else if (selectUnit.size() > 1) {
            confirm('只能选择一个角色！');
            return;
        }
        if (selectUnit.size() == 1) {
            roleIDs = jQuery("#tableRoleManage").jqGrid('getGridParam', 'selrow');
            initTree(roleIDs);
        }
        $("#dialogareaRole").dialog('open');
    });

    $("#seloperatType").change(function () {
        initTree();
    });

    $("#selrole").change(function () {
        initTree();
    });

    $("#dialogareaRole").dialog({
        modal: true,
        autoOpen: false,
        height: 400,
        buttons: {
            "确定": function () {
                var roleid = roleIDs;
                var menuid = $("#areatreeRole").getCkOperat();
                $.ajax({
                    type: "post",
                    dataType: "json",
                    url: getControllerUrl("SystemManagement", "SaveRight"),
                    data: { "menuId": menuid.toString(), "roleId": roleid },
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

function initTree(roleIDs) {
    var roleId = roleIDs;
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "json", //返回json格式的数据
        data: { roleId: roleId },
        url: getControllerUrl("SystemManagement", "GetMenuTree"),
        success: function (data) {
            load(data);
        },
        error: function () {
            alert("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}

function load(data) {
    var o = { showcheck: false };
    o.data = data;
    $("#areatreeRole").treeview(o);
    $(".bbit-tree-elbow-minus").click();
    $("#areatreeRole_43").find(".bbit-tree-elbow-end-minus").click();
    
}
