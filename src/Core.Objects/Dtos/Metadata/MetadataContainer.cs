using Core.Objects.Entities.CMS;
using Core.Objects.Extensions;
using Core.Objects.Workflow.Activities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Core.Objects.Dtos.Metadata
{

    public class MetadataContainer
    {
        static readonly string[] ignoreMethods = new[] { "GetType", "GetHashCode", "Equals", "ToString" };

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

        public IEnumerable<MethodContainer> Methods { get; set; }

        public MetadataContainer() { }

        public MetadataContainer(Type type)
        {
            IsValueType = type.IsValueType || type == typeof(string);
            Type = GetTypeName(type);
            Name = type.Name;
            DisplayName = type.Name;
            Description = type.Name;
            ServerType = type.AssemblyQualifiedName;
            ServerTypeName = type.GetCSharpTypeName();
            IsSystemManaged = type.GetCustomAttribute<IsSystemManagedAttribute>() != null;

            Properties = type.IsValueType || type == typeof(string)
                ? new List<PropertyContainer>(0)
                : type.GetProperties().Where(p => p.Name != "AssignCode").Select(PropertyInfoFor).ToList();

            Methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName && !m.IsPrivate && !ignoreMethods.Contains(m.Name))
                .Where(m => !type.IsAssignableFrom(typeof(Activity)) || !m.Name.Contains("Execute"))
                .Select(m => MethodInfoFor(m))
                .ToArray();
        }

        public MetadataContainer Resource(string setName, string culture, IEnumerable<Resource> resources)
        {
            // B2B|Credit

            string cacheKey = $"{setName}|{ServerTypeName.Split('.').Last()}";
            Resource resource = resources.ForKeyAndCulture(cacheKey, culture);
            return new()
            {
                Type = Type,
                ServerTypeName = ServerTypeName,
                ServerType = ServerType,
                IsValueType = IsValueType,
                IsEntity = IsEntity,
                IsJoinEntity = IsJoinEntity,
                HasEndpoint = HasEndpoint,
                IsSystemManaged = IsSystemManaged,
                Category = Category,
                Name = Name,
                DisplayName = resource?.DisplayName ?? DisplayName,
                Description = resource?.Description ?? Description,
                Properties = Properties.Select(p => p.Resource(cacheKey, culture, resources)).ToArray(),
                Methods = Methods
            };
        }

        public MetadataContainer(Type type, bool isEntity, bool hasEndpoint) : this(type)
        {
            IsEntity = isEntity;
            IsJoinEntity = isEntity && type.IsJoinType();
            HasEndpoint = hasEndpoint;
        }

        static MethodContainer MethodInfoFor(MethodInfo method)
        {
            return new MethodContainer
            {
                Name = method.Name,
                Returns = new ParameterContainer
                {
                    Name = "return",
                    Type = GetTypeName(method.ReturnType),
                    ServerType = method.ReturnType.ToString(),
                    ServerTypeName = method.ReturnType.GetCSharpTypeName(),
                    IsGeneric = method.ReturnType.IsGenericParameter,
                    IsValueType = method.ReturnType.IsValueType || method.ReturnType == typeof(string)
                },
                Parameters = method.GetParameters()
                    .Select(p => new ParameterContainer
                    {
                        Name = p.Name,
                        Type = GetTypeName(p.ParameterType),
                        ServerType = p.ParameterType.ToString(),
                        ServerTypeName = p.ParameterType.GetCSharpTypeName(),
                        IsGeneric = p.ParameterType.IsGenericParameter,
                        IsValueType = p.ParameterType.IsValueType || p.ParameterType == typeof(string)
                    })
                    .ToArray()
            };
        }

        static string GetTypeName(Type p)
        {
            if (p == typeof(string))
                return "string";

            if (typeof(IEnumerable).IsAssignableFrom(p))
                return "array";

            return lookup.ContainsKey(p)
                ? lookup[p]
                : "object";
        }

        static readonly Dictionary<Type, string> lookup = new()
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
            { typeof(float?), "number" }
        };

        PropertyContainer PropertyInfoFor(PropertyInfo p)
        {
            return new PropertyContainer()
            {
                Name = p.Name,
                Type = GetTypeName(p.PropertyType),
                ServerType = p.PropertyType.ToString(),
                ServerTypeName = p.PropertyType.GetCSharpTypeName(),
                IsValueType = p.PropertyType.IsValueType || p.PropertyType == typeof(string),
                DisplayName = p.Name,
                ShortDisplayName = p.Name,
                Description = p.Name,
                IsReadOnly = !p.CanWrite,
                IsSystemManaged = p.GetCustomAttribute<IsSystemManagedAttribute>() != null,

                Template = p.GetCustomAttribute(typeof(KeyAttribute)) != null || p.Name == "Id"
                    ? "key" 
                    : p.Name,

                IsRequired =
                    !(p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) && p.PropertyType.IsValueType
                    || p.GetCustomAttribute<RequiredAttribute>() != null
            };
        }
    }
}