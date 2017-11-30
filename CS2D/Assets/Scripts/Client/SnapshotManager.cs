using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnapshotManager{

	List<Interpolate> snapshots;
	int snapshotBuffer = 2;
    public bool alreadyAdded;
	GameData lastGiven;
	GameData lastGivenNotInterpolated;
    int acum;
	public SnapshotManager(int bufferSize){
        snapshots = new List<Interpolate>(bufferSize);
        snapshotBuffer = bufferSize;
	}

	public void Add(GameData gd){
        Interpolate i = new Interpolate(gd);
        Debug.Log("Estoy agregando un snapshot");
        if (snapshots.Count == 0) {
            for(int j = 0; j<snapshotBuffer - 1; j++) {
                snapshots.Add(i);
            }
        }
        if (alreadyAdded) {
            if (gd.Time > snapshots[snapshots.Count - 1].gd.Time)
                snapshots.RemoveAt(snapshots.Count - 1);
            else
                return;
        }
        snapshots.Add(i);
        alreadyAdded = true;
}

	public List<PlayerData> get(){
        Debug.Log("Snapshot size: " + snapshots.Count);
        alreadyAdded = false;
        if(snapshots.Count == 0) {
            return null;
        }
		Interpolate g = snapshots [0];
		snapshots.RemoveAt (0);
        if (g.interpolated) {
            g.gd = interpolate();
        } else {
			//Debug.Log ("Time: " + (g.gd.Time - ((lastGiven == null)?0:lastGiven.Time)));
            acum = 0;
			lastGivenNotInterpolated = g.gd;
        }
		lastGiven = g.gd;
		return g.gd.Players;
	}
	private GameData interpolate (){
        Debug.Log ("Interpole");
        int i = 0;
        while (i < snapshots.Count && snapshots[i].interpolated) {
            i++;
        }
        if(i == snapshots.Count) {
            Debug.Log("Retorno lastGiven");
            return lastGiven;
        } else {
            List<PlayerData> from = lastGivenNotInterpolated.Players;
            List<PlayerData> to = snapshots[i].gd.Players;
            List<PlayerData> res = new List<PlayerData>();
            for(int j = 0; j<to.Count;j++) {
                bool found = false;
                for(int k = 0; k < from.Count && !found; k++) {
                    if(to[k].PlayerId == from[k].PlayerId) {
                        found = true;
                        Debug.Log("i: " + i + ", acum: " + acum);
                        PlayerData newPd = new PlayerData {
							Position = Vector3.Lerp(from[k].Position, to[j].Position, (1.0f + acum) / (2 + i + acum)),
                            PlayerId = from[k].PlayerId
                        };
                        //Debug.Log("From: " + from[k].Position + ", To: " + to[j].Position + ", Inter: " + newPd.Position);
                        res.Add(newPd);
                    }
                }
            }
            GameData inter = new GameData(); 
            inter.Players = res;
			inter.Time = (lastGiven.Time + Time.deltaTime);
            acum++;
            return inter;
        }
        
    }

	public float getRatio(float starting, float ending){
		float dif = ending - starting;
		Debug.Log ("Dif: " + dif + " Delta:" + Time.deltaTime);
		return Mathf.Max(Time.deltaTime / dif, 1);
	}
    
    public void AddInterpolated() {
        Debug.Log("Estoy agregando un interpolated");
        if (snapshots.Count > 0) {
            snapshots.Add(new Interpolate());
        }
    }

    private class Interpolate {
        public bool interpolated;
        public GameData gd;

        public Interpolate() {
            interpolated = true;
        }
        public Interpolate(GameData gd) {
            interpolated = false;
            this.gd = gd;
        }
    }
}
