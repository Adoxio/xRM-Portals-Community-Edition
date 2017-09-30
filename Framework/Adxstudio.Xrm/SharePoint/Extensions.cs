/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Adxstudio.Xrm.Resources;
using Microsoft.SharePoint.Client;

namespace Adxstudio.SharePoint
{
	/// <summary>
	/// Helper methods on the <see cref="ClientContext"/> class.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Writes the provided file stream to the specified folder.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="file"></param>
		/// <param name="folder"></param>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public static string SaveFile(this ClientContext context, FileStream file, Folder folder, bool overwrite = false)
		{
			var filename = Path.GetFileName(file.Name);

			return SaveFile(context, file, folder, filename, overwrite);
		}

		/// <summary>
		/// Writes the provided stream to the specified folder using the provided filename.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="file"></param>
		/// <param name="folder"></param>
		/// <param name="filename"></param>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public static string SaveFile(this ClientContext context, Stream file, Folder folder, string filename, bool overwrite = false)
		{
			var url = folder.ServerRelativeUrl + "/" + filename;

			// SaveBinaryDirect does not work on Online since it creates its own WebRequest separate from the ClientContext
			// Microsoft.SharePoint.Client.File.SaveBinaryDirect(context, url, file, overwrite);

			if (context.ServerVersion.Major == 14) // SharePoint 2010
			{
				using (var ms = new MemoryStream())
				{
					file.CopyTo(ms);
					var content = ms.ToArray();

					var fci = new FileCreationInformation { Url = url, Overwrite = overwrite, Content = content };
					folder.Files.Add(fci);

					context.ExecuteQuery();
				}
			}
			else
			{
				using (file)
				{
					var fci = new FileCreationInformation { Url = url, Overwrite = overwrite, ContentStream = file };

					folder.Files.Add(fci);

					context.ExecuteQuery();
				}
			}

			return url;
		}

		/// <summary>
		/// Adds or retrieves an existing folder under the specified list.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="listUrl"></param>
		/// <param name="folderUrl"></param>
		/// <returns></returns>
		public static Folder AddOrGetExistingFolder(this ClientContext context, string listUrl, string folderUrl)
		{
			// Ensure safe folder URL - it cannot begin or end with dot, contain consecutive dots, or any of ~ " # % & * : < > ? \ { | }
			var spSafeFolderUrl = Regex.Replace(folderUrl, @"(\.{2,})|([\~\""\#\%\&\*\:\<\>\?\\\{\|\}])|(^\.)|(\.$)", string.Empty);

			var trimmedFolderUrl = spSafeFolderUrl.Trim('/');

			Folder folder;
			if (TryGetFolder(context, listUrl + "/" + trimmedFolderUrl, out folder))
			{
				return folder;
			}

			var list = context.GetListByUrl(listUrl);

			return context.CreateFolderPath(list.RootFolder, trimmedFolderUrl);
		}

		public static List GetListByUrl(this ClientContext context, string listUrl)
		{
			// try find the list with the relative URL
			var lists = context.LoadQuery(context.Web.Lists
				.Where(l => l.RootFolder.Name == listUrl)
				.Include(l => l.ContentTypes));

			context.ExecuteQuery();

			var list = lists.FirstOrDefault();

			if (list == null)
			{
				throw new NotSupportedException("No list could be found with the relative URL listUrl. Ensure that document management has been enabled for the entity.");
			}

			return list;
		}

		/// <summary>
		/// Try finding an existing folder with the given serverRelativeUrl.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="serverRelativeUrl"></param>
		/// <param name="folder"></param>
		/// <returns>folder will be null if nothing is found</returns>
		public static bool TryGetFolder(this ClientContext context, string serverRelativeUrl, out Folder folder)
		{
			folder = null;
			var web = context.Web;
			var existingFolder = web.GetFolderByServerRelativeUrl(serverRelativeUrl);
			
			context.Load(existingFolder);

			var exists = false;
			try
			{
				context.ExecuteQuery();
				exists = true;
			}
			catch { }

			if (exists)
			{
				folder = existingFolder;
			}
			
			return folder != null;
		}

		private static Folder CreateFolderPath(this ClientContext context, Folder parentFolder, string folderUrl)
		{
			var web = context.Web;
			var folderUrls = folderUrl.Split('/');
			var firstFolder = folderUrls[0];

			if (!parentFolder.IsPropertyAvailable("ServerRelativeUrl"))
			{
				context.Load(parentFolder, parent => parent.ServerRelativeUrl);
				context.ExecuteQuery();
			}

			Folder folder;
			if (!TryGetFolder(context, parentFolder.ServerRelativeUrl + "/" + firstFolder, out folder))
			{
				folder = parentFolder.Folders.Add(firstFolder);
				web.Context.Load(folder);
				web.Context.ExecuteQuery();
			}

			if (folderUrls.Length <= 1)
			{
				return folder;
			}
			
			var subFolderUrl = string.Join("/", folderUrls, 1, folderUrls.Length - 1);
			
			return context.CreateFolderPath(folder, subFolderUrl);
		}
	}
}
