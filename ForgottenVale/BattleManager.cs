using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForgottenVale
{
    enum PlayerOptions
    {
        Fight,
        Item,
        Null,
        Magick,
    }

    enum BattleStage 
    { 
        PreRound,
        PlayerTurn,
        EnemyTurn,
        PostRound,
    }

    enum RoundProg
    {
        Zero,
        s1,
        s2,
        s3,
    }

    class BattleManager
    {
        // Class Variables
        PlayerInfo m_player;
        EnemyBase m_currEnemy;

        private bool m_battleOver;
        private int m_enemyStage = 0;

        PlayerOptions currOption = PlayerOptions.Null;
        BattleStage m_currBattStage = BattleStage.PlayerTurn;
        RoundProg currProg = RoundProg.Zero;

        // Cursor Placement
        private Vector2[] positions = new Vector2[6] { new Vector2(1200, 780), new Vector2(1550, 780), new Vector2(1200, 880), new Vector2(1550, 880), new Vector2(1200, 980), new Vector2(1550, 980) };

        private int m_cursorPos, m_maxOptions;

        // Sound
        private float movPitch, movVol;

        // text display
        private TextManager tMan;
        public string m_battNotification = "Choose an action...";    // NEEDS TO NOT BE PUBLIC !!!
        private float textSpeed;

        // companion moves info
        private int m_shieldWallValue = 5;
        private bool m_magickAxe = false;
        private bool m_retribution = false;
        private bool m_sniperShot = false;


        public bool BattleOver
        {
            get
            {
                return m_battleOver;
            }
            set
            {
                m_battleOver = value;
            }
        }
        public BattleStage CurrBattleStage
        {
            get
            {
                return m_currBattStage;
            }
        }
        public int EnemyHP 
        {
            get
            {
                return m_currEnemy.HitPoints;
            }
        }
        public string CurrEnemyName 
        {
            get
            {
                return m_currEnemy.Name;
            }
        }



        public BattleManager(PlayerInfo player, EnemyBase enemyType)
        {
            m_player = player;
            m_currEnemy = enemyType;

            m_cursorPos = 0;
            m_battleOver = false;

            movPitch = 0f;    // from -1 to 1
            movVol = 0.3f;    // from 0 to 1
            
            textSpeed = -0.01f;
            textRefresh(textSpeed);
        }

        public void textRefresh(float tSpeed)
        {
            tMan = new TextManager(m_battNotification, Game1.uiFontOne, 950);
        }

        public void flee(int fleeChance)
        {
            m_battNotification = "Player attempts to flee...";
            textRefresh(textSpeed);

            if ((m_currEnemy.Name == "Forn" || m_currEnemy.Name == "Simmons" || m_currEnemy.Name == "Landru_6000"))
            {
                m_battleOver = false;
                m_battNotification = "You can't run from me Wanderer!";
                textRefresh(textSpeed);
                //m_currBattStage = BattleStage.EnemyTurn;
            }
            else
            {
                int temp = Game1.RNG.Next(0, fleeChance);

                if (temp == 0)
                {
                    m_battleOver = false;
                    m_battNotification = "You can't Escape...";
                    textRefresh(textSpeed);
                    m_currBattStage = BattleStage.EnemyTurn;
                }
                else
                {
                    m_battNotification = "Got away safely... [press A]";
                    textRefresh(textSpeed);
                    m_battleOver = true;
                }
            }
        }

        public void preRound(GamePadState padCurr, GamePadState padOld)
        {
            if (currProg == RoundProg.Zero)
            {
                if (m_player.Defence > 1 && m_magickAxe)
                {
                    switch (m_player.Defence)
                    {
                        case 3:
                            m_player.Defence = 2;
                            m_battNotification = "As the barrier begins to weaken, Hewran springs forth and strikes with her mighty magick axe!";
                            textRefresh(textSpeed);
                            break;
                        case 2:
                            m_player.Defence = 1;
                            m_battNotification = "As the barrier is destroyed, Hewran springs forth and strikes with her mighty magick axe!";
                            textRefresh(textSpeed);
                            break;
                        default:
                            break;
                    }
                    m_currEnemy.HitPoints -= 20;
                    m_magickAxe = false;
                    currProg = RoundProg.s1;
                }
                else if (m_player.Defence > 1)
                {
                    switch (m_player.Defence)
                    {
                        case 3:
                            m_player.Defence = 2;
                            m_battNotification = "The barrier was weakened!";
                            textRefresh(textSpeed);
                            break;
                        case 2:
                            m_player.Defence = 1;
                            m_battNotification = "The barrier was destroyed!";
                            textRefresh(textSpeed);
                            break;
                        default:
                            break;
                    }
                    currProg = RoundProg.s1;
                }
                else if (m_magickAxe)
                {
                    m_currEnemy.HitPoints -= 20;
                    m_battNotification = "Hewran springs forth and strikes with her mighty magick axe!";
                    textRefresh(textSpeed);
                    m_magickAxe = false;
                    currProg = RoundProg.s1;
                }
                else
                {
                    m_currBattStage = BattleStage.PlayerTurn;
                    currProg = RoundProg.Zero;
                    m_battNotification = "Choose an action...";
                    textRefresh(textSpeed);
                }
            }
            else if (currProg == RoundProg.s1)
            {
                if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)
                {
                    m_currBattStage = BattleStage.PlayerTurn;
                    currProg = RoundProg.Zero;
                    m_battNotification = "Choose an action...";
                    textRefresh(textSpeed);
                }
            }

            //m_currBattStage = BattleStage.PlayerTurn;
        }

        public void playerTurn(GamePadState padCurr, GamePadState padOld, SoundEffect moveCurs)
        {
            #region SET MAX NUMBER OF OPTIONS PER STAGE
            switch (currOption)
            {
                // max option should be set to the maximum number of options ***MINUS ONE***

                case PlayerOptions.Fight:
                    m_maxOptions = 3;
                    break;
                case PlayerOptions.Item:
                    m_maxOptions = 1;
                    break;
                case PlayerOptions.Null:
                    m_maxOptions = 2;
                    break;
                case PlayerOptions.Magick:
                    m_maxOptions = 5;
                    break;
                default:
                    break;

            }
            #endregion

            #region MOVING THE CURSOR
            if (padCurr.DPad.Right == ButtonState.Pressed && padOld.DPad.Right == ButtonState.Released)
            {
                if (m_cursorPos < m_maxOptions)
                {
                    m_cursorPos++;
                }
                else { m_cursorPos = 0; }
                moveCurs.Play(movVol, movPitch, 0);
            }
            else if (padCurr.DPad.Left == ButtonState.Pressed && padOld.DPad.Left == ButtonState.Released)
            {
                if (m_cursorPos > 0)
                {
                    m_cursorPos--;
                }
                else { m_cursorPos = m_maxOptions; }
                moveCurs.Play(movVol, movPitch, 0);
            }
            else if (padCurr.DPad.Down == ButtonState.Pressed && padOld.DPad.Down == ButtonState.Released)
            {
                if (m_maxOptions % 2 == 0)  // if the max position is in the left column
                {
                    if (m_cursorPos < (m_maxOptions - 1))
                    {
                        m_cursorPos += 2;
                    }
                    else
                    {
                        if (m_cursorPos == m_maxOptions)
                        {
                            m_cursorPos = 0;
                        }
                        else if (m_cursorPos == (m_maxOptions - 1))
                        {
                            m_cursorPos = 1;
                        }
                    }
                }
                else                        // if the max position is in the right column
                {
                    if (m_cursorPos < (m_maxOptions - 1))
                    {
                        m_cursorPos += 2;
                    }
                    else
                    {
                        if (m_cursorPos == m_maxOptions)
                        {
                            m_cursorPos = 1;
                        }
                        else if (m_cursorPos == (m_maxOptions - 1))
                        {
                            m_cursorPos = 0;
                        }
                    }
                }
                moveCurs.Play(movVol, movPitch, 0);

            }
            else if (padCurr.DPad.Up == ButtonState.Pressed && padOld.DPad.Up == ButtonState.Released)
            {
                if (m_cursorPos > 1)
                {
                    m_cursorPos -= 2;
                }
                else if (m_cursorPos <= 1)
                {
                    if (m_maxOptions % 2 == 0)  // if the max position is in the left column
                    {
                        switch (m_cursorPos)
                        {
                            case 0:
                                m_cursorPos = m_maxOptions;
                                break;
                            case 1:
                                m_cursorPos = m_maxOptions - 1;
                                break;
                        }
                    }
                    else                        // if the max position is in the right column
                    {
                        switch (m_cursorPos)
                        {
                            case 0:
                                m_cursorPos = m_maxOptions - 1;
                                break;
                            case 1:
                                m_cursorPos = m_maxOptions;
                                break;
                        }
                    }
                }
                moveCurs.Play(movVol, movPitch, 0);
            }
            #endregion

            #region SELECTING AN OPTION or GOING BACK ONE STEP
            if (m_sniperShot)
            {
                m_currEnemy.HitPoints -= 30;
                m_battNotification = "BANG! Juan takes the shot! Right in the kisser for 30 damage!";
                textRefresh(textSpeed);
                m_currBattStage = BattleStage.EnemyTurn;
                m_sniperShot = false;
            }
            else if (currOption == PlayerOptions.Null)
            {
                if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)
                {
                    switch (m_cursorPos)
                    {
                        case 0:
                            currOption = PlayerOptions.Fight;
                            m_battNotification = "How will you attack?";
                            textRefresh(textSpeed);
                            m_cursorPos = 0;
                            break;
                        case 1:
                            currOption = PlayerOptions.Item;
                            m_battNotification = "Choose an item.";
                            textRefresh(textSpeed);
                            m_cursorPos = 0;
                            break;
                        case 2:
                            flee(m_currEnemy.FleeChance);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (currOption == PlayerOptions.Fight)
            {
                if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)
                {
                    switch (m_cursorPos)
                    {
                        case 0:
                            int tempDamage = m_player.melee();
                            m_currEnemy.HitPoints -= tempDamage;
                            m_battNotification = "Whack!! Enemy Takes " + tempDamage + " Damage!";
                            textRefresh(textSpeed);
                            m_currBattStage = BattleStage.EnemyTurn;
                            break;
                        case 1:
                            currOption = PlayerOptions.Magick;
                            m_battNotification = "Which spell will you cast?";
                            textRefresh(textSpeed);
                            break;
                        case 2:
                            if (!m_player.HeroGone)
                            {
                                if (m_player.SpiritOrbs > 0)
                                {
                                    int heroDamage = m_player.HeroStrike();
                                    m_currEnemy.HitPoints -= heroDamage;
                                    m_player.SpiritOrbs--;
                                    m_battNotification = "Ka-Blamo!! You unleash the spirit of the Legendary Hero. Enemy Takes " + heroDamage + " Damage!";
                                    textRefresh(textSpeed);
                                    m_currBattStage = BattleStage.EnemyTurn;
                                }
                                else
                                {
                                    m_battNotification = "You need to get more spirit orbs before you can summon the Legendary Hero.";
                                    textRefresh(textSpeed);
                                }
                            }
                            else
                            {
                                m_battNotification = "The spirit of the legendary hero has departed this world...";
                                textRefresh(textSpeed);
                            }

                            break;
                        case 3:
                            if (m_player.GetCompanion == Companion.TheWarrior)
                            {
                                // Magick Axe
                                m_battNotification = "You begin to enchant Hewrans axe with powerful magick...";
                                m_magickAxe = true;
                                textRefresh(textSpeed);
                                m_currBattStage = BattleStage.EnemyTurn;
                            }
                            else if (m_player.GetCompanion == Companion.TheGunslinger)
                            {
                                // Sniper Shot
                                m_battNotification = "Juan draws out his rifle and takes aim...";
                                m_sniperShot = true;
                                textRefresh(textSpeed);
                                m_currBattStage = BattleStage.EnemyTurn;
                            }
                            else if (m_player.GetCompanion == Companion.TheWitchdoctor)
                            {
                                m_player.HitPoints += 30;
                                if (m_player.HitPoints > m_player.MaxHP) { m_player.HitPoints = m_player.MaxHP; }
                                m_battNotification = "Nerwen casts a healing spell, you regain Hit Points.";
                                textRefresh(textSpeed);
                                m_currBattStage = BattleStage.EnemyTurn;
                            }

                            break;
                        default:
                            break;
                    }
                    m_cursorPos = 0;
                }
                else if (padCurr.Buttons.B == ButtonState.Pressed && padOld.Buttons.B == ButtonState.Released)
                {
                    currOption = PlayerOptions.Null;
                    m_battNotification = "Choose an action...";
                    textRefresh(textSpeed);
                    m_cursorPos = 0;
                }
            }
            else if (currOption == PlayerOptions.Magick)
            {
                if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)
                {
                    switch (m_cursorPos)
                    {
                        case 0:
                            // cast magick wave
                            if (m_player.MagickPoints > 5)
                            {
                                int waveDamage = m_player.magickWave();
                                m_currEnemy.HitPoints -= waveDamage;
                                m_battNotification = "You cast Magick Wave! Enemy Takes " + waveDamage + " Damage.";
                                textRefresh(textSpeed);
                                m_currBattStage = BattleStage.EnemyTurn;
                            }
                            else 
                            { 
                                m_battNotification = "You dont have enough magick points."; 
                                textRefresh(textSpeed); 
                            }
                            
                            break;
                        case 1:
                            // cast barrier
                            if (m_player.MagickPoints > 8)
                            {
                                if (m_player.Defence <= 1)
                                {
                                    m_player.barrier();
                                    m_battNotification = "You cast Barrier! a temporary shield was raised.";
                                    textRefresh(textSpeed);
                                    m_currBattStage = BattleStage.EnemyTurn;
                                }
                                else
                                {
                                    m_battNotification = "A Barrier is already in place.";
                                    textRefresh(textSpeed);
                                }
                            }
                            else
                            {
                                m_battNotification = "You dont have enough magick points.";
                                textRefresh(textSpeed);
                            }

                            break;
                        case 2:
                            // cast special dance
                            if (m_player.MagickPoints > 8)
                            {
                                if (m_currEnemy.Charmed)
                                {
                                    m_battNotification = "This foe is already under your spell.";
                                    textRefresh(textSpeed);
                                }
                                else
                                {
                                    if (m_currEnemy.Name == "Forn")
                                    {
                                        m_battNotification = "'Well Bless My Bark! What a wonderfull performance... so elegant.. so gracefull... I'm simply DELIGHTED! Please, take the shard and go with my blessing.";
                                        textRefresh(textSpeed);
                                        m_currBattStage = BattleStage.EnemyTurn;
                                        m_battleOver = true;
                                        m_player.MagickPoints -= 8;
                                    }
                                    else
                                    {
                                        switch (m_player.specialDance(m_currEnemy.Resilience))
                                        {
                                            case 1:
                                                m_battNotification = "Your enemy is not impressed...";
                                                textRefresh(textSpeed);
                                                m_currBattStage = BattleStage.EnemyTurn;
                                                return;
                                            case 2:
                                                m_battNotification = "Your opponent is charmed by your elegance.";
                                                textRefresh(textSpeed);
                                                m_currEnemy.Charmed = true;
                                                m_currBattStage = BattleStage.EnemyTurn;
                                                return;
                                            case 3:
                                                m_battNotification = "Your opponent is DELIGHTED by your performance and swiftly vacates the battlefield.";
                                                textRefresh(textSpeed);
                                                m_currBattStage = BattleStage.EnemyTurn;
                                                m_battleOver = true;
                                                return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                m_battNotification = "You dont have enough magick points.";
                                textRefresh(textSpeed);
                            }

                            break;
                        case 3:
                            // cast Wish
                            if (m_player.MagickPoints > 10)
                            {
                                if (m_currEnemy.Name == "Simmons")
                                {
                                    m_battNotification = "'Wait... What... ... Really? You wished for my FREEDOM! Thank you, kind wanderer. It would be rude to kill you now.. so.. i gues you can go. Rock On Little Buddy!'";
                                    textRefresh(textSpeed);
                                    m_currBattStage = BattleStage.EnemyTurn;
                                    m_battleOver = true;
                                }
                                else
                                {
                                    m_player.MagickPoints -= 10;
                                    int roll = Game1.RNG.Next(1, 7);
                                    int damage = 0;
                                    string wishText = " ";
                                    switch (roll)
                                    {
                                        case 1:
                                            damage = 10;
                                            wishText = "A ghostly hand is summoned to the battlefield. It slaps you in the face for " + damage + " damage... not ideal.";
                                            m_player.HitPoints -= damage;
                                            break;
                                        case 2:
                                            damage = 0;
                                            wishText = "It's quiet, A solitary tumbleweed rolls across the battlefield... nothing happens.";
                                            break;
                                        case 3:
                                            damage = 40 + m_player.Power;
                                            wishText = "CLANGGG! An anvil appears from nowhere and falls on to the enemy dealing " + damage + " damage... a crushing blow.";
                                            m_currEnemy.HitPoints -= damage;
                                            break;
                                        case 4:
                                            wishText = "Tree like roots momentarily sprout from your feet and burrow into the ground. your health is fully restored as nutrients from the earth envigorate your body.";
                                            m_player.HitPoints = m_player.MaxHP;
                                            break;
                                        case 5:
                                            damage = (m_currEnemy.HitPoints / 2) + m_player.Power;
                                            wishText = "A wild otter appeared, yelled 'pay the taxman!', then took half the enemies health and some of your gold... how odd.";
                                            if (m_player.Coins > 0)
                                            {
                                                if (m_player.Coins > 5)
                                                {
                                                    m_player.Coins -= 5;
                                                }
                                                else
                                                {
                                                    m_player.Coins--;
                                                }
                                            }
                                            m_currEnemy.HitPoints -= damage;
                                            break;
                                        case 6:
                                            damage = 20 + m_player.Power;
                                            wishText = "Enemy stood on a rake. Eeuuurrrgghheeeuuurrggghh!";
                                            m_currEnemy.HitPoints -= damage;
                                            break;
                                    }
                                    m_battNotification = "You make a wish. " + wishText;
                                    textRefresh(textSpeed);
                                    m_currBattStage = BattleStage.EnemyTurn;
                                }
                            }
                            else
                            {
                                m_battNotification = "You dont have enough magick points.";
                                textRefresh(textSpeed);
                            }

                            break;
                        case 4:
                            // cast Rock'N'Stone
                            if (m_player.MagickPoints > 15)
                            {
                                int rnsDamage = m_player.rockNstone();
                                m_currEnemy.HitPoints -= rnsDamage;
                                m_battNotification = "HAAaaRRRooOOoomM!!! you unleash the fury of nature and deal " + rnsDamage + " Damage.";
                                textRefresh(textSpeed);
                                m_currBattStage = BattleStage.EnemyTurn;
                            }
                            else
                            {
                                m_battNotification = "You dont have enough magick points.";
                                textRefresh(textSpeed);
                            }

                            break;
                        case 5:
                            // cast Firewall
                            if (m_player.MagickPoints > 10)
                            {
                                int fireDamage = m_player.fireWall();
                                m_currEnemy.HitPoints -= fireDamage;
                                m_battNotification = "You scorch the enemy with fire for " + fireDamage + " Damage. The enemy is surrounded by a ring of fire.";
                                textRefresh(textSpeed);
                                m_currEnemy.Flamed = true;
                                m_currBattStage = BattleStage.EnemyTurn;
                            }
                            else
                            {
                                m_battNotification = "You dont have enough magick points.";
                                textRefresh(textSpeed);
                            }

                            break;
                        default:
                            break;
                    }
                    m_cursorPos = 0;
                }
                else if (padCurr.Buttons.B == ButtonState.Pressed && padOld.Buttons.B == ButtonState.Released)
                {
                    currOption = PlayerOptions.Fight;
                    m_battNotification = "How will you attack?";
                    textRefresh(textSpeed);
                    m_cursorPos = 0;
                }
            }
            else if (currOption == PlayerOptions.Item)
            {
                if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)
                {
                    switch (m_cursorPos)
                    {
                        case 0:
                            // Use Health Pot
                            if (m_player.HealthPotion > 0 && m_player.HitPoints < m_player.MaxHP)
                            {
                                m_player.HitPoints += m_player.HealthRecovery;
                                if (m_player.HitPoints > m_player.MaxHP) { m_player.HitPoints = m_player.MaxHP; }
                                m_player.HealthPotion--;
                                m_battNotification = "You used a health potion, you regained hit points.";
                                textRefresh(textSpeed);
                                m_currBattStage = BattleStage.EnemyTurn;
                            }
                            else
                            {
                                if (m_player.HitPoints >= m_player.MaxHP)
                                {
                                    m_battNotification = "You're full on health";
                                    textRefresh(textSpeed);
                                }
                                else
                                {
                                    m_battNotification = "You're all out of health potions";
                                    textRefresh(textSpeed);
                                }
                            }
                            
                            break;
                        case 1:
                            // Use Magick Pot
                            if (m_player.MagickPotion > 0 && m_player.MagickPoints < m_player.MaxMP)
                            {
                                m_player.MagickPoints += m_player.MagickRecovery;
                                if (m_player.MagickPoints > m_player.MaxMP) { m_player.MagickPoints = m_player.MaxMP; }
                                m_player.MagickPotion--;
                                m_battNotification = "You used a magick potion, you regained magick points.";
                                textRefresh(textSpeed);
                                m_currBattStage = BattleStage.EnemyTurn;
                            }
                            else
                            {
                                if (m_player.MagickPoints >= m_player.MaxMP)
                                {
                                    m_battNotification = "You're full on Magick";
                                    textRefresh(textSpeed);
                                }
                                else
                                {
                                    m_battNotification = "You're all out of magick potions";
                                    textRefresh(textSpeed);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    m_cursorPos = 0;
                }
                else if (padCurr.Buttons.B == ButtonState.Pressed && padOld.Buttons.B == ButtonState.Released)
                {
                    currOption = PlayerOptions.Null;
                    m_battNotification = "Choose an action...";
                    textRefresh(textSpeed);
                    m_cursorPos = 0;
                }
            }
            #endregion
        }

        public void enemyTurn(GamePadState padCurr, GamePadState padOld)
        {
            switch (m_enemyStage)
            {
                case 0:
                    if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)
                    {
                        if (m_currEnemy.Charmed)
                        {
                            if(Game1.RNG.Next(1,11) <= 7)
                            {
                                m_battNotification = "Enemy is charmed and refuses to attack";
                                textRefresh(textSpeed);
                                m_enemyStage += 2;
                                //m_currBattStage = BattleStage.PostRound;
                                //m_enemyStage = 0;
                                //currOption = PlayerOptions.Null;
                            }
                            else
                            {
                                m_currEnemy.enemyTurn();
                                m_battNotification = "Enemy regained their wits and attacked. " + m_currEnemy.Notification;
                                textRefresh(textSpeed);
                                m_currEnemy.Charmed = false;
                                m_enemyStage++;
                            }
                        }
                        else
                        {
                            m_currEnemy.enemyTurn();
                            m_battNotification = m_currEnemy.Notification;
                            textRefresh(textSpeed);
                            m_enemyStage++;
                        }
                    }
                    return;
                case 1:
                    if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)
                    {
                        if (m_player.GetCompanion == Companion.TheWarrior)
                        {
                            int tmpDmg = (m_currEnemy.Damage / m_player.Defence) - m_shieldWallValue;               //
                            if (tmpDmg < 0) { tmpDmg = 0; }                                                         //
                            m_player.HitPoints -= tmpDmg;                                                           // Deals damage according to selected attack which is then divided by players defence score - shield wall
                            m_battNotification = "Hewrans shield wall absorbed some damage, You took " + tmpDmg + " damage!";
                            if (m_currEnemy.MagickDamage != 0)
                            {
                                m_player.MagickPoints -= m_currEnemy.MagickDamage;
                                if (m_player.MagickPoints < 0) { m_player.MagickPoints = 0; }
                                m_currEnemy.MagickDamage = 0;
                                m_battNotification = m_battNotification + " Your magick was drained.";
                            }
                            textRefresh(textSpeed);
                            m_enemyStage++;
                        }
                        else
                        {
                            m_player.HitPoints -= m_currEnemy.Damage / m_player.Defence;                                  // Deals damage according to selected attack which is then divided by players defence score
                            m_battNotification = "You took " + m_currEnemy.Damage / m_player.Defence + " damage!";
                            if (m_currEnemy.MagickDamage != 0)
                            {
                                m_player.MagickPoints -= m_currEnemy.MagickDamage;
                                if (m_player.MagickPoints < 0) { m_player.MagickPoints = 0; }
                                m_currEnemy.MagickDamage = 0;
                                m_battNotification = m_battNotification + " Your magick was drained.";
                            }
                            textRefresh(textSpeed);
                            m_enemyStage++;
                            if (m_player.GetCompanion == Companion.TheGunslinger) { m_retribution = true; }
                        }
                    }
                    return;
                case 2:
                    if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)            //     ** this is where the empty click comes from
                    {                                                                                                    //
                        m_currBattStage = BattleStage.PostRound;                                                         //         ** could put burn in here
                        m_enemyStage = 0;                                                                                //
                        currOption = PlayerOptions.Null;                                                                 //
                    }                                                                                                    //
                    return;
            }
        }

        public void postRound(GamePadState padCurr, GamePadState padOld)
        {
            if (currProg == RoundProg.Zero)
            {
                if (m_currEnemy.Flamed && m_retribution)
                {
                    m_currEnemy.HitPoints -= 15;
                    m_battNotification = "FLAMING RETRIBUTION!! Juan takes a Shot as the enemy is burned by the flames. A brutal Combo!";
                    textRefresh(textSpeed);
                    m_currEnemy.Flamed = false;
                    m_retribution = false;
                    currProg = RoundProg.s1;
                }
                else if (m_currEnemy.Flamed)
                {
                    m_currEnemy.HitPoints -= 10;
                    m_battNotification = "Enemy is burned by the flames!";
                    textRefresh(textSpeed);
                    m_currEnemy.Flamed = false;
                    currProg = RoundProg.s1;
                }
                else if (m_retribution)
                {
                    m_currEnemy.HitPoints -= 5;
                    m_battNotification = "RETRIBUTION!! Juan takes a shot at the enemy!";
                    textRefresh(textSpeed);
                    m_retribution = false;
                    currProg = RoundProg.s1;
                }
                else
                {
                    m_battNotification = "Here we go again";
                    textRefresh(textSpeed);
                    m_currBattStage = BattleStage.PreRound;
                }
            }
            else if (currProg == RoundProg.s1)
            {
                if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)
                {
                    m_battNotification = "Here we go again";
                    textRefresh(textSpeed);
                    m_currBattStage = BattleStage.PreRound;
                    currProg = RoundProg.Zero;
                }
            }
        }

        public void drawMe(SpriteBatch sb, Texture2D cursor, Texture2D cover, GameTime gT, SoundEffect blip)
        {
            //sb.DrawString(Game1.debugFont, "Player HP: " + m_player.HitPoints + " Enemy HP : " + m_enemyHitPoints + " " + debugString, new Vector2(200, 200),Color.Black);

            #region DRAWING THE BATTLE HUD
            tMan.DrawMe(sb, gT, new Vector2(100, positions[0].Y), Color.White, textSpeed, blip);
            //sb.DrawString(Game1.uiFontOne, m_battNotification, new Vector2(100, positions[0].Y), Color.White);
            sb.Draw(cursor, positions[m_cursorPos], Color.White);

            if (currOption == PlayerOptions.Null)
            {
                sb.DrawString(Game1.uiFontOne, "Fight", new Vector2(positions[0].X + 60, positions[0].Y), Color.White);
                sb.DrawString(Game1.uiFontOne, "Item", new Vector2(positions[1].X + 60, positions[1].Y), Color.White);
                sb.DrawString(Game1.uiFontOne, "Flee", new Vector2(positions[2].X + 60, positions[2].Y), Color.White);
            }
            else if (currOption == PlayerOptions.Fight)
            {
                sb.DrawString(Game1.uiFontOne, "Melee", new Vector2(positions[0].X + 60, positions[0].Y), Color.White);
                sb.DrawString(Game1.uiFontOne, "Magick", new Vector2(positions[1].X + 60, positions[1].Y), Color.White);
                if (m_player.SpiritOrbs > 0)
                {
                    sb.DrawString(Game1.uiFontOne, "Hero \n Strike (" + m_player.SpiritOrbs + ")", new Vector2(positions[2].X + 60, positions[2].Y), Color.White);
                }
                else { sb.DrawString(Game1.uiFontOne, "Hero \n Strike (" + m_player.SpiritOrbs + ")", new Vector2(positions[2].X + 60, positions[2].Y), Color.LightGray); }
                if (m_player.GetCompanion == Companion.Alone) 
                {
                    sb.DrawString(Game1.uiFontOne, "Companion \n Strike", new Vector2(positions[3].X + 60, positions[3].Y), Color.LightGray);
                }
                else 
                {
                    switch (m_player.GetCompanion) 
                    {
                        case Companion.TheGunslinger:
                            sb.DrawString(Game1.uiFontOne, "Sniper \n Shot", new Vector2(positions[3].X + 60, positions[3].Y), Color.White);
                            break;
                        case Companion.TheWarrior:
                            sb.DrawString(Game1.uiFontOne, "Magick \n Axe", new Vector2(positions[3].X + 60, positions[3].Y), Color.White);
                            break;
                        case Companion.TheWitchdoctor:
                            sb.DrawString(Game1.uiFontOne, "Battle \n Heal", new Vector2(positions[3].X + 60, positions[3].Y), Color.White);
                            break;
                    }
                }
            }
            else if (currOption == PlayerOptions.Magick)
            {
                if (m_player.GetSpellKnown(0)) { sb.DrawString(Game1.uiFontOne, "Magick \n Wave", new Vector2(positions[0].X + 60, positions[0].Y), Color.White); }
                if (m_player.GetSpellKnown(1)) { sb.DrawString(Game1.uiFontOne, "Barrier", new Vector2(positions[1].X + 60, positions[1].Y), Color.White); }
                if (m_player.GetSpellKnown(2)) 
                { 
                    sb.DrawString(Game1.uiFontOne, "Special \n Dance", new Vector2(positions[2].X + 60, positions[2].Y), Color.White); 
                }
                else { sb.DrawString(Game1.uiFontOne, "???", new Vector2(positions[2].X + 60, positions[2].Y), Color.LightGray); }
                if (m_player.GetSpellKnown(3)) 
                { 
                    sb.DrawString(Game1.uiFontOne, "Wish", new Vector2(positions[3].X + 60, positions[3].Y), Color.White); 
                }
                else { sb.DrawString(Game1.uiFontOne, "???", new Vector2(positions[3].X + 60, positions[3].Y), Color.LightGray); }
                if (m_player.GetSpellKnown(4)) 
                { 
                    sb.DrawString(Game1.uiFontOne, "Rock 'n' Stone", new Vector2(positions[4].X + 60, positions[4].Y), Color.White); 
                }
                else { sb.DrawString(Game1.uiFontOne, "???", new Vector2(positions[4].X + 60, positions[4].Y), Color.LightGray); }
                if (m_player.GetSpellKnown(5)) 
                { 
                    sb.DrawString(Game1.uiFontOne, "Firewall", new Vector2(positions[5].X + 60, positions[5].Y), Color.White); 
                }
                else { sb.DrawString(Game1.uiFontOne, "???", new Vector2(positions[5].X + 60, positions[5].Y), Color.LightGray); }
            }
            else if(currOption == PlayerOptions.Item)
            {
                if (m_player.HealthPotion > 0)
                {
                    sb.DrawString(Game1.uiFontOne, "Health \nPotion (" + m_player.HealthPotion + ")", new Vector2(positions[0].X + 60, positions[0].Y), Color.White);
                }
                else { sb.DrawString(Game1.uiFontOne, "Health \nPotion (" + m_player.HealthPotion + ")", new Vector2(positions[0].X + 60, positions[0].Y), Color.LightGray); }

                if (m_player.MagickPotion > 0)
                {
                    sb.DrawString(Game1.uiFontOne, "Magick \n Potion (" + m_player.MagickPotion + ")", new Vector2(positions[1].X + 60, positions[1].Y), Color.White);
                }
                else { sb.DrawString(Game1.uiFontOne, "Magick \n Potion (" + m_player.MagickPotion + ")", new Vector2(positions[1].X + 60, positions[1].Y), Color.LightGray); }
            }

            if (m_currBattStage != BattleStage.PlayerTurn)
            {
                sb.Draw(cover, Vector2.Zero, Color.White);
            }
            #endregion

            // HP BARS
            m_player.drawMe(sb);
            m_currEnemy.drawMe(sb, gT);
        }
    }
}
