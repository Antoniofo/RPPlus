using Life;
using Life.DB;
using Life.Network;
using Life.UI;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using System.Linq;
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
            _server.OnPlayerTryToHackATM += TryHack;

            SChatCommand reload = new SChatCommand("/reloadrpplus", "Reload the config of RPPlus", "/reloadrpplus",
                (player, arg) =>
                {
                    if (player.IsAdmin)
                    {
                        string json = File.ReadAllText(ConfPath);
                        _config = JsonConvert.DeserializeObject<RPPlusConfig>(json);
                        player.SendText("Reload Successful");
                    }
                    else
                    {
                        player.SendText("<color=red>Your not an admin</color>");
                    }
                });

            SChatCommand reset = new SChatCommand("/resetcooldown", "Reset the cooldown to hack DAB", "/resetcooldown",
                (player, arg) =>
                {
                    if (player.IsAdmin)
                    {
                        _lastRob = 0;
                        player.SendText("Cooldown reset");
                    }
                    else
                    {
                        player.SendText("<color=red>Your not an admin</color>");
                    }
                });
            reload.Register();
            reset.Register();
        }

        public void TryHack(Player player)
        {
            UIPanel uiPanel = new UIPanel("Hack Tool V0.3", UIPanel.PanelType.Text).AddButton(
                    "Commencer le Hack", (ui =>
                    {
                        if (_lastRob > Nova.UnixTimeNow())
                        {
                            player.SendText(
                                "<color=red>Veuillez attendre quelques heures avant de braquer ce DAB.</color > ");
                        }
                        else
                        {
                            _server.lifeManager.StartCoroutine(GetMoney(player, player.setup.transform.position));
                            player.SendText("<color=red>HackTool.exe et en cours de lancement</color > ");
                            player.ClosePanel(ui);
                        }
                    })).AddButton("Close", (player.ClosePanel))
                .SetText("HackTool.exe Error de piratage lancement de FIX-HackTool.exe");
            player.ShowPanelUI(uiPanel);
        }

        private void InitDirectory()
        {
            _dirPath = $"{pluginsPath}/RPPlus";
            ConfPath = $"{_dirPath}/config.json";


            if (!Directory.Exists(_dirPath))
                Directory.CreateDirectory(_dirPath);

            if (!File.Exists(ConfPath))
            {

                _config = new RPPlusConfig() { hackCooldown = 28800L, minMoney = 500, maxMoney = 1000 };
                var json = JsonConvert.SerializeObject(_config);
                File.WriteAllText(ConfPath, json);
            }
            else
            {
                string json2 = File.ReadAllText(ConfPath);
                try
                {
                    _config = new RPPlusConfig();
                    _config = JsonConvert.DeserializeObject<RPPlusConfig>(json2);
                    string newJson = JsonConvert.SerializeObject(_config);
                    File.WriteAllText(ConfPath, newJson);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.Message);
                    Debug.LogException(e);
                }
            }
        }

        private IEnumerator GetMoney(Player player, Vector3 position)
        {
            foreach (Player p in _server.Players.Where(p =>
                     {
                         if (p.isInGame)
                         {
                             Characters character = p.character;
                             Bizs job;
                             try
                             {
                                 job = Nova.biz.bizs.Where(u => u.Id == character.BizId).First();
                             }
                             catch
                             {
                                 return false;
                             }
                             
                             if (job != null)
                             {                                 
                                 if (character != null && job.IsActivity(Life.BizSystem.Activity.Type.LawEnforcement))
                                 {
                                     return p.serviceMetier;
                                 }
                             }                             
                         }

                         return false;
                     }))
            {
                p.setup.TargetPlayClairon(0.5f);
                p.SendText(
                    "<color=#e8472a>Un braquage de DAB est en cours, un point a été placé sur votre carte.</color>");
                p.setup.TargetSetGPSTarget(position);
                string name = player.GetFullName().Replace("A", "$").Replace("E", "%").Replace("I", "^");
                p.SendText("<color=#e8472a>La caméra a identifié un individu, les résultats peuvent être inexacts :" +
                           name + " </color> ");
            }

            _lastRob = Nova.UnixTimeNow() + _config.hackCooldown;
            int number = 0;
            int i = 0;
            for (i = 0; i < 60; i++)
            {
                if (Vector3.Distance(player.setup.transform.position, position) < 4.0)
                {
                    number++;
                    if (i == 0)
                        player.SendText("<color=#e8472a>Braquage en cours, veuillez patienter...</color>");

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
            if (i == 60)
                player.SendText("Braquage terminé");
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