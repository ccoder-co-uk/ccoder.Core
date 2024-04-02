namespace Core.Api.Controllers
{
    public class ModelStateError
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string[] Errors { get; set; }
    }
}