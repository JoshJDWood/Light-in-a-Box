using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CastBeam : MonoBehaviour
{
    LightBeam2D beam2D;
    List<RayData> rayDataSet = new List<RayData>();
    List<Vector2> rayIndices = new List<Vector2>();
    List<Vector3> shapePath = new List<Vector3>();
    List<Vector3> shapePathReuseable = new List<Vector3>();
    int bounces = 0;
    int maxBounces = 3;

    // Start is called before the first frame update
    void Start()
    {
        Shine(gameObject.transform.position);
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Destroy(GameObject.Find("2D Lightbeam"));
            Destroy(GameObject.Find("2D Lightbeam"));
            rayDataSet.Clear();
            shapePath.Clear();
            shapePathReuseable.Clear();
            Shine(gameObject.transform.position);
        }
    }

    void Shine(Vector2 pos)
    {
        Vector2 dir = new Vector2(0, 1);
        float angleincrement = 1;
        for (float i = 0; i < 360; i += angleincrement)
        {
            RayData curRayData;
            dir = Quaternion.Euler(0, 0, angleincrement) * dir;
            CastRay(pos, dir);
            curRayData.bounces = bounces;
            curRayData.hits = rayIndices.ToArray();
            rayDataSet.Add(curRayData);

            bounces = 0;
            rayIndices.Clear();
        }

        FindShapePaths();
    }

    void CastRay(Vector2 pos, Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, dir, 10);
        rayIndices.Add(hit.point);

        if (hit.collider.gameObject.tag == "Mirror" && bounces < maxBounces)
        {
            bounces++;
            Vector2 nextdir = Vector2.Reflect(dir, hit.normal);
            Vector2 nextpos = hit.point + nextdir * 0.0001f;

            CastRay(nextpos, nextdir);
        }
    }

    void FindShapePaths()
    {
        int count = 0;
        int prevDepth = 0;       
        int[] reflectionS = new int[maxBounces];//if i change the number of bouces possible in cast ray must change the size of these arrays
        int[] reflectionE = new int[maxBounces];

        foreach (RayData idx in rayDataSet)
        {            
            AssessDepth(idx.bounces);

            shapePath.Add(idx.hits[0]);
            count++;
        }

        beam2D = new LightBeam2D(shapePath.ToArray());

        void AssessDepth(int bounces)
        {
            if (bounces > prevDepth)
            {
                reflectionS[prevDepth] = count;
                prevDepth++;
                AssessDepth(bounces);
            }
            else if (bounces < prevDepth)
            {
                prevDepth--;
                reflectionE[prevDepth] = count;
                OrderShapePath(reflectionS[prevDepth], reflectionE[prevDepth], prevDepth);
                if(shapePathReuseable.Count > 2)
                {
                    beam2D = new LightBeam2D(shapePathReuseable.ToArray());
                }
                shapePathReuseable.Clear();
                AssessDepth(bounces);
            }
        }
    }

    void OrderShapePath(int reflectionS, int reflectionE, int depth)
    {
        for (int i = reflectionS; i < reflectionE; i++)
        {
            shapePathReuseable.Add(rayDataSet[i].hits[depth]);
        }
        for (int i = reflectionE - 1; i >= reflectionS; i--)
        {
            shapePathReuseable.Add(rayDataSet[i].hits[depth + 1]);
        }
    }
}
