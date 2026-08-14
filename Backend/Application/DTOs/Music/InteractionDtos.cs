namespace Application.DTOs.Music;

/// <summary>Поточний стан "вподобаного" для треку та користувача.</summary>
public class TrackLikeStateDto
{
    public Guid TrackId { get; set; }
    public bool IsLiked { get; set; }
    public long LikesCount { get; set; }

    /// <summary>Коли користувач додав трек до вподобаних. null, якщо не вподобано.</summary>
    public DateTime? LikedAt { get; set; }
}

/// <summary>Результат спроби зафіксувати прослуховування.</summary>
public enum ListenRecordStatus
{
    /// <summary>Подію записано в історію, лічильник відтворень збільшено.</summary>
    Recorded = 0,

    /// <summary>Прослухано замало, щоб вважати це відтворенням.</summary>
    TooShort = 1,

    /// <summary>Те саме відтворення вже зафіксовано щойно (захист від дублювання).</summary>
    Throttled = 2
}

public class ListenRegistrationDto
{
    public Guid TrackId { get; set; }
    public ListenRecordStatus Status { get; set; }

    /// <summary>true лише коли Status == Recorded.</summary>
    public bool Recorded => Status == ListenRecordStatus.Recorded;

    /// <summary>Актуальний лічильник відтворень треку після операції.</summary>
    public long PlaysCount { get; set; }

    /// <summary>Час зафіксованої події. null, якщо запис не створено.</summary>
    public DateTime? ListenedAt { get; set; }

    /// <summary>Скільки мілісекунд треба прослухати, щоб подія зарахувалась.</summary>
    public int RequiredListenedMs { get; set; }
}

public class ListeningHistoryEntryDto
{
    public Guid Id { get; set; }
    public DateTime ListenedAt { get; set; }
    public int? DurationListenedMs { get; set; }
    public TrackDto Track { get; set; } = new();
}
