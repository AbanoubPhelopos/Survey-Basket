namespace Survey_Basket.Infrastructure.Services;

public class FileAnswerStorageService : IFileAnswerStorage
{
    public async Task<string> SaveAsync(Guid pollId, Guid questionId, Guid userId, string fileName, string contentType, byte[] content, CancellationToken cancellationToken = default)
    {
        var safeFileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var extension = Path.GetExtension(safeFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = contentType switch
            {
                "application/pdf" => ".pdf",
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                _ => ".bin"
            };
        }

        var folder = Path.Combine(AppContext.BaseDirectory, "uploads", "survey-answers", pollId.ToString(), questionId.ToString());
        Directory.CreateDirectory(folder);

        var storedName = $"{userId:N}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var path = Path.Combine(folder, storedName);

        await File.WriteAllBytesAsync(path, content, cancellationToken);

        return path;
    }
}
