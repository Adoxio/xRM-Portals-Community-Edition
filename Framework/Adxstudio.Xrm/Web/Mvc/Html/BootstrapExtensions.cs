/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for rendering a bootstrap modal within Adxstudio Portals applications.
	/// </summary>
	public static class BootstrapExtensions
	{
		/// <summary>
		/// Size options for a bootstrap modal
		/// </summary>
		public enum BootstrapModalSize
		{
			/// <summary>
			/// Default
			/// </summary>
			Default,
			/// <summary>
			/// Large. CSS class 'modal-lg' is added to the modal-dialog.
			/// </summary>
			Large,
			/// <summary>
			/// Small. CSS class 'modal-sm' is added to the modal-dialog.
			/// </summary>
			Small
		}

		/// <summary>
		/// Renders a bootstrap modal
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="size">Size of the modal to create.</param>
		/// <param name="title">Title assigned to the modal's header.</param>
		/// <param name="body">Content body assigned to the modal.</param>
		/// <param name="cssClass">CSS class to be assigned to the modal.</param>
		/// <param name="id">ID to be assigned to the modal.</param>
		/// <param name="omitDismissButton">Indicates whether the dismiss button should be rendered in the header.</param>
		/// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
		/// <param name="omitFooter">Indicates whether the footer should be rendered.</param>
		/// <param name="omitPrimaryButton">Indicates whether the primary button should be rendered.</param>
		/// <param name="omitCloseButton">Indicates whether the close button should be rendered.</param>
		/// <param name="primaryButtonText">Text assigned to the primary button.</param>
		/// <param name="closeButtonText">Text assigned to the close button.</param>
		/// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
		/// <param name="primaryButtonCssClass">CSS class assigned to the primary button.</param>
		/// <param name="closeButtonCssClass">CSS class assigned to the close button.</param>
		/// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
		/// <param name="footerButtonCollection">HTML that will be prepended to the modal's footer.</param>
		/// <param name="isForm">Is Form.</param>
		/// <param name="isLookup">Is Lookup.</param>
		/// <returns>Html string containing the markup for a boostrap modal.</returns>
		public static IHtmlString BoostrapModal(this HtmlHelper html, BootstrapModalSize size,
			string title, string body = null, string cssClass = null, string id = null, bool omitDismissButton = false,
			string dismissButtonSrText = "Close", bool omitFooter = false, bool omitPrimaryButton = false, bool omitCloseButton = false,
			string primaryButtonText = "Ok", string closeButtonText = "Cancel", string titleCssClass = null, string primaryButtonCssClass = null,
			string closeButtonCssClass = null, IDictionary<string, string> htmlAttributes = null, Dictionary<string, string> footerButtonCollection = null,
			bool isForm = false, bool isLookup = false)
		{
			var modal = new TagBuilder("section");
			if (!string.IsNullOrWhiteSpace(cssClass))
			{
				modal.AddCssClass(cssClass);
			}
			modal.AddCssClass("fade");
			modal.AddCssClass("modal");
			modal.MergeAttribute("tabindex", "-1");
			modal.MergeAttribute("role", "dialog");
			modal.MergeAttribute("aria-label", title);
			modal.MergeAttribute("aria-hidden", "true");
			modal.MergeAttribute("data-backdrop", "static");
			modal.MergeAttributes(htmlAttributes);
			if (!string.IsNullOrWhiteSpace(id))
			{
				modal.MergeAttribute("id", id);
			}

			var modalDialog = new TagBuilder("div");
			modalDialog.AddCssClass("modal-dialog");
			switch (size)
			{
				case BootstrapModalSize.Large:
					modalDialog.AddCssClass("modal-lg");
					break;
				case BootstrapModalSize.Small:
					modalDialog.AddCssClass("modal-sm");
					break;
			}

			var modalContent = new TagBuilder("div");
			modalContent.AddCssClass("modal-content");

			var modalHeader = new TagBuilder("div");
			modalHeader.AddCssClass("modal-header");
			if (!omitDismissButton)
			{
				var close = new TagBuilder("button");
				close.AddCssClass("close");
				close.MergeAttribute("tabindex", "0");
				close.MergeAttribute("type", "button");
				close.MergeAttribute("title", dismissButtonSrText ?? ResourceManager.GetString("Close_DefaultText"));
				close.MergeAttribute("data-dismiss", "modal");
				close.MergeAttribute("aria-label", Resources.ResourceManager.GetString("Close_DefaultText"));
				var span = new TagBuilder("span");
				span.MergeAttribute("aria-hidden", "true");
				span.InnerHtml = "&times;";
				var srspan = new TagBuilder("span");
				srspan.AddCssClass("sr-only");
				srspan.SetInnerText(dismissButtonSrText ?? "Close");
				close.InnerHtml = span.ToString();
				close.InnerHtml += srspan.ToString();
				modalHeader.InnerHtml += close.ToString();
			}
			var h1 = new TagBuilder("h1");
			h1.AddCssClass("modal-title h4");
			h1.InnerHtml = title ?? string.Empty;
			h1.MergeAttribute("title", h1.InnerHtml);
			modalHeader.InnerHtml += h1.ToString();

			modalContent.InnerHtml += modalHeader.ToString();

			var modalBody = new TagBuilder("div");

			modalBody.AddCssClass("modal-body");
			if (isForm) modalBody.AddCssClass("form-horizontal");
			modalBody.InnerHtml = body ?? string.Empty;

			modalContent.InnerHtml += modalBody.ToString();

			if (!omitFooter)
			{
				var modalFooter = new TagBuilder("div");
				modalFooter.AddCssClass("modal-footer");

				if (footerButtonCollection != null)
				{
					modalFooter.InnerHtml += footerButtonCollection["New"];
				}

				var button = new TagBuilder("button");
				if (!omitPrimaryButton)
				{
					if (!string.IsNullOrWhiteSpace(primaryButtonCssClass))
					{
						button.AddCssClass(primaryButtonCssClass);
					}
					button.AddCssClass("btn btn-primary");
					button.AddCssClass("primary");
					button.MergeAttribute("type", "button");
					button.MergeAttribute("tabindex", "0");
					button.MergeAttribute("aria-label", primaryButtonText ?? Resources.ResourceManager.GetString("Ok_DefaultText"));
                    button.MergeAttribute("title", primaryButtonText ?? "Ok");
                    button.InnerHtml = primaryButtonText ?? "Ok";
					if (!isLookup)
						modalFooter.InnerHtml += button.ToString();
				}

				var close = new TagBuilder("button");
				if (!omitCloseButton)
				{
					if (!string.IsNullOrWhiteSpace(closeButtonCssClass))
					{
						close.AddCssClass(closeButtonCssClass);
					}
					close.AddCssClass("btn btn-default");
					close.AddCssClass("cancel");
					close.MergeAttribute("type", "button");
					close.MergeAttribute("tabindex", "0");
					close.MergeAttribute("aria-label", closeButtonText ?? Resources.ResourceManager.GetString("Cancel_DefaultText"));
					close.MergeAttribute("data-dismiss", "modal");
					close.MergeAttribute("title", closeButtonText ?? "Cancel");
					close.InnerHtml = closeButtonText ?? "Cancel";
					if (!isLookup)
						modalFooter.InnerHtml += close.ToString();
				}

				if (isLookup && !omitPrimaryButton && !omitCloseButton)
				{
					modalFooter.InnerHtml += button.ToString() + close.ToString();
				}

				if (footerButtonCollection != null)
				{
					modalFooter.InnerHtml += footerButtonCollection["RemoveButton"];
				}

				modalContent.InnerHtml += modalFooter.ToString();
			}

			modalDialog.InnerHtml = modalContent.ToString();

			modal.InnerHtml = modalDialog.ToString();

			return new HtmlString(modal.ToString());
		}
	}
}
