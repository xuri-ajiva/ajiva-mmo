using SixLabors.ImageSharp.PixelFormats;

namespace Client.Renderer.Resources;

internal static class CoolTextureGenerator
{
    private static readonly Rgba32 Black = new Rgba32(20, 20, 20, 255);
    private static readonly Rgba32 Orange = new Rgba32(255, 140, 0, 255);
    private static readonly Rgba32 White = new Rgba32(220, 220, 220, 255);
    private static readonly Rgba32 Purple = new Rgba32(255, 0, 255, 255);

    private static void DrawLine(Span<Rgba32> data, i32 width, i32 height, i32 x1, i32 y1, i32 x2, i32 y2,
        i32 thickness, Rgba32 color)
    {
        if (x1 == x2) // Vertical Line
        {
            var startX = Math.Max(0, x1 - (thickness - 1) / 2);
            var endX = Math.Min(width, x1 + thickness / 2 + 1); // +1 for width calculation
            var startY = Math.Max(0, Math.Min(y1, y2));
            var endY = Math.Min(height, Math.Max(y1, y2) + 1); // +1 because y2 is often inclusive

            for (var y = startY; y < endY; ++y)
            {
                for (var x = startX; x < endX; ++x)
                {
                    // Avoid drawing over edge boundary if line is thick
                    if (x >= 0 && x < width && y >= 0 && y < height)
                        data[y * width + x] = color;
                }
            }
        }
        else if (y1 == y2) // Horizontal Line
        {
            var startY = Math.Max(0, y1 - (thickness - 1) / 2);
            var endY = Math.Min(height, y1 + thickness / 2 + 1); // +1 for height calculation
            var startX = Math.Max(0, Math.Min(x1, x2));
            var endX = Math.Min(width, Math.Max(x1, x2) + 1); // +1 because x2 is often inclusive

            for (var y = startY; y < endY; ++y)
            {
                for (var x = startX; x < endX; ++x)
                {
                    // Avoid drawing over edge boundary if line is thick
                    if (x >= 0 && x < width && y >= 0 && y < height)
                        data[y * width + x] = color;
                }
            }
        }
    }


    public static void GenerateCoolPattern(Span<Rgba32> data, i32 width, i32 height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width and height must be positive.");
        }

        // --- Parameters ---
        var outlineThickness =
            Math.Max(1, Math.Min(width / 10, height / 10)); // Make outline proportional, at least 1px
        var thinLineThickness = 1;
        var thickLineThickness = Math.Max(1, outlineThickness / 2); // Make center line a bit thicker

        // --- 1. Fill Background ---
        data.Fill(Black);

        // --- 2. Draw Orange Outline ---
        // Top border
        DrawLine(data, width, height, 0, (outlineThickness - 1) / 2, width - 1, (outlineThickness - 1) / 2,
            outlineThickness, Orange);
        // Bottom border
        DrawLine(data, width, height, 0, height - 1 - (outlineThickness - 1) / 2, width - 1,
            height - 1 - (outlineThickness - 1) / 2, outlineThickness, Orange);
        // Left border
        DrawLine(data, width, height, (outlineThickness - 1) / 2, 0, (outlineThickness - 1) / 2, height - 1,
            outlineThickness, Orange);
        // Right border
        DrawLine(data, width, height, width - 1 - (outlineThickness - 1) / 2, 0, width - 1 - (outlineThickness - 1) / 2,
            height - 1, outlineThickness, Orange);


        // --- 3. Draw Measurement Lines ---
        var qW = width / 4;
        var hW = width / 2;
        var tqW = 3 * width / 4;
        var qH = height / 4;
        var hH = height / 2;
        var tqH = 3 * height / 4;

        // Vertical Lines
        DrawLine(data, width, height, hW, 0, hW, height - 1, thickLineThickness, White); // Center
        DrawLine(data, width, height, qW, 0, qW, height - 1, thinLineThickness, White); // Quarter
        DrawLine(data, width, height, tqW, 0, tqW, height - 1, thinLineThickness, White); // Three-Quarter

        // Horizontal Lines
        DrawLine(data, width, height, 0, hH, width - 1, hH, thickLineThickness, White); // Center
        DrawLine(data, width, height, 0, qH, width - 1, qH, thinLineThickness, White); // Quarter
        DrawLine(data, width, height, 0, tqH, width - 1, tqH, thinLineThickness, White); // Three-Quarter


        // --- 4. Draw Question Mark (Pixel Art) ---
        var centerX = width / 2;
        var centerY = height / 2;

        // Define the shape relative to the center. Adjust these for desired look/size.
        // This list defines pixels to color relative to (centerX, centerY)
        List<(i32 dx, i32 dy)> questionMarkPixels = new List<(i32, i32)>
        {
            // Scale factor based on image size (simple approach)
            // Ensures the '?' is not just a few pixels on large textures
            (0, (i32)(-0.12 * height)), (1, (i32)(-0.12 * height)), // Top part
            (-1, (i32)(-0.09 * height)), (2, (i32)(-0.09 * height)),
            (-1, (i32)(-0.06 * height)), (2, (i32)(-0.06 * height)),
            (-1, (i32)(-0.03 * height)), (1, (i32)(-0.03 * height)), // Hook under
            (0, (i32)(0 * height)), // Straight down
            (0, (i32)(0.03 * height)),
            (0, (i32)(0.06 * height)),
            // Dot
            (0, (i32)(0.12 * height))
        };
        // Add thickness to the pixel art '?' by drawing neighbors
        var finalMarkPixels = new HashSet<(i32, i32)>();
        var markThickness = Math.Max(1, (thickLineThickness + 1) / 2); // Make thickness similar to center line

        foreach (var (dx, dy) in questionMarkPixels)
        {
            for (var ty = -(markThickness - 1) / 2; ty <= markThickness / 2; ty++)
            {
                for (var tx = -(markThickness - 1) / 2; tx <= markThickness / 2; tx++)
                {
                    finalMarkPixels.Add((dx + tx, dy + ty));
                }
            }
        }


        // Draw the calculated pixels
        foreach (var (dx, dy) in finalMarkPixels)
        {
            var drawX = centerX + dx;
            var drawY = centerY + dy;

            // Bounds check before drawing
            if (drawX >= outlineThickness && drawX < width - outlineThickness && // Don't draw over outline
                drawY >= outlineThickness && drawY < height - outlineThickness)
            {
                // Optional: Avoid drawing over measurement lines? (more complex check)
                // if (!IsOnMeasurementLine(drawX, drawY, width, height, thinLineThickness, thickLineThickness))
                // {
                data[drawY * width + drawX] = White; // Use the line color for the '?'
                // }
            }
        }
    }

    public static void GenerateMissingTexture(Span<Rgba32> data, i32 width, i32 height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width and height must be positive.");
        }

        // iconic missing texture pattern from gmod aka purple and black checkerboard
        var checkerSize = 8;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = (x / checkerSize + y / checkerSize) % 2 == 0 ? Purple : Black;
                data[y * width + x] = color;
            }
        }
    }

    public static void GenerateNormalMap(Span<Rgba32> data, i32 width, i32 height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width and height must be positive.");
        }

        // flat nornal map
        var normalColor = new Rgba32(0.5f, 0.5f, 1.0f, 1.0f); // Flat normal (pointing outwards)
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                data[y * width + x] = normalColor;
            }
        }
    }

    public static void GenerateRoughnessMap(Span<Rgba32> data, i32 extent3DWidth, i32 extent3DHeight)
    {
        if (extent3DWidth <= 0 || extent3DHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(extent3DWidth), "Width and height must be positive.");
        }

        // flat roughness map
        var roughnessColor = White; // Flat roughness (fully rough)
        for (var y = 0; y < extent3DHeight; y++)
        {
            for (var x = 0; x < extent3DWidth; x++)
            {
                data[y * extent3DWidth + x] = roughnessColor;
            }
        }
    }

    public static void GenerateBlackMap(Span<Rgba32> data, i32 width, i32 height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width and height must be positive.");
        }

        // flat black map
        var blackColor = Black;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                data[y * width + x] = blackColor;
            }
        }
    }

    public static void GenerateEmissiveMap(Span<Rgba32> data, i32 width, i32 height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width and height must be positive.");
        }

        // checkerboard emissive map
        var checkerSize = 8;
        var emissiveColor1 = Orange;
        var emissiveColor2 = Black;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = (x / checkerSize + y / checkerSize) % 2 == 0 ? emissiveColor1 : emissiveColor2;
                data[y * width + x] = color;
            }
        }
    }
}
