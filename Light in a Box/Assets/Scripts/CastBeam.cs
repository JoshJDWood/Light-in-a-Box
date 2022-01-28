using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastBeam : MonoBehaviour
{
    [SerializeField] Material material;
    LightBeam beam;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 360; i++)
        {
            transform.Rotate(Vector3.forward, 1);
            beam = new LightBeam(gameObject.transform.position, gameObject.transform.up, material);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //***// code to rotate a single beam //***//
        //if(Input.GetKey(KeyCode.LeftArrow))
        //{
        //    Destroy(GameObject.Find("Light Beam"));
        //    transform.Rotate(Vector3.forward, Time.deltaTime * -20);
        //    beam = new LightBeam(gameObject.transform.position, -gameObject.transform.right, material);
        //}
        //else if (Input.GetKey(KeyCode.RightArrow))
        //{
        //    Destroy(GameObject.Find("Light Beam"));
        //    transform.Rotate(Vector3.forward, Time.deltaTime * 20);
        //    beam = new LightBeam(gameObject.transform.position, -gameObject.transform.right, material);
        //}
    }
}
