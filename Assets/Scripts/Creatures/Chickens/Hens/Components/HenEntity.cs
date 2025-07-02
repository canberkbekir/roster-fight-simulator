using Creatures.Chickens.Base; 
using UnityEngine;

namespace Creatures.Chickens.Hens.Components
{
    public class HenEntity : ChickenEntity
    {    
        [Header("Additional Components")]
        [SerializeField] private LayingEggHandler layingEggHandler;
        [SerializeField] private NestHandler nestHandler;
        
        public LayingEggHandler LayingEggHandler => layingEggHandler;
        public NestHandler NestHandler => nestHandler;
        public override void Init(Chicken chicken)
        {
            base.Init(chicken);
            Chicken.Gender = ChickenGender.Female; 
            
            // Initialize additional components
            layingEggHandler.Init(this);
            nestHandler.Init(this);
        }
    }
}
