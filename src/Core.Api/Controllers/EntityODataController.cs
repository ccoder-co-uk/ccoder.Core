using Core.Objects;
using Core.Objects.Dtos;
using Core.Objects.Extensions;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using System.Linq.Dynamic.Core;

namespace Core.Api.Controllers
{
    /// <summary>
    /// Base Entity Controller
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    public abstract class EntityODataController<T, TKey, TUser> : CoreODataController, IDisposable
        where T : class
    {
        protected IService<T, TUser> Service { get; private set; }

        public EntityODataController(IService<T, TUser> service, ICoreAuthInfo auth, ILogger log) : base(auth, log) => 
            Service = service;

        /// <summary>
        /// Describe the current type
        /// </summary>
        /// <returns>type def (metadata)</returns>
        [HttpGet]
        public virtual IActionResult GetMetadata() => 
            Ok(GetMetadataForType(typeof(T), true, true));

        /// <summary>
        /// Gets the entity collection.
        /// </summary>
        /// <param name="queryOptions">The query options.</param>        
        /// <returns>The entity collection.</returns>
        [HttpGet]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.AllFunctions,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 5,
            MaxExpansionDepth = 5
        )]
        public virtual IActionResult Get(ODataQueryOptions<T> queryOptions) => 
            Ok(Service.GetAll(false));

        /// <summary>
        /// Gets the specific entity from the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The specific entity.</returns>
        [HttpGet]
        [AllowAnonymous]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.AllFunctions,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 3,
            MaxExpansionDepth = 3
        )]
        public virtual IActionResult Get([FromRoute] TKey key)
        {
            // do a get all call and then a where expression to get the one with the key
            System.Linq.Expressions.Expression<Func<T, bool>> idEquals = typeof(T).IdEquals<T>(key);
            IQueryable<T> result = Service.GetAll(false).AsQueryable().Where(idEquals);
            return result != null ? Ok(SingleResult.Create(result)) : NotFound();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.AllFunctions,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 5,
            MaxExpansionDepth = 5
        )]
        public virtual async Task<IActionResult> Post([FromBody] T entity) => 
            ModelState.IsValid 
                ? Ok(await Service.AddAsync(entity)) 
                : BadRequest(ModelState);

        [HttpPost]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.AllFunctions,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 5,
            MaxExpansionDepth = 5
        )]
        public virtual async Task<IActionResult> AddAll([FromBody] ODataCollection<T> items)
            => ModelState.IsValid ? Ok(await Service.AddAllAsync(items.Value)) : BadRequest(ModelState);

        [HttpPost]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.AllFunctions,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 5,
            MaxExpansionDepth = 5
        )]
        public virtual async Task<IActionResult> UpdateAll([FromBody] ODataCollection<T> items)
            => ModelState.IsValid ? Ok((await Service.UpdateAllAsync(items.Value)).AsQueryable<Result<T>>()) : BadRequest(ModelState);

        [HttpPost]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.AllFunctions,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 5,
            MaxExpansionDepth = 5
        )]
        public virtual async Task<IActionResult> AddOrUpdateAll([FromBody] ODataCollection<T> items)
            => ModelState.IsValid ? Ok(await Service.AddOrUpdate(items.Value)) : BadRequest(ModelState);

        /// <summary>
        /// Base update method for entity changes
        /// </summary>
        /// <param name="key">the entity key</param>
        /// <param name="entity"the updated entity></param>
        /// <returns></returns>
        [HttpPut]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.AllFunctions,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 5,
            MaxExpansionDepth = 5
        )]
        public virtual async Task<IActionResult> Put([FromRoute] TKey key, [FromBody] T entity)
            => ModelState.IsValid ? Ok(await Service.UpdateAsync(entity)) : BadRequest(ModelState);

        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IActionResult> Patch([FromRoute] TKey key, Delta<T> delta)
        {
            // don't like this, would rather pass the delta to the service layer but that would force a 
            // dependency on the OData framework in the business layer.
            T origentity = Service.Get(key);
            if (origentity == null)
            {
                return NotFound();
            }

            delta.Patch(origentity);
            return Ok(await Service.UpdateAsync(origentity));
        }

        [HttpDelete]
        public virtual async Task<IActionResult> Delete([FromRoute] TKey key)
        {
            await Service.DeleteAsync(key);
            return Ok();
        }


        [HttpPost]
        public virtual async Task<IActionResult> DeleteAll([FromBody] ODataCollection<TKey> items)
        {
            if (ModelState.IsValid)
            {
                List<Result<TKey>> workload = new();
                foreach (TKey id in items.Value)
                {
                    try
                    {
                        await Service.DeleteAsync(id);
                        workload.Add(new Result<TKey> { Item = id, Success = true, Message = "Done!" });
                    }
                    catch (Exception ex)
                    {
                        workload.Add(new Result<TKey> { Item = id, Success = false, Message = ex.Message });
                    }
                }
                return Ok(workload);
            }
            else
            {
                return BadRequest(ModelState);
            }
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
}