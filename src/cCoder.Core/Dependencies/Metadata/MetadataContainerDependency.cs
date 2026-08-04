// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;
using cCoder.Data.Extensions;
using System.Collections;

namespace cCoder.Core.Dependencies.Metadata;

internal static class MetadataContainerDependency
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

    internal static MetadataContainer CreateMetadataContainer(
        Type type,
        bool isEntity,
        bool hasEndpoint) =>
        InitializeMetadataContainer(
            container: new MetadataContainer(),
            type: type,
            isEntity: isEntity,
            hasEndpoint: hasEndpoint);

    internal static ExtendedMetadataContainer CreateExtendedMetadataContainer(
        Type type,
        bool isEntity,
        bool hasEndpoint) =>
        InitializeMetadataContainer(
            container: new ExtendedMetadataContainer(),
            type: type,
            isEntity: isEntity,
            hasEndpoint: hasEndpoint);

    private static TContainer InitializeMetadataContainer<TContainer>(
        TContainer container,
        Type type,
        bool isEntity,
        bool hasEndpoint)
        where TContainer : MetadataContainer
    {
        container.IsValueType = type.IsValueType || type == typeof(string);
        container.Type = GetClientType(type: type);
        container.Name = type.Name;
        container.DisplayName = type.Name;
        container.Description = type.Name;
        container.ServerType = type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
        container.ServerTypeName = type.FullName ?? type.Name;
        container.Properties = [];
        container.IsEntity = isEntity;
        container.IsJoinEntity = isEntity && type.IsJoinType();
        container.HasEndpoint = hasEndpoint;

        return container;
    }

    private static string GetClientType(Type type) =>
        type == typeof(string)
            ? "string"
            : typeof(IEnumerable).IsAssignableFrom(c: type)
                ? "array"
                : TypeLookup.TryGetValue(key: type, value: out string typeName)
                    ? typeName
                    : "object";
}