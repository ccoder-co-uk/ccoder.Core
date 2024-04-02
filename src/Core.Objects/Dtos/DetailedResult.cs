namespace Core.Objects.Dtos
{
    public class DetailedResult<T, TDetails> : Result<T>
    {
        public TDetails Details { get; set; }
    }
}
