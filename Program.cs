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
        private static Gesture index_gesture;
        private static Gesture twoFingers_gesture;
        private static Gesture ThreeFingers_gesture;
        private static Gesture FourFingers_gesture;
        private static Gesture FiveFingers_gesture;


        static void Main(string[] args)
        {       
            Console.Title = "GesturesServiceStatus[Initializing]";
            Console.WriteLine("Execute one of the following gestures: Like, Drop-the-Mic, Rotate-Right ! press 'ctrl+c' to exit");

            // One can optionally pass the hostname/IP address where the gestures service is hosted
            var gesturesServiceHostName = !args.Any() ? "localhost" : args[0];
            RegisterGestures(gesturesServiceHostName).Wait();
            Console.ReadKey();
        }

        private static async Task RegisterGestures(string gesturesServiceHostName)
        {
            // Step 1: Connect to Microsoft Gestures service            
            _gesturesService = GesturesServiceEndpointFactory.Create(gesturesServiceHostName);            
            _gesturesService.StatusChanged += (s, arg) => Console.Title = $"GesturesServiceStatus [{arg.Status}]";            
            await _gesturesService.ConnectAsync();

            // Step 2: Define bunch of custom Gestures, each detection of the gesture will emit some message into the console
            await RegisterIndexGesture();
            await RegisterTwoFingersGesture();
            await RegisterThreeFingersGesture();
            await RegisterFourFingersGesture();
            await RegisterFiveFingersGesture();


        }

        private static async Task RegisterIndexGesture()
        {
            // Start with defining the first pose, ...
            var closed_fist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
            var index = new HandPose("Index", new FingerPose(new[] { Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
                                               new FingerPose(new[] { Finger.Thumb,Finger.Middle,Finger.Ring,Finger.Pinky }, FingerFlexion.Folded));

            // ... finally define the gesture using the hand pose objects defined above forming a simple state machine: hold -> rotate
            index_gesture = new Gesture("Index", closed_fist, index);
            index_gesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

            // Step 3: Register the gesture             
            // Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be 
            // detected even it was initiated not by this application or if the this application isn't in focus
           await _gesturesService.RegisterGesture(index_gesture, isGlobal: true);
        }

        private static async Task RegisterTwoFingersGesture()
        {
            // Start with defining the first pose, ...
            var closed_fist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
            var twoFingers = new HandPose("TwoFingers", new FingerPose(new[] { Finger.Index, Finger.Middle }, FingerFlexion.Open, PoseDirection.Forward),
                            new FingerPose(new[] { Finger.Thumb, Finger.Ring, Finger.Pinky }, FingerFlexion.Folded));

            // ... finally define the gesture using the hand pose objects defined above forming a simple state machine: hold -> rotate*/
            twoFingers_gesture = new Gesture("TwoFingers", closed_fist, twoFingers);
            twoFingers_gesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

            // Step 3: Register the gesture             
            // Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be 
            // detected even it was initiated not by this application or if the this application isn't in focus
            await _gesturesService.RegisterGesture(twoFingers_gesture, isGlobal: true);
        }

        private static async Task RegisterThreeFingersGesture()
        {
            // Start with defining the first pose, ...
            var closed_fist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
            var ThreeFingers = new HandPose("ThreeFingers", new FingerPose(new[] { Finger.Index, Finger.Middle ,Finger.Ring}, FingerFlexion.Open, PoseDirection.Forward),
                            new FingerPose(new[] { Finger.Thumb, Finger.Pinky }, FingerFlexion.Folded));

            // ... finally define the gesture using the hand pose objects defined above forming a simple state machine: hold -> rotate*/
            ThreeFingers_gesture = new Gesture("ThreeFingers", closed_fist, ThreeFingers);
            ThreeFingers_gesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

            // Step 3: Register the gesture             
            // Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be 
            // detected even it was initiated not by this application or if the this application isn't in focus
            await _gesturesService.RegisterGesture(ThreeFingers_gesture, isGlobal: true);
        }

        private static async Task RegisterFourFingersGesture()
        {
            // Start with defining the first pose, ...
            var closed_fist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
            var FourFingers = new HandPose("FourFingers", new FingerPose(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky }, FingerFlexion.Open, PoseDirection.Forward),
                            new FingerPose(new[] { Finger.Thumb }, FingerFlexion.Folded));

            // ... finally define the gesture using the hand pose objects defined above forming a simple state machine: hold -> rotate*/
            FourFingers_gesture = new Gesture("FourFingers", closed_fist, FourFingers);
            FourFingers_gesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

            // Step 3: Register the gesture             
            // Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be 
            // detected even it was initiated not by this application or if the this application isn't in focus
            await _gesturesService.RegisterGesture(FourFingers_gesture, isGlobal: true);
        }

        private static async Task RegisterFiveFingersGesture()
        {
            // Start with defining the first pose, ...
            var closed_fist = new HandPose("ClosedFist", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
            var FiveFingers = new HandPose("FiveFingers", new FingerPose(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky, Finger.Thumb }, FingerFlexion.Open, PoseDirection.Forward));

            // ... finally define the gesture using the hand pose objects defined above forming a simple state machine: hold -> rotate*/
            FiveFingers_gesture = new Gesture("FiveFingers", closed_fist, FiveFingers);
            FiveFingers_gesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

            // Step 3: Register the gesture             
            // Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be 
            // detected even it was initiated not by this application or if the this application isn't in focus
            await _gesturesService.RegisterGesture(FiveFingers_gesture, isGlobal: true);
        }

        private static void OnGestureDetected(object sender, GestureSegmentTriggeredEventArgs args, ConsoleColor foregroundColor)
        {

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Gesture detected! : ");
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(args.GestureSegment.Name);
            Console.ResetColor();
        }
    }
}
