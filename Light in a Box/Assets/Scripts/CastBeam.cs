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
    List<Vector3> shapePath2 = new List<Vector3>();
    int bounces = 0;

    Vector3[] lightPath = { new Vector3(1, 1), new Vector3(1, 0), new Vector3(0, 1) };

    // Start is called before the first frame update
    void Start()
    {
        Shine(gameObject.transform.position);
        FindShapePaths();
        beam2D = new LightBeam2D(shapePath.ToArray());
        beam2D2 = new LightBeam2D(shapePath2.ToArray());
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
            shapePath2.Clear();
            Shine(gameObject.transform.position);
            FindShapePaths();
            beam2D = new LightBeam2D(shapePath.ToArray());
            beam2D2 = new LightBeam2D(shapePath2.ToArray());
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
    }

    void CastRay(Vector2 pos, Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, dir, 10);
        rayIndices.Add(hit.point);

        if (hit.collider.gameObject.tag == "Mirror" && bounces < 5)
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
        int reflectionS = 0;
        int reflectionE = 0;
        bool inreflection = false;
        foreach (RayData idx in rayDataSet)
        {
            if (idx.bounces > 0 && !inreflection)
            {
                reflectionS = count;
                inreflection = true;
            }
            else if(idx.bounces == 0 && inreflection)
            {
                reflectionE = count;
                inreflection = false;
            }
            shapePath.Add(idx.hits[0]);
            count++;
        }

        for (int i = reflectionS; i < reflectionE; i++)
        {
            shapePath2.Add(rayDataSet[i].hits[0]);
        }
        for (int i = reflectionE - 1; i >= reflectionS; i--)
        {
            shapePath2.Add(rayDataSet[i].hits[1]);
        }
    }
}
