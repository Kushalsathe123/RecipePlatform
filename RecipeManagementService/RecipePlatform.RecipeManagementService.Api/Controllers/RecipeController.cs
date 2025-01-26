using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipePlatform.RecipeManagementService.Contracts.DTO;
using RecipePlatform.RecipeManagementService.Contracts.Interfaces;
using RecipePlatform.RecipeManagementService.Contracts.Responses;
using System.Net;
using System.Security.Claims;

namespace RecipePlatform.RecipeManagementService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public RecipeController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        [Authorize]
        [HttpPost("create-recipe")]
        public async Task<IActionResult> AddRecipe([FromForm] CreateRecipeDto createRecipeDto)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                var recipeId = await _recipeService.CreateRecipeAsync(createRecipeDto,userId);

                // Return success response with status code 201 Created
                return StatusCode(StatusCodes.Status201Created, new BaseResponse<string>
                {
                    Success = true,
                    Message = $"Recipe added successfully with ID: {recipeId}",
                    Data = recipeId,
                    StatusCode = StatusCodes.Status201Created
                });
            }
            catch (Exception ex)
            {
                // Return error response with status code 500 Internal Server Error
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while adding the recipe.",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = new List<Error> { new Error { Code = "AddRecipeError", Message = ex.Message } }
                });
            }
        }

        //[Authorize(Roles = "Admin")]

        [HttpPost("approve-recipe")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRecipe([FromBody] ApproveRecipeDto approveRecipeDto)
        {
            var (isSuccess, errorMessage) = await _recipeService.ApproveRecipeAsync(approveRecipeDto.RecipeId, approveRecipeDto.UserId);

            if (isSuccess)
            {
                return Ok(new BaseResponse<string>
                {
                    Success = true,
                    Message = $"Recipe with ID {approveRecipeDto.RecipeId} approved successfully.",
                    StatusCode = StatusCodes.Status200OK
                });
            }

            return NotFound(new BaseResponse<string>
            {
                Success = false,
                Message = errorMessage,
                StatusCode = StatusCodes.Status404NotFound,
                Errors = new List<Error> { new Error { Code = "ApprovalError", Message = errorMessage } }
            });
        }

        [HttpPost("reject-recipe")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRecipe([FromBody] RejectRecipeDto recipeDto)
        {
            var (isSuccess, errorMessage) = await _recipeService.RejectRecipeAsync(recipeDto.RecipeId, recipeDto.UserId, recipeDto.Feedback);

            if (isSuccess)
            {
                return Ok(new BaseResponse<string>
                {
                    Success = true,
                    Message = $"Recipe with ID {recipeDto.RecipeId} has been rejected.",
                    StatusCode = StatusCodes.Status200OK
                });
            }

            // Return a structured error response
            return NotFound(new BaseResponse<string>
            {
                Success = false,
                Message = errorMessage,
                StatusCode = StatusCodes.Status404NotFound,
                Errors = new List<Error> { new Error { Code = "RejectRecipeError", Message = errorMessage } }
            });
        }

        [Authorize]
        [HttpPost("like-recipe")]
        public async Task<IActionResult> LikeRecipe([FromBody] LikeDto like)
        {
            try
            {
                var UserId = GetAuthenticatedUserId();
                await _recipeService.LikeRecipeAsync(like.RecipeId, UserId);
                return Ok(new BaseResponse<string>
                {
                    Success = true,
                    Message = "Recipe liked successfully.",
                    StatusCode = StatusCodes.Status200OK
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new BaseResponse<string>
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while liking the recipe.",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = new List<Error> { new Error { Code = "LikeRecipeError", Message = ex.Message } }
                });
            }
        }

        [Authorize]
        [HttpPost("unlike-recipe")]
        public async Task<IActionResult> UnlikeRecipe([FromBody] LikeDto like)
        {
            try
            {
                var UserId = GetAuthenticatedUserId();
                await _recipeService.UnlikeRecipeAsync(like.RecipeId,UserId);
                return Ok(new BaseResponse<string>
                {
                    Success = true,
                    Message = "Recipe unliked successfully.",
                    StatusCode = StatusCodes.Status200OK
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new BaseResponse<string>
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while unliking the recipe.",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = new List<Error> { new Error { Code = "UnlikeRecipeError", Message = ex.Message } }
                });
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GetRecipesResponse>> GetPendingRecipes()
        {
            var response = new GetRecipesResponse();
            try
            {
                var recipes = await _recipeService.GetPendingRecipesAsync();
                response.Recipes = recipes;
                response.Success = true;
                response.Message = "Pending recipes retrieved successfully";
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(response, "Failed to retrieve pending recipes", ex);
            }
        }

        [HttpGet("approved")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GetRecipesResponse>> GetApprovedRecipes()
        {
            var response = new GetRecipesResponse();
            try
            {
                var recipes = await _recipeService.GetApprovedRecipesAsync();
                response.Recipes = recipes;
                response.Success = true;
                response.Message = "Approved recipes retrieved successfully";
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(response, "Failed to retrieve approved recipes", ex);
            }
        }

        [HttpGet("rejected")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GetRecipesResponse>> GetRejectedRecipes()
        {
            var response = new GetRecipesResponse();
            try
            {
                var recipes = await _recipeService.GetRejectedRecipesAsync();
                response.Recipes = recipes;
                response.Success = true;
                response.Message = "Rejected recipes retrieved successfully";
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(response, "Failed to retrieve rejected recipes", ex);
            }
        }

        [HttpGet("user/pending")]
        [Authorize]
        public async Task<ActionResult<GetRecipesResponse>> GetUserPendingRecipes()
        {
            var response = new GetRecipesResponse();
            try
            {
                int userId = GetAuthenticatedUserId();
                var recipes = await _recipeService.GetPendingRecipesByUserIdAsync(userId);
                response.Recipes = recipes;
                response.Success = true;
                response.Message = "User's pending recipes retrieved successfully";
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return HandleException(response, "Failed to retrieve user's pending recipes", ex);
            }
        }

        [HttpGet("user/approved")]
        [Authorize]
        public async Task<ActionResult<GetRecipesResponse>> GetUserApprovedRecipes()
        {
            var response = new GetRecipesResponse();
            try
            {
                int userId = GetAuthenticatedUserId();
                var recipes = await _recipeService.GetApprovedRecipesByUserIdAsync(userId);
                response.Recipes = recipes;
                response.Success = true;
                response.Message = "User's approved recipes retrieved successfully";
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return HandleException(response, "Failed to retrieve user's approved recipes", ex);
            }
        }

        [HttpPut("edit/{recipeId}")]
        [Authorize]
        public async Task<ActionResult<EditRecipeResponse>> EditRecipe(int recipeId, [FromForm] EditRecipeDto editRecipeDto)
        {
            var response = new EditRecipeResponse();
            try
            {
                int userId = GetAuthenticatedUserId();
                var updatedRecipe = await _recipeService.EditRecipeAsync(recipeId, userId, editRecipeDto);
                if (updatedRecipe == null)
                {
                    response.Success = false;
                    response.Message = "Recipe not found or user not authorized to edit";
                    return NotFound(response);
                }

                response.Recipe = updatedRecipe;
                response.Success = true;
                response.Message = "Recipe updated successfully";
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return HandleException(response, "Failed to update recipe", ex);
            }
        }

        [HttpDelete("delete/{recipeId}")]
        [Authorize]
        public async Task<ActionResult<DeleteRecipeResponse>> DeleteRecipe(int recipeId)
        {
            var response = new DeleteRecipeResponse();
            try
            {
                int userId = GetAuthenticatedUserId();
                var result = await _recipeService.DeleteRecipeAsync(recipeId, userId);
                if (!result)
                {
                    response.Success = false;
                    response.Message = "Recipe not found or user not authorized to delete";
                    response.Deleted = false;
                    return NotFound(response);
                }

                response.Success = true;
                response.Message = "Recipe deleted successfully";
                response.Deleted = true;
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return HandleException(response, "Failed to delete recipe", ex);
            }
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new ArgumentException("Invalid or missing user ID in the token");
            }
            return userId;
        }

        private ObjectResult HandleException(BaseResponse response, string message, Exception ex)
        {
            response.Success = false;
            response.Message = message;
            response.Errors.Add(new Error { Code = ((int)HttpStatusCode.InternalServerError).ToString(), Message = ex.Message });
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }

    }
}
