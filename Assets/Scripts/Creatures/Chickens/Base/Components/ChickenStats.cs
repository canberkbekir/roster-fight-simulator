using Mirror;

namespace Creatures.Chickens.Base.Components
{
    public enum StatType
    {
        Strength,
        Agility,
        Endurance,
        Intelligence,
        Health
    }
    public class ChickenStats : NetworkBehaviour, IChickenComponent
    {
        [SyncVar(hook = nameof(OnStatChanged))] public int strength;
        [SyncVar] public int agility;
        [SyncVar] public int endurance;
        [SyncVar] public int intelligence;
        [SyncVar] public int health;

        private ChickenEntity _owner;

        public void Init(ChickenEntity entity)
        {
            _owner = entity;
        }

        void OnStatChanged(int oldVal, int newVal)
        {
            // Notify visual/UI
        }
    }
}