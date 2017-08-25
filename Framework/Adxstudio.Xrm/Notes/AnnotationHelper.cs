/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Text;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Notes
{
	/// <summary>
	/// Helper methods for working with annotations
	/// </summary>
	public static class AnnotationHelper
	{
		/// <summary>
		/// Prefix for denoting a note is to be published to the portal.
		/// </summary>
		public const string WebAnnotationPrefix = "*WEB*";
		/// <summary>
		/// Prefix for denoting a note is private.
		/// </summary>
		public const string PrivateAnnotationPrefix = "*PRIVATE*";
		/// <summary>
		/// Prefix for denoting a note is public.
		/// </summary>
		public const string PublicAnnotationPrefix = "*PUBLIC*";

		/// <summary>
		/// Forces a valid file name
		/// </summary>
		public static string EnsureValidFileName(string fileName)
		{
			return fileName.IndexOf("\\", StringComparison.Ordinal) >= 0 ? fileName.Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1) : fileName;
		}

		/// <summary>
		/// Produces the subject text with id of the contact record creating the note and privacy tag.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="user">Entity Reference to the current portal user's contact</param>
		/// <param name="isPrivate">Boolean value indicating whether the note is private or not.</param>
		public static string BuildNoteSubject(OrganizationServiceContext serviceContext, EntityReference user, bool isPrivate = false)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");

			var now = DateTime.UtcNow;

			if (user == null || user.LogicalName != "contact")
			{
				return string.Format("Note created on {0}{1}", now, isPrivate ? PrivateAnnotationPrefix : string.Empty);
			}

			var contact = serviceContext.RetrieveSingle(
				user.LogicalName, 
				FetchAttribute.None,
				new[] {
					new Condition("statecode", ConditionOperator.Equal, 0),
					new Condition("contactid", ConditionOperator.Equal, user.Id)
				});

			if (contact == null)
			{
				return string.Format("Note created on {0}{1}", now, isPrivate ? PrivateAnnotationPrefix : string.Empty);
			}

			// Tack the contact entity reference onto the end of the note subject, so that if we really wanted to, we
			// could parse this subject and find the portal user that submitted the note.
			return string.Format("Note created on {0} by {1} [{2}:{3}]{4}", now, contact.GetAttributeValue<string>("fullname"), contact.LogicalName, contact.Id, isPrivate ? PrivateAnnotationPrefix : string.Empty);
		}

		public static string BuildNoteSubject(IDataAdapterDependencies dependencies)
		{
			return BuildNoteSubject(dependencies.GetServiceContext(), dependencies.GetPortalUser());
		}

		/// <summary>
		/// Try to get the id of the contact that created the note to retrieve the entity reference for the contact.
		/// </summary>
		/// <param name="subject">The subject of the annotation</param>
		/// <returns>Return null if no contact is specified, otherwise it returns an entity reference to the contact that created the note.</returns>
		public static EntityReference GetNoteContact(string subject)
		{
			if (string.IsNullOrWhiteSpace(subject))
			{
				return null;
			}

			// String format is "Note created on {0} by {1} [{2}:{3}]"

			var startIndex = subject.IndexOf("by ", StringComparison.InvariantCulture);

			if (startIndex == -1)
			{
				return null;
			}

			try
			{
				var contactString = subject.Substring(startIndex);

				var name = contactString.Substring(3, contactString.IndexOf("[", StringComparison.InvariantCulture) - 4);

				var pos1 = contactString.IndexOf(":", StringComparison.InvariantCulture) + 1;
				var pos2 = contactString.IndexOf("]", StringComparison.InvariantCulture);

				var contactid = contactString.Substring(pos1, pos2 - pos1);

				Guid contactGuid;

				if (Guid.TryParse(contactid, out contactGuid))
				{
					return new EntityReference("contact", contactGuid) { Name = name };
				}
			}
			catch (Exception)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to extract contact EntityReference from note subject. {0}", subject));
            }

			return null;
		}

		/// <summary>
		/// Gets the name of the createdby systemuser.
		/// </summary>
		/// <param name="annotation">Note record</param>
		public static string GetNoteCreatedByName(Entity annotation)
		{
			if (annotation == null)
			{
				return null;
			}

			var createdby = annotation.GetAttributeValue<EntityReference>("createdby");

			return createdby == null ? null : createdby.Name;
		}

		public static string GetNoteCreatedByName(IAnnotation annotation)
		{
			return GetNoteCreatedByName(annotation.Entity);
		}

		/// <summary>
		/// Gets whether or not the note is private i.e. whether 
		/// </summary>
		/// <param name="annotation"></param>
		/// <returns></returns>
		public static bool GetNotePrivacy(Entity annotation)
		{
			if (annotation == null)
			{
				return false;
			}

			var subject = annotation.GetAttributeValue<string>("subject");

			return !string.IsNullOrWhiteSpace(subject) && subject.Contains(PrivateAnnotationPrefix);
		}

		public static bool GetNotePrivacy(IAnnotation annotation)
		{
			if (annotation == null)
			{
				return false;
			}

			return !string.IsNullOrWhiteSpace(annotation.Subject) && annotation.Subject.Contains(PrivateAnnotationPrefix);
		}

		/// <summary>
		/// Simple formatter that takes plain text input and does a simple transformation to HTML. The input text
		/// is HTML-encoded, blank lines (double linebreaks) are wrapped in paragraphs, and single linebreaks are
		/// replaced with HTML breaks. Optionally, any URLs in the text can be converted to HTML links.
		/// </summary>
		/// <param name="text">note text</param>
		/// <returns>HTML string</returns>
		public static IHtmlString FormatNoteText(object text)
		{
			return text == null ? null : new SimpleHtmlFormatter().Format(text.ToString().Replace(WebAnnotationPrefix, string.Empty, StringComparison.InvariantCultureIgnoreCase).Replace(PublicAnnotationPrefix, string.Empty, StringComparison.InvariantCultureIgnoreCase));
		}

		public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison)
		{
			StringBuilder sb = new StringBuilder();

			int previousIndex = 0;
			int index = str.IndexOf(oldValue, comparison);
			while (index != -1)
			{
				sb.Append(str.Substring(previousIndex, index - previousIndex));
				sb.Append(newValue);
				index += oldValue.Length;

				previousIndex = index;
				index = str.IndexOf(oldValue, index, comparison);
			}
			sb.Append(str.Substring(previousIndex));

			return sb.ToString();
		}
	}
}
