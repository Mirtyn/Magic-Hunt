using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Models
{
    public class StatsHolder
    {
        public float Damage;
        public float Speed;
        public int Piercing;
        public float Size;
        public IElement Element;

        public static StatsHolder Default = new StatsHolder
        {
            Damage = 1,
            Speed = 5,
            Piercing = 0,
            Size = 0.25f,
            Element = null
        };
    }
}
