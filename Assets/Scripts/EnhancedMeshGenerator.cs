using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

// Enhanced MeshGenerator with collision, player control, and camera following
public class EnhancedMeshGenerator : MonoBehaviour
{
    [Header("Mesh and Material Settings")]
    public Material material;
    public int instanceCount = 100;
    private Mesh cubeMesh;
    private List<Matrix4x4> matrices = new List<Matrix4x4>(); //
    private List<int> colliderIds = new List<int>(); // 
    public List<Powerup> powerups = new List<Powerup>(); //

    [Header("Box Dimensions")]
    public float width = 1f;
    public float height = 1f;
    public float depth = 1f;

    [Header("Player Movement Settings")]
    public float movementSpeed = 5f;
    public float gravity = 9.8f;

    [Header("Player Settings")]
    private int playerID = -1;
    private Vector3 playerVelocity = Vector3.zero;
    private bool isGrounded = false;


    [Header("Player Combat")]

    public float playerBounceBack = 3f;
    public int playerHealth = 3;
    public float playerHitTimer = 0f;
    public float hitStunDuration = 0.3f;
    public float hitStunTimer = 0f;

    [Header("Enemy Settings")]
    public int enemyCount = 5;
    public float enemySpeed = 2f;
    private List<int> enemyIDs = new List<int>();
    private Dictionary<int, Vector3> enemyVelocities = new Dictionary<int, Vector3>();

    [Header("End Zone")]
    public Vector3 endZonePosition = new Vector3(20f, 0f, 0f);
    public Vector3 endZoneSize = new Vector3(3f, 3f, 3f);

    private int endZoneID = -1;


    [Header("Camera Settings")]
    // Camera reference
    public PlayerCameraFollow cameraFollow;

    [Header("Z Position")]
    // Z-position constant for all boxes
    public float constantZPosition = 0f;

    [Header("Spawn Settings")]
    // Range for random generation
    public float minX = -50f;
    public float maxX = 50f;
    public float minY = -50f;
    public float maxY = 50f;

    [Header("Ground Settings")]
    // Ground plane settings
    public float groundY = -20f;
    public float groundWidth = 200f;
    public float groundDepth = 200f;

    [Header("Debug Gizmos")]
    public bool showGround = true;
    public bool showSpawnRange = true;
    public bool showBoxes = true;

    public bool showEndZone = true;

    void Start()
    {
        // Find or create camera if not assigned
        SetupCamera();

        // Create the cube mesh
        CreateCubeMesh();

        // Create player box
        CreatePlayer();

        // Create ground
        CreateGround();

        // Set up random boxes
        GenerateRandomBoxes();

        CreateEndZone();
    }

    void SetupCamera()
    {
        if (cameraFollow == null)
        {
            // Try to find existing camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // Check if it already has our script
                cameraFollow = mainCamera.GetComponent<PlayerCameraFollow>();
                if (cameraFollow == null)
                {
                    // Add our script to existing camera
                    cameraFollow = mainCamera.gameObject.AddComponent<PlayerCameraFollow>();
                }
            }
            else
            {
                // No main camera found, create a new one
                GameObject cameraObj = new GameObject("PlayerCamera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cameraFollow = cameraObj.AddComponent<PlayerCameraFollow>();

                // Set this as the main camera
                cam.tag = "MainCamera";
            }

            // Configure default camera settings
            cameraFollow.offset = new Vector3(0, 0, -15);
            cameraFollow.smoothSpeed = 0.1f;
        }
    }

    void CreateCubeMesh() // Create a simple cube mesh with proper dimensions and UVs
    {
        cubeMesh = new Mesh();

        // Create 8 vertices for the cube (corners)
        Vector3[] vertices = new Vector3[8]
        {
               // Bottom face
    new Vector3(-width/2, -height/2, -depth/2),
    new Vector3(width/2, -height/2, -depth/2),
    new Vector3(width/2, -height/2, depth/2),
    new Vector3(-width/2, -height/2, depth/2),

    // Top face
    new Vector3(-width/2, height/2, -depth/2),
    new Vector3(width/2, height/2, -depth/2),
    new Vector3(width/2, height/2, depth/2),
    new Vector3(-width/2, height/2, depth/2)
        };

        // Triangles for the 6 faces (2 triangles per face)
        int[] triangles = new int[36]
        {
            // Front face triangles (facing -Z)
            0, 4, 1,
            1, 4, 5,
            
            // Back face triangles (facing +Z)
            2, 6, 3,
            3, 6, 7,
            
            // Left face triangles (facing -X)
            0, 3, 4,
            4, 3, 7,
            
            // Right face triangles (facing +X)
            1, 5, 2,
            2, 5, 6,
            
            // Bottom face triangles (facing -Y)
            0, 1, 3,
            3, 1, 2,
            
            // Top face triangles (facing +Y)
            4, 7, 5,
            5, 7, 6
        };

        Vector2[] uvs = new Vector2[8];
        for (int i = 0; i < 8; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / width, vertices[i].z / depth);
        }

        cubeMesh.vertices = vertices;
        cubeMesh.triangles = triangles;
        cubeMesh.uv = uvs;
        cubeMesh.RecalculateNormals();
        cubeMesh.RecalculateBounds();
    }

    void CreateGround()
    {
        // Create a large ground plane
        Vector3 groundPosition = new Vector3(0, groundY, constantZPosition);
        Vector3 groundScale = new Vector3(groundWidth, 1f, groundDepth);
        Quaternion groundRotation = Quaternion.identity;

        // Register with collision system - use actual dimensions
        int groundID = CollisionManager.Instance.RegisterCollider(
            groundPosition,
            new Vector3(groundWidth, 1f, groundDepth),
            false);

        // Create transformation matrix
        Matrix4x4 groundMatrix = Matrix4x4.TRS(groundPosition, groundRotation, groundScale);
        matrices.Add(groundMatrix);
        colliderIds.Add(groundID);

        // Update the matrix in collision manager
        CollisionManager.Instance.UpdateMatrix(groundID, groundMatrix);
    }

    void GenerateRandomBoxes()
    {
        // Create random boxes (excluding player and ground)
        for (int i = 0; i < instanceCount - 2; i++)
        {
            // Random position (constant Z)
            Vector3 position = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                constantZPosition
            );

            // Random rotation only around Z axis
            Quaternion rotation = Quaternion.identity; // No rotation for better collision handlingq

            // Random non-uniform scale - different for each dimension
            Vector3 scale = new Vector3(
                Random.Range(0.5f, 3f),
                Random.Range(0.5f, 3f),
                Random.Range(0.5f, 3f)
            );

            // Register with collision system - properly handle rectangular shapes
            int id = CollisionManager.Instance.RegisterCollider(
                position,
                new Vector3(width * scale.x, height * scale.y, depth * scale.z),
                false);

            // Create transformation matrix
            Matrix4x4 boxMatrix = Matrix4x4.TRS(position, rotation, scale);
            matrices.Add(boxMatrix);
            colliderIds.Add(id);

            // Update the matrix in collision manager
            CollisionManager.Instance.UpdateMatrix(id, boxMatrix);
        }
    }

    //=========================================================================================================================
    //Update 
    //=========================================================================================================================

    void Update()
    {
        // Update hit timers
        if (playerHitTimer > 0) playerHitTimer -= Time.deltaTime;
        if (hitStunTimer > 0) hitStunTimer -= Time.deltaTime;


        CheckEndZone();

        UpdatePlayer();
        RenderBoxes();
        UpdateEnemies();
    }


    //=========================================================================================================================
    // Player 
    //=========================================================================================================================
    void UpdatePlayer()
    {
        if (playerID == -1) return;

        // Get current player matrix
        Matrix4x4 playerMatrix = matrices[colliderIds.IndexOf(playerID)];
        DecomposeMatrix(playerMatrix, out Vector3 pos, out Quaternion rot, out Vector3 scale);

        //Gravity
        if (!isGrounded)
        {
            float gravityScale = playerVelocity.y > 0 ? 9.8f : 4.5f;
            playerVelocity.y -= gravityScale * Time.deltaTime;
        }

        //Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            playerVelocity.y = 12f;
            isGrounded = false;
        }

        // Get horizontal input
        float horizontal = 0;
        if (hitStunTimer <= 0)
        {
            if (Input.GetKey(KeyCode.A)) horizontal -= 1;
            if (Input.GetKey(KeyCode.D)) horizontal += 1;
        }

        // Update player position based on input
        if (!isGrounded)
        {
            horizontal *= 0.5f;
        }
        Vector3 newPos = pos;
        newPos.x += (horizontal * movementSpeed + playerVelocity.x) * Time.deltaTime;

        // Apply horizontal movement if no collision
        if (!CheckCollisionAt(playerID, new Vector3(newPos.x, pos.y, pos.z)))
        {
            pos.x = newPos.x;
        }

        // Apply gravity/vertical movement
        newPos = pos;
        newPos.y += playerVelocity.y * Time.deltaTime;

        // Check for vertical collisions
        if (CheckCollisionAt(playerID, new Vector3(pos.x, newPos.y, pos.z)))
        {
            // We hit something below or above
            if (playerVelocity.y < 0)
            {
                // We hit something below
                isGrounded = true;
            }
            playerVelocity.y = 0;
        }
        else
        {
            // No collision, apply gravity
            pos.y = newPos.y;
            isGrounded = false;
        }

        // Update matrix
        Matrix4x4 newMatrix = Matrix4x4.TRS(pos, rot, scale);
        matrices[colliderIds.IndexOf(playerID)] = newMatrix;

        // Update collider position - properly handle rectangular shape
        CollisionManager.Instance.UpdateCollider(playerID, pos, new Vector3(width * scale.x, height * scale.y, depth * scale.z));
        CollisionManager.Instance.UpdateMatrix(playerID, newMatrix);

        // Update camera to follow player
        if (cameraFollow != null)
        {
            cameraFollow.SetPlayerPosition(pos);
        }

        playerVelocity.x *= 0.9f;

        // Stop very small movement
        if (Mathf.Abs(playerVelocity.x) < 0.01f)
            playerVelocity.x = 0f;


    }

    void CreatePlayer()
    {
        // Create player at a specific position
        Vector3 playerPosition = new Vector3(0, 10, constantZPosition);
        Vector3 playerScale = Vector3.one;
        Quaternion playerRotation = Quaternion.identity;
        Vector3 newPos = playerPosition;
        newPos.y += playerVelocity.y * Time.deltaTime;

        playerVelocity.x *= 0.9f; // horizontal friction
        if (isGrounded && Mathf.Abs(playerVelocity.y) < 0.1f) playerVelocity.y = 0f;

        // Register with collision system - properly handle width/height/depth
        playerID = CollisionManager.Instance.RegisterCollider(
            playerPosition,
            new Vector3(width * playerScale.x, height * playerScale.y, depth * playerScale.z),
            true);

        // Create transformation matrix
        Matrix4x4 playerMatrix = Matrix4x4.TRS(playerPosition, playerRotation, playerScale);
        matrices.Add(playerMatrix);
        colliderIds.Add(playerID);

        // Update the matrix in collision manager
        CollisionManager.Instance.UpdateMatrix(playerID, playerMatrix);


    }
    //=========================================================================================================================
    // Collisions
    //=========================================================================================================================    
    bool CheckCollisionAt(int id, Vector3 position)
    {
        return CollisionManager.Instance.CheckCollision(id, position, out _);
    }
    //=========================================================================================================================
    // Rendering
    //=========================================================================================================================    
    void RenderBoxes()
    {
        // Convert list to array for Graphics.DrawMeshInstanced
        Matrix4x4[] matrixArray = matrices.ToArray();

        // Draw instanced meshes in batches of 1023 (GPU limit)
        for (int i = 0; i < matrixArray.Length; i += 1023)
        {
            int batchSize = Mathf.Min(1023, matrixArray.Length - i);
            Matrix4x4[] batchMatrices = new Matrix4x4[batchSize];
            System.Array.Copy(matrixArray, i, batchMatrices, 0, batchSize);
            Graphics.DrawMeshInstanced(cubeMesh, 0, material, batchMatrices, batchSize);
        }
    }

    void DecomposeMatrix(Matrix4x4 matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        position = matrix.GetPosition();
        rotation = matrix.rotation;
        scale = matrix.lossyScale;
    }

    //=========================================================================================================================
    // Obstacles
    //=========================================================================================================================   

    // Add a new random box at runtime (can be called from button or other trigger)
    public void AddRandomBox()
    {
        Vector3 position = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            constantZPosition
        );

        Debug.Log($"Adding box at position: {position}");

        Quaternion rotation = Quaternion.identity;

        // Random non-uniform scale - different for each dimension
        Vector3 scale = new Vector3(
            Random.Range(0.5f, 3f),
            Random.Range(0.5f, 3f),
            Random.Range(0.5f, 3f)
        );

        // Register with collision system - properly handle rectangular shapes
        int id = CollisionManager.Instance.RegisterCollider(
            position,
            new Vector3(width * scale.x, height * scale.y, depth * scale.z),
            false);

        Matrix4x4 boxMatrix = Matrix4x4.TRS(position, rotation, scale);
        matrices.Add(boxMatrix);
        colliderIds.Add(id);

        CollisionManager.Instance.UpdateMatrix(id, boxMatrix);
    }

    //=========================================================================================================================
    // Enemies 
    //=========================================================================================================================

    public void AddEnemy()
    {
        Vector3 position = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            constantZPosition
        );

        Quaternion rotation = Quaternion.identity;
        Vector3 scale = new Vector3(1f, 1f, 1f);

        int id = CollisionManager.Instance.RegisterCollider(
            position,
            new Vector3(width * scale.x, height * scale.y, depth * scale.z),
            false
        );

        Matrix4x4 enemyMatrix = Matrix4x4.TRS(position, rotation, scale);
        matrices.Add(enemyMatrix);
        colliderIds.Add(id);
        enemyIDs.Add(id);

        // Initialize vertical velocity
        enemyVelocities[id] = Vector3.zero;

        CollisionManager.Instance.UpdateMatrix(id, enemyMatrix);
    }
    void UpdateEnemies()
    {
        foreach (int id in enemyIDs)
        {
            int index = colliderIds.IndexOf(id);
            if (index < 0) continue;

            Matrix4x4 mat = matrices[index];
            Vector3 pos = mat.GetPosition();
            Vector3 scale = mat.lossyScale;
            Quaternion rot = mat.rotation;

            // Horizontal left-right movement
            pos.x += Mathf.Sin(Time.time + id) * enemySpeed * Time.deltaTime;

            // Gravity
            Vector3 velocity = enemyVelocities[id];
            velocity.y -= 9.8f * Time.deltaTime;
            Vector3 newPos = pos + velocity * Time.deltaTime;

            // Collision with ground
            if (CollisionManager.Instance.CheckCollision(id, newPos, out _))
            {
                velocity.y = 0;
                newPos.y = pos.y;
            }

            pos = newPos;
            enemyVelocities[id] = velocity;

            // Update matrix and collider
            matrices[index] = Matrix4x4.TRS(pos, rot, scale);
            CollisionManager.Instance.UpdateMatrix(id, matrices[index]);
            CollisionManager.Instance.UpdateCollider(id, pos, scale);

            // --- Player collision check ---
            if (playerID != -1)
            {
                Vector3 playerPos = matrices[colliderIds.IndexOf(playerID)].GetPosition();
                Vector3 playerScale = matrices[colliderIds.IndexOf(playerID)].lossyScale;

                if (IsCollidingWithPlayer(id))
                {
                    // Damage the player
                    playerHealth--;
                    Debug.Log("Player hit! Health: " + playerHealth);

                    // Knockback (bounce back)
                    Vector3 direction = (playerPos - pos).normalized; // push away from enemy
                    Vector3 playerVelocity = direction * playerBounceBack;
                    playerVelocity.y = 5f; // slight upward bounce

                    ApplyPlayerKnockback(playerVelocity);

                    hitStunTimer = hitStunDuration;
                }
            }
        }
    }
    void ApplyPlayerKnockback(Vector3 velocity)
    {
        if (playerID == -1) return;

        int index = colliderIds.IndexOf(playerID);
        Matrix4x4 playerMatrix = matrices[index];
        Vector3 pos = playerMatrix.GetPosition();
        Quaternion rot = playerMatrix.rotation;
        Vector3 scale = playerMatrix.lossyScale;
        playerVelocity.x = velocity.x;
        playerVelocity.y = velocity.y;

        // Apply knockback directly
        pos += velocity * Time.deltaTime;

        matrices[index] = Matrix4x4.TRS(pos, rot, scale);
        CollisionManager.Instance.UpdateMatrix(playerID, matrices[index]);
        CollisionManager.Instance.UpdateCollider(playerID, pos, scale);
    }
    bool IsCollidingWithPlayer(int enemyID)
    {
        if (playerID == -1) return false;

        int enemyIndex = colliderIds.IndexOf(enemyID);
        int playerIndex = colliderIds.IndexOf(playerID);

        if (enemyIndex < 0 || playerIndex < 0) return false;

        Vector3 enemyPos = matrices[enemyIndex].GetPosition();
        Vector3 enemyScale = matrices[enemyIndex].lossyScale;

        Vector3 playerPos = matrices[playerIndex].GetPosition();
        Vector3 playerScale = matrices[playerIndex].lossyScale;

        // AABB collision check
        bool overlapX = Mathf.Abs(enemyPos.x - playerPos.x) < (enemyScale.x + playerScale.x) / 2f;
        bool overlapY = Mathf.Abs(enemyPos.y - playerPos.y) < (enemyScale.y + playerScale.y) / 2f;
        bool overlapZ = Mathf.Abs(enemyPos.z - playerPos.z) < (enemyScale.z + playerScale.z) / 2f;

        return overlapX && overlapY && overlapZ;
    }

    //=========================================================================================================================
    // Ending Objective
    //=========================================================================================================================
    void CreateEndZone()
    {
        Quaternion rotation = Quaternion.identity;

        // Use endZoneSize directly for both collider and rendering
        Vector3 colliderSize = endZoneSize;

        // Register as trigger
        endZoneID = CollisionManager.Instance.RegisterCollider(
            endZonePosition,
            colliderSize,
            true // trigger
        );

        // Use same size for the matrix
        Matrix4x4 matrix = Matrix4x4.TRS(endZonePosition, rotation, colliderSize);
        matrices.Add(matrix);
        colliderIds.Add(endZoneID);

        CollisionManager.Instance.UpdateMatrix(endZoneID, matrix);
    }

    void CheckEndZone()
    {
        if (playerID == -1 || endZoneID == -1) return;

        // Get player's position and collider size
        int playerIndex = colliderIds.IndexOf(playerID);
        Vector3 playerPos = matrices[playerIndex].GetPosition();
        Vector3 playerSize = new Vector3(width, height, depth);


        // Use your CheckOverlap function
        if (CollisionManager.Instance.CheckOverlap(endZoneID, playerPos, playerSize))
        {
            Debug.Log("🎉 Player reached the end zone!");
            EndGame();
        }
    }
    void EndGame()
    {
        Debug.Log("🎉 Level Complete!");

        // Stop time
        Time.timeScale = 0f;

        // Optional: disable player control
        enabled = false;
    }



    //=========================================================================================================================
    // Debugging Gizmos to visualize spawn area and ground
    //=========================================================================================================================
    void OnDrawGizmos()
    {
        if (showGround) DrawGroundMeshGizmos();
        if (showSpawnRange) DrawSpawnRange();
        if (showBoxes) DrawAllBoxesMeshGizmos();
        if (endZoneID != -1) DrawEndZoneGizmos();
    }
    void DrawGroundMeshGizmos()
    {
        if (matrices.Count == 0) return;

        // Ground is created second: index 1
        Gizmos.color = Color.green;

        if (matrices.Count > 1)
        {
            Gizmos.matrix = matrices[1]; // TRS of ground
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one); // Cube at origin, scaled/rotated by matrix
            Gizmos.matrix = Matrix4x4.identity; // reset
        }
    }
    void DrawAllBoxesMeshGizmos()
    {
        Gizmos.color = Color.cyan; // color for all cubes

        foreach (var matrix in matrices)
        {
            Gizmos.matrix = matrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        Gizmos.matrix = Matrix4x4.identity; // reset
    }
    void DrawSpawnRange()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = new Vector3(
            (minX + maxX) / 2f,
            (minY + maxY) / 2f,
            constantZPosition
        );

        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);

        Gizmos.DrawWireCube(center, size);


    }
    void DrawEndZoneGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(endZonePosition, new Vector3(width * endZoneSize.x, height * endZoneSize.y, depth * endZoneSize.z));
    }

}


