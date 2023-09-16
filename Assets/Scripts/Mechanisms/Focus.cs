using UnityEngine;
using System;
using System.Collections;

namespace NaniCore.UnityPlayground
{
    public class Focus : MonoBehaviour
    {
        #region Serialized fields
        [SerializeField] private Material material;
        [Serializable]
        public struct FocusRadiusMap
        {
            public float Normal;
            public float Hovering;
            public float Grabbing;
        }
        [SerializeField] private FocusRadiusMap focusRadiusMap;
        [SerializeField] private float speed = 1f;
        #endregion

        #region Fields
        private float r;
        #endregion

        #region Functions
        public void UpdateFocusAnimated(int type)
        {
            switch (type)
            {
                case 0:
                    ZoomCubic(focusRadiusMap.Normal);
                    break;
                case 1:
                    ZoomCubic(focusRadiusMap.Hovering);
                    break;
                case 2:
                    ZoomCubic(focusRadiusMap.Grabbing);
                    break;
            }
        }

        #region Easing Functions
        private Coroutine currCoroutine;
        private void ZoomCubic(float newr)
        {
            if (currCoroutine != null) StopCoroutine(currCoroutine);
            currCoroutine = StartCoroutine(IZoomCubic(newr));
        }

        private IEnumerator IZoomCubic(float newr)
        {
            float x = 0f, oldr = r;
            while (x < 1f)
            {
                x += speed * Time.deltaTime;
                float y = 5.093f * x * x * x - 10.231f * x * x + 6.139f * x;
                r = oldr + (newr - oldr) * y;
                material.SetFloat("_Radius", r);
                yield return 0;
            }
        }
        #endregion
        #endregion
    }
}
