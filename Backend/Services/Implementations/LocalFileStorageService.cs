using CampusConnect.Api.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CampusConnect.Api.Services.Implementations;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<(string relativePath, string originalName, string mimeType, long size)> SaveFileAsync(IFormFile file)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsDir = Path.Combine(webRoot, "uploads");

        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var ext = Path.GetExtension(file.FileName);
        var uniqueFileName = $"evidencia-{DateTime.UtcNow.Ticks}-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsDir, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/{uniqueFileName}";
        return (relativePath, file.FileName, file.ContentType, file.Length);
    }
}
