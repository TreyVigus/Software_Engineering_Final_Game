/* This file contains the resources which, I believe, makes it easier for the 
 * programmer to add or delete resources.  The advantage of using this technique is
 * that the resources that are available can be seen on Intellisense.  Examples of
 * how the resources are used can be found throughout the AC Framework by doing 
 * a search -- JC */

// ACFramework 1.3 changes:  I made a better sound engine than the one that existed in 1.2.  
// The previous sound engine would just play one sound at a time  It was challenging, but there is a new
// sound engine in this version which mixes sounds so they can be played at the same time.
// The sound engine still needs work, but it is not a high priority now.  One of these days, I'll
// get around to it.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using OpenTK.Audio;
using OpenTK.Input;
using System.Windows.Forms;

namespace ACFramework
{
    class BitmapRes
    {
        // place bitmap identifiers here in numerical order,
        // then place the bitmap name in the Bitmap array
        private static readonly int SolidColor = 0;
        public static readonly int Door = 1;
        public static readonly int Wall1 = 9;
        public static readonly int Wall2 = 2;
        public static readonly int Wall3 = 3;
        public static readonly int Graphics1 = 4;
        public static readonly int Graphics2 = 10;
        public static readonly int Graphics3 = 5;
        public static readonly int Sky = 6;
        public static readonly int Wood2 = 7;
        public static readonly int Concrete = 8;
        public static readonly int Metal = 11;

        

        private static readonly string[] Bitmap = new string[] {
                "dummy", // nonexistent, used as placeholder -- JC
                "door.bmp",
                "wall2.bmp",
                "wall3.bmp",
                "graphics1.bmp",
                "graphics3.bmp",
                "sky.bmp",
                "wood2.bmp",
                "concrete1.bmp",
                "wall1.bmp",
                "graphics2.bmp",
                "metal1.bmp"
            };

        public BitmapRes()
        {
            for (int i = 0; i < Bitmap.Length; i++)
                Bitmap[i] = "bitmaps\\" + Bitmap[i];
        }

        public static string getResource(int resourceID)
        {
            return Bitmap[resourceID];
        }

        public static int Solid
        {
            get
            {
                return SolidColor;
            }
        }

    }

    class Sound
    {
        private AudioContext context;
        private AutoResetEvent swh = new AutoResetEvent(true);
        private bool somethingToPlay;
        private int [] request = new int [2]; // doubled when necessary
        private int nrequests = 0;
        private object locker = new object();
        private bool nosound = false;

        public static readonly int Pop = 0;
        public static readonly int Clap = 1;
        public static readonly int Crunch = 2;
        public static readonly int Goopy = 3;
        public static readonly int LaserFire = 4;
        public static readonly int Hallelujah = 5;
        // new sounds
        public static readonly int HammerOnce = 6;
        public static readonly int BottleRocket = 7;
        public static readonly int Blip = 8;

        private static readonly string[] sound = new string[] {
                "pop.wav",
                "clap.wav",
                "crunch.wav",
                "goopy.wav",
                "laserfire3.wav",
                "hallelujah.wav",
                "hammeronce.wav",
                "bottlerocket.wav",
                "blip.wav"
            };

        public Sound()
        {
            try
            {
                context = new AudioContext();
                for (int i = 0; i < sound.Length; i++)
                {
                    sound[i] = "sounds\\" + sound[i];
                }
                somethingToPlay = false;
                new Thread(soundEngine).Start();
            }
            catch (OpenTK.Audio.AudioDeviceException)
            {
                MessageBox.Show("Your computer either has its speakers disabled, you have no sound device, or you have an incompatible sound device.  You may continue to use the AC Framework without sound.");
                nosound = true;
            }

        }

        public void soundEngine()
        {
            AudioReader ar;
            int buff;
            int state;
            // start off at a capacity of 2, will double when necessary
            int [] currentSounds = new int[2];
            int[] soundBluePrint = new int[2];  // to build a list of sounds which will be copied
                // into currentSounds -- this is so I don't have to modify currentSounds while it
                // is being used, which could cause problems since it is a reference object
            int nSounds;  // current number of sounds

            while (true)
            {
                swh.WaitOne();
                lock (locker)
                {
                    if (!somethingToPlay)  // a Set may have been called while requests were processed
                                           // below 
                        continue;
                }
                int k;
                lock (locker)
                {
                    somethingToPlay = false;
                    nSounds = 0;
                    if (currentSounds.Length < request.Length)
                        Array.Resize(ref currentSounds, request.Length);
                    for (k = 0; k < nrequests; k++)
                    {
                        buff = AL.GenBuffer();
                        currentSounds[nSounds] = AL.GenSource();
                        ar = new AudioReader(sound[request[k]]);
                        AL.BufferData(buff, ar.ReadToEnd());
                        AL.Source(currentSounds[nSounds], ALSourcei.Buffer, buff);
                        nSounds++;
                    }
                    // request array is used up, so clear it
                    if (request.Length > 2)
                        Array.Resize(ref request, 2);
                    nrequests = 0;
                }
                AL.SourcePlay(nSounds, currentSounds);
                do
                {
                    Thread.Sleep(100); // seems like I kind set it less than 100 without sound distortion
                    int nCurrentSounds = nSounds;
                    nSounds = 0;
                    // check if something new to play
                    // if new sounds are played, we cannot mess with the currentSounds array, which is being used --
                    // we have to build up a soundBluePrint array
                    lock (locker)
                    {
                        if (somethingToPlay)
                        {
                            somethingToPlay = false;
                            if (nCurrentSounds + request.Length > soundBluePrint.Length)
                                Array.Resize(ref soundBluePrint, nCurrentSounds + request.Length);
                            for (k = 0; k < nrequests; k++)
                            {
                                buff = AL.GenBuffer();
                                soundBluePrint[nSounds] = AL.GenSource();
                                ar = new AudioReader(sound[request[k]]);
                                AL.BufferData(buff, ar.ReadToEnd());
                                AL.Source(soundBluePrint[nSounds], ALSourcei.Buffer, buff);
                                nSounds++;
                            }
                            if (request.Length > 2)
                                Array.Resize(ref request, 2);
                            nrequests = 0;                           
                        }
                    }

                    // Query the sources to find out if any stop playing
                    int temp = nCurrentSounds;
                    for (int j = 0; j < temp; j++)
                    {
                        AL.GetSource(currentSounds[j], ALGetSourcei.SourceState, out state);
                        if ((ALSourceState)state == ALSourceState.Playing)
                        {
                            // I want to pause them here, so I can include them with the new 
                            // sounds later -- if played again without being paused, they
                            // will start from the beginning.  I want them to be played
                            // simulatenously with the new sounds, but to pick up where
                            // they left off
                            AL.SourcePause(currentSounds[j]);
                            soundBluePrint[nSounds] = currentSounds[j];
                            nSounds++;
                        }
                        else
                            // free these resources for later sounds
                            AL.DeleteSource(currentSounds[j]);
                    }
                    if (nSounds > 0)
                    {
                        if (currentSounds.Length < nSounds)
                            Array.Resize(ref currentSounds, nSounds);
                        for (k = 0; k < nSounds; k++)
                            currentSounds[k] = soundBluePrint[k];
                        AL.SourcePlay(nSounds, currentSounds);
                    }
                } while (nSounds > 0);
            }
        }

        public void play(int i)
        {
            if (!nosound)
            {
                lock (locker)
                {
                    somethingToPlay = true;
                    if (request.Length == nrequests + 1)
                        Array.Resize(ref request, 2 * request.Length);
                    request[nrequests] = i;
                    nrequests++;
                }
                swh.Set();
            }
        }


    }

    class vk
    {
        // add more keys here that you need in numerical order
        // then add to the array below in the same order
        public static readonly int ControlLeft = 0;
        public static readonly int ControlRight = 1;
        public static readonly int ShiftLeft = 2;
        public static readonly int ShiftRight = 3;
        public static readonly int Left = 4;
        public static readonly int Right = 5;
        public static readonly int Up = 6;
        public static readonly int Down = 7;
        public static readonly int PageUp = 8;
        public static readonly int PageDown = 9;
        public static readonly int Home = 10;
        public static readonly int End = 11;
        public static readonly int Space = 12;
        public static readonly int Insert = 13;
        public static readonly int Delete = 14;
        public static readonly int U = 15;
        public static readonly int I = 16;
        public static readonly int D = 17;
        public static readonly int W = 18;
        public static readonly int A = 19;
        public static readonly int S = 20;
        public static readonly int L = 21;

        public static Key[] key;

        public vk()
        {
            key = new Key[] { Key.ControlLeft, 
                Key.ControlRight,
                Key.ShiftLeft,
                Key.ShiftRight,
                Key.Left,
                Key.Right,
                Key.Up,
                Key.Down,
                Key.PageUp,
                Key.PageDown,
                Key.Home,
                Key.End,
                Key.Space,
                Key.Insert,
                Key.Delete,
                Key.U,
                Key.I,
                Key.D,
                Key.W,
                Key.A,
                Key.S,
                Key.L
            };

        }

        public static Key[] KeyList
        {
            get { return key; }
            set { key = value; }
        }

    }

    struct ModelsMD2Info
    {
        public string ModelFolder;
        public string ModelPcx;
        public float offset;
        public bool randomSelection;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder">The folder name for your model.</param>
        /// <param name="filename">The pcx file name of your model.</param>
        /// <param name="offs">Adjusts positiion relative to floor.  Try 0.0f. 
        /// Adjust to positive value if feet are under the floor, adjust to negative value if floating.</param>
        /// <param name="ranselect">true for random selection of the critter, 
        /// otherwise false.  At least one must be set true.</param>
        public ModelsMD2Info(string folder, string filename, float offs,
            bool ranselect)
        {
            ModelFolder = folder;
            ModelPcx = filename;
            offset = offs;
            randomSelection = ranselect;
        }
    }

    class ModelsMD2
    {
        // add more models here that you need in numerical order
        // then add to the array below in the same order

        public static readonly int Squidge = 0;
        public static readonly int Robot = 1;
        public static readonly int Link = 2;
        public static readonly int Knight = 3;
        public static readonly int CitrusFrog = 4;
        public static readonly int Mog = 5;
        public static readonly int DrFreak = 6;
        public static readonly int DragonKnight = 7;
        public static readonly int MrSkeltal = 8;
        public static readonly int MrSkeltalBlue = 9;

        // at least one must be set true
        private static readonly ModelsMD2Info[] minfo = {
            new ModelsMD2Info( "Squidge", "squidge.pcx", 0.0f, true ),
            new ModelsMD2Info( "robot", "robot.pcx", 0.2f, true ),
            new ModelsMD2Info( "link", "soft_link.pcx", 0.1f, true ),
            new ModelsMD2Info( "pknight", "ctf_b.pcx", 0.15f, true ),
            new ModelsMD2Info( "citrusfrog", "BigRed.pcx", 0.3f, true ),
            new ModelsMD2Info( "mog", "mog.pcx", 0.3f, false ),
            new ModelsMD2Info( "drfreak", "bmovie.pcx", 0.3f, false ),
            new ModelsMD2Info( "dragonknight", "dragon_armour.pcx", -0.15f, false ),
            new ModelsMD2Info( "ichabod", "ctf_r.pcx", 0.3f, false),
            new ModelsMD2Info( "ichabod", "ctf_b.pcx", 0.3f, false)
        };            

        private static int[] randomCritters;

        public ModelsMD2()
        {
            for (int i = 0; i < minfo.Length; i++)
                minfo[i].ModelPcx = "models\\" + minfo[i].ModelFolder + "\\" + 
                    minfo[i].ModelPcx;
            int count = 0;
            for (int i = 0; i < minfo.Length; i++ )
                if ( minfo[i].randomSelection == true )
                    count++;
            randomCritters = new int[count];
            count = 0;
            for (int i = 0; i < minfo.Length; i++ )
                if ( minfo[i].randomSelection == true )
                {
                    randomCritters[count] = i;
                    count++;
                }
        }

        public string getModelFileName(int modelIndex)
        {
            return "models\\" + minfo[modelIndex].ModelFolder + "\\tris.MD2";
        }

        public string getSkinFileName(int modelIndex)
        {
            return minfo[modelIndex].ModelPcx;
        }

        public cVector3 getCorrectionPercents(int modelIndex)
        {
            cVector3 cp = new cVector3();
            cp.Z = minfo[modelIndex].offset;
            return cp;
        }

        public int selectRandomCritter()
        {
            int selection = (int)Framework.randomOb.random((uint)randomCritters.Length);
            return randomCritters[selection];
        }
    }    
}
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              