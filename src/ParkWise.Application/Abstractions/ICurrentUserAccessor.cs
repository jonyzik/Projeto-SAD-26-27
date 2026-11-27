namespace ParkWise.Application.Abstractions;

/// <summary>
/// Identifica o utilizador autenticado. Enquanto o módulo de autenticação não estiver integrado,
/// a implementação predefinida devolve <c>null</c> e as simulações são guardadas sem utilizador.
/// </summary>
public interface ICurrentUserAccessor
{
    Guid? CurrentUserId { get; }
}
