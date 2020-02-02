using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalRenderer : MonoBehaviour
{
    public int points = 2000;
    public int endPoints = 500;
    public float maxScale = 0.7f;
    public float amplitude = 1.0f;

    public float brightness = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        LineRenderer rend = gameObject.GetComponent<LineRenderer>();
        rend.positionCount = points;

        Material material = rend.material;
        material.SetFloat("_Mode", 4f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.paused)
        {
            Vector3[] vertices = new Vector3[points];
            float amp = Mathf.Clamp(GameManager.instance.signalAmplitude, 0.03f, 1.0f);
            float ampScale = maxScale * amp;
            for (int i = 0; i < points; i++)
            {
                float scale;
                if (i < endPoints || i > points - endPoints)
                {
                    float s;
                    if (i < endPoints)
                    {
                        s = i / (float)endPoints;
                    }
                    else
                    {
                        s = (points - i) / (float)endPoints;
                    }
                    scale = Random.Range(-ampScale * s, ampScale * s);
                }
                else
                {
                    scale = Random.Range(-ampScale, ampScale);
                }
                vertices[i] = transform.position + new Vector3(
                    (transform.localScale.x / (float)points) * i - transform.localScale.x / 2,
                    scale,
                    transform.localScale.y * -1.2f
                );
            }

            LineRenderer rend = gameObject.GetComponent<LineRenderer>();
            rend.positionCount = points;
            rend.SetPositions(vertices);

            Color col = rend.material.GetColor("_Color");
            col.a = brightness;
            rend.material.SetColor("_Color", new Color(col.r, col.g, col.b, col.a));

            AudioSource audio = gameObject.GetComponent<AudioSource>();
            audio.volume = Mathf.Clamp(amp, 0.3f, 1.0f);
        }
    }
}
