var getAdListURL = getControllerUrl("SystemManagement", "GetAdList"); //查询操作
var deleteAdURL = getControllerUrl("SystemManagement", "DelAd"); // 删除

//---------------------------初始化Ajax请求，查询用户列表-------------------------------------------------
var isNumber = /^[A-Za-z0-9]+$/;

$(function () {

    //初始化jqGrid表格事件
    $('#tableAdManage').jqGrid({
        url: getAdListURL,
        datatype: "json",
        mtype: 'POST',
        search: '',
        colNames: [' ID','广告位置', '广告名称', '图片地址',  '状态'],
        colModel: [
                { name: 'ID', index: 'ID', sortable: true, key: true, hidden: true },
                 { name: 'PositionType', index: 'PositionType', sortable: true },
                { name: 'Title', index: 'Title', sortable: true },
                { name: 'LinkUrl', index: 'LinkUrl', sortable: true },
                { name: 'IsShow', index: 'IsShow', sortable: true }
            ],
        pager: '#pagerAdManage',
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
            $('#tableAdManage').trigger("gridComplete");
        }
    });

    //---------------------------添加 -------------------------------------------------
    $("#btnlinkAddAd").click(function () {
        var options = { width: 550, height: 300, max: false, mask: true, fresh: true };
        var url = getControllerUrl("SystemManagement", "AddAdView");
        $.pdialog.open(encodeURI(url), "AddAd", "添加广告位", options);
    });

    //---------------------------编辑 -------------------------------------------------
    $("#btnlinkUpAd").click(function () {
        var ID;
        var selectUnit = $(':checked', '#tableAdManage');
        if (selectUnit.size() == 0) {
            confirm('请选择修改的信息！'); return;
        }
        if (selectUnit.size() > 1) {
            confirm('只能选择一条信息！'); return;
            
        }
        if(selectUnit.size()==1)
        {
            ID = jQuery("#tableAdManage").jqGrid('getGridParam', 'selrow');
        }
        var options = { width: 550, height: 300, max: false, mask: true, fresh: true };
        var url = getControllerUrl("SystemManagement", "UpdateAdView") + "?ID=" + ID;
        $.pdialog.open(encodeURI(url), "UpAd", "修改广告位", options);
    });
    //---------------------------删除 -------------------------------------------------
    $('#btnlinkDelAd').click(function () {
        var selectUnit = $(':checked', '#tableAdManage');
        if (selectUnit.size() == 0) {
            confirm('请选折要删除的信息！');
            return;
        } else {
            var ids = jQuery("#tableAdManage").jqGrid('getGridParam', 'selrow');

            $.ajax({
                type: "post", //使用get方法访问后台 
                dataType: "json", //返回json格式的数据
                url: deleteAdURL,
                data:{IsShow:0,IDs:ids} ,
                success: function (data) {
                    $('#tableAdManage').setGridParam({
                        url: getAdListURL,
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
    var url = getControllerUrl("SystemManagement", "UpAdView") + "?ID=" + obj;
    $.pdialog.open(encodeURI(url), "UpAd", "广告位", options);
}