using System.Collections.Generic;
using UnityEngine;

namespace SLiquid
{
    [RequireComponent(typeof(Collider)), RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(SLiquid_Slosher))]
    public class SLiquid_Volume : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float liquidLevel = 0;
        [Range(0f, 10f)]
        public float minSpillRate = 0.1f;
        [Range(0f, 10f)]
        public float maxSpillRate = 3f;
        public float spillRadius = 0f;
        public Color liquidColor = Color.white;

        private static readonly SortedSet<int> stencilRefs = new SortedSet<int>();

        protected SLiquid_Dispenser spiller;

        protected int stencil = 0;
        protected Material liquidMat;
        protected Material liquidCapMat;
        protected Transform liquidCap;

        [HideInInspector]
        public Plane cutPlane = new Plane();
        protected float maxSpillDistance = 0f;
        protected Vector4 planeRepresentation = new Vector4();
        protected Vector3[] vertices;

        protected Bounds localBounds;
        protected Vector3 boundsCenter = new Vector3();
        protected Vector3 boundsTopCenter = new Vector3();
        protected List<Vector3> topVertices = new List<Vector3>();
        protected SLiquid_Slosher slosher;

        protected MeshRenderer volumeRenderer;
        protected MeshRenderer capRenderer;

        protected static bool initialized = false;

        protected void Awake()
        {
            if (!initialized)
            {
                for (int i = 1; i < 256; i++)
                {
                    stencilRefs.Add(i);
                }
                initialized = true;
            }
            stencil = stencilRefs.Min;
            stencilRefs.Remove(stencil);
        }

        protected void OnDestroy()
        {
            stencilRefs.Add(stencil);
        }

        protected void Start()
        {
            spiller = GetComponentInChildren<SLiquid_Dispenser>();
            slosher = GetComponent<SLiquid_Slosher>();

            liquidCap = transform.GetChild(1);
            volumeRenderer = GetComponent<MeshRenderer>();
            capRenderer = liquidCap.GetComponent<MeshRenderer>();

            liquidMat = volumeRenderer.material;
            liquidCapMat = capRenderer.material;
            liquidMat.SetInt("_StencilRef", stencil);
            liquidCapMat.SetInt("_StencilRef", stencil);

            UpdateColor(liquidColor);

            Mesh mesh = GetComponent<MeshFilter>().mesh;
            localBounds = new Bounds(mesh.bounds.center, mesh.bounds.size);
            boundsCenter = localBounds.center;
            boundsTopCenter = boundsCenter;
            boundsTopCenter.y = localBounds.max.y;

            UpdatePlane(Vector3.up, transform.position);

            spiller.transform.localPosition = boundsTopCenter + Vector3.left * spillRadius;
            if (minSpillRate > maxSpillRate)
            {
                minSpillRate = maxSpillRate;
            }

            // build a list of the top of the volume to detect if we want to spill liquid
            vertices = new Vector3[mesh.vertices.Length];
            mesh.vertices.CopyTo(vertices, 0);
            for (int i = 0; i < vertices.Length; i++)
            {
                if (Mathf.Approximately(vertices[i].y, localBounds.max.y) && !topVertices.Contains(vertices[i]))
                {
                    topVertices.Add(vertices[i]);
                }
            }
            maxSpillDistance = localBounds.size.y * topVertices.Count;
        }

        protected void Update()
        {
            if (liquidLevel == 0f)
            {
                if (volumeRenderer.enabled == true)
                {
                    volumeRenderer.enabled = false;
                    capRenderer.enabled = false;
                }
                spiller.Deactivate();
            }
            else
            {
                if (volumeRenderer.enabled == false)
                {
                    volumeRenderer.enabled = true;
                    capRenderer.enabled = true;
                }

                Vector3 bottom = transform.TransformPoint(vertices[0]);
                Vector3 top = transform.TransformPoint(vertices[0]);
                Vector3 center = transform.TransformPoint(boundsCenter);
                for (int i = 1; i < vertices.Length; i++)
                {
                    Vector3 v = transform.TransformPoint(vertices[i]);
                    if (v.y < bottom.y)
                    {
                        bottom = v;
                    }
                    if (v.y > top.y)
                    {
                        top = v;
                    }
                }

                Vector3 liquidCenter = bottom + (top - bottom) * liquidLevel;
                float y = liquidCenter.y;
                liquidCenter = center + Vector3.Dot(liquidCenter - center, transform.up) * transform.up;
                liquidCenter.y = y;

                UpdatePlane(slosher.GetSurfaceNormal(), liquidCenter);

                //test topVertices against cut plane to determine if we should be leaking fluid
                float spillDistance = 0f;
                int spill = 0;

                Vector3 spillLocation = Vector3.zero;
                foreach (Vector3 v in topVertices)
                {
                    float d = cutPlane.GetDistanceToPoint(transform.TransformPoint(v));
                    if (d <= 0f)
                    {
                        spill++;
                        spillDistance -= d;
                        // Sum up all the vectores from the top vertices to calculate the average later
                        spillLocation += v - boundsTopCenter;
                    }
                }
                if (spillDistance > 0f)
                {
                    float spillRate = Mathf.Lerp(minSpillRate, maxSpillRate, spillDistance / maxSpillDistance);
                    liquidLevel = Mathf.Clamp01(liquidLevel - spillRate * Time.deltaTime);

                    // Calculate the average over all "leaking" vertices and
                    // position liquid spiller to face to the part of the glass that is leaking
                    if (spill == topVertices.Count)
                    {
                        spillLocation = boundsTopCenter + Vector3.left * spillRadius;
                    }
                    else
                    {
                        spillLocation = boundsTopCenter + spillLocation.normalized * spillRadius;
                    }
                    spiller.transform.localPosition = spillLocation;
                    spiller.fillRate = spillRate;
                    spiller.Activate();
                }
                else
                {
                    spiller.Deactivate();
                }
            }
        }

        protected void UpdatePlane(Vector3 normal, Vector3 position)
        {
            cutPlane.SetNormalAndPosition(normal, position);
            planeRepresentation.Set(cutPlane.normal.x, cutPlane.normal.y, cutPlane.normal.z, cutPlane.distance);
            liquidMat.SetVector("_Plane", planeRepresentation);
            //liquidMatFront.SetVector("_Plane", planeRepresentation);

            liquidCap.position = position;
            liquidCap.rotation = Quaternion.FromToRotation(Vector3.up, normal);
        }

        public Color Fill(float fraction, Color color)
        {
            liquidLevel = Mathf.Clamp01(liquidLevel + fraction);

            if (liquidLevel > 0)
            {
                Color newColor = Color.Lerp(liquidColor, color, fraction / liquidLevel);
                UpdateColor(newColor);
            }
            return color;
        }

        protected void UpdateColor(Color color)
        {
            liquidColor = color;
            liquidMat.color = liquidColor;
            liquidCapMat.color = liquidColor;
            spiller.SetColor(liquidColor);
        }

        public bool IsEmpty()
        {
            return liquidLevel == 0f;
        }
    }
}