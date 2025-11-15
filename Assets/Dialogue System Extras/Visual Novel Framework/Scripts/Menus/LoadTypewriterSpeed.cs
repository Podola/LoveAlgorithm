using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.VisualNovelFramework
{
	
	public class LoadTypewriterSpeed : MonoBehaviour
    {

        public AbstractTypewriterEffect typewriterEffect;

        public IEnumerator Start()
        {
            if (typewriterEffect == null && (DialogueManager.dialogueUI as AbstractDialogueUI) != null)
            {
                typewriterEffect = (DialogueManager.dialogueUI as AbstractDialogueUI).GetComponentInChildren<AbstractTypewriterEffect>();
            }
            var characterSpeed = PlayerPrefs.GetFloat("CharacterSpeed", 50);
            if (typewriterEffect != null) typewriterEffect.charactersPerSecond = characterSpeed;
            yield return null;
            if (DialogueManager.displaySettings != null && DialogueManager.displaySettings.subtitleSettings != null)
            {
                DialogueManager.displaySettings.subtitleSettings.subtitleCharsPerSecond = Mathf.Min(characterSpeed, DialogueManager.displaySettings.subtitleSettings.subtitleCharsPerSecond);
            }
        }
	}

}