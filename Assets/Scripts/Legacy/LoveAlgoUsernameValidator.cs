#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace LoveAlgo.UI.Gameplay
{
    /// <summary>
    /// 공통 유저네임 검증 규칙을 제공하는 정적 헬퍼.
    /// </summary>
    public static class LoveAlgoUsernameRules
    {
        public const int MinLength = 2;
        public const int MaxLength = 10;

        private static readonly HashSet<char> AllowedHangulCharacters = BuildAllowedHangulCharacters();
        private static readonly HashSet<char> AllowedLatinCharacters = BuildAllowedLatinCharacters();
        private static readonly HashSet<char> ExplicitlyBannedHangul = new HashSet<char>
        {
            '뀂', '웳', '쪣', '욊'
        };

        public static IReadOnlyCollection<char> HangulCharacters => AllowedHangulCharacters;
        public static IReadOnlyCollection<char> LatinCharacters => AllowedLatinCharacters;

        public static IEnumerable<char> GetAllAllowedCharacters()
        {
            foreach (var c in AllowedHangulCharacters)
            {
                yield return c;
            }

            foreach (var c in AllowedLatinCharacters)
            {
                yield return c;
            }
        }

        public static char FilterCharacter(string currentText, int insertionIndex, char addedChar, out UsernameValidationError error)
        {
            error = UsernameValidationError.None;

            if (char.IsWhiteSpace(addedChar))
            {
                error = UsernameValidationError.InvalidCharacter;
                return '\0';
            }

            if (IsAllowedHangul(addedChar))
            {
                if (WouldCauseMixedScript(currentText, UsernameScriptKind.Hangul))
                {
                    error = UsernameValidationError.MixedScript;
                    return '\0';
                }

                return addedChar;
            }

            if (IsAllowedLatin(addedChar))
            {
                if (WouldCauseMixedScript(currentText, UsernameScriptKind.Latin))
                {
                    error = UsernameValidationError.MixedScript;
                    return '\0';
                }

                return addedChar;
            }

            error = UsernameValidationError.InvalidCharacter;
            return '\0';
        }

        public static UsernameValidationResult Validate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return UsernameValidationResult.Fail(UsernameValidationError.Empty);
            }

            var trimmed = input.Trim();
            if (trimmed.Length < MinLength)
            {
                return UsernameValidationResult.Fail(UsernameValidationError.LengthTooShort);
            }

            if (trimmed.Length > MaxLength)
            {
                return UsernameValidationResult.Fail(UsernameValidationError.LengthTooLong);
            }

            var detectedScript = UsernameScriptKind.None;

            foreach (var ch in trimmed)
            {
                var charScript = GetScriptKind(ch);
                if (charScript == UsernameScriptKind.None)
                {
                    return UsernameValidationResult.Fail(UsernameValidationError.InvalidCharacter, ch);
                }

                if (detectedScript == UsernameScriptKind.None)
                {
                    detectedScript = charScript;
                }
                else if (detectedScript != charScript)
                {
                    return UsernameValidationResult.Fail(UsernameValidationError.MixedScript, ch);
                }

                if (charScript == UsernameScriptKind.Hangul && !IsAllowedHangul(ch))
                {
                    return UsernameValidationResult.Fail(UsernameValidationError.InvalidCharacter, ch);
                }

                if (charScript == UsernameScriptKind.Latin && !IsAllowedLatin(ch))
                {
                    return UsernameValidationResult.Fail(UsernameValidationError.InvalidCharacter, ch);
                }
            }

            return UsernameValidationResult.Success(trimmed);
        }

        private static UsernameScriptKind GetScriptKind(char ch)
        {
            if (IsAllowedHangul(ch))
            {
                return UsernameScriptKind.Hangul;
            }

            if (IsAllowedLatin(ch))
            {
                return UsernameScriptKind.Latin;
            }

            return UsernameScriptKind.None;
        }

        private static bool WouldCauseMixedScript(string currentText, UsernameScriptKind newScript)
        {
            var existingScript = UsernameScriptKind.None;

            foreach (var ch in currentText)
            {
                var script = GetScriptKind(ch);
                if (script == UsernameScriptKind.None)
                {
                    continue;
                }

                if (existingScript == UsernameScriptKind.None)
                {
                    existingScript = script;
                }
                else if (existingScript != script)
                {
                    return true;
                }
            }

            if (existingScript == UsernameScriptKind.None)
            {
                return false;
            }

            return existingScript != newScript;
        }

        private static bool IsAllowedHangul(char ch)
        {
            if (!AllowedHangulCharacters.Contains(ch))
            {
                return false;
            }

            return !ExplicitlyBannedHangul.Contains(ch);
        }

        private static bool IsAllowedLatin(char ch)
        {
            return AllowedLatinCharacters.Contains(ch);
        }

        private static HashSet<char> BuildAllowedHangulCharacters()
        {
            var allowedInitialIndices = new HashSet<int> { 0, 2, 3, 5, 6, 7, 9, 11, 12, 14, 15, 16, 17, 18 };
            var allowedMedialIndices = new HashSet<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 18, 19, 20 };
            var allowedFinalIndices = new HashSet<int> { 0, 1, 4, 7, 8, 16, 17, 19, 21, 22, 23, 24, 25, 26, 27 };

            var result = new HashSet<char>();

            foreach (var initial in allowedInitialIndices)
            {
                foreach (var medial in allowedMedialIndices)
                {
                    foreach (var final in allowedFinalIndices)
                    {
                        int codePoint = 0xAC00 + (initial * 21 + medial) * 28 + final;
                        if (codePoint > 0xD7A3)
                        {
                            continue;
                        }

                        result.Add((char)codePoint);
                    }
                }
            }

            return result;
        }

        private static HashSet<char> BuildAllowedLatinCharacters()
        {
            var result = new HashSet<char>();

            for (char c = 'A'; c <= 'Z'; c++)
            {
                result.Add(c);
            }

            for (char c = 'a'; c <= 'z'; c++)
            {
                result.Add(c);
            }

            return result;
        }
    }

    public enum UsernameScriptKind
    {
        None,
        Hangul,
        Latin
    }

    public enum UsernameValidationError
    {
        None,
        Empty,
        LengthTooShort,
        LengthTooLong,
        InvalidCharacter,
        MixedScript
    }

    public readonly struct UsernameValidationResult
    {
        private UsernameValidationResult(bool isValid, UsernameValidationError error, char? invalidChar, string? sanitized)
        {
            IsValid = isValid;
            Error = error;
            InvalidCharacter = invalidChar;
            SanitizedValue = sanitized;
        }

        public bool IsValid { get; }
        public UsernameValidationError Error { get; }
        public char? InvalidCharacter { get; }
        public string? SanitizedValue { get; }

        public static UsernameValidationResult Success(string sanitized) =>
            new UsernameValidationResult(true, UsernameValidationError.None, null, sanitized);

        public static UsernameValidationResult Fail(UsernameValidationError error, char? invalidChar = null) =>
            new UsernameValidationResult(false, error, invalidChar, null);
    }

    /// <summary>
    /// TMP_InputField에 바인딩하여 실시간 필터링 및 최종 검증을 수행한다.
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public sealed class LoveAlgoUsernameValidator : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField = default!;
        [SerializeField] private TextMeshProUGUI? errorLabel;
        [SerializeField] private UnityEvent<string>? onValidationSucceeded;
        [SerializeField] private UnityEvent<UsernameValidationError>? onValidationFailed;

        private void Reset()
        {
            inputField = GetComponent<TMP_InputField>();
            errorLabel = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            EnsureInputField();
            inputField.onValidateInput = HandleValidateInput;
            inputField.onEndEdit.AddListener(HandleEndEdit);
        }

        private void OnDisable()
        {
            if (inputField == null)
            {
                return;
            }

            inputField.onValidateInput = null;
            inputField.onEndEdit.RemoveListener(HandleEndEdit);
        }

        private void EnsureInputField()
        {
            if (inputField == null)
            {
                inputField = GetComponent<TMP_InputField>();
            }
        }

        private char HandleValidateInput(string text, int charIndex, char addedChar)
        {
            var filtered = LoveAlgoUsernameRules.FilterCharacter(text, charIndex, addedChar, out var error);
            if (filtered == '\0')
            {
                WriteError(error, addedChar);
            }
            else
            {
                ClearError();
            }

            return filtered;
        }

        private void HandleEndEdit(string _)
        {
            var result = LoveAlgoUsernameRules.Validate(inputField.text);
            if (result.IsValid)
            {
                ClearError();
                onValidationSucceeded?.Invoke(result.SanitizedValue!);
            }
            else
            {
                WriteError(result.Error, result.InvalidCharacter);
                onValidationFailed?.Invoke(result.Error);
            }
        }

        private void WriteError(UsernameValidationError error, char? problematicChar)
        {
            if (errorLabel == null)
            {
                return;
            }

            errorLabel.text = error switch
            {
                UsernameValidationError.Empty => "이름을 입력해주세요.",
                UsernameValidationError.LengthTooShort => $"이름은 최소 {LoveAlgoUsernameRules.MinLength}자 이상이어야 합니다.",
                UsernameValidationError.LengthTooLong => $"이름은 최대 {LoveAlgoUsernameRules.MaxLength}자까지 입력할 수 있습니다.",
                UsernameValidationError.MixedScript => "한글 또는 영어 중 한 언어만 사용할 수 있습니다.",
                UsernameValidationError.InvalidCharacter => problematicChar.HasValue
                    ? $"'{problematicChar}' 문자는 사용할 수 없습니다."
                    : "사용할 수 없는 문자가 포함되어 있습니다.",
                _ => string.Empty
            };
        }

        private void ClearError()
        {
            if (errorLabel != null)
            {
                errorLabel.text = string.Empty;
            }
        }
    }
}

