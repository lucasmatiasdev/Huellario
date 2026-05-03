namespace domain.dtos.Product;

public class ProductImageDto{
    public int Id { get; set; }
    public required string Url { get; set; }
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
}