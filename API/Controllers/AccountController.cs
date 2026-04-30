using System.Security.Claims;
using API.Common.Claims;
using API.Common.Mapping;
using API.DTOs;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountController(SignInManager<AppUser> signInManager) : BaseApiController
{
    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    /// <param name="registerDTO">Данные для регистрации.</param>
    /// <response code="200">Пользователь успешно зарегистрирован.</response>
    /// <response code="400">Ошибки валидации (например, email уже занят или слабый пароль).</response>
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDTO registerDTO)
    {
        var user = new AppUser()
        {
            FirstName = registerDTO.FirstName,
            LastName = registerDTO.LastName,
            Email = registerDTO.Email,
            UserName = registerDTO.Email
        };

        var result = await signInManager.UserManager.CreateAsync(user, registerDTO.Password);

        if(!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem();
        }

        return Ok();
    }

    /// <summary>
    /// Выход из текущей сессии.
    /// </summary>
    /// <response code="204">Выход выполнен успешно.</response>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    /// <summary>
    /// Получение информации о текущем пользователе.
    /// </summary>
    /// <remarks>
    /// Если пользователь не авторизован — возвращает 204 No Content.
    /// Если авторизован — возвращает профиль с адресом и ролями.
    /// </remarks>
    /// <response code="200">Данные пользователя.</response>
    /// <response code="204">Пользователь не авторизован.</response>
    [HttpGet("user-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> GetUserInfo()
    {
        if (User.Identity?.IsAuthenticated is false) return NoContent();
        var user = await signInManager.UserManager.GetUserByEmailWithAddress(User);
        return Ok(new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            Address = user.Address?.ToDTO(),
            Roles = User.FindFirstValue(ClaimTypes.Role)
        });

    }

    /// <summary>
    /// Проверка статуса аутентификации текущего запроса.
    /// </summary>
    /// <response code="200">Возвращает <c>isAuthenticated: true/false</c>.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetAuthState()
    {
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
        });
    }

    /// <summary>
    /// Создание или обновление адреса текущего пользователя.
    /// </summary>
    /// <param name="addressDTO">Данные адреса.</param>
    /// <response code="200">Адрес успешно сохранён, возвращает обновлённый адрес.</response>
    /// <response code="400">Ошибка при сохранении адреса.</response>
    [HttpPost("address")]
    [Authorize]
    [ProducesResponseType(typeof(AddressDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Address>> CreateOrUpdateAddress(AddressDTO addressDTO)
    {
        var user = await signInManager.UserManager.GetUserByEmailWithAddress(User);
        if(user.Address is null)
        {
            user.Address = addressDTO.ToEntity();
        }
        else
        {
            user.Address.UpdateFromDTO(addressDTO);
        }

        var result = await signInManager.UserManager.UpdateAsync(user);

        return !result.Succeeded ? BadRequest("Problem updating user address") : Ok(user.Address.ToDTO()); 
    }
}
