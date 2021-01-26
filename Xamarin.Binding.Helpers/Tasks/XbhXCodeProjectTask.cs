using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MSBuildTaskItem = Microsoft.Build.Utilities.TaskItem;

namespace Xamarin.Binding.Helpers.Tasks
{
	public class XbhXCodeProjectTask : Build.AsyncTask
	{

		[Required]
		public string IntermediateOutputPath { get; set; }

		public ITaskItem[] Projects { get; set; }

		[Required]
		public string TargetFrameworkPath { get; set; }

		[Output]
		public ITaskItem[] NativeFrameworks { get; set; }

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
			foreach (var p in Projects)
			{
				var projectPath = p.ItemSpec;
				var generateBinding = false;

				bool.TryParse(p.GetMetadata("GenerateBinding"), out generateBinding);

				// TODO: 

				/*
				 * xcodebuild the project for each arch
				 * lipo the archs out to an intermediate path
				 * add the frameworks to the NativeFrameworks as outputs
				 * 
				 */
				
			}
		}
	}
}
