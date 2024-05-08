using System.Globalization;
using System.Reflection;

namespace cCoder.Core.Objects.Extensions;

public class CompositePropertyInfo : PropertyInfo
{

    public override PropertyAttributes Attributes => (PropertyAttributes)PropertyType.Attributes;

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override Type PropertyType { get; }

    public override Type DeclaringType => PropertyType;

    public override string Name => PropertyType.Name;

    public override Type ReflectedType => PropertyType.ReflectedType;
    public CompositePropertyInfo(Type type)
    {
        PropertyType = type;
    }

    public override MethodInfo[] GetAccessors(bool nonPublic) => throw new NotImplementedException();

    public override object[] GetCustomAttributes(bool inherit) => PropertyType.GetCustomAttributes(inherit);

    public override object[] GetCustomAttributes(Type attributeType, bool inherit) => PropertyType.GetCustomAttributes(attributeType, inherit);

    public override MethodInfo GetGetMethod(bool nonPublic) => throw new NotImplementedException();

    public override ParameterInfo[] GetIndexParameters() => throw new NotImplementedException();

    public override MethodInfo GetSetMethod(bool nonPublic) => throw new NotImplementedException();

    public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) => throw new NotImplementedException();
    public override bool IsDefined(Type attributeType, bool inherit) => PropertyType.IsDefined(attributeType, inherit);
    public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) => throw new NotImplementedException();
}