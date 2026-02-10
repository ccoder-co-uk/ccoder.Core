using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Extensions;
using System.IO.Compression;
using File = cCoder.Core.Objects.Entities.DMS.File;

namespace cCoder.Core.Objects.Extensions;

public static class ZipArchiveExtensions
{
    public static async Task<T[]> DeserializeAsync<T>(this ZipArchiveEntry entry) => Data.ParseJson<T[]>(await new StreamReader(entry.Open()).ReadToEndAsync());

    /// <summary>
    /// Adds the given DMS folder to the given zip archive
    /// </summary>
    /// <param name="zip"></param>
    /// <param name="folder"></param>
    /// <param name="ctx"></param>
    /// <param name="prefix"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    public static ZipArchive AddFolder(this ZipArchive zip, Folder folder, IDataContext ctx = null, string prefix = null, string search = "")
    {
        ICollection<Folder> folders = ctx != null 
            ? ctx.GetAll<Folder>(false)
                .Where(f => f.ParentId == folder.Id)
                .ToArray() 
            : folder.SubFolders;

        ICollection<File> files = ctx != null 
            ? ctx.GetAll<File>(false)
                .Where(f => f.FolderId == folder.Id)
                .ToArray() 
            : folder.Files;

        string entryName = prefix == null 
            ? $"{folder.Name}/" 
            : $"{prefix}{folder.Name}/";

        _ = zip.CreateEntry(entryName, CompressionLevel.Optimal);
        folders.ForEach(f => zip.AddFolder(f, ctx, entryName));

        files.ForEach(f => 
        {
            if (search.IsNullOrEmpty() || entryName.Contains(search))
                zip.AddFile(f, ctx, entryName);
        });

        return zip;
    }

    /// <summary>
    /// Adds the given DMS file to the given zip archve
    /// </summary>
    /// <param name="zip"></param>
    /// <param name="file"></param>
    /// <param name="ctx"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static ZipArchive AddFile(this ZipArchive zip, File file, IDataContext ctx = null, string prefix = null)
    {
        string entryName = prefix != null 
            ? $"{prefix}{file.Name}" 
            : file.Name;

        byte[] rawBytes = (ctx != null ? ctx.GetAll<FileContent>(false) : file.Contents.AsQueryable())
            .OrderByDescending(fc => fc.Version)
            .Where(fc => fc.FileId == file.Id)
            .Select(fc => fc.RawData)
            .FirstOrDefault();

        if (rawBytes != null)
        {
            using Stream s = zip.CreateEntry(entryName, CompressionLevel.Optimal).Open();
            s.Write(rawBytes, 0, rawBytes.Length);
        }

        return zip;
    }

    public static void AddTextFile(this ZipArchive zip, string path, string text)
    {
        using StreamWriter s = new(zip.CreateEntry(path, CompressionLevel.Optimal).Open());
        s.Write(text);
        s.Flush();
    }

    public static int Depth(this ZipArchiveEntry entry) => 
        entry.FullName.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length;
}