using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//resolution preset
public class ResPreset {

	public string name;
	public int screenWidth;
	public int screenHeight;
	public bool isFirst; // fullscreen
	public bool isCustom; 

	//for preset "No Preset"
	public ResPreset(){
		isFirst = true;
		name = "No Preset";
	}

	public ResPreset(string name_, int screenWidth_, int screenHeight_){

		name = name_;
		screenWidth = screenWidth_;
		screenHeight = screenHeight_;
		isFirst = false;
		isCustom = false;

	}

}
