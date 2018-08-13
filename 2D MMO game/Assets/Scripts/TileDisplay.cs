using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileDisplay : MonoBehaviour {

    [SerializeField]
    private Canvas info;
    [SerializeField]
    private Text textName;
    [SerializeField]
    private Text textLevel;
    [SerializeField]
    private Button demolishButton;

    public void SetUpTile(Tile tile, Sprite sprite)
    {
        GetComponent<SpriteRenderer>().sprite = sprite;
        textName.text = tile.tileName;

        ShowLevelAndDestroy(false);

        if (tile is HouseTile)
        {
            textLevel.text = ((HouseTile)tile).level.ToString();

            ShowLevelAndDestroy(true);
        }
    }

    private void ShowLevelAndDestroy(bool state)
    {
        textLevel.gameObject.SetActive(state);
        demolishButton.gameObject.SetActive(state);
    }

    private void OnMouseDown()
    {
        info.gameObject.SetActive(true);
    }
}
