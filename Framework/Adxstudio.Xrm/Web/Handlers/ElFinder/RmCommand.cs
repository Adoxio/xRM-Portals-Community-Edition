/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web.Data.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	/// <summary>
	/// Handler for elFinder "rm" command.
	/// </summary>
	/// <remarks>
	/// Delete file or directory (recursive) (http://elrte.org/redmine/projects/elfinder/wiki/Client-Server_Protocol_EN#rm).
	/// 
	/// Arguments:
	/// 
	/// - cmd : rm
	/// - current : hash of directory from where to delete
	/// - targets : array of hashes of files/directories to delete
	/// 
	/// Response: open with directory tree, on errors add error and errorData.
	/// </remarks>
	public class RmCommand : ICommand
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
					error = ResourceManager.GetString("Unable_To_Retrieve_Current_Directory_Error")
				};
			}

			bool canWrite;

			try
			{
				canWrite = fileSystem.Using(cwd, fs => fs.Current.CanWrite);
			}
			catch (InvalidOperationException)
			{
				return new ErrorCommandResponse
				{
					error = ResourceManager.GetString("Unable_To_Retrieve_Current_Directory_Error")
				};
			}

			if (!canWrite)
			{
				return new ErrorCommandResponse
				{
					error = ResourceManager.GetString("Delete_Permission_Denied_For_Current_Directory_Error")
				};
			}

			var errors = RemoveFiles(commandContext);

			try
			{
				return fileSystem.Using(cwd, fs => GetResponse(commandContext, fs, errors));
			}
			catch (InvalidOperationException)
			{
				return new ErrorCommandResponse
				{
					error = ResourceManager.GetString("Unable_To_Retrieve_Current_Directory_Error")
				};
			}
		}

		private static CommandResponse GetResponse(ICommandContext commandContext, IFileSystemContext fileSystemContext, List<Tuple<string, string>> errors)
		{
			var response = new OpenCommand().GetResponse(commandContext, fileSystemContext, true);

			if (errors.Any())
			{
				var errorMessages = errors.Select(e => "[{0}: {1}]".FormatWith(e.Item1, e.Item2));

				response.error = ResourceManager.GetString("Uploading_Files_Error").FormatWith(string.Join(",", errorMessages));
			}

			return response;
		}

		private static List<Tuple<string, string>> RemoveFiles(ICommandContext commandContext)
		{
			var errors = new List<Tuple<string, string>>();

			var targetHashes = (commandContext.Parameters["targets[]"] ?? string.Empty).Split(',');

			if (!targetHashes.Any())
			{
				return errors;
			}

			var portal = commandContext.CreatePortalContext();
			var website = portal.Website.ToEntityReference();
			var security = commandContext.CreateSecurityProvider();
			var dataServiceProvider = commandContext.CreateDependencyProvider().GetDependency<ICmsDataServiceProvider>();

			foreach (var targetHash in targetHashes)
			{
				var serviceContext = commandContext.CreateServiceContext();

				Entity target;

				if (!TryGetTargetEntity(serviceContext, targetHash, website, out target))
				{
					errors.Add(new Tuple<string, string>(targetHash, ResourceManager.GetString("Unable_To_Retrieve_Target_Entity_For_Given_Hash_Error")));

					continue;
				}

				try
				{
					OrganizationServiceContextInfo serviceContextInfo;
					EntitySetInfo entitySetInfo;

					if (dataServiceProvider != null
					    && OrganizationServiceContextInfo.TryGet(serviceContext.GetType(), out serviceContextInfo)
					    && serviceContextInfo.EntitySetsByEntityLogicalName.TryGetValue(target.LogicalName, out entitySetInfo))
					{
						dataServiceProvider.DeleteEntity(serviceContext, entitySetInfo.Property.Name, target.Id);
					}
					else
					{
						if (!security.TryAssert(serviceContext, target, CrmEntityRight.Change))
						{
							errors.Add(new Tuple<string, string>(GetDisplayName(target), ResourceManager.GetString("Delete_Permission_Denied_For_Target_Entity_Error")));

							continue;
						}

						CrmEntityInactiveInfo inactiveInfo;

						if (CrmEntityInactiveInfo.TryGetInfo(target.LogicalName, out inactiveInfo))
						{
							serviceContext.SetState(inactiveInfo.InactiveState, inactiveInfo.InactiveStatus, target);
						}
						else
						{
							serviceContext.DeleteObject(target);
							serviceContext.SaveChanges();
						}
					}
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("{0} {1}", ResourceManager.GetString("Deleting_File_Exception"), e.ToString()));
					errors.Add(new Tuple<string, string>(GetDisplayName(target), e.Message));
				}
			}

			return errors;
		}

		private static string GetDisplayName(Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.Attributes.Contains("adx_title"))
			{
				return entity.GetAttributeValue<string>("adx_title") ?? entity.GetAttributeValue<string>("adx_name");
			}

			return entity.GetAttributeValue<string>("adx_name");
		}

		private static bool TryGetTargetEntity(OrganizationServiceContext serviceContext, string hash, EntityReference website, out Entity target)
		{
			target = null;

			DirectoryContentHash hashInfo;

			if (!DirectoryContentHash.TryParse(hash, out hashInfo))
			{
				return false;
			}

			Tuple<string, string> targetSchema;

			if (!RmTargetSchemaLookup.TryGetValue(hashInfo.LogicalName, out targetSchema))
			{
				return false;
			}

			target = serviceContext.CreateQuery(hashInfo.LogicalName)
				.FirstOrDefault(e => e.GetAttributeValue<Guid>(targetSchema.Item1) == hashInfo.Id
					&& e.GetAttributeValue<EntityReference>(targetSchema.Item2) == website);

			return target != null;
		}

		private static readonly IDictionary<string, Tuple<string, string>> RmTargetSchemaLookup = new Dictionary<string, Tuple<string, string>>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "adx_webfile", new Tuple<string, string>("adx_webfileid", "adx_websiteid") },
		};
	}
}
