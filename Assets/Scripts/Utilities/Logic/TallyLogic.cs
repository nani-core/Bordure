using UnityEngine;

namespace NaniCore {
    public class TallyLogic : Logic {
        #region Serialized fields
        [Min(0)] public int targetCount;
        #endregion

        #region Fields
        private int count = 0;
        #endregion

        private void IncrementalInvoke() {
            count++;
            if (count == targetCount) {
                callback.Invoke();
            }
        }

        public override void Invoke() {
            IncrementalInvoke();
        }

        public void Rewind() {
            if (count == 0) return;

            count--;
        }
    }
}
