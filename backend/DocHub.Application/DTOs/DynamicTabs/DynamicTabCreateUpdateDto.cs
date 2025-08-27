using System.Collections.Generic;

namespace DocHub.Application.DTOs.DynamicTabs;

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
