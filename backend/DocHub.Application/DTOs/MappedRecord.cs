using System.Collections.Generic;

namespace DocHub.Application.DTOs
{
    public class MappedRecord
    {
        public Dictionary<string, object> Data { get; set; } = new();
        public List<string> ValidationErrors { get; set; } = new();
        public bool IsValid => ValidationErrors.Count == 0;
    }
}
