namespace cCoder.Core.Objects.Dtos;

/// <summary>
///  Represents a pre-processed page in a  more complete front end format.
///  Created to take a lot of the extra work the front end does away and have the back end handle it
///  as part of a simple api request for the page
/// </summary>
public class RenderResult
{
    // keys
    public int AppId { get; set; }
    public int PageId { get; set; }
    public int? ParentId { get; set; }
    public string UserId { get; set; }
    public bool ShowOnMenus { get; set; }
    public bool Edit { get; set; }

    // meta data
    public string Culture { get; set; }
    public string Theme { get; set; }
    public string Path { get; set; }
    public string Layout { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Keywords { get; set; }

    // content
    public string HeaderHtml { get; set; }
    public string BodyHtml { get; set; }
    public int StatusCode { get; set; } = 200;

    public dynamic KeyInfo() => new
    {
        AppId,
        PageId,
        ParentId
    };
}
