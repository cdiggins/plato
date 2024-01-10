namespace Plato;

/// <summary>
/// Prevents an instance of a type from being shared. You can only
/// have one reference to this tpe. This allows safe usage without locking
/// and guaranteed immutabiliy. This is enforced by a Plato code analyzer.
/// </summary>
public class UniqueAttribute : Attribute
{
}