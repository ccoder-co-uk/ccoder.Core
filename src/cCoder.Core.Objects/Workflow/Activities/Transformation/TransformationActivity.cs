namespace cCoder.Core.Objects.Workflow.Activities.Transformation
{
    public abstract class TransformationActivity<TSource, TResult> : Activity
    {

        public TSource Source { get; set; }


        public TResult Result { get; set; }
    }
}