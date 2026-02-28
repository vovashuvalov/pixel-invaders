using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalacticCoopShooter.Rendering;

public static class PrimitiveRenderer
{
    public static void DrawRect(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rectangle, Color color)
    {
        spriteBatch.Draw(pixel, rectangle, color);
    }

    public static void DrawOutline(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rectangle, int thickness, Color color)
    {
        DrawRect(spriteBatch, pixel, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, thickness), color);
        DrawRect(spriteBatch, pixel, new Rectangle(rectangle.Left, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
        DrawRect(spriteBatch, pixel, new Rectangle(rectangle.Left, rectangle.Top, thickness, rectangle.Height), color);
        DrawRect(spriteBatch, pixel, new Rectangle(rectangle.Right - thickness, rectangle.Top, thickness, rectangle.Height), color);
    }

    public static void DrawHeart(SpriteBatch spriteBatch, Texture2D pixel, Vector2 position, int unit, Color color)
    {
        var x = (int)position.X;
        var y = (int)position.Y;

        DrawRect(spriteBatch, pixel, new Rectangle(x + unit, y, unit, unit), color);
        DrawRect(spriteBatch, pixel, new Rectangle(x + unit * 3, y, unit, unit), color);
        DrawRect(spriteBatch, pixel, new Rectangle(x, y + unit, unit * 5, unit), color);
        DrawRect(spriteBatch, pixel, new Rectangle(x + unit, y + unit * 2, unit * 3, unit), color);
        DrawRect(spriteBatch, pixel, new Rectangle(x + unit * 2, y + unit * 3, unit, unit), color);
    }
}
