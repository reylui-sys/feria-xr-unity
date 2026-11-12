using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Si se usa URP
// using UnityEngine.Rendering.HighDefinition; // Si se usa HDRP
// using UnityEngine.Rendering.PostProcessing; // Si se usa el antiguo Post-Processing

public class BrightnessController : MonoBehaviour
{
    [Header("UI y Post-Processing")]
    public Slider brightnessSlider;
    public Volume globalVolume;

    private ColorAdjustments colorAdjustments;

    void Start()
    {
        // Intenta obtener el efecto de Color Adjustments del volumen
        if (globalVolume.profile.TryGet(out colorAdjustments))
        {
            // Asigna el valor inicial del slider para que coincida con el volumen
            brightnessSlider.value = colorAdjustments.postExposure.value;

            // Escucha los cambios que el jugador haga en el slider. En lugar de hacerlo en el OnValueChanged del inspector.
            brightnessSlider.onValueChanged.AddListener(ChangeBrightness);
        }
        else
        {
            Debug.LogWarning("No se encontró 'Color Adjustments' en el Global Volume.");
        }
    }

    // Esta función se ejecuta cada vez que se mueve el slider
    public void ChangeBrightness(float value)
    {
        if (colorAdjustments != null)
        {
            // Cambia la exposición de la pantalla
            colorAdjustments.postExposure.value = value;
        }
    }
}