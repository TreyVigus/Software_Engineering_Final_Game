// For AC Framework 1.2, default parameters were added -- JC

using System;
using System.Drawing;

namespace ACFramework
{ 
	
	class cSpriteTextureBox : cSpriteComposite 
	{ 
		protected cRealBox3 _pskeleton; 
		
		protected void _initialize() 
		{ 
			cSpriteRectangle prect; 
			_childspriteptr.RemoveAll(); 
			for ( int boxside = 0; boxside < 6; boxside++) 
		//	for (int boxside = 0; boxside < 1; boxside++) //Used this instead for debuggin 
				//Force in 6 sprites.
			    switch( boxside ) 
			    { 
			    case 0: 
				    prect = new cSpriteRectangle( Loy, Loz, Hiy, Hiz ); 
				    add( prect ); 
				    break; 
			    case 1: 
				    prect = new cSpriteRectangle( Loy, Loz, Hiy, Hiz ); 
				    add( prect ); 
				    break; 
			    case 2: 
				    prect = new cSpriteRectangle( Lox, Loz, Hix, Hiz ); 
				    add( prect ); 
				    break; 
			    case 3: 
				    prect = new cSpriteRectangle( Lox, Loz, Hix, Hiz ); 
				    add( prect ); 
				    break; 
			    case 4: 
				    prect = new cSpriteRectangle( Lox, Loy, Hix, Hiy ); 
				    add( prect ); 
				    break; 
			    case 5: 
				    prect = new cSpriteRectangle( Lox, Loy, Hix, Hiy ); 
				    add( prect ); 
				    break; 
			    } 
			_fixspriteattitudes(); 
		} 

		
		protected void _fixspriteattitude( int boxside ) 
		{ 
			/* Adjust each case so the z-axis direction faces out. */ 
			switch( boxside ) 
			{ 
			case 0 : 
				_childspriteptr[ 0 ].SpriteAttitude =  
					cMatrix3.translation( new cVector3( Lox, Midy, Midz)).mult( 
						cMatrix3.xRotation((float) -Math.PI / 2.0f )).mult( 
						cMatrix3.yRotation( (float) Math.PI / 2.0f )).mult( 
						cMatrix3.zRotation( (float) Math.PI )); 
				break; 
			case 1 : 
				_childspriteptr[ 1 ].SpriteAttitude = 
					cMatrix3.translation( new cVector3( Hix, Midy, Midz)).mult( 
						cMatrix3.xRotation( (float) Math.PI / 2.0f )).mult( 
						cMatrix3.yRotation( (float) Math.PI / 2.0f )); 
				break; 
			case 2 : 
				_childspriteptr[ 2 ].SpriteAttitude = 
					cMatrix3.translation( new cVector3( Midx, Loy, Midz)).mult( 
						cMatrix3.zRotation( (float) Math.PI / 2.0f )).mult( 
						cMatrix3.yRotation( (float) Math.PI / 2.0f )).mult( 
						cMatrix3.zRotation( (float) Math.PI / 2.0f ) 
						); 
				break; 
			case 3 : 
				_childspriteptr[ 3 ].SpriteAttitude = 
					cMatrix3.translation( new cVector3( Midx, Hiy, Midz)).mult( 
						cMatrix3.zRotation( 3 * (float) Math.PI / 2.0f )).mult( 
						cMatrix3.yRotation( (float) Math.PI / 2.0f )).mult( 
						cMatrix3.zRotation( (float) Math.PI / 2.0f ) 
						); 
				break; 
			case 4 : 
				_childspriteptr[ 4 ].SpriteAttitude = 
					cMatrix3.translation( new cVector3( Midx, Midy, Loz)).mult( 
					cMatrix3.yRotation( (float) Math.PI )); 
				break; 
			case 5 : 
				_childspriteptr[ 5 ].SpriteAttitude = 
					cMatrix3.translation( new cVector3( Midx, Midy, Hiz)); 
				break; 
			} 
		} 

		
		protected void _fixspriteattitudes() 
		{ 
			for ( int boxside = 0; boxside < _childspriteptr.Size; boxside++) 
				_fixspriteattitude( boxside ); 
		} 

		public cSpriteTextureBox( ) 
		{ 
			_pskeleton = new cRealBox3( new cVector3( -1.0f, -1.0f, -1.0f ), 
                new cVector3( 1.0f, 1.0f, 1.0f ) ); 
			_initialize(); 
		} 
		
		public cSpriteTextureBox( cVector3 locorner ) 
		{ 
			_pskeleton = new cRealBox3( locorner, new cVector3( 1.0f, 1.0f, 1.0f ) ); 
			_initialize(); 
		} 

		public cSpriteTextureBox( cVector3 locorner, cVector3 hicorner, int resourceID = -1 ) 
		{ 
			_pskeleton = new cRealBox3( locorner, hicorner ); 
			_initialize(); 
			if ( resourceID != -1 ) 
				setAllSidesTexture( resourceID, 1 ); 
		}

        public cSpriteTextureBox(cVector3 locorner, cVector3 hicorner, Color color)
        {
            _pskeleton = new cRealBox3(locorner, hicorner);
            _initialize();
            setSideColors(color);
        }

        public cSpriteTextureBox(cVector3 locorner, cVector3 hicorner, int resourceID,
            int xtilecount ) 
		{ 
			_pskeleton = new cRealBox3( locorner, hicorner ); 
			_initialize(); 
			if ( resourceID != -1 ) 
				setAllSidesTexture( resourceID, xtilecount ); 
		} 

		public cSpriteTextureBox( cRealBox3 pskeleton, int resourceID = -1 ) 
		{ 
			_pskeleton = new cRealBox3( pskeleton.LoCorner, pskeleton.HiCorner); 
			_initialize(); 
			setAllSidesTexture( resourceID, 1 ); 
		}

        public cSpriteTextureBox(cRealBox3 pskeleton, Color color)
        {
            _pskeleton = new cRealBox3(pskeleton.LoCorner, pskeleton.HiCorner);
            _initialize();
            setSideColors(color);
        }

        public cSpriteTextureBox(cRealBox3 pskeleton, int resourceID, int xtilecount ) 
		{ 
			_pskeleton = new cRealBox3( pskeleton.LoCorner, 
					pskeleton.HiCorner); 
			_initialize(); 
			setAllSidesTexture( resourceID, xtilecount ); 
		} 
		
	//Accessors 
	
		public cSprite pside( int boxside ) 
		{ 
			if (!( _childspriteptr.Size > boxside )) 
				return null; 
			return _childspriteptr[ boxside ]; 
		} 

		
		/* enabledisplaylist specifies whetehr or not you want to
			try and use a display list to show this kind of sprite. By default this is TRUE.  We 
			make it FALSE because the cSpriteTextureBox actually runs slower with displaylists, so we
			want to be able turn of display lists for them.  */ 
	//Mutator 
		
        public void setSideTexture( int boxside, int resourceID, int xtilecount = 1 ) 
		{ 
			if ( boxside < 0 || boxside >= _childspriteptr.Size || resourceID == -1 ) 
				return ; 
			_childspriteptr.SetAt( boxside, 
				new cSpriteIconBackground( resourceID, 
					_pskeleton.side( boxside ), xtilecount )); 
			_fixspriteattitudes(); 
			_newgeometryflag = true; 
			fixResourceID(); 
		} 

		public void setAllSidesTexture( int resourceID, int xtilecount = 1 ) 
		{ 
			for ( int i = 0; i < _childspriteptr.Size; i++) 
				setSideTexture( i, resourceID, xtilecount ); 
			fixResourceID(); 
		} 

		
		public void setPlainRectangle( int boxside ) 
		{ 
			if (!( _childspriteptr.Size > boxside )) 
				return ; 
			cSpriteRectangle prect = null; 
			switch( boxside ) 
			{ 
			case 0 : 
				prect = new cSpriteRectangle( Loy, Loz, Hiy, Hiz); 
				_childspriteptr.SetAt( boxside, prect );; 
				break; 
			case 1 : 
				prect = new cSpriteRectangle( Loy, Loz, Hiy, Hiz); 
				_childspriteptr.SetAt( boxside, prect );; 
				break; 
			case 2 : 
				prect = new cSpriteRectangle( Lox, Loz, Hix, Hiz); 
				_childspriteptr.SetAt( boxside, prect );; 
				break; 
			case 3 : 
				prect = new cSpriteRectangle( Lox, Loz, Hix, Hiz); 
				_childspriteptr.SetAt( boxside, prect );; 
				break; 
			case 4 : 
				prect = new cSpriteRectangle( Lox, Loy, Hix, Hiy); 
				_childspriteptr.SetAt( boxside, prect );; 
				break; 
			case 5 : 
				prect = new cSpriteRectangle( Lox, Loy, Hix, Hiy); 
				_childspriteptr.SetAt( boxside, prect );; 
				break; 
			}
            cColorStyle c = new cColorStyle();
            c.copy(pcolorstyle());
			prect.ColorStyle = c; //Use the base class colorstyle.
			_fixspriteattitude( boxside ); 
			fixResourceID(); 
		} 

		//Makes this side a plain rectangle.
	
		public void setSideInvisible( int boxside ) 
		{ 
			setPlainRectangle( boxside );
            _childspriteptr[boxside].pcolorstyle().Filled = false; 
			_childspriteptr[ boxside ].pcolorstyle().Edged = false; 
			fixResourceID(); 
		} 

		
		public void setSideSolidColor( int boxside, Color color ) 
		{
            if (boxside < 0 || boxside >= _childspriteptr.Size )
                return;
            _childspriteptr.SetAt(boxside,
                new cSpriteIconBackground(BitmapRes.Solid,
                    _pskeleton.side(boxside), 1, color ));
            _fixspriteattitudes();
            _newgeometryflag = true;
            fixResourceID();
		} 

		//Makes the side a rect 
	
		public void setSideColors( Color color ) 
		{
            for (int i = 0; i < _childspriteptr.Size; i++)
                setSideSolidColor(i, color);
            fixResourceID(); 
        } 

		//Just sets the fill colors 
	
		//cSprite overloads 
	
		public override void copy( cSprite psprite ) //Use this in copy constructor and operator= 
		{ 
			base.copy( psprite ); 
			if ( !psprite.IsKindOf( "cSpriteTextureBox" )) 
				return ; //You're done if psprite isn't a cSpriteTextureBox*.
			cSpriteTextureBox ptexturebox = ( cSpriteTextureBox )( psprite ); /* I know it is a
			cSpriteTextureBox at this point, but I need to do a cast, so the compiler will 
			let me call cSpriteTextureBox methods. */
            _pskeleton.copy(ptexturebox._pskeleton);
		} 

        public override cSprite copy( )
        {
            cSpriteTextureBox s = new cSpriteTextureBox();
            s.copy(this);
            return s;
        }

        public override bool IsKindOf( string str )
        {
            return str == "cSpriteTextureBox" || base.IsKindOf( str );
        }

		
	//Overload the cSpriteComposite draw function 
	
		public override void draw( cGraphics pgraphics, int drawflags ) 
		{ /* Use the base _spriteattitude and then walk the array of child sprites and call their
		imagedraw methods.   */ 
			pgraphics.pushMatrix(); 
			pgraphics.multMatrix( _spriteattitude );
            foreach (cSprite s in _childspriteptr)
                _childspriteptr.ElementAt().draw(pgraphics, drawflags);
            pgraphics.popMatrix(); 
		} 

		/* We cascade
			to the children like cSpriteComposite, but if drawflags is WIREFRAME,
			we draw the skeleton. */ 
	
		public virtual cRealBox3 Skeleton
		{
			get
				{ return _pskeleton; }
			set
			{
                _pskeleton.copy(value);
			    _initialize(); //Puts default blank rectangles in each face.
		    }
		}

		public virtual float Lox
		{
			get
				{ return _pskeleton.Lox;}
		}

		public virtual float Loy
		{
			get
				{ return _pskeleton.Loy;}
		}

		public virtual float Loz
		{
			get
				{ return _pskeleton.Loz;}
		}

		public virtual float Hix
		{
			get
				{ return _pskeleton.Hix;}
		}

		public virtual float Hiy
		{
			get
				{ return _pskeleton.Hiy;}
		}

		public virtual float Hiz
		{
			get
				{ return _pskeleton.Hiz;}
		}

		public virtual float Midx
		{
			get
				{ return _pskeleton.Midx;}
		}

		public virtual float Midy
		{
			get
				{ return _pskeleton.Midy;}
		}

		public virtual float Midz
		{
			get
				{ return _pskeleton.Midz;}
		}

		public override bool EnabledDisplayList
		{
			get
				 { return false; }
		}


	}
}
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     