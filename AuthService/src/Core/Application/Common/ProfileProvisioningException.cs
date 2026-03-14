namespace AuthenticationService.Application.Common;

public sealed class ProfileProvisioningException : Exception
{
    public ProfileProvisioningException(string message, bool isConflict, Exception? innerException = null)
        : base(message, innerException)
    {
        IsConflict = isConflict;
    }

    public bool IsConflict { get; }
}
