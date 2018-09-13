// begin Rucker's comment

/* This is a set of 32-bit-based randomizing functions written by Rudy Rucker,
rucker@mathcs.sjsu.edu.  The functions use a standard C library technique. The code
is written to be fully portable, although there is one line you need to comment in
or out at the head of Randomizer.cpp according to whether or not you use Microsoft
MFC.
	 These randomizing functions are based on a modular scheme derived
from the Microsoft implmentation of the C library randomizer. In the Microsoft
implementation, the C Library int rand() function works by rand works by
maintaining a _holdrand variable and iterating _holdrand * 214013 + 2531011.
rand() returns (_holdrand >> 16) & 07FFF, which is a 15 bit positive short
integer.  We use the same scheme, but tailor it to return a 32 bit unsigned 
int integer. Returning _holdrand gives too much correlation, so we actually
execute the _holdrand update twice, and use the high words of the two
successive _holdrands as the upper and lower words of the value we return.
	The Randomizer.cpp file includes a historical note at the end about an 
unsuccessful attempt to base the randomizer on Wolfram's CA Rule 30.
	We  make cRandomizer a singleton class, menaing that an application can only make
and use one instance of it.  To accomplish this, we (a) make the constructors private,
(b) add a public pinstance() method that returns a pointer to a unique static readonly cRandomizer*
object _pinstancesingleton which is initially NULL(c) make the pinstance() implementation 
resposible for calling "new" to allocate _pinstancesingleton if it is NULL and (d) add
a static readonly deleteSingleton() method to be called by the App to delete _pinstancesingleton
at exit.
 */
// end Rucker's comment

//begin Childs comment

// I've had to change almost every function in here.
// The mutate functions were left pretty much intact from Rucker's original code -- JC

//end Childs comment

using System;

namespace ACFramework
{
    class cRandomizer
    {
        private static Random _pinstancesingleton = null;
        public cRandomizer() { //Uses the C# randomizer and seeds 
                            // using C#'s time-dependent generated seed
            if ( _pinstancesingleton == null )
                _pinstancesingleton = new Random( );
            }
        public cRandomizer( Int32 seed ) { // seeds with a specific seed
                    // On each execution, it will give the same random numbers
                    // so it is useful for debugging
	        if ( _pinstancesingleton == null )
                _pinstancesingleton = new Random( seed );
            }

        public uint random() { //Return a random uint
            return (uint)((long)_pinstancesingleton.Next(Int32.MinValue, Int32.MaxValue) -
                Int32.MinValue);
            }
        public uint random(uint n) {
         //Return a uint betweeon 0 and n - 1
            return (uint)((long)_pinstancesingleton.Next(Int32.MinValue, (int)(Int32.MinValue + (long)n )) -
                Int32.MinValue);
        }
        public int random(int lon, int hin) { //int between lon and hin inclusive
            return _pinstancesingleton.Next( lon, hin + 1 ); }
        public bool randomBOOL() { // randomly returns true or false
            int value = _pinstancesingleton.Next( 0, 2 );
            return (value == 0)? false : true;
            }
        public bool randomBOOL( float truthweight ) { // Return true truthweight often.
            double value = _pinstancesingleton.NextDouble( );
            return ( value <= (double) truthweight )? true : false;
            }
        public byte randomByte( ) { //Return a byte between 0 and 255
            return (byte) _pinstancesingleton.Next( 0, 256 ); }
        public ushort randomShort(ushort n) { // Short between 0 and n-1
            return (ushort) _pinstancesingleton.Next( 0, (int) n ); } 
        public float randomReal( ) { //A real between 0.0 and 1.0
            return (float) _pinstancesingleton.NextDouble(); }
        public float randomSignedReal( ) { //A real between -1.0 and 1.0
            return randomReal( -1.0f, 1.0f ); }
        public float randomReal(float lo, float hi) { //A real between lo and hi
            return lo + (float) _pinstancesingleton.NextDouble() * (hi - lo); }
        public float mutate(float base1, float lo, float hi, float percent) { //Mutate base by percent of size.
	        if (percent == 1.0)
		        return randomReal(lo,hi);
	        float temp, range = hi-lo;
	        temp = base1 + randomSignedReal()*percent*range;
	        if (temp<lo) temp = lo; 
            if (temp > hi) temp = hi;
	        return temp;
            }
        public int mutate(int base1, int lo, int hi, float percent) { //Mutate base by percent of size.
	        if (percent == 1.0)
		        return random(lo,hi);
	        int temp, range = hi-lo;
	        temp = (int)(base1 + randomSignedReal()*percent*range);
	        if (temp<lo) temp = lo; 
            if (temp > hi) temp = hi;
	        return temp;
            }
        public int mutateColor(int basecolor, float percent) { //Mutate a color.
	        if (percent == 1.0)
		        return randomColor();
            return (int) ((1.0 - percent)*basecolor + percent*randomColor()); 
        }
        public float randomSign( ) { //1.0 or -1.0
            return ( randomBOOL() )? -1.0f : 1.0f; }
        public void randomUnitDiskPair(out float x, out float y) {
        // Makes (x,y) a random point with distance <= 1 from (0,0)
   	    
        // Rucker's implementation is commented out below.  I'm a little 
        // concerned that there would be an unusually low chance for
        // one of the pair members to be close to 1.0, so I'm going
        // to use a different -- although probably unfortunately slower --
        // approach to the problem.  -- JC

        //    x = randomSignedReal();
	    //    y = randomSignedReal();
	    //    while (x*x + y*y > 1.0)
	    //    {
		//        x = randomSignedReal();
		//        y = randomSignedReal();
	    //    }

            if ( randomBOOL() ) {
                x = randomSignedReal();
                float temp = (float) Math.Sqrt( 1.0 - x * x );
                y = randomReal( -temp, temp );
            }
            else
            {
                y = randomSignedReal();
                float temp = (float) Math.Sqrt( 1.0 - y * y );
                x = randomReal( -temp, temp );
            }
        }

        public void randomUnitPair(out float x, out float y) {
        // Makes (x,y) a random point with distance 1 from (0,0)
	        float angle = randomReal(0.0f, (float) (2.0 * System.Math.PI));
	        x = (float) Math.Cos(angle);
	        y = (float) Math.Sin(angle);
        }

        public void randomUnitSphereTriple(out float x, out float y, out float z) {
        // Makes (x,y,z) a random point with distance <= 1 from (0,0)
        
        // Rucker's code
	    // x = randomSignedReal();
	    // y = randomSignedReal();
	    // z = randomSignedReal();
	    // while (x*x + y*y + z*z > 1.0)
	    // {
		//     x = randomSignedReal();
		//     y = randomSignedReal();
		//     z = randomSignedReal();
	    // }

        float temp, a;
        x = y = z = 0.0f;  // C# can't figure out that all "out" parameters
            // will be assigned a value, so without this line, we get a 
            // compiler error
        switch( random( 0, 2 ) ) {
            case 0 : x = randomSignedReal( );
                     a = 1.0f - x * x;
                     temp = (float) Math.Sqrt( a );
                     if ( randomBOOL() ) {
                        y = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - y * y );
                        z = randomReal( -temp, temp );
                     }
                     else {
                        z = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - z * z );
                        y = randomReal( -temp, temp );
                     }
                    break;
            case 1 : y = randomSignedReal( );
                     a = 1.0f - y * y;
                     temp = (float) Math.Sqrt( a );
                     if ( randomBOOL() ) {
                        x = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - x * x );
                        z = randomReal( -temp, temp );
                     }
                     else {
                        z = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - z * z );
                        x = randomReal( -temp, temp );
                     }
                    break;
            case 2 : z = randomSignedReal( );
                     a = 1.0f - z * z;
                     temp = (float) Math.Sqrt( a );
                     if ( randomBOOL() ) {
                        x = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - x * x );
                        y = randomReal( -temp, temp );
                     }
                     else {
                        y = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - y * y );
                        x = randomReal( -temp, temp );
                     }
                    break;
            }
        }

        public void randomUnitTriple(out float x, out float y, out float z) {
        // Makes (x,y,z) a random point with distance 1 from (0,0,0)
        
        // Rucker's code 
        // float norm = 0.0; 
	    //    while (norm <= 0.001) //Avoid dividing by something small.
	    //    {
		//        randomUnitSphereTriple(x, y, z);
		//        norm = Math.Sqrt(x*x + y*y + z*z);
	    //    }
	    //    x /= norm;
	    //    y /= norm;
	    //    z /= norm;

         x = y = z = 0.0f;  // C# can't figure out that all "out" parameters
         // will be assigned a value, so without this line, we get a 
         // compiler error
         float temp, a;
         switch( random( 0, 2 ) ) {
            case 0 : x = randomSignedReal( );
                     a = 1.0f - x * x;
                     temp = (float) Math.Sqrt( a );
                     if ( randomBOOL() ) {
                        y = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - y * y );
                        z = randomSign() * temp;
                     }
                     else {
                        z = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - z * z );
                        y = randomSign() * temp;
                     }
                    break;
            case 1 : y = randomSignedReal( );
                     a = 1.0f - y * y;
                     temp = (float) Math.Sqrt( a );
                     if ( randomBOOL() ) {
                        x = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - x * x );
                        z = randomSign() * temp;
                     }
                     else {
                        z = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - z * z );
                        x = randomSign() * temp;
                     }
                    break;
            case 2 : z = randomSignedReal( );
                     a = 1.0f - z * z;
                     temp = (float) Math.Sqrt( a );
                     if ( randomBOOL() ) {
                        x = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - x * x );
                        y = randomSign() * temp;
                     }
                     else {
                        y = randomReal( -temp, temp );
                        temp = (float) Math.Sqrt( a - y * y );
                        x = randomSign() * temp;
                     }
                    break;
            }
        }

        public int randomColor() { // creates a random 4-byte integer for use
                                // in the Color.FromArgb method
                // We can't pass in Int32.MaxValue, because 1 is added to it
                // in the random function, which would create an overflow -- JC
            return random( Int32.MinValue, Int32.MaxValue - 1 );
        }
    }

}