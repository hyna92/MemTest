using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DefineLib
{
    public class StaticDefine
    {

        #region 0. Color
        public static SolidColorBrush RGB_MSG_TEST = Brushes.Yellow;
        public static SolidColorBrush RGB_MSG_PASS = Brushes.Lime;
        public static SolidColorBrush RGB_MSG_FAIL = Brushes.Red;
        #endregion

        
        #region 0. Network
        public const int NET_FAIL = -1;
        public const int NET_SUCCESS = 0;


        #endregion


        #region 0. Command List
        public const int COMMAND_POWER_ON = 11;
        public const int COMMAND_POWER_OFF = 12;
        //public const int COMMAND_POWER_RESET = 13;

        public const int COMMAND_VOLTAGE_SET = 21;
        public const int COMMAND_VOLTAGE_GET = 22;

        public const int COMMAND_TEST_RUN = 31;
        public const int COMMAND_TEST_STOP = 32;
        public const int COMMAND_TEST_PAUSE = 33;

        public const int COMMAND_KVM_SELECT = 41;

        public const int COMMAND_SERIAL_OPEN = 51;
        public const int COMMAND_SERIAL_CLOSE = 52;
        #endregion


        #region 0. Packet : Category
        public const int MSG_CATEGORY_SYSTEM = 0;
        public const int MSG_CATEGORY_VOLTAGE = 1;
        public const int MSG_CATEGORY_TEST = 2;
        #endregion


        #region 0. Packet : Action
        public const int MSG_ACTION_SET = 0;
        public const int MSG_ACTION_GET = 1;
        #endregion


        #region 0. Packet : Content "System"
        public const int MSG_CONTENT_KEY = 0;
        public const int MSG_CONTENT_VERSION = 1;
        public const int MSG_CONTENT_CPU = 2;
        public const int MSG_CONTENT_MEMORY = 3;
        #endregion


        #region 0. Packet : Content "Voltage" 
        public const int MSG_CONTENT_CPU0_VDD = 00;
        public const int MSG_CONTENT_CPU0_VDDQ = 01;
        public const int MSG_CONTENT_CPU0_VDDQ2 = 02;
        public const int MSG_CONTENT_CPU1_VDD = 10;
        public const int MSG_CONTENT_CPU1_VDDQ = 11;
        public const int MSG_CONTENT_CPU1_VDDQ2 = 12;
        #endregion


        #region 0. Packet : Content "Test" 
        public const int MSG_CONTENT_ITEM_COUNT = 0;
        public const int MSG_CONTENT_ITEM_NAME = 1;
        public const int MSG_CONTENT_STATUS = 2;
        public const int MSG_CONTENT_PROGRESS = 3;
        public const int MSG_CONTENT_ERROR_COUNT = 4;
        public const int MSG_CONTENT_ERROR_DATA = 5;
        #endregion


        #region 0. Packet : Option 
        public const int MSG_OPTION_NULL = 0;
        #endregion


        #region 0. Packet : Data 
        //public const string MSG_PASS = "0";
        public const string MSG_FAIL = "INVALID";
        #endregion


        #region 0. Packet : Data "Status"
        public const int BOARD_STATUS_RUN_READY = 0;
        public const int BOARD_STATUS_RUN = 1;
        public const int BOARD_STATUS_FINISH_READY = 2;
        public const int BOARD_STATUS_FINISH = 3;
        public const int BOARD_STATUS_PAUSE = 4;
        #endregion


        #region 0. Test State (GUI Setting)
        public const int TEST_STATE_NULL = 0;
        public const int TEST_STATE_PASS = 1;   // PASS (Complete)
        public const int TEST_STATE_FAIL = 2;   // STOP & FAIL (Complete) 

        public const int TEST_STATE_TESTSTATE_START = 10;
       // public const int TEST_STATE_TEST_READY = 11;
        public const int TEST_STATE_TEST_ING = 11;
        public const int TEST_STATE_TEST_PAUSE = 12;
        public const int TEST_STATE_TESTSTATE_END = 19;
        #endregion


        #region 0. Serial Command 
        public const byte SERIAL_CMD_PW_ON = 0x81;
        public const byte SERIAL_CMD_PW_OFF = 0x82;
        public const byte SERIAL_CMD_PW_RESET = 0x83;
        public const byte SERIAL_CMD_PW_FORCE_OFF = 0x84;
        public const byte SERIAL_CMD_BD_SEL = 0x85;
        public const byte SERIAL_CMD_PW_STATE = 0x8A;
        #endregion


        #region 0. Serial Return State
        public const byte SERIAL_RET_RECV_OK = 0xFA;
        public const byte SERIAL_RET_CHKSUM_ERR = 0xFE;
        public const byte SERIAL_RET_CMD_END = 0x0D;
        #endregion


        #region 0. Path
        public const string PATH_LOG = ".\\Log";
        public const string PATH_RESULT = ".\\Result";
        #endregion

    }
}
