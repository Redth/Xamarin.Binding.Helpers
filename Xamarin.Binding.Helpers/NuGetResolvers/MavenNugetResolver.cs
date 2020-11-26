using System.Threading.Tasks;

namespace Xamarin.Binding.Helpers.NugetResolvers
{
	public abstract class MavenNugetResolver
	{
		public abstract Task<NuGetSuggestion> Resolve(string mavenGroupId, string mavenArtifactId, string mavenRequestedVersion, string mavenResolvedVersion);
	}
}
