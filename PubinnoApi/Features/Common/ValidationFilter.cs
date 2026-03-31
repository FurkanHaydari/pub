using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace PubinnoApi.Features.Common;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService(typeof(IValidator<T>)) as IValidator<T>;
        if (validator is not null)
        {
            var entity = context.Arguments.OfType<T>().FirstOrDefault();
            if (entity is not null)
            {
                var validationResult = await validator.ValidateAsync(entity);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
                }
            }
        }
        return await next(context);
    }
}
