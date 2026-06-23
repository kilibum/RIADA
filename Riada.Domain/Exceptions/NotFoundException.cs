namespace Riada.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object key)
        : base("NOT_FOUND", $"{entity} with key '{key}' was not found.") { }
}
