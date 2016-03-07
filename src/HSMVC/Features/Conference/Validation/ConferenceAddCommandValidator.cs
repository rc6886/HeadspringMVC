using FluentValidation;
using FluentValidation.Results;
using HSMVC.Constants;
using HSMVC.Features.Conference.Commands;

namespace HSMVC.Features.Conference.Validation
{
    public class ConferenceAddCommandValidator : AbstractValidator<ConferenceAddCommand>
    {
        public ConferenceAddCommandValidator(ConferenceValidatorHelper validatorHelper)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(ConferenceValidatorHelper.RequiredMessage("Name"));
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage(ConferenceValidatorHelper.RequiredMessage("StartDate"))
                .Must(ConferenceValidatorHelper.IsAValidDate).WithMessage(ConferenceValidatorHelper.NotAValidDateMessage("StartDate"));
            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .WithMessage(ErrorMessages.StartDateGreaterThanEndDate);
            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage(ConferenceValidatorHelper.RequiredMessage("EndDate"))
                .Must(ConferenceValidatorHelper.IsAValidDate).WithMessage(ConferenceValidatorHelper.NotAValidDateMessage("EndDate"));
            RuleFor(x => x.Cost).NotNull().WithMessage(ConferenceValidatorHelper.RequiredMessage("Cost"));
            RuleFor(x => x.AttendeeCount).NotNull().WithMessage(ConferenceValidatorHelper.RequiredMessage("Attendee Count"));

            Custom(conference => validatorHelper.DoesConferenceNameAlreadyExist(null, conference.Name)
                ? new ValidationFailure("Name", $"The conference {conference.Name} name already exists.")
                : null);
        }
    }
}