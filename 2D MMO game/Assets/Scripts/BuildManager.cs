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
            if (instance == null)
            {
                instance = FindObjectOfType<BuildManager>();
            }
            return instance;
        }
    }

    public static Canvas previousInfoTile;
    public static GameObject previousInfoTileOutline;

    //For ZoomOut optimisation
    private static float previousHeight;
    private static float previousWidth;

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
    private Image loadingPanel;
    [SerializeField]
    private Text loadingText;

    //Test Cheat - delete later
    private int[] nextHouseXpos;
    private int[] nextHouseYpos;

    private void Awake()
    {
        tileName = new Dictionary<string, string>();
        houseLevel = new Dictionary<string, int>();

        GameManager.Instance.OnHouseCountChanged += OnHouseCountChange;
        GameManager.Instance.OnCameraMove += DisplayMap;
        GameManager.Instance.OnCameraMoveClear += OnCameraMoveClear;

        //Set up for ZoomOut display
        Camera cam = Camera.main;
        previousHeight = 2f * cam.orthographicSize;
        previousWidth = previousHeight * cam.aspect;

        loadingText.text = "Loading";
        StartCoroutine(LoadingScreen());
    }

    void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //Offline version
            TextAsset offlineJSON = Resources.Load<TextAsset>("offlineJSON");
            json = offlineJSON.text;

            Initialization();
        }
        else
        {
            StartCoroutine(LoadJSON());
        }
    }

    IEnumerator LoadJSON()
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
        nextHouseXpos = new int[number_of_houses];
        nextHouseYpos = new int[number_of_houses];

        //Create map houses
        int currentHouses = 0;
        while (currentHouses < number_of_houses)
        {
            int x = UnityEngine.Random.Range(225, map_width - 225);
            int y = UnityEngine.Random.Range(225, map_height - 225);
            int houseType = UnityEngine.Random.Range(0, countDiffrentHouses);

            //Check if there is already a house
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
        textCount.text = houseCount.ToString();
        DisplayMap("default");
        Debug.Log("Loaded after " + Time.realtimeSinceStartup + " - houses on the map: " + houseCount);
        StartCoroutine(FadeIn());
    }

    public void DisplayMap(string action)
    {
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        float camX = cam.gameObject.transform.position.x;
        float camY = cam.gameObject.transform.position.y;

        float xStart = camX - width / 2 + 1;
        float xEnd = camX + width / 2;

        float yStart = camY - height / 2 + 1;
        float yEnd = camY + height / 2;

        switch (action)
        {
            case "Left":
                MoveCameraLeft(xStart, yStart, yEnd);
                break;
            case "Right":
                MoveCameraRight(xEnd, yStart, yEnd);
                break;
            case "Up":
                MoveCameraUp(yEnd, xStart, xEnd);
                break;
            case "Down":
                MoveCameraDown(yStart, xStart, xEnd);
                break;
            case "ZoomIn":
                CameraZoomIn();
                break;
            case "ZoomOut":
                CameraZoomOut(xStart, xEnd, yStart, yEnd);
                break;
            default: CameraOnLoad(xStart, xEnd, yStart, yEnd);
                break;
        }
    }

    #region Optimisation

    private void MoveCameraLeft(float tempX_Y, float yStart, float yEnd)
    {
        int x = (int)tempX_Y - 1;

        for (int y = (int)yStart; y < yEnd; y++)
        {
            if (x < 0 || x > map_width - 1 || y < 0 || y > map_height - 1)
            {
                continue;
            }

            string type = GetTileType(map[x, y]);

            if (map[x, y] == GetTileNumber("house1") || map[x, y] == GetTileNumber("house2"))
            {
                CreateHouse(type, x, y);
                continue;
            }

            CreateTile(type, x , y);
        }
    }

    private void MoveCameraRight(float tempX_Y, float yStart, float yEnd)
    {
        int x = (int)tempX_Y + 1;

        for (int y = (int)yStart; y < yEnd; y++)
        {
            if (x < 0 || x > map_width - 1 || y < 0 || y > map_height - 1)
            {
                continue;
            }

            string type = GetTileType(map[x, y]);

            if (map[x, y] == GetTileNumber("house1") || map[x, y] == GetTileNumber("house2"))
            {
                CreateHouse(type, x, y);
                continue;
            }

            CreateTile(type, x, y);
        }
    }

    private void MoveCameraUp(float tempX_Y, float xStart, float xEnd)
    {
        int y = (int)tempX_Y + 1;

        for (int x = (int)xStart; x < xEnd; x++)
        {
            if (x < 0 || x > map_width - 1 || y < 0 || y > map_height - 1)
            {
                continue;
            }

            string type = GetTileType(map[x, y]);

            if (map[x, y] == GetTileNumber("house1") || map[x, y] == GetTileNumber("house2"))
            {
                CreateHouse(type, x, y);
                continue;
            }

            CreateTile(type, x, y);
        }
    }

    private void MoveCameraDown(float tempX_Y, float xStart, float xEnd)
    {
        int y = (int)tempX_Y - 1;

        for (int x = (int)xStart; x < xEnd; x++)
        {
            if (x < 0 || x > map_width - 1 || y < 0 || y > map_height - 1)
            {
                continue;
            }

            string type = GetTileType(map[x, y]);

            if (map[x, y] == GetTileNumber("house1") || map[x, y] == GetTileNumber("house2"))
            {
                CreateHouse(type, x, y);
                continue;
            }

            CreateTile(type, x, y);
        }
    }

    private void CameraZoomIn()
    {
        //Set up previous camera borders
        float[] borders = PreviousCameraBorders();

        foreach (Transform child in parent)
        {
            int x = (int)child.position.x;
            int y = (int)child.position.y;
            if (x < borders[0] || x > borders[1] || y < borders[2] || y > borders[3])
            {
                Destroy(child.gameObject);
            }
        }

        Camera cam = Camera.main;
        previousHeight = 2f * cam.orthographicSize;
        previousWidth = previousHeight * cam.aspect;
    }

    private void CameraZoomOut(float xStart, float xEnd, float yStart, float yEnd)
    {
        //Set up previous camera borders
        float[] borders = PreviousCameraBorders();

        for (int x = (int)xStart; x < xEnd; x++)
        {
            for (int y = (int)yStart; y < yEnd; y++)
            {
                if (x < 0 || x > map_width - 1 || y < 0 || y > map_height - 1)
                {
                    continue;
                }

                //Display only new tiles
                if (x < borders[0] || x > borders[1] || y < borders[2] || y > borders[3])
                {
                    string type = GetTileType(map[x, y]);

                    if (map[x, y] == GetTileNumber("house1") || map[x, y] == GetTileNumber("house2"))
                    {
                        CreateHouse(type, x, y);
                        continue;
                    }

                    CreateTile(type, x, y);
                }
            }
        }

        Camera cam = Camera.main;
        previousHeight = 2f * cam.orthographicSize;
        previousWidth = previousHeight * cam.aspect;
    }

    private void CameraOnLoad(float xStart, float xEnd, float yStart, float yEnd)
    {
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
                    CreateHouse(type, x, y);
                    continue;
                }

                CreateTile(type, x, y);
            }
        }
    }

    private void OnCameraMoveClear(string side)
    {
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        float camX = cam.gameObject.transform.position.x;
        float camY = cam.gameObject.transform.position.y;

        float xStart = camX - width / 2 + 1;
        float xEnd = camX + width / 2;

        float yStart = camY - height / 2 + 1;
        float yEnd = camY + height / 2;

        switch (side)
        {
            case "Left":
                ClearLeft(xStart, yStart, yEnd);
                break;
            case "Right":
                ClearRight(xEnd, yStart, yEnd);
                break;
            case "Up":
                ClearUp(yEnd, xStart, xEnd);
                break;
            case "Down":
                ClearDown(yStart, xStart, xEnd);
                break;
            default:
                break;
        }
    }

    private void ClearLeft(float tempX_Y, float yStart, float yEnd)
    {
        int x = (int)tempX_Y - 1;

        //Optimise better if needed (destroy previously saved objects - cam left border)
        foreach (Transform child in parent)
        {
            if (child.position.x <= x)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void ClearRight(float tempX_Y, float yStart, float yEnd)
    {
        int x = (int)tempX_Y + 1;

        //Optimise better if needed (destroy previously saved objects - cam right border)
        foreach (Transform child in parent)
        {
            if (child.position.x >= x)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void ClearUp(float tempX_Y, float xStart, float xEnd)
    {
        int y = (int)tempX_Y + 1;

        //Optimise better if needed (destroy previously saved objects - cam top border)
        foreach (Transform child in parent)
        {
            if (child.position.y >= y)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void ClearDown(float tempX_Y, float xStart, float xEnd)
    {
        int y = (int)tempX_Y - 1;

        //Optimise better if needed (destroy previously saved objects - cam bottom border)
        foreach (Transform child in parent)
        {
            if (child.position.y <= y)
            {
                Destroy(child.gameObject);
            }
        }
    }

    #endregion

    private float[] PreviousCameraBorders()
    {
        Camera cam = Camera.main;
        float camX = cam.gameObject.transform.position.x;
        float camY = cam.gameObject.transform.position.y;

        float previousXStart = camX - previousWidth / 2 + 1;
        float previousXEnd = camX + previousWidth / 2;

        float previousYStart = camY - previousHeight / 2 + 1;
        float previousYEnd = camY + previousHeight / 2;

        float[] borders = new float[4];
        borders[0] = previousXStart;
        borders[1] = previousXEnd;
        borders[2] = previousYStart;
        borders[3] = previousYEnd;

        return borders;
    }

    private void CreateHouse(string type, int x, int y)
    {
        HouseTile houseTile = new HouseTile(type, tileName[type], houseLevel[type]);
        displayTile.SetUpTile(houseTile, GetTileSprite(type));
        Instantiate(displayTile, new Vector3(x, y, 0), Quaternion.identity, parent);
    }

    private void CreateTile(string type, int x, int y)
    {
        Tile tile = new Tile(type, tileName[type]);
        displayTile.SetUpTile(tile, GetTileSprite(type));
        Instantiate(displayTile, new Vector3(x, y, 0), Quaternion.identity, parent);
    }

    private IEnumerator LoadingScreen()
    {
        string s = ".";
        while (true)
        {
            loadingText.text += s;

            yield return new WaitForSeconds(0.5f);

            if (loadingText.text.Contains("..."))
            {
                loadingText.text = "Loading";
            }
        }
    }

    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(1);

        float t = 1f;
        while (t > 0)
        {
            t -= Time.deltaTime;
            loadingPanel.color = new Color(0f, 0f, 0f, t);
            loadingText.color = new Color(0f, 0f, 0f, t);
            yield return 0;
        }

        loadingPanel.gameObject.SetActive(false);
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

        if (houseCount <= 0)
        {
            GameManager.Instance.GameOver();
        }
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
