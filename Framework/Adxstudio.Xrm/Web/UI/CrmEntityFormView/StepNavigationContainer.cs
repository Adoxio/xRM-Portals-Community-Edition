/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Enumeration of the types of navigation between steps.
	/// </summary>
	public enum StepNavigationType
	{
		/// <summary>
		/// Cancel step.
		/// </summary>
		Cancel,
		/// <summary>
		/// Move to next step.
		/// </summary>
		Next,
		/// <summary>
		/// Move to previous step.
		/// </summary>
		Previous,
		/// <summary>
		/// Submit step.
		/// </summary>
		Submit
	}

	/// <summary>
	/// Store a dynamically added step container for a given type.
	/// </summary>
	public class StepNavigationContainer : PlaceHolder
	{
		/// <summary>
		/// StepNavigationContainer class initialization.
		/// </summary>
		/// <param name="navigationType"></param>
		public StepNavigationContainer(StepNavigationType navigationType)
		{
			NavigationType = navigationType;
		}

		/// <summary>
		/// The type of step navigation.
		/// </summary>
		public StepNavigationType NavigationType { get; private set; }
	}
}
