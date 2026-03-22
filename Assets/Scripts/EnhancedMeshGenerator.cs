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


    public List<Enemy> enemies = new List<Enemy>(); 

    [Header("POwerUPs")]
    public List<Fireball> fireballs = new List<Fireball>();
    public List<Powerup> powerups = new List<Powerup>();

    public float fireballSpeed = 12f;
    public Material powerupMaterial;
    bool hasFireballPower = false;
    float fireballTimer = 0f;
    float fireballDuration = 5f; 




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
    public int playerHP = 5;

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

        // Create Enemy
        CreateEnemy(new Vector3(5, 5, constantZPosition));
        CreateEnemy(new Vector3(-10, 3, constantZPosition));
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

    void CreatePlayer()
    {
        // Create player at a specific position
        Vector3 playerPosition = new Vector3(0, 10, constantZPosition);
        Vector3 playerScale = Vector3.one;
        Quaternion playerRotation = Quaternion.identity;

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
            Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

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

    void Update()
    {
        //PLayer INput
        UpdatePlayer(); 
        UpdateEnemies();
        UpdatePowerups();
        UpdateFireballs(); 

        RenderFireballs();
        RenderPowerups();
        RenderBoxes();

        HandleFireballPower();

        //Collision Check
        // foreach(var enemy in enemies)
        // {
        //     if (CollisionManager.Instance.CheckOverlap(playerID, enemy.position, enemy.size))
        //     player.TakeDamage(1);
        // }


    }

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
        if (Input.GetKey(KeyCode.A)) horizontal -= 1;
        if (Input.GetKey(KeyCode.D)) horizontal += 1;

        // Update player position based on input
        if (!isGrounded)
        {
            horizontal *= 0.5f;
        }
        Vector3 newPos = pos;
        newPos.x += horizontal * movementSpeed * Time.deltaTime;

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
            Matrix4x4 camPlayerMatrix = matrices[colliderIds.IndexOf(playerID)];
            DecomposeMatrix(camPlayerMatrix, out Vector3 camPlayerPos, out _, out _);
            cameraFollow.SetPlayerPosition(camPlayerPos);
        }
    }


    bool CheckCollisionAt(int id, Vector3 position)
    {
        return CollisionManager.Instance.CheckCollision(id, position, out _);
    }

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

    // Add a new random box at runtime (can be called from button or other trigger)
    public void AddRandomBox()
    {
        Vector3 position = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            constantZPosition
        );

        Debug.Log($"Adding box at position: {position}");

        Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

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
    // Debugging Gizmos to visualize spawn area and ground
    //=========================================================================================================================
    void OnDrawGizmos()
    {
        if (showGround) DrawGroundMeshGizmos();
        if (showSpawnRange) DrawSpawnRange();
        if (showBoxes) DrawAllBoxesMeshGizmos();
        DrawAllPowerupsGizmos();
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

    void DrawAllPowerupsGizmos()
    {
        if (powerups == null || powerups.Count == 0) return;

        foreach (var p in powerups)
        {
            if (!p.active) continue;

            // Choose color based on type
            if (p.type == "life") Gizmos.color = Color.green;
            else if (p.type == "fireball") Gizmos.color = Color.red;
            else Gizmos.color = Color.magenta; 

            Gizmos.matrix = Matrix4x4.TRS(p.position, Quaternion.identity, Vector3.one * 0.6f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        Gizmos.matrix = Matrix4x4.identity; 
    }

    //===================================================================================================================
    //Enemy Functions
    //===================================================================================================================

    void CreateEnemy(Vector3 position)
    {
        Vector3 scale = Vector3.one;

        int id = CollisionManager.Instance.RegisterCollider(
            position,
            new Vector3(width * scale.x, height * scale.y, depth * scale.z),
            false
        );

        Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, scale);

        matrices.Add(matrix);
        colliderIds.Add(id);

        int matrixIndex = matrices.Count - 1;

        Enemy enemy = new Enemy(id, position, new Vector3(width, height, depth), 5f);
        enemy.matrixIndex = matrixIndex;

        enemies.Add(enemy);

        CollisionManager.Instance.UpdateMatrix(id, matrix);
    }

    void UpdateEnemies()
    {
        foreach (var e in enemies)
        {
            e.UpdateMovement(Time.deltaTime);
            e.position.x += e.speed * Time.deltaTime;

             if (!e.isGrounded)
            {
                e.velocity.y -= gravity * Time.deltaTime;
            }
            else
            {
                e.velocity.y = 0;
            }

            Vector3 newPos = e.position;
            newPos.y += e.velocity.y * Time.deltaTime;

            // Check vertical collision (ground/platforms)
            if (CollisionManager.Instance.CheckCollision(e.id, newPos, out _))
            {
                if (e.velocity.y < 0)
                    e.isGrounded = true;

                e.velocity.y = 0;
            }
            else
            {
                e.position.y = newPos.y;
                e.isGrounded = false;
            }

            // Update matrix
            Matrix4x4 matrix = matrices[e.matrixIndex];
            DecomposeMatrix(matrix, out _, out Quaternion rot, out Vector3 scale);

            Matrix4x4 newMatrix = Matrix4x4.TRS(e.position, rot, scale);
            matrices[e.matrixIndex] = newMatrix;

            // Update collision
            CollisionManager.Instance.UpdateCollider(e.id, e.position, e.size);
            CollisionManager.Instance.UpdateMatrix(e.id, newMatrix);

            // Damage player
            if (CollisionManager.Instance.CheckOverlap(playerID, e.position, e.size))
            {
                Debug.Log("Player Damaged");
                playerHP--;
            }
        }
    }
    //===================================================================================================================
    //Power Up
    //===================================================================================================================
    void CreatePowerup(Vector3 position, string type)
    {
        Vector3 scale = Vector3.one * 0.8f;

        int id = CollisionManager.Instance.RegisterCollider(
            position,
            new Vector3(width * scale.x, height * scale.y, depth * scale.z),
            false
        );

        Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, scale);

        matrices.Add(matrix);
        colliderIds.Add(id);

        int matrixIndex = matrices.Count - 1;

        Powerup p = new Powerup(
            id,
            position,
            new Vector3(width * scale.x, height * scale.y, depth * scale.z),
            type,
            matrixIndex
        );

        powerups.Add(p);

        CollisionManager.Instance.UpdateMatrix(id, matrix);
    }

    void UpdatePowerups()
    {
        foreach (var p in powerups)
        {
            if (!p.active) continue;

            if (CollisionManager.Instance.CheckOverlap(playerID, p.position, p.size))
            {
                ActivatePowerup(p);
            }
        }
    }

    void ActivatePowerup(Powerup p)
    {
        if (!p.active) return;

        switch (p.type)
        {
            case "life":
                playerHP++;
                break;

            case "fireball":
            hasFireballPower = true;
            fireballTimer = fireballDuration;
            break;
        }

        // Disable
        p.active = false;

        matrices[p.matrixIndex] = Matrix4x4.TRS(p.position, Quaternion.identity, Vector3.zero);
        CollisionManager.Instance.RemoveCollider(p.id);
    }

    void RenderPowerups()
    {
        if (powerups == null || cubeMesh == null || powerupMaterial == null) return;

        foreach (var p in powerups)
        {
            if (!p.active) continue; // Only draw active powerups

            // Choose color or material based on type
            Material mat = new Material(powerupMaterial); // default material
            if (p.type == "life") mat.color = Color.green;
            else if (p.type == "fireball") mat.color = Color.red;

            Matrix4x4 matrix = Matrix4x4.TRS( 
                p.position,
                Quaternion.identity,
                Vector3.one * 0.6f 
        );

        Graphics.DrawMesh(cubeMesh, matrix, mat, 0); 
        }
    }
    
    public void AddRandomPowerup()
    {
        // Random position inside your bounds
        Vector3 position = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            constantZPosition
        );

        // Randomly choose a type
        string[] types = { "life", "fireball" }; // you can add more later
        string type = types[Random.Range(0, types.Length)];

        // Call your existing CreatePowerup method
        CreatePowerup(position, type);

        Debug.Log($"Spawned powerup '{type}' at {position}");
    }
    //===================================================================================================================
    //Fireball Spawner
    //===================================================================================================================

    void SpawnFireball()
    {
        Matrix4x4 playerMatrix = matrices[colliderIds.IndexOf(playerID)];
        DecomposeMatrix(playerMatrix, out Vector3 pos, out _, out _);

        Vector3 scale = Vector3.one * 0.5f;

        Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, scale);

        matrices.Add(matrix);
        int matrixIndex = matrices.Count - 1;

        Fireball fb = new Fireball(
            pos,
            new Vector3(width * scale.x, height * scale.y, depth * scale.z),
            fireballSpeed,
            matrixIndex
        );

        fireballs.Add(fb);
    }

    void UpdateFireballs()
    {
        for (int i = 0; i < fireballs.Count; i++)
        {
            Fireball fb = fireballs[i];

            // Move forward (right)
            fb.position += Vector3.right * fb.speed * Time.deltaTime;

            // Check collision with enemies
            foreach (var e in enemies)
            {
                if (CollisionManager.Instance.CheckOverlap(e.id, fb.position, fb.size))
                {
                    // Kill enemy
                    CollisionManager.Instance.RemoveCollider(e.id);
                    matrices[e.matrixIndex] = Matrix4x4.TRS(e.position, Quaternion.identity, Vector3.zero);

                    // Remove fireball
                    matrices[fb.matrixIndex] = Matrix4x4.TRS(fb.position, Quaternion.identity, Vector3.zero);
                    fireballs.RemoveAt(i);
                    i--;
                    break;
                }
            }

            // Update fireball matrix
            if (i >= 0 && i < fireballs.Count)
            {
                Matrix4x4 newMatrix = Matrix4x4.TRS(fb.position, Quaternion.identity, Vector3.one * 0.5f);
                matrices[fb.matrixIndex] = newMatrix;
            }
        }
    }

    void RenderFireballs()
    {
        foreach (var fb in fireballs)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(fb.position, Quaternion.identity, Vector3.one * 0.5f);
            Graphics.DrawMesh(cubeMesh, matrix, powerupMaterial, 0);
        }
    }

    void HandleFireballPower()
    {
        if (!hasFireballPower) return;

        fireballTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            SpawnFireball();
        }

        if (fireballTimer <= 0f)
        {
            hasFireballPower = false;
        }

    }

    
    

}

