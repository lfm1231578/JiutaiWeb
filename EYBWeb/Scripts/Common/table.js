$(function () {
    $("#articles").delegate(".gridTable thead  th", "click", function () {
        var $th = $(this);
        var $orderfiled;
         var $orderfiled = $th.attr("class");
        //var $orderfiled = $("#orderField").val();
        var $filed = $th.attr("orderField");
        var submitform = document.getElementById("searchForm");
        if ($filed == undefined || $filed == "") return;
        if ($orderfiled == "asc") {
            //改变样式
            $th.removeClass("asc");
            $th.addClass("desc");
            $orderfiled = $th.attr("orderField");
            //赋值
            $("#orderDirection").val("desc");
            $("#orderField").val($orderfiled);
            $("#btnsumbit").click();
            //提交
           // submitform.submit();
        } else {
            //改变样式
            $th.removeClass("desc");
            $th.addClass("asc");
            $orderfiled = $th.attr("orderField");
            //赋值

            $("#orderDirection").val("asc");
            $("#orderField").val($orderfiled);
            $("#btnsumbit").click();
            //提交
            //submitform.submit();
        }
    })
 
})