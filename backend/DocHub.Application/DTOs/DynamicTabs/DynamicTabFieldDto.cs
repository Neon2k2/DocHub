using System.Collections.Generic;

namespace DocHub.Application.DTOs.DynamicTabs;

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

public class DynamicTabFieldUpdateDto
{
    public string Label { get; set; }
    public bool IsRequired { get; set; }
    public string DefaultValue { get; set; }
    public string ValidationRules { get; set; }
    public int Order { get; set; }
    public Dictionary<string, object> Configuration { get; set; }
}
