/*
* FORM VALIDATION MADE EASY
* ------------------------------------------
* Created by Victor Jonsson <http://www.victorjonsson.se>
* Documentation and issue tracking on Github <https://github.com/victorjonsson/jQuery-Form-Validator/>
*
* @license GPLv2 http://www.example.com/licenses/gpl.html
* @version 1.5.4
*/
(function(a){a.extend(a.fn,{validateOnBlur:function(b,c){var d=a(this);return d.find("textarea,input,select").blur(function(){a(this).doValidate(b,c)}),d},showHelpOnFocus:function(b){b||(b="data-validation-help");var c=a(this);return c.find("textarea,input").each(function(){var c=a(this).attr(b);c&&a(this).focus(function(){var b=a(this);b.parent().find(".jquery_form_help").length==0&&b.after(a("<span />").addClass("jquery_form_help").text(c).hide().fadeIn())}).blur(function(){a(this).parent().find(".jquery_form_help").fadeOut("slow",function(){a(this).remove()})})}),c},doValidate:function(b,c,d){typeof d=="undefined"&&(d=!0);var e=a(this),f={ignore:[],validationRuleAttribute:"data-validation",validationErrorMsgAttribute:"data-validation-error-msg",errorElementClass:"error",borderColorOnError:"red",dateFormat:"yyyy-mm-dd"};c&&a.extend(f,c),b&&a.extend(jQueryFormUtils.LANG,b),b=jQueryFormUtils.LANG;var g=e.attr("type");jQueryFormUtils.defaultBorderColor===null&&g!=="submit"&&g!=="checkbox"&&g!=="radio"&&(jQueryFormUtils.defaultBorderColor=e.css("border-color")),e.removeClass(f.errorElementClass).parent().find(".jquery_form_error_message").remove(),f.borderColorOnError!==""&&e.css("border-color",jQueryFormUtils.defaultBorderColor);if(!jQueryFormUtils.ignoreInput(e.attr("name"),g,f)){var h=jQueryFormUtils.validateInput(e,b,f);h===!0?e.unbind("keyup"):(e.addClass(f.errorElementClass).parent().append('<span class="jquery_form_error_message">'+h+"</span>"),f.borderColorOnError!==""&&e.css("border-color",f.borderColorOnError),d&&e.bind("keyup",function(){a(this).doValidate(b,c,!1)}))}return a(this)},validate:function(b,c){var d={ignore:[],errorElementClass:"error",borderColorOnError:"red",errorMessageClass:"jquery_form_error_message",validationRuleAttribute:"data-validation",validationErrorMsgAttribute:"data-validation-error-msg",errorMessagePosition:"top",scrollToTopOnError:!0,dateFormat:"yyyy-mm-dd"};c&&a.extend(d,c),b&&a.extend(jQueryFormUtils.LANG,b),b=jQueryFormUtils.LANG;var e=function(a){jQuery.inArray(a,f)<0&&f.push(a)},f=[],g=[],h=a(this);h.find("input,textarea,select").each(function(){var c=a(this),i=c.attr("type");if(!jQueryFormUtils.ignoreInput(c.attr("name"),i,d))if(i==="radio"){var j=c.attr(d.validationRuleAttribute);if(typeof j!="undefined"&&j==="required"){var k=c.attr("name"),l=!1;h.find("input[name="+k+"]").each(function(){if(a(this).is(":checked"))return l=!0,!1});if(!l){var m=c.attr(d.validationErrorMsgAttribute);c.attr("data-validation-current-error",m||b.requiredFields),f.push(m||b.requiredFields),g.push(c)}}}else{jQueryFormUtils.defaultBorderColor===null&&i&&(jQueryFormUtils.defaultBorderColor=a.trim(c.css("border-color")));var n=jQueryFormUtils.validateInput(c,b,d,h);n!==!0&&(g.push(c),c.attr("data-validation-current-error",n),e(n))}});var i=jQueryFormUtils.defaultBorderColor===null||jQueryFormUtils.defaultBorderColor.indexOf(" ")>-1&&jQueryFormUtils.defaultBorderColor.indexOf("rgb")==-1?"border":"border-color";h.find("input,textarea,select").css(i,jQueryFormUtils.defaultBorderColor).removeClass(d.errorElementClass),a("."+d.errorMessageClass.split(" ").join(".")).remove(),a(".jquery_form_error_message").remove();if(g.length>0){for(var j=0;j<g.length;j++)d.borderColorOnError!==""&&g[j].css("border-color",d.borderColorOnError),g[j].addClass(d.errorElementClass);if(d.errorMessagePosition==="top"){var k="<strong>"+b.errorTitle+"</strong>";for(var j=0;j<f.length;j++)k+="<br />* "+f[j];h.children().eq(0).before('<div class="'+d.errorMessageClass+'">'+k+"</div>"),d.scrollToTopOnError&&a(window).scrollTop(h.offset().top-20)}else for(var j=0;j<g.length;j++){var l=g[j].parent(),m=l.find("span[class=jquery_form_error_message]");m.length>0?m.eq(0).text(g[j].attr("data-validation-current-error")):l.append('<span class="jquery_form_error_message">'+g[j].attr("data-validation-current-error")+"</span>")}return!1}return!0},restrictLength:function(a){return new jQueryFormUtils.lengthRestriction(this,a),this}})})(jQuery);var jQueryFormUtils={};jQueryFormUtils.defaultBorderColor=null,jQueryFormUtils.validateEmail=function(a){var b=/^([a-zA-Z0-9_\.\-])+@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;if(b.test(a)){var c=a.split("@");if(c.length==2)return jQueryFormUtils.validateDomain(c[1])}return!1},jQueryFormUtils.validatePhoneNumber=function(a){var b=a.match(/\+/g),c=a.match(/-/g);return b!==null&&b.length>1||c!==null&&c.length>1?!1:b!==null&&a.indexOf("+")!==0?!1:(a=a.replace(/([-|\+])/g,""),a.length>8&&a.match(/[^0-9]/g)===null)},jQueryFormUtils.validateSwedishMobileNumber=function(a){if(!jQueryFormUtils.validatePhoneNumber(a))return!1;a=a.replace(/[^0-9]/g,"");var b=a.substring(0,3);return a.length!=10&&b!=="467"?!1:a.length!=11&&b==="467"?!1:/07[0-9{1}]/.test(b)||b==="467"&&a.substr(3,1)==="0"},jQueryFormUtils.validateUKVATNumber=function(a){a=a.replace(/[^0-9]/g,"");if(a.length<9)return!1;var b=!1,c=a.split(""),d=Number(c[7]+c[8]),e=c[0],f=c[1];if(e==0&&f>0)return!1;var g=0;for(var h=0;h<7;h++)g+=c[h]*(8-h);var i=0,h=0;for(var j=8;j>=2;j--)i+=c[h]*j,h++;while(g>0)g-=97;return g=Math.abs(g),d==g&&(b=!0),b||(g%=97,g>=55?g-=55:g+=42,g==d&&(b=!0)),b},jQueryFormUtils.validateBirthdate=function(a,b){var c=this.parseDate(a,b);if(!c)return!1;var d=new Date,e=d.getFullYear(),f=c[0],g=c[1],h=c[2];if(f===e){var i=d.getMonth()+1;if(g===i){var j=d.getDate();return h<=j}return g<i}return f<e&&f>e-124},jQueryFormUtils.ignoreInput=function(a,b,c){if(b==="submit"||b==="button")return!0;for(var d=0;d<c.ignore.length;d++)if(c.ignore[d]===a)return!0;return!1},jQueryFormUtils.parseDate=function(a,b){var c=b.replace(/[a-zA-Z]/gi,"").substring(0,1),d="^",e=b.split(c);for(var f=0;f<e.length;f++)d+=(f>0?"\\"+c:"")+"(\\d{"+e[f].length+"})";d+="$";var g=a.match(new RegExp(d));if(g===null)return!1;var h=function(a,b,c){for(var d=0;d<b.length;d++)if(b[d].substring(0,1)===a)return jQueryFormUtils.parseDateInt(c[d+1]);return-1},i=h("m",e,g),j=h("d",e,g),k=h("y",e,g);return i===2&&j>28&&(k%4!==0||k%100===0&&k%400!==0)||i===2&&j>29&&(k%4===0||k%100!==0&&k%400===0)||i>12||i===0?!1:this.isShortMonth(i)&&j>30||!this.isShortMonth(i)&&j>31||j===0?!1:[k,i,j]},jQueryFormUtils.parseDateInt=function(a){return a.indexOf("0")===0&&(a=a.replace("0","")),parseInt(a,10)},jQueryFormUtils.validateSwedishSecurityNumber=function(a){if(!a.match(/^(\d{4})(\d{2})(\d{2})(\d{4})$/))return!1;var b=RegExp.$1,c=jQueryFormUtils.parseDateInt(RegExp.$2),d=jQueryFormUtils.parseDateInt(RegExp.$3),e=new Array(31,28,31,30,31,30,31,31,30,31,30,31);if(b%400===0||b%4===0&&b%100!==0)e[1]=29;if(c<1||c>12||d<1||d>e[c-1])return!1;a=a.substring(2,a.length);var f="";for(var g=0;g<a.length;g++)f+=((g+1)%2+1)*a.substring(g,g+1);var h=0;for(g=0;g<f.length;g++)h+=parseInt(f.substring(g,g+1),10);return h%10===0},jQueryFormUtils.validateTime=function(a){if(a.match(/^(\d{2}):(\d{2})$/)===null)return!1;var b=parseInt(a.split(":")[0],10),c=parseInt(a.split(":")[1],10);return b>24||c>59||b===24&&c>0?!1:!0},jQueryFormUtils.validateFloat=function(a){return a.match(/^(\-|)([0-9]+)\.([0-9]+)$/)!==null},jQueryFormUtils.validateInteger=function(a){return a!==""&&a.replace(/[0-9]/g,"")===""},jQueryFormUtils.isShortMonth=function(a){return a%2===0&&a<7||a%2!==0&&a>7},jQueryFormUtils.simpleSpamCheck=function(a,b){var c=b.match(/captcha([0-9a-z]+)/i)[1].replace("captcha","");return a===c},jQueryFormUtils.validateDomain=function(a){a=a.replace("ftp://","").replace("https://","").replace("http://","").replace("www.","");var b=new Array(".com",".net",".org",".biz",".coop",".info",".museum",".name",".pro",".edu",".gov",".int",".mil",".ac",".ad",".ae",".af",".ag",".ai",".al",".am",".an",".ao",".aq",".ar",".as",".at",".au",".aw",".az",".ba",".bb",".bd",".be",".bf",".bg",".bh",".bi",".bj",".bm",".bn",".bo",".br",".bs",".bt",".bv",".bw",".by",".bz",".ca",".cc",".cd",".cf",".cg",".ch",".ci",".ck",".cl",".cm",".cn",".co",".cr",".cu",".cv",".cx",".cy",".cz",".de",".dj",".dk",".dm",".do",".dz",".ec",".ee",".eg",".eh",".er",".es",".et",".fi",".fj",".fk",".fm",".fo",".fr",".ga",".gd",".ge",".gf",".gg",".gh",".gi",".gl",".gm",".gn",".gp",".gq",".gr",".gs",".gt",".gu",".gv",".gy",".hk",".hm",".hn",".hr",".ht",".hu",".id",".ie",".il",".im",".in",".io",".iq",".ir",".is",".it",".je",".jm",".jo",".jp",".ke",".kg",".kh",".ki",".km",".kn",".kp",".kr",".kw",".ky",".kz",".la",".lb",".lc",".li",".lk",".lr",".ls",".lt",".lu",".lv",".ly",".ma",".mc",".md",".mg",".mh",".mk",".ml",".mm",".mn",".mo",".mp",".mq",".mr",".ms",".mt",".mu",".mv",".mw",".mx",".my",".mz",".na",".nc",".ne",".nf",".ng",".ni",".nl",".no",".np",".nr",".nu",".nz",".om",".pa",".pe",".pf",".pg",".ph",".pk",".pl",".pm",".pn",".pr",".ps",".pt",".pw",".py",".qa",".re",".ro",".rw",".ru",".sa",".sb",".sc",".sd",".se",".sg",".sh",".si",".sj",".sk",".sl",".sm",".sn",".so",".sr",".st",".sv",".sy",".sz",".tc",".td",".tf",".tg",".th",".tj",".tk",".tm",".tn",".to",".tp",".tr",".tt",".tv",".tw",".tz",".ua",".ug",".uk",".um",".us",".uy",".uz",".va",".vc",".ve",".vg",".vi",".vn",".vu",".ws",".wf",".ye",".yt",".yu",".za",".zm",".zw",".mobi",".xxx"),c=a.lastIndexOf("."),d=a.substring(0,c),e=a.substring(c,a.length),f=!1;for(var g=0;g<b.length;g++)if(b[g]===e){if(e!==".uk"){f=!0;break}var h=a.split("."),i=h[h.length-2],j=new Array("co","me","ac","gov","judiciary","ltd","mod","net","nhs","nic","org","parliament","plc","police","sch","bl","british-library","jet","nls");for(var k=0;k<j.length;k++)if(j[k]===i){f=!0;break}}if(!f)return!1;if(c<2||c>57)return!1;var l=d.substring(0,1),m=d.substring(d.length-1,d.length);return l==="-"||l==="."||m==="-"||m==="."?!1:d.split(".").length>3||d.split("..").length>1?!1:d.replace(/[0-9a-z\.\-]/g,"")!==""?!1:!0},jQueryFormUtils.attrInt=function(a,b){var c=new RegExp("("+b+"[0-9-]+)","g");return a.match(c)[0].replace(/[^0-9\-]/g,"")},jQueryFormUtils.validateInput=function(a,b,c,d){var e=a.val();e=e||"",e instanceof Array||(e=[e]);if(a.get(0).nodeName=="SELECT"&&a.attr("multiple")){var f=a.attr(c.validationRuleAttribute),g=a.attr(c.validationErrorMsgAttribute);if(f.indexOf("validate_num_answers")>-1){var h=jQueryFormUtils.attrInt(f,"num");if(h>e.length)return g||b.badNumberOfSelectedOptionsStart+h+b.badNumberOfSelectedOptionsEnd}}for(var i=0;i<e.length;i++){var j=jQueryFormUtils.validate(e[i],a,b,c,d);if(j!==!0)return j}return!0},jQueryFormUtils.validate=function(a,b,c,d,e){var f=b.attr("data-validation-optional"),g=!1,h=!1,i=b.attr("data-validation-if-checked");if(i!=null){g=!0,e||(e=b.closest("form"));var j=e.find('input[name="'+i+'"]');j.prop("checked")&&(h=!0)}if(!a&&f==="true"||g&&!h)return!0;var k=b.attr(d.validationRuleAttribute),l=b.attr(d.validationErrorMsgAttribute);if(typeof k!="undefined"&&k!==null){if(k.indexOf("required")>-1&&a==="")return l||c.requiredFields;if(k.indexOf("validate_min_length")>-1&&a.length<jQueryFormUtils.attrInt(k,"length"))return l||c.tooShortStart+jQueryFormUtils.attrInt(k,"length")+c.tooShortEnd;if(k.indexOf("validate_max_length")>-1&&a.length>jQueryFormUtils.attrInt(k,"length"))return l||c.tooLongStart+jQueryFormUtils.attrInt(k,"length")+c.tooLongEnd;if(k.indexOf("validate_length")>-1){var m=jQueryFormUtils.attrInt(k,"length").split("-");if(a.length<parseInt(m[0],10)||a.length>parseInt(m[1],10))return l||c.badLength+jQueryFormUtils.attrInt(k,"length")+c.tooLongEnd}if(k.indexOf("validate_email")>-1&&!jQueryFormUtils.validateEmail(a))return l||c.badEmail;if(k.indexOf("validate_domain")>-1&&!jQueryFormUtils.validateDomain(a))return l||c.badDomain;if(k.indexOf("validate_url")>-1&&!jQueryFormUtils.validateUrl(a))return l||c.badUrl;if(k.indexOf("validate_float")>-1&&!jQueryFormUtils.validateFloat(a))return l||c.badFloat;if(k.indexOf("validate_int")>-1&&!jQueryFormUtils.validateInteger(a))return l||c.badInt;if(k.indexOf("validate_time")>-1&&!jQueryFormUtils.validateTime(a))return l||c.badTime;if(k.indexOf("validate_date")>-1&&!jQueryFormUtils.parseDate(a,d.dateFormat))return l||c.badDate;if(k.indexOf("validate_birthdate")>-1&&!jQueryFormUtils.validateBirthdate(a,d.dateFormat))return l||c.badDate;if(k.indexOf("validate_phone")>-1&&!jQueryFormUtils.validatePhoneNumber(a))return l||c.badTelephone;if(k.indexOf("validate_swemobile")>-1&&!jQueryFormUtils.validateSwedishMobileNumber(a))return l||c.badTelephone;if(k.indexOf("validate_spamcheck")>-1&&!jQueryFormUtils.simpleSpamCheck(a,k))return l||c.badSecurityAnswer;if(k.indexOf("validate_ukvatnumber")>-1&&!jQueryFormUtils.validateUKVATNumber(a))return l||c.badUKVatAnswer;if(k.indexOf("validate_custom")>-1&&k.indexOf("regexp/")>-1){var n=new RegExp(k.split("regexp/")[1].split("/")[0]);if(!n.test(a))return l||c.badCustomVal}if(k.indexOf("validate_swesc")>-1&&!jQueryFormUtils.validateSwedishSecurityNumber(a))return l||c.badSecurityNumber;if(k.indexOf("validate_confirmation")>-1&&typeof e!="undefined"){var o="",p=e.find("input[name="+b.attr("name")+"_confirmation]").eq(0);p&&(o=p.val());if(a!==o)return l||c.notConfirmed}}return!0},jQueryFormUtils.LANG={errorTitle:"Form submission failed!",requiredFields:"You have not answered all required fields",badTime:"You have not given a correct time",badEmail:"You have not given a correct e-mail address",badTelephone:"You have not given a correct phone number",badSecurityAnswer:"You have not given a correct answer to the security question",badDate:"You have not given a correct date",tooLongStart:"You have given an answer longer than ",tooLongEnd:" characters",tooShortStart:"You have given an answer shorter than ",tooShortEnd:" characters",badLength:"You have to give an answer between ",notConfirmed:"Values could not be confirmed",badDomain:"Incorrect domain value",badUrl:"Incorrect url value",badFloat:"Incorrect float value",badCustomVal:"You gave an incorrect answer",badInt:"Incorrect integer value",badSecurityNumber:"Your social security number was incorrect",badUKVatAnswer:"Incorrect UK VAT Number",badNumberOfSelectedOptionsStart:"You have to choose at least ",badNumberOfSelectedOptionsEnd:" answers"},jQueryFormUtils.validateUrl=function(a){var b=/^(https|http|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|\[|\]|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$/i;if(b.test(a)){var c=a.split(/^https|^http|^ftp/i)[1].replace("://",""),d=c.indexOf("/");return d>-1&&(c=c.substr(0,d)),jQueryFormUtils.validateDomain(c)}return!1},jQueryFormUtils.lengthRestriction=function(a,b){this.input=a,this.maxLength=parseInt(b.text(),10);var c=this;$(this.input).keyup(function(){$(this).val($(this).val().substring(0,c.maxLength)),b.text(c.maxLength-$(this).val().length)}).focus(function(){$(this).keyup()}).trigger("keyup")};