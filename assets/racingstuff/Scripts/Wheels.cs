//Wheels.cs generates particles/sounds according to the surface texture when the car is in a drift
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(WheelCollider))]
public class Wheels : MonoBehaviour {
	
	[System.Serializable]
	public class SurfaceType{
		public string surfaceName;
		public Texture2D texture;
		public GameObject skidParticle;
		public AudioClip skidSound;
		public float sidewaySlipLimit;
		public float forwardSlipLimit;
	}
	
	public enum MeshSurfaceDetection{SingleMaterial,MultipleMaterial}
	public bool useSurfaceDetection = true;
	
	[Header("Surface Detection")]
	public MeshSurfaceDetection meshSurfaceDetection;
	public AudioSource skidAudioSource;
	public List<SurfaceType> surfaceTypes = new List<SurfaceType>();
	private WheelCollider _wc;
	private WheelHit wheelHit;
	private Terrain terrain;
	private TerrainData terrainData;
	private SplatPrototype[] splatPrototypes;
	private RaycastHit hit;
	private Texture2D currentTexture;
	
	[Space(10)]
	
	[Header("Non Surface Detection")]
	public GameObject _skidObject;
	public AudioClip _skidSound;
	public float sidewaySlipLimit;
	public float forwardSlipLimit;
	
	
	void Start(){
		Initialize();
		GetTerrainInfo();
	}
	
	void Initialize(){
		
		_wc = GetComponent<WheelCollider>();
				
		//Configure the sound
		if(skidAudioSource){
			skidAudioSource.spatialBlend = 1.0f;
			skidAudioSource.loop = true;
			skidAudioSource.volume = 0.0f;
			skidAudioSource.Play();
		}
		
		//Instantiate all particles as child objects
		for(int i = 0; i < surfaceTypes.Count; i++){
			if(surfaceTypes[i].skidParticle && useSurfaceDetection){
				GameObject particle = (GameObject)Instantiate(surfaceTypes[i].skidParticle,transform.position,Quaternion.identity);
				particle.transform.parent = transform;
				particle.GetComponent<ParticleEmitter>().emit = false;
			}
		}
		
		//Non surface detection initialization
		if(!useSurfaceDetection){
			if(_skidObject){
				GameObject particle = (GameObject)Instantiate(_skidObject,transform.position,Quaternion.identity);
				particle.transform.parent = transform;
				particle.GetComponent<ParticleEmitter>().emit = false;
			}
			
			if(_skidSound){
				skidAudioSource.clip = _skidSound;
			}
		}
	}
	
	
	void GetTerrainInfo(){
		if(Terrain.activeTerrain){
			terrain = Terrain.activeTerrain;
			terrainData = terrain.terrainData;
			splatPrototypes = terrain.terrainData.splatPrototypes;
		}
	}
	
	
	void Update () {
		if(useSurfaceDetection)
			SurfaceDetection();
		else
			NonSurfaceDetection();
	}
	
	void SurfaceDetection(){
		Ray ray = new Ray(transform.position + (Vector3.up * 0.1f), Vector3.down);
		
		//check if the wheel is currently on a terrain or a renderer and get the currentTexture
		if(Physics.Raycast(ray, out hit,  0.2f + (GetComponent<WheelCollider>().radius))){
			
			if(hit.collider.GetComponent<Terrain>()){
			//	currentTexture = splatPrototypes[GetTerrainTexture(transform.position)].texture;
			}
			
			else if(hit.collider.gameObject.GetComponent<Renderer>()){
					switch(meshSurfaceDetection){
						case MeshSurfaceDetection.SingleMaterial :
							currentTexture = (Texture2D)hit.collider.gameObject.GetComponent<Renderer>().material.mainTexture;
						break;
					
						case MeshSurfaceDetection.MultipleMaterial :
							currentTexture = GetSurfaceTexture(); //Please note that "MultipleMaterial" MeshSurfaceDetection is performance intensive!
						break;
				}
			}
		}
		
		//ensure the audio source is always playing
		if(!skidAudioSource.isPlaying){
			skidAudioSource.Play();
		}
		
		EmitSkidParticles();

		//helper to visualize the ground checker ray
		#if UNITY_EDITOR
		Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * (0.2f + (GetComponent<WheelCollider>().radius))),Color.yellow);
		#endif
	}
	
	
	void EmitSkidParticles(){
		
		for(int i = 0; i < surfaceTypes.Count; i++){
			
			if(currentTexture == surfaceTypes[i].texture){
				
				if(surfaceTypes[i].skidSound){
					skidAudioSource.clip = surfaceTypes[i].skidSound;
				}
				
				_wc.GetGroundHit(out wheelHit);
				
				if(Mathf.Abs(wheelHit.sidewaysSlip) >= surfaceTypes[i].sidewaySlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= surfaceTypes[i].forwardSlipLimit || transform.root.GetComponent<Car_Controller>().controllable == true && GetComponent<WheelCollider>().brakeTorque > 1000){
					
					foreach(Transform t in transform){
						if(t.name == surfaceTypes[i].skidParticle.name + "(Clone)"){
							t.GetComponent<ParticleEmitter>().emit = true;
						}
					}
        			        			
					if(skidAudioSource)
						skidAudioSource.volume = Mathf.Abs(wheelHit.sidewaysSlip) + Mathf.Abs(wheelHit.forwardSlip) + 0.5f;
				}
				
				else{
					
					foreach(Transform t in transform){
						if(t.GetComponent<ParticleEmitter>()){
							t.GetComponent<ParticleEmitter>().emit = false;
						}
					}
                   	
					if(skidAudioSource)
						skidAudioSource.volume -= Time.deltaTime;
				}
			}
			
			else{
				foreach(Transform t in transform){
					if(t.name == surfaceTypes[i].skidParticle.name + "(Clone)"){
						t.GetComponent<ParticleEmitter>().emit = false;
					}
				}
			}
		}
	}
    
    
    void NonSurfaceDetection(){
    	_wc.GetGroundHit(out wheelHit);
		if(Mathf.Abs(wheelHit.sidewaysSlip) >= sidewaySlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= forwardSlipLimit){
			
			//enable particles
			foreach(Transform t in transform){
				if(t.name == _skidObject.name + "(Clone)"){
					t.GetComponent<ParticleEmitter>().emit = true;
				}
			}
			
			//skid Sound
			skidAudioSource.volume = Mathf.Abs(wheelHit.sidewaysSlip) + Mathf.Abs(wheelHit.forwardSlip);
		}
		else{
			//skid sound
			skidAudioSource.volume -= Time.deltaTime;
			
			//disable particles
			foreach(Transform t in transform){
				if(t.name == _skidObject.name + "(Clone)"){
					t.GetComponent<ParticleEmitter>().emit = false;
				}
			}
		}
    }
       		
	//returns the mainTexture of a renderer's material at this position
	private Texture2D GetSurfaceTexture(){
		Texture2D texture = null;
		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hit, 0.2f + (GetComponent<WheelCollider>().radius))){
			if(hit.collider.gameObject.GetComponent<Renderer>()){
				MeshFilter meshFilter = (MeshFilter)hit.collider.GetComponent(typeof(MeshFilter));
				Mesh mesh = meshFilter.mesh;
				int totalSubMeshes = mesh.subMeshCount;
				int[] subMeshes = new int[totalSubMeshes];
				for(int i = 0; i < totalSubMeshes; i++){
					subMeshes[i] = mesh.GetTriangles(i).Length / 3;
				}
				
				int hitSubMesh = 0;
				int maxVal = 0;
				
				for(int i = 0; i < totalSubMeshes; i ++){
					maxVal += subMeshes[i];
					if(hit.triangleIndex <= maxVal - 1){
						hitSubMesh = i + 1;
						break;
					}
				}
				texture = (Texture2D)hit.collider.gameObject.GetComponent<Renderer>().materials[hitSubMesh - 1].mainTexture;
			}
		}
		return texture;
	}
	
	/*returns an array containing the relative mix of textures
       on the main terrain at this world position.*/
	private float[] GetTextureMix(Vector3 worldPos) {
		
		terrain = Terrain.activeTerrain;
		terrainData = terrain.terrainData;
		Vector3 terrainPos = terrain.transform.position;
		
		int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
		int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
		
		float[,,] splatmapData = terrainData.GetAlphamaps(mapX,mapZ,1,1);
		
		float[] cellMix = new float[splatmapData.GetUpperBound(2)+1];
		for (int n=0; n<cellMix.Length; ++n){
			cellMix[n] = splatmapData[0,0,n];    
		}
		
		return cellMix;        
	}
	
	/*returns the zero-based index of the most dominant texture
       on the main terrain at this world position.*/
	private int GetTerrainTexture(Vector3 worldPos) {
		
		float[] mix = GetTextureMix(worldPos);
		float maxMix = 0;
		int maxIndex = 0;
		
		for (int n=0; n<mix.Length; ++n){
			
			if (mix[n] > maxMix){
				maxIndex = n;
				maxMix = mix[n];
			}
		}
		
		return maxIndex;
	}
	
}
