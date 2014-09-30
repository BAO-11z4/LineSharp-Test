using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Web.Http;
using LineSharp;
using NLog;
using System.Timers;
namespace LineSharp.Controller
{
    public class RequestParam
    {
        public LineFunction function { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public List<string> to { get; set; }
        public string message { get; set; }
        public string findContact { get; set; }
    }

    public enum LineFunction
    {
        Login = 1,
        GetContacts = 2,
        FindContact = 3,
        SendMessage = 4
    }

    public class LineController: ApiController
    {
        private Timer timer;
        private static LineClient line;
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static String msg = String.Empty;

        /// <summary>
        /// 建立新的Client
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public dynamic Client(RequestParam param)
        {
            if (param == null) return false;
            string user = String.Empty;
            string password = String.Empty;
            switch (param.function)
            {
                case LineFunction.GetContacts:
                    return this.getContacts();
                case LineFunction.Login:
                    return this.login(param.user, param.password);
                case LineFunction.FindContact:
                    return this.findContact(param.findContact);
                case LineFunction.SendMessage:
                    return this.sendMessage(param.to, param.message);
                default:
                    return false;
            }
        }
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool login(string user, string password) {
            if (line != null) line.Logout();
            if (timer != null) timer.Dispose();

            line = new LineClient();
            line.OnLogin += new LineClient.LoggedInEvent((Result loginResult) =>
            {
                //Everything worked
                if (loginResult == Result.OK)
                {
                    msg = "Authed successfully!";
                    logger.Trace(msg);
                    //Console.WriteLine(msg);
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
                    
                    msg = "Verify your account on your mobile device using PIN: " + pin + ". It times out after about 3 minutes.";
                    logger.Trace(msg);

                    //Then call this function, then enter the pin on the mobile device.
                    line.VerifyPin();
                    //WARNING: This function will hang until the pin verifies.
                }
                else
                {
                    msg = "Did not auth successfully. Paused.";
                    logger.Trace(msg);
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

                    line.Login(user, password, verifierToken);
                    // :P
                }

            });

            line.OnReceiveMessage += (o, eventArgs) =>
            {
                logger.Trace(eventArgs.Message.Text);
                line.SendMessage(eventArgs.Message.From, @"我收到您的訊息了, 稍候給您回覆" + Environment.NewLine + @"謝謝");
            };

            line.OnNotifiedReceivedCall += (o, eventArgs) =>
            {
                logger.Trace("[LineSharp] Got a call from " + line.GetContact(eventArgs.Operation.Param1).Name);
                line.SendMessage(eventArgs.Operation.Param1, @"很抱歉, 我無法接聽電話" + Environment.NewLine + @"請留言");

            };

            //line.Login("wii.at.android@gmail.com", "chungyih", "qg2tNEsgGGgfTyiuL2IjNxjTAJpW7R0d");
            line.Login(user, password);

            timer = new Timer();
            timer.Elapsed += timer_Elapsed;
            timer.Interval = 100;
            timer.Start();
            return true;
        }

        /// <summary>
        /// 取得聯絡人列表
        /// </summary>
        /// <returns></returns>
        private List<LineSharp.Common.Contact> getContacts()
        {
            return line.GetContacts(line.GetContactIDs());
        }

        /// <summary>
        /// 依contactid尋找Contact
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private LineSharp.Common.Contact findContact(string id)
        {
            return line.GetContact(id);
        }

        private bool sendMessage(List<string> contacts, string message)
        {
            if (contacts == null) return false;
            if (String.IsNullOrEmpty(message)) return false;
            foreach (string ct in contacts)
            {
                try
                {
                    line.SendMessage(ct, message);
                }
                catch (Exception e)
                {
                    logger.Error("Send message failed, to ID: " + ct + ", " + e.Message);
                }
            }
            return true;
        }
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            while (true) line.Update();
        }
    }
}
