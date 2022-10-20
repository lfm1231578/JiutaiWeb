$(function () {

    $("#dialogareaDepartment").dialog({
        modal: true,
        autoOpen: false,
        height: 300,
        buttons: {
            "确定": function () {
                var items = $("#treeContentArea").getTSNs();
                if (typeof (items) == "undefined" || items.length == 0) {
                    alert('请选择部门！');
                    return;
                }
                
                var text = "";
                var vals = "";
                for (var i = 0; i < items.length; i++) {
                    text += items[i].text+",";
                    vals += items[i].value+",";
                }
                $("#department").val(text.substring(0, text.length - 1)).attr("areacode", vals.substring(0, vals.length - 1));
                $("#hiddendepartment").val(vals.substring(0,vals.length-1));

                $(this).dialog("close");
            },
            "取消": function () {
                $(this).dialog("close");
            }
        }
    });
    //选择部门
    $("#department").click(function () {
        $("#dialogareaDepartment").dialog('open');
    });
    $.ajax({
        type: "POST",
        dataType: "json",
        url: getControllerUrl("SystemManagement", "GetDepartmentTree"),
        success: function (data) {
            var o = { showcheck: true };
            o.data = data;
            $("#treeContentArea").treeview(o);
        },
        error: function (xhr) {
            alert("出错了！" + xhr.responseText);
        }
    });

});