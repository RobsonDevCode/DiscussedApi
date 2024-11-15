using System.ComponentModel.DataAnnotations;

namespace DiscussedApi.Models.Topic
{
    public class Topic
    {
        [Key]
        public Guid TopicId { get; set; }
        public string Name { get; init; }
        public DateOnly DtCreated { get; init; }
        public string Category { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public int Likes {  get; init; }

    }
}
