using System;
using System.Collections.Generic;
using UnityEngine;

namespace LoveAlgo
{
    /// <summary>
    /// BG/BGM/SFX/Sequence 리소스 매핑을 저장하는 중앙 카탈로그.
    /// CSV 임포터가 갱신하며, 런타임 시스템은 이 데이터를 참조합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "LoveAlgoResourceCatalog", menuName = "LoveAlgo/Resource Catalog")]
    public class LoveAlgoResourceCatalog : ScriptableObject
    {
        [Serializable]
        public class BackgroundEntry
        {
            public string id = string.Empty;
            public string description = string.Empty;
        }

        [Serializable]
        public class BgmEntry
        {
            public string id = string.Empty;
            public string resourceName = string.Empty;
        }

        [Serializable]
        public class SfxEntry
        {
            public string id = string.Empty;
            public string resourceName = string.Empty;
        }

    [Serializable]
    public class SequenceEntry
    {
        public string id = string.Empty;
        public string command = string.Empty;
    }

    [Serializable]
    public class StandingEntry
    {
        public int actorId;
        public string expression = string.Empty;
        public string resourcePath = string.Empty;
    }

    public List<BackgroundEntry> backgrounds = new List<BackgroundEntry>();
    public List<BgmEntry> bgmTracks = new List<BgmEntry>();
    public List<SfxEntry> soundEffects = new List<SfxEntry>();
    public List<SequenceEntry> sequenceTemplates = new List<SequenceEntry>();
    public List<StandingEntry> standingSprites = new List<StandingEntry>();

        public void SetBackgrounds(IEnumerable<BackgroundEntry> entries)
        {
            backgrounds.Clear();
            backgrounds.AddRange(entries);
        }

        public void SetBgmTracks(IEnumerable<BgmEntry> entries)
        {
            bgmTracks.Clear();
            bgmTracks.AddRange(entries);
        }

        public void SetSoundEffects(IEnumerable<SfxEntry> entries)
        {
            soundEffects.Clear();
            soundEffects.AddRange(entries);
        }

    public void SetSequenceTemplates(IEnumerable<SequenceEntry> entries)
    {
        sequenceTemplates.Clear();
        sequenceTemplates.AddRange(entries);
    }

    public void SetStandingSprites(IEnumerable<StandingEntry> entries)
    {
        standingSprites.Clear();
        standingSprites.AddRange(entries);
    }

    /// <summary>
    /// actorId와 expression으로 Standing 스프라이트 리소스 경로를 찾습니다.
    /// </summary>
    public string GetStandingResourcePath(int actorId, string expression)
    {
        var entry = standingSprites.Find(s => s.actorId == actorId && s.expression.Equals(expression, System.StringComparison.OrdinalIgnoreCase));
        return entry?.resourcePath ?? string.Empty;
    }

    /// <summary>
    /// 배경 ID로 리소스 경로를 찾습니다.
    /// </summary>
    public string GetBackgroundPath(string bgId)
    {
        if (string.IsNullOrEmpty(bgId)) return string.Empty;
        
        // 배경 카탈로그에 등록되어 있는지 확인
        var entry = backgrounds.Find(bg => bg.id.Equals(bgId, System.StringComparison.OrdinalIgnoreCase));
        if (entry != null)
        {
            return $"Backgrounds/{bgId}";
        }
        return string.Empty;
    }

    /// <summary>
    /// BGM ID로 리소스 경로를 찾습니다.
    /// </summary>
    public string GetBgmPath(string bgmId)
    {
        if (string.IsNullOrEmpty(bgmId)) return string.Empty;
        
        // BGM 카탈로그에 등록되어 있는지 확인
        var entry = bgmTracks.Find(bgm => bgm.id.Equals(bgmId, System.StringComparison.OrdinalIgnoreCase));
        if (entry != null)
        {
            return $"Audio/BGM/{bgmId}";
        }
        return string.Empty;
    }

    /// <summary>
    /// SFX ID로 리소스 경로를 찾습니다.
    /// </summary>
    public string GetSfxPath(string sfxId)
    {
        if (string.IsNullOrEmpty(sfxId)) return string.Empty;
        
        // SFX 카탈로그에 등록되어 있는지 확인
        var entry = soundEffects.Find(sfx => sfx.id.Equals(sfxId, System.StringComparison.OrdinalIgnoreCase));
        if (entry != null)
        {
            return $"Audio/SFX/{sfxId}";
        }
        return string.Empty;
    }
}
}

