using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MentorApp.Data
{
    [SuppressMessage("Minor Code Smell", "S3900:Update this implementation of 'ISerializable' to conform to the recommended serialization pattern", Justification = "'Exception' implements 'ISerializable' so 'ISerializable' can be removed from the inheritance list")]
    [SuppressMessage("Minor Code Smell", "S4055:Literals should not be passed as localized parameters", Justification = "No value pulling as localized parameter.")]
    [SuppressMessage("Minor Code Smell", "S2221:\"Exception\" should not be caught when not required by called methods", Justification = "Added catch for exception to ensure stacktrace is not return to the client.")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Acceptable to catch all errors for general code")]
    [ExcludeFromCodeCoverage]
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbException)
            {
                var statusCode = StatusCodes.Status500InternalServerError;

                if (dbException.InnerException != null)
                {
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                }

                await ProcessExceptionAsync(httpContext, statusCode, dbException, dbException.Message);
            }
            catch (SqlException se)
            {
                var statusCode = StatusCodes.Status500InternalServerError;

                if (se.Message.Contains("Invalid column", StringComparison.OrdinalIgnoreCase))
                {
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                }

                await ProcessExceptionAsync(httpContext, statusCode, se, se.Message);
            }
            catch (HttpStatusCodeException hsce)
            {
                string message = string.Empty;

                if (!string.IsNullOrEmpty(hsce.Message))
                {
                    message = $"trace ID: '{hsce.Message}'";
                }

                await ProcessExceptionAsync(httpContext, hsce.StatusCode, hsce, message);
            }
            catch (Exception ex)
            {
                await ProcessExceptionAsync(httpContext, StatusCodes.Status500InternalServerError, ex, "Application Internal Error");
            }
        }

        public static async Task ProcessExceptionAsync(HttpContext httpContext, int statusCode, Exception exception, string message)
        {
            if (httpContext.Response.HasStarted)
            {
                throw exception;
            }

            ProcessException(httpContext, statusCode, exception, message);

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsync(message);
        }

        private static void ProcessException(HttpContext httpContext, int statusCode, Exception exception, string message = "Stack")
        {
            var OperationProperties = new[]
            {
                $"Request Status ({statusCode})",
                $"Request Method ({httpContext.Request.Method})",
                $"Request Path ({httpContext.Request?.Path.Value})",
                $"Exception Message: {exception?.Message}",
                $"Exception Stack: {exception?.StackTrace}"
            };
            foreach (var operation in OperationProperties)
            {
                System.Diagnostics.Trace.TraceError($"Exception [{message}] => {operation}");
            };

            if (exception?.InnerException != null)
            {
                ProcessException(httpContext, statusCode, exception.InnerException, "Inner Exception");
            }
        }
    }
}
