#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants.CreateTenant;

internal record CreateTenantCommand(string TenantName, string TenantCode, Status TenantStatus);

internal sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    internal CreateTenantCommandValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty()
            .WithMessage("Tenant name is required")
            .MaximumLength(50)
            .WithMessage("Tenant name cannot be more than 50 characters");

        RuleFor(x => x.TenantCode)
            .MaximumLength(10)
            .WithMessage("Tenant code cannot be more than 10 characters");

        RuleFor(x => x.TenantStatus).IsInEnum().WithMessage("Tenant status is invalid");
    }
}

internal class CreateTenantHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<int> Handle(
        CreateTenantCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(command.TenantName))
        {
            throw new ArgumentException("Tenant name is required");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();

        if (
            await dbContext
                .Set<TenantEntity>()
                .AnyAsync(t => t.TenantName == command.TenantName, cancellationToken)
        )
        {
            throw new InvalidOperationException(
                $"Tenant with name \"{command.TenantName}\" already exists"
            );
        }

        var tenant = new TenantEntity
        {
            TenantName = command.TenantName,
            TenantCode = command.TenantCode,
            TenantStatusId = (int)Status.Active
        };

        await dbContext.Set<TenantEntity>().AddAsync(tenant, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("tenants");
        }

        return tenant.TenantId;
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
