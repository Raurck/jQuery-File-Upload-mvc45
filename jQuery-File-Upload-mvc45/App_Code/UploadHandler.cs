using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using jQuery_File_Upload_mvc45.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace jQuery_File_Upload_mvc45
{
    public class UploadHandler : HttpTaskAsyncHandler
    {
        /// <summary>
        ///   You will need to configure this handler in the Web.config file of your
        ///   web and register it with IIS before being able to use it. For more information
        ///   see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>

        #region IHttpHandler Members

        private const string HttpVerbGet = "GET";
        private const string HttpVerbPost = "POST";
        private const string HttpVerbPut = "PUT";
        private const string HttpVerbHead = "HEAD";
        private const string HttpVerbOptions = "OPTIONS";
        private const string HttpVerbDelete = "DELETE";
        private const string ContentRangeHeader = "Content-Range";
        private const string ContentDispositionHeader = "Content-Disposition";
        private const string StoragePath = "~/Files/";
        private const string StrChunkError = "Attempt to upload chunked file containing no or more than one fragment per request";
        private const string StrStreamError = "Upload request contains no file input stream";
        private const string QueryFileNameParameter = "f";
        private static string StorageRoot => Path.Combine(HttpContext.Current.Server.MapPath(StoragePath)); //Path should! always end with '/'

        public override Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.AddHeader("Pragma", "no-cache");
            context.Response.AddHeader("Cache-Control", "private, no-cache");

            return HandleMethod(context);
        }

        private static JsonSerializerSettings JsonSettings =>
            new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            };

        // Handle request based on method
        private async Task HandleMethod(HttpContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case HttpVerbHead:
                case HttpVerbGet:
                    if (GivenFilename(context)) DeliverFile(context);
                    else await ListCurrentFiles(context).ConfigureAwait(false);
                    break;

                case HttpVerbPost:
                case HttpVerbPut:
                    await UploadFile(context).ConfigureAwait(false);
                    break;

                case HttpVerbDelete:
                    DeleteFile(context);
                    break;

                case HttpVerbOptions:
                    ReturnOptions(context);
                    break;

                default:
                    context.Response.ClearHeaders();
                    context.Response.StatusCode = (int) HttpStatusCode.MethodNotAllowed;
                    break;
            }
        }

        private static void ReturnOptions(HttpContext context)
        {
            context.Response.AddHeader("Allow", $"{HttpVerbOptions},{HttpVerbHead},{HttpVerbGet},{HttpVerbPost},{HttpVerbPut},{HttpVerbDelete}");
            context.Response.StatusCode = 200;
        }

        // Delete file from the server
        private static void DeleteFile(HttpContext context)
        {
            var filePath = StorageRoot + context.Request[QueryFileNameParameter];

            if (File.Exists(filePath))
                File.Delete(filePath);

            var deleteResult = new FileDeleteResult
            {
                Files = new Dictionary<string, bool>
                {
                    [context.Request[QueryFileNameParameter]] = true
                }
            };
            AddResponseJsonHeaders(context);
            context.Response.Write(JsonConvert.SerializeObject(deleteResult, JsonSettings));
        }

        // Upload file to the server
        private async Task UploadFile(HttpContext context)
        {
            var statuses = new List<FilesStatus>();
            var headers = context.Request.Headers;
            if (IsWholeFileUploadRequest(headers, out var contentRangeHeader))
                await UploadWholeFile(context, statuses).ConfigureAwait(false);
            else if (IsPartialFileUploadRequest(headers, out var contentDispositionHeader))
            {
                await UploadPartialFile(
                        (contentDispositionHeader.FileNameStar ?? contentDispositionHeader.FileName).Replace("\"", string.Empty),
                        contentRangeHeader,
                        context,
                        statuses)
                    .ConfigureAwait(false);
            }

            WriteJsonIframeSafe(context, statuses);
        }

        private static bool IsPartialFileUploadRequest(NameValueCollection headers, out ContentDispositionHeaderValue contentDispositionHeader)
        {
            return ContentDispositionHeaderValue.TryParse(headers[ContentDispositionHeader], out contentDispositionHeader)
                   && !string.IsNullOrWhiteSpace(contentDispositionHeader.FileName);
        }

        private static bool IsWholeFileUploadRequest(NameValueCollection headers, out ContentRangeHeaderValue contentRangeHeader)
        {
            return !ContentRangeHeaderValue.TryParse(headers[ContentRangeHeader], out contentRangeHeader)
                   || contentRangeHeader.From == 0 && contentRangeHeader.To == contentRangeHeader.Length;
        }

        // Upload partial file
        private static async Task UploadPartialFile(string fileName, ContentRangeHeaderValue rangeHeader, HttpContext context, ICollection<FilesStatus> statuses)
        {
            if (context.Request.Files.Count != 1)
                throw new HttpRequestValidationException(StrChunkError);
            var inputStream = context.Request.Files[0]?.InputStream;
            if (inputStream == null)
                throw new HttpRequestValidationException(StrStreamError);

            var fullName = Path.Combine(StorageRoot, fileName);
            using (var fs = new FileStream(fullName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Seek(rangeHeader.From ?? 0, SeekOrigin.Begin);
                await inputStream.CopyToAsync(fs).ConfigureAwait(false);
            }

            if (IsLastChunkUploaded(rangeHeader))
                statuses.Add(await FilesStatus.GetFileStatus(new FileInfo(fullName)).ConfigureAwait(false));
        }

        private static bool IsLastChunkUploaded(ContentRangeHeaderValue rangeHeader)
        {
            return rangeHeader.To == rangeHeader.Length - 1;
        }

        // Upload entire file
        private static async Task UploadWholeFile(HttpContext context, List<FilesStatus> statuses)
        {
            var prepareResults = context.Request.Files.AllKeys
                .Select(fileName => context.Request.Files[fileName])
                .Where(file => file != null)
                .Select(
                    file =>
                    {
                        var fullPath = StorageRoot + file.FileName;
                        file.SaveAs(fullPath);
                        return FilesStatus.GetFileStatus(fullPath);
                    })
                .ToArray();
            await Task.WhenAll(prepareResults).ConfigureAwait(false);
            statuses.AddRange(prepareResults.Select(t => t.Result));
        }

        private static void AddResponseJsonHeaders(HttpContext context)
        {
            context.Response.AddHeader("Vary", "Accept");
            try
            {
                if (context.Request["HTTP_ACCEPT"].Contains("application/json"))
                {
                    context.Response.ContentType = "application/json";
                    context.Response.AddHeader("Pragma", "no-cache");
                    context.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate");
                    context.Response.AddHeader("X-Content-Type-Options", "nosniff");
                }
                else
                    context.Response.ContentType = "text/plain";
            }
            catch
            {
                context.Response.ContentType = "text/plain";
            }
        }

        private static void WriteJsonIframeSafe(HttpContext context, List<FilesStatus> statuses)
        {
            AddResponseJsonHeaders(context);
            var result = new FileUploadResult
            {
                Files = statuses
            };
            context.Response.Write(JsonConvert.SerializeObject(result, JsonSettings));
        }

        private static bool GivenFilename(HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Request[QueryFileNameParameter]);
        }

        private static void DeliverFile(HttpContext context)
        {
            var filename = context.Request[QueryFileNameParameter];
            var filePath = StorageRoot + filename;

            if (File.Exists(filePath))
            {
                context.Response.AddHeader("X-Content-Type-Options", "nosniff");
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + filename + "\"");
                context.Response.ContentType = "application/octet-stream";
                context.Response.ClearContent();
                context.Response.WriteFile(filePath);
            }
            else
                context.Response.StatusCode = 404;
        }

        private static async Task ListCurrentFiles(HttpContext context)
        {
            var files =
                new DirectoryInfo(StorageRoot)
                    .GetFiles("*", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
                    .Select(f => FilesStatus.GetFileStatus(f))
                    .ToArray();
            await Task.WhenAll(files).ConfigureAwait(false);
            context.Response.AddHeader("Content-Disposition", "inline; filename=\"files.json\"");
            context.Response.Write(JsonConvert.SerializeObject(files, JsonSettings));
            context.Response.ContentType = "application/json";
        }

        #endregion
    }
}