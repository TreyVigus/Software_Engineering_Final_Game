using System;

namespace ACFramework
{ 
	
	/* The current implementaiotn is kludgy, it woudl be better to do something closer to the
		cCollider code. */ 
	
	/* The cDistanceAndDirection utility class is needed for the values to be looked up
	in the cMetricCritter.  This calss saves information about the distance and unit
	vector direction between pairs of critters. */ 
	class cDistanceAndDirection 
	{ 
		public float _distance; 
		public cVector3 _direction; 
		
		public cDistanceAndDirection()
        {
            _distance = 0.0f;
            _direction = new cVector3(); /* (0.0, 0.0, 0.0) */
        } 
		
		public cDistanceAndDirection( float dist, cVector3 dir )
        { 
            _distance = dist;
            _direction = new cVector3();
            _direction.copy( dir ); 
        }

        public void copy(cDistanceAndDirection dd)
        {
            _distance = dd._distance;
            _direction.copy(dd._direction);
        }
	} 
}