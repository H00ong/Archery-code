using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ParticleColorChange : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#if UNITY_EDITOR
    [ContextMenu("Particle Color/Red")]
    private void SetParticleColorRed() => SetParticleStartColor(Color.red, "Set particle color to red");

    [ContextMenu("Particle Color/Orange")]
    private void SetParticleColorOrange() => SetParticleStartColor(new Color(1f, 0.5f, 0f), "Set particle color to orange");

    [ContextMenu("Particle Color/Green")]
    private void SetParticleColorGreen() => SetParticleStartColor(new Color(0.22f, 0.79f, 0.42f), "Set particle color to green");

    [ContextMenu("Particle Color/Sky")]
    private void SetParticleColorSky() => SetParticleStartColor(new Color(0.4f, 0.75f, 1f), "Set particle color to sky");

    [ContextMenu("Particle Color/Purple")]
    private void SetParticleColorPurple() => SetParticleStartColor(new Color(0.65f, 0.2f, 1f), "Set particle color to purple");

    private void SetParticleStartColor(Color color, string undoName)
    {
        var particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        Undo.RecordObjects(particleSystems, undoName);

        foreach (var particleSystem in particleSystems)
        {
            var main = particleSystem.main;
            main.startColor = color;
            EditorUtility.SetDirty(particleSystem);
        }
    }
#endif
}
