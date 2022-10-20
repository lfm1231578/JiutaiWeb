$(function () {
    //页面自动高度
    var docheight = getDocumentHeight() - $("#header").height() - $("#footer").height() + 15;
    $("#mainContent").height(docheight);

   $(".mainContentRight_bottom").css("height", docheight - 60);

     
    //面板初始化
    $("div.panel", document).jPanel();
    //分页样式
    //$("#badoopager").css({ position: "absolute", bottom: "0px", width: "100%" });
    //选择菜单切换
    $("#selmenu").change(function () {
        var href = $("#selmenu").find("option:selected").attr("href");
        location.href = href;
    });
    $("#seldiv").click(function () { $(".dropdown-menu").css("display", "block") })
    $("#header_bottom").mouseleave(function () { $(".dropdown-menu").css("display", "none") })
        $(window).resize(function () {
            $("#mainContentRight").height(getDocumentHeight() - $("#header").height() - $("#footer").height() - 5);
            $(".mainContentRight_bottom").height(getDocumentHeight() - $("#header").height() - $("#footer").height() - 60);
        });
    $(".searchInput").focus(function () {
        if ($(this).val() == "搜索") {
            $(this).val("");
        }
    })
    //清理缓存问题
    $("#btnsumbit").click(function () {
        $("#txtTime").val(Math.random());
    })

    //点击选中行颜色
    $("#articles").delegate("tr", "click", function () {
        $(this).addClass("trBackground");
    })

    //点击选中行颜色
    //$("#articles").delegate2(document.getElementsByClassName("box"), "click", function () {
    //    $(this).addClass("trBackground");
    //})
});
function sohwLocale() {
    var objD = new Date();
    var str, colorhead, colorfoot;
    var yy = objD.getYear();
    if (yy < 1900) yy = yy + 1900;
    var MM = objD.getMonth() + 1;
    if (MM < 10) MM = '0' + MM;
    var dd = objD.getDate();
    if (dd < 10) dd = '0' + dd;
    var hh = objD.getHours();
    if (hh < 10) hh = '0' + hh;
    var mm = objD.getMinutes();
    if (mm < 10) mm = '0' + mm;
    var ss = objD.getSeconds();
    if (ss < 10) ss = '0' + ss;
    var ww = objD.getDay();
    if (ww == 0) colorhead = "<span style='font-size:16px;'>";
    if (ww > 0 && ww < 6) colorhead = "<span style='font-size:16px;'>";
    if (ww == 6) colorhead = "<span style='font-size:16px;'>";
    if (ww == 0) ww = "星期日";
    if (ww == 1) ww = "星期一";
    if (ww == 2) ww = "星期二";
    if (ww == 3) ww = "星期三";
    if (ww == 4) ww = "星期四";
    if (ww == 5) ww = "星期五";
    if (ww == 6) ww = "星期六";
    colorfoot = "</span>"
    str = colorhead + yy + "-" + MM + "-" + dd + "  " + ww + colorfoot;
    document.getElementById("div_hour").innerHTML =   hh + ":" + mm  ;
    document.getElementById("div_timer").innerHTML = str;
    //return (str);
}
function tick() {

    sohwLocale();
    window.setTimeout("tick()", 1000);
}
 

