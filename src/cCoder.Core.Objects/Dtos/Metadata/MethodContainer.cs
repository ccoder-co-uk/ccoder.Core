namespace cCoder.Core.Objects.Dtos.Metadata;

public class MethodContainer
{
    public string Name { get; set; }
    public ParameterContainer[] Parameters { get; set; }
    public ParameterContainer Returns { get; set; }
}