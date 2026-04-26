using API.DTOs;
using API.Extensions;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Intrefaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class OrdersController(ICartService cartService, IUnitOfWork unit) : BaseApiController
{
  [HttpPost]
  public async Task<ActionResult<Order>> CreateOrder(CreateOrderDTO OrderDTO)
    {
        var email = User.GetEmail();
        var cart = await cartService.GetCartAsync(OrderDTO.CartId);

        if(cart is null) return BadRequest("Cart not found");
        if(cart.PaymentIntentId is null) return BadRequest("No paymentIntent for this order");
        
        var items = new List<OrderItem>();
        foreach (var item in cart.Items)
        {
            var productItem = await unit.Repository<Product>().GetByIdAsync(item.ProductId);
            if(productItem is null) return BadRequest("Problem with the order");
            var itemOrder = new ProductItemOrdered
            {
                ProductId = productItem.Id,
                ProductName = productItem.Name,
                PictureUrl = productItem.PictureUrl
            };
            
            var orderItem = new OrderItem
            {
                ItemOrdered = itemOrder,
                Price = productItem.Price,
                Quantity = item.Quantity
            };
            items.Add(orderItem);
        }

        var deliveryMethod = await unit.Repository<DeliveryMethod>().GetByIdAsync(OrderDTO.DeliveryMethodId);
        if(deliveryMethod is null) return BadRequest("No delivery method selected");

        var order = new Order
        {
          OrderItems = items,
          DeliveryMethod = deliveryMethod,
          ShippingAddress = OrderDTO.ShippingAddress,
          SubTotal = items.Sum(x => x.Price * x.Quantity),
          PaymentSummary = OrderDTO.PaymentSummary,
          PaymentIntentId = cart.PaymentIntentId,
          BuyerEmail = email
        };

        unit.Repository<Order>().Add(order);
        if(await unit.Complete()) return order;
        return BadRequest("Problem with creating order");
    }

   
   [HttpGet]
   public async Task<ActionResult<IReadOnlyList<Order>>> GetOrdersForUser()
    {
        var spec = new OrderSpecification(User.GetEmail());
        var orders = await unit.Repository<Order>().ListAsync(spec);
        var ordersToReturn = orders.Select(o => o.ToDTO()).ToList();
        if(orders is null) return NotFound();
        return Ok(ordersToReturn);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDTO>> GetOrderById(int id)
    {
        var spec = new OrderSpecification(User.GetEmail(), id);
        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);
        if(order is null) return NotFound();
        return order.ToDTO();
    }

}
