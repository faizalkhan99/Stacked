using UnityEngine;
using System; // Required for Action

public class InputManager : MonoBehaviour
{
    // C# Event to notify other scripts when a tap occurs.
    public static event Action OnPlayerTap;

    void Update()
    {
        // Detect tap input.
        if (Input.GetMouseButtonDown(0))
        {
            OnPlayerTap?.Invoke();
        }
    }
}