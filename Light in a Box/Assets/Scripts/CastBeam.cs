using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CastBeam : MonoBehaviour
{
    List<RayData> rayDataSet = new List<RayData>();
    List<Vector2> rayIndices = new List<Vector2>();
    List<Vector2> rayNormals = new List<Vector2>();
    List<Vector3> shapePath = new List<Vector3>();
    List<Vector3> shapePathReuseable = new List<Vector3>();
    int bounces = 0;
    Vector2 prevRayPos = new Vector2(0, 0);
    bool skipCorner = false;
    bool winColor = false;
    int maxBounces = 4;
    int beamCount = 0;
        

    public void Relight(bool beatPuzzle)
    {
        LightOff();
        beamCount = 0;
        winColor = beatPuzzle;
        rayDataSet.Clear();
        shapePath.Clear();
        shapePathReuseable.Clear();
        Shine(gameObject.transform.position);
    }

    public void LightOff()
    {
        for (int i = 0; i < beamCount; i++)
        {
            Destroy(GameObject.Find("2D Lightbeam " + i));
        }
    }

    void Shine(Vector2 pos)
    {
        Vector2 dir = new Vector2(1, 0);
        float angleincrement = 0.5f;
        for (float i = 0; i < 360; i += angleincrement)
        {
            RayData curRayData;
            dir = Quaternion.Euler(0, 0, angleincrement) * dir;
            CastRay(pos, dir);
            if (skipCorner)
            {
                skipCorner = false;
            }
            else
            {
                curRayData.bounces = bounces;
                curRayData.hits = rayIndices.ToArray();
                curRayData.normals = rayNormals.ToArray();
                rayDataSet.Add(curRayData);
            }

            bounces = 0;
            rayIndices.Clear();
            rayNormals.Clear();
        }

        FindShapePaths();
    }

    void CastRay(Vector2 pos, Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, dir, 10);
        float xDiff = Math.Abs(hit.point.x - prevRayPos.x);
        float yDiff = Math.Abs(hit.point.y - prevRayPos.y);
        if (xDiff * xDiff + yDiff * yDiff <= 0.00001) //fix for corners/(close to corners) that cause lighting errors
        {
            skipCorner = true;
            return;
        }

        rayIndices.Add(hit.point);
        rayNormals.Add(hit.normal);
        prevRayPos = hit.point;

        if (hit.collider.gameObject.CompareTag("Mirror") && bounces < maxBounces)
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

        new LightBeam2D(shapePath.ToArray(), beamCount, winColor);
        beamCount++;

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
            else if (count > 0 && bounces > 0)
            {
                for (int i = 0; i < Math.Min(bounces, rayDataSet[count - 1].bounces); i++)
                {
                    if (Math.Abs(rayDataSet[count].normals[i].x - rayDataSet[count - 1].normals[i].x) > 0.001
                        || (rayDataSet[count].hits[i] - rayDataSet[count - 1].hits[i]).magnitude > 0.25)
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
        if (depth % 2 == 0)
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
                new LightBeam2D(shapePathReuseable.ToArray(), beamCount, winColor);
                beamCount++;
            }
            shapePathReuseable.Clear();
        }
        else
        {
            for (int i = reflectionE - 1; i >= reflectionS; i--)
            {
                shapePathReuseable.Add(rayDataSet[i].hits[depth]);
            }
            for (int i = reflectionS; i < reflectionE; i++)
            {
                shapePathReuseable.Add(rayDataSet[i].hits[depth + 1]);
            }
            if (shapePathReuseable.Count > 2)
            {
                new LightBeam2D(shapePathReuseable.ToArray(), beamCount, winColor);
                beamCount++;
            }
            shapePathReuseable.Clear();
        }
    }
}
