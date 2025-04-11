using System;
using Mirror;
using UnityEngine;

namespace Networks
{
    public class NetworkVisibilityHandler : NetworkBehaviour
    {
        public enum VisibilityType
        {
            OnlyLocalPlayer,
            OthersButNotLocal,
            Everyone
        }

        [SerializeField] private VisibilityType visibilityType = VisibilityType.OnlyLocalPlayer;

        void Start()
        {
            switch (visibilityType)
            {
                case VisibilityType.OnlyLocalPlayer:
                    if (!isLocalPlayer) gameObject.SetActive(false);
                    break;

                case VisibilityType.OthersButNotLocal:
                    if (isLocalPlayer) gameObject.SetActive(false);
                    break;

                case VisibilityType.Everyone:
                    // Do nothing
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}