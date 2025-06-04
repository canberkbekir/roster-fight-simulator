using Creatures.Genes.Features;
using Creatures.Genes.Features.Base;
using Mirror;
using UnityEngine;

namespace Creatures.Roosters.Utils
{
    [DisallowMultipleComponent]
    public class BodyPart : NetworkBehaviour
    {
        [SerializeField] private AppearanceGeneType bodyPartType;
        [SerializeField] private Renderer partRenderer;

        [SyncVar(hook = nameof(OnColorChanged))]
        private Color _partColor = Color.white;

        private bool _pendingColor;
        private Color _pendingColorValue;

        private void Awake()
        {
            partRenderer ??= GetComponent<Renderer>();
            if (partRenderer.material != null)
                partRenderer.material = new Material(partRenderer.material);
            ApplyColor(_partColor);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            ApplyColor(_partColor);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            ApplyColor(_partColor);

            if (_pendingColor && netIdentity.netId != 0)
            {
                CmdSetColor(_pendingColorValue);
                _pendingColor = false;
            }
        }

        public void HandleAppearanceGeneFeature(AppearanceGeneFeature gene)
        {
            if (gene.AppearanceGeneType != bodyPartType ||
                gene.AppearanceEffectType != AppearanceEffectType.Color)
                return;

            if (isServer)
            {
                if (netIdentity.netId == 0)
                {
                    _partColor = gene.ColorValue;
                    ApplyColor(_partColor);
                }
                else
                {
                    _partColor = gene.ColorValue;
                }
            }
            else
            {
                if (netIdentity.netId == 0)
                {
                    _pendingColor = true;
                    _pendingColorValue = gene.ColorValue;
                }
                else
                {
                    CmdSetColor(gene.ColorValue);
                }
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdSetColor(Color c)
        {
            _partColor = c;
        }

        private void OnColorChanged(Color _, Color newColor)
        {
            ApplyColor(newColor);
        }

        private void ApplyColor(Color c)
        {
            if (partRenderer?.material != null)
                partRenderer.material.color = c;

            // if (bodyPartType == AppearanceGeneType.Body)
            //     Debug.Log($"Applying color {c} to {gameObject.name} ({bodyPartType})");
        }
    }
}
