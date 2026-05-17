namespace Mingley.Application.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, StatusCode = 200, Message = message, Data = data };
    public static ApiResponse<T> Created(T data, string message = "Created") =>
        new() { Success = true, StatusCode = 201, Message = message, Data = data };
    public static ApiResponse<T> Fail(string message, int statusCode = 400) =>
        new() { Success = false, StatusCode = statusCode, Message = message };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public static ApiResponse Ok(string message = "Success") =>
        new() { Success = true, StatusCode = 200, Message = message };
    public static ApiResponse Fail(string message, int statusCode = 400) =>
        new() { Success = false, StatusCode = statusCode, Message = message };
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public bool HasNext { get; set; }
}
