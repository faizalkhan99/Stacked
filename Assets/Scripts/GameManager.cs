// using System.Collections;
// using UnityEngine;
// using UnityEngine.SceneManagement; // Needed for scene reloading on restart/home

// // Enum for game states
// public enum GameState { Starting, Playing, Paused, GameOver }

// // Enum for slicing mechanic choice (Ensure SlicingMechanic.cs exists)
// // public enum SlicingMechanic { CutBlock, NoCut }

// // Enum for swing direction (Ensure SwingDirection.cs exists)
// // public enum SwingDirection { ... }

// public class GameManager : MonoBehaviour
// {
//     private static GameManager instance;
//     public static GameManager Instance
//     {
//         get
//         {
//             if (instance == null) instance = FindFirstObjectByType<GameManager>();
//             if (instance == null) Debug.LogError("GameManager instance could not be found!");
//             return instance;
//         }
//     }
//     public GameState GetCurrentState() => currentState;
//     [Header("Game Objects")]
//     [Tooltip("Drag your in-scene base platform here.")]
//     public Transform baseBlockPlatform; //set as public to access from other scripts.
//     [Tooltip("The simple cube prefab used in CutBlock mode.")]
//     [SerializeField] private GameObject blockPrefab;
//     [Tooltip("The detailed model prefab used in NoCut mode.")]
//     [SerializeField] private GameObject noCutBlockPrefab;
//     [SerializeField] private CameraFollow mainCamera;
//     [SerializeField] private Transform cranePivot;

//     [Header("Managers")]
//     [Tooltip("Drag the 'cranePivot' GameObject here (which has the CraneVisuals script).")]
//     [SerializeField] private CraneVisuals craneVisuals;
//     [SerializeField] private ColorManager colorManager;
//     [Tooltip("Drag the GameObject with the UIManager script here.")]
//     [SerializeField] private UIManager uiManager; // Assign in Inspector

//     [Header("Game Mechanics")]
//     public SlicingMechanic slicingMechanic = SlicingMechanic.CutBlock;
//     [Range(0, 1)]
//     [SerializeField] private float perfectPlacementThreshold = 0.1f;

//     [Header("NoCut Wobble Settings")]
//     [Tooltip("Drag your 'TowerContainer' parent object here.")]
//     [SerializeField] private Transform towerContainer;
//     [Tooltip("The max accumulated offset before the tower collapses.")]
//     [SerializeField] private float maxWobble = 2.0f;
//     [Tooltip("How much the tower visually rocks.")]
//     [SerializeField] private float wobbleVisualFactor = 0.5f;
//     [Tooltip("How fast the tower wobbles back and forth.")]
//     [SerializeField] private float wobbleSpeed = 5f; // Renamed from _wobbleSpeed

//     [Header("Swing Control")]
//     public SwingDirection swingDirection = SwingDirection.Positive_X_Negative_Z;

//     [Header("Game Settings")]
//     [SerializeField] private float minSpeed = 1.5f;
//     [SerializeField] private float maxSpeed = 5f;
//     [SerializeField] private int scoreToReachMaxSpeed = 30;
//     [SerializeField] private float gameOverTimeout = 3f;

//     [Header("Swing Settings")]
//     [SerializeField] private float swingDistance = 4f;
//     [SerializeField] private float ropeLength = 5f;

//     [Header("Debris Settings")]
//     [SerializeField] private float explosionForce = 50f;

//     [Header("Effects")]
//     [SerializeField] private ParticleSystem backgroundSparkles;
//     [Tooltip("How long the block takes to scale in when spawned.")]
//     [SerializeField] private float blockScaleInDuration = 0.2f; // NEW Scale animation time

//     // --- Private State Variables ---
//     private BlockCutter _blockCutter;
//     private Transform _lastPlacedBlock;
//     private Transform _currentSwingingBlock;
//     private Transform _currentFallingBlock;
//     private int _score = 0;
//     private float _wobbleX = 0f;
//     private float _wobbleZ = 0f;
//     private GameState currentState = GameState.Starting; // Start in Menu

//     // --- Event Subscriptions ---
//     void OnEnable() { InputManager.OnPlayerTap += HandlePlayerTap; }
//     void OnDisable() { InputManager.OnPlayerTap -= HandlePlayerTap; }

//     void Awake()
//     {
//         // Enforce Singleton
//         if (instance != null && instance != this) { Destroy(gameObject); return; }
//         instance = this;
//         _blockCutter = gameObject.AddComponent<BlockCutter>();
//     }

//     void Start()
//     {
//         // --- Essential Manager/Object Checks ---
//         if (uiManager == null) { Debug.LogError("UIManager is not assigned in the Inspector!"); enabled = false; return; } // Disable script if UI missing
//         if (blockPrefab == null) Debug.LogError("CutBlock mode 'blockPrefab' is not assigned!");
//         if (noCutBlockPrefab == null) Debug.LogError("NoCut mode 'noCutBlockPrefab' is not assigned!");
//         if (baseBlockPlatform == null) { Debug.LogError("Base Block Platform is not assigned!"); enabled = false; return; }
//         if (craneVisuals == null) { Debug.LogError("Crane Visuals is not assigned!"); enabled = false; return; }
//         if (towerContainer == null && slicingMechanic == SlicingMechanic.NoCut) { Debug.LogWarning("Tower Container is not assigned (needed for NoCut wobble)!"); }
//         // --- Initial Setup ---
//         currentState = GameState.Starting; // Ensure we start in menu state
//         HandleTimeScale(1);
//         if (craneVisuals != null) craneVisuals.Hide(); // Ensure crane is hidden
//         if (towerContainer != null) towerContainer.gameObject.SetActive(false); // Hide tower container initially
//         if (baseBlockPlatform != null) baseBlockPlatform.gameObject.SetActive(false); // Hide base initially
//         if (backgroundSparkles != null) backgroundSparkles.Stop(); // Ensure particles are stopped
//         // Reset camera to initial position via CameraFollow script
//         if (mainCamera != null && baseBlockPlatform != null) mainCamera.ResetCamera(baseBlockPlatform); // Reset on Start     

//         Debug.Log("Game State: Starting (Main Menu)");
//     }

//     void Update()
//     {
//         // Only update wobble visuals if playing
//         if (currentState == GameState.Playing)
//         {
//             UpdateWobbleVisuals();
//         }
//     }

//     // --- Input Handling ---
//     private void HandlePlayerTap()
//     {
//         // Only allow dropping if the game is in the Playing state
//         if (currentState == GameState.Playing && _currentSwingingBlock != null)
//         {
//             DropBlock();
//         }
//         // No restart on tap here - use UI button
//     }

//     // --- Core Gameplay Loop ---
//     private void SpawnNewBlock()
//     {
//         if (currentState != GameState.Playing) return; // Don't spawn if not playing
//         if (cranePivot == null) { Debug.LogError("Crane Pivot not assigned!"); return; }

//         GameObject prefabToSpawn = (slicingMechanic == SlicingMechanic.CutBlock) ? blockPrefab : noCutBlockPrefab;
//         if (prefabToSpawn == null) { Debug.LogError($"Prefab for {slicingMechanic} mode is not assigned!"); return; }

//         GameObject newBlockObj = Instantiate(prefabToSpawn);
//         // --- SCALE: Instantiate at scale zero ---
//         newBlockObj.transform.localScale = Vector3.zero;

//         // Apply Color
//         if (colorManager != null)
//         {
//             Renderer blockRenderer = newBlockObj.GetComponentInChildren<Renderer>();
//             if (blockRenderer != null)
//             {
//                 Color newColor = colorManager.GetCurrentBlockColor(_score);
//                 blockRenderer.material.color = newColor;
//             }
//             else { Debug.LogWarning($"SpawnNewBlock: Could not find Renderer in children of '{newBlockObj.name}'!"); }
//         }

//         // Determine Scale
//         Vector3 targetScale;
//         if (_score == 0) { targetScale = prefabToSpawn.transform.localScale; }
//         else if (slicingMechanic == SlicingMechanic.CutBlock) { targetScale = _lastPlacedBlock.transform.localScale; }
//         else { targetScale = noCutBlockPrefab.transform.localScale; }
//         //newBlockObj.transform.localScale = targetScale;

//         // Attach to Claw
//         BlockFollowClaw follower = newBlockObj.GetComponent<BlockFollowClaw>();
//         if (follower == null) { follower = newBlockObj.AddComponent<BlockFollowClaw>(); Debug.LogWarning($"Added BlockFollowClaw to {newBlockObj.name}. Add it to prefab!"); }

//         if (craneVisuals != null && craneVisuals.clawTransform != null)
//         {
//             Vector3 initialBlockPosition = craneVisuals.clawTransform.position - (Vector3.up * craneVisuals.blockAttachOffset);
//             newBlockObj.transform.position = initialBlockPosition;
//             follower.Initialize(craneVisuals.clawTransform, craneVisuals.blockAttachOffset);
//         }
//         else
//         {
//             Debug.LogError("Cannot position/attach block, CraneVisuals/Claw missing!");
//             Destroy(newBlockObj);
//             SetGameOver("Crane setup error");
//             return;
//         }
//         // --- SCALE OF THE BLOCK: Start the scaling coroutine ---
//         StartCoroutine(ScaleBlockIn(newBlockObj.transform, targetScale));
//         _currentSwingingBlock = newBlockObj.transform;
//     }

//     // --- NEW Coroutine to scale block ---
//     private IEnumerator ScaleBlockIn(Transform blockTransform, Vector3 targetScale)
//     {
//         float timer = 0f;
//         Vector3 startScale = Vector3.zero;

//         while (timer < blockScaleInDuration)
//         {
//             // Check if block was destroyed mid-animation (e.g., game over)
//             if (blockTransform == null) yield break;

//             timer += Time.deltaTime;
//             float t = Mathf.SmoothStep(0f, 1f, timer / blockScaleInDuration); // Smoother interpolation
//             blockTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
//             yield return null;
//         }

//         // Ensure final exact scale
//         if (blockTransform != null) blockTransform.localScale = targetScale;
//     }
//     // ---
//     private void DropBlock()
//     {
//         if (_currentSwingingBlock == null || currentState != GameState.Playing) return;

//         _currentFallingBlock = _currentSwingingBlock;
//         _currentFallingBlock.GetComponent<BlockFollowClaw>().Release();

//         // Add detector and start miss timer
//         DroppedBlockDetector detector = _currentFallingBlock.gameObject.AddComponent<DroppedBlockDetector>();
//         detector.gameManager = this; // Link detector back to this GameManager
//         _currentSwingingBlock = null; // Prevent multiple drops
//         Invoke(nameof(CheckForMiss), gameOverTimeout);
//     }

//     public void ProcessLandedBlock(Transform landedBlock)
//     {
//         if (currentState != GameState.Playing) return;

//         CancelInvoke(nameof(CheckForMiss)); // Block landed, cancel the timeout
//         Transform previousBlock = _lastPlacedBlock;
//         _currentFallingBlock = null;

//         if (previousBlock != null) { previousBlock.tag = "Block"; } // Demote previous top block

//         // Temporarily reset wobble for fair placement checks
//         Quaternion originalWobbleRotation = Quaternion.identity;
//         if (slicingMechanic == SlicingMechanic.NoCut && towerContainer != null)
//         {
//             originalWobbleRotation = towerContainer.rotation;
//             towerContainer.rotation = Quaternion.identity;
//         }

//         // Calculate placement details (Y position, differences, grace zone)
//         float previousBlockTopY = previousBlock.position.y + (previousBlock.localScale.y / 2f);
//         // Assuming NoCut Prefab pivot is CENTERED (adjust if needed)
//         float landedBlockPivotToBottom = landedBlock.localScale.y / 2f;
//         float correctLandedY = previousBlockTopY + landedBlockPivotToBottom;

//         float diffX = Mathf.Abs(landedBlock.position.x - previousBlock.position.x);
//         float diffZ = Mathf.Abs(landedBlock.position.z - previousBlock.position.z);
//         float graceX = (previousBlock.localScale.x / 2) * perfectPlacementThreshold;
//         float graceZ = (previousBlock.localScale.z / 2) * perfectPlacementThreshold;


//         // --- Placement Logic Branching ---
//         bool isPerfect = (_score > 0 && diffX <= graceX && diffZ <= graceZ);

//         if (isPerfect)
//         {
//             Debug.Log("PERFECT PLACEMENT!");
//             AudioManager.Instance.PlaySFX(SoundID.PerfectLanding);
//             landedBlock.SetPositionAndRotation(new Vector3(previousBlock.position.x, correctLandedY, previousBlock.position.z), Quaternion.identity);
//             if (slicingMechanic == SlicingMechanic.CutBlock) { landedBlock.localScale = previousBlock.localScale; }
//             // No wobble change on perfect
//             HandleSuccessfulPlacement(landedBlock, previousBlock);
//         }
//         else if (_score == 0) // First block dropped (never perfect, never cut)
//         {
//             landedBlock.position = new Vector3(landedBlock.position.x, correctLandedY, landedBlock.position.z);
//             RunNoCutLogic(landedBlock, previousBlock);
//         }
//         else // Subsequent non-perfect blocks
//         {
//             if (slicingMechanic == SlicingMechanic.CutBlock)
//             {
//                 // Cutter handles Y pos
//                 RunCutLogic(landedBlock, previousBlock);
//             }
//             else // NoCut logic
//             {
//                 landedBlock.position = new Vector3(landedBlock.position.x, correctLandedY, landedBlock.position.z);
//                 RunNoCutLogic(landedBlock, previousBlock);
//             }
//         }

//         // Restore visual wobble AFTER placement logic
//         if (slicingMechanic == SlicingMechanic.NoCut && towerContainer != null)
//         {
//             towerContainer.rotation = originalWobbleRotation;
//         }
//     }

//     private void RunCutLogic(Transform landedBlock, Transform previousBlock)
//     {
//         bool cutSuccess = _blockCutter.Cut(landedBlock, previousBlock, explosionForce);
//         if (cutSuccess) { HandleSuccessfulPlacement(landedBlock, previousBlock); }
//         else { SetGameOver("Missed the cut!"); }
//     }

//     // This is the robust version, trusting physics for the hit check
//     private void RunNoCutLogic(Transform landedBlock, Transform previousBlock)
//     {
//         // STEP 1: Assume Hit (Collision already confirmed)

//         // STEP 2: Add Wobble
//         float diffX = landedBlock.position.x - previousBlock.position.x;
//         float diffZ = landedBlock.position.z - previousBlock.position.z;
//         _wobbleX += diffX;
//         _wobbleZ += diffZ;

//         // STEP 3: Check Collapse
//         if (Mathf.Abs(_wobbleX) > maxWobble || Mathf.Abs(_wobbleZ) > maxWobble)
//         {
//             Debug.LogWarning($"Tower Collapse Triggered! Wobble (X:{_wobbleX:F2}, Z:{_wobbleZ:F2}) > Max ({maxWobble})");
//             Destroy(landedBlock.gameObject);
//             SetGameOver("Tower collapsed!");
//             return;
//         }

//         // STEP 4: Place Successfully
//         landedBlock.rotation = Quaternion.identity;
//         HandleSuccessfulPlacement(landedBlock, previousBlock);
//     }

//     private void HandleSuccessfulPlacement(Transform newPlacedBlock, Transform previousBlock)
//     {
//         AudioManager.Instance.PlaySFX(SoundID.ImperfectLanding);
//         newPlacedBlock.tag = "TopBlock"; // Mark as the new target

//         // Parent to container only in NoCut mode for wobbling
//         if (towerContainer != null)
//         {
//             newPlacedBlock.SetParent(towerContainer);
//             // Debug.Log($"Parented {newPlacedBlock.name} to TowerContainer."); // Optional log
//         }
//         else if (slicingMechanic == SlicingMechanic.NoCut) // Only warn if needed for wobble
//         {
//             Debug.LogWarning("TowerContainer is null, cannot parent block for wobble effect!");
//         }

//         IncrementScore(); // Handles score and UI update

//         _lastPlacedBlock = newPlacedBlock; // Update reference for next placement

//         // Move crane pivot up
//         if (cranePivot != null && previousBlock != null)
//         {
//             float heightGained = newPlacedBlock.localScale.y; // Assumes pivot is centered
//             float newTargetY = craneVisuals.transform.position.y + heightGained; // Calculate target Y
//             craneVisuals.SetTargetHeight(newTargetY); // Tell visuals to move smoothly
//         }

//         if (mainCamera != null) mainCamera.SetTarget(newPlacedBlock); // Update camera target
//         SpawnNewBlock(); // Spawn the next block immediately
//     }

//     public void IncrementScore()
//     {
//         _score++;
//         if (uiManager) uiManager.UpdateScoreText(_score);
//         // Optional: Play sound every 10 points etc.
//     }

//     public void HandleMissedPlacement(Transform missedBlock) // Hit old block
//     {
//         if (currentState == GameState.GameOver) return;
//         CancelInvoke(nameof(CheckForMiss));
//         _currentFallingBlock = null;
//         SetGameOver("Hit an old block!");
//         Destroy(missedBlock.gameObject, 3f);
//     }

//     private void CheckForMiss() // Timeout - fell into void
//     {
//         if (currentState == GameState.GameOver) return;
//         if (_currentFallingBlock != null)
//         {
//             Destroy(_currentFallingBlock.gameObject);
//             SetGameOver("Dropped into the void!");
//         }
//     }

//     private void UpdateWobbleVisuals()
//     {
//         // Only run wobble if Playing and in NoCut mode
//         if (currentState != GameState.Playing || slicingMechanic != SlicingMechanic.NoCut || towerContainer == null)
//         {
//             if (towerContainer != null) towerContainer.rotation = Quaternion.identity;
//             return;
//         }

//         // "Bounce" wobble effect
//         float wobbleAmountX = Mathf.Abs(Mathf.Sin(Time.time * wobbleSpeed)) * _wobbleX * wobbleVisualFactor;
//         float wobbleAmountZ = Mathf.Abs(Mathf.Sin(Time.time * wobbleSpeed)) * _wobbleZ * wobbleVisualFactor;

//         Quaternion targetRotation = Quaternion.Euler(wobbleAmountZ, 0, -wobbleAmountX);
//         float smoothingSpeed = 3f; // Can make public if needed
//         towerContainer.rotation = Quaternion.Slerp(towerContainer.rotation, targetRotation, Time.deltaTime * smoothingSpeed);
//     }
//     public void StartGame() // Called by Play Button
//     {
//         if (currentState == GameState.Playing) return;

//         Debug.Log("GameManager: StartGame() called.");
//         currentState = GameState.Playing;
//         Time.timeScale = 1f;
//         _score = 0;
//         _wobbleX = 0f;
//         _wobbleZ = 0f;

//         // Reset Tower visuals/position
//         if (towerContainer)
//         {
//             towerContainer.rotation = Quaternion.identity;
//             foreach (Transform child in towerContainer)
//             {
//                 if (child != baseBlockPlatform) Destroy(child.gameObject);
//             }
//         }
//         if (baseBlockPlatform)
//         {
//             baseBlockPlatform.tag = "TopBlock";
//             _lastPlacedBlock = baseBlockPlatform;
//             if (mainCamera) mainCamera.ResetCamera(baseBlockPlatform); // Reset camera
//             if (towerContainer && baseBlockPlatform.parent == towerContainer)
//             {
//                 baseBlockPlatform.SetParent(null);
//             }
//             baseBlockPlatform.gameObject.SetActive(true); // Ensure base is active
//         }
//         else { Debug.LogError("StartGame: BaseBlockPlatform reference is missing!"); return; }

//         if (towerContainer) towerContainer.gameObject.SetActive(true); // Ensure container is active


//         // --- PIVOT POSITION IS *NOT* RESET HERE ---
//         // The position set in the Unity Editor Inspector is respected.


//         // Init colors and crane swing parameters
//         if (colorManager != null) colorManager.InitializeColors();
//         if (craneVisuals != null && cranePivot != null)
//         { // Check both references
//             Vector3 moveAxis = GetVectorFromDirection(swingDirection);
//             // Initialize crane visuals using the current (editor-set or last frame's) pivot position
//             craneVisuals.Initialize(minSpeed, swingDistance, ropeLength, moveAxis);

//             // --- CORRECTED: Tell visuals its CURRENT target height ---
//             craneVisuals.SetTargetHeight(cranePivot.position.y);
//             // ---
//         }
//         else
//         {
//             if (craneVisuals == null) Debug.LogError("StartGame: CraneVisuals reference is missing!");
//             if (cranePivot == null) Debug.LogError("StartGame: CranePivot reference is missing!");
//         }

//         // Start particles
//         if (backgroundSparkles != null) backgroundSparkles.Play();

//         SpawnNewBlock(); // Spawn the first actual block
//         if (uiManager) uiManager.UpdateScoreText(_score);

//         Debug.Log("Game State: Playing (Started/Restarted)");
//     }




//     public void PauseGame() // Called by Pause Button
//     {
//         if (currentState != GameState.Playing) return;
//         currentState = GameState.Paused;
//         AudioManager.Instance.PauseBGM();
//         HandleTimeScale(0);
//         if (uiManager) uiManager.ShowPauseMenu(true);
//         Debug.Log("Game State: Paused");
//     }

//     public void ResumeGame() // Called by Resume Button
//     {
//         if (currentState != GameState.Paused) return;
//         currentState = GameState.Playing;
//         AudioManager.Instance.UnpauseBGM();
//         HandleTimeScale(1);
//         if (uiManager) uiManager.ShowPauseMenu(false);
//         Debug.Log("Game State: Playing (Resumed)");
//     }

//     public void GoToMainMenu() // Called by Home Button
//     {
//         HandleTimeScale(1);
//         currentState = GameState.Starting;
//         // Reloading the scene is the cleanest way to reset everything
//         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//         Debug.Log("Returning to Main Menu via Scene Reload...");
//     }

//     public void RestartGame() // Called by Restart Button
//     {
//         // For single scene, reloading is the easiest way to reset fully
//         HandleTimeScale(1);
//         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//         // If you needed to restart without reload, you would call a detailed ResetGameElements() function here, then call StartGame().
//         Debug.Log("Restarting Game via Scene Reload...");
//     }

//     // Central Game Over method
//     private void SetGameOver(string reason)
//     {
//         if (currentState == GameState.GameOver) return;
//         currentState = GameState.GameOver;
//         Debug.Log($"Game Over! {reason} Final Score: {_score}");
//         _currentFallingBlock = null;
//         _currentSwingingBlock = null; // Block input

//         if (craneVisuals != null)
//             craneVisuals.Hide(); // Stop crane
//         if (backgroundSparkles != null) backgroundSparkles.Stop(); // Stop particles

//         CancelInvoke(nameof(CheckForMiss)); // Cancel any pending miss checks
//         // --- Trigger Camera Zoom Out ---
//         if (mainCamera != null && (towerContainer != null || baseBlockPlatform != null)) // Check if tower exists
//         {
//             Bounds combinedBounds = new Bounds(); // Start empty
//             bool boundsInitialized = false;

//             // Include base platform in bounds calculation
//             if (baseBlockPlatform != null && baseBlockPlatform.GetComponent<Renderer>() != null)
//             {
//                 combinedBounds = baseBlockPlatform.GetComponent<Renderer>().bounds;
//                 boundsInitialized = true;
//             }

//             // Encapsulate all blocks within the tower container
//             if (towerContainer != null)
//             {
//                 foreach (Transform block in towerContainer)
//                 {
//                     // Check if the block itself has a renderer or if its children do
//                     Renderer r = block.GetComponent<Renderer>() ?? block.GetComponentInChildren<Renderer>();
//                     if (r != null)
//                     {
//                         if (boundsInitialized) combinedBounds.Encapsulate(r.bounds);
//                         else { combinedBounds = r.bounds; boundsInitialized = true; } // Init with first block if base missing renderer
//                     }
//                 }
//             }

//             if (boundsInitialized) mainCamera.AnimateZoomToShowTower(combinedBounds); // Tell camera to zoom
//             else Debug.LogWarning("SetGameOver: Could not calculate tower bounds for zoom (No Renderers found?).");

//         }
//         else
//         {
//             Debug.LogError("Cannot zoom out - CameraFollow or TowerContainer/BasePlatform missing!");
//         }
//         // ---
//         if (uiManager)
//         {
//             AudioManager.Instance.PlaySFX(SoundID.GameOver);
//             AudioManager.Instance.PauseBGM(); // Pause music
//             uiManager.ShowGameOver(); // Tell UI AFTER triggering zoom
//         }
//         // Optional: Freeze time
//         // Time.timeScale = 0f;
//     }
//     // --- END GAME STATE METHODS ---

//     public void HandleTimeScale(int val)
//     {
//         Time.timeScale = val;
//     }
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
//             default:
//                 Debug.LogWarning($"GetVectorFromDirection: Unhandled SwingDirection '{direction}'. Defaulting to Right.");
//                 return Vector3.right;
//         }
//     }
// }





















using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GameState { Starting, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<GameManager>();
            if (instance == null) Debug.LogError("GameManager instance could not be found!");
            return instance;
        }
    }
    public GameState GetCurrentState() => currentState;

    [Header("Game Objects")]
    public Transform baseBlockPlatform;
    public GameObject blockPrefab; // Cube for Cut Mode
    public GameObject noCutBlockPrefab; // Detailed model for NoCut Mode
    public CameraFollow mainCamera;
    public Transform cranePivot;

    [Header("Managers")]
    public CraneVisuals craneVisuals;
    public ColorManager colorManager;
    public UIManager uiManager;

    [Header("Game Mechanics")]
    public SlicingMechanic slicingMechanic = SlicingMechanic.CutBlock;
    [Range(0, 1)]
    public float perfectPlacementThreshold = 0.1f;

    // --- CONSOLIDATED COMBO SETTINGS ---
    [Header("Combo Settings (For BOTH Modes)")]
    [Tooltip("Combo count needed to trigger a block size increase (e.g., 5). Triggers on multiples (5, 10, 15...).")]
    public int comboToIncreaseSize = 5;
    [Tooltip("How long the landed block takes to animate its new size (in seconds).")]
    public float comboScaleDuration = 0.4f;

    [Header("CutBlock Combo Settings")]
    [Tooltip("How much to increase the X and Z scale by in Cut mode when a combo is hit.")]
    public float scaleIncreaseAmountCut = 0.1f;
    // ---

    [Header("NoCut Wobble Settings")]
    public Transform towerContainer;
    public float maxWobble = 2.0f;
    public float wobbleVisualFactor = 0.5f;
    public float wobbleSpeed = 5f;

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
    [Tooltip("How long the block takes to scale in when spawned.")]
    public float blockScaleAnimDuration = 0.2f;

    // --- Private State Variables ---
    private BlockCutter _blockCutter;
    private Transform _lastPlacedBlock;
    private Transform _currentSwingingBlock;
    private Transform _currentFallingBlock;
    private int _score = 0;
    private float _wobbleX = 0f;
    private float _wobbleZ = 0f;
    private GameState currentState = GameState.Starting;
    private int _perfectCombo = 0;
    private Vector3 _currentNoCutScale;

    // --- NEW HIGH SCORE ---
    private int _highScore = 0;
    private const string HighScoreKey = "STACK_HIGH_SCORE"; // Key for PlayerPrefs
    // ---

    // --- Event Subscriptions ---
    void OnEnable() { InputManager.OnPlayerTap += HandlePlayerTap; }
    void OnDisable() { InputManager.OnPlayerTap -= HandlePlayerTap; }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        _blockCutter = gameObject.AddComponent<BlockCutter>();
        // --- LOAD HIGH SCORE ---
        _highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        // ---
    }

    void Start()
    {
        // --- Essential Manager/Object Checks ---
        if (uiManager == null) { Debug.LogError("UIManager is not assigned!"); enabled = false; return; }
        if (mainCamera == null) { Debug.LogError("CameraFollow reference (mainCamera) is not assigned!"); enabled = false; return; }
        if (blockPrefab == null) Debug.LogError("CutBlock mode 'blockPrefab' is not assigned!");
        if (noCutBlockPrefab == null) Debug.LogError("NoCut mode 'noCutBlockPrefab' is not assigned!");
        if (baseBlockPlatform == null) { Debug.LogError("Base Block Platform is not assigned!"); enabled = false; return; }
        if (craneVisuals == null) { Debug.LogError("Crane Visuals is not assigned!"); enabled = false; return; }
        if (cranePivot == null) { Debug.LogError("Crane Pivot GameObject is not assigned!"); enabled = false; return; }
        if (towerContainer == null && slicingMechanic == SlicingMechanic.NoCut) { Debug.LogWarning("Tower Container is not assigned (needed for NoCut wobble)!"); }

        currentState = GameState.Starting;
        Time.timeScale = 1f;
        // Hide game elements initially.
        if (craneVisuals != null) craneVisuals.Hide();
        if (towerContainer != null) towerContainer.gameObject.SetActive(false);
        if (baseBlockPlatform != null) baseBlockPlatform.gameObject.SetActive(false);
        if (backgroundSparkles != null) backgroundSparkles.Stop();
        if (mainCamera != null && baseBlockPlatform != null) mainCamera.ResetCamera(baseBlockPlatform);

        Debug.Log("Game State: Starting (Main Menu)");
    }
    // --- NEW: Public getter for UIManager ---
    public int GetHighScore() { return _highScore; }
    // ---
    void Update()
    {
        if (currentState == GameState.Playing)
        {
            UpdateWobbleVisuals();
        }
    }

    private void HandlePlayerTap()
    {
        if (currentState == GameState.Playing && _currentSwingingBlock != null)
        {
            DropBlock();
        }
    }

    private void SpawnNewBlock()
    {
        if (currentState != GameState.Playing) return;
        if (cranePivot == null) { Debug.LogError("Crane Pivot not assigned!"); return; }

        GameObject prefabToSpawn = (slicingMechanic == SlicingMechanic.CutBlock) ? blockPrefab : noCutBlockPrefab;
        if (prefabToSpawn == null) { Debug.LogError($"Prefab for {slicingMechanic} mode is not assigned!"); return; }

        GameObject newBlockObj = Instantiate(prefabToSpawn);
        newBlockObj.transform.localScale = Vector3.zero; // Start at scale zero

        // Apply Color
        if (colorManager != null)
        {
            Renderer blockRenderer = newBlockObj.GetComponentInChildren<Renderer>();
            if (blockRenderer != null)
            {
                Color newColor = colorManager.GetCurrentBlockColor(_score);
                blockRenderer.material.color = newColor;
            }
            else { Debug.LogWarning($"SpawnNewBlock: Could not find Renderer in children..."); }
        }

        // Determine Target Scale
        Vector3 targetScale;
        if (_score == 0) { targetScale = prefabToSpawn.transform.localScale; }
        else if (slicingMechanic == SlicingMechanic.CutBlock) { targetScale = _lastPlacedBlock.transform.localScale; }
        else { targetScale = _currentNoCutScale; } // Use the current NoCut scale

        // Attach to Claw
        BlockFollowClaw follower = newBlockObj.GetComponent<BlockFollowClaw>();
        if (follower == null) { follower = newBlockObj.AddComponent<BlockFollowClaw>(); }
        if (craneVisuals != null && craneVisuals.clawTransform != null)
        {
            Vector3 initialBlockPosition = craneVisuals.clawTransform.position - (Vector3.up * craneVisuals.blockAttachOffset);
            newBlockObj.transform.position = initialBlockPosition; // Set position BEFORE init
            follower.Initialize(craneVisuals.clawTransform, craneVisuals.blockAttachOffset);
        }
        else { /* Error handling */ return; }

        // Start Scaling Coroutine
        StartCoroutine(ScaleBlockIn(newBlockObj.transform, targetScale, blockScaleAnimDuration));

        _currentSwingingBlock = newBlockObj.transform;
    }

    private IEnumerator ScaleBlockIn(Transform blockTransform, Vector3 targetScale, float duration)
    {
        float timer = 0f;
        Vector3 startScale = blockTransform.localScale; // Use current scale (might be 0)
        while (timer < duration)
        {
            if (blockTransform == null) yield break;
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            blockTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        if (blockTransform != null) blockTransform.localScale = targetScale;
    }

    private void DropBlock()
    {
        if (_currentSwingingBlock == null || currentState != GameState.Playing) return;
        _currentFallingBlock = _currentSwingingBlock;
        _currentFallingBlock.GetComponent<BlockFollowClaw>().Release();
        DroppedBlockDetector detector = _currentFallingBlock.gameObject.AddComponent<DroppedBlockDetector>();
        detector.gameManager = this;
        _currentSwingingBlock = null;
        Invoke(nameof(CheckForMiss), gameOverTimeout);
    }

    public void ProcessLandedBlock(Transform landedBlock)
    {
        if (currentState != GameState.Playing) return;
        CancelInvoke(nameof(CheckForMiss));
        Transform previousBlock = _lastPlacedBlock;
        _currentFallingBlock = null;

        if (previousBlock != null) { previousBlock.tag = "Block"; }

        Quaternion originalWobbleRotation = Quaternion.identity;
        if (slicingMechanic == SlicingMechanic.NoCut && towerContainer != null)
        {
            originalWobbleRotation = towerContainer.rotation;
            towerContainer.rotation = Quaternion.identity;
        }

        // --- Placement Calculations ---
        float previousBlockTopY = previousBlock.position.y + (previousBlock.localScale.y / 2f);
        float landedBlockPivotToBottom = landedBlock.localScale.y / 2f;
        float correctLandedY = previousBlockTopY + landedBlockPivotToBottom;

        float diffX = Mathf.Abs(landedBlock.position.x - previousBlock.position.x);
        float diffZ = Mathf.Abs(landedBlock.position.z - previousBlock.position.z);
        float graceX = (previousBlock.localScale.x / 2) * perfectPlacementThreshold;
        float graceZ = (previousBlock.localScale.z / 2) * perfectPlacementThreshold;

        bool isPerfect = (_score > 0 && diffX <= graceX && diffZ <= graceZ);

        // --- PERFECT PLACEMENT ---
        if (isPerfect)
        {
            AudioManager.Instance.PlaySFX(SoundID.PerfectLanding);
            Debug.Log("PERFECT PLACEMENT!");

            _perfectCombo++;
            if (uiManager) uiManager.ShowComboText(_perfectCombo);

            // Align Y immediately, then smooth-slide to perfect XY
            landedBlock.position = new Vector3(landedBlock.position.x, correctLandedY, landedBlock.position.z);
            StartCoroutine(SmoothSlideAndSettle(landedBlock, previousBlock, 0.3f));

            bool isComboTrigger = (_perfectCombo > 0 && _perfectCombo % comboToIncreaseSize == 0);
            if (isComboTrigger && slicingMechanic == SlicingMechanic.CutBlock)
            {
                StartCoroutine(AnimateLandedBlockScale(landedBlock, previousBlock, comboScaleDuration));
            }
            else
            {
                if (slicingMechanic == SlicingMechanic.CutBlock)
                    landedBlock.localScale = previousBlock.localScale;

                HandleSuccessfulPlacement(landedBlock, previousBlock);
            }

            return;
        }
        else // --- IMPERFECT PLACEMENT ---
        {
            AudioManager.Instance.PlaySFX(SoundID.ImperfectLanding);
            // Reset combo
            if (_perfectCombo > 0 && uiManager) uiManager.HideComboText();
            _perfectCombo = 0;

            if (_score == 0)
            {
                // First block ever placed → accept it, don’t run Cut or NoCut logic
                landedBlock.SetPositionAndRotation(new Vector3(
                    landedBlock.position.x,
                    correctLandedY,
                    landedBlock.position.z
                ), Quaternion.identity);

                // Always accept first block as valid foundation
                HandleSuccessfulPlacement(landedBlock, previousBlock);
                return;
            }
            else if (slicingMechanic == SlicingMechanic.CutBlock)
            {
                RunCutLogic(landedBlock, previousBlock);
            }
            else // NoCut
            {
                landedBlock.position = new Vector3(landedBlock.position.x, correctLandedY, landedBlock.position.z);
                RunNoCutLogic(landedBlock, previousBlock);
            }
        }

        // Restore visual wobble
        if (slicingMechanic == SlicingMechanic.NoCut && towerContainer != null)
        {
            towerContainer.rotation = originalWobbleRotation;
        }
    }
    private IEnumerator SmoothSlideAndSettle(Transform landedBlock, Transform previousBlock, float duration)
    {
        if (landedBlock == null || previousBlock == null) yield break;

        Vector3 startPos = landedBlock.position;
        Vector3 targetPos = new Vector3(
            previousBlock.position.x,
            startPos.y, // Y already aligned before calling
            previousBlock.position.z
        );

        Quaternion startRot = landedBlock.rotation;
        // Give a tiny random rotation for visual life
        Quaternion midRot = Quaternion.Euler(
            startRot.eulerAngles.x + Random.Range(-2f, 2f),
            startRot.eulerAngles.y,
            startRot.eulerAngles.z + Random.Range(-2f, 2f)
        );
        Quaternion targetRot = Quaternion.identity;

        float timer = 0f;
        while (timer < duration)
        {
            if (landedBlock == null) yield break;
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);

            // First half – ease to mid rotation, second half – ease to target
            float rotT = t < 0.5f ? Mathf.SmoothStep(0f, 1f, t * 2f) : Mathf.SmoothStep(0f, 1f, (t - 0.5f) * 2f);

            landedBlock.position = Vector3.Lerp(startPos, targetPos, t);

            if (t < 0.5f)
                landedBlock.rotation = Quaternion.Slerp(startRot, midRot, rotT);
            else
                landedBlock.rotation = Quaternion.Slerp(midRot, targetRot, rotT);

            yield return null;
        }

        landedBlock.position = targetPos;
        landedBlock.rotation = targetRot;
    }

    // This coroutine is now ONLY for the CutBlock combo reward
    private IEnumerator AnimateLandedBlockScale(Transform landedBlock, Transform previousBlock, float duration)
    {
        // Calculate the new scale based on Cut mode rules
        float newX = landedBlock.localScale.x + scaleIncreaseAmountCut;
        float newZ = landedBlock.localScale.z + scaleIncreaseAmountCut;
        Vector3 newScale = new(newX, landedBlock.localScale.y, newZ);

        AudioManager.Instance.PlaySFX(SoundID.BlockGrow);
        // Animate the scale of the block that just landed
        yield return StartCoroutine(ScaleBlockIn(landedBlock, newScale, duration));

        // NOW, after the animation is done, complete the placement
        HandleSuccessfulPlacement(landedBlock, previousBlock);
    }

    private void RunCutLogic(Transform landedBlock, Transform previousBlock)
    {
        // This is called on a NON-PERFECT placement
        _perfectCombo = 0; // Reset combo
        if (uiManager) uiManager.HideComboText();

        bool cutSuccess = _blockCutter.Cut(landedBlock, previousBlock, explosionForce);
        if (cutSuccess) { HandleSuccessfulPlacement(landedBlock, previousBlock); }
        else { SetGameOver("Missed the cut!"); }
    }

    private void RunNoCutLogic(Transform landedBlock, Transform previousBlock)
    {
        // This is called on a NON-PERFECT placement (or score 0)
        if (_score > 0) // Don't reset for the very first block
        {
            _perfectCombo = 0;
            if (uiManager) uiManager.HideComboText();
        }

        // ... (Wobble logic and collapse check) ...
        float diffX = landedBlock.position.x - previousBlock.position.x;
        float diffZ = landedBlock.position.z - previousBlock.position.z;
        _wobbleX += diffX; _wobbleZ += diffZ;

        if (Mathf.Abs(_wobbleX) > maxWobble || Mathf.Abs(_wobbleZ) > maxWobble)
        {
            Destroy(landedBlock.gameObject); SetGameOver("Tower collapsed!"); return;
        }

        landedBlock.rotation = Quaternion.identity;
        HandleSuccessfulPlacement(landedBlock, previousBlock);
    }

    // private void HandleSuccessfulPlacement(Transform newPlacedBlock, Transform previousBlock)
    // {
    //     newPlacedBlock.tag = "TopBlock";
    //     // Parent block to container (for wobble AND bounds calculation)
    //     if (towerContainer != null)
    //         newPlacedBlock.SetParent(towerContainer);
    //     else Debug.LogWarning("TowerContainer is null, cannot parent block!");

    //     IncrementScore();
    //     _lastPlacedBlock = newPlacedBlock;

    //     // Move crane pivot up smoothly via CraneVisuals
    //     if (craneVisuals != null && cranePivot != null && previousBlock != null)
    //     {
    //         float heightGained = newPlacedBlock.localScale.y;
    //         float newTargetY = cranePivot.position.y + heightGained;
    //         craneVisuals.SetTargetHeight(newTargetY);
    //     }

    //     if (mainCamera != null) mainCamera.SetTarget(newPlacedBlock);
    //     else Debug.LogError("HandleSuccessfulPlacement: mainCamera reference is missing!");

    //     SpawnNewBlock();
    // }


    private void HandleSuccessfulPlacement(Transform newPlacedBlock, Transform previousBlock)
    {
        AudioManager.Instance.PlaySFX(SoundID.ImperfectLanding);
        newPlacedBlock.tag = "TopBlock"; // Mark as the new target

        // Parent to container only in NoCut mode for wobbling
        if (towerContainer != null)
        {
            newPlacedBlock.SetParent(towerContainer);
            // Debug.Log($"Parented {newPlacedBlock.name} to TowerContainer."); // Optional log
        }
        else if (slicingMechanic == SlicingMechanic.NoCut) // Only warn if needed for wobble
        {
            Debug.LogWarning("TowerContainer is null, cannot parent block for wobble effect!");
        }

        IncrementScore(); // Handles score and UI update

        _lastPlacedBlock = newPlacedBlock; // Update reference for next placement

        // Move crane pivot up
        if (cranePivot != null && previousBlock != null)
        {
            float heightGained = newPlacedBlock.localScale.y; // Assumes pivot is centered
            float newTargetY = craneVisuals.transform.position.y + heightGained; // Calculate target Y
            craneVisuals.SetTargetHeight(newTargetY); // Tell visuals to move smoothly
        }

        if (mainCamera != null) mainCamera.SetTarget(newPlacedBlock); // Update camera target
        SpawnNewBlock(); // Spawn the next block immediately
    }




    public void IncrementScore()
    {
        _score++;
        if (uiManager) uiManager.UpdateScoreText(_score);
    }

    public void HandleMissedPlacement(Transform missedBlock)
    {
        if (currentState == GameState.GameOver) return;
        CancelInvoke(nameof(CheckForMiss));
        _currentFallingBlock = null;
        SetGameOver("Hit an old block!");
        Destroy(missedBlock.gameObject, 3f);
    }

    private void CheckForMiss()
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
        if (currentState != GameState.Playing || slicingMechanic != SlicingMechanic.NoCut || towerContainer == null)
        {
            if (towerContainer != null) towerContainer.rotation = Quaternion.identity;
            return;
        }

        float wobbleAmountX = Mathf.Abs(Mathf.Sin(Time.time * wobbleSpeed)) * _wobbleX * wobbleVisualFactor;
        float wobbleAmountZ = Mathf.Abs(Mathf.Sin(Time.time * wobbleSpeed)) * _wobbleZ * wobbleVisualFactor;

        Quaternion targetRotation = Quaternion.Euler(wobbleAmountZ, 0, -wobbleAmountX);
        float smoothingSpeed = 3f;
        towerContainer.rotation = Quaternion.Slerp(towerContainer.rotation, targetRotation, Time.deltaTime * smoothingSpeed);
    }

    // --- GAME STATE METHODS ---
    public void StartGame()
    {
        if (currentState == GameState.Playing) return;
        Debug.Log("GameManager: StartGame() called.");
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        _score = 0;
        _wobbleX = 0f; _wobbleZ = 0f;

        // --- RESET COMBO AND SCALE ---
        _perfectCombo = 0;
        // Initialize the scale based on which prefab we will use
        if (slicingMechanic == SlicingMechanic.CutBlock)
        {
            _currentNoCutScale = blockPrefab.transform.localScale; // Use Cut prefab scale as base
        }
        else
        {
            _currentNoCutScale = noCutBlockPrefab.transform.localScale; // Use NoCut prefab scale as base
        }
        // ---

        // Reset Tower
        if (towerContainer)
        {
            towerContainer.rotation = Quaternion.identity;
            foreach (Transform child in towerContainer)
            {
                if (child != baseBlockPlatform) Destroy(child.gameObject);
            }
            towerContainer.gameObject.SetActive(true); // Ensure container is active
        }
        if (baseBlockPlatform)
        {
            baseBlockPlatform.tag = "TopBlock";
            _lastPlacedBlock = baseBlockPlatform;
            if (mainCamera) mainCamera.ResetCamera(baseBlockPlatform); // Reset camera
            if (towerContainer && baseBlockPlatform.parent == towerContainer) { baseBlockPlatform.SetParent(null); }
            baseBlockPlatform.gameObject.SetActive(true); // Ensure base is active
        }
        else { Debug.LogError("StartGame: BaseBlockPlatform reference is missing!"); return; }


        // --- Reset Crane ---
        if (cranePivot != null && baseBlockPlatform != null)
        {
            if (craneVisuals != null)
            {
                Vector3 moveAxis = GetVectorFromDirection(swingDirection);
                craneVisuals.Initialize(minSpeed, swingDistance, ropeLength, moveAxis);
                craneVisuals.SetTargetHeight(cranePivot.position.y); // Use editor position
            }
        }

        // Init colors, particles
        if (colorManager != null) colorManager.InitializeColors();
        if (backgroundSparkles != null) backgroundSparkles.Play();

        SpawnNewBlock();
        if (uiManager) uiManager.UpdateScoreText(_score);
        Debug.Log("Game State: Playing (Started/Restarted)");
    }

    public void PauseGame()
    {
        AudioManager.Instance.PauseBGM();
        if (currentState != GameState.Playing) return;
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        if (uiManager) uiManager.ShowPauseMenu(true);
        Debug.Log("Game State: Paused");
    }

    public void ResumeGame()
    {
        AudioManager.Instance.UnpauseBGM();
        if (currentState != GameState.Paused) return;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        if (uiManager) uiManager.ShowPauseMenu(false);
        Debug.Log("Game State: Playing (Resumed)");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SetGameOver(string reason)
    {
        if (currentState == GameState.GameOver) return;
        currentState = GameState.GameOver;
        Debug.Log($"Game Over! {reason} Final Score: {_score}");
        _currentFallingBlock = null;
        _currentSwingingBlock = null;

        _perfectCombo = 0; // Reset combo
        if (uiManager) uiManager.HideComboText(); // Hide text

        if (craneVisuals != null) craneVisuals.Hide();
        if (backgroundSparkles != null) backgroundSparkles.Stop();
        CancelInvoke(nameof(CheckForMiss));

        // --- NEW HIGH SCORE LOGIC ---
        bool isNewHighScore = false;
        if (_score > _highScore)
        {
            isNewHighScore = true;
            _highScore = _score;
            PlayerPrefs.SetInt(HighScoreKey, _highScore);
            PlayerPrefs.Save();
            Debug.Log("New High Score Saved!");
        }
        // ---

        // Trigger Camera Zoom Out
        if (mainCamera != null && (towerContainer != null || baseBlockPlatform != null))
        {
            Bounds combinedBounds = new Bounds();
            bool boundsInitialized = false;
            Renderer baseRenderer = baseBlockPlatform?.GetComponentInChildren<Renderer>();
            if (baseRenderer != null && baseRenderer.enabled) { combinedBounds = baseRenderer.bounds; boundsInitialized = true; }

            if (towerContainer != null)
            {
                for (int i = 0; i < towerContainer.childCount; i++)
                {
                    Transform block = towerContainer.GetChild(i);
                    Renderer r = block.GetComponentInChildren<Renderer>();
                    if (r != null && r.enabled)
                    {
                        if (boundsInitialized) combinedBounds.Encapsulate(r.bounds);
                        else { combinedBounds = r.bounds; boundsInitialized = true; }
                    }
                }
            }
            if (boundsInitialized)
            {
                mainCamera.AnimateZoomToShowTower(combinedBounds);
            }
            else
            {
                Debug.LogWarning("SetGameOver: Could not calculate bounds for zoom.");
            }
        }
        else
        {
            Debug.LogError("Cannot zoom out - CameraFollow or Tower/Base missing!");
        }

        if (uiManager)
        {
            AudioManager.Instance.PlaySFX(SoundID.GameOver);
            uiManager.ShowGameOver(_score, _highScore, isNewHighScore);
        }
    }
    // --- END GAME STATE METHODS ---

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