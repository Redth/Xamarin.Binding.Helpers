using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xamarin.Binding.Helpers.NugetResolvers;

namespace Xamarin.Binding.Helpers.Tests
{
    public class Tests
    {
        const string dependenciesTree =
@"
> Task :mylibrary:dependencies

------------------------------------------------------------
Project :mylibrary
------------------------------------------------------------

implementation - Implementation only dependencies for 'main' sources.
+--- com.mapbox.mapboxsdk:mapbox-android-sdk:9.5.0
|    +--- com.mapbox.mapboxsdk:mapbox-android-telemetry:6.1.0
|    |    +--- com.squareup.okhttp3:okhttp:3.12.0 -> 3.12.3
|    |    |    \--- com.squareup.okio:okio:1.15.0
|    |    +--- com.google.code.gson:gson:2.8.5 -> 2.8.6
|    |    +--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    |    +--- com.mapbox.mapboxsdk:mapbox-android-core:3.0.0
|    |    \--- androidx.legacy:legacy-support-core-utils:1.0.0
|    |         +--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    |         +--- androidx.core:core:1.0.0 -> 1.3.0
|    |         |    +--- androidx.annotation:annotation:1.1.0
|    |         |    +--- androidx.lifecycle:lifecycle-runtime:2.0.0 -> 2.1.0
|    |         |    |    +--- androidx.lifecycle:lifecycle-common:2.1.0
|    |         |    |    |    \--- androidx.annotation:annotation:1.1.0
|    |         |    |    +--- androidx.arch.core:core-common:2.1.0
|    |         |    |    |    \--- androidx.annotation:annotation:1.1.0
|    |         |    |    \--- androidx.annotation:annotation:1.1.0
|    |         |    +--- androidx.versionedparcelable:versionedparcelable:1.1.0
|    |         |    |    +--- androidx.annotation:annotation:1.1.0
|    |         |    |    \--- androidx.collection:collection:1.0.0 -> 1.1.0
|    |         |    |         \--- androidx.annotation:annotation:1.1.0
|    |         |    \--- androidx.collection:collection:1.0.0 -> 1.1.0 (*)
|    |         +--- androidx.documentfile:documentfile:1.0.0
|    |         |    \--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    |         +--- androidx.loader:loader:1.0.0
|    |         |    +--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    |         |    +--- androidx.core:core:1.0.0 -> 1.3.0 (*)
|    |         |    +--- androidx.lifecycle:lifecycle-livedata:2.0.0
|    |         |    |    +--- androidx.arch.core:core-runtime:2.0.0
|    |         |    |    |    +--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    |         |    |    |    \--- androidx.arch.core:core-common:2.0.0 -> 2.1.0 (*)
|    |         |    |    +--- androidx.lifecycle:lifecycle-livedata-core:2.0.0
|    |         |    |    |    +--- androidx.lifecycle:lifecycle-common:2.0.0 -> 2.1.0 (*)
|    |         |    |    |    +--- androidx.arch.core:core-common:2.0.0 -> 2.1.0 (*)
|    |         |    |    |    \--- androidx.arch.core:core-runtime:2.0.0 (*)
|    |         |    |    \--- androidx.arch.core:core-common:2.0.0 -> 2.1.0 (*)
|    |         |    \--- androidx.lifecycle:lifecycle-viewmodel:2.0.0 -> 2.1.0
|    |         |         \--- androidx.annotation:annotation:1.1.0
|    |         +--- androidx.localbroadcastmanager:localbroadcastmanager:1.0.0
|    |         |    \--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    |         \--- androidx.print:print:1.0.0
|    |              \--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    +--- com.mapbox.mapboxsdk:mapbox-sdk-geojson:5.3.0
|    |    \--- com.google.code.gson:gson:2.8.6
|    +--- com.mapbox.mapboxsdk:mapbox-android-gestures:0.7.0
|    |    +--- androidx.core:core:1.0.0 -> 1.3.0 (*)
|    |    \--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    +--- com.mapbox.mapboxsdk:mapbox-android-accounts:0.7.0
|    |    \--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    +--- com.mapbox.mapboxsdk:mapbox-android-sdk-gl-core:5.0.0
|    +--- com.mapbox.mapboxsdk:mapbox-sdk-turf:5.3.0
|    |    \--- com.mapbox.mapboxsdk:mapbox-sdk-geojson:5.3.0 (*)
|    +--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    +--- androidx.fragment:fragment:1.0.0 -> 1.1.0
|    |    +--- androidx.annotation:annotation:1.1.0
|    |    +--- androidx.core:core:1.1.0 -> 1.3.0 (*)
|    |    +--- androidx.collection:collection:1.1.0 (*)
|    |    +--- androidx.viewpager:viewpager:1.0.0
|    |    |    +--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    |    |    +--- androidx.core:core:1.0.0 -> 1.3.0 (*)
|    |    |    \--- androidx.customview:customview:1.0.0
|    |    |         +--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    |    |         \--- androidx.core:core:1.0.0 -> 1.3.0 (*)
|    |    +--- androidx.loader:loader:1.0.0 (*)
|    |    +--- androidx.activity:activity:1.0.0
|    |    |    +--- androidx.annotation:annotation:1.1.0
|    |    |    +--- androidx.core:core:1.1.0 -> 1.3.0 (*)
|    |    |    +--- androidx.lifecycle:lifecycle-runtime:2.1.0 (*)
|    |    |    +--- androidx.lifecycle:lifecycle-viewmodel:2.1.0 (*)
|    |    |    \--- androidx.savedstate:savedstate:1.0.0
|    |    |         +--- androidx.annotation:annotation:1.1.0
|    |    |         +--- androidx.arch.core:core-common:2.0.1 -> 2.1.0 (*)
|    |    |         \--- androidx.lifecycle:lifecycle-common:2.0.0 -> 2.1.0 (*)
|    |    \--- androidx.lifecycle:lifecycle-viewmodel:2.0.0 -> 2.1.0 (*)
|    \--- com.squareup.okhttp3:okhttp:3.12.3 (*)
+--- androidx.appcompat:appcompat:1.2.0
|    +--- androidx.annotation:annotation:1.1.0
|    +--- androidx.core:core:1.3.0 (*)
|    +--- androidx.cursoradapter:cursoradapter:1.0.0
|    |    \--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    +--- androidx.fragment:fragment:1.1.0 (*)
|    +--- androidx.appcompat:appcompat-resources:1.2.0
|    |    +--- androidx.collection:collection:1.0.0 -> 1.1.0 (*)
|    |    +--- androidx.annotation:annotation:1.1.0
|    |    +--- androidx.core:core:1.0.1 -> 1.3.0 (*)
|    |    +--- androidx.vectordrawable:vectordrawable:1.1.0
|    |    |    +--- androidx.annotation:annotation:1.1.0
|    |    |    +--- androidx.core:core:1.1.0 -> 1.3.0 (*)
|    |    |    \--- androidx.collection:collection:1.1.0 (*)
|    |    \--- androidx.vectordrawable:vectordrawable-animated:1.1.0
|    |         +--- androidx.vectordrawable:vectordrawable:1.1.0 (*)
|    |         +--- androidx.interpolator:interpolator:1.0.0
|    |         |    \--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    |         \--- androidx.collection:collection:1.1.0 (*)
|    +--- androidx.drawerlayout:drawerlayout:1.0.0
|    |    +--- androidx.annotation:annotation:1.0.0 -> 1.1.0
|    |    +--- androidx.core:core:1.0.0 -> 1.3.0 (*)
|    |    \--- androidx.customview:customview:1.0.0 (*)
|    \--- androidx.collection:collection:1.0.0 -> 1.1.0 (*)
\--- com.google.android.material:material:1.2.1
     +--- androidx.annotation:annotation:1.0.1 -> 1.1.0
     +--- androidx.appcompat:appcompat:1.1.0 -> 1.2.0 (*)
     +--- androidx.cardview:cardview:1.0.0
     |    \--- androidx.annotation:annotation:1.0.0 -> 1.1.0
     +--- androidx.coordinatorlayout:coordinatorlayout:1.1.0
     |    +--- androidx.annotation:annotation:1.1.0
     |    +--- androidx.core:core:1.1.0 -> 1.3.0 (*)
     |    +--- androidx.customview:customview:1.0.0 (*)
     |    \--- androidx.collection:collection:1.0.0 -> 1.1.0 (*)
     +--- androidx.core:core:1.2.0 -> 1.3.0 (*)
     +--- androidx.annotation:annotation-experimental:1.0.0
     +--- androidx.fragment:fragment:1.0.0 -> 1.1.0 (*)
     +--- androidx.lifecycle:lifecycle-runtime:2.0.0 -> 2.1.0 (*)
     +--- androidx.recyclerview:recyclerview:1.0.0 -> 1.1.0
     |    +--- androidx.annotation:annotation:1.1.0
     |    +--- androidx.core:core:1.1.0 -> 1.3.0 (*)
     |    +--- androidx.customview:customview:1.0.0 (*)
     |    \--- androidx.collection:collection:1.0.0 -> 1.1.0 (*)
     +--- androidx.transition:transition:1.2.0
     |    +--- androidx.annotation:annotation:1.1.0
     |    +--- androidx.core:core:1.0.1 -> 1.3.0 (*)
     |    \--- androidx.collection:collection:1.0.0 -> 1.1.0 (*)
     +--- androidx.vectordrawable:vectordrawable:1.1.0 (*)
     \--- androidx.viewpager2:viewpager2:1.0.0
          +--- androidx.annotation:annotation:1.1.0
          +--- androidx.fragment:fragment:1.1.0 (*)
          +--- androidx.recyclerview:recyclerview:1.1.0 (*)
          +--- androidx.core:core:1.1.0 -> 1.3.0 (*)
          \--- androidx.collection:collection:1.1.0 (*)

(*) - dependencies omitted (listed previously)

A web-based, searchable dependency report is available by adding the --scan option.

BUILD SUCCESSFUL in 1s
1 actionable task: 1 executed
";
        [Fact]
        public void ParseGradleDependenciesTree()
        {
            var deps = GradleUtil.ParseDependenciesTree(dependenciesTree.Split(Environment.NewLine));

            var tree = GradleUtil.PrintTree(deps);

            var flat = new List<LocalMavenArtifact>();

            void flatten(IEnumerable<LocalMavenArtifact> node)
			{
                foreach (var n in node)
				{
                    flat.Add(n);
                    flatten(n.Dependencies);
				}
			}

            flatten(deps);

            Assert.NotEmpty(tree);
            Assert.NotEmpty(deps);

            Assert.Contains(flat, d => d.GroupId == "androidx.transition" && d.ArtifactId == "transition" && d.Depth == 2);
        }

        [Theory]
        [InlineData("androidx.collection", "collection", "1.0.0", "1.1.0", "Xamarin.AndroidX.Collection", "1.1.0")]
        [InlineData("androidx.vectordrawable", "vectordrawable-animated", "1.1.0", null, "Xamarin.AndroidX.VectorDrawable.Animated", "1.1.0")]
        [InlineData("androidx.core", "core", "1.9.0", null, "Xamarin.AndroidX.Core", "1.9.0")]
        [InlineData("androidx.core", "core-ktx", "1.9.0", null, "Xamarin.AndroidX.Core.Core.Ktx", "1.9.0")]
        [InlineData("com.google.android.gms", "play-services-basement", "17.0.0", "17.1.0", "Xamarin.GooglePlayServices.Basement", "117.1.0")]
        [InlineData("com.squareup.okhttp3", "okhttp", "3.12.3", null, "Square.OkHttp3", "3.12.3")]
        [InlineData("com.squareup.okio", "okio", "1.15.0", null, "Square.OkIO", "1.15.0")]
        [InlineData("com.google.android.material", "material", "1.2.1", null, "Xamarin.Google.Android.Material", "1.2.1-rc1")]
        public async Task ResolveNuGetPackages(string groupId, string artifactId, string reqVersion, string resolvedVersion, string packageId, string nugetVerison)
        {
            var r = await MavenNuGetSomelier.SuggestNuget(new LocalMavenArtifact
            {
                GroupId = groupId,
                ArtifactId = artifactId,
                RequestedVersion = reqVersion,
                ResolvedVersion = resolvedVersion
            },
            new ExplicitMavenNugetResolver(
                new ExplicitMavenNugetMapping("com.squareup.okhttp3", "okhttp", "3.12.3", "Square.OkHttp3", "3.12.3"),
                new ExplicitMavenNugetMapping("com.squareup.okio", "okio", "1.15.0", "Square.OkIO", "1.15.0")),
            new AndroidXMavenNugetResolver(),
            new GooglePlayServicesMavenNugetResolver(),
            new FirebaseMavenNugetResolver(),
            new KnownMavenNugetResolver());

            Assert.Equal(packageId, r.PackageId, true, true);

            var atLeastVersion = NuGetVersion.Parse(nugetVerison);
            Assert.True(NuGetVersion.Parse(r.Version) >= atLeastVersion);
        }

        [Theory] // expect          min         max         pre?   avail
        [InlineData("1.0.0",        "0.9",      "1.0.0",    false, new [] { "0.1", "0.9", "1.0", "1.1" })]
        [InlineData("0.9.0",        "0.9",      null,       false, new[] { "0.1", "0.9", "1.0", "1.1" })]
        [InlineData("0.9.0",        "0.9",      "1.0.0",    false, new[] { "0.1", "0.9", "1.1" })]
        [InlineData("1.0.0",        "0.9",      null,       false, new[] { "0.1", "1.0", "1.1" })]
        [InlineData("1.2.1-rc1",    "1.2.1",    null,       true,  new[] { "0.1", "1.1.0.5", "1.2.1-rc1" })]
        [InlineData("2.8.6",        "2.8.5",    "2.8.6",    false, new[] { "2.8.5", "2.8.5.1", "2.8.6" })]
        [InlineData("2.8.5.1",      "2.8.5",    "2.8.6",    false, new[] { "2.8.5", "2.8.5.1" })]
        [InlineData("2.8.5.1",      "2.8.5",    "2.8.5",    false, new[] { "2.8.5", "2.8.5.1" })]
        public void IdealVersionTests(string expected, string min, string max, bool allowPrerelease, string[] available)
		{
            var versions = available.Select(v => NuGetVersion.Parse(v));

            var best = NuGetUtil.FindBestVersion(versions, min, max, allowPrerelease);

            Assert.Equal(expected, best.ToNormalizedString());
		}
    }
}
