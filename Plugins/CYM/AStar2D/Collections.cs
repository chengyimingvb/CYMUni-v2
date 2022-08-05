using System.Collections.Generic;

namespace CYM.AStar2D
{
    internal sealed class OpenNodeMap<T> 
        where T : class, IBaseNode
    {
        // Private
        private IBaseNode[,] nodeMap = null;
        private int width = 0;
        private int height = 0;
        private int count = 0;

        // Properties
        public T this[int x, int y] => nodeMap[x, y] as T;

        public T this[T node] => nodeMap[node.Index.X, node.Index.Y] as T;
        public int Width => width;
        public int Height => height;

        public int Count => count;
        // Constructor
        public OpenNodeMap(int width, int height)
        {
            // Store values
            this.width = width;
            this.height = height;

            // Create the map
            nodeMap = new IBaseNode[width, height];
        }

        // Methods
        public bool contains(T value)
        {
            // Get the item at the index
            IBaseNode item = nodeMap[value.Index.X, value.Index.Y];

            if (item == null)
                return false;

            if (value.Equals(item) == false)
                return false;

            return true;
        }

        public void add(T value)
        {
            // Get the item at the index
            //IPathNode item = nodeMap[value.Index.X, value.Index.Y];

            // Update the size and value
            count++;
            nodeMap[value.Index.X, value.Index.Y] = value;
        }

        public void remove(T value)
        {
            // Get the item at the index
            //IPathNode item = nodeMap[value.Index.X, value.Index.Y];

            // Update the size and value
            count--;
            nodeMap[value.Index.X, value.Index.Y] = null;
        }

        public void clear()
        {
            // Reset all values to null
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    nodeMap[x, y] = null;

            // Reset the count
            count = 0;
        }
    }

    internal sealed class NodeQueue<T>
        where T : IBaseNode
    {
        // Private
        private List<T> collection = new List<T>();
        private IComparer<T> comparer;

        // Properties
        public int Count => collection.Count;
        // Constructor
        public NodeQueue(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }

        // Methods
        public int Push(T value)
        {
            int size = Count;
            int length = 0;

            // Update the index
            value.TileNodeIndex = Count;

            // Add to the collection
            collection.Add(value);

            do
            {
                // Check for empty
                if (size == 0)
                    break;

                // Calcualte the length
                length = (size - 1) / 2;

                // Compare the elements
                if (CompareElement(size, length) < 0)
                {
                    // Swap the elements to sort
                    SwapElement(size, length);
                    size = length;
                }
                else
                {
                    // Exit the loop
                    break;
                }
            }
            while (true);

            return size;
        }

        public T Pop()
        {
            // Get the item at the front of the list
            T value = collection[0];

            int size = 0;
            int index0 = 0;
            int index1 = 0;
            int length = 0;

            // Move the last element to the front
            collection[0] = collection[Count - 1];
            collection[0].TileNodeIndex = 0;

            // Remove the last item
            collection.RemoveAt(Count - 1);

            // Update the value index
            value.TileNodeIndex = -1;

            do
            {
                length = size;
                index0 = 2 * size + 1;
                index1 = 2 * size + 2;

                // Check if index 0 is better
                if (Count > index0 && CompareElement(size, index0) > 0)
                    size = index0;

                // CHeck if index 1 is better
                if (Count > index1 && CompareElement(size, index1) > 0)
                    size = index1;

                // No improvement was found
                if (size == length)
                    break;

                // Swap the elements so that the priority is lower
                SwapElement(size, length);
            }
            while (true);

            return value;
        }

        public T Peek()
        {
            // Make sure there is atleast 1 item in the collection
            if (Count > 0)
                return collection[0];

            // Return an error value (null usually)
            return default(T);
        }

        public void SwapElement(int x, int y)
        {
            // Get the value and store it in a temp value
            T value = collection[x];

            // Switch the values
            collection[x] = collection[y];
            collection[y] = value;

            // Update the priority indexes
            collection[x].TileNodeIndex = x;
            collection[y].TileNodeIndex = y;
        }

        public int CompareElement(int x, int y)
        {
            // Use the comparer to check the values
            return comparer.Compare(collection[x], collection[y]);
        }

        public void Refresh(T value)
        {
            while ((value.TileNodeIndex - 1 >= 0) && (CompareElement(value.TileNodeIndex - 1, value.TileNodeIndex) > 0))
                SwapElement(value.TileNodeIndex - 1, value.TileNodeIndex);

            while ((value.TileNodeIndex + 1 < Count) && (CompareElement(value.TileNodeIndex + 1, value.TileNodeIndex) < 0))
                SwapElement(value.TileNodeIndex + 1, value.TileNodeIndex);
        }

        public void Clear()
        {
            collection.Clear();
        }
    }
}
