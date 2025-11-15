using System;
using UnityEngine;

namespace LoveAlgo.Data
{
    public enum EpisodeStage
    {
        Intro,
        FirstEvent,
        Festival,
        SecondEvent,
        Retreat,
        ThirdEvent,
        Confession,
        Ending,
        MessengerWindow,
        DailySlice
    }

    public enum EpisodeGiftWindow
    {
        None,
        SecondEvent,
        ThirdEvent
    }

    [Serializable]
    public struct EpisodePointBreakdown
    {
        [SerializeField] private int eventPoints;
        [SerializeField] private int dialoguePointCap;
        [SerializeField] private int messengerPointCap;
        [SerializeField] private int miniGamePointCap;
        [SerializeField] private int giftBonusCap;

        public int EventPoints => eventPoints;
        public int DialoguePointCap => dialoguePointCap;
        public int MessengerPointCap => messengerPointCap;
        public int MiniGamePointCap => miniGamePointCap;
        public int GiftBonusCap => giftBonusCap;
    }

    [CreateAssetMenu(fileName = "EpisodeDefinition", menuName = "LoveAlgo/Data/Episode Definition")]
    public sealed class EpisodeDefinition : ScriptableObject
    {
        [SerializeField] private string episodeId = "first_meeting";
        [SerializeField] private string displayName = "1차 개인 이벤트";
        [SerializeField] private EpisodeStage stage = EpisodeStage.FirstEvent;
        [SerializeField] private ScheduleMode scheduleMode = ScheduleMode.StoryEvent;
        [SerializeField] private EpisodePointBreakdown points;
        [SerializeField] private EpisodeGiftWindow giftWindow = EpisodeGiftWindow.None;
        [SerializeField] private bool locksFreeActions = true;
        [SerializeField, TextArea] private string summary;

        public string EpisodeId => episodeId;
        public string DisplayName => displayName;
        public EpisodeStage Stage => stage;
        public ScheduleMode ScheduleMode => scheduleMode;
        public EpisodePointBreakdown Points => points;
        public EpisodeGiftWindow GiftWindow => giftWindow;
        public bool LocksFreeActions => locksFreeActions;
        public string Summary => summary;
    }
}
