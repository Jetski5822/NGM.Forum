(function ($) {
    $(".create").live("click",
        function (e) {
            e.preventDefault();
            var currentUrl = $(this).attr('href');

            $.ajax({
                url: currentUrl,
                success: function (data) {
                    var str = $(data).find(".zone-content")
                    $(".zone-content").append(str);
                }
            });
        });
})(jQuery);