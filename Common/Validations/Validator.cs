﻿using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DiscussedApi.Common.Validations
{
    public static class Validator<T> where T : class
    {
        public async static Task<(ValidationResult? SuccesfulValidation, ModelStateDictionary? FaliedValidation)> ValidationAsync(T request, IValidator<T> validator)
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var modelStateDictionary = new ModelStateDictionary();

                foreach (ValidationFailure failure in validationResult.Errors)
                {
                    modelStateDictionary.AddModelError(failure.PropertyName, failure.ErrorMessage);
                }

                return (null, modelStateDictionary);
            }

            return (validationResult, null);
        }
    }
}