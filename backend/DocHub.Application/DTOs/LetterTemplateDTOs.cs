using System.Text.Json.Serialization;

namespace DocHub.Application.DTOs
{
    public class LetterTemplateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string DataSource { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public List<LetterTemplateFieldDto> Fields { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
    }

    public class LetterTemplateFieldDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public FieldType Type { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }
        public string ValidationRegex { get; set; }
        public string DataSourceField { get; set; }
        public int SortOrder { get; set; }
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FieldType
    {
        Text,
        Number,
        Date,
        Select,
        MultiSelect,
        Boolean,
        File,
        Image,
        RichText,
        Signature
    }

    public class CreateLetterTemplateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string DataSource { get; set; }
        public bool IsActive { get; set; }
        public List<LetterTemplateFieldDto> Fields { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class UpdateLetterTemplateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string DataSource { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class CreateLetterTemplateFieldDto
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public FieldType Type { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }
        public string ValidationRegex { get; set; }
        public string DataSourceField { get; set; }
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    public class UpdateLetterTemplateFieldDto
    {
        public string Label { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }
        public string ValidationRegex { get; set; }
        public string DataSourceField { get; set; }
        public Dictionary<string, object> Configuration { get; set; }
    }

    public class FieldReorderDto
    {
        public string FieldId { get; set; }
        public int NewSortOrder { get; set; }
    }
}
