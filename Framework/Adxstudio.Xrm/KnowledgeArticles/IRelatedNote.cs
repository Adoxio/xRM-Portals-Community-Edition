/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.KnowledgeArticles
{
	/// <summary>
	///  IRelatedNote interface
	/// </summary>
	public interface IRelatedNote
	{
		/// <summary>
		/// Description of Note
		/// </summary>
		string Description { get; }

		/// <summary>
		/// File Name
		/// </summary>
		string FileName { get; }

		/// <summary>
		/// File Url
		/// </summary>
		string FileUrl { get; }
	}
}
