var getDepartmentListUrl = getControllerUrl("SystemManagement", "GetDepartmentTree");
var getDepartmentUrl = getControllerUrl("SystemManagement", "GetDepartment");
//---------------------------初始化Ajax请求，查询地区列表-------------------------------------------------
$(function () {
    //获取部门人员信息
    $('#tableDepartmentManage1').jqGrid({
        url: getControllerUrl("SystemManagement", "GetDepartmentUser"),
        datatype: "json",
        mtype: 'POST',
        search: '',
        colNames: ['ID', '员工工号', '姓名', '部门'],
        colModel: [
                            { name: 'ID', index: 'ID', sortable: true, key: true, hidden: true },
                            { name: 'WorkNumber ', index: 'WorkNumber', sortable: true },
                            { name: 'UserName', index: 'UserName', sortable: true },
                            { name: 'Department', index: 'Department', sortable: true },
                        ],
        pager: '#pagerDepartmentManage1',
        pagerpos: 'center',
        rowNum: 100,
        height: 100,
        width: 280,
        multiselect: true,
        altRows: true,
        multiboxonly: true,
        viewrecords: true,
        pginput: false,
        emptyrecords: "没有符合查询条件的数据",
        //        autowidth: true,
        altclass: 'tableAltClass',
        gridComplete: function () {
            $('#tableDepartmentManage1').trigger("gridComplete");
        }
    });
    $.ajax({
        type: "post", //使用get方法访问后台 
        dataType: "json", //返回json格式的数据
        url: getDepartmentListUrl,
        success: function (data) {
            var o = { onnodeclick: function (item) {
                $('#tableDepartmentManage1').setGridParam({
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

    $("#dialogareaDepartment1").dialog({
        modal: true,
        autoOpen: false,
        height: 260,
        width: 320,
        buttons: {
            "确定": function () {
                var userid = jQuery("#tableDepartmentManage1").jqGrid('getGridParam', 'selrow');
                var result = jQuery("#tableDepartmentManage1").jqGrid('getRowData', userid);
                $("#UserName1").val(result.UserName);
                $("#hiddendepartment1").val(userid);

                $(this).dialog("close");
            },
            "取消": function () {
                $(this).dialog("close");
            }
        }
    });


    //选择部门
    $("#UserName1").click(function () {
        $("#dialogareaDepartment1").dialog('open');
    });

});


