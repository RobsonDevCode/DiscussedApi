﻿using DiscussedApi.Authenctication;
using MailKit;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using NLog;
using System.Security.Cryptography;

namespace DiscussedApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public ExceptionHandlingMiddleware(RequestDelegate requestDelegate)
        {
            _next = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Error(ex, ex.Message);
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = ex.Message
                };
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
            catch(KeyNotFoundException ex)
            {
                _logger.Error(ex, ex.Message);
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = ex.Message,
                };

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
            catch(ServiceNotAuthenticatedException ex)
            {
                _logger.Error(ex, ex.Message);
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = ex.Message
                };

                context.Response.StatusCode= StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
            catch (CryptographicException ex)
            {
                await Status500(context, ex);
            }
            catch(BuildTokenException ex)
            {
                await Status500WithNoDetails(context, ex);
            }
            catch (Exception ex)
            {
                //handle database errors more securely as it could contain sensitive info
                if(ex is MySqlException)
                {
                    await Status500WithNoDetails(context, ex);
                }

                await Status500(context, ex);
            }
        }
        private async Task Status500(HttpContext context, Exception ex)
        {
            _logger.Error(ex, ex.Message);

            var problemDetails = new ProblemDetails()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Detail = ex.Message 
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        private async Task Status500WithNoDetails(HttpContext context, Exception ex)
        {
            _logger.Error(ex, ex.Message);

            var problemDetails = new ProblemDetails()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }



    }
}
