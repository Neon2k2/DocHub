namespace DocHub.Application.DTOs
{


    public class DynamicTabFieldDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }
        public string ValidationRules { get; set; }
        public int Order { get; set; }
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    public class CreateDynamicTabDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DynamicTabFieldDto> Fields { get; set; } = new();
        public Dictionary<string, object> Configuration { get; set; } = new();
        public bool IsVisible { get; set; }
        public int Order { get; set; }
    }

    public class UpdateDynamicTabDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Configuration { get; set; }
        public bool IsVisible { get; set; }
        public int Order { get; set; }
    }

    public class DynamicTabFieldUpdateDto
    {
        public string Label { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }
        public string ValidationRules { get; set; }
        public int Order { get; set; }
        public Dictionary<string, object> Configuration { get; set; }
    }

}
