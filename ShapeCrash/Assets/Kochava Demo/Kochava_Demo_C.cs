using UnityEngine;
using System.Collections;

public class Kochava_Demo_C : MonoBehaviour
{
	
	private Rect WindowControllerRect = new Rect (0, 0, 200, 0);
	private Vector2 scrollPosition;
	
	// Use this for initialization
	void Start ()
	{
		//declare attribution callback, passing the method as the parameter
		Kochava.AttributionCallback AttributionDelegate = new Kochava.AttributionCallback(AttributionCallback);
		//pass it to the attribution callback setter method
		Kochava.SetAttributionCallback(AttributionDelegate);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void  OnGUI ()
	{
		//Draw "Window
		WindowControllerRect = new Rect (10, 20, 250, Screen.height - 30);
		WindowControllerRect = GUI.Window (0, WindowControllerRect, WindowController, "Kochava SDK Demo (C#)");
	}
	
	void  WindowController (int windowID)
	{
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label ("Event Queue Length: " + Kochava.eventQueueLength);
		GUILayout.Label ("Last Event Post Time: " + Kochava.eventPostingTime);
		
		GUILayout.FlexibleSpace ();
		GUILayout.Space (10);
			
		if (GUILayout.Button ("Fire C# Event")) {
			Kochava.FireEvent ("Test C# Event", "Test C# Event Data - " + Random.Range(1, 15));
		}
		
		GUILayout.FlexibleSpace ();
		
		if(Kochava.EventLog.Count > 0) {
			
			GUILayout.Space (10);
			
			GUILayout.Label ("Event Log:");
			scrollPosition = GUILayout.BeginScrollView (scrollPosition);
			
			foreach (Kochava.LogEvent item in Kochava.EventLog)
			{
				GUILayout.Space (10);
				
				if(item.level == Kochava.KochLogLevel.error)
					GUILayout.Label ("*** " + item.text + " ***");
				else
					GUILayout.Label (item.text);
			}
			
			GUILayout.EndScrollView ();
			
		}
		
		GUILayout.FlexibleSpace ();
		
	}

	//delegate method declaration to handle Attribution data
	static void AttributionCallback(string data)
	{
		Debug.Log ("Attribution Data " + data);
	}
	
	//delegate method declaration to handle iBeacon boundary crossed data
	static void iBeaconCallback(string data)
	{
		Debug.Log ("iBeaconCallback Data " + data);
	}
	
}