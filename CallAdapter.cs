///Author - Sulakshman Kumar Yellapu
///Description - Send meter event response , meter voltage response and meter ping response to the OMS system

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdmsAdapter;
using NetUtil;
using System.Configuration;
using AdapterSamples;
using System.Data;
using System.Collections;
using System.Web.Script.Serialization;
using AdapterSamples.model;
using AdapterSamples.GenusMeterService;
using System.Globalization;
using System.ServiceModel.Diagnostics;
using System.Timers;
using System.IO;


namespace ApapterSamples
{

    /// <summary>
    /// This class demonstrations how to use IdmsConnector to connect to Call Server.
    /// 
    /// </summary>
    public class CallAdapter
    {
        public int chkListenerConnect = 1;
        public IdmsAdapterConfig chkconfig;
        public bool chkForConnection = false;
        public DateTime chkHelloDateTime = new DateTime();
        public System.Timers.Timer TheTimer = new System.Timers.Timer();
        public int testi = 0;
        GenusHESClient GenusMeterService = new GenusHESClient();
        private IdmsConnector callConnector;   // CallConnector
        private uint _sequence = 0;            // message sequence
        // list of account numbers used for sending calls
        //private string[] accountList = { "555125747", "555098057", "555328659", "555033144", "555144548", "555280979" };
        private string[] accountList = { "555125747" };
        //multiple meter test
        //private string[] meterList = { "GSSHP1633", "GSSHP1629", "GSSHP1608", "GSSHP1647" };
        //single meter test
        private string[] meterList = { "GSSHP1633" };
        private int callIndex = 0;    // index marked the calls that has been sent
        public int MessageCount = 0;
        public bool sts = true;//to stop Meter Event Iteration
        public string fromtime;
        public string totime;
        clsDatabase clsdb = new clsDatabase();
        public static bool IsRestartCalled = false;
        /// <summary>
        /// Constructor
        /// It creates a callConnector that connects to the Call Server and registers LogMessage and MessagesReceivedFromIdms callbacks
        /// 
        /// </summary>
        /// 

        public void SetTimerForHelloResponse(double interval, bool startNow)
        {

            // this is not really necessary, since the default value
            // of a new System.Timer's 'AutoReset Property is 'true: 
            // shown here only for educational value
            //
            // if AutoReset is set to 'false: the Elapsed EventHandler 
            // is called once: the System.Timer must be re-started to use again
            TheTimer.AutoReset = true;

            TheTimer.Interval = interval;

            TheTimer.Elapsed += new ElapsedEventHandler(TheTimer_Elapsed);

            TheTimer.Enabled = startNow;
        }

        /// <summary>
        /// This timer event elapsed while system is in idle mode or call server restarted . every 30 seconds this timer will be called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TheTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            TimeSpan t = DateTime.Now - chkHelloDateTime;

            if (chkForConnection == false) // checking previously connection established..
            {
                ReconnectService();
            }
            else
            {
                if (t.Seconds > 30) // checking hello response time period
                {
                    ReconnectService();
                }
            }


            //// test case
            // Console.WriteLine("Testing Hello Response minuts : "+t.Minutes);
            //timerCount++;
        }
        /// <summary>
        /// This method handle servres redurancy mechanism
        /// </summary>
        public void ReconnectService()
        {
            try
            {
                if (chkListenerConnect == 1)
                {
                    chkListenerConnect = 2;
                    chkconfig.ListenerHost = ConfigurationManager.AppSettings["ListenerHost2"].ToString();
                }
                else
                {
                    chkListenerConnect = 1;
                    chkconfig.ListenerHost = ConfigurationManager.AppSettings["ListenerHost"].ToString();
                }

                callConnector = new IdmsConnector(chkconfig);   // Construct the CallConnector by passing the Config object
                callConnector.OnLogMessage += new IdmsConnector.LogMessageDelegate(this.LogMessageToConsole);   // register OnLogMessage call back
                callConnector.OnMessageReceivedFromIdms += new IdmsConnector.MessageReceivedFromIdmsDelagate(this.OnMessageFromIdms);  // register OnMessageReceivedFromIdms call back
                callConnector.OnConnectionChangeEventHandler += new IdmsConnector.OnConnectionChangeDelegate(this.OnConnectionChangeEvent);  // register OnConnectionChangeEventHandler
            }
            catch (Exception ex)
            {


            }
        }
        public void StartTimer()
        {
            TheTimer.Enabled = true;
        }
        public void StopTimer()
        {
            TheTimer.Enabled = false;
        }
        public CallAdapter()
        {
            // Configuration for how to connect to Call Server


            chkListenerConnect = 1;
            IdmsAdapterConfig config = new IdmsAdapterConfig()
            {
                LogFilePath = ConfigurationManager.AppSettings["LogFilePath"].ToString(),// path for the log file
                ListenerHost = ConfigurationManager.AppSettings["ListenerHost"].ToString(),   // host of the Call Server
                ListenerPort = Convert.ToInt32(ConfigurationManager.AppSettings["ListenerPort"]),          // port number for the Call Server
                CheckMessageSecurity = Convert.ToBoolean(ConfigurationManager.AppSettings["CheckMessageSecurity"].ToString()),  // If CheckMessageSecurity is turned on, this needs to match the server setting
                UseCredentialFile = Convert.ToBoolean(ConfigurationManager.AppSettings["UseCredentialFile"].ToString()),    // If UseCredentialFile is turned on, this needs to match the server setting
                AllowUnregisteredSids = Convert.ToBoolean(ConfigurationManager.AppSettings["AllowUnregisteredSids"].ToString()),// If AllowUnregisteredSids is turned on, this needs to match the server setting
                TimeToLive = Convert.ToInt32(ConfigurationManager.AppSettings["TimeToLive"]),             // How long in second the message will live before considered as expired
                HelloIntervalSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["HelloIntervalSeconds"]),  // How frequent that Hello message is sent in seconds
                AppName = ConfigurationManager.AppSettings["AppName"].ToString()

            };
            chkconfig = config;

            callConnector = new IdmsConnector(config);   // Construct the CallConnector by passing the Config object

            callConnector.OnLogMessage += new IdmsConnector.LogMessageDelegate(this.LogMessageToConsole);   // register OnLogMessage call back
            callConnector.OnMessageReceivedFromIdms += new IdmsConnector.MessageReceivedFromIdmsDelagate(this.OnMessageFromIdms);  // register OnMessageReceivedFromIdms call back
            callConnector.OnConnectionChangeEventHandler += new IdmsConnector.OnConnectionChangeDelegate(this.OnConnectionChangeEvent);  // register OnConnectionChangeEventHandler


            SetTimerForHelloResponse(30000, false);

            StartTimer();


        }
        public CallAdapter(IdmsAdapterConfig config)
        {
            callConnector = new IdmsConnector(config);   // Construct the CallConnector by passing the Config object
            callConnector.OnLogMessage += new IdmsConnector.LogMessageDelegate(this.LogMessageToConsole);   // register OnLogMessage call back
            callConnector.OnMessageReceivedFromIdms += new IdmsConnector.MessageReceivedFromIdmsDelagate(this.OnMessageFromIdms);  // register OnMessageReceivedFromIdms call back
            callConnector.OnConnectionChangeEventHandler += new IdmsConnector.OnConnectionChangeDelegate(this.OnConnectionChangeEvent);  // register OnConnectionChangeEventHandler
        }
        /// <summary>
        /// For sample this just logs the message to Console
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="text"></param>
        public void LogMessageToConsole(NetMessageBase msg, string text)
        {
            Console.Out.WriteLine(text);
        }
        /// <summary>
        /// Call back when Connection status changed, just log the changes here
        /// </summary>
        /// <param name="connected"></param>
        /// <param name="hostname"></param>
        public void OnConnectionChangeEvent(bool connected, string hostname)
        {
            chkForConnection = true;
            string status = connected ? "Good" : "Broken";


            if (status == "Broken")
            {

                //if (IsRestartCalled == false)
                //{
                //    IsRestartCalled = true;

                    createLogFile();

                    // OMS Application restart ....
                    ResetApp ts = new ResetApp();
                    ts.methodRestart();

                //}

            }
            Console.WriteLine("The connection to Call server at " + hostname + " is " + status);
        }
        /// <summary>
        /// This gets called when a message is received from the Call Server
        /// Based on the type of message, it can be just simply logged, or handled specifically based on the application needs.
        /// </summary>
        /// <param name="msg"></param>
        public void OnMessageFromIdms(NetMessageBase msg)
        {
           
            Console.Out.WriteLine("Received message " + msg.GetType());
            MessageCount++;
            if (msg is HelloResponseV1)
            {

                chkHelloDateTime = DateTime.Now;
                HelloResponseV1 r = msg as HelloResponseV1;

                if (r.MySystemId != MessageBaseV1.CALL_SERVER)
                {
                    Console.WriteLine("We are connecting to {0}, not the right server. Disconnecting.", r.MySystemId);
                    callConnector.CloseConnection();
                    
                }
                else if (MessageCount == 10)
                {
                    callConnector.CloseConnection();
                }
            }
            else if (msg is AckNackV1)
            {
                AckNackV1 ack = (AckNackV1)msg;
                if (ack.IsAck)
                    Console.Out.WriteLine("Ack for message " + ack.MessageSequence);
            }
            else if (msg is MeterEventQueryAmiV1)
            {
                createTestLogFile("MeterEventQueryAmiV1 request from call server");
                MeterEventQueryAmiV1 timestmp = (MeterEventQueryAmiV1)msg;
                DateTime time = DateTime.Now;
                totime = time.ToString("yyyyMMddHHmmss");
                fromtime = Convert.ToDateTime(timestmp.FromTimeStamp).ToString("yyyyMMddHHmmss");

                GetMeterEventResponse();
            }
            else if (msg is MeterPingRequestAmiV1)
            {
                createTestLogFile("MeterPingRequestAmiV1 request from call server");
                // process MeterEventQueryAmiV1 here
                MeterPingRequestAmiV1 png = (MeterPingRequestAmiV1)msg;
                List<MeterIdType> MeterIds = png.MeterIds;
                createTestLogFile("MeterPingRequestAmiV1 - Total meter requests " + MeterIds.Count);
                if (MeterIds.Count > 0)
                {

                    MeterPingQuery(MeterIds);
                }


            }
            else if (msg is MeterVoltPingReqAmiV1)
            {
                createTestLogFile("MeterVoltPingReqAmiV1 request from call server");
                // process MeterVoltPingAmiV1 here
                MeterVoltPingReqAmiV1 vlt = (MeterVoltPingReqAmiV1)msg;
                List<MeterIdType> MeterIds = vlt.MeterIds;
                createTestLogFile("MeterVoltPingReqAmiV1 - Total meter requests " + MeterIds.Count);
                if (MeterIds.Count > 0)
                {
                    MeterVoltEventQuery(MeterIds);
                }


            }

            //section 2 IVR Events
            else if (msg is CallQueryCivV1)
            {
                SendCallQueryResponse();
            }
            //else if (msg is CustomerCorrectionCivV1)
            //{
            //    // process MeterEventQueryAmiV1 here
            //}
            //else if (msg is CaseNoteQueryCivV1)
            //{
            //    // process MeterEventQueryAmiV1 here
            //}
            else
            {

            }
        }
        #region Meter Event Query Response
        public void GetMeterEventResponse()
        {
            try
            {
                Hashtable hTable = new Hashtable();
                hTable.Add("@fromtime", fromtime);
                hTable.Add("@totime", totime);
                // notified 0 meteres rettriving
                DataTable dts = clsdb.GetDataTable("Query_AMIMDMS_GetMeterEvents", hTable);
                createTestLogFile("MeterEventQueryAmiV1 - Getting meter event data from MDMS Database - Request fromtime:" + fromtime + " and Request totime:" + totime + "");
                createTestLogFile("MeterEventQueryAmiV1 - Total Records From MDMS Database is " + dts.Rows.Count);
                if (dts.Rows.Count > 0)
                {
                    createTestLogFile("MeterEventQueryAmiV1 - sending meter event response to call server");
                    SendMeterEventResponse(dts);
                }
                else
                {
                    dts = null;
                    SendMeterEventResponse(dts);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error found {0}" + ex.Message.ToString());
            }

        }
        public void SendMeterEventResponse(DataTable dtmain)
        {
            MeterEventResponseAmiV1 msg = new MeterEventResponseAmiV1();

            if (dtmain != null)
            {
                msg.MeterResponses = new List<MeterResponseAmiV1>();
                //callIndex = 0;
                if (callIndex >= dtmain.Rows.Count)
                {
                    callIndex = 0;
                    // sts = false;
                    // return;
                }
                for (int i = 0; i < dtmain.Rows.Count; i++)
                {
                    MeterResponseAmiV1 call = new MeterResponseAmiV1()
                    {
                        MeterId = dtmain.Rows[i]["MeterId"].ToString(),
                        IsIn = Convert.ToBoolean(dtmain.Rows[i]["PowerIn"]),
                        IsOut = Convert.ToBoolean(dtmain.Rows[i]["PowerOut"]),
                        QueryTimeStamp = Convert.ToDateTime(dtmain.Rows[i]["PowerTime"].ToString()),
                        TimeReported = DateTime.Now
                    };
                    //update Update_Notify
                    Hashtable hTable = new Hashtable();
                    hTable.Add("@ID", dtmain.Rows[i]["TId"].ToString());
                    int res = clsdb.ExecuteNonQuery("Update_Notify", hTable);
                    if (res > 0)
                    {

                    }

                    msg.UpToTimeStamp = DateTime.Now;
                    msg.MeterResponses.Add(call);
                    msg.Sequence = _sequence++;
                    // Call the CallConnector to send the message to server
                    //sts = false;
                    callIndex++;  // mark the next call Index
                }
            }
            callConnector.SendMessageToIdms(msg);
            createTestLogFile("MeterEventQueryAmiV1 -  meter event response to call server sent completed");
            //}
            //else
            //{
            //    if (!sts)
            //    {
            //        // MeterResponseAmiV1 call = new MeterResponseAmiV1();
            //        // msg.MeterResponses.Add(call);
            //        msg.Sequence = _sequence++;
            //        callConnector.SendMessageToIdms(msg);
            //    }
            //}
            //callIndex++;  // mark the next call Index

            // Create a Meter Status out message
            // This will be shows in NetClient as red round marker around the Customer to indicate the Meter is out. 
            // To verify, copy and paste the following url in the NetClient viewer
            // NETVIEW://RT/VIEW/DETAIL/RAVEN?ZOOM=15.4730346034505&X=940905.512911328&Y=813055.594137977&FORCENONMAINON&AUTOLABELS&ADJACENT=1&SHOWPOTENTIALFAULTHALOS&CUSTOMERLAYERS=(ALL)&CREWLAYERS=(ALL)&FLTINDHALOS=(BOTH)
            //MeterStatusAmiV1 meterStatusAmi = new MeterStatusAmiV1()
            //{

            //    MeterId = ConfigurationManager.AppSettings["MeterId"].ToString(),
            //    Status = (int)eAmiQueryResult.Out,
            //    LastCommunicationTime = DateTime.Now
            //};

            // msg.MeterStatuses.Add(meterStatusAmi);
            //msg.Sequence = _sequence++;
            //callConnector.SendMessageToIdms(msg);   // Call the CallConnector to send the message to server
        }
        #endregion
        #region Meter Ping Query Response
        public void MeterPingQuery(List<MeterIdType> meterids)
        {
            try
            {
                MeterPingResponseAmiV1 msg = new MeterPingResponseAmiV1();
                if (callIndex >= meterids.Count)
                {
                    callIndex = 0;
                    // sts = false;
                    // return;
                }
                for (int i = 0; i < meterids.Count; i++)
                {

                    try
                    {
                        string meterno = meterids[i].value.ToString();

                        createTestLogFile("MeterPingRequestAmiV1 - sending meter ping request to the Genus Service.Meter No :" + meterno);

                        MeterPingStatusEntity lst = GenusMeterService.GetLatestPingTimeForMeter(meterno);
                       
                        if (lst == null)
                            createTestLogFile("MeterPingRequestAmiV1 - meter ping response from genus service is null ");
                        else
                            createTestLogFile("MeterPingRequestAmiV1 - meter ping response from genus service is : Status -"+lst.Status);
                      
                        if (lst.LastCommunication.Trim() == "")
                        {
                            lst.LastCommunication = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                        }

                        DateTime dt = DateTime.ParseExact(lst.LastCommunication, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                        MeterStatusAmiV1 meterStatusAmi = new MeterStatusAmiV1()
                        {
                            MeterId = lst.MeterID,
                            Status = (int)lst.Status,
                            LastCommunicationTime = dt
                            // LastCommunicationTime = Convert.ToDateTime(DateTime.Now)
                        };
                        msg.MeterStatuses.Add(meterStatusAmi);
                        msg.Sequence = _sequence++;
                        callIndex++;  // mark the next call Index

                    }
                    catch (Exception ex)
                    {


                    }
                }

                callConnector.SendMessageToIdms(msg);
                createTestLogFile("MeterPingRequestAmiV1 - meter ping response sent to the call server");

                //Console.WriteLine("MeterPingResponse");
                //Console.WriteLine("==========================================================");
                //Console.WriteLine(Environment.NewLine + "Meter ID=" + pingAttribute.MeterID);
                //Console.WriteLine(Environment.NewLine + "Last Comm=" + pingAttribute.LastCommunication);
                //Console.WriteLine(Environment.NewLine + "Status=" + pingAttribute.Status);
                //Console.WriteLine("==========================================================");

                //Console.WriteLine("{0}","Meter ID=" +pingAttribute.MeterID + "Last Comm=" + pingAttribute.LastCommunication + "Status=" + pingAttribute.Status);
                // Console.ReadLine();
                //MeterPingResponseAmiV1 msg = new MeterPingResponseAmiV1();
                //MeterStatusAmiV1 meterStatusAmi = new MeterStatusAmiV1()
                //{

                //    MeterId = ConfigurationManager.AppSettings["MeterId"].ToString(),
                //    Status = (int)eAmiQueryResult.Out,
                //    LastCommunicationTime = DateTime.Now
                //};

                //msg.MeterStatuses.Add(meterStatusAmi);
                //msg.Sequence = _sequence++;
                //callConnector.SendMessageToIdms(msg);   // Call the CallConnector to send the message to server
            }
            catch (Exception ex)
            {


            }
        }
        #endregion
        #region Meter Volt Query Response
        public void MeterVoltEventQuery(List<MeterIdType> meterids)
        {
            try
            {
                MeterVoltPingResAmiV1 msg = new MeterVoltPingResAmiV1();
                if (callIndex >= meterids.Count)
                {
                    callIndex = 0;
                    // sts = false;
                    // return;
                }
                for (int i = 0; i < meterids.Count; i++)
                {

                    try
                    {

                        string meterno = meterids[i].value.ToString();
                        createTestLogFile("MeterVoltPingResAmiV1 - sending meter voltage ping request to the Genus Service.Meter No :" + meterno);
                        MeterVoltEntity lst = GenusMeterService.GetVoltEventForMeter(meterno);

                        if (lst == null)
                            createTestLogFile("MeterVoltPingResAmiV1 - meter voltage ping response from genus service is null ");
                        else
                            createTestLogFile("MeterVoltPingResAmiV1 - meter voltage ping response from genus service is : VoltA -" + lst.VoltA+ "&VoltB -" + lst.VoltB + "&VoltC -" + lst.VoltC + "");

                        if (lst.TimeReported.Trim() == "")
                        {
                            lst.TimeReported = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                        }

                        DateTime dt = DateTime.ParseExact(lst.TimeReported, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                        MeterVoltStatusAmi mvstatus = new MeterVoltStatusAmi()
                        {
                            MeterId = lst.MeterID.ToString(),
                            VoltA = (float)lst.VoltA,
                            VoltB = (float)lst.VoltB,
                            VoltC = (float)lst.VoltC,
                            LastCommunicationTime = dt
                        };
                        msg.MeterVoltStats.Add(mvstatus);
                        msg.Sequence = _sequence++;
                        callIndex++;  // mark the next call Index

                        //Uri geturi = new Uri(ConfigurationManager.AppSettings["genusMeterVolt"].ToString() + meterno); //replace your url  
                        //System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                        //System.Net.Http.HttpResponseMessage responseGet = await client.GetAsync(geturi);
                        //if (responseGet.StatusCode == System.Net.HttpStatusCode.OK)
                        //{
                        //    string response = await responseGet.Content.ReadAsStringAsync();
                        //    if (response != null &&
                        //       response.Length > 0)
                        //    {
                        //        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        //        MeterVoltQ voltAttribute = jsonSerializer.Deserialize<MeterVoltQ>(response);
                        //        //return logic of volt query

                        //        MeterVoltStatusAmi mvstatus = new MeterVoltStatusAmi()
                        //        {
                        //            MeterId = voltAttribute.MeterID.ToString(),
                        //            VoltA = (float)voltAttribute.VoltA,
                        //            VoltB = (float)voltAttribute.VoltB,
                        //            VoltC = (float)voltAttribute.VoltC,
                        //            LastCommunicationTime = Convert.ToDateTime(voltAttribute.TimeReported)
                        //        };
                        //        msg.MeterVoltStats.Add(mvstatus);
                        //        msg.Sequence = _sequence++;
                        //        callIndex++;  // mark the next call Index
                        //    }
                        //    else
                        //    {

                        //    }
                        //}
                        //else
                        //{

                        //}


                    }
                    catch (Exception ex)
                    {


                    }
                }
                callConnector.SendMessageToIdms(msg);
                createTestLogFile("MeterVoltPingResAmiV1 - meter voltage ping response sent to the call server");
            }
            catch (Exception ex)
            {


            }



            /////////////////////////////////////////////////////////////////////////////////////

            //MeterVoltEventResponseAmiV1 msg = new MeterVoltEventResponseAmiV1();
            //msg.MeterResponses = new List<MeterVoltResponseAmi>();
            //test meter GSS700042

            //Console.WriteLine("MeterVoltEventResponse");
            //Console.WriteLine("==========================================================");
            //Console.WriteLine(Environment.NewLine + "Meter ID=" + voltAttribute.MeterID );
            //Console.WriteLine(Environment.NewLine +"Reported TS=" + voltAttribute.TimeReported );
            //Console.WriteLine(Environment.NewLine + "Query TS=" + voltAttribute.QueryTimeStamp);
            //Console.WriteLine(Environment.NewLine + "Volt A=" + voltAttribute.VoltA );
            //Console.WriteLine(Environment.NewLine + "Volt B=" + voltAttribute.VoltB);
            //Console.WriteLine(Environment.NewLine + "Volt C=" + voltAttribute.VoltC);
            //Console.WriteLine("==========================================================");
            //Console.WriteLine("{0}", "Meter ID=" + voltAttribute.MeterID + "Query TS=" + voltAttribute.QueryTimeStamp + "Reported TS=" + voltAttribute.TimeReported + "Volt A=" + voltAttribute.VoltA + "Volt B=" + voltAttribute.VoltB + "Volt C=" + voltAttribute.VoltC);
            //Console.ReadLine();
        }
        #endregion

        /// <summary>
        /// Check if the server is ready
        /// </summary>
        /// <returns></returns>
        public bool IsReady()
        {
            return callConnector.IsReady();
        }
        /// <summary>
        /// Send Call Query Response, include any new calls in the response
        /// </summary>
        public void SendCallQueryResponse()
        {

            CallResponseCivV1 msg = new CallResponseCivV1();
            msg.TroubleMessages = new List<TroubleCallCivV1>();
            msg.UpToTimeStamp = DateTime.Now;

            if (callIndex >= accountList.Count()) return;  // all the calls have been sent, no new calls, just return

            // create a trouble call, and send it
            // when all the calls have been sent from the account list, will result a predicted incident in DMS for the Fuse 21576999 to be predicted out
            // To verify, copy and paste the following url in the NetClient viewer
            // NETVIEW://RT/VIEW/DETAIL/RAVEN?ZOOM=8.07299993919073&X=943126.781956531&Y=813140.206680607&FORCENONMAINON&AUTOLABELS&ADJACENT=1&SHOWPOTENTIALFAULTHALOS&CUSTOMERLAYERS=(ALL)&CREWLAYERS=(ALL)&FLTINDHALOS=(BOTH)
            TroubleCallCivV1 call = new TroubleCallCivV1()
            {
                AccountId = accountList[callIndex],
                CallTime = DateTime.Now,
                Comments = "The light is out",
                Region_1 = "NORTH",
                Region_2 = "WST",
                X = 0,
                Y = 0,
                PriorityCode = 0,
                QueryTimeStamp = DateTime.Now,
                CallType = 1,
                Condition_1 = 0,
                Condition_2 = 0,
                Description = 0,
                IsFromCallback = false,
                WantsCallback = true
            };
            msg.TroubleMessages.Add(call);
            msg.Sequence = _sequence++;
            callConnector.SendMessageToIdms(msg);   // Call the CallConnector to send the message to server

            callIndex++;  // mark the next call Index

        }
        /// <summary>
        /// Sent Meter Ping Response, this usually is called if the server send a MeterPingRequest, 
        /// For demo purpose, send the PingResponse without the PingRequest
        /// </summary>
        public void SendMeterPingResponse()
        {
            MeterPingResponseAmiV1 msg = new MeterPingResponseAmiV1();
            msg.MeterStatuses = new List<MeterStatusAmiV1>();
            // Create a Meter Status out message
            // This will be shows in NetClient as red round marker around the Customer to indicate the Meter is out. 
            // To verify, copy and paste the following url in the NetClient viewer
            // NETVIEW://RT/VIEW/DETAIL/RAVEN?ZOOM=15.4730346034505&X=940905.512911328&Y=813055.594137977&FORCENONMAINON&AUTOLABELS&ADJACENT=1&SHOWPOTENTIALFAULTHALOS&CUSTOMERLAYERS=(ALL)&CREWLAYERS=(ALL)&FLTINDHALOS=(BOTH)
            //MeterStatusAmiV1 meterStatusAmi = new MeterStatusAmiV1()
            //{

            //    MeterId = ConfigurationManager.AppSettings["MeterId"].ToString(),
            //    Status = (int)eAmiQueryResult.Out,
            //    LastCommunicationTime = DateTime.Now
            //};

            // msg.MeterStatuses.Add(meterStatusAmi);
            msg.Sequence = _sequence++;
            callConnector.SendMessageToIdms(msg);   // Call the CallConnector to send the message to server
        }
        /// <summary>
        /// Main method of the CallAdapter
        /// 
        /// </summary>
        /// <param name="args"></param>
        static public void TestCallMessages(string[] args)
        {
            CallAdapter adapter = new CallAdapter();  // create the CallAdatper



            //string s=Convert.ToDateTime("26/09/2016 1:05:45 PM").ToString()

            // adapter.TestConnectionWithServer1();
            //adapter.TestConnectionWithServer();
            // send the Meter Out message
            //adapter.SendMeterPingResponse("0");
            //adapter.MeterVoltEventQuery("0");
            //adapter.MeterPingQuery("0");
        }

        public bool TestConnectionWithServer()
        {
            Console.WriteLine("Waiting for the connection with Call Server to be ready");
            // wait for the Apapter to be ready, this involves exchange Hello messages with the server 
            while (!IsReady())
            {
                Console.Write(".");
                Thread.Sleep(50);
            }
            return IsReady();
        }

        public void TestConnectionWithServer1()
        {

            try
            {

                MeterPingStatusEntity lst = GenusMeterService.GetLatestPingTimeForMeter("304562");
                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                string s1 = jsonSerializer.Serialize(lst);
                //MeterPingQ pingAttribute = jsonSerializer.Deserialize<MeterPingQ>(s);

                DateTime dt = DateTime.ParseExact("27-09-2016 09:00", "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);

                DateTime d = DateTime.Parse("09-18-2016 09:54");
                string s = d.ToString("MM-dd-yyyy hh:mm");
                MeterStatusAmiV1 meterStatusAmi = new MeterStatusAmiV1()
                {
                    MeterId = lst.MeterID,
                    Status = (int)lst.Status,
                    LastCommunicationTime = dt
                    // LastCommunicationTime = Convert.ToDateTime(DateTime.Now)
                };
                //msg.MeterStatuses.Add(meterStatusAmi);
                // msg.Sequence = _sequence++;
                //callIndex++;  // mark the next call Index
                // callConnector.SendMessageToIdms(msg);

            }
            catch (Exception ex)
            {


            }

        }

        public void createLogFile()
        {
            try
            {
                CreateLogFiles Err = new CreateLogFiles();



                string s = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Log";
                if (!Directory.Exists(s))
                {
                    Directory.CreateDirectory(s);
                }
                s += "/BrokenConnection_";
                Err.ErrorLog(s, "CALL SERVER CONNECTION BROKEN.....");
                Err.ErrorLog(s, "OMS APPLICATION RESTARTED....");
            }
            catch (Exception ex)
            {


            }
        }

        public void createTestLogFile(string message)
        {
            try
            {
                bool EnableTestLog=Convert.ToBoolean(ConfigurationManager.AppSettings["EnableTestLog"].ToString());
                if (EnableTestLog)
                {
                    CreateLogFiles testLog = new CreateLogFiles();
                    string s = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Log";
                    if (!Directory.Exists(s))
                    {
                        Directory.CreateDirectory(s);
                    }
                    s += "/TestData_" + DateTime.Now.ToString("ddMMyyyy");
                    testLog.SaveTestLog(s, message);
                }
                
            }
            catch (Exception ex)
            {


            }
        }

    }
}
