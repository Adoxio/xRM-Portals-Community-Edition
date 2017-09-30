/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;
using System.Web.Routing;

namespace Site.Areas.Account
{
	public class AccountAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Account"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"Account/SignIn/Region",
				"{region}/SignIn",
				new { pageless = true, area = "Account", controller = "Login", action = "Login" });

			context.MapRoute(
				"Account/SignIn",
				"SignIn",
				new { pageless = true, area = "Account", controller = "Login", action = "Login" });

			context.MapRoute(
				"Account/Redeem/Region",
				"{region}/Register",
				new { pageless = true, area = "Account", controller = "Login", action = "RedeemInvitation" });

			context.MapRoute(
				"Account/Redeem",
				"Register",
				new { pageless = true, area = "Account", controller = "Login", action = "RedeemInvitation" });

			context.Routes.MapPageRoute(
				"Register/Region",
				"{region}/Account/Login/Register",
				"~/Areas/Account/Views/Login/Registration.aspx",  
				true,
				GetRouteValueDictionaryForNonMvc());

			context.Routes.MapPageRoute(
				"Register",
				"Account/Login/Register",
				"~/Areas/Account/Views/Login/Registration.aspx",
				true,
				GetRouteValueDictionaryForNonMvc());

			context.MapRoute(
				"Account/Login/Region",
				"{region}/Account/{controller}/{action}",
				new { pageless = true, area = "Account", action = "Login" },
				new { controller = "Login" });

			context.MapRoute(
				"Account/Login",
				"Account/{controller}/{action}",
				new { pageless = true, area = "Account", action = "Login" },
				new { controller = "Login" });

			context.MapRoute(
				"Account/Manage/Region",
				"{region}/Account/{controller}/{action}",
				new { pageless = true, area = "Account", action = "Index" },
				new { controller = "Manage" });

			context.MapRoute(
				"Account/Manage",
				"Account/{controller}/{action}",
				new { pageless = true, area = "Account", action = "Index" },
				new { controller = "Manage" });

			context.MapRoute("Facebook/Pages/Region", "{region}/app/facebook", new { pageless = true, controller = "Login", action = "FacebookExternalLoginCallback" });
			context.MapRoute("Facebook/Pages", "app/facebook", new { pageless = true, controller = "Login", action = "FacebookExternalLoginCallback" });

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private static RouteValueDictionary GetRouteValueDictionaryForNonMvc()
		{
			var routeValueDictionary = new RouteValueDictionary();
			routeValueDictionary.Add("nonMVC", true);
			return routeValueDictionary;
		}
	}
}
