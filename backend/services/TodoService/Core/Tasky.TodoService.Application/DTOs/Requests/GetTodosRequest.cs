namespace Tasky.TodoService.Application.DTOs.Requests;

public class GetTodosRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool? IsCompleted { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}