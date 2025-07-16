using System;
using AI.Chickens;
using Creatures.Chickens.Base; 
using UnityEngine;
using UnityEngine.Serialization;

namespace Creatures.Chickens.Hens.Components
{ 
    public enum HenState
    {
        Wander,
        SeekNest, 
        Incubate
    }
    
    public class HenEntity : ChickenEntity
    {    
        [FormerlySerializedAs("layingEggHandler")]
        [Header("Additional Components")]
        [SerializeField] private HenEggHandler henEggHandler;
        [FormerlySerializedAs("nestHandler")] [SerializeField] private HenNestHandler henNestHandler;  
        
        public HenEggHandler HenEggHandler => henEggHandler;
        public HenNestHandler HenNestHandler => henNestHandler;
        public HenState CurrentState => _henState;

        public event Action<HenState> OnStateChanged;

        private HenState _henState;
        public override void Init(Chicken chicken)
        {
            base.Init(chicken);
            Chicken.Gender = ChickenGender.Female; 
            
            // Initialize additional components
            henEggHandler.Init(this);
            henNestHandler.Init(this);
        }

        public void SetState(HenState newState)
        {
            _henState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
