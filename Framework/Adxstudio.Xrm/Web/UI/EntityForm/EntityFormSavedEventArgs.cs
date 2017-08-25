/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Web.UI.EntityForm
{
	/// <summary>
	/// Event arguments passed to the saved event.
	/// </summary>
	public class EntityFormSavedEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments passed to the saved event
		/// </summary>
		/// <param name="exceptionHandled">Indicates if the exception was handled</param>
		/// <param name="entityId">The ID of the target entity updated or inserted</param>
		/// <param name="entityLogicalName">Logical Name of the target entity</param>
		/// <param name="exception">Errors occuring during update</param>
		public EntityFormSavedEventArgs(Guid? entityId, string entityLogicalName, Exception exception, bool exceptionHandled, string entityDisplayName = null)
		{
			EntityId = entityId;
			EntityLogicalName = entityLogicalName;
			Exception = exception;
			ExceptionHandled = exceptionHandled;
			EntityDisplayName = entityDisplayName;
		}
		/// <summary>
		/// The ID of the target entity updated or inserted
		/// </summary>
		public Guid? EntityId { get; private set; }

		/// <summary>
		/// Logical Name of the target entity.
		/// </summary>
		public string EntityLogicalName { get; private set; }

		/// <summary>
		/// Errors occuring during update.
		/// </summary>
		public Exception Exception { get; private set; }

		/// <summary>
		/// Indicates if the exception was handled.
		/// </summary>
		public bool ExceptionHandled { get; set; }

		/// <summary>
		/// Entity name of the target entity.
		/// </summary>
		public string EntityDisplayName { get; private set; }
	}
}
