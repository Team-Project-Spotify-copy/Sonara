using Microsoft.Extensions.Configuration;

namespace Sonara.Tests.Infrastructure;

public static class TestConfiguration
{
    public static IConfiguration Create(params (string Key, string Value)[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();
    }
}
