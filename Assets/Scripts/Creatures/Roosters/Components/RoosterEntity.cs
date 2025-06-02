using System;
using Creatures.Roosters.Utils;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Creatures.Roosters.Components
{
    public class RoosterEntity : NetworkBehaviour
    {
        [SerializeField] private RoosterStats _stats;
        [SerializeField] private RoosterGenome _genome;
        [SerializeField] private RoosterSkills _skills;
        [SerializeField] private RoosterEquipment _equipment;
        [SerializeField] private RoosterAppearance _appearance;
        [SerializeField] private string _name;

        public RoosterStats Stats { get; private set; }
        public RoosterGenome Genome { get; private set; }
        public RoosterSkills Skills { get; private set; }
        public RoosterEquipment Equipment { get; private set; }
        public RoosterAppearance Appearance { get; private set; }
        public RoosterEventBus EventBus { get; private set; }
        public string RoosterName => _name;
        public Rooster Rooster { get; set; }
        public RoosterGender Gender => Rooster.Gender;

       
        [SyncVar] public bool isPregnant = false; 
        [SyncVar] public uint currentNestNetId = 0;  

        private bool _isInitialized;

        private void Awake()
        {
            Stats = _stats != null ? _stats : GetComponent<RoosterStats>();
            Genome = _genome != null ? _genome : GetComponent<RoosterGenome>();
            Skills = _skills != null ? _skills : GetComponent<RoosterSkills>();
            Equipment = _equipment != null ? _equipment : GetComponent<RoosterEquipment>();
            Appearance = _appearance != null ? _appearance : GetComponent<RoosterAppearance>();
        }

        public void Init(Rooster rooster)
        {
            Rooster = rooster ?? throw new ArgumentNullException(nameof(rooster));
            if (_isInitialized) return;

            EventBus = new RoosterEventBus();
            Stats.Init(this);
            Skills.Init(this);
            Equipment.Init(this);
            Appearance.Init(this);
            Genome.Init(this, rooster.Genes);
            _isInitialized = true;
        }

         
        [Server]
        public void MarkPregnant(uint fatherNetId)
        {
            if (isPregnant) return;
            isPregnant = true; 
        }

        
        [Server]
        public void AssignNest(uint nestNetId)
        {
            currentNestNetId = nestNetId;
        }

         
        [Server]
        public void ClearNest()
        {
            isPregnant = false;
            currentNestNetId = 0;
        }
    }
}
