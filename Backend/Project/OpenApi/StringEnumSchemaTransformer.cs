using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WebApp.OpenApi;

/// <summary>
/// Перелічення серіалізуються рядками (JsonStringEnumConverter у Program.cs), але генератор
/// OpenAPI описує їх як integer. Без цього трансформера контракт суперечив би реальним
/// відповідям, і фронтенд очікував би числа замість "Recorded" / "SignedUrl".
/// </summary>
public sealed class StringEnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;

        if (!type.IsEnum)
        {
            return Task.CompletedTask;
        }

        schema.Type = JsonSchemaType.String;
        schema.Format = null;
        schema.Pattern = null;
        schema.Default = null;
        schema.Enum = Enum.GetNames(type)
            .Select(name => (JsonNode)name)
            .ToList();

        return Task.CompletedTask;
    }
}
