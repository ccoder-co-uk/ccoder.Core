using System.Collections;
using cCoder.Data.Extensions;

namespace cCoder.Core.Models.Metadata;

public class MetadataContainer
{
    private static readonly Dictionary<Type, string> TypeLookup = new()
    {
        { typeof(short), "number" },
        { typeof(int), "number" },
        { typeof(long), "number" },
        { typeof(short?), "number" },
        { typeof(int?), "number" },
        { typeof(long?), "number" },
        { typeof(ushort), "number" },
        { typeof(uint), "number" },
        { typeof(ulong), "number" },
        { typeof(ushort?), "number" },
        { typeof(uint?), "number" },
        { typeof(ulong?), "number" },
        { typeof(byte), "number" },
        { typeof(byte?), "number" },
        { typeof(decimal), "number" },
        { typeof(decimal?), "number" },
        { typeof(string), "string" },
        { typeof(DateTime), "date" },
        { typeof(DateTime?), "date" },
        { typeof(TimeSpan), "time" },
        { typeof(TimeSpan?), "time" },
        { typeof(DateTimeOffset), "date" },
        { typeof(DateTimeOffset?), "date" },
        { typeof(Guid), "guid" },
        { typeof(Guid?), "guid" },
        { typeof(bool), "bool" },
        { typeof(bool?), "bool" },
        { typeof(double), "number" },
        { typeof(double?), "number" },
        { typeof(float), "number" },
        { typeof(float?), "number" },
    };

    public string Type { get; set; }
    public string ServerTypeName { get; set; }
    public bool IsValueType { get; set; }
    public bool IsEntity { get; set; }
    public bool IsJoinEntity { get; set; }
    public bool HasEndpoint { get; set; }
    public bool IsSystemManaged { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string ServerType { get; set; }
    public IEnumerable<PropertyContainer> Properties { get; set; }

    public MetadataContainer()
    {
    }

    public MetadataContainer(Type type)
    {
        IsValueType = type.IsValueType || type == typeof(string);
        Type = GetTypeName(type);
        Name = type.Name;
        DisplayName = type.Name;
        Description = type.Name;
        ServerType = type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
        ServerTypeName = type.FullName ?? type.Name;
        Properties = Array.Empty<PropertyContainer>();
    }

    public MetadataContainer(Type type, bool isEntity, bool hasEndpoint)
        : this(type)
    {
        IsEntity = isEntity;
        IsJoinEntity = isEntity && type.IsJoinType();
        HasEndpoint = hasEndpoint;
    }

    private static string GetTypeName(Type type)
    {
        if (type == typeof(string))
            return "string";

        if (typeof(IEnumerable).IsAssignableFrom(type))
            return "array";

        return TypeLookup.TryGetValue(type, out string name)
            ? name
            : "object";
    }
}

public class ExtendedMetadataContainer : MetadataContainer
{
    public IEnumerable<OperationContainer> Operations { get; set; }

    public ExtendedMetadataContainer()
    {
    }

    public ExtendedMetadataContainer(Type type)
        : base(type)
    {
    }

    public ExtendedMetadataContainer(Type type, bool isEntity, bool hasEndpoint)
        : base(type, isEntity, hasEndpoint)
    {
    }
}

public class MetadataContainerSet
{
    public string Name { get; set; }
    public string UriBase { get; set; }
    public ExtendedMetadataContainer[] Types { get; set; }
}

public class PropertyContainer
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string ServerType { get; set; }
    public string ServerTypeName { get; set; }
    public string Template { get; set; }
    public string DisplayName { get; set; }
    public string ShortDisplayName { get; set; }
    public string Description { get; set; }
    public bool IsGeneric { get; set; }
    public bool IsValueType { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsRequired { get; set; }
    public bool IsSystemManaged { get; set; }
}

public class OperationContainer
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string Definition { get; set; }
    public string HttpVerb { get; set; }
    public bool Queryable { get; set; }
    public MetadataContainer ReturnType { get; set; }
    public IDictionary<string, string> Parameters { get; set; }
}
