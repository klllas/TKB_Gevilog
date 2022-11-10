using System;
using System.Collections.Generic;
using System.Text;
using GEUTEBRUECK.GeViSoftSDKNET.ActionsWrapper;
using GEUTEBRUECK.GeViSoftSDKNET.ActionsWrapper.ActionDispatcher;
using GEUTEBRUECK.GeViSoftSDKNET.ActionsWrapper.SystemActions;
using System.Threading;
using System.Timers;

namespace CS_Console_Client
{
    class Program
    {
        static private void myConnectProgress(object sender, GeViSoftConnectProgressEventArgs e)
        {
            Console.WriteLine("Connecting... {0} of {1}", e.Progress, e.Effort);
        }

        static private void myDatabaseCallback(object sender, GeViSoftDatabaseNotificationEventArgs e)
        {
            switch (e.ServerNotificationType)
            {
                case GeViServerNotification.NFServer_GoingShutdown:
                    Console.WriteLine("GeViServer is shutting down.");
                    break;
                case GeViServerNotification.NFServer_Disconnected:
                    Console.WriteLine("This client has been disconnected.");
                    break;
                case GeViServerNotification.NFServer_SetupModified:
                    Console.WriteLine("GeViServer setup has been modified.");
                    break;
                case GeViServerNotification.NFServer_NewMessage:
                    Console.WriteLine("Prijata udalost")
                    break;
            }
        }

        static private void myCustomActionCallback(object sender, GeViAct_CustomActionEventArgs e)
        {
            Console.WriteLine("Received CustomAction({0}, {1})", e.aCustomInt, e.aCustomText);
        }

        static private void registerCallbacks()
        {
            myDB.ConnectProgress += new GeViSoftConnectProgressEventHandler(myConnectProgress);
            myDB.DatabaseNotification += new GeViSoftDatabaseNotificationEventHandler(myDatabaseCallback);
            myDB.ReceivedCustomAction += new GeViAct_CustomActionEventHandler(myCustomActionCallback);
            myDB.ReceivedEventStarted += new GeViAct_EventStartedEventHandler(myDb_ReceivedEventStarted);
            myDB.RegisterCallback();
        }


        static void myDb_ReceivedEventStarted(object sender, GeViAct_EventStartedEventArgs e)
        {
        }


        static private GeViDatabase myDB;
        static volatile private bool establishConnection;

        public static bool IsConnected()
        {
            bool status = false;
            if (myDB != null)
            {
                status = myDB.SendPing();
            }
            return status;
        }

        public static void ConnectionHandler()
        {
            while (establishConnection)
            {
                bool connectionEstablished = false;
                myDB = new GeViDatabase();
                myDB.Create("192.168.82.10", "sysadmin", "masterkey");
                registerCallbacks();
                GeViConnectResult res = myDB.Connect();
                if (res == GeViConnectResult.connectOk)
                {
                    connectionEstablished = true;
                    Console.WriteLine("Connected to GeViSoft");
                }
                for (; ; )
                {
                    System.Threading.Thread.Sleep(1000);
                    if (myDB != null)
                    {
                        if ((connectionEstablished))
                        {
                            connectionEstablished = myDB.SendPing();
                        }
                        else
                        {
                            Console.WriteLine("No Connection... Trying to reconnect!");
                            if (GeViConnectResult.connectOk == myDB.Connect())
                            {
                                connectionEstablished = true;
                                Console.WriteLine("Connected to GeViSoft");
                            }
                        }
                    }
                    else //myDB == null
                    {
                    }
                }
            }
            if (myDB != null)
            {
                myDB.Disconnect();
                myDB.Dispose();
            }
        }

        static void Main(string[] args)
        {
            int x = 0;
            establishConnection = true;
            Thread ConnectionHandlerThread = new Thread(ConnectionHandler);
            ConnectionHandlerThread.Start();
            while (true)
            {
                Thread.Sleep(5000);
                if (IsConnected())
                {
                    GeViAction myAction = new GeViAct_CustomAction(x++, "alive!");
                    myDB.SendMessage(myAction);
                }
            }
        }
    }
}
