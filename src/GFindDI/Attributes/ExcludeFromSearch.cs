namespace GFindDI.Attributes;

/// <summary>
/// Denotes that the decorated class should be excluded from certain search or discovery mechanisms,
/// such as those used for dependency injection or type scanning.
/// </summary>
/// <remarks>
/// This attribute is typically used to prevent a class from being considered during automated processes
/// like runtime type registration or assembly scanning. It can be applied only to classes.
/// </remarks>
/// <example>
/// Applying this attribute on a class ensures that the class is ignored when performing
/// reflection-based searches.
/// </example>
/// <seealso cref="System.Attribute"/>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ExcludeFromSearchAttribute : Attribute;