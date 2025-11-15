using UnityEngine;

namespace PixelCrushers.DialogueSystem.VisualNovelFramework
{
	
	public class OptionsPanel : GeneralPanel
    {

        [Header("Default Options Values")]
        public float defaultMusicVolume = 1;
        public float defaultSFXVolume = 1;
        public float defaultCharacterSpeed = 50;

        [Header("Options UI Controls")]
        public UnityEngine.UI.Slider musicVolumeSlider;
        public UnityEngine.UI.Slider sfxVolumeSlider;
        public UnityEngine.UI.Slider characterSpeedSlider;

        public float musicVolume
        {
            get { return PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : defaultMusicVolume; }
            set { PlayerPrefs.SetFloat("MusicVolume", value); }
        }

        public float sfxVolume
        {
            get { return PlayerPrefs.HasKey("SfxVolume") ? PlayerPrefs.GetFloat("SfxVolume") : defaultSFXVolume; }
            set { PlayerPrefs.SetFloat("SfxVolume", value); }
        }

        public float characterSpeed
        {
            get { return PlayerPrefs.HasKey("CharacterSpeed") ? PlayerPrefs.GetFloat("CharacterSpeed") : defaultCharacterSpeed; }
            set { PlayerPrefs.SetFloat("CharacterSpeed", value); }
        }

        protected override void Start()
        {
            if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
            if (characterSpeedSlider != null) characterSpeedSlider.value = characterSpeed;
        }

        public void MusicVolumeChanged()
        {
            SetVolume(musicVolumeSlider, GameObject.Find("Music Audio Source"), "MusicVolume");
        }

        public void SfxVolumeChanged()
        {
            SetVolume(sfxVolumeSlider, DialogueManager.Instance.gameObject, "SfxVolume");
        }

        private void SetVolume(UnityEngine.UI.Slider slider, GameObject audioSourceObject, string playerPrefsKey)
        {
            if (slider == null) return;
            var audioSource = (audioSourceObject != null) ? audioSourceObject.GetComponent<AudioSource>() : null;
            if (audioSource != null) audioSource.volume = slider.value;
            PlayerPrefs.SetFloat(playerPrefsKey, slider.value);
        }

        public void TypewriterSpeedChanged()
        {
            var typewriterEffect = VNFrameworkFindUtility.FindFirstInstance<AbstractTypewriterEffect>();
            if (characterSpeedSlider == null || typewriterEffect == null) return;
            typewriterEffect.charactersPerSecond = characterSpeedSlider.value;
            PlayerPrefs.SetFloat("CharacterSpeed", characterSpeedSlider.value);
            DialogueManager.DisplaySettings.subtitleSettings.subtitleCharsPerSecond = Mathf.Min(characterSpeedSlider.value, DialogueManager.DisplaySettings.subtitleSettings.subtitleCharsPerSecond);
        }
	}

}