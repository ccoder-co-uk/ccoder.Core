namespace Core.Objects.Dtos.Metadata
{
    public class ParameterContainer
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string ServerType { get; set; }
        public string ServerTypeName { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsValueType { get; set; }
    }
}