#region Using directives
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.System;
using FTOptix.Alarm;
using FTOptix.SerialPort;
#endregion

public class ApplicationNameLogic : BaseNetLogic
{
    public override void Start()
    {
        Label Label = Owner as Label;
        Label.Text = Project.Current.BrowseName;
    }

    public override void Stop()
    {
    }
}
