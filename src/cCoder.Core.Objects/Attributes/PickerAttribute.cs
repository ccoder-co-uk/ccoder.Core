using System;

namespace cCoder.Core.Objects
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class PickerAttribute : Attribute
    {
        public string Type { get; }

        public string Filter { get; }

        public string Display { get; }

        public string Value { get; set; }

        public PickerAttribute(string endpoint, string display = "Name", string value = "Id", string filter = "")
        {
            Type = endpoint;
            Filter = filter;
            Display = display;
            Value = value;
        }
    }
}
