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
}
