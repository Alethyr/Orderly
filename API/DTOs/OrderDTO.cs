using System.ComponentModel.DataAnnotations;
using Core.Entities.OrderAggregate;

namespace API.DTOs;

public class OrderDTO
{
  public int Id {get;set;}
  public DateTime OrderDate { get; set; }
  [Required]
  public  string BuyerEmail{ get; set; }  = null!;
  [Required]
  public  ShippingAddress ShippingAddress { get; set; } = null!;
  [Required]
  public  string DeliveryMethod { get; set; } = null!;
  public decimal ShippingPrice { get; set; }
  [Required]
  public  PaymentSummary PaymentSummary { get; set; } = null!;
  [Required]
  public  List<OrderItemDTO> OrderItems { get; set; } = null!;
  public decimal SubTotal { get; set; }
  [Required]
  public  string Status { get; set; } = null!;
  public decimal Total { get; set; }
  [Required]
  public  string PaymentIntentId { get; set; } = null!;


}
