using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Application.DTOs.Playlists;
using WebApp.Contracts;
using Xunit;

namespace Sonara.Tests;

public class RequestValidationMetadataTests
{
    public static TheoryData<Type> RequestTypes => new()
    {
        typeof(AddTrackToPlaylistRequest),
        typeof(RegisterListenRequest),
        typeof(TrackBatchRequest),
        typeof(CreatePlaylistRequest),
        typeof(UpdatePlaylistRequest)
    };

    private static ConstructorInfo PrimaryConstructor(Type type) =>
        type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();

    [Theory]
    [MemberData(nameof(RequestTypes))]
    public void Validation_attributes_are_never_declared_on_record_properties(Type type)
    {
        var offenders = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttributes<ValidationAttribute>(inherit: true).Any())
            .Select(p => p.Name)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"{type.Name} declares validation attributes on properties ({string.Join(", ", offenders)}). " +
            "Move them onto the primary-constructor parameters, or MVC will throw and return 500.");
    }

    [Theory]
    [MemberData(nameof(RequestTypes))]
    public void Validation_attributes_are_declared_on_the_constructor_parameters(Type type)
    {
        var decorated = PrimaryConstructor(type)
            .GetParameters()
            .Any(p => p.GetCustomAttributes<ValidationAttribute>(inherit: true).Any());

        Assert.True(decorated, $"{type.Name} has no validation attributes on its primary-constructor parameters.");
    }

    [Fact]
    public void TrackBatchRequest_rejects_an_empty_id_list()
    {
        var parameter = PrimaryConstructor(typeof(TrackBatchRequest))
            .GetParameters()
            .Single(p => p.Name == "Ids");

        var minLength = parameter.GetCustomAttribute<MinLengthAttribute>();

        Assert.NotNull(minLength);
        Assert.Equal(1, minLength!.Length);
        Assert.False(minLength.IsValid(Array.Empty<Guid>()));
        Assert.True(minLength.IsValid(new[] { Guid.NewGuid() }));
    }

    [Fact]
    public void AddTrackToPlaylistRequest_rejects_an_empty_guid()
    {
        var parameter = PrimaryConstructor(typeof(AddTrackToPlaylistRequest))
            .GetParameters()
            .Single(p => p.Name == "TrackId");

        var notEmpty = parameter.GetCustomAttribute<Application.Validators.NotEmptyGuidAttribute>();

        Assert.NotNull(notEmpty);
        Assert.False(notEmpty!.IsValid(Guid.Empty));
        Assert.True(notEmpty.IsValid(Guid.NewGuid()));
    }
}
