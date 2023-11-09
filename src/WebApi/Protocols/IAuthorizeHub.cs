namespace WebApi.Protocols
{
    public record SignInReq(long UserId, string Password, int ServerId);

    public interface IAuthorizeHub
    {
        Task SignInAsync(SignInReq context);
    }
}
