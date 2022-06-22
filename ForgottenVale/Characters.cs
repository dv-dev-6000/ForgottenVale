using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ForgottenVale
{
    class baseNPC : Animated2D
    {
        //class variables
        protected string m_charName;
        protected bool m_isActive;
        protected int m_ID;

        public int ID
        {
            get
            {
                return m_ID;
            }
        }
        public string CharName
        {
            get
            {
                return m_charName;
            }
        }
        public bool IsActive
        {
            get
            {
                return m_isActive;
            }
            set
            {
                m_isActive = value;
            }
        }

        public baseNPC(Texture2D spriteSheet, Vector2 position, int rows, int cols, int fps, string name) : base(spriteSheet, position, rows, cols, fps)
        {
            m_charName = name;
            m_isActive = true;
            m_ID = -1;
        }

        public virtual void updateMe() 
        {
            
        }

        public override void drawme(SpriteBatch sBatch, GameTime gt)
        {
            if (m_isActive)
            {
                m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

                if (m_updateTrigger >= 1)
                {
                    m_updateTrigger = 0;
                    m_srcRect.X += m_srcRect.Width;
                    if (m_srcRect.X == m_txr.Width)
                        m_srcRect.X = 0;
                }

                sBatch.Draw(m_txr, new Vector2(m_pos.X * Game1.TILESIZE, (m_pos.Y * Game1.TILESIZE) - 14), m_srcRect, Color.White);
            }
        }
    }

    class EnemySprite : baseNPC
    {
        //class variables
        

        public EnemySprite(Texture2D spriteSheet, Vector2 position, int rows, int cols, int fps, string name, int id) : base(spriteSheet, position, rows, cols, fps, name)
        {
            IsActive = false;
            m_ID = id;
        }

        public override void drawme(SpriteBatch sBatch, GameTime gt)
        {
            if (m_isActive)
            {
                m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

                if (m_updateTrigger >= 1)
                {
                    m_updateTrigger = 0;
                    m_srcRect.X += m_srcRect.Width;
                    if (m_srcRect.X == m_txr.Width)
                        m_srcRect.X = m_srcRect.Width;
                }

                sBatch.Draw(m_txr, new Vector2(m_pos.X * Game1.TILESIZE, (m_pos.Y * Game1.TILESIZE) - 14), m_srcRect, Color.White);
            }
            else
            {
                m_srcRect.X = 0;
                sBatch.Draw(m_txr, new Vector2(m_pos.X * Game1.TILESIZE, (m_pos.Y * Game1.TILESIZE) - 14), m_srcRect, Color.White);
            }
        }
    }

    class BossSprite : baseNPC
    {
        //class variables


        public BossSprite(Texture2D spriteSheet, Vector2 position, int rows, int cols, int fps, string name, int id) : base(spriteSheet, position, rows, cols, fps, name)
        {
            IsActive = false;
            m_ID = id;
        }

        public override void drawme(SpriteBatch sBatch, GameTime gt)
        {
            if (m_isActive)
            {
                m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

                if (m_updateTrigger >= 1)
                {
                    m_updateTrigger = 0;
                    m_srcRect.X += m_srcRect.Width;
                    if (m_srcRect.X == m_txr.Width)
                        m_srcRect.X = m_txr.Width - (m_srcRect.Width * 2);
                }

                sBatch.Draw(m_txr, new Vector2(m_pos.X * Game1.TILESIZE, (m_pos.Y * Game1.TILESIZE) - 14), m_srcRect, Color.White);
            }
            else
            {
                m_srcRect.X = 0;
                sBatch.Draw(m_txr, new Vector2(m_pos.X * Game1.TILESIZE, (m_pos.Y * Game1.TILESIZE) - 14), m_srcRect, Color.White);
            }
        }
    }
}
