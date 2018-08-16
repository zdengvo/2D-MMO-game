using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;

public class BuildManager : MonoBehaviour
{
    //Singleton
    private static BuildManager instance;
    public static BuildManager Instance
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

    public static Canvas previousInfoTile;
    public static GameObject previousInfoTileOutline;

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

    private int[,] map;
    private int map_width;
    private int map_height;
    private int number_of_houses;
    private int[] possibleMapTiles;
    private Dictionary<string, string> tileName;
    private Dictionary<string, int> houseLevel;
    private int countDiffrentHouses;

    [SerializeField]
    private Transform parent;
    [SerializeField]
    private TileDisplay displayTile;

    [SerializeField]
    private Text textCount;
    private int houseCount;

    [SerializeField]
    private GameObject loadingPanel;
    [SerializeField]
    private Text loadingText;
    private bool isLoadFinished;

    //Test Cheat - delete later
    private int[] nextHouseXpos;
    private int[] nextHouseYpos;

    private void Awake()
    {
        tileName = new Dictionary<string, string>();
        houseLevel = new Dictionary<string, int>();

        GameManager.Instance.OnHouseCountChanged += OnHouseCountChange;
        GameManager.Instance.OnCameraMove += DisplayMap;

        isLoadFinished = false;
        loadingText.text = "Loading";
        StartCoroutine(LoadingScreen());
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

        //Delete later
        nextHouseXpos = new int[number_of_houses+1];
        nextHouseYpos = new int[number_of_houses+1];

        //Create map houses
        int currentHouses = 0;
        while (currentHouses <= number_of_houses)
        {
            int x = UnityEngine.Random.Range(0, map_width);
            int y = UnityEngine.Random.Range(0, map_height);
            int houseType = UnityEngine.Random.Range(0, countDiffrentHouses);

            //Check if there is already house
            if (map[x, y] == GetTileNumber("house1") || map[x, y] == GetTileNumber("house2"))
            {
                continue;
            }

            map[x, y] = (houseType == 0) ? GetTileNumber("house1") : GetTileNumber("house2");

            //Delete later
            nextHouseXpos[currentHouses] = x;
            nextHouseYpos[currentHouses] = y;

            currentHouses++; 
        }

        //Create map tiles
        for (int i = 0; i < map_width; i++)
        {
            for (int j = 0; j < map_height; j++)
            {
                int type = UnityEngine.Random.Range(0, possibleMapTiles.Length);

                if (map[i, j] == GetTileNumber("house1") || map[i, j] == GetTileNumber("house2"))
                {
                    houseCount++;
                    continue;
                }

                if (type == GetTileNumber("house1") || type == GetTileNumber("house2"))
                {
                    type = 0;
                }

                map[i, j] = possibleMapTiles[type];
            }
        }
        OnHouseCountChange();
        DisplayMap();
        Debug.Log("Loaded after " + Time.realtimeSinceStartup + " - houses on the map: " + houseCount);
        isLoadFinished = true;
        loadingPanel.SetActive(false);
    }

    public void DisplayMap()
    {
        float camX = Camera.main.gameObject.transform.position.x;
        float camY = Camera.main.gameObject.transform.position.y;
       
        float xStart = camX - 3;
        float xEnd = camX + 4;

        float yStart = camY - 5;
        float yEnd = camY + 6;

        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        for (int x = (int)xStart; x < xEnd; x++)
        {
            for (int y = (int)yStart; y < yEnd; y++)
            {
                if (x < 0 || x > map_width - 1 || y < 0 || y > map_height - 1)
                {
                    continue;
                }

                string type = GetTileType(map[x, y]);

                if (map[x, y] == GetTileNumber("house1") || map[x, y] == GetTileNumber("house2"))
                {
                    HouseTile houseTile = new HouseTile(type, tileName[type], houseLevel[type]);
                    displayTile.SetUpTile(houseTile, GetTileSprite(type));
                    Instantiate(displayTile, new Vector3(x, y, 0), Quaternion.identity, parent);
                    continue;
                }

                Tile tile = new Tile(type, tileName[type]);
                displayTile.SetUpTile(tile, GetTileSprite(type));
                Instantiate(displayTile, new Vector3(x, y, 0), Quaternion.identity, parent);
            }
        }
    }

    private IEnumerator LoadingScreen()
    {
        string s = ".";
        while (isLoadFinished == false)
        {
            loadingText.text += s;

            yield return new WaitForSeconds(0.25f);
        }
    }

    public Tile GetDefaultTile()
    {
        string type = GetTileType((int)TileTypes.grass);
        return new Tile(type, tileName[type]);
    }

    public Sprite GetDefaultTileSprite()
    {
        string type = GetTileType((int)TileTypes.grass);
        return Resources.Load<Sprite>("map_resources/" + type);
    }

    public Sprite GetTileSprite(string type)
    {
        return Resources.Load<Sprite>("map_resources/" + type);
    }

    public void OnHouseCountChange()
    {
        houseCount--;
        textCount.text = houseCount.ToString();
    }

    public int GetTileNumber(string tileType)
    {
        return (int)System.Enum.Parse(typeof(TileTypes), tileType);
    }

    public string GetTileType(int tileNumber)
    {
        return System.Enum.GetName(typeof(TileTypes), tileNumber);
    }

    public int[,] GetMap()
    {
        return map;
    }

    public int GetMapWidth()
    {
        return map_width;
    }
    public int GetMapHeight()
    {
        return map_height;
    }

    //Delete Later
    public int[] GetAllXpos()
    {
        return nextHouseXpos;
    }
    //Delete Later
    public int[] GetAllYpos()
    {
        return nextHouseYpos;
    }
}
