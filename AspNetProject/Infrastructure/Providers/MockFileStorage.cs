using AspNetProject.Domain.Ports.Out;

namespace AspNetProject.Infrastructure.Providers;

public class MockFileStorage : IFileStorage
{
    public Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        // Simulate upload
        return Task.FromResult($"local-storage/{fileName}");
    }

    public Task<Stream> DownloadAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Stream.Null);
    }
}

