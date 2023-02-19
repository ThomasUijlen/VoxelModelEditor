using Godot;
using System;
using System.Collections.Concurrent;

namespace VoxelPlugin {
public partial class ThreadPool : Node
{
    public int THREAD_COUNT = 5;
    private PoolThread[] threadPool;
    private ConcurrentQueue<FunctionRequest> functionQueue = new ConcurrentQueue<FunctionRequest>();

    private bool poolActive = true;

    public override void _Ready()
    {
        threadPool = new PoolThread[THREAD_COUNT];

        for(int i = 0; i < threadPool.Length; i++) {
            threadPool[i] = new PoolThread();
            PoolThread poolThread = threadPool[i];
            poolThread.pool = this;
            int threadI = i;
            Action action = () => {ThreadFunction(threadI);};
            poolThread.thread.Start(Callable.From(action), GodotThread.Priority.Normal);
        }
    }

    public override void _Process(double delta)
    {
        if(functionQueue.Count == 0) return;

        for(int i = 0; i < threadPool.Length; i++) {
            if(functionQueue.Count == 0) break;
            PoolThread poolThread = threadPool[i];

            if(!poolThread.thread.IsAlive()) {
                poolThread.thread.WaitToFinish();
                int threadI = i;
                Action action = () => {ThreadFunction(threadI);};
                poolThread.thread.Start(Callable.From(action), GodotThread.Priority.Normal);
            }
            
            if(!poolThread.active) {
                FunctionRequest functionRequest = null;
                if(functionQueue.TryDequeue(out functionRequest)) poolThread.CallFunction(functionRequest);
            }
        }
    }

    public bool ThreadFree() {
        int freeThreads = 0;
        for(int i = 0; i < threadPool.Length; i++) {
            if(!threadPool[i].active) freeThreads += 1;
        }

        if(freeThreads > functionQueue.Count) return true;

        return false;
    }

    public bool ThreadsActive() {
        for(int i = 0; i < threadPool.Length; i++) {
            if(threadPool[i].active) return true;
        }
        return false;
    }

    public FunctionRequest RequestFunctionCall(Node node, string functionName) {
        FunctionRequest functionRequest = new FunctionRequest(node, functionName);
        functionRequest.pool = this;
        functionQueue.Enqueue(functionRequest);
        return functionRequest;
    }

    public FunctionRequest RequestFunctionCall(Node node, string functionName, Godot.Collections.Array parameters) {
        FunctionRequest functionRequest = new FunctionRequest(node, functionName, parameters);
        functionRequest.pool = this;
        functionQueue.Enqueue(functionRequest);
        return functionRequest;
    }

    private void ThreadFunction(int i) {
        PoolThread poolThread = threadPool[i];

        while(poolActive) {
            poolThread.semaphore.Wait();
            if(!poolActive) return;
            if(poolThread.functionRequest.node != null && IsInstanceValid(poolThread.functionRequest.node)) {
                if(poolThread.functionRequest.parameters != null) {
                    poolThread.functionRequest.node.Callv(
                        poolThread.functionRequest.functionName,
                        poolThread.functionRequest.parameters);
                } else {
                    poolThread.functionRequest.node.Call(
                        poolThread.functionRequest.functionName);
                }
                
            }
            poolThread.CallDeferred("FunctionFinished");
        }
    }

    public override void _ExitTree()
    {
        poolActive = false;
        for(int i = 0; i < threadPool.Length; i++) {
            PoolThread poolThread = threadPool[i];
            poolThread.semaphore.Post();
        }
    }

    private partial class PoolThread : Resource {
        public ThreadPool pool;
        public GodotThread thread;
        public Semaphore semaphore;
        public bool active;
        public FunctionRequest functionRequest;

        public PoolThread() {
            thread = new GodotThread();
            semaphore = new Semaphore();
            active = false;
        }

        public void CallFunction(FunctionRequest functionRequest) {
            this.functionRequest = functionRequest;
            functionRequest.processed = true;
            active = true;
            semaphore.Post();
        }

        public void FunctionFinished() {
            active = false;
            functionRequest = null;
        }
    }

    public class FunctionRequest {
        public Node node;
        public String functionName;
        public Godot.Collections.Array parameters;
        public bool processed = false;
        public ThreadPool pool;

        public FunctionRequest(Node node, String functionName, Godot.Collections.Array parameters) : this(node, functionName) {
            this.parameters = parameters;
        }

        public FunctionRequest(Node node, String functionName) {
            this.node = node;
            this.functionName = functionName;
        }
    }
}
}