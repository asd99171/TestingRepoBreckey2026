using System.Collections.Generic;
using UnityEngine;

public sealed class GridOccupancyIndex : MonoBehaviour
{
    private readonly Dictionary<Vector2Int, GridOccupant> occupants = new Dictionary<Vector2Int, GridOccupant>();

    public void Register(GridOccupant occ)
    {
        if (occ == null)
        {
            return;
        }

        occ.RefreshCellFromTransform();

        if (this.occupants.ContainsKey(occ.Cell))
        {
            this.occupants[occ.Cell] = occ;
        }
        else
        {
            this.occupants.Add(occ.Cell, occ);
        }
    }

    public void Unregister(GridOccupant occ)
    {
        if (occ == null)
        {
            return;
        }

        if (this.occupants.ContainsKey(occ.Cell) && this.occupants[occ.Cell] == occ)
        {
            this.occupants.Remove(occ.Cell);
        }
    }

    public GridOccupant GetAtCell(Vector2Int cell)
    {
        GridOccupant occ;
        if (this.occupants.TryGetValue(cell, out occ))
        {
            return occ;
        }

        return null;
    }

    public void Move(GridOccupant occ, Vector2Int from, Vector2Int to)
    {
        if (occ == null)
        {
            return;
        }

        if (this.occupants.ContainsKey(from) && this.occupants[from] == occ)
        {
            this.occupants.Remove(from);
        }

        occ.SetCell(to);

        if (this.occupants.ContainsKey(to))
        {
            this.occupants[to] = occ;
        }
        else
        {
            this.occupants.Add(to, occ);
        }
    }
}
