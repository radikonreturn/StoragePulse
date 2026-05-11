namespace WIMS.Core.Exceptions;

public class DomainException : Exception
{
    public DomainException() { }
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException() : base("The requested entity was not found.") { }
    public EntityNotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" with key ({key}) was not found.") { }
}
