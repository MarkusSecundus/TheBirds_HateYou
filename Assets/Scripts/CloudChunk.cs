using MarkusSecundus.Utils.Datastructs;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Procgen.Chunking;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CloudChunk : MonoBehaviour, IChunkInitializer
{
    CloudChunksSharedData _shared;
    [SerializeField] Vector2Int _textureResolution = new Vector2Int(128, 128);

    [SerializeField] Color _colorMin = new Color(1f, 1f, 1f, 0f);
    [SerializeField] Color _colorMax = new Color(1f, 1f, 1f, 1f);
    [SerializeField] FilterMode _filterMode = FilterMode.Bilinear;
    [SerializeField] AnimationCurve _alphaCurve = AnimationCurve.EaseInOut(-1f, 0f, 1f, 1f);


    public void InitChunk(Vector3Int chunkCoords, ChunkSystem chunkSystem)
    {
        _shared = chunkSystem.GetComponent<CloudChunksSharedData>();

        Rect area = new Rect(chunkSystem.GetChunkLocalOrigin(chunkCoords), chunkSystem.ChunkDimensions);

        _generateTexture(GetComponent<RawImage>(), area);
    }


    
    void _generateTexture(RawImage img, Rect area)
    {
        Color32 minColor = _colorMin, maxColor = _colorMax;
        var noise = _shared.Noise;
        var alphaCurve = _alphaCurve;

        var texture = new Texture2D(_textureResolution.x, _textureResolution.y);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = _filterMode;

        var textureFiller = Task.Run(() =>
        {
            var pixels = new Array2D<Color32>(_textureResolution.x, _textureResolution.y);
            for (int x = 0; x < _textureResolution.x; ++x)
            {
                for (int y = 0; y < _textureResolution.y; ++y)
                {
                    Vector2 coords = area.min + area.size.MultiplyElems(new Vector2(x, y) / (_textureResolution - Vector2.one));
                    float value = noise.Sample2F(coords);
                    float ratio = alphaCurve.Evaluate(value).Clamp01();
                    pixels[x, y] = Color32.Lerp(minColor, maxColor, ratio);
                }
            }
            return pixels;
        });

        StartCoroutine(impl());
        IEnumerator impl()
        {
            while (true)
            {
                if (textureFiller.IsCompleted)
                {
                    img.color = Color.white;
                    texture.SetPixels32(textureFiller.Result.BackingArray);
                    texture.Apply();
                    img.texture = texture;
                    yield break;
                }
                yield return null;
            }
        }
    }
}
