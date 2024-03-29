using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using System.Reflection;

public class LightBeam2D
{    
    GameObject beamObj;
    Light2D beam;
    //float falloffInt = 0.1f;
    float falloffSize = 0.04f;
    bool alphaBlendOnOverlap = true;
    private Color defualtColor = new Color(0.78f, 1, 1);
    private Color winColor = new Color(0.5f, 1, 1);

    public LightBeam2D(Vector3[] shapePath, int id, bool isWinColor)
    {
        this.beamObj = new GameObject();
        this.beamObj.name = "2D Lightbeam " + id;

        this.beam = this.beamObj.AddComponent(typeof(Light2D)) as Light2D;
        this.beam.lightType = Light2D.LightType.Freeform;
        if (isWinColor)
        {
            this.beam.color = winColor;
        }
        else
        {
            this.beam.color = defualtColor;
        }

        SetAlphaBlendOnOverlap(beam, alphaBlendOnOverlap);
        SetFalloffSize(beam, falloffSize);
        SetShapePath(beam, shapePath);        
        //SetFalloffIntensity(beam, falloffInt);
    }    

    ////for getting past read only in light2D////
    void SetFieldValue<T>(object obj, string name, T val)
    {
        var field = obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(obj, val);
    }

    void SetShapePath(Light2D light, Vector3[] path)
    {
        SetFieldValue<Vector3[]>(light, "m_ShapePath", path);
    }

    void SetFalloffIntensity(Light2D light, float falloffInt)
    {
        SetFieldValue<float>(light, "m_FalloffIntensity", falloffInt);
    }

    void SetFalloffSize(Light2D light, float falloffSize)
    {
        SetFieldValue<float>(light, "m_ShapeLightFalloffSize", falloffSize);
    }

    void SetAlphaBlendOnOverlap(Light2D light, bool alphaBlendOnOverlap)
    {
        SetFieldValue<bool>(light, "m_AlphaBlendOnOverlap", alphaBlendOnOverlap);
    }
    ////--------------------------------////

    //public void UpdateLightPath(Vector3[] newLightPath)
    //{
    //    lightPath = newLightPath;
    //    SetShapePath(beam, lightPath);
    //}
}
