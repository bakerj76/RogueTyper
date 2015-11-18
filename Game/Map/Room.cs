using System.Collections.Generic;
using System.Windows;
using Game.GameObjects;
using Game.GameObjects.Items;
using Game.GameObjects.TextEvents;
using Game.Managers;

namespace Game.Map
{
    #region Room Class
    public class Room
    {
        private readonly bool[,] _doors;
        private readonly bool[,] _occupied;

        /// <summary> The directions that the room can be split. </summary> 
        public enum CutDirections { Horizontal, Vertical };

        /// <summary> The way the room is cut. </summary>
        public struct Cut
        {
            /// <summary> The direction of the cut. </summary>
            public CutDirections Direction { get; set; }
            /// <summary> The position of the cut. </summary>
            public Vector Position { get; set; }
            /// <summary> The position of the door in the cut. </summary>
            public int Door;

        }

        /// <summary> The position and the dimensions of the room. </summary>
        public Rect RoomRect { get; private set; }
        /// <summary> How this room is cut. </summary>
        public Cut Split { get; private set; }

        /// <summary> Gets a value indicating whether this room has a split. </summary>
        /// <value><c>true</c> if it does not have a split; otherwise, <c>false</c>.</value>
        public bool Transparent { get; private set; }
        /// <summary> Sets if the room will even show up. </summary>
        public bool Deleted { get; set; }

        /// <summary> Gets the parent room containing this room. </summary>
        public Room Parent { get; private set; }
        /// <summary> Gets the up/left child room of this room if it is transparent. </summary>
        public Room ChildA { get; private set; }
        /// <summary> Gets the down/right child room of this room if it is transparent. </summary>
        public Room ChildB { get; private set; }

        /// <summary> The enemies in the room. </summary>
        public List<Enemy> Enemies { get; private set; }

        /// <summary> A list of items in the room. </summary>
        public List<Item> Items { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Room"/> class.
        /// </summary>
        /// <param name="x">The top-left x coordinate.</param>
        /// <param name="y">The top-left y coordinate.</param>
        /// <param name="width">The width of the room.</param>
        /// <param name="height">The height of the room.</param>
        /// <param name="parent">The parent of this room.</param>
        public Room(int x, int y, int width, int height, Room parent)
        {
            _doors = new bool[width + 1, height + 1];
            _occupied = new bool[width, height];

            RoomRect = new Rect(x, y, width, height);
            Parent = parent;

            Enemies = new List<Enemy>();
            Items = new List<Item>();

            Transparent = false;
        }

        /// <summary> Split the room with a direction and position. </summary>
        /// <param name="direction"> The direction of the split. </param>
        /// <param name="position"> Where the split is located. </param>
        /// <param name="door"> Where the door is located on the split. </param>
        public void SplitRoom(CutDirections direction, Vector position, int door)
        {
            Split = new Cut { Direction = direction, Position = position, Door = door };
            Transparent = true;

            if (direction == CutDirections.Horizontal)
            {
                var x = (int)RoomRect.Left;
                var y1 = (int)RoomRect.Top;
                var y2 = (int)Split.Position.Y;

                var width = (int)RoomRect.Width;
                var height1 = (int)(Split.Position.Y - RoomRect.Top + 1);
                var height2 = (int)(RoomRect.Bottom - Split.Position.Y);

                ChildA = new Room(x, y1, width, height1, this);
                ChildB = new Room(x, y2, width, height2, this);

                ChildA.AddDoor(door, (int)position.Y);
                ChildB.AddDoor(door, (int)position.Y);
            }
            else
            {
                var x1 = (int)RoomRect.Left;
                var x2 = (int)Split.Position.X;
                var y = (int)RoomRect.Top;

                var width1 = (int)(Split.Position.Y - RoomRect.Left + 1);
                var width2 = (int)(RoomRect.Right - Split.Position.X);
                var height = (int)RoomRect.Height;

                ChildA = new Room(x1, y, width1, height, this);
                ChildB = new Room(x2, y, width2, height, this);

                ChildA.AddDoor((int)position.X, door);
                ChildB.AddDoor((int)position.X, door);
            }

            // Add parent doors to room
            var parent = this;

            while (parent != null)
            {
                for (var y = (int)parent.RoomRect.Y; y < parent.RoomRect.Y + parent.RoomRect.Height; y++)
                {
                    for (var x = (int)parent.RoomRect.X; x < parent.RoomRect.X + parent.RoomRect.Width; x++)
                    {
                        if (!parent.GetDoor(x, y)) continue;

                        ChildA.AddDoor(x, y);
                        ChildB.AddDoor(x, y);
                    }
                }

                parent = parent.Parent;
            }
        }

        /// <summary> Places the split between the rooms onto the tile map. </summary>
        /// <param name="map">The tile map.</param>
        public void PlaceRoom(ref int[,] map)
        {
            for (var y = (int)RoomRect.Y; y < RoomRect.Y + RoomRect.Height; y += (int)RoomRect.Height - 1)
            {
                for (var x = (int)RoomRect.X; x < RoomRect.X + RoomRect.Width; x++)
                {
                    if (!GetDoor(x, y))
                        map[x, y] = 1;
                }
            }

            for (var y = (int)RoomRect.Y; y < RoomRect.Y + RoomRect.Height; y++)
            {
                for (var x = (int)RoomRect.X; x < RoomRect.X + RoomRect.Width; x += (int)RoomRect.Width - 1)
                {
                    if (!GetDoor(x, y))
                        map[x, y] = 1;
                }
            }
        }

        public Vector GetMiddle()
        {
            return new Vector(RoomRect.X + RoomRect.Width / 2, RoomRect.Y + RoomRect.Height / 2);
        }

        public Vector GetRandomPointInRoom()
        {
            return new Vector(RoomRect.X + 1 + StaticRandom.Random.Next((int)RoomRect.Width - 2),
                              RoomRect.Y + 1 + StaticRandom.Random.Next((int)RoomRect.Height - 2));
        }

        public Vector GetRandomPointInDistance(Vector origin, double minDistance)
        {
            var point = GetRandomPointInRoom();

            while ((point - origin).Length < minDistance)
            {
                point = GetRandomPointInRoom();
            }

            return point;
        }

        private void AddDoor(int x, int y)
        {
            if (RoomRect.Contains(x, y))
            {
                _doors[x - (int)RoomRect.X, y - (int)RoomRect.Y] = true;
            }
        }

        public bool GetDoor(int x, int y)
        {
            if (RoomRect.Contains(x, y))
            {
                return _doors[x - (int)RoomRect.X, y - (int)RoomRect.Y];
            }

            return false;
        }

        public void AddEnemy(Enemy enemy)
        {
            if (Enemies.Count >= (RoomRect.Width - 2)*(RoomRect.Height - 2)) return;

            Enemies.Add(enemy);
            // Prevent Overlap
            enemy.Layer -= Enemies.Count*0.1;
        }

        public void SpawnEnemies(Player player, GameObjectManager manager)
        {
            var temp = new Enemy[Enemies.Count];
            Enemies.CopyTo(temp);

            foreach (var enemy in temp)
            {
                var position = GetRandomPointInDistance(player.Position, 4);
                var relativePos = position - (Vector)RoomRect.TopLeft;

                if (_occupied[(int) relativePos.X, (int) relativePos.Y])
                {
                    enemy.Kill();
                    continue;
                }

                _occupied[(int) relativePos.X, (int) relativePos.Y] = true;
                enemy.Position = position;
                manager.AddObject(enemy);
                enemy.Draw();
            }
        }
    }
    #endregion
}
