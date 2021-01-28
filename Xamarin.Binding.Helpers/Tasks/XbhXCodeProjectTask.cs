using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MSBuildTaskItem = Microsoft.Build.Utilities.TaskItem;

namespace Xamarin.Binding.Helpers.Tasks
{
	public class XbhXCodeProjectTask : Build.AsyncTask
	{

		[Required]
		public string IntermediateOutputPath { get; set; }

		[Required]
		public string SdkBinPath { get; set; }

		public ITaskItem[] Projects { get; set; }

		[Required]
		public string TargetFrameworkPath { get; set; }

		[Output]
		public ITaskItem[] NativeFrameworks { get; set; }


		DirectoryInfo fullIntermediateOutputPath;

		public override bool Execute()
		{
			Task.Run(async () =>
			{
				try
				{
					fullIntermediateOutputPath = new DirectoryInfo(IntermediateOutputPath);
					if (!fullIntermediateOutputPath.Exists)
						fullIntermediateOutputPath.Create();

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
			var xcodebuildPath = new FileInfo(Path.Combine(SdkBinPath, "xcodebuild"));

			var nativeRefs = new List<MSBuildTaskItem>();

			foreach (var p in Projects)
			{
				var projectPath = new FileInfo(p.ItemSpec);
				var generateBinding = false;

				bool.TryParse(p.GetMetadata("GenerateBinding"), out generateBinding);

				/*
				 * xcodebuild the project for each arch
				 * lipo the archs out to an intermediate path
				 * add the frameworks to the NativeFrameworks as outputs
				 * 
				 */
				var buildSettingsJsonStr = (await XCodeBuild(xcodebuildPath, projectPath, "-showBuildSettings", "-json"))
					?.StandardCombinedText;

				var buildSettings = JArray.Parse(buildSettingsJsonStr ?? "[]")?[0]?["buildSettings"];

				var bsProjectName = buildSettings?["PROJECT_NAME"]?.Value<string>()
					?? Path.GetFileNameWithoutExtension(projectPath.Name);

				var bsFullProductName = buildSettings?["FULL_PRODUCT_NAME"]?.Value<string>()
					?? $"{bsProjectName}.framework";
					

				var libPath = buildSettings?["EXECUTABLE_PATH"]?.Value<string>()
					?? bsFullProductName + "/" + bsProjectName;

				// Build arm64
				await XCodeBuild(xcodebuildPath, projectPath, "iphoneos", "arm64");

				var libArm64 = Path.Combine(projectPath.DirectoryName, "build", "Release-iphoneos", libPath);

				// Build x86_64
				await XCodeBuild(xcodebuildPath, projectPath, "iphonesimulator", "x86_64");

				var libX64 = Path.Combine(projectPath.DirectoryName, "build", "Release-iphonesimulator", libPath);

				// FAT file destination
				var libFat = Path.Combine(fullIntermediateOutputPath.FullName, "xcode", libPath);

				// Copy this to the obj/ intermediate final path for the fat file
				// We can lipo to the same file as the output, slightly more efficient?
				RecursiveDirectoryCopy(libX64, libFat);

				// Lipo the arm64 into the copied x86_64 we just did
				var pargs = new ProcessArgumentBuilder();
				pargs.Append("-create");
				pargs.Append("-o");
				pargs.AppendQuoted(libFat);
				pargs.AppendQuoted(libArm64);

				await ProcessRunner.RunAsync(new FileInfo("/usr/bin/lipo"), pargs);

				// Add to the outputs
				nativeRefs.Add(new MSBuildTaskItem(libFat));
			}

			NativeFrameworks = nativeRefs.ToArray();
		}

		Task<ProcessResult> XCodeBuild(FileInfo xcodebuildPath, FileInfo xcodeprojPath, params string[] args)
        {
			var parg = new ProcessArgumentBuilder();

			foreach (var a in args)
				parg.Append(a);

			parg.Append("-project");
			parg.AppendQuoted(xcodeprojPath.FullName);

			return ProcessRunner.RunAsync(xcodebuildPath, parg, xcodeprojPath.Directory);
		}

		static void RecursiveDirectoryCopy(string sourceDirName, string destDirName)
		{
			// Get the subdirectories for the specified directory.
			var dir = new DirectoryInfo(sourceDirName);

			var dirs = dir.GetDirectories();

			// If the destination directory doesn't exist, create it.       
			Directory.CreateDirectory(destDirName);

			// Get the files in the directory and copy them to the new location.
			var files = dir.GetFiles();
			foreach (var file in files)
			{
				var tempPath = Path.Combine(destDirName, file.Name);
				file.CopyTo(tempPath, false);
			}

			// If copying subdirectories, copy them and their contents to new location.
			foreach (var subdir in dirs)
			{
				string tempPath = Path.Combine(destDirName, subdir.Name);
				RecursiveDirectoryCopy(subdir.FullName, tempPath);
			}
		}
	}
}
