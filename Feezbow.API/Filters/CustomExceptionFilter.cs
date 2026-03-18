using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WannabeTrello.Application.Common.Exceptions;
using WannabeTrello.Domain.Exceptions;

namespace WannabeTrello.Filters;

public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<CustomExceptionFilter> _logger;

        public CustomExceptionFilter(ILogger<CustomExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var code = HttpStatusCode.InternalServerError;
            var message = "Došlo je do neočekivane greške.";
            var errors = new Dictionary<string, string[]>();
            var traceId = context.HttpContext.TraceIdentifier;

            switch (context.Exception)
            {
                case ValidationException validationException:
                    code = HttpStatusCode.BadRequest;
                    message = "Došlo je do jedne ili više grešaka validacije.";
                    errors = validationException.Errors.ToDictionary(e => e.Key, e => e.Value);
                    break;
                case NotFoundException notFoundException:
                    code = HttpStatusCode.NotFound;
                    message = notFoundException.Message;
                    break;
                case AccessDeniedException accessDeniedException:
                    code = HttpStatusCode.Forbidden;
                    message = accessDeniedException.Message;
                    break;
                case UnauthorizedAccessException unauthorizedAccessException:
                    code = HttpStatusCode.Unauthorized;
                    message = unauthorizedAccessException.Message;
                    break;
                case DomainException domainException:
                    code = HttpStatusCode.BadRequest;
                    message = domainException.Message;
                    break;
                default:
                    _logger.LogError(context.Exception, "Neobrađeni izuzetak se dogodio. TraceId: {TraceId}", traceId);
                    break;
            }

            context.Result = new ObjectResult(new
            {
                status = (int)code,
                message = message,
                errors = errors.Count != 0 ? errors : null,
                traceId
            })
            {
                StatusCode = (int)code
            };

            context.ExceptionHandled = true;
        }
    }