using API.Common.Mapping;
using API.SignalR;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Intrefaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stripe;

namespace API.Controllers;

public class PaymentsController(IPaymentService paymentService,
    IUnitOfWork unit, ILogger<PaymentsController> logger, 
    IConfiguration config, IHubContext<NotificationHub> hubContext) 
        : BaseApiController
{
    private readonly string _whSecret = config["StripeSettings:WhSecret"]!;

    /// <summary>
    /// Создание или обновление PaymentIntent.
    /// </summary>
    /// <remarks>
    /// Вызывать перед оформлением заказа. 
    /// Если PaymentIntent уже существует - обновляет сумму.
    /// Требует валидной корзины с товарами.
    /// </remarks>
    /// <response code="200">PaymentIntent успешно создан/обновлён.</response>
    /// <response code="400">Проблема с корзиной (cart).</response>
    /// <param name="cartId">Идентификатор корзины</param>
    [HttpPost("{cartId}")]
    [Authorize]
    [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShoppingCart>> CreateOrUpdatePaymentIntent(string cartId)
        {
            var cart = await paymentService.CreateOrUpdatePaymentIntent(cartId);
            if(cart is null) return BadRequest("Problem with your cart");
            return Ok(cart);
        }

    /// <summary>
    /// Получение методов доставки.
    /// </summary>
    /// <response code="200">Список методов доставки. Возвращает пустой массив если методов доставки нет.</response>
    [HttpGet("delivery-methods")]
    [ProducesResponseType(typeof(IReadOnlyList<DeliveryMethod>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
    {
        return Ok(await unit.Repository<DeliveryMethod>().ListAllAsync());
    }

    [HttpPost("webhook")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = ConstructStripeEvent(json);
            if(stripeEvent.Data.Object is not PaymentIntent intent)
            {
                return BadRequest("Invalid event data");
            }

            await HandlePaymentIntentSucceeded(intent);

            return Ok();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe Webhook Error");
            return StatusCode(StatusCodes.Status500InternalServerError, "Webhook Error");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred");
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
        }
    }

    private async Task HandlePaymentIntentSucceeded(PaymentIntent intent)
    {
        if(intent.Status == "succeeded")
        {
            var spec = new OrderSpecification(intent.Id, true);
            var order = await unit.Repository<Order>().GetEntityWithSpec(spec) 
                ?? throw new Exception("Order not found");

            var orderTotalInCents = (long)Math.Round(order.GetTotal() * 100,
                 MidpointRounding.AwayFromZero);

            if(orderTotalInCents != intent.Amount)
            {
                order.Status = OrderStatus.PaymentMisMatch;
            }
            else
            {
                order.Status = OrderStatus.PaymentReceived;
            }

            await unit.Complete();
            
            var connectionid = NotificationHub.GetConnectionIdByEmail(order.BuyerEmail);

            if (!string.IsNullOrEmpty(connectionid)){
                await hubContext.Clients.Client(connectionid).
                    SendAsync("OrderCompleteNotification", order.ToDTO());
            }
        }
    }

    private Event ConstructStripeEvent(string json)
    {
        try
        {
            return EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _whSecret);
        }
        catch (Exception ex)
        {
            
            logger.LogError(ex, "Failed to construct stripe event");
            throw new StripeException("Invalid signature");
        }
    }
}
