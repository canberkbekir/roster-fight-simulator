using Mirror;

namespace Roosters.Components
{
    public enum StatType
    {
        Strength,
        Agility,
        Endurance,
        Intelligence,
        Health
    }
    public class RoosterStats : NetworkBehaviour, IRoosterComponent
    {
        [SyncVar(hook = nameof(OnStatChanged))] public int strength;
        [SyncVar] public int agility;
        [SyncVar] public int endurance;
        [SyncVar] public int intelligence;
        [SyncVar] public int health;

        private RoosterEntity _owner;

        public void Init(RoosterEntity entity)
        {
            _owner = entity;
        }

        void OnStatChanged(int oldVal, int newVal)
        {
            // Notify visual/UI
        }
    }
}