using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPath
{
    public interface IQPathUnitScript
    {
        float CostToEnterHex(IQPathTileScript sourceTile, IQPathTileScript destinationTile);
    }
}
