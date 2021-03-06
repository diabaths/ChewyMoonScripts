﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using TeamFightCalculator.Properties;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;
using Rectangle = SharpDX.Rectangle;

namespace TeamFightCalculator
{
    /// <summary>
    ///     The program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        ///     The status of the calculation
        /// </summary>
        public enum CalcStatus
        {
            /// <summary>
            ///     There are no enemies.
            /// </summary>
            NoEnemies,

            /// <summary>
            ///     The calculation was calculated
            /// </summary>
            Calculated
        }

        #region Constants

        /// <summary>
        ///     The height
        /// </summary>
        private const int Height = 100;

        /// <summary>
        ///     The width
        /// </summary>
        private const int Width = 275;

        #endregion

        #region Static Fields

        /// <summary>
        ///     The calculated slots
        /// </summary>
        private static readonly SpellSlot[] CalculatedSlots = {SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R};

        /// <summary>
        ///     The ally damage
        /// </summary>
        private static double allyDamage;

        /// <summary>
        ///     The ally health
        /// </summary>
        private static double allyHealth;

        /// <summary>
        ///     The enemy damage
        /// </summary>
        private static double enemyDamage;

        /// <summary>
        ///     The enemy health
        /// </summary>
        private static double enemyHealth;

        /// <summary>
        ///     The last mouse position
        /// </summary>
        private static Vector2 lastMousePos;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the allies.
        /// </summary>
        /// <value>
        ///     The allies.
        /// </value>
        private static IEnumerable<Obj_AI_Hero> Allies { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the box is being dragged.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the box is being dragged; otherwise, <c>false</c>.
        /// </value>
        private static bool BeingDragged { get; set; }

        /// <summary>
        ///     Gets or sets the box line.
        /// </summary>
        /// <value>
        ///     The box line.
        /// </value>
        private static Line BoxLine { get; set; }

        /// <summary>
        ///     Gets the calculate range.
        /// </summary>
        /// <value>
        ///     The calculate range.
        /// </value>
        private static int CalculateRange
        {
            get { return Menu.Item("CalculateRange").GetValue<Slider>().Value; }
        }

        /// <summary>
        ///     Gets or sets the enemies.
        /// </summary>
        /// <value>
        ///     The enemies.
        /// </value>
        private static IEnumerable<Obj_AI_Hero> Enemies { get; set; }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        private static Menu Menu { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the mouse is over the box.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the mouse is over the box; otherwise, <c>false</c>.
        /// </value>
        private static bool MouseOverBox
        {
            get
            {
                return
                    new Rectangle((int) Position.X, (int) Position.Y - Height/2, Width, Height).Contains(
                        Utils.GetCursorPos());
            }
        }

        /// <summary>
        ///     Gets or sets the position.
        /// </summary>
        /// <value>
        ///     The position.
        /// </value>
        private static Vector2 Position { get; set; }

        /// <summary>
        ///     Gets the real box position.
        /// </summary>
        /// <value>
        ///     The real box position.
        /// </value>
        private static Vector2 RealBoxPosition
        {
            get { return Position - new Vector2(0, Height/2f); }
        }

        /// <summary>
        ///     Gets or sets the sprite.
        /// </summary>
        /// <value>
        ///     The sprite.
        /// </value>
        private static Sprite Sprite { get; set; }

        /// <summary>
        ///     Gets or sets the status.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        private static CalcStatus Status { get; set; }

        /// <summary>
        ///     Gets or sets the text font.
        /// </summary>
        /// <value>
        ///     The text font.
        /// </value>
        private static Font TextFont { get; set; }

        /// <summary>
        ///     Gets or sets the last result.
        /// </summary>
        /// <value>
        ///     The last result.
        /// </value>
        private static Bitmap LastResult { get; set; }

        /// <summary>
        ///     Gets or sets the indicator sprite.
        /// </summary>
        /// <value>
        ///     The indicator sprite.
        /// </value>
        private static Render.Sprite IndicatorSprite { get; set; }

        /// <summary>
        ///     Gets the indicator position.
        /// </summary>
        /// <value>
        ///     The indicator position.
        /// </value>
        private static Vector2 IndicatorPosition
        {
            get { return RealBoxPosition + new Vector2(Width - 100, 10); }
        }

        /// <summary>
        ///     Gets or sets the text height spacing.
        /// </summary>
        /// <value>
        ///     The text height spacing.
        /// </value>
        private static int TextHeightSpacing { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [draw calculation].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [draw calculation]; otherwise, <c>false</c>.
        /// </value>
        private static bool DrawCalculation { get; set; }

        /// <summary>
        ///     Gets or sets the green bitmap.
        /// </summary>
        /// <value>
        ///     The green bitmap.
        /// </value>
        private static Bitmap GreenBitmap { get; set; }

        /// <summary>
        ///     Gets or sets the red bitmap.
        /// </summary>
        /// <value>
        ///     The red bitmap.
        /// </value>
        private static Bitmap RedBitmap { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = new Menu("Team Fight Calculator", "cmTFC", true);

            var calcSettingMenu = new Menu("Calculation Settings", "CalcSettings");
            calcSettingMenu.AddItem(new MenuItem("SpellReadyCheck", "Check if spells are ready").SetValue(true));
            Menu.AddSubMenu(calcSettingMenu);

            Menu.AddItem(new MenuItem("CalculateRange", "Calculate Range").SetValue(new Slider(0x5DC, 0x9C4, 0x1388)));
            Menu.AddItem(new MenuItem("XPos", "X Position").SetValue(new Slider(100, 0, Drawing.Width)));
            Menu.AddItem(new MenuItem("YPos", "Y Position").SetValue(new Slider(200, 0, Drawing.Height)));
            Menu.AddItem(new MenuItem("DrawCalculation", "Draw Calculation").SetValue(true));
            Menu.AddItem(new MenuItem("DrawRange", "Draw Calculation Range").SetValue(true));
            Menu.AddToMainMenu();

            Menu.Item("DrawCalculation").ValueChanged += (sender, args) => DrawCalculation = args.GetNewValue<bool>();
            DrawCalculation = Menu.Item("DrawCalculation").GetValue<bool>();

            Menu.Item("XPos").ValueChanged +=
                (sender, args) =>
                {
                    if (!BeingDragged)
                    {
                        Position = new Vector2(args.GetNewValue<Slider>().Value, Position.Y);
                    }
                };
            Menu.Item("YPos").ValueChanged +=
                (sender, args) =>
                {
                    if (!BeingDragged)
                    {
                        Position = new Vector2(Position.X, args.GetOldValue<Slider>().Value);
                    }
                };

            Position = new Vector2(Menu.Item("XPos").GetValue<Slider>().Value,
                Menu.Item("YPos").GetValue<Slider>().Value);
        }

        /// <summary>
        ///     Fired when the game is drawn.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Menu.Item("DrawRange").IsActive())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position,
                    Menu.Item("CalculateRange").GetValue<Slider>().Value, Color.CadetBlue);
            }

            if (Status == CalcStatus.NoEnemies || !DrawCalculation)
            {
                return;
            }

            // Draw box
            BoxLine.Width = Height;
            BoxLine.Begin();
            BoxLine.Draw(new[] {Position, Position + new Vector2(Width, 0)}, new ColorBGRA(0, 0, 0, 256/2));
            BoxLine.End();

            // Draw text
            TextFont.DrawText(
                null,
                string.Format("Ally Damage: {0}", (int) allyDamage),
                (int) RealBoxPosition.X + 10,
                (int) RealBoxPosition.Y + 5,
                new ColorBGRA(0, 255, 0, 255));

            TextFont.DrawText(
                null,
                string.Format("Ally Health: {0}", (int) allyHealth),
                (int) (RealBoxPosition.X + 10),
                (int) RealBoxPosition.Y + 5 + TextHeightSpacing,
                new ColorBGRA(0, 255, 0, 255));

            TextFont.DrawText(
                null,
                string.Format("Enemy Damage: {0}", (int) enemyDamage),
                (int) (RealBoxPosition.X + 10),
                (int) RealBoxPosition.Y + 5 + TextHeightSpacing*2,
                new ColorBGRA(255, 0, 0, 255));

            TextFont.DrawText(
                null,
                string.Format("Enemy Health: {0}", (int) enemyHealth),
                (int) (RealBoxPosition.X + 10),
                (int) RealBoxPosition.Y + 5 + TextHeightSpacing*3,
                new ColorBGRA(255, 0, 0, 255));
        }

        /// <summary>
        ///     Fired when the DirectX device is lost.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Drawing_OnPreReset(EventArgs args)
        {
            BoxLine.OnLostDevice();
            TextFont.OnLostDevice();
        }

        /// <summary>
        ///     Fired when the DirectX device is reset.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void DrawingOnOnPostReset(EventArgs args)
        {
            BoxLine.OnResetDevice();
            TextFont.OnResetDevice();
        }

        /// <summary>
        ///     Fired when the game is loaded.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            CreateMenu();

            var heroes = ObjectManager.Get<Obj_AI_Hero>();
            var objAiHeroes = heroes as Obj_AI_Hero[] ?? heroes.ToArray();

            Allies = objAiHeroes.Where(x => x.IsAlly);
            Enemies = objAiHeroes.Where(x => x.IsEnemy);

            Status = CalcStatus.NoEnemies;

            BoxLine = new Line(Drawing.Direct3DDevice);
            TextFont = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 14,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Antialiased
                });
            Sprite = new Sprite(Drawing.Direct3DDevice);

            IndicatorSprite = new Render.Sprite(Resources.Green_Check, IndicatorPosition);
            IndicatorSprite.PositionUpdate += () => IndicatorPosition;
            IndicatorSprite.VisibleCondition =
                sender => Status != CalcStatus.NoEnemies && DrawCalculation;
            IndicatorSprite.Scale = new Vector2(0.4f);
            IndicatorSprite.Add();

            TextHeightSpacing = TextFont.MeasureText(Sprite, "A").Height + 10;

            GreenBitmap = Resources.Green_Check;
            RedBitmap = Resources.Red_X;

            Game.PrintChat(
                "<font color=\"#7CFC00\"><b>Team Fight Calculator:</b></font> by ChewyMoon loaded. The UI only shows when there is enemies! (for now)");

            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += DrawingOnOnPostReset;
            Game.OnWndProc += Game_OnWndProc;
            Game.OnUpdate += GameOnOnUpdate;
        }

        /// <summary>
        ///     Fired when the game receives an event.
        /// </summary>
        /// <param name="args">The <see cref="WndEventArgs" /> instance containing the event data.</param>
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (BeingDragged)
            {
                var newPos = Utils.GetCursorPos();
                Position += newPos - lastMousePos;
                lastMousePos = newPos;
            }

            if (args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN && MouseOverBox)
            {
                BeingDragged = true;
                lastMousePos = Utils.GetCursorPos();
            }

            if (args.Msg == (uint) WindowsMessages.WM_LBUTTONUP && BeingDragged)
            {
                Menu.Item("XPos").SetValue(new Slider((int) Position.X, 0, Drawing.Width));
                Menu.Item("YPos").SetValue(new Slider((int) Position.Y, 0, Drawing.Height));

                BeingDragged = false;
            }
        }

        /// <summary>
        ///     Fired when the game is updated.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameOnOnUpdate(EventArgs args)
        {
            var enemies = Enemies.Where(x => x.IsValidTarget(CalculateRange)).ToArray();
            var allies = Allies.Where(x => x.IsValidTarget(CalculateRange, false)).ToArray();

            if (!enemies.Any())
            {
                Status = CalcStatus.NoEnemies;
                return;
            }

            var spellReadyCheck = Menu.Item("SpellReadyCheck").IsActive();

            allyDamage = CalculateDamage(allies.ToList(), spellReadyCheck);
            enemyDamage = CalculateDamage(enemies.ToList(), spellReadyCheck);

            allyHealth = allies.Sum(x => x.Health);
            enemyHealth = enemies.Sum(x => x.Health);

            var result = allyHealth - enemyDamage > enemyHealth - allyDamage ? GreenBitmap : RedBitmap;

            if (LastResult != result)
            {
                IndicatorSprite.Remove();
                IndicatorSprite.UpdateTextureBitmap((Bitmap) result.Clone());
                IndicatorSprite.Add();
            }

            LastResult = result;

            Status = CalcStatus.Calculated;
        }

        /// <summary>
        ///     Calculates the damage.
        /// </summary>
        /// <param name="heroes">The heroes.</param>
        /// <param name="spellReadyCheck">if set to <c>true</c> checks if the spell is ready.</param>
        /// <returns></returns>
        private static double CalculateDamage(List<Obj_AI_Hero> heroes,
            bool spellReadyCheck)
        {
            var dmg = 0d;

            for (var i = 0; i < 5; i++)
            {
                var i1 = i;

                dmg += heroes.Sum(
                    x =>
                        x.GetComboDamage(new Obj_AI_Base(),
                            (spellReadyCheck ? CalculatedSlots.Where(y => x.GetSpell(y).IsReady()) : CalculatedSlots)
                                .Select(z => new Tuple<SpellSlot, int>(z, i1))) +
                        x.GetAutoAttackDamage(new Obj_AI_Base(), true));
            }

            return dmg;
        }

        /// <summary>
        ///     The entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #endregion
    }
}