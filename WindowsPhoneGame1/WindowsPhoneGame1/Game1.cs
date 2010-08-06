using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace WindowsPhoneGame1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D texture1;
        Texture2D texture2;
        Vector2 spritePosition1;
        Vector2 spritePosition2;
        Vector2 spriteSpeed1 = new Vector2(50.0f, 50.0f);
        Vector2 spriteSpeed2 = new Vector2(100.0f, 100.0f);
        int sprite1Height;
        int sprite1Width;
        int sprite2Height;
        int sprite2Width;

        const float SPEED = 240f / 1000; // pixels per millisecond
        const string TEXT = "Hello, Windows Phone!";
 
        SpriteFont kootenay14;
        Vector2 midPoint;
        Vector2 pathVector;
        Vector2 pathDirection;
        Vector2 textPosition;
        Vector2 position1;
        float lapSpeed;
        float tLap;
        SoundEffect soundEffect;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            texture1 = Content.Load<Texture2D>("GameThumbnail");
            texture2 = Content.Load<Texture2D>("GameThumbnail");

            soundEffect = Content.Load<SoundEffect>("explosion");


            spriteBatch = new SpriteBatch(GraphicsDevice);
            Rectangle clientBounds = this.Window.ClientBounds;
            kootenay14 = this.Content.Load<SpriteFont>("Kootenay14");
            Vector2 textSize = kootenay14.MeasureString(TEXT);
            position1 = new Vector2(clientBounds.Right - textSize.X, clientBounds.Top);
            Vector2 position2 = new Vector2(clientBounds.Left, clientBounds.Bottom - textSize.Y);
            midPoint = Vector2.Lerp(position1, position2, 0.5f);
            pathVector = position2 - position1;
            lapSpeed = SPEED / (2 * pathVector.Length());
            pathDirection = pathVector;
            pathDirection.Normalize();
            textPosition = position1;


            spritePosition1.X = 0;
            spritePosition1.Y = 0;

            spritePosition2.X = graphics.GraphicsDevice.Viewport.Width - texture1.Width;
            spritePosition2.Y = graphics.GraphicsDevice.Viewport.Height - texture1.Height;

            sprite1Height = texture1.Bounds.Height;
            sprite1Width = texture1.Bounds.Width;

            sprite2Height = texture2.Bounds.Height;
            sprite2Width = texture2.Bounds.Width;
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
                ButtonState.Pressed)
                this.Exit();

            // Move the sprite around.
            UpdateSprite(gameTime, ref spritePosition1, ref spriteSpeed1);
            UpdateSprite(gameTime, ref spritePosition2, ref spriteSpeed2);
            CheckForCollision();

            base.Update(gameTime);


            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            float pixelChange = SPEED * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            textPosition += pixelChange * pathDirection;
            if ((textPosition - midPoint).LengthSquared() > (0.5f * pathVector).LengthSquared())
            {
                float excess = (textPosition - midPoint).Length() -
                    (0.5f * pathVector).Length();

                pathDirection = -pathDirection;
                textPosition += 2 * excess * pathDirection;
            }
            tLap += lapSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            tLap %= 1;
            float pLap = tLap < 0.5f ? 2 * tLap : 2 - 2 * tLap;
            textPosition = position1 + pLap * pathVector;

            base.Update(gameTime);

        }

        void UpdateSprite(GameTime gameTime, ref Vector2 spritePosition, ref Vector2 spriteSpeed)
        {
            // Move the sprite by speed, scaled by elapsed time.
            spritePosition +=
                spriteSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            int MaxX =
                graphics.GraphicsDevice.Viewport.Width - texture1.Width;
            int MinX = 0;
            int MaxY =
                graphics.GraphicsDevice.Viewport.Height - texture1.Height;
            int MinY = 0;

            // Check for bounce.
            if (spritePosition.X > MaxX)
            {
                spriteSpeed.X *= -1;
                spritePosition.X = MaxX;
            }

            else if (spritePosition.X < MinX)
            {
                spriteSpeed.X *= -1;
                spritePosition.X = MinX;
            }

            if (spritePosition.Y > MaxY)
            {
                spriteSpeed.Y *= -1;
                spritePosition.Y = MaxY;
            }

            else if (spritePosition.Y < MinY)
            {
                spriteSpeed.Y *= -1;
                spritePosition.Y = MinY;
            }

        }

        void CheckForCollision()
        {
            BoundingBox bb1 = new BoundingBox(new Vector3(spritePosition1.X - (sprite1Width / 2), spritePosition1.Y - (sprite1Height / 2), 0), new Vector3(spritePosition1.X + (sprite1Width / 2), spritePosition1.Y + (sprite1Height / 2), 0));

            BoundingBox bb2 = new BoundingBox(new Vector3(spritePosition2.X - (sprite2Width / 2), spritePosition2.Y - (sprite2Height / 2), 0), new Vector3(spritePosition2.X + (sprite2Width / 2), spritePosition2.Y + (sprite2Height / 2), 0));

            if (bb1.Intersects(bb2))
            {
                soundEffect.Play();
            }

        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the sprite.
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            spriteBatch.Draw(texture1, spritePosition1, Color.White);
            spriteBatch.DrawString(kootenay14, TEXT, textPosition, Color.White);
            spriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque);
            spriteBatch.Draw(texture2, spritePosition2, Color.Gray);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
