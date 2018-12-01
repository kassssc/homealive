using System;
using Microsoft.Gestures.Endpoint;
using Microsoft.Gestures;
using System.Threading.Tasks;
using System.Linq;

namespace ConsoleManaged
{
		class Program
		{
				private static GesturesServiceEndpoint _gesturesService;
				private static Gesture oneFingerGesture;
				private static Gesture twoFingerGesture;
				private static Gesture threeFingerGesture;
				private static Gesture fourFingerGesture;
				private static Gesture fiveFingerGesture;

				private static string[] gesturesList =
				{
					"oneFinger",
					"twoFingers",
					"threeFingers",
					"fourFingers",
					"fiveFingers"
				};

				private static readonly string dir = System.IO.Directory.GetCurrentDirectory() + "/";
				private static readonly string configFileName = "config.homealive";
				private static Dictionary<string, LifxAPI> lightAPIs = new Dictionary<string, LifxAPI>();

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

						Console.Title = "GesturesServiceStatus[Initializing]";
						Console.WriteLine("Execute one of the following gestures: closedFist->oneFinger, twoFingers, threeFingers, fourFingers, fiveFingers\npress 'ctrl+c' to exit");

						// One can optionally pass the hostname/IP address where the gestures service is hosted
						var gesturesServiceHostName = !args.Any() ? "localhost" : args[0];
						RegisterGestures(gesturesServiceHostName).Wait();
						Console.ReadKey();

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
						}

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
						await RegisterFiveFingerGesture();
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

				private static async Task RegisterFiveFingerGesture()
				{
						// Start with defining the poses:
						var closedFist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
						var fiveFingers = new HandPose("FiveFingers", new FingerPose(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky, Finger.Thumb },
														 FingerFlexion.Open, PoseDirection.Forward));

						// Then define the gesture using the hand pose objects defined above forming a simple state machine: closedFist -> fiveFingers
						fiveFingerGesture = new Gesture("FiveFingers", closedFist, fiveFingers);
						fiveFingerGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.White);


						// Step 3: Register the gesture
						// Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be
						// detected even it was initiated not by this application or if the this application isn't in focus
						await _gesturesService.RegisterGesture(fiveFingerGesture, isGlobal: true);
				}

				private static void OnGestureDetected(object sender, GestureSegmentTriggeredEventArgs args, ConsoleColor foregroundColor)
				{
						LightListResponse response;

						string gesture = args.GestureSegment.Name;

						response = lightAPIs[gesture].Toggle();

						Console.ForegroundColor = ConsoleColor.Red;
						Console.Write("Gesture detected! : ");
						Console.ForegroundColor = foregroundColor;
						Console.WriteLine(gesture);
						Console.ResetColor();

				}

				private static void ConfigRoutine()
				{
						lightAPIs = new Dictionary<string, LifxAPI>();
						List<Light> lights = LifxAPI.GetLightStatus().Results;
						Console.WriteLine(lights.Count + " light(s) found");
						Console.WriteLine("Assigning lights to gestures...");

						string[] configFileLines = new string[lights.Count];
						for (int i = 0; i < lights.Count; i++)
						{
								Console.Write("{0}\t to command: ", lights[i].Label);
								string assignedGesture = Console.ReadLine();
								while(!IsValidGestureName(assignedGesture))
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
								Console.WriteLine("Toggle Command: {1}\tLight Label: {0}", lightConfig.Key, lightConfig.Value.Label);
						}
				}

				private static bool IsValidGestureName(string name)
				{
					return gesturesList.IndexOf(name, value) != -1;
				}
		}
}