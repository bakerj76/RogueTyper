using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Game.Resources;

namespace Game.GameObjects
{
    public class Tile : Sprite
    {
        public enum TileType
        {
            None = -1,
            Floor,
            Wall
        };

        private TileType _type;
        public TileType Type { get { return _type; } set{ SetType(value); } }

        public Tile(Vector position, TileType tileType) : base(position)
        {
            Type = tileType;

            Layer = -1;

            if (tileType != TileType.None)
                SpriteImage = ResourceLoader.Tiles[(int) tileType];

            SetupDraw();
        }

        private new void SetupDraw()
        {
            if (Type != TileType.Wall)
            {
                base.SetupDraw();
                return;
            }

            // Setup cube
            var vertices = new [] 
            {
                new Point3D(-0.5, -0.5, 0.0), 
                new Point3D( 0.5, -0.5, 0.0), 
                new Point3D(-0.5,  0.5, 0.0),
                new Point3D( 0.5,  0.5, 0.0), 
                new Point3D(-0.5, -0.5, 1.0), 
                new Point3D( 0.5, -0.5, 1.0),
                new Point3D(-0.5,  0.5, 1.0), 
                new Point3D( 0.5,  0.5, 1.0)
            };

            var faces = new[] 
            {
                1, 2, 3, 1, 0, 2, //back
                0, 6, 2, 6, 0, 4, //left
                7, 2, 6, 7, 3, 2, //top
                0, 5, 4, 5, 0, 1, //bottom
                7, 1, 3, 7, 5, 1, //right
                4, 7, 6, 4, 5, 7  //front
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

        private void SetType(TileType newType)
        {
            if (_type == newType) return;

            _type = newType;

            if (newType != TileType.None)
            {
                SetupDraw();

                SpriteImage = ResourceLoader.Tiles[(int) _type];
                Alive = true;
                Draw();
            }
            else
            {
                SpriteImage = null;
                Kill();
            }
        }

        public override void Input(KeyEventArgs e)
        {
            
        }
    }
}
