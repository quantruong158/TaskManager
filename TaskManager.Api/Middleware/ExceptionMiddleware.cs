using System.Net;
using System.Text.Json;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Models;

namespace TaskManager.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                
                context.Response.ContentType = "application/json";
                
                var response = new ErrorResponse
                {
                    Message = ex.Message,
                    Type = ex.GetType().Name
                };

                switch (ex)
                {
                    case NotFoundException:
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                        
                    case BadRequestException:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                        
                    case UnauthorizedException:
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;
                        
                    case ForbiddenException:
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        break;
                        
                    default:
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        if (_env.IsDevelopment())
                        {
                            response.Details = ex.StackTrace;
                        }
                        break;
                }

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                await context.Response.WriteAsync(json);
            }
        }
    }
}