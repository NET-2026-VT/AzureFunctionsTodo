using AzureFunctionsTodo.DTOs;
using AzureFunctionsTodo.Models;

namespace AzureFunctionsTodo.Services
{
    public interface ITodoService
    {
        Task<ToDoItem> CreateTodoAsync(CreateTodoRequestDto request);
        Task<bool> DeleteTodoAsync(string id);
        Task<List<ToDoItem>> GetAllTodosAsync();
        Task<ToDoItem?> GetTodoAsync(string id);
        Task<ToDoItem?> UpdateTodoAsync(string id, UpdateTodoRequestDto request);
    }
}