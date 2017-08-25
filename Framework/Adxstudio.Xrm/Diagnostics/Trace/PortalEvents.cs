/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// These codes will be sent to IIS logs for reporting
    /// </summary>
    public enum PortalEvents
    {
        TraceError = 1001,
        TraceWarning = 1002,
        TraceInfo = 1003,
        FeatureUsage = 1004,
        Authentication = 1005
    }
}
