using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseTile : Tile
{
    public int level { get; set; }
    public HouseTile(string type, string tileName, int level) : base(type, tileName)
    {
        this.level = level;
    }
}
