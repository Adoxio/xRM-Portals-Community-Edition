/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	public class DirectoryContentHash
	{
		private static readonly Encoding _encoding = Encoding.UTF8;
		private readonly EntityReference _entityReference;

		public DirectoryContentHash(Entity entity, bool isDirectory = false) : this(entity.ToEntityReference(), isDirectory)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}
		}

		public DirectoryContentHash(EntityReference entityReference, bool isDirectory = false)
		{
			if (entityReference == null)
			{
				throw new ArgumentNullException("entityReference");
			}

			_entityReference = entityReference;
			IsDirectory = isDirectory;
		}

		public Guid Id
		{
			get { return _entityReference.Id; }
		}

		public bool IsDirectory { get; private set; }

		public string LogicalName
		{
			get { return _entityReference.LogicalName; }
		}

		public EntityReference ToEntityReference()
		{
			return new EntityReference(_entityReference.LogicalName, _entityReference.Id);
		}

		public override string ToString()
		{
			return Base64Encode(string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}", LogicalName, Id, IsDirectory));
		}

		public static bool TryParse(string value, out DirectoryContentHash hash)
		{
			hash = null;

			try
			{
				if (string.IsNullOrEmpty(value))
				{
					return false;
				}

				var decoded = Base64Decode(value);

				var match = Regex.Match(decoded, "^(?<LogicalName>[^:]+):(?<Id>.+):(?<IsDirectory>.*)$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

				if (!match.Success)
				{
					return false;
				}

				var logicalName = match.Groups["LogicalName"].Value;

				Guid id;

				if (!Guid.TryParse(match.Groups["Id"].Value, out id))
				{
					return false;
				}

				bool isDirectoryValue;

				var isDirectory = bool.TryParse(match.Groups["IsDirectory"].Value, out isDirectoryValue) && isDirectoryValue;

				hash = new DirectoryContentHash(new EntityReference(logicalName, id), isDirectory);

				return true;
			}
			catch
			{
				return false;
			}
		}

		private static string Base64Encode(string value)
		{
			var bytes = _encoding.GetBytes(value);

			return Convert.ToBase64String(bytes);
		}

		private static string Base64Decode(string value)
		{
			var bytes = Convert.FromBase64String(value);

			return _encoding.GetString(bytes);
		}
	}
}
