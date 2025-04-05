namespace GFindDI;

/// <summary>
/// Provides functionality for validating whether a specified type is an interface.
/// </summary>
internal static class InterfaceValidator
{
    /// <summary>
    /// Validates that the specified generic type parameter is an interface.
    /// </summary>
    /// <typeparam name="TClass">The type to validate. It must be an interface; otherwise, an exception is thrown.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if the specified type is not an interface.</exception>
    internal static void ValidateInterface<TClass>()
    {
        if (!typeof(TClass).IsInterface)
        {
            throw new InvalidOperationException($"{typeof(TClass).Name} must be an interface.");
        }
    }
}