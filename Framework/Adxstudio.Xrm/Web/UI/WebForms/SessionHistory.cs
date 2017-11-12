/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Web.UI.WebForms
{
	/// <summary>
	/// Web Form Session History.
	/// </summary>
	[DataContract]
	[Serializable]
	public class SessionHistory
	{
		/// <summary>
		/// ID of the Session.
		/// </summary>
		[DataMember]
		public Guid Id { get; set; }

		/// <summary>
		/// ID of the Web Form.
		/// </summary>
		[DataMember]
		public Guid WebFormId { get; set; }

		/// <summary>
		/// Index of the current step.
		/// </summary>
		[DataMember]
		public int CurrentStepIndex { get; set; }

		/// <summary>
		/// ID of the current step.
		/// </summary>
		[DataMember]
		public Guid CurrentStepId { get; set; }

		/// <summary>
		/// The definition of the primary record associated with the history.
		/// </summary>
		[DataMember]
		public ReferenceEntity PrimaryRecord { get; set; }

		/// <summary>
		/// ID of the authenticated user's contact record.
		/// </summary>
		[DataMember]
		public Guid ContactId { get; set; }

		/// <summary>
		/// ID of the current session shopping cart.
		/// </summary>
		[DataMember]
		public Guid QuoteId { get; set; }

		/// <summary>
		/// ID of the authenticated user's system user record.
		/// </summary>
		[DataMember]
		public Guid SystemUserId { get; set; }

		/// <summary>
		/// Identification of the anonymous user.
		/// </summary>
		[DataMember]
		public string AnonymousIdentification { get; set; }

		/// <summary>
		/// User's IP Address.
		/// </summary>
		[DataMember]
		public string UserHostAddress { get; set; }

		/// <summary>
		/// User's Identity Name.
		/// </summary>
		[DataMember]
		public string UserIdentityName { get; set; }

		/// <summary>
		/// Steps visisted by the user.
		/// </summary>
		[DataMember]
		public List<Step> StepHistory { get; set; }

		/// <summary>
		/// Step
		/// </summary>
		[DataContract]
		[Serializable]
		public class Step
		{
			/// <summary>
			/// Index of the current step.
			/// </summary>
			[DataMember]
			public int Index { get; set; }

			/// <summary>
			/// Unique identifier of the Web Form Step.
			/// </summary>
			[DataMember]
			public Guid ID { get; set; }

			/// <summary>
			/// A boolean value indicating whether the step is active or just a history record.
			/// </summary>
			[DataMember(IsRequired = false)]
			public bool? IsActive { get; set; }

			/// <summary>
			/// The Unique identifier of the previous Web Form Step.
			/// </summary>
			[DataMember]
			public Guid PreviousStepID { get; set; }

			/// <summary>
			/// Details of the target entity saved.
			/// </summary>
			[DataMember]
			public ReferenceEntity ReferenceEntity { get; set; }
		}

		/// <summary>
		/// Class contains details of the target entity saved during the web form step.
		/// </summary>
		[DataContract]
		[Serializable]
		public class ReferenceEntity
		{
			/// <summary>
			/// Unique identifier of the entity saved.
			/// </summary>
			[DataMember]
			public Guid ID { get; set; }

			/// <summary>
			/// Logical name of the entity saved.
			/// </summary>
			[DataMember]
			public string LogicalName { get; set; }

			/// <summary>
			/// Logical name of the entity's primary key.
			/// </summary>
			[DataMember]
			public string PrimaryKeyLogicalName { get; set; }
		}
	}
}
