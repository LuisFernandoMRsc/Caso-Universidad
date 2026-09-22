using CampusConnect.Api.DTOs;
using CampusConnect.Api.Models.Entities;
using CampusConnect.Api.Models.Enums;
using CampusConnect.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusConnect.Api.Controllers;

[ApiController]
[Route("api")]
public class RecursosController : ControllerBase
{
    private readonly IRecursosService _recursosService;

    public RecursosController(IRecursosService recursosService)
    {
        _recursosService = recursosService;
    }

    [HttpGet("categorias")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoriaSolicitud>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategorias()
    {
        var categorias = await _recursosService.GetCategoriasAsync();
        return Ok(ApiResponse<IEnumerable<CategoriaSolicitud>>.Ok(categorias, "Categorías obtenidas"));
    }

    [HttpGet("recursos")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RecursoInstitucional>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecursos([FromQuery] TipoRecurso? tipo, [FromQuery] string? search)
    {
        var recursos = await _recursosService.GetRecursosAsync(tipo, search);
        return Ok(ApiResponse<IEnumerable<RecursoInstitucional>>.Ok(recursos, "Recursos obtenidos"));
    }

    [Authorize(Roles = "ADMIN,ADMINISTRATIVO")]
    [HttpGet("personal")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPersonalDisponible()
    {
        var personal = await _recursosService.GetPersonalDisponibleAsync();
        return Ok(ApiResponse<IEnumerable<object>>.Ok(personal, "Personal disponible obtenido"));
    }

    [Authorize(Roles = "ADMIN,ADMINISTRATIVO")]
    [HttpPost("recursos")]
    [ProducesResponseType(typeof(ApiResponse<RecursoInstitucional>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRecurso([FromBody] CreateRecursoDto dto)
    {
        var recurso = await _recursosService.CreateRecursoAsync(dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RecursoInstitucional>.Ok(recurso, "Recurso creado"));
    }
}
