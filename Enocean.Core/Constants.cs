namespace Enocean.Core.Constants
{
    public enum PACKET
    {
        RESERVED = 0x00,
        // RADIO == RADIO_ERP1
        // Kept for backwards compatibility reasons, for example custom packet
        // generation shouldn't be affected...
        RADIO = 0x01,
        RADIO_ERP1 = 0x01,
        RESPONSE = 0x02,
        RADIO_SUB_TEL = 0x03,
        EVENT = 0x04,
        COMMON_COMMAND = 0x05,
        SMART_ACK_COMMAND = 0x06,
        REMOTE_MAN_COMMAND = 0x07,
        RADIO_MESSAGE = 0x09,
        // RADIO_ADVANCED == RADIO_ERP2
        // Kept for backwards compatibility reasons
        RADIO_ADVANCED = 0x0A,
        RADIO_ERP2 = 0x0A,
        RADIO_802_15_4 = 0x10,
        COMMAND_2_4 = 0x11
    }
    public enum RETURN_CODE
    {
        OK = 0x00,
        ERROR = 0x01,
        NOT_SUPPORTED = 0x02,
        WRONG_PARAM = 0x03,
        OPERATION_DENIED = 0x04
    }

    public enum EVENT_CODE
    {
        SA_RECLAIM_NOT_SUCCESFUL = 0x01,
        SA_CONFIRM_LEARN = 0x02,
        SA_LEARN_ACK = 0x03,
        CO_READY = 0x04,
        CO_EVENT_SECUREDEVICES = 0x05
    }
    public enum RORG
    {
        UNDEFINED = 0x00,
        RPS = 0xF6,
        BS1 = 0xD5,
        BS4 = 0xA5,
        VLD = 0xD2,
        MSC = 0xD1,
        ADT = 0xA6,
        SM_LRN_REQ = 0xC6,
        SM_LRN_ANS = 0xC7,
        SM_REC = 0xA7,
        SYS_EX = 0xC5,
        SEC = 0x30,
        SEC_ENCAPS = 0x31,
        UTE = 0xD4
    }
    public enum PARSE_RESULT
    {
        OK = 0x00,
        INCOMPLETE = 0x01,
        CRC_MISMATCH = 0x03
    }

    public static class DB0
    {
        public const int BIT_0 = 1;
        public const int BIT_1 = 2;
        public const int BIT_2 = 3;
        public const int BIT_3 = 4;
        public const int BIT_4 = 5;
        public const int BIT_5 = 6;
        public const int BIT_6 = 7;
        public const int BIT_7 = 8;
    }

    public static class DB1
    {
        public const int BIT_0 = 9;
        public const int BIT_1 = 10;
        public const int BIT_2 = 11;
        public const int BIT_3 = 12;
        public const int BIT_4 = 13;
        public const int BIT_5 = 14;
        public const int BIT_6 = 15;
        public const int BIT_7 = 16;
    }

    public static class DB2
    {
        public const int BIT_0 = 17;
        public const int BIT_1 = 18;
        public const int BIT_2 = 19;
        public const int BIT_3 = 20;
        public const int BIT_4 = 21;
        public const int BIT_5 = 22;
        public const int BIT_6 = 23;
        public const int BIT_7 = 24;
    }

    public static class DB3
    {
        public const int BIT_0 = 25;
        public const int BIT_1 = 26;
        public const int BIT_2 = 27;
        public const int BIT_3 = 28;
        public const int BIT_4 = 29;
        public const int BIT_5 = 30;
        public const int BIT_6 = 31;
        public const int BIT_7 = 32;
    }

    public static class DB4
    {
        public const int BIT_0 = 33;
        public const int BIT_1 = 34;
        public const int BIT_2 = 35;
        public const int BIT_3 = 36;
        public const int BIT_4 = 37;
        public const int BIT_5 = 38;
        public const int BIT_6 = 39;
        public const int BIT_7 = 40;
    }

    public static class DB5
    {
        public const int BIT_0 = 41;
        public const int BIT_1 = 42;
        public const int BIT_2 = 43;
        public const int BIT_3 = 44;
        public const int BIT_4 = 45;
        public const int BIT_5 = 46;
        public const int BIT_6 = 47;
        public const int BIT_7 = 48;
    }

    public static class DB6
    {
        public const int BIT_0 = 49;
        public const int BIT_1 = 50;
        public const int BIT_2 = 51;
        public const int BIT_3 = 52;
        public const int BIT_4 = 53;
        public const int BIT_5 = 54;
        public const int BIT_6 = 55;
        public const int BIT_7 = 56;
    }
}
