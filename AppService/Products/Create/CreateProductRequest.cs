namespace AppService.Products.Create;

public record CreateProductRequest(string ProductName, decimal Price, int Stock, int CategoryId);


