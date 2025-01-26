using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Contracts.Responses
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }

    public class BaseResponse<T>
    {
        public string Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<Error> Errors { get; set; }
        public int StatusCode { get; set; } // New StatusCode property
        public T Data { get; set; } // Generic data to carry any response data, like recipe info.

        public BaseResponse()
        {
            Id = Guid.NewGuid().ToString();
            Success = true;
            Errors = new List<Error>();
            StatusCode = StatusCodes.Status200OK; // Default to 200 OK
        }

        public BaseResponse(string message, int statusCode) : this()
        {
            Success = true;
            Message = message;
            StatusCode = statusCode;
        }

        public BaseResponse(string message, bool success, int statusCode) : this()
        {
            Success = success;
            Message = message;
            StatusCode = statusCode;
        }

        public override string ToString()
        {
            string error = Errors != null && Errors.Count > 0 ? Errors[0].Message : string.Empty;
            return $"Id:{Id}, Message:{Message}, StatusCode:{StatusCode}, Errors:{error}";
        }
    }
}
