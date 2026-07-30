// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.IntegrationTests.Infrastructure;

using Xunit;

[CollectionDefinition(Name)]
public sealed class IntegrationAcceptanceCollection
    : ICollectionFixture<IntegrationAcceptanceFixture>
{
    public const string Name = "Integration acceptance";
}