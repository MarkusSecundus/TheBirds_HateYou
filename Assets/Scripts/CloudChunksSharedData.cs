using MarkusSecundus.Utils.Procgen.Chunking;
using MarkusSecundus.Utils.Procgen.Noise;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudChunksSharedData : MonoBehaviour
{
    [SerializeField] OctaveOpenSimplexNoise.Config _noise = OctaveOpenSimplexNoise.Config.Default;
    public OctaveOpenSimplexNoise Noise { get; private set; }

    ChunkSystem _chunkSystem;
    // Start is called before the first frame update
    void Awake()
    {
        _chunkSystem = GetComponent<ChunkSystem>();
        Noise = new OctaveOpenSimplexNoise(_chunkSystem.Rand, _noise);
    }
}
