using System.Security.Claims;
using System.Text.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using dwindlist.Models.EntityManager;
using dwindlist.Dtos;

namespace dwindlist.Controllers;

public class TodoItemController : Controller
{
    [Authorize]
    [HttpPost]
    public ActionResult Add(TodoItemAddDto todoItemAddDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var claimsIdentity = User.Identity as ClaimsIdentity;
        if (claimsIdentity == null)
        {
            return Ok("{\"text\": \"null claim\"}");
            /* return BadRequest(); */
        }

        var userIdClaim = claimsIdentity.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Ok("{\"text\": \"null userid claim\"}");
            /* return BadRequest(); */
        }

        var userIdValue = userIdClaim.Value;

        todoItemAddDto.UserId = userIdValue;
        var todoItemManager = new TodoItemManager();
        var itemId = todoItemManager.AddItem(todoItemAddDto);
        todoItemAddDto.UserId = null;

        var json = JsonSerializer.Serialize(todoItemAddDto);

        return Ok(json);
    }
}
