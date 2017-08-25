/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;

namespace Site.Areas.Account
{
    /// <summary>
    /// Throttling unauthenticated user requests to the CRM service by restricting the amount of failed login attempts within a configurable amount of time. 
    /// Once the configured amount of login attempts has been hit the system will inform the user they need to wait a configured amount of time before being able to retry.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ThrottleAttribute : ActionFilterAttribute
    {
        public string Name { get; set; }

        //Number of attempts until denied Initial Default value = 1000
        private int Attempts { get; set; }

        //Time span for max attempts to be within Initial Default value = 3 min
        private double TimeLimit { get; set; }

        //Wait time for user once denied request occurs Initial Default value = 10 min
        private double TimeWait { get; set; }

        private const int DefaultAttempts = 1000;

        private readonly TimeSpan _defaultTimeLimit = new TimeSpan(00, 03, 00);

        private readonly TimeSpan _defaultTimeWait = new TimeSpan(00, 10, 00);

        public ThrottleAttribute()
        {
            var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
            var attempts = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website,
                    "Authentication/LoginThrottling/MaxInvaildAttemptsFromIPAddress");
            var timeLimit = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website,
                    "Authentication/LoginThrottling/MaxAttemptsTimeLimitTimeSpan");
            var timeWait = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website,
                    "Authentication/LoginThrottling/IpAddressTimeoutTimeSpan");

            int customAttempts;
            TimeSpan customTimeLimit;
            TimeSpan customTimeWait;

            Attempts = int.TryParse(attempts, out customAttempts) ? customAttempts : DefaultAttempts;
            TimeLimit = TimeSpan.TryParse(timeLimit, out customTimeLimit)
                ? customTimeLimit.TotalMinutes
                : _defaultTimeLimit.TotalMinutes;
            TimeWait = TimeSpan.TryParse(timeWait, out customTimeWait)
                ? customTimeWait.TotalMinutes
                : _defaultTimeWait.TotalMinutes;
        }

        public override void OnActionExecuting(ActionExecutingContext executingContext)
        {
            var key = string.Concat(Name, "-", GetIpAddress());
            var countLock = new CountLock();
            var castLocked = HttpRuntime.Cache[key] as CountLock;

            if (castLocked != null && castLocked.IsLocked)
            {
                executingContext.Controller.ViewBag.Locked = true;
                executingContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
            }

            else
            {
                var cacheExperation = TimeLimit;
                countLock.Count = 1;
                countLock.IsLocked = false;

                if (castLocked != null)
                {
                    countLock.Count = castLocked.Count + 1;
                    if (countLock.Count > Attempts)
                    {
                        cacheExperation = TimeWait;
                        countLock.IsLocked = true;

                        executingContext.Controller.ViewBag.Locked = true;
                        executingContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    }
                }
                
                HttpRuntime.Cache.Insert(key,
                        countLock,
                        null,
                        DateTime.Now.AddMinutes(cacheExperation),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Low, null);
            }
        }

        /// <summary>
        /// Removes the cache object if the login was successful
        /// </summary>
        /// <param name="executedContext"></param>
        public override void OnActionExecuted(ActionExecutedContext executedContext)
        {
            var key = string.Concat(Name, "-", GetIpAddress());
            if (executedContext.Controller.ViewBag.LoginSuccessful)
            {
                HttpRuntime.Cache.Remove(key);
            }
        }


        private class CountLock
        {
            public int Count { get; set; }
            public bool IsLocked { get; set; }
        }

        // if use load balancer 
        private static string GetIpAddress()
        {
            var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            else
            {
                ip = ip.Split(',')
                        .Last()
                        .Trim();
            }

            return ip;
        }
    }
}
