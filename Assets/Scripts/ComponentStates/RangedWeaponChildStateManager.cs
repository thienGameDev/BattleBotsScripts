using Mirror;
using RobotComponentLib.RangedWeaponLib;
using UnityEngine;

namespace ComponentStates {
    public abstract class RangedWeaponChildStateManager : NetworkBehaviour {
        public const float MOVING_SPEED = 10f;

        public GameObject rangedParent;
        public Transform Head { get; private set; }

        public Transform Tail { get; private set; }

        private void OnEnable() {
            if (Head != null && Tail != null) return;
            CreateHeadTail();
        }

        [Server]
        public void DestroySelf() {
            Debug.Log("Child is destroyed");
            NetworkServer.UnSpawn(gameObject);
            SetChildTransparency(1f);
            var nwRangedWeapon = rangedParent.GetComponent<NwRangedWeapon>();
            nwRangedWeapon.ReturnChild(gameObject);
        }

        [ClientRpc]
        public void RpcHideChild() {
            if(isClientOnly) return;
            Debug.Log($"Child is hidden. isClientOnly? {isClientOnly}");
            SetChildTransparency(0f);
            // gameObject.SetActive(false);
        }
        
        private void SetChildTransparency(float a) {
            // var spriteRender = GetComponent<SpriteRenderer>();
            // var color = spriteRender.color;
            // color.a = a;
            // spriteRender.color = color;
            var childRenders = GetComponentsInChildren<SpriteRenderer>();
            foreach (var render in childRenders) {
                var currentColor = render.material.color;
                currentColor.a = a;
                render.material.color = currentColor;
            }
        }
        
        [ServerCallback]
        private void OnBecameInvisible() {
            // Debug.LogWarning("Child is out of view");
            DestroySelf();        
        }

        [Client]
        public override void OnStartClient() {
            //if (isClientOnly) Destroy(this);
        }

        public abstract void EnterOnHold();
        public abstract void Execute();
        
        private void CreateHeadTail() {
            var headWeapon = new GameObject("Head") {
                transform = {
                    parent = transform,
                    localPosition = Vector3.right
                }
            };
            Head = headWeapon.transform;
            var tailWeapon = new GameObject("Tail") {
                transform = {
                    parent = transform,
                    localPosition = Vector3.left
                }
            };
            Tail = tailWeapon.transform;
        }
    }
}
