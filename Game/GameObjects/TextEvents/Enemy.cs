using System;
using System.Windows;
using System.Windows.Media;

namespace Game.GameObjects.TextEvents
{
    public class Enemy : TextEvent
    {
        private const double BaseSpeed = 0.5;
        private const double DepthMultiplier = 0.1;
        private const double HitDistance = 0.5;
        
        private readonly int _depth;
        private readonly MediaPlayer _killSound;

        public int Damage { get; private set; }

        public delegate void DeathDelegate(Enemy thisEnemy);
        public event DeathDelegate Death;

        public Enemy(Vector position, Player player, string text, int depth) : base(position, player, text)
        {
            _depth = depth;
            Damage = 33;
            TypingText.Layer = 0.5;

            _killSound = new MediaPlayer();
            _killSound.Open(new Uri(@"..\..\Resources\Audio\kill.mp3", UriKind.Relative));

            Matched += _killSound.Stop;
            Matched += _killSound.Play;
        }

        public void OnDeath(Enemy thisEnemy)
        {
            if (Death != null)
                Death(this);
        }

        public override void Update()
        {
            if (!Alive) return;

            var diff = Player.Position - Position;

            if (diff.Length < HitDistance)
            {
                var damage = Text.Length - TypingText.CheckMatch(Player.Typed);
                Player.Hurt(_depth*damage);
                Kill();
                return;
            }

            diff.Normalize();

            Position += diff*(BaseSpeed + _depth*DepthMultiplier) * CompositionTargetEx.DeltaFrame;
            TypingText.Position = Position;
            
            base.Update();
        }

        public override void Kill()
        {
            OnDeath(this);
            base.Kill();
        }
    }
}
