using UnityEngine;

namespace PixelCrushers.DialogueSystem.VisualNovelFramework
{
	
	public class LoadVolume : MonoBehaviour
    {

        public enum VolumeType { Music, SFX };

        public VolumeType volumeType = VolumeType.Music;

        public void Start()
        {
            var audioSource = GetComponent<AudioSource>();
            if (audioSource != null && Menus.instance != null && Menus.instance.optionsPanel != null)
            {
                audioSource.volume = (volumeType == VolumeType.Music)
                    ? Menus.instance.optionsPanel.musicVolume
                    : Menus.instance.optionsPanel.sfxVolume;
            }
        }
	}

}