using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace VoxelPlugin {
public partial class Chunk
{
    private static ConcurrentDictionary<Vector3I, SuggestionLib> chunkSuggestions = new ConcurrentDictionary<Vector3I, SuggestionLib>();

    public void SuggestChange(Vector3 position, BlockType blockType, int priority = 0) {
        Vector3I chunkCoord = Chunk.PositionToChunkCoord(position);

        if(Chunk.SetBlock(position, blockType, priority)) return;

        Suggestion suggestion = new Suggestion(position, blockType, priority);
        SuggestionLib lib = chunkSuggestions.AddOrUpdate(chunkCoord, new SuggestionLib(), (c, s) => s);
        lib.suggestions.Enqueue(suggestion);
    }

    public void ProcessSuggestions() {
        Vector3I chunkCoord = Chunk.PositionToChunkCoord(position);
        if(!chunkSuggestions.ContainsKey(chunkCoord)) return;

        SuggestionLib suggestionLib = chunkSuggestions[chunkCoord];
        while(suggestionLib.suggestions.Count > 0) {
            Suggestion suggestion;
            if(suggestionLib.suggestions.TryDequeue(out suggestion)) {
                SetBlock(suggestion.position, suggestion.change, suggestion.priority);
            }
        }
    }
}

public class SuggestionLib {
    public ConcurrentQueue<Suggestion> suggestions = new ConcurrentQueue<Suggestion>();
}

public class Suggestion {
    public int priority = 0;
    public BlockType change;
    public Vector3 position;

    public Suggestion(Vector3 position, BlockType change, int priority) {
        this.position = position;
        this.priority = priority;
        this.change = change;
    }
}
}
