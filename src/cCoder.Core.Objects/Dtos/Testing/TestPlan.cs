using System.ComponentModel.DataAnnotations;

namespace cCoder.Core.Objects.Dtos.Testing;

public class TestPlan
{
    [Key]
    public string Name { get; set; }

    public Test[] Tests { get; set; }
}

public class Test
{
    [Key]
    public string Name { get; set; }

    public string Result { get; set; }

    public TestAction[] Actions { get; set; }
}

public class TestArgs
{
    public HttpClient Api { get; set; }
    public Test Test { get; set; }
    public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
}