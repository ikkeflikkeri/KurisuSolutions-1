﻿#region Copyright © 2015 Kurisu Solutions
// All rights are reserved. Transmission or reproduction in part or whole,
// any form or by any means, mechanical, electronical or otherwise, is prohibited
// without the prior written consent of the copyright owner.
// 
// Document:	Spells/CoreSpell.cs
// Date:		22/09/2015
// Author:		Robin Kurisu
#endregion

using System;
using System.Linq;
using Activator.Base;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Activator.Spells
{
    public class CoreSpell
    {
        internal virtual string Name { get; set; }
        internal virtual string DisplayName { get; set; }
        internal virtual float Range { get; set; }
        internal virtual MenuType[] Category { get; set; }
        internal virtual int DefaultMP { get; set; }
        internal virtual int DefaultHP { get; set; }

        public Menu Menu { get; private set; }
        public Menu Parent { get { return Menu.Parent; } }
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public Obj_AI_Hero LowTarget
        {
            get
            {
                return ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget(Range))
                    .OrderBy(ene => ene.Health/ene.MaxHealth*100).First();
            }
        }

        public CoreSpell CreateMenu(Menu root)
        {
            try
            {
                if (Player.GetSpellSlot(Name) == SpellSlot.Unknown)
                    return null;

                Menu = new Menu(DisplayName, "m" + Name);
                Menu.AddItem(new MenuItem("use" + Name, "Use " + DisplayName)).SetValue(false).Permashow();

                if (Category.Any(t => t == MenuType.Stealth))
                    Menu.AddItem(new MenuItem("Stealth" + Name + "pct", "Use on Stealth")).SetValue(true);

                if (Category.Any(t => t == MenuType.SlowRemoval))
                    Menu.AddItem(new MenuItem("use" + Name + "sr", "Use on Slows")).SetValue(true);

                if (Category.Any(t => t == MenuType.EnemyLowHP)) 
                    Menu.AddItem(new MenuItem("enemylowhp" + Name + "pct", "Use on Enemy HP % <="))
                        .SetValue(new Slider(DefaultHP))
                        .SetTooltip("Will Use " + Name + " on Enemy if Their HP % < Value");

                if (Category.Any(t => t == MenuType.SelfLowHP))
                    Menu.AddItem(new MenuItem("selflowhp" + Name + "pct", "Use on Hero HP % <="))
                        .SetValue(new Slider(DefaultHP))
                        .SetTooltip("Will Use " + Name + " When the Income Damage + Hero's HP % < Value");

                if (Category.Any(t => t == MenuType.SelfMuchHP))
                    Menu.AddItem(new MenuItem("selfmuchhp" + Name + "pct", "Use on Hero Dmg Dealt % >="))
                        .SetValue(new Slider(25))
                        .SetTooltip("Will Use " + Name + " When the Hero's Income Damage % > Value");

                if (Category.Any(t => t == MenuType.SelfLowMP))
                    Menu.AddItem(new MenuItem("selflowmp" + Name + "pct", "Use on Hero Mana % <="))
                        .SetValue(new Slider(DefaultMP));

                if (Category.Any(t => t == MenuType.SelfLowHP))
                    Menu.AddItem(new MenuItem("selflowhp" + Name + "th", "Minimum Dmg Dealt %"))
                        .SetValue(new Slider(5))
                        .SetTooltip("The Minimum Percentage of Damage to Trigger");

                if (Category.Any(t => t == MenuType.SelfCount))
                    Menu.AddItem(new MenuItem("selfcount" + Name, "Use on # Near Hero >="))
                        .SetValue(new Slider(3, 1, 5));

                if (Category.Any(t => t == MenuType.SelfMinMP))
                    Menu.AddItem(new MenuItem("selfminmp" + Name + "pct", "Minimum Mana/Energy %")).SetValue(new Slider(40));

                if (Category.Any(t => t == MenuType.SelfMinHP))
                    Menu.AddItem(new MenuItem("selfminhp" + Name + "pct", "Minimum HP %")).SetValue(new Slider(40));

                if (Category.Any(t => t == MenuType.SpellShield))
                {
                    Menu.AddItem(new MenuItem("ss" + Name + "all", "Use on Any Spell")).SetValue(false);
                    Menu.AddItem(new MenuItem("ss" + Name + "cc", "Use on Crowd Control")).SetValue(true);
                }

                if (Category.Any(t => t == MenuType.Zhonyas))
                {
                    Menu.AddItem(new MenuItem("use" + Name + "norm", "Use on Dangerous (Spells)"))
                        .SetTooltip("Not recommended to enable on spells with long cooldowns.").SetValue(false);
                    Menu.AddItem(new MenuItem("use" + Name + "ulti", "Use on Dangerous (Ultimates Only)")).SetValue(true);
                }

                if (Category.Any(t => t == MenuType.ActiveCheck))
                    Menu.AddItem(new MenuItem("mode" + Name, "Mode: "))
                        .SetValue(new StringList(new[] { "Always", "Combo" }));

                root.AddSubMenu(Menu);
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Game.PrintChat("<font color=\"#FFF280\">Exception thrown at CoreSpell.CreateMenu: </font>: " + e.Message);
            }

            return this;
        }

        public void CastOnBestTarget(Obj_AI_Hero primary, bool nonhero = false)
        {
            if (TargetSelector.GetPriority(primary) >= 2)
                UseSpellOn(primary);

            else if (LowTarget != null)
                UseSpellOn(LowTarget);
        }

        public bool IsReady()
        {
            return Player.GetSpellSlot(Name).IsReady();
        }

        public void UseSpell(bool combo = false)
        {
            if (!combo || Activator.Origin.Item("usecombo").GetValue<KeyBind>().Active)
            {
                if (Utils.GameTimeTickCount - Activator.LastUsedTimeStamp > Activator.LastUsedDuration)
                {
                    if (Player.GetSpellSlot(Name).IsReady())
                    {
                        if (!Activator.Player.IsRecalling() &&
                            !Activator.Player.Spellbook.IsChanneling &&
                            !Activator.Player.Spellbook.IsCastingSpell)
                        {
                            Player.Spellbook.CastSpell(Player.GetSpellSlot(Name));
                            Activator.LastUsedTimeStamp = Utils.GameTimeTickCount;
                            Activator.LastUsedDuration = 1000;
                        }
                    }
                }
            }
        }

        public void UseSpellTowards(Vector3 targetpos, bool combo = false)
        {
            if (!combo || Activator.Origin.Item("usecombo").GetValue<KeyBind>().Active)
            {
                if (Utils.GameTimeTickCount - Activator.LastUsedTimeStamp > Activator.LastUsedDuration)
                {
                    if (Player.GetSpellSlot(Name).IsReady())
                    {
                        if (!Activator.Player.IsRecalling() &&
                            !Activator.Player.Spellbook.IsChanneling &&
                            !Activator.Player.Spellbook.IsCastingSpell)
                        {
                            Player.Spellbook.CastSpell(Player.GetSpellSlot(Name), targetpos);
                            Activator.LastUsedTimeStamp = Utils.GameTimeTickCount;
                            Activator.LastUsedDuration = 1000;
                        }
                    }
                }
            }
        }

        public void UseSpellOn(Obj_AI_Base target, bool combo = false)
        {
            if (!combo || Activator.Origin.Item("usecombo").GetValue<KeyBind>().Active)
            {
                if (Utils.GameTimeTickCount - Activator.LastUsedTimeStamp > Activator.LastUsedDuration)
                {
                    if (!Activator.Player.IsRecalling() &&
                        !Activator.Player.Spellbook.IsChanneling &&
                        !Activator.Player.Spellbook.IsCastingSpell)
                    {
                        Player.Spellbook.CastSpell(Player.GetSpellSlot(Name), target);
                        Activator.LastUsedTimeStamp = Utils.GameTimeTickCount;
                    }
                }
            }
        }

        public virtual void OnTick(EventArgs args)
        {
     
        }
    }
}
