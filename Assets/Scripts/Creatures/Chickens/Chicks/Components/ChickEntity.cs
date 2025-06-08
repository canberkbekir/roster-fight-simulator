using Creatures.Chickens.Base;
using Creatures.Chickens.Hens;
using UnityEngine;

namespace Creatures.Chickens.Chicks.Components
{
    public class ChickEntity : ChickenEntity
    {     
        [Header("Additional Components")]
        [SerializeField] private ChickGrowthHandler chickGrowth;

        public ChickGrowthHandler ChickGrowth => chickGrowth;
        public override void Init(Chicken chicken)
        {
            base.Init(chicken);
            chickGrowth.Init(this);
        }
    }
}
