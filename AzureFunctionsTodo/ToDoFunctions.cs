using AzureFunctionsTodo.DTOs;
using AzureFunctionsTodo.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
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

                var response = req.CreateResponse(HttpStatusCode.OK);
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

    }
}
