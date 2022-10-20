var getSlideshowListURL = getControllerUrl("SystemManagement", "GetSlideshowList"); //查询操作
var deleteSlideshowURL = getControllerUrl("SystemManagement", "DelSlideshow"); // 删除

//---------------------------初始化Ajax请求，查询用户列表-------------------------------------------------
var isNumber = /^[A-Za-z0-9]+$/;

$(function () {

    //初始化jqGrid表格事件
    $('#tableSlideshowManage').jqGrid({
        url: getSlideshowListURL,
        datatype: "json",
        mtype: 'POST',
        search: '',
        colNames: [' ID', '标签', '图片说明', '链接地址', '排序', '创建时间', '状态', '操作'],
        colModel: [
                { name: 'ID', index: 'ID', sortable: true, key: true, hidden: true },
                { name: 'Type', index: 'Type', sortable: true },
                { name: 'Title', index: 'Title', sortable: true },
                { name: 'LinkUrl', index: 'LinkUrl', sortable: true },
                 { name: 'Sort', index: 'Sort', sortable: true },
                  { name: 'CreateTime', index: 'CreateTime', sortable: true },
                { name: 'IsShow', index: 'IsShow', sortable: true },
                { name: 'cz', index: 'cz', sortable: true }

            ],
        pager: '#pagerSlideshowManage',
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
            $('#pagerSlideshowManage').trigger("gridComplete");
        }
    });

    //---------------------------添加 -------------------------------------------------
    $("#btnlinkAddSlideshow").click(function () {
        var options = { width: 600, height: 500, max: false, mask: true, fresh: true };
        var url = getControllerUrl("SystemManagement", "AddSlideshowView");
        $.pdialog.open(encodeURI(url), "AddSlideshow", "幻灯片", options);
    });

    //---------------------------编辑 -------------------------------------------------
    $("#btnlinkUpSlideshow").click(function () {
        var ID;
        var selectUnit = $(':checked', '#tableSlideshowManage');
        if (selectUnit.size() == 0) {
            confirm('请选择修改的信息！'); return;
        }
        if (selectUnit.size() > 1) {
            confirm('只能选择一条信息！'); return;
            
        }
        if(selectUnit.size()==1)
        {
            ID = jQuery("#tableSlideshowManage").jqGrid('getGridParam', 'selrow');
        }
        var options = { width: 600, height: 500, max: false, mask: true, fresh: true };
        var url = getControllerUrl("SystemManagement", "UpSlideshowView") + "?ID=" + ID;
        $.pdialog.open(encodeURI(url), "UpSlideshow", "幻灯片", options);
    });
    //---------------------------删除 -------------------------------------------------
    $('#btnlinkDelSlideshow').click(function () {
        var selectUnit = $(':checked', '#tableSlideshowManage');
        if (selectUnit.size() == 0) {
            confirm('请选折要删除的信息！');
            return;
        } else {
            var ids = jQuery("#tableSlideshowManage").jqGrid('getGridParam', 'selarrrow');
            var ids = ids.join(',');
            $.ajax({
                type: "post", //使用get方法访问后台 
                dataType: "json", //返回json格式的数据
                url: deleteSlideshowURL,
                data:{IsShow:2,IDs:ids} ,
                success: function (data) {
                    $('#tableSlideshowManage').setGridParam({
                        url: getSlideshowListURL,
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
    var url = getControllerUrl("SystemManagement", "UpSlideshowView") + "?ID=" + obj;
    $.pdialog.open(encodeURI(url), "UpSlideshow", "幻灯片", options);
}