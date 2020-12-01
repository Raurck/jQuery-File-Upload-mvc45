$(document).ready(function () {
    'use strict';
    // Initialize the jQuery File Upload widget:
    $('#fileupload').fileupload({
        url: '//localhost:9137/upload'
    });

    $('#fileupload').fileupload('option', {
        url: '//localhost:9137/upload',
        maxChunkSize: 2000000,
        maxFileSize: 100000000,
        resizeMaxWidth: 1920,
        resizeMaxHeight: 1200,
        disableImageResize: /Android(?!.*Chrome)|Opera/
            .test(window.navigator.userAgent),
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i
    });

    $('#fileupload').fileupload(
        'option',
        'redirect',
        window.location.href.replace(
            /\/[^\/]*$/,
            '/cors/result.html?%s'
        )
    );

    // Upload server status check for browsers with CORS support:
    $('#fileupload').addClass('fileupload-processing');
    $.ajax({
        // Uncomment the following to send cross-domain cookies:
        //xhrFields: {withCredentials: true},
        url: $('#fileupload').fileupload('option', 'url'),
        dataType: 'json',
        context: $('#fileupload')[0]
    }).always(function () {
        $(this).removeClass('fileupload-processing');
    }).done(function (result) {
        $(this).fileupload('option', 'done')
            .call(this, $.Event('done'), { result: result });
    });
 
});
