$(document).ready(function () {
    'use strict';
    // Initialize the jQuery File Upload widget:
    $('#fileupload').fileupload(

);

    $('#fileupload').fileupload('option', {
        maxFileSize: 500000000,
        resizeMaxWidth: 1920,
        resizeMaxHeight: 1200
    });
});
