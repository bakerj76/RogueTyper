using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Game.GameObjects.TextEvents;
using Game.Map;

namespace Game.GameObjects.Items
{
    public abstract class Item : TextEvent
    {
        protected Level CurrentLevel;

        protected Item(Vector position, Player player, Level level, string text) : base(position, player, text)
        {
            CurrentLevel = level;

            //Inventory is a WIP!
            //Matched += PickUp;
            Matched += Activate;

            var pickupSound = new MediaPlayer();
            pickupSound.Open(new Uri(@"..\..\Resources\Audio\pickup.mp3", UriKind.Relative));

            Matched += pickupSound.Stop;
            Matched += pickupSound.Play;
        }

        public abstract void Activate();

        public void PickUp()
        {
            Player.Inventory.Add(this);
            CurrentLevel.CurrentRoom.Items.Remove(this);
        }

        public override void Draw()
        {
            base.Draw();

            TypingText.Alive = true;
            TypingText.Draw();
        }
    }
}
