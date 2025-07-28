
using AspNetWebApiCore.Controllers;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace AspNetWebApiCore.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;     
        private  ClaimsPrincipal _user;
        private string ClientId = "0";
        private string Client_Guid;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                try 
                {                  
                    if (context != null && context.User != null)
                    {
                        _user = context.User;
                        ClientId = _user.FindFirst(ClaimTypes.NameIdentifier).Value;
                        Client_Guid = _user.FindFirst(ClaimTypes.Name).Value;                    
                    }
                } 
                catch(Exception ex1)
                {
                    Log.Error(ex1, "Global exception reading context.user");
                }
                Log.Error(ex, "Global exception caught for clientID: " + ClientId + " ClientKey: " + Client_Guid);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var response = Common.ApiResponseHelper<string>.FailResponse( Convert.ToInt32(ClientId), "An unexpected error occurred");
                var json_response = JsonSerializer.Serialize(response);  
                await context.Response.WriteAsync(json_response);
            }
        }
    }
}
