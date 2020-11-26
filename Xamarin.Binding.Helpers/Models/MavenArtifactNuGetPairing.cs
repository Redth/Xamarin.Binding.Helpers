namespace Xamarin.Binding.Helpers
{
	public class MavenArtifactNuGetPairing
	{
		public NuGetSuggestion NuGet { get; set; }
		
		public LocalMavenArtifact MavenDependency { get; set; }
	}
}
