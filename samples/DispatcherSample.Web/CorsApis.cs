using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace DispatcherSample.Web
{
    public interface ICorsService
    {
        CorsResult EvaluatePolicy(HttpContext context, CorsPolicy policy);

        void ApplyResult(CorsResult result, HttpResponse response);
    }

    public class DefaultCorsService : ICorsService
    {
        public void ApplyResult(CorsResult result, HttpResponse response)
        {
            if (result.AllowedOrigin != null)
            {
                response.Headers[HeaderNames.AccessControlAllowOrigin] = result.AllowedOrigin;
            }
        }

        public CorsResult EvaluatePolicy(HttpContext context, CorsPolicy policy)
        {
            var corsResult = new CorsResult();
            var origin = context.Request.Headers[HeaderNames.Origin];
            if (policy.IsOriginAllowed(origin))
            {
                corsResult.AllowedOrigin = origin;
            }
            return corsResult;
        }
    }

    /// <summary>
    /// Results returned by <see cref="ICorsService"/>.
    /// </summary>
    public class CorsResult
    {
        private TimeSpan? _preflightMaxAge;

        /// <summary>
        /// Gets or sets the allowed origin.
        /// </summary>
        public string AllowedOrigin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the resource supports user credentials.
        /// </summary>
        public bool SupportsCredentials { get; set; }

        /// <summary>
        /// Gets the allowed methods.
        /// </summary>
        public IList<string> AllowedMethods { get; } = new List<string>();

        /// <summary>
        /// Gets the allowed headers.
        /// </summary>
        public IList<string> AllowedHeaders { get; } = new List<string>();

        /// <summary>
        /// Gets the allowed headers that can be exposed on the response.
        /// </summary>
        public IList<string> AllowedExposedHeaders { get; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating if a 'Vary' header with the value 'Origin' is required.
        /// </summary>
        public bool VaryByOrigin { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> for which the results of a preflight request can be cached.
        /// </summary>
        public TimeSpan? PreflightMaxAge
        {
            get
            {
                return _preflightMaxAge;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _preflightMaxAge = value;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("AllowCredentials: ");
            builder.Append(SupportsCredentials);
            builder.Append(", PreflightMaxAge: ");
            builder.Append(PreflightMaxAge.HasValue ?
                PreflightMaxAge.Value.TotalSeconds.ToString() : "null");
            builder.Append(", AllowOrigin: ");
            builder.Append(AllowedOrigin);
            builder.Append(", AllowExposedHeaders: {");
            builder.Append(string.Join(",", AllowedExposedHeaders));
            builder.Append("}");
            builder.Append(", AllowHeaders: {");
            builder.Append(string.Join(",", AllowedHeaders));
            builder.Append("}");
            builder.Append(", AllowMethods: {");
            builder.Append(string.Join(",", AllowedMethods));
            builder.Append("}");
            return builder.ToString();
        }
    }

    /// <summary>
    /// Defines the policy for Cross-Origin requests based on the CORS specifications.
    /// </summary>
    public class CorsPolicy
    {
        private TimeSpan? _preflightMaxAge;

        /// <summary>
        /// Default constructor for a CorsPolicy.
        /// </summary>
        public CorsPolicy()
        {
            IsOriginAllowed = DefaultIsOriginAllowed;
        }

        /// <summary>
        /// Gets a value indicating if all headers are allowed.
        /// </summary>
        public bool AllowAnyHeader
        {
            get
            {
                if (Headers == null || Headers.Count != 1 || Headers.Count == 1 && Headers[0] != "*")
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating if all methods are allowed.
        /// </summary>
        public bool AllowAnyMethod
        {
            get
            {
                if (Methods == null || Methods.Count != 1 || Methods.Count == 1 && Methods[0] != "*")
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating if all origins are allowed.
        /// </summary>
        public bool AllowAnyOrigin
        {
            get
            {
                if (Origins == null || Origins.Count != 1 || Origins.Count == 1 && Origins[0] != "*")
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets or sets a function which evaluates whether an origin is allowed.
        /// </summary>
        public Func<string, bool> IsOriginAllowed { get; set; }

        /// <summary>
        /// Gets the headers that the resource might use and can be exposed.
        /// </summary>
        public IList<string> ExposedHeaders { get; } = new List<string>();

        /// <summary>
        /// Gets the headers that are supported by the resource.
        /// </summary>
        public IList<string> Headers { get; } = new List<string>();

        /// <summary>
        /// Gets the methods that are supported by the resource.
        /// </summary>
        public IList<string> Methods { get; } = new List<string>();

        /// <summary>
        /// Gets the origins that are allowed to access the resource.
        /// </summary>
        public IList<string> Origins { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> for which the results of a preflight request can be cached.
        /// </summary>
        public TimeSpan? PreflightMaxAge
        {
            get
            {
                return _preflightMaxAge;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _preflightMaxAge = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the resource supports user credentials in the request.
        /// </summary>
        public bool SupportsCredentials { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("AllowAnyHeader: ");
            builder.Append(AllowAnyHeader);
            builder.Append(", AllowAnyMethod: ");
            builder.Append(AllowAnyMethod);
            builder.Append(", AllowAnyOrigin: ");
            builder.Append(AllowAnyOrigin);
            builder.Append(", PreflightMaxAge: ");
            builder.Append(PreflightMaxAge.HasValue ?
                PreflightMaxAge.Value.TotalSeconds.ToString() : "null");
            builder.Append(", SupportsCredentials: ");
            builder.Append(SupportsCredentials);
            builder.Append(", Origins: {");
            builder.Append(string.Join(",", Origins));
            builder.Append("}");
            builder.Append(", Methods: {");
            builder.Append(string.Join(",", Methods));
            builder.Append("}");
            builder.Append(", Headers: {");
            builder.Append(string.Join(",", Headers));
            builder.Append("}");
            builder.Append(", ExposedHeaders: {");
            builder.Append(string.Join(",", ExposedHeaders));
            builder.Append("}");
            return builder.ToString();
        }

        private bool DefaultIsOriginAllowed(string origin)
        {
            return Origins.Contains(origin, StringComparer.Ordinal);
        }
    }

    public interface IEnableCorsAttribute
    {
        string PolicyName
        {
            get; set;
        }
    }

    public class EnableCorsAttribute : IEnableCorsAttribute
    {
        public string PolicyName
        {
            get; set;
        }
    }

    public interface IDisableCorsAttribute { }

    public class DisableCorsAttribute : IDisableCorsAttribute { }

    /// <summary>
    /// CORS-related constants.
    /// </summary>
    public interface ICorsPolicyProvider
    {
        /// <summary>
        /// Gets a <see cref="CorsPolicy"/> from the given <paramref name="context"/>
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> associated with this call.</param>
        /// <param name="policyName">An optional policy name to look for.</param>
        /// <returns>A <see cref="CorsPolicy"/></returns>
        Task<CorsPolicy> GetPolicyAsync(HttpContext context, string policyName);
    }

    public class DefaultCorsPolicyProvider : ICorsPolicyProvider
    {
        private readonly CorsOptions _options;

        /// <summary>
        /// Creates a new instance of <see cref="DefaultCorsPolicyProvider"/>.
        /// </summary>
        /// <param name="options">The options configured for the application.</param>
        public DefaultCorsPolicyProvider(IOptions<CorsOptions> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc />
        public Task<CorsPolicy> GetPolicyAsync(HttpContext context, string policyName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult(_options.GetPolicy(policyName ?? _options.DefaultPolicyName));
        }
    }

    /// <summary>
    /// Provides programmatic configuration for Cors.
    /// </summary>
    public class CorsOptions
    {
        private string _defaultPolicyName = "__DefaultCorsPolicy";
        private IDictionary<string, CorsPolicy> PolicyMap { get; } = new Dictionary<string, CorsPolicy>();

        public string DefaultPolicyName
        {
            get
            {
                return _defaultPolicyName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _defaultPolicyName = value;
            }
        }

        /// <summary>
        /// Adds a new policy and sets it as the default.
        /// </summary>
        /// <param name="policy">The <see cref="CorsPolicy"/> policy to be added.</param>
        public void AddDefaultPolicy(CorsPolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            AddPolicy(DefaultPolicyName, policy);
        }

        /// <summary>
        /// Adds a new policy and sets it as the default.
        /// </summary>
        /// <param name="configurePolicy">A delegate which can use a policy builder to build a policy.</param>
        public void AddDefaultPolicy(Action<CorsPolicyBuilder> configurePolicy)
        {
            if (configurePolicy == null)
            {
                throw new ArgumentNullException(nameof(configurePolicy));
            }

            AddPolicy(DefaultPolicyName, configurePolicy);
        }

        /// <summary>
        /// Adds a new policy.
        /// </summary>
        /// <param name="name">The name of the policy.</param>
        /// <param name="policy">The <see cref="CorsPolicy"/> policy to be added.</param>
        public void AddPolicy(string name, CorsPolicy policy)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            PolicyMap[name] = policy;
        }

        /// <summary>
        /// Adds a new policy.
        /// </summary>
        /// <param name="name">The name of the policy.</param>
        /// <param name="configurePolicy">A delegate which can use a policy builder to build a policy.</param>
        public void AddPolicy(string name, Action<CorsPolicyBuilder> configurePolicy)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (configurePolicy == null)
            {
                throw new ArgumentNullException(nameof(configurePolicy));
            }

            var policyBuilder = new CorsPolicyBuilder();
            configurePolicy(policyBuilder);
            PolicyMap[name] = policyBuilder.Build();
        }

        /// <summary>
        /// Gets the policy based on the <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name of the policy to lookup.</param>
        /// <returns>The <see cref="CorsPolicy"/> if the policy was added.<c>null</c> otherwise.</returns>
        public CorsPolicy GetPolicy(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return PolicyMap.ContainsKey(name) ? PolicyMap[name] : null;
        }
    }

    /// <summary>
    /// Exposes methods to build a policy.
    /// </summary>
    public class CorsPolicyBuilder
    {
        private readonly CorsPolicy _policy = new CorsPolicy();

        /// <summary>
        /// Creates a new instance of the <see cref="CorsPolicyBuilder"/>.
        /// </summary>
        /// <param name="origins">list of origins which can be added.</param>
        public CorsPolicyBuilder(params string[] origins)
        {
            WithOrigins(origins);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CorsPolicyBuilder"/>.
        /// </summary>
        /// <param name="policy">The policy which will be used to intialize the builder.</param>
        public CorsPolicyBuilder(CorsPolicy policy)
        {
            Combine(policy);
        }

        /// <summary>
        /// Adds the specified <paramref name="origins"/> to the policy.
        /// </summary>
        /// <param name="origins">The origins that are allowed.</param>
        /// <returns>The current policy builder.</returns>
        public CorsPolicyBuilder WithOrigins(params string[] origins)
        {
            foreach (var req in origins)
            {
                _policy.Origins.Add(req);
            }

            return this;
        }

        /// <summary>
        /// Adds the specified <paramref name="headers"/> to the policy.
        /// </summary>
        /// <param name="headers">The headers which need to be allowed in the request.</param>
        /// <returns>The current policy builder.</returns>
        public CorsPolicyBuilder WithHeaders(params string[] headers)
        {
            foreach (var req in headers)
            {
                _policy.Headers.Add(req);
            }
            return this;
        }

        /// <summary>
        /// Adds the specified <paramref name="exposedHeaders"/> to the policy.
        /// </summary>
        /// <param name="exposedHeaders">The headers which need to be exposed to the client.</param>
        /// <returns>The current policy builder.</returns>
        public CorsPolicyBuilder WithExposedHeaders(params string[] exposedHeaders)
        {
            foreach (var req in exposedHeaders)
            {
                _policy.ExposedHeaders.Add(req);
            }

            return this;
        }

        /// <summary>
        /// Adds the specified <paramref name="methods"/> to the policy.
        /// </summary>
        /// <param name="methods">The methods which need to be added to the policy.</param>
        /// <returns>The current policy builder.</returns>
        public CorsPolicyBuilder WithMethods(params string[] methods)
        {
            foreach (var req in methods)
            {
                _policy.Methods.Add(req);
            }

            return this;
        }

        /// <summary>
        /// Sets the policy to allow credentials.
        /// </summary>
        /// <returns>The current policy builder.</returns>
        public CorsPolicyBuilder AllowCredentials()
        {
            _policy.SupportsCredentials = true;
            return this;
        }

        /// <summary>
        /// Sets the policy to not allow credentials.
        /// </summary>
        /// <returns>The current policy builder.</returns>
        public CorsPolicyBuilder DisallowCredentials()
        {
            _policy.SupportsCredentials = false;
            return this;
        }

        /// <summary>
        /// Ensures that the policy allows any origin.
        /// </summary>
        /// <returns>The current policy builder.</returns>
        public CorsPolicyBuilder AllowAnyOrigin()
        {
            _policy.Origins.Clear();
            _policy.Origins.Add("*");
            return this;
        }

        /// <summary>
        /// Ensures that the policy allows any method.
        /// </summary>
        /// <returns>The current policy builder.</returns>
        public CorsPolicyBuilder AllowAnyMethod()
        {
            _policy.Methods.Clear();
            _policy.Methods.Add("*");
            return this;
        }

        /// <summary>
        /// Ensures that the policy allows any header.
        /// </summary>
        /// <returns>The current policy builder.</returns>
        public CorsPolicyBuilder AllowAnyHeader()
        {
            _policy.Headers.Clear();
            _policy.Headers.Add("*");
            return this;
        }

        /// <summary>
        /// Sets the preflightMaxAge for the underlying policy.
        /// </summary>
        /// <param name="preflightMaxAge">A positive <see cref="TimeSpan"/> indicating the time a preflight
        /// request can be cached.</param>
        /// <returns>The current policy builder.</returns>
        public CorsPolicyBuilder SetPreflightMaxAge(TimeSpan preflightMaxAge)
        {
            _policy.PreflightMaxAge = preflightMaxAge;
            return this;
        }

        /// <summary>
        /// Sets the specified <paramref name="isOriginAllowed"/> for the underlying policy.
        /// </summary>
        /// <param name="isOriginAllowed">The function used by the policy to evaluate if an origin is allowed.</param>
        /// <returns>The current policy builder.</returns>
        public CorsPolicyBuilder SetIsOriginAllowed(Func<string, bool> isOriginAllowed)
        {
            _policy.IsOriginAllowed = isOriginAllowed;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="CorsPolicy.IsOriginAllowed"/> property of the policy to be a function
        /// that allows origins to match a configured wildcarded domain when evaluating if the 
        /// origin is allowed.
        /// </summary>
        /// <returns>The current policy builder.</returns>
        //public CorsPolicyBuilder SetIsOriginAllowedToAllowWildcardSubdomains()
        //{
        //    _policy.IsOriginAllowed = _policy.IsOriginAnAllowedSubdomain;
        //    return this;
        //}

        /// <summary>
        /// Builds a new <see cref="CorsPolicy"/> using the entries added.
        /// </summary>
        /// <returns>The constructed <see cref="CorsPolicy"/>.</returns>
        public CorsPolicy Build()
        {
            return _policy;
        }

        /// <summary>
        /// Combines the given <paramref name="policy"/> to the existing properties in the builder.
        /// </summary>
        /// <param name="policy">The policy which needs to be combined.</param>
        /// <returns>The current policy builder.</returns>
        private CorsPolicyBuilder Combine(CorsPolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            WithOrigins(policy.Origins.ToArray());
            WithHeaders(policy.Headers.ToArray());
            WithExposedHeaders(policy.ExposedHeaders.ToArray());
            WithMethods(policy.Methods.ToArray());
            SetIsOriginAllowed(policy.IsOriginAllowed);

            if (policy.PreflightMaxAge.HasValue)
            {
                SetPreflightMaxAge(policy.PreflightMaxAge.Value);
            }

            if (policy.SupportsCredentials)
            {
                AllowCredentials();
            }
            else
            {
                DisallowCredentials();
            }

            return this;
        }
    }
}
