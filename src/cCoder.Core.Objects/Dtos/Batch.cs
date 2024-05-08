namespace cCoder.Core.Objects.Dtos;

/// <summary>
/// Batch request
/// </summary>
/// <typeparam name="T"></typeparam>
public class Batch<T>
{
    /// <summary>
    /// Flag to specify if the whole thing should be treated as a single transaction.
    /// If true and any single item fails in the transactional batch then the batch will fail and rollback.
    /// If false
    /// </summary>
    public bool Transactionional { get; set; }
    public IEnumerable<T> Items { get; set; }
}