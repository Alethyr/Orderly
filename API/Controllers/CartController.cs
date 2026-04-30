using Core.Entities;
using Core.Intrefaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class CartController(ICartService cartService) : BaseApiController
{
    /// <summary>
    /// Получение корзины по id.
    /// </summary>
    /// <response code="200">Корзина по id успешно найдена.</response>
    /// <param name="id">Идентификатор корзины</param>
    [HttpGet]
    [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
    public async Task<ActionResult<ShoppingCart>> GetCartById(string id)
    {
        ShoppingCart? cart = await cartService.GetCartAsync(id);
        return Ok(cart ?? new ShoppingCart { Id = id });
    }

    /// <summary>
    /// Создание/обновление корзины.
    /// </summary>
    /// <response code="200">Корзина по id успешно создана/обновлена.</response>
    /// <param name="cart">Данные корзины</param>
    [HttpPost]
    [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
    public async Task<ActionResult<ShoppingCart>> UpsertCart(ShoppingCart cart)
    {
        ShoppingCart? upserCart = await cartService.SetCartAsync(cart);
        return Ok(upserCart);
    }

    /// <summary>
    /// Удаление корзины.
    /// </summary>
    /// <response code="200">Корзина по id успешно удалена.</response>
    /// <response code="404">Корзина не существует..</response>
    /// <param name="id">Идентификатор корзины</param>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCart(string id)
    {
        bool isDeleted = await cartService.DeleteCartAsync(id);
        return isDeleted ? NoContent() : NotFound("Cart doesnt exist.");
    }
}
