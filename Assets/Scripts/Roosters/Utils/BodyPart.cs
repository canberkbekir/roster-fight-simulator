using Genes.Features;
using Genes.Features.Base;
using Mirror;
using UnityEngine;

namespace Roosters.Utils
{
    [DisallowMultipleComponent]
    public class BodyPart : NetworkBehaviour
    {
        [SerializeField] private AppearanceGeneType bodyPartType;
        [SerializeField] private Renderer partRenderer;

        // This is the server-authoritative color.
        [SyncVar(hook = nameof(OnColorChanged))]
        private Color _partColor = Color.white;

        private void Awake()
        {
            // Grab or cache the renderer
            partRenderer ??= GetComponent<Renderer>();

            // Give us a unique material instance so we don't tint the shared asset
            if (partRenderer.material != null)
                partRenderer.material = new Material(partRenderer.material);

            // Apply whatever color we have (default or from spawn)
            ApplyColor(_partColor);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            // If HandleAppearanceGeneFeature was called BEFORE we actually spawned,
            // we'll already have set _partColor on the server instance,
            // so this makes sure it's baked into the spawn payload.
            ApplyColor(_partColor);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            // When the spawn message arrives, SyncVar will be deserialized
            // and invoke our hook (OnColorChanged), but just in case:
            ApplyColor(_partColor);
        }

        /// <summary>
        /// Call this whenever you get a color‐gene for this body part.
        /// 
        /// – If we're still on the server *before* spawn (netId == 0), we just set 
        ///   the SyncVar directly.  That value will be included in the very first 
        ///   spawn message to all clients.  
        /// 
        /// – If we're on the server *after* spawn, setting the SyncVar here will 
        ///   trigger the hook on all clients.  
        /// 
        /// – If we're a client *after* spawn, we Cmd the server.  
        /// 
        /// – If we're a client *before* spawn, we do nothing—wait for the server’s
        ///   initial value to arrive in the spawn payload.
        /// </summary>
        public void HandleAppearanceGeneFeature(AppearanceGeneFeature gene)
        {
            if (gene.AppearanceGeneType != bodyPartType ||
                gene.AppearanceEffectType != AppearanceEffectType.Color)
                return;

            if (isServer)
            {
                // SERVER
                if (netIdentity.netId == 0)
                {
                    // Pre-spawn on server: seed the initial color
                    _partColor = gene.ColorValue;
                    ApplyColor(_partColor); 
                }
                else
                {
                    // Post-spawn on server: update SyncVar normally
                    _partColor = gene.ColorValue;
                }
            }
            else
            {
                // CLIENT
                if (netIdentity.netId != 0)
                {
                    // Only send a Cmd once we've got a valid netId
                    CmdSetColor(gene.ColorValue);
                }
                // else: ignore pre-spawn client events
            }
        }

        // This will only ever run on the server.
        [Command(requiresAuthority = false)]
        private void CmdSetColor(Color c)
        {
            _partColor = c;
        }

        // Whenever _partColor changes (spawn or Cmd), apply it:
        private void OnColorChanged(Color oldColor, Color newColor)
        {
            ApplyColor(newColor);
        }

        private void ApplyColor(Color c)
        {
            if (partRenderer?.material != null)
                partRenderer.material.color = c;
            
            if(bodyPartType == AppearanceGeneType.Body)
                Debug.Log($"Applying color {c} to {gameObject.name} ({bodyPartType})");
        }
    }
}
