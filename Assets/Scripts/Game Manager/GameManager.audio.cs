using UnityEngine;

namespace NaniCore.Stencil {
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

		public void PlayPhysicalSound(AudioClip sound, Vector3 position, Transform under, float strength) {
			float maxGain = Settings.maxPhysicalSoundGain;
			float volume = (1f - 1f / (strength / maxGain + 1f)) * maxGain;
			volume *= Settings.physicalSoundBaseGain;

			// 这b玩意死活调不好，给我整红温了。
			Vector2 range = Vector2.zero;
			range.y = Settings.physicalSoundRange * strength;
			range.x = range.y * Mathf.Exp(-Settings.physicalSoundAttenuation);
			range.y *= strength;

			var coroutine = AudioUtility.PlayOneShotAtCoroutine(
				sound, position, under,
				new() {
					volume = volume,
					range = range,
					spatialBlend = 1f,
				}
			);
			Instance.StartCoroutine(coroutine);
		}

		public void PlayPhysicalSound(AudioClip sound, Rigidbody rb) {
			float strength = rb.mass;
			if(!rb.isKinematic) {
				if((rb.constraints | RigidbodyConstraints.FreezePosition) == 0)
					strength *= rb.velocity.magnitude;
			}

			PlayPhysicalSound(sound, rb.position, rb.transform, strength);
		}
		#endregion

		#region Life cycle
		protected void InitializeAudio() {
			AudioListener = AudioListener;
		}
		#endregion
	}
}