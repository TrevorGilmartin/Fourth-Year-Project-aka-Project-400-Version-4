using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPath
{
    /// <summary>
    /// 
    ///   Tile[] ourPath = QPath.FindPath( ourWorld, theUnit, startTile, endTile );
    /// 
    ///   theUnit is a object that is the thing actually trying to path between
    ///   tiles.  It might have special logic based on its movement type and the
    ///   type of tiles being moved through
    /// 
    ///   Our tiles need to be able to return the following information:
    ///     1)  List of neighbours
    ///     2)  The aggregate cost to enter this tile from another tile
    /// 
    /// </summary>
    public static class QPathScript
    {

        public static T[] FindPath<T>(
            IQPathWorldScript world,
            IQPathUnitScript unit,
            T startTile,
            T endTile,
            CostEstimateDelegate costEstimateFunction
            ) where T : IQPathTileScript
        {
            if (world == null || unit == null || startTile == null || endTile == null)
            {
                Debug.LogError("Null Value passed  to QPath::FindPath");
                return null;
            }

            // Call on our actual path solver
            QPath_AStarScript<T> resolver = new QPath_AStarScript<T>(world, unit, startTile, endTile, costEstimateFunction);
            resolver.DoWork();

            return resolver.GetList();
        }
    }
    public delegate float CostEstimateDelegate(IQPathTileScript a, IQPathTileScript b);
}
