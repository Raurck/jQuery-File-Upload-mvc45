﻿using System.Web.Optimization;

namespace jQuery_File_Upload_mvc45
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui-{version}.js",
//site.js contains fileuploader initialization, triggered on document complete
                        "~/Scripts/site.js")); 

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));



            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

//fileupload script bundle in right order
            bundles.Add(new ScriptBundle("~/bundles/fileupload").Include(
                        "~/Scripts/jQuery.FileUpload/locale/locale.js",
                        "~/Scripts/jQuery.FileUpload/template/tmpl.min.js",
                        "~/Scripts/jQuery.FileUpload/load-image/load-image.all.min.js",
                        "~/Scripts/jQuery.FileUpload/canvas-to-blob/canvas-to-blob.js",
                        "~/Scripts/jQuery.FileUpload/jquery.iframe-transport.js",
                        "~/Scripts/jQuery.FileUpload/jquery.fileupload.js",
                        "~/Scripts/jQuery.FileUpload/jquery.fileupload-process.js",
                        "~/Scripts/jQuery.FileUpload/jquery.fileupload-image.js",
                        "~/Scripts/jQuery.FileUpload/jquery.fileupload-audio.js",
                        "~/Scripts/jQuery.FileUpload/jquery.fileupload-video.js",
                        "~/Scripts/jQuery.FileUpload/jquery.fileupload-validate.js",
                        "~/Scripts/jQuery.FileUpload/jquery.fileupload-ui.js"
                        ));

//fileupload css bundle in right order
            bundles.Add(new StyleBundle("~/Content/fileupload").Include(
                      "~/Content/jquery.fileupload/css/jquery.fileupload.css",
                      "~/Content/jquery.fileupload/css/jquery.fileupload-ui.css"));

        }
    }
}
