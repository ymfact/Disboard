using System.Windows.Media;

namespace Disboard
{
    public static class Macro
    {
        public static Color Color(this string html)
        {
            var color = System.Drawing.ColorTranslator.FromHtml(html);
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        public static Brush Brush(this string html)
            => new SolidColorBrush(html.Color());

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
