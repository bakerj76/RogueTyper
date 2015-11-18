using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Game.GameObjects;
using Game.GameObjects.Items;
using Game.GameObjects.TextEvents;
using Game.Managers;
using Game.Resources;

namespace Game.Map
{
    public class Level
    {
        private const double DropRate = 0.05;

        private readonly GameObjectManager _manager;
        private readonly Stopwatch _wpmTimer;
        private readonly UI _ui;

        private int[,] _map;
        private List<Vector> _levelDoors;
        private TextEvent _stairs;
        private int _currentEnemies;
        private int _wpmCalcs;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte Depth { get; private set; }
        public double WPM { get; private set; }
        public bool GameOver { get; private set; }
        public int Score { get; private set; }

        public Room Start { get; private set; }
        public Room End { get; private set; }
        public Room CurrentRoom { get; private set; }

        public MapGenerator Map { get; private set; }
        public Player Player { get; private set; }

        public List<TextEvent> TextEvents { get; private set; }


        public Level(int width, int height, GameObjectManager manager)
        {
            Width = width;
            Height = height;
            _manager = manager;
            _levelDoors = new List<Vector>();
            TextEvents = new List<TextEvent>();
            GameOver = true;

            Player = new Player(new Vector());
            Player.FinishedMoving += SetupRoom;

            _ui = new UI(Player, this);
            _wpmTimer = new Stopwatch();

            _map = new int[width, height];
        }

        public void StartGame(int seed = -1)
        {
            GameOver = false;

            Depth = 1;
            _wpmCalcs = 0;
            WPM = 0;
            Score = 0;
            
            _manager.ClearObjects();
            _manager.AddUI(_ui);

            Map = new MapGenerator(Width, Height, 8, seed < 0 ? new Random().Next() : seed);

            SetupMap();
        }

        public void SetupMap()
        {
            _wpmTimer.Stop();
            _wpmTimer.Reset();

            RemoveEverything();

            Map.CreateRooms(5);

            Start = GetRandomRoom(Map.Root);
            End = GetRandomRoom(Map.Root);
            CurrentRoom = Start;

            Player.Revive();
            Player.Position = Start.GetRandomPointInRoom();
            _manager.AddObject(Player);
            Player.Draw();

            MainWindow.Camera.Focus(Start);

            _stairs = new TextEvent(End.GetRandomPointInRoom(), Player, "Escape");
            _stairs.Matched += NextFloor;
            _manager.AddObject(_stairs);
            _stairs.Kill();

            _ui.Revive();
            _ui.Draw();
            
            SpawnEnemies();
            SetupRoom();

            _manager.Draw();
        }

        private void RemoveEverything()
        {
            _manager.ClearObjects();
            if (_stairs != null) _stairs.Kill();
            RemoveTextEvents(true);
        }

        public void SetupRoom()
        {
            SetupTiles();

            foreach (var item in CurrentRoom.Items)
            {
                item.TypingText.Enable();
                item.Draw();

                item.Alive = true;
                TextEvents.Add(item);
            }

            if (CurrentRoom.Enemies.Count > 0)
            {
                _currentEnemies = CurrentRoom.Enemies.Count;
                _wpmTimer.Start();

                CurrentRoom.SpawnEnemies(Player, _manager);
            }
            else
            {
                DrawDoors();
            }
        }

        public void MovePlayer(int doorX, int doorY, Room nextRoom, MovePlayerText.Directions direction)
        {
            RemoveTextEvents();

            CurrentRoom = nextRoom;
            Player.Move(doorX, doorY, nextRoom, direction);            
        }

        public void NextFloor()
        {
            _manager.RemoveObject(_stairs);
            Depth++;
            SetupMap();
        }

        private void SetupTiles()
        {
            for (var y = (int)CurrentRoom.RoomRect.Top; y < CurrentRoom.RoomRect.Top + CurrentRoom.RoomRect.Height; y++)
            {
                for (var x = (int)CurrentRoom.RoomRect.Left; x < CurrentRoom.RoomRect.Left + CurrentRoom.RoomRect.Width; x++)
                {
                    _manager.AddTile(x, y, (Tile.TileType)Map.TileMap[x, y]);
                }
            }
        }

        private void SpawnEnemies()
        {
            var numEnemies = StaticRandom.Random.Next(Depth*2 + 10, Depth*4 + 10);

            for (var i = 0; i < numEnemies; i++)
            {
                var room = GetRandomRoom(Map.Root);

                // Make sure it's not the starting room
                while (room == Start)
                {
                    room = GetRandomRoom(Map.Root);
                }

                byte minDifficulty = Depth - 3 < 0 ? (byte)0 : (byte)(Depth - 3);
                var enemy = new Enemy(new Vector(-1000, -1000),
                                      Player, 
                                      ResourceLoader.GetRandomWord(minDifficulty, Depth), 
                                      Depth);

                enemy.Death += OnEnemyDeath;
                enemy.Alive = false;

                room.Enemies.Add(enemy);
            }
        }

        private void OnEnemyDeath(Enemy thisEnemy)
        {
            if (GameOver) return;

            CurrentRoom.Enemies.Remove(thisEnemy);
            if (CurrentRoom.Enemies.Count <= 0)
            {
                var minutes = _wpmTimer.Elapsed.TotalMinutes;
                _wpmTimer.Stop();
                _wpmTimer.Reset();

                _wpmCalcs++;

                if (minutes > 0)
                {
                    WPM += (_currentEnemies/minutes - WPM)/_wpmCalcs; // Get the running sum
                    Score += (int)(_currentEnemies/minutes);
                }

                DrawDoors();
            }

            if (StaticRandom.Random.NextDouble() <= DropRate)
            {
                Item item;

                if (StaticRandom.Random.NextDouble() <= 0.75)
                {
                    item = new HealthPotion(thisEnemy.Position, Player, this);
                }
                else
                {
                    item = new StrengthPotion(thisEnemy.Position, Player, this);
                }

                CurrentRoom.Items.Add(item);
                TextEvents.Add(item);
                _manager.AddObject(item);
                item.Draw();
            }

            if (Player.HealthPoints > 0) return;

            GameOver = true;

            if (CurrentRoom.Enemies.Count > 0)
            {
                foreach (var enemy in CurrentRoom.Enemies)
                {
                    enemy.Kill();
                }
            }

            RemoveEverything();
            _ui.Kill();
        }

        private void DrawDoors()
        {
            GetDoors();

            foreach (var door in _levelDoors)
            {
                var textEvent = new MovePlayerText(door, Player, MovePlayerText.DirectionEnum(CurrentRoom, door), this);

                TextEvents.Add(textEvent);
                _manager.AddObject(textEvent);
                textEvent.Draw();
            }

            if (CurrentRoom == End)
            {
                _manager.AddObject(_stairs);
                TextEvents.Add(_stairs);
                _stairs.Draw();
                _stairs.TypingText.Enable();
                _stairs.Alive = true;
            }
        }

        private void RemoveTextEvents(bool newMap = false)
        {
            foreach (var t in TextEvents)
            {
                if (t == _stairs || (t is Item && !newMap))
                {
                    t.TypingText.Disable();
                    t.Draw();
                    t.Alive = false;
                    continue;
                }

                t.Kill();
                _manager.RemoveObject(t);
            }

            TextEvents = new List<TextEvent>();
            _levelDoors = new List<Vector>();
        }

        private static Room GetRandomRoom(Room node)
        {
            if (!node.Transparent) return node;

            var aOrB = StaticRandom.Random.Next(2);
            return aOrB == 0 ? GetRandomRoom(node.ChildA) : GetRandomRoom(node.ChildB);
        }

        private void GetDoors()
        {
            for (var y = (int)CurrentRoom.RoomRect.Y; y < CurrentRoom.RoomRect.Y + CurrentRoom.RoomRect.Height; y++)
            {
                for (var x = (int)CurrentRoom.RoomRect.X; x < CurrentRoom.RoomRect.X + CurrentRoom.RoomRect.Width; x++)
                {
                    if (CurrentRoom.GetDoor(x, y))
                        _levelDoors.Add(new Vector(x, y));
                }
            }
        }
    }
}
