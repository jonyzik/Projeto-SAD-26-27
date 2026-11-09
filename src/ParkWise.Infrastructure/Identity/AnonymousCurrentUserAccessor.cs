using ParkWise.Application.Abstractions;

namespace ParkWise.Infrastructure.Identity;

/// <summary>Substituir pela implementação do módulo de autenticação (dev/auth) quando estiver disponível.</summary>
public sealed class AnonymousCurrentUserAccessor : ICurrentUserAccessor
{
    public Guid? CurrentUserId => null;
}
