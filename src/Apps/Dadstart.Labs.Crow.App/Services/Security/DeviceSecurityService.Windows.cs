#if WINDOWS
namespace Dadstart.Labs.Crow.Services.Security;

using Windows.Security.Credentials.UI;

public sealed partial class DeviceSecurityService
{
    private partial bool PlatformIsBiometricAvailable()
    {
        try
        {
            var availability = UserConsentVerifier.CheckAvailabilityAsync().AsTask().GetAwaiter().GetResult();
            return availability == UserConsentVerifierAvailability.Available;
        }
        catch
        {
            return false;
        }
    }

    private partial async Task<bool> PlatformAuthenticateAsync(string reason, CancellationToken cancellationToken)
    {
        var availability = await UserConsentVerifier.CheckAvailabilityAsync().AsTask(cancellationToken);
        if (availability != UserConsentVerifierAvailability.Available)
        {
            return false;
        }

        var result = await UserConsentVerifier.RequestVerificationAsync(reason).AsTask(cancellationToken);
        return result == UserConsentVerificationResult.Verified;
    }
}
#endif

