namespace CYM.AStar2D
{
    /// <summary>
    /// Helper class used to describe a grid index including an X and Y position.
    /// Used by all pathfinding methods to describe location in the search space.
    /// </summary>
    public struct Index
    {
        // Public
        /// <summary>
        /// Represents an index instance that is pre-initialized to 0, 0.
        /// </summary>
        public static readonly Index Zero = new Index(0, 0);

        /// <summary>
        /// Represents an invalid index equal to -1, -1.
        /// </summary>
        public static readonly Index Invalid = new Index(-1, -1);

        // Properties
        /// <summary>
        /// The X component of the index.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        ///  The Y component of the index.
        /// </summary>
        public int Y { get; private set; }

        // Constructor
        /// <summary>
        /// Parameter constructor accepting short values.
        /// </summary>
        /// <param name="x">The X component as a short</param>
        /// <param name="y">The Y component as a short</param>
        public Index(short x, short y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Parameter constructor accepting integer values.
        /// </summary>
        /// <param name="x">The X component as an integer</param>
        /// <param name="y">The Y component as an integer</param>
        public Index(int x, int y)
        {
            this.X = (short)x;
            this.Y = (short)y;
        }

        /// <summary>
        /// Parameter constructor accepting another instance.
        /// </summary>
        /// <param name="other">The <see cref="Index"/> instance to copy</param>
        public Index(Index other)
        {
            this.X = other.X;
            this.Y = other.Y;
        }

        // Methods
        /// <summary>
        /// Overriden equals method.
        /// </summary>
        /// <param name="obj">The object to compare against</param>
        /// <returns>True if the specified object is equal to this object</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if ((obj is Index) == false)
                return false;

            Index index = (Index)obj;

            if (this.X == index.X &&
                this.Y == index.Y)
                return true;

            return false;
        }

        /// <summary>
        /// Overriden hash code method.
        /// </summary>
        /// <returns>The hash code for this <see cref="Index"/></returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }

        /// <summary>
        /// Overriden to string method.
        /// </summary>
        /// <returns>This <see cref="Index"/> represented as a string value</returns>
        public override string ToString()
        {
            return string.Format("Index = ({0}, {1})", X, Y);
        }

        /// <summary>
        /// Addition operator.
        /// </summary>
        /// <param name="a">The first <see cref="Index"/></param>
        /// <param name="b">The second <see cref="Index"/></param>
        /// <returns>The resulting <see cref="Index"/></returns>
        public static Index operator +(Index a, Index b)
        {
            // Add index values
            return new Index(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// Subtraction operator.
        /// </summary>
        /// <param name="a">The first <see cref="Index"/></param>
        /// <param name="b">The second <see cref="Index"/></param>
        /// <returns>The resulting <see cref="Index"/></returns>
        public static Index operator -(Index a, Index b)
        {
            // Subtract index values
            return new Index(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// Equals operator.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Index a, Index b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Not equal operator.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Index a, Index b)
        {
            return !a.Equals(b);
        }
    }
}
