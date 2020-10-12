using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdmsAdapter;
using NetUtil;

namespace ApapterSamples
{
    /// <summary>
    /// This class demonstrations how to use IdmsConnector to connect to Crew Server.
    /// 
    /// </summary>
    public class CrewAdapter
    {
        private IdmsConnector crewConnector;   // CrewConnector
        private uint _sequence = 0;            // message sequence
        // This is used to look up the incident that has been reported, key is the incident Id and the value is IncidentUpdateWfmV1 message that contains the incident details
        private Dictionary<string, IncidentUpdateWfmV1> incidentMsgList = new Dictionary<string, IncidentUpdateWfmV1>();

        /// <summary>
        /// Constructor, this uses the command line argument to construct IdmsConnector, the command line should contains the following format
        /// /CONFIG IdmsAdapterConfig-file
        /// </summary>
        /// <param name="args"></param>
        public CrewAdapter(string[] args)
        {
            crewConnector = new IdmsConnector(args);  // Construct the CallConnector by passing the command line arguments
            crewConnector.OnLogMessage += new IdmsConnector.LogMessageDelegate(this.LogMessageToConsole);  // register OnLogMessage call back
            crewConnector.OnMessageReceivedFromIdms += new IdmsConnector.MessageReceivedFromIdmsDelagate(this.OnMessageFromIdms);   // register OnMessageReceivedFromIdms call back
            crewConnector.OnConnectionChangeEventHandler += new IdmsConnector.OnConnectionChangeDelegate(this.OnConnectionChangeEvent);  // register OnConnectionChangeEventHandler
        }

        public CrewAdapter(IdmsAdapterConfig config)
        {
            crewConnector = new IdmsConnector(config);  // Construct the CallConnector by passing the command line arguments
            crewConnector.OnLogMessage += new IdmsConnector.LogMessageDelegate(this.LogMessageToConsole);  // register OnLogMessage call back
            crewConnector.OnMessageReceivedFromIdms += new IdmsConnector.MessageReceivedFromIdmsDelagate(this.OnMessageFromIdms);   // register OnMessageReceivedFromIdms call back
            crewConnector.OnConnectionChangeEventHandler += new IdmsConnector.OnConnectionChangeDelegate(this.OnConnectionChangeEvent);  // register OnConnectionChangeEventHandler
        }

        /// <summary>
        /// For sample this just logs the message to Console
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="text"></param>
        public void LogMessageToConsole(NetMessageBase msg, string text)
        {
           
            MessageBaseV1 msgBase = (MessageBaseV1)msg; 
            Console.Out.WriteLine(text + " with sequence = " + msgBase.Sequence);
        }

        /// <summary>
        /// Call back when Connection status changed, just log the changes here
        /// </summary>
        /// <param name="connected"></param>
        /// <param name="hostname"></param>
        public void OnConnectionChangeEvent(bool connected, string hostname)
        {
            string status = connected ? "Good" : "Broken";
            Console.WriteLine("The connection to Crew server at " + hostname + " is " + status);
        }

        /// <summary>
        /// This gets called when a message is received from the Crew Server
        /// Based on the type of message, it can be just simply logged, or handled specifically based on the application needs.
        /// </summary>
        /// <param name="msg"></param>
        public void OnMessageFromIdms(NetMessageBase idmsMsg)
        {
            Console.Out.WriteLine("Received message " + idmsMsg.GetType());
            if (idmsMsg.GetType() == typeof(AckNackV1))
            {
                AckNackV1 ack = (AckNackV1)idmsMsg;
                if ( ack.IsAck )
                    Console.Out.WriteLine("Ack for message " + ack.MessageSequence);   // log the Ack  message
                else
                    Console.Out.WriteLine("NoAck for message " + ack.MessageSequence); 
            }
            else if (idmsMsg.GetType() == typeof(IncidentUpdateWfmV1))
            {
                IncidentUpdateWfmV1 iu = (IncidentUpdateWfmV1)idmsMsg;
                Console.Out.WriteLine("Incident Id=" + iu.IncidentId + ", update type = " + ((eUpdateType)iu.UpdateType).ToString());

                // save the incident details for later reference
                if (!incidentMsgList.ContainsKey(iu.IncidentId))
                    incidentMsgList.Add(iu.IncidentId, iu);
                else
                    incidentMsgList[iu.IncidentId] = iu;

                SendAckMessage(iu);              // Send Ack message
                SendIncidentStatusMessage(iu);   // Send incident status message
            }
            else if (idmsMsg.GetType() == typeof(CrewUpdateWfmV1))
            {
                CrewUpdateWfmV1 ca = (CrewUpdateWfmV1)idmsMsg;
                Console.Out.WriteLine("Incident Id=" + ca.IncidentId + ", crew Id = " + ca.CrewId + ", update type = " + ((eUpdateType)ca.UpdateType).ToString());

                SendAckMessage(ca);             // Send Ack message
                SendCrewEnrouteMessage(ca);     // Send Crew Enrount status message
                SendCrewOnsiteMessage(ca);      // Send Crew Onsite status message
                SendVehicleLocationMessage(ca); // send Vehicle location update message
            }
            
        }

        /// <summary>
        /// Check if the server is ready
        /// </summary>
        /// <returns></returns>
        public bool IsReady()
        {
            return crewConnector.IsReady();
        }

        /// <summary>
        /// Send Ack Message for CrewUpdateWfmV1
        /// </summary>
        /// <param name="ca"></param>
        public void SendAckMessage(CrewUpdateWfmV1 ca)
        {
            AckNackV1 ack = new AckNackV1();
            ack.IsAck = true;
            ack.ErrNo = 0; //no error
            ack.Id = ca.IncidentId;
            ack.MessageSequence = ca.Sequence;
            ack.Sequence = _sequence++;

            //optional
            ack.Name = ca.IncidentId;
            ack.AdditionalErrorInformation = ca.ToString();
            crewConnector.SendMessageToIdms(ack);
        }

        /// <summary>
        /// Send Ack Message for IncidentUpdateWfmV1
        /// </summary>
        /// <param name="incUpdate"></param>
        public void SendAckMessage(IncidentUpdateWfmV1 incUpdate)
        {
            AckNackV1 ack = new AckNackV1();
            //required
            ack.IsAck = true;
            ack.ErrNo = 0; //no error
            ack.Id = incUpdate.IncidentId;
            ack.MessageSequence = incUpdate.Sequence;
            ack.Sequence = _sequence++;

            //optional
            ack.Name = incUpdate.IncidentName;
            ack.AdditionalErrorInformation = incUpdate.ToString();
            crewConnector.SendMessageToIdms(ack);
        }

        /// <summary>
        /// Send Crew Enroute message
        /// </summary>
        /// <param name="ca"></param>
        public void SendCrewEnrouteMessage(CrewUpdateWfmV1 ca)
        {
            CrewStatusWfmV1 msg = new CrewStatusWfmV1()
            {
                CrewId = ca.CrewId,
                Status =(int)eWfmCrewStatus.EnRoute,
                Sequence = _sequence++,
                Guid = Guid.NewGuid().ToString()
            };
            crewConnector.SendMessageToIdms(msg);
        }

        /// <summary>
        /// Send Crew Onsite message
        /// </summary>
        /// <param name="ca"></param>
        public void SendCrewOnsiteMessage(CrewUpdateWfmV1 ca)
        {
            CrewStatusWfmV1 msg = new CrewStatusWfmV1()
            {
                CrewId = ca.CrewId,
                Status = (int)eWfmCrewStatus.OnSite,
                Sequence = _sequence++,
                Guid = Guid.NewGuid().ToString()
            };
            crewConnector.SendMessageToIdms(msg);
        }

        /// <summary>
        /// Send Incident Status message
        /// </summary>
        /// <param name="iu"></param>
        public void SendIncidentStatusMessage(IncidentUpdateWfmV1 iu)
        {
            IncidentStatusWfmV1 msg = new IncidentStatusWfmV1()
            {
                Cause = 5,  //
                WfmEtr = DateTime.Now.AddMinutes(30).ToUniversalTime(),
                IncidentId = iu.IncidentId,
                Sequence = _sequence++,
                Guid = Guid.NewGuid().ToString()
            };

            crewConnector.SendMessageToIdms(msg);
        }

        /// <summary>
        /// Send vehicle location message
        /// </summary>
        /// <param name="ca"></param>
        public void SendVehicleLocationMessage(CrewUpdateWfmV1 ca)
        {
            VehicleLocationWfmV1 locations = new VehicleLocationWfmV1();
            IncidentUpdateWfmV1 incidentMsg = incidentMsgList[ca.IncidentId];
            if ( incidentMsg != null ) {
                locations.AddCrewLocation(ca.CrewId, incidentMsg.LatLong.X + 80 , incidentMsg.LatLong.Y+20);  // position the vehicle right beside the incident
                locations.Sequence = _sequence++;
                locations.Guid = Guid.NewGuid().ToString();
                crewConnector.SendMessageToIdms(locations);
            }
        }

        /// <summary>
        /// Main method of the CrewAdapter
        /// 
        /// </summary>
        /// <param name="args"></param>
        static public void TestCrewMessages(string[] args)
        {
            CrewAdapter adapter = new CrewAdapter(args);  // create the CrewAdapter and then wait for it's ready
            adapter.TestConnectionWithServer();
        }

        public bool TestConnectionWithServer() 
        {
            Console.WriteLine("Waiting for the connection with Crew Server to be ready");
            
            while (!IsReady())
            {   
                Console.Write(".");
                Thread.Sleep(100);
            }

            return IsReady();
        }
    }
}
