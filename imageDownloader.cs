using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class imageDownloader : MonoBehaviour
{
	[System.Serializable]
	public class grabbedImage {
		public string userID;
		public Texture2D img;
		public int latestVersion;
	}
	[System.Serializable]
	public class pendingData {
		public string userID;
		public RawImage img;
		public float secondsInQueue;
	}

	public List<grabbedImage> grabbedImages;

	public List<pendingData> sendDataQueue;

	public void addToWaitlist(RawImage theImage, string theID) {
		if(!sendDataQueue.Any(x => x.img == theImage)){
			pendingData PD = new pendingData();
			PD.img = theImage;
			PD.userID = theID;
			sendDataQueue.Add(PD);
		}
		
	}

	public IEnumerator getIMGFromID(string IDTest , int vrzn) {
		bool existingImage = false; int id = 0;


		if (grabbedImages.Count > 0) {
			if (grabbedImages.Any(x => x.userID == IDTest)) {
				if (grabbedImages.Find(x => x.userID == IDTest).version == vrzn) {
					id = grabbedImages.FindIndex(x => x.userID == IDTest);
					existingImage = true;
					//The version hasn't changed since you last downloaded a pic, not downloading anything else.
					yield break;
				}
			}
		}

		grabbedImage GI = new grabbedImage();
		GI.userID = userID;
		GI.version = vrzn;

		UnityWebRequest request = UnityWebRequestTexture.GetTexture("url");
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) { Debug.Log(request.error); }
		else { GI.img = ((DownloadHandlerTexture)request.downloadHandler).texture; }

		if (existingImage) { grabbedImages[id] = GI; }
		if (!existingImage) { grabbedImages.Add(GI); }
	}

	private void Update() {
		//return downloaded images to the corresponding texture
		//remove downloaded data from the queue after assigning to a texture

		if(sendDataQueue.Count > 0) {
			for(int i = 0; i < sendDataQueue.Count; i++) {
				if(grabbedImages.Count > 0) {
					if(grabbedImages.Any(x => x.fbmID == sendDataQueue[i].userID)) {
						if(sendDataQueue[i].img != null) {
							sendDataQueue[i].img.texture = grabbedImages.Find(x => x.fbmID == sendDataQueue[i].userID).img;
							sendDataQueue.Remove(sendDataQueue[i]);
						}
						else {
							sendDataQueue.Remove(sendDataQueue[i]);
						}
						break;
					}
				}

				//if the downloaded image is not claimed after a certain amount of time remove it from the queue
				//default to a premade image and assign that to the texture instead
				sendDataQueue[i].secondsInQueue += 1f * Time.unscaledDeltaTime;
				if(sendDataQueue[i].secondsInQueue >= 5) { sendDataQueue.Remove(sendDataQueue[i]); }
			}
		}
	}
}
