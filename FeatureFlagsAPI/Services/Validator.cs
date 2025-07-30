using System.Diagnostics;
using System.Runtime.CompilerServices;

using FeatureFlagsAPI.Models;

namespace FeatureFlagsAPI.Services;

public static class Validator
{
    public static IDictionary<string, string[]> Validate(
            (Func<bool> condition, string errorMessage)[] conditions,
            [CallerMemberName] string propName = ""
        )
    {
        var validator = new Dictionary<string, string[]>();
        var errors = new List<string>();
        foreach ((Func<bool> condition, string errorMessage) in conditions) 
            try
            {
                if (condition())
                    errors.Add(errorMessage);
            }
            catch (Exception ex)
            {
                errors.Add(Debugger.IsAttached ? ex.Message : "Unexpected error occured.");
            }
        if (errors.Count > 0)
            validator[propName] = errors.ToArray();
        return validator;
    }

    public static bool IsTimeInSecondsValid(int seconds) =>
        seconds >= 0;

    public static bool IsIdValid(string id) =>
        !string.IsNullOrWhiteSpace(id);

    public static bool IsIdValid(int id) => 
        id > 0;

    public static bool IsFeatureFlagValid(FeatureFlag flag) =>
        flag is not null and { Id: >= 0 } && IsFeatureFlagNameValid(flag.Name);

    public static bool IsFeatureFlagNameValid(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        bool isCorrect = true;
        for (int i = 0; i < name.Length; i++)
            isCorrect &= char.IsLetterOrDigit(name[i]);
        return isCorrect;
    }

    public static bool IsUserValid(User user) =>
        user is not null && IsUserNameValid(user.Name);

    public static bool IsUserNameValid(string name) =>
        name?.Trim() is { Length: > 0 };
}
