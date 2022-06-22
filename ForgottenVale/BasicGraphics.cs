using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ForgottenVale
{
    // A basic class set up to handle displaying static images on screen 
    class StaticGraphic
    {
        protected Vector2 m_pos;
        protected Texture2D m_txr;

        public Vector2 Position
        {
            get
            {
                return m_pos;
            }
            set
            {
                m_pos = value;
            }
        }

        public StaticGraphic(Vector2 position, Texture2D txrImage)
        {
            m_pos = position;
            m_txr = txrImage;
        }

        public virtual void drawme(SpriteBatch sBatch)
        {
            sBatch.Draw(m_txr, m_pos, Color.White);
        }

        public virtual void drawRock(SpriteBatch sBatch)
        {
            sBatch.Draw(m_txr, m_pos * 64, Color.White);
        }
    }


    // A class set up to handle moving sprites
    class MotionGraphic : StaticGraphic
    {
        protected Vector2 m_velocity;

        public MotionGraphic(Vector2 position, Texture2D txr) : base(position, txr)
        {
            m_velocity = Vector2.Zero;
        }

        public virtual void updateme(Vector2 vel)
        {
            m_velocity = vel;

            m_pos = m_pos + m_velocity;
        }
    }


    // A class set up to handle animated sprites
    class Animated2D : MotionGraphic
    {
        protected Rectangle m_srcRect;
        protected float m_updateTrigger;
        protected int m_framesPerSecond;
        protected int m_rows, m_cols;

        public Animated2D(Texture2D spriteSheet, Vector2 position, int rows, int cols, int fps) : base(position, spriteSheet)
        {
            m_rows = rows;
            m_cols = cols;

            m_srcRect = new Rectangle(0, 0, spriteSheet.Width / m_cols, spriteSheet.Height / m_rows);
            m_updateTrigger = 0;
            m_framesPerSecond = fps;
        }

        public virtual void drawme(SpriteBatch sBatch, GameTime gt)
        {
            m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

            if (m_updateTrigger >= 1)
            {
                m_updateTrigger = 0;
                m_srcRect.X += m_srcRect.Width;
                if (m_srcRect.X == m_txr.Width)
                {
                    m_srcRect.X = 0;

                    if (m_rows == 2 && m_srcRect.Y == 0)
                    {
                        m_srcRect.Y = m_txr.Height / 2;
                    }
                    else if (m_rows == 2 && m_srcRect.Y == m_txr.Height / 2)
                    {
                        m_srcRect.Y = 0;
                    }
                }
                    
            }

            sBatch.Draw(m_txr, new Vector2(m_pos.X * Game1.TILESIZE, m_pos.Y * Game1.TILESIZE), m_srcRect, Color.White); 
        }

        public virtual void drawmeHigher(SpriteBatch sBatch, GameTime gt)
        {
            m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

            if (m_updateTrigger >= 1)
            {
                m_updateTrigger = 0;
                m_srcRect.X += m_srcRect.Width;
                if (m_srcRect.X == m_txr.Width)
                    m_srcRect.X = 0;
            }

            sBatch.Draw(m_txr, new Vector2((m_pos.X * Game1.TILESIZE) + 5, (m_pos.Y * Game1.TILESIZE)-20), m_srcRect, Color.White);
        }

    }

    // A class set up to handle moving sprites
    class FadingGraphic : StaticGraphic
    {
        private float m_timer;

        public float Timer
        {
            get
            {
                return m_timer;
            }
        }

        public FadingGraphic(Vector2 position, Texture2D txr, float timer) : base(position, txr)
        {
            m_timer = timer;
        }

        public virtual void updateme(GameTime gt)
        {
            m_timer -= (float)gt.ElapsedGameTime.TotalSeconds;
        }

        public virtual void updateWithPos(GameTime gt, Vector2 camPos)
        {
            m_pos = camPos;
            m_timer -= (float)gt.ElapsedGameTime.TotalSeconds;
        }

        public override void drawme(SpriteBatch sBatch)
        {
            sBatch.Draw(m_txr, m_pos, Color.White * m_timer);

            //base.drawme(sBatch);
        }
    }
}