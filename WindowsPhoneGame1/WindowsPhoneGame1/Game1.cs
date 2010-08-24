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
 
        public static SpriteFont kootenay14;
        Vector2 pathVector;
        Vector2 textPosition;
        Vector2 startPosition;

        float lapSpeed;
        float tLap;
        SoundEffect soundEffect;

        List<String> debugOutput = new List<String>();
        int upperCoordX = 0;
        int lowerCoordX = 800;
        int leftCoordY = 480;
        int rightCoordY = 0;

        public static Texture2D texture;
        Texture2D squareTex;
        Texture2D triangleTex;
        Texture2D square;
        Texture2D circleTex;
        Texture2D whiteSquare;
        public static Texture2D siloTexture;
        public static Texture2D radarTexture;
        public static Texture2D interceptorTexture;

        List<Crater> m_craters = new List<Crater>();
        List<Missile> m_missiles = new List<Missile>();
        List<Shockwave> m_shockwaves = new List<Shockwave>();
        List<InterceptorSite> m_interceptorSites = new List<InterceptorSite>();
        List<Interceptor> m_interceptors = new List<Interceptor>();
        List<Border> m_borders = new List<Border>();
        List<CellControl> m_cells = new List<CellControl>();

        List<Radar> m_radar = new List<Radar>();
        List<MissileSilo> m_silos = new List<MissileSilo>();

        Dictionary<Building,Texture2D> buildingToTextureMap = new Dictionary<Building,Texture2D>();
        bool m_IsKeyDown;
        bool m_StartClickOnObject;
        bool m_StartClickOnAddBuilding;
        Building selectedBuilding;
        bool m_IsKeyReleased;
        TouchCollection m_previousTouchState;

        SoundEffect m_missileLaunch;
        SoundEffect m_missileStrike1;
        SoundEffect m_missileStrike2;
        SoundEffect m_siren;
        Vector2 m_startDragPosition;
        static Vector2 m_translation;
        public static float HalfPI = (float) Math.PI / 2;

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
            texture = Content.Load<Texture2D>("square");
            circleTex = Content.Load<Texture2D>("circle");
            squareTex = Content.Load<Texture2D>("squared");
            triangleTex = Content.Load<Texture2D>("triangle");
            whiteSquare = Content.Load<Texture2D>("white2x2");
            siloTexture = triangleTex;
            radarTexture = circleTex;
            interceptorTexture = squareTex;

            soundEffect = Content.Load<SoundEffect>("explosion");
            Rectangle clientBounds = this.Window.ClientBounds;

            debugOutput.Add("x:");
            debugOutput.Add("y:");

            m_translation = Vector2.Zero;
            m_missileLaunch = Content.Load<SoundEffect>("missile");
            m_missileStrike1 = Content.Load<SoundEffect>("explosion");
            
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            kootenay14 = this.Content.Load<SpriteFont>("Kootenay14");
            Vector2 textSize = kootenay14.MeasureString(TEXT);
            startPosition = new Vector2(clientBounds.Right - textSize.X, clientBounds.Top);

            buildingToTextureMap[Building.None] = null;
            buildingToTextureMap[Building.Silo] = siloTexture;
            buildingToTextureMap[Building.Radar] = radarTexture;
            buildingToTextureMap[Building.InterceptorSite] = interceptorTexture;

            m_cells = new List<CellControl>();
            
            CellControl silo = new CellControl();
            silo.itemContainedInCell = Building.Silo;
            
            CellControl interceptor = new CellControl();
            interceptor.itemContainedInCell = Building.InterceptorSite;

            CellControl radar = new CellControl();
            radar.itemContainedInCell = Building.Radar;

            m_cells.Add(silo);
            m_cells.Add(interceptor);
            m_cells.Add(radar);



            // putting border together (buttons at bottom of screen)
            m_borders = new List<Border>();
            int squareDimensionInPixels = clientBounds.Width / m_cells.Count;
            int numberOfBordersAlongSquareLength = squareDimensionInPixels / whiteSquare.Bounds.Width;
            int borderTopCoordinate = lowerCoordX - (int)whiteSquare.Bounds.Width - squareDimensionInPixels;
            int numberOfSquaresInVerticalColumn = squareDimensionInPixels / whiteSquare.Bounds.Height;
            int pixelSpacingBetweenColumns = leftCoordY / m_cells.Count;

            foreach (CellControl c in m_cells)
            {
                c.texture = buildingToTextureMap[c.itemContainedInCell];
                c.SquareDimensionInPixels = squareDimensionInPixels;
            }
            // populates cells/borders (for UI controls at bottom of screen)
            for(int cellIndex = 0; cellIndex < m_cells.Count; cellIndex++)
            {
                // calculate the text positions for the building icon that is housed within this cell. 
                m_cells[cellIndex].textPosition = new Vector2(borderTopCoordinate + squareDimensionInPixels / 2, clientBounds.Right - (cellIndex * squareDimensionInPixels) - squareDimensionInPixels / 2);

                //calculate the bounding rectangle for this cell. 
                m_cells[cellIndex].boundingRectangle = new Rectangle(
                            borderTopCoordinate,
                            clientBounds.Right - ((cellIndex + 1)*squareDimensionInPixels),
                            squareDimensionInPixels,
                            squareDimensionInPixels
                        );
                
                // draw horizontal line for this cell
                for (int i = 0; i < numberOfBordersAlongSquareLength; i++)
                {
                    var b = new Border();
                    b.texture = whiteSquare;    
                    b.scale = 1f;
                    b.textPosition = new Vector2(borderTopCoordinate, clientBounds.Right - (i * whiteSquare.Bounds.Width + squareDimensionInPixels*cellIndex)  );
                    m_borders.Add(b);
                }

                // draw vertical lines for this cell
                for (int k = 0; k < numberOfSquaresInVerticalColumn; k++)
                {
                    Border b = new Border();
                    b.texture = whiteSquare;
                    b.scale = 1f;
                    b.textPosition = new Vector2( borderTopCoordinate + (k * whiteSquare.Bounds.Height), leftCoordY - (cellIndex * pixelSpacingBetweenColumns) );
                    m_borders.Add(b);
                }
            }

            m_silos = new List<MissileSilo>();
            MissileSilo initialSilo = new MissileSilo();
            initialSilo.textPosition = new Vector2(clientBounds.Top, clientBounds.Right - 30);
            m_silos.Add(initialSilo);

            Vector2 endPosition = new Vector2(clientBounds.Left, clientBounds.Bottom - textSize.Y);
            pathVector = endPosition - startPosition;

            lapSpeed = SPEED / (2 * pathVector.Length());
            textPosition = startPosition;

            m_IsKeyDown = false;
            m_StartClickOnObject = false;
            m_StartClickOnAddBuilding = false; 
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

            var touchState = TouchPanel.GetState();

            if (touchState.Count > 0)
            {
                debugOutput[0] = "x: " + touchState[0].Position.X.ToString();
                debugOutput[1] = "y: " + touchState[0].Position.Y.ToString();
            }

            // check to see if the user is starting to touch the screen. 
            if( touchState.Count > 0 && m_previousTouchState.Count == 0 )
            {
                m_IsKeyDown = true;
                m_startDragPosition = touchState[0].Position;

                // figure out if the users touch intersects with any objects. 
                m_StartClickOnObject = false;
                Vector2 touchReleasePoint = touchState[0].Position;

                Rectangle touchRectangle = Vect2Rect(  touchReleasePoint );
                if ( GetRectangleCoordinatesForTexture( m_silos[0] ).Intersects(touchRectangle) )
                {
                    m_StartClickOnObject = true;
                }

                foreach(CellControl c in m_cells)
                {
                    if( c.boundingRectangle.Intersects(touchRectangle) )
                    {
                        m_StartClickOnAddBuilding = true;
                        selectedBuilding = c.itemContainedInCell;
                    }
                }
                
                // if not, then just move the map position. 
            }

            // check to see if the user is just dragging an already existing touch. 
            if( touchState.Count > 0 
                && !m_StartClickOnObject
                && !m_StartClickOnAddBuilding
                && touchState[0].State == TouchLocationState.Moved )
            {

                m_translation += touchState[0].Position - m_previousTouchState[0].Position;
            }

            // check to see if the user has released an existing touch.
            if( touchState.Count > 0 
                && (m_StartClickOnObject || m_StartClickOnAddBuilding)
                && touchState[0].State == TouchLocationState.Released 
                && (m_previousTouchState[0].State == TouchLocationState.Moved || m_previousTouchState[0].State == TouchLocationState.Pressed) )

            {
                Vector2 touchReleasePoint = MapClickPointToMapCoordinates(touchState[0].Position);

                // check to see if the release point is outside the bounds of the border control. 
                // it it isn't, we won't actually add a building. 
                if(touchReleasePoint.X < m_cells.Min( c => (lowerCoordX - c.SquareDimensionInPixels)))
                {
                    if (selectedBuilding == Building.Radar)
                    {
                        Radar r = new Radar();
                        r.textPosition = touchReleasePoint;
                        m_radar.Add(r);
                        selectedBuilding = Building.None;
                    }

                    else if (selectedBuilding == Building.InterceptorSite)
                    {
                        InterceptorSite i = new InterceptorSite();
                        i.textPosition = touchReleasePoint;
                        m_interceptorSites.Add(i);
                        selectedBuilding = Building.None;
                    }
                    else if (selectedBuilding == Building.Silo)
                    {
                        MissileSilo ms = new MissileSilo();
                        ms.textPosition = touchReleasePoint;
                        m_silos.Add(ms);
                        selectedBuilding = Building.None;
                    }
                }

                m_StartClickOnAddBuilding = false;
               

                // if we started our click on the silo, then launch a missile. 
                if(m_StartClickOnObject)
                {

                    var missile = new Missile();

                    Vector2 textSize = kootenay14.MeasureString(TEXT);

                    textSize = kootenay14.MeasureString("missile");
                    missile.startPosition = m_silos[0].textPosition;
                    missile.textPosition = missile.startPosition;
                    missile.pathVector = touchReleasePoint - missile.startPosition;
                    missile.lapSpeed = SPEED / (2 * missile.pathVector.Length());
                    missile.scale = 1;
                    float rotation = (float)Math.Acos((double)missile.pathVector.X / missile.pathVector.Length());

                    if (missile.pathVector.Y < 0)
                    {
                        //compensation for the Inverse cosine function, which only has range from (0 < theta < pi ), and we need 
                        //a full 2*PI range. 
                        rotation = -rotation;
                    }

                    missile.rotation = rotation;
                    m_missiles.Add(missile);
                    m_missileLaunch.Play();
                }

                // if we started clicking on a building, then provide a building halo until it's placed. 
                if(m_StartClickOnAddBuilding)
                {
                    
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

            m_previousTouchState = touchState;
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
            graphics.GraphicsDevice.Clear(Color.Black);
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
            foreach(MissileSilo m in m_silos)
            {
                spriteBatch.Draw(m.texture, MapGameToScreenCoordinates(m.textPosition + GlobalDisplacement), m.color);
            }

            Rectangle clientBounds = this.Window.ClientBounds;
            Vector2 stringDimensions = kootenay14.MeasureString("blah");
            for(int i = 0; i < debugOutput.Count; i++)
            {
                var val = debugOutput[i];
                spriteBatch.DrawString(
                        kootenay14, 
                        val, 
                        new Vector2(clientBounds.Top + i*stringDimensions.Y, clientBounds.Right - 30), 
                        Color.White,
                        (float) -Math.PI / 2, 
                        Vector2.Zero,
                        1f, 
                        SpriteEffects.None, 
                        0f);
            }
            foreach (CellControl c in m_cells)
            {
                spriteBatch.Draw(c.texture, c.textPosition, null, Color.White, c.rotation, Vector2.Zero, c.scale, SpriteEffects.None, 0f);
            }
            foreach (Border b in m_borders)
            {
                spriteBatch.Draw(b.texture, b.textPosition, null, b.color, b.rotation, Vector2.Zero, b.scale, SpriteEffects.None, 0f);
            }
            foreach (Missile m in m_missiles)
            {       
                spriteBatch.Draw(m.texture, MapGameToScreenCoordinates(m.textPosition + GlobalDisplacement), null, m.color, m.rotation, Vector2.Zero, m.scale, SpriteEffects.None, 0f);
            }
            foreach (Crater c in m_craters)
            {
                spriteBatch.Draw(c.texture, MapGameToScreenCoordinates(c.textPosition + GlobalDisplacement), null, c.color, c.rotation, Vector2.Zero, c.scale, SpriteEffects.None, 0f);
            }
            foreach(Radar r in m_radar)
            {
                spriteBatch.Draw(r.texture, MapGameToScreenCoordinates(r.textPosition + GlobalDisplacement), null, r.color, r.rotation, Vector2.Zero, r.scale, SpriteEffects.None, 0f);
            }
            foreach (InterceptorSite i in m_interceptorSites)
            {
                spriteBatch.Draw(i.texture, MapGameToScreenCoordinates(i.textPosition + GlobalDisplacement), null, i.color, i.rotation, Vector2.Zero, i.scale, SpriteEffects.None, 0f);
            }
            foreach (Interceptor i in m_interceptors)
            {
                spriteBatch.Draw(i.texture, MapGameToScreenCoordinates(i.textPosition + GlobalDisplacement), null, i.color, i.rotation, Vector2.Zero, i.scale, SpriteEffects.None, 0f);
            }
            if(m_StartClickOnAddBuilding && m_previousTouchState.Count > 0)
            {
                spriteBatch.Draw(buildingToTextureMap[selectedBuilding], m_previousTouchState[0].Position, null, Color.Green, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque);
            //spriteBatch.Draw(texture2, spritePosition2, Color.Gray);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// inputs a vector, and returns a 1x1 rectangle at the vectors position. 
        /// this is a helper function for a task which I commonly use when determining
        /// if a point intersects with a rectangle. 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Rectangle Vect2Rect(Vector2 v)
        {
            return new Rectangle((int)v.X, (int)v.Y, 1, 1);
        }

        public Rectangle ApplyWorldTranslationToRectangle(Rectangle r)
        {
            return new Rectangle(
                    (int)(r.X + m_translation.X),
                    (int)(r.Y + m_translation.Y),
                    r.Height,
                    r.Width);
        }

        public Rectangle GetRectangleCoordinatesForTexture(TextureItem t)
        {
            return ApplyWorldTranslationToRectangle(
                new Rectangle(
                    (int)t.textPosition.X,
                    (int)t.textPosition.Y,
                    t.texture.Bounds.Height,
                    t.texture.Bounds.Width
                    )
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameCoordinates"></param>
        /// <returns></returns>
        public static Vector2 MapGameToScreenCoordinates(Vector2 gameCoordinates)
        {
            Vector2 screenCoordinates = m_translation + gameCoordinates;
            return screenCoordinates;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clickPoint"></param>
        /// <returns></returns>
        public static Vector2 MapClickPointToMapCoordinates(Vector2 clickPoint)
        {
            Vector2 mapCoordinates = clickPoint - m_translation;
            return mapCoordinates;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class TextureItem
    {
        public Vector2 textPosition;
        public float rotation = 0f;
        public Vector2 origin = Vector2.Zero;
        public float scale = 1f;
        public Color color = Color.Pink;
        public Texture2D texture = Game1.texture;
    }
    /// <summary>
    /// 
    /// </summary>
    public class TextArt
    {
        public List<TextureItem> items = new List<TextureItem>();
    }
    /// <summary>
    /// 
    /// </summary>
    public class MissileSilo : TextureItem
    {
        public TextArt art = new TextArt();

        public MissileSilo()
        {
            this.color = Color.Red;
            this.texture = Game1.siloTexture;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Interceptor : TextureItem
    {
    }
    /// <summary>
    /// 
    /// </summary>
    public class InterceptorSite : TextureItem
    {
        public InterceptorSite()
        {
            this.color = Color.LimeGreen;
            this.texture = Game1.interceptorTexture;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class Radar : TextureItem
    {
        public Radar()
        {
            this.color = Color.Blue;
            this.texture = Game1.radarTexture;           
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Crater : TextureItem
    {
        public Crater()
        {
            this.color = Color.DarkGray;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Missile : TextureItem
    {
        public Vector2 pathVector;
        public Vector2 startPosition;
        public float lapSpeed;
        public float tLap;
        public float origin;

        public Missile()
        {
            this.color = Color.LightSlateGray;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Shockwave
    {
        public Vector2 m_displacement;
        public double totalMsStartTime;
    }

    /// <summary>
    /// 
    /// </summary>
    public class Border : TextureItem
    {
        public Border()
        {
            this.color = Color.GhostWhite;
        }
    }

    /// <summary>
    /// class to wrap the 'cells' that contain the  buildings which the user can deploy, drawn on the screen bottom.
    /// </summary>
    public class CellControl : TextureItem 
    {
        public Building itemContainedInCell;
        public Rectangle boundingRectangle;
        public int SquareDimensionInPixels;

    }

    /// <summary>
    /// 
    /// </summary>
    public enum Building
    {
        Base,
        Silo,
        InterceptorSite,
        Radar,
        None
    }
}
