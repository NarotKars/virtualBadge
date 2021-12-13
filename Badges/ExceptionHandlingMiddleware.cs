using DataAccess.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Badges
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch(SqlException e)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.Message);
            }
            catch(AggregateException e)
            {
                foreach(var exception in e.InnerExceptions)
                {
                    if(exception.GetType().Name == nameof(ArgumentException))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await context.Response.WriteAsync(e.Message);
                    }
                    else if(exception.GetType().Name == nameof(SqlException))
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync(e.Message);
                    }
                    else if(exception.GetType().Name == nameof(KeyCloakException))
                    {
                        context.Response.StatusCode = ((KeyCloakException)exception).statusCode;
                        await context.Response.WriteAsync(e.Message);
                    }
                }
            }
            catch(Exception e)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.Message);
            }
        }
    }
}
