using System.Security.Claims;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BagController : BaseApiController
{   
    /// <summary>
    /// Ошибка 401
    /// </summary>
    /// <response code="401">Unauthorized</response>
    [HttpGet("unauthorized")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetUnauthorized()
    {
        return Unauthorized();
    }

    /// <summary>
    /// Ошибка 400
    /// </summary>
    /// <response code="400">BadRequest</response>
    [HttpGet("badrequest")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetBadRequest()
    {
        return BadRequest();
    }

    /// <summary>
    /// Ошибка 404
    /// </summary>
    /// <response code="404">NotFound</response>
    [HttpGet("notfound")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetNotFound()
    {
        return NotFound();
    }

    /// <summary>
    /// Ошибка 500
    /// </summary>
    /// <response code="500">InternalError</response>
    [HttpGet("internalerror")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetInternalError()
    {
        throw new Exception("This is a test exception");
    }

    /// <summary>
    /// Ошибка 500
    /// </summary>
    /// <response code="500">ValidationError</response>
    [HttpPost("validationerror")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetValidationError(CreateProductDTO product)
    {
        return Ok();
    }

    /// <summary>
    /// Проверка авторизации
    /// </summary>
    /// <response code="200">AuthroizeCheck</response>
    [Authorize]
    [HttpGet("secret")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetSecret()
    {
      var name = User.FindFirst(ClaimTypes.Name)?.Value;
      var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      return Ok($"Hello {name} with the id of {id}");
    }

    /// <summary>
    /// Проверка роли Админа
    /// </summary>
    /// <response code="200">AdminCheck</response>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin-secret")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAdminSecret()
    {
      var name = User.FindFirst(ClaimTypes.Name)?.Value;
      var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var isAdmin = User.IsInRole("Admin");
      var roles = User.FindFirstValue(ClaimTypes.Role);

      return Ok(new
      {
          name,
          id,
          isAdmin,
          roles
      });
    }
}
