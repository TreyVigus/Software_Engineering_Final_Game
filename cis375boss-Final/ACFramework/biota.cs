using System;

// AC Framework 1.2 changes: removed code for cCritterArmed
//      made copies of critter list in main list-walking functions in case someone would like
//      to call a list-walking function while code is executing in the main list-walking
//      function foreach loop  (this would cause significant errors)
//      Also combined some functions by adding default parameters

namespace ACFramework
{ 
	
	struct cServiceRequest 
	{ 
		public cCritter  _pclient; 
		public string _request; 
		
		public cServiceRequest( cCritter pclient, string request ) 
		{ 
			_pclient = pclient; 
			_request = request; 
		} 
		
	} 
	
	
	class cBiota : LinkedList<cCritter> 
	{ //Statics 
		public static readonly int NOINDEX = -1; // -1, impossible index, For use by cBiota._index().
	//Non-serialized helper members; 
		public cGame _pgame; 
		public LinkedList<cServiceRequest> _servicerequestlist; 
		
		public cBiota( cGame pgame = null )
            : base( delegate( out cCritter c1, cCritter c2 )
                {
                    c1 = c2;
                }
            )
		{ 
            _pgame = pgame;
            _servicerequestlist = new LinkedList<cServiceRequest>(
                delegate( out cServiceRequest s1, cServiceRequest s2 )
                {
                    s1._pclient = s2._pclient;
                    s1._request = s2._request;
                }
                );
		} 

		
        private void _delete_me( )
        {
            if ( ElementAt( ) == _pgame.Player) //You can't delete the player.
	            return; 
            bool killingfocusflag = ( _pgame.pFocus() == ElementAt( ));
            SavePos();
// I thought the following code would be good to have, but it just creates problems -- JC
//            if (ElementAt().IsKindOf("cCritterArmed"))
//            {
//                cCritterArmed ca = (cCritterArmed) ElementAt();
//                foreach (cCritterBullet cb in ca.pbulletarray())
//                    ca.pbulletarray().ElementAt().destruct();
//            }
            RestorePos();
            ElementAt().destruct();
            RestorePos();
            RemoveAt( ); //Never leave a bad pointer in the list.
            if ( killingfocusflag ) 
	            _pgame.Focus = null; 
        }

        /* remove the element from the array, 
			moving all the other array members one index closer to the head.
			Call delete on element, to free the memory.  Invoke with _servicerequest "delete_me". */ 
	
       
        private void _move_to_front( )
        { /* Remove the critter  from the list and move all the others
        one index closer to the front. Then insert the critter at the front.  If the
        _pgame's player is in first place, put it after that, otherwise put it in first
        place.  Idea is to always keep any onscreen player on top. */ 
            if ( Size < 2 ) /* There's nothing to do if you have one or no members. */ 
	            return; 
            cCritter pmovecritter = ElementAt( ); 
            RemoveAt( ); 
            if ( ElementAt( 0 ) == _pgame.Player ) //We know it's safe to do GetAt(0), as size was >= 2 before removing.
	            InsertAt( 1, pmovecritter ); //After the player 
            else 
	            InsertAt( 0, pmovecritter ); //On top.
        }

        /* Move element from its current array
			position, to the head of the array, either in first position or (if there is an onscreen
			player in first position to second position).  We draw the cBiota in the draw last first
			draw first last order, so that visually the first things in the array show up on top
			or in front of the others.	Invoke with _servicerequest "move_to_front". */ 
        

        private void _replicate( )
        {
            if ( ElementAt( ) == _pgame.Player ) //You can't replicate the player.
		        return; 
	        cCritter ptemp = ElementAt( ).copy(); 
	        ptemp.mutate( cCritter.MF_NUDGE ); //So it's not on top of me.
	        Add( ptemp ); /* We overload the Add to also update the _pgame->_pcollider. */ 
		}
		/* Make a mutated copy of the
			element and insert it into the array right after the element.*/ 

        private void _spawn( )
        { 
            cCritter toSpawn = ElementAt();
	        if ( toSpawn == _pgame.Player ) //You can't spawn the player.
		        return; 
	        cVector3 oldposition, oldvelocity; 
	        string pclientclass = toSpawn.RuntimeClass;
            foreach ( cCritter c in this )
            {
                /* Don't spawn onto yourself and don't spawn onto a critter of a different class type.
            You don't spawn to the player for instance becasue if you and player are both cCritterArmed, then
            calling the copy function would change the player's _pbulletclass  */ 
                if ( ElementAt() == toSpawn || ElementAt().RuntimeClass != pclientclass )
                    continue;
                oldposition = ElementAt().Position;
                oldvelocity = ElementAt().Velocity;
        /* I could clone each critter if I were interested in behavior changes.  But this would cause problems
    because I'm not sure the player should be changed, also because there is a connection between
    cCritterArmed and cCritterBullet.  If I ever want to copy behavior I might use a limited spawn that
    only clones among some copatible critters.
	        delete ElementAt(i); //Immediately replace this with a good critter.
	        ElementAt(i) = GetAt(clientindex)->clone();
    Instead of cloning the critter I will copy the critter params and let this call clone the sprite. */ 
		        ElementAt( ).copy( toSpawn ); 
		        ElementAt( ).mutate( cSprite.MF_ALL | cPolygon.MF_ALL ); 
		        ElementAt( ).moveTo( oldposition );
                ElementAt().Velocity = oldvelocity; 
            }
        } 

		/* Change all the other critter's sprites
			into mutations of element's sprite. */ 
	
		private void _zap( ) 
		{ 
			ElementAt( ).randomize( cCritter.MF_VELOCITY | cSprite.MF_ALL | cPolygon.MF_ALL ); 
		} 

		// Do a 100% mutation.

    //LinkedList Overloads 
	
		public override void Add( cCritter pmember ) 
		{ 
            foreach( cCritter c in this )
				_pgame.Collider.smartAdd( pmember, ElementAt( )); 
			/* I want to insert critters in such a way that if a critter uses the
		same texture as an existing critter, they will be stored next to each 
		other.  The reason is that there a time overhead for activating a
		texture, so it's faster to draw all same-textured critters one after
		the other. */ 
			if ( (Size == 0) || //If nothing in the list or...
				!( pmember.Sprite.UsesTexture )) //It's a non-textured critter 
				    base.Add( pmember ); //Put it at the end of the list.
			else //look for a similarly textured critter.
			{ 
				int oldresourceID; 
				int newresourceID = pmember.Sprite.ResourceID; 
				int insertindex = 1; /* Default position to insert at will be 1,
				right after the player. But if you have an offscreen player
				who's not in the list, you better make instertindex be 0.
				We'll check for this now.  It's safe to access ElementAt(0),
				because the if case just above took care of the empty list
				case. */ 
				if (!( ElementAt( 0 ).IsKindOf( "cCritterArmedPlayer"))) 
						//Note that cPlayer inherits from cCritterArmedPlayer, 
						//so don't use cCritterPlayer as the base class here.
					insertindex = 0; //The offscreen player case.
                bool insertplace = false;
                foreach ( cCritter c in this )
                {
                    if ( insertplace ) // we want to insert right after where it is found
                        break;
					oldresourceID = ElementAt( ).Sprite.ResourceID; 
					if ( oldresourceID == newresourceID )
                    {
                        insertindex = -1;
                        insertplace = true;
                    }
                }
                if ( insertindex == -1 )
				    InsertAt( pmember ); 
                else
                    base.InsertAt( insertindex, pmember );
                /* InsertAt puts
                pmember at insertindex, and shifts up by the index i of each
				member currently indexed at i >= insertindex. */ 
			} 
			if ( pmember != null ) 
				pmember.Owner = this; 
		} 

		//Keep private, and overloads to fix _powner.
	
		public override void InsertAt( int index, cCritter pmember ) 
		{ 
			base.InsertAt( index, pmember ); 
			if ( pmember != null ) 
				pmember.Owner = this; 
		} 

		//Keep private, and overload to fix _powner.
		
		public int _index( cCritter pcritter ) 
		{ 
            int i = 0;
            foreach ( cCritter c in this )
            {
                if ( ElementAt() == pcritter )
                    return i;
                i++;
            }
            
			return cBiota.NOINDEX; //Means -1.  Use this to mean not found.
		} 

		/* Find the i index where pcritter appears in
			the list.  Return NOINDEX, or -1, if it's not in the list. These 
			index numbers are fed to the servicerequest functions. */ 
	
		public void purgeNonPlayerCritters() 
		{ //See comment on purgeCritters.  purgeNonPlayerCritters gets rid of all but player.
            foreach ( cCritter c in this )
                if ( ElementAt() != _pgame.Player )
                    ElementAt().delete_me();
			processServiceRequests(); 	
		} 

		/* Does a delete on all the members except 
			_pgame->player(), thus shrinks _pbiota to size 0 or 1. */ 
	
		public void purgeNonPlayerNonWallCritters() 
		{ //See comment on purgeCritters.  purgeNonPlayerCritters gets rid of all but player.
            foreach ( cCritter c in this )
                if ( ElementAt() != _pgame.Player && !ElementAt().IsKindOf( "cCritterWall" ))
                    ElementAt().delete_me();
			processServiceRequests(); 	
		} 

		/* Deletes all except player and walls. */ 
	
		public void purgeCritters( ) 
		{
            foreach (cCritter c in this)
                ElementAt().delete_me(); // don't want to RemoveAll, because cleanup
                                        // calls have to be made for some critters
            processServiceRequests(); 	
		} 

        public void purgeCritters( string pruntimeclass ) 
		{ /*Rather than deleting the pointers and removing from the list, we use the
	servicerequest mechanism, which handles things like _pgame->pFocus(), etc. */ 
            foreach ( cCritter c in this )
                if ( ElementAt().IsKindOf( pruntimeclass ))
                    ElementAt().delete_me();
            processServiceRequests(); 	
		} 

		/* Gets rid of all
			of a desired type of critter, such as cCritterBullet.  If called with no argument,
			gets rid of all critters.  If you want to save the player use purgeNonPlayerCritters.*/ 
	
		public void removeReferencesTo( cCritter pdeadcritter ) 
		{ 
		//Remove references to pdeadcritter in any critter already in the cBiota list.
            foreach (cCritter c in this)
            {
                cCritter pcritter = ElementAt();
                if ( pcritter != null )
                    pcritter.removeReferencesTo( pdeadcritter );
            }

            /* Also remove any references to pdeadcritter in any critter waiting to get into the cBiota 
		list. This fixes a subtle bug that would arise if I fired two bullets with cForceObjectSeek
		at the same target.  If the first bullet killed the target before the second bullet actually
		got added into the biota, then the second bullet would have a bad pointer. */ 

            foreach( cServiceRequest s in _servicerequestlist )
                if ( s._request == "add_me" )
                    _servicerequestlist.ElementAt()._pclient.removeReferencesTo( pdeadcritter );

		} 

		/* Calls pcritter.removeReferencesTo(
			pdeadcritter) for every pcritter in the list. */ 
	//Mutator 
	
		public void setWrapflag( int wrapflag ) 
		{ 
            foreach ( cCritter c in this )
                ElementAt().WrapFlag = wrapflag;
		} 

		//Sets critters wrapflags.
	
		public void setGame( cGame pgame ) { _pgame = pgame; } 
	//Accessor 
		
		public int count( )  // just count critters if no arguments -- JC 
		{ 
            return Size;
		} 

		public int count( string pruntimeclass )   
		{ 
			int found = 0; 
            foreach ( cCritter c in this )
                if ( c.IsKindOf( pruntimeclass ))
                    found++;
			return found; 
		} 

        public int count( string pruntimeclass, bool includesubclasses ) 
		{ 
			int found = 0; 
            foreach ( cCritter c in this )
                if ( (includesubclasses && ElementAt().IsKindOf( pruntimeclass )) ||
                    ( !includesubclasses && ElementAt().RuntimeClass == pruntimeclass ))
                        found++;
			return found; 
		} 

		
			/* This counts critters of the type pruntimeclass.  If includesubclasses is TRUE, it also
			looks for critters that are subclasses of the pruntimeclass; if includesublclasses is FALSE,
			it looks only for critters that have the same class type as pruntimeclass.  The default
			settings are so that count() will count all the critters. */ 
	
		public cCritter player() { return _pgame.Player;} 

		
		public cRealBox3 border() 
		{ 
			cRealBox3 b = new cRealBox3(); 
			b.copy( _pgame.Border ); 
			return b; 
		} 

		
		public cGame pgame() { return _pgame; } 
	//Clist Overloads 
		
		public override cCritter GetAt( int i ) { if ( i == NOINDEX ) return null; else return base.GetAt( i );} 
			//This declaration also has the effect of making the method public.
	//Metric methods.   
		
		public float distance( cCritter pa, cCritter pb ) 
		{ 
			return pa.Position.distanceTo( pb.Position); 
		} 

		
		public cVector3 direction( cCritter pa, cCritter pb ) 
		{ 
			cVector3 dir = pb.Position.sub( pa.Position); 
			dir.normalize(); 
			return dir; 
		} 

		
		public cDistanceAndDirection distanceAndDirection( cCritter pa, cCritter pb ) 
		{ 
			cVector3 dir = pb.Position.sub( pa.Position ); 
			float distance = dir.Magnitude; 
			if ( distance > 0.00001f ) 
				dir.divassign( distance ); 
			else 
				dir = new cVector3( 1.0f, 0.0f ); //default unit vector 
			return new cDistanceAndDirection( distance, dir ); 
		} 

		
	//Distance and touch  methods 
		//For points 
	
		public cCritter pickLowestIndexTouched( cVector3 vclick ) 
		{ /* Draw draws the lower-numbered critters last, so those are visually "on top".)*/ 

            foreach (cCritter c in this)
                if ( ElementAt().touch( vclick ))
                    return ElementAt();

            return null;
        }
            
		public cCritter pickClosestTouched( cVector3 vclick ) 
		{ 
			LinkedList<cCritter> touchlist = touchList( vclick ); 
			if ( touchlist.Size == 0 ) 
				return null; 
			float closestdistance = 1000000000.0f , testdistance; 
			cCritter closest = touchlist.ElementAt(0);
            foreach (cCritter c in touchlist)
            {
                testdistance = touchlist.ElementAt().distanceTo( vclick );
                if ( testdistance < closestdistance )
                {
					closestdistance = testdistance; 
					closest = touchlist.ElementAt(); 
				} 
            }
			return closest; 
        }

		public LinkedList<cCritter> touchList( cVector3 vclick ) 
		{ 
			LinkedList<cCritter> touchlist = new LinkedList<cCritter>(
                delegate( out cCritter c1, cCritter c2 )
                {
                    c1 = c2;
                }
                ); 

            foreach( cCritter c in this )
                if ( ElementAt().touch( vclick ) )
                    touchlist.Add( ElementAt() );
            return touchlist;
        }

		
		//For Critters 
	
		public LinkedList<cCritter> touchList( cCritter pcrittercenter ) 
		{ 
			LinkedList<cCritter> touchlist = new LinkedList<cCritter>(
                delegate( out cCritter c1, cCritter c2 )
                {
                    c1 = c2;
                }
                ); 

            foreach ( cCritter c in this )
                if ( pcrittercenter.touch( ElementAt() ))
                    touchlist.Add( ElementAt() );
            return touchlist;
		} 

		/* Return critters touching pcenter. */ 
		public cCritter pickClosestCritter( cCritter pcrittercenter )
		{ 
			cCritter pclose = null; 
			cCritter ptest; 
			float closestdistance = 1000000000.0f ; 
			float testdistance; 
            foreach ( cCritter c in this )
            {
                ptest = ElementAt();
                if ( ptest == pcrittercenter )
                    continue;
				testdistance = pcrittercenter.distanceTo( ptest ); 
				if ( testdistance < closestdistance ) 
				{ 
					closestdistance = testdistance; 
					pclose = ptest; 
				} 
            } 
            
			return pclose; 
		} 
	
		public cCritter pickClosestCritter( cCritter pcrittercenter, string pruntimeclass )
		{ 
			cCritter pclose = null; 
			cCritter ptest; 
			float closestdistance = 1000000000.0f ; 
			float testdistance; 
            foreach ( cCritter c in this )
            {
                ptest = ElementAt();
                if ( ptest == pcrittercenter )
                    continue;
				if (!ptest.IsKindOf( pruntimeclass )) 
                    continue;
				testdistance = pcrittercenter.distanceTo( ptest ); 
				if ( testdistance < closestdistance ) 
				{ 
					closestdistance = testdistance; 
					pclose = ptest; 
				} 
            } 
            
			return pclose; 
		} 

        public cCritter pickClosestCritter( cCritter pcrittercenter, 
            string pruntimeclass, bool includesubclasses ) 
		{ 
			cCritter pclose = null; 
			cCritter ptest; 
			float closestdistance = 1000000000.0f ; 
			float testdistance; 
            foreach ( cCritter c in this )
            {
                ptest = ElementAt();
                if ( ptest == pcrittercenter )
                    continue;
				if (!( 
					( includesubclasses && ptest.IsKindOf( pruntimeclass )) || 
					( !includesubclasses && ptest.RuntimeClass == pruntimeclass ) 
				)) 
                    continue;
				testdistance = pcrittercenter.distanceTo( ptest ); 
				if ( testdistance < closestdistance ) 
				{ 
					closestdistance = testdistance; 
					pclose = ptest; 
				} 
            } 
            
			return pclose; 
		} 

		
			/* This looks for the closest critter to the pcenter critter. It searches for critters of
			the type pruntimeclass.  If includesubclasses is TRUE, it also looks for critters that are 
			subclasses of the pruntimeclass; if includesublclasses if FALSE, it looks only for critters
			that have the same class type as pruntimeclass.  The default settings are so that
			pickClosestCritter(pcenter) will look for the closest critter of any kind. */ 

        //For sightlines 
	
		public cCritter pickTopTouched( cLine sightline ) 
		{ /*Same code as pickClosestTouched but with a different sorting critereon;
		we use lineCoord(sightline) instead of distanceTo(sightline). */ 
			LinkedList<cCritter> touchlist = touchList( sightline ); 
			if ( touchlist.Size == 0 ) 
				return null; 
			float closestdistance = 1000000000.0f , testdistance; 
			cCritter closest = touchlist.ElementAt(0); //Everyone in the list is acceptable, so start with 0 is ok.

            foreach (cCritter c in touchlist)
            {
                testdistance = sightline.lineCoord( touchlist.ElementAt().Position);
				if ( testdistance < closestdistance ) 
				{ 
					closestdistance = testdistance; 
					closest = touchlist.ElementAt(); 
				}
			} 
            
			return closest; 
		} 

        public cCritter pickTopTouched( cLine sightline, cCritter pcritterignore ) 
		{ /*Same code as pickClosestTouched but with a different sorting critereon;
		we use lineCoord(sightline) instead of distanceTo(sightline). */ 
			LinkedList<cCritter> touchlist = touchList( sightline, pcritterignore ); 
			if ( touchlist.Size == 0 ) 
				return null; 
			float closestdistance = 1000000000.0f , testdistance; 
			cCritter closest = touchlist.ElementAt(0); //Everyone in the list is acceptable, so start with 0 is ok.

            foreach (cCritter c in touchlist)
            {
                testdistance = sightline.lineCoord( touchlist.ElementAt().Position);
				if ( testdistance < closestdistance ) 
				{ 
					closestdistance = testdistance; 
					closest = touchlist.ElementAt(); 
				}
			} 
            
			return closest; 
		} 

		
		public cCritter pickClosestTouched( cLine sightline ) 
		{ 
			LinkedList<cCritter> touchlist = touchList( sightline ); 
			if ( touchlist.Size == 0 ) 
				return null; 
			float closestdistance = 1000000000.0f , testdistance; 
			cCritter closest = touchlist.ElementAt(0);
 
            foreach ( cCritter c in touchlist )
            {
                testdistance = touchlist.ElementAt().distanceTo( sightline );
				if ( testdistance < closestdistance ) 
				{ 
					closestdistance = testdistance; 
					closest = touchlist.ElementAt(); 
				} 
			} 
			return closest; 
		} 

        public cCritter pickClosestTouched( cLine sightline, cCritter pcritterignore ) 
		{ 
			LinkedList<cCritter> touchlist = touchList( sightline, pcritterignore ); 
			if ( touchlist.Size == 0 ) 
				return null; 
			float closestdistance = 1000000000.0f , testdistance; 
			cCritter closest = touchlist.ElementAt(0);
 
            foreach ( cCritter c in touchlist )
            {
                testdistance = touchlist.ElementAt().distanceTo( sightline );
				if ( testdistance < closestdistance ) 
				{ 
					closestdistance = testdistance; 
					closest = touchlist.ElementAt(); 
				} 
			} 
			return closest; 
		} 

		public cCritter pickClosestAhead( cLine sightline, cCritter pcrittercenter ) 
		{ 
			float closestdistance = 1000000000.0f , testdistance, angle; 
		    cCritter closest = null;    	

            foreach ( cCritter c in this )
            {
                cCritter ppossible = ElementAt();
				angle = sightline._tangent.angleBetween( 
					ppossible.Position.sub( pcrittercenter.Position)); 
				if ( ppossible == pcrittercenter || Math.Abs( angle ) > (float) Math.PI / 8.0f ) //Only go for those ahead.
					continue; //Skip this critter.
				testdistance = ppossible.distanceTo( sightline ); //Pick closest to sightline 
			//	testdistance = ppossible->distanceTo(pcrittercenter); //OR Pick closest to pcrittercenter 
				if ( testdistance < closestdistance ) 
				{ 
					closestdistance = testdistance; 
					closest = ppossible; 
				} 
			} 

            return closest;
		} 
		
		public cCritter pickClosestAhead( cLine sightline, cCritter pcrittercenter, 
            float visionangle ) 
		{ 
			float closestdistance = 1000000000.0f , testdistance, angle; 
		    cCritter closest = null;    	

            foreach ( cCritter c in this )
            {
                cCritter ppossible = ElementAt();
				angle = sightline._tangent.angleBetween( 
					ppossible.Position.sub( pcrittercenter.Position)); 
				if ( ppossible == pcrittercenter || Math.Abs( angle ) > visionangle / 2.0f ) //Only go for those ahead.
					continue; //Skip this critter.
				testdistance = ppossible.distanceTo( sightline ); //Pick closest to sightline 
			//	testdistance = ppossible->distanceTo(pcrittercenter); //OR Pick closest to pcrittercenter 
				if ( testdistance < closestdistance ) 
				{ 
					closestdistance = testdistance; 
					closest = ppossible; 
				} 
			} 

            return closest;
		} 

        public cCritter pickClosestAhead( cLine sightline, cCritter pcrittercenter, 
            float visionangle, string pruntimeclass ) 
		{ 
			float closestdistance = 1000000000.0f , testdistance, angle; 
		    cCritter closest = null;    	

            foreach ( cCritter c in this )
            {
                cCritter ppossible = ElementAt();
				angle = sightline._tangent.angleBetween( 
					ppossible.Position.sub( pcrittercenter.Position)); 
				if (!ppossible.IsKindOf( pruntimeclass )) 
					continue; //Skip this critter.
				if ( ppossible == pcrittercenter || Math.Abs( angle ) > visionangle / 2.0f ) //Only go for those ahead.
					continue; //Skip this critter.
				testdistance = ppossible.distanceTo( sightline ); //Pick closest to sightline 
			//	testdistance = ppossible->distanceTo(pcrittercenter); //OR Pick closest to pcrittercenter 
				if ( testdistance < closestdistance ) 
				{ 
					closestdistance = testdistance; 
					closest = ppossible; 
				} 
			} 

            return closest;
		} 

        public cCritter pickClosestAhead( cLine sightline, cCritter pcrittercenter, 
            float visionangle, string pruntimeclass, bool includesubclasses ) 
		{ 
			float closestdistance = 1000000000.0f , testdistance, angle; 
		    cCritter closest = null;    	

            foreach ( cCritter c in this )
            {
                cCritter ppossible = ElementAt();
				angle = sightline._tangent.angleBetween( 
					ppossible.Position.sub( pcrittercenter.Position)); 
				if (!( //Only go for the right type of critter.
					( includesubclasses && ppossible.IsKindOf( pruntimeclass )) || 
					( !includesubclasses && ppossible.RuntimeClass == pruntimeclass ) 
				)) 
					continue; //Skip this critter.
				if ( ppossible == pcrittercenter || Math.Abs( angle ) > visionangle / 2.0f ) //Only go for those ahead.
					continue; //Skip this critter.
				testdistance = ppossible.distanceTo( sightline ); //Pick closest to sightline 
			//	testdistance = ppossible->distanceTo(pcrittercenter); //OR Pick closest to pcrittercenter 
				if ( testdistance < closestdistance ) 
				{ 
					closestdistance = testdistance; 
					closest = ppossible; 
				} 
			} 

            return closest;
		} 

		
			/* looks along the sightline from pcrittercenter and finds candidate critters
			of the desired pruntimeclass and which are within the visionangle field around
			sighline. Of these candidates, the critter that's actually closest to the
			sightline is chosen.*/ 
	
        public LinkedList<cCritter> touchList( cLine sightline, cCritter pcritterignore = null ) 
		{ 
			LinkedList<cCritter> touchlist = new LinkedList<cCritter>(
                delegate( out cCritter c1, cCritter c2 )
                {
                    c1 = c2;
                }
                ); 

            foreach ( cCritter c in this )
                if ( ElementAt().touch( sightline ) && ElementAt() != pcritterignore )
                    touchlist.Add( ElementAt());

            return touchlist; 
		} 

		
	//Service request methods 
	
		public void addServiceRequest( cServiceRequest servicerequest ) 
		{ 
			_servicerequestlist.Add( servicerequest ); 
		} 

		
		public bool processServiceRequests() 
		{ 
			bool success = true; 
			int clientindex; 
		/* Do two passes, first do all the deletes, then do the adds and other stuff.  The reason to get the 
	deletes out of the way first is that when I add a critter to a cBiota I also plan to add all
	relevant collision pairs to the the cCollider associtaed with the cBiota's owner cGame.  
	And I wouldn't want to be setting up a collision for something I'm about to delete. */ 
        //The delete loop

            LinkedList<cServiceRequest> _servicecopy = new LinkedList<cServiceRequest>(
                delegate(out cServiceRequest s1, cServiceRequest s2)
                {
                    s1._pclient = s2._pclient;
                    s1._request = s2._request;
                }
                );

            _servicecopy.Copy(_servicerequestlist);

            foreach ( cServiceRequest s in _servicecopy )
            {
                if ( s._request == "delete_me" )
                {
                    clientindex = _index( s._pclient ); // Note:  This sets the current
                            // position of the LinkedList at the client -- JC
                    if ( clientindex == cBiota.NOINDEX )
                        success = false;
                    else   // delete
                        _delete_me( );
                }
            }

        //The loop for add and other requests 
		    foreach ( cServiceRequest s in _servicerequestlist )
            {
                clientindex = _index( s._pclient );
                if ( s._request == "add_me" )
                {
                    if ( clientindex != cBiota.NOINDEX )  // You already added this guy.
                        success = false;
                    else
                        Add( s._pclient );
                    continue;
                }
                if ( clientindex == cBiota.NOINDEX )
                {
                    success = false;
                    continue;
                }
                else if ( s._request == "delete_me" )
                    continue; // do nothing -- already handled
				else if ( s._request == "move_to_front" )
                    _move_to_front( );
                else if ( s._request == "replicate" ) 
                    _replicate( );
                else if ( s._request == "spawn" )
                    _spawn( );
				else if ( s._request == "zap" )
                    _zap( );
                else
                    success = false;
            } 
            
			_servicerequestlist.RemoveAll(); //You did them all, so empty it out.
			return success; /* FALSE means you couldn't find one of the _pclient 
			requesters.  Either someone put a bad value into the list or one
			of your members posted a request for a "delete_me" or a "convert..." 
			followed by another request, which can't be honored as the caller
			pointer's no longer there. */ 
		} 

		
	//list-walking methods.
		public virtual void draw( cGraphics pgraphics, int drawflags = 0 ) 
		{ 
			cCritter pcritter; 

            /* The critters should be drawn from the end of the list to the beginning of
             * the list.  I did not make the LinkedList doubly-linked, so that we don't
             * use an extra pointer's worth of space for each member.  The LinkedList has
             * an indexer, which would allows us to uses indexes to go from the back of
             * the list to the front, but this would end up being theta-n-squared.  So
             * I ended up using making a list in reverse order first.  Thus, this function 
             * is theta-n instead of theta-n-squared, a significant savings in time.  -- JC */

            LinkedList<cCritter> reverseList = new LinkedList<cCritter> (
                delegate( out cCritter c1, cCritter c2 )
                {
                    c1 = c2;
                });

            foreach ( cCritter c in this )
                reverseList.InsertAt( 0, ElementAt() );

            foreach ( cCritter c in reverseList ) /* Draw the lower-numbered critters last, 
			        so those are visually "on top".)*/ 
            {
				pcritter = reverseList.ElementAt( );
                if ( pcritter == null ) 
					return; 
				pcritter.draw( pgraphics, drawflags ); 
				if ( pcritter == _pgame.pFocus()) 
					pcritter.drawHighlight( pgraphics, cSprite.HIGHLIGHTRATIO ); 
			} 
		} 

		
			//cBiota.draw is normally called in cGraphics.graphicsOnDraw.
			//All the following list walking methods are called in cGame.step.
	
		public void move( float dt ) 
		{
            // necessary to make a copy in case someone wants to call a list-walking function
            // from other code executed in foreach
            LinkedList<cCritter> clist = new LinkedList<cCritter>(
                delegate(out cCritter c1, cCritter c2)
                {
                    c1 = c2;
                }
            );
            clist.Copy(this); 
            foreach ( cCritter c in clist )
                if ( clist.ElementAt() != pgame().pFocus() || clist.ElementAt() == pgame().Player)
                    clist.ElementAt().move( dt );
        }

		//Calls each critter's move(dt).
	
		public void update( ACView pactiveview, float dt ) 
		{
            // necessary to make a copy in case someone wants to call a list-walking function
            // from other code executed in foreach
            LinkedList<cCritter> clist = new LinkedList<cCritter>(
                delegate(out cCritter c1, cCritter c2)
                {
                    c1 = c2;
                }
            );
            clist.Copy(this);
            foreach (cCritter c in clist)
                clist.ElementAt().update( pactiveview, dt );
		} 

		/* Calls all critters' updates. The pview can be passed to sniff. */ 
	
		public void animate( float dt ) 
		{
            // necessary to make a copy in case someone wants to call a list-walking function
            // from other code executed in foreach
            LinkedList<cCritter> clist = new LinkedList<cCritter>(
                delegate(out cCritter c1, cCritter c2)
                {
                    c1 = c2;
                }
            );
            clist.Copy(this); 
            foreach ( cCritter c in clist )
                clist.ElementAt().animate( dt );
		} 

		//Calls each critter's _psprite->animate(dt).
	
		public void feellistener( float dt ) 
		{
            // necessary to make a copy in case someone wants to call a list-walking function
            // from other code executed in foreach
            LinkedList<cCritter> clist = new LinkedList<cCritter>(
                delegate(out cCritter c1, cCritter c2)
                {
                    c1 = c2;
                }
            );
            clist.Copy(this); 
 
            foreach ( cCritter c in clist )
                clist.ElementAt().feellistener( dt );
		} 

		/* Calls each critter's listen (for controller input like key and mouse).
			The cListenerCursor needs the dt to set the _velocity. */

        public virtual bool NewGeometryFlag
        {
            set
            {
                // necessary to make a copy in case someone wants to call a list-walking function
                // from other code executed in foreach
                LinkedList<cCritter> clist = new LinkedList<cCritter>(
                    delegate(out cCritter c1, cCritter c2)
                    {
                        c1 = c2;
                    }
                );
                clist.Copy(this);

                foreach (cCritter c in clist)
                    clist.ElementAt().Sprite.NewGeometryFlag = value;
            }
        }
        
		/* Use to set each pcritter->psprite()->newgeometryflag
			to FALSE, we use this after we've drawn the critters in all the views and the draws
			have possibly made new display list IDs for the geometry of any sprite that had its
			newgeomtryflag TRUE. Can also use to set to TRUE when toggling between solid
			and transparent view. */ 
	} 
}