using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord
{
    [Flags]
    internal enum DiscordFormats
    {
        Bold = 1,
        Underline = 1 << 1,
        TextGrey = 1 << 2,
        TextRed = 1 << 3,
        TextGreen = 1 << 4,
        TextYellow = 1 << 5,
        TextBlue = 1 << 6,
        TextPink = 1 << 7,
        TextCyan = 1 << 8,
        TextWhite = 1 << 9,
        BackgroundDarkBlue = 1 << 10,
        BackgroundOrange = 1 << 11,
        BackgroundMarbleBLue = 1 << 12,
        BackgroundGreyish = 1 << 13,
        BackgroundGrey = 1 << 14,
        BackgroundIndigo = 1 << 15,
        BackgroundLightGrey = 1 << 16,
        BackgroundWhite = 1 << 17,
        None = 1 << 18,
    }

    internal static class DiscordFormater
    {
        public static string CodeBlockColor(object source, DiscordFormats formats = DiscordFormats.None)
        {
            if (formats == DiscordFormats.None)
                return source.ToString();
            StringBuilder sb = new();
            sb.Append("\u001b[");
            List<int> temp = new();

            foreach (DiscordFormats format in Enum.GetValues(formats.GetType()).Cast<DiscordFormats>().Where(x => formats.HasFlag(x)))
            {
                temp.Add(FormatToAnsiCode[format]);
            }

            sb.Append(string.Join(";", temp));
            sb.Append("m");

            sb.Append(source);
            sb.Append("\u001b[0m");
            return sb.ToString();
        }

        public static string GetRawAnsiCode(DiscordFormats formats)
        {
            if (formats == DiscordFormats.None)
                return "\u001b[0m";

            StringBuilder sb = new();
            sb.Append("\u001b[");

            List<int> temp = new();
            foreach (DiscordFormats format in Enum.GetValues(formats.GetType()).Cast<DiscordFormats>().Where(x => formats.HasFlag(x)))
            {
                temp.Add(FormatToAnsiCode[format]);
            }

            sb.Append(string.Join(';', temp));
            sb.Append('m');

            return sb.ToString();
        }

        private static Dictionary<DiscordFormats, int> FormatToAnsiCode = new Dictionary<DiscordFormats, int>()
        {
            [DiscordFormats.None] = 0,
            [DiscordFormats.Bold] = 1,
            [DiscordFormats.Underline] = 4,
            [DiscordFormats.TextGrey] = 30,
            [DiscordFormats.TextRed] = 31,
            [DiscordFormats.TextGreen] = 32,
            [DiscordFormats.TextYellow] = 33,
            [DiscordFormats.TextBlue] = 34,
            [DiscordFormats.TextPink] = 35,
            [DiscordFormats.TextCyan] = 36,
            [DiscordFormats.TextWhite] = 37,
            [DiscordFormats.BackgroundDarkBlue] = 40,
            [DiscordFormats.BackgroundOrange] = 41,
            [DiscordFormats.BackgroundMarbleBLue] = 42,
            [DiscordFormats.BackgroundGreyish] = 43,
            [DiscordFormats.BackgroundGrey] = 44,
            [DiscordFormats.BackgroundIndigo] = 45,
            [DiscordFormats.BackgroundLightGrey] = 46,
            [DiscordFormats.BackgroundWhite] = 47,
        };
    }
}
