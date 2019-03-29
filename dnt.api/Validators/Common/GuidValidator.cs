namespace dnt.api.Validators.Common
{
    using System;
    using FluentValidation;

    public class GuidValidator : AbstractValidator<Guid>
    {
        public GuidValidator()
        {
            RuleFor(g => g).NotEmpty();
        }
    }
}