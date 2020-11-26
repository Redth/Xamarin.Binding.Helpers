using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Binding.Helpers.NugetResolvers;

namespace Xamarin.Binding.Helpers
{
	public class MavenNuGetSomelier
	{
		public static async Task<NuGetSuggestion> SuggestNuget(LocalMavenArtifact dependency, params MavenNugetResolver[] resolvers)
		{
			foreach (var r in resolvers)
			{
				var n = await r.Resolve(dependency.GroupId, dependency.ArtifactId, dependency.RequestedVersion, dependency.ResolvedVersion);

				if (n != null)
					return n;
			}

			return null;
		}

		public static async Task<IEnumerable<MavenArtifactNuGetPairing>> MapNuGets(IEnumerable<LocalMavenArtifact> mavenDependencies, params MavenNugetResolver[] resolvers)
		{
			var results = new List<MavenArtifactNuGetPairing>();

			foreach (var mavenDep in mavenDependencies)
				await MapNuGets(mavenDep, results, resolvers);

			return results;
		}

		static async Task MapNuGets(LocalMavenArtifact mavenDep, List<MavenArtifactNuGetPairing> bindingDeps, params MavenNugetResolver[] resolvers)
		{
			var nuget = await SuggestNuget(mavenDep, resolvers);

			// Did we already add one?
			if (!bindingDeps.Any(bd => bd.MavenDependency.GroupId.Equals(mavenDep.GroupId, StringComparison.OrdinalIgnoreCase) && bd.MavenDependency.ArtifactId.Equals(mavenDep.ArtifactId, StringComparison.OrdinalIgnoreCase)))
			{
				bindingDeps.Add(new MavenArtifactNuGetPairing
				{
					NuGet = await SuggestNuget(mavenDep, resolvers),
					MavenDependency = mavenDep
				});

				if (nuget == null)
				{
					foreach (var child in mavenDep.Dependencies)
					{
						await MapNuGets(child, bindingDeps, resolvers);
					}
				}
			}
		}
	}
}
