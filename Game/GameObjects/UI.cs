using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Game.GameObjects.Items;
using Game.GameObjects.TextEvents;
using Game.Map;

namespace Game.GameObjects
{
    public class UI : Sprite
    {
        private const double DistanceFromCamera = 5;

        private readonly Player _player;
        private readonly Level _level;


        private readonly List<Sprite> _uiElements;

        private readonly TypingText _health;
        private readonly TypingText _depth;
        private readonly TypingText _wpm;
        private readonly TypingText _score;

        // Inventory is a WIP!
        //private List<TextEvent> _inventory;

        public UI(Player player, Level level)
        {
            _uiElements = new List<Sprite>();

            _player = player;
            _level = level;

            _health = new TypingText(new Vector(3, 2), "HP: " + player.HealthPoints + "/" + player.MaxHealthPoints, Brushes.Red, Brushes.Black);
            _depth = new TypingText(new Vector(3, 1.5), "Depth: " + level.Depth, Brushes.Red, Brushes.Black);
            _wpm = new TypingText(new Vector(3, 1), "WPM: " + level.WPM, Brushes.Red, Brushes.Black);
            _score = new TypingText(new Vector(-3, -2), "Score: " + level.Score, Brushes.Red, Brushes.Black);

            //_inventory = new List<TextEvent>();
            //CloseInventory();

            _uiElements.Add(_health);
            _uiElements.Add(_depth);
            _uiElements.Add(_wpm);
            _uiElements.Add(_score);
        }

        /*private void CloseInventory()
        {
            foreach (var item in _inventory)
            {
                _uiElements.Remove(item);
                item.Kill();
            }

            _inventory = new List<TextEvent> { new TextEvent(new Vector(-3, 0), _player, "Inventory") };
            _inventory[0].Matched += OpenInventory;

            _uiElements.Add(_inventory[0].TypingText);
        }

        private void OpenInventory()
        {
            foreach (var item in _inventory)
            {
                item.Kill();
            }

            _inventory = new List<TextEvent>();

            for (var i = 0; i < _player.Inventory.Count; i++)
            {
                var item = _player.Inventory[i];
                var textEvent = new TextEvent(new Vector(-3, i*0.5), _player, item.Text);
                textEvent.Matched += item.Activate;
                textEvent.Matched += CloseInventory;

                _inventory.Add(textEvent);
                _uiElements.Add(textEvent.TypingText);
            }
        }*/

        

        public override void Draw()
        {
            base.Draw();
            foreach (var element in _uiElements)
            {
                if (!element.Alive) continue;
                element.Draw();
            }
        }

        public override void Input(KeyEventArgs e)
        {
            foreach (var element in _uiElements)
            {
                if (!element.Alive) continue;
                element.Input(e);
            }
        }

        public override void Update()
        {
            var right = Vector3D.CrossProduct(MainWindow.Camera.LookDirection, new Vector3D(0, 1, 0));
            var up = Vector3D.CrossProduct(MainWindow.Camera.LookDirection, right);
            var uiPlane = (Vector3D)MainWindow.Camera.Position3D + MainWindow.Camera.LookDirection * DistanceFromCamera;

            _health.Text = "HP: " + _player.HealthPoints + "/" + _player.MaxHealthPoints;
            _depth.Text = "Depth: " + _level.Depth;
            _wpm.Text = "WPM: " + (int)_level.WPM;
            _score.Text = "Score: " + _level.Score;

            foreach (var element in _uiElements)
            {
                if (!element.Alive || element.SpriteModel == null) continue;

                var pos = uiPlane + right*element.Position.X + up*element.Position.Y;

                var transGroup = new Transform3DGroup();
                transGroup.Children.Add(new ScaleTransform3D(new Vector3D(0.75, 0.375, 1)));
                transGroup.Children.Add(new TranslateTransform3D(pos));
                element.SpriteModel.Transform = transGroup;
            }
        }

        public void Revive()
        {
            Alive = true;
            foreach (var element in _uiElements)
            {
                if (!element.Alive) continue;
                element.Alive = true;
            }
        }

        public override void Kill()
        {
            foreach (var element in _uiElements)
            {
                if (!element.Alive) continue;
                element.Kill();
            }
            base.Kill();
        }
    }
}
