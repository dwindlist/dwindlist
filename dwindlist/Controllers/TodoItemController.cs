using dwindlist.Dtos;
using dwindlist.Models.EntityManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace dwindlist.Controllers;

public class TodoItemController : Controller
{
    private string? GetUserId(ClaimsIdentity? claimsIdentity)
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

        var todoItemManager = new TodoItemManager();
        todoItemManager.AddItem(userId, todoItemAddDto);

        return Ok();
    }

    [Authorize]
    [HttpPut]
    public ActionResult Toggle(int id)
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

        var todoItemManager = new TodoItemManager();
        todoItemManager.ToggleItem(userId, id);

        return Ok();
    }

    [Authorize]
    [HttpPut]
    public ActionResult UpdateLabel(int id, [FromBody] TodoItemUpdateDto todoItemUpdateDto)
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

        var todoItemManager = new TodoItemManager();
        todoItemManager.UpdateItemLabel(userId, id, todoItemUpdateDto);

        return Ok();
    }
}
