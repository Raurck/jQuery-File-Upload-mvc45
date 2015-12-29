using System;
using System.IO;
using System.Drawing;
using System.IO;

namespace jQuery_File_Upload_mvc45.Upload
{
    public class FilesStatus
    {
        
        public const string HandlerPath = "/Upload/";

        //public string group { get; set; }
        public string name { get; set; }
        //public string type { get; set; }
        public int size { get; set; }
        //public string progress { get; set; }
        public string url { get; set; }
        public string thumbnailUrl { get; set; }
        public string deleteUrl { get; set; }
        public string deleteType { get; set; }
        //public string error { get; set; }

        public FilesStatus() { }

        public bool ThumbnailCallback()
        {
            return true;
        }
        private string Scale(string imageFileName, int trgHeigth)
        {
            Image.GetThumbnailImageAbort callback =
                              new Image.GetThumbnailImageAbort(ThumbnailCallback);

            if (System.IO.File.Exists(imageFileName))
            {

                int trgWidth = 4 * trgHeigth / 3;

                Image img1 = new Bitmap(imageFileName);

                string fileName = Path.Combine(Path.GetDirectoryName(imageFileName),"tmb", Path.GetFileNameWithoutExtension(imageFileName) + Path.GetExtension(imageFileName));

                if (System.IO.File.Exists(fileName))
                {
                    return fileName;
                }
                int h = 3 * img1.Width / 4;
                int w = 4 * img1.Height / 3;

                Rectangle cropRect = new Rectangle(0, 0, img1.Width, img1.Height);
                if ((img1.Height - h) / (float)img1.Height > 0.03)
                {
                    cropRect = new Rectangle(0, (img1.Height - h) / 2, img1.Width, h);
                }
                else if (((img1.Height - h) / (float)img1.Height) < -0.03)
                {
                    cropRect = new Rectangle((img1.Width - w) / 2, 0, w, img1.Height);
                }

                img1 = (img1 as Bitmap).Clone(cropRect, img1.PixelFormat);

                using (Image img2 = img1.GetThumbnailImage(trgWidth, trgHeigth, callback, new IntPtr()))
                {
                    if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(imageFileName), "tmb")))
                    {
                        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(imageFileName), "tmb"));
                    }
                    using (FileStream fs = System.IO.File.OpenWrite(fileName))
                    {
                        img2.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }

                return fileName;
            }
            return null;
        }

        public FilesStatus(FileInfo fileInfo)
        {
            SetValues(fileInfo.Name, (int)fileInfo.Length, fileInfo.FullName);
        }

        public FilesStatus(string fileName, int fileLength, string fullPath)
        {
            SetValues(fileName, fileLength, fullPath);
        }

        private void SetValues(string fileName, int fileLength, string fullPath)
        {
            name = fileName;
            //type = "image/png";
            size = fileLength;
            //progress = "1.0";
            url = HandlerPath + "?f=" + fileName;
            deleteUrl = HandlerPath + "?f=" + fileName;
            deleteType = "DELETE";

            var ext = Path.GetExtension(fullPath);

            var fileSize = ConvertBytesToMegabytes(new FileInfo(fullPath).Length);
            if (fileSize > 0.5 || !IsImage(ext)) thumbnailUrl = "/Content/img/generalFile.png";
            else thumbnailUrl = @"data:image/png;base64," + EncodeFile(Scale(fullPath,80));
        }

        private bool IsImage(string ext)
        {
            return ext == ".gif" || ext == ".jpg" || ext == ".png";
        }

        private string EncodeFile(string fileName)
        {
            return Convert.ToBase64String(File.ReadAllBytes(fileName));
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    
}
}