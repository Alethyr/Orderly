using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateProductDTO
{
    [Required]
    public  string Name { get; set; } = null!;
    [Required]
    public  string Description { get; set; } = null!;
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; } 
    [Required]
    public  string PictureUrl { get; set; } = null!;
    [Required]
    public  string Type { get; set; } = null!;
    [Required]
    public  string Brand { get; set; } = null!;
    [Range(1, int.MaxValue, ErrorMessage = "Quantity in stock must be atleast 1")]
    public int QuantityInStock { get; set; }
}
