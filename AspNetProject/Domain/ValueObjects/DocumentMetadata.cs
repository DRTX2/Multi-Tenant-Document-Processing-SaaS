namespace AspNetProject.Domain.ValueObjects;

public class DocumentMetadata
{
    public string FileName { get; init; }
    public string ContentType { get; init; }
    public long SizeInBytes { get; init; }

    private DocumentMetadata() 
    {
        FileName = null!;
        ContentType = null!;
    } // For EF Core

    public DocumentMetadata(string fileName, string contentType, long sizeInBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));
        
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type cannot be empty", nameof(contentType));
        
        if (sizeInBytes < 0)
            throw new ArgumentException("Size cannot be negative", nameof(sizeInBytes));

        FileName = fileName;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not DocumentMetadata other) return false;
        return FileName == other.FileName &&
               ContentType == other.ContentType &&
               SizeInBytes == other.SizeInBytes;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FileName, ContentType, SizeInBytes);
    }
}