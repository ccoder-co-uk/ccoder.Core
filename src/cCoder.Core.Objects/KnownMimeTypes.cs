namespace cCoder.Core.Objects;

public struct Mapping
{
    public string FileExtension { get; set; }
    public string MimeType { get; set; }
}


public static class MimeType
{
    public static IEnumerable<Mapping> All { get; } = new List<Mapping>()
    {
        new() { FileExtension = "au", MimeType = "audio/basic" },
        new() { FileExtension = "avi", MimeType = "video/avi" },
        new() { FileExtension = "bin", MimeType = "application/octet-stream" },
        new() { FileExtension = "bm", MimeType = "image/bmp" },
        new() { FileExtension = "bmp", MimeType = "image/bmp" },
        new() { FileExtension = "boo", MimeType = "application/book" },
        new() { FileExtension = "book", MimeType = "application/book" },
        new() { FileExtension = "boz", MimeType = "application/x-bzip2" },
        new() { FileExtension = "bsh", MimeType = "application/x-bsh" },
        new() { FileExtension = "bz", MimeType = "application/x-bzip" },
        new() { FileExtension = "bz2", MimeType = "application/x-bzip2" },
        new() { FileExtension = "c", MimeType = "text/plain" },
        new() { FileExtension = "cat", MimeType = "application/vnd.ms-pki.seccat" },
        new() { FileExtension = "cc", MimeType = "text/plain" },
        new() { FileExtension = "ccad", MimeType = "application/clariscad" },
        new() { FileExtension = "cco", MimeType = "application/x-cocoa" },
        new() { FileExtension = "cdf", MimeType = "application/cdf" },
        new() { FileExtension = "cer", MimeType = "application/pkix-cert" },
        new() { FileExtension = "cha", MimeType = "application/x-chat" },
        new() { FileExtension = "chat", MimeType = "application/x-chat" },
        new() { FileExtension = "class", MimeType = "application/java" },
        new() { FileExtension = "com", MimeType = "text/plain" },
        new() { FileExtension = "css", MimeType = "text/css" },
        new() { FileExtension = "def", MimeType = "text/plain" },
        new() { FileExtension = "dir", MimeType = "application/x-director" },
        new() { FileExtension = "dl", MimeType = "video/dl" },
        new() { FileExtension = "doc", MimeType = "application/msword" },
        new() { FileExtension = "dot", MimeType = "application/msword" },
        new() { FileExtension = "dp", MimeType = "application/commonground" },
        new() { FileExtension = "dump", MimeType = "application/octet-stream" },
        new() { FileExtension = "dvi", MimeType = "application/x-dvi" },
        new() { FileExtension = "exe", MimeType = "application/octet-stream" },
        new() { FileExtension = "f", MimeType = "text/plain" },
        new() { FileExtension = "gif", MimeType = "image/gif" },
        new() { FileExtension = "gzip", MimeType = "application/x-gzip" },
        new() { FileExtension = "html", MimeType = "text/html" },
        new() { FileExtension = "htmls", MimeType = "text/html" },
        new() { FileExtension = "ico", MimeType = "image/x-icon" },
        new() { FileExtension = "imap", MimeType = "application/x-httpd-imap" },
        new() { FileExtension = "java", MimeType = "text/plain" },
        new() { FileExtension = "jpeg", MimeType = "image/jpeg" },
        new() { FileExtension = "jpg", MimeType = "image/jpeg" },
        new() { FileExtension = "js", MimeType = "text/javascript" },
        new() { FileExtension = "log", MimeType = "text/plain" },
        new() { FileExtension = "mime", MimeType = "www/mime" },
        new() { FileExtension = "mov", MimeType = "video/quicktime" },
        new() { FileExtension = "movie", MimeType = "video/x-sgi-movie" },
        new() { FileExtension = "mp2", MimeType = "audio/mpeg" },
        new() { FileExtension = "mp3", MimeType = "audio/mpeg3" },
        new() { FileExtension = "mpeg", MimeType = "video/mpeg" },
        new() { FileExtension = "mpg", MimeType = "audio/mpeg" },
        new() { FileExtension = "o", MimeType = "application/octet-stream" },
        new() { FileExtension = "pdf", MimeType = "application/pdf" },
        new() { FileExtension = "pic", MimeType = "image/pict" },
        new() { FileExtension = "pict", MimeType = "image/pict" },
        new() { FileExtension = "psd", MimeType = "application/octet-stream" },
        new() { FileExtension = "pwz", MimeType = "application/vnd.ms-powerpoint" },
        new() { FileExtension = "rgb", MimeType = "image/x-rgb" },
        new() { FileExtension = "rt", MimeType = "text/richtext" },
        new() { FileExtension = "sprite", MimeType = "application/x-sprite" },
        new() { FileExtension = "text", MimeType = "text/plain" },
        new() { FileExtension = "tiff", MimeType = "image/tiff" },
        new() { FileExtension = "txt", MimeType = "text/plain" },
        new() { FileExtension = "wav", MimeType = "audio/wav" },
        new() { FileExtension = "word", MimeType = "application/msword" },
        new() { FileExtension = "wri", MimeType = "application/mswrite" },
        new() { FileExtension = "xl", MimeType = "application/excel" },
        new() { FileExtension = "xla", MimeType = "application/excel" },
        new() { FileExtension = "xlb", MimeType = "application/excel" },
        new() { FileExtension = "xlc", MimeType = "application/excel" },
        new() { FileExtension = "xld", MimeType = "application/excel" },
        new() { FileExtension = "xlk", MimeType = "application/excel" },
        new() { FileExtension = "xll", MimeType = "application/excel" },
        new() { FileExtension = "xlm", MimeType = "application/excel" },
        new() { FileExtension = "xls", MimeType = "application/excel" },
        new() { FileExtension = "xlsx", MimeType = "application/excel" },
        new() { FileExtension = "xlt", MimeType = "application/excel" },
        new() { FileExtension = "xlv", MimeType = "application/excel" },
        new() { FileExtension = "xlw", MimeType = "application/excel" },
        new() { FileExtension = "xm", MimeType = "audio/xm" },
        new() { FileExtension = "xml", MimeType = "application/xml" },
        new() { FileExtension = "xml", MimeType = "text/xml" },
        new() { FileExtension = "png", MimeType = "image/png" },
        new() { FileExtension = "zip", MimeType = "application/zip" },
        new() { FileExtension = "zoo", MimeType = "application/octet-stream" },
        new() { FileExtension = "json", MimeType = "application/json" },
        new() { FileExtension = "svg", MimeType = "image/svg+xml" }
    };

    public static Mapping? Get(string fileExtension) => 
        All.Any(m => m.FileExtension == fileExtension.ToLower()) 
            ? All.FirstOrDefault(m => m.FileExtension == fileExtension.ToLower()) 
            : null;
}