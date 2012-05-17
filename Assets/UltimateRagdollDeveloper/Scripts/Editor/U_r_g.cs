using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;

/// <summary>
/// 2011-10-23
/// ULTIMATE RAGDOLL GENERATOR V1.8
/// © THE ARC GAMES STUDIO 2011
/// DESIGNED WITH UNITY 3.4.1f5
/// 
/// REFER TO THE README FILE FOR USAGE SPECIFICATIONS
/// </summary>
public class U_r_g : EditorWindow {
	[MenuItem ("GameObject/Create Other/Ultimate ragdoll dev...")]
    static void Init () {
		EditorWindow.GetWindow (typeof (U_r_g),true,"Ultimate Ragdoll Generator Developer");
    }
	
	private static Transform varsource;
	private static Transform varsourceconnector;
	//option variables. refer to the readme for explanations
	private static float vartotalmass = 75;
	private static PhysicMaterial varmaterial = null;
	private static bool varurgentities = true;
	private static float varfakelimbdistance = 0.5f;
	private static bool varcreatekinematic = false;
	private static float varrigidity = 1;
	private static float varmaxchildcount = 4;
	private static int varexplorationsize = 0;
	private static int varcurrentexplorationsize = 0;
	private static bool varexplorehead = true;
	private static bool varexplorespine = true;
	private static bool varexplorearmleft = true;
	private static bool varexplorearmright = true;
	private static bool varexplorelegleft = true;
	private static bool varexplorelegright = true;
	private static bool varpreservelimbslinktoroot = true;
	private static bool varfitshoulders = true;
	private static float varabsorbtolerance = 0.25f;
	private static float varhightensionvalue = 66;
	private static float varmidtensionvalue = 33;
	private static float varlowtensionvalue = 16;
	private static float varmintensionvalue = 8;
	private static float vardampeningstrenght = 4.5f;
	private static float varspringstrenght = 9f;
	private static float vardrag = 0f;
	private static float varangulardrag = 0.05f;
	private static int varmatrixsize = 17;
	private static float varheightwindow = 0.02f;
	private static float varwidthwindow = 0.05f;
	private static bool varautomaticallysearchskinnedmeshrenderer = true;
	private static bool varclearphysics = true;
	private static bool varverbose = false;
	//stores the cardinal matrix of each bone in respect to its parent
	private static Transform[][] varbonesmatrix = new Transform[varmatrixsize][];
	//global bones matrix navigation indexes
	private static int varcoordx, varcoordy; //used by metstorelimbinbonesmatrix, metstorecardinaldirectioninbonesmatrix
	//global shoulder and leg y coordinate in the bones matrix
	private static int varshouldersy = -1;
	private static int varlegsy = -1;
	//global variables holding the star search results that define the limbs bones
	private static Transform varheadstart, vararmlstart, vararmrstart, varleglstart, varlegrstart;
	private static Transform varheadend, vararmlend, vararmrend, varleglend, varlegrend;
	//very important global storage for the values that are calculated by the star finder recursive function
	private static float vararmlcurrentdistance, vararmrcurrentdistance;
	//global indexes for the topmost and lowest bone y indexes in the bones matrix
	private static int vartopboney = varmatrixsize;
	private static int varbottomboney = 0;
	//auxilliary gui values
	private bool vardisplayoptions = false;
	private bool varlimbdisplayoptions = false;
	private Vector2 varmainscroll;
	
	//auxilliary exploration variables for the arms
	private static Transform vartemparmlstart = null;
	private static Transform vartemparmrstart = null;

	//auxilliary fake limb constants
	private const string confakelimbsuffixleft = "_left";
	private const string confakelimbsuffixright = "_right";

	
	/// <summary>
	/// helper function to display messages in behalf of Debug.Log
	/// </summary>
	/// <param name="varpmessage">
	/// The message to be sent to the console
	/// </param>
	/// <param name="varplevel">
	/// The level of the message
	/// </param>
	private static void metprint(string varpmessage, int varplevel) {
		switch (varplevel) {
		case 0:
			if (varverbose==true)
				Debug.Log(varpmessage);
			break;
		case 1:
			Debug.LogWarning(varpmessage);
			break;
		case 2:
			Debug.LogError(varpmessage);
			break;
		case 4:
			if (varverbose==true)
				Debug.LogWarning(varpmessage);
			break;
		default:
			Debug.Log(varpmessage);
			break;
		}
	}
	
	//helper variable that defines a cardinal direction as a direction wheel
	private enum enumcardinaldirections {
		dirnone = 0,
		dirup = 1,
		dirupright = 2,
		dirright = 3,
		dirdownright = 4,
		dirdown = 5,
		dirdownleft = 6,
		dirleft = 7,
		dirupleft = 8
	}
	
	/// <summary>
	/// Creates a matrix like normalized Vector2 from two positions, which tells where is the target positioned in respect to the source
	/// in the form of x and y (for example if target is to the right of the source, the result will be a vector2(1,0)
	/// </summary>
	/// <param name="varpsource">
	/// Source transform. The reference position.
	/// </param>
	/// <param name="varptarget">
	/// Target transform. The one which will be located by the function's return
	/// </param>
	/// <returns>
	/// Normalized vector which locates the target in respect to the source.
	/// </returns>
	private static Vector2 metcardinaldistance(Transform varpsource, Transform varptarget) {
		Vector2 varreturnvalue = new Vector2();
		Vector3 vardistance = varptarget.position - varpsource.position;
		if (vardistance.x > varwidthwindow)
			varreturnvalue.x = 1;
		if (vardistance.x < -varwidthwindow)
			varreturnvalue.x = -1;
		if (Mathf.Abs(vardistance.x) < varwidthwindow)
			varreturnvalue.x = 0;
		if (vardistance.y > varheightwindow)
			varreturnvalue.y = 1;
		if (vardistance.y < -varheightwindow)
			varreturnvalue.y = -1;
		if (Mathf.Abs(vardistance.y) < varheightwindow)
			varreturnvalue.y = 0;
		return varreturnvalue;
	}
	
	/// <summary>
	/// This function will return a normalized Vector2 expressing the cardinal distance of a cardinal direction
	/// will be used to calculate displacement between two bones
	/// </summary>
	/// <param name="varpdirection">
	/// the source cardinal direction (left, right, top, etc.)
	/// </param>
	/// <returns>
	/// The displacement needed to move in varpdirection direction
	/// </returns>
	private static Vector2 metcardinaldistancefromdirection(enumcardinaldirections varpdirection) {
		Vector2 varreturnvalue = new Vector2();
		switch (varpdirection) {
			case enumcardinaldirections.dirup:
				varreturnvalue.x = 0;
				varreturnvalue.y = 1;
				break;
			case enumcardinaldirections.dirupright:
				varreturnvalue.x = 1;
				varreturnvalue.y = 1;
				break;
			case enumcardinaldirections.dirright:
				varreturnvalue.x = 1;
				varreturnvalue.y = 0;
				break;
			case enumcardinaldirections.dirdownright:
				varreturnvalue.x = 1;
				varreturnvalue.y = -1;
				break;
			case enumcardinaldirections.dirdown:
				varreturnvalue.x = 0;
				varreturnvalue.y = -1;
				break;
			case enumcardinaldirections.dirdownleft:
				varreturnvalue.x = -1;
				varreturnvalue.y = -1;
				break;
			case enumcardinaldirections.dirleft:
				varreturnvalue.x = -1;
				varreturnvalue.y = 0;
				break;
			case enumcardinaldirections.dirupleft:
				varreturnvalue.x = -1;
				varreturnvalue.y = 1;
				break;
			case enumcardinaldirections.dirnone:
				varreturnvalue = Vector2.zero;
				break;
			default:
				varreturnvalue = Vector2.zero;
				break;
		}
		return varreturnvalue;
	}
	
	/// <summary>
	/// Calculates and returns the 'cardinal direction' of a Vector2
	/// Will be used to estimate one position in respect to another
	/// </summary>
	/// <param name="varpcardinaldistance">
	/// The source cardinal vector. Usually the result of metcardinaldistance
	/// </param>
	/// <returns>
	/// The cardinal direction of the original target in respect to the source
	/// </returns>
	private static enumcardinaldirections metcardinaldirection(Vector2 varpcardinaldistance) {
		int varreturnvalue = 0;
		
		//no direction
		if (varpcardinaldistance.x == 0 && varpcardinaldistance.y == 0) {
			varreturnvalue = 0;
		}
		else {
			if (varpcardinaldistance.y != 0) {
				if (varpcardinaldistance.x != 0) {
					if (varpcardinaldistance.y > 0) {
						if (varpcardinaldistance.x > 0)
							varreturnvalue = 2; //up right
						if (varpcardinaldistance.x < 0)
							varreturnvalue = 8; //up left
					}
					if (varpcardinaldistance.y < 0)  {
						if (varpcardinaldistance.x > 0)
							varreturnvalue = 4; //down right
						if (varpcardinaldistance.x < 0)
							varreturnvalue = 6; //down left
					}	
				}
				else {
					if (varpcardinaldistance.y > 0)
						varreturnvalue = 1; //up
					if (varpcardinaldistance.y < 0)
						varreturnvalue = 5;  //down
				}
			}
			else {
				if (varpcardinaldistance.x > 0)
					varreturnvalue = 3; //right
				if (varpcardinaldistance.x < 0)
					varreturnvalue = 7; //left
			}
		}
		//convert the int value back to enumcardinaldirections and return it
		return (enumcardinaldirections)varreturnvalue;
	}
	
	/// <summary>
	/// This recursive function will return the first sub bone of varproot that is found along varpdirection, to determine if the sub bone
	/// pertains to a certain limb or not. The function will deal with cohincident bones and multi branched sub bones.
	/// </summary>
	/// <param name="varpdirection">
	/// The cardinal direction of the search (top, left, right, etc.)
	/// </param>
	/// <param name="varproot">
	/// The source bone which will be used as reference for the direction of search
	/// </param>
	/// <param name="varporigin">
	/// Recursion helper. Will be equal to varproot in the method call.
	/// </param>
	/// <returns>
	/// The reference to the most suitable bone in respect to varproot, along varpdirection
	/// </returns>
	private static Transform metfindvalidcardinalcandidate(enumcardinaldirections varpdirection, Transform varproot, Transform varporigin) {
		Transform varreturnvalue = varproot;
		Transform varcurrentcandidate = null;
		Transform varcurrentcandidatehasvalidsubbones = null;
		Transform varlastcandidate = null;
		Transform varlastcandidatehasvalidsubbones = null;
		enumcardinaldirections vardirection;
		
		//iterate only if the starting bone is not null
		if (varproot != null) {
			foreach (Transform varsubbone in varproot) {
				//store the last valid candidate for successive testing
				varlastcandidate = varcurrentcandidate;
				varlastcandidatehasvalidsubbones = varcurrentcandidatehasvalidsubbones;
				//calculate the cardinal direction (up, left, upleft, right, etc.) between the origin and the current sub bone
				vardirection = metcardinaldirection(metcardinaldistance(varporigin,varsubbone));
				//if the direction is not the desired one, we issue a new recursive call to be sure that there's no sub bone
				//which is actually in the required direction (rules out cohincident bone heads and same direction sub bones)
				if (vardirection != varpdirection) {			
					varcurrentcandidate = metfindvalidcardinalcandidate(varpdirection, varsubbone, varporigin);
				}
				//this sub bone's direction is the one we're looking for so we store it as the current valid candidate
				if (vardirection == varpdirection) {
					varcurrentcandidate = varsubbone;
					varcurrentcandidatehasvalidsubbones = metfindvalidcardinalcandidate(varpdirection, varcurrentcandidate, varporigin);
				}
				//return value decision check if we have a current candidate
				if (varcurrentcandidate != null) {
					//we have a current candidate check if we already have a return value candidate
					if (varlastcandidate != null) {
						//we have both a return value candidate and a new value candidate. check if these candidates have
						//valid origin direction sub bones
						//the last candidate has no valid direction sub bones, but the new candidate does
						if (varlastcandidatehasvalidsubbones == null && varcurrentcandidatehasvalidsubbones != null) {
							//the return value candidate is overwritten with the current candidate
							varreturnvalue = varcurrentcandidate;
						}
						//both candidates have sub bones in the required direction.
						if (varlastcandidatehasvalidsubbones != null && varcurrentcandidatehasvalidsubbones != null) {
							 //We rule out the fittest candidate by counting both candidates sub bones
							if (metcountsubbones(varlastcandidate) < metcountsubbones(varcurrentcandidate)) {
								varreturnvalue = varcurrentcandidate;
							}
						}
						//both candidates have no sub bones in the required direction
						if (varlastcandidatehasvalidsubbones == null && varcurrentcandidatehasvalidsubbones == null) {
							if (varpdirection == enumcardinaldirections.dirup)
								//specific head case related to single bone-head armatures
								if (varcurrentcandidate.childCount == 0)
									varreturnvalue = varcurrentcandidate;
						}
					}
					//we have a current candidate and no return value candidate
					else
						//the return vaulue candidate becomes the current candidate
						varreturnvalue = varcurrentcandidate;
				}
			}
			//we effectively found no valid candidates since the return value equals the starting bone, so we null the result
			if (varreturnvalue == varproot) {
				varreturnvalue = null;
			}
		}
		return varreturnvalue;
	}
	
	/// <summary>
	/// very simple recursive function that counts the sub bones of varpsource, used to weigh out a limb in respect to another
	/// </summary>
	/// <param name="varpsource">
	/// The source bone
	/// </param>
	/// <returns>
	/// Returns the total number of varpsource sub bones
	/// </returns>
	private static int metcountsubbones(Transform varpsource) {
		int varreturnvalue = 0;
		foreach (Transform varsubbone in varpsource) {
			varreturnvalue+= metcountsubbones(varsubbone);
		}
		return varreturnvalue+varpsource.childCount;
	}
	
	/// <summary>
	/// A simple function that calculates the segment distance between varptarget and varpsource moving up the parent chain and adding
	/// each segment to the final result. This function is needed to calculate extension of varptarget in respect to varpsource.
	/// </summary>
	/// <param name="varpsource">
	/// The 'destination' bone. It's considered implied that varptarget is a sub bone of this one.
	/// </param>
	/// <param name="varptarget">
	/// The starting bone. Parents will be cycled to reach varpsource.
	/// </param>
	/// <returns>
	/// The whole segment per segment distance that separates varptarget from varpsource.
	/// </returns>
	private static float metpathlength(Transform varpsource, Transform varptarget) {
		float varreturnvalue = 0;
		bool varactualparent = false;
		Transform varcurrentpath;
		
		varcurrentpath = varptarget;
		//cycle varptarget's parents until varpsource is reached
		while (varcurrentpath.parent != null && !varactualparent) {
			//add the distance magnitude to the final result
			varreturnvalue+= (varcurrentpath.position-varcurrentpath.parent.position).magnitude;
			varcurrentpath = varcurrentpath.parent;
			//complete the cycle since varpsource was found
			if (varcurrentpath == varpsource)
				varactualparent = true;
		}
		if (!varactualparent)
			varreturnvalue = 0;
		return varreturnvalue;
	}

	/// <summary>
	/// Used to check if varptarget is a sub bone of varpsource
	/// </summary>
	/// <param name="varpsource">
	/// The bone that will be compared with the target's parents
	/// </param>
	/// <param name="varptarget">
	/// The bone whose parents will be sweeped up to the root if necessary
	/// </param>
	/// <returns>
	/// True if varptarget is a sub bone of varpsource
	/// </returns>
	private static bool metcheckifsameorparent(Transform varpsource, Transform varptarget) {
		bool varreturnvalue = false;
		Transform varrelationsearch = varptarget.parent;
		//bones are the same. return true.
		if (varptarget == varpsource)
			varreturnvalue = true;
		//if the source is null or it equals the root or start bone, we presume that the search is unnecessary
		if (varpsource != null && varpsource != varsource) {
			//as long as there's a parent for varptarget, and varptarget's parent is not varpsource
			while (varrelationsearch != null && varrelationsearch != varpsource) {
				varrelationsearch = varrelationsearch.parent;
			}
			if (varrelationsearch == varpsource) {
				varreturnvalue = true;
			}
		}
		return varreturnvalue;
	}
	
	/// <summary>
	/// Recursive procedure that fully explores the armature in search of the extremities.
	/// Allocates the global variables 	varheadstart, vararmlstart, vararmrstart, varleglstart, varlegrstart
	/// NOTE: Very CPU intensive, albeit fast. For bad models (animation models rather than game models), it might even seem to hang unity.
	/// </summary>
	/// <param name="varpsource">
	/// The source transform. Needs necessarily be the parent of all the extremities.
	/// </param>
	/// <param name="varporigin">
	/// Recursion helper. In method calls needs be the same as varpsource. During iteration, will become the parent bone of each body segment
	/// which will be used to determine the segment's direction and likeliness of a certain limb.
	/// </param>
	/// <param name="varplimb">
	/// Used to determine the body segment to look for, from the hardcoded values 'head', 'legs', 'arms'. Will be set by the recursion and needs be "" in the method call
	/// </param>
	private static void metstarfinder(Transform varpsource, Transform varporigin, string varplimb) {
		//cardinal direction value
		enumcardinaldirections varcurrentdirection;
		//distance in Unity units of the current sub bone in respect to varporigin
		float varcurrentdistance = 0;
		bool varheadless = true;
		
		//init. triggered on method call only
		if (varpsource == varporigin && varplimb == "") {
			//reset the global limb start variables
			varheadstart = varpsource;
			vararmlstart = varpsource;
			vararmrstart = varpsource;
			varleglstart = varpsource;
			varlegrstart = varpsource;
			//distance calculation variables for the arms. will be compared with varcurrentdistance to determine the best extremity candidate
			vararmlcurrentdistance = 0;
			vararmrcurrentdistance = 0;
			//set 'head' for first iteration
			varplimb = "head";
		}
		switch (varplimb) {
			case "head":
				//scan the whole armature
				foreach (Transform varsubbone in varpsource) {
					//determine the current sub bone cardinal direction in respect to the origin (up, left, right, etc.)
					varcurrentdirection = metcardinaldirection(metcardinaldistance(varporigin, varsubbone));
					//if this bone is up from the source, and its y coordinate is higher, it's a better candidate
					if (varsubbone.position.y > varheadstart.position.y && varcurrentdirection == enumcardinaldirections.dirup)
						varheadstart = varsubbone;
					//specific head case where the head is one bone
					if (metcardinaldirection(metcardinaldistance(varheadstart, varsubbone))== enumcardinaldirections.dirnone) {
						//exchange the head bone only if it has no children (for those several cases in which the models are actually 'headless', bone wise
						if (varsubbone.childCount == 0) {
							varheadstart = varsubbone;
						}
						else {
							//the head bone becomes the one with the most children between the cohincident candidates
							if (metcountsubbones(varsubbone) > metcountsubbones(varheadstart))
								varheadstart = varsubbone;
						}
					}
					//call the recursion on the next sub bone, for the head
					metstarfinder(varsubbone,varporigin, varplimb);
					//we've cycled at least once, so the model is not headless
					if (varheadless == true)
						varheadless	= false;
				}
				//head iteration is finished, call the recursion for the legs
				if (varpsource == varporigin) {
					metstarfinder(varporigin,varporigin, "legs");
				}
				break;
			case "legs":
				//scan the whole armature
				foreach (Transform varsubbone in varpsource) {
					//determine the current sub bone cardinal direction in respect to the origin (up, left, right, etc.)
					varcurrentdirection = metcardinaldirection(metcardinaldistance(varporigin, varsubbone));
					//assign a new extremity candidate if the current sub bone is to the side of the spine, in a 90° angle from the origin
					//left leg
					if (varsubbone.position.y <= varleglstart.position.y && varsubbone.position.x < varporigin.position.x && (varcurrentdirection == enumcardinaldirections.dirleft || varcurrentdirection == enumcardinaldirections.dirdownleft || varcurrentdirection == enumcardinaldirections.dirdown))
					    varleglstart = varsubbone;
					//right leg
					if (varsubbone.position.y <= varlegrstart.position.y && varsubbone.position.x > varporigin.position.x && (varcurrentdirection == enumcardinaldirections.dirright || varcurrentdirection == enumcardinaldirections.dirdownright || varcurrentdirection == enumcardinaldirections.dirdown))
					    varlegrstart = varsubbone;
					//call the recursion on the next sub bone, for the legs
					metstarfinder(varsubbone,varporigin, varplimb);
				}
				//legs iteration is finished, call the recursion for the arms
				if (varpsource == varporigin) {
					metstarfinder(varporigin,varporigin, "arms");
				}
				break;
			case "arms":
				//scan the whole armature for a 'strict shoulder' where the arms are stretched horizontally
				foreach (Transform varsubbone in varpsource) {
					//determine the current sub bone cardinal direction in respect to the origin (up, left, right, etc.)
					varcurrentdirection = metcardinaldirection(metcardinaldistance(varporigin, varsubbone));
					//calculate the distance in units from the origin
					varcurrentdistance = Mathf.Abs(varsubbone.position.x-varporigin.position.x);
					//compare the current sub bone with current arm bones only if the current bone is not a sub bone of the head (posed that the model is not headless) or legs
					if ((!metcheckifsameorparent(varheadstart, varsubbone) || (varheadless)) && !metcheckifsameorparent(varleglstart, varsubbone) && !metcheckifsameorparent(varlegrstart, varsubbone)) {
						if (!metcheckifsameorparent(varsubbone, varheadstart) && !metcheckifsameorparent(varsubbone, varleglstart) && !metcheckifsameorparent(varsubbone, varlegrstart)) {
							//assign a new candidate if the bone is to the side of the spine, farther away than the last candidate, and not a sub bone of the head or the legs
							//left arm
							if (varcurrentdistance > vararmlcurrentdistance && varsubbone.position.x < varporigin.position.x && varsubbone.position.y > varleglstart.position.y && varsubbone.position.y > varlegrstart.position.y) {
								vararmlstart = varsubbone;
								vararmlcurrentdistance = varcurrentdistance;
							}
							//right arm
							if (varcurrentdistance > vararmrcurrentdistance && varsubbone.position.x > varporigin.position.x && varsubbone.position.y > varleglstart.position.y && varsubbone.position.y > varlegrstart.position.y) {
								vararmrstart = varsubbone;
								vararmrcurrentdistance = varcurrentdistance;
							}
						}
					}
					//call the recursion for the next sub bone of the arms
					metstarfinder(varsubbone,varporigin, varplimb);
				}
				if (varpsource == varporigin) {
					metstarfinder(varporigin,varporigin, "arms_secundary");
				}
				break;
			case "arms_secundary":
				//scan the whole armature
				foreach (Transform varsubbone in varpsource) {
					//determine the current sub bone cardinal direction in respect to the origin (up, left, right, etc.)
					varcurrentdirection = metcardinaldirection(metcardinaldistance(varporigin, varsubbone));
					//calculate the distance in units from the origin
					varcurrentdistance = metpathlength(varporigin, varsubbone);
					//compare the current sub bone with current arm bones only if the current bone is not a sub bone of the head (posed that the model is not headless) or legs
					if ((!metcheckifsameorparent(varheadstart, varsubbone) || (varheadless)) && !metcheckifsameorparent(varleglstart, varsubbone) && !metcheckifsameorparent(varlegrstart, varsubbone)) {
						if (!metcheckifsameorparent(varsubbone, varheadstart) && !metcheckifsameorparent(varsubbone, varleglstart) && !metcheckifsameorparent(varsubbone, varlegrstart)) {
							//assign a new candidate if the bone is to the side of the spine, farther away than the last candidate, and not a sub bone of the head or the legs
							//left arm
							if (varcurrentdistance > vararmlcurrentdistance && varsubbone.position.x < varporigin.position.x && varsubbone.position.y > varleglstart.position.y && varsubbone.position.y > varlegrstart.position.y) {
								if (vararmlstart.position.y > varsubbone.position.y) {
									//candidate the new bone only if it's farther away to the side in respect to the current start
									if (vararmlstart.position.x > varsubbone.position.x) {
										vararmlstart = varsubbone;
										vararmlcurrentdistance = varcurrentdistance;
									}
								}
							}
							//right arm
							if (varcurrentdistance > vararmrcurrentdistance && varsubbone.position.x > varporigin.position.x && varsubbone.position.y > varleglstart.position.y && varsubbone.position.y > varlegrstart.position.y) {
								if (vararmrstart.position.y > varsubbone.position.y) {
									//candidate the new bone only if it's farther away to the side in respect to the current start
									if (vararmlstart.position.x < varsubbone.position.x) {
										vararmrstart = varsubbone;
										vararmrcurrentdistance = varcurrentdistance;
									}
								}
							}
						}
					}
					//call the recursion for the next sub bone of the arms
					metstarfinder(varsubbone,varporigin, varplimb);
				}
				break;
		}
	}
	
	/// <summary>
	/// Function that will return the transform of that bone which is the immediate sub bone of varptarget, and which is a parent of varpsource
	/// </summary>
	/// <param name="varpsource">
	/// The source transform whose parents will be climbed up to varptarget
	/// </param>
	/// <param name="varptarget">
	/// The target bone which will be searched in varpsource's parent chain
	/// </param>
	/// <returns>
	/// The immediate sub bone of varptarget which is a parent of varpsource
	/// </returns>
	private static Transform metclimbuptobone(Transform varpsource, Transform varptarget) {
		Transform varreturnvalue = null;
		varreturnvalue = varpsource;
		//end and cycle condition to keep iterating through the parents for the source
		while (varreturnvalue.parent != null && varreturnvalue.parent != varptarget) {
			varreturnvalue = varreturnvalue.parent;
		}
		
		//return null if the source has no parent or if it has an immediate parent which is the target
		if (varreturnvalue != null)
			if (varreturnvalue.parent == null || varreturnvalue.parent != varptarget)
				varreturnvalue = null;
		return varreturnvalue;
	}
	
	/// <summary>
	/// The function 'connects' a limb bone with a spine bone, basically checking each spine bone to see if such bone is in the parent chain of the limbstart
	/// </summary>
	/// <param name="varplimbstart">
	/// The origin bone of the limb. It's basically the extremity found by the star finder
	/// </param>
	/// <param name="varplimbend">
	/// The current limb end candidate wichi will be 'trimmed' when necessary
	/// </param>
	/// <returns>
	/// The new transform candidate of the most suitable limb end
	/// </returns>
	private static Transform metassurelimbminimumdistance(Transform varplimbstart, Transform varplimbend) {
		//temp bones for cycling through the limbs
		Transform varcurrentbone = null;
		Transform varreturnvalue = null;
		int varlocalcoordy, varlocalcoordx, varfailsafe;

		//assing the spine exploration variables, to start from the root
		varlocalcoordy = varbottomboney;
		varlocalcoordx = varmatrixsize/2;
		varreturnvalue = varplimbend;
		//assign the emergency cycle exit for very badly designed models which cause coordinate traps
		varfailsafe = varmatrixsize;
		
		//scan the spine from the root to the top bone
		while (varlocalcoordy >= 0 && varlocalcoordy >= vartopboney && varreturnvalue != null) {
			//store the current spine bone
			varcurrentbone = varbonesmatrix[varlocalcoordy][varlocalcoordx];
			//check if the current spine bone is a parent of the limb in which case assign it as the return value
			while (varreturnvalue != null && metcardinaldirection(metcardinaldistance(varcurrentbone,varreturnvalue)) == enumcardinaldirections.dirnone && varfailsafe >= 0) {
				varreturnvalue = metclimbuptobone(varplimbstart, varreturnvalue);
				varfailsafe--;
			}
			//go up to the next spine bone
			varlocalcoordy--;
		}
		return varreturnvalue;
	}
	
	/// <summary>
	/// Assure that arms and legs don't share the same origin, which would mean a zero lenght shoulder space and thigh space
	/// The procedure will check arms and legs origins and will trim the end bones in the case of a cohincidence
	/// </summary>
	private static void metstarnormalize() {
		Transform varlocalarmlstart, varlocalarmrstart, varlocalleglstart, varlocallegrstart;
		//bone trap fail safe for very bad models
		int varfailsafe;
		
		//local start copies
		varlocalarmlstart = vararmlstart;
		varlocalarmrstart = vararmrstart;
		varlocalleglstart = varleglstart;
		varlocallegrstart = varlegrstart;
		
		varfailsafe = varmatrixsize;
		metprint("Normalize start: Arm l end " + vararmlend + " - start " + vararmlstart +"\nArm r end " + vararmrend + " - start " + vararmrstart + "\nLeg l end " + varleglend + " - start " + varleglstart + "\nLeg r end " + varlegrend + " - start " + varlegrstart,0);
		//trim the arms starts if the distance between them is none
		while (varlocalarmlstart != null && varlocalarmrstart != null && varfailsafe > 0 && metcardinaldirection(metcardinaldistance(varlocalarmlstart, varlocalarmrstart)) == enumcardinaldirections.dirnone) {
			varlocalarmlstart = metclimbuptobone(vararmlend, varlocalarmlstart);
			varlocalarmrstart = metclimbuptobone(vararmrend, varlocalarmrstart);
			varfailsafe--;
		}
		varfailsafe = varmatrixsize;
		//trim the legs starts if the distance between them is none
		while (varlocalleglstart != null && varlocallegrstart != null && varfailsafe > 0 && metcardinaldirection(metcardinaldistance(varlocalleglstart, varlocallegrstart)) == enumcardinaldirections.dirnone) {
			varlocalleglstart = metclimbuptobone(varleglend, varlocalleglstart);
			varlocallegrstart = metclimbuptobone(varlegrend, varlocallegrstart);
			varfailsafe--;
		}
		//reassign the global variables
		vararmlstart = varlocalarmlstart;
		vararmrstart = varlocalarmrstart;
		varleglstart = varlocalleglstart;
		varlegrstart = varlocallegrstart;
		metprint("Nomralize end: Arm l end " + vararmlend + " - start " + vararmlstart +"\nArm r end " + vararmrend + " - start " + vararmrstart + "\nLeg l end " + varleglend + " - start " + varleglstart + "\nLeg r end " + varlegrend + " - start " + varlegrstart,0);
	}
	
	/// <summary>
	/// This function determines which is the actual limb segment candidate that will be store in the bones matrix along with its extension
	/// </summary>
	private static void metstarfindorigins() {
		//local spine scanning variables
		int varlocalcoordy, varlocalcoordx;
		//local copies of the global variables compiled by metstarfinder
		Transform varlocalarmlstart, varlocalarmrstart, varlocalleglstart, varlocallegrstart, varlocalnexus, varcurrentspinebone;
		
		//set the local limb start bones
		varlocalarmlstart = vararmlstart;
		varlocalarmrstart = vararmrstart;
		varlocalleglstart = varleglstart;
		varlocallegrstart = varlegrstart;
		//set the spine scanner variables to the root
		varlocalcoordy = varbottomboney;
		varlocalcoordx = varmatrixsize/2;
		//cycle through the spine in the bones matrix
		while (varlocalcoordy >= vartopboney && varlocalcoordy >= 0) {
			varcurrentspinebone = varbonesmatrix[varlocalcoordy][varlocalcoordx];
			//arm l, locate the start candidate
			varlocalnexus = metclimbuptobone(varlocalarmlstart, varcurrentspinebone);
			if (varlocalnexus != null) {
				varlocalarmlstart = varlocalnexus;
			}
			//arm r, locate the start candidate
			varlocalnexus = metclimbuptobone(varlocalarmrstart, varcurrentspinebone);
			if (varlocalnexus != null) {
				varlocalarmrstart = varlocalnexus;
			}
			//leg l, locate the start candidate
			varlocalnexus = metclimbuptobone(varlocalleglstart, varcurrentspinebone);
			if (varlocalnexus != null) {
				varlocalleglstart = varlocalnexus;
			}
			//leg r, locate the start candidate
			varlocalnexus = metclimbuptobone(varlocallegrstart, varcurrentspinebone);
			if (varlocalnexus != null) {
				varlocallegrstart = varlocalnexus;
			}
			varlocalcoordy--;
		}
		//the function now has valid candidates or null for each limb, so find the spine bone that's the topmost parent of the limb
		varlocalarmlstart = metassurelimbminimumdistance(vararmlstart, varlocalarmlstart);
		varlocalarmrstart = metassurelimbminimumdistance(vararmrstart, varlocalarmrstart);
		varlocalleglstart = metassurelimbminimumdistance(varleglstart, varlocalleglstart);
		varlocallegrstart = metassurelimbminimumdistance(varlegrstart, varlocallegrstart);
		//assign the newly found limb ends to the respective global variables
		vararmlend = vararmlstart;
		vararmrend = vararmrstart;
		varleglend = varleglstart;
		varlegrend = varlegrstart;
		//reassing the limb starts
		vararmlstart = varlocalarmlstart;
		vararmrstart = varlocalarmrstart;
		varleglstart = varlocalleglstart;
		varlegrstart = varlocallegrstart;
		//the star has been resolved to its maximum extent. proceed to normalize distancing the limbs from the spine in case of shared heads
		metstarnormalize();
	}
	
	/// <summary>
	/// A simple function that calculates varplimb y position and compares it with each spine bone in the bones matrix
	/// to return the bones matrix y candidate where the limb is located
	/// </summary>
	/// <param name="varplimb">
	/// The limb that needs be located along the spine
	/// </param>
	/// <returns>
	/// The bones matrix y candidate of the limb
	/// </returns>
	private static int metlocatelimbinspine(Transform varplimb) {
		//local spine navigation
		int varlocaly, varlocalx, varreturnvalue;
		//helper spine bone
		Transform varcurrentspinebone;
		//helper boolean to determine if the limb has been found or not
		bool varfitted = false;
		
		//start to scan from the spine root
		varlocaly = varbottomboney;
		varlocalx = varmatrixsize/2;
		varreturnvalue = -1;
		if (varplimb != null) {
			//cycle the spine as long as varplimb's y coordinate is higher than the spine's bone
			while (!varfitted && varlocaly > 0 && varbonesmatrix[varlocaly][varlocalx] != null) {
				varcurrentspinebone = varbonesmatrix[varlocaly][varlocalx];
				if (varplimb.position.y <= varcurrentspinebone.position.y) {
					varfitted = true;
					varreturnvalue = varlocaly;
				}
				varlocaly --;
			}
		}
		//if the limb's y is higher than the topmost bone's, return the topmost coordinate
		if (varreturnvalue == -1)
			varreturnvalue = vartopboney;
		return varreturnvalue;
	}
	
	/// <summary>
	/// This function stores the limbs in the bones matrix following a predetermined direction and setup
	/// </summary>
	/// <param name="varparms">
	/// true if the method call is to store the arms, false if the method call is to store the legs
	/// </param>
	/// <param name="varpleft">
	/// true for storing the left side of the limb
	/// </param>
	/// <returns>
	/// will always return 1. provided for debug purposes.
	/// </returns>
	private static int metstorelimbinbonesmatrix(bool varparms, bool varpleft) {
		int varreturnvalue = 1;
		//helper variables to control the matrix navigation and limb exploration
		int vardisplacement;
		//helper directions to compare the limbs and determine part fitness for the limbs
		enumcardinaldirections varlimbdirection;
		//helper part for iteration
		Transform varcurrentcandidate;
		int varfittedlimby = -1;
		
		//init the iteration variables
		varcurrentcandidate = null;
		vardisplacement = 0;
		
		//assign the global bones matrix variables that hold the shoulders and legs y coordinates in the matrix
		//thanks to metlocatelimbinspine which returns the limb's y position along the spine
		//and assign the displacement helper variable based on varparms and varpleft, to determine the direction
		//of bones matrix allocation based on the limb and side
		//left arm
		if (varparms == true && varpleft == true && vararmlstart != null) {
			varfittedlimby = metlocatelimbinspine(vararmlstart);
			varcurrentcandidate = vararmlstart;
			varshouldersy = varfittedlimby;
			vardisplacement = -1;
		}
		//right arm
		if (varparms == true && varpleft == false && vararmrstart != null) {
			varfittedlimby = metlocatelimbinspine(vararmrstart);
			varcurrentcandidate = vararmrstart;
			if (varshouldersy == vartopboney)
				varshouldersy = varfittedlimby;
			vardisplacement = 1;
		}
		//left leg
		if (varparms == false && varpleft == true && varleglstart != null) {
			varcurrentcandidate = varleglstart;
			varlegsy = varbottomboney;
			vardisplacement = -1;
		}
		//right leg
		if (varparms == false && varpleft == false && varlegrstart != null) {
			varcurrentcandidate = varlegrstart;
			varlegsy = varbottomboney;
			vardisplacement = 1;
		}
		
		//check if the current limb and side have a valid candidate thanks to the star functions
		if (varcurrentcandidate != null) {
			//pre set the x coordinate based on the limb side through varpleft
			varcoordx = (varmatrixsize/2)-vardisplacement;
			//pre set the y coordinate based on the limb through varparms
			if (!varparms) {
				varcoordy = varlegsy;
			}
			else {
				varcoordy = varshouldersy;
			}
			metprint("Storing in bones matrix: shoulders " + varshouldersy +  " legs " + varlegsy + " candidate " + varcurrentcandidate + " position " + varcoordy + " " + varcoordx,4);
			varlimbdirection = enumcardinaldirections.dirnone;
			//determine the direction to follow based on the limb
			if (!varparms) {
				varlimbdirection = enumcardinaldirections.dirdown;
			}
			else {
				if (varpleft)
					varlimbdirection = enumcardinaldirections.dirleft;
				else 
					varlimbdirection = enumcardinaldirections.dirright;
			}
			//store the limb in the bones matrix (the function is the same for the spine and all limbs)
			varreturnvalue = metstorecardinaldirectioninbonesmatrix(varlimbdirection, varcurrentcandidate, varcurrentcandidate, false, false, false);
		}
		return varreturnvalue;
	}
	
	/// <summary>
	/// Calculates if varpdirection is 'lax' in respect to varpsource, meaning if it's 90 degrees around varpsource direction
	/// for example, right top is lax in respect to right, as is right down. Equally, left is lax in respect to left down, as is down.
	/// </summary>
	/// <param name="varpdirection">
	/// The direction we need to know the laxity of
	/// </param>
	/// <param name="varpsource">
	/// The source direction, to which varpdirection will be cast
	/// </param>
	/// <returns>
	/// True if varpdirection is lax in respect to varpsource
	/// </returns>
	private static bool metlaxdirection(enumcardinaldirections varpdirection, enumcardinaldirections varpsource) {
		bool varreturnvalue = false;
		//ignore the cases where parameters are none
		if (varpdirection != enumcardinaldirections.dirnone && varpsource != enumcardinaldirections.dirnone)  {
			//convert directions in integers
			int varlocaldirection = (int)varpdirection;
			int varplus = (int)varpsource;
			int varminus = (int)varpsource;
			varplus++;
			varminus--;
			//loop the values if they exceed the direction wheel limits
			if (varplus > 8)
				varplus = 1;
			if (varminus < 1)
				varminus = 8;
			//confront the values
			if (varlocaldirection == (int)varpsource || varlocaldirection == varplus || varlocaldirection == varminus) {
				varreturnvalue = true;
			}
		}
		return varreturnvalue;
	}
	
	/// <summary>
	/// Add a character joint and a rigidbody to varpdestination, and connect the new rigidbody to varporigin's one
	/// </summary>
	/// <param name="varpdestination">
	/// The bone to which the physics will be added
	/// </param>
	/// <param name="varporigin">
	/// The bone which rigidbody will act as anchor for varpdestination's new rigidbody
	/// </param>
	static void metaddphysicstobone(Transform varpdestination, Transform varporigin) {
		//cache the rigidbody and add it if necessary
		Rigidbody varcurrentrigidbody = varpdestination.GetComponent<Rigidbody>();
		if (varcurrentrigidbody == null)
			varcurrentrigidbody = varpdestination.gameObject.AddComponent<Rigidbody>();
		//set the global drag values of the rigidbody (added for consistency of unlinked limbs, with the inclusion of the body part selector)
		if (vardrag != 0)
			varcurrentrigidbody.drag = vardrag;
		if (varangulardrag != 0.05f)
			varcurrentrigidbody.angularDrag = varangulardrag;
		if (varcreatekinematic)
			varcurrentrigidbody.isKinematic = true;
		//failsafe to avoid adding physics to the source bone or otherwise unparented bones (for very bad, animation models)
		if (varpdestination.parent != null && varpdestination != varsource) {
			CharacterJoint varjoint = (CharacterJoint)varpdestination.gameObject.AddComponent<CharacterJoint>();
			//connect the rigidbody if varpdestination has a parented physical object
			if (varporigin != null)
				varjoint.connectedBody = varporigin.GetComponent<Rigidbody>();
		}
	}
	
	/// <summary>
	/// this procedure scans the spine and limbs and stores rigidbodies and joints for each, respecting the maximum
	/// exploration size set in the options. the result is purely mechanical since the physics will all have equal, default
	/// values and will need further setup to behave properly
	/// </summary>
	private static void metaddbonephysics() {
		//local bones matrix exploration variables
		int varlocaly, varlocalx;
		//helper bones for the iteration
		Transform varcurrentbone, varoriginbone;
		
		varcurrentbone = null;
		varoriginbone = null;
		//spine
		varcurrentexplorationsize = 0;
		varlocaly = varbottomboney;
		varlocalx = varmatrixsize/2;
		//scan the spine from the source bone to the top
		while ((varcurrentexplorationsize < varexplorationsize || varexplorationsize == 0) && varlocaly >= 0 && varbonesmatrix[varlocaly][varlocalx] != null) {
			varcurrentbone = varbonesmatrix[varlocaly][varlocalx];
			//add physics to the spine bones
			metaddphysicstobone(varcurrentbone, varoriginbone);
			varoriginbone = varcurrentbone;
			varcurrentexplorationsize++;
			varlocaly--;
		}
		//if there's at least one arm
		if (varshouldersy >= 0) {
			//arm left
			//calculate the minimum distance of the shoulders
			varcurrentexplorationsize = varbottomboney-varshouldersy;
			varlocaly = varshouldersy;
			varlocalx = varmatrixsize/2;
			varcurrentbone = null;
			varoriginbone = varbonesmatrix[varlocaly][varlocalx];
			varlocalx++;
			//scan the left arm enforcing the exploration size
			while ((varcurrentexplorationsize < varexplorationsize || varexplorationsize == 0) && varlocalx < varmatrixsize && varbonesmatrix[varlocaly][varlocalx] != null) {
				varcurrentbone = varbonesmatrix[varlocaly][varlocalx];
				//add the physics
				metaddphysicstobone(varcurrentbone, varoriginbone);
				varoriginbone = varcurrentbone;
				varcurrentexplorationsize++;
				varlocalx++;
			}
			//arm right same logic as the left arm but with a different displacement
			varcurrentexplorationsize = varbottomboney-varshouldersy;
			varlocaly = varshouldersy;
			varlocalx = varmatrixsize/2;
			varcurrentbone = null;
			varoriginbone = varbonesmatrix[varlocaly][varlocalx];
			varlocalx--;
			//scan the left arm enforcing the exploration size
			while ((varcurrentexplorationsize < varexplorationsize || varexplorationsize == 0) && varlocalx >= 0 && varbonesmatrix[varlocaly][varlocalx] != null) {
				varcurrentbone = varbonesmatrix[varlocaly][varlocalx];
				//add the physics
				metaddphysicstobone(varcurrentbone, varoriginbone);
				varoriginbone = varcurrentbone;
				varcurrentexplorationsize++;
				varlocalx--;
			}
		}
		if (varlegsy >=0) {
			//leg left
			varcurrentexplorationsize = varlegsy-varbottomboney+1;
			varlocaly = varlegsy;
			varlocalx = varmatrixsize/2;
			varcurrentbone = null;
			varoriginbone = varbonesmatrix[varlocaly][varlocalx];
			varlocalx++;
			//scan the left leg enforcing the exploration size
			while ((varcurrentexplorationsize < varexplorationsize || varexplorationsize == 0) && varlocaly<varmatrixsize && varbonesmatrix[varlocaly][varlocalx] != null) {
				varcurrentbone = varbonesmatrix[varlocaly][varlocalx];
				//add the physics
				metaddphysicstobone(varcurrentbone, varoriginbone);
				varoriginbone = varcurrentbone;
				varcurrentexplorationsize++;
				varlocaly++;
			}
			//leg right
			varcurrentexplorationsize = varlegsy-varbottomboney+1;
			varlocaly = varlegsy;
			varlocalx = varmatrixsize/2;
			varcurrentbone = null;
			varoriginbone = varbonesmatrix[varlocaly][varlocalx];
			varlocalx--;
			//scan the right leg enforcing the exploration size
			while ((varcurrentexplorationsize < varexplorationsize || varexplorationsize == 0) && varlocaly<varmatrixsize && varbonesmatrix[varlocaly][varlocalx] != null) {
				varcurrentbone = varbonesmatrix[varlocaly][varlocalx];
				//add the physics
				metaddphysicstobone(varcurrentbone, varoriginbone);
				varoriginbone = varcurrentbone;
				varcurrentexplorationsize++;
				varlocaly++;
			}
		}
	}
	
	/// <summary>
	/// This is a recursive function responsible for storing a cardinal direction in the bones matrix. It'll basically memorize a complete body segment
	/// </summary>
	/// <param name="varpdirection">
	/// Cardinal direction of exploration (up, right, left, etc.)
	/// </param>
	/// <param name="varproottransform">
	/// The starting point of the exploration. The spine always starts at x varmatrixsize/2, y varmatrixsize/2
	/// </param>
	/// <param name="varporigin">
	/// Auxilliary exploration variable necessary for recursion. In method calls, needs be always equal to varproottransform
	/// </param>
	/// <param name="varpskip">
	/// Auxilliary recursion variable. It needs be false in all method calls, but will be set to true by the algorithm if the bone needs to be skipped rather than stored.
	/// </param>
	/// <param name="varpstoretopbottomspinelimits">
	/// Important variable that needs be true for the spine and false for the other body segments. Will memorize the top and bottom y coordinate of the relative bones.
	/// </param>
	/// <param name="varpstrict">
	/// If the value is true, the direction of the sub bones will need be exactly like varpdirection. If varpstric is false, a sub bone can be in a +45 -45 angle in respect to the root to still qualify as a good candidate for the body segment.
	/// </param>
	/// <returns>
	/// If there's an exploration error the return value will be 0, otherwise it'll be 1 in all other cases.
	/// </returns>
	private static int metstorecardinaldirectioninbonesmatrix(enumcardinaldirections varpdirection, Transform varproottransform, Transform varporigin, bool varpskip, bool varpstoretopbottomspinelimits, bool varpstrict) {
		int varreturnvalue = 1;
		int varcandidatebones = 0;
		enumcardinaldirections vardirection = enumcardinaldirections.dirnone;
		Transform varcandidate = null;
		Vector2 vardirectiontofollow = new Vector2();
		bool varlaxdirection = false;
		
		vardirectiontofollow = metcardinaldistancefromdirection(varpdirection);
		if (!varpskip) {
			metprint("Storing " + varproottransform.name + " x " + varcoordx + " y " + varcoordy,4);
			//we can't store the current bone since we exceeded the bones matrix limit
			if (varcoordx>=varmatrixsize || varcoordy>=varmatrixsize || varcoordx < 0 || varcoordy < 0) 
				return 0;
			varbonesmatrix[varcoordy][varcoordx] = varproottransform;
			if (varpstoretopbottomspinelimits) {
				if (vartopboney > varcoordy)
					vartopboney = varcoordy;
				if (varbottomboney < varcoordy)
					varbottomboney = varcoordy;
			}
			varcoordx-=(int)vardirectiontofollow.x;
			varcoordy-=(int)vardirectiontofollow.y;
			varreturnvalue++;
		}
		//explore the current bone if it's a strict exploration, or otherwise only if the child cound is inferior to the sub bones exploration count limit in the options
		if (varproottransform.childCount < varmaxchildcount || varpstrict) {
			foreach (Transform varsubbone in varproottransform) {
				//determine the current direction between the current sub bone and the origin
				vardirection = metcardinaldirection(metcardinaldistance(varporigin,varsubbone));
				//if the direction is not the desired one, we proceed to further checks
				if (vardirection != varpdirection) {
					varlaxdirection = true;
					//check if the current direction is lax, and thus valid, in respect to the requested one
					varlaxdirection = metlaxdirection(vardirection, varpdirection);
					//if the current direction is acceptable, iterate a recursion with a request to store the current bone
					if (varpstrict || !varlaxdirection) {
						varreturnvalue = metstorecardinaldirectioninbonesmatrix(varpdirection, varsubbone, varporigin,true ,varpstoretopbottomspinelimits, varpstrict);
					}
					//the current direction is not acceptable. iterate a recursion skipping the current sub bone
					else {
						varreturnvalue = metstorecardinaldirectioninbonesmatrix(varpdirection, varsubbone, varporigin,false ,varpstoretopbottomspinelimits, varpstrict);
					}
					//there was an error storing the last bone. exit the function.
					if (varreturnvalue == 0) {
						return 0;
					}
				}
				//we store the valid candidate in case the current direction is the one we're looking for
				if (vardirection == varpdirection) {
					varcandidate = varsubbone;
					varcandidatebones++;
				}
			}
		}
		//if we have more than one candidate, we need to find the most suitable one to store in the next iteration
		if (varcandidatebones>1) {
			varcandidate = null;
			//find the first direction sub bone for these candidates
			metprint("Find the first valid " + varpdirection.ToString() + " candidate for " + varproottransform,4);
			varcandidate = metfindvalidcardinalcandidate(varpdirection,varproottransform, varproottransform);
		}
		//we now have one valid candidate if it actually exists
		if (varcandidate != null) {
			//if the exploration is strict, setup the candidate as the origin for the next iteration (assures a geometrical 'straight line' in the bones matrix)
			if (varpstrict) {
				metstorecardinaldirectioninbonesmatrix(varpdirection, varcandidate, varcandidate, false, varpstoretopbottomspinelimits, varpstrict);
			}
			//if the exploration is lax, setup the origin as the origin for the next iteration (allows a segmented line for the limb structure. the bones matrix stays always straight)
			else {
				metstorecardinaldirectioninbonesmatrix(varpdirection, varcandidate, varporigin, false, varpstoretopbottomspinelimits, varpstrict);
			}
			varcurrentexplorationsize++;
		}
		return varreturnvalue;
	}
	
	/// <summary>
	/// very important procedure that will set up each limb and part behavior to effectively act as expected thanks to weight distribution
	/// and joint mechanics set up. the weight data comes from us air force datasheets, and the articulation data comes from arithmetic analysis of the body joints
	/// </summary>
	private void metnormalizebonesmatrix() {
		//helper bone
		Transform varcurrentpart;

		//spine balancement. Total spine weight in humanoids is around 55% of total body mass, with 15% on the pelvis and 5% on the head.
		float varcurrentspineweight, varaccountedweight;
		int varlocalcoordy = varmatrixsize/2;
		int varlocalcoordx = varlocalcoordy;
		int varcurrentlimit = -1;
		int varauxvalue = 0;
		
		varcurrentspineweight = 0;
		varaccountedweight = 0;
		//we manually set the root weight which we consider the pelvis, since its starting position is constant in the matrix
		varcurrentspineweight = vartotalmass * 0.15f;
		varcurrentpart = varbonesmatrix[varlocalcoordy][varlocalcoordx];
		metprint("Normalizing spine root",0);
		//set up the preexisting character joint of the source bone
		metsetmassandjoint(varcurrentpart, Vector3.right, Vector3.forward, -varlowtensionvalue,varlowtensionvalue,varmintensionvalue, varcurrentspineweight);
		//we identify the head as all that's above the shoulders, and distribute the 5% of the body mass weight in all the relevant vertical bones
		varlocalcoordy = varshouldersy;
		varcurrentspineweight = (vartotalmass * 0.05f) / (vartopboney-varshouldersy);
		while (varlocalcoordy >= vartopboney && varbonesmatrix[varlocalcoordy][varlocalcoordx] != null) {
			varcurrentpart = varbonesmatrix[varlocalcoordy][varlocalcoordx];
			varcurrentspineweight = vartotalmass * 0.05f;
			varaccountedweight+= varcurrentspineweight;
			metprint("Normalizing topmost bones " + varcurrentpart.name,0);
			metsetmassandjoint(varcurrentpart, Vector3.right, Vector3.forward, -varmidtensionvalue,varmidtensionvalue,varlowtensionvalue, varcurrentspineweight);
			varlocalcoordy--;
		}
		//distribute 30% of the body weight into all of the spine that's not pelvis and head (
		varlocalcoordy = varbottomboney-1;
		if (varshouldersy == -1) {
			varcurrentlimit = vartopboney;
		}
		else {
			varcurrentlimit = varshouldersy+1;
		}
		varauxvalue = (varbottomboney-varcurrentlimit);
		varcurrentspineweight = (vartotalmass * 0.30f) / varauxvalue;
		while (varlocalcoordy >= varcurrentlimit && varbonesmatrix[varlocalcoordy][varlocalcoordx]) {
			varcurrentpart = varbonesmatrix[varlocalcoordy][varlocalcoordx];
			varaccountedweight+= varcurrentspineweight;
			metprint("Setting spine mass and joint for " + varcurrentpart,0);
			metsetmassandjoint(varcurrentpart, Vector3.right, Vector3.forward, -(varlowtensionvalue/varauxvalue),(varlowtensionvalue/varauxvalue),(varmintensionvalue/varauxvalue), varcurrentspineweight);
			varlocalcoordy--;
		}
		
		//arms set up
		int varbonenodes = 1;
		float varcurrentweight = 0;		
		int varlocaly, varlocalx;
		//helper variable that switches angles based on body side, for correct setup of individual limbs and parts when pointing forward
		int varrotationfixer = 1;
		
		//iterate only if there's at least one arm
		if (varshouldersy>=0) {
			varlocaly = varshouldersy;
			varlocalx = varmatrixsize/2;
			//for cycle to optimize the code. basically swaps from lef to right
			for (int varcounter = 0; varcounter < 2; varcounter++) {
				//there's at least one arm node, since we have shoulders
				varbonenodes = 1;
				varcurrentweight = 0;
				//pass the left side first
				if (varcounter == 0) {
					varlocalx = (varmatrixsize/2)+1;
					//calculate the number of left side nodes
					while (varlocalx+1 < varmatrixsize && varbonesmatrix[varlocaly][varlocalx+1] != null) {
						varbonenodes++;
						varlocalx++;
					}
					//reset the x exploration variable to the start of the left arm
					varlocalx = (varmatrixsize/2)+1;
					varrotationfixer = 1;
				}
				else {
					varlocalx = (varmatrixsize/2)-1;
					//calculate the number of right side nodes to compensate for asymmetrical explorations
					while (varlocalx-1 >= 0 && varbonesmatrix[varlocaly][varlocalx-1] != null) {
						varbonenodes++;
						varlocalx--;
					}
					//reset the x exploration variable to the start of the right arm
					varlocalx= (varmatrixsize/2)-1;
					varrotationfixer = -1;
				}

				//explore the nodes and assign the proper weights
				varlocaly = varshouldersy;
				if (varbonenodes == 0) {
					metprint("No Arm extensions. Unable to allocate Upper body weight. Releasing extremity.",2);
					//destroy the extremity physics since they have no attached physics part
					metreleaseextremity(varbonesmatrix[varlocaly][varlocalx]);
				}
				else {
					//one arm node. distribute the whole 4.5% of the side's weight to it
					if (varbonenodes == 1)
						varcurrentweight = vartotalmass * 0.045f;
					//at least two nodes. assign this weight to the shoulder+forearm
					if (varbonenodes > 1)
						varcurrentweight = vartotalmass * 0.025f;
					varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
					metsetmassandjoint(varcurrentpart,Vector3.up, Vector3.up, -varmintensionvalue,varhightensionvalue,varmidtensionvalue,varcurrentweight);
					//two nodes total we consider them the shoulder+forearm and the arm so we assign the 2% of the arm+hand weight
					if (varbonenodes == 2)
						varcurrentweight = vartotalmass * 0.020f;
					//more than two nodes, we can assign the correct 1.5% weight of the arm to this part
					if (varbonenodes > 2)
						varcurrentweight = vartotalmass * 0.015f;
					//check the side to determine where is the next arm part
					if (varcounter == 0) {
						varcurrentpart = varbonesmatrix[varlocaly][varlocalx+1];
					}
					else {
						varcurrentpart = varbonesmatrix[varlocaly][varlocalx-1];
					}
					//check if we absord the shoulders or not, to better articulate the joint
					if (varfitshoulders == false) {
						metsetmassandjoint(varcurrentpart,Vector3.forward, Vector3.up, -varmidtensionvalue*varrotationfixer,varhightensionvalue*varrotationfixer,varhightensionvalue,varcurrentweight);
					}
					else {
						metsetmassandjoint(varcurrentpart,Vector3.forward, Vector3.up, -varhightensionvalue*varrotationfixer,varhightensionvalue*varrotationfixer,varhightensionvalue,varcurrentweight);
					}
					//three nodes so we have a shoulder+forearm, an arm and a hand which weighs .5%
					if (varbonenodes == 3)
						varcurrentweight = vartotalmass * 0.005f;
					//more than 3 nodes. distribute the .5 of the extremity to it
					if (varbonenodes > 3)
						varcurrentweight = (vartotalmass * 0.005f) / (varbonenodes-2);
					varbonenodes = varbonenodes-2;
					if (varcounter == 0)
						varlocalx +=2;
					else
						varlocalx -=2;
					//cycle through all the remaining body parts besides the arm and assign hand mechanics to them
					while (varbonenodes > 0) {
						varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
						metsetmassandjoint(varcurrentpart,Vector3.up, Vector3.forward, 0,-varhightensionvalue*varrotationfixer,varmidtensionvalue,varcurrentweight);
						if (varcounter == 0)
							varlocalx++;
						else
							varlocalx--;
						varbonenodes--;
					}
				}
			}
		}
		else {
			if (varexplorationsize == 0) {
				metprint("There is no spine derivate that resembles the shoulders. Unable to fine-tune the arms joints and weights.\nTry doubling the bone vertical tolerance parameter.",2);
			}
			else {
				metprint("There is no spine derivate that resembles the shoulders. If they actually exist and you want to include them in the ragdoll,\nyou should increase the maxiumum sub bones allowed or the exploration size.",1);
			}

		}
		
		//legs set up with same logic as the arms
		if (varlegsy>=0) {
			varlocaly = varbottomboney;
			varlocalx = varmatrixsize/2;
			for (int varcounter = 0; varcounter < 2; varcounter++) {
				//there's at least one leg node
				varbonenodes = 1;
				varcurrentweight = 0;
				//pass the left side first
				if (varcounter == 0)
					varlocalx = (varmatrixsize/2)+1;
				else 
					varlocalx = (varmatrixsize/2)-1;
				varlocaly = varbottomboney;
				//go down to the foot (actually the lowest bone if there's an exploration limit)
				while (varlocaly+1 < varmatrixsize && varbonesmatrix[varlocaly+1][varlocalx] != null) {
					varbonenodes++;
					varlocaly++;
				}
				
				varlocaly = varbottomboney;
				if (varbonenodes == 0) {
					metprint("No leg extensions. Unable to allocate lower body weight. Releasing extremity.",2);
					metreleaseextremity(varbonesmatrix[varlocaly][varlocalx]);
				}
				else {
					if (varbonenodes == 1)
						varcurrentweight = vartotalmass * 0.145f;
					if (varbonenodes > 1)
						varcurrentweight = vartotalmass * 0.125f;
					varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
					metsetmassandjoint(varcurrentpart,Vector3.right, Vector3.forward, -varlowtensionvalue,varhightensionvalue,varmidtensionvalue,varcurrentweight);
					if (varbonenodes == 2)
						varcurrentweight = vartotalmass * 0.065f;
					if (varbonenodes > 2)
						varcurrentweight = vartotalmass * 0.05f;
					varcurrentpart = varbonesmatrix[varlocaly+1][varlocalx];
					metsetmassandjoint(varcurrentpart,Vector3.right, Vector3.forward, -varhightensionvalue,-varmidtensionvalue,0,varcurrentweight);
					if (varbonenodes == 3)
						varcurrentweight = vartotalmass * 0.015f;
					if (varbonenodes > 3)
						varcurrentweight = (vartotalmass * 0.015f) / (varbonenodes-2);
					varbonenodes = varbonenodes-2;
					varlocaly +=2;
					while (varbonenodes > 0) {
						varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
						metsetmassandjoint(varcurrentpart,Vector3.right, Vector3.forward, -varmintensionvalue,0,0,varcurrentweight);
						varlocaly++;
						varbonenodes--;
					}
				}
			}
		}
		else {
			if (varexplorationsize == 0) {
				metprint("There is no spine derivate that resembles the legs. Unable to fine-tune the legs joints and weights.\nTry doubling the bone vertical tolerance parameter, or increasing the exploration value.",2);
			}
			else {
				metprint("There is no spine derivate that resembles the legs. If they actually exist and you want to include them in the ragdoll,\nyou should increase the maximum sub bones allowed or the exploration size.",1);
			}
		}
			
	}
	
	/// <summary>
	/// determines a Vector3's 'direction' and returns the index of the most relevant axis
	/// fundamental for component orientation (capsules, joints, etc.)
	/// </summary>
	/// <param name="varpaxis">
	/// The reference axis
	/// </param>
	/// <returns>
	/// returns a normalized vector which points in the direction of the original axis
	/// </returns>
	private Vector3 metdirectionfromaxis(Vector3 varpaxis) {
		Vector3 varreturnvalue;
		int vardirectionaxis = 0;
		
		varreturnvalue = Vector3.zero;
		//cetermine the direction of the axis
		for (int varcounter = 1; varcounter < 3; varcounter++) {
			if (Mathf.Abs(varpaxis[varcounter]) > Mathf.Abs(varpaxis[vardirectionaxis])) {
				vardirectionaxis = varcounter;
			}
		}
		//check for the direction side and swap sign if necessary
		if (varpaxis[vardirectionaxis] > 0) {
			varreturnvalue[vardirectionaxis] = 1;
		}
		else {
			varreturnvalue[vardirectionaxis] = -1;
		}
		return varreturnvalue;
	}
	
	/// <summary>
	/// Recursive function that calculates the weight of a limb adding all of the part's sub bones weight
	/// </summary>
	/// <param name="varppart">
	/// The limb in question
	/// </param>
	/// <returns>
	/// The weight in kilograms of varppart and all its sub bones
	/// </returns>
	private float metlimbmass(Transform varppart) {
		float varreturnvalue = 0;

		if (varppart.GetComponent<Rigidbody>() != null) {
			varreturnvalue = varppart.GetComponent<Rigidbody>().mass;
		}
		foreach (Transform varchildren in varppart) {
			varreturnvalue+= metlimbmass(varchildren);
		}
		return varreturnvalue;
	}
	
	/// <summary>
	/// Technical recursive function that scans the whole armature and sets the joints mode, damper and spring based on the mass of the joint's limb
	/// </summary>
	/// <param name="varpparent">
	/// The target of the tweak
	/// </param>
	/// <param name="varpfrequency">
	/// Sampling frequency for the spring and damping
	/// </param>
	/// <param name="varpdamping">
	/// The damping value in hertz
	/// </param>
	/// <param name="varpdampfactor">
	/// The damping strength in kg/s
	/// </param>
	/// <param name="varpelasticityfactor">
	/// The elasticity in m/s
	/// </param>
	//http://developer.anscamobile.com/content/game-edition-physics-joints
	private void metnormalizerigidity(Transform varpparent, float varpfrequency, float varpdamping, float varpdampfactor, float varpelasticityfactor) {
		CharacterJoint varcurrentjoint = varpparent.GetComponent<CharacterJoint>();
		if (varcurrentjoint != null) {
			metprint("Normalizing joint " + varpparent + " rigidity",0);
			JointDrive varcurrentjointdrive = new JointDrive();
			varcurrentjointdrive.mode = JointDriveMode.Position;
			varcurrentjointdrive.positionDamper = varpdampfactor * (varrigidity/varpfrequency) * varpdamping * metlimbmass(varpparent);
			varcurrentjointdrive.positionSpring = varpelasticityfactor * (varrigidity/varpfrequency) * varpfrequency * metlimbmass(varpparent);
			varcurrentjoint.rotationDrive = varcurrentjointdrive;
			foreach (Transform varchild in varpparent) {
				metnormalizerigidity(varchild, varpfrequency, varpdamping, varpdampfactor,varpelasticityfactor);
			}
		}
	}
	
	/// <summary>
	/// The procedure retrieves varppart's rigidbody and characterjoint and sets them up as per the parameters
	/// </summary>
	/// <param name="varppart">
	/// The bone whose rigidbody and joint will be set by the call
	/// </param>
	/// <param name="varptwistaxis">
	/// The vector representing the 'through' direction of the movement for the joint
	/// </param>
	/// <param name="varpswingaxis">
	/// The vector representing the 'along' direction of movement for the joint
	/// </param>
	/// <param name="varplimitminimum">
	/// The low twist limit of the joint
	/// </param>
	/// <param name="varplimitmaximum">
	/// The high twist limit of the joint
	/// </param>
	/// <param name="varplimitswing">
	/// The swing limit of the joint
	/// </param>
	/// <param name="varppartweight">
	/// The mass in kilograms for varppart's rigidbody
	/// </param>
	private void metsetmassandjoint(Transform varppart, Vector3 varptwistaxis, Vector3 varpswingaxis, float varplimitminimum, float varplimitmaximum, float varplimitswing, float varppartweight ) {
		//we simply ignore null parts since it just means that they're beyond the exploration size
		if (varppart != null) {
			//retrieve the rigidbody and set its mass
			Rigidbody varcurrentbody = varppart.GetComponent<Rigidbody>();
			//null check to avoid exploration verification in bones matrix normalization
			if (varcurrentbody != null) {
				varcurrentbody.mass = varppartweight;
			}
			//retrieve the character joint and set it by parameters
			CharacterJoint varcurrentjoint = varppart.GetComponent<CharacterJoint>();
			//null check to avoid exploration verification in bones matrix normalization
			if (varcurrentjoint != null) {
				metprint("Setting up the joint for " + varppart.name + " joint",0);
				//determine the local direction of the axis for the joint
				varcurrentjoint.axis = metdirectionfromaxis(varppart.InverseTransformDirection(varptwistaxis));
				varcurrentjoint.swingAxis = metdirectionfromaxis(varppart.InverseTransformDirection(varpswingaxis));
				SoftJointLimit varcurrentlimit = new SoftJointLimit();
				//set the low twist
				varcurrentlimit.limit = varplimitminimum;
				varcurrentjoint.lowTwistLimit = varcurrentlimit;
				//set the high twist
				varcurrentlimit.limit = varplimitmaximum;
				varcurrentjoint.highTwistLimit = varcurrentlimit;
				//set the swing
				varcurrentlimit.limit = varplimitswing;
				varcurrentlimit.spring = 0;
				varcurrentjoint.swing1Limit = varcurrentlimit;
			}
		}
	}
	
	/// <summary>
	/// Recursive function that will destroy sphere, box and capsule colliders, joints and rigidbodies from varpparent onwards
	/// </summary>
	/// <param name="varpparent">
	/// The parent of all the sub bones that will be cleared of physics objects
	/// </param>
	private void metclearphysics(Transform varpparent) {
		Component[] varcolliders = varpparent.GetComponentsInChildren(typeof(SphereCollider));
		Component[] varboxcolliders = varpparent.GetComponentsInChildren(typeof(BoxCollider));
		Component[] varcapsulecolliders = varpparent.GetComponentsInChildren(typeof(CapsuleCollider));
		Component[] varjoints = varpparent.GetComponentsInChildren(typeof(CharacterJoint));
		Component[] varrigidbodies = varpparent.GetComponentsInChildren(typeof(Rigidbody));
		Component[] varactuators = varpparent.GetComponentsInChildren(typeof(clsurgentactuator));
		
		//destroy the physics
		foreach (SphereCollider varcurrentcollider in varcolliders) {
			DestroyImmediate(varcurrentcollider);
		}
		foreach (BoxCollider varcurrentcollider in varboxcolliders) {
			DestroyImmediate(varcurrentcollider);
		}
		foreach (CapsuleCollider varcurrentcollider in varcapsulecolliders) {
			DestroyImmediate(varcurrentcollider);
		}
		foreach (CharacterJoint varcurrentcharacterjoint in varjoints) {
			DestroyImmediate(varcurrentcharacterjoint);
		}
		foreach (Rigidbody varcurrentrigidbody in varrigidbodies) {
			DestroyImmediate(varcurrentrigidbody);
		}
		foreach (clsurgentactuator varcurrentactuator in varactuators) {
			DestroyImmediate(varcurrentactuator);
		}
		DestroyImmediate(varpparent.root.GetComponent<clsurgent>());
	}
	
	private static int mettransformdirectionaxis(Vector3 varpaxis) {
		int varreturnvalue = 0;
		if (Mathf.Abs(varpaxis[varreturnvalue]) < Mathf.Abs(varpaxis[1]))
			varreturnvalue = 1;
		if (Mathf.Abs(varpaxis[varreturnvalue]) < Mathf.Abs(varpaxis[2]))
			varreturnvalue = 2;
		
		return varreturnvalue;
	}
	
	private enum enumdirection {
		normal = 0,
		perpendicular = 1,
		lateral = 2
	}
	
	private static Transform varlastsource, varlasttarget;
	private static float varlastcapsulesize;
	
	/// <summary>
	/// Adds a capsule collider to the body parts varpsource and varptarget. the collider will go from varpsource to varptarget,
	/// along varpdirection. If the bones aren't related the function will exit.
	/// </summary>
	/// <param name="varpsource">
	/// The starting point of the collider. Will be the transform that will actually host the collider
	/// </param>
	/// <param name="varptarget">
	/// The destination of the collider. This is usually a child since otherwise the transform will not link along with the source
	/// </param>
	/// <param name="varpradius">
	/// The radius of the collider. If passed will be set directly. If passed as zero will be calculated as the new height/2.
	/// </param>
	/// <returns>
	/// Returns > 0 if there's no problems
	/// </returns>
	private int metaddcapsulecollidertolimbpart(Transform varpsource, Transform varptarget, float varpradius) {
		int varreturnvalue = 0;
		Bounds varnewbounds = new Bounds();
		Transform varlocalpart = varptarget;
		CapsuleCollider varnewcollider;
		BoxCollider varnewboxcollider = null;
		BoxCollider varlastboxcollider = null;
		
		if (!metcheckifsameorparent(varpsource, varptarget))
			return 0;
/*		
		//latent substitution of direct encapsulation. code has not proven beneficial
		while (varlocalpart.parent != null && varlocalpart != varpsource) {
			varnewbounds.Encapsulate(varpsource.InverseTransformPoint(varlocalpart.position));
			varlocalpart = varlocalpart.parent;
		}
*/
		//add source and target to the current bounds
		varnewbounds.Encapsulate(varpsource.InverseTransformPoint(varpsource.position));
		varnewbounds.Encapsulate(varpsource.InverseTransformPoint(varlocalpart.position));

		//check if the collider is already in place. added as failsafe if for some reason the tool is running in play mode
		varnewcollider = varpsource.gameObject.GetComponent<CapsuleCollider>();
		//add and set collider
		if (varnewcollider == null)
			varnewcollider = varpsource.gameObject.AddComponent<CapsuleCollider>();
		//add the physic material if it's set in the options
		if (varmaterial!=null)
			varnewcollider.material = varmaterial;
		varnewcollider.center = varnewbounds.center;
		varnewcollider.height = varnewbounds.size[mettransformdirectionaxis(varnewbounds.size)];
		//1.6a override of the original direction lookup function in favor of a new direct transformation.
		//varnewcollider.direction = mettransformdirectionaxis(mettransformaxis(varpsource, varpdirection));
		varnewcollider.direction = mettransformdirectionaxis(varpsource.InverseTransformDirection((varlocalpart.position-varpsource.position).normalized));
		//check the radius parameter and calculate or assign it as needed
		if (varpradius != 0)
			varnewcollider.radius = varpradius;
		else
			varnewcollider.radius = varnewcollider.height/2;
		//if the radius is superior to the heigh, the collider would deform and become an exploded sphere
		//this check assures that it remains a capsule in such cases
		if (varpradius > varnewcollider.height/2)
			varnewcollider.radius = varnewcollider.height/2;
		
		//we are about to add a capsule collider to the current source-target bone, but we check if the
		//capsule size is above the absorption tolerance
		if (varnewcollider.radius*2+varnewcollider.height / varlastcapsulesize< varabsorbtolerance) {
			//the new capsule is small, and will be absorbed
			//we first release the last target, since it will be included in the new, bigger collider
			//but we memorize its joint anchor, since it'll replace the current part's one
			Rigidbody varlasttargetjointanchor = null;
			if (varlasttarget.GetComponent<CharacterJoint>() != null) {
 				varlasttargetjointanchor = varlasttarget.GetComponent<CharacterJoint>().connectedBody;
			}
			metreleaseextremity(varlocalpart);
			varlastboxcollider = varlastsource.GetComponent<BoxCollider>();
			//check the previous capsule to see if it was a small capsule too
			if (varlastboxcollider != null) {
				//last capsule was small, so we retrieve its box collider to absorb the current capsule
				varnewbounds = new Bounds();
				varnewbounds.center = varlastboxcollider.center;
				varnewbounds.size = varlastboxcollider.size;
				varnewbounds.Encapsulate(varlastsource.InverseTransformPoint(varlocalpart.position));
				varlastboxcollider.center = varnewbounds.center;
				varlastboxcollider.size = varnewbounds.size;
				//replace the rigidbody anchor of the joint, since otherwise it'd become unlinked
				Rigidbody varcurrentjointanchor = null;
				if (varlocalpart.GetComponent<CharacterJoint>() != null) {
					varcurrentjointanchor = varlocalpart.GetComponent<CharacterJoint>().connectedBody;
					if (varcurrentjointanchor == null)
						varcurrentjointanchor = varlasttargetjointanchor;
				}
				//release this extremity source, since it's been absorbed
				metreleaseextremity(varpsource);
			}
			else {
				//last capsule was normal but this is the first small one of the limb, so we create a new collider
				varnewboxcollider = varpsource.gameObject.GetComponent<BoxCollider>();
				if (varnewboxcollider == null)
					varnewboxcollider = varpsource.gameObject.AddComponent<BoxCollider>();
				//add the physic material if it's set in the options
				if (varmaterial!=null)
					varnewcollider.material = varmaterial;
				varnewboxcollider.center = varnewbounds.center;
				Vector3 varnewsize = varnewbounds.size;
				if (varnewsize.x < varnewcollider.radius)
					varnewsize.x = varnewcollider.radius;
				if (varnewsize.y < varnewcollider.radius)
					varnewsize.y = varnewcollider.radius;
				if (varnewsize.z < varnewcollider.radius)
					varnewsize.z = varnewcollider.radius;
				varnewboxcollider.size = varnewsize;
			}
		}
		//store the last source only if we haven't absorbed a capsule in this iteration
		if (varlastboxcollider == null)
			varlastsource = varpsource;
		varlasttarget = varptarget;
		//check what happened in this iteration
		if (varnewboxcollider != null) {
			//we added a new box collider, so this is a little capsule, and we need to destroy the
			//capsule collider
			DestroyImmediate(varnewcollider);
		}
		else {
			//this is a normal capsule, so we overwrite the last capsule size variable, since the current size is above absorption tolerance
			//but only if we didn't absorb a capsule this iteration
			if (varlastboxcollider == null)
				varlastcapsulesize = varnewcollider.radius*2+varnewcollider.height;
			else {
				//this is a further little capsule, and we've absorbed it into the last box collider, so we destroy its capsule
				DestroyImmediate(varnewcollider);
			}
		}

		varreturnvalue = 1;
		return varreturnvalue;
	}
	
	/// <summary>
	/// the procedure that scans the armature and assigns relevant colliders based on the body part. the colliders will be sized centered
	/// and oriented in respect to the body part and its 'hierarchy' in the limb
	/// </summary>
	private void metnormalizecolliders() {
		//helper bones matrix navigation indexes
		int varlocaly, varlocalx;
		//centering and dimensioning bounds helper
		Bounds varcurrentbounds = new Bounds();
		//bone matrix helper
		Transform varcurrentpart = null;
		//auxilliary helper
		Transform varauxpart = null;
		//variables holding temporary size and center for the new collider
		//1.8 deprecated Vector3 varcurrentsize = new Vector3();
		//auxilliary variables that will determine the size of the collider structure for the whole armature
		float varspinewidth = 0;
		float varspinedepth = 0;
		float varspineheight = 0;
		//the reference for the new collider that will be added to the spine
		BoxCollider varnewcollider = null;

		//init
		varlocaly = varmatrixsize/2;
		varlocalx = varlocaly;
		varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
		
		//preliminary check to avoid errors in very bad animation models. not necessary for all game models tested
		if (varcurrentpart == null)
			return;
		
		//normalize the spine
		//use the bounds to grow the size of the minimum geometry that will hold the shoulders, the thighs and the source bone
		if (varshouldersy >= 0) {
			metprint("Inclusion of the shoulders",4);
			varauxpart = varbonesmatrix[varshouldersy][varlocalx+1];
			if (varauxpart != null)
				varcurrentbounds.Encapsulate(varcurrentpart.InverseTransformPoint(varauxpart.position));
			varauxpart = varbonesmatrix[varshouldersy][varlocalx-1];
			if (varauxpart != null)
				varcurrentbounds.Encapsulate(varcurrentpart.InverseTransformPoint(varauxpart.position));
		}
		if (varlegsy >= 0) {
			metprint("Inclusion of the legs",4);
			varauxpart = varbonesmatrix[varlegsy][varlocalx+1];
			if (varauxpart != null)
				varcurrentbounds.Encapsulate(varcurrentpart.InverseTransformPoint(varauxpart.position));
			varauxpart = varbonesmatrix[varlegsy][varlocalx-1];
			if (varauxpart != null)
				varcurrentbounds.Encapsulate(varcurrentpart.InverseTransformPoint(varauxpart.position));
		}
		//if there's at least another spine bone, encapsulate it to assure there's at least an essential vertical profile for the spine
		if (varlocaly-1 >= 0)
			varauxpart = varbonesmatrix[varlocaly-1][varlocalx];
			if (varauxpart != null)			
				varcurrentbounds.Encapsulate(varcurrentpart.InverseTransformPoint(varauxpart.position));
		
		//extrapolate and alter the depth of the bounds, which is medically half of the width of the body
		//but only if the actual depth is inferior to that value
		//varcurrentsize = varcurrentpart.TransformPoint(varcurrentbounds.size)-varcurrentpart.position;
		varspinewidth = varcurrentbounds.size[mettransformdirectionaxis(varcurrentpart.InverseTransformDirection(((varcurrentpart.position + Vector3.right)-varcurrentpart.position).normalized))];//.x;
		varspineheight = varcurrentbounds.size[mettransformdirectionaxis(varcurrentpart.InverseTransformDirection(((varcurrentpart.position + Vector3.up)-varcurrentpart.position).normalized))];//.y;
		varspinedepth = varcurrentbounds.size[mettransformdirectionaxis(varcurrentpart.InverseTransformDirection(((varcurrentpart.position + Vector3.forward)-varcurrentpart.position).normalized))];//.z;
		if (Mathf.Abs(varspinedepth) < Mathf.Abs(varspineheight / 2)) {
			varspinedepth = varspineheight / 2;
		}
		//additional spine depth control in case there are no shoulders and thus the spine height can't be calculated instantly
		//torso depth is two thirds of the width
		if (Mathf.Abs(varspinedepth) < Mathf.Abs(varspinewidth / 1.5f)) {
			varspinedepth = varspinewidth / 1.5f;
		}
		
		//calculate the topmost bone matrix index for the spine limit		
		int varspinetop = vartopboney;		
		if (varshouldersy > 0 && varshouldersy - vartopboney > 0) {
			varspinetop = varshouldersy;
		}
		
		//scroll through the spine to add box colliders to each segment, respecting the exploration size
		varcurrentexplorationsize = 0;
		for (int varcounter = 0; varcounter < (varmatrixsize/2)-varspinetop; varcounter++) {
			varcurrentpart = varbonesmatrix[varlocaly-varcounter][varlocalx];
			metprint("Adding box collider to " + varcurrentpart,0);
			//create a new collider
			varnewcollider = varcurrentpart.gameObject.AddComponent<BoxCollider>();
			//add the physic material if it's set in the options
			if (varmaterial!=null)
				varnewcollider.material = varmaterial;
			//reset the bounds
			varcurrentbounds = new Bounds();
			//encapsulate the thighs when we're scrolling the source bone. this is necessary to actually attach the thigh
			//to the source bone in case the legs have a lower y
			if (varcounter == 0 && varlegsy>0) {
				float varleftlegy = 0;
				float varrightlegy = 0;
				//check thighs y coordinate to verify if they are at the same or lower height than the source bone
				if (varlocalx+1<varmatrixsize)
					if (varbonesmatrix[varbottomboney][varlocalx+1] != null)
						varleftlegy = varbonesmatrix[varbottomboney][varlocalx+1].position.y;
				if (varlocalx-1>=0 )
					if (varbonesmatrix[varbottomboney][varlocalx-1] != null)
						varrightlegy = varbonesmatrix[varbottomboney][varlocalx-1].position.y;
				if (varleftlegy > varrightlegy)
					varleftlegy = varrightlegy;
				//encapsulate the thighs thanks to the lowest of the two
				if (varcurrentpart.position.y > varleftlegy) {
					if (varlocalx+1 < varmatrixsize && varbonesmatrix[varbottomboney][varlocalx+1] != null)
						varcurrentbounds.Encapsulate(varcurrentpart.InverseTransformPoint(varbonesmatrix[varbottomboney][varlocalx+1].position));
					if (varlocalx-1 >= 0 && varbonesmatrix[varbottomboney][varlocalx-1] != null)
						varcurrentbounds.Encapsulate(varcurrentpart.InverseTransformPoint(varbonesmatrix[varbottomboney][varlocalx-1].position));
				}
			}
			
			//encapsulate the current spine node
			varcurrentbounds.Encapsulate(varcurrentpart.InverseTransformPoint(varcurrentpart.position));
			//encapsulate the node higher in the spine
			varcurrentbounds.Encapsulate(varcurrentpart.InverseTransformPoint(varbonesmatrix[varlocaly-varcounter-1][varlocalx].position));
			//retrieve the current size of the new bounds
			//1.8 deprecated varcurrentsize = varcurrentpart.TransformPoint(varcurrentbounds.size)-varcurrentpart.position;
			//since the body parts can be rotated in different ways, we create a global reference where the standard axes represent
			//depth, width and height, to override the sizes of the current spine node collider
			
			//determine the forward axis
			//Vector3 varcurrentpoint = varcurrentpart.position + Vector3.forward;
			//int varcurrentaxis = mettransformdirectionaxis(varcurrentpart.InverseTransformDirection((varcurrentpoint-varcurrentpart.position).normalized));
			Vector3 varnewsize = new Vector3();
			//ori varnewsize[varcurrentaxis] = varspinedepth;
			//1.8
			varnewsize[mettransformdirectionaxis(varcurrentpart.InverseTransformDirection(((varcurrentpart.position + Vector3.forward)-varcurrentpart.position).normalized))] = varspinedepth;
			
			//determine the right axis
			//varcurrentpoint = varcurrentpart.position + Vector3.right;
			//varcurrentaxis = mettransformdirectionaxis(varcurrentpart.InverseTransformDirection((varcurrentpoint-varcurrentpart.position).normalized));
			//ori varnewsize[varcurrentaxis] = varspinewidth;
			//1.8
			int varcurrentaxis = mettransformdirectionaxis(varcurrentpart.InverseTransformDirection(((varcurrentpart.position + Vector3.right)-varcurrentpart.position).normalized));
			varnewsize[varcurrentaxis] = varspinewidth;
			//shoulder fixes
			if (varcounter == (varmatrixsize/2)-varshouldersy-1) {
				//we enforce the 'fit shoulders' option and assure that the shoulder spine collider does not intersect them
				if (varfitshoulders==false) {
					if (vararmlstart != null && vararmrstart != null) {
						if (Mathf.Abs(vararmlstart.position.x-vararmrstart.position.x)<varspinewidth) {
							varnewsize[varcurrentaxis] = Mathf.Abs(vararmlstart.position.x-vararmrstart.position.x);
						}
					}
				}
				else {
					//we absorb shoulder bones until the arms start outside the hips
					//clearly can only be done if the legs exist
					vartemparmlstart = null;
					vartemparmrstart = null;
					int vartempy = varshouldersy;
					int vartempx = (varmatrixsize/2)+1;
					if (varleglstart != null) {
						while (vartempx<varmatrixsize && varbonesmatrix[vartempy][vartempx] != null && vartemparmlstart == null) {
							if (varleglstart.position.x-varbonesmatrix[vartempy][vartempx].position.x>=0) {
								vartemparmlstart = varbonesmatrix[vartempy][vartempx];
							}
							vartempx++;
						}
					}
					vartempx = (varmatrixsize/2)-1;
					if (varlegrstart != null) {
						while (vartempx>=0 && varbonesmatrix[vartempy][vartempx] != null && vartemparmrstart == null) {
							if (varlegrstart.position.x-varbonesmatrix[vartempy][vartempx].position.x<=0) {
								vartemparmrstart = varbonesmatrix[vartempy][vartempx];
							}
							vartempx--;
						}
					}
					//since we can have asymmetric shoulder balancement, we use encapsulation to correctly offset the center
					if (vartemparmlstart != null) {
						varcurrentbounds.Encapsulate(varcurrentpart.InverseTransformPoint(vartemparmlstart.position));
					}
					if (vartemparmrstart != null) {
						varcurrentbounds.Encapsulate(varcurrentpart.InverseTransformPoint(vartemparmrstart.position));
					}
					varnewsize[varcurrentaxis] = varcurrentbounds.size[mettransformdirectionaxis(varcurrentpart.InverseTransformDirection(((varcurrentpart.position + Vector3.right)-varcurrentpart.position).normalized))];
				}
			}
						
			//determine the up axis
			//varcurrentpoint = varcurrentpart.position + Vector3.up;
			//varcurrentaxis = mettransformdirectionaxis(varcurrentpart.InverseTransformDirection((varcurrentpoint-varcurrentpart.position).normalized));
			//ori varnewsize[varcurrentaxis] = varcurrentsize[mettransformdirectionaxis(varcurrentpart.InverseTransformDirection(((varcurrentpart.position + varcurrentpart.up)-varcurrentpart.position).normalized))];
			//1.8
			varnewsize[mettransformdirectionaxis(varcurrentpart.InverseTransformDirection(((varcurrentpart.position + Vector3.up)-varcurrentpart.position).normalized))] = varcurrentbounds.size[mettransformdirectionaxis(varcurrentpart.InverseTransformDirection(((varcurrentpart.position + Vector3.up)-varcurrentpart.position).normalized))];
			//assign the transformed size to the new collider size
			varnewcollider.size = varnewsize;			
			//assign the collider center
			varnewcollider.center = varcurrentbounds.center;
			varcurrentexplorationsize++;
			if (varcurrentexplorationsize >= varexplorationsize && varexplorationsize != 0)
				break;
		}
		
		Vector3 vardepthfixer = new Vector3();
		//add the head collider. if there's no shoulders we place no head though
		Transform varheadbone = null;
		if (varshouldersy>0 && (varbottomboney-varshouldersy < varexplorationsize || varexplorationsize == 0)) {
			//there's at least one head bone above the shoulders
			metprint("Number of head bones: " + (varshouldersy-vartopboney),0);
			if (varshouldersy-vartopboney>0) {
				//set up the head starting coordinates
				//1.8 we start encapsulating from the neck up to avoid neck inconsistencies
				varlocaly = varshouldersy;
				varlocalx = varmatrixsize/2;
				varheadbone = varbonesmatrix[varlocaly][varlocalx];
				//encapsulate all the head bones to properly set the center and size of the head
				//start with the first node, which is known to exist
				varcurrentbounds = new Bounds();
				varcurrentpart = varheadbone;
				varcurrentbounds.Encapsulate(varheadbone.InverseTransformPoint(varheadbone.position));
				
				//we've set the starting point for the shoulder and now proceed to encapsulate all available head bones
				//of which there's at least one
				varlocaly--;
				float varminimumradius = 0;
				while (varlocaly >=0 && varbonesmatrix[varlocaly][varlocalx] != null) {
					varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
					//encapsulation and minimum radius calculation
					varcurrentbounds.Encapsulate(varheadbone.InverseTransformPoint(varcurrentpart.position));
					varminimumradius = Mathf.Abs(varheadbone.position.y-varcurrentpart.position.y);
					//we release this bone since the actual physics will be set up in the head bone and this bone is used for center
					//and size of the bounds
					metreleaseextremity(varcurrentpart);
					varlocaly--;
				}
				//we eventually add the collider, which will be one which encapsulates all head bones, and set center and radius for it
				SphereCollider varheadcollider = varheadbone.gameObject.AddComponent<SphereCollider>();
				//add the physic material if it's set in the options
				if (varmaterial!=null)
					varnewcollider.material = varmaterial;
				varheadcollider.center = varheadbone.InverseTransformPoint(varcurrentpart.position);
				varheadcollider.radius = varcurrentbounds.size[mettransformdirectionaxis(varcurrentbounds.size)]/2;
				//we use the minimum radius to determine if the topmost bone of the head is the skull top, or an alternate spot inside
				//the head (for example the nose or the mouth), and use that value to correct the radius in those cases
				if (varheadcollider.radius < varminimumradius)
					varheadcollider.radius = varminimumradius;
			}
		}
		
		Rigidbody varjointfixer = null;
		bool varfoundfixedarmstart = false;
		//add the arms colliders if there's at least one arm
		if (varshouldersy>=0) {
			//left arm
			varlocaly = varshouldersy;
			varlocalx = (varmatrixsize/2)+1;
			//scroll the arm to its maximum extent
			varcurrentexplorationsize = varbottomboney-varshouldersy;
			while (varlocalx+1 < varmatrixsize && varbonesmatrix[varlocaly][varlocalx+1] != null && (varcurrentexplorationsize < varexplorationsize || varexplorationsize == 0)) {
				//get the current arm part
				varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
				//an arm's depth is medically wide one half of the torso's width
				vardepthfixer = Vector3.forward*(varspinedepth/2);
				vardepthfixer = varcurrentpart.InverseTransformPoint(vardepthfixer+varcurrentpart.position);
				//check if the current part is the fixed arm part after the shoulder skip
				if (varfoundfixedarmstart == false && (varcurrentpart == vartemparmlstart || vartemparmlstart == null))
					varfoundfixedarmstart = true;
				//retrieve the anchor of the first arm part to use it in case multiple arm nodes are skipped because of the shoulder fix
				if (varjointfixer != null) {
					if (varcurrentpart.GetComponent<CharacterJoint>() != null)
						varcurrentpart.GetComponent<CharacterJoint>().connectedBody = varjointfixer;
					varjointfixer = null;
				}
				//add the collider only if the arm start has been found
				if (varfoundfixedarmstart) {
					metaddcapsulecollidertolimbpart(varcurrentpart, varbonesmatrix[varlocaly][varlocalx+1],Mathf.Abs(vardepthfixer[mettransformdirectionaxis(vardepthfixer)]/2));
				}
				else {
					//otherwise release the extremity
					if (varcurrentpart.GetComponent<CharacterJoint>() != null && varjointfixer == null)
						varjointfixer = varcurrentpart.GetComponent<CharacterJoint>().connectedBody;
					metreleaseextremity(varcurrentpart);
				}
				varlocalx++;
				varcurrentexplorationsize++;
			}
			//release the extremity since it's the last one in the limb chain
			metreleaseextremity(varbonesmatrix[varlocaly][varlocalx]);
			//right arm. same logic as the left.
			varlocaly = varshouldersy;
			varlocalx = (varmatrixsize/2)-1;
			varfoundfixedarmstart = false;
			varjointfixer = null;
			varcurrentexplorationsize = varbottomboney-varshouldersy;
			while (varlocalx-1 >= 0 && varbonesmatrix[varlocaly][varlocalx-1] != null && (varcurrentexplorationsize < varexplorationsize || varexplorationsize == 0)) {
				varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
				vardepthfixer = Vector3.forward*(varspinedepth/2);
				vardepthfixer = varcurrentpart.InverseTransformPoint(vardepthfixer+varcurrentpart.position);
				if (varfoundfixedarmstart == false && (varcurrentpart == vartemparmrstart || vartemparmrstart == null))
					varfoundfixedarmstart = true;
				if (varjointfixer != null) {
					if (varcurrentpart.GetComponent<CharacterJoint>() != null)
						varcurrentpart.GetComponent<CharacterJoint>().connectedBody = varjointfixer;
					varjointfixer = null;
				}
				if (varfoundfixedarmstart) {
					metaddcapsulecollidertolimbpart(varcurrentpart, varbonesmatrix[varlocaly][varlocalx-1],Mathf.Abs(vardepthfixer[mettransformdirectionaxis(vardepthfixer)]/2));
				}
				else {
					if (varcurrentpart.GetComponent<CharacterJoint>() != null && varjointfixer == null)
						varjointfixer = varcurrentpart.GetComponent<CharacterJoint>().connectedBody;
					metreleaseextremity(varcurrentpart);
				}
				varlocalx--;
				varcurrentexplorationsize++;
			}
			metreleaseextremity(varbonesmatrix[varlocaly][varlocalx]);
		}
		
		//add the legs colliders. same logic as the arms.
		if (varlegsy>=0) {
			//left leg
			varlocaly = varbottomboney;
			varlocalx = (varmatrixsize/2)+1;
			//we preload the exploration size to 1 since the legs are always supposed to be attached to the spine
			varcurrentexplorationsize = 1;
			while (varlocaly+1 < varmatrixsize && varbonesmatrix[varlocaly+1][varlocalx] != null && (varcurrentexplorationsize < varexplorationsize || varexplorationsize == 0)) {
				varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
				vardepthfixer = Vector3.forward*(varspinedepth/1.8f);
				vardepthfixer = varcurrentpart.InverseTransformPoint(vardepthfixer+varcurrentpart.position);
				metaddcapsulecollidertolimbpart(varcurrentpart, varbonesmatrix[varlocaly+1][varlocalx],Mathf.Abs(vardepthfixer[mettransformdirectionaxis(vardepthfixer)]/1.8f));
				varlocaly++;
				varcurrentexplorationsize++;
			}
			metreleaseextremity(varbonesmatrix[varlocaly][varlocalx]);
			//right leg follows the same logic as the left one
			varlocaly = varbottomboney;
			varlocalx = (varmatrixsize/2)-1;
			varcurrentexplorationsize = 1;
			while (varlocaly+1 < varmatrixsize && varbonesmatrix[varlocaly+1][varlocalx] != null && (varcurrentexplorationsize < varexplorationsize || varexplorationsize == 0)) {
				varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
				vardepthfixer = Vector3.forward*(varspinedepth/1.8f);
				vardepthfixer = varcurrentpart.InverseTransformPoint(vardepthfixer+varcurrentpart.position);
				metaddcapsulecollidertolimbpart(varcurrentpart, varbonesmatrix[varlocaly+1][varlocalx],Mathf.Abs(vardepthfixer[mettransformdirectionaxis(vardepthfixer)]/1.8f));
				varlocaly++;
				varcurrentexplorationsize++;
			}
			metreleaseextremity(varbonesmatrix[varlocaly][varlocalx]);
		}
	}

	/// <summary>
	/// this is the procedure responsible for the creation of the URG entity structure in the source
	/// gameobject.
	/// After execution the source's root will host the clsurgent class, which receives a call for each
	/// event handled in clsurgentactuator, which is the class that gets installed in all collider-bound
	/// body parts by this same procedure
	/// </summary>
	/// <param name="varpsource">
	/// The source object, which will be used as reference
	/// </param>
	private void meturgentmanager(Transform varpsource) {
		//iteration helpers
		int varlocaly, varlocalx, varparts, varpartindex, varxdirection, varydirection;
		clsurgutils.enumparttypes varcurrentparttype;
		//reference cache
		clsurgent varcurrenturgentity;
		clsurgentactuator varcurrentnode;
		Transform varcurrentpart;
		CharacterJoint varauxjoint;
		//auxilliary part holders
		SortedList<int,Transform> varhead, varspine, vararmleft, vararmright, varlegleft, varlegright, varcurrentlist;
		
		//init the body parts list to empty
		varhead = new SortedList<int,Transform>();
		varspine = new SortedList<int,Transform>();
		vararmleft = new SortedList<int,Transform>();
		vararmright = new SortedList<int,Transform>();
		varlegleft = new SortedList<int,Transform>();
		varlegright = new SortedList<int,Transform>();
		
		//init the helpers
		varparts = varpartindex = varlocaly = varlocalx = 0;
		varcurrentlist = null;
		varxdirection = varydirection = -1;
		varcurrentparttype = clsurgutils.enumparttypes.spine;
		//we iterate only if the URG entities option is checked
		if (varurgentities) {
			//for a start, an URGent manager is added to the root, if necessary
			varcurrenturgentity = varsource.root.GetComponent<clsurgent>();
			if (varcurrenturgentity == null)
				varcurrenturgentity = varsource.root.gameObject.AddComponent<clsurgent>();
			//fake cycle for optimization
			for (int variteration = 0; variteration < 6; variteration++) {
				varcurrentlist = new SortedList<int, Transform>();
				//we use the iteration to preset the body part values for indexes and lists
				switch (variteration) {
					case 0: //spine nodes
						varlocaly = varlocalx = varmatrixsize/2;
						varydirection = -1;
						varxdirection = 0;
						varcurrentlist = varspine;
						varcurrentparttype = clsurgutils.enumparttypes.spine;
						break;
					case 1: //head nodes
						varlocaly = varshouldersy;
						varlocalx = varmatrixsize/2;
						varydirection = -1;
						varxdirection = 0;
						varcurrentlist = varhead;
						varcurrentparttype = clsurgutils.enumparttypes.head;
						break;
					case 2: //left arm nodes
						varlocaly = varshouldersy;
						varlocalx = (varmatrixsize/2)+1;
						varydirection = 0;
						varxdirection = +1;
						varcurrentlist = vararmleft;
						varcurrentparttype = clsurgutils.enumparttypes.arm_left;
						break;
					case 3: //right arm nodes
						varlocaly = varshouldersy;
						varlocalx = (varmatrixsize/2)-1;
						varydirection = 0;
						varxdirection = -1;
						varcurrentlist = vararmright;
						varcurrentparttype = clsurgutils.enumparttypes.arm_right;
						break;
					case 4: //left leg nodes
						varlocaly = varlegsy;
						varlocalx = (varmatrixsize/2)+1;
						varydirection = +1;
						varxdirection = 0;
						varcurrentlist = varlegleft;
						varcurrentparttype = clsurgutils.enumparttypes.leg_left;
						break;
					case 5:  //right leg nodes
						varlocaly = varlegsy;
						varlocalx = (varmatrixsize/2)-1;
						varydirection = +1;
						varxdirection = 0;
						varcurrentlist = varlegright;
						varcurrentparttype = clsurgutils.enumparttypes.leg_right;
						break;
				}
				metprint("Iteration " + variteration,0);
				varpartindex = 0;
				//scroll the extension in all its length
				while (varlocaly >= 0 && varlocaly < varmatrixsize && varlocalx >=0 && varlocalx < varmatrixsize && varbonesmatrix[varlocaly][varlocalx] != null &&
				      ((variteration == 0 && varlocaly > varshouldersy) || variteration>0)) {
					//cache the current part
					varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
					metprint("Part " + varcurrentpart,0);
					//add the part to the urgent manager only if there's a collider in it (necessary to avoid false managing in case of partial ragdolls)
					if (varcurrentpart.GetComponent<Collider>() != null) {
						//there's a collider, so we add the part to the current list
						varcurrentlist.Add(varparts, varcurrentpart);
						//fetch or create a new urgent actuator
						varcurrentnode = varcurrentpart.GetComponent<clsurgentactuator>();
						if (varcurrentnode== null)
							varcurrentnode = varcurrentpart.gameObject.AddComponent<clsurgentactuator>();
						//assign a reference to the urg manager for the node
						//this is useful for functionality expansion since it allows direct access to manager functions
						varcurrentnode.vargamurgentsource = varcurrenturgentity;
						//we now check the current part joint to find its rigidbody parent
						varauxjoint = varcurrentpart.GetComponent<CharacterJoint>();
						if (varauxjoint != null) //should never happen
							if (varauxjoint.connectedBody != null) //should never happen
								varcurrentnode.vargamsource = varauxjoint.connectedBody.transform;
							else
								varcurrentnode.vargamsource = varpsource;
						else
							varcurrentnode.vargamsource = varpsource;
						//set the body part type
						varcurrentnode.vargamparttype = varcurrentparttype;
						//set the part name in the instance
						varcurrentnode.vargampartinstancename = varcurrentnode.name;
						//set the part index in the instance
						varcurrentnode.vargampartindex = varpartindex;
						varpartindex++;
						varparts++;
					}
					//'move' the direction pointer to keep scrolling
					varlocalx += varxdirection;
					varlocaly += varydirection;
				}
			}
			metprint("Parts " + varparts,0);
			//we now instantiate the singular body extensions, to copy the current exploration into the 
			//URG entities manager and create the persistent references to the ragdolled armature
			varcurrenturgentity.vargamnodes.vargamspine = new Transform[varspine.Count];
			varspine.Values.CopyTo(varcurrenturgentity.vargamnodes.vargamspine,0);
			varcurrenturgentity.vargamnodes.vargamhead = new Transform[varhead.Count];
			varhead.Values.CopyTo(varcurrenturgentity.vargamnodes.vargamhead,0);

			varcurrenturgentity.vargamnodes.vargamarmleft = new Transform[vararmleft.Count];
			vararmleft.Values.CopyTo(varcurrenturgentity.vargamnodes.vargamarmleft,0);
			varcurrenturgentity.vargamnodes.vargamarmright = new Transform[vararmright.Count];
			vararmright.Values.CopyTo(varcurrenturgentity.vargamnodes.vargamarmright,0);

			varcurrenturgentity.vargamnodes.vargamlegleft = new Transform[varlegleft.Count];
			varlegleft.Values.CopyTo(varcurrenturgentity.vargamnodes.vargamlegleft,0);
			varcurrenturgentity.vargamnodes.vargamlegright = new Transform[varlegright.Count];
			varlegright.Values.CopyTo(varcurrenturgentity.vargamnodes.vargamlegright,0);
		}
	}
	
	/// <summary>
	/// Destroys the joint and rigidbody of varpextremity. Used in extremities that won't have a collider set up for them
	/// </summary>
	/// <param name="varpextremity">
	/// The bone that will have its joint and rigidbody destroyed
	/// </param>
	private static void metreleaseextremity(Transform varpextremity) {
		//added null check failsafes for awkward and broken models
		if (varpextremity != null) {
			if (varpextremity.GetComponent<CharacterJoint>() != null)
				DestroyImmediate(varpextremity.GetComponent<CharacterJoint>());
			if (varpextremity.GetComponent<Rigidbody>() != null)
				DestroyImmediate(varpextremity.GetComponent<Rigidbody>());
		}
	}

	/// <summary>
	/// The recursive function reassigns the specified source to match the first good candidate for holding the 'rig' of the model
	/// The rig is the armature root, which is the parent of all transform bones
	/// </summary>
	/// <param name="varpsource">
	/// Current source for the ragdoll armature
	/// </param>
	/// <param name="varpskmnotfound">
	/// Specifies if the skinned mesh rendered has been found or not. Will be used to override its search, and look for the first sub bone with at least a sub bone.
	/// </param>
	/// <returns>
	/// returns true if a valid source candidate was found. false otherwise.
	/// </returns>
	private bool metreassignsource(Transform varpsource, bool varpskmnotfound) {
		bool varfound = false;
		metprint("Scanning " + varpsource.name,0);
		if (!varpskmnotfound) {
			if (varpsource.GetComponent<SkinnedMeshRenderer>() != null) {
				metprint("Found skinned mesh renderer",0);
				varfound = true;
				varsource = varpsource;
			}
			else {
				foreach (Transform varcurrentchildtransform in varpsource) {
					varfound =  metreassignsource(varcurrentchildtransform, varpskmnotfound);
					if (varfound)
						break;
				}
			}
		}
		else {
			//0927 1109 changed from varpsource.childCount >= 3 to avoid root traps in bad models
			if (varpsource.childCount > 0 && varpsource != varpsource.root) {
				metprint("Found the first bone with 3 or more sub bones.",0);
				varfound = true;
				varsource = varpsource;
			}
			else {
				foreach (Transform varcurrentchildtransform in varpsource) {
					varfound =  metreassignsource(varcurrentchildtransform, varpskmnotfound);
					if (varfound) {
						break;
					}
				}
			}
		}
		return varfound;
	}
	
	
	/// <summary>
	/// This function adds an emergency root collider to which connect the extremities, in case the 'Preserve limbs link to root' option is enabled
	/// </summary>
	private void metfixorphanedbodyparts() {
		//if the option to preserve links is disabled, we skip the routine
		if (varpreservelimbslinktoroot == false) {
			return;
		}
		//auxilliary navigation variables
		int varlocaly, varlocalx;
		//limb and physics caching
		Transform varroot;
		Rigidbody varrootrigidbody;
		
		varlocaly = (varmatrixsize/2);
		varlocalx = varlocaly;
		metprint("Checking limbs connections and adding a root rigidbody if needed",0);
		
		//cache the root
		varroot = varbonesmatrix[varlocaly][varlocalx];
		//get the root rigidbody
		varrootrigidbody = varroot.GetComponent<Rigidbody>();
		//there's no root rigidbody so we store one anew, and set it as inert to avoid overriding a possible spine exclusion from the ragdoll
		//we don't set its mass since it won't actually behave as a physics object but only as an anchor
		if (varrootrigidbody == null) {
			varrootrigidbody = varroot.gameObject.AddComponent<Rigidbody>();
			varrootrigidbody.isKinematic = true;
			varrootrigidbody.useGravity = false;
		}
		//1.6a failsafe version fixes missing joint links in abnormal children bones that loop back to the spine
		CharacterJoint[] varjoints = varroot.GetComponentsInChildren<CharacterJoint>();
		foreach (CharacterJoint varcurrentjoint in varjoints ) {
			if (varcurrentjoint.connectedBody == null) {
				//the joint's connected body is null so we replace it with our current rigidbody
				metprint("Fixed an orphaned body part " + varcurrentjoint.name,1);
				varcurrentjoint.connectedBody = varrootrigidbody;
			}
		}
	}
	
	/// <summary>
	/// Checks the limbs to ragdoll from the options setup, and destroys the physics accordingly
	/// </summary>
	private void metenforcelimbexploration() {
		//bones matrix exploration indexes
		int varcounter;
		int varlocaly, varlocalx;
		//auxilliary body part
		Transform varcurrentpart;

		//init the indexes to the root and to the shoulders, or top bone
		if (varshouldersy > vartopboney)
			varlocaly = varshouldersy;
		else
			varlocaly = vartopboney;
		varlocalx = varmatrixsize/2;
		if (varexplorespine == false) {
			metprint("Enforcing exploration: purging spine",0);
			//We can't tell if the physics here are the original ones or our own, regardless of 'destroy physics' option.
			//We however need to proceed and destroy the physics we would have created
			for (varcounter = varmatrixsize/2; varcounter > varlocaly; varcounter--) {
				varcurrentpart = varbonesmatrix[varcounter][varlocalx];
				metreleaseextremity(varcurrentpart);
				if (varcurrentpart.GetComponent<BoxCollider>() != null)
					DestroyImmediate(varcurrentpart.GetComponent<BoxCollider>());
			}
		}
		//reset the indexes
		varlocaly = varmatrixsize/2;
		varlocalx = varlocaly;
		//destroy physics from the 'neck' up
		if (varexplorehead == false) {
			//enforce the head
			metprint("Enforcing exploration: purging head",0);
			for (varcounter = varshouldersy; varcounter >= vartopboney; varcounter--) {
				varcurrentpart = varbonesmatrix[varcounter][varlocalx];
				metreleaseextremity(varcurrentpart);
				if (varcurrentpart.GetComponent<SphereCollider>() != null)
					DestroyImmediate(varcurrentpart.GetComponent<SphereCollider>());
			}
		}
		//cycle trick to avoid a procedure
		//in order, the cycle stands for 0 = left arm, 1 = right arm, 2 = left leg, 3 = right leg
		for (varcounter = 0; varcounter < 4; varcounter++) {
			//skip the extremity if it's included in the exploration
			if (varcounter == 0 && !varexplorearmleft || varcounter == 1 && !varexplorearmright || varcounter == 2 && !varexplorelegleft || varcounter == 3 && !varexplorelegright) {
				//determine starting coordinates of the matrix
				if (varcounter == 0 || varcounter == 2)
					varlocalx = (varmatrixsize/2)+1;
				else
					varlocalx = (varmatrixsize/2)-1;
				if (varcounter == 0 || varcounter == 1)
					varlocaly = varshouldersy;
				else
					varlocaly = varlegsy;
				//move along the limb's extension
				while (varlocalx>=0 && varlocalx<varmatrixsize && varlocaly >=0 && varlocaly < varmatrixsize && varbonesmatrix[varlocaly][varlocalx] != null) {
					//store the current part in temp variable
					varcurrentpart = varbonesmatrix[varlocaly][varlocalx];
					metprint("Enforcing exploration: purging " + varcurrentpart.name,0);
					metreleaseextremity(varcurrentpart);
					if (varcurrentpart.GetComponent<CapsuleCollider>() != null)
						DestroyImmediate(varcurrentpart.GetComponent<CapsuleCollider>());
					//check the cycle to determine what coordinate to move to proceed along the limb
					//left arm, x coordinate increase
					if (varcounter == 0)
						varlocalx++;
					//right arm, x coordinate decrease
					if (varcounter == 1)
						varlocalx--;
					//legs, y coordinate increase
					if (varcounter == 2 || varcounter == 3)
						varlocaly++;
				}
			}			
		}
	}
	

	/// <summary>
	/// The core ragdoll creation function. Will call all the necessary subroutines to go from model to ragdoll.
	/// </summary>
	private void metcreateragdoll() {
		//local value of the exploration size parameter
		varcurrentexplorationsize = varexplorationsize;
		//minimum hardcoded matrix size check
		if (varmatrixsize < 17)
			varmatrixsize = 17;
		//start the creation if there's a target
		if (varsource != null) {
			//basic check for limb exploration
			if (!varexplorehead && !varexplorespine && !varexplorearmleft && !varexplorearmright && !varexplorelegleft && !varexplorelegright) {
				metprint("There's no limbs to explore. Assure that at least one limb is ticked in Options- Limbs to ragdoll",1);
				return;
			}
			vartopboney = varmatrixsize;
			varbottomboney = 0;
			varshouldersy = -1;
			varlegsy = -1;
			
			//search for a valid ragdoll candidate
			if (varsource.GetComponent<SkinnedMeshRenderer>() == null) {
				if (varautomaticallysearchskinnedmeshrenderer) {
					if (varsource != varsource.root) {
						metprint("Since the specified source object is different than its root, automatic search of the root has been disabled for this iteration.",1);
					}
					else {
						//reassign the source since the specified source equals its root (and there's no game model which is rigged from the root)
						if (!metreassignsource(varsource, false)) {
							if (!metreassignsource(varsource.root, true)) {
								metprint("URG! could not find a skinned mesh renderer in the subtree of the chosen object.\nYou will need to identify the root bone of the model to continue,\nAdditionally, notice that 'Automatically search for skinnedmeshrenderer' option is now turned off.",2);
								varautomaticallysearchskinnedmeshrenderer = false;
								return;
							}
						}
						else {
							//the source is not equal to its root, so check if there's at least one sub bone
							if (varsource.childCount == 0) {
								metprint("URG! automatically found a skinnedmeshrenderer component, but it's bone has no children.\nUsing secondary search method (less accurate).",1);
								//since there were no sub bones in the specified source, we try to find the first candidate for a good source
								if (!metreassignsource(varsource.root, true)) {
									metprint("The secondary root bone search yielded no results. Disabling automatic search for skinnedmeshrenderer.\nYou will need to identify the root bone of the model to continue.",2);
									varautomaticallysearchskinnedmeshrenderer = false;
									return;
								}
							}
							else 
								metprint("Source reassigned to child " + varsource.name,1);
						}
					}
				}
				else {
					metprint("No skinned mesh renderer component in the selected source.\nThis could mean that the model is not suitable for being transformed into ragdoll.",1);
				}
			}
			
			//this cycle represents a fix for the 'microphone' occurence, where the armature is located at y-0 coordinate, and is wrongly identified
			//as the armature center
			int varmaxiterations = 2;
			string varbonesreport;
			Transform varoriginalsource = null;
			bool varrestoreoriginalsource = false;
			while (varmaxiterations>0) {
				vartopboney = varmatrixsize;				
				//init the bones matrix
				varbonesmatrix = new Transform[varmatrixsize][];
				for (int varcounter = 0; varcounter < varbonesmatrix.Length; varcounter++) {
					varbonesmatrix[varcounter] = new Transform[varmatrixsize];
				}
				//if 'clear physics' option is checked, this call will destroy all existing physics objects from the source onwards
				if (varclearphysics) {
					metclearphysics(varsource);
				}
				//set the matrix exploration report variable
				varbonesreport = "";
				//store the spine
				varcoordy = varmatrixsize/2;
				varcoordx = varcoordy;
				if (metstorecardinaldirectioninbonesmatrix(enumcardinaldirections.dirup,varsource, varsource, false, true, true) == 0) {
					varmatrixsize*=2;
					metprint("The armature could not be explored. Bone matrix size has been doubled. Please try to create the ragdoll again now",2);
					return;
				}
				//print the spine exploration if the 'verbose' option is active (through the call to 'metprint')
				for (int varcounter = 0; varcounter < varbonesmatrix.Length; varcounter++) {
					for (int varaux = 0; varaux < varbonesmatrix[varcounter].Length; varaux++) {
						if (varbonesmatrix[varcounter][varaux] == null) {
							varbonesreport += " _ ";
						}
						else {
							varbonesreport += " o ";
						}
					}
					varbonesreport += "\n";
				}
				metprint("Follows armature exploration map\n" + varbonesreport,0);
				//find the extremities
				metstarfinder(varsource,varsource,"");
				//print the extremities search result
				metprint("Extremities search result:\nhead " + (varheadstart != null ? varheadstart.name : "") + "\narm l " + (vararmlstart != null ? vararmlstart.name : "")+ "\narm r " + (vararmrstart != null ? vararmrstart.name : "" ) + "\nleg l " + (varleglstart != null ? varleglstart.name : "") + "\nleg r " + (varlegrstart != null ? varlegrstart.name : ""),1);
				//find the origins of the extremities
				metstarfindorigins();
				metprint("Origins search result:\nhead " + (varheadstart != null ? varheadstart.name : "") + "\narm l " + (vararmlstart != null ? vararmlstart.name : "")+ "\narm r " + (vararmrstart != null ? vararmrstart.name : "" ) + "\nleg l " + (varleglstart != null ? varleglstart.name : "") + "\nleg r " + (varlegrstart != null ? varlegrstart.name : ""),1);
				//try to fix the source if the legs aren't found in the first iteration
				//but only if we haven't found the legs, or the smr search is disabled
				if (varrestoreoriginalsource || (varleglstart != null || varlegrstart != null) || varautomaticallysearchskinnedmeshrenderer == false) {
					varmaxiterations = 0;
				}
				else {
					if (varoriginalsource == null)
						varoriginalsource = varsource;
					//get a new source candidate only if we can iterate again
					if (varmaxiterations>1) {
						metprint("Could not find legs with the current source candidate. Searching a new one.",1);
						if (varsource.childCount>0) {
							varsource = varsource.GetChild(0);
							Debug.LogError("new source " + varsource);
						}
						else {
							metprint("Could not find a source candidate that exposed the legs.",1);
							varmaxiterations = 0;
						}
					}
					else {
						//we restore one level of parentship (note that this is just enough to satisfy the hardcoded number of two iterations)
						if (varsource.parent != null) {
							metprint("Restoring the original source.",1);
							varsource = varoriginalsource;
							varrestoreoriginalsource = true;
							varmaxiterations++;
						}
					}
				}
				varmaxiterations--;
			}
			//store the limbs in the bones matrix
			metstorelimbinbonesmatrix(true, true);  //arm left
			metstorelimbinbonesmatrix(true, false);  //arm right
			metstorelimbinbonesmatrix(false, true);  //leg left
			metstorelimbinbonesmatrix(false, false);  //leg right
			varbonesreport = "";
			for (int varcounter = 0; varcounter < varbonesmatrix.Length; varcounter++) {
				for (int varaux = 0; varaux < varbonesmatrix[varcounter].Length; varaux++) {
					if (varbonesmatrix[varcounter][varaux] == null) {
						varbonesreport += " _ ";
					}
					else {
						varbonesreport += " o ";
					}
				}
				if (varcounter == varshouldersy )
					varbonesreport += "   /shoulders cross here";
				if (varcounter == varlegsy )
					varbonesreport += "   /legs cross here";
				varbonesreport += "\n";
			}
			metprint("Follows armature exploration map\n" + varbonesreport,1);
			//add physics elements to the bones matrix
			metaddbonephysics();
			//analyze the bones matrix adding colliders and setting physics parameters
			metnormalizebonesmatrix();
			
			//add colliders to the parts in the bones matrix to allow the ragdoll to interact with the world
			metnormalizecolliders();
			//set the rigidity of the joints (100 and 1 are hard coded sampling rates for elasticity and weight damping)
			metnormalizerigidity(varsource,100,1,vardampeningstrenght,varspringstrenght);
			//enforce the limb exploration limitations
			metenforcelimbexploration();
			//we assure that no joint remains orphaned if we've set the relative option
			metfixorphanedbodyparts();
			//call to URGent setup
			meturgentmanager(varsource);
			//lastly we make a call to the source linker.
			if (clsurgutils.metconnectbodies(varsource, varsourceconnector, true)>0)
				metprint("Successfully connected " + varsource.name + " with " + varsourceconnector.name,0);
			Debug.LogWarning("Ragdoll creation process completed.");
		}
		else {
			metprint("Please drag the root transform of the object you want to ragdoll in the relative slot.",2);
		}
	}
	
	/// <summary>
	/// This function creates two fake limbs to be used in linear ragdolls, to add depth (as in -collider width-)
	/// to the ragdoll, wich without shoulders or hips can only be created in two dimensions.
	/// NOTE: the extensions will be added in respect to "World", which means that the character must be ideally
	/// facing along the z axis for proper orientation.
	/// </summary>
	/// <param name="varptarget">
	/// The source to which the extensions will be added, one to the left, one to the right
	/// </param>
	/// <param name="varpdistance">
	/// The distance in units (1 unit = 1 meter) between the source and the lateral placement of the extensions
	/// </param>
	private void metaddfakelimbs(Transform varptarget, float varpdistance) {
		//helper variables
		GameObject varcurrentlimb = null;
		string varsuffix = "";
		Vector3 varvector = new Vector3();
		float varyfreeze = 0;
		
		//ignore calls for null targets
		if (varptarget != null) {
			//iterate the limbs
			for (int varcounter = 0; varcounter <2; varcounter++) {
				//left
				if (varcounter == 0) {
					varsuffix = confakelimbsuffixleft;
					varvector = -Vector3.right;
				}
				//right
				if (varcounter == 1) {
					varsuffix = confakelimbsuffixright;
					varvector = Vector3.right;
				}
				varvector.y = -varheightwindow;
				//search the limb
				varcurrentlimb = null;
				if (varptarget.FindChild(varptarget.name + varsuffix) == null)
					varcurrentlimb = new GameObject(varptarget.name + varsuffix);
				else
					varcurrentlimb = varptarget.FindChild(varptarget.name + varsuffix).gameObject;
				varcurrentlimb.transform.position = Vector3.zero;
				//perform the setup with failsafe check
				if (varcurrentlimb != null) {
					varcurrentlimb.transform.position = varptarget.transform.position + varcurrentlimb.transform.InverseTransformPoint(varvector * varpdistance);
					//store the left limb position to avoid asymmetries while creating a linear, vertical ragdoll
					if (varcounter == 0)
						varyfreeze = varcurrentlimb.transform.position.y;
					//replicate the y position for the right limb to assure simmetry
					if (varcounter == 1) {
						varvector = varcurrentlimb.transform.position;
						varvector.y = varyfreeze;
						varcurrentlimb.transform.position = varvector;
					}
					
					varcurrentlimb.transform.parent = varptarget;
				}
			}
			metprint("Limbs set",0);
		}
	}
	
	/// <summary>
	/// Searches for two specific children of the varptarget, and destroys them without confirmation
	/// NOTE: this function performs no check, since it should be only called in tandem with metaddfakelimbs
	/// </summary>
	/// <param name="varptarget">
	/// The source which supposedly holds the previously added fake limbs through metaddfakelimbs
	/// </param>
	private void metremovefakelimbs(Transform varptarget) {
		Transform varcurrentlimb = null;
		
		if ( varptarget!= null) {
			varcurrentlimb = varptarget.FindChild(varptarget.name + confakelimbsuffixleft);
			if (varcurrentlimb != null) {
				DestroyImmediate(varcurrentlimb.gameObject);
				metprint("Removed left extension",0);
			}
			varcurrentlimb = varptarget.FindChild(varptarget.name + confakelimbsuffixright);
			if (varcurrentlimb != null) {
				DestroyImmediate(varcurrentlimb.gameObject);
				metprint("Removed right extension",0);
			}
		}
	}
	
    void OnGUI () {
        GUILayout.Label ("THE ARC GAMES STUDIO");
		GUILayout.Label ("For basic humanoid rigs\n(optimal results with models t-posed,symmetrical along the up axis,\n1 head, 1 spine, 2 arms and 2 legs)", EditorStyles.boldLabel);
		GUILayout.Label ("Refer to the README file for an explanation of the options available.", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Drag the object to ragdoll from the scene,\nor the prefab from the project folder:");
		GUILayout.FlexibleSpace();
		varsource = EditorGUILayout.ObjectField(varsource,typeof(Transform),true, GUILayout.Width(150)) as Transform;
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Link this ragdoll to an object:");
		GUILayout.FlexibleSpace();
		varsourceconnector = EditorGUILayout.ObjectField(varsourceconnector,typeof(Transform),true, GUILayout.Width(150)) as Transform;
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Total mass in Kg (important to set\nto a realistical volume-weight value):");
		GUILayout.FlexibleSpace();
		vartotalmass = EditorGUILayout.FloatField(vartotalmass, GUILayout.Width(50));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		vardisplayoptions = EditorGUILayout.Foldout(vardisplayoptions, "Options (fine-tuning)");
		GUILayout.EndHorizontal();
		varmainscroll = EditorGUILayout.BeginScrollView(varmainscroll, GUILayout.Width (450));
		GUILayout.BeginVertical();
		if(vardisplayoptions) {
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Physic material (must be in a Resource folder):");
			GUILayout.FlexibleSpace();
			varmaterial = EditorGUILayout.ObjectField(varmaterial,typeof(PhysicMaterial),true, GUILayout.Width(150)) as PhysicMaterial;
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Add URG entity scripts to body parts (default checked):");
			GUILayout.FlexibleSpace();
			varurgentities = GUILayout.Toggle(varurgentities,"");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add fake limbs"))
				metaddfakelimbs(varsource, varfakelimbdistance);
			GUILayout.Label("Distance:");
			varfakelimbdistance = EditorGUILayout.FloatField(varfakelimbdistance,GUILayout.Width(50));
			//by default, if we specify a fake limb distance inferior to the horizontal tolerance, we set the horizontal
			//tolerance accordingly
			if (varfakelimbdistance < varwidthwindow && varfakelimbdistance > 0) {
				varwidthwindow = varfakelimbdistance;
				metprint("NOTE: the horizontal tolerance has been lowered, to comply with fake limbs addition\nPlease check the new value correctness", 1);
			}
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Delete fake limbs", GUILayout.Width(150)))
				metremovefakelimbs(varsource);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Kinematic ragdoll (default unchecked):");
			GUILayout.FlexibleSpace();
			varcreatekinematic = GUILayout.Toggle(varcreatekinematic,"");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Rigidity (default 1):");
			GUILayout.FlexibleSpace();
			varrigidity = EditorGUILayout.FloatField(varrigidity,GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Maximum sub bones allowed for exploration (default 4):");
			GUILayout.FlexibleSpace();
			varmaxchildcount = EditorGUILayout.FloatField(varmaxchildcount, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Exploration limit (default 0, 0 equals no limit):");
			GUILayout.FlexibleSpace();
			varexplorationsize = EditorGUILayout.IntField(varexplorationsize, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			varlimbdisplayoptions = EditorGUILayout.Foldout(varlimbdisplayoptions, "Body parts (unchecking any can lead to unexpected results) :");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			if (varlimbdisplayoptions) {
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Head? :");
				GUILayout.FlexibleSpace();
				varexplorehead = GUILayout.Toggle(varexplorehead,"");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Spine? :");
				GUILayout.FlexibleSpace();
				varexplorespine = GUILayout.Toggle(varexplorespine,"");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Left arm? :");
				GUILayout.FlexibleSpace();
				varexplorearmleft = GUILayout.Toggle(varexplorearmleft,"");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Right arm? :");
				GUILayout.FlexibleSpace();
				varexplorearmright = GUILayout.Toggle(varexplorearmright,"");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Left leg? :");
				GUILayout.FlexibleSpace();
				varexplorelegleft = GUILayout.Toggle(varexplorelegleft,"");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Right leg? :");
				GUILayout.FlexibleSpace();
				varexplorelegright = GUILayout.Toggle(varexplorelegright,"");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Preserve limb links to the root (default checked) :");
				GUILayout.FlexibleSpace();
				varpreservelimbslinktoroot = GUILayout.Toggle(varpreservelimbslinktoroot,"");
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
			}
			GUILayout.BeginHorizontal();
			GUILayout.Label("Absorb shoulders (if narrower than legs default checked):");
			GUILayout.FlexibleSpace();
			varfitshoulders = GUILayout.Toggle(varfitshoulders,"");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Absorb tolerance (default 0.25):");
			GUILayout.FlexibleSpace();
			varabsorbtolerance = EditorGUILayout.FloatField(varabsorbtolerance, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("High tension value (default 66):");
			GUILayout.FlexibleSpace();
			varhightensionvalue = EditorGUILayout.FloatField(varhightensionvalue, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Medium tension value (default 33):");
			GUILayout.FlexibleSpace();
			varmidtensionvalue = EditorGUILayout.FloatField(varmidtensionvalue, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Low tension value (default 16):");
			GUILayout.FlexibleSpace();
			varlowtensionvalue = EditorGUILayout.FloatField(varlowtensionvalue, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Minimum tension value (default 8):");
			GUILayout.FlexibleSpace();
			varmintensionvalue = EditorGUILayout.FloatField(varmintensionvalue, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Dampening strength (default 4.5):");
			GUILayout.FlexibleSpace();
			vardampeningstrenght = EditorGUILayout.FloatField(vardampeningstrenght, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Spring strength value (default 9):");
			GUILayout.FlexibleSpace();
			varspringstrenght = EditorGUILayout.FloatField(varspringstrenght, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Drag (default 0):");
			GUILayout.FlexibleSpace();
			vardrag = EditorGUILayout.FloatField(vardrag, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Angular drag (default 0.5):");
			GUILayout.FlexibleSpace();
			varangulardrag = EditorGUILayout.FloatField(varangulardrag, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Sampling matrix size (default 17):");
			GUILayout.FlexibleSpace();
			varmatrixsize = EditorGUILayout.IntField(varmatrixsize, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Bone vertical difference tolerance (default 0.02):");
			GUILayout.FlexibleSpace();
			varheightwindow = EditorGUILayout.FloatField(varheightwindow, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Bone horizontal difference tolerance (default 0.05):");
			GUILayout.FlexibleSpace();
			varwidthwindow = EditorGUILayout.FloatField(varwidthwindow, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Clear existing physics? (will delete joints and colliders):");
			GUILayout.FlexibleSpace();
			varclearphysics = GUILayout.Toggle(varclearphysics,"");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Automatically search for skinnedmeshrenderer child if not found:");
			GUILayout.FlexibleSpace();
			varautomaticallysearchskinnedmeshrenderer = GUILayout.Toggle(varautomaticallysearchskinnedmeshrenderer,"");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Verbose (default unchecked. Warnings and errors always displayed):");
			GUILayout.FlexibleSpace();
			varverbose = GUILayout.Toggle(varverbose,"");
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
		
		GUILayout.EndVertical();
		EditorGUILayout.EndScrollView();
		GUILayout.Space(5);	
	
		GUILayout.BeginHorizontal();
			if(GUILayout.Button("Create ragdoll")) {
				metcreateragdoll();
			}
			if(GUILayout.Button("Connect ragdoll")) {
				if (clsurgutils.metconnectbodies(varsource, varsourceconnector, true)>0)
					metprint("Successfully connected " + varsource.name + " with " + varsourceconnector.name,1);
			}
		GUILayout.EndHorizontal();
		GUILayout.Space(5);	
		if(GUILayout.Button("Clear all physics, from root and all children")) {
			if (varsource!=null) {
				metprint("Clearing all physics from " + varsource.root.name,1);
				metclearphysics(varsource.root);
			}
		}
		if(GUILayout.Button("Clear physics, from source and all children")) {
			if (varsource!=null) {
				metprint("Clearing physics from " + varsource,1);
				metclearphysics(varsource);
			}
		}
	}		
	//preference storing functions
	void OnFocus() {
		if (varmaterial == null) {
			varmaterial = Resources.Load(EditorPrefs.GetString("URGmaterial")) as PhysicMaterial;
		}
		varurgentities = EditorPrefs.GetBool("URGsetentities");
		varcreatekinematic = EditorPrefs.GetBool("URGKinematicragdoll");
		
	}
	
	void OnLostFocus() {
		if (varmaterial!=null)
			EditorPrefs.SetString("URGmaterial", varmaterial.name);
		EditorPrefs.SetBool("URGsetentities", varurgentities);
		EditorPrefs.SetBool("URGKinematicragdoll", varcreatekinematic);
	}
	
	void OnDestroy() {
		if (varmaterial!=null)
			EditorPrefs.SetString("URGmaterial", varmaterial.name);
		EditorPrefs.SetBool("URGsetentities", varurgentities);
		EditorPrefs.SetBool("URGKinematicragdoll", varcreatekinematic);
	}
		
}
