using BonusesApp.Core.Services.Account.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace BonusesApp.Server.Filters;

/// <summary>
/// Глобальный фильтр исключений для обработки всех исключений в приложении
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        // Логируем исключение
        _logger.LogError(context.Exception, 
            "Произошло исключение: {Message}", 
            context.Exception.Message);

        // Определяем HTTP статус-код на основе типа исключения
        var statusCode = GetStatusCode(context.Exception);

        // Создаем объект ответа с сообщением об ошибке
        var errorResponse = new
        {
            message = context.Exception.Message,
            type = context.Exception.GetType().Name
        };

        // Устанавливаем результат
        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = (int)statusCode
        };

        // Помечаем исключение как обработанное
        context.ExceptionHandled = true;
    }

    /// <summary>
    /// Определяет HTTP статус-код на основе типа исключения
    /// </summary>
    private static HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            // Кастомные исключения проекта
            UserNotFoundException => HttpStatusCode.NotFound,
            UserAccountException => HttpStatusCode.BadRequest,
            UserRoleException => HttpStatusCode.BadRequest,
            
            // Стандартные исключения
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            NotImplementedException => HttpStatusCode.NotImplemented,
            _ => HttpStatusCode.InternalServerError
        };
    }
}

