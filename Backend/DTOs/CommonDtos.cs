namespace CampusConnect.Api.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Meta { get; set; }
    public object? Error { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Operación realizada exitosamente", object? meta = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Meta = meta
        };
    }

    public static ApiResponse<T> Fail(string message, object? error = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = error
        };
    }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / (Limit > 0 ? Limit : 1));
}
