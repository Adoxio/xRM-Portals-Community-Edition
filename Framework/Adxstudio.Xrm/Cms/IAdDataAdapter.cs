/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Cms
{
    public interface IAdDataAdapter
    {
        IAd SelectAd(Guid adId);

        IAd SelectAd(string adName);

        IAdPlacement SelectAdPlacement(Guid adPlacementId);

        IAdPlacement SelectAdPlacement(string adPlacementName);

        IAd SelectRandomAd(Guid adPlacementId);

        IAd SelectRandomAd(string adPlacementName);
    }
}
