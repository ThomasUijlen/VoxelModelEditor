using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace VoxelPlugin {
public partial class Chunk
{
    private static ConcurrentDictionary<Vector3I, SuggestionLib> chunkSuggestions = new ConcurrentDictionary<Vector3I, SuggestionLib>();

    public static void SuggestChange(Chunk chunk, Vector3 position, BlockType blockType, int priority = 0) {
        if(chunk != null) {
            Chunk changedChunk = Chunk.GetChunk(position);
            if(chunk == changedChunk) {
                Chunk.SetBlock(position, blockType, priority);
                return;
            }
        }

        Vector3I chunkCoord = Chunk.PositionToChunkCoord(position);

        Suggestion suggestion = new Suggestion(position, blockType, priority);
        SuggestionLib lib = chunkSuggestions.AddOrUpdate(chunkCoord, new SuggestionLib(), (c, s) => s);
        lib.suggestions.Enqueue(suggestion);
    }

    public void ProcessSuggestions() {
        if(generating) return;
        Vector3I chunkCoord = Chunk.PositionToChunkCoord(position);
        if(!chunkSuggestions.ContainsKey(chunkCoord)) return;

        SuggestionLib suggestionLib = chunkSuggestions[chunkCoord];
        if(suggestionLib.suggestions.Count == 0) return;

        automaticUpdating = false;

        int tries = 10000;
        while(suggestionLib.suggestions.Count > 0 && tries > 0) {
            tries -= 1;
            
            Suggestion suggestion;
            if(suggestionLib.suggestions.TryDequeue(out suggestion)) {
                SetBlock(suggestion.position, suggestion.change, suggestion.priority);
            }
        }

        automaticUpdating = true;
        Update(false);
        UpdateSurroundingChunks();
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
