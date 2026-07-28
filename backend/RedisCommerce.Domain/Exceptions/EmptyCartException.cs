namespace RedisCommerce.Domain.Exceptions;

public class EmptyCartException : Exception
{
    public EmptyCartException(int userId)
        : base($"Cart for user '{userId}' is empty. Add items before checking out.")
    {
    }
}
