using CampusConnect.Api.Data;
using CampusConnect.Api.DTOs;
using CampusConnect.Api.Models.Entities;
using CampusConnect.Api.Models.Enums;
using CampusConnect.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusConnect.Api.Services.Implementations;

public class RecursosService : IRecursosService
{
    private readonly ApplicationDbContext _context;

    public RecursosService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoriaSolicitud>> GetCategoriasAsync()
    {
        return await _context.Categorias
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<RecursoInstitucional>> GetRecursosAsync(TipoRecurso? tipo, string? search)
    {
        var query = _context.Recursos.AsQueryable();

        if (tipo.HasValue)
        {
            query = query.Where(r => r.Tipo == tipo.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(r =>
                r.CodigoInventario.ToLower().Contains(s) ||
                r.Nombre.ToLower().Contains(s) ||
                r.Ubicacion.ToLower().Contains(s));
        }

        return await query.OrderBy(r => r.Nombre).ToListAsync();
    }

    public async Task<IEnumerable<object>> GetPersonalDisponibleAsync()
    {
        return await _context.Usuarios
            .Where(u => u.Activo && (u.Rol == RolUsuario.TECNICO || u.Rol == RolUsuario.ADMINISTRATIVO || u.Rol == RolUsuario.ADMIN))
            .Select(u => new
            {
                u.Id,
                u.NombreCompleto,
                u.Email,
                u.Rol,
                u.Telefono,
                TotalAsignadasActivas = u.Asignaciones.Count(a => a.Activo)
            })
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync();
    }

    public async Task<RecursoInstitucional> CreateRecursoAsync(CreateRecursoDto dto)
    {
        var recurso = new RecursoInstitucional
        {
            CodigoInventario = dto.CodigoInventario.Trim().ToUpper(),
            Nombre = dto.Nombre.Trim(),
            Tipo = dto.Tipo,
            Ubicacion = dto.Ubicacion.Trim(),
            Estado = dto.Estado
        };

        await _context.Recursos.AddAsync(recurso);
        await _context.SaveChangesAsync();
        return recurso;
    }
}
