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
    public class ChickenStats : ChickenComponentBase
    {
        [SyncVar(hook = nameof(OnStatChanged))] public int strength;
        [SyncVar] public int agility;
        [SyncVar] public int endurance;
        [SyncVar] public int intelligence;
        [SyncVar] public int health;

        public override void Init(ChickenEntity owner)
        {
            base.Init(owner);
        }

        void OnStatChanged(int oldVal, int newVal)
        {
            // Notify visual/UI
        }
    }
}