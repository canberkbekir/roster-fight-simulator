// RoosterEntity.cs

using System;
using Genes.Base;
using Genes.Base.ScriptableObjects;
using Mirror;
using UnityEngine;

namespace Roosters.Components
{
    public class RoosterEntity : NetworkBehaviour
    {  
        [SerializeField] private RoosterStats _stats;
        [SerializeField] private RoosterGenome _genome;
        [SerializeField] private RoosterSkills _skills;
        [SerializeField] private RoosterEquipment _equipment;
        [SerializeField] private RoosterAppearance _appearance;
        [SerializeField] private string _name;
        [SerializeField] private RoosterGender _gender;

        public RoosterStats Stats { get; private set; }
        public RoosterGenome Genome { get; private set; }
        public RoosterSkills Skills { get; private set; }
        public RoosterEquipment Equipment { get; private set; }
        public RoosterAppearance Appearance { get; private set; } 
        public RoosterEventBus EventBus { get; private set; }
        public string RoosterName => _name;
        public Rooster Rooster { get;  set; } 
        public RoosterGender Gender { get; private set; }

        
        private bool _isInitialized;

        private void Awake()
        {
            // wire up your serialized sub‐components
            Stats = _stats != null ? _stats : GetComponent<RoosterStats>();
            Genome = _genome != null ? _genome : GetComponent<RoosterGenome>();
            Skills = _skills != null ? _skills : GetComponent<RoosterSkills>();
            Equipment = _equipment != null ? _equipment : GetComponent<RoosterEquipment>();
            Appearance = _appearance != null ? _appearance : GetComponent<RoosterAppearance>();
        }

        /// <summary>
        /// Public entry point to run all your server‐side Init logic
        /// on a throw‐away instance.
        /// </summary>
        public void Init(Rooster rooster)
        {
            Rooster = rooster ?? throw new ArgumentNullException(nameof(rooster));
            if (_isInitialized) return;
            EventBus = new RoosterEventBus();
            Stats.Init(this);
            Skills.Init(this);
            Equipment.Init(this);
            Appearance.Init(this);
            Genome.Init(this,rooster.Genes);  
            _isInitialized = true;
        } 
    }
}