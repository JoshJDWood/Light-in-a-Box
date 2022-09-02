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

    public override bool Equals(object obj)
    {
        return obj is BlockData data &&
               id == data.id &&
               r == data.r;
    }

    public override int GetHashCode()
    {
        int hashCode = 1727120629;
        hashCode = hashCode * -1521134295 + id.GetHashCode();
        hashCode = hashCode * -1521134295 + r.GetHashCode();
        return hashCode;
    }
}
