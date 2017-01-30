using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Elvenar
{
    [XmlRoot("City")]
    public class ElvenarCity : ElvenarBase<CityEntity>
    {
        public CityEntity[] Entities { get; set; }

        public ElvenarCity()
        {
            Entities = new CityEntity[0];
        }

        public void AddEntity(CityEntity entity)
        {
            Entities = Add(Entities, entity);
        }
    }

    public class CityEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Race { get; set; }
        public string Type { get; set; }
        public int Level { get; set; }
        public Position Size { get; set; }
        public DateTime ConstructionTime { get; set; }
        public EntityUpgrade Upgrade { get; set; }
        public Production Production { get; set; }
        public int RankingPoints { get; set; }

        public CityEntity()
        { }

        public CityEntity(dynamic data)
        {
            Id = data["id"];
            Name = data["name"];
            Race = data["race"];
            Type = data["type"];
            Level = data["level"];
            Size = new Position { X = data["width"], Y = data["length"] };
            ConstructionTime = new DateTime(0).AddSeconds(DynamicExtension.GetKeyValue(data, "construction_time") ?? 0);
            Upgrade = new EntityUpgrade(DynamicExtension.GetKeyValue(data, "upgradeRequirements"));
            Production = new Production(DynamicExtension.GetKeyValue(data, "production"));
            RankingPoints = DynamicExtension.GetKeyValue(data, "rankingPoints") ?? 0;
        }
    }

    public class Production
    {
        public Option[] Options { get; set; }

        public Production()
        { }

        public Production(dynamic data)
        {
            var list = new List<Option>();
            if (data != null)
            {
                var production = DynamicExtension.GetKeyValue(data, "production");
                if (production != null)
                    foreach (dynamic option in production["products"])
                    {
                        list.Add(new Option(option));
                    }
            }
            Options = list.ToArray();
        }
    }

    public class Option
    {
        public int Id { get; set; }
        public int Money { get; set; }
        public int Population { get; set; }
        public int Supplies { get; set; }
        public int Mana { get; set; }
        public int Scrolls { get; set; }
        public DateTime Time { get; set; }

        public Option()
        { }

        public Option(dynamic data)
        {
            Id = data["production_option"];
            Time = new DateTime().AddSeconds(data["production_time"]);
            Scrolls = DynamicExtension.GetKeyValue(data["requirements"]["resources"]["resources"], "scrolls") ?? 0;
            Mana = DynamicExtension.GetKeyValue(data["requirements"]["resources"]["resources"], "mana") ?? 0;
            Money = DynamicExtension.GetKeyValue(data["requirements"]["resources"]["resources"], "money") ?? 0;
            Supplies = DynamicExtension.GetKeyValue(data["requirements"]["resources"]["resources"], "supplies") ?? 0;
            Population = DynamicExtension.GetKeyValue(data["requirements"]["resources"]["resources"], "population") ?? 0;
        }
    }

    public class EntityUpgrade
    {
        public int Money { get; set; }
        public int Population { get; set; }
        public int Culture { get; set; }   
        public int Mana { get; set; }      
        
        public EntityUpgrade()
        { }

        public EntityUpgrade(dynamic data)
        {
            if (data == null) return;

            Money = DynamicExtension.GetKeyValue(data["resources"]["resources"], "money") ?? 0;
            Population = DynamicExtension.GetKeyValue(data["resources"]["resources"], "population") ?? 0;
            Culture = DynamicExtension.GetKeyValue(data, "demand_for_happiness") ?? 0;
            Mana = DynamicExtension.GetKeyValue(data["resources"]["resources"], "population") ?? 0;
        }
    }
}