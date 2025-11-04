using UnityEngine;
using System;
using UnityEngine.EventSystems;

// This attribute makes the script run in the editor, allowing OnGUI to draw
[ExecuteInEditMode] 
public class InputManager : MonoBehaviour
{
    public static event Action OnPlayerTap;

    [Header("Tap Zone Settings")]
    [Tooltip("The percentage of the screen (from the bottom) that counts as the gameplay tap area. 0.85 = bottom 85%.")]
    [Range(0.1f, 1f)]
    public float gameplayAreaHeight = 0.85f;

    // We no longer need the complex EventSystem checks from before
    
    void Update()
    {
        // Only run game logic if the game is actually playing
        if (!Application.isPlaying) return;

        // Check for the tap
        if (Input.GetMouseButtonDown(0))
        {
            // 1. Get the tap's Y position. (0 is at the bottom)
            float tapY = Input.mousePosition.y;

            // 2. Calculate the pixel height of the gameplay zone
            float gameplayZoneTopY = Screen.height * gameplayAreaHeight;

            // 3. Check if the tap is BELOW that line
            if (tapY < gameplayZoneTopY)
            {
                // Yes, tap is in the gameplay area. Fire the event.
                OnPlayerTap?.Invoke();
            }
            else
            {
                // No, tap is in the UI area (top 15%).
                // We do nothing. The EventSystem will handle the Pause Button.
            }
        }
    }

    // Special Unity method draws to the screen's GUI
    void OnGUI()
    {
        // Only draw this visual helper if we are in the Editor AND not playing
        if (Application.isEditor && !Application.isPlaying)
        {
            // Calculate the Y coordinate of the dividing line
            float gameplayZoneTopY = Screen.height * gameplayAreaHeight;
            // Calculate the height of the "safe" UI zone at the top
            float uiZoneHeight = Screen.height - gameplayZoneTopY;

            // Create a Rect for the UI zone
            Rect uiZoneRect = new Rect(0, 0, Screen.width, uiZoneHeight);

            // Create a semi-transparent red color for the box
            Color boxColor = new Color(1.0f, 0.0f, 0.0f, 0.3f); 
            
            // Draw the semi-transparent box
            GUI.color = boxColor;
            GUI.Box(uiZoneRect, GUIContent.none);

            // Draw a label in the middle of the box
            GUI.color = Color.red; // Reset color for text
            GUIStyle labelStyle = new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 30
            };
            GUI.Label(uiZoneRect, "UI ZONE. Gameplay touches will be ignored!", labelStyle);
        }
    }
}