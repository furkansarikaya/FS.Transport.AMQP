using FS.Transport.AMQP.Configuration;

namespace FS.Transport.AMQP.RetryPolicies;

/// <summary>
/// Factory interface for creating retry policies
/// </summary>
public interface IRetryPolicyFactory
{
    /// <summary>
    /// Creates a retry policy based on the specified settings
    /// </summary>
    /// <param name="settings">Retry policy settings</param>
    /// <returns>Configured retry policy</returns>
    IRetryPolicy CreateRetryPolicy(RetryPolicySettings settings);
    
    /// <summary>
    /// Creates a retry policy of the specified type
    /// </summary>
    /// <param name="policyType">Policy type name</param>
    /// <param name="settings">Retry policy settings</param>
    /// <returns>Configured retry policy</returns>
    IRetryPolicy CreateRetryPolicy(string policyType, RetryPolicySettings settings);
}
