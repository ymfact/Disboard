namespace Disboard
{
    public static class Macro
    {
        /// <summary>
        /// Red
        /// </summary>
        public static string R(string text) => $"```diff\n- {text}\n```";
        /// <summary>
        /// Orange
        /// </summary>
        public static string O(string text) => $"```css\n[ {text} ]\n```";
        /// <summary>
        /// Yellow
        /// </summary>
        public static string Y(string text) => $"```fix\n{text}\n```";
        /// <summary>
        /// Green
        /// </summary>
        public static string G(string text) => $"```diff\n+ {text}\n```";
        /// <summary>
        /// Teal
        /// </summary>
        public static string T(string text) => $"```bash\n\" {text} \"\n```";
        /// <summary>
        /// Blue
        /// </summary>
        public static string B(string text) => $"```ini\n[ {text} ]\n```";
        /// <summary>
        /// White
        /// </summary>
        public static string W(string text) => $"```{(text.Length == 0 ? " " : text)}```";
    }
}
