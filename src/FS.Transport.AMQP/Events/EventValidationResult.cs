namespace FS.Transport.AMQP.Events;

/// <summary>
/// Result of event validation
/// </summary>
public class EventValidationResult
{
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();

    /// <summary>
    /// Validation errors
    /// </summary>
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    /// <summary>
    /// Validation warnings
    /// </summary>
    public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();

    /// <summary>
    /// Whether the validation passed (no errors)
    /// </summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>
    /// Whether there are any warnings
    /// </summary>
    public bool HasWarnings => _warnings.Count > 0;

    /// <summary>
    /// Adds a validation error
    /// </summary>
    /// <param name="error">Error message</param>
    public void AddError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
            _errors.Add(error);
    }

    /// <summary>
    /// Adds a validation warning
    /// </summary>
    /// <param name="warning">Warning message</param>
    public void AddWarning(string warning)
    {
        if (!string.IsNullOrWhiteSpace(warning))
            _warnings.Add(warning);
    }

    /// <summary>
    /// Gets a summary of the validation result
    /// </summary>
    /// <returns>Validation summary</returns>
    public override string ToString()
    {
        if (IsValid && !HasWarnings)
            return "Valid";

        var summary = new List<string>();
        
        if (_errors.Count > 0)
            summary.Add($"{_errors.Count} error(s)");
            
        if (_warnings.Count > 0)
            summary.Add($"{_warnings.Count} warning(s)");

        return string.Join(", ", summary);
    }
}