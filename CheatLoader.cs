using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


namespace CheatMenu
{
    public class CheatLoader : MelonMod
    {
        /*
         * Cheat Menu 1.0.0, by Malvorium
         * 
         * Okay so I don't have much experience when it comes to coding; what's best coding practices and whatnot
         * pls don't hurt me :(
         * 
         */
        private static readonly int WINDOW_WIDTH = 100, WINDOW_HEIGHT = 100, MIN_STATE_WIDTH = 20, MIN_STATE_HEIGHT = 20;
        

        // show at top-left corner of the screen
        
        private Rect cheatWindow = new Rect(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT);

        private Vector2 scrollPos;
        private int contributionPoints = 0;
        private Dictionary<HeroMenuPrefabTask, int> skillMap, limitSkillMap;
        private static GUIStyle textCenter;
        private static GUIContent normalContent;

        private bool isButtonPressed, isWindowPresenceReduced;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("I Am Not A Leader | Cheat Menu, Initialized!");
            normalContent = new GUIContent("Cheat Menu", "Cheat Window 1.0.0 by Malvorium");
            skillMap = new Dictionary<HeroMenuPrefabTask, int>();
            limitSkillMap = new Dictionary<HeroMenuPrefabTask, int>();
            foreach (HeroMenuPrefabTask skill in Enum.GetValues(typeof(HeroMenuPrefabTask)))
            {
                skillMap.Add(skill, ManagementInfo.MiHeroInfo.SkillScore(skill));
                limitSkillMap.Add(skill, ManagementInfo.MiHeroInfo.LimitSkill(skill));
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Debug: {sceneName} successfully loaded.");
            if(!SceneManager.GetActiveScene().name.Equals("Title"))
            {
                contributionPoints = ManagementInfo.MiCoreInfo.Contribution;  
            }
        }



        public override void OnGUI()
        {
            textCenter = new GUIStyle(GUI.skin.GetStyle("label")) { alignment = TextAnchor.MiddleCenter };

            if(SceneManager.GetActiveScene().name.Equals("Title"))
            {
                cheatWindow = GUILayout.Window(0, cheatWindow, DrawWindowContents, normalContent, GUILayout.Width(WINDOW_WIDTH), GUILayout.Height(WINDOW_HEIGHT));
                return;
            }
            else if (isWindowPresenceReduced)
            {
                
                cheatWindow = GUILayout.Window(1, cheatWindow,
                        DrawWindowContents, new GUIContent("Cheat", "Currently minimized!"), GUILayout.Width(MIN_STATE_WIDTH), GUILayout.Height(MIN_STATE_HEIGHT));
            }
            else
            {
                cheatWindow = GUILayout.Window(0, cheatWindow, DrawWindowContents, normalContent);
            }

            if(isButtonPressed)
            {
                TopDisplay.Instance.RefreshDisplay();
                isButtonPressed = false;
            }
        }


        private void DrawWindowContents(int id)
        {
            if (SceneManager.GetActiveScene().name.Equals("Title"))
            {
                GUILayout.Label("Hi, please load an active game ^_^");
            }
            else if (id == 1)
            {
                if (GUILayout.Button("Show menu"))
                {
                    isWindowPresenceReduced = false;
                }
            }
            else
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(400), GUILayout.Width(300));
                if (!isWindowPresenceReduced)
                {
                    if (GUILayout.Button("Minimize menu presence"))
                    {
                        isWindowPresenceReduced = true;
                        
                    }
                    GUILayout.Label("Modify Gold", textCenter);
                    foreach (GoldAmountPreset gold in Enum.GetValues(typeof(GoldAmountPreset)))
                    {
                        if (GUILayout.Button($"Add {(int)gold} Gold"))
                        {
                            Cheat.PerformGoldAddition((int)gold);
                            isButtonPressed = true;
                        }
                    }
                    GUILayout.Label("Modify Contribution", textCenter);
                    // according to the game's code, the cap limit for contribution is 9999. Any more than that means the game will clamp it to cap limit
                    contributionPoints = (int)GUILayout.HorizontalSlider(contributionPoints, 0, 9999);
                    GUILayout.Label(contributionPoints.ToString(), textCenter);
                    if (GUILayout.Button("Set contribution value"))
                    {
                        Cheat.PerformContributionSet(contributionPoints);

                        isButtonPressed = true;
                    }
                    GUILayout.Label("Modify skills", textCenter);
                    foreach (HeroMenuPrefabTask skill in Enum.GetValues(typeof(HeroMenuPrefabTask)))
                    {
                        GUILayout.Label(skill.ToString());
                        skillMap[skill] = Convert.ToInt16(GUILayout.TextField(skillMap[skill].ToString(), ManagementInfo.MiHeroInfo.LimitSkill(skill).ToString().Length));
                        skillMap[skill] = Convert.ToInt16(Regex.Replace(skillMap[skill].ToString(), @"[^0-9]", ""));
                        // if the value somehow is beyond the current skill limit, clamp the thing to current skill limit
                        float floatvalue = Mathf.Clamp(skillMap[skill], 0, ManagementInfo.MiHeroInfo.LimitSkill(skill));
                        skillMap[skill] = (int)floatvalue;
                        if (GUILayout.Button($"Set input amount for {skill}"))
                        {
                            Cheat.PerformSkillSet(skill, skillMap[skill], false);
                            RefreshHeroMenu();
                        }
                    }
                    GUILayout.Label("Modify skill limit", textCenter);
                    foreach (HeroMenuPrefabTask skillLimit in Enum.GetValues(typeof(HeroMenuPrefabTask)))
                    {
                        GUILayout.Label(skillLimit.ToString());
                        //
                        limitSkillMap[skillLimit] = Convert.ToInt16(GUILayout.HorizontalSlider(limitSkillMap[skillLimit], 0, 999));
                        GUILayout.Label(limitSkillMap[skillLimit].ToString(), textCenter);
                        if (GUILayout.Button($"Set limit skill for {skillLimit}"))
                        {
                            Cheat.PerformSkillSet(skillLimit, limitSkillMap[skillLimit], true);
                            RefreshHeroMenu();
                        }
                    }
                    GUILayout.EndScrollView();

                }
                
            }
            GUI.DragWindow();
        }

        // refreshes hero menu so we can see changes in real time since the game does not refresh automatically if a value has changed
        // unless manually reopened
        public static void RefreshHeroMenu()
        {
            HeroMenuPrefab currentSelectedMenu = (HeroMenuPrefab)TownMenuManager.Instance.HeroMane.GetType()
                                .GetField("m_hmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                .GetValue(TownMenuManager.Instance.HeroMane);
            
            TownMenuManager.Instance.HeroMane.PressedMenu(currentSelectedMenu);
        }
    }
}
