namespace Riada.Domain.Exceptions;

public class AccessDeniedException : DomainException
{
    public string? DenialReason { get; }

    public AccessDeniedException(string reason)
        : base("ACCESS_DENIED", reason)
    {
        DenialReason = reason;
    }
}
