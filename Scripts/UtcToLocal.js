$(document).ready(function () {

    //use the langauge of the page.  if its like en-US then try en-US first and if not en.
    var langSpecifier = $('html')[0].lang;
    
    var langFound = false;
    if (langSpecifier) {
        langSpecifier = langSpecifier.toLowerCase()
        var baseLang = "";
        moment.lang(langSpecifier); //en-us
        if (moment.lang() != langSpecifier) {
            var langParts = langSpecifier.split('-');
            if (langParts.length > 1) {
                baseLang = langParts[0];
                moment.lang(baseLang); //en
            }
        }
        langFound = (moment.lang() == langSpecifier || moment.lang() == baseLang);
    }
    
    $(".utc-date-time").each(function () {        
        
        var utcDate = $(this).html();
        var momentDate;
        if (langFound) {
            //use a date with words in the selected language like Tue, Feb 25 2020 10:09 PM
            momentDate = moment(utcDate).format('llll');
        } else {
            //no translation exists so use a number only format in the more universal day/month/year format
            momentDate = moment(utcDate).format('D/M/YYYY hh:mm');  
        }
        $(this).html(momentDate);
    });

});