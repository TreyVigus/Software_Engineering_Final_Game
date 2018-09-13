using System;
using System.Drawing;
using OpenTK.Graphics;
using System.Windows.Forms;

namespace ACFramework
{

    class ACView
    {
        public static readonly int SIMPLIFIED_BACKGROUND = 1;
        public static readonly int FULL_BACKGROUND = 2;

        public static readonly int DF_STANDARD = 0;
        public static readonly int DF_WIREFRAME = 8;
        public static readonly int DF_FULL_BACKGROUND = 16;
        public static readonly int DF_SIMPLIFIED_BACKGROUND = 32;

        public static readonly bool STARTBITMAPBACKGROUNDFLAG = false;
        public static readonly bool STARTSOLIDBACKGROUNDFLAG = false;

        private cGraphicsOpenGL _pgraphics;
	    private cCritterViewer _pviewpointcritter;
        private int _drawflags;

        public ACView() 
        {
            _drawflags = DF_STANDARD;
	        if (STARTBITMAPBACKGROUNDFLAG)
		        _drawflags |= DF_FULL_BACKGROUND;
	        else
		        _drawflags &= ~DF_FULL_BACKGROUND;
	        if (STARTSOLIDBACKGROUNDFLAG)
		        _drawflags |= DF_SIMPLIFIED_BACKGROUND;
	        else
		        _drawflags &= ~DF_SIMPLIFIED_BACKGROUND;
            _pviewpointcritter = new cCritterViewer(this);
            _pgraphics = new cGraphicsOpenGL();
            _pviewpointcritter.Listener = new cListenerViewerRide();
            pgame().Viewpoint = _pviewpointcritter;
            
        }

        public cGame pgame()
        {
            return Framework.Pdoc.pgame();
        }

        public Color sniff(cVector3 sniffpoint)
        {
	        return _pgraphics.sniff(sniffpoint);
        }

        public cVector3 pixelToPlayerPlaneVector(int xpix, int ypix)
        {
	        return pixelToCritterPlaneVector(xpix, ypix, pgame().Player);
        }

        public cVector3 pixelToCritterPlaneVector(int xpix, int ypix, cCritter pcritter)
        {
	        return _pgraphics.pixelAndPlaneToVector(xpix, ypix, pcritter.Plane);
        }

        public void setUseBackground(int backgroundtype)
        {
	        if (pgame().SkyBox != null)
		        pgame().SkyBox.NewGeometryFlag = true;
	        if (backgroundtype == FULL_BACKGROUND)
	        {
		        _drawflags &= ~DF_SIMPLIFIED_BACKGROUND;
		        _drawflags |= DF_FULL_BACKGROUND;
	        }
	        else if (backgroundtype == SIMPLIFIED_BACKGROUND)
	        {
		        _drawflags |= DF_SIMPLIFIED_BACKGROUND;
		        _drawflags &= ~DF_FULL_BACKGROUND;
	        }
	        else
	        {
		        _drawflags &= ~DF_SIMPLIFIED_BACKGROUND;
		        _drawflags &= ~DF_FULL_BACKGROUND;
	        }
        }

        public cCritterViewer pviewpointcritter( )
        {
            return _pviewpointcritter;
        }

        public cGraphics pgraphics()
        {
            return _pgraphics;         
        }

        public void OnMouseMove( Point point ) 
        {
	        if ( Framework.Leftclick ) //You're dragging.
		        OnSetCursor( point );
			        //OnSetCursor args are the window of the mouse, the hit-test code, the mouse message.
	        pgame().onMouseMove(this, point);
        }

        public void OnSetCursor( Point point ) 
         {
	        if (_pviewpointcritter.Listener.IsKindOf("cListenerViewerRide"))
		        pgame().CursorPos = pixelToPlayerYonWallVector(point.X, point.Y, 
			        0.5f * _pviewpointcritter.toFarZ());
			        /* If we are riding the critter, we want to pick a point on the "yon"
			        wall, that is, the viewer's far clip plane.  Given that we're on the
			        critter, that distance from us will the viewpointcritter's toFarZ(),
			        inlined as {return fabs(_zfar - _position.z());} */
	        else
		        pgame().CursorPos = pixelToPlayerPlaneVector(point.X, point.Y);
			        /* Otherwise we pick a point in the plane of the player's body,
			        that is, his tangent and normal plane. */
        }

        public cVector3 pixelToPlayerYonWallVector(int xpix, int ypix, float distancetoyon)
        {
	        return pixelToCritterYonWallVector(xpix, ypix, pgame().Player, distancetoyon);
        }

        public cVector3 pixelToCritterYonWallVector(int xpix, int ypix, 
            cCritter pcritter, float distancetoyon)
        {
	        return _pgraphics.pixelAndPlaneToVector(xpix, ypix, 
                new cPlane( 
                pcritter.Position.add(pcritter.AttitudeTangent.mult(distancetoyon)), 
                pcritter.AttitudeTangent.neg()));
		        //The two args to cPlane constructor are (origin, binormal), origin meaning a point on the plane.
        }

        public void setCursorPosToCritter(cCritter pcritter)
        {
	        int intcrittx, intcritty;
	        float zbuff;
	        vectorToPixel(pcritter.Position, out intcrittx, out intcritty, out zbuff);
            Framework.SetCursorPos( intcrittx, intcritty );
        }

        void vectorToPixel(cVector3 position, out int xpix, out int ypix, out float zbuff)
        {
	        _pgraphics.vectorToPixel(position, out xpix, out ypix, out zbuff);
        }

        public void OnDraw()
        {
        //If you've just changed the game type, or restarted a game, 
        //use the game's initialization code on this view.
	        if (ACDoc.Restart)
	        {
		        _pviewpointcritter = new cCritterViewer(this); // Looks at pgame().border()
                pgame().View = this;
		        pgame().Viewpoint = _pviewpointcritter;
		        pgraphics().installLightingModel(pgame().LightingModel);
                ACDoc.Restart = false;
		        //And now go on and show the game.
	        }
            //Animate the viewer
            float dt = Framework.Pdoc.getdt();
            _pviewpointcritter.feellistener(dt); 
	        _pviewpointcritter.move(dt); 
	        _pviewpointcritter.update(this, dt); 
		        //possibly feel forces or sniff pixels or align position with player.
	        _pviewpointcritter.animate(dt);
            
            _pgraphics.garbageCollect(); // gets rid of textures, etc., that haven't
                    // been used for a while
            
            //Graphically show the status of the game.

            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit);

            //Install the projection and view matrices.
	        _pviewpointcritter.loadProjectionMatrix(); // Initializes the PROJECTION matrix or,
	        _pviewpointcritter.loadViewMatrix(); 

            //Draw the world, by default as a background and a foreground rectangle.
            pgame().drawWorld(_pgraphics, _drawflags); 

            //Draw the critters.
	        pgame().drawCritters(_pgraphics, _drawflags);

            GL.Finish();

            // and then, SwapBuffers displays everything on the screen in Framework
        }

    }
}
