var searchLogURL = getControllerUrl("SystemManagement", "GetLogList");

$(function () {
    autoResize({
        dataGrid: '#logList'
    });
    $('.datepicker').datepicker({
        changeMonth: true,
        changeYear: true,
        dateFormat: 'yy-mm-dd 00:00:00'
    });
    $('.datepicker1').datepicker({
        changeMonth: true,
        changeYear: true,
        dateFormat: 'yy-mm-dd 23:59:59'
    });
    var obj = new Object();
    obj.startTime = $('#StartTime').val();
   
    obj.endTime = $('#EndTime').val();
    obj.account = $('#txtUserAllInfo').val();
    obj.module = $('#ModuleList').val();
        
    $('#logList').jqGrid({
        url: searchLogURL,
        datatype: "json",
        mtype: 'POST',
        postData:obj,
        search: '',
        colNames: ['日志时间', '操作者', '动作', 'IP地址', '操作模块', '日志内容'],
        colModel: [
	                        { name: 'OperateTime', index: 'OperateTime', sortable: true },
	                        { name: 'Account', index: 'Account', sortable: true },
	                        { name: 'Action', index: 'Action', sortable: true },
	                        { name: 'ClientIP', index: 'ClientIP', sortable: true },
	                        { name: 'ModuleName', index: 'ModuleName', sortable: true },
	                        { name: 'Memo', index: 'Memo', sortable: true, key: false },
   	                    ],
        pager: $('#logListPage'),
        pagerpos: 'center',
        rowNum: 100,
        altRows: true,
        autowidth: true,
        viewrecords: true,
        emptyrecords: "没有符合查询条件的数据",
        height: GetJqGridHeight()-50,
        altRows: true,
        altclass: 'tableAltClass',
        gridComplete: function () {
            $('#logList').trigger("gridComplete");
        }
    });
    //-------------------------------------日志管理--查询业务-------------------------------------
    $('#btnsearch').click(function () {

        if ($('#StartTime').val() == "" && $('#txtLogEndTime').val() != "") {
            alert("起始时间不能为空！"); return;
        }
        if ($('#EndTime').val() != "" && $('#txtLogEndTime').val() == "") {
            alert("结束时间不能为空！"); return;
        }
        var search = new Object();
        search.startTime = $('#StartTime').val();
        search.endTime = $('#EndTime').val();
        search.account = $('#txtUserAllInfo').val();
        search.module = $('#ModuleList').val();
        
        $('#logList').setGridParam({
            url: searchLogURL,
            postData: search,
            page: 1
        }).trigger("reloadGrid");


    });

    //-------------------------------------以下其他业务，页面暂不实现-------------------------------------------------
    //日志管理--清除业务
    $('#btnClear').click(function () {
        $('#selUserID').val("全部");
        $('#ddlModul').val("0")
        $('#txtLogContent').val("")
        $('#beginTime').attr('value', beginTime);
        $('#endTime').attr('value', endTime);
    });

    //日志管理--导出所有
    $('#btnExportAll').click(function () {
        var obj = document.getElementById('ddlModul');
        var index = obj.selectedIndex; //序号，取当前选中选项的序号
        var val = obj.options[index].text;
        var RoleManageAllParms = $('#beginTime').val() + "," + $('#endTime').val() + "," + encodeURI($('#selUserID').val()) + "," + encodeURI(val) + "," + encodeURI($('#txtLogContent').val());
        ExportExcel("日志管理", "", "LogManageAll", RoleManageAllParms);
    });

    //日志管理--导出
    $('#btnExport').click(function () {
        ExportExcel("日志管理", "", "LogManage", "");
    });

    $('.datepicker').datepicker({
        beforeShow: function (input, inst) {
            inst.dpDiv.css('z-index', '999999');
        }
    });
});

function resetParams() {
    fileExportInfo.fileName = "DomainImplement";
    fileExportInfo.nameSpaceName = "PASS.PASSWEBAPP.DomainImpl";
    fileExportInfo.className = "LogManageImpl";

    fileExportInfo.inputParams += "beginTime=" + $('#beginTime').val();
    fileExportInfo.inputParams += "⊕endTime=" + $('#endTime').val();
    fileExportInfo.inputParams += "⊕user=" + $('#selUserID').val() || 0;
    var obj = document.getElementById('ddlModul');
    var index = obj.selectedIndex; //序号，取当前选中选项的序号
    var val = obj.options[index].text;
    val = (val == "全部") ? "" : val;
    fileExportInfo.inputParams += "⊕moduleName=" + encodeURI(val);
    var logText = $('#txtLogContent').val();
    fileExportInfo.inputParams += "⊕logContent=" + encodeURI(logText);
    fileExportInfo.inputParams += "⊕Page=LogManage"; //标识不同页面
    return true;
}