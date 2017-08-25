/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	/// <summary>
	/// Interface for implementation of individual elFinder service commands.
	/// </summary>
	/// <remarks>
	/// General parameters often used when sending request to connector:
	/// 
	/// - current : (String) hash from the path of a current working directory
	/// - target : (String) hash from the path to a file that take action
	/// - targets : (Array) array of hashes of files/directories which take action
	/// 
	/// For any command request connector can send next parameters:
	/// 
	/// - error : (String) string containing error message
	/// - errorData : (Object) object with non-fatal errors, keys - filenames, values - error message
	/// - select : (Array) array of hashes of files/directories which will be selected after action by client (upload, mkdir, mkfile)
	/// - debug : (Object) object contains debug information
	/// 
	/// Commands of client not always match connector commands:
	///
	/// - reload, back client commands use connector command open
	/// - list, icons, quicklook, copy, cut client commands work without request to connector
	/// 
	/// Any connector command which get invalid argument from client stops it execution and returns error message, except open command which returns error message and contents ot root directory.
	/// </remarks>
	public interface ICommand
	{
		CommandResponse GetResponse(ICommandContext commandContext);
	}
}
