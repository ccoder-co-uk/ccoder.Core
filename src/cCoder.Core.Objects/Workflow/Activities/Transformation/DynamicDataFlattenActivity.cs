namespace cCoder.Core.Objects.Workflow.Activities.Transformation;

public class DynamicDataFlattenActivity : TransformationActivity<IEnumerable<object>, dynamic[]>
{
    public override async Task Execute() => Result = await Task.FromResult(Source.SelectMany(o => Data.Flatten(o)).ToArray());
}