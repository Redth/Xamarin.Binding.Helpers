using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Xml;
using Xamarin.Binding.Helpers.NugetResolvers;

namespace Xamarin.Binding.Helpers.Tests
{
    public class UnitTest1
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
        public void Test1()
        {
            var deps = GradleUtil.ParseDependenciesTree(dependenciesTree.Split(Environment.NewLine));

            var tree = GradleUtil.PrintTree(deps);

            Console.WriteLine(tree);

            Assert.NotEmpty(deps);
        }

        [Theory]
        [InlineData("androidx.collection", "collection", "1.0.0", "1.1.0", "Xamarin.AndroidX.Collection", "1.1.0")]
        [InlineData("androidx.vectordrawable", "vectordrawable-animated", "1.1.0", null, "Xamarin.AndroidX.VectorDrawable.Animated", "1.1.0")]
        [InlineData("com.google.android.gms", "play-services-basement", "17.0.0", "17.1.0", "Xamarin.GooglePlayServices.Basement", "117.2.1")]
        [InlineData("com.squareup.okhttp3", "okhttp", "3.12.3", null, "Square.OkHttp3", "3.12.3")]
        [InlineData("com.squareup.okio", "okio", "1.15.0", null, "Square.OkIO", "1.15.0")]
        public async Task Test2(string groupId, string artifactId, string reqVersion, string resolvedVersion, string packageId, string nugetVerison)
        {
            var r = await MavenNuGetSomelier.SuggestNuget(new LocalMavenArtifact
            {
                GroupId = groupId,
                ArtifactId = artifactId,
                RequestedVersion = reqVersion,
                ResolvedVersion = resolvedVersion
            }, 
            new AndroidXMavenNugetResolver(),
            new GooglePlayServicesMavenNugetResolver(),
            new FirebaseMavenNugetResolver(),
            new ExplicitMavenNugetResolver(
                new ExplicitMavenNugetMapping("com.squareup.okhttp3", "okhttp", "3.12.3", "Square.OkHttp3", "3.12.3"),
                new ExplicitMavenNugetMapping("com.squareup.okio", "okio", "1.15.0", "Square.OkIO", "1.15.0")));

            Assert.Equal(packageId, r.PackageId, true, true);

            var atLeastVersion = SemanticVersion.Parse(nugetVerison);
            Assert.True(NuGetVersion.Parse(r.Version) >= atLeastVersion);
        }

        [Fact]
        public async Task Test3()
		{
            var r = await GradleUtil.RunGradleProjectCommand(
                @"C:\Users\jondi\Downloads\DuoNativeSamples\MyApplication",
                "mylibrary:dependencies", "--configuration", "implementation");

            Assert.NotNull(r);
		}

        [Fact]
        public async Task Test4()
		{
            var androidProject = @"C:\Users\jondi\Downloads\DuoNativeSamples\MyApplication";
            
            var depTreeText = await GradleUtil.RunGradleProjectCommand(androidProject, "mylibrary:dependencies", "--configuration", "implementation");
            var resolvedDepText = await GradleUtil.RunGradleProjectCommand(androidProject, "mylibrary:fetchXamarinBindingInfo");

            var deps = GradleUtil.ParseDependenciesTree(depTreeText);

            var bindingDeps = await MavenNuGetSomelier.MapNuGets(deps,
                new AndroidXMavenNugetResolver(),
                new GooglePlayServicesMavenNugetResolver(),
                new FirebaseMavenNugetResolver(),
                new ExplicitMavenNugetResolver(
                    new ExplicitMavenNugetMapping("com.squareup.okhttp3", "okhttp", "3.12.3", "Square.OkHttp3", "3.12.3"),
                    new ExplicitMavenNugetMapping("com.squareup.okio", "okio", "1.15.0", "Square.OkIO", "1.15.0"),
                    new ExplicitMavenNugetMapping("com.google.code.gson", "gson", "2.8.5", "GoogleGson", "2.8.5"),
                    new ExplicitMavenNugetMapping("com.google.android.material", "material", "1.2.1", "Xamarin.Google.Android.Material", "1.2.1-rc1")));


            var mavenArtifactLocalFiles = new Dictionary<string, string>();

            const string xamGradleArtifactPrefix = "XAMARIN.GRADLE.ARTIFACT|";

            var localArtifacts = new List<string>();

            foreach (var line in resolvedDepText)
			{
                if (!line.StartsWith(xamGradleArtifactPrefix))
                    continue;

                var parts = line.Substring(xamGradleArtifactPrefix.Length).Split('|', 2, StringSplitOptions.RemoveEmptyEntries);

                if ((parts?.Length ?? 0) != 2)
                    continue;

                if (parts[0] == "LOCAL")
                    localArtifacts.Add(parts[1]);
                else
				{
                    var bindingDep = bindingDeps.FirstOrDefault(bd =>
                        $"{bd.MavenDependency.GroupId}:{bd.MavenDependency.ArtifactId}".Equals(parts[0], StringComparison.OrdinalIgnoreCase));
                    if (bindingDep != null)
                        bindingDep.MavenDependency.File = parts[1];
				}
			}



            var text = new StringBuilder();

            text.AppendLine("<ItemGroup>");
            foreach(var bd in bindingDeps.Where(b => b.NuGet != null))
			{
                text.AppendLine($"    <PackageReference Include=\"{bd.NuGet.PackageId}\" Version=\"{bd.NuGet.Version}\" />");
            }
            text.AppendLine("</ItemGroup>");

            text.AppendLine("<ItemGroup>");
            foreach (var bd in bindingDeps.Where(b => b.NuGet == null))
            {
                var ext = System.IO.Path.GetExtension(bd.MavenDependency.File);

                var isAar = ext?.Equals(".aar", StringComparison.OrdinalIgnoreCase) ?? false;

                if (isAar)
                    text.AppendLine($"    <AndroidAarLibrary Include=\"{bd.MavenDependency.File}\" />");
                else
                    text.AppendLine($"    <AndroidExternalJavaLibrary Include=\"{bd.MavenDependency.File}\" />");
            }
            text.AppendLine("</ItemGroup>");

            Assert.NotNull(text.ToString());
            Assert.NotEmpty(bindingDeps);
        }
    }
}
