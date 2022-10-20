var getDepartmentListUrl = getControllerUrl("SystemManagement", "GetDepartmentTree");
var getDepartmentUrl = getControllerUrl("SystemManagement", "GetDepartment");
//---------------------------初始化Ajax请求，查询地区列表-------------------------------------------------
$(function () {
    //获取部门人员信息
    $('#tableDepartmentManage').jqGrid({
        url: getControllerUrl("SystemManagement", "GetDepartmentUser"),
        datatype: "json",
        mtype: 'POST',
        search: '',
        colNames: ['ID', '员工工号', '姓名', '部门', '性别', '岗位', '查看详情'],
        colModel: [
                            { name: 'ID', index: 'ID', sortable: true, key: true, hidden: true },
                            { name: 'WorkNumber ', index: 'WorkNumber', sortable: true },
                            { name: 'UserName', index: 'UserName', sortable: true },
                            { name: 'Department', index: 'Department', sortable: true },
                            { name: 'Sex', index: 'Sex', sortable: true },
                            { name: 'Duty', index: 'Duty', sortable: true },
                            { name: '', index: '', sortable: true, hidden: true }

                        ],
        pager: '#pagerDepartmentManage',
        pagerpos: 'center',
        rowNum: 100,
        height: 200,
        width: 580,
        multiselect: true,
        altRows: true,
        multiboxonly: true,
        viewrecords: true,
        pginput: false,
        emptyrecords: "没有符合查询条件的数据",
        //        autowidth: true,
        altclass: 'tableAltClass',
        gridComplete: function () {
            $('#tableDepartmentManage').trigger("gridComplete");
        }
    });
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "json", //返回json格式的数据
        url: getDepartmentListUrl,
        success: function (data) {
            var o = { onnodeclick: function (item) {
                $('#tableDepartmentManage').setGridParam({
                    url: getControllerUrl("SystemManagement", "GetDepartmentUser") + "?DeptCode=" + item.value,
                    page: 1
                }).trigger("reloadGrid");
            }
            };
            o.data = data;
            $("#treeContent").treeview(o);
        },
        error: function () {
        }
    });

    $("#dialogareaDepartment").dialog({
        modal: true,
        autoOpen: false,
        height: 380,
        width: 800,
        buttons: {
            "确定": function () {
                var username = "";
                var result = "";
                var ids = jQuery("#tableDepartmentManage").jqGrid('getGridParam', 'selarrrow');
                for (var i = 0; i < ids.length; i++) {
                    username = jQuery("#tableDepartmentManage").jqGrid('getRowData', ids[i]);
                    result += username.UserName + ",";
                }

                $("#UserName").val(result);
                $("#UserNames").val(result);
                $("#hiddendepartment").val(ids.join(','));

                $(this).dialog("close");
            },
            "取消": function () {
                $(this).dialog("close");
            }
        }
    });


    //选择部门
    $("#UserName").click(function () {
        $("#dialogareaDepartment").dialog('open');
    });

});