using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Spell : Weapon
    {
        public int ManaCost;
        public int LevelRequirement;
        public Spell(int minimumDamage, int maximumDamage, int id, string name, string namePlural, int manaCost, int levelRequirement, bool learned = false) : base(minimumDamage, maximumDamage, id, name, namePlural)
        {
            ManaCost = manaCost;
            LevelRequirement = levelRequirement;
        }
    }
}
