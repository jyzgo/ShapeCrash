#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JsonFx.Json;

public enum FireEventType
{
    Achievement,
    AddToCart,
    AddToWishList,
    CheckoutStart,
    LevelComplete,
    Purchase,
    Rating,
    RegistrationComplete,
    Search,
    TutorialComplete,
    View
}

public class FireEventParameters
{

    internal Dictionary<string, object> valuePayload;
    internal string eventName;

    internal Dictionary<FireEventType, string> eventNameList;

    // constructor - provide an eventType enumeration value
    public FireEventParameters(FireEventType fireEventType)
    {
        valuePayload = new Dictionary<string, object>();
        // create a map of enumerations and assign the event name
        eventNameList = new Dictionary<FireEventType, string>()
            {
                { FireEventType.Achievement, "Achievement" },
                { FireEventType.AddToCart, "Add to Cart" },
                { FireEventType.AddToWishList, "Add to Wish List" },
                { FireEventType.CheckoutStart, "Checkout Start" },
                { FireEventType.LevelComplete, "Level Complete" },
                { FireEventType.Purchase, "Purchase" },
                { FireEventType.Rating, "Rating" },
                { FireEventType.RegistrationComplete, "Registration Complete" },
                { FireEventType.Search, "Search" },
                { FireEventType.TutorialComplete, "Tutorial Complete" },
                { FireEventType.View, "View" }
            };
        if (!eventNameList.TryGetValue(fireEventType, out eventName))
            eventName = "";
    }

    public string checkoutAsGuest { set { valuePayload.Add("checkout_as_guest", value ?? ""); } }
    public string contentId { set { valuePayload.Add("content_id", value ?? ""); } }
    public string contentType { set { valuePayload.Add("content_type", value ?? ""); } }
    public string currency { set { valuePayload.Add("currency", value ?? ""); } }
    public string dateString { set { if (!valuePayload.ContainsKey("now_date")) valuePayload.Add("now_date", value ?? ""); } }
    public System.DateTime date { set { if (!valuePayload.ContainsKey("now_date")) valuePayload.Add("now_date", value.ToUniversalTime().ToString() ?? ""); } }
    public string description { set { valuePayload.Add("description", value ?? ""); } }
    public string destination { set { valuePayload.Add("destination", value ?? ""); } }
    public float duration { set { valuePayload.Add("duration", value); } }
    public string endDateString { set { if (!valuePayload.ContainsKey("end_date")) valuePayload.Add("end_date", value ?? ""); } }
    public System.DateTime endDate { set { if (!valuePayload.ContainsKey("end_date")) valuePayload.Add("end_date", value.ToUniversalTime().ToString() ?? ""); } }
    public string itemAddedFrom { set { valuePayload.Add("item_added_from", value ?? ""); } }
    public string level { set { valuePayload.Add("level", value ?? ""); } }
    public float maxRatingValue { set { valuePayload.Add("max_rating_value", value); } }
    public string name { set { valuePayload.Add("name", value ?? ""); } }
    public string orderId { set { valuePayload.Add("order_id", value ?? ""); } }
    public string origin { set { valuePayload.Add("origin", value ?? ""); } }
    public float price { set { valuePayload.Add("price", value); } }
    public string quantity { set { valuePayload.Add("quantity", value ?? ""); } }
    public float ratingValue { set { valuePayload.Add("rating_value", value); } }
    public string receiptId { set { valuePayload.Add("receipt_id", value ?? ""); } }
    public string referralFrom { set { valuePayload.Add("referral_from", value ?? ""); } }
    public string registrationMethod { set { valuePayload.Add("registration_method", value ?? ""); } }
    public string results { set { valuePayload.Add("results", value ?? ""); } }
    public string score { set { valuePayload.Add("score", value ?? ""); } }
    public string searchTerm { set { valuePayload.Add("search_term", value ?? ""); } }
    public string startDateString { set { if (!valuePayload.ContainsKey("start_date")) valuePayload.Add("start_date", value ?? ""); } }
    public System.DateTime startDate { set { if (!valuePayload.ContainsKey("start_date")) valuePayload.Add("start_date", value.ToUniversalTime().ToString() ?? ""); } }
    public string success { set { valuePayload.Add("success", value ?? ""); } }
    public string userId { set { valuePayload.Add("user_id", value ?? ""); } }
    public string userName { set { valuePayload.Add("user_name", value ?? ""); } }
    public string validated { set { valuePayload.Add("validated", value ?? ""); } }    
}

[ExecuteInEditMode]
public class Kochava : MonoBehaviour
{
	
	#region Hardware Integration
	
	#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern string GetExternalKochavaDeviceIdentifiers_iOS(bool incognitoMode);
	[DllImport ("__Internal")]
		private static extern void GetExternalKochavaInfo_iOS(bool loggingEnabled, bool incognitoMode, bool gatheriAdAttribution, int attempts, int attributionWait, int retryWait, string cookieCollectionTargets );
	[DllImport ("__Internal")]
	private static extern bool GetExternalKochavaLocationApproved_iOS();
	[DllImport ("__Internal")]
	private static extern void GetExternalLocationReport_iOS(bool loggingEnabled, int locationAccuracy,int locationTimeout);
	#endif

	#endregion
	
	#region Settings
	
	// Application-Configurable Settings
	public string kochavaAppId = "";
	public string kochavaAppIdIOS = "";
	public string kochavaAppIdAndroid = "";
	public string kochavaAppIdKindle = "";
	public string kochavaAppIdBlackberry = "";
	public string kochavaAppIdWindowsPhone = "";
	public bool debugMode = false;
	public static bool DebugMode { get { return _S.debugMode; } set { _S.debugMode = value; } }
	public bool incognitoMode = false;
	public static bool IncognitoMode { get { return _S.incognitoMode; } set { _S.incognitoMode = value; } }
	public bool requestAttribution = false;
	public static bool RequestAttribution { get { return _S.requestAttribution; } set { _S.requestAttribution = value; } }
	private bool retrieveAttribution = false;
	private bool debugServer = false;
	[HideInInspector]
	public string appVersion = "";
	[HideInInspector]
	public string partnerName = "";
	public bool appLimitAdTracking = false;
	public static bool AppLimitAdTracking { get { return _S.appLimitAdTracking; } set { _S.appLimitAdTracking = value; } }
	[HideInInspector]
	public string userAgent = "";
	public bool adidSupressed = false;
	public static bool AdidSupressed { get { return _S.adidSupressed; } set { _S.adidSupressed = value; } }
	private static int device_id_delay = 60;
	
	#pragma warning disable 0414
	private string whitelist = null;	
	private static bool adidBlacklisted = false;
	#pragma warning restore 0414
	
	private static AttributionCallback attributionCallback;	
	public delegate void AttributionCallback(string callbackString);

	private string appIdentifier = "";

	public static void SetAttributionCallback(AttributionCallback callback)
	{
		attributionCallback = callback;
	}
	
	// Hardwired Helpers
	private string appPlatform = "desktop";
	private string kochavaDeviceId = "";
	
	// Attribution
//	private Dictionary<string, object> attributionData = null;
//	public static Dictionary<string, object> AttributionData { get { return _S.attributionData; } set { _S.attributionData = value; } }
	private string attributionDataStr = "";
	public static string AttributionDataStr { get { return _S.attributionDataStr; } set { _S.attributionDataStr = value; } }
	
	// Identifier Blacklisting
	private List<string> devIdBlacklist = new List<string> ();
	public static List<string> DevIdBlacklist { get { return _S.devIdBlacklist; } set { _S.devIdBlacklist = value; } }
	
	// Event name blacklisting
	private List<string> eventNameBlacklist = new List<string> ();
	public static List<string> EventNameBlacklist { get { return _S.eventNameBlacklist; } set { _S.eventNameBlacklist = value; } }
	
	// Economics
	private const string CURRENCY_DEFAULT = "USD";
	public string appCurrency = CURRENCY_DEFAULT;

	// Session Tracking
	public enum KochSessionTracking {
		full,			// Accuratley track active users - register launch, pause, resume, and exit
		basic,			// Track active users - register launch and exit
		minimal,		// Track user play time	- register exit
		none			// Do not track session data
	};
	public KochSessionTracking sessionTracking = KochSessionTracking.full;
	public static KochSessionTracking SessionTracking { get { return _S.sessionTracking; } set { _S.sessionTracking = value; } }
	
	#endregion
	
	#region Initilization
	
	// Global Constants
	
	public const string KOCHAVA_VERSION = "20160914";
	public const string KOCHAVA_PROTOCOL_VERSION = "4";
	private const int MAX_LOG_SIZE = 50;
	private const int MAX_QUEUE_SIZE = 75;
	private const int MAX_POST_TIME = 15;
	private const int POST_FAIL_RETRY_DELAY = 30;
	private const int QUEUE_KVINIT_WAIT_DELAY = 15;
	private const string API_URL = "https://control.kochava.com";
    private const string TRACKING_URL = API_URL + "/track/kvTracker?v" + KOCHAVA_PROTOCOL_VERSION;
	private const string INIT_URL = API_URL + "/track/kvinit";
	private const string QUERY_URL = API_URL + "/track/kvquery";
	private const string KOCHAVA_QUEUE_STORAGE_KEY = "kochava_queue_storage";
	private const int KOCHAVA_ATTRIBUTION_INITIAL_TIMER = 7;
	private const int KOCHAVA_ATTRIBUTION_DEFAULT_TIMER = 60;
	private int KVTRACKER_WAIT = 60;	

	// Logging
	
	public enum KochLogLevel {
		error,
		warning,
		debug
	};
	
	public class LogEvent
	{
		public string text;
		public float time;
		public KochLogLevel level;
		
		public LogEvent (string text, KochLogLevel level)
		{
			this.text = text;
			this.time = Time.time;
			this.level = level;
		}
	}
	
	private List<LogEvent> _EventLog = new List<LogEvent> ();
	public static List<LogEvent> EventLog { get { return _S._EventLog; } }
	
	// Data Tracking
	
	private Dictionary<string, object> hardwareIdentifierData = new Dictionary<string, object> ();
	private Dictionary<string, object> hardwareIntegrationData = new Dictionary<string, object> ();
	private Dictionary<string, object> appData;
	
	// Event Queue Handling
	
	public class QueuedEvent
	{
		public float eventTime;
		public Dictionary<string, object> eventData;
	}
	
	private Queue<QueuedEvent> eventQueue = new Queue<QueuedEvent> ();
	public static int eventQueueLength { get { return _S.eventQueue.Count; } }
	
	private float processQueueKickstartTime = 0;
	private bool queueIsProcessing = false;
	private float _eventPostingTime = 0.0f;
	public static float eventPostingTime { get { return _S._eventPostingTime; } }

	// location services
	private bool doReportLocation = false;
	private int locationAccuracy = 50;	// meters
	private int locationTimeout = 15;	// seconds
	private int locationStaleness = 15;	// minutes

	// iad attribution retrieval
	private int iAdAttributionAttempts = 3;
	private int iAdAttributionWait = 20;
	private int iAdRetryWait = 10;

	// watchlist check
	private bool send_id_updates = false;

	// Singleton instance reference
	private static Kochava _S;
	
	public void Awake ()
	{
		
		if(!Application.isPlaying)
			return;
		
		// /We are a duplicate, brought to life by the original constructor scene being reloaded.
		if (_S) {
			Log ("detected two concurrent integration objects - please place your integration object in a scene which will not be reloaded.");
			Destroy (this.gameObject);
			return;
		}
		DontDestroyOnLoad (this.gameObject);

		if (_S == null) 
		{
			Kochava._S = this;
		}

		gameObject.name = "_Kochava Analytics";
		
		Log ("Kochava SDK Initialized.\nVersion: " + KOCHAVA_VERSION + "\nProtocol Version: " + KOCHAVA_PROTOCOL_VERSION + "", KochLogLevel.debug);
		
		if(
			kochavaAppId.Length==0 &&
			kochavaAppIdIOS.Length==0 &&
			kochavaAppIdAndroid.Length==0 &&
			kochavaAppIdKindle.Length==0 &&
			kochavaAppIdBlackberry.Length==0 &&
			kochavaAppIdWindowsPhone.Length==0 &&

			partnerName.Length==0)
		{
		   Log ("No Kochava App Id or Partner Name - SDK will terminate");
		   Destroy (this.gameObject);
		}  
		
		loadQueue ();
		
	}
	
	public void Start ()
	{
		if(!Application.isPlaying)
			return;

		if (_S == null) 
		{
			Kochava._S = this;
		}

		Init();
	}
	
	public void OnEnable ()
	{
		if(!Application.isPlaying)
			return;

		if (_S == null) 
		{
			Kochava._S = this;
		}
	}
	
	private void Init()
	{
		
		
		try {
			
			// Poll Hardware Integration libraries for device-specific data
			
			#if !UNITY_EDITOR && UNITY_IPHONE
			if(incognitoMode)
			{
				Log ("Incognito Mode enabled, using simplified handshake protocol");
			}
			
			appPlatform = "ios";
			if(kochavaAppIdIOS != "")
				kochavaAppId = kochavaAppIdIOS;
			
			try
			{
				string hardwareIdentifiersString = GetExternalKochavaDeviceIdentifiers_iOS(incognitoMode);
				hardwareIdentifierData = JsonReader.Deserialize<Dictionary<string, object>> (hardwareIdentifiersString);
				Log ("Received (" + hardwareIdentifierData.Count + ") parameters from Hardware Integration Library (identifiers): " + hardwareIdentifiersString);
			}
			catch(Exception e)
			{
				Log ("Failed GetExternalKochavaDeviceIdentifiers_iOS: " + e, KochLogLevel.warning);
			}
			
			if(hardwareIdentifierData.ContainsKey("user_agent"))
			{
				userAgent = hardwareIdentifierData["user_agent"].ToString();
				Log ("userAgent set to: " + userAgent, KochLogLevel.debug);
			}

			#endif

			#if !UNITY_EDITOR && UNITY_ANDROID
			
			try
			{
				// Get Android context
				AndroidJNIHelper.debug = true;
				using(AndroidJavaClass androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
					AndroidJavaObject androidActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
					AndroidJavaObject androidContext = androidActivity.Call<AndroidJavaObject>("getApplicationContext");
					
					AndroidJavaClass androidHelperClass = new AndroidJavaClass("com.kochava.android.tracker.lite.KochavaSDKLite");
					string hardwareIdentifiersString = androidHelperClass.CallStatic<string>("GetExternalKochavaDeviceIdentifiers_Android", androidContext, AdidSupressed);
					Log ("Hardware Integration Diagnostics: " + hardwareIdentifiersString);
					
					
					hardwareIdentifierData = JsonReader.Deserialize<Dictionary<string, object>> (hardwareIdentifiersString);
					Log ("Received (" + hardwareIdentifierData.Count + ") parameters from Hardware Integration Library (identifiers): " + hardwareIdentifiersString);
				}
			}
			catch(Exception e)
			{
				Log ("Failed GetExternalKochavaDeviceIdentifiers_Android: " + e, KochLogLevel.warning);
			}
			
			if(hardwareIdentifierData.ContainsKey("user_agent"))
			{
				userAgent = hardwareIdentifierData["user_agent"].ToString();
				Log ("userAgent set to: " + userAgent, KochLogLevel.debug);
			}
			
			
			// Kindle
			if(userAgent.Contains("kindle") || userAgent.Contains("silk"))
			{
				appPlatform = "kindle";
				if(kochavaAppIdKindle != "")
					kochavaAppId = kochavaAppIdKindle;
			}
			
			// Other Android Device
			else
			{
				appPlatform = "android";
				if(kochavaAppIdAndroid != "")
					kochavaAppId = kochavaAppIdAndroid;
			}
			
			#endif
			
			#if !UNITY_EDITOR && UNITY_BLACKBERRY
			appPlatform = "blackberry";
			if(kochavaAppIdBlackberry != "")
				kochavaAppId = kochavaAppIdBlackberry;
			#endif
			
			#if !UNITY_EDITOR && UNITY_WP8
			appPlatform = "wp8";
			if(kochavaAppIdWindowsPhone != "")
				kochavaAppId = kochavaAppIdWindowsPhone;
			#endif
			
					
			if(hardwareIdentifierData.ContainsKey("package"))
			{
				appIdentifier = hardwareIdentifierData["package"].ToString();
				Log ("appIdentifier set to: " + appIdentifier, KochLogLevel.debug);
			}

			
			
			// autoprovisioned kochava_app_ids
			
			if(PlayerPrefs.HasKey("kochava_app_id"))
			{
				kochavaAppId = PlayerPrefs.GetString("kochava_app_id");
				Log ("Loaded kochava_app_id from persistent storage: " + kochavaAppId, KochLogLevel.debug);
			}
			
			// kochava_device_id determination
			
			// have device id on file, good to go
			if(PlayerPrefs.HasKey("kochava_device_id"))							// most likely autoprovisioned during a previous app launch
			{
				kochavaDeviceId = PlayerPrefs.GetString("kochava_device_id");
				Log ("Loaded kochava_device_id from persistent storage: " + kochavaDeviceId, KochLogLevel.debug);
			}
			
			// no device id on file, scrape something together
			else
			{
				// incognito mode - use a (nondeterministic) guid
				if(incognitoMode)
				{
					kochavaDeviceId = "KA" + System.Guid.NewGuid().ToString().Replace ("-", "");
					Log ("Using autogenerated \"incognito\" kochava_device_id: " + kochavaDeviceId, KochLogLevel.debug);
				}
				
				// standard mode - anything goes
				else
				{
					// check for original id
					string kochavaDeviceIdOrig = "";
					if(PlayerPrefs.HasKey("data_orig_kochava_device_id"))
						kochavaDeviceIdOrig = PlayerPrefs.GetString("data_orig_kochava_device_id");
					if(kochavaDeviceIdOrig != "")
					{
						kochavaDeviceId = kochavaDeviceIdOrig;
						Log ("Using \"orig\" kochava_device_id: " + kochavaDeviceId, KochLogLevel.debug);
					}
					
					// make something up on the spot
					else 															
					{
						// gathering the SystemInfo.deviceUniqueIdentifier on Android involves accessing device IMEI information, which undesirably necessitates inclusion of the read_phone_state permission in the Android manifest.
						#if UNITY_ANDROID
						kochavaDeviceId = "KU" + System.Guid.NewGuid().ToString().Replace ("-", "");
						#endif
						
						#if !UNITY_ANDROID
						kochavaDeviceId = "KU" + SystemInfo.deviceUniqueIdentifier.Replace ("-", "");
						#endif
						
						Log ("Using autogenerated kochava_device_id: " + kochavaDeviceId, KochLogLevel.debug);
					}
				}
			}
			
			// preserve original SDK values for posterity
			
			if(!PlayerPrefs.HasKey("data_orig_kochava_app_id") && kochavaAppId != "")
				PlayerPrefs.SetString("data_orig_kochava_app_id", kochavaAppId);
			
			if(!PlayerPrefs.HasKey("data_orig_kochava_device_id") && kochavaDeviceId != "")
				PlayerPrefs.SetString("data_orig_kochava_device_id", kochavaDeviceId);
			
			if(!PlayerPrefs.HasKey("data_orig_session_tracking"))
				PlayerPrefs.SetString("data_orig_session_tracking", sessionTracking.ToString());
			
			if(!PlayerPrefs.HasKey("data_orig_currency") && appCurrency != "")
				PlayerPrefs.SetString("data_orig_currency", appCurrency);
			
			// other prior kvinit response flag overrides
			
			if(PlayerPrefs.HasKey("currency"))
			{
				appCurrency = PlayerPrefs.GetString("currency");
				Log ("Loaded currency from persistent storage: " + appCurrency, KochLogLevel.debug);
			}
			if(PlayerPrefs.HasKey("blacklist"))
			{
				try
				{
					string devIdBlacklistStr = PlayerPrefs.GetString("blacklist");
					devIdBlacklist = new List<string> ();
					string[] devIdBlacklistArr = (string[]) JsonReader.Deserialize<string[]> (devIdBlacklistStr);
					for ( int i = devIdBlacklistArr.Length-1; i >= 0; i-- )
						devIdBlacklist.Add(devIdBlacklistArr[i]);
					Log ("Loaded device_id blacklist from persistent storage: " + devIdBlacklistStr, KochLogLevel.debug);
				}
				catch(Exception e)
				{
					Log ("Failed loading device_id blacklist from persistent storage: " + e, KochLogLevel.warning);
				}
			}
			

			if(PlayerPrefs.HasKey("attribution"))
			{
				try
				{
					attributionDataStr = PlayerPrefs.GetString("attribution");
//					attributionData = JsonReader.Deserialize<Dictionary<string, object>> (attributionDataStr);
					Log ("Loaded attribution data from persistent storage: " + attributionDataStr, KochLogLevel.debug);
				}
				catch(Exception e)
				{
					Log ("Failed loading attribution data from persistent storage: " + e, KochLogLevel.warning);
				}
			}
			
			if(PlayerPrefs.HasKey("session_tracking"))
			{
				try
				{
					string sessionTrackingStr = PlayerPrefs.GetString("session_tracking");
					sessionTracking = (KochSessionTracking) System.Enum.Parse( typeof( KochSessionTracking ), sessionTrackingStr, true);
					Log ("Loaded session tracking mode from persistent storage: " + sessionTrackingStr, KochLogLevel.debug);
				}
				catch(Exception e)
				{
					Log ("Failed loading session tracking mode from persistent storage: " + e, KochLogLevel.warning);
				}
			}

			// if kvinit and kvtracker last times don't exist, seed them
			if(!PlayerPrefs.HasKey("kvinit_wait"))
			{
				PlayerPrefs.SetString ("kvinit_wait", "60");
			}
			if(!PlayerPrefs.HasKey("kvinit_last_sent"))
			{
				PlayerPrefs.SetString ("kvinit_last_sent", "0");
			}
			if(!PlayerPrefs.HasKey("kvtracker_wait"))
			{
				PlayerPrefs.SetString ("kvtracker_wait", "60");
			}
			if(!PlayerPrefs.HasKey("last_location_time"))
			{
				PlayerPrefs.SetString ("last_location_time", "0");
			}


			// make sure ok to send a kvinit - could have been delayed with kvinit_wait
			double lastKVInitSendTime = double.Parse(PlayerPrefs.GetString("kvinit_last_sent"));
			double currtime = CurrentTime();
			double kvinit_wait = double.Parse(PlayerPrefs.GetString("kvinit_wait"));

//			Log ("last: " + lastKVInitSendTime + "  curr: " + currtime + "  wait: " + kvinit_wait);

			if ( (currtime-lastKVInitSendTime) > kvinit_wait )
			{

				// initiate kvinit request
				
				Dictionary<string, object> initData_data = new Dictionary<string, object>() {
					{"partner_name", partnerName},
					{"package", appIdentifier},
					{"platform", appPlatform},
					{"session_tracking", sessionTracking.ToString()},
					{"currency", appCurrency == null || appCurrency == "" ? CURRENCY_DEFAULT : appCurrency},
					{"os_version", SystemInfo.operatingSystem}
				};
				
				if(requestAttribution && !PlayerPrefs.HasKey("attribution"))
					retrieveAttribution = true;
				Log("retrieve attrib: " + (bool)retrieveAttribution);
				
				
				if(hardwareIdentifierData.ContainsKey("IDFA"))
					initData_data.Add("idfa", hardwareIdentifierData["IDFA"]);
				if(hardwareIdentifierData.ContainsKey("IDFV"))
					initData_data.Add("idfv", hardwareIdentifierData["IDFV"]);
				
				Dictionary<string, object> initData_orig = new Dictionary<string, object>() {
					{"kochava_app_id", PlayerPrefs.GetString("data_orig_kochava_app_id")},
					{"kochava_device_id", PlayerPrefs.GetString("data_orig_kochava_device_id")},
					{"session_tracking", PlayerPrefs.GetString("data_orig_session_tracking")},
					{"currency", PlayerPrefs.GetString("data_orig_currency")}
				};
				Dictionary<string, object> initData = new Dictionary<string, object>() {
					{"action", "init"},
					{"data", initData_data},
					{"data_orig", initData_orig},
					{"kochava_app_id", kochavaAppId },
					{"kochava_device_id", kochavaDeviceId },
					{"sdk_version", "Unity3D-" + KOCHAVA_VERSION},
					{"sdk_protocol", KOCHAVA_PROTOCOL_VERSION},
				};
				
				StartCoroutine(Init_KV (JsonWriter.Serialize (initData)));
			}
			else
			{
				// if not going to send a kvinit, need to populate appData
				appData = new Dictionary<string, object> () {
					{"kochava_app_id", kochavaAppId },
					{"kochava_device_id", kochavaDeviceId },
					{"sdk_version", "Unity3D-" + KOCHAVA_VERSION},
					{"sdk_protocol", KOCHAVA_PROTOCOL_VERSION},
				};

				if(PlayerPrefs.HasKey("eventname_blacklist"))
				{
					string[] temp = JsonReader.Deserialize<string[]> (PlayerPrefs.GetString("eventname_blacklist"));
					List<string> tempEventNameBlacklist = new List<string>();
					for(int i = 0; i < temp.Length; i++)
					{
							tempEventNameBlacklist.Add(temp[i]);
					}
					eventNameBlacklist = tempEventNameBlacklist;
				}

			}


		} catch (Exception e) {
			Log ("Overall failure in init: " + e, KochLogLevel.warning);
		}

	}
	
	private IEnumerator Init_KV(string postData)
	{
		if (Application.internetReachability == NetworkReachability.NotReachable) {
			Log ("internet not reachable", KochLogLevel.warning);
			yield return new WaitForSeconds (POST_FAIL_RETRY_DELAY);
			
			// retry
			StartCoroutine (Init_KV (postData));
			yield break;
		} else {
			
			Log ("Initiating kvinit handshake...", KochLogLevel.debug);
			
			Dictionary<string, string> headers = new Dictionary<string, string> () {
				{"Content-Type", "application/xml"},
				{"User-Agent", userAgent},
			};

			Log (postData, KochLogLevel.debug);

			float wwwLoadTime = Time.time;
			
			WWW www = new WWW (INIT_URL, System.Text.Encoding.UTF8.GetBytes (postData), headers);
			yield return www;

			// kvinit request failure
			if ( !String.IsNullOrEmpty (www.error)) {
				Log ("Kvinit handshake failed: " + www.error + ", seconds: " + (Time.time - wwwLoadTime) + ")", KochLogLevel.warning);
				yield return new WaitForSeconds (POST_FAIL_RETRY_DELAY);
				
				// retry
				StartCoroutine (Init_KV (postData));
				yield break;
			}
			
			// Deserialize JSON response from server
			Dictionary<string, object> serverResponse = new Dictionary<string, object> ();
			if (www.text != "") {
				
				try {
					serverResponse = JsonReader.Deserialize<Dictionary<string, object>> (www.text);
				} catch (Exception e) {
					Log ("Failed Deserialize JSON response to kvinit: " + e, KochLogLevel.warning);
				}
			}

			Log (www.text, KochLogLevel.debug);

			// kvinit parse failure
			if (!serverResponse.ContainsKey ("success")) {
				Log ("Kvinit handshake parsing failed: " + www.text, KochLogLevel.warning);
				yield return new WaitForSeconds (POST_FAIL_RETRY_DELAY);
				
				// retry
				StartCoroutine (Init_KV (postData));
				yield break;
			}
			else {
				
				// kvinit success!

				PlayerPrefs.SetString ("kvinit_last_sent", CurrentTime().ToString());

				Log ("...kvinit handshake complete, processing response flags...", KochLogLevel.debug);
				
				// response flags
				if (serverResponse.ContainsKey ("flags")) {
					
					Dictionary<string, object> serverResponseFlags = (Dictionary<string, object>)serverResponse ["flags"];
					
					if (serverResponseFlags.ContainsKey ("kochava_app_id")) {
						kochavaAppId = serverResponseFlags ["kochava_app_id"].ToString ();
						PlayerPrefs.SetString ("kochava_app_id", kochavaAppId);
						Log ("Saved kochava_app_id to persistent storage: " + kochavaAppId, KochLogLevel.debug);
					}
					if (serverResponseFlags.ContainsKey ("kochava_device_id")) {
						kochavaDeviceId = serverResponseFlags ["kochava_device_id"].ToString ();
					}
					if (serverResponseFlags.ContainsKey ("resend_initial") && (bool)serverResponseFlags ["resend_initial"] == true) {
						// clear watchlist, which will trigger the initInitial command to fire after the callback to GetExternalKochavaInfo fires
						PlayerPrefs.DeleteKey ("watchlistProperties");
						Log ("Refiring initial event, as requested by kvinit response flag", KochLogLevel.debug);
					}
					if (serverResponseFlags.ContainsKey ("session_tracking")) {
						try {
							sessionTracking = (KochSessionTracking)System.Enum.Parse (typeof(KochSessionTracking), serverResponseFlags ["session_tracking"].ToString ());
							PlayerPrefs.SetString ("session_tracking", sessionTracking.ToString ());
							Log ("Saved session_tracking mode to persistent storage: " + sessionTracking.ToString (), KochLogLevel.debug);
						} catch (Exception e) {
							Log ("Failed System.Enum.Parse of KochSessionTracking: " + e, KochLogLevel.warning);
						}
					}
					if (serverResponseFlags.ContainsKey ("currency")) {
						appCurrency = serverResponseFlags ["currency"].ToString ();
						if(appCurrency.Equals(""))
							appCurrency = CURRENCY_DEFAULT;

						PlayerPrefs.SetString ("currency", appCurrency);
						Log ("Saved currency to persistent storage: " + appCurrency, KochLogLevel.debug);
					}

					if (serverResponseFlags.ContainsKey ("getattribution_wait")) {
						string attributionWaitString = serverResponseFlags ["getattribution_wait"].ToString();

						int attributionWait = int.Parse(attributionWaitString);
						
						if ( attributionWait < 1 )
							attributionWait = 1;
						if ( attributionWait > 30 )
							attributionWait = 30;

						PlayerPrefs.SetString ("getattribution_wait", attributionWait.ToString());
						Log ("Saved getattribution_wait to persistent storage: " + attributionWait, KochLogLevel.debug);
					}

					if(serverResponseFlags.ContainsKey("delay_for_referrer_data"))
					{
						device_id_delay = (int)serverResponseFlags["delay_for_referrer_data"];
						Log ("delay_for_referrer_data received: " + device_id_delay, KochLogLevel.debug);
						if(device_id_delay < 0)
						{
							Log("device_id_delay returned was less than 0 (" + device_id_delay + "), setting device_id_delay to 0." );
							device_id_delay = 0;
						}
						else if(device_id_delay > 120)
						{
							Log("device_id_delay returned was greater than 120 (" + device_id_delay + "), setting device_id_delay to 120." );
							device_id_delay = 120;
						}
						else
							Log("setting device_id_delay to: " + device_id_delay);

					}

					if (serverResponseFlags.ContainsKey ("kvinit_wait"))
					{
						string kvinitWaitString = serverResponseFlags ["kvinit_wait"].ToString();
						
						int kvinitWait = int.Parse(kvinitWaitString);
						
						if ( kvinitWait < 60 )
							kvinitWait = 60;
						if ( kvinitWait > 604800 )	// one week
							kvinitWait = 604800;
						
						PlayerPrefs.SetString ("kvinit_wait", kvinitWait.ToString());
						Log ("Saved kvinit_wait to persistent storage: " + kvinitWait, KochLogLevel.debug);
					}
					else {
						PlayerPrefs.SetString ("kvinit_wait", "60");
						Log ("Saved kvinit_wait to persistent storage: " + "60", KochLogLevel.debug);
					}
					
					if (serverResponseFlags.ContainsKey ("kvtracker_wait"))
					{
						string kvtrackerWaitString = serverResponseFlags ["kvtracker_wait"].ToString();
						
						int kvtrackerWait = int.Parse(kvtrackerWaitString);
						
						if ( kvtrackerWait < 60 )
							kvtrackerWait = 60;
						if ( kvtrackerWait > 604800 )	// one week
							kvtrackerWait = 604800;
						
						PlayerPrefs.SetString ("kvtracker_wait", kvtrackerWait.ToString());
						Log ("Saved kvtracker_wait to persistent storage: " + kvtrackerWait, KochLogLevel.debug);
						KVTRACKER_WAIT = kvtrackerWait;
					}
					else {
						PlayerPrefs.SetString ("kvtracker_wait", "60");
						Log ("Saved kvtracker_wait to persistent storage: " + "60", KochLogLevel.debug);
						KVTRACKER_WAIT = 60;
					}
					
					if (serverResponseFlags.ContainsKey ("location_accuracy"))
					{
						string accuracyString = serverResponseFlags ["location_accuracy"].ToString();

						int accuracy = int.Parse (accuracyString);

						if ( accuracy < 10 )
							accuracy = 10;
						if ( accuracy > 5000 )
							accuracy = 5000;

						locationAccuracy = accuracy;
					}
					if (serverResponseFlags.ContainsKey ("location_timeout"))
					{
						string timeoutString = serverResponseFlags ["location_timeout"].ToString();

						int timeout = int.Parse(timeoutString);

						if ( timeout < 3 )
							timeout = 3;
						if ( timeout > 60 )
							timeout = 60;

						locationTimeout = timeout;
					}
					if (serverResponseFlags.ContainsKey ("location_staleness"))
					{
						string stalenessString = serverResponseFlags ["location_staleness"].ToString();

						int staleness = int.Parse (stalenessString);

						if ( staleness < 1 )
							staleness = 1;
						if ( staleness > 10080 )
							staleness = 10080;

						locationStaleness = staleness;
					}

					// iAd attribution retrieval
					if (serverResponseFlags.ContainsKey ("iad_attribution_attempts"))
					{
						string attemptsString = serverResponseFlags ["iad_attribution_attempts"].ToString();
						
						int attempts = int.Parse (attemptsString);
						
						if ( attempts < 1 )
							attempts = 1;
						if ( attempts > 10 )
							attempts = 10;
						
						iAdAttributionAttempts = attempts;
					}
					if (serverResponseFlags.ContainsKey ("iad_attribution_wait"))
					{
						string waitString = serverResponseFlags ["iad_attribution_wait"].ToString();
						
						int wait = int.Parse (waitString);
						
						if ( wait < 1 )
							wait = 1;
						if ( wait > 120 )
							wait = 120;
						
						iAdAttributionWait = wait;
					}
					if (serverResponseFlags.ContainsKey ("iad_retry_wait"))
					{
						string retryWaitString = serverResponseFlags ["iad_retry_wait"].ToString();
						
						int retryWait = int.Parse (retryWaitString);
						
						if ( retryWait < 1 )
							retryWait = 1;
						if ( retryWait > 60 )
							retryWait = 60;
						
						iAdRetryWait = retryWait;
					}

					if (serverResponseFlags.ContainsKey ("send_id_updates") && (bool)serverResponseFlags ["send_id_updates"] == true) 
					{
						send_id_updates = true;
					}
				}
														
				// device id blacklisting
				if (serverResponse.ContainsKey ("blacklist")) {
					devIdBlacklist = new List<string> ();
					if (serverResponse ["blacklist"].GetType ().GetElementType () == typeof(string)) {
						try {
							string[] devIdBlacklistArr = (string[])serverResponse ["blacklist"];
							for (int i = devIdBlacklistArr.Length-1; i >= 0; i--)
							{
								devIdBlacklist.Add (devIdBlacklistArr [i]);
								
								if(devIdBlacklistArr[i].ToLower().Equals("adid"))
								{
									adidBlacklisted = true;
								}
							}
						} catch (Exception e) {
							Log ("Failed parsing device_identifier blacklist received from server: " + e, KochLogLevel.warning);
						}
					}
					try {
						string devIdBlacklistStr = JsonWriter.Serialize (devIdBlacklist);
						PlayerPrefs.SetString ("blacklist", devIdBlacklistStr);
						Log ("Saved device_identifier blacklist (" + devIdBlacklist.Count + " elements) to persistent storage: " + devIdBlacklistStr, KochLogLevel.debug);
					} catch (Exception e) {
						Log ("Failed saving device_identifier blacklist to persistent storage: " + e, KochLogLevel.warning);
					}
				}
				
				// device id whitelisting
				if (serverResponse.ContainsKey ("whitelist")) 
				{
					if (serverResponse ["whitelist"].GetType ().GetElementType () == typeof(string)) 
					{					
						string result = "{";
						try 
						{
							string[] devIdWhitelistArr = (string[])serverResponse ["whitelist"];
							
							for (int i = devIdWhitelistArr.Length-1; i >= 0; i--)
							{
								if ( devIdWhitelistArr[i] == "location" )
									doReportLocation = true;

								if(i != 0)
								{
									result += devIdWhitelistArr[i] + ",";
								}
								else
								{
									result += devIdWhitelistArr[i];
								}
							}
							
						} 
						catch (Exception e) 
						{
							Log ("Failed parsing device_identifier whitelist received from server: " + e, KochLogLevel.warning);
						}
						result += "}";
						Log("whitelist string: " + result);
						whitelist = result;
					}
				}

				// event name blacklisting
				if (serverResponse.ContainsKey ("eventname_blacklist")) {
					if (serverResponse ["eventname_blacklist"].GetType ().GetElementType () == typeof(string)) {
						try {
							string[] eventNameBlacklistArr = (string[])serverResponse ["eventname_blacklist"];
							for (int i = eventNameBlacklistArr.Length-1; i >= 0; i--)
								eventNameBlacklist.Add (eventNameBlacklistArr [i]);
						} catch (Exception e) {
							Log ("Failed parsing eventname_blacklist received from server: " + e, KochLogLevel.warning);
						}
					}
					PlayerPrefs.SetString("eventname_blacklist",JsonWriter.Serialize (eventNameBlacklist));
				}

				// Populate initial app data
				appData = new Dictionary<string, object> () {
					{"kochava_app_id", kochavaAppId },
					{"kochava_device_id", kochavaDeviceId },
					{"sdk_version", "Unity3D-" + KOCHAVA_VERSION},
					{"sdk_protocol", KOCHAVA_PROTOCOL_VERSION},
				};
				
				// general data persistence
				PlayerPrefs.SetString ("kochava_device_id", kochavaDeviceId);
				Log ("Saved kochava_device_id to persistent storage: " + kochavaDeviceId, KochLogLevel.debug);
				
				// Fire Initial event for App or OS upgrades
				// We have to wait for kvinit completeon as blacklisted device_identifiers, etc can be customized via kvinit response flags
				
				if (sessionTracking == KochSessionTracking.full || sessionTracking == KochSessionTracking.basic)
					_S._fireEvent ("session", new Dictionary<string, object> () {
						{ "state", "launch" }
					});
				
				#if !UNITY_EDITOR && UNITY_IPHONE
				bool debugLogging = false;
				if (DebugMode)
					debugLogging = true;

				bool gatheriAdAttribution = false;
				if ( !PlayerPrefs.HasKey("watchlistProperties") )
				    gatheriAdAttribution = true;

				string cookieCollectionTargets = "";
				if (serverResponse.ContainsKey ("cookie_collection")) 
				{
					cookieCollectionTargets = JsonWriter.Serialize (serverResponse["cookie_collection"]);
				}

				GetExternalKochavaInfo_iOS(debugLogging, incognitoMode, gatheriAdAttribution, iAdAttributionAttempts, iAdAttributionWait, iAdRetryWait, cookieCollectionTargets);
				#endif

				#if !UNITY_EDITOR && UNITY_ANDROID
				// Get Android context
				AndroidJNIHelper.debug = true;
				using(AndroidJavaClass androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
					AndroidJavaObject androidActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
					AndroidJavaObject androidContext = androidActivity.Call<AndroidJavaObject>("getApplicationContext");
					
					AndroidJavaClass androidHelperClass = new AndroidJavaClass("com.kochava.android.tracker.lite.KochavaSDKLite");
					androidHelperClass.CallStatic<string>("GetExternalKochavaInfo_Android", androidContext, whitelist, device_id_delay, PlayerPrefs.GetString("blacklist"), AdidSupressed);
					
				}
				#endif

				if(doReportLocation)
				{
					double currtime = CurrentTime();
					double lastLocationTime = double.Parse(PlayerPrefs.GetString("last_location_time"));

					if ( currtime - lastLocationTime > locationStaleness * 60 )
					{
						#if !UNITY_EDITOR && UNITY_IPHONE
						bool isLocationServicesApproved = GetExternalKochavaLocationApproved_iOS();
						
						if (isLocationServicesApproved)
						{
							debugLogging = false;
							if (DebugMode)
								debugLogging = true;

							Log ("LOCATION APPROVED: " + isLocationServicesApproved );
							GetExternalLocationReport_iOS(debugLogging,locationAccuracy,locationTimeout);
							
						}
						#endif

						#if !UNITY_EDITOR && UNITY_ANDROID
						using(AndroidJavaClass androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
							AndroidJavaObject androidActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
							AndroidJavaObject androidContext = androidActivity.Call<AndroidJavaObject>("getApplicationContext");
							
							AndroidJavaClass androidHelperClass = new AndroidJavaClass("com.kochava.android.tracker.lite.KochavaSDKLite");
							androidHelperClass.CallStatic<string>("GetExternalLocationReport_Android", locationAccuracy, locationTimeout, locationStaleness);
							Log("Calling android location gather");
						}
						#endif
					}

				}

			}
		}		
	}
	
	void DeviceInformationCallback(string deviceInfo) {
		try
		{
			hardwareIntegrationData = JsonReader.Deserialize<Dictionary<string, object>> (deviceInfo);
			Log ("Received (" + hardwareIntegrationData.Count + ") parameters from Hardware Integration Library (device info): " + deviceInfo);
		}
		catch(Exception e)
		{
			Log ("Failed Deserialize hardwareIntegrationData: " + e, KochLogLevel.warning);
		}
		
		// if watchlistProperties hasn't been persisted, then the initial command hasn't yet been sent
		if ( !PlayerPrefs.HasKey("watchlistProperties") )
			initInitial();
		else
			ScanWatchlistChanges();
	}
	
	public static void InitInitial ()
	{
		_S.initInitial();
	}
	
	private void initInitial()
	{
		Dictionary<string, object> initialParams = new Dictionary<string, object>();
		
		try
		{
			initialParams.Add ("device", SystemInfo.deviceModel);
			
			if ( hardwareIntegrationData.ContainsKey("package") )
				initialParams.Add ("package", hardwareIntegrationData["package"]);
			else
				initialParams.Add ("package", appIdentifier);
			
			if ( hardwareIntegrationData.ContainsKey("app_version") )
				initialParams.Add ("app_version", hardwareIntegrationData["app_version"]);
			else
				initialParams.Add ("app_version", appVersion);
		
			if ( hardwareIntegrationData.ContainsKey("app_short_string") )
				initialParams.Add ("app_short_string", hardwareIntegrationData["app_short_string"]);
			else
				initialParams.Add ("app_short_string", appVersion);
			
			initialParams.Add ("currency", appCurrency == "" ? CURRENCY_DEFAULT : appCurrency);

			if(!devIdBlacklist.Contains ("screen_size"))
			{
				initialParams.Add ("disp_h", Screen.height);
				initialParams.Add ("disp_w", Screen.width);
			}

			if(!devIdBlacklist.Contains ("device_orientation") && hardwareIntegrationData.ContainsKey("device_orientation") )
			{
				initialParams.Add ("device_orientation", hardwareIntegrationData["device_orientation"]);
			}

			if(!devIdBlacklist.Contains ("screen_brightness") && hardwareIntegrationData.ContainsKey("screen_brightness") )
			{
				initialParams.Add ("screen_brightness", hardwareIntegrationData["screen_brightness"]);
			}

			if(!devIdBlacklist.Contains ("network_conn_type"))
			{
				bool internetReachableCellular = false;
				bool internetReachableWifi = false;
				switch (Application.internetReachability)
				{
				case NetworkReachability.ReachableViaCarrierDataNetwork:
					internetReachableCellular = true;
					break;
				case NetworkReachability.ReachableViaLocalAreaNetwork:
					internetReachableWifi = true;
					break;
				}
				if(internetReachableCellular)
				{
					initialParams.Add ("network_conn_type", "cellular");
				}
				else if (internetReachableWifi)
				{
					initialParams.Add ("network_conn_type", "wifi");
				}		
			}

#if !UNITY_EDITOR && UNITY_IPHONE
			if( hardwareIntegrationData.ContainsKey("cookie_collection") )
			{
				initialParams.Add ("cookie_collection", hardwareIntegrationData["cookie_collection"]);
			}

			// game center data (user id and alias - ios only) will come through the
			// identity_link payload from the unity ios shim
            // Identity link (
            if( hardwareIntegrationData.ContainsKey("identity_link") )
			{
				initialParams.Add ("identity_link", hardwareIntegrationData["identity_link"]);
			}
#endif



            initialParams.Add ("os_version", SystemInfo.operatingSystem);
			initialParams.Add ("app_limit_tracking", appLimitAdTracking);
			
			if(!devIdBlacklist.Contains ("hardware")) {
				initialParams.Add ("device_processor", SystemInfo.processorType);
				initialParams.Add ("device_cores", SystemInfo.processorCount);
				initialParams.Add ("device_memory", SystemInfo.systemMemorySize);
				initialParams.Add ("graphics_memory_size", SystemInfo.graphicsMemorySize);
				initialParams.Add ("graphics_device_name", SystemInfo.graphicsDeviceName);
				initialParams.Add ("graphics_device_vendor", SystemInfo.graphicsDeviceVendor);
				initialParams.Add ("graphics_device_id", SystemInfo.graphicsDeviceID);
				initialParams.Add ("graphics_device_vendor_id", SystemInfo.graphicsDeviceVendorID);
				initialParams.Add ("graphics_device_version", SystemInfo.graphicsDeviceVersion);
				initialParams.Add ("graphics_shader_level", SystemInfo.graphicsShaderLevel);
			}

			// Piracy Check
			if(!devIdBlacklist.Contains ("is_genuine") && Application.genuineCheckAvailable)
				initialParams.Add("is_genuine", (Application.genuine ? "1" : "0"));
			
			// IDFA
			if(!devIdBlacklist.Contains ("idfa") && hardwareIntegrationData.ContainsKey("IDFA"))
				initialParams.Add("idfa", hardwareIntegrationData["IDFA"]);
			
			// IDFV
			if(!devIdBlacklist.Contains ("idfv") && hardwareIntegrationData.ContainsKey("IDFV"))
				initialParams.Add("idfv", hardwareIntegrationData["IDFV"]);
			
			// UDID
			if(!devIdBlacklist.Contains ("udid") && hardwareIntegrationData.ContainsKey("UDID"))
				initialParams.Add("udid", hardwareIntegrationData["UDID"]);
			
			// iad_attribution
			if(!devIdBlacklist.Contains ("iad_attribution") && hardwareIntegrationData.ContainsKey("iad_attribution"))
				initialParams.Add("iad_attribution", hardwareIntegrationData["iad_attribution"]);
			if(!devIdBlacklist.Contains ("app_purchase_date") && hardwareIntegrationData.ContainsKey("app_purchase_date"))
				initialParams.Add("app_purchase_date", hardwareIntegrationData["app_purchase_date"]);
			if(!devIdBlacklist.Contains ("iad_impression_date") && hardwareIntegrationData.ContainsKey("iad_impression_date"))
				initialParams.Add("iad_impression_date", hardwareIntegrationData["iad_impression_date"]);
			if(!devIdBlacklist.Contains ("iad_attribution_details") && hardwareIntegrationData.ContainsKey("iad_attribution_details"))
				initialParams.Add("iad_attribution_details", hardwareIntegrationData["iad_attribution_details"]);            
			
			// Android ID
			if(!devIdBlacklist.Contains ("android_id") && hardwareIntegrationData.ContainsKey("android_id"))
				initialParams.Add("android_id", hardwareIntegrationData["android_id"]);
			
			// UA (for click > install fingerprinting)
			//			if(!devIdBlacklist.Contains ("user_agent") && hardwareIntegrationData.ContainsKey("user_agent"))
			//				initialParams.Add("user_agent", hardwareIntegrationData["user_agent"]);

			// adid - Android only
			if(!devIdBlacklist.Contains ("adid") && hardwareIntegrationData.ContainsKey("adid"))
				initialParams.Add("adid", hardwareIntegrationData["adid"]);
			
			// Facebook Attribution ID
			if(!devIdBlacklist.Contains ("fb_attribution_id") && hardwareIntegrationData.ContainsKey("fb_attribution_id"))
				initialParams.Add("fb_attribution_id", hardwareIntegrationData["fb_attribution_id"]);
			
			// device limited tracking
			if (hardwareIntegrationData.ContainsKey("device_limit_tracking"))
				initialParams.Add("device_limit_tracking", hardwareIntegrationData["device_limit_tracking"]);            

			// bssid
			if(!devIdBlacklist.Contains ("bssid") && hardwareIntegrationData.ContainsKey("bssid"))
				initialParams.Add("bssid", hardwareIntegrationData["bssid"]);
			
			// carrier_name
			if(!devIdBlacklist.Contains ("carrier_name") && hardwareIntegrationData.ContainsKey("carrier_name"))
				initialParams.Add("carrier_name", hardwareIntegrationData["carrier_name"]);
			
			// volume
			if(!devIdBlacklist.Contains ("volume") && hardwareIntegrationData.ContainsKey("volume"))
				initialParams.Add("volume", hardwareIntegrationData["volume"]);
			
			// language
			if(hardwareIntegrationData.ContainsKey("language"))
				initialParams.Add("language", hardwareIntegrationData["language"]);
				
			// ids
			if(hardwareIntegrationData.ContainsKey("ids"))
				initialParams.Add("ids", hardwareIntegrationData["ids"]);
				
			// referrer data type
			if(hardwareIntegrationData.ContainsKey("conversion_type"))
				initialParams.Add("conversion_type", hardwareIntegrationData["conversion_type"]);
				
			// referrer data
			if(hardwareIntegrationData.ContainsKey("conversion_data"))
				initialParams.Add("conversion_data", hardwareIntegrationData["conversion_data"]);
			
			initialParams.Add("usertime", (UInt32)CurrentTime());
			if((UInt32)Time.time > 0)
				initialParams.Add("uptime", (UInt32)Time.time);	// Dont' use Time.realtimeSinceStartup, as it would continue incrementing when app is in background
			
			float upDelta = UptimeDelta();
			if(upDelta >= 1)
				initialParams.Add("updelta", (UInt32)upDelta);
			
		}
		catch(Exception e)
		{
			Log ("Error preparing initial event: " + e, KochLogLevel.error);
		}
		finally
		{
			_fireEvent ("initial", initialParams);

			if (retrieveAttribution == true)
			{
				int attributionWait = KOCHAVA_ATTRIBUTION_INITIAL_TIMER;

				if(PlayerPrefs.HasKey("getattribution_wait"))
				{
					string attributionWaitString = PlayerPrefs.GetString("getattribution_wait");
					attributionWait = int.Parse(attributionWaitString);
				}

				Log("Will check for attribution in: " + attributionWait);

				StartCoroutine("KochavaAttributionTimerFired",attributionWait);
			}
		}
		
		try
		{
			// set watchlist values
			Dictionary<string, object> watchlistParams = new Dictionary<string, object>();
			if (hardwareIntegrationData.ContainsKey ("device_limit_tracking"))
				watchlistParams.Add ("device_limit_tracking", hardwareIntegrationData["device_limit_tracking"].ToString() );
			
			watchlistParams.Add ("os_version", SystemInfo.operatingSystem );
			watchlistParams.Add ("app_limit_tracking", appLimitAdTracking);
			
			if (hardwareIntegrationData.ContainsKey ("language"))
				watchlistParams.Add ("language", hardwareIntegrationData ["language"].ToString ());
			
			if ( hardwareIntegrationData.ContainsKey("app_version") )
				watchlistParams.Add ("app_version", hardwareIntegrationData["app_version"].ToString() );
			else
				watchlistParams.Add ("app_version", appVersion);
			
			if ( hardwareIntegrationData.ContainsKey("app_short_string") )
				watchlistParams.Add ("app_short_string", hardwareIntegrationData["app_short_string"].ToString() );
			else
				watchlistParams.Add ("app_short_string", appVersion);

			if(!devIdBlacklist.Contains ("idfa") && hardwareIntegrationData.ContainsKey("IDFA"))
				watchlistParams.Add ("idfa", hardwareIntegrationData["IDFA"].ToString() );
			
			if(!devIdBlacklist.Contains ("adid") && hardwareIntegrationData.ContainsKey("adid"))
				watchlistParams.Add("adid", hardwareIntegrationData["adid"]);
			
			string watchlistString = JsonWriter.Serialize (watchlistParams);
			PlayerPrefs.SetString ("watchlistProperties", watchlistString);
			
			Log ("watchlistString: " + watchlistString);
		}
		catch(Exception e)
		{
			Log ("Error setting watchlist: " + e, KochLogLevel.error);
		}
		
	}
	
	#endregion

	#if !UNITY_EDITOR && UNITY_IPHONE
	#region GameKit

	void GameKitUserIdUpdate(string userInfo) 
	{
			Dictionary<string, object> gameCenterUserInfo = new Dictionary<string, object> ();

			try
			{
					gameCenterUserInfo = JsonReader.Deserialize<Dictionary<string, object>> (userInfo);
			}
			catch(Exception e)
			{
					Log ("Failed Deserialize gameKitUserInfo: " + e, KochLogLevel.warning);
			}

			if (gameCenterUserInfo.ContainsKey ("ko_gamecenter_id") || gameCenterUserInfo.ContainsKey ("ko_gamecenter_alias")) 
			{
					Dictionary<string, object> identities = new Dictionary<string, object> ();

					if (gameCenterUserInfo.ContainsKey ("ko_gamecenter_id"))
					{
							identities.Add ("ko_gamecenter_id", gameCenterUserInfo ["ko_gamecenter_id"]);
					}
					if (gameCenterUserInfo.ContainsKey ("ko_gamecenter_alias"))
					{
							identities.Add ("ko_gamecenter_alias", gameCenterUserInfo ["ko_gamecenter_alias"]);
					}

					IdentityLink (identities);
			}
	}

	#endregion
	#endif
	
	#region Watchlist Monitoring
	
	public void ScanWatchlistChanges ()
	{
		try
		{
			
			if ( PlayerPrefs.HasKey("watchlistProperties") )			{
				
				string watchlistString = PlayerPrefs.GetString("watchlistProperties");
				Log("retrieve watchlist: " + watchlistString);
				Dictionary<string, object> watchlistData = null;
				watchlistData = JsonReader.Deserialize<Dictionary<string, object>> (watchlistString);
				
				Dictionary<string, object> changeData = new Dictionary<string, object> ();
				
				// app_version
				if ( watchlistData.ContainsKey("app_version") )			{
					if ( hardwareIntegrationData.ContainsKey("app_version") )		{
						if ( watchlistData["app_version"].ToString() != hardwareIntegrationData["app_version"].ToString() )		{
							changeData.Add ("app_version", hardwareIntegrationData["app_version"].ToString() );
							watchlistData["app_version"] = hardwareIntegrationData["app_version"].ToString();
						}
					}
					else {
						if ( watchlistData["app_version"].ToString() != appVersion )	{
							changeData.Add ("app_version", appVersion );
							watchlistData["app_version"] = appVersion;
						}
					}
				}
				
				// app_short_string
				if ( watchlistData.ContainsKey("app_short_string") )			{
					
					if ( hardwareIntegrationData.ContainsKey("app_short_string") )		{
						if ( watchlistData["app_short_string"].ToString() != hardwareIntegrationData["app_short_string"].ToString() )		{
							changeData.Add ("app_short_string", hardwareIntegrationData["app_short_string"].ToString() );
							watchlistData["app_short_string"] = hardwareIntegrationData["app_short_string"].ToString();
						}
					}
					else {
						if ( watchlistData["app_short_string"].ToString() != appVersion )	{
							changeData.Add ("app_short_string", appVersion );
							watchlistData["app_short_string"] = appVersion;
						}
					}
				}
				
				// os version
				if ( watchlistData.ContainsKey("os_version") )		{
					if ( watchlistData["os_version"].ToString() != SystemInfo.operatingSystem )		{
						changeData.Add ("os_version", SystemInfo.operatingSystem );
						watchlistData["os_version"] = SystemInfo.operatingSystem;
					}
				}
				
				// language
				if ( watchlistData.ContainsKey("language") && hardwareIntegrationData.ContainsKey("language") )		{
					if ( watchlistData["language"].ToString() != hardwareIntegrationData["language"].ToString() )		{
						changeData.Add ("language", hardwareIntegrationData["language"].ToString() );
						watchlistData["language"] = hardwareIntegrationData["language"].ToString();
					}
				}
				
				// device limit tracking
				if ( watchlistData.ContainsKey("device_limit_tracking") && hardwareIntegrationData.ContainsKey("device_limit_tracking") )		{
					if ( watchlistData["device_limit_tracking"].ToString() != hardwareIntegrationData["device_limit_tracking"].ToString() )			{
						changeData.Add ("device_limit_tracking", hardwareIntegrationData["device_limit_tracking"].ToString() );
						watchlistData["device_limit_tracking"] = hardwareIntegrationData["device_limit_tracking"].ToString();
					}
				}
				
				// app limit tracking
				if ( watchlistData.ContainsKey("app_limit_tracking") )		{
					if ( bool.Parse(watchlistData["app_limit_tracking"].ToString()) != appLimitAdTracking )			{
						changeData.Add ("app_limit_tracking", appLimitAdTracking );
						watchlistData["app_limit_tracking"] = appLimitAdTracking;
					}
				}

				if(send_id_updates)
				{

					// idfa
					if(!devIdBlacklist.Contains ("idfa") && watchlistData.ContainsKey("idfa") && hardwareIntegrationData.ContainsKey("IDFA"))		{
						if ( watchlistData["idfa"].ToString() != hardwareIntegrationData["IDFA"].ToString() )			{
							changeData.Add ("idfa", hardwareIntegrationData["IDFA"].ToString() );
							watchlistData["idfa"] = hardwareIntegrationData["IDFA"].ToString();
						}
					}
					
					// adid
					if(!devIdBlacklist.Contains ("adid") && watchlistData.ContainsKey("adid") && hardwareIntegrationData.ContainsKey("adid"))		{
						if ( watchlistData["adid"].ToString() != hardwareIntegrationData["adid"].ToString() )			{
							changeData.Add ("adid", hardwareIntegrationData["adid"].ToString() );
							watchlistData["adid"] = hardwareIntegrationData["adid"].ToString();
						}
					}
				}
				// only if there are items which have changed do we need to perform updates
				if ( changeData.Count > 0 )			{
					
					string updatedWatchlistString = JsonWriter.Serialize (watchlistData);
					string changeDataString = JsonWriter.Serialize(changeData);
					
					Log("final watchlist: " + updatedWatchlistString);
					Log("changeData: " + changeDataString);
					
					PlayerPrefs.SetString ("watchlistProperties", updatedWatchlistString);
					
					// now call the update command
					_S._fireEvent ("update", changeData);
					
				}
				else {
					Log("No watchdata changed");
				}
			}
		}
		catch(Exception e)
		{
			Log ("Error scanning watchlist: " + e, KochLogLevel.error);
		}
		
	}
	
	
	#endregion
	
	#region Continous Monitoring
	
	public void Update ()
	{
		if(!Application.isPlaying)
		{
			
			// automatic app-meta update hack
			#if UNITY_EDITOR
			if(PlayerSettings.bundleVersion != "") //appVersion == "")
				appVersion = PlayerSettings.bundleVersion;
			if(appVersion == "")
				appVersion = "1.0";
			#endif
			
		} else {
			// @todo - automatically watched game state variables such as level, score, etc
			
			// Processing queue is waiting to be kickstarted at some point in the future
			if (processQueueKickstartTime != 0 && Time.time > processQueueKickstartTime) {
				processQueueKickstartTime = 0;
				StartCoroutine ("processQueue");
			}
			
		}
		
	}
	
	#endregion
	
	#region Event Tracking, Identity Linking, App Limit Ad Tracking

	static public string GetKochavaDeviceId ()
	{
		if(PlayerPrefs.HasKey("kochava_device_id"))							// most likely autoprovisioned during a previous app launch
		{
			return PlayerPrefs.GetString("kochava_device_id");
		}
		else
		{
			return "";
		}
	}

	static public void SetLimitAdTracking (bool appLimitTracking)
	{
		Kochava.AppLimitAdTracking = appLimitTracking;
		_S.ScanWatchlistChanges();
	}
	
	static public void FireEvent (Dictionary<string, object> properties)
	{
		_S._fireEvent ("event", properties);
	}
	
	static public void FireEvent (Hashtable propHash)
	{
		Dictionary<string, object> properties = new Dictionary<string, object> ();
		foreach (DictionaryEntry row in propHash)
			properties.Add((string)row.Key, (object)row.Value);
		_S._fireEvent ("event", properties);
	}
	
	static public void FireEvent (string eventName, string eventData)
	{
		if(!Kochava.EventNameBlacklist.Contains (eventName))
		{
			_S._fireEvent ("event", new Dictionary<string, object> () {
				{ "event_name", eventName },
				{ "event_data", eventData == null ? "" : eventData }
			});
		}
	}

    static public void FireEventStandard(FireEventParameters fireEventParameters)
    {
        // bail if it's null or an empty event name for some reason
        if (fireEventParameters == null || fireEventParameters.eventName == null || fireEventParameters.eventName.Length<1)
            return;        
        // stringify it ("" if null)
        string eventData = JsonWriter.Serialize(fireEventParameters.valuePayload) ?? "";
        // fire the event but mark as standard
        if (!Kochava.EventNameBlacklist.Contains(fireEventParameters.eventName))
        {
            _S._fireEvent("event", new Dictionary<string, object>() {
                { "event_name", fireEventParameters.eventName },
                { "event_data", eventData },
                { "event_standard", true.ToString() } 
            });
        }
    }

    static public void FireSpatialEvent (string eventName, float x, float y)
	{
		FireSpatialEvent (eventName, x, y, 0, "");
	}
	
	static public void FireSpatialEvent (string eventName, float x, float y, string eventData)
	{
		FireSpatialEvent (eventName, x, y, 0, eventData == null ? "" : eventData);
	}
	
	static public void FireSpatialEvent (string eventName, float x, float y, float z)
	{
		FireSpatialEvent (eventName, x, y, z, "");
	}
	
	static public void FireSpatialEvent (string eventName, float x, float y, float z, string eventData)
	{
        if (!Kochava.EventNameBlacklist.Contains(eventName))
        {
                _S._fireEvent("spatial", new Dictionary<string, object>() {
                { "event_name", eventName },
                { "event_data", eventData },
                {"x", x},
                {"y", y},
                {"z", z}
            });
        }
	}
	
	static public void IdentityLink (string key, string val)
	{
		_S._fireEvent ("identityLink", new Dictionary<string, object> () {
			{ key, val }
		});
	}
	
	static public void IdentityLink (Dictionary<string, object> identities)
	{
		_S._fireEvent ("identityLink", identities);
	}

	static public void DeeplinkEvent (string uri, string sourceApp)
	{
		_S._fireEvent ("deeplink", new Dictionary<string, object> () {
			{ "uri", uri },
			{ "source_app", sourceApp }
		});
	}

	private void _fireEvent (string eventAction, Dictionary<string, object> eventData)
	{
		if (eventData.ContainsKey("event_name") && (eventData ["event_name"] == null || eventData ["event_name"].Equals("")))
		{
			Log("Cannot create event with null/empty event name.");
			return;
		}

		Dictionary<string, object> postData = new Dictionary<string, object> ();
		
		if(!eventData.ContainsKey("usertime"))
			eventData.Add("usertime", (UInt32)CurrentTime());
		
		if(!eventData.ContainsKey("uptime") && (UInt32)Time.time > 0)
			eventData.Add("uptime", (UInt32)Time.time);	// Dont' use Time.realtimeSinceStartup, as it would continue incrementing when app is in background
		
		float upDelta = UptimeDelta();
		if(!eventData.ContainsKey("updelta") && upDelta >= 1)
			eventData.Add("updelta", (UInt32)upDelta);
		
		/*	// @TODO - This is where we add geotracking
	if(!eventData.ContainsKey("geo_lat"))
		eventData.Add("geo_lat", "");
	if(!eventData.ContainsKey("geo_lon"))
		eventData.Add("geo_lon", "");
	*/
		
		postData.Add ("action", eventAction);
		postData.Add ("data", eventData);
		
		if(eventPostingTime != 0.0f)
			postData.Add ("last_post_time", (float)eventPostingTime);
		
		if(debugMode)
			postData.Add ("debug", (bool)true);
		if(debugServer)
			postData.Add ("debugServer", (bool)true);

		bool isInitial = false;
		if (eventAction == "initial")
			isInitial = true;


		postEvent (postData, isInitial);
	}
	
	private void postEvent (Dictionary<string, object> data, bool isInitial)
	{
		QueuedEvent queuedEvent = new QueuedEvent ();
		queuedEvent.eventTime = Time.time;
		queuedEvent.eventData = data;
		
		eventQueue.Enqueue (queuedEvent);

		if (isInitial) {
			StartCoroutine ("processQueue");
		}
		else if ( eventQueue.Count >= MAX_QUEUE_SIZE) {
			StartCoroutine ("processQueue");
		}
		else {
			processQueueKickstartTime = Time.time + KVTRACKER_WAIT;
		}
	}
	
	void LocationReportCallback(string locationInfo) {

		Log ("location info: " + locationInfo);

		double currtime = CurrentTime();
		PlayerPrefs.SetString ("last_location_time", currtime.ToString());

		try
		{
			Dictionary<string, object> locationData = new Dictionary<string, object> ();
			locationData = JsonReader.Deserialize<Dictionary<string, object>> (locationInfo);

			Dictionary<string, object> changeData = new Dictionary<string, object> ();
			changeData.Add ("location", locationData );
			_S._fireEvent ("update", changeData);
		}
		catch(Exception e)
		{
			Log ("Failed Deserialize hardwareIntegrationData: " + e, KochLogLevel.warning);
		}
	}

	#endregion
	
	#region Queue Management
	
	private IEnumerator processQueue ()
	{
		// Queue should only be processed by one processQueue coroutine at a time
		if (queueIsProcessing)
			yield break;
		
		// We now have an exclusive lock on queue processing
		queueIsProcessing = true;
		
		// wait for Kvinit to provide us with AppData
		while(appData == null)
		{
			yield return new WaitForSeconds(QUEUE_KVINIT_WAIT_DELAY);
			if(appData == null)
				Log ("Event posting delayed (AppData null, kvinit handshake incomplete or Unity reloaded assemblies)", KochLogLevel.debug);
		}


		List<object> requestArray = new List<object>();
		List<object> saveArray = new List<object>();

		float postTime = Time.time;

		while (eventQueue.Count > 0) {
			
			// copy active event out of the queue
			QueuedEvent queuedEvent = eventQueue.Peek ();

			// put the event into saveArray in case it has to be put back because the network request fails
			saveArray.Add (queuedEvent);

			try
			{
				// create a copy of eventData so we can augment it without modifying data in queue (which will be reposted later if this posting fails)
				Dictionary<string, object> eventData = queuedEvent.eventData;
				
				// augment event with standardized AppData
				foreach(KeyValuePair<string, object> row in appData)
				{
					if(!eventData.ContainsKey(row.Key))
						eventData.Add(row.Key, row.Value);
				}

				requestArray.Add(eventData);

				eventQueue.Dequeue ();
			}
			catch(Exception e)
			{
				Log ("Event posting failure: " + e, KochLogLevel.error);
			}
		}

		if (requestArray.Count > 0)
		{
			string postData = JsonWriter.Serialize (requestArray);

			Log ("Posting event: " + postData.Replace("{", "{\n").Replace(",", ",\n"), KochLogLevel.debug);
			
			Dictionary<string, string> headers = new Dictionary<string, string>() {
				{"Content-Type", "application/json"},
				{"User-Agent", userAgent},
			};

			Log(postData, KochLogLevel.debug);
			WWW www = new WWW (TRACKING_URL, System.Text.Encoding.UTF8.GetBytes (postData), headers);
			yield return www;

			try
			{
				// Deserialize JSON response from server
				Dictionary<string, object> serverResponse = new Dictionary<string, object> ();
				if (www.error == null && www.text != "") {
					Log ("Server Response Received: " + WWW.UnEscapeURL(www.text), KochLogLevel.debug);
					serverResponse = JsonReader.Deserialize<Dictionary<string, object>> (www.text);
				}
				
				// Event posting failure - timeout or otherwise
				bool retry = true;
				bool success = (serverResponse.ContainsKey ("success"));
				if ( !String.IsNullOrEmpty(www.error) || !success)
				{
					_eventPostingTime = -1;
					
					if (!String.IsNullOrEmpty(www.error)) {
						Log ("Event Posting Failed: " + www.error, KochLogLevel.error);
					}
					else {
						Log ("Event Posting Did Not Succeed: " + (www.text == "" ? "(Blank response from server)" : www.text), KochLogLevel.error);
						if (serverResponse.ContainsKey ("error") || www.text == "")
							retry = false;
					}

					RequeuePostEvents(saveArray);

					// Transport success, posting failure
					if (retry == false) {
						//eventQueue.Dequeue ();
						Log ("Event posting failure, event dequeued: " + serverResponse ["error"], KochLogLevel.warning);
					}
					
					// Transport failure, wait then resend
					else
					{
						// Configure queue posting kickstarter, then kill queue posting
						processQueueKickstartTime = Time.time + POST_FAIL_RETRY_DELAY;			// Set a time in the future for processQueue to be automatically relaunched by the update loop 
						queueIsProcessing = false;
						yield break;				// Terminate queue processing for now, update loop will relaunch queue processing in POST_FAIL_RETRY_DELAY seconds
					}
					
					// Event posting success!
				} else {

					//eventQueue.Dequeue ();		// Event was posted successfully, and can now be safely removed from the queue
					_eventPostingTime = (Time.time - postTime);
					
					Log ("Event Posted (" + _eventPostingTime + " seconds to upload)");
					
					// Browser Flashing
					if (serverResponse.ContainsKey("cta") && serverResponse ["CTA"].ToString () == "1") {
						Application.OpenURL (serverResponse ["URL"].ToString ());
					}
				}
			}
			catch(Exception e)
			{
				Log ("Event posting response processing failure: " + e, KochLogLevel.error);
			}
		}


		queueIsProcessing = false;
	}

	public void RequeuePostEvents(List<object> saveArray)
	{
		for ( int i = 0; i < saveArray.Count; i ++ )
		{
			QueuedEvent queuedEvent = (QueuedEvent)saveArray[i];
			eventQueue.Enqueue (queuedEvent);
		}
	}

	public void OnApplicationPause (bool didPause)
	{
		// Register state changes (appData != null ensures we don't register a resume state during app launch)
		if(sessionTracking == KochSessionTracking.full && appData != null)
		{
			_S._fireEvent ("session", new Dictionary<string, object> () {
				{ "state", (didPause ? "pause" : "resume") }
			});
		}
		
		// Queue loading on resume is unnecessary, but it is important to save the queue on pause in case the application is exited before being resumed
		if (didPause)		{
			saveQueue ();
		}
		// if resumed, then trigger an information retrieve from the shim later which will trigger watching processing
		else {
			
			Log("received - app resume");
					
			// only proceed with these items if a kvinit response has been received
			// watchlistProperties doesn't exist until the initial has been sent
			// the initial won't be sent until the first kvinit response was received
			if (PlayerPrefs.HasKey ("watchlistProperties")) 
			{
					#if !UNITY_EDITOR && UNITY_IPHONE
					bool debugLogging = false;
					if (DebugMode)
					debugLogging = true;
					GetExternalKochavaInfo_iOS(debugLogging, incognitoMode, false, 0, 0, 0, "");
					#endif

					#if !UNITY_EDITOR && UNITY_ANDROID
					// Get Android context
					AndroidJNIHelper.debug = true;
					using(AndroidJavaClass androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
					AndroidJavaObject androidActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
					AndroidJavaObject androidContext = androidActivity.Call<AndroidJavaObject>("getApplicationContext");

					AndroidJavaClass androidHelperClass = new AndroidJavaClass("com.kochava.android.tracker.lite.KochavaSDKLite");
					androidHelperClass.CallStatic<string>("GetExternalKochavaInfo_Android", androidContext, whitelist, device_id_delay, PlayerPrefs.GetString("blacklist"), AdidSupressed);
					}
					#endif


					if(doReportLocation)
					{
							double currtime = CurrentTime();
							double lastLocationTime = double.Parse(PlayerPrefs.GetString("last_location_time"));

							if ( currtime - lastLocationTime > locationStaleness * 60 )
							{
									#if !UNITY_EDITOR && UNITY_IPHONE
									bool isLocationServicesApproved = GetExternalKochavaLocationApproved_iOS();

									if (isLocationServicesApproved)
									{
									debugLogging = false;
									if (DebugMode)
									debugLogging = true;

									GetExternalLocationReport_iOS(debugLogging,locationAccuracy,locationTimeout);
									}
									#endif
							}

					}
			}

		}
	}
	
	public void OnApplicationQuit ()
	{
		
		if(sessionTracking == KochSessionTracking.full || sessionTracking == KochSessionTracking.basic || sessionTracking == KochSessionTracking.minimal)
		{
			_S._fireEvent ("session", new Dictionary<string, object> () {
				{ "state", "quit" }
			});
		}
		
		saveQueue ();
	}
	
	private void saveQueue ()
	{
		if (eventQueue.Count > 0) {
			try
			{
				string jsonStr = JsonWriter.Serialize (eventQueue);
				PlayerPrefs.SetString (KOCHAVA_QUEUE_STORAGE_KEY, jsonStr);
				Log ("Event Queue saved: " + jsonStr, KochLogLevel.debug);
			}
			catch(Exception e)
			{
				Log ("Failure saving event queue: " + e, KochLogLevel.error);
			}
		}
	}
	
	private void loadQueue ()
	{
		try
		{
			if (PlayerPrefs.HasKey (KOCHAVA_QUEUE_STORAGE_KEY))
			{
				string jsonStr = PlayerPrefs.GetString (KOCHAVA_QUEUE_STORAGE_KEY);
				int eventsLoaded = 0;
				QueuedEvent[] jsonQueue = JsonReader.Deserialize<QueuedEvent[]> (jsonStr); 
				foreach (QueuedEvent jsonEvent in jsonQueue) {
					if (!eventQueue.Contains (jsonEvent))
					{
						eventQueue.Enqueue (jsonEvent);
						eventsLoaded++;
					}
				}
				
				Log ("Loaded (" + eventsLoaded + ") events from persistent storage", KochLogLevel.debug);
				
				PlayerPrefs.DeleteKey (KOCHAVA_QUEUE_STORAGE_KEY);
				StartCoroutine ("processQueue");
			}
		}
		catch(Exception e)
		{
			Log ("Failure loading event queue: " + e, KochLogLevel.debug);
		}
	}
	
	public static void ClearQueue ()
	{
		_S.StartCoroutine ("clearQueue");
	}
	
	private IEnumerator clearQueue ()
	{
		try
		{
			Log ("Clearing (" + eventQueueLength + ") events from upload queue...");
			_S.StopCoroutine ("processQueue");
		}
		catch(Exception e)
		{
			Log ("Failure clearing event queue: " + e, KochLogLevel.error);
		}
		
		yield return null;
		
		try
		{
			_S.queueIsProcessing = false;
			_S.eventQueue = new Queue<QueuedEvent> ();
		}
		catch(Exception e)
		{
			Log ("Failure clearing event queue: " + e, KochLogLevel.error);
		}
	}
	
	#endregion
	
	#region Ad Serving
	
	// Ad Serving
	public void GetAd (int webView, int height, int width)
	{
		Log ("Adserver Implementation Pending"); // @TODO - implement ad server
	}
	
	#endregion
	
	#region Helper Utilities

	private static string[] Chop(string value, int length)
	{
		int strLength = value.Length;
		int strCount = (strLength + length - 1) / length;
		string[] result = new string[strCount];
		for (int i = 0; i < strCount; ++i)
		{
			result[i] = value.Substring(i * length, Mathf.Min(length, strLength));
			strLength -= length;
		}
		return result;
	}
	
	private void Log (string msg)
	{
		Log (msg, KochLogLevel.warning);
	}
	
	private void Log (string msg, KochLogLevel level)
	{

		if (msg.Length > 1000) 
		{
			string[] msgResultArray = Chop (msg, 1000);
			if (level == KochLogLevel.error)
				Debug.Log ("*** Kochava Error: ");
			else if (debugMode) 
				Debug.Log ("Kochava: ");
			foreach (string s in msgResultArray) 
			{
				Debug.Log (s);
			}
		} 
		else 
		{
			if (level == KochLogLevel.error)
				Debug.Log ("*** Kochava Error: " + msg + " ***");
			else if (debugMode) 
				Debug.Log ("Kochava: " + msg);
		}
		
		if(debugMode || level == KochLogLevel.error || level == KochLogLevel.warning)
			_EventLog.Add(new LogEvent(msg, level));
		
		if(_EventLog.Count > MAX_LOG_SIZE)
			_EventLog.RemoveAt (0);
	}
	
	public static void ClearLog ()
	{
		_S._EventLog.Clear ();
	}
	
	private static readonly System.DateTime Jan1st1970 = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
	protected internal static double CurrentTime()
	{
		System.TimeSpan unix_time = (System.DateTime.UtcNow - Jan1st1970);
		return unix_time.TotalSeconds;
	}
	
	private static float uptimeDelta; 
	private static float uptimeDeltaUpdate; 
	protected internal static float UptimeDelta()
	{
		uptimeDelta = Time.time - uptimeDeltaUpdate;
		// if(uptimeDelta < .1) uptimeDelta = 1;	// Launching the app probably took more than a second...
		uptimeDeltaUpdate = Time.time;
		return uptimeDelta;
	}
	
	private string CalculateMD5Hash(string input)
	{
		try
		{
			// step 1, calculate MD5 hash from input
			System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
			byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
			byte[] hash = md5.ComputeHash(inputBytes);
			
			// step 2, convert byte array to hex string
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				// sb.Append(hash[i].ToString("X2"));	// Uppercase
				sb.Append(hash[i].ToString("x2"));		// Lowercase
			}
			return sb.ToString();
		}
		catch(Exception e)
		{
			Log ("Failure calculating MD5 hash: " + e, KochLogLevel.error);
			return "";
		}
	}
	
	private string CalculateSHA1Hash(string input)
	{
		try
		{
			byte[] hashData = new System.Security.Cryptography.SHA1Managed().ComputeHash (System.Text.Encoding.ASCII.GetBytes (input));
			string hash = string.Empty;
			foreach (var b in hashData)
				hash += b.ToString("x2");
			return hash;
		}
		catch(Exception e)
		{
			Log ("Failure calculating SHA1 hash: " + e, KochLogLevel.error);
			return "";
		}
	}
	

	public IEnumerator KochavaAttributionTimerFired(int delayTime)
	{
		yield return new WaitForSeconds(delayTime);

		Log ("attribution timer wait completed");

		Dictionary<string, object> queryData = new Dictionary<string, object>() {
			{"action", "get_attribution"},
			{"kochava_app_id", kochavaAppId },
			{"kochava_device_id", kochavaDeviceId },
			{"sdk_version", "Unity3D-" + KOCHAVA_VERSION},
			{"sdk_protocol", KOCHAVA_PROTOCOL_VERSION},
		};
		string queryString = (JsonWriter.Serialize(queryData));
		
		
		Dictionary<string, string> headers = new Dictionary<string, string>() {
			{"Content-Type", "application/xml"},
			{"User-Agent", userAgent},
		};
		Log(queryString, KochLogLevel.debug);

		float wwwLoadTime = Time.time;

		WWW www = new WWW (QUERY_URL, System.Text.Encoding.UTF8.GetBytes (queryString), headers);
		yield return www;


		// attribution request failure
		if (!String.IsNullOrEmpty(www.error)) {
			Log("kvquery (attribution) handshake failed: " + www.error + ", seconds: " + (Time.time - wwwLoadTime) + ")", KochLogLevel.warning);
			
			// retry
			StartCoroutine("KochavaAttributionTimerFired",KOCHAVA_ATTRIBUTION_DEFAULT_TIMER);
			yield break;
		}
		
		// Deserialize JSON response from server
		Dictionary<string, object> serverResponse = new Dictionary<string, object> ();
		Log("server response: " + www.text);
		if (www.text != "")	
		{
			try
			{
				serverResponse = JsonReader.Deserialize<Dictionary<string, object>> (www.text);
			}
			catch(Exception e)
			{
				Log ("Failed Deserialize JSON response to kvquery (attribution): " + e, KochLogLevel.warning);
			}
		}
		
		Log(www.text, KochLogLevel.debug);
		
		
		// parse failure
		if (!serverResponse.ContainsKey("success")) {
			
			Log("kvquery (attribution) handshake parsing failed: " + www.text, KochLogLevel.warning);
			
			// retry
			StartCoroutine("KochavaAttributionTimerFired",KOCHAVA_ATTRIBUTION_DEFAULT_TIMER);
			yield break;
		}
		
		if ( int.Parse(serverResponse["success"].ToString()) == 0) {
			Log("kvquery (attribution) did not return success = true " + www.text, KochLogLevel.warning);
			
			// retry
			StartCoroutine("KochavaAttributionTimerFired",KOCHAVA_ATTRIBUTION_DEFAULT_TIMER);
			yield break;
		}
		
		if (serverResponse.ContainsKey("data"))
		{
			Dictionary<string, object> attributionDataChunk = new Dictionary<string, object>();			
			
			try
			{
				//attributionDataChunk = JsonReader.Deserialize<Dictionary<string, object>>(attributionDataString);
				attributionDataChunk = (Dictionary<string, object>)serverResponse["data"];
			}
			catch(Exception e)
			{
				Log ("Failed Deserialize JSON attribution data chunk: " + e, KochLogLevel.warning);
			}
			
			int retry = 0;
			if (attributionDataChunk.ContainsKey("retry"))
			{
				retry = int.Parse(attributionDataChunk["retry"].ToString());
				Log ("attribution retry: " + retry, KochLogLevel.warning);
			}
			
			if (retry == -1)
			{
				if (attributionDataChunk.ContainsKey("attribution"))
				{
					String attributionString = JsonWriter.Serialize(attributionDataChunk["attribution"]);
					PlayerPrefs.SetString("attribution", attributionString);
					attributionDataStr = attributionString;
					Log ("Saved attribution chunk to persistent storage: " + attributionString, KochLogLevel.warning);
					
					// trigger callback to host app
					if (attributionCallback != null)
					    attributionCallback(attributionString);

				}
			}
			
			// this shouldn't be the case, but if so, retry at the default time
			if (retry == 0)
			{
				StartCoroutine("KochavaAttributionTimerFired",KOCHAVA_ATTRIBUTION_DEFAULT_TIMER);
			}
			
			if (retry > 0)
			{
				StartCoroutine("KochavaAttributionTimerFired",retry);
			}
		}
	}

	static public string GetAttributionData ()
	{	
		return Kochava.AttributionDataStr;
	}
	


	#endregion
	

}