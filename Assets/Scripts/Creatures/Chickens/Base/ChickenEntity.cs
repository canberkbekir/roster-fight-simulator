using System;
using AI.Base;
using Creatures.Chickens.Base.Components;
using Creatures.Chickens.Base.Utils;
using Interactions.Objects.Breeders;
using Mirror;
using UnityEngine; 

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
        [SerializeField] protected ChickenHungerHandler hungerHandler;
        [Space]
        [Header("— Chicken References —")]
        [SerializeField] protected BaseAI ai;
        
        
        public ChickenStats Stats { get; protected set; }
        public ChickenGenome Genome { get; protected set; }
        public ChickenSkills Skills { get; protected set; }
        public ChickenAppearance Appearance { get; protected set; }
        public ChickenEquipment Equipment { get; protected set; }
        public ChickenReproduction Reproduction { get; protected set; }
        public ChickenHungerHandler HungerHandler { get; protected set; }
        public string ChickenName => chickenName;
        public ChickenGender Gender => Chicken.Gender;
        public ChickenEventBus EventBus { get; protected set; }
        public Chicken Chicken { get; protected set; }
        public BaseAI ChickenAI => ai;

        [SyncVar] private uint _breederNetId;

        public Breeder Breeder
        {
            get
            {
                if (_breederNetId == 0) return null;

                if (isServer)
                {
                    if (NetworkServer.spawned.TryGetValue(_breederNetId, out var obj))
                        return obj.GetComponent<Breeder>();
                }
                else if (isClient)
                {
                    if (NetworkClient.spawned.TryGetValue(_breederNetId, out var obj))
                        return obj.GetComponent<Breeder>();
                }

                return null;
            }
        }

        [Server] 
        public void AssignBreeder(Breeder breeder)
        {
            _breederNetId = breeder ? breeder.netId : 0;
            Debug.Log(_breederNetId == 0
                ? $"[ChickenEntity:{name}] Unassigned from breeder"
                : $"[ChickenEntity:{name}] Assigned to breeder {_breederNetId}");
        }
        
        private bool _isInitialized;
        
        private void Awake()
        {
            Stats = stats ? stats : GetComponent<ChickenStats>();
            Genome = genome ? genome : GetComponent<ChickenGenome>();
            Skills = skills ? skills : GetComponent<ChickenSkills>();
            Equipment = equipment ? equipment : GetComponent<ChickenEquipment>();
            Appearance = appearance ? appearance : GetComponent<ChickenAppearance>();
            Reproduction = reproduction ? reproduction : GetComponent<ChickenReproduction>();
            HungerHandler = hungerHandler ? hungerHandler : GetComponent<ChickenHungerHandler>();
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
            HungerHandler.Init(this);
            Genome.Init(this, data.Genes);

            _isInitialized = true;
        }
    }
}
