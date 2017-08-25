/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Profile;

namespace Adxstudio.Xrm.Web.Mvc
{
	/// <summary>
	/// Represents a state object that can be passed to <see cref="System.Web.Mvc.AreaRegistration.RegisterAllAreas(object)"/>, and
	/// which provides extensions useful to Area modules in Adxstudio Portals applications.
	/// </summary>
	public interface IPortalAreaRegistrationState
	{
		event ProfileMigrateEventHandler Profile_MigrateAnonymous;

		void OnProfile_MigrateAnonymous(object sender, ProfileMigrateEventArgs args);
	}
}
