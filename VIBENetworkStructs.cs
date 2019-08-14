using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct VIBEControllerMessage
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string cmd;
    [MarshalAs(UnmanagedType.I4)]
    public int deviceID;
    [MarshalAs(UnmanagedType.R8)]
    public double val;
    [MarshalAs(UnmanagedType.R8)]
    public double timestamp;
    [MarshalAs(UnmanagedType.I4)]
    public int sampcnt;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string joyName;
    [MarshalAs(UnmanagedType.I4)]
    public int joyAxes;
    [MarshalAs(UnmanagedType.I4)]
    public int joyButtons;
};

public struct VIBETranslatedControllerMessage
{

    [MarshalAs(UnmanagedType.I4)]
    public int componentID;

    [MarshalAs(UnmanagedType.R8)]
    public double value;

    [MarshalAs(UnmanagedType.I4)]
    public int eventType;

    public IntPtr joystickName;
};

//[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
//public struct VIBEControllerMessage
//{
//    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
//    public string cmd;
//    [MarshalAs(UnmanagedType.I4)]
//    public int deviceID;
//    [MarshalAs(UnmanagedType.R8)]
//    public double val;
//    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
//    public string joyName;
//};


public struct XSIMStatusMessage
{
    [MarshalAs(UnmanagedType.R8)]
    public float simulationTime;
    [MarshalAs(UnmanagedType.I4)]
    public int messageVersion;
    [MarshalAs(UnmanagedType.I4)]
    public int status;
    [MarshalAs(UnmanagedType.I4)]
    public int notificationOrRequest;
};

public struct XSIMUpdateMessage
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string source;
    [MarshalAs(UnmanagedType.U1)]
    public byte statusFlag;
    [MarshalAs(UnmanagedType.R8)]
    public float timeReceived;

    [MarshalAs(UnmanagedType.I4)]
    public int smmttid;
    [MarshalAs(UnmanagedType.I4)]
    public int ID;
    [MarshalAs(UnmanagedType.I4)]
    public int seciID;			// Unique ID assigned by VIBE
    [MarshalAs(UnmanagedType.I4)]
    public int targetAscID;
    [MarshalAs(UnmanagedType.I4)]
    public int hullNum;
    [MarshalAs(UnmanagedType.I4)]
    public int autopilot;
    [MarshalAs(UnmanagedType.I4)]
    public int useExternalPhysics;

    [MarshalAs(UnmanagedType.R8)]
    public float lat;
    [MarshalAs(UnmanagedType.R8)]
    public float lon;
    [MarshalAs(UnmanagedType.R8)]
    public float heading;
    [MarshalAs(UnmanagedType.R8)]
    public float altitude;

    [MarshalAs(UnmanagedType.R8)]
    public float pitch;
    [MarshalAs(UnmanagedType.R8)]
    public float roll;

    [MarshalAs(UnmanagedType.R8)]
    public float course;
    [MarshalAs(UnmanagedType.R8)]
    public float speed;

    [MarshalAs(UnmanagedType.I4)]
    public int state;

    [MarshalAs(UnmanagedType.I4)]
    public int EngineSmokeOn;
    [MarshalAs(UnmanagedType.I4)]
    public int FlamesPresent;
    [MarshalAs(UnmanagedType.I4)]
    public int SmokePlumePresent;
    [MarshalAs(UnmanagedType.I4)]
    public int RunningLights;

    [MarshalAs(UnmanagedType.R8)]
    public double viewHeight;
    [MarshalAs(UnmanagedType.R8)]
    public double viewHeading;
    [MarshalAs(UnmanagedType.R8)]
    public double viewPitch;
    [MarshalAs(UnmanagedType.R8)]
    public double viewRoll;
};

//public struct XSIMUpdateMessage
//{
//    [MarshalAs(UnmanagedType.I4)]
//    public int nsi_messageCount;
//    [MarshalAs(UnmanagedType.R8)]
//    public double nsi_timeRec;

//    [MarshalAs(UnmanagedType.I4)]
//    public int smmttid;
//    [MarshalAs(UnmanagedType.I4)]
//    public int ID;
//    [MarshalAs(UnmanagedType.I4)]
//    public int hullNum;

//    [MarshalAs(UnmanagedType.R8)]
//    public double lat;
//    [MarshalAs(UnmanagedType.R8)]
//    public double lon;
//    [MarshalAs(UnmanagedType.R8)]
//    public double depth;

//    [MarshalAs(UnmanagedType.R8)]
//    public double heading;
//    [MarshalAs(UnmanagedType.R8)]
//    public double speed;

//    [MarshalAs(UnmanagedType.I4)]
//    public int state;
//};

public struct VIBECommandMessage
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string command;
};

public struct NSIMessageInfo
{
    [MarshalAs(UnmanagedType.I4)]
    int messageNumber;
    [MarshalAs(UnmanagedType.R8)]
    double timeReceived;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    string source;
};

[StructLayout(LayoutKind.Sequential)]
public struct NSIAgentReturnStruct
{
    [MarshalAs(UnmanagedType.I4)]
    public int totalNumberOfMessages;

    [MarshalAs(UnmanagedType.I4)]
    public int numberOfVIBECommandMessages;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public VIBECommandMessage[] VIBECommandMessages;

    [MarshalAs(UnmanagedType.I4)]
    public int numberOfVIBEControllerMessages;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public VIBETranslatedControllerMessage[] VIBEControllerMessages;

    [MarshalAs(UnmanagedType.I4)]
    public int numberOfXSIMStatusMessages;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public XSIMStatusMessage[] XSIMStatusMessages;

    [MarshalAs(UnmanagedType.I4)]
    public int numberOfXSIMUpdateMessages;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public XSIMUpdateMessage[] XSIMUpdateMessages;

    [MarshalAs(UnmanagedType.I4)]
    public int numberOfGlobalUpdateMessages;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public Global[] GlobalUpdateMessages;

    [MarshalAs(UnmanagedType.I4)]
    public int numberOfEnvUpdateMessages;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public Global_Env[] EnvUpdateMessages;

    [MarshalAs(UnmanagedType.I4)]
    public int masterStatus;

    [MarshalAs(UnmanagedType.I4)]
    public int initScene;

    [MarshalAs(UnmanagedType.I4)]
    public int nameChanged;

    public NSIAgentReturnStruct(int a_Num = 0)
    {
        totalNumberOfMessages = a_Num;

        numberOfVIBECommandMessages = a_Num;
        VIBECommandMessages = new VIBECommandMessage[1024];

        numberOfVIBEControllerMessages = a_Num;
        VIBEControllerMessages = new VIBETranslatedControllerMessage[1024];

        numberOfXSIMStatusMessages = a_Num;
        XSIMStatusMessages = new XSIMStatusMessage[1024];

        numberOfXSIMUpdateMessages = a_Num;
        XSIMUpdateMessages = new XSIMUpdateMessage[1024];

        numberOfGlobalUpdateMessages = a_Num;
        GlobalUpdateMessages = new Global[5];

        numberOfEnvUpdateMessages = a_Num;
        EnvUpdateMessages = new Global_Env[5];

        masterStatus = 0;
        initScene = 0;
        nameChanged = 0;
    }
};

[StructLayout(LayoutKind.Sequential)]
public struct AnnounceStruct
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string SourceName;
};

//[StructLayout(LayoutKind.Sequential)]
//public struct NSIAgentReturnStruct
//{
//    [MarshalAs(UnmanagedType.I4)]
//    public int totalNumberOfMessages;

//    [MarshalAs(UnmanagedType.I4)]
//    public int numberOfVIBECommandMessages;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
//    public VIBECommandMessage[] VIBECommandMessages;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
//    public NSIMessageInfo[] VIBECommandMessageInfo;

//    [MarshalAs(UnmanagedType.I4)]
//    public int numberOfVIBEControllerMessages;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
//    public VIBETranslatedControllerMessage[] VIBEControllerMessages;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
//    public NSIMessageInfo[] VIBEControllerMessageInfo;

//    [MarshalAs(UnmanagedType.I4)]
//    public int numberOfXSIMStatusMessages;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
//    public XSIMStatusMessage[] XSIMStatusMessages;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
//    public NSIMessageInfo[] XSIMStatusMessageInfo;

//    [MarshalAs(UnmanagedType.I4)]
//    public int numberOfXSIMUpdateMessages;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
//    public XSIMUpdateMessage[] XSIMUpdateMessages;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
//    public NSIMessageInfo[] XSIMUpdateMessageInfo;

//    public NSIAgentReturnStruct(int a_Num = 0)
//    {
//        totalNumberOfMessages = a_Num;

//        numberOfVIBECommandMessages = a_Num;
//        VIBECommandMessages = new VIBECommandMessage[1024];
//        VIBECommandMessageInfo = new NSIMessageInfo[1024];

//        numberOfVIBEControllerMessages = a_Num;
//        VIBEControllerMessages = new VIBETranslatedControllerMessage[1024];
//        VIBEControllerMessageInfo = new NSIMessageInfo[1024];

//        numberOfXSIMStatusMessages = a_Num;
//        XSIMStatusMessages = new XSIMStatusMessage[1024];
//        XSIMStatusMessageInfo = new NSIMessageInfo[1024];

//        numberOfXSIMUpdateMessages = a_Num;
//        XSIMUpdateMessages = new XSIMUpdateMessage[1024];
//        XSIMUpdateMessageInfo = new NSIMessageInfo[1024];
//    }
//};

[StructLayout(LayoutKind.Sequential)]
public struct contactUpdate
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public char[] source;	// who is the sender "MWSE", "VIBE", "ETC"
    [MarshalAs(UnmanagedType.AsAny)]
    public char statusFlag;	// indicates Remove(0xFF), Alive(0x00), etc
    [MarshalAs(UnmanagedType.R8)]
    public float timeReceived;

    [MarshalAs(UnmanagedType.I4)]
    public int smmttid;         	// Identifies the Entity type of Ship, Plane, Buoy, ETC.
    [MarshalAs(UnmanagedType.I4)]
    public int ID;			// Unique ID from sender
    [MarshalAs(UnmanagedType.I4)]
    public int seciID;			// Unique ID assigned by VIBE
    [MarshalAs(UnmanagedType.I4)]
    public int targetAscID;		// An entity attached to this object that can be released, (Towed Array, Missile Launcher etc.)
    [MarshalAs(UnmanagedType.I4)]
    public int hullNum;			// Future use 
    [MarshalAs(UnmanagedType.I4)]
    public int autopilot;		// 0 = local controlled; 1 = network controlled

    [MarshalAs(UnmanagedType.I4)]
    public int useExternalPhysics;

    // This is the minimum data that should be provided
    [MarshalAs(UnmanagedType.R8)]
    public float lat;
    [MarshalAs(UnmanagedType.R8)]
    public float lon;
    [MarshalAs(UnmanagedType.R8)]
    public float heading;			// ships set heading... Where it is pointing
    [MarshalAs(UnmanagedType.R8)]
    public float altitude;		// Pos (above sea level); Neg Below sea level

    // Optional Pitch & Roll should be provided if external Phy model is used
    [MarshalAs(UnmanagedType.R8)]
    public float pitch;
    [MarshalAs(UnmanagedType.R8)]
    public float roll;

    // Optional These are optional inputs. (entities may use VIBE Phy interaction)
    [MarshalAs(UnmanagedType.R8)]
    public float course;			// Direction ship wants to travel... not used by everyone
    [MarshalAs(UnmanagedType.R8)]
    public float speed;

    [MarshalAs(UnmanagedType.I4)]
    public int state;          		// Enum; See "EntityState" below 

    // Optional for systems that have the capability
    [MarshalAs(UnmanagedType.I4)]
    public int EngineSmokeOn;
    [MarshalAs(UnmanagedType.I4)]
    public int FlamesPresent;
    [MarshalAs(UnmanagedType.I4)]
    public int SmokePlumePresent;
    [MarshalAs(UnmanagedType.I4)]
    public int RunningLights;

    // Optional used for external control of entity's view point
    [MarshalAs(UnmanagedType.R8)]
    public double viewHeight;
    [MarshalAs(UnmanagedType.R8)]
    public double viewHeading;
    [MarshalAs(UnmanagedType.R8)]
    public double viewPitch;
    [MarshalAs(UnmanagedType.R8)]
    public double viewRoll;

};

public struct VIBEHeartbeat
{
    [MarshalAs(UnmanagedType.I4)]
    public int numberOfContacts;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
    public contactUpdate[] contacts;

    [MarshalAs(UnmanagedType.I4)]
    public int viewAttachedID;
};

public enum UpdateType
{
    POSITION = 0,
    ROTATION,
    STATE,
    SYSTEMSTATE,
    SPEED,
    TURNSPEED
};

public enum EntityState
{
    DEFAULT_POSUPDATE = 0,
    EFFECTOR_LAUNCHING,
    EFFECTOR_COMPLETE,
    OBJECT_DISABLED,
    SYSTEM_TURNED_ON,
    MINE_ACTUATED,
    DETECTION,
    N81DEMOAQUAD_ARRIVED_AT_SHIP,
    EFFECTOR_MISSED,
    MINE_DETECTED,
    MINE_CLASSIFIED,
    MINE_REACQUIRED,
    MINE_REACQUIRED_NEUTRALIZE,
    MINE_IDENTIFIED,
    MINE_IDENTIFIED_NONMINE,
    MINE_NEUTRALIZED,
    SENSORS_ACTIVATED,
    SENSORS_DEACTIVATED,
    SYSTEM_LAUNCHED,
    SYSTEM_COMPLETE,
    SWITCH_LOADOUT_1,
    SWITCH_LOADOUT_2,
    SWITCH_LOADOUT_3,
    SPECIAL_ACTION_1,
    SPECIAL_ACTION_2,
    STARTED,
    STOPPED,
    PAUSED,
    TURNING,
    MOVE_TO_THREAT,
    RETURN_TO_HOST,
    WAYPOINTS_START,
    WAYPOINTS_STOP,
    CLASSIFY,
    REACQUIRE,
    REACQUIRE_NEUTRALIZE,
    IDENTIFY,
    NEUTRALIZE,
    SINGLERUN,
    REPEAT,
    LAUNCH,
    RECOVER,
    LOAD,
    UNLOAD,
    LAND,
    ATTACH,
    DETTACH,
    WAIT_UNTIL_TARGET_IS_MOVING_AWAY,
    REACQUIRE_SURF_MILCOS,
    REACQUIRE_NEARSURFACE_MILCOS,
    REACQUIRE_VOLUME_MILCOS,
    REACQUIRE_BOTTOM_MILCOS,
    REACQUIRE_BURIED_MILCOS,
    IDENTIFY_SURF_MILCOS,
    IDENTIFY_NEARSURFACE_MILCOS,
    IDENTIFY_VOLUME_MILCOS,
    IDENTIFY_BOTTOM_MILCOS,
    IDENTIFY_BURIED_MILCOS,
    REACQUIRE_SURF_NEUTRALIZE,
    REACQUIRE_NEARSURFACE_NEUTRALIZE,
    REACQUIRE_VOLUME_NEUTRALIZE,
    REACQUIRE_BOTTOM_NEUTRALIZE,
    REACQUIRE_BURIED_NEUTRALIZE,
    NEUTRALIZE_NEARSURFACE,
    NEUTRALIZE_VOLUME,
    NEUTRALIZE_BOTTOM,
    PASS_WAYPOINTS,
    DROP_NEARSURFACEMINE,
    DROP_SURFACEMINE,
    DROP_VOLUMEMINE,
    DROP_BOTTOMMINE,
    START_SURFACE_MINELINE,
    START_BOTTOM_MINELINE,
    STOP_MINELINE,
    DEPTHMODE_ALT,
    DEPTHMODE_DEPTH,
    SURFACE,
    DIVE,
    UNKNOWN
}

public struct PrecisionGPS
{
    public double lat;
    public double lon;
    public double alt;

    public PrecisionGPS(double a_Lat, double a_Lon)
    {
        lat = a_Lat;
        lon = a_Lon;
        alt = 0;
    }

    public PrecisionGPS(double a_Lat, double a_Lon, double a_Alt)
    {
        lat = a_Lat;
        lon = a_Lon;
        alt = a_Alt;
    }
};

public class SimUpdate
{
    public UpdateType Type;
    public Vector3 Position = Vector3.zero; // LAT LON ELE
    public Vector3 Rotation = Vector3.zero;
    public PrecisionGPS GPS; // LAT LON ELE
    public EntityState State = EntityState.DEFAULT_POSUPDATE;
    public float Value = -1;
}


[StructLayout(LayoutKind.Sequential)]
public struct Global
{
    [MarshalAs(UnmanagedType.I4)]
    public int month;
    [MarshalAs(UnmanagedType.I4)]
    public int day;
    [MarshalAs(UnmanagedType.I4)]
    public int year;
    [MarshalAs(UnmanagedType.R8)]
    public double time; // Once I figure out time format this needs to be changed
    [MarshalAs(UnmanagedType.I4)]
    public int state;
};

[StructLayout(LayoutKind.Sequential)]
public struct Global_Env
{
    [MarshalAs(UnmanagedType.I4)]
    public int cloudDensity;
    [MarshalAs(UnmanagedType.I4)]
    public int cloudCeiling;
    [MarshalAs(UnmanagedType.I4)]
    public int visibilityRange;
    [MarshalAs(UnmanagedType.I4)]
    public int seaState;
    [MarshalAs(UnmanagedType.I4)]
    public int precipType;
    [MarshalAs(UnmanagedType.I4)]
    public int precipIntensity;
    [MarshalAs(UnmanagedType.R8)]
    public double windHeading;
    [MarshalAs(UnmanagedType.R8)]
    public double windSpeed;
};

[StructLayout(LayoutKind.Sequential)]
public struct contactsStruct
{
    [MarshalAs(UnmanagedType.I4)]
    public int numberOfContacts;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public contactUpdate[] contacts;
};

[StructLayout(LayoutKind.Sequential)]
public struct SECIUpdate
{
    public Global Global;
    public Global_Env Environment;
    public contactsStruct Contacts;
    public int Init;
};

