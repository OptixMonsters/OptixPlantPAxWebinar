#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.RAEtherNetIP;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.OPCUAServer;
using FTOptix.Retentivity;
using FTOptix.CommunicationDriver;
using FTOptix.NetLogic;
using FTOptix.AuditSigning;
using FTOptix.EventLogger;
using FTOptix.Store;
using FTOptix.Core;
using System.Collections.Generic;
using System.Linq;
using FTOptix.Alarm;
using System.Runtime.CompilerServices;
using FTOptix.SQLiteStore;
using FTOptix.System;
using FTOptix.SerialPort;
#endregion

public class raP_5_20_PINTLK_NL : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        CreateBanks();

    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }



    public void CreateBanks()
    {
        string BasePath = "";
        IUAObject launchObj = null;
        int valBankMap = 0;

        try
        {

            // Step 1: Get the Alias Node to determine the Launch Object
            var aliasNode = Owner.GetAlias("raSDK1_DialogBox");
            launchObj = InformationModel.GetObject(aliasNode.NodeId);

            // Step 2: Get the Ref_Tag from the Alias
            var refTag = InformationModel.Get(launchObj.GetVariable("Ref_Tag").Value);
            string topContainer = "";
            string refTagBrowsePath = GetOptixPathByNode (refTag, topContainer);
            Log.Info ( "Ref_Tag browse path:  " + refTagBrowsePath);

            // Step 3:  Do some string manipulation to get the content after the "CommDrivers"
            //          The Interlock tag name should end with "_0" for the first bank.  
            //          We will strip off the "0", for the base path
            string tag_0_BrowsePath = "CommDrivers" + refTagBrowsePath.Split("CommDrivers")[1];
            BasePath = tag_0_BrowsePath.Substring(0, tag_0_BrowsePath.Length - 1);

            // Step 4:  Create the Intlk_0 object and read the bank map
            valBankMap = this.AddToLaunchObect(launchObj, BasePath, 0);

            // Step 5:  Add the rest of the banks
            for (int i = 1; i < 8; i++)
            {
                if ( (valBankMap & (1 << i)) != 0) {
                   this.AddToLaunchObect(launchObj, BasePath, i);
                }
            }
 
            //  We need to force a reload of the panel, since the panel may have rendered before we added the bank reference tags
            var navPanel = Owner.Find("NavigationPanel");
            var navPanelUI = (NavigationPanel)navPanel;
            //Log.Info(navPanel.BrowseName);
            navPanelUI.ChangePanelByTabName("Home");

            //navPanel = Owner.Find("np_Advanced");
            //navPanelUI = (NavigationPanel)navPanel;
            ////Log.Info(navPanel.BrowseName);
            //navPanelUI.ChangePanelByTabName("Engineering");


        }
        catch
        {
            if (launchObj == null)
            {
                Log.Error("Interlock Dialog Box startup", "Error getting Alias Node Id" );
            }
            else if (BasePath == "")
            {
                Log.Error("Interlock Dialog Box startup", "CommDrivers folder not found" );
            }
            else if (valBankMap == 0)
            {
                Log.Error("Interlock Dialog Box startup", "Error reading Val_BankMap" );
            }
            else 
            {
                Log.Error("Interlock Dialog Box startup", "Unknown error occured" );
            }
        }

    }

    // This routine creates a variable and connects it to a tag named {basePath} + {idx}
    public int AddToLaunchObect(IUAObject launchObj, string basePath, int idx)
    {
        int valBankMap = 0;

        IUANode logixBankTag = Project.Current.Get( basePath + (string)idx.ToString());
        // Make new variable for each bank
        IUAVariable newBankVar = InformationModel.MakeVariable("Ref_Tag" + '_' + (string)idx.ToString(), OpcUa.DataTypes.NodeId);
        // Assign value of Logix Tag NodeId to new variable 
        newBankVar.Value = logixBankTag.NodeId;
        // Add new variable into the launch object
        launchObj.Add(newBankVar);

        // Return the bank map for the first instance only
        if ( idx==0)
        {
            valBankMap = (int)logixBankTag.Children.GetVariable("Val_BankMap").RemoteRead().Value;
        }

        return valBankMap;
    }

    public string GetOptixPathByNode(IUANode inputNode, string topContainer)
    {
        List<string> pathToVar = new List<string>();
        FindBrowsePath(inputNode);
        if (pathToVar.Count > 0)
        {
            var launchAliasPath = ConstructBrowsePath();
            return launchAliasPath;
        }
        else
        {
            return null;
        }

        string ConstructBrowsePath()
        {
            string outStr = topContainer;
            for (long i = (pathToVar.LongCount() - 1); i >= 0; i--)
            {
                outStr = outStr + "/" + pathToVar[(int)i];
            }
            pathToVar = new List<string>();
            return outStr;
        }
 
        void FindBrowsePath(IUANode inputNode)
        {
            if (inputNode.Owner != null)
            {
                if (inputNode.BrowseName == topContainer)
                {
                    return;
                }
                pathToVar.Add(inputNode.BrowseName);
                FindBrowsePath(inputNode.Owner);
            }
        }
    }

}
