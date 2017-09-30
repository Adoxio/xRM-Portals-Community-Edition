/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;

namespace Adxstudio.Xrm.Web.Mvc
{
    /// <summary>
    /// custom attribute to validate csrf token for ajax requests.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AjaxValidateAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {

        private const string TokenName = "__RequestVerificationToken";

        /// <summary>
        /// The event which fires on authorization of a request.
        /// </summary>
        /// <param name="filterContext">gets the http context around the request.</param>
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            if (request.HttpMethod == WebRequestMethods.Http.Post)
            {
                //  Ajax POSTs and normal form posts have to be treated differently when it comes
                //  to validating the AntiForgeryToken
                if (request.IsAjaxRequest())
                {
                    this.Validate(filterContext.RequestContext);
                }
                else
                {
                    new ValidateAntiForgeryTokenAttribute()
                        .OnAuthorization(filterContext);
                }
            }
        }

        /// <summary>
        /// Custom Token Validation this is used in a standalone mode in http handlers.
        /// </summary>
        /// <param name="requestContext">Use in header key "__RequestVerificationToken" </param>
		public void Validate(RequestContext requestContext)
		{
            var request = requestContext.HttpContext.Request;

            if (request.HttpMethod != WebRequestMethods.Http.Post) return; //Only check Post requests


            var cookieToken = request.Cookies[TokenName];
            if (cookieToken == null)
            {
                throw new HttpAntiForgeryException();
            }

            //if the token has been added to the header use it instead as it might not exist on the form
            if (request.Headers[TokenName] != null)
            {
                AntiForgery.Validate(cookieToken.Value, request.Headers[TokenName]);
            }
            else
            {
                AntiForgery.Validate();
            }
		}
    }
}
