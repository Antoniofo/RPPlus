using Life;
using Life.DB;
using Life.Network;
using Life.UI;
using System.Collections;
using System.IO;
using System.Linq;
using Socket.Newtonsoft.Json;
using UnityEngine;

namespace RPPlus
{
    public class RoleplayPlus : Plugin
    {
        private LifeServer _server;
        private long _lastRob;
        private static string _dirPath;
        public static string ConfPath;
        private RPPlusConfig _config;

        public RoleplayPlus(IGameAPI api) : base(api)
        {
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();

            InitDirectory();

            _server = Nova.server;
            _server.OnPlayerTryToHackATM += (player) =>
            {
                _server.GetCountOnlineBiz(1, true);
                UIPanel uiPanel = new UIPanel("Hack Tool V0.3", UIPanel.PanelType.Input).AddButton(
                        "Commencer le Hack", (ui =>
                        {
                            if (_lastRob > Nova.UnixTimeNow())
                            {
                                player.SendText(
                                    "<color=red>Veuillez attendre quelques heures avant de braquer ce DAB.</color > ");
                            }
                            else
                            {
                                _server.lifeManager.StartCoroutine(
                                    this.GetMoney(player, player.setup.transform.position));
                                player.SendText("<color=red>HackTool.exe et en cours de lancement</color > ");
                                player.ClosePanel(ui);
                            }
                        })).AddButton("Close", (player.ClosePanel))
                    .SetText("HackTool.exe Error de piratage lancement de FIX-HackTool.exe");
                player.ShowPanelUI(uiPanel);
            };

            SChatCommand reload = new SChatCommand("/reloadrpplus", "Reload the config of RPPlus", "/reloadrpplus",
                (player, arg) => { _config = JsonConvert.DeserializeObject<RPPlusConfig>(ConfPath); });
            reload.Register();
        }

        private void InitDirectory()
        {
            _dirPath = $"{pluginsPath}/RPPlus";
            ConfPath = $"{_dirPath}/config.json";
            if (!Directory.Exists(_dirPath))
                Directory.CreateDirectory(_dirPath);
            if (!File.Exists(ConfPath))
            {
                _config = new RPPlusConfig() { hackCooldown = 28800, minMoney = 500, maxMoney = 1000 };
                var json = JsonConvert.SerializeObject(_config);
                File.WriteAllText(ConfPath, json);
            }
            else
            {
                _config = JsonConvert.DeserializeObject<RPPlusConfig>(ConfPath);
            }
        }

        private IEnumerator GetMoney(Player player, Vector3 position)
        {
            foreach (Player p in _server.Players.Where(p =>
                     {
                         if (p.isInGame)
                         {
                             Characters character = p.character;
                             if (character != null && Nova.biz.bizs[character.BizId].IsActivity(0))
                             {
                                 return p.serviceMetier;
                             }
                         }

                         return false;
                     }))
            {
                p.setup.TargetPlayClairon(0.5f);
                p.SendText(
                    "<color=#e8472a>Un braquage de DAB est en cours, un point a été placé sur votre carte.</color > ");
                p.setup.TargetSetGPSTarget(position);
                string name = player.GetFullName().Replace("A", "$").Replace("E", "%").Replace("I", "^");
                p.SendText("<color=#e8472a>La caméra a identifié un individu, les résultats peuvent être inexacts :" +
                           name + " </color > ");
            }

            _lastRob = Nova.UnixTimeNow() + _config.hackCooldown;
            int number = 0;
            for (int i = 0; i < 60; i++)
            {
                if (Vector3.Distance(player.setup.transform.position, position) < 4.0)
                {
                    number++;
                    if (i == 0) 
                        player.SendText("<color=#e8472a>Braquage en cours, veuillez patienter...< / color > ");    
                    
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    player.SendText(
                        "<color=#e8472a>Braquage annulé.</color > ");
                    break;
                }
            }

            int money = Random.Range(_config.minMoney, _config.maxMoney) * number;
            player.AddMoney(money, "ATM ROB");
            player.SendText($"<color=#e8472a>Vous avez volé {money}€</color>");
        }
    }

    [System.Serializable]
    public class RPPlusConfig
    {
        public long hackCooldown;

        public int minMoney;

        public int maxMoney;

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(RoleplayPlus.ConfPath, json);
        }
    }
}