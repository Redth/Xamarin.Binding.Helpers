using System.Threading.Tasks;

namespace Xamarin.Binding.Helpers.NugetResolvers
{
	public class GooglePlayServicesMavenNugetResolver : MavenNugetResolver
	{
		public override async Task<NuGetSuggestion> Resolve(string mavenGroupId, string mavenArtifactId, string mavenRequestedVersion, string mavenResolvedVersion)
		{
			if (!mavenGroupId.StartsWith("com.google.android.gms"))
				return null;

			var packageId = "Xamarin.GooglePlayServices." + mavenArtifactId.Replace("play-services-", string.Empty).Replace("-", ".");

			var nugetVersion = await NuGetUtil.FindBestVersion(packageId, "1" + mavenRequestedVersion, "1" + mavenResolvedVersion);

			if (nugetVersion != null)
				return new NuGetSuggestion { PackageId = packageId, Version = nugetVersion.ToNormalizedString() };

			return null;
		}
	}
}
