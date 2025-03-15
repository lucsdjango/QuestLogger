using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Text;

public class QTMListener : MonoBehaviour
{

    private UdpClient udpClient;

    private readonly Queue<string> incomingQueue = new Queue<string>();
    
    Thread receiveThread;


    private static bool waitingForStart = false;
    private static bool waitingForStop = false;
    public static string msgString = "";
    public static string fileName = "";


    private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

    public TMPro.TextMeshProUGUI debugText;

    // Start is called before the first frame update
    void Start()
    {
        StartConnection(8989);
    }

    // Update is called once per frame
    void Update()
    {

        lock (fileName)
        { 
            debugText.text = fileName;
            
        }
    }

    public void StartConnection(int receivePort)
    {
        /*
        try { udpClient = new UdpClient(receivePort); }
        catch (Exception e)
        {
            Debug.Log("Failed to listen for UDP at port " + receivePort + ": " + e.Message);

            return;
        }
        Debug.Log("Created receiving client at ip  and port " + receivePort);
        
        //udpClient.Close();
        */
        StartListeningForStart();
    }

    public struct UdpState
    {
        public UdpClient u;
        public IPEndPoint e;
    }

    public void StartListeningForStart()
    {
        waitingForStart = true;
        
        IPEndPoint e = new IPEndPoint(IPAddress.Any, 8989);
        udpClient = new UdpClient(e);

        //udpClient.Connect(e.AddressFamily, e.Port);

        Debug.Log("receiving ...");

        receiveThread = new Thread(() => ListenForStart());
        receiveThread.Start();
    }

    static void CheckForStartMsg(IAsyncResult ar)
    {


        msgString = "received";
        UdpClient u = ((UdpState)(ar.AsyncState)).u;
        IPEndPoint e = ((UdpState)(ar.AsyncState)).e;

        
        byte[] msg = u.EndReceive(ar, ref e);
        lock (msgString)
            msgString = Encoding.ASCII.GetString(msg);
        //Debug.Log("This is the message you received " + msgString);
        if (waitingForStart && msgString.Contains("CaptureStart"))
        {
            waitingForStart = false;
            waitingForStop = true;
            u.Close();
        }
        
        //Debug.Log("waitingForStart " + waitingForStart);
        //Debug.Log("waitingForStop " + waitingForStop);

    }
    
    public void StartListeningForStop()
    {
        waitingForStop = true;
        if (receiveThread != null && receiveThread.IsAlive)
            receiveThread.Abort();

        receiveThread = new Thread(() => ListenForStop());
        receiveThread.Start();



    }
    
    private void ListenForStart()
    {
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8989);
        Debug.Log("waiting for start");
        //debugText.text = "waiting for start";
        while (waitingForStart)
        {
            Debug.Log("waiting inside");
            //debugText.text = "waiting inside";
            try
            {
                Byte[] receivedBytes = udpClient.Receive(ref remoteIpEndPoint); // Blocks until a message returns on this socket from a remote host.
                msgString = Encoding.ASCII.GetString(receivedBytes);
                Debug.Log("This is the message you received " + msgString);
                //debugText.text = "This is the message you received " + msgString;
                if (msgString.Contains("CaptureStart"))
                {
                    //< Name VALUE = "ID00test_0016"
                    string keyString = "Name VALUE=\"";
                    int idxStart    = msgString.IndexOf(keyString) + keyString.Length;
                    int idxEnd      = msgString.IndexOf("\"", idxStart);
                    lock (fileName)
                    {
                        fileName = msgString.Substring(idxStart, idxEnd - idxStart);
                        print(fileName);
                    }
                    stopWatch.Start();
                    waitingForStart = false;
                    waitingForStop = true;

                }
                
            }
            catch (SocketException e)
            {
                // 10004 thrown when socket is closed
                if (e.ErrorCode != 10004)
                {
                    Debug.Log("Socket exception while receiving data from udp client: " + e.Message);
                    //debugText.text = "Socket exception while receiving data from udp client: " + e.Message;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error receiving data from udp client: " + e.Message);
                //debugText.text = "Error receiving data from udp client: " + e.Message;
            }
        }
    }

    private void ListenForStop()
    {
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8989);
        Debug.Log("waiting for stop");
        //debugText.text = "waiting for stop";

        while (waitingForStop)
        {
            try
            {
                Byte[] receivedBytes = udpClient.Receive(ref remoteIpEndPoint); // Blocks until a message returns on this socket from a remote host.
                lock (msgString)
                    msgString = Encoding.ASCII.GetString(receivedBytes);
                Debug.Log("This is the message you received " + msgString);
                //debugText.text = "This is the message you received " + msgString;
                if (msgString.Contains("CaptureStop"))
                {
                    waitingForStop = false;
                    waitingForStart = true;

                }

            }
            catch (SocketException e)
            {
                // 10004 thrown when socket is closed
                if (e.ErrorCode != 10004)
                {
                    Debug.Log("Socket exception while receiving data from udp client: " + e.Message);
                    //debugText.text = "Socket exception while receiving data from udp client: " + e.Message;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error receiving data from udp client: " + e.Message);
                //debugText.text = "Error receiving data from udp client: " + e.Message;
            }
        }
    }
    
    public long StartReceivedThisLongAgo()
    {
        if (!waitingForStart && waitingForStop)
        {
            long ms = stopWatch.ElapsedMilliseconds;
            stopWatch.Stop();
            stopWatch.Reset();
            StartListeningForStop();
            return ms;
        }
        else
        {
            return -1;
        }
    }

    public bool HasReceivedStop()
    {
        if (waitingForStart && !waitingForStop)
        {
            StartListeningForStart();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HasReceivedStart()
    {
        //print("HasReceivedStart inside lock");
        //Debug.Log(waitingForStart + " " + waitingForStop);
        if (!waitingForStart && waitingForStop)
        {
            
            return true;
        }
        else
        {
            return false;
        }
    }

    void OnDestroy()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();

        }
        udpClient.Close();

    }

}
