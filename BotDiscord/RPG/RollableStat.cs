﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Linq;

namespace BotDiscord.RPG
{
    public abstract class RollableStat : IEquatable<RollableStat>
    {
        private static XmlDocument xmlAliases = null;

        public RollableStat(string group, string name, int value)
        {
            Group = group;
            Name = name;
            Aliases = new List<string>();
            Value = value;

            if (xmlAliases == null)
            {
                string xmlString = string.Empty;
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BotDiscord.StatAliases.xml"))
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        xmlString = sr.ReadToEnd();
                    }
                }

                xmlAliases = new XmlDocument();
                xmlAliases.LoadXml(xmlString);
            }

            XmlNodeList aliasNodes = xmlAliases.SelectSingleNode($"Alisases//Stat[@value=\"{Name}\"]")?.ChildNodes;
            if (aliasNodes != null)
            {
                foreach (XmlElement element in aliasNodes)
                {
                    Aliases.Add(element.GetAttribute("value"));
                }
            }
        }

        public override string ToString()
        {
            return string.Format("<{0}, {1}>", Name, Value);
        }

        public bool Equals(RollableStat? other)
        {
            return other != null && other.Name == this.Name;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as RollableStat);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public string Group { get; set; }
        public string Name { get; set; }
        public List<string> Aliases { get; set; }
        public int Value { get; set; }
    }
}
