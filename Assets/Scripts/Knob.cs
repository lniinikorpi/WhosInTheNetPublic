using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knob : MonoBehaviour
{
    [SerializeField]
    private bool adjustX;
    [SerializeField]
    private bool adjustY;

    private float highPitch = 1.25f;
    private float lowPitch = 0.75f;

    private float adjustment = 0;
    public void AdjustValue(float value)
    {
        GameManager gm = GameManager.instance;

        if (adjustX)
        {
            float oldValue = gm.x;
            gm.x = TranslateValue(value);
            adjustment += gm.x - oldValue;
            PlayAudio(gm.x);
        }

        if (adjustY)
        {
            float oldValue = gm.y;
            gm.y = TranslateValue(value);
            adjustment += gm.y - oldValue;
            PlayAudio(gm.y);
        }
    }

    private void PlayAudio(float currentValue)
    {
        GameManager gm = GameManager.instance;
        if (Mathf.Abs(adjustment) > (gm.maxValue - gm.minValue) / 20)
        {
            adjustment = 0;
            AudioSource audio = gameObject.GetComponent<AudioSource>();
            audio.clip = gm.knobSound;
            if (currentValue >= 0)
            {
                audio.pitch = 1.25f - ((gm.maxValue - currentValue) / gm.maxValue) * 0.25f;
            }
            else
            {
                audio.pitch = 0.75f + ((gm.minValue - currentValue) / gm.minValue) * 0.25f;
            }
            audio.Play();
        }
    }

    private float TranslateValue(float zRotation)
    {
        GameManager gm = GameManager.instance;
        float v;
        if (zRotation >= 320)
        {
            v = (zRotation - 320) / 260;
        }
        else
        {
            v = (zRotation + 40) / 260;
        }
        return v * (gm.maxValue - gm.minValue) - (gm.maxValue - gm.minValue)/2;
    }
}
