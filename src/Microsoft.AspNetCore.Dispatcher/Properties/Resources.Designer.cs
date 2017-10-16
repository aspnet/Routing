// <auto-generated />
namespace Microsoft.AspNetCore.Dispatcher
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Resources
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.AspNetCore.Dispatcher.Resources", typeof(Resources).GetTypeInfo().Assembly);

        /// <summary>
        /// Multiple endpoints matched. The following endpoints matched the request:{0}{0}{1}
        /// </summary>
        internal static string AmbiguousEndpoints
        {
            get => GetString("AmbiguousEndpoints");
        }

        /// <summary>
        /// Multiple endpoints matched. The following endpoints matched the request:{0}{0}{1}
        /// </summary>
        internal static string FormatAmbiguousEndpoints(object p0, object p1)
            => string.Format(CultureInfo.CurrentCulture, GetString("AmbiguousEndpoints"), p0, p1);

        /// <summary>
        /// A path segment that contains more than one section, such as a literal section or a parameter, cannot contain a catch-all parameter.
        /// </summary>
        internal static string TemplateRoute_CannotHaveCatchAllInMultiSegment
        {
            get => GetString("TemplateRoute_CannotHaveCatchAllInMultiSegment");
        }

        /// <summary>
        /// A path segment that contains more than one section, such as a literal section or a parameter, cannot contain a catch-all parameter.
        /// </summary>
        internal static string FormatTemplateRoute_CannotHaveCatchAllInMultiSegment()
            => GetString("TemplateRoute_CannotHaveCatchAllInMultiSegment");

        /// <summary>
        /// A path segment cannot contain two consecutive parameters. They must be separated by a '/' or by a literal string.
        /// </summary>
        internal static string TemplateRoute_CannotHaveConsecutiveParameters
        {
            get => GetString("TemplateRoute_CannotHaveConsecutiveParameters");
        }

        /// <summary>
        /// A path segment cannot contain two consecutive parameters. They must be separated by a '/' or by a literal string.
        /// </summary>
        internal static string FormatTemplateRoute_CannotHaveConsecutiveParameters()
            => GetString("TemplateRoute_CannotHaveConsecutiveParameters");

        /// <summary>
        /// The route template separator character '/' cannot appear consecutively. It must be separated by either a parameter or a literal value.
        /// </summary>
        internal static string TemplateRoute_CannotHaveConsecutiveSeparators
        {
            get => GetString("TemplateRoute_CannotHaveConsecutiveSeparators");
        }

        /// <summary>
        /// The route template separator character '/' cannot appear consecutively. It must be separated by either a parameter or a literal value.
        /// </summary>
        internal static string FormatTemplateRoute_CannotHaveConsecutiveSeparators()
            => GetString("TemplateRoute_CannotHaveConsecutiveSeparators");

        /// <summary>
        /// A catch-all parameter cannot be marked optional.
        /// </summary>
        internal static string TemplateRoute_CatchAllCannotBeOptional
        {
            get => GetString("TemplateRoute_CatchAllCannotBeOptional");
        }

        /// <summary>
        /// A catch-all parameter cannot be marked optional.
        /// </summary>
        internal static string FormatTemplateRoute_CatchAllCannotBeOptional()
            => GetString("TemplateRoute_CatchAllCannotBeOptional");

        /// <summary>
        /// A catch-all parameter can only appear as the last segment of the route template.
        /// </summary>
        internal static string TemplateRoute_CatchAllMustBeLast
        {
            get => GetString("TemplateRoute_CatchAllMustBeLast");
        }

        /// <summary>
        /// A catch-all parameter can only appear as the last segment of the route template.
        /// </summary>
        internal static string FormatTemplateRoute_CatchAllMustBeLast()
            => GetString("TemplateRoute_CatchAllMustBeLast");

        /// <summary>
        /// The literal section '{0}' is invalid. Literal sections cannot contain the '?' character.
        /// </summary>
        internal static string TemplateRoute_InvalidLiteral
        {
            get => GetString("TemplateRoute_InvalidLiteral");
        }

        /// <summary>
        /// The literal section '{0}' is invalid. Literal sections cannot contain the '?' character.
        /// </summary>
        internal static string FormatTemplateRoute_InvalidLiteral(object p0)
            => string.Format(CultureInfo.CurrentCulture, GetString("TemplateRoute_InvalidLiteral"), p0);

        /// <summary>
        /// The route parameter name '{0}' is invalid. Route parameter names must be non-empty and cannot contain these characters: '{{', '}}', '/'. The '?' character marks a parameter as optional, and can occur only at the end of the parameter. The '*' character marks a parameter as catch-all, and can occur only at the start of the parameter.
        /// </summary>
        internal static string TemplateRoute_InvalidParameterName
        {
            get => GetString("TemplateRoute_InvalidParameterName");
        }

        /// <summary>
        /// The route parameter name '{0}' is invalid. Route parameter names must be non-empty and cannot contain these characters: '{{', '}}', '/'. The '?' character marks a parameter as optional, and can occur only at the end of the parameter. The '*' character marks a parameter as catch-all, and can occur only at the start of the parameter.
        /// </summary>
        internal static string FormatTemplateRoute_InvalidParameterName(object p0)
            => string.Format(CultureInfo.CurrentCulture, GetString("TemplateRoute_InvalidParameterName"), p0);

        /// <summary>
        /// The route template cannot start with a '/' or '~' character.
        /// </summary>
        internal static string TemplateRoute_InvalidRouteTemplate
        {
            get => GetString("TemplateRoute_InvalidRouteTemplate");
        }

        /// <summary>
        /// The route template cannot start with a '/' or '~' character.
        /// </summary>
        internal static string FormatTemplateRoute_InvalidRouteTemplate()
            => GetString("TemplateRoute_InvalidRouteTemplate");

        /// <summary>
        /// There is an incomplete parameter in the route template. Check that each '{' character has a matching '}' character.
        /// </summary>
        internal static string TemplateRoute_MismatchedParameter
        {
            get => GetString("TemplateRoute_MismatchedParameter");
        }

        /// <summary>
        /// There is an incomplete parameter in the route template. Check that each '{' character has a matching '}' character.
        /// </summary>
        internal static string FormatTemplateRoute_MismatchedParameter()
            => GetString("TemplateRoute_MismatchedParameter");

        /// <summary>
        /// An optional parameter cannot have default value.
        /// </summary>
        internal static string TemplateRoute_OptionalCannotHaveDefaultValue
        {
            get => GetString("TemplateRoute_OptionalCannotHaveDefaultValue");
        }

        /// <summary>
        /// An optional parameter cannot have default value.
        /// </summary>
        internal static string FormatTemplateRoute_OptionalCannotHaveDefaultValue()
            => GetString("TemplateRoute_OptionalCannotHaveDefaultValue");

        /// <summary>
        /// In the segment '{0}', the optional parameter '{1}' is preceded by an invalid segment '{2}'. Only a period (.) can precede an optional parameter.
        /// </summary>
        internal static string TemplateRoute_OptionalParameterCanbBePrecededByPeriod
        {
            get => GetString("TemplateRoute_OptionalParameterCanbBePrecededByPeriod");
        }

        /// <summary>
        /// In the segment '{0}', the optional parameter '{1}' is preceded by an invalid segment '{2}'. Only a period (.) can precede an optional parameter.
        /// </summary>
        internal static string FormatTemplateRoute_OptionalParameterCanbBePrecededByPeriod(object p0, object p1, object p2)
            => string.Format(CultureInfo.CurrentCulture, GetString("TemplateRoute_OptionalParameterCanbBePrecededByPeriod"), p0, p1, p2);

        /// <summary>
        /// An optional parameter must be at the end of the segment. In the segment '{0}', optional parameter '{1}' is followed by '{2}'.
        /// </summary>
        internal static string TemplateRoute_OptionalParameterHasTobeTheLast
        {
            get => GetString("TemplateRoute_OptionalParameterHasTobeTheLast");
        }

        /// <summary>
        /// An optional parameter must be at the end of the segment. In the segment '{0}', optional parameter '{1}' is followed by '{2}'.
        /// </summary>
        internal static string FormatTemplateRoute_OptionalParameterHasTobeTheLast(object p0, object p1, object p2)
            => string.Format(CultureInfo.CurrentCulture, GetString("TemplateRoute_OptionalParameterHasTobeTheLast"), p0, p1, p2);

        /// <summary>
        /// The route parameter name '{0}' appears more than one time in the route template.
        /// </summary>
        internal static string TemplateRoute_RepeatedParameter
        {
            get => GetString("TemplateRoute_RepeatedParameter");
        }

        /// <summary>
        /// The route parameter name '{0}' appears more than one time in the route template.
        /// </summary>
        internal static string FormatTemplateRoute_RepeatedParameter(object p0)
            => string.Format(CultureInfo.CurrentCulture, GetString("TemplateRoute_RepeatedParameter"), p0);

        /// <summary>
        /// In a route parameter, '{' and '}' must be escaped with '{{' and '}}'.
        /// </summary>
        internal static string TemplateRoute_UnescapedBrace
        {
            get => GetString("TemplateRoute_UnescapedBrace");
        }

        /// <summary>
        /// In a route parameter, '{' and '}' must be escaped with '{{' and '}}'.
        /// </summary>
        internal static string FormatTemplateRoute_UnescapedBrace()
            => GetString("TemplateRoute_UnescapedBrace");

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);

            System.Diagnostics.Debug.Assert(value != null);

            if (formatterNames != null)
            {
                for (var i = 0; i < formatterNames.Length; i++)
                {
                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
                }
            }

            return value;
        }
    }
}
