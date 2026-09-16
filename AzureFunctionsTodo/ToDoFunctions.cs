using AzureFunctionsTodo.DTOs;
using AzureFunctionsTodo.Models;
using AzureFunctionsTodo.Services;
using Grpc.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace AzureFunctionsTodo
{
    public class ToDoFunctions
    {
        private readonly ITodoService _todoService;

        public ToDoFunctions(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [Function("CreateTodo")]
        public async Task<HttpResponseData> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route ="todos")] HttpRequestData req,
            FunctionContext functionContext)
        {
            var logger = functionContext.GetLogger("TodoFunctions");

            try
            {
                logger.LogInformation("Creating a new todo item");

                using var reader = new StreamReader(req.Body);
                var requestBody = await reader.ReadToEndAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var request = JsonSerializer.Deserialize<CreateTodoRequestDto>(requestBody, options);

                if(request == null || string.IsNullOrEmpty(request.Title))
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Title is requred");
                    return badResponse; 
                }

                var todo = await _todoService.CreateTodoAsync(request);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(todo);

                return response; 
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating todo item");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Something went wrong when trying to create a todo item");
                return errorResponse; 

            }
        }

        [Function("GetTodo")]
        public async Task<HttpResponseData> GetTodo(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route ="todos/{id}")] HttpRequestData req, string id, FunctionContext functionContext)
        {
            var logger = functionContext.GetLogger("TodoFunctions");
            try
            {
                logger.LogInformation($"Getting todo item with id: {id}");

                var todo = await _todoService.GetTodoAsync(id);

                if(todo == null)
                {
                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync($"Todo with id {id} not found");
                    return notFound;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(todo);
                return response; 
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting todo item");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Couldn't find todo item");
                return error; 
                
            }
        }

        [Function("GetAllTodos")]
        public async Task<HttpResponseData> GetAllTodos(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route ="todos")] HttpRequestData req, FunctionContext functionContext)
        {
            var logger = functionContext.GetLogger("TodoFunctions");

            try
            {
                logger.LogInformation("Getting all todo items");
                var todos = await _todoService.GetAllTodosAsync();

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(todos ?? new List<ToDoItem>());
                return response; 
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting todo items");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Error getting all the todos");
                return error;
            }
        }

        [Function("UpdateTodo")]
        public async Task<HttpResponseData> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "todos/{id}")] HttpRequestData req, string id, FunctionContext functionContext)
        {
            var logger = functionContext.GetLogger("TodoFunctions");

            try
            {
                logger.LogInformation($"Updating todo item with id: {id}");

                using var reader = new StreamReader(req.Body);
                var requestBody = await reader.ReadToEndAsync();

                var request = JsonSerializer.Deserialize<UpdateTodoRequestDto>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (request == null || string.IsNullOrEmpty(request.Title))
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Title is requred");
                    return badResponse;
                }

                var updated = await _todoService.UpdateTodoAsync(id, request);

                if(updated == null)
                {
                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync($"Todo with id {id} not found");
                    return notFound; 
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(updated);
                return response; 

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating todo items");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Error updating the todo item");
                return error;
            }
        }

        [Function("DeleteTodo")]
        public async Task<HttpResponseData> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route ="todos/{id}")] HttpRequestData req, string id, FunctionContext functionContext)
        {
            var logger = functionContext.GetLogger("TodoFunctions");
            try
            {
                logger.LogInformation($"Deleting todo item with id: {id}");

                var deleted = await _todoService.DeleteTodoAsync(id);

                if (!deleted)
                {
                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync($"Todo with id {id} not found");
                    return notFound;
                }

                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response; 
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting todo item");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Error deleting the todo item");
                return error;
            }
        }


    }
}
