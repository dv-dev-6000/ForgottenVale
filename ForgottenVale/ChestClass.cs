using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ForgottenVale
{
    class ChestClass : StaticGraphic
    {
        // Chest Contents
        // 1 = health potion
        // 2 = magick potion
        // 3 = gold coins
        // 4 = spirit orb
        // 5 = nerwens staff
        // 6 = dancing shoes
        // 7 = Wish
        // 8 = RockNstone
        // 9 = fireWall

        // Class Variables
        private int m_id, m_content;
        private bool m_isOpen;
        private Rectangle m_srcRect;

        public bool IsOpen
        {
            get
            {
                return m_isOpen;
            }
            set
            {
                m_isOpen = value;
            }
        }
        public int ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }
        public int Contents
        {
            get
            {
                return m_content;
            }
        }

        public ChestClass(Vector2 pos, Texture2D tex, int ID, int contents) : base (pos, tex)
        {
            m_id = ID;
            m_isOpen = false;
            m_srcRect = new Rectangle(0, 0, m_txr.Width / 2, m_txr.Height);
            m_content = contents;
        }

        public override void drawme(SpriteBatch sBatch)
        {
            if (m_isOpen)
            {
                m_srcRect.X = m_txr.Width / 2;
            }
            else
            {
                m_srcRect.X = 0;
            }

            sBatch.Draw(m_txr, new Vector2(m_pos.X * Game1.TILESIZE, (m_pos.Y * Game1.TILESIZE) - 14), m_srcRect, Color.White);
        }
    }
}
