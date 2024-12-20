using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace DiscussedApi.Models.ApiResponses.Error
{
    public class ErrorResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        public ErrorResponse(string message)
        {
            Message = message;
        }
    }


}
