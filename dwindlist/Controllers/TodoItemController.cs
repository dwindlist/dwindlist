using dwindlist.Dtos;
using dwindlist.Models.EntityManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace dwindlist.Controllers;

public class TodoItemController : Controller
{
    [Authorize]
    [HttpPost]
    public ActionResult Add([FromBody] TodoItemAddDto todoItemAddDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var claimsIdentity = User.Identity as ClaimsIdentity;
        if (claimsIdentity == null)
        {
            return BadRequest();
        }

        var userIdClaim = claimsIdentity.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return BadRequest();
        }

        var userIdValue = userIdClaim.Value;

        todoItemAddDto.UserId = userIdValue;
        var todoItemManager = new TodoItemManager();
        todoItemAddDto.Id = todoItemManager.AddItem(todoItemAddDto);
        todoItemAddDto.UserId = null;

        var json = JsonSerializer.Serialize(todoItemAddDto);

        return Ok(json);
    }
}
