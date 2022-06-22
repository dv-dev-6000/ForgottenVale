using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ForgottenVale
{
    class TileClass
    {
        private Texture2D m_tex;
        private Vector2 m_pos;
        private Rectangle m_rect;
        private bool isWalkable, isWilderness;

        private string m_NPCname;
        private int m_chestID, m_enemyID;

        public bool IsWalkable
        {
            get
            {
                return isWalkable;
            }
            set
            {
                isWalkable = value;
            }
        }
        public bool IsWilderness
        {
            get
            {
                return isWilderness;
            }
        }

        public string NPC_Name
        {
            get
            {
                return m_NPCname;
            }
            set
            {
                m_NPCname = value;
            }
        }

        public int ChestID
        {
            get
            {
                return m_chestID;
            }
            set
            {
                m_chestID = value;
            }
        }

        public int EnemyID
        {
            get
            {
                return m_enemyID;
            }
            set
            {
                m_enemyID = value;
            }
        }

        public Vector2 TilePos 
        {
            get
            {
                return m_pos;
            }
        }

        public TileClass(Vector2 pos, bool Walkable, Texture2D tex, bool wilderness = false)
        {
            m_pos = pos;
            m_rect = new Rectangle(new Point((int)pos.X + 2, (int)pos.Y+2), new Point(60, 60));
            m_tex = tex;

            isWalkable = Walkable;
            if (wilderness) { isWilderness = true; }

            m_NPCname = "unknown";
            m_chestID = -1;
            m_enemyID = -1;
        }

        public virtual void drawme(SpriteBatch sBatch)
        {
            if (isWalkable)
            {
                sBatch.Draw(m_tex, new Rectangle(new Point((int)(m_pos.X * 64) + 2, (int)(m_pos.Y * 64) + 2), new Point(60, 60)), Color.BlueViolet * 0.5f); // write better code
            }
            else
            {
                sBatch.Draw(m_tex, new Rectangle(new Point((int)(m_pos.X * 64) + 2, (int)(m_pos.Y * 64) + 2), new Point(60, 60)), Color.DarkRed * 0.5f); // write better code
            }
            sBatch.DrawString(Game1.debugFont, m_pos.X + " - " + m_pos.Y, m_pos*64, Color.White);
            if (isWilderness) { sBatch.DrawString(Game1.debugFont, "Wild", new Vector2((m_pos.X * 64), (m_pos.Y * 64) + 20), Color.White); }
            if (m_NPCname != "unknown") { sBatch.DrawString(Game1.debugFont, m_NPCname + " " + m_enemyID, new Vector2((m_pos.X * 64), (m_pos.Y * 64) + 40), Color.White); }
        }
    }
}
