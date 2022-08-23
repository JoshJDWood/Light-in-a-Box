using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BlockData
{
    public int id;
    public int r;

    public BlockData(int idIn, int rIn)
    {
        id = idIn;
        r = rIn;
    }

    public static bool operator ==(BlockData lhs, BlockData rhs)
    {
        return (lhs.id == rhs.id && lhs.r == rhs.r);
    }
    public static bool operator !=(BlockData lhs, BlockData rhs)
    {
        return !(lhs == rhs);
    }

}
