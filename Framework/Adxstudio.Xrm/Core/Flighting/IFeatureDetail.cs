/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adxstudio.Xrm.Core.Flighting
{
	/// <summary>
	/// Granularity at which we can control the feature being enabled using the FCB
	/// </summary>
	public enum FeatureLocation
	{
		Organization,
		Portal,
		Global
	}

	/// <summary>
	/// Every FCB needs to define value for these basic details
	/// </summary>
	public interface IFeatureDetail
	{
		/// <summary>
		/// Name of the Feature
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Indicate whether the feature is enabled.
		/// </summary>
		bool IsEnabled { get; set; }

		/// <summary>
		/// Location where the FCB is going to affect
		/// </summary>
		FeatureLocation FeatureLocation { get; set; }

	}
}
