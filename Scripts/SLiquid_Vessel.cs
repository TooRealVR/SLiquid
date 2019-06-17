using UnityEngine;

namespace SLiquid
{
    public class SLiquid_Vessel : MonoBehaviour
    {
        protected Material dissolve;
        protected SLiquid_Volume volume;

        protected float time = 0f;
        protected float fadeTime = 0f;

        void Start()
        {
            dissolve = GetComponent<MeshRenderer>().material;
            dissolve.SetFloat("_DissolveAmount", 1f);

            volume = GetComponentInChildren<SLiquid_Volume>();
            volume.liquidLevel = 0f;
        }

        public void Materialize(float fadeInTime)
        {
            fadeTime = fadeInTime;
        }

        // Update is called once per frame
        void Update()
        {
            if (fadeTime > 0f && time < fadeTime)
            {
                time += Time.deltaTime;
                dissolve.SetFloat("_DissolveAmount", Mathf.Clamp01(1f - time / fadeTime));
            }
        }
    }
}
