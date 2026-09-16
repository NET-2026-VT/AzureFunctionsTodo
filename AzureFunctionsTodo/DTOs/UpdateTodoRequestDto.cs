using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsTodo.DTOs
{
    public class UpdateTodoRequestDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
    }
}
