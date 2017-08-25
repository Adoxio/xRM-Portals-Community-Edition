/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	/// <summary>
	/// Handler for elFinder "open" command. Needed to workaround Safari upload issue.
	/// </summary>
	/// <remarks>
	/// http://elrte.org/redmine/projects/elfinder/wiki/Client-Server_Protocol_EN#ping
	/// http://www.webmasterworld.com/macintosh_webmaster/3300569.htm
	/// 
	/// Arguments:
	/// 
	/// - cmd : ping
	/// 
	/// Response: send empty page with headers Connection: close.
	/// </remarks>
	public class PingCommand : ICommand
	{
		public CommandResponse GetResponse(ICommandContext commandContext)
		{
			return new PingCommandResponse();
		}
	}
}
