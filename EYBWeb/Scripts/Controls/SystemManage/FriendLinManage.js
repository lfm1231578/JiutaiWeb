var getFriendLinListURL = getControllerUrl("SystemManagement", "GetFriendLinList"); //查询操作
var deleteFriendLinURL = getControllerUrl("SystemManagement", "DelFriendLin"); // 删除

//---------------------------初始化Ajax请求，查询用户列表-------------------------------------------------
var isNumber = /^[A-Za-z0-9]+$/;

$(function () {

    //初始化jqGrid表格事件
    $('#tableFriendLinManage').jqGrid({
        url: getFriendLinListURL,
        datatype: "json",
        mtype: 'POST',
        search: '',
        colNames: [' ID', '名称', '链接地址', '排序',  '状态', '操作'],
        colModel: [
                { name: 'ID', index: 'ID', sortable: true, key: true, hidden: true },
                 
                { name: 'Title', index: 'Title', sortable: true },
                { name: 'LinkUrl', index: 'LinkUrl', sortable: true },
                 { name: 'Sort', index: 'Sort', sortable: true },
                 
                { name: 'IsShow', index: 'IsShow', sortable: true },
                { name: 'cz', index: 'cz', sortable: true }

            ],
        pager: '#pagerFriendLinManage',
        pagerpos: 'center',
        rowNum: 100,
        height: GetJqGridHeight() - 110,
        multiselect: true,
        altRows: true,
        multiboxonly: true,
        viewrecords: true,
        pginput: false,
        emptyrecords: "没有符合查询条件的数据",
        autowidth: true,
        altclass: 'tableAltClass',
        gridComplete: function () {
            $('#pagerFriendLinManage').trigger("gridComplete");
        }
    });

    //---------------------------添加 -------------------------------------------------
    $("#btnlinkAddFriendLin").click(function () {
        var options = { width: 600, height: 500, max: false, mask: true, fresh: true };
        var url = getControllerUrl("SystemManagement", "AddFriendLinView");
        $.pdialog.open(encodeURI(url), "AddFriendLin", "友情链接", options);
    });

    //---------------------------编辑 -------------------------------------------------
    $("#btnlinkUpFriendLin").click(function () {
        var ID;
        var selectUnit = $(':checked', '#tableFriendLinManage');
        if (selectUnit.size() == 0) {
            confirm('请选择修改的信息！'); return;
        }
        if (selectUnit.size() > 1) {
            confirm('只能选择一条信息！'); return;
            
        }
        if(selectUnit.size()==1)
        {
            ID = jQuery("#tableFriendLinManage").jqGrid('getGridParam', 'selrow');
        }
        var options = { width: 600, height: 500, max: false, mask: true, fresh: true };
        var url = getControllerUrl("SystemManagement", "UpFriendLinView") + "?ID=" + ID;
        $.pdialog.open(encodeURI(url), "UpFriendLin", "友情链接", options);
    });
    //---------------------------删除 -------------------------------------------------
    $('#btnlinkDelFriendLin').click(function () {
        var selectUnit = $(':checked', '#tableFriendLinManage');
        if (selectUnit.size() == 0) {
            confirm('请选折要删除的信息！');
            return;
        } else {
            var ids = jQuery("#tableFriendLinManage").jqGrid('getGridParam', 'selarrrow');
            var ids = ids.join(',');
            $.ajax({
                type: "post", //使用get方法访问后台 
                dataType: "json", //返回json格式的数据
                url: deleteFriendLinURL,
                data:{IsShow:2,IDs:ids} ,
                success: function (data) {
                    $('#tableFriendLinManage').setGridParam({
                        url: getFriendLinListURL,
                        page: 1
                    }).trigger("reloadGrid");
                    alertMsg.correct("操作成功！");

                },
                error: function () {
                    alertMsg.warn("对不起！系统出错啦，我们会尽快跟踪处理的！");
                }
            });
            $(this).dialog('close');
        }
    });

});
//详细页
function ck(obj) {

    var options = { width: 600, height: 500, max: false, mask: true, fresh: true };
    var url = getControllerUrl("SystemManagement", "UpFriendLinView") + "?ID=" + obj;
    $.pdialog.open(encodeURI(url), "UpFriendLin", "友情链接", options);
}