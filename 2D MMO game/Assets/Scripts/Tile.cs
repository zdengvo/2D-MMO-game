using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile {

    public string type { get; set; }
    public string tileName { get; set; }

    public Tile(string type, string tileName)
    {
        this.type = type;
        this.tileName = tileName;
    }
}
