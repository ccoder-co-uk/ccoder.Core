using System;

namespace Core.Objects
{
    /// <summary>
    /// Put on methods with this signature
    ///  public Task HandleHelloWorld(T sentObject)
    /// </summary>
    /// <remarks></remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class HandlerForAttribute : Attribute
    {
        public string EventName { get; }

        public HandlerForAttribute(string eventName)
        {
            EventName = eventName;
        }
    }
}