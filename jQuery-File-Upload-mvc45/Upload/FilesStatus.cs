using System;
using System.IO;
using System.Drawing;

namespace jQuery_File_Upload_mvc45.Upload
{
    public class FilesStatus
    {
        
        public const string HandlerPath = "/Upload/";
        const int tmbSize = 80;
        const string tmbSubfolder = "tmb";
        const string imageExtension = ".jpg|.png|.gif";
        const string thumbnailUrlDef = "/Content/img/generalFile.png";

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

            if (File.Exists(imageFileName))
            {

                int trgWidth = 4 * trgHeigth / 3;

                string fileName = Path.Combine(Path.GetDirectoryName(imageFileName), tmbSubfolder, Path.GetFileNameWithoutExtension(imageFileName) + Path.GetExtension(imageFileName));

                if (File.Exists(fileName))
                {
                    return fileName;
                }

                Image img1 = new Bitmap(imageFileName);

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
                    if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(imageFileName), tmbSubfolder)))
                    {
                        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(imageFileName), tmbSubfolder));
                    }
                    using (FileStream fs = File.OpenWrite(fileName))
                    {
                        img2.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }

                img1.Dispose();
                return fileName;
            }
            return imageFileName;
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

            thumbnailUrl = thumbnailUrlDef;

            if (IsImage(Path.GetExtension(fullPath)))
            {
                thumbnailUrl = @"data:image/jpg;base64," + EncodeFile(Scale(fullPath, tmbSize));
            }
        }

        private bool IsImage(string ext)
        {
            bool isImg = false;
            foreach (var extStr in imageExtension.Split('|'))
            {
                isImg = isImg ||( ext.ToLower() == extStr.ToLower());
            }
            return isImg;
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