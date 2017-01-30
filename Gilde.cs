using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Elvenar
{
    [XmlRoot("Gilde")]
    public class Gilde
    {
        [XmlArray("Mitglieder")]
        public Spieler[] Mitglieder { get; set; }

        [XmlArray("Turniere")]
        public Turnier[] Turniere { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public DateTime Timestamp { get; set; }

        public Gilde()
        {
            Mitglieder = new Spieler[0];
            Turniere = new Turnier[0];
        }

        public void AddMitglied(Spieler spieler)
        {
            var liste = Mitglieder.ToList();
            liste.Add(spieler);
            Mitglieder = liste.ToArray();
        }

        public void AddTurnier(Turnier turnier)
        {
            var liste = Turniere.ToList();
            liste.Add(turnier);
            Turniere = liste.ToArray();
        }
    }

    public class Turnier
    {
        [XmlArray("Spieler")]
        public Spieler[] Spieler { get; set; }

        [XmlAttribute]
        public DateTime Timestamp { get; set; }

        public Turnier()
        {
            Spieler = new Spieler[0];
        }

        public void AddSpieler(Spieler spieler)
        {
            var liste = Spieler.ToList();
            liste.Add(spieler);
            Spieler = liste.ToArray();
        }
    }

    public class Spieler
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int Score { get; set; }
    }
}
