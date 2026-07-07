using FluentValidation.Results;

namespace STLMS.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("One or more validation failures occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}

public class NotFoundException(string entityName, object key)
    : Exception($"Entity \"{entityName}\" ({key}) was not found.");

public class ConflictException(string message) : Exception(message);

public class ForbiddenException(string message = "You do not have permission to perform this action.") : Exception(message);

public class UnauthorizedAppException(string message = "Invalid credentials.") : Exception(message);
