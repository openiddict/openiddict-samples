namespace Geonosis.Auth.ViewModels.Shared;

internal class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
