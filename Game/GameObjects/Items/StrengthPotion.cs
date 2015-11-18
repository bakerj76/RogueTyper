using System.Windows;
using Game.Map;

namespace Game.GameObjects.Items
{
    class StrengthPotion : Item
    {
        public StrengthPotion(Vector position, Player player, Level level) :
            base(position, player, level, "STRENGTHPOTION")
        {

        }

        public override void Activate()
        {
            Player.MaxHealthPoints += 5;
        }
    }
}
