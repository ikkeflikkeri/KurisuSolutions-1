﻿using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using CM = KurisuNidalee.CastManager;
using Color = System.Drawing.Color;
using KL = KurisuNidalee.KurisuLib;

namespace KurisuNidalee
{
    internal class KurisuNidalee
    {
        internal static Menu Root;
        internal static Obj_AI_Hero Target;
        internal static Orbwalking.Orbwalker Orbwalker;
        internal static Obj_AI_Hero Player = ObjectManager.Player;

        internal KurisuNidalee()
        {                                                             
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Nidalee")
                return;

            #region Root Menu
            Root = new Menu("Kurisu's Nidalee", "nidalee", true);

            var orbm = new Menu(":: Orbwalker", "orbm");
            Orbwalker = new Orbwalking.Orbwalker(orbm);
            Root.AddSubMenu(orbm);

            var ccmenu = new Menu(":: Nidalee Settings", "ccmenu");

            var humenu = new Menu(":: Human Settings", "humenu");

            var ndhq = new Menu("(Q)  Javelin", "ndhq");
            ndhq.AddItem(new MenuItem("ndhqcheck", "Check Hitchance")).SetValue(true);
            ndhq.AddItem(new MenuItem("ndhqch", "-> Min Hitchance"))
                .SetValue(new StringList(new[] {"Low", "Medium", "High", "Very High"}, 2));
            ndhq.AddItem(new MenuItem("qsmcol", "-> Smite Collision")).SetValue(true);
            ndhq.AddItem(new MenuItem("ndhqco", "Enable in Combo")).SetValue(true);
            ndhq.AddItem(new MenuItem("ndhqha", "Enable in Harass")).SetValue(true);
            ndhq.AddItem(new MenuItem("ndhqjg", "Enable in Jungle")).SetValue(true);
            ndhq.AddItem(new MenuItem("ndhqwc", "Enable in WaveClear")).SetValue(false);
            humenu.AddSubMenu(ndhq);

            var ndhw = new Menu("(W) Bushwhack", "ndhw");
            ndhw.AddItem(new MenuItem("ndhwco", "Enable in Combo")).SetValue(true);
            ndhw.AddItem(new MenuItem("ndhwjg", "Enable in Jungle")).SetValue(true);
            ndhw.AddItem(new MenuItem("ndhwwc", "Enable in WaveClear")).SetValue(false);
            ndhw.AddItem(new MenuItem("ndhwforce", "Location"))
                .SetValue(new StringList(new[] {"Prediction", "Behind Target"}));
            humenu.AddSubMenu(ndhw);

            var ndhe = new Menu("(E)  Primal Surge", "ndhe");
            ndhe.AddItem(new MenuItem("ndheon", "Enable Healing")).SetValue(true);
            ndhe.AddItem(new MenuItem("ndhemana", "-> Minumum Mana")).SetValue(new Slider(55, 1));
            ndhe.AddItem(new MenuItem("ndhesw", "Switch Forms")).SetValue(false);

            foreach (var hero in HeroManager.Allies)
            {
                ndhe.AddItem(new MenuItem("x" + hero.ChampionName, "Heal on " + hero.ChampionName))
                    .SetValue(hero.IsMe);
                ndhe.AddItem(new MenuItem("z" + hero.ChampionName, hero.ChampionName + " below Pct% "))
                    .SetValue(new Slider(66, 1, 99));
            }

            humenu.AddSubMenu(ndhe);

            var ndhr = new Menu("(R) Aspect of the Cougar", "ndhr");
            ndhr.AddItem(new MenuItem("ndhrco", "Enable in Combo")).SetValue(true);
            ndhr.AddItem(new MenuItem("ndhrcreq", "-> Require Swipe/Takedown")).SetValue(true);
            ndhr.AddItem(new MenuItem("ndhrha", "Enable in Harass")).SetValue(true);
            ndhr.AddItem(new MenuItem("ndhrjg", "Enable in Jungle")).SetValue(true);
            ndhr.AddItem(new MenuItem("ndhrjreq", "-> Require Swipe/Takedown")).SetValue(true);
            ndhr.AddItem(new MenuItem("ndhrwc", "Enable in WaveClear")).SetValue(false);
            ndhr.AddItem(new MenuItem("ndhrgap", ":: Auto (R) Enemy Gapclosers")).SetValue(true);
            humenu.AddSubMenu(ndhr);

            var comenu = new Menu(":: Cougar Settings", "comenu");

            var ndcq = new Menu("(Q) Takedown", "ndcq");
            ndcq.AddItem(new MenuItem("ndcqco", "Enable in Combo")).SetValue(true);
            ndcq.AddItem(new MenuItem("ndcqha", "Enable in Harass")).SetValue(true);
            ndcq.AddItem(new MenuItem("ndcqjg", "Enable in Jungle")).SetValue(true);
            ndcq.AddItem(new MenuItem("ndcqwc", "Enable in WaveClear")).SetValue(true);
            ndcq.AddItem(new MenuItem("ndcqgap", ":: Auto (Q) Enemy Gapclosers")).SetValue(true);
            comenu.AddSubMenu(ndcq);

            var ndcw = new Menu("(W) Pounce", "ndcw");
            ndcw.AddItem(new MenuItem("ndcwcheck", "Check Hitchance")).SetValue(false);
            ndcw.AddItem(new MenuItem("ndcwch", "-> Min Hitchance"))
                .SetValue(new StringList(new[] {"Low", "Medium", "High", "Very High"}, 2));
            ndcw.AddItem(new MenuItem("ndcwco", "Enable in Combo")).SetValue(true);
            ndcw.AddItem(new MenuItem("ndcwhunt", "-> Ignore Checks if Hunted")).SetValue(false);
            ndcw.AddItem(new MenuItem("ndcwdistco", "-> Pounce Only if > AARange")).SetValue(true);
            ndcw.AddItem(new MenuItem("ndcwjg", "Enable in Jungle")).SetValue(true);
            ndcw.AddItem(new MenuItem("ndcwwc", "Enable in WaveClear")).SetValue(true);
            ndcw.AddItem(new MenuItem("ndcwdistwc", "-> Pounce Only if > AARange")).SetValue(false);
            ndcw.AddItem(new MenuItem("ndcwene", "-> Dont Pounce into Enemies")).SetValue(true);
            ndcw.AddItem(new MenuItem("ndcwtow", "-> Dont Pounce into Turret")).SetValue(true);
            comenu.AddSubMenu(ndcw);

            var ndce = new Menu("(E) Swipe", "ndce");

            ndce.AddItem(new MenuItem("ndcecheck", "Check Hitchance")).SetValue(false);
            ndce.AddItem(new MenuItem("ndcech", "-> Min Hitchance"))
                .SetValue(new StringList(new[] {"Low", "Medium", "High", "Very High"}, 2));
            ndce.AddItem(new MenuItem("ndceco", "Enable in Combo")).SetValue(true);
            ndce.AddItem(new MenuItem("ndceha", "Enable in Harass")).SetValue(true);
            ndce.AddItem(new MenuItem("ndcejg", "Enable in Jungle")).SetValue(true);
            ndce.AddItem(new MenuItem("ndcewc", "Enable in WaveClear")).SetValue(true);
            ndce.AddItem(new MenuItem("ndcenum", "-> Minimum Minions Hit")).SetValue(new Slider(3, 1, 5));           
            comenu.AddSubMenu(ndce);

            var ndcr = new Menu("(R) Aspect of the Cougar", "ndcr");
            ndcr.AddItem(new MenuItem("ndcrco", "Enable in Combo")).SetValue(true);
            ndcr.AddItem(new MenuItem("ndcrha", "Enable in Harass")).SetValue(true);
            ndcr.AddItem(new MenuItem("ndcrjg", "Enable in Jungle")).SetValue(true);
            ndcr.AddItem(new MenuItem("ndcrwc", "Enable in WaveClear")).SetValue(false);

            comenu.AddSubMenu(ndcr);


            var dmenu = new Menu(":: Draw Settings", "dmenu");
            dmenu.AddItem(new MenuItem("dp", ":: Draw Q Range")).SetValue(true);
            dmenu.AddItem(new MenuItem("dti", ":: Draw Q Timer")).SetValue(false);
            dmenu.AddItem(new MenuItem("dt", ":: Draw Target")).SetValue(true);
            dmenu.AddItem(new MenuItem("drawroot", ":: Draw Root Timer (Jungle)")).SetValue(true);
            ccmenu.AddSubMenu(dmenu);

            var xmenu = new Menu(":: Jungle Settings", "xmenu");
            xmenu.AddItem(new MenuItem("spcol", ":: Switch to Cougar if Spear Collision (Jungle)")).SetValue(false);
            xmenu.AddItem(new MenuItem("jgaacount", ":: AA Weaving (Jungle)"))
                .SetValue(new KeyBind('H', KeyBindType.Toggle))
                .SetTooltip("Require auto attacks before switching to Cougar").Permashow();
            xmenu.AddItem(new MenuItem("aareq", "-> Required auto attack Count (Jungle)"))
                .SetValue(new Slider(2, 1, 5));
            xmenu.AddItem(new MenuItem("kitejg", ":: Pounce Away (Jungle)")).SetTooltip("Try kiting with pounce.")
                .SetValue(false);
            ccmenu.AddSubMenu(xmenu);

            var aamenu = new Menu(":: Automatic Settings", "aamenu");
            aamenu.AddItem(new MenuItem("alvl6", ":: Auto (R) Level Up")).SetValue(true);
            aamenu.AddItem(new MenuItem("ndcegap", ":: Auto (E) Swipe Gapclosers")).SetValue(true);
            aamenu.AddItem(new MenuItem("ndhqgap", ":: Auto (Q) Javelin Gapclosers")).SetValue(true);

            aamenu.AddItem(new MenuItem("ndhqimm", ":: Auto (Q) Javelin Immobile")).SetValue(true);
            foreach (var ene in HeroManager.Enemies)
            {
                aamenu.AddItem(new MenuItem("autoq" + ene.ChampionName, "-> " + ene.ChampionName)).SetValue(false);
            }

            aamenu.AddItem(new MenuItem("ndhwimm", ":: Auto (W) Bushwhack Immobile")).SetValue(true);
            foreach (var ene in HeroManager.Enemies)
            {
                aamenu.AddItem(new MenuItem("autow" + ene.ChampionName, "-> " + ene.ChampionName)).SetValue(false);
            }

            ccmenu.AddItem(new MenuItem("pstyle", ":: Play Style"))
                .SetValue(new StringList(new[] {"Assassin", "Team Fighter"}, 1));


            ccmenu.AddSubMenu(comenu);
            ccmenu.AddSubMenu(humenu);
            ccmenu.AddSubMenu(aamenu);

            Root.AddSubMenu(ccmenu);

            var sset = new Menu(":: Smite Settings", "sset");
            sset.AddItem(new MenuItem("jgsmite", ":: Enable Smite")).SetValue(true);
            sset.AddItem(new MenuItem("jgsmitetd", ":: Takedown + Smite")).SetValue(true);
            sset.AddItem(new MenuItem("jgsmiteep", "-> Smite Epic")).SetValue(true);
            sset.AddItem(new MenuItem("jgsmitebg", "-> Smite Large")).SetValue(true);
            sset.AddItem(new MenuItem("jgsmitesm", "-> Smite Small")).SetValue(false);
            sset.AddItem(new MenuItem("jgsmitehe", "-> Smite On Hero")).SetValue(true);
            Root.AddSubMenu(sset);


            Root.AddItem(new MenuItem("usecombo", ":: Combo [active]")).SetValue(new KeyBind(32, KeyBindType.Press));
            Root.AddItem(new MenuItem("useharass", ":: Harass [active]"))
                .SetValue(new KeyBind('C', KeyBindType.Press));
            Root.AddItem(new MenuItem("usefarm", ":: Wave/Junge Clear [active]"))
                .SetValue(new KeyBind('V', KeyBindType.Press));
            Root.AddItem(new MenuItem("flee", ":: Flee/Walljumper [active]"))
                .SetValue(new KeyBind('A', KeyBindType.Press));

            Root.AddToMainMenu();

            #endregion

            Game.OnUpdate += Game_OnUpdate;
            Game.PrintChat("<b><font color=\"#FF33D6\">Kurisu's Nidalee</font></b> - Loaded!");

            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
            Obj_AI_Base.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
        }

        static void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            var hero = sender as Obj_AI_Hero;
            if (hero != null && hero.IsEnemy && KL.SpellTimer["Javelin"].IsReady() && Root.Item("ndhqimm").GetValue<bool>())
            {
                if (Root.Item("autoq" + hero.ChampionName).GetValue<bool>() && hero.IsValidTarget(KL.Spells["Javelin"].Range))
                {
                    if (args.Buff.Type == BuffType.Stun || args.Buff.Type == BuffType.Snare ||
                        args.Buff.Type == BuffType.Taunt || args.Buff.Type == BuffType.Knockback)
                    {
                        if (!KL.CatForm())
                        {
                            KL.Spells["Javelin"].Cast(hero);
                            KL.Spells["Javelin"].CastIfHitchanceEquals(hero, HitChance.Immobile);
                        }
                        else
                        {
                            if (KL.Spells["Aspect"].IsReady() &&
                                KL.Spells["Javelin"].Cast(hero) == Spell.CastStates.Collision)
                                KL.Spells["Aspect"].Cast();
                        }
                    }
                }
            }

            if (hero != null && hero.IsEnemy && KL.SpellTimer["Bushwhack"].IsReady() && Root.Item("ndhwimm").GetValue<bool>())
            {
                if (Root.Item("autow" + hero.ChampionName).GetValue<bool>() && hero.IsValidTarget(KL.Spells["Bushwhack"].Range))
                {
                    if (args.Buff.Type == BuffType.Stun || args.Buff.Type == BuffType.Snare ||
                        args.Buff.Type == BuffType.Taunt || args.Buff.Type == BuffType.Knockback)
                    {
                        KL.Spells["Bushwhack"].Cast(hero);
                        KL.Spells["Bushwhack"].CastIfHitchanceEquals(hero, HitChance.Immobile);
                    }
                }
            }
        }

        #region OnLevelUp
        static void Obj_AI_Base_OnLevelUp(Obj_AI_Base sender, EventArgs args)
        {
            var hero = sender as Obj_AI_Hero;
            if (hero != null && hero.IsMe)
            {
                switch (hero.Level)
                {
                    case 6:
                        Utility.DelayAction.Add(70 + Math.Min(60, Game.Ping),
                            () => { Player.Spellbook.LevelSpell(SpellSlot.R); });
                        break;
                    case 11:
                        Utility.DelayAction.Add(70 + Math.Min(60, Game.Ping),
                            () => { Player.Spellbook.LevelSpell(SpellSlot.R); });
                        break;
                    case 16:
                        Utility.DelayAction.Add(70 + Math.Min(60, Game.Ping),
                            () => { Player.Spellbook.LevelSpell(SpellSlot.R); });
                        break;
                }
            }
        }

        #endregion

        #region OnDraw
        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || !Player.IsValid)
            {
                return;
            }

            foreach (var unit in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget(900) && x.PassiveRooted()))
            {
                var b = unit.GetBuff("NidaleePassiveMonsterRoot");
                if (b.Caster.IsMe && b.EndTime - Game.Time > 0)
                {
                    var tpos = Drawing.WorldToScreen(unit.Position);
                    Drawing.DrawText(tpos[0], tpos[1], Color.DeepPink,
                        "ROOTED " + (b.EndTime - Game.Time).ToString("F"));
                }               
            }

            if (Root.Item("dti").GetValue<bool>())
            {
                var pos = Drawing.WorldToScreen(Player.Position);

                Drawing.DrawText(pos[0] + 100, pos[1] - 135, Color.White,
                    "Q: " + KL.SpellTimer["Javelin"].ToString("F"));             
            }

            if (Root.Item("dt").GetValue<bool>() && Target != null)
            {
                if (Root.Item("pstyle").GetValue<StringList>().SelectedIndex == 0)
                {
                    Render.Circle.DrawCircle(Target.Position, Target.BoundingRadius, Color.DeepPink, 6);
                }
            }

            if (Root.Item("dp").GetValue<bool>())
            {
                Render.Circle.DrawCircle(KL.Player.Position, !KL.CatForm()
                    ? KL.Spells["Javelin"].Range : KL.Spells["ExPounce"].Range, Color.FromArgb(155, Color.DeepPink), 4);
            }
        }

        #endregion

        internal static void Game_OnUpdate(EventArgs args)
        {
            Target = TargetSelector.GetTarget(KL.Spells["Javelin"].Range, TargetSelector.DamageType.Magical);

            #region Active Modes

            if (Root.Item("usecombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Root.Item("useharass").GetValue<KeyBind>().Active)
            {
                Harass();
            }

            if (Root.Item("usefarm").GetValue<KeyBind>().Active)
            {
                Jungle();
                WaveClear();
            }

            if (Root.Item("flee").GetValue<KeyBind>().Active)
            {
                Flee();
            }

            #endregion

            #region Auto Heal

            // auto heal on ally hero
            if (Root.Item("ndheon").GetValue<bool>() && KL.SpellTimer["Primalsurge"].IsReady())
            {
                if (!KL.NotLearned(KL.Spells["Primalsurge"]))
                {
                    if (!Player.Spellbook.IsChanneling && !Player.IsRecalling())
                    {
                        if (Root.Item("flee").GetValue<KeyBind>().Active && KL.CatForm())
                            return;

                        if (Player.Mana / Player.MaxMana * 100 < Root.Item("ndhemana").GetValue<Slider>().Value)
                            return;

                        if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None || !KL.CatForm())
                        {
                            foreach (
                                var hero in
                                    HeroManager.Allies.Where(
                                        h => Root.Item("x" + h.ChampionName).GetValue<bool>() &&
                                             h.IsValidTarget(KL.Spells["Primalsurge"].Range, false) &&
                                             h.Health / h.MaxHealth * 100 < Root.Item("z" + h.ChampionName).GetValue<Slider>().Value))
                            {
                                if (KL.CatForm() == false)
                                    KL.Spells["Primalsurge"].CastOnUnit(hero);

                                if (KL.CatForm() && Root.Item("ndhesw").GetValue<bool>() &&
                                    KL.Spells["Aspect"].IsReady())
                                    KL.Spells["Aspect"].Cast();
                            }
                        }
                    }
                }
            }

            #endregion
        }

        internal static void Combo()
        {
            var assassin = Root.Item("pstyle").GetValue<StringList>().SelectedIndex == 0;

            if (!Player.IsWindingUp)
            {
                CM.CastJavelin(assassin ? Target : TargetSelector.GetTarget(KL.Spells["Javelin"].Range, TargetSelector.DamageType.Magical), "co");
                CM.SwitchForm(assassin ? Target : TargetSelector.GetTarget(KL.Spells["Javelin"].Range, TargetSelector.DamageType.Magical), "co");
            }

            CM.CastBushwhack(assassin ? Target : TargetSelector.GetTarget(KL.Spells["Bushwhack"].Range, TargetSelector.DamageType.Magical), "co");
            CM.CastTakedown(assassin ? Target : TargetSelector.GetTarget(KL.Spells["Takedown"].Range, TargetSelector.DamageType.Magical), "co");
            CM.CastPounce(assassin ? Target : TargetSelector.GetTarget(KL.Spells["ExPounce"].Range, TargetSelector.DamageType.Magical), "co");
            CM.CastSwipe(assassin ? Target : TargetSelector.GetTarget(KL.Spells["Swipe"].Range, TargetSelector.DamageType.Magical), "co");
        }

        internal static void Harass()
        {
            CM.CastJavelin(TargetSelector.GetTarget(KL.Spells["Javelin"].Range, TargetSelector.DamageType.Magical), "ha");
            CM.CastTakedown(TargetSelector.GetTarget(KL.Spells["Takedown"].Range, TargetSelector.DamageType.Magical), "ha");
            CM.CastSwipe(TargetSelector.GetTarget(KL.Spells["Swipe"].Range, TargetSelector.DamageType.Magical), "ha");
            CM.SwitchForm(TargetSelector.GetTarget(KL.Spells["Javelin"].Range, TargetSelector.DamageType.Magical), "ha");
        }

        internal static void Jungle()
        {
            foreach (
                var minion in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => KL.MinionList.Any(y => x.Name.StartsWith(y) || x.IsHunted())))
            {
                if (minion.IsValidTarget(KL.Spells["ExPounce"].Range) && (!minion.Name.Contains("Mini") || minion.IsHunted()))
                {
                    CM.CastJavelin(minion, "jg");
                    CM.CastPounce(minion, "jg");
                    CM.CastBushwhack(minion, "jg");
                    CM.CastTakedown(minion, "jg");
                    CM.CastSwipe(minion, "jg");

                    if (minion.PassiveRooted() && Root.Item("jgaacount").GetValue<KeyBind>().Active && 
                        Player.Distance(minion.ServerPosition) > 450)
                    {
                        return;
                    }

                    CM.SwitchForm(minion, "jg");

                    if (!minion.IsHunted() && !minion.Name.Contains("Mini"))
                    {
                        return;
                    }
                }
            }

            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(x => !x.IsMinion))
            {
                if (minion.IsValidTarget(KL.Spells["Pounce"].Range + 250))
                {
                    CM.CastJavelin(minion, "jg");
                    CM.CastBushwhack(minion, "jg");
                    CM.CastTakedown(minion, "jg");
                    CM.CastPounce(minion, "jg");
                    CM.CastSwipe(minion, "jg");
                    CM.SwitchForm(minion, "jg");
                }
            }
        }

        internal static void WaveClear()
        {
            foreach (var minion in KL.MinionCache.Values.Where(x => x.IsMinion && x.IsValidTarget(KL.Spells["ExPounce"].Range)))
            {
                CM.CastJavelin(minion, "wc");
                CM.CastBushwhack(minion, "wc");
                CM.CastTakedown(minion, "wc");
                CM.CastPounce(minion, "wc");
                CM.CastSwipe(minion, "wc");
                CM.SwitchForm(minion, "wc");
            }
        }


        internal static void Flee()
        {
            if (!KL.CatForm() && KL.Spells["Aspect"].IsReady())
            {
                if (KL.SpellTimer["Pounce"].IsReady())
                    KL.Spells["Aspect"].Cast();
            }

            var wallCheck = KL.GetFirstWallPoint(KL.Player.Position, Game.CursorPos);

            if (wallCheck != null)
                wallCheck = KL.GetFirstWallPoint((Vector3) wallCheck, Game.CursorPos, 5);

            var movePosition = wallCheck != null ? (Vector3) wallCheck : Game.CursorPos;

            var tempGrid = NavMesh.WorldToGrid(movePosition.X, movePosition.Y);
            var fleeTargetPosition = NavMesh.GridToWorld((short) tempGrid.X, (short)tempGrid.Y);

            Obj_AI_Base target = null;

            var wallJumpPossible = false;

            if (KL.CatForm() && KL.SpellTimer["Pounce"].IsReady() && wallCheck != null)
            {
                var wallPosition = movePosition;

                var direction = (Game.CursorPos.To2D() - wallPosition.To2D()).Normalized();
                float maxAngle = 80f;
                float step = maxAngle / 20;
                float currentAngle = 0;
                float currentStep = 0;
                bool jumpTriggered = false;

                while (true)
                {
                    if (currentStep > maxAngle && currentAngle < 0)
                        break;

                    if ((currentAngle == 0 || currentAngle < 0) && currentStep != 0)
                    {
                        currentAngle = (currentStep) * (float)Math.PI / 180;
                        currentStep += step;
                    }

                    else if (currentAngle > 0)
                        currentAngle = -currentAngle;

                    Vector3 checkPoint;

                    if (currentStep == 0)
                    {
                        currentStep = step;
                        checkPoint = wallPosition + KL.Spells["Pounce"].Range * direction.To3D();
                    }

                    else
                        checkPoint = wallPosition + KL.Spells["Pounce"].Range * direction.Rotated(currentAngle).To3D();

                    if (checkPoint.IsWall()) 
                        continue;

                    wallCheck = KL.GetFirstWallPoint(checkPoint, wallPosition);

                    if (wallCheck == null) 
                        continue;

                    var wallPositionOpposite =  (Vector3) KL.GetFirstWallPoint((Vector3)wallCheck, wallPosition, 5);

                    if (KL.Player.GetPath(wallPositionOpposite).ToList().To2D().PathLength() -
                        KL.Player.Distance(wallPositionOpposite) > 200)
                    {
                        if (KL.Player.Distance(wallPositionOpposite) < KL.Spells["Pounce"].Range - KL.Player.BoundingRadius / 2)
                        {
                            KL.Spells["Pounce"].Cast(wallPositionOpposite);
                            jumpTriggered = true;
                            break;
                        }

                        else
                            wallJumpPossible = true;
                    }

                    else
                    {
                        Render.Circle.DrawCircle(Game.CursorPos, 35, Color.Red, 2);
                    }
                }

                if (!jumpTriggered)
                    Orbwalking.Orbwalk(target, Game.CursorPos, 90f, 0f, false, false);
            }

            else
            {
                Orbwalking.Orbwalk(target, Game.CursorPos, 90f, 0f, false, false);
                if (KL.CatForm() && KL.SpellTimer["Pounce"].IsReady())
                    KL.Spells["Pounce"].Cast(Game.CursorPos);
            }
        }
    }
}
