/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


namespace Adxstudio.Xrm.Cases
{
	/// <summary>
	/// Represents security permission information for a single case for a given user.
	/// </summary>
	public interface ICaseAccess
	{
		bool Delete { get; }

		bool Public { get; }

		bool Read { get; }

		bool Write { get; }
	}
}
