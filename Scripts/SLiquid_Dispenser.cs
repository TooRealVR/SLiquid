using UnityEngine;

namespace SLiquid
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SLiquid_Dispenser : MonoBehaviour
    {
        public Color liquidColor;
        [Range(0f, 10f)]
        public float fillRate = 0.5f;

        protected ParticleSystem spillParticleSystem;
        protected ParticleSystem.MainModule spillParticles;
        protected ParticleSystem.MainModule splashParticles;
        protected RaycastHit liquidJetImpact;
        protected Ray liquidJetRay = new Ray(Vector3.zero, Vector3.down);
        protected bool isEmitting = false;

        private int layerMask;

        protected void Start()
        {
            for (int i = 0; i < 32; i++)
            {
                if (LayerMask.LayerToName(i) == "Liquid")
                {
                    layerMask = 1 << i;
                    layerMask = ~layerMask;
                    break;
                }
            }

            spillParticleSystem = GetComponent<ParticleSystem>();
            spillParticles = spillParticleSystem.main;
            splashParticles = spillParticleSystem.subEmitters.GetSubEmitterSystem(0).main;
            SetColor(liquidColor);
        }

        public void Activate()
        {
            isEmitting = true;
        }

        public void Deactivate()
        {
            isEmitting = false;
            if (spillParticleSystem.isPlaying)
            {
                spillParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public void SetColor(Color color)
        {
            liquidColor = color;
            if (spillParticleSystem != null)
            {
                spillParticles.startColor = liquidColor;
            }
        }

        protected bool CheckInsideCollider(Collider collider, Vector3 point)
        {
            // We do a Raycast from the impact point to the center of the collider.
            // If the impact point is inside the collider, there should be no collision
            Vector3 direction = collider.bounds.center - point;
            Ray ray = new Ray(point, direction);
            return !collider.Raycast(ray, out _, direction.magnitude);
        }

        protected void Update()
        {
            if (isEmitting)
            {
                splashParticles.startColor = liquidColor;
                // Raycast straight down from liquid dispenser and set the particle lifetime accordingly
                liquidJetRay.origin = transform.TransformPoint(spillParticleSystem.shape.position);
                if (Physics.Raycast(liquidJetRay, out liquidJetImpact, Mathf.Infinity, layerMask, QueryTriggerInteraction.Collide))
                {
                    float distance = liquidJetImpact.distance;
                    // Did we hit a mug? Check if the point is inside and fill it
                    Transform root = liquidJetImpact.collider.transform.root;
                    if (root.CompareTag("Vessel"))
                    {
                        SLiquid_Volume volume = root.GetComponentInChildren<SLiquid_Volume>();
                        Collider volumeInside = volume.GetComponent<Collider>();
                        if (CheckInsideCollider(volumeInside, liquidJetImpact.point))
                        {
                            Color vesselLiquidColor = volume.Fill(fillRate * Time.deltaTime, liquidColor);

                            // If the vessel is not empty raycast against the liquid level plane and update distance if point is still inside the mug
                            if (!volume.IsEmpty())
                            {
                                if (volume.cutPlane.Raycast(liquidJetRay, out float planeDistance) && CheckInsideCollider(volumeInside, liquidJetRay.GetPoint(planeDistance)))
                                {
                                    distance = planeDistance;
                                    // We hit the liquid vessel so update the color of generated splashes
                                    splashParticles.startColor = vesselLiquidColor;
                                }
                            }
                        }
                    }
                    // Set the startLifetime for particles so that they expire at the impact point
                    // t = SQRT(2 * s / a)
                    spillParticles.startLifetime = Mathf.Sqrt((2f * distance) / (Physics.gravity.magnitude * spillParticles.gravityModifierMultiplier));
                }
                // Emit particles with new lifetime set
                if (!spillParticleSystem.isPlaying)
                {
                    // Make sure we emit at least some particles for short bursts when spilling on activation
                    spillParticleSystem.Emit((int)(spillParticleSystem.emission.rateOverTime.constant / 2f));
                    spillParticleSystem.Play();
                }
            }
        }
    }
}