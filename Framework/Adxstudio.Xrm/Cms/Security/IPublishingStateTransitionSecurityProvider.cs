/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cms.Security
{
	///<summary>
	///</summary>
	public interface IPublishingStateTransitionSecurityProvider
	{
		///<summary>
		/// Asserts current user has the right to perform the state transition from fromSate to toState.
		///</summary>
		///<param name="context"></param>
		///<param name="website"></param>
		///<param name="fromState"></param>
		///<param name="toState"></param>
		void Assert(OrganizationServiceContext context, Entity website, Entity fromState, Entity toState);

		///<summary>
		/// Asserts current user has the right to perform the state transition from fromSate to toState.
		///</summary>
		///<param name="context"></param>
		///<param name="website"></param>
		///<param name="fromStateId"></param>
		///<param name="toStateId"></param>
		void Assert(OrganizationServiceContext context, Entity website, Guid fromStateId, Guid toStateId);

		///<summary>
		/// Asserts current user has the right to perform the state transition from fromSate to toState.
		///</summary>
		///<param name="context"></param>
		///<param name="website"></param>
		///<param name="fromState"></param>
		///<param name="toState"></param>
		bool TryAssert(OrganizationServiceContext context, Entity website, Entity fromState, Entity toState);

		///<summary>
		/// Asserts current user has the right to perform the state transition from fromSate to toState.
		///</summary>
		///<param name="context"></param>
		///<param name="website"></param>
		///<param name="fromStateId"></param>
		///<param name="toStateId"></param>
		bool TryAssert(OrganizationServiceContext context, Entity website, Guid fromStateId, Guid toStateId);

	}
}
