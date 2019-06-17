using UnityEngine;
using UnityEngine.Animations;

namespace SLiquid
{
    public class SLiquid_Slosher : MonoBehaviour
    {
        public float liquidAngularLimit = 7.5f;
        public float liquidBounciness = 0.3f;
        public float pendulumMass = 1f;
        public float pendulumDrag = 2.5f;
        public float pendulumLength = 0.25f;

        protected GameObject anchor;
        protected GameObject pendulum;

        private static int count = 1;
        private int layer;

        public Vector3 GetSurfaceNormal()
        {
            return pendulum.transform.up;
        }

        void Start()
        {
            for (int i = 0; i < 32; i++)
            {
                if (LayerMask.LayerToName(i) == "Liquid")
                {
                    layer = i;
                    break;
                }
            }

            anchor = new GameObject(transform.parent.name + "_slosher " + count)
            {
                layer = layer,
            };
            anchor.transform.position = transform.position;

            Rigidbody anchorBody = anchor.AddComponent<Rigidbody>();
            anchorBody.mass = 1f;
            anchorBody.isKinematic = true;
            anchorBody.useGravity = false;

            ConfigurableJoint joint = anchor.AddComponent<ConfigurableJoint>();
            joint.anchor = Vector3.zero;
            joint.axis = Vector3.right;
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = new Vector3(0f, pendulumLength, 0f);
            joint.secondaryAxis = Vector3.up;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Limited;
            SoftJointLimit sjl = joint.lowAngularXLimit;
            sjl.limit = -liquidAngularLimit;
            sjl.bounciness = liquidBounciness;
            joint.lowAngularXLimit = sjl;

            sjl = joint.highAngularXLimit;
            sjl.limit = liquidAngularLimit;
            sjl.bounciness = liquidBounciness;
            joint.highAngularXLimit = sjl;

            sjl = joint.angularZLimit;
            sjl.limit = liquidAngularLimit;
            sjl.bounciness = liquidBounciness;
            joint.angularZLimit = sjl;

            PositionConstraint constraint = anchor.AddComponent<PositionConstraint>();
            ConstraintSource source = new ConstraintSource()
            {
                weight = 1f,
                sourceTransform = transform
            };
            constraint.AddSource(source);
            constraint.locked = true;
            constraint.constraintActive = true;

            pendulum = new GameObject(transform.parent.name + "_slosher_pendulum " + count)
            {
                layer = layer
            };
            pendulum.transform.position = transform.position - Vector3.down * pendulumLength;

            Rigidbody pendulumBody = pendulum.AddComponent<Rigidbody>();
            pendulumBody.mass = pendulumMass;
            pendulumBody.drag = pendulumDrag;
            pendulumBody.useGravity = true;

            SphereCollider collider = pendulum.AddComponent<SphereCollider>();
            collider.radius = 0.01f;

            joint.connectedBody = pendulumBody;

            count++;
        }

        private void OnDestroy()
        {
            if (anchor != null)
            {
                Destroy(anchor);
            }
            if (pendulum != null)
            {
                Destroy(pendulum);
            }
        }
    }
}