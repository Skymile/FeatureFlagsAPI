using System.Text;

namespace FeatureFlagsAPI.Services;

public static class PathConverter
{
    public static string ToName(string path)
    {
        var sb = new StringBuilder("Get".Length + path.Length)
            .Append("Get");

        for (int i = 0; i < path.Length; ++i)
        {
            char c = path[i];
            bool isValid = c is not '/' and not '{' and not '}';

            if (i is 0 || isValid && path[i - 1] is '/' or '{' or '}')
                sb.Append(char.ToUpper(c));
            else if (isValid)
                sb.Append(c);
        }

        return sb.ToString();
    }
}
