using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class OrderItemDTO
{
    public int ProductId { get; set; } 
    [Required]
    public string ProductName { get; set; } = null!;
    [Required]
    public string PictureUrl { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}