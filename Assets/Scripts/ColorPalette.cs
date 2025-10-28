using UnityEngine;

// [System.Serializable] allows us to see and edit this in the Unity Inspector.
[System.Serializable]
public class ColorPalette
{
    [Tooltip("The color gradient for the stackable blocks.")]
    public Gradient blockGradient;

    [Tooltip("The color gradient for the camera's background.")]
    public Gradient backgroundGradient;
}