using AI.Chickens;
using Creatures.Chickens.Roosters.Components;
using Mirror;

namespace Creatures.Chickens.Base.Components
{ 
    public class ChickenReproduction : ChickenComponentBase
    {
        [SyncVar] private uint _pregnantByNetId;

        [field: SyncVar]
        public bool IsPregnant { get; private set; } 
        public RoosterEntity PregnantBy
        {
            get
            {
                if (_pregnantByNetId == 0)
                { 
                    return null;
                }

                if (NetworkServer.spawned.TryGetValue(_pregnantByNetId, out var roosterObj))
                {
                    var rooster = roosterObj.GetComponent<RoosterEntity>(); 
                    return rooster;
                }
 
                return null;
            }
        }

        #region Server‐Only Methods

        [Server]
        public void MarkPregnant(uint fatherNetId)
        {
            if (IsPregnant) return;
            IsPregnant = true; 
            _pregnantByNetId = fatherNetId;
        }

        [Server]
        public void UnmarkPregnant()
        {
            if (!IsPregnant) return;
            IsPregnant = false;
        }  

        public override void OnStartServer()
        {
            base.OnStartServer(); 
            if (IsPregnant) return;   
        }

        #endregion 
        [Server]
        public bool TryBreedWith(ChickenReproduction otherRepro)
        {
            if (!otherRepro) return false;
            if (otherRepro == this) return false;  
 
            var selfGender = Owner.Gender;
            var otherGender = otherRepro.Owner.Gender;
            if (selfGender == otherGender) return false;

            ChickenReproduction maleSide, femaleSide;
            switch (selfGender)
            {
                case ChickenGender.Male when otherGender == ChickenGender.Female:
                    maleSide = this;
                    femaleSide = otherRepro;
                    break;
                case ChickenGender.Female when otherGender == ChickenGender.Male:
                    maleSide = otherRepro;
                    femaleSide = this;
                    break;
                default:
                    return false;
            }
 
            if (femaleSide.IsPregnant) return false; 
 
            femaleSide.MarkPregnant(maleSide.Owner.netId);
 
            maleSide.ForceResetToWander();

            // femaleSide.ForcePregnantSeekNest();
 
            return true;
        }

        #region Utility Methods for AI

        [Server]
        public void ForceResetToWander()
        {
            var ai = Owner.GetComponent<RoosterAI>();
            if (ai != null)
                ai.ForceToWander();
        }
        //
        // [Server]
        // public void ForcePregnantSeekNest()
        // {
        //     var ai = Owner.GetComponent<HenAI>();
        //     if (ai != null)
        //         ai.ForceSetPregnantSeekNest();
        // }

        #endregion
    }
}
