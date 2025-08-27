using System.Collections.Generic;

namespace DocHub.Application.DTOs
{
    public class SheetInfo
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = new();
        public bool HasHeaderRow { get; set; } = true;
        public int DataRowCount { get; set; }
        public List<string> DataTypes { get; set; } = new();
        public List<string> SampleData { get; set; } = new();
    }
}
