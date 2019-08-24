using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathEmission : MonoBehaviour
{

    [SerializeField]
    private float duration;

    private Color baseColour = new Color(0.5943396f, 0.5005056f, 0.3840779f);

    private Material material;
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        float phi = (Time.time / duration) * 2 * Mathf.PI;
        float amplitude = Mathf.Cos(phi) * 0.5f + 0.5f;

        Color newColor = baseColour * Mathf.LinearToGammaSpace(amplitude);
        material.SetColor("_EmissionColor", newColor);
    }
}
