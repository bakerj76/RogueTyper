using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Game.GameObjects
{
    public abstract class Sprite
    {
        protected MeshGeometry3D Mesh;
        private Vector _lastPosition;
        private Vector _lastScale;
        private bool _redraw;

        public Vector Position { get; set; }
        public Vector Scale { get; set; }
        public Material SpriteImage { get; set; }
        public ModelVisual3D SpriteModel { get; set; }
        public double Layer { get; set; }
        public bool Alive { get; set; }

        protected Sprite()
        {
            Position = new Vector();
            Scale = new Vector(1, 1);
            SetupDraw();

            Alive = true;
        }

        protected Sprite(Vector position)
        {
            Position = position;
            Scale = new Vector(1, 1);
            SetupDraw();

            Alive = true;
        }

        protected Sprite(Vector position, int layer) : this(position)
        {
            Layer = layer;
        }

        protected void SetupDraw()
        {
            // Set up a quad
            var vertices = new[]
                           {
                               new Point3D(-0.5, -0.5, 0.0),
                               new Point3D( 0.5, -0.5, 0.0),
                               new Point3D( 0.5,  0.5, 0.0),
                               new Point3D(-0.5,  0.5, 0.0),
                           };

            var faces = new[] 
                        {
                            0, 1, 2, 
                            0, 2, 3,
                        };

            var textCoords = new[]
                             {
                                 new Point(0, 1), new Point(1, 1),
                                 new Point(1, 0), new Point(0, 0),
                             };

            Mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection(vertices),
                TriangleIndices = new Int32Collection(faces),
                TextureCoordinates = new PointCollection(textCoords),
            };
        }

        /// <summary>
        /// Handles user input
        /// </summary>
        public abstract void Input(KeyEventArgs e);

        /// <summary>
        /// Handles the object updating
        /// </summary>
        public virtual void Update()
        {
            if (SpriteModel == null) return;
            if (Position == _lastPosition && Scale == _lastScale && !_redraw) return;

            var transGroup = new Transform3DGroup();
            transGroup.Children.Add(new ScaleTransform3D(new Vector3D(Scale.X, Scale.Y, 1)));
            transGroup.Children.Add(new TranslateTransform3D(new Vector3D(Position.X, Position.Y, Layer)));

            SpriteModel.Transform = transGroup;

            _lastPosition = Position;
            _lastScale = Scale;
        }

        /// <summary>
        /// Sets up a quad to draw the sprite on
        /// </summary>
        public virtual void Draw()
        {
            if (!Alive) return;

            if (SpriteModel != null) MainWindow.Viewport.Children.Remove(SpriteModel);

            SpriteModel = new ModelVisual3D
                          {
                              Content = new GeometryModel3D(Mesh, SpriteImage),
                          };

            _redraw = true;
            Update();
            _redraw = false;

            MainWindow.Viewport.Children.Add(SpriteModel);
        }

        public virtual void Kill()
        {
            Alive = false;
            MainWindow.Viewport.Children.Remove(SpriteModel);
        }
    }
}
