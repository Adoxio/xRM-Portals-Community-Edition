/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	/// <summary>
	/// Handler for elFinder "open" command.
	/// </summary>
	/// <remarks>
	/// Implementation targets elFinder 1.2 (http://elrte.org/redmine/projects/elfinder/wiki/Client-Server_Protocol_EN#open).
	/// 
	/// Open directory or send file to browser. Also handles client initialization, in which case init=true parameter will be sent.
	/// 
	/// Arguments:
	/// 
	/// - cmd : open
	/// - target : hash of directory to open
	/// - tree : Optional argument, if set additionally include file tree in response
	/// 
	/// Response must contain (Object) cwd and (Array) cdc, additionally can contain (Object) tree and/or (Boolean) tmb, example:
	/// 
	/// {
	///     "cwd" : { // (Object) Current Working Directory - information about current directory
	///         "name"  : "Home",                             // (String)  directory name
	///         "hash"  : "b4473c8c08d1d499ecd7112f3398f125", // (String)  hash from absolute path to current dir
	///         "mime"  : "directory",                        // (String)  always set to "directory" 
	///         "rel"   : "Home",                             // (String)  relative path to current directory
	///         "size"  : 0,                                  // (Number)  directory size in bytes
	///         "date"  : "30 Jan 2010 14:25",                // (String)  modification time (mtime)
	///         "read"  : true,                               // (Boolean) read access
	///         "write" : true,                               // (Boolean) write access
	///         "rm"    : false                               // (Boolean) delete access
	///     },
	///     "cdc" : [ // (Array) (of Objects) Current Directory Content - info about current dir content
	///     {
	///         "name"  : "link to README",                   // (String)  file/dir name
	///         "hash"  : "4fc059e61577f0267fbd6c1c5bafb1b4", // (String)  hash
	///         "url"   : "http://localhost:8001/~troex/git/elfinder/files/wiki/README.txt", // (String) URL
	///         "date"  : "Today 16:50",                      // (String)  modification time (mtime)
	///         "mime"  : "text/plain",                       // (String)  MIME-type of file or "directory" 
	///         "size"  : 10,                                 // (Number)  file/dir size in bytes
	///         "read"  : true,                               // (Boolean) read access
	///         "write" : true,                               // (Boolean) write access
	///         "rm"    : true,                               // (Boolean) delete access
	///         "link"  : "8d331825ebfbe1ddae14d314bf81a712", // (String)  only for links, hash of file to
	///                                                       //           which point link
	///         "linkTo": "Home/README.txt",                  // (String)  only for links, relative path to
	///                                                       //           file on which points links
	///         "parent": "b4473c8c08d1d499ecd7112f3398f125", // (String)  only for links, hash of directory
	///                                                       //           in which is linked file is located
	///         "resize": true,                               // (Boolean) only for images, if true will
	///                                                       //           enable Resize contextual menu
	///         "dim"   : "600x400",                          // (String)  only for images, dimension of image
	///                                                       //           must be set if resize is set to true.
	///         "tmb"   : "http://localhost:8001/~troex/git/elfinder/files/.tmb/4fc059e61577f0267fbd6c1c5bafb1b4.png" 
	///                                                       // (String) only for images
	///                                                       // URL to thumbnail
	///     },
	///     {
	///         // ...
	///     }
	///     ],
	///     "tree" : { // (Object) directory tree. Optional parameter, only if "tree" was requested
	///     "name"  : "Home",                             // (String)  dir name
	///         "hash"  : "b4473c8c08d1d499ecd7112f3398f125", // (String)  hash
	///         "read"  : true,                               // (Boolean) read access
	///         "write" : true,                               // (Boolean) write access
	///         "dirs"  : [                                   // (Array) (of Objects) array of child directories
	///         {
	///             "name"  : "test",
	///             "hash"  : "ac4b61565950a73395c871f9c3fc7362",
	///             "read"  : true,
	///             "write" : true,
	///             "dirs"  : [
	///             {
	///                 // ...
	///             },
	///             {
	///                 // ...
	///             }
	///             ]
	///         }
	///     },
	///     "tmb" : true // (Boolean) Optional parameter, if true, will cause client to initiate _tmb_ commands.
	/// }
	/// 
	/// Important!* if invalid target argument was requested, connector will root directory content (cdc, cdw) and error message (error).
	/// </remarks>
	public class OpenCommand : ICommand
	{
		public CommandResponse GetResponse(ICommandContext commandContext)
		{
			var fileSystem = commandContext.CreateFileSystem();

			// If no target directory is specified (as on first init), use a working directory, if
			// specified.
			var hash = string.IsNullOrEmpty(commandContext.Parameters["target"])
				? commandContext.Parameters["working"]
				: commandContext.Parameters["target"];

			var response = new OpenCommandResponse
			{
				tmb = false,
				disabled = commandContext.DisabledCommands.ToArray(),
				parameters = new InitializationParameters
				{
					dotFiles = false,
					archives = new string[] { },
					extract = new string[] { },
				}
			};

			DirectoryContentHash cwd;

			if (DirectoryContentHash.TryParse(hash, out cwd))
			{
				try
				{
					return fileSystem.Using(cwd, fs => GetResponse(commandContext, fs, response));
				}
				catch (InvalidOperationException e)
				{
					response.error = e.Message;
				}
			}

			try
			{
				return fileSystem.Using(fs => GetResponse(commandContext, fs, response));
			}
			catch (InvalidOperationException e)
			{
				response.error = e.Message;

				return response;
			}
		}

		internal CommandResponse GetResponse(ICommandContext commandContext, IFileSystemContext fileSystemContext, bool forceTree = false)
		{
			return GetResponse(commandContext, fileSystemContext, new OpenCommandResponse
			{
				tmb = false,
				disabled = commandContext.DisabledCommands.ToArray(),
				parameters = new InitializationParameters
				{
					dotFiles = false,
					archives = new string[] { },
					extract = new string[] { },
				}
			}, forceTree);
		}

		private static CommandResponse GetResponse(ICommandContext commandContext, IFileSystemContext fileSystemContext, OpenCommandResponse response, bool forceTree = false)
		{
			response.cwd = fileSystemContext.Current.Info;
			response.cdc = fileSystemContext.Current.Children;

			bool treeRequested;

			if (forceTree || (bool.TryParse(commandContext.Parameters["tree"] ?? string.Empty, out treeRequested) && treeRequested))
			{
				response.tree = fileSystemContext.Tree;
			}

			return response;
		}
	}
}
