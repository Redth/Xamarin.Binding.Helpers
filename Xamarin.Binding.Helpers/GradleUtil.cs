using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xamarin.Binding.Helpers
{
	public class GradleUtil
	{
		static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;

		

		public static async Task<IEnumerable<string>> RunGradleProjectCommand(string projectPath, params string[] args)
		{

			var parg = new ProcessArgumentBuilder();
			foreach (var a in args)
				parg.Append(a);

			var r = await ProcessRunner.RunAsync(new FileInfo(Path.Combine(projectPath, "gradlew" + (IsWindows ? ".bat" : string.Empty))), parg, new DirectoryInfo(projectPath));

			return r.StandardOutput;
		}

		const string rxpTreeDepth = @"(?<depth>\|    |\+--- |\\--- |     ){1,}(?<group>[^:]+):(?<artifact>[^:]+):(?<requestedVersion>[0-9\.]+)(\s?->\s?(?<resolvedVersion>[0-9.]+))?";
		
		public static List<LocalMavenArtifact> ParseDependenciesTree(IEnumerable<string> lines)
		{
			var rxDepth = new Regex(rxpTreeDepth, RegexOptions.Singleline | RegexOptions.Compiled);

			var rootDependencies = new List<LocalMavenArtifact>();


			LocalMavenArtifact parentDependency = null;

			foreach (var line in lines)
			{
				var match = rxDepth.Match(line);

				if (!match.Success)
					continue;

				var depth = match.Groups?["depth"]?.Captures?.Count ?? 0;

				var groupId = match.Groups?["group"]?.Value;
				var artifactId = match.Groups?["artifact"]?.Value;
				var reqVersion = match.Groups?["requestedVersion"]?.Value;
				var resVersion = match.Groups?["resolvedVersion"]?.Value;

				if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(artifactId) || string.IsNullOrEmpty(reqVersion))
					continue;

				var dep = new LocalMavenArtifact
				{
					GroupId = groupId,
					ArtifactId = artifactId,
					RequestedVersion = reqVersion,
					ResolvedVersion = resVersion,
					Depth = depth
				};

				if (dep.Depth == 1)
				{
					// No parent, this is a root level dependency
					rootDependencies.Add(dep);
				}
				else if (dep.Depth > parentDependency.Depth)
				{
					// item is a dependency of hte parent
					dep.Parent = parentDependency;
					parentDependency.Dependencies.Add(dep);
				}
				else if (dep.Depth < parentDependency.Depth)
				{
					// This is up a level and a sibling of the parent
					var climbUp = parentDependency.Depth - dep.Depth;
					var actualParent = parentDependency.Parent;
					for (int i = 0; i < climbUp; i++)
						actualParent = actualParent.Parent;
					dep.Parent = actualParent;
					actualParent.Dependencies.Add(dep);
				}
				else if (dep.Depth == parentDependency.Depth)
				{
					// This is a sibling of the current parent
					dep.Parent = parentDependency.Parent;
					parentDependency.Parent.Dependencies.Add(dep);
				}

				parentDependency = dep;

			}

			return rootDependencies;
		}

		public static string PrintTree(List<LocalMavenArtifact> dependencies)
		{
			var text = new StringBuilder();

			foreach (var d in dependencies)
				PrintDependency(0, text, d);

			return text.ToString();
		}

		static void PrintDependency(int depth, StringBuilder text, LocalMavenArtifact dependency)
		{
			text.Append(new string(' ', depth * 4));
			text.Append($"{dependency.GroupId}:{dependency.ArtifactId}:{dependency.RequestedVersion}");
			if (!string.IsNullOrEmpty(dependency.ResolvedVersion))
				text.Append($" -> {dependency.ResolvedVersion}");
			text.AppendLine();

			if (dependency.Dependencies != null && dependency.Dependencies.Count > 0)
			{
				foreach (var d in dependency.Dependencies)
					PrintDependency(depth + 1, text, d);
			}
		}
	}
}
