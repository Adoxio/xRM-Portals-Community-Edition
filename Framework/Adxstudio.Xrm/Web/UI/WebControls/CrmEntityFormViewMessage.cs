/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Custom message used by CrmEntityFormView control.
	/// </summary>
	[Serializable]
	public class CrmEntityFormViewMessage : IStateManager
	{
		[NonSerialized]
		private readonly StateBag _stateBag;

		/// <summary>
		/// Class intialization
		/// </summary>
		public CrmEntityFormViewMessage()
		{
			_stateBag = new StateBag();
		}

		/// <summary>
		/// Type of message.
		/// </summary>
		public string MessageType
		{
			get { return ViewState["MessageType"] as string; }
			set { ViewState["MessageType"] = value; }
		}

		/// <summary>
		/// A Format String of the message.
		/// </summary>
		/// <example>
		/// {0} is a required field.
		/// </example>
		public string FormatString
		{
			get { return ViewState["FormatString"] as string; }
			set { ViewState["FormatString"] = value; }
		}

		public bool IsTrackingViewState { get; private set; }

		/// <summary>
		/// State Information
		/// </summary>
		public StateBag ViewState
		{
			get { return _stateBag; }
		}

		public void LoadViewState(object savedState)
		{
			if (savedState == null) return;

			var objArray = (object[])savedState;

			if (objArray[0] != null)
			{
				((IStateManager)ViewState).LoadViewState(objArray[0]);
			}
		}

		public object SaveViewState()
		{
			var obj = ((IStateManager)ViewState).SaveViewState();

			return obj != null ? new[] { obj } : null;
		}

		public void TrackViewState()
		{
			IsTrackingViewState = true;

			((IStateManager)ViewState).TrackViewState();
		}
	}
}
