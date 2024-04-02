using System.Collections.Generic;

namespace Core.Objects.Dtos
{
    public class ValidationSummary<T>
    {
        public IEnumerable<T> ValidatedData { get; set; }
        public IEnumerable<Result<T>> Failures { get; set; }
    }
}
