using UnityEngine;
using System.Collections;

public static class Util{

	public static bool IsPressed()
	{
		return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0);
	}

	public static bool IsPressing()
	{
		return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Joystick1Button0);
	}
}