using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Microsoft.AspNetCore.Routing
{
    public class RouteParameterParserBenchmark
    {
        [Benchmark]
        public void MultipleConstraints()
        {
            RouteParameterParser.ParseRouteParameter(@"param:test(abc:somevalue):name(test1:differentname:alias=default-value");
        }

        [Benchmark]
        public void NoConstraints()
        {
            RouteParameterParser.ParseRouteParameter(@"param=");
        }

        [Benchmark]
        public void SingleConstraint()
        {
            RouteParameterParser.ParseRouteParameter(@"param:int=111111");
        }
    }
}
