using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForgottenVale
{
    enum Direction
    {
        South,
        West,
        North,
        East
    }

    class PlayerClass : Animated2D
    {
        //class variables

        private Direction m_facing;
        private Vector2 m_destPos;
        private bool m_isMoving;
        private int m_encounterChance;
        private float m_speed;

        private string m_talkTo;
        private int m_targetChest, m_targetEnemy;

        public int m_safeSteps; // make private after testing

        public Vector2 Destination
        {
            get
            {
                return m_destPos;
            }
        }

        public Direction Facing
        {
            get
            {
                return m_facing;
            }
        }

        public float Speed
        {
            get
            {
                return m_speed;
            }
            set
            {
                m_speed = value;
            }
        }

        public int EncounterChance
        {
            get
            {
                return m_encounterChance;
            }
            set
            {
                m_encounterChance = value;
            }
        } 

        public string TalkTo
        {
            get
            {
                return m_talkTo;
            }
            set
            {
                m_talkTo = value;
            }
        }
        public int TargetChest
        {
            get
            {
                return m_targetChest;
            }
            set
            {
                m_targetChest = value;
            }
        }
        public int TargetEnemy
        {
            get
            {
                return m_targetEnemy;
            }
            set
            {
                m_targetEnemy = value;
            }
        }

        public PlayerClass(Texture2D spriteSheet, Vector2 position, int rows, int cols, int fps) : base(spriteSheet, position, rows, cols, fps)
        {
            m_facing = Direction.South;
            m_destPos = m_pos;
            m_isMoving = false;
            m_safeSteps = 0;

            m_speed = 0.06f;
            m_talkTo = "unknown";
            m_targetChest = -1;
            m_targetEnemy = -1;
        }

        public void moveme(Direction moveDir)
        {
            m_facing = moveDir;

            switch (moveDir)
            {
                case Direction.North:
                    m_destPos.Y--;
                    break;
                case Direction.South:
                    m_destPos.Y++;
                    break;
                case Direction.East:
                    m_destPos.X++;
                    break;
                case Direction.West:
                    m_destPos.X--;
                    break;
            }

            if (m_safeSteps> 0)     // reduce safe steps by one every move if above zero
            {
                m_safeSteps--;
            }
        }

        public void checkForEncounter(TileClass tile)
        {
            if (tile.IsWilderness && m_safeSteps <= 0)
            {
                m_encounterChance = Game1.RNG.Next(1, 36);  // randomly assign a value to generate chance of battle ** value of 5 triggers battle ** 

            }
            else { m_encounterChance = 0; }
        }

        public void checkForInteraction(TileClass tile)
        {
            if (tile.NPC_Name != "unknown")
            {
                m_talkTo = tile.NPC_Name;
                m_targetEnemy = tile.EnemyID;
            }
            else if (tile.ChestID != -1)
            {
                m_targetChest = tile.ChestID;
            }
        }

        public void updateme(GameTime gameTime, TileClass[,] nGrid, GamePadState padCurr, GamePadState padOld)
        {
            if (m_pos == m_destPos)
            {
                m_isMoving = false;
            }
            else { m_isMoving = true; }


            if (!m_isMoving)
            {

                if (padCurr.DPad.Up == ButtonState.Pressed)                                          // move north from stationary
                {
                    if (nGrid[(int)m_pos.X, (int)m_pos.Y - 1].IsWalkable)
                    {
                        moveme(Direction.North);
                        checkForEncounter(nGrid[(int)m_pos.X, (int)m_pos.Y - 1]);
                    }
                    else { m_facing = Direction.North; }
                }
                else if (padCurr.DPad.Down == ButtonState.Pressed)                                   // move south from stationary
                {
                    if (nGrid[(int)m_pos.X, (int)m_pos.Y + 1].IsWalkable)
                    {
                        moveme(Direction.South);
                        checkForEncounter(nGrid[(int)m_pos.X, (int)m_pos.Y + 1]);
                    }
                    else { m_facing = Direction.South; }
                }
                else if (padCurr.DPad.Left == ButtonState.Pressed)                                   // move west from stationary
                {
                    if (nGrid[(int)m_pos.X - 1, (int)m_pos.Y].IsWalkable)
                    {
                        moveme(Direction.West);
                        checkForEncounter(nGrid[(int)m_pos.X - 1, (int)m_pos.Y]);
                    }
                    else { m_facing = Direction.West; }
                }
                else if (padCurr.DPad.Right == ButtonState.Pressed)                                  // move east from stationary
                {
                    if (nGrid[(int)m_pos.X + 1, (int)m_pos.Y].IsWalkable)
                    {
                        moveme(Direction.East);
                        checkForEncounter(nGrid[(int)m_pos.X + 1, (int)m_pos.Y]);
                    }
                    else { m_facing = Direction.East; }
                }
            }
            else
            {
                if (padCurr.DPad.Up == ButtonState.Pressed)
                {
                    if ((m_pos.Y < m_destPos.Y + 0.1f && m_facing == Direction.North) || m_facing == Direction.South)            // continue north while traveling north (to avoid pause) or switch from south to north instantly
                    {
                        if (nGrid[(int)m_destPos.X, (int)m_destPos.Y - 1].IsWalkable)
                        {
                            checkForEncounter(nGrid[(int)m_destPos.X, (int)m_destPos.Y - 1]);
                            moveme(Direction.North);
                        }
                            
                    }
                }
                else if (padCurr.DPad.Down == ButtonState.Pressed)
                {
                    if ((m_pos.Y > m_destPos.Y - 0.1f && m_facing == Direction.South) || m_facing == Direction.North)            // continue south while traveling south (to avoid pause) or switch from north to south instantly
                    {
                        if (nGrid[(int)m_destPos.X, (int)m_destPos.Y + 1].IsWalkable) 
                        {
                            checkForEncounter(nGrid[(int)m_destPos.X, (int)m_destPos.Y + 1]);
                            moveme(Direction.South);
                        }  
                    }
                }
                else if (padCurr.DPad.Left == ButtonState.Pressed)
                {
                    if ((m_pos.X < m_destPos.X + 0.1f && m_facing == Direction.West) || m_facing == Direction.East)               // continue west while traveling west (to avoid pause) or switch from east to west instantly
                    {
                        if (nGrid[(int)m_destPos.X - 1, (int)m_destPos.Y].IsWalkable)
                        {
                            checkForEncounter(nGrid[(int)m_destPos.X - 1, (int)m_destPos.Y]);
                            moveme(Direction.West);
                        } 
                    }
                }
                else if (padCurr.DPad.Right == ButtonState.Pressed)
                {
                    if ((m_pos.X > m_destPos.X - 0.1f && m_facing == Direction.East) || m_facing == Direction.West)              // continue east while traveling east (to avoid pause) or switch from west to east instantly
                    {
                        if (nGrid[(int)m_destPos.X + 1, (int)m_destPos.Y].IsWalkable)
                        {
                            checkForEncounter(nGrid[(int)m_destPos.X + 1, (int)m_destPos.Y]);
                            moveme(Direction.East);
                        }
                    }
                }

                if (padCurr.Buttons.A == ButtonState.Pressed) // enable run speed
                {
                    m_speed = 0.1f;
                }
                else
                {
                    m_speed = 0.06f;
                }


                if (m_facing == Direction.East)                  // This code actually moves the player while the code above sets the players destination based on controler input
                {                                                //
                    if (m_destPos.X > m_pos.X)                    //
                    {                                            //
                        m_pos.X += m_speed;                      //
                    }                                            //
                    else { m_pos = m_destPos; }                  //
                }                                                //
                else if (m_facing == Direction.West)             //
                {                                                //
                    if (m_destPos.X < m_pos.X)                   //
                    {                                            //
                        m_pos.X -= m_speed;                      //
                    }                                            //
                    else { m_pos = m_destPos; }                  //
                }                                                //
                else if (m_facing == Direction.South)            //
                {                                                //
                    if (m_destPos.Y > m_pos.Y)                   //
                    {                                            //
                        m_pos.Y += m_speed;                      //
                    }                                            //
                    else { m_pos = m_destPos; }                  //
                }                                                //
                else if (m_facing == Direction.North)            //
                {                                                //
                    if (m_destPos.Y < m_pos.Y)                   //
                    {                                            //
                        m_pos.Y -= m_speed;                      //
                    }                                            //
                    else { m_pos = m_destPos; }                  //
                }                                                //
            }

            m_srcRect.Y = (int)m_facing * m_srcRect.Height;                          // position source rect for spritesheet animation 


            // check for Dialog/interaction 
            if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)
            {
                switch (m_facing)
                {
                    case Direction.North:
                        checkForInteraction(nGrid[(int)m_destPos.X, (int)m_destPos.Y - 1]);
                        break;
                    case Direction.South:
                        checkForInteraction(nGrid[(int)m_destPos.X, (int)m_destPos.Y + 1]);
                        break;
                    case Direction.West:
                        checkForInteraction(nGrid[(int)m_destPos.X - 1, (int)m_destPos.Y]);
                        break;
                    case Direction.East:
                        checkForInteraction(nGrid[(int)m_destPos.X + 1, (int)m_destPos.Y]);
                        break;
                }
            }
        }

        public override void drawme(SpriteBatch sBatch, GameTime gt)
        {
            if (m_isMoving)
            {
                m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * (m_framesPerSecond * (m_speed*15));
            }

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
