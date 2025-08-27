using System.ComponentModel.DataAnnotations;
using DocHub.Application.Validation;

namespace DocHub.Application.DTOs.DynamicTabs;

public class TabReorderDto
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int NewSortOrder { get; set; }
}
