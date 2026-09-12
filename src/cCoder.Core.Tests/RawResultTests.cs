// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Exposures.OData.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace cCoder.Core.Tests.Exposures.OData.Responses;

public sealed partial class RawResultTests
{
    [Fact]
    public void RawResult_WhenCreated_PreservesRawResponseAndSuccessStatus()
    {
        // Given
        const string response = "raw response";

        // When
        RawResult result = new(response: response);

        // Then
        result.Content
            .Should()
            .Be(expected: response);

        result.StatusCode
            .Should()
            .Be(expected: StatusCodes.Status200OK);
    }
}