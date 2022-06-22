using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForgottenVale
{
    enum Companion 
    {
        Alone,
        TheWarrior,
        TheGunslinger,
        TheWitchdoctor
    }

    class PlayerInfo
    {
        // Class Variables
        private int m_hitPoints, m_hitPointsMax, m_magickPoints, m_magickPointsMax, m_spiritOrbs, m_coins;      // players availible points
        private Companion currCompanion;                                                                        // current companion

        private bool[] spellsKnown;                                                                             // array to track which spells are known
        private Texture2D m_playerPortrait, m_healthTex;                                                        // player image to be displayed in HUD
        private bool m_gotShoes, m_gotStaff, m_isBound;
        private bool m_heroEnc, m_justDied, m_shrineActive, m_triggerLandru, m_heroGone;

        private int m_defence, m_power;
        private int m_hPotion, m_mPotion, m_hRecovery, m_mRecovery, m_endTrig;

        private string m_currObjective;

        private Vector2 hpPos, mpPos;

        private int m_nerwenRank, m_kevinRank, m_brooxRank, m_legHeroRank, m_calliRank, m_lanRank;

        public bool HeroEncTrig
        {
            get
            {
                return m_heroEnc;
            }
            set
            {
                m_heroEnc = value;
            }
        }

        #region Relationship Rank Accessor Metrhods
        public int NerwenRank
        {
            get
            {
                return m_nerwenRank;
            }
            set
            {
                m_nerwenRank = value;
            }
        }
        public int CalliRank
        {
            get
            {
                return m_calliRank;
            }
            set
            {
                m_calliRank = value;
            }
        }
        public int KevinRank
        {
            get
            {
                return m_kevinRank;
            }
            set
            {
                m_kevinRank = value;
            }
        }
        public int BrooxRank
        {
            get
            {
                return m_brooxRank;
            }
            set
            {
                m_brooxRank = value;
            }
        }
        public int LegHeroRank
        {
            get
            {
                return m_legHeroRank;
            }
            set
            {
                m_legHeroRank = value;
            }
        }
        public int LandruRank
        {
            get
            {
                return m_lanRank;
            }
            set
            {
                m_lanRank = value;
            }
        }
        #endregion

        #region Accessor Methods
        public int HitPoints
        {
            get
            {
                return m_hitPoints;
            }
            set
            {
                m_hitPoints = value;
            }
        }
        public int MaxHP
        {
            get
            {
                return m_hitPointsMax;
            }
        }
        public int MagickPoints
        {
            get
            {
                return m_magickPoints;
            }
            set
            {
                m_magickPoints = value;
            }
        }
        public int MaxMP
        {
            get
            {
                return m_magickPointsMax;
            }
        }
        public int SpiritOrbs
        {
            get
            {
                return m_spiritOrbs;
            }
            set
            {
                m_spiritOrbs = value;
            }
        }
        public int MagickPotion
        {
            get
            {
                return m_mPotion;
            }
            set
            {
                m_mPotion = value;
            }
        }
        public int EndTrigger
        {
            get
            {
                return m_endTrig;
            }
            set
            {
                m_endTrig = value;
            }
        }
        public int MagickRecovery
        {
            get
            {
                return m_mRecovery;
            }
        }
        public int HealthPotion
        {
            get
            {
                return m_hPotion;
            }
            set
            {
                m_hPotion = value;
            }
        }
        public int HealthRecovery
        {
            get
            {
                return m_hRecovery;
            }
        }
        public int Coins
        {
            get
            {
                return m_coins;
            }
            set
            {
                m_coins = value;
            }
        }
        public string CurrObjective
        {
            get
            {
                return m_currObjective;
            }
            set
            {
                m_currObjective = value;
            }
        }
        public int Defence
        {
            get
            {
                return m_defence;
            }
            set
            {
                m_defence = value;
            }
        }
        public int Power
        {
            get
            {
                return m_power;
            }
            set
            {
                m_power = value;
            }
        }
        public bool HeroGone
        {
            get
            {
                return m_heroGone;
            }
            set
            {
                m_heroGone = value;
            }
        }
        public bool GotStaff
        {
            get
            {
                return m_gotStaff;
            }
            set
            {
                m_gotStaff = value;
            }
        }
        public bool GotShoes
        {
            get
            {
                return m_gotShoes;
            }
            set
            {
                m_gotShoes = value;
            }
        }
        public bool IsBound
        {
            get
            {
                return m_isBound;
            }
            set
            {
                m_isBound = value;
            }
        }
        public bool JustDied
        {
            get
            {
                return m_justDied;
            }
            set
            {
                m_justDied = value;
            }
        }
        public bool ShrineActive
        {
            get
            {
                return m_shrineActive;
            }
            set
            {
                m_shrineActive = value;
            }
        }
        public bool TriggerLandru
        {
            get
            {
                return m_triggerLandru;
            }
            set
            {
                m_triggerLandru = value;
            }
        }
        public Companion GetCompanion
        {
            get
            {
                return currCompanion;
            }
            set
            {
                currCompanion = value;
            }
        }
        #endregion

        public bool GetSpellKnown(int spell)
        {
            return spellsKnown[spell];
        }

        public void LearnSpell(int spell)
        {
            spellsKnown[spell] = true;
        }

        public PlayerInfo(Texture2D playerPortrait, Texture2D healthTex)
        {
            m_hitPointsMax = 96;
            m_hitPoints = m_hitPointsMax;
            m_magickPointsMax = 83;
            m_magickPoints = m_magickPointsMax;
            m_spiritOrbs = 0;

            m_playerPortrait = playerPortrait;
            m_healthTex = healthTex;

            currCompanion = Companion.Alone;

            spellsKnown = new bool[6] { true, true, false, false, false, false };

            m_defence = 1;
            m_power = 0;

            m_mPotion = 1;
            m_hPotion = 1;
            m_coins = 0;

            m_gotShoes = false;
            m_gotStaff = false;

            hpPos = new Vector2(940, 640);
            mpPos = new Vector2(1065, 580);

            m_mRecovery = 15;
            m_hRecovery = 30;

            m_nerwenRank = 0;
            m_kevinRank = 0;
            m_brooxRank = 0;
            m_legHeroRank = 0;
            m_calliRank = 0;
            m_endTrig = 0;      // 0 = not ended, 1 = landru defeated, 2 = player refused to fight
            m_lanRank = 0;

            m_justDied = false;
            m_shrineActive = false;
            m_triggerLandru = false;
            m_heroGone = false;
            
            m_currObjective =   "Search the clearing " +
                                "\n'A magical power seems to eminate from " +
                                "\nthe area, i should investigate.'";

            m_heroEnc = false;
        }

        #region Player Attacks
        public int melee()
        {
            int damage = 10 + m_power;//Game1.RNG.Next(10, 16) + m_power;

            return damage;
        }
        public int HeroStrike()
        {
            int damage = 30 + m_power;

            return damage;
        }
        public int magickWave()
        {
            // deals magick damage
            int damage = Game1.RNG.Next(15,21) + m_power;
            m_magickPoints -= 5;

            return damage;
        }
        public void barrier()
        {
            // create temporary shield
            m_defence = 3;
            m_magickPoints -= 8;
        }
        public int specialDance(int res)
        {
            // charm an opponent
            // returns  1 on fail
            //          2 on success
            //          3 on crit success
            m_magickPoints -= 8;

            if (res == 0)                                       // if enemy reslience is zero then auto crit
            {                                                   //
                return 3;                                       //
            }                                                   //
            else                                                // if resilience is more than zero then roll D10
            {                                                   //
                int roll = Game1.RNG.Next(1, 11);               //
                if (roll == 10)                                 //      if 10 rolled crit
                {                                               //
                    return 3;                                   //
                }                                               //
                else if (roll > res)                            //      if roll is higher than resilience then succeed
                {                                               //
                    return 2;                                   //
                }                                               //
                else { return 1; }                              //      if lower than or equal to resilience then fail
            }                                                   //
        }
        public int rockNstone()
        {
            // attack with the fury of nature
            int damage = Game1.RNG.Next(25, 31) + m_power;
            m_magickPoints -= 15;

            return damage;
        }
        public int fireWall()
        {
            // mid level fire attack, deals damage to enemy when enemy attacks
            int damage = Game1.RNG.Next(20, 31) + m_power;
            m_magickPoints -= 10;

            return damage;
        }
        #endregion

        public virtual void drawMe(SpriteBatch sb)
        {
            if (m_hitPoints > m_hitPointsMax / 4)
            {
                for (int i = 1; i <= m_hitPoints; i++)
                {
                    sb.Draw(m_healthTex, new Rectangle((int)hpPos.X + (i * 10), (int)hpPos.Y, 10, 35), Color.Green);
                }
            }
            else
            {
                for (int i = 1; i <= m_hitPoints; i++)
                {
                    sb.Draw(m_healthTex, new Rectangle((int)hpPos.X + (i * 10), (int)hpPos.Y, 10, 35), Color.Red);
                }
            }
            
            for (int i = 1; i <= m_magickPoints; i++)
            {
                sb.Draw(m_healthTex, new Rectangle((int)mpPos.X + (i * 10), (int)mpPos.Y, 10, 35), Color.Purple);
            }
        }
    }
}
