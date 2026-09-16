using Azure;
using Azure.Data.Tables;
using AzureFunctionsTodo.DTOs;
using AzureFunctionsTodo.Models;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsTodo.Services
{
    public class TodoService : ITodoService
    {
        private readonly TableClient _tableClient;
        public TodoService(TableServiceClient tableServiceClient)
        {
            _tableClient = tableServiceClient.GetTableClient("TodoItems");
            _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();

        }

        public async Task<ToDoItem> CreateTodoAsync(CreateTodoRequestDto request)
        {
            var todoItem = new ToDoItem
            {
                PartitionKey = "TODO",
                RowKey = Guid.NewGuid().ToString(),
                Title = request.Title,
                Description = request.Description,
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow

            };

            await _tableClient.AddEntityAsync(todoItem);
            return todoItem;
        }

        public async Task<ToDoItem?> GetTodoAsync(string id)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<ToDoItem>("TODO", id);
                return response.Value;
            }
            catch (RequestFailedException)
            {

                return null;
            }
        }

        public async Task<List<ToDoItem>> GetAllTodosAsync()
        {
            List<ToDoItem> todos = new List<ToDoItem>();

            await foreach (var todo in _tableClient.QueryAsync<ToDoItem>(filter: $"PartitionKey eq 'TODO'"))
            {
                todos.Add(todo);
            }
            return todos.OrderByDescending(t => t.CreatedDate).ToList();
        }
        public async Task<ToDoItem?> UpdateTodoAsync(string id, UpdateTodoRequestDto request)
        {
            var existingToDo = await GetTodoAsync(id);

            if (existingToDo == null)
                return null;

            existingToDo.Title = request.Title;
            existingToDo.Description = request.Description;
            existingToDo.IsCompleted = request.IsCompleted;

            await _tableClient.UpdateEntityAsync(existingToDo, existingToDo.ETag);
            return existingToDo;

        }

        public async Task<bool> DeleteTodoAsync(string id)
        {
            try
            {
                await _tableClient.DeleteEntityAsync("TODO", id);
                return true;
            }
            catch (RequestFailedException)
            {

                return false;
            }
        }
    }
}
