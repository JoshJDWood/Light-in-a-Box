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

    LightBeam2D beam2D2;
    List<Vector3> shapePathRD1 = new List<Vector3>();
    int bounces = 0;

    Vector3[] lightPath = { new Vector3(1, 1), new Vector3(1, 0), new Vector3(0, 1) };

    // Start is called before the first frame update
    void Start()
    {
        Shine(gameObject.transform.position);
    }
        

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Destroy(GameObject.Find("2D Lightbeam"));
            Destroy(GameObject.Find("2D Lightbeam"));
            rayDataSet.Clear();
            shapePath.Clear();
            shapePathRD1.Clear();
            Shine(gameObject.transform.position);
        }
    }

    void Shine(Vector2 pos)
    {
        Vector2 dir = new Vector2(0, 1);
        int angleincrement = 1;
        for (int i = 0; i < 360; i += angleincrement)
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

        if (hit.collider.gameObject.tag == "Mirror" && bounces < 3)
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
        int reflectionD1S = 0;
        int reflectionD1E = 0;
        bool inReflectionDepth1 = false;
        foreach (RayData idx in rayDataSet)
        {
            if (idx.bounces > 0 && !inReflectionDepth1)
            {
                reflectionD1S = count;
                inReflectionDepth1 = true;
            }
            else if(idx.bounces == 0 && inReflectionDepth1)
            {
                reflectionD1E = count;
                inReflectionDepth1 = false;
                OrderShapePath(reflectionD1S, reflectionD1E, 1);
                beam2D2 = new LightBeam2D(shapePathRD1.ToArray());
                shapePathRD1.Clear();
            }
            shapePath.Add(idx.hits[0]);
            count++;
        }

        beam2D = new LightBeam2D(shapePath.ToArray());
    }

    void OrderShapePath(int reflectionS, int reflectionE, int depth)
    {
        for (int i = reflectionS; i < reflectionE; i++)
        {
            shapePathRD1.Add(rayDataSet[i].hits[depth - 1]);
        }
        for (int i = reflectionE - 1; i >= reflectionS; i--)
        {
            shapePathRD1.Add(rayDataSet[i].hits[depth]);
        }
    }
}
