using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForgottenVale
{
    class Notifications
    {
        // class variables
        private Texture2D m_panelTex;
        private Vector2 m_drawPos, m_drawOffset, m_drawStringOffset;
        private int m_chestContent;
        private float m_timer, m_stringLegnth, m_stringHeight;
        private string m_notTex;

        public float Timer
        {
            get
            {
                return m_timer;
            }
        }

        public Notifications(Texture2D panTex, int contents)
        {
            m_panelTex = panTex;
            m_chestContent = contents;

            m_drawOffset = new Vector2(960 - (m_panelTex.Width / 2), 150 - (m_panelTex.Height / 2));

            m_timer = 2;

            #region Info on chest content IDs
            // Chest Contents
            // 1 = health potion
            // 2 = magick potion
            // 3 = gold coins
            // 4 = spirit orb
            // 5 = nerwens staff
            // 6 = dancing shoes
            #endregion

            // assign notification text
            switch (m_chestContent)
            {
                case 1:
                    m_notTex = "Found Health Potion";
                    break;
                case 2:
                    m_notTex = "Found Magick Potion";
                    break;
                case 3:
                    m_notTex = "Found Gold Coins";
                    break;
                case 4:
                    m_notTex = "Found Spirit Orb";
                    break;
                case 5:
                    m_notTex = "* Found Nerwens Staff *";
                    break;
                case 6:
                    m_notTex = "* Found Dancing Shoes *";
                    break;
                case 7:
                    m_notTex = "* Learned Wish Spell *";
                    break;
                case 8:
                    m_notTex = "* Learned Rock n Stone *";
                    break;
                case 9:
                    m_notTex = "* Learned Fire Wall *";
                    break;
                case 10:
                    m_notTex = "* Learned Special Dance *";
                    break;
                default:
                    m_notTex = "PLACEHOLDER TEXT";
                    break;
            }

            // measure string
            m_stringLegnth = Game1.uiFontOne.MeasureString(m_notTex).X;
            m_stringHeight = Game1.uiFontOne.MeasureString(m_notTex).Y;

            // set up draw position
            m_drawStringOffset = new Vector2(960 - (m_stringLegnth/ 2), 150 - (m_stringHeight / 2));
        }

        public void drawMe(SpriteBatch sb, GameTime gt, Vector2 drawPos)
        {
            m_drawPos = drawPos;
            m_timer -= (float)gt.ElapsedGameTime.TotalSeconds;

            sb.Draw(m_panelTex, m_drawPos + m_drawOffset, Color.White);
            sb.DrawString(Game1.uiFontOne, m_notTex, m_drawPos + m_drawStringOffset, Color.White);
        }
    }
}
