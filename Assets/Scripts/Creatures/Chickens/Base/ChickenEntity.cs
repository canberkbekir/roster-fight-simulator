using System;
using Creatures.Chickens.Base.Components;
using Creatures.Chickens.Base.Utils;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Creatures.Chickens.Base
{
    public abstract class ChickenEntity : NetworkBehaviour
    {
        [SerializeField] protected string chickenName;

        [Header("— Common Components —")]
        [SerializeField] protected ChickenStats stats;
        [SerializeField] protected ChickenGenome genome;
        [SerializeField] protected ChickenSkills skills;
        [SerializeField] protected ChickenAppearance appearance;
        [SerializeField] protected ChickenEquipment equipment;
        [SerializeField] protected ChickenReproduction reproduction;
        
        
        public ChickenStats Stats { get; protected set; }
        public ChickenGenome Genome { get; protected set; }
        public ChickenSkills Skills { get; protected set; }
        public ChickenAppearance Appearance { get; protected set; }
        public ChickenEquipment Equipment { get; protected set; }
        public ChickenReproduction Reproduction { get; protected set; }
        public string ChickenName => chickenName;
        public ChickenGender Gender { get; protected set; }
        public ChickenEventBus EventBus { get; protected set; }
        public Chicken Chicken { get; protected set; }
        
        private bool _isInitialized;
        
        private void Awake()
        {
            Stats = stats ? stats : GetComponent<ChickenStats>();
            Genome = genome ? genome : GetComponent<ChickenGenome>();
            Skills = skills ? skills : GetComponent<ChickenSkills>();
            Equipment = equipment ? equipment : GetComponent<ChickenEquipment>();
            Appearance = appearance ? appearance : GetComponent<ChickenAppearance>();
            Reproduction = reproduction ? reproduction : GetComponent<ChickenReproduction>();
        }
        
        public virtual void Init(Chicken data)
        {
            if (_isInitialized) return;
            Chicken = data;
            if (data == null) throw new ArgumentNullException(nameof(data));

            EventBus = new ChickenEventBus();

            Stats.Init(this);
            Skills.Init(this);
            Appearance.Init(this); 
            Equipment.Init(this);
            Reproduction.Init(this);
            Genome.Init(this, data.Genes);

            _isInitialized = true;
        }
    }
}
