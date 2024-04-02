using System;

namespace Core.Objects.Dtos
{
    public class Signature
    {
        public string Caller { get; set; }
        public DateTimeOffset ValidUntil { get; set; }
    }
}
