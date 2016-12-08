using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

    [HideInInspector]
    public MapGeneration generationData;
    List<Pathfinding> population;
    public GameObject actor;
    public GameObject destination;
    public GameObject tile;
    public GameObject tile2;

    public int totalGenerations = 100;
    public int recombinationChance = 65;
    public int mutationChance = 1;

    public int generationCount = 1;
    public int populationSize = 1;
    public int chromosomeSegmentCount = 1;

    public int startX, startY;
    public int endX, endY;

    bool completed = false;
    float cost = float.MaxValue;
    long best;

    // Create a new map and population
    void Start( ) {
        PlayerPrefs.DeleteAll();
        if(!PlayerPrefs.HasKey("recomb"))
        {
            PlayerPrefs.SetInt("recomb", recombinationChance);
            PlayerPrefs.SetInt("mutate", mutationChance);
            PlayerPrefs.SetInt("size", populationSize);
            PlayerPrefs.SetInt("segcount", chromosomeSegmentCount);

            PlayerPrefs.SetInt("startX", startX);
            PlayerPrefs.SetInt("startY", startY);

            PlayerPrefs.SetInt("endX", endX);
            PlayerPrefs.SetInt("endY", endY);
        }

        recombinationChance = PlayerPrefs.GetInt("recomb");
        mutationChance = PlayerPrefs.GetInt("mutate");
        populationSize = PlayerPrefs.GetInt("size");
        chromosomeSegmentCount = PlayerPrefs.GetInt("segcount");
        startX = PlayerPrefs.GetInt("startX");
        startY = PlayerPrefs.GetInt("startY");

        endX = PlayerPrefs.GetInt("endX");
        endY = PlayerPrefs.GetInt("endY");

        //Generate Map
        generationData = gameObject.GetComponent<MapGeneration>();
        generationData.BuildMap();
        population = new List<Pathfinding>();
        for (int i = 0; i < populationSize; ++i)
        {
            GameObject g = (GameObject)Instantiate(actor, new Vector3(startX, -startY, -5), Quaternion.identity);
            g.name = "Actor" + generationCount.ToString();
            Pathfinding p = g.AddComponent<Pathfinding>();
            p.Create(this);
            population.Add(p);
        }
        Instantiate(destination, new Vector3(endX, -endY, -7), Quaternion.identity);

        
    }

    void NextGeneration()
    {
        print("Next Generation");
        generationCount++;
        population = population.OrderBy(go => go.distance).ToList();

        List<Pathfinding> tmp = new List<Pathfinding>();
        for(int i = 0; i < 20; ++i)
        {
            tmp.Add(population[0]);
            population.RemoveAt(0);
        }

        foreach (Pathfinding po in population)
        {
            Destroy(po.gameObject);
        }

        population = new List<Pathfinding>();

        foreach(Pathfinding po in tmp)
        {
            GameObject g = (GameObject)Instantiate(actor, new Vector3(startX, -startY, -5), Quaternion.identity);
            g.name = "Actor" + generationCount.ToString();
            Pathfinding p = g.AddComponent<Pathfinding>();
            p.Create(po);
            population.Add(p);
        }

        while (population.Count < populationSize)
        {
            int val1 = Random.Range(0, 20);
            int val2 = Random.Range(0, 20);

            if (Random.Range(0,100) < recombinationChance)
            {
                

                int recombLocation = Random.Range(1, chromosomeSegmentCount*2);
                GameObject g = (GameObject)Instantiate(actor, new Vector3(startX, -startY, -5), Quaternion.identity);
                g.name = "Actor" + generationCount.ToString();
                Pathfinding p = g.AddComponent<Pathfinding>();
                p.Create(tmp[val1].binaryString, tmp[val2].binaryString, recombLocation, this);
                population.Add(p);

                g = (GameObject)Instantiate(actor, new Vector3(startX, -startY, -5), Quaternion.identity);
                g.name = "Actor" + generationCount.ToString();
                p = g.AddComponent<Pathfinding>();
                p.Create(tmp[val2].binaryString, tmp[val1].binaryString, recombLocation, this);
                population.Add(p);

            }
            else
            {
                GameObject g = (GameObject)Instantiate(actor, new Vector3(startX, -startY, -5), Quaternion.identity);
                g.name = "Actor" + generationCount.ToString();
                Pathfinding p = g.AddComponent<Pathfinding>();
                p.Create(tmp[val1].binaryString, tmp[val2].binaryString, 0, this);
                population.Add(p);

                g = (GameObject)Instantiate(actor, new Vector3(startX, -startY, -5), Quaternion.identity);
                g.name = "Actor" + generationCount.ToString();
                p = g.AddComponent<Pathfinding>();
                p.Create(tmp[val2].binaryString, tmp[val1].binaryString, 0, this);
                population.Add(p);
            }
            
        }
           
        foreach(Pathfinding po in tmp)
        {
           Destroy(po.gameObject);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        bool generationFinished = true;
        foreach (Pathfinding po in population)
        {
            if(po.finished == 2 && po.distance <= cost)

            {
                best = po.binaryString;
                if (totalGenerations == 100)
                {
                    totalGenerations = Mathf.Min(generationCount + 9, totalGenerations);
                    GameObject g = (GameObject)Instantiate(actor, new Vector3(startX, -startY, -5), Quaternion.identity);
                    g.name = "Actor" + generationCount.ToString();
                    Pathfinding p = g.AddComponent<Pathfinding>();
                    p.Create(this, best);

                    p.StartCoroutine("PrintPath", tile2);
                }
                cost = po.distance;
            }
            generationFinished = generationFinished && po.finished > 0;
        }
        if(generationCount > totalGenerations  && !completed)
        {
            completed = true;
            foreach(Pathfinding go in population)
            {
                Destroy(go.gameObject);
            }

            GameObject g = (GameObject)Instantiate(actor, new Vector3(startX, -startY, -5), Quaternion.identity);
            g.name = "Actor" + generationCount.ToString();
            Pathfinding p = g.AddComponent<Pathfinding>();
            p.Create(this, best);

            p.StartCoroutine("PrintPath", tile);
            return;
        }
        if(generationFinished && population.Count > 2 && !completed)
        {
            NextGeneration();
        }
    }
}
