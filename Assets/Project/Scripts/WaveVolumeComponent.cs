using System;
using UnityEngine.Rendering;

[Serializable]
public class WaveVolumeComponent : VolumeComponent
{
    public FloatParameter Wave = new FloatParameter(0.0f);
    public FloatParameter Intensity = new FloatParameter(0.0f);
}
