using System.Text.Json.Serialization;

namespace DiscussedApi.Models.ApiResponses.Email
{
    public class EmailConfirmationApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty; 
    }
}
