using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Game.GameObjects.TextEvents
{
    public class TextEvent : Sprite
    {
        protected readonly Player Player;
        public readonly TypingText TypingText;

        public string Text { get; private set; }

        public delegate void MatchedDelegate();
        public event MatchedDelegate Matched;

        public TextEvent(Vector position, Player player, string text) : base(position)
        {
            Text = text;
            Player = player;

            TypingText = new TypingText(position, Text, background: Brushes.Black);

            Matched += Kill;
            Matched += Player.EraseTyped;
        }

        protected virtual void OnMatched()
        {
            if (Matched != null)
                Matched();
        }

        public override void Input(System.Windows.Input.KeyEventArgs e)
        {
            if (!Alive || !Player.TextHasChanged) return;

            if (String.IsNullOrWhiteSpace(Player.Typed))
            {
                TypingText.ColoredCharacters = 0;
                return;
            }

            var match = TypingText.CheckMatch(Player.Typed);
            TypingText.ColoredCharacters = match < Player.Typed.Length ? 0 : match;

            if (match == Text.Length)
            {
                OnMatched();
            }
        }

        public override void Update()
        {
            TypingText.Update();
            base.Update();
        }


        public override void Kill()
        {   
            TypingText.Kill();
            base.Kill();
        }

        public override void Draw()
        {
            base.Draw();

            if (!Alive) return;

            TypingText.Alive = true;
            TypingText.Draw();
        }
    }
}
