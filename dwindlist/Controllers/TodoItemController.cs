using dwindlist.Dtos;
using dwindlist.Models.EntityManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace dwindlist.Controllers;

public class TodoItemController : Controller
{
    public string? GetUserId(ClaimsIdentity? claimsIdentity)
    {
        if (claimsIdentity == null)
        {
            return null;
        }

        var userIdClaim = claimsIdentity.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return null;
        }

        return userIdClaim.Value;
    }

    [Authorize]
    public IActionResult Index(int? id)
    {
        if (id == null)
        {
            id = 0;
        }

        var userId = GetUserId(User.Identity as ClaimsIdentity);
        if (userId == null)
        {
            return BadRequest();
        }

        var todoItemManager = new TodoItemManager();
        var todoList = todoItemManager.GetTodoList(userId, (int)id);

        return View(todoList);
    }

    [Authorize]
    [HttpPost]
    public ActionResult Add([FromBody] TodoItemAddDto todoItemAddDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId(User.Identity as ClaimsIdentity);
        if (userId == null)
        {
            return BadRequest();
        }

        todoItemAddDto.UserId = userId;
        var todoItemManager = new TodoItemManager();
        todoItemAddDto.Id = todoItemManager.AddItem(todoItemAddDto);
        todoItemAddDto.UserId = null;

        var json = JsonSerializer.Serialize(todoItemAddDto);

        return Ok(json);
    }
}
