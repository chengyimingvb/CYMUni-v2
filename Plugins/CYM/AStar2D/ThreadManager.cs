using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CYM.AStar2D
{
    [HideMonoScript]
    public sealed class ThreadManager : MonoBehaviour, IEnumerable<WorkerThread>
    {
        // Private
        private static readonly float threadSpawnThreshold = 0.6f;
        private static readonly int minWorkerThreads = 1;
        private static ThreadManager manager = null;
        private List<WorkerThread> threads = new List<WorkerThread>();

        // Public
        public static readonly int maxAllowedWorkerThreads = 3;
        [Range(0, 16)]
        public int maxWorkerThreads = 16;
        public int maxIdleFrames = 240;

        #region life
        private void Update()
        {
            // Make sure there is always atleast 1 thread
            if (maxWorkerThreads <= 0)
                maxWorkerThreads = minWorkerThreads;

            // Process messages for this frame
            foreach (WorkerThread thread in threads)
                thread.ProcessMessageQueue();
            terminateIdleWorkers();
        }
        private void OnDestroy()
        {
            DoDestroy();
        }
        public int ActiveThreads
        {
            get { return threads.Count; }
        }
        #endregion

        #region static
        // Properties
        public static ThreadManager Active
        {
            get
            {
                // Launch the manager
                LaunchIfRequired();

                return manager;
            }
        }
        // Methods
        public static void LaunchIfRequired()
        {
            // CHeck for valid manager
            if (manager != null)
                return;

            // Check for any other instances
            ThreadManager externalManager = Component.FindObjectOfType<ThreadManager>();

            // Chekc for any found managers
            if (externalManager == null)
            {
                // Create a parent object
                GameObject go = new GameObject("AStar2D-ThreadManager");
                // Add the component
                manager = go.AddComponent<ThreadManager>();

                // Dont destroy the object
                //DontDestroyOnLoad(go);
            }
            else
            {
                // Store a reference
                manager = externalManager;
            }
        }
        #endregion

        #region set
        public void DoDestroy()
        {
            // Process each thread
            foreach (WorkerThread thread in threads)
            {
                // Dispatch each message immediatley
                while (thread.IsMessageQueueEmpty == false)
                    thread.ProcessMessageQueue();

                // Terminate the thread
                thread.EndOrAbort();
            }

            // Clear the list
            threads.Clear();

            // Reset the reference
            manager = null;
        }
        public void asyncRequest(AsyncPathRequest request)
        {
            // Get the worker
            WorkerThread thread = FindSuitableWorker();

            // Push the request
            thread.AsyncRequest(request);
        }
        public bool HasThread(WorkerThread thread)
        {
            return threads.Contains(thread);
        }
        #endregion

        #region get
        public int GetThreadID(WorkerThread thread)
        {
            return threads.IndexOf(thread);
        }
        public IEnumerator<WorkerThread> GetEnumerator()
        {
            return threads.GetEnumerator();
        }
        #endregion

        #region private
        private WorkerThread spawnThread()
        {
            // Create a new worker
            WorkerThread thread = new WorkerThread(threads.Count);

            // Register
            threads.Add(thread);

            // Begin
            thread.Launch();

            return thread;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return threads.GetEnumerator();
        }
        private IEnumerator threadTerminateRoutine(WorkerThread thread)
        {
            // Make sure all messages are dispatched before killing the thread
            while (thread.IsMessageQueueEmpty == false)
            {
                // Process thr threads messages
                thread.ProcessMessageQueue();

                // Wait for next frame
                yield return null;
            }

            // We can now kill the thread
            thread.EndOrAbort();
        }
        private void terminateIdleWorkers()
        {
            int totalThreads = threads.Count;

            // We cant termainte the remaining threads
            if (totalThreads <= minWorkerThreads)
                return;

            // Process the list of threads
            for (int i = 0; i < threads.Count; i++)
            {
                // Check if a thread is idleing
                if (threads[i].ThreadLoad == 0)
                {
                    if (threads[i].IdleFrames > maxIdleFrames)
                    {
                        // Triger routine
                        StartCoroutine(threadTerminateRoutine(threads[i]));

                        // Remove from list
                        threads.RemoveAt(i);
                    }
                }
            }
        }
        private WorkerThread FindSuitableWorker()
        {
            // Make sure there is a worker to handle the request
            if (threads.Count == 0)
                return spawnThread();

            // Try to find a suitable thread
            WorkerThread candidate = threads[0];
            float best = 1;

            foreach (WorkerThread thread in threads)
            {
                if (thread.ThreadLoad < best)
                {
                    candidate = thread;
                    best = thread.ThreadLoad;
                }
            }

            // Check for no candidate
            if (best >= threadSpawnThreshold)
            {
                // Check if we can spawn a new thread
                if (threads.Count < maxWorkerThreads)
                {
                    // Create a new worker for the request
                    return spawnThread();
                }
            }

            return candidate;
        }
        #endregion
    }

    public sealed class AsyncPathRequest
    {
        // Properties
        public SearchGrid Grid { get; private set; }

        public Index Start { get; private set; }

        public Index End { get; private set; }

        public DiagonalMode Diagonal { get; private set; }

        public BaseTraversal2D Traversal2D { get; private set; }

        internal PathRequestDelegate Callback { get; private set; }

        internal long TimeStamp { get; private set; }

        // Constructor
        public AsyncPathRequest(SearchGrid grid, Index start, Index end, PathRequestDelegate callback)
        {
            this.Grid = grid;
            this.Start = start;
            this.End = end;
            this.Callback = callback;

            // Create a time stamp
            TimeStamp = DateTime.UtcNow.Ticks;
        }

        public AsyncPathRequest(SearchGrid grid, Index start, Index end, DiagonalMode diagonal, BaseTraversal2D traversal2D, PathRequestDelegate callback)
        {
            this.Grid = grid;
            this.Start = start;
            this.End = end;
            this.Diagonal = diagonal;
            this.Callback = callback;
            this.Traversal2D = traversal2D;

            // Create a time stamp
            TimeStamp = DateTime.UtcNow.Ticks;
        }
    }

    public sealed class WorkerThread
    {
        // Private        
        private static readonly int averageRange = 8;
        private static readonly int timeout = 300;
        private static short id = 0;

        private Queue<AsyncPathRequest> incoming = new Queue<AsyncPathRequest>();
        private Queue<AsyncPathResult> outgoing = new Queue<AsyncPathResult>();
        private List<long> timing = new List<long>();

        private Thread thread = null;
        private volatile bool isRunning = false;
        private volatile bool isMessageQueueEmpty = false;
        private volatile float threadLoad = 0;
        private volatile int idleFrames = 0;
        private long averageTime = 1;
        private int workerID = 0;

        // Public
        public static readonly long targetTime = 10; // Max milliseconds per frame - anything more is considered as stress

        // Constructor
        public WorkerThread(int id)
        {
            this.workerID = id;
        }
        // Properties
        public float ThreadLoad
        {
            get { return threadLoad; }
        }
        public bool IsMessageQueueEmpty
        {
            get { return isMessageQueueEmpty; }
        }
        public int IdleFrames
        {
            get { return idleFrames; }
        }

        #region set
        // Methods
        public void Launch()
        {
            // Create the thread
            thread = new Thread(new ThreadStart(ThreadMain));

            // Initialize
            thread.IsBackground = true;
            thread.Name = string.Format("AStar_2D (Worker [{0}])", id++);

            // Start the thread
            thread.Start();
        }
        public void EndOrAbort()
        {
            // Set the running flag
            isRunning = false;

            // Check for valid thread
            if (thread == null)
                return;

            // Wait for the thread to quit
            thread.Join(timeout);

            // Check if the thread is still active
            if (thread.IsAlive == true)
            {
                // Force quit
                thread.Abort();
            }
        }
        public void AsyncRequest(AsyncPathRequest request)
        {
            // Lock the queue
            lock (incoming)
            {
                // Push the request
                incoming.Enqueue(request);
            }
        }
        /// <summary>
        /// Only call this method from the main thread.
        /// </summary>
        public void ProcessMessageQueue()
        {
            AsyncPathResult result = null;

            // Lock the output queue
            lock (outgoing)
            {
                // Update the flag
                isMessageQueueEmpty = (outgoing.Count == 0);

                // Get the result
                if (outgoing.Count > 0)
                    result = outgoing.Dequeue();
            }

            if (result != null)
            {
                // Process the result
                result.invokeCallback();
            }
        }
        private void ThreadMain()
        {
            // Set the flag
            isRunning = true;

            // Used to calcualte the average            
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            try
            {
                // Loop forever
                while (isRunning == true)
                {
                    // Get a request
                    AsyncPathRequest request = null;

                    // Lock the input queue
                    lock (incoming)
                    {
                        // Calcualte the current thread load
                        CalcualteLoad();

                        // Get a request
                        if (incoming.Count > 0)
                            request = incoming.Dequeue();
                    }

                    // Check for a request
                    if (request == null)
                    {
                        idleFrames++;

                        // Take a long sleep - no load
                        Thread.Sleep((int)targetTime);
                        continue;
                    }

                    // Reset the idle frames
                    idleFrames = 0;

                    // Begin timing
                    timer.Reset();
                    timer.Start();

                    // Lock the grid while we search
                    lock (request.Grid)
                    {
                        // Execute the request
                        request.Grid.FindPath(request.Start, request.End, request.Diagonal, request.Traversal2D, (Path path, PathRequestStatus status) =>
                        {
                            // Create a result
                            AsyncPathResult result = new AsyncPathResult(request, path, status);

                            // Push the result to the outgoing queue
                            lock (outgoing)
                            {
                                // Add result
                                outgoing.Enqueue(result);
                            }
                        });
                    }

                    // Stop timing and calculate the average time
                    timer.Stop();
                    CalculateAverageTime(timer.ElapsedMilliseconds);

                    // Calculate the amount of rest time based on the thread load
                    int sleepDuration = (int)((1 - threadLoad) * targetTime);

                    // Sleep based on the current thread load
                    Thread.Sleep(sleepDuration);
                } // End while
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }
        /// <summary>
        /// Only call while the incoming queue is locked
        /// </summary>
        private void CalcualteLoad()
        {
            // The number of waiting tasks
            int awaiting = incoming.Count;

            // Take a performance sample
#if UNITY_EDITOR
            Performance.AddUsageSample(workerID, threadLoad);
#endif

            // Get the average time per task
            long estimatedCompletionTime = averageTime * awaiting;

            // Check for excessive
            if (estimatedCompletionTime > targetTime)
            {
                // Direct assign max load value
                threadLoad = 1;
            }
            else
            {
                // Calcualte the load as a scalar
                threadLoad = estimatedCompletionTime / (float)targetTime;
            }


        }
        private void CalculateAverageTime(long addValue)
        {
#if UNITY_EDITOR
            Performance.AddTimingSample((float)addValue / targetTime);
#endif

            // Add the values
            timing.Add(addValue);

            // Check for many
            if (timing.Count > averageRange)
                timing.RemoveAt(0);

            long accumulator = 1;

            // Add each value
            foreach (long value in timing)
                accumulator += value;

            // Cache average
            averageTime = accumulator / timing.Count;
        }
        #endregion
    }
    public sealed class AsyncPathResult
    {
        // Private
        private AsyncPathRequest request = null;
        private Path result = null;
        private PathRequestStatus status = PathRequestStatus.InvalidIndex;

        // Properties
        public AsyncPathRequest Request
        {
            get { return request; }
        }

        public Path Result
        {
            get { return result; }
        }

        public PathRequestStatus Status
        {
            get { return status; }
        }

        // Constructor
        public AsyncPathResult(AsyncPathRequest request, Path result, PathRequestStatus status)
        {
            this.request = request;
            this.result = result;
            this.status = status;
        }

        // Methods
        public void invokeCallback()
        {
            // Make sure the request has been assigned
            if (request != null)
            {
                // Make sure there is a listener
                if (request.Callback != null)
                {
                    // Trigger the method
                    request.Callback(result, status);
                }
            }
        }
    }
}
