/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cases
{
	using System.Collections.Generic;
	using Adxstudio.Xrm.Notes;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Provides data operations for a single case.
	/// </summary>
	public interface ICaseDataAdapter
	{
		/// <summary>
		/// Append a new note to a case.
		/// </summary>
		/// <param name="text">The text of the note.</param>
		/// <param name="filename">The filename of the optional note attachment.</param>
		/// <param name="contentType">The content (mime) type of the optional note attachment.</param>
		/// <param name="fileContent">The byte contents of the optional note attachment.</param>
		/// <param name="ownerId">The owner of the optional note attachment.</param>
		void AddNote(string text, string filename = null, string contentType = null, byte[] fileContent = null, EntityReference ownerId = null);

		/// <summary>
		/// Cancels a case.
		/// </summary>
		void Cancel();

		/// <summary>
		/// Reopens (i.e., re-activates) a case.
		/// </summary>
		void Reopen();

		/// <summary>
		/// Resolves a case, with a description of the resolution.
		/// </summary>
		/// <param name="customerSatisfactionCode"></param>
		/// <param name="resolutionSubject"></param>
		/// <param name="resolutionDescription"></param>
		void Resolve(int? customerSatisfactionCode, string resolutionSubject, string resolutionDescription);

		/// <summary>
		/// Resolves a case, with a description of the resolution.
		/// </summary>
		/// <param name="customerSatisfactionCode"></param>
		/// <param name="resolutionSubject"></param>
		/// <param name="resolutionDescription"></param>
		/// <param name="statuscode"></param>
		void Resolve(int? customerSatisfactionCode, string resolutionSubject, string resolutionDescription, int? statuscode);

		/// <summary>
		/// Selects the attributes of the case.
		/// </summary>
		ICase Select();

		/// <summary>
		/// Selects access permissions for the case, for the current portal user.
		/// </summary>
		/// <returns></returns>
		ICaseAccess SelectAccess();

		/// <summary>
		/// Selects the <see cref="ICaseNote">notes</see> associated with the case.
		/// </summary>
		IEnumerable<IAnnotation> SelectNotes();

		/// <summary>
		/// Selects the <see cref="ICaseResolution">resolutions</see> associated with the case.
		/// </summary>
		IEnumerable<ICaseResolution> SelectResolutions();
	}
}
