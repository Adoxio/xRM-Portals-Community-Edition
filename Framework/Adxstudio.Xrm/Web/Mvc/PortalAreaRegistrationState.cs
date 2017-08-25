/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Profile;

namespace Adxstudio.Xrm.Web.Mvc
{
	/// <summary>
	/// Default implementation of <see cref="IPortalAreaRegistrationState"/>.
	/// </summary>
	public class PortalAreaRegistrationState : IPortalAreaRegistrationState
	{
		public event ProfileMigrateEventHandler Profile_MigrateAnonymous;

		public void OnProfile_MigrateAnonymous(object sender, ProfileMigrateEventArgs args)
		{
			var handler = Profile_MigrateAnonymous;

			if (handler != null)
			{
				handler(sender, args);
			}
		}
	}
}
