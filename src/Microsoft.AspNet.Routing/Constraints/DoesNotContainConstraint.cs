public class DoesNotContainConstraint : IRouteConstraint
    {
        private readonly string m_Substring;

        public DoesNotContainConstraint(string substring)
        {
            m_Substring = substring;
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, IDictionary<string, object> values, RouteDirection routeDirection)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            if (routeKey == null)
            {
                throw new ArgumentNullException(nameof(routeKey));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            object routeValue;

            if (values.TryGetValue(routeKey, out routeValue)
                && routeValue != null)
            {
                string parameterValueString = Convert.ToString(routeValue, CultureInfo.InvariantCulture);

                return !parameterValueString.Contains(m_Substring);
            }

            return true;
        }
    }
