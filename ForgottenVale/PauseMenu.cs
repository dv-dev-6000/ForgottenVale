using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForgottenVale
{
    class PauseMenu
    {
        // class variables
        private Texture2D m_menuTex, m_cursorTex;
        private Vector2 m_drawPos;
        private PlayerInfo m_pInfo;

        private Vector2[] cursorLocs = new Vector2[3] { new Vector2(1570, 575), new Vector2(1570, 735) , new Vector2(1570, 895) };
        private int m_cursorPos;

        private bool isPaused;

        public bool IsPaused
        {
            get
            {
                return isPaused;
            }
            set
            {
                isPaused = value;
            }
        }

        public PauseMenu(Texture2D menuTex, Texture2D cursorTex, PlayerInfo pInfo)
        {
            m_menuTex = menuTex;
            m_cursorTex = cursorTex;
            m_cursorPos = 0;
            
            m_pInfo = pInfo;

            isPaused = false;
        }

        public void updateMe(GamePadState padCurr, GamePadState padOld, Vector2 drawPos, SoundEffect movCurs)
        {
            m_drawPos = drawPos;

            // move the cursor
            if (padCurr.DPad.Up == ButtonState.Pressed && padOld.DPad.Up == ButtonState.Released)
            {
                if (m_cursorPos > 0)
                {
                    m_cursorPos--;
                }
                else
                {
                    m_cursorPos = 2;
                }
                movCurs.Play(0.3f, 0, 0);
            }
            else if(padCurr.DPad.Down == ButtonState.Pressed && padOld.DPad.Down == ButtonState.Released)
            {
                if (m_cursorPos < 2)
                {
                    m_cursorPos++;
                }
                else
                {
                    m_cursorPos = 0;
                }
                movCurs.Play(0.3f, 0, 0);
            }

            // Select Option
            if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)
            {
                switch (m_cursorPos)
                {
                    case 0:
                        if (m_pInfo.HitPoints < m_pInfo.MaxHP && m_pInfo.HealthPotion > 0)
                        {
                            m_pInfo.HitPoints += m_pInfo.HealthRecovery;
                            m_pInfo.HealthPotion--;
                            if (m_pInfo.HitPoints > m_pInfo.MaxHP) { m_pInfo.HitPoints = m_pInfo.MaxHP; }
                        }
                        else
                        {
                            // play bad sound
                        }
                        break;
                    case 1:
                        if (m_pInfo.MagickPoints < m_pInfo.MaxMP && m_pInfo.MagickPotion > 0)
                        {
                            m_pInfo.MagickPoints += m_pInfo.MagickRecovery;
                            m_pInfo.MagickPotion--;
                            if (m_pInfo.MagickPoints > m_pInfo.MaxMP) { m_pInfo.MagickPoints = m_pInfo.MaxMP; }
                        }
                        else
                        {
                            // play bad sound
                        }
                        break;
                    case 2:
                        isPaused = false;
                        break;
                }

            }
            else if (padCurr.Buttons.B == ButtonState.Pressed && padOld.Buttons.B == ButtonState.Released)
            {
                isPaused = false;
            }
        }

        public void drawMe(SpriteBatch sb)
        {
            sb.Draw(m_menuTex, m_drawPos, Color.White);
            sb.Draw(m_cursorTex, m_drawPos + cursorLocs[m_cursorPos], Color.LightSkyBlue);

            // draw info text

            // player info
            sb.DrawString(Game1.uiFontOne, "" + ((m_pInfo.HitPoints * 100) / m_pInfo.MaxHP) + "%", m_drawPos + new Vector2(438, 110), Color.Black);            // display points as percent
            sb.DrawString(Game1.uiFontOne, "" + ((m_pInfo.MagickPoints * 100) / m_pInfo.MaxMP) + "%", m_drawPos + new Vector2(438, 210), Color.Black);         //

            sb.DrawString(Game1.uiFontOne, " " + m_pInfo.HealthPotion, m_drawPos + new Vector2(630, 110), Color.Black);
            sb.DrawString(Game1.uiFontOne, " " + m_pInfo.MagickPotion, m_drawPos + new Vector2(630, 210), Color.Black);

            sb.DrawString(Game1.uiFontOne, " " + m_pInfo.Coins, m_drawPos + new Vector2(820, 110), Color.Black);
            sb.DrawString(Game1.uiFontOne, " " + m_pInfo.SpiritOrbs, m_drawPos + new Vector2(820, 210), Color.Black);

            // spells known
            if (m_pInfo.GetSpellKnown(0)) { sb.DrawString(Game1.uiFontTwo, "Magick Wave: \nBasic magick attack", m_drawPos + new Vector2(120, 433), Color.White); }
            if (m_pInfo.GetSpellKnown(1)) { sb.DrawString(Game1.uiFontTwo, "Barrier: \nA temporary shield to protect the player", m_drawPos + new Vector2(120, 528), Color.White); }
            if (m_pInfo.GetSpellKnown(2)) { sb.DrawString(Game1.uiFontTwo, "Special Dance: \nPacify enemies with your elegance", m_drawPos + new Vector2(120, 623), Color.White); }
            if (m_pInfo.GetSpellKnown(3)) { sb.DrawString(Game1.uiFontTwo, "Wish: \nMake a wish and hope for the best", m_drawPos + new Vector2(120, 718), Color.White); }
            if (m_pInfo.GetSpellKnown(4)) { sb.DrawString(Game1.uiFontTwo, "Rock 'n' Stone: \nHurl debris at an enemy", m_drawPos + new Vector2(120, 813), Color.White); }
            if (m_pInfo.GetSpellKnown(5)) { sb.DrawString(Game1.uiFontTwo, "Firewall: \nEncircle your enemy with flames", m_drawPos + new Vector2(120, 908), Color.White); }

            // companion info
            if (m_pInfo.GetCompanion == Companion.Alone) {  sb.DrawString(Game1.uiFontTwo, "No Companion Info", m_drawPos + new Vector2(1030, 510), Color.White); }
            if (m_pInfo.GetCompanion == Companion.TheGunslinger) 
            {
                sb.DrawString(Game1.uiFontTwo, "Juan the Gunslinger", m_drawPos + new Vector2(1030, 510), Color.White);
                sb.DrawString(Game1.uiFontTwo, "(Active) Sniper Shot: \nA deadly rifle shot, \nTakes one round to \naim.", m_drawPos + new Vector2(1030, 600), Color.White);
                sb.DrawString(Game1.uiFontTwo, "(Passive) Retribution: \nTriggers a weak \nattack when the \nplayer takes damage.", m_drawPos + new Vector2(1030, 800), Color.White);
            }
            if (m_pInfo.GetCompanion == Companion.TheWarrior) 
            {
                sb.DrawString(Game1.uiFontTwo, "Hewran the Warrior", m_drawPos + new Vector2(1030, 510), Color.White);
                sb.DrawString(Game1.uiFontTwo, "(Active) Magick Axe: \nStrike with a deadly, \nEnchanted axe.", m_drawPos + new Vector2(1030, 600), Color.White);
                sb.DrawString(Game1.uiFontTwo, "(Passive) Shield Wall: \nIncreased defence \nwhen travelling \nwith the warrior.", m_drawPos + new Vector2(1030, 800), Color.White);
            }
            if (m_pInfo.GetCompanion == Companion.TheWitchdoctor) 
            {
                sb.DrawString(Game1.uiFontTwo, "Nerwen the Ranger", m_drawPos + new Vector2(1030, 510), Color.White);
                sb.DrawString(Game1.uiFontTwo, "(Active) Battle Heal: \nRegain Health by, \nthe power of magick", m_drawPos + new Vector2(1030, 600), Color.White);
                sb.DrawString(Game1.uiFontTwo, "(Passive) High Stakes: \nDeal more damage \nwhen health is low", m_drawPos + new Vector2(1030, 800), Color.White);
            }

            // objective
            sb.DrawString(Game1.uiFontTwo, m_pInfo.CurrObjective, m_drawPos + new Vector2(1030, 150), Color.White);
        }
    }
}
