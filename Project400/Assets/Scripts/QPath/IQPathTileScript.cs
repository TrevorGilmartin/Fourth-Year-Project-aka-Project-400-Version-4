using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPath
{
    public interface IQPathTileScript
    {
        IQPathTileScript[] GetNeighbours();

        float AggregateCostToEnter(float costSoFar, IQPathTileScript sourceTile, IQPathUnitScript theUnit);
    }
}

