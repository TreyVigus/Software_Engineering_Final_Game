/*  This is the main file for the AC Framework.  The AC Framework was developed from
 * Rudy Rucker's excellent Pop framework, which was written in an old-style MFC 
 * ( compatible with .NET 2003), and is still available from 
 * www.rudyrucker.com/computergames.  Over the years, I've used the Pop framework for
 * my CIS 375 class, gradually making improvements to it (which Rudy Rucker did as well
 * from the first version of the Pop framework, and will still be an ongoing process
 * with the AC Framework).  One of the improvements that I made to the MFC code was to
 * make it possible for the models to do other things besides run, which is all they
 * could initially do.  I did this by creating a setState virtual function (with an
 * overload) in the cSprite base class.  Then, I made some appropriate modifications
 * for different states in the spritequake files.
 * 
 *  During the development of the ACFramework, I fixed the cListenerScooterYHopper and
 * the other listener hopper, so that you could only hop again after you touch down.
 * Previously, this had a bug that allowed you to hop again in mid air.  I also added
 * an artificial texture for setting the walls of a cSpriteTextureBox to different colors.
 * Sometimes, these walls would never appear, but this was probably a deficiency in
 * current game cards in their support of OpenGL more than the Pop framework.  I noticed 
 * that the bitmap textures would always appear, however, so I created an artificial 
 * bitmap of 1 x 1 pixel that can be used for color, and it works well.  I don't believe
 * that the current version of this AC Framework is completely bug free, though I try to
 * work out a bug whenever I encounter one.
 * 
 * I've tried to keep Rudy Rucker's excellent game algorithms intact, but
 * unfortunately, MFC programming is much different than C# programming, and much of
 * this had to be redesigned to make it work.  I've kept a lot of Rucker's comments in,
 * but they really need to be cleaned up, and I will probably get around to doing this
 * in the next year or so.  Many of Rucker's comments simply won't make sense because
 * of the redesign.  I've changed some of Rucker's comments and added a lot of comments 
 * myself for clarity, but not yet enough.
 * 
 * One of the most pervasive changes came about because the Pop framework made extensive
 * use of MFC's CArray class, which doesn't exist in C#.  I ended up making a data 
 * structure to mimic the CArray class, but I ended up making 
 * it a linked list, because I thought it would be more efficient.  The CArray class 
 * has InsertAt and RemoveAt functions, which require working with the middle of an 
 * array and, as a result, elements are slid one way or the other.  This makes them 
 * theta-n functions on average.  The linked list is theta-1 in this respect.  The 
 * advantage of using CArray is that elements can be randomly accessed in theta-1 time,
 * whereas the linked list has to be traversed to get to the element in question.  
 * However, most of the time, such accesses are done sequentially, processing the whole
 * array.  Therefore, we might as well be traversing a linked list.  I've got an iterator
 * set up in the LinkedList class, so we can use a foreach loop to do this.  All of the 
 * function names in the linked list are the same as they are in the CArray, and I put a 
 * C# indexer into the linked list so that the use of it is still much like an array.  In 
 * fact, the CArray and LinkedList should be pretty much interchangeable.  I did use
 * a delegate in the C# constructor for doing assignments, which are really intended for
 * deep copies, so that there are no side effects.  They are sometimes used for shallow
 * copies, though, for speed reasons -- in such cases, the deep copy wasn't necessary.
 * I shoved the LinkedList in the helper.cs file, along with some other things that I
 * needed to design for C#.
 * 
 * I also ended up making an IsKindOf function, that I needed to make virtual in many of
 * the base classes.  For every derived class, the IsKindOf checks the class type and
 * every base class it inherited from.  So this function will return true if the
 * class name matches the string parameter supplied to it, or any of its base classes
 * match the string parameter.  This is great for polymorphism, because the type of
 * any class can be checked at its most specific level or at any level.
 * 
 * The Pop framework made OpenGL calls for 3D graphics, instead of using Microsoft's
 * DirectX.  Microsoft's C# didn't have a hint of OpenGL that I could detect (thanks,
 * Bill), so I ended up looking for something that I could integrate into C# for OpenGL,
 * and it ended up being OpenTK.  I had a lot of trouble getting sounds to work without
 * using DirectX.  I didn't want to use DirectX because Microsoft doesn't seem to care
 * about backwards compatibility any more, and I thought it would just end up being a 
 * hassle with future versions.  I finally downloaded an OpenAL installer from Creative 
 * Labs, which solved the main problem of making a sound.  I still had a problem playing 
 * sounds in a realistic way within a game framework, but I finally developed a 
 * "sound engine" that I put into the resource.cs file.  I made use of C#'s threading,
 * so that it just runs in a loop, sleeping and waiting for a sound request.  All the 
 * Creative Labs installer did was put in two DLL's into the Windows\System32 folder, 
 * with no change to the registry.  So I took the DLL's and put them into the bin\Debug 
 * folder of the project.  They have to be put into bin\Release, of course, if one wants
 * to make a Release version (along with the models, sounds, and bitmaps folders).
 * 
 * This file was developed using OpenTK's QuickStart file as a starting point.
 * 
 * The thing that I liked most about the Pop framework is that it was great for teaching
 * students about complex inheritance issues and polymorphism.  Other game frameworks
 * might be better, but they are lacking in this educational element.  I hope to 
 * continue this educational element with the AC Framework, but in C#.
 *  
 * -- Jeffrey Childs, July 12, 2008
 * 
 * I've made a few changes to this version (1.1) of the AC Framework, so that it differs from 
 * the previous version (1).  These changes are:
 * 
 *  (1) If the programmer tries to use the wrong type of bullet with a certain critter,
 *  error messages will be shown on the screen.  For example, the player critter cannot
 *  use cCritterBulletSilver or any of its derived classes.
 *  (2) I've changes resource.cs so that it incorporates model files.  This makes models
 *  considerably easier to add into the game.
 *  (3) A bug exists which makes critters appear thin sometimes when setAttitude is used.
 *  I created a function called rotationAngle which I placed into VectorTransformation.cs.
 *  This function can be used with rotateAttitude, and no distortions of critters will take place.
 *  You rotate the tangent of the model into the attitudeTangent, so that it faces the
 *  direction it is moving in.  You can do this from the model's update function by 
 *  making the following function call:
 *        rotateAttitude( tangent().rotationAngle( attitudeTangent() ) );
 *  
 * -- Jeffrey Childs, April 20, 2009
 
 *  This version is now referred to as 1.2.  Rather than make an extremely long comment section 
 *  here, I've described the changes at the tops of the appropriate files.
 *  
 * -- Jeffrey Childs, Februrary 3, 2011
 * 
*/

using System;
using System.Drawing;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Platform;

namespace ACFramework
{
    class Framework : GameWindow
    {
        public static readonly float MIN_DT = 0.01f;  // don't allow more than 100 frames per second
        private static float _mindt;
        public static readonly float _maxdt = 0.1f; // if less than 10 frames per second,
                            // just pretend it is 10 frames per second
        private static float _runspeed = 1.0f;  // can increase to about 10 if desired
        private static long savetime = 0;  // used to help calculate dt
        private static cKeyInfo keydev;  // stores information about keys
        public static ACView view;  // has to do with drawing the scenery and critters
        private static ACDoc pdoc;   // has to do with establishing the game
        private static LinkedList<float> dthistory;  // I keep a history of dt's and
                    // take the average, so that the dt doesn't change too much because
                    // of issues like C#'s garbage collection 
        private static float dtsum = 0.0f;  // for the average dt
        private static int framecount = 0;  // for the average dt
        private static bool leftclick = false;  // set to true if left mouse button is clicked
        public static readonly cRandomizer randomOb = new cRandomizer(); /* This uses a time-dependent 
             generated seed, so a different sequence of random numbers will be produced 
             on each execution of the program.  When troubleshooting, pass in an 
             integer parameter to use as a seed, say 1.  Then the same sequence of 
             random numbers will be generated, allowing for consistent execution and 
             smoother troubleshooting of a problem -- JC */

        public static readonly BitmapRes bitm = new BitmapRes();  // for bitmap resources
        public static readonly Sound snd = new Sound();  // for playing sounds
        public static readonly vk vkeys = new vk();  // the keys that the programmer decides
                // to use in the game -- if programmers want to add more 
                // recognizable keys for more functionality, I have a resource.cs file
                // that they can add them in
        public static readonly ModelsMD2 models = new ModelsMD2();

        // Creates a 800x600 window with the title AC Framework
        public Framework() : base(800, 600, GraphicsMode.Default, "AC Framework") { }

        public static void setRunspeed(float rs) 
        {
            if (rs < 0.0f)
                rs = 0.0f;
            else if (rs > 10.0f)
                rs = 10.0f;
            _runspeed = rs;
        }

        public static void SetCursorPos(int x, int y)
        {
            Point p = new Point(x, y);
            Cursor.Position = p;
        }

        public void ShowStatusMessage(string message)
        {
            base.Title = message;  // I put the status of the game in the title bar,
                                // you can do something different here if you want
        }

        // In this function, I initialize the objects that I declared above, and
        // also initialize the OpenGL settings -- I used the same initial OpenGL
        // settings that are used in the Pop framework, and Rucker's comments are 
        // included about these initial settings
        public override void OnLoad(EventArgs e)
        {
            DateTime currentDate = new DateTime();
            currentDate = DateTime.Now;
            savetime = currentDate.Ticks;
            _mindt = MIN_DT;
            Keydev = new cKeyInfo();
            dthistory = new LinkedList<float>(
                delegate(out float f1, float f2)
                {
                    f1 = f2;
                }
                );
            pdoc = new ACDoc();
            view = new ACView();
            GL.ClearColor(Color.SteelBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.ClearDepth(1.0);
            // enable depth testing 
            GL.Enable(EnableCap.Normalize);
            /* By default DepthTest is off.  We normally need it so that nearer things cover 
        further things.  Don't need it, though, in the two dimensional case. */
            GL.DepthFunc(DepthFunction.Less);
            /* This is the default depth test, but we make it explicity.  Skips drawing any pixel with
        a smaller depth than the zbuffer depth value at that spot. */
            //	::glShadeModel(GL_SMOOTH); //The default shading model, instead we prefer GL_FLAT 
            GL.ShadeModel(ShadingModel.Flat);
            /* Smooth interpolates from the vertices across a polygon, while
            Flat picks one vertex (the first of the poly) and uses that color across
            the polygon. Default is Smooth.  Runs about 75% as fast as Flat, and there's
            no point to it for polyhedra, as all verts of a polyhedron face have the same
            normal anyway (Unless you tilt the vert normals to make the polyhedron resemble
            a curved surface, in which case you do want Smooth and should temporarily 
            turn it on for that object with a glShadeModel call). */
            //	::glEnable(GL_LINE_SMOOTH);  
            //Anti-alias lines.  Makes lines a bit more solid looking, but costs a lot of speed.
            //	::glEnable(GL_POLYGON_SMOOTH);  
            /* To make the polygon edges smoother. Don't even THINK of using this one, it cuts your speed
        to almost nothing. */
            //::glEnable(GL_CULL_FACE); 
            /* Don't draw back-facing polygons to save speed?  Better not.  First of all, we need to
        draw them in demo mode, as the teapot seems to have	clockwise polygons. Second of all,
        the cSpriteIcon doesn't work if we cull faces. */
         //   GL.Enable(EnableCap.Lighting);  
            /* Default is lighting ON.  We do in fact 
        always want to use lighting with OpenGL, as, surprisingly, OpenGL is FASTER with lighting 
        turned on!  Do be aware that if have lighting on, you MUST install some lights 
        or everything is black.  So we MUST have a call to installLightingModel here as well.  
        To be safe, we turn the light on with a call to our installLightingModel with
        a NULL argument to give a default  behavior that turns lighting on and adds some
        lights. */
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1); //We're not padding at ends of lines when we write textures.
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1); //We're not padding at ends of lines when we read textures.
        }

        // This is called when the window is resized.  I also set the camera's aspect
        // here, too.
        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            if ( view != null ) // it will be null on the first call -- JC
                view.pviewpointcritter().setAspect((float) Width / (float) Height ); // width/height ratio
        }

        // This is normally called when it is time to set up the next frame.  The game
        // logic, therefore, would normally be placed here.  Originally, I thought that
        // this would be a good place to calculate the dt and update the positions,
        // velocities, and accelerations of the critters, etc.  However, after some
        // reflection, I realized that the OnRenderFrame (below) should be called as
        // often as possible, and the dt and updates should be done there.  I use the
        // OnUpdateFrame to just get input from the user, and have it set to get it
        // about 30 times per second.  This gives me really decent keyboard and mouse
        // sensitivity.  Meanwhile, all the time that it takes to calculate the updates,
        // etc., is included in the dt, since I do this in the OnRenderFrame.  -- JC
        public override void OnUpdateFrame(UpdateFrameEventArgs e)
        {
            for (int i = 0; i < vk.KeyList.Length; i++)
            {
                if (Keyboard[vk.KeyList[i]])
                    Keydev.setkey(i);
                else
                    Keydev.resetkey(i);
            }

            if (Mouse[MouseButton.Left])
                leftclick = true;
            else
                leftclick = false;

            if (Mouse.XDelta != 0 || Mouse.YDelta != 0)
                view.OnMouseMove( new Point( Mouse.X, Mouse.Y ));

        }

        // this is called when it is time to render the next frame.
        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            if (pdoc.pgame().GameOver )
            {
                MessageBox.Show(pdoc.pgame().GameOverMessage);
                Exit();
            }
            
            
            DateTime currentDate = new DateTime();
            currentDate = DateTime.Now;
                /* This gets the number of 100-nanosecond time intervals that have 
                elapsed since midnight, January 1, 0001 AD.  I hope it is really this
                accurate, but it is probably more of a joke.  Yet, I don't believe that
                there is anything more accurate that can be used in C# for calculating
                a decent dt -- JC  */
            long elapsedTimeIn100ns = currentDate.Ticks - savetime;
                // savetime is the time saved from a previous iteration of OnRenderFrame
            savetime = currentDate.Ticks;
            float timeInSeconds = elapsedTimeIn100ns / 10000000.0f; // elapsed time in seconds
            if (timeInSeconds < _mindt)
                timeInSeconds = _mindt;
            else if (timeInSeconds > _maxdt)
                timeInSeconds = _maxdt;
                // I won't let the time be less than _mindt or greater than _maxdt so
                // that we get a decent number of frames per second.  If it's less
                // than _mindt, the picture can get kind of jittery.  If it's greater
                // than _maxdt, the number of frames per second is down in the mud.
                // At that point, we might as well just pretend we are getting 10 frames
                // per second, which gives as good a picture as can be expected -- JC
            float thisdt = _runspeed * timeInSeconds;
                // you can set _runspeed higher than 1.0 above, which will make things
                // appear to run faster -- I don't think it is necessary, though, and
                // it might do more harm than good.  Note that if the real
                // TimeInSeconds is greater than _maxdt, and we "pretend" we are
                // getting 10 frames per second, it has the same effect as 
                // increasing the _runspeed -- it's just an artificial increase -- JC
            if (framecount < 30)
            {
                dthistory.Add(thisdt);
                framecount++;
                dtsum += thisdt;
            }
            else
            {
                dtsum -= dthistory[0];
                dthistory.RemoveAt();
                dtsum += thisdt;
                dthistory.Add( thisdt );
            }
                // I keep a history of the past 30 dt's, and take the average of them
                // to use as the final dt (below).  This can compensate for such
                // artificial changes in the real dt as when C# spends some unexpected
                // time doing its garbage collection.  We don't want the dt to change
                // too abruptly or the picture will be screwed up and jittery. -- JC

            float dt = dtsum / framecount;

                // You can Console.WriteLine here the value 1.0 / dt, and it will tell
                // you how many frames per second you are getting.  About 50 frames
                // per second seems to be ideal, but anything between 10 and 100 seems
                // OK.  If you are getting close to 10, you probably don't have a 
                // compatible graphics card, and you have all graphics running from software -- JC
 
            pdoc.stepDoc(dt, view);

                // the stepDoc call does a lot -- calculates all positions, velocities,
                // and accelerations for critters, sets up the drawing for the game
                // world, sets up the drawing of the critters, etc. -- this call is
                // the main call to the guts of AC Framework -- JC

            ShowStatusMessage(view.pgame().statusMessage());
 
            SwapBuffers();  // a GameWindow method whieh displays everything
                            // that was drawn in stepDoc -- JC

        }

        static public ACDoc Pdoc
        {
            get { return pdoc; }
            set { pdoc = value; }
        }

        static public cKeyInfo Keydev
        {
            get { return keydev; }
            set { keydev = value; }
        }

        static public bool Leftclick
        {
            get { return leftclick; }
            set { leftclick = value; }
        }

        static public float Runspeed
        {
            get { return _runspeed; }
            set { _runspeed = value; }
        }


        static void Main()
        {
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (Framework framework = new Framework())
            {
                framework.Run(30.0, 0.0);
            }
        }
    }
}