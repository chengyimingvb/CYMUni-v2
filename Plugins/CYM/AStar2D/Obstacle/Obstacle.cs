using Sirenix.OdinInspector;
using UnityEngine;

namespace CYM.AStar2D
{
    [HideMonoScript]
    /// <summary>
    /// Represents a game object that can obstruct the pathfinding grid using a <see cref="Collider2D"/> to define the bounds of the object. 
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        private Collider2D colliderObstacle = null;
        private bool isDirty = false;

        // Protected
        /// <summary>
        /// Returns true if the obstacle is enabled for obstruction.
        /// </summary>
        protected bool isObstructing = true;

        // Public
        /// <summary>
        /// The amount of extra area that the collider bounds should expand to.
        /// </summary>
        public const float colliderPadding = 0.25f;

        /// <summary>
        /// The <see cref="AStarGrid"/> that the obstacle belongs to or null if the default grid should be used. 
        /// </summary>
        [HideInInspector]
        public AStarGrid astarGrid => AStarGrid.DefaultGrid;

        // Properties
        /// <summary>
        /// Returns true if the obstacle needs to be rebuilt.
        /// </summary>
        public bool IsDirty
        {
            get { return isDirty; }
        }

        /// <summary>
        /// Returns true if this obstacle is enabled for obstruction.
        /// </summary>
        public bool IsObstructing
        {
            get
            {
                return isObstructing;
            }
            set { isObstructing = value; }
        }

        // Methods
        public virtual void Start()
        {
            if (astarGrid != null && colliderObstacle != null)
                astarGrid.RegisterObstacle(this);
            SetDirty();
        }
        private void Awake()
        {
            colliderObstacle = GetComponent<Collider2D>();
            if (astarGrid != null && colliderObstacle != null)
                astarGrid.RegisterObstacle(this);
            SetDirty();
        }
        public virtual void OnEnable()
        {
            if (astarGrid != null && colliderObstacle != null)
                astarGrid.RegisterObstacle(this);
            SetDirty();
        }
        public virtual void OnDisable()
        {
            // Unregister obstacle
            if (astarGrid != null && colliderObstacle != null)
                astarGrid.UnregisterObstacle(this);
        }

        /// <summary>
        /// Causes the dynamic obstacle to becode invalid forcing it to be updated in the search space.
        /// Note that the update may not be immediate as it must be handled by the managing <see cref="AStarGrid"/>. 
        /// </summary>
        public void SetDirty()
        {
            // Set the flag
            isDirty = true;
        }

        /// <summary>
        /// Called by the pathfinding system when this obstacle has just been updated in the search space.
        /// </summary>
        public void OnObstacleUpdated()
        {
            // Reset the flag
            isDirty = false;
        }

        /// <summary>
        /// Checks whether this obstacle is occupying the specified position.
        /// </summary>
        /// <param name="position">The world position to check</param>
        /// <returns>True if the obstacle is occupying the position of false if not</returns>
        public virtual bool IsOccupiedByObstacle(Vector3 position)
        {
            // Check for obstruction
            if (isObstructing == false ||
                colliderObstacle == null)
            {
                // We dont need to consider this obstacle for pathfinding
                return false;
            }

            // Check for point - collider collision
            return colliderObstacle.OverlapPoint(position);
        }

        /// <summary>
        /// Get the bounding box for the dynamic obstacle.
        /// This helps to reduce the performance imapct by working on a portion of the search space that the bounding box overlaps.
        /// </summary>
        /// <returns>The bounding box for the obstacles collider</returns>
        public virtual Bounds GetObstacleBounds()
        {
            // Check for collider
            if (colliderObstacle == null)
                return new Bounds();

            // Get the collider bounds
            Bounds result = colliderObstacle.bounds;

            result.min -= new Vector3(colliderPadding, colliderPadding, 0);
            result.max += new Vector3(colliderPadding, colliderPadding, 0);

            return result;
        }
    }
}
