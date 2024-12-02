using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DiscussedApi.Models.Comments.Replies
{
    public class RepliesWithComment
    {
        public Comment Comment { get; set; }
        public List<Reply> Replies { get; set; }
    }
}
