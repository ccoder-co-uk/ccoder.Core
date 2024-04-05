using System.Collections.Generic;
using System.Linq;

namespace cCoder.Core.Objects
{
    public struct Mapping
    {
        public string FileExtension { get; set; }
        public string MimeType { get; set; }
    }


    public static class MimeType
    {
        public static IEnumerable<Mapping> All { get; } = new List<Mapping>()
        {
            new Mapping { FileExtension = "au", MimeType = "audio/basic" },
            new Mapping { FileExtension = "avi", MimeType = "video/avi" },
            new Mapping { FileExtension = "bin", MimeType = "application/octet-stream" },
            new Mapping { FileExtension = "bm", MimeType = "image/bmp" },
            new Mapping { FileExtension = "bmp", MimeType = "image/bmp" },
            new Mapping { FileExtension = "boo", MimeType = "application/book" },
            new Mapping { FileExtension = "book", MimeType = "application/book" },
            new Mapping { FileExtension = "boz", MimeType = "application/x-bzip2" },
            new Mapping { FileExtension = "bsh", MimeType = "application/x-bsh" },
            new Mapping { FileExtension = "bz", MimeType = "application/x-bzip" },
            new Mapping { FileExtension = "bz2", MimeType = "application/x-bzip2" },
            new Mapping { FileExtension = "c", MimeType = "text/plain" },
            new Mapping { FileExtension = "cat", MimeType = "application/vnd.ms-pki.seccat" },
            new Mapping { FileExtension = "cc", MimeType = "text/plain" },
            new Mapping { FileExtension = "ccad", MimeType = "application/clariscad" },
            new Mapping { FileExtension = "cco", MimeType = "application/x-cocoa" },
            new Mapping { FileExtension = "cdf", MimeType = "application/cdf" },
            new Mapping { FileExtension = "cer", MimeType = "application/pkix-cert" },
            new Mapping { FileExtension = "cha", MimeType = "application/x-chat" },
            new Mapping { FileExtension = "chat", MimeType = "application/x-chat" },
            new Mapping { FileExtension = "class", MimeType = "application/java" },
            new Mapping { FileExtension = "com", MimeType = "text/plain" },
            new Mapping { FileExtension = "css", MimeType = "text/css" },
            new Mapping { FileExtension = "def", MimeType = "text/plain" },
            new Mapping { FileExtension = "dir", MimeType = "application/x-director" },
            new Mapping { FileExtension = "dl", MimeType = "video/dl" },
            new Mapping { FileExtension = "doc", MimeType = "application/msword" },
            new Mapping { FileExtension = "dot", MimeType = "application/msword" },
            new Mapping { FileExtension = "dp", MimeType = "application/commonground" },
            new Mapping { FileExtension = "dump", MimeType = "application/octet-stream" },
            new Mapping { FileExtension = "dvi", MimeType = "application/x-dvi" },
            new Mapping { FileExtension = "exe", MimeType = "application/octet-stream" },
            new Mapping { FileExtension = "f", MimeType = "text/plain" },
            new Mapping { FileExtension = "gif", MimeType = "image/gif" },
            new Mapping { FileExtension = "gzip", MimeType = "application/x-gzip" },
            new Mapping { FileExtension = "html", MimeType = "text/html" },
            new Mapping { FileExtension = "htmls", MimeType = "text/html" },
            new Mapping { FileExtension = "ico", MimeType = "image/x-icon" },
            new Mapping { FileExtension = "imap", MimeType = "application/x-httpd-imap" },
            new Mapping { FileExtension = "java", MimeType = "text/plain" },
            new Mapping { FileExtension = "jpeg", MimeType = "image/jpeg" },
            new Mapping { FileExtension = "jpg", MimeType = "image/jpeg" },
            new Mapping { FileExtension = "js", MimeType = "text/javascript" },
            new Mapping { FileExtension = "log", MimeType = "text/plain" },
            new Mapping { FileExtension = "mime", MimeType = "www/mime" },
            new Mapping { FileExtension = "mov", MimeType = "video/quicktime" },
            new Mapping { FileExtension = "movie", MimeType = "video/x-sgi-movie" },
            new Mapping { FileExtension = "mp2", MimeType = "audio/mpeg" },
            new Mapping { FileExtension = "mp3", MimeType = "audio/mpeg3" },
            new Mapping { FileExtension = "mpeg", MimeType = "video/mpeg" },
            new Mapping { FileExtension = "mpg", MimeType = "audio/mpeg" },
            new Mapping { FileExtension = "o", MimeType = "application/octet-stream" },
            new Mapping { FileExtension = "pdf", MimeType = "application/pdf" },
            new Mapping { FileExtension = "pic", MimeType = "image/pict" },
            new Mapping { FileExtension = "pict", MimeType = "image/pict" },
            new Mapping { FileExtension = "psd", MimeType = "application/octet-stream" },
            new Mapping { FileExtension = "pwz", MimeType = "application/vnd.ms-powerpoint" },
            new Mapping { FileExtension = "rgb", MimeType = "image/x-rgb" },
            new Mapping { FileExtension = "rt", MimeType = "text/richtext" },
            new Mapping { FileExtension = "sprite", MimeType = "application/x-sprite" },
            new Mapping { FileExtension = "text", MimeType = "text/plain" },
            new Mapping { FileExtension = "tiff", MimeType = "image/tiff" },
            new Mapping { FileExtension = "txt", MimeType = "text/plain" },
            new Mapping { FileExtension = "wav", MimeType = "audio/wav" },
            new Mapping { FileExtension = "word", MimeType = "application/msword" },
            new Mapping { FileExtension = "wri", MimeType = "application/mswrite" },
            new Mapping { FileExtension = "xl", MimeType = "application/excel" },
            new Mapping { FileExtension = "xla", MimeType = "application/excel" },
            new Mapping { FileExtension = "xlb", MimeType = "application/excel" },
            new Mapping { FileExtension = "xlc", MimeType = "application/excel" },
            new Mapping { FileExtension = "xld", MimeType = "application/excel" },
            new Mapping { FileExtension = "xlk", MimeType = "application/excel" },
            new Mapping { FileExtension = "xll", MimeType = "application/excel" },
            new Mapping { FileExtension = "xlm", MimeType = "application/excel" },
            new Mapping { FileExtension = "xls", MimeType = "application/excel" },
            new Mapping { FileExtension = "xlsx", MimeType = "application/excel" },
            new Mapping { FileExtension = "xlt", MimeType = "application/excel" },
            new Mapping { FileExtension = "xlv", MimeType = "application/excel" },
            new Mapping { FileExtension = "xlw", MimeType = "application/excel" },
            new Mapping { FileExtension = "xm", MimeType = "audio/xm" },
            new Mapping { FileExtension = "xml", MimeType = "application/xml" },
            new Mapping { FileExtension = "xml", MimeType = "text/xml" },
            new Mapping { FileExtension = "png", MimeType = "image/png" },
            new Mapping { FileExtension = "zip", MimeType = "application/zip" },
            new Mapping { FileExtension = "zoo", MimeType = "application/octet-stream" },
            new Mapping { FileExtension = "json", MimeType = "application/json" },
            new Mapping { FileExtension = "svg", MimeType = "image/svg+xml" }
        };

        public static Mapping? Get(string fileExtension) => 
            All.Any(m => m.FileExtension == fileExtension.ToLower()) 
                ? All.FirstOrDefault(m => m.FileExtension == fileExtension.ToLower()) 
                : null;
    }
}