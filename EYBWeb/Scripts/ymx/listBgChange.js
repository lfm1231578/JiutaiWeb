//$(function () {
//    $("tbody tr td").each(function (a) {
//        $(this).hover(function () {
//            let n = 15;
//            let a1 = a;
//            let b1 = a;
//            let tdLength = $("tbody tr td").length;
//            for (let i = 0; i < a; i++) {
//                if ((a1 - n) >= 0) {
//                    a1 = a1 - n
//                    $("tbody tr td").eq(a1).css("background-color", "#FFF0F5");
//                }

//            }

//            for (let i = 0; i < tdLength; i++) {

//                if (b1 + n < tdLength) {
//                    b1 = b1 + n;
//                    $("tbody tr td").eq(b1).css("background-color", "#FFF0F5");

//                }

//            }
//            $("tbody tr td").eq(a).css("background-color", "#FFF0F5");

//        }, function () {
//            let n = 15;
//            let a1 = a;
//            let b1 = a;
//            let tdLength = $("tbody tr td").length;
//            for (let i = 0; i < tdLength; i++) {
//                if ((a1 - n) >= 0) {
//                    a1 = a1 - n;
//                    $("tbody tr td").eq(a1).css("background-color", "");
//                }

//            }

//            for (let i = 0; i < tdLength; i++) {

//                if (b1 + n < tdLength) {
//                    b1 = b1 + n;
//                    $("tbody tr td").eq(b1).css("background-color", "");

//                }

//            }
//            $("tbody tr td").eq(a).css("background-color", "");
//        })
//    })
//})