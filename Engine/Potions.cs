using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class HealingPotion : Item
    {
        public int AmountToHeal { get; set; }

        public HealingPotion (int id, string name, string namePlural, int amountToHeal) : base(id, name, namePlural)
        {
            AmountToHeal = amountToHeal;
        }

    }

    public class ManaPotion : Item
    {
        public int AmountToRestore { get; set; }

        public ManaPotion(int id, string name, string namePlural, int amountToRestore) : base(id, name, namePlural)
        { AmountToRestore = amountToRestore; }
    }
}
