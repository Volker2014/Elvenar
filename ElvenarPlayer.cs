using System.Linq;
using System.Xml.Serialization;

namespace Elvenar
{
    [XmlRoot("Elvenarplayer")]
    public class ElvenarPlayer
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Race { get; set; }
        public string CityName { get; set; }
        public string Guild { get; set; }
        public Position CitySize { get; set; }

        [XmlArray("CityMap")]
        public Entity[] Entities { get; set; }

        [XmlArray("Areas")]
        public Area[] UnlockedAreas { get; set; }

        public ElvenarPlayer()
        {
            Entities = new Entity[0];
            UnlockedAreas = new Area[0];
        }

        public void AddEntity(Entity entity)
        {
            var liste = Entities.ToList();
            liste.Add(entity);
            Entities = liste.ToArray();
        }

        public void AddArea(Area area)
        {
            var liste = UnlockedAreas.ToList();
            liste.Add(area);
            UnlockedAreas = liste.ToArray();
        }
    }

    public class Area
    {
        public Position Start { get; set; }
        public Position Size { get; set; }
    }

    public class Entity
    {
        public int Id { get; set; }
        public string CityEntityId { get; set; }
        public string Type { get; set; }
        public int Level { get; set; }
        public Position Position { get; set; }
    }
}
