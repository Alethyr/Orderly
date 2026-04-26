using API.DTOs;
using Core.Entities.OrderAggregate;

namespace API.Extensions;

public static class OrderMappingExtension
{
  public static OrderDTO ToDTO(this Order order)
    {
        return new OrderDTO
        {
            Id = order.Id,
            BuyerEmail = order.BuyerEmail,
            OrderDate = order.OrderDate,
            ShippingAddress = order.ShippingAddress,
            PaymentSummary = order.PaymentSummary,
            DeliveryMethod = order.DeliveryMethod.Description,
            ShippingPrice = order.DeliveryMethod.Price,
            OrderItems = order.OrderItems.Select(x => x.ToDTO()).ToList(),
            SubTotal = order.SubTotal,
            Total = order.GetTotal(),
            Status = order.Status.ToString()         
        };
    }

    public static OrderItemDTO ToDTO(this OrderItem orderItem)
    {
        return new OrderItemDTO
        {
             ProductId = orderItem.ItemOrdered.ProductId,
             ProductName = orderItem.ItemOrdered.ProductName,
             PictureUrl = orderItem.ItemOrdered.PictureUrl,
             Price = orderItem.Price,
             Quantity = orderItem.Quantity
        };
    }

}
