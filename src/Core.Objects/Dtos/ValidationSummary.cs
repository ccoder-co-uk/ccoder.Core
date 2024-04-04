using System.Collections.Generic;

namespace cCoder.Core.Objects.Dtos
{
    public class ValidationSummary<T>
    {
        public IEnumerable<T> ValidatedData { get; set; }
        public IEnumerable<Result<T>> Failures { get; set; }
    }
}
