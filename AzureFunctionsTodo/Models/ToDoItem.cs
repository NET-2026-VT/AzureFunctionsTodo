using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsTodo.Models
{
    public class ToDoItem : ITableEntity
    {
        public string PartitionKey { get; set; } = "TODO";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; 
    }
}
