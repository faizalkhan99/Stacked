// using System;
// using UnityEngine;
// public enum GameState { Starting, Playing, Paused, GameOver }
// public class GameManager : MonoBehaviour
// {
//     private static GameManager instance;
//     public static GameManager Instance
//     {
//         get
//         {
//             if (instance == null) Debug.LogError("GameManager instance is null!");
//             return instance;
//         }
//     }


//     [Header("Game Objects")]
//     public Transform baseBlockPlatform;
//     public GameObject blockPrefab;
//     public GameObject noCutBlockPrefab;

//     public CameraFollow mainCamera;
//     public Transform cranePivot;

//     [Header("Visuals")]
//     [Tooltip("Drag the 'cranePivot' GameObject here (which has the CraneVisuals script).")]
//     public CraneVisuals craneVisuals;

//     [Header("Managers")]
//     public UIManager uiManager;
//     public ColorManager colorManager;

//     [Header("Game Mechanics")]
//     public SlicingMechanic slicingMechanic = SlicingMechanic.CutBlock;
//     [Range(0, 1)]
//     public float perfectPlacementThreshold;

//     [Header("NoCut Wobble Settings")]
//     [Tooltip("Drag your 'TowerContainer' parent object here.")]
//     public Transform towerContainer; // Make sure this is assigned!
//     [Tooltip("The max offset before the tower collapses.")]
//     public float maxWobble;
//     [Tooltip("How much the tower visually rocks.")]
//     public float wobbleVisualFactor;
//     [Tooltip("How fast the tower wobbles back and forth.")]
//     public float _wobbleSpeed;

//     [Header("Swing Control")]
//     public SwingDirection swingDirection = SwingDirection.Positive_X_Negative_Z;

//     [Header("Game Settings")]
//     public float minSpeed;
//     public float maxSpeed;
//     public int scoreToReachMaxSpeed;
//     public float gameOverTimeout;

//     [Header("Swing Settings")]
//     public float swingDistance;
//     public float ropeLength;

//     [Header("Debris Settings")]
//     public float explosionForce;
//     [Header("Effects")]
//     public ParticleSystem backgroundSparkles;

//     // Private variables
//     private BlockCutter _blockCutter;
//     private Transform _lastPlacedBlock;
//     private Transform _currentSwingingBlock;
//     private Transform _currentFallingBlock;
//     private int _score = 0;
//     private float _wobbleX = 0f;
//     private float _wobbleZ = 0f;
//     // --- NEW GAME STATE ---
//     private GameState currentState = GameState.Starting;
//     // ---

//     // Subscribe to InputManager event when enabled
//     void OnEnable()
//     {
//         InputManager.OnPlayerTap += HandlePlayerTap;
//     }

//     // Unsubscribe when disabled
//     void OnDisable()
//     {
//         InputManager.OnPlayerTap -= HandlePlayerTap;
//     }
//     void Awake()
//     {
//         if (instance != null && instance != this) { Destroy(gameObject); return; }
//         instance = this;
//         _blockCutter = gameObject.AddComponent<BlockCutter>();
//     }

//     void Start()
//     {
//         if (uiManager == null) { Debug.LogError("UIManager is not assigned in the Inspector!"); return; }
//         if (blockPrefab == null) Debug.LogError("CutBlock mode 'blockPrefab' is not assigned!");
//         if (noCutBlockPrefab == null) Debug.LogError("NoCut mode 'noCutBlockPrefab' is not assigned!");
//         if (baseBlockPlatform == null) { Debug.LogError("Base Block Platform is not assigned!"); return; }
//         if (craneVisuals == null) { Debug.LogError("Crane Visuals is not assigned!"); return; }
//         if (towerContainer == null && slicingMechanic == SlicingMechanic.NoCut) { Debug.LogError("Tower Container is not assigned (needed for NoCut wobble)!"); }
//         if (backgroundSparkles != null) backgroundSparkles.Play();
//         baseBlockPlatform.tag = "TopBlock";
//         _lastPlacedBlock = baseBlockPlatform;
//         mainCamera.SetTarget(_lastPlacedBlock);

//         if (colorManager != null)
//         {
//             colorManager.InitializeColors(); // Color initialization
//         }
//         else Debug.LogWarning("ColorManager is not assigned.");

//         Vector3 moveAxis = GetVectorFromDirection(swingDirection);
//         craneVisuals.Initialize(minSpeed, swingDistance, ropeLength, moveAxis);

//         SpawnNewBlock();

//         // --- SET INITIAL STATE ---
//         currentState = GameState.Playing;
//         HandleTimeScale(1);
//         Debug.Log("Game State: Playing");
//         // ---
//     }

//     public void HandleTimeScale(int val)
//     {
//         Time.timeScale = val;
//     }

//     // Update is now only for wobble visuals (and potentially pause checks later)
//     void Update()
//     {
//         // Only update wobble visuals if playing
//         if (currentState == GameState.Playing)
//         {
//             UpdateWobbleVisuals();
//         }
//     }

//     // --- NEW: Handles input from InputManager ---
//     private void HandlePlayerTap()
//     {
//         // Only allow dropping if the game is in the Playing state
//         if (currentState == GameState.Playing && _currentSwingingBlock != null)
//         {
//             DropBlock();
//         }
//         else if (currentState == GameState.GameOver)
//         {
//             // Optional: Allow tap to restart game after game over
//             // RestartGame();
//         }
//     }
//     // ---

//     private void SpawnNewBlock()
//     {
//         // --- Debug Start ---
//         Debug.Log($"--- Spawning Block --- Score: {_score}, Mode: {slicingMechanic.ToString()} ---");

//         // --- Essential Checks ---
//         if (cranePivot == null) { Debug.LogError("Crane Pivot not assigned!"); return; }
//         if (craneVisuals == null || craneVisuals.clawTransform == null) { Debug.LogError("CraneVisuals or its ClawTransform is not assigned!"); return; } // Added check for claw

//         // --- Choose Prefab Based On Mode ---
//         GameObject prefabToInstantiate = null;
//         if (slicingMechanic == SlicingMechanic.CutBlock)
//         {
//             prefabToInstantiate = blockPrefab;
//             if (prefabToInstantiate == null) { Debug.LogError("CutBlock mode 'blockPrefab' is not assigned!"); return; }
//             Debug.Log($"Selected CutBlock prefab ('{prefabToInstantiate.name}') for instantiation. Prefab scale: {prefabToInstantiate.transform.localScale}");
//         }
//         else
//         {
//             prefabToInstantiate = noCutBlockPrefab;
//             if (prefabToInstantiate == null) { Debug.LogError("NoCut mode 'noCutBlockPrefab' is not assigned!"); return; }
//             Debug.Log($"Selected NoCutBlock prefab ('{prefabToInstantiate.name}') for instantiation. Prefab scale: {prefabToInstantiate.transform.localScale}");
//         }

//         // --- Instantiate (at Origin initially) ---
//         GameObject newBlockObj = Instantiate(prefabToInstantiate);
//         Debug.Log($"Instantiated block '{newBlockObj.name}'. Initial scale: {newBlockObj.transform.localScale}");

//         // --- Apply Color ---
//         if (colorManager != null)
//         {
//             Renderer blockRenderer = newBlockObj.GetComponentInChildren<Renderer>();
//             if (blockRenderer != null)
//             {
//                 Color newColor = colorManager.GetCurrentBlockColor(_score);
//                 blockRenderer.material.color = newColor;
//             }
//             else
//             {
//                 Debug.LogWarning($"SpawnNewBlock: Could not find Renderer in children of '{newBlockObj.name}'!");
//             }
//         }

//         // --- Determine and Set Final Scale ---
//         Vector3 targetScale = Vector3.one; // Default
//         if (_score == 0)
//         {
//             // First block always uses its OWN prefab's scale
//             targetScale = prefabToInstantiate.transform.localScale;
//             Debug.Log($"Score 0: Setting scale based on instantiated prefab's scale: {targetScale}");
//         }
//         else if (slicingMechanic == SlicingMechanic.CutBlock)
//         {
//             // Subsequent blocks in Cut mode get scale from the last placed block
//             targetScale = _lastPlacedBlock.transform.localScale;
//             Debug.Log($"Cut Mode (Score > 0): Setting scale based on _lastPlacedBlock's scale: {targetScale}");
//         }
//         else
//         { // NoCut mode and score > 0
//             // Subsequent blocks in NoCut mode use the NoCut PREFAB's scale
//             // *** Force using the specific NoCut prefab reference here ***
//             if (noCutBlockPrefab != null)
//             {
//                 targetScale = noCutBlockPrefab.transform.localScale; // Explicitly use noCutBlockPrefab scale
//                 Debug.Log($"NoCut Mode (Score > 0): Explicitly setting scale based on noCutBlockPrefab asset's scale: {targetScale}");
//             }
//             else
//             {
//                 Debug.LogError("noCutBlockPrefab reference is missing when trying to set scale!");
//                 targetScale = Vector3.one; // Fallback scale
//             }
//         }
//         newBlockObj.transform.localScale = targetScale;
//         Debug.Log($"Final scale set for '{newBlockObj.name}': {newBlockObj.transform.localScale}");

//         // --- FIX: Set Initial Position BEFORE Initializing Follower ---
//         // Calculate where the block should be based on the claw's CURRENT position
//         Vector3 initialBlockPosition = craneVisuals.clawTransform.position - (Vector3.up * craneVisuals.blockAttachOffset);
//         newBlockObj.transform.position = initialBlockPosition;
//         Debug.Log($"Set initial position for '{newBlockObj.name}' to: {initialBlockPosition}");
//         // --- End Fix ---

//         // --- Initialize Follower ---
//         BlockFollowClaw follower = newBlockObj.GetComponent<BlockFollowClaw>();
//         if (follower == null) { follower = newBlockObj.AddComponent<BlockFollowClaw>(); Debug.LogWarning($"Added BlockFollowClaw to {newBlockObj.name} at runtime. Add it to the prefab!"); }
//         // Initialize AFTER setting the position
//         follower.Initialize(craneVisuals.clawTransform, craneVisuals.blockAttachOffset);

//         _currentSwingingBlock = newBlockObj.transform;
//         Debug.Log("--- SpawnNewBlock Finished ---");
//     }
//     private void DropBlock()
//     {
//         _currentFallingBlock = _currentSwingingBlock;
//         _currentFallingBlock.GetComponent<BlockFollowClaw>().Release();

//         DroppedBlockDetector detector = _currentFallingBlock.gameObject.AddComponent<DroppedBlockDetector>();
//         detector.gameManager = this;
//         _currentSwingingBlock = null;
//         Invoke(nameof(CheckForMiss), gameOverTimeout);
//     }

//     public void ProcessLandedBlock(Transform landedBlock)
//     {
//         CancelInvoke(nameof(CheckForMiss));
//         Transform previousBlock = _lastPlacedBlock;
//         _currentFallingBlock = null;

//         if (previousBlock != null)
//         {
//             previousBlock.tag = "Block";
//         }

//         // --- THIS IS THE FIX ---
//         Quaternion originalWobbleRotation = Quaternion.identity;
//         if (slicingMechanic == SlicingMechanic.NoCut && towerContainer != null)
//         {
//             // 1. Store the current visual wobble rotation.
//             originalWobbleRotation = towerContainer.rotation;
//             // 2. Temporarily reset the tower to be perfectly upright for the check.
//             towerContainer.rotation = Quaternion.identity;
//         }
//         // --- END FIX ---

//         // Now, all position calculations below will use the stable, un-wobbled positions.
//         float diffX = Mathf.Abs(landedBlock.position.x - previousBlock.position.x);
//         float diffZ = Mathf.Abs(landedBlock.position.z - previousBlock.position.z);
//         float graceX = (previousBlock.localScale.x / 2) * perfectPlacementThreshold;
//         float graceZ = (previousBlock.localScale.z / 2) * perfectPlacementThreshold;
//         float newY = previousBlock.position.y + (previousBlock.localScale.y / 2f) + (landedBlock.localScale.y / 2f);

//         // Perfect Placement Check (Unaffected by wobble now)
//         if (_score > 0 && diffX <= graceX && diffZ <= graceZ)
//         {
//             Debug.Log("PERFECT PLACEMENT!");
//             landedBlock.position = new Vector3(previousBlock.position.x, newY, previousBlock.position.z);
//             landedBlock.rotation = Quaternion.identity;
//             if (slicingMechanic == SlicingMechanic.CutBlock)
//             {
//                 landedBlock.localScale = previousBlock.localScale;
//             }
//             HandleSuccessfulPlacement(landedBlock, previousBlock);
//             // Restore wobble immediately after handling placement
//             if (slicingMechanic == SlicingMechanic.NoCut && towerContainer != null) towerContainer.rotation = originalWobbleRotation;
//             return;
//         }

//         // Regular Placement Logic (Unaffected by wobble now)
//         if (_score == 0)
//         {
//             landedBlock.position = new Vector3(landedBlock.position.x, newY, landedBlock.position.z);
//             RunNoCutLogic(landedBlock, previousBlock);
//         }
//         else
//         {
//             if (slicingMechanic == SlicingMechanic.CutBlock)
//             {
//                 RunCutLogic(landedBlock, previousBlock);
//             }
//             else
//             {
//                 landedBlock.position = new Vector3(landedBlock.position.x, newY, landedBlock.position.z);
//                 RunNoCutLogic(landedBlock, previousBlock);
//             }
//         }

//         // --- RESTORE WOBBLE ---
//         // Restore the visual wobble AFTER the placement logic is done.
//         // UpdateWobbleVisuals() in the next frame will take over smoothly.
//         if (slicingMechanic == SlicingMechanic.NoCut && towerContainer != null)
//         {
//             towerContainer.rotation = originalWobbleRotation;
//         }
//         // ---
//     }
//     private void RunCutLogic(Transform landedBlock, Transform previousBlock)
//     {
//         bool cutSuccess = _blockCutter.Cut(landedBlock, previousBlock, explosionForce);
//         if (cutSuccess)
//         {
//             HandleSuccessfulPlacement(landedBlock, previousBlock);
//         }
//         else
//         {
//             HandleGameOver("Missed the cut!");
//         }
//     }


//     private void RunNoCutLogic(Transform landedBlock, Transform previousBlock)
//     {
//         // --- STEP 1: Assume a Hit ---
//         // Since ProcessLandedBlock called this function, we know DroppedBlockDetector
//         // detected a collision with the 'TopBlock'. We no longer need CheckMissNoCut here,
//         // making the logic robust against pivot point or collider/scale mismatches.

//         // --- STEP 2: Calculate Offset & Add to Wobble Score ---
//         // Calculate the difference based on the final landed positions.
//         // ProcessLandedBlock should have already corrected the Y position before calling this.
//         float diffX = landedBlock.position.x - previousBlock.position.x;
//         float diffZ = landedBlock.position.z - previousBlock.position.z;
//         _wobbleX += diffX;
//         _wobbleZ += diffZ;
//         // Optional: Add a debug log to monitor wobble values
//         // Debug.Log($"Wobble Update: Added (X:{diffX:F2}, Z:{diffZ:F2}). Current Wobble (X:{_wobbleX:F2}, Z:{_wobbleZ:F2})");

//         // --- STEP 3: Check for Collapse Condition ---
//         // Compare the accumulated wobble against the maximum allowed limit.
//         if (Mathf.Abs(_wobbleX) > maxWobble || Mathf.Abs(_wobbleZ) > maxWobble)
//         {
//             Debug.LogWarning($"Tower Collapse Triggered! Wobble (X:{_wobbleX:F2}, Z:{_wobbleZ:F2}) exceeded Max Wobble ({maxWobble})");
//             Destroy(landedBlock.gameObject); // Destroy the block that caused the collapse
//             HandleGameOver("Tower collapsed!"); // Trigger game over sequence
//             return; // Stop further processing
//         }

//         // --- STEP 4: Successful (Non-Collapsed) Placement ---
//         // Ensure the block is perfectly flat, regardless of wobble animation.
//         landedBlock.rotation = Quaternion.identity;

//         // Proceed with the standard successful placement steps.
//         HandleSuccessfulPlacement(landedBlock, previousBlock);
//     }


//     private void HandleSuccessfulPlacement(Transform newPlacedBlock, Transform previousBlock)
//     {
//         newPlacedBlock.tag = "TopBlock";

//         // Parent the new block to the tower so it wobbles
//         if (towerContainer != null)
//             newPlacedBlock.SetParent(towerContainer);

//         _score++;
//         _lastPlacedBlock = newPlacedBlock;

//         if (cranePivot != null && previousBlock != null)
//         {
//             float heightGained = newPlacedBlock.localScale.y;
//             cranePivot.position += new Vector3(0, heightGained, 0);
//         }

//         mainCamera.SetTarget(newPlacedBlock);
//         SpawnNewBlock();
//     }

//     private bool CheckMissNoCut(Transform fallingBlock, Transform stationaryBlock)
//     {
//         float diffX = Mathf.Abs(fallingBlock.position.x - stationaryBlock.position.x);
//         float diffZ = Mathf.Abs(fallingBlock.position.z - stationaryBlock.position.z);
//         float maxOverlapDistX = (fallingBlock.localScale.x / 2f) + (stationaryBlock.localScale.x / 2f);
//         float maxOverlapDistZ = (fallingBlock.localScale.z / 2f) + (stationaryBlock.localScale.z / 2f);

//         if (diffX >= maxOverlapDistX || diffZ >= maxOverlapDistZ)
//         {
//             return true;
//         }

//         return false;
//     }

//     public void HandleMissedPlacement(Transform missedBlock)
//     {
//         CancelInvoke(nameof(CheckForMiss));
//         _currentFallingBlock = null;
//         HandleGameOver("Hit an old block!");
//         Destroy(missedBlock.gameObject, 3f);
//     }

//     private void CheckForMiss()
//     {
//         if (_currentFallingBlock != null)
//         {
//             Destroy(_currentFallingBlock.gameObject);
//             HandleGameOver("Dropped into the void!");
//         }
//     }

//     private void UpdateWobbleVisuals()
//     {
//         if (slicingMechanic != SlicingMechanic.NoCut || towerContainer == null)
//         {
//             if (towerContainer != null)
//                 towerContainer.rotation = Quaternion.identity;
//             return;
//         }

//         // Change from a Sin wave to an Abs(Sin) "bounce" wave.
//         float wobbleAmountX = Mathf.Abs(Mathf.Sin(Time.time * _wobbleSpeed)) * _wobbleX * wobbleVisualFactor;
//         float wobbleAmountZ = Mathf.Abs(Mathf.Sin(Time.time * _wobbleSpeed)) * _wobbleZ * wobbleVisualFactor;
//         // ---

//         // Use Slerp for a smooth rotation
//         Quaternion targetRotation = Quaternion.Euler(wobbleAmountZ, 0, -wobbleAmountX);
//         towerContainer.rotation = Quaternion.Slerp(towerContainer.rotation, targetRotation, Time.deltaTime * 5f);
//     }

//     private void HandleGameOver(string reason)
//     {
//         Debug.Log($"Game Over! {reason} Final Score: {_score}");
//         _currentFallingBlock = null;
//         _currentSwingingBlock = null;

//         if (craneVisuals != null)
//             craneVisuals.Hide();
//     }

//     public void StartGame()
//     {
//         if (currentState == GameState.Playing) return; // Prevent restarting if already playing

//         currentState = GameState.Playing;
//         Time.timeScale = 1f;
//         _score = 0;
//         _wobbleX = 0f;
//         _wobbleZ = 0f;

//         // Reset Tower visuals/position
//         if(towerContainer) {
//             towerContainer.rotation = Quaternion.identity;
//             // Destroy existing stacked blocks
//              foreach (Transform child in towerContainer) {
//                  if(child != baseBlockPlatform) Destroy(child.gameObject); // Keep base platform if it's a child
//              }
//         }
//         // Ensure base platform is correctly set as the first target
//         if (baseBlockPlatform) {
//             // If base platform is NOT a child of TowerContainer, ensure it's reset
//              baseBlockPlatform.tag = "TopBlock";
//              _lastPlacedBlock = baseBlockPlatform;
//              mainCamera.SetTarget(_lastPlacedBlock);
//         } else {
//             Debug.LogError("StartGame: BaseBlockPlatform reference is missing!");
//             return;
//         }


//         // Reset crane position relative to base platform height
//          if (cranePivot != null) {
//               cranePivot.position = new Vector3(
//                  baseBlockPlatform.position.x, // Align horizontally with base
//                  baseBlockPlatform.position.y + baseBlockPlatform.localScale.y/2f + ropeLength, // Position above base
//                  baseBlockPlatform.position.z); // Align horizontally with base
//          }


//         // Initialize colors
//         if (colorManager != null) colorManager.InitializeColors();

//         // Start crane swing
//         if (craneVisuals != null) {
//             Vector3 moveAxis = GetVectorFromDirection(swingDirection);
//             craneVisuals.Initialize(minSpeed, swingDistance, ropeLength, moveAxis);
//         }

//         SpawnNewBlock(); // Spawn the first block
//         if(uiManager) uiManager.UpdateScoreText(_score); // Set UI score to 0

//         Debug.Log("Game State: Playing (Started/Restarted)");
//     }

//     private void SetGameOver(string reason)
//     {
//         // Prevent setting game over multiple times
//         if (currentState == GameState.GameOver) return;
//         currentState = GameState.GameOver;
//         Debug.Log($"Game Over! {reason} Final Score: {_score}");
//         _currentFallingBlock = null; // Ensure no lingering checks
//         _currentSwingingBlock = null; // Stop further input processing

//         if (craneVisuals != null)
//             craneVisuals.Hide(); // Stop crane visuals

//         // Here you would trigger UI, effects, etc.
//         if (backgroundSparkles != null) backgroundSparkles.Stop();
//         // Example: Invoke("ShowGameOverScreen", 2f);
//     }

//     public void TogglePause()
//     {
//         if (currentState == GameState.GameOver) return; // Can't pause if already over

//         if (currentState == GameState.Paused)
//         {
//             ResumeGame();
//         }
//         else if (currentState == GameState.Playing)
//         {
//             PauseGame();
//         }
//     }

//     public void PauseGame()
//     {
//         currentState = GameState.Paused;
//         Time.timeScale = 0f; // Freeze game time
//         Debug.Log("Game State: Paused");
//         // Show Pause UI here
//     }

//     public void ResumeGame()
//     {
//         currentState = GameState.Playing;
//         Time.timeScale = 1f; // Resume game time
//         Debug.Log("Game State: Playing (Resumed)");
//         // Hide Pause UI here
//     }

//     public void RestartGame() // Example restart function
//     {
//         Time.timeScale = 1f; // Ensure time scale is reset
//         UnityEngine.SceneManagement.SceneManager.LoadScene(
//             UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
//         );
//     }
//     // ---
//     private Vector3 GetVectorFromDirection(SwingDirection direction)
//     {
//         switch (direction)
//         {
//             case SwingDirection.Positive_X: return Vector3.right;
//             case SwingDirection.Negative_X: return Vector3.left;
//             case SwingDirection.Positive_Z: return Vector3.forward;
//             case SwingDirection.Negative_Z: return Vector3.back;
//             case SwingDirection.Positive_X_Positive_Z: return new Vector3(1, 0, 1).normalized;
//             case SwingDirection.Positive_X_Negative_Z: return new Vector3(1, 0, -1).normalized;
//             case SwingDirection.Negative_X_Positive_Z: return new Vector3(-1, 0, 1).normalized;
//             case SwingDirection.Negative_X_Negative_Z: return new Vector3(-1, 0, -1).normalized;
//             default: return Vector3.right;
//         }
//     }
// }




using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene reloading on restart/home

// Enum for game states
public enum GameState { Starting, Playing, Paused, GameOver }

// Enum for slicing mechanic choice (Ensure SlicingMechanic.cs exists)
// public enum SlicingMechanic { CutBlock, NoCut }

// Enum for swing direction (Ensure SwingDirection.cs exists)
// public enum SwingDirection { ... }

public class GameManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            // Find the instance if it's null (useful for button calls)
            if (instance == null) instance = FindFirstObjectByType<GameManager>();
            if (instance == null) Debug.LogError("GameManager instance could not be found!");
            return instance;
        }
    }
    // ---

    [Header("Game Objects")]
    [Tooltip("Drag your in-scene base platform here.")]
    public Transform baseBlockPlatform;
    [Tooltip("The simple cube prefab used in CutBlock mode.")]
    public GameObject blockPrefab;
    [Tooltip("The detailed model prefab used in NoCut mode.")]
    public GameObject noCutBlockPrefab;
    public CameraFollow mainCamera;
    public Transform cranePivot;

    [Header("Managers")]
    [Tooltip("Drag the 'cranePivot' GameObject here (which has the CraneVisuals script).")]
    public CraneVisuals craneVisuals;
    public ColorManager colorManager;
    [Tooltip("Drag the GameObject with the UIManager script here.")]
    public UIManager uiManager; // Assign in Inspector

    [Header("Game Mechanics")]
    public SlicingMechanic slicingMechanic = SlicingMechanic.CutBlock;
    [Range(0, 1)]
    public float perfectPlacementThreshold = 0.1f;

    [Header("NoCut Wobble Settings")]
    [Tooltip("Drag your 'TowerContainer' parent object here.")]
    public Transform towerContainer;
    [Tooltip("The max accumulated offset before the tower collapses.")]
    public float maxWobble = 2.0f;
    [Tooltip("How much the tower visually rocks.")]
    public float wobbleVisualFactor = 0.5f;
    [Tooltip("How fast the tower wobbles back and forth.")]
    public float wobbleSpeed = 5f; // Renamed from _wobbleSpeed

    [Header("Swing Control")]
    public SwingDirection swingDirection = SwingDirection.Positive_X_Negative_Z;

    [Header("Game Settings")]
    public float minSpeed = 1.5f;
    public float maxSpeed = 5f;
    public int scoreToReachMaxSpeed = 30;
    public float gameOverTimeout = 3f;

    [Header("Swing Settings")]
    public float swingDistance = 4f;
    public float ropeLength = 5f;

    [Header("Debris Settings")]
    public float explosionForce = 50f;

    [Header("Effects")]
    public ParticleSystem backgroundSparkles;

    // --- Private State Variables ---
    private BlockCutter _blockCutter;
    private Transform _lastPlacedBlock;
    private Transform _currentSwingingBlock;
    private Transform _currentFallingBlock;
    private int _score = 0;
    private float _wobbleX = 0f;
    private float _wobbleZ = 0f;
    private GameState currentState = GameState.Starting; // Start in Menu

    // --- Event Subscriptions ---
    void OnEnable() { InputManager.OnPlayerTap += HandlePlayerTap; }
    void OnDisable() { InputManager.OnPlayerTap -= HandlePlayerTap; }

    void Awake()
    {
        // Enforce Singleton
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        // Optional: DontDestroyOnLoad(gameObject); // If GameManager should persist across scenes

        _blockCutter = gameObject.AddComponent<BlockCutter>();
    }

    void Start()
    {
        // --- Essential Manager/Object Checks ---
        if (uiManager == null) { Debug.LogError("UIManager is not assigned in the Inspector!"); enabled = false; return; } // Disable script if UI missing
        if (blockPrefab == null) Debug.LogError("CutBlock mode 'blockPrefab' is not assigned!");
        if (noCutBlockPrefab == null) Debug.LogError("NoCut mode 'noCutBlockPrefab' is not assigned!");
        if (baseBlockPlatform == null) { Debug.LogError("Base Block Platform is not assigned!"); enabled = false; return; }
        if (craneVisuals == null) { Debug.LogError("Crane Visuals is not assigned!"); enabled = false; return; }
        if (towerContainer == null && slicingMechanic == SlicingMechanic.NoCut) { Debug.LogWarning("Tower Container is not assigned (needed for NoCut wobble)!"); }

        // --- Initial Setup ---
        currentState = GameState.Starting; // Ensure we start in menu state
        HandleTimeScale(1); // Ensure time is running for menus
        // UIManager's Start() should handle showing the main menu.
        // We don't initialize colors, crane, or spawn blocks until StartGame() is called.
        Debug.Log("Game State: Starting (Main Menu)");
    }

    void Update()
    {
        // Only update wobble visuals if playing
        if (currentState == GameState.Playing)
        {
            UpdateWobbleVisuals();
        }
    }

    // --- Input Handling ---
    private void HandlePlayerTap()
    {
        // Only allow dropping if the game is in the Playing state
        if (currentState == GameState.Playing && _currentSwingingBlock != null)
        {
            DropBlock();
        }
        // No restart on tap here - use UI button
    }

    // --- Core Gameplay Loop ---
    private void SpawnNewBlock()
    {
        if (currentState != GameState.Playing) return; // Don't spawn if not playing
        if (cranePivot == null) { Debug.LogError("Crane Pivot not assigned!"); return; }

        GameObject prefabToSpawn = (slicingMechanic == SlicingMechanic.CutBlock) ? blockPrefab : noCutBlockPrefab;
        if (prefabToSpawn == null) { Debug.LogError($"Prefab for {slicingMechanic} mode is not assigned!"); return; }

        GameObject newBlockObj = Instantiate(prefabToSpawn);

        // Apply Color
        if (colorManager != null)
        {
            Renderer blockRenderer = newBlockObj.GetComponentInChildren<Renderer>();
            if (blockRenderer != null)
            {
                Color newColor = colorManager.GetCurrentBlockColor(_score);
                blockRenderer.material.color = newColor;
            }
            else { Debug.LogWarning($"SpawnNewBlock: Could not find Renderer in children of '{newBlockObj.name}'!"); }
        }

        // Determine Scale
        Vector3 targetScale;
        if (_score == 0) { targetScale = prefabToSpawn.transform.localScale; }
        else if (slicingMechanic == SlicingMechanic.CutBlock) { targetScale = _lastPlacedBlock.transform.localScale; }
        else { targetScale = noCutBlockPrefab.transform.localScale; }
        newBlockObj.transform.localScale = targetScale;

        // Attach to Claw
        BlockFollowClaw follower = newBlockObj.GetComponent<BlockFollowClaw>();
        if (follower == null) { follower = newBlockObj.AddComponent<BlockFollowClaw>(); Debug.LogWarning($"Added BlockFollowClaw to {newBlockObj.name}. Add it to prefab!"); }

        if (craneVisuals != null && craneVisuals.clawTransform != null)
        {
            Vector3 initialBlockPosition = craneVisuals.clawTransform.position - (Vector3.up * craneVisuals.blockAttachOffset);
            newBlockObj.transform.position = initialBlockPosition;
            follower.Initialize(craneVisuals.clawTransform, craneVisuals.blockAttachOffset);

        }
        else
        {
            Debug.LogError("Cannot position/attach block, CraneVisuals/Claw missing!");
            Destroy(newBlockObj);
            SetGameOver("Crane setup error");
            return;
        }

        _currentSwingingBlock = newBlockObj.transform;
    }

    private void DropBlock()
    {
        if (_currentSwingingBlock == null || currentState != GameState.Playing) return;

        _currentFallingBlock = _currentSwingingBlock;
        _currentFallingBlock.GetComponent<BlockFollowClaw>().Release();

        // Add detector and start miss timer
        DroppedBlockDetector detector = _currentFallingBlock.gameObject.AddComponent<DroppedBlockDetector>();
        detector.gameManager = this; // Link detector back to this GameManager
        _currentSwingingBlock = null; // Prevent multiple drops
        Invoke(nameof(CheckForMiss), gameOverTimeout);
    }

    public void ProcessLandedBlock(Transform landedBlock)
    {
        if (currentState != GameState.Playing) return;

        CancelInvoke(nameof(CheckForMiss)); // Block landed, cancel the timeout
        Transform previousBlock = _lastPlacedBlock;
        _currentFallingBlock = null;

        if (previousBlock != null) { previousBlock.tag = "Block"; } // Demote previous top block

        // Temporarily reset wobble for fair placement checks
        Quaternion originalWobbleRotation = Quaternion.identity;
        if (slicingMechanic == SlicingMechanic.NoCut && towerContainer != null)
        {
            originalWobbleRotation = towerContainer.rotation;
            towerContainer.rotation = Quaternion.identity;
        }

        // Calculate placement details (Y position, differences, grace zone)
        float previousBlockTopY = previousBlock.position.y + (previousBlock.localScale.y / 2f);
        // Assuming NoCut Prefab pivot is CENTERED (adjust if needed)
        float landedBlockPivotToBottom = landedBlock.localScale.y / 2f;
        float correctLandedY = previousBlockTopY + landedBlockPivotToBottom;

        float diffX = Mathf.Abs(landedBlock.position.x - previousBlock.position.x);
        float diffZ = Mathf.Abs(landedBlock.position.z - previousBlock.position.z);
        float graceX = (previousBlock.localScale.x / 2) * perfectPlacementThreshold;
        float graceZ = (previousBlock.localScale.z / 2) * perfectPlacementThreshold;


        // --- Placement Logic Branching ---
        bool isPerfect = (_score > 0 && diffX <= graceX && diffZ <= graceZ);

        if (isPerfect)
        {
            Debug.Log("PERFECT PLACEMENT!");
            landedBlock.position = new Vector3(previousBlock.position.x, correctLandedY, previousBlock.position.z);
            landedBlock.rotation = Quaternion.identity;
            if (slicingMechanic == SlicingMechanic.CutBlock) { landedBlock.localScale = previousBlock.localScale; }
            // No wobble change on perfect
            HandleSuccessfulPlacement(landedBlock, previousBlock);
        }
        else if (_score == 0) // First block dropped (never perfect, never cut)
        {
            landedBlock.position = new Vector3(landedBlock.position.x, correctLandedY, landedBlock.position.z);
            RunNoCutLogic(landedBlock, previousBlock);
        }
        else // Subsequent non-perfect blocks
        {
            if (slicingMechanic == SlicingMechanic.CutBlock)
            {
                // Cutter handles Y pos
                RunCutLogic(landedBlock, previousBlock);
            }
            else // NoCut logic
            {
                landedBlock.position = new Vector3(landedBlock.position.x, correctLandedY, landedBlock.position.z);
                RunNoCutLogic(landedBlock, previousBlock);
            }
        }

        // Restore visual wobble AFTER placement logic
        if (slicingMechanic == SlicingMechanic.NoCut && towerContainer != null)
        {
            towerContainer.rotation = originalWobbleRotation;
        }
    }

    private void RunCutLogic(Transform landedBlock, Transform previousBlock)
    {
        bool cutSuccess = _blockCutter.Cut(landedBlock, previousBlock, explosionForce);
        if (cutSuccess) { HandleSuccessfulPlacement(landedBlock, previousBlock); }
        else { SetGameOver("Missed the cut!"); }
    }

    // This is the robust version, trusting physics for the hit check
    private void RunNoCutLogic(Transform landedBlock, Transform previousBlock)
    {
        // STEP 1: Assume Hit (Collision already confirmed)

        // STEP 2: Add Wobble
        float diffX = landedBlock.position.x - previousBlock.position.x;
        float diffZ = landedBlock.position.z - previousBlock.position.z;
        _wobbleX += diffX;
        _wobbleZ += diffZ;

        // STEP 3: Check Collapse
        if (Mathf.Abs(_wobbleX) > maxWobble || Mathf.Abs(_wobbleZ) > maxWobble)
        {
            Debug.LogWarning($"Tower Collapse Triggered! Wobble (X:{_wobbleX:F2}, Z:{_wobbleZ:F2}) > Max ({maxWobble})");
            Destroy(landedBlock.gameObject);
            SetGameOver("Tower collapsed!");
            return;
        }

        // STEP 4: Place Successfully
        landedBlock.rotation = Quaternion.identity;
        HandleSuccessfulPlacement(landedBlock, previousBlock);
    }

    private void HandleSuccessfulPlacement(Transform newPlacedBlock, Transform previousBlock)
    {
        newPlacedBlock.tag = "TopBlock"; // Mark as the new target

        // Parent to container only in NoCut mode for wobbling
        if (towerContainer != null && slicingMechanic == SlicingMechanic.NoCut)
            newPlacedBlock.SetParent(towerContainer);

        IncrementScore(); // Handles score and UI update

        _lastPlacedBlock = newPlacedBlock; // Update reference for next placement

        // Move crane pivot up
        if (cranePivot != null && previousBlock != null)
        {
            float heightGained = newPlacedBlock.localScale.y; // Assumes pivot is centered
            cranePivot.position += new Vector3(0, heightGained, 0);
        }

        mainCamera.SetTarget(newPlacedBlock); // Update camera target
        SpawnNewBlock(); // Spawn the next block immediately
    }

    public void IncrementScore()
    {
        _score++;
        if (uiManager) uiManager.UpdateScoreText(_score);
        // Optional: Play sound every 10 points etc.
    }

    public void HandleMissedPlacement(Transform missedBlock) // Hit old block
    {
        if (currentState == GameState.GameOver) return;
        CancelInvoke(nameof(CheckForMiss));
        _currentFallingBlock = null;
        SetGameOver("Hit an old block!");
        Destroy(missedBlock.gameObject, 3f);
    }

    private void CheckForMiss() // Timeout - fell into void
    {
        if (currentState == GameState.GameOver) return;
        if (_currentFallingBlock != null)
        {
            Destroy(_currentFallingBlock.gameObject);
            SetGameOver("Dropped into the void!");
        }
    }

    private void UpdateWobbleVisuals()
    {
        // Only run wobble if Playing and in NoCut mode
        if (currentState != GameState.Playing || slicingMechanic != SlicingMechanic.NoCut || towerContainer == null)
        {
            if (towerContainer != null) towerContainer.rotation = Quaternion.identity;
            return;
        }

        // "Bounce" wobble effect
        float wobbleAmountX = Mathf.Abs(Mathf.Sin(Time.time * wobbleSpeed)) * _wobbleX * wobbleVisualFactor;
        float wobbleAmountZ = Mathf.Abs(Mathf.Sin(Time.time * wobbleSpeed)) * _wobbleZ * wobbleVisualFactor;

        Quaternion targetRotation = Quaternion.Euler(wobbleAmountZ, 0, -wobbleAmountX);
        float smoothingSpeed = 3f; // Can make public if needed
        towerContainer.rotation = Quaternion.Slerp(towerContainer.rotation, targetRotation, Time.deltaTime * smoothingSpeed);
    }


    // --- GAME STATE METHODS CALLED BY UIMANAGER ---

    public void StartGame() // Called by Play Button
    {
        if (currentState == GameState.Playing) return;

        Debug.Log("GameManager: StartGame() called.");
        currentState = GameState.Playing;
        HandleTimeScale(1);
        _score = 0;
        _wobbleX = 0f;
        _wobbleZ = 0f;

        // Reset Tower
        if (towerContainer)
        {
            towerContainer.rotation = Quaternion.identity;
            // Destroy existing stacked blocks, but KEEP the base platform
            foreach (Transform child in towerContainer)
            {
                // Check if child is not the base platform before destroying
                if (child != baseBlockPlatform) Destroy(child.gameObject);
            }
        }
        // Ensure base platform is reset correctly (tag, position if needed)
        if (baseBlockPlatform)
        {
            baseBlockPlatform.tag = "TopBlock";
            _lastPlacedBlock = baseBlockPlatform;
            mainCamera.SetTarget(_lastPlacedBlock);
            // Ensure base is NOT parented to tower container if it wobbles
            if (towerContainer && baseBlockPlatform.parent == towerContainer)
            {
                baseBlockPlatform.SetParent(null); // Unparent base from wobbling container
            }
        }
        else { Debug.LogError("StartGame: BaseBlockPlatform reference is missing!"); return; }


        // Reset crane position
        // if (cranePivot != null && baseBlockPlatform != null)
        // {
        //     cranePivot.position = new Vector3(
        //        baseBlockPlatform.position.x,
        //        baseBlockPlatform.position.y + baseBlockPlatform.localScale.y / 2f + ropeLength,
        //        baseBlockPlatform.position.z);
        // }

        // Init colors and crane
        if (colorManager != null) colorManager.InitializeColors();
        if (craneVisuals != null)
        {
            Vector3 moveAxis = GetVectorFromDirection(swingDirection);
            craneVisuals.Initialize(minSpeed, swingDistance, ropeLength, moveAxis);
        }

        // Start particles
        if (backgroundSparkles != null) backgroundSparkles.Play();

        SpawnNewBlock(); // Spawn the first actual block
        if (uiManager) uiManager.UpdateScoreText(_score); // Set UI score to 0

        Debug.Log("Game State: Playing (Started/Restarted)");
    }

    public void PauseGame() // Called by Pause Button
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Paused;
        HandleTimeScale(0);
        if (uiManager) uiManager.ShowPauseMenu(true);
        Debug.Log("Game State: Paused");
    }

    public void ResumeGame() // Called by Resume Button
    {
        if (currentState != GameState.Paused) return;
        currentState = GameState.Playing;
        HandleTimeScale(1);
        if (uiManager) uiManager.ShowPauseMenu(false);
        Debug.Log("Game State: Playing (Resumed)");
    }

    public void GoToMainMenu() // Called by Home Button
    {
        HandleTimeScale(1);
        currentState = GameState.Starting;
        // Reloading the scene is the cleanest way to reset everything
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Returning to Main Menu via Scene Reload...");
    }

    public void RestartGame() // Called by Restart Button
    {
        // For single scene, reloading is the easiest way to reset fully
        HandleTimeScale(1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // If you needed to restart without reload, you would call a detailed ResetGameElements() function here, then call StartGame().
        Debug.Log("Restarting Game via Scene Reload...");
    }

    // Central Game Over method
    private void SetGameOver(string reason)
    {
        if (currentState == GameState.GameOver) return;
        currentState = GameState.GameOver;
        Debug.Log($"Game Over! {reason} Final Score: {_score}");
        _currentFallingBlock = null;
        _currentSwingingBlock = null; // Block input

        if (craneVisuals != null)
            craneVisuals.Hide(); // Stop crane
        if (backgroundSparkles != null) backgroundSparkles.Stop(); // Stop particles

        CancelInvoke(nameof(CheckForMiss)); // Cancel any pending miss checks

        if (uiManager) uiManager.ShowGameOver(); // Tell UI

        // Optional: Freeze time
        // Time.timeScale = 0f;
    }
    // --- END GAME STATE METHODS ---

    public void HandleTimeScale(int val)
    {
        Time.timeScale = val;
    }
    private Vector3 GetVectorFromDirection(SwingDirection direction)
    {
        switch (direction)
        {
            case SwingDirection.Positive_X: return Vector3.right;
            case SwingDirection.Negative_X: return Vector3.left;
            case SwingDirection.Positive_Z: return Vector3.forward;
            case SwingDirection.Negative_Z: return Vector3.back;
            case SwingDirection.Positive_X_Positive_Z: return new Vector3(1, 0, 1).normalized;
            case SwingDirection.Positive_X_Negative_Z: return new Vector3(1, 0, -1).normalized;
            case SwingDirection.Negative_X_Positive_Z: return new Vector3(-1, 0, 1).normalized;
            case SwingDirection.Negative_X_Negative_Z: return new Vector3(-1, 0, -1).normalized;
            default:
                Debug.LogWarning($"GetVectorFromDirection: Unhandled SwingDirection '{direction}'. Defaulting to Right.");
                return Vector3.right;
        }
    }
}