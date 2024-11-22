using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using DiscussedApi.Models.Topic;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Processing.Comments.ParallelProcess;
using DiscussedApi.Reopisitory.Comments;
using DiscussedApi.Reopisitory.Profiles;
using DiscussedApi.Reopisitory.Topics;
using Discusseddto;
using Discusseddto.Comment;
using Discusseddto.CommentDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MimeKit.Cryptography;
using NLog;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;

namespace DiscussedApi.Processing.Comments
{
    public class CommentProcessing : ICommentProcessing
    {
        private readonly ICommentDataAccess _commentDataAccess;
        private readonly IProfileDataAccess _profileDataAccess;
        private readonly IProcessCommentsConcurrently _processCommentsConcurrently;
        private readonly ITopicDataAccess _topicDataAccess;
        private readonly UserManager<User> _userManager;
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public CommentProcessing(ICommentDataAccess commentDataAccess, IProfileDataAccess profileDataAccess, 
                                UserManager<User> userManager, IProcessCommentsConcurrently processCommentsConcurrently,
                                ITopicDataAccess topicDataAccess, IMemoryCache memoryCache)
        {
             _commentDataAccess = commentDataAccess;
            _profileDataAccess = profileDataAccess;
            _userManager = userManager;
            _processCommentsConcurrently = processCommentsConcurrently;
            _topicDataAccess = topicDataAccess;
        }

        public async Task<List<Comment>> GetFollowingCommentsAsync(Guid userId,string topic, long? nextPageToken, CancellationToken ctx)
        {
            try
            {
                List<Guid?> userFollowing = await _profileDataAccess.GetUserFollowing(userId);

                //if user following is null check if they are a new user and display content based on prompts selected
                if (userFollowing.Count == 0 || userFollowing == null)
                {
                    _logger.Info($"User {userId} follows no accounts");
                    return new List<Comment>();
                }

                //get comments from user following who's posted
                var commentsFromFollowed = await _processCommentsConcurrently.GetCommentsConcurrently(userFollowing, topic, nextPageToken, ctx);

                if (commentsFromFollowed == null) 
                    throw new ArgumentNullException($"Comments From Followers cannot be null when user has followers");

                if (commentsFromFollowed.Count == 0)
                {
                    //no comments from following on todays topic
                    return new List<Comment>();
                }

                return commentsFromFollowed;

            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }

        }


        /// <summary>
        /// GetTopCommentsAsync: tokenised paging for load more comments functionality, if token is null we'll get the first page of comments
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="nextPageToken"></param>
        /// <param name="ctx"></param>
        /// <returns cref="Comment">List of immutable comments</returns>
        public async Task<ImmutableList<Comment>> GetTopCommentsAsync(string topic, long? nextPageToken, CancellationToken ctx)
        {
            try
            {
                return await _commentDataAccess.GetTopCommentsForTodaysTopic(topic,nextPageToken, ctx);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task PostCommentAsync(NewCommentDto newComment, CancellationToken ctx)
        {

            if (newComment == null)
            {
                _logger.Error("Error Posting Comment: Comment is null");
                throw new ArgumentNullException("Comment was null");
            }

            try
            {
                Comment comment = new()
                {
                    Id = newComment.Id,
                    TopicId = newComment.TopicId,
                    UserId = newComment.UserId,
                    UserName = newComment.UserName,
                    Content = newComment.Content,
                    ReplyCount = 0,
                    Likes = 0,
                    DtCreated = DateTime.UtcNow,
                    DtUpdated = DateTime.UtcNow,
                };

                await _commentDataAccess.PostCommentAsync(comment, ctx);
            }
            catch (Exception ex) 
            {
                _logger.Error(ex, ex.Message);
                throw;
            }

        }
        public async Task<Comment> EditCommentContentAsync(UpdateCommentDto updateComment, CancellationToken ctx)
        {
            try
            {
                if (!await _commentDataAccess.IsCommentValid(updateComment.CommentId, ctx))
                    throw new KeyNotFoundException("Comment not found or has been deleted");

                 //check if user exists
                 User user = await _userManager.FindByIdAsync(updateComment.UserId.ToString()) ??
                          throw new KeyNotFoundException("User who posted comment could not be found or has deleted there account");


                return await _commentDataAccess.UpdateCommentContentAsync(updateComment, ctx);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
        public async Task<Comment> LikeOrDislikeCommentAsync(LikeCommentDto commentToEditDto)
        {
            try
            {
                if (!await _commentDataAccess.IsCommentValid(commentToEditDto.CommentId))
                    throw new KeyNotFoundException("Comment not found or has been deleted");

                return await _commentDataAccess.UpdateCommentLikesAsync(commentToEditDto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
        public async Task DeleteCommentAsync(Guid commentId, CancellationToken ctx)
        {
            try
            {
                if (!await _commentDataAccess.IsCommentValid(commentId, ctx))
                    throw new KeyNotFoundException("Comment not found or has been deleted already");

                await _commentDataAccess.DeleteCommentAsyncEndpoint(commentId, ctx);

                _logger.Info($"Comment {commentId} has been deleted");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
