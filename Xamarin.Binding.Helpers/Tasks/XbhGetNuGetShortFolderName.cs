using System;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using NuGet.Frameworks;
using Xamarin.Build;

namespace Xamarin.Binding.Helpers.Tasks
{
	public class XbhGetNuGetShortFolderName : AsyncTask
	{
		[Required]
		public string TargetFrameworkMoniker { get; set; }
		
		public string TargetPlatformMoniker { get; set; }


		[Output]
		public string NuGetShortFolderName { get; set; }

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

		Task DoExecute()
		{
 			NuGetShortFolderName = NuGetFramework
				.ParseComponents(TargetFrameworkMoniker, TargetPlatformMoniker)
				.GetShortFolderName();

			return Task.CompletedTask;
		}
	}
}