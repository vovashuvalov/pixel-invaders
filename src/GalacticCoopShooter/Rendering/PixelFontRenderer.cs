using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalacticCoopShooter.Rendering;

public sealed class PixelFontRenderer
{
    private const int GlyphWidth = 5;
    private const int GlyphHeight = 5;
    private const int GlyphSpacing = 1;

    private static readonly IReadOnlyDictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
    {
        ['A'] = [".###.", "#...#", "#####", "#...#", "#...#"],
        ['B'] = ["####.", "#...#", "####.", "#...#", "####."],
        ['C'] = [".####", "#....", "#....", "#....", ".####"],
        ['D'] = ["####.", "#...#", "#...#", "#...#", "####."],
        ['E'] = ["#####", "#....", "####.", "#....", "#####"],
        ['F'] = ["#####", "#....", "####.", "#....", "#...."],
        ['G'] = [".####", "#....", "#.###", "#...#", ".###."],
        ['H'] = ["#...#", "#...#", "#####", "#...#", "#...#"],
        ['I'] = ["#####", "..#..", "..#..", "..#..", "#####"],
        ['J'] = ["..###", "...#.", "...#.", "#..#.", ".##.."],
        ['K'] = ["#...#", "#..#.", "###..", "#..#.", "#...#"],
        ['L'] = ["#....", "#....", "#....", "#....", "#####"],
        ['M'] = ["#...#", "##.##", "#.#.#", "#...#", "#...#"],
        ['N'] = ["#...#", "##..#", "#.#.#", "#..##", "#...#"],
        ['O'] = [".###.", "#...#", "#...#", "#...#", ".###."],
        ['P'] = ["####.", "#...#", "####.", "#....", "#...."],
        ['Q'] = [".###.", "#...#", "#...#", "#..##", ".####"],
        ['R'] = ["####.", "#...#", "####.", "#..#.", "#...#"],
        ['S'] = [".####", "#....", ".###.", "....#", "####."],
        ['T'] = ["#####", "..#..", "..#..", "..#..", "..#.."],
        ['U'] = ["#...#", "#...#", "#...#", "#...#", ".###."],
        ['V'] = ["#...#", "#...#", "#...#", ".#.#.", "..#.."],
        ['W'] = ["#...#", "#...#", "#.#.#", "##.##", "#...#"],
        ['X'] = ["#...#", ".#.#.", "..#..", ".#.#.", "#...#"],
        ['Y'] = ["#...#", ".#.#.", "..#..", "..#..", "..#.."],
        ['Z'] = ["#####", "...#.", "..#..", ".#...", "#####"],
        ['0'] = [".###.", "#..##", "#.#.#", "##..#", ".###."],
        ['1'] = ["..#..", ".##..", "..#..", "..#..", ".###."],
        ['2'] = [".###.", "#...#", "...#.", "..#..", "#####"],
        ['3'] = ["####.", "....#", "..##.", "....#", "####."],
        ['4'] = ["#..#.", "#..#.", "#####", "...#.", "...#."],
        ['5'] = ["#####", "#....", "####.", "....#", "####."],
        ['6'] = [".###.", "#....", "####.", "#...#", ".###."],
        ['7'] = ["#####", "...#.", "..#..", ".#...", ".#..."],
        ['8'] = [".###.", "#...#", ".###.", "#...#", ".###."],
        ['9'] = [".###.", "#...#", ".####", "....#", ".###."],
        [':'] = [".....", "..#..", ".....", "..#..", "....."],
        ['-'] = [".....", ".....", ".###.", ".....", "....."],
        ['/'] = ["....#", "...#.", "..#..", ".#...", "#...."],
        ['.'] = [".....", ".....", ".....", "..#..", "....."],
        ['!'] = ["..#..", "..#..", "..#..", ".....", "..#.."],
        ['?'] = [".###.", "...#.", "..#..", ".....", "..#.."],
        [' '] = [".....", ".....", ".....", ".....", "....."]
    };

    public void DrawText(SpriteBatch spriteBatch, Texture2D pixel, string text, Vector2 position, int scale, Color color)
    {
        var cursor = position;
        var upperText = text.ToUpperInvariant();

        for (var i = 0; i < upperText.Length; i++)
        {
            var character = upperText[i];

            if (character == '\n')
            {
                cursor.X = position.X;
                cursor.Y += (GlyphHeight + 1) * scale;
                continue;
            }

            if (!Glyphs.TryGetValue(character, out var glyph))
            {
                glyph = Glyphs['?'];
            }

            for (var y = 0; y < GlyphHeight; y++)
            {
                for (var x = 0; x < GlyphWidth; x++)
                {
                    if (glyph[y][x] != '#')
                    {
                        continue;
                    }

                    var pixelPosition = new Rectangle(
                        (int)cursor.X + (x * scale),
                        (int)cursor.Y + (y * scale),
                        scale,
                        scale);

                    spriteBatch.Draw(pixel, pixelPosition, color);
                }
            }

            cursor.X += (GlyphWidth + GlyphSpacing) * scale;
        }
    }

    public Vector2 MeasureText(string text, int scale)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Vector2.Zero;
        }

        var currentLineWidth = 0;
        var widestLine = 0;
        var lineCount = 1;

        var upperText = text.ToUpperInvariant();

        for (var i = 0; i < upperText.Length; i++)
        {
            if (upperText[i] == '\n')
            {
                widestLine = Math.Max(widestLine, currentLineWidth);
                currentLineWidth = 0;
                lineCount++;
                continue;
            }

            currentLineWidth += (GlyphWidth + GlyphSpacing) * scale;
        }

        widestLine = Math.Max(widestLine, currentLineWidth);
        var height = lineCount * GlyphHeight * scale + (lineCount - 1) * scale;

        return new Vector2(widestLine, height);
    }
}
