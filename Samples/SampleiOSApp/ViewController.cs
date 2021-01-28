using Foundation;
using System;
using System.Runtime.InteropServices;
using UIKit;

namespace SampleiOSApp
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static extern IntPtr Intptr_objc_msgSend(IntPtr receiver, IntPtr selector);


        public static string Ping()
        {
            var c = new ObjCRuntime.Class("Example");
            var s = new ObjCRuntime.Selector("Ping");

            var r = Intptr_objc_msgSend(c.Handle, s.Handle);

            return NSString.FromHandle(r).ToString();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.


            var pong = Ping();

            Console.WriteLine(pong);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}