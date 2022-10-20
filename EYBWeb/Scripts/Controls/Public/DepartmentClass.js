$(function () {

    $("#dialogareaDepartment").dialog({
        modal: true,
        autoOpen: false,
        height: 400,
        buttons: {
            "确定": function () {
                var items = $("#treeContentArea").getTSNs();
                if (typeof (items) == "undefined" || items.length == 0) {
                    alert('请选择部门！');
                    return;
                }
                if (items.length > 1) {
                    alert("只能选择一个部门！");
                    return;
                }
                var text = "";
                var vals = "";
                for (var i = 0; i < items.length; i++) {
                    text += items[i].text;
                    vals += items[i].value;
                }
                currentdepartment.val(text).attr("areacode", vals);
                $("#hiddendepartment").val(vals);

                $(this).dialog("close");
            },
            "取消": function () {
                $(this).dialog("close");
            }
        }
    });
    //选择部门
    $(".department").click(function () {

        currentdepartment = $(this);
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