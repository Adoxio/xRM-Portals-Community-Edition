/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services
{
	/// <summary>
	/// Provides event handling behaviour for <see cref="EventCachedOrganizationService"/> events.
	/// </summary>
	public interface IOrganizationServiceEventProvider
	{
		void Created(object sender, OrganizationServiceCreatedEventArgs args);
		void Deleted(object sender, OrganizationServiceDeletedEventArgs args);
		void Executed(object sender, OrganizationServiceExecutedEventArgs args);
		void Updated(object sender, OrganizationServiceUpdatedEventArgs args);
	}
}
