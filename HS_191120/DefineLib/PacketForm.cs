using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefineLib
{
    public class DataPacket
    {
        private int iCategory;
        private int iAction;
        private int iContent;
        private int iOption;
        private string strData;

        public int MSGCategory
        {
            get { return iCategory; }
            private set { iCategory = value; }
        }

        public int MSGAction
        {
            get { return iAction; }
            private set { iAction = value; }
        }

        public int MSGContent
        {
            get { return iContent; }
            private set { iContent = value; }
        }

        public int MSGOption
        {
            get { return iOption; }
            private set { iOption = value; }
        }

        public string MSGData
        {
            get { return strData; }
            private set { strData = value; }
        }

        // Send Packet
        public DataPacket(int Category, int Action, int Content, int Option)
        {
            MSGCategory = Category;
            MSGAction = Action;
            MSGContent = Content;
            MSGOption = Option;
        }

        // Receive Packet
        public DataPacket(int Category, int Action, int Content, int Option, string Data)
        {
            MSGCategory = Category;
            MSGAction = Action;
            MSGContent = Content;
            MSGOption = Option;
            MSGData = Data;
        }
    }

}
