using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;

public class BuildManager : MonoBehaviour
{
    //Singleton in unity
    private static BuildManager instance;
    public static BuildManager I
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<BuildManager>();
            }
            return instance;
        }
    }

    public enum TileTypes
    {
        grass,
        trees1,
        trees2,
        sand,
        water,
        house1,
        house2,
        count
    } 

    [SerializeField]
    private string URL;
    private string json;

    private int map_width;
    private int map_height;
    private int number_of_houses;
    private int[] possibleMapTiles;
    private Dictionary<string, string> tileName;
    private Dictionary<string, int> houseLevel;
    private int countDiffrentHouses;

    [SerializeField]
    private GameObject[] tiles;
    private int[,] map;
    [SerializeField]
    private Transform parent;
    [SerializeField]
    private TileDisplay displayTile;

    [SerializeField]
    private Text textCount;

    private void Awake()
    {
        tileName = new Dictionary<string, string>();
        houseLevel = new Dictionary<string, int>();
    }

    IEnumerator Start()
    {
        //Load JSON
        using (WWW www = new WWW(URL))
        {
            yield return www;

            json = www.text;
        }

        Initialization();
    }
    private void Initialization()
    {
        //Parse JSON
        JSONNode jsonObject = JSON.Parse(json);
        
        map_width = jsonObject["map_width"].AsInt;
        map_height = jsonObject["map_height"].AsInt;
        number_of_houses = jsonObject["number_of_houses"].AsInt;

        GetRequiredMapTiles(jsonObject);

        CreateMap();
    }

    private void GetRequiredMapTiles(JSONNode jsonObject)
    {
        int numberOfMapTiles = jsonObject["tiles"].AsArray.Count;
        possibleMapTiles = new int[numberOfMapTiles];

        for (int i = 0; i < numberOfMapTiles; i++)
        {
            string tileType = jsonObject["tiles"][i][0];

            tileName[tileType] = jsonObject["tiles"][i][1];
            possibleMapTiles[i] = GetTileNumber(tileType);

            if (tileType.Equals(GetTileType((int)TileTypes.house1)))
            {
                houseLevel[tileType] = jsonObject["tiles"][i][2];
                countDiffrentHouses++;
            }
            else if (tileType.Equals(GetTileType((int)TileTypes.house2)))
            {
                houseLevel[tileType] = jsonObject["tiles"][i][2];
                countDiffrentHouses++;
            }
        }
    }

    private void CreateMap()
    {
        map = new int[map_width, map_height];

        //Create map houses
        int currentHouses = 0;
        while (currentHouses <= number_of_houses)
        {
            int x = Random.Range(0, map_width);
            int y = Random.Range(0, map_height);
            int houseType = Random.Range(0, countDiffrentHouses);

            map[x, y] = (houseType == 0) ? GetTileNumber("house1") : GetTileNumber("house2");
            
            //Setup and create house tile on map
            string type = GetTileType(map[x, y]);
            HouseTile houseTile = new HouseTile(type, tileName[type], houseLevel[type]);
            displayTile.SetUpTile(houseTile, GetTileSprite(type));
            Instantiate(displayTile, new Vector3(x, y, 0), Quaternion.identity, parent);

            currentHouses++;
        }

        currentHouses = 0;
        //Create map tiles
        for (int i = 0; i < map_width; i++)
        {
            for (int j = 0; j < map_height; j++)
            {
                int type = Random.Range(0, possibleMapTiles.Length);

                if (map[i, j] == GetTileNumber("house1") || map[i, j] == GetTileNumber("house2"))
                {
                    currentHouses++;
                    continue;
                }

                map[i, j] = possibleMapTiles[type];
            }
        }

        DisplayNumberOfHouses(--currentHouses);

        //Test - Make each tile of the map interactable according to its type (show name of the tile on tap)
        //Remove later
        for (int i = 0; i < 5; i++)
        {
            string type = GetTileType(i);
            Tile houseTile = new Tile(type, tileName[type]);
            displayTile.SetUpTile(houseTile, GetTileSprite(type));
            Instantiate(displayTile, new Vector3(i, 1, 0), Quaternion.identity, parent);
        }
    }

    private Sprite GetTileSprite(string type)
    {
        return Resources.Load<Sprite>("map_resources/" + type);
    }

    public void DisplayNumberOfHouses(int count)
    {
        textCount.text = count.ToString();
        Debug.Log(Time.realtimeSinceStartup + " " + "Finish" + " " + count);
    }

    public int GetTileNumber(string tileType)
    {
        return (int)System.Enum.Parse(typeof(TileTypes), tileType);
    }

    public string GetTileType(int tileNumber)
    {
        return System.Enum.GetName(typeof(TileTypes), tileNumber);
    }
}
