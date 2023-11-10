using dwindlist.Dtos;
using dwindlist.Models.EntityManager;
using dwindlist.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace dwindlist.Controllers
{
    /// <summary>
    /// Handles requests and validation.
    /// </summary>
    /// <remarks>
    /// See <see cref="TodoItemManager"/> for actual business logic, which objects of this class call.
    /// </remarks>
    public class TodoItemController : Controller
    {
        /// <summary>
        /// Extracts <see cref="System.Security.Principal.IIdentity.Name">User ID</see>
        /// from a <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsPrincipal.Identity"/> (see example)</param>
        /// <returns>
        /// Extracted <see cref="System.Security.Principal.IIdentity.Name">User ID</see> if <paramref name="claimsIdentity"/> contains it;
        /// otherwise, returns <c>null</c>.
        /// </returns>
        /// <example>
        /// Example usage:
        /// <code>
        /// string? userId = GetUserId(User.Identity as ClaimsIdentity);
        /// </code>
        /// </example>
        /// <remarks>
        /// Theoretically should never return null, as API endpoints are decorated with <see cref="AuthorizeAttribute"/>.<br/>
        /// Return value can be used as a <see cref="Models.TodoItem.UserId">UserId</see> for <see cref="Models.TodoItem">TodoItem</see> lookup.
        /// </remarks>
        private static string? GetUserId(ClaimsIdentity? claimsIdentity)
        {
            if (claimsIdentity == null)
            {
                return null;
            }

            Claim? userIdClaim = claimsIdentity.Claims.FirstOrDefault(
                x => x.Type == ClaimTypes.NameIdentifier
            );

            return userIdClaim?.Value;
        }

        /// <summary>
        /// Gets the <see href="https://github.com/dwindlist/dwindlist/blob/main/dwindlist/Views/TodoItem/Index.cshtml">default list view</see>
        /// given a <see cref="Models.TodoItem">root item</see>.
        /// </summary>
        /// <param name="id">Id of the <see cref="Models.TodoItem">item</see> to be used as the <see cref="TodoList.RootId">root</see>.</param>
        /// <returns>
        /// Default list view with <paramref name="id"/> as the root id.
        /// </returns>
        /// <remarks>
        /// The root is the parent of the parent (outer) list items. See <see cref="TodoList"/>.
        /// </remarks>
        [Authorize]
        public IActionResult Index(int? id)
        {
            // get top level items if no root id provided.
            id ??= 0;

            // UserId should never be null because of the [Authorize] decorator,
            // but just for safety, always check anyway.
            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return HandleNoUserId();
            }

            TodoItemManager todoItemManager = new();
            TodoList todoList;
            try
            {
                todoList = todoItemManager.GetTodoList(userId, (int)id);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    // redirect to home if trying to access an illegal item
                    case ItemNotFoundException:
                    case ItemNotOwnedException:
                        return Redirect("/");
                    // otherwise, return internal server error
                    // TODO: create proper error page
                    default:
                        return InternalServerError(e, "An unknown error occurred");
                }
            }

            return View(todoList);
        }

        /// <summary>
        /// Searches for <see cref="Models.TodoItem">items</see> with matching labels.
        /// </summary>
        /// <param name="id">The search string.</param>
        /// <returns>
        /// <see href="https://github.com/dwindlist/dwindlist/blob/main/dwindlist/Views/TodoItem/Index.cshtml">Filtered list view</see>
        /// with search results if <paramref name="id"/> is provided;
        /// otherwise, redirects to default list view.
        /// </returns>
        /// <remarks>
        /// Matches are determined by substring.
        /// </remarks>
        [Authorize]
        public IActionResult Search(string id)
        {
            // UserId should never be null because of the [Authorize] decorator,
            // but just for safety, always check anyway.
            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return HandleNoUserId();
            }

            // If no search string is provided, redirect to the top level of the list.
            if (id == null)
            {
                return Redirect("/");
            }

            TodoItemManager todoItemManager = new();
            FilteredList filteredList = todoItemManager.SearchTodoList(userId, id);
            filteredList.Label = $"Search: {id}";

            return View("Filtered", filteredList);
        }

        /// <summary>
        /// Filters for <see cref="Models.TodoItem">items</see> marked as complete.
        /// </summary>
        /// <returns>
        /// <see href="https://github.com/dwindlist/dwindlist/blob/main/dwindlist/Views/TodoItem/Filtered.cshtml">Filtered list view</see>
        /// with completed items.
        /// </returns>
        [Authorize]
        public IActionResult Completed()
        {
            // UserId should never be null because of the [Authorize] decorator,
            // but just for safety, always check anyway.
            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return HandleNoUserId();
            }

            TodoItemManager todoItemManager = new();
            FilteredList filteredList = todoItemManager.FilterTodoListByStatus(userId, true);
            filteredList.Label = "Completed";

            return View("Filtered", filteredList);
        }

        /// <summary>
        /// Filters for <see cref="Models.TodoItem">items</see> marked as incomplete.
        /// </summary>
        /// <returns>
        /// <see href="https://github.com/dwindlist/dwindlist/blob/main/dwindlist/Views/TodoItem/Filtered.cshtml">Filtered list view</see>
        /// with incomplete items.
        /// </returns>
        [Authorize]
        public IActionResult Incomplete()
        {
            // UserId should never be null because of the [Authorize] decorator,
            // but just for safety, always check anyway.
            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return HandleNoUserId();
            }

            TodoItemManager todoItemManager = new();
            FilteredList filteredList = todoItemManager.FilterTodoListByStatus(userId, false);
            filteredList.Label = "Incomplete";

            return View("Filtered", filteredList);
        }

        /// <summary>
        /// Adds a new <see cref="Models.TodoItem">item</see>.
        /// </summary>
        /// <param name="id">The parent of which the item will be added to.</param>
        /// <param name="todoItemDto"><see cref="TodoItemDto">DTO</see> containing the new <see cref="Models.TodoItem">item</see>'s label.</param>
        /// <returns>
        /// Created HTTP status on success; otherwise, returns bad request
        /// (i.e., invalid model state).
        /// </returns>
        [Authorize]
        [HttpPost]
        public ActionResult Add(int? id, [FromBody] TodoItemDto todoItemDto)
        {
            // UserId should never be null because of the [Authorize] decorator,
            // but just for safety, always check anyway.
            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return HandleNoUserId();
            }

            // Reject request to add new item with invalid labels
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            id ??= 0;

            TodoItemManager todoItemManager = new();
            try
            {
                todoItemManager.AddItem(userId, (int)id, todoItemDto);
            }
            catch (Exception e)
            {
                return HandleErrorWithMessage(e, "Failed to add item");
            }

            const int createdHttpCode = 201;
            return StatusCode(createdHttpCode);
        }

        /// <summary>
        /// Toggles an <see cref="Models.TodoItem">item</see> as complete or incomplete.
        /// </summary>
        /// <param name="id">Id of the <see cref="Models.TodoItem">item</see> to be toggled complete or incomplete</param>
        /// <returns>
        /// An HTTP status. On success, also returns a boolean representing whether the frontend should update.
        /// </returns>
        [Authorize]
        [HttpPut]
        public ActionResult ToggleStatus(int id)
        {
            // UserId should never be null because of the [Authorize] decorator,
            // but just for safety, always check anyway.
            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return HandleNoUserId();
            }

            // tell frontend to update
            bool updateFrontend;

            TodoItemManager todoItemManager = new();
            try
            {
                updateFrontend = todoItemManager.ToggleItemStatus(userId, id);
            }
            catch (Exception e)
            {
                return HandleErrorWithMessage(e, "Failed to toggle item status");
            }

            return Ok(updateFrontend);
        }

        /// <summary>
        /// Expands or collapses an <see cref="Models.TodoItem">item</see>.
        /// </summary>
        /// <param name="id">Id of the <see cref="Models.TodoItem">item</see> to be expanded or collapsed</param>
        /// <returns>
        /// An HTTP status.
        /// </returns>
        [Authorize]
        [HttpPut]
        public ActionResult ToggleExpanded(int id)
        {
            // UserId should never be null because of the [Authorize] decorator,
            // but just for safety, always check anyway.
            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return HandleNoUserId();
            }

            TodoItemManager todoItemManager = new();

            try
            {
                todoItemManager.ToggleItemExpanded(userId, id);
            }
            catch (Exception e)
            {
                return HandleErrorWithMessage(e, "Failed to expand/collapse item");
            }

            return Ok();
        }

        /// <summary>
        /// Updates the label of an <see cref="Models.TodoItem">item</see>.
        /// </summary>
        /// <param name="id">Id of the <see cref="Models.TodoItem">item</see> to be updated.</param>
        /// <param name="todoItemDto"><see cref="TodoItemDto">DTO</see> containing the <see cref="Models.TodoItem">item</see>'s updated label.</param>
        /// <returns>
        /// An HTTP status.
        /// </returns>
        [Authorize]
        [HttpPut]
        public ActionResult UpdateLabel(int id, [FromBody] TodoItemDto todoItemDto)
        {
            // UserId should never be null because of the [Authorize] decorator,
            // but just for safety, always check anyway.
            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return HandleNoUserId();
            }

            // Reject request to update item with invalid label
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TodoItemManager todoItemManager = new();
            try
            {
                todoItemManager.UpdateItemLabel(userId, id, todoItemDto);
            }
            catch (Exception e)
            {
                return HandleErrorWithMessage(e, "Failed to update item label");
            }

            return Ok();
        }

        /// <summary>
        /// Deletes an <see cref="Models.TodoItem">item</see>.
        /// </summary>
        /// <param name="id">Id of the <see cref="Models.TodoItem">item</see> to be deleted.</param>
        /// <returns>
        /// An HTTP status. On success, also returns a boolean representing whether the frontend should update.
        /// </returns>
        /// <remarks>
        /// <see cref="Models.TodoItem">Items</see> are not totally deleted; they are marked with a property that is ignored during queries.
        /// </remarks>
        [Authorize]
        [HttpPut]
        public ActionResult Delete(int id)
        {
            // UserId should never be null because of the [Authorize] decorator,
            // but just for safety, always check anyway.
            string? userId = GetUserId(User.Identity as ClaimsIdentity);
            if (userId == null)
            {
                return HandleNoUserId();
            }

            // tell frontend to update
            bool updateFrontend;

            TodoItemManager todoItemManager = new();
            try
            {
                updateFrontend = todoItemManager.DeleteItem(userId, id);
            }
            catch (Exception e)
            {
                return HandleErrorWithMessage(e, "Failed to delete item");
            }

            return Ok(updateFrontend);
        }

        /// <summary>
        /// Unauthorized wrapper for custom message.
        /// </summary>
        /// <returns>
        /// <see cref="Unauthorized"/> status with custom message.
        /// </returns>
        private ActionResult HandleNoUserId()
        {
            return Unauthorized("Could not obtain user ID.");
        }

        /// <summary>
        /// StatusCode wrapper for Internal Server Error HTTP status.
        /// </summary>
        /// <param name="e">Exception that was caught</param>
        /// <param name="message">Custom message to be prepended to error message.</param>
        /// <returns>
        /// <see cref="StatusCodeResult"/> return value.
        /// </returns>
        private ActionResult InternalServerError(Exception e, string message)
        {
            const int internalSeverErrorCode = 500;
            string errorMessage = message + ": " + e.Message;
            return StatusCode(internalSeverErrorCode, errorMessage);
        }

        /// <summary>
        /// Handler for common error handling logic.
        /// </summary>
        /// <param name="e">Exception that was caught</param>
        /// <param name="message">Custom message to be prepended to error message.</param>
        /// <returns>
        /// <see cref="BadRequestObjectResult"/> or <see cref="StatusCodeResult"/>.
        /// </returns>
        private ActionResult HandleErrorWithMessage(Exception e, string message)
        {
            switch (e)
            {
                case ItemNotFoundException:
                case ItemNotOwnedException:
                    string errorMessage = message + ": " + e.Message;
                    return BadRequest(errorMessage);
                default:
                    return InternalServerError(e, message);
            }
        }
    }
}
