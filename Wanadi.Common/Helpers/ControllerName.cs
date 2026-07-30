using Microsoft.AspNetCore.Mvc;

namespace Wanadi.Common.Helpers;

public static class ControllerName
{
    public static string Of<TController>() where TController : Controller
        => typeof(TController).Name.Replace("Controller", string.Empty);
}