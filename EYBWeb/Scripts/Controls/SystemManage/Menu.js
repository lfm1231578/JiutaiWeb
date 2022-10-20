$(function () {
    $('#menumanage').height($(document).height());
    loadTree();
    $('#btnSave').click(function () {
        if (!validity())
            return;
        var title = $('#txtTitle').val();
        var url = $('#txtUrl').val();
        var className = $('#txtClass').val();
        var funMemo = $("#txtFunMemo").val();
        var opertMemo = $("#txtOpertMemo").val();
        var id = $(this).attr("menuid");
        var optype = $('#selType').val();
        var parentId = $(this).attr("parentmenuid");
        var btncode = $("#txtCode").val();
        var ctype = $('#radmenu').attr("checked") ? 1 : 3;
        $.ajax({
            type: "post", //使用get方法访问后台 
            dataType: "json", //返回json格式的数据
            url: getControllerUrl("SystemManagement", "AddUpdateDeleteMenu"),
            data: { "id": id, "parentId": parentId, "optype": optype, "ctype": ctype, "title": title, "url": url, "className": className, "btnCode": btncode, funMemo: funMemo, opertMemo: opertMemo },
            success: function (data) {
                if (data == null || data.IsSuccess == false)
                    alertMsg.warn("您的当前操作未成功，请重新操作！");
                else {
                    alertMsg.warn("操作成功！");
                    loadTree();
                }
            },
            error: function () {
                alertMsg.warn("对不起！系统出错啦，我们会尽快跟踪处理的！");
            }
        });
    });

    $('#radbutton,#radmenu').click(function () {
        if ($(this).attr("checked") && $(this).attr("id") == "radbutton") {
            $('#dlbutton').show();
        } else {
            $('#dlbutton').hide();
        }
    });

});

function loadTree() {
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "json", //返回json格式的数据
        url: getControllerUrl("SystemManagement", "GetAllMenuForTree"),
        data: { opertId: 0 },
        success: function (treedata) {
            var o = { onnodeclick: function (item) {
                $.ajax({
                    type: "post", //使用get方法访问后台 
                    dataType: "json", //返回json格式的数据
                    url: getControllerUrl("SystemManagement", "GetEntity"),
                    data: { "id": item.id },
                    success: function (data) {
                        if (data != null) {
                            $('#txtTitle').val(data.UIControlName);
                            $('#txtUrl').val(data.Url);
                            $('#txtClass').val(data.ClassName);
                            $("#txtCode").val(data.UiControlCode);
                            $('#btnSave').attr("menuid", data.UIControlID).attr("parentmenuid", data.ParentUIControlID);
                            if (data.UIControlTypeID == 3)
                                $('#radbutton').attr("checked", "checked");
                        } else {
                            alertMsg.warn("系统查询不到数据，请稍后操作！");
                        }
                    },
                    error: function () {
                        alertMsg.warn("对不起！系统出错啦，我们会尽快跟踪处理的！");
                    }
                });
            }
            };
            o.data = treedata;
            $("#tree").treeview(o);
        },
        error: function () {
            alertMsg.warn("对不起！系统出错啦，我们会尽快跟踪处理的！");
        }
    });

}