/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Identity
{
	using System;
	using System.Linq;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using System.Xml;
	using Microsoft.AspNet.Identity;
	using Microsoft.Owin.Security;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk;

	public class CrmClaimsIdentityFactory<TUser> : ClaimsIdentityFactory<TUser>
		where TUser : CrmUser
	{
		private static readonly string[] _externalLoginClaimsToExclude =
		{
			"http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
			System.Security.Claims.ClaimTypes.NameIdentifier,
			System.Security.Claims.ClaimTypes.Name,
			System.Security.Claims.ClaimTypes.Email,
		};

		public IAuthenticationManager AuthenticationManager { get; }
		public bool KeepExternalLoginClaims { get; set; }

		public CrmClaimsIdentityFactory(IAuthenticationManager authenticationManager)
		{
			if (authenticationManager == null) throw new ArgumentNullException("authenticationManager");

			this.AuthenticationManager = authenticationManager;
		}

		public override async Task<ClaimsIdentity> CreateAsync(UserManager<TUser, string> manager, TUser user, string authenticationType)
		{
			var identity = await base.CreateAsync(manager, user, authenticationType).WithCurrentCulture();

			var loginInfo = await this.AuthenticationManager.GetExternalLoginInfoAsync();
			var provider = loginInfo?.Login?.LoginProvider;

			if (!string.IsNullOrWhiteSpace(provider))
			{
				identity.AddClaim(this.ToClaim("http://schemas.adxstudio.com/xrm/2014/02/identity/claims/loginprovider", provider, null));
			}

			if (!string.IsNullOrWhiteSpace(user.Email))
			{
				identity.AddClaim(this.ToClaim(System.Security.Claims.ClaimTypes.Email, user.Email, null));
			}

			if (user.ContactId != null)
			{
				identity.AddClaim(this.ToClaim("http://schemas.adxstudio.com/xrm/2014/02/identity/claims/id", user.ContactId.Id.ToString(), null));
				identity.AddClaim(this.ToClaim("http://schemas.adxstudio.com/xrm/2014/02/identity/claims/logicalname", user.ContactId.LogicalName, null));
			}

			if (this.KeepExternalLoginClaims)
			{
				if (loginInfo != null && loginInfo.ExternalIdentity != null)
				{
					// exclude identifier types that may result in producing a false identity

					var claims = loginInfo.ExternalIdentity.Claims.Where(claim => !_externalLoginClaimsToExclude.Contains(claim.Type));
					identity.AddClaims(claims);
				}
			}

			return identity;
		}

		protected virtual Claim ToClaim(string type, string value, string valueType)
		{
			return new Claim(type, value, valueType, null);
		}

		protected virtual Claim ToClaim(string entityName, string attribute, object value, string name)
		{
			return this.ToClaim(this.ToClaimType(entityName, attribute), this.ToClaimValue(value, name), this.ToClaimValueType(value, name));
		}

		protected virtual string ToClaimType(string entityName, string attribute)
		{
			return "http://schemas.adxstudio.com/xrm/2014/02/claims/{0}/{1}".FormatWith(entityName, attribute);
		}

		protected virtual string ToClaimValue(object value, string name)
		{
			if (value is string) return XmlConvert.EncodeName(value as string);
			if (value is OptionSetValue) return XmlConvert.ToString((value as OptionSetValue).Value);
			if (value is EntityReference) return XmlConvert.ToString((value as EntityReference).Id);
			if (value is Money) return XmlConvert.ToString((value as Money).Value);
			if (value is BooleanManagedProperty) return XmlConvert.ToString((value as BooleanManagedProperty).Value);
			if (value is bool) return XmlConvert.ToString(((bool)value));
			if (value is char) return XmlConvert.ToString(((char)value));
			if (value is decimal) return XmlConvert.ToString(((decimal)value));
			if (value is sbyte) return XmlConvert.ToString(((sbyte)value));
			if (value is short) return XmlConvert.ToString(((short)value));
			if (value is int) return XmlConvert.ToString(((int)value));
			if (value is long) return XmlConvert.ToString(((long)value));
			if (value is byte) return XmlConvert.ToString(((byte)value));
			if (value is ushort) return XmlConvert.ToString(((ushort)value));
			if (value is uint) return XmlConvert.ToString(((uint)value));
			if (value is ulong) return XmlConvert.ToString(((ulong)value));
			if (value is float) return XmlConvert.ToString(((float)value));
			if (value is double) return XmlConvert.ToString(((double)value));
			if (value is TimeSpan) return XmlConvert.ToString(((TimeSpan)value));
			if (value is DateTime) return XmlConvert.ToString(((DateTime)value), XmlDateTimeSerializationMode.Utc);
			if (value is DateTimeOffset) return XmlConvert.ToString(((DateTimeOffset)value));
			if (value is Guid) return XmlConvert.ToString(((Guid)value));

			return XmlConvert.EncodeName(value.ToString());
		}

		protected virtual string ToClaimValueType(object value, string name)
		{
			if (value == null) return null;
			if (value is OptionSetValue) return ClaimValueTypes.Integer;
			if (value is EntityReference) return null;
			if (value is Money) return ExtendedClaimValueTypes.Decimal;
			if (value is BooleanManagedProperty) return ClaimValueTypes.Boolean;
			if (value is bool) return ClaimValueTypes.Boolean;
			if (value is char) return null;
			if (value is decimal) return ExtendedClaimValueTypes.Decimal;
			if (value is sbyte) return ExtendedClaimValueTypes.Byte;
			if (value is short) return ExtendedClaimValueTypes.Short;
			if (value is int) return ClaimValueTypes.Integer;
			if (value is long) return ExtendedClaimValueTypes.Long;
			if (value is byte) return ExtendedClaimValueTypes.UnsignedByte;
			if (value is ushort) return ExtendedClaimValueTypes.UnsignedShort;
			if (value is uint) return ClaimValueTypes.Integer;
			if (value is ulong) return ExtendedClaimValueTypes.UnsignedLong;
			if (value is float) return ExtendedClaimValueTypes.Float;
			if (value is double) return ClaimValueTypes.Double;
			if (value is TimeSpan) return ExtendedClaimValueTypes.Duration;
			if (value is DateTime) return ClaimValueTypes.DateTime;
			if (value is DateTimeOffset) return ClaimValueTypes.DateTime;
			if (value is Guid) return null;

			return null;
		}

		public static class ExtendedClaimValueTypes
		{
			public const string Byte = "http://www.w3.org/2001/XMLSchema#byte";
			public const string UnsignedByte = "http://www.w3.org/2001/XMLSchema#unsignedByte";
			public const string Short = "http://www.w3.org/2001/XMLSchema#short";
			public const string UnsignedShort = "http://www.w3.org/2001/XMLSchema#unsignedShort";
			public const string Long = "http://www.w3.org/2001/XMLSchema#long";
			public const string UnsignedLong = "http://www.w3.org/2001/XMLSchema#unsignedLong";
			public const string Decimal = "http://www.w3.org/2001/XMLSchema#decmial";
			public const string Float = "http://www.w3.org/2001/XMLSchema#float";
			public const string Duration = "http://www.w3.org/2001/XMLSchema#duration";
		}
	}
}
