using System.Windows;
using Game.Map;

namespace Game.GameObjects.Items
{
    class HealthPotion : Item
    {
        public HealthPotion(Vector position, Player player, Level level) : 
            base(position, player, level, "HEALTHPOTION")
        {

        }

        public override void Activate()
        {
            Player.Hurt(-25);
        }
    }
}
