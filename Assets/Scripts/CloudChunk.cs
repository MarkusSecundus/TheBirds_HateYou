using MarkusSecundus.Utils.Datastructs;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Procgen.Chunking;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CloudChunk : MonoBehaviour, IChunkInitializer
{
    CloudChunksSharedData _shared;
    [SerializeField] Vector2Int _textureResolution = new Vector2Int(128, 128);

    [SerializeField] Color _colorMin = new Color(1f, 1f, 1f, 0f);
    [SerializeField] Color _colorMax = new Color(1f, 1f, 1f, 1f);
    [SerializeField] AnimationCurve _alphaCurve = AnimationCurve.EaseInOut(-1f, 0f, 1f, 1f);

    public void InitChunk(Vector3Int chunkCoords, ChunkSystem chunkSystem)
    {
        Debug.Log($"Initializing chunk {chunkCoords}");
        _shared = chunkSystem.GetComponent<CloudChunksSharedData>();

        //Rect area = new Rect(chunkSystem.GetChunkWorldOrigin(chunkCoords), chunkSystem.ChunkDimensions);

        var img = GetComponent<RawImage>();
        img.texture = _generateTexture(img.rectTransform.GetRect());
    }


    
    Texture2D _generateTexture(Rect area)
    {
        Color32 minColor = _colorMin, maxColor = _colorMax;
        var ret = new Texture2D(_textureResolution.x, _textureResolution.y);
        ret.wrapMode = TextureWrapMode.Clamp;
        
        var pixels = new Array2D<Color32>(_textureResolution.x, _textureResolution.y);
        for (int x = 0; x < _textureResolution.x; ++x)
        {
            for (int y = 0; y < _textureResolution.y; ++y)
            {
                Vector2 coords = area.min + area.size.MultiplyElems(new Vector2(x, y) / (_textureResolution - Vector2.one));
                float value = _shared.Noise.Sample2F(coords);
                float ratio = _alphaCurve.Evaluate(value).Clamp01();
                pixels[x,y] = Color32.Lerp(minColor, maxColor, ratio);
            }
        }
        //pixels[0, 0] = pixels[1, 0] =pixels[0, 1] = Color.red;
        //pixels[_textureResolution.x - 1, _textureResolution.y - 1] = Color.yellow;
        ret.SetPixels32(pixels.BackingArray);

        ret.Apply();
        return ret;
    }
}
