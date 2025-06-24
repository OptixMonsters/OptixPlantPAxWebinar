#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.SQLiteStore;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.Store;
using FTOptix.OPCUAServer;
using FTOptix.RAEtherNetIP;
using FTOptix.NetLogic;
using FTOptix.Retentivity;
using FTOptix.CommunicationDriver;
using FTOptix.AuditSigning;
using FTOptix.EventLogger;
using FTOptix.System;
using FTOptix.UI;
using FTOptix.Core;
using FTOptix.Alarm;
using FTOptix.SerialPort;
#endregion

public class raP_5_20_PINTLK_NL_HMITypesLoadBanks : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        this.LoadBanks();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }
    private void LoadBanks()
    {
        Int32 valBankMap = 0;
        const string BANK_MAP_TAG_NAME = "_Val_BankMap";  // Bank map tag name used in this container
        const string BANK_ID_TAG_NAME = "Set_BankId";  // Bank Id tag name used when creating banks

        const string LOGID = "Interlock HMI: Load Banks"; // ID used for the error logs
        const string LIST_WIDGET_NAME = "raP_5_20_PINTLK_AdvHMITypeList";  // Name of the list widget used when there are no banks
        const string BANK_WIDGET_NAME = "raP_5_20_PINTLK_AdvHMITypeListBank";  // Name of bank widget

        // Read Val_BankMap tag to determine which banks (if any) are in use 
        try
        {
            valBankMap = Owner.GetVariable(BANK_MAP_TAG_NAME).Value;
        }
        catch
        {
            Log.Error(LOGID, "Error reading Bank Map tag " + BANK_MAP_TAG_NAME);
        }



        if (valBankMap == 0)
        {
            // when the value is zero there are no banks, just a single interlock.  We just need to add the List widget
            try
            {
                IUANode MyWidget = Project.Current.Find(LIST_WIDGET_NAME);  // search for the widget in the model
                IUAObject newInstance = InformationModel.MakeObject(LIST_WIDGET_NAME, MyWidget.NodeId);  // create a new instance
                Owner.Add(newInstance);  // add the instance to our container
            }
            catch
            {
                Log.Error(LOGID, "Error adding List Widget " + LIST_WIDGET_NAME);
            }

        }
        else
        {
            // For each bank in use, create a bank list instance and add it to the vertical Container
            try
            {
                // Get the Engineering Bank widget node
                IUANode MyWidget = Project.Current.Find(BANK_WIDGET_NAME);

                for (int i = 0; i < 8; i++)
                {
                    if ((valBankMap & (1 << i)) != 0)  // if the bit is set, the bank is in use
                    {
                        try
                        {
                            IUAObject newInstance = InformationModel.MakeObject(BANK_WIDGET_NAME + i, MyWidget.NodeId); // create a new instance
                            newInstance.GetVariable(BANK_ID_TAG_NAME).Value = i;  // set the bank tag value
                            Owner.Add(newInstance); // add the instance to our container
                        }
                        catch
                        {
                            Log.Error(LOGID, "Error adding Bank Widget " + BANK_WIDGET_NAME + i);
                        }
                    }
                }
            }
            catch
            {
                Log.Error(LOGID, "Error finding widget " + BANK_WIDGET_NAME);
            }

        }
    }
}
