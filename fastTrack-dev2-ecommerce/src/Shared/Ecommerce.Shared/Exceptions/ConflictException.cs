namespace Ecommerce.Shared.Exceptions;

// e.g. duplicate email, invalid order status change
public class ConflictException(string message) : Exception(message);
