// For AC Framework 1.2, ZEROVECTOR and other vectors were removed,
// default parameters were added -- JC

using System;
using System.Drawing;

namespace ACFramework
{ 
	// Graphics.h : interface of the cGraphics class 
	// 
	///////////////////////////////////////////////////////////////////////////// 
	// begin Rucker's comment
    
    /*  This is a base class used to derive cGraphicsOpenGL from.
		Certainly OpenGL is slower on a system that is not using hardware OpenGL acceleration. 
	(Note that to get acceleration, you (a) have to have a graphics card with an OpenGL acceleator and 
	(b) have to have your machine set to a graphics mode in which the card accelerates OpenGL: normally 
	16-bit color (65K colors) or "True Color" are accelerated, but other modes may not be. You can tell
	if you are getting hardware acceleration by looking at the messsages in the Output window when running
	in the Debug mode.  If the message says you are using an OpenGL implementation by Microsoft, then
	you aren't getting acceleration, if the message says  you are using an implmenetation by the
	maker of your graphics card, then you are getting accelerationg.  See
	http://www.opengl.org/developers/faqs/technical/mswindows.htm for more about getting hardware acceleration.
		Eventually it would be nice to allow the use of DirectX graphics as another option. */ 
    // end Rucker's comment

    // begin Childs comment

    /*  In the AC Framework, there is only one useful graphics class, that is cGraphicsOpenGL.  In the Pop
     * framework, there were two that inherited from cGraphics: cGraphicsMFC and cGraphicsOpenGL.  cGraphicsMFC
     * was for 2D games, which the AC Framework does not try to implement.  It also used MFC programming
     * extensively, so there was no use for it in C#.  I could have abolished the cGraphics base class since
     * there was only one useful graphics class in the AC Framework, but I decided not to.  Someday, someone
     * might want to make a DirectX graphics class (or some other graphics class), so the cGraphics base class 
     * still makes sense. -- JC
     */
 
    // end Childs comment
     

	


	class cGraphics 
	{ 
		public static readonly short MODELVIEW = 0; 
		public static readonly short PROJECTION = 1; 
		public static readonly short TEXTURE = 2; 
		protected cTexture _ptextureactive; 
		
		public cGraphics() 
        {
            _ptextureactive = null;
        }
		
		public virtual bool is3D(){ return false; } /* This will return TRUE for cGraphics classes which
			render in 3D and for which the viewpoint does more than stare straight down ortho style. */ 
	
	//Matrix Methods 

        public virtual short MatrixModeProperty
        {
            set     // this is just for overriding
            {
            }
        }
		
		public virtual void loadMatrix( cMatrix3 matrix ){} 
		
		public virtual void loadIdentity(){} 
		
		public virtual void pushMatrix(){} 
		
		public virtual void popMatrix(){} 
		
		public virtual void multMatrix( cMatrix3 rightmatrix ){} 
		
		public virtual void translate( cVector3 translation ){} 
	//Perspective Matrix methods 
		
		public virtual void ortho( float left, float right, float bottom, float top, float nearzclip, float farzclip ){} 
		
		public virtual void perspective( float fieldofviewangleindegrees, float xtoyaspectratio, float nearzclip, float farzclip ){} 
		
		public virtual void frustum( float l, float r, float b, float t, float n, float f ) {} 
	//Special Methods 
		
		public virtual void garbageCollect(){} /*Release all the not-recently-used 
			stored textures and displaylist IDs. */ 
		
		public virtual void free(){} /* Release all the stored textures and 
			displaylist IDs */ 
		
		public virtual void activate(){} 
		
		public virtual void display( ){} 
		
		public virtual void setRealBox( cRealBox3 border ){} 
		
		public virtual void setClearColor( Color colorref ){} 
		
		public virtual void clear( CRect clearrect ){} 
		
		public virtual void vectorToPixel( cVector3 position, out int xpix, out int ypix, out float zbuff )
        {
            xpix = ypix = 1;
            zbuff = 1.0f;
        } 
		
        public virtual cVector3 pixelToVector( int xpix, int ypix, float zbuff = 0.0f )
        { 
            return new cVector3();
        } 
		
		public virtual cLine pixelToSightLine( int xpix, int ypix ) //This is virtual, cGraphicsMFC does different 
		{ /* This is a line that runs from the viewer's eye to the direction matching the pixel point. */ 
			cVector3 nearpoint = pixelToVector( xpix, ypix, 0.0f ); 
			cVector3 farpoint = pixelToVector( xpix, ypix, 1.0f ); 
			cVector3 tangent = farpoint.sub( nearpoint );
            if (tangent.IsPracticallyZero)
                tangent = new cVector3(0.0f, 0.0f, -1.0f);
            else
                tangent.normalize(); 
			return new cLine( nearpoint, tangent ); //Second arg should be a unit vector.
		} 

		/* This is a line that runs
			from the viewer's eye to the direction matching the pixel point. This whole line is
			projected as a point at the pixel point.  When pickign a critter, we look for the ones
			close to the pixelToSightLine for the screen pick pixel. I put in a default implementation
			to do it the "right" way in 3D, and I overload this for cGrahicsMFC to make 
			the  direction of the line just be the negative Z axis. */ 
	
		public cVector3 pixelAndPlaneToVector( int xpix, int ypix, cPlane plane ) 
		{ 
			return plane.intersect( pixelToSightLine( xpix, ypix )); 
				//Find the line of sight and intersect it with the plane.
		} 

		/* This 
			method unprojects to the near and far possiblities for the pixel position, draws the "sight
			line" between the two (I call it a "sight line" because these are all positions that 
			get projected into the same pixel), and finds the point where the line crosses the plane.
			This method is useful if you know what plane you are trying to pick in; often, for instance,
			you want to pick a point in the plane of the player. It uses the virtual pixelToSightLine
			method. */ 	

        public CRect realBoxToCRect( cRealBox3 realbox ) 
		{ 
			int intlox, intloy, inthix, inthiy; 
			float zbuff; //dummy to ignore 
			vectorToPixel( new cVector3( realbox.Lox, realbox.Loy), out intlox, out intloy, out zbuff ); 
			vectorToPixel( new cVector3( realbox.Hix, realbox.Hiy), out inthix, out inthiy, out zbuff ); 
			return new CRect( intlox, inthiy, inthix, intloy ); /* The CRect constructor expects
			args in the left, top, right, bottom order. */ 
		} 
		
		public virtual Color sniff( cVector3 sniffpoint ){ return Color.FromArgb(0);} 
	//Texture methods 

        public virtual cTexture ActiveTexture
        {
            get
                { return _ptextureactive; }
        }
		
		public virtual bool selectTexture( cTexture ptexture ){ return false; } /* Returns
			TRUE if the texture is selected, returns FALSE if either the ptexture is
			NULL or if an equivalent texture was already active so ptexture
			didn't need to be selected. */ 
	//Graphics overloadables 
		
		public virtual void setViewport( int width, int height ){} /* CpopView calls this in OnSize to set the
			pixel size of the view window. */ 
		
		public virtual void adjustAttributes( cSprite psprite ){} /* You may want to adjust things like
			activation of texture or lighting depending on the sprite. */ 
		
		public virtual void setMaterialColor( Color color, float alpha = 1.0f ){} 

        public virtual void setMaterialColorFrontAndBack( Color color, float alpha = 1.0f ){} 

        public virtual void drawline( cVector3 posa, cVector3 posb, 
            cColorStyle pcolorstyle ){} //reallinewidth 0 means 1 pixel wide.
		
		public void drawXYrectangle( cVector3 cornerlo, cVector3 cornerhi, 
            cColorStyle pcolorstyle, int drawflags ) 
		{ 
			drawrectangle( cornerlo, new cVector3( cornerhi.X, cornerlo.Y, cornerlo.Z), 
				cornerhi, new cVector3( cornerlo.X, cornerhi.Y, cornerlo.Z), 
				pcolorstyle, drawflags ); 
		} 

		/* Turns the 2 points into 4 and calls the virtual
			drawrectangle method that uses four corners.  The 2 corner call assumes
			the rectangle has its z values averaged between
			cornerlo and cornerhi and traverses the 4 corners starting out heading from 
			cornerlo.x() to cornerhi.x(). */ 
	
		public virtual void drawrectangle( cVector3 corner0, cVector3 corner1, 
            cVector3 corner2, cVector3 corner3, cColorStyle pcolorstyle, 
            int drawflags ){} //Defined in indivdiual graphics files.
		
		public virtual void drawcircle( cVector3 center, float radius, 
            cColorStyle pcolorstyle, int drawflags ){} 
		
		public virtual void drawpolygon( cPolygon ppolygon, int drawflags ){} 
		
		public virtual void drawstarpolygon( cPolygon ppolygon, int drawflags ){} 
			//Need to handle non-_convex polygons differently in OpenGL 
		
		public virtual void drawbitmap( cSpriteIcon picon, int drawflags ){} 
		
		public virtual void selectSkinTexture( cMD2Model pmodel ){} 
		
		public virtual void interpolateAndRender( cMD2Model pmodel, vector_t [] vlist, 
            int startframe, int endframe, float interpolationpercent) { } 
		
        public virtual void drawtext( string str, int pixx = 0, int pixy = 0 ){} 

    //cGraphics text overloads 
		
		public virtual void use3DText( bool yesno ){} 
	//cGraphics Lighting overloads 
		
		public virtual void installLightingModel( cLightingModel plightingmodel = null ){} 
			/* This gets called by CpopView::setGraphicsClass in the form
			pgraphics()->installLightingModel(pgame()->plightingmodel()).  For now, 
			installLightingModel does nothing in MFC and sets up some default standard
			lights in OpenGL.  Eventually the cLightingModel will have useful fields,
			but at present all it has is a BOOL _enablelighting. I only have the
			default NULL argument so I can call this in the cGraphicsOpenGL constructor. */

        public virtual bool EnableLighting
        {
            set  // this is just for overriding
            {
            }
        }/* Use enableLighting to turn on and off the OpenGL 
			lighting model. */ 
	//Display list overloads 

        public virtual bool SupportsDisplayList
        {
            get
                { return false; }
        }
		
		public virtual bool activateDisplayList( cSprite psprite ){ return false; } /* In sophisticated graphics
			implementations, this call gets an unsigned int _activeDisplayListID based on the
			psprite's spriteID and newgeometryflag.  If the list has already been recorded,	we set 
			a BOOL _activeDisplayListIDIsReady to TRUE, otherwise we set _activeDisplayListIDIsReady 
			to FALSE.  And then we return _activeDisplayListIDIsReady. If the return is FALSE,
			you need to call a drawsomething method before calling callActiveDisplayList. */ 
		
		public virtual void callActiveDisplayList( cSprite psprite ){} /* In sophisticated graphics
			implementations, if _activeDisplayListIDIsReady is FALSE, we close the list and 
			add it to the _map_SpriteIDToDisplayListID.  And then in any case we call
			a graphics method to call the list. */ 
	//Other graphics overloads 
		
		public virtual void setClipRegion( cRealBox3 pclipbox ){} 
		
	} 
	
	/* The cLightingModel prototype below is just a start on the class.
	Eventually it should have information about the locations of lights,
	the ambient, diffuse, and specular paraemters for their three colors.
	I think the materials parameters need to be part of cColorStyle.
	cLightingModel should have params for all the calls now made in the 
	cGraphicsOpenGL::installLightingModel(cLightingModel *plightingmodel).
	*/ 
	class cLightingModel 
	{ 
		protected bool _enablelighting; 

		public cLightingModel( bool enablelighting = true )
        {
            _enablelighting = enablelighting;
        }

        public virtual bool EnableLighting
        {
            get
                { return _enablelighting; }
            set
                { _enablelighting = value; }
        }
		
	} 
}