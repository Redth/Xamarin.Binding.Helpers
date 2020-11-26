using System.Threading.Tasks;

namespace Xamarin.Binding.Helpers.NugetResolvers
{
	public class FirebaseMavenNugetResolver : MavenNugetResolver
	{
		public override async Task<NuGetSuggestion> Resolve(string mavenGroupId, string mavenArtifactId, string mavenRequestedVersion, string mavenResolvedVersion)
		{
			if (!mavenGroupId.StartsWith("com.google.firebase"))
				return null;

			var packageId = "Xamarin.Firebase." + mavenArtifactId.Replace("firebase-", string.Empty).Replace("-", ".");

			var nugetVersion = await NuGetUtil.FindBestVersion(packageId, "1" + mavenRequestedVersion, "1" + mavenResolvedVersion);

			if (nugetVersion != null)
				return new NuGetSuggestion { PackageId = packageId, Version = nugetVersion.ToNormalizedString() };

			return null;
		}
	}
}
