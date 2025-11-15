using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

/// <summary>
/// This script runs the back log. It records dialogue lines as they're played.
/// The ShowBackLog() method fills a back log window with the recorded lines.
/// </summary>
public class BackLogExample : MonoBehaviour
{

    public Transform logEntryContainer;
    public LogEntryTemplate logEntryTemplate;

    private List<Subtitle> log = new List<Subtitle>();
    private List<GameObject> instances = new List<GameObject>();

    private void Awake()
    {
        logEntryTemplate.gameObject.SetActive(false);
    }

    public void OnConversationLine(Subtitle subtitle)
    {
        if (!string.IsNullOrEmpty(subtitle.formattedText.text))
        {
            log.Add(subtitle);
        }
    }

    public void ShowBackLog()
    {
        instances.ForEach(instance => Destroy(instance));
        instances.Clear();
        foreach (Subtitle subtitle in log)
        {
            var instance = Instantiate(logEntryTemplate, logEntryContainer);
            instances.Add(instance.gameObject);
            instance.gameObject.SetActive(true);
            instance.Assign(subtitle);
        }
    }
}
