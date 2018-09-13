// In AC Framework Version 1.2, default _slices and _stacks is set to 32 instead of 16
// Default parameters were added -- JC

using System;

namespace ACFramework
{ 
	
	class cSpriteSphere : cSprite 
	{
        protected int _slices;
        protected int _stacks;
        protected cGLShape glshape;

        public cSpriteSphere( float radius = 1.0f, int slices = 32, int stacks = 32 )  
		{ 
		    _slices = slices; 
		    _stacks = stacks; 
		    _radius = radius; 
            glshape = new cGLShape();
		} 

		//Default constructor calls initializer 
	//Overloaded cSprite methods 
	
		public void copy( cSpriteSphere pspritesphere ) 
        {
            base.copy(pspritesphere);
            _slices = pspritesphere._slices;
            _stacks = pspritesphere._stacks;
        }

        public override cSprite copy()
        {
            cSpriteSphere s = new cSpriteSphere();
            s.copy(this);
            return s;
        }

        public override bool IsKindOf(string str)
        {
            return str == "cSpriteSphere" || base.IsKindOf(str);
        }
		
		public override void mutate( int mutationflags, float mutationstrength ) 
		{ 
			base.mutate( mutationflags, mutationstrength ); 
		} 

		
		public override void imagedraw( cGraphics pgraphics, int drawflags ) 
		{ 
			if (( Edged & !Filled ) || ( (drawflags & ACView.DF_WIREFRAME) != 0 )) 
			/* If the sphere is filled, lets not draw its edges unless we're in wireframe
			The reason I put this in is because in many games the sprite by default is
			edged and filled for the sake of the polygonal sprites, and then if we select
			a sphere sprite and its edged as well as filled it runs too slow. */ 
			{ 
				pgraphics.setMaterialColor( LineColor ); 
				glshape.glutWireSphere( _radius, _slices, _stacks ); 
			} 
			if ( Filled && ( (drawflags & ACView.DF_WIREFRAME) == 0 )) 
			{ 
				pgraphics.setMaterialColor( FillColor ); 
				glshape.glutSolidSphere( _radius, _slices, _stacks ); 
			} 
		} 

		
	} 
	
}