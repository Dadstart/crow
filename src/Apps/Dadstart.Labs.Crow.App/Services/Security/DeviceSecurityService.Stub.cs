#if !WINDOWS
namespace Dadstart.Labs.Crow.Services.Security;

public sealed partial class DeviceSecurityService
{
    private partial bool PlatformIsBiometricAvailable()
        => false;

    private partial Task<bool> PlatformAuthenticateAsync(string reason, CancellationToken cancellationToken)
        => Task.FromResult(false);
}
#endif

