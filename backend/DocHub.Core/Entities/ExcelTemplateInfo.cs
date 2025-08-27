using System.Collections.Generic;

namespace DocHub.Core.Entities
{
    public class ExcelTemplateInfo
    {
        public List<string> SheetNames { get; set; } = new();
        public List<string> Headers { get; set; } = new();
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
    }
}
