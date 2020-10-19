using System.Windows.Media;

namespace Disboard
{
    /// <summary>
    /// 간단하고 유용한 매크로 모음입니다.
    /// </summary>
    public static class Macro
    {
        static Color Color(this string html)
        {
            var color = System.Drawing.ColorTranslator.FromHtml(html);
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        /// <summary>
        /// 색상 문자열을 WPF 컨트롤에 사용할 수 있는 브러쉬로 만듭니다.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
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
