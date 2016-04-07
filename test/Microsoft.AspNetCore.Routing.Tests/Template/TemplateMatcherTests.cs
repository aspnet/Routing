// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Template.Tests
{
    public class TemplateMatcherTests
    {
        private static IInlineConstraintResolver _inlineConstraintResolver = GetInlineConstraintResolver();

        [Fact]
        public void MatchSingleRoute()
        {
            // Arrange
            var matcher = CreateMatcher("{controller}/{action}/{id}");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/Bank/DoAction/123", values);

            // Assert
            Assert.True(match);
            Assert.Equal("Bank", values["controller"]);
            Assert.Equal("DoAction", values["action"]);
            Assert.Equal("123", values["id"]);
        }

        [Fact]
        public void NoMatchSingleRoute()
        {
            // Arrange
            var matcher = CreateMatcher("{controller}/{action}/{id}");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/Bank/DoAction", values);

            // Assert
            Assert.False(match);
        }

        [Fact]
        public void MatchSingleRouteWithDefaults()
        {
            // Arrange
            var matcher = CreateMatcher("{controller}/{action}/{id}", new { id = "default id" });

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/Bank/DoAction", values);

            // Assert
            Assert.Equal("Bank", values["controller"]);
            Assert.Equal("DoAction", values["action"]);
            Assert.Equal("default id", values["id"]);
        }

        [Fact]
        public void NoMatchSingleRouteWithDefaults()
        {
            // Arrange
            var matcher = CreateMatcher("{controller}/{action}/{id}", new { id = "default id" });

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/Bank", values);

            // Assert
            Assert.False(match);
        }

        [Fact]
        public void MatchRouteWithLiterals()
        {
            // Arrange
            var matcher = CreateMatcher("moo/{p1}/bar/{p2}", new { p2 = "default p2" });

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/moo/111/bar/222", values);

            // Assert
            Assert.Equal("111", values["p1"]);
            Assert.Equal("222", values["p2"]);
        }

        [Fact]
        public void MatchRouteWithLiteralsAndDefaults()
        {
            // Arrange
            var matcher = CreateMatcher("moo/{p1}/bar/{p2}", new { p2 = "default p2" });

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/moo/111/bar/", values);

            // Assert
            Assert.Equal("111", values["p1"]);
            Assert.Equal("default p2", values["p2"]);
        }

        [Theory]
        [InlineData(@"{p1:regex(^\d{{3}}-\d{{3}}-\d{{4}}$)}", "/123-456-7890")] // ssn
        [InlineData(@"{p1:regex(^\w+\@\w+\.\w+)}", "/asd@assds.com")] // email
        [InlineData(@"{p1:regex(([}}])\w+)}", "/}sda")] // Not balanced }
        [InlineData(@"{p1:regex(([{{)])\w+)}", "/})sda")] // Not balanced {
        public void MatchRoute_RegularExpression_Valid(
            string template,
            string path)
        {
            // Arrange
            var matcher = CreateMatcher(template);

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch(path, values);

            // Assert
            Assert.True(match);
        }

        [Theory]
        [InlineData("moo/{p1}.{p2?}", "/moo/foo.bar", "foo", "bar")]
        [InlineData("moo/{p1?}", "/moo/foo", "foo", null)]
        [InlineData("moo/{p1?}", "/moo", null, null)]
        [InlineData("moo/{p1}.{p2?}", "/moo/foo", "foo", null)]
        [InlineData("moo/{p1}.{p2?}", "/moo/foo..bar", "foo.", "bar")]
        [InlineData("moo/{p1}.{p2?}", "/moo/foo.moo.bar", "foo.moo", "bar")]
        [InlineData("moo/{p1}.{p2}", "/moo/foo.bar", "foo", "bar")]
        [InlineData("moo/foo.{p1}.{p2?}", "/moo/foo.moo.bar", "moo", "bar")]
        [InlineData("moo/foo.{p1}.{p2?}", "/moo/foo.moo", "moo", null)]
        [InlineData("moo/.{p2?}", "/moo/.foo", null, "foo")]
        [InlineData("moo/.{p2?}", "/moo", null, null)]
        [InlineData("moo/{p1}.{p2?}", "/moo/....", "..", ".")]
        [InlineData("moo/{p1}.{p2?}", "/moo/.bar", ".bar", null)]
        public void MatchRoute_OptionalParameter_FollowedByPeriod_Valid(
            string template,
            string path,
            string p1,
            string p2)
        {
            // Arrange
            var matcher = CreateMatcher(template);

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch(path, values);

            // Assert
            if (p1 != null)
            {
                Assert.Equal(p1, values["p1"]);
            }
            if (p2 != null)
            {
                Assert.Equal(p2, values["p2"]);
            }
        }

        [Theory]
        [InlineData("moo/{p1}.{p2}.{p3?}", "/moo/foo.moo.bar", "foo", "moo", "bar")]
        [InlineData("moo/{p1}.{p2}.{p3?}", "/moo/foo.moo", "foo", "moo", null)]
        [InlineData("moo/{p1}.{p2}.{p3}.{p4?}", "/moo/foo.moo.bar", "foo", "moo", "bar")]
        [InlineData("{p1}.{p2?}/{p3}", "/foo.moo/bar", "foo", "moo", "bar")]
        [InlineData("{p1}.{p2?}/{p3}", "/foo/bar", "foo", null, "bar")]
        [InlineData("{p1}.{p2?}/{p3}", "/.foo/bar", ".foo", null, "bar")]
        [InlineData("{p1}/{p2}/{p3?}", "/foo/bar/baz", "foo", "bar", "baz")]
        public void MatchRoute_OptionalParameter_FollowedByPeriod_3Parameters_Valid(
            string template,
            string path,
            string p1,
            string p2,
            string p3)
        {
            // Arrange
            var matcher = CreateMatcher(template);

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch(path, values);

            // Assert
            Assert.Equal(p1, values["p1"]);

            if (p2 != null)
            {
                Assert.Equal(p2, values["p2"]);
            }

            if (p3 != null)
            {
                Assert.Equal(p3, values["p3"]);
            }
        }

        [Theory]
        [InlineData("moo/{p1}.{p2?}", "/moo/foo.")]
        [InlineData("moo/{p1}.{p2?}", "/moo/.")]
        [InlineData("moo/{p1}.{p2}", "/foo.")]
        [InlineData("moo/{p1}.{p2}", "/foo")]
        [InlineData("moo/{p1}.{p2}.{p3?}", "/moo/foo.moo.")]
        [InlineData("moo/foo.{p2}.{p3?}", "/moo/bar.foo.moo")]
        [InlineData("moo/foo.{p2}.{p3?}", "/moo/kungfoo.moo.bar")]
        [InlineData("moo/foo.{p2}.{p3?}", "/moo/kungfoo.moo")]
        [InlineData("moo/{p1}.{p2}.{p3?}", "/moo/foo")]
        [InlineData("{p1}.{p2?}/{p3}", "/foo./bar")]
        [InlineData("moo/.{p2?}", "/moo/.")]
        [InlineData("{p1}.{p2}/{p3}", "/.foo/bar")]
        public void MatchRoute_OptionalParameter_FollowedByPeriod_Invalid(string template, string path)
        {
            // Arrange
            var matcher = CreateMatcher(template);

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch(path, values);

            // Assert
            Assert.False(match);
        }

        [Fact]
        public void MatchRouteWithOnlyLiterals()
        {
            // Arrange
            var matcher = CreateMatcher("moo/bar");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/moo/bar", values);

            // Assert
            Assert.True(match);
            Assert.Empty(values);
        }

        [Fact]
        public void NoMatchRouteWithOnlyLiterals()
        {
            // Arrange
            var matcher = CreateMatcher("moo/bars");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/moo/bar", values);

            // Assert
            Assert.False(match);
        }

        [Fact]
        public void MatchRouteWithExtraSeparators()
        {
            // Arrange
            var matcher = CreateMatcher("moo/bar");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/moo/bar/", values);

            // Assert
            Assert.True(match);
            Assert.Empty(values);
        }

        [Fact]
        public void MatchRouteUrlWithExtraSeparators()
        {
            // Arrange
            var matcher = CreateMatcher("moo/bar/");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/moo/bar", values);

            // Assert
            Assert.True(match);
            Assert.Empty(values);
        }

        [Fact]
        public void MatchRouteUrlWithParametersAndExtraSeparators()
        {
            // Arrange
            var matcher = CreateMatcher("{p1}/{p2}/");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/moo/bar", values);

            // Assert
            Assert.True(match);
            Assert.Equal("moo", values["p1"]);
            Assert.Equal("bar", values["p2"]);
        }

        [Fact]
        public void NoMatchRouteUrlWithDifferentLiterals()
        {
            // Arrange
            var matcher = CreateMatcher("{p1}/{p2}/baz");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/moo/bar/boo", values);

            // Assert
            Assert.False(match);
        }

        [Fact]
        public void NoMatchLongerUrl()
        {
            // Arrange
            var matcher = CreateMatcher("{p1}");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/moo/bar", values);

            // Assert
            Assert.False(match);
        }

        [Fact]
        public void MatchSimpleFilename()
        {
            // Arrange
            var matcher = CreateMatcher("DEFAULT.ASPX");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/default.aspx", values);

            // Assert
            Assert.True(match);
        }

        [Theory]
        [InlineData("{prefix}x{suffix}", "/xxxxxxxxxx")]
        [InlineData("{prefix}xyz{suffix}", "/xxxxyzxyzxxxxxxyz")]
        [InlineData("{prefix}xyz{suffix}", "/abcxxxxyzxyzxxxxxxyzxx")]
        [InlineData("{prefix}xyz{suffix}", "/xyzxyzxyzxyzxyz")]
        [InlineData("{prefix}xyz{suffix}", "/xyzxyzxyzxyzxyz1")]
        [InlineData("{prefix}xyz{suffix}", "/xyzxyzxyz")]
        [InlineData("{prefix}aa{suffix}", "/aaaaa")]
        [InlineData("{prefix}aaa{suffix}", "/aaaaa")]
        public void VerifyRouteMatchesWithContext(string template, string path)
        {
            var matcher = CreateMatcher(template);

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch(path, values);

            // Assert
            Assert.True(match);
        }

        [Fact]
        public void MatchRouteWithExtraDefaultValues()
        {
            // Arrange
            var matcher = CreateMatcher("{p1}/{p2}", new { p2 = (string)null, foo = "bar" });

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/v1", values);

            // Assert
            Assert.True(match);
            Assert.Equal<int>(3, values.Count);
            Assert.Equal("v1", values["p1"]);
            Assert.Null(values["p2"]);
            Assert.Equal("bar", values["foo"]);
        }

        [Fact]
        public void MatchPrettyRouteWithExtraDefaultValues()
        {
            // Arrange
            var matcher = CreateMatcher(
                "date/{y}/{m}/{d}",
                new { controller = "blog", action = "showpost", m = (string)null, d = (string)null });

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/date/2007/08", values);

            // Assert
            Assert.True(match);
            Assert.Equal<int>(5, values.Count);
            Assert.Equal("blog", values["controller"]);
            Assert.Equal("showpost", values["action"]);
            Assert.Equal("2007", values["y"]);
            Assert.Equal("08", values["m"]);
            Assert.Null(values["d"]);
        }

        [Fact]
        public void GetRouteDataWithMultiSegmentParamsOnBothEndsMatches()
        {
            RunTest(
                "language/{lang}-{region}",
                "/language/en-US",
                null,
                new RouteValueDictionary(new { lang = "en", region = "US" }));
        }

        [Fact]
        public void GetRouteDataWithMultiSegmentParamsOnLeftEndMatches()
        {
            RunTest(
                "language/{lang}-{region}a",
                "/language/en-USa",
                null,
                new RouteValueDictionary(new { lang = "en", region = "US" }));
        }

        [Fact]
        public void GetRouteDataWithMultiSegmentParamsOnRightEndMatches()
        {
            RunTest(
                "language/a{lang}-{region}",
                "/language/aen-US",
                null,
                new RouteValueDictionary(new { lang = "en", region = "US" }));
        }

        [Fact]
        public void GetRouteDataWithMultiSegmentParamsOnNeitherEndMatches()
        {
            RunTest(
                "language/a{lang}-{region}a",
                "/language/aen-USa",
                null,
                new RouteValueDictionary(new { lang = "en", region = "US" }));
        }

        [Fact]
        public void GetRouteDataWithMultiSegmentParamsOnNeitherEndDoesNotMatch()
        {
            RunTest(
                "language/a{lang}-{region}a",
                "/language/a-USa",
                null,
                null);
        }

        [Fact]
        public void GetRouteDataWithMultiSegmentParamsOnNeitherEndDoesNotMatch2()
        {
            RunTest(
                "language/a{lang}-{region}a",
                "/language/aen-a",
                null,
                null);
        }

        [Fact]
        public void GetRouteDataWithSimpleMultiSegmentParamsOnBothEndsMatches()
        {
            RunTest(
                "language/{lang}",
                "/language/en",
                null,
                new RouteValueDictionary(new { lang = "en" }));
        }

        [Fact]
        public void GetRouteDataWithSimpleMultiSegmentParamsOnBothEndsTrailingSlashDoesNotMatch()
        {
            RunTest(
                "language/{lang}",
                "/language/",
                null,
                null);
        }

        [Fact]
        public void GetRouteDataWithSimpleMultiSegmentParamsOnBothEndsDoesNotMatch()
        {
            RunTest(
                "language/{lang}",
                "/language",
                null,
                null);
        }

        [Fact]
        public void GetRouteDataWithSimpleMultiSegmentParamsOnLeftEndMatches()
        {
            RunTest(
                "language/{lang}-",
                "/language/en-",
                null,
                new RouteValueDictionary(new { lang = "en" }));
        }

        [Fact]
        public void GetRouteDataWithSimpleMultiSegmentParamsOnRightEndMatches()
        {
            RunTest(
                "language/a{lang}",
                "/language/aen",
                null,
                new RouteValueDictionary(new { lang = "en" }));
        }

        [Fact]
        public void GetRouteDataWithSimpleMultiSegmentParamsOnNeitherEndMatches()
        {
            RunTest(
                "language/a{lang}a",
                "/language/aena",
                null,
                new RouteValueDictionary(new { lang = "en" }));
        }

        [Fact]
        public void GetRouteDataWithMultiSegmentStandamatchMvcRouteMatches()
        {
            RunTest(
                "{controller}.mvc/{action}/{id}",
                "/home.mvc/index",
                new RouteValueDictionary(new { action = "Index", id = (string)null }),
                new RouteValueDictionary(new { controller = "home", action = "index", id = (string)null }));
        }

        [Fact]
        public void GetRouteDataWithMultiSegmentParamsOnBothEndsWithDefaultValuesMatches()
        {
            RunTest(
                "language/{lang}-{region}",
                "/language/-",
                new RouteValueDictionary(new { lang = "xx", region = "yy" }),
                null);
        }

        [Fact]
        public void GetRouteDataWithUrlWithMultiSegmentWithRepeatedDots()
        {
            RunTest(
                "{Controller}..mvc/{id}/{Param1}",
                "/Home..mvc/123/p1",
                null,
                new RouteValueDictionary(new { Controller = "Home", id = "123", Param1 = "p1" }));
        }

        [Fact]
        public void GetRouteDataWithUrlWithTwoRepeatedDots()
        {
            RunTest(
                "{Controller}.mvc/../{action}",
                "/Home.mvc/../index",
                null,
                new RouteValueDictionary(new { Controller = "Home", action = "index" }));
        }

        [Fact]
        public void GetRouteDataWithUrlWithThreeRepeatedDots()
        {
            RunTest(
                "{Controller}.mvc/.../{action}",
                "/Home.mvc/.../index",
                null,
                new RouteValueDictionary(new { Controller = "Home", action = "index" }));
        }

        [Fact]
        public void GetRouteDataWithUrlWithManyRepeatedDots()
        {
            RunTest(
                "{Controller}.mvc/../../../{action}",
                "/Home.mvc/../../../index",
                null,
                new RouteValueDictionary(new { Controller = "Home", action = "index" }));
        }

        [Fact]
        public void GetRouteDataWithUrlWithExclamationPoint()
        {
            RunTest(
                "{Controller}.mvc!/{action}",
                "/Home.mvc!/index",
                null,
                new RouteValueDictionary(new { Controller = "Home", action = "index" }));
        }

        [Fact]
        public void GetRouteDataWithUrlWithStartingDotDotSlash()
        {
            RunTest(
                "../{Controller}.mvc",
                "/../Home.mvc",
                null,
                new RouteValueDictionary(new { Controller = "Home" }));
        }

        [Fact]
        public void GetRouteDataWithUrlWithStartingBackslash()
        {
            RunTest(
                @"\{Controller}.mvc",
                @"/\Home.mvc",
                null,
                new RouteValueDictionary(new { Controller = "Home" }));
        }

        [Fact]
        public void GetRouteDataWithUrlWithBackslashSeparators()
        {
            RunTest(
                @"{Controller}.mvc\{id}\{Param1}",
                @"/Home.mvc\123\p1",
                null,
                new RouteValueDictionary(new { Controller = "Home", id = "123", Param1 = "p1" }));
        }

        [Fact]
        public void GetRouteDataWithUrlWithParenthesesLiterals()
        {
            RunTest(
                @"(Controller).mvc",
                @"/(Controller).mvc",
                null,
                new RouteValueDictionary());
        }

        [Fact]
        public void GetRouteDataWithUrlWithTrailingSlashSpace()
        {
            RunTest(
                @"Controller.mvc/ ",
                @"/Controller.mvc/ ",
                null,
                new RouteValueDictionary());
        }

        [Fact]
        public void GetRouteDataWithUrlWithTrailingSpace()
        {
            RunTest(
                @"Controller.mvc ",
                @"/Controller.mvc ",
                null,
                new RouteValueDictionary());
        }

        [Fact]
        public void GetRouteDataWithCatchAllCapturesDots()
        {
            // DevDiv Bugs 189892: UrlRouting: Catch all parameter cannot capture url segments that contain the "."
            RunTest(
                "Home/ShowPilot/{missionId}/{*name}",
                "/Home/ShowPilot/777/12345./foobar",
                new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "ShowPilot",
                    missionId = (string)null,
                    name = (string)null
                }),
                new RouteValueDictionary(new { controller = "Home", action = "ShowPilot", missionId = "777", name = "12345./foobar" }));
        }

        [Fact]
        public void RouteWithCatchAllClauseCapturesManySlashes()
        {
            // Arrange
            var matcher = CreateMatcher("{p1}/{*p2}");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/v1/v2/v3", values);

            // Assert
            Assert.True(match);
            Assert.Equal<int>(2, values.Count);
            Assert.Equal("v1", values["p1"]);
            Assert.Equal("v2/v3", values["p2"]);
        }

        [Fact]
        public void RouteWithCatchAllClauseCapturesTrailingSlash()
        {
            // Arrange
            var matcher = CreateMatcher("{p1}/{*p2}");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/v1/", values);

            // Assert
            Assert.True(match);
            Assert.Equal<int>(2, values.Count);
            Assert.Equal("v1", values["p1"]);
            Assert.Null(values["p2"]);
        }

        [Fact]
        public void RouteWithCatchAllClauseCapturesEmptyContent()
        {
            // Arrange
            var matcher = CreateMatcher("{p1}/{*p2}");

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/v1", values);

            // Assert
            Assert.True(match);
            Assert.Equal<int>(2, values.Count);
            Assert.Equal("v1", values["p1"]);
            Assert.Null(values["p2"]);
        }

        [Fact]
        public void RouteWithCatchAllClauseUsesDefaultValueForEmptyContent()
        {
            // Arrange
            var matcher = CreateMatcher("{p1}/{*p2}", new { p2 = "catchall" });

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/v1", values);

            // Assert
            Assert.True(match);
            Assert.Equal<int>(2, values.Count);
            Assert.Equal("v1", values["p1"]);
            Assert.Equal("catchall", values["p2"]);
        }

        [Fact]
        public void RouteWithCatchAllClauseIgnoresDefaultValueForNonEmptyContent()
        {
            // Arrange
            var matcher = CreateMatcher("{p1}/{*p2}", new { p2 = "catchall" });

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch("/v1/hello/whatever", values);

            // Assert
            Assert.True(match);
            Assert.Equal<int>(2, values.Count);
            Assert.Equal("v1", values["p1"]);
            Assert.Equal("hello/whatever", values["p2"]);
        }

        [Fact]
        public void GetRouteDataDoesNotMatchOnlyLeftLiteralMatch()
        {
            // DevDiv Bugs 191180: UrlRouting: Wrong template getting matched if a url segment is a substring of the requested url
            RunTest(
                "foo",
                "/fooBAR",
                null,
                null);
        }

        [Fact]
        public void GetRouteDataDoesNotMatchOnlyRightLiteralMatch()
        {
            // DevDiv Bugs 191180: UrlRouting: Wrong template getting matched if a url segment is a substring of the requested url
            RunTest(
                "foo",
                "/BARfoo",
                null,
                null);
        }

        [Fact]
        public void GetRouteDataDoesNotMatchMiddleLiteralMatch()
        {
            // DevDiv Bugs 191180: UrlRouting: Wrong template getting matched if a url segment is a substring of the requested url
            RunTest(
                "foo",
                "/BARfooBAR",
                null,
                null);
        }

        [Fact]
        public void GetRouteDataDoesMatchesExactLiteralMatch()
        {
            // DevDiv Bugs 191180: UrlRouting: Wrong template getting matched if a url segment is a substring of the requested url
            RunTest(
                "foo",
                "/foo",
                null,
                new RouteValueDictionary());
        }

        [Fact]
        public void GetRouteDataWithWeimatchParameterNames()
        {
            RunTest(
                "foo/{ }/{.!$%}/{dynamic.data}/{op.tional}",
                "/foo/space/weimatch/omatcherid",
                new RouteValueDictionary() { { " ", "not a space" }, { "op.tional", "default value" }, { "ran!dom", "va@lue" } },
                new RouteValueDictionary() { { " ", "space" }, { ".!$%", "weimatch" }, { "dynamic.data", "omatcherid" }, { "op.tional", "default value" }, { "ran!dom", "va@lue" } });
        }

        [Fact]
        public void GetRouteDataDoesNotMatchRouteWithLiteralSeparatomatchefaultsButNoValue()
        {
            RunTest(
                "{controller}/{language}-{locale}",
                "/foo",
                new RouteValueDictionary(new { language = "en", locale = "US" }),
                null);
        }

        [Fact]
        public void GetRouteDataDoesNotMatchesRouteWithLiteralSeparatomatchefaultsAndLeftValue()
        {
            RunTest(
                "{controller}/{language}-{locale}",
                "/foo/xx-",
                new RouteValueDictionary(new { language = "en", locale = "US" }),
                null);
        }

        [Fact]
        public void GetRouteDataDoesNotMatchesRouteWithLiteralSeparatomatchefaultsAndRightValue()
        {
            RunTest(
                "{controller}/{language}-{locale}",
                "/foo/-yy",
                new RouteValueDictionary(new { language = "en", locale = "US" }),
                null);
        }

        [Fact]
        public void GetRouteDataMatchesRouteWithLiteralSeparatomatchefaultsAndValue()
        {
            RunTest(
                "{controller}/{language}-{locale}",
                "/foo/xx-yy",
                new RouteValueDictionary(new { language = "en", locale = "US" }),
                new RouteValueDictionary { { "language", "xx" }, { "locale", "yy" }, { "controller", "foo" } });
        }

        [Fact]
        public void MatchSetsOptionalParameter()
        {
            // Arrange
            var route = CreateMatcher("{controller}/{action?}");
            var url = "/Home/Index";

            var values = new RouteValueDictionary();

            // Act
            var match = route.TryMatch(url, values);

            // Assert
            Assert.True(match);
            Assert.Equal(2, values.Count);
            Assert.Equal("Home", values["controller"]);
            Assert.Equal("Index", values["action"]);
        }

        [Fact]
        public void MatchDoesNotSetOptionalParameter()
        {
            // Arrange
            var route = CreateMatcher("{controller}/{action?}");
            var url = "/Home";

            var values = new RouteValueDictionary();

            // Act
            var match = route.TryMatch(url, values);

            // Assert
            Assert.True(match);
            Assert.Equal(1, values.Count);
            Assert.Equal("Home", values["controller"]);
            Assert.False(values.ContainsKey("action"));
        }

        [Fact]
        public void MatchDoesNotSetOptionalParameter_EmptyString()
        {
            // Arrange
            var route = CreateMatcher("{controller?}");
            var url = "";

            var values = new RouteValueDictionary();

            // Act
            var match = route.TryMatch(url, values);

            // Assert
            Assert.True(match);
            Assert.Equal(0, values.Count);
            Assert.False(values.ContainsKey("controller"));
        }

        [Fact]
        public void Match_EmptyRouteWith_EmptyString()
        {
            // Arrange
            var route = CreateMatcher("");
            var url = "";

            var values = new RouteValueDictionary();

            // Act
            var match = route.TryMatch(url, values);

            // Assert
            Assert.True(match);
            Assert.Equal(0, values.Count);
        }

        [Fact]
        public void MatchMultipleOptionalParameters()
        {
            // Arrange
            var route = CreateMatcher("{controller}/{action?}/{id?}");
            var url = "/Home/Index";

            var values = new RouteValueDictionary();

            // Act
            var match = route.TryMatch(url, values);

            // Assert
            Assert.True(match);
            Assert.Equal(2, values.Count);
            Assert.Equal("Home", values["controller"]);
            Assert.Equal("Index", values["action"]);
            Assert.False(values.ContainsKey("id"));
        }

        private TemplateMatcher CreateMatcher(string template, object defaults = null)
        {
            return new TemplateMatcher(
                TemplateParser.Parse(template),
                new RouteValueDictionary(defaults));
        }

        private static void RunTest(
            string template,
            string path,
            RouteValueDictionary defaults,
            IDictionary<string, object> expected)
        {
            // Arrange
            var matcher = new TemplateMatcher(
                TemplateParser.Parse(template), 
                defaults ?? new RouteValueDictionary());

            var values = new RouteValueDictionary();

            // Act
            var match = matcher.TryMatch(new PathString(path), values);

            // Assert
            if (expected == null)
            {
                Assert.False(match);
            }
            else
            {
                Assert.True(match);
                Assert.Equal(expected.Count, values.Count);
                foreach (string key in values.Keys)
                {
                    Assert.Equal(expected[key], values[key]);
                }
            }
        }

        private static IInlineConstraintResolver GetInlineConstraintResolver()
        {
            var services = new ServiceCollection().AddOptions();
            var serviceProvider = services.BuildServiceProvider();
            var accessor = serviceProvider.GetRequiredService<IOptions<RouteOptions>>();
            return new DefaultInlineConstraintResolver(accessor);
        }
    }
}
