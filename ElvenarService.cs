using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Elvenar
{
    public class ElvenarService
    {
        public bool IsModified { get; private set; }

        public ElvenarEnv Load(string filename)
        {
            return Load(new StreamReader(filename));
        }

        public ElvenarEnv Load(TextReader reader)
        {
            IsModified = false;
            var serializer = new XmlSerializer(typeof(ElvenarEnv));
            var elvenar = serializer.Deserialize(reader) as ElvenarEnv;
            return elvenar;
        }

        public void Save(ElvenarEnv elvenar, int myPosition, IEnumerable<string> polierenList, 
            IEnumerable<string> quests, string filename)
        {
            Save(elvenar, myPosition, polierenList, quests, new StreamWriter(filename));
        }

        public void Save(ElvenarEnv elvenar, int myPosition, IEnumerable<string> polierenList, 
            IEnumerable<string> quests, TextWriter writer)
        {
            elvenar.MyPosition = myPosition;
            elvenar.Polieren = polierenList.ToArray();
            elvenar.Quests = quests.ToArray();
            var serializer = new XmlSerializer(elvenar.GetType());
            serializer.Serialize(writer, elvenar);
            IsModified = false;
        }
    }
}
