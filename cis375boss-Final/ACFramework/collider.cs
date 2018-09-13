// For AC Framework 1.2, functions were combined using default parameters -- JC

using System;
using System.Windows.Forms;

namespace ACFramework
{ 
	
	class cColliderPair 
	{ 
		public cCritter _pcrittercaller; //Make public so we don't bother with accessor.
		public cCritter _pcritterarg; 
		
        public cColliderPair( cCritter pcrittercaller = null, cCritter pcritterarg = null ) 
		{ 
			_pcrittercaller = pcrittercaller; 
			_pcritterarg = pcritterarg; 
		} /* Note
			that with the defaults this acts as a no-arg constructor as well.  We
			let cCollider::smartAdd handle  the heavy lifting of figuring out which 
			order to build our pairs in, given two arbitrary critter pointers. */ 
		
		public bool involves( cCritter pcrittertest ) 
			{ return pcrittertest == _pcrittercaller || pcrittertest == _pcritterarg; } 
		
		public bool collideThePair( ) 
		{ 
			bool collided; 
		
			collided = _pcrittercaller.collide( _pcritterarg ); 
		
			return collided; 
		} 

	} 
	
	class cColliderLevel : LinkedList<cColliderPair> 
	{ 
		
		public cColliderLevel()
          :  base( delegate( out cColliderPair c1, cColliderPair c2 )
            {
                c1 = c2;
            }
            )
        {
        } 
		
		public void removeReferencesTo( cCritter pdeadcritter ) 
		{
            if ( Size == 0 )
                return;
            
            cColliderPair cpair;
            First( out cpair );

            do
            {
                if (cpair.involves(pdeadcritter))
                    RemoveNext();
            } while ( GetNext( out cpair ) );

        }
		
		public void iterateCollide( bool checkpriority ) 
		{
            foreach (cColliderPair cpair in this)
                cpair.collideThePair( );
		} 
	} 
	
	class cCollider 
	{ 
		public static readonly int DONTCOLLIDE = 2; 
		public static readonly int COLLIDEEITHERWAY = 0; 
		public static readonly int COLLIDEASCALLER = 1; 
		public static readonly int COLLIDEASARG = -1; 
		/* The CP_??? are default cCritter _collisionpriority values, in increasing size for
			increasingly high priority, where in a pair of critters, the higher priority
			critter is the caller of the collide method, and the lower priority critter
			is the argument to the collide call. I make these Real so I can always squeeze
			value in between, and I make them non-const in case a game wants to change their
			values. At present the values are, in order, 100, 200, 300, 400, 500, and a million. */ 
		public static readonly float CP_CRITTER = 100.0f; 
		public static readonly float CP_PLAYER = 200.0f; 
		public static readonly float CP_SILVERBULLET = 300.0f; 
		public static readonly float CP_BULLET = 400.0f; 
		public static readonly float CP_WALL = 500.0f; 
		public static readonly float CP_MAXIMUM = 1000000.0f; 
		public static readonly float CP_MINIMUM = 0; 
		protected cColliderLevel _wallpairs; 
		protected cColliderLevel _nonwallpairs; 
		
		public cCollider()
        {
            _wallpairs = new cColliderLevel();
            _nonwallpairs = new cColliderLevel();
        } 
		
		public void removeAll() 
		{ 
			_wallpairs.RemoveAll(); 
			_nonwallpairs.RemoveAll(); 
		} 

		
		public virtual void smartAdd( cCritter pcritter, cCritter pcritterother ) 
		{ 
			cColliderPair pnewpair = null; 
			int collideswith = pcritter.collidesWith( pcritterother ); 
			int othercollideswith = pcritterother.collidesWith( pcritter ); 
			if ( collideswith == DONTCOLLIDE || othercollideswith == DONTCOLLIDE ) 
				return ; //Don't collide if either one is unwilling, even if the other was willing. */ 
			if ( collideswith == COLLIDEASCALLER || collideswith == COLLIDEEITHERWAY ) 
				pnewpair = new cColliderPair( pcritter, pcritterother ); 
			else //(collideswith == cCollider::COLLIDEASARG) 
				pnewpair = new cColliderPair( pcritterother, pcritter ); 
			if (( pnewpair._pcrittercaller.CollidePriority == CP_WALL ) || 
				( pnewpair._pcritterarg.CollidePriority == CP_WALL )) 
				_wallpairs.Add( pnewpair ); 
			else 
				_nonwallpairs.Add( pnewpair ); 
		
			//ASSERT(collideswith == -othercollideswith); 
				/* We chose the collision type codes to make this ASSERT
			likely at this point, but it might not always be true.  Only comment it in
			for testing. Do note that we bail before we hit it if either type is DONTCOLLIDE. */ 
		} 

		/* The smartAdd will only add a
			cCollisionPair if ther critters want to collide, and if it adds one, it'll put the 
			critter into the cCollisionPair in the correct caller/arg order. You might someday want
			to overload this. */ 
	
		public void removeReferencesTo( cCritter pdeadcritter ) 
		{ 
			_wallpairs.removeReferencesTo( pdeadcritter ); 
			_nonwallpairs.removeReferencesTo( pdeadcritter ); 
		} 

		
		public void build( cBiota pbiota ) 
		{ 
			removeAll(); //Clear it out.
			for ( int i = 0; i < pbiota.Size; i++) 
				for ( int j = i + 1; j < pbiota.Size; j++) 
					smartAdd( pbiota.GetAt( i ), pbiota.GetAt( j )); 
		} 

		
		public void iterateCollide() 
		{ 
			_wallpairs.iterateCollide( false ); 
			_nonwallpairs.iterateCollide( true ); 
		} 
	} 
}