using Microsoft.Gestures;
using Microsoft.Gestures.Endpoint;
using Microsoft.Gestures.Stock.HandPoses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ConsoleManaged
{
	class Program
	{
		private static GesturesServiceEndpoint _gesturesService;

		private static Gesture oneFingerGesture;
		private static Gesture twoFingerGesture;
		private static Gesture threeFingerGesture;
		private static Gesture fourFingerGesture;

		private static Gesture openGesture;
		private static Gesture closeGesture;

		private static Gesture rotateGestureUp1;
		private static Gesture rotateGestureUp2;
		private static Gesture rotateGestureDown1;
		private static Gesture rotateGestureDown2;

		private static string[] gesturesList =
		{
			"OneFinger",
			"TwoFingers"
		};

		private static readonly string dir = System.IO.Directory.GetCurrentDirectory() + "/";
		private static readonly string configFileName = "config.homealive";
		private static Dictionary<string, LifxAPI> lightAPIs = new Dictionary<string, LifxAPI>();
        private static string gesturesServiceHostName;

        // Stores all the lights in our system
        // Stores with light label as key and LifxAPI object as value
        // Each light in the system will have its own LifxAPI object
        private static Dictionary<string, LifxAPI> lights;

		static void Main(string[] args)
		{
			if (!System.IO.File.Exists(dir + configFileName))
			{
				ConfigRoutine();
			}
			else
			{
				LoadConfig();
			}

			//Console.Title = "GesturesServiceStatus[Initializing]";
			//Console.WriteLine("Execute one of the following gestures: closedFist->oneFinger, twoFingers, threeFingers, fourFingers, fiveFingers\npress 'ctrl+c' to exit");
			PrintConfigInfo();
			// One can optionally pass the hostname/IP address where the gestures service is hosted
			gesturesServiceHostName = !args.Any() ? "localhost" : args[0];

			//
			// Main Program Loop
			//
			while (true)
			{
				Console.WriteLine("Gesture detection started...\nPress 'esc' to pause gesture detection and show menu");

				StartGestureDetection();

				ConsoleKeyInfo keyPressed;
				do {
					keyPressed = Console.ReadKey(true);
				} while (keyPressed.Key != ConsoleKey.Escape);

				StopGestureDetection();

				//
				// Menu Loop
				//
				while (true)
				{
					string userInput;
					Console.Write("Enter a command - ");
					userInput = Console.ReadLine();
					string[] commandArray = userInput.Split(null);

					if (userInput == "q")
					{
						Environment.Exit(0);
					}
					if (userInput == "config" || userInput == "c")
					{
						ConfigRoutine();
					}
					else if (userInput == "help" || userInput == "h")
					{
						PrintConfigInfo();
					}
					else if (userInput == "status")
					{
						LightListResponse response = LifxAPI.GetLightStatus();
					}
					else if (userInput == "start" || userInput == "s")
					{
						break;
					}
					else {
						Console.WriteLine("Invalid input");
					}
				}

			}
		}

		private static void OnGestureDetected(object sender, GestureSegmentTriggeredEventArgs args, ConsoleColor foregroundColor)
		{
			LightListResponse response;

			string gesture = args.GestureSegment.Name;
			if (gesture == "RotateUp1"){
				response = lightAPIs["Onefinger"].BrightnessUp();
			}
			else if (gesture == "RotateDown1") {
				response = lightAPIs["Onefinger"].BrightnessDown();
			}
			else if (gesture == "RotateUp2") {
				response = lightAPIs["TwoFingers"].BrightnessUp();
			}
			else if (gesture == "RotateDown2") {
				response = lightAPIs["TwoFingers"].BrightnessDown();
			}
			else if (lightAPIs.ContainsKey(gesture))
			{
				response = lightAPIs[gesture].Toggle();
			}
			else if (gesture == "Open")
			{
				LifxAPI.TurnOnAll();
			}
			else if (gesture == "Close")
			{
				LifxAPI.TurnOffAll();
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("Gesture detected! : ");
			Console.ForegroundColor = foregroundColor;
			Console.WriteLine(gesture);
			Console.ResetColor();
		}

		private static void ConfigRoutine()
		{
			lightAPIs = new Dictionary<string, LifxAPI>();
            try
            {
                List<Light> lights = LifxAPI.GetLightStatus().Results;
                Console.WriteLine(lights.Count + " light(s) found");
                Console.WriteLine("Assigning lights to gestures...");

                string[] configFileLines = new string[lights.Count];
                for (int i = 0; i < lights.Count; i++)
                {
                    Console.Write("{0}\t to command: ", lights[i].Label);
                    string assignedGesture = Console.ReadLine();
                    while (!IsValidGestureName(assignedGesture))
                    {
                        Console.Write("Invalid gesture assignment, please try again: ");
                        assignedGesture = Console.ReadLine();
                    }
                    configFileLines[i] = lights[i].Label + "\t" + assignedGesture;

                }
                Console.WriteLine(dir + configFileName);
                System.IO.File.WriteAllLines(dir + configFileName, configFileLines);
                Console.WriteLine("Configuration Complete!");
                LoadConfig();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

		}

		private static void LoadConfig()
		{
			string[] configFileLines = System.IO.File.ReadAllLines(dir + configFileName);
			foreach (string line in configFileLines)
			{
				string[] splitLine = line.Split("\t");
				lightAPIs.Add(splitLine[1], new LifxAPI(splitLine[0]));
			}
		}

		private static void PrintConfigInfo()
		{
			foreach (KeyValuePair<string, LifxAPI> lightConfig in lightAPIs)
			{
				Console.WriteLine("Toggle Command: {0}\tLight Label: {1}", lightConfig.Key, lightConfig.Value.Label);
			}
		}

		private static string SelectLight(string gestureName)
		{
            return "";
		}

		private static bool IsValidGestureName(string name)
		{
			return Array.IndexOf(gesturesList, name) != -1;
		}

		private static void StartGestureDetection()
		{
			RegisterGestures(gesturesServiceHostName).Wait();
		}

		private static void StopGestureDetection()
		{
			_gesturesService.Disconnect();
		}

		private static async Task RegisterGestures(string gesturesServiceHostName)
		{
			// Step 1: Connect to Microsoft Gestures service
			_gesturesService = GesturesServiceEndpointFactory.Create(gesturesServiceHostName);
			_gesturesService.StatusChanged += (s, arg) => Console.Title = $"GesturesServiceStatus [{arg.Status}]";
			await _gesturesService.ConnectAsync();

			// Step 2: Define bunch of custom Gestures, each detection of the gesture will emit some message into the console
			await RegisterOneFingerGesture();
			await RegisterTwoFingerGesture();
			await RegisterThreeFingerGesture();
			await RegisterFourFingerGesture();
			await RegisterOpenGesture();
			await RegisterCloseGesture();
			await RegisterRotateUp1Gesture();
			await RegisterRotateUp2Gesture();
			await RegisterRotateDown1Gesture();
			await RegisterRotateDown2Gesture();
		}

		private static async Task RegisterOneFingerGesture()
		{
			// Start with defining the poses:
			var closedFist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
			var oneFinger = new HandPose("OneFinger", new FingerPose(new[] { Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
											  new FingerPose(new[] { Finger.Thumb, Finger.Middle, Finger.Ring, Finger.Pinky }, FingerFlexion.Folded));

			// Then define the gesture using the hand pose objects defined above forming a simple state machine: closedFist -> oneFinger
			oneFingerGesture = new Gesture("OneFinger", closedFist, oneFinger);
			oneFingerGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

			// Step 3: Register the gesture
			// Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be
			// detected even it was initiated not by this application or if the this application isn't in focus
			await _gesturesService.RegisterGesture(oneFingerGesture, isGlobal: true);
		}

		private static async Task RegisterTwoFingerGesture()
		{
			// Start with defining the poses:
			var closedFist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
			var twoFingers = new HandPose("TwoFingers", new FingerPose(new[] { Finger.Index, Finger.Middle }, FingerFlexion.Open, PoseDirection.Forward),
							 new FingerPose(new[] { Finger.Thumb, Finger.Ring, Finger.Pinky }, FingerFlexion.Folded));

			// Then define the gesture using the hand pose objects defined above forming a simple state machine: closedFist -> twoFingers
			twoFingerGesture = new Gesture("TwoFingers", closedFist, twoFingers);
			twoFingerGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Blue);

			// Step 3: Register the gesture
			// Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be
			// detected even it was initiated not by this application or if the this application isn't in focus
			await _gesturesService.RegisterGesture(twoFingerGesture, isGlobal: true);
		}

		private static async Task RegisterThreeFingerGesture()
		{
			// Start with defining the poses:
			var closedFist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
			var threeFingers = new HandPose("ThreeFingers", new FingerPose(new[] { Finger.Index, Finger.Middle, Finger.Ring }, FingerFlexion.Open, PoseDirection.Forward),
							 new FingerPose(new[] { Finger.Thumb, Finger.Pinky }, FingerFlexion.Folded));

			// Then define the gesture using the hand pose objects defined above forming a simple state machine: closedFist -> threeFingers
			threeFingerGesture = new Gesture("ThreeFingers", closedFist, threeFingers);
			threeFingerGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Cyan);

			// Step 3: Register the gesture
			// Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be
			// detected even it was initiated not by this application or if the this application isn't in focus
			await _gesturesService.RegisterGesture(threeFingerGesture, isGlobal: true);
		}

		private static async Task RegisterFourFingerGesture()
		{
			// Start with defining the poses:
			var closedFist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
			var fourFingers = new HandPose("FourFingers", new FingerPose(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky }, FingerFlexion.Open, PoseDirection.Forward),
							  new FingerPose(new[] { Finger.Thumb }, FingerFlexion.Folded));

			// Then define the gesture using the hand pose objects defined above forming a simple state machine: closedFist -> fourFingers
			fourFingerGesture = new Gesture("FourFingers", closedFist, fourFingers);
			fourFingerGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Green);

			// Step 3: Register the gesture
			// Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be
			// detected even it was initiated not by this application or if the this application isn't in focus
			await _gesturesService.RegisterGesture(fourFingerGesture, isGlobal: true);
		}

		private static async Task RegisterOpenGesture()
		{
			// Start with defining the poses:
			var closedFist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
			var fiveFingers = new HandPose("FiveFingers", new FingerPose(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky, Finger.Thumb },
							 FingerFlexion.Open, PoseDirection.Forward));

			// Then define the gesture using the hand pose objects defined above forming a simple state machine: closedFist -> fiveFingers
			openGesture = new Gesture("Open", closedFist, fiveFingers);
			openGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.White);


			// Step 3: Register the gesture
			// Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be
			// detected even it was initiated not by this application or if the this application isn't in focus
			await _gesturesService.RegisterGesture(openGesture, isGlobal: true);
		}

		private static async Task RegisterCloseGesture()
		{
			// Start with defining the poses:
			var closedFist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
			var fiveFingers = new HandPose("FiveFingers", new FingerPose(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky, Finger.Thumb },
							 FingerFlexion.Open, PoseDirection.Forward));

			// Then define the gesture using the hand pose objects defined above forming a simple state machine: closedFist -> fiveFingers
			closeGesture = new Gesture("Close", fiveFingers, closedFist);
			closeGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.White);


			// Step 3: Register the gesture
			// Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be
			// detected even it was initiated not by this application or if the this application isn't in focus
			await _gesturesService.RegisterGesture(closeGesture, isGlobal: true);
		}


		private static async Task RegisterRotateUp1Gesture()
		{
			// Start with defining the first pose, ...
			var hold = new HandPose("Hold", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
											new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
											new FingerPose(new[] { Finger.Middle, Finger.Ring, Finger.Pinky }, FingerFlexion.Folded),
											new FingertipPlacementRelation(Finger.Index, RelativePlacement.Above, Finger.Thumb));
			// ... define the second pose, ...
			var rotate = new HandPose("Rotate", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
												new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
												new FingerPose(new[] { Finger.Middle, Finger.Ring, Finger.Pinky }, FingerFlexion.Folded),
												new FingertipPlacementRelation(Finger.Index, RelativePlacement.Right, Finger.Thumb));//change to left

			// ... finally define the gesture using the hand pose objects defined above forming a simple state machine: hold -> rotate
			rotateGestureUp1 = new Gesture("RotateUp1", hold, rotate);
			rotateGestureUp1.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

			// Step 3: Register the gesture
			// Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be
			// detected even it was initiated not by this application or if the this application isn't in focus
			await _gesturesService.RegisterGesture(rotateGestureUp1, isGlobal: true);
		}

		private static async Task RegisterRotateDown1Gesture()
		{
			// Start with defining the first pose, ...
			var hold = new HandPose("Hold", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
											new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
											new FingerPose(new[] { Finger.Middle, Finger.Ring, Finger.Pinky }, FingerFlexion.Folded),
											new FingertipPlacementRelation(Finger.Index, RelativePlacement.Above, Finger.Thumb));
			// define the second pose, ...
			var rotate = new HandPose("Rotate1", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
												 new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
												 new FingerPose(new[] { Finger.Middle, Finger.Ring, Finger.Pinky }, FingerFlexion.Folded),
												 new FingertipPlacementRelation(Finger.Index, RelativePlacement.Left, Finger.Thumb));//change to left

			// finally define the gesture using the hand pose objects defined above forming a simple state machine: hold -> rotate
			rotateGestureDown1 = new Gesture("RotateDown1", hold, rotate);
			rotateGestureDown1.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

			// Step 3: Register the gesture
			// Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be
			// detected even it was initiated not by this application or if the this application isn't in focus
			await _gesturesService.RegisterGesture(rotateGestureDown1, isGlobal: true);
		}

		private static async Task RegisterRotateUp2Gesture()
		{
			// Start with defining the first pose, ...
			var hold = new HandPose("Hold", new FingerPose(new[] { Finger.Thumb, Finger.Index, Finger.Middle }, FingerFlexion.Open, PoseDirection.Forward),
											new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
											new FingertipPlacementRelation(Finger.Index, RelativePlacement.Above, Finger.Thumb),
											new FingertipPlacementRelation(Finger.Middle, RelativePlacement.Above, Finger.Thumb));
			// ... define the second pose, ...
			var rotate = new HandPose("Rotate2", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
												 new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
												 new FingertipPlacementRelation(Finger.Index, RelativePlacement.Right, Finger.Thumb),
												 new FingertipPlacementRelation(Finger.Middle, RelativePlacement.Right, Finger.Thumb));

			// ... finally define the gesture using the hand pose objects defined above forming a simple state machine: hold -> rotate
			rotateGestureUp2 = new Gesture("RotateUp2", hold, rotate);
			rotateGestureUp2.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

			// Step 3: Register the gesture
			// Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be
			// detected even it was initiated not by this application or if the this application isn't in focus
			await _gesturesService.RegisterGesture(rotateGestureUp2, isGlobal: true);
		}

		private static async Task RegisterRotateDown2Gesture()
		{
			// Start with defining the first pose, ...
			var hold = new HandPose("Hold", new FingerPose(new[] { Finger.Thumb, Finger.Index, Finger.Middle }, FingerFlexion.Open, PoseDirection.Forward),
											new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
											new FingertipPlacementRelation(Finger.Index, RelativePlacement.Above, Finger.Thumb),
											new FingertipPlacementRelation(Finger.Middle, RelativePlacement.Above, Finger.Thumb));
			// ... define the second pose, ...
			var rotate = new HandPose("Rotate2", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
												 new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
												 new FingertipPlacementRelation(Finger.Index, RelativePlacement.Left, Finger.Thumb),
												 new FingertipPlacementRelation(Finger.Middle, RelativePlacement.Left, Finger.Thumb));

			// ... finally define the gesture using the hand pose objects defined above forming a simple state machine: hold -> rotate
			rotateGestureUp2 = new Gesture("RotateDown2", hold, rotate);
			rotateGestureUp2.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

			// Step 3: Register the gesture
			// Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be
			// detected even it was initiated not by this application or if the this application isn't in focus
			await _gesturesService.RegisterGesture(rotateGestureUp2, isGlobal: true);
		}
	}
}