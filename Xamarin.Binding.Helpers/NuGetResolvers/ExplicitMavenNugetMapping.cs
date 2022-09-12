namespace Xamarin.Binding.Helpers.NugetResolvers
{
	public class ExplicitMavenNugetMapping
	{
		public ExplicitMavenNugetMapping(string mavenGroupId, string mavenArtifactId, string mavenVersion, string nugetPackageId, string nugetVersion, bool ignore = false)
		{
			MavenGroupId = mavenGroupId;
			MavenArtifactId = mavenArtifactId;
			MavenVersion = mavenVersion;
			NuGetPackageId = nugetPackageId;
			NuGetVersion = nugetVersion;
			Ignore = ignore;
		}

		public string MavenGroupId { get; set; }
		public string MavenArtifactId { get; set; }
		public string MavenVersion { get; set; }

		public string NuGetPackageId { get; set; }
		public string NuGetVersion { get; set; }

		public bool Ignore { get;set; }
	}
}
