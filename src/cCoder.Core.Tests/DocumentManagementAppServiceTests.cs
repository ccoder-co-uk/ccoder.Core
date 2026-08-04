// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.DocumentManagement;
using cCoder.Core.Models.Exceptions;
using cCoder.Core.Services.Foundations.DocumentManagement;
using cCoder.Data.Models.CMS;
using FluentAssertions;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class DocumentManagementAppServiceTests
{
    [Fact]
    public async Task ShouldDelegateFlatAppLifecycleAsync()
    {
        // Given
        App app = new()
        {
            Id = 42,
            DefaultCultureId = "en-GB",
            TenantId = "tenant",
            Name = "Documents",
            Domain = "documents.test",
            DefaultTheme = "Default",
            ConfigJson = "{}",
        };

        Mock<IDocumentManagementAppBroker> brokerMock = new(behavior: MockBehavior.Strict);

        brokerMock.Setup(expression: broker => broker.AddAppAsync(newApp:
            It.Is<App>(match: value =>
                !ReferenceEquals(objA: value, objB: app)
                && value.Id == app.Id
                && value.Name == app.Name)))
            .Returns(value: ValueTask.CompletedTask);

        brokerMock.Setup(expression: broker => broker.UpdateAppAsync(updatedApp:
            It.Is<App>(match: value =>
                !ReferenceEquals(objA: value, objB: app)
                && value.Id == app.Id
                && value.Name == app.Name)))
            .Returns(value: ValueTask.CompletedTask);

        brokerMock.Setup(expression: broker => broker.DeleteAppAsync(appId: app.Id))
            .Returns(value: ValueTask.CompletedTask);

        DocumentManagementAppService service = new(
            documentManagementAppBroker: brokerMock.Object);

        // When
        await service.AddAppAsync(newApp: app);
        await service.UpdateAppAsync(updatedApp: app);
        await service.DeleteAppAsync(appId: app.Id);

        // Then
        brokerMock.VerifyAll();
    }

    [Theory]
    [InlineData("add")]
    [InlineData("update")]
    public async Task ShouldRejectNullAppBeforeBrokerCallAsync(string operation)
    {
        // Given
        Mock<IDocumentManagementAppBroker> brokerMock = new(behavior: MockBehavior.Strict);

        DocumentManagementAppService service = new(
            documentManagementAppBroker: brokerMock.Object);

        // When
        Func<Task> action = operation == "add"
            ? async () => await service.AddAppAsync(newApp: null)
            : async () => await service.UpdateAppAsync(updatedApp: null);
        // Then
        await action.Should()
            .ThrowAsync<CoreValidationException>();

        brokerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ShouldWrapUnexpectedBrokerFailureAsync()
    {
        // Given
        App app = new() { Id = 42 };
        InvalidOperationException brokerException = new(message: "failure");
        Mock<IDocumentManagementAppBroker> brokerMock = new(behavior: MockBehavior.Strict);

        brokerMock.Setup(expression: broker => broker.DeleteAppAsync(appId: app.Id))
            .ThrowsAsync(exception: brokerException);

        DocumentManagementAppService service = new(
            documentManagementAppBroker: brokerMock.Object);

        // When
        Func<Task> action = async () => await service.DeleteAppAsync(appId: app.Id);

        // Then
        CoreServiceException exception = (await action.Should()
            .ThrowAsync<CoreServiceException>()).Which;

        exception.InnerException.Should()
            .BeSameAs(expected: brokerException);

        brokerMock.VerifyAll();
    }
}