using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.VisualNovelFramework
{
	
	public class SaveGamePanel : GeneralPanel
    {

		public UnityEngine.UI.Button[] slots;
		public GameObject confirmOverwritePanel;
		public GameObject saveInProgressPanel;

		private Menus menus = null;
		private SaveHelper saveHelper = null;
		private int currentSlotNum = -1;

		public override void Open()
		{
            base.Open();
			if (menus == null) menus = VNFrameworkFindUtility.FindFirstInstance<Menus>();
			if (saveHelper == null) saveHelper = menus.GetComponent<SaveHelper>();
            StartCoroutine(SetupPanelAtEndOfFrame());
        }

        private IEnumerator SetupPanelAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            SetupPanel();
        }

        private void SetupPanel()
		{
			for (int slotNum = 0; slotNum < slots.Length; slotNum++)
			{
				var slot = slots[slotNum];
				var slotLabel = slot.GetComponentInChildren<UnityEngine.UI.Text>();
				if (slotLabel != null) slotLabel.text = saveHelper.GetSlotSummary(slotNum);
			}
		}

		public void SelectSlot(int slotNum)
		{
			currentSlotNum = slotNum;
			if (saveHelper.IsGameSavedInSlot(slotNum))
			{
				confirmOverwritePanel.SetActive(true);
			}
			else
			{
				ConfirmSave();
			}
		}

		public void ConfirmSave()
		{
			StartCoroutine(SaveCoroutine());
		}

		private IEnumerator SaveCoroutine()
		{
			if (Debug.isDebugBuild) Debug.Log("Dialogue System Menus: Saving game in slot " + currentSlotNum);
			saveInProgressPanel.SetActive(true);
			yield return null;
			saveHelper.SaveGame(currentSlotNum);
			saveInProgressPanel.SetActive(false);
			Close();
		}

	}

}