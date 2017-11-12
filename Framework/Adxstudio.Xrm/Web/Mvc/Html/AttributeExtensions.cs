/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using DotLiquid;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for rendering portal view entity attributes in Adxstudio Portals applications.
	/// </summary>
	public static class AttributeExtensions
	{
		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
        /// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose attribute will be rendered.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <param name="editType">
		/// The type of CMS editing interface to use for this attribute. The values "text" and "html" are supported by default.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="logicalName"/> is not a valid attribute, returns an empty string.
		/// </returns>
		public static IHtmlString Attribute(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string logicalName, string editType = "html", string editTitle = null, bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return Attribute(html, portalViewContext.GetEntity(serviceContext, entity), logicalName, editType, editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
        /// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose attribute will be rendered.</param>
		/// <param name="attributeExpression">
        /// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <param name="editType">
		/// The type of CMS editing interface to use for this attribute. The values "text" and "html" are supported by default.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission.
		/// </returns>
		public static IHtmlString Attribute<TEntity>(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, Expression<Func<TEntity, object>> attributeExpression, string editType = "html", string editTitle = null, bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var logicalName = PortalExtensions.GetAttributeLogicalNameFromExpression(attributeExpression);

			return Attribute(html, serviceContext, entity, logicalName, editType, editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. Implicitly uses the current portal context entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <param name="editType">
		/// The type of CMS editing interface to use for this attribute. The values "text" and "html" are supported by default.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission. If <paramref name="logicalName"/> is not a valid attribute, returns an empty string.
		/// </returns>
		public static IHtmlString Attribute(this HtmlHelper html, string logicalName, string editType = "html", string editTitle = null, bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return Attribute(html, portalViewContext.Entity, logicalName, editType, editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. Implicitly uses the current portal context entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attributeExpression">
        /// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <param name="editType">
		/// The type of CMS editing interface to use for this attribute. The values "text" and "html" are supported by default.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission.
		/// </returns>
		public static IHtmlString Attribute<TEntity>(this HtmlHelper html, Expression<Func<TEntity, object>> attributeExpression, string editType = "html", string editTitle = null, bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var logicalName = PortalExtensions.GetAttributeLogicalNameFromExpression(attributeExpression);

			return Attribute(html, logicalName, editType, editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose attribute will be rendered.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <param name="editType">
		/// The type of CMS editing interface to use for this attribute. The values "text" and "html" are supported by default.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="entity"/> is null, or <paramref name="logicalName"/> is not a valid attribute, returns an empty
		/// string.
		/// </returns>
		public static IHtmlString Attribute(this HtmlHelper html, IPortalViewEntity entity, string logicalName, string editType = "html", string editTitle = null, bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			if (entity == null)
			{
				return new HtmlString(string.Empty);
			}

			var attribute = entity.GetAttribute(logicalName);

			return attribute == null
				? new HtmlString(string.Empty)
				: Attribute(html, attribute, editType, editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose attribute will be rendered.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <param name="editType">
		/// The type of CMS editing interface to use for this attribute. The values "text" and "html" are supported by default.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="entity"/> is null, returns an empty string.
		/// </returns>
		public static IHtmlString Attribute<TEntity>(this HtmlHelper html, IPortalViewEntity entity, Expression<Func<TEntity, object>> attributeExpression, string editType = "html", string editTitle = null, bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var logicalName = PortalExtensions.GetAttributeLogicalNameFromExpression(attributeExpression);

			return Attribute(html, entity, logicalName, editType, editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attribute">The portal entity attribute to be rendered.</param>
		/// <param name="editType">
		/// The type of CMS editing interface to use for this attribute. The values "text" and "html" are supported by default.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="attribute"/> is null, returns an empty string.
		/// </returns>
		public static IHtmlString Attribute(this HtmlHelper html, IPortalViewAttribute attribute, string editType = "html", string editTitle = null, bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return AttributeInternal(html, attribute, editType, editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}
		
		internal static IHtmlString AttributeInternal(this HtmlHelper html, IPortalViewAttribute attribute, string editType = "html", string editTitle = null, bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault, Context liquidContext = null, string defaultValue = null, string languageName = null)
		{
			if (attribute == null)
			{
				return new HtmlString(string.Empty);
			}

			var tag = new TagBuilder(tagName ?? "div");

			if (!string.IsNullOrEmpty(cssClass))
			{
				tag.AddCssClass(cssClass);
			}

			if (attribute.Value == null && defaultValue == null)
			{
				tag.AddCssClass("no-value");
			}

			tag.AddCssClass("xrm-attribute");
			tag.AddCssClass("xrm-editable-{0}".FormatWith(editType));

			var valueContainer = new TagBuilder(tagName ?? "div");

			valueContainer.AddCssClass("xrm-attribute-value");

			var stringValue = attribute.Value != null ? attribute.Value.ToString() : (defaultValue ?? string.Empty);

			stringValue = liquidEnabled
				? liquidContext == null
					? html.Liquid(stringValue)
					: html.Liquid(stringValue, liquidContext)
				: stringValue;

			if (htmlEncode)
			{
				valueContainer.AddCssClass("xrm-attribute-value-encoded");
				valueContainer.SetInnerText(stringValue);
			}
			else
			{
				valueContainer.InnerHtml = stringValue;
			}

			tag.InnerHtml += valueContainer.ToString();

			if (languageName != null)
			{
				tag.MergeAttribute("data-languageContext", languageName);
			}

			if (attribute.Editable)
			{
				tag.MergeAttribute("data-encoded", htmlEncode ? "true" : "false", true);
				tag.MergeAttribute("data-liquid", liquidEnabled ? "true" : "false", true);

				var portalViewContext = PortalExtensions.GetPortalViewContext(html);

				portalViewContext.RenderEditingMetadata(attribute, tag, editTitle);
			}

			return new HtmlString(tag.ToString());
		}

		/// <summary>
		/// Renders a CRM entity attribute value, with no encoding or other modification. Implicitly uses the current portal context
		/// entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="logicalName">The logical name of the attribute value to be returned.</param>
		/// <returns>A CRM entity attribute value.</returns>
		public static object AttributeLiteral(this HtmlHelper html, string logicalName, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return AttributeLiteral(html, portalViewContext.Entity, logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders a CRM entity attribute value, with no encoding or other modification. Implicitly uses the current portal context
		/// entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <returns>A CRM entity attribute value.</returns>
		public static object AttributeLiteral<TEntity>(this HtmlHelper html, Expression<Func<TEntity, object>> attributeExpression, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var logicalName = PortalExtensions.GetAttributeLogicalNameFromExpression(attributeExpression);

			return AttributeLiteral(html, logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders a CRM entity attribute value, with no encoding or other modification. Implicitly uses the current portal context
		/// entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The entity whose attribute is to be returned.</param>
		/// <param name="logicalName">The logical name of the attribute value to be returned.</param>
		/// <returns>A CRM entity attribute value.</returns>
		public static object AttributeLiteral(this HtmlHelper html, IPortalViewEntity entity, string logicalName, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			if (entity == null)
			{
				return new HtmlString(string.Empty);
			}

			var attribute = entity.GetAttribute(logicalName);

			return attribute == null
				? string.Empty
				: AttributeLiteral(html, attribute, liquidEnabled);
		}

		/// <summary>
		/// Renders a CRM entity attribute value, with no encoding or other modification. Implicitly uses the current portal context
		/// entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The entity whose attribute is to be returned.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <returns>A CRM entity attribute value.</returns>
		public static object AttributeLiteral<TEntity>(this HtmlHelper html, IPortalViewEntity entity, Expression<Func<TEntity, object>> attributeExpression, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var logicalName = PortalExtensions.GetAttributeLogicalNameFromExpression(attributeExpression);

			return AttributeLiteral(html, entity, logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders a CRM entity attribute value, with no encoding or other modification. Implicitly uses the current portal context
		/// entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attribute">The attribute whose value is to be returned.</param>
		/// <returns>A CRM entity attribute value.</returns>
		public static object AttributeLiteral(this HtmlHelper html, IPortalViewAttribute attribute, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return liquidEnabled && attribute.Value is string
				? html.Liquid(attribute.Value as string)
				: attribute.Value;
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute value is intended to support HTML content, and will be editable with the "html" editor
		/// type.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
        /// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose attribute will be rendered.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="logicalName"/> is not a valid attribute, returns an empty string.
		/// </returns>
		public static IHtmlString HtmlAttribute(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string logicalName, string editTitle = null, string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, serviceContext, entity, logicalName, "html", editTitle, false, "div", cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute value is intended to support HTML content, and will be editable with the "html" editor
		/// type.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
        /// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose attribute will be rendered.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission.
		/// </returns>
		public static IHtmlString HtmlAttribute<TEntity>(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, Expression<Func<TEntity, object>> attributeExpression, string editTitle = null, string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, serviceContext, entity, attributeExpression, "html", editTitle, false, "div", cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute value is intended to support HTML content, and will be editable with the "html" editor
		/// type. Implicitly uses the current portal context entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="logicalName"/> is not a valid attribute, returns an empty string.
		/// </returns>
		public static IHtmlString HtmlAttribute(this HtmlHelper html, string logicalName, string editTitle = null, string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, logicalName, "html", editTitle, false, "div", cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute value is intended to support HTML content, and will be editable with the "html" editor
		/// type. Implicitly uses the current portal context entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission.
		/// </returns>
		public static IHtmlString HtmlAttribute<TEntity>(this HtmlHelper html, Expression<Func<TEntity, object>> attributeExpression, string editTitle = null, string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, attributeExpression, "html", editTitle, false, "div", cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute value is intended to support HTML content, and will be editable with the "html" editor
		/// type.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose attribute will be rendered.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="entity"/> is null, or <paramref name="logicalName"/> is not a valid attribute, returns an empty
		/// string.
		/// </returns>
		public static IHtmlString HtmlAttribute(this HtmlHelper html, IPortalViewEntity entity, string logicalName, string editTitle = null, string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, entity, logicalName, "html", editTitle, false, "div", cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute value is intended to support HTML content, and will be editable with the "html" editor
		/// type.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose attribute will be rendered.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission. If <paramref name="entity"/> is null, returns an empty
		/// string.
		/// </returns>
		public static IHtmlString HtmlAttribute<TEntity>(this HtmlHelper html, IPortalViewEntity entity, Expression<Func<TEntity, object>> attributeExpression, string editTitle = null, string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, entity, attributeExpression, "html", editTitle, false, "div", cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute value is intended to support HTML content, and will be editable with the "html" editor
		/// type.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attribute">The portal entity attribute to be rendered.</param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="attribute"/> is null, returns an empty string.
		/// </returns>
		public static IHtmlString HtmlAttribute(this HtmlHelper html, IPortalViewAttribute attribute, string editTitle = null, string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, attribute, "html", editTitle, false, "div", cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute will be editable with the "text" editor type.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
        /// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose attribute will be rendered.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="logicalName"/> is not a valid attribute, returns an empty string.
		/// </returns>
		public static IHtmlString TextAttribute(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string logicalName, string editTitle = null, bool htmlEncode = true, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, serviceContext, entity, logicalName, "text", editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute will be editable with the "text" editor type.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose attribute will be rendered.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission.
		/// </returns>
		public static IHtmlString TextAttribute<TEntity>(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, Expression<Func<TEntity, object>> attributeExpression, string editTitle = null, bool htmlEncode = true, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, serviceContext, entity, attributeExpression, "text", editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute will be editable with the "text" editor type. Implicitly uses the current portal context
		/// entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="logicalName"/> is not a valid attribute, returns an empty string.
		/// </returns>
		public static IHtmlString TextAttribute(this HtmlHelper html, string logicalName, string editTitle = null, bool htmlEncode = true, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, logicalName, "text", editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute will be editable with the "text" editor type. Implicitly uses the current portal context
		/// entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission.
		/// </returns>
		public static IHtmlString TextAttribute<TEntity>(this HtmlHelper html, Expression<Func<TEntity, object>> attributeExpression, string editTitle = null, bool htmlEncode = true, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, attributeExpression, "text", editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute will be editable with the "text" editor type.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose attribute will be rendered.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="entity"/> is null, or <paramref name="logicalName"/> is not a valid attribute, returns an empty
		/// string.
		/// </returns>
		public static IHtmlString TextAttribute(this HtmlHelper html, IPortalViewEntity entity, string logicalName, string editTitle = null, bool htmlEncode = true, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, entity, logicalName, "text", editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute will be editable with the "text" editor type.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose attribute will be rendered.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission. If <paramref name="entity"/> is null, returns an empty string.
		/// </returns>
		public static IHtmlString TextAttribute<TEntity>(this HtmlHelper html, IPortalViewEntity entity, Expression<Func<TEntity, object>> attributeExpression, string editTitle = null, bool htmlEncode = true, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, entity, attributeExpression, "text", editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users
		/// with permission. The attribute will be editable with the "text" editor type.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attribute">The portal entity attribute to be rendered.</param>
		/// <param name="editTitle">
		/// The title/description of the attribute shown to the user by the CMS editing interface.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If <paramref name="attribute"/> is null, returns an empty string.
		/// </returns>
		public static IHtmlString TextAttribute(this HtmlHelper html, IPortalViewAttribute attribute, string editTitle = null, bool htmlEncode = true, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			return Attribute(html, attribute, "text", editTitle, htmlEncode, tagName, cssClass, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML style element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
        /// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose attribute will be rendered.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <returns>
		/// An HTML style element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString StyleAttribute(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string logicalName, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return StyleAttribute(html, portalViewContext.GetEntity(serviceContext, entity), logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML style element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
        /// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose attribute will be rendered.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <returns>
		/// An HTML style element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString StyleAttribute<TEntity>(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, Expression<Func<TEntity, object>> attributeExpression, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var logicalName = PortalExtensions.GetAttributeLogicalNameFromExpression(attributeExpression);

			return StyleAttribute(html, serviceContext, entity, logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML style element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <returns>
		/// An HTML style element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString StyleAttribute(this HtmlHelper html, string logicalName, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return StyleAttribute(html, portalViewContext.Entity, logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML style element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <returns>
		/// An HTML style element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString StyleAttribute<TEntity>(this HtmlHelper html, Expression<Func<TEntity, object>> attributeExpression, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var logicalName = PortalExtensions.GetAttributeLogicalNameFromExpression(attributeExpression);

			return StyleAttribute(html, logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML style element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose attribute will be rendered.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <returns>
		/// An HTML style element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString StyleAttribute(this HtmlHelper html, IPortalViewEntity entity, string logicalName, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			if (entity == null)
			{
				return new HtmlString(string.Empty);
			}

			var attribute = entity.GetAttribute(logicalName);

			return attribute == null
				? new HtmlString(string.Empty)
				: StyleAttribute(html, attribute, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML style element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose attribute will be rendered.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <returns>
		/// An HTML style element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString StyleAttribute<TEntity>(this HtmlHelper html, IPortalViewEntity entity, Expression<Func<TEntity, object>> attributeExpression, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var logicalName = PortalExtensions.GetAttributeLogicalNameFromExpression(attributeExpression);

			return StyleAttribute(html, entity, logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML style element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attribute">The portal entity attribute to be rendered.</param>
		/// <returns>
		/// An HTML style element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString StyleAttribute(this HtmlHelper html, IPortalViewAttribute attribute, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			if (attribute == null)
			{
				return new HtmlString(string.Empty);
			}

			var attributeLiteral = html.AttributeLiteral(attribute, liquidEnabled);

			if (attributeLiteral == null)
			{
				return new HtmlString(string.Empty);
			}

			var attributeString = attributeLiteral.ToString();

			if (string.IsNullOrEmpty(attributeString))
			{
				return new HtmlString(string.Empty);
			}

			var tag = new TagBuilder("style");

			tag.Attributes.Add("type", "text/css");

			tag.InnerHtml = attributeString;

			return new HtmlString(tag.ToString());
		}

		/// <summary>
		/// Renders an HTML script element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
        /// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose attribute will be rendered.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <returns>
		/// An HTML script element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString ScriptAttribute(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string logicalName, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return ScriptAttribute(html, portalViewContext.GetEntity(serviceContext, entity), logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML script element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose attribute will be rendered.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <returns>
		/// An HTML script element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString ScriptAttribute<TEntity>(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, Expression<Func<TEntity, object>> attributeExpression, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var logicalName = PortalExtensions.GetAttributeLogicalNameFromExpression(attributeExpression);

			return ScriptAttribute(html, serviceContext, entity, logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML script element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <returns>
		/// An HTML script element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString ScriptAttribute(this HtmlHelper html, string logicalName, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return ScriptAttribute(html, portalViewContext.Entity, logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML script element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <returns>
		/// An HTML script element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString ScriptAttribute<TEntity>(this HtmlHelper html, Expression<Func<TEntity, object>> attributeExpression, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var logicalName = PortalExtensions.GetAttributeLogicalNameFromExpression(attributeExpression);

			return ScriptAttribute(html, logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML script element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose attribute will be rendered.</param>
		/// <param name="logicalName">The logical name of the attribute to be rendered.</param>
		/// <returns>
		/// An HTML script element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString ScriptAttribute(this HtmlHelper html, IPortalViewEntity entity, string logicalName, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			if (entity == null)
			{
				return new HtmlString(string.Empty);
			}

			var attribute = entity.GetAttribute(logicalName);

			return attribute == null
				? new HtmlString(string.Empty)
				: ScriptAttribute(html, attribute, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML script element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose attribute will be rendered.</param>
		/// <param name="attributeExpression">
		/// Expression selecting an accessor property of generic type "TEntity", corresponding to an attribute accessor on an
		/// early-bound entity class.
		/// </param>
		/// <returns>
		/// An HTML script element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString ScriptAttribute<TEntity>(this HtmlHelper html, IPortalViewEntity entity, Expression<Func<TEntity, object>> attributeExpression, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var logicalName = PortalExtensions.GetAttributeLogicalNameFromExpression(attributeExpression);

			return ScriptAttribute(html, entity, logicalName, liquidEnabled);
		}

		/// <summary>
		/// Renders an HTML script element containing the contents of a given CRM entity attribute.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="attribute">The portal entity attribute to be rendered.</param>
		/// <returns>
		/// An HTML script element. If the attribute does not exist or has no value, returns an empty string.
		/// </returns>
		public static IHtmlString ScriptAttribute(this HtmlHelper html, IPortalViewAttribute attribute, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			if (attribute == null)
			{
				return new HtmlString(string.Empty);
			}

			var attributeLiteral = html.AttributeLiteral(attribute, liquidEnabled);

			if (attributeLiteral == null)
			{
				return new HtmlString(string.Empty);
			}

			var attributeString = attributeLiteral.ToString();

			if (string.IsNullOrEmpty(attributeString))
			{
				return new HtmlString(string.Empty);
			}

			var tag = new TagBuilder("script");

			tag.Attributes.Add("type", "text/javascript");

			tag.InnerHtml = attributeString;

			return new HtmlString(tag.ToString());
		}
	}
}
