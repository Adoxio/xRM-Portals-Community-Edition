/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	[ToolboxData("<{0}:ImageCaptcha Runat=\"server\" Height=\"100px\" Width=\"300px\" />")]
	[Designer("Adxstudio.Cms.Web.UI.Design.ControlDesigner")]
	public class ImageCaptcha : Captcha
	{
		/// <summary>
		/// The generated image.
		/// </summary>
		private Image _image;

		/// <summary>
		/// Gets or sets the URL used to render the image to the client.
		/// </summary>
		[Category("Behavior")]
		[Description("The URL path used to render the image to the client.")]
		[DefaultValue((string)null)]
		public string ImageUrl { get; set; }

		/// <summary>
		/// Creates the DynamicImage and HiddenField controls.
		/// </summary>
		protected override sealed void CreateChildControls()
		{
			// We're based on BaseValidator/Label, so make sure to render child controls,
			// though most likely no additional controls will be created.
			base.CreateChildControls();

			if (string.IsNullOrEmpty(ImageUrl))
			{
				throw new InvalidOperationException("ImageUrl cannot be null or empty.");
			}

			// Make sure that the size of this control has been properly defined.
			// We need the size in pixels in order to properly generate an image.
			if (Width.IsEmpty || Width.Type != UnitType.Pixel || Height.IsEmpty || Height.Type != UnitType.Pixel)
			{
				throw new InvalidOperationException("Missing dimensions of ImageCaptcha control (Width, Height) in pixels.");
			}

			// Create and configure the dynamic image.  We won't setup the actual
			// Bitmap for it until later.
			_image = new Image
			{
				BorderColor = BorderColor,
				BorderStyle = BorderStyle,
				BorderWidth = BorderWidth,
				ToolTip = ToolTip,
				EnableViewState = false
			};

			Controls.Add(_image);
		}

		/// <summary>Render the challenge.</summary>
		/// <param name="challengeId">The ID of the challenge.</param>
		/// <param name="content">The content to render.</param>
		protected override sealed void RenderChallenge(Guid challengeId, string content)
		{
			// Generate the link to the image generation handler.
			_image.Width = Width;
			_image.Height = Height;
			_image.ImageUrl = "{0}?width={1}&height={2}&id={3}".FormatWith(VirtualPathUtility.ToAbsolute(ImageUrl), (int)Width.Value, (int)Height.Value, challengeId);
		}
	}
}
