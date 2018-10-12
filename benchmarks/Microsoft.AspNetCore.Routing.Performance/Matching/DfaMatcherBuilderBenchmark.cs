using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.TestObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Routing.Matching
{
    public class DfaMatcherBuilderBenchmark
    {
        private IEnumerable<MatcherPolicy> _policies;
        private ILoggerFactory _loggerFactory;
        private DefaultParameterPolicyFactory _parameterPolicyFactory;
        private RouteEndpoint[] _endpoints;
        private EndpointSelector _selector;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _policies = new List<MatcherPolicy>()
                {
                    CreateNodeBuilderPolicy(4),
                    CreateUberPolicy(2),
                    CreateNodeBuilderPolicy(3),
                    CreateEndpointComparerPolicy(5),
                    CreateNodeBuilderPolicy(1),
                    CreateEndpointSelectorPolicy(9),
                    CreateEndpointComparerPolicy(7),
                    CreateNodeBuilderPolicy(6),
                    CreateEndpointSelectorPolicy(10),
                    CreateUberPolicy(12),
                    CreateEndpointComparerPolicy(11)
                };
            _loggerFactory = NullLoggerFactory.Instance;
            _selector = new DefaultEndpointSelector();
            _parameterPolicyFactory = new DefaultParameterPolicyFactory(Options.Create(new RouteOptions()), new TestServiceProvider());
            _endpoints = new RouteEndpoint[12]
                {
                    CreateEndpoint("/account", "GET"),
                    CreateEndpoint("/analyze", "POST"),
                    CreateEndpoint("/apis", "GET"),
                    CreateEndpoint("/Applications", "GET"),
                    CreateEndpoint("/ApplicationTypes", "GET"),
                    CreateEndpoint("/apps/", "GET"),
                    CreateEndpoint("/apps/", "POST"),
                    CreateEndpoint("/authorizationServers", "GET"),
                    CreateEndpoint("/backends", "GET"),
                    CreateEndpoint("/BuildJob", "POST"),
                    CreateEndpoint("/certificates", "POST"),
                    CreateEndpoint("/certificates", "GET")
                };
        }

        [Benchmark]
        public void Ctor()
        {
            new DfaMatcherBuilder(_loggerFactory, _parameterPolicyFactory, _selector, _policies);
        }

        [Benchmark]
        public void ConstructAndBuild()
        {
            var builder = new DfaMatcherBuilder(_loggerFactory, _parameterPolicyFactory, _selector, _policies);

            for (var i = 0; i < _endpoints.Length; i++)
                builder.AddEndpoint(_endpoints[i]);

            builder.Build();
        }

        private static RouteEndpoint CreateEndpoint(string template, string httpMethod)
        {
            return CreateEndpoint(template, metadata: new object[]
            {
                new HttpMethodMetadata(new string[]{ httpMethod, }),
            });
        }

        private static RouteEndpoint CreateEndpoint(string template, params object[] metadata)
        {
            var endpointMetadata = new List<object>(metadata ?? Array.Empty<object>());

            return new RouteEndpoint(
                (context) => Task.CompletedTask,
                RoutePatternFactory.Parse(template, null, null),
                0,
                new EndpointMetadataCollection(endpointMetadata),
                null);
        }

        private static MatcherPolicy CreateNodeBuilderPolicy(int order)
        {
            return new TestNodeBuilderPolicy(order);
        }
        private static MatcherPolicy CreateEndpointComparerPolicy(int order)
        {
            return new TestEndpointComparerPolicy(order);
        }

        private static MatcherPolicy CreateEndpointSelectorPolicy(int order)
        {
            return new TestEndpointSelectorPolicy(order);
        }

        private static MatcherPolicy CreateUberPolicy(int order)
        {
            return new TestUberPolicy(order);
        }

        private class TestUberPolicy : TestMatcherPolicyBase, INodeBuilderPolicy, IEndpointComparerPolicy
        {
            public TestUberPolicy(int order) : base(order)
            {
            }

            public IComparer<Endpoint> Comparer => new TestEndpointComparer();

            public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
            {
                return false;
            }

            public PolicyJumpTable BuildJumpTable(int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges)
            {
                throw new NotImplementedException();
            }

            public IReadOnlyList<PolicyNodeEdge> GetEdges(IReadOnlyList<Endpoint> endpoints)
            {
                throw new NotImplementedException();
            }
        }

        private class TestNodeBuilderPolicy : TestMatcherPolicyBase, INodeBuilderPolicy
        {
            public TestNodeBuilderPolicy(int order) : base(order)
            {
            }

            public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
            {
                return false;
            }

            public PolicyJumpTable BuildJumpTable(int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges)
            {
                throw new NotImplementedException();
            }

            public IReadOnlyList<PolicyNodeEdge> GetEdges(IReadOnlyList<Endpoint> endpoints)
            {
                throw new NotImplementedException();
            }
        }

        private class TestEndpointComparerPolicy : TestMatcherPolicyBase, IEndpointComparerPolicy
        {
            public TestEndpointComparerPolicy(int order) : base(order)
            {
            }

            public IComparer<Endpoint> Comparer => new TestEndpointComparer();

            public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
            {
                return false;
            }

            public Task ApplyAsync(HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates)
            {
                throw new NotImplementedException();
            }
        }

        private class TestEndpointSelectorPolicy : TestMatcherPolicyBase, IEndpointSelectorPolicy
        {
            public TestEndpointSelectorPolicy(int order) : base(order)
            {
            }

            public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
            {
                return false;
            }

            public Task ApplyAsync(HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates)
            {
                throw new NotImplementedException();
            }
        }

        private abstract class TestMatcherPolicyBase : MatcherPolicy
        {
            private int _order;

            protected TestMatcherPolicyBase(int order)
            {
                _order = order;
            }

            public override int Order { get { return _order; } }
        }

        private class TestEndpointComparer : IComparer<Endpoint>
        {
            public int Compare(Endpoint x, Endpoint y)
            {
                return 0;
            }
        }
    }
}
