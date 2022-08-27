using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;

namespace SteeringBehavior
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont temporary;

        Vector2 startPosition = new Vector2(100, 100);

        Rectangle boundsToCheck;

        List<Particle> Edges = new List<Particle>();

        Random rand = new Random();

        string text = "STAN";


        bool drawn = false;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            graphics.PreferredBackBufferWidth = 900;
            graphics.ApplyChanges();

            IsMouseVisible = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.

            
            spriteBatch = new SpriteBatch(GraphicsDevice);

            temporary = Content.Load<SpriteFont>("File");
            var size = temporary.MeasureString(text);

            boundsToCheck = new Rectangle((int)startPosition.X, (int)startPosition.Y, (int)size.X, (int)size.Y);

            

            var points = FindEdgesWithGraphicsPath(text, new Vector2(0, 0));
            foreach (var item in points)
            {
                var pos = new Vector2(rand.Next(0, GraphicsDevice.Viewport.Width), rand.Next(0, GraphicsDevice.Viewport.Height));
                Edges.Add(new Particle(pos, item, rand));
            }

            // TODO: use this.Content to load your game content here
        }

        private int TwoDToOneD(int x, int y, int width)
        {
            return y * width + x;
        }

        private float GrayScale(Color color)
        {
            return (color.R + color.G + color.B) / 3;
        }

        private List<Vector2> FindEdgesWithGraphicsPath(string text, Vector2 position)
        {
            //"♥"
            List<Vector2> points = new List<Vector2>();
            var Position = new Vector2((int)position.X, (int)position.Y);
            var font = new System.Drawing.Font("Arial", 100);

            var letterSize = temporary.MeasureString("A");

            var letterSpacing = temporary.MeasureString("AA") - letterSize * new Vector2(2, 1);

            foreach(var item in text)
            {
                var old = Position;
                
                if (item < 32 || item > 126)
                {
                    Position += letterSize * new Vector2(1, 0) + letterSpacing;
                }
                else
                {
                    Position += temporary.MeasureString(item.ToString()) * new Vector2(1, 0) + letterSpacing;
                }
                if (item == ' ') continue;

                

                points.AddRange(FindEdgesWithGraphicsPath(item.ToString(), old, font));
            }
            return points;
        }
        private List<Vector2> FindEdgesWithGraphicsPath(string text, Vector2 position, System.Drawing.Font font)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            var stupidPosition = new System.Drawing.Point((int)position.X, (int)position.Y);
            path.AddString(text, font.FontFamily, 0, font.Size, stupidPosition, System.Drawing.StringFormat.GenericDefault);


            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < path.PathPoints.Length; i++)
            {
                var current = new Vector2(path.PathPoints[i].X, path.PathPoints[i].Y);
                points.Add(current);

                if (i + 1 < path.PathPoints.Length)
                {
                    var next = new Vector2(path.PathPoints[i + 1].X, path.PathPoints[i + 1].Y);

                    var distance = Vector2.Distance(current, next);
                    if (distance < 5) continue;

                    float travelPercentage = 0.05f;
                    while (travelPercentage <= 1)
                    {
                        points.Add(Vector2.Lerp(current, next, travelPercentage));
                        travelPercentage += 0.08f;
                    }
                }
            }

            return points;
        }
        private List<Vector2> FindEdges(Rectangle boundsToCheck)
        {
            List<Vector2> points = new List<Vector2>();

            int threshhold = 10;

            var startX = boundsToCheck.X;
            var startY = boundsToCheck.Y;
            var boundsWidth = boundsToCheck.Width + boundsToCheck.X;
            var boundsHeight = boundsToCheck.Height + boundsToCheck.Y;

            var startIndex = TwoDToOneD(startX, startY, boundsWidth);
            var endIndex = TwoDToOneD(boundsWidth, boundsHeight, boundsWidth);

            Color[] colors = new Color[GraphicsDevice.Viewport.Width * GraphicsDevice.Viewport.Height];
            GraphicsDevice.GetBackBufferData(colors);

            Color[,] screen = new Color[GraphicsDevice.Viewport.Height, GraphicsDevice.Viewport.Width];
            for (int i = 0; i < colors.Length; i++)
            {
                screen[i / GraphicsDevice.Viewport.Width, i % GraphicsDevice.Viewport.Width] = colors[i];
            }

            for (int x = startX; x < boundsWidth; x++)
            {
                for (int y = startY; y < boundsHeight; y++)
                {
                    var color = screen[y, x];

                    var currentColorGrayScale = GrayScale(color);

                    if (x - 1 >= 0)
                    {
                        var grayScale = GrayScale(screen[y, x - 1]);
                        if (currentColorGrayScale - grayScale > threshhold)
                        {
                            //edge found
                            points.Add(new Vector2(x, y));
                        }
                    }
                    else if (x + 1 < boundsToCheck.Width)
                    {
                        var grayScale = GrayScale(screen[y, x + 1]);
                        if (currentColorGrayScale - grayScale > threshhold)
                        {
                            //edge found
                            points.Add(new Vector2(x, y));
                        }
                    }
                    if (y - 1 >= 0)
                    {
                        var grayScale = GrayScale(screen[y - 1, x]);
                        if (currentColorGrayScale - grayScale > threshhold)
                        {
                            //edge found
                            points.Add(new Vector2(x, y));
                        }
                    }
                    else if (y + 1 < boundsToCheck.Height)
                    {
                        var grayScale = GrayScale(screen[y + 1, x]);
                        if (currentColorGrayScale - grayScale > threshhold)
                        {
                            //edge found
                            points.Add(new Vector2(x, y));
                        }
                    }
                }
            }

            return points;
        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState mouse = Mouse.GetState();

            // TODO: Add your update logic here
            
            foreach (var edge in Edges)
            {
                edge.Behaviors(mouse.Position.ToVector2());
                edge.Update();
            }

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);


            spriteBatch.Begin();

            foreach (var item in Edges)
            {
                item.Draw(spriteBatch);
            }

            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
