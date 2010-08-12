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


        const float SPEED = 120f / 1000; // pixels per millisecond
        const string TEXT = "Hello, Windows Phone!";
 
        SpriteFont kootenay14;
        Vector2 pathVector;
        Vector2 textPosition;
        Vector2 startPosition;

        float lapSpeed;
        float tLap;
        SoundEffect soundEffect;

        List<Crater> m_craters = new List<Crater>();
        List<Missile> m_missiles = new List<Missile>();
        List<Shockwave> m_shockwaves = new List<Shockwave>();

        MissileSilo m_silo = new MissileSilo();
        bool m_IsKeyDown;
        bool m_IsKeyReleased;
        
        SoundEffect m_missileLaunch;
        SoundEffect m_missileStrike1;
        SoundEffect m_missileStrike2;
        SoundEffect m_siren;
        Vector2 m_shockwaveDisplacement;

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

            m_missileLaunch = Content.Load<SoundEffect>("missile");
            m_missileStrike1 = Content.Load<SoundEffect>("explosion");
            
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Rectangle clientBounds = this.Window.ClientBounds;
            kootenay14 = this.Content.Load<SpriteFont>("Kootenay14");
            Vector2 textSize = kootenay14.MeasureString(TEXT);
            startPosition = new Vector2(clientBounds.Right - textSize.X, clientBounds.Top);
         
            m_silo.message = kootenay14;
            m_silo.textPosition = new Vector2(clientBounds.Right, clientBounds.Top);
            m_silo.text = "Silo";

            
            Vector2 endPosition = new Vector2(clientBounds.Left, clientBounds.Bottom - textSize.Y);
            pathVector = endPosition - startPosition;

            lapSpeed = SPEED / (2 * pathVector.Length());
            textPosition = startPosition;

            for(int i = 0; i < 0; i++)
            {
                m_missiles.Add(new Missile() );
            }

            for(int i = 0; i < m_missiles.Count(); i++)
            {
                Missile m = m_missiles[i];
                m.message = kootenay14;
                textSize = m.message.MeasureString("missile");
                m.textPosition = startPosition;
                m.startPosition = new Vector2(clientBounds.Right - textSize.X, clientBounds.Top + i*30);
                m.lapSpeed = lapSpeed;
                m.pathVector = endPosition - startPosition;
                m.rotation = -(float) Math.PI / 2f;
                m.scale = 1;
            }

            m_IsKeyDown = false;
            m_IsKeyReleased = false;
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

            var touchstate = TouchPanel.GetState();
            if (touchstate.Count > 0)
            {
                for (int i = 0; i < touchstate.Count; i++)
                {
                    var touchData = touchstate[i];
                    if (touchData.State == TouchLocationState.Pressed)
                    {
                        m_IsKeyDown = true;
                    }
                    
                    if (touchData.State == TouchLocationState.Released)
                    {

                        Vector2 touchReleasePoint = touchData.Position;
                        var missile = new Missile();

                        missile.message = this.Content.Load<SpriteFont>("Kootenay14");
                        Vector2 textSize = kootenay14.MeasureString(TEXT);
                    
                        textSize = missile.message.MeasureString("missile");
                        missile.startPosition = m_silo.textPosition;
                        missile.textPosition = missile.startPosition;
                        missile.pathVector = touchReleasePoint - missile.startPosition;
                        missile.lapSpeed = SPEED / (2 * missile.pathVector.Length());
                        missile.scale = 1;
                        float rotation = (float)Math.Acos((double)missile.pathVector.X / missile.pathVector.Length()) - (float)Math.PI / 2;
                        
                        if (missile.pathVector.Y > 0)
                        {
                            //compensation for the Inverse cosine function, which only has range from (0 < theta < pi ), and we need 
                            //a full 2*PI range. 
                            rotation = -rotation;
                        }

                        missile.rotation = rotation;
                        m_missiles.Add(missile);
                        m_missileLaunch.Play();
                    }
                }
            }
            
            // Move the sprite around.
            UpdateSprite(gameTime, ref spritePosition1, ref spriteSpeed1);
            UpdateSprite(gameTime, ref spritePosition2, ref spriteSpeed2);
            CheckForCollision();

            base.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            tLap += lapSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            tLap %= 1;
            float pLap = tLap < 0.5f ? 2 * tLap : 2 - 2 * tLap;
            textPosition = startPosition + pLap * pathVector;

            foreach(Missile m in m_missiles)
            {
                m.tLap += m.lapSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                m.tLap %= 1;
                pLap = 2 * m.tLap ;

                if (m.tLap > 0.5f)
                {
                    // the missile is at it's end point, we need to add a crater. 
                    Crater crater = new Crater();
                    crater.message = kootenay14;
                    crater.text = "Crater";
                    crater.textPosition = m.textPosition;

                    m_craters.Add(crater);
                    m_missileStrike1.Play();

                    Shockwave shock = new Shockwave();
                    shock.totalMsStartTime = gameTime.TotalGameTime.TotalMilliseconds;
                    shock.m_displacement = new Vector2(10f);
                    m_shockwaves.Add(shock);
                }

                m.textPosition = m.startPosition + m.pathVector * pLap;
            }

            //remove any missiles that have reached their destination. 
            m_missiles = m_missiles.Where(m => m.tLap < 0.5f).ToList();
            var shockwavesToEliminate = m_shockwaves.Count(s => (gameTime.TotalGameTime.TotalMilliseconds - s.totalMsStartTime) > 1000);
            if (shockwavesToEliminate > 0)
            {
                //System.Diagnostics.Debug.Assert(false);
            }

            m_shockwaves = m_shockwaves.Where(s => (gameTime.TotalGameTime.TotalMilliseconds - s.totalMsStartTime) < 1000).ToList();

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
                //soundEffect.Play();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            Vector2 GlobalDisplacement = Vector2.Zero;
            foreach (Shockwave s in m_shockwaves)
            {

                double timeSinceShockStart = gameTime.TotalGameTime.TotalMilliseconds - s.totalMsStartTime;

                if( timeSinceShockStart < 400 )
                {
                    float displacement = (float) (400 * Math.Sin(timeSinceShockStart) / timeSinceShockStart);
                    GlobalDisplacement += new Vector2( 0f, displacement);
                }
            }

            // Draw the sprite.
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            //spriteBatch.Draw(texture1, spritePosition1, Color.White);
            spriteBatch.DrawString(kootenay14, TEXT, textPosition, Color.White);
            spriteBatch.DrawString(m_silo.message, m_silo.text, m_silo.textPosition + GlobalDisplacement, Color.Gray);
            foreach (Missile m in m_missiles)
            {       
                spriteBatch.DrawString(kootenay14, "missile", m.textPosition, Color.White, m.rotation, Vector2.Zero, m.scale, SpriteEffects.None, 0);
            }

            foreach (Crater c in m_craters)
            {
                spriteBatch.DrawString(c.message, c.text, c.textPosition + GlobalDisplacement, Color.Red, (float)(-(Math.PI)/2f), Vector2.Zero, 1f, SpriteEffects.None, 0);
            }

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque);
            //spriteBatch.Draw(texture2, spritePosition2, Color.Gray);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    public class TextItem
    {
        public SpriteFont message;
        public string text;
        public Vector2 textPosition;
    }

    public class MissileSilo : TextItem
    {
    }

    public class Crater : TextItem
    {
    }

    public class Missile : TextItem
    {
        public Vector2 pathVector;
        public Vector2 startPosition;
        public float lapSpeed;
        public float tLap;
        public float rotation;
        public float origin;
        public float scale;        
    }

    public class Shockwave
    {
        public Vector2 m_displacement;
        public double totalMsStartTime;
    }

}
