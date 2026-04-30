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

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpGet("user-info")]
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

    [HttpGet]
    public ActionResult GetAuthState()
    {
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
        });
    }

    [HttpPost("address")]
    [Authorize]
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
