#region Using directives

using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.Store;
using FTOptix.UI;
using FTOptix.EventLogger;
using FTOptix.SQLiteStore;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.AuditSigning;
using FTOptix.Alarm;
using FTOptix.SerialPort;

#endregion

public class SignWorkflowHandler : BaseNetLogic
{
    private Button _confirmButton;

    public override void Start()
    {
        _confirmButton = Owner.Get<Button>("Confirm");
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void CheckResult(SignResult signResult)
    {
        switch(signResult)
        {
            case SignResult.Succeeded:
                {
                    var auditDialog = (AuditDialogBox)LogicObject.GetAlias("AuditDialog");
                    auditDialog.Close();
                    return;
                }
            case SignResult.FirstUserLoginFailed:
                {
                    var wrongPasswordDialog = (DialogType)((IUAObject)LogicObject.Owner).ObjectType.Owner.Get("WorkflowFailDialog");
                    var firstUserName = InformationModel.Get(Owner.Get<ComboBox>("Username").SelectedItem).BrowseName;
                    wrongPasswordDialog.Get<Label>("User").LocalizedText = new LocalizedText(LogicObject.NodeId.NamespaceIndex, "SigningWorkflowFirstLoginFailed");
                    _confirmButton.OpenDialog(wrongPasswordDialog);
                    return;
                }
            case SignResult.FirstUserNotAuthorized:
                {
                    var wrongPasswordDialog = (DialogType)((IUAObject)LogicObject.Owner).ObjectType.Owner.Get("WorkflowFailDialog");
                    var firstUserName = InformationModel.Get(Owner.Get<ComboBox>("Username").SelectedItem).BrowseName;
                    wrongPasswordDialog.Get<Label>("User").LocalizedText = new LocalizedText(LogicObject.NodeId.NamespaceIndex, "SigningWorkflowFirstLoginUnauthorized");
                    _confirmButton.OpenDialog(wrongPasswordDialog);
                    return;
                }
        }
    }
}
