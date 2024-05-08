namespace cCoder.Core.Objects;

public class Path
{
    public static Path Empty { get; } = new(string.Empty);

    public string Name => Segments.LastOrDefault();

    public string FullPath { get; }

    public string Lowered => FullPath.ToLower();

    public string[] Segments => FullPath.Split('/');

    public Path ParentPath => Segments.Length > 1 ? new Path(string.Join("/", Segments)[..(FullPath.Length - (1 + Segments.Last().Length))]) : Empty;

    public string Extension => Segments.LastOrDefault()?.Contains('.') ?? false ? Segments.LastOrDefault()?.Split('.').Last().ToLower() ?? string.Empty : string.Empty;

    public string MimeType => Objects.MimeType.Get(Extension)?.MimeType ?? "text/plain";

    public int Length => FullPath.Length;

    public int Depth => Segments.Length;

    public bool IsToFile => Extension.Length > 0;

    public Path(string path)
    {
        FullPath = (path ?? string.Empty).Trim().Trim('/');
    }

    public override string ToString() => FullPath;
}