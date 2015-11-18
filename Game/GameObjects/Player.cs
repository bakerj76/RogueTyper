using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Game.GameObjects.Items;
using Game.GameObjects.TextEvents;
using Game.Map;
using Game.Resources;

namespace Game.GameObjects
{
    public class Player : Sprite
    {
        private const double MoveSpeed = 10;

        private readonly KeyConverter _keyConverter;
        private Vector _movePos;
        private MovePlayerText.Directions _direction;
        private readonly TypingText _typingText;
        private bool _moving;
        private readonly MediaPlayer _typeSound;

        public String Typed { get; private set; }
        public bool TextHasChanged { get; private set; }
        public int HealthPoints { get; private set; }
        public int MaxHealthPoints { get; set; }
        public List<Item> Inventory { get; private set; } 
        
        public delegate void FinishedMovingDelegate();
        public event FinishedMovingDelegate FinishedMoving;

        public Player(Vector position) : base(position)
        {
            Typed = "";
            Revive();

            _keyConverter = new KeyConverter();
            _typingText = new TypingText(position, "", background: Brushes.Black);
            SpriteImage = ResourceLoader.Player;

            _typeSound = new MediaPlayer();
            _typeSound.Open(new Uri(@"..\..\Resources\Audio\type.mp3", UriKind.Relative));

            Inventory = new List<Item>();

            _typingText.Layer = 0.02;
            Layer = 0.01;
        }

        protected virtual void OnFinishedMoving()
        {
            _typingText.Position = Position;

            if (FinishedMoving != null)
                FinishedMoving();
        }

        public override void Input(KeyEventArgs e)
        {
            // Only do this when there's a keyup event
            if (e.IsUp)
            {
                if (e.Key == Key.Back)
                {
                    EraseTyped();
                    return;
                }

                var s = _keyConverter.ConvertToString(e.Key);

                if (!String.IsNullOrWhiteSpace(s) && s.Length == 1)
                {
                    var c = s[0];

                    if (Char.IsLetter(c))
                    {
                        ChangeText(Typed + c);
                        return;
                    }
                }
            }

            TextHasChanged = false;
        }

        private void ChangeText(string text)
        {
            _typeSound.Stop();
            _typeSound.Play();

            TextHasChanged = true;

            Typed = text.ToLower();
            _typingText.Text = Typed;

            _typingText.Position = Position;
        }

        public override void Update()
        {
            if (!_moving)
            {
                base.Update();
                return;
            }

            var diff =  _movePos - Position;

            if (_direction == MovePlayerText.Directions.Up || _direction == MovePlayerText.Directions.Down)
            {
                if (Math.Abs(diff.X) >= MoveSpeed*CompositionTargetEx.DeltaFrame)
                {
                    Position += new Vector(Math.Sign(diff.X)*MoveSpeed*CompositionTargetEx.DeltaFrame, 0);
                }
                else
                {
                    Position = new Vector(_movePos.X, Position.Y);

                    if (Math.Abs(diff.Y) >= MoveSpeed*CompositionTargetEx.DeltaFrame)
                    {
                        Position += new Vector(0, Math.Sign(diff.Y)*MoveSpeed*CompositionTargetEx.DeltaFrame);
                    }
                    else
                    {
                        DoneMoving();
                    }
                }
            }
            else
            {
                if (Math.Abs(diff.Y) >= MoveSpeed*CompositionTargetEx.DeltaFrame)
                {
                    Position += new Vector(0, Math.Sign(diff.Y)*MoveSpeed*CompositionTargetEx.DeltaFrame);
                }
                else
                {
                    Position = new Vector(Position.X, _movePos.Y);

                    if (Math.Abs(diff.X) >= MoveSpeed*CompositionTargetEx.DeltaFrame)
                    {
                        Position += new Vector(Math.Sign(diff.X)*MoveSpeed*CompositionTargetEx.DeltaFrame, 0);
                    }
                    else
                    {
                        DoneMoving();
                    }
                }
            }

            base.Update();
        }

        private void DoneMoving()
        {
            Position = _movePos;
            _moving = false;
            OnFinishedMoving();
        }

        public override void Draw()
        {
            base.Draw();

            _typingText.Draw();
        }

        public void Move(int doorX, int doorY, Room nextRoom, MovePlayerText.Directions direction)
        {
            _moving = true;
            _direction = direction;
            _movePos = new Vector(doorX, doorY);
            MainWindow.Camera.Focus(nextRoom);
        }

        public void EraseTyped()
        {
            ChangeText("");
        }

        public void Revive()
        {
            MaxHealthPoints = 100;
            HealthPoints = MaxHealthPoints;
        }

        public void Hurt(int health)
        {
            HealthPoints -= health;

            HealthPoints = HealthPoints > MaxHealthPoints ? MaxHealthPoints : HealthPoints;

            if (HealthPoints <= 0)
            {
                Kill();
            }
        }
    }
}
