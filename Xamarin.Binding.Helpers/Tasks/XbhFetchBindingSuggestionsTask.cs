using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Build;
using MSBuildTaskItem = Microsoft.Build.Utilities.TaskItem;

namespace Xamarin.Binding.Helpers.Tasks
{
	public class XbhFetchBindingSuggestionsTask : AsyncTask
	{
		public ITaskItem[] Projects { get; set; }

		[Output]
		public ITaskItem[] AndroidAars { get; set; }

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

			foreach (var proj in Projects)
			{
				var intermediateOutputPath = proj.GetMetadata("IntermediateOutputPath");

				var packageRefs = Path.Combine(intermediateOutputPath, "xbh", "_xbhpackagerefs.txt");
				
				var pkgRefs = new List<NuGetSuggestion>();

				// PackageId|Version
				if (File.Exists(packageRefs))
				{
					var lines = File.ReadAllLines(packageRefs) ?? new string[0];

					foreach (var line in lines)
					{
						if (string.IsNullOrEmpty(line))
							continue;

						var parts = line?.Split(new[] { '|' }, 2);
						if ((parts?.Length ?? 0) == 2)
							pkgRefs.Add(new NuGetSuggestion
							{
								PackageId = parts[0],
								Version = parts[1]
							});
					}
				}

				var artifacts = Path.Combine(intermediateOutputPath, "_xbhartifacts.txt");

				if (File.Exists(artifacts))
				{
					var lines = File.ReadAllLines(artifacts) ?? new string[0];

					foreach (var line in lines)
					{
						if (string.IsNullOrEmpty(line))
							continue;

						aars.Add(new MSBuildTaskItem(line));
					}
				}

			}

			AndroidAars = aars.ToArray();
		}
	}
}
