namespace Esport.Domain.Exceptions;

/// <summary>
/// Ressource introuvable (mapp�e en HTTP 404 par le middleware).
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Conflit (doublon, suppression bloqu�e...) mapp� en HTTP 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>
/// R�gle m�tier viol�e mapp�e en HTTP 400.
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
