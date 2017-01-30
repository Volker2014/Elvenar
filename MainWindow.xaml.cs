using MouseKeyboardLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Elvenar
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<Macro> Macros { get; set; }
        public ObservableCollection<string> PolierenList { get; set; }
        public ObservableCollection<string> QuestList { get; set; }

        public Brush SaveBorder { get; set; }

        public string NewQuest
        {
            set
            {
                if (cbxQuestCurrent.SelectedItem != null)
                {
                    if (string.IsNullOrEmpty(cbxQuestCurrent.SelectedItem as string))
                        cbxQuestCurrent.Items.RemoveAt(cbxQuestCurrent.SelectedIndex);
                    return;
                }
                if (!string.IsNullOrEmpty(value))
                {
                    QuestList.Add(value);
                    cbxQuestCurrent.SelectedItem = value;
                }
            }
        }

        private string _filename = "";
        private bool _isModified = false;
        private MacroService _macroService;
        private ElvenarService _elvenarService;
        private ElvenarEnv _elvenar;
        private Task _macroTask;
        private CancellationTokenSource _macroToken;
        private KeyboardHook _keyHook;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _elvenarService = new ElvenarService();
            InitServices(new ElvenarEnv());
            Loaded += Load;
            SaveBorder = Brushes.Black;
        }

        private void InitServices(ElvenarEnv elvenar)
        {
            _elvenar = elvenar;
            Macros = new ObservableCollection<Macro>(elvenar.Macros ?? new Macro[0]);
            PolierenList = new ObservableCollection<string>(elvenar.Polieren ?? new string[0]);
            QuestList = new ObservableCollection<string>(elvenar.Quests ?? new string[0]);
            _macroService = new MacroService(elvenar.Symbols);
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Elvenar Macro|*.elm";
            if (!string.IsNullOrEmpty(_filename))
            {
                dlg.FileName = _filename;
            }
            else
            {
                dlg.InitialDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            }
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            _filename = dlg.FileName;
            Title = "Elvenar Makros - " + _filename;

            ElvenarEnv elvenar = _elvenarService.Load(_filename);
            InitServices(elvenar);

            NotifyPropertyChanged("Macros");
            NotifyPropertyChanged("PolierenList");
            NotifyPropertyChanged("QuestList");
        }

        private void ShowHelp(object sender, RoutedEventArgs e)
        {
            var message = "Elvenar Makro Tool - Version " + Assembly.GetExecutingAssembly().GetName().Version +
                "\n\nLinke Seite:\nListe der Makros. Auswählen und Ausführen\n" +
                "Mitte:\nListe der Symbole mit Warten nach dem Klick. Reihenfolge über Pfeil nach oben/unten. "+
                "Entfernen über Pfeil nach rechts.\n" +
                "rechte Seite:\nListe der Symbole. Bestimme, um neues Symbol mit Position aufzunehmen\n" +
                "Auswählen und zum Makro hinzufügen (Pfeil nach links) oder Position anzeigen.";
            System.Windows.Forms.MessageBox.Show(message, "Hilfe Elvenar Makro", MessageBoxButtons.OK);
        }

        private void OpenSymbolWindow(object sender, RoutedEventArgs e)
        {
            var viewModel = new SymbolViewModel(_elvenar.Symbols);
            var symbolWindow = new SymbolWindow(viewModel, new MouseHook());

            if (symbolWindow.ShowDialog() != true) return;

            _elvenar.Symbols = viewModel.Symbols.ToArray();
            var service = new MacroService(_elvenar.Symbols);
            foreach (var replace in viewModel.ReplaceSymbolNames)
                service.ReplaceSymbolName(Macros, replace.Key, replace.Value);
            SetModified(true);
        }

        private void OpenMacroWindow(object sender, RoutedEventArgs e)
        {
            var viewModel = new MacroViewModel(_elvenar.Symbols, _elvenar.Macros);
            var macroWindow = new MacroWindow(viewModel);
            if (macroWindow.ShowDialog() != true) return;

            _elvenar.Macros = viewModel.Macros.ToArray();
            Macros = new ObservableCollection<Macro>(_elvenar.Macros ?? new Macro[0]);
            SetModified(true);
        }

        private void RunMacro(object sender, RoutedEventArgs e)
        {
            var macro = dataGridMacros.SelectedItem as Macro;
            if (macro == null) return;
            StartTask(() => _macroService.Run(macro.Steps, LeftClick), () => dataGridMacros.SelectedItem = null);
        }

        private void StartTask(Action startAction, Action endAction)
        {
            _keyHook = new KeyboardHook();
            _keyHook.KeyDown += KeyHook_KeyDown;
            _keyHook.Start();
            startAction();
            endAction();
            _keyHook.Stop();
            _keyHook = null;
            return;

            _macroToken = new CancellationTokenSource();
            var backgroundScheduler = TaskScheduler.Default;
            var context = TaskScheduler.FromCurrentSynchronizationContext();
            _keyHook = new KeyboardHook();
            _keyHook.KeyDown += KeyHook_KeyDown;
            _keyHook.Start();
            _macroTask = Task.Factory.StartNew(() =>
            {
                startAction();
            }, _macroToken.Token, TaskCreationOptions.LongRunning, backgroundScheduler).ContinueWith((tsk) =>
            {
                _keyHook.Stop();
                _keyHook = null;
                endAction();
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, context);
        }

        private void KeyHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (_macroTask == null) return;
            if (e.KeyCode != Keys.Escape) return;
            _keyHook.Stop();
            _keyHook = null;
            _macroToken.Cancel();        
        }

        private void RunPolieren(object sender, RoutedEventArgs e)
        {
            var polieren = cbxPolieren.SelectedItem as string;
            var macro = Macros.FirstOrDefault(m => m.Name == polieren);
            if (macro == null) return;
            StartTask(() => _macroService.Run(macro.Steps, LeftClick), () => 
                {
                    if (cbxPolieren.SelectedIndex + 1 < cbxPolieren.Items.Count)
                        cbxPolieren.SelectedIndex++;
                    else
                        cbxPolieren.SelectedIndex = 0;
                    NotifyPropertyChanged("PolierenList");
                });
        }

        private void RunKampfPaladin(object sender, RoutedEventArgs e)
        {
            StartTask(() => RunMacro("Autom. Kampf Paladin"), () => { });
        }

        private void RunKampf(object sender, RoutedEventArgs e)
        {
            StartTask(() => RunMacro("Autom. Kampf"), () => { });
        }

        private void RunProvinz(object sender, RoutedEventArgs e)
        {
            StartTask(() => RunMacro("zur Provinz"), () => { });
        }

        private void RunFinish(object sender, RoutedEventArgs e)
        {
            RunQuestLoop("Quest Abschliessen");
        }

        private void RunCancel(object sender, RoutedEventArgs e)
        {
            RunQuestLoop("Quest Ablehnen");
        }

        private void RunQuestLoop(string questMacro)
        {
            if (cbxQuestNext.SelectedIndex == -1)
                cbxQuestNext.SelectedIndex = GetNextIndex(cbxQuestCurrent.SelectedIndex);
            int currentIndex = cbxQuestCurrent.SelectedIndex;
            int nextIndex = cbxQuestNext.SelectedIndex;
            StartTask(() =>
            {
                currentIndex = RunQuest(questMacro, currentIndex);
                while (currentIndex != nextIndex)
                {
                    currentIndex = RunQuest("Quest Ablehnen", currentIndex);
                }
            }, () =>
            {
                cbxQuestCurrent.SelectedIndex = cbxQuestNext.SelectedIndex;
                cbxQuestNext.SelectedIndex = GetNextIndex(currentIndex);
                NotifyPropertyChanged("QuestList");
            });
        }

        private int RunQuest(string questMacro, int index)
        {
            if (!RunMacro(questMacro)) return index;
            return GetNextIndex(index);
        }

        private int GetNextIndex(int index)
        {
            if (index + 1 < QuestList.Count)
                return index+1;
            return 0;
        }

        private void CityEntities(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Http response|*.json";
            dlg.InitialDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            var reader = new StreamReader(dlg.FileName);
            string jsonString = reader.ReadToEnd();
            var jss = new JavaScriptSerializer();
            jss.MaxJsonLength = 10000000;
            var table = jss.Deserialize<dynamic>(jsonString);

            var city = new ElvenarCity();
            foreach (var row in table)
            {
                city.AddEntity(new CityEntity(row));
            }

            var filename = Path.Combine(Path.GetDirectoryName(dlg.FileName), dlg.FileName + ".elc");
            var writer = new StreamWriter(filename);
            city.Write(writer);

            Mouse.OverrideCursor = null;
        }

        private void TransformList(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Http response|*.json";
            dlg.InitialDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            var reader = new StreamReader(dlg.FileName);
            string jsonString = reader.ReadToEnd();
            var jss = new JavaScriptSerializer();
            jss.MaxJsonLength = 10000000;
            var table = jss.Deserialize<dynamic>(jsonString);
            object data = null;
            foreach (var row in table)
            {
                if (row["requestClass"] == "GuildService")
                    data = CreateGilde(row);
                else if (row["requestClass"] == "TournamentService")
                {
                    var turnier = CreateTurnier(row);
                    (data as Gilde).AddTurnier(turnier);
                }
                else if (row["requestClass"] == "RankingService")
                {
                    var turnier = CreateRanking(row);
                    var gilde = new Gilde();
                    gilde.AddTurnier(turnier);
                    data = gilde;
                }
                else if (row["requestClass"] == "StartupService")
                {
                    data = CreateCityMap(row, "user_data", "user_name");
                    ElvenarArchitekt(data as ElvenarPlayer, dlg.FileName);
                }
                else if (row["requestClass"] == "OtherPlayerService")
                {
                    data = CreateCityMap(row, "other_player", "name");
                    ElvenarArchitekt(data as ElvenarPlayer, dlg.FileName);
                }
            }

            var filename = Path.Combine(Path.GetDirectoryName(dlg.FileName), dlg.FileName + ".ele");
            var serializer = new XmlSerializer(data.GetType());
            var writer = new StreamWriter(filename);
            serializer.Serialize(writer, data);

            Mouse.OverrideCursor = null;
        }

        private object CreateCityMap(dynamic row, string userdataTag, string usernameTag)
        {
            var responseData = row["responseData"];
            var userdata = responseData[userdataTag];
            var player = new ElvenarPlayer
            {
                Name = userdata[usernameTag],
                Id = Convert.ToInt32(userdata["player_id"]),
                Race = userdata["race"],
                CityName = userdata["city_name"],
                Guild = userdata["guild_info"]["name"],
                CitySize = GetKeyValue(responseData, "mapSize") != null ? new Position
                {
                    X = responseData["mapSize"]["width"],
                    Y = responseData["mapSize"]["height"]
                } : null
            };
            foreach (var entity in responseData["city_map"]["entities"])
            {
                player.AddEntity(new Entity
                {
                    Id = Convert.ToInt32(entity["id"]),
                    CityEntityId = entity["cityentity_id"],
                    Type = entity["type"],
                    Level = Convert.ToInt32(entity["level"]),
                    Position = new Position
                    {
                        X = GetKeyValue(entity, "x") ?? 0,
                        Y = GetKeyValue(entity, "y") ?? 0
                    }
                });
            }
            foreach (var area in responseData["city_map"]["unlocked_areas"])
            {
                player.AddArea(new Area
                {
                    Start = new Position
                    {
                        X = GetKeyValue(area, "x") ?? 0,
                        Y = GetKeyValue(area, "y") ?? 0
                    },
                    Size = new Position
                    {
                        X = area["width"],
                        Y = area["length"]
                    }
                });
            }
            return player;
        }

        private Turnier CreateRanking(dynamic row)
        {
            var responseData = row["responseData"];
            var turnier = new Turnier();
            var rankings = GetKeyValue(responseData, "rankings");
            if (rankings == null) return turnier;
            foreach (var ranking in rankings)
            {
                if (ranking["__class__"] == "PlayerRankingVO")
                    turnier.AddSpieler(new Spieler { Name = ranking["player"]["name"], Score = Convert.ToInt32(ranking["points"]) });
                else if (ranking["__class__"] == "GuildRankingVO")
                    turnier.AddSpieler(new Spieler { Name = ranking["guild_info"]["name"], Score = Convert.ToInt32(ranking["points"]) });
                else if (ranking["__class__"] == "TournamentRankingVO")
                    turnier.AddSpieler(new Spieler { Name = ranking["player"]["name"],
                        Score = Convert.ToInt32(DynamicExtension.GetKeyValue(ranking, "points") ?? 0)});
            }
            return turnier;
        }

        private Turnier CreateTurnier(dynamic row)
        {
            var responseData = row["responseData"];
            var turnier = new Turnier();
            turnier.Timestamp = DateTime.Now;
            var contributors = GetKeyValue(responseData, "contributors");
            if (contributors == null) return turnier;
            foreach (var contributor in contributors)
            {
                turnier.AddSpieler(new Spieler { Name = contributor["player"]["name"], Score = Convert.ToInt32(contributor["score"]) });
            }

            return turnier;
        }

        private static object CreateGilde(dynamic row)
        {
            var responseData = row["responseData"];
            var gilde = new Gilde();
            gilde.Name = responseData["name"];
            gilde.Timestamp = DateTime.Now;
            foreach (var member in responseData["members"])
            {
                gilde.AddMitglied(new Spieler { Name = member["player"]["name"], Score = Convert.ToInt32(member["score"]) });
            }
            return gilde;
        }

        public static dynamic GetKeyValue(dynamic settings, string key)
        {
            try
            {
                return settings[key];
            }
            catch
            {
                return null;
            }
        }

        private void ElvenarArchitekt(ElvenarPlayer player, string fileName)
        {
            var table = new Dictionary<string, object>();
            var cityMap = new Dictionary<string, object>();

            var areas = new List<object>();
            foreach (var area in player.UnlockedAreas)
            {
                var item = new Dictionary<string, object>();
                item["x"] = area.Start.X;
                item["y"] = area.Start.Y;
                item["width"] = area.Size.X;
                item["length"] = area.Size.Y;
                areas.Add(item);
            }
            cityMap["unlocked_areas"] = areas.ToArray();

            var entities = new List<object>();
            foreach (var entity in player.Entities)
            {
                var item = new Dictionary<string, object>();
                item["id"] = entity.Id;
                item["cityentity_id"] = entity.CityEntityId;
                item["x"] = entity.Position.X;
                item["y"] = entity.Position.Y;
                entities.Add(item);
            }
            cityMap["entities"] = entities.ToArray();

            table["city_map"] = cityMap;
            table["user_data"] = new Dictionary<string, object> { { "race", player.Race } };

            var writer = new StreamWriter(fileName + ".ela");
            var jss = new JavaScriptSerializer();
            var jsonString = jss.Serialize(table);
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(jsonString);
            string decodedString = System.Convert.ToBase64String(toEncodeAsBytes);
            writer.Write(decodedString);
            writer.Close();
        }

        private bool RunMacro(string macroName)
        {
            var macro = Macros.FirstOrDefault(m => m.Name == macroName);
            if (macro == null) return false;
            _macroService.Run(macro.Steps, LeftClick);
            return true;
        }

        private void LeftClick(int x, int y, int delay)
        {
            MouseSimulator.Position = new System.Drawing.Point(x, y);
            MouseSimulator.Click(MouseKeyboardLibrary.MouseButton.Left);
            Thread.Sleep(delay * 1000);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (_filename == null)
            {
                var dlg = new SaveFileDialog();
                dlg.FileName = "elvenar.xml";
                dlg.InitialDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
                _filename = dlg.FileName;
            }

            _elvenarService.Save(_elvenar, PolierenList, QuestList, _filename);

            SetModified(_elvenarService.IsModified);
        }

        private void SetModified(bool modified)
        {
            _isModified = modified;
            if (modified)
                SaveBorder = Brushes.Red;
            else
                SaveBorder = Brushes.Black;
        }

        private void RemovePolieren(object sender, RoutedEventArgs e)
        {
            var selectedPolieren = cbxPolieren.SelectedItem as string;
            if (selectedPolieren == null) return;
            PolierenList.Remove(selectedPolieren);
            SetModified(true);
            NotifyPropertyChanged("PolierenList");
        }

        private void AddPolieren(object sender, RoutedEventArgs e)
        {
            var macro = dataGridMacros.SelectedItem as Macro;
            if (macro == null) return;
            if (PolierenList.Contains(macro.Name)) return;
            PolierenList.Add(macro.Name);
            SetModified(true);
            NotifyPropertyChanged("PolierenList");
        }
    }
}
