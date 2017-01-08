using System.Xml.Serialization;

namespace Elvenar
{
    [XmlRoot("Elvenar")]
    public class ElvenarEnv
    {
        [XmlArray("Symbols")]
        public Symbol[] Symbols { get; set; }

        [XmlArray("Macros")]
        public Macro[] Macros { get; set; }
    }
}
