using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BloodCraftUI.Config;
using BloodCraftUI.UI.ModContent;
using Unity.Entities;
using BloodCraftUI.Utils;
using ProjectM;

namespace BloodCraftUI.Services
{
    internal static partial class MessageService
    {
        private static AbilitySchoolType? _oldVersionColor;
        private static string _oldVersionName;
        public const string BCCOM_LISTBOXES1 = ".fam lc";
        public const string BCCOM_LISTBOXES2 = ".familiar listboxes";
        public const string BCCOM_SWITCHBOX = ".fam ec {0}";
        public const string BCCOM_BOXCONTENT = ".fam l";
        public const string BCCOM_BINDFAM = ".fam v {0}";
        public const string BCCOM_UNBINDFAM = ".fam dv";
        public const string BCCOM_FAMSTATS = ".fam d";
        public const string BCCOM_COMBAT = ".fam alc";
        public const string BCCOM_ENABLEEQUIP = ".fam e";
        public const string BCCOM_TOGGLEFAM = ".fam al";
        public const string BCCOM_DELETEFAM = ".fam rf {0}";
        public const string BCCOM_PRESTIGEFAM = ".fam pr";
        
        // Novos comandos adicionados
        public const string BCCOM_UNITTYPE = ".fam u";
        public const string BCCOM_RENAMEBOX = ".fam rc {0} {1}";
        public const string BCCOM_MOVEFAMILIAR = ".fam mf {0}";
        public const string BCCOM_DELETEBOX = ".fam dc {0}";
        public const string BCCOM_ADDBOX = ".fam ac {0}";
        public const string BCCOM_LISTEMOTES = ".fam le";
        public const string BCCOM_RESETFAMILIARS = ".fam r";
        public const string BCCOM_SEARCHFAMILIAR = ".fam pc {0}";
        public const string BCCOM_SMARTBIND = ".fam va {0}";
        public const string BCCOM_CHOOSESHINY = ".fam es {0}";
        public const string BCCOM_TOGGLEOPTIONS = ".fam ao {0}";
        public const string BCCOM_CHANGEABILITY = ".fam th";
        public const string BCCOM_TRADEFAMILIAR = ".fam tf {0}";
        
        // Comandos corretos que os emotes acionam
        public const string EMOTE_CALLDISMISS = ".fam al";          // Acenar: CallDismiss → alternar
        public const string EMOTE_COMBATMODE = ".fam alc";          // Saudar: CombatMode → alternarcombate
        public const string EMOTE_BINDUNBIND_SMART = ".fam v -1";      // Aplaudir: BindUnbind → Reset (mais seguro para bind/unbind)
        public const string EMOTE_INTERACTMODE = ".fam u";          // Chamar: InteractMode → unidade (melhor correspondência)
        public const string EMOTE_CASTFAMILIARSKILL = ".fam th";    // Apontar: CastFamiliarSkill → trocarhabilidade 
        public const string EMOTE_CLASSABILITY = ".fam u";          // Encolher: ClassAbility → unidade

        private enum InterceptFlag
        {
            ListBoxes,
            ListBoxContent,
            FamStats
        }

        private static readonly Dictionary<InterceptFlag, int> Flags = new();
        const string COLOR_PATTERN = "<color=.*?>(.*?)</color>";
        const string EXTRACT_BOX_NAME_PATTERN = "<color=[^>]+>(?<box>.*?)</color>";
        const string EXTRACT_COLOR_PATTERN = "(?<=<color=)[^>]+";
        const string EXTRACT_FAM_LVL_PATTERN = @"Seu familiar.*?\é nível <color=[^>]+>(\d+)</color>.*?com <color=[^>]+>(\d+)</color> prestígios.*?tem <color=[^>]+>(\d+)</color>.*?\(<color=[^>]+>(\d+)%</color>\)";
        const string EXTRACT_FAM_STATS_PATTERN = @"<color=[^>]+>([^<]+)</color>\s*\(<color=[^>]+>([^<)]+)\)";
        const string EXTRACT_FAM_NAME_PATTERN = @"<color=[^>]+>(?<name>[^<]+)</color>";
        const string EXTRACT_FAM_SCHOOL_PATTERN = @"\((?<school>[^)]+)\)\s*\[";

        private static string _currentBox;
        private static FamStats _currentFamStats;
        public static bool BoxContentFlag { get; set; }
        
        // Flags para controlar destruição de mensagens apenas quando acionado via interface
        public static bool DestroyBoxListMessages { get; set; }
        public static bool DestroyFamStatsMessages { get; set; }

        internal static void HandleMessage(Entity entity)
        {
            var chatMessage = entity.Read<ChatMessageServerEvent>();
            var message = chatMessage.MessageText.Value;

            if (chatMessage.MessageType == ServerChatMessageType.Local)
            {
                if (message.StartsWith(".fam"))
                {
                    switch (message)
                    {
                        case not null when message.StartsWith(".fam v"):
                        case not null when message.StartsWith(".familiar bind"):
                            Settings.LastBindCommand = message;
                            break;
                        case not null when message.StartsWith(BCCOM_FAMSTATS):
                            ClearFlags();
                            Flags[InterceptFlag.FamStats] = 1;
                            break;
                        case not null when message.StartsWith(BCCOM_LISTBOXES1):
                            ClearFlags();
                            Flags[InterceptFlag.ListBoxes] = 1;
                            var panel = Plugin.UIManager.GetPanel<BoxListPanel>();
                            if (panel != null)
                            {
                                panel.Reset();
                                LogUtils.LogInfo("BoxListPanel encontrado e resetado");
                            }
                            else
                            {
                                LogUtils.LogInfo("BoxListPanel é null!");
                            }
                            break;
                        case not null when message.StartsWith(BCCOM_BOXCONTENT):
                            ClearFlags();
                            Flags[InterceptFlag.ListBoxContent] = 1;
                            if (_currentBox != null)
                            {
                                var boxPanel = Plugin.UIManager.GetBoxPanel(_currentBox);
                                if (boxPanel != null)
                                {
                                    boxPanel.Reset();
                                    ProcessBoxContentEntry(message);
                                }
                            }
                            break;
                    }


                    if (Settings.ClearServerMessages)
                        DestroyMessage(entity);
                    return;
                }
            }

            if (!chatMessage.MessageType.Equals(ServerChatMessageType.System))
                return;

            switch (message)
            {
                /////// FLAGS
                case not null when message.Contains(">unbound</color>!"):
                    break;
                case not null when message.Contains(">bound</color>!"):
                {
                    //old version failsafe
                    var regex =
                        new Regex(@"<color=\w+>(?<name>[^<]+)</color>(?:\s*<color=(?<color>#[A-Fa-f0-9]{6})>\*</color>)?");

                    Match match = regex.Match(message);
                    if (match.Success)
                    {
                        _oldVersionName = match.Groups["name"].Value;
                        _oldVersionColor = match.Groups["color"].Success ? GameHelper.GetSchoolFromHexColor(match.Groups["color"].Value) : null;
                    }
                }
                    break;

                /////// CLEANUP
                case not null when message.StartsWith("Não foi possível encontrar um familiar ativo"):
                case not null when message.StartsWith("Não foi possível localizar um familiar"):
                case not null when message.Contains("nenhum familiar ativo"):
                    if (Settings.ClearServerMessages)
                        DestroyMessage(entity);
                    break;
                case not null when message.StartsWith("Seu familiar"):
                    ClearFlags();
                    Flags[InterceptFlag.FamStats] = Settings.IsFamStatsPanelEnabled ? 1 : 0;
                    ProcessFamStatsData(message, 0);
                    // Só destroi mensagem se foi acionado via interface
                    if (DestroyFamStatsMessages)
                        DestroyMessage(entity);
                    break;
                case not null when message.StartsWith("[") && message.Contains("CAIXAS"):
                case not null when message.Contains("[ CAIXAS ]"):
                case not null when message.Contains("Lista de caixas"):
                    LogUtils.LogInfo($"Resposta de caixas detectada: {message}");
                    ClearFlags();
                    Flags[InterceptFlag.ListBoxes] = Settings.IsBoxPanelEnabled ? 1 : 0;
                    Plugin.UIManager.GetPanel<BoxListPanel>()?.Reset();

                    // Só destroi mensagem se foi acionado via interface
                    if (DestroyBoxListMessages)
                    {
                        LogUtils.LogInfo("Destruindo mensagem de caixas via interface");
                        DestroyMessage(entity);
                    }
                    break;
                case not null when message.StartsWith("<color=yellow>1</color>|"):
                    if (!BoxContentFlag) break;
                    BoxContentFlag = false;
                    ClearFlags();
                    Flags[InterceptFlag.ListBoxContent] = Settings.IsBoxPanelEnabled ? 1 : 0;
                    if (_currentBox != null)
                    {
                        var panel = Plugin.UIManager.GetBoxPanel(_currentBox);
                        if (panel != null)
                        {
                            panel.Reset();
                            ProcessBoxContentEntry(message);
                        }
                    }
                    if (Settings.ClearServerMessages)
                        DestroyMessage(entity);
                    break;
                case not null when message.StartsWith("A caixa"):
                    // Para mensagens como "A caixa <color=white>NomeDaCaixa</color> foi escolhida!"
                    var matches = Regex.Matches(message, COLOR_PATTERN);
                    if (matches.Count > 0)
                        _currentBox = matches.FirstOrDefault()?.Groups[1].Value;
                    break;
                case not null when message.StartsWith("Emote actions <color=red>disabled</color>"):
                    if(_famEquipSequenceActive)
                        EnqueueMessage(BCCOM_ENABLEEQUIP);
                    break;
                case not null when message.StartsWith("Emote actions <color=green>enabled</color>"):
                    if (_famEquipSequenceActive)
                        FinishAutoEnableFamiliarEquipmentSequence();
                    break;

                default:
                    {
                        //fam stats
                        if (Flags.HasKeyValue(InterceptFlag.FamStats, 1))
                        {
                            if (message.StartsWith("Seu familiar"))
                            {
                                ProcessFamStatsData(message, 0);
                            }
                            else if (message.StartsWith("<color=#00FFFF>MaxHealth"))
                            {
                                ProcessFamStatsData(message, 1);
                            }
                            else if (message.StartsWith("<color=green>"))
                            {
                                ProcessFamStatsData(message, 2);
                            }
                            DestroyMessage(entity);
                        }

                        //list box content
                        if (Flags.HasKeyValue(InterceptFlag.ListBoxContent, 1))
                        {
                            //stop
                            if (message.Length >= 2 && !message.Contains("</color>") && !message.StartsWith("["))
                            {
                                Flags.SetValue(InterceptFlag.ListBoxContent, 0);
                                return;
                            }

                            ProcessBoxContentEntry(message);
                            DestroyMessage(entity);
                        }

                        //list boxes
                        if (Flags.HasKeyValue(InterceptFlag.ListBoxes, 1))
                        {
                            LogUtils.LogInfo($"Processando linha de caixas: {message}");
                            
                            // Para quando encontrar uma linha vazia ou que não seja relevante para caixas
                            if (message.Length < 2 || message.StartsWith(".fam") || message.StartsWith("Uso:") || message.StartsWith("Comando"))
                            {
                                LogUtils.LogInfo("Parando processamento de caixas");
                                Flags.SetValue(InterceptFlag.ListBoxes, 0);
                                return;
                            }
                            
                            // Processa mensagens como "CaixaA e CaixaB" ou individual com cores
                            if (message.Contains("<color"))
                            {
                                // Processa com regex para nomes coloridos
                                Regex regex = new Regex(EXTRACT_BOX_NAME_PATTERN);
                                MatchCollection boxMatches = regex.Matches(message);

                                foreach (Match match in boxMatches)
                                {
                                    var text = match.Groups["box"].Value;
                                    if (!string.IsNullOrEmpty(text))
                                    {
                                        Plugin.UIManager.GetPanel<BoxListPanel>()?.AddListEntry(text);
                                    }
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(message) && !message.StartsWith("["))
                            {
                                // Processa texto simples separado por vírgulas e "e"
                                var boxNames = message.Split(new string[] { ", ", " e ", " " }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var boxName in boxNames)
                                {
                                    var cleanName = boxName.Trim();
                                    if (!string.IsNullOrEmpty(cleanName) && cleanName.Length > 1 && !cleanName.Contains("["))
                                    {
                                        Plugin.UIManager.GetPanel<BoxListPanel>()?.AddListEntry(cleanName);
                                    }
                                }
                            }

                            // Só destroi mensagem se foi acionado via interface
                            if (DestroyBoxListMessages)
                                DestroyMessage(entity);
                        }
                    }
                    break;
            }
        }

        private static void UpdateFamStatsUI()
        {
            Plugin.UIManager.GetPanel<FamStatsPanel>()?.UpdateData(_currentFamStats);
        }

        private static void ProcessFamStatsData(string message, int type)
        {
            switch (type)
            {
                case 0: //level data - mensagem inicial com nível e experiência
                {
                    _currentFamStats = new FamStats();
                    //old version failsafe
                    _currentFamStats.Name = _oldVersionName;
                    _currentFamStats.School = _oldVersionColor?.ToString();

                    var match = Regex.Match(message, EXTRACT_FAM_LVL_PATTERN);
                    if (match.Success)
                    {
                        _currentFamStats.Level = int.Parse(match.Groups[1].Value);
                        _currentFamStats.PrestigeLevel = int.Parse(match.Groups[2].Value);
                        _currentFamStats.ExperienceValue = int.Parse(match.Groups[3].Value);
                        _currentFamStats.ExperiencePercent = int.Parse(match.Groups[4].Value);
                    }
                    else
                    {
                        // Fallback: tentar extrair apenas nível básico
                        var levelMatch = Regex.Match(message, @"nível <color=[^>]+>(\d+)</color>");
                        if (levelMatch.Success)
                        {
                            _currentFamStats.Level = int.Parse(levelMatch.Groups[1].Value);
                            _currentFamStats.PrestigeLevel = 0;
                            _currentFamStats.ExperienceValue = 0;
                            _currentFamStats.ExperiencePercent = 0;
                        }
                    }
                }
                    break;
                case 1: // stats - para mensagens com estatísticas
                    {
                        var matches = Regex.Matches(message, EXTRACT_FAM_STATS_PATTERN);

                        foreach (Match match in matches)
                        {
                            if (match.Success)
                            {
                                string propName = match.Groups[1].Value.Trim();
                                string value = match.Groups[2].Value;
                                
                                // Mapear nomes em português para inglês
                                string mappedName = propName switch
                                {
                                    "Vida Máxima" => "MaxHealth",
                                    "Poder Físico" => "PhysicalPower", 
                                    "Poder Mágico" => "SpellPower",
                                    "Resistência Física" => "PhysicalResistance",
                                    "Resistência Mágica" => "SpellResistance",
                                    _ => propName
                                };
                                
                                switch (mappedName)
                                {
                                    case "MaxHealth":
                                        _currentFamStats.MaxHealth = value;
                                        break;
                                    case "PhysicalPower":
                                        _currentFamStats.PhysicalPower = value;
                                        break;
                                    case "SpellPower":
                                        _currentFamStats.SpellPower = value;
                                        break;
                                    default:
                                        _currentFamStats.Stats.Add(propName, value);
                                        break;
                                }
                            }
                        }

                        ClearFlags();
                        UpdateFamStatsUI();
                    }
                    break;
                case 2: //name - para mensagens do tipo "Seu familiar <color=...>Nome</color> (Shiny) [99][5]"
                {
                    var nameMatch = Regex.Match(message, @"Seu familiar\s+<color=[^>]+>(?<name>[^<]+)</color>");
                    if (nameMatch.Success)
                    {
                        _currentFamStats.Name = nameMatch.Groups["name"].Value;
                    }
                    
                    // Extrair shiny info se existir
                    var shinyMatch = Regex.Match(message, @"\((?<shiny>[^)]+)\)");
                    if (shinyMatch.Success)
                    {
                        _currentFamStats.School = shinyMatch.Groups["shiny"].Value;
                    }
                    else 
                    {
                        _currentFamStats.School = null;
                    }
                }
                    break;

            }
        }

        private static void ProcessBoxContentEntry(string message)
        {
            try
            {
                // Para mensagens como "[1] <color=#56b5e1>NomeFamiliar</color> (Shiny) [99][5]"
                var numberMatch = Regex.Match(message, @"^\[(\d+)\]");
                if (!numberMatch.Success) return;
                
                var number = int.Parse(numberMatch.Groups[1].Value);
                
                // Extrair nome do familiar
                var nameMatch = Regex.Match(message, @"<color=#[^>]+>([^<]+)</color>");
                if (!nameMatch.Success) return;
                
                string familiarName = nameMatch.Groups[1].Value;
                
                // Verificar se é shiny
                AbilitySchoolType? spellSchool = null;
                if (message.Contains("Shiny"))
                {
                    var colorMatch = Regex.Match(message, @"<color=(#[^>]+)>Shiny</color>");
                    if (colorMatch.Success)
                    {
                        spellSchool = GameHelper.GetSchoolFromHexColor(colorMatch.Groups[1].Value);
                    }
                }
                
                // Extrair níveis se presentes
                var levelInfo = "";
                var levelMatches = Regex.Matches(message, @"\[<color=[^>]+>(\d+)</color>\]");
                if (levelMatches.Count > 0)
                {
                    levelInfo = $" [{levelMatches[0].Groups[1].Value}";
                    if (levelMatches.Count > 1)
                    {
                        levelInfo += $"][{levelMatches[1].Groups[1].Value}";
                    }
                    levelInfo += "]";
                }
                
                string displayText = familiarName + levelInfo;
                Plugin.UIManager.GetBoxPanel(_currentBox)?.AddListEntry(number, displayText, spellSchool);
            }
            catch (Exception ex)
            {
                LogUtils.LogError($"{nameof(ProcessBoxContentEntry)} parsing error: {ex.Message}");
            }
        }

        private static void ClearFlags()
        {
            Flags.Clear();
        }

        private static void DestroyMessage(Entity entity)
        {
            if (Settings.ClearServerMessages)
                Plugin.EntityManager.DestroyEntity(entity);
        }
    }

    public class FamStats
    {
        public int Level { get; set; }
        public int PrestigeLevel { get; set; }
        public int ExperienceValue { get; set; }
        public int ExperiencePercent { get; set; }
        public string MaxHealth { get; set; }
        public string PhysicalPower { get; set; }
        public string SpellPower { get; set; }
        public string Name { get; set; }
        public string School { get; set; }
        public Dictionary<string, string> Stats { get; set; } = new();

        public string CurrentHealth { get; set; }
    }
}
