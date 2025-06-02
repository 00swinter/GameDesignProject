using UnityEngine;
using System;

public class NoiseTexture
{
    public int Width { get; }
    public int Height { get; }
    public float Scale { get; }
    public int Seed { get; }

    private Texture2D _texture;
    private float _offsetX;
    private float _offsetY;

    /// <summary>
    /// Generates a new noise texture.
    /// </summary>
    /// <param name="width">Texture width in pixels.</param>
    /// <param name="height">Texture height in pixels.</param>
    /// <param name="scale">How “zoomed in” the noise is. Larger values -> more variation.</param>
    /// <param name="seed">Random seed for texture variation.</param>
    public NoiseTexture(int width, int height, float scale, int seed)
    {
        Width = Mathf.Max(1, width);
        Height = Mathf.Max(1, height);
        Scale = Mathf.Max(0.0001f, scale);
        Seed = seed;

        // Use seed to pick a random offset in noise space
        var prng = new System.Random(seed);
        _offsetX = prng.Next(-100000, 100000);
        _offsetY = prng.Next(-100000, 100000);

        _texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };

        Generate();
    }

    /// <summary>
    /// The generated Texture2D you can assign to materials, UI, etc.
    /// </summary>
    public Texture2D Texture => _texture;

    /// <summary>
    /// Rebuilds the noise texture (e.g. if you change scale or seed).
    /// </summary>
    public void Generate()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                float sampleX = (_offsetX + x) / Width * Scale;
                float sampleY = (_offsetY + y) / Height * Scale;
                float noiseVal = Mathf.PerlinNoise(sampleX, sampleY);
                _texture.SetPixel(x, y, new Color(noiseVal, noiseVal, noiseVal));
            }
        }
        _texture.Apply();
    }

    /// <summary>
    /// Sample the noise at normalized UV coordinates [0..1].
    /// </summary>
    /// <param name="u">Horizontal coordinate (0 = left, 1 = right).</param>
    /// <param name="v">Vertical coordinate (0 = bottom, 1 = top).</param>
    /// <returns>Noise value in [0,1].</returns>
    public float SampleUV(float u, float v)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(u * Width), 0, Width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(v * Height), 0, Height - 1);
        return _texture.GetPixel(x, y).r;
    }

    /// <summary>
    /// Sample the noise at integer pixel coordinates.
    /// </summary>
    public float SamplePixel(int x, int y)
    {
        x = Mathf.Clamp(x, 0, Width - 1);
        y = Mathf.Clamp(y, 0, Height - 1);
        return _texture.GetPixel(x, y).r;
    }

    /// <summary>
    /// Create a Sprite from the generated texture.
    /// </summary>
    /// <param name="pixelsPerUnit">Sprite’s pixels-per-unit setting (default 100).</param>
    /// <param name="pivot">Normalized pivot point (default center).</param>
    public Sprite CreateSprite(float pixelsPerUnit = 100f, Vector2? pivot = null)
    {
        var p = pivot ?? new Vector2(0.5f, 0.5f);
        return Sprite.Create(
            _texture,
            new Rect(0, 0, Width, Height),
            p,
            pixelsPerUnit,
            0,
            SpriteMeshType.FullRect
        );
    }
}
