// using System.Collections.Generic;
// using UnityEngine;

// public class GameManager : MonoBehaviour
// {
//     [Header("References")]
//     public Material material;
//     public Material fireballMaterial;

//     [Header("Settings")]
//     public float width = 1f;
//     public float height = 1f;
//     public float depth = 1f;

//     public float movementSpeed = 5f;
//     public float fireballSpeed = 10f;

//     // Core data
//     private List<Matrix4x4> matrices = new List<Matrix4x4>();
//     private List<int> colliderIds = new List<int>();

//     // Systems



//     // Player
//     private int playerID = -1;
//     private Vector3 playerVelocity;
//     private Vector3 facingDirection = Vector3.right;
//     private bool isGrounded;

//     // Fireball
//     private bool hasFireball = true;
//     private float fireCooldown = 0.3f;
//     private float fireTimer = 0f;

//     // Enemies
//     private List<int> enemyIDs = new List<int>();

//     void Start()
//     {
//         CreatePlayer();

//         // Initialize systems
//         fireballSystem = new FireballSystem(matrices, fireballSpeed, 50f);
//     }

//     void Update()
//     {
//         UpdateTimers();

//         UpdatePlayer();

//         fireballSystem.Update(colliderIds, enemyIDs);

//         Render();
//     }


//     void UpdateTimers()
//     {
//         fireTimer = Mathf.Max(0f, fireTimer - Time.deltaTime);
//     }





// }