using System.ComponentModel.DataAnnotations;

namespace cCoder.Core.Models;

public class Result
{
    [Key]
    public virtual string Id { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}

public class Result<T> : Result
{
    private string id;

    [Key]
    public override string Id
    {
        get
        {
            if (id != null)
                return id;

            try
            {
                return Item is null ? null : ((dynamic)Item).Id?.ToString();
            }
            catch
            {
                return null;
            }
        }
        set => id = value;
    }

    public T Item { get; set; }

    public Result<TNew> ToNew<TNew>(TNew item) =>
        new() { Success = Success, Message = Message, Item = item };
}

public class AuditResultsByUser
{
    public string UserName { get; set; }
    public int Total { get; set; }
    public int January { get; set; }
    public int February { get; set; }
    public int March { get; set; }
    public int April { get; set; }
    public int May { get; set; }
    public int June { get; set; }
    public int July { get; set; }
    public int August { get; set; }
    public int September { get; set; }
    public int October { get; set; }
    public int November { get; set; }
    public int December { get; set; }
}

public class AuditResultByProperty
{
    public string Property { get; set; }
    public int Total { get; set; }
    public int January { get; set; }
    public int February { get; set; }
    public int March { get; set; }
    public int April { get; set; }
    public int May { get; set; }
    public int June { get; set; }
    public int July { get; set; }
    public int August { get; set; }
    public int September { get; set; }
    public int October { get; set; }
    public int November { get; set; }
    public int December { get; set; }
}
