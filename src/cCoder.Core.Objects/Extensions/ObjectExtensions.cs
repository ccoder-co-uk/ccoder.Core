using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace cCoder.Core.Objects.Extensions;

/// <summary>
/// Object extensions
/// </summary>
public static class ObjectExtensions
{
    public static JsonSerializerSettings GetJSONSettings() => new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Objects,
        Formatting = Newtonsoft.Json.Formatting.None,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        ContractResolver = new DefaultContractResolver { IgnoreSerializableAttribute = true }
    };

    public static JsonSerializerSettings GetODataJsonSettings() => new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        TypeNameHandling = TypeNameHandling.None,
        Formatting = Newtonsoft.Json.Formatting.None,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        ContractResolver = new DefaultContractResolver { IgnoreSerializableAttribute = true },
        MaxDepth = 4
    };

    /// <summary>
    /// Checks if the type implements the generic interface.
    /// </summary>
    /// <param name="generic">The generic type.</param>
    /// <param name="toCheck">To check type.</param>
    /// <returns>The type of the first generic argument if the To check type implements the generic interface, otherwise null.</returns>
    public static bool ImplementsGenericInterface(this Type toCheck, Type generic)
    {
        Type[] interfaces = toCheck.GetInterfaces();
        if (toCheck.IsInterface)
            interfaces = interfaces.Concat(new[] { toCheck }).ToArray();

        return interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition().FullName == generic.FullName) != null;
    }

    /// <summary>
    /// Gets the id / primary key value for an object used as an entity in a DbContext.
    /// Uses GetIdProperty() below as a source for the property information and returns whatever value is in that property.
    /// </summary>
    /// <param name="entityObject">The entity object.</param>
    /// <returns>integer key value if found</returns>
    /// <exception cref="InvalidOperationException">No valid property name found on this object type.</exception>
    public static object GetId(this object entityObject)
    {
        Type type = entityObject.GetType();
        PropertyInfo idProperty = entityObject.GetIdProperty();

        return idProperty != null
            ? idProperty.GetValue(entityObject, null)
            : throw new InvalidOperationException("Object type '" + type.Name + "' appears to not have an id property in its definition.");
    }

    /// <summary>
    /// Gets the id / primary key property for an object used as an entity in a DbContext.
    /// May work on other types but relies on their being a property with the appropriate naming convention ...
    /// Looks for either "Id" or "{TypeName}Id" property and returns it's value.
    /// Core code always uses integers for primary keys so an assumption is made that value is of type int.
    /// </summary>
    /// <param name="entityObject">The entity object.</param>
    /// <returns>integer key value if found</returns>
    /// <exception cref="InvalidOperationException">No valid property name found on this object type.</exception>
    public static PropertyInfo GetIdProperty(this object entityObject) => 
        entityObject.GetType().GetIdProperty();

    private static bool IsNotAKey(PropertyInfo p, bool includeForeignKeys) => 
        p.GetCustomAttribute<KeyAttribute>() == null && (includeForeignKeys || p.GetCustomAttribute<ForeignKeyAttribute>() == null);

    private static bool IsComplex(PropertyInfo p) => 
        p.PropertyType.IsValueType || p.PropertyType == typeof(string);

    private static bool IsNotASystemManagedProperty(PropertyInfo p) => 
        p.Name != "CreationDate" && p.Name != "Created" && p.Name != "CreatedOn";

    /// <summary>
    /// Performs a "shallow copy" of value type properties and basic strings
    /// in to the current object by property name matching (types don't have to match).
    /// </summary>
    /// <param name="objectToUpdate">The object to update.</param>
    /// <param name="objectToCopyFrom">The object to copy from.</param>
    public static T UpdateFrom<T>(this T objectToUpdate, object objectToCopyFrom, bool includeForeignKeys = true)
    {
        if (objectToCopyFrom == null)
            return objectToUpdate;

        PropertyInfo[] properties = objectToUpdate.GetType().GetProperties();
        Type copyType = objectToCopyFrom.GetType();

        foreach (PropertyInfo prop in properties)
        {
            if (IsNotAKey(prop, includeForeignKeys) && IsComplex(prop) && IsNotASystemManagedProperty(prop))
            {
                PropertyInfo copyProp = copyType.GetProperty(prop.Name);

                if (copyProp != null && copyProp.CanWrite)
                    prop.SetValue(objectToUpdate, copyProp.GetValue(objectToCopyFrom, null), null);
            }
        }

        return objectToUpdate;
    }

    /// <summary>
    /// Translates an Object in to a dictionary
    /// </summary>
    /// <param name="obj">the object</param>
    /// <returns>dictionary</returns>
    public static IDictionary<string, object> ToDictionary(this object obj)
    {
        PropertyInfo[] propertyInfos = obj.GetType().GetProperties();
        Dictionary<string, object> result = propertyInfos.ToDictionary(propertyInfo => propertyInfo.Name, propertyInfo => obj.GetProperty(propertyInfo.Name) ?? string.Empty);
        return result;
    }

    /// <summary>
    /// Translates a dictionary in to an object
    /// </summary>
    /// <param name="source">the dictionary</param>
    /// <returns>the object</returns>
    public static dynamic ToObject(this IDictionary<string, object> source)
    {
        dynamic result = new ExpandoObject();
        source.ForEach(i => ((IDictionary<string, object>)result)[i.Key] = i.Value);
        return result;
    }

    /// <summary>
    /// Gets the real type of an enity not the EF generated proxy type
    /// </summary>
    /// <param name="obj">the object in question</param>
    /// <returns>Type of the object</returns>
    public static Type GetNonProxiedType(this object obj)
    {
        Type type = obj.GetType();

        if (type.FullName.StartsWith("System.Data.Entity.DynamicProxies"))
            type = type.BaseType;

        return type;
    }

    /// <summary>
    /// gets the given property on the given object
    /// </summary>
    /// <param name="o"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private static object GetProperty(this object o, string name)
    {
        object value = null;
        PropertyInfo propertyInfo = o.GetType().GetProperty(name);

        if (propertyInfo != null)
            value = o.GetType().GetProperty(name).GetValue(o, null);

        return value;
    }

    /// <summary>
    /// Serialises an object to XML
    /// </summary>
    /// <param name="o">the object</param>
    /// <returns>the string of xml</returns>
    public static string ToXml(this object o)
    {
        XmlSerializer serializer = new(o.GetType());
        using XmlTextWriter writer = new(new MemoryStream(), Encoding.UTF8);
        serializer.Serialize(writer, o);
        _ = writer.BaseStream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(writer.BaseStream);
        return reader.ReadToEnd();
    }

    public static XmlReader ToXmlReader(this object o)
    {
        XmlSerializer serializer = new(o.GetType());
        using XmlTextWriter writer = new(new MemoryStream(), Encoding.UTF8);
        serializer.Serialize(writer, o);
        _ = writer.BaseStream.Seek(0, SeekOrigin.Begin);
        return new XmlTextReader(writer.BaseStream);
    }

    public static string ToJson(this object o) => 
        o.ToJson(GetJSONSettings());

    public static string ToJson(this object o, int depth)
    {
        JsonSerializerSettings settings = new();
        settings.UpdateFrom(GetJSONSettings());
        settings.MaxDepth = depth;
        return JsonConvert.SerializeObject(o, settings);
    }

    public static string ToJsonForOdata(this object o) => 
        JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.None, GetODataJsonSettings());

    public static string ToJson(this object o, JsonSerializerSettings settings) => 
        JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.None, settings);

    public static string ToJson(this object o, JsonSerializerSettings settings, Newtonsoft.Json.Formatting format) => 
        JsonConvert.SerializeObject(o, format, settings);

    public static string ToCsv(this object o, IEnumerable<Resource> resources, string delimiter = ";", string quotes = "", string culture = "") => 
        new CsvFileBuilder() { Resources = resources, Culture = culture, Delimiter = delimiter, Quotes = quotes }
            .BuildFor(o);

    public static Stream ToExcel(this object o, IEnumerable<Resource> resources, string culture = "") => 
        new ExcelFileBuilder(culture, resources)
            .BuildFor(o);

    /// <summary>
    /// Validates the given object using it's data annotations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="o"></param>
    /// <returns></returns>
    public static Result<T> Validate<T>(this T o)
    {
        Result<T> result = new() { Item = o, Success = true };
        ValidationContext vc = new(o, null, null);
        try
        {
            List<ValidationResult> problems = new();
            bool valid = Validator.TryValidateObject(o, vc, problems);

            if (!valid)
            {
                result.Success = false;
                result.Message = string.Join("\n", problems.Select(p => p.ErrorMessage).ToArray());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return result;
    }
}