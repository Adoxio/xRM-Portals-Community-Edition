/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.Runtime.Caching;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Caching;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	public enum TextCase { Mixed, Upper, Lower }

	/// <summary>
	/// The base class for all Reverse Turing Test challenges.
	/// </summary>
	public abstract class Captcha : System.Web.UI.WebControls.WebControl, INamingContainer
	{
		/// <summary>
		/// Stores the challenge ID associated for future requests.
		/// </summary>
		private HtmlInputHidden _hiddenData;

		/// <summary>
		/// Whether Authenticate has previously succeeded in this HttpRequest.
		/// </summary>
		private bool _authenticated;

		/// <summary>
		/// Backing store for Expiration.
		/// </summary>
		private int _expiration = 120;

		[Category("Data")]
		[Description("The name of the ObjectCacheElement used for configuring the caching services.")]
		[DefaultValue(null)]
		public string ObjectCacheName { get; set; }

		/// <summary>
		/// Gets or sets the duration of time (seconds) a user has before the challenge expires.
		/// </summary>
		/// <value>
		/// The duration of time (seconds) a user has before the challenge expires.
		/// </value>
		[Category("Behavior")]
		[Description("The duration of time (seconds) a user has before the challenge expires.")]
		[DefaultValue(120)]
		public int SecondsBeforeExpiration
		{
			get { return _expiration; }
			set { _expiration = value; }
		}

		private TextCase _challengeTextCase = TextCase.Lower;

		/// <summary>
		/// Gets or sets the <see cref="TextCase"/> of the generated challenge.
		/// </summary>
		[Category("Behavior")]
		[Description("The text case of the challenge (e.g., upper-case).")]
		[DefaultValue(TextCase.Lower)]
		public TextCase ChallengeTextCase
		{
			get { return _challengeTextCase; }
			set { _challengeTextCase = value; }
		}

		private int _challengeTextLength = 5;

		/// <summary>
		/// Gets or sets the text length of the challenge.
		/// </summary>
		/// <value>
		/// The text length of the challenge.
		/// </value>
		[Category("Behavior")]
		[Description("The text length of the challenge.")]
		[DefaultValue(5)]
		public int ChallengeTextLength
		{
			get { return _challengeTextLength; }
			set { _challengeTextLength = value; }
		}

		/// <summary>
		/// Selects a word to use in an image.
		/// </summary>
		/// <returns>
		/// The word to use in the challenge.
		/// </returns>
		protected virtual string ChooseWord()
		{
			var word = new RandomStringGenerator(ChallengeTextCase);

			return word.Build(ChallengeTextLength);
		}
		
		/// <summary>
		/// Creates the hidden field control that stores the challenge ID.
		/// </summary>
		protected override void CreateChildControls()
		{
			_hiddenData = new HtmlInputHidden { EnableViewState = false };

			Controls.Add(_hiddenData);

			base.CreateChildControls();
		}

		/// <summary>
		/// Generates a new image and fills in the dynamic image and hidden field appropriately.
		/// </summary>
		/// <param name="e">Ignored.</param>
		protected sealed override void OnPreRender(EventArgs e)
		{
			// Gets a word for the challenge, associates it with a new ID, and stores it for the client.
			var content = ChooseWord();

			var id = Guid.NewGuid();

			SetChallengeText(id, content, DateTime.Now.AddSeconds(_expiration), ObjectCacheName);

			_hiddenData.Value = id.ToString();

			// Generates a challenge based on the selected word/phrase.
			RenderChallenge(id, content);
			
			base.OnPreRender(e);
		}

		/// <summary>
		/// Gets the challenge text for a particular ID.
		/// </summary>
		/// <param name="challengeId">The ID of the challenge text to retrieve.</param>
		/// <returns>
		/// The text associated with the specified ID; null if no text exists.
		/// </returns>
		public static string GetChallengeText(Guid challengeId, string objectCacheName)
		{
			return ObjectCacheManager.GetInstance(objectCacheName).Get(GetChallengeCacheKey(challengeId)) as string;
		}

		internal static string GetChallengeCacheKey(Guid challengeId)
		{
			return "captcha:{0}".FormatWith(challengeId);
		}

		/// <summary>
		/// Sets the challenge text for a particular ID.
		/// </summary>
		/// <param name="challengeId">The ID of the challenge with which this text should be associated.</param>
		/// <param name="text">The text to store along with the challenge ID.</param>
		/// <param name="expiration">The expiration date fo the challenge.</param>
		internal static void SetChallengeText(Guid challengeId, string text, DateTime expiration, string objectCacheName)
		{
			var key = GetChallengeCacheKey(challengeId);

			if (text == null)
			{
				ObjectCacheManager.GetInstance(objectCacheName).Remove(key);
			}
			else
			{
				var policy = new CacheItemPolicy { Priority = CacheItemPriority.NotRemovable };
				ObjectCacheManager.GetInstance(objectCacheName).Insert(key, text, policy);
			}
		}

		/// <summary>
		/// Authenticates user-supplied data against that retrieved using the challenge ID.
		/// </summary>
		/// <param name="userData">The user-supplied data.</param>
		/// <returns>
		/// Whether the user-supplied data matches that retrieved using the challenge ID.
		/// </returns>
		internal bool Authenticate(string userData)
		{
			// We want to allow multiple authentication requests within the same HTTP request,
			// so we can the result as a member variable of the class (non-static).
			if (_authenticated)
			{
				return true;
			}

			// If no authentication has happened previously, and if the user has supplied text,
			// and if the ID is stored correctly in the page, and if the user text matches the challenge text,
			// then set the challenge text, note that we've authenticated, and return true.  Otherwise, failed authentication.
			if (!(string.IsNullOrEmpty(userData) || string.IsNullOrEmpty(_hiddenData.Value)))
			{
				try
				{
					var id = new Guid(_hiddenData.Value);

					var text = GetChallengeText(id, ObjectCacheName);

					if (text != null && string.Compare(userData, text) == 0)
					{
						_authenticated = true;

						SetChallengeText(id, null, DateTime.MinValue, ObjectCacheName);

						return true;
					}
				}
				catch (FormatException)
				{
					// Swallow the exception.
				}
			}

			return false;
		}

		/// <summary>
		/// Generates the challenge and presents it to the user.
		/// </summary>
		/// <param name="id">The ID of the challenge.</param>
		/// <param name="content">The content to render.</param>
		protected abstract void RenderChallenge(Guid id, string content);

		internal class RandomStringGenerator
		{
			private readonly string _characterSet;
			private readonly Random _random = new Random();

			private const string UpperCaseCharacterSet = "ABCDEFGHJKMNPQRSTUVWXYZ";
			private const string LowerCaseCharacterSet = "abcdefghjkmnpqrstuvwxyz";
			private const string NumericCharacterSet   = "23456789";

			public RandomStringGenerator(TextCase textCase) : this(BuildCharacterSet(textCase)) { }

			public RandomStringGenerator(string characterSet)
			{
				_characterSet = characterSet;
			}

			public string CharacterSet
			{
				get { return _characterSet; }
			}

			public string Build(int length)
			{
				var randomString = new StringBuilder(length);

				for (var i = 0; i < length; i++)
				{
					randomString.Append(GetRandomCharacter());
				}

				return randomString.ToString();
			}

			protected char GetRandomCharacter()
			{
				return CharacterSet[_random.Next(CharacterSet.Length)];
			}

			private static string BuildCharacterSet(TextCase textCase)
			{
				var characterSet = new StringBuilder(NumericCharacterSet);

				if (textCase == TextCase.Upper || textCase == TextCase.Mixed)
				{
					characterSet.Append(UpperCaseCharacterSet);
				}

				if (textCase == TextCase.Lower || textCase == TextCase.Mixed)
				{
					characterSet.Append(LowerCaseCharacterSet);
				}

				return characterSet.ToString();
			}
		}
	}
}
