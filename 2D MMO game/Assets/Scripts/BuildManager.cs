using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class BuildManager : MonoBehaviour
{
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
    private int[] mapTiles;
    private int countDiffrentHouses;

    [SerializeField]
    private GameObject[] tiles;
    private int[,] map;
    [SerializeField]
    private Transform parent;

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
        mapTiles = new int[numberOfMapTiles];

        for (int i = 0; i < numberOfMapTiles; i++)
        {
            string tileType = jsonObject["tiles"][i][0];
            mapTiles[i] = GetTileNumber(tileType);

            if (tileType.Equals(GetTileName((int)TileTypes.house1)) || tileType.Equals(GetTileName((int)TileTypes.house2)))
            {
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
            Instantiate(tiles[map[x, y]], new Vector3(x, y, 0), Quaternion.identity, parent);

            currentHouses++;
        }

        currentHouses = 0;
        //Create map tiles
        for (int i = 0; i < map_width; i++)
        {
            for (int j = 0; j < map_height; j++)
            {
                int type = Random.Range(0, mapTiles.Length);

                if (map[i, j] == GetTileNumber("house1") || map[i, j] == GetTileNumber("house2"))
                {
                    currentHouses++;
                    continue;
                }

                map[i, j] = mapTiles[type];
            }
        }

        Debug.Log(Time.realtimeSinceStartup + " " + "Finish" + " " + --currentHouses);
    }

    public int GetTileNumber(string tileName)
    {
        return (int)System.Enum.Parse(typeof(TileTypes), tileName);
    }

    public string GetTileName(int tileNumber)
    {
        return System.Enum.GetName(typeof(TileTypes), tileNumber);
    }
}
