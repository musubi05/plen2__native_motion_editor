using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class PlenConnect : MonoBehaviour {
	public ObjectsController objects;
	public string controlServerUrl = "http://127.0.0.1:17264/";

	// Use this for initialization
	void Start () {
		objects.isPlenConnecting = false;
	}

	public void Connect(Action<bool, string, LitJson.JsonData> callback ) {
		StartCoroutine (Get ("connect/?callback=JSON_CALLBACK", (isSuccess, message, response) => {
			objects.isPlenConnecting = isSuccess;
			callback.Invoke(isSuccess, message, response);
		}));
	}
	public void Disconnect(Action<bool, string, LitJson.JsonData> callback ) {
		StartCoroutine (Get ("disconnect/?callback=JSON_CALLBACK", (isSuccess, message, response) => {
			objects.isPlenConnecting = !isSuccess;
			callback.Invoke(isSuccess, message, response);
		}));
	}
	public void Install(string jsonStr, Action<bool, string, LitJson.JsonData> callback ) {
		StartCoroutine (Post ("install/", Encoding.Default.GetBytes(jsonStr), (isSuccess, message, response) => {
			objects.isPlenConnecting = isSuccess;
			callback.Invoke(isSuccess, message, response);
		}));
	}


	IEnumerator Get(string relativePath, Action<bool, string, LitJson.JsonData> callback ) {
		var header = new Dictionary<string, string> ();
		header.Add ("Accept-Language", "ja");

		var www = new WWW (controlServerUrl  + relativePath , null, header);
		yield return www;
	
		if (www.error == null) {
			// Control Server Response ... "  JSON_CALLBACK(~~~~~~~~~~~~~~~~~~~~)"
			var jsonData = LitJson.JsonMapper.ToObject(www.text.Remove (www.text.Length - 1).Replace ("JSON_CALLBACK(", ""));
			callback.Invoke (true, www.text, jsonData);
		} else {
			callback.Invoke (false, www.error, null);
		}
	}

	IEnumerator Post(string relativePath, byte[] postData, Action<bool, string, LitJson.JsonData> callback) {
		var header = new Dictionary<string, string> ();
		header.Add ("Content-Type", "application/json; charset=UTF-8");

		var www = new WWW (controlServerUrl + relativePath, postData, header);

		yield return www;
		if (www.error == null) {
			// Control Server Response ... "  JSON_CALLBACK(~~~~~~~~~~~~~~~~~~~~)"
			var jsonData = LitJson.JsonMapper.ToObject(www.text);
			callback.Invoke (true, www.text, jsonData);
		} else {
			callback.Invoke (false, www.text, null);
		}
			
	}
}
