using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Binding.Helpers.NugetResolvers
{
	public class ExplicitMavenNugetResolver : MavenNugetResolver
	{
		public List<ExplicitMavenNugetMapping> Mappings { get; set; } = new List<ExplicitMavenNugetMapping>();

		public ExplicitMavenNugetResolver()
		{
		}

		public ExplicitMavenNugetResolver(IEnumerable<ExplicitMavenNugetMapping> mappings)
		{
			Mappings.AddRange(mappings);
		}

		public ExplicitMavenNugetResolver(params ExplicitMavenNugetMapping[] mappings)
		{
			Mappings.AddRange(mappings);
		}

		public override async Task<NuGetSuggestion> Resolve(string mavenGroupId, string mavenArtifactId, string mavenRequestedVersion, string mavenResolvedVersion)
		{
			foreach (var m in Mappings)
			{
				// Empty nuget package id means don't resolve
				if (string.IsNullOrEmpty(m.NuGetPackageId))
					return null;

				// If no nuget version specified, try latest
				if (string.IsNullOrEmpty(m.NuGetVersion))
				{
					m.NuGetVersion = (await NuGetUtil.GetVersions(m.NuGetPackageId))?.LastOrDefault()?.ToNormalizedString();
					if (string.IsNullOrEmpty(m.NuGetVersion))
						return null;
				}

				// Are group and artifact id's equal?
				if (m.MavenGroupId.Equals(mavenGroupId, StringComparison.OrdinalIgnoreCase)
					&& m.MavenArtifactId.Equals(mavenArtifactId, StringComparison.OrdinalIgnoreCase))
				{
					// make sure we can parse the maven version of the mapping
					if (SemanticVersion.TryParse(m.MavenVersion, out var mapMavenSemVer))
					{
						// Check two possibilities:
						// 1. The map version is >= resolved maven version 
						// 2. The map version is >= than the requested maven version
						if ((SemanticVersion.TryParse(mavenResolvedVersion, out var mavenResSemver)
							&& mapMavenSemVer >= mavenResSemver)
							|| (SemanticVersion.TryParse(mavenRequestedVersion, out var mavenReqSemver)
							&& mapMavenSemVer >= mavenReqSemver))
							return new NuGetSuggestion
							{
								PackageId = m.NuGetPackageId,
								Version = m.NuGetVersion
							};
					}
					else
					{
						return new NuGetSuggestion
							{
								PackageId = m.NuGetPackageId,
								Version = m.NuGetVersion
							};
					}
					
				}
			}

			return null;
		}
	}
}
