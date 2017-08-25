/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.IO
{
	using System;
	using System.IO;
	using Microsoft.Practices.TransientFaultHandling;

	/// <summary>
	/// Helpers related to <see cref="System.IO.File"/> and <see cref="System.IO.Directory"/>.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// A <see cref="ITransientErrorDetectionStrategy"/> for App_Data transient errors.
		/// </summary>
		private class AppDataTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
		{
			/// <summary>
			/// Flags transient errors.
			/// </summary>
			/// <param name="ex">The error.</param>
			/// <returns>'true' if the error is transient.</returns>
			public bool IsTransient(Exception ex)
			{
				return ex is IOException
					|| ex is UnauthorizedAccessException;
			}
		}

		/// <summary>
		/// Creates a default <see cref="RetryPolicy"/>.
		/// </summary>
		/// <param name="retryStrategy">The retry strategy.</param>
		/// <returns>The retry policy.</returns>
		public static RetryPolicy CreateRetryPolicy(this RetryStrategy retryStrategy)
		{
			var detectionStrategy = new AppDataTransientErrorDetectionStrategy();
			return new RetryPolicy(detectionStrategy, retryStrategy);
		}

		/// <summary>
		/// Creates a <see cref="FileSystemWatcher"/> with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="path">The path.</param>
		/// <returns>The watcher.</returns>
		public static FileSystemWatcher CreateFileSystemWatcher(this RetryPolicy retryPolicy, string path)
		{
			return retryPolicy.ExecuteAction(() => new FileSystemWatcher(path)
			{
				InternalBufferSize = 1024 * 64,
				NotifyFilter = NotifyFilters.FileName,
				EnableRaisingEvents = true,
				IncludeSubdirectories = false,
			});
		}

		/// <summary>
		/// Creates a <see cref="DirectoryInfo"/> with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="path">The path.</param>
		/// <returns>The directory.</returns>
		public static DirectoryInfo GetDirectory(this RetryPolicy retryPolicy, string path)
		{
			return retryPolicy.ExecuteAction(() => new DirectoryInfo(path));
		}

		/// <summary>
		/// Calls Exists method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="path">The path.</param>
		/// <returns>'true' if the directory exists.</returns>
		public static bool DirectoryExists(this RetryPolicy retryPolicy, string path)
		{
			return retryPolicy.ExecuteAction(() => Directory.Exists(path));
		}

		/// <summary>
		/// Calls CreateDirectory method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="path">The path.</param>
		/// <returns>The directory.</returns>
		public static DirectoryInfo DirectoryCreate(this RetryPolicy retryPolicy, string path)
		{
			return retryPolicy.ExecuteAction(() => Directory.CreateDirectory(path));
		}

		/// <summary>
		/// Calls Delete method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="path">The path.</param>
		/// <param name="recursive">The flag to delete subdirectories.</param>
		public static void DirectoryDelete(this RetryPolicy retryPolicy, string path, bool recursive)
		{
			retryPolicy.ExecuteAction(() => Directory.Delete(path, recursive));
		}

		/// <summary>
		/// Calls GetDirectories method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="directory">The directory.</param>
		/// <param name="searchPattern">The filter.</param>
		/// <returns>The directories.</returns>
		public static DirectoryInfo[] GetDirectories(this RetryPolicy retryPolicy, DirectoryInfo directory, string searchPattern)
		{
			return retryPolicy.ExecuteAction(() => directory.GetDirectories(searchPattern));
		}

		/// <summary>
		/// Calls GetFiles method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="directory">The directory.</param>
		/// <param name="searchPattern">The filter.</param>
		/// <returns>The files.</returns>
		public static FileInfo[] GetFiles(this RetryPolicy retryPolicy, DirectoryInfo directory, string searchPattern)
		{
			return retryPolicy.ExecuteAction(() => directory.GetFiles(searchPattern));
		}

		/// <summary>
		/// Calls Exists method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="path">The path.</param>
		/// <returns>'true' if the file exists.</returns>
		public static bool FileExists(this RetryPolicy retryPolicy, string path)
		{
			return retryPolicy.ExecuteAction(() => File.Exists(path));
		}

		/// <summary>
		/// Calls Move method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="sourceFileName">The source path.</param>
		/// <param name="destFileName">The destination path.</param>
		public static void FileMove(this RetryPolicy retryPolicy, string sourceFileName, string destFileName)
		{
			retryPolicy.ExecuteAction(() => File.Move(sourceFileName, destFileName));
		}

		/// <summary>
		/// Calls Delete method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="path">The path.</param>
		public static void FileDelete(this RetryPolicy retryPolicy, string path)
		{
			retryPolicy.ExecuteAction(() => File.Delete(path));
		}

		/// <summary>
		/// Calls Delete method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="file">The file.</param>
		public static void FileDelete(this RetryPolicy retryPolicy, FileInfo file)
		{
			retryPolicy.ExecuteAction(file.Delete);
		}

		/// <summary>
		/// Calls Open method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="path">The path.</param>
		/// <param name="mode">The access mode.</param>
		/// <returns>The file stream.</returns>
		public static FileStream Open(this RetryPolicy retryPolicy, string path, FileMode mode)
		{
			return retryPolicy.ExecuteAction(() => File.Open(path, mode));
		}

		/// <summary>
		/// Calls OpenText method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="path">The path.</param>
		/// <returns>The file stream.</returns>
		public static TextReader OpenText(this RetryPolicy retryPolicy, string path)
		{
			return retryPolicy.ExecuteAction(() => File.OpenText(path));
		}

		/// <summary>
		/// Calls WriteAllBytes method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="path">The path.</param>
		/// <param name="bytes">The data.</param>
		public static void WriteAllBytes(this RetryPolicy retryPolicy, string path, byte[] bytes)
		{
			retryPolicy.ExecuteAction(() => File.WriteAllBytes(path, bytes));
		}

		/// <summary>
		/// Calls ReadAllBytes method with retries.
		/// </summary>
		/// <param name="retryPolicy">The retry policy.</param>
		/// <param name="path">The path.</param>
		/// <returns>The data.</returns>
		public static byte[] ReadAllBytes(this RetryPolicy retryPolicy, string path)
		{
			return retryPolicy.ExecuteAction(() => File.ReadAllBytes(path));
		}
	}
}
