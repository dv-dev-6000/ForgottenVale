using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForgottenVale
{
    class StartMenu
    {
        // vars
        Texture2D m_menuTex, m_howToPageTex, m_cursorTex, m_howToSecondTex, m_currTutPage;
        Texture2D m_newBut, m_howToBut, m_contBut, m_quitBut;

        private Vector2 m_menuPos;

        private Vector2[] cursorLocs;
        private int m_cursorPos;

        private bool m_gameInProg, m_tutorialUp;

        public bool IsTutorialUp
        {
            get
            {
                return m_tutorialUp;
            }
            set
            {
                m_tutorialUp = value;
            }
        }

        public StartMenu(Texture2D menuTex, Texture2D howToTex, Texture2D cursorTex, Texture2D newBut, Texture2D howToBut, Texture2D contBut, Texture2D quitBut, Texture2D how2)
        {
            m_menuTex = menuTex;
            m_cursorTex = cursorTex;
            m_cursorPos = 0;

            m_contBut = contBut;
            m_howToBut = howToBut;
            m_newBut = newBut;
            m_quitBut = quitBut;
            m_howToPageTex = howToTex;
            m_howToSecondTex = how2;

            m_currTutPage = m_howToSecondTex;

            m_menuPos = new Vector2(960 - m_menuTex.Width/2, 430);
            cursorLocs = new Vector2[4] { new Vector2(m_menuPos.X + 60, m_menuPos.Y + 60), new Vector2(m_menuPos.X + 60, m_menuPos.Y + 160), new Vector2(m_menuPos.X + 60, m_menuPos.Y + 260), new Vector2(m_menuPos.X + 60, m_menuPos.Y + 360) };
        }

        public void updateMe(GamePadState padCurr, GamePadState padOld, SoundEffect uiMove)
        {
            // move the cursor
            if (padCurr.DPad.Up == ButtonState.Pressed && padOld.DPad.Up == ButtonState.Released && !IsTutorialUp)
            {
                if (m_cursorPos > 0)
                {
                    m_cursorPos--;
                }
                else
                {
                    m_cursorPos = 3;
                }
                uiMove.Play(0.3f, 0, 0);
            }
            else if (padCurr.DPad.Down == ButtonState.Pressed && padOld.DPad.Down == ButtonState.Released && !IsTutorialUp)
            {
                if (m_cursorPos < 3)
                {
                    m_cursorPos++;
                }
                else
                {
                    m_cursorPos = 0;
                }
                uiMove.Play(0.3f, 0, 0);
            }

            if (IsTutorialUp)
            {
                if (padCurr.DPad.Right == ButtonState.Pressed && padOld.DPad.Right == ButtonState.Released && m_currTutPage == m_howToPageTex)
                {
                    m_currTutPage = m_howToSecondTex;
                }
                else if (padCurr.DPad.Left == ButtonState.Pressed && padOld.DPad.Left == ButtonState.Released && m_currTutPage == m_howToSecondTex)
                {
                    m_currTutPage = m_howToPageTex;
                }

                if (padCurr.Buttons.B == ButtonState.Pressed && padOld.Buttons.B == ButtonState.Released)
                {
                    IsTutorialUp = false;
                }
            }

        }

        public int chooseMe()
        {
            return m_cursorPos;
        }

        public void drawMe(SpriteBatch sb, bool gameInProg)
        {
            m_gameInProg = gameInProg;

            sb.Draw(m_menuTex, m_menuPos, Color.White);
            sb.Draw(m_cursorTex, cursorLocs[m_cursorPos], Color.White);

            sb.Draw(m_howToBut, cursorLocs[0] + new Vector2(110, 10), Color.White);
            sb.Draw(m_newBut, cursorLocs[1] + new Vector2(110, 10), Color.White);
            if (m_gameInProg) 
            { 
                sb.Draw(m_contBut, cursorLocs[2] + new Vector2(110, 10), Color.White); 
            }
            else { sb.Draw(m_contBut, cursorLocs[2] + new Vector2(110, 10), Color.Gray); }
            sb.Draw(m_quitBut, cursorLocs[3] + new Vector2(110, 10), Color.White);

            if (IsTutorialUp)
            {
                sb.Draw(m_currTutPage, Vector2.Zero, Color.White);
            }

            //sb.DrawString(Game1.uiFontOne, "" + m_gameInProg, new Vector2(1, 50), Color.White);
        }
    }
}
