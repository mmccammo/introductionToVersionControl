using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Text;

public class NetworkMonitor : MonoBehaviour {

    public int groupNumber = 0;

    public Vector2 networkVehicleRange;
    public Dictionary<string, Dictionary<int, int>> XSIMID_To_VIBEID_Dict;
    public Dictionary<string, Dictionary<int, int>> VIBEID_To_XSIMID_Dict;

    private int currentNetworkID;

    public float translateStepSize = 1.0f;
    public float rotateStepSize = 1.0f;

    public float m_UpdateFrequency = 1.0f; // In seconds
    public int m_NumberOfProjectedFrames = 30;
    public float m_TimeSinceLastXSIMUpdate = 0.0f;

    public bool m_UseFixedUpdate = true;

    public float m_TimeSinceLastHeartbeat = 0;
    public float m_TimeBetweenHeartbeats = 1;

    public bool m_HeartbeatsActive;
    public bool m_NetworkAgentActive;

    public int m_TotalNumberOfMessages = 0;
    public int m_NumberOfVIBECommandMessages = 0;
    public int m_NumberOfVIBEControllerMessages = 0;
    public int m_NumberOfXSIMStatusMessages = 0;
    public int m_NumberOfXSIMUpdateMessages = 0;
    public int m_NumberOfGlobalUpdateMessages = 0;
    public int m_NumberOfEnvUpdateMessages = 0;

    private int m_NumMessagesPerSecond = 0;
    public int m_NumberOfNSIMessagesPerSecond;
    public float m_Timer = 0;

    public bool m_AllowExternalPhysics;

    public int m_DebugStatus;

    public contactUpdate m_SlaveUpdate;
    // Constructor

    [DllImport("NSICS_Wrapper")]public static extern IntPtr NSICS_Create(int masterStatus, int debug);

    // Destructors
    [DllImport("NSICS_Wrapper", EntryPoint = "NSICS_Delete")] public static extern void NSICS_Delete(IntPtr value);
    [DllImport("NSICS_Wrapper", EntryPoint = "NSICS_Stop")] public static extern void NSICS_Stop(IntPtr value); //This currently causes a crash

    // Initialization

    [DllImport("NSICS_Wrapper", EntryPoint = "NSICS_SetGroup")] public static extern void NSICS_SetGroup(IntPtr value, int num);
    [DllImport("NSICS_Wrapper", EntryPoint = "NSICS_GetGroup")] public static extern int NSICS_GetGroup(IntPtr value);
    [DllImport("NSICS_Wrapper", EntryPoint = "NSICS_Start")] public static extern void NSICS_Start(IntPtr value);

    // Message Receiver

    [DllImport("NSICS_Wrapper")] public static extern int NSICS_Select(IntPtr value, out NSIAgentReturnStruct ts);
   

    [DllImport("NSICS_Wrapper", EntryPoint = "NSICS_ClearMessages")] public static extern int NSICS_ClearMessages(IntPtr value, int num);
    [DllImport("NSICS_Wrapper", EntryPoint = "NSICS_ClearAllMessages")] public static extern int NSICS_ClearAllMessages(IntPtr value);

    // Broadcasts

    [DllImport("NSICS_Wrapper", EntryPoint = "NSICS_RequestSimulationState")] public static extern int NSICS_RequestSimulationState(IntPtr value, int a_Status);
    [DllImport("NSICS_Wrapper", EntryPoint = "NSICS_RequestInitialPositions")] public static extern int NSICS_RequestInitialPositions(IntPtr value);
    [DllImport("NSICS_Wrapper", CallingConvention = CallingConvention.StdCall)] public static extern void NSICS_BroadcastDebugMessage(IntPtr value, string a_Message, int a_Channel);
    [DllImport("NSICS_Wrapper", CallingConvention = CallingConvention.StdCall, EntryPoint = "NSICS_BroadcastHeartbeat")] public static extern int NSICS_BroadcastHeartbeat(IntPtr value, ref VIBEHeartbeat a_HB);

    [DllImport("NSICS_Wrapper", CallingConvention = CallingConvention.StdCall)] public static extern int NSICS_VIBEAnnounce(IntPtr value);

    // Misc

    [DllImport("NSICS_Wrapper", EntryPoint = "NSICS_Gettime")] public static extern double NSICS_Gettime(IntPtr value);

    [DllImport("NSICS_Wrapper", CallingConvention = CallingConvention.StdCall)] public static extern void NSICS_SwitchGroupID(IntPtr value, int a_ID);

    [DllImport("NSICS_Wrapper", CallingConvention = CallingConvention.StdCall, EntryPoint = "NSICS_BroadcastVIBESlaveUpdate")] public static extern int NSICS_BroadcastVIBESlaveUpdate(IntPtr value, ref contactUpdate a_Contact);

    [DllImport("NSICS_Wrapper", CallingConvention = CallingConvention.StdCall)] public static extern int NSICS_BroadcastKingMe(IntPtr value);

    [DllImport("NSICS_Wrapper", CallingConvention = CallingConvention.StdCall)] public static extern void NSICS_Identify(IntPtr value, out AnnounceStruct a_AnnounceStruct);

    IntPtr NSI_Agent;

    public bool m_Initialized = false;
    bool m_LinkedAndInitialized = false;

    public bool m_UseNavigationV1;
    public bool m_UseNavigationV2;
    public bool m_UseNavigationV3;

    public NSIAgentReturnStruct m_MessageBox = new NSIAgentReturnStruct();

    public int numCMDMsgs = 0;

    public int m_NumberOfIncomingMessages = 0;

    VIBECommandMessage[] m_VIBECommandMessages;
    VIBETranslatedControllerMessage[] m_VIBEControllerMessages;
    XSIMUpdateMessage[] m_XSIMUpdateMessages;
    XSIMStatusMessage[] m_XSIMStatusMessages;

    XSIMUpdateMessage m_BlankXSIMUpdateMessage;

    VIBEHeartbeat m_HeartbeatMessage;


    int m_CurrentVIBEIDBeingProcessed;
    int m_CurrentXSIMIDBeingProcessed;
    Vector2 m_CurrentGPSPosBeingProcessed;

    public string m_VIBEInstanceName;
    public bool m_MoveCameraToFirstNetworkVehicle = false;
    public AnnounceStruct m_AnnounceStruct;

    public bool m_NetworkEnabled;

    public void SetGroupID(int a_ID)
    {
        if (Application.isPlaying && m_NetworkAgentActive)
        {
            if (NSI_Agent != null)
            {
                groupNumber = a_ID;
                NSICS_SwitchGroupID(NSI_Agent, groupNumber);
            }
        }
    }

    public bool Start()
    {
        Debug.Log("SimStart: " + gameObject.name);
        m_Initialized = false;
        Initialize();
        return m_Initialized;
    }

    public void ClearNetworkLists()
    {
        XSIMID_To_VIBEID_Dict.Clear();
        VIBEID_To_XSIMID_Dict.Clear();
    }

    // Use this for initialization
    void Initialize()
    {
        if (!m_Initialized)
        {
            if (Application.isPlaying)
            {
                m_NetworkAgentActive = false;

                //groupNumber = SimulatorSettings.getNSIGroupID();
                //m_NetworkVehicles = new List<Entity>();

                //m_DebugStatus = SimulatorSettings.getSimCore().m_DebugType;

                //if (SimulatorSettings.getMaster()) NSI_Agent = NSICS_Create(1, m_DebugStatus);
                //else NSI_Agent = NSICS_Create(0, m_DebugStatus);

                NSI_Agent = NSICS_Create(1, m_DebugStatus);


               NSICS_SetGroup(NSI_Agent, groupNumber);
                Debug.Log("NSI Agent Running on GroupNumber: " + NSICS_GetGroup(NSI_Agent));
                NSICS_Start(NSI_Agent);

                Debug.Log(Time.realtimeSinceStartup);
                //Common.Wait(5);
                Debug.Log(Time.realtimeSinceStartup);

                m_NetworkAgentActive = true;

                if (networkVehicleRange == Vector2.zero)
                {
                    networkVehicleRange.x = 1000;
                    networkVehicleRange.y = 2000;
                }
                currentNetworkID = (int)networkVehicleRange.x;
                XSIMID_To_VIBEID_Dict = new Dictionary<string, Dictionary<int, int>>();
                VIBEID_To_XSIMID_Dict = new Dictionary<string, Dictionary<int, int>>();

                //requestInititialPositionsFromExternalSims();

                m_BlankXSIMUpdateMessage.ID = -1;
                m_BlankXSIMUpdateMessage.smmttid = -1;

                m_TimeSinceLastXSIMUpdate = 0.0f;

                m_VIBECommandMessages = new VIBECommandMessage[1024];
                m_VIBEControllerMessages = new VIBETranslatedControllerMessage[1024];
                m_XSIMUpdateMessages = new XSIMUpdateMessage[1024];
                m_XSIMStatusMessages = new XSIMStatusMessage[1024];

                if (m_UseNavigationV3)
                {
                    m_UseNavigationV1 = false;
                    m_UseNavigationV2 = false;
                }
                else if (m_UseNavigationV2)
                {
                    m_UseNavigationV1 = false;
                    m_UseNavigationV3 = false;
                }
                else if (m_UseNavigationV1)
                {
                    m_UseNavigationV2 = false;
                    m_UseNavigationV3 = false;
                }
                else
                {
                    m_UseNavigationV1 = false;
                    m_UseNavigationV2 = false;
                    m_UseNavigationV3 = true;
                }

                m_MessageBox = new NSIAgentReturnStruct();
                m_MessageBox.VIBECommandMessages = new VIBECommandMessage[1024];
                m_MessageBox.VIBEControllerMessages = new VIBETranslatedControllerMessage[1024];
                m_MessageBox.XSIMStatusMessages = new XSIMStatusMessage[1024];
                m_MessageBox.XSIMUpdateMessages = new XSIMUpdateMessage[1024];

                m_MessageBox.GlobalUpdateMessages = new Global[5];
                m_MessageBox.EnvUpdateMessages = new Global_Env[5];

                m_HeartbeatMessage = new VIBEHeartbeat();
                m_HeartbeatMessage.contacts = new contactUpdate[1000];

              //  m_ValidHBEntities = new List<Entity>();

          //      Identify();
                SendAnnounce();
            }
            else
            {
                m_NetworkAgentActive = false;
            }

            m_Initialized = true;
        }

    }

    public void SendAnnounce()
    {
        if (m_NetworkAgentActive)
        {
            NSICS_VIBEAnnounce(NSI_Agent);
        }
    }

    public void checkScriptLinks()
    {
        //if (m_EntityList == null)
        //{
        //    m_EntityList = SimulatorSettings.getEntityList();
        //}

        //if (m_ModelManager == null)
        //{
        //    m_ModelManager = SimulatorSettings.getModelManager();
        //}

        //if (m_InputManager == null)
        //{
        //    m_InputManager = SimulatorSettings.getInputManager();
        //}

        //if (m_EnvManager == null)
        //{
        //    m_EnvManager = SimulatorSettings.getEnvironmentManager();
        //}

        m_LinkedAndInitialized = true;
    }

    public void StopNSI()
    {
        if (m_NetworkAgentActive)
        {
            m_NetworkAgentActive = false;
            NSICS_Stop(NSI_Agent);
            NSICS_Delete(NSI_Agent);
        }
    }


    void SetHandler()
    {

    }

    void Update()
    {
        if (!m_UseFixedUpdate)
        {
            if (m_Initialized && Application.isPlaying)
            {
                if (!m_LinkedAndInitialized) checkScriptLinks();

                if (m_NetworkAgentActive) UpdateNetwork();


            }
        }
    }

    void FixedUpdate()
    {
        if (m_UseFixedUpdate)
        {
            if (m_Initialized && Application.isPlaying)
            {
                if (!m_LinkedAndInitialized) checkScriptLinks();

                if (m_NetworkAgentActive) UpdateNetwork();


            }
        }
    }

    void UpdateNetwork()
    {
        //Debug.Log("Updating Network");

        //Debug.Log("m_MessageBox: " + m_MessageBox.totalNumberOfMessages);

        //if (NSI_Agent != null)
        //{
        //    Debug.Log("Agent exists");
        //}
        //else
        //{
        //    Debug.Log("Agent does not exist");
        //}

        //Debug.Log("NSI Time 1: " + NSICS_Gettime(NSI_Agent));

        //Common.Wait(2);

        //Debug.Log("NSI Time 2: " + NSICS_Gettime(NSI_Agent));

        //Common.Wait(2);

        //Debug.Log("NSI Time 3: " + NSICS_Gettime(NSI_Agent));

        if (!m_NetworkEnabled) return;

        if (!m_NetworkAgentActive) return;

        m_NumberOfIncomingMessages = NSICS_Select(NSI_Agent, out m_MessageBox);

        m_TimeSinceLastHeartbeat += Time.deltaTime;

        m_Timer += Time.deltaTime;

        if (m_Timer > 1)
        {
            m_NumberOfNSIMessagesPerSecond = m_NumMessagesPerSecond;
            m_Timer = 0;
            m_NumMessagesPerSecond = 0;
        }
        else
        {
            m_NumMessagesPerSecond += m_NumberOfIncomingMessages;
        }



        //NSICS_PopulateMessagebox(NSI_Agent, out m_MessageBox);

        //Debug.Log(m_MessageBox.masterStatus);

        //if (m_MessageBox.masterStatus == 0 && SimulatorSettings.getMaster())
        //{
        //    SimulatorSettings.setMaster(false);
        //    Debug.Log("[NetMon] VIBE was demoted.");
        //}
        //else if (m_MessageBox.masterStatus == 1 && !SimulatorSettings.getMaster())
        //{
        //    SimulatorSettings.setMaster(true);
        //    Debug.Log("[NetMon] VIBE was promoted.");
        //}

        if (m_MessageBox.nameChanged == 1)
        {
            //Identify();
            //m_VIBEInstanceName = NSICS_Identify(NSI_Agent).SourceName;
        }

        if (m_MessageBox.totalNumberOfMessages > 0)
        {
            m_TotalNumberOfMessages = m_MessageBox.totalNumberOfMessages;
            m_NumberOfVIBECommandMessages = m_MessageBox.numberOfVIBECommandMessages;
            m_NumberOfVIBEControllerMessages = m_MessageBox.numberOfVIBEControllerMessages;
            m_NumberOfXSIMStatusMessages = m_MessageBox.numberOfXSIMStatusMessages;
            m_NumberOfXSIMUpdateMessages = m_MessageBox.numberOfXSIMUpdateMessages;
            m_NumberOfGlobalUpdateMessages = m_MessageBox.numberOfGlobalUpdateMessages;
            m_NumberOfEnvUpdateMessages = m_MessageBox.numberOfEnvUpdateMessages;

            if (m_MessageBox.initScene == 1)
            {
                Debug.Log("Reinitializing scene");
                m_MessageBox.initScene = 0;
            }

            if (m_NumberOfVIBECommandMessages > 0)
            {
               // parseVIBECommandMessage(m_MessageBox.VIBECommandMessages, m_NumberOfVIBECommandMessages);
            }

            if (m_NumberOfVIBEControllerMessages > 0)
            {
                //parseVIBEControllerMessage(m_MessageBox.VIBEControllerMessages, m_NumberOfVIBEControllerMessages);

                for (int i = 0; i < m_NumberOfVIBEControllerMessages; i++)
                {
                    Debug.Log(Marshal.PtrToStringAnsi(m_MessageBox.VIBEControllerMessages[i].joystickName));

                }
                if (m_NumberOfXSIMStatusMessages > 0)
            {
                //parseXSIMStatusMessage(m_MessageBox.XSIMStatusMessages, m_NumberOfXSIMStatusMessages);
            }
            if (m_NumberOfXSIMUpdateMessages > 0)
            {
                //for (int i = 0; i <= m_NumberOfXSIMUpdateMessages; i++)
                //{
                //    if (i > m_XSIMUpdateMessages.Length - 1)
                //    {
                //        break;
                //    }
                //    else
                //    {
                //        m_XSIMUpdateMessages[i] = m_BlankXSIMUpdateMessage;
                //    }
                //}

               // parseXSIMUpdateMessage(m_MessageBox.XSIMUpdateMessages, m_NumberOfXSIMUpdateMessages);
            }
            if (m_NumberOfGlobalUpdateMessages > 0)
            {
                Debug.Log("Got Global Update");

                //parseGlobalUpdateMessage(m_MessageBox.GlobalUpdateMessages, m_NumberOfGlobalUpdateMessages);

            }
            if (m_NumberOfEnvUpdateMessages > 0)
            {
                Debug.Log("Got Env Update");
                //parseEnvUpdateMessage(m_MessageBox.EnvUpdateMessages, m_NumberOfEnvUpdateMessages);
            }

            ClearAllMessages();

            m_MessageBox.totalNumberOfMessages = 0;
            m_MessageBox.numberOfVIBECommandMessages = 0;
            m_MessageBox.numberOfVIBEControllerMessages = 0;
            m_MessageBox.numberOfXSIMStatusMessages = 0;
            m_MessageBox.numberOfXSIMUpdateMessages = 0;
                    }
        }

        //if (m_HeartbeatsActive && m_TimeSinceLastHeartbeat >= m_TimeBetweenHeartbeats && Application.isPlaying)
        //{
        //    if (SimulatorSettings.getMaster())
        //    {
        //        sendHeartbeat();
        //    }
        //    else
        //    {
        //        slaveUpdate();
        //    }
        //    m_TimeSinceLastHeartbeat = 0;
        //}

        //if (numMessages > 1000)
        //{
        //    //Debug.Log("NSICS_Select returns(" + numMessages + ") - m_MessageStruct: totalNumberOfMessages(" + m_MessageStruct.totalNumberOfMessages + ") numberOfVIBECommandMessages (" + m_MessageStruct.numberOfVIBECommandMessages + ") numberOfVIBEControllerMessages (" + m_MessageStruct.numberOfVIBEControllerMessages + ") numberOfXSIMStatusMessages (" + m_MessageStruct.numberOfXSIMStatusMessages + ") numberOfXSIMUpdateMessages (" + m_MessageStruct.numberOfXSIMUpdateMessages + ")");

        //    int totalNumberOfMessages = numMessages;
        //    int numberOfVIBECommandMessages = NSICS_GetNumberOfVIBECommandMessages(NSI_Agent);
        //    int numberOfVIBEControllerMessages = NSICS_GetNumberOfVIBEControllerMessages(NSI_Agent);
        //    int numberOfXSIMStatusMessages = NSICS_GetNumberOfXSIMStatusMessages(NSI_Agent);
        //    int numberOfXSIMUpdateMessages = NSICS_GetNumberOfXSIMUpdateMessages(NSI_Agent);

        //    if (numberOfVIBECommandMessages > 1024) numberOfVIBECommandMessages = 1024;
        //    if (numberOfVIBEControllerMessages > 1024) numberOfVIBEControllerMessages = 1024;
        //    if (numberOfXSIMStatusMessages > 1024) numberOfXSIMStatusMessages = 1024;
        //    if (numberOfXSIMUpdateMessages > 1024) numberOfXSIMUpdateMessages = 1024;

        //    //Debug.Log("NSICS_Select returns(" + numMessages + ") - m_MessageStruct: totalNumberOfMessages(" + totalNumberOfMessages + ") numberOfVIBECommandMessages (" + numberOfVIBECommandMessages + ") numberOfVIBEControllerMessages (" + numberOfVIBEControllerMessages + ") numberOfXSIMStatusMessages (" + numberOfXSIMStatusMessages + ") numberOfXSIMUpdateMessages (" + numberOfXSIMUpdateMessages + ")");

        //    if (numberOfVIBECommandMessages > 0)
        //    {
        //        NSICS_GetVIBECommandMessagesByNum(NSI_Agent, m_VIBECommandMessages, numberOfVIBECommandMessages);
        //        parseVIBECommandMessage(m_VIBECommandMessages, numberOfVIBECommandMessages);
        //    }
        //    if (SimulatorSettings.getInputManager().getInputMode() == JoystickInputMode.NSI)
        //    {
        //        if (numberOfVIBEControllerMessages > 0)
        //        {
        //            NSICS_GetVIBEControllerMessagesByNum(NSI_Agent, m_VIBEControllerMessages, numberOfVIBEControllerMessages);
        //            parseVIBEControllerMessage(m_VIBEControllerMessages, numberOfVIBEControllerMessages);
        //        }
        //    }
        //    if (numberOfXSIMStatusMessages > 0)
        //    {
        //        NSICS_GetXSIMStatusMessagesByNum(NSI_Agent, m_XSIMStatusMessages, numberOfXSIMStatusMessages);
        //        parseXSIMStatusMessage(m_XSIMStatusMessages, numberOfXSIMStatusMessages);
        //    }
        //    if (numberOfXSIMUpdateMessages > 0)
        //    {
        //        for (int i = 0; i <= numberOfXSIMUpdateMessages; i++)
        //        {
        //            if (i > m_XSIMUpdateMessages.Length - 1)
        //            {
        //                break;
        //            }
        //            else
        //            {
        //                m_XSIMUpdateMessages[i] = m_BlankXSIMUpdateMessage;
        //            }
        //        }

        //        NSICS_GetXSIMUpdateMessagesByNum(NSI_Agent, m_XSIMUpdateMessages, numberOfXSIMUpdateMessages);
        //        parseXSIMUpdateMessage(m_XSIMUpdateMessages, numberOfXSIMUpdateMessages);
        //    }

        //    NSICS_ClearAllMessages(NSI_Agent);
        //}

        //if(m_TimeSinceLastHeartbeat > m_TimeBetweenHeartbeats)
        //{
        //    sendHeartbeat();
        //    m_TimeSinceLastHeartbeat = 0;
        //}
    }

    //public void Identify()
    //{
    //    if (m_NetworkAgentActive)
    //    {
    //        NSICS_Identify(NSI_Agent, out m_AnnounceStruct);
    //        if (m_AnnounceStruct.SourceName != null)
    //        {
    //            Debug.Log(m_AnnounceStruct.SourceName);
    //            m_VIBEInstanceName = m_AnnounceStruct.SourceName;
    //        }
    //        else
    //        {
    //            Debug.Log("m_AnnounceStruct.SourceName is null");
    //        }
    //    }
    //}

    public void ClearAllMessages()
    {
        if (m_NetworkAgentActive)
        {
            m_TotalNumberOfMessages = 0;
            m_NumberOfVIBECommandMessages = 0;
            m_NumberOfVIBEControllerMessages = 0;
            m_NumberOfXSIMStatusMessages = 0;
            m_NumberOfXSIMUpdateMessages = 0;
            m_NumberOfGlobalUpdateMessages = 0;
            m_NumberOfEnvUpdateMessages = 0;

            NSICS_ClearAllMessages(NSI_Agent);
        }
    }

    int getVIBEIDFromXSIMID(int a_XSIMID, string a_Src)
    {
        int l_VIBEID = -1;
        Dictionary<int, int> l_Dict;

        if (XSIMID_To_VIBEID_Dict.TryGetValue(a_Src, out l_Dict))
        {
            //Debug.Log("Source exists in Dict");
            l_Dict = XSIMID_To_VIBEID_Dict[a_Src];

            if (l_Dict.TryGetValue(a_XSIMID, out l_VIBEID))
            {
                //Debug.Log("Entry exists in Dict");
                l_VIBEID = l_Dict[a_XSIMID];
            }
            else
            {
                l_VIBEID = currentNetworkID;
                l_Dict.Add(a_XSIMID, l_VIBEID);
                currentNetworkID++;
            }
        }
        else
        {
            //Debug.Log("Source does not exist in Dict");
            l_Dict = new Dictionary<int, int>();
            l_VIBEID = currentNetworkID;
            l_Dict.Add(a_XSIMID, l_VIBEID);

            XSIMID_To_VIBEID_Dict.Add(a_Src, l_Dict);
            currentNetworkID++;
        }

        return l_VIBEID;
    }

    //public float getHeightForVehicleType(string a_VehicleType, Vector3 a_Position)
    //{
    //    float l_Ret = 0.0f;

    //    if (a_VehicleType.Equals("BOAT"))
    //    {
    //        l_Ret = m_EnvManager.GetHeightOfWaterAtPoint(a_Position);
    //    }
    //    else if (a_VehicleType.Equals("HOVERCRAFT"))
    //    {
    //        l_Ret = m_EnvManager.GetHeightOfWaterAtPoint(a_Position);
    //    }
    //    else if (a_VehicleType.Equals("QUADCOPTER"))
    //    {
    //        l_Ret = 30.0f;
    //    }
    //    else if (a_VehicleType.Equals("SUB"))
    //    {
    //        l_Ret = -20.0f;
    //    }
    //    else if (a_VehicleType.Equals("BUOY"))
    //    {
    //        l_Ret = m_EnvManager.GetHeightOfWaterAtPoint(a_Position);
    //    }
    //    else if (a_VehicleType.Equals("PROJECTILE"))
    //    {
    //        l_Ret = m_EnvManager.GetHeightOfWaterAtPoint(a_Position) - 7;
    //    }

    //    return l_Ret;
    //}

    //void parseVIBECommandMessage(VIBECommandMessage[] a_Messages, int a_NumMessages)
    //{
    //    if (a_Messages.Length >= a_NumMessages)
    //    {
    //        for (int i = 0; i < a_NumMessages; i++)
    //        {
    //            if (a_Messages[i].command != String.Empty)
    //            {
    //                CMD.ExecuteCommand(a_Messages[i].command.Trim());
    //            }
    //        }
    //    }
    //}

    //public void sendDebug(string a_DebugMessage, int a_Channel = 0)
    //{
    //    if (Application.isPlaying && m_NetworkAgentActive)
    //    {
    //        if (NSI_Agent != null)
    //        {
    //            NSICS_BroadcastDebugMessage(NSI_Agent, a_DebugMessage, a_Channel);
    //        }
    //        else
    //        {
    //            Debug.Log("Sim attempted to send a debug message before Net Monitor came online. Channel " + a_Channel + " - Contents('" + a_DebugMessage + "')");
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log(a_DebugMessage);
    //    }
    //}

    //public void slaveUpdate()
    //{
    //    if (m_NetworkAgentActive)
    //    {
    //        TranslateToContactUpdate(ref m_SlaveUpdate, SimulatorSettings.getActiveEntity());

    //        m_SlaveUpdate.source = m_VIBEInstanceName.ToCharArray();
    //        m_SlaveUpdate.useExternalPhysics = 1;

    //        if (m_VIBEInstanceName.Length > 0)
    //        {
    //            m_SlaveUpdate.source = m_VIBEInstanceName.ToCharArray();
    //        }

    //        NSICS_BroadcastVIBESlaveUpdate(NSI_Agent, ref m_SlaveUpdate);
    //    }
    //}

    //public void sendHeartbeat()
    //{

    //    if (Application.isPlaying && m_NetworkAgentActive)
    //    {
    //        if (m_Initialized)
    //        {
    //            int i = 0;

    //            m_HeartbeatMessage.numberOfContacts = SimulatorSettings.getEntityList().m_NumEntities;

    //            //if (m_ValidHBEntities.Count > 99) m_ValidHBEntities.RemoveRange(99, m_ValidHBEntities.Count - 99);

    //            m_HeartbeatMessage.viewAttachedID = SimulatorSettings.getDirector().getCurrentEntity().GetComponent<Entity>().m_EntityID;

    //            foreach (Entity l_Entity in SimulatorSettings.getEntityList().getEntities())
    //            {
    //                TranslateToContactUpdate(ref m_HeartbeatMessage.contacts[i], l_Entity); // old method
    //                i++;

    //                //if (l_Entity.m_SerializationCreated)
    //                //{
    //                //    m_HeartbeatMessage.contacts[i] = l_Entity.m_SerializedEntityUpdate;
    //                //    i++;
    //                //}
    //            }

    //            //Debug.Log("m_HeartbeatMessage.numberOfContacts: " + m_HeartbeatMessage.numberOfContacts + " - m_HeartbeatMessage.viewAttachedID: " + m_HeartbeatMessage.viewAttachedID);
    //            NSICS_BroadcastHeartbeat(NSI_Agent, ref m_HeartbeatMessage);
    //        }
    //    }
    //}
    //void TranslateToContactUpdate(ref contactUpdate a_Contact, Entity a_Entity)
    //{
    //    if (a_Entity)
    //    {
    //        a_Contact.source = a_Entity.m_SourceName.ToCharArray();
    //        a_Contact.ID = a_Entity.m_EntityID;

    //        a_Contact.seciID = a_Entity.m_EntityID;
    //        a_Contact.smmttid = a_Entity.m_EntitySMMTTID;

    //        a_Contact.lat = a_Entity.currentGPSPosition.x;
    //        a_Contact.lon = a_Entity.currentGPSPosition.y;
    //        a_Contact.altitude = a_Entity.m_CurrentAltitude;

    //        a_Contact.state = (int)a_Entity.m_EntityState;
    //        a_Contact.useExternalPhysics = 1;

    //        a_Contact.pitch = a_Entity.transform.rotation.eulerAngles.x;
    //        a_Contact.heading = a_Entity.transform.rotation.eulerAngles.y;
    //        a_Contact.roll = a_Entity.transform.rotation.eulerAngles.z;

    //        a_Contact.viewHeading = SimulatorSettings.getDirector().m_CurrentViewRotation.y;
    //        a_Contact.viewHeight = SimulatorSettings.getDirector().m_CurrentViewHeight;
    //        a_Contact.viewPitch = SimulatorSettings.getDirector().m_CurrentViewRotation.x;
    //        a_Contact.viewRoll = SimulatorSettings.getDirector().m_CurrentViewRotation.z;
    //    }
    //}

    //int parseStringToInt(string a_Input)
    //{
    //    int l_Ret = -1;

    //    if (int.TryParse(a_Input, out l_Ret)) // Valid input, do something with it.
    //    {

    //    }
    //    else // Not a number, do something else with it.
    //    {
    //        l_Ret = -1;
    //    }

    //    return l_Ret;
    //}

    //void parseVIBEControllerMessage(VIBETranslatedControllerMessage[] a_Messages, int a_NumMessages)
    //{

    //    for (int i = 0; i < a_NumMessages; i++)
    //    {
    //        //Debug.Log(Marshal.PtrToStringAnsi(a_Messages[i].joystickName));

    //        string l_JoystickName = Marshal.PtrToStringAnsi(a_Messages[i].joystickName);
    //        string l_JoystickIP = "";
    //        string l_JoystickType = "";
    //        int l_JoystickNumber = 0;

    //        string[] l_Words = l_JoystickName.Split('-');

    //        if (l_Words.Length > 2)
    //        {
    //            l_JoystickIP = l_Words[0];
    //            l_JoystickType = l_Words[1];
    //            l_JoystickNumber = parseStringToInt(l_Words[2]);
    //        }


    //        Entity l_Entity = null;
    //        ControllerSourceInformation l_ControllerSource;

    //        if (l_JoystickName == null)
    //        {
    //            continue;
    //        }

    //        if (!m_InputManager.controllerExistsInList(l_JoystickName))
    //        {
    //            l_ControllerSource = m_InputManager.addControllerSource(l_JoystickName);

    //            if (l_ControllerSource != null)
    //            {
    //                if (m_InputManager.m_NumberOfControllerSources == 1) // If this is the first controller added
    //                {
    //                    l_Entity = SimulatorSettings.getActiveEntity();
    //                    if (SimulatorSettings.getActiveEntity().m_ControllerAssigned == false)
    //                    {
    //                        m_InputManager.assignControllerSource(l_ControllerSource.SourceVIBEJoystickID, l_Entity.m_EntityID);
    //                    }

    //                }
    //                else
    //                {
    //                    l_Entity = m_InputManager.getControlledObject(l_JoystickName);
    //                }
    //            }
    //            else
    //            {
    //                Debug.Log("addControllerSource return null for controllerinfo: " + l_JoystickName);
    //            }
    //        }
    //        else
    //        {
    //            l_ControllerSource = m_InputManager.getControllerSourceInfo(l_JoystickName);
    //            if (l_ControllerSource != null)
    //            {
    //                if (l_ControllerSource.ConnectedObjectController != null)
    //                {
    //                    l_Entity = l_ControllerSource.ConnectedObjectController;
    //                }
    //            }
    //        }

    //        if (l_Entity == null)
    //        {
    //            Debug.Log("Controller " + m_InputManager.getControllerSource(l_JoystickName).SourceVIBEJoystickID + "]: " + l_JoystickName + " exists in the list, but it has no assigned object. Skipping Message Number: " + i + ".");
    //        }
    //        else
    //        {
    //            string l_Event = "";

    //            //Debug.Log("Command: " + a_Messages[i].eventType + " " + a_Messages[i].componentID + " " + a_Messages[i].value + " on Entity: " + l_Entity.name);

    //            switch (a_Messages[i].eventType)
    //            {
    //                case (int)VIBEControllerEventType.AXISMOTION:
    //                    l_Event = m_InputManager.getAxisNameFromNumber(l_ControllerSource.SourceJoystickName, a_Messages[i].componentID).Trim();
    //                    m_InputManager.HandleNetworkControllerInput(l_Entity, l_Event, (float)a_Messages[i].value);
    //                    break;
    //                case (int)VIBEControllerEventType.BUTTONDOWN:
    //                    l_Event = m_InputManager.getButtonNameFromNumber(l_JoystickType, a_Messages[i].componentID).Trim();
    //                    m_InputManager.HandleNetworkControllerInput(l_Entity, l_Event, 1.0f);
    //                    break;
    //                case (int)VIBEControllerEventType.BUTTONUP:
    //                    l_Event = m_InputManager.getButtonNameFromNumber(l_JoystickType, a_Messages[i].componentID);
    //                    m_InputManager.HandleNetworkControllerInput(l_Entity, l_Event, 0.0f);
    //                    break;
    //                case (int)VIBEControllerEventType.JOYHAT:
    //                    l_Event = m_InputManager.getDPadNameFromNumber(l_JoystickType, (int)a_Messages[i].value);
    //                    m_InputManager.HandleNetworkControllerInput(l_Entity, l_Event, 0.0f);
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }
    //    }
    //}

    //void parseXSIMStatusMessage(XSIMStatusMessage[] a_Messages, int a_NumMessages)
    //{
    //    for (int i = 0; i < a_NumMessages; i++)
    //    {

    //        Debug.Log("Message Type: " + ((XSIMNotificationOrRequest)a_Messages[i].notificationOrRequest).ToString("F") + " - Status: " + ((XSIMStatus)a_Messages[i].status).ToString("F"));

    //        if (a_Messages[i].notificationOrRequest == (int)XSIMNotificationOrRequest.REQUEST)
    //        {

    //        }
    //        else if (a_Messages[i].notificationOrRequest == (int)XSIMNotificationOrRequest.NOTIFICATION)
    //        {
    //            if ((XSIMStatus)a_Messages[i].status == XSIMStatus.SIM_RELOADED)
    //            {
    //                Debug.Log("XSIM Reloaded. Removing Entities.");

    //                //m_EntityList.removeAllEntities();
    //                RemoveAllNetworkVehicles();
    //            }
    //            if ((XSIMStatus)a_Messages[i].status == XSIMStatus.SIM_PAUSED)
    //            {
    //                Debug.Log("XSIM Paused. Animations paused.");

    //                SimulatorSettings.getAnimationManager().Playing(false);
    //                m_EntityList.PauseNetworkVehicles(true);
    //            }
    //            if ((XSIMStatus)a_Messages[i].status == XSIMStatus.SIM_UNPAUSED)
    //            {
    //                Debug.Log("XSIM Unpaused. Animations resumed.");

    //                SimulatorSettings.getAnimationManager().Playing(true);
    //                m_EntityList.PauseNetworkVehicles(false);
    //            }
    //        }
    //    }
    //}

    //void RemoveAllNetworkVehicles()
    //{
    //    foreach (Entity l_Entity in m_NetworkVehicles)
    //    {
    //        m_EntityList.removeEntity(l_Entity);
    //    }

    //    m_NetworkVehicles.Clear();
    //}
    //void parseGlobalUpdateMessage(Global[] a_Messages, int a_NumMessages)
    //{
    //    for (int i = 0; i < a_NumMessages; i++)
    //    {
    //        //Debug.Log("Parsing Global Update");

    //        SimulatorSettings.getTimeManager().SetTime(a_Messages[i].year, a_Messages[i].month, a_Messages[i].day, a_Messages[i].time * 3600);
    //        //SimulatorSettings.getTimeManager().SetDate(a_Messages[i].year, a_Messages[i].month, a_Messages[i].day);
    //        //SimulatorSettings.getTimeManager().SetTimeOfDay((float)a_Messages[i].time);
    //    }
    //}

    //void parseEnvUpdateMessage(Global_Env[] a_Messages, int a_NumMessages)
    //{
    //    for (int i = 0; i < a_NumMessages; i++)
    //    {
    //        //Debug.Log("Parsing Env Update");
    //        //Debug.Log(a_Messages[i].cloudCeiling + " - " + a_Messages[i].cloudDensity + " - " + a_Messages[i].precipIntensity);
    //        SimulatorSettings.getEnvironmentManager().changeWeatherID(a_Messages[i].cloudDensity);
    //        CMD.ExecuteCommand("setFog " + a_Messages[i].visibilityRange);

    //        if (a_Messages[i].seaState != SimulatorSettings.getEnvironmentManager().m_SeaState)
    //        {
    //            CMD.ExecuteCommand("seaState " + a_Messages[i].seaState);
    //        }

    //    }
    //}

    //bool IsValidXSIMUpdate(XSIMUpdateMessage a_Message)
    //{
    //    if (a_Message.smmttid == -1) return false;
    //    if (IsNaN(a_Message)) return false;

    //    if (!m_EntityList.vehicleTypeExistsInList(Mathf.Abs(a_Message.smmttid)))
    //    {
    //        Debug.Log("Got invalid SMMTID: " + a_Message.smmttid);
    //        return false; // Ignore invalid SMMTID
    //    }

    //    if (!SimulatorSettings.getMaster())
    //    {
    //        if (a_Message.source == m_VIBEInstanceName)
    //        {
    //            return false;
    //        }
    //    }

    //    //if (a_Message.source == "VIBE") return false;

    //    return true;
    //}

    //void GetSIMID(int a_XSIMID, int a_SECIID, string a_Source)
    //{
    //    m_CurrentVIBEIDBeingProcessed = a_SECIID;

    //    if (a_Source == "SECI")
    //    {
    //        m_CurrentXSIMIDBeingProcessed = m_CurrentVIBEIDBeingProcessed;
    //    }
    //    else
    //    {
    //        m_CurrentXSIMIDBeingProcessed = getVIBEIDFromXSIMID(a_XSIMID, a_Source);
    //        m_CurrentVIBEIDBeingProcessed = m_CurrentXSIMIDBeingProcessed;
    //    }
    //}

    //bool CheckIfStatusFlagMessage(byte a_StatusFlag)
    //{
    //    if (a_StatusFlag == 255)
    //    {
    //        Debug.Log("Got status flag remove on seciID: " + m_CurrentVIBEIDBeingProcessed);
    //        m_CurrentEntityBeingProcessed.m_QueuedForDestruction = true;
    //        SimulatorSettings.getEntityList().removeEntity(m_CurrentVIBEIDBeingProcessed);
    //        return true;
    //    }

    //    return false;
    //}

    //void parseXSIMUpdateMessage(XSIMUpdateMessage[] a_Messages, int a_NumMessages)
    //{
    //    for (int i = 0; i < a_NumMessages; i++)
    //    {
    //        if (!IsValidXSIMUpdate(a_Messages[i])) continue; // Check if it's a valid message
    //        if (CheckIfStatusFlagMessage(a_Messages[i].statusFlag)) continue; // Check if it's a StatusFlag message

    //        GetSIMID(a_Messages[i].ID, a_Messages[i].seciID, a_Messages[i].source); // Determine the local sim ID

    //        m_CurrentEntityBeingProcessed = m_EntityList.getEntityByVIBEID(m_CurrentVIBEIDBeingProcessed); // Get the entity from the EntityList

    //        // Saves memory to use a predetermined member variable for temp storage
    //        m_CurrentGPSPosBeingProcessed.x = (float)a_Messages[i].lat;
    //        m_CurrentGPSPosBeingProcessed.y = (float)a_Messages[i].lon;

    //        if (m_CurrentEntityBeingProcessed == null) // If the entity does not exist, create it
    //        {
    //            addVehicle(m_CurrentVIBEIDBeingProcessed, a_Messages[i].smmttid, m_CurrentXSIMIDBeingProcessed, m_CurrentGPSPosBeingProcessed, (float)a_Messages[i].altitude, a_Messages[i].source, a_Messages[i].timeReceived, (float)a_Messages[i].heading, (float)a_Messages[i].speed, a_Messages[i].state, true, Convert.ToBoolean(a_Messages[i].useExternalPhysics));

    //            if (m_AllowExternalPhysics) m_CurrentEntityBeingProcessed.m_UseExternalPhysics = Convert.ToBoolean(a_Messages[i].useExternalPhysics);
    //            else m_CurrentEntityBeingProcessed.m_UseExternalPhysics = false;
    //        }
    //        else // If the entity already exists, update it
    //        {
    //            switch (m_CurrentEntityBeingProcessed.m_ControlType)
    //            {
    //                case EntityControlType.NETWORK:
    //                    updateVehicle(m_CurrentEntityBeingProcessed, a_Messages[i]);
    //                    break;
    //                case EntityControlType.PLAYER:
    //                    setVehicle(m_CurrentEntityBeingProcessed, a_Messages[i]);
    //                    break;
    //                default:
    //                    break;
    //            }


    //        }

    //    }

    //}

    //public void requestExternalSimulationState(string a_State)
    //{
    //    if (Application.isPlaying && m_NetworkAgentActive)
    //    {
    //        XSIMStatus l_Status = XSIMStatus.UNKNOWN;

    //        if ((a_State.Equals("INITIAL_POSITIONS")) || (a_State.Equals("Static")))
    //        {
    //            l_Status = XSIMStatus.INITIAL_POSITIONS;
    //        }
    //        else if ((a_State.Equals("SIM_PAUSED")) || (a_State.Equals("Pause")))
    //        {
    //            l_Status = XSIMStatus.SIM_PAUSED;
    //        }
    //        else if ((a_State.Equals("SIM_RELOADED")) || (a_State.Equals("Reload")))
    //        {
    //            l_Status = XSIMStatus.SIM_RELOADED;
    //        }
    //        else if ((a_State.Equals("SIM_START")) || (a_State.Equals("Start")))
    //        {
    //            l_Status = XSIMStatus.SIM_START;
    //        }
    //        else if ((a_State.Equals("SIM_UNPAUSED")) || (a_State.Equals("Unpause")))
    //        {
    //            l_Status = XSIMStatus.SIM_UNPAUSED;
    //        }

    //        if (l_Status != XSIMStatus.UNKNOWN)
    //        {
    //            NSICS_RequestSimulationState(NSI_Agent, (int)l_Status);
    //        }
    //        else
    //        {
    //            Debug.Log("User input unknow state (Options are Static, Pause, Reload, Start, Unpause)");
    //        }
    //    }
    //}

    //public void requestExternalSimulationState(XSIMStatus a_Status)
    //{
    //    if (Application.isPlaying && m_NetworkAgentActive)
    //    {
    //        NSICS_RequestSimulationState(NSI_Agent, (int)a_Status);
    //    }
    //}

    //public void requestInititialPositionsFromExternalSims()
    //{
    //    if (Application.isPlaying && m_NetworkAgentActive)
    //    {
    //        NSICS_RequestInitialPositions(NSI_Agent);
    //    }
    //}

    //void SetStateIfValid(Entity a_Entity, int a_RawState)
    //{
    //    if (Enum.IsDefined(typeof(EntityState), a_RawState))
    //    {
    //        a_Entity.setState((EntityState)a_RawState);
    //    }
    //}

    //public bool IsNaN(XSIMUpdateMessage a_Message)
    //{
    //    if (System.Double.IsNaN(a_Message.lat)) return true;
    //    else if (System.Double.IsNaN(a_Message.lon)) return true;
    //    else if (System.Single.IsNaN(a_Message.pitch)) return true;
    //    else if (System.Single.IsNaN(a_Message.heading)) return true;
    //    else if (System.Single.IsNaN(a_Message.roll)) return true;

    //    return false;
    //}

    //public void updateVehicle(Entity a_Entity, XSIMUpdateMessage a_Message)
    //{
    //    if (a_Entity != null)
    //    {
    //        Vector3 currentXYZPosition = a_Entity.transform.position;
    //        Vector3 newXYZDestination = GPSEncoder.GPSToUCS((float)a_Message.lat, (float)a_Message.lon);

    //        if (a_Message.useExternalPhysics == 0)
    //        {
    //            newXYZDestination.y = currentXYZPosition.y;

    //            a_Message.altitude = a_Entity.m_CurrentAltitude;
    //        }
    //        else // Use external physics
    //        {
    //            newXYZDestination.y = a_Message.altitude;

    //            //Debug.Log("Heading is: " + a_Message.heading);

    //        }

    //        a_Entity.addPosition(a_Message);
    //        a_Entity.SetStatusFlag(a_Message.statusFlag);
    //        SetStateIfValid(a_Entity, a_Message.state);
    //    }
    //    else
    //    {
    //        Debug.Log("Network Monitor is attempting to use a prefab that doesn't have a vehicle controller. This will not function correctly.");
    //    }
    //}


    //public void setVehicle(Entity a_Entity, XSIMUpdateMessage a_Message)
    //{
    //    //FIXME - This should eventually be pulled out and placed in a background loader

    //    Entity l_Controller = a_Entity;

    //    if (l_Controller != null)
    //    {
    //        if (a_Message.state != 0) Debug.Log("[" + a_Message.timeReceived + "] Netmonitor received message for " + a_Entity.m_EntityName + "(" + l_Controller.m_EntityID + ") to switch to state: " + a_Message.state + "(" + (EntityState)a_Message.state as string + ")");

    //        //l_Controller.m_EntityState = (EntityState)a_Message.state;

    //        Vector3 currentXYZPosition = l_Controller.transform.position;
    //        Vector3 newXYZDestination = GPSEncoder.GPSToUCS((float)a_Message.lat, (float)a_Message.lon);
    //        newXYZDestination.y = currentXYZPosition.y;

    //        a_Message.altitude = l_Controller.m_CurrentAltitude;

    //        l_Controller.SetPosition(newXYZDestination);

    //        l_Controller.m_StatusFlag = a_Message.statusFlag;

    //        l_Controller.setState((EntityState)a_Message.state);

    //    }
    //}

    //public void addVehicle(XSIMUpdateMessage a_Message)
    //{
    //    bool extPhysics;

    //    if (a_Message.useExternalPhysics == 0) extPhysics = false;
    //    else extPhysics = true;

    //    addVehicle(a_Message.seciID, a_Message.smmttid, a_Message.ID, new Vector2((float)a_Message.lat, (float)a_Message.lon), a_Message.altitude, a_Message.source, a_Message.timeReceived, a_Message.heading, a_Message.speed, a_Message.state, true, extPhysics);
    //}

    //public void addVehicle(XSIMUpdateMessage a_Message, bool a_IsNetworkVehicle)
    //{
    //    addVehicle(a_Message.seciID, a_Message.smmttid, a_Message.ID, new Vector2((float)a_Message.lat, (float)a_Message.lon), a_Message.altitude, a_Message.source, a_Message.timeReceived, a_Message.heading, a_Message.speed, a_Message.state, true, a_IsNetworkVehicle);
    //}

    //public void addVehicle(int a_ID, int a_SMMTID, int a_XSIMID, Vector2 a_Coord, float a_Elevation, string a_Source, float timeReceived, float a_Heading = 0, float a_Speed = 0, int a_State = 0, bool a_NetworkVehicle = true, bool a_UseExternalPhysics = false)
    //{
    //    string l_VehicleType;

    //    bool l_LoadModel = false;
    //    bool l_Visible = true;

    //    bool l_HasParent = false;
    //    Entity l_Parent = null;

    //    Vector3 newXYZDestination = GPSEncoder.GPSToUCS(a_Coord.x, a_Coord.y);

    //    if (a_SMMTID < 0)
    //    {
    //        l_Visible = false;
    //        a_SMMTID = Mathf.Abs(a_SMMTID);
    //    }

    //    if (a_SMMTID == 1501 || a_SMMTID == 2501)
    //    {
    //        Entity l_Launcher = m_EntityList.getClosestEntityOfSMMTTIDIgnoringY(newXYZDestination, 2205);

    //        if (!l_Launcher)
    //        {
    //            l_Launcher = m_EntityList.getClosestEntityOfSMMTTIDIgnoringY(newXYZDestination, 2202);
    //        }

    //        Entity l_Target = m_EntityList.getClosestEntityOfSMMTTIDIgnoringY(newXYZDestination, 94);

    //        if (!l_Target)
    //        {
    //            l_Target = m_EntityList.getClosestEntityOfSMMTTIDIgnoringY(newXYZDestination, 2001);
    //        }

    //        if (l_Launcher && l_Target)
    //        {
    //            if (l_Launcher.m_NumberOfEffectorsRemaining > 0)
    //            {
    //                SimulatorSettings.getEffectorLauncher().launchEffector(l_Launcher, l_Target, a_SMMTID);
    //            }
    //        }

    //        return;
    //    }

    //    if (a_SMMTID == 99)
    //    {
    //        Entity l_Launcher = m_EntityList.getClosestEntityOfSMMTTIDIgnoringY(newXYZDestination, 100);

    //        if (l_Launcher)
    //        {
    //            if (Vector3.Distance(l_Launcher.transform.position, newXYZDestination) < 1)
    //            {
    //                if (l_Launcher.m_ControlObjects.GetControlObject("DockingPoint"))
    //                {
    //                    newXYZDestination = l_Launcher.m_ControlObjects.GetControlObject("DockingPoint").transform.position;

    //                    l_Parent = l_Launcher;
    //                    l_HasParent = true;
    //                }
    //            }
    //        }

    //    }



    //    l_VehicleType = m_EntityList.getVehicleTypeFromSMMTTID(a_SMMTID);

    //    //if (l_VehicleType == null)
    //    //{
    //    //    Debug.Log("Vehicle Type for VTID: " + smmttid + " returned null");
    //    //    return;
    //    //}

    //    //Debug.DrawLine(newXYZDestination, newXYZDestination + new Vector3(0, 5, 0), Color.red, 30);

    //    ModelInformation l_ModelInfo = SimulatorSettings.getModelManager().getModelInformation(a_SMMTID);


    //    //GameObject l_Object = Resources.Load("Prefabs/" + l_ModelInfo.ModelName + "_Prefab") as GameObject; // Attempt to load specific prefab
    //    //if (l_Object != null)
    //    //{
    //    //    Debug.Log("Loading prefab.");
    //    //}
    //    //else
    //    //{
    //    //    //Does not have a vehicle-specific prefab, load standard boat prefab

    //    //    l_Object = m_ModelManager.getStandardVehicleParts(smmttid);
    //    //    l_LoadModel = true;
    //    //    Debug.Log("Loading Generic: " + l_Object.name);
    //    //}

    //    ////entity = Instantiate(l_Object, newXYZDestination, Quaternion.identity);



    //    Entity l_Controller = m_EntityList.createAndAddEntity(a_ID, a_SMMTID, newXYZDestination, false); //entity.GetComponent<Entity>();

    //    if (l_Controller)
    //    {
    //        l_Controller.m_EntityType = l_VehicleType;
    //        l_Controller.m_SourceName = a_Source;
    //        l_Controller.m_XSIMID = a_XSIMID;

    //        l_Controller.m_NetworkVehicleHasBeenPlaced = true;

    //        if (a_NetworkVehicle) l_Controller.switchEntityControl(EntityControlType.NETWORK);
    //        else l_Controller.switchEntityControl(EntityControlType.NONE);

    //        l_Controller.setMovementEnabled(true);
    //        l_Controller.m_OriginalPosition = newXYZDestination;
    //        l_Controller.m_NetworkMonitor = this;

    //        m_EntityList.assignDisplayName(l_Controller);

    //        l_Controller.m_CurrentAltitude = newXYZDestination.y;

    //        l_Controller.m_DesiredAltitude = l_Controller.m_CurrentAltitude;


    //        if (a_NetworkVehicle)
    //        {
    //            // Assume the height value is unfilled

    //            if ((m_EnvManager != null) && (Application.isPlaying))
    //            {
    //                newXYZDestination.y = getHeightForVehicleType(l_VehicleType, newXYZDestination);
    //            }
    //            if (l_Controller.m_EntityType == "TETHER")
    //            {
    //                Tethered l_Tethered = l_Controller as Tethered;
    //                l_Tethered.m_TimeToRiseToTargetHeightInSeconds = l_ModelInfo.TimeToRise;
    //            }

    //            if (l_Controller.m_EntityType == "BUOY")
    //            {
    //                Buoy l_Buoy = l_Controller as Buoy;
    //                l_Buoy.m_TimeToRiseToTargetHeightInSeconds = l_ModelInfo.TimeToRise;
    //            }
    //            if (l_Controller.m_EntityType == "QUADCOPTER")
    //            {
    //                Entity l_Launcher = m_EntityList.getClosestEntityOfSMMTTIDIgnoringY(l_Controller, 2301);

    //                if (l_Launcher == null)
    //                {
    //                    l_Launcher = m_EntityList.getClosestEntityOfSMMTTIDIgnoringY(l_Controller, 2300);
    //                }

    //                if (l_Launcher)
    //                {
    //                    float l_WaterHeight = m_EnvManager.GetHeightOfWaterAtPoint(l_Launcher.transform.position);
    //                    l_Controller.m_CurrentAltitude = l_WaterHeight + l_Controller.m_ModelBounds.extents.y;
    //                }
    //                else
    //                {
    //                    float l_WaterHeight = m_EnvManager.GetHeightOfWaterAtPoint(l_Controller.transform.position);
    //                    l_Controller.m_CurrentAltitude = l_WaterHeight;
    //                }

    //                l_Controller.m_DesiredAltitude = newXYZDestination.y;
    //                l_Controller.m_EnableElevationUpdates = true;
    //                l_Controller.m_ElevationChangeSpeed = 0.1f;

    //                Debug.Log("Network Quadcopter created with desired Altitude:" + l_Controller.m_DesiredAltitude);
    //            }

    //            if (l_Controller.m_EntityType == "PROJECTILE")
    //            {
    //                Projectile l_Projectile = l_Controller as Projectile;

    //                switch (l_Projectile.m_TrackingType)
    //                {
    //                    case Projectile_TrackingType.DUMMY_TRACKING:
    //                        l_Controller.m_ControlType = EntityControlType.SCRIPT;
    //                        break;
    //                    case Projectile_TrackingType.ID_TRACKING:
    //                        l_Controller.m_ControlType = EntityControlType.SCRIPT;
    //                        break;
    //                    case Projectile_TrackingType.NETWORK:
    //                        l_Controller.m_ControlType = EntityControlType.NETWORK;
    //                        break;
    //                    case Projectile_TrackingType.POINT_TRACKING:
    //                        l_Controller.m_ControlType = EntityControlType.SCRIPT;
    //                        break;
    //                    case Projectile_TrackingType.SEEKER_TRACKING:
    //                        l_Controller.m_ControlType = EntityControlType.SCRIPT;
    //                        break;
    //                }

    //                if (l_Controller.m_EntitySMMTTID == 1502) // Network Controlled / Signal-Triggered Detonation Effectors
    //                {
    //                    Entity l_Launcher = m_EntityList.getClosestEntityOfTypeIgnoringY(l_Controller.transform.position, "BOAT");

    //                    if (l_Launcher)
    //                    {
    //                        l_Controller.m_CurrentAltitude = l_Launcher.transform.position.y;
    //                        l_Controller.m_DesiredAltitude = 30;
    //                        l_Controller.m_EnableElevationUpdates = true;
    //                        l_Controller.m_ElevationChangeSpeed = 0.06f;
    //                    }
    //                }

    //                if (l_Controller.m_EntitySMMTTID == 2502) // Network Controlled / Signal-Triggered Detonation Effectors
    //                {
    //                    Entity l_Launcher = m_EntityList.getClosestEntityOfSMMTTIDIgnoringY(l_Controller, 2202);

    //                    if (l_Launcher)
    //                    {
    //                        l_Controller.m_CurrentAltitude = l_Launcher.transform.position.y;
    //                        l_Controller.m_DesiredAltitude = 0;
    //                        l_Controller.m_EnableElevationUpdates = true;
    //                        l_Controller.m_ElevationChangeSpeed = 0.08f;
    //                    }
    //                }

    //                if (l_Controller.m_EntitySMMTTID == 1501 || l_Controller.m_EntitySMMTTID == 2501) // Entity Tracking / Impact-Triggered Detonation Effectors
    //                {
    //                    Entity l_Launcher = m_EntityList.getClosestEntityOfSMMTTIDIgnoringY(l_Controller, 2205);

    //                    if (l_Launcher)
    //                    {
    //                        l_Controller.m_CurrentAltitude = l_Launcher.transform.position.y;
    //                    }

    //                    Entity l_Target = m_EntityList.getClosestEntityOfSMMTTIDIgnoringY(l_Controller, 94);

    //                    if (l_Target)
    //                    {
    //                        l_Projectile.m_ActualTarget = l_Target.gameObject;
    //                        l_Projectile.m_ControlType = EntityControlType.SCRIPT;
    //                        l_Projectile.m_AcceptNewUpdates = false;
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            l_Controller.m_CurrentAltitude = a_Elevation;
    //        }


    //        l_Controller.InitialPlacement(new Vector3(l_Controller.transform.position.x, l_Controller.m_CurrentAltitude, l_Controller.transform.position.z), new Vector3(0, (float)a_Heading, 0));

    //        l_Controller.setVisibleInScene(l_Visible);

    //        if (l_HasParent)
    //        {
    //            l_Controller.SetParent(l_Parent);
    //        }

    //        l_Controller.setState((EntityState)a_State);
    //        l_Controller.ExternalStart();

    //        if (l_LoadModel) l_Controller.m_ModelLoader.LoadModel(l_ModelInfo.ModelName);

    //        m_CurrentEntityBeingProcessed = l_Controller;
    //    }

    //    if (Application.isPlaying) m_NetworkVehicles.Add(l_Controller);
    //    l_Controller.gameObject.SetActive(true);

    //    if (m_MoveCameraToFirstNetworkVehicle)
    //    {
    //        if (SimulatorSettings.getActiveEntity().m_EntityID != 0) SimulatorSettings.getDirector().switchtoVehicleByID(0);
    //        SimulatorSettings.getEntityList().moveEntityToEntity(0, l_Controller.m_EntityID);
    //        m_MoveCameraToFirstNetworkVehicle = false;
    //    }
    //}

    //public double GetNSITime()
    //{
    //    //Debug.Log("GetNSITime");
    //    if (m_NetworkAgentActive)
    //    {
    //        return NSICS_Gettime(NSI_Agent);
    //    }
    //    else
    //    {
    //        return 0;
    //    }
    //}

    //public void toggleHeartbeats()
    //{
    //    toggleHeartbeats(!m_HeartbeatsActive);
    //}

    //public void toggleHeartbeats(bool a_Bool)
    //{
    //    m_HeartbeatsActive = a_Bool;
    //}

    //public void BroadcastKingMeMessage()
    //{
    //    if (m_NetworkAgentActive)
    //    {
    //        NSICS_BroadcastKingMe(NSI_Agent);
    //    }
    //}
}
