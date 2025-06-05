using Creatures.Chickens.Base;
using Creatures.Chickens.Hens;

namespace Creatures.Chickens.Chicks.Components
{
    public class ChickEntity : ChickenEntity
    {   
        public new Chick Chicken { get; set; } 
        public override void Init(Chicken chicken)
        {
            base.Init(chicken);
        }
    }
}
