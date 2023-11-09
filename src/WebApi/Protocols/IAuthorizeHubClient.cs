namespace WebApi.Protocols
{
    public interface IAuthorizeHubClient
    {
        Task OnSucceed(string access_token);

        Task OnFailed(int ErrorCode, string Message, object? ExtensionData);
    }
}
