﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using ClassLibrary;

namespace Client
{
    public class CClient : MarshalByRefObject, IClient
    {
        private readonly string USERNAME;
        private readonly string CLIENT_URL;

        // preferred server
        private readonly string SERVER_URL;
        // obtain server remote object
        private IServer _remoteServer;

        /// <summary>
        /// Creates TCP channel, saves relevant information for remoting, registers itself as
        /// remote object and gets preferred server's remote object.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="clientUrl"></param>
        /// <param name="serverUrl"></param>
        public CClient(string username, string clientUrl, string serverUrl)
        {
            USERNAME = username;
            CLIENT_URL = clientUrl;
            // creates TCP channel
            TcpChannel clientChannel = new TcpChannel(PortExtractor.Extract(CLIENT_URL));
            ChannelServices.RegisterChannel(clientChannel, false);
            // create the client's remote object
            RemotingServices.Marshal(this, username, typeof(CClient));

            SERVER_URL = serverUrl;           
            // retrieve server's proxy
            _remoteServer = (IServer)Activator.GetObject(typeof(IServer), SERVER_URL);
            // register new user in remote server
            _remoteServer.RegisterUser(username, CLIENT_URL);
        }


        public void List()
        {
            List<MeetingProposal> proposals = _remoteServer.List(USERNAME);

            foreach(MeetingProposal proposal in proposals)
            {
                Console.WriteLine(proposal);
            }
        }

        public void Create(string meetingTopic, string minAttendees, List<string> slots, List<string> invitees = null)
        {
            List<DateLocation> parsedSlots = ParseSlots(slots);
            List<string> parsedInvitees = invitees;
            MeetingProposal proposal = new MeetingProposal {
                Coordinator = USERNAME,
                Topic = meetingTopic,
                MinAttendees = Int32.Parse(minAttendees),
                DateLocationSlots = parsedSlots,
                Invitees = parsedInvitees,
                Records = new List<MeetingRecord>()
            };
            _remoteServer.Create(proposal);

            // TODO
            if (invitees != null)
            {
                // send to all invitees
            }
            else
            {
                // send to every client
            }

        }

        public void Join(string meetingTopic, List<string> slots)
        {
            List<DateLocation> parsedSlots = ParseSlots(slots);
            MeetingRecord record = new MeetingRecord
            {
                Name = USERNAME,
                DateLocationSlots = parsedSlots
            };
            _remoteServer.Join(meetingTopic, record);
        }

        public void Close(string meetingTopic)
        {

        }

        public void Wait(string milliseconds)
        {

        }

        // slots -> Lisboa,2019-11-14 Porto,2020-02-03
        public List<DateLocation> ParseSlots(List<string> slots)
        {
            List<DateLocation> parsedSlots = new List<DateLocation>();
            foreach (string slot in slots)
            {
                // local and date are separated by a comma
                List<string> slotDetail = slot.Split(',').ToList();
                parsedSlots.Add(new DateLocation(slotDetail[0], slotDetail[1]));
            }

            return parsedSlots;
        }
    }
}
