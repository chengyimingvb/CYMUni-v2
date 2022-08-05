//------------------------------------------------------------------------------
// BaseWeightPainter2D.cs
// Copyright 2020 2020/1/17 
// Created by CYM on 2020/1/17
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using UnityEngine;
namespace CYM.AStar2D
{
    [HideMonoScript]
    public class WeightPainter : MonoBehaviour
    {
        // Private
        private bool isPainting = false;
        private bool isErasing = false;

        // Public
        /// <summary>
        /// The gameobject to use as the brush.
        /// </summary>
        public GameObject brushObject;
        /// <summary>
        /// The maximum radius that the brush can use.
        /// </summary>
        public float maxBrushRadius = 2;
        /// <summary>
        /// The maximum strenght value that the brush can use.
        /// </summary>
        public float maxBrushStrength = 2;
        /// <summary>
        /// Should nodes further from the center of the brush be affected less than closer nodes.
        /// </summary>
        public bool linearFalloff = true;
        /// <summary>
        /// The initial size of the brush.
        /// </summary>
        public float brushSize = 0.5f;
        /// <summary>
        /// The initial strength of the brush.
        /// </summary>
        public float brushStrength = 0.5f;

        // Properties
        /// <summary>
        /// Check whether we are currently painting weights.
        /// </summary>
        public bool IsPainting
        {
            get { return isPainting == true; }
        }

        /// <summary>
        /// Get the actual brush strength value.
        /// </summary>
        public float CurrentBrushStrength
        {
            get { return maxBrushStrength * brushStrength; }
        }

        /// <summary>
        /// Get the actual brush size value.
        /// </summary>
        public float CurrentBrushSize
        {
            get { return maxBrushRadius * brushSize; }
        }

        /// <summary>
        /// Get the current active camera in the scene.
        /// </summary>
        public Camera Cam => BaseGlobal.MainCamera;

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Sync or brush values
            SetBrushSize(brushSize);
            SetBrushStrength(brushStrength);

            // Hide the brush object
            EndPainting();
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Update()
        {
            // Check if we are drawing
            if (isPainting == true)
            {
                // Get the mouse position
                Vector2 mousePos = Input.mousePosition;

                // Project into world space
                Vector2 mouseWorldPos = Cam.ScreenToWorldPoint(mousePos);

                // Move brush around
                if(brushObject)
                    brushObject.transform.position = mouseWorldPos;

                // Check for mouse down
                if (Input.GetMouseButton(0) == true)
                {
                    // Apply weighting to all BaseNode2Ds
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(mouseWorldPos, CurrentBrushSize);

                    // Update each collider
                    foreach (Collider2D collider in colliders)
                    {
                        // Try to get the BaseNode2D component
                        Node node = collider.GetComponent<Node>();

                        // Check for error
                        if (node == null)
                            continue;

                        // Calculate the apply amount
                        float applyAmount = CurrentBrushStrength * Time.deltaTime;

                        // Apply the weighting
                        if (linearFalloff == true)
                        {
                            // Find the distance from center
                            float distance = Vector2.Distance(node.Pos, mouseWorldPos);

                            // Create a fade effect from center
                            float falloffStrength = Mathf.InverseLerp(CurrentBrushSize, 0, distance);

                            // include the falloff strength
                            applyAmount *= falloffStrength;
                        }

                        // Check for erasing
                        if (isErasing == true)
                            applyAmount = -applyAmount;

                        // Add the amount
                        node.Weighting += applyAmount;
                        node.UpdateTileColor();
                    }
                }
            }
        }

        /// <summary>
        /// Trigger the painter to enter painting mode where weighting values can be increased by painting.
        /// </summary>
        public void StartPainting()
        {
            // Enable the object
            if (brushObject)
                brushObject?.SetActive(true);

            // Set the drawing flag
            isPainting = true;
            isErasing = false;
        }

        /// <summary>
        /// Trigger the painter to enter erase mode where weighting values can be decreased by painting.
        /// </summary>
        public void StartErasing()
        {
            // Enable the object
            if (brushObject)
                brushObject?.SetActive(true);

            // Set the drawing flag
            isPainting = true;
            isErasing = true;
        }

        /// <summary>
        /// Trigger the painter to exit painting mode.
        /// </summary>
        public void EndPainting()
        {
            // Disable the object
            if(brushObject)
                brushObject?.SetActive(false);

            // Set the drawing flag
            isPainting = false;
        }

        /// <summary>
        /// Set the target brush size to use. 
        /// The size will affect how large the painting brush is.
        /// </summary>
        /// <param name="value">The normalized brush size to use</param>
        public void SetBrushSize(float value)
        {
            brushSize = value;

            // Calcualte the scale of the brush
            float scale = (maxBrushRadius * 3.2f) * brushSize;

            // Scale the brush
            if(brushObject)
                brushObject.transform.localScale = new Vector3(scale, scale, 1);
        }

        /// <summary>
        /// Set the target brush strength to use.
        /// This will determine how much the weighting values are influenced during painting and erasing.
        /// </summary>
        /// <param name="value">The normalized brush strength to use</param>
        public void SetBrushStrength(float value)
        {
            brushStrength = value;
        }
    }
}