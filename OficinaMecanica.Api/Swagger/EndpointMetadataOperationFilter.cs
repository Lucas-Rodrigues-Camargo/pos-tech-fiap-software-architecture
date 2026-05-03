using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OficinaMecanica.Api.Swagger;

public sealed class EndpointMetadataOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var summary = context.MethodInfo.GetCustomAttribute<EndpointSummaryAttribute>();
        var description = context.MethodInfo.GetCustomAttribute<EndpointDescriptionAttribute>();

        if (summary is not null)
        {
            operation.Summary = summary.Summary;
        }

        if (description is not null)
        {
            operation.Description = description.Description;
        }
    }
}
