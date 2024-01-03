using UnityEngine;

namespace NaniCore.Loopool {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private AudioListener audioListener;
		#endregion

		#region Interfaces
		public AudioListener AudioListener {
			get => audioListener;
			set {
				if(value == null)
					value = null;
				if(audioListener != null && value == audioListener)
					return;

				if(audioListener != null) {
#if UNITY_EDITOR
					if(Application.isPlaying)
						DestroyImmediate(audioListener);
					else
						Destroy(audioListener);
#else
					Destroy(audioListener);
#endif
				}

				audioListener = value;

				if(audioListener == null) {
					audioListener = gameObject.EnsureComponent<AudioListener>();
				}
			}
		}
		#endregion

		#region Life cycle
		protected void InitializeAudio() {
			AudioListener = AudioListener;
		}
		#endregion
	}
}