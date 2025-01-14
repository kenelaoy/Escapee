using System;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// SE再生コンポーネント
	/// 基本はシステム内部で使うので外から呼ばないこと
	/// </summary>
	[AddComponentMenu("Utage/Lib/Sound/Se")]
	internal class SoundSe : MonoBehaviour
	{
		public AudioSource AudioSource { get; private set; }

		internal void Init(AudioClip audioClip, float volume, bool loop)
		{
			this.AudioSource = this.gameObject.AddComponent<AudioSource>();
			this.AudioSource.clip = audioClip;
			this.AudioSource.volume = volume;
			this.AudioSource.loop = loop;
			this.AudioSource.Play();
		}

		void Update()
		{
			if (!AudioSource.isPlaying)
			{
				GameObject.Destroy(this.gameObject);
			}
		}

		internal void FadeOut(float fadeTime)
		{
			iTween.AudioTo(this.gameObject, iTween.Hash("volume", 0.0f, "time", fadeTime));
		}

		internal bool IsPlaying(string label)
		{
			return( this.gameObject.name == label && AudioSource.isPlaying);
		}
	};
}
