/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	public class ThumbnailCommand : ICommand
	{
		public CommandResponse GetResponse(ICommandContext commandContext)
		{
			return new ThumbnailResponse
			{
				current = commandContext.Parameters["current"],
				tmb = false,
				images = new object()
			};
		}
	}
}
