using Microsoft.AspNetCore.Routing;
using System.Globalization;

namespace WannabeTrello.Extensions;

public class LongRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var value) && value != null)
        {
            // If already a long, it's valid
            if (value is long)
            {
                return true;
            }

            // Try to parse the string value as long
            var stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);
            return long.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }

        return false;
    }
}
