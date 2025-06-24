#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.CoreBase;
using FTOptix.System;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.OPCUAServer;
using FTOptix.RAEtherNetIP;
using FTOptix.NetLogic;
using FTOptix.Retentivity;
using FTOptix.CommunicationDriver;
using FTOptix.AuditSigning;
using FTOptix.EventLogger;
using FTOptix.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using FTOptix.Alarm;
using FTOptix.SerialPort;
#endregion

public class AuditSigningConfiguration : BaseNetLogic
{
    private LongRunningTask myLongRunningTask;
    private const string CommDriversCategoryFolderType = "CommDriversCategoryFolder";
    private const string RAEtherNetIPDriverType = "RAEtherNetIPDriver";
    private const string RAEtherNetIPStationType = "RAEtherNetIPStation";
    [ExportMethod]
    public void CreateAuditSigning()
    {
        myLongRunningTask = new LongRunningTask(CreateAuditSigningTask, LogicObject);
        myLongRunningTask.Start();
        UAManagedCore.Log.Info("AuditSigning Configuration", "Importing audit configuration... Please wait...");

    }
    public void CreateAuditSigningTask(LongRunningTask task)
    {
        var eSignatureTemplate = new List<EsigTemplate>
        {
            new EsigTemplate { DataType = "P_ANALOG_OUTPUT", TagMember = "MCmd_Bypass", EsigWorkflowType = "Confirm", Caption = "Bypass interlocks for {@Description}" },
            new EsigTemplate { DataType = "P_DISCRETE_4STATE", TagMember = "MCmd_Bypass", EsigWorkflowType = "Confirm", Caption = "Bypass interlocks and permissives for {@Description}" },
            new EsigTemplate { DataType = "P_DISCRETE_OUTPUT", TagMember = "MCmd_Bypass", EsigWorkflowType = "Confirm", Caption = "Bypass interlocks and permissives for {@Description}" },
            new EsigTemplate { DataType = "P_MOTOR_DISCRETE", TagMember = "MCmd_Bypass", EsigWorkflowType = "Confirm", Caption = "Bypass interlocks and permissives for {@Description}" },
            new EsigTemplate { DataType = "P_PID", TagMember = "MCmd_Bypass", EsigWorkflowType = "Confirm", Caption = "Bypass interlocks for {@Description}" },
            new EsigTemplate { DataType = "P_VALVE_DISCRETE", TagMember = "MCmd_Bypass", EsigWorkflowType = "Confirm", Caption = "Bypass interlocks and permissives for {@Description}" },
            new EsigTemplate { DataType = "P_VARIABLE_SPEED_DRIVE", TagMember = "MCmd_Bypass", EsigWorkflowType = "Confirm", Caption = "Bypass interlocks and permissives for {@Description}" },
            new EsigTemplate { DataType = "P_ANALOG_OUTPUT", TagMember = "MCmd_OoS", EsigWorkflowType = "Confirm", Caption = "Take {@Description} Out of Service" },
            new EsigTemplate { DataType = "P_DEADBAND", TagMember = "MCmd_OoS", EsigWorkflowType = "Confirm", Caption = "Take {@Description} Out of Service" },
            new EsigTemplate { DataType = "P_DISCRETE_4STATE", TagMember = "MCmd_OoS", EsigWorkflowType = "Confirm", Caption = "Take {@Description} Out of Service" },
            new EsigTemplate { DataType = "P_DISCRETE_OUTPUT", TagMember = "MCmd_OoS", EsigWorkflowType = "Confirm", Caption = "Take {@Description} Out of Service" },
            new EsigTemplate { DataType = "P_DOSING", TagMember = "MCmd_OoS", EsigWorkflowType = "Confirm", Caption = "Take {@Description} Out of Service" },
            new EsigTemplate { DataType = "P_MOTOR_DISCRETE", TagMember = "MCmd_OoS", EsigWorkflowType = "Confirm", Caption = "Take {@Description} Out of Service" },
            new EsigTemplate { DataType = "P_PID", TagMember = "MCmd_OoS", EsigWorkflowType = "Confirm", Caption = "Take {@Description} Out of Service" },
            new EsigTemplate { DataType = "P_VALVE_DISCRETE", TagMember = "MCmd_OoS", EsigWorkflowType = "Confirm", Caption = "Take {@Description} Out of Service" },
            new EsigTemplate { DataType = "P_VARIABLE_SPEED_DRIVE", TagMember = "MCmd_OoS", EsigWorkflowType = "Confirm", Caption = "Take {@Description} Out of Service" },
            new EsigTemplate { DataType = "P_ANALOG_INPUT", TagMember = "MCmd_SubstPV", EsigWorkflowType = "Confirm", Caption = "Replace the PV with a substitute value for {@Description}" },
            new EsigTemplate { DataType = "P_DISCRETE_INPUT", TagMember = "MCmd_SubstPV", EsigWorkflowType = "Confirm", Caption = "Replace the PV with a substitute value for {@Description}" },
            new EsigTemplate { DataType = "P_ANALOG_INPUT", TagMember = "MCmd_Virtual", EsigWorkflowType = "Confirm", Caption = "Place {@Description} in virtual operation" },
            new EsigTemplate { DataType = "P_ANALOG_OUTPUT", TagMember = "MCmd_Virtual", EsigWorkflowType = "Confirm", Caption = "Place {@Description} in virtual operation" },
            new EsigTemplate { DataType = "P_DISCRETE_4STATE", TagMember = "MCmd_Virtual", EsigWorkflowType = "Confirm", Caption = "Place {@Description} in virtual operation" },
            new EsigTemplate { DataType = "P_DISCRETE_INPUT", TagMember = "MCmd_Virtual", EsigWorkflowType = "Confirm", Caption = "Place {@Description} in virtual operation" },
            new EsigTemplate { DataType = "P_DISCRETE_OUTPUT", TagMember = "MCmd_Virtual", EsigWorkflowType = "Confirm", Caption = "Place {@Description} in virtual operation" },
            new EsigTemplate { DataType = "P_DOSING", TagMember = "MCmd_Virtual", EsigWorkflowType = "Confirm", Caption = "Place {@Description} in virtual operation" },
            new EsigTemplate { DataType = "P_MOTOR_DISCRETE", TagMember = "MCmd_Virtual", EsigWorkflowType = "Confirm", Caption = "Place {@Description} in virtual operation" },
            new EsigTemplate { DataType = "P_VALVE_DISCRETE", TagMember = "MCmd_Virtual", EsigWorkflowType = "Confirm", Caption = "Place {@Description} in virtual operation" },
            new EsigTemplate { DataType = "P_VARIABLE_SPEED_DRIVE", TagMember = "MCmd_Virtual", EsigWorkflowType = "Confirm", Caption = "Place {@Description} in virtual operation" }
        };
        var commDrivers = Project.Current.Get("CommDrivers");
        var dataTypeList = eSignatureTemplate.Select(Item => Item.DataType).Distinct().ToList();
        var tags = SearchTag(commDrivers, dataTypeList);
        bool isCreated = false;
        foreach (var row in eSignatureTemplate)
        {
            foreach (var Controlinfo in tags)  
            {
                IUANode EtherNetIPDriverNode = Project.Current.Find(Controlinfo.DriverName);
                var inputPath = GetTagPath(EtherNetIPDriverNode, "CommDrivers", Controlinfo.FolderName, Controlinfo.TagName, Controlinfo.StationName);
                var ControllerTag = Project.Current.Get(inputPath);
                if (ControllerTag != null)
                {
                    string dataTypeBrowserName = ((UAManagedCore.UANode)((UAManagedCore.UAVariable)ControllerTag).VariableType).BrowseName;
                    if (Regex.IsMatch(dataTypeBrowserName, $"^{row.DataType}\\d*$") && Regex.IsMatch(Controlinfo.DataType, $"^{row.DataType}\\d*$"))
                    {
                        foreach (var tagChildren in ControllerTag.Children)
                        {
                            string tagBrowser = tagChildren.BrowseName;
                            if (tagBrowser == row.TagMember && Regex.IsMatch(dataTypeBrowserName, $"^{row.DataType}\\d*$"))
                            {
                                RemovTagMemberAuditSigning(tagChildren);
                                AddAuditSignature(row, tagChildren, ControllerTag);
                                isCreated = true;
                            }
                        }
                    }
                }
            }   
        }
        if (isCreated) // Check the flag and print the log message 
        {
            UAManagedCore.Log.Info("AuditSigning Configuration", "AuditSigning configuration imported.");
        }
        else
        {
            UAManagedCore.Log.Error("AuditSigning Configuration", "No AuditSigning created.");
        }
    }
    public void AddAuditSignature(EsigTemplate row, IUANode tagChildren, IUANode controllerTag)
    {
        var audSig = InformationModel.MakeObject<FTOptix.AuditSigning.AuditInfo>("AuditSigning Signature");
        audSig.WorkflowType = WorkflowType.Confirm;
        tagChildren.Add(audSig);
        CreateIdentifiersForItems(row.Caption, "Caption", tagChildren, controllerTag);
    }
    public void CreateIdentifiersForItems(string items, string variableName, IUANode tagChildren, IUANode tag)
    {

        const string pattern = @"{(.*?)}";
        // Create a new variable and add it to the children of the tag
        IUAVariable confirmItem = InformationModel.MakeVariable(variableName, OpcUa.DataTypes.String);
        tagChildren.Find("AuditSigning Signature").Children.Add(confirmItem);
        // Find matches in the items string
        var matches = Regex.Matches(items, pattern);
        if (Regex.IsMatch(items, @".*\{.*\}.*"))
        {
            // Create a string formatter
            var stringFormatter = InformationModel.Make<StringFormatter>("StringFormatter1");
            for (int i = 0; i < matches.Count; i++)
            {
                String matchValue = matches[i].Groups[1].Value;
                UAManagedCore.UAVariable extendedTag = null;
                // Match a string that start with "@"
                if (matchValue.StartsWith("@"))
                {
                    variableName = matchValue.Substring(1);
                    extendedTag = (UAManagedCore.UAVariable)tag.Find(variableName);
                    var source = InformationModel.MakeVariable("Source" + i, OpcUa.DataTypes.BaseDataType);
                    stringFormatter.Refs.AddReference(FTOptix.CoreBase.ReferenceTypes.HasSource, source);
                    source.SetDynamicLink(extendedTag);
                    string patterns = "{" + matches[i].Groups[1].Value + "}";
                    string replacement = "{" + i.ToString() + "}";
                    items = items.Replace(patterns, replacement);
                }
            }
            stringFormatter.Format = items;
            confirmItem.SetConverter(stringFormatter);
        }
    }
    public class EsigTemplate
    {
        public string DataType { get; set; }
        public string TagMember { get; set; }
        public string EsigWorkflowType { get; set; }
        public string Caption { get; set; }
    }
    private List<ControlInfo> SearchTag(IUANode node, List<string> dataTypeList)
    {
        // Initialize a list to store the tags
        List<ControlInfo> tags = new List<ControlInfo>();
        string objectTypeBrowseName = string.Empty;
        // Determine the type of the node and get its BrowseName
        if (node is IUAVariable)
        {
            objectTypeBrowseName = ((IUAVariable)node).VariableType.BrowseName;
        }
        else
        {
            objectTypeBrowseName = ((UAObject)node).ObjectType.BrowseName;
        }
        // If the node type is CommDriversCategoryFolderType or RAEtherNetIPDriverType, recursively search its children
        if (objectTypeBrowseName == CommDriversCategoryFolderType || objectTypeBrowseName == RAEtherNetIPDriverType)
        {
            foreach (var item in node.Children)
            {
                tags.AddRange(SearchTag(item, dataTypeList));
            }
        }
        // If the node type is RAEtherNetIPStationType, search the "Tags" folder
        else if (objectTypeBrowseName == RAEtherNetIPStationType)
        {
            var folder = node.Find<Folder>("Tags");
            foreach (var item in folder.Children)
            {
                tags.AddRange(SearchTag(item, dataTypeList));
            }
        }
        // If the node is a Folder, search its non-Folder children
        else if (node is Folder)
        {
            foreach (var item in node.Children)
            {
                if (item is not Folder)
                {
                    tags.AddRange(SearchTag(item, dataTypeList));
                }
            }
        }
        // For other types of nodes, extract the control name and control folder name from the path
        else
        {
            // Check if the prefix is in the dataTypeList
            if (dataTypeList.Any(item => Regex.IsMatch(objectTypeBrowseName, $"^{item}\\d*$")))
            {
                var inputPath = FindPath(node).TrimEnd('/');
                string pattern = @".*/(.*?)/(.*?)/Tags/(.*?)/.*";
                Match match = Regex.Match(inputPath, pattern);
                if (match.Success)
                {
                    ControlInfo controlInfo = new ControlInfo
                    {
                        DataType = objectTypeBrowseName,
                        DriverName = match.Groups[1].Value,
                        FolderName = match.Groups[3].Value,
                        StationName = match.Groups[2].Value,
                        TagName = node.BrowseName
                    };
                    tags.Add(controlInfo);
                }
            }
        }
        return tags;
    }
    private string FindPath(IUANode node, string path = "")
    {
        if (node.BrowseName != "CommDrivers")
            path = FindPath(node.Owner, path);
        path += node.BrowseName + "/";
        return path;
    }
    public class ControlInfo
    {
        public string StationName { get; set; }
        public string FolderName { get; set; }
        public string DataType { get; set; }
        public string TagName { get; set; }
        public string DriverName { get; set; }
    }
    public static string GetTagPath(IUANode inputNode, string topContainer, string folderName, string tagName, string stationName)
    {
        List<string> pathToVar = new List<string>();
        FindBrowsePath(inputNode);
        if (pathToVar.Count > 0)
        {
            var Path = ConstructBrowsePath();
            Path += "/" + stationName + "/" + "Tags" + "/" + folderName + "/" + tagName;
            return Path;
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
    public void RemovTagMemberAuditSigning(IUANode tag)
    {
        foreach (var child in tag.Children)
        {
            if (child.BrowseName == "AuditSigning Signature")
            {
                tag.Remove(child);
            }
        }
    }
}
