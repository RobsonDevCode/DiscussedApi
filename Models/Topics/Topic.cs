using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DiscussedApi.Models.Topic
{
    public class Topic
    {
        [Key]
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("dt_created")]
        public DateOnly DtCreated { get; init; }
        [JsonPropertyName("category")]
        public string Category { get; init; } = string.Empty;
        [JsonPropertyName("is_active")]
        public bool IsActive { get; init; }
        [JsonPropertyName("likes")]
        public long Likes {  get; init; }

    }
}
