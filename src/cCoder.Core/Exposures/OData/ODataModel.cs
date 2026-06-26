using Microsoft.OData.Edm;


namespace cCoder.Core.Exposures.OData;

/// <summary>
/// Container for OData Model information constructed by ODataModelBuilder implementations
/// in order to setup OData graphs
/// </summary>
public class ODataModel
{
    /// <summary>
    /// Goes in to first part of Url when routing to this model
    /// Urls in the model should follow the routing convention (if mapped as standard routes): ~/{context}/{Type}...
    /// </summary>
    public string Context { get; set; }

    /// <summary>
    /// Meta description of the model
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// An ODataModelBuilder will construct this
    /// </summary>
    public IEdmModel EDMModel { get; set; }
}




