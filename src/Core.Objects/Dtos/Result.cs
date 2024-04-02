using Core.Objects.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Core.Objects.Dtos
{
    public class Result
    {
        [Key]
        public virtual string Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class Result<T> : Result
    {
        string id = null;

        [Key]
        public override string Id
        {
            get
            {
                if (id == null)
                {
                    try
                    {
                        return Item?.GetId()?.ToString();
                    }
                    catch { return null; }
                }
                else
                {
                    return id;
                }
            }
            set { id = value; }
        }

        public T Item { get; set; }

        public Result<TNew> ToNew<TNew>(TNew item)
            => new() { Success = Success, Message = Message, Item = item };
    }
}
