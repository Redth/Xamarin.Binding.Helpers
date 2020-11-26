using System;
using System.Collections.Generic;
using System.Text;
using NuGet.Protocol;
using NuGet.Packaging;
using NuGet.Repositories;
using NuGet.Protocol.Core.Types;
using NuGet.Common;
using System.Threading.Tasks;
using System.Threading;
using NuGet.Versioning;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Runtime.Versioning;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;

namespace Xamarin.Binding.Helpers
{
	public class NuGetUtil
	{
		public static async Task<IEnumerable<NuGetVersion>> GetVersions(string packageId)
		{
			var cache = new SourceCacheContext();
			var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
			FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

			var versions = await resource.GetAllVersionsAsync(
				packageId,
				cache,
				NullLogger.Instance,
				CancellationToken.None);

			return versions;
		}

		public static async Task<NuGetVersion> FindBestVersion(string packageId, string minVersion, string idealVersion = null, bool allowPrerelease = false)
		{
			var versions = await GetVersions(packageId);

			return FindBestVersion(versions, minVersion, idealVersion, allowPrerelease);

		}

		public static NuGetVersion FindBestVersion(IEnumerable<NuGetVersion> versions, string minVersion, string idealVersion = null, bool allowPrerelease = false)
		{
			NuGetVersion currentBest = null;

			if (versions != null && !string.IsNullOrEmpty(minVersion))
			{
				if (!NuGetVersion.TryParse(minVersion, out var semMinVer))
					return null;

				if (!NuGetVersion.TryParse(idealVersion, out var semIdealVer))
					semIdealVer = semMinVer;

				foreach (var version in versions)
				{
					// Don't initially pick anything that's below minimum version
					if (version < semMinVer)
						continue;

					// If it's in our range between min / ideal (inclusive) and not prerelease
					// Keep grabbing the higher version
					if (version >= semMinVer && version <= semIdealVer && !version.IsPrerelease && version > currentBest)
					{
						currentBest = version;
					}
					else
					{
						// If we have a version that's > our ideal version
						// we may still want it because it could be a .1 revision over the current best
						if (version >= semIdealVer)
						{
							// If 1.2.3.x and the maven version is 1.2.3, it means it's just a revision to the binding of 
							// the same maven artifact, so we want to prefer it in that case
							if (version.Major == semIdealVer.Major && version.Minor == semIdealVer.Minor && version.Patch == semIdealVer.Patch)
							{
								if (!version.IsPrerelease && version > currentBest)
									currentBest = version;
							}
							else
							{
								// We're on a version that is > the ideal, if it's stable, let's use that
								// as it's closest to what was requested, and we found nothing else
								// closer to either the min or ideal
								if (!version.IsPrerelease && currentBest == null)
									return version;
							}
						}
					}

				}

				// Return what we found, or the last stable, or the last whatever if nothing else
				if (currentBest == null)
				{
					// Try and find a stable to return that's newer than the min
					currentBest = versions.LastOrDefault(v => v >= semMinVer && !v.IsPrerelease);

					// If we're ok with prerelease as a last effort, find one newer than min
					// even allow a prerelease of the required stable version
					if (currentBest == null && allowPrerelease)
						currentBest = versions.LastOrDefault(v => v >= semMinVer
						|| (v.Major >= semMinVer.Major && v.Minor >= semMinVer.Minor && v.Patch >= semMinVer.Patch));
				}
			}

			return currentBest;
		}

		public static List<NuGetSuggestion> GetProjectPackages(string projectPath, string projectExtensionsPath, string targetFramework)
			=> GetProjectPackagesConfigPackages(projectPath)
				.Concat(GetProjectJsonPackages(projectExtensionsPath, targetFramework))
				.ToList();

		public static List<NuGetSuggestion> GetProjectPackagesConfigPackages(string projectPath)
		{
			var packages = new List<NuGetSuggestion>();

			var path = Path.Combine(projectPath, "packages.config");

			if (!File.Exists(path))
				return packages;

			var xdoc = XDocument.Load(path);

			var packageNodes = xdoc.XPathSelectElements("/packages/package");

			foreach (var pkgNode in packageNodes)
			{
				var nugetId = pkgNode?.Attribute("id")?.Value;
				var nugetVersion = pkgNode?.Attribute("version")?.Value;

				if (!packages.Any(p => p.PackageId.Equals(nugetId, StringComparison.OrdinalIgnoreCase)))
				{
					packages.Add(new NuGetSuggestion
					{
						PackageId = nugetId,
						Version = nugetVersion
					});
				}
			}

			return packages;
		}

		public static List<NuGetSuggestion> GetProjectJsonPackages(string projectExtensionsPath, string targetFramework)
		{
			var packages = new List<NuGetSuggestion>();

			var path = Path.Combine(projectExtensionsPath, "project.assets.json");

			if (!File.Exists(path))
				return packages;

			var json = JObject.Parse(File.ReadAllText(path));
			var tfm = NuGetFramework.Parse(targetFramework);

			var targets = json?["targets"];
			JToken projTarget = targets?[tfm.DotNetFrameworkName];

			foreach (var rf in projTarget)
			{
				if (rf.First()["type"].Value<string>().Equals("package", StringComparison.OrdinalIgnoreCase))
				{
					var str = (rf as JProperty)?.Name;

					var parts = str.Split(new[] { '/' }, 2);

					if (parts != null && parts.Length == 2)
					{
						var nugetId = parts[0];
						var nugetVersion = parts[1];

						if (!packages.Any(p => p.PackageId.Equals(nugetId, StringComparison.OrdinalIgnoreCase)))
						{
							packages.Add(new NuGetSuggestion
							{
								PackageId = nugetId,
								Version = nugetVersion
							});
						}
					}
				}
			}

			return packages;
		}

	}
}
