using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Routing.Patterns;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing.Template
{
    public class TemplateSegmentBenchmark
    {
        private RoutePatternPathSegment _segment;
        private TemplateSegment _templateSegment;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _segment = RoutePatternFactory.Segment(new List<RoutePatternPart>
                {
                    new RoutePatternSeparatorPart("a"),
                    new RoutePatternSeparatorPart("c"),
                    new RoutePatternSeparatorPart("e"),
                    new RoutePatternSeparatorPart("f"),
                    new RoutePatternSeparatorPart("g"),
                });
            _templateSegment = new TemplateSegment(_segment);
        }

        [Benchmark]
        public void Ctor_RoutePatternPathSegment()
        {
            new TemplateSegment(_segment);
        }

        [Benchmark]
        public void ToRoutePatternPathSegment()
        {
            _templateSegment.ToRoutePatternPathSegment();
        }
    }
}
