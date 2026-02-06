using System;

namespace Runtime.Domain
{
    public static class SaveIds
    {
        public const string Machine1 = "machine_1";
        public const string Machine2 = "machine_2";
        public const string Machine3 = "machine_3";
        public const string Machine4 = "machine_4";
        public const string Machine5 = "machine_5";
        public const string Computer1 = "computer_1";
        public const string DoorSalaOrdenador = "door_sala_ordenador";
        public const string DoorSalaCarga = "door_sala_carga";
        public const string RoomSalaOrdenador = "room_sala_ordenador";
        public const string RoomSalaCarga = "room_sala_carga";
    }

    [Serializable]
    public class GameSaveData
    {
        public int version = 1;
        public float cameraX;
        public float cameraY;
        public float cameraZ;
        public float cameraSize;
        public string[] saveIds;
        public string[] saveJsons;
    }

    [Serializable]
    public class MoñecoSaveData
    {
        public float x;
        public float y;
        public int direction;
        public bool isInteracting;
        public string assignedMachineId;
    }

    [Serializable]
    public class MachineSaveData
    {
        public string id;
        public int currentPresses;
        public bool hasWorker;
        public bool isEnabled;
    }

    [Serializable]
    public class ComputerSaveData
    {
        public string id;
        public int currentPresses;
        public bool repaired;
    }

    [Serializable]
    public class DoorSaveData
    {
        public string id;
        public bool isOpen;
    }

    [Serializable]
    public class RoomSaveData
    {
        public string id;
        public bool discovered;
    }

    [Serializable]
    public class BagSaveData
    {
        public int total;
        public int inside;
    }
}
