/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services.Cache
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web.Hosting;
	using System.Web.Security;
	using Adxstudio.Xrm.AspNet;
	using Adxstudio.Xrm.IO;
	using Adxstudio.Xrm.Json;
	using Adxstudio.Xrm.Performance;
	using Adxstudio.Xrm.Threading;
	using Adxstudio.Xrm.Web;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// A job that warms up the cache from disk files.
	/// </summary>
	public class WarmupCacheJob : FluentSchedulerJob
	{
		/// <summary>
		/// The context.
		/// </summary>
		public CrmDbContext Context { get; private set; }

		/// <summary>
		/// The settings.
		/// </summary>
		public WarmupCacheSettings Settings { get; private set; }

		/// <summary>
		/// The full path of the input folder.
		/// </summary>
		public string AppDataFullPath { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WarmupCacheJob" /> class.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="settings">The settings.</param>
		public WarmupCacheJob(CrmDbContext context, WarmupCacheSettings settings)
		{
			this.Context = context;
			this.Settings = settings;
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

			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.Cache, PerformanceMarkerArea.Cms, PerformanceMarkerTagName.WarmupCache))
			{
				if (!this.Settings.AppDataRetryPolicy.DirectoryExists(this.AppDataFullPath))
				{
					return;
				}

				var directory = this.Settings.AppDataRetryPolicy.GetDirectory(this.AppDataFullPath);
				var files = directory.GetFiles(this.Settings.FilenameFormat.FormatWith("*"));
				var expiresOn = DateTimeOffset.UtcNow - this.Settings.ExpirationWindow;

				foreach (var file in files)
				{
					try
					{
						var expired = file.LastWriteTimeUtc < expiresOn;

						if (!expired)
						{
							ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Reading: " + file.FullName);

							var bytes = this.Settings.AppDataRetryPolicy.ReadAllBytes(file.FullName);
							var json = Encoding.UTF8.GetString(MachineKey.Unprotect(bytes, this.Settings.GetType().ToString()));
							var request = CrmJsonConvert.DeserializeObject(json) as OrganizationRequest;

							if (request != null)
							{
								this.Context.Service.ExecuteRequest(request);
							}
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
	}
}
