using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace jQuery_File_Upload_mvc45.Infrastructure
{
    [PublicAPI]
    public class FilesStatus
    {
        private const string KnownDefaultImageUrl = "/Content/img/generalFile.png";
        private readonly FileInfo fileInfo;
        private readonly string handlerPath;
        private readonly int tmbSize;
        private readonly string tmbSubFolder;
        private readonly string defaultImageUrl;
        private const int TmbSize = 80;
        private const string TmbSubfolder = "tmb";

        public string Name { get; private set; }
        public long Size { get; private set; }
        public string Url { get; private set; }
        public string ThumbnailUrl { get; private set; }
        public string DeleteUrl { get; private set; }
        public string DeleteType { get; private set; }

        public static Task<FilesStatus> GetFileStatus(
            FileInfo fileInfo,
            string handlerPath = "/Upload/",
            int tmbSize = TmbSize,
            string tmbSubFolder = TmbSubfolder,
            string defaultImageUrl = KnownDefaultImageUrl)
            => new FilesStatus(fileInfo, handlerPath, tmbSize, tmbSubFolder, defaultImageUrl).Init();

        public static Task<FilesStatus> GetFileStatus(
            string imagePath,
            string handlerPath = "/Upload/",
            int tmbSize = TmbSize,
            string tmbSubFolder = TmbSubfolder,
            string defaultImageUrl = KnownDefaultImageUrl)
            => File.Exists(imagePath)
                ? GetFileStatus(new FileInfo(imagePath), handlerPath, tmbSize, tmbSubFolder, defaultImageUrl)
                : Task.FromResult<FilesStatus>(null);

        private FilesStatus(
            FileInfo fileInfo,
            string handlerPath = "/Upload/",
            int tmbSize = TmbSize,
            string tmbSubFolder = TmbSubfolder,
            string defaultImageUrl = KnownDefaultImageUrl)
        {
            this.fileInfo = fileInfo;
            this.handlerPath = handlerPath;
            this.tmbSize = tmbSize;
            this.tmbSubFolder = tmbSubFolder;
            this.defaultImageUrl = defaultImageUrl;
        }

        private string GetTmbFile([NotNull] string imageFileName, int targetHeight)
        {
            var targetWidth = 4*targetHeight/3;

            var thumbnailImageFileName = Path.Combine(
                Path.GetDirectoryName(imageFileName) ?? "./",
                tmbSubFolder,
                Path.GetFileNameWithoutExtension(imageFileName) + Path.GetExtension(imageFileName));

            if (File.Exists(thumbnailImageFileName))
                return thumbnailImageFileName;

            using (var originalImage = new Bitmap(imageFileName))
            {
                var cropRect = GetCropRect(originalImage);
                using (var croppedImage = originalImage.Clone(cropRect, originalImage.PixelFormat))
                {
                    using (var thumbnailImage = croppedImage.GetThumbnailImage(targetWidth, targetHeight, () => true, new IntPtr()))
                    {
                        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(imageFileName) ?? "./", TmbSubfolder));
                        using (var fs = File.OpenWrite(thumbnailImageFileName))
                        {
                            thumbnailImage.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }

                        return thumbnailImageFileName;
                    }
                }
            }
        }

        private static Rectangle GetCropRect(Bitmap originalImage)
        {
            var height = 3*originalImage.Width/4;
            var width = 4*originalImage.Height/3;

            if ((originalImage.Height - height)/(float) originalImage.Height > 0.03)
                return new Rectangle(0, (originalImage.Height - height)/2, originalImage.Width, height);
            if ((originalImage.Height - height)/(float) originalImage.Height < -0.03)
                return new Rectangle((originalImage.Width - width)/2, 0, width, originalImage.Height);

            return new Rectangle(0, 0, originalImage.Width, originalImage.Height);
        }

        private async Task<FilesStatus> Init()
        {
            Name = fileInfo.Name;
            Size = fileInfo.Length;
            Url = handlerPath + "?f=" + Name;
            DeleteUrl = handlerPath + "?f=" + Name;
            DeleteType = "DELETE";
            ThumbnailUrl = defaultImageUrl;

            if (await ImageDetector.GetKnownFileType(fileInfo.FullName).ConfigureAwait(false) != FileType.Unknown)
            {
                ThumbnailUrl = @"data:image/jpg;base64," + await GetEncodedFileContent(GetTmbFile(fileInfo.FullName, tmbSize)).ConfigureAwait(false);
            }

            return this;
        }

        private static async Task<string> GetEncodedFileContent(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, (int) stream.Length).ConfigureAwait(false);
                return Convert.ToBase64String(buffer);
            }
        }
    }
}