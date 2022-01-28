using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBeam
{
    Vector2 pos, dir;

    GameObject beamObj;
    LineRenderer beam;
    List<Vector2> beamIndices = new List<Vector2>();

    public LightBeam(Vector2 pos, Vector2 dir, Material material)
    {
        this.beam = new LineRenderer();
        this.beamObj = new GameObject();
        this.beamObj.name = "Light Beam";
        this.pos = pos;
        this.dir = dir;

        this.beam = this.beamObj.AddComponent(typeof(LineRenderer)) as LineRenderer;
        this.beam.sortingLayerName = "1";
        this.beam.startWidth = 0.05f;
        this.beam.endWidth = 0.05f;
        this.beam.material = material;
        this.beam.startColor = Color.yellow;
        this.beam.endColor = Color.yellow;

        beamIndices.Add(pos);
        CastRay(pos, dir, beam);
    }

    void CastRay(Vector2 pos, Vector2 dir, LineRenderer beam)
    {      
        
        RaycastHit2D hit = Physics2D.Raycast(pos, dir, 10);
        beamIndices.Add(hit.point);

        if (hit.collider.gameObject.tag == "Mirror")
        {
            
            Vector2 nextdir = Vector2.Reflect(dir, hit.normal);
            Vector2 nextpos = hit.point + nextdir * 0.0001f;            

            CastRay(nextpos, nextdir, beam);
        }
        UpdateBeam();
    }

    void UpdateBeam()
    {
        int count = 0;
        beam.positionCount = beamIndices.Count;

        foreach(Vector2 idx in beamIndices)
        {
            beam.SetPosition(count, idx);
            count++;
        }
    }
}
