
initControls();

$(function () {
    $("#btnPersonalInternetSettingSave").click(function () {

            var obj = new Object();
            if ($("#txtPersonalInternetSettingUnitSelect").val() == "") {
                alertMsg.warn("请选择场所！");
                return;
            }
            var unitCode = @(ViewBag.unitcode);//alert(obj.unitCode);
            obj.UserCount = $("#txtPersonalInternetSettingUserCount").val();
            obj.cbSGICFindPassword = $("input[id='cbSGICFindPassword']:checked").val();
            obj.cbSGICModifyPassword = $("input[id='cbSGICModifyPassword']:checked").val();
            obj.cbSGICReSendPassword = $("input[id='cbSGICReSendPassword']:checked").val();
            obj.chkAllowScannerUp = $("input[id='chkAllowScannerUp']:checked").val();
            obj.chkAllowSubtract = $("input[id='chkAllowSubtract']:checked").val();
            obj.chkAllowCheckCopy = $("input[id='chkAllowCheckCopy']:checked").val();
            obj.chkAllowFirstIdCard = $("input[id='chkAllowFirstIdCard']:checked").val();
            obj.chkAllowManualRegNIIC = $("input[id='chkAllowManualRegNIIC']:checked").val();
            obj.EnableOtherCertMannual = $("input[id='EnableOtherCertMannual']:checked").val();
            obj.txtPersonalInternetSettingOpentimes = $("#txtPersonalInternetSettingOpentimes").val();
            obj.chkOpenBillSystemCheck = $("input[id='chkOpenBillSystemCheck']:checked").val();

            obj.radchkAllow = $("input[id='radchkAllow']:checked").val();
            obj.AllowBusiness= $("input:radio[name='radAllowUnitBusiness']:checked").val();
            obj.chkbillingsystemP2001 = $("input[id='chkbillingsystemP2001']:checked").val();
            obj.chkbillingsystemP3002 = $("input[id='chkbillingsystemP3002']:checked").val();
            obj.chkbillingsystemM2006 = $("input[id='chkbillingsystemM2006']:checked").val();
            obj.chkbillingsystemS2007 = $("input[id='chkbillingsystemS2007']:checked").val();
            obj.chkbillingsystemJ2045 = $("input[id='chkbillingsystemJ2045']:checked").val();
            obj.chkbillingsystemY2046 = $("input[id='chkbillingsystemY2046']:checked").val();
            obj.radFingerprintmachineY = $("input[id='radFingerprintmachineY']:checked").val();
            obj.radEnterScratchY = $("input[id='radEnterScratchY'] :checked").val();
            obj.radChecktemporarycardnumberY = $("input[id='radChecktemporarycardnumberY']:checked").val();
            obj.txtPersonalInternetSettingtempcardnum = $("#txtPersonalInternetSettingtempcardnum").val();
            obj.radgenerationcardserialnumberY = $("input[id='radgenerationcardserialnumberY']:checked").val();
            obj.radopenIDcardY = $("input[id='radopenIDcardY']:checked").val();
            obj.radCheckAuditInterfaceY = $("input[id='radCheckAuditInterfaceY']:checked").val();
            obj.centerid = '@centerid';
            obj.unitID='@unitid';
            $.ajax({
                type: "POST",
                dataType: "Json",
                url: getControllerUrl("TechnicalSupport", "SetUnitSetting") + "?unitCode=" + unitCode,
                data: obj,
                success: function (data) {
                    if (data != null) {
                        alertMsg.correct("设置成功！");
                    }
                }, error: function (xhr) {
                    alertMsg.error("出问题了" + xhr.responseText);
                }
            });
        });
});

function initControls() {  

        var ddlDefaultPage = "@(entity[SysSettingGatherEntity.Public, SysSettingGatherEntity.DefaultPage] != null ? entity[SysSettingGatherEntity.Public, SysSettingGatherEntity.DefaultPage].FieldValue : "")";
        $("#txtPersonalInternetSettingUserCount").val(internetSettingUserCount);
        var cbSGICFindPassword ="@(entity[unitcode, entity.SGICFindPassword]!=null ? (entity[unitcode, entity.SGICFindPassword].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("二代证绑定手机,网吧控制台可-查询上机密码",cbSGICFindPassword);
        
        if ( cbSGICFindPassword == "checked")
            $("#cbSGICFindPassword").attr("checked",true);     //是
        else        
            $("#cbSGICFindPassword").attr("checked",false);     //否
            
        var cbSGICModifyPassword ="@(entity[unitcode, entity.SGICModifyPassword] != null ? (entity[unitcode, entity.SGICModifyPassword].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("二代证绑定手机,网吧控制台可-修改密码",cbSGICModifyPassword);
        if ( cbSGICModifyPassword == "checked")
        
            $("#cbSGICModifyPassword").attr("checked",true);     //是
        else        
            $("#cbSGICModifyPassword").attr("checked",false);     //否
            
        var cbSGICReSendPassword ="@(entity[unitcode, entity.SGICReSendPassword] != null ? (entity[unitcode, entity.SGICReSendPassword].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("二代证绑定手机,网吧控制台可-重发密码到手机",cbSGICReSendPassword);
       
        if ( cbSGICReSendPassword == "checked")
            $("#cbSGICReSendPassword").attr("checked",true);     //是
        else        
            $("#cbSGICReSendPassword").attr("checked",false);     //否



                var AllowBusiness = "@(entity[unitcode, entity.AllowBusiness] != null ? (entity[unitcode, entity.AllowBusiness].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("禁止场所营业",AllowBusiness);  

        if ( AllowBusiness == "checked" )
            $("#radAllowUnitBusinessyes").attr("checked",true);        //开启
        else 
            $("#radAllowUnitBusinessno").attr("checked",true);        //开启        


        var radEnterScratch ="@(entity[unitcode, entity.IsNeedRegisterCardNo] != null ? (entity[unitcode, entity.IsNeedRegisterCardNo].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("是否在指纹注册要求输入刮刮卡",radEnterScratch);

        if ( radEnterScratch == "checked")
            $("#radEnterScratchY").attr("checked",true);     //是
        else        
            $("#radEnterScratchN").attr("checked",true);     //否
            
        var checkAuditInterface = "@(entity[unitcode, entity.IsBillFunctionNoCheckFail] != null ? (entity[unitcode, entity.IsBillFunctionNoCheckFail].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("审计接口号检查",checkAuditInterface);
        if ( checkAuditInterface == "checked")
            $("#radCheckAuditInterfaceY").attr("checked",true);     //是
        else 
            $("#radCheckAuditInterfaceN").attr("checked",true);     //否

        var chkScannerUp = "@(entity[unitcode, entity.SC_AllowRegist] != null ? (entity[unitcode, entity.SC_AllowRegist].ParamValue == "Y" ? "checked" : "" ):"")";
        alertMsg.warn("扫描仪注册",chkScannerUp);                
        if ( chkScannerUp == "checked" )
            $("#chkAllowScannerUp").attr("checked",true);        //开启
        else 
            $("#chkCloseScannerUp").attr("checked",true);        //关闭
            
        var chkSubtract = "@(entity[unitcode, entity.SC_AllowSubtract] != null ? (entity[unitcode, entity.SC_AllowSubtract].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("扣点功能开关",chkSubtract);  
        if ( chkSubtract == "checked" )
            $("#chkAllowSubtract").attr("checked",true);        //开启
        else 
            $("#chkCloseSubtract").attr("checked",true);        //开启            
            
         
            
        var chkCheckCopy = "@(entity[unitcode, entity.SC_CheckCopy] != null ? (entity[unitcode, entity.SC_CheckCopy].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("是否对复印件检查开关",chkCheckCopy);  
        if ( chkCheckCopy == "checked" )
            $("#chkAllowCheckCopy").attr("checked",true);        //开启
        else 
            $("#chkCloseCheckCopy").attr("checked",true);        //开启
            
        var chkFirstIdCard = "@(entity[unitcode, entity.SC_AllowFirstIdCard] != null ? (entity[unitcode, entity.SC_AllowFirstIdCard].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("是否支持一代证扫描",chkFirstIdCard);  
        if ( chkFirstIdCard == "checked" )
            $("#chkAllowFirstIdCard").attr("checked",true);        //开启
        else 
            $("#chkCloseFirstIdCard").attr("checked",true);        //开启
            
        var chkManualRegNIIC = "@(entity[unitcode, entity.EnableOtherCertMannualRegist] != null ? (entity[unitcode, entity.EnableOtherCertMannualRegist].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("通过手动输入证件信息",chkManualRegNIIC);  
        if ( chkManualRegNIIC == "checked" )
            $("#chkAllowManualRegNIIC").attr("checked",true);        //开启
        else 
            $("#chkCloseManualRegNIIC").attr("checked",true);        //开启  

        var OtherCertMannual = "@(entity[unitcode, entity.CheckScannerManualCount] != null ? (entity[unitcode, entity.CheckScannerManualCount].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("外国人上网功能(手动开户)开关",OtherCertMannual);  
        if ( OtherCertMannual == "checked" )
            $("#EnableOtherCertMannual").attr("checked",true);        //开启
        else 
            $("#unEnableOtherCertMannual").attr("checked",true);        //开启  
            
        var opentimes = "@(entity[unitcode, entity.AllowScannerManualCount] != null ? entity[unitcode, entity.AllowScannerManualCount].ParamValue :"" )";
        alertMsg.warn("输入每天允许手动开户次数",opentimes);  
        $("#txtPersonalInternetSettingOpentimes").val(opentimes);     
                        
        var chkBillSystemCheck = "@(entity[unitcode, entity.OpenBillSystemCheck] != null ? (entity[unitcode, entity.OpenBillSystemCheck].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("计费厂家检查",chkBillSystemCheck);  
        if ( chkBillSystemCheck == "checked" )
            $("#chkOpenBillSystemCheck").attr("checked",true);        //开启
        else 
            $("#chkCloseBillSystemCheck").attr("checked",true);        //开启  
            
        var radch = "@(entity[unitcode, entity.IsAllowLoginByClientError] != null ? (entity[unitcode, entity.IsAllowLoginByClientError].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("是否开启客户端异常时允许用户上机控制",radch);  
        if ( radch == "checked" )
            $("#radchkAllow").attr("checked",true);        //开启
        else 
            $("#radchkClose").attr("checked",true);        //开启  
            
        var chkbillingsystemP2001 = "@(entity[unitcode, entity.BillSystemCode] != null ? (entity[unitcode, entity.BillSystemCode].ParamValue.Contains("P2001") ? "checked" : ""):"")";
        alertMsg.warn("网吧可接受的计费系统--Pubwin",chkbillingsystemP2001);  
        if ( chkbillingsystemP2001 == "checked" )
            $("#chkbillingsystemP2001").attr("checked",true);        //开启
        else 
            $("#chkbillingsystemP2001").attr("checked",false);        //开启  

        var chkbillingsystemP3002 = "@(entity[unitcode, entity.BillSystemCode] != null ? (entity[unitcode, entity.BillSystemCode].ParamValue.Contains("P3002") ? "checked" : ""):"")";
        alertMsg.warn("网吧可接受的计费系统--万象",chkbillingsystemP3002);  
        if ( chkbillingsystemP3002 == "checked" )
            $("#chkbillingsystemP3002").attr("checked",true);        //开启
        else 
            $("#chkbillingsystemP3002").attr("checked",false);        //开启  
            
        var chkbillingsystemM2006 = "@(entity[unitcode, entity.BillSystemCode] != null ? (entity[unitcode, entity.BillSystemCode].ParamValue.Contains("M2006") ? "checked" : ""):"")";
        alertMsg.warn("网吧可接受的计费系统--麦科",chkbillingsystemM2006);  
        if ( chkbillingsystemM2006 == "checked" )
            $("#chkbillingsystemM2006").attr("checked",true);        //开启
        else 
            $("#chkbillingsystemM2006").attr("checked",false);        //开启
            
        var chkbillingsystemS2007 = "@(entity[unitcode, entity.BillSystemCode] != null ? (entity[unitcode, entity.BillSystemCode].ParamValue.Contains("S2007") ? "checked" : ""):"")";
        alertMsg.warn("网吧可接受的计费系统--希之光",chkbillingsystemS2007);  
        if ( chkbillingsystemS2007 == "checked" )
            $("#chkbillingsystemS2007").attr("checked",true);        //开启
        else 
            $("#chkbillingsystemS2007").attr("checked",false);        //开启

        var chkbillingsystemJ2045 = "@(entity[unitcode, entity.BillSystemCode] != null ? (entity[unitcode, entity.BillSystemCode].ParamValue.Contains("J2045") ? "checked" : ""):"")";
        alertMsg.warn("网吧可接受的计费系统--佳星",chkbillingsystemJ2045);  
        if ( chkbillingsystemJ2045 == "checked" )
            $("#chkbillingsystemJ2045").attr("checked",true);        //开启
        else 
            $("#chkbillingsystemJ2045").attr("checked",false);        //开启
            
        var chkbillingsystemY2046 = "@(entity[unitcode, entity.BillSystemCode] != null ? (entity[unitcode, entity.BillSystemCode].ParamValue.Contains("Y2046") ? "checked" : ""):"")";
        alertMsg.warn("网吧可接受的计费系统--摇钱树",chkbillingsystemY2046);  
        if ( chkbillingsystemY2046 == "checked" )
            $("#chkbillingsystemY2046").attr("checked",true);        //开启
        else 
            $("#chkbillingsystemY2046").attr("checked",false);        //开启

        var radFingerprintmachine = "@(entity[unitcode, entity.SwitchLocaleDevice] != null ? (entity[unitcode, entity.SwitchLocaleDevice].ParamValue == "Y" ? "checked" : ""):"")";
        alertMsg.warn("指纹机是否开启",radFingerprintmachine);  
        if ( radFingerprintmachine == "checked" )
            $("#radFingerprintmachineY").attr("checked",true);        //开启
        else 
            $("#radFingerprintmachineN").attr("checked",true);        //开启  
            
        checkTempCardCount();//是否检查临时卡数量
        
        var tempcardnum = "@(entity[unitcode, entity.AllowTempCardCount] != null ? (entity[unitcode, entity.AllowTempCardCount].ParamValue != "" ? entity[unitcode, entity.AllowTempCardCount].ParamValue : "0") : "0")";
        alertMsg.warn("允许临时卡数量",tempcardnum);  
        $("#txtPersonalInternetSettingtempcardnum").val(tempcardnum);        //开启
    
        generationCardSerialNumber();//检查二代证序列号
        openIDcard();//是否开启ID卡
    }