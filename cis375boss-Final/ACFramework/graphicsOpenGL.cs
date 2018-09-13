// For AC Framework 1.2, default parameters were added -- JC

using System;
using System.Collections;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using System.Windows.Forms;

namespace ACFramework
{ 
	
    // begin Rucker's comment
	/*
		The OpenGL code in this module is inspired by code in Ron Fosner, "OpenGL Programming for 
	Windows 95 and Windows NT", (Addison-Wesley 1997).  Similar code can also be found in
	the online MSDN "Guide to OpenGL" that comes with Visual C++.
		Note that OpenGL is likely to run too slow for videogames on a system that is not using hardware
	OpenGL acceleration. To get acceleration, you (a) have to have a graphics card with an OpenGL 
	acceleator and (b) have to have your machine set to a graphics mode in which the card accelerates 
	OpenGL: normally 16-bit color (65K colors) or "True Color" are accelerated, but other modes may not be. 
		You can tell if you are getting hardware acceleration by looking at the messsages in the Output window 
	when running in the Debug mode.  If the message says you are using an OpenGL implementation by Microsoft,
	then you aren't getting acceleration, if the message says  you are using an implmenetation by the
	maker of your graphics card, then you are getting acceleration.  (These messages are output by
	TRACE statements inside our cGraphicsOpenGL::initializeOpenGL method.)
		See http://www.opengl.org/developers/faqs/technical/mswindows.htm for more info about getting
	hardware acceleration.
	*/
    // end Rucker's comment

    // begin Childs comment

    /* In Rucker's code, the maps inherited from CMap, which doesn't exist in C#.  However, CMap is really not
     * much more than a hash table, and C# has a Hashtable class.  Therefore, I changed the code so that they
     * inherit from Hashtable and use Hashtable functions.  I've also added another map for solid colors, which
     * I believe will work better for many hardware platforms -- it certainly worked better on my computer.  I
     * simply convert the C# System Color into an integer using the ToArgb function, and then use that as the key
     * in the Hashtable.  It retrieves the texture, which is just an artificial 1 x 1 pixel bitmap of the 
     * specified System Color that the programmer uses.  I call the map and map entry ColorToSolidTexture.
     * 
     * I thought this file would be a nightmare to convert to C#, since OpenTK in C# is different than OpenGL
     * in MFC.  I also didn't know too much at the time about OpenGL, and I didn't have any documentation for 
     * OpenTK, which contributed to my apprehension.  Thankfully, there is a real correspondence between OpenGL
     * and OpenTK, and it is revealed by using Intellisense (if not for Intellisense, it would have taken me
     * forever).  So it was a lot of work to convert, but it wasn't a nightmare.
     */

    // end Childs comment

    //==========ResourceID 
    struct cMapEntry_ColorToSolidTexture
    {
        public static readonly int FRESHLIFESPAN = 10000; /* How many updates you wait till killing an unused
				image off. Let's try 5. */
        //members 
        public cTexture _pTexture;
        public int _lifespan;
        //methods 

        public cMapEntry_ColorToSolidTexture(cTexture ptex)
        {
            _pTexture = ptex;
            _lifespan = cMapEntry_ColorToSolidTexture.FRESHLIFESPAN;
        }
    }

    struct HashPair_ColorToSolidTexture
    {
        public int k;
        public cMapEntry_ColorToSolidTexture ctst;
    }

    class cMap_ColorToSolidTexture : Hashtable
    /* If you ever use the C# Hashtable for anything, MAKE SURE that you use consistent keys --
     * don't use an uint key in one place and an int key somewhere else -- the Hashtable
     * class will treat them differently.  In other words, a uint key of 1 will be a 
     * different key than an int key of 1 -- this problem drove me crazy, because I never
     * realized until now that a Hashtable can have different types of keys -- and
     * I still don't think that they should have different types of keys.
     * All the Hashtables used here have uint keys. -- JC  */

    
    {

        public cTexture lookupTexture(int resourceID)
        {
            if (Contains(resourceID))
            {
                cMapEntry_ColorToSolidTexture pmapentry =
                    (cMapEntry_ColorToSolidTexture)this[resourceID];
                pmapentry._lifespan = cMapEntry_ColorToSolidTexture.FRESHLIFESPAN;
                //So garbageCollect knows this texture is worth keeping around.
                return pmapentry._pTexture;
            }
            else
                return null; //No texture found.
        }

        public void garbageCollect()
        {
            /*	The idea here is that for every mapentry with a 0 or negative lifespan we delete its _pTexture. */

            LinkedList<HashPair_ColorToSolidTexture> pairs =
                new LinkedList<HashPair_ColorToSolidTexture>(
                    delegate(out HashPair_ColorToSolidTexture h1,
                        HashPair_ColorToSolidTexture h2)
                    {
                        h1 = h2;
                    }
                );

            HashPair_ColorToSolidTexture pair;
            foreach (int key in this.Keys)
            {
                pair.k = key;
                pair.ctst = (cMapEntry_ColorToSolidTexture)this[key];
                pairs.Add(pair);
            }

            if (pairs.Size == 0)
                return;

            pairs.First(out pair);
            do
            {
                if (--(pair.ctst._lifespan) <= 0)
                    Remove(pair.k);
                else
                    this[pair.k] = pair.ctst;
            } while (pairs.GetNext(out pair));
        }  
    }

    //==========ResourceID 
	struct cMapEntry_ResourceIDToTexture 
	{ 
		public static readonly int FRESHLIFESPAN = 10000; /* How many updates you wait till killing an unused
				image off. Let's try 5. */ 
		//members 
		public cTexture _pTexture; 
		public int _lifespan; 
		//methods 
			
		public cMapEntry_ResourceIDToTexture( cTexture ptex ) 
		{ 
			_pTexture = ptex; 
			_lifespan = cMapEntry_ResourceIDToTexture.FRESHLIFESPAN; 
		} 
	}

    struct HashPair_ResourceIDToTexture
    {
        public uint k;
        public cMapEntry_ResourceIDToTexture ritt;
    }
    
    class cMap_ResourceIDToTexture : Hashtable 
	/* If you ever use the C# Hashtable for anything, MAKE SURE that you use consistent keys --
     * don't use an uint key in one place and an int key somewhere else -- the Hashtable
     * class will treat them differently.  In other words, a uint key of 1 will be a 
     * different key than an int key of 1 -- this problem drove me crazy, because I never
     * realized until now that a Hashtable can have different types of keys -- and
     * I still don't think that they should have different types of keys.
     * All the Hashtables used here have uint keys. -- JC  */
    { 
		public cTexture lookupTexture( uint resourceID ) 
		{ 
			if ( Contains( resourceID ) )
            {
                cMapEntry_ResourceIDToTexture pmapentry =
                    (cMapEntry_ResourceIDToTexture) this[resourceID]; 
				pmapentry._lifespan = cMapEntry_ResourceIDToTexture.FRESHLIFESPAN; 
					//So garbageCollect knows this texture is worth keeping around.
				return pmapentry._pTexture;
            }
            else
				return null; //No texture found.
		}

        public void garbageCollect()
        {
            /*	The idea here is that for every mapentry with a 0 or negative lifespan we delete its _pTexture. */

            LinkedList<HashPair_ResourceIDToTexture> pairs =
                new LinkedList<HashPair_ResourceIDToTexture>(
                    delegate(out HashPair_ResourceIDToTexture h1,
                        HashPair_ResourceIDToTexture h2)
                    {
                        h1 = h2;
                    }
                );

            HashPair_ResourceIDToTexture pair;
            foreach (uint key in this.Keys)
            {
                pair.k = key;
                pair.ritt = (cMapEntry_ResourceIDToTexture)this[key];
                pairs.Add(pair);
            }

            if (pairs.Size == 0)
                return;

            pairs.First(out pair);
            do
            {
                if (--(pair.ritt._lifespan) <= 0)
                    Remove(pair.k);
                else
                    this[pair.k] = pair.ritt;
            } while (pairs.GetNext(out pair));
        }  

    } 
	
	//=========SkinFileID 
	struct cMapEntry_SkinFileIDToTexture 
	{ 
			public static readonly int FRESHLIFESPAN = 10000; /* How many updates you wait till killing an unused
				skin texture off. Let's try 20. */ 
		//members 
			public cTexture _pTexture; 
			public int _lifespan; 
		//methods 
			
		public cMapEntry_SkinFileIDToTexture( cTexture ptex ) 
			{ 			
				_pTexture = ptex; 
				_lifespan = cMapEntry_SkinFileIDToTexture.FRESHLIFESPAN; 
			} 
		
	}

    struct HashPair_SkinFileIDToTexture
    {
        public uint k;
        public cMapEntry_SkinFileIDToTexture sfit;
    }
    
    class cMap_SkinFileIDToTexture : Hashtable  
	{ 
		
		public cTexture lookupTexture( uint skinFileID ) 
		{
            if (Contains(skinFileID))
            {
                cMapEntry_SkinFileIDToTexture pmapentry =
                    (cMapEntry_SkinFileIDToTexture)this[skinFileID];
                pmapentry._lifespan = cMapEntry_SkinFileIDToTexture.FRESHLIFESPAN;
                //So garbageCollect knows this texture is worth keeping around.
                return pmapentry._pTexture;
            }
            else
                return null; //No texture found.
		}

        public void garbageCollect()
        {
            /*	The idea here is that for every mapentry with a 0 or negative lifespan we delete its _pTexture. */

            LinkedList<HashPair_SkinFileIDToTexture> pairs =
                new LinkedList<HashPair_SkinFileIDToTexture>(
                    delegate(out HashPair_SkinFileIDToTexture h1,
                        HashPair_SkinFileIDToTexture h2)
                    {
                        h1 = h2;
                    }
                );

            HashPair_SkinFileIDToTexture pair;
            foreach (uint key in this.Keys)
            {
                pair.k = key;
                pair.sfit = (cMapEntry_SkinFileIDToTexture)this[key];
                pairs.Add(pair);
            }

            if (pairs.Size == 0)
                return;

            pairs.First(out pair);
            do
            {
                if (--(pair.sfit._lifespan) <= 0)
                    Remove(pair.k);
                else
                    this[pair.k] = pair.sfit;
            } while (pairs.GetNext(out pair));
        }  

    } 
	
	//==========SpriteID 
	struct cMapEntry_SpriteIDToDisplayListID 
	{ 
			public static readonly int FRESHLIFESPAN = 10000; /* How many updates you wait till killing an unused
				displayListID off. Let's try 50. */ 
		//members 
			public uint _displayListID; 
			public int _lifespan; 
		//methods 
			
		public cMapEntry_SpriteIDToDisplayListID( int displayListID ) 
		{ 
			_displayListID = (uint) displayListID; 
			_lifespan = cMapEntry_SpriteIDToDisplayListID.FRESHLIFESPAN; 
		} 
		
	}

    struct HashPair_SpriteIDToDisplayListID
    {
        public uint k;
        public cMapEntry_SpriteIDToDisplayListID sitdli;
    }
    
    class cMap_SpriteIDToDisplayListID : Hashtable 
	{ 
		public uint lookupDisplayListID( uint spriteID ) 
		{
            if (Contains(spriteID))
            {
                cMapEntry_SpriteIDToDisplayListID pmapentry =
                    (cMapEntry_SpriteIDToDisplayListID) this[spriteID];
                pmapentry._lifespan = cMapEntry_SpriteIDToDisplayListID.FRESHLIFESPAN;
                //So garbageCollect knows this texture is worth keeping around.
                return pmapentry._displayListID;
            }
            else
                return 0; //0 is cGraphicsOpenGL.INVALIDDISPLAYLIST;
		}

        public void garbageCollect()
        {
            /*	The idea here is that for every mapentry with a 0 or negative lifespan we delete its _pTexture. */

            LinkedList<HashPair_SpriteIDToDisplayListID> pairs =
                new LinkedList<HashPair_SpriteIDToDisplayListID>(
                    delegate(out HashPair_SpriteIDToDisplayListID h1,
                        HashPair_SpriteIDToDisplayListID h2)
                    {
                        h1 = h2;
                    }
                );

            HashPair_SpriteIDToDisplayListID pair;
            foreach (uint key in this.Keys)
            {
                pair.k = key;
                pair.sitdli = (cMapEntry_SpriteIDToDisplayListID)this[key];
                pairs.Add(pair);
            }

            if (pairs.Size == 0)
                return;

            pairs.First(out pair);
            do
            {
                if (--(pair.sitdli._lifespan) <= 0)
                    Remove(pair.k);
                else
                    this[pair.k] = pair.sitdli;
            } while (pairs.GetNext(out pair));
        }  

    } 
	
	class cGraphicsOpenGL : cGraphics 
    {
    //Helper 
		public static readonly int DEFAULT_LINEPIXELS = 3; /* Thickness for lines, until I think of a way to use a
			reallinewidth parameter. */ 
		public static readonly uint INVALIDDISPLAYLISTID = 0; //This shadows cGraphics::INVALIDDISPLAYLISTID 
		public static readonly byte GLCIRCLESLICES = 16; 
		public static readonly byte NUMBEROFLIGHTS = 4;  // set between 1 and 4
	//Maps 
		protected cMap_SpriteIDToDisplayListID _map_SpriteIDToDisplayListID; 
		protected cMap_ResourceIDToTexture _map_ResourceIDToTexture; 
		protected cMap_SkinFileIDToTexture _map_SkinFileIDToTexture;
        protected cMap_ColorToSolidTexture _map_ColorToSolidTexture;
        /* The analagous _map is static readonly in cGraphicsMFC because the different cGraphicsMFC veiws
			can share the cMemoryDC resources.  In cGraphicsOpenGL _map_ResourceIDToTexture
			is NOT static readonly, because the pTexture objects are OpenGL texture objects similar to 
			display lists, and these are specific to the OpenGL instance that they are created in.
			 So we can't share them across views. */ 
	//Fields for recording displaylists 
        protected uint _activeDisplayListID; 
		protected bool _activeDisplayListIDIsReady;
        
    // arrays        
        protected float[] a;
        protected float[] b;
        protected float[] result;
        protected vector_t[] vertex;

        public cGraphicsOpenGL()
        {
            _activeDisplayListID = INVALIDDISPLAYLISTID;
            _activeDisplayListIDIsReady = false;
            /* Need to call initializeOpenGL(pview), but you have to wait till you have a pview
            passed by cGraphicsOpenGL.setOwnerView(CView *pview). */
            _map_SpriteIDToDisplayListID = new cMap_SpriteIDToDisplayListID();
            _map_ResourceIDToTexture = new cMap_ResourceIDToTexture();
            _map_SkinFileIDToTexture = new cMap_SkinFileIDToTexture();
            _map_ColorToSolidTexture = new cMap_ColorToSolidTexture();
            a = new float[3];
            b = new float[3];
            result = new float[3];
            vertex = new vector_t[3];
            for (int i = 0; i < 3; i++)
                vertex[i] = new vector_t(0);
            installLightingModel(null);
            /* Actually we can leave this line out, as the CpopView::setGraphicsClass
            calls installLightingModel anyway. */
        }
        
        // Routines that draw the three axis lines 

        public void draw3DAxes( ) 
		{ 
			// draw the tickmarked axes 
			draw3DAxesLine( -10.0f, 10.0f, 0, 20 ); 
			draw3DAxesLine( -10.0f, 10.0f, 1, 20 ); 
			draw3DAxesLine( -10.0f, 10.0f, 2, 20 ); 
		} 

        public void draw3DAxes( float start ) 
		{ 
            float finish;
			// make sure that start < 10.0 
            if (start > 10.0f)
            {
                finish = start;
                start = -10.0f;
            }
            else
                finish = 10.0f;
		
            float delta = finish - start; 
			int ticks = delta > 1.0f ? ( int ) delta : 0; 
		
			// draw the tickmarked axes 
			draw3DAxesLine( start, finish, 0, ticks ); 
			draw3DAxesLine( start, finish, 1, ticks ); 
			draw3DAxesLine( start, finish, 2, ticks ); 
		} 

        public void draw3DAxes( float start, float finish ) 
		{ 
			// make sure that start < finish 
			if ( start > finish ) 
				{ 
				float temp = start; 
				start = finish; 
				finish = temp; 
				} 
		
			float delta = finish - start; 
			int ticks = delta > 1.0f ? ( int ) delta : 0; 
		
			// draw the tickmarked axes 
			draw3DAxesLine( start, finish, 0, ticks ); 
			draw3DAxesLine( start, finish, 1, ticks ); 
			draw3DAxesLine( start, finish, 2, ticks ); 
		} 
	
		public void draw3DAxes( float start, float finish, int ticks ) 
		{ 
			// make sure that start < finish 
			if ( start > finish ) 
				{ 
				float temp = start; 
				start = finish; 
				finish = start; 
				} 
		
			// if ticks < 0 and delta is larger than 1, place the ticks 
			// on each scales unit length 
			if ( 0 > ticks ) 
				{ 
				float delta = finish - start; 
				ticks = delta > 1.0f ? ( int ) delta : 0; 
				} 
		
			// draw the tickmarked axes 
			draw3DAxesLine( start, finish, 0, ticks ); 
			draw3DAxesLine( start, finish, 1, ticks ); 
			draw3DAxesLine( start, finish, 2, ticks ); 
		} 

		
		public void draw3DAxesLine( float start, float finish, 	int axis_id, int ticks ) 
		{ 
			float px, py, pz; 
			float tickx, ticky, tickz; 
			float pdx, pdy, pdz, tinytick;
            float delta = ( finish - start )/( ticks < 1 ? 1 : ticks ); 
			float [] negativeColor = { 1.0f, 0.0f, 0.0f }; 
			float [] positiveColor = { 0.0f, 1.0f, 0.0f }; 
		
			pdx = pdy = pdz = px = py = pz = 0.0f; 
			tickx = ticky = tickz = 0.0f; 
			tinytick = 0.05f; 
		
			// select which of the 3 axes is going to vary 
			if ( 0 == axis_id ) // X axis 
				{ 
				pdx = delta; 	
				ticky = tinytick; 	
				px = start; 	
				} 
			else if ( 1 == axis_id ) // Y axis 
				{ 
				pdy = delta; 	
				tickx = tinytick; 	
				py = start; 	
				} 
			else 	// default Z axis 
				{ 
				pdz = delta; 	
				ticky = tinytick; 	
				pz = start; 	
				} 
		
            GL.PushAttrib(AttribMask.EnableBit);
				/* Save the enabled or disabled state of 
			GL_LIGHTING, GL_ALPHA_TEST, and GL_TEXTURE_2D, as we may change all of these here. */ 
            GL.Disable(EnableCap.Lighting);
            /* Lines look bad with lighting.  We'll restore the lighting
			with the glPopAttrib below. */

            GL.Begin(BeginMode.Lines);
		
			// now draw the two lines that make up the axis 
            GL.Color3(negativeColor);
            GL.Vertex3(px, py, pz);
            GL.Vertex3(0.0f, 0.0f, 0.0f);

            GL.Color3(positiveColor);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(px + pdx * ticks, py + pdy * ticks, pz + pdz * ticks);
		
			// now draw the tick marks 
			int i; 
			for ( i = 0; i < ticks; i++) 
				{ 
				if ( i < ticks / 2 ) 
				{ 
                    GL.Color3(negativeColor);
                } 
				else 
				{
                    GL.Color3(positiveColor);
				} 
		
                GL.Vertex3(px - tickx, py - ticky, pz - tickz);
                GL.Vertex3(px + tickx, py + ticky, pz + tickz);
		
				px += pdx; 
				py += pdy; 
				pz += pdz; 
				} 
		
            GL.End();
		
            GL.PopAttrib();
        } 

		
		// The increments are in 5ths of the maximum allowable values 
 	

	
	//========================================cGraphics Overloads========================== 
	

	//cGraphics overloads 
		
		public override void garbageCollect() 
		{
            _map_ColorToSolidTexture.garbageCollect();
            _map_ResourceIDToTexture.garbageCollect(); 
			_map_SkinFileIDToTexture.garbageCollect(); 
			_map_SpriteIDToDisplayListID.garbageCollect();
		} 

		/*Release all the not-recently-used 
			stored textures and displaylist IDs. */ 
	
	
		public override void vectorToPixel( cVector3 position, out int xpix, out int ypix, out float zbuff ) 
		{ 
			double [] modelviewmatrix = new double[ 16 ]; 
			double [] projectionmatrix = new double[ 16 ]; 
			int [] viewport = new int[ 4 ]; 
            GL.GetDouble(GetPName.ModelviewMatrix, modelviewmatrix);
            GL.GetDouble(GetPName.ProjectionMatrix, projectionmatrix);
            GL.GetInteger(GetPName.Viewport, viewport);
			double [] doublex = new double[1];
            double [] doubley = new double[1];
            double [] doublez = new double[1]; /* Need these as gluProject demands these args be double,
			not int or float. */
            Glu.Project(position.X, position.Y, position.Z,
                modelviewmatrix, projectionmatrix, viewport,
                doublex, doubley, doublez);
            xpix = ( int ) doublex[0]; 
			ypix = ( int )( viewport[ 3 ] - doubley[0] ); /* OpenGL counts y pixels from bottom to top, but we will use
			the Windows	programming convention of couting them from top to bottom.  And viewport[3] 
			is the height of the viewport. */ 
			zbuff = ( float ) doublez[0]; 
		} 
		
		public override cVector3 pixelToVector( int xpix, int ypix, float zbuff = 0.0f ) 
		{
            double[] doublex = new double[1];
            double[] doubley = new double[1];
            double[] doublez = new double[1];
            double[] modelviewmatrix = new double[16]; 
			double [] projectionmatrix = new double[ 16 ]; 
			int [] viewport = new int[ 4 ];
            GL.GetDouble(GetPName.ModelviewMatrix, modelviewmatrix);
            GL.GetDouble(GetPName.ProjectionMatrix, projectionmatrix);
            GL.GetInteger(GetPName.Viewport, viewport);
            ypix = viewport[ 3 ] - ypix; /* OpenGL counts y pixels from bottom to top, but we will use
			the Windows	programming convention of couting them from top to bottom.  viewport[3]
			is the height of the viewport. */
            Glu.UnProject((double) xpix, (double) ypix, (double) zbuff,
                modelviewmatrix, projectionmatrix, viewport,
                doublex, doubley, doublez);
            return new cVector3((float)doublex[0], (float)doubley[0], (float)doublez[0]);
        } 

		
			/* Note that the cGraphicsOpenGL::pixelToVector method is not all that accurate, and tends
			to be off by about 1%.  You shouldn't depend on it being an exact inverse of vectorToPixel. */ 
	
	//Matrix Method overloads 

        public override short MatrixModeProperty
        {
            set
            {
                if (value == MODELVIEW)
                    GL.MatrixMode( MatrixMode.Modelview );
                else if (value == PROJECTION)
                    GL.MatrixMode( MatrixMode.Projection );
                else
                    GL.MatrixMode(MatrixMode.Texture);
            }
        }
        
		public override void loadMatrix( cMatrix3 matrix ) 
		{
            float [] m = new float[16];
            int k = 0;
            for ( int i = 0; i < 4; i++ )
                for ( int j = 0; j < 4; j++ ) 
                {
                    m[k] = matrix.Elements[j,i];
                    k++;
                }
            
            GL.LoadMatrix( m );
        }

        public override void loadIdentity() { GL.LoadIdentity(); }

        public override void pushMatrix() { GL.PushMatrix(); }

        public override void popMatrix() { GL.PopMatrix(); } 
		
		public override void multMatrix( cMatrix3 rightmatrix )  
		{
            float[] m = new float[16];
            int k = 0;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    m[k] = rightmatrix.Elements[j, i];
                    k++;
                }

            GL.MultMatrix( m );
        }

		public override void translate( cVector3 translation ) 
		{
            GL.Translate(translation.X, translation.Y, translation.Z);
        }

        //Projection matrix methods.
		
		public override void ortho( float left, float right, float bottom, float top, float nearzclip, float farzclip ) 
		{
            GL.Ortho((double)left, (double)right, (double)top, (double)bottom,
                (double)nearzclip, (double)farzclip);
        } 
		
		public override void perspective( float fieldofviewangleindegrees, float xtoyaspectratio, float nearzclip, float farzclip ) 
		{
            Glu.Perspective( (double) fieldofviewangleindegrees,
                (double) xtoyaspectratio, (double) nearzclip, (double) farzclip );
        } 
		
		public override void frustum( float l, float r, float b, float t, float n, float f ) 
		{
            GL.Frustum((double)l, (double)r, (double)b, (double)t, (double)n, (double)f);
        } 

//cGraphics drawing overloads 
		
		// not a drawing overload -- JC
        public cTexture _getTexture( cSpriteIcon picon ) 
		{ 
			cTexture ptexture = _map_ResourceIDToTexture.lookupTexture( (uint) picon.ResourceID); 
				/* If successful, the lookupTexture call will set the _lifetime to 
			cMapEntry_ResourceIDToTexture.FRESHLIFESPAN to protect the thing from
			getting culled by garbageCollect. */ 
			if ( ptexture == null ) 	//We didn't find one, so we make one.
			{ 
				ptexture = new cTexture( picon.ResourceID, picon.UsesTransparentMask); 
					/* We record a new cMapEntry_ResourceIDToTexture matching the
				resourceID and ptexture.  The constructor gives it a FRESHLIFESPAN
				as well. */ 
				_map_ResourceIDToTexture[ (uint) picon.ResourceID ] = 
					new cMapEntry_ResourceIDToTexture( ptexture ); 
			} 
				/* The picon has an _aspect that perhaps has not yet been checked against the aspect of
			the texture it will get.  If we don't require that the
			icon keep some fixed preset aspect (as when we use the icon to fill a fixed aspect
			backgroudn rectangle for instance), we go ahead and tell it to match the aspect 
			of the image the texture is based on. Note that the cSpriteIcon::setAspect will 
			not chagne the 	cSpriteIcon->radius(), but it will change its sizex and sizey to
			have a ratio matching the aspect.*/ 
			if ( !picon.ImageLoaded && !picon.PresetAspect) 
					picon.Aspect = (float) ptexture.ImageAspect; 
			return ptexture; 	//All done!							 
		}

        public cTexture _getSolidTexture(cSpriteIcon picon)
        {
            cTexture ptexture = _map_ColorToSolidTexture.lookupTexture(picon.BitmapColor.ToArgb());
            /* If successful, the lookupTexture call will set the _lifetime to 
        cMapEntry_ResourceIDToTexture.FRESHLIFESPAN to protect the thing from
        getting culled by garbageCollect. */
            if (ptexture == null) 	//We didn't find one, so we make one.
            {
                ptexture = new cTexture(picon.BitmapColor);
                /* We record a new cMapEntry_ResourceIDToTexture matching the
            resourceID and ptexture.  The constructor gives it a FRESHLIFESPAN
            as well. */
                _map_ColorToSolidTexture[picon.BitmapColor.ToArgb()] =
                    new cMapEntry_ColorToSolidTexture(ptexture);
            } 

            /* The picon has an _aspect that perhaps has not yet been checked against the aspect of
        the texture it will get.  If we don't require that the
        icon keep some fixed preset aspect (as when we use the icon to fill a fixed aspect
        backgroudn rectangle for instance), we go ahead and tell it to match the aspect 
        of the image the texture is based on. Note that the cSpriteIcon::setAspect will 
        not chagne the 	cSpriteIcon->radius(), but it will change its sizex and sizey to
        have a ratio matching the aspect.*/
            if (!picon.ImageLoaded && !picon.PresetAspect)
                picon.Aspect = (float)ptexture.ImageAspect;
            return ptexture; 	//All done!							 
        } 
		
		public override void setViewport( int width, int height ) 
		{ 
            GL.Viewport(0, 0, width, height);
        } 

		
		public override void drawline( cVector3 posa, cVector3 posb, cColorStyle pcolorstyle ) 
		{
            GL.PushAttrib(AttribMask.EnableBit);
			    /* Save the enabled or disabled state of 
			    GL_LIGHTING and GL_TEXTURE_2D, as we disable these here. */
            GL.Disable(EnableCap.Lighting);
				/* Lines look bad with lighting.  We'll restore the lighting
			    with the glPopAttrib below. */
            GL.Disable(EnableCap.Texture2D); // Lines don't work with texture on.

            GL.LineWidth(DEFAULT_LINEPIXELS);  
			GL.Begin(BeginMode.Lines); 
			setMaterialColorFrontAndBack( pcolorstyle.LineColor); //For use with lighting 
			setVertexColor( pcolorstyle.LineColor); //For use with no lighting 
			setGLVertex( posa ); 
			setGLVertex( posb ); 
			GL.End();

            GL.PopAttrib(); //Leave lighting and texture as it was before.
		} 

		//reallinewidth 0 means 1 pixel wide.
	
		public override void drawrectangle( cVector3 corner0, cVector3 corner1, cVector3 corner2, cVector3 corner3, cColorStyle pcolorstyle, int drawflags ) 
		{  
			cVector3 normal = corner1.sub( corner0 ).mult( corner2.sub( corner0 ) ); 
			if ( pcolorstyle.Filled && ((ACView.DF_WIREFRAME & drawflags) == 0)) 
			{ 
				setMaterialColorFrontAndBack( pcolorstyle.FillColor); //For use with lighting 
				setVertexColor( pcolorstyle.FillColor); //For use with no lighting 
				GL.Begin(BeginMode.Polygon);
                GL.Normal3(normal.X, normal.Y, normal.Z);
                GL.Vertex3(corner0.X, corner0.Y, corner0.Z); 
				GL.Vertex3( corner1.X, corner1.Y, corner1.Z); 
				GL.Vertex3( corner2.X, corner2.Y, corner2.Z); 
				GL.Vertex3( corner3.X, corner3.Y, corner3.Z); 
				GL.End(); 
			} 
			GL.PushAttrib(AttribMask.EnableBit); /* Save the enabled or disabled state of 
			GL_LIGHTING and GL_TEXTURE_2D, as we disable these here. */ 
			GL.Disable(EnableCap.Lighting); 
				/* Lines look bad with lighting.  We'll restore the lighting
			with the glPopAttrib below. */ 
			GL.Disable(EnableCap.Texture2D); // Lines done't work with texture on.
			if ( pcolorstyle.Edged || ( drawflags & ACView.DF_WIREFRAME) != 0 ) 
			{ 
				setVertexColor( pcolorstyle.LineColor ); //For no lighting 
				setMaterialColorFrontAndBack( pcolorstyle.LineColor); //For lighting 
				GL.LineWidth( DEFAULT_LINEPIXELS ); //Must set this outside a glBegin/glEnd block.
				GL.Begin(BeginMode.LineLoop);
                GL.Normal3(normal.X, normal.Y, normal.Z);
                GL.Vertex3(corner0.X, corner0.Y, corner0.Z);
                GL.Vertex3(corner1.X, corner1.Y, corner1.Z);
                GL.Vertex3(corner2.X, corner2.Y, corner2.Z);
                GL.Vertex3(corner3.X, corner3.Y, corner3.Z);
                GL.End(); 
            } 
			GL.PopAttrib(); //Leave lighting and texture as it was before.
		} 

		
		public override void drawcircle( cVector3 center, float radius, cColorStyle pcolorstyle, int drawflags ) 
		{ // DON'T COPY 
			float angle = 0.0f; 
			float angleinc = 2.0f * (float) Math.PI / GLCIRCLESLICES; 
			cVector3 [] circlevert = new cVector3[ GLCIRCLESLICES ]; 
			for ( int i = 0; i < GLCIRCLESLICES; i++) 
			{ 
				circlevert[ i ] = center.add( new cVector3( radius * (float) Math.Cos( angle ), radius * (float) Math.Sin( angle ), 0.0f )); 
				angle += angleinc; 
			} 
			if ( pcolorstyle.Filled && (( drawflags & ACView.DF_WIREFRAME ) == 0)) 
			{ 
				GL.Begin(BeginMode.TriangleFan); 
				setMaterialColorFrontAndBack( pcolorstyle.FillColor); //For use with lighting 
				setVertexColor( pcolorstyle.FillColor); //For use with no lighting 
				setGLVertex( center ); 
				for ( int i = 0; i < GLCIRCLESLICES; i++) 
					setGLVertex( circlevert[ i ]); 
				setGLVertex( circlevert[ 0 ]); 
				GL.End(); 
			} 
			if ( pcolorstyle.Edged || ( (drawflags & ACView.DF_WIREFRAME) != 0 )) 
			{ 
				GL.LineWidth( DEFAULT_LINEPIXELS ); //Must set this outside a glBegin/glEnd block.
				GL.Begin(BeginMode.LineLoop); 
				setMaterialColorFrontAndBack( pcolorstyle.LineColor); //For use with lighting 
				setVertexColor( pcolorstyle.LineColor); //For use with no lighting 
				for ( int i = 0; i < GLCIRCLESLICES; i++) 
				    setGLVertex( circlevert[ i ]); 
				GL.End(); 
			} 
		} 

		
		public override void drawpolygon( cPolygon ppolygon, int drawflags ) 
		{ 
			/* In my first version of the code I didn't know about glBegin(GL_POLYGON)
		and thought I had to break the polygon into triangles myself.  Feb, 18, 
		2004, I switched to using GL_POLYGON in the drawPolygon call, and cleaned
		up the code as well. It runs at about the same speed, though. But it's
		cleaner and now I can now use this method for rectangles, which looked
		like four triangles in the old way I was doing it. But doing this breaks
		my non-convex polygons, that is, my stars.  So I use the old way of
		drawing polygons as a drawstarpolygon method below. */

            int i; //We'll have a lot of loops.
		//Find lighting, and decide whether to draw fill, draw edges, or both.  
            bool drawfill = ppolygon.Filled && (( drawflags & ACView.DF_WIREFRAME) == 0); 
					/* We fill if the polygon asks for fill and if we're not in
				 wireframe mode. */
            bool drawedges = ( !drawfill ) && //Edges suck if you have fill on.
				( ppolygon.Edged || ( (drawflags & ACView.DF_WIREFRAME) != 0 )); 
					/* We draw edges if polygon asks for edges, or if wireframe is 
				on. */ 
		//Now allocate and register your vertices array.
			int size = ppolygon.vertCount();
			//So I don't have to repeatedly write out the long expression.
		/* First create a vertex array holding the polygon vertices.  It doesn't matter that we may
	be inside the display list GL_COMPILE recorder, because all the GL_COMPILE actually
	"sees" is the actual values used in the vertex arrays. */
            GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);
				//Just so we can exit with state as we entered.
            GL.EnableClientState(EnableCap.VertexArray);
			float prismdz = ppolygon.PrismDz;
			float [] vertices = null; 
				//We'll use vertices to store the (bottom) face of the polygon, 
				//and in the prismdz case we put the extra top face in there too.
			ushort [] quadindices = null; 
			float [] edgenormals = null; 
			if ( prismdz != 0.0f ) 
			{ 
				vertices = new float[ 2 * 3 *( size )]; 
					/* The idea is to make two copies of an array that holds
				holds, flattened out, the x, y, and z coordinates of the
				size-many vertices.  And then, if prismDz()>0.0, you put
				another copy of this flat array, but with prismDz() added
				to the z-positioned numbers. */ 
				quadindices = new ushort[ 4 * size ]; 
				edgenormals = new float[ 3 * size ]; 
			} 
			else //No prismdz means only draw one face, and you don't need edges.
				vertices = new float[ 3 *( size )]; 
		//Now register the vertices pointer.
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertices); 
				/* Arguments are (size of tuple, type of number, stride, array).
			The stride = 0 means don't skip over any vertices. */ 
		//Set the size-count vertices of the polygon in slots 0 to size-1.
			for ( i = 0; i < size; i++) 
			{ 
				vertices[ 3 * i ] = ppolygon.getVertex( i ).X; 
				vertices[ 3 * i + 1 ] = ppolygon.getVertex( i ).Y; 
				vertices[ 3 * i + 2 ] = ppolygon.getVertex( i ).Z; 
			} 	
		
		//Now get the main face normals ready.
			/* Assuming the polygon vertices are labeled CCW.   For our standard polys,
			upnormal and downnormal will	just be the pos and neg ZAXIS.
			For the edge faces, we take the cross product between the edge
			line and the upnormal, which works for asteroids and stars as
			well as for regular convex polys.*/ 
			cVector3 upnormal = new cVector3();
            cVector3 downnormal = new cVector3(); 
			if ( ppolygon.vertCount() >= 3 ) 
				upnormal = ppolygon.getVertex( 1 ).sub( ppolygon.getVertex( 0 )).mult( 
				( ppolygon.getVertex( 2 ).sub( ppolygon.getVertex( 0 )))); 
			if ( upnormal.IsZero ) 
				upnormal.copy( new cVector3( 0.0f, 0.0f, 1.0f )); 
			downnormal = upnormal.neg(); 
		
		/* Note that  we draw the polygon with bottom face in the xy plane with the top face
	in the plane z = prismdz */ 
			if ( prismdz != 0.0f ) 
			{ 
		//Make the top face 
				for ( i = 0; i < size; i++) 
				{ 
					vertices[ 3 *( size ) + 3 * i ] = vertices[ 3 * i ]; 
					vertices[ 3 *( size ) + 3 * i + 1 ] = vertices[ 3 * i + 1 ]; 
					vertices[ 3 *( size ) + 3 * i + 2 ] = vertices[ 3 * i + 2 ] + prismdz; 
				} 
		/* Prepare the arrays you'll need to draw the edges of the prism. The vertices for these can be
	found inside our vertices array, but in a special order.  Think of a polygon that is, say,
	a triangle with verts at positions 1, 2, 3 and a copy
	of the triangle with verts translated by prismDz(), at positions 1', 2', 3'.  To go around
	the edge rects in counter clockwise order we want to hit 1, 2, 2', 1' and 2, 3, 3', 2' and so on.
	OpenGL lets us store such a "rat's nest" index order in an array. */ 
				for ( i = 0; i < size - 1; i++) 
				{ 
					quadindices[ 4 * i ] = (ushort) i; 
					quadindices[ 4 * i + 1 ] = (ushort) (i + 1); 
						/* Now skip to the second copy of	the verts, which is
					the copy with the prismDz() added into the z coords. */ 
					quadindices[ 4 * i + 2 ] = (ushort) (size + i + 1); 
					quadindices[ 4 * i + 3 ] = (ushort) (size + i); 
				} 
				//Now make the closing polygon edge face by hand.
				i = size -1; 
				quadindices[ 4 * i ] = (ushort) (size - 1); 
				quadindices[ 4 * i + 1 ] = (ushort) 0; 
				quadindices[ 4 * i + 2 ] = (ushort) size; 
				quadindices[ 4 * i + 3 ] = (ushort) (size + size - 1); 
		
				cVector3 edgenormal; 
					/* Do a cross product between an edge-line and the
				 upnormal to get a correct outward normal.  */ 
				for ( i = 0; i < size -1; i++) 
				{ 
					edgenormal = ( ppolygon.getVertex( i + 1 ).sub( ppolygon.getVertex( i ))).mult( 
						upnormal ); 
					edgenormal = edgenormal.mult( upnormal ); 
						//We expect upnormal to be cVector::ZAXIS 
					edgenormals[ 3 * i ] = edgenormal.X; 
					edgenormals[ 3 * i + 1 ] = edgenormal.Y; 
					edgenormals[ 3 * i + 2 ] = edgenormal.Z; 
				} 
			//Have to do the last edge normal as a special case as you wrap around to 0 
				i = size -1; 
				edgenormal = ppolygon.getVertex( 0 ).sub( ppolygon.getVertex( i )).mult( 0.5f ); 
				edgenormal = edgenormal.mult( upnormal ); 
				edgenormals[ 3 * i ] = edgenormal.X; 
				edgenormals[ 3 * i + 1 ] = edgenormal.Y; 
				edgenormals[ 3 * i + 2 ] = edgenormal.Z; 
			} //End the (prismdz) array loading code.
		//HERE we finally start making some gl calls.
			if ( drawfill ) 
			{
				setMaterialColorFrontAndBack( ppolygon.FillColor); //For use with lighting 
				setVertexColor( ppolygon.FillColor); //For use with no lighting 
				if ( prismdz == 0.0f ) 
				{ 
		//Draw the top-facing polygon in non-prismdz 
					GL.Normal3( upnormal.X, upnormal.Y, upnormal.Z);
                    GL.DrawArrays(BeginMode.Polygon, 0, size);
						/* The top polygon. The glDrawArrays call takes the arguments
					(mode, starting vertex number, total number of vertices).
					It knows to pull off three Reals per counted vertex, that is,
					the array is regarded as an array of 3-tuples. */ 
				} //End drawfill non-prismdz case.
				else //(prismdz) fill case 
				{ 
		//Draw the top-facing polygon in prismdz 
					GL.Normal3( upnormal.X, upnormal.Y, upnormal.Z); 
					GL.DrawArrays(BeginMode.Polygon, size, size ); 
						/* The top polygon. The glDrawArrays call takes the arguments
					(mode, starting vertex number, total number of vertices).
					It knows to pull off three Reals per counted vertex, that is,
					the array is regarded as an array of 3-tuples. Note that in
					the prismdz case, the top polygon starts at vertex number size
					instead of at vertex number 0.*/ 
		//Draw the bottom-facing polygon.
						/* The bottom face should have its vertices listed in the
					reverse order from the natural one so that, seen from outside
					the prism (which will be "from below"), they appear in the
					proper counterclockwise order. As far as the compiled code 
					is concerned what we do here is in fact about the same as a
					glDrawArrays call, except that this lets us reverse the
					order. */ 
					GL.Begin(BeginMode.Polygon); 
					GL.Normal3( downnormal.X, downnormal.Y, downnormal.Z);
                    for (i = size - 1; i >= 0; i--) //Verts in reverse order.
                        GL.ArrayElement(i);
					GL.End(); 
		//Draw the vertical band edge around the polygon 
					setMaterialColorDim( ppolygon.FillColor, 1.5f ); 
						//Make the edge brighter for lighting 
					setVertexColorDim( ppolygon.FillColor, 1.5f ); 
						//Make the edge brighter for no lighting 
					int normalsindex = 0; 
					int quadindex = 0; 
					for ( i = 0; i < size; i++) 
					{ 
						GL.Begin(BeginMode.Quads); 
						GL.Normal3( edgenormals[ normalsindex++], 
							edgenormals[ normalsindex++], 
							edgenormals[ normalsindex++]); 
						GL.ArrayElement( quadindices[ quadindex++]); 
						GL.ArrayElement( quadindices[ quadindex++]); 
						GL.ArrayElement( quadindices[ quadindex++]); 
						GL.ArrayElement( quadindices[ quadindex++]); 
						GL.End(); 
					} 
				} //End drawfill prismdz  case.
			} //End drawfill case 
			if ( drawedges ) 
				// We have no lighting and are filling and edging , or we have no fill 
			{ 
		//First pick a color for the lines.
				if ( drawfill ) /* If drawfill and drawedges are both on, this means you
				want contrasting edges. */ 
				{ 
					setVertexColor( ppolygon.LineColor); 
						//For use with no lighting 
					setMaterialColorFrontAndBack( ppolygon.LineColor); 
						//For use with lighting 
				} 
				else /* Drawfill is off and drawedges is on, drawing a skeleton.
				In this case we use the polygon's fillcolor for the line. */ 
				{ 
					setVertexColor( ppolygon.FillColor); 
					setMaterialColorFrontAndBack( ppolygon.FillColor); 
				} 
				GL.PushAttrib(AttribMask.EnableBit); /* Save the enabled or disabled state of 
			GL_LIGHTING and GL_TEXTURE_2D, as we disable these here. */
                GL.Disable(EnableCap.Lighting); 
				/* Lines look bad with lighting.  We'll restore the lighting
			with the glPopAttrib below. */ 
				GL.Disable(EnableCap.Texture2D); // Lines done't work with texture on.
		//Draw the edge line of the bottom polygon 
				GL.LineWidth( DEFAULT_LINEPIXELS ); 
					//Must set this outside a glBegin/glEnd block.
				GL.DrawArrays(BeginMode.LineLoop, 0, size ); 
					/* The glDrawArrays call takes the arguments
				(mode, starting vertex number, total number of vertices. */ 
				if ( prismdz != 0.0f ) 
				{ 
		//Draw the edge line of the top polygon 
					GL.DrawArrays(BeginMode.LineLoop, size, size ); 
		//Draw vertical lines for the edges of the quads around the polygon edge.
					int quadindex = 0; 
					for ( i = 0; i < size; i++) 
					{ 
						GL.Begin(BeginMode.Lines); 
							/* A quad's points are indexed so 0, 1
						are on bottom, and 2, 3 are on top, going
						the other way.  This means the vertical
						lines are from 0 to 3 and from 1 to 2. */ 
						GL.ArrayElement( quadindices[ quadindex ]); 
						GL.ArrayElement( quadindices[ quadindex + 3 ]); 
						GL.ArrayElement( quadindices[ quadindex + 1 ]); 
						GL.ArrayElement( quadindices[ quadindex + 2 ]); 
						quadindex += 4; 
						GL.End(); 
					} 
				} //End drawedges prismdz case 
			GL.PopAttrib(); //Leave lighting and texture as it was before.
			} //End drawedges case 
		
			GL.PopClientAttrib(); //To undo the glPushClientAttrib(GL_CLIENT_VERTEX_ARRAY_BIT); 
		} 

		
		public override void drawstarpolygon( cPolygon ppolygon, int drawflags ) 
		{ 
			int i; 
			int size = ppolygon.vertCount(); //So I don't have to repeatedly write out the long expression.
		/* First create a vertex array holding the polygon vertices.  It doesn't matter that we may
	be inside the display list GL_COMPILE recorder, because all the GL_COMPILE actually "sees"
	is the actual values used in the vertex arrays. */
            GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);
                //Just so we can exit with state as we entered.
            GL.EnableClientState(EnableCap.VertexArray); 
			float [] vertices = new float[ 2 * 3 *( size + 2 )]; /* The idea is to make two copies of an array that holds
			holds, flattened out, the x, y, and z coordinates of these size+2 points: the center,
			the size-many vertices, and the 0th vertex one more time.  And then, if prismDz()>0.0, you put
			another copy of this flat array, but with prismDz() added to the z-positioned numbers. */ 
			GL.Enable(EnableCap.Lighting); 
		//Set the origin in slot 0 (Not the polygon center, as we already took that into account 
		//by multiplying _spriteattitude at the right end of the modelveiw matrix.
			vertices[ 0 ] = 0.0f; //ppolygon->center().x(); NO 
			vertices[ 1 ] = 0.0f; //ppolygon->center().y(); 
			vertices[ 2 ] = 0.0f; //ppolygon->center().z(); 
		//Set the size-count vertices of the polygon in slots 1 to size.
			for ( i = 0; i < size; i++) 
			{ 
				vertices[ 3 + 3 * i ] = ppolygon.getVertex( i ).X; 
				vertices[ 3 + 3 * i + 1 ] = ppolygon.getVertex( i ).Y; 
				vertices[ 3 + 3 * i + 2 ] = ppolygon.getVertex( i ).Z; 
			} 	
		//Repeat the first vertex of the polygon in slot size+1 to close up (need this for the GL_TRIANGLE_FAN).
			vertices[ 3 + 3 * size ] = vertices[ 3 ]; 
			vertices[ 3 + 3 * size + 1 ] = vertices[ 3 + 1 ]; 
			vertices[ 3 + 3 * size + 2 ] = vertices[ 3 + 2 ]; 
			GL.VertexPointer( 3, VertexPointerType.Float , 0, vertices ); //(size of tuple, type of number, stride, array) 
		
		//We'll use these for the bottom face of the polygon, and we'll need them for drawing the edges as well.
			float prismdz = ppolygon.PrismDz; 
		/* Note that  we draw the polygon with bottom face in the xy plane with the top face
	in the plane z = prismdz */ 
		
		//	Real scalefactor = ppolygon->spriteattitude().scalefactor(); //For debugging.
		//	if (scalefactor != 1.0)//For debugging.
		//		TRACE("scalefactor %f", scalefactor);//For debugging.
			for ( i = 0; i < size + 2; i++) 
			{ 
				vertices[ 3 *( size + 2 ) + 3 * i ] = vertices[ 3 * i ]; 
				vertices[ 3 *( size + 2 ) + 3 * i + 1 ] = vertices[ 3 * i + 1 ]; 
				vertices[ 3 *( size + 2 ) + 3 * i + 2 ] = vertices[ 3 * i + 2 ] + ppolygon.PrismDz; 
			} 	
		/* Prepare the arrays you'll need to draw the edges of the prism. The vertices for these can be
	found inside our vertices array, but in a special order.  Think of a polygon that is, say,
	a triangle with verts at positions 1, 2, 3 and a copy
	of the triangle with verts translated by prismDz(), at positions 1', 2', 3'.  To go around
	the edge rects in counter clokwise order we want to hit 1, 2, 2', 1' and 2, 3, 3', 2' and so on.
	OpenGL lets us store such a "rat's nest" index order in an array. */ 
			ushort [] quadindices = new ushort[ 4 * size ]; 
			for ( i = 0; i < size; i++) 
			{ 
				quadindices[ 4 * i ] = (ushort) (1 + i); //The 1+ is to skip the center vertex in 0th place.
				quadindices[ 4 * i + 1 ] = (ushort) (1 + i + 1); 
				quadindices[ 4 * i + 2 ] = (ushort) (( size + 2 ) + 1 + i + 1); /* The (size+2) + skips to the second
				copy of	the verts, which is the copy with the prismDz() added into the z coords. */ 
				quadindices[ 4 * i + 3 ] = (ushort) (( size + 2 ) + 1 + i); 
			} 
		
		//Now get the normals ready.
			/* Assuming the polygon vertices are labelled CCW.   For our standard polys, upnormal 
			and downnormal will	just be the pos and neg ZAXIS. For the edge faces, 
			we take the cross product between the edge line and the upnormal, which works
			for asteroids and stars as well as for regular convex polys.   */ 
			cVector3 upnormal = new cVector3();
            cVector3 downnormal = new cVector3(); 
			if ( ppolygon.vertCount() >= 3 ) 
				upnormal = ppolygon.getVertex( 1 ).sub( ppolygon.getVertex( 0 )).mult( 
				( ppolygon.getVertex( 2 ).sub( ppolygon.getVertex( 0 )))); 
			if ( upnormal.IsZero ) 
				upnormal = new cVector3( 0.0f, 0.0f, 1.0f ); 
			upnormal.normalize(); 
			downnormal = upnormal.neg(); 
			float [] edgenormals = new float[ 3 * size ]; 
			cVector3 edgenormal; 
		/* Do a cross product between an edge-line and the upnormal to get a correct outward normal.  */ 
			for ( i = 0; i < size -1; i++) 
			{ 
				edgenormal = ppolygon.getVertex( i + 1 ).sub( ppolygon.getVertex( i )).mult( 
					upnormal ); 
				//Midpoint of line between vert i and vert i+1 
				edgenormal.normalize(); 
				edgenormal = edgenormal.mult( upnormal ); //We expect upnormal to be cVector::ZAXIS 
				edgenormals[ 3 * i ] = edgenormal.X; 
				edgenormals[ 3 * i + 1 ] = edgenormal.Y; 
				edgenormals[ 3 * i + 2 ] = edgenormal.Z; 
			} 
			//Have to do the last edge normal as a special case as you wrap around to 0 
			i = size -1; 
			edgenormal = ppolygon.getVertex( 0 ).sub( ppolygon.getVertex( i )).mult( 0.5f ); 
			edgenormal.normalize(); 
			edgenormal = edgenormal.mult( upnormal ); 
			edgenormals[ 3 * i ] = edgenormal.X; 
			edgenormals[ 3 * i + 1 ] = edgenormal.Y; 
			edgenormals[ 3 * i + 2 ] = edgenormal.Z; 
		//Find lighting, and decide whether to draw fill, draw edges, or both.  
			int currentlighting; 
			GL.GetInteger(GetPName.Lighting, out currentlighting ); 
			bool drawfill = ppolygon.Filled && (( drawflags & ACView.DF_WIREFRAME) == 0 ); 
					/* We fill if the polygon asks for fill and if we're not in
				 wireframe mode. */ 
			bool drawedges = ( !drawfill ) && //Edges suck if you have fill on.
				( ppolygon.Edged || ( (drawflags & ACView.DF_WIREFRAME) != 0 )); 
					/* We draw edges if polygon asks for edges, or if wireframe is 
				on. */ 
		//HERE we finally start making some gl calls.
			if ( drawfill ) 
			{ 
				setMaterialColorFrontAndBack( ppolygon.FillColor); //For use with lighting 
				setVertexColor( ppolygon.FillColor); //For use with no lighting 
		//Draw the bottom-facing polygon.
				/* What we want to do here is to draw the center of the polygon, draw the size vertices, and
				then draw the frist vertex again to close up the fan. You might think you could do a
				simple call to ::glDrawArrays(GL_TRIANGLE_FAN, 0, size + 2);
				But, if we assume that in general we are drawing a prism, then the bottom face should have
				its vertices listed in the reverse order from the natural one so that, seen from outside the
				prism (which will be "from below"), they appear in the proper counterclockwise order. 
				This will prevent the bottom face from incorrectly disappearing if we call
				::glEnable(GL_CULLFACE). As far as the compiled code is concerned what we do here is in
				fact about the same as a glDrawArrays call, except that this lets us reverse the order. */ 
				GL.Begin(BeginMode.TriangleFan); 
				GL.Normal3( downnormal.X, downnormal.Y, downnormal.Z); 
				GL.ArrayElement( 0 ); //The center from slot 0.
				GL.ArrayElement( 1 ); //The first vertex of the polygon from slot 1.
				for ( i = size; i > 0; i--) //The other polygon vertices in reverse order from slots size to 1.
					GL.ArrayElement( i ); 
				GL.ArrayElement( 1 ); /* The first polygon vertex to close up the fan.  Or could use (size+1),
				as that's a copy of the first vertex as well. */ 
				GL.End(); 
		//Draw the top-facing polygon 
				GL.Normal3( upnormal.X, upnormal.Y, upnormal.Z ); 
				GL.DrawArrays(BeginMode.TriangleFan, size + 2, size + 2 ); 
					/* The top polygon. We have the center,
				the size-count vertices, and the repeated initial vertex in place.  The glDrawArrays call
				takes the arguments	(mode, starting vertex number, total number of vertices. It knows to
				pull off three Reals per counted vertex, that is, the array is regarded as an array of
				3-tuples. */ 
		//Draw the vertical band edge around the polygon 
				setMaterialColorDim( ppolygon.FillColor, 1.5f ); //Make the edge brighter  
				setVertexColorDim( ppolygon.FillColor, 1.5f ); //Make the edge brighter  
				int normalsindex = 0; 
				int quadindex = 0; 
				for ( i = 0; i < size; i++) 
				{ 
					GL.Begin(BeginMode.Quads); 
					GL.Normal3( edgenormals[ normalsindex++], edgenormals[ normalsindex++], 
						edgenormals[ normalsindex++]); /* I wonder if something is wrong
						here, as the lighting I see on the edges doesn't seem logical */ 
					GL.ArrayElement( quadindices[ quadindex++]); 
					GL.ArrayElement( quadindices[ quadindex++]); 
					GL.ArrayElement( quadindices[ quadindex++]); 
					GL.ArrayElement( quadindices[ quadindex++]); 
					GL.End(); 
				} 
			} 
			if ( drawedges ) //Either we have no lighting and are filling and edging , or we have no fill 
			{ 
				if ( drawfill ) /* If drawfill and drawedges are both on, this means you
				want contrasting edges. */ 
				{ 
					setVertexColor( ppolygon.LineColor); //For use with no lighting 
					setMaterialColorFrontAndBack( ppolygon.LineColor); //For use with no lighting 
				} 
				else /* If drawfill is off and drawedges is on, we are drawing a skeleton of the polygon.
				In this case we use the polygon's fillcolor for the line. */ 
				{ 
					setVertexColor( ppolygon.FillColor); 
					setMaterialColorFrontAndBack( ppolygon.FillColor); 
				} 
				GL.PushAttrib(AttribMask.EnableBit); /* Save the enabled or disabled state of 
			GL_LIGHTING and GL_TEXTURE_2D, as we disable these here. */ 
				GL.Disable(EnableCap.Lighting); 
				/* Lines look bad with lighting.  We'll restore the lighting
			with the glPopAttrib below. */ 
				GL.Disable(EnableCap.Texture2D); // Lines done't work with texture on.
		//Draw the edge line of the bottom polygon 
				GL.LineWidth( DEFAULT_LINEPIXELS ); //Must set this outside a glBegin/glEnd block.
				GL.DrawArrays(BeginMode.LineLoop, 1, size ); /* The glDrawArrays call takes the arguments
				(mode, starting vertex number, total number of vertices. */ 
		//Draw the edge line of the top polygon 
				GL.LineWidth( DEFAULT_LINEPIXELS ); //Must set this outside a glBegin/glEnd block.
				GL.DrawArrays(BeginMode.LineLoop, size + 2 + 1, size ); 
		//Draw vertical lines for the edges of the quads around the polygon edge.
				GL.LineWidth( DEFAULT_LINEPIXELS ); //Must set this outside a glBegin/glEnd block.
				int quadindex = 0; 
				for ( i = 0; i < size; i++) 
				{ 
					GL.Begin(BeginMode.Lines); 
						/* As it happens a quad's points are indexed so 0, 1 are on bottom, and 2, 3 are on
					top, going the other way.  This means the vertical lines are from 0 to 3 and
					from 1 to 2. */ 
					GL.ArrayElement( quadindices[ quadindex ]); 
					GL.ArrayElement( quadindices[ quadindex + 3 ]); 
					GL.ArrayElement( quadindices[ quadindex + 1 ]); 
					GL.ArrayElement( quadindices[ quadindex + 2 ]); 
					quadindex += 4; 
					GL.End(); 
				} 
			GL.PopAttrib(); //Leave lighting and texture as it was before.
			} 
		//Release the memory for the vertex arrays.
			GL.PopClientAttrib(); //To undo the glPushClientAttrib(GL_CLIENT_VERTEX_ARRAY_BIT); 
		} 
		
		public override void drawbitmap( cSpriteIcon picon, int drawflags ) 
		{ 
			//Get the ptexture so you can find the index of the Texture object inside it.
            cTexture ptexture = null;
            if (picon.ResourceID == BitmapRes.Solid)
                ptexture = _getSolidTexture(picon);
            else
                ptexture = _getTexture( picon ); 
			if ( !picon.ImageLoaded ) //One way or another, you've managed to load the image now.
				picon.ImageLoaded = true; 
            selectTexture( ptexture ); 
	
			if (!( picon.Tiled)) 
			{ 	
				GL.Begin(BeginMode.Quads); //Walk from lower left counterclockwise around the rectangle.
				GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex3(-0.5f * picon.Sizex, -0.5f * picon.Sizey, 0.0f ); 
				GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex3( 0.5f * picon.Sizex, -0.5f * picon.Sizey, 0.0f ); 
				GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex3( 0.5f * picon.Sizex, 0.5f * picon.Sizey, 0.0f ); 
				GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex3(-0.5f * picon.Sizex, 0.5f * picon.Sizey, 0.0f ); 
				GL.End(); 
			} 
			else //begin tiled() case 
			{
				int xtiles = picon.XTileCount; // number of y tiles 
				int ytiles = (int) Math.Ceiling( xtiles / picon.Aspect); //aspect is sizex/sizey.
				float startX = picon.Lox; 
				float startY = picon.Loy; 
				float stepX = picon.Sizex / xtiles; 
				float stepY = picon.Sizey / ytiles; 
				GL.Begin(BeginMode.Quads); 
				for ( int j = 0; j < ytiles; j++) 
				{ 
					for ( int i = 0; i < xtiles; i++) 
					{ 
						GL.TexCoord2( 0, 0 ); 	
						GL.Vertex3( startX, startY, 0.0f ); 
	
						GL.TexCoord2( 1, 0 ); 	
						GL.Vertex3( startX + stepX, startY, 0.0f ); 
	
						GL.TexCoord2( 1, 1 ); 	
						GL.Vertex3( startX + stepX, startY + stepY, 0.0f ); 
	
						GL.TexCoord2( 0, 1 ); 	
						GL.Vertex3( startX, startY + stepY, 0.0f ); 
	
					startX += stepX; 
					} 
					startX = picon.Lox; 
					startY += stepY; 
				} 
				GL.End(); 
			} //End tiled() case 
	//		ptexture->unselect(); 
	} 
	
	//Texture Overloads 
	
		public override void adjustAttributes( cSprite psprite ) 
		{ 
				/* I'll enable or disable GL_TEXTURE_2D.  It must be ON to draw
			a cSpriteIcon or cSpriteQuake, but if I draw a solid face, then
			texture must be OFF, or the face comes out black. */ 
			if ( !psprite.UsesTexture ) 
				GL.Disable(EnableCap.Texture2D);
			if ( psprite.UsesTexture ) 
			{
				GL.Enable(EnableCap.Texture2D); 
		/* The following calls don't do anything unles we've enabled texture. 
	If we comment out ALLOWALPHATRANSPARENCY in texture.cpp, then you will
	in fact never have transparent icon. The speed of transparency, however,
	seems not to be a noticeable factor.  What IS noticeable is that, transparent 
	or not, cSpritIcon will  slowly if they are very large (near to the viewer
	and taking up a lot of the window with a very coarse magnified pixel size.) */
                if (psprite.UsesTransparentMask )
                {
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
                    /*	We are going to set the transparent parts of our textures to have
                alpha value of zero.  There are two possible ways to let things "show through"
                the transparent alpha.  We presently do a clipping-style alpha test.  Even if
                we were to turn on blending, some tests indicate that without the alpha test,
                the textures would only be transparent in one direction. 
                    This technique for getting a transparent background of a texture is
                mentioned in the Red Book, and indexed under "billboarding". We have set
                the alpha to 0 for our transparent texture backgrounds, but we need
                the GL_ALPHA_TEST as well, otherwise the "transparent" pixels can still cover
                up underlying pixels (unless we enable GL_BLEND, but I find the GL_ALPHA_TEST
                easier to get working, as described in the next comment. */
                    //::glEnable(GL_ALPHA_TEST); 
                    /* I'm turning alpha test on in my 
                    cGraphics::adjustAttributes(psprite) call*/
                    GL.AlphaFunc(AlphaFunction.Greater, 0.1f);
                    GL.Color4(0.0f, 0.0f, 0.0f, 1.0f); //Fix BaseColor and BaseAlpha 
                    /* As well as making the background transparent, we still have the issue
                of making the colors show up as you want them too when GL_BLEND is on.
                    Set the BaseColor (first three numbers) and BaseAlpha (fourth
                number), for the polygon you plan to put the texture onto.  The
                texture has its own TextureColor and TextureAlpha.
                    How the Texture and Base blend depends on which _textureFunction
                my texture has.  (Our transparent cTexture objects use GL_BLEND and
                the non-transparent ones use GL_DECAL.) Where necessary, we think of
                the * as operating term-by-term on	the color components.  The
                formula for GL_BLEND is this:
                    FinalColor = BaseColor*(1-TextureColor) + TextureEnviromentColor*TextureColor;
                    FinalAlpha = BaseAlpha * TextureAlpha
                    In other words, with GL_BLEND, if TextureAlpha	is 1 we see the TextureColor,
                and if TextureAlpha is 0.0	we see transparency.  This last comment
                depends on making the BaseColor have RGB 0.0 and on making
                the TextureEnvironmentColor have RGB 1.0.  You change
                TextureEnvironmentColor with this glTexEnvfv call. */
                    float[] environmentcolors = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
                    GL.TexEnvv(TextureEnvTarget.TextureEnv,
                        TextureEnvParameter.TextureEnvColor, environmentcolors);
                }
                else //Not usesTransparentMask.
                {
                    GL.TexEnv(TextureEnvTarget.TextureEnv,
                        TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Decal);
                    /*	In the GL_DECAL case, if we write TextureColor and 
                TextureAlpha to stand for, respectively, the color triple
                and alpha value at a given texture pixel, and if we write
                 BaseColor and BaseAlpha for, respectively, the color triple
                and alpha value at a given pixel of the polygon you plan to
                put the texture onto, THEN if 
                TextureAlpha is 1 we see the texture only, and if
                TextureAlpha is	0 we see the original color of the 
                polygon.  More precisely,
                    FinalColor = (1-TextureAlpha)*BaseColor +
                            TextureAlpha*TextureColor;
                    FinalAlpha = BaseAlpha.	*/
                }
            } 
			if ( !psprite.UsesAlpha ) 
				GL.Disable(EnableCap.AlphaTest); 
			if ( psprite.UsesAlpha ) 
			{
				GL.Enable(EnableCap.AlphaTest); 
	//			::glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA); 
			} 
			if ( !psprite.UsesLighting ) 
				GL.Disable(EnableCap.Lighting);
            if (psprite.UsesLighting )
                GL.Enable(EnableCap.Lighting);
		} 

		/* You may want to adjust things like
			activation of texture or lighting depending on the sprite. */ 
	
		public override bool selectTexture( cTexture ptexture ) /* Returns
			TRUE if the texture is selected, returns FALSE if either the ptexture is
			NULL or if an equivalent texture was already active so ptexture
			didn't need to be selected. */ 
		{ 
			if ( _ptextureactive != null ) 
			{
				if ( ptexture.TextureID == _ptextureactive.TextureID ) 
					return false; 
			}
            GL.BindTexture(TextureTarget.Texture2D, ptexture.TextureID );
				/* Or I could call ptexture->select(), which wraps this same call. */ 
			_ptextureactive = ptexture; 	
			return true; 	
		} 

		/* Returns
			TRUE if the texture is selected, returns FALSE if either the ptexture is
			NULL or if an equivalent texture was already active so ptexture
			didn't need to be selected. */ 
	
	//Special MD2 Methods 
	
		public override void selectSkinTexture( cMD2Model pmodel ) 
		{ 
			int skinfileID = pmodel.SkinFileKey;

			cTexture ptexture = _map_SkinFileIDToTexture.lookupTexture( (uint) skinfileID ); 
				/* If successful, the lookupTexture call will set the _lifetime to 
			cMapEntry_SkinFileIDToTexture.FRESHLIFESPAN to protect the thing from
			getting culled by garbageCollect. */ 
			if ( ptexture == null ) 	//We didn't find one, so we make one.
			{
				ptexture = new cTexture( pmodel.TextureInfo ); 
					/* We record a new cMapEntry_ResourceIDToTexture matching the
				resourceID and ptexture.  The constructor gives it a FRESHLIFESPAN
				as well. */ 
				_map_SkinFileIDToTexture[ (uint) skinfileID ] =
					new cMapEntry_SkinFileIDToTexture( ptexture ); 
			} 
			selectTexture( ptexture ); /* If _ptextureactive == ptexture does nothing,
			otherwise uses ::glBindTexture(GL_TEXTURE_2D, ptexture->_textureID) */ 
		} 

		
		public void CalculateNormal( float[] p1, float[] p2, float[] p3 ) 
		{ 
			
			a[ 0 ] = p1[ 0 ] - p2[ 0 ]; 
			a[ 1 ] = p1[ 1 ] - p2[ 1 ]; 
			a[ 2 ] = p1[ 2 ] - p2[ 2 ]; 
			
			b[ 0 ] = p1[ 0 ] - p3[ 0 ]; 
			b[ 1 ] = p1[ 1 ] - p3[ 1 ]; 
			b[ 2 ] = p1[ 2 ] - p3[ 2 ]; 
			
			result[ 0 ] = a[ 1 ] * b[ 2 ] - b[ 1 ] * a[ 2 ]; 
			result[ 1 ] = b[ 0 ] * a[ 2 ] - a[ 0 ] * b[ 2 ]; 
			result[ 2 ] = a[ 0 ] * b[ 1 ] - b[ 0 ] * a[ 1 ]; 
			GL.Normal3( result[ 0 ], result[ 1 ], result[ 2 ]);
 
 		} 

		
		public override void interpolateAndRender( cMD2Model pmodel, vector_t [] vlist, 
            int startframe, int endframe, float interpolationpercent ) 
		{
            int i; // index counter 
			float x1, y1, z1; // current frame point values 
			float x2, y2, z2; // next frame point values 
			mesh_t [] triangles = pmodel.TriIndex; 
			int numberoftriangles = pmodel.NumTriangles;
            texCoord_t [] texturecoords = pmodel.TextureCoords; 		// texture coordinate list 
				
				/* At one point, I tested if it would be faster here to
			call :glNewList(1, GL_COMPILE), draw into a display list object,
			and then at the end call glEndList(); glCallList(1);
			The answer is, NO, don't do this, as it cuts your speed to 30% of
			what it is otherwise.  The reason is perhaps that the move messes
			up your use of texture IDs.	Just do the triangles. */ 
		
			selectSkinTexture( pmodel ); 
			setMaterialColor( Color.White ); /* For the lighting calculations
			on the meshes, I'll treat the creatutures as as white.  I'll use
			the specular component of the lighting later, thanks to the fact
			that I've called 	::glLightModeli(GL_LIGHT_MODEL_COLOR_CONTROL,
			GL_SEPARATE_SPECULAR_COLOR); */ 
			//::glTexEnvi(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_MODULATE); 
				/* I can try using GL_MODULATE insetad of teh default GL_DECAL. */

            GL.Begin(BeginMode.Triangles);
			for ( i = 0; i < numberoftriangles; i++) 
			{ 
				// get first points of each frame 
				x1 = vlist[ startframe + triangles[ i ].meshIndex[ 0 ]].point[ 0 ]; 
				y1 = vlist[ startframe + triangles[ i ].meshIndex[ 0 ]].point[ 1 ]; 
				z1 = vlist[ startframe + triangles[ i ].meshIndex[ 0 ]].point[ 2 ]; 
				x2 = vlist[ endframe + triangles[ i ].meshIndex[ 0 ]].point[ 0 ]; 
				y2 = vlist[ endframe + triangles[ i ].meshIndex[ 0 ]].point[ 1 ]; 
				z2 = vlist[ endframe + triangles[ i ].meshIndex[ 0 ]].point[ 2 ]; 
				
				// store first interpolated vertex of triangle 
				//Note that interpol is between 0.0 and 1.0 
				vertex[ 0 ].point[ 0 ] = x1 + interpolationpercent * ( x2 - x1 ); 
				vertex[ 0 ].point[ 1 ] = y1 + interpolationpercent * ( y2 - y1 ); 
				vertex[ 0 ].point[ 2 ] = z1 + interpolationpercent * ( z2 - z1 ); 
				
				// get second points of each frame 
				x1 = vlist[ startframe + triangles[ i ].meshIndex[ 2 ]].point[ 0 ]; 
				y1 = vlist[ startframe + triangles[ i ].meshIndex[ 2 ]].point[ 1 ]; 
				z1 = vlist[ startframe + triangles[ i ].meshIndex[ 2 ]].point[ 2 ]; 
				x2 = vlist[ endframe + triangles[ i ].meshIndex[ 2 ]].point[ 0 ]; 
				y2 = vlist[ endframe + triangles[ i ].meshIndex[ 2 ]].point[ 1 ]; 
				z2 = vlist[ endframe + triangles[ i ].meshIndex[ 2 ]].point[ 2 ]; 
				
				// store second interpolated vertex of triangle 
				vertex[ 2 ].point[ 0 ] = x1 + interpolationpercent * ( x2 - x1 ); 
				vertex[ 2 ].point[ 1 ] = y1 + interpolationpercent * ( y2 - y1 ); 
				vertex[ 2 ].point[ 2 ] = z1 + interpolationpercent * ( z2 - z1 ); 
				
				// get third points of each frame 
				x1 = vlist[ startframe + triangles[ i ].meshIndex[ 1 ]].point[ 0 ]; 
				y1 = vlist[ startframe + triangles[ i ].meshIndex[ 1 ]].point[ 1 ]; 
				z1 = vlist[ startframe + triangles[ i ].meshIndex[ 1 ]].point[ 2 ]; 
				x2 = vlist[ endframe + triangles[ i ].meshIndex[ 1 ]].point[ 0 ]; 
				y2 = vlist[ endframe + triangles[ i ].meshIndex[ 1 ]].point[ 1 ]; 
				z2 = vlist[ endframe + triangles[ i ].meshIndex[ 1 ]].point[ 2 ]; 
				
				// store third interpolated vertex of triangle 
				vertex[ 1 ].point[ 0 ] = x1 + interpolationpercent * ( x2 - x1 ); 
				vertex[ 1 ].point[ 1 ] = y1 + interpolationpercent * ( y2 - y1 ); 
				vertex[ 1 ].point[ 2 ] = z1 + interpolationpercent * ( z2 - z1 ); 
				
				// calculate the normal of the triangle 
				CalculateNormal( vertex[ 0 ].point, vertex[ 2 ].point, vertex[ 1 ].point ); 
				
				// render properly textured triangle 
				GL.TexCoord2( texturecoords[ triangles[ i ].stIndex[ 0 ]].s, texturecoords[ triangles[ i ].stIndex[ 0 ]].t ); 
                GL.Vertex3( vertex[ 0 ].point );
				
				GL.TexCoord2( texturecoords[ triangles[ i ].stIndex[ 2 ]].s, texturecoords[ triangles[ i ].stIndex[ 2 ]].t ); 
                GL.Vertex3(vertex[2].point); 
				
				GL.TexCoord2( texturecoords[ triangles[ i ].stIndex[ 1 ]].s, texturecoords[ triangles[ i ].stIndex[ 1 ]].t ); 
                GL.Vertex3(vertex[1].point); 
			} 
			GL.End(); 
		} 

		
	
	
	
	//cGraphics Lighting overloads 
	
        public override void installLightingModel( cLightingModel plightingmodel = null ) 
		{ 
			if ( plightingmodel == null ) 
				EnableLighting = true; 
			else 
				EnableLighting = plightingmodel.EnableLighting; 
		
		/* At present we always add these lights, later we'll tailor this code to
	get the light info out of the plightingmodel object. */ 
		
		//1 set the material properties for objects.
            float[] materialSpecular = new float[] 
				// { 0.1f, 0.1f, 0.1f, 1.0f }; 
					//Look ok, but a bit dull with materialShininess 2 
					//Pops too hard with materialShininess 5 
				 // { 0.3f, 0.3f, 0.3f, 1.0f };  
					//Pretty nice flow with materialShininess 2 
					//Pops too hard with materialShininess 3 
				// { 0.45f, 0.45f, 0.45f, 1.0f }; 
					//Smooth lively flow with materialShininess 2.
				{ 0.6f, 0.6f, 0.6f, 1.0f }; 
					//Pops  a bit with materialShininess 2.
					//Pops hard glaring white highlights with materialShininess 15.
            float[] materialShininess = new float[1] { 2.0f }; 
				//{ 15.0f } gave too abrupt a glare  
				//was 35.0.  Bigger means smaller sized highlight.  Range is 0.0 to 128.0 
			GL.Materialv( MaterialFace.Front, MaterialParameter.Specular, materialSpecular ); 
			GL.Materialv(MaterialFace.Front, MaterialParameter.Shininess, materialShininess ); 
		//	::glMaterialfv( GL_FRONT, GL_SPECULAR, materialSpecular ); 
		//	::glMaterialfv( GL_FRONT, GL_SHININESS, materialShininess ); 
			setMaterialColor( Color.White ); //Restore to a default state.
			/*cGraphicsOpenGL.setMaterialColor is my method, and this call
		with a CN_WHITE
		argument is equivalent to
		float materialAmbDiff[4]={ 1.0f, 1.0f, 1.0f, 1.0f };
		::glMaterialfv( GL_FRONT, GL_AMBIENT_AND_DIFFUSE, materialAmbDiff );
		We alter the GL_AMBIENT_AND_DIFFUSE setting with a call to setMaterialColor
		inside each of our cGraphicsOpenGL.draw??? methods. We make these 
		setMaterialColor calls so that our objects will have intrinsic colors. */ 
		
		//2 set the LightModel settings, including global ambient lighting.
		float [] global_ambient = new float[ ] { 0.75f, 0.75f, 0.75f, 1.0f }; //fairly bright 
			GL.LightModelv(LightModelParameter.LightModelAmbient, global_ambient ); 
			//::glLightModeli(GL_LIGHT_MODEL_LOCAL_VIEWER, GL_TRUE ); 
				/* If I enable GL_LIGHT_MODEL_LOCAL_VIEWER it runs slower but the highlights are more accurate */ 
		//	::glLightModeli(GL_LIGHT_MODEL_TWO_SIDE, GL_TRUE ); 
				/* If I use the GL_LIGHT_MODEL_TWO_SIDE, I have nice light when I'm
			 like inside a teapot. */ 
				/* This next call is an OpenGL 1.2 method, and I need to include
			glext.h for it to work, as wretched Windows still ships with only
			OpenGl 1.1, trying to make us use DirectX.  Thanks, Bill... */ 
			GL.LightModel( LightModelParameter.LightModelColorControl, 
                (int) LightModelColorControl.SeparateSpecularColor ); 
		
		//3 set the lights.
		
			//3.1 Define the lights' properties.
		/* The ambient is a directionless light that's simply added in; really since we have
	a global_ambient above it's less confusing to have the indivdiual light ambients be zero.
		The diffuse is closest to the "color" of the light.
		The specular is used for color highlights, logically it can match diffuse, but
	it looks peppier if its different.
		The position, if it has 0 in the fourth coord, is specifying the (x,y,z) direction
	of the light, treated as parallel lines form infinity.  If you want to put the light in the 
	position (x,y,z) then you put 1 in the fourth coord, and then the light is viewed as
	radiating in all directions form this point. */ 
		/* Let's try and pick four directions of light with roughly one dirction comign in towards
	the origin from each vertex of a tetrahedron positioned with a point on the negative z
	axis and another point on the yz plane. 
		We'll give different colors for the different directions, for the sake of visual
	variety.  But try and make the net sum of light coming down from the positive Z be
	white. */ 
		    float [] ambient0 = new float []{ 0.1f, 0.1f, 0.1f, 1.0f }; 
				    //Only need to turn ONE light's ambience on, 
		    float [] diffuse0 = new float[] { 0.1f, 0.1f, 0.4f, 1.0f }; //Blue 
		    float [] specular0 = new float[] { 0.1f, 0.1f, 0.4f, 1.0f }; 
		    float [] position0 = new float[] { -1.0f, -0.5f, -1.0f, 0.0f }; //Dir to origin from hix, hiy, hiz.
    		
		    float [] ambient1 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f }; 
		    float [] diffuse1 = new float[] { 0.4f, 0.1f, 0.1f, 1.0f }; //Red 
		    float [] specular1 = new float[] { 0.4f, 0.1f, 0.1f, 1.0f }; 
		    float [] position1 = new float[] { 1.0f, -0.5f, -1.0f, 0.0f }; //Dir to origin from lox, hiy, hiz.
    		
		    float [] ambient2 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f }; 
		    float [] diffuse2 = new float[] { 0.1f, 0.4f, 0.1f, 1.0f }; //Green 
		    float [] specular2 = new float[] { 0.1f, 0.4f, 0.1f, 1.0f }; 
		    float [] position2 = new float[] { 0.0f, 1.0f, -1.0f, 0.0f }; //Dir to origin from zero, loy, hiz 
    		
		    float [] ambient3 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f }; 
		    float [] diffuse3 = new float[] { 0.25f, 0.250f, 0.25f, 1.0f }; //White 
		    float [] specular3 = new float[] { 0.25f, 0.25f, 0.25f, 1.0f }; 
		    float [] position3 = new float[] { 0.0f, 0.0f, 1.0f, 0.0f }; //Dir to origin from zero, zero, loz 
			    /* 3.2 turn off all lights.  There are least eight by default, and possibly more in
		    some implemetnations. */ 
		    GL.Disable(EnableCap.Light0);
            GL.Disable(EnableCap.Light1);
            GL.Disable(EnableCap.Light2);
            GL.Disable(EnableCap.Light3);
            GL.Disable(EnableCap.Light4);
    		
			    //3.3 Enable your lights 
			    //Enable Light0 
			    //Always have at least one light.
		    GL.Enable(EnableCap.Light0); 
		    GL.Lightv(LightName.Light0, LightParameter.Ambient, ambient0 ); 
		    GL.Lightv(LightName.Light0, LightParameter.Position, position0 ); 
		    GL.Lightv(LightName.Light0, LightParameter.Diffuse, diffuse0 ); 
		    GL.Lightv(LightName.Light0, LightParameter.Specular, specular0 ); 
		    //Enable Light1 
		    if ( NUMBEROFLIGHTS > 1 ) //NUMBEROFLIGHTS is between 1 and 4.
		    {
                GL.Enable(EnableCap.Light1);
                GL.Lightv(LightName.Light1, LightParameter.Ambient, ambient1);
                GL.Lightv(LightName.Light1, LightParameter.Position, position1);
                GL.Lightv(LightName.Light1, LightParameter.Diffuse, diffuse1);
                GL.Lightv(LightName.Light1, LightParameter.Specular, specular1); 
            } 
		    //Enable Light2 
		    if ( NUMBEROFLIGHTS > 2 ) //NUMBEROFLIGHTS is a #define between 1 and 4.
		    {
                GL.Enable(EnableCap.Light2);
                GL.Lightv(LightName.Light2, LightParameter.Ambient, ambient2);
                GL.Lightv(LightName.Light2, LightParameter.Position, position2);
                GL.Lightv(LightName.Light2, LightParameter.Diffuse, diffuse2);
                GL.Lightv(LightName.Light2, LightParameter.Specular, specular2);
            } 
		    //Enable Light3  
		    if ( NUMBEROFLIGHTS > 3 ) //NUMBEROFLIGHTS is a #define between 1 and 4.
		    {
                GL.Enable(EnableCap.Light3);
                GL.Lightv(LightName.Light3, LightParameter.Ambient, ambient3);
                GL.Lightv(LightName.Light3, LightParameter.Position, position3);
                GL.Lightv(LightName.Light3, LightParameter.Diffuse, diffuse3);
                GL.Lightv(LightName.Light3, LightParameter.Specular, specular3);
            } 
	    } 

		
			/* This gets called by CpopView::setGraphicsClass in the form
			pgraphics()->installLightingModel(pgame()->plightingmodel()).  For now, 
			installLightingModel does nothing in MFC and sets up some default standard
			lights in OpenGL.  Eventually the cLightingModel will have useful fields,
			but at present all it has is a BOOL _enablelighting. I only have the
			default NULL argument so I can call this in the cGraphicsOpenGL constructor. */

        public override bool EnableLighting
        {
            set
            {
                if (value)
                    GL.Enable(EnableCap.Lighting);
                else
                    GL.Disable(EnableCap.Lighting);
            }
        }


		/* Use enableLighting to turn on and off the OpenGL 
			lighting model. */ 
	//Display list overloads 

        public override bool SupportsDisplayList
        {
            get
                { return true; }
        }
		
		public override bool activateDisplayList( cSprite psprite ) /* This call gets an
		_activeDisplayListID based on the psprite's spriteID and newgeometryflag.  If the
		list has already been recorded,	we set _activeDisplayListIDIsReady to TRUE,
		otherwise we set _activeDisplayListIDIsReady to FALSE.  And then we return
		_activeDisplayListIDIsReady. */ 
		{ 
			bool newgeometryflag = psprite.NewGeometryFlag; 
			_activeDisplayListID = /* Set the cGraphicsOpenGL field _activeDisplayListID so we can
				reference it in the cGraphicsOpenGL.callDisplayList(). */ 
			_map_SpriteIDToDisplayListID.lookupDisplayListID( psprite.SpriteID); 
			if ( _activeDisplayListID != cGraphicsOpenGL.INVALIDDISPLAYLISTID && //nonzero means its a valid ID.
				!( psprite.NewGeometryFlag )) //sprite geometry hasn't been changed 
					return _activeDisplayListIDIsReady = true; //Means you have a valid, playable _activeDisplayListID 
		/* If you get here either its a new sprite, or the sprite has a newgeometryflag. If
	you have an entry, but it's no good anymore, you have to get rid of it three 
	different ways: Remove it from the CList, invalidate its OpenGL list, and delete
	it from memory. */ 
			if ( _activeDisplayListID != cGraphicsOpenGL.INVALIDDISPLAYLISTID && //nonzero means its a valid ID.
				( psprite.NewGeometryFlag )) //You have an entry, but it's no good anymore.
			{ 
                GL.DeleteLists(_activeDisplayListID, 1);
				_map_SpriteIDToDisplayListID.Remove( psprite.SpriteID ); 
			} 
			_activeDisplayListID = (uint) GL.GenLists( 1 ); //Get the next display list ID from OpenGL 
			/* Turn on the display list recorder.  It's OK to have it on while you do the 
		array setup stuff, as the only commands the recorder will catch are the gl and glu calls. */
            GL.NewList(_activeDisplayListID, ListMode.Compile);
				/* Start recording the display list after this call. */ 
			return _activeDisplayListIDIsReady = false; //Means you must still record onto the _activeDisplayListID 
		} 

		/* This call gets an
			_activeDisplayListID based on the psprite's spriteID and newgeometryflag.  If the
			list has already been recorded,	we set _activeDisplayListIDIsReady to TRUE,
			otherwise we set _activeDisplayListIDIsReady to FALSE.  And then we return
			_activeDisplayListIDIsReady. If the return is FALSE, you need to call a drawsomething
			method before calling callActiveDisplayList. */ 
	
		public override void callActiveDisplayList( cSprite psprite ) 
		{ 
			if ( !_activeDisplayListIDIsReady ) 	
			{
                GL.EndList(); 
				_map_SpriteIDToDisplayListID[ psprite.SpriteID ] = 
					new cMapEntry_SpriteIDToDisplayListID( (int) _activeDisplayListID ); 
				/* I once had a memory leak from this line, apparantly my
				cMap_SpriteIDToDisplayListID.free() wasn't
				killing all of the cMapEntry_SpriteIDToDisplayListID* in
				the destructor call for _map_SpriteIDToDisplayListID, which cascades
				out of the cGraphicsOpenGL destructor. To see the leak, start in 
				cGameSpacewar with windows graphics and polypolygons for the asteroid sprites,
				switch to Use View|OpenGL Grpahics, and close. */ 
			}
            GL.CallList(_activeDisplayListID);
		/* Note that when we were doing the GL_COMPILE Of the display list ID, none
	of the ::gl commands were actually executed as in immediate mode.  So to actually
	see anything this first time we need to "play the tape" of the display list
	commands. Normally this dosn't matter, as most sprites get drawn over and 
	over, but in the case where you're doing a one-shot draw of a temporary
	sprite variable, this is an issue, for if we dont do the glCallList, then
	you won't see anything. */ 
		} 

		/* If _activeDisplayListIDIsReady is
			FALSE,	we close the list and add it to the _map_SpriteIDToDisplayListID.  And then in
			any case we call ::glCallList(_activeDisplayListID). */ 
	//Other graphics overloads 
	
		public override void setClearColor( Color color ) 
		{ 
			float r = color.R / 255.0f; 
			float g = color.G / 255.0f; 
			float b = color.B / 255.0f; 
			GL.ClearColor( r, g, b, 1.0f ); 
		} 

		
		public override void clear( CRect clearrect ) 
		{ 
			//OpenGL has no use for the clearrect argument.
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit ); 
		} 

		
		public override void setClipRegion( cRealBox3 pclipbox ){}

        public override void setMaterialColor(Color color, float alpha = 1.0f ) 
		{ 
			float r = color.R / 255.0f; 
			float g = color.G / 255.0f; 
			float b = color.B / 255.0f; 
			float [] materialAmbDiff = new float[] { r, g, b, alpha };
            GL.Materialv(MaterialFace.Front, MaterialParameter.AmbientAndDiffuse, materialAmbDiff);
            GL.Color4(r, g, b, alpha); 
		} 

		public override void setMaterialColorFrontAndBack( Color color, float alpha = 1.0f ) 
		{ 
			float r = color.R / 255.0f; 
			float g = color.G / 255.0f; 
			float b = color.B / 255.0f; 
			float [] materialAmbDiff = new float[] { r, g, b, alpha };
            GL.Materialv(MaterialFace.FrontAndBack, MaterialParameter.AmbientAndDiffuse,
                materialAmbDiff);
            GL.Color4(r, g, b, alpha);
        } 

		
	//cGraphicsOpenGL methods that haven't been migrated into cGraphics yet...
	//Color methods 
		
        public void setVertexColor( Color color, float alpha = 1.0f ) 
		{ 
			float r = color.R / 255.0f; 
			float g = color.G / 255.0f; 
			float b = color.B / 255.0f; 
			GL.Color4( r, g, b, alpha ); 
		} 

        public void setVertexColorDim( Color color, float dimfactor = 0.5f, float alpha = 1.0f ) 
		{
            float r = dimfactor * color.R / 255.0f;
            float g = dimfactor * color.G / 255.0f;
            float b = dimfactor * color.B / 255.0f;
            if (r < 0.0f)
                r = 0.0f;
            else if (r > 1.0f)
                r = 1.0f;
            if (g < 0.0f)
                g = 0.0f;
            else if (g > 1.0f)
                g = 1.0f;
            if (b < 0.0f)
                b = 0.0f;
            else if (b > 1.0f)
                b = 1.0f;
            GL.Color4(r, g, b, alpha);
        } 

		
			//Set a color but reduce all intensity by dimfactor.

        public void setMaterialColorDim( Color color, float dimfactor = 0.5f ) 
		{ 
			float r = dimfactor * color.R / 255.0f; 
			float g = dimfactor * color.G / 255.0f; 
			float b = dimfactor * color.B / 255.0f;
            if (r < 0.0f)
                r = 0.0f;
            else if (r > 1.0f)
                r = 1.0f;
            if (g < 0.0f)
                g = 0.0f;
            else if (g > 1.0f)
                g = 1.0f;
            if (b < 0.0f)
                b = 0.0f;
            else if (b > 1.0f)
                b = 1.0f;
            float[] materialAmbDiff = new float[] { r, g, b, 1.0f };
            GL.Materialv(MaterialFace.Front, MaterialParameter.AmbientAndDiffuse,
                materialAmbDiff);
            GL.Color4(r, g, b, 1.0f);
        } 

		
	//Vertex methods 
	
		public void setGLVertex( cVector2 vert ){ GL.Vertex2( vert.X, vert.Y );} //Assumes float.
		
		public void setGLVertex( cVector3 vert ) 
			{ GL.Vertex3( vert.X, vert.Y, vert.Z );} //Assumes float.
		
	} 
}