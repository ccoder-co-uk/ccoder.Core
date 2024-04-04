using System;

namespace cCoder.Core.Objects.Dtos.Workflow
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class IgnoreWhenFlowCompleteAttribute : Attribute { }
}
