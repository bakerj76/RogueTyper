using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Game.Map;

namespace Game.GameObjects.TextEvents
{
    public class MovePlayerText : TextEvent
    {
        private readonly Level _level;
        private readonly Directions _direction;

        public enum Directions { Up, Down, Left, Right }

        public MovePlayerText(Vector position, Player player, Directions direction, Level level) : base(position, player, GetDirection(direction))
        {
            _level = level;
            _direction = direction;

            Matched += MovePlayer;
        }

        private void MovePlayer()
        {
            var next = NextPosition(Position, _direction);
            _level.MovePlayer((int)next.X, (int)next.Y, _level.Map.GetRoom(next), _direction);
        }

        public static string GetDirection(Directions dir)
        {
            switch (dir)
            {
                case Directions.Up:
                    return "Up";
                case Directions.Down:
                    return "Down";
                case Directions.Left:
                    return "Left";
                case Directions.Right:
                    return "Right";
                default:
                    return "---";
            }
        }

        public static Directions DirectionEnum(Room room, Vector door)
        {
            if ((int)door.X == (int)room.RoomRect.X)
            {
                return Directions.Left;
            }

            if ((int)door.X == (int)(room.RoomRect.X + room.RoomRect.Width - 1))
            {
                return Directions.Right;
            }

            if ((int)door.Y == (int)room.RoomRect.Y)
            {
                return Directions.Down;
            }
            
            return Directions.Up;
        }

        public static Point NextPosition(Vector roomEntrance, Directions direction)
        {
            switch (direction)
            {
                case Directions.Up:
                    return (Point)(roomEntrance + new Vector(0, 2));
                case Directions.Down:
                    return (Point)(roomEntrance + new Vector(0, -2));
                case Directions.Left:
                    return (Point)(roomEntrance + new Vector(-2, 0));
                case Directions.Right:
                    return (Point)(roomEntrance + new Vector(2, 0));
                default:
                    return (Point)roomEntrance;
            }
        }
    }
}
