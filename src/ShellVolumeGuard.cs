using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DjiMicKey {
    // Does not set volume, simulate Volume Down, install drivers or persist settings.
    sealed class ShellVolumeGuard : NativeWindow, IDisposable {
        const string Caption="DJIMicKey Shell Volume Guard v1",Property="DJIMicKey.Shell.Enabled.v1";
        [DllImport("DJIMicKey.Shell.dll",SetLastError=true)] static extern bool ShellInstall();
        [DllImport("DJIMicKey.Shell.dll")] static extern void ShellRemove();
        [DllImport("DJIMicKey.Shell.dll")] static extern bool ShellTestDecision(IntPtr owner,int code,IntPtr data);
        [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool SetProp(IntPtr window,string name,IntPtr value);
        [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern IntPtr RemoveProp(IntPtr window,string name);
        readonly InputTrace trace;bool installed,enabled;public int Blocked {get;private set;}
        public ShellVolumeGuard(InputTrace trace,bool testOnly=false){this.trace=trace;try{CreateHandle(new CreateParams{Caption=testOnly?Caption+" test "+Guid.NewGuid():Caption});if(!testOnly){if(!ShellInstall())throw new Win32Exception();installed=true;}trace.Add("SHELL_FILTER installed="+installed+"; scope=global-unhandled-VolumeUp-while-RX-mapping-enabled; no-volume-setters");}catch{Dispose();throw;}}
        public void SetEnabled(bool value){if(Handle==IntPtr.Zero)return;if(value==enabled)return;if(value){if(!SetProp(Handle,Property,new IntPtr(1)))throw new Win32Exception();}else RemoveProp(Handle,Property);enabled=value;trace.Add("SHELL_FILTER enabled="+enabled);}
        public string Status {get{return "installed="+installed+" enabled="+enabled+" blocked="+Blocked+" coverage=unhandled-shell-command-only";}}
        public bool TestDecision(int code,int command){return ShellTestDecision(Handle,code,new IntPtr(command<<16));}
        protected override void WndProc(ref Message m){if(m.Msg==0x8000+42){Blocked++;trace.Add("SHELL_BLOCK command="+InputTrace.AppCommand(m.LParam)+" handlerProcessId="+m.WParam+" total="+Blocked+" source-device-not-available");m.Result=IntPtr.Zero;return;}base.WndProc(ref m);}
        public void Dispose(){if(Handle!=IntPtr.Zero)RemoveProp(Handle,Property);enabled=false;if(installed){ShellRemove();installed=false;}if(Handle!=IntPtr.Zero)DestroyHandle();}
    }
}
