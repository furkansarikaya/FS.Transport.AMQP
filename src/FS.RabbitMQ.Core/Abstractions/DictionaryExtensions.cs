namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Provides extension methods for working with dictionaries in a read-only context.
/// </summary>
internal static class DictionaryExtensions
{
    /// <summary>
    /// Creates a read-only wrapper around the specified dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to wrap. Cannot be null.</param>
    /// <returns>A read-only dictionary that wraps the original dictionary.</returns>
    /// <remarks>
    /// This extension method provides a convenient way to create read-only views
    /// of dictionaries while maintaining reference semantics for performance.
    /// The underlying dictionary can still be modified through its original reference,
    /// but the returned wrapper prevents modification through the read-only interface.
    /// </remarks>
    public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        return dictionary;
    }
}