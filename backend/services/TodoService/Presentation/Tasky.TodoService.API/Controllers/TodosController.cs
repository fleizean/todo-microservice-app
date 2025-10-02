using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasky.TodoService.Application.DTOs.Requests;
using Tasky.TodoService.Application.DTOs.Responses;
using Tasky.TodoService.Application.Services;

namespace Tasky.TodoService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodosController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodosController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<TodoListResponse>> GetTodos([FromQuery] GetTodosRequest request)
    {
        var userId = GetUserId();
        var todos = await _todoService.GetTodosAsync(userId, request);
        return Ok(todos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoResponse>> GetTodo(int id)
    {
        var userId = GetUserId();
        var todo = await _todoService.GetTodoByIdAsync(id, userId);
        
        if (todo == null)
            return NotFound();
            
        return Ok(todo);
    }

    [HttpPost]
    public async Task<ActionResult<TodoResponse>> CreateTodo([FromBody] CreateTodoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        var todo = await _todoService.CreateTodoAsync(userId, request);
        
        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TodoResponse>> UpdateTodo(int id, [FromBody] UpdateTodoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        var todo = await _todoService.UpdateTodoAsync(id, userId, request);
        
        if (todo == null)
            return NotFound();
            
        return Ok(todo);
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> ToggleTodo(int id, [FromBody] ToggleTodoRequest request)
    {
        var userId = GetUserId();
        var result = await _todoService.ToggleTodoAsync(id, userId, request);
        
        if (!result)
            return NotFound();
            
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var userId = GetUserId();
        var result = await _todoService.DeleteTodoAsync(id, userId);
        
        if (!result)
            return NotFound();
            
        return NoContent();
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTodoCount()
    {
        var userId = GetUserId();
        var count = await _todoService.GetTodoCountAsync(userId);
        return Ok(count);
    }
}