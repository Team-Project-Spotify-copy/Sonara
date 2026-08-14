namespace Application.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message = "The request conflicts with the current state of the resource.")
        : base(message)
    {
    }
}

public class MediaUnavailableException : ConflictException
{
    public Guid TrackId { get; }

    public MediaUnavailableException(Guid trackId)
        : base($"Track ({trackId}) has no playable media.")
    {
        TrackId = trackId;
    }
}

public class StorageUnavailableException : Exception
{
    public StorageUnavailableException(string message = "Media storage is temporarily unavailable.", Exception? inner = null)
        : base(message, inner)
    {
    }
}
