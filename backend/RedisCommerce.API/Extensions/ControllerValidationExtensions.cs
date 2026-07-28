using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace RedisCommerce.API.Extensions;

public static class ControllerValidationExtensions
{
    public static bool ApplyValidationErrors(this ControllerBase controller, ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return true;
        }

        foreach (var error in validationResult.Errors)
        {
            controller.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return false;
    }
}
