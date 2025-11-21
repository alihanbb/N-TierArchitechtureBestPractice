namespace AppService.Categories.Dto;

public class CategoryDto
{
    // Parameterless constructor for AutoMapper
    public CategoryDto() { }

    // Constructor for unit tests
    public CategoryDto(int categoryId, string categoryName)
    {
        CategoryId = categoryId;
        CategoryName = categoryName;
    }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
}

