using NuGet.Frameworks;

namespace Xamarin.Binding.Helpers
{
	public class NuGetSuggestion
	{
		public string PackageId { get; set; }
		public string Version { get; set; }

		public NuGetFramework Framework { get; set; }

	}
}
