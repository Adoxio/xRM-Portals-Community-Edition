/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services.Cache
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Caching;
	using System.Text;
	using System.Web.Hosting;
	using System.Web.Security;
	using Adxstudio.Xrm.IO;
	using Adxstudio.Xrm.Json;
	using Adxstudio.Xrm.Performance;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Threading;
	using Adxstudio.Xrm.Web;
	using Microsoft.Xrm.Client;

	/// <summary>
	/// A job that persists cache item requests to disk.
	/// </summary>
	public class PersistCachedRequestsJob : FluentSchedulerJob
	{
		/// <summary>
		/// The settings.
		/// </summary>
		public WarmupCacheSettings Settings { get; private set; }

		/// <summary>
		/// The full path of the output folder.
		/// </summary>
		public string AppDataFullPath { get; private set; }

		/// <summary>
		/// Reference to the object cache.
		/// </summary>
		public ObjectCache ObjectCache { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PersistCachedRequestsJob" /> class.
		/// </summary>
		/// <param name="objectCache">Reference to the ObjectCache.</param>
		/// <param name="settings">The settings.</param>
		public PersistCachedRequestsJob(ObjectCache objectCache, WarmupCacheSettings settings)
		{
			this.Settings = settings;
			this.ObjectCache = objectCache;
			this.AppDataFullPath = settings.AppDataPath.StartsWith("~/")
				? HostingEnvironment.MapPath(settings.AppDataPath)
				: settings.AppDataPath;
		}

		/// <summary>
		/// The body.
		/// </summary>
		/// <param name="id">The activity id.</param>
		protected override void ExecuteInternal(Guid id)
		{
			var exceptions = new List<Exception>();

			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.Cache, PerformanceMarkerArea.Cms, PerformanceMarkerTagName.PersistCachedRequests))
			{
				this.Settings.AppDataRetryPolicy.DirectoryCreate(this.AppDataFullPath);

				// find cached requests for writing to disk
				foreach (var telemetry in this.ObjectCache.Select(item => item.Value).OfType<CacheItemTelemetry>())
				{
					if (telemetry.Request != null)
					{
						this.Save(this.AppDataFullPath, telemetry);
					}
				}

				// cleanup exipired files
				var directory = this.Settings.AppDataRetryPolicy.GetDirectory(this.AppDataFullPath);
				var files = this.Settings.AppDataRetryPolicy.GetFiles(directory, this.Settings.FilenameFormat.FormatWith("*"));
				var expiresOn = DateTimeOffset.UtcNow - this.Settings.ExpirationWindow;

				foreach (var file in files)
				{
					try
					{
						var expired = file.LastWriteTimeUtc < expiresOn;

						if (expired)
						{
							ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Deleting: " + file.FullName);

							this.Settings.AppDataRetryPolicy.FileDelete(file);
						}
					}
					catch (Exception e)
					{
						WebEventSource.Log.GenericWarningException(e);

						if (this.Settings.PropagateExceptions)
						{
							exceptions.Add(e);
						}
					}
				}
			}

			if (this.Settings.PropagateExceptions && exceptions.Any())
			{
				throw new AggregateException(exceptions);
			}
		}

		/// <summary>
		/// Error event.
		/// </summary>
		/// <param name="e">The error.</param>
		protected override void OnError(Exception e)
		{
			WebEventSource.Log.GenericWarningException(e);
		}

		/// <summary>
		/// Write this item to a folder.
		/// </summary>
		/// <param name="folderPath">The folder path.</param>
		/// <param name="telemetry">The cache item telemetry.</param>
		private void Save(string folderPath, CacheItemTelemetry telemetry)
		{
			if (telemetry.Request != null)
			{
				var request = CrmJsonConvert.SerializeObject(telemetry.Request);
				var key = request.GetHashCode();
				var filename = string.Format(this.Settings.FilenameFormat, key);
				var fullPath = Path.Combine(folderPath, filename);
				var bytes = MachineKey.Protect(Encoding.UTF8.GetBytes(request), this.Settings.GetType().ToString());

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Writing: " + fullPath);

				this.Settings.AppDataRetryPolicy.WriteAllBytes(fullPath, bytes);
			}
		}
	}
}
