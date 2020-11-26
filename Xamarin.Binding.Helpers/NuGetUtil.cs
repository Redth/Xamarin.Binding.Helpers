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

			if (versions != null)
			{
				SemanticVersion.TryParse(minVersion, out var semMinVer);
				SemanticVersion.TryParse(idealVersion, out var semIdealVer);

				//foreach (var version in versions)
				//{
				//	if (version == semIdealVer)
				//		return version;
				//}

				foreach (var version in versions.Reverse())
				{
					if (!allowPrerelease && version.IsPrerelease)
						continue;

					if (semIdealVer != null && version >= semIdealVer)
						return version;
					if (semMinVer != null && version >= semMinVer)
						return version;
				}

				return versions.FirstOrDefault();
			}

			return null;
		}

		public static List<NuGetSuggestion> GetProjectPackages(string projectPath, string projectExtensionsPath, string targetFramework)
			=> GetProjectPackagesConfigPackages(projectPath)
				.Concat(GetProjectJsonPackages(projectExtensionsPath, targetFramework))
				.ToList();

		public static List<NuGetSuggestion> GetProjectPackagesConfigPackages(string projectPath)
		{
			var packages = new List<NuGetSuggestion>();

			var path = Path.Combine(projectPath, "packages.config");

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

			JObject json = JObject.Parse(File.ReadAllText(path));


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
