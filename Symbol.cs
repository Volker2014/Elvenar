using System.Xml.Serialization;

namespace Elvenar
{
    public class Position
    {
        [XmlAttribute]
        public int X { get; set; }

        [XmlAttribute]
        public int Y { get; set; }

        public override string ToString()
        {
            return string.Format("X: {0}, Y {1}", X, Y);
        }
    }

    public class Symbol
    {
        [XmlAttribute]
        public string Name { get; set; }

        public Position Position { get; set; }
    }
}
