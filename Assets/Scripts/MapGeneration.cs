using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class MapGeneration : MonoBehaviour {

    public GameObject wall;
    public GameObject floor;

    public List<List<int>> mapData;
    public string mapName = "map2.txt";

	// Use this for initialization
	public void BuildMap () {
        mapName = mapName;
        mapData = new List<List<int>>();
        ReadMap();
	}

    private void ReadMap()
    {
        GameObject map = new GameObject();
        map.name = "Map";
        string[] mapInfo = new StreamReader(Application.dataPath + "/Maps/" + mapName).ReadToEnd().Split('\n');
        for (int i = 0; i < mapInfo.Length; ++i)
        {
            mapData.Add(new List<int>());
            for (int j = 0; j < mapInfo[0].Length; ++j)
            {
                if (!char.IsNumber(mapInfo[i][j]))
                {
                    continue;
                }
                GameObject t;
                switch(mapInfo[i][j])
                {
                    case '9':
                        t = (GameObject)Instantiate(wall, new Vector3(j, -i, 0), Quaternion.identity);
                        t.transform.parent = map.transform;
                        break;
                    case '5':

                        break;
                    case '1':
                        t = (GameObject)Instantiate(floor, new Vector3(j, -i, 0), Quaternion.identity);
                        t.transform.parent = map.transform;
                        break;
                }
                
                mapData[i].Add(int.Parse(mapInfo[i][j].ToString()));
            }
        }
        
    }

}
