using PawRescue.Domain.Common;

namespace PawRescue.Domain.Entities;

/// <summary>Maps to the "Images" table. Only the URL/key is stored; the file lives in cloud storage.</summary>
public class ReportImage : BaseEntity
{
    public Guid ReportId { get; private set; }
    public AnimalReport Report { get; private set; } = null!;

    public string Url { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public bool IsPrimary { get; private set; }

    private ReportImage() { }

    public ReportImage(Guid reportId, string url, string storageKey, string contentType, long sizeBytes, bool isPrimary)
    {
        ReportId = reportId;
        Url = url;
        StorageKey = storageKey;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        IsPrimary = isPrimary;
    }

    public void MarkAsPrimary(bool value) => IsPrimary = value;
}
