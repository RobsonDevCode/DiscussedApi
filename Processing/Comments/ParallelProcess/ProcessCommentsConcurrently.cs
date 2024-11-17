using DiscussedApi.Configuration;
using DiscussedApi.Models.Comments;
using DiscussedApi.Reopisitory.Comments;
using NLog;
using System.Collections.Concurrent;

namespace DiscussedApi.Processing.Comments.ParallelProcess
{
    public class ProcessCommentsConcurrently : IProcessCommentsConcurrently
    {
        private readonly ICommentDataAccess _commentDataAccess;
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();   

        public ProcessCommentsConcurrently(ICommentDataAccess commentDataAccess)
        {
            _commentDataAccess = commentDataAccess;
        }

        public async Task<List<Comment>> GetCommentsConcurrently(List<Guid?> userIds, string topic ,CancellationToken ctx)
        {
            if (!userIds.Any())
                throw new ArgumentException("User ids cannot be null when getting comments");

            ConcurrentBag<List<Comment>> comments = new ConcurrentBag<List<Comment>>();

            try
            {
                await Parallel.ForAsync(0, userIds.Count(), new ParallelOptions()
                {
                    CancellationToken = ctx,
                    MaxDegreeOfParallelism = Settings.ParallelWorkers
                },

                async (i, ctx) =>
                {
                    try
                    {
                        var getCommentsByUser = await _commentDataAccess.GetCommentsPostedByFollowing(userIds[i], topic ,ctx);
                        comments.Add(getCommentsByUser);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, ex.Message);
                        throw;
                    }
                });

                if (comments.Count() == 0)
                    throw new Exception("No comments returned");

                return comments.SelectMany(x => x)
                               .OrderByDescending(x => x.DtCreated)
                               .ThenByDescending(x => x.Likes)
                               .ToList();
            }

            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
       
    }
}
