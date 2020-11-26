using System;
using System.Threading.Tasks;

namespace Xamarin.Binding.Helpers.NugetResolvers
{
	public class AndroidXMavenNugetResolver : MavenNugetResolver
	{
		public override async Task<NuGetSuggestion> Resolve(string mavenGroupId, string mavenArtifactId, string mavenRequestedVersion, string mavenResolvedVersion)
		{
			if (!mavenGroupId.StartsWith("androidx."))
			{
				// Material design is a different pattern
				if (mavenGroupId.Equals("com.google.android.material", StringComparison.OrdinalIgnoreCase)
					&& mavenArtifactId.Equals("material", StringComparison.OrdinalIgnoreCase))
				{
					var materialPkgId = "Xamarin.Google.Android.Material";
					var materialNugetVersion = await NuGetUtil.FindBestVersion(materialPkgId, mavenRequestedVersion, mavenResolvedVersion, true);

					if (materialNugetVersion != null)
						return new NuGetSuggestion { PackageId = materialPkgId, Version = materialNugetVersion.ToNormalizedString() };
				}

				return null;
			}

			var firstDashIndex = mavenArtifactId.IndexOf('-');

			var packageId = mavenGroupId + "." + mavenArtifactId.Replace("-", ".");


			if (firstDashIndex > 0 && mavenGroupId.EndsWith(mavenArtifactId.Substring(0, firstDashIndex)))
				packageId = mavenGroupId + "." + mavenArtifactId.Substring(firstDashIndex + 1).Replace("-", ".");
			else if (mavenGroupId.EndsWith(mavenArtifactId, StringComparison.OrdinalIgnoreCase))
				packageId = mavenGroupId;

			packageId = "xamarin." + packageId;

			var nugetVersion = await NuGetUtil.FindBestVersion(packageId, mavenRequestedVersion, mavenResolvedVersion);

			if (nugetVersion != null)
				return new NuGetSuggestion { PackageId = packageId, Version = nugetVersion.ToNormalizedString() };

			return null;
		}
	}
}
