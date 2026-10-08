// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class SetupDatabaseTests
{
    [Fact]
    public async Task IsInitializedAsyncShouldUseConfiguredStoreData()
    {
        // Given
        string databaseName = Guid.NewGuid()
            .ToString();

        DbContextOptionsBuilder<SetupDatabaseTestContext> optionsBuilder =
            new();

        DbContextOptions<SetupDatabaseTestContext> options = optionsBuilder
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        await using SetupDatabaseTestContext context = new(options: options);

        context.Entities.Add(
            entity: new SetupDatabaseTestEntity());

        await context.SaveChangesAsync();

        // When
        bool isInitialized = await SetupDatabase
            .IsInitializedAsync<SetupDatabaseTestEntity>(
                context: context,
                cancellationToken: CancellationToken.None);

        // Then
        isInitialized.Should()
            .BeTrue();
    }

    private sealed class SetupDatabaseTestContext(
        DbContextOptions<SetupDatabaseTestContext> options)
        : DbContext(options: options)
    {
        internal DbSet<SetupDatabaseTestEntity> Entities =>
            Set<SetupDatabaseTestEntity>();
    }

    private sealed class SetupDatabaseTestEntity
    {
        public int Id { get; set; }
    }
}