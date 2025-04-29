// RoosterEntity.cs

using System;
using Genes.Base;
using Mirror;
using UnityEngine;

namespace Roosters.Components
{
    public class RoosterEntity : NetworkBehaviour
    {
        [Header("Rooster Info")] public string roosterName;
        public GeneData[] preReadyGenes = Array.Empty<GeneData>();

        [SerializeField] private RoosterStats _stats;
        [SerializeField] private RoosterGenome _genome;
        [SerializeField] private RoosterSkills _skills;
        [SerializeField] private RoosterEquipment _equipment;
        [SerializeField] private RoosterAppearance _appearance;

        public RoosterStats Stats { get; private set; }
        public RoosterGenome Genome { get; private set; }
        public RoosterSkills Skills { get; private set; }
        public RoosterEquipment Equipment { get; private set; }
        public RoosterAppearance Appearance { get; private set; }

        public RoosterEventBus EventBus { get; private set; }

        
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

        public override void OnStartServer()
        {
            base.OnStartServer();
            Init();
        }

        /// <summary>
        /// Public entry point to run all your server‐side Init logic
        /// on a throw‐away instance.
        /// </summary>
        public void Init()
        {
            if (_isInitialized) return;
            EventBus = new RoosterEventBus();
            Genome.Init(this); 
            Stats.Init(this);
            Skills.Init(this);
            Equipment.Init(this);
            Appearance.Init(this);
            _isInitialized = true;
        }
    }
}