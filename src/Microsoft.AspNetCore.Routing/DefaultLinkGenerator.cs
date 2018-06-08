using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Routing
{
    public class DefaultLinkGenerator : ILinkGenerator
    {
        private readonly IEndpointFinder _endpointFinder;
        private readonly ILogger<DefaultLinkGenerator> _logger;

        public DefaultLinkGenerator(
            IEndpointFinder endpointFinder,
            ILogger<DefaultLinkGenerator> logger)
        {
            _endpointFinder = endpointFinder;
            _logger = logger;
        }

        public string GetLink(Address address, IDictionary<string, object> values)
        {
            if (TryGetLink(address, values, out var link))
            {
                return link;
            }

            throw new InvalidOperationException("Could not find a matching endpoint to generate a link");
        }

        public bool TryGetLink(Address address, IDictionary<string, object> values, out string link)
        {
            var endpoint = _endpointFinder.FindEndpoint(address);
            if (endpoint == null)
            {
                _logger.LogDebug($"Could not find an endpoint having an address with name '{address.Name}' and " +
                    $"MethodInfo '{address.MethodInfo.DeclaringType.FullName}.{address.MethodInfo.Name}'.");
            }

            link = GetLink(endpoint.Template, values);
            return link != null;
        }

        private string GetLink(string template, IDictionary<string, object> values)
        {
            if (!template.Contains("{"))
            {
                return template;
            }

            var updatable = new StringBuilder(template);

            var segments = template.Split('/');
            foreach (var segment in segments)
            {
                if (IsVariableSegment(segment))
                {
                    var trimmed = segment.TrimStart('{').TrimEnd('}');

                    if (values.ContainsKey(trimmed))
                    {
                        updatable.Replace(segment, values[trimmed].ToString());
                    }
                    else
                    {
                        throw new InvalidOperationException($"Supplied values do not match the given endpoint's template");
                    }
                }
            }

            return updatable.ToString();
        }

        private bool IsVariableSegment(string templateSegment)
        {
            return templateSegment.StartsWith("{") && templateSegment.EndsWith("}");
        }
    }
}
