using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

public class BaseController : Controller
{
    protected string SessionId
    {
        get
        {
            var sessionKey = "CartSessionId";
            var id = HttpContext.Session.GetString(sessionKey);
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
                HttpContext.Session.SetString(sessionKey, id);
            }
            return id;
        }
    }
}