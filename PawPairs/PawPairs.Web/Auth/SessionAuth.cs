namespace PawPairs.Web.Auth;

public static class SessionAuth
{
    private const string UserIdKey = "UserId";
    private const string UserNameKey = "UserName";

    public static void SignIn(HttpContext ctx, Guid userId, string displayName)
    {
        ctx.Session.SetString(UserIdKey, userId.ToString());
        ctx.Session.SetString(UserNameKey, displayName);
    }

    public static void SignOut(HttpContext ctx)
    {
        ctx.Session.Remove(UserIdKey);
        ctx.Session.Remove(UserNameKey);
    }

    public static Guid? GetUserId(HttpContext ctx)
    {
        var s = ctx.Session.GetString(UserIdKey);
        return Guid.TryParse(s, out var id) ? id : null;
    }

    public static string? GetUserName(HttpContext ctx)
        => ctx.Session.GetString(UserNameKey);

    public static bool IsSignedIn(HttpContext ctx)
        => GetUserId(ctx).HasValue;
}