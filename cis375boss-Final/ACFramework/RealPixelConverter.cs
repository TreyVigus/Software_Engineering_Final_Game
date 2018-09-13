using System;

namespace ACFramework
{ /*----Log
	Created September 26, 1995
	October 5, 1995. Changed max? names to c? names.  Added changed_size_flag
	flag in case you have a program that uses realpixelconverter to compute and
	store a lot of pixel/real info; the status of the changed_size_flag
	tells the program when it needs to recompute its pixel/real info.
	October 9, 1995.  Added PixelToReal and two Zoom methods.
	Feb 7, 1996 Temporarily changed RealToPixel and PixelToReal to
	use pass by reference.  This change was later removed as it 
	did not work reliably.  Added _fixedaspect switch to toggle 
	between isometric mode and fill-the-whole-window mode.
	December 13, 1997.  Re-edited and cleaned up comments.  Added
	the private _fixup_constants function to avoid code duplication.
	June 21, 1999.  Changed the name of the class from the former 
	"Frame" to the new "cRealPixelConverter".
	July 26, 1999.  Added a feature that has the converter effectively rotate
	the RealWindow to have its loger direction in the same direction as the longer
	direction of the pixelview. _autorotate_flag
	July 23, 1999.  Added the _forcerotate_flag so one view can slave its rotation to
	another view, could be useful to match a subsidiary view to a main view.  I've
	never actually used this flag so I can't be sure it really works.
	August 9, 1999.  Enclosed the CLAMP checks inside an ASSERT, so that they are
	turned off in the release build for better speed.  The CLAMP checks are to 
	prevent a real from being converted into an integer so large that it wraps around
	and appears negative.  This is not going to happen in simple 2D programs, but it
	is a risk when you use a cRealPixelConverter in a 3D program where you are
	projecting objects onto a plane; an object close to a projection point can
	project out arbitrarily far onto a plane.  Even here, though it will be better
	to risk not doing the check in the Release build.  Instead you should catch the
	problem in a Debug buidl and prevent it form happening, e.g. by using a clip
	plane to not process an object that's too close to the projection point. 
	-----Usage
	(1) Put "#include realpixelconverter.h" before using the class.
	(2) Put cRealPixelConverter _realpixelconverter; as a global variable in your Windows32 program.  In MFC, have it as a member of your CView class.
	(3) Use _realpixelconverter.setRealWindow(Real lox,Real loy,Real hix,Real hiy);
		to change Real window box if desired.  In Windows32, do this in 
		WM_CREATE; in MFC do it in CView::OnCreate.  You can change it
		again as a result of user input later on, i.e. for panning or zooming.
	(4) Put realpixelconverter.setPixelWindow(LOWORD(lParam), HIWORD(lParam)); in	WM_SIZE in Windows 32.  In MFC, put the following line in CView::OnSize.
		_realpixelconverter.setPixelWindow(cx, cy); This adjusts _cx and _cy to 
		match current pixel window size.
	(5) Use _realpixelconverter.realToPixel(rx, ry, &ix, &iy)
		to convert Real (rx, ry) to int (ix, iy).
	(6) Use _realpixelconverter.pixelToReal(int ix, int iy, Real &rx, Real &ry)
		to convert int (ix, iy) [e.g. from a mouse click] to  Real (rx, ry).
	(7) Use _realpixelconverter.zoomAtPixel(int pixx, int pixy, Real zoomfactor); to
		zoom in or out at a given mouseclick point.  zoomfactor > 1.
		0 makes image smaller, zoomfactor < 1.0 makes image bigger.
	(8) Use _realpixelconverter.setFixedAspect(TRUE) to keep circles from changing
		to ellipses as you resize Pixel window.  In this case the 
		Pixel window stays centered on the center, and there may be some
		unused areas of the onscreen window at the edges.  Use FALSE to keep
		things 	fixed in same relative Pixel window positions, but
		at the cost of deforming things (circles to ellipses).  TRUE is default.
	(9) Use _realpixelconverter.setAutoRotate(TRUE) to make the display automatically
		rotate 90 degrees counterclockwise for best use of screen space.  Use this
		if there's nothing that has to be horizontal vs. vertical.  This doesn't 
		work smoothly with FixedAspect(FALSE), so use with the default FixedAspect TRUE.
	May 21, 2002. Took out _forcerotate_flag and _autorotate_flag.*/ 
	
	// took out the setRealWindow with the cRealBox parameter -- JC 

	// For AC Framework 1.2, default parameters were added -- JC

	class cRealPixelConverter 
	{
        static readonly private float INTEGER_OVERFLOW = 2000000000.0f; /* This is designed to match the range
		of a 32-bit integer, which goes from -2 Gig to 2 Gig.  You need to change
		this value if you use a different size of integer. */ 
		private int _left, _top, _right, _bottom; //Current pixel window 
		private int _cx, _cy; //current pixel window size in pixels. 
		private int _midx, _midy; /*Used by RealToPixel().  Pixel center of window.
		int _left, _top, _right, _bottom; //Pixel coords for the occasional times
			cases when the pixel window happens not just be (0,0) to (_cx,_cy)*/ 
		private float _realcenterx, _realcentery; //Real coords of center of window. 
		private float _realradiusx, _realradiusy; 
			//The real value of half the window, origin in center. 
		private float _realpixperx, _realpixpery; 
			//Length per pixel, same as _mid?/_realradius? 
		private float _constx, _consty; //precalculated terms to speed up RealToPixel 
		private bool _changed_size_flag; 
		private bool _fixed_aspect_flag; //See (8) above. 
		
		private void _fixup_constants() /* Helper function to update
			_realpixper?, _const? and _changed_size_flag. */ 
		{ /* Precompute the four constants used in realToPixel and
	turn on the _changed_size_flag so that anyone using this cRealPixelConverter
	object will know that any realToPixel computation results they've
	stored must now be redone. */ 
			float realwindowaspect = Height / Width; 
			float pixelwindowaspect = (float) _cy / _cx; 
			_realpixperx = ((float)( _midx - _left )) / _realradiusx; 
			_realpixpery = ((float)( _midy - _top )) / _realradiusy; 
			if ( _fixed_aspect_flag ) 
			{ 
				if ( _realpixperx < _realpixpery ) 
					_realpixpery = _realpixperx; 
				else 
					_realpixperx = _realpixpery; 
			} 
			_constx = _midx - _realcenterx * _realpixperx; 
			_consty = _midy + _realcentery * _realpixpery; 
			_changed_size_flag = true; 
		
		} 

		public cRealPixelConverter( float radiusx = 2.0f, float radiusy = 1.5f ) 
		{ 
			if ( radiusx == 0.0f )
                radiusx = 0.00001f;  
			if ( radiusy == 0.0f )
                radiusy = 0.00001f; //Never allow zeroes. 
			_realradiusx = radiusx; 
			_realradiusy = radiusy; 
            initialize( );
        }

        public bool RealCLAMP(ref float x, float lo, float hi)
        {
	        float oldx = x;
            if (x < lo)
                x = lo;
            else if (x > hi)
                x = hi;
	        return x!=oldx;
        }

        private void initialize( )
        {
		/* The default realpixelconverter will have a RealWindow 4 wide and 3 high, in 
			the same proportion as a standard window (such as 800 by 600 pixels).
			We'll put the center at (0.0, 0.0) and let the bottom left corner be
			(-2.0, -1.5), and the top right corner be (2.0, 1.5). The default
			PixelWindow will be 800 by 600. */ 
			_changed_size_flag = true; 
			_fixed_aspect_flag = true; 
			_realcenterx = _realcentery = 0.0f; //Default center at origin. 
		/* Force some numbers in to be safe, though normally you'll
	call setPixelWindow again in the initial WM_SIZE with
	the actual size of your active window. */ 
			setPixelWindow(800, 600); 
        }

		public void setPixelWindow( int cx, int cy ) //Assumes pixel window is from (0,0) to (cx,cy) 
		{ 
			_left = 0; _top = 0; _right = cx; _bottom = cy; 
			_cx = cx; 
			_cy = cy; 
			_midx = cx /2; 
			_midy = cy /2; 
			if ( _midx == 0 ) 
				_midx = 1; 
			if ( _midy == 0 ) 
				_midy = 1; //Do this so _realpixperx can't be 0 in pixelToReal. 
			_fixup_constants(); 
		} 

		public void setPixelWindow( int left, int top, int right, int bottom ) 
		{ 
			_left = left; _top = top; _right = right; _bottom = bottom; 
			_cx = _right - _left; 
			_cy = _bottom - _top; 
			_midx = _left + _cx /2; 
			_midy = _top + _cy /2; 
			if ( _midx == 0 ) 
				_midx = 1; 
			if ( _midy == 0 ) 
				_midy = 1; //Do this so _realpixperx can't be 0 in pixelToReal. 
			_fixup_constants(); 
		} 

		public void setRealWindow( float lox, float loy, float hix, float hiy ) 
		/* Usually we like to pass variables by reference if we change them,
			but in the special case of the next two functions we don't
			do this. The reason is that these functions are used inside
			lots of classes, and C++ doesn't always do the right thing if
			you ask it to generate a reference to a class member.  Instead
			we'll always explicitly generate the pointer at the coding
			time with the "&" operator. */ 
		{ 
			_realcenterx = ( lox + hix ) / 2.0f; //Average the values 
			_realcentery = ( loy + hiy ) / 2.0f; //Average to get the center. 
			_realradiusx = hix - _realcenterx; 
			_realradiusy = hiy - _realcentery; 
			if ( _realradiusx == 0.0f )
                _realradiusx = 0.00001f;  
			if ( _realradiusy == 0.0f )
                _realradiusy = 0.00001f;  
			_fixup_constants(); 
		}

        // I commented this out, but someone might want to use it -- JC
        //	void setRealWindow( cRealBox3 box )
        // {
        //	    setRealWindow(box.lox(), box.loy(), box.hix(), box.hiy());
        // }

        public void realToPixel(float rx, float ry, out int ix, out int iy) 
		/*Converts real coords to integer pixel coords.  Bails with a FALSE
			if an integer coord would be >INTEGER_OVERFLOW or <-INTEGER_OVERFLOW. */ 
		{ /*Designed to match the centers of the 2 windows even if
	_fixed_aspect_flag.  Originally, we had the more obvious
		tempx = _midx + (rx - _realcenterx) * (_midx/_realradiusx);
		tempy = _midy - (ry - _realcentery) * (_midy/_rearadiusy);
	To speed it up, we precompute _midx/_realradiusx
	as _realpixperx and we precompute 
	_midx -_realcenterx*_realpixperx as constx, so now
	tempx = constx + rx*_realpixperx.  Note that you have
	to recompute these constants whenever you change the window
	sizes. Another thing we do here is to not allow integers
	that might overflow a thirty-two bit register. */ 
			float tempx, tempy; 
			tempx = _constx + rx * _realpixperx; 
			tempy = _consty - ry * _realpixpery; 
			RealCLAMP( ref tempx, - INTEGER_OVERFLOW, INTEGER_OVERFLOW ); 
			RealCLAMP( ref tempy, - INTEGER_OVERFLOW, INTEGER_OVERFLOW ); 
			ix = ( int ) tempx; 
			iy = ( int ) tempy; 
		} 

		public void realToInt( float rx, out int ix ) 
		/*Converts scalar real quantities, like circle radii, to an appropriate pixel
			 size. Chooses min value between x and y directions if not fixed aspect. */ 
		{ 
			float tempx;
            tempx = rx * ((_realpixperx < _realpixpery) ? _realpixperx : _realpixpery);
            RealCLAMP(ref tempx, -INTEGER_OVERFLOW, INTEGER_OVERFLOW);
            ix = (int)tempx; 
		} 

		public void realToIntAnisotropic( float rx, out int ix, out int iy ) /* Use this in
			case FixedAspect is FALSE and you want it to show.  If, for
			instance you want to draw your real space cicles as pixel space ellipses. */ 
		{ 
			float tempx, tempy; 
			tempx = rx * _realpixperx; 
			tempy = rx * _realpixpery;
            RealCLAMP(ref tempx, -INTEGER_OVERFLOW, INTEGER_OVERFLOW); 
			ix = ( int ) tempx; 
			iy = ( int ) tempy; 
		} 

		public void pixelToReal( int ix, int iy, out float rx, out float ry ) 
			//Convert pixel coords to real coords. 
		{ 
		
			rx = (( float )( ix - _midx )/ _realpixperx ) + _realcenterx; 
			ry = (( float )( _midy - iy )/ _realpixpery ) + _realcentery; 
		} 

		public void intToReal( int ix, out float rx ) /* convert a pixel distance to a real.
			Chooses max value between x and y directions if not fixed aspect. */ 
		{ 
			rx = ((float) ix) / (( _realpixperx > _realpixpery )? _realpixperx : _realpixpery );
		} 

		public void zoom( float newcenterx, float newcentery, float zoomfactor ) 
		//Sets RealWindow center to args, multiplies radius by zoomfactor. 
		{ 
			_realcenterx = newcenterx; 
			_realcentery = newcentery; 
			_realradiusx *= zoomfactor; 
			_realradiusy *= zoomfactor; 
			_fixup_constants(); 
		} 

		public void zoomAtPixel( int pixx, int pixy, float zoomfactor ) //Same 
		//as the other Zoom, but calls PixelToReal first. 
		{ 
			float newcenterx, newcentery; 
			pixelToReal( pixx, pixy, out newcenterx, out newcentery ); 
			zoom( newcenterx, newcentery, zoomfactor ); 
		} 

		public virtual int Cx
		{
			get
				 { return _cx; }
		}

		public virtual int Cy
		{
			get
				 { return _cy; }
		}

		public virtual float LoRealx
		{
			get
				 { return _realcenterx - _realradiusx; }
		}

		public virtual float HiRealx
		{
			get
				 { return _realcenterx + _realradiusx; }
		}

		public virtual float LoRealy
		{
			get
				 { return _realcentery - _realradiusy; }
		}

		public virtual float HiRealy
		{
			get
				 { return _realcentery + _realradiusy; }
		}

		public virtual float Width
		{
			get
				 { return 2* _realradiusx; }
		}

		public virtual float Height
		{
			get
				 { return 2* _realradiusy; }
		}

		public virtual bool ChangedSizeFlag
		{
			get
				 { return _changed_size_flag; }
			set
				{ _changed_size_flag = value; }
		}

		public virtual bool FixedAspect
		{
			get
				 { return _fixed_aspect_flag; }
			set
			{ 
			    _fixed_aspect_flag = value; 
			    _fixup_constants(); 
		    }
		}


	}
}
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         