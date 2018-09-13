using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics;
using System.Collections;

namespace ACFramework
{ 
	
	///////////////////////////////////////////////////////////////////////////// 
	// 
	// texture.h : interface of the cTexture class 
	// 
	///////////////////////////////////////////////////////////////////////////// 
	
	class cTexture 
	{ 
		public static readonly double LOG2 = Math.Log(2.0); 
		protected static readonly bool USEMIPMAP = true; 
		protected static readonly int MAXEDGESIZE = 1024; 
			/* The actual aspect of the resource image we loaded will live in double _imageaspect,
			but we will size the texture to powers of two and keep that in _cx and _cy. */ 
		protected double _imageaspect; 
		protected int _cx; //These are powers of two that try to match the size of the resource image.
		protected int _cy; 
		protected bool _transparent; 
		protected int _pixeldataformat; //Will be 4 (GL_RGBA)  or 3 (GL_RGB) according to _transparent TRUE or FALSE  
		protected bool _usemipmap; 
		protected uint _textureID; //An OpenGL-generated integer type name for a texture object.

        public static double min(double a, double b)
        {
            return (a < b) ? a : b;
        }
        
        public static void makePowersOfTwo( ref int cx, ref int cy ) 
		{
			/* We want to disallow the possiblity of a texture with an edge greater than cTexture::MAXEDGESIZE.
			So to start with we shrink the cx and cy down to be less than this size. */ 
			double clampcx = cx; 
			double clampcy = cy; 
			while ( clampcx > cTexture.MAXEDGESIZE || clampcy > cTexture.MAXEDGESIZE ) 
			{ 
				clampcx /= 2.0f; 
				clampcy /= 2.0f; 
			} 
			double logcx = Math.Log( clampcx ) / LOG2; //This gives the log base 2.
			double logcy = Math.Log( clampcy ) / LOG2; 
			double targetaspect = ( double ) cx /( double ) cy; 
				/* We consider rounding the log base 2 both down or up to the nearest integer. */ 
			double newcxlo = min( cTexture.MAXEDGESIZE, Math.Pow( 2.0, Math.Floor( logcx ))); 
			double newcxhi = min( cTexture.MAXEDGESIZE, Math.Pow( 2.0, Math.Ceiling( logcx ))); 
			double newcylo = min( cTexture.MAXEDGESIZE, Math.Pow( 2.0, Math.Floor( logcy ))); 
			double newcyhi = min( cTexture.MAXEDGESIZE, Math.Pow( 2.0, Math.Ceiling( logcy ))); 
			double [] testaspect = new double[ 4 ]; /* I want to get the best match for the targetaspect, so I will
			try all four possibile combinations of lo and hi for x and y. */ 
			testaspect[ 0 ] = newcxlo / newcylo; 
			testaspect[ 1 ] = newcxlo / newcyhi; 
			testaspect[ 2 ] = newcxhi / newcylo; 
			testaspect[ 3 ] = newcxhi / newcyhi; 
			int bestaspectindex = 3; //All in all, I would prefer to stretch.
			double bestaspecterror = Math.Abs( testaspect[ 3 ]-targetaspect ); 
			for ( int i = 2; i >= 0; i--) 
				if ( Math.Abs( testaspect[ i ]-targetaspect ) < bestaspecterror ) 
				{ 
					bestaspectindex = i; 
					bestaspecterror = Math.Abs( testaspect[ i ]-targetaspect ); 
				} 
			switch( bestaspectindex ) 
			{ 
				case 0 : 
					cx = ( int ) newcxlo; 
					cy = ( int ) newcylo; 
					break; 
				case 1 : 
					cx = ( int ) newcxlo; 
					cy = ( int ) newcyhi; 
					break; 
				case 2 : 
					cx = ( int ) newcxhi; 
					cy = ( int ) newcylo; 
					break; 
				case 3 : 
					cx = ( int ) newcxhi; 
					cy = ( int ) newcyhi; 
					break; 
			} 
        } 
		
		protected byte [] _readDC( Bitmap pMemDC ) 
		{ 
			byte [] pdata = null; 
			int cx = pMemDC.Width; 
			int cy = pMemDC.Height; 
			uint index = 0; 
			Color colorref; 
			if ( !_transparent ) 
			{ 
				uint arraysize = 3 * ( uint ) cx * ( uint ) cy; 
				pdata = new byte[ arraysize ]; 
				/* We step the y values in the reverse order because OpenGL counts y pixels from bottom to
				top unlike Windows CDCs, which count from top to bottom. */ 
				for ( int j = cy -1; j >= 0; j--) 
					for ( int i = 0; i < cx; i++) 
					{ 
						colorref = pMemDC.GetPixel( i, j );
						pdata[ index++] = colorref.R; 
						pdata[ index++] = colorref.G; 
						pdata[ index++] = colorref.B; 
					} 
			} 
			else //transparent.  This means show with a transparent background.
			{ 
				pdata = new byte[ 4 * cx * cy ]; 
				Color transparentcolorref; 
				transparentcolorref = pMemDC.GetPixel( 0, 0 ); 
					/* We will treat the color found in the upper left corner as transparent. */ 
				for ( int j = cy -1; j >= 0; j--) 
					for ( int i = 0; i < cx; i++) 
					{ 
						colorref = pMemDC.GetPixel( i, j ); 
						pdata[ index++] = colorref.R; 
						pdata[ index++] = colorref.G; 
						pdata[ index++] = colorref.B; 
						pdata[ index++] = colorref == transparentcolorref ? (byte) 0 : (byte) 255; 
						/* Set the alpha to minimum on "background" pixels, and to maximum
						otherwise.  These values get normalized to 0.0 and 1.0 in
						the fragment operations. */ 
					} 
			} 
			return pdata; //Return it to the cTexture constructor method, where it gets used and deleted.
		} 

		/* A helper function that gets a GLubyte *pdata
			array out of the pMemDC so you can scale it and pass it to _maketexture.
			 Used in the constructors.*/ 
	
		protected void _maketexture( byte [] pdata ) 
		{ 	/* usemipmap uses more memory, makes a nicer looking
			images across a range of size scales.  It may be slower. */
            GL.Enable(EnableCap.Texture2D);
            GL.GenTextures(1, out _textureID); /* Get an unused name for a texture
			object, which is something a bit like a display list.  That is,
			it's a kind of handle which will enable us to rapidly reaccess a
			texture once we've first loaded it. */
            GL.BindTexture(TextureTarget.Texture2D, _textureID);  /* Makes this texture current
			so that all the following calls in _maketexture() affect it. */
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS,  
                (int) TextureWrapMode.Clamp); 
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT, 
                (int) TextureWrapMode.Clamp);
			if ( _usemipmap ) 
			{
                GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMinFilter, 
                    (int) TextureMinFilter.Linear);  
                GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMagFilter, 
                    (int) TextureMagFilter.Linear );  
                Glu.Build2DMipmap(TextureTarget.Texture2D, _pixeldataformat,
                    _cx, _cy, 
                    (_pixeldataformat == 3)? PixelFormat.Rgb : PixelFormat.Rgba,
                    PixelType.UnsignedByte, pdata );
			} 
			else //No usemipmap means use only use one texture 
			{ 
			//Use faster size filters.
                GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMagFilter, 
                    (int) TextureMagFilter.Nearest);  
                GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMinFilter, 
                    (int) TextureMinFilter.Nearest ); // GL_NEAREST 
                GL.TexImage2D(TextureTarget.Texture2D, 0,
                    (_pixeldataformat == 3) ? PixelInternalFormat.Rgb : 
                        PixelInternalFormat.Rgba,
                    _cx, _cy, 0,
                    (_pixeldataformat == 3) ? PixelFormat.Rgb : PixelFormat.Rgba,
                    PixelType.UnsignedByte, pdata);
			}
            GL.BindTexture(TextureTarget.Texture2D, 0); //Close off the call to glBindTexture started above.
            GL.Disable(EnableCap.Texture2D);
		}

	
		public cTexture( int resourceID, bool transparent )  
		{ 
            _transparent = transparent;
			byte [] pdata = null;
			if ( _transparent ) 
				_pixeldataformat = 4; 
			else 
				_pixeldataformat = 3; 
			_usemipmap = cTexture.USEMIPMAP;
            Bitmap pDC = new Bitmap(BitmapRes.getResource(resourceID));
            if (pDC.Width > MAXEDGESIZE || pDC.Height > MAXEDGESIZE)
            {
                MessageBox.Show(string.Format("Edge size cannot exceed {0}.\nPlease use a photo editor to resize the bitmap {1} or set MAXEDGESIZE in texture.cs.\nQuit the program immediately",
                    MAXEDGESIZE, resourceID));
                return;
            }
            int imagecx = pDC.Width;
            int imagecy = pDC.Height; 
			_cx = imagecx; 
			_cy = imagecy; 
			_imageaspect = ( double ) _cx / ( double ) _cy; /* By default we set the aspect to match the aspect
			of the  image we loaded, before doing the makePowersOfTwo call. Even though we are going
			to clamp the texture to powers of two, if we apply it to a rectangle with proportions
			matching _imageaspect, the image will appear unstretched; the two stretches cancel out. */ 
			makePowersOfTwo( ref _cx, ref _cy ); 
			if ( _cx == imagecx && _cy == imagecy ) // bitmap is fine 
			{ 
				pdata = _readDC( pDC ); //Uses the _transparent flag to decide how to read.
				pDC = null;
                
                _maketexture( pdata ); 
				pdata = null; 
				return; 
			}

            MessageBox.Show(string.Format("You must use a photo editor to resize the bitmap {0} so that its dimensions are powers of 2.\nQuit the program immediately.", resourceID));
            return; 
		} 

		//For use with resrource bitmaps 

        public cTexture(Color color)
        {
            _cx = 1;
            _cy = 1;
            byte[] pdata = new byte[(uint) 3];

            pdata[0] = color.R;
            pdata[1] = color.G;
            pdata[2] = color.B;

            _transparent = false;
            _pixeldataformat = 3;
            _usemipmap = true;
            _imageaspect = 1.0; 
            
            _maketexture( pdata );
        }

        //For use with solid colors 

        public cTexture(cTextureInfo ptextureinfo)  
		{ 
            _transparent = false;
            _usemipmap = true;
            uint bytesperpixel = 4; 
			_pixeldataformat = 4; 
			int imagecx = _cx = ptextureinfo._width; 
			int imagecy = _cy = ptextureinfo._height; 
			_imageaspect = ( double ) _cx / ( double ) _cy; /* By default we set the aspect to match the aspect
			of the  image we loaded, before doing the makePowersOfTwo call. Even though we are going
			to clamp the texture to powers of two, if we apply it to a rectangle with proportions
			matching _imageaspect, the image will appear unstretched; the two stretches cancel out. */ 
		
			makePowersOfTwo( ref _cx, ref _cy ); 
			if ( _cx != imagecx || _cy != imagecy ) //Need to rescale 
			{ 
				uint arraysize = 
					bytesperpixel * ( uint ) _cx * ( uint ) _cy; 
				byte [] pdatascaled = new byte[ arraysize ]; 
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
                GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                Glu.ScaleImage(
                    (_pixeldataformat == 3) ? PixelFormat.Rgb : PixelFormat.Rgba,
                    imagecx, imagecy, PixelType.UnsignedByte,
                    ptextureinfo.Data, _cx, _cy, PixelType.UnsignedByte,
                    pdatascaled);
                string errorstring = Glu.ErrorString(GL.GetError());
				ptextureinfo.resetData( _cx, _cy, pdatascaled );
           } 		
		
			_maketexture( ptextureinfo.Data ); 
		} 

		//For use with quake MD2 models 

        public virtual uint TextureID
        {
            get
                { return _textureID; }
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

        public virtual double ImageAspect
        {
            get
                { return _imageaspect; }
        }

        public virtual bool UsesTransparentMask
        {
            get
                { return _transparent; }
        }

		public void select()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
			/* Makes this texture active. */ 
        }

		public void unselect()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        } /* Makes OpenGL
			revert to the default empty texture. Also prevents texture calls from
			affecting this texture. */ 
	} 

}