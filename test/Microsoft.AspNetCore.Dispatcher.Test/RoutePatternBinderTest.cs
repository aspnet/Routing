// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Testing.xunit;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.WebEncoders.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Dispatcher
{
    public class RoutePatternBinderTest
    {
        public RoutePatternBinderTest()
        {
            BinderFactory = new RoutePatternBinderFactory(new UrlTestEncoder(), new DefaultObjectPoolProvider());
        }

        public RoutePatternBinderFactory BinderFactory { get; }

        public static TheoryData EmptyAndNullDefaultValues =>
            new TheoryData<string, DispatcherValueCollection, DispatcherValueCollection, string>
            {
                {
                    "Test/{val1}/{val2}",
                    new DispatcherValueCollection(new {val1 = "", val2 = ""}),
                    new DispatcherValueCollection(new {val2 = "SomeVal2"}),
                    null
                },
                {
                    "Test/{val1}/{val2}",
                    new DispatcherValueCollection(new {val1 = "", val2 = ""}),
                    new DispatcherValueCollection(new {val1 = "a"}),
                    "/UrlEncode[[Test]]/UrlEncode[[a]]"
                },
                {
                    "Test/{val1}/{val2}/{val3}",
                    new DispatcherValueCollection(new {val1 = "", val3 = ""}),
                    new DispatcherValueCollection(new {val2 = "a"}),
                    null
                },
                {
                    "Test/{val1}/{val2}",
                    new DispatcherValueCollection(new {val1 = "", val2 = ""}),
                    new DispatcherValueCollection(new {val1 = "a", val2 = "b"}),
                    "/UrlEncode[[Test]]/UrlEncode[[a]]/UrlEncode[[b]]"
                },
                {
                    "Test/{val1}/{val2}/{val3}",
                    new DispatcherValueCollection(new {val1 = "", val2 = "", val3 = ""}),
                    new DispatcherValueCollection(new {val1 = "a", val2 = "b", val3 = "c"}),
                    "/UrlEncode[[Test]]/UrlEncode[[a]]/UrlEncode[[b]]/UrlEncode[[c]]"
                },
                {
                    "Test/{val1}/{val2}/{val3}",
                    new DispatcherValueCollection(new {val1 = "", val2 = "", val3 = ""}),
                    new DispatcherValueCollection(new {val1 = "a", val2 = "b"}),
                    "/UrlEncode[[Test]]/UrlEncode[[a]]/UrlEncode[[b]]"
                },
                {
                    "Test/{val1}/{val2}/{val3}",
                    new DispatcherValueCollection(new {val1 = "", val2 = "", val3 = ""}),
                    new DispatcherValueCollection(new {val1 = "a"}),
                    "/UrlEncode[[Test]]/UrlEncode[[a]]"
                },
                {
                    "Test/{val1}",
                    new DispatcherValueCollection(new {val1 = "42", val2 = "", val3 = ""}),
                    new DispatcherValueCollection(),
                    "/UrlEncode[[Test]]"
                },
                {
                    "Test/{val1}/{val2}/{val3}",
                    new DispatcherValueCollection(new {val1 = "42", val2 = (string)null, val3 = (string)null}),
                    new DispatcherValueCollection(),
                    "/UrlEncode[[Test]]"
                },
                {
                    "Test/{val1}/{val2}/{val3}/{val4}",
                    new DispatcherValueCollection(new {val1 = "21", val2 = "", val3 = "", val4 = ""}),
                    new DispatcherValueCollection(new {val1 = "42", val2 = "11", val3 = "", val4 = ""}),
                    "/UrlEncode[[Test]]/UrlEncode[[42]]/UrlEncode[[11]]"
                },
                {
                    "Test/{val1}/{val2}/{val3}",
                    new DispatcherValueCollection(new {val1 = "21", val2 = "", val3 = ""}),
                    new DispatcherValueCollection(new {val1 = "42"}),
                    "/UrlEncode[[Test]]/UrlEncode[[42]]"
                },
                {
                    "Test/{val1}/{val2}/{val3}/{val4}",
                    new DispatcherValueCollection(new {val1 = "21", val2 = "", val3 = "", val4 = ""}),
                    new DispatcherValueCollection(new {val1 = "42", val2 = "11"}),
                    "/UrlEncode[[Test]]/UrlEncode[[42]]/UrlEncode[[11]]"
                },
                {
                    "Test/{val1}/{val2}/{val3}",
                    new DispatcherValueCollection(new {val1 = "21", val2 = (string)null, val3 = (string)null}),
                    new DispatcherValueCollection(new {val1 = "42"}),
                    "/UrlEncode[[Test]]/UrlEncode[[42]]"
                },
                {
                    "Test/{val1}/{val2}/{val3}/{val4}",
                    new DispatcherValueCollection(new {val1 = "21", val2 = (string)null, val3 = (string)null, val4 = (string)null}),
                    new DispatcherValueCollection(new {val1 = "42", val2 = "11"}),
                    "/UrlEncode[[Test]]/UrlEncode[[42]]/UrlEncode[[11]]"
                },
            };

        [ConditionalTheory]
        [MemberData(nameof(EmptyAndNullDefaultValues))]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void Binding_WithEmptyAndNull_DefaultValues(
            string pattern,
            DispatcherValueCollection defaults,
            DispatcherValueCollection values,
            string expected)
        {
            // Arrange
            var binder = BinderFactory.Create(pattern, defaults);

            // Act & Assert
            (var acceptedValues, var combinedValues) = binder.GetValues(ambientValues: null, values: values);
            if (acceptedValues == null)
            {
                if (expected == null)
                {
                    return;
                }
                else
                {
                    Assert.NotNull(acceptedValues);
                }
            }

            var result = binder.BindValues(acceptedValues);
            if (expected == null)
            {
                Assert.Null(result);
            }
            else
            {
                Assert.NotNull(result);
                Assert.Equal(expected, result);
            }
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithMultiSegmentParamsOnBothEndsMatches()
        {
            RunTest(
                "language/{lang}-{region}",
                null,
                new DispatcherValueCollection(new { lang = "en", region = "US" }),
                new DispatcherValueCollection(new { lang = "xx", region = "yy" }),
                "/UrlEncode[[language]]/UrlEncode[[xx]]UrlEncode[[-]]UrlEncode[[yy]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithMultiSegmentParamsOnLeftEndMatches()
        {
            RunTest(
                "language/{lang}-{region}a",
                null,
                new DispatcherValueCollection(new { lang = "en", region = "US" }),
                new DispatcherValueCollection(new { lang = "xx", region = "yy" }),
                "/UrlEncode[[language]]/UrlEncode[[xx]]UrlEncode[[-]]UrlEncode[[yy]]UrlEncode[[a]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithMultiSegmentParamsOnRightEndMatches()
        {
            RunTest(
                "language/a{lang}-{region}",
                null,
                new DispatcherValueCollection(new { lang = "en", region = "US" }),
                new DispatcherValueCollection(new { lang = "xx", region = "yy" }),
                "/UrlEncode[[language]]/UrlEncode[[a]]UrlEncode[[xx]]UrlEncode[[-]]UrlEncode[[yy]]");
        }

        public static TheoryData OptionalParamValues =>
            new TheoryData<string, DispatcherValueCollection, DispatcherValueCollection, DispatcherValueCollection, string>
            {
                // defaults
                // ambient values
                // values
                {
                    "Test/{val1}/{val2}.{val3?}",
                    new DispatcherValueCollection(new {val1 = "someval1", val2 = "someval2"}),
                    new DispatcherValueCollection(new {val3 = "someval3"}),
                    new DispatcherValueCollection(new {val3 = "someval3"}),
                    "/UrlEncode[[Test]]/UrlEncode[[someval1]]/UrlEncode[[someval2]]UrlEncode[[.]]UrlEncode[[someval3]]"
                },
                {
                    "Test/{val1}/{val2}.{val3?}",
                    new DispatcherValueCollection(new {val1 = "someval1", val2 = "someval2"}),
                    new DispatcherValueCollection(new {val3 = "someval3a"}),
                    new DispatcherValueCollection(new {val3 = "someval3v"}),
                    "/UrlEncode[[Test]]/UrlEncode[[someval1]]/UrlEncode[[someval2]]UrlEncode[[.]]UrlEncode[[someval3v]]"
                },
                {
                    "Test/{val1}/{val2}.{val3?}",
                    new DispatcherValueCollection(new {val1 = "someval1", val2 = "someval2"}),
                    new DispatcherValueCollection(new {val3 = "someval3a"}),
                    new DispatcherValueCollection(),
                    "/UrlEncode[[Test]]/UrlEncode[[someval1]]/UrlEncode[[someval2]]UrlEncode[[.]]UrlEncode[[someval3a]]"
                },
                {
                    "Test/{val1}/{val2}.{val3?}",
                    new DispatcherValueCollection(new {val1 = "someval1", val2 = "someval2"}),
                    new DispatcherValueCollection(),
                    new DispatcherValueCollection(new {val3 = "someval3v"}),
                    "/UrlEncode[[Test]]/UrlEncode[[someval1]]/UrlEncode[[someval2]]UrlEncode[[.]]UrlEncode[[someval3v]]"
                },
                {
                    "Test/{val1}/{val2}.{val3?}",
                    new DispatcherValueCollection(new {val1 = "someval1", val2 = "someval2"}),
                    new DispatcherValueCollection(),
                    new DispatcherValueCollection(),
                    "/UrlEncode[[Test]]/UrlEncode[[someval1]]/UrlEncode[[someval2]]"
                },
                {
                    "Test/{val1}.{val2}.{val3}.{val4?}",
                    new DispatcherValueCollection(new {val1 = "someval1", val2 = "someval2" }),
                    new DispatcherValueCollection(),
                    new DispatcherValueCollection(new {val4 = "someval4", val3 = "someval3" }),
                    "/UrlEncode[[Test]]/UrlEncode[[someval1]]UrlEncode[[.]]UrlEncode[[someval2]]UrlEncode[[.]]"
                    + "UrlEncode[[someval3]]UrlEncode[[.]]UrlEncode[[someval4]]"
                },
                {
                    "Test/{val1}.{val2}.{val3}.{val4?}",
                    new DispatcherValueCollection(new {val1 = "someval1", val2 = "someval2" }),
                    new DispatcherValueCollection(),
                    new DispatcherValueCollection(new {val3 = "someval3" }),
                    "/UrlEncode[[Test]]/UrlEncode[[someval1]]UrlEncode[[.]]UrlEncode[[someval2]]UrlEncode[[.]]"
                    + "UrlEncode[[someval3]]"
                },
                {
                    "Test/.{val2?}",
                    new DispatcherValueCollection(new { }),
                    new DispatcherValueCollection(),
                    new DispatcherValueCollection(new {val2 = "someval2" }),
                    "/UrlEncode[[Test]]/UrlEncode[[.]]UrlEncode[[someval2]]"
                },
                {
                    "Test/{val1}.{val2}",
                    new DispatcherValueCollection(new {val1 = "someval1", val2 = "someval2" }),
                    new DispatcherValueCollection(),
                    new DispatcherValueCollection(new {val3 = "someval3" }),
                    "/UrlEncode[[Test]]/UrlEncode[[someval1]]UrlEncode[[.]]UrlEncode[[someval2]]?" +
                    "UrlEncode[[val3]]=UrlEncode[[someval3]]"
                },
            };

        [ConditionalTheory]
        [MemberData(nameof(OptionalParamValues))]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithMultiSegmentWithOptionalParam(
            string pattern,
            DispatcherValueCollection defaults,
            DispatcherValueCollection ambientValues,
            DispatcherValueCollection values,
            string expected)
        {
            // Arrange
            var binder = BinderFactory.Create(pattern, defaults);

            // Act & Assert
            (var acceptedValues, var combinedValues) = binder.GetValues(ambientValues: ambientValues, values: values);
            if (acceptedValues == null)
            {
                if (expected == null)
                {
                    return;
                }
                else
                {
                    Assert.NotNull(acceptedValues);
                }
            }

            var result = binder.BindValues(acceptedValues);
            if (expected == null)
            {
                Assert.Null(result);
            }
            else
            {
                Assert.NotNull(result);
                Assert.Equal(expected, result);
            }
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithMultiSegmentParamsOnNeitherEndMatches()
        {
            RunTest(
                "language/a{lang}-{region}a",
                null,
                new DispatcherValueCollection(new { lang = "en", region = "US" }),
                new DispatcherValueCollection(new { lang = "xx", region = "yy" }),
                "/UrlEncode[[language]]/UrlEncode[[a]]UrlEncode[[xx]]UrlEncode[[-]]UrlEncode[[yy]]UrlEncode[[a]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithMultiSegmentParamsOnNeitherEndDoesNotMatch()
        {
            RunTest(
                "language/a{lang}-{region}a",
                null,
                new DispatcherValueCollection(new { lang = "en", region = "US" }),
                new DispatcherValueCollection(new { lang = "", region = "yy" }),
                null);
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithMultiSegmentParamsOnNeitherEndDoesNotMatch2()
        {
            RunTest(
                "language/a{lang}-{region}a",
                null,
                new DispatcherValueCollection(new { lang = "en", region = "US" }),
                new DispatcherValueCollection(new { lang = "xx", region = "" }),
                null);
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithSimpleMultiSegmentParamsOnBothEndsMatches()
        {
            RunTest(
                "language/{lang}",
                null,
                new DispatcherValueCollection(new { lang = "en" }),
                new DispatcherValueCollection(new { lang = "xx" }),
                "/UrlEncode[[language]]/UrlEncode[[xx]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithSimpleMultiSegmentParamsOnLeftEndMatches()
        {
            RunTest(
                "language/{lang}-",
                null,
                new DispatcherValueCollection(new { lang = "en" }),
                new DispatcherValueCollection(new { lang = "xx" }),
                "/UrlEncode[[language]]/UrlEncode[[xx]]UrlEncode[[-]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithSimpleMultiSegmentParamsOnRightEndMatches()
        {
            RunTest(
                "language/a{lang}",
                null,
                new DispatcherValueCollection(new { lang = "en" }),
                new DispatcherValueCollection(new { lang = "xx" }),
                "/UrlEncode[[language]]/UrlEncode[[a]]UrlEncode[[xx]]");
        }

        [Fact]
        public void GetVirtualPathWithSimpleMultiSegmentParamsOnNeitherEndMatches()
        {
            RunTest(
                "language/a{lang}a",
                null,
                new DispatcherValueCollection(new { lang = "en" }),
                new DispatcherValueCollection(new { lang = "xx" }),
                "/UrlEncode[[language]]/UrlEncode[[a]]UrlEncode[[xx]]UrlEncode[[a]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithMultiSegmentStandardMvcRouteMatches()
        {
            RunTest(
                "{controller}.mvc/{action}/{id}",
                new DispatcherValueCollection(new { action = "Index", id = (string)null }),
                new DispatcherValueCollection(new { controller = "home", action = "list", id = (string)null }),
                new DispatcherValueCollection(new { controller = "products" }),
                "/UrlEncode[[products]]UrlEncode[[.mvc]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithMultiSegmentParamsOnBothEndsWithDefaultValuesMatches()
        {
            RunTest(
                "language/{lang}-{region}",
                new DispatcherValueCollection(new { lang = "xx", region = "yy" }),
                new DispatcherValueCollection(new { lang = "en", region = "US" }),
                new DispatcherValueCollection(new { lang = "zz" }),
                "/UrlEncode[[language]]/UrlEncode[[zz]]UrlEncode[[-]]UrlEncode[[yy]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithDefaultValue()
        {
            // URL should be found but excluding the 'id' parameter, which has only a default value.
            RunTest(
               "{controller}/{action}/{id}",
               new DispatcherValueCollection(new { id = "defaultid" }),
               new DispatcherValueCollection(new { controller = "home", action = "oldaction" }),
               new DispatcherValueCollection(new { action = "newaction" }),
               "/UrlEncode[[home]]/UrlEncode[[newaction]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithEmptyStringRequiredValueReturnsNull()
        {
            RunTest(
                "foo/{controller}",
                null,
                new DispatcherValueCollection(new { }),
                new DispatcherValueCollection(new { controller = "" }),
                null);
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithNullRequiredValueReturnsNull()
        {
            RunTest(
                "foo/{controller}",
                null,
                new DispatcherValueCollection(new { }),
                new DispatcherValueCollection(new { controller = (string)null }),
                null);
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithRequiredValueReturnsPath()
        {
            RunTest(
                "foo/{controller}",
                null,
                new DispatcherValueCollection(new { }),
                new DispatcherValueCollection(new { controller = "home" }),
                "/UrlEncode[[foo]]/UrlEncode[[home]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithNullDefaultValue()
        {
            // URL should be found but excluding the 'id' parameter, which has only a default value.
            RunTest(
                "{controller}/{action}/{id}",
                new DispatcherValueCollection(new { id = (string)null }),
                new DispatcherValueCollection(new { controller = "home", action = "oldaction", id = (string)null }),
                new DispatcherValueCollection(new { action = "newaction" }),
                "/UrlEncode[[home]]/UrlEncode[[newaction]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathCanFillInSeparatedParametersWithDefaultValues()
        {
            RunTest(
                "{controller}/{language}-{locale}",
                new DispatcherValueCollection(new { language = "en", locale = "US" }),
                new DispatcherValueCollection(),
                new DispatcherValueCollection(new { controller = "Orders" }),
                "/UrlEncode[[Orders]]/UrlEncode[[en]]UrlEncode[[-]]UrlEncode[[US]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathWithUnusedNullValueShouldGenerateUrlAndIgnoreNullValue()
        {
            RunTest(
                "{controller}.mvc/{action}/{id}",
                new DispatcherValueCollection(new { action = "Index", id = "" }),
                new DispatcherValueCollection(new { controller = "Home", action = "Index", id = "" }),
                new DispatcherValueCollection(new { controller = "Home", action = "TestAction", id = "1", format = (string)null }),
                "/UrlEncode[[Home]]UrlEncode[[.mvc]]/UrlEncode[[TestAction]]/UrlEncode[[1]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithMissingValuesDoesntMatch()
        {
            RunTest(
                "{controller}/{action}/{id}",
                null,
                new { controller = "home", action = "oldaction" },
                new { action = "newaction" },
                null);
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithEmptyRequiredValuesReturnsNull()
        {
            RunTest(
                "{p1}/{p2}/{p3}",
                null,
                new { p1 = "v1", },
                new { p2 = "", p3 = "" },
                null);
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithEmptyOptionalValuesReturnsShortUrl()
        {
            RunTest(
                "{p1}/{p2}/{p3}",
                new { p2 = "d2", p3 = "d3" },
                new { p1 = "v1", },
                new { p2 = "", p3 = "" },
                "/UrlEncode[[v1]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlShouldIgnoreValuesAfterChangedParameter()
        {
            RunTest(
                "{controller}/{action}/{id}",
                new { action = "Index", id = (string)null },
                new { controller = "orig", action = "init", id = "123" },
                new { action = "new", },
                "/UrlEncode[[orig]]/UrlEncode[[new]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithNullForMiddleParameterIgnoresRemainingParameters()
        {
            RunTest(
                "UrlGeneration1/{controller}.mvc/{action}/{category}/{year}/{occasion}/{SafeParam}",
                new { year = 1995, occasion = "Christmas", action = "Play", SafeParam = "SafeParamValue" },
                new { controller = "UrlRouting", action = "Play", category = "Photos", year = "2008", occasion = "Easter", SafeParam = "SafeParamValue" },
                new { year = (string)null, occasion = "Hola" },
                "/UrlEncode[[UrlGeneration1]]/UrlEncode[[UrlRouting]]UrlEncode[[.mvc]]/UrlEncode[[Play]]/"
                + "UrlEncode[[Photos]]/UrlEncode[[1995]]/UrlEncode[[Hola]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithEmptyStringForMiddleParameterIgnoresRemainingParameters()
        {
            var ambientValues = new DispatcherValueCollection();
            ambientValues.Add("controller", "UrlRouting");
            ambientValues.Add("action", "Play");
            ambientValues.Add("category", "Photos");
            ambientValues.Add("year", "2008");
            ambientValues.Add("occasion", "Easter");
            ambientValues.Add("SafeParam", "SafeParamValue");

            var values = new DispatcherValueCollection();
            values.Add("year", String.Empty);
            values.Add("occasion", "Hola");

            RunTest(
                "UrlGeneration1/{controller}.mvc/{action}/{category}/{year}/{occasion}/{SafeParam}",
                new DispatcherValueCollection(new { year = 1995, occasion = "Christmas", action = "Play", SafeParam = "SafeParamValue" }),
                ambientValues,
                values,
                "/UrlEncode[[UrlGeneration1]]/UrlEncode[[UrlRouting]]UrlEncode[[.mvc]]/"
                + "UrlEncode[[Play]]/UrlEncode[[Photos]]/UrlEncode[[1995]]/UrlEncode[[Hola]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithEmptyStringForMiddleParameterShouldUseDefaultValue()
        {
            var ambientValues = new DispatcherValueCollection();
            ambientValues.Add("Controller", "Test");
            ambientValues.Add("Action", "Fallback");
            ambientValues.Add("param1", "fallback1");
            ambientValues.Add("param2", "fallback2");
            ambientValues.Add("param3", "fallback3");

            var values = new DispatcherValueCollection();
            values.Add("controller", "subtest");
            values.Add("param1", "b");

            RunTest(
                "{controller}.mvc/{action}/{param1}",
                new DispatcherValueCollection(new { action = "Default" }),
                ambientValues,
                values,
                "/UrlEncode[[subtest]]UrlEncode[[.mvc]]/UrlEncode[[Default]]/UrlEncode[[b]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlVerifyEncoding()
        {
            var values = new DispatcherValueCollection();
            values.Add("controller", "#;?:@&=+$,");
            values.Add("action", "showcategory");
            values.Add("id", 123);
            values.Add("so?rt", "de?sc");
            values.Add("maxPrice", 100);

            RunTest(
                "{controller}.mvc/{action}/{id}",
                new DispatcherValueCollection(new { controller = "Home" }),
                new DispatcherValueCollection(new { controller = "home", action = "Index", id = (string)null }),
                values,
                "/%23;%3F%3A@%26%3D%2B$,.mvc/showcategory/123?so%3Frt=de%3Fsc&maxPrice=100",
                UrlEncoder.Default);
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlGeneratesQueryStringForNewValuesAndEscapesQueryString()
        {
            var values = new DispatcherValueCollection(new { controller = "products", action = "showcategory", id = 123, maxPrice = 100 });
            values.Add("so?rt", "de?sc");

            RunTest(
                "{controller}.mvc/{action}/{id}",
                new DispatcherValueCollection(new { controller = "Home" }),
                new DispatcherValueCollection(new { controller = "home", action = "Index", id = (string)null }),
                values,
               "/UrlEncode[[products]]UrlEncode[[.mvc]]/UrlEncode[[showcategory]]/UrlEncode[[123]]" +
               "?UrlEncode[[so?rt]]=UrlEncode[[de?sc]]&UrlEncode[[maxPrice]]=UrlEncode[[100]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlGeneratesQueryStringForNewValuesButIgnoresNewValuesThatMatchDefaults()
        {
            RunTest(
                "{controller}.mvc/{action}/{id}",
                new DispatcherValueCollection(new { controller = "Home", Custom = "customValue" }),
                new DispatcherValueCollection(new { controller = "Home", action = "Index", id = (string)null }),
                new DispatcherValueCollection(
                    new
                    {
                        controller = "products",
                        action = "showcategory",
                        id = 123,
                        sort = "desc",
                        maxPrice = 100,
                        custom = "customValue"
                    }),
                "/UrlEncode[[products]]UrlEncode[[.mvc]]/UrlEncode[[showcategory]]/UrlEncode[[123]]" +
                "?UrlEncode[[sort]]=UrlEncode[[desc]]&UrlEncode[[maxPrice]]=UrlEncode[[100]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualPathEncodesParametersAndLiterals()
        {
            RunTest(
                "bl%og/{controller}/he llo/{action}",
                null,
                new DispatcherValueCollection(new { controller = "ho%me", action = "li st" }),
                new DispatcherValueCollection(),
                "/bl%25og/ho%25me/he%20llo/li%20st",
                UrlEncoder.Default);
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetVirtualDoesNotEncodeLeadingSlashes()
        {
            RunTest(
                "{controller}/{action}",
                null,
                new DispatcherValueCollection(new { controller = "/home", action = "/my/index" }),
                new DispatcherValueCollection(),
                "/home/%2Fmy%2Findex",
                UrlEncoder.Default);
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithLeadingTildeSlash()
        {
            RunTest(
                "~/foo",
                null,
                null,
                new DispatcherValueCollection(new { }),
                "/UrlEncode[[foo]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithLeadingSlash()
        {
            RunTest(
                "/foo",
                null,
                null,
                new DispatcherValueCollection(new { }),
                "/UrlEncode[[foo]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithCatchAllWithValue()
        {
            RunTest(
                "{p1}/{*p2}",
                new DispatcherValueCollection(new { id = "defaultid" }),
                new DispatcherValueCollection(new { p1 = "v1" }),
                new DispatcherValueCollection(new { p2 = "v2a/v2b" }),
                "/UrlEncode[[v1]]/UrlEncode[[v2a/v2b]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithCatchAllWithEmptyValue()
        {
            RunTest(
                "{p1}/{*p2}",
                new DispatcherValueCollection(new { id = "defaultid" }),
                new DispatcherValueCollection(new { p1 = "v1" }),
                new DispatcherValueCollection(new { p2 = "" }),
                "/UrlEncode[[v1]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void GetUrlWithCatchAllWithNullValue()
        {
            RunTest(
                "{p1}/{*p2}",
                new DispatcherValueCollection(new { id = "defaultid" }),
                new DispatcherValueCollection(new { p1 = "v1" }),
                new DispatcherValueCollection(new { p2 = (string)null }),
                "/UrlEncode[[v1]]");
        }

        [ConditionalFact]
        [FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "Fails due to dotnet/standard#567")]
        public void RoutePatternBinder_KeepsExplicitlySuppliedRouteValues_OnFailedRouetMatch()
        {
            // Arrange
            var pattern = "{area?}/{controller=Home}/{action=Index}/{id?}";
            var encoder = new UrlTestEncoder();
            var binder = BinderFactory.Create(pattern);
            var ambientValues = new DispatcherValueCollection();
            var routeValues = new DispatcherValueCollection(new { controller = "Test", action = "Index" });

            // Act
            var valuesResult = binder.GetValues(ambientValues, routeValues);
            var result = binder.BindValues(valuesResult.acceptedValues);

            // Assert
            Assert.Null(result);
            Assert.Equal(2, valuesResult.combinedValues.Count);
            object routeValue;
            Assert.True(valuesResult.combinedValues.TryGetValue("controller", out routeValue));
            Assert.Equal("Test", routeValue?.ToString());
            Assert.True(valuesResult.combinedValues.TryGetValue("action", out routeValue));
            Assert.Equal("Index", routeValue?.ToString());
        }

#if ROUTE_COLLECTION

                [Fact]
        public void GetUrlShouldValidateOnlyAcceptedParametersAndUserDefaultValuesForInvalidatedParameters()
        {
            // Arrange
            var rd = CreateRouteData();
            rd.Values.Add("Controller", "UrlRouting");
            rd.Values.Add("Name", "MissmatchedValidateParams");
            rd.Values.Add("action", "MissmatchedValidateParameters2");
            rd.Values.Add("ValidateParam1", "special1");
            rd.Values.Add("ValidateParam2", "special2");

            IRouteCollection rc = new DefaultRouteCollection();
            rc.Add(CreateRoute(
                "UrlConstraints/Validation.mvc/Input5/{action}/{ValidateParam1}/{ValidateParam2}",
                new DispatcherValueCollection(new { Controller = "UrlRouting", Name = "MissmatchedValidateParams", ValidateParam2 = "valid" }),
                new DispatcherValueCollection(new { ValidateParam1 = "valid.*", ValidateParam2 = "valid.*" })));

            rc.Add(CreateRoute(
                "UrlConstraints/Validation.mvc/Input5/{action}/{ValidateParam1}/{ValidateParam2}",
                new DispatcherValueCollection(new { Controller = "UrlRouting", Name = "MissmatchedValidateParams" }),
                new DispatcherValueCollection(new { ValidateParam1 = "special.*", ValidateParam2 = "special.*" })));

            var values = CreateDispatcherValueCollection();
            values.Add("Name", "MissmatchedValidateParams");
            values.Add("ValidateParam1", "valid1");

            // Act
            var vpd = rc.GetVirtualPath(GetHttpContext("/app1", "", ""), values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal<string>("/app1/UrlConstraints/Validation.mvc/Input5/MissmatchedValidateParameters2/valid1", vpd.VirtualPath);
        }

               [Fact]
        public void GetUrlWithRouteThatHasExtensionWithSubsequentDefaultValueIncludesExtensionButNotDefaultValue()
        {
            // Arrange
            var rd = CreateRouteData();
            rd.Values.Add("controller", "Bank");
            rd.Values.Add("action", "MakeDeposit");
            rd.Values.Add("accountId", "7770");

            IRouteCollection rc = new DefaultRouteCollection();
            rc.Add(CreateRoute(
                "{controller}.mvc/Deposit/{accountId}",
                new DispatcherValueCollection(new { Action = "DepositView" })));

            // Note: This route was in the original bug, but it turns out that this behavior is incorrect. With the
            // recent fix to Route (in this changelist) this route would have been selected since we have values for
            // all three required parameters.
            //rc.Add(new Route {
            //    Url = "{controller}.mvc/{action}/{accountId}",
            //    RouteHandler = new DummyRouteHandler()
            //});

            // This route should be chosen because the requested action is List. Since the default value of the action
            // is List then the Action should not be in the URL. However, the file extension should be included since
            // it is considered "safe."
            rc.Add(CreateRoute(
                "{controller}.mvc/{action}",
                new DispatcherValueCollection(new { Action = "List" })));

            var values = CreateDispatcherValueCollection();
            values.Add("Action", "List");

            // Act
            var vpd = rc.GetVirtualPath(GetHttpContext("/app1", "", ""), values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal<string>("/app1/Bank.mvc", vpd.VirtualPath);
        }

        [Fact]
        public void GetUrlWithRouteThatHasDifferentControllerCaseShouldStillMatch()
        {
            // Arrange
            var rd = CreateRouteData();
            rd.Values.Add("controller", "Bar");
            rd.Values.Add("action", "bbb");
            rd.Values.Add("id", null);

            IRouteCollection rc = new DefaultRouteCollection();
            rc.Add(CreateRoute("PrettyFooUrl", new DispatcherValueCollection(new { controller = "Foo", action = "aaa", id = (string)null })));

            rc.Add(CreateRoute("PrettyBarUrl", new DispatcherValueCollection(new { controller = "Bar", action = "bbb", id = (string)null })));

            rc.Add(CreateRoute("{controller}/{action}/{id}", new DispatcherValueCollection(new { action = "Index", id = (string)null })));

            var values = CreateDispatcherValueCollection();
            values.Add("Action", "aaa");
            values.Add("Controller", "foo");

            // Act
            var vpd = rc.GetVirtualPath(GetHttpContext("/app1", "", ""), values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal<string>("/app1/PrettyFooUrl", vpd.VirtualPath);
        }

        [Fact]
        public void GetUrlWithNoChangedValuesShouldProduceSameUrl()
        {
            // Arrange
            var rd = CreateRouteData();
            rd.Values.Add("controller", "Home");
            rd.Values.Add("action", "Index");
            rd.Values.Add("id", null);

            IRouteCollection rc = new DefaultRouteCollection();
            rc.Add(CreateRoute("{controller}.mvc/{action}/{id}", new DispatcherValueCollection(new { action = "Index", id = (string)null })));

            rc.Add(CreateRoute("{controller}/{action}/{id}", new DispatcherValueCollection(new { action = "Index", id = (string)null })));

            var values = CreateDispatcherValueCollection();
            values.Add("Action", "Index");

            // Act
            var vpd = rc.GetVirtualPath(GetHttpContext("/app1", "", ""), values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal<string>("/app1/Home.mvc", vpd.VirtualPath);
        }

        [Fact]
        public void GetUrlAppliesConstraintsRulesToChooseRoute()
        {
            // Arrange
            var rd = CreateRouteData();
            rd.Values.Add("controller", "Home");
            rd.Values.Add("action", "Index");
            rd.Values.Add("id", null);

            IRouteCollection rc = new DefaultRouteCollection();
            rc.Add(CreateRoute(
                "foo.mvc/{action}",
                new DispatcherValueCollection(new { controller = "Home" }),
                new DispatcherValueCollection(new { controller = "Home", action = "Contact", httpMethod = CreateHttpMethodConstraint("get") })));

            rc.Add(CreateRoute(
                "{controller}.mvc/{action}",
                new DispatcherValueCollection(new { action = "Index" }),
                new DispatcherValueCollection(new { controller = "Home", action = "(Index|About)", httpMethod = CreateHttpMethodConstraint("post") })));

            var values = CreateDispatcherValueCollection();
            values.Add("Action", "Index");

            // Act
            var vpd = rc.GetVirtualPath(GetHttpContext("/app1", "", ""), values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal<string>("/app1/Home.mvc", vpd.VirtualPath);
        }

        [Fact]
        public void GetUrlWithValuesThatAreCompletelyDifferentFromTheCurrentRoute()
        {
            // Arrange
            HttpContext context = GetHttpContext("/app", null, null);
            IRouteCollection rt = new DefaultRouteCollection();
            rt.Add(CreateRoute("date/{y}/{m}/{d}", null));
            rt.Add(CreateRoute("{controller}/{action}/{id}", null));

            var rd = CreateRouteData();
            rd.Values.Add("controller", "home");
            rd.Values.Add("action", "dostuff");

            var values = CreateDispatcherValueCollection();
            values.Add("y", "2007");
            values.Add("m", "08");
            values.Add("d", "12");

            // Act
            var vpd = rt.GetVirtualPath(context, values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal<string>("/app/date/2007/08/12", vpd.VirtualPath);
        }

        [Fact]
        public void GetUrlWithValuesThatAreCompletelyDifferentFromTheCurrentRouteAsSecondRoute()
        {
            // Arrange
            HttpContext context = GetHttpContext("/app", null, null);

            IRouteCollection rt = new DefaultRouteCollection();
            rt.Add(CreateRoute("{controller}/{action}/{id}"));
            rt.Add(CreateRoute("date/{y}/{m}/{d}"));

            var rd = CreateRouteData();
            rd.Values.Add("controller", "home");
            rd.Values.Add("action", "dostuff");

            var values = CreateDispatcherValueCollection();
            values.Add("y", "2007");
            values.Add("m", "08");
            values.Add("d", "12");

            // Act
            var vpd = rt.GetVirtualPath(context, values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal<string>("/app/date/2007/08/12", vpd.VirtualPath);
        }

        [Fact]
        public void GetVirtualPathUsesCurrentValuesNotInRouteToMatch()
        {
            // Arrange
            HttpContext context = GetHttpContext("/app", null, null);
            TemplateRoute r1 = CreateRoute(
                "ParameterMatching.mvc/{Action}/{product}",
                new DispatcherValueCollection(new { Controller = "ParameterMatching", product = (string)null }),
                null);

            TemplateRoute r2 = CreateRoute(
                "{controller}.mvc/{action}",
                new DispatcherValueCollection(new { Action = "List" }),
                new DispatcherValueCollection(new { Controller = "Action|Bank|Overridden|DerivedFromAction|OverrideInvokeActionAndExecute|InvalidControllerName|Store|HtmlHelpers|(T|t)est|UrlHelpers|Custom|Parent|Child|TempData|ViewFactory|LocatingViews|AccessingDataInViews|ViewOverrides|ViewMasterPage|InlineCompileError|CustomView" }),
                null);

            var rd = CreateRouteData();
            rd.Values.Add("controller", "Bank");
            rd.Values.Add("Action", "List");
            var valuesDictionary = CreateDispatcherValueCollection();
            valuesDictionary.Add("action", "AttemptLogin");

            // Act for first route
            var vpd = r1.GetVirtualPath(context, valuesDictionary);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal<string>("ParameterMatching.mvc/AttemptLogin", vpd.VirtualPath);

            // Act for second route
            vpd = r2.GetVirtualPath(context, valuesDictionary);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal<string>("Bank.mvc/AttemptLogin", vpd.VirtualPath);
        }

#endif

#if DATA_TOKENS
        [Fact]
        public void GetVirtualPathWithDataTokensCopiesThemFromRouteToVirtualPathData()
        {
            // Arrange
            HttpContext context = GetHttpContext("/app", null, null);
            TemplateRoute r = CreateRoute("{controller}/{action}", null, null, new DispatcherValueCollection(new { foo = "bar", qux = "quux" }));

            var rd = CreateRouteData();
            rd.Values.Add("controller", "home");
            rd.Values.Add("action", "index");
            var valuesDictionary = CreateDispatcherValueCollection();

            // Act
            var vpd = r.GetVirtualPath(context, valuesDictionary);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal<string>("home/index", vpd.VirtualPath);
            Assert.Equal(r, vpd.Route);
            Assert.Equal<int>(2, vpd.DataTokens.Count);
            Assert.Equal("bar", vpd.DataTokens["foo"]);
            Assert.Equal("quux", vpd.DataTokens["qux"]);
        }
#endif

#if ROUTE_FORMAT_HELPER

        [Fact]
        public void UrlWithEscapedOpenCloseBraces()
        {
            RouteFormatHelper("foo/{{p1}}", "foo/{p1}");
        }

        [Fact]
        public void UrlWithEscapedOpenBraceAtTheEnd()
        {
            RouteFormatHelper("bar{{", "bar{");
        }

        [Fact]
        public void UrlWithEscapedOpenBraceAtTheBeginning()
        {
            RouteFormatHelper("{{bar", "{bar");
        }

        [Fact]
        public void UrlWithRepeatedEscapedOpenBrace()
        {
            RouteFormatHelper("foo{{{{bar", "foo{{bar");
        }

        [Fact]
        public void UrlWithEscapedCloseBraceAtTheEnd()
        {
            RouteFormatHelper("bar}}", "bar}");
        }

        [Fact]
        public void UrlWithEscapedCloseBraceAtTheBeginning()
        {
            RouteFormatHelper("}}bar", "}bar");
        }

        [Fact]
        public void UrlWithRepeatedEscapedCloseBrace()
        {
            RouteFormatHelper("foo}}}}bar", "foo}}bar");
        }

        private static void RouteFormatHelper(string routeUrl, string requestUrl)
        {
            var defaults = new DispatcherValueCollection(new { route = "matched" });
            var r = CreateRoute(routeUrl, defaults, null);

            GetRouteDataHelper(r, requestUrl, defaults);
            GetVirtualPathHelper(r, new DispatcherValueCollection(), null, Uri.EscapeUriString(requestUrl));
        }

#endif

#if CONSTRAINTS
        [Fact]
        public void GetVirtualPathWithNonParameterConstraintReturnsUrlWithoutQueryString()
        {
            // DevDiv Bugs 199612: UrlRouting: UrlGeneration should not append parameter to query string if it is a Constraint parameter and not a Url parameter
            RunTest(
                "{Controller}.mvc/{action}/{end}",
                null,
                new DispatcherValueCollection(new { foo = CreateHttpMethodConstraint("GET") }),
                new DispatcherValueCollection(),
                new DispatcherValueCollection(new { controller = "Orders", action = "Index", end = "end", foo = "GET" }),
                "Orders.mvc/Index/end");
        }

        [Fact]
        public void GetVirtualPathWithValidCustomConstraints()
        {
            // Arrange
            HttpContext context = GetHttpContext("/app", null, null);
            CustomConstraintTemplateRoute r = new CustomConstraintTemplateRoute("{controller}/{action}", null, new DispatcherValueCollection(new { action = 5 }));

            var rd = CreateRouteData();
            rd.Values.Add("controller", "home");
            rd.Values.Add("action", "index");

            var valuesDictionary = CreateDispatcherValueCollection();

            // Act
            var vpd = r.GetVirtualPath(context, valuesDictionary);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal<string>("home/index", vpd.VirtualPath);
            Assert.Equal(r, vpd.Route);
            Assert.NotNull(r.ConstraintData);
            Assert.Equal(5, r.ConstraintData.Constraint);
            Assert.Equal("action", r.ConstraintData.ParameterName);
            Assert.Equal("index", r.ConstraintData.ParameterValue);
        }

        [Fact]
        public void GetVirtualPathWithInvalidCustomConstraints()
        {
            // Arrange
            HttpContext context = GetHttpContext("/app", null, null);
            CustomConstraintTemplateRoute r = new CustomConstraintTemplateRoute("{controller}/{action}", null, new DispatcherValueCollection(new { action = 5 }));

            var rd = CreateRouteData();
            rd.Values.Add("controller", "home");
            rd.Values.Add("action", "list");

            var valuesDictionary = CreateDispatcherValueCollection();

            // Act
            var vpd = r.GetVirtualPath(context, valuesDictionary);

            // Assert
            Assert.Null(vpd);
            Assert.NotNull(r.ConstraintData);
            Assert.Equal(5, r.ConstraintData.Constraint);
            Assert.Equal("action", r.ConstraintData.ParameterName);
            Assert.Equal("list", r.ConstraintData.ParameterValue);
        }

#endif

        [Theory]
        [InlineData(null, null, true)]
        [InlineData("blog", null, false)]
        [InlineData(null, "store", false)]
        [InlineData("Cool", "cool", true)]
        [InlineData("Co0l", "cool", false)]
        public void RoutePartsEqualTest(object left, object right, bool expected)
        {
            // Arrange & Act & Assert
            if (expected)
            {
                Assert.True(RoutePatternBinder.RoutePartsEqual(left, right));
            }
            else
            {
                Assert.False(RoutePatternBinder.RoutePartsEqual(left, right));
            }
        }

        private void RunTest(
            string pattern,
            object defaults,
            object ambientValues,
            object values,
            string expected)
        {
            RunTest(
                pattern,
                new DispatcherValueCollection(defaults),
                new DispatcherValueCollection(ambientValues),
                new DispatcherValueCollection(values),
                expected);
        }

        private void RunTest(
            string pattern,
            DispatcherValueCollection defaults,
            DispatcherValueCollection ambientValues,
            DispatcherValueCollection values,
            string expected,
            UrlEncoder encoder = null)
        {
            // Arrange
            var binderFactory = encoder == null ? BinderFactory : new RoutePatternBinderFactory(encoder, new DefaultObjectPoolProvider());
            var binder = binderFactory.Create(pattern, defaults ?? new DispatcherValueCollection());

            // Act & Assert
            (var acceptedValues, var combinedValues) = binder.GetValues(ambientValues, values);
            if (acceptedValues == null)
            {
                if (expected == null)
                {
                    return;
                }
                else
                {
                    Assert.NotNull(acceptedValues);
                }
            }

            var result = binder.BindValues(acceptedValues);
            if (expected == null)
            {
                Assert.Null(result);
            }
            else
            {
                Assert.NotNull(result);

                // We want to chop off the query string and compare that using an unordered comparison
                var expectedParts = new PathAndQuery(expected);
                var actualParts = new PathAndQuery(result);

                Assert.Equal(expectedParts.Path, actualParts.Path);

                if (expectedParts.Parameters == null)
                {
                    Assert.Null(actualParts.Parameters);
                }
                else
                {
                    Assert.Equal(expectedParts.Parameters.Count, actualParts.Parameters.Count);

                    foreach (var kvp in expectedParts.Parameters)
                    {
                        Assert.True(actualParts.Parameters.TryGetValue(kvp.Key, out var value));
                        Assert.Equal(kvp.Value, value);
                    }
                }
            }
        }

        private class PathAndQuery
        {
            public PathAndQuery(string uri)
            {
                var queryIndex = uri.IndexOf("?", StringComparison.Ordinal);
                if (queryIndex == -1)
                {
                    Path = uri;
                }
                else
                {
                    Path = uri.Substring(0, queryIndex);

                    var query = uri.Substring(queryIndex + 1);
                    Parameters =
                        query
                            .Split(new char[] { '&' }, StringSplitOptions.None)
                            .Select(s => s.Split(new char[] { '=' }, StringSplitOptions.None))
                            .ToDictionary(pair => pair[0], pair => pair[1]);
                }
            }

            public string Path { get; private set; }

            public Dictionary<string, string> Parameters { get; private set; }
        }
    }
}
