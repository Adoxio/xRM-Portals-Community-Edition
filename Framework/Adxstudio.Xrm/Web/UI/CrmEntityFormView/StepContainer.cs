/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Store a dynamically added container for a step.
	/// </summary>
	public class StepContainer : PlaceHolder
	{
		/// <summary>
		/// StepContainer class initialization.
		/// </summary>
		/// <param name="stepIndex"></param>
		public StepContainer(int stepIndex)
		{
			StepIndex = stepIndex;
		}

		/// <summary>
		/// Index of the step.
		/// </summary>
		public int StepIndex { get; private set; }
	}
}
