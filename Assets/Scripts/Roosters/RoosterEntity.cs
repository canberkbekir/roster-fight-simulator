using Mirror;
using UnityEngine;

namespace Roosters
{
    [RequireComponent(typeof(RoosterStats))]
    [RequireComponent(typeof(RoosterGenome))]
    [RequireComponent(typeof(RoosterSkills))]
    [RequireComponent(typeof(RoosterEquipment))]
    [RequireComponent(typeof(RoosterAppearance))]
    public class RoosterEntity : NetworkBehaviour
    {
        public string roosterId;
        public string ownerId;
        public string roosterName;

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

        private void Awake()
        {
            // Either use manually assigned fields or fallback to GetComponent
            Stats = _stats != null ? _stats : GetComponent<RoosterStats>();
            Genome = _genome != null ? _genome : GetComponent<RoosterGenome>();
            Skills = _skills != null ? _skills : GetComponent<RoosterSkills>();
            Equipment = _equipment != null ? _equipment : GetComponent<RoosterEquipment>();
            Appearance = _appearance != null ? _appearance : GetComponent<RoosterAppearance>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer(); 
            Stats.Init(this);
            Genome.Init(this);
            Skills.Init(this);
            Equipment.Init(this);
            Appearance.Init(this);
            
        }
    }
}