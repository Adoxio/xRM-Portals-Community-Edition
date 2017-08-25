/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Mapping
{
	/// <summary>
	/// Map and distance calculation helper class
	/// </summary>
	public static class GeoHelpers
	{
		/// <summary>
		/// Radius of the earth in Kilometers.
		/// </summary>
		public static int EarthRadiusInKilometers
		{
			get { return 6371; }
		}

		/// <summary>
		/// Radius of the earth in miles.
		/// </summary>
		public static int EarthRadiusInMiles
		{
			get { return 3959; }
		}

		/// <summary>
		/// Distance unit of measure.
		/// </summary>
		public enum Units
		{
			/// <summary>
			/// Km
			/// </summary>
			Kilometers,
			/// <summary>
			/// mi
			/// </summary>
			Miles
		}

		/// <summary>
		/// Convert degrees to radians.
		/// </summary>
		/// <param name="degrees">Degrees</param>
		public static double DegreesToRadians(double degrees)
		{
			return Math.PI * degrees / 180.0;
		}

		/// <summary>
		/// Convert radians to degrees.
		/// </summary>
		/// <param name="radians">Radians</param>
		public static double RadiansToDegrees(double radians)
		{
			return radians * (180.0 / Math.PI);
		}
	}
}
