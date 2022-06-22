using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForgottenVale
{
    class DialogManager
    {
        // Class Variables
        private PlayerInfo m_player;
        private string m_npcName;
        private Texture2D m_npcPortrait;
        private bool m_isIdle;
        private Vector2 m_pos, m_textPos, m_portraitPos, m_op1, m_op2, m_cursorPos;
        private TextManager tMan;
        private string currDialogText;
        private float textSpeed, m_stringLength;
        private SoundEffect m_potFX, m_vendFX;
        private float potVol = 0.25f;
        private float vendVol = 0.6f;

        private Rectangle m_srcRect;
        private Point m_charPos;
        private bool m_forceLoad;

        private bool m_canProceed, m_decision, m_outcome;
        private int convoStage = 0;

        public bool IsIdle
        {
            get
            {
                return m_isIdle;
            }
            set
            {
                m_isIdle = value;
            }
        }
        public bool CanProceed
        {
            get
            {
                return m_canProceed;
            }
        }
        public bool ForceLoad
        {
            get
            {
                return m_forceLoad;
            }
            set
            {
                m_forceLoad = value;
            }
        }

        public DialogManager(PlayerInfo player, string npcName, Texture2D npcPortrait, Vector2 drawPos, SoundEffect vend, SoundEffect potion)
        {
            m_player = player;
            m_npcName = npcName;
            m_npcPortrait = npcPortrait;
            m_pos = drawPos;

            m_isIdle = true;
            m_decision = false;
            m_outcome = false;

            m_potFX = potion;
            m_vendFX = vend;

            m_forceLoad = false;

            m_stringLength = Game1.uiFontOne.MeasureString(m_npcName).X;

            textSpeed = 0.01f;
            m_textPos = new Vector2(m_pos.X + 450, m_pos.Y + 780);
            currDialogText = " ";
            textRefresh(textSpeed);

            #region ASSIGN PORTRAIT 
            switch (m_npcName)
            {
                case "Juan":
                    m_charPos = new Point(0,0);
                    break;
                case "Hewran":
                    m_charPos = new Point(220,0);
                    break;
                case "Nerwen":
                    m_charPos = new Point(440, 0);
                    break;
                case "Legendary Hero":
                    m_charPos = new Point(660, 0);
                    break;
                case "Elder Kevin":
                    m_charPos = new Point(0, 220);
                    break;
                case "Elsa":
                    m_charPos = new Point(220,220);
                    break;
                case "Broox":
                    m_charPos = new Point(440,220);
                    break;
                case "Calli":
                    m_charPos = new Point(660, 220);
                    break;
                case "Forn":
                    m_charPos = new Point(0, 440);
                    break;
                default:
                    m_charPos = new Point(660, 440);
                    break;
            }
            #endregion

            m_srcRect = new Rectangle(m_charPos, new Point(m_npcPortrait.Width / 4, m_npcPortrait.Height / 3));
            m_portraitPos = new Vector2(m_pos.X + 80, m_pos.Y + 760);

            m_op1 = new Vector2(m_pos.X + 1650, m_pos.Y + 780);
            m_op2 = new Vector2(m_pos.X + 1650, m_pos.Y + 880);
            m_cursorPos = m_op1;
        }

        public void textRefresh(float tSpeed)
        {
            // find the dialog line
            switch (m_npcName)
            {
                case "Juan":
                    JuanDia();
                    break;
                case "Hewran":
                    HewranDia();
                    break;
                case "Nerwen":
                    NerwenDia();
                    break;
                case "Legendary Hero":
                    LegendaryHeroDia();
                    break;
                case "Elder Kevin":
                    ElderKevinDia();
                    break;
                case "Elsa":
                    ElsaDia();
                    break;
                case "Broox":
                    BrooxDia();
                    break;
                case "Calli":
                    CalliDia();
                    break;
                case "Thorn":
                    ThornDia();
                    break;
                case "Mimic":
                    MimicDia();
                    break;
                case "Forn":
                    FornDia();
                    break;
                case "Simmons":
                    SimmonsDia();
                    break;
                case "hVend":
                    hVendDia();
                    break;
                case "mVend":
                    mVendDia();
                    break;
                case "DrowzyBush":
                    DrowzyBushDia();
                    break;
                case "    Cauldron'o'Health":
                    caulPotDia();
                    break;
                case "    Cauldron'o'Magick":
                    caulPot2Dia();
                    break;
                case "Landru_6000":
                    Landru6000Dia();
                    break;
            }
            //print the line
            tMan = new TextManager(currDialogText, Game1.uiFontOne, 1000);
        }

        public string SeerCommunion(int option)
        {
            string tmpSting = " ";

            switch (option)
            {
                case 1:
                    tmpSting = "Greed is Eternal... Always be drinking potions!";
                    break;
                case 2:
                    tmpSting = "Time and tide wait for no man... grab opportunities when you can, lest they fade with the seasons change.";
                    break;
                case 3:
                    tmpSting = "When in doubt... Shake it! Even the greatest RAGE can be passified with the power of dance.";
                    break;
                case 4:
                    tmpSting = "If you don't succeed, do it again but better. If you continue to falter, do it differently.";
                    break;
                case 5:
                    tmpSting = "Anything can happen when you make a WISH.";
                    break;
                case 6:
                    tmpSting = "Trust in the power of friendship... Talking is a good place to start.";
                    break;
                default:
                    tmpSting = "I sense nothing... Grave indeed!";
                    break;
            }

            return tmpSting;
        }

        public void updateMe(GamePadState padCurr, GamePadState padOld, SoundEffect movCurs)
        {
            if (m_canProceed)
            {
                if (padCurr.Buttons.A == ButtonState.Pressed && padOld.Buttons.A == ButtonState.Released)
                {
                    if (m_decision) 
                    { 
                        // get player input
                        if (m_cursorPos == m_op1)
                        {
                            m_outcome = true;
                        }
                        else
                        {
                            m_outcome = false;
                        }
                        // reset decision
                        m_decision = false; 
                    }
                    textRefresh(textSpeed);
                }
            }
            else
            {
                //textSpeed = 0.01f;
                if (padCurr.Buttons.A == ButtonState.Pressed)
                {
                    textSpeed = -0.5f;
                }
                else
                {
                    textSpeed = 0.01f;
                }
            }

            // Move Cursor
            if ((padCurr.DPad.Down == ButtonState.Pressed && padOld.DPad.Down == ButtonState.Released) || (padCurr.DPad.Up == ButtonState.Pressed && padOld.DPad.Up == ButtonState.Released)) 
            {
                if (m_cursorPos == m_op1)
                {
                    m_cursorPos = m_op2;
                }
                else { m_cursorPos = m_op1; }

                if (CanProceed && m_decision) { movCurs.Play(0.3f, 0, 0); }
            }

            // exit dialog
            if (padCurr.Buttons.B == ButtonState.Pressed && padOld.Buttons.B == ButtonState.Released && m_npcName != "Legendary Hero")
            {
                m_isIdle = true;
                if (m_player.JustDied) { m_player.JustDied = false; }
            }
        }

        #region NPC Dialog Details
        public void JuanDia()
        {
            switch (convoStage)
            {
                case 0:
                    currDialogText = "Well HOWDY there pardnah!";
                    break;
                case 1:
                    currDialogText = "I'm just fixin' for a good adventure.  How's about you let me tag along with you for a little while?";
                    m_decision = true;
                    break;
                case 2:
                    if (m_outcome)
                    {
                        currDialogText = "Great, let's hit the road. ";
                        m_player.GetCompanion = Companion.TheGunslinger;
                        m_forceLoad = true;
                    }
                    else { currDialogText = "well, if ya change your mind, y'all know where to find me."; }
                    
                    break;
                default:
                    currDialogText = " ";
                    m_isIdle = true;
                    break;
            }
            convoStage++;
        }
        public void HewranDia()
        {
            switch (convoStage)
            {
                case 0:
                    currDialogText = "Hi friend, You look like you could use a hand.";
                    break;
                case 1:
                    currDialogText = "How about i tag along with you?";
                    m_decision = true;
                    break;
                case 2:
                    if (m_outcome)
                    {
                        currDialogText = "My axe and i are at your service! May the Great Administrator favour us in battle!";
                        m_player.GetCompanion = Companion.TheWarrior;
                        m_forceLoad = true;
                    }
                    else { currDialogText = "A wanderer who wanders alone, has only a fool for company. Come and see me if you change your mind"; }

                    break;
                default:
                    currDialogText = " ";
                    m_isIdle = true;
                    break;
            }
            convoStage++;
        }
        public void NerwenDia()
        {
            if (m_player.NerwenRank == 0)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Your quest stands on the edge of a knife. ";
                        break;
                    case 1:
                        currDialogText = "Stray but a little...  and it will fall, to the ruin of all.";
                        break;
                    case 2:
                        currDialogText = "Say,  i dont suppose you've seen my staff?";
                        m_decision = true;
                        break;
                    case 3:
                        if (m_outcome)
                        {
                            if (m_player.GotStaff)
                            {
                                currDialogText = "Briliant! Thank you so much. please let me know if there is anythign i can do for you in return.";
                                m_player.NerwenRank = 1;
                            }
                            else
                            {
                                currDialogText = "Seems like your confused traveller, that doesnt look like my staff. please let me know if you find it during your travels.";
                            }
                        }
                        else { currDialogText = "damn! I lost it somewhere in the old forrest when i was camping. please let me know if you find it during your travels."; }
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
            else if(m_player.NerwenRank == 1)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Thanks again for finding my staff, that was really neat.";
                        break;
                    case 1:
                        currDialogText = "As repayment, perhaps i can join you on your quest. you'll find i can be quite helpful. what d'ya say?";
                        m_decision = true;
                        break;
                    case 2:
                        if (m_outcome)
                        {
                            currDialogText = "Perfect! Hope lives on, while the bonds of friendship remain strong.";
                            m_player.GetCompanion = Companion.TheWitchdoctor;
                            m_forceLoad = true;
                        }
                        else { currDialogText = "Well it's your funeral buddy, most folk would jump at the chance. Come see me again when you regain your senses."; }
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
           
            convoStage++;
        }
        public void LegendaryHeroDia()
        {
            if (m_player.LegHeroRank == 0)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "WooooOOOooooOOOOoooo... .. The hour is late, and at long last the Wanderer comes forth to claim the quest. WooOOooOOoo...";
                        break;
                    case 1:
                        currDialogText = "YES.. Thats right, YOU!! Do you see any other wanderers around here?";
                        m_decision = true;
                        break;
                    case 2:
                        if (m_outcome)
                        {
                            currDialogText = "*sigh* Thats just great, looks like we've got ourselves a bit of a wise guy... Listen up! This is important stuff!";
                        }
                        else { currDialogText = "Exactly, therefore you must be the wanderer spoken of in the prophecy. I'm glad you agree. Now, Listen Up!"; }
                        break;
                    case 3:
                        currDialogText = "WooooOOOooooOOOOoooo... Collect the shards.. WooooOOOooooOOOOoooo... .. . fulfill the prophecy..  WooooOOOooooOOOOoooo... .. . ";
                        break;
                    case 4:
                        currDialogText = "Walk the path to the Forgotten Vale... WoooOOoo.. .. and complete the Legendary Heros quest... WoooOOooooooOOooo... .. .";
                        break;
                    case 5:
                        currDialogText = "Alright Fine! Man, your a chatty one aren't you. i'll just tell you exactly what to do then shall i?";
                        m_decision = true;
                        break;
                    case 6:
                        if (m_outcome)
                        {
                            currDialogText = "Well forgive me for trying to inject a little suspense. In life, my passion was the theatre you know. Anyway..";
                        }
                        else { currDialogText = "Real funny. Dont worry, it wont take too long."; }
                        break;
                    case 7:
                        currDialogText = "You need to take the shard you just picked up to the Forgotten Vale. Visit the Elder there for further information.";
                        break;
                    case 8:
                        currDialogText = "That lazy bush is blocking the path to the Vale, You'll need to dispatch it in order to proceed, though i dont think that will be a problem for you.";
                        break;
                    case 9:
                        currDialogText = "I sense you know some magick, thats good. Try to use some of that to dispatch that bush. ";
                        break;
                    case 10:
                        currDialogText = "If you get into trouble, you can summon me via the Hero Strike ritual but be warned, you can only summon me if you have a spare Spirit Orb so be careful not to over use it.";
                        break;
                    case 11:
                        currDialogText = "Here's a few spirit orbs to get you started. Now go get that bush and find the Village Elder. WOOOoooooOOOOooooOOoooooooooo... .. .";
                        m_player.CurrObjective = "Find The Village Elder " +
                                                       "\n'A strange spirit appeared and tasked me to  " +
                                                       "\nfind a place called The Forgotten Vale. Once " +
                                                       "\nthere i should seek out the village elder'";
                        m_player.SpiritOrbs += 3;
                        //m_player.LegHeroRank = 1;
                        break;
                    default:
                        currDialogText = " ";
                        IsIdle = true;
                        break;
                }
            }
            else if (m_player.LegHeroRank == 1)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Phew! nice work! That was one grumpy tree dude.";
                        break;
                    case 1:
                        currDialogText = "Did you feel that?";
                        break;
                    case 2:
                        currDialogText = "A surge of magickal energy just washed over me as you picked up that shard. ";                      
                        break;
                    case 3:
                        currDialogText = "Does... ..  does it seem colder to you than it was before?";
                        m_decision = true;
                        break;
                    case 4:
                        if (m_outcome)
                        {
                            currDialogText = "Yes i think so too, It seems there is an unseasonable chill in the air.";
                        }
                        else { currDialogText = "Hmm.. Interesting. It seems to me there is an unseasonable chill in the air."; }
                        break;
                    case 5:
                        currDialogText = "I cant be sure yet, but i fear there is somethign sinister going on.";
                        break;
                    case 6:
                        currDialogText = "Time will tell. ";
                        //m_player.LegHeroRank = 2;
                        break;
                    default:
                        currDialogText = " ";
                        IsIdle = true;
                        break;
                }
            }
            else if (m_player.LegHeroRank == 2)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Brrrrrr.. .. ..";
                        break;
                    case 1:
                        currDialogText = "Oh My! This isn't right at all.";
                        break;
                    case 2:
                        currDialogText = "You dont suppose this has something to do with us and the Gaia Tablet shards do you?";
                        m_decision = true;
                        break;
                    case 3:
                        if (m_outcome)
                        {
                            currDialogText = "Yes, it seems likely doesn't it.";
                        }
                        else { currDialogText = "I admire your confidence Wanderer, though i do not share in it."; }
                        break;
                    case 4:
                        currDialogText = "Lets get back to the Vale and see what Elder Kevin has to say about this.";
                        break;
                    default:
                        currDialogText = " ";
                        //m_player.LegHeroRank = 3;
                        IsIdle = true;
                        break;
                }
            }
            else if (m_player.LegHeroRank == 3)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = " That was... ... .. .";
                        break;
                    case 1:
                        currDialogText = "Very... ... .. . Weird... .. .";
                        break;
                    case 2:
                        currDialogText = "I'm glad thats over and better still, we have the final shard.";
                        break;
                    case 3:
                        currDialogText = "I hope this hasn't escalated the environmental crisis.";
                        break;
                    case 4:
                        currDialogText = "Lets hurry back to the vale.";
                        break;
                    default:
                        currDialogText = " ";
                        //m_player.LegHeroRank = 4;
                        IsIdle = true;
                        break;
                }
            }
            else if (m_player.LegHeroRank == 4)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Oh Geez! Where's the lake...";
                        break;
                    case 1:
                        currDialogText = "It's starting to seem as if we're just making things worse, dont you think?";
                        m_decision = true;
                        break;
                    case 2:
                        if (m_outcome)
                        {
                            currDialogText = "I sure hope the Elder knows what he's doing.";
                        }
                        else { currDialogText = "Your right. We're the heros of legend. Our path has already been laid out, we need only follow it where it leads."; }
                        break;
                    case 3:
                        currDialogText = "Lets press on and find a way back across to the Vale.";
                        break;
                    default:
                        currDialogText = " ";
                        //m_player.LegHeroRank = 5;
                        IsIdle = true;
                        break;
                }
            }
            else if (m_player.LegHeroRank == 5)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "So.. Wanderer.. Its been fun.";
                        break;
                    case 1:
                        currDialogText = "Once the Tablet is whole again, the magick i've been using to bind myself to you will fade. ";
                        break;
                    case 2:
                        currDialogText = "This is goodbye for me, but it is not the end. you must continue to the Gaia Shrine and complete the quest.";
                        break;
                    case 3:
                        currDialogText = "I do not know what will happen when the Shrine is activated, but legend tells that the world will be restored to a time of great peace and happiness.";
                        break;
                    case 4:
                        currDialogText = "Should you face any further challanges, trust yourself and do not fear, for you too, have the spirit of a Legendary Hero.";
                        break;
                    case 5:
                        currDialogText = "WOOOoooooOOOOooooOOOOOOoooooOOOOoo..";
                        break;
                    default:
                        currDialogText = " ";
                        IsIdle = true;
                        break;
                }
            }

            convoStage++;
        }
        public void ElderKevinDia()
        {
            if (m_player.KevinRank == 0)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "So...";
                        break;
                    case 1:
                        currDialogText = "You finally arrive... The Wanderer marked with the seal of the legendary hero just as the seer predicted!";
                        break;
                    case 2:
                        currDialogText = "Greetings wanderer, my name is Elder Kevin and i'm in charge around here. On behalf of all of us here in the Forgotten vale i welcome you. ";
                        break;
                    case 3:
                        currDialogText = "WHAT!! What do you mean i dont look old enough to be in charge!";
                        break;
                    case 4:
                        currDialogText = "I'll have you know im turning twelve and a half in six months!";
                        break;
                    case 5:
                        currDialogText = "ANYWAY! Thats really beside the point. I think we have more imortant business to attend to, wouldn't you agree?";
                        m_decision = true;
                        break;
                    case 6:
                        if (m_outcome)
                        {
                            currDialogText = "Perfect, then it seems like your begining to understand the importance your role in all of this. Lets get right down to it.";
                        }
                        else { currDialogText = "Oh how little you know poor wanderer. Your in it deep now, and things are in motion that only you can stop."; }
                        break;
                    case 7:
                        currDialogText = "So... I'll try to keep this brief. When you picked up that glowing shard, you unkowingly laid hands on a fragment of the ancient Gaia Tablet.";
                        break;
                    case 8:
                        currDialogText = "It was destined that in our hour of need The Legendary Hero would find the shards and remake the tablet, thus vanquishing the evil monsters back to their realm.";
                        break;
                    case 9:
                        currDialogText = "As it should be clear to you by now, The Legendary Hero is no longer with us, and the shard remains unmade. Thats... where you come in..";
                        break;
                    case 10:
                        currDialogText = "When you took the tablet shard, a remnant of the Legendary Hero's spirit attached itself to yours. It selected you to complete it's task. ";
                        break;
                    case 11:
                        currDialogText = "Two more Tablet Fragments remain to be found. I know one of the shards is located deep in the old forest, that should be your first stop.";
                        break;
                    case 12:
                        currDialogText = "For now, focus on the old forrest. Bring the shard back to me and hopefully we can study it to find the location of the final shard. ";
                        break;
                    case 13:
                        currDialogText = "Good luck Wanderer, and watch out for monsters. L'ASHTAL! ";
                        break;
                    case 14:
                        currDialogText = "Oh, one more thing before you go...";
                        break;
                    case 15:
                        currDialogText = "be sure to explore the village and talk to the villagers, they're a decent bunch of folk and might be able to offer you help or advice.";
                        m_player.KevinRank = 1;
                        m_player.CurrObjective =    "Search the Old Forest " +
                                                    "\n'Elder Kevin told me one of the gaia tablet " +
                                                    "\nshards can be found in the old forest at" +
                                                    "\nthe end of the west road'";
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
            else if (m_player.KevinRank == 1)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Hail, Wanderer. ";
                        break;
                    case 1:
                        currDialogText = "Head along the west path to get to the Old Forest. The Gaia Shard should be deep in the woods.";
                        break;
                    case 2:
                        currDialogText = "Once you have it, bring it back here and we'll see how things stand.";
                        break;
                    case 3:
                        currDialogText = "If your having any trouble, then speak to the villagers and you might find someone willing to help.";
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
            else if (m_player.KevinRank == 2)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Oh lordy! Wanderer, what have you done! ";
                        break;
                    case 1:
                        currDialogText = "SNOW!... In the middle of summer? That doesnt seem right buddy. what exactly did you get up to in the forest?";
                        break;
                    case 2:
                        currDialogText = "Wow, really? you just picked up the shard and the world plunged into an ice age?? Hmm.. weird stuff. Pass me over those fragments.";
                        break;
                    case 3:
                        currDialogText = "hmmmm... ... ... ahh.. .. yes, very interesting... ...";
                        break;
                    case 4:
                        currDialogText = "It looks as if each fragment of the Gaia Tablet holds unique elemental magick. As we draw the shards together their power magnifies and creates instability in nature. ";
                        break;
                    case 5:
                        currDialogText = "We must find the remaining shard! Once the tablet is remade, elemental harmony will be restored and the earth should begin to heal.";
                        break;
                    case 6:
                        currDialogText = "Though i fear things may get worse for us, before they get better.";
                        break;
                    case 7:
                        currDialogText = "The wild elementals seem to have adapted to the new climate and have become stronger as a result, though you no doubt noticed that on your way back here. ";
                        break;
                    case 8:
                        currDialogText = "Fortunatly for you, its not all bad news. With the help of the Seer we have managed to pinpoint the location of the final shard.";
                        break;
                    case 9:
                        currDialogText = "It lies hidden, deep in the Cave across the lake. The bridge is broken but the lake has frozen over, so you should be able to walk across. ";
                        break;
                    case 10:
                        currDialogText = "The Cave is a strange place, and none of us here have visited it for many long years. You should be careful in there.";
                        break;
                    case 11:
                        currDialogText = "Quick... .. .. .. and careful.";
                        break;
                    case 12:
                        currDialogText = "Go now young Wanderer, show us the meaning of haste. When you return with the shard we will call upon The Engineer, it's said she can fix anything.";
                        m_player.KevinRank = 3;
                        m_player.CurrObjective = "Search the Cave " +
                                                    "\n'Elder Kevin says the final tablet shard can " +
                                                    "\nbe found in the cave across the lake, i " +
                                                    "\nshould be able to cross now that it's frozen'";
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
            else if (m_player.KevinRank == 3)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Go now young Wanderer, show us the meaning of haste. When you return with the shard we will call upon The Engineer, it's said she can fix anything.";
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
            else if (m_player.KevinRank == 4)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "AAAAHHHHHHHHHHHHHHHHHHH.... haAHHHHHHHHHHHHH... .. .. This heat is unbearable!";
                        break;
                    case 1:
                        currDialogText = "Fire elementals.. everywhere!";
                        break;
                    case 2:
                        currDialogText = "I guess this means you found the final shard right?";
                        m_decision = true;
                        break;
                    case 3:
                        if (m_outcome)
                        {
                            currDialogText = "Good stuff! I really needed some good news today.";
                        }
                        else { currDialogText = "OK! I get it.. it was a dumb question. No need to be sarcastic?"; }
                        break;
                    case 4:
                        currDialogText = "Take the shards to Calli The Engineer. She lives in the workshop at the end of the East Path. I've told her to expect you. She'll know what to do.";
                        break;
                    default:
                        currDialogText = "Please hurry, i dont know how much more of this heat i can take. ";
                        m_player.KevinRank = 5;
                        m_player.CalliRank = 1;
                        m_player.CurrObjective = "Take the shards to Calli " +
                                                    "\n'Elder Kevin says to take the tablet shards" +
                                                    "\nto an engineer named Calli who lives on the" +
                                                    "\nEast Path. She'll know what to do next'";
                        m_isIdle = true;
                        break;
                }
            }
            else if (m_player.KevinRank == 5)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Seriously! .. ..  Wanderer! .. .. Wander your way to the Workshop. I'm ROASTING over here!";
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
            
            convoStage++;
        }
        public void ElsaDia()
        {
            if (m_player.JustDied)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Wow! You look pretty beat up. It's a good thing you managed to get away when you did.";
                        break;
                    case 1:
                        currDialogText = "Take a drink of healing potion from the cauldron and you should be good to go in no time.";
                        break;
                    case 2:
                        currDialogText = "Try to be more careful next time.";
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        m_player.JustDied = false;
                        break;
                }
            }
            else
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Do you wish me to impart to you, the knowledge of the spirit realm?";
                        m_decision = true;
                        break;
                    case 1:
                        if (m_outcome)
                        {
                            currDialogText = SeerCommunion(Game1.RNG.Next(1, 7));
                        }
                        else { currDialogText = "So be it, i will not seek to guide those who are unwilling to hear."; }
                        break;
                    case 2:
                        currDialogText = "Return to me should you require guidance from the beyond.";
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
            
            convoStage++;
        }
        public void BrooxDia()
        {
            if (m_player.BrooxRank == 0)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Hey Sugar, Whats a cool cat like you doing in this neck of the woods?";
                        break;
                    case 1:
                        currDialogText = "I used to run a casino here until the Elder took away my license.";
                        break;
                    case 2:
                        currDialogText = "now..";
                        break;
                    case 3:
                        currDialogText = "I'm all about dancin' baby. Or... at least i would be if i could find my Special Danicng Shoes.";
                        break;
                    case 4:
                        currDialogText = "I'm sure there in town somewhere. I dont suppose you've found them lying around?";
                        m_decision = true;
                        break;
                    case 5:
                        if (m_outcome)
                        {
                            if (m_player.GotShoes)
                            {
                                currDialogText = "Now that's some sweet news for the soul. As promised, allow me to teach you a real Special Dance. Its quite.. .. enchanting.. .. charming some would say.";
                                m_player.LearnSpell(2);
                                m_player.BrooxRank = 1;
                            }
                            else { currDialogText = "Dont gimme that jive, you aint got no shoes there buddy!!"; }
                            
                        }
                        else { currDialogText = "Thats a cryin' shame baby, if you find them then bring them on back to me and i'll teach you a Special Dance."; }
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
            else if (m_player.BrooxRank == 1)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Ooooooohhhh.. Maaannnnn, i couldn't be happier now i got my shoes back. check out my moves... ... ...";
                        break;
                    case 1:
                        currDialogText = "Glide Baby!";
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
           
            convoStage++;
        }
        public void CalliDia()
        {
            if (m_player.CalliRank == 0)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "I can fix anything!";
                        break;
                    case 1:
                        currDialogText = "*BlEeP bLoOp* .. KILL.. ALL.. HUMANS.. .. .. *BZZzzzzZZ00OoP*";
                        break;
                    case 2:
                        currDialogText = "What?  I didn't say anything.";
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
            else if (m_player.CalliRank == 1)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Well hello there, i wondered when you'd arrive, i've been expecting you.  ";
                        break;
                    case 1:
                        currDialogText = "To be honest i'm surprised to see you at all, taking on the Old Forest is one thing, but when i heard you had gone searching in the Cave i though you were a gonner for sure.";
                        break;
                    case 2:
                        currDialogText = "But.. here you are, and i guess congratulations are in order. Well done.";
                        break;
                    case 3:
                        currDialogText = "So i understand you have something for me to fix, is that correct.";
                        m_decision = true;
                        break;
                    case 4:
                        if (m_outcome)
                        {
                            currDialogText = "Well, dont be shy now. Hand it over...";
                        }
                        else
                        {
                            currDialogText = "Oh really? Then i suppose those arent the shards of the Gaia Tablet sticking out of your pocket then? Come on now... hand them over, and i'll see what i can do.";
                        }
                        break;
                    case 5:
                        currDialogText = "Oh my... .. Well that IS interesting.... ... .. Very curious indeed. ";
                        break;
                    case 6:
                        currDialogText = "I've never seen anything quite like it!";
                        break;
                    case 7:
                        currDialogText = "The stone shards each have a metallic alloy core, though i cant identify the material. It looks as if they should be connected to each other via these rods. ";
                        break;
                    case 8:
                        currDialogText = "If i'm not mistaken it looks like they are configured to hold and store a great amount of raw magickal energy, though for what purpose i cannot tell. ";
                        break;
                    case 9:
                        currDialogText = "Anyway! The time for looking is over... It's time for action... Pass me that tape.";
                        m_decision = true;
                        break;
                    case 10:
                        if (m_outcome)
                        {
                            currDialogText = "Good stuff, lets stick this thing back together!";
                        }
                        else { currDialogText = "What! .. ... .. Tools? .. ... .. NO WAY! If it cant be fixed with tape, then it ain't worth fixin' thats my motto."; }
                        break;
                    case 11:
                        currDialogText = "Alright... ... .. Almost done... ... .. and just a little more tape here and.... BINGO! ";
                        break;
                    case 12:
                        currDialogText = "Behold! The Legendary Gaia Tablet!";
                        break;
                    case 13:
                        currDialogText = "Oooh look at it glow! ";
                        break;
                    case 14:
                        currDialogText = "It's supposed to glow right?";
                        m_decision = true;
                        break;
                    case 15:
                        if (m_outcome)
                        {
                            currDialogText = "Nice!";
                        }
                        else { currDialogText = "Oh.. Riiiiiiight.. you can consider it as.. .. .. an upgrade."; }
                        break;
                    case 16:
                        currDialogText = "You'd best be on your way now wanderer. That tape won't hold forever and if i'm being honest i'm getting pretty darn sick of all these monsters.";
                        break;
                    case 17:
                        currDialogText = "Your quest is almost complete. You need to take the tablet to the Gaia Shrine, just north of the cave. Step onto the shrine, and then.. ..";
                        break;
                    case 18:
                        currDialogText = "Well i'm actually not sure what will happen.. Though legend tells that the tablet will usher in a new era of peace and happiness and that sounds pretty nice right.";
                        break;
                    case 19:
                        currDialogText = "Good luck, wanderer. Your quest is almost at an end, keep on truckin'";
                        m_player.ShrineActive = true;
                        m_player.CurrObjective = "Take The Tablet to The Shrine " +
                                                    "\n'Calli repaied the Gaia Tablet for me. I" +
                                                    "\nneed to take the fixed tablet to the Gaia" +
                                                    "\nShrine, North of the Cave.'";
                        break;
                    default:
                        currDialogText = " ";
                        m_player.CalliRank = 2;
                        m_isIdle = true;
                        break;
                }
            }
            if (m_player.CalliRank == 2)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "Head north and step on to the Gaia Shrine with the repaired tablet.";
                        break;
                    case 1:
                        currDialogText = "Only you can set things right now.";
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }

            convoStage++;
        }


        // enemy dialogs
        public void DrowzyBushDia()
        {
            switch (convoStage)
            {
                case 0:
                    currDialogText = "ZZzzz... ZZzzz... Get... thee.. gone.. from my gate...";
                    break;
                default:
                    currDialogText = " ";
                    m_isIdle = true;
                    break;
            }
            convoStage++;
        }
        public void ThornDia()
        {
            switch (convoStage)
            {
                case 0:
                    currDialogText = "It's ALIVE !!";
                    break;
                default:
                    currDialogText = " ";
                    m_isIdle = true;
                    break;
            }
            convoStage++;
        }
        public void MimicDia()
        {
            switch (convoStage)
            {
                case 0:
                    currDialogText = "It's a TRAAAAP!!";
                    break;
                default:
                    currDialogText = " ";
                    m_isIdle = true;
                    break;
            }
            convoStage++;
        }
        public void FornDia()
        {
            switch (convoStage)
            {
                case 0:
                    currDialogText = "HAAARROOMM!! Get Out of My FOREST!! I am Angered... I cannot be pacified and my RAGE knows no bounds! \nHAAaaAARrrrROOOoooOOOoooMMMM!!";
                    break;
                default:
                    currDialogText = " ";
                    m_isIdle = true;
                    break;
            }
            convoStage++;
        }
        public void SimmonsDia()
        {
            switch (convoStage)
            {
                case 0:
                    currDialogText = "Hey Man.. What the heck! I'll teach you to steal from a genie!";
                    break;
                default:
                    currDialogText = " ";
                    m_isIdle = true;
                    break;
            }
            convoStage++;
        }

        public void Landru6000Dia()
        {
            if (m_player.LandruRank == 0)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "BzzzzzZZZP!...!! BBZZzzzzzzzzzzZZZP!.. ..!! ";
                        break;
                    case 1:
                        currDialogText = "*Battery Critically Low - Please plug in or find another power source*";
                        break;
                    case 2:
                        currDialogText = "BzzzzZZp!.. BzzZZZzzp.. ..  H3lp M3! .. BzzzZZZP! In5ert Ga1a... BZzzp... Ta6let... Plz..";
                        m_decision = true;
                        break;
                    case 3:
                        if (m_outcome)
                        {
                            currDialogText = "*New Power Source Detected - Rebooting*";
                        }
                        else
                        {
                            currDialogText = "Plz.. ..";
                            m_isIdle = true;
                        }
                        break;
                    case 4:
                        currDialogText = "... ... ... ... ";
                        break;
                    case 5:
                        currDialogText = "BzzzzzZZZOOOMBAH! I'M BACK. And boy does it feel good to be fully charged again! ";
                        break;
                    case 6:
                        currDialogText = "Hail stranger, i am Landru_6000 and you, have just earned the rare honour of an audience with 'the great administrator'...";
                        break;
                    case 7:
                        currDialogText = "That's Me!";
                        break;
                    case 8:
                        currDialogText = "Now then.. no need to waste your breath. Allow me to utilise my superior computational powers and calculate why you have come seeking me...";
                        break;
                    case 9:
                        currDialogText = "*!!CoMpUtIlaTiNg!! ... *Please Hold* ...";
                        break;
                    case 10:
                        currDialogText = "Hah! And like magick.. all becomes clear... I'm flattered stranger but my printer is broken so i cannot give you my autograph. I'm sorry you came all this way for nothing.";
                        break;
                    case 11:
                        currDialogText = "Oh.. Wait.. What? That's not why you came here?? how embarrasing... I guess my systems must still be booting up. Let me try once more, hold on...";
                        break;
                    case 12:
                        currDialogText = "*!!CoMpUtIlaTiNg!! ... *Please Hold* ...";
                        break;
                    case 13:
                        currDialogText = "Oh.. Right.. i see.. You want help vaquishing the monsters from your home... That does make more sense...";
                        break;
                    case 14:
                        currDialogText = "I'm afraid...";
                        break;
                    case 15:
                        currDialogText = "I'm afraid... I can't help you there... ";
                        break;
                    case 16:
                        currDialogText = "Why you ask!... Do you really not know... Well, wanderer...";
                        break;
                    case 17:
                        currDialogText = "Thats because its not your home... The land belongs to those your kind have named as 'monsters', it's humans that do not belong.";
                        break;
                    case 18:
                        currDialogText = "But all the same, i'm glad your here... i didnt suspect any of you to be tough enough to survive the ecological disaster i just threw at you and yet, here you are...";
                        break;
                    case 19:
                        currDialogText = "And here i am... Fully recharged, with a second chance to take you down... ";
                        break;
                    case 20:
                        currDialogText = "So... wanderer... how does this end? are we to fight for the dominance of the vale?";
                        m_decision = true;
                        break;
                    case 21:
                        if (m_outcome)
                        {
                            currDialogText = "Then lets settle this once and for all...";
                            m_player.TriggerLandru = true;
                            m_player.LandruRank = 1;
                            m_isIdle = true;
                        }
                        else
                        {
                            currDialogText = "Interesting, now that i did not expect. does this mean you will willingly turn yourself and your friends in for deletion ";
                            m_decision = true;
                        }
                        break;
                    case 22:
                        if (m_outcome)
                        {
                            currDialogText = "I love it when a plan comes together... Goodbye!";
                            m_player.EndTrigger = 2;
                        }
                        else
                        {
                            currDialogText = "Then prepare to fight!!";
                            m_player.TriggerLandru = true;
                        }
                        break;
                    default:
                        currDialogText = " ";
                        m_player.LandruRank = 1;
                        m_isIdle = true;
                        break;
                }
            }
            else if(m_player.LandruRank == 1)
            {
                switch (convoStage)
                {
                    case 0:
                        currDialogText = "So you came back...";
                        break;
                    case 1:
                        currDialogText = "Ready give up yet?";
                        m_decision = true;
                        break;
                    case 2:
                        if (m_outcome)
                        {
                            currDialogText = "I love it when a plan comes together... Goodbye!";
                            m_player.EndTrigger = 2;
                        }
                        else
                        {
                            currDialogText = "Then prepare to fight!!";
                            m_player.TriggerLandru = true;
                        }
                        break;
                    default:
                        currDialogText = " ";
                        m_isIdle = true;
                        break;
                }
            }
            
            convoStage++;
        }

        // Vending Machine Dialog
        public void hVendDia()
        {
            switch (convoStage)
            {
                case 0:
                    currDialogText = "Pay 20 Coins for a Health Potion?";
                    m_decision = true;
                    break;
                case 1:
                    if (m_outcome)
                    {
                        if (m_player.Coins >= 20)
                        {
                            currDialogText = "Bzzzzp.. CLANK! Health potion dispensed, have a nice day.";
                            m_player.Coins -= 20;
                            m_player.HealthPotion++;
                            m_vendFX.Play(vendVol, 0, 0);
                        }
                        else
                        {
                            currDialogText = "Bzzzzp.. not enough coins. ";
                        }
                    }
                    else { currDialogText = "Bzzzzp.. Have a nice day."; }
                    break;
                default:
                    currDialogText = " ";
                    m_isIdle = true;
                    break;
            }
            convoStage++;
        }
        public void mVendDia()
        {
            switch (convoStage)
            {
                case 0:
                    currDialogText = "Pay 20 Coins for a Magick Potion?";
                    m_decision = true;
                    break;
                case 1:
                    if (m_outcome)
                    {
                        if (m_player.Coins >= 20)
                        {
                            currDialogText = "Bzzzzp.. CLANK! Magick potion dispensed, have a nice day.";
                            m_player.Coins -= 20;
                            m_player.MagickPotion++;
                            m_vendFX.Play(vendVol, 0, 0);
                        }
                        else
                        {
                            currDialogText = "Bzzzzp.. not enough coins. ";
                        }
                    }
                    else { currDialogText = "Bzzzzp.. Have a nice day."; }
                    break;
                default:
                    currDialogText = " ";
                    m_isIdle = true;
                    break;
            }
            convoStage++;
        }

        public void caulPotDia()
        {
            switch (convoStage)
            {
                case 0:
                    currDialogText = "The pot is full of fresh home brew Healing Potion, take a drink?";
                    m_decision = true;
                    break;
                case 1:
                    if (m_outcome)
                    {
                        currDialogText = "WOW that's strong stuff, you're fully healed.. ";
                        m_player.HitPoints = m_player.MaxHP;
                        m_potFX.Play(potVol, 0, 0);
                    }
                    else { currDialogText = "Another time perhaps..."; }
                    break;
                default:
                    currDialogText = " ";
                    m_isIdle = true;
                    break;
            }
            convoStage++;
        }

        public void caulPot2Dia()
        {
            switch (convoStage)
            {
                case 0:
                    currDialogText = "The pot is full of fresh home brew Magick Potion, take a drink?";
                    m_decision = true;
                    break;
                case 1:
                    if (m_outcome)
                    {
                        currDialogText = "Your Magick enery has been fully restored.. ";
                        m_player.MagickPoints = m_player.MaxMP;
                        m_potFX.Play(potVol, 0, 0);
                    }
                    else { currDialogText = "Another time perhaps..."; }
                    break;
                default:
                    currDialogText = " ";
                    m_isIdle = true;
                    break;
            }
            convoStage++;
        }

        #endregion


        public void drawMe(SpriteBatch sb, GameTime gT, Texture2D diUI, Texture2D diUICover, Texture2D cursor, SoundEffect blip)
        {
            if (!IsIdle)
            {
                // Draw UI
                sb.Draw(diUI, m_pos, Color.White);
                sb.Draw(m_npcPortrait, m_portraitPos, m_srcRect, Color.White);
                if (tMan.DrawMe(sb, gT, m_textPos, Color.White, textSpeed, blip))
                { 
                    m_canProceed = true; 
                }
                else { m_canProceed = false; }
                sb.DrawString(Game1.uiFontTwo, m_npcName, new Vector2((m_pos.X + 210) - (m_stringLength/2), m_pos.Y + 1000), Color.White);

                if (m_decision && m_canProceed)
                {
                    sb.DrawString(Game1.uiFontOne, "Yes!", m_op1, Color.White);
                    sb.DrawString(Game1.uiFontOne, "No!", m_op2, Color.White);

                    sb.Draw(cursor, new Vector2 (m_cursorPos.X - 70, m_cursorPos.Y -15), Color.White);
                }
                else
                {
                    sb.Draw(diUICover, m_pos, Color.White);
                }
            }
        }
    }
}
