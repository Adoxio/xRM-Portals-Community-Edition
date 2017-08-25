/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services
{
	using System;

	/// <summary>
	/// Represents debugging caller details.
	/// </summary>
	[Serializable]
	internal struct Caller
	{
		/// <summary>
		/// The member name.
		/// </summary>
		public string MemberName { get; set; }

		/// <summary>
		/// The source file path.
		/// </summary>
		public string SourceFilePath { get; set; }

		/// <summary>
		/// The source line number.
		/// </summary>
		public int SourceLineNumber { get; set; }
	}
}
