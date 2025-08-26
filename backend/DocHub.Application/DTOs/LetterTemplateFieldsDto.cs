using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class FieldReorderDto
{
    public string Id { get; set; } = string.Empty;
    public int NewSortOrder { get; set; }
}
