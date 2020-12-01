using System.Collections.Generic;
using JetBrains.Annotations;

namespace jQuery_File_Upload_mvc45.Infrastructure
{
    [PublicAPI]
    public class FileUploadResult
    {
        public List<FilesStatus> Files { get; set; }
    }
}