using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class WaveVolumeComponent : VolumeComponent
{
    public FloatParameter Wave = new FloatParameter(0f);
    public FloatParameter Intensity = new FloatParameter(0f);
    public FloatParameter Speed = new FloatParameter(0f);
    public Vector2Parameter Offset = new Vector2Parameter(Vector2.zero);
    public IntParameter NeedsEffect = new IntParameter(0);
}
