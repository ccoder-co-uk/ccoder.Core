using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Extensions;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Reflection;

namespace cCoder.Core.Api.Controllers;

public abstract class JoinEntityOdataController<T, TUser, TKeyLeft, TKeyRight> : CoreODataController, IDisposable
    where T : class, new()
{
    protected IService<T, TUser> Service { get; private set; }

    public JoinEntityOdataController(IService<T, TUser> service, ICoreAuthInfo auth, ILogger log) 
        : base(auth, log) { Service = service; }

    /// <summary>
    /// Describe the current type
    /// </summary>
    /// <returns>type def (metadata)</returns>
    [HttpGet]
    public virtual IActionResult GetMetadata() => Ok(GetMetadataForType(typeof(T), true, true));

    /// <summary>
    /// Gets the entity collection.
    /// </summary>   
    /// <returns>The entity collection.</returns>
    [HttpGet]
    [EnableQuery(
        AllowedArithmeticOperators = AllowedArithmeticOperators.All,
        AllowedFunctions = AllowedFunctions.AllFunctions,
        AllowedLogicalOperators = AllowedLogicalOperators.All,
        AllowedQueryOptions = AllowedQueryOptions.All,
        MaxAnyAllExpressionDepth = 3,
        MaxExpansionDepth = 3
    )]
    public virtual IActionResult Get() => Ok(Service.GetAll(false));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<IActionResult> Post([FromBody] T entity) => ModelState.IsValid ? Ok(await Service.AddAsync(entity)) : BadRequest(ModelState);

    [HttpPost]
    public virtual async Task<IActionResult> AddAll([FromBody] ODataCollection<T> items) => ModelState.IsValid ? Ok(await Service.AddAllAsync(items.Value)) : BadRequest(ModelState);

    [HttpPost]
    public virtual async Task<IActionResult> DeleteAll([FromBody] ODataCollection<T> items)
    {
        if (ModelState.IsValid)
        {
            await Service.DeleteAllAsync(items.Value);
            return Ok();
        }
        else
            return BadRequest(ModelState);
    }

    protected virtual object BuildKey(TKeyLeft left, TKeyRight right)
    {
        Type type = typeof(T);
        string[] keyNames = type.Name.SplitCamelCase().Select(i => i + "Id").ToArray();
        T key = new();
        PropertyInfo leftProp = type.GetProperty(keyNames[0]);
        PropertyInfo rightProp = type.GetProperty(keyNames[1]);
        leftProp.SetValue(key, left);
        rightProp.SetValue(key, right);
        return key;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && Service != null)
        {
            Service.Dispose();
            Service = null;
        }
    }
}