using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Game.GameObjects;

namespace Game.Managers
{
    public class GameObjectManager
    {
        private readonly Queue<Sprite> _queuedAdd;
        private readonly Queue<Sprite> _queuedRemove;
        private bool _markForDraw;

        public Tile[,] Tiles { get; private set; }
        private readonly List<Sprite> _gameObjectList;
        private UI _ui;

        public GameObjectManager(int levelWidth, int levelHeight)
        {
            _queuedAdd = new Queue<Sprite>();
            _queuedRemove = new Queue<Sprite>();
            _markForDraw = false;

            _gameObjectList = new List<Sprite>();
            Tiles = new Tile[levelWidth, levelHeight];

            for (var y = 0; y < levelHeight; y++)
            {
                for (var x = 0; x < levelWidth; x++)
                {
                    Tiles[x, y] = new Tile(new Vector(x, y), Tile.TileType.None);
                }
            }
        }

        public void AddTile(int posX, int posY, Tile.TileType type)
        {
            var temp = Tiles[posX, posY];
            temp.Type = type;
        }

        public Tile GetTile(int posX, int posY)
        {
            return Tiles[posX, posY];
        }

        public void AddUI(UI ui)
        {
            _ui = ui;
        }

        public void ClearObjects()
        {
            foreach (var tile in Tiles)
            {
                tile.Type = Tile.TileType.None;
            }

            foreach (var obj in _gameObjectList)
            {
                obj.Kill();
                RemoveObject(obj);
            }
        }

        public void AddObject(Sprite obj)
        {
            obj.Alive = true;
            _queuedAdd.Enqueue(obj);
        }

        public void RemoveObject(Sprite obj)
        {
            _queuedRemove.Enqueue(obj);
        }

        public void Update()
        {
            AddFromQueue();

            if (_markForDraw)
            {
                ActuallyDraw();
                _markForDraw = false;
            }
            
            if (_ui != null) _ui.Update();
            foreach (var obj in _gameObjectList)
            {
                obj.Update();
            }
        }

        public void Input(KeyEventArgs e)
        {
            AddFromQueue();

            if (_ui != null) _ui.Input(e);
            foreach (var obj in _gameObjectList)
            {
                obj.Input(e);
            }
        }

        public void Draw()
        {
            _markForDraw = true;
        }

        private void ActuallyDraw()
        {
            if(_ui != null) _ui.Draw();
            foreach (var obj in _gameObjectList)
            {
                obj.Draw();
            }
        }

        private void AddFromQueue()
        {
            while (_queuedAdd.Count > 0)
                _gameObjectList.Add(_queuedAdd.Dequeue());

            while (_queuedRemove.Count > 0)
                _gameObjectList.Remove(_queuedRemove.Dequeue());
        }
    }
}
