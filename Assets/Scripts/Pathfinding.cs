using UnityEngine;
using System.Collections;

public class Pathfinding : MonoBehaviour
{

    //1 indicates chromosome finished, 2 indicates destination reached
    public int finished = 0;
    public float distance = 0;

    //Number of two bit segments in the binaryString
    public int binarySegments = 3;

    /* Chromosome for the actor, contains the movement data for the actor
       00 = Down, 01 = left, 10 = up, 11 = right
    */
    public long binaryString = 0;

    /* Cost of the sum of movement, all moves increment the cost based on the next tiles cost
       After all movements on the chromosome have been used up/ destination has been reached, 
       the distance from the end point is calculated and added to this score. Null movements,
       (attempting to move to a wall) will still add the tile penalty
    */
    public int pathCost = 0;

    //Contains the data for the map currently in use
    GameManager mapData;

    //Location within the Map used for pathfinding
    public int xPos = 0;
    public int yPos = 0;

    public void Create(Pathfinding pf)
    {
        xPos = pf.mapData.startX;
        yPos = pf.mapData.startY;
        binarySegments = pf.binarySegments;
        binaryString = pf.binaryString;
        mapData = pf.mapData;
        StartCoroutine("PathFind");
    }

    public void Create(GameManager gM, long bst)
    {
        xPos = gM.startX;
        yPos = gM.startY;
        binarySegments = gM.chromosomeSegmentCount;
        binaryString = bst;
        mapData = gM;
    }

    /*
        Create a chromosome and start the movement coroutine to calculate the fitness of the chromosome

        segmentCount, number of two bit segments in the chromosome
    */
    public void Create(GameManager gm)
    {
        xPos = gm.startX;
        yPos = gm.startY;
        this.transform.position = new Vector3(xPos, -yPos, -5);
        mapData = gm;
        binarySegments = gm.chromosomeSegmentCount;
        binaryString = Random.Range(0, 4);
        for (int i = 1; i < binarySegments; ++i)
        {
            binaryString = binaryString << 2;
            long val = Random.Range(0, 4);
            binaryString = binaryString | val;
        }

        StartCoroutine("PathFind");
    }

    public void Create(long chromosome1, long chromosome2, int recombPoint, GameManager gm)
    {
        xPos = gm.startX;
        yPos = gm.startY;
        this.transform.position = new Vector3(xPos, -yPos, -5);
        mapData = gm;
        binarySegments = gm.chromosomeSegmentCount;

        long tempBinaryString;
        //If recombination occurs use the two parent chromosomes to determine the chromosome
        if (recombPoint != 0 && recombPoint != gm.chromosomeSegmentCount * 2)
        {
            int secondaryPoint = (gm.chromosomeSegmentCount * 2) - recombPoint;
            tempBinaryString = (chromosome1 >> recombPoint) << recombPoint;
            long tempBinaryString2 = (chromosome2 << secondaryPoint) >> secondaryPoint;
            binaryString = tempBinaryString | tempBinaryString2;
        }
        else
        {
            binaryString = chromosome1;
        }

        tempBinaryString = binaryString;

        //Attemp to mutate the chromosome
        long current = 1;
        for (int i = 1; i < binarySegments * 2; ++i)
        {
            if (Random.Range(0, 100) < gm.mutationChance)
            {
                tempBinaryString = tempBinaryString ^ current;
            }
            current = current << 1;
        }
        binaryString = tempBinaryString;
        StartCoroutine("PathFind");

    }

    IEnumerator PathFind()
    {
        //Contains the remaining movements in the binary string after each call
        //Starts off equivalent to the Binary String and then is bit shifted after each move
        long remainder = binaryString;
        for (int i = 0; i < binarySegments; ++i)
        {
            if (xPos == mapData.endX && yPos == mapData.endY)
            {
                finished++;
                break;
            }
            long movementValue = remainder & 3;
            switch (movementValue)
            {
                case 0:
                    yPos += mapData.generationData.mapData[yPos + 1][xPos] == 9 ? 0 : 1;
                    distance += mapData.generationData.mapData[yPos + 1][xPos];
                    break;
                case 1:
                    xPos -= mapData.generationData.mapData[yPos][xPos - 1] == 9 ? 0 : 1;
                    distance += mapData.generationData.mapData[yPos][xPos - 1];
                    break;
                case 2:
                    yPos -= mapData.generationData.mapData[yPos - 1][xPos] == 9 ? 0 : 1;
                    distance += mapData.generationData.mapData[yPos - 1][xPos];
                    break;
                case 3:
                    xPos += mapData.generationData.mapData[yPos][xPos + 1] == 9 ? 0 : 1;
                    distance += mapData.generationData.mapData[yPos][xPos + 1];
                    break;
            }
            this.transform.position = new Vector3(xPos, -yPos, -5);
            remainder = remainder >> 2;
            yield return new WaitForSeconds(.3f);
        }
        finished++;
        if (finished !=2)
        {
            distance += 300;
        }
        distance += 100 * Mathf.Sqrt(Mathf.Pow(xPos - mapData.endX, 2) + Mathf.Pow(yPos - mapData.endY, 2));
    }

    public IEnumerator PrintPath(GameObject tile)
    {
        xPos = mapData.startX;
        yPos = mapData.startY;
        this.transform.position = new Vector3(xPos, -yPos, -5);
        Instantiate(tile, new Vector3(xPos, -yPos, -3), Quaternion.identity);
        long remainder = binaryString;
        for (int i = 0; i < binarySegments; ++i)
        {
            if (xPos == mapData.endX && yPos == mapData.endY)
            {
                break;
            }
            long movementValue = remainder & 3;
            switch (movementValue)
            {
                case 0:
                    yPos += mapData.generationData.mapData[yPos + 1][xPos] == 9 ? 0 : 1;
                    break;
                case 1:
                    xPos -= mapData.generationData.mapData[yPos][xPos - 1] == 9 ? 0 : 1;
                    break;
                case 2:
                    yPos -= mapData.generationData.mapData[yPos - 1][xPos] == 9 ? 0 : 1;
                    break;
                case 3:
                    distance += mapData.generationData.mapData[yPos][xPos + 1];
                    break;
            }
            Instantiate(tile, new Vector3(xPos, -yPos, -3), Quaternion.identity);
            this.transform.position = new Vector3(xPos, -yPos, -5);
            remainder = remainder >> 2;
            yield return new WaitForSeconds(.4f);
        }
    }
}
