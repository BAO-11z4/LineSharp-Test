//Author MID: #REDACTED
using System;
using System.Data.Odbc;
using LineSharp;
using LineSharp.Json_Datatypes;
using System.Collections.Generic;

using System.Web;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace LineSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:8080");

            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{id}",
                new { id = RouteParameter.Optional });

            using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }

            /* 以下為原始程式
            LineClient line = new LineClient();
            line.OnLogin += new LineClient.LoggedInEvent((Result loginResult) =>
            {
                    //Everything worked
                if (loginResult == Result.OK)
                {
                    Console.WriteLine("Authed successfully!");
                    List<Common.Contact> contacts = line.GetContacts(line.GetContactIDs());
                    foreach (Common.Contact ct in contacts.FindAll(x => x.Name.ToLower().Contains("wii")))
                    {
                        line.SendMessage(ct.ID, "我登入了系統");
                    }
                }
                    //Phone verification needed
                else if (loginResult == Result.REQUIRES_PIN_VERIFICATION)
                {
                    //The user then is required to enter the pin (retrieved from calling
                    string pin = line.Pin;
                    //)

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Verify your account on your mobile device using PIN: " + pin);
                    Console.WriteLine("It times out after about 3 minutes.");
                    Console.ForegroundColor = ConsoleColor.White;

                    //Then call this function, then enter the pin on the mobile device.
                    line.VerifyPin();
                    //WARNING: This function will hang until the pin verifies.
                }
                else
                {
                    Console.WriteLine("Did not auth successfully. Paused.");
                    Console.Read();
                    Environment.Exit(0);
                }

                
            });

            line.OnPinVerified += new LineClient.PinVerifiedEvent((Result pinVerifiedResult, string verifierToken) =>
            {
                //The pin was verified, or it had timed out???
                if (pinVerifiedResult == Result.OK)
                {
                    //Success. Log in using this. After logging in this way, if there's a certificate, you should
                    //save that somewhere and use it to log in again, because apparently it's nice not to have to 
                    //verify your pin every single time. I'll implement logging in and using a cert later though.

                    line.Login("wii.at.android@gmail.com", "chungyih", verifierToken);
                    // :P
                }

            });

            line.OnReceiveMessage += (o, eventArgs) =>
            {
                Console.WriteLine(eventArgs.Message.Text);
                line.SendMessage(eventArgs.Message.From, @"我收到您的訊息了, 稍候給您回覆" + Environment.NewLine + @"謝謝");
            };

            line.OnNotifiedReceivedCall += (o, eventArgs) =>
            {
                Console.WriteLine("[LineSharp] Got a call from " + line.GetContact(eventArgs.Operation.Param1).Name);
                line.SendMessage(eventArgs.Operation.Param1, @"很抱歉, 我無法接聽電話" + Environment.NewLine + @"請留言");
                
            };

            //line.Login("wii.at.android@gmail.com", "chungyih", "qg2tNEsgGGgfTyiuL2IjNxjTAJpW7R0d");
            line.Login("wii.at.android@gmail.com", "chungyih");
            while (true) line.Update();
            //Console.Read();
            //Line.Logout();
            */
        }
    }
}