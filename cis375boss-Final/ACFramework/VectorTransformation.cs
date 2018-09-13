// For AC Framework 1.2, ZEROVECTOR and other vectors were removed,
// default parameters were added -- JC

using System;
using OpenTK;
using OpenTK.Graphics;

namespace ACFramework
{ 
    // begin Rucker's comment
    
    /* 
	History:
	
		The cVectorTransformation file implements 2 and 3 dimensional vectors and some
	matrices for transforming them.  They were developed by Rudy Rucker for CS 116A at SJSU 
	in Spring, 1996 in a series of 5 versions, up through cVector5.h and cVector5.cpp.
		This cVectorTransformation version was developed in Spring, 1999, for use in 
	Rucker's SOFTWARE PROJECTS text.  It was extensively rewritten at this time.
		The code in this module was very heavily reworked again in Spring/Summer 2001
	as part of the port to 3D OpenGL from our old 2D Windows API graphics.  These
	graphics versions are called cGraphicsMFC and cGraphicsOpenGL; see the
	graphics.h header for more information.
	
	Usage:
	
		The cVector2 and cVector3 classes represent two and three dimensional vectors.
	The add, sub and mult functions are for addition, subtration, and scalar multiplication.
	The mod functions are used for the dot product.  Cross product has not been implemented.
		The cMatrix2 and cMatrix3 classes are used as transformations on,
	respectively, the cVector2 and cVector3 objects.  To apply the transformation to a 
	vector we use v.apply(M) or the overloaded mult function as in v = M.mult( u ).
		Our transformations effectively perform a scaling, shear and/or a rotation
	followed by a translation.  When you multiply two transformations to get S = R * T, the effect of S will be
	to perform T and to then perform R.
	
	Implementation:
		We think of a 2-D vector as a 1 by 3 column matrix with a 1.0 in the bottom
	row, and we think of a cMatrix2 as a 3 by 3 matrix with a bottom identiy
	row of 0.0  0.0  1.0.
		The reason we do this is so that we can fit a translation into the matrix as the
	third column, and we want our matrices square so we an invert them. Our
	implementation computes the product of a cMatrix2 and cVector2 by doing the 
	standard multiplication of a 3x3 matrix times a 1x3 column vector. 
		The same ideas appply to the realtionship cVector3 and cMatrix3. */ 
	// end Rucker's comment

    // begin Childs comment
    /*  Because of its use of C++ overloaded operators, I had to radically change
     * much of the code in this file, which made profound changes throughout the
     * AC framework when it came to vector and matrix manipulation.  While C#
     * does have overloaded operators, they are more restricted than they are in
     * C++, and they have the required oddity of having to implement them in pairs,
     * which sometimes is just not practical.  I decided to discard all of the
     * overloaded operator functions and replace them with more conventional 
     * functions, which unfortunately, are not as intuitive to use.  However, Java
     * programmers should feel at home, since overloaded operators aren't even
     * available in Java at all. 
     * 
     * I added a function to the cVector3 class called rotationAngle.  It can be 
     * used to give an accurate angle of rotation, and is intended to be used to
     * align a model's orientation so that it is facing in the direction it is 
     * moving in.  I couldn't use the existing function, angleBetween, because
     * it took the arc cosine without any consideration to the clockwise or
     * counterclockwise rotation direction.  It is intended to be used from 
     * such a critter's update function by calling:
     *      rotateAttitude( tangent().rotationAngle( attitudeTangent() ));
     *      
     * -- JC
     */
    // end Childs comment

	
	//--TWO and THREE DIMENSIONAL Vectors ------
	
	class cVector2 
	{ 
		public static readonly float PRACTICALLY_ZERO = 0.001f; /* Used by the isZero method to treat as zero
			vectors the occasional neglibible rounded off remains of a vector that should be 
			zero. See isPracticallyZero below. */ 
		public static readonly float PRACTICALLY_PARALLEL_COSINE = 0.8f; /* Used when you dot product two unit vectors
			together; if they're "practiclaly parallel" the dot product is greater than this. */ 
		private float _x, _y; 
		
		public cVector2( float ix = 0.0f, float iy = 0.0f, float iz = 0.0f )
        { _x = ix; _y = iy; } 
		
		public cVector2( cVector3 v ) 
		{ 
			_x = v.X; _y = v.Y; 
		}

    //Mutators 
        public void set(float ix, float iy, float iz = 0.0f ) { _x = ix; _y = iy; }

		public void setZero(){ _x = 0.0f; _y = 0.0f;} 
		
		public cVector2 normalize() //Make yourself of unit length and return self.
		{ //Make the cVector2 a unit cVector2, and return self 
            cVector2 V = new cVector2();
            float m = Magnitude; 
			if ( m < 0.00001f ) 
			{ 
				_x = 1.0f; 
				_y = 0; 
				m = 0.0f; 
			} 
			else 
			{ 
				_x /= m; 
				_y /= m; 
			} 
		//	return m; Used to return magnitude 
            V.copy(this);
            return V; 
		} 

		public float normalizeAndReturnMagnitude() //Make yourself of unit length and return mag.
		{ 
			float m = Magnitude; 
			if ( m < 0.00001f ) 
			{ 
				_x = 1.0f; 
				_y = 0; 
				m = 0.0f; 
			} 
			else 
			{ 
				_x /= m; 
				_y /= m; 
			} 
			return m; 
		} 

		public cVector2 roundOff() /* Get rid of any component with size less than SMALL_REAL.  This
			can be useful if you have some nagging little roundoff residue in a compoment that
			should be 0. Return self. */ 
		{
            cVector2 V = new cVector2();
			if ( Math.Abs( _x ) < 0.00001f ) 
				_x = 0.0f; 
			if ( Math.Abs( _y ) < 0.00001f ) 
				_y = 0.0f;
            V.copy(this);
			return V; 
		}

    //Accessors  
		public bool isPracticallyEqualTo( cVector2 u ) { return sub( u ).Magnitude < PRACTICALLY_ZERO; } 
			/* Sometimes if I do something like u = cVector2(1,0) - cVector2(1,0),
				u I can end up with a nonzero magnitude.  So I use this to winnow out
				marginally inaccurate vectors that should be zero. */ 
	
    //Other Methods 
		
		public float distance( cVector2 v ) //Distance 
		{ 
			float dx = _x - v._x, dy = _y - v._y; 
			return (float) Math.Sqrt( dx * dx + dy * dy ); 
		} 

		public float angleBetween( cVector2 v ) /* The angle of u
			minus the angle of v, that is, the counterclockwise	angle gotten by
			turning from v to u.  Value between 0 and 2*PI. */ 
		{ /* Use fact that dot product of two vectors is the cosine of the angle between them.  Always returns
	an acute angle, between 0.0 and PI. */ 
			float umag = Magnitude; 
			float vmag = v.Magnitude; 
			if ( (umag == 0.0f) || (vmag == 0.0f) ) //If either is a zero vector, just return a zero angle.
				return 0.0f; 
			float cosine = mod( v )/( umag * vmag );
            if (cosine < -1.0f)
                cosine = -1.0f;
            else if (cosine > 1.0f)
                cosine = 1.0f;
			return (float) Math.Acos( cosine ); 
		} 

		public cVector2 defaultNormal() //Useful sometimes to arbitrarily pick a normal 
		{ 
			if ( _x != 0.0f || _y != 0.0f ) 
				return new cVector2(-_y, _x, 0.0f ); 
			else //always return nonzero vector.
				return new cVector2( 0.0f, 1.0f ); 
		} 

    // Used to be overloaded operators -- JC

		public cVector2 addassign( cVector2 u )
        {
            cVector2 V = new cVector2();
            _x += u._x; 
            _y += u._y;
            V.copy(this);
            return V; 
        } 
		
		public cVector2 subassign( cVector2 u )
        {
            cVector2 V = new cVector2(); 
            _x -= u._x; 
            _y -= u._y;
            V.copy(this);
            return V; 
        } 
		
		public cVector2 multassign( float f )
        {
            cVector2 V = new cVector2();
            _x *= f; 
            _y *= f;
            V.copy(this);
            return V; 
        } 
		
		public cVector2 divassign( float f ) 
		{
            cVector2 V = new cVector2();
			if ( Math.Abs( f ) >= 0.00001f ) 
			{ 
				_x /= f; 
				_y /= f; 
			}
            V.copy(this);
			return V; 
		} 

		public cVector2 turn( float turnangle ) //Means rotate aroudn z axis 
		{
            cVector2 V = new cVector2();
            if (turnangle == 0.0f)
            {
                V.copy(this);
                return V;
            }
			apply( cMatrix2.zRotation( turnangle ));
            V.copy(this);
            return V; 
		} 

		public cVector2 rotate( cSpin spin ) //Stands for more general roatoin when in higher D.
		{
            cVector2 V = new cVector2();
            if (spin.SpinAngle == 0.0f)
            {
                V.copy(this);
                return V;
            }
			apply( cMatrix2.rotation( spin ));
            V.copy(this);
            return V; 
		} 

		public cVector2 mult( float f ) //Scalar prod 
		{ 
			return new cVector2( _x * f, _y * f ); 
		} 

		public static cVector2 mult( float f, cVector2 u ) //Scalar prod 
		{ 
			return new cVector2( u._x * f, u._y * f ); 
		} 

		public cVector2 div( float f ) //Scalar division 
		{ 
			return new cVector2( _x / f, _y / f ); 
		} 

		public cVector2 add( cVector2 v ) //cVector2 sum 
		{ 
			return new cVector2( _x + v._x, _y + v._y ); 
		} 

		public cVector2 sub( cVector2 v ) 
			//cVector2 difference.
		{ 
			return new cVector2( _x - v._x, _y - v._y ); 
		} 

		public cVector2 neg( ){ return new cVector2(-_x,-_y );} 
		
		public cVector3 mult( cVector2 v ) //Returns (0,0,1) 
			/* We have a default 2D cross product method so the interface is like the interface of the cVector3. */ 
			//Unary -
		{ 
			if ( IsZero || v.IsZero ) 
				return new cVector3( 0.0f, 0.0f, 0.0f ); 
			else 
				return ( new cVector3( 0.0f, 0.0f, 1.0f )); 
		} 

		public float mod( cVector2 v ) //Dot product 
		{ 
			return ( _x * v._x + _y * v._y ); 
		} 

		public bool equal( cVector2 v ) //equality predicate.
			{ return ( _x == v._x && _y == v._y );} 
		
		public bool notequal( cVector2 v ) //inequality predicate.
			{ return !( _x == v._x && _y == v._y );} 

    //Matrix methods 
		
		public void apply( cMatrix2 M ) //set self = M*self; 
		{ /* Assume for now that the bottom row of matrix is always 0 0 1.
		This operation effectively does the scaling and rotation, followed by the
		translation. */
			copy( M.mult( this ) ); 
		}

        public virtual void copy(cVector2 v)
        {
            _x = v._x;
            _y = v._y;
        }

    //Factory Method 
        public static cVector2 randomUnitVector() 
		{ 
			float vx, vy; 
			Framework.randomOb.randomUnitPair(out vx, out vy ); 
			return new cVector2( vx, vy ); 
		} 

        public float distanceTo( cVector2 v ) 
		{ 
			float dx = _x - v._x, dy = _y - v._y; 
			return (float) Math.Sqrt( dx * dx + dy * dy ); 
		} 

		public cVector2 direction() //Return a unit vector in your direction.
		{ //Make a unit cVector2, and return it 
			float m = Magnitude;
            cVector2 dir = new cVector2();
            dir.copy(this);
			if ( m < 0.00001f ) 
			{ 
				dir._x = 1.0f; 
				dir._y = 0; 
			} 
			else 
			{ 
				dir._x /= m; 
				dir._y /= m; 
			} 
			return dir; 
		} 

		public virtual float Z
		{
			get
				 { return 0.0f;}
			set
				 { }
		}

		public virtual float Magnitude
		{
			get
				 { return (float) Math.Sqrt( _x * _x + _y * _y );}
			set
				 //Make your length be value 
		    { 
			    normalize(); 
			    _x *= value; 
			    _y *= value; 
		    }
		}

        public virtual float X
		{
			get
				 { return _x; }
		}

		public virtual float Y
		{
			get
				 { return _y; }
		}

		public virtual float Angle
		{
			get
				 /* Returns an angle between 0 and 2*PI, and for the
			 degenerate vector (0,0) it returns 0. */ 
		    { 
			    float ang = (float) Math.Atan2( _y, _x ); //Note that atan2 takes the args in reverse order.
			    //This will range between -pi and pi, and I prefer 0 to 2pi, so I fix this.
			    if ( ang < 0.0f ) 
				    ang += 2 * (float) Math.PI; 
			    if ( ang == 2 * (float) Math.PI ) 
				    ang = 0.0f; 
			    return ang; 
		    }
		}

		public virtual bool IsZero
		{
			get
				 { return _x == 0.0f && _y == 0.0f;}
		}

		public virtual bool IsPracticallyZero
		{
			get
				 { return Magnitude < PRACTICALLY_ZERO; }
		}


	}

    class cVector3
    {
        public static readonly float PRACTICALLY_ZERO = cVector2.PRACTICALLY_ZERO; /* Used by the isZero method to treat as zero
			vectors the occasional neglibible rounded off remains of a vector that should be 
			zero. See isPracticallyZero below. */
        public static readonly float PRACTICALLY_PARALLEL_COSINE = cVector2.PRACTICALLY_PARALLEL_COSINE; /* Used when you dot product two unit vectors
			together; if they're "practiclaly parallel" the dot product is greater than this. */

        private float _x, _y, _z;

        public cVector3(float ix = 0.0f, float iy = 0.0f, float iz = 0.0f)
        {
            _x = ix;
            _y = iy;
            _z = iz;
        }

        public cVector3(cVector2 v) { _x = v.X; _y = v.Y; _z = 0.0f; }

        //Mutators 

        public void set(float ix, float iy, float iz) { _x = ix; _y = iy; _z = iz; }

        public void set(float ix, float iy) { _x = ix; _y = iy; } //leave z alone 

        public void setZero() { _x = 0.0f; _y = 0.0f; _z = 0.0f; }

        public cVector3 normalize() //Make yourself of unit length and return self.
        { //Make the cVector3 a unit cVector3, and return self 
            cVector3 V = new cVector3();
            float m = Magnitude;
            if (m < 0.00001f)
            {
                _x = 1.0f;
                _y = 0;
                _z = 0;
                m = 0.0f;
            }
            else
            {
                _x /= m;
                _y /= m;
                _z /= m;
            }
            //	return m; //Used to return magnitude 

            V.copy(this);
            return V;
        }

        public virtual void copy(cVector3 V)
        {
            _x = V._x;
            _y = V._y;
            _z = V._z;
        }

        public float normalizeAndReturnMagnitude() //Make yourself of unit length and return mag.
        {
            float m = Magnitude;
            if (m < 0.00001f)
            {
                _x = 1.0f;
                _y = 0;
                _z = 0;
                m = 0.0f;
            }
            else
            {
                _x /= m;
                _y /= m;
                _z /= m;
            }
            return m;
        }

        public cVector3 roundOff() /* Get rid of any component with size less than SMALL_REAL.  This
			can be useful if you have some nagging little roundoff residue in a compoment that
			should be 0. Return self. */
        {
            cVector3 V = new cVector3();
            if ((float)Math.Abs(_x) < 0.00001f)
                _x = 0.0f;
            if ((float)Math.Abs(_y) < 0.00001f)
                _y = 0.0f;
            if ((float)Math.Abs(_z) < 0.00001f)
                _z = 0.0f;
            V.copy(this);
            return V;
        }

        public void apply(cMatrix3 M) //Apply M to self.
        { /* Assume for now that the bottom row of matrix is always 0 0 0 1.
		This operation effectively does the scaling and rotation, followed by the
		translation. Its important to use the dummy tempx and tempy otherwise the
		update values get used prematurely in the _y and _z updates. */
            float tempx, tempy;
            tempx = _x * M.mat(0, 0) + _y * M.mat(0, 1) + _z * M.mat(0, 2) + M.mat(0, 3);
            tempy = _x * M.mat(1, 0) + _y * M.mat(1, 1) + _z * M.mat(1, 2) + M.mat(1, 3);
            _z = _x * M.mat(2, 0) + _y * M.mat(2, 1) + _z * M.mat(2, 2) + M.mat(2, 3);
            _x = tempx;
            _y = tempy;
        }

        public cVector3 turn(float angle) //Means rotate around z axis.
        {
            return rotate(new cSpin(angle)); //Real constructor assumes rotation around Z axis.
        }

        public cVector3 rotate(cSpin spin) //Can be around any axis.
        {
            cVector3 V = new cVector3();
            if (spin.SpinAngle == 0.0f)
            {
                V.copy(this);
                return V;
            }
            apply(cMatrix3.rotation(spin));
            V.copy(this);
            return V;
        }

        // Projects into the xy plane, makes a unit vector, returns copy.

        //Accessors  

        public float distanceTo(cVector3 v)
        {
            float dx = _x - v._x, dy = _y - v._y, dz = _z - v._z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public bool isPracticallyEqualTo(cVector3 u) { return sub(u).Magnitude < PRACTICALLY_ZERO; }
        /* Sometimes if I do something like u = cVector3(1,0) - cVector3(1,0),
                u I can end up with a nonzero magnitude.  So I use this to winnow out
                inaccurate vectors that should be zero. */

        //Other Methods 

        public float distance(cVector3 v) //Distance 
        {
            float dx = _x - v._x, dy = _y - v._y, dz = _z - v._z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }


        // This function was created to orient a model so that it is facing
        // the direction that it is moving in -- see comments at top -- JC
        public float rotationAngle(cVector3 v)
        {
            // returns the angle to rotate this vector into the v vector,
            // only works in the xz plane

            float umag = Magnitude;
            float angle1 = (float)Math.Acos(X / umag);
            if (Z < 0)
                angle1 = 2.0f * (float)Math.PI - angle1;
            float vmag = v.Magnitude;
            float angle2 = (float)Math.Acos(v.X / vmag);
            if (v.Z < 0)
                angle2 = 2.0f * (float)Math.PI - angle2;
            return angle2 - angle1;
        }


        public float angleBetween(cVector3 v) /* The angle of u
			minus the angle of v, that is, the counterclockwise	angle gotten by
			turning from v to u.  Value between 0 and 2*PI. */
        {
            /* Use fact that dot product of two vectors is the cosine of the angle between them.  Always returns
        an acute angle, between 0.0 and PI. */
            float umag = Magnitude;
            float vmag = v.Magnitude;
            if ((umag == 0.0f) || (vmag == 0.0f)) //If either is a zero vector, just return a zero angle.
                return 0.0f;
            float cosine = mod(v) / (umag * vmag);
            if (cosine < -1.0f)
                cosine = -1.0f;
            else if (cosine > 1.0f)
                cosine = 1.0f;
            return (float)Math.Acos(cosine);
        }

        public cVector3 defaultNormal() //Useful sometimes to arbitrarily pick a normal 
        {
            if (_x != 0.0f || _y != 0.0f)
                return new cVector3(-_y, _x, 0.0f);
            else //it points along z axis up or down.
                return new cVector3(1.0f, 0.0f, 0.0f);
        }

        //Used to be overloaded operators -- JC 

        public cVector3 addassign(cVector3 u)
        {
            cVector3 V = new cVector3();
            _x += u._x;
            _y += u._y;
            _z += u._z;
            V.copy(this);
            return V;
        }

        public cVector3 subassign(cVector3 u)
        {
            cVector3 V = new cVector3();
            _x -= u._x;
            _y -= u._y;
            _z -= u._z;
            V.copy(this);
            return V;
        }

        public cVector3 multassign(float f)
        {
            cVector3 V = new cVector3();
            _x *= f;
            _y *= f;
            _z *= f;
            V.copy(this);
            return V;
        }

        public cVector3 divassign(float f)
        {
            cVector3 V = new cVector3();
            if ((float)Math.Abs(f) >= 0.00001f)
            {
                _x /= f;
                _y /= f;
                _z /= f;
            }
            V.copy(this);
            return V;
        }


        //Factory Method 
        public static cVector3 randomUnitVector()
        {
            float vx, vy, vz;
            Framework.randomOb.randomUnitTriple(out vx, out vy, out vz);
            return new cVector3(vx, vy, vz);
        }

        public cVector3 mult(float f) //Scalar prod 
        {
            return new cVector3(_x * f, _y * f, _z * f);
        }

        public static cVector3 mult(float f, cVector3 u) //Scalar prod 
        {
            return new cVector3(u._x * f, u._y * f, u._z * f);
        }

        public cVector3 div(float f) //Scalar division 
        {
            return new cVector3(_x / f, _y / f, _z / f);
        }

        public cVector3 add(cVector3 v) //cVector3 sum 
        {
            return new cVector3(_x + v._x, _y + v._y, _z + v._z);
        }

        public cVector3 sub(cVector3 v) //cVector3 difference.
        {
            return new cVector3(_x - v._x, _y - v._y, _z - v._z);
        }

        public cVector3 neg() { return new cVector3(-_x, -_y, -_z); } //Unary -

        public float mod(cVector3 v) //Dot product 
        {
            return (_x * v._x + _y * v._y + _z * v._z);
        }

        public cVector3 mult(cVector3 v) //cross product 
        {
            return new cVector3(_y * v._z - _z * v._y, -_x * v._z + _z * v._x,
                _x * v._y - _y * v._x);
        }

        public bool equal(cVector3 v) //equality predicate.
        { return (_x == v._x && _y == v._y && _z == v._z); }

        public bool notequal(cVector3 v) //inequality predicate.
        { return !(_x == v._x && _y == v._y && _z == v._z); }

        //OpenGL Methods 

        //      This is Rucker's glVertex function, still written in C++, using gl.h.
        //      I commented it out, because it is not currently being used; if the
        //      cGraphicsOpenGL class were reworked in processing model triangles, it may be 
        //      useful -- JC

        //		void glVertex(){::glVertex3fv( & _x );} 

        
        public virtual float Z
        {
            get
            { return _z; }
            set
            { _z = value; }
        }

        public virtual float Magnitude
        {
            get
            //Return the cVector3's length.
            {
                return (float)Math.Sqrt(_x * _x + _y * _y + _z * _z);
            }
            set
            //Make your length be value 
            {
                normalize();
                _x *= value;
                _y *= value;
                _z *= value;
            }
        }

        public virtual cVector3 XYUnitVector
        {
            get
            { return new cVector3(_x, _y, 0.0f).normalize(); }
        }

        public virtual float X
        {
            get
            { return _x; }
        }

        public virtual float Y
        {
            get
            { return _y; }
        }

        public virtual float Angle
        {
            get
            //Collapse to 2D and get that angle.
            { //Just copy the cVector2 code.
                float ang = (float)Math.Atan2(_y, _x); //Note that atan2 takes the args in reverse order.
                //This will range between -pi and pi, and I prefer 0 to 2pi, so I fix this.
                if (ang < 0.0f)
                    ang += 2 * (float)Math.PI;
                if (ang == 2 * (float)Math.PI)
                    ang = 0.0f;
                return ang;
            }
        }

        public virtual cVector3 Direction
        {
            get
            //Return a unit vector in your direction.
            { //Make a unit cVector3, and return it 
                float m = Magnitude;
                cVector3 dir = new cVector3();
                dir.copy(this);
                if (m < 0.00001f)
                {
                    dir._x = 1.0f;
                    dir._y = 0;
                    dir._z = 0;
                }
                else
                {
                    dir._x /= m;
                    dir._y /= m;
                    dir._z /= m;
                }
                return dir;
            }
        }

        public virtual bool IsZero
        {
            get
            { return _x == 0.0f && _y == 0.0f && _z == 0.0f; }
        }

        public virtual bool IsPracticallyZero
        {
            get
            { return Magnitude < PRACTICALLY_ZERO; }
        }


    }

    //Helper class 
    class cSpin
    {
        public static readonly ushort XAXISTYPE = 1;
        public static readonly ushort YAXISTYPE = 2;
        public static readonly ushort ZAXISTYPE = 3;
        public static readonly ushort ARBITRARYAXISTYPE = 4;
        private float _spinangle;
        private cVector3 _spinaxis;
        private ushort _axistype;

        public cSpin()
        {
            _spinaxis = new cVector3();
            _spinangle = 0.0f;
            _spinaxis = new cVector3(1.0f, 0.0f, 0.0f);
            _axistype = ZAXISTYPE;
        }

        public cSpin(cVector3 spinvector)
        {
            _spinaxis = new cVector3();
            _spinaxis.copy(spinvector);
            _axistype = ARBITRARYAXISTYPE;
            _spinangle = _spinaxis.Magnitude;
            _spinaxis.normalize();
        }

        public cSpin(float spinangle, cVector3 spinaxis)
        {
            _spinaxis = new cVector3();
            _spinangle = spinangle;
            _spinaxis.copy(spinaxis);
            _axistype = ARBITRARYAXISTYPE;
            _spinaxis.normalize();
        }

        public cSpin(float angle)
        {
            _spinangle = angle;
            _spinaxis = new cVector3(0.0f, 0.0f, 1.0f);
            _axistype = ZAXISTYPE;
        }

        public void setZero() { _spinangle = 0.0f; _axistype = ZAXISTYPE; _spinaxis = new cVector3(0.0f, 0.0f, 1.0f); }

        public static cSpin mult(float f, cSpin spin)
        {
            cSpin fspin = new cSpin();
            fspin.copy(spin);
            fspin._spinangle *= f;
            return fspin;
        }

        public virtual void copy(cSpin spin)
        {
            _axistype = spin._axistype;
            _spinangle = spin._spinangle;
            _spinaxis = spin._spinaxis;
        }

        public virtual float SpinAngle
        {
            get
            { return _spinangle; }
        }

        public virtual cVector3 SpinAxis
        {
            get
            {
                cVector3 V = new cVector3();
                V.copy(_spinaxis);
                return V;
            }
        }

        public virtual ushort AxisType
        {
            get
            { return _axistype; }
        }


    }

    // ----- Two and Three Dimensional Matrixes, or Transformations -------

    class cMatrix2
    {
        private float[,] _mat = new float[3, 3];
        /*Note that _mat[i][j] means the entry in the i_th row and the j_th
        column, so this is similar to usual matrix notation like m_ij.  One
        minor confusion is that C language indices start at 0, but matrix
        indices usually start at 1.*/

        //Constructor 

        public cMatrix2() { identity(); } //Call the Identity operation at construction.

        public virtual void copy(cMatrix2 M)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    _mat[i, j] = M._mat[i, j];
        }

        public cMatrix2(float e00, float e10, float e20, float e01, float e11, float e21,
            float e02, float e12, float e22) /* Feed the args in as columns, so first three are first
			column, and so on. */
        {
            set(e00, e10, e20, e01, e11, e21, e02, e12, e22);
        }

        public cMatrix2(cVector2 col0, cVector2 col1, cVector2 col2)
        /* This can be used to make a cMatrix2(tangent, normal, position) to
            set the cols to tangent, normal and position. */
        {
            _mat[0, 0] = col0.X;
            _mat[1, 0] = col0.Y;
            _mat[2, 0] = 0.0f;
            _mat[0, 1] = col1.X;
            _mat[1, 1] = col1.Y;
            _mat[2, 1] = 0.0f;
            _mat[0, 2] = col2.X;
            _mat[1, 2] = col2.Y;
            _mat[2, 2] = 1.0f;
        }

        public cMatrix2(cVector2 col0, cVector2 col1, cVector2 col2, cVector2 col3)
        /* To match the four arg constructor of cVector3, we include this, but ignore the binormal argument.
            Note it's binormal we ignore and NOT position.  This is so cMatrix2(tangent, normal, binormal,
                position) will set the cols to tangent, normal and position. */
        { /* Note that we IGNORE col2 and USE col3.  This is not a typo, I really mean to ignore col2, the 
		second to last argument. This is because we normally feed in the args tangent, normal,
		(meaningless in 2D) binormal, and position.  */
            _mat[0, 0] = col0.X;
            _mat[1, 0] = col0.Y;
            _mat[2, 0] = 0.0f;
            _mat[0, 1] = col1.X;
            _mat[1, 1] = col1.Y;
            _mat[2, 1] = 0.0f;
            _mat[0, 2] = col3.X;
            _mat[1, 2] = col3.Y;
            _mat[2, 2] = 1.0f;
        }

        //Accessor 
        public float mat(int i, int j) { return _mat[i, j]; }

        public cVector2 column(int i)
        {
            return new cVector2(_mat[0, i], _mat[1, i]);
        }

        //Mutators 
        public void identity() //Set to the identity transformation.
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    _mat[i, j] = (i == j) ? 1 : 0;
        }

        public void set(float e00, float e10, float e20, float e01, float e11, float e21,
            float e02, float e12, float e22) /* Feed the args in as columns, so first three are first
			column, and so on. */
        {
            _mat[0, 0] = e00;
            _mat[1, 0] = e10;
            _mat[2, 0] = e20;
            _mat[0, 1] = e01;
            _mat[1, 1] = e11;
            _mat[2, 1] = e21;
            _mat[0, 2] = e02;
            _mat[1, 2] = e12;
            _mat[2, 2] = e22;
        }

        public void setColumn(int j, cVector2 u) //j is 0, 1, or 2.
        {
            _mat[0, j] = u.X;
            _mat[1, j] = u.Y;
        }

        public void setColumn(int j, cVector3 u) //j is 0, 1, or 2.
        {
            _mat[0, j] = u.X;
            _mat[1, j] = u.Y;
        }

        public void setRotationAndScale(float ang, float sc = 1.0f)
        { //This transformation does the Rotation and Scale 
            _mat[0, 0] = sc * (float)Math.Cos(ang);
            _mat[0, 1] = -sc * (float)Math.Sin(ang);
            _mat[1, 0] = sc * (float)Math.Sin(ang);
            _mat[1, 1] = sc * (float)Math.Cos(ang);
        }

        public void setRotationAndScaleAboutCenter(cVector2 center, float ang, float sc = 1.0f)
        { /* This transformation does the Rotation and Scale about center.  Because these
		transformations are linear, we can do the scale and rotation and then do 
		a translation to undo the motion caused the rotation and scaling of the center
		point by the scale and rotation. */
            cVector2 centerimage = new cVector2(center.X, center.Y);
            setRotationAndScale(ang, sc);
            _mat[0, 2] = _mat[1, 2] = 0.0f;
            centerimage.apply(this);
            translate(center.sub(centerimage));
        }

        public void translate(cVector2 trans)
        {
            _mat[0, 2] += trans.X;
            _mat[1, 2] += trans.Y;
        }

        public void orthonormalize() //Make first two columns an orthonormal basis.
        {
            cVector2 tan = column(0), norm = column(1);
            tan.normalize();
            norm.subassign(tan.mult(norm.mod(tan)));
            norm.normalize();
            setColumn(0, tan);
            setColumn(1, norm);
        }

        //Matrix methods 
        //Overloaded operators 
        public cMatrix2 multassign(cMatrix2 M)
        { //Assume for now that the bottom row of the matrix is always 0 0 1 
            cMatrix2 m = new cMatrix2();
            copy(mult(M));
            m.copy(this);
            return m;
        }

        public cMatrix2 mult(cMatrix2 L)
        { //Assume for now that the bottom row of the matrix is always 0 0 1 
            cMatrix2 M = new cMatrix2();

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 3; j++)
                {
                    M._mat[i, j] = 0.0f;
                    for (int k = 0; k < 3; k++)
                        M._mat[i, j] += _mat[i, k] * L._mat[k, j];
                }
            return M;
        }

        public cVector2 mult(cVector2 v)
        { /* Assume for now that the bottom row of matrix is always 0 0 1.
		This operation effectively does the scaling and rotation, followed by the
		translation. */
            return new cVector2(v.X * _mat[0, 0] + v.Y * _mat[0, 1] + _mat[0, 2],
                v.X * _mat[1, 0] + v.Y * _mat[1, 1] + _mat[1, 2]);
        }

        public static cMatrix2 mult(float f, cMatrix2 M)
        {
            cMatrix2 m = new cMatrix2();
            m.copy(M);
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    m.Elements[i, j] *= f;
            return m;
        }

        //Static Factory methods----------------------

        public static cMatrix2 identityMatrix() { return new cMatrix2(); }

        public static cMatrix2 xRotation(float ang) { return new cMatrix2(); } //return identity.

        public static cMatrix2 yRotation(float ang) { return new cMatrix2(); } //return identity.

        public static cMatrix2 zRotation(float ang)
        { //Rotate x-axis towards y-axis by ang radians.
            cMatrix2 M = new cMatrix2();

            M._mat[0, 0] = (float)Math.Cos(ang);
            M._mat[0, 1] = -(float)Math.Sin(ang);
            M._mat[1, 0] = -M._mat[0, 1]; //sin(ang) 
            M._mat[1, 1] = M._mat[0, 0]; //cos(ang) 
            return M;
        }

        public static cMatrix2 rotation(float ang) { return rotation(new cSpin(ang)); }

        public static cMatrix2 rotation(cSpin spin) /* ignore axis variable,
			its only here for consistency with the Vector3 interface. */
        { //Rotate x-axis towards y-axis by ang radians.
            cMatrix2 M = new cMatrix2();
            if (spin.SpinAngle == 0.0f)
                return M; //identity.
            M._mat[0, 0] = (float)Math.Cos(spin.SpinAngle);
            M._mat[0, 1] = -(float)Math.Sin(spin.SpinAngle);
            M._mat[1, 0] = -M._mat[0, 1]; //sin(spin.spinangle()) 
            M._mat[1, 1] = M._mat[0, 0]; //cos(spin.spinangle()) 
            return M;
        }

        public static cMatrix2 rotationFromUnitToUnit(cVector2 u, cVector2 v) /* Assume that
			u and v are unit vectors.  This gives a matrix that rotates u into v. */
        {
            /* ONly call this on unit vectors as arguments.  The components of u are cos and sin of
        the angle a that u makes with the x axis.  The components of v are cos and sin of angle b
        that v mkes with the x-axis.  We want to turn counterclockwise by the angle b-a. So now
        we get the componetns of the turnangle b-a.*/
            float turncos = v.X * u.X + v.Y * u.Y; /* Use the trig formula for
		 cos(b - a)= cosb cosa + sinb sina */
            float turnsin = v.Y * u.X - v.X * u.Y; //sin(b-a) = sinb cosa - cosb sina 
            //Make a z-rotation matrix.  Remember that the constructor args go in by columns.
            return new cMatrix2(turncos, turnsin, 0.0f, //col0 
            -turnsin, turncos, 0.0f, //col1 
            0.0f, 0.0f, 1.0f); //col2 
        }

        public static cMatrix2 scale(float scalefactor)
        {
            cMatrix2 M = new cMatrix2();

            M._mat[0, 0] = scalefactor;
            M._mat[1, 1] = scalefactor;
            return M;
        }

        public static cMatrix2 translation(cVector2 trans)
        {
            cMatrix2 M = new cMatrix2();

            M._mat[0, 2] = trans.X;
            M._mat[1, 2] = trans.Y;
            return M;
        }

        public virtual float[,] Elements
        {
            get
            { return _mat; }
        }

        public virtual cVector2 LastColumn
        {
            get
            { return column(2); }
            set
            { setColumn(2, value); }
        }

        public virtual float ScaleFactor
        {
            get
            //How much scaling does this do?  Assume isotropic, so test on unit vector of XAXIS 
            {
                cVector2 uvectorimage = new cVector2(_mat[0, 0], _mat[1, 0]);
                return uvectorimage.Magnitude;
            }
        }

        public virtual float ZTranslation
        {
            set
            { }
        }

        public virtual cMatrix2 Inverse
        {
            get
            { /*This only works if we assume the matrix is a rigid-body matrix, i.e.
	that its cVector2 columns are orthonormal unit vectors as are its cVector2
	rows (,ignoring the third column.)  You can have any translation you
	like in the third column. See comment on cMatrix2::rigidBodyINverse. */
                cMatrix2 inv = new cMatrix2();

                for (int i = 0; i < 2; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        inv._mat[i, j] = _mat[j, i];
                        inv._mat[i, 2] -= inv._mat[i, j] * _mat[j, 2];
                    }
                return inv;
            }
        }

        public virtual cMatrix2 Transpose
        {
            get

        //See the comments on the rigidBody methods in cMatrix3 and in the vectortransformation.cpp.
            {
                cMatrix2 transpose = new cMatrix2();
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        transpose._mat[i, j] = _mat[j, i];
                return transpose;
            }
        }

        public virtual cMatrix2 NormalTransformation
        {
            get
            { /*See the comment on cMatrix3::rigidBodyNormalTransformation() for an explanation of this. */
                return Inverse.Transpose;
            }
        }


    }

    class cMatrix3
    {
        private float[,] _mat = new float[4, 4];
        /*Note that _mat[i][j] means the entry in the i_th row and the j_th
        column, so this is similar to usual matrix notation like m_ij.  One
        minor confusion is that C language indices start at 0, but matrix
        indices usually start at 1.*/

        //Constructor and copy 

        public cMatrix3() { identity(); } //Call the Identity operation at construction.

        public cMatrix3(float e00, float e10, float e20, float e30, float e01, float e11, float e21, float e31, float e02, float e12, float e22, float e32, float e03, float e13, float e23, float e33) /* We
			feed the arguments in column order, that is the first four are the first column, and so on. */
        {
            set(e00, e10, e20, e30, e01, e11, e21, e31, e02, e12, e22, e32, e03, e13, e23, e33);
        }

        public cMatrix3(float[] vert) //Assume vert is an array of 16.
        {
            set(vert);
        }

        public cMatrix3(cVector3 col0, cVector3 col1, cVector3 col2, cVector3 col3) /*This will set the cols to tangent, normal and position. */
        {
            _mat[0, 0] = col0.X;
            _mat[1, 0] = col0.Y;
            _mat[2, 0] = col0.Z;
            _mat[3, 0] = 0.0f;
            _mat[0, 1] = col1.X;
            _mat[1, 1] = col1.Y;
            _mat[2, 1] = col1.Z;
            _mat[3, 1] = 0.0f;
            _mat[0, 2] = col2.X;
            _mat[1, 2] = col2.Y;
            _mat[2, 2] = col2.Z;
            _mat[3, 2] = 0.0f;
            _mat[0, 3] = col3.X;
            _mat[1, 3] = col3.Y;
            _mat[2, 3] = col3.Z;
            _mat[3, 3] = 1.0f;
        }

        public virtual void copy(cMatrix3 M)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    _mat[i, j] = M._mat[i, j];
        }

        public cMatrix3(cMatrix2 M) /* Use this so you can upgrade a cMatrix2 for loading into OpenGL */
        {
            _mat[0, 0] = M.mat(0, 0);
            _mat[1, 0] = M.mat(1, 0);
            _mat[2, 0] = 0.0f;
            _mat[3, 0] = 0.0f;
            _mat[0, 1] = M.mat(0, 1);
            _mat[1, 1] = M.mat(1, 1);
            _mat[2, 1] = 0.0f;
            _mat[3, 1] = 0.0f;
            _mat[0, 2] = 0.0f;
            _mat[1, 2] = 0.0f;
            _mat[2, 2] = 1.0f;
            _mat[3, 2] = 0.0f;
            _mat[0, 3] = M.mat(0, 2);
            _mat[1, 3] = M.mat(1, 2);
            _mat[2, 3] = 0.0f;
            _mat[3, 3] = 1.0f;
        }

        //Accessor 
        public float mat(int i, int j) { return _mat[i, j]; }

        public cVector3 column(int i)
        {
            return new cVector3(_mat[0, i], _mat[1, i], _mat[2, i]);
        }

        //Mutators 
        public void identity() //Set to the identity transformation.
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    _mat[i, j] = (i == j) ? 1 : 0;
        }

        public void set(float e00, float e10, float e20, float e30, float e01, float e11,
            float e21, float e31, float e02, float e12, float e22, float e32, float e03,
            float e13, float e23, float e33) /* We
			feed the arguments in column order, that is the first four are the first 
                                               column, and so on. */
        {
            _mat[0, 0] = e00;
            _mat[1, 0] = e10;
            _mat[2, 0] = e20;
            _mat[3, 0] = e30;
            _mat[0, 1] = e01;
            _mat[1, 1] = e11;
            _mat[2, 1] = e21;
            _mat[3, 1] = e31;
            _mat[0, 2] = e02;
            _mat[1, 2] = e12;
            _mat[2, 2] = e22;
            _mat[3, 2] = e32;
            _mat[0, 3] = e03;
            _mat[1, 3] = e13;
            _mat[2, 3] = e23;
            _mat[3, 3] = e33;
        }

        public void set(float[] vert) //Assume vert is an array of 16.
        {
            set(vert[0], vert[1], vert[2], vert[3], vert[4], vert[5], vert[6], vert[7],
                vert[8], vert[9], vert[10], vert[11], vert[12], vert[13], vert[14], vert[15]);
        }

        public void setColumn(int j, cVector3 u) //j is 0, 1, 2, or 3.
        {
            _mat[0, j] = u.X;
            _mat[1, j] = u.Y;
            _mat[2, j] = u.Z;
        }

        public void translate(cVector3 trans)
        {
            _mat[0, 3] += trans.X;
            _mat[1, 3] += trans.Y;
            _mat[2, 3] += trans.Z;
        }

        public void orthonormalize() //Make first three columns an orthonormal basis.
        {
            cVector3 tan = column(0), norm = column(1), binorm = column(2);
            tan.normalize();
            norm.subassign(tan.mult(norm.mod(tan)));
            norm.normalize();
            binorm = tan.mult(norm);
            setColumn(0, tan);
            setColumn(1, norm);
            setColumn(2, binorm);
        }

        //Mutators 
        //Used to be overloaded operators -- JC 
        public cMatrix3 multassign(cMatrix3 M)
        { //Assume for now that the bottom row of the matrix is always 0 0 1 
            cMatrix3 m = new cMatrix3();
            copy(mult(M));
            m.copy(this);
            return m;
        }

        public cMatrix3 mult(cMatrix3 L)
        { //Assume for now that the bottom row of the matrix is always 0 0 0 1 
            cMatrix3 M = new cMatrix3();

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 4; j++)
                {
                    M._mat[i, j] = 0.0f;
                    for (int k = 0; k < 4; k++)
                        M._mat[i, j] += _mat[i, k] * L._mat[k, j];
                }
            return M;
        }

        public cVector3 mult(cVector3 V)
        { //Assume for now that the bottom row of matrix is always 0 0 0 1 
            return new cVector3(
            V.X * _mat[0, 0] + V.Y * _mat[0, 1] + V.Z * _mat[0, 2] + _mat[0, 3],
            V.X * _mat[1, 0] + V.Y * _mat[1, 1] + V.Z * _mat[1, 2] + _mat[1, 3],
            V.X * _mat[2, 0] + V.Y * _mat[2, 1] + V.Z * _mat[2, 2] + _mat[2, 3]
            );
        }

        public static cMatrix3 mult(float f, cMatrix3 M)
        {
            cMatrix3 m = new cMatrix3();
            m.copy(M);
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    m.Elements[i, j] *= f;
            return m;
        }
        //Factory methods 
        /* The following  operations create special matrices*/

        public static cMatrix3 identityMatrix() { return new cMatrix3(); }

        public static cMatrix3 xRotation(float ang)
        { //Rotate y-axis towards z-axis by ang radians.  Pos rot bucks y "up" and z "down".
            cMatrix3 M = new cMatrix3();

            M._mat[1, 1] = (float)Math.Cos(ang);
            M._mat[1, 2] = -(float)Math.Sin(ang);
            M._mat[2, 1] = (float)Math.Sin(ang);
            M._mat[2, 2] = (float)Math.Cos(ang);
            return M;
        }

        public static cMatrix3 yRotation(float ang)
        { //Rotate z-axis towards x-axis by ang radians.  Pos rot bucks z "up, x axis "down".
            cMatrix3 M = new cMatrix3();

            M._mat[0, 0] = (float)Math.Cos(ang);
            M._mat[0, 2] = (float)Math.Sin(ang); //I switched the sign of this to make this match Matrix3::rotation.
            M._mat[2, 0] = -(float)Math.Sin(ang); //swithced this too.
            M._mat[2, 2] = (float)Math.Cos(ang);
            return M;
        }

        public static cMatrix3 zRotation(float ang)
        { //Rotate x-axis towards y-axis by ang radians. Pos rot bucks y "down".
            cMatrix3 M = new cMatrix3();

            M._mat[0, 0] = (float)Math.Cos(ang);
            M._mat[0, 1] = -(float)Math.Sin(ang);
            M._mat[1, 0] = -M._mat[0, 1]; //sin(ang) 
            M._mat[1, 1] = M._mat[0, 0]; //cos(ang) 
            return M;
        }

        public static cMatrix3 rotation(float ang) { return rotation(new cSpin(ang)); }

        public static cMatrix3 rotation(cSpin spin)
        {
            if (spin.SpinAngle == 0.0f)
                return new cMatrix3(); //identity.
            float c = (float)Math.Cos(spin.SpinAngle);
            float ccomp = 1.0f - c;
            float s = (float)Math.Sin(spin.SpinAngle);
            float x = spin.SpinAxis.X;
            float y = spin.SpinAxis.Y;
            float z = spin.SpinAxis.Z;
            /* Now feed in the numbers a column at a time (not a row at a time, as our cMatrix3 
        constructor	gets fed by columns).  I copied these formulae from the glRotate entry of the
        OpenGL Reference Manual, and triple checked I got it right. Note that since we feed in by 
        columns, what you see here is the transpose of what you see in the Manual. */
            return new cMatrix3(
                x * x * ccomp + c, x * y * ccomp + z * s, x * z * ccomp - y * s, 0.0f,
                x * y * ccomp - z * s, y * y * ccomp + c, y * z * ccomp + x * s, 0.0f,
                x * z * ccomp + y * s, y * z * ccomp - x * s, z * z * ccomp + c, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
        }

        public static cMatrix3 rotationFromUnitToUnit(cVector3 u, cVector3 v) /* Assume that
			u and v are unit vectors.  This gives a matrix that rotates u into v. */
        {
            cMatrix3 M = new cMatrix3(); //Identity to start with 
            float angle = u.angleBetween(v); //Will be 0 if either vector is zero or if they point the same way.
            if (angle == 0.0f)
                return M; //return the identity in the degenerate cases.
            cVector3 axis = u.mult(v); //We don't need to normalize as cSpin consturctor or OpenGL will normalize.
            M = cMatrix3.rotation(new cSpin(angle, axis));
            return M;
        }

        public static cMatrix3 translation(cVector3 trans)
        {
            cMatrix3 M = new cMatrix3();

            M._mat[0, 3] = trans.X;
            M._mat[1, 3] = trans.Y;
            M._mat[2, 3] = trans.Z;
            return M;
        }

        public static cMatrix3 scale(float scalefactor)
        {
            cMatrix3 M = new cMatrix3();

            M._mat[0, 0] = scalefactor;
            M._mat[1, 1] = scalefactor;
            M._mat[2, 2] = scalefactor;
            return M;
        }

        public static cMatrix3 scale(float xscale, float yscale, float zscale)
        {
            cMatrix3 M = new cMatrix3();

            M._mat[0, 0] = xscale;
            M._mat[1, 1] = yscale;
            M._mat[2, 2] = zscale;
            return M;
        }

        public virtual float[,] Elements
        {
            get
            { return _mat; }
        }

        public virtual cVector3 LastColumn
        {
            get
            { return column(3); }
            set
            { setColumn(3, value); }
        }

        public virtual float ScaleFactor
        {
            get
            //How much scaling does this do?  Assume isotropic, so test on unit vector of XAXIS 
            {
                cVector3 uvectorimage = new cVector3(_mat[0, 0], _mat[1, 0], _mat[2, 0]);
                return uvectorimage.Magnitude;
            }
        }

        public virtual float ZTranslation
        {
            set
            { LastColumn = new cVector3(LastColumn.X, LastColumn.Y, value); }
        }

        public virtual cMatrix3 Transpose
        {
            get
            {
                cMatrix3 transpose = new cMatrix3();
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        transpose._mat[i, j] = _mat[j, i];
                return transpose;
            }
        }

        public virtual cMatrix3 Inverse
        {
            get
            /*This only works if we assume the matrix is a rigid-body matrix, 
       i.e. that its cVector3 columns are orthonormal unit vectors as are its cVector3
       rows (,ignoring the foruth column.)  You can have any translation you like in the fourth column. */
            { /*This only works if we assume the matrix is a rigid-body matrix, i.e.
	that its cVector3 columns are orthonormal unit vectors as are its cVector3
	rows (,ignoring the foruth column.)  You can have any translation you
	like in the fourth column.  The inverse of such a matrix just takes the
	transpose of the 3x3 rotational part, and then adjusts the fourth column
	so that the matrix product will give zeroes there.  Thanks to Bob Holt
	of Autodesk for showing me this trick.*/
                cMatrix3 inv = new cMatrix3(); //default constructor sets it to identity.
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        inv._mat[i, j] = _mat[j, i];
                        inv._mat[i, 3] -= inv._mat[i, j] * _mat[j, 3];
                    }
                return inv;
            }
        }

        public virtual cMatrix3 NormalTransformation
        {
            get
            { /*As with inverse we should only call this on a rigid-body matrix,  i.e. on
		a matrix such that its cVector3 columns are orthonormal unit vectors, as are its cVector3
		rows (,ignoring the foruth column.)  You can have any translation you like in the fourth
		column. as define in the fourth column.  The idea here is that if tan' = mat*tan, and normal
		was the normal to tan, we want to find the right value for the nmat so that norm' = N*norm.
		Note that here I'm using mat and nmat to stand for matrices, the first is this caller matrix,
		the second is the matrix I want to return.  I'll also use matinverse to stand for 
		mat.inverse().
		We know norm%tan is 0, using our dot product symbol %.  Suppose we write a varaible in CAPITALS
		to mean the transpose (flip of the ij positions). We think of our cVector3 as column vectors,
		so NORM would be a row vector, and we could write the dot product norm%tan as NORM*tan, where
		we think of * as ordinary matrix multiplication.
		NORM*tan = 0, since norm and tan are perpendicular.
		NORM*matinverse*mat*tan = 0, since matinverse*mat is the identity.
		(NORM*matinverse)*tan' = 0, since tan1 = mat*tan.
		NORM' = NORM*matinverse, since NORM' is to be the normal to tan'.
		norm' = MATINVERSE*norm, since for matrices, transpose(b)*transpose(a) = transpose(a*b),
			and transpose(transpose(a)) = a.
		So we return the transpose of the inverse.  After all this work, note that if
		the fourth column of _mat is just 0001, then the matrix returned is just _mat! */
                return Inverse.Transpose;
            }
        }


    }

    class cLine
    {
        public cVector3 _origin;
        public cVector3 _tangent;

        public cLine()
        {
            _tangent = new cVector3(1, 0, 0); //Default origin is (0,0,0) 
        }

        public cLine(cVector3 origin, cVector3 tangent)
        {
            _origin = new cVector3(origin.X, origin.Y, origin.Z);
            _tangent = new cVector3(tangent.X, tangent.Y, tangent.Z);
        }

        public float distanceTo(cVector3 point)
        {
            /* We draw a segment form the point to the _origin that lies on the line.  Then we take out
        the compoent of this segment that lies parallel to the line to get a segment that goes
        directly from the point to the line, makign a right angle with it. */
            cVector3 pointtoline = _origin.sub(point);
            cVector3 wastemotion = _tangent.mult(pointtoline.mod(_tangent));
            pointtoline.subassign(wastemotion);
            return pointtoline.Magnitude;
        }

        public float lineCoord(cVector3 point) /* Project point onto the line, then
			take it's coord wiht the line numbered so _origin is 0.0, _origin+_tangent is 1.0, 
			and so on. */
        {
            /* Project point onto the line, then take it's coord wiht the line numbered so _origin
            is 0.0, _origin+_tangent is 1.0, and so on. */
            cVector3 linetopoint = point.sub(_origin);
            return linetopoint.mod(_tangent);
        }
    }

    class cPlane
    {
        public cVector3 _origin;
        public cVector3 _binormal;

        public cPlane()
        {
            _binormal = new cVector3(0, 0, 1); //Default origin is (0,0,0) 
        }

        public cPlane(cVector3 origin, cVector3 binormal)
        {
            _origin = new cVector3(origin.X, origin.Y, origin.Z);
            _binormal = new cVector3(binormal.X, binormal.Y, binormal.Z);
        }

        public cVector3 project(cVector3 point)
        {
            cVector3 topoint = point.sub(_origin);
            topoint.subassign(_binormal.mult(topoint.mod(_binormal))); /* Subtract off any component that lies
			perpendicular to the plane. */
            return _origin.add(topoint);
        }

        public cVector3 intersect(cLine line)
        {
            /* We give the line as the points P(t) of the form (line._origin + t*line._tangent), and we specify
        the plane as the points P such that P-_origin % _binormal = 0;  To find our intersection,
        we look for the t such that P(t) satisfies the plane condition.  That is, we want a t such that
        ((line._origin + t*line._tangent) - _origin)%_binormal == 0; Since % distributes over
        + and -, this is t = (_origin - line._origin)%binormal / line._tangent%binormal; The only difficulty
        is to avoid dividing by zero, which is the case where line is parallel to the plane. */

            float denominator = line._tangent.mod(_binormal); //We want to take the reciprocal of this.
            if (Math.Abs(denominator) < 0.00001f)
            {
                if (denominator >= 0)
                    denominator = 1000000000.0f;
                else
                    denominator = -1000000000.0f;
            }
            else
                denominator = 1.0f / denominator;
            float t = (_origin.sub(line._origin)).mod(_binormal.mult(denominator));
            cVector3 planepoint = (line._origin.add(line._tangent.mult(t))); /* This is almost your answer, but
			it will definitely be off the plane in case the line was parallel to the plane, and it
			may have slopped off anyway.  So fix this. */
            return project(planepoint);
        }
    }
}
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         