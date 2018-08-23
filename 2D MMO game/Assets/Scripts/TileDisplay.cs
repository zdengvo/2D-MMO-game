using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TileDisplay : MonoBehaviour {

    [SerializeField]
    private Canvas info;
    [SerializeField]
    private GameObject outline;
    [SerializeField]
    private Text textName;
    [SerializeField]
    private Text textLevel;
    [SerializeField]
    private Button demolishButton;

    private void Awake()
    {
        demolishButton.onClick.AddListener(Demolish);
    }

    public void SetUpTile(Tile tile, Sprite sprite)
    {
        GetComponent<SpriteRenderer>().sprite = sprite;
        textName.text = tile.tileName;

        ShowLevelAndDestroyButton(false);

        if (tile is HouseTile)
        {
            textLevel.text = ((HouseTile)tile).level.ToString();

            ShowLevelAndDestroyButton(true);
        }
    }

    private void ShowLevelAndDestroyButton(bool state)
    {
        textLevel.gameObject.SetActive(state);
        demolishButton.gameObject.SetActive(state);
    }
    
    private void OnMouseDown()
    {
        if (IsPointerOverUIObject())
        {
            return;
        }

        if (BuildManager.previousInfoTile != null && BuildManager.previousInfoTileOutline)
        {
            BuildManager.previousInfoTile.gameObject.SetActive(false);
            BuildManager.previousInfoTileOutline.gameObject.SetActive(false);

            //If i click on same tile again turn off info
            if (BuildManager.previousInfoTile == info || BuildManager.previousInfoTileOutline == outline)
            {
                BuildManager.previousInfoTile = null;
                BuildManager.previousInfoTileOutline = null;
                return;
            }
        }

        info.gameObject.SetActive(true);
        outline.gameObject.SetActive(true);
        BuildManager.previousInfoTile = info;
        BuildManager.previousInfoTileOutline = outline;
    }

    private void Demolish()
    {
        //Update Map and make demolished tile as grass one
        BuildManager.Instance.GetMap()[(int)gameObject.transform.position.x, (int)gameObject.transform.position.y] = (int)BuildManager.TileTypes.grass;
        SetUpTile(BuildManager.Instance.GetDefaultTile(), BuildManager.Instance.GetDefaultTileSprite());

        GameManager.Instance.OnHouseCountChanged(); 
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
