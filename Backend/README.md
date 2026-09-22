# Campus Connect - API Backend en ASP.NET Core y C# con PostgreSQL

API REST centralizada para la gestión institucional de solicitudes universitarias (mantenimiento, soporte tecnológico, infraestructura, equipamiento) y generación de reportes y métricas operativas en tiempo real, desarrollada con **ASP.NET Core 9 (C#)**, **Entity Framework Core**, y base de datos **PostgreSQL 18**.

---

## 🎯 Problemas que Soluciona

1. **Eliminación de canales dispersos**: Centraliza reportes que antes se perdían entre papel, WhatsApp y correo electrónico.
2. **Prevención de duplicidades**: Cada solicitud genera automáticamente un código único e irrepetible (`SOL-2026-XXXX`).
3. **Trazabilidad y Derivación**: Auditoría completa de cada cambio de estado, fecha, hora y responsable asignado.
4. **Evidencia Fotográfica y Geolocalización**: Soporte multipart para carga de fotos de fallas directamente desde la app móvil.
5. **Comunicación Transparente**: Hilo de comentarios dentro del ticket entre estudiantes y administrativos/técnicos.
6. **Módulo de Reportes Consolidados**: Generación inmediata de indicadores clave (KPIs), métricas por tipo y prioridad, y descargas ejecutivas en **PDF** y **Excel**.

---

## ⚙️ Tecnologías Utilizadas

- **Lenguaje**: C# (.NET 9)
- **Framework Web**: ASP.NET Core Web API
- **ORM**: Entity Framework Core 9 (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Base de Datos**: PostgreSQL 18 (`campus_connect_db`)
- **Autenticación**: JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) y `BCrypt.Net`
- **Generación de Reportes PDF**: `QuestPDF`
- **Generación de Reportes Excel**: `ClosedXML`
- **Documentación Interactiva**: Swagger UI (`Swashbuckle.AspNetCore`)

---

## 🚀 Cómo Iniciar el Backend

Desde la terminal en la carpeta `Backend/`:

```powershell
dotnet run --launch-profile http
```

La API quedará escuchando en:
- **Base URL**: `http://localhost:5000`
- **Documentación Swagger UI interactiva**: `http://localhost:5000/swagger`

---

## 🔑 Credenciales de Prueba Precargadas

| Rol | Correo Electrónico | Contraseña | Carnet / Código |
| :--- | :--- | :--- | :--- |
| **Administrador General** | `admin@campusconnect.edu` | `Admin123!` | `ADM-001` |
| **Personal Administrativo** | `soporte@campusconnect.edu` | `Soporte123!` | `ADM-002` |
| **Técnico de Soporte/Redes** | `tecnico@campusconnect.edu` | `Tecnico123!` | `TEC-001` |
| **Estudiante 1** | `estudiante@campusconnect.edu` | `Estudiante123!` | `EST-2024-001` |
| **Estudiante 2** | `maria.gomez@campusconnect.edu` | `Estudiante123!` | `EST-2024-002` |

---

## 📋 Catálogo de Endpoints

### 1. Autenticación (`/api/Auth`)
- `POST /api/Auth/register`: Registro de estudiantes con carnet y contraseña.
- `POST /api/Auth/login`: Autenticación y obtención de JWT Bearer Token.
- `GET /api/Auth/me`: Consulta de perfil del usuario logueado.

### 2. Catálogos y Recursos (`/api`)
- `GET /api/categorias`: Listado de categorías (Mantenimiento, Soporte, Infraestructura, Equipamiento, etc.).
- `GET /api/recursos`: Listado y búsqueda de recursos universitarios (aulas, PCs de laboratorio, proyectores).
- `GET /api/personal`: Personal técnico y administrativo disponible para asignación.

### 3. Solicitudes (`/api/Solicitudes`)
- `POST /api/Solicitudes`: Creación con fotos de evidencia (`multipart/form-data`).
- `GET /api/Solicitudes/mis-solicitudes`: Bandeja del estudiante en la app móvil.
- `GET /api/Solicitudes`: Bandeja general con filtros de estado, prioridad, fecha y búsqueda.
- `GET /api/Solicitudes/{id}`: Detalle exhaustivo del ticket.
- `PATCH /api/Solicitudes/{id}/estado`: Cambio de estado (`REGISTRADA`, `ASIGNADA`, `EN_PROCESO`, `RESUELTA`, `CERRADA`).
- `POST /api/Solicitudes/{id}/asignar`: Derivación a un técnico o administrativo.
- `PATCH /api/Solicitudes/{id}/prioridad`: Cambio de prioridad (`BAJA`, `MEDIA`, `ALTA`, `URGENTE`).
- `POST /api/Solicitudes/{id}/comentarios`: Mensajería estudiante <-> administración.
- `GET /api/Solicitudes/{id}/trazabilidad`: Historial cronológico auditado.

### 4. Analítica y Reportes (`/api/Reportes`)
- `GET /api/Reportes/dashboard`: Total recibidas, pendientes, resueltas, tiempos promedio de atención y efectividad.
- `GET /api/Reportes/por-tipo`: Solicitudes agrupadas por categoría.
- `GET /api/Reportes/por-prioridad`: Distribución porcentual por prioridad.
- `GET /api/Reportes/por-responsable`: Rendimiento y tickets asignados por personal.
- `GET /api/Reportes/exportar/pdf?token={jwt}`: Descarga directa de informe ejecutivo en formato PDF.
- `GET /api/Reportes/exportar/excel?token={jwt}`: Descarga directa de consolidado en formato Excel (.xlsx).
