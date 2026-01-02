namespace AspNetProject.Domain.ValueObjects;

public class DocumentVersion
{
    public int VersionNumber { get; init; }
    public string FileHash { get; init; }
    public string StoragePath { get; init; }
    public DateTime CreatedAt { get; init; }

    private DocumentVersion() 
    {
        FileHash = null!;
        StoragePath = null!;
    } // For EF Core

    public DocumentVersion(int versionNumber, string fileHash, string storagePath)
    {
        if (versionNumber <= 0)
            throw new ArgumentException("Version number must be positive", nameof(versionNumber));
        
        VersionNumber = versionNumber;
        FileHash = fileHash ?? throw new ArgumentNullException(nameof(fileHash));
        StoragePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
        CreatedAt = DateTime.UtcNow;
    }

    // Equals and GetHashCode for Value Object equality
    public override bool Equals(object? obj)
    {
        if (obj is not DocumentVersion other) return false;
        return VersionNumber == other.VersionNumber &&
               FileHash == other.FileHash &&
               StoragePath == other.StoragePath;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(VersionNumber, FileHash, StoragePath);
    }
}