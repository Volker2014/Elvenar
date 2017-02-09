using System.Xml.Serialization;

namespace Elvenar
{
    [XmlRoot("Elvenar")]
    public class ElvenarEnv
    {
        public int MyPosition { get; set; }

        [XmlArray("Symbols")]
        public Symbol[] Symbols { get; set; }

        [XmlArray("Macros")]
        public Macro[] Macros { get; set; }

        [XmlArray("Polieren")]
        public string[] Polieren { get; set; }

        [XmlArray("Quests")]
        public string[] Quests { get; set; }
    }
}
