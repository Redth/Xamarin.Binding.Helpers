using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Binding.Helpers.NugetResolvers;

namespace Xamarin.Binding.Helpers
{
    public class AndroidStudioProjectInfo
	{
        public string ModuleArtifact { get; set; }

        public List<MavenArtifactNuGetPairing> AllDependencies { get; set; }

        public IEnumerable<NuGetSuggestion> NuGetDependencies =>
            AllDependencies
                .Where(nd => !string.IsNullOrEmpty(nd.NuGet?.Version) && !string.IsNullOrEmpty(nd.NuGet?.PackageId))
                .Select(nd => nd.NuGet);

        public IEnumerable<LocalMavenArtifact> MavenArtifacts => 
            AllDependencies
                .Where(ma => !string.IsNullOrEmpty(ma.MavenDependency?.File))
                .Select(ma => ma.MavenDependency);

	}
	public class AndroidStudioProject
	{
		public AndroidStudioProject(string projectPath)
		{
            ProjectPath = projectPath;
		}

		public string ProjectPath { get; set; }

		public async Task<AndroidStudioProjectInfo> GetDependencies(string module, IEnumerable<ExplicitMavenNugetMapping> mavenNugetMappings)
		{
            var modulePath = Path.Combine(ProjectPath, module);

            if (!Directory.Exists(modulePath))
                throw new DirectoryNotFoundException($"Module directory for '{module}' was not found in project directory '{ProjectPath}'.");

            var moduleGradleFile = Path.Combine(modulePath, "build.gradle");

            if (!File.Exists(moduleGradleFile))
                throw new FileNotFoundException($"The build.gradle file for the '{module}' module was not found in the project's module directory: '{ProjectPath}'.", moduleGradleFile);

            var depTreeText = await GradleUtil.RunGradleProjectCommand(ProjectPath, $"{module}:dependencies", "--configuration", "implementation");
            var resolvedDepText = await GradleUtil.RunGradleProjectCommand(ProjectPath, $"{module}:fetchXamarinBindingInfo");

            if (resolvedDepText.Contains("Task 'fetchXamarinBindingInfo' not found"))
                throw new InvalidOperationException($"Failed to execute 'fetchXamarinBindingInfo' gradle task.  Ensure the build.gradle for your project's module '{module}' has the Xamarin Binding Helpers gradle plugin loaded using the `apply from: '<url-to-binding-helpers-gradle-plugin>'` line near the top of the build.gradle.");

            await GradleUtil.RunGradleProjectCommand(ProjectPath, $"{module}:bundleReleaseAar");

            var deps = GradleUtil.ParseDependenciesTree(depTreeText);

            var bindingDeps = await MavenNuGetSomelier.MapNuGets(deps,
                new ExplicitMavenNugetResolver(mavenNugetMappings),
                new AndroidXMavenNugetResolver(),
                new GooglePlayServicesMavenNugetResolver(),
                new FirebaseMavenNugetResolver(),
                new KnownMavenNugetResolver());

            var mavenArtifactLocalFiles = new Dictionary<string, string>();

            const string xamGradleArtifactPrefix = "XAMARIN.GRADLE.ARTIFACT|";

            var localArtifacts = new List<string>();

            string libraryLocalAar = null;

            foreach (var line in resolvedDepText)
            {
                if (!line.StartsWith(xamGradleArtifactPrefix))
                    continue;

                var parts = line.Substring(xamGradleArtifactPrefix.Length).Split(new [] { '|' }, 2, StringSplitOptions.RemoveEmptyEntries);

                if ((parts?.Length ?? 0) != 2)
                    continue;

                if (parts[0] == "LOCAL")
                    localArtifacts.Add(parts[1]);
                else if (parts[0] == "LIBRARY")
                    libraryLocalAar = parts[1];
                else
                {
                    var bindingDep = bindingDeps.FirstOrDefault(bd =>
                        $"{bd.MavenDependency.GroupId}:{bd.MavenDependency.ArtifactId}".Equals(parts[0], StringComparison.OrdinalIgnoreCase));
                    if (bindingDep != null)
                        bindingDep.MavenDependency.File = parts[1];
                }
            }

            return new AndroidStudioProjectInfo { AllDependencies = bindingDeps.ToList(), ModuleArtifact = libraryLocalAar };
        }
	}
}
