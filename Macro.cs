using System.Xml.Serialization;

namespace Elvenar
{
    public class Step
    {
        [XmlAttribute]
        public string Symbol { get; set; }

        [XmlAttribute]
        public int Delay { get; set; }
    }

    public class Macro
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlArray("Steps")]
        public Step[] Steps { get; set; }
    }
}
