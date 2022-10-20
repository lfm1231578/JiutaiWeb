
var GetRoleListURL = getControllerUrl("SystemManagement", "GetRoleList");
var addRoleURL = getControllerUrl("SystemManagement", "AddRole");
var deleteRoleURL = getControllerUrl("SystemManagement", "DeleteRole");
var editRoleURL = getControllerUrl("SystemManagement", "EditRole");
var getOutPutRoleInfoUrl = getControllerUrl("FileHandle", "ExportRoleInfoDataToFile"); //导出  
//---------------------------初始化Ajax请求，查询角色列表-------------------------------------------------
$(function () {
    autoResize({
        dataGrid: '#tableRoleManage'
    });

    $('#tableRoleManage').jqGrid({
        url: GetRoleListURL,
        datatype: "json",
        mtype: 'post',
        search: '',
        colNames: ['角色ID', '角色名称', '描述', '操作'],
        colModel: [
	                    { name: 'RoleID', index: 'RoleID', sortable: true, key: true, hidden: true },
	                    { name: 'RoleName', index: 'RoleName', sortable: true },
	                    { name: 'Memo', index: 'Memo', sortable: true },
        //                        { name: 'IsActivity', index: 'IsActivity', sortable: true},
                        {name: '', index: '', sortable: true }
   	              ],
        pager: $('#pagerRoleManage'),
        pagerpos: 'center',
        rowNum: 100,
        multiselect: true,
        altRows: true,
        multiboxonly: true,
        autowidth: true,
        viewrecords: true,
        height: GetJqGridHeight() - 50,
        emptyrecords: "没有符合查询条件的数据",
        gridComplete: function () {
            $('#tableRoleManage').trigger("gridComplete");
        }
    });
    //设置数据权限
    $("#btnsetdataRight").click(function () {
        var id = jQuery("#tableRoleManage").jqGrid('getGridParam', 'selrow');
        var data = jQuery("#tableRoleManage").jqGrid('getRowData', id);
        window.top.art.dialog({ title: "设置-" + data.RoleName + '-数据权限', id: 'edit', iframe: getControllerUrl("SystemManagement", "RoleManageDataRight") + '?id=' + id + "&name=" + data.RoleName + "&meno=" + data.Memo, width: '398', height: '171', show: true }, function () { var d = window.top.art.dialog({ id: 'edit' }).data.iframe; d.document.getElementById('dosubmit').click(); return false; }, function () { window.top.art.dialog({ id: 'edit' }).close() });

    })


});
function edit(id, name) {
    var data = jQuery("#tableRoleManage").jqGrid('getRowData', id);
    window.top.art.dialog({ id: 'edit' }).close();
    window.top.art.dialog({ title: "编辑" + '--' + name, id: 'edit', iframe: getControllerUrl("SystemManagement", "RoleManageUpdate") + '?id=' + id + "&name=" + data.RoleName + "&meno=" + data.Memo, width: '480', height: '250', show: true }, function () { var d = window.top.art.dialog({ id: 'edit' }).data.iframe; d.document.getElementById('dosubmit').click(); return false; }, function () { window.top.art.dialog({ id: 'edit' }).close() });
}
function check() {

    var ids = jQuery("#tableRoleManage").jqGrid('getGridParam', 'selarrrow');
    if (ids == null || ids == "") {
        window.top.art.dialog({ content: "请选择角色", lock: true, width: '200', height: '50', time: 1.5 }, function () { });
        return false;
    }
    $("#myform").attr("action", getControllerUrl("SystemManagement", "DeleteRole") + "?rolelist=" + ids.join(','));
    return true;
}
