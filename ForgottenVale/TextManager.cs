using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForgottenVale
{
    class TextManager
    {
        private string m_fulltext;
        private int m_textSoFar;
        private float m_speed;
        private float m_ttt;

        private SpriteFont m_currFont;

        /// <param name="text">The text that is to be displayed.</param>
        /// <param name="spriteFont">The font to use.</param>
        /// <param name="maxLineWidth">maximum pixel legnth before a new line.</param>
        public TextManager(string text, SpriteFont spriteFont, float maxLineWidth)
        {
            m_textSoFar = 0;
            m_fulltext = WrapText(spriteFont, text, maxLineWidth);
            m_currFont = spriteFont;
        }

        public string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = spriteFont.MeasureString(" ").X;

            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }

        /// <param name="sb">The spritebatch currently in use.</param>
        /// <param name="gT">The game time so we can time the appearance delay.</param>
        /// <param name="speed">The speed that the letters appear at.</param>
        /// <param name="loc">The top left pixel of the text.</param>
        /// <param name="colour">The colour to display the text.</param>
        /// <returns>True if it's finished, false if it's still typing.</returns>
        public bool DrawMe(SpriteBatch sb, GameTime gT, Vector2 loc, Color colour, float speed, SoundEffect blip)
        {
            m_speed = speed;

            sb.DrawString(m_currFont, m_fulltext.Substring(0, m_textSoFar), loc, colour);

            if (m_textSoFar >= m_fulltext.Length)
                return true;
            else
            {

                if (m_ttt < 0)
                {
                    float pitch = -(float)Game1.RNG.NextDouble();
                    if (pitch < -0.5f) { pitch = pitch / 4; }

                    m_textSoFar++;
                    m_ttt = m_speed;
                    if (m_textSoFar % 2 == 0) { blip.Play(0.1f, pitch, 0); } //0.2f pitch
                }
                else
                {
                    m_ttt -= (float)gT.ElapsedGameTime.TotalSeconds;
                }
                return false;
            }
        }
    }
}
