﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Binding.Helpers
{
	internal class ProcessArgumentBuilder
	{
		public List<string> args = new List<string>();

		public ProcessArgumentBuilder Append(string arg)
		{
			args.Add(arg);
			return this;
		}

		public ProcessArgumentBuilder AppendQuoted(string arg)
		{
			args.Add($"\"{arg}\"");
			return this;
		}

		public override string ToString()
			=> string.Join(" ", args);
	}

	internal class ProcessRunner
	{
		public static ProcessResult Run(FileInfo exe, ProcessArgumentBuilder builder, DirectoryInfo workingDirectory = null)
		{
			var p = new ProcessRunner(exe, builder, workingDirectory);
			return p.WaitForExit();
		}

		public static Task<ProcessResult> RunAsync(FileInfo exe, ProcessArgumentBuilder builder, DirectoryInfo workingDirectory = null)
		{
			var p = new ProcessRunner(exe, builder, workingDirectory);
			return p.WaitForExitAsync();
		}

		readonly List<string> standardOutput;
		readonly List<string> standardError;
		readonly Process process;

		public ProcessRunner(FileInfo executable, ProcessArgumentBuilder builder, DirectoryInfo workingDirectory = null)
			: this(executable, builder, System.Threading.CancellationToken.None, false, workingDirectory)
		{ }

		public ProcessRunner(FileInfo executable, ProcessArgumentBuilder builder, System.Threading.CancellationToken cancelToken, bool redirectStandardInput = false, DirectoryInfo workingDirectory = null)
		{
			standardOutput = new List<string>();
			standardError = new List<string>();

			//* Create your Process
			process = new Process();
			process.StartInfo.FileName = executable.FullName;
			process.StartInfo.Arguments = builder.ToString();
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;

			if (workingDirectory != null && workingDirectory.Exists)
				process.StartInfo.WorkingDirectory = workingDirectory.FullName;

			if (redirectStandardInput)
				process.StartInfo.RedirectStandardInput = true;

			process.OutputDataReceived += (s, e) =>
			{
				if (e.Data != null)
					standardOutput.Add(e.Data);
			};
			process.ErrorDataReceived += (s, e) =>
			{
				if (e.Data != null)
					standardError.Add(e.Data);
			};
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			if (cancelToken != System.Threading.CancellationToken.None)
			{
				cancelToken.Register(() =>
				{
					try { process.Kill(); }
					catch { }
				});
			}
		}

		public int ExitCode
			=> process.HasExited ? process.ExitCode : -1;

		public bool HasExited
			=> process?.HasExited ?? false;

		public void Kill()
			=> process?.Kill();

		public void StandardInputWrite(string input)
		{
			if (!process.StartInfo.RedirectStandardInput)
				throw new InvalidOperationException();

			process.StandardInput.Write(input);
		}

		public void StandardInputWriteLine(string input)
		{
			if (!process.StartInfo.RedirectStandardInput)
				throw new InvalidOperationException();

			process.StandardInput.WriteLine(input);
		}

		public ProcessResult WaitForExit()
		{
			process.WaitForExit();

			return new ProcessResult(standardOutput, standardError, process.ExitCode);
		}

		public Task<ProcessResult> WaitForExitAsync()
		{
			var tcs = new TaskCompletionSource<ProcessResult>();

			Task.Run(() =>
			{
				var r = WaitForExit();
				tcs.TrySetResult(r);
			});

			return tcs.Task;
		}
	}

	public class ProcessResult
	{
		public readonly List<string> StandardOutput;
		public readonly List<string> StandardError;

		public IEnumerable<string> StandardCombined
			=> (StandardError ?? Enumerable.Empty<string>())
			.Concat(StandardOutput ?? Enumerable.Empty<string>());

		public string StandardCombinedText
			=> string.Join(Environment.NewLine, StandardCombined);

		public readonly int ExitCode;

		public bool Success
			=> ExitCode == 0;

		internal ProcessResult(List<string> stdOut, List<string> stdErr, int exitCode)
		{
			StandardOutput = stdOut;
			StandardError = stdErr;
			ExitCode = exitCode;
		}
	}
}
