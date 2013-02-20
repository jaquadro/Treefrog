using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Amphibian;
using Amphibian.Behaviors;
using Amphibian.Collision;
using Amphibian.Input;
using Amphibian.Geometry;
using LilyPath;
using Editor.Model;

namespace Editor
{
    public class AmphibianGameControl : GameControl
    {
        #region Fields

        Engine _engine;

        #endregion

        public Frame frame;
        public TileMap map;

        public int mousex;
        public int mousey;

        private float _zoom = 1f;

        //public EditorViewport _viewport;

        private bool _hover;

        public SpriteBatch SpriteBatch
        {
            get { return _engine.SpriteBatch; }
        }

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                //if (_viewport != null) {
                //    _viewport.Zoom = value;
                //}
            }
        }

        protected override void Initialize ()
        {
            base.Initialize();

            _engine = new Engine(GraphicsDeviceService);
            _engine.Content.RootDirectory = "Content";

            map = new TileMap("");

            // Frame
            frame = new Frame(_engine);
            frame.Width = 2000;
            frame.Height = 720;

            // Objects
            Frog frog = new Frog(400, 200);
            frog.Name = "frog";
            frame.AddComponent(frog);

            //_viewport = new EditorViewport(MainForm as Form1, map);
        }

        double _deltaFPS;

        protected override void Update (GameTime gameTime)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalSeconds;
            double fps = 1.0 / elapsed;
            _deltaFPS += gameTime.ElapsedGameTime.TotalSeconds;

            if (_deltaFPS > 1) {
                _deltaFPS -= 1;
                MainForm.Text = "GameLab (" + fps + ")";
            }

            _engine.Update(gameTime);

            Form1 form = MainForm as Form1;

            /*Vector2 offset = new Vector2(-form.CanvasHScroll.Value * _zoom, -form.CanvasVScroll.Value * _zoom);
            if (this.Width > map.PixelsWide * _zoom) {
                offset.X = (this.Width - map.PixelsWide * _zoom) / 2;
            }
            if (this.Height > map.PixelsHigh * _zoom) {
                offset.Y = (this.Height - map.PixelsHigh * _zoom) / 2;
            }

            int tx = (int)Math.Floor(((float)mousex - offset.X) / (map.TileWidth * _zoom));
            int ty = (int)Math.Floor(((float)mousey - offset.Y) / (map.TileHeight * _zoom));

            if (!_hover || tx < 0 || ty < 0 || tx >= map.TilesWide || ty >= map.TilesHigh) {
                form.StatusCoord.Text = "";
            }
            else {
                form.StatusCoord.Text = tx + ", " + ty;
            }

            form.Label.Text = " HV: " + form.CanvasHScroll.Value + ", " + form.CanvasVScroll.Value;
            form.Label.Text += " CSZ: " + Width + ", " + Height;
            form.Label.Text += " MSZ: " + map.PixelsWide + ", " + map.PixelsHigh;
            form.Label.Text += " DF: " + (map.PixelsWide - Width) + ", " + (map.PixelsHigh - Height);
            form.Label.Text += " HVS: " + form.CanvasHScroll.Maximum + ", " + form.CanvasVScroll.Maximum;*/
        }

        protected override void Draw (GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            //_engine.Draw(gameTime);

            Form1 form = MainForm as Form1;

            /*Vector2 offset = new Vector2(-form.CanvasHScroll.Value, -form.CanvasVScroll.Value);
            if (this.Width > map.PixelsWide) {
                offset.X = (this.Width - map.PixelsWide) / 2;
            }
            if (this.Height > map.PixelsHigh) {
                offset.Y = (this.Height - map.PixelsHigh) / 2;
            }*/
            
            //Matrix tranf = Matrix.CreateTranslation(offset.X, offset.Y, 0) * Matrix.CreateScale(_zoom);

            //_engine.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, tranf);

            //_viewport.Draw();
            //_engine.SpriteBatch.End();
        }

        protected override void OnMouseEnter (EventArgs e)
        {
            base.OnMouseEnter(e);

            _hover = true;
        }

        protected override void OnMouseLeave (EventArgs e)
        {
            base.OnMouseLeave(e);

            _hover = false;

            Form1 form = MainForm as Form1;
            form.StatusCoord.Text = "";
        }
    }

    

    public class Frog : AnimatedSpriteObject, ICollidable
    {
        private Mask _mask;

        public Frog (FPInt x, FPInt y)
            : base()
        {
            _position.X = x;
            _position.Y = y;
            //AddBehavior(new CircleMovement(this, new Vector2(x, y), 50, MathHelper.Pi));
        }

        protected override void Load ()
        {
            StaticSprite frame1 = new StaticSprite();
            StaticSprite frame2 = new StaticSprite();

            frame1.Load(Parent.Engine.Content, "Froggy", new Rectangle(0, 0, 34, 29));
            frame1.Origin = new Vector2(17, 28);
            frame2.Load(Parent.Engine.Content, "Froggy", new Rectangle(34, 0, 34, 29));
            frame2.Origin = new Vector2(18, 28);

            frame1.Scale = 2f;
            frame2.Scale = 2f;
            _sequence.Scale = 2f;

            _sequence.AddSprite(frame1, 0.5f);
            _sequence.AddSprite(frame2, 0.5f);

            _sequence.RepeatIndefinitely = true;
            _sequence.Start();

            //_mask = new CircleMask(new PointFP(8, 8), 10);
            _mask = new AABBMask(new PointFP(-16, -32), new PointFP(16, 0));
            _mask.Position = _position;

            Dictionary<PlatformAction, PlatformAction> cmap = new Dictionary<PlatformAction, PlatformAction>();
            cmap[PlatformAction.Left] = PlatformAction.Left;
            cmap[PlatformAction.Right] = PlatformAction.Right;
            cmap[PlatformAction.Up] = PlatformAction.Up;
            cmap[PlatformAction.Down] = PlatformAction.Down;
            cmap[PlatformAction.Jump] = PlatformAction.Jump;
            cmap[PlatformAction.Action] = PlatformAction.Action;

            /*PaddleMovement<PaddleAction> behavior = new PaddleMovement<PaddleAction>(this, "player", cmap);
            behavior.Origin = new Vector2(400, 200);
            behavior.Range = 100;
            behavior.Speed = 120;
            behavior.Direction = PaddleDirection.Vertical;
            AddBehavior(behavior);*/

            /*PlayerPlatformMovement<PlatformAction> behavior = new PlayerPlatformMovement<PlatformAction>(this, "player", cmap);
            behavior.AccelX = (FPInt)0.5;
            behavior.DecelX = (FPInt)0.25;
            behavior.MinVelocityX = (FPInt)(-3);
            behavior.MaxVelocityX = (FPInt)3;
            behavior.AccelY = (FPInt)0.4;
            behavior.MinVelocityY = -12;
            behavior.MaxVelocityY = 12;
            behavior.AccelStateY = PlatformAccelState.Accelerate;*/

            //AddBehavior(behavior);

            CircleMovement circle = new CircleMovement(this, new PointFP(300, 300), 150, 1);
            AddBehavior(circle);
        }

        public override void Update ()
        {
            base.Update();

            _sequence.Update(Parent.Engine.GameTime);

            //Parent.Camera.ScrollTo((int)X, (int)Y);
        }

        public override void Draw ()
        {
            base.Draw();

            //_mask.Draw(Parent.Engine.SpriteBatch);
        }

        public Mask CollisionMask
        {
            get { return _mask; }
        }
    }
}
