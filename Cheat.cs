using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace CheatMenuSpace
{
    public class Cheat
    {
      
        public static void PerformGoldAddition(int amount)
        {
            ManagementInfo.MiCoreInfo.Gold += amount;
            Melon<CheatLoader>.Logger.Msg($"Added {amount} to player.");
        }

        public static void PerformContributionSet(int amount)
        {
            ManagementInfo.MiCoreInfo.Contribution = amount;
            Melon<CheatLoader>.Logger.Msg($"Changed contribution to {amount} ");
        }

        public static void PerformSkillSet(HeroMenuPrefabTask skillType, int amount, bool forSkillLimit)
        {
            string skillVarName = "m_skill";
            if(forSkillLimit)
            {
                skillVarName = "m_limitSkill";
            }
            Type heroInfoClass = typeof(ManagementInfo.MiHeroInfo);
            FieldInfo skillArrayField = heroInfoClass.GetField(skillVarName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
            int[] skills = (int[]) skillArrayField.GetValue(null);
            int originalVal = skills[(int)skillType];
            skills[(int) skillType] = amount;
            skillArrayField.SetValue(null, skills);
            Melon<CheatLoader>.Logger.Msg($"Changed skill {(forSkillLimit ? "limit" : "")}{skillType.ToString()} value. {originalVal} -> {amount} ");
        }
    }
}
