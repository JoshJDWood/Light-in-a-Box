using System.Collections;
using System.Collections.Generic;
using System;
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
        Vector2 dir = new Vector2(-1, 0);
        float angleincrement = 1f;
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
        int[] reflectionS = new int[maxBounces];
        int[] reflectionE = new int[maxBounces];
        Vector2 newDir;
        Vector2 prevDir;

        foreach (RayData idx in rayDataSet)
        {            
            AssessDepth(idx.bounces);

            shapePath.Add(idx.hits[0]);
            count++;
        }

        for (int i = 0; i < prevDepth; i++)
        {
            reflectionE[i] = count;
            OrderShapePath(reflectionS[i], reflectionE[i], i);            
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
                AssessDepth(bounces);
            }
            else if (count > 1 && bounces > 0)
            {
                int prev2Depths = Math.Min(prevDepth, rayDataSet[count - 2].bounces);
                for (int i = 0; i < Math.Min(bounces, prev2Depths); i++)
                {
                    newDir = rayDataSet[count].hits[i] - rayDataSet[count - 1].hits[i];
                    prevDir = rayDataSet[count - 1].hits[i] - rayDataSet[count - 2].hits[i];
                    if (Math.Abs(newDir.normalized.x - prevDir.normalized.x) > 0.01)
                    {
                        reflectionE[i] = count;
                        OrderShapePath(reflectionS[i], reflectionE[i], i);                        
                        reflectionS[i] = count;
                    }
                }
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
        if (shapePathReuseable.Count > 2)
        {
            beam2D = new LightBeam2D(shapePathReuseable.ToArray());
        }
        shapePathReuseable.Clear();
    }
}
