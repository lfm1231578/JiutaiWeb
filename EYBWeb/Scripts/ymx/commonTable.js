$(function () {
  window.setTimeout(function () {
        var gridtablewidth = $(".gridTable").width();
        var thlen = $(".gridTable thead th").length;
        //let theadWidth = $(".gridTable tbody").width()
      let theadWidth = $(".gridTable").width();
        $(".gridTable thead").css("width", theadWidth + "px")
        let tdWidth = (theadWidth / thlen) + "px";
        $(".gridTable thead th").css("width", tdWidth);
        $(".gridTable tr td").css("width", tdWidth);
    }, 1)

})