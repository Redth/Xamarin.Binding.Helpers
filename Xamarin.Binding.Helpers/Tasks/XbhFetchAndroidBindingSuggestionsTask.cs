using Microsoft.Build.Framework;
using Newtonsoft.Json;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Build;
using MSBuildTaskItem = Microsoft.Build.Utilities.TaskItem;

namespace Xamarin.Binding.Helpers.Tasks
{
	public class XbhFetchAndroidBindingSuggestionsTask : AsyncTask
	{
		public ITaskItem[] SuggestionsFile { get; set; }

		[Output]
		public ITaskItem[] AndroidAars { get; set; }

		[Required]
		public string TargetFramework { get; set; }
		
		[Required]
		public string ProjectDirectory { get; set; }

		[Required]
		public string ProjectExtensionsPath { get; set; }

		public override bool Execute()
		{
			Task.Run(async () =>
			{
				try
				{
					await DoExecute();
				}
				catch (Exception ex)
				{
					Log.LogErrorFromException(ex);
				}
				finally
				{
					Complete();
				}

			});

			return base.Execute();
		}

		async Task DoExecute()
		{
			var aars = new List<MSBuildTaskItem>();
			var missingNugets = new List<NuGetSuggestion>();
			var oldNugets = new List<NuGetSuggestion>();

			foreach (var suggestionFile in SuggestionsFile)
			{

				var suggestionsJson = File.ReadAllText(suggestionFile.ItemSpec);

				var suggestions = JsonConvert.DeserializeObject<AndroidSuggestions>(suggestionsJson);

				foreach (var a in suggestions.LocalArtifacts)
					aars.Add(new MSBuildTaskItem(a.MavenDependency.File));

				foreach (var aar in suggestions.LocalBindableArtifacts)
					aars.Add(new MSBuildTaskItem(aar));

				// TODO: Check nugets

				var referencedNugets = NuGetUtil.GetProjectPackages(
					ProjectDirectory,
					ProjectExtensionsPath,
					TargetFramework);

				foreach (var n in suggestions.NuGets)
				{
					var refd = referencedNugets.FirstOrDefault(r => r.PackageId.Equals(n.NuGet.PackageId, StringComparison.OrdinalIgnoreCase));

					if (refd == null)
					{
						missingNugets.Add(n.NuGet);
						continue;
					}

					var refdVersion = NuGetVersion.Parse(refd.Version);
					var suggVersion = NuGetVersion.Parse(n.NuGet.Version);

					if (refdVersion < suggVersion)
						oldNugets.Add(n.NuGet);
				}
			}

			if (missingNugets.Any())
			{
				var sb = new StringBuilder();

				sb.AppendLine("Your project is missing these NuGet package references:");

				foreach (var n in missingNugets)
					sb.AppendLine($"\t<PackageReference Include=\"{n.PackageId}\" Version=\"{n.Version}\" />");

				LogError(sb.ToString());
			}

			if (oldNugets.Any())
			{
				var sb = new StringBuilder();

				sb.AppendLine("Your project has references to older NuGet package references and requires updates.  Please update the following packages to the versions listed:");

				foreach (var n in missingNugets)
					sb.AppendLine($"\t<PackageReference Include=\"{n.PackageId}\" Version=\"{n.Version}\" />");

				LogError(sb.ToString());
			}

			AndroidAars = aars.ToArray();
		}
	}
}
