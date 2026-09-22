using CampusConnect.Api.DTOs;
using CampusConnect.Api.Models.Entities;
using CampusConnect.Api.Models.Enums;

namespace CampusConnect.Api.Services.Interfaces;

public interface IRecursosService
{
    Task<IEnumerable<CategoriaSolicitud>> GetCategoriasAsync();
    Task<IEnumerable<RecursoInstitucional>> GetRecursosAsync(TipoRecurso? tipo, string? search);
    Task<IEnumerable<object>> GetPersonalDisponibleAsync();
    Task<RecursoInstitucional> CreateRecursoAsync(CreateRecursoDto dto);
}
