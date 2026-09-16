using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsTodo.DTOs
{
    public class CreateTodoRequestDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
