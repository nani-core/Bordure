using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace VladStorm {

[System.Serializable]
public class PalettePreset : UnityEngine.Object {

	public new string name;
	// private string filename;
	// public bool isCustom = false; //is custom palette

	Texture2D texOg = null; //original texture

	public Texture2D texSortedPre = null; //preparations

	public RTHandle texSorted;
	public int texSortedWidth = 0;
	// public Texture2D texSortedPre;

	//for custom palettes
	public PalettePreset(Texture2D _tex){

		texOg = _tex;// as Texture2D;
		if(texOg!=null) Prepare();
	}

	public PalettePreset(string name_, string filename){

		name = name_;
		// filename = filename_;

		if(filename!=""){			
			texOg = Resources.Load("palettes/"+filename) as Texture2D;
			if(texOg!=null)
				Prepare();
		}

	}



	void Prepare(){

		// texOg = Resources.Load("palettes/"+filename) as Texture2D;

      //prepare gray as .a
      int cntr = 0;
      Color[] cols = new Color[ texOg.width * texOg.height ];
      for(int i=0;i<texOg.width;i++)
      for(int j=0;j<texOg.height;j++){
      	Color col = texOg.GetPixel(i,j);
      	col.a = (col.r+col.g+col.b)/3f;
      	cols[cntr] = col;

      	cntr++;
      }

      //sort by a
		Color temp;
		for (int j = 0; j < cols.Length; j++) 
		for (int i = 0; i < cols.Length - 1; i++) 
		  	if( cols[i].a > cols[i+1].a) { //? a+b+c /3 ?
		  	// if(cols[i].grayscale > cols[i + 1].grayscale) { //? a+b+c /3 ?		  	
            temp = cols[i+1];
            cols[i+1] = cols[i];
            cols[i] = temp;
		  	}


		//generate palette tex      
      // DestroyImmediate(texSortedPre);
	  	int width = texOg.width * texOg.height;
      // Debug.Log(width);
      if(width>128*128) { Debug.Log("Palette Texture is too big"); return;}
      texSortedPre = new Texture2D(width, 1, TextureFormat.ARGB32, false);
      texSortedPre.filterMode = FilterMode.Point; 
      // texSortedPre.Create();
      


		//put
		for (int i = 0; i < cols.Length; i++) 	{			
      	texSortedPre.SetPixel(i,0,cols[i]);			
		}
      texSortedPre.Apply();

      texSortedWidth = texSortedPre.width;

      //texture2d -> RTHandle
      RTHandles.Release(texSorted);
      texSorted = RTHandles.Alloc(
         texSortedWidth, 
         1,
         TextureXR.slices, ///TODO check coz might be a problem in vr 
         filterMode: FilterMode.Point, 
         // colorFormat: GraphicsFormat.R32_UInt, 
         dimension: TextureXR.dimension, 
         useDynamicScale: true, 
         enableRandomWrite: true);

      Graphics.Blit(texSortedPre, texSorted.rt);
      // Debug.Log("F"+texSortedWidth);

	}



}
}
