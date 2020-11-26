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
	}
}
