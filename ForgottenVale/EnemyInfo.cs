using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForgottenVale
{
    // Base Enemy Class
    class EnemyBase
    {
        // Class Variables
        protected int m_enemyHitPoints, m_fleeChance;
        protected Texture2D m_enemySpriteSheet, m_healthTex;
        protected string m_enemyName, m_notification;

        protected int m_defence, m_resilience;
        protected bool m_isCharmed, m_isFlamed;

        protected int m_damage, m_magickDrain;
        protected string m_secondary;

        protected Vector2 m_posCenter, m_origin;

        protected int m_barUnitWidth;

        public int HitPoints
        {
            get
            {
                return m_enemyHitPoints;
            }
            set
            {
                m_enemyHitPoints = value;
            }
        }
        public bool Charmed
        {
            get
            {
                return m_isCharmed;
            }
            set
            {
                m_isCharmed = value;
            }
        }
        public bool Flamed
        {
            get
            {
                return m_isFlamed;
            }
            set
            {
                m_isFlamed = value;
            }
        }
        public int Damage
        {
            get
            {
                return m_damage;
            }
        }
        public int FleeChance
        {
            get
            {
                return m_fleeChance;
            }
        }
        public int MagickDamage
        {
            get
            {
                return m_magickDrain;
            }
            set
            {
                m_magickDrain = value;
            }
        }
        public int Resilience
        {
            get
            {
                return m_resilience;
            }
        }
        public string Name
        {
            get
            {
                return m_enemyName;
            }
        }
        public string Notification
        {
            get
            {
                return m_notification;
            }
        }

        public EnemyBase(Texture2D spritesheet, Texture2D healthTex)
        {
            m_enemyHitPoints = 100;
            m_fleeChance = 8;
            m_enemySpriteSheet = spritesheet;
            m_healthTex = healthTex;
            m_enemyName = "baseEnemy";

            m_defence = 0;
            m_resilience = 0;
            m_isCharmed = false;
            m_isFlamed = false;

            m_damage = 0;
            m_magickDrain = 0;
            m_secondary = "null";

            m_posCenter = new Vector2(1520, 250);
            m_origin = new Vector2(spritesheet.Width / 2, spritesheet.Height / 2);

            m_barUnitWidth = 10;
        }

        public virtual void enemyTurn()
        {

        }

        public virtual void drawMe(SpriteBatch sb, GameTime gt)
        {
            for (int i = 1; i <= m_enemyHitPoints; i++)
            {
                sb.Draw(m_healthTex, new Rectangle((13 + (i * m_barUnitWidth))-m_barUnitWidth, 24, m_barUnitWidth, 35), Color.Green);
            }
        }
    }

    // ELEMENTAL ENEMY
    class Elemental : EnemyBase
    {
        // Class Variables
        private string m_currEnemy;
        private Texture2D m_outerRingTex;
        private float m_rot;

        public Elemental(Texture2D spritesheet, Texture2D tex2, Texture2D healthTex, string enemyType) : base (spritesheet, healthTex)
        {
            m_currEnemy = enemyType;
            m_outerRingTex = tex2;
            m_rot = 0;

            switch (m_currEnemy)
            {
                case "Earth":
                    m_enemyHitPoints = 32;
                    m_barUnitWidth = 30;
                    m_fleeChance = 7;
                    m_resilience = 2;
                    break;

                case "Ice":
                    m_enemyHitPoints = 48;
                    m_barUnitWidth = 20;
                    m_fleeChance = 5;
                    m_resilience = 4;
                    break;

                case "Fire":
                    m_enemyHitPoints = 64;
                    m_barUnitWidth = 15;
                    m_fleeChance = 3;
                    m_resilience = 6;
                    break;
            }
        }

        public void bind()
        {
            // include consideration for player agility 

            int randNum = Game1.RNG.Next(1, 4);
            if (randNum == 2) 
            { 
                m_secondary = "bind";

                if (m_currEnemy == "Earth")
                {
                    m_notification = m_notification + "The player was bound! ";
                }
                else if (m_currEnemy == "Ice")
                {
                    m_notification = m_notification + "It's super effective ";
                    m_damage = m_damage + 10;
                }
            }
        }

        public override void enemyTurn()
        {
            int randNum = Game1.RNG.Next(1, 5);

            switch (m_currEnemy)
            {
                case "Earth":

                    switch (randNum)
                    {
                        case 1:
                            m_notification = "Earth Elemental used Pebble ";
                            m_damage = 10;
                            break;
                        case 2:
                            m_notification = "Earth Elemental used Rock. ";
                            m_damage = 20;
                            break;
                        case 3:
                            m_notification = "Earth Elemental used Quake ";
                            m_damage = 25;
                            break;
                        default:
                            m_notification = "Earth Elemental used Melee. ";
                            m_damage = 15;
                            break;
                    }
                    
                    break;

                case "Ice":

                    switch (randNum)
                    {
                        case 1:
                            m_notification = "Ice Elemental used Weak Frost ";
                            m_damage = 15;
                            break;
                        case 2:
                            m_notification = "Ice Elemental used IceBlast ";
                            m_damage = 30;
                            break;
                        case 3:
                            m_notification = "Ice Elemental used Chill Winds. ";
                            m_damage = 25;
                            bind();
                            break;
                        default:
                            m_notification = "Ice Elemental used Frost ";
                            m_damage = 20;
                            break;
                    }
                    
                    break;

                case "Fire":

                    switch (randNum)
                    {
                        case 1:
                            m_notification = "Fire Elemental used Scorch. ";
                            m_damage = 25;
                            break;
                        case 2:
                            m_notification = "Fire Elemental used Nova. ";
                            m_damage = 35;
                            break;
                        case 3:
                            m_notification = "Fire Elemental used Nova. ";
                            m_damage = 35;
                            break;
                        default:
                            m_notification = "Fire Elemental used Scorch. ";
                            m_damage = 25;
                            break;
                    }
                    
                    break;
            }

            //base.enemyTurn();
        }

        public override void drawMe(SpriteBatch sb, GameTime gt)
        {
            m_rot += 0.01f;
            // Draw enemy
            sb.Draw(m_outerRingTex, m_posCenter, null, Color.White, m_rot, m_origin, 1, SpriteEffects.None, 0);
            sb.Draw(m_enemySpriteSheet, m_posCenter, null, Color.White, 0, m_origin, 1, SpriteEffects.None, 0);

            // healthbar drawn in base class
            base.drawMe(sb, gt); 
        }
    }

    // MIMIC ENEMY
    class Mimic : EnemyBase
    {
        // Class Variables
        float m_updateTrigger, m_framesPerSecond;
        Rectangle m_srcRect;

        public Mimic(Texture2D spritesheet, Texture2D healthTex) : base(spritesheet, healthTex)
        {
            m_enemyHitPoints = 64;
            m_barUnitWidth = 15;
            m_fleeChance = 2;
            m_resilience = 5;

            m_updateTrigger = 0;
            m_framesPerSecond = 3;
            m_srcRect = new Rectangle(0, 0, spritesheet.Width / 3, spritesheet.Height);
        }

        public override void enemyTurn()
        {
            int randNum = Game1.RNG.Next(1, 5);
            switch (randNum)
            {
                case 1:
                    m_notification = "Mimic used Grand Slam. ";
                    m_damage = 25;
                    break;
                case 2:
                    m_notification = "Mimic used Magick Drain. ";
                    m_damage = 10;
                    m_magickDrain = 15;
                    break;
                default:
                    m_notification = "Mimic used Slam. ";
                    m_damage = 20;
                    break;
            }

            //base.enemyTurn();
        }

        public override void drawMe(SpriteBatch sb, GameTime gt)
        {

            m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

            if (m_updateTrigger >= 1)
            {
                m_updateTrigger = 0;
                m_srcRect.X += m_srcRect.Width;
                if (m_srcRect.X == m_enemySpriteSheet.Width)
                    m_srcRect.X = m_srcRect.Width;
            }

            sb.Draw(m_enemySpriteSheet, m_posCenter, m_srcRect, Color.White, 0, m_origin, 3, SpriteEffects.None, 0);

            // healthbar drawn in base class
            base.drawMe(sb, gt);
        }
    }

    // Thorn ENEMY
    class Thorn : EnemyBase
    {
        // Class Variables
        float m_updateTrigger, m_framesPerSecond;
        Rectangle m_srcRect;

        public Thorn(Texture2D spritesheet, Texture2D healthTex) : base(spritesheet, healthTex)
        {
            m_enemyHitPoints = 48;
            m_barUnitWidth = 20;
            m_fleeChance = 2;
            m_resilience = 3;

            m_updateTrigger = 0;
            m_framesPerSecond = 3;
            m_srcRect = new Rectangle(0, 0, spritesheet.Width / 3, spritesheet.Height);
        }

        public override void enemyTurn()
        {
            int randNum = Game1.RNG.Next(1, 5);
            switch (randNum)
            {
                case 1:
                    m_notification = "Thorn used Snare. ";
                    m_damage = 25;
                    break;
                case 2:
                    m_notification = "Thorn used Berry and regained HP. ";
                    m_damage = 0;
                    m_enemyHitPoints += 10;
                    if (m_enemyHitPoints > 48) { m_enemyHitPoints = 48; }
                    break;
                default:
                    m_notification = "Thorn used Thrash. ";
                    m_damage = 15;
                    break;
            }

            //base.enemyTurn();
        }

        public override void drawMe(SpriteBatch sb, GameTime gt)
        {

            m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

            if (m_updateTrigger >= 1)
            {
                m_updateTrigger = 0;
                m_srcRect.X += m_srcRect.Width;
                if (m_srcRect.X == m_enemySpriteSheet.Width)
                    m_srcRect.X = m_srcRect.Width;
            }

            sb.Draw(m_enemySpriteSheet, m_posCenter, m_srcRect, Color.White, 0, m_origin, 2, SpriteEffects.None, 0);

            // healthbar drawn in base class
            base.drawMe(sb, gt);
        }
    }

    // Drowzy Bush
    class DrowzyBush : EnemyBase
    {
        // Class Variables
        float m_updateTrigger, m_framesPerSecond;
        Rectangle m_srcRect;

        public DrowzyBush(Texture2D spritesheet, Texture2D healthTex) : base(spritesheet, healthTex)
        {
            m_enemyHitPoints = 64;
            m_barUnitWidth = 15;
            m_fleeChance = 5;
            m_resilience = 3;

            m_updateTrigger = 0;
            m_framesPerSecond = 3;
            m_srcRect = new Rectangle(0, 0, spritesheet.Width / 3, spritesheet.Height);
        }

        public override void enemyTurn()
        {
            m_notification = "Drowzy Bush is sleepy and doesn't attack.";
            m_damage = 0;
            //base.enemyTurn();
        }

        public override void drawMe(SpriteBatch sb, GameTime gt)
        {

            m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

            if (m_updateTrigger >= 1)
            {
                m_updateTrigger = 0;
                m_srcRect.X += m_srcRect.Width;
                if (m_srcRect.X == m_enemySpriteSheet.Width)
                    m_srcRect.X = m_srcRect.Width;
            }

            sb.Draw(m_enemySpriteSheet, m_posCenter, m_srcRect, Color.White, 0, m_origin, 2, SpriteEffects.None, 0);

            // healthbar drawn in base class
            base.drawMe(sb, gt);
        }
    }

    // Forn Boss
    class Forn : EnemyBase
    {
        // Class Variables
        float m_updateTrigger, m_framesPerSecond;
        Rectangle m_srcRect;

        public Forn(Texture2D spritesheet, Texture2D healthTex) : base(spritesheet, healthTex)
        {
            m_enemyHitPoints = 96;
            m_barUnitWidth = 10;
            m_fleeChance = 1;           // cant flee
            m_resilience = 0;           // cant resist special dance
            m_enemyName = "Forn";

            m_updateTrigger = 0;
            m_framesPerSecond = 3;
            m_srcRect = new Rectangle(0, 0, spritesheet.Width / 7, spritesheet.Height); // **
        }

        public override void enemyTurn()
        {
            int randNum = Game1.RNG.Next(1, 5);
            switch (randNum)
            {
                case 1:
                    m_notification = "HAARRRROOMM! Forn used Rock 'n' Stone. ";
                    m_damage = 35;
                    break;
                case 2:
                    m_notification = "Forn used Absorb and sapped some of your health";
                    m_damage = 25;
                    m_enemyHitPoints += 20;
                    if (m_enemyHitPoints > 96) { m_enemyHitPoints = 96; }
                    break;
                default:
                    m_notification = "Forn used Furious Rage! ";
                    m_damage = 20;
                    break;
            }

            //base.enemyTurn();
        }

        public override void drawMe(SpriteBatch sb, GameTime gt)
        {

            m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

            if (m_updateTrigger >= 1)
            {
                m_updateTrigger = 0;
                m_srcRect.X += m_srcRect.Width;
                if (m_srcRect.X == m_enemySpriteSheet.Width)
                    m_srcRect.X = m_enemySpriteSheet.Width - (m_srcRect.Width * 2);
            }

            sb.Draw(m_enemySpriteSheet, m_posCenter, m_srcRect, Color.White, 0, new Vector2(m_srcRect.Width/2, m_srcRect.Height/2), 2, SpriteEffects.None, 0);

            // healthbar drawn in base class
            base.drawMe(sb, gt);
        }
    }

    // Simmons Boss
    class Simmons : EnemyBase
    {
        // Class Variables
        float m_updateTrigger, m_framesPerSecond;
        Rectangle m_srcRect;

        public Simmons(Texture2D spritesheet, Texture2D healthTex) : base(spritesheet, healthTex)
        {
            m_enemyHitPoints = 96;
            m_barUnitWidth = 10;
            m_fleeChance = 1;           // cant flee
            m_resilience = 6;
            m_enemyName = "Simmons";

            m_updateTrigger = 0;
            m_framesPerSecond = 3;
            m_srcRect = new Rectangle(0, 0, spritesheet.Width / 4, spritesheet.Height); // **
        }

        public override void enemyTurn()
        {
            int randNum = Game1.RNG.Next(1, 5);
            switch (randNum)
            {
                case 1:
                    m_notification = "Simmons used Heavy Metal! CLANK!";
                    m_damage = 40;
                    break;
                case 2:
                    m_notification = "Simmons used Holy Magick!";
                    m_damage = 35;
                    m_magickDrain = 20;
                    break;
                default:
                    m_notification = "Simmons used Lick! ";
                    m_damage = 20;
                    break;
            }

            //base.enemyTurn();
        }

        public override void drawMe(SpriteBatch sb, GameTime gt)
        {

            m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

            if (m_updateTrigger >= 1)
            {
                m_updateTrigger = 0;
                m_srcRect.X += m_srcRect.Width;
                if (m_srcRect.X == m_enemySpriteSheet.Width)
                    m_srcRect.X = m_enemySpriteSheet.Width - (m_srcRect.Width * 2);
            }

            sb.Draw(m_enemySpriteSheet, m_posCenter, m_srcRect, Color.White, 0, new Vector2(m_srcRect.Width / 2, m_srcRect.Height / 2), 2, SpriteEffects.None, 0);

            // healthbar drawn in base class
            base.drawMe(sb, gt);
        }
    }

    // Landru Boss
    class Landru6000 : EnemyBase
    {
        // Class Variables
        float m_updateTrigger, m_framesPerSecond;
        Rectangle m_srcRect;

        public Landru6000(Texture2D spritesheet, Texture2D healthTex) : base(spritesheet, healthTex)
        {
            m_enemyHitPoints = 192;
            m_barUnitWidth = 5;
            m_fleeChance = 1;           // cant flee
            m_resilience = 7;
            m_enemyName = "Landru_6000";

            m_updateTrigger = 0;
            m_framesPerSecond = 6;
            m_srcRect = new Rectangle(0, 0, spritesheet.Width / 2, spritesheet.Height); // **
        }

        public override void enemyTurn()
        {
            int randNum = Game1.RNG.Next(1, 5);
            switch (randNum)
            {
                case 1:
                    m_notification = "Landru_6000 used antivirus!";
                    m_damage = 35;
                    break;
                case 2:
                    m_notification = "Landru_6000 used EXTERMINATE!";
                    m_damage = 25;
                    m_magickDrain = 10;
                    break;
                default:
                    m_notification = "Landru_6000 used 0001 1101 0100! ";
                    m_damage = 20;
                    break;
            }

            //base.enemyTurn();
        }

        public override void drawMe(SpriteBatch sb, GameTime gt)
        {

            m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

            if (m_updateTrigger >= 1)
            {
                m_updateTrigger = 0;
                m_srcRect.X += m_srcRect.Width;
                if (m_srcRect.X == m_enemySpriteSheet.Width)
                    m_srcRect.X = 0;
            }

            sb.Draw(m_enemySpriteSheet, m_posCenter, m_srcRect, Color.White, 0, new Vector2(m_srcRect.Width / 2, m_srcRect.Height / 2), 4, SpriteEffects.None, 0);

            // healthbar drawn in base class
            base.drawMe(sb, gt);
        }
    }
}

