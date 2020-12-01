using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace jQuery_File_Upload_mvc45.Infrastructure
{
    public static class ImageDetector
    {
        private static readonly Dictionary<FileType, byte[]> KnownFileHeaders;
        private static readonly int MaxHeaderLength;

        static ImageDetector()
        {
            KnownFileHeaders = new Dictionary<FileType, byte[]>
            {
                {FileType.Jpeg, new byte[] {0xFF, 0xD8}}, // JPEG
                {FileType.Bmp, new byte[] {0x42, 0x4D}}, // BMP
                {FileType.Gif, new byte[] {0x47, 0x49, 0x46}}, // GIF
                {FileType.Png, new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A}}, // PNG
                {FileType.Pdf, new byte[] {0x25, 0x50, 0x44, 0x46}} // PDF
            };
            MaxHeaderLength = KnownFileHeaders.Values.Max(v => v.Length);
        }

        private static bool IsType(ReadOnlySpan<byte> data, KeyValuePair<FileType, byte[]> checkingType)
        {
            if (data.Length < checkingType.Value.Length)
                return false;
            var slice = data.Slice(0, checkingType.Value.Length);
            return slice.SequenceEqual(checkingType.Value);
        }

        public static async Task<FileType> GetKnownFileType(string path)
        {
            if (!File.Exists(path))
                return FileType.Unknown;
            using (var streamOpenRead = File.OpenRead(path))
            {
                var buffer = new byte[MaxHeaderLength];
                var readCount = await streamOpenRead.ReadAsync(buffer, 0, MaxHeaderLength).ConfigureAwait(false);
                return GetKnownFileType(new ReadOnlySpan<byte>(buffer, 0, readCount));
            }
        }

        private static FileType GetKnownFileType(ReadOnlySpan<byte> data)
        {
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var check in KnownFileHeaders)
            {
                if (IsType(data, check))
                    return check.Key;
            }
            return FileType.Unknown;
        }
    }
}