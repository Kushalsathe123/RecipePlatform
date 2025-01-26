using RecipePlatform.RecipeManagementService.Contracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Contracts.Responses
{
    public class ErrorMessage
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class BaseResponse
    {
        public string Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Error> Errors { get; set; }

        public BaseResponse()
        {
            Id = Guid.NewGuid().ToString();
            Success = true;
            Errors = new List<Error>();
        }
    }

    public class GetRecipesResponse : BaseResponse
    {
        public IEnumerable<RecipeDto>? Recipes { get; set; }
    }

    public class EditRecipeResponse : BaseResponse
    {
        public RecipeDto? Recipe { get; set; }
    }

    public class DeleteRecipeResponse : BaseResponse
    {
        public bool Deleted { get; set; }
    }
}
