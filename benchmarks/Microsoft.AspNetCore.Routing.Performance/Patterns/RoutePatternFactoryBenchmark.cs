using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Routing.Patterns;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Routing.Routing
{
    public class RoutePatternFactoryBenchmark
    {
        private IEnumerable<RoutePatternPart> _patterns;
        private RoutePatternPart[] _patternArray;
        private IEnumerable<RoutePatternParameterPolicyReference> _parameterPolicies;
        private RoutePatternParameterPolicyReference[] _parameterPoliciesArray;
        private IEnumerable<RoutePatternPathSegment> _segments;
        private RoutePatternPathSegment[] _segmentsArray;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var patternList = new List<RoutePatternPart>
                {
                    new RoutePatternSeparatorPart("a"),
                    new RoutePatternSeparatorPart("c"),
                    new RoutePatternSeparatorPart("e"),
                    new RoutePatternSeparatorPart("f"),
                    new RoutePatternSeparatorPart("g"),
                };
            _patterns = patternList;
            _patternArray = patternList.ToArray();

            _parameterPolicies = new List<RoutePatternParameterPolicyReference>
                {
                    new RoutePatternParameterPolicyReference(@"regex(^\d{3}-\d{3}-\d{4}$)"),
                    new RoutePatternParameterPolicyReference(@"regex(^\d{1,2}\/\d{1,2}\/\d{4}$)"),
                    new RoutePatternParameterPolicyReference(@"regex(^\w+\@\w+\.\w+)"),
                    new RoutePatternParameterPolicyReference(@"regex(([}])\w+)"),
                    new RoutePatternParameterPolicyReference(@"regex(([{(])\w+)"),
                };
            _parameterPoliciesArray = _parameterPolicies.ToArray();

            _segments = new List<RoutePatternPathSegment>
                {
                    new RoutePatternPathSegment(new RoutePatternPart[] {RoutePatternFactory.LiteralPart("x"), RoutePatternFactory.SeparatorPart("y")}),
                    new RoutePatternPathSegment(new RoutePatternPart[] {RoutePatternFactory.SeparatorPart("|"), RoutePatternFactory.LiteralPart("a")}),
                    new RoutePatternPathSegment(new RoutePatternPart[] {RoutePatternFactory.ParameterPart("id"), RoutePatternFactory.LiteralPart("z")}),
                };
            _segmentsArray = _segments.ToArray();
        }


        [Benchmark]
        public void Segment_IEnumerable()
        {
            RoutePatternFactory.Segment(_patterns);
        }

        [Benchmark]
        public void Segment_Array()
        {
            RoutePatternFactory.Segment(_patternArray);
        }

        [Benchmark]
        public void ParameterPart_Name()
        {
            RoutePatternFactory.ParameterPart("id");
        }

        [Benchmark]
        public void ParameterPart_NameAndDefault()
        {
            RoutePatternFactory.ParameterPart("id", 5);
        }

        [Benchmark]
        public void ParameterPart_NameAndDefaultAndKind()
        {
            RoutePatternFactory.ParameterPart("id", 5, RoutePatternParameterKind.CatchAll);
        }

        [Benchmark]
        public void ParameterPart_NameAndDefaultAndKindPolicies_IEnumerable()
        {
            RoutePatternFactory.ParameterPart("id", 5, RoutePatternParameterKind.Standard, _parameterPolicies);
        }

        [Benchmark]
        public void ParameterPart_NameAndDefaultAndKindPolicies_Array()
        {
            RoutePatternFactory.ParameterPart("id", 5, RoutePatternParameterKind.Standard, _parameterPoliciesArray);
        }

        [Benchmark]
        public void Pattern_Segments_Array()
        {
            RoutePatternFactory.Pattern(_segmentsArray);
        }

        [Benchmark]
        public void Pattern_Segments_IEnumerable()
        {
            RoutePatternFactory.Pattern(_segments);
        }
    }
}
