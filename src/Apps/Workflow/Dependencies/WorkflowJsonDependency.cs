// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Workflow.Dependencies;

internal sealed class WorkflowJsonDependency
{
    public WorkflowRequest DeserializeWorkflowRequest(string json) =>
        JsonConvert.DeserializeObject<WorkflowRequest>(
            value: json,
            settings: new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.None,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new DefaultContractResolver
                {
                    IgnoreSerializableAttribute = true
                }
            });
}