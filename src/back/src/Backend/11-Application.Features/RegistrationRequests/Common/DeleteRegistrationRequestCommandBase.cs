using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tools.Logging;

namespace Application.Features.RegistrationRequests.Common;

public class DeleteRegistrationRequestCommandBase : DeleteRegistrationRequestModel, IRequest<Result> { }

public abstract class DeleteRegistrationRequestCommandValidatorBase<T_Model> : AbstractValidator<T_Model>
    where T_Model : DeleteRegistrationRequestModel
{
    protected readonly ReadOnlyDbContext _context;

    protected DeleteRegistrationRequestCommandValidatorBase(ReadOnlyDbContext context)
    {
        _context = context;

        RuleFor(v => v.Id)
            .NotEmpty()
            .WithMessage(ValidationErrorCode.Required.ToString())
            .DependentRules(() =>
            {
                RuleFor(v => v.Id)
                    .MustAsync(
                        (id, token) => _context.RegistrationRequests.AnyAsync(rr => rr.Id == id, token)
                    )
                    .WithMessage(ValidationErrorCode.RegistrationRequestMustExist.ToString())
                    .DependentRules(() =>
                    {
                        AddCustomRules();
                    });
            });
    }

    protected abstract void AddCustomRules();
}

public abstract class DeleteRegistrationRequestCommandHandlerBase<T_Command>(WritableDbContext context)
    : IRequestHandler<T_Command, Result>
    where T_Command : DeleteRegistrationRequestCommandBase
{
    public async Task<Result> Handle(T_Command command, CancellationToken cancellationToken)
    {
        using var activity = ActivitySourceLog.CQRS.Start().AddParameter(command, r => r.Id);

        var registrationRequest = await context
            .RegistrationRequests.AsNoTracking()
            .Include(r => r.Citizen)
            .Include(r => r.RegistrationRequestDocuments)
            .FirstAsync(rr => rr.Id == command.Id, cancellationToken);

        if (registrationRequest.Citizen != null)
        {
            context.Citizens.Remove(registrationRequest.Citizen);
        }

        if (registrationRequest.RegistrationRequestDocuments != null)
        {
            context.RegistrationRequestDocuments.RemoveRange(
                registrationRequest.RegistrationRequestDocuments
            );
        }

        context.RegistrationRequests.Remove(registrationRequest);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Default();
    }
}
