using Core.Entities;
using Core.Intrefaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class CouponsController(ICouponService couponService) : BaseApiController
{
    /// <summary>
    /// Валидация купона.
    /// </summary>
    /// <response code="200">Купон валидный.</response>
    /// <response code="400">Купон невалидный.</response>
    /// <param name="code">Name купона</param>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(AppCoupon), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AppCoupon>> ValidateCoupon(string code)
    {
        var coupon = await couponService.GetCouponFromPromoCodeAsync(code);
        if(coupon is null) return BadRequest("Invalid coupon code");
        return Ok(coupon);
    }
    
}
