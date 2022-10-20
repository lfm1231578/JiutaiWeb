function isEmailAvailable() {
    var dd = $("#txtEmail").val();
    var myreg = /^([a-zA-Z0-9]+[_|\_|\.]?)*[a-zA-Z0-9]+@([a-zA-Z0-9]+[_|\_|\.]?)*[a-zA-Z0-9]+\.[a-zA-Z]{2,3}$/;
    if (!myreg.test(dd)) {
       // alert("请输入正确电子邮箱地址！");
        return false;
    }
}