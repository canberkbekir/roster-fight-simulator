using Mirror;
using ParrelSync;
using UnityEngine;

namespace Networks
{
    public class BaseNetworkManager : NetworkManager
    {
        public override void Start()
        {
            base.Start();  

            if (ClonesManager.IsClone())
            {
                Debug.Log("Clone detected: Starting as client...");
                networkAddress = "localhost";
                StartClient();
            }
            else
            {
                Debug.Log("Original project: Starting as host...");
                StartHost();  
            }
        }
    }
}