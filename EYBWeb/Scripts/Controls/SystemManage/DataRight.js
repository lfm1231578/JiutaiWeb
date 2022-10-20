var userids;

$(function () {

    $("#dialogarea").dialog({
        modal: true,
        autoOpen: false,
        height: 400,
        buttons: {
            "确定": function () {
                var items = $("#areatree").getTSVs();
                $('#txtArea').val("总选择了" + items.length + "个地区").attr("areacode", items.toString());

                var roleid = userids;
                var area = $('#txtArea').attr('areacode');
                $.ajax({
                    type: "post",
                    dataType: "json",
                    url: getControllerUrl("SystemManagement", "SetDataRight"),
                    data: { "areacode": area, roleid: roleid },
                    success: function (data) {
                        if (data == null || !data.IsSuccess) {
                            alertMsg.warn("您本次授权操作失败！");
                        } else {
                            alert("您本次授权操作成功！");
                            $(this).dialog("close");

                        }
                    },
                    error: function () {
                        alertMsg.warn("对不起！系统出错啦，我们会尽快跟踪处理的！");
                    }
                });
                $(this).dialog("close");

            },
            "取消": function () {
                $(this).dialog("close");
            }
        }
    });

    $('#btnlinkUserAreaPermit').click(function () {
        var selectUnit = $(':checked', '#tableUserManage');
        if (selectUnit.size() == 0) {
            confirm('请选择要设置的用户！');
            return;
        } else if (selectUnit.size() > 1) {
            alert('只能选择一个用户！');
            return;
        }
        if (selectUnit.size() == 1) {
            userids = jQuery("#tableUserManage").jqGrid('getGridParam', 'selrow');
        }

        loadDataRight(userids);
       
      

        $("#dialogarea").dialog('open');
    });

    $("#selrole").change(function () {
        loadDataRight();
    });

    $('#btnSave').click(function () {
        var roleid = userids;
        var area = $('#txtArea').attr('areacode');
        $.ajax({
            type: "post",
            dataType: "json",
            url: getControllerUrl("SystemManagement", "SetDataRight"),
            data: { "areacode": area, roleid: roleid },
            success: function (data) {
                if (data == null || !data.IsSuccess) {
                    alertMsg.warn("您本次授权操作失败！");
                } else {
                    alertMsg.warn("您本次授权操作成功！");
                }
            },
            error: function () {
                alertMsg.warn("对不起！系统出错啦，我们会尽快跟踪处理的！");
            }
        });
    });

    $("#txtArea").click(function () {
        $('#dialogarea').dialog("open");
    });
});

function loadDataRight(userids) {
    var roleid = userids;
    $.ajax({
        type: "post",
        dataType: "json",
        url: getControllerUrl("SystemManagement", "GetDataRight"),
        data: { roleId: roleid },
        success: function (data) {
            loadTree(data);
        },
        error: function () {
            alertMsg.warn("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });
}

function loadTree(data) {
   
    var o = { showcheck: true };
    o.data = data;
    $("#areatree").treeview(o);
    $(".bbit-tree-elbow-minus").click();
    $("#areatree_54").find(".bbit-tree-elbow-end-minus").click();
}
