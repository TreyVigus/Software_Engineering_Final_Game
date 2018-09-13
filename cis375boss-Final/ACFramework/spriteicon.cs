// For AC Framework 1.2, default parameters were added -- JC


using System;
using System.Drawing;

namespace ACFramework
{ 
	
	class cSpriteIcon : cSprite 
	{ 
		protected bool _transparent; /*Usually _transparent is TRUE, but if we use a cSpriteIcon for, say,
			a backgroudn bitmap, we might want _transparent to be FALSE. */ 
		protected bool _tiled; 
		protected float _aspect; /* The ratio of the width to the height.  Gets set when you first
			set up the image to match the _resourceID in the cGraphics object. */ 
		protected float _sizex, _sizey; //These are helpers kept in synch with _aspect and _radius.
		protected bool _imageloaded; /* The first time we draw the icon and actually load its image,
			we set the _aspect to match the image unless presetaspect is TRUE. */ 
		protected bool _presetaspect; /* If _presetaspect holds, then the first time we load the image,
			we crop the image so that its size matches the _aspect. */ 
		protected float _visualradius; /* Maintain a _visualradius smaller than _radius
			which is the radius of the rectangle out to the corners.  A typical
			bitmap might not go out to the corners of the rect its in, but rather out
			to its sides.  Let's try the average of the distance to the sides. */ 
		protected int _xtilecount;
        protected Color _bitmapcolor;
		
		public cSpriteIcon()  
		{ 
			_transparent = true; 
			_tiled = false; 
			_xtilecount = 1; 
			_imageloaded = false; 
			_presetaspect = false; 
			Filled = false; 
			Aspect = 1.0f; //Sets _sizex and _sizey to match _radius which was set in cSprite::cSprite.
		} 

        public cSpriteIcon( int resourceID, bool transparent = true )  
		{ 
			_transparent = transparent; 
			_tiled = false; 
			_xtilecount = 1; 
			_imageloaded = false; 
			_presetaspect = false; 
			_visualradius = 1.0f; 
            _resourceID = resourceID; 
			Filled = false; 
			Aspect = 1.0f; 
				//Sets _sizex, _sizey, _visualradius to match _radius which was set in cSprite::cSprite.
		} 
		
		public cSpriteIcon( int resourceID, bool transparent, bool presetaspect )  
		{ 
			_transparent = transparent; 
			_tiled = false; 
			_xtilecount = 1; 
			_imageloaded = false; 
			_presetaspect = false;  // this was set to false in Rucker's code, left as is -- JC 
			_visualradius = 1.0f; 
            _resourceID = resourceID; 
			Filled = false; 
			Aspect = 1.0f; 
				//Sets _sizex, _sizey, _visualradius to match _radius which was set in cSprite::cSprite.
		} 
		
	//Accessors 
	
		/* Overload to returns something based on
			_visualradius. */ 
	

		public void setSize( float sizex, float sizey ) 
		{ 
			if ( sizey < 0.00001f ) 
				return ; 
			_sizex = sizex; 
			_sizey = sizey; 
			_radius = (float) Math.Sqrt( _sizex * _sizex + _sizey * _sizey ); 
			_aspect = _sizex / _sizey; 
			Aspect = _aspect; //Fix _visualradius.
		} 

		
	//cSprite overloads 
		

		
		public override void copy( cSprite psprite ) //Use this in copy constructor and operator= 
		{ 
			base.copy( psprite ); 
			if ( !psprite.IsKindOf( "cSpriteIcon" )) 
				return ; //You're done if psprite isn't a cSpriteIcon*.
			cSpriteIcon picon = ( cSpriteIcon )( psprite ); /* I know it is a
			cSpriteIcon at this point, but I need to do a cast, so the compiler will 
			let me call cSpriteIcon methods. */ 
			_transparent = picon._transparent; 
			Aspect = picon._aspect; //Sets _aspect, _sizex, _sizey, _visualradius 
			_presetaspect = picon._presetaspect;
            _bitmapcolor = picon._bitmapcolor;
            _xtilecount = picon._xtilecount;
            _tiled = picon._tiled;
		}
 
        public override cSprite copy( )
        {
            cSpriteIcon s = new cSpriteIcon();
            s.copy(this);
            return s;
        }

        public override bool IsKindOf( string str )
        {
            return ( str == "cSpriteIcon" ) || base.IsKindOf( str );
        }

		
	//The imagedraw function 
	
		public override void imagedraw( cGraphics pgraphics, int drawflags = 0 ) 
		{
			if ( (drawflags & ACView.DF_WIREFRAME) == 0 ) 
				pgraphics.drawbitmap( this, drawflags ); 
			else //we're in wireframe mode 
				pgraphics.drawXYrectangle( LoCorner, HiCorner, 
					pcolorstyle(), drawflags ); 
		} 
    
		public virtual float Aspect
		{
			get
				 { return _aspect; }
            set
            {
			    /* We write the code to preserve two equations: (a) sizex/sizey = aspect and
		        (b) sqrt(sizex^2 + sizey^2) = diameter = 2 * radius;
		        (a) says sizex = aspect * sizey.  Substituting this into (b) and squaring both 
		        sides I get (aspect^2 + 1)*sizey^2 = 4 * radius^2.  Solving this, I get
		        sizey = 2 * radius * sqrt(1/(1+aspect^2))
		        */ 
			    _aspect = Math.Abs( value ); //Don't allow negative aspect.
			    _sizey = 2.0f * _radius * (float) Math.Sqrt( 1.0f /( 1.0f + _aspect * _aspect )); 
			    _sizex = _aspect * _sizey; 
		        //	_visualradius = 0.5 * (0.5 * __min(_sizex, _sizey)) + _radius); 
			    _visualradius = 0.25f *( _sizex + _sizey ); //Avg of distances to sides.
			    _newgeometryflag = true;
            }
		} 

		public virtual float Sizex
		{
			get
				{ return _sizex; }
		}

		public virtual float Sizey
		{
			get
				{ return _sizey; }
		}

		public virtual float Lox
		{
			get
				{ return - _sizex * 0.5f;}
		}

		public virtual float Loy
		{
			get
				{ return - _sizey * 0.5f;}
		}

		public override float Radius
		{
			get
    		{ 
	    		return _spriteattitude.ScaleFactor * _visualradius; 
		    }
            set
		    { 
			    /* Let's assume that when you call this, your goal is to match the visualradius to 
		        the radius argument. */ 
			    float radiustovisual = _radius / _visualradius; 
			    _radius = radiustovisual * value; 
			    Aspect = _aspect; //Fix _sizex and _sizey.
		    } 
		}

        public virtual Color BitmapColor
		{
			get
			{
                return _bitmapcolor;
            }
		}

		public virtual cVector3 LoCorner
		{
			get
				{ return new cVector3(-_sizex * 0.5f, -_sizey * 0.5f );}
		}

		public virtual cVector3 HiCorner
		{
			get
				{ return new cVector3( _sizex * 0.5f, _sizey * 0.5f );}
		}

		public virtual bool Tiled
		{
			get
				{ return _tiled; }
			set
				{ _tiled = value; }
		}

		public virtual int XTileCount
		{
			get
				{ return _xtilecount; }
		}

		public virtual bool ImageLoaded
		{
			get
				{ return _imageloaded; }
			set
				{ _imageloaded = value; }
		}

		public virtual bool PresetAspect
		{
			get
				{ return _presetaspect; }
		}

        /* enabledisplaylist specifies whetehr or not you want to
            try and use a display list to show this kind of sprite. By
            default this is TRUE.  We make it FALSE because the
            cSpriteIcon actually runs slower with displaylists, so we
            want to be able turn off display lists for them.  
              Using display lists for the texture-implemeneted bitmaps 
            of cSpriteIcon garners me no noticeable speed.  The fact
            that the cTexture is already a texture object must meant 
            that you've already gotten as much as you can out of that.
            All that using a display list in here does is save the time
            of making a few GL mode setting calls.  If anything, this 
            runs maybe 20% slower with the display list, probably
            because the display list call overhead outweighs making
            the GL mode sets in immediate mode. But test this on your
            own machine by making the TRUE if you like.  */
        public override bool EnabledDisplayList
		{
			get
				 { return false; }
		}

		public override bool UsesTexture
		{
			get
				{ return true; }
		}

		public override bool UsesTransparentMask
		{
			get
				 { return _transparent; }
		}

		public override bool UsesAlpha
		{
			get
				{ return _transparent; }
		}

		public override bool UsesLighting
		{
			get
				{ return !_transparent; }
		}


    } 
	
	class cSpriteIconBackground : cSpriteIcon 
	{ 
		
		public cSpriteIconBackground(){} 
		
        public cSpriteIconBackground( int resourceID, cRealBox2 borderrect, int xtilecount = 1 ) 		
        { 
			_xtilecount = xtilecount; 
			if ( _xtilecount > 1 ) 
				_tiled = true; 
			_transparent = false; 
			_presetaspect = true; 
			_resourceID = resourceID; 
			_radius = borderrect.Radius; 
			_sizex = borderrect.XSize; 
			_sizey = borderrect.YSize; 
			_aspect = ( float ) _sizex /( float ) _sizey; 
			_visualradius = 0.25f *( _sizex + _sizey ); //Avg of distances to sides.
		}

        public cSpriteIconBackground(int resourceID, cRealBox2 borderrect, 
            int xtilecount, Color color)
        {
            _xtilecount = xtilecount;
            if (_xtilecount > 1)
                _tiled = true;
            _transparent = false;
            _presetaspect = true;
            _resourceID = resourceID;
            _radius = borderrect.Radius;
            _sizex = borderrect.XSize;
            _sizey = borderrect.YSize;
            _aspect = (float)_sizex / (float)_sizey;
            _visualradius = 0.25f * (_sizex + _sizey); //Avg of distances to sides.
            _bitmapcolor = color;
        }

        public override cSprite copy()
        {
            cSpriteIconBackground s = new cSpriteIconBackground();
            s.copy(this);
            return s;
        }

        public override bool IsKindOf( string str )
        {
            return ( str == "cSpriteIconBackground") || base.IsKindOf( str );
        }
	}
}
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          