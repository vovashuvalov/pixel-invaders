using GalacticCoopShooter.Core;
using GalacticCoopShooter.Rendering;
using GalacticCoopShooter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalacticCoopShooter;

public sealed class ShooterGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly List<Star> _stars = [];

    private SpriteBatch? _spriteBatch;
    private Texture2D? _pixel;
    private PixelFontRenderer? _font;
    private float _backgroundTime;

    private readonly record struct Star(Vector2 Position, float Speed, int Size);

    public ShooterGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = GameConfig.ScreenWidth;
        _graphics.PreferredBackBufferHeight = GameConfig.ScreenHeight;
        _graphics.SynchronizeWithVerticalRetrace = true;

        IsFixedTimeStep = true;
        IsMouseVisible = true;

        Content.RootDirectory = "Content";
        Window.Title = "Pixel Invaders";

        BuildStarfield();
    }

    public InputManager Input { get; } = new();
    public GameStateManager StateManager { get; } = new();
    public Random Random { get; } = new();

    public SpriteBatch SpriteBatch => _spriteBatch ?? throw new InvalidOperationException("SpriteBatch is not initialized.");
    public Texture2D Pixel => _pixel ?? throw new InvalidOperationException("Pixel texture is not initialized.");
    public PixelFontRenderer Font => _font ?? throw new InvalidOperationException("Pixel font is not initialized.");

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

        _font = new PixelFontRenderer();
        StateManager.ChangeState(new MenuState(this));
    }

    protected override void Update(GameTime gameTime)
    {
        Input.Update();

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        UpdateStarfield(deltaTime);

        StateManager.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(5, 8, 20));

        SpriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: DepthStencilState.None,
            rasterizerState: RasterizerState.CullNone);

        DrawBackground();
        StateManager.Draw(gameTime);

        SpriteBatch.End();

        base.Draw(gameTime);
    }

    private void BuildStarfield()
    {
        var seededRandom = new Random(1337);

        for (var i = 0; i < 95; i++)
        {
            var star = new Star(
                new Vector2(seededRandom.Next(0, GameConfig.ScreenWidth), seededRandom.Next(0, GameConfig.ScreenHeight)),
                16f + seededRandom.Next(0, 95),
                seededRandom.Next(1, 3));

            _stars.Add(star);
        }
    }

    private void UpdateStarfield(float deltaTime)
    {
        _backgroundTime += deltaTime;

        for (var i = 0; i < _stars.Count; i++)
        {
            var star = _stars[i];
            var newPosition = star.Position + new Vector2(0f, star.Speed * deltaTime);

            if (newPosition.Y > GameConfig.ScreenHeight + star.Size)
            {
                newPosition = new Vector2(Random.Next(0, GameConfig.ScreenWidth), -star.Size);
            }

            _stars[i] = star with { Position = newPosition };
        }
    }

    private void DrawBackground()
    {
        var stripeHeight = 12;
        var topColor = new Color(10, 20, 60);
        var bottomColor = new Color(2, 4, 18);

        for (var y = 0; y < GameConfig.ScreenHeight; y += stripeHeight)
        {
            var t = y / (float)GameConfig.ScreenHeight;
            var color = Color.Lerp(topColor, bottomColor, t);
            PrimitiveRenderer.DrawRect(SpriteBatch, Pixel, new Rectangle(0, y, GameConfig.ScreenWidth, stripeHeight), color);
        }

        var pulse = 0.45f + (MathF.Sin(_backgroundTime * 0.5f) * 0.2f);
        PrimitiveRenderer.DrawRect(SpriteBatch, Pixel, new Rectangle(70, 100, 240, 170), new Color(20, 70, 150) * pulse);
        PrimitiveRenderer.DrawRect(SpriteBatch, Pixel, new Rectangle(620, 240, 280, 200), new Color(18, 55, 120) * (pulse * 0.9f));

        for (var i = 0; i < _stars.Count; i++)
        {
            var star = _stars[i];
            var alpha = 0.4f + 0.4f * MathF.Abs(MathF.Sin(_backgroundTime + i));
            var color = new Color(200, 220, 255) * alpha;

            PrimitiveRenderer.DrawRect(
                SpriteBatch,
                Pixel,
                new Rectangle((int)star.Position.X, (int)star.Position.Y, star.Size, star.Size),
                color);
        }
    }
}
