/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.Web.UI;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Provide declarations of form fields with custom ASP.NET Validator controls and addition field properties to influence the field's control rendering.
	/// </summary>
	/// <remarks>
	///  Usage:
    /// <![CDATA[
	///  <adx:CrmEntityFormView runat="server" ID="FormView" EntityName="lead" FormName="Web Form" DataSourceID="FormViewDataSource">
	///   <Fields>
	///    <adx:CrmEntityFormViewField runat="server" AttributeName="parentcustomerid" Type="Dropdown"></adx:CrmEntityFormViewField>
	///    <adx:CrmEntityFormViewField runat="server" AttributeName="lastname">
	///     <CustomValidatorsTemplate>
	///      <asp:CustomValidator ID="lastname_customvalidator" runat="server" ErrorMessage="Custom Validator Error" ValidationGroup="Contact" OnServerValidate="OnServerValidateLastName">
	///      </asp:CustomValidator>
	///     </CustomValidatorsTemplate>
	///    </adx:CrmEntityFormViewField>
	///   </Fields>
	///  </adx:CrmEntityFormView>
    /// ]]>
	/// </remarks>
	[Serializable]
	public class CrmEntityFormViewField : IStateManager
	{
		[NonSerialized]
		private readonly StateBag _stateBag;

		/// <summary>
		/// Constructor
		/// </summary>
		public CrmEntityFormViewField()
		{
			_stateBag = new StateBag();
		}

		/// <summary>
		/// The type of control to render.
		/// </summary>
		public FieldType Type
		{
			get
			{
				var type = (ViewState["Type"]);
				if (type == null)
				{
					return FieldType.Default;
				}
				return (FieldType)type;
			}
			set { ViewState["Type"] = value; }
		}

		/// <summary>
		/// Logical name of the attribute
		/// </summary>
		public string AttributeName
		{
			get { return ViewState["AttributeName"] as string; }
			set { ViewState["AttributeName"] = value; }
		}

		/// <summary>
		/// Custom validators for this field in addition to the existing validators.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), DefaultValue((string)null)]
		public ITemplate CustomValidatorsTemplate { get; set; }

		public bool IsTrackingViewState { get; private set; }

		/// <summary>
		/// Instance of StateBag class for maintaining ViewState.
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

	/// <summary>
	/// Type enumeration
	/// </summary>
	public enum FieldType
	{
		/// <summary>
		/// Default control type
		/// </summary>
		Default,
		/// <summary>
		/// Render a lookup as a dropdown control
		/// </summary>
		Dropdown
	}
}
