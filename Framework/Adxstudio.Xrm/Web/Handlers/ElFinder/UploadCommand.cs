/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	/// <summary>
	/// Handler for elFinder "upload" command.
	/// </summary>
	/// <remarks>
	/// File upload (http://elrte.org/redmine/projects/elfinder/wiki/Client-Server_Protocol_EN#upload).
	/// 
	/// Arguments (send using POST):
	/// 
	/// - cmd : upload
	/// - current : hash of directory where to upload files
	/// - upload : array of files to upload (upload[])
	/// 
	/// Response:
	/// 
	/// 1. If no files where uploaded return:
	/// 
	/// {
	///     "error" : "Unable to upload files" 
	/// }
	/// 
	/// 2. If at least one file was uploaded response with open and select. If some files failed to upload append error and errorData:
	/// 
	/// {
	///     // open
	///     "select"    : [ "8d331825ebfbe1ddae14d314bf81a712" ], // (Array)  array of hashes of files that were uploaded
	///     "error"     : "Some files was not uploaded",          // (String) return if not all files where uploaded
	///     "errorData" : {                                       // (Object) warnings which files were not uploaded
	///         "some-file.exe" : "Not allowed file type"         // (String) "file name": "error" 
	///     }
	/// }
	/// </remarks>
	public class UploadCommand : ICommand
	{
		public CommandResponse GetResponse(ICommandContext commandContext)
		{
			var fileSystem = commandContext.CreateFileSystem();

			var hash = commandContext.Parameters["current"];

			DirectoryContentHash cwd;

			if (!DirectoryContentHash.TryParse(hash, out cwd))
			{
				return new ErrorCommandResponse
				{
					error = ResourceManager.GetString("Unable_To_Retrieve_Current_Directory_For_Upload_Error")
				};
			}

			DirectoryUploadInfo cwdInfo;

			try
			{
				cwdInfo = fileSystem.Using(cwd, fs => new DirectoryUploadInfo(fs.Current));
			}
			catch (InvalidOperationException)
			{		
				return new ErrorCommandResponse
				{
					error = ResourceManager.GetString("Unable_To_Retrieve_Current_Directory_For_Upload_Error")
				};
			}

			if (!(cwdInfo.SupportsUpload && cwdInfo.CanWrite))
			{
				return new ErrorCommandResponse
				{
					error = ResourceManager.GetString("Upload_Permission_Denied_For_Current_Directory_Error")
				};
			}

			var files = GetUploadedFiles(commandContext.Files);
			
			if (!files.Any())
			{
				return new ErrorCommandResponse
				{
					error = "No valid files were uploaded"
				};
			}

			var portal = commandContext.CreatePortalContext();
			var publishingState = GetPublishingState(portal, cwdInfo.Entity);

			if (publishingState == null)
			{
				return new ErrorCommandResponse
				{
					error = ResourceManager.GetString("Unable_To_Retrieve_Current_Directory_For_Upload_Error")
				};
			}

			List<string> @select;
			List<Tuple<string, string>> errors;
			
			CreateFiles(commandContext, cwdInfo, files, publishingState, out @select, out errors);

			try
			{
				return fileSystem.Using(cwd, fs => GetResponse(commandContext, fs, @select, errors));
			}
			catch (InvalidOperationException)
			{
				return new ErrorCommandResponse
				{
					error = ResourceManager.GetString("Unable_To_Retrieve_Current_Directory_For_Upload_Error")
				};
			}
		}

		private CommandResponse GetResponse(ICommandContext commandContext, IFileSystemContext fileSystemContext, List<string> @select, List<Tuple<string, string>> errors)
		{
			var response = new OpenCommand().GetResponse(commandContext, fileSystemContext);

			response.select = select.ToArray();

			if (errors.Any())
			{
				var errorMessages = errors.Select(e => "[{0}: {1}]".FormatWith(e.Item1, e.Item2));

				response.error = ResourceManager.GetString("Uploading_Files_Error").FormatWith(string.Join(",", errorMessages));
			}

			return response;
		}

		private static void CreateFiles(ICommandContext commandContext, DirectoryUploadInfo uploadInfo, IEnumerable<HttpPostedFile> files, EntityReference publishingState, out List<string> @select, out List<Tuple<string, string>> errors)
		{
			@select = new List<string>();
			errors = new List<Tuple<string, string>>();
			
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies();
			var annotationDataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
			var website = HttpContext.Current.GetWebsite();

			var location = website.Settings.Get<string>("WebFiles/StorageLocation");

			StorageLocation storageLocation;
			if (!Enum.TryParse(location, true, out storageLocation))
			{
				storageLocation = StorageLocation.CrmDocument;
			}

			var maxFileSizeErrorMessage = website.Settings.Get<string>("WebFiles/MaxFileSizeErrorMessage");

			var annotationSettings = new AnnotationSettings(dataAdapterDependencies.GetServiceContext(),
				storageLocation: storageLocation, maxFileSizeErrorMessage: maxFileSizeErrorMessage);
						
			foreach (var file in files)
			{
				var serviceContext = commandContext.CreateServiceContext();

				try
				{
					var webFile = new Entity("adx_webfile");

					var fileName = Path.GetFileName(file.FileName);

					webFile.Attributes["adx_name"] = fileName;
					webFile.Attributes["adx_partialurl"] = GetPartialUrlFromFileName(fileName);
					webFile.Attributes["adx_websiteid"] = website.Entity.ToEntityReference();
					webFile.Attributes["adx_publishingstateid"] = publishingState;
					webFile.Attributes["adx_hiddenfromsitemap"] = true;
					webFile.Attributes[uploadInfo.WebFileForeignKeyAttribute] = uploadInfo.EntityReference;

					serviceContext.AddObject(webFile);
					serviceContext.SaveChanges();

					annotationDataAdapter.CreateAnnotation(new Annotation
					{
						Regarding = webFile.ToEntityReference(),
						FileAttachment = AnnotationDataAdapter.CreateFileAttachment(new HttpPostedFileWrapper(file), annotationSettings.StorageLocation)
					}, annotationSettings);

					@select.Add(new DirectoryContentHash(webFile.ToEntityReference()).ToString());
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format(@"Exception uploading file: {0}",  e.ToString()));

                    errors.Add(new Tuple<string, string>(file.FileName, e.Message));
				}
			}
		}

		private static string GetPartialUrlFromFileName(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			return HttpUtility.UrlPathEncode(fileName);
		}

		private static EntityReference GetPublishingState(IPortalContext portal, Entity directory)
		{
			// Load all publishing states in website.
			var publishingStates = portal.ServiceContext.CreateQuery("adx_publishingstate")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == portal.Website.ToEntityReference())
				.ToList();

			// Favour a publishing state that is both visible and the default.
			var visibleAndDefaultState = publishingStates.FirstOrDefault(e => e.GetAttributeValue<bool?>("adx_isvisible").GetValueOrDefault() && e.GetAttributeValue<bool?>("adx_isdefault").GetValueOrDefault());

			if (visibleAndDefaultState != null)
			{
			    return visibleAndDefaultState.ToEntityReference();
			}

			// Next best, try to use the publishing state of the parent directory.
			if (directory.Attributes.Contains("adx_publishingstateid"))
			{
				var directoryPublishingState = directory.GetAttributeValue<EntityReference>("adx_publishingstateid");

				if (directoryPublishingState != null)
				{
					return directoryPublishingState;
				}
			}

			// Next, just try for the default.
			var defaultState = publishingStates.FirstOrDefault(e => e.GetAttributeValue<bool?>("adx_isdefault").GetValueOrDefault());

			if (defaultState != null)
			{
			    return defaultState.ToEntityReference();
			}

			// Failing that, try for visible.
			var visibleState = publishingStates.FirstOrDefault(e => e.GetAttributeValue<bool?>("adx_isvisible").GetValueOrDefault());

			if (visibleState != null)
			{
			    return visibleState.ToEntityReference();
			}

			// Failing that, settle for any state.
			var firstState = publishingStates.FirstOrDefault();

			if (firstState != null)
			{
			    return firstState.ToEntityReference();
			}

			return null;
		}

		private static HttpPostedFile[] GetUploadedFiles(HttpFileCollection files)
		{
			var validFiles = new List<HttpPostedFile>();

			for (var i = 0; i < files.Count; i++)
			{
				var key = files.GetKey(i);

				if (key != "upload[]")
				{
					continue;
				}

				var file = files[i];

				if (string.IsNullOrWhiteSpace(file.FileName))
				{
					continue;
				}

				validFiles.Add(file);
			}

			return validFiles.ToArray();
		}

		private class DirectoryUploadInfo
		{
			public DirectoryUploadInfo(IDirectory directory)
			{
				CanWrite = directory.CanWrite;
				Entity = directory.Entity;
				EntityReference = directory.EntityReference;
				SupportsUpload = directory.SupportsUpload;
				WebFileForeignKeyAttribute = directory.WebFileForeignKeyAttribute;
			}

			public bool CanWrite { get; private set; }

			public Entity Entity { get; private set; }

			public EntityReference EntityReference { get; private set; }

			public bool SupportsUpload { get; private set; }

			public string WebFileForeignKeyAttribute { get; private set; }
		}
	}
}
