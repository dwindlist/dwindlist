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
    public ActionResult Add(int id, [FromBody] TodoItemDto todoItemDto)
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
        todoItemManager.AddItem(userId, id, todoItemDto);

        return Ok();
    }

    [Authorize]
    [HttpPut]
    public ActionResult ToggleStatus(int id)
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
        todoItemManager.ToggleItemStatus(userId, id);

        return Ok();
    }

    [Authorize]
    [HttpPut]
    public ActionResult ToggleExpanded(int id)
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
        todoItemManager.ToggleItemExpanded(userId, id);

        return Ok();
    }

    [Authorize]
    [HttpPut]
    public ActionResult UpdateLabel(int id, [FromBody] TodoItemDto todoItemDto)
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
        todoItemManager.UpdateItemLabel(userId, id, todoItemDto);

        return Ok();
    }
}
