using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace API.OpenApi;

public class CommonResponsesTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authorizeAttributes = context.Description
            .ActionDescriptor
            .EndpointMetadata
            .OfType<AuthorizeAttribute>()
            .ToList();

        if (authorizeAttributes.Count == 0) return Task.CompletedTask;

        operation.Responses ??= new OpenApiResponses();
        operation.Responses["401"] = new OpenApiResponse { Description = "Unauthorized" };

        if (authorizeAttributes.Any(a => !string.IsNullOrEmpty(a.Roles)))
            operation.Responses["403"] = new OpenApiResponse { Description = "Forbidden" };

        return Task.CompletedTask;
    }
}