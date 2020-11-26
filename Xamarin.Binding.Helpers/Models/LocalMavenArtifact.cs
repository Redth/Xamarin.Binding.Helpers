using System;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.Binding.Helpers
{
	public class LocalMavenArtifact
	{
		public string GroupId { get; set; }
		public string ArtifactId { get; set; }
		public string RequestedVersion { get; set; }
		public string ResolvedVersion { get; set; }
		public string File { get; set; }

		bool? isAar;

		public bool IsAar
		{
			get
			{
				if (!isAar.HasValue)
					isAar = !string.IsNullOrEmpty(File) && Path.GetExtension(File).Equals(".aar", StringComparison.OrdinalIgnoreCase);

				return isAar.Value;
			}
		}

		bool? isJar;

		public bool IsJar
		{
			get
			{
				if (!isJar.HasValue)
					isJar = !string.IsNullOrEmpty(File) && Path.GetExtension(File).Equals(".jar", StringComparison.OrdinalIgnoreCase);

				return isJar.Value;
			}
		}

		public LocalMavenArtifact Parent { get; set; } = null;

		public int Depth { get; set; } = 0;

		public List<LocalMavenArtifact> Dependencies { get; set; } = new List<LocalMavenArtifact>();

		public override string ToString()
			=> $"{GroupId}:{ArtifactId} -> {RequestedVersion} ({ResolvedVersion}) -> {Depth}";
	}
}
