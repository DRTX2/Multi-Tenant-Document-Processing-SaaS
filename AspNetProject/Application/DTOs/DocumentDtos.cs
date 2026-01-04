using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Application.DTOs;

public record DocumentResponse(
    Guid Id,
    Guid TenantId,
    Guid OwnerUserId,
    string FileName,
    string ContentType,
    long SizeInBytes,
    string Status,
    DateTime CreatedAt,
    IEnumerable<DocumentVersionDto> Versions
);

public record DocumentVersionDto(
    int VersionNumber,
    string FilePath,
    long SizeInBytes,
    DateTime CreatedAt
);

public record UpdateDocumentMetadataRequest(
    string FileName,
    string ContentType
);
