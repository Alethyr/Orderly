using API.Common.Mapping;
using API.DTOs;
using API.RequestHelpers;
using Core.Entities.OrderAggregate;
using Core.Intrefaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(IUnitOfWork unit, IPaymentService paymentService) : BaseApiController
{
    /// <summary>
    /// Получение списка всех заказов с поддержкой фильтрации и пагинации.
    /// </summary>
    /// <response code="200">Список заказов успешно получен.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="403">Недостаточно прав для выполнения операции.</response>
    /// <param name="specParams">Параметры спецификации (фильтрация, сортировка, пагинация)</param>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(Pagination<OrderDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderDTO>>> GetOrders([FromQuery] OrderSpecParams specParams)
    {
        var spec = new OrderSpecification(specParams);
        return await CreatePagedResult(unit.Repository<Order>(), spec, specParams.PageIndex, specParams.PageSize, o => o.ToDTO());
    }

    /// <summary>
    /// Получение заказа по id.
    /// </summary>
    /// <response code="200">Заказ успешно получен.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="403">Недостаточно прав для выполнения операции.</response>
    /// <response code="404">Заказ не найден.</response>
    /// <param name="id">Идентификатор заказа</param>
    [HttpGet("orders/{id:int}")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDTO>> GetOrderById(int id)
    {
        var spec = new OrderSpecification(id);
        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);
        if(order is null) return NotFound("No order with that ID");
        return Ok(order.ToDTO());
    }

    /// <summary>
    /// Возврат заказа по id.
    /// </summary>
    /// <response code="200">Возврат успешен.</response>
    /// <response code="400">Проблема с возвратом.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="403">Недостаточно прав для выполнения операции.</response>
    /// <response code="404">Заказ не найден.</response>
    /// <response code="409">Заказ не был оплачен.</response>
    /// <param name="id">Идентификатор заказа</param>
    [HttpPost("orders/refund/{id:int}")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDTO>> RefundOrder(int id)
    {
        var spec = new OrderSpecification(id);
        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);

        if(order is null) 
            return NotFound("No order with that ID");

        if(order.Status == OrderStatus.Pending)
            return Conflict("Payment not recieved for this order");

        var result = await paymentService.RefundPayment(order.PaymentIntentId);
        
        if(result == "succeeded")
        {
            order.Status = OrderStatus.Refunded;
            await unit.Complete();
            return Ok(order.ToDTO());
        }

        return BadRequest("Problem with refunding order");      
    }
}

