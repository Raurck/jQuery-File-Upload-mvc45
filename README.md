# jQuery-File-Upload-mvc45

This is an example of integration jQuery-File-Upload plugin with an ASP.NET MVC 4.5 application.

It is based on [original plugin demo](https://github.com/blueimp/jQuery-File-Upload/) and [maxpavlov/jQuery-File-Upload.MVC3](https://github.com/maxpavlov/jQuery-File-Upload.MVC3)
 supports batch upload, download and deletion of uploaded files.

NuGet package of jQuery-File-Upload does not contain all nessesary files, so they added to subfolders of /jQuery.FileUpload/.

Demo was made using this plugins from Sebastian Tschan
  jQuery File Upload Plugin 5.42.3
  jQuery Iframe Transport Plugin 1.8.3
  JavaScript Templates
  JavaScript Load Image
  JavaScript Canvas to Blob
  
Demo uses HttpHandler to operate with client requests

Template section in Index.cshtml is vital for upload in this plugin configuration.
