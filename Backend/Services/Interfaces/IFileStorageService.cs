using Microsoft.AspNetCore.Http;

namespace CampusConnect.Api.Services.Interfaces;

public interface IFileStorageService
{
    Task<(string relativePath, string originalName, string mimeType, long size)> SaveFileAsync(IFormFile file);
}
