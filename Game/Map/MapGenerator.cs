using System;
using System.Windows;

namespace Game.Map
{
    #region MapGenerator Class
    public class MapGenerator
    {
        private const int MaxTries = 1000;
        private readonly int _minRoomSize;
        private readonly float _removeProbability;

        public Room Root { get; private set; }
        private int[,] _tileMap;
        public int[,] TileMap 
        { 
            get { return _tileMap; }
        }
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary> Creates the BSP-generated map of the level. </summary>
        /// <param name="width">The width of the level.</param>
        /// <param name="height">The height of the level.</param>
        /// <param name="minimumRoomSize">The minimum room size a room can be horizontally and vertically.</param>
        /// <param name="seed">The random seed to generate the room with</param>
        /// <param name="probabilityToRemove">The probability of marking a room as removed</param>
        public MapGenerator(int width, int height, int minimumRoomSize, int seed = 100, float probabilityToRemove = 0.1f)
        {
            Width = width;
            Height = height;

            _minRoomSize = minimumRoomSize;
            _removeProbability = probabilityToRemove;

            StaticRandom.SetSeed(seed);
        }

        private static void SetupTileMap(int[,] tileMap)
        {
            for (var y = 0; y < tileMap.GetLength(1); y += tileMap.GetLength(1) - 1)
                for (var x = 0; x < tileMap.GetLength(0); x++)
                    tileMap[x, y] = 1;

            for (var y = 0; y < tileMap.GetLength(1); y++)
                for (var x = 0; x < tileMap.GetLength(0); x += tileMap.GetLength(0) - 1)
                    tileMap[x, y] = 1;
        }

        /// <summary> Creates all of the rooms on the map. </summary>
        /// <param name="splits"> The amount of times a room should be split. </param>
        public void CreateRooms(int splits)
        {
            Root = new Room(0, 0, Width, Height, null);
            _tileMap = new int[Width, Height];

            SetupTileMap(_tileMap);
            RecursiveSplit(splits, Root);
            GenerateTileMap(Root);
        }

        /// <summary> Recursively creates the BSPs. </summary>
        /// <param name="splits"> The current number of remaining splits a room can have. </param>
        /// <param name="node"> The room to be split. </param>
        private void RecursiveSplit(int splits, Room node, int enumeration = 0)
        {
            // Avoid an infinite loop
            if (enumeration >= 1000)
            {
                return;
            }

            // Get the x and y-ranges.
            var xrange1 = (int)node.RoomRect.Left + _minRoomSize;
            var xrange2 = (int)node.RoomRect.Right - _minRoomSize;
            var yrange1 = (int)node.RoomRect.Top + _minRoomSize;
            var yrange2 = (int)node.RoomRect.Bottom - _minRoomSize;

            // Check if the room is already too small to cut.
            if (splits <= 0 || (xrange1 > xrange2 && yrange1 > yrange2))
            {
                node.Deleted = StaticRandom.Random.NextDouble() <= _removeProbability;
                return;
            }

            // Initialize variables.
            int pos, door;
            Room.CutDirections dir;

            // Cut horizontally if the room is too small horizontally.
            if (xrange1 > xrange2)
            {
                dir = Room.CutDirections.Horizontal;
            }
            // Cut vertically if the room is too small vertically.
            else if (yrange1 > yrange2)
            {
                dir = Room.CutDirections.Vertical;
            }
            // Cut randomly if the room is large enough.
            else
            {
                var length = Enum.GetValues(typeof(Room.CutDirections)).Length;
                dir = (Room.CutDirections)StaticRandom.Random.Next(0, length);
            }

            // Depending on the cut direction, set up where the cut will be and where the door will be.
            if (dir == Room.CutDirections.Horizontal)
            {
                var tries = 0; // If you can't find a split after 1000 tries, guessing is probably OK
                do
                {
                    pos = StaticRandom.Random.Next(yrange1, yrange2);
                    tries++;
                } while (node.Parent != null && (IsOnDoor(node.Parent, dir, pos)) && tries < MaxTries);

                door = StaticRandom.Random.Next((int) node.RoomRect.Left + 1, (int) node.RoomRect.Right - 1); 
            }
            else
            {
                var tries = 0;
                do
                { 
                    pos = StaticRandom.Random.Next(xrange1, xrange2);
                    tries++;
                } while (node.Parent != null && (IsOnDoor(node.Parent, dir, pos)) && tries < MaxTries);

                door = StaticRandom.Random.Next((int)node.RoomRect.Top + 1, (int)node.RoomRect.Bottom - 1);
            }

            // Split the room.
            node.SplitRoom(dir, new Vector(pos, pos), door);
            // Recursively cut the room's children.
            RecursiveSplit(splits - 1, node.ChildA, enumeration + 1);
            RecursiveSplit(splits - 1, node.ChildB, enumeration + 1);
        }

        private void GenerateTileMap(Room node)
        {
            if (node == null) return;

            if (node.Transparent)
            {
                GenerateTileMap(node.ChildA);
                GenerateTileMap(node.ChildB);
            }
            else
            {
                node.PlaceRoom(ref _tileMap);
            }
        }

        private bool IsOnDoor(Room current, Room.CutDirections dir, int pos)
        {
            if (current == null) return false;

            if (dir != current.Split.Direction && current.Split.Door == pos)
            {
                return true;
            }
            
            return IsOnDoor(current.Parent, dir, pos);
        }

        public Room GetRoom(Point tile)
        {
            tile += new Vector(1, 1);

            var node = Root;

            if (!node.RoomRect.Contains(tile)) return null;

            while (node.Transparent)
            {
                node = node.ChildA.RoomRect.Contains(tile) ? node.ChildA : node.ChildB;
            }

            return node;
        }
    }
    #endregion
}
