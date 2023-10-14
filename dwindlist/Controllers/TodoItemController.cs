using dwindlist.Dtos;
using dwindlist.Models.EntityManager;
using dwindlist.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace dwindlist.Controllers
{
    public class TodoItemController : Controller
    {
        private static string? GetUserId(ClaimsIdentity? claimsIdentity)
        {
            if (claimsIdentity == null)
            {
                return null;
            }

            Claim? userIdClaim = claimsIdentity.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            return userIdClaim?.Value;
        }

        [Authorize]
        public IActionResult Index(int? id)
        {
            id ??= 0;

            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return BadRequest();
            }

            TodoItemManager todoItemManager = new();
            TodoList todoList = todoItemManager.GetTodoList(userId, (int)id);

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

            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return BadRequest();
            }

            TodoItemManager todoItemManager = new();
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

            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return BadRequest();
            }

            TodoItemManager todoItemManager = new();
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

            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return BadRequest();
            }

            TodoItemManager todoItemManager = new();
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

            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return BadRequest();
            }

            TodoItemManager todoItemManager = new();
            todoItemManager.UpdateItemLabel(userId, id, todoItemDto);

            return Ok();
        }
    }
}
