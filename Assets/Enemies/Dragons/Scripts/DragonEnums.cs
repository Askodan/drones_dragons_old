using UnityEngine;
using System.Collections;

public enum DragonFlightState{
	gliding = 0,//
	flapping = 1,//
	turningLeft = 2,//
	turningRight = 3,//
	flappingLeft = 4,//
	flappingRight = 5,//
	diving = 6, //
	touchingTheSky = 7, //
	hovering = 8,
	breaking = 9,
	turningBack = 10,
}
public enum DragonGroundedState{
	run = 1,
	crawl = 2,

}